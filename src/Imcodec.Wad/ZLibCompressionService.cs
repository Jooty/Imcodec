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

using System.IO.Compression;

namespace Imcodec.Wad;

public static class ZLibCompressionService {

    /// <summary>
    /// Decompresses a span of compressed data using the Zlib Inflate algorithm.
    /// </summary>
    /// <param name="compressedData">The compressed data as a ReadOnlySpan.</param>
    /// <param name="expectedSize">The expected size of the decompressed data.</param>
    /// <returns>The decompressed data as a ReadOnlyMemory.</returns>
    public static ReadOnlyMemory<byte> Inflate(ReadOnlySpan<byte> compressedData, int expectedSize) {
        using var compressedStream = new MemoryStream(compressedData.ToArray());
        using var zlibStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        var decompressedData = new byte[expectedSize];

        int bytesRead = zlibStream.Read(decompressedData, 0, expectedSize);
        if (bytesRead != expectedSize) {
            throw new InvalidOperationException("Decompressed data size does not match the expected size.");
        }

        return new ReadOnlyMemory<byte>(decompressedData);
    }

    /// <summary>
    /// Asynchronously decompresses a span of compressed data using the Zlib Inflate algorithm.
    /// </summary>
    /// <param name="compressedData">The compressed data as a ReadOnlyMemory.</param>
    /// <param name="expectedSize">The expected size of the decompressed data.</param>
    /// <returns>A task that represents the asynchronous decompression operation,
    /// with the decompressed data as a ReadOnlyMemory.</returns>
    public static async Task<ReadOnlyMemory<byte>> InflateAsync(ReadOnlyMemory<byte> compressedData, int expectedSize) {
        using var compressedStream = new MemoryStream(compressedData.ToArray());
        using var zlibStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        var decompressedData = new byte[expectedSize];

        int bytesRead = await zlibStream.ReadAsync(decompressedData, 0, expectedSize);
        if (bytesRead != expectedSize) {
            throw new InvalidOperationException("Decompressed data size does not match the expected size.");
        }

        return decompressedData;
    }

    /// <summary>
    /// Compresses the data in the given ReadOnlySpan using the Zlib Deflate algorithm.
    /// </summary>
    /// <param name="data">The data to be compressed as a ReadOnlySpan.</param>
    /// <returns>The compressed data as a ReadOnlyMemory.</returns>
    public static ReadOnlyMemory<byte> Deflate(ReadOnlySpan<byte> data) {
        using var outputStream = new MemoryStream();
        using var zlibStream = new DeflateStream(outputStream, CompressionMode.Compress);

        zlibStream.Write(data);
        zlibStream.Flush();

        return new ReadOnlyMemory<byte>(outputStream.ToArray());
    }

    /// <summary>
    /// Asynchronously compresses the data in the given ReadOnlyMemory using the Zlib Deflate algorithm.
    /// </summary>
    /// <param name="data">The data to be compressed as a ReadOnlyMemory.</param>
    /// <returns>A task that represents the asynchronous compression operation,
    /// with the compressed data as a ReadOnlyMemory.</returns>
    public static async Task<ReadOnlyMemory<byte>> DeflateAsync(ReadOnlyMemory<byte> data) {
        using var outputStream = new MemoryStream();
        using var zlibStream = new DeflateStream(outputStream, CompressionMode.Compress);

        await zlibStream.WriteAsync(data);
        await zlibStream.FlushAsync();

        return new ReadOnlyMemory<byte>(outputStream.ToArray());
    }

}
