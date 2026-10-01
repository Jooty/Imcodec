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
using Imcodec.Math;
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

/// <summary>
/// The object a trigger owns (a door, a collision volume, a lever): a core object info the game places
/// when the trigger activates. Live data always carries a class hash for it.
/// </summary>
[PropertySerializationTarget]
public partial record TriggerObjectInfo : CoreObjectInfo {

    public override uint GetHash() => 0x2147D190;

    // Like Volume: the live data carries the location as three floats and the template id under the
    // gid hash, besides the CoreObjectInfo properties, plus the object's own name (a DynaTrigger_* instance).
    [PropertyField(0x7DB3F828, 31)] public float m_locationX { get; set; }
    [PropertyField(0x7DB3F829, 31)] public float m_locationY { get; set; }
    [PropertyField(0x7DB3F82A, 31)] public float m_locationZ { get; set; }
    [PropertyField(0x40183401, 31)] public new ulong m_templateID { get; set; }
    [PropertyField(0xC6E6048B, 31)] public ByteString m_triggerObjName { get; set; }

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
public partial record ResActorDialog : TypeCache.Result {

    public override uint GetHash() => 338444955;

    [PropertyField(0xBEFF7179, 31)] public string m_dialogPrefix { get; set; } = "";

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
    [PropertyField(0xC49A5E28, 31)] public string m_subEventName { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResAddSpell : TypeCache.Result {

    public override uint GetHash() => 1774023420;

    public uint m_templateID { get; set; }
    [PropertyField(0xA03F077C, 31)] public string m_spellName { get; set; } = "";

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

    [PropertyField(0x65742E4E, 31)] public string m_questName { get; set; } = "";

    [PropertyField(0x7A80F14E, 7)] public ByteString m_entryName { get; set; }
    [PropertyField(0x52C8F7DA, 7)] public bool m_isQuestRegistry { get; set; }
    [PropertyField(0x30753FF7, 7)] public int m_value { get; set; }

}

[PropertySerializationTarget]
public partial record ResIncrementEntry : TypeCache.Result {

    public override uint GetHash() => 461317711;

    [PropertyField(0x7A80F14E, 31)] public string m_entryName { get; set; } = "";
    [PropertyField(0x52C8F7DA, 31)] public bool m_isQuestRegistry { get; set; }
    [PropertyField(0x65742E4E, 31)] public string m_questName { get; set; } = "";

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

    [PropertyField(0x75F095D0, 31)] public ulong m_itemTemplateID { get; set; }
    // PropertyField: m_lootOptions (string)
    [PropertyField(0x7C57B320, 31)] public bool m_sendLootMessage { get; set; }
    [PropertyField(0x896B452E, 31)] public string m_sourceType { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResAddMaxGold : TypeCache.Result {

    public override uint GetHash() => 1875038822;

    // PropertyField: m_maxGoldToAdd (int)

}

[PropertySerializationTarget]
public partial record ResDeleteItem : TypeCache.Result {

    public override uint GetHash() => 822278902;

    [PropertyField(0x75F095D0, 31)] public ulong m_itemTemplateID { get; set; }
    [PropertyField(0x0A160539, 31)] public int m_quantity { get; set; }
    [PropertyField(0x896B452E, 31)] public string m_sourceType { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResAddHealth : TypeCache.Result {

    public override uint GetHash() => 794463279;

    [PropertyField(0x5BA99137, 31)] public int m_healthFlat { get; set; }
    [PropertyField(0x3130443E, 31)] public float m_healthPercent { get; set; }
    [PropertyField(0x7998C787, 31)] public bool m_useFlat { get; set; }

}

[PropertySerializationTarget]
public partial record ResPostEvent : TypeCache.Result {

    public override uint GetHash() => 412163800;

    [PropertyField(0xD036EBFE, 31)] public string m_eventName { get; set; } = "";
    [PropertyField(0xC49A5E28, 31)] public string m_subEventName { get; set; } = "";


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
    // Two more flags the data carries; their purpose is not known.
    [PropertyField(0x0216BD40, 31)] public bool unknown_bool_0216BD40 { get; set; }
    [PropertyField(0x67EAA06D, 31)] public bool unknown_bool_67EAA06D { get; set; }

}

[PropertySerializationTarget]
public partial record ResWait : TypeCache.Result {

    public override uint GetHash() => 526762782;

    [PropertyField(0x8F3C69B4, 31)] public double m_secondsToWait { get; set; }

}

[PropertySerializationTarget]
public partial record ResEmote : TypeCache.Result {

    public override uint GetHash() => 475497368;

    [PropertyField(0x909C53D6, 31)] public string m_emoteName { get; set; } = "";
    [PropertyField(0x7BBDAE56, 31)] public string m_emoteState { get; set; } = "";
    [PropertyField(0x0D3CBEAD, 31)] public bool m_loop { get; set; }
    [PropertyField(0x810B576F, 31)] public string m_particleAsset { get; set; } = "";
    [PropertyField(0xCAFE1ED5, 31)] public string m_particleNode { get; set; } = "";
    [PropertyField(0x95DF7F14, 31)] public string m_personaName { get; set; } = "";
    [PropertyField(0xD55CED84, 31)] public string m_soundAsset { get; set; } = "";
    [PropertyField(0x129FF778, 31)] public bool m_usePersona { get; set; }
    [PropertyField(0x3C2F28A7, 31)] public bool m_useTarget { get; set; }

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
    // The state the object is put into (for example "Idle_Open"). Live sends a string here; it was a bool.
    [PropertyField(0x7B3D75AB, 31)] public ByteString m_triggerObjState { get; set; }

}

/// <summary>
/// A trigger requirement that holds while the named zone object is in the given state
/// (for example "SnakeObelisk" in "Idle_On").
/// </summary>
[PropertySerializationTarget]
public partial record ReqTriggerObjectState : Requirement {

    public override uint GetHash() => 0x1B314198;

    [PropertyField(0xC6E6048B, 31)] public ByteString m_triggerObjName { get; set; }
    [PropertyField(0x7B3D75AB, 31)] public ByteString m_triggerObjState { get; set; }

}

/// <summary>
/// A trigger requirement on a Monster_Killed event: the killed monster's template carries the
/// adjective (for example "KrokTut-Ice-KTBoss-L04.AdjRef").
/// </summary>
[PropertySerializationTarget]
public partial record ReqMonsterKilled : Requirement {

    public override uint GetHash() => 0x6CDC00F6;

    // Unmapped meaning; observed false or true across the client data.
    [PropertyField(0x64407CD8, 31)] public bool m_flag { get; set; }
    // The data holds a list of adjectives (count 1, then the text); it was read as one string before.
    [PropertyField(0xBE731606, 31)] public List<string> m_monsterAdjectives { get; set; } = [];
    /// <summary>The first adjective, for callers that match one name.</summary>
    public string m_monsterAdjective => m_monsterAdjectives.Count > 0 ? m_monsterAdjectives[0] : "";

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
    [PropertyField(0xA04FFEBC, 31)] public double m_delay { get; set; }
    [PropertyField(0x95DF7F14, 31)] public string m_personaName { get; set; } = "";
    [PropertyField(0x391AA9C4, 31)] public bool m_source { get; set; }

}

[PropertySerializationTarget]
public partial record ResDownloadElement : TypeCache.Result {

    public override uint GetHash() => 101365432;

    [PropertyField(0x924B0AAD, 31)] public string m_elementPackageList { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResRemoveTriggerObject : TypeCache.Result {

    public override uint GetHash() => 803366521;

    [PropertyField(0xC6E6048B, 31)] public string m_triggerObjName { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResCompleteQuestGoal : TypeCache.Result {

    public override uint GetHash() => 1279893472;

    [PropertyField(0x5AB2329F, 31)] public string m_goalName { get; set; } = "";
    [PropertyField(0x65742E4E, 31)] public string m_questName { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResAddBadge : TypeCache.Result {

    public override uint GetHash() => 2041850521;

    [PropertyField(0xB2FD59CF, 31)] public string m_badgeName { get; set; } = "";

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

    [PropertyField(0x7A80F14E, 31)] public string m_entryName { get; set; } = "";
    [PropertyField(0x52C8F7DA, 31)] public bool m_isQuestRegistry { get; set; }
    [PropertyField(0x65742E4E, 31)] public string m_questName { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResCinematicActor : TypeCache.Result {

    public override uint GetHash() => 16312488;

    // Hashes are StringHash.HashPropertyName(name, type), in the order the client's zone triggers write them.
    [PropertyField(0x9BA8BF49, 134217735)] public ByteString m_cinematicName { get; set; }
    [PropertyField(0x444373FA, 31)] public ZoneRouter m_router { get; set; } = new();
    [PropertyField(0x1D70805C, 31)] public bool m_blocking { get; set; }
    [PropertyField(0x3AAF6E2F, 31)] public bool m_startAtActor { get; set; }
    [PropertyField(0x61437E16, 31)] public bool m_startAtTargetActor { get; set; }
    [PropertyField(0x78B7B1EE, 31)] public Vector3 m_startLoc { get; set; }
    [PropertyField(0x3C1B4C58, 31)] public bool m_endAtActor { get; set; }
    [PropertyField(0x5BB196FF, 31)] public bool m_endAtTargetActor { get; set; }
    [PropertyField(0x7B00E397, 31)] public Vector3 m_endLoc { get; set; }
    // Name unknown: 0x197BBD69 is a bool, false in every client trigger. The old field list named m_routing
    // here, but m_routing does not hash to it as any type.
    [PropertyField(0x197BBD69, 31)] public bool m_unknown_bool_1 { get; set; }
    [PropertyField(0x4FA58BBA, 31)] public int m_objectTemplateID { get; set; }
    [PropertyField(0x3DAC4C0A, 31)] public bool m_unique { get; set; }
    [PropertyField(0x66ECE9B3, 31)] public ByteString m_uniqueName { get; set; }
    [PropertyField(0xA4092DFC, 31)] public ByteString m_uniqueBusyMsg { get; set; }

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

    [PropertyField(0x20F1DBCE, 31)] public int m_coolDownTime { get; set; }
    [PropertyField(0xB62CA9E6, 31)] public string m_registryEntry { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResToggleQuestEffect : TypeCache.Result {

    public override uint GetHash() => 1383058250;

    [PropertyField(0x5AF71A69, 31)] public bool m_addEffect { get; set; }
    [PropertyField(0x78F28C29, 31)] public string m_effectName { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResDespawnLeashedObject : TypeCache.Result {

    public override uint GetHash() => 577427349;

    [PropertyField(0x1FD186ED, 31)] public int m_followerTemplateID { get; set; }

}

[PropertySerializationTarget]
public partial record ResRemoveEffect : TypeCache.Result {

    public override uint GetHash() => 837967761;

    [PropertyField(0x78F28C29, 31)] public string m_effectName { get; set; } = "";
    [PropertyField(0x7DE4E7CB, 31)] public bool m_useOriginatorID { get; set; }

}

[PropertySerializationTarget]
public partial record ResSpawnLeashedObject : TypeCache.Result {

    public override uint GetHash() => 1505576005;

    // PropertyField: m_followerTemplateID (int)

}

[PropertySerializationTarget]
public partial record ResAddEffect : TypeCache.Result {

    public override uint GetHash() => 53623319;

    [PropertyField(0x78F28C29, 31)] public string m_effectName { get; set; } = "";
    [PropertyField(0x71FCB022, 31)] public bool m_playerOnly { get; set; }
    // PropertyField: m_spEffectInfo (string)

}

[PropertySerializationTarget]
public partial record ResAddTriggerObject : TypeCache.Result {

    public override uint GetHash() => 1893729846;

    [PropertyField(0xC6E6048B, 31)] public string m_triggerObjName { get; set; } = "";
    [PropertyField(0x7B3D75AB, 31)] public string m_triggerObjState { get; set; } = "";

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
    public ulong m_templateID { get; set; }
    public uint m_spellID { get; set; }

    // PropertyField: m_spellName (string)
    // PropertyField: m_subCircle (int)

}

[PropertySerializationTarget]
public partial record ResDrawHand : TypeCache.Result {

    public ulong m_templateID { get; set; }

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

    [PropertyField(0x896B452E, 31)] public string m_sourceType { get; set; } = "";
    [PropertyField(0x40183401, 31)] public ulong m_templateID { get; set; }

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

    [PropertyField(0x1D70805C, 31)] public bool m_blocking { get; set; }
    [PropertyField(0x9BA8BF49, 31)] public string m_cinematicName { get; set; } = "";
    [PropertyField(0x3C1B4C58, 31)] public bool m_endAtActor { get; set; }
    [PropertyField(0x5BB196FF, 31)] public bool m_endAtTargetActor { get; set; }
    // PropertyField: m_endLoc (string)
    // PropertyField: m_objectTemplateID (int)
    // PropertyField: m_router (string)
    // PropertyField: m_routing (string)
    [PropertyField(0x3AAF6E2F, 31)] public bool m_startAtActor { get; set; }
    [PropertyField(0x61437E16, 31)] public bool m_startAtTargetActor { get; set; }
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
    [PropertyField(0x6B608CF6, 31)] public float m_fadeTime { get; set; }
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

    [PropertyField(0x60307EE5, 31)] public bool m_bIncludeAllPlayersInZone { get; set; }
    [PropertyField(0x9BA8BF49, 31)] public string m_cinematicName { get; set; } = "";
    [PropertyField(0x9134F150, 31)] public string m_stageName { get; set; } = "";

}

[PropertySerializationTarget]
public partial record ResReduceMana : TypeCache.Result {

    public override uint GetHash() => 74783451;

    [PropertyField(0x4D1B12C5, 31)] public float m_manaPercent { get; set; }

}

[PropertySerializationTarget]
public partial record ResInitiateCombat : TypeCache.Result {

    public override uint GetHash() => 1486342711;

    [PropertyField(0x718B453C, 31)] public bool m_aggroActor { get; set; }
    [PropertyField(0x4AC25B8F, 31)] public float m_aggroRadius { get; set; }
    [PropertyField(0x4A39CF6A, 31)] public bool m_aggroTarget { get; set; }
    [PropertyField(0x1755F82C, 31)] public bool m_allPlayers { get; set; }
    [PropertyField(0xC15B9ED3, 31)] public string m_sigilLabel { get; set; } = "";

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
public partial record InteractableBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x42CF875B;

    [PropertyField(0x1E1748E2, 31)] public List<InteractOptionTemplate?> m_interactOptions { get; set; } = [];

}

[PropertySerializationTarget]
public partial record InteractOptionTemplate : PropertyClass {

    public override uint GetHash() => 0x0F3D1A6B;

    [PropertyField(0x6A94A9CF, 31)] public string m_questEvent { get; set; } = "";
    // Usage goals whose m_clientTags name one of these tags can use this option.
    [PropertyField(0x5AB57C2D, 31)] public List<string> m_goalTags { get; set; } = [];

}

/// <summary>
/// The interact option most objects use (levers, braziers, crystal stands, candles): the base option's
/// event and goal tags plus the requirements that offer it and the state the object enters when used.
/// </summary>
[PropertySerializationTarget]
public partial record InteractStateOptionTemplate : InteractOptionTemplate {

    public override uint GetHash() => 0x6BEDFAC0;

    // The generator does not flatten a server base type into a derived one, so the base fields repeat here.
    [PropertyField(0x6A94A9CF, 31)] public new string m_questEvent { get => base.m_questEvent; set => base.m_questEvent = value; }
    [PropertyField(0x5AB57C2D, 31)] public new List<string> m_goalTags { get => base.m_goalTags; set => base.m_goalTags = value; }
    [PropertyField(0x92DEF557, 31)] public RequirementList m_requirements { get; set; } = new();
    // Results run when a player uses the option (a ResPostEvent raising "LeverUsed", for example).
    [PropertyField(0xCA3EBE94, 31)] public ResultList m_results { get; set; } = new();
    // The option is offered while the object is in this state (empty: always).
    [PropertyField(0xC7B0B26B, 31)] public ByteString m_visibleState { get; set; }
    [PropertyField(0x93C0C21B, 31)] public ByteString m_enterState { get; set; }

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

public sealed record ResDropTable : Result {

    public override uint GetHash() => 0x1C5E3D8D;

    [PropertyField(0x0, 1)] public string m_tableName { get; set; } = "";
    [PropertyField(0x1, 1)] public int m_maxRolls { get; set; } = 0;

}

public sealed record ResLearnSpell : Result {

    public override uint GetHash() => 0x1C5E3D8D;

    [PropertyField(0x0, 1)] public uint m_templateID { get; set; } = 0;

}

// Quest results may be gated by requirements in the data (e.g. a school-gated learn-spell
// reward); the server's result executor evaluates them before running the handler.
public partial record Result {

    public RequirementList? m_requirements { get; set; }

}

// ---- Types for classes that decoded as null (Task 8) ----

/// <summary>
/// Compares a zone counter (by name) against m_numericValue using m_operatorType.
/// </summary>
[PropertySerializationTarget]
public partial record ReqZoneCounter : ReqNumeric {

    public override uint GetHash() => 0x1210DAB3;

    [PropertyField(0xA5D8835C, 31)] public string m_counterName { get; set; } = "";

}

/// <summary>
/// ReqNumeric plus an unsigned value; seen in the WC_TT01_Balance_L04 treasure-tower zones.
/// The class name is a placeholder: no name found in the client data hashes to 0x0FF838AC.
/// </summary>
[PropertySerializationTarget]
public partial record ReqUnknown0FF838AC : ReqNumeric {

    public override uint GetHash() => 0x0FF838AC;

    [PropertyField(0x3D0EFECA, 31)] public uint m_value { get; set; }

}

/// <summary>
/// A bare ReqNumeric subclass (numeric compare with no extra field).
/// The class name is a placeholder: no name found in the client data hashes to 0x14CA64DE.
/// </summary>
[PropertySerializationTarget]
public partial record ReqUnknown14CA64DE : ReqNumeric {

    public override uint GetHash() => 0x14CA64DE;

}

/// <summary>
/// Player mana compared with m_numericValue (absolute or percent).
/// </summary>
[PropertySerializationTarget]
public partial record ReqMana : ReqNumeric {

    public override uint GetHash() => 0x4CE6811D;

    [PropertyField(0x1EA5F300, 31)] public bool m_isPercent { get; set; }

}

/// <summary>
/// Player health compared with m_numericValue (absolute or percent).
/// </summary>
[PropertySerializationTarget]
public partial record ReqHealth : ReqNumeric {

    public override uint GetHash() => 0x6489CC4B;

    [PropertyField(0x1EA5F300, 31)] public bool m_isPercent { get; set; }

}

/// <summary>
/// Tutorial stage (by name) compared with m_numericValue.
/// </summary>
[PropertySerializationTarget]
public partial record ReqTutorialStage : ReqNumeric {

    public override uint GetHash() => 0x779B35AA;

    [PropertyField(0xBEB44550, 31)] public string m_tutorialName { get; set; } = "";

}

/// <summary>
/// Holds while the named trigger is in the given state (TRIGGER_STATE_ACTIVE / INACTIVE / READY).
/// </summary>
[PropertySerializationTarget]
public partial record ReqTriggerState : Requirement {

    public override uint GetHash() => 0x219824AA;

    [PropertyField(0xB8C90C10, 31)] public string m_triggerName { get; set; } = "";
    [PropertyField(0x855E4CFD, 31)] public string m_triggerState { get; set; } = "";

}

/// <summary>
/// Player has the named effect active.
/// </summary>
[PropertySerializationTarget]
public partial record ReqHasEffect : Requirement {

    public override uint GetHash() => 0x08A71817;

    [PropertyField(0x78F28C29, 31)] public string m_effectName { get; set; } = "";

}

/// <summary>
/// The named zone token is set.
/// </summary>
[PropertySerializationTarget]
public partial record ReqZoneToken : Requirement {

    public override uint GetHash() => 0x055B2AB5;

    [PropertyField(0x662724BD, 31)] public string m_tokenName { get; set; } = "";

}

/// <summary>
/// The named zone token value lies in [m_minValue, m_maxValue].
/// </summary>
[PropertySerializationTarget]
public partial record ReqZoneTokenValue : Requirement {

    public override uint GetHash() => 0x5D4AC455;

    [PropertyField(0x662724BD, 31)] public string m_tokenName { get; set; } = "";
    [PropertyField(0x59EA3E5B, 31)] public int m_minValue { get; set; }
    [PropertyField(0x096BEE9D, 31)] public int m_maxValue { get; set; }

}

/// <summary>
/// Player carries m_quantity of an item (by template id or adjective).
/// </summary>
[PropertySerializationTarget]
public partial record ReqHasItems : Requirement {

    public override uint GetHash() => 0x3DC8A70D;

    [PropertyField(0xCC6A4B4A, 31)] public string m_adjective { get; set; } = "";
    [PropertyField(0x40183401, 31)] public ulong m_templateID { get; set; }
    [PropertyField(0x0A160539, 31)] public int m_quantity { get; set; }

}

/// <summary>
/// Player has the named registry entry (housing / daily entries such as "BeeHouseRide").
/// The class name is a guess: no name found in the client data hashes to 0x3BBD003B.
/// </summary>
[PropertySerializationTarget]
public partial record ReqHasRegistryEntry : Requirement {

    public override uint GetHash() => 0x3BBD003B;

    [PropertyField(0xB62CA9E6, 31)] public string m_registryEntry { get; set; } = "";

}

/// <summary>
/// Player currently has a polymorph effect.
/// </summary>
[PropertySerializationTarget]
public partial record ReqHasPolymorphEffect : Requirement {

    public override uint GetHash() => 0x34E83575;

}

/// <summary>
/// Player is a customer-service account.
/// </summary>
[PropertySerializationTarget]
public partial record ReqIsCSR : Requirement {

    public override uint GetHash() => 0x5A2E0180;

}

/// <summary>
/// A field-less requirement (42 uses).
/// The class name is a placeholder: no name found in the client data hashes to 0x50E82908.
/// </summary>
[PropertySerializationTarget]
public partial record ReqUnknown50E82908 : Requirement {

    public override uint GetHash() => 0x50E82908;

}

/// <summary>
/// A field-less requirement (4 uses).
/// The class name is a placeholder: no name found in the client data hashes to 0x02D1D615.
/// </summary>
[PropertySerializationTarget]
public partial record ReqUnknown02D1D615 : Requirement {

    public override uint GetHash() => 0x02D1D615;

}

/// <summary>
/// Two names (for example "Wheel_Wildlife_01" and "Wildlife_South"); used by the wheel puzzles.
/// The class name is a placeholder: no name found in the client data hashes to 0x2E532A74.
/// </summary>
[PropertySerializationTarget]
public partial record ReqUnknown2E532A74 : Requirement {

    public override uint GetHash() => 0x2E532A74;

    [PropertyField(0xD88B249C, 31)] public string m_nameA { get; set; } = "";
    [PropertyField(0xD88B249D, 31)] public string m_nameB { get; set; } = "";

}

/// <summary>
/// The named encounter / quest has been completed.
/// </summary>
[PropertySerializationTarget]
public partial record ReqEncounterComplete : Requirement {

    public override uint GetHash() => 0x5A6796CA;

    [PropertyField(0xB78BAB2F, 31)] public string m_encounterName { get; set; } = "";

}

/// <summary>
/// Player is transformed into the named form (Transformation_ShadowWeaver).
/// </summary>
[PropertySerializationTarget]
public partial record ReqHasTransformation : Requirement {

    public override uint GetHash() => 0x545538F1;

    [PropertyField(0xBADD86AE, 31)] public string m_transformationName { get; set; } = "";

}

/// <summary>
/// A quest name plus a variable name (for example HG-SIT-Snake-Boss).
/// </summary>
[PropertySerializationTarget]
public partial record ReqGetEncounterVariable : Requirement {

    public override uint GetHash() => 0x4C1B08BA;

    [PropertyField(0x65742E4E, 31)] public string m_questName { get; set; } = "";
    [PropertyField(0x76A4D645, 31)] public string m_varName { get; set; } = "";

}

/// <summary>
/// A requirement on one template id (seen with ids near 1.53M).
/// The class name is a placeholder: no name found in the client data hashes to 0x2C948ACC.
/// </summary>
[PropertySerializationTarget]
public partial record ReqUnknown2C948ACC : Requirement {

    public override uint GetHash() => 0x2C948ACC;

    [PropertyField(0x3687EE39, 31)] public uint m_templateID { get; set; }

}

/// <summary>
/// Player has at least m_gold gold.
/// </summary>
[PropertySerializationTarget]
public partial record ReqHasGold : Requirement {

    public override uint GetHash() => 0x22641285;

    [PropertyField(0x0D142440, 31)] public int m_gold { get; set; }

}

/// <summary>
/// A field-less requirement used by the mount interact options (154 templates).
/// </summary>
[PropertySerializationTarget]
public partial record ReqIsInParty : Requirement {

    public override uint GetHash() => 0x7BA77EC3;

}

/// <summary>
/// A field-less requirement used by the mount interact options (154 templates).
/// The class name is a placeholder: no name found in the client data hashes to 0x3569555F.
/// </summary>
[PropertySerializationTarget]
public partial record ReqUnknown3569555F : Requirement {

    public override uint GetHash() => 0x3569555F;

}

/// <summary>
/// A field-less requirement used by the mount interact options (154 templates).
/// The class name is a placeholder: no name found in the client data hashes to 0x6A164DC9.
/// </summary>
[PropertySerializationTarget]
public partial record ReqUnknown6A164DC9 : Requirement {

    public override uint GetHash() => 0x6A164DC9;

}

/// <summary>
/// A field-less requirement used by the mount interact options (154 templates).
/// The class name is a placeholder: no name found in the client data hashes to 0x001CD4F2.
/// </summary>
[PropertySerializationTarget]
public partial record ReqUnknown001CD4F2 : Requirement {

    public override uint GetHash() => 0x001CD4F2;

}

/// <summary>
/// Sets or adds to a zone counter; m_action is ZCA_Set or ZCA_Add.
/// </summary>
[PropertySerializationTarget]
public partial record ResZoneCounter : TypeCache.Result {

    public override uint GetHash() => 0x1210D8B3;

    [PropertyField(0xA5D8835C, 31)] public string m_counterName { get; set; } = "";
    [PropertyField(0x57578836, 31)] public string m_action { get; set; } = "";
    [PropertyField(0x30753FF7, 31)] public int m_value { get; set; }

}

/// <summary>
/// Disables the named zone token.
/// </summary>
[PropertySerializationTarget]
public partial record ResZoneTokenDisable : TypeCache.Result {

    public override uint GetHash() => 0x522B2FA1;

    [PropertyField(0x662724BD, 31)] public string m_tokenName { get; set; } = "";
    [PropertyField(0x284F3B38, 31)] public bool m_zoneWide { get; set; }

}

/// <summary>
/// Enables the named zone token.
/// </summary>
[PropertySerializationTarget]
public partial record ResZoneTokenEnable : TypeCache.Result {

    public override uint GetHash() => 0x6C9F577C;

    [PropertyField(0x662724BD, 31)] public string m_tokenName { get; set; } = "";
    [PropertyField(0x284F3B38, 31)] public bool m_zoneWide { get; set; }

}

/// <summary>
/// Adds m_delta to the named zone token.
/// </summary>
[PropertySerializationTarget]
public partial record ResZoneTokenModify : TypeCache.Result {

    public override uint GetHash() => 0x33823ED4;

    [PropertyField(0x662724BD, 31)] public string m_tokenName { get; set; } = "";
    [PropertyField(0x2F31B844, 31)] public int m_delta { get; set; }
    [PropertyField(0x284F3B38, 31)] public bool m_zoneWide { get; set; }

}

/// <summary>
/// Resets the named zone token.
/// </summary>
[PropertySerializationTarget]
public partial record ResZoneTokenReset : TypeCache.Result {

    public override uint GetHash() => 0x4D4EA7A7;

    [PropertyField(0x662724BD, 31)] public string m_tokenName { get; set; } = "";
    [PropertyField(0x284F3B38, 31)] public bool m_zoneWide { get; set; }

}

/// <summary>
/// Plays a music cue (EM_Cue_*, *_FullSong) with a fade time.
/// </summary>
[PropertySerializationTarget]
public partial record ResWizPlayMusic : TypeCache.Result {

    public override uint GetHash() => 0x6FB5E029;

    [PropertyField(0xB0AA85FD, 31)] public string m_musicName { get; set; } = "";
    [PropertyField(0x6B608CF6, 31)] public float m_fadeTime { get; set; }
    [PropertyField(0x37E2E182, 31)] public bool m_flagA { get; set; }
    [PropertyField(0x3F2D4568, 31)] public bool m_flagB { get; set; }
    [PropertyField(0x53F67331, 31)] public bool m_flagC { get; set; }
    [PropertyField(0x444373FA, 31)] public ZoneRouter? m_soundInfo { get; set; }
    [PropertyField(0x2C2BC314, 31)] public float m_startDelay { get; set; }

}

/// <summary>
/// Polymorphs the player into the given template.
/// </summary>
[PropertySerializationTarget]
public partial record ResAddPolymorphEffect : TypeCache.Result {

    public override uint GetHash() => 0x3F7D1775;

    [PropertyField(0x3D2C06C0, 31)] public uint m_polymorphTemplateID { get; set; }

}

/// <summary>
/// Removes every polymorph effect from the player.
/// </summary>
[PropertySerializationTarget]
public partial record ResRemovePolymorphEffects : TypeCache.Result {

    public override uint GetHash() => 0x4C7CBE48;

}

/// <summary>
/// Runs one of several result lists chosen by weight (m_percent).
/// </summary>
[PropertySerializationTarget]
public partial record ResRandomResults : TypeCache.Result {

    public override uint GetHash() => 0x42C143A6;

    [PropertyField(0x77394DFB, 31)] public List<RandomResult?> m_results { get; set; } = [];

}

/// <summary>
/// Plays a .nif effect at a position for a duration.
/// </summary>
[PropertySerializationTarget]
public partial record ResPlayGraphic : TypeCache.Result {

    public override uint GetHash() => 0x66033FF4;

    [PropertyField(0x10EA450F, 31)] public float m_x { get; set; }
    [PropertyField(0x10EA4510, 31)] public float m_y { get; set; }
    [PropertyField(0x10EA4511, 31)] public float m_z { get; set; }
    [PropertyField(0x154B4C88, 31)] public float m_yaw { get; set; }
    [PropertyField(0x36DB057D, 31)] public float m_duration { get; set; }
    [PropertyField(0xB9CE90BC, 31)] public string m_filename { get; set; } = "";

}

/// <summary>
/// Battleground sigil: casts spells when the sigil is used.
/// </summary>
[PropertySerializationTarget]
public partial record ResCastSigilSpell : TypeCache.Result {

    public override uint GetHash() => 0x28400ABC;

    [PropertyField(0xA9CE9B38, 31)] public List<string> m_spellNameList { get; set; } = [];
    [PropertyField(0xBD6734F4, 31)] public string m_sigilName { get; set; } = "";
    [PropertyField(0xC01849EA, 31)] public string m_sigilDisplayName { get; set; } = "";
    [PropertyField(0x77A22F2E, 31)] public float m_range { get; set; }
    [PropertyField(0xAF55EF92, 31)] public string m_stringA { get; set; } = "";
    [PropertyField(0x9BC1F695, 31)] public string m_stringB { get; set; } = "";
    [PropertyField(0x453374A7, 31)] public bool m_flag { get; set; }

}

/// <summary>
/// Awards the named badge (to everyone in the zone when m_allInZone).
/// </summary>
[PropertySerializationTarget]
public partial record ResIncrementBadge : TypeCache.Result {

    public override uint GetHash() => 0x7887F2B8;

    [PropertyField(0xB2FD59CF, 31)] public string m_badgeName { get; set; } = "";
    [PropertyField(0x2EB6A55F, 31)] public bool m_allInZone { get; set; }

}

/// <summary>
/// Awards up to three badges.
/// The class name is a guess: no name found in the client data hashes to 0x3CCB49B6.
/// </summary>
[PropertySerializationTarget]
public partial record ResAwardBadges : TypeCache.Result {

    public override uint GetHash() => 0x3CCB49B6;

    [PropertyField(0xB2FD59CF, 31)] public string m_badgeName { get; set; } = "";
    [PropertyField(0x69DABEA1, 31)] public string m_badgeName2 { get; set; } = "";
    [PropertyField(0x69DABEA2, 31)] public string m_badgeName3 { get; set; } = "";

}

/// <summary>
/// Named location plus a flag (Start, TargetLocation_Room_02); a location move.
/// The class name is a placeholder: no name found in the client data hashes to 0x2D21EDCB.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown2D21EDCB : TypeCache.Result {

    public override uint GetHash() => 0x2D21EDCB;

    [PropertyField(0x6CBB5D15, 31)] public string m_locationName { get; set; } = "";
    [PropertyField(0x432B179A, 31)] public bool m_flag { get; set; }

}

/// <summary>
/// Battleground timer: m_action (Add, Set, Remove) with a duration text such as "2m".
/// </summary>
[PropertySerializationTarget]
public partial record ResBGAdjustTime : TypeCache.Result {

    public override uint GetHash() => 0x77D61540;

    [PropertyField(0x7ED86D26, 31)] public string m_action { get; set; } = "";
    [PropertyField(0x8ABFBE61, 31)] public string m_duration { get; set; } = "";

}

/// <summary>
/// One integer value (always 1 in the data).
/// The class name is a placeholder: no name found in the client data hashes to 0x6B04997C.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown6B04997C : TypeCache.Result {

    public override uint GetHash() => 0x6B04997C;

    [PropertyField(0x30753FF7, 31)] public int m_value { get; set; }

}

/// <summary>
/// Adds progress to a class project (TreasureTower_01_*).
/// The class name is a placeholder: no name found in the client data hashes to 0x7AA255A7.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown7AA255A7 : TypeCache.Result {

    public override uint GetHash() => 0x7AA255A7;

    [PropertyField(0x662BCD69, 31)] public string m_classProjectName { get; set; } = "";
    [PropertyField(0x2B0BB46B, 31)] public int m_amount { get; set; }

}

/// <summary>
/// Class project by name.
/// The class name is a placeholder: no name found in the client data hashes to 0x0758F7C9.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown0758F7C9 : TypeCache.Result {

    public override uint GetHash() => 0x0758F7C9;

    [PropertyField(0x662BCD69, 31)] public string m_classProjectName { get; set; } = "";

}

/// <summary>
/// Class project by name (MonthlyGauntletEvent_*).
/// The class name is a placeholder: no name found in the client data hashes to 0x08965241.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown08965241 : TypeCache.Result {

    public override uint GetHash() => 0x08965241;

    [PropertyField(0x662BCD69, 31)] public string m_classProjectName { get; set; } = "";

}

/// <summary>
/// Integer activity type.
/// The class name is a placeholder: no name found in the client data hashes to 0x77D46519.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown77D46519 : TypeCache.Result {

    public override uint GetHash() => 0x77D46519;

    [PropertyField(0x06382429, 31)] public int m_activityType { get; set; }

}

/// <summary>
/// A field-less result (50 uses).
/// The class name is a placeholder: no name found in the client data hashes to 0x117B3E95.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown117B3E95 : TypeCache.Result {

    public override uint GetHash() => 0x117B3E95;

}

/// <summary>
/// A field-less result (12 uses).
/// The class name is a placeholder: no name found in the client data hashes to 0x598D00C0.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown598D00C0 : TypeCache.Result {

    public override uint GetHash() => 0x598D00C0;

}

/// <summary>
/// A field-less result (5 uses).
/// The class name is a placeholder: no name found in the client data hashes to 0x10971E74.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown10971E74 : TypeCache.Result {

    public override uint GetHash() => 0x10971E74;

}

/// <summary>
/// A field-less result (1 use).
/// The class name is a placeholder: no name found in the client data hashes to 0x67051AF2.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown67051AF2 : TypeCache.Result {

    public override uint GetHash() => 0x67051AF2;

}

/// <summary>
/// A field-less result (1 use).
/// The class name is a placeholder: no name found in the client data hashes to 0x09108739.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown09108739 : TypeCache.Result {

    public override uint GetHash() => 0x09108739;

}

/// <summary>
/// A field-less result (93 uses).
/// The class name is a placeholder: no name found in the client data hashes to 0x1420BD5B.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown1420BD5B : TypeCache.Result {

    public override uint GetHash() => 0x1420BD5B;

}

/// <summary>
/// Sets a quest variable (IgnacioAlive, TawniAlive) to a boolean.
/// </summary>
[PropertySerializationTarget]
public partial record ResEncounterSetVariable : TypeCache.Result {

    public override uint GetHash() => 0x782791AB;

    [PropertyField(0x65742E4E, 31)] public string m_questName { get; set; } = "";
    [PropertyField(0x76A4D645, 31)] public string m_varName { get; set; } = "";
    [PropertyField(0x309B1C10, 31)] public bool m_value { get; set; }

}

/// <summary>
/// One template id (near 1.56M).
/// The class name is a placeholder: no name found in the client data hashes to 0x7960FE1A.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown7960FE1A : TypeCache.Result {

    public override uint GetHash() => 0x7960FE1A;

    [PropertyField(0x282E105E, 31)] public uint m_templateID { get; set; }

}

/// <summary>
/// If the requirements pass run the then results, else the else results.
/// </summary>
[PropertySerializationTarget]
public partial record ResIfThenElse : TypeCache.Result {

    public override uint GetHash() => 0x0028C7E8;

    [PropertyField(0xC50B1035, 31)] public RequirementList? m_requirements { get; set; }
    [PropertyField(0x78BF7829, 31)] public ResultList? m_thenResults { get; set; }
    [PropertyField(0xC8F8D003, 31)] public ResultList? m_elseResults { get; set; }

}

/// <summary>
/// One integer (1).
/// The class name is a placeholder: no name found in the client data hashes to 0x5991CB8C.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown5991CB8C : TypeCache.Result {

    public override uint GetHash() => 0x5991CB8C;

    [PropertyField(0x74A0C0D1, 31)] public int m_value { get; set; }

}

/// <summary>
/// Posts an event (EVENT_PVE_Wave3); shaped like ResPostEvent.
/// The class name is a placeholder: no name found in the client data hashes to 0x1DC65E8F.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown1DC65E8F : TypeCache.Result {

    public override uint GetHash() => 0x1DC65E8F;

    [PropertyField(0xD036EBFE, 31)] public string m_eventName { get; set; } = "";
    [PropertyField(0xC49A5E28, 31)] public string m_subEventName { get; set; } = "";

}

/// <summary>
/// Starts a minigame (SilverChestGame, Skull_Riders) with its loot tables.
/// </summary>
[PropertySerializationTarget]
public partial record ResMinigame : TypeCache.Result {

    public override uint GetHash() => 0x337E042D;

    [PropertyField(0xB48A3F82, 31)] public string m_minigame { get; set; } = "";
    [PropertyField(0x5A4C7AB4, 31)] public List<string> m_lootTables { get; set; } = [];
    [PropertyField(0x24A5AA8F, 31)] public int m_successScore { get; set; }

}

/// <summary>
/// Teleports to a named location and posts a zone event (Wave_End_Reset).
/// The class name is a placeholder: no name found in the client data hashes to 0x5ABB361D.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown5ABB361D : TypeCache.Result {

    public override uint GetHash() => 0x5ABB361D;

    [PropertyField(0x68AC3A4D, 31)] public string m_teleportTo { get; set; } = "";
    [PropertyField(0x853094B9, 31)] public string m_zoneEvent { get; set; } = "";

}

/// <summary>
/// Points (100).
/// The class name is a placeholder: no name found in the client data hashes to 0x4591AF80.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown4591AF80 : TypeCache.Result {

    public override uint GetHash() => 0x4591AF80;

    [PropertyField(0x31EEB077, 31)] public int m_points { get; set; }

}

/// <summary>
/// Adds or removes an accompanying NPC (by template id).
/// The class name is a guess: no name found in the client data hashes to 0x4F5B4296.
/// </summary>
[PropertySerializationTarget]
public partial record ResToggleAccompanyNPC : TypeCache.Result {

    public override uint GetHash() => 0x4F5B4296;

    [PropertyField(0x1010669C, 31)] public bool m_add { get; set; }
    [PropertyField(0x4CB232F6, 31)] public uint m_templateID { get; set; }

}

/// <summary>
/// Puts a trigger object into a state; the ResModifyTriggerObject twin with one extra text field.
/// </summary>
[PropertySerializationTarget]
public partial record ResStateChange : TypeCache.Result {

    public override uint GetHash() => 0x714DF48B;

    [PropertyField(0xC6E6048B, 31)] public string m_triggerObjName { get; set; } = "";
    [PropertyField(0x7B3D75AB, 31)] public string m_triggerObjState { get; set; } = "";
    [PropertyField(0x94EAC863, 31)] public string m_subState { get; set; } = "";

}

/// <summary>
/// Rolls the camera by an angle (radians).
/// </summary>
[PropertySerializationTarget]
public partial record ResRollCamera : TypeCache.Result {

    public override uint GetHash() => 0x6C25E4E4;

    [PropertyField(0x127A88B0, 31)] public float m_roll { get; set; }

}

/// <summary>
/// Adds a zone timer (live sends MSG_ADDZONETIMER): name, UI text, countdown and the events it posts when it expires (MalistaireCinematic, TitanIntroCinematic). The first result of the Malistaire Lair StartCinematicTrigger.
/// The class name is a guess: no name found in the client data hashes to 0x3DEC71C5.
/// </summary>
[PropertySerializationTarget]
public partial record ResAddZoneTimer : TypeCache.Result {

    public override uint GetHash() => 0x3DEC71C5;

    [PropertyField(0x76D902FD, 31)] public string m_timerName { get; set; } = "";
    [PropertyField(0xA99CA27E, 31)] public string m_stringA { get; set; } = "";
    [PropertyField(0x6F0C091A, 31)] public string m_timerUI { get; set; } = "";
    [PropertyField(0x4C1C5E27, 31)] public float m_countdownTime { get; set; }
    [PropertyField(0x067DE33C, 31)] public int m_mode { get; set; }
    [PropertyField(0x97366311, 31)] public List<string> m_countdownEvents { get; set; } = [];
    [PropertyField(0x6617CE3F, 31)] public string m_clientEvent { get; set; } = "";
    [PropertyField(0x5B8DA654, 31)] public string m_stringB { get; set; } = "";

}

/// <summary>
/// Equips an item template in a category (Pet).
/// </summary>
[PropertySerializationTarget]
public partial record ResEquipItem : TypeCache.Result {

    public override uint GetHash() => 0x373FD603;

    [PropertyField(0xD60CB6DA, 31)] public string m_categoryName { get; set; } = "";
    [PropertyField(0x40183401, 31)] public ulong m_templateID { get; set; }

}

/// <summary>
/// Unequips a category (Mount).
/// </summary>
[PropertySerializationTarget]
public partial record ResUnequipCategory : TypeCache.Result {

    public override uint GetHash() => 0x6C7BF45B;

    [PropertyField(0x896B452E, 31)] public string m_sourceType { get; set; } = "";
    [PropertyField(0xD60CB6DA, 31)] public string m_categoryName { get; set; } = "";

}

/// <summary>
/// Player owns the expansion (ExpansionData::EDC_Bank). The data carries only this field, with no m_applyNOT / m_operator.
/// The class name is a guess: no name found in the client data hashes to 0x5820E493.
/// </summary>
[PropertySerializationTarget]
public partial record ReqHasExpansion : Requirement {

    public override uint GetHash() => 0x5820E493;

    [PropertyField(0xA1405A86, 31)] public string m_category { get; set; } = "";

}

/// <summary>
/// A teleport-like ride (TriggerCarpetRide) to a zone and location.
/// </summary>
[PropertySerializationTarget]
public partial record ResCarpetRide : TypeCache.Result {

    public override uint GetHash() => 0x4E2FC697;

    [PropertyField(0x896B452E, 31)] public string m_sourceType { get; set; } = "";
    [PropertyField(0x4FA54B98, 31)] public ulong m_templateID { get; set; }
    [PropertyField(0x9B4A9479, 31)] public string m_destinationZone { get; set; } = "";
    [PropertyField(0x5B46533B, 31)] public string m_destinationLoc { get; set; } = "";

}

/// <summary>
/// A category text (LemuriaRaid).
/// The class name is a placeholder: no name found in the client data hashes to 0x1540ED0D.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown1540ED0D : TypeCache.Result {

    public override uint GetHash() => 0x1540ED0D;

    [PropertyField(0x9FDCD6B9, 31)] public string m_category { get; set; } = "";

}

/// <summary>
/// Joins the combat of the labelled sigil.
/// </summary>
[PropertySerializationTarget]
public partial record ResJoinCombatSigil : TypeCache.Result {

