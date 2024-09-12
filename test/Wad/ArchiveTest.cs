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

using Imcodec.Wad;

namespace Imcodec.Test.Wad;

public class ArchiveTest {

    [Fact]
    public void Constructor_ShouldInitializeFileEntries() {
        // Arrange
        var files = new Dictionary<string, FileEntry> {
            { "file1.txt", new FileEntry { Offset = 0, UncompressedSize = 100, IsCompressed = false } },
            { "file2.txt", new FileEntry { Offset = 100, UncompressedSize = 200, IsCompressed = true } }
        };
        using var stream = new MemoryStream();

        // Act
        var archive = new Archive(files, stream, 0);

        // Assert
        Assert.Equal(2, archive.FileCount);
    }

    [Fact]
    public void OpenFile_ShouldReturnFileData() {
        // Arrange
        var fileEntry = new FileEntry {
            Offset = 0,
            UncompressedSize = 4,
            IsCompressed = false
        };

        var files = new Dictionary<string, FileEntry> { { "file1.txt", fileEntry } };
        using var stream = new MemoryStream([1, 2, 3, 4]);
        var archive = new Archive(files, stream, 0);

        // Act
        var result = archive.OpenFile("file1.txt");

        // Assert
        Assert.NotNull(result);
        Assert.Equal([1, 2, 3, 4], result.Value.ToArray());
    }

    [Fact]
    public async Task OpenFileAsync_ShouldReturnFileData() {
        // Arrange
        var fileEntry = new FileEntry { Offset = 0, UncompressedSize = 4, IsCompressed = false };
        var files = new Dictionary<string, FileEntry> { { "file1.txt", fileEntry } };
        using var stream = new MemoryStream([1, 2, 3, 4]);
        var archive = new Archive(files, stream, 0);

        // Act
        var result = await archive.OpenFileAsync("file1.txt");

        // Assert
        Assert.NotNull(result);
        Assert.Equal([1, 2, 3, 4], result.Value.ToArray());
    }

    [Fact]
    public void OpenFile_ShouldReturnNullIfFileNotFound() {
        // Arrange
        var files = new Dictionary<string, FileEntry>();
        using var stream = new MemoryStream();
        var archive = new Archive(files, stream, 0);

        // Act
        var result = archive.OpenFile("nonexistent.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task OpenFileAsync_ShouldReturnNullIfFileNotFound() {
        // Arrange
        var files = new Dictionary<string, FileEntry>();
        using var stream = new MemoryStream();
        var archive = new Archive(files, stream, 0);

        // Act
        var result = await archive.OpenFileAsync("nonexistent.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Package_ShouldReturnCorrectMemoryBlock() {
        // Arrange
        var fileEntry1 = new FileEntry { Offset = 0, UncompressedSize = 4, IsCompressed = false };
        var fileEntry2 = new FileEntry { Offset = 4, UncompressedSize = 4, IsCompressed = false };
        var files = new Dictionary<string, FileEntry> {
            { "file1.txt", fileEntry1 },
            { "file2.txt", fileEntry2 }
        };
        using var stream = new MemoryStream([1, 2, 3, 4, 5, 6, 7, 8]);
        var archive = new Archive(files, stream, 1);

        // Act
        var result = archive.Package();

        // Assert
        var expectedHeader = "KIWAD"u8.ToArray();
        var expectedVersion = BitConverter.GetBytes((uint) 1);
        var expectedFileCount = BitConverter.GetBytes((uint) 2);
        var expectedData = expectedHeader.Concat(expectedVersion).Concat(expectedFileCount).ToArray();
        Assert.True(result.Span[..expectedData.Length].SequenceEqual(expectedData));
    }

}
