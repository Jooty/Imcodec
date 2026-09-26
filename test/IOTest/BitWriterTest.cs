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

    [Fact]
    public void CompactString_WritesCharacters() {
        // Arrange. A compact length is one bit (long or short) and 7 bits of length.
        var writer = new BitWriter();
        writer.WithCompactLengths();

        // Act
        writer.WriteString("Female");
        writer.WriteUInt8(0xAB);
        var data = writer.GetData();

        // Assert
        Assert.Equal("0C46656D616C65AB", Convert.ToHexString(data));
    }

    [Fact]
    public void CompactString_LengthContinuesBitRun_CharactersAreAligned() {
        // Arrange
        var writer = new BitWriter();
        writer.WithCompactLengths();

        // Act
        writer.WriteBits((byte) 0b101, 3);
        writer.WriteString("Human");
        var data = writer.GetData();

        var reader = new BitReader(data);
        reader.WithCompactLengths();
        var bits = reader.ReadBits<byte>(3);
        var value = reader.ReadString();

        // Assert
        Assert.Equal("550048756D616E", Convert.ToHexString(data));
        Assert.Equal(0b101, bits);
        Assert.Equal("Human", (string) value);
    }

    [Fact]
    public void CompactString_LongString_Uses31BitLength() {
        // Arrange. The client writes 128 characters and up as a set bit followed by 31 bits of length.
        var text = new string('a', 200);
        var writer = new BitWriter();
        writer.WithCompactLengths();

        // Act
        writer.WriteString(text);
        var data = writer.GetData();

        var reader = new BitReader(data);
        reader.WithCompactLengths();
        var value = reader.ReadString();

        // Assert
        Assert.Equal(204, data.Length);
        Assert.Equal("91010000", Convert.ToHexString(data, 0, 4));
        Assert.Equal(text, (string) value);
    }

    [Fact]
    public void CompactString_Empty_DoesNotAlignFollowingBits() {
        // Arrange. With no characters to write, the client never byte aligns after the length.
        var writer = new BitWriter();
        writer.WithCompactLengths();

        // Act
        writer.WriteBits((byte) 0b101, 3);
        writer.WriteString(string.Empty);
        writer.WriteBit(true);
        var data = writer.GetData();

        var reader = new BitReader(data);
        reader.WithCompactLengths();
        var bits = reader.ReadBits<byte>(3);
        var value = reader.ReadString();
        var bit = reader.ReadBit();

        // Assert
        Assert.Equal("0508", Convert.ToHexString(data));
        Assert.Equal(0b101, bits);
        Assert.Equal(string.Empty, (string) value);
        Assert.True(bit);
    }

    [Fact]
    public void CompactWString_WritesCharacters() {
        // Arrange. The length counts characters, not bytes.
        var writer = new BitWriter();
        writer.WithCompactLengths();

        // Act
        writer.WriteWString("Hi");
        var data = writer.GetData();

        var reader = new BitReader(data);
        reader.WithCompactLengths();
        var value = reader.ReadWString();

        // Assert
        Assert.Equal("0448006900", Convert.ToHexString(data));
        Assert.Equal("Hi", value);
    }

}
