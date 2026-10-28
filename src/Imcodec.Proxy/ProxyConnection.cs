/*
BSD 3-Clause License

Copyright (c) 2024, Jooty

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.
*/

using System.Net.Sockets;
using Imcodec.Cryptography;
using Imcodec.IO;
using Imcodec.MessageLayer;
using Imcodec.MessageLayer.Generated;
using static Imcodec.MessageLayer.ControlMessageProtocol;

namespace Imcodec.Proxy;

/// <summary>
/// One client relayed to one upstream server: the handshake on both sides, then a pump for each direction
/// until either side closes.
/// </summary>
internal sealed class ProxyConnection {

    private readonly ProxyServer _server;
    private readonly TcpClient _client;
    private readonly string _upstreamHost;
    private readonly int _upstreamPort;
    private readonly string _label;

    public ProxyConnection(ProxyServer server, TcpClient client, string upstreamHost, int upstreamPort, string label) {
        _server = server;
        _client = client;
        _upstreamHost = upstreamHost;
        _upstreamPort = upstreamPort;
        _label = label;
    }

    /// <summary>
    /// Relays the connection until either side closes or the proxy shuts down. Never throws.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken) {
        _server.Log?.WriteLine($"[{_label}] client connected");
        try {
            await RelayAsync(cancellationToken);
            _server.Log?.WriteLine($"[{_label}] closed");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            _server.Log?.WriteLine($"[{_label}] closed by shutdown");
        }
        catch (Exception ex) {
            _server.Log?.WriteLine($"[{_label}] failed: {ex.Message}");
        }
    }

    private async Task RelayAsync(CancellationToken cancellationToken) {
        using var client = _client;
        using var upstream = new TcpClient();
        using var crypto = new SessionCrypto(_server.Keyring, _server.ClientSignature);
        using var pumps = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        await upstream.ConnectAsync(_upstreamHost, _upstreamPort, cancellationToken);
        var clientStream = client.GetStream();
        var upstreamStream = upstream.GetStream();
        var clientReader = new FrameReader(clientStream);
        var upstreamReader = new FrameReader(upstreamStream);

        var offer = await ReadHandshakeAsync<SessionOffer>(upstreamReader, CaptureWriter.SERVER_SENDER, cancellationToken);
        crypto.VerifyOffer(offer);
        await upstreamStream.WriteAsync(MessageEncoder.Encode(crypto.BuildAccept(offer)), cancellationToken);
        if (crypto.IsEncrypted) {
            upstreamReader.StartDecrypting(crypto.ServerCipher!, AesStreamCipher.SERVER_CHUNK_BLOCKS);
        }

        var plainOffer = new SessionOffer {
            SessionId = offer.SessionId,
            TimestampUpper = offer.TimestampUpper,
            TimestampLower = offer.TimestampLower,
            Milliseconds = offer.Milliseconds,
        };
        await clientStream.WriteAsync(MessageEncoder.Encode(plainOffer), cancellationToken);
        await ReadHandshakeAsync<SessionAccept>(clientReader, CaptureWriter.CLIENT_SENDER, cancellationToken);

        var toUpstream = PumpAsync(clientReader, upstreamStream, crypto.ClientCipher, CaptureWriter.CLIENT_SENDER, pumps.Token);
        var toClient = PumpAsync(upstreamReader, clientStream, null, CaptureWriter.SERVER_SENDER, pumps.Token);
        var first = await Task.WhenAny(toUpstream, toClient);

        await pumps.CancelAsync();
        client.Close();
        upstream.Close();
        try {
            await Task.WhenAll(toUpstream, toClient);
        }
        catch (Exception) when (first.IsCompletedSuccessfully) {
            // One side closed cleanly; the other pump then fails on its closed socket.
        }
    }

    private async Task<T> ReadHandshakeAsync<T>(FrameReader reader, string sender, CancellationToken cancellationToken)
        where T : class, IMessage {
        var frame = await reader.ReadFrameAsync(cancellationToken)
            ?? throw new EndOfStreamException($"The connection closed before the {typeof(T).Name}.");
        var (messages, undecoded) = DecodeFrame(frame);
        _server.Capture?.Record(_label, sender, frame, messages, undecoded);
        if (messages is not [T message] || undecoded.Count > 0) {
            throw new InvalidDataException($"Expected a {typeof(T).Name} to open the session.");
        }
        return message;
    }

    private async Task PumpAsync(FrameReader source, Stream destination, AesStreamCipher? encryptor, string sender,
        CancellationToken cancellationToken) {
        while (await source.ReadFrameAsync(cancellationToken) is { } frame) {
            var (messages, undecoded) = DecodeFrame(frame);
            _server.Capture?.Record(_label, sender, frame, messages, undecoded);

            var outgoing = sender == CaptureWriter.SERVER_SENDER ? PointHandoffsAtProxy(frame, messages, undecoded) : frame;
            await destination.WriteAsync(encryptor?.Encrypt(outgoing) ?? outgoing, cancellationToken);
        }
    }

    private static (IReadOnlyList<IMessage> Messages, List<(byte ServiceId, byte MessageId)> Undecoded) DecodeFrame(byte[] frame) {
        var undecoded = new List<(byte ServiceId, byte MessageId)>();
        var messages = new MessageStreamDecoder((serviceId, messageId, _) => undecoded.Add((serviceId, messageId))).Feed(frame);
        if (messages.Count == 0 && undecoded.Count == 0) {
            // A frame the decoder rejects outright still gets an entry, so nothing relayed is missing from a capture.
            undecoded.Add((0, 0));
        }
        return (messages, undecoded);
    }

    private byte[] PointHandoffsAtProxy(byte[] frame, IReadOnlyList<IMessage> messages,
        IReadOnlyList<(byte ServiceId, byte MessageId)> undecoded) {
        if (!messages.Any(IsHandoff)) {
            return frame;
        }
        if (undecoded.Count > 0) {
            // Re-encoding would drop the messages that did not decode, so the handoff goes out untouched.
            _server.Log?.WriteLine($"[{_label}] a handoff shares its frame with a message that does not decode; not redirected");
            return frame;
        }

        foreach (var message in messages) {
            RedirectHandoff(message);
        }
        return messages.SelectMany(message => MessageEncoder.Encode(message)).ToArray();
    }

    private static bool IsHandoff(IMessage message)
        => message is LOGIN_7_PROTOCOL.MSG_CHARACTERSELECTED or GAME_5_PROTOCOL.MSG_SERVERTRANSFER;

    private void RedirectHandoff(IMessage message) {
        switch (message) {
            case LOGIN_7_PROTOCOL.MSG_CHARACTERSELECTED selected when !selected.IP.IsEmpty:
                selected.TCPPort = ProxyPortFor(selected.IP, selected.TCPPort);
                selected.IP = ProxyServer.LOOPBACK_HOST;
                break;
            case GAME_5_PROTOCOL.MSG_SERVERTRANSFER transfer:
                RedirectServerTransfer(transfer);
                break;
        }
    }

    private void RedirectServerTransfer(GAME_5_PROTOCOL.MSG_SERVERTRANSFER transfer) {
        if (!transfer.IP.IsEmpty) {
            transfer.TCPPort = ProxyPortFor(transfer.IP, transfer.TCPPort);
            transfer.IP = ProxyServer.LOOPBACK_HOST;
        }
        if (!transfer.FallbackIP.IsEmpty) {
            transfer.FallbackTCPPort = ProxyPortFor(transfer.FallbackIP, transfer.FallbackTCPPort);
            transfer.FallbackIP = ProxyServer.LOOPBACK_HOST;
        }
    }

    private int ProxyPortFor(ByteString upstreamHost, int upstreamPort) {
        var localPort = _server.ShardPort(upstreamHost, upstreamPort);
        _server.Log?.WriteLine($"[{_label}] handoff to {(string) upstreamHost}:{upstreamPort} redirected to {ProxyServer.LOOPBACK_HOST}:{localPort}");
        return localPort;
    }

}
