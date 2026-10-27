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

using System.IO.Enumeration;
using Cocona;
using Imcodec.Wad;

namespace Imcodec.Bootstrap;

public class Program {

    private const string ROOT_ARCHIVE_FILE_NAME = "Root.wad";
    private const string MESSAGE_DEFINITION_FILE_PATTERN = "*Messages*.xml";
    private const string DEFAULT_MESSAGE_DEFINITION_DIRECTORY = "src/Imcodec.MessageLayer/GeneratorInput";
    private const string DEFAULT_TYPE_DUMP_DIRECTORY = "src/Imcodec.ObjectProperty/GeneratorInput";
    private const int FAILURE_EXIT_CODE = 1;
    private const int NOT_AVAILABLE_EXIT_CODE = 3;
    private static readonly string[] EXCLUDED_MESSAGE_DEFINITION_FILE_NAMES = [
        "AISClientMessages.xml",
        "BaseMessages.xml",
    ];

    public static void Main(string[] args)
        => CoconaApp.Run<Program>(args);

    [Command("messages", Description = "Extract the message definition XMLs from the client's root archive.")]
    public int Messages(
        [Option("client", Description = "Client install directory.")] string client,
        [Option("out", Description = "Output directory.")] string? @out = null) {
        var outputDirectory = @out ?? DEFAULT_MESSAGE_DEFINITION_DIRECTORY;

        var archivePath = FindRootArchive(client);
        if (archivePath is null) {
            return Fail($"{ROOT_ARCHIVE_FILE_NAME} not found under {Path.Combine(client, "Data", "GameData")}");
        }

        using var archiveStream = File.OpenRead(archivePath);
        var archive = ArchiveParser.Parse(archiveStream);
        if (archive is null) {
            return Fail($"{archivePath} is not a readable archive");
        }

        Directory.CreateDirectory(outputDirectory);
        var written = 0;
        foreach (var entryName in archive.Files.Keys) {
            var fileName = Path.GetFileName(entryName);
            if (!FileSystemName.MatchesSimpleExpression(MESSAGE_DEFINITION_FILE_PATTERN, fileName, ignoreCase: true)
                || EXCLUDED_MESSAGE_DEFINITION_FILE_NAMES.Contains(fileName, StringComparer.OrdinalIgnoreCase)) {
                continue;
            }

            var contents = archive.OpenFile(entryName);
            if (contents is null) {
                return Fail($"{entryName} could not be read from {archivePath}");
            }

            File.WriteAllBytes(Path.Combine(outputDirectory, fileName), contents.Value.ToArray());
            written++;
        }

        if (written == 0) {
            return Fail($"no message definition files found in {archivePath}");
        }

        Console.WriteLine($"imcodec-bootstrap: wrote {written} message definition files to {outputDirectory}");
        return 0;
    }

    [Command("types", Description = "Dump the client's property class types (not available yet).")]
    public int Types(
        [Option("client", Description = "Client install directory.")] string client,
        [Option("out", Description = "Output directory.")] string? @out = null) {
        var outputDirectory = @out ?? DEFAULT_TYPE_DUMP_DIRECTORY;
        Console.Error.WriteLine($"imcodec-bootstrap: static type dumper not available yet; drop a type dump into {outputDirectory}");
        return NOT_AVAILABLE_EXIT_CODE;
    }

    [Command("all", Description = "Run messages, then types.")]
    public int All(
        [Option("client", Description = "Client install directory.")] string client,
        [Option("messages-out", Description = "Message definition output directory.")] string? messagesOut = null,
        [Option("types-out", Description = "Type dump output directory.")] string? typesOut = null) {
        var messagesResult = Messages(client, messagesOut);
        return messagesResult != 0 ? messagesResult : Types(client, typesOut);
    }

    private static string? FindRootArchive(string client) {
        var dataDirectory = FindChild(client, "Data", directory: true);
        var gameDataDirectory = dataDirectory is null ? null : FindChild(dataDirectory, "GameData", directory: true);
        return gameDataDirectory is null ? null : FindChild(gameDataDirectory, ROOT_ARCHIVE_FILE_NAME, directory: false);
    }

    private static string? FindChild(string parent, string name, bool directory) {
        if (!Directory.Exists(parent)) {
            return null;
        }

        var children = directory ? Directory.EnumerateDirectories(parent) : Directory.EnumerateFiles(parent);
        return children.FirstOrDefault(child => string.Equals(Path.GetFileName(child), name, StringComparison.OrdinalIgnoreCase));
    }

    private static int Fail(string reason) {
        Console.Error.WriteLine($"imcodec-bootstrap: {reason}");
        return FAILURE_EXIT_CODE;
    }

}
