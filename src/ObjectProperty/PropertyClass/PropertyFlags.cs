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

namespace Imcodec.ObjectProperty.PropertyClass;

[Flags]
public enum PropertyFlags {

    PF_Save              = 1 << 0,
    PF_Copy              = 1 << 1,
    PF_Public            = 1 << 2,
    PF_Transmit          = 1 << 3,
    PF_AuthorityTransmit = 1 << 4,
    PF_Persistent        = 1 << 5,
    PF_Deprecated        = 1 << 6,
    PF_NoScript          = 1 << 7,
    PF_Encode            = 1 << 8,
    PF_Blob              = 1 << 9,

    PF_Immutable         = 1 << 16,
    PF_FileName          = 1 << 17,
    PF_Color             = 1 << 18,

    PF_Bits              = 1 << 20,
    PF_Enum              = 1 << 21,
    PF_Localized         = 1 << 22,
    PF_StringKey         = 1 << 23,
    PF_ObjectId          = 1 << 24,
    PF_ReferenceId       = 1 << 25,

    PF_ObjectName        = 1 << 27,
    PF_HasBaseClass      = 1 << 28,

}
