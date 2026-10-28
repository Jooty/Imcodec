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

using System.Collections.Concurrent;
using Cocona;
using Imcodec.Wad;

namespace Imcodec.Cli;

public sealed class ArchiveCommands {

    private const int FAILURE_EXIT_CODE = 1;
    private static readonly List<string> s_deserExtIncludeList = ["xml", "bin", "bcd"];

    /// <summary>
    /// Unpacks the given archive file to the specified output directory. When the output path is omitted
    /// the archive is extracted to a folder named after it, beside it.
    /// </summary>
    [Command("unpack", Description = "Extract an archive's files to a directory.")]
    public int UnpackArchive(
            [Argument(Description = "The path to the archive file.")] string archivePath,
            [Argument(Description = "The path to the output directory")] string outputPath = ".",
            [Option("deser", Description = "Attempt deserialization of archive files")] bool attemptDeserialization = false,
            [Option("verbose", Description = "Enable verbose output. Note that for larger archives, "
                + "this can tremendously decrease performance.")] bool verbose = false) {
        if (!File.Exists(archivePath)) {
            Console.Error.WriteLine($"The specified archive file '{archivePath}' does not exist.");

            return FAILURE_EXIT_CODE;
        }

        // The archive is streamed, not read whole into memory: a root archive is hundreds of megabytes and
        // entries are inflated lazily. The stream must stay open for the duration of the extraction.
        using var archiveStream = File.OpenRead(archivePath);
        Archive? archive;
        try {
            archive = ArchiveParser.Parse(archiveStream);
        }
        catch (InvalidArchiveFormatException) {
            Console.Error.WriteLine("The specified archive file is not a valid WAD archive.");

            return FAILURE_EXIT_CODE;
        }

        if (archive == null) {
            Console.Error.WriteLine("The specified archive file is not a valid WAD archive.");

            return FAILURE_EXIT_CODE;
        }

        var archiveName = IOUtility.ExtractFileName(archivePath);

        outputPath = GetOutputDirectory(archivePath, outputPath, archiveName!);
        _ = Directory.CreateDirectory(outputPath);

        var files = UnpackArchiveFiles(archive);
        WriteArchiveFilesToDisk(files, outputPath, attemptDeserialization, verbose);

        Console.WriteLine($"Successfully extracted '{archiveName}' to '{outputPath}'.");

        return 0;
    }

    /// <summary>
    /// Lists the entries in the given archive without extracting them. Entry metadata is read from the
    /// header, so nothing is inflated; <c>--verbose</c> adds each entry's compressed and uncompressed size.
    /// </summary>
    [Command("list", Description = "List an archive's entries without extracting them.")]
    public int ListArchive(
            [Argument(Description = "The path to the archive file.")] string archivePath,
            [Option("verbose", Description = "Show each entry's compressed and uncompressed size.")] bool verbose = false) {
        if (!File.Exists(archivePath)) {
            Console.Error.WriteLine($"The specified archive file '{archivePath}' does not exist.");

            return FAILURE_EXIT_CODE;
        }

        using var archiveStream = File.OpenRead(archivePath);
        Archive? archive;
        try {
            archive = ArchiveParser.Parse(archiveStream);
        }
        catch (InvalidArchiveFormatException) {
            Console.Error.WriteLine("The specified archive file is not a valid WAD archive.");

            return FAILURE_EXIT_CODE;
        }

        if (archive == null) {
            Console.Error.WriteLine("The specified archive file is not a valid WAD archive.");

            return FAILURE_EXIT_CODE;
        }

        foreach (var entryName in archive.Files.Keys.OrderBy(static name => name, StringComparer.Ordinal)) {
            if (verbose) {
                var entry = archive.Files[entryName].Value;
                Console.WriteLine($"{entry.UncompressedSize,12} {entry.CompressedSize,12}  {entryName}");
            }
            else {
                Console.WriteLine(entryName);
            }
        }

        Console.WriteLine($"{archive.FileCount} entries in '{IOUtility.ExtractFileName(archivePath)}'.");

        return 0;
    }

    public static Dictionary<FileEntry, byte[]?> UnpackArchiveFiles(Archive archive) {
        // Files within the archive are lazy loaded. Extract each file in parallel.
        var files = new ConcurrentDictionary<FileEntry, byte[]?>();
        Parallel.ForEach(archive.Files, entry => {
            var fileData = archive.OpenFile(entry.Key);
            if (fileData == null) {
                Console.WriteLine($"Failed to extract file '{entry.Key}'.");
                return;
            }

            files[entry.Value.Value] = fileData.Value.ToArray();
        });

        return new Dictionary<FileEntry, byte[]?>(files);
    }

    public static void WriteArchiveFilesToDisk(Dictionary<FileEntry, byte[]?> files,
                                               string outputPath,
                                               bool attemptDeserialization,
                                               bool verbose) {
        // Write (and optionally deserialize) each file in parallel.
        Parallel.ForEach(files, file => {
            var fileEntry = file.Key;
            var fileData = file.Value;
            var fileExt = IOUtility.ExtractFileExtension(fileEntry.FileName!);
            var fileOutputPath = CreateFileOutputPath(outputPath, fileEntry.FileName!);

            // Either write to the file, or attempt deserialization. If we fail to deserialize, we'll write
            // the bytes to disk.
            if (attemptDeserialization && s_deserExtIncludeList.Contains(fileExt)) {
                var deserializedData = Deserialization.TryDeserializeFile(fileEntry.FileName!, fileData!, verbose);
                if (deserializedData != null) {
                    // If we deserialized successfully, remove the existing extension and add the deserialization
                    // suffix.
                    fileOutputPath = IOUtility.RemoveExtension(fileOutputPath);
                    fileOutputPath = $"{fileOutputPath}{Deserialization.DeserializationSuffix}";
                    File.WriteAllText(fileOutputPath, deserializedData);

                    if (verbose) {
                        Console.WriteLine($"Deserialized '{fileEntry.FileName}' to '{fileOutputPath}'.");
                    }

                    return;
                }
            }

            File.WriteAllBytes(fileOutputPath, fileData!);

            if (verbose) {
                Console.WriteLine($"Extracted '{fileEntry.FileName}' to '{fileOutputPath}'.");
            }
        });
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
