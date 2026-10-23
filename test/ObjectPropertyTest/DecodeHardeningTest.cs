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

using System.Buffers.Binary;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;

namespace Imcodec.Test.ObjectPropertyTest;

public class DecodeHardeningTest {

    /*
    Loot info lists carry a list of objects, and requirement lists nest inside themselves. The blob below is the
    uncompressed loot table from LootTableTest: list hash, list length, item hash, then the item and gold info.
    */

    private const string LootTableBlob = "2A0367480100000089876B65050000000000050000005E39841B010000000200000000000000";
    private const int LIST_LENGTH_OFFSET = 4;
    private const int LIST_ITEM_HASH_OFFSET = 8;
    private const uint FORGED_LIST_LENGTH = 0x7FFFFFFF;
    private const PropertyFlags TransmitMask = (PropertyFlags) 31;

    private static byte[] ReadLootTableBlob() => Convert.FromHexString(LootTableBlob);

    private static void WriteUInt32At(byte[] buffer, int offset, uint value)
        => BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset), value);

    private static byte[] SerializeNestedRequirementLists(int depth) {
        var current = new RequirementList();
        for (var level = 1; level < depth; level++) {
            current = new RequirementList { m_requirements = [current] };
        }

        var serializer = new ObjectSerializer(false, SerializerFlags.None) { MaxObjectDepth = int.MaxValue };
        Assert.True(serializer.Serialize(current, TransmitMask, out var blob));

        return blob;
    }

    [Fact]
    public void NormalObjectStillRoundTrips() {
        var serializer = new ObjectSerializer(false, SerializerFlags.None);

        var success = serializer.Deserialize<LootInfoList>(ReadLootTableBlob(), TransmitMask, out var lootTable);

        Assert.True(success);
        Assert.NotNull(lootTable);
        Assert.Single(lootTable.m_loot);
        Assert.IsType<MagicXPLootInfo>(lootTable.m_loot[0]);
    }

    [Fact]
    public void TopLevelHashOfAnotherTypeFails() {
        var serializer = new ObjectSerializer(false, SerializerFlags.None);

        var success = serializer.Deserialize<GoldLootInfo>(ReadLootTableBlob(), TransmitMask, out var output);

        Assert.False(success);
        Assert.Null(output);
    }

    [Fact]
    public void ListItemOfAnotherTypeFails() {
        var blob = ReadLootTableBlob();
        WriteUInt32At(blob, LIST_ITEM_HASH_OFFSET, new RequirementList().GetHash());
        var serializer = new ObjectSerializer(false, SerializerFlags.None);

        var success = serializer.Deserialize<LootInfoList>(blob, TransmitMask, out var output);

        Assert.False(success);
        Assert.Null(output);
    }

    [Fact]
    public void NestingPastMaxDepthFails() {
        var serializer = new ObjectSerializer(false, SerializerFlags.None);
        var blob = SerializeNestedRequirementLists(serializer.MaxObjectDepth + 1);

        var success = serializer.Deserialize<RequirementList>(blob, TransmitMask, out var output);

        Assert.False(success);
        Assert.Null(output);
    }

    [Fact]
    public void NestingAtMaxDepthSucceeds() {
        var serializer = new ObjectSerializer(false, SerializerFlags.None);
        var blob = SerializeNestedRequirementLists(serializer.MaxObjectDepth);

        var success = serializer.Deserialize<RequirementList>(blob, TransmitMask, out var output);

        Assert.True(success);
        Assert.NotNull(output);
    }

    [Fact]
    public void DepthResetsAfterAFailedDecode() {
        var serializer = new ObjectSerializer(false, SerializerFlags.None);
        var tooDeep = SerializeNestedRequirementLists(serializer.MaxObjectDepth + 1);
        var atLimit = SerializeNestedRequirementLists(serializer.MaxObjectDepth);

        Assert.False(serializer.Deserialize<RequirementList>(tooDeep, TransmitMask, out _));
        Assert.True(serializer.Deserialize<RequirementList>(atLimit, TransmitMask, out _));
    }

    [Fact]
    public void ForgedListLengthFails() {
        var blob = ReadLootTableBlob();
        WriteUInt32At(blob, LIST_LENGTH_OFFSET, FORGED_LIST_LENGTH);
        var serializer = new ObjectSerializer(false, SerializerFlags.None);

        var success = serializer.Deserialize<LootInfoList>(blob, TransmitMask, out var output);

        Assert.False(success);
        Assert.Null(output);
    }

    [Fact]
    public void ForgedCompactListLengthFails() {
        var blob = new byte[] { 0x2A, 0x03, 0x67, 0x48, 0xFF, 0xFF, 0xFF, 0xFF };
        var serializer = new ObjectSerializer(false, SerializerFlags.CompactLength);

        var success = serializer.Deserialize<LootInfoList>(blob, TransmitMask, out var output);

        Assert.False(success);
        Assert.Null(output);
    }

    [Fact]
    public void TruncatedBufferFails() {
        var blob = ReadLootTableBlob();

        for (var length = 0; length < blob.Length; length++) {
            var serializer = new ObjectSerializer(false, SerializerFlags.None);

            var success = serializer.Deserialize<LootInfoList>(blob[..length], TransmitMask, out var output);

            Assert.False(success);
            Assert.Null(output);
        }
    }

}
