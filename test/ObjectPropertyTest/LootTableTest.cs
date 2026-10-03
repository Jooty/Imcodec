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

using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;

namespace Imcodec.Test.ObjectPropertyTest;

public class LootTableTest {

    /*
    Loot reward lists contain a little of everything we need to test. They contain a list of types which derive PropertyClass,
    basic struct types, and a ByteString. We will test the serialization and deserialization of a loot table.
    */

    private const string LootTableBlob = "2A0367480100000089876B65050000000000050000005E39841B010000000200000000000000";
    private const string LootTableBlobCompressed = "2600000078DAD3624EF760646060E86CCF4E65650001101967D9220D12656280000067CB0401";

    // Same table with compact lengths: the u32 list count becomes a zero bit and 7 bits of length.
    private const string LootTableBlobCompact = "2A0367480289876B650500000000050000005E39841B010000000200000000000000";

    [Fact]
    public void TryDeserializeLootTableBlob() {
        // Deserialize a loot info list.
        var serializer = new ObjectSerializer(false, SerializerFlags.None);
        var byteBlob = Convert.FromHexString(LootTableBlob);
        var deserializeSuccess = serializer.Deserialize<LootInfoList>(byteBlob, (PropertyFlags) 31, out var lootTable);

        Assert.True(deserializeSuccess);
        Assert.NotNull(lootTable);
        Assert.NotNull(lootTable.m_goldInfo);
        Assert.True(lootTable.m_goldInfo.m_goldAmount == 2);
        Assert.True(lootTable.m_goldInfo.m_lootType == LOOT_TYPE.LOOT_TYPE_GOLD);

        Assert.NotNull(lootTable.m_loot);
        Assert.True(lootTable.m_loot.Count == 1);
        Assert.True(lootTable.m_loot[0] is MagicXPLootInfo);
        Assert.True(lootTable.m_loot[0].m_lootType == LOOT_TYPE.LOOT_TYPE_MAGIC_XP);
    }

    [Fact]
    public void TrySerializeLootTable() {
        // Serialize a loot info list and see if it matches the expected blob.
        var serializer = new ObjectSerializer(false);
        var lootTable = new LootInfoList {
            m_loot = [
                new MagicXPLootInfo { m_lootType = LOOT_TYPE.LOOT_TYPE_MAGIC_XP, m_experience = 5 }
            ],
            m_goldInfo = new GoldLootInfo {
                m_goldAmount = 2,
                m_lootType = LOOT_TYPE.LOOT_TYPE_GOLD
            }
        };

        var serializeSuccess = serializer.Serialize(lootTable, (PropertyFlags) 31, out var byteBlob);
        Assert.True(serializeSuccess);

        var hexBlob = Convert.ToHexString(byteBlob);
        Assert.Equal(LootTableBlob, hexBlob);
    }

    private static LootInfoList CreateLootTable() => new() {
        m_goldInfo = new GoldLootInfo {
            m_goldAmount = 2,
            m_lootType = LOOT_TYPE.LOOT_TYPE_GOLD
        },
        m_loot = [
            new MagicXPLootInfo { m_lootType = LOOT_TYPE.LOOT_TYPE_MAGIC_XP, m_experience = 5 }
        ]
    };

    private static void AssertLootTable(LootInfoList? lootTable) {
        Assert.NotNull(lootTable);
        Assert.NotNull(lootTable.m_goldInfo);
        Assert.True(lootTable.m_goldInfo.m_goldAmount == 2);
        Assert.True(lootTable.m_goldInfo.m_lootType == LOOT_TYPE.LOOT_TYPE_GOLD);

        Assert.NotNull(lootTable.m_loot);
        Assert.True(lootTable.m_loot.Count == 1);
        Assert.True(lootTable.m_loot[0] is MagicXPLootInfo);
        Assert.True(lootTable.m_loot[0].m_lootType == LOOT_TYPE.LOOT_TYPE_MAGIC_XP);
    }

    [Fact]
    public void TrySerializeWithManualCompression() {
        // The manual framing is a length prefix and zlib stream around the plain bytes.
        var serializer = new ObjectSerializer(false, SerializerFlags.None);

        Assert.True(serializer.Serialize(CreateLootTable(), (PropertyFlags) 31, out var byteBlob));

        var compressed = Compression.CompressWithLength((byte[]) byteBlob);
        Assert.Equal(LootTableBlobCompressed, Convert.ToHexString(compressed));
    }

    [Fact]
    public void TryDeserializeWithManualCompression() {
        var serializer = new ObjectSerializer(false, SerializerFlags.None);
        var plain = Compression.DecompressWithLength(Convert.FromHexString(LootTableBlobCompressed));

        Assert.Equal(LootTableBlob, Convert.ToHexString(plain));
        Assert.True(serializer.Deserialize<LootInfoList>(plain, (PropertyFlags) 31, out var lootTable));
        AssertLootTable(lootTable);
    }

