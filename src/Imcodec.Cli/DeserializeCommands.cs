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

using System;
using Newtonsoft.Json;
using Cocona;
using Imcodec.ObjectProperty;

namespace Imcodec.Cli;

public class DeserializeCommands {

    [Command("deser", Description = "Deserialize a BiND buffer to a JSON file.")]
    public void Deserialize([Argument] string inputPath,
                            [Argument] string outputPath = ".") {
        // Ensure that the file exists.
        if (!File.Exists(inputPath)) {
            Console.WriteLine("The input file does not exist.");
            return;
        }

        // Read the file into a buffer and deserialize it.
        var buffer = File.ReadAllBytes(inputPath);
        var fileName = IOUtility.ExtractFileName(inputPath);
        var jsonObj = Deserialization.TryDeserializeFile(fileName, buffer);
        if (jsonObj == null) {
            Console.WriteLine("Failed to deserialize the buffer.");
            return;
        }

        // Get the output file path. Suffix the file name with '_deser.json' if the output path is the default.
        outputPath = GetOutputFile(inputPath, outputPath);
        outputPath = IOUtility.RemoveExtension(outputPath) + Deserialization.DeserializationSuffix;

        // Write the deserialized object to the output file.
        File.WriteAllText(outputPath, jsonObj);
        Console.WriteLine($"Successfully deserialized the buffer to '{outputPath}'.");
    }

    private static string GetOutputFile(string inputFilePath, string outputPath) {
        // '.' denotes default. If the output path is the default, use the archive path.
        if (outputPath == ".") {
            outputPath = inputFilePath;
        }

        return outputPath;
    }

}
