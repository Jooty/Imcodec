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

namespace Imcodec.Wad;

public sealed class FileEntry {

    public uint Offset { get; init; }
    public uint Size { get; init; }
    public uint CompressedSize { get; init; }
    public bool IsCompressed { get; init; }
    public uint Crc32 { get; init; }
    public string? FileName { get; init; }

    public void PackToStream(BitWriter writer) {
        writer.WriteUInt32(Offset);
        writer.WriteUInt32(Size);
        writer.WriteUInt32(CompressedSize);
        writer.WriteBit(IsCompressed);
        writer.WriteUInt32(Crc32);
        writer.WriteBigString(FileName!);
    }

}
