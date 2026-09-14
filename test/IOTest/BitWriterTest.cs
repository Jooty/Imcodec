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

namespace Imcodec.Test.IOTest;

public class BitWriterTest {

    [Fact]
    public void GetData_TrailingBit_IsNotDropped() {
        // Arrange. Mirrors the serializer flags header: a dword followed by a
        // single bit, with no byte-aligned write after it to force a flush.
        var writer = new BitWriter();

        // Act
        writer.WriteUInt32(9);
        writer.WriteBit(true);
        var data = writer.GetData();

        // Assert
        Assert.Equal(5, data.Length);
    }

    [Fact]
    public void GetData_TrailingPartialBits_AreNotDropped() {
        // Arrange
        var writer = new BitWriter();

        // Act
        writer.WriteUInt32(1);
        writer.WriteBits((byte) 0x05, 3);
        var data = writer.GetData();

        // Assert
        Assert.Equal(5, data.Length);
    }

    [Fact]
    public void GetData_BitFollowedByAlignedWrite_IsUnchanged() {
        // Arrange. Bits in the middle of a buffer were already flushed by the
        // next byte-aligned write; this guards against a double flush.
        var writer = new BitWriter();

        // Act
        writer.WriteBit(true);
        writer.WriteUInt32(1);
        var data = writer.GetData();

        // Assert
        Assert.Equal(5, data.Length);
    }

    [Fact]
    public void GetData_NoTrailingBits_IsUnchanged() {
        // Arrange
        var writer = new BitWriter();

        // Act
        writer.WriteUInt32(1);
        var data = writer.GetData();

        // Assert
        Assert.Equal(4, data.Length);
    }

    [Fact]
    public void FlagsHeader_RoundTrips_WithoutConsumingPayload() {
        // Arrange. The reader consumes a whole byte per ReadBit, so a dropped
        // flag byte shifts the entire payload by one.
        byte[] payload = [0xAA, 0xBB, 0xCC, 0xDD];
        var writer = new BitWriter();
        writer.WriteUInt32(9);
        writer.WriteBit(true);

        // Act
        var combined = writer.GetData().Concat(payload).ToArray();
        var reader = new BitReader(combined);
        var flags = reader.ReadUInt32();
        var bit = reader.ReadBit();
        var rest = reader.GetRelativeData();

        // Assert
        Assert.Equal(9u, flags);
        Assert.True(bit);
        Assert.Equal(payload, rest);
    }

    [Fact]
    public void TrailingBit_RoundTripsAsWritten() {
        // Arrange. A buffer whose last value is a bool: the reader swallows the
        // resulting EndOfStreamException and hands back false, so a dropped byte
        // corrupts the value silently rather than throwing.
        var writer = new BitWriter();
        writer.WriteUInt32(0xDEADBEEF);
        writer.WriteBit(true);

        // Act
        var reader = new BitReader(writer.GetData());
        var value = reader.ReadUInt32();
        var bit = reader.ReadBit();

        // Assert
        Assert.Equal(0xDEADBEEF, value);
        Assert.True(bit);
    }

}
