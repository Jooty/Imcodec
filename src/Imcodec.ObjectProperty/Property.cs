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

using System.Collections;
using System.Reflection;
using Imcodec.IO;

namespace Imcodec.ObjectProperty;

[AttributeUsage(AttributeTargets.Property)]
public class AutoPropertyAttribute(uint hash, int flags) : Attribute {

    public uint Hash = hash;
    public int Flags = flags;

}

/// <summary>
/// Represents an interface of <see cref="Property<T>"/>, allowing for a list
/// of that type to be stored in a vector without specifying the type.
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
    /// Encodes the value of the property using the specified
    /// <paramref name="writer"/> and <paramref name="serializer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="BitWriter"/> used to write the
    /// encoded value.</param>
    /// <param name="serializer">The <see cref="ObjectSerializer"/> used for
    /// nested property class serialization.</param>
    /// <returns><c>true</c> if the encoding is successful; otherwise,
    /// <c>false</c>.</returns>
    internal abstract bool Encode(BitWriter writer, ObjectSerializer serializer);

    /// <summary>
    /// Decodes the value of the property using the provided
    /// <paramref name="reader"/> and <paramref name="serializer"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BitReader"/> used to read the
    /// encoded value.</param>
    /// <param name="serializer">The <see cref="ObjectSerializer"/> used to
    /// deserialize nested property classes.</param>
    /// <returns><c>true</c> if the decoding is successful; otherwise,
    /// <c>false</c>.</returns>
    internal abstract bool Decode(BitReader reader, ObjectSerializer serializer);

}

/// <summary>
/// Represents a property of a <see cref="PropertyClass"/> object.
/// </summary>
public sealed class Property<T>(uint hash,
                                PropertyFlags flags,
                                MethodInfo? getter,
                                MethodInfo? setter,
                                object propertyClass) : IProperty {

    uint IProperty.Hash { get; } = hash;
    PropertyFlags IProperty.Flags { get; } = flags;

    private static bool IsVector
        => typeof(T).GetGenericTypeDefinition() == typeof(List<>);
    private static bool IsEnum
        => typeof(T).IsEnum;
    private static Type InnerType
        => IsVector ? typeof(T).GetGenericArguments()[0] : typeof(T);

    private MethodInfo? Getter { get; } = getter
        ?? throw new ArgumentNullException(nameof(getter));
    private MethodInfo? Setter { get; } = setter
        ?? throw new ArgumentNullException(nameof(setter));
    private object TargetObject { get; } = propertyClass
        ?? throw new ArgumentNullException(nameof(propertyClass));

    bool IProperty.Encode(BitWriter writer, ObjectSerializer serializer) {
        // If the val is a list, encode the list elements.
        if (IsVector) {
            var list = (IList) (Getter?.Invoke(TargetObject, null)
                ?? new List<T>());
            WriteVectorSize(writer, list.Count, serializer);

            foreach (var val in list) {
                if (!Property<T>.EncodeElement(writer, serializer, val)) {
                    return false;
                }
            }
        }
        else {
            if (!Property<T>.EncodeElement(writer,
                                           serializer,
                                           Getter?.Invoke(TargetObject, null))) {
                return false;
            }
        }

        return true;
    }

    bool IProperty.Decode(BitReader reader, ObjectSerializer serializer) {
        // If the val is a list, decode the list elements.
        if (IsVector) {
            var len = ReadVectorSize(reader, serializer);
            var listType = typeof(List<>).MakeGenericType(InnerType);
            var list = (IList) Activator.CreateInstance(listType)!;

            for (int i = 0; i < len; i++) {
                var decodeSuccess = Property<T>.DecodeElement(reader,
                                                              serializer,
                                                              out var val);
                if (!decodeSuccess) {
                    return false;
                }

                var index = list.Add(val!);

                // If the index is less than 0, the element was not added to the list.
                if (index < 0) {
                    return false;
                }
            }

            // Cast the list to the appropriate type and set the value.
            _ = Setter?.Invoke(TargetObject, [(T) list!]);
        }
        else {
            if (!Property<T>.DecodeElement(reader, serializer, out var val)) {
                return false;
            }

            // Cast the list to the appropriate type and set the value.
            _ = Setter?.Invoke(TargetObject, [(T) val!]);

            return true;
        }

        return true;
    }

    private static bool EncodeElement(BitWriter writer,
                                      ObjectSerializer serializer,
                                      object? val) {
        if (InnerType.IsSubclassOf(typeof(PropertyClass))) {
            return Property<T>.EncodeNestedPropertyClass(writer,
                                                         (PropertyClass) val!,
                                                         serializer);
        }
        else if (IsEnum) {
            writer.WriteUInt32((uint) (int) val!);
            return true;
        }
        else if (StreamPropertyCodec.TryGetWriter<T>(out var codec)) {
            codec.Invoke(writer, (T) val!);
            return true;
        }
        else {
            throw new InvalidOperationException($"No codec found for type {typeof(T).Name}");
        }
    }

    private static bool DecodeElement(BitReader reader,
                                      ObjectSerializer serializer,
                                      out object? val) {
        if (InnerType.IsSubclassOf(typeof(PropertyClass))) {
            var decodeSuccess = DecodeNestedPropertyClass(reader,
                                                          serializer,
                                                          out var propertyClass);
            val = propertyClass;
            return decodeSuccess;
        }
        else if (IsEnum) {
            val = (T?) Enum.ToObject(InnerType, reader.ReadUInt32());
            return true;
        }
        else if (StreamPropertyCodec.TryGetReader<T>(out var codec)) {
            val = (T?) codec.Invoke(reader);
            return true;
        }
        else {
            throw new InvalidOperationException($"No codec found for type {typeof(T).Name}");
        }
    }

    private static bool EncodeNestedPropertyClass(BitWriter writer,
                                                  PropertyClass propertyClass,
                                                  ObjectSerializer serializer) {
        if (propertyClass == null) {
            writer.WriteUInt32(0);
            return true;
        }

        writer.WriteUInt32(propertyClass.GetHash());
        return propertyClass.Encode(writer, serializer);
    }

    private static bool DecodeNestedPropertyClass(BitReader reader,
                                                  ObjectSerializer serializer,
                                                  out PropertyClass? propertyClass) {
        propertyClass = null;

        var hash = reader.ReadUInt32();
        if (hash == 0) {
            // Nothing to read.
            return true;
        }

        // Dispatch this hash and see what property class we need to create.
        propertyClass = TypeCache.Dispatch(hash);
        return propertyClass != null && propertyClass.Decode(reader, serializer);
    }

    private static uint ReadVectorSize(BitReader reader,
                                       ObjectSerializer serializer)
        // Read the vector size. The size is encoded as a compact length if the flag is set.
        // Otherwise, the size is encoded as a 32-bit unsigned integer.
        => serializer.SerializerFlags.HasFlag(SerializerFlags.CompactLength)
            ? reader.ReadBits<uint>(reader.ReadBit() ? 31 : 7)
            : reader.ReadUInt32();

    private static void WriteVectorSize(BitWriter writer,
                                        int size,
                                        ObjectSerializer serializer) {
        // Write the vector size. The size is encoded as a compact length if the flag is set.
        // Otherwise, the size is encoded as a 32-bit unsigned integer.
        if (serializer.SerializerFlags.HasFlag(SerializerFlags.CompactLength)) {
            writer.WriteBits((uint) size, size < byte.MaxValue ? 7 : 31);
        }
        else {
            writer.WriteUInt32((uint) size);
        }
    }

}