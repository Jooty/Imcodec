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

using Imcodec.ObjectProperty.PropertyClass.Types;

namespace Imcodec.ObjectProperty.PropertyClass;

internal static class TypeDetermine {

    internal static ReflectedType<T>? GetAs<T>() => typeof(T) switch {
        var t when t == typeof(bool) => new _Bool() as ReflectedType<T>,
        var t when t == typeof(byte) => new _Byte() as ReflectedType<T>,
        var t when t == typeof(char) => new _Char() as ReflectedType<T>,
        var t when t == typeof(short) => new _Short() as ReflectedType<T>,
        var t when t == typeof(ushort) => new _UShort() as ReflectedType<T>,
        var t when t == typeof(int) => new _Int() as ReflectedType<T>,
        var t when t == typeof(uint) => new _UInt() as ReflectedType<T>,
        var t when t == typeof(long) => new _Long() as ReflectedType<T>,
        var t when t == typeof(ulong) => new _ULong() as ReflectedType<T>,
        var t when t == typeof(float) => new _Float() as ReflectedType<T>,
        var t when t == typeof(double) => new _Double() as ReflectedType<T>,
        _ => null,
    };

}
