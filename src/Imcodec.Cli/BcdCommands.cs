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
using Imcodec.BCD;
using Newtonsoft.Json;

namespace Imcodec.Cli;

/// <summary>
/// Commands for working with Binary Collision Data (BCD) files.
/// </summary>
public sealed class BcdCommands {

    /// <summary>
    /// Parses a BCD file and converts it to JSON format.
    /// </summary>
    /// <param name="inputPath">The path to the BCD file to parse.</param>
    /// <param name="outputPath">The path where to save the JSON output. Defaults to the input path with .json extension.</param>
    /// <param name="verbose">Enable verbose output showing details of each collision object.</param>
    [Command("parse", Description = "Parse a BCD file and convert it to JSON format.")]
    public void ParseBcdFile(
        [Argument(Description = "The path to the BCD file to parse.")] string inputPath,
        [Argument(Description = "The path where to save the JSON output.")] string outputPath = ".",
        [Option("verbose", Description = "Enable verbose output showing details of each collision object.")] bool verbose = false) {
        // Validate input file exists
        if (!File.Exists(inputPath)) {
            Console.WriteLine($"The specified BCD file '{inputPath}' does not exist.");

            return;
        }

        try {
            // Parse the BCD file
            var bcd = Bcd.ParseFromFile(inputPath);

            if (verbose) {
                Console.WriteLine($"Successfully parsed BCD file with {bcd.Collisions.Count} collision objects:");
                for (int i = 0; i < bcd.Collisions.Count; i++) {
                    var collision = bcd.Collisions[i];
                    Console.WriteLine($"  [{i}] {collision.Geometry.Name} ({collision.Geometry.Params.GetType().Name})");
                    Console.WriteLine($"      Category: {collision.CategoryFlags}, Collision: {collision.CollisionFlags}");
                    if (collision.Mesh != null) {
                        Console.WriteLine($"      Mesh: {collision.Mesh.Vertices.Count} vertices, {collision.Mesh.Faces.Count} faces");
                    }
                }
            }

            // Determine output path
            outputPath = GetOutputFilePath(inputPath, outputPath, ".json");

            // Create wrapper object with metadata
            var bcdInfo = new {
                _fileName = Path.GetFileName(inputPath),
                _parsedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _imcodecVersion = typeof(BcdCommands).Assembly.GetName()?.Version?.ToString() ?? "Unknown",
                _collisionCount = bcd.Collisions.Count,
                _collisions = bcd.Collisions
            };

            // Serialize to JSON with nice formatting
            var jsonSettings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };

            var json = JsonConvert.SerializeObject(bcdInfo, jsonSettings);
            File.WriteAllText(outputPath, json);

            Console.WriteLine($"Successfully converted '{Path.GetFileName(inputPath)}' to JSON: '{outputPath}'");
        }
        catch (InvalidDataException ex) {
            Console.WriteLine($"Invalid BCD file format: {ex.Message}");
        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to parse BCD file: {ex.Message}");
            if (verbose) {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Creates a new BCD file from a JSON file that was previously exported.
    /// </summary>
    /// <param name="inputPath">The path to the JSON file to convert back to BCD.</param>
    /// <param name="outputPath">The path where to save the BCD output. Defaults to the input path with .bcd extension.</param>
    /// <param name="verbose">Enable verbose output.</param>
    [Command("create", Description = "Create a BCD file from a JSON file that was previously exported.")]
    public void CreateBcdFile(
        [Argument(Description = "The path to the JSON file to convert back to BCD.")] string inputPath,
        [Argument(Description = "The path where to save the BCD output.")] string outputPath = ".",
        [Option("verbose", Description = "Enable verbose output.")] bool verbose = false) {
        // Validate input file exists
        if (!File.Exists(inputPath)) {
            Console.WriteLine($"The specified JSON file '{inputPath}' does not exist.");

            return;
        }

        try {
            // Read and parse JSON
            var json = File.ReadAllText(inputPath);
            var jsonSettings = new JsonSerializerSettings {
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };

            // Try to deserialize as our BCD info wrapper first
            try {
                var bcdInfo = JsonConvert.DeserializeObject<dynamic>(json, jsonSettings);
                var bcd = new Bcd();

                // Extract collisions from the wrapper object
                if (bcdInfo?._collisions != null) {
                    var collisionsJson = JsonConvert.SerializeObject(bcdInfo._collisions);
                    bcd.Collisions = JsonConvert.DeserializeObject<List<Collision>>(collisionsJson, jsonSettings) ?? new List<Collision>();
                }
                else {
                    // Fallback: try to deserialize as direct BCD object
                    bcd = JsonConvert.DeserializeObject<Bcd>(json, jsonSettings) ?? new Bcd();
                }

                if (verbose) {
                    Console.WriteLine($"Loaded {bcd.Collisions.Count} collision objects from JSON");
                }

                // Determine output path
                outputPath = GetOutputFilePath(inputPath, outputPath, ".bcd");

                // Write BCD file
                bcd.WriteToFile(outputPath);

                Console.WriteLine($"Successfully created BCD file '{outputPath}' from '{Path.GetFileName(inputPath)}'");
            }
            catch (JsonException ex) {
                Console.WriteLine($"Invalid JSON format: {ex.Message}");
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to create BCD file: {ex.Message}");
            if (verbose) {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Displays information about a BCD file without converting it.
    /// </summary>
    /// <param name="inputPath">The path to the BCD file to inspect.</param>
    [Command("info", Description = "Display information about a BCD file without converting it.")]
    public void ShowBcdInfo([Argument(Description = "The path to the BCD file to inspect.")] string inputPath) {
        // Validate input file exists
        if (!File.Exists(inputPath)) {
            Console.WriteLine($"The specified BCD file '{inputPath}' does not exist.");

            return;
        }

        try {
            // Parse the BCD file
            var bcd = Bcd.ParseFromFile(inputPath);

            Console.WriteLine($"BCD File Information: {Path.GetFileName(inputPath)}");
            Console.WriteLine($"File Size: {new FileInfo(inputPath).Length:N0} bytes");
            Console.WriteLine($"Collision Objects: {bcd.Collisions.Count}");
            Console.WriteLine();

            // Group by geometry type
            var typeGroups = bcd.Collisions
                .GroupBy(c => c.Geometry.Params.GetType().Name)
                .OrderBy(g => g.Key);

            Console.WriteLine("Geometry Types:");
            foreach (var group in typeGroups) {
                Console.WriteLine($"  {group.Key}: {group.Count()} objects");
            }

            // Show flag usage
            var categoryFlags = bcd.Collisions
                .Select(c => c.CategoryFlags)
                .Distinct()
                .OrderBy(f => f);
            var collisionFlags = bcd.Collisions
                .Select(c => c.CollisionFlags)
                .Distinct()
                .OrderBy(f => f);

            Console.WriteLine();
            Console.WriteLine("Category Flags in use:");
            foreach (var flag in categoryFlags) {
                var count = bcd.Collisions.Count(c => c.CategoryFlags == flag);
                Console.WriteLine($"  {flag}: {count} objects");
            }

            Console.WriteLine();
            Console.WriteLine("Collision Flags in use:");
            foreach (var flag in collisionFlags) {
                var count = bcd.Collisions.Count(c => c.CollisionFlags == flag);
                Console.WriteLine($"  {flag}: {count} objects");
            }

            // Show mesh statistics
            var meshObjects = bcd.Collisions.Where(c => c.Mesh != null).ToList();
            if (meshObjects.Any()) {
                Console.WriteLine();
                Console.WriteLine("Mesh Statistics:");
                Console.WriteLine($"  Objects with meshes: {meshObjects.Count}");
                Console.WriteLine($"  Total vertices: {meshObjects.Sum(m => m.Mesh!.Vertices.Count):N0}");
                Console.WriteLine($"  Total faces: {meshObjects.Sum(m => m.Mesh!.Faces.Count):N0}");

                var avgVertices = meshObjects.Average(m => m.Mesh!.Vertices.Count);
                var avgFaces = meshObjects.Average(m => m.Mesh!.Faces.Count);
                Console.WriteLine($"  Average vertices per mesh: {avgVertices:F1}");
                Console.WriteLine($"  Average faces per mesh: {avgFaces:F1}");
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to read BCD file: {ex.Message}");
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