    public override uint GetHash() => 0x654CD213;

    [PropertyField(0xC15B9ED3, 31)] public string m_sigilLabel { get; set; } = "";

}

/// <summary>
/// Joins the labelled sigil.
/// </summary>
[PropertySerializationTarget]
public partial record ResJoinSigil : TypeCache.Result {

    public override uint GetHash() => 0x496AB6C5;

    [PropertyField(0xC15B9ED3, 31)] public string m_sigilLabel { get; set; } = "";

}

/// <summary>
/// Four result lists (branches).
/// The class name is a placeholder: no name found in the client data hashes to 0x4A306801.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown4A306801 : TypeCache.Result {

    public override uint GetHash() => 0x4A306801;

    [PropertyField(0xD4B346A9, 31)] public ResultList? m_resultsA { get; set; }
    [PropertyField(0xC0F51B8A, 31)] public ResultList? m_resultsB { get; set; }
    [PropertyField(0xAD36F06B, 31)] public ResultList? m_resultsC { get; set; }
    [PropertyField(0x9978C54C, 31)] public ResultList? m_resultsD { get; set; }

}

/// <summary>
/// One flag.
/// The class name is a placeholder: no name found in the client data hashes to 0x48DDB347.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown48DDB347 : TypeCache.Result {

    public override uint GetHash() => 0x48DDB347;

