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
namespace Imcodec.Cryptography;

public static class Fnv1a {

    private const uint OFFSET_BASIS = 0x811C9DC5;
    private const uint PRIME = 0x01000193;

    /// <summary>
    /// Computes the 32-bit FNV-1a hash of the data.
    /// </summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The hash.</returns>
    public static uint Hash(ReadOnlySpan<byte> data) {
        var state = OFFSET_BASIS;
        foreach (var b in data) {
            state = (state ^ b) * PRIME;
        }
        return state;
    }
}
