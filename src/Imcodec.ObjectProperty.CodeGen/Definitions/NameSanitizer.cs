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

internal static class NameSanitizer {

    private static readonly Dictionary<string, string> s_internalTypeTranslationDict = new() {
        // Primitive
        { "int",              "int"            },
        { "unsigned int",     "uint"           },
        { "short",            "short"          },
        { "unsigned short",   "ushort"         },
        { "std.string",       "ByteString"     },
        { "std.wstring",      "WideByteString" },
        { "wstring",          "WideByteString" },
        { "long",             "long"           },
        { "unsigned long",    "ulong"          },
        { "float",            "float"          },
        { "bool",             "bool"           },
        { "double",           "double"         },
        { "char",             "char"           },
        { "wchar_t",          "char"           },
        { "unsigned char",    "byte"           },
        { "unsigned __int64", "GID"            },

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

    /// <summary>
    /// Gets the corresponding C# type for a given C++ type.
    /// Cleans up the given input string representing a type name by removing
    /// certain prefixes, suffixes, and special characters.
    /// </summary>
    /// <param name="input">The input string representing a type name.</param>
    /// <returns>The cleaned up type name.</returns>
    internal static string SanitizeIdentifier(string input) {
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

        // Remove any `.m_full` suffixes.
        input = input.Replace(".m_full", "");

        // Trim the class name to the right-most accessor.
        var lastAccessorIndex = input.LastIndexOf(".");
        if (lastAccessorIndex != -1) {
            input = input.Substring(lastAccessorIndex + 1);
        }

        return input;
    }

    /// <summary>
    /// Cleans up the given input string representing an enum option by removing
    /// certain special characters and replacing spaces with underscores.
    /// </summary>
    /// <param name="input">The input string representing an enum option.</param>
    /// <returns>The cleaned up enum option.</returns>
    internal static string SanitizeEnumOption(string input) {
        input = SanitizeIdentifier(input);

        // Replace spaces with underscores.
        input = input.Replace(" ", "_");

        return input;
    }

    /// <summary>
    /// Gets the corresponding C# type for a given C++ type.
    /// </summary>
    /// <param name="cppType">The C++ type to translate.</param>
    /// <param name="isVector">Whether the type is a vector.</param>
    /// <returns>The corresponding C# type.</returns>
    internal static string GetCsharpType(string cppType, bool isVector) {
        cppType = SanitizeIdentifier(cppType);

        // Check if the type is in the translation dictionary.
        // If not, we'll assume it's a derivative of PropertyClass.
        if (s_internalTypeTranslationDict.TryGetValue(cppType, out var type)) {
            return isVector ? $"List<{type}>" : type;
        }
        else {
            return isVector ? $"List<{cppType}>" : cppType;
        }
    }

}
