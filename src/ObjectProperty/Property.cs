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
/// Represents a property with its associated flags, transferability, and a pointer to the data.
/// This inferface hack is used to allow the <see cref="Property{T}"/> class to be stored in a dictionary.
/// </summary>
public interface IProperty {

    /// <summary>
    /// The unique hash of the property.
    /// </summary>
    internal uint Hash { get; }

    /// <summary>
    /// The flags of the property.
    /// </summary>
    internal PropertyFlags Flags { get; }

    /// <summary>
    /// Encodes the value of the property using the specified <paramref name="writer"/> and <paramref name="serializer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="BitWriter"/> used to write the encoded value.</param>
    /// <param name="serializer">The <see cref="ObjectSerializer"/> used for nested property class serialization.</param>
    /// <returns><c>true</c> if the encoding is successful; otherwise, <c>false</c>.</returns>
    internal abstract bool Encode(BitWriter writer, ObjectSerializer serializer);

    /// <summary>
    /// Decodes the value of the property using the provided <paramref name="reader"/> and <paramref name="serializer"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BitReader"/> used to read the encoded value.</param>
    /// <param name="serializer">The <see cref="ObjectSerializer"/> used to deserialize nested property classes.</param>
    /// <returns><c>true</c> if the decoding is successful; otherwise, <c>false</c>.</returns>
    internal abstract bool Decode(BitReader reader, ObjectSerializer serializer);

}

/// <summary>
/// Represents a property with its associated flags, transferability, and a pointer to the data.
/// </summary>
public sealed class Property<T>(uint hash, PropertyFlags flags, Func<T> getter, Action<T> setter) : IProperty {

    uint IProperty.Hash { get; } = hash;
    PropertyFlags IProperty.Flags { get; } = flags;

    private Func<T> Getter { get; } = getter ?? throw new ArgumentNullException(nameof(getter));
    private Action<T> Setter { get; } = setter ?? throw new ArgumentNullException(nameof(setter));
    private bool IsVector => typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>);
    private bool IsEnum => typeof(T).IsEnum;
    private Type InnerType { get {
        if (IsVector) {
            return typeof(T).GetGenericArguments()[0];
        }

        return typeof(T);
    }}

    bool IProperty.Encode(BitWriter writer, ObjectSerializer serializer) {
        var val = Getter();

        // If val is of type PropertyClass, encode the object properties.
        if (val is PropertyClass propertyClass) {
            return Property<T>.EncodeNestedPropertyClass(writer, propertyClass, serializer);
        }

        if (StreamPropertyCodec.TryGetWriter<T>(out var codec)) {
            codec.Invoke(writer, val!);
            return true;
        } else {
            throw new InvalidOperationException($"No codec found for type {typeof(T).Name}");
        }
    }

    bool IProperty.Decode(BitReader reader, ObjectSerializer serializer) {
        // If the val is a list, decode the list elements.
        if (IsVector) {
            var len = serializer.SerializerFlags.HasFlag(SerializerFlags.CompactLength)
                ? reader.ReadUInt8()
                : reader.ReadUInt32();

            for (int i = 0; i < len; i++) {
                if (!DecodeElement(reader, serializer)) {
                    return false;
                }
            }
        }
        else {
            return DecodeElement(reader, serializer);
        }

        return true;
    }

    private bool DecodeElement(BitReader reader, ObjectSerializer serializer) {
        var val = Getter();

        if (InnerType.IsSubclassOf(typeof(PropertyClass))) {
            var innerPropClassInstance = (PropertyClass) Activator.CreateInstance(InnerType);
            return DecodeNestedPropertyClass(reader, innerPropClassInstance, serializer);
        }
        else if (IsEnum) {
            val = (T?) Enum.ToObject(InnerType, reader.ReadInt32());
            Setter(val!);
            return true;
        }
        else if (StreamPropertyCodec.TryGetReader<T>(out var codec)) {
            val = (T?) codec.Invoke(reader);
            Setter(val!);
            return true;
        }
        else {
            throw new InvalidOperationException($"No codec found for type {typeof(T).Name}");
        }
    }

    private static bool EncodeNestedPropertyClass(BitWriter writer, PropertyClass propertyClass, ObjectSerializer serializer) {
        if (propertyClass == null) {
            writer.WriteUInt32(0);
            return true;
        }

        writer.WriteUInt32(propertyClass.GetHash());
        return propertyClass.Encode(writer, serializer);
    }

    private static bool DecodeNestedPropertyClass(BitReader reader, PropertyClass propertyClass, ObjectSerializer serializer) {
        var hash = reader.ReadUInt32();
        if (hash == 0) {
            // Nothing to read.
            return true;
        }

        return propertyClass.Decode(reader, serializer);
    }

}