    [Fact]
    public void TrySerializeWithCompressFlag() {
        var serializer = new ObjectSerializer(false, SerializerFlags.Compress);

        Assert.True(serializer.Serialize(CreateLootTable(), (PropertyFlags) 31, out var byteBlob));
        Assert.Equal("01" + LootTableBlobCompressed, Convert.ToHexString(byteBlob));

        var reader = new ObjectSerializer(false, SerializerFlags.Compress);
        Assert.True(reader.Deserialize<LootInfoList>(byteBlob, (PropertyFlags) 31, out var lootTable));
        AssertLootTable(lootTable);
    }

    [Fact]
    public void TryDeserializeCompressFlagWithMarkerClear() {
        var serializer = new ObjectSerializer(false, SerializerFlags.Compress);
        var byteBlob = Convert.FromHexString("00" + LootTableBlob);

        Assert.True(serializer.Deserialize<LootInfoList>(byteBlob, (PropertyFlags) 31, out var lootTable));
        AssertLootTable(lootTable);
    }

    [Fact]
    public void TrySerializeFlagsWithCompressFlag() {
        var serializer = new ObjectSerializer(false, SerializerFlags.SerializeFlags | SerializerFlags.Compress);

        Assert.True(serializer.Serialize(CreateLootTable(), (PropertyFlags) 31, out var byteBlob));
        Assert.Equal("0900000001" + LootTableBlobCompressed, Convert.ToHexString(byteBlob));

        var reader = new ObjectSerializer(false, SerializerFlags.SerializeFlags);
        Assert.True(reader.Deserialize<LootInfoList>(byteBlob, (PropertyFlags) 31, out var lootTable));
        AssertLootTable(lootTable);
    }

    [Fact]
    public void TryDecompressWithLengthRawFlag() {
        var raw = Convert.FromHexString(LootTableBlob);
        var framed = BitConverter.GetBytes((uint) raw.Length | 0x80000000u).Concat(raw).ToArray();

        Assert.Equal(raw, Compression.DecompressWithLength(framed));
        Assert.Throws<ArgumentException>(() => Compression.DecompressWithLength([1, 0, 0]));
    }

    [Fact]
    public void TrySerializeWithCompactLengths() {
        // Serialize a loot info list with compact lengths, match the expected blob and read it back.
        var lootTable = new LootInfoList {
            m_loot = [
                new MagicXPLootInfo { m_lootType = LOOT_TYPE.LOOT_TYPE_MAGIC_XP, m_experience = 5 }
            ],
            m_goldInfo = new GoldLootInfo {
                m_goldAmount = 2,
                m_lootType = LOOT_TYPE.LOOT_TYPE_GOLD
            }
        };

        var serializeSuccess = new ObjectSerializer(false, SerializerFlags.CompactLength)
            .Serialize(lootTable, (PropertyFlags) 31, out var byteBlob);
        Assert.True(serializeSuccess);
        Assert.Equal(LootTableBlobCompact, Convert.ToHexString(byteBlob));

        var deserializeSuccess = new ObjectSerializer(false, SerializerFlags.CompactLength)
            .Deserialize<LootInfoList>(byteBlob, (PropertyFlags) 31, out var decoded);
        Assert.True(deserializeSuccess);
        Assert.NotNull(decoded);
        Assert.True(decoded.m_loot.Count == 1);
        Assert.True(decoded.m_goldInfo.m_goldAmount == 2);
    }

    [Fact]
    public void TrySerializeLongListWithCompactLengths() {
        // A list of 128 items or more gets a set bit followed by 31 bits of count.
        var lootTable = new LootInfoList {
            m_loot = [.. Enumerable.Range(0, 200).Select(static i => (LootInfo) new MagicXPLootInfo { m_experience = i })],
            m_goldInfo = new GoldLootInfo { m_goldAmount = 2 }
        };

        var serializeSuccess = new ObjectSerializer(false, SerializerFlags.CompactLength)
            .Serialize(lootTable, (PropertyFlags) 31, out var byteBlob);
        Assert.True(serializeSuccess);
        Assert.Equal("91010000", Convert.ToHexString(byteBlob, 4, 4));

        var deserializeSuccess = new ObjectSerializer(false, SerializerFlags.CompactLength)
            .Deserialize<LootInfoList>(byteBlob, (PropertyFlags) 31, out var decoded);
        Assert.True(deserializeSuccess);
        Assert.Equal(200, decoded!.m_loot.Count);
        Assert.Equal(199, ((MagicXPLootInfo) decoded.m_loot[199]).m_experience);
    }

}
