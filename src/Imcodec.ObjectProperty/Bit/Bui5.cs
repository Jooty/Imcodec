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

[JsonConverter(typeof(FiveBitByteConverter))]
public struct Bui5(byte value) : IConvertible {

    public byte Value { readonly get; set; } = value;

    public static explicit operator Bui5(byte value) => new(value);
    public static implicit operator byte(Bui5 bits) => bits.Value;

    public readonly TypeCode GetTypeCode() => TypeCode.Byte;
    public readonly bool ToBoolean(IFormatProvider? provider) => Value != 0;
    public readonly char ToChar(IFormatProvider? provider) => (char) Value;
    public readonly sbyte ToSByte(IFormatProvider? provider) => (sbyte) Value;
    public readonly byte ToByte(IFormatProvider? provider) => Value;
    public readonly short ToInt16(IFormatProvider? provider) => Value;
    public readonly ushort ToUInt16(IFormatProvider? provider) => Value;
    public readonly int ToInt32(IFormatProvider? provider) => Value;
    public readonly uint ToUInt32(IFormatProvider? provider) => Value;
    public readonly long ToInt64(IFormatProvider? provider) => Value;
    public readonly ulong ToUInt64(IFormatProvider? provider) => Value;
    public readonly float ToSingle(IFormatProvider? provider) => Value;
    public readonly double ToDouble(IFormatProvider? provider) => Value;
    public readonly decimal ToDecimal(IFormatProvider? provider) => Value;
    public readonly DateTime ToDateTime(IFormatProvider? provider) => throw new InvalidCastException();
    public readonly string ToString(IFormatProvider? provider) => Value.ToString();
    public readonly object ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType(Value, conversionType);

}

public class FiveBitByteConverter : JsonConverter {

    public override bool CanConvert(Type objectType)
      => objectType == typeof(Bui5);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
        if (value is Bui5 fiveBitByte) {
            writer.WriteValue(fiveBitByte.Value);
        }
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
      => reader.Value != null && byte.TryParse(reader.Value.ToString(), out var byteValue)
         ? new Bui5(byteValue)
         : null;

}