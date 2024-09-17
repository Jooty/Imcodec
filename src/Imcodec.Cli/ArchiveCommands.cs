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

    private const string DeserializationSuffix = "_deser.json";
    private static readonly List<string> s_deserExtIncludeList = [ "xml", "bin" ];

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
            [Option("deser", Description = "Attempt deserialization of archive files")] bool attemptDeserialization = false) {
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
        using var archiveStream = new MemoryStream(fileData);
        var archive = ArchiveParser.Parse(archiveStream);
        if (archive == null) {
            Console.WriteLine("The specified archive file is not a valid WAD archive.");
            return;
        }

        var archiveName = IOUtility.ExtractFileName(archivePath);

        // If the output directory does not exist, create it.
        outputPath = GetOutputDirectory(archivePath, outputPath, archiveName!);
        if (!Directory.Exists(outputPath)) {
            Directory.CreateDirectory(outputPath);
        }

        var files = UnpackArchiveFiles(archive);
        WriteArchiveFilesToDisk(files, outputPath, attemptDeserialization);
    }

    public static Dictionary<FileEntry, byte[]?> UnpackArchiveFiles(Archive archive) {
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
                                               bool attemptDeserialization) {
        foreach (var file in files) {
            var fileEntry = file.Key;
            var fileData = file.Value;
            var fileExt = IOUtility.ExtractFileExtension(fileEntry.FileName!);
            var fileOutputPath = CreateFileOutputPath(outputPath, fileEntry.FileName!);

            // Either write to the file, or attempt deserialization. If we fail to deserialize, we'll write
            // the bytes to disk.
            if (attemptDeserialization && s_deserExtIncludeList.Contains(fileExt)) {
                var deserializedData = TryDeserializeFile(fileEntry.FileName!, fileData!);
                if (deserializedData != null) {
                    // If we deserialized successfully, remove the existing extension and add the deserialization
                    // suffix.
                    fileOutputPath = IOUtility.RemoveExtension(fileOutputPath);
                    fileOutputPath = $"{fileOutputPath}{DeserializationSuffix}";
                    File.WriteAllText(fileOutputPath, deserializedData);

                    continue;
                }
            }

            File.WriteAllBytes(fileOutputPath, fileData!);
        }
    }

    private static string? TryDeserializeFile(string fileName, byte[] fileData) {
        try {
            var bindSerializer = new FileSerializer();
            if (bindSerializer.Deserialize<PropertyClass>(fileData, out var propertyClass)) {
                var jsonSerializerSettings = new JsonSerializerSettings {
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };

                return JsonConvert.SerializeObject(propertyClass, Formatting.Indented, jsonSerializerSettings);
            }
            else {
                return null;
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to deserialize file ({fileName}): {ex.Message}");
            return null;
        }
    }

    private static string CreateFileOutputPath(string basePath, string fileName) {
        var actualFileName = IOUtility.ExtractFileName(fileName);
        var actualPath = IOUtility.ExtractDirectoryPath(fileName);

        var fullPath = Path.Combine(basePath, actualPath!);
        var fullOutputPath = Path.Combine(fullPath, actualFileName!);
        if (!Directory.Exists(fullPath)) {
            Directory.CreateDirectory(fullPath);
        }

        return fullOutputPath;
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
