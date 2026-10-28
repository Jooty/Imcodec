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

using System.Net;
using System.Net.Sockets;
using Imcodec.Cryptography;
using Imcodec.MessageLayer;
using Imcodec.MessageLayer.Generated;
using Imcodec.Proxy;
using static Imcodec.MessageLayer.ControlMessageProtocol;

namespace Imcodec.Test.ProxyTest;

public class ProxyRelayTest {

    private const string LOOPBACK_HOST = "127.0.0.1";
    private const int TEST_TIMEOUT_SECONDS = 20;
    private const int CLIENT_PAYLOAD_LENGTH = 20_000;
    private const int UPSTREAM_PAYLOAD_LENGTH = 40_000;

    [Fact]
    public async Task Relay_ForwardsFramesByteExactBothWays_AndClosesWithTheClient() {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(TEST_TIMEOUT_SECONDS));
        var cancellationToken = timeout.Token;
        var upstreamListener = StartUpstream(out var upstreamPort);
        try {
            await using var proxy = new ProxyServer(SyntheticKeyring.Shared.Keyring);
            var localPort = proxy.StartLogin(LOOPBACK_HOST, upstreamPort);
            using var session = await RelayedSession.OpenAsync(upstreamListener, localPort, cancellationToken);

            byte[][] fromClient = [
                MessageEncoder.Encode(new KeepAlive { SessionId = session.SessionId, Milliseconds = 12, ElapsedSessionTime = 3 }),
                MessageEncoder.Encode(CharacterInfo(CLIENT_PAYLOAD_LENGTH, seed: 1)),
            ];
            byte[][] fromUpstream = [
                MessageEncoder.Encode(CharacterInfo(UPSTREAM_PAYLOAD_LENGTH, seed: 2)),
                MessageEncoder.Encode(new KeepAliveResponse { SessionId = session.SessionId, Milliseconds = 12, ElapsedSessionTime = 3 }),
                MessageEncoder.Encode(CharacterInfo(UPSTREAM_PAYLOAD_LENGTH, seed: 3)),
            ];

            await session.SendFromClientAsync(fromClient, cancellationToken);
            foreach (var expected in fromClient) {
                Assert.Equal(expected, await session.UpstreamReader.ReadFrameAsync(cancellationToken));
            }

            await session.SendFromUpstreamAsync(fromUpstream, cancellationToken);
            foreach (var expected in fromUpstream) {
                Assert.Equal(expected, await session.ClientReader.ReadFrameAsync(cancellationToken));
            }

            session.Client.Close();
            Assert.Null(await ReadAfterCloseAsync(session.UpstreamReader, cancellationToken));
        }
        finally {
            upstreamListener.Stop();
        }
    }

    internal static TcpListener StartUpstream(out int port) {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        port = ((IPEndPoint) listener.LocalEndpoint).Port;
        return listener;
    }

    internal static LOGIN_7_PROTOCOL.MSG_CHARACTERINFO CharacterInfo(int length, int seed)
        => new() { CharacterInfo = Enumerable.Range(seed, length).Select(i => (byte) i).ToArray() };

    internal static async Task<byte[]?> ReadAfterCloseAsync(FrameReader reader, CancellationToken cancellationToken) {
        try {
            return await reader.ReadFrameAsync(cancellationToken);
        }
        catch (IOException) {
            return null;
        }
    }

    internal static T DecodeSingle<T>(byte[]? frame) where T : class, IMessage {
        Assert.NotNull(frame);
        return Assert.IsType<T>(Assert.Single(MessageEncoder.Decode(frame)!));
    }

    /// <summary>
    /// A fake plaintext client and a fake encrypting upstream server joined through the proxy.
    /// </summary>
    internal sealed class RelayedSession : IDisposable {

        public required TcpClient Client { get; init; }
        public required FrameReader ClientReader { get; init; }
        public required TcpClient Upstream { get; init; }
        public required FrameReader UpstreamReader { get; init; }
        public required SessionCrypto ServerCrypto { get; init; }
        public required ushort SessionId { get; init; }

        /// <summary>
        /// Connects a client to the proxy, accepts the proxy on the fake upstream and runs both handshakes.
        /// </summary>
        public static async Task<RelayedSession> OpenAsync(TcpListener upstreamListener, int localPort,
            CancellationToken cancellationToken) {
            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, localPort, cancellationToken);
            var upstream = await upstreamListener.AcceptTcpClientAsync(cancellationToken);
            var upstreamReader = new FrameReader(upstream.GetStream());
            var clientReader = new FrameReader(client.GetStream());

            var offer = SessionCryptoTest.MakeSignedOffer();
            await upstream.GetStream().WriteAsync(MessageEncoder.Encode(offer), cancellationToken);
            var serverCrypto = new SessionCrypto(SyntheticKeyring.Shared.Keyring);
            serverCrypto.VerifyOffer(offer);
            var accept = DecodeSingle<SessionAccept>(await upstreamReader.ReadFrameAsync(cancellationToken));
            serverCrypto.ProcessAccept(accept, SyntheticKeyring.Shared.Servers[serverCrypto.Offer!.KeySlot]);
            upstreamReader.StartDecrypting(serverCrypto.ClientCipher!, AesStreamCipher.CLIENT_CHUNK_BLOCKS);

            var plainOffer = DecodeSingle<SessionOffer>(await clientReader.ReadFrameAsync(cancellationToken));
            Assert.False(SessionCryptoBlocks.HasCrypto(plainOffer.CryptoPayload));
            Assert.Equal(offer.SessionId, plainOffer.SessionId);
            var clientAccept = new SessionAccept { SessionId = plainOffer.SessionId };
            await client.GetStream().WriteAsync(MessageEncoder.Encode(clientAccept), cancellationToken);

            return new RelayedSession {
                Client = client,
                ClientReader = clientReader,
                Upstream = upstream,
                UpstreamReader = upstreamReader,
                ServerCrypto = serverCrypto,
                SessionId = offer.SessionId,
            };
        }

        /// <summary>
        /// Writes plaintext frames from the client in one write.
        /// </summary>
        public async Task SendFromClientAsync(byte[][] frames, CancellationToken cancellationToken)
            => await Client.GetStream().WriteAsync(frames.SelectMany(frame => frame).ToArray(), cancellationToken);

        /// <summary>
        /// Encrypts frames as the upstream server and writes them in one write.
        /// </summary>
        public async Task SendFromUpstreamAsync(byte[][] frames, CancellationToken cancellationToken) {
            var encrypted = ServerCrypto.ServerCipher!.Encrypt(frames.SelectMany(frame => frame).ToArray());
            await Upstream.GetStream().WriteAsync(encrypted, cancellationToken);
        }

        public void Dispose() {
            Client.Dispose();
            Upstream.Dispose();
            ServerCrypto.Dispose();
        }

    }

}
