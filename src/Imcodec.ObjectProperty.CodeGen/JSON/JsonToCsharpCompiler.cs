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

namespace Imcodec.ObjectProperty.CodeGen.JSON {
    internal class JsonToCsharpCompiler : ExternToCsharpCompiler {

        private List<EnumDefinition> _enumDefinitions = new List<EnumDefinition>();

        internal override Definition[] Compile(string externCode) {
            // First pass: Parse the JSON dump manifest into a list of class definitions.
            var jsonDumpManifest = GetJsonDumpManifest(externCode)
                ?? throw new Exception("Failed to parse the JSON dump manifest.");

            var classDefinitions = GetDefinitions(jsonDumpManifest);
            if (classDefinitions.Count == 0) {
                throw new Exception("No class definitions were found in the JSON dump manifest.");
            }

            // Second pass: class definitions currently only have the string names of their parent classes.
            // On the second pass, we want to find the actual class definitions for each parent class.
            // We also want to remove any properties that are duplicated in the parent classes.
            SetBaseClasses(ref classDefinitions);
            RemoveDuplicatePropertiesFromClasses(ref classDefinitions);

            // Third pass: Combine duplicate enum options into a single enum definition.
            CombineDuplicateEnumOptions(ref _enumDefinitions);

            // Return both the class definitions and the enum definitions.
            var combined = new List<Definition>();
            combined.AddRange(classDefinitions);
            combined.AddRange(_enumDefinitions);

            return combined.ToArray();
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
            // Anything containing a '<' is usually client jargon. We don't want to generate code for it.
            if (dumpedClass.Name.Contains('<') || dumpedClass.Name.Contains('*')) {
                return null;
            }

            // The client has a lot of these standalone enums with a hash. They do not contain options, so they are
            // useless. Instead, we want to deal with the properties in a PropertyClass definition.
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

                // If the PropertyDefinition is an enum, we need to add it to the enum definitions.
                if (propertyDefinition.IsEnum) {
                    // The C# type may contain 'List<>'. We want to remove this.
                    var abstractType = propertyDefinition.CsharpType!.Replace("List<", "").Replace(">", "");
                    var enumDefinition = new EnumDefinition(abstractType, propertyDefinition.EnumOptions);
                    _enumDefinitions.Add(enumDefinition);
                }

                classDefinition.ExclusiveProperties.Add(propertyDefinition);
            }

            return classDefinition;
        }

        private static PropertyDefinition? GetPropertyDefinition(string propertyName, JsonDumpProperty dumpedProperty)
            => new(propertyName, dumpedProperty.Type!, dumpedProperty.Flags, dumpedProperty.Container!, dumpedProperty.Hash,
                   dumpedProperty.EnumOptions);

        private static void SetBaseClasses(ref List<PropertyClassDefinition> classDefinitions) {
            foreach (var classDefinition in classDefinitions) {
                if (classDefinition.BaseClassNames.Count == 0) {
                    throw new Exception($"Class \"{classDefinition.Name}\" has no base class.");
                }

                // Find each of the base classes in the list of class definitions.
                // Our integrity check has a -1 because the base class is always
                // 'PropertyClass', which is always defined.
                var baseClasses = classDefinitions.Where(c => classDefinition.BaseClassNames.Contains(c.Name!)).ToList();
                if (baseClasses.Count != classDefinition.BaseClassNames.Count - 1) {
                    throw new Exception($"Class \"{classDefinition.Name}\" has a base class that was not found.");
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
                    foreach (var property in baseclass.ExclusiveProperties) {
                        classDefinition.ExclusiveProperties.RemoveAll(p => p.Hash == property.Hash);
                    }
                }
            }
        }

        private static void CombineDuplicateEnumOptions(ref List<EnumDefinition> enumDefinitions) {
            // Combine duplicate enum options into a single enum definition.
            // This is useful for enums that are defined in multiple classes.
            var combinedEnums = new List<EnumDefinition>();
            foreach (var enumDefinition in enumDefinitions) {
                var existingEnum = combinedEnums.FirstOrDefault(e => e.Name == enumDefinition.Name);
                if (existingEnum == null) {
                    combinedEnums.Add(enumDefinition);
                    continue;
                }

                foreach (var option in enumDefinition.Options) {
                    // If the option already exists, we don't need to add it.
                    if (existingEnum.Options.ContainsKey(option.Key)) {
                        continue;
                    }

                    existingEnum.Options.Add(option.Key, option.Value);
                }
            }

            enumDefinitions = combinedEnums;
        }

        private static JsonDumpManifest GetJsonDumpManifest(string json)
            => JsonSerializer.Deserialize<JsonDumpManifest>(json)!;

    }
}
