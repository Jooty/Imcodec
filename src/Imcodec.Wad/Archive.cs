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
/// Represents an archive, holding multiple packed files.
/// </summary>
public sealed class Archive {

    private readonly Dictionary<string, Lazy<FileEntry>> _fileEntries = [];
    private readonly Stream _archiveStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="Archive"/> class.
    /// </summary>
    /// <param name="files">The files in the archive.</param>
    /// <param name="archiveStream">The stream containing the archive data.</param>
    public Archive(Dictionary<string, FileEntry> files, Stream archiveStream) {
        _archiveStream = archiveStream;

        foreach (var file in files) {
            _fileEntries.Add(file.Key, new Lazy<FileEntry>(() => file.Value));
        }
    }

    /// <summary>
    /// Opens a file as a memory block.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>A memory block of the file data, or null if not found.</returns>
    public ReadOnlyMemory<byte>? OpenFile(string fileName) {
        if (_fileEntries.TryGetValue(fileName, out var lazyEntry)) {
            var fileEntry = lazyEntry.Value;
            return ReadFileData(fileEntry);
        }

        return null;
    }

    /// <summary>
    /// Opens a file as a memory block asynchronously.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>A memory block of the file data, or null if not found.</returns>
    public async Task<ReadOnlyMemory<byte>?> OpenFileAsync(string fileName) {
        if (_fileEntries.TryGetValue(fileName, out var lazyEntry)) {
            var fileEntry = lazyEntry.Value;
            return await ReadFileDataAsync(fileEntry);
        }

        return null;
    }

    /// <summary>
    /// Gets the number of file entries in the archive.
    /// </summary>
    public int FileCount
        => _fileEntries.Count;

    /// <summary>
    /// Checks if the archive contains a file.
    /// </summary>
    public bool ContainsFile(string fileName)
        => _fileEntries.ContainsKey(fileName);

    /// <summary>
    /// Gets the size of the archive.
    /// </summary>
    /// <returns>The size of the archive.</returns>
    public uint Size() {
        // Get the last file entry. Add the offset and size to get the end of the file.
        var lastFileEntry = _fileEntries.Last().Value.Value;

        return lastFileEntry.Offset + lastFileEntry.Size;
    }

    private ReadOnlyMemory<byte> ReadFileData(FileEntry fileEntry) {
        _archiveStream.Seek(fileEntry.Offset, SeekOrigin.Begin);
        var fileSpan = ReadFromStream(_archiveStream, fileEntry.Size);

        if (fileEntry.IsCompressed) {
            return ZLibCompressionService.Inflate(fileSpan, (int) fileEntry.CompressedSize);
        }

        return new ReadOnlyMemory<byte>(fileSpan.ToArray());
    }

    private async Task<ReadOnlyMemory<byte>> ReadFileDataAsync(FileEntry fileEntry) {
        _archiveStream.Seek(fileEntry.Offset, SeekOrigin.Begin);
        var buffer = new byte[fileEntry.Size];
        await _archiveStream.ReadAsync(buffer, 0, buffer.Length);

        if (fileEntry.IsCompressed) {
            return await ZLibCompressionService.InflateAsync(buffer, (int) fileEntry.CompressedSize);
        }

        return buffer;
    }

    private static ReadOnlySpan<byte> ReadFromStream(Stream stream, long size) {
        var buffer = new byte[size];
        var read = stream.Read(buffer, 0, (int) size);
        if (read != size) {
            throw new Exception($"{nameof(ReadFromStream)} did not read proper size. GOT: {read} EXPECTED: {size}");
        }

        return buffer;
    }

}