    [PropertyField(0x0F80FF6A, 31)] public bool m_flag { get; set; }

}

/// <summary>
/// Removes the named spell.
/// </summary>
[PropertySerializationTarget]
public partial record ResRemoveSpell : TypeCache.Result {

    public override uint GetHash() => 0x6A7B1528;

    [PropertyField(0xA03F077C, 31)] public string m_spellName { get; set; } = "";

}

/// <summary>
/// A housing teleport with a transition id.
/// The class name is a placeholder: no name found in the client data hashes to 0x6DD00C76.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown6DD00C76 : TypeCache.Result {

    public override uint GetHash() => 0x6DD00C76;

    [PropertyField(0x9B4A9479, 31)] public string m_destinationZone { get; set; } = "";
    [PropertyField(0x5B46533B, 31)] public string m_destinationLoc { get; set; } = "";
    [PropertyField(0x391D7325, 31)] public uint m_transitionID { get; set; }

}

/// <summary>
/// A teleport to a location in the current zone with a flag.
/// The class name is a placeholder: no name found in the client data hashes to 0x46316EFB.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown46316EFB : TypeCache.Result {

    public override uint GetHash() => 0x46316EFB;

    [PropertyField(0x5B46533B, 31)] public string m_destinationLoc { get; set; } = "";
    [PropertyField(0x0F091FA4, 31)] public bool m_flag { get; set; }

}

