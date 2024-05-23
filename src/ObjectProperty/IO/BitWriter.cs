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

using Imcodec.ObjectProperty.PropertyClass.Types;
using System.Text;

namespace Imcodec.IO;

public class BitWriter : BitManipulator {

    public readonly BinaryWriter Writer;

    /// <summary>
    /// Initializes a new instance of the BitWriter class with an empty memory stream.
    /// </summary>
    public BitWriter() {
        base.Stream = new MemoryStream();
        Writer = new BinaryWriter(Stream);
    }

    /// <summary>
    /// Initializes a new instance of the BitWriter class with an existing stream.
    /// </summary>
    /// <param name="existingStream">The existing stream to use.</param>
    public BitWriter(Stream existingStream) {
        base.Stream = existingStream;
        Writer = new BinaryWriter(Stream);
    }

    /// <summary>
    /// Initializes a new instance of the BitWriter class with the specified byte array.
    /// </summary>
    /// <param name="data">The byte array to use.</param>
    public BitWriter(byte[] data) {
        base.Stream = new MemoryStream(data);
        Writer = new BinaryWriter(Stream);
    }

    /// <summary>
    /// Writes a single bit to the stream. The bits will not be flushed prior.
    /// </summary>
    /// <param name="bit">The bit to write to the stream.</param>
    public void WriteBit(bool bit) {
        WriteBit(bit ? (byte) 1 : (byte) 0);
    }

    /// <summary>
    /// Writes a single bit to the stream. The bits will not be flushed prior.
    /// </summary>
    /// <param name="bit">The bit to write to the stream.</param>
    public void WriteBit(byte bit) {
        --base.BitPosition;

        if (bit == 1) {
            base.BitValue |= (byte) (1 << base.BitPosition);
        }
        if (base.BitPosition == 0) {
            FlushBits();
        }
    }

    /// <summary>
    /// Writes a certain amount of bits to the stream, represented as a given type T.
    /// </summary>
    /// <param name="bit">The bit to be written to the stream, represented as a given type T.</param>
    /// <param name="count">The length of how many bits will be written to the stream.</param>
    /// <typeparam name="T">The given type.</typeparam>
    public unsafe void WriteBits<T>(T bit, int count) where T : unmanaged {
        var ptr = (byte*) &bit;

        for (int i = 0; i < count; i++) {
            if (i % 8 == 0 && i != 0) {
                ptr++;
            }
            WriteBit((byte) ((*ptr >> i % 8) & 1));
        }
    }

    private void FlushBits() {
        if (Writer is null) {
            throw new NullReferenceException($"Cannot flush bits. {nameof(Writer)} is null.");
        }
        else if (base.BitPosition == 8) {
            return;
        }

        Writer.Write(Reverse(base.BitValue));
        base.BitValue = 0;
        base.BitPosition = 8;
    }

}
