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
using Imcodec.ObjectProperty.Attributes;

namespace Imcodec.ObjectProperty.TypeCache;

public partial class ServerGeneratedTypeRegistry : TypeRegistry {

    // The following overrides are not populated. They are placeholders for the generated code.
    // The generated code will add each type to the dictionary in the constructor.

    private readonly Dictionary<uint, System.Type> _types = [];

    public override System.Type? LookupType(uint hash)
        => _types.TryGetValue(hash, out var type) ? type : null;

    public override void RegisterType(uint hash, System.Type type)
        => _types[hash] = type;

}

[PropertySerializationTarget]
public partial record WizZoneTriggers : PropertyClass {

    public override uint GetHash() => 0x06DAAC43;

    [PropertyField(0x3F1DB764, 31)] public List<Trigger?> m_triggers { get; set; } = [];

}

[PropertySerializationTarget]
public partial record Trigger : PropertyClass {

    public override uint GetHash() => 0x068C265B;

    [PropertyField(0xB8C90C10, 31)] public ByteString m_triggerName { get; set; }
    [PropertyField(0x3933D634, 31)] public uint m_triggerMax { get; set; }
    [PropertyField(0x767AAC3C, 31)] public uint m_cooldown { get; set; }
    [PropertyField(0x2E8B9981, 31)] public uint m_cooldownRand { get; set; }
    [PropertyField(0x3282D78A, 31)] public bool m_pulsar { get; set; }
    [PropertyField(0x7DB09CC1, 31)] public List<ByteString> m_activateEvents { get; set; } = [];
    [PropertyField(0xA7BEADF6, 31)] public List<ByteString> m_fireEvents { get; set; } = [];
    [PropertyField(0x62A2160A, 31)] public List<ByteString> m_deactivateEvents { get; set; } = [];
    [PropertyField(0x5C548D5F, 31)] public List<ByteString> m_unknown { get; set; } = [];
    [PropertyField(0xA955FFA6, 31)] public RequirementList m_requirements { get; set; } = new();
    [PropertyField(0xE11C8ADA, 31)] public ResultList m_results { get; set; } = new();
    [PropertyField(0x794EA0DF, 31)] public uint unknown_uint_3 { get; set; }
    [PropertyField(0x88B9D287, 31)] public ByteString unknown_str_3 { get; set; }
    [PropertyField(0x8177DA98, 31)] public TriggerObjectInfo m_triggerObjInfo { get; set; } = new();

}

[PropertySerializationTarget]
public partial record TriggerObjectBase : CoreObjectInfo {

    // todo: need actual hashes here
    public override uint GetHash() => 0x0000001;

    // Why would a trigger have a location? Isn't this what volumes are for?
    [PropertyField(0x7DB3F828, 31)] public float m_locationX { get; set; }
    [PropertyField(0x7DB3F829, 31)] public float m_locationY { get; set; }
    [PropertyField(0x7DB3F82A, 31)] public float m_locationZ { get; set; }

}

[PropertySerializationTarget]
public partial record TriggerObjectInfo : TriggerObjectBase {

    // todo: need actual hashes here
    public override uint GetHash() => 0x0000002;

}

[PropertySerializationTarget]
public partial record WizZoneVolumes : PropertyClass {

    public override uint GetHash() => 0x1B6EF770;

    [PropertyField(0x884BFB48, 31)] public List<Volume?> m_volumes { get; set; } = [];

}

[PropertySerializationTarget]
public partial record Volume : CoreObjectInfo {

    public override uint GetHash() => 0x1B7B55F6;

    // CoreObjectInfo properties end here.
    [PropertyField(0xC6E6048B, 31)] public ByteString m_volumeName { get; set; }
    [PropertyField(0x7DB3F828, 31)] public float m_locationX { get; set; }
    [PropertyField(0x7DB3F829, 31)] public float m_locationY { get; set; }
    [PropertyField(0x7DB3F82A, 31)] public float m_locationZ { get; set; }
    [PropertyField(0x40183401, 31)] public new ulong m_templateID { get; set; }
    [PropertyField(0x8987B2CC, 31)] public ByteString m_primitiveType { get; set; }
    [PropertyField(0x3AF933DF, 31)] public float m_radius { get; set; }
    [PropertyField(0x2D481539, 31)] public float m_length { get; set; }
    [PropertyField(0x35EBF597, 31)] public float m_width { get; set; }
    [PropertyField(0x3492258C, 31)] public int unknown_int { get; set; }
    [PropertyField(0x3B3CD5DA, 31)] public bool unknown_1 { get; set; }
    [PropertyField(0x71FCB022, 31)] public byte unknown_2 { get; set; }
    [PropertyField(0x8576192E, 31)] public List<ByteString> m_enterEvents { get; set; } = [];
    [PropertyField(0xAB57CF4A, 31)] public List<ByteString> m_exitEvents { get; set; } = [];

}

