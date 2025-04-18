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
using System;
using System.Collections.Generic;

namespace Imcodec.MessageLayer;

public class ControlMessageProtocol : MessageProtocol {

    public byte ServiceId => 0;
    public string ProtocolType => "CONTROL";
    public int ProtocolVersion => 1;
    public string ProtocolDescription => "Responsible for general network session management";

    public IMessage Dispatch(byte messageId)
        => messageId switch {
            0 => new SessionOffer(),
            3 => new KeepAlive(),
            4 => new KeepAliveResponse(),
            5 => new SessionAccept(),
            _ => throw new NotImplementedException($"Message ID {messageId} not implemented.")
        };

    public class SessionOffer : IMessage {

        public byte MessageOrder => 0;
        public byte ServiceId => 0;
        public byte AccessLevel => 0;

        public ushort SessionId { get; set; }
        public int TimestampUpper { get; set; }
        public int TimestampLower { get; set; }
        public uint Milliseconds { get; set; }

        void IMessage.Encode(BitWriter writer) {
            writer.WriteUInt16(SessionId);
            writer.WriteInt32(TimestampUpper);
            writer.WriteInt32(TimestampLower);
            writer.WriteUInt32(Milliseconds);
        }

        void IMessage.Decode(BitReader reader) {
            SessionId = reader.ReadUInt16();
            TimestampUpper = reader.ReadInt32();
            TimestampLower = reader.ReadInt32();
            Milliseconds = reader.ReadUInt32();
        }

    }

    public class KeepAlive : IMessage {

        public byte MessageOrder => 3;
        public byte ServiceId => 0;
        public byte AccessLevel => 0;

        public ushort SessionId { get; set; }
        public ushort Milliseconds { get; set; }
        public ushort ElapsedSessionTime { get; set; }

        void IMessage.Encode(BitWriter writer) {
            writer.WriteUInt16(SessionId);
            writer.WriteUInt16(Milliseconds);
            writer.WriteUInt16(ElapsedSessionTime);
        }

        void IMessage.Decode(BitReader reader) {
            SessionId = reader.ReadUInt16();
            Milliseconds = reader.ReadUInt16();
            ElapsedSessionTime = reader.ReadUInt16();
        }

    }

    public class KeepAliveServer : IMessage {

        public byte MessageOrder => 3;
        public byte ServiceId => 0;
        public byte AccessLevel => 0;

        public ushort SessionId { get; set; }
        public uint Milliseconds { get; set; }

        void IMessage.Encode(BitWriter writer) {
            writer.WriteUInt16(SessionId);
            writer.WriteUInt32(Milliseconds);
        }

        void IMessage.Decode(BitReader reader) {
            SessionId = reader.ReadUInt16();
            Milliseconds = reader.ReadUInt32();
        }

    }

    public class KeepAliveResponse : IMessage {

        public byte MessageOrder => 4;
        public byte ServiceId => 0;
        public byte AccessLevel => 0;

        public ushort SessionId { get; set; }
        public ushort Milliseconds { get; set; }
        public ushort ElapsedSessionTime { get; set; }

        void IMessage.Encode(BitWriter writer) {
            writer.WriteUInt16(SessionId);
            writer.WriteUInt16(Milliseconds);
            writer.WriteUInt16(ElapsedSessionTime);
        }

        void IMessage.Decode(BitReader reader) {
            SessionId = reader.ReadUInt16();
            Milliseconds = reader.ReadUInt16();
            ElapsedSessionTime = reader.ReadUInt16();
        }

    }

    public class SessionAccept : IMessage {

        public byte MessageOrder => 5;
        public byte ServiceId => 0;
        public byte AccessLevel => 0;

        public ushort Reserved1 { get; set; }
        public int TimestampUpper { get; set; }
        public int TimestampLower { get; set; }
        public uint Milliseconds { get; set; }
        public ushort SessionId { get; set; }

        void IMessage.Encode(BitWriter writer) {
            writer.WriteUInt16(Reserved1);
            writer.WriteInt32(TimestampUpper);
            writer.WriteInt32(TimestampLower);
            writer.WriteUInt32(Milliseconds);
            writer.WriteUInt16(SessionId);
        }

        void IMessage.Decode(BitReader reader) {
            Reserved1 = reader.ReadUInt16();
            TimestampUpper = reader.ReadInt32();
            TimestampLower = reader.ReadInt32();
            Milliseconds = reader.ReadUInt32();
            SessionId = reader.ReadUInt16();
        }

    }

}