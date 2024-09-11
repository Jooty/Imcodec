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
using System.IO.Compression;

namespace Imcodec.Test.Wad;

public class ZLibCompressionServiceTest {

    [Fact]
    public void Inflate_ValidCompressedData_ReturnsDecompressedData() {
        // Arrange
        byte[] originalData = [1, 2, 3, 4, 5];
        byte[] compressedData;
        var expectedSize = originalData.Length;

        using (var outputStream = new MemoryStream()) {
            using (var zlibStream = new DeflateStream(outputStream, CompressionMode.Compress)) {
                zlibStream.Write(originalData, 0, originalData.Length);
            }
            compressedData = outputStream.ToArray();
        }

        // Act
        var decompressedData = ZLibCompressionService.Inflate(compressedData, expectedSize);

        // Assert
        Assert.Equal(originalData, decompressedData.ToArray());
    }

}