[PropertySerializationTarget]
public partial record ResTeleport : TypeCache.Result {

    public override uint GetHash() => 228794493;

    public string? m_destinationLoc { get; set; }
    public string? m_destinationZone { get; set; }
    [PropertyField(0x2, 31)] public byte m_exitTeleporter { get; set; }
    [PropertyField(0x3, 31)] public byte m_teleporterTag { get; set; }
    [PropertyField(0x4, 31)] public TeleportType m_teleportType { get; set; }
    [PropertyField(0x5, 31)] public byte m_transitionID { get; set; }

    public enum TeleportType {
        TELEPORT_STATIC,
    }

}

[PropertySerializationTarget]
public partial record ResDisplayText : TypeCache.Result {

    public override uint GetHash() => 0x774C0B33;

    [PropertyField(0x66603160, 31)] public ByteString m_text { get; set; }
    [PropertyField(0x0D1B703C, 31)] public int m_type { get; set; }
    [PropertyField(0x3AF933DF, 31)] public float m_radius { get; set; }
    [PropertyField(0x431157E7, 31)] public float m_locationX { get; set; }
    [PropertyField(0x431157E8, 31)] public float m_locationY { get; set; }
    [PropertyField(0x431157E9, 31)] public float m_locationZ { get; set; }

    [PropertyField(0x2EB6A55F, 31)] public bool m_unknown_bool { get; set; }
    [PropertyField(0x7E84339F, 31)] public bool m_unknown_bool_2 { get; set; }
    [PropertyField(0x57EDA63C, 31)] public bool m_unknown_bool_3 { get; set; }

}

[PropertySerializationTarget]
public partial record ResPlaySound : TypeCache.Result {
    public override uint GetHash() => 0x3C626744;

    [PropertyField(0x444373FA, 31)] public ZoneRouter m_router { get; set; } = new();
    [PropertyField(0x87BA8BE5, 31)] public ByteString m_soundName { get; set; }
    [PropertyField(0x3B9498D7, 31)] public bool m_blocking { get; set; }
    [PropertyField(0x2C2BC314, 31)] public float m_reinteractTime { get; set; }
}

[PropertySerializationTarget]
public partial record ZoneRouter : PropertyClass {

    public override uint GetHash() => 0xDA51FA8;

    [PropertyField(0x12773D2D, 31)] public float m_locX { get; set; }
    [PropertyField(0x12773D2E, 31)] public float m_locY { get; set; }
    [PropertyField(0x12773D2F, 31)] public float m_locZ { get; set; }
    [PropertyField(0xC7FCACAC, 31)] public RoutingType m_routingType { get; set; }
    [PropertyField(0xE36CE99, 31)] public bool m_useLocation { get; set; }
    [PropertyField(0x148E0B6D, 31)] public bool m_useTriggerLocation { get; set; }

    public enum RoutingType {
        ROUTING_ACTOR,
        ROUTING_ZONE,
        ROUTING_PROXIMITY,
    }

}

[PropertySerializationTarget]
public partial record ResPlayCinematic : TypeCache.Result {

    public override uint GetHash() => 16312488;

    [PropertyField(0x9BA8BF49, 134217735)] public ByteString m_cinematicName { get; set; }
    [PropertyField(0x444373FA, 31)] public ZoneRouter m_router { get; set; } = new();
    [PropertyField(0x1D70805C, 31)] public bool m_unknown_bool_1 { get; set; }
    [PropertyField(0x3AAF6E2F, 31)] public bool m_unknown_bool_2 { get; set; }
    [PropertyField(0x61436E16, 31)] public bool m_unknown_bool_3 { get; set; }
    [PropertyField(0x78B7B1EE, 31)] public ByteString m_unknown_string_1 { get; set; }
    [PropertyField(0x3C1B4C58, 31)] public bool m_unknown_bool_4 { get; set; }
    [PropertyField(0x5BB196FF, 31)] public bool m_unknown_bool_5 { get; set; }
    [PropertyField(0x7B00E397, 31)] public ByteString m_unknown_string_2 { get; set; }
    [PropertyField(0x197BBD69, 31)] public bool m_unknown_bool_6 { get; set; }
    [PropertyField(0x4FA58BBA, 31)] public float m_unknown_float_1 { get; set; }
    [PropertyField(0x3DAC4C0A, 31)] public bool m_unknown_bool_7 { get; set; }
    [PropertyField(0x66ECE9B3, 31)] public ByteString m_unknown_string_3 { get; set; }
    [PropertyField(0xA4092DFC, 31)] public bool m_unknown_bool_8 { get; set; }
    [PropertyField(0x61437E16, 31)] public bool m_unknown_bool_9 { get; set; }

}

