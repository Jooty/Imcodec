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
using Imcodec.BuildTools;
using Imcodec.TypeDump;

namespace Imcodec.Cli;

/// <summary>
/// Regenerates the source generator inputs from a client install. The same work runs automatically
/// during a build; these commands re-run it on demand, which is what you want after a client update.
/// </summary>
public sealed class BootstrapCommands {

    private const string CLIENT_DIRECTORY_ENVIRONMENT_VARIABLE = "ImcodecClientDir";
    private const string MESSAGE_DEFINITION_DIRECTORY = "src/Imcodec.MessageLayer/GeneratorInput";
    private const string TYPE_DUMP_DIRECTORY = "src/Imcodec.ObjectProperty/GeneratorInput";
    private const string TYPE_DUMP_FILE_NAME = "typedump.json";
    private const int FAILURE_EXIT_CODE = 1;
    private const int MISSING_CLIENT_EXIT_CODE = 2;

    /// <summary>
    /// Extracts the message definitions from the client into the message generator's input folder.
    /// </summary>
    [Command("messages", Description = "Extract the client's message definitions.")]
    public int Messages(
            [Option("client", Description = "Client install directory. Defaults to the ImcodecClientDir environment variable.")] string? client = null,
            [Option("out", Description = "Output directory.")] string outputDirectory = MESSAGE_DEFINITION_DIRECTORY) {
        var clientDirectory = ResolveClientDirectory(client);
        if (clientDirectory is null) {
            return MissingClient();
        }

        try {
            var written = GeneratorInputBootstrapper.ExtractMessageDefinitions(clientDirectory, outputDirectory);
            Console.WriteLine($"Extracted {written.Count} message definitions to '{outputDirectory}'.");

            return 0;
        }
        catch (Exception ex) {
            Console.Error.WriteLine(ex.Message);

            return FAILURE_EXIT_CODE;
        }
    }

    /// <summary>
    /// Dumps the client's property class types into the object property generator's input folder. The
    /// dumper runs in-process; <c>--extended</c> adds the extra per-type and per-property detail the
    /// generator does not need but which is useful for inspecting the type system.
    /// </summary>
    [Command("types", Description = "Dump the client's property class types.")]
    public int Types(
            [Option("client", Description = "Client install directory. Defaults to the ImcodecClientDir environment variable.")] string? client = null,
            [Option("out", Description = "Output directory.")] string outputDirectory = TYPE_DUMP_DIRECTORY,
            [Option("extended", Description = "Also emit the extended per-type and per-property detail.")] bool extended = false,
            [Option("verbose", Description = "Report emulation progress to standard error.")] bool verbose = false)
        => DumpTypes(client, outputDirectory, extended, verbose);

    /// <summary>
    /// Refreshes both the message definitions and the client type dump. Run this after a client update.
    /// </summary>
    [Command("all", Description = "Refresh both the message definitions and the client type dump.")]
    public int All(
            [Option("client", Description = "Client install directory. Defaults to the ImcodecClientDir environment variable.")] string? client = null,
            [Option("messages-out", Description = "Message definition output directory.")] string messageDefinitionDirectory = MESSAGE_DEFINITION_DIRECTORY,
            [Option("types-out", Description = "Type dump output directory.")] string typeDumpDirectory = TYPE_DUMP_DIRECTORY) {
        var messagesResult = Messages(client, messageDefinitionDirectory);

        return messagesResult != 0 ? messagesResult : Types(client, typeDumpDirectory);
    }

    internal static int DumpTypes(string? client, string outputDirectory, bool extended, bool verbose) {
        var clientDirectory = ResolveClientDirectory(client);
        if (clientDirectory is null) {
            return MissingClient();
        }

        try {
            _ = Directory.CreateDirectory(outputDirectory);
            var (manifest, extendedManifest) = TypeDumper.Dump(clientDirectory, extended, verbose ? Console.Error : null);

            var dumpPath = Path.Combine(outputDirectory, TYPE_DUMP_FILE_NAME);
            using (var dumpStream = File.Create(dumpPath)) {
                TypeDumper.WriteJson(manifest, dumpStream, extended: extendedManifest);
            }

            Console.WriteLine($"Wrote type dump ({manifest.Classes.Count} types) to '{dumpPath}'.");

            return 0;
        }
        catch (Exception ex) {
            Console.Error.WriteLine(ex.Message);

            return FAILURE_EXIT_CODE;
        }
    }

    private static string? ResolveClientDirectory(string? client)
        => client ?? Environment.GetEnvironmentVariable(CLIENT_DIRECTORY_ENVIRONMENT_VARIABLE);

    private static int MissingClient() {
        Console.Error.WriteLine(
            $"No client directory given. Pass --client or set the {CLIENT_DIRECTORY_ENVIRONMENT_VARIABLE} environment variable.");

        return MISSING_CLIENT_EXIT_CODE;
    }

}
