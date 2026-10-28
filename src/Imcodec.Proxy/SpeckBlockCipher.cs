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
using System.Numerics;

namespace Imcodec.Proxy;

internal sealed class SpeckBlockCipher : IBlockDecryptor {

    private const int ROUNDS = 26;
    private const int KEY_SIZE = 12;
    private const int KEY_WORDS = 3;

    private readonly uint[] _roundKeys = new uint[ROUNDS];

    public int BlockSize => 8;

    public SpeckBlockCipher(ReadOnlySpan<byte> key) {
        if (key.Length != KEY_SIZE) {
            throw new ArgumentException("Only 96-bit keys are supported.", nameof(key));
        }

        var schedule = new uint[ROUNDS + KEY_WORDS - 2];
        var k = BinaryPrimitives.ReadUInt32LittleEndian(key);
        for (var i = 0; i < KEY_WORDS - 1; i++) {
            schedule[i] = BinaryPrimitives.ReadUInt32LittleEndian(key[((i + 1) * 4)..]);
        }

        for (var i = 0; i < ROUNDS; i++) {
            _roundKeys[i] = k;
            if (i == ROUNDS - 1) {
                break;
            }
            schedule[i + KEY_WORDS - 1] = (k + BitOperations.RotateRight(schedule[i], 8)) ^ (uint) i;
            k = BitOperations.RotateLeft(k, 3) ^ schedule[i + KEY_WORDS - 1];
        }
    }

    public void EncryptBlock(ReadOnlySpan<byte> input, Span<byte> output) {
        var y = BinaryPrimitives.ReadUInt32LittleEndian(input);
        var x = BinaryPrimitives.ReadUInt32LittleEndian(input[4..]);

        for (var i = 0; i < ROUNDS; i++) {
            x = (BitOperations.RotateRight(x, 8) + y) ^ _roundKeys[i];
            y = BitOperations.RotateLeft(y, 3) ^ x;
        }

        BinaryPrimitives.WriteUInt32LittleEndian(output, y);
        BinaryPrimitives.WriteUInt32LittleEndian(output[4..], x);
    }

    public void DecryptBlock(ReadOnlySpan<byte> input, Span<byte> output) {
        var y = BinaryPrimitives.ReadUInt32LittleEndian(input);
        var x = BinaryPrimitives.ReadUInt32LittleEndian(input[4..]);

        for (var i = ROUNDS - 1; i >= 0; i--) {
            y = BitOperations.RotateRight(y ^ x, 3);
            x = BitOperations.RotateLeft((x ^ _roundKeys[i]) - y, 8);
        }

        BinaryPrimitives.WriteUInt32LittleEndian(output, y);
        BinaryPrimitives.WriteUInt32LittleEndian(output[4..], x);
    }

}
