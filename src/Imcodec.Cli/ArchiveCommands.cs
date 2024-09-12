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

using System;
using Cocona;
using Imcodec.Wad;

namespace Imcodec.Cli;

public sealed class ArchiveCommands {

    /// <summary>
    /// Unpacks the given archive file to the specified output directory.
    /// </summary>
    /// <param name="archivePath">The path to the archive file.</param>
    /// <param name="outputPath">The path to the output directory.</param>
    /// <remarks>
    /// If the output path is not specified, the current directory will be used.
    /// </remarks>
    [Command("unpack")]
    public void UnpackArchive([Argument] string archivePath,
                              [Argument] string outputPath = ".") {
        // Validate that a file exists at the given path. We'll also begin parsing the file as if it was
        // an archive. If at any point we determine that the file is not a valid archive, we'll stop and
        // inform the user.
        if (!File.Exists(archivePath)) {
            Console.WriteLine($"The specified archive file '{archivePath}' does not exist.");
            return;
        }

        using var archiveStream = new MemoryStream(File.ReadAllBytes(archivePath));
        var archive = ArchiveParser.Parse(archiveStream);
        if (archive == null) {
            Console.WriteLine("The specified archive file is not a valid WAD archive.");
            return;
        }

        var archiveName = ExtractFileName(archivePath);
        outputPath = GetOutputPath(archivePath, outputPath, archiveName!);

        if (!Directory.Exists(outputPath)) {
            Directory.CreateDirectory(outputPath);
        }

        UnpackFiles(archive, outputPath);
    }

    private static void UnpackFiles(Archive archive, string outputPath) {
        foreach (var entry in archive.Files) {
            // 'entry' is just a record of the file. We need to extract the actual file data.
            var fileData = archive.OpenFile(entry.Key);
            if (fileData == null) {
                Console.WriteLine($"Failed to extract file '{entry.Key}'.");
                continue;
            }

            var actualFileName = ExtractFileName(entry.Key);
            var actualPath = ExtractDirectoryPath(entry.Key) ?? "";

            var fullPath = Path.Combine(outputPath, actualPath!);
            var fullOutputPath = Path.Combine(fullPath, actualFileName!);
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
            }

            File.WriteAllBytes(fullOutputPath, fileData.Value.ToArray());
        }
    }

    private static string GetOutputPath(string archivePath, string outputPath, string archiveName) {
        // The character '.' is used to represent the current directory. If the output path is the current
        // directory, we'll use the archive name as the output directory.
        if (outputPath == ".") {
            outputPath = Path.Combine(Path.GetDirectoryName(archivePath)!, RemoveExtension(archiveName));
        }

        return outputPath;
    }

    private static string? ExtractFileName(string path) {
        // Get the file name after the last directory separator.
        // Verify it's a file name and not a directory.
        var fileName = path.Split(Path.DirectorySeparatorChar).Last();
        return fileName.Contains('.') ? fileName : null;
    }

    private static string? ExtractDirectoryPath(string path) {
        // Get the path without the file name.
        // Verify it's a directory path and not a file name.
        var idx = path.LastIndexOf(Path.DirectorySeparatorChar);
        if (idx == -1) {
            return null;
        }

        var pathWithoutFileName = path[..idx];
        return pathWithoutFileName.Contains('.') ? null : pathWithoutFileName;
    }

    private static string RemoveExtension(string path) {
        // Remove the file extension from the file name.
        var fileName = ExtractFileName(path);
        return fileName?.Split('.').First() ?? path;
    }

}
