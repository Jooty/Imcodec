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

using System.Security.Cryptography;
using Imcodec.Cryptography;
using Imcodec.MessageLayer;
using static Imcodec.MessageLayer.ControlMessageProtocol;

namespace Imcodec.Proxy;

/// <summary>
/// The crypto side of one session's handshake: checking an offer against the keyring, answering it with an
/// accept, and deriving the AES streams from the secrets the accept carries.
/// </summary>
public sealed class SessionCrypto : IDisposable {

    private const byte ACCEPT_PRESENCE_FLAG = 1;
    private const byte ACCEPT_BLOCK_FLAGS = 1;

    private readonly Keyring _keyring;
    private readonly ClientSignature? _clientSignature;

    public SessionOfferCryptoBlock? Offer { get; private set; }

    /// <summary>
    /// The answer to the offer's tamper challenge, when a client signature was available to compute it.
    /// </summary>
    public uint? ChallengeAnswer { get; private set; }

    public AesStreamCipher? ClientCipher { get; private set; }
    public AesStreamCipher? ServerCipher { get; private set; }

    public bool IsEncrypted => ClientCipher is not null;

    public SessionCrypto(Keyring keyring, ClientSignature? clientSignature = null) {
        _keyring = keyring;
        _clientSignature = clientSignature;
    }

    /// <summary>
    /// Checks an offer's signature against the keyring and remembers what the accept has to repeat.
    /// An offer without a crypto block leaves the session unencrypted.
    /// </summary>
    /// <exception cref="InvalidDataException">The offer is malformed or its signature does not verify.</exception>
    public void VerifyOffer(SessionOffer offer) {
        Offer = null;
        ChallengeAnswer = null;
        if (!SessionCryptoBlocks.HasCrypto(offer.CryptoPayload)) {
            return;
        }

        var (signed, signature) = SessionCryptoBlocks.SplitOffer(offer.CryptoPayload);
        var block = SessionCryptoBlocks.DecodeOfferBlock(signed);
        if (block.KeySlot >= _keyring.Records.Count) {
            throw new InvalidDataException("The session offer names a key slot the keyring does not have.");
        }

        if (block.Challenge.Length < SessionOfferCryptoBlock.CHALLENGE_PARAMETERS_OFFSET
            || block.HashRegionOffset + block.HashRegionLength > _keyring.Raw.Length) {
            throw new InvalidDataException("The session offer challenge does not fit the keyring.");
        }

        using var slot = _keyring.OpenSlot(block.KeySlot);
        if (!slot.Verify(signed, signature)) {
            throw new InvalidDataException("The session offer signature does not verify.");
        }

        Offer = block;
        if (_clientSignature is not null && block.ChallengeType == ClientSignature.CHALLENGE_TYPE) {
            ChallengeAnswer = _clientSignature.AnswerChallenge(block.ChallengeParameters);
        }
    }

    /// <summary>
    /// Answers a verified offer: picks the AES key and nonce, encrypts the proofs to the offer's key and
    /// starts the session's AES streams.
    /// </summary>
    /// <exception cref="InvalidOperationException">No offer with a crypto block was verified.</exception>
    public SessionAccept BuildAccept(SessionOffer offer, byte[]? key = null, byte[]? nonce = null) {
        var accept = new SessionAccept { SessionId = offer.SessionId };
        if (Offer is null) {
            return accept;
        }

        var block = new SessionAcceptCryptoBlock {
            Flags = ACCEPT_BLOCK_FLAGS,
            KeyHash = HashKeyRegion(Offer),
            ChallengeAnswer = ChallengeAnswer ?? 0,
            Echo = Offer.Echo,
            Timestamp = (uint) DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Key = key ?? RandomNumberGenerator.GetBytes(SessionAcceptCryptoBlock.SECRET_SIZE),
            Nonce = nonce ?? RandomNumberGenerator.GetBytes(SessionAcceptCryptoBlock.SECRET_SIZE),
        };

        using var slot = _keyring.OpenSlot(Offer.KeySlot);
        var encrypted = slot.Encrypt(SessionCryptoBlocks.EncodeAcceptBlock(block));
        accept.CryptoPayload = SessionCryptoBlocks.JoinAccept(ACCEPT_PRESENCE_FLAG, encrypted);

        StartStreams(block);
        return accept;
    }

    /// <summary>
    /// Reads an accept with the private key of the offer's slot, checks its proofs against the verified
    /// offer and starts the session's AES streams.
    /// </summary>
    /// <exception cref="InvalidDataException">The echo, key hash or challenge answer does not match the offer.</exception>
    public void ProcessAccept(SessionAccept accept, RsaKeySlot privateKey) {
        if (Offer is null || !SessionCryptoBlocks.HasCrypto(accept.CryptoPayload)) {
            ClientCipher = null;
            ServerCipher = null;
            return;
        }

        var plain = privateKey.Decrypt(SessionCryptoBlocks.EncryptedPart(accept.CryptoPayload));
        var block = SessionCryptoBlocks.DecodeAcceptBlock(plain);

        if (block.Echo != Offer.Echo) {
            throw new InvalidDataException("The session accept echo does not match the offer.");
        }
        if (block.KeyHash != HashKeyRegion(Offer)) {
            throw new InvalidDataException("The session accept key hash does not match the keyring.");
        }
        if (ChallengeAnswer is { } expected && expected != block.ChallengeAnswer) {
            throw new InvalidDataException("The session accept challenge answer does not match the client signature.");
        }

        StartStreams(block);
    }

    /// <summary>
    /// Replaces an offer's signature with one made by another key of the same slot.
    /// </summary>
    public static SessionOffer ResignOffer(SessionOffer offer, RsaKeySlot signer) {
        var (signed, _) = SessionCryptoBlocks.SplitOffer(offer.CryptoPayload);
        return new SessionOffer {
            SessionId = offer.SessionId,
            TimestampUpper = offer.TimestampUpper,
            TimestampLower = offer.TimestampLower,
            Milliseconds = offer.Milliseconds,
            CryptoPayload = SessionCryptoBlocks.JoinOffer(signed, signer.Sign(signed)),
        };
    }

    public void Dispose() {
        ClientCipher?.Dispose();
        ServerCipher?.Dispose();
    }

    private uint HashKeyRegion(SessionOfferCryptoBlock offer) {
        var region = _keyring.Raw.AsSpan(offer.HashRegionOffset, offer.HashRegionLength);
        return Fnv1a.Hash(region);
    }

    private void StartStreams(SessionAcceptCryptoBlock block) {
        ClientCipher?.Dispose();
        ServerCipher?.Dispose();
        ClientCipher = AesStreamCipher.Client(block.Key, block.Nonce);
        ServerCipher = AesStreamCipher.Server(block.Key, block.Nonce);
    }

}
