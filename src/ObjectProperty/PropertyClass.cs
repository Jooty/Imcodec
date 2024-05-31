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

/// <summary>
/// Defines class capable of undergoing binary serialization.
/// </summary>
public abstract class PropertyClass {

    public abstract uint GetHash();

    protected List<IProperty> Properties { get; } = [];

    /// <summary>
    /// Called before encoding the object to the binary stream.
    /// </summary>
    public virtual void OnPreEncode() { }

    /// <summary>
    /// Called after encoding the object to the binary stream.
    /// </summary>
    public virtual void OnPostEncode() { }

    /// <summary>
    /// Called before decoding the object from the binary stream.
    /// </summary>
    public virtual void OnPreDecode() { }

    /// <summary>
    /// Called after decoding the object from the binary stream.
    /// </summary>
    public virtual void OnPostDecode() { }

    /// <summary>
    /// Encodes the object properties using the specified <see cref="BitWriter"/> and <see cref="ObjectSerializer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="BitWriter"/> used to write the encoded data.</param>
    /// <param name="serializer">The <see cref="ObjectSerializer"/> used to serialize the object properties.</param>
    /// <returns><c>true</c> if the encoding is successful; otherwise, <c>false</c>.</returns>
    internal bool Encode(BitWriter writer, ObjectSerializer serializer) {
        OnPreEncode();

        writer.WriteUInt32(GetHash());

        if (serializer.Versionable) {
            return EncodeVersionable(writer, serializer);
        }

        foreach (var property in Properties) {
            if (!IsPropertyEligibleForProcessing(property, serializer)) {
                continue;
            }

            var encodeSuccess = property.Encode(writer, serializer);
            if (!encodeSuccess) {
                return false;
            }
        }

        OnPostEncode();
        return true;
    }

    /// <summary>
    /// Decodes the object properties using the specified <see cref="BitReader"/> and <see cref="ObjectSerializer"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BitReader"/> used for decoding.</param>
    /// <param name="serializer">The <see cref="ObjectSerializer"/> used for decoding.</param>
    /// <returns><c>true</c> if the decoding is successful for all properties; otherwise, <c>false</c>.</returns>
    internal bool Decode(BitReader reader, ObjectSerializer serializer) {
        OnPreDecode();

        // We don't want to read the hash here, since the caller should have already read it.
        // Otherwise, we wouldn't be here in the first place.

        if (serializer.Versionable) {
            return DecodeVersionable(reader, serializer);
        }

        foreach (var property in Properties) {
            if (!IsPropertyEligibleForProcessing(property, serializer)) {
                continue;
            }

            var decodeSuccess = property.Decode(reader, serializer);
            if (!decodeSuccess) {
                return false;
            }
        }

        OnPostDecode();
        return true;
    }

    private bool DecodeVersionable(BitReader reader, ObjectSerializer serializer) {
        // Properties may be out of order in the binary stream. Read the hash and size of the first property, which we
        // know is at the beginning of the stream. From there, we can tell how the properties are laid out.
        var propMap = Properties.ToDictionary(static p => p.Hash, p => p);
        var objectStart = reader.BitPos();
        var objectSize = reader.ReadUInt32();

        while (reader.BitPos() - objectStart < objectSize) {
            var propertyStart = reader.BitPos();
            var propertySize = reader.ReadUInt32();
            var propertyHash = reader.ReadUInt32();

            if (!propMap.TryGetValue(propertyHash, out var property)) {
                return false;
            }

            // Decode the property.
            if (!property.Decode(reader, serializer)) {
                return false;
            }

            // Ensure that the property size is correct.
            if (reader.BitPos() - propertyStart != propertySize) {
                return false;
            }

            // Seek bit to the end of this object.
            reader.SeekBit((int) (propertyStart + propertySize));
        }

        // Ensure that the object size is correct.
        if (reader.BitPos() - objectStart != objectSize) {
            return false;
        }

        // Seek bit to the end of this object.
        reader.SeekBit((int) (objectStart + objectSize));
        return true;
    }

    private bool EncodeVersionable(BitWriter writer, ObjectSerializer serializer) {
        writer.WriteUInt32(0); // Placeholder for the size.
        writer.WriteUInt32(GetHash());

        foreach (var property in Properties) {
            if (!IsPropertyEligibleForProcessing(property, serializer)) {
                continue;
            }

            // Write the hash and size of the property.
            writer.WriteUInt32(0); // Placeholder for the size.
            writer.WriteUInt32(property.Hash);

            var sizeBitPos = writer.BitPos();
            var encodeSuccess = property.Encode(writer, serializer);
            if (!encodeSuccess) {
                return false;
            }

            // Write the size of the property.
            var size = writer.BitPos() - sizeBitPos;
            writer.SeekBit(sizeBitPos);
            writer.WriteUInt32((uint)size);
            writer.SeekBit(sizeBitPos + size);
        }

        // Write the size of the object.
        var objectSize = writer.BitPos();
        writer.SeekBit(0);
        writer.WriteUInt32((uint)objectSize);
        writer.SeekBit(objectSize);
        return true;
    }

    private static bool IsPropertyEligibleForProcessing(IProperty property, ObjectSerializer serializer) {
        // Any property with the encoding flag set will always be encoded so long as the serializer requests it.
        var dirtyEncode = serializer.SerializerFlags.HasFlag(SerializerFlags.AlwaysEncode)
            && property.Flags.HasFlag(PropertyFlags.Prop_Encode);

        // Check if the property mask is met and if the property is deprecated.
        var propertyMaskMet = (property.Flags & serializer.PropertyMask) == property.Flags;
        var deprecated = property.Flags.HasFlag(PropertyFlags.Prop_Deprecated);

        // Skip properties that are not marked for serialization, or are deprecated and not dirty encoded.
        if (!propertyMaskMet || (deprecated && !dirtyEncode)) {
            return false;
        }

        return true;
    }

}
