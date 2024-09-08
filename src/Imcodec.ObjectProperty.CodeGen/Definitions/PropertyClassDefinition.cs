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

using System.Collections.Generic;
using System.Diagnostics;

namespace Imcodec.ObjectProperty.CodeGen.Definitions;

internal class PropertyClassDefinition : Definition {

    internal List<string> BaseClassNames{ get; set; } = new List<string>();
    internal List<PropertyClassDefinition> BaseClasses { get; set; } = [];
    internal List<PropertyDefinition> Properties { get; set; } = [];

    // ctor
    internal PropertyClassDefinition(string className, uint hash) {
        if (className.StartsWith("enum")) {
            throw new System.Exception("Cannot create a PropertyClassDefinition for an enum.");
        }

        Name = NameSanitizer.SanitizeIdentifier(className);
        Hash = hash;
    }

    internal void AddBaseClass(string baseName)
        => BaseClassNames.Add(NameSanitizer.SanitizeIdentifier(baseName));


}
