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

namespace Imcodec.POI;

/// <summary>
/// An event point inside a <see cref="Poi"/> object.
/// Describes an interactive goal, collectable, or quest marker at a zone coordinate.
/// </summary>
public class Point {

    /// <summary>
    /// Whether the quest helper references this point.
    /// </summary>
    [JsonProperty("no_quest_helper")]
    public bool NoQuestHelper { get; set; }

    /// <summary>
    /// The ID of the zone this point is part of.
    /// </summary>
    [JsonProperty("zone_id")]
    public ushort ZoneId { get; set; }

    /// <summary>
    /// The template ID associated with this point.
    /// </summary>
    [JsonProperty("template_id")]
    public ulong TemplateId { get; set; }

    /// <summary>
    /// The location of this point (X, Y, Z).
    /// </summary>
    [JsonProperty("location")]
    public float[] Location { get; set; } = new float[3];

    /// <summary>
    /// Whether this point is an interactable NPC.
    /// </summary>
    [JsonProperty("interactable")]
    public bool Interactable { get; set; }

    /// <summary>
    /// Whether this point is a collectable item.
    /// </summary>
    [JsonProperty("collectable")]
    public bool Collectable { get; set; }

    internal static Point ReadFrom(BinaryReader reader) {
        return new Point {
            NoQuestHelper = reader.ReadBoolean(),
            ZoneId = reader.ReadUInt16(),
            TemplateId = reader.ReadUInt64(),
            Location = new[] {
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle()
            },
            Interactable = reader.ReadBoolean(),
            Collectable = reader.ReadBoolean()
        };
    }

    internal void WriteTo(BinaryWriter writer) {
        writer.Write(NoQuestHelper);
        writer.Write(ZoneId);
        writer.Write(TemplateId);
        foreach (var v in Location) {
            writer.Write(v);
        }
        writer.Write(Interactable);
        writer.Write(Collectable);
    }

}
