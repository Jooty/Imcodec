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

public class MessageStreamDecoderTests {

    [Fact]
    public void AcceptThenAttach_OneBuffer() {
        // Arrange
        var accept = new ControlMessageProtocol.SessionAccept {
            Reserved1 = 1,
            TimestampUpper = 2,
            TimestampLower = 3,
            Milliseconds = 4,
            SessionId = 5
        };
        var attach = CreateAttach();
        var decoder = new MessageStreamDecoder();

        // Act
        var messages = decoder.Feed(Concat(MessageEncoder.Encode(accept), MessageEncoder.Encode(attach)));

        // Assert
        Assert.Equal(2, messages.Count);
        Assert.Equal(0, decoder.PendingBytes);

        var decodedAccept = Assert.IsType<ControlMessageProtocol.SessionAccept>(messages[0]);
        Assert.Equal(accept.Reserved1, decodedAccept.Reserved1);
        Assert.Equal(accept.TimestampUpper, decodedAccept.TimestampUpper);
        Assert.Equal(accept.TimestampLower, decodedAccept.TimestampLower);
        Assert.Equal(accept.Milliseconds, decodedAccept.Milliseconds);
        Assert.Equal(accept.SessionId, decodedAccept.SessionId);

        var decodedAttach = Assert.IsType<GAME_5_PROTOCOL.MSG_ATTACH>(messages[1]);
        AssertAttachEqual(attach, decodedAttach);
    }

    [Fact]
    public void TwoFramesPlusPartialThird() {
        // Arrange
        var frame1 = MessageEncoder.Encode(new GAME_5_PROTOCOL.MSG_QUERY_LOGOUT { IsInstance = 1 });
        var frame2 = MessageEncoder.Encode(new WIZARD_12_PROTOCOL.MSG_PLAYERWIZBANG { StateName = "state" });
        var frame3 = MessageEncoder.Encode(CreateAttach());
        var decoder = new MessageStreamDecoder();

        // Act
        var first = decoder.Feed(Concat(frame1, frame2, frame3[..10]));
        var pendingAfterFirst = decoder.PendingBytes;
        var second = decoder.Feed(frame3[10..]);

        // Assert
        Assert.Equal(2, first.Count);
        Assert.IsType<GAME_5_PROTOCOL.MSG_QUERY_LOGOUT>(first[0]);
        Assert.IsType<WIZARD_12_PROTOCOL.MSG_PLAYERWIZBANG>(first[1]);
        Assert.Equal(10, pendingAfterFirst);

        var attach = Assert.Single(second);
        AssertAttachEqual(CreateAttach(), Assert.IsType<GAME_5_PROTOCOL.MSG_ATTACH>(attach));
        Assert.Equal(0, decoder.PendingBytes);
    }

