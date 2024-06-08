/*
BSD 3-Clause License

Copyright (c) 2024, Revive101

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

using Imcodec.Identification;
using Imcodec.Mathematics;
using Imcodec.ObjectProperty.Bit;
using Imcodec.Strings;

namespace Imcodec.ObjectProperty.CodeGen.AST;

internal record PropertyClassDefinition {

    internal string ClassName { get; }
    internal string BaseClassName { get; }
    internal uint Hash { get; }
    internal List<PropertyDefinition> Properties { get; }

    internal PropertyClassDefinition(string className, string baseClassName, uint hash, List<PropertyDefinition> properties) {
        this.ClassName = className;
        this.BaseClassName = baseClassName;
        this.Hash = hash;
        this.Properties = properties;
    }

}
