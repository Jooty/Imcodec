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

namespace Imcodec.ObjectProperty.PropertyClass.Types;

/// <summary>
/// Represents a vector of reflected values.
/// </summary>
internal sealed class _Vector<T>() : ReflectedType<T[]> where T : ReflectedType<T> {

    internal override bool Decode(out T[] value, BitReader reader) {
        value = default; //todo
        return true;
    }

    internal override bool Encode(T[] values, BitWriter writer) {
        // todo
        return true;
    }

}
