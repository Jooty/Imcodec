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

using Newtonsoft.Json;

namespace Imcodec.ObjectProperty.Bit;

[JsonConverter(typeof(LongWordConverter))]
public struct S24(int value) : IConvertible {

    public int Value { readonly get; set; } = value;

    public static explicit operator S24(int value) => new(value);
    public static implicit operator int(S24 bits) => bits.Value;

    public readonly TypeCode GetTypeCode() => TypeCode.Byte;
    public readonly bool ToBoolean(IFormatProvider? provider) => Value != 0;
    public readonly char ToChar(IFormatProvider? provider) => (char) Value;
    public readonly sbyte ToSByte(IFormatProvider? provider) => (sbyte) Value;
    public readonly byte ToByte(IFormatProvider? provider) => (byte) Value;
    public readonly short ToInt16(IFormatProvider? provider) => (short) Value;
    public readonly ushort ToUInt16(IFormatProvider? provider) => (ushort) Value;
    public readonly int ToInt32(IFormatProvider? provider) => Value;
    public readonly uint ToUInt32(IFormatProvider? provider) => (uint) Value;
    public readonly long ToInt64(IFormatProvider? provider) => Value;
    public readonly ulong ToUInt64(IFormatProvider? provider) => (ulong) Value;
    public readonly float ToSingle(IFormatProvider? provider) => Value;
    public readonly double ToDouble(IFormatProvider? provider) => Value;
    public readonly decimal ToDecimal(IFormatProvider? provider) => Value;
    public readonly DateTime ToDateTime(IFormatProvider? provider) => throw new InvalidCastException();
    public readonly string ToString(IFormatProvider? provider) => Value.ToString();
    public readonly object ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType(Value, conversionType);

}

public class LongWordConverter : JsonConverter {

    public override bool CanConvert(Type objectType)
      => objectType == typeof(S24);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        if (value is S24 longWord) {
            writer.WriteValue(longWord.Value);
        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
      => reader.Value != null && int.TryParse(reader.Value.ToString(), out var intValue)
         ? new S24(intValue)
         : null;

}