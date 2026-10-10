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

using System.Buffers.Binary;

namespace Imcodec.MessageLayer;

/// <summary>
/// Reassembles whole network frames from the chunks a stream socket delivers and decodes them.
/// One read can hold several frames or only part of one; this keeps the partial tail for the next read.
/// </summary>
/// <remarks>
/// Instances hold per-connection state and are not thread safe.
/// </remarks>
public sealed class MessageStreamDecoder {

    private const byte MagicHeaderFirstByte = 0x0D;
    private const byte MagicHeaderSecondByte = 0xF0;
    private const int FrameLengthFieldOffset = 2;
    private const int FrameLengthFieldSize = 2;

    // Magic (2) + length field (2) + control flag, opcode and padding (4) + trailing null (1).
    private const int MinimumFrameLength = 9;
    private const int LongFormHeaderLength = 12;
    private const int MessageLayerHeaderLength = 4;
    private const int LongFormControlFlagOffset = 8;
    private const int InitialBufferSize = 512;
    private static readonly byte[] s_magicHeader = [MagicHeaderFirstByte, MagicHeaderSecondByte];

    private readonly Action<byte, byte, Exception>? _onDecodeFailure;
    private byte[] _buffer = new byte[InitialBufferSize];
    private int _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageStreamDecoder"/> class.
    /// </summary>
    /// <param name="onDecodeFailure">
    /// Called with the service id, message id and exception of a message that was skipped because it could not be decoded.
    /// </param>
    public MessageStreamDecoder(Action<byte, byte, Exception>? onDecodeFailure = null) {
        _onDecodeFailure = onDecodeFailure;
    }

    /// <summary>
    /// Gets the number of buffered bytes that do not yet form a complete frame.
    /// </summary>
    public int PendingBytes => _count;

    /// <summary>
    /// Gets the number of bytes the most recent <see cref="Feed"/> discarded while looking for the next frame start.
    /// </summary>
    public int SkippedBytes { get; private set; }

    /// <summary>
    /// Reads the total length of the frame at the start of <paramref name="data"/>.
    /// </summary>
    /// <param name="data">The bytes of the frame, starting at its magic header.</param>
    /// <param name="frameLength">The total frame length in bytes, including the header and the trailing null.</param>
    /// <returns>False if more bytes are needed to know the length.</returns>
    public static bool TryGetFrameLength(ReadOnlySpan<byte> data, out int frameLength) {
        frameLength = 0;
        if (data.Length < FrameLengthFieldOffset + FrameLengthFieldSize) {
            return false;
        }

        var lengthField = BinaryPrimitives.ReadUInt16LittleEndian(data[FrameLengthFieldOffset..]);
        if (lengthField != MessageEncoder.LongPacketMarker) {
            frameLength = lengthField + FrameLengthFieldOffset + FrameLengthFieldSize;

            return true;
        }

        // The long form moves the control flag behind a 32-bit body length.
        if (data.Length <= LongFormControlFlagOffset) {
            return false;
        }

        var bodyLength = BinaryPrimitives.ReadUInt32LittleEndian(data[(FrameLengthFieldOffset + FrameLengthFieldSize)..]);
        var isControl = data[LongFormControlFlagOffset] == 1;
        var longFrameLength = LongFormHeaderLength + (isControl ? 0 : MessageLayerHeaderLength) + bodyLength + 1L;
        frameLength = (int) System.Math.Min(longFrameLength, int.MaxValue);

        return true;
    }

    /// <summary>
    /// Adds received bytes and decodes every frame that is now complete.
    /// </summary>
    /// <remarks>
    /// Bytes that do not start a frame are skipped up to the next magic header, see <see cref="SkippedBytes"/>.
    /// A trailing magic header byte is kept in case the rest arrives with the next read.
    /// </remarks>
    /// <param name="data">The bytes of one socket read.</param>
    /// <returns>The messages of all completed frames, in the order they were sent.</returns>
    public IReadOnlyList<IMessage> Feed(ReadOnlySpan<byte> data) {
        Append(data);
        SkippedBytes = 0;

        var messages = new List<IMessage>();
        var consumed = 0;
        while (consumed < _count) {
            var window = new ReadOnlySpan<byte>(_buffer, consumed, _count - consumed);
            if (!HasMagicHeaderPrefix(window)) {
                consumed += SkipToNextFrameStart(window, searchFrom: 0);

                continue;
            }

            if (!TryGetFrameLength(window, out var frameLength)) {
                break;
            }

            if (frameLength < MinimumFrameLength) {
                // The magic header was a false hit, so look for the next one behind it.
                consumed += SkipToNextFrameStart(window, searchFrom: 1);

                continue;
            }

            if (window.Length < frameLength) {
                break;
            }

            DecodeFrame(window[..frameLength], messages);
            consumed += frameLength;
        }

        if (consumed > 0) {
            Array.Copy(_buffer, consumed, _buffer, 0, _count - consumed);
            _count -= consumed;
        }

        return messages;
    }

    private int SkipToNextFrameStart(ReadOnlySpan<byte> window, int searchFrom) {
        var found = window[searchFrom..].IndexOf(s_magicHeader);
        var endsWithMagicStart = window[^1] == MagicHeaderFirstByte;

        // Without a hit, a trailing first magic byte stays: its second byte may come with the next read.
        var skipped = found >= 0 ? searchFrom + found
            : endsWithMagicStart ? window.Length - 1
            : window.Length;
        SkippedBytes += skipped;

        return skipped;
    }

    private void Append(ReadOnlySpan<byte> data) {
        var required = _count + data.Length;
        if (required > _buffer.Length) {
            Array.Resize(ref _buffer, System.Math.Max(required, _buffer.Length * 2));
        }

        data.CopyTo(_buffer.AsSpan(_count));
        _count = required;
    }

    private static bool HasMagicHeaderPrefix(ReadOnlySpan<byte> window)
        => window[0] == MagicHeaderFirstByte && (window.Length < 2 || window[1] == MagicHeaderSecondByte);

    private void DecodeFrame(ReadOnlySpan<byte> frame, List<IMessage> messages) {
        try {
            var decoded = MessageEncoder.Decode(frame.ToArray(), _onDecodeFailure);
            if (decoded is not null) {
                messages.AddRange(decoded);
            }
        }
        catch (Exception ex) {
            var (serviceId, messageId) = PeekMessageIds(frame);
            _onDecodeFailure?.Invoke(serviceId, messageId, ex);
        }
    }

    private static (byte serviceId, byte messageId) PeekMessageIds(ReadOnlySpan<byte> frame) {
        // Offset of the control flag, which the long form pushes back by the 32-bit body length.
        var flagOffset = BinaryPrimitives.ReadUInt16LittleEndian(frame[FrameLengthFieldOffset..]) == MessageEncoder.LongPacketMarker
            ? LongFormControlFlagOffset
            : FrameLengthFieldOffset + FrameLengthFieldSize;
        if (frame.Length <= flagOffset + 1) {
            return (0, 0);
        }

        if (frame[flagOffset] == 1) {
            return (0, frame[flagOffset + 1]);
        }

        var messageHeaderOffset = flagOffset + 4;

        return frame.Length > messageHeaderOffset + 1
            ? (frame[messageHeaderOffset], frame[messageHeaderOffset + 1])
            : ((byte) 0, (byte) 0);
    }

}
