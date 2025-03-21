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

    public Dictionary<string, Lazy<FileEntry>> Files { get; private set; } = [];
    private readonly Stream _archiveStream;
    private readonly uint _version;

    /// <summary>
    /// Initializes a new instance of the <see cref="Archive"/> class.
    /// </summary>
    /// <param name="files">The files in the archive.</param>
    /// <param name="archiveStream">The stream containing the archive data.</param>
    /// <param name="version">The version of the archive.</param>
    public Archive(Dictionary<string, FileEntry> files, Stream archiveStream, uint version) {
        _archiveStream = archiveStream;
        _version = version;

        foreach (var file in files) {
            Files.Add(file.Key, new Lazy<FileEntry>(() => file.Value));
        }
    }

    /// <summary>
    /// Opens a file as a memory block.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <returns>A memory block of the file data, or null if not found.</returns>
    public Memory<byte>? OpenFile(string fileName) {
        if (Files.TryGetValue(fileName, out var lazyEntry)) {
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
    public async Task<Memory<byte>?> OpenFileAsync(string fileName) {
        if (Files.TryGetValue(fileName, out var lazyEntry)) {
            var fileEntry = lazyEntry.Value;
            
            return await ReadFileDataAsync(fileEntry);
        }

        return null;
    }

    /// <summary>
    /// Gets the number of file entries in the archive.
    /// </summary>
    public int FileCount
        => Files.Count;

    /// <summary>
    /// Checks if the archive contains a file.
    /// </summary>
    public bool ContainsFile(string fileName)
        => Files.ContainsKey(fileName);

    /// <summary>
    /// Gets the size of the archive.
    /// </summary>
    /// <returns>The size of the archive.</returns>
    public uint Size() {
        return (uint) _archiveStream.Length;
    }

    /// <summary>
    /// Packages the archive into a memory block.
    /// </summary>
    /// <returns>A memory block of the archive data.</returns>
    public Memory<byte> Package() {
        var writer = new BitWriter();

        // Write magic header
        var header = "KIWAD"u8.ToArray();
        writer.WriteBytes(header);

        writer.WriteUInt32(_version);
        writer.WriteUInt32((uint) Files.Count);

        foreach (var fileEntry in Files.Values) {
            fileEntry.Value.PackToStream(writer);
        }

        return writer.GetData();
    }

    public MemoryStream GetData() {
        var data = new MemoryStream((int) _archiveStream.Length);
        _archiveStream.Seek(0, SeekOrigin.Begin);
        _archiveStream.CopyTo(data);
        data.Seek(0, SeekOrigin.Begin);

        return data;
    }

    private Memory<byte> ReadFileData(FileEntry fileEntry) {
        _ = _archiveStream.Seek(fileEntry.Offset, SeekOrigin.Begin);
        var readLen = fileEntry.IsCompressed ? fileEntry.CompressedSize : fileEntry.UncompressedSize;
        var fileSpan = ReadFromStream(_archiveStream, readLen);

        return fileEntry.IsCompressed 
            ? ZLibUtility.Inflate(fileSpan, (int) fileEntry.UncompressedSize) 
            : fileSpan;
    }

    private async Task<Memory<byte>> ReadFileDataAsync(FileEntry fileEntry) {
        _ = _archiveStream.Seek(fileEntry.Offset, SeekOrigin.Begin);
        var buffer = new byte[fileEntry.UncompressedSize];
        await _archiveStream.ReadExactlyAsync(buffer);

        return fileEntry.IsCompressed 
            ? await ZLibUtility.InflateAsync(buffer, (int) fileEntry.CompressedSize) 
            : (Memory<byte>) buffer;
    }

    private static Memory<byte> ReadFromStream(Stream stream, long size) {
        var buffer = new byte[size];
        var read = stream.Read(buffer, 0, (int) size);
        
        return read != size
            ? throw new Exception($"{nameof(ReadFromStream)} did not read proper size. GOT: {read} EXPECTED: {size}")
            : (Memory<byte>) buffer;
    }

}