[PropertySerializationTarget]
public partial record ResActorDialog : TypeCache.Result {

    public override uint GetHash() => 338444955;

    // PropertyField: m_dialogPrefix (unknown)

    [PropertyField(0x7593142F, 7)] public ByteString m_activePersona { get; set; }
    [PropertyField(0xB62CA9E6, 7)] public ByteString m_registryEntry { get; set; }
    [PropertyField(0x4E221FCC, 7)] public ActorDialog m_dialog { get; set; } = new();
    [PropertyField(0x896AB42D, 31)] public ByteString m_quest { get; set; }
    [PropertyField(0x7AA52925, 7)] public bool m_broadcastToZone { get; set; }
    [PropertyField(0x1CBE628E, 7)] public bool m_displayInQuestList { get; set; }
    [PropertyField(0x2A2A1184, 7)] public bool m_oneShot { get; set; }

}

[PropertySerializationTarget]
public partial record ResAddGold : TypeCache.Result {

    public override uint GetHash() => 703672453;

    [PropertyField(0xD142440, 7)] public int m_gold { get; set; }
    [PropertyField(0x896B452E, 7)] public ByteString m_sourceType { get; set; }

}

[PropertySerializationTarget]
public partial record ResAddMagicXP : TypeCache.Result {

    public override uint GetHash() => 1320311385;

    [PropertyField(0x896B452E, 7)] public ByteString m_sourceType { get; set; }
    [PropertyField(0x0, 7)] public int m_experience { get; set; }
    [PropertyField(0x0, 7)] public float m_magicSchool { get; set; }

}

[PropertySerializationTarget]
public partial record ResLoot : TypeCache.Result {

    public override uint GetHash() => 475964190;

    [PropertyField(0x0, 7)] public LootInfoList m_lootTable { get; set; }

}

[PropertySerializationTarget]
public partial record ResPostQuestEvent : TypeCache.Result {

    public override uint GetHash() => 1936095675;

    [PropertyField(0xD036EBFE, 7)] public ByteString m_eventName { get; set; }
    // PropertyField: m_subEventName (unknown)

}

[PropertySerializationTarget]
public partial record ResAddSpell : TypeCache.Result {

    public override uint GetHash() => 1774023420;

    // PropertyField: m_spellName (string)

}

[PropertySerializationTarget]
public partial record ResAddTrainingPoints : TypeCache.Result {

    public override uint GetHash() => 1627552040;

    // PropertyField: m_sourceType (string)
    // PropertyField: m_trainingPoints (int)

}

[PropertySerializationTarget]
public partial record ResModifyEntry : TypeCache.Result {

    public override uint GetHash() => 185712126;

    // PropertyField: m_questName (unknown)

    [PropertyField(0x7A80F14E, 7)] public ByteString m_entryName { get; set; }
    [PropertyField(0x52C8F7DA, 7)] public bool m_isQuestRegistry { get; set; }
    [PropertyField(0x30753FF7, 7)] public int m_value { get; set; }
    [PropertyField(0x65742E4E, 7)] public ByteString m_questName { get; set; }

}

[PropertySerializationTarget]
public partial record ResIncrementEntry : TypeCache.Result {

    public override uint GetHash() => 461317711;

    // PropertyField: m_entryName (string)
    // PropertyField: m_isQuestRegistry (int)
    // PropertyField: m_questName (unknown)

}

[PropertySerializationTarget]
public partial record ResAddMissionDoor : TypeCache.Result {

    public override uint GetHash() => 1202940500;

