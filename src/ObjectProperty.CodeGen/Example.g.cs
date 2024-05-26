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

namespace Imcodec.ObjectProperty.CodeGen;

public static class Generation {
    public static PropertyClass Dispatch(uint hash) =>
        hash switch {
            1214710570 => new LootInfoList(),
            461650270 => new GoldLootInfo(),
            1119240234 => new LootInfo(),
            1246183594 => new LootInfoBase(),
            _ => null,
        };
}

public partial class LootInfoList : PropertyClass {

    public override uint GetHash() => 1214710570;

    public GoldLootInfo m_goldInfo { get; set; }
    public List<LootInfo> m_loot { get; set; }

    public LootInfoList() {
        base.Properties.Add(new Property<GoldLootInfo>(461650270, PropertyFlags.Prop_Public, () => m_goldInfo, (val) => m_goldInfo = val));
        base.Properties.Add(new Property<List<LootInfo>>(1119240234, PropertyFlags.Prop_Public, () => m_loot, (val) => m_loot = val));
    }

}

public partial class GoldLootInfo : LootInfo {

    public override uint GetHash() => 461650270;

    public int m_goldAmount { get; set; }

    public GoldLootInfo() {
        base.Properties.Add(new Property<int>(0, PropertyFlags.Prop_Public, () => m_goldAmount, (val) => m_goldAmount = val));
    }

}

public partial class LootInfo : LootInfoBase {

    public override uint GetHash() => 1119240234;

    public LOOT_TYPE m_lootType { get; set; }

    public LootInfo() {
        base.Properties.Add(new Property<LOOT_TYPE>(0, PropertyFlags.Prop_Public, () => m_lootType, (val) => m_lootType = val));
    }

    public enum LOOT_TYPE {

        LOOT_TYPE_NONE = 0,
        LOOT_TYPE_GOLD = 1,
        LOOT_TYPE_MANA = 2,
        LOOT_TYPE_ITEM = 3,
        LOOT_TYPE_TREASURE_CARD = 4,
        LOOT_TYPE_MAGIC_XP = 5,
        LOOT_TYPE_ADD_SPELL = 6,
        LOOT_TYPE_MAX_HEALTH = 7,
        LOOT_TYPE_MAX_GOLD = 8,
        LOOT_TYPE_MAX_MANA = 9,
        LOOT_TYPE_MAX_POTION = 10,
        LOOT_TYPE_TRAINING_POINTS = 11,
        LOOT_TYPE_RECIPE = 12,
        LOOT_TYPE_CRAFTING_SLOT = 13,
        LOOT_TYPE_REAGENT = 14,
        LOOT_TYPE_PETSNACK = 15,
        LOOT_TYPE_GARDENING_XP = 16,
        LOOT_TYPE_PET_XP = 17,
        LOOT_TYPE_TREASURE_TABLE = 18,
        LOOT_TYPE_ARENA_POINTS = 19,
        LOOT_TYPE_ARENA_BONUS_POINTS = 20,
        LOOT_TYPE_GROUP = 21,
        LOOT_TYPE_FISHING_XP = 22,
        LOOT_TYPE_EVENT_CURRENCY_1 = 24,
        LOOT_TYPE_EVENT_CURRENCY_2 = 25,
        LOOT_TYPE_PVP_CURRENCY = 26,
        LOOT_TYPE_PVP_CURRENCY_BONUS = 27,
        LOOT_TYPE_FURNITURE_ESSENCE = 28,

    }

}

public partial class LootInfoBase : PropertyClass {

    public override uint GetHash() => 1246183594;

}
