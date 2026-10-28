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

internal sealed class LeaBlockCipher : IBlockDecryptor {

    private const int ROUNDS = 24;
    private const int KEY_SIZE = 16;
    private const int ROUND_KEY_WORDS = 6;

    private static readonly uint[] DELTA = [
        0xc3efe9db, 0x44626b02, 0x79e27c8a, 0x78df30ec,
        0x715ea49e, 0xc785da0a, 0xe04ef22a, 0xe5c40957,
    ];

    private readonly uint[] _roundKeys = new uint[ROUNDS * ROUND_KEY_WORDS];

    public int BlockSize => 16;

    public LeaBlockCipher(ReadOnlySpan<byte> key) {
        if (key.Length != KEY_SIZE) {
            throw new ArgumentException("Only 128-bit keys are supported.", nameof(key));
        }

        Span<uint> t = stackalloc uint[4];
        for (var i = 0; i < 4; i++) {
            t[i] = BinaryPrimitives.ReadUInt32LittleEndian(key[(i * 4)..]);
        }

        for (var i = 0; i < ROUNDS; i++) {
            var delta = DELTA[i & 3];
            t[0] = BitOperations.RotateLeft(t[0] + BitOperations.RotateLeft(delta, i), 1);
            t[1] = BitOperations.RotateLeft(t[1] + BitOperations.RotateLeft(delta, i + 1), 3);
            t[2] = BitOperations.RotateLeft(t[2] + BitOperations.RotateLeft(delta, i + 2), 6);
            t[3] = BitOperations.RotateLeft(t[3] + BitOperations.RotateLeft(delta, i + 3), 11);

            var rk = i * ROUND_KEY_WORDS;
            _roundKeys[rk] = t[0];
            _roundKeys[rk + 1] = t[1];
            _roundKeys[rk + 2] = t[2];
            _roundKeys[rk + 3] = t[1];
            _roundKeys[rk + 4] = t[3];
            _roundKeys[rk + 5] = t[1];
        }
    }

    public void EncryptBlock(ReadOnlySpan<byte> input, Span<byte> output) {
        var x0 = BinaryPrimitives.ReadUInt32LittleEndian(input);
        var x1 = BinaryPrimitives.ReadUInt32LittleEndian(input[4..]);
        var x2 = BinaryPrimitives.ReadUInt32LittleEndian(input[8..]);
        var x3 = BinaryPrimitives.ReadUInt32LittleEndian(input[12..]);

        for (var i = 0; i < ROUNDS; i++) {
            var rk = i * ROUND_KEY_WORDS;
            var n0 = BitOperations.RotateLeft((x0 ^ _roundKeys[rk]) + (x1 ^ _roundKeys[rk + 1]), 9);
            var n1 = BitOperations.RotateRight((x1 ^ _roundKeys[rk + 2]) + (x2 ^ _roundKeys[rk + 3]), 5);
            var n2 = BitOperations.RotateRight((x2 ^ _roundKeys[rk + 4]) + (x3 ^ _roundKeys[rk + 5]), 3);
            (x0, x1, x2, x3) = (n0, n1, n2, x0);
        }

        BinaryPrimitives.WriteUInt32LittleEndian(output, x0);
        BinaryPrimitives.WriteUInt32LittleEndian(output[4..], x1);
        BinaryPrimitives.WriteUInt32LittleEndian(output[8..], x2);
        BinaryPrimitives.WriteUInt32LittleEndian(output[12..], x3);
    }

    public void DecryptBlock(ReadOnlySpan<byte> input, Span<byte> output) {
        var y0 = BinaryPrimitives.ReadUInt32LittleEndian(input);
        var y1 = BinaryPrimitives.ReadUInt32LittleEndian(input[4..]);
        var y2 = BinaryPrimitives.ReadUInt32LittleEndian(input[8..]);
        var y3 = BinaryPrimitives.ReadUInt32LittleEndian(input[12..]);

        for (var i = ROUNDS - 1; i >= 0; i--) {
            var rk = i * ROUND_KEY_WORDS;
            var x0 = y3;
            var x1 = (BitOperations.RotateRight(y0, 9) - (x0 ^ _roundKeys[rk])) ^ _roundKeys[rk + 1];
            var x2 = (BitOperations.RotateLeft(y1, 5) - (x1 ^ _roundKeys[rk + 2])) ^ _roundKeys[rk + 3];
            var x3 = (BitOperations.RotateLeft(y2, 3) - (x2 ^ _roundKeys[rk + 4])) ^ _roundKeys[rk + 5];
            (y0, y1, y2, y3) = (x0, x1, x2, x3);
        }

        BinaryPrimitives.WriteUInt32LittleEndian(output, y0);
        BinaryPrimitives.WriteUInt32LittleEndian(output[4..], y1);
        BinaryPrimitives.WriteUInt32LittleEndian(output[8..], y2);
        BinaryPrimitives.WriteUInt32LittleEndian(output[12..], y3);
    }

}
