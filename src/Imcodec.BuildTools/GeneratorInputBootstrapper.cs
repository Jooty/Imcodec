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
using System.Xml;
using Imcodec.Wad;

namespace Imcodec.BuildTools;

/// <summary>
/// Produces the inputs the Imcodec source generators consume - the message definitions and the client
/// type dump - directly from a client installation. This type depends only on input-free projects, so it
/// can be built and run before any generated code exists, which is what keeps the generator bootstrap
/// free of a chicken-and-egg dependency on the rest of the toolchain.
/// </summary>
public static class GeneratorInputBootstrapper {

    private const string CLIENT_DATA_DIRECTORY_NAME = "Data";
    private const string CLIENT_GAME_DATA_DIRECTORY_NAME = "GameData";
    private const string ARCHIVE_FILE_PATTERN = "*.wad";
    private const string MESSAGE_DEFINITION_FILE_PATTERN = "*Messages*.xml";
    private const string PROTOCOL_SERVICE_ID_ELEMENT_NAME = "ServiceID";
    private const char WORLD_ARCHIVE_NAME_SEPARATOR = '-';
    private static readonly byte[] EXCLUDED_PROTOCOL_SERVICE_IDS = [1, 19];

    /// <summary>
    /// Extracts every message definition the message generator understands from the client's root data
    /// archive into <paramref name="outputDirectory"/>, flattening archive paths so the generator's
    /// non-recursive glob sees them. Returns the paths that were written.
    /// </summary>
    public static IReadOnlyList<string> ExtractMessageDefinitions(string clientDirectory, string outputDirectory) {
        var archivePath = FindRootArchive(clientDirectory)
            ?? throw new InvalidOperationException(
                $"No data archive containing message definitions was found under '{clientDirectory}'.");

        using var archiveStream = File.OpenRead(archivePath);
        var archive = ArchiveParser.Parse(archiveStream)
            ?? throw new InvalidOperationException($"'{archivePath}' is not a readable data archive.");

        _ = Directory.CreateDirectory(outputDirectory);

        var written = new List<string>();
        foreach (var entryName in archive.Files.Keys) {
            var fileName = Path.GetFileName(entryName);
            if (!FileSystemName.MatchesSimpleExpression(MESSAGE_DEFINITION_FILE_PATTERN, fileName, ignoreCase: true)) {
                continue;
            }

            var contents = archive.OpenFile(entryName)
                ?? throw new InvalidOperationException($"'{entryName}' could not be read from '{archivePath}'.");
            var bytes = contents.ToArray();
            if (IsExcludedProtocol(bytes)) {
                continue;
            }

            var destination = Path.Combine(outputDirectory, fileName);
            File.WriteAllBytes(destination, bytes);
            written.Add(destination);
        }

        if (written.Count == 0) {
            throw new InvalidOperationException($"No message definitions were found in '{archivePath}'.");
        }

        return written;
    }

    /// <summary>
    /// Writes the client type dump into <paramref name="outputDirectory"/> and returns its path. The dump
    /// is produced by the static type dumper when it is part of the build; otherwise this reports that the
    /// dumper is not wired in so the caller can fall back to a hand-dropped dump.
    /// </summary>
    public static string DumpTypes(string clientDirectory, string outputDirectory) {
        _ = Directory.CreateDirectory(outputDirectory);
#if IMCODEC_TYPEDUMP_AVAILABLE
        return Imcodec.TypeDump.TypeDumper.Dump(clientDirectory, outputDirectory);
#else
        _ = clientDirectory;
        throw new NotSupportedException(
            "The static type dumper is not part of this build; place a type dump JSON in the output "
            + "directory instead.");
#endif
    }

    private static string? FindRootArchive(string clientDirectory) {
        var gameDataDirectory = ResolveGameDataDirectory(clientDirectory);
        if (gameDataDirectory is null) {
            return null;
        }

        // World and zone archives are named "<world>-<zone>.wad"; the root data archive carries no such
        // separator, so unseparated names are the cheap candidates to try first. The decision is made by
        // content, though: the root archive is simply whichever one holds the message definitions.
        var orderedCandidates = Directory
            .EnumerateFiles(gameDataDirectory, ARCHIVE_FILE_PATTERN, SearchOption.TopDirectoryOnly)
            .OrderBy(path => Path.GetFileNameWithoutExtension(path).Contains(WORLD_ARCHIVE_NAME_SEPARATOR))
            .ThenByDescending(path => new FileInfo(path).Length);

        return orderedCandidates.FirstOrDefault(ContainsMessageDefinitions);
    }

    private static string? ResolveGameDataDirectory(string clientDirectory) {
        var dataDirectory = FindChildDirectory(clientDirectory, CLIENT_DATA_DIRECTORY_NAME);
        return dataDirectory is null ? null : FindChildDirectory(dataDirectory, CLIENT_GAME_DATA_DIRECTORY_NAME);
    }

    private static string? FindChildDirectory(string parent, string name) {
        if (!Directory.Exists(parent)) {
            return null;
        }

        return Directory
            .EnumerateDirectories(parent)
            .FirstOrDefault(child => string.Equals(Path.GetFileName(child), name, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsMessageDefinitions(string archivePath) {
        using var stream = File.OpenRead(archivePath);
        Archive? archive;
        try {
            archive = ArchiveParser.Parse(stream);
        }
        catch (InvalidArchiveFormatException) {
            return false;
        }

        return archive is not null && archive.Files.Keys.Any(entryName =>
            FileSystemName.MatchesSimpleExpression(
                MESSAGE_DEFINITION_FILE_PATTERN, Path.GetFileName(entryName), ignoreCase: true));
    }

    private static bool IsExcludedProtocol(byte[] messageDefinition) {
        var serviceId = ReadProtocolServiceId(messageDefinition);
        return serviceId.HasValue && Array.IndexOf(EXCLUDED_PROTOCOL_SERVICE_IDS, serviceId.Value) >= 0;
    }

    private static byte? ReadProtocolServiceId(byte[] messageDefinition) {
        var settings = new XmlReaderSettings {
            DtdProcessing = DtdProcessing.Prohibit,
            IgnoreComments = true,
            IgnoreWhitespace = true,
        };

        using var stream = new MemoryStream(messageDefinition);
        using var reader = XmlReader.Create(stream, settings);
        while (reader.Read()) {
            if (reader.NodeType == XmlNodeType.Element
                && string.Equals(reader.Name, PROTOCOL_SERVICE_ID_ELEMENT_NAME, StringComparison.OrdinalIgnoreCase)) {
                return byte.TryParse(reader.ReadElementContentAsString(), out var serviceId) ? serviceId : null;
            }
        }

        return null;
    }

}
