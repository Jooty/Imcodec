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
using System.Runtime.InteropServices;

namespace Imcodec.Proxy;

/// <summary>
/// The OS-specific steps of catching the client: listing processes, reading a process's command line,
/// freezing it, and launching the patcher or the client. One instance for the platform the proxy runs on.
/// </summary>
public interface IClientPlatform {

    /// <summary>
    /// Starts the patcher so the user can log in, inheriting the proxy's own environment.
    /// </summary>
    Process StartPatcher(string patcherExe);

    /// <summary>
    /// The ids of the processes running now.
    /// </summary>
    IReadOnlyList<int> ListProcessIds();

    /// <summary>
    /// The command line of a process, executable first, or null if it can no longer be read.
    /// </summary>
    IReadOnlyList<string>? TryReadCommandLine(int processId);

    /// <summary>
    /// Suspends every thread of a process so it cannot reach the network before it is killed.
    /// </summary>
    void Freeze(int processId);

    /// <summary>
    /// The working directory to relaunch the client in, read from the frozen process where the OS allows it.
    /// </summary>
    string WorkingDirectoryFor(int processId, string clientExe);

    /// <summary>
    /// Starts the client the same way the patcher did, with the given arguments and working directory.
    /// </summary>
    Process StartClient(string clientExe, IReadOnlyList<string> arguments, string workingDirectory);

    /// <summary>
    /// The one-line reason this platform cannot run the patcher, or null when it can.
    /// </summary>
    string? PrerequisiteFailure();

}

internal sealed class LinuxClientPlatform : IClientPlatform {

    private const string PROC = "/proc";
    private const int SIGSTOP = 19;

    private readonly string _wineCommand;

    public LinuxClientPlatform(string wineCommand) {
        _wineCommand = wineCommand;
    }

    public string? PrerequisiteFailure()
        => ResolveWine() is null
            ? $"Wine is required on Linux to run the patcher; install it or pass --wine <path> (looked for '{_wineCommand}')."
            : null;

    public Process StartPatcher(string patcherExe)
        => StartClient(patcherExe, [], Path.GetDirectoryName(Path.GetFullPath(patcherExe)) ?? ".");

    public IReadOnlyList<int> ListProcessIds() {
        var ids = new List<int>();
        foreach (var directory in Directory.EnumerateDirectories(PROC)) {
            if (int.TryParse(Path.GetFileName(directory), out var pid)) {
                ids.Add(pid);
            }
        }
        return ids;
    }

    public IReadOnlyList<string>? TryReadCommandLine(int processId) {
        try {
            var raw = File.ReadAllBytes(Path.Combine(PROC, processId.ToString(), "cmdline"));
            return SplitNulls(raw);
        }
        catch (Exception) {
            return null;
        }
    }

    public void Freeze(int processId) {
        if (kill(processId, SIGSTOP) != 0) {
            throw new InvalidOperationException($"Could not freeze process {processId}.");
        }
    }

    public string WorkingDirectoryFor(int processId, string clientExe) {
        try {
            var link = Directory.ResolveLinkTarget(Path.Combine(PROC, processId.ToString(), "cwd"), returnFinalTarget: true);
            if (link is not null) {
                return link.FullName;
            }
        }
        catch (Exception) {
            // The frozen process exposed no readable cwd, so fall back to the client executable's directory.
        }
        return Path.GetDirectoryName(Path.GetFullPath(clientExe)) ?? ".";
    }

    public Process StartClient(string clientExe, IReadOnlyList<string> arguments, string workingDirectory) {
        var start = new ProcessStartInfo { FileName = ResolveWine() ?? _wineCommand, WorkingDirectory = workingDirectory, UseShellExecute = false };
        start.ArgumentList.Add(clientExe);
        foreach (var argument in arguments) {
            start.ArgumentList.Add(argument);
        }
        return Process.Start(start) ?? throw new InvalidOperationException("The client process could not be started.");
    }

