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

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;

namespace Imcodec.Benchmarks;

[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ObjectSerializerBenchmarks {

    private ObjectSerializer _serializer = null!;
    private LootInfoList _lootTable = null!;
    private byte[] _serializedData = [];
    private byte[] _compressedData = [];
    private const string LootTableBlob = "2A0367480100000089876B65050000000000050000005E39841B0100000002000000";
    private const string LootTableBlobCompressed = "2600000078DAD3624EF760646060E86CCF4E65650001101967D9220D12656280000067CB0401";

    [GlobalSetup]
    public void Setup() {
        // Initialize the serializer and test data
        _serializer = new ObjectSerializer(false, SerializerFlags.None);

        // Create a sample loot table
        _lootTable = new LootInfoList {
            m_goldInfo = new GoldLootInfo {
                m_goldAmount = 2,
                m_lootType = LOOT_TYPE.LOOT_TYPE_GOLD
            },
            m_loot = [
                new MagicXPLootInfo
                {
                    m_lootType = LOOT_TYPE.LOOT_TYPE_MAGIC_XP,
                    m_experience = 5
                }
            ]
        };

        // Pre-serialize data for deserialization benchmark
        _serializedData = Convert.FromHexString(LootTableBlob);
        _compressedData = Convert.FromHexString(LootTableBlobCompressed);
    }

    [Benchmark]
    public void SerializeLootTable() {
        _serializer.Serialize(_lootTable, (PropertyFlags) 31, out var _);
    }

    [Benchmark]
    public void DeserializeLootTable() {
        _serializer.Deserialize<LootInfoList>(_serializedData, (PropertyFlags) 31, out var _);
    }

    [Benchmark]
    public void SerializeWithCompression() {
        var compressedSerializer = new ObjectSerializer(false, SerializerFlags.Compress);
        compressedSerializer.Serialize(_lootTable, (PropertyFlags) 31, out var _);
    }

    [Benchmark]
    public void DeserializeWithCompression() {
        var compressedSerializer = new ObjectSerializer(false, SerializerFlags.Compress);
        compressedSerializer.Deserialize<LootInfoList>(_compressedData, (PropertyFlags) 31, out var _);
    }

}

public class Program {

    public static void Main(string[] args) {
        var summary = BenchmarkRunner.Run<ObjectSerializerBenchmarks>();
    }

}