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

/*

Blob: 2A 03 67 48 01 00 00 00 89 87 6B 65 05 00 00 00 00 00 05 00 00 00 5E 39 84 1B 01 00 00 00 02 00 00 00

Structure:

- LootInfoList
    - m_goldInfo (GoldLootInfo):
        - m_goldAmount: 2
        - m_lootType  : LOOT_TYPE_GOLD
    - m_loot (list) (1) (LootInfo):
        - MagicXPLootInfo:
            - m_experience : 5
            - m_lootType   : LOOT_TYPE_MAGIC_XP
            - m_magicSchool: ""

*/

using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.CodeGen;

namespace Imcodec.Test.ObjectPropertyTest;

public class LootTableTest {

    private const string LOOT_TABLE_BLOB = "2A0367480100000089876B65050000000000050000005E39841B0100000002000000";

    [Fact]
    public void TryDeserializeLootTableBlob() {
        var serializer = new ObjectSerializer();
        var byteBlob = Convert.FromHexString(LOOT_TABLE_BLOB);
        var deserializeSuccess = serializer.Deserialize<LootInfoList>(byteBlob, PropertyFlags.Prop_Public, out var lootTable);

        Assert.True(deserializeSuccess);
        Assert.NotNull(lootTable);
    }

}
