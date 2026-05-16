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
/// Representation of a Point of Interest (POI) file.
/// This format describes interactive events at zone coordinates,
/// including goals, teleporters, zone mobs, and quest markers.
/// </summary>
public class Poi {

    /// <summary>
    /// A list of all zone names described by this file.
    /// </summary>
    [JsonProperty("zone_names")]
    public List<string> ZoneNames { get; set; } = [];

    /// <summary>
    /// A mapping of goal IDs to the respective <see cref="Point"/>s.
    /// </summary>
    [JsonProperty("goals")]
    public Dictionary<ulong, Point> Goals { get; set; } = [];

    /// <summary>
    /// A mapping of zone IDs to lists of interactable template IDs.
    /// </summary>
    [JsonProperty("interactive_goals")]
    public Dictionary<uint, List<ulong>> InteractiveGoals { get; set; } = [];

    /// <summary>
    /// Teleporter entries between zones in this file.
    /// </summary>
    [JsonProperty("teleporters")]
    public Dictionary<uint, List<Teleporter>> Teleporters { get; set; } = [];

    /// <summary>
    /// A mapping of goal IDs to goal adjectives.
    /// </summary>
    [JsonProperty("goal_adjectives")]
    public Dictionary<ulong, List<uint>> GoalAdjectives { get; set; } = [];

    /// <summary>
    /// A list of zone mobs for each zone ID in the file.
    /// </summary>
    [JsonProperty("zone_mobs")]
    public Dictionary<uint, List<string>> ZoneMobs { get; set; } = [];

    /// <summary>
    /// Attempts to parse a POI file from a given <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>A parsed <see cref="Poi"/> object.</returns>
    /// <exception cref="InvalidDataException">Thrown when the file format is invalid.</exception>
    /// <exception cref="EndOfStreamException">Thrown when the stream ends unexpectedly.</exception>
    public static Poi Parse(Stream stream) {
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        var poi = new Poi();

        // zone_names: count(u32) → for each: len(u32) + UTF-8 string
        var zoneNameCount = reader.ReadUInt32();
        for (int i = 0; i < zoneNameCount; i++) {
            poi.ZoneNames.Add(ReadString(reader));
        }

        // goals: count(u32) → for each: key(u64), Point
        var goalCount = reader.ReadUInt32();
        for (int i = 0; i < goalCount; i++) {
            var key = reader.ReadUInt64();
            poi.Goals[key] = Point.ReadFrom(reader);
        }

        // interactive_goals: count(u32) → for each: key(u32), count(u32), [u64...]
        var interactiveCount = reader.ReadUInt32();
        for (int i = 0; i < interactiveCount; i++) {
            var key = reader.ReadUInt32();
            var listCount = reader.ReadUInt32();
            var list = new List<ulong>((int) listCount);
            for (int j = 0; j < listCount; j++) {
                list.Add(reader.ReadUInt64());
            }
            poi.InteractiveGoals[key] = list;
        }

        // teleporters: count(u32) → for each: key(u32), count(u32), [Teleporter...]
        var teleporterCount = reader.ReadUInt32();
        for (int i = 0; i < teleporterCount; i++) {
            var key = reader.ReadUInt32();
            var listCount = reader.ReadUInt32();
            var list = new List<Teleporter>((int) listCount);
            for (int j = 0; j < listCount; j++) {
                list.Add(Teleporter.ReadFrom(reader));
            }
            poi.Teleporters[key] = list;
        }

        // goal_adjectives: count(u32) → for each: key(u64), count(u32), [u32...]
        var adjectiveCount = reader.ReadUInt32();
        for (int i = 0; i < adjectiveCount; i++) {
            var key = reader.ReadUInt64();
            var listCount = reader.ReadUInt32();
            var list = new List<uint>((int) listCount);
            for (int j = 0; j < listCount; j++) {
                list.Add(reader.ReadUInt32());
            }
            poi.GoalAdjectives[key] = list;
        }

        // zone_mobs: count(u32) → for each: key(u32), str_len(u32)+UTF-8...
        // Each entry is a single mob string; entries with the same key are grouped.
        var zoneMobCount = reader.ReadUInt32();
        for (int i = 0; i < zoneMobCount; i++) {
            var key = reader.ReadUInt32();
            var mob = ReadString(reader);
            if (!poi.ZoneMobs.TryGetValue(key, out var list)) {
                list = [];
                poi.ZoneMobs[key] = list;
            }
            list.Add(mob);
        }

        return poi;
    }

    /// <summary>
    /// Parses a POI file from a file path.
    /// </summary>
    /// <param name="filePath">Path to the POI file.</param>
    /// <returns>A parsed <see cref="Poi"/> object.</returns>
    public static Poi ParseFromFile(string filePath) {
        using var fileStream = File.OpenRead(filePath);
        
        return Parse(fileStream);
    }

    /// <summary>
    /// Writes the POI data to the given <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <exception cref="IOException">Thrown when writing fails.</exception>
    public void Write(Stream stream) {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        // zone_names
        writer.Write((uint) ZoneNames.Count);
        foreach (var name in ZoneNames) {
            WriteString(writer, name);
        }

        // goals
        writer.Write((uint) Goals.Count);
        foreach (var (key, point) in Goals) {
            writer.Write(key);
            point.WriteTo(writer);
        }

        // interactive_goals
        writer.Write((uint) InteractiveGoals.Count);
        foreach (var (key, list) in InteractiveGoals) {
            writer.Write(key);
            writer.Write((uint) list.Count);
            foreach (var v in list) {
                writer.Write(v);
            }
        }

        // teleporters
        writer.Write((uint) Teleporters.Count);
        foreach (var (key, list) in Teleporters) {
            writer.Write(key);
            writer.Write((uint) list.Count);
            foreach (var t in list) {
                t.WriteTo(writer);
            }
        }

        // goal_adjectives
        writer.Write((uint) GoalAdjectives.Count);
        foreach (var (key, list) in GoalAdjectives) {
            writer.Write(key);
            writer.Write((uint) list.Count);
            foreach (var v in list) {
                writer.Write(v);
            }
        }

        // zone_mobs: count(u32) → for each key and each mob: key(u32), str_len(u32)+UTF-8
        // Flatten the dictionary into individual entries.
        var totalMobEntries = ZoneMobs.Sum(kv => kv.Value.Count);
        writer.Write((uint) totalMobEntries);
        foreach (var (key, list) in ZoneMobs) {
            foreach (var mob in list) {
                writer.Write(key);
                WriteString(writer, mob);
            }
        }
    }

    /// <summary>
    /// Writes the POI data to a file.
    /// </summary>
    /// <param name="filePath">Path where to save the POI file.</param>
    public void WriteToFile(string filePath) {
        using var fileStream = File.Create(filePath);
        Write(fileStream);
    }

    private static string ReadString(BinaryReader reader) {
        var len = reader.ReadUInt32();
        var bytes = reader.ReadBytes((int) len);
        return Encoding.UTF8.GetString(bytes);
    }

    private static void WriteString(BinaryWriter writer, string value) {
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write((uint) bytes.Length);
        writer.Write(bytes);
    }

}
