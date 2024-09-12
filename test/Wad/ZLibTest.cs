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

public class ZLibTest {

    [Fact]
    public void Deflate_ValidData_ReturnsCompressedData() {
        // Arrange
        byte[] originalData = [1, 2, 3, 4, 5];
        var data = new Memory<byte>(originalData);

        // Act
        var compressedData = ZLibUtility.Deflate(data);

        // Assert
        Assert.NotNull(compressedData);
        Assert.NotEqual(originalData.Length, compressedData.Length);
    }

    [Fact]
    public void DeflateAndInflate_ValidData_ReturnsOriginalData() {
        // Arrange
        byte[] originalData = [1, 2, 3, 4, 5];
        var data = new Memory<byte>(originalData);

        // Act
        var compressedData = ZLibUtility.Deflate(data);
        var decompressedData = ZLibUtility.Inflate(compressedData, originalData.Length);

        // Assert
        Assert.Equal(originalData, decompressedData.ToArray());
    }

}
