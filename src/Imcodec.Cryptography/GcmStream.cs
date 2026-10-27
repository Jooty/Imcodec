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
using System.Security.Cryptography;

namespace Imcodec.Cryptography;

/// <summary>
/// Streaming AES-GCM with a 16-byte nonce. The keystream and authentication state carry over between
/// calls, which the one-shot <see cref="AesGcm"/> cannot do.
/// </summary>
internal sealed class GcmStream : IDisposable {

    private const int BLOCK_SIZE = 16;
    private static readonly UInt128 s_reductionPolynomial = new(0xE100000000000000UL, 0);

    private readonly Aes _aes;
    private readonly UInt128 _hashKey;
    private readonly byte[] _tagMask = new byte[BLOCK_SIZE];
    private readonly byte[] _counterBlock = new byte[BLOCK_SIZE];
    private readonly byte[] _keystream = new byte[BLOCK_SIZE];
    private readonly byte[] _pendingCipherBlock = new byte[BLOCK_SIZE];
    private int _keystreamUsed = BLOCK_SIZE;
    private int _pendingCipherLength;
    private UInt128 _hashState;
    private ulong _cipherByteCount;

    public GcmStream(byte[] key, ReadOnlySpan<byte> nonce) {
        if (nonce.Length != BLOCK_SIZE) {
            throw new ArgumentException("The nonce must be 16 bytes long.", nameof(nonce));
        }

        _aes = Aes.Create();
        _aes.Key = key;

        Span<byte> zero = stackalloc byte[BLOCK_SIZE];
        Span<byte> hashKeyBytes = stackalloc byte[BLOCK_SIZE];
        _aes.EncryptEcb(zero, hashKeyBytes, PaddingMode.None);
        _hashKey = BinaryPrimitives.ReadUInt128BigEndian(hashKeyBytes);

        // GCM derives the initial counter by hashing a nonce that is not 12 bytes long.
        foreach (var block in new[] { PadToBlock(nonce), LengthBlock(0, (ulong) nonce.Length * 8) }) {
            _hashState = Multiply(_hashState ^ BinaryPrimitives.ReadUInt128BigEndian(block), _hashKey);
        }

        BinaryPrimitives.WriteUInt128BigEndian(_counterBlock, _hashState);
        _aes.EncryptEcb(_counterBlock, _tagMask, PaddingMode.None);
        _hashState = 0;
    }

    public void Transform(ReadOnlySpan<byte> input, Span<byte> output, bool encrypting) {
        for (var i = 0; i < input.Length; i++) {
            if (_keystreamUsed == BLOCK_SIZE) {
                NextKeystreamBlock();
            }

            var cipherByte = encrypting ? (byte) (input[i] ^ _keystream[_keystreamUsed]) : input[i];
            output[i] = (byte) (input[i] ^ _keystream[_keystreamUsed]);
            _keystreamUsed++;
            AbsorbCipherByte(cipherByte);
        }
    }

    public byte[] FinishTag() {
        if (_pendingCipherLength > 0) {
            Array.Clear(_pendingCipherBlock, _pendingCipherLength, BLOCK_SIZE - _pendingCipherLength);
            AbsorbPendingBlock();
        }

        var lengths = LengthBlock(0, _cipherByteCount * 8);
        _hashState = Multiply(_hashState ^ BinaryPrimitives.ReadUInt128BigEndian(lengths), _hashKey);

        var tag = new byte[BLOCK_SIZE];
        BinaryPrimitives.WriteUInt128BigEndian(tag, _hashState);
        for (var i = 0; i < BLOCK_SIZE; i++) {
            tag[i] ^= _tagMask[i];
        }
        return tag;
    }

    public void Dispose() => _aes.Dispose();

    private void NextKeystreamBlock() {
        var counter = BinaryPrimitives.ReadUInt32BigEndian(_counterBlock.AsSpan(12)) + 1;
        BinaryPrimitives.WriteUInt32BigEndian(_counterBlock.AsSpan(12), counter);
        _aes.EncryptEcb(_counterBlock, _keystream, PaddingMode.None);
        _keystreamUsed = 0;
    }

    private void AbsorbCipherByte(byte cipherByte) {
        _pendingCipherBlock[_pendingCipherLength++] = cipherByte;
        _cipherByteCount++;
        if (_pendingCipherLength == BLOCK_SIZE) {
            AbsorbPendingBlock();
        }
    }

    private void AbsorbPendingBlock() {
        _hashState = Multiply(_hashState ^ BinaryPrimitives.ReadUInt128BigEndian(_pendingCipherBlock), _hashKey);
        _pendingCipherLength = 0;
    }

    private static byte[] PadToBlock(ReadOnlySpan<byte> data) {
        var padded = new byte[(data.Length + BLOCK_SIZE - 1) / BLOCK_SIZE * BLOCK_SIZE];
        data.CopyTo(padded);
        return padded;
    }

    private static byte[] LengthBlock(ulong associatedBits, ulong cipherBits) {
        var block = new byte[BLOCK_SIZE];
        BinaryPrimitives.WriteUInt64BigEndian(block, associatedBits);
        BinaryPrimitives.WriteUInt64BigEndian(block.AsSpan(8), cipherBits);
        return block;
    }

    private static UInt128 Multiply(UInt128 x, UInt128 y) {
        UInt128 product = 0;
        var shifted = y;
        for (var bit = 127; bit >= 0; bit--) {
            if (((x >> bit) & 1) != 0) {
                product ^= shifted;
            }
            shifted = (shifted & 1) != 0 ? (shifted >> 1) ^ s_reductionPolynomial : shifted >> 1;
        }
        return product;
    }
}
