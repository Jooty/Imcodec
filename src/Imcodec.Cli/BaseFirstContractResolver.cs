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

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Imcodec.Cli;

/// <summary>
/// Orders serialized properties so that properties declared in base classes
/// appear before properties declared in derived classes. Property classes
/// declare their inherited properties first in the wire format, so this keeps
/// the JSON dump in the same order as the binary data.
/// </summary>
internal sealed class BaseFirstContractResolver : DefaultContractResolver {

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
        var properties = base.CreateProperties(type, memberSerialization);

        // OrderBy is stable, so properties declared in the same type keep
        // their declaration order.
        return properties
            .OrderBy(property => InheritanceDepth(property.DeclaringType))
            .ToList();
    }

    private static int InheritanceDepth(Type? type) {
        var depth = 0;
        for (; type?.BaseType != null; type = type.BaseType) {
            depth++;
        }

        return depth;
    }

}
