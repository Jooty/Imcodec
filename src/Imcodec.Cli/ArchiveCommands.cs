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
using Imcodec.ObjectProperty;
using Imcodec.Wad;
using Newtonsoft.Json;

namespace Imcodec.Cli;

public sealed class ArchiveCommands {

    private static readonly List<string> s_deserExtIncludeList = ["xml", "bin"];

    /// <summary>
    /// Unpacks the given archive file to the specified output directory.
    /// </summary>
    /// <param name="archivePath">The path to the archive file.</param>
    /// <param name="outputPath">The path to the output directory.</param>
    /// <remarks>
    /// If the output path is not specified, the current directory will be used.
    /// </remarks>
    [Command("unpack")]
    public void UnpackArchive(
            [Argument(Description = "The path to the archive file.")] string archivePath,
            [Argument(Description = "The path to the output directory")] string outputPath = ".",
            [Option("deser", Description = "Attempt deserialization of archive files")] bool attemptDeserialization = false,
            [Option("verbose", Description = "Enable verbose output. Note that for larger archives, "
                + "this can tremendously decrease performance.")] bool verbose = false) {
        // Validate that a file exists at the given path. We'll also begin parsing the file as if it was
        // an archive. If at any point we determine that the file is not a valid archive, we'll stop and
        // inform the user.
        if (!File.Exists(archivePath)) {
            Console.WriteLine($"The specified archive file '{archivePath}' does not exist.");
            return;
        }

        // Read the file data and parse it as an archive. If the file is not a valid archive, we'll inform
        // the user and stop.
        var fileData = File.ReadAllBytes(archivePath);
        Archive archive;
        using var archiveStream = new MemoryStream(fileData);
        try {
            archive = ArchiveParser.Parse(archiveStream)!;
            if (archive == null) {
                Console.WriteLine("The specified archive file is not a valid WAD archive.");
                return;
            }
        }
        catch (InvalidArchiveFormatException) {
            Console.WriteLine("The specified archive file is not a valid WAD archive.");
            return;
        }

        var archiveName = IOUtility.ExtractFileName(archivePath);

        // If the output directory does not exist, create it.
        outputPath = GetOutputDirectory(archivePath, outputPath, archiveName!);
        if (!Directory.Exists(outputPath)) {
            _ = Directory.CreateDirectory(outputPath);
        }

        var files = UnpackArchiveFiles(archive);
        WriteArchiveFilesToDisk(files, outputPath, attemptDeserialization, verbose);

        Console.WriteLine($"Successfully extracted '{archiveName}' to '{outputPath}'.");
    }

    public static Dictionary<FileEntry, byte[]?> UnpackArchiveFiles(Archive archive) {
        // Files within the archive are lazy loaded. We'll iterate through each file and extract the data.
        var files = new Dictionary<FileEntry, byte[]?>();
        foreach (var entry in archive.Files) {
            var fileData = archive.OpenFile(entry.Key);
            var fileEntry = entry.Value.Value;
            if (fileData == null) {
                Console.WriteLine($"Failed to extract file '{entry.Key}'.");
                continue;
            }

            files.Add(fileEntry, fileData.Value.ToArray());
        }

        return files;
    }

    public static void WriteArchiveFilesToDisk(Dictionary<FileEntry, byte[]?> files,
                                               string outputPath,
                                               bool attemptDeserialization,
                                               bool verbose) {
        foreach (var file in files) {
            var fileEntry = file.Key;
            var fileData = file.Value;
            var fileExt = IOUtility.ExtractFileExtension(fileEntry.FileName!);
            var fileOutputPath = CreateFileOutputPath(outputPath, fileEntry.FileName!);

            // Either write to the file, or attempt deserialization. If we fail to deserialize, we'll write
            // the bytes to disk.
            if (attemptDeserialization && s_deserExtIncludeList.Contains(fileExt)) {
                var deserializedData = Deserialization.TryDeserializeFile(fileEntry.FileName!, fileData!);
                if (deserializedData != null) {
                    // If we deserialized successfully, remove the existing extension and add the deserialization
                    // suffix.
                    fileOutputPath = IOUtility.RemoveExtension(fileOutputPath);
                    fileOutputPath = $"{fileOutputPath}{Deserialization.DeserializationSuffix}";
                    File.WriteAllText(fileOutputPath, deserializedData);

                    if (verbose) {
                        Console.WriteLine($"Deserialized '{fileEntry.FileName}' to '{fileOutputPath}'.");
                    }

                    continue;
                }
            }

            File.WriteAllBytes(fileOutputPath, fileData!);

            if (verbose) {
                Console.WriteLine($"Extracted '{fileEntry.FileName}' to '{fileOutputPath}'.");
            }
        }
    }

    private static string CreateFileOutputPath(string basePath, string fileName) {
        // Get the directory path and file name
        var directoryPath = Path.GetDirectoryName(fileName);
        var actualFileName = Path.GetFileName(fileName);

        // If there's no directory structure in fileName, just combine base path with filename
        if (string.IsNullOrEmpty(directoryPath)) {
            return Path.Combine(basePath, actualFileName);
        }

        // Combine base path with the directory structure
        var fullDirectoryPath = Path.Combine(basePath, directoryPath);

        // Create all necessary directories
        _ = Directory.CreateDirectory(fullDirectoryPath);

        // Return the full path including the file name
        return Path.Combine(fullDirectoryPath, actualFileName);
    }

    private static string GetOutputDirectory(string archivePath, string outputPath, string archiveName) {
        // The character '.' is used to represent the current directory. If the output path is the current
        // directory, we'll use the archive name as the output directory.
        if (outputPath == ".") {
            outputPath = Path.Combine(Path.GetDirectoryName(archivePath)!, IOUtility.RemoveExtension(archiveName));
        }

        return outputPath;
    }

}
