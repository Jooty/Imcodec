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

using System.Text.Json.Serialization.Metadata;

namespace Imcodec.Cli;

internal static class BaseFirstTypeResolver {

    public static void OrderProperties(JsonTypeInfo typeInfo) {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) {
            return;
        }
        for (int i = 0; i < typeInfo.Properties.Count; i++) {
            var prop = typeInfo.Properties[i];
            var depth = InheritanceDepth(prop.DeclaringType);
            prop.Order = depth * 1000 + i;
        }
    }

    private static int InheritanceDepth(Type? type) {
        var depth = 0;
        for (; type?.BaseType != null; type = type.BaseType) {
            depth++;
        }

        return depth;
    }

}
