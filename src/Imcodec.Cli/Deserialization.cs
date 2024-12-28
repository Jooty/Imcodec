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

internal class DeserializedObjectInfo {

    internal required string _fileName;
    internal uint _flags;
    internal required string _className;
    internal uint _hash;
    internal required string _deserializedOn;
    internal required string _imcodecVersion;
    internal required PropertyClass _object;

}

public static class Deserialization {

    public const string DeserializationSuffix = "_deser.json";

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
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
                };
                var jsonObj = JsonConvert.SerializeObject(deserializedObjectInfo, Formatting.Indented, jsonSerializerSettings);

                return jsonObj;
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

}
