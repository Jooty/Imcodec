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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Imcodec.Cli;

/// <summary>
/// Commands for working with Point of Interest (POI) files.
/// </summary>
public sealed class PoiCommands {

    private const int FAILURE_EXIT_CODE = 1;

    /// <summary>
    /// Parses a POI file and converts it to JSON format.
    /// </summary>
    /// <param name="inputPath">The path to the POI file to parse.</param>
    /// <param name="outputPath">The path where to save the JSON output. Defaults to the input path with .json extension.</param>
    /// <param name="verbose">Enable verbose output showing details of each section.</param>
    [Command("parse", Description = "Parse a POI file and convert it to JSON format.")]
    public int ParsePoiFile(
        [Argument(Description = "The path to the POI file to parse.")] string inputPath,
        [Argument(Description = "The path where to save the JSON output.")] string outputPath = ".",
        [Option("verbose", Description = "Enable verbose output showing details of each section.")] bool verbose = false) {
        if (!File.Exists(inputPath)) {
            Console.Error.WriteLine($"The specified POI file '{inputPath}' does not exist.");

            return FAILURE_EXIT_CODE;
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
            outputPath = IOUtility.ResolveOutputFilePath(inputPath, outputPath, ".json");

            // Create wrapper object with metadata
            var poiInfo = new {
                _fileName = Path.GetFileName(inputPath),
                _parsedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _imcodecVersion = typeof(PoiCommands).Assembly.GetName()?.Version?.ToString() ?? "Unknown",
                _poi = poi
            };

            // Serialize to JSON with nice formatting
            var jsonOptions = new JsonSerializerOptions {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var json = JsonSerializer.Serialize(poiInfo, jsonOptions);

            File.WriteAllText(outputPath, json);

            Console.WriteLine($"Successfully converted '{Path.GetFileName(inputPath)}' to JSON: '{outputPath}'");

            return 0;
        }
        catch (InvalidDataException ex) {
            Console.Error.WriteLine($"Invalid POI file format: {ex.Message}");

            return FAILURE_EXIT_CODE;
        }
        catch (EndOfStreamException ex) {
            Console.Error.WriteLine($"POI file ended unexpectedly: {ex.Message}");

            return FAILURE_EXIT_CODE;
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Failed to parse POI file: {ex.Message}");
            if (verbose) {
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return FAILURE_EXIT_CODE;
        }
    }

    /// <summary>
    /// Displays information about a POI file without converting it.
    /// </summary>
    /// <param name="inputPath">The path to the POI file to inspect.</param>
    [Command("info", Description = "Display information about a POI file without converting it.")]
    public int ShowPoiInfo([Argument(Description = "The path to the POI file to inspect.")] string inputPath) {
        if (!File.Exists(inputPath)) {
            Console.Error.WriteLine($"The specified POI file '{inputPath}' does not exist.");

            return FAILURE_EXIT_CODE;
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

            return 0;
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Failed to read POI file: {ex.Message}");

            return FAILURE_EXIT_CODE;
        }
    }

}
