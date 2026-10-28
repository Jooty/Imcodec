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
using Cocona;
using Imcodec.Proxy;

namespace Imcodec.Cli;

/// <summary>
/// Runs the proxy between a local client and the upstream servers.
/// </summary>
public sealed class ProxyCommands {

    private const string DEFAULT_WINE_COMMAND = "wine";
    private const int FAILURE_EXIT_CODE = 1;

    /// <summary>
    /// Starts the patcher, catches the client it launches and relays it to the upstream servers.
    /// With no patcher, relays a local port to the given upstream until Ctrl+C.
    /// </summary>
    /// <param name="client">The client install directory, read for the server public keys.</param>
    /// <param name="patcher">The patcher executable to start and catch the client from.</param>
    /// <param name="wine">The command that runs the patcher on Linux.</param>
    /// <param name="upstream">The upstream login endpoint as host:port; with a patcher, overrides the caught one.</param>
    /// <param name="listen">The local port the client connects to, or 0 for any free one.</param>
    /// <param name="out">The capture file to write, with credentials redacted.</param>
    [PrimaryCommand]
    public async Task<int> Run(
        [Option("client", Description = "The client install directory.")] string client,
        [Option("patcher", Description = "The patcher executable to start and catch the client from.")] string? patcher = null,
        [Option("wine", Description = "The command that runs the patcher on Linux.")] string wine = DEFAULT_WINE_COMMAND,
        [Option("upstream", Description = "The upstream login endpoint as host:port.")] string? upstream = null,
        [Option("listen", Description = "The local port the client connects to, 0 for any free one.")] int listen = 0,
        [Option("out", Description = "The capture file to write, with credentials redacted.")] string? @out = null) {
        if (!Directory.Exists(client)) {
            Console.Error.WriteLine($"The client directory '{client}' does not exist.");
            return FAILURE_EXIT_CODE;
        }

        var upstreamGiven = upstream is not null;
        if (upstreamGiven && !TryParseEndpoint(upstream, out _, out _)) {
            Console.Error.WriteLine("Give the upstream login endpoint as --upstream host:port.");
            return FAILURE_EXIT_CODE;
        }
        if (patcher is null && !upstreamGiven) {
            Console.Error.WriteLine("Give --patcher <exe> to catch the client, or --upstream host:port to relay a port.");
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
        if (@out is not null) {
            Console.WriteLine($"Writing the capture to '{@out}'.");
        }

        return patcher is null
            ? await RelayManuallyAsync(proxy, upstream!, listen, stop.Token)
            : await CatchAndRelayAsync(proxy, client, patcher, wine, upstream, listen, stop.Token);
    }

    private async Task<int> RelayManuallyAsync(ProxyServer proxy, string upstream, int listen, CancellationToken cancellationToken) {
        _ = TryParseEndpoint(upstream, out var upstreamHost, out var upstreamPort);
        if (!TryStartLogin(proxy, upstreamHost, upstreamPort, listen, out var port)) {
            return FAILURE_EXIT_CODE;
        }

        Console.WriteLine($"Listening on {ProxyServer.LOOPBACK_HOST}:{port}, relaying to {upstreamHost}:{upstreamPort}. Point the client there. Ctrl+C stops.");
        await WaitForCancellationAsync(cancellationToken);
        return 0;
    }

    private async Task<int> CatchAndRelayAsync(ProxyServer proxy, string client, string patcher, string wine,
        string? upstream, int listen, CancellationToken cancellationToken) {
        var platform = ClientCatcher.PlatformFor(wine);
        if (platform is null) {
            Console.Error.WriteLine("Catching the client with a patcher is only supported on Windows and Linux.");
            return FAILURE_EXIT_CODE;
        }

        var catcher = new ClientCatcher(client, patcher, platform, Console.Out);
        if (catcher.PrerequisiteFailure() is { } failure) {
            Console.Error.WriteLine(failure);
            return FAILURE_EXIT_CODE;
        }

        CaughtClient caught;
        try {
            caught = await catcher.CatchAsync(cancellationToken);
        }
        catch (OperationCanceledException) {
            Console.WriteLine("Stopping.");
            return 0;
        }
        catch (InvalidDataException ex) {
            Console.Error.WriteLine(ex.Message);
            return FAILURE_EXIT_CODE;
        }

        var upstreamHost = caught.Endpoint.Host;
        var upstreamPort = caught.Endpoint.Port;
        if (upstream is not null) {
            _ = TryParseEndpoint(upstream, out upstreamHost, out upstreamPort);
        }
        if (!TryStartLogin(proxy, upstreamHost, upstreamPort, listen, out var port)) {
            return FAILURE_EXIT_CODE;
        }
        Console.WriteLine($"Listening on {ProxyServer.LOOPBACK_HOST}:{port}, relaying to {upstreamHost}:{upstreamPort}.");

        using var relaunched = catcher.Relaunch(caught, port);
        await WaitForClientExitAsync(relaunched, cancellationToken);
        return 0;
    }

    private static bool TryStartLogin(ProxyServer proxy, string upstreamHost, int upstreamPort, int listen, out int port) {
        port = 0;
        try {
            port = proxy.StartLogin(upstreamHost, upstreamPort, listen);
            return true;
        }
        catch (SocketException) {
            Console.Error.WriteLine($"The local port {listen} is already in use; pass a free --listen port or 0.");
            return false;
        }
    }

    private static async Task WaitForClientExitAsync(System.Diagnostics.Process client, CancellationToken cancellationToken) {
        try {
            await client.WaitForExitAsync(cancellationToken);
            Console.WriteLine("The client exited; stopping.");
        }
        catch (OperationCanceledException) {
            Console.WriteLine("Stopping.");
            TryKill(client);
        }
    }

    private static async Task WaitForCancellationAsync(CancellationToken cancellationToken) {
        try {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException) {
            Console.WriteLine("Stopping.");
        }
    }

    private static void TryKill(System.Diagnostics.Process client) {
        try {
            if (!client.HasExited) {
                client.Kill();
            }
        }
        catch (Exception) {
            // The client is already gone, which is the outcome we wanted.
        }
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
