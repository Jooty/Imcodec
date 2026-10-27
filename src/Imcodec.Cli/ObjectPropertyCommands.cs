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
using Imcodec.ObjectProperty;

namespace Imcodec.Cli;

public partial class ObjectPropertyCommands {

    private const int FAILURE_EXIT_CODE = 1;

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

    private static string GetOutputFile(string inputFilePath, string outputPath)
        => outputPath == "." ? inputFilePath : outputPath;

    private static bool IsHexBlob(string hexBlob) {
        var stripped = hexBlob.Where(static c => !char.IsWhiteSpace(c)).ToArray();

        return stripped.Length > 0
            && stripped.Length % 2 == 0
            && stripped.All(static c => Uri.IsHexDigit(c));
    }

}