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

internal class PropertyDefinition : Definition {

    private static readonly Dictionary<string, string> s_internalTypeTranslationDict = new() {
        // Primitive
        { "int",              "int"            },
        { "unsigned int",     "uint"           },
        { "short",            "short"          },
        { "unsigned short",   "ushort"         },
        { "std.string",       "ByteString"     },
        { "std.wstring",      "WideByteString" },
        { "long",             "long"           },
        { "unsigned long",    "ulong"          },
        { "float",            "float"          },
        { "bool",             "bool"           },
        { "double",           "double"         },
        { "char",             "char"           },
        { "wchar_t",          "char"           },
        { "unsigned char",    "byte"           },
        { "unsigned __int64", "ulong"          },

        // Internal
        { "gid",              "GID"            },
        { "Vector3D",         "Vector3"        },
        { "Euler",            "Vector3"        },
        { "Quaternion",       "Quaternion"     },
        { "Matrix3x3",        "Matrix"         },
        { "Color",            "Color"          },
        { "Rect<float>",      "RectangleF"     },
        { "Rect<int>",        "Rectangle"      },
        { "Point<float>",     "Vector2"        },
        { "Point<int>",       "Point"          },
        { "Size<int>",        "Point"          },
        { "SerializedBuffer", "ByteString"     },
        { "SimpleVert",       "string"         },
        { "SimpleFace",       "string"         },

        { "bui2",             "Bui2"           }, // 2-bit byte
        { "bui4",             "Bui4"           }, // 4-bit byte
        { "bui5",             "Bui5"           }, // 5-bit byte
        { "bui7",             "Bui7"           }, // 7-bit byte
        { "s24",              "S24"            }, // 24-bit signed integer
        { "u24",              "U24"            }, // 24-bit unsigned integer
    };

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
        this.Name = name;
        this.Flags = flags;
        this.Hash = hash;
        this.IsEnum = cppType.StartsWith("enum");
        if (this.IsEnum) {
            this.EnumOptions = CleanupEnumOptions(enumOptions);
        }

        this.IsVector = IsContainerDynamic(container);
        this.CsharpType = GetCsharpType(cppType, IsVector);
    }

    private static bool IsContainerDynamic(string container)
        => container is "std::vector" or "List";

    private static Dictionary<string, int> CleanupEnumOptions(Dictionary<string, object> enumOptions) {
        var cleanedOptions = new Dictionary<string, int>();
        foreach (var option in enumOptions) {
            if (int.TryParse(option.Value.ToString(), out var value)) {
                var cleanedKey = NameCleanupUtil.CleanupWizardName(option.Key);
                cleanedOptions.Add(cleanedKey, value);
            }
        }

        return cleanedOptions;
    }

    private static string GetCsharpType(string cppType, bool isVector) {
        cppType = NameCleanupUtil.CleanupWizardName(cppType);

        if (s_internalTypeTranslationDict.TryGetValue(cppType, out var type)) {
            if (isVector) {
                return $"List<{type}>";
            }
            else {
                return type;
            }
        }
        else {
            return cppType;
        }
    }

}