    // PropertyField: m_advanced (int)
    // PropertyField: m_clientTag (unknown)
    // PropertyField: m_missionDoorLoc (unknown)
    // PropertyField: m_missionDoorTag (string)
    // PropertyField: m_missionDoorZone (unknown)
    // PropertyField: m_useQuestAsOriginator (int)

}

[PropertySerializationTarget]
public partial record ResItemLoot : TypeCache.Result {

    public override uint GetHash() => 1989908347;

    // PropertyField: m_itemTemplateID (int)
    // PropertyField: m_lootOptions (string)
    // PropertyField: m_sendLootMessage (int)
    // PropertyField: m_sourceType (string)

}

[PropertySerializationTarget]
public partial record ResAddMaxGold : TypeCache.Result {

    public override uint GetHash() => 1875038822;

    // PropertyField: m_maxGoldToAdd (int)

}

[PropertySerializationTarget]
public partial record ResDeleteItem : TypeCache.Result {

    public override uint GetHash() => 822278902;

    // PropertyField: m_itemTemplateID (int)
    // PropertyField: m_quantity (int)
    // PropertyField: m_sourceType (string)

}

[PropertySerializationTarget]
public partial record ResAddHealth : TypeCache.Result {

    public override uint GetHash() => 794463279;

    // PropertyField: m_healthFlat (int)
    // PropertyField: m_healthPercent (float)
    // PropertyField: m_useFlat (int)

}

[PropertySerializationTarget]
public partial record ResPostEvent : TypeCache.Result {

    public override uint GetHash() => 412163800;

    // PropertyField: m_eventName (string)
    // PropertyField: m_subEventName (unknown) // Deserialization fails if this is made a string. 72 bits