/// <summary>
/// Adds or removes aggro.
/// </summary>
[PropertySerializationTarget]
public partial record ResToggleAggro : TypeCache.Result {

    public override uint GetHash() => 0x1704451D;

    [PropertyField(0xC95CF2D2, 31)] public string m_stringA { get; set; } = "";
    [PropertyField(0x2D4A8BEC, 31)] public bool m_addAggro { get; set; }

}

/// <summary>
/// Takes m_gold gold (InteractRemoveGold).
/// </summary>
[PropertySerializationTarget]
public partial record ResRemoveGold : TypeCache.Result {

    public override uint GetHash() => 0x4B44B50E;

    [PropertyField(0x896B452E, 31)] public string m_sourceType { get; set; } = "";
    [PropertyField(0x0D142440, 31)] public int m_gold { get; set; }

}

/// <summary>
/// Deals m_damage (absolute or percent).
/// The class name is a placeholder: no name found in the client data hashes to 0x60CAA028.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown60CAA028 : TypeCache.Result {

    public override uint GetHash() => 0x60CAA028;

    [PropertyField(0x14F56A59, 31)] public int m_damage { get; set; }
    [PropertyField(0x22FE62DE, 31)] public bool m_usePercentage { get; set; }

}

/// <summary>
/// Checks whether the player has a badge.
/// </summary>
[PropertySerializationTarget]
public partial record ResCheckBadge : TypeCache.Result {

