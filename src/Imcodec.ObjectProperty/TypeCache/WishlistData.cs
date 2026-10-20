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
using Imcodec.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Imcodec.ObjectProperty.TypeCache;

public sealed class WishlistData {

    public const uint Magic = 0xFACEACE7;
    public const uint Version = 1;

    public static ByteString Empty { get; } = new WishlistData().ToByteString();

    public bool KeepOnWishlistAfterPurchase { get; set; } = true;
    public List<uint> Items { get; set; } = [];

    public void Encode(BitWriter writer) {
        writer.WriteUInt32(Magic);
        writer.WriteUInt32(Version);
        writer.WriteUInt16(KeepOnWishlistAfterPurchase ? (ushort) 1 : (ushort) 0);

        WriteCount(writer, Items.Count);

        foreach (var item in Items) {
            writer.WriteUInt32(item);
            writer.WriteUInt16(0);
        }
    }

    public static bool TryDecode(BitReader reader, [NotNullWhen(true)] out WishlistData? data) {
        data = null;

        try {
            if (reader.ReadUInt32() != Magic || reader.ReadUInt32() != Version) {
                return false;
            }

            var decoded = new WishlistData {
                KeepOnWishlistAfterPurchase = reader.ReadUInt16() != 0
            };

            var count = reader.ReadUInt16();
            decoded.Items = new List<uint>(count);

            for (var i = 0; i < count; i++) {
                var templateId = reader.ReadUInt32();
                _ = reader.ReadUInt16();
                decoded.Items.Add(templateId);
            }

            data = decoded;
            return true;
        } catch (Exception) {
            return false;
        }
    }

    public ByteString ToByteString() {
        using var writer = new BitWriter();
        Encode(writer);

        return writer.GetData();
    }

    public static bool TryFromByteString(ByteString blob, [NotNullWhen(true)] out WishlistData? data) {
        if (blob.IsEmpty) {
            data = new WishlistData();
            return true;
        }

        using var reader = new BitReader(blob);
        return TryDecode(reader, out data);
    }

    public static bool TryRemoveItem(ByteString blob, uint templateId, out ByteString updatedBlob) {
        updatedBlob = blob;

        if (!TryFromByteString(blob, out var data) || data.Items.RemoveAll(id => id == templateId) == 0) {
            return false;
        }

        updatedBlob = data.ToByteString();
        return true;
    }

    private static void WriteCount(BitWriter writer, int count) {
        if (count > ushort.MaxValue) {
            throw new InvalidOperationException($"A Wishlist holds {count} entries, at most {ushort.MaxValue} fit its u16 count.");
        }

        writer.WriteUInt16((ushort) count);
    }
}
