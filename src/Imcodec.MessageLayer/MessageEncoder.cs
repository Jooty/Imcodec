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
using Imcodec.MessageLayer.Generated;

namespace Imcodec.MessageLayer;

/// <summary>
/// Provides functionality to encode and decode messages for network transmission.
/// </summary>
public static class MessageEncoder {

    private const ushort MagicHeader = 0xF00D;
    private const byte ControlMessageServiceId = 0;
    private const byte PacketHeaderLengthSmall = 5; // +1 from the trailing null byte
    private const byte PacketHeaderLengthLarge = 9; // +1 from the trailing null byte
    
    private const byte MessageLayerHeaderLength = 4;

    /// <summary>
    /// Encodes a message into a byte array for network transmission.
    /// </summary>
    /// <param name="message">The message to encode.</param>
    /// <returns>A byte array containing the encoded message.</returns>
    public static byte[] Encode(IMessage message) {
        var bodyData = EncodeMessageBody(message);
        var isControl = message.ServiceId == ControlMessageServiceId;
        var headerData = EncodePacketHeader(bodyData.Length, isControl, message.MessageOrder);

        if (isControl) {
            // If this is a control message, we can simply craft the whole packet and return.
            return EncodeFullPacket(headerData, bodyData);
        }
        else {
            // For non-control messages, we need a secondary header (Message Layer header).
            var secondaryHeaderData = EncodeMessageLayerHeader(message.ServiceId, message.MessageOrder, bodyData.Length);
            var fullHeaderData = new byte[headerData.Length + secondaryHeaderData.Length];

            // Write the header data to the full header buffer.
            Array.Copy(headerData, 0, fullHeaderData, 0, headerData.Length);
            Array.Copy(secondaryHeaderData, 0, fullHeaderData, headerData.Length, secondaryHeaderData.Length);

            return EncodeFullPacket(fullHeaderData, bodyData);
        }
    }

    /// <summary>
    /// Decodes a byte array into a collection of messages.
    /// </summary>
    /// <param name="buffer">The byte array to decode.</param>
    /// <returns>A collection of decoded messages if successful, otherwise null.</returns>
    public static IReadOnlyCollection<IMessage>? Decode(byte[] buffer) {
        if (buffer is null || buffer.Length <= 0) {
            return null;
        }

        var reader = new BitReader(buffer);

        // Verify the magic header.
        if (!DecodeMagicHeader(reader)) {
            return null;
        }

        var (packetLength, isControl, opCode) = DecodePacketHeader(reader);

        // Control messages are never squashed.
        if (isControl) {
            var controlProtocol = new ControlMessageProtocol();
            var decodedControlPacket = controlProtocol.Dispatch(opCode);
            decodedControlPacket?.Decode(reader);

            return decodedControlPacket is null
                ? null
                : new[] { decodedControlPacket };
        }

        // For non-control messages, we may have multiple messages in one packet.
        var messages = new List<IMessage>();
        while (reader.BitPos() < packetLength * 8) {
            var message = DecodeMessage(reader);
            if (message != null) {
                messages.Add(message);
            }
        }

        return messages;
    }

    private static byte[] EncodeMessageBody(IMessage message) {
        var writer = new BitWriter();
        message.Encode(writer);

        return writer.GetData();
    }

    private static byte[] EncodePacketHeader(int bodyDataLength, bool isControl, byte opCode) {
        var writer = new BitWriter();

        // Write the magic header.
        writer.WriteUInt16(MagicHeader);

        // Write large header or small header, depending on the size of the payload.
        var isLongPacket = bodyDataLength > 0x777F;
        var smallPacketLength = isControl
            ? bodyDataLength + PacketHeaderLengthSmall
            : bodyDataLength + PacketHeaderLengthLarge;

        writer.WriteUInt16((ushort) (isLongPacket ? 0x8000 : smallPacketLength));

        // For large packets, write the full length.
        if (isLongPacket) {
            writer.WriteUInt32((uint) bodyDataLength);
        }

        // Write body header.
        writer.WriteUInt8((byte) (isControl ? 1 : 0));      // isControl flag
        writer.WriteUInt8((byte) (isControl ? opCode : 0)); // opCode
        writer.WriteUInt16(0);                              // Padding

        return writer.GetData();
    }

    private static byte[] EncodeMessageLayerHeader(byte serviceId, byte messageOrder, int bodyDataLength) {
        var writer = new BitWriter();
        writer.WriteUInt8(serviceId);
        writer.WriteUInt8(messageOrder);

        // Write the length of the body including the message layer header.
        writer.WriteUInt16((ushort) (bodyDataLength + MessageLayerHeaderLength));

        return writer.GetData();
    }

    private static byte[] EncodeFullPacket(byte[] header, byte[] payload) {
        // Create a new byte array from all the buffers combined. The +1 is the trailing null byte..
        var packet = new byte[payload.Length + header.Length + 1];

        // Combine all the buffers into the new byte array.
        Array.Copy(header, 0, packet, 0, header.Length);
        Array.Copy(payload, 0, packet, header.Length, payload.Length);

        return packet;
    }

    private static bool DecodeMagicHeader(BitReader reader) {
        var header = reader.ReadUInt16();

        return header == MagicHeader;
    }

    private static (ushort packetLength, bool isControl, byte opCode) DecodePacketHeader(BitReader reader) {
        var packetLength = reader.ReadUInt16();
        var isControl = reader.ReadUInt8() == 1;
        var opCode = reader.ReadUInt8();
        _ = reader.ReadUInt16(); // Skip padding

        return (packetLength, isControl, opCode);
    }

    private static IMessage? DecodeMessage(BitReader reader) {
        var (serviceId, messageId, messageLength) = DecodeMessageLayerHeader(reader);

        try {
            var protocol = ProtocolList.Dispatch(serviceId);
            var messageTemplate = protocol.Dispatch(messageId);

            return DecodeMessageBody(reader, messageTemplate);
        }
        catch (NotSupportedException) {
            return null;
        }
    }

    private static (byte serviceId, byte messageId, ushort length) DecodeMessageLayerHeader(BitReader reader) {
        var serviceId = reader.ReadUInt8();
        var messageId = reader.ReadUInt8();
        var length = reader.ReadUInt16();

        return (serviceId, messageId, length);
    }

    private static IMessage? DecodeMessageBody(BitReader reader, IMessage message) {
        try {
            message.Decode(reader);
            return message;
        }
        catch {
            return null;
        }
    }

}