    public override uint GetHash() => 0x75E5430C;

    [PropertyField(0xB21D085D, 31)] public string m_badgeAdjective { get; set; } = "";

}

/// <summary>
/// Plays a dialogue sound (Sound/Dialogue/*.mp3) over an NPC bubble.
/// The class name is a placeholder: no name found in the client data hashes to 0x52E3DAFC.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown52E3DAFC : TypeCache.Result {

    public override uint GetHash() => 0x52E3DAFC;

    [PropertyField(0xB0C0E21D, 31)] public string m_stringA { get; set; } = "";
    [PropertyField(0x1EAEE387, 31)] public bool m_isPersona { get; set; }
    [PropertyField(0x3CC7143E, 31)] public ulong m_idA { get; set; }
    [PropertyField(0x444373FA, 31)] public ZoneRouter? m_soundInfo { get; set; }
    [PropertyField(0x4D2D0C96, 31)] public bool m_flagA { get; set; }
    [PropertyField(0x9292129D, 31)] public string m_stringB { get; set; } = "";
    [PropertyField(0x2FBDB87C, 31)] public bool m_noBubble { get; set; }
    [PropertyField(0x898BDE44, 31)] public string m_sound { get; set; } = "";
    [PropertyField(0x5FED55E8, 31)] public float m_soundRadiusOverride { get; set; }
    [PropertyField(0x2B83CCDA, 31)] public int m_widthOverride { get; set; }

}

