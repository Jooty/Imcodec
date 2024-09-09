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

namespace Imcodec.ObjectProperty;

// This class is empty on purpose; it is meant to be populated by the code generator.
// It exists here, partially, to allow the object serializer to by default use this registry.

public partial class ClientGeneratedTypeRegistry : TypeRegistry {

    private readonly Dictionary<uint, System.Type> _typeMap = [];

    public override void RegisterType(uint hash, System.Type type)
        => _typeMap[hash] = type;

    public override System.Type? LookupType(uint hash) {
        _typeMap.TryGetValue(hash, out var type);
        return type;
    }

}
