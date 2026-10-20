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
using Imcodec.Types;
using System.Diagnostics.CodeAnalysis;

namespace Imcodec.ObjectProperty.TypeCache;

/// <summary>
/// Represents the serialized binary structure for KingsIsle Emotes Radial Menu layouts
/// (<c>m_radialMenuData</c>, <c>m_radialMenu2Data</c>, <c>m_radialMenu3Data</c>).
/// </summary>
public sealed class RadialMenuData {

    public const uint Magic = 0xFACEACE8;

    /// <summary>
    /// Current format version used by modern clients.
    /// </summary>
    public const uint CurrentVersion = 3;

    public static ByteString Empty { get; } = new RadialMenuData().ToByteString();

    /// <summary>
    /// Format version (supported: 1, 2, 3; modern client saves version 3).
    /// </summary>
    public uint Version { get; set; } = CurrentVersion;

    /// <summary>
    /// Radial menu page identifier or configuration option (present in Version >= 2).
    /// </summary>
    public uint PageId { get; set; } = 0;

    public List<RadialMenuSlotInfo> Slots { get; set; } = [];

    public void Encode(BitWriter writer) {
        writer.WriteUInt32(Magic);
        writer.WriteUInt32(Version);

        if (Version >= 2) {
            writer.WriteUInt32(PageId);
        }

        WriteCount(writer, Slots.Count);

        foreach (var slot in Slots) {
            writer.WriteInt16(slot.m_slot);
            writer.WriteUInt64(slot.m_emoteID);
            writer.WriteUInt32(slot.m_theEmoteTemplateID);

            if (Version >= 3) {
                writer.WriteUInt32(slot.m_menuChatPhraseChatID);
            }
        }
    }

    public static bool TryDecode(BitReader reader, [NotNullWhen(true)] out RadialMenuData? data) {
        data = null;

        try {
            var magic = reader.ReadUInt32();
            if (magic != Magic) {
                return false;
            }

            var version = reader.ReadUInt32();
            if (version == 0 || version >= 4) {
                // Client supports versions 1, 2, and 3
                return false;
            }

            var decoded = new RadialMenuData {
                Version = version,
                PageId = version >= 2 ? reader.ReadUInt32() : 0
            };

            var count = reader.ReadUInt16();
            decoded.Slots = new List<RadialMenuSlotInfo>(count);

            for (var i = 0; i < count; i++) {
                var slot = new RadialMenuSlotInfo {
                    m_slot = reader.ReadInt16(),
                    m_emoteID = reader.ReadUInt64(),
                    m_theEmoteTemplateID = reader.ReadUInt32(),
                    m_menuChatPhraseChatID = version >= 3 ? reader.ReadUInt32() : 0
                };

                decoded.Slots.Add(slot);
            }

            data = decoded;
            return true;
        } catch (Exception) {
            return false;
        }
    }

    public ByteString ToByteString() {
        using var writer = new BitWriter();
        Encode(writer);

        return writer.GetData();
    }

    public static bool TryFromByteString(ByteString blob, [NotNullWhen(true)] out RadialMenuData? data) {
        if (blob.IsEmpty) {
            data = new RadialMenuData();
            return true;
        }

        using var reader = new BitReader(blob);
        return TryDecode(reader, out data);
    }

    /// <summary>
    /// Retrieves the slot configuration for the specified slot index, if present.
    /// </summary>
    public RadialMenuSlotInfo? GetSlot(short slotIndex) {
        return Slots.Find(s => s.m_slot == slotIndex);
    }

    /// <summary>
    /// Sets an emote action on the specified slot index.
    /// </summary>
    public void SetEmoteSlot(short slotIndex, GID emoteId, uint emoteTemplateId) {
        Slots.RemoveAll(s => s.m_slot == slotIndex);
        Slots.Add(new RadialMenuSlotInfo {
            m_slot = slotIndex,
            m_emoteID = emoteId,
            m_theEmoteTemplateID = emoteTemplateId,
            m_menuChatPhraseChatID = 0
        });
    }

    /// <summary>
    /// Sets a menu chat phrase action on the specified slot index.
    /// </summary>
    public void SetMenuChatSlot(short slotIndex, uint menuChatPhraseId) {
        Slots.RemoveAll(s => s.m_slot == slotIndex);
        Slots.Add(new RadialMenuSlotInfo {
            m_slot = slotIndex,
            m_emoteID = 0,
            m_theEmoteTemplateID = 0,
            m_menuChatPhraseChatID = menuChatPhraseId
        });
    }

    /// <summary>
    /// Removes the slot configuration at the specified slot index.
    /// </summary>
    public bool RemoveSlot(short slotIndex) {
        return Slots.RemoveAll(s => s.m_slot == slotIndex) > 0;
    }

    /// <summary>
    /// Clears all slots.
    /// </summary>
    public void Clear() {
        Slots.Clear();
    }

    private static void WriteCount(BitWriter writer, int count) {
        if (count > ushort.MaxValue) {
            throw new InvalidOperationException($"A RadialMenu holds {count} entries, at most {ushort.MaxValue} fit its u16 count.");
        }

        writer.WriteUInt16((ushort) count);
    }

}
