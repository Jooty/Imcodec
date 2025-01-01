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
using Imcodec.IO;
using Imcodec.Types;
using Imcodec.Math;
using Imcodec.CoreObject;

namespace Imcodec.Test.CoreObjectTest;

public class DummyTypeRegistry : TypeRegistry {

    private static Dictionary<uint, System.Type> TypeMap { get; set; } = new() {
        { 350837933, typeof(ClientObject) },
        { 1152306685, typeof(CoreObject) },
        { 1200596153, typeof(RenderBehavior) },
        { 1653772158, typeof(WizClientObjectItem) }
    };

    public override void RegisterType(uint hash, System.Type t)
        => TypeMap[hash] = t;

    public override System.Type? LookupType(uint type)
        => TypeMap.TryGetValue(type, out var t) ? t : null;

}

public partial record WizClientObjectItem : ClientObject {

    public override uint GetHash() => 1653772158;

    [AutoProperty(900965981, 31)] public int m_primaryColor { get; set; }
    [AutoProperty(1337683384, 31)] public int m_pattern { get; set; }
    [AutoProperty(1616550081, 63)] public int m_secondaryColor { get; set; }
    [AutoProperty(1022427803, 63)] public GID m_displayID { get; set; }
    [AutoProperty(2004128457, 31)] public uint m_itemFlags { get; set; }

}

public partial record ClientObject : CoreObject {

    public override uint GetHash() => 350837933;

    [AutoProperty(210498418, 31)] public GID m_characterId { get; set; }
}

public partial record CoreObject : PropertyClass {

    public override uint GetHash() => 1152306685;

    [AutoProperty(1850812559, 31)] public List<BehaviorInstance>? m_inactiveBehaviors { get; set; }
    [AutoProperty(2312465444, 16777247)] public ulong m_globalID { get; set; }
    [AutoProperty(1298909658, 16777247)] public ulong m_permID { get; set; }
    [AutoProperty(2239683611, 31)] public Vector3 m_location { get; set; }
    [AutoProperty(2344058766, 31)] public Vector3 m_orientation { get; set; }
    [AutoProperty(503137701, 31)] public float m_fScale { get; set; }
    [AutoProperty(633907631, 31)] public ulong m_templateID { get; set; }
    [AutoProperty(3553984419, 31)] public ByteString m_debugName { get; set; }
    [AutoProperty(3023276954, 31)] public ByteString m_displayKey { get; set; }
    [AutoProperty(965291410, 31)] public uint m_zoneTagID { get; set; }
    [AutoProperty(123130076, 31)] public short m_speedMultiplier { get; set; }
    [AutoProperty(1054318939, 31)] public ushort m_nMobileID { get; set; }

}

public partial record RenderBehavior : BehaviorInstance {

    public override uint GetHash() => 1200596153;

}