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

using System.Buffers.Binary;
using System.Text.Json;
using Imcodec.Cryptography;
using Imcodec.MessageLayer;
using Imcodec.Proxy;
using static Imcodec.MessageLayer.ControlMessageProtocol;

namespace Imcodec.Test.ProxyTest;

public class SessionCryptoTest {

    private const string CAPTURE_FILE_VARIABLE = "IMCODEC_CAPTURE_FILE";
    private const string SYNTHETIC_DUMP =
        "0a00000000010203040506070809060000003233343536372500000005121f2c394653606d7a8794a1aebbc8d5e2effc091623303d4a5764717e8b98a5b2bfccd9";
    private const uint SYNTHETIC_CHALLENGE_ANSWER = 2657274799;
    private const byte KEY_SLOT = 3;
    private const ushort HASH_REGION_OFFSET = 392;
    private const ushort HASH_REGION_LENGTH = 472;
    private const uint ECHO = 0xA1B2C3D4;

    internal static SessionOffer MakeSignedOffer(uint echo = ECHO, byte slot = KEY_SLOT, ushort regionLength = HASH_REGION_LENGTH) {
        var challenge = new byte[17];
        BinaryPrimitives.WriteUInt16LittleEndian(challenge, HASH_REGION_OFFSET);
        BinaryPrimitives.WriteUInt16LittleEndian(challenge.AsSpan(2), regionLength);
        challenge[SessionOfferCryptoBlock.CHALLENGE_TYPE_OFFSET] = ClientSignature.CHALLENGE_TYPE;
        BinaryPrimitives.WriteUInt32LittleEndian(challenge.AsSpan(5), 0x200);
        BinaryPrimitives.WriteUInt32LittleEndian(challenge.AsSpan(9), 0);
        BinaryPrimitives.WriteUInt32LittleEndian(challenge.AsSpan(13), 0x12345678);

        var signed = SessionCryptoBlocks.EncodeOfferBlock(new SessionOfferCryptoBlock {
            Flags = 2, KeySlot = slot, KeyMask = 0xA7, Challenge = challenge, Echo = echo,
        });
        var server = SyntheticKeyring.Shared.Servers[slot];
        return new SessionOffer {
            SessionId = 3944,
            TimestampUpper = 1,
            TimestampLower = 2,
            Milliseconds = 883,
            CryptoPayload = SessionCryptoBlocks.JoinOffer(signed, server.Sign(signed)),
        };
    }

    private static T RoundTrip<T>(T message) where T : class, IMessage {
        var decoded = MessageEncoder.Decode(MessageEncoder.Encode(message));
        return Assert.IsType<T>(Assert.Single(decoded!));
    }

    [Fact]
    public void Handshake_DerivesMatchingAesStreams() {
        var synthetic = SyntheticKeyring.Shared;
        var signature = new ClientSignature(Convert.FromHexString(SYNTHETIC_DUMP));
        var offer = RoundTrip(MakeSignedOffer());

        using var client = new SessionCrypto(synthetic.Keyring, signature);
        client.VerifyOffer(offer);
        var accept = RoundTrip(client.BuildAccept(offer));

        using var server = new SessionCrypto(synthetic.Keyring, signature);
        server.VerifyOffer(offer);
        server.ProcessAccept(accept, synthetic.Servers[KEY_SLOT]);

        Assert.Equal(SYNTHETIC_CHALLENGE_ANSWER, client.ChallengeAnswer);
        Assert.Equal(offer.SessionId, accept.SessionId);
        var payload = Enumerable.Range(0, 10_000).Select(i => (byte) i).ToArray();
        Assert.Equal(payload, server.ClientCipher!.Decrypt(client.ClientCipher!.Encrypt(payload)));
        Assert.Equal(payload, client.ServerCipher!.Decrypt(server.ServerCipher!.Encrypt(payload)));
    }

    [Fact]
    public void BuildAccept_CarriesEchoKeyHashAndChallengeAnswer() {
        var synthetic = SyntheticKeyring.Shared;
        var signature = new ClientSignature(Convert.FromHexString(SYNTHETIC_DUMP));
        var offer = MakeSignedOffer();
        var key = Enumerable.Range(0, 16).Select(i => (byte) i).ToArray();
        var nonce = Enumerable.Range(16, 16).Select(i => (byte) i).ToArray();

        using var client = new SessionCrypto(synthetic.Keyring, signature);
        client.VerifyOffer(offer);
        var accept = client.BuildAccept(offer, key, nonce);

        var plain = synthetic.Servers[KEY_SLOT].Decrypt(SessionCryptoBlocks.EncryptedPart(accept.CryptoPayload));
        var block = SessionCryptoBlocks.DecodeAcceptBlock(plain);
        Assert.Equal(ECHO, block.Echo);
        Assert.Equal(SYNTHETIC_CHALLENGE_ANSWER, block.ChallengeAnswer);
        Assert.Equal(Fnv1a.Hash(synthetic.Keyring.Raw.AsSpan(HASH_REGION_OFFSET, HASH_REGION_LENGTH)), block.KeyHash);
        Assert.Equal(key, block.Key);
        Assert.Equal(nonce, block.Nonce);
    }

