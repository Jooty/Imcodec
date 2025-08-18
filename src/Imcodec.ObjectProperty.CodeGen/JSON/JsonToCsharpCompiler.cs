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

internal class ClassNameCandidate {
    internal string OriginalName { get; }
    internal string SanitizedName { get; set; }
    internal JsonDumpClass DumpedClass { get; }

    internal ClassNameCandidate(string originalName, string sanitizedName, JsonDumpClass dumpedClass) {
        OriginalName = originalName;
        SanitizedName = sanitizedName;
        DumpedClass = dumpedClass;
    }
}

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
        // First pass: Collect all candidates with their original and sanitized names.
        var candidates = new List<ClassNameCandidate>();

        foreach (var dumpedClass in dumpedManifest.Classes) {
            var originalName = dumpedClass.Value.Name;
            var sanitizedName = NameSanitizer.SanitizeIdentifier(originalName);

            if (originalName.Contains('*') || originalName.Contains("SharedPointer") ||
                originalName.StartsWith("enum") || dumpedClass.Value.BaseClasses.Count <= 0) {
                continue;
            }

            candidates.Add(new ClassNameCandidate(originalName, sanitizedName, dumpedClass.Value));
        }

        // Second pass: Detect and resolve collisions.
        ResolveNameCollisions(candidates);

        // Third pass: Create PropertyClassDefinitions with resolved names.
        var classDefinitions = new List<PropertyClassDefinition>();
        foreach (var candidate in candidates) {
            var classDefinition = CreateDefinitionFromCandidate(candidate);
            if (classDefinition != null) {
                classDefinitions.Add(classDefinition);
            }
        }

        return classDefinitions;
    }

    private void ResolveNameCollisions(List<ClassNameCandidate> candidates) {
        // Group candidates by sanitized name to find collisions
        var collisionGroups = candidates
            .GroupBy(c => c.SanitizedName)
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var collisionGroup in collisionGroups) {
            var conflictingCandidates = collisionGroup.ToList();

            // Find the "root" candidate (one without namespace, or shortest namespace).
            var rootCandidate = FindRootCandidate(conflictingCandidates);

            // Resolve collisions by adding distinguishing prefixes.
            foreach (var candidate in conflictingCandidates) {
                if (candidate == rootCandidate) {
                    continue; 
                }

                var distinguishingPrefix = ExtractDistinguishingPrefix(candidate.OriginalName);
                candidate.SanitizedName = $"{distinguishingPrefix}_{candidate.SanitizedName}";
            }
        }
    }

    private ClassNameCandidate FindRootCandidate(List<ClassNameCandidate> conflictingCandidates) {
        // Prefer the candidate with no namespace (just "class ResultItem")
        // or the one with the shortest namespace.
        return conflictingCandidates
            .OrderBy(c => GetNamespaceDepth(c.OriginalName))
            .ThenBy(c => c.OriginalName.Length)
            .First();
    }

    private int GetNamespaceDepth(string originalName) {
        // Count the number of "::" separators to determine namespace depth
        var cleanName = originalName.Replace("class ", "").Replace("struct ", "");
        return cleanName.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries).Length - 1;
    }

    private string ExtractDistinguishingPrefix(string originalName) {
        // Extract the namespace part that will distinguish this class
        // "class Search::ResultItem" → "Search"
        // "class Battle::UI::ResultItem" → "Battle_UI"

        var cleanName = originalName.Replace("class ", "").Replace("struct ", "");
        var parts = cleanName.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length <= 1) {
            return "Base"; 
        }

        // Take all parts except the last one (which is the class name).
        var namespaceParts = parts.Take(parts.Length - 1);

        return string.Join("_", namespaceParts.Select(NameSanitizer.SanitizeIdentifier));
    }

    private PropertyClassDefinition? CreateDefinitionFromCandidate(ClassNameCandidate candidate) {
        // Use the original GetDefinition logic but with the resolved name.
        var dumpedClass = candidate.DumpedClass;

        var classDefinition = new PropertyClassDefinition(candidate.SanitizedName, dumpedClass.Hash);

        foreach (var baseClass in dumpedClass.BaseClasses) {
            classDefinition.AddBaseClass(baseClass);
        }

        foreach (var dumpedProperty in dumpedClass.Properties) {
            var propertyDefinition = GetPropertyDefinition(dumpedProperty.Key, dumpedProperty.Value);
            if (propertyDefinition == null) {
                continue;
            }

            // If the PropertyDefinition is an enum, we need to add it to the enum definitions.
            if (propertyDefinition.IsEnum) {
                var abstractType = propertyDefinition.CsharpType!.Replace("List<", "").Replace(">", "");
                var enumDefinition = new EnumDefinition(abstractType, propertyDefinition.EnumOptions);
                _enumDefinitions.Add(enumDefinition);
            }

            classDefinition.AllProperties.Add(propertyDefinition);
        }

        return classDefinition;
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
