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
using Imcodec.MessageLayer;
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

        var offer = await ReadHandshakeAsync<SessionOffer>(upstreamReader, cancellationToken);
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
        await ReadHandshakeAsync<SessionAccept>(clientReader, cancellationToken);

        var toUpstream = PumpAsync(clientReader, upstreamStream, crypto.ClientCipher, pumps.Token);
        var toClient = PumpAsync(upstreamReader, clientStream, null, pumps.Token);
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

    private static async Task<T> ReadHandshakeAsync<T>(FrameReader reader, CancellationToken cancellationToken)
        where T : class, IMessage {
        var frame = await reader.ReadFrameAsync(cancellationToken)
            ?? throw new EndOfStreamException($"The connection closed before the {typeof(T).Name}.");
        var messages = MessageEncoder.Decode(frame);
        if (messages is not { Count: 1 } || messages.First() is not T message) {
            throw new InvalidDataException($"Expected a {typeof(T).Name} to open the session.");
        }
        return message;
    }

    private static async Task PumpAsync(FrameReader source, Stream destination, AesStreamCipher? encryptor,
        CancellationToken cancellationToken) {
        while (await source.ReadFrameAsync(cancellationToken) is { } frame) {
            var outgoing = encryptor?.Encrypt(frame) ?? frame;
            await destination.WriteAsync(outgoing, cancellationToken);
        }
    }

}
