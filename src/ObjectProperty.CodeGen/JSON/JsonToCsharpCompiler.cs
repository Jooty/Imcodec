/*
BSD 3-Clause License

Copyright (c) 2024, Revive101

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

using Imcodec.ObjectProperty.CodeGen.AST;
using System.Text.Json;

namespace Imcodec.ObjectProperty.CodeGen.JSON;

internal class JsonToCsharpCompiler : ExternToCsharpCompiler {

    internal override string? Compile(string externCode) {
        var jsonDumpManifest = GetJsonDumpManifest(externCode);
        if (jsonDumpManifest is null) {
            return null;
        }

        var classDefinitions = new List<PropertyClassDefinition>();
        foreach (var dumpedClass in jsonDumpManifest.Classes) {
            var classDefinition = GetPropertyClassDefinition(dumpedClass.Value);
            if (classDefinition is not null) {
                classDefinitions.Add(classDefinition);
            }
        }

        return null;
    }

    private static PropertyClassDefinition? GetPropertyClassDefinition(JsonDumpClass dumpedClass) {
        var classDefinition = new PropertyClassDefinition(dumpedClass.Name, dumpedClass.Hash) {
            BaseClassNames = [.. dumpedClass.BaseClasses]
        };

        foreach (var dumpedProperty in dumpedClass.Properties) {
            var propertyDefinition = GetPropertyDefinition(dumpedProperty.Key, dumpedProperty.Value);
            if (propertyDefinition is not null) {
                classDefinition.Properties.Add(propertyDefinition);
            }
        }

        return classDefinition;
    }

    private static PropertyDefinition? GetPropertyDefinition(string propertyName, JsonDumpProperty dumpedProperty) {
        // If the type begins with "enum," skip.
        if (dumpedProperty.Type.StartsWith("enum")) {
            return null;
        }

        return new PropertyDefinition(propertyName, dumpedProperty.Type, dumpedProperty.Flags, dumpedProperty.Container, dumpedProperty.Hash);
    }

    private static JsonDumpManifest GetJsonDumpManifest(string json)
        => JsonSerializer.Deserialize<JsonDumpManifest>(json)!;

}