/// <summary>
/// A field-less result used by the mount interact options (153 templates).
/// </summary>
[PropertySerializationTarget]
public partial record ResRideMount : TypeCache.Result {

    public override uint GetHash() => 0x0309D887;

}

/// <summary>
/// A field-less result used by one mount template.
/// </summary>
[PropertySerializationTarget]
public partial record ResLeashObject : TypeCache.Result {

    public override uint GetHash() => 0x6F9EB867;

}

/// <summary>
/// Turns the player into the given pet template (the WC_OldeTown "Trigger ForcePlayerAsPet" triggers).
/// The class name is a guess: no name found in the client data hashes to 0x493C61E2.
/// </summary>
[PropertySerializationTarget]
public partial record ResForcePlayerAsPet : TypeCache.Result {

    public override uint GetHash() => 0x493C61E2;

    [PropertyField(0x13389F32, 31)] public uint m_petTemplate { get; set; }

}

/// <summary>
/// A quest name plus a goal name (like ResCompleteQuestGoal); seen in KT_Z05I06_MainTheater.
/// The class name is a placeholder: no name found in the client data hashes to 0x707DD096.
/// </summary>
[PropertySerializationTarget]
public partial record ResUnknown707DD096 : TypeCache.Result {

    public override uint GetHash() => 0x707DD096;

