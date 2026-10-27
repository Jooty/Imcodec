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

namespace Imcodec.TypeDump;

/// <summary>
/// What the client's type system holds beyond the base manifest, keyed by the same type hashes.
/// Produced only when the extended dump is requested.
/// </summary>
public sealed class ExtendedManifest {

    public Dictionary<string, ExtendedClass> Classes { get; } = [];

}

public sealed class ExtendedClass {

    public int TypeId { get; init; }
    public uint Size { get; init; }
    public bool Pointer { get; init; }
    public bool Reference { get; init; }
    public bool Singleton { get; init; }
    public long BaseOffset { get; init; }
    public Dictionary<string, ExtendedProperty> Properties { get; } = [];
    public List<ExtendedMethod> Methods { get; } = [];

}

public sealed class ExtendedProperty {

    public string? Note { get; init; }
    public string? PersistenceInfo { get; init; }
    public bool? PrimaryKey { get; init; }
    public bool? Nullable { get; init; }
    public string? PersistenceDefault { get; init; }
    public string? Default { get; set; }
    public uint? Count { get; init; }
    public List<uint> AlternateHashes { get; } = [];

}

public sealed class ExtendedMethod {

    public required string Name { get; init; }
    public string? ReturnType { get; init; }
    public List<string> Arguments { get; } = [];
    public int Flags { get; init; }
    public string Description { get; init; } = string.Empty;

}