    [Fact]
    public void VerifyOffer_TamperedBlock_Throws() {
        var offer = MakeSignedOffer();
        offer.CryptoPayload[5] ^= 1;

        using var client = new SessionCrypto(SyntheticKeyring.Shared.Keyring);

        Assert.Throws<InvalidDataException>(() => client.VerifyOffer(offer));
    }

    [Fact]
    public void ProcessAccept_EchoFromAnotherOffer_Throws() {
        var synthetic = SyntheticKeyring.Shared;
        var offer = MakeSignedOffer();
        var otherOffer = MakeSignedOffer(echo: ECHO + 1);

        using var other = new SessionCrypto(synthetic.Keyring);
        other.VerifyOffer(otherOffer);
        var accept = other.BuildAccept(otherOffer);

        using var server = new SessionCrypto(synthetic.Keyring);
        server.VerifyOffer(offer);

        Assert.Throws<InvalidDataException>(() => server.ProcessAccept(accept, synthetic.Servers[KEY_SLOT]));
    }

    [Fact]
    public void ProcessAccept_WrongChallengeAnswer_Throws() {
        var synthetic = SyntheticKeyring.Shared;
        var offer = MakeSignedOffer();

        using var client = new SessionCrypto(synthetic.Keyring);
        client.VerifyOffer(offer);
        var accept = client.BuildAccept(offer);

        using var server = new SessionCrypto(synthetic.Keyring, new ClientSignature(Convert.FromHexString(SYNTHETIC_DUMP)));
        server.VerifyOffer(offer);

        Assert.Throws<InvalidDataException>(() => server.ProcessAccept(accept, synthetic.Servers[KEY_SLOT]));
    }

    [Fact]
    public void VerifyOffer_HashRegionOutsideKeyring_Throws() {
        var offer = MakeSignedOffer(regionLength: 0xFFFF);

        using var client = new SessionCrypto(SyntheticKeyring.Shared.Keyring);

        Assert.Throws<InvalidDataException>(() => client.VerifyOffer(offer));
    }

    [Fact]
    public void ResignOffer_VerifiesAgainstTheNewKeyOnly() {
        var synthetic = SyntheticKeyring.Shared;
        using var injected = RsaKeySlot.Generate();
        var resigned = SessionCrypto.ResignOffer(MakeSignedOffer(), injected);

        var (signed, signature) = SessionCryptoBlocks.SplitOffer(resigned.CryptoPayload);

        Assert.True(injected.Verify(signed, signature));
        Assert.False(synthetic.Servers[KEY_SLOT].Verify(signed, signature));
    }

    [Fact]
    public void Offer_WithoutCryptoBlock_StaysPlaintext() {
        var offer = RoundTrip(new SessionOffer { SessionId = 7 });

        using var client = new SessionCrypto(SyntheticKeyring.Shared.Keyring);
        client.VerifyOffer(offer);
        var accept = RoundTrip(client.BuildAccept(offer));

        Assert.False(SessionCryptoBlocks.HasCrypto(accept.CryptoPayload));
        Assert.False(client.IsEncrypted);
    }

    [Fact]
    public void CapturedHandshake_DecodesIntoCryptoBlocks() {
        var path = Environment.GetEnvironmentVariable(CAPTURE_FILE_VARIABLE);
        if (string.IsNullOrEmpty(path)) {
            return;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var checkedOffers = 0;
        var checkedAccepts = 0;
        foreach (var entry in document.RootElement.EnumerateArray()) {
            var data = entry.GetProperty("data");
            var name = data.GetProperty("name").GetString();
            if (name is not ("SessionOfferMessage" or "SessionAcceptMessage")) {
                continue;
            }

            var fields = data.GetProperty("fields");
            var payload = Convert.FromHexString(fields.GetProperty("signed_msg").GetProperty("value").GetString()!.Replace(" ", ""));
            var sessionId = (ushort) fields.GetProperty("session_id").GetProperty("value").GetInt32();
            if (name == "SessionOfferMessage") {
                var offer = RoundTrip(new SessionOffer { SessionId = sessionId, CryptoPayload = payload });
                var (signed, signature) = SessionCryptoBlocks.SplitOffer(offer.CryptoPayload);
                var block = SessionCryptoBlocks.DecodeOfferBlock(signed);

                Assert.Equal(SessionCryptoBlocks.SIGNATURE_SIZE, signature.Length);
                Assert.Equal(ClientSignature.CHALLENGE_TYPE, block.ChallengeType);
                Assert.Equal(ClientSignature.CHALLENGE_PARAMETERS_SIZE, block.ChallengeParameters.Length);
                Assert.True(block.HashRegionOffset + block.HashRegionLength <= 1706);
                Assert.Equal(payload, offer.CryptoPayload);
                checkedOffers++;
            }
            else {
                var accept = RoundTrip(new SessionAccept { SessionId = sessionId, CryptoPayload = payload });

                Assert.True(SessionCryptoBlocks.HasCrypto(accept.CryptoPayload));
                Assert.Equal(SessionCryptoBlocks.SIGNATURE_SIZE, SessionCryptoBlocks.EncryptedPart(accept.CryptoPayload).Length);
                checkedAccepts++;
            }
        }

        Assert.True(checkedOffers > 0);
        Assert.True(checkedAccepts > 0);
    }

}
