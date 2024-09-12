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

namespace Imcodec.Cli;

internal static class IOUtility {

    internal static string GetOutputDirectory(string archivePath, string outputPath, string archiveName) {
        // The character '.' is used to represent the current directory. If the output path is the current
        // directory, we'll use the archive name as the output directory.
        if (outputPath == ".") {
            outputPath = Path.Combine(Path.GetDirectoryName(archivePath)!, RemoveExtension(archiveName));
        }

        return outputPath;
    }

    internal static string GetOutputFile(string archivePath, string outputPath) {
        if (outputPath == ".") {
            outputPath = archivePath;
        }

        return outputPath;
    }

    internal static string? ExtractFileName(string path) {
        // Get the file name after the last directory separator.
        // Verify it's a file name and not a directory.
        var fileName = path.Split(Path.DirectorySeparatorChar).Last();
        return fileName.Contains('.') ? fileName : null;
    }

    internal static string? ExtractDirectoryPath(string path) {
        // Get the path without the file name.
        // Verify it's a directory path and not a file name.
        var idx = path.LastIndexOf(Path.DirectorySeparatorChar);
        if (idx == -1) {
            return null;
        }

        var pathWithoutFileName = path[..idx];
        return pathWithoutFileName.Contains('.') ? null : pathWithoutFileName;
    }

    internal static string RemoveExtension(string path) {
        // Remove the file extension from the file name.
        var fileName = ExtractFileName(path);
        return fileName?.Split('.').First() ?? path;
    }

}
