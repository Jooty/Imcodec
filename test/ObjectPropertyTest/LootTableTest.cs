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

    private const string LootTableBlob = "2A0367480100000089876B65050000000000050000005E39841B0100000002000000";
    private const string LootTableBlobCompressed = "2200000078DAD3624EF760646060E86CCF4E65650001101967D9220D126502620057C70401";

    [Fact]
    public void TryDeserializeLootTableBlob() {
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
            m_goldInfo = new GoldLootInfo {
                m_goldAmount = 2,
                m_lootType = LOOT_TYPE.LOOT_TYPE_GOLD
            },
            m_loot = [
                new MagicXPLootInfo { m_lootType = LOOT_TYPE.LOOT_TYPE_MAGIC_XP, m_experience = 5 }
            ]
        };

        var serializeSuccess = serializer.Serialize(lootTable, (PropertyFlags) 31, out var byteBlob);
        Assert.True(serializeSuccess);
        Assert.True(byteBlob is not null);

        var hexBlob = BitConverter.ToString(byteBlob).Replace("-", "");
        Assert.Equal(LootTableBlob, hexBlob);
    }

    [Fact]
    public void TrySerializeWithCompression() {
        // Serialize a loot info list with compression and see if it matches the expected blob.
        var serializer = new ObjectSerializer(false, SerializerFlags.Compress);
        var lootTable = new LootInfoList {
            m_goldInfo = new GoldLootInfo {
                m_goldAmount = 2,
                m_lootType = LOOT_TYPE.LOOT_TYPE_GOLD
            },
            m_loot = [
                new MagicXPLootInfo { m_lootType = LOOT_TYPE.LOOT_TYPE_MAGIC_XP, m_experience = 5 }
            ]
        };

        var serializeSuccess = serializer.Serialize(lootTable, (PropertyFlags) 31, out var byteBlob);
        Assert.True(serializeSuccess);
        Assert.True(byteBlob is not null);

        var hexBlob = BitConverter.ToString(byteBlob).Replace("-", "");
        Assert.Equal(LootTableBlobCompressed, hexBlob);
    }

    [Fact]
    public void TryDeserializeWithCompression() {
        // Deserialize a compressed loot info list and see if it matches the expected blob.
        var serializer = new ObjectSerializer(false, SerializerFlags.Compress);
        var byteBlob = Convert.FromHexString(LootTableBlobCompressed);
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

}
