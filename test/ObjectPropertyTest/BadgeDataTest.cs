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

public sealed class BadgeDataTest {

    /*
    The m_badgeData string of the first player object of a capture, without the u16 length (0x1A) the property
    serializer writes in front of it: the magic, version 1, one claimed reward, one new badge, no pending overcount reward.
    */
    private const string CapturedBlob = "CEFACE2A010000000100" + "13B01555" + "0100" + "95F09624" + "86C4E568" + "0000";

    private const string EmptyBlob = "CEFACE2A01000000" + "000000000000";

    [Fact]
    public void TryDecodeCapturedBlob() {
        var blob = Convert.FromHexString(CapturedBlob);

        Assert.Equal(26, blob.Length);
        var data = Decode(blob);

        Assert.Equal([0x5515B013u], data.ClaimedRewards);
        Assert.Equal([new BadgeData.NewBadge(0x2496F095u, 0x68E5C486u)], data.NewBadges);
        Assert.Empty(data.PendingOvercountRewards);
    }

    [Fact]
    public void TryEncodeCapturedBlobByteIdentical() {
        var blob = Convert.FromHexString(CapturedBlob);
        var data = Decode(blob);

        Assert.Equal(blob, (byte[]) data.ToByteString());

        using var writer = new BitWriter();
        data.Encode(writer);
        Assert.Equal(blob, writer.GetData());
    }

    [Fact]
    public void TryRoundTripEmptyData() {
        var empty = new BadgeData();
        var blob = empty.ToByteString();

        Assert.Equal(Convert.FromHexString(EmptyBlob), (byte[]) blob);
        var decoded = Decode(blob);
        Assert.Empty(decoded.ClaimedRewards);
        Assert.Empty(decoded.NewBadges);
        Assert.Empty(decoded.PendingOvercountRewards);
        Assert.Equal((byte[]) blob, (byte[]) decoded.ToByteString());
    }

    [Fact]
    public void TryRoundTripEveryList() {
        var data = new BadgeData {
            ClaimedRewards = [7, 1, uint.MaxValue],
            NewBadges = [new(5, 100), new(2, 50)],
            PendingOvercountRewards = [9, 3, 3, 8],
        };

        var decoded = Decode(data.ToByteString());

        Assert.Equal(data.ClaimedRewards, decoded.ClaimedRewards);
        Assert.Equal(data.NewBadges, decoded.NewBadges);
        Assert.Equal(data.PendingOvercountRewards, decoded.PendingOvercountRewards);
    }

    [Fact]
    public void TryRoundTripListsAtTheCountLimit() {
        var data = new BadgeData {
            ClaimedRewards = [.. Enumerable.Range(0, ushort.MaxValue).Select(i => (uint) i)],
        };

        var decoded = Decode(data.ToByteString());
        Assert.Equal(data.ClaimedRewards, decoded.ClaimedRewards);
    }

    [Fact]
    public void TryEncodeListLongerThanItsCountThrows() {
        var data = new BadgeData {
            PendingOvercountRewards = [.. Enumerable.Repeat(1u, ushort.MaxValue + 1)],
        };

        Assert.Throws<InvalidOperationException>(() => data.ToByteString());
    }

    [Fact]
    public void TryDecodeEmptyStringAsEmptyLists() {
        // The client skips loading an empty m_badgeData, which leaves its lists empty.
        foreach (var blob in new[] { ByteString.Empty, new ByteString([]) }) {
            var decoded = Decode(blob);

            Assert.Empty(decoded.ClaimedRewards);
            Assert.Empty(decoded.NewBadges);
            Assert.Empty(decoded.PendingOvercountRewards);
        }
    }

    [Fact]
    public void TryDecodeRejectsMalformedBlobs() {
        var captured = Convert.FromHexString(CapturedBlob);

        var wrongMagic = (byte[]) captured.Clone();
        wrongMagic[0] ^= 0xFF;

        var wrongVersion = (byte[]) captured.Clone();
        wrongVersion[4] = 2;

        Assert.False(BadgeData.TryFromByteString(wrongMagic, out var data));
        Assert.Null(data);
        Assert.False(BadgeData.TryFromByteString(wrongVersion, out _));
        Assert.False(BadgeData.TryFromByteString(captured[..^1], out _));
        Assert.False(BadgeData.TryFromByteString(captured[..10], out _));
        Assert.False(BadgeData.TryFromByteString(new byte[] { 0xCE }, out _));
        Assert.False(BadgeData.TryFromByteString(captured.Append((byte) 0).ToArray(), out _));
    }

    [Fact]
    public void TryCarryBlobInBadgeBehaviorProperty() {
        // The property serializer writes the u16 length in front of the blob.
        var blob = Convert.FromHexString(CapturedBlob);
        var serializer = new ObjectSerializer(false);
        var behavior = new BadgeBehavior { m_badgeData = blob };

        Assert.True(serializer.Serialize(behavior, (PropertyFlags) 7, out var buffer));
        Assert.True(((byte[]) buffer).AsSpan().EndsWith(Convert.FromHexString("1A00" + CapturedBlob)));

        Assert.True(serializer.Deserialize<BadgeBehavior>(buffer, (PropertyFlags) 7, out var decoded));
        Assert.NotNull(decoded);
        Assert.Equal(blob, (byte[]) decoded.m_badgeData);
        Assert.Equal([0x5515B013u], Decode(decoded.m_badgeData).ClaimedRewards);
    }

    private static BadgeData Decode(ByteString blob) {
        Assert.True(BadgeData.TryFromByteString(blob, out var data));

        return Assert.IsType<BadgeData>(data);
    }

}
