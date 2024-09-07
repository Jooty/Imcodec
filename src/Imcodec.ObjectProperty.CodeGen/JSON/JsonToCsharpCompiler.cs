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

            // Second pass: class definitions currently only have the string names of their parent classes.
            // On the second pass, we want to find the actual class definitions for each parent class.
            // We also want to remove any properties that are duplicated in the parent classes.
            SetBaseClasses(ref classDefinitions);
            RemoveDuplicatePropertiesFromClasses(ref classDefinitions);

            return [.. classDefinitions];
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

        private static PropertyClassDefinition? GetPropertyClassDefinition(JsonDumpClass dumpedClass) {
            if (dumpedClass.Name.Contains('<')) {
                return null;
            }

            if (dumpedClass.Name.StartsWith("enum")) {
                return null;
            }

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

                classDefinition.Properties.Add(propertyDefinition);
            }

            return classDefinition;
        }

        private static PropertyDefinition? GetPropertyDefinition(string propertyName, JsonDumpProperty dumpedProperty) {
            // If the type begins with "enum," skip.
            if (dumpedProperty.Type.StartsWith("enum")) {
                return null;
            }

            return new PropertyDefinition(propertyName,
                                          dumpedProperty.Type,
                                          dumpedProperty.Flags,
                                          dumpedProperty.Container,
                                          dumpedProperty.Hash);
        }

        private static void SetBaseClasses(ref List<PropertyClassDefinition> classDefinitions) {
            foreach (var classDefinition in classDefinitions) {
                if (classDefinition.BaseClassNames.Count == 0) {
                    throw new Exception($"Class \"{classDefinition.ClassName}\" has no base class.");
                }

                // Find each of the base classes in the list of class definitions.
                // Our integrity check has a -1 because the base class is always
                // 'PropertyClass', which is always defined.
                var baseClasses = classDefinitions.Where(c => classDefinition.BaseClassNames.Contains(c.ClassName)).ToList();
                if (baseClasses.Count != classDefinition.BaseClassNames.Count - 1) {
                    throw new Exception($"Class \"{classDefinition.ClassName}\" has a base class that was not found.");
                }

                classDefinition.BaseClasses = baseClasses;
            }
        }

        private static void RemoveDuplicatePropertiesFromClasses(ref List<PropertyClassDefinition> classDefinitions) {
            // Each PropertyClassDefinition has a number of parent classes. We want to remove the properties
            // that are duplicated in the parent classes. If the definition only inherits from PropertyClass, we
            // don't need to do anything.
            foreach (var classDefinition in classDefinitions) {
                if (classDefinition.BaseClasses.Count == 0) {
                    continue;
                }

                foreach (var baseclass in classDefinition.BaseClasses) {
                    foreach (var property in baseclass.Properties) {
                        if (classDefinition.Properties.Any(p => p.Name == property.Name)) {
                            classDefinition.Properties.Remove(property);
                        }
                    }
                }
            }
        }

        private static JsonDumpManifest GetJsonDumpManifest(string json)
            => JsonSerializer.Deserialize<JsonDumpManifest>(json);

    }
}
