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

namespace Imcodec.ObjectProperty.CodeGen.Definitions {
    internal class PropertyDefinition : Definition {

        internal string CsharpType { get; private set; }
        internal uint Flags { get; }
        internal bool IsVector { get; }
        internal bool IsEnum { get; private set; }
        internal Dictionary<string, int> EnumOptions { get; private set; } = new Dictionary<string, int>();

        // ctor
        internal PropertyDefinition(string name,
                                    string cppType,
                                    uint flags,
                                    string container,
                                    uint hash,
                                    Dictionary<string, object> enumOptions) {
            this.Name = NameSanitizer.SanitizeIdentifier(name);
            this.IsVector = IsContainerDynamic(container);
            this.Flags = flags;
            this.Hash = hash;

            // Check if the type is an enum. If it is, clean up the options.
            this.IsEnum = cppType.StartsWith("enum");
            if (this.IsEnum) {
                this.EnumOptions = CleanupEnumOptions(enumOptions);
                this.CsharpType = NameSanitizer.GetCsharpType(cppType, false);
            }
            else {
                this.CsharpType = NameSanitizer.GetCsharpType(cppType, IsVector);
            }
        }

        private static bool IsContainerDynamic(string container)
            => container is "std::vector" or "List";

        private static Dictionary<string, int> CleanupEnumOptions(Dictionary<string, object> enumOptions) {
            var cleanedOptions = new Dictionary<string, int>();
            foreach (var option in enumOptions) {
                if (int.TryParse(option.Value.ToString(), out var value)) {
                    var cleanedKey = NameSanitizer.SanitizeEnumOption(option.Key);
                    cleanedOptions.Add(cleanedKey, value);
                }
            }

            return cleanedOptions;
        }

    }
}
