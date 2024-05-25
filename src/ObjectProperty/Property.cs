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
public interface IProperty { }

/// <summary>
/// Represents a property with its associated flags, transferability, and a pointer to the data.
/// </summary>
public sealed class Property<T>(uint hash, PropertyFlags flags, Func<T> getter, Action<T> setter) : IProperty {

    /// <summary>
    /// The unique hash of the property.
    /// </summary>
    public uint Hash { get; } = hash;

    /// <summary>
    /// The flags of the property.
    /// </summary>
    public PropertyFlags Flags { get; } = flags;

    private Func<T> Getter { get; } = getter;
    private Action<T> Setter { get; } = setter;

    internal bool Encode(BitWriter writer, SerializerFlags serializerFlags) {
        if (StreamPropertyCodec.TryGetWriter<T>(out var codec)) {
            var val = Getter();
            codec.Invoke(writer, val!);

            return true;
        } else {
            throw new InvalidOperationException($"No codec found for type {typeof(T).Name}");
        }
    }

    internal bool Decode(BitReader reader, SerializerFlags serializerFlags) {
        if (StreamPropertyCodec.TryGetReader<T>(out var codec)) {
            var val = codec.Invoke(reader);
            Setter((T) val!);

            return true;
        } else {
            throw new InvalidOperationException($"No codec found for type {typeof(T).Name}");
        }
    }

}
