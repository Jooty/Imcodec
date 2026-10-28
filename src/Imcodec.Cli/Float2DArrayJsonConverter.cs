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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Imcodec.Cli;

/// <summary>
/// Serializes a two-dimensional float array (such as a rotation matrix) as an array of rows. The built-in
/// resolver cannot handle multidimensional arrays, so without this the whole report fails to write. Reads
/// the same row-of-rows shape back for the reverse conversion.
/// </summary>
internal sealed class Float2DArrayJsonConverter : JsonConverter<float[,]> {

    public override float[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var rows = JsonSerializer.Deserialize<float[][]>(ref reader, options) ?? [];
        var columnCount = rows.Length == 0 ? 0 : rows[0].Length;
        var result = new float[rows.Length, columnCount];
        for (var row = 0; row < rows.Length; row++) {
            for (var column = 0; column < columnCount; column++) {
                result[row, column] = rows[row][column];
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, float[,] value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        for (var row = 0; row < value.GetLength(0); row++) {
            writer.WriteStartArray();
            for (var column = 0; column < value.GetLength(1); column++) {
                writer.WriteNumberValue(value[row, column]);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }

}
