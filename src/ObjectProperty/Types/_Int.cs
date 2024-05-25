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

using Imcodec.IO;

namespace Imcodec.ObjectProperty.Types;

/// <summary>
/// Represents a reflected type of <see cref="int"/>.
/// </summary>
internal sealed class _Int() : PropertyType<int> {

    internal override bool Decode(out int value, BitReader reader) {
        // Ensure that reading another 4 bytes is possible.
        var typeSizeInBits = sizeof(int) * 8;
        if (reader.BitPos() + typeSizeInBits > reader.Count() * 8) {
            throw new InvalidOperationException($"Reading another {typeSizeInBits / 8} bytes is not possible.");
        }

        value = reader.Reader.ReadInt32();
        return true;
    }

    internal override bool Encode(int value, BitWriter writer) {
        writer.Writer.Write(value);
        return true;
    }

}
