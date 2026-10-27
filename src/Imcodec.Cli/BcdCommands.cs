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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Imcodec.Cli;

/// <summary>
/// Commands for working with Binary Collision Data (BCD) files.
/// </summary>
public sealed class BcdCommands {

    private const int FAILURE_EXIT_CODE = 1;

    /// <summary>
    /// Parses a BCD file and converts it to JSON format.
    /// </summary>
    /// <param name="inputPath">The path to the BCD file to parse.</param>
    /// <param name="outputPath">The path where to save the JSON output. Defaults to the input path with .json extension.</param>
    /// <param name="verbose">Enable verbose output showing details of each collision object.</param>
    [Command("parse", Description = "Parse a BCD file and convert it to JSON format.")]
    public int ParseBcdFile(
        [Argument(Description = "The path to the BCD file to parse.")] string inputPath,
        [Argument(Description = "The path where to save the JSON output.")] string outputPath = ".",
        [Option("verbose", Description = "Enable verbose output showing details of each collision object.")] bool verbose = false) {
        if (!File.Exists(inputPath)) {
            Console.Error.WriteLine($"The specified BCD file '{inputPath}' does not exist.");

            return FAILURE_EXIT_CODE;
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
            outputPath = IOUtility.ResolveOutputFilePath(inputPath, outputPath, ".json");

            // Create wrapper object with metadata
            var bcdInfo = new {
                _fileName = Path.GetFileName(inputPath),
                _parsedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _imcodecVersion = typeof(BcdCommands).Assembly.GetName()?.Version?.ToString() ?? "Unknown",
                _collisionCount = bcd.Collisions.Count,
                _collisions = bcd.Collisions
            };

            // Serialize to JSON with nice formatting
            var jsonOptions = new JsonSerializerOptions {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var json = JsonSerializer.Serialize(bcdInfo, jsonOptions);

            File.WriteAllText(outputPath, json);

            Console.WriteLine($"Successfully converted '{Path.GetFileName(inputPath)}' to JSON: '{outputPath}'");

            return 0;
        }
        catch (InvalidDataException ex) {
            Console.Error.WriteLine($"Invalid BCD file format: {ex.Message}");

            return FAILURE_EXIT_CODE;
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Failed to parse BCD file: {ex.Message}");
            if (verbose) {
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return FAILURE_EXIT_CODE;
        }
    }

    /// <summary>
    /// Creates a new BCD file from a JSON file that was previously exported.
    /// </summary>
    /// <param name="inputPath">The path to the JSON file to convert back to BCD.</param>
    /// <param name="outputPath">The path where to save the BCD output. Defaults to the input path with .bcd extension.</param>
    /// <param name="verbose">Enable verbose output.</param>
    [Command("create", Description = "Create a BCD file from a JSON file that was previously exported.")]
    public int CreateBcdFile(
        [Argument(Description = "The path to the JSON file to convert back to BCD.")] string inputPath,
        [Argument(Description = "The path where to save the BCD output.")] string outputPath = ".",
        [Option("verbose", Description = "Enable verbose output.")] bool verbose = false) {
        if (!File.Exists(inputPath)) {
            Console.Error.WriteLine($"The specified JSON file '{inputPath}' does not exist.");

            return FAILURE_EXIT_CODE;
        }
        try {
            var jsonOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            var bcd = new Bcd();
            // Parse JSON using JsonDocument for fast, zero-allocation inspection
            using (var stream = File.OpenRead(inputPath))
            using (var doc = JsonDocument.Parse(stream)) {
                var root = doc.RootElement;
                // Extract collisions from the wrapper object (_collisions) if present
                if (root.TryGetProperty("_collisions", out var collisionsEl)) {
                    bcd.Collisions = collisionsEl.Deserialize<List<Collision>>(jsonOptions) ?? [];
                }
                else {
                    // Fallback: try to deserialize root as direct BCD object
                    bcd = root.Deserialize<Bcd>(jsonOptions) ?? new Bcd();
                }
            }
            if (verbose) {
                Console.WriteLine($"Loaded {bcd.Collisions.Count} collision objects from JSON");
            }
            // Determine output path
            outputPath = IOUtility.ResolveOutputFilePath(inputPath, outputPath, ".bcd");
            // Write BCD file
            bcd.WriteToFile(outputPath);
            Console.WriteLine($"Successfully created BCD file '{outputPath}' from '{Path.GetFileName(inputPath)}'");

            return 0;
        }
        catch (JsonException ex) {
            Console.Error.WriteLine($"Invalid JSON format: {ex.Message}");

            return FAILURE_EXIT_CODE;
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Failed to create BCD file: {ex.Message}");
            if (verbose) {
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return FAILURE_EXIT_CODE;
        }
    }

    /// <summary>
    /// Displays information about a BCD file without converting it.
    /// </summary>
    /// <param name="inputPath">The path to the BCD file to inspect.</param>
    [Command("info", Description = "Display information about a BCD file without converting it.")]
    public int ShowBcdInfo([Argument(Description = "The path to the BCD file to inspect.")] string inputPath) {
        if (!File.Exists(inputPath)) {
            Console.Error.WriteLine($"The specified BCD file '{inputPath}' does not exist.");

            return FAILURE_EXIT_CODE;
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

            return 0;
        }
        catch (Exception ex) {
            Console.Error.WriteLine($"Failed to read BCD file: {ex.Message}");

            return FAILURE_EXIT_CODE;
        }
    }

}
