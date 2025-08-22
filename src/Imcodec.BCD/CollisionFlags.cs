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

namespace Imcodec.BCD;

/// <summary>
/// Attribute flags encoded in Geometry objects.
/// </summary>
[Flags]
public enum CollisionFlags : uint {
    
    None = 0,
    Object = 1 << 0,
    Walkable = 1 << 1,
    Hitscan = 1 << 3,
    LocalPlayer = 1 << 4,
    Water = 1 << 6,
    ClientObject = 1 << 7,
    Trigger = 1 << 8,
    Fog = 1 << 9,
    Goo = 1 << 10,
    Fish = 1 << 11,
    Muck = 1 << 12
    
}