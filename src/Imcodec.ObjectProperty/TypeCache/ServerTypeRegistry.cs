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

namespace Imcodec.ObjectProperty.TypeCache;

public class ServerTypeRegistry : TypeRegistry {

    private readonly Dictionary<uint, System.Type> _types = [];

    public override System.Type? LookupType(uint hash) 
        => _types.TryGetValue(hash, out var type) ? type : null;

    // ctor
    public ServerTypeRegistry() { 
        // Get all the records that are a subclass of the current one.
        var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(PropertyClass)));
        
        // Register each type found.
        foreach (var type in types) {
            var instance = (PropertyClass)Activator.CreateInstance(type)!;
            RegisterType(instance.GetHash(), type);
        }
    }

    public override void RegisterType(uint hash, System.Type type) 
        => _types[hash] = type;

    public record WizZoneTriggers : PropertyClass {
        public override uint GetHash() => 0x06DAAC43;

        [AutoProperty(0x3F1DB764, 31)] public List<Trigger>? m_triggers { get; set; }
    }

    public record Trigger : PropertyClass {
        public override uint GetHash() => 0x068C265B;

        [AutoProperty(0xB8C90C10, 31)] public ByteString m_triggerName { get; set; }
        [AutoProperty(0x3933D634, 31)] public uint m_triggerMax { get; set; }
        [AutoProperty(0x767AAC3C, 31)] public uint m_cooldown { get; set; }
        [AutoProperty(0x2E8B9981, 31)] public uint m_cooldownRand { get; set; }
        [AutoProperty(0x3282D78A, 31)] public bool m_pulsar { get; set; }
        [AutoProperty(0x7DB09CC1, 31)] public List<ByteString>? m_activateEvents { get; set; }
        [AutoProperty(0xA7BEADF6, 31)] public List<ByteString>? m_fireEvents { get; set; }
        [AutoProperty(0x62A2160A, 31)] public List<ByteString>? m_deactivateEvents { get; set; }
        [AutoProperty(0x5C548D5F, 31)] public List<ByteString>? m_unknown { get; set; }
        [AutoProperty(0xA955FFA6, 31)] public RequirementList? m_requirements { get; set; }
        [AutoProperty(0xE11C8ADA, 31)] public ResultList? m_results { get; set; }
        [AutoProperty(0x794EA0DF, 31)] public uint unknown_uint_3 { get; set; }
        [AutoProperty(0x88B9D287, 31)] public ByteString unknown_str_3 { get; set; }
        [AutoProperty(0x8177DA98, 31)] public TriggerObjectInfo? m_triggerObjInfo { get; set; }
    }

    // ? Why does this exist?
    public record TriggerObjectBase : CoreObjectInfo {
        // todo: need actual hashes here
        public override uint GetHash() => 0xFFFFFF;

        // Why would a trigger have a location? Isn't this what volumes are for?
        [AutoProperty(0x7DB3F828, 31)] public float m_locationX { get; set; }
        [AutoProperty(0x7DB3F829, 31)] public float m_locationY { get; set; }
        [AutoProperty(0x7DB3F82A, 31)] public float m_locationZ { get; set; }
    }

    public record TriggerObjectInfo : TriggerObjectBase {
        // todo: need actual hashes here
        public override uint GetHash() => 0xFFFFFF;
    }

    public record WizZoneVolumes : PropertyClass {
        public override uint GetHash() => 0x1B6EF770;

        [AutoProperty(0x884BFB48, 31)] public List<Volume>? m_volumes { get; set; }
    }

    public record Volume : CoreObjectInfo {
        public override uint GetHash() => 0x1B7B55F6;

        // CoreObjectInfo properties end here.
        [AutoProperty(0xC6E6048B, 31)] public ByteString m_volumeName { get; set; }
        [AutoProperty(0x7DB3F828, 31)] public float m_locationX { get; set; }
        [AutoProperty(0x7DB3F829, 31)] public float m_locationY { get; set; }
        [AutoProperty(0x7DB3F82A, 31)] public float m_locationZ { get; set; }
        [AutoProperty(0x40183401, 31)] public new ulong m_templateID { get; set; }
        [AutoProperty(0x8987B2CC, 31)] public ByteString m_primitiveType { get; set; }
        [AutoProperty(0x3AF933DF, 31)] public float m_radius { get; set; }
        [AutoProperty(0x2D481539, 31)] public float m_length { get; set; }
        [AutoProperty(0x35EBF597, 31)] public float m_width { get; set; }
        [AutoProperty(0x3492258C, 31)] public int unknown_int { get; set; }
        [AutoProperty(0x3B3CD5DA, 31)] public bool unknown_1 { get; set; }
        [AutoProperty(0x71FCB022, 31)] public byte unknown_2 { get; set; }
        [AutoProperty(0x8576192E, 31)] public List<ByteString>? m_enterEvents { get; set; }
        [AutoProperty(0xAB57CF4A, 31)] public List<ByteString>? m_exitEvents { get; set; }
    }

    public record ResTeleport : TypeCache.Result {
        public override uint GetHash() => 228794493;

        public string? m_destinationLoc { get; set; }
        public string? m_destinationZone { get; set; }
        [AutoProperty(0x2, 31)] public byte m_exitTeleporter { get; set; }
        [AutoProperty(0x3, 31)] public byte m_teleporterTag { get; set; }
        [AutoProperty(0x4, 31)] public TeleportType m_teleportType { get; set; }
        [AutoProperty(0x5, 31)] public byte m_transitionID { get; set; }

        public enum TeleportType {
            TELEPORT_STATIC,
        }
    }

    public record ResDisplayText : TypeCache.Result {
        public override uint GetHash() => 0x774C0B33;

        [AutoProperty(0x66603160, 31)] public ByteString m_text { get; set; }
        [AutoProperty(0x0D1B703C, 31)] public int m_type { get; set; }
        [AutoProperty(0x3AF933DF, 31)] public float m_radius { get; set; }
        [AutoProperty(0x431157E7, 31)] public float m_locationX { get; set; }
        [AutoProperty(0x431157E8, 31)] public float m_locationY { get; set; }
        [AutoProperty(0x431157E9, 31)] public float m_locationZ { get; set; }

        [AutoProperty(0x2EB6A55F, 31)] public bool m_unknown_bool { get; set; }
        [AutoProperty(0x7E84339F, 31)] public bool m_unknown_bool_2 { get; set; }
        [AutoProperty(0x57EDA63C, 31)] public bool m_unknown_bool_3 { get; set; }
    }

    public record ResPlaySound : TypeCache.Result {
        public override uint GetHash() => 0x3C626744;

        [AutoProperty(0x444373FA, 31)] public ZoneRouter? m_router { get; set; }
        [AutoProperty(0x87BA8BE5, 31)] public ByteString m_soundName { get; set; }
        [AutoProperty(0x3B9498D7, 31)] public bool m_blocking { get; set; }
        [AutoProperty(0x2C2BC314, 31)] public float m_reinteractTime { get; set; }
    }

    public record ZoneRouter : PropertyClass {
        public override uint GetHash() => 0xDA51FA8;

        [AutoProperty(0x12773D2D, 31)] public float m_locX { get; set; }
        [AutoProperty(0x12773D2E, 31)] public float m_locY { get; set; }
        [AutoProperty(0x12773D2F, 31)] public float m_locZ { get; set; }
        [AutoProperty(0xC7FCACAC, 31)] public RoutingType m_routingType { get; set; }
        [AutoProperty(0xE36CE99, 31)] public bool m_useLocation { get; set; }
        [AutoProperty(0x148E0B6D, 31)] public bool m_useTriggerLocation { get; set; }

        public enum RoutingType {
            ROUTING_ACTOR,
            ROUTING_ZONE,
            ROUTING_PROXIMITY,
        }
    }

    public record ResPlayCinematic : TypeCache.Result {
        public override uint GetHash() => 16312488;

        [AutoProperty(2611527497, 134217735)] public ByteString m_cinematicName { get; set; }
        [AutoProperty(0x444373FA, 31)] public ZoneRouter? m_router { get; set; }
        [AutoProperty(0x1D70805C, 31)] public bool m_unknown_bool_1 { get; set; }
        [AutoProperty(0x3AAF6E2F, 31)] public bool m_unknown_bool_2 { get; set; }
        [AutoProperty(0x61436E16, 31)] public bool m_unknown_bool_3 { get; set; }
        [AutoProperty(0x78B7B1EE, 31)] public ByteString m_unknown_string_1 { get; set; }
        [AutoProperty(0x3C1B4C58, 31)] public bool m_unknown_bool_4 { get; set; }
        [AutoProperty(0x5BB196FF, 31)] public bool m_unknown_bool_5 { get; set; }
        [AutoProperty(0x7B00E397, 31)] public ByteString m_unknown_string_2 { get; set; }
        [AutoProperty(0x197BBD69, 31)] public bool m_unknown_bool_6 { get; set; }
        [AutoProperty(0x4FA58BBA, 31)] public float m_unknown_float_1 { get; set; }
        [AutoProperty(0x3DAC4C0A, 31)] public bool m_unknown_bool_7 { get; set; }
        [AutoProperty(0x66ECE9B3, 31)] public ByteString m_unknown_string_3 { get; set; }
        [AutoProperty(0xA4092DFC, 31)] public bool m_unknown_bool_8 { get; set; }
        [AutoProperty(0x61437E16, 31)] public bool m_unknown_bool_9 { get; set; }
    }

    public record ResActorDialog : TypeCache.Result {
        public override uint GetHash() => 338444955;

        // AutoProperty: m_dialogPrefix (unknown)

        [AutoProperty(1972573231, 7)] public ByteString m_activePersona { get; set; }
        [AutoProperty(3056380390, 7)] public ByteString m_registryEntry { get; set; }
        [AutoProperty(1310859212, 7)] public ActorDialog? m_dialog { get; set; }
        [AutoProperty(2305471533, 31)] public ByteString m_quest { get; set; }
        [AutoProperty(2057644325, 7)] public bool m_broadcastToZone { get; set; }
        [AutoProperty(482239118, 7)] public bool m_displayInQuestList { get; set; }
        [AutoProperty(707400068, 7)] public bool m_oneShot { get; set; }
    }

    public record ResAddGold : TypeCache.Result {
        public override uint GetHash() => 703672453;

        [AutoProperty(219423808, 7)] public int m_gold { get; set; }
        [AutoProperty(2305508654, 7)] public ByteString m_sourceType { get; set; }
    }

    public record ResAddMagicXP : TypeCache.Result {
        public override uint GetHash() => 1320311385;

        // AutoProperty: m_experience (int)
        // AutoProperty: m_magicSchool (string)
        [AutoProperty(2305508654, 7)] public ByteString m_sourceType { get; set; }
    }

    public record ResLoot : TypeCache.Result {
        public override uint GetHash() => 475964190;

        // AutoProperty: m_lootTable (string)
    }

    public record ResPostQuestEvent : TypeCache.Result {
        public override uint GetHash() => 1936095675;

        [AutoProperty(3493260286, 7)] public ByteString m_eventName { get; set; }
        // AutoProperty: m_subEventName (unknown)
    }

    public record ResAddSpell : TypeCache.Result {
        public override uint GetHash() => 1774023420;

        // AutoProperty: m_spellName (string)
    }

    public record ResAddTrainingPoints : TypeCache.Result {
        public override uint GetHash() => 1627552040;

        // AutoProperty: m_sourceType (string)
        // AutoProperty: m_trainingPoints (int)
    }

    public record ResModifyEntry : TypeCache.Result {
        public override uint GetHash() => 185712126;

        // AutoProperty: m_questName (unknown)

        [AutoProperty(2055270734, 7)] public ByteString m_entryName { get; set; }
        [AutoProperty(1388902362, 7)] public bool m_isQuestRegistry { get; set; }
        [AutoProperty(812990455, 7)] public int m_value { get; set; }
        [AutoProperty(1702112846, 7)] public ByteString m_questName { get; set; }
    }

    public record ResIncrementEntry : TypeCache.Result {
        public override uint GetHash() => 461317711;

        // AutoProperty: m_entryName (string)
        // AutoProperty: m_isQuestRegistry (int)
        // AutoProperty: m_questName (unknown)
    }

    public record ResAddMissionDoor : TypeCache.Result {
        public override uint GetHash() => 1202940500;

        // AutoProperty: m_advanced (int)
        // AutoProperty: m_clientTag (unknown)
        // AutoProperty: m_missionDoorLoc (unknown)
        // AutoProperty: m_missionDoorTag (string)
        // AutoProperty: m_missionDoorZone (unknown)
        // AutoProperty: m_useQuestAsOriginator (int)
    }

    public record ResItemLoot : TypeCache.Result {
        public override uint GetHash() => 1989908347;

        // AutoProperty: m_itemTemplateID (int)
        // AutoProperty: m_lootOptions (string)
        // AutoProperty: m_sendLootMessage (int)
        // AutoProperty: m_sourceType (string)
    }

    public record ResAddMaxGold : TypeCache.Result {
        public override uint GetHash() => 1875038822;

        // AutoProperty: m_maxGoldToAdd (int)
    }

    public record ResDeleteItem : TypeCache.Result {
        public override uint GetHash() => 822278902;

        // AutoProperty: m_itemTemplateID (int)
        // AutoProperty: m_quantity (int)
        // AutoProperty: m_sourceType (string)
    }

    public record ResAddHealth : TypeCache.Result {
        public override uint GetHash() => 794463279;

        // AutoProperty: m_healthFlat (int)
        // AutoProperty: m_healthPercent (float)
        // AutoProperty: m_useFlat (int)
    }

    public record ResPostEvent : TypeCache.Result {
        public override uint GetHash() => 412163800;

        // AutoProperty: m_eventName (string)
        // AutoProperty: m_subEventName (unknown) // Deserialization fails if this is made a string. 72 bits

        [AutoProperty(3493260286, 31)] public string? m_eventName { get; set; }
    }

    public record ResRemoveDynaMod : TypeCache.Result {
        public override uint GetHash() => 552459800;

        [AutoProperty(1601665858, 31)] public ByteString m_dynaModClientTag { get; set; }
        [AutoProperty(1110452292, 31)] public bool m_useQuestAsOriginator { get; set; }
    }

    public record ResAddDynaMod : TypeCache.Result {
        public override uint GetHash() => 3938346;

        [AutoProperty(1601665858, 31)] public ByteString m_dynaModClientTag { get; set; }
        [AutoProperty(1110452292, 31)] public bool m_dynaModRemove { get; set; }
        [AutoProperty(1494302749, 31)] public bool m_useQuestAsOriginator { get; set; }
        [AutoProperty(2072855560, 31)] public ByteString m_dynaModState { get; set; }
        [AutoProperty(2171167736, 8388615)] public ByteString m_zoneName { get; set; }
    }

    public record ResWait : TypeCache.Result {
        public override uint GetHash() => 526762782;

        [AutoProperty(2403101108, 31)] public uint m_secondsToWait { get; set; }
    }

    public record ResEmote : TypeCache.Result {
        public override uint GetHash() => 475497368;

        // AutoProperty: m_emoteName (string)
        // AutoProperty: m_emoteState (string)
        // AutoProperty: m_loop (int)
        // AutoProperty: m_particleAsset (unknown)
        // AutoProperty: m_particleNode (unknown)
        // AutoProperty: m_personaName (unknown)
        // AutoProperty: m_soundAsset (unknown)
        // AutoProperty: m_usePersona (int)
        // AutoProperty: m_useTarget (int)
    }

    public record ResAddEncounterXP : TypeCache.Result {
        public override uint GetHash() => 798482874;

        // AutoProperty: m_experience (int)
    }

    public record ResMarkZoneNoWarn : TypeCache.Result {
        public override uint GetHash() => 1534247075;

    }

    public record ResDownloadPackage : TypeCache.Result {
        public override uint GetHash() => 807468757;

        [AutoProperty(2325737891, 31)] public List<ByteString>? m_packageList { get; set; }
    }

    public record ResAddTreasureSpell : TypeCache.Result {
        public override uint GetHash() => 1787561491;

        // AutoProperty: m_sourceType (string)
        // AutoProperty: m_spellName (string)
    }

    public record ResModifyTriggerObject : TypeCache.Result {
        public override uint GetHash() => 1263108441;

        [AutoProperty(3336963211, 31)] public ByteString m_triggerObjName { get; set; }
        [AutoProperty(2067625387, 31)] public bool m_triggerObjState { get; set; }
    }

    public record ResSpawn : TypeCache.Result {
        public override uint GetHash() => 723600258;

        // Not confident about these types
        [AutoProperty(1481718190, 31)] public ulong m_spawnID { get; set; }
        [AutoProperty(142527940, 31)] public bool m_activate { get; set; }
    }

    public record ResReInteract : TypeCache.Result {
        public override uint GetHash() => 682171658;

        // AutoProperty: m_actorType (string)
        // AutoProperty: m_delay (int)
        // AutoProperty: m_personaName (string)
        // AutoProperty: m_source (int)
    }

    public record ResDownloadElement : TypeCache.Result {
        public override uint GetHash() => 101365432;

        // AutoProperty: m_elementPackageList (string)
    }

    public record ResRemoveTriggerObject : TypeCache.Result {
        public override uint GetHash() => 803366521;

        // AutoProperty: m_triggerObjName (string)
    }

    public record ResCompleteQuestGoal : TypeCache.Result {
        public override uint GetHash() => 1279893472;

        // AutoProperty: m_goalName (string)
        // AutoProperty: m_questName (string)
    }

    public record ResAddBadge : TypeCache.Result {
        public override uint GetHash() => 2041850521;

        // AutoProperty: m_badgeName (string)
    }

    public record ResAddEncounter : TypeCache.Result {
        public override uint GetHash() => 798724538;

        // AutoProperty: m_experience (int)
    }

    public record ResDespawn : TypeCache.Result {
        public override uint GetHash() => 1383450208;

        // AutoProperty: m_despawnEffect (unknown) // uint perhaps?
        // AutoProperty: m_spawnID (int)
        // AutoProperty: m_templateID (int)
    }

    public record ResRemoveEntry : TypeCache.Result {
        public override uint GetHash() => 1874483934;

        // AutoProperty: m_entryName (string)
        // AutoProperty: m_isQuestRegistry (int)
        // AutoProperty: m_questName (unknown)
    }

    public record ResCinematicActor : TypeCache.Result {
        public override uint GetHash() => 16312488;

        // AutoProperty: m_blocking (int)
        // AutoProperty: m_cinematicName (string)
        // AutoProperty: m_endAtActor (int)
        // AutoProperty: m_endAtTargetActor (int)
        // AutoProperty: m_endLoc (string)
        // AutoProperty: m_objectTemplateID (int)
        // AutoProperty: m_router (string)
        // AutoProperty: m_routing (string)
        // AutoProperty: m_startAtActor (int)
        // AutoProperty: m_startAtTargetActor (int)
        // AutoProperty: m_startLoc (string)
        // AutoProperty: m_unique (int)
        // AutoProperty: m_uniqueBusyMsg (unknown)
        // AutoProperty: m_uniqueName (unknown)
    }

    public record ResFallthrough : TypeCache.Result {
        public override uint GetHash() => 1962253934;

        // AutoProperty: m_options (string)
    }

    public record ResultOption : TypeCache.Result {
        public override uint GetHash() => 1471095109;

        // AutoProperty: m_requirements (string)
        // AutoProperty: m_results (string)
    }

    public record ResMaxPotions : TypeCache.Result {
        public override uint GetHash() => 309059205;

        // AutoProperty: m_potionsToAdd (int)
        // AutoProperty: m_sourceType (string)
    }

    public record ResRemoveMissionDoor : TypeCache.Result {
        public override uint GetHash() => 1714192944;

        // AutoProperty: m_missionDoorTag (string)
        // AutoProperty: m_useQuestAsOriginator (int)
    }

    public record ResAddCraftingSlot : TypeCache.Result {
        public override uint GetHash() => 633964307;

        // AutoProperty: m_slotDelta (int)
        // AutoProperty: m_sourceType (string)
    }

    public record ResTimeStampEntry : TypeCache.Result {
        public override uint GetHash() => 1826857186;

        // AutoProperty: m_coolDownTime (int)
        // AutoProperty: m_registryEntry (string)
    }

    public record ResToggleQuestEffect : TypeCache.Result {
        public override uint GetHash() => 1383058250;

        // AutoProperty: m_addEffect (int)
        // AutoProperty: m_effectName (string)
    }

    public record ResDespawnLeashedObject : TypeCache.Result {
        public override uint GetHash() => 577427349;

        // AutoProperty: m_followerTemplateID (int)
    }

    public record ResRemoveEffect : TypeCache.Result {
        public override uint GetHash() => 837967761;

        // AutoProperty: m_effectName (string)
        // AutoProperty: m_useOriginatorID (int)
    }

    public record ResSpawnLeashedObject : TypeCache.Result {
        public override uint GetHash() => 1505576005;

        // AutoProperty: m_followerTemplateID (int)
    }

    public record ResAddEffect : TypeCache.Result {
        public override uint GetHash() => 53623319;

        // AutoProperty: m_effectName (unknown)
        // AutoProperty: m_playerOnly (int)
        // AutoProperty: m_spEffectInfo (string)
    }

    public record ResAddTriggerObject : TypeCache.Result {
        public override uint GetHash() => 1893729846;

        // AutoProperty: m_triggerObjName (string)
        // AutoProperty: m_triggerObjState (string)
    }

    public record ResSetPips : TypeCache.Result {
        public override uint GetHash() => 1375195018;

        // AutoProperty: m_numPips (int)
        // AutoProperty: m_subCircle (int)
    }

    public record ResClearHand : TypeCache.Result {
        public override uint GetHash() => 1100956089;

        // AutoProperty: m_subCircle (int)
    }

    public record ResGiveSpell : TypeCache.Result {
        public override uint GetHash() => 151650171;

        // AutoProperty: m_spellName (string)
        // AutoProperty: m_subCircle (int)
    }

    public record ResUpdatePips : TypeCache.Result {
        public override uint GetHash() => 266932982;

    }

    public record ResSyncScript : TypeCache.Result {
        public override uint GetHash() => 919520188;

        // AutoProperty: m_function (string)
        // AutoProperty: m_script (string)
    }

    public record ResAddEnergy : TypeCache.Result {
        public override uint GetHash() => 43418311;

        // AutoProperty: m_energyFlat (int)
        // AutoProperty: m_energyPercent (int)
        // AutoProperty: m_sourceType (string)
        // AutoProperty: m_useFlat (int)
    }

    public record ResAddElixir : TypeCache.Result {
        public override uint GetHash() => 66638275;

        // AutoProperty: m_sourceType (string)
        // AutoProperty: m_templateID (int)
    }

    public record ResGivePowerPip : TypeCache.Result {
        public override uint GetHash() => 1254124953;

        // AutoProperty: m_subCircle (int)
    }

    public record ResSetGardeningLevel : TypeCache.Result {
        public override uint GetHash() => 875336888;

        // AutoProperty: m_level (int)
    }

    public record ResDownloadBrowser : TypeCache.Result {
        public override uint GetHash() => 2128108307;

    }

    public record ResSetFishingLevel : TypeCache.Result {
        public override uint GetHash() => 638582544;

        // AutoProperty: m_level (int)
    }

    public record ResAddMana : TypeCache.Result {
        public override uint GetHash() => 1980688359;

        [AutoProperty(2305508654, 7)] public ByteString m_sourceType { get; set; }
        [AutoProperty(1926272286, 7)] public int m_manaFlat { get; set; }
        [AutoProperty(1293619909, 7)] public float m_manaPercent { get; set; }
        [AutoProperty(138922614, 7)] public int m_overfill { get; set; }
        [AutoProperty(2040055687, 7)] public bool m_useFlat { get; set; }
    }

    public record ResClearSpellbook : TypeCache.Result {
        public override uint GetHash() => 1887868329;

    }

    public record ResClearExperience : TypeCache.Result {
        public override uint GetHash() => 1952853362;

    }

    public record ResShowGUI : TypeCache.Result {
        public override uint GetHash() => 742393364;

        [AutoProperty(2274463339, 31)] public ByteString m_guiDisplay { get; set; }
        [AutoProperty(2717603296, 31)] public ByteString m_guiFile { get; set; }
    }

    public record ResCinematic : TypeCache.Result {
        public override uint GetHash() => 82637767;

        // AutoProperty: m_blocking (int)
        // AutoProperty: m_cinematicName (string)
        // AutoProperty: m_endAtActor (int)
        // AutoProperty: m_endAtTargetActor (int)
        // AutoProperty: m_endLoc (string)
        // AutoProperty: m_objectTemplateID (int)
        // AutoProperty: m_router (string)
        // AutoProperty: m_routing (string)
        // AutoProperty: m_startAtActor (int)
        // AutoProperty: m_startAtTargetActor (int)
        // AutoProperty: m_startLoc (string)
        // AutoProperty: m_unique (int)
        // AutoProperty: m_uniqueBusyMsg (unknown)
        // AutoProperty: m_uniqueName (unknown)
    }

    public record ResAddRecipe : TypeCache.Result {
        public override uint GetHash() => 1895914322;

        // AutoProperty: m_recipeName (string)
        // AutoProperty: m_sourceType (string)
    }

    public record ResClientNotifyText : TypeCache.Result {
        public override uint GetHash() => 2001472307;

        // AutoProperty: m_allInZone (int)
        // AutoProperty: m_text (string)
        // AutoProperty: m_type (int)
    }

    public record ResControlBackgroundMusic : TypeCache.Result {
        public override uint GetHash() => 1144211986;

        // AutoProperty: m_action (string)
        // AutoProperty: m_fadeTime (int)
        // AutoProperty: m_router (string)

        [AutoProperty(1734553689, 31)] public ByteString m_action { get; set; }
    }

    public record ResUnlockShadowMagic : TypeCache.Result {
        public override uint GetHash() => 513283589;

    }

    public record ResStartStagedCinematic : TypeCache.Result {
        public override uint GetHash() => 145615551;

        // AutoProperty: m_bIncludeAllPlayersInZone (int)
        // AutoProperty: m_cinematicName (string)
        // AutoProperty: m_stageName (string)
    }

    public record ResReduceMana : TypeCache.Result {
        public override uint GetHash() => 74783451;

        // AutoProperty: m_manaPercent (float)
    }

    public record ResInitiateCombat : TypeCache.Result {
        public override uint GetHash() => 1486342711;

        // AutoProperty: m_aggroActor (int)
        // AutoProperty: m_aggroRadius (int)
        // AutoProperty: m_aggroTarget (int)
        // AutoProperty: m_allPlayers (bool)
        // AutoProperty: m_sigilLabel (string)
    }

    public record CombatSigilObjectInfo : CoreObjectInfo {
        public override uint GetHash() => 478486736;

        // Properties here are listed in order of their understanding.
        // The template for this object is a DynamicTriggerTemplate.

        [AutoProperty(0x7B91Df78, 31)] public ByteString m_sigilType { get; set; }
        [AutoProperty(0xADC3A56F, 31)] public ByteString m_zoneTag2 { get; set; }
        [AutoProperty(0x3AF933DF, 31)] public float m_radius { get; set; }
        [AutoProperty(0x595FC144, 31)] public int m_firstTeamToAct { get; set; }
        [AutoProperty(0x4AFCF400, 2097183)] public SigilInitiativeSwitchMode m_initiativeSwitchMode { get; set; }
        [AutoProperty(0x203340FD, 31)] public int m_initiativeSwitchRounds { get; set; }
        [AutoProperty(0x975DE361, 268435463)] public List<ByteString>? m_lootTable { get; set; }
        [AutoProperty(0x5DB0B6E8, 31)] public bool m_disableTimer { get; set; }
        [AutoProperty(0x7DB09CC1, 31)] public List<ByteString>? m_activateEvents { get; set; }
        [AutoProperty(0x62A2160A, 31)] public List<ByteString>? m_deactivateEvents { get; set; }

        // HASH : 0x71FCB022
        // SIZE : 65 bits
        // Very confident about the type.
        [AutoProperty(0x71FCB022, 31)] public bool m_unknown_boolean_1 { get; set; }

        // HASH : 0x3BF5B2D
        // SIZE : 65 bits
        // Very confident about the type.
        [AutoProperty(0x3BF5B2D, 31)] public bool m_unknown_boolean_2 { get; set; }

        // HASH : 0x3C345132
        // SIZE : 72 bits
        [AutoProperty(0x3C345132, 31)] public bool m_unknown_boolean_3 { get; set; }

        // HASH : 0x61B4E11E
        // SIZE : 72 bits
        [AutoProperty(0x61B4E11E, 31)] public bool m_unknown_boolean_4 { get; set; }

        // HASH : 0x37BEB1CF
        // SIZE : 8
        [AutoProperty(0x37BEB1CF, 31)] public int m_unknown_uint_2 { get; set; }

        // HASH : 0x62794D39
        // SIZE : 8
        [AutoProperty(0x62794D39, 31)] public uint m_unknown_uint_3 { get; set; }

        // HASH : 0x6FA14D24
        // SIZE : 8
        [AutoProperty(0x6FA14D24, 31)] public uint m_unknown_uint_4 { get; set; }
    }

    public record WizBangPriorityTemplate : PropertyClass {
        public override uint GetHash() => 511049413;

        [AutoProperty(2003411223, 31)] public List<ByteString>? m_priorityList { get; set; }
    }

    public record UnknownSpawnObjectInfo : SpawnObjectInfo {
        public override uint GetHash() => 1839222684;

        [AutoProperty(0x975DE361, 268435463)] public List<ByteString>? m_lootTable { get; set; }
    }

    public record MinigameSigilInfo : CoreObjectInfo {
        public override uint GetHash() => 234614075;

        [AutoProperty(0x7B91Df78, 31)] public ByteString m_sigilType { get; set; }
        [AutoProperty(0xADC3A56F, 31)] public ByteString m_zoneTag2 { get; set; }
        [AutoProperty(0x3AF933DF, 31)] public float m_radius { get; set; }
        [AutoProperty(0x71FCB022, 31)] public byte unknown_2 { get; set; }
        [AutoProperty(0x3C345132, 31)] public bool m_unknown_boolean_3 { get; set; }
        [AutoProperty(0x7DB09CC1, 31)] public List<ByteString>? m_activateEvents { get; set; }
        [AutoProperty(0x62A2160A, 31)] public List<ByteString>? m_deactivateEvents { get; set; }
        [AutoProperty(0x3BF5B2D, 31)] public bool m_unknown_boolean_2 { get; set; }
        [AutoProperty(0xA955FFA6, 31)] public RequirementList? m_requirements { get; set; }

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

}