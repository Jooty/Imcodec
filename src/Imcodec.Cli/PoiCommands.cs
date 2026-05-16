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

using Cocona;
using Imcodec.POI;
using Newtonsoft.Json;

namespace Imcodec.Cli;

/// <summary>
/// Commands for working with Point of Interest (POI) files.
/// </summary>
public sealed class PoiCommands {

    /// <summary>
    /// Parses a POI file and converts it to JSON format.
    /// </summary>
    /// <param name="inputPath">The path to the POI file to parse.</param>
    /// <param name="outputPath">The path where to save the JSON output. Defaults to the input path with .json extension.</param>
    /// <param name="verbose">Enable verbose output showing details of each section.</param>
    [Command("parse", Description = "Parse a POI file and convert it to JSON format.")]
    public void ParsePoiFile(
        [Argument(Description = "The path to the POI file to parse.")] string inputPath,
        [Argument(Description = "The path where to save the JSON output.")] string outputPath = ".",
        [Option("verbose", Description = "Enable verbose output showing details of each section.")] bool verbose = false) {
        // Validate input file exists
        if (!File.Exists(inputPath)) {
            Console.WriteLine($"The specified POI file '{inputPath}' does not exist.");

            return;
        }

        try {
            // Parse the POI file
            var poi = Poi.ParseFromFile(inputPath);

            if (verbose) {
                Console.WriteLine($"Successfully parsed POI file:");
                Console.WriteLine($"  Zone Names: {poi.ZoneNames.Count}");
                Console.WriteLine($"  Goals: {poi.Goals.Count}");
                Console.WriteLine($"  Interactive Goals: {poi.InteractiveGoals.Count}");
                Console.WriteLine($"  Teleporters: {poi.Teleporters.Count}");
                Console.WriteLine($"  Goal Adjectives: {poi.GoalAdjectives.Count}");
                Console.WriteLine($"  Zone Mobs: {poi.ZoneMobs.Count}");

                if (poi.ZoneNames.Count > 0) {
                    Console.WriteLine($"  Zones: {string.Join(", ", poi.ZoneNames)}");
                }
            }

            // Determine output path
            outputPath = GetOutputFilePath(inputPath, outputPath, ".json");

            // Create wrapper object with metadata
            var poiInfo = new {
                _fileName = Path.GetFileName(inputPath),
                _parsedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _imcodecVersion = typeof(PoiCommands).Assembly.GetName()?.Version?.ToString() ?? "Unknown",
                _poi = poi
            };

            // Serialize to JSON with nice formatting
            var jsonSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };

            var json = JsonConvert.SerializeObject(poiInfo, jsonSettings);
            File.WriteAllText(outputPath, json);

            Console.WriteLine($"Successfully converted '{Path.GetFileName(inputPath)}' to JSON: '{outputPath}'");
        }
        catch (InvalidDataException ex) {
            Console.WriteLine($"Invalid POI file format: {ex.Message}");
        }
        catch (EndOfStreamException ex) {
            Console.WriteLine($"POI file ended unexpectedly: {ex.Message}");
        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to parse POI file: {ex.Message}");
            if (verbose) {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Displays information about a POI file without converting it.
    /// </summary>
    /// <param name="inputPath">The path to the POI file to inspect.</param>
    [Command("info", Description = "Display information about a POI file without converting it.")]
    public void ShowPoiInfo([Argument(Description = "The path to the POI file to inspect.")] string inputPath) {
        // Validate input file exists
        if (!File.Exists(inputPath)) {
            Console.WriteLine($"The specified POI file '{inputPath}' does not exist.");

            return;
        }

        try {
            // Parse the POI file
            var poi = Poi.ParseFromFile(inputPath);

            Console.WriteLine($"POI File Information: {Path.GetFileName(inputPath)}");
            Console.WriteLine($"File Size: {new FileInfo(inputPath).Length:N0} bytes");
            Console.WriteLine();

            Console.WriteLine("Section Summary:");
            Console.WriteLine($"  Zone Names:       {poi.ZoneNames.Count}");
            Console.WriteLine($"  Goals:            {poi.Goals.Count}");
            Console.WriteLine($"  Interactive Goals: {poi.InteractiveGoals.Count}");
            Console.WriteLine($"  Teleporters:       {poi.Teleporters.Count}");
            Console.WriteLine($"  Goal Adjectives:   {poi.GoalAdjectives.Count}");
            Console.WriteLine($"  Zone Mobs:         {poi.ZoneMobs.Count}");
            Console.WriteLine();

            if (poi.ZoneNames.Count > 0) {
                Console.WriteLine("Zones:");
                foreach (var name in poi.ZoneNames) {
                    Console.WriteLine($"  - {name}");
                }
                Console.WriteLine();
            }

            if (poi.Teleporters.Count > 0) {
                Console.WriteLine("Teleporter Destinations:");
                foreach (var (zoneId, teleporters) in poi.Teleporters) {
                    foreach (var tp in teleporters) {
                        Console.WriteLine($"  Zone {zoneId} -> {tp.Destination} ({tp.Position[0]:F1}, {tp.Position[1]:F1}, {tp.Position[2]:F1})");
                    }
                }
                Console.WriteLine();
            }

            if (poi.ZoneMobs.Count > 0) {
                Console.WriteLine("Zone Mobs:");
                foreach (var (zoneId, mobs) in poi.ZoneMobs) {
                    Console.WriteLine($"  Zone {zoneId}: {string.Join(", ", mobs)}");
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to read POI file: {ex.Message}");
        }
    }

    /// <summary>
    /// Determines the output file path based on input path and user-specified output.
    /// </summary>
    private static string GetOutputFilePath(string inputPath, string outputPath, string newExtension) {
        if (outputPath == ".") {
            // Use input path but change extension
            return Path.ChangeExtension(inputPath, newExtension);
        }

        if (Directory.Exists(outputPath)) {
            // Output is a directory, use input filename with new extension
            var fileName = Path.GetFileNameWithoutExtension(inputPath);
            return Path.Combine(outputPath, fileName + newExtension);
        }

        // Use the specified output path as-is
        return outputPath;
    }

}
