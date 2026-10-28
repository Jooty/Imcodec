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

using Cocona;
using Imcodec.Proxy;

namespace Imcodec.Cli;

/// <summary>
/// Runs the proxy between a local client and the upstream servers.
/// </summary>
public sealed class ProxyCommands {

    private const int FAILURE_EXIT_CODE = 1;

    /// <summary>
    /// Relays a local port to the upstream login server until Ctrl+C.
    /// </summary>
    /// <param name="client">The client install directory, read for the server public keys.</param>
    /// <param name="upstream">The upstream login endpoint as host:port.</param>
    /// <param name="listen">The local port the client connects to, or 0 for any free one.</param>
    /// <param name="out">The capture file to write, with credentials redacted.</param>
    [PrimaryCommand]
    public async Task<int> Run(
        [Option("client", Description = "The client install directory.")] string client,
        [Option("upstream", Description = "The upstream login endpoint as host:port.")] string? upstream = null,
        [Option("listen", Description = "The local port the client connects to, 0 for any free one.")] int listen = 0,
        [Option("out", Description = "The capture file to write, with credentials redacted.")] string? @out = null) {
        if (!Directory.Exists(client)) {
            Console.Error.WriteLine($"The client directory '{client}' does not exist.");
            return FAILURE_EXIT_CODE;
        }
        if (!TryParseEndpoint(upstream, out var upstreamHost, out var upstreamPort)) {
            Console.Error.WriteLine("Give the upstream login endpoint as --upstream host:port.");
            return FAILURE_EXIT_CODE;
        }

        Keyring keyring;
        try {
            keyring = Keyring.EjectFromDirectory(client);
        }
        catch (InvalidDataException ex) {
            Console.Error.WriteLine(ex.Message);
            return FAILURE_EXIT_CODE;
        }

        using var stop = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) => {
            args.Cancel = true;
            stop.Cancel();
        };

        using var capture = @out is null ? null : new CaptureWriter(@out);
        await using var proxy = new ProxyServer(keyring, log: Console.Out, capture: capture);
        var port = proxy.StartLogin(upstreamHost, upstreamPort, listen);
        Console.WriteLine($"Listening on {ProxyServer.LOOPBACK_HOST}:{port}, relaying to {upstreamHost}:{upstreamPort}. Ctrl+C stops.");
        if (@out is not null) {
            Console.WriteLine($"Writing the capture to '{@out}'.");
        }

        try {
            await Task.Delay(Timeout.Infinite, stop.Token);
        }
        catch (OperationCanceledException) {
            Console.WriteLine("Stopping.");
        }
        return 0;
    }

    private static bool TryParseEndpoint(string? endpoint, out string host, out int port) {
        host = string.Empty;
        port = 0;
        var separator = endpoint?.LastIndexOf(':') ?? -1;
        if (separator <= 0) {
            return false;
        }

        host = endpoint![..separator];
        return int.TryParse(endpoint[(separator + 1)..], out port) && port is > 0 and <= ushort.MaxValue;
    }

}