    private string? ResolveWine() {
        if (File.Exists(_wineCommand)) {
            return _wineCommand;
        }
        var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? [];
        return paths.Select(directory => Path.Combine(directory, _wineCommand)).FirstOrDefault(File.Exists);
    }

    private static string[] SplitNulls(byte[] raw) {
        var text = System.Text.Encoding.UTF8.GetString(raw);
        return text.Split('\0', StringSplitOptions.RemoveEmptyEntries);
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int kill(int pid, int sig);

}

internal sealed class WindowsClientPlatform : IClientPlatform {

    private const uint PROCESS_QUERY_INFORMATION = 0x0400;
    private const uint PROCESS_VM_READ = 0x0010;
    private const uint PROCESS_SUSPEND_RESUME = 0x0800;
    private const int PROCESS_COMMAND_LINE_INFORMATION = 60;

    public string? PrerequisiteFailure() => null;

    public Process StartPatcher(string patcherExe)
        => StartClient(patcherExe, [], Path.GetDirectoryName(Path.GetFullPath(patcherExe)) ?? ".");

    public IReadOnlyList<int> ListProcessIds()
        => Process.GetProcesses().Select(process => process.Id).ToArray();

    public IReadOnlyList<string>? TryReadCommandLine(int processId) {
        var handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, (uint) processId);
        if (handle == IntPtr.Zero) {
            return null;
        }

        try {
            return ReadCommandLine(handle);
        }
        finally {
            CloseHandle(handle);
        }
    }

    public void Freeze(int processId) {
        var handle = OpenProcess(PROCESS_SUSPEND_RESUME, false, (uint) processId);
        if (handle == IntPtr.Zero) {
            throw new InvalidOperationException($"Could not open process {processId} to freeze it.");
        }

        try {
            if (NtSuspendProcess(handle) != 0) {
                throw new InvalidOperationException($"Could not freeze process {processId}.");
            }
        }
        finally {
            CloseHandle(handle);
        }
    }

    public string WorkingDirectoryFor(int processId, string clientExe)
        => Path.GetDirectoryName(Path.GetFullPath(clientExe)) ?? ".";

    public Process StartClient(string clientExe, IReadOnlyList<string> arguments, string workingDirectory) {
        var start = new ProcessStartInfo { FileName = clientExe, WorkingDirectory = workingDirectory, UseShellExecute = false };
        foreach (var argument in arguments) {
            start.ArgumentList.Add(argument);
        }
        return Process.Start(start) ?? throw new InvalidOperationException("The client process could not be started.");
    }

    private static IReadOnlyList<string>? ReadCommandLine(IntPtr handle) {
        var needed = 0;
        _ = NtQueryInformationProcess(handle, PROCESS_COMMAND_LINE_INFORMATION, IntPtr.Zero, 0, ref needed);
        if (needed <= 0) {
            return null;
        }

        var buffer = Marshal.AllocHGlobal(needed);
        try {
            if (NtQueryInformationProcess(handle, PROCESS_COMMAND_LINE_INFORMATION, buffer, needed, ref needed) != 0) {
                return null;
            }
            var commandLine = Marshal.PtrToStringUni(Marshal.ReadIntPtr(buffer, IntPtr.Size));
            return commandLine is null ? null : SplitCommandLine(commandLine);
        }
        finally {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static string[] SplitCommandLine(string commandLine) {
        var argv = CommandLineToArgvW(commandLine, out var count);
        if (argv == IntPtr.Zero) {
            return [];
        }

        try {
            var arguments = new string[count];
            for (var i = 0; i < count; i++) {
                arguments[i] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, i * IntPtr.Size)) ?? string.Empty;
            }
            return arguments;
        }
        finally {
            LocalFree(argv);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint access, bool inheritHandle, uint processId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("ntdll.dll")]
    private static extern int NtSuspendProcess(IntPtr handle);

    [DllImport("ntdll.dll")]
    private static extern int NtQueryInformationProcess(IntPtr handle, int infoClass, IntPtr buffer, int length, ref int returnLength);

    [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CommandLineToArgvW(string commandLine, out int count);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr handle);

}
