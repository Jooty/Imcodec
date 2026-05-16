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
using System.Text;

namespace Imcodec.POI;

/// <summary>
/// Representation of a teleporter entry in <see cref="Poi"/> files.
/// </summary>
public class Teleporter {

    /// <summary>
    /// The destination zone for the teleport.
    /// </summary>
    [JsonProperty("destination")]
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// The exact teleport position in the zone (X, Y, Z).
    /// </summary>
    [JsonProperty("position")]
    public float[] Position { get; set; } = new float[3];

    internal static Teleporter ReadFrom(BinaryReader reader) {
        var destLen = reader.ReadUInt32();
        var destBytes = reader.ReadBytes((int) destLen);
        var destination = Encoding.UTF8.GetString(destBytes);

        return new Teleporter {
            Destination = destination,
            Position = new[] {
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle()
            }
        };
    }

    internal void WriteTo(BinaryWriter writer) {
        var destBytes = Encoding.UTF8.GetBytes(Destination);
        writer.Write((uint) destBytes.Length);
        writer.Write(destBytes);
        foreach (var v in Position) {
            writer.Write(v);
        }
    }

}
