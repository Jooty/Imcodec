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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Imcodec.Proxy;

/// <summary>
/// The command line, working directory and login endpoint read from the frozen client, ready to relaunch.
/// </summary>
public sealed record CaughtClient(string ClientExe, IReadOnlyList<string> Arguments, string WorkingDirectory, ClientLoginEndpoint Endpoint);

/// <summary>
/// Starts the official patcher from the client install, waits for it to launch the client, freezes the client before it reaches the
/// network to read where it was told to connect, then relaunches it pointed at the proxy.
/// </summary>
/// <remarks>
/// The proxy never sees the account password; it only reads the client's command line, whose login token
/// is passed through unchanged and never logged.
/// </remarks>
public sealed class ClientCatcher {

    public const string BIN_DIRECTORY_NAME = "Bin";
    public const string EXECUTABLE_SEARCH_PATTERN = "*.exe";

    private const int DETECTION_POLL_MILLISECONDS = 20;

    private readonly string _clientDirectory;
    private readonly IClientPlatform _platform;
    private readonly TextWriter? _log;

    public ClientCatcher(string clientDirectory, IClientPlatform platform, TextWriter? log = null) {
        _clientDirectory = clientDirectory;
        _platform = platform;
        _log = log;
    }

    /// <summary>
    /// The platform support for the host the proxy runs on, or null on an unsupported OS.
    /// </summary>
    /// <param name="wineCommand">The command that runs the patcher on Linux.</param>
    public static IClientPlatform? PlatformFor(string wineCommand) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            return new LinuxClientPlatform(wineCommand);
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return new WindowsClientPlatform();
        }
        return null;
    }

    /// <summary>
    /// The one-line reason the catcher cannot run, naming what to fix, or null when everything is in place.
    /// </summary>
    public string? PrerequisiteFailure() {
        if (!Directory.Exists(_clientDirectory)) {
            return $"The client directory '{_clientDirectory}' does not exist.";
        }
        if (ClientExecutableNames().Count == 0) {
            return $"No '{EXECUTABLE_SEARCH_PATTERN}' found in the client's '{BIN_DIRECTORY_NAME}' directory under '{_clientDirectory}'.";
        }
        if (!TryFindEntryExecutable(_clientDirectory, out _, out var entryFailure)) {
            return entryFailure;
        }
        return _platform.PrerequisiteFailure();
    }

    /// <summary>
    /// Finds the single executable in the install root, the entry stub a shortcut runs, which owns the
    /// patcher's bank selection and self-update.
    /// </summary>
    public static bool TryFindEntryExecutable(string clientDirectory, [NotNullWhen(true)] out string? entryExe,
        [NotNullWhen(false)] out string? failure) {
        var found = Directory.Exists(clientDirectory)
            ? Directory.EnumerateFiles(clientDirectory, EXECUTABLE_SEARCH_PATTERN).Order().ToArray()
            : [];
        entryExe = found.Length == 1 ? Path.GetFullPath(found[0]) : null;
        failure = found.Length switch {
            1 => null,
            0 => $"Expected exactly one '{EXECUTABLE_SEARCH_PATTERN}' in the install root '{clientDirectory}', found none.",
            _ => $"Expected exactly one '{EXECUTABLE_SEARCH_PATTERN}' in the install root '{clientDirectory}', found {found.Length}: {string.Join(", ", found.Select(Path.GetFileName))}."
        };
        return entryExe is not null;
    }

    /// <summary>
    /// Whether a command line, executable first, is the game client: it names one of the client's
    /// executables and carries the login endpoint. A process without the endpoint, such as the bug
    /// reporter, is not the client.
    /// </summary>
    public static bool IsClientCommandLine(IReadOnlyList<string> commandLine, IReadOnlyCollection<string> executableNames)
        => NamesClient(commandLine, executableNames) && ClientCommandLine.TryFindLoginEndpoint(commandLine) is not null;

    /// <summary>
    /// Starts the patcher and returns once the client it launches has been frozen, read and killed.
    /// </summary>
    /// <exception cref="InvalidDataException">The install root has no single entry executable.</exception>
    public async Task<CaughtClient> CatchAsync(CancellationToken cancellationToken) {
        var executableNames = ClientExecutableNames();
        if (!TryFindEntryExecutable(_clientDirectory, out var entryExe, out var failure)) {
            throw new InvalidDataException(failure);
        }

        using var patcher = _platform.StartPatcher(entryExe);
        _log?.WriteLine("Patcher started; log in there as usual.");

        var caught = await DetectAndFreezeAsync(executableNames, patcher.Id, cancellationToken);
        _log?.WriteLine($"Caught the client: {caught.ClientExe} {ClientCommandLine.Redacted(caught.Arguments)}");
        return caught;
    }

    /// <summary>
    /// Relaunches the caught client with its login endpoint pointed at the proxy, and returns the process.
    /// </summary>
    public Process Relaunch(CaughtClient caught, int listenPort) {
        var arguments = ClientCommandLine.WithLoginEndpoint(caught.Arguments, ProxyServer.LOOPBACK_HOST, listenPort);
        _log?.WriteLine($"Relaunching the client: {caught.ClientExe} {ClientCommandLine.Redacted(arguments)}");
        return _platform.StartClient(caught.ClientExe, arguments, caught.WorkingDirectory);
    }

    private async Task<CaughtClient> DetectAndFreezeAsync(IReadOnlyCollection<string> executableNames, int patcherPid,
        CancellationToken cancellationToken) {
        var known = _platform.ListProcessIds().ToHashSet();
        known.Add(patcherPid);
        while (true) {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var pid in _platform.ListProcessIds()) {
                if (!known.Add(pid)) {
                    continue;
                }
                if (TryCatch(pid, executableNames, out var caught)) {
                    return caught;
                }
            }
            await Task.Delay(DETECTION_POLL_MILLISECONDS, cancellationToken);
        }
    }

    private bool TryCatch(int pid, IReadOnlyCollection<string> executableNames, out CaughtClient caught) {
        caught = null!;
        var commandLine = _platform.TryReadCommandLine(pid);
        if (commandLine is null || commandLine.Count == 0 || !IsClientCommandLine(commandLine, executableNames)) {
            return false;
        }

        // Freeze before anything else so the client cannot reach the network, then read and kill it.
        _platform.Freeze(pid);
        var clientExe = commandLine[0];
        var arguments = commandLine.Skip(1).ToArray();
        var workingDirectory = _platform.WorkingDirectoryFor(pid, clientExe);
        var endpoint = ClientCommandLine.FindLoginEndpoint(arguments);
        KillFrozen(pid);

        caught = new CaughtClient(clientExe, arguments, workingDirectory, endpoint);
        return true;
    }

    private IReadOnlyCollection<string> ClientExecutableNames() {
        var binDirectory = Path.Combine(_clientDirectory, BIN_DIRECTORY_NAME);
        if (!Directory.Exists(binDirectory)) {
            return [];
        }
        return Directory.EnumerateFiles(binDirectory, EXECUTABLE_SEARCH_PATTERN)
            .Select(Path.GetFileName)
            .Where(name => name is not null)
            .Select(name => name!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool NamesClient(IReadOnlyList<string> commandLine, IReadOnlyCollection<string> executableNames)
        => commandLine.Any(token => executableNames.Contains(Path.GetFileName(token.Replace('\\', '/'))));

    private void KillFrozen(int pid) {
        try {
            using var process = Process.GetProcessById(pid);
            process.Kill();
        }
        catch (Exception ex) {
            _log?.WriteLine($"Could not kill the frozen client {pid}: {ex.Message}");
        }
    }

}
