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

}
