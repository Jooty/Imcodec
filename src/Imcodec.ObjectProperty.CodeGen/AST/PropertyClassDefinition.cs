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

namespace Imcodec.ObjectProperty.CodeGen.AST;

[DebuggerDisplay("{ClassName}")]
internal class PropertyClassDefinition {

    internal string ClassName { get; set; }
    internal uint Hash { get; set; }

    internal List<string> BaseClassNames{ get; set; } = new List<string>();
    internal List<PropertyClassDefinition> BaseClasses { get; set; } = [];
    internal List<PropertyDefinition> Properties { get; set; } = [];

    // ctor
    internal PropertyClassDefinition(string className, uint hash) {
        if (className.StartsWith("enum")) {
            throw new System.Exception("Cannot create a PropertyClassDefinition for an enum.");
        }

        ClassName = CleanupClassName(className);
        Hash = hash;
    }

    internal void AddBaseClass(string baseName) {
        BaseClassNames.Add(CleanupClassName(baseName));
    }

    private static string CleanupClassName(string className) {
        // The type may be a shared pointer, with syntax of SharedPointer<{actualType}>.
        // We'll want to remove that part and just leave the actual type.
        var sharedPointerIndex = className.IndexOf("SharedPointer<");
        if (sharedPointerIndex != -1) {
            var endOfActualType = className.IndexOf(">", sharedPointerIndex);
            className = className.Substring(sharedPointerIndex + 14, endOfActualType - sharedPointerIndex - 14);
        }

        // If the type begins with "class," trim that off.
        if (className.StartsWith("class")) {
            className = className.Replace("class ", "");
        }

        // If the type begins with "struct," trim that off.
        if (className.StartsWith("struct")) {
            className = className.Replace("struct ", "");
        }

        // Remove any pointers.
        className = className.Replace("*", "");

        // Replace any C++ accessors with C# accessors.
        className = className.Replace("::", ".");

        // Trim the class name to the right-most accessor.
        var lastAccessorIndex = className.LastIndexOf(".");
        if (lastAccessorIndex != -1) {
            className = className.Substring(lastAccessorIndex + 1);
        }

        return className;
    }

}
