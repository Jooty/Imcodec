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

using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Imcodec.IO;

[DebuggerDisplay("{ToString()}")]
[JsonConverter(typeof(ByteStringJsonConverter))]
public readonly struct ByteString {

    private readonly byte[] _bytes;

    public ByteString(byte[] bytes)
        => _bytes = bytes;

    public ByteString(string toString)
        => _bytes = Encoding.Latin1.GetBytes(toString);

    public static implicit operator string(ByteString byteString)
        => byteString._bytes is null
            ? string.Empty
            : Encoding.Latin1.GetString(byteString._bytes);

    public static implicit operator ByteString(string str) {
        if (str is null) {
            return new ByteString();
        }

        return new ByteString(Encoding.Latin1.GetBytes(str));
    }

    public static implicit operator byte[](ByteString byteString)
        => byteString._bytes;

    public static implicit operator ByteString(byte[] buffer)
        => new(buffer);

    public override readonly string? ToString()
        => _bytes is null ? null : Encoding.Latin1.GetString(_bytes);

    public readonly int Length {
        get {
            if (_bytes is null) {
                return 0;
            }

            return _bytes.Length;
        }
    }

}

public sealed class ByteStringJsonConverter : JsonConverter<ByteString> {
    public override ByteString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString() ?? string.Empty);

    public override void Write(Utf8JsonWriter writer, ByteString value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString() ?? string.Empty);
}