    [Fact]
    public void ByteAtATime() {
        // Arrange
        var stream = Concat(
            MessageEncoder.Encode(new GAME_5_PROTOCOL.MSG_QUERY_LOGOUT { IsInstance = 1 }),
            MessageEncoder.Encode(new ControlMessageProtocol.KeepAlive { SessionId = 7 }),
            MessageEncoder.Encode(CreateAttach()));
        var decoder = new MessageStreamDecoder();
        var messages = new List<IMessage>();

        // Act
        foreach (var value in stream) {
            messages.AddRange(decoder.Feed([value]));
        }

        // Assert
        Assert.Equal(3, messages.Count);
        Assert.IsType<GAME_5_PROTOCOL.MSG_QUERY_LOGOUT>(messages[0]);
        Assert.Equal(7, Assert.IsType<ControlMessageProtocol.KeepAlive>(messages[1]).SessionId);
        AssertAttachEqual(CreateAttach(), Assert.IsType<GAME_5_PROTOCOL.MSG_ATTACH>(messages[2]));
        Assert.Equal(0, decoder.PendingBytes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(-1)]
    public void SplitPoints(int split) {
        // Arrange
        var frame = MessageEncoder.Encode(CreateAttach());
        var splitAt = split < 0 ? frame.Length - 1 : split;
        var decoder = new MessageStreamDecoder();

        // Act
        var head = decoder.Feed(frame[..splitAt]);
        var tail = decoder.Feed(frame[splitAt..]);

        // Assert
        Assert.Empty(head);
        var message = Assert.Single(tail);
        AssertAttachEqual(CreateAttach(), Assert.IsType<GAME_5_PROTOCOL.MSG_ATTACH>(message));
        Assert.Equal(0, decoder.PendingBytes);
    }

    [Fact]
    public void LogoutPair() {
        // Arrange
        var bang = new WIZARD_12_PROTOCOL.MSG_PLAYERWIZBANG { StateName = "logout" };
        var logout = new GAME_5_PROTOCOL.MSG_QUERY_LOGOUT { IsInstance = 1 };
        var decoder = new MessageStreamDecoder();

        // Act
        var messages = decoder.Feed(Concat(MessageEncoder.Encode(bang), MessageEncoder.Encode(logout)));

        // Assert
        Assert.Equal(2, messages.Count);
        Assert.Equal("logout", (string) Assert.IsType<WIZARD_12_PROTOCOL.MSG_PLAYERWIZBANG>(messages[0]).StateName);
        Assert.Equal(1, Assert.IsType<GAME_5_PROTOCOL.MSG_QUERY_LOGOUT>(messages[1]).IsInstance);
    }

    [Fact]
    public void LoginPair() {
        // Arrange
        var accept = new ControlMessageProtocol.SessionAccept { SessionId = 9 };
        var validate = new LOGIN_7_PROTOCOL.MSG_USER_VALIDATE {
            UserID = 123456789UL,
            PassKey3 = "passkey",
            MachineID = 987654321UL,
            Locale = "English",
            PatchClientID = "patch",
            PlatformChatID = "chat"
        };
        var decoder = new MessageStreamDecoder();

        // Act
        var messages = decoder.Feed(Concat(MessageEncoder.Encode(accept), MessageEncoder.Encode(validate)));

        // Assert
        Assert.Equal(2, messages.Count);
        Assert.Equal(9, Assert.IsType<ControlMessageProtocol.SessionAccept>(messages[0]).SessionId);

        var decodedValidate = Assert.IsType<LOGIN_7_PROTOCOL.MSG_USER_VALIDATE>(messages[1]);
        Assert.Equal(validate.UserID, decodedValidate.UserID);
        Assert.Equal((string) validate.PassKey3, (string) decodedValidate.PassKey3);
        Assert.Equal(validate.MachineID, decodedValidate.MachineID);
        Assert.Equal((string) validate.Locale, (string) decodedValidate.Locale);
    }

    [Theory]
    [InlineData(new byte[] { 0x0E, 0xF0, 0x05, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00 })]
    [InlineData(new byte[] { 0x0D, 0x00, 0x05, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00 })]
    [InlineData(new byte[] { 0x41 })]
    public void BadMagic_NoFrameFound_SkipsAllBytes(byte[] junk) {
        // Arrange
        var decoder = new MessageStreamDecoder();

        // Act
        var skippedMessages = decoder.Feed(junk);
        var skippedBytes = decoder.SkippedBytes;
        var messages = decoder.Feed(MessageEncoder.Encode(new GAME_5_PROTOCOL.MSG_QUERY_LOGOUT { IsInstance = 1 }));

        // Assert
        Assert.Empty(skippedMessages);
        Assert.Equal(junk.Length, skippedBytes);
        Assert.Single(messages);
        Assert.Equal(0, decoder.SkippedBytes);
        Assert.Equal(0, decoder.PendingBytes);
    }

    [Fact]
    public void GarbageThenValidFrame_FrameIsDecoded() {
        // Arrange
        var garbage = new byte[] { 0x41, 0x42, 0x0D, 0x00, 0x99 };
        var decoder = new MessageStreamDecoder();

        // Act
        var messages = decoder.Feed(Concat(garbage, MessageEncoder.Encode(CreateAttach())));

        // Assert
        var message = Assert.Single(messages);
        AssertAttachEqual(CreateAttach(), Assert.IsType<GAME_5_PROTOCOL.MSG_ATTACH>(message));
        Assert.Equal(garbage.Length, decoder.SkippedBytes);
        Assert.Equal(0, decoder.PendingBytes);
    }

    [Fact]
    public void GarbageBetweenFrames_BothFramesAreDecoded() {
        // Arrange
        var garbage = new byte[] { 0x01, 0x02, 0x03 };
        var decoder = new MessageStreamDecoder();

        // Act
        var messages = decoder.Feed(Concat(
            MessageEncoder.Encode(new GAME_5_PROTOCOL.MSG_QUERY_LOGOUT { IsInstance = 1 }),
            garbage,
            MessageEncoder.Encode(CreateAttach())));

        // Assert
        Assert.Equal(2, messages.Count);
        Assert.IsType<GAME_5_PROTOCOL.MSG_QUERY_LOGOUT>(messages[0]);
        Assert.IsType<GAME_5_PROTOCOL.MSG_ATTACH>(messages[1]);
        Assert.Equal(garbage.Length, decoder.SkippedBytes);
    }

    [Fact]
    public void GarbageEndingInMagicStart_KeepsItForTheNextRead() {
        // Arrange
        var garbage = new byte[] { 0x41, 0x42 };
        var frame = MessageEncoder.Encode(CreateAttach());
        var decoder = new MessageStreamDecoder();

        // Act
        var head = decoder.Feed(Concat(garbage, frame[..1]));
        var pendingAfterHead = decoder.PendingBytes;
        var skippedAfterHead = decoder.SkippedBytes;
        var tail = decoder.Feed(frame[1..]);

        // Assert
        Assert.Empty(head);
        Assert.Equal(1, pendingAfterHead);
        Assert.Equal(garbage.Length, skippedAfterHead);
        var message = Assert.Single(tail);
        AssertAttachEqual(CreateAttach(), Assert.IsType<GAME_5_PROTOCOL.MSG_ATTACH>(message));
    }

    [Fact]
    public void MagicWithTooShortLength_SkipsToNextFrame() {
        // Arrange
        // A magic header whose length field cannot hold a frame header, followed by a valid frame.
        var falseHeader = new byte[] { 0x0D, 0xF0, 0x02, 0x00 };
        var decoder = new MessageStreamDecoder();

        // Act
        var messages = decoder.Feed(Concat(falseHeader, MessageEncoder.Encode(CreateAttach())));

        // Assert
        Assert.IsType<GAME_5_PROTOCOL.MSG_ATTACH>(Assert.Single(messages));
        Assert.Equal(falseHeader.Length, decoder.SkippedBytes);
        Assert.Equal(0, decoder.PendingBytes);
    }

    [Fact]
    public void TryGetFrameLength_ShortAndControlForms() {
        // Arrange
        var control = MessageEncoder.Encode(new ControlMessageProtocol.KeepAlive { SessionId = 1 });
        var game = MessageEncoder.Encode(CreateAttach());

        // Act / Assert
        Assert.False(MessageStreamDecoder.TryGetFrameLength(control[..3], out _));
        Assert.True(MessageStreamDecoder.TryGetFrameLength(control[..4], out var controlLength));
        Assert.Equal(control.Length, controlLength);
        Assert.True(MessageStreamDecoder.TryGetFrameLength(game, out var gameLength));
        Assert.Equal(game.Length, gameLength);
    }

    [Fact]
    public void TryGetFrameLength_LongForm() {
        // Arrange
        // Magic, 0x8000 marker, 32-bit body length (0x00010000), then the control flag.
        var nonControl = new byte[] { 0x0D, 0xF0, 0x00, 0x80, 0x00, 0x00, 0x01, 0x00, 0x00 };
        var control = new byte[] { 0x0D, 0xF0, 0x00, 0x80, 0x00, 0x00, 0x01, 0x00, 0x01 };

        // Act / Assert
        Assert.False(MessageStreamDecoder.TryGetFrameLength(nonControl[..8], out _));
        Assert.True(MessageStreamDecoder.TryGetFrameLength(nonControl, out var nonControlLength));
        Assert.Equal(12 + 4 + 0x10000 + 1, nonControlLength);
        Assert.True(MessageStreamDecoder.TryGetFrameLength(control, out var controlLength));
        Assert.Equal(12 + 0x10000 + 1, controlLength);
    }

    [Fact]
    public void LongForm_RoundTrip() {
        // Arrange
        var state = new string('x', 40000);
        var frame = MessageEncoder.Encode(new WIZARD_12_PROTOCOL.MSG_PLAYERWIZBANG { StateName = state });
        var logout = MessageEncoder.Encode(new GAME_5_PROTOCOL.MSG_QUERY_LOGOUT { IsInstance = 1 });
        var decoder = new MessageStreamDecoder();

        // Act
        Assert.True(MessageStreamDecoder.TryGetFrameLength(frame, out var frameLength));
        var head = decoder.Feed(frame[..20000]);
        var tail = decoder.Feed(Concat(frame[20000..], logout));

        // Assert
        Assert.Equal(frame.Length, frameLength);
        Assert.Empty(head);
        Assert.Equal(2, tail.Count);
        Assert.Equal(state, (string) Assert.IsType<WIZARD_12_PROTOCOL.MSG_PLAYERWIZBANG>(tail[0]).StateName);
        Assert.IsType<GAME_5_PROTOCOL.MSG_QUERY_LOGOUT>(tail[1]);
    }

    [Fact]
    public void LongerMessageBody_NextMessageStillDecodes() {
        // Arrange
        // MSG_QUERY_LOGOUT carries one body byte; this record has three extra ones the message does not read.
        var longerRecord = Record(serviceId: 5, messageId: 164, body: [1, 0xAA, 0xBB, 0xCC]);
        var secondRecord = Record(serviceId: 5, messageId: 164, body: [0]);
        var frame = SquashedFrame(longerRecord, secondRecord);

        // Act
        var viaEncoder = MessageEncoder.Decode(frame);
        var viaDecoder = new MessageStreamDecoder().Feed(frame);

        // Assert
        foreach (var messages in new IReadOnlyCollection<IMessage>?[] { viaEncoder, viaDecoder }) {
            Assert.NotNull(messages);
            Assert.Equal(2, messages.Count);
            Assert.Equal(1, Assert.IsType<GAME_5_PROTOCOL.MSG_QUERY_LOGOUT>(messages.ElementAt(0)).IsInstance);
            Assert.Equal(0, Assert.IsType<GAME_5_PROTOCOL.MSG_QUERY_LOGOUT>(messages.ElementAt(1)).IsInstance);
        }
    }

    [Fact]
    public void UndecodableFirst_SecondStillDecodes() {
        // Arrange
        // Service 200 does not exist; the second record is a valid MSG_QUERY_LOGOUT.
        var unknownRecord = Record(serviceId: 200, messageId: 1, body: [1, 2, 3, 4, 5]);
        var validRecord = Record(serviceId: 5, messageId: 164, body: [1]);
        var frame = SquashedFrame(unknownRecord, validRecord);
        var failures = new List<(byte ServiceId, byte MessageId)>();

        // Act
        var messages = new MessageStreamDecoder((serviceId, messageId, _) => failures.Add((serviceId, messageId)))
            .Feed(frame);

        // Assert
        var message = Assert.Single(messages);
        Assert.Equal(1, Assert.IsType<GAME_5_PROTOCOL.MSG_QUERY_LOGOUT>(message).IsInstance);
        Assert.Equal([((byte) 200, (byte) 1)], failures);
    }

    [Fact]
    public void UnknownControlOpcode_NextFrameStillDecodes() {
        // Arrange
        var control = new byte[] { 0x0D, 0xF0, 0x05, 0x00, 0x01, 99, 0x00, 0x00, 0x00 };
        var logout = MessageEncoder.Encode(new GAME_5_PROTOCOL.MSG_QUERY_LOGOUT { IsInstance = 1 });
        var failures = new List<(byte ServiceId, byte MessageId)>();

        // Act
        var messages = new MessageStreamDecoder((serviceId, messageId, _) => failures.Add((serviceId, messageId)))
            .Feed(Concat(control, logout));

        // Assert
        Assert.IsType<GAME_5_PROTOCOL.MSG_QUERY_LOGOUT>(Assert.Single(messages));
        Assert.Equal([((byte) 0, (byte) 99)], failures);
    }

    private static GAME_5_PROTOCOL.MSG_ATTACH CreateAttach() => new() {
        UserID = 1001,
        CharID = 2002,
        ZoneName = "WizardCity/WC_Ravenwood",
        Location = "Spawn",
        PassKey = "pass",
        Locale = "English",
        LoginKey = "",
        PlatformGamerTag = ""
    };

    private static void AssertAttachEqual(GAME_5_PROTOCOL.MSG_ATTACH expected, GAME_5_PROTOCOL.MSG_ATTACH actual) {
        Assert.Equal(expected.UserID, actual.UserID);
        Assert.Equal(expected.CharID, actual.CharID);
        Assert.Equal((string) expected.ZoneName, (string) actual.ZoneName);
        Assert.Equal((string) expected.Location, (string) actual.Location);
        Assert.Equal((string) expected.PassKey, (string) actual.PassKey);
        Assert.Equal((string) expected.Locale, (string) actual.Locale);
    }

    private static byte[] Concat(params byte[][] parts) => [.. parts.SelectMany(part => part)];

    // A message layer record: service id, message id, length (including these four bytes), body.
    private static byte[] Record(byte serviceId, byte messageId, byte[] body) {
        var length = (ushort) (body.Length + 4);

        return [serviceId, messageId, (byte) (length & 0xFF), (byte) (length >> 8), .. body];
    }

    // A non-control frame holding several records: magic, length field, flag/opcode/padding, records, trailing null.
    private static byte[] SquashedFrame(params byte[][] records) {
        var recordBytes = Concat(records);
        var lengthField = (ushort) (recordBytes.Length + 5);

        return [0x0D, 0xF0, (byte) (lengthField & 0xFF), (byte) (lengthField >> 8), 0, 0, 0, 0, .. recordBytes, 0];
    }

}