    [PropertyField(0xD036EBFE, 31)] public string m_eventName { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResRemoveDynaMod : TypeCache.Result {

    public override uint GetHash() => 552459800;

    [PropertyField(0x5F777B42, 31)] public ByteString m_dynaModClientTag { get; set; }
    [PropertyField(0x42302844, 31)] public bool m_useQuestAsOriginator { get; set; }

}

[PropertySerializationTarget]
public partial record ResAddDynaMod : TypeCache.Result {

    public override uint GetHash() => 3938346;

    [PropertyField(0x5F777B42, 31)] public ByteString m_dynaModClientTag { get; set; }
    [PropertyField(0x42302844, 31)] public bool m_dynaModRemove { get; set; }
    [PropertyField(0x5911401D, 31)] public bool m_useQuestAsOriginator { get; set; }
    [PropertyField(0x7B8D4408, 31)] public ByteString m_dynaModState { get; set; }
    [PropertyField(0x816963F8, 8388615)] public ByteString m_zoneName { get; set; }

}

[PropertySerializationTarget]
public partial record ResWait : TypeCache.Result {

    public override uint GetHash() => 526762782;

    [PropertyField(0x8F3C69B4, 31)] public uint m_secondsToWait { get; set; }

}

[PropertySerializationTarget]
public partial record ResEmote : TypeCache.Result {

    public override uint GetHash() => 475497368;

    // PropertyField: m_emoteName (string)
    // PropertyField: m_emoteState (string)
    // PropertyField: m_loop (int)
    // PropertyField: m_particleAsset (unknown)
    // PropertyField: m_particleNode (unknown)
    // PropertyField: m_personaName (unknown)
    // PropertyField: m_soundAsset (unknown)
    // PropertyField: m_usePersona (int)
    // PropertyField: m_useTarget (int)

}

[PropertySerializationTarget]
public partial record ResAddEncounterXP : TypeCache.Result {

    public override uint GetHash() => 798482874;

    // PropertyField: m_experience (int)

}

[PropertySerializationTarget]
public partial record ResMarkZoneNoWarn : TypeCache.Result {

    public override uint GetHash() => 1534247075;


}

[PropertySerializationTarget]
public partial record ResDownloadPackage : TypeCache.Result {

    public override uint GetHash() => 807468757;

    [PropertyField(0x8A9FF1A3, 31)] public List<ByteString>? m_packageList { get; set; }

}

[PropertySerializationTarget]
public partial record ResAddTreasureSpell : TypeCache.Result {

    public override uint GetHash() => 1787561491;

    // PropertyField: m_sourceType (string)
    // PropertyField: m_spellName (string)

}

[PropertySerializationTarget]
public partial record ResModifyTriggerObject : TypeCache.Result {

    public override uint GetHash() => 1263108441;

    [PropertyField(0xC6E6048B, 31)] public ByteString m_triggerObjName { get; set; }
    [PropertyField(0x7B3D75AB, 31)] public bool m_triggerObjState { get; set; }

}

[PropertySerializationTarget]
public partial record ResSpawn : TypeCache.Result {

    public override uint GetHash() => 723600258;

    // Not confident about these types
    [PropertyField(0x585139AE, 31)] public ulong m_spawnID { get; set; }
    [PropertyField(0x87ECDC4, 31)] public bool m_activate { get; set; }
    public ulong templateID { get; set; }
    public List<NodeObject>? nodes;
}

[PropertySerializationTarget]
public partial record ResReInteract : TypeCache.Result {

    public override uint GetHash() => 682171658;

    // PropertyField: m_actorType (string)
    // PropertyField: m_delay (int)
    // PropertyField: m_personaName (string)
    // PropertyField: m_source (int)

}

[PropertySerializationTarget]
public partial record ResDownloadElement : TypeCache.Result {

    public override uint GetHash() => 101365432;

    // PropertyField: m_elementPackageList (string)

}

[PropertySerializationTarget]
public partial record ResRemoveTriggerObject : TypeCache.Result {

    public override uint GetHash() => 803366521;

    // PropertyField: m_triggerObjName (string)

}

[PropertySerializationTarget]
public partial record ResCompleteQuestGoal : TypeCache.Result {

    public override uint GetHash() => 1279893472;

    // PropertyField: m_goalName (string)
    // PropertyField: m_questName (string)

}

[PropertySerializationTarget]
public partial record ResAddBadge : TypeCache.Result {

    public override uint GetHash() => 2041850521;

    // PropertyField: m_badgeName (string)

}

[PropertySerializationTarget]
public partial record ResAddEncounter : TypeCache.Result {

    public override uint GetHash() => 798724538;

    // PropertyField: m_experience (int)

}

[PropertySerializationTarget]
public partial record ResDespawn : TypeCache.Result {

    public override uint GetHash() => 1383450208;

    [PropertyField(0x585139AE, 31)] public ulong m_spawnID { get; set; }
    [PropertyField(0x0, 31)] public bool m_despawnEffect { get; set; }
    [PropertyField(0x40183401, 31)] public new ulong m_templateID { get; set; }

}

[PropertySerializationTarget]
public partial record ResRemoveEntry : TypeCache.Result {

    public override uint GetHash() => 1874483934;

    // PropertyField: m_entryName (string)
    // PropertyField: m_isQuestRegistry (int)
    // PropertyField: m_questName (unknown)

}

[PropertySerializationTarget]
public partial record ResCinematicActor : TypeCache.Result {

    public override uint GetHash() => 16312488;

    // PropertyField: m_blocking (int)
    // PropertyField: m_cinematicName (string)
    // PropertyField: m_endAtActor (int)
    // PropertyField: m_endAtTargetActor (int)
    // PropertyField: m_endLoc (string)
    // PropertyField: m_objectTemplateID (int)
    // PropertyField: m_router (string)
    // PropertyField: m_routing (string)
    // PropertyField: m_startAtActor (int)
    // PropertyField: m_startAtTargetActor (int)
    // PropertyField: m_startLoc (string)
    // PropertyField: m_unique (int)
    // PropertyField: m_uniqueBusyMsg (unknown)
    // PropertyField: m_uniqueName (unknown)

}

[PropertySerializationTarget]
public partial record ResFallthrough : TypeCache.Result {

    public override uint GetHash() => 1962253934;

    // PropertyField: m_options (string)

}

[PropertySerializationTarget]
public partial record ResultOption : TypeCache.Result {

    public override uint GetHash() => 1471095109;

    // PropertyField: m_requirements (string)
    // PropertyField: m_results (string)

}

[PropertySerializationTarget]
public partial record ResMaxPotions : TypeCache.Result {

    public override uint GetHash() => 309059205;

    // PropertyField: m_potionsToAdd (int)
    // PropertyField: m_sourceType (string)

}

[PropertySerializationTarget]
public partial record ResRemoveMissionDoor : TypeCache.Result {

    public override uint GetHash() => 1714192944;

    // PropertyField: m_missionDoorTag (string)
    // PropertyField: m_useQuestAsOriginator (int)

}

[PropertySerializationTarget]
public partial record ResAddCraftingSlot : TypeCache.Result {

    public override uint GetHash() => 633964307;

    // PropertyField: m_slotDelta (int)
    // PropertyField: m_sourceType (string)

}

[PropertySerializationTarget]
public partial record ResTimeStampEntry : TypeCache.Result {

    public override uint GetHash() => 1826857186;

    // PropertyField: m_coolDownTime (int)
    // PropertyField: m_registryEntry (string)

}

[PropertySerializationTarget]
public partial record ResToggleQuestEffect : TypeCache.Result {

    public override uint GetHash() => 1383058250;

    // PropertyField: m_addEffect (int)
    // PropertyField: m_effectName (string)

}

[PropertySerializationTarget]
public partial record ResDespawnLeashedObject : TypeCache.Result {

    public override uint GetHash() => 577427349;

    // PropertyField: m_followerTemplateID (int)

}

[PropertySerializationTarget]
public partial record ResRemoveEffect : TypeCache.Result {

    public override uint GetHash() => 837967761;

    // PropertyField: m_effectName (string)
    // PropertyField: m_useOriginatorID (int)

}

[PropertySerializationTarget]
public partial record ResSpawnLeashedObject : TypeCache.Result {

    public override uint GetHash() => 1505576005;

    // PropertyField: m_followerTemplateID (int)

}

[PropertySerializationTarget]
public partial record ResAddEffect : TypeCache.Result {

    public override uint GetHash() => 53623319;

    // PropertyField: m_effectName (unknown)
    // PropertyField: m_playerOnly (int)
    // PropertyField: m_spEffectInfo (string)

}

[PropertySerializationTarget]
public partial record ResAddTriggerObject : TypeCache.Result {

    public override uint GetHash() => 1893729846;

    // PropertyField: m_triggerObjName (string)
    // PropertyField: m_triggerObjState (string)

}

[PropertySerializationTarget]
public partial record ResSetPips : TypeCache.Result {

    public override uint GetHash() => 1375195018;

    // PropertyField: m_numPips (int)
    // PropertyField: m_subCircle (int)

}

[PropertySerializationTarget]
public partial record ResClearHand : TypeCache.Result {

    public override uint GetHash() => 1100956089;

    // PropertyField: m_subCircle (int)

}

[PropertySerializationTarget]
public partial record ResGiveSpell : TypeCache.Result {

    public override uint GetHash() => 151650171;

    // PropertyField: m_spellName (string)
    // PropertyField: m_subCircle (int)

}

[PropertySerializationTarget]
public partial record ResUpdatePips : TypeCache.Result {

    public override uint GetHash() => 266932982;

}

[PropertySerializationTarget]
public partial record ResAddEnergy : TypeCache.Result {

    public override uint GetHash() => 43418311;

    // PropertyField: m_energyFlat (int)
    // PropertyField: m_energyPercent (int)
    // PropertyField: m_sourceType (string)
    // PropertyField: m_useFlat (int)

}

[PropertySerializationTarget]
public partial record ResAddElixir : TypeCache.Result {

    public override uint GetHash() => 66638275;

    // PropertyField: m_sourceType (string)
    // PropertyField: m_templateID (int)

}

[PropertySerializationTarget]
public partial record ResGivePowerPip : TypeCache.Result {

    public override uint GetHash() => 1254124953;

    // PropertyField: m_subCircle (int)

}

[PropertySerializationTarget]
public partial record ResSetGardeningLevel : TypeCache.Result {

    public override uint GetHash() => 875336888;

    // PropertyField: m_level (int)

}

[PropertySerializationTarget]
public partial record ResDownloadBrowser : TypeCache.Result {

    public override uint GetHash() => 2128108307;

}

[PropertySerializationTarget]
public partial record ResSetFishingLevel : TypeCache.Result {

    public override uint GetHash() => 638582544;

    // PropertyField: m_level (int)
}

[PropertySerializationTarget]
public partial record ResAddMana : TypeCache.Result {

    public override uint GetHash() => 1980688359;

    [PropertyField(0x896B452E, 7)] public ByteString m_sourceType { get; set; }
    [PropertyField(0x72D0951E, 7)] public int m_manaFlat { get; set; }
    [PropertyField(0x4D1B12C5, 7)] public float m_manaPercent { get; set; }
    [PropertyField(0x847CA76, 7)] public int m_overfill { get; set; }
    [PropertyField(0x7998C787, 7)] public bool m_useFlat { get; set; }

}

[PropertySerializationTarget]
public partial record ResClearSpellbook : TypeCache.Result {

    public override uint GetHash() => 1887868329;

}

[PropertySerializationTarget]
public partial record ResClearExperience : TypeCache.Result {

    public override uint GetHash() => 1952853362;

}

[PropertySerializationTarget]
public partial record ResShowGUI : TypeCache.Result {

    public override uint GetHash() => 742393364;

    [PropertyField(0x87918E6B, 31)] public ByteString m_guiDisplay { get; set; }
    [PropertyField(0x0, 31)] public ByteString m_guiFile { get; set; }

}

[PropertySerializationTarget]
public partial record ResCinematic : TypeCache.Result {

    public override uint GetHash() => 82637767;

    // PropertyField: m_blocking (int)
    // PropertyField: m_cinematicName (string)
    // PropertyField: m_endAtActor (int)
    // PropertyField: m_endAtTargetActor (int)
    // PropertyField: m_endLoc (string)
    // PropertyField: m_objectTemplateID (int)
    // PropertyField: m_router (string)
    // PropertyField: m_routing (string)
    // PropertyField: m_startAtActor (int)
    // PropertyField: m_startAtTargetActor (int)
    // PropertyField: m_startLoc (string)
    // PropertyField: m_unique (int)
    // PropertyField: m_uniqueBusyMsg (unknown)
    // PropertyField: m_uniqueName (unknown)

}

[PropertySerializationTarget]
public partial record ResAddRecipe : TypeCache.Result {

    public override uint GetHash() => 1895914322;

    // PropertyField: m_recipeName (string)
    // PropertyField: m_sourceType (string)

}

[PropertySerializationTarget]
public partial record ResControlBackgroundMusic : TypeCache.Result {

    public override uint GetHash() => 1144211986;

    // PropertyField: m_action (string)
    // PropertyField: m_fadeTime (int)
    // PropertyField: m_router (string)

    [PropertyField(0x67633059, 31)] public ByteString m_action { get; set; }

}

[PropertySerializationTarget]
public partial record ResUnlockShadowMagic : TypeCache.Result {

    public override uint GetHash() => 513283589;

}

[PropertySerializationTarget]
public partial record ResStartStagedCinematic : TypeCache.Result {

    public override uint GetHash() => 145615551;

    // PropertyField: m_bIncludeAllPlayersInZone (int)
    // PropertyField: m_cinematicName (string)
    // PropertyField: m_stageName (string)

}

[PropertySerializationTarget]
public partial record ResReduceMana : TypeCache.Result {

    public override uint GetHash() => 74783451;

    // PropertyField: m_manaPercent (float)

}

[PropertySerializationTarget]
public partial record ResInitiateCombat : TypeCache.Result {

    public override uint GetHash() => 1486342711;

    // PropertyField: m_aggroActor (int)
    // PropertyField: m_aggroRadius (int)
    // PropertyField: m_aggroTarget (int)
    // PropertyField: m_allPlayers (bool)
    // PropertyField: m_sigilLabel (string)

}

[PropertySerializationTarget]
public partial record CombatSigilObjectInfo : CoreObjectInfo {

    public override uint GetHash() => 478486736;

    // Properties here are listed in order of their understanding.
    // The template for this object is a DynamicTriggerTemplate.

    [PropertyField(0x7B91Df78, 31)] public ByteString m_sigilType { get; set; }
    [PropertyField(0xADC3A56F, 31)] public ByteString m_zoneTag2 { get; set; }
    [PropertyField(0x3AF933DF, 31)] public float m_radius { get; set; }
    [PropertyField(0x595FC144, 31)] public int m_firstTeamToAct { get; set; }
    [PropertyField(0x4AFCF400, 2097183)] public SigilInitiativeSwitchMode m_initiativeSwitchMode { get; set; }
    [PropertyField(0x203340FD, 31)] public int m_initiativeSwitchRounds { get; set; }
    [PropertyField(0x975DE361, 268435463)] public List<ByteString> m_lootTable { get; set; } = [];
    [PropertyField(0x5DB0B6E8, 31)] public bool m_disableTimer { get; set; }
    [PropertyField(0x7DB09CC1, 31)] public List<ByteString> m_activateEvents { get; set; } = [];
    [PropertyField(0x62A2160A, 31)] public List<ByteString> m_deactivateEvents { get; set; } = [];

    // HASH : 0x71FCB022
    // SIZE : 65 bits
    // Very confident about the type.
    [PropertyField(0x71FCB022, 31)] public bool m_unknown_boolean_1 { get; set; }

    // HASH : 0x3BF5B2D
    // SIZE : 65 bits
    // Very confident about the type.
    [PropertyField(0x3BF5B2D, 31)] public bool m_unknown_boolean_2 { get; set; }

    // HASH : 0x3C345132
    // SIZE : 72 bits
    [PropertyField(0x3C345132, 31)] public bool m_unknown_boolean_3 { get; set; }

    // HASH : 0x61B4E11E
    // SIZE : 72 bits
    [PropertyField(0x61B4E11E, 31)] public bool m_unknown_boolean_4 { get; set; }

    // HASH : 0x37BEB1CF
    // SIZE : 8
    [PropertyField(0x37BEB1CF, 31)] public int m_unknown_uint_2 { get; set; }

    // HASH : 0x62794D39
    // SIZE : 8
    [PropertyField(0x62794D39, 31)] public uint m_unknown_uint_3 { get; set; }

    // HASH : 0x6FA14D24
    // SIZE : 8
    [PropertyField(0x6FA14D24, 31)] public uint m_unknown_uint_4 { get; set; }

}

[PropertySerializationTarget]
public partial record WizBangPriorityTemplate : PropertyClass {

    public override uint GetHash() => 511049413;

    [PropertyField(0x7769A117, 31)] public List<ByteString> m_priorityList { get; set; } = [];

}

[PropertySerializationTarget]
public partial record UnknownSpawnObjectInfo : SpawnObjectInfo {

    public override uint GetHash() => 1839222684;

    [PropertyField(0x975DE361, 268435463)] public List<ByteString> m_lootTable { get; set; } = [];
}

[PropertySerializationTarget]
public partial record MobDeckBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 1451865413;

    [PropertyField(0xA03E1197, 268435463)] public List<ByteString> m_spellList { get; set; } = [];

}

[PropertySerializationTarget]
public partial record MinigameSigilInfo : CoreObjectInfo {

