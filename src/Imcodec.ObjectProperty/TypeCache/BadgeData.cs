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

using System.Diagnostics.CodeAnalysis;
using Imcodec.IO;

namespace Imcodec.ObjectProperty.TypeCache;

/// <summary>
/// The content of the <c>m_badgeData</c> blob of a <c>BadgeBehavior</c>. The client packs it in
/// <c>BaseBadgeBehavior::OnPreSave</c> and reads it in <c>BaseBadgeBehavior::OnPostLoad</c>. It is not a property
/// class and carries no class hash: the magic <see cref="Magic"/>, the <see cref="Version"/>, then three lists,
/// each a u16 count followed by its entries, all little-endian. The length in front of the whole blob is written by
/// the property serializer that stores the <c>m_badgeData</c> string.
/// </summary>
public sealed class BadgeData {

    public const uint Magic = 0x2ACEFACE;
    public const uint Version = 1;

    /// <summary>
    /// A badge the client lists as new, and when it was earned. The client keeps an entry only while its time is
    /// less than a day old when it loads the blob.
    /// </summary>
    /// <param name="BadgeId">The id of the badge, as <c>NewBadgeData.m_badgeID</c>.</param>
    /// <param name="Time">The unix time in seconds, as <c>NewBadgeData.m_badgeTime</c>.</param>
    public readonly record struct NewBadge(uint BadgeId, uint Time);

    /// <summary>
    /// The ids of the badges whose reward was claimed (<c>BaseBadgeBehavior::HasClaimedReward</c>).
    /// </summary>
    public List<uint> ClaimedRewards { get; set; } = [];

    /// <summary>
    /// The badges listed as new (<c>BaseBadgeBehavior::HasBadge</c>).
    /// </summary>
    public List<NewBadge> NewBadges { get; set; } = [];

    /// <summary>
    /// The ids of the badges with a pending overcount reward (<c>BaseBadgeBehavior::HasPendingOvercountReward</c>).
    /// </summary>
    public List<uint> PendingOvercountRewards { get; set; } = [];

    /// <summary>
    /// Writes the blob. A list of more than <see cref="ushort.MaxValue"/> entries cannot be counted by the u16 the
    /// client reads, so it throws.
    /// </summary>
    /// <param name="writer">The writer to write the blob to.</param>
    public void Encode(BitWriter writer) {
        writer.WriteUInt32(Magic);
        writer.WriteUInt32(Version);

        WriteCount(writer, ClaimedRewards.Count);
        foreach (var badgeId in ClaimedRewards) {
            writer.WriteUInt32(badgeId);
        }

        WriteCount(writer, NewBadges.Count);
        foreach (var newBadge in NewBadges) {
            writer.WriteUInt32(newBadge.BadgeId);
            writer.WriteUInt32(newBadge.Time);
        }

        WriteCount(writer, PendingOvercountRewards.Count);
        foreach (var badgeId in PendingOvercountRewards) {
            writer.WriteUInt32(badgeId);
        }
    }

    /// <summary>
    /// Reads a blob. It fails on a wrong magic, a wrong version, a blob that ends inside a list and a blob with
    /// bytes left after the last list.
    /// </summary>
    /// <param name="reader">The reader at the start of the blob.</param>
    /// <param name="data">The badge data read, or null when it fails.</param>
    /// <returns>True when the whole blob was read.</returns>
    public static bool TryDecode(BitReader reader, [NotNullWhen(true)] out BadgeData? data) {
        data = null;

        try {
            if (reader.ReadUInt32() != Magic || reader.ReadUInt32() != Version) {
                return false;
            }

            var decoded = new BadgeData();
            for (var left = reader.ReadUInt16(); left > 0; left--) {
                decoded.ClaimedRewards.Add(reader.ReadUInt32());
            }

            for (var left = reader.ReadUInt16(); left > 0; left--) {
                decoded.NewBadges.Add(new NewBadge(reader.ReadUInt32(), reader.ReadUInt32()));
            }

            for (var left = reader.ReadUInt16(); left > 0; left--) {
                decoded.PendingOvercountRewards.Add(reader.ReadUInt32());
            }

            if (reader.GetRelativeData().Length > 0) {
                return false;
            }

            data = decoded;

            return true;
        }
        catch (EndOfStreamException) {
            return false;
        }
    }

    /// <summary>
    /// Writes the blob into the string that a <c>BadgeBehavior</c> holds as <c>m_badgeData</c>.
    /// </summary>
    public ByteString ToByteString() {
        using var writer = new BitWriter();
        Encode(writer);

        return writer.GetData();
    }

    /// <summary>
    /// Reads the <c>m_badgeData</c> string of a <c>BadgeBehavior</c>. An empty string reads as empty lists, as the
    /// client loads it.
    /// </summary>
    /// <param name="blob">The <c>m_badgeData</c> string.</param>
    /// <param name="data">The badge data read, or null when it fails.</param>
    /// <returns>True when the whole blob was read.</returns>
    public static bool TryFromByteString(ByteString blob, [NotNullWhen(true)] out BadgeData? data) {
        if (blob.IsEmpty) {
            data = new BadgeData();

            return true;
        }

        using var reader = new BitReader(blob);

        return TryDecode(reader, out data);
    }

    private static void WriteCount(BitWriter writer, int count) {
        if (count > ushort.MaxValue) {
            throw new InvalidOperationException($"A badge data list holds {count} entries, at most {ushort.MaxValue} fit its u16 count.");
        }

        writer.WriteUInt16((ushort) count);
    }

}