    [PropertyField(0x65742E4E, 31)] public string m_questName { get; set; } = "";
    [PropertyField(0x5AB2329F, 31)] public string m_goalName { get; set; } = "";

}

/// <summary>
/// Chests that reroll their loot for crowns.
/// </summary>
[PropertySerializationTarget]
public partial record PaidLootRollBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x3AE8311B;

    [PropertyField(0x6742DDAF, 31)] public int m_costCrowns { get; set; }
    [PropertyField(0x50AC2DED, 31)] public int m_dailyUses { get; set; }
    [PropertyField(0x975DE361, 31)] public string m_lootTable { get; set; } = "";
    [PropertyField(0x24130706, 31)] public float m_luckMultiplier { get; set; }

}

/// <summary>
/// A questing NPC: persona name and quest list.
/// The class name is a guess: no name found in the client data hashes to 0x1F024B02.
/// </summary>
[PropertySerializationTarget]
public partial record WizardQuestingBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x1F024B02;

    [PropertyField(0x95DF7F14, 31)] public string m_personaName { get; set; } = "";
    [PropertyField(0x65733869, 31)] public List<string> m_questList { get; set; } = [];
    [PropertyField(0x4DE56BCD, 31)] public float m_npcProximity { get; set; }
    [PropertyField(0x64385755, 31)] public string m_stringA { get; set; } = "";

}

/// <summary>
/// Generic NPC service (banker, vendor...): interact text overrides.
/// The class name is a guess: no name found in the client data hashes to 0x3170B6A0.
/// </summary>
[PropertySerializationTarget]
public partial record BasicNPCServiceBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x3170B6A0;

    [PropertyField(0x7C6511DA, 31)] public string m_interactTextOverride { get; set; } = "";
    [PropertyField(0x7139D4B7, 31)] public string m_interactTitleOverride { get; set; } = "";
    [PropertyField(0x4B155DA2, 31)] public float m_distanceOverride { get; set; }
    [PropertyField(0x60786AAC, 31)] public string m_stringA { get; set; } = "";
    [PropertyField(0xBED1C2C9, 31)] public string m_stringB { get; set; } = "";
    [PropertyField(0x651DA0D8, 31)] public bool m_clickToInteractOnly { get; set; }
    [PropertyField(0x29169ED8, 31)] public bool m_petInteractOnly { get; set; }

}

/// <summary>
/// A spell trainer: the list of trainable spells.
/// </summary>
[PropertySerializationTarget]
public partial record WizTrainingBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x0464F8BE;

    [PropertyField(0x80CE8EA7, 31)] public List<SpellTraining?> m_spells { get; set; } = [];

}

/// <summary>
/// A crafting station (recipe list, title and icon overrides).
/// </summary>
[PropertySerializationTarget]
public partial record AlchemyStationBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x2003E995;

    [PropertyField(0x9E4F2A12, 31)] public string m_stringA { get; set; } = "";
    [PropertyField(0xC56598EF, 31)] public List<string> m_recipeList { get; set; } = [];
    [PropertyField(0x2CD21BD5, 31)] public bool m_noCooldown { get; set; }
    [PropertyField(0x2DC479D6, 31)] public bool m_noCategories { get; set; }
    [PropertyField(0x7C85036F, 31)] public bool m_flag { get; set; }
    [PropertyField(0x6EAD3D7D, 31)] public string m_titleOverride { get; set; } = "";
    [PropertyField(0x996035EB, 31)] public string m_createButtonOverride { get; set; } = "";
    [PropertyField(0x60934C04, 31)] public string m_iconOverride { get; set; } = "";

}

/// <summary>
/// An object that runs results when photographed.
/// </summary>
[PropertySerializationTarget]
public partial record PhotographedObjectBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x3667258A;

    [PropertyField(0xBEF6F93F, 31)] public ResultList? m_results { get; set; }
    [PropertyField(0x59C8A1FE, 31)] public string m_schoolName { get; set; } = "";

}

/// <summary>
/// A PvP kiosk: match types and arenas.
/// </summary>
[PropertySerializationTarget]
public partial record PvPKioskBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x7386C1FD;

    [PropertyField(0x6C22883D, 31)] public List<string> m_tournamentTypes { get; set; } = [];
    [PropertyField(0xB3427BBB, 31)] public List<string> m_arenaNames { get; set; } = [];

}

/// <summary>
/// A kiosk.
/// </summary>
[PropertySerializationTarget]
public partial record KioskBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x07E19F00;

    [PropertyField(0x4FCEB66E, 31)] public bool m_flag { get; set; }

}

/// <summary>
/// Battleground polymorph improvement station.
/// </summary>
[PropertySerializationTarget]
public partial record BattlegroundPolymorphImprovementBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x1E825005;

}

