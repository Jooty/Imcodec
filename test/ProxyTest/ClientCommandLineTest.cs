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

using Imcodec.Proxy;

namespace Imcodec.Test.ProxyTest;

public class ClientCommandLineTest {

    private const string LOGIN_HOST = "login.example.test";
    private const int LOGIN_PORT = 12000;
    private const string PROXY_HOST = "127.0.0.1";
    private const int PROXY_PORT = 51000;
    private const string LOGIN_TOKEN = "secret-login-token";
    private const string CLIENT_EXE_NAME = "client.exe";
    private const string REPORTER_EXE_NAME = "reporter.exe";
    private const string CLIENT_EXE_PATH = "C:\\game\\Bin\\client.exe";
    private const string REPORTER_EXE_PATH = "C:\\game\\Bin\\reporter.exe";

    private static string[] CommandLine(params string[] before)
        => [.. before, "-L", LOGIN_HOST, LOGIN_PORT.ToString(), "-A", LOGIN_TOKEN];

    [Fact]
    public void FindLoginEndpoint_ReadsHostAndPortWhereverTheFlagSits() {
        var endpoint = ClientCommandLine.FindLoginEndpoint(CommandLine("-W", "wide"));

        Assert.Equal(LOGIN_HOST, endpoint.Host);
        Assert.Equal(LOGIN_PORT, endpoint.Port);
        Assert.Equal(2, endpoint.FlagIndex);
    }

    [Fact]
    public void FindLoginEndpoint_NoFlag_Throws()
        => Assert.Throws<InvalidDataException>(() => ClientCommandLine.FindLoginEndpoint(["-A", LOGIN_TOKEN]));

    [Fact]
    public void FindLoginEndpoint_FlagWithoutPort_Throws()
        => Assert.Throws<InvalidDataException>(() => ClientCommandLine.FindLoginEndpoint(["-L", LOGIN_HOST]));

    [Fact]
    public void FindLoginEndpoint_NonNumericPort_Throws()
        => Assert.Throws<InvalidDataException>(() => ClientCommandLine.FindLoginEndpoint(["-L", LOGIN_HOST, "notaport"]));

    [Fact]
    public void WithLoginEndpoint_SwapsOnlyHostAndPort_LeavesEveryOtherArgumentIntact() {
        var original = CommandLine("-W", "wide");

        var rewritten = ClientCommandLine.WithLoginEndpoint(original, PROXY_HOST, PROXY_PORT);

        Assert.Equal(["-W", "wide", "-L", PROXY_HOST, PROXY_PORT.ToString(), "-A", LOGIN_TOKEN], rewritten);
        Assert.Equal(CommandLine("-W", "wide"), original);
    }

    [Fact]
    public void Redacted_ShowsOnlyTheLoginEndpoint_HidesTheToken() {
        var redacted = ClientCommandLine.Redacted(CommandLine("-W", "wide"));

        Assert.Contains($"-L {LOGIN_HOST} {LOGIN_PORT}", redacted);
        Assert.DoesNotContain(LOGIN_TOKEN, redacted);
        Assert.Equal($"{ClientCommandLine.REDACTED_ARGUMENT} {ClientCommandLine.REDACTED_ARGUMENT} -L {LOGIN_HOST} {LOGIN_PORT} {ClientCommandLine.REDACTED_ARGUMENT} {ClientCommandLine.REDACTED_ARGUMENT}", redacted);
    }

    [Fact]
    public void Redacted_NoLoginEndpoint_HidesEveryArgument() {
        var redacted = ClientCommandLine.Redacted(["-A", LOGIN_TOKEN]);

        Assert.DoesNotContain(LOGIN_TOKEN, redacted);
        Assert.Equal($"{ClientCommandLine.REDACTED_ARGUMENT} {ClientCommandLine.REDACTED_ARGUMENT}", redacted);
    }

    [Fact]
    public void IsClientCommandLine_BinExecutableWithLoginFlag_IsTheClient()
        => Assert.True(ClientCatcher.IsClientCommandLine([CLIENT_EXE_PATH, .. CommandLine()], [CLIENT_EXE_NAME]));

    [Fact]
    public void IsClientCommandLine_BinExecutableWithoutLoginFlag_IsIgnored()
        => Assert.False(ClientCatcher.IsClientCommandLine([REPORTER_EXE_PATH, "-A", LOGIN_TOKEN], [CLIENT_EXE_NAME, REPORTER_EXE_NAME]));

    [Fact]
    public void IsClientCommandLine_LoginFlagWithoutBinExecutable_IsIgnored()
        => Assert.False(ClientCatcher.IsClientCommandLine(["other.exe", .. CommandLine()], [CLIENT_EXE_NAME]));

    [Fact]
    public void TryFindEntryExecutable_OneExecutable_ReturnsItsFullPath() {
        using var install = new TempInstall("entry.exe");

        Assert.True(ClientCatcher.TryFindEntryExecutable(install.Root, out var entryExe, out var failure));
        Assert.Equal(Path.GetFullPath(Path.Combine(install.Root, "entry.exe")), entryExe);
        Assert.Null(failure);
    }

    [Fact]
    public void TryFindEntryExecutable_NoExecutable_NamesWhatWasFound() {
        using var install = new TempInstall("readme.txt");

        Assert.False(ClientCatcher.TryFindEntryExecutable(install.Root, out var entryExe, out var failure));
        Assert.Null(entryExe);
        Assert.Contains("found none", failure);
    }

    [Fact]
    public void TryFindEntryExecutable_SeveralExecutables_NamesEachOne() {
        using var install = new TempInstall("a.exe", "b.exe");

        Assert.False(ClientCatcher.TryFindEntryExecutable(install.Root, out var entryExe, out var failure));
        Assert.Null(entryExe);
        Assert.Contains("found 2: a.exe, b.exe", failure);
    }

    [Fact]
    public void TryFindEntryExecutable_IgnoresExecutablesInBin() {
        using var install = new TempInstall("entry.exe", Path.Combine("Bin", CLIENT_EXE_NAME));

        Assert.True(ClientCatcher.TryFindEntryExecutable(install.Root, out var entryExe, out _));
        Assert.Equal(Path.GetFullPath(Path.Combine(install.Root, "entry.exe")), entryExe);
    }

    private sealed class TempInstall : IDisposable {

        public string Root { get; } = Directory.CreateTempSubdirectory().FullName;

        public TempInstall(params string[] files) {
            foreach (var file in files) {
                var path = Path.Combine(Root, file);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, []);
            }
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);

    }

}
