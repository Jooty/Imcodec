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

using Imcodec.CoreObject;
using Imcodec.Cryptography;
using Imcodec.IO;
using Imcodec.Math;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;

namespace Imcodec.Test.ObjectPropertyTest;

public sealed class ServerTypesTest {

    private static readonly string s_serverTypesObjectSerializer
        = """
        43ACDA06010000005B268C0615005472696767657254657374466F72496D636F64
        65630100000000000000000000000001000000080046697265644F6E2100000000
        01000000090046697265644F666621000000000000000000000000000000000000
        00000000
        """;

    [Fact]
    public void TryDeserializeServerTypes() {
        var serializer = new ObjectSerializer(false, SerializerFlags.None);
        var byteBlob = ConvertFromHexString(s_serverTypesObjectSerializer);
        var deserializeSuccess = serializer.Deserialize<WizZoneTriggers>(byteBlob, (PropertyFlags) 31, out var serverTypes);

        Assert.True(deserializeSuccess);
        Assert.NotNull(serverTypes);
    }

    [Fact]
    public void TryDispatchServerTypeDerivedFromServerType() {
        var serializer = new ObjectSerializer(true, SerializerFlags.None);
        var sigil = new PvPCombatSigilObjectInfo {
            m_sigilType = "PvPSigil8Actor2Sides",
            m_templateID = 123456,
            m_location = new Vector3(1f, 2f, 3f),
        };

        Assert.True(serializer.Serialize(sigil, (PropertyFlags) 1, out var buffer));
        Assert.True(serializer.Deserialize<CombatSigilObjectInfo>(buffer, (PropertyFlags) 1, out var decoded));

        var pvpSigil = Assert.IsType<PvPCombatSigilObjectInfo>(decoded);
        Assert.Equal(744574030u, pvpSigil.GetHash());
        Assert.Equal("PvPSigil8Actor2Sides", (string) pvpSigil.m_sigilType);
        Assert.Equal(123456ul, (ulong) pvpSigil.m_templateID);
        Assert.Equal(3f, pvpSigil.m_location.Z);
    }

    [Fact]
    public void CombatSigilFieldsKeepTheirHashes() {
        Assert.Equal(0x61B4E11Eu, StringHash.HashPropertyName("m_disablePlayerDecks", "bool"));

        var serializer = new ObjectSerializer(true, SerializerFlags.None);
        var sigil = new CombatSigilObjectInfo {
            m_sigilType = "CombatSigil8Actor",
            m_baseEnemyCount = -1,
            m_disablePlayerDecks = true,
        };

        Assert.True(serializer.Serialize(sigil, (PropertyFlags) 1, out var buffer));
        Assert.True(serializer.Deserialize<CombatSigilObjectInfo>(buffer, (PropertyFlags) 1, out var decoded));
        Assert.Equal(-1, decoded.m_baseEnemyCount);
        Assert.True(decoded.m_disablePlayerDecks);
    }

    [Fact]
    public void TryRoundTripPetGameKioskBehaviors() {
        foreach (var hex in s_petGameKioskBehaviors) {
            var kiosk = RoundTripBehavior<PetGameKioskBehaviorTemplate>(hex);

            Assert.Equal(0x6550ADFBu, kiosk.GetHash());
            Assert.Equal("PetGameKioskBehavior", (string) kiosk.m_behaviorName);
        }

        var names = s_petGameKioskBehaviors
            .Select(hex => RoundTripBehavior<PetGameKioskBehaviorTemplate>(hex).m_petGameName)
            .ToList();
        Assert.Equal(7, names.Count);
        Assert.Single(names, string.IsNullOrEmpty);
        Assert.Equal(6, names.Where(name => name.StartsWith("PetGame", StringComparison.Ordinal)).Distinct().Count());
    }

    [Fact]
    public void TryRoundTripPetSnackCollectorBehaviors() {
        var radii = new List<float>();
        foreach (var hex in s_petSnackCollectorBehaviors) {
            var collector = RoundTripBehavior<PetSnackCollectorBehaviorTemplate>(hex);

            Assert.Equal(0x6ABEDCD3u, collector.GetHash());
            Assert.Equal("PetSnackCollectorBehavior", (string) collector.m_behaviorName);
            radii.Add(collector.m_radius);
        }

        Assert.Equal([150f, 200f], radii.Order());
    }

