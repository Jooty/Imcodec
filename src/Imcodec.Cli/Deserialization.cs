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
using Imcodec.CoreObject;
using Newtonsoft.Json;

namespace Imcodec.Cli;

internal class DeserializedObjectInfo {

    public required string _fileName { get; set; }
    public uint _flags { get; set; }
    public required string _className { get; set; }
    public uint _hash { get; set; }
    public required string _deserializedOn { get; set; }
    public required string _imcodecVersion { get; set; }
    public required PropertyClass _object { get; set; }

}

internal class DeserializedBlobInfo {

    public required string _rawBlob { get; set; }
    public required string _deserializedOn { get; set; }
    public required string _imcodecVersion { get; set; }
    public required uint _flags { get; set; }
    public required string _serializerType { get; set; }
    public required bool _verbose { get; set; }
    public required string _objectType { get; set; }
    public required PropertyClass _object { get; set; }

}

public static class Deserialization {

    public const string DeserializationSuffix = "_deser.json";

    private static readonly List<uint> s_commonPropertyFlags = [
        1, 6, 7, 16, 19, 23, 24, 25, 27, 30, 31, 39,
        55, 59, 63, 71, 134, 135, 159, 263, 287, 519, 551,
        575, 65536, 65543, 65567, 65664, 65671, 65695, 65703,
        65727, 66087, 131079, 131103, 131207, 131335, 131359,
        262151, 262279, 1048583, 2097159, 2097183, 2097215, 4194439,
        8388615, 8388639, 8388871, 16777223, 16777247, 16777279,
        33554439, 33554463, 33554471, 33554491, 33554495, 33554695,
        33554719, 134217735, 134217759, 136314887, 268435463, 268435487,
    ];

    /// <summary>
    /// Attempts to deserialize the given file data into a PropertyClass object. If successful, the
    /// deserialized object will be returned as a JSON string.
    /// </summary>
    /// <param name="fileName">The name of the file being deserialized.</param>
    /// <param name="fileData">The data of the file being deserialized.</param>
    /// <returns>The deserialized object as a JSON string, or null if deserialization failed.</returns>
    public static string? TryDeserializeFile(string fileName, byte[] fileData) {
        try {
            var bindSerializer = new BindSerializer();
            if (bindSerializer.Deserialize<PropertyClass>(fileData, out var propertyClass)) {
                // Wrap the deserialized object with additional information.
                var deserializedObjectInfo = new DeserializedObjectInfo {
                    _fileName = fileName,
                    _flags = (uint) bindSerializer.SerializerFlags,
                    _className = propertyClass.GetType().Name,
                    _hash = propertyClass.GetHash(),
                    _deserializedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    _imcodecVersion = typeof(ArchiveCommands).Assembly.GetName()?.Version?.ToString() ?? "Unknown",
                    _object = propertyClass
                };

                // Ensure that enums are written as strings.
                var jsonSerializerSettings = new JsonSerializerSettings {
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
                };
                var jsonObj = JsonConvert.SerializeObject(deserializedObjectInfo, Formatting.Indented, jsonSerializerSettings);

                return jsonObj;
            }
            else {
                return null;
            }
        }
        catch (Exception ex) {
            Console.WriteLine("=== Deserialization failed ===");
            Console.WriteLine($"Failed to deserialize file ({fileName}): {ex.Message} {ex.StackTrace}");
            Console.WriteLine("===============================");
            
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize the given hex blob into a PropertyClass object. If successful, the
    /// deserialized object will be returned as a JSON string.
    /// </summary>
    /// <param name="hexBlob">The hex blob to be deserialized.</param>
    /// <returns>The deserialized object as a JSON string, or null if deserialization failed.</returns>
    public static string? TryDeserializeHexBlob(string hexBlob) {
        // Remove spaces from the blob if they're present.
        hexBlob = hexBlob
            .Replace(" ", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty);
        var buffer = Convert.FromHexString(hexBlob);

        // Create configurations for all serializers to try.
        var serializerConfigs = new List<(string Name, Func<ObjectSerializer> Factory, bool IsVerbose)> {
            // Standard serializers.
            ("ObjectCompact",               static () => new ObjectSerializer(false, SerializerFlags.None), false),
            ("ObjectCompactCompressed",     static () => new ObjectSerializer(false, SerializerFlags.Compress), false),
            ("ObjectVerbose",               static () => new ObjectSerializer(true, SerializerFlags.None), true),
            ("ObjectVerboseCompressed",     static () => new ObjectSerializer(true, SerializerFlags.Compress), true),
            
            // Core serializers.
            ("CoreObject",                  static () => new CoreObjectSerializer(false, SerializerFlags.None), false),
            ("CoreObjectCompressed",        static () => new CoreObjectSerializer(false, SerializerFlags.Compress), false),
            ("CoreObjectVerbose",           static () => new CoreObjectSerializer(true, SerializerFlags.None), true),
            ("CoreObjectVerboseCompressed", static () => new CoreObjectSerializer(true, SerializerFlags.Compress), true)
        };

        // Attempt to deserialize the blob using a number of methods.
        // Return the first successful deserialization.
        foreach (var (Name, Factory, IsVerbose) in serializerConfigs) {
            var serializer = Factory();

            foreach (var flag in s_commonPropertyFlags) {
                try {
                    if (serializer.Deserialize<PropertyClass>(buffer, flag, out var propertyClass)) {
                        var info = new {
                            _rawBlob = hexBlob,
                            _deserializedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            _imcodecVersion = typeof(ArchiveCommands).Assembly.GetName()?.Version?.ToString() ?? "Unknown",
                            _flags = flag,
                            _serializerType = Name,
                            _verbose = IsVerbose,
                            _objectType = propertyClass!.GetType().Name,
                            _object = propertyClass
                        };

                        // Ensure that enums are written as strings.
                        var jsonSerializerSettings = new JsonSerializerSettings {
                            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
                        };
                        var jsonObj = JsonConvert.SerializeObject(info, Formatting.Indented, jsonSerializerSettings);

                        return jsonObj;
                    }
                }
                catch {
                    continue;
                }
            }
        }

        // If no deserialization was successful, return null.
        Console.WriteLine("=== Deserialization failed ===");
        Console.WriteLine($"Failed to deserialize hex blob: {hexBlob}");
        Console.WriteLine("===============================");
        
        return null;
    }

}