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

using Imcodec.Cryptography;
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
public sealed class Property<T>(T? val) : IProperty {

    public class TestPropertyClass : PropertyClass {
        public override uint GetHash() => 0;

        public Property<bool> MyProperty = new(true);
        public Property<byte> MyProperty2 = new(0);
    }

    /// <summary>
    /// A reference to the value of the property.
    /// </summary>
    public T? Value = val;

    /// <summary>
    /// The type reader relevant to the property.
    /// </summary>
    internal PropertyType<T> _type { get; } = PropertyType<T>.NewAs()
        ?? throw new InvalidOperationException($"No reflected type found for {typeof(T).Name}.");

    /// <summary>
    /// Encodes the value of the property using the specified <see cref="BitWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="BitWriter"/> to use for encoding.</param>
    /// <returns><c>true</c> if the encoding is successful; otherwise, <c>false</c>.</returns>
    internal bool Encode(BitWriter writer) => _type.Encode(Value, writer);

    /// <summary>
    /// Decodes the value of the property from the given <see cref="BitReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BitReader"/> to read the value from.</param>
    /// <returns><c>true</c> if the value was successfully decoded and assigned to the property; otherwise, <c>false</c>.</returns>
    internal bool Decode(BitReader reader, SerializerFlags serializerFlags) {
        if (_type.Decode(out T value, reader)) {
            Value = value;
            return true;
        }

        return false;
    }

}
