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

internal record PropertyDefinition {

    private static readonly Dictionary<string, Type> s_internalTypeTranslationDict = new() {
        // Primitive
        { "int",              typeof(int)            },
        { "unsigned int",     typeof(uint)           },
        { "short",            typeof(short)          },
        { "unsigned short",   typeof(ushort)         },
        { "std.string",       typeof(ByteString)     },
        { "std.wstring",      typeof(WideByteString) },
        { "long",             typeof(long)           },
        { "unsigned long",    typeof(ulong)          },
        { "float",            typeof(float)          },
        { "bool",             typeof(bool)           },
        { "double",           typeof(double)         },
        { "char",             typeof(char)           },
        { "wchar_t",          typeof(char)           },
        { "unsigned char",    typeof(byte)           },
        { "unsigned __int64", typeof(ulong)          },

        // Internal
        { "gid",              typeof(GID)            },
        { "Vector3D",         typeof(Vector3)        },
        { "Euler",            typeof(Vector3)        },
        { "Quaternion",       typeof(Quaternion)     },
        { "Matrix3x3",        typeof(Matrix)         },
        { "Color",            typeof(Color)          },
        { "Rect<float>",      typeof(RectangleF)     },
        { "Rect<int>",        typeof(Rectangle)      },
        { "Point<float>",     typeof(Vector2)        },
        { "Point<int>",       typeof(Point)          },
        { "Size<int>",        typeof(Point)          },
        { "SerializedBuffer", typeof(ByteString)     },
        { "SimpleVert",       typeof(string)         },
        { "SimpleFace",       typeof(string)         },

        { "bui2",             typeof(Bui2)           }, // 2-bit byte
        { "bui4",             typeof(Bui4)           }, // 4-bit byte
        { "bui5",             typeof(Bui5)           }, // 5-bit byte
        { "bui7",             typeof(Bui7)           }, // 7-bit byte
        { "s24",              typeof(S24)            }, // 24-bit signed integer
        { "u24",              typeof(U24)            }, // 24-bit unsigned integer
    };

    internal string Name { get; }
    internal string CsharpType { get; private set; }
    internal PropertyFlags Flags { get; }
    internal bool IsVector { get; }
    internal uint Hash { get; }

    // ctor
    internal PropertyDefinition(string name, string cppType, uint flags, string container, uint hash) {
        this.Name = name;
        this.Flags = (PropertyFlags) flags;
        this.Hash = hash;

        this.IsVector = IsContainerDynamic(container);
        this.CsharpType = GetCsharpType(cppType, IsVector);
    }

    private static bool IsContainerDynamic(string container) {
        if (container is "std::vector" or "List") {
            return true;
        }

        return false;
    }

    private static string GetCsharpType(string cppType, bool isVector) {
        // The type may be a shared pointer, with syntax of SharedPointer<{actualType}>.
        // We'll want to remove that part and just leave the actual type.
        var sharedPointerIndex = cppType.IndexOf("SharedPointer<");
        if (sharedPointerIndex != -1) {
            cppType = cppType[(sharedPointerIndex + "SharedPointer<".Length)..];
            cppType = cppType[..^1];
        }

        // Remove any pointers.
        cppType = cppType.Replace("*", "");

        // Replace C++'s '::' with dot notation.
        cppType = cppType.Replace("::", ".");

        // If the type begins with "class" or "struct," just set that.
        if (cppType.StartsWith("class") || cppType.StartsWith("struct")) {
            return cppType.Replace("class ", "").Replace("struct ", "");
        }

        if (s_internalTypeTranslationDict.TryGetValue(cppType, out var type)) {
            if (isVector) {
                return $"List<{type.Name}>";
            }
            else {
                return type.Name;
            }
        }
        else {
            throw new Exception($"Unknown type: {cppType}");
        }
    }

}
