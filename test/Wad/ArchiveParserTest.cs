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

public class ArchiveParserTest {

    [Fact]
    public void Parse_ValidArchiveStream_ReturnsArchive() {
        // Arrange
        var archiveData = new byte[] {
            // "KIWAD" magic header
            0x4B, 0x49, 0x57, 0x41, 0x44,
            // Version
            0x01, 0x00, 0x00, 0x00,
            // File count
            0x01, 0x00, 0x00, 0x00,
            // File entry
            0x10, 0x00, 0x00, 0x00, // Offset
            0x20, 0x00, 0x00, 0x00, // Size
            0x20, 0x00, 0x00, 0x00, // Compressed size
            0x00,                   // IsCompressed
            0x12, 0x34, 0x56, 0x78, // CRC32
            // File name length (4 bytes)
            0x06, 0x00, 0x00, 0x00,
            // File name
            0x66, 0x69, 0x6C, 0x65, 0x31, 0x00 // "file1\0"
        };
        using var stream = new MemoryStream(archiveData);

        // Act
        var archive = ArchiveParser.Parse(stream);

        // Assert
        Assert.NotNull(archive);
        Assert.True(archive.FileCount == 1);
        Assert.True(archive.ContainsFile("file1"));
    }

    [Fact]
    public void Parse_InvalidMagicHeader_ThrowsInvalidArchiveFormatException() {
        // Arrange
        var invalidHeaderData = new byte[] {
            // Invalid magic header
            0x00, 0x00, 0x00, 0x00, 0x00
        };
        using var stream = new MemoryStream(invalidHeaderData);

        // Act & Assert
        Assert.Throws<InvalidArchiveFormatException>(() => ArchiveParser.Parse(stream));
    }

    [Fact]
    public void Parse_Version2WithPadding_ParsesCorrectly() {
        // Arrange
        var archiveData = new byte[] {
            // "KIWAD" magic header
            0x4B, 0x49, 0x57, 0x41, 0x44,
            // Version
            0x02, 0x00, 0x00, 0x00,
            // File count
            0x01, 0x00, 0x00, 0x00,
            // Padding byte
            0x00,
            // File entry
            0x10, 0x00, 0x00, 0x00, // Offset
            0x20, 0x00, 0x00, 0x00, // Size
            0x20, 0x00, 0x00, 0x00, // Compressed size
            0x01,                   // IsCompressed
            0x12, 0x34, 0x56, 0x78, // CRC32
            // File name length (4 bytes)
            0x06, 0x00, 0x00, 0x00,
            // File name
            0x66, 0x69, 0x6C, 0x65, 0x31, 0x00 // "file1\0"
        };
        using var stream = new MemoryStream(archiveData);

        // Act
        var archive = ArchiveParser.Parse(stream);

        // Assert
        Assert.NotNull(archive);
        Assert.True(archive.FileCount == 1);
        Assert.True(archive.ContainsFile("file1"));
    }

}
