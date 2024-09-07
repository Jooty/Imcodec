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
using Imcodec.ObjectProperty.CodeGen.AST;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: InternalsVisibleTo("Imcodec.Test")]

namespace Imcodec.ObjectProperty.CodeGen.JSON {
    internal class JsonToCsharpCompiler : ExternToCsharpCompiler {

        internal override PropertyClassDefinition[] Compile(string externCode) {
            // First pass: Parse the JSON dump manifest into a list of class definitions.
            var jsonDumpManifest = GetJsonDumpManifest(externCode)
                ?? throw new Exception("Failed to parse the JSON dump manifest.");

            var classDefinitions = GetPropertyClassDefinitions(jsonDumpManifest);
            if (classDefinitions.Count == 0) {
                throw new Exception("No class definitions were found in the JSON dump manifest.");
            }

            return classDefinitions.ToArray();
        }

        private static List<PropertyClassDefinition> GetPropertyClassDefinitions(JsonDumpManifest dumpedManifest) {
            var classDefinitions = new List<PropertyClassDefinition>();
            foreach (var dumpedClass in dumpedManifest.Classes) {
                var classDefinition = GetPropertyClassDefinition(dumpedClass.Value);
                if (classDefinition == null) {
                    continue;
                }

                if (classDefinitions.Any(c => c.ClassName == classDefinition.ClassName)) {
                    // todo: don't continue. we need to know why there was a duplicate.
                    continue;
                }

                classDefinitions.Add(classDefinition);
            }

            return classDefinitions;
        }

        private static PropertyClassDefinition GetPropertyClassDefinition(JsonDumpClass dumpedClass) {
            if (dumpedClass.Name.Contains('<')) {
                return null;
            }

            var classDefinition = new PropertyClassDefinition(dumpedClass.Name, dumpedClass.Hash) {
                BaseClassNames = dumpedClass.BaseClasses.ToList()
            };

            foreach (var dumpedProperty in dumpedClass.Properties) {
                var propertyDefinition = GetPropertyDefinition(dumpedProperty.Key, dumpedProperty.Value);
                classDefinition.Properties.Add(propertyDefinition);
            }

            return classDefinition;
        }

        private static PropertyDefinition GetPropertyDefinition(string propertyName, JsonDumpProperty dumpedProperty) {
            // If the type begins with "enum," skip.
            if (dumpedProperty.Type.StartsWith("enum")) {
                return null;
            }

            return new PropertyDefinition(propertyName, dumpedProperty.Type, dumpedProperty.Flags, dumpedProperty.Container, dumpedProperty.Hash);
        }

        private static JsonDumpManifest GetJsonDumpManifest(string json)
            => JsonSerializer.Deserialize<JsonDumpManifest>(json);

    }
}
