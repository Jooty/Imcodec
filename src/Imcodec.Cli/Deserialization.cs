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

using Imcodec.ObjectProperty;
using Newtonsoft.Json;

namespace Imcodec.Cli;

public static class Deserialization {

    public const string DeserializationSuffix = "_deser.json";

    private static readonly string s_nameSpace = typeof(ArchiveCommands).Namespace!
        .Split('.')[0]
        .Replace("$", "");
    private static readonly string s_deserFileHeaderStart
        = $"//------------------------------------------------------------------------------\n"
        + $"// This file was deserialized by ${s_nameSpace}.\n"
        + $"//";
    private static readonly string s_deserFileHeaderEnd
        = $"//------------------------------------------------------------------------------";

    /// <summary>
    /// Attempts to deserialize the given file data into a PropertyClass object. If successful, the
    /// deserialized object will be returned as a JSON string.
    /// </summary>
    /// <param name="fileName">The name of the file being deserialized.</param>
    /// <param name="fileData">The data of the file being deserialized.</param>
    /// <returns>The deserialized object as a JSON string, or null if deserialization failed.</returns>
    public static string? TryDeserializeFile(string fileName, byte[] fileData) {
        try {
            var bindSerializer = new FileSerializer();
            if (bindSerializer.Deserialize<PropertyClass>(fileData, out var propertyClass)) {
                // Ensure that enums are written as strings.
                var jsonSerializerSettings = new JsonSerializerSettings {
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };
                var jsonObj = JsonConvert.SerializeObject(propertyClass, Formatting.Indented, jsonSerializerSettings);

                // Prefix the file with the Imcodec header.
                var flags = bindSerializer.SerializerFlags;
                var fancyFlags = FormatFlagsToString(flags);
                var header = BuildDeserializationHeader(
                    $"File: {fileName}",
                    $"Serializer Flags: {fancyFlags}",
                    $"PropertyClass: {propertyClass.GetType().Name}",
                    $"PropertyClass Hash: {propertyClass.GetHash()}\n",
                    $"Deserialized on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    $"Imcodec Version: {typeof(ArchiveCommands).Assembly.GetName().Version}"
                );

                return $"{header}\n\n{jsonObj}";
            }
            else {
                return null;
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to deserialize file ({fileName}): {ex.Message} {ex.StackTrace}");
            return null;
        }
    }

    private static string FormatFlagsToString(SerializerFlags flags) {
        // Return the flags split apart as their string name, separated by a wall '|'.
        var flagNames = Enum.GetNames(flags.GetType());
        var flagValues = Enum.GetValues(flags.GetType());
        var flagString = string.Empty;
        for (var i = 0; i < flagValues.Length; i++) {
            if ((flags & (SerializerFlags) flagValues.GetValue(i)!) != 0) {
                flagString += flagNames[i] + " | ";
            }
        }

        return flagString.TrimEnd('|', ' ');
    }

    private static string BuildDeserializationHeader(params string[] headerLines) {
        var header = $"{s_deserFileHeaderStart}\n";
        foreach (var line in headerLines) {
            var splitLines = line.Split('\n');
            foreach (var splitLine in splitLines) {
                header += $"// {splitLine}\n";
            }
        }

        header += $"{s_deserFileHeaderEnd}";

        return header;
    }

}
