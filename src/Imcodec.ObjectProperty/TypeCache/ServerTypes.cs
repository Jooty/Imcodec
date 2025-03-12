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

[PropertySerializationTarget]
public partial record WizZoneTriggers : PropertyClass {

    public override uint GetHash() => 0x06DAAC43;

}

[PropertySerializationTarget]
public partial record Trigger : PropertyClass {

    public override uint GetHash() => 0x068C265B;

    [PropertyField(0xB8C90C10, 31)] public ByteString m_triggerName { get; set; }
    [PropertyField(0x3933D634, 31)] public uint m_triggerMax { get; set; }
    [PropertyField(0x767AAC3C, 31)] public uint m_cooldown { get; set; }
    [PropertyField(0x2E8B9981, 31)] public uint m_cooldownRand { get; set; }
    [PropertyField(0x3282D78A, 31)] public bool m_pulsar { get; set; }
    [PropertyField(0x7DB09CC1, 31)] public List<ByteString>? m_activateEvents { get; set; }
    [PropertyField(0xA7BEADF6, 31)] public List<ByteString>? m_fireEvents { get; set; }
    [PropertyField(0x62A2160A, 31)] public List<ByteString>? m_deactivateEvents { get; set; }
    [PropertyField(0x5C548D5F, 31)] public List<ByteString>? m_unknown { get; set; }
    [PropertyField(0xA955FFA6, 31)] public RequirementList? m_requirements { get; set; }
    [PropertyField(0xE11C8ADA, 31)] public ResultList? m_results { get; set; }
    [PropertyField(0x794EA0DF, 31)] public uint unknown_uint_3 { get; set; }
    [PropertyField(0x88B9D287, 31)] public ByteString unknown_str_3 { get; set; }
    [PropertyField(0x8177DA98, 31)] public TriggerObjectInfo? m_triggerObjInfo { get; set; }

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
