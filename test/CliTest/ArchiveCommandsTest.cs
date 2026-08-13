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

using Imcodec.Cli;
using Imcodec.Wad;

namespace Imcodec.Test.CliTest;

public sealed class ArchiveCommandsTest {

    [Fact]
    public void UnpackArchiveFiles_ExtractsAllFiles() {
        // Arrange
        const int fileCount = 16;
        const int fileSize = 16;
        var streamData = new byte[fileCount * fileSize];
        var files = new Dictionary<string, FileEntry>();
        for (var i = 0; i < fileCount; i++) {
            var offset = i * fileSize;
            for (var j = 0; j < fileSize; j++) {
                streamData[offset + j] = (byte) (i * fileSize + j);
            }

            files.Add($"file{i}.bin", new FileEntry {
                FileName = $"file{i}.bin",
                Offset = (uint) offset,
                UncompressedSize = fileSize,
                IsCompressed = false
            });
        }

        using var stream = new MemoryStream(streamData);
        var archive = new Archive(files, stream, 0);

        // Act
        var unpacked = ArchiveCommands.UnpackArchiveFiles(archive);

        // Assert
        Assert.Equal(fileCount, unpacked.Count);
        for (var i = 0; i < fileCount; i++) {
            var expectedData = Enumerable.Range(i * fileSize, fileSize).Select(v => (byte) v).ToArray();
            var fileEntry = unpacked.Single(pair => pair.Key.FileName == $"file{i}.bin");

            Assert.Equal(expectedData, fileEntry.Value);
        }
    }

    [Fact]
    public void WriteArchiveFilesToDisk_WritesAllFiles() {
        // Arrange
        var outputPath = Path.Combine(Path.GetTempPath(), $"imcodec-test-{Guid.NewGuid():N}");
        var files = new Dictionary<FileEntry, byte[]?>();
        for (var i = 0; i < 8; i++) {
            files.Add(new FileEntry { FileName = $"file{i}.bin" }, [.. Enumerable.Repeat((byte) i, 8)]);
        }

        try {
            // The CLI creates the output directory before writing; mirror that.
            Directory.CreateDirectory(outputPath);

            // Act
            ArchiveCommands.WriteArchiveFilesToDisk(files, outputPath, attemptDeserialization: false, verbose: false);

            // Assert
            for (var i = 0; i < 8; i++) {
                var outputFile = Path.Combine(outputPath, $"file{i}.bin");

                Assert.True(File.Exists(outputFile));
                Assert.Equal(Enumerable.Repeat((byte) i, 8).ToArray(), File.ReadAllBytes(outputFile));
            }
        }
        finally {
            if (Directory.Exists(outputPath)) {
                Directory.Delete(outputPath, true);
            }
        }
    }

}
