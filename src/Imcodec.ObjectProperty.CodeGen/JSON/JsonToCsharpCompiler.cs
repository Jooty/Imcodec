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

using System.Runtime.CompilerServices;
using Imcodec.ObjectProperty.CodeGen.Definitions;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: InternalsVisibleTo("Imcodec.Test")]
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Imcodec.ObjectProperty.CodeGen.JSON;

internal class JsonToCsharpCompiler : ExternToCsharpCompiler {

    private readonly List<EnumDefinition> _enumDefinitions = [];

    internal override Definition[] Compile(string externCode) {
        // First pass: Parse the JSON dump manifest into a list of class definitions.
        var jsonDumpManifest = GetJsonDumpManifest(externCode)
            ?? throw new Exception("Failed to parse the JSON dump manifest.");

        var classDefinitions = GetDefinitions(jsonDumpManifest);
        if (classDefinitions.Count == 0) {
            throw new Exception("No class definitions were found in the JSON dump manifest.");
        }

        // Return both the class definitions and the enum definitions.
        var combined = new List<Definition>();
        combined.AddRange(classDefinitions);
        combined.AddRange(_enumDefinitions);

        return [.. combined];
    }

    private List<PropertyClassDefinition> GetDefinitions(JsonDumpManifest dumpedManifest) {
        var classDefinitions = new List<PropertyClassDefinition>();
        foreach (var dumpedClass in dumpedManifest.Classes) {
            var classDefinition = GetDefinition(dumpedClass.Value);
            if (classDefinition == null) {
                continue;
            }

            if (classDefinitions.Any(c => c.Name == classDefinition.Name)) {
                continue;
            }

            classDefinitions.Add(classDefinition);
        }

        return classDefinitions;
    }

    private PropertyClassDefinition? GetDefinition(JsonDumpClass dumpedClass) {
        // Anything containing a '*' is a pointer. We don't want to deal with these.
        // We also don't want to deal with SharedPointer.
        if (dumpedClass.Name.Contains('*') || dumpedClass.Name.Contains("SharedPointer")) {
            return null;
        }

        // The client has a lot of these standalone enums with a hash. They do not contain options, so they are
        // useless. Instead, we want to deal with the properties in a PropertyClass definition.
        if (dumpedClass.Name.StartsWith("enum")) {
            return null;
        }

        // Types without base classes do not derive PropertyCLass.
        if (dumpedClass.BaseClasses.Count <= 0) {
            return null;
        }

        var classDefinition = new PropertyClassDefinition(dumpedClass.Name,
                                                          dumpedClass.Hash);
        foreach (var baseClass in dumpedClass.BaseClasses) {
            classDefinition.AddBaseClass(baseClass);
        }

        foreach (var dumpedProperty in dumpedClass.Properties) {
            var propertyDefinition = GetPropertyDefinition(dumpedProperty.Key,
                                                           dumpedProperty.Value);
            if (propertyDefinition == null) {
                continue;
            }

            // If the PropertyDefinition is an enum, we need to add it to the enum definitions.
            if (propertyDefinition.IsEnum) {
                // The C# type may contain 'List<>'. We want to remove this.
                var abstractType = propertyDefinition.CsharpType!.Replace("List<", "").Replace(">", "");
                var enumDefinition = new EnumDefinition(abstractType, propertyDefinition.EnumOptions);
                _enumDefinitions.Add(enumDefinition);
            }

            classDefinition.AllProperties.Add(propertyDefinition);
        }

        return classDefinition;
    }

    private static PropertyDefinition? GetPropertyDefinition(string propertyName, JsonDumpProperty dumpedProperty)
        => new(propertyName, dumpedProperty.Type!, dumpedProperty.Flags, dumpedProperty.Container!, dumpedProperty.Hash,
               dumpedProperty.EnumOptions);

    private static JsonDumpManifest GetJsonDumpManifest(string json)
        => JsonSerializer.Deserialize<JsonDumpManifest>(json)!;

}
