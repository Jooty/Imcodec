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
using System.Text.Json;
using Cocona;
using Imcodec.ObjectProperty;

namespace Imcodec.Cli;

public class DeserializeCommands {

    private const uint BiNDDefaultFlags = 0x7;

    [Command("de", Description = "Deserialize a BiND buffer to a JSON file.")]
    public void Deserialize([Argument] string inputPath,
                            [Argument] string outputPath = ".") {
        // Ensure that the file exists.
        if (!File.Exists(inputPath)) {
            Console.WriteLine("The input file does not exist.");
            return;
        }

        // Read the file into a buffer and deserialize it.
        var buffer = File.ReadAllBytes(inputPath);
        var bindSerializer = new BindSerializer();
        if (!bindSerializer.Deserialize<PropertyClass>(buffer, (PropertyFlags) BiNDDefaultFlags, out var propertyClass)) {
            Console.WriteLine("Failed to deserialize the buffer.");
            return;
        }

        var fileName = IOUtility.ExtractFileName(inputPath);
        outputPath = IOUtility.GetOutputFile(inputPath, outputPath);

        // Serialize the property class to a JSON file.
        var stringJson = JsonSerializer.Serialize(propertyClass, new JsonSerializerOptions {
            WriteIndented = true
        });

        File.WriteAllText($"{outputPath}.json", stringJson);
    }

}
