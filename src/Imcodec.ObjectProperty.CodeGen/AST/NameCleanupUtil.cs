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

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Imcodec.ObjectProperty.CodeGen.AST;

internal static class NameCleanupUtil {

    internal static string CleanupWizardName(string input) {
        // The type may be a shared pointer, with syntax of SharedPointer<{actualType}>.
        // We'll want to remove that part and just leave the actual type.
        var sharedPointerIndex = input.IndexOf("SharedPointer<");
        if (sharedPointerIndex != -1) {
            input = input.Substring(sharedPointerIndex + "SharedPointer<".Length);
            input = input.Substring(0, input.Length - 1);
        }

        // Remove any pointers.
        input = input.Replace("*", "");

        // Replace C++'s '::' with dot notation.
        input = input.Replace("::", ".");

        // Replace dashes with underscores.
        input = input.Replace("-", "_");

        // Remove any 'class,' 'struct,' or 'enum' prefixes.
        input = input.Replace("class ", "");
        input = input.Replace("struct ", "");
        input = input.Replace("enum ", "");

        // Trim the class name to the right-most accessor.
        var lastAccessorIndex = input.LastIndexOf(".");
        if (lastAccessorIndex != -1) {
            input = input.Substring(lastAccessorIndex + 1);
        }

        return input;
    }

}
