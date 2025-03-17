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

using Imcodec.MessageLayer;
using Imcodec.MessageLayer.Generated;

namespace Imcodec.Test.MessageLayerTest;

public class MessageEncoderTests {

    [Fact]
    public void RoundTrip_EncodeDecode_ReturnsSameMessage() {
        // Arrange
        var originalMessage = new CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPSSPELLCAST {
            SpellTemplateID = 12345,
            IsTreasureCard = 1
        };

        // Act
        var encoded = MessageEncoder.Encode(originalMessage);
        var decoded = MessageEncoder.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Single(decoded);

        var decodedMessage = decoded.First() as CANTRIPSMESSAGES_57_PROTOCOL.MSG_CANTRIPSSPELLCAST;
        Assert.NotNull(decodedMessage);
        Assert.Equal(originalMessage.SpellTemplateID, decodedMessage.SpellTemplateID);
        Assert.Equal(originalMessage.IsTreasureCard, decodedMessage.IsTreasureCard);
    }

    [Fact]
    public void RoundTrip_EncodeDecodeControlMessage_ReturnsSameMessage() {
        // Arrange
        var originalMessage = new ControlMessageProtocol.KeepAlive {
            SessionId = 12345,
        };

        // Act
        var encoded = MessageEncoder.Encode(originalMessage);
        var decoded = MessageEncoder.Decode(encoded);

        // Assert
        Assert.NotNull(decoded);
        Assert.Single(decoded);

        var decodedMessage = decoded.First() as ControlMessageProtocol.KeepAlive;
        Assert.NotNull(decodedMessage);
        Assert.Equal(originalMessage.SessionId, decodedMessage.SessionId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new byte[0])]
    public void Decode_EmptyOrNullBuffer_ReturnsNull(byte[] buffer) {
        // Act
        var result = MessageEncoder.Decode(buffer);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Decode_InvalidMagicHeader_ReturnsNull() {
        // Arrange
         // Invalid magic header (0x0EF0 instead of 0x0DF0).
        var buffer = new byte[] { 0x0E, 0xF0, 0x05, 0x00, 0x01, 0x01, 0x00, 0x00 };

        // Act
        var result = MessageEncoder.Decode(buffer);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Decode_SampleBuffer_ReturnsCorrectMessage() {
        // Arrange
        // Convert hex string "0D F0 11 00 00 00 00 00 05 24 0C 00 01 00 02 00 03 00 04 00 00" to byte array.
        var buffer = new byte[] {
            0x0D, 0xF0, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x05, 0x24, 0x0C, 0x00, 0x01, 0x00, 0x02, 0x00,
            0x03, 0x00, 0x04, 0x00, 0x00
        };

        // Act
        var result = MessageEncoder.Decode(buffer);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        var message = result.First() as GAME_5_PROTOCOL.MSG_CLIENTMOVE;
        Assert.NotNull(message);
        Assert.Equal(1, message.LocationX);
        Assert.Equal(2, message.LocationY);
        Assert.Equal(3, message.LocationZ);
        Assert.Equal(4, message.Direction);
    }

    [Fact]
    public void Encode_SampleBuffer_ReturnsCorrectByteArray() {
        // Arrange
        var buffer = new byte[] {
            0x0D, 0xF0, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x05, 0x24, 0x0C, 0x00, 0x01, 0x00, 0x02, 0x00,
            0x03, 0x00, 0x04, 0x00, 0x00
        };

        var message = new GAME_5_PROTOCOL.MSG_CLIENTMOVE {
            LocationX = 1,
            LocationY = 2,
            LocationZ = 3,
            Direction = 4
        };

        // Act
        var result = MessageEncoder.Encode(message);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(buffer, result);
    }

}