    public override uint GetHash() => 234614075;

    [PropertyField(0x7B91Df78, 31)] public ByteString m_sigilType { get; set; }
    [PropertyField(0xADC3A56F, 31)] public ByteString m_zoneTag2 { get; set; }
    [PropertyField(0x3AF933DF, 31)] public float m_radius { get; set; }
    [PropertyField(0x71FCB022, 31)] public byte unknown_2 { get; set; }
    [PropertyField(0x3C345132, 31)] public bool m_unknown_boolean_3 { get; set; }
    [PropertyField(0x7DB09CC1, 31)] public List<ByteString> m_activateEvents { get; set; } = [];
    [PropertyField(0x62A2160A, 31)] public List<ByteString> m_deactivateEvents { get; set; } = [];
    [PropertyField(0x3BF5B2D, 31)] public bool m_unknown_boolean_2 { get; set; }
    [PropertyField(0xA955FFA6, 31)] public RequirementList m_requirements { get; set; } = new();

    // HASH : 0xC8C87586
    // SIZE : 87 bits

    // HASH : 0x842FF241
    // SIZE : 80 bits

    // HASH : 0x616A2C5E
    // SIZE : 80 bits

    // HASH : 0x5A66B8B0
    // SIZE: 432 bits

    // HASH : 0x916BBDD2
    // SIZE: 80 bits

    // HASH : 0x996A9C1F
    // SIZE : 80 bits

    // HASH : 0x8BAF14A0
    // SIZE: 440 bits

    // HASH : 0xAA85756F
    // SIZE: 96 bits

    // HASH : 0x24A5AA8F
    // SIZE : 96 bits

    // HASH : 0x3741C8F6
    // SIZE : 65 bits

    // HASH : 0x8D357D29
    // SIZE : 232 bits

    // HASH : 0x721F110C
    // SIZE : 80 bits

    // HASH : 0x23716AB9
    // SIZE : 96 bits

    // HASH : 0x8976BB77
    // SIZE : 80 bits

    // HASH : 0x9FC5C046
    // SIZE : 80 bits

    // HASH : 0xBD76D4A6
    // SIZE : 80 bits

    // HASH : 0xD12A0AB9
    // SIZE : 80 bits

    // HASH : 0x382DCB82
    // SIZE : 96 bits

    // HASH : 0x9C15DD8E
    // SIZE : 96 bits

    // HASH : 0x492B6BF1
    // SIZE : 65 bits

}