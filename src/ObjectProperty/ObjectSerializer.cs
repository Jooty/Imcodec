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
/// <param name="propertyMask">The property flags to use for serialization.</param>
public class ObjectSerializer(bool Versionable = true,
                              SerializerFlags Behaviors = SerializerFlags.None) {

    /// <summary>
    /// States whether the object is versionable. If true, <see cref="Property"/> hashes and sizes
    /// are stored in the binary stream. Otherwise, the data is serialized in order of declaration.
    /// </summary>
    public bool Versionable { get; set; } = Versionable;

    /// <summary>
    /// States the behaviors of the serializer.
    /// </summary>
    public SerializerFlags SerializerFlags { get; set; } = Behaviors;

    /// <summary>
    /// The property flags to use for serialization.
    /// </summary>
    public PropertyFlags PropertyMask { get; set; } = PropertyFlags.Prop_Transmit | PropertyFlags.Prop_AuthorityTransmit;

    /// <summary>
    /// Deserializes the input buffer into an instance of type T. The property mask is set to <see cref="PropertyMask"/>,
    /// which is the default or the last set property mask.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="inputBuffer">The input buffer containing the serialized data.</param>
    /// <param name="output">The deserialized object of type T.</param>
    /// <returns>True if deserialization is successful; otherwise, false.</returns>
    public bool Deserialize<T>(byte[] inputBuffer, out T output) where T : PropertyClass
        => Deserialize(inputBuffer, PropertyMask, out output);

    /// <summary>
    /// Deserializes the input buffer into an instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="inputBuffer">The input buffer containing the serialized data.</param>
    /// <param name=""propertyMask"">The property flags to use for serialization.</param>
    /// <param name="output">When this method returns, contains the deserialized object of type <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the deserialization is successful; otherwise, <c>false</c>.</returns>
    public bool Deserialize<T>(byte[] inputBuffer, PropertyFlags propertyMask, out T output) where T : PropertyClass {
        output = default;
        this.PropertyMask = propertyMask;
        var reader = new BitReader(inputBuffer);

        // If the behaviors flag is set to use compression, decompress the input buffer.
        if (SerializerFlags.HasFlag(SerializerFlags.Compress)) {
            reader = Decompress(reader);
            if (reader == null) {
                return false;
            }
        }

        var propertyClass = DeserializeInternal(reader);
        if (propertyClass == null) {
            return false;
        }

        output = (T)propertyClass;
        return true;
    }

    /// <summary>
    /// Compresses the data using the specified <see cref="BitWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="BitWriter"/> containing the data to compress.</param>
    /// <returns>A <see cref="BitWriter"/> containing the compressed data.</returns>
    protected virtual BitWriter Compress(BitWriter writer) {
        var uncompressedSize = writer.GetData().Length;
        var compressedData = Compression.Compress(writer.GetData());

        var bufferSize = compressedData.Length + 4;
        var tempBuffer = new byte[bufferSize];

        using var memoryStream = new MemoryStream(tempBuffer);
        using var binaryWriter = new BinaryWriter(memoryStream);

        binaryWriter.Write(uncompressedSize);
        binaryWriter.Write(compressedData);

        return new BitWriter(tempBuffer);
    }

    /// <summary>
    /// Decompresses the data using the specified <see cref="BitReader"/>.
    /// </summary>
    /// <param name="inputBuffer">Th    /// ee cref="BitReader"/> containing the compressed data.</param>
    /// <returns>A <see cref="BitReader"/> containing the decompressed data.</returns>
    protected virtual BitReader? Decompress(BitReader inputBuffer) {
        var uncompressedLength = inputBuffer.ReadInt32();
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
        var hash = inputBuffer.ReadUInt32();
        if (hash == 0) {
            return false;
        }

        propertyClass = CodeGen.Generation.Dispatch(hash);
        return propertyClass != null;
    }

    private PropertyClass? DeserializeInternal(BitReader inputBuffer) {
        if (!PreloadObject(inputBuffer, out var propertyClass)) {
            return null;
        }

        propertyClass?.OnPreDecode();
        propertyClass?.Decode(inputBuffer, this);
        propertyClass?.OnPostDecode();

        return propertyClass;
    }

}
