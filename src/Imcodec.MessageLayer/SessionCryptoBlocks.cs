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

namespace Imcodec.MessageLayer;

/// <summary>
/// The crypto block of a session offer: which key signs it, the tamper challenge and the echo value
/// the accept has to repeat.
/// </summary>
public sealed class SessionOfferCryptoBlock {

    public const int CHALLENGE_HASH_REGION_SIZE = 4;
    public const int CHALLENGE_TYPE_OFFSET = 4;
    public const int CHALLENGE_PARAMETERS_OFFSET = 5;

    public byte Flags { get; set; }
    public byte KeySlot { get; set; }
    public byte KeyMask { get; set; }
    public byte[] Challenge { get; set; } = [];
    public uint Echo { get; set; }

    /// <summary>
    /// Offset into the keyring blob of the region the accept has to hash.
    /// </summary>
    public ushort HashRegionOffset => BitConverter.ToUInt16(Challenge, 0);

    /// <summary>
    /// Length of the keyring region the accept has to hash.
    /// </summary>
    public ushort HashRegionLength => BitConverter.ToUInt16(Challenge, 2);

    public byte ChallengeType => Challenge[CHALLENGE_TYPE_OFFSET];

    public ReadOnlySpan<byte> ChallengeParameters => Challenge.AsSpan(CHALLENGE_PARAMETERS_OFFSET);

    public void Encode(BitWriter writer) {
        writer.WriteUInt8(Flags);
        writer.WriteUInt8(KeySlot);
        writer.WriteUInt8(KeyMask);
        writer.WriteUInt8((byte) Challenge.Length);
        writer.WriteBytes(Challenge);
        writer.WriteUInt32(Echo);
    }

    public void Decode(BitReader reader) {
        Flags = reader.ReadUInt8();
        KeySlot = reader.ReadUInt8();
        KeyMask = reader.ReadUInt8();
        Challenge = reader.ReadBytes(reader.ReadUInt8());
        Echo = reader.ReadUInt32();
    }

}

/// <summary>
/// The crypto block of a session accept, before it is encrypted: the client's proofs and the secrets
/// of the session's AES streams.
/// </summary>
public sealed class SessionAcceptCryptoBlock {

    public const int SECRET_SIZE = 16;

    public byte Flags { get; set; }
    public uint KeyHash { get; set; }
    public uint ChallengeAnswer { get; set; }
    public uint Echo { get; set; }
    public uint Timestamp { get; set; }
    public byte[] Key { get; set; } = new byte[SECRET_SIZE];
    public byte[] Nonce { get; set; } = new byte[SECRET_SIZE];

    public void Encode(BitWriter writer) {
        writer.WriteUInt8(Flags);
        writer.WriteUInt32(KeyHash);
        writer.WriteUInt32(ChallengeAnswer);
        writer.WriteUInt32(Echo);
        writer.WriteUInt32(Timestamp);
        writer.WriteBytes(Key);
        writer.WriteBytes(Nonce);
    }

    public void Decode(BitReader reader) {
        Flags = reader.ReadUInt8();
        KeyHash = reader.ReadUInt32();
        ChallengeAnswer = reader.ReadUInt32();
        Echo = reader.ReadUInt32();
        Timestamp = reader.ReadUInt32();
        Key = reader.ReadBytes(SECRET_SIZE);
        Nonce = reader.ReadBytes(SECRET_SIZE);
    }

}

/// <summary>
/// Splits and joins the crypto payloads that session offers and accepts carry.
/// </summary>
public static class SessionCryptoBlocks {

    public const int SIGNATURE_SIZE = 256;
    public const int ACCEPT_PRESENCE_FLAG_SIZE = 1;
    public static readonly byte[] NO_CRYPTO_PAYLOAD = [0];

    /// <summary>
    /// Whether the payload carries a crypto block at all.
    /// </summary>
    public static bool HasCrypto(byte[] payload) => payload.Length > NO_CRYPTO_PAYLOAD.Length;

    /// <summary>
    /// Splits an offer's payload into the signed block bytes and the signature.
    /// </summary>
    /// <exception cref="InvalidDataException">The payload is too short to hold a signature.</exception>
    public static (byte[] Signed, byte[] Signature) SplitOffer(byte[] payload) {
        if (payload.Length <= SIGNATURE_SIZE) {
            throw new InvalidDataException("The session offer payload is too short to hold a signature.");
        }
        return (payload[..^SIGNATURE_SIZE], payload[^SIGNATURE_SIZE..]);
    }

    public static byte[] JoinOffer(byte[] signed, byte[] signature) => [.. signed, .. signature];

    /// <summary>
    /// Returns the RSA-encrypted bytes of an accept's payload, without the presence flag.
    /// </summary>
    public static byte[] EncryptedPart(byte[] payload) => payload[ACCEPT_PRESENCE_FLAG_SIZE..];

    public static byte[] JoinAccept(byte presenceFlag, byte[] encrypted) => [presenceFlag, .. encrypted];

    public static SessionOfferCryptoBlock DecodeOfferBlock(byte[] signed) {
        var block = new SessionOfferCryptoBlock();
        block.Decode(new BitReader(signed));
        return block;
    }

    public static byte[] EncodeOfferBlock(SessionOfferCryptoBlock block) {
        var writer = new BitWriter();
        block.Encode(writer);
        return writer.GetData();
    }

    public static SessionAcceptCryptoBlock DecodeAcceptBlock(byte[] plain) {
        var block = new SessionAcceptCryptoBlock();
        block.Decode(new BitReader(plain));
        return block;
    }

    public static byte[] EncodeAcceptBlock(SessionAcceptCryptoBlock block) {
        var writer = new BitWriter();
        block.Encode(writer);
        return writer.GetData();
    }

}
