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
/// A Bind serializer that serializes and deserializes
/// <see cref="PropertyClass"/> objects. The key distinction
/// between this and the <see cref="ObjectSerializer"/> is that
/// this serializer appends a magic header to the serialized
/// buffer. This is used for serializing a <see cref="PropertyClass"/>
/// object to a file.
/// <param name="Versionable">States whether the object is versionable.</param>
/// <param name="Behaviors">States the behaviors of the serializer.</param>
/// <param name="typeRegistry">The type registry to use for serialization.</param>
public class BindSerializer : ObjectSerializer {

    public BindSerializer() : base(true, SerializerFlags.None, null) { }

    /// <summary>
    /// The magic header for the BiND serializer.
    /// </summary>
    public const uint BiNDMagic = 0x644E4942;

    /// <summary>
    /// The default flags for the BiND serializer.
    /// </summary>
    public const uint BiNDDefaultFlags = 0x7;

    /// <summary>
    /// Serializes a <see cref="PropertyClass"/> object to a buffer.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="output">The buffer to write the serialized object to.</param>
    /// <returns>True if the object was successfully serialized, false otherwise.</returns>
    public bool Serialize<T>(T input, out byte[]? output) where T : PropertyClass
        => Serialize(input, (PropertyFlags) BiNDDefaultFlags, out output);

    /// <summary>
    /// Serializes a <see cref="PropertyClass"/> object to a buffer.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="input">The object to serialize.</param>
    /// <param name="propertyMask">The property mask to use for serialization.</param>
    /// <param name="output">The buffer to write the serialized object to.</param>
    /// <returns>True if the object was successfully serialized, false otherwise.</returns>
    public override bool Serialize(PropertyClass input, PropertyFlags propertyMask, out byte[]? output) {
        output = default;

        // First, call the base class serialize and check if it was successful.
        // We then want to suffix the buffer with the BiND magic.
        if (!base.Serialize(input, propertyMask, out var baseOutput)) {
            return false;
        }

        // Create a new buffer with the size of the output buffer plus the size of the magic and flags.
        var buffer = new byte[baseOutput!.Length + sizeof(uint) * 2];

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

    /// <summary>
    /// Deserializes a buffer to a <see cref="PropertyClass"/> object.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="inputBuffer">The buffer to deserialize.</param>
    /// <param name="output">The object to write the deserialized object to.</param>
    /// <returns>True if the buffer was successfully deserialized, false otherwise.</returns>
    public bool Deserialize<T>(byte[] inputBuffer, out T output) where T : PropertyClass
        => Deserialize(inputBuffer, (PropertyFlags) BiNDDefaultFlags, out output);

    /// <summary>
    /// Deserializes a buffer to a <see cref="PropertyClass"/> object.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="inputBuffer">The buffer to deserialize.</param>
    /// <param name="propertyMask">The property mask to use for deserialization.</param>
    /// <param name="output">The object to write the deserialized object to.</param>
    /// <returns>True if the buffer was successfully deserialized, false otherwise.</returns>
    public override bool Deserialize<T>(byte[] inputBuffer, PropertyFlags propertyMask, out T output) {
        output = default!;
        var bindHeaderLength = sizeof(uint) * 2; // Magic and flags.

        var reader = new BitReader(inputBuffer);

        // Check if the input buffer is too small to contain the magic header.
        if (inputBuffer.Length < bindHeaderLength) {
            return false;
        }

        // Read the magic header and check if it is correct.
        var magic = reader.ReadUInt32();
        var skipLen = 0;
        if (magic == BiNDMagic) {
            // If the BiND header is present, the serializer flags will be next.
            var flags = reader.ReadUInt32();
            if ((flags & 8) != 0) {
                _ = reader.ReadBit();
            }
            base.SerializerFlags = (SerializerFlags) flags;

            skipLen = bindHeaderLength;
        }

        var baseInput = inputBuffer.Skip(skipLen).ToArray();

        return base.Deserialize(baseInput, propertyMask, out output!);
    }

}