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
using System.Buffers.Binary;

namespace Imcodec.ObjectProperty;

/// <summary>
/// A serializer that prefixes the buffer with a magic header and serializer flags.
/// This is usually used for BiND buffers, which are serialized files found within the client.
/// todo: docs
/// </summary>
public class BindSerializer : ObjectSerializer {

    private const uint BiNDMagic = 0x644E4942;
    private const uint BiNDDefaultFlags = 0x7;

    public bool Serialize<T>(T input, out byte[]? output) where T : PropertyClass
        => Serialize(input, (PropertyFlags) BiNDDefaultFlags, out output);

    public override bool Serialize(PropertyClass input, PropertyFlags propertyMask, out byte[]? output) {
        output = default;

        // First, call the base class serialize and check if it was successful.
        // We then want to suffix the buffer with the BiND magic.
        if (!base.Serialize(input, propertyMask, out var baseOutput)) {
            return false;
        }

        // Create a new buffer with the size of the output buffer plus the size of the magic.
        var buffer = new byte[baseOutput!.Length + sizeof(uint)];

        // Write the magic header and serializer flags.
        BinaryPrimitives.WriteUInt32LittleEndian(buffer, BiNDMagic);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(sizeof(uint)), BiNDDefaultFlags);

        // Copy the output buffer to the new buffer.
        var skipLen = sizeof(uint) * 2;
        var bufferSpan = buffer.AsSpan(skipLen);
        baseOutput.CopyTo(bufferSpan);
        output = buffer;

        return true;
    }

    public bool Deserialize<T>(byte[] inputBuffer, out T output) where T : PropertyClass
        => Deserialize(inputBuffer, (PropertyFlags) BiNDDefaultFlags, out output);

    public override bool Deserialize<T>(byte[] inputBuffer, PropertyFlags propertyMask, out T output) {
        output = default!;

        var reader = new BitReader(inputBuffer);

        // Check if the input buffer is too small to contain the magic header.
        if (inputBuffer.Length < sizeof(uint)) {
            return false;
        }

        // Read the magic header and check if it is correct.
        var magic = reader.ReadUInt32();
        if (magic != BiNDMagic) {
            return false;
        }

        // If the BiND header is present, the serializer flags will be next.
        var flags = (SerializerFlags) reader.ReadUInt32();
        if ((((uint) flags) & 8) != 0) {
            _ = reader.ReadBit();
        }
        base.SerializerFlags = flags;

        // Call the base class deserialize and check if it was successful.
        var skipLen = sizeof(uint) * 2; // Skip the magic and flags.
        var baseInput = inputBuffer.Skip(skipLen).ToArray();
        if (!base.Deserialize<T>(baseInput, propertyMask, out var baseOutput)) {
            return false;
        }

        output = baseOutput!;

        return true;
    }

}