    [Fact]
    public void TrySerializeNewPetGameKioskBehavior() {
        var serializer = new ObjectSerializer(true, SerializerFlags.CompactLength | SerializerFlags.StringEnums);
        var kiosk = new PetGameKioskBehaviorTemplate {
            m_behaviorName = new ByteString("PetGameKioskBehavior"u8.ToArray()),
            m_petGameName = "PetGameTest",
        };

        Assert.True(serializer.Serialize(kiosk, (PropertyFlags) 7, out var buffer));
        Assert.True(serializer.Deserialize<BehaviorTemplate>(buffer, (PropertyFlags) 7, out var decoded));

        var decodedKiosk = Assert.IsType<PetGameKioskBehaviorTemplate>(decoded);
        Assert.Equal("PetGameTest", decodedKiosk.m_petGameName);
        Assert.Equal("PetGameKioskBehavior", (string) decodedKiosk.m_behaviorName);
    }

    [Fact]
    public void TryRoundTripEveryRaceName() {
        // The pet and mount templates name their race as a string enum, so every option must survive a round trip.
        var serializer = new ObjectSerializer(true, SerializerFlags.CompactLength | SerializerFlags.StringEnums);
        foreach (var race in Enum.GetValues<eRace>()) {
            var template = new PetItemBehaviorTemplate { m_eRace = race };
            Assert.True(serializer.Serialize(template, (PropertyFlags) 7, out var buffer));
            Assert.True(serializer.Deserialize<PetItemBehaviorTemplate>(buffer, (PropertyFlags) 7, out var decoded));
            Assert.NotNull(decoded);
            Assert.Equal(race, decoded.m_eRace);
        }
    }

    private static T RoundTripBehavior<T>(string hex) where T : BehaviorTemplate {
        var serializer = new ObjectSerializer(true, SerializerFlags.CompactLength | SerializerFlags.StringEnums);
        var bytes = Convert.FromHexString(hex);

        Assert.True(serializer.Deserialize<BehaviorTemplate>(bytes, (PropertyFlags) 7, out var decoded));
        var behavior = Assert.IsType<T>(decoded);

        Assert.True(serializer.Serialize(behavior, (PropertyFlags) 7, out var reencoded));
        Assert.Equal(bytes, (byte[]) reencoded);

        return behavior;
    }

    /*
    The behaviors of the pet game kiosks and of the maze objects as the game data stores them: the class hash and the
    object, with compact lengths and string enums. These are every distinct byte string among the 7 kiosk behaviors
    and the 9 snack collector behaviors of the game data.
    */
    private static readonly string[] s_petGameKioskBehaviors = [
        "FBAD506550010000E80000002C849BBA2850657447616D654B696F736B4265686176696F72480000003FB1CBAB00",
        "FBAD5065B8010000E80000002C849BBA2850657447616D654B696F736B4265686176696F72B00000003FB1CBAB1A50657447616D6543616E6E6F6E",
        "FBAD5065B0010000E80000002C849BBA2850657447616D654B696F736B4265686176696F72A80000003FB1CBAB1850657447616D6544616E6365",
        "FBAD5065A8010000E80000002C849BBA2850657447616D654B696F736B4265686176696F72A00000003FB1CBAB1650657447616D6544726F70",
        "FBAD5065A8010000E80000002C849BBA2850657447616D654B696F736B4265686176696F72A00000003FB1CBAB1650657447616D654D617A65",
        "FBAD5065F8010000E80000002C849BBA2850657447616D654B696F736B4265686176696F72F00000003FB1CBAB2A50657447616D654F62737461636C65436F75727365",
        "FBAD5065B0010000E80000002C849BBA2850657447616D654B696F736B4265686176696F72A80000003FB1CBAB1850657447616D655369656765",
    ];

    private static readonly string[] s_petSnackCollectorBehaviors = [
        "D3DCBE6A90010000100100002C849BBA32506574536E61636B436F6C6C6563746F724265686176696F7260000000DF33F93A00001643",
        "D3DCBE6A90010000100100002C849BBA32506574536E61636B436F6C6C6563746F724265686176696F7260000000DF33F93A00004843",
    ];

    private static byte[] ConvertFromHexString(string hexString) {
        var cleanHex = new string([.. hexString.Where(c => !char.IsWhiteSpace(c))]);

        return [.. Enumerable.Range(0, cleanHex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(cleanHex.Substring(x, 2), 16))];
    }

}