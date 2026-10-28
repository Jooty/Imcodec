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
using System.Text.Json;
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
    private const string TEST_PASS_KEY = "not-a-real-pass-key";

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

    [Fact]
    public async Task Handoff_IsRedirectedToALocalPortThatRelaysToTheNamedServer() {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(TEST_TIMEOUT_SECONDS));
        var cancellationToken = timeout.Token;
        var loginListener = StartUpstream(out var loginPort);
        var shardListener = StartUpstream(out var shardPort);
        try {
            await using var proxy = new ProxyServer(SyntheticKeyring.Shared.Keyring);
            var localLoginPort = proxy.StartLogin(LOOPBACK_HOST, loginPort);
            using var login = await RelayedSession.OpenAsync(loginListener, localLoginPort, cancellationToken);

            var handoff = CharacterSelected(shardPort);
            await login.SendFromUpstreamAsync([MessageEncoder.Encode(handoff), MessageEncoder.Encode(handoff)], cancellationToken);
            var first = DecodeSingle<LOGIN_7_PROTOCOL.MSG_CHARACTERSELECTED>(await login.ClientReader.ReadFrameAsync(cancellationToken));
            var repeat = DecodeSingle<LOGIN_7_PROTOCOL.MSG_CHARACTERSELECTED>(await login.ClientReader.ReadFrameAsync(cancellationToken));

            Assert.Equal(ProxyServer.LOOPBACK_HOST, (string) first.IP);
            Assert.NotEqual(shardPort, first.TCPPort);
            Assert.Equal(first.TCPPort, repeat.TCPPort);
            Assert.Equal(handoff.CharID, first.CharID);
            Assert.Equal((string) handoff.ZoneName, (string) first.ZoneName);

            using var shard = await RelayedSession.OpenAsync(shardListener, first.TCPPort, cancellationToken);
            var hello = MessageEncoder.Encode(CharacterInfo(CLIENT_PAYLOAD_LENGTH, seed: 4));
            await shard.SendFromClientAsync([hello], cancellationToken);
            Assert.Equal(hello, await shard.UpstreamReader.ReadFrameAsync(cancellationToken));
        }
        finally {
            loginListener.Stop();
            shardListener.Stop();
        }
    }

    [Fact]
    public async Task Capture_RecordsBothDirections_AndRedactsCredentials() {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(TEST_TIMEOUT_SECONDS));
        var cancellationToken = timeout.Token;
        var capturePath = Path.Combine(Path.GetTempPath(), $"imcodec-proxy-{Guid.NewGuid():N}.json");
        var upstreamListener = StartUpstream(out var upstreamPort);
        try {
            using (var capture = new CaptureWriter(capturePath)) {
                await using var proxy = new ProxyServer(SyntheticKeyring.Shared.Keyring, capture: capture);
                var localPort = proxy.StartLogin(LOOPBACK_HOST, upstreamPort);
                using var session = await RelayedSession.OpenAsync(upstreamListener, localPort, cancellationToken);

                var validate = MessageEncoder.Encode(new LOGIN_7_PROTOCOL.MSG_USER_VALIDATE {
                    UserID = 5, PassKey3 = TEST_PASS_KEY, MachineID = 6, Locale = "en", PatchClientID = "p", PlatformChatID = "c",
                });
                await session.SendFromClientAsync([validate], cancellationToken);
                Assert.Equal(validate, await session.UpstreamReader.ReadFrameAsync(cancellationToken));

                var info = MessageEncoder.Encode(CharacterInfo(CLIENT_PAYLOAD_LENGTH, seed: 5));
                await session.SendFromUpstreamAsync([info], cancellationToken);
                Assert.Equal(info, await session.ClientReader.ReadFrameAsync(cancellationToken));
            }

            var text = await File.ReadAllTextAsync(capturePath, cancellationToken);
            Assert.DoesNotContain(TEST_PASS_KEY, text);

            using var document = JsonDocument.Parse(text);
            var entries = document.RootElement.EnumerateArray().ToArray();
            Assert.Contains(entries, entry => entry.GetProperty("sender").GetString() == CaptureWriter.CLIENT_SENDER);
            Assert.Contains(entries, entry => entry.GetProperty("sender").GetString() == CaptureWriter.SERVER_SENDER);
            Assert.All(entries, entry => Assert.Equal(ProxyServer.LOGIN_CONNECTION_LABEL, entry.GetProperty("connection").GetString()));

            var validateEntry = Assert.Single(entries, entry => NameOf(entry) == nameof(LOGIN_7_PROTOCOL.MSG_USER_VALIDATE));
            var fields = validateEntry.GetProperty("data").GetProperty("fields");
            Assert.Equal(CaptureWriter.REDACTED_MARKER, fields.GetProperty("PassKey3").GetProperty("value").GetString());
            Assert.Equal(5UL, fields.GetProperty("UserID").GetProperty("value").GetUInt64());
            Assert.Equal(CaptureWriter.REDACTED_MARKER, validateEntry.GetProperty("raw").GetString());

            var offerEntry = Assert.Single(entries, entry => NameOf(entry) == nameof(SessionOffer));
            Assert.Equal(CaptureWriter.REDACTED_MARKER,
                offerEntry.GetProperty("data").GetProperty("fields").GetProperty("CryptoPayload").GetProperty("value").GetString());
            Assert.Single(entries, entry => NameOf(entry) == nameof(LOGIN_7_PROTOCOL.MSG_CHARACTERINFO));
        }
        finally {
            upstreamListener.Stop();
            File.Delete(capturePath);
        }
    }

    internal static string? NameOf(JsonElement entry)
        => entry.GetProperty("data") is { ValueKind: JsonValueKind.Object } data ? data.GetProperty("name").GetString() : null;

    internal static LOGIN_7_PROTOCOL.MSG_CHARACTERSELECTED CharacterSelected(int shardPort) => new() {
        IP = LOOPBACK_HOST,
        TCPPort = shardPort,
        UDPPort = shardPort,
        Key = "handoff-key",
        UserID = 11,
        CharID = 22,
        ZoneID = 33,
        ZoneName = "TestZone",
        Location = "Start",
        Slot = 1,
        LoginServer = "test-login",
    };

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
