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

using Ionic.Zlib;

namespace Imcodec.Wad;

public static class ZLibUtility {

    /// <summary>
    /// Decompresses a span of compressed data using the Zlib Inflate algorithm.
    /// </summary>
    /// <param name="compressedData">The compressed data as a ReadOnlySpan.</param>
    /// <param name="expectedSize">The expected size of the decompressed data.</param>
    /// <returns>The decompressed data as a Memory.</returns>
    public static Memory<byte> Inflate(Memory<byte> compressedData, int expectedSize) {
        // Create the decompression stream.
        var compressedStream = new MemoryStream(compressedData.ToArray());
        using var zLibStream = new ZlibStream(compressedStream, CompressionMode.Decompress);

        var buffer = new byte[expectedSize];
        var outputStream = new MemoryStream();
        int bytesRead;
        while ((bytesRead = zLibStream.Read(buffer, 0, buffer.Length)) > 0) {
            outputStream.Write(buffer, 0, bytesRead);
        }

        if (buffer.Length != expectedSize) {
            throw new InvalidOperationException("Decompressed data size does not match the expected size.");
        }

        return new Memory<byte>(buffer);
    }

    /// <summary>
    /// Decompresses a span of compressed data using the Zlib Inflate algorithm asynchronously.
    /// </summary>
    /// <param name="compressedData">The compressed data as a ReadOnlySpan.</param>
    /// <param name="expectedSize">The expected size of the decompressed data.</param>
    /// <returns>The decompressed data as a Memory.</returns>
    /// <remarks>This method is asynchronous and should be used when decompressing large amounts of data.</remarks>
    public static async Task<Memory<byte>> InflateAsync(Memory<byte> compressedData, int expectedSize) {
        // Create the decompression stream.
        var compressedStream = new MemoryStream(compressedData.ToArray());
        using var zLibStream = new ZlibStream(compressedStream, CompressionMode.Decompress);

        var buffer = new byte[expectedSize];
        var outputStream = new MemoryStream();
        int bytesRead;
        while ((bytesRead = await zLibStream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
            await outputStream.WriteAsync(buffer, 0, bytesRead);
        }

        if (bytesRead != expectedSize) {
            throw new InvalidOperationException("Decompressed data size does not match the expected size.");
        }

        return new Memory<byte>(buffer);
    }

    /// <summary>
    /// Compresses a span of data using the Zlib Deflate algorithm.
    /// </summary>
    /// <param name="data">The data to compress as a ReadOnlySpan.</param>
    /// <returns>The compressed data as a Memory.</returns>
    public static Memory<byte> Deflate(Memory<byte> data) {
        var outputStream = new MemoryStream();
        using var zLibStream = new ZlibStream(outputStream, CompressionMode.Compress, CompressionLevel.Default, true);

        var mutableStream = new MemoryStream(data.ToArray());
        mutableStream.CopyTo(zLibStream);

        zLibStream.Close();

        return new Memory<byte>(outputStream.ToArray());
    }

    /// <summary>
    /// Compresses a span of data using the Zlib Deflate algorithm asynchronously.
    /// </summary>
    /// <param name="data">The data to compress as a ReadOnlySpan.</param>
    /// <returns>The compressed data as a Memory.</returns>
    public static async Task<Memory<byte>> DeflateAsync(Memory<byte> data) {
        var outputStream = new MemoryStream();
        using var zLibStream = new ZlibStream(outputStream, CompressionMode.Compress, CompressionLevel.Default, true);

        var mutableStream = new MemoryStream(data.ToArray());
        await mutableStream.CopyToAsync(zLibStream);

        zLibStream.Close();

        return new Memory<byte>(outputStream.ToArray());
    }

}
