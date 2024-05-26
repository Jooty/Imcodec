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

        foreach (var property in Properties) {
            var castedProperty = property as Property<object>
                ?? throw new InvalidOperationException($"Property {property.GetType().Name} is not a Property<object>");

            if (!ShouldEncodeProperty(castedProperty, serializer)) {
                continue;
            }

            var encodeSuccess = castedProperty.Encode(writer, serializer);
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

        foreach (var property in Properties) {
            var castedProperty = property as Property<object>
                ?? throw new InvalidOperationException($"Property {property.GetType()} is not a {typeof(Property<object>).Name}");

            if (!ShouldEncodeProperty(castedProperty, serializer)) {
                continue;
            }

            var decodeSuccess = castedProperty.Decode(reader, serializer);
            if (!decodeSuccess) {
                return false;
            }
        }

        OnPostDecode();
        return true;
    }

    private static bool ShouldEncodeProperty(Property<object> castedProperty, ObjectSerializer serializer) {
        // Any property with the encoding flag set will always be encoded so long as the serializer requests it.
        var dirtyEncode = serializer.SerializerFlags.HasFlag(SerializerFlags.AlwaysEncode)
            && castedProperty.Flags.HasFlag(PropertyFlags.Prop_Encode);

        // Skip properties that are not marked for serialization, or are deprecated and not dirty encoded.
        if ((castedProperty.Flags & serializer.PropertyMask) != castedProperty.Flags
            || castedProperty.Flags.HasFlag(PropertyFlags.Prop_Deprecated)
            && !dirtyEncode) {
            return false;
        }

        return true;
    }

}
