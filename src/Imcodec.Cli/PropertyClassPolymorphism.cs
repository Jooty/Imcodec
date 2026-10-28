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

using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Type = System.Type;

namespace Imcodec.Cli;

/// <summary>
/// Resolves the concrete <see cref="PropertyClass"/> behind a JSON node from the type registries, so object
/// graphs written by the CLI can be read back. Every registered type that derives from a declared type becomes
/// one of its polymorphic cases, discriminated by its type hash.
/// </summary>
internal static class PropertyClassPolymorphism {

    private const string DISCRIMINATOR_PROPERTY_NAME = "$type";

    private static readonly Lazy<IReadOnlyDictionary<uint, Type>> s_typesByHash = new(CreateTypesByHash);

    public static Type? LookupType(uint hash)
        => s_typesByHash.Value.TryGetValue(hash, out var type) ? type : null;

    public static void Apply(JsonTypeInfo typeInfo) {
        if (typeInfo.Kind != JsonTypeInfoKind.Object || !typeof(PropertyClass).IsAssignableFrom(typeInfo.Type)) {
            return;
        }

        var cases = s_typesByHash.Value
            .Where(entry => typeInfo.Type.IsAssignableFrom(entry.Value))
            .GroupBy(static entry => entry.Value)
            .Select(static group => group.First())
            .ToList();
        if (cases.Count == 0 || (cases.Count == 1 && cases[0].Value == typeInfo.Type)) {
            return;
        }

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions {
            TypeDiscriminatorPropertyName = DISCRIMINATOR_PROPERTY_NAME,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType
        };
        foreach (var (hash, type) in cases) {
            typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(type, hash.ToString()));
        }
    }

    private static Dictionary<uint, Type> CreateTypesByHash() {
        var typesByHash = new Dictionary<uint, Type>();

        // Client types win over server types on a shared hash, matching the serializer's own lookup order.
        foreach (var (hash, type) in new ServerGeneratedTypeRegistry().GetRegisteredTypes()) {
            typesByHash[hash] = type;
        }

        foreach (var (hash, type) in new ClientGeneratedTypeRegistry().GetRegisteredTypes()) {
            typesByHash[hash] = type;
        }

        return typesByHash;
    }

}
