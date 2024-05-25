/*
BSD 3-Clause License

Copyright (c) 2024, Revive101

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

namespace Imcodec.IO;

/// <summary>
/// Wrapper around a BinaryReader that allows reading individual bits.
/// </summary>
public class BitReader : BitManipulator {

    public readonly BinaryReader Reader;

    /// <summary>
    /// Initializes a new instance of the BitReader class with an existing stream.
    /// </summary>
    /// <param name="existingStream">The existing stream to use.</param>
    public BitReader(Stream existingStream) {
        base.Stream = existingStream;
        Reader = new BinaryReader(Stream);
    }

    /// <summary>
    /// Initializes a new instance of the BitReader class with the specified byte array.
    /// </summary>
    /// <param name="data">The byte array to use.</param>
    public BitReader(byte[] data) {
        base.Stream = new MemoryStream(data);
        Reader = new BinaryReader(Stream);
    }

    /// <summary>
    /// Reads a single bit from the stream. Will not reset the bit position, unless it is over 8.
    /// </summary>
    /// <returns>A boolean flag denoting the value of the bit that was read.</returns>
    public bool ReadBit() {
        if (base.BitPosition == 8) {
            try {
                base.BitValue = Reverse(Reader.ReadByte());
            }
            catch (EndOfStreamException) {
                base.BitValue = 0;
            }
            base.BitPosition = 0;
        }

        int returnValue = base.BitValue;
        base.BitValue <<= 1;
        base.BitPosition++;

        return (returnValue & 0x80) != 0;
    }

    /// <summary>
    /// Reads a certain amount of bits.
    /// </summary>
    /// <param name="bitCount">The amount of bits to read.</param>
    /// <typeparam name="T">The type that will be used to represent the bits read.</typeparam>
    /// <returns>The declared type T.</returns>
    public unsafe T ReadBits<T>(int bitCount) where T : unmanaged {
        //if (sizeof(T) * 8 >= bitCount) {
        //    throw new ArgumentException("The type T is too large to hold the amount of bits specified.");
        //}

        var obj = new T();
        var ptr = (byte*) &obj;

        for (int i = 0; i < bitCount; i++) {
            if (i % 8 == 0 && i != 0) {
                ptr++;
            }

            if (ReadBit()) {
                *ptr |= (byte) (1 << (i % 8));
            }
        }

        return obj;
    }

}
