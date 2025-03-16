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

using Imcodec.IO;

namespace Imcodec.Wad;

/// <summary>
///
/// </summary>
public class ArchiveParser {

    /// <summary>
    /// Parses an archive from a stream.
    /// </summary>
    /// <param name="archiveStream">The stream containing the archive data.</param>
    /// <returns>The parsed archive.</returns>
    /// <exception cref="InvalidArchiveFormatException">Thrown when the stream does not contain a valid archive.</exception>
    /// <exception cref="Exception">Thrown when the file name could not be read.</exception>
    public static Archive? Parse(Stream archiveStream) {
        var binaryReader = new BitReader(archiveStream);

        // Validate that this is a valid Archive by reading the first 5 bytes.
        Span<byte> headerBuf = binaryReader.ReadBytes(5);
        if (!IsMagicHeader(headerBuf)) {
            throw new InvalidArchiveFormatException("Invalid magic header.");
        }

        var version = binaryReader.ReadUInt32();
        var fileCount = binaryReader.ReadUInt32();
        var files = new Dictionary<string, FileEntry>();

        // Newer versions have a padding byte here.
        if (version >= 2) {
            _ = binaryReader.ReadBytes(1);
        }

        // Read through each file in the archive and create a FileEntry from the file header.
        for (int i = 0; i < fileCount; i++) {
            var fileEntry = ReadFileEntry(binaryReader);
            if (fileEntry.FileName != null) {
                files.Add(fileEntry.FileName, fileEntry);
            }
        }

        // Verify the amount of file entries read matches the file count.
        if (files.Count != fileCount) {
            throw new InvalidArchiveFormatException("Failed to read all file entries.");
        }

        return new Archive(files, archiveStream, version);
    }

    private static FileEntry ReadFileEntry(BitReader binaryReader) {
        var offset = binaryReader.ReadUInt32();
        var size = binaryReader.ReadUInt32();
        var compressedSize = binaryReader.ReadUInt32();
        var isCompressed = binaryReader.ReadBool();
        var crc32 = binaryReader.ReadUInt32();

        // Format the fileName to remove null terminate operator.
        var rawFileName = binaryReader.ReadBigString().ToString();
        if (rawFileName is "") {
            throw new Exception("Failed to read file name.");
        }

        var fileName = rawFileName!.Replace("\0", "");

        return new FileEntry() {
            FileName = fileName,
            CompressedSize = compressedSize,
            Crc32 = crc32,
            IsCompressed = isCompressed,
            Offset = offset,
            UncompressedSize = size,
        };
    }

    private static bool IsMagicHeader(Span<byte> header) {
        byte[] magicHeader = "KIWAD"u8.ToArray();
        for (int i = 0; i < header.Length; i++) {
            if (header[i] != magicHeader[i]) {
                return false;
            }
        }
        
        return true;
    }

}
