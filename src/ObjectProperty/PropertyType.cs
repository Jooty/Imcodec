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
using Imcodec.ObjectProperty.Types;

namespace Imcodec.ObjectProperty;

/// <summary>
/// Represents an abstract base class for reflected types.
/// </summary>
internal abstract class PropertyType<T>() {

    /// <summary>
    /// Decodes the value of the specified type from the binary reader.
    /// </summary>
    /// <param name="value">The value to load.</param>
    /// <param name="reader">The binary reader to read from.</param>
    /// <returns><c>true</c> if the value was successfully loaded; otherwise, <c>false</c>.</returns>
    internal abstract bool Decode(out T value, BitReader reader);

    /// <summary>
    /// Encodes the value of the specified type to the binary writer.
    /// </summary>
    /// <param name="writer">The binary writer to write to.</param>
    /// <returns><c>true</c> if the value was successfully saved; otherwise, <c>false</c>.</returns>
    internal abstract bool Encode(T value, BitWriter writer);

    /// <summary>
    /// Creates a new instance of the <see cref="PropertyType{T}"/> class based on the type parameter <typeparamref name="T"/>.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="PropertyType{T}"/> if the type parameter <typeparamref name="T"/> matches one of the supported types,
    /// otherwise returns <c>null</c>.
    /// </returns>
    internal static PropertyType<T>? NewAs() => typeof(T) switch {
        var t when t == typeof(bool)   => new _Bool()   as PropertyType<T>,
        var t when t == typeof(byte)   => new _Byte()   as PropertyType<T>,
        var t when t == typeof(char)   => new _Char()   as PropertyType<T>,
        var t when t == typeof(short)  => new _Short()  as PropertyType<T>,
        var t when t == typeof(ushort) => new _UShort() as PropertyType<T>,
        var t when t == typeof(int)    => new _Int()    as PropertyType<T>,
        var t when t == typeof(uint)   => new _UInt()   as PropertyType<T>,
        var t when t == typeof(long)   => new _Long()   as PropertyType<T>,
        var t when t == typeof(ulong)  => new _ULong()  as PropertyType<T>,
        var t when t == typeof(float)  => new _Float()  as PropertyType<T>,
        var t when t == typeof(double) => new _Double() as PropertyType<T>,
        _ => null,
    };

}
