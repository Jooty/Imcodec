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

namespace Imcodec.Test.CryptographyTest;

public class AesStreamCipherTest {

    private const string REFERENCE_FIRST_CHUNK_SHA256 = "1e12b7b5b6e26975d963cc25e7e3ef4e43fa0602f4440aa8642af649b5b6c056";
    private const string REFERENCE_FIRST_CHUNK_TAG = "f29d56b28aa7e393592aa7e025fb918e";
    private const string REFERENCE_SECOND_CHUNK_SHA256 = "330adba01d5a2c47614d748e6bcdf324801e61bbe8d67a33d18e65947a4c3c60";
    private const string REFERENCE_SHORT_CIPHERTEXT = "22f114f4a8e6bf635f17ad4767cb6ca864b07369eafa897639e75ebc721ace3877bc5f9ea57adc23";
    private const int PLAINTEXT_LENGTH = 5000;
    private const int CLIENT_CHUNK_BYTES = AesStreamCipher.CLIENT_CHUNK_BLOCKS * AesStreamCipher.BLOCK_SIZE;
    private const int SERVER_CHUNK_BYTES = AesStreamCipher.SERVER_CHUNK_BLOCKS * AesStreamCipher.BLOCK_SIZE;

    private static readonly byte[] s_key = Enumerable.Range(0, 16).Select(i => (byte) i).ToArray();
    private static readonly byte[] s_nonce = Enumerable.Range(100, 16).Select(i => (byte) i).ToArray();
    private static readonly byte[] s_rotationNonce = Enumerable.Range(200, 16).Select(i => (byte) i).ToArray();

    [Fact]
    public void Encrypt_ShortInput_MatchesReferenceGcm() {
        using var cipher = AesStreamCipher.Create(s_key, s_nonce, AesStreamCipher.CLIENT_CHUNK_BLOCKS);

        var encrypted = cipher.Encrypt(Plaintext(40));

        Assert.Equal(REFERENCE_SHORT_CIPHERTEXT, Convert.ToHexStringLower(encrypted));
    }

    [Fact]
    public void Encrypt_ThroughRotation_MatchesReferenceGcm() {
        using var cipher = AesStreamCipher.Create(s_key, s_nonce, AesStreamCipher.CLIENT_CHUNK_BLOCKS, () => s_rotationNonce);

        var encrypted = cipher.Encrypt(Plaintext(PLAINTEXT_LENGTH));

        var trailerStart = CLIENT_CHUNK_BYTES;
        var secondChunkStart = trailerStart + AesStreamCipher.TAG_SIZE + AesStreamCipher.NONCE_SIZE;
        Assert.Equal(secondChunkStart + (PLAINTEXT_LENGTH - CLIENT_CHUNK_BYTES), encrypted.Length);
        Assert.Equal(REFERENCE_FIRST_CHUNK_SHA256, Sha256Hex(encrypted[..trailerStart]));
        Assert.Equal(REFERENCE_FIRST_CHUNK_TAG, Convert.ToHexStringLower(encrypted[trailerStart..(trailerStart + AesStreamCipher.TAG_SIZE)]));
        Assert.Equal(s_rotationNonce, encrypted[(trailerStart + AesStreamCipher.TAG_SIZE)..secondChunkStart]);
        Assert.Equal(REFERENCE_SECOND_CHUNK_SHA256, Sha256Hex(encrypted[secondChunkStart..]));
    }

    [Fact]
    public void Encrypt_InPieces_EqualsEncryptingAtOnce() {
        using var whole = AesStreamCipher.Create(s_key, s_nonce, AesStreamCipher.CLIENT_CHUNK_BLOCKS, () => s_rotationNonce);
        using var pieces = AesStreamCipher.Create(s_key, s_nonce, AesStreamCipher.CLIENT_CHUNK_BLOCKS, () => s_rotationNonce);
        var plaintext = Plaintext(PLAINTEXT_LENGTH);

        var expected = whole.Encrypt(plaintext);
        var actual = new List<byte>();
        foreach (var length in new[] { 1, 15, 17, 4000, 63, 904 }) {
            actual.AddRange(pieces.Encrypt(plaintext.AsSpan(actual.Count - TrailerBytes(actual.Count), length)));
        }

        Assert.Equal(expected, actual.ToArray());
    }

    [Fact]
    public void Decrypt_RoundTripsAcrossSeveralRotations() {
        var plaintext = Plaintext(CLIENT_CHUNK_BYTES * 3 + 123);
        using var encryptor = AesStreamCipher.Client(s_key, s_nonce);
        using var decryptor = AesStreamCipher.Client(s_key, s_nonce);

        var decrypted = decryptor.Decrypt(encryptor.Encrypt(plaintext));

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Decrypt_ServerChunkRoundTripsAcrossRotation() {
        var plaintext = Plaintext(SERVER_CHUNK_BYTES + 77);
        using var encryptor = AesStreamCipher.Server(s_key, s_nonce);
        using var decryptor = AesStreamCipher.Server(s_key, s_nonce);

        Assert.Equal(plaintext, decryptor.Decrypt(encryptor.Encrypt(plaintext)));
    }

    [Fact]
    public void Decrypt_SeparateFrames_KeepsStreamState() {
        var plaintext = Plaintext(CLIENT_CHUNK_BYTES + 500);
        using var encryptor = AesStreamCipher.Client(s_key, s_nonce);
        using var decryptor = AesStreamCipher.Client(s_key, s_nonce);
        var firstFrame = encryptor.Encrypt(plaintext.AsSpan(0, 3000));
        var secondFrame = encryptor.Encrypt(plaintext.AsSpan(3000));

        var decrypted = decryptor.Decrypt(firstFrame).Concat(decryptor.Decrypt(secondFrame)).ToArray();

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Decrypt_TamperedChunk_FailsTagCheck() {
        using var encryptor = AesStreamCipher.Client(s_key, s_nonce);
        using var decryptor = AesStreamCipher.Client(s_key, s_nonce);
        var encrypted = encryptor.Encrypt(Plaintext(CLIENT_CHUNK_BYTES + 10));
        encrypted[5] ^= 1;

        Assert.Throws<AuthenticationTagMismatchException>(() => decryptor.Decrypt(encrypted));
    }

    [Fact]
    public void Decrypt_MissingTrailer_Throws() {
        using var encryptor = AesStreamCipher.Client(s_key, s_nonce);
        using var decryptor = AesStreamCipher.Client(s_key, s_nonce);
        var encrypted = encryptor.Encrypt(Plaintext(CLIENT_CHUNK_BYTES));

        Assert.Throws<InvalidDataException>(() => decryptor.Decrypt(encrypted.AsSpan(0, CLIENT_CHUNK_BYTES)));
    }

    private static int TrailerBytes(int producedBytes) =>
        producedBytes > CLIENT_CHUNK_BYTES ? AesStreamCipher.TAG_SIZE + AesStreamCipher.NONCE_SIZE : 0;

    private static byte[] Plaintext(int length) =>
        Enumerable.Range(0, length).Select(i => (byte) (i * 7 % 251)).ToArray();

    private static string Sha256Hex(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
