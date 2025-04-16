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

namespace Imcodec.ObjectProperty.CodeGen.Definitions; 

[Flags]
public enum PropertyFlags {

    Prop_None = 0,
    Prop_Save = 1 << 0,
    Prop_Copy = 1 << 1,
    Prop_Public = 1 << 2,
    Prop_Transmit = 1 << 3,
    Prop_AuthorityTransmit = 1 << 4,
    Prop_Persistent = 1 << 5,
    Prop_Deprecated = 1 << 6,
    Prop_NoScript = 1 << 7,
    Prop_DirtyEncode = 1 << 8,
    Prop_Blob = 1 << 9,
    Prop_Immutable = 1 << 16,
    Prop_FileName = 1 << 17,
    Prop_Color = 1 << 18,
    Prop_Bits = 1 << 20,
    Prop_Enum = 1 << 21,
    Prop_Localized = 1 << 22,
    Prop_StringKey = 1 << 23,
    Prop_ObjectId = 1 << 24,
    Prop_ReferenceId = 1 << 25,
    Prop_ObjectName = 1 << 27,
    Prop_HasBaseClass = 1 << 28,

}
