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

using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Imcodec.ObjectProperty;

public static class Compression {

    private static readonly ThreadLocal<Deflater> s_cachedDeflater =
        new(static () => new Deflater(Deflater.BEST_COMPRESSION, false));

    /// <summary>
    /// Compresses the given byte array using the Deflate algorithm with
    /// the best compression level.
    /// </summary>
    /// <param name="_bytes">The byte array to compress.</param>
    /// <returns>The compressed byte array.</returns>
    public static byte[] Compress(byte[] bytes) {
        var deflater = s_cachedDeflater.Value!;
        deflater.Reset();

        using var outputStream = new MemoryStream(bytes.Length);
        using (var deflaterStream = new DeflaterOutputStream(outputStream, deflater, 1024)) {
            deflaterStream.IsStreamOwner = false; // Don't dispose the deflater when done.
            deflaterStream.Write(bytes, 0, bytes.Length);
            deflaterStream.Finish();
        }

        return outputStream.ToArray();
    }

    /// <summary>
    /// Decompresses a byte array using the InflaterInputStream class and
    /// returns the result as an IEnumerable of bytes.
    /// </summary>
    /// <param name="bytes">The byte array to decompress.</param>
    /// <returns>An IEnumerable of bytes representing the decompressed data.</returns>
    public static byte[] Decompress(byte[] bytes) {
        MemoryStream resStream = new();
        using (MemoryStream memoryStream = new(bytes)) {
            using InflaterInputStream inflater = new(memoryStream);
            inflater.CopyTo(resStream);
        }

        return resStream.ToArray();
    }

}
