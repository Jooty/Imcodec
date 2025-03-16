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

/// <summary>
/// Provides utility methods for working with files and directories.
/// </summary>
internal static class IOUtility {

    /// <summary>
    /// Extracts the file name from the given path.
    /// </summary>
    /// <param name="path">The path to extract the file name from.</param>
    /// <returns>The file name if it exists; otherwise, an empty string.</returns>
    internal static string ExtractFileName(string path) {
        // Get the file name after the last directory separator.
        // Verify it's a file name and not a directory.
        var fileName = path.Split(Path.DirectorySeparatorChar).Last();

        return fileName.Contains('.') ? fileName : "";
    }

    /// <summary>
    /// Extracts the directory path from the given path.
    /// </summary>
    /// <param name="path">The path to extract the directory path from.</param>
    /// <returns>The directory path if it exists; otherwise, an empty string.</returns>
    internal static string ExtractDirectoryPath(string path) {
        // Get the path without the file name.
        // Verify it's a directory path and not a file name.
        var idx = path.LastIndexOf(Path.DirectorySeparatorChar);
        if (idx == -1) {
            return "";
        }

        var pathWithoutFileName = path[..idx];

        return pathWithoutFileName.Contains('.') ? "" : pathWithoutFileName;
    }

    /// <summary>
    /// Gets the extension of the file from the given path.
    /// </summary>
    /// <param name="path">The path to extract the file extension from.</param>
    /// <returns>The file extension if it exists; otherwise, an empty string.</returns>
    internal static string ExtractFileExtension(string path) {
        // Get the file extension from the file name.
        var fileName = ExtractFileName(path);

        return fileName?.Split('.').Last() ?? "";
    }

    /// <summary>
    /// Gets the extension of the file from the given path.
    /// </summary>
    /// <param name="path">The path to extract the file extension from.</param>
    /// <returns>The file extension if it exists; otherwise, an empty string.</returns>
    internal static string RemoveExtension(string path) {
        // Split on the last index of '.'.
        var idx = path.LastIndexOf('.');
        
        return idx == -1 ? path : path[..idx];
    }

}
