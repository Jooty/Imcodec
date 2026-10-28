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

namespace Imcodec.Proxy;

/// <summary>
/// Listens on local ports and relays every client that connects to the upstream server behind that port,
/// as an unencrypted session toward the client and an encrypted one toward the server.
/// </summary>
public sealed class ProxyServer : IAsyncDisposable {

    public const string LOGIN_CONNECTION_LABEL = "login";

    private readonly CancellationTokenSource _shutdown = new();
    private readonly List<TcpListener> _listeners = [];
    private readonly List<Task> _running = [];
    private readonly Lock _gate = new();

    internal Keyring Keyring { get; }
    internal ClientSignature? ClientSignature { get; }
    internal TextWriter? Log { get; }

    /// <summary>
    /// Creates a proxy with no listeners yet.
    /// </summary>
    /// <param name="keyring">The server public keys of the client install.</param>
    /// <param name="clientSignature">Answers the offer's tamper challenge when the upstream server sends one.</param>
    /// <param name="log">Receives one line for each connection that opens, closes or fails.</param>
    public ProxyServer(Keyring keyring, ClientSignature? clientSignature = null, TextWriter? log = null) {
        Keyring = keyring;
        ClientSignature = clientSignature;
        Log = log;
    }

    /// <summary>
    /// Starts relaying a local loopback port to the upstream login server.
    /// </summary>
    /// <param name="upstreamHost">The host of the upstream login server.</param>
    /// <param name="upstreamPort">The port of the upstream login server.</param>
    /// <param name="listenPort">The local port, or 0 for any free one.</param>
    /// <returns>The local port the client connects to.</returns>
    public int StartLogin(string upstreamHost, int upstreamPort, int listenPort = 0)
        => StartListener(upstreamHost, upstreamPort, listenPort, LOGIN_CONNECTION_LABEL);

    public async ValueTask DisposeAsync() {
        await _shutdown.CancelAsync();

        Task[] running;
        lock (_gate) {
            _listeners.ForEach(listener => listener.Stop());
            running = [.. _running];
        }

        try {
            await Task.WhenAll(running);
        }
        catch (Exception) {
            // Accept loops end by cancellation and connections have reported their own failures.
        }
        _shutdown.Dispose();
    }

    private int StartListener(string upstreamHost, int upstreamPort, int listenPort, string label) {
        var listener = new TcpListener(IPAddress.Loopback, listenPort);
        listener.Start();
        lock (_gate) {
            _listeners.Add(listener);
            _running.Add(AcceptClientsAsync(listener, upstreamHost, upstreamPort, label));
        }
        return ((IPEndPoint) listener.LocalEndpoint).Port;
    }

    private async Task AcceptClientsAsync(TcpListener listener, string upstreamHost, int upstreamPort, string label) {
        var cancellationToken = _shutdown.Token;
        while (!cancellationToken.IsCancellationRequested) {
            var client = await listener.AcceptTcpClientAsync(cancellationToken);
            var connection = new ProxyConnection(this, client, upstreamHost, upstreamPort, label);
            lock (_gate) {
                _running.Add(connection.RunAsync(cancellationToken));
            }
        }
    }

}
