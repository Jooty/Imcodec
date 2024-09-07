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

using Imcodec.IO;
using Imcodec.Math;
using Imcodec.Types;

namespace Imcodec.ObjectProperty;

/// <summary>
/// Declares how a certain type of property should be encoded and decoded.
/// </summary>
internal static class StreamPropertyCodec {

    private static readonly Dictionary<Type, Func<BitReader, object>> s_primitiveReaders = new() {
        { typeof(byte),           (r) => r.ReadUInt8()                     },
        { typeof(char),           (r) => r.ReadUInt8()                     },
        { typeof(bool),           (r) => r.ReadBit()                       },
        { typeof(short),          (r) => r.ReadInt16()                     },
        { typeof(ushort),         (r) => r.ReadUInt16()                    },
        { typeof(int),            (r) => r.ReadInt32()                     },
        { typeof(uint),           (r) => r.ReadUInt32()                    },
        { typeof(long),           (r) => r.ReadInt64()                     },
        { typeof(double),         (r) => r.ReadDouble()                    },
        { typeof(ulong),          (r) => r.ReadUInt64()                    },
        { typeof(ByteString),     (r) => r.ReadString()                    },
        { typeof(WideByteString), (r) => r.ReadWString()                   },
        { typeof(float),          (r) => r.ReadFloat()                     },
        { typeof(Vector3),        (r) => r.ReadVector3()                   },
        { typeof(Quaternion),     (r) => r.ReadQuaternion()                },
        { typeof(Matrix),         (r) => r.ReadMatrix()                    },
        { typeof(Color),          (r) => r.ReadColor()                     },
        { typeof(Rectangle),      (r) => r.ReadRectangle()                 },
        { typeof(RectangleF),     (r) => r.ReadRectangleF()                },
        { typeof(Vector2),        (r) => r.ReadVector2()                   },
        { typeof(Point),          (r) => r.ReadVector2()                   },
        { typeof(Bui2),           (r) => r.ReadBits<byte>(2)               },
        { typeof(Bui4),           (r) => r.ReadBits<byte>(4)               },
        { typeof(Bui5),           (r) => r.ReadBits<byte>(5)               },
        { typeof(Bui7),           (r) => r.ReadBits<byte>(7)               },
        { typeof(GID),            (r) => r.ReadUInt64()                    },
        { typeof(S24),            (r) => r.ReadBits<S24>(24)               },
        { typeof(U24),            (r) => r.ReadBits<U24>(24)               },
    };

    private static readonly Dictionary<Type, Action<BitWriter, object>> s_primitiveWriters = new() {
        { typeof(byte),           (r, v) => r.WriteUInt8((byte)v)                },
        { typeof(char),           (r, v) => r.WriteUInt8(Convert.ToByte(v))      },
        { typeof(bool),           (r, v) => r.WriteBit((bool)v)                  },
        { typeof(short),          (r, v) => r.WriteInt16((short)v)               },
        { typeof(ushort),         (r, v) => r.WriteUInt16((ushort)v)             },
        { typeof(int),            (r, v) => r.WriteInt32((int)v)                 },
        { typeof(uint),           (r, v) => r.WriteUInt32((uint)v)               },
        { typeof(long),           (r, v) => r.WriteInt64((long)v)                },
        { typeof(ulong),          (r, v) => r.WriteUInt64((ulong)v)              },
        { typeof(double),         (r, v) => r.WriteDouble((double)v)             },
        { typeof(ByteString),     (r, v) => r.WriteString((ByteString)v)         },
        { typeof(WideByteString), (r, v) => r.WriteWString((WideByteString)v)    },
        { typeof(float),          (r, v) => r.WriteFloat((float)v)               },
        { typeof(Vector3),        (r, v) => r.WriteVector3((Vector3)v)           },
        { typeof(Quaternion),     (r, v) => r.WriteQuaternion((Quaternion)v)     },
        { typeof(Matrix),         (r, v) => r.WriteMatrix((Matrix)v)             },
        { typeof(Color),          (r, v) => r.WriteColor((Color)v)               },
        { typeof(Rectangle),      (r, v) => r.WriteRectangle((Rectangle)v)       },
        { typeof(RectangleF),     (r, v) => r.WriteRectangleF((RectangleF)v)     },
        { typeof(Vector2),        (r, v) => r.WriteVector2((Vector2)v)           },
        { typeof(Point),          (r, v) => r.WriteVector2((Point)v)             },
        { typeof(Bui2),           (r, v) => r.WriteBits((Bui2)v, 2)              },
        { typeof(Bui4),           (r, v) => r.WriteBits((Bui4)v, 4)              },
        { typeof(Bui5),           (r, v) => r.WriteBits((Bui5)v, 5)              },
        { typeof(Bui7),           (r, v) => r.WriteBits((Bui7)v, 7)              },
        { typeof(GID),            (r, v) => r.WriteUInt64((GID)v)                },
        { typeof(S24),            (r, v) => r.WriteBits((S24)v, 24)              },
        { typeof(U24),            (r, v) => r.WriteBits((U24)v, 24)              },
    };

    /// <summary>
    /// Tries to get the reader for the specified type.
    /// </summary>
    /// <typeparam name="T">The type for which to get the reader.</typeparam>
    /// <param name="codec">The reader function for the specified type, if found;
    /// otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the reader for the specified type is found;
    /// otherwise, <c>false</c>.</returns>
    internal static bool TryGetReader<T>(out Func<BitReader, object> codec) {
        if (s_primitiveReaders.TryGetValue(typeof(T), out var func)) {
            codec = func;
            return true;
        }

        codec = null!;
        return false;
    }

    /// <summary>
    /// Tries to get the writer for a specified type.
    /// </summary>
    /// <typeparam name="T">The type for which to get the writer.</typeparam>
    /// <param name="codec">The writer for the specified type, if found;
    /// otherwise, null.</param>
    /// <returns>true if the writer for the specified type is found;
    /// otherwise, false.</returns>
    internal static bool TryGetWriter<T>(out Action<BitWriter, object> codec) {
        if (s_primitiveWriters.TryGetValue(typeof(T), out var func)) {
            codec = func;
            return true;
        }

        codec = null!;
        return false;
    }

}