/// <summary>
/// A mob that keeps its distance from the player.
/// </summary>
[PropertySerializationTarget]
public partial record EvasionBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x00B7568F;

    [PropertyField(0x31137441, 31)] public float m_distanceA { get; set; }
    [PropertyField(0x3C6C51CE, 31)] public float m_distanceB { get; set; }
    [PropertyField(0x4ED34A74, 31)] public float m_distanceC { get; set; }
    [PropertyField(0x013356CF, 31)] public bool m_flagA { get; set; }
    [PropertyField(0x5830B572, 31)] public float m_valueA { get; set; }
    [PropertyField(0x361A70BA, 31)] public bool m_flagB { get; set; }
    [PropertyField(0x43894B9D, 31)] public float m_angle { get; set; }

}

/// <summary>
/// A vendor: shop lists and title.
/// </summary>
[PropertySerializationTarget]
public partial record WizShoppingBehaviorTemplate : BehaviorTemplate {

    public override uint GetHash() => 0x6CF3E77D;

    [PropertyField(0x4DE56BCD, 31)] public float m_npcProximity { get; set; }
    [PropertyField(0xC7A83557, 31)] public string m_shopTitle { get; set; } = "";
    [PropertyField(0x1A9FBA56, 31)] public int m_shopType { get; set; }
    [PropertyField(0x626D6715, 31)] public bool m_CSRTestShop { get; set; }
    [PropertyField(0x4ECBD678, 31)] public int m_furnitureShop { get; set; }
    [PropertyField(0x1A9AD38E, 31)] public List<ulong> m_shopList { get; set; } = [];
    [PropertyField(0xC56598EF, 31)] public List<string> m_recipeList { get; set; } = [];
    [PropertyField(0x720E6E01, 31)] public string m_stringA { get; set; } = "";
    [PropertyField(0x788A4C47, 31)] public string m_stringB { get; set; } = "";
    [PropertyField(0xB26ED656, 31)] public string m_stringC { get; set; } = "";

}

/// <summary>
/// One weighted branch of ResRandomResults.
/// </summary>
[PropertySerializationTarget]
public partial record RandomResult : PropertyClass {

    public override uint GetHash() => 0x779D0272;

    [PropertyField(0x5E42E888, 31)] public float m_percent { get; set; }
    [PropertyField(0xB80E294E, 31)] public ResultList? m_results { get; set; }

}

/// <summary>
/// One spell a trainer teaches.
/// </summary>
[PropertySerializationTarget]
public partial record SpellTraining : PropertyClass {

    public override uint GetHash() => 0x1455D777;

    [PropertyField(0xD6E10342, 31)] public RequirementList? m_requirements { get; set; }
    [PropertyField(0xA4F41059, 31)] public string m_completeText { get; set; } = "";
    [PropertyField(0xC9C855C4, 31)] public string m_schoolName { get; set; } = "";
    [PropertyField(0x7085724E, 31)] public uint m_cost { get; set; }
    [PropertyField(0xA03F077C, 31)] public string m_spellName { get; set; } = "";

}

/// <summary>
/// A trigger that is also an interactable object state gate: the Trigger fields plus the interact-option state fields (seen as "Trigger DoorCollision", "Bug7Trigger" in the KT_Retreat / KT_Vault style zones).
/// </summary>
[PropertySerializationTarget]
public partial record StateTrigger : Trigger {

    public override uint GetHash() => 0x0BFE98C8;

    [PropertyField(0xB8C90C10, 31)] public new ByteString m_triggerName { get => base.m_triggerName; set => base.m_triggerName = value; }
    [PropertyField(0x3933D634, 31)] public new uint m_triggerMax { get => base.m_triggerMax; set => base.m_triggerMax = value; }
    [PropertyField(0x767AAC3C, 31)] public new uint m_cooldown { get => base.m_cooldown; set => base.m_cooldown = value; }
    [PropertyField(0x2E8B9981, 31)] public new uint m_cooldownRand { get => base.m_cooldownRand; set => base.m_cooldownRand = value; }
    [PropertyField(0x3282D78A, 31)] public new bool m_pulsar { get => base.m_pulsar; set => base.m_pulsar = value; }
    [PropertyField(0x7DB09CC1, 31)] public new List<ByteString> m_activateEvents { get => base.m_activateEvents; set => base.m_activateEvents = value; }
    [PropertyField(0xA7BEADF6, 31)] public new List<ByteString> m_fireEvents { get => base.m_fireEvents; set => base.m_fireEvents = value; }
    [PropertyField(0x62A2160A, 31)] public new List<ByteString> m_deactivateEvents { get => base.m_deactivateEvents; set => base.m_deactivateEvents = value; }
    [PropertyField(0x5C548D5F, 31)] public new List<ByteString> m_unknown { get => base.m_unknown; set => base.m_unknown = value; }
    [PropertyField(0xA955FFA6, 31)] public new RequirementList m_requirements { get => base.m_requirements; set => base.m_requirements = value; }
    [PropertyField(0xE11C8ADA, 31)] public new ResultList m_results { get => base.m_results; set => base.m_results = value; }
    [PropertyField(0x794EA0DF, 31)] public new uint unknown_uint_3 { get => base.unknown_uint_3; set => base.unknown_uint_3 = value; }
    [PropertyField(0x88B9D287, 31)] public new ByteString unknown_str_3 { get => base.unknown_str_3; set => base.unknown_str_3 = value; }
    [PropertyField(0x8177DA98, 31)] public new TriggerObjectInfo m_triggerObjInfo { get => base.m_triggerObjInfo; set => base.m_triggerObjInfo = value; }
    [PropertyField(0x9E395DDD, 31)] public string m_requiredState { get; set; } = "";
    [PropertyField(0xCC89A0B0, 31)] public string m_nextState { get; set; } = "";
    [PropertyField(0xC7B0B26B, 31)] public string m_visibleState { get; set; } = "";
    [PropertyField(0x6A94A9CF, 31)] public string m_questEvent { get; set; } = "";
    [PropertyField(0x9E15CA2E, 31)] public string m_stringA { get; set; } = "";
    [PropertyField(0x5AB57C2D, 31)] public List<string> m_goalTags { get; set; } = [];
    [PropertyField(0x2D36C206, 31)] public bool m_flagA { get; set; }
    [PropertyField(0x13E34809, 31)] public bool m_flagB { get; set; }

}

/// <summary>
/// The interact option of mountable objects (Mount-2P_Rhino-001 and 153 others): requirements and results, a cooldown and display texts. A sibling of InteractStateOptionTemplate.
/// The class name is a guess: no name found in the client data hashes to 0x1455519F.
/// </summary>
[PropertySerializationTarget]
public partial record InteractCooldownOptionTemplate : InteractOptionTemplate {

    public override uint GetHash() => 0x1455519F;

    [PropertyField(0x92DEF557, 31)] public RequirementList? m_requirements { get; set; }
    [PropertyField(0xCA3EBE94, 31)] public ResultList? m_results { get; set; }
    [PropertyField(0x13E34809, 31)] public bool m_flagB { get; set; }
    [PropertyField(0x908C2F53, 31)] public string m_hideOnState { get; set; } = "";
    [PropertyField(0x5CA29C45, 31)] public bool m_useCooldown { get; set; }
    [PropertyField(0xC7B0B26B, 31)] public string m_visibleState { get; set; } = "";
    [PropertyField(0x767AAC3C, 31)] public float m_cooldown { get; set; }
    [PropertyField(0x2E8B9981, 31)] public float m_cooldownRand { get; set; }
    [PropertyField(0x6A94A9CF, 31)] public new string m_questEvent { get => base.m_questEvent; set => base.m_questEvent = value; }
    [PropertyField(0x5AB57C2D, 31)] public new List<string> m_goalTags { get => base.m_goalTags; set => base.m_goalTags = value; }
    [PropertyField(0x2D36C206, 31)] public bool m_flagA { get; set; }
    [PropertyField(0x766CDA0B, 31)] public string m_serviceResult { get; set; } = "";
    [PropertyField(0xB4338B9A, 31)] public string m_displayKey { get; set; } = "";
    [PropertyField(0x9274F5CD, 31)] public string m_iconKey { get; set; } = "";

}
