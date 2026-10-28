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

namespace Imcodec.Proxy;

/// <summary>
/// The parsed login endpoint of a client command line, with the argument list that carried it.
/// </summary>
public readonly record struct ClientLoginEndpoint(string Host, int Port, int FlagIndex);

/// <summary>
/// Reads and rewrites the login endpoint a patcher put on the client's command line, and hides the rest
/// of the command line, which carries the account's login token, from logs and captures.
/// </summary>
public static class ClientCommandLine {

    public const string LOGIN_ENDPOINT_FLAG = "-L";
    public const string REDACTED_ARGUMENT = "[REDACTED]";

    /// <summary>
    /// Finds the <c>-L host port</c> triple wherever it sits on the command line.
    /// </summary>
    /// <param name="arguments">The client's arguments, without the executable.</param>
    /// <exception cref="InvalidDataException">The flag is absent or lacks a host and a numeric port behind it.</exception>
    public static ClientLoginEndpoint FindLoginEndpoint(IReadOnlyList<string> arguments) {
        for (var i = 0; i < arguments.Count; i++) {
            if (arguments[i] != LOGIN_ENDPOINT_FLAG) {
                continue;
            }
            if (i + 2 >= arguments.Count || !int.TryParse(arguments[i + 2], out var port)) {
                break;
            }
            return new ClientLoginEndpoint(arguments[i + 1], port, i);
        }
        throw new InvalidDataException($"The client command line has no '{LOGIN_ENDPOINT_FLAG} <host> <port>'.");
    }

    /// <summary>
    /// Returns a copy of the arguments with only the login host and port replaced, every other argument intact.
    /// </summary>
    public static string[] WithLoginEndpoint(IReadOnlyList<string> arguments, string host, int port) {
        var endpoint = FindLoginEndpoint(arguments);
        var rewritten = arguments.ToArray();
        rewritten[endpoint.FlagIndex + 1] = host;
        rewritten[endpoint.FlagIndex + 2] = port.ToString();
        return rewritten;
    }

    /// <summary>
    /// Renders the command line for a log with every argument but the login endpoint replaced by
    /// <see cref="REDACTED_ARGUMENT"/>, so the login token never reaches a log or a capture.
    /// </summary>
    public static string Redacted(IReadOnlyList<string> arguments) {
        var endpoint = TryFindLoginEndpoint(arguments);
        var shown = arguments.Select((argument, index) =>
            endpoint is { } found && index >= found.FlagIndex && index <= found.FlagIndex + 2
                ? argument
                : REDACTED_ARGUMENT);
        return string.Join(' ', shown);
    }

    /// <summary>
    /// Like <see cref="FindLoginEndpoint"/>, but returns null instead of throwing when there is none.
    /// </summary>
    public static ClientLoginEndpoint? TryFindLoginEndpoint(IReadOnlyList<string> arguments) {
        try {
            return FindLoginEndpoint(arguments);
        }
        catch (InvalidDataException) {
            return null;
        }
    }

}
