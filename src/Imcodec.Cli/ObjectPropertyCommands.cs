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
using Cocona;
using Imcodec.ObjectProperty;

namespace Imcodec.Cli;

public partial class ObjectPropertyCommands {

    private const int FAILURE_EXIT_CODE = 1;
    private const string SERIALIZATION_SUFFIX = "_ser.bind";

    [Command("file", Description = "Deserialize a BiND buffer to a JSON file.")]
    public int DeserializeFile([Argument] string inputPath,
                               [Argument] string outputPath = ".") {
        if (!File.Exists(inputPath)) {
            Console.Error.WriteLine("The input file does not exist.");

            return FAILURE_EXIT_CODE;
        }

        var buffer = File.ReadAllBytes(inputPath);
        var fileName = IOUtility.ExtractFileName(inputPath);
        var jsonObj = Deserialization.TryDeserializeFile(fileName, buffer);
        if (jsonObj == null) {
            Console.Error.WriteLine("Failed to deserialize the buffer.");

            return FAILURE_EXIT_CODE;
        }

        // Suffix the file name with '_deser.json' when the output path is the default.
        outputPath = GetOutputFile(inputPath, outputPath);
        outputPath = IOUtility.RemoveExtension(outputPath) + Deserialization.DeserializationSuffix;

        File.WriteAllText(outputPath, jsonObj);
        Console.WriteLine($"Successfully deserialized the buffer to '{outputPath}'.");

        return 0;
    }

    [Command("serialize", Description = "Serialize a JSON file written by 'file' back to a BiND buffer.")]
    public int SerializeFile([Argument] string inputPath,
                             [Argument] string outputPath = ".") {
        if (!File.Exists(inputPath)) {
            Console.Error.WriteLine("The input file does not exist.");

            return FAILURE_EXIT_CODE;
        }

        byte[] buffer;
        try {
            buffer = SerializeJson(File.ReadAllText(inputPath));
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException or KeyNotFoundException) {
            Console.Error.WriteLine($"Failed to serialize the JSON: {ex.Message}");

            return FAILURE_EXIT_CODE;
        }

        // Replace the '_deser.json' suffix with '_ser.bind' when the output path is the default.
        outputPath = outputPath == "." ? GetSerializedFile(inputPath) : outputPath;

        File.WriteAllBytes(outputPath, buffer);
        Console.WriteLine($"Successfully serialized the object to '{outputPath}'.");

        return 0;
    }

    [Command("blob", Description = "Deserialize a hex byte blob as JSON.")]
    public int DeserializeBlob([Argument] string hexBlob) {
        if (!IsHexBlob(hexBlob)) {
            Console.Error.WriteLine("The hex blob is empty or is not valid hexadecimal.");

            return FAILURE_EXIT_CODE;
        }

        var jsonObj = Deserialization.TryDeserializeHexBlob(hexBlob);
        if (jsonObj == null) {
            Console.Error.WriteLine("Failed to deserialize the byte array.");

            return FAILURE_EXIT_CODE;
        }

        Console.WriteLine(jsonObj);

        return 0;
    }

    private static byte[] SerializeJson(string json) {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var hash = root.GetProperty("_hash").GetUInt32();
        var flags = (SerializerFlags) root.GetProperty("_flags").GetUInt32();
        var type = PropertyClassPolymorphism.LookupType(hash)
            ?? throw new InvalidDataException($"No registered type has the hash {hash}.");

        var propertyClass = root.GetProperty("_object").Deserialize(type, Deserialization.JsonOptions) as PropertyClass
            ?? throw new InvalidDataException("The object could not be read as a property class.");

        // The BiND reader switches the flag header on when it sees the magic, so the writer must emit it.
        var serializer = new BindSerializer { SerializerFlags = flags | SerializerFlags.SerializeFlags };
        if (!serializer.Serialize(propertyClass, out var output)) {
            throw new InvalidDataException("The object could not be serialized.");
        }

        return output;
    }

    private static string GetSerializedFile(string inputFilePath) {
        var withoutSuffix = inputFilePath.EndsWith(Deserialization.DeserializationSuffix)
            ? inputFilePath[..^Deserialization.DeserializationSuffix.Length]
            : IOUtility.RemoveExtension(inputFilePath);

        return withoutSuffix + SERIALIZATION_SUFFIX;
    }

    private static string GetOutputFile(string inputFilePath, string outputPath)
        => outputPath == "." ? inputFilePath : outputPath;

    private static bool IsHexBlob(string hexBlob) {
        var stripped = hexBlob.Where(static c => !char.IsWhiteSpace(c)).ToArray();

        return stripped.Length > 0
            && stripped.Length % 2 == 0
            && stripped.All(static c => Uri.IsHexDigit(c));
    }

}