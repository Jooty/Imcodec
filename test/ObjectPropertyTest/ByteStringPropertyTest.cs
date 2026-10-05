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
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;

namespace Imcodec.Test.ObjectPropertyTest;

public class ByteStringPropertyTest {

    [Fact]
    public void StdStringPropertyKeepsNonUtf8Bytes() {
        var raw = new byte[] { 0x80, 0xE9, 0xFF };
        var serializer = new ObjectSerializer(false);
        var source = new ActorRenameCinematicAction { m_newNamePattern = raw };

        Assert.True(serializer.Serialize(source, (PropertyFlags) 7, out var blob));
        Assert.True(serializer.Deserialize<ActorRenameCinematicAction>(blob, (PropertyFlags) 7, out var result));

        Assert.NotNull(result);
        Assert.Equal(raw, (byte[]) result.m_newNamePattern);
    }

    [Fact]
    public void EqualityComparesContent() {
        var a = new ByteString(new byte[] { 1, 2, 3 });
        var b = new ByteString(new byte[] { 1, 2, 3 });

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.False(a.Equals(new ByteString(new byte[] { 1, 2 })));
    }

    [Fact]
    public void ListContainsFindsByContent() {
        var list = new List<ByteString> { new(new byte[] { 0x61, 0x62 }), new("quest") };

        Assert.Contains(new ByteString(new byte[] { 0x61, 0x62 }), list);
        Assert.Contains((ByteString) "quest", list);
        Assert.DoesNotContain((ByteString) "other", list);
    }

    [Fact]
    public void DefaultEqualsEmpty() {
        var empty = new ByteString(new byte[0]);

        Assert.True(default(ByteString).Equals(empty));
        Assert.Equal(default(ByteString).GetHashCode(), empty.GetHashCode());
    }

}
