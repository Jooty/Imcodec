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

namespace Imcodec.Cryptography;

public sealed class RsaKeySlot : IDisposable {

    private readonly RSA _rsa;

    public RsaKeySlot(RSA rsa) {
        _rsa = rsa;
    }

    /// <summary>
    /// Creates a slot from a DER-encoded SubjectPublicKeyInfo public key.
    /// </summary>
    public static RsaKeySlot FromPublicKey(ReadOnlySpan<byte> subjectPublicKeyInfo) {
        var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(subjectPublicKeyInfo, out _);
        return new RsaKeySlot(rsa);
    }

    /// <summary>
    /// Creates a slot from a DER-encoded PKCS#8 private key.
    /// </summary>
    public static RsaKeySlot FromPrivateKey(ReadOnlySpan<byte> pkcs8PrivateKey) {
        var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(pkcs8PrivateKey, out _);
        return new RsaKeySlot(rsa);
    }

    /// <summary>
    /// Generates a new key pair.
    /// </summary>
    public static RsaKeySlot Generate(int keySizeBits = 2048) => new(RSA.Create(keySizeBits));

    public byte[] ExportPublicKey() => _rsa.ExportSubjectPublicKeyInfo();

    public byte[] ExportPrivateKey() => _rsa.ExportPkcs8PrivateKey();

    /// <summary>
    /// Encrypts with the public key using OAEP and SHA-1.
    /// </summary>
    public byte[] Encrypt(ReadOnlySpan<byte> data) => _rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA1);

    /// <summary>
    /// Decrypts with the private key using OAEP and SHA-1.
    /// </summary>
    public byte[] Decrypt(ReadOnlySpan<byte> data) => _rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA1);

    /// <summary>
    /// Signs with the private key using PKCS#1 v1.5 and SHA-1.
    /// </summary>
    public byte[] Sign(ReadOnlySpan<byte> data) =>
        _rsa.SignData(data, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

    /// <summary>
    /// Verifies a PKCS#1 v1.5 and SHA-1 signature with the public key.
    /// </summary>
    public bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature) =>
        _rsa.VerifyData(data, signature, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

    public void Dispose() => _rsa.Dispose();
}
