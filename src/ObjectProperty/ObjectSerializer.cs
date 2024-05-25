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

using Imcodec.IO;

namespace Imcodec.ObjectProperty;

[Flags]
public enum SerializerFlags {

    None,
    UseFlags      = 1 << 0, // States the serializer should use these flags for deserialization.
    CompactLength = 1 << 1, // Length prefixes are compacted into smaller data types whenever possible.
    StringEnums   = 1 << 2, // Some enums are made into strings.
    Compress      = 1 << 3, // Use ZLib compression.
    AlwaysEncode  = 1 << 4, // Always serialize properties with bitflag `8`.

}

/// <summary>
/// States the behaviors of the serializer.
/// </summary>
/// <param name="Versionable">If true, <see cref="Property"/> hashes and sizes
/// are stored in the binary stream. Otherwise, the data is serialized in order of declaration.</param>
/// <param name="Behaviors">States the behaviors of the serializer.</param>
public class ObjectSerializer(bool Versionable = true, SerializerFlags Behaviors = SerializerFlags.None) {

    /// <summary>
    /// States whether the object is versionable. If true, <see cref="Property"/> hashes and sizes
    /// are stored in the binary stream. Otherwise, the data is serialized in order of declaration.
    /// </summary>
    public bool Versionable { get; set; } = Versionable;

    /// <summary>
    /// States the behaviors of the serializer.
    /// </summary>
    public SerializerFlags Behaviors { get; set; } = Behaviors;

    public bool Deserialize<T>(byte[] inputBuffer, PropertyFlags propertyMask, out T output) where T : PropertyClass {
        output = default;
        var reader = new BitReader(inputBuffer);

        // If the behaviors flag is set to use compression, decompress the input buffer.
        if (Behaviors.HasFlag(SerializerFlags.Compress)) {
            reader = Decompress(reader);
            if (reader == null) {
                return false;
            }
        }

        var propertyClass = DeserializeInternal(reader, propertyMask);
        if (propertyClass == null) {
            return false;
        }

        output = (T)propertyClass;
        return true;
    }

    /// <summary>
    /// Decompresses the data using the specified <see cref="BitReader"/>.
    /// </summary>
    /// <param name="inputBuffer">The <see cref="BitReader"/> containing the compressed data.</param>
    /// <returns>A <see cref="BitReader"/> containing the decompressed data.</returns>
    protected virtual BitReader? Decompress(BitReader inputBuffer) {
        var uncompressedLength = inputBuffer.Reader.ReadInt32();
        var decompressedData = Compression.Decompress(inputBuffer.GetData()[4..]);

        // If the decompressed data length does not match the recorded length, log it and return null.
        if (decompressedData.Length != uncompressedLength) {
            throw new Exception("Decompressed data length does not match the recorded length.");
        }

        return new BitReader(decompressedData);
    }

    /// <summary>
    /// Preloads an object from the input buffer based on the provided hash value.
    /// </summary>
    /// <param name="inputBuffer">The input buffer containing the serialized data.</param>
    /// <param name="propertyClass">The loaded property class, if found.</param>
    /// <returns><c>true</c> if the object was preloaded successfully; otherwise, <c>false</c>.</returns>
    protected virtual bool PreloadObject(BitReader inputBuffer, out PropertyClass? propertyClass) {
        propertyClass = null;
        var hash = inputBuffer.Reader.ReadUInt32();
        if (hash == 0) {
            return false;
        }

        propertyClass = CodeGen.Generation.Dispatch(hash);
        return propertyClass != null;
    }

    private PropertyClass? DeserializeInternal(BitReader inputBuffer, PropertyFlags propertyMask) {
        if (!PreloadObject(inputBuffer, out var propertyClass)) {
            return null;
        }

        propertyClass?.OnPreDecode();
        if (Versionable) {
            propertyClass?.DecodeVersionable(inputBuffer, Behaviors, propertyMask);
        }
        else {
            propertyClass?.Decode(inputBuffer, Behaviors, propertyMask);
        }
        propertyClass?.OnPostDecode();

        return propertyClass;
    }

}
