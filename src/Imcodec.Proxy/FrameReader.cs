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

using Imcodec.Cryptography;
using Imcodec.MessageLayer;

namespace Imcodec.Proxy;

/// <summary>
/// Cuts one direction of a session's byte stream into whole frames, decrypting it first once the
/// session's AES stream has started.
/// </summary>
/// <remarks>
/// Instances hold per-connection state and are not thread safe.
/// </remarks>
internal sealed class FrameReader {

    private const int READ_BUFFER_SIZE = 16 * 1024;
    private const int MINIMUM_FRAME_LENGTH = 9;
    private const int CHUNK_TRAILER_SIZE = AesStreamCipher.TAG_SIZE + AesStreamCipher.NONCE_SIZE;

    private readonly Stream _stream;
    private readonly byte[] _readBuffer = new byte[READ_BUFFER_SIZE];
    private byte[] _plain = new byte[READ_BUFFER_SIZE];
    private int _plainCount;
    private byte[] _pendingCiphertext = [];
    private AesStreamCipher? _decryptor;
    private int _chunkBytes;
    private int _chunkPeriodBytes;
    private int _offsetInChunkPeriod;

    public FrameReader(Stream stream) {
        _stream = stream;
    }

    /// <summary>
    /// Treats every byte after the frames read so far as ciphertext of the given AES stream.
    /// </summary>
    /// <param name="decryptor">The cipher of the side that sends this direction.</param>
    /// <param name="chunkBlocks">The chunk length of that side, in blocks.</param>
    public void StartDecrypting(AesStreamCipher decryptor, int chunkBlocks) {
        _decryptor = decryptor;
        _chunkBytes = chunkBlocks * AesStreamCipher.BLOCK_SIZE;
        _chunkPeriodBytes = _chunkBytes + CHUNK_TRAILER_SIZE;
        _offsetInChunkPeriod = 0;
        _pendingCiphertext = _plain.AsSpan(0, _plainCount).ToArray();
        _plainCount = 0;
        DecryptPending();
    }

    /// <summary>
    /// Reads the next whole frame, with its header and trailing null.
    /// </summary>
    /// <returns>The frame, or null once the stream has ended.</returns>
    /// <exception cref="InvalidDataException">The stream does not hold a frame where one has to start.</exception>
    public async ValueTask<byte[]?> ReadFrameAsync(CancellationToken cancellationToken) {
        while (true) {
            if (TryTakeFrame(out var frame)) {
                return frame;
            }

            var read = await _stream.ReadAsync(_readBuffer, cancellationToken);
            if (read == 0) {
                return null;
            }
            Accept(_readBuffer.AsSpan(0, read));
        }
    }

    private bool TryTakeFrame(out byte[] frame) {
        frame = [];
        var window = _plain.AsSpan(0, _plainCount);
        if (!MessageStreamDecoder.TryGetFrameLength(window, out var frameLength)) {
            return false;
        }
        if (frameLength < MINIMUM_FRAME_LENGTH) {
            throw new InvalidDataException("The stream does not hold a frame where one has to start.");
        }
        if (window.Length < frameLength) {
            return false;
        }

        frame = window[..frameLength].ToArray();
        window[frameLength..].CopyTo(_plain);
        _plainCount -= frameLength;
        return true;
    }

    private void Accept(ReadOnlySpan<byte> data) {
        if (_decryptor is null) {
            AppendPlain(data);
            return;
        }

        var pending = new byte[_pendingCiphertext.Length + data.Length];
        _pendingCiphertext.CopyTo(pending, 0);
        data.CopyTo(pending.AsSpan(_pendingCiphertext.Length));
        _pendingCiphertext = pending;
        DecryptPending();
    }

    private void DecryptPending() {
        var available = _pendingCiphertext.Length;
        var endInChunkPeriod = (int) ((_offsetInChunkPeriod + (long) available) % _chunkPeriodBytes);

        // The cipher needs a chunk's whole tag and nonce in the same call that ends the chunk, so a
        // read that stops at or inside a trailer keeps everything from the chunk's last byte back.
        var feedable = endInChunkPeriod < _chunkBytes
            ? available
            : available - (endInChunkPeriod - _chunkBytes) - 1;
        if (feedable <= 0) {
            return;
        }

        AppendPlain(_decryptor!.Decrypt(_pendingCiphertext.AsSpan(0, feedable)));
        _offsetInChunkPeriod = (int) ((_offsetInChunkPeriod + (long) feedable) % _chunkPeriodBytes);
        _pendingCiphertext = _pendingCiphertext[feedable..];
    }

    private void AppendPlain(ReadOnlySpan<byte> data) {
        var required = _plainCount + data.Length;
        if (required > _plain.Length) {
            Array.Resize(ref _plain, System.Math.Max(required, _plain.Length * 2));
        }

        data.CopyTo(_plain.AsSpan(_plainCount));
        _plainCount = required;
    }

}
