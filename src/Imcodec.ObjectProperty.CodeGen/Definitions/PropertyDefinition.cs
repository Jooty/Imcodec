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

namespace Imcodec.ObjectProperty.CodeGen.Definitions;

internal class PropertyDefinition : Definition {

    internal string CsharpType { get; private set; }
    internal uint Flags { get; }
    internal bool IsVector { get; }
    internal bool IsEnum { get; private set; }
    internal Dictionary<string, int> EnumOptions { get; private set; } = [];

    // ctor
    internal PropertyDefinition(string name,
                                string cppType,
                                uint flags,
                                string container,
                                uint hash,
                                Dictionary<string, object> enumOptions) {
        Name = NameSanitizer.SanitizeIdentifier(name);
        IsVector = IsContainerDynamic(container);
        Flags = flags;
        Hash = hash;

        // Check if the type is an enum. If it is, clean up the options.
        IsEnum = cppType.StartsWith("enum");
        if (IsEnum) {
            EnumOptions = CleanupEnumOptions(enumOptions);
            CsharpType = NameSanitizer.GetCsharpType(cppType, false);
        }
        else {
            CsharpType = NameSanitizer.GetCsharpType(cppType, IsVector);
        }
    }

    private static bool IsContainerDynamic(string container)
        => container is "std::vector" or "List" or "Vector";

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
