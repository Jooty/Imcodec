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

using System.Diagnostics;
using System.Text;

namespace Imcodec.Strings;

[DebuggerDisplay("{ToString()}")]
public readonly struct WideByteString {

    private readonly byte[] _bytes;

    public WideByteString(byte[] bytes) {
        _bytes = bytes;
    }

    public WideByteString(string str) {
        _bytes = Encoding.Unicode.GetBytes(str);
    }

    public static implicit operator string(WideByteString byteString) {
        return byteString._bytes is null
            ? string.Empty
            : Encoding.Unicode.GetString(byteString._bytes);
    }

    public static implicit operator WideByteString(string str) {
        if (str is null) {
            return new WideByteString();
        }

        return new WideByteString(Encoding.Unicode.GetBytes(str));
    }

    public static implicit operator byte[](WideByteString byteString) {
        return byteString._bytes;
    }

    public static implicit operator WideByteString(byte[] buffer) {
        return new WideByteString(buffer);
    }

    public override string? ToString() {
        return _bytes is null ? null : Encoding.Unicode.GetString(_bytes);
    }

    public int Length => _bytes?.Length ?? 0;

}
