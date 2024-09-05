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

namespace Imcodec.Types;

public struct GID : IConvertible {

   // todo: go back and redo this. this is a union type in C++. How can we do that in C#?

    private ulong _value;

    public ulong Value { readonly get => _value; set => _value = value; }
    public GID(ulong value) => _value = value;

    public static explicit operator GID(ulong value) => new GID(value);
    public static implicit operator ulong(GID gid) => gid._value;

    public readonly TypeCode GetTypeCode() => TypeCode.UInt64;
    public readonly bool ToBoolean(IFormatProvider? provider) => _value != 0;
    public readonly char ToChar(IFormatProvider? provider) => (char) _value;
    public readonly sbyte ToSByte(IFormatProvider? provider) => (sbyte) _value;
    public readonly byte ToByte(IFormatProvider? provider) => (byte) _value;
    public readonly short ToInt16(IFormatProvider? provider) => (short) _value;
    public readonly ushort ToUInt16(IFormatProvider? provider) => (ushort) _value;
    public readonly int ToInt32(IFormatProvider? provider) => (int) _value;
    public readonly uint ToUInt32(IFormatProvider? provider) => (uint) _value;
    public readonly long ToInt64(IFormatProvider? provider) => (long) _value;
    public readonly ulong ToUInt64(IFormatProvider? provider) => _value;
    public readonly float ToSingle(IFormatProvider? provider) => _value;
    public readonly double ToDouble(IFormatProvider? provider) => _value;
    public readonly decimal ToDecimal(IFormatProvider? provider) => _value;
    public readonly DateTime ToDateTime(IFormatProvider? provider) => throw new InvalidCastException();
    public readonly string ToString(IFormatProvider? provider) => _value.ToString();
    public readonly object ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType(_value, conversionType);

}
