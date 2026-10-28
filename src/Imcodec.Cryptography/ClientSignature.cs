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

namespace Imcodec.Cryptography;

/// <summary>
/// The client's code signature dump and the answer to the session's tamper challenge computed from it.
/// </summary>
public sealed class ClientSignature {

    public const byte CHALLENGE_TYPE = 0xF1;
    public const int CHALLENGE_PARAMETERS_SIZE = 12;

    private const uint CONTROL_DISABLE = 1u << 5;
    private const uint CONTROL_INCLUDE_OFFSETS = 1u << 9;
    private const uint CONTROL_INCLUDE_MODULES = 1u << 12;
    private const uint CONTROL_OBFUSCATE = 1u << 17;
    private const uint FNV_PRIME = 0x01000193;
    private const uint PJW_HIGH_NIBBLE_MASK = 0xF0000000;

    private readonly byte[] _offsets;
    private readonly byte[] _modules;
    private readonly byte[] _code;

    /// <summary>
    /// Parses a decrypted dump made of three length-prefixed sections: offsets, modules and code.
    /// </summary>
    /// <exception cref="InvalidDataException">A section runs past the end of the dump.</exception>
    public ClientSignature(ReadOnlySpan<byte> dump) {
        var position = 0;
        _offsets = ReadSection(dump, ref position);
        _modules = ReadSection(dump, ref position);
        _code = ReadSection(dump, ref position);
    }

    /// <summary>
    /// Computes the answer from the challenge parameters of a session offer: control mask, spec and key,
    /// each a little-endian 32-bit value.
    /// </summary>
    public uint AnswerChallenge(ReadOnlySpan<byte> challengeParameters) {
        if (challengeParameters.Length < CHALLENGE_PARAMETERS_SIZE) {
            throw new ArgumentException("Too few bytes to perform the challenge.", nameof(challengeParameters));
        }

        return AnswerChallenge(
            BinaryPrimitives.ReadUInt32LittleEndian(challengeParameters),
            BinaryPrimitives.ReadUInt32LittleEndian(challengeParameters[4..]),
            BinaryPrimitives.ReadUInt32LittleEndian(challengeParameters[8..]));
    }

    public uint AnswerChallenge(uint controlMask, uint spec, uint key) {
        if ((controlMask & CONTROL_DISABLE) != 0) {
            return 0;
        }

        var buffer = BuildSignatureBuffer(controlMask, key);

        if ((controlMask & CONTROL_OBFUSCATE) != 0) {
            Span<byte> maskBytes = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(maskBytes, controlMask);
            for (var i = 0; i < buffer.Length; i++) {
                buffer[i] ^= maskBytes[i & 3];
            }
        }

        var result = spec >> 8;
        var chunkSize = (int) ((spec & 0x3C) >> 2) + 1;
        var rounds = (int) ((spec & 0xC0) >> 6) + 1;
        for (var round = 0; round < rounds; round++) {
            for (var offset = 0; offset < chunkSize; offset++) {
                for (var index = offset; index < buffer.Length; index += chunkSize) {
                    result = HashRound(spec & 0b11, result, buffer[index]);
                }
            }
        }
        return result;
    }

    private static byte[] ReadSection(ReadOnlySpan<byte> dump, ref int position) {
        if (dump.Length - position < sizeof(uint)) {
            throw new InvalidDataException("The signature dump ends before a section length.");
        }

        var length = BinaryPrimitives.ReadUInt32LittleEndian(dump[position..]);
        position += sizeof(uint);
        if (length > dump.Length - position) {
            throw new InvalidDataException("A signature dump section runs past the end of the dump.");
        }

        var section = dump.Slice(position, (int) length).ToArray();
        position += (int) length;
        return section;
    }

    private byte[] BuildSignatureBuffer(uint controlMask, uint key) {
        var result = new List<byte>();
        if ((controlMask & CONTROL_INCLUDE_OFFSETS) != 0) {
            result.AddRange(Scramble(_offsets, key));
        }
        if ((controlMask & CONTROL_INCLUDE_MODULES) != 0) {
            result.AddRange(Scramble(_modules, key));
        }
        result.AddRange(Scramble(_code, key));
        return result.ToArray();
    }

    private static List<byte> Scramble(byte[] data, uint key) {
        Span<byte> keyBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(keyBytes, key);

        var step = (int) (
            ((key & (1u << 3)) >> 3)
            | ((key & (1u << 5)) >> 4)
            | ((key & (1u << 7)) >> 5)
            | ((key & (1u << 14)) >> 11)
            | ((key & (1u << 18)) >> 14));

        var scrambled = new List<byte>(data.Length + data.Length / 2);
        foreach (var b in data) {
            if (step != 0 && scrambled.Count != 0 && scrambled.Count % step == 0) {
                scrambled.Add(scrambled[^1]);
            }
            scrambled.Add((byte) (keyBytes[scrambled.Count & 3] ^ b));
        }
        return scrambled;
    }

    private static uint HashRound(uint algorithm, uint accumulator, byte value) => algorithm switch {
        0 => (accumulator ^ value) * FNV_PRIME,
        1 => (accumulator * FNV_PRIME) ^ value,
        2 => JenkinsOneAtATimeRound(accumulator, value),
        _ => PjwRound(accumulator, value),
    };

    private static uint JenkinsOneAtATimeRound(uint accumulator, byte value) {
        accumulator += value;
        accumulator += accumulator << 10;
        return accumulator ^ (accumulator >> 6);
    }

    private static uint PjwRound(uint accumulator, byte value) {
        accumulator = (accumulator << 4) + value;
        var high = accumulator & PJW_HIGH_NIBBLE_MASK;
        if (high != 0) {
            accumulator ^= high >> 24;
        }
        return accumulator & ~high;
    }
}
