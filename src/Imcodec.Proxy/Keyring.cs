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
using Imcodec.Cryptography;

namespace Imcodec.Proxy;

/// <summary>
/// One server public key of the keyring, with the cipher and operand that obfuscated it in the client image.
/// </summary>
public sealed record KeyringRecord(
    KeyringCipher Cipher,
    byte Operand,
    byte[] DecryptionKey,
    byte[] DecryptionIv,
    byte[] PublicKey);

/// <summary>
/// The server public keys the client trusts, read out of the obfuscated blob in a client image.
/// The image is only ever read.
/// </summary>
public sealed class Keyring {

    public const int KEY_COUNT = 5;
    public const byte BLOB_FIRST_BYTE = 0x35;

    private const int RECORD_LENGTH_FIELD_SIZE = 2;
    private const int RECORD_HEADER_SIZE = 3;
    private const int MIN_RECORD_SIZE = 40;
    private const int MAX_RECORD_SIZE = 1024;
    private const int KEY_LENGTH_FIELD_SIZE = 2;
    private const byte DER_SEQUENCE_TAG = 0x30;
    private const byte DER_LONG_LENGTH_TAG = 0x82;
    private const string CLIENT_IMAGE_SEARCH_PATTERN = "*.exe";

    public byte[] Raw { get; }
    public IReadOnlyList<KeyringRecord> Records { get; }

    private Keyring(byte[] raw, IReadOnlyList<KeyringRecord> records) {
        Raw = raw;
        Records = records;
    }

    /// <summary>
    /// Finds the keyring blob in a client image by its structure and decodes its public keys.
    /// </summary>
    /// <exception cref="InvalidDataException">No blob in the image decodes to a full set of public keys.</exception>
    public static Keyring Eject(ReadOnlySpan<byte> image) {
        if (TryEject(image, out var keyring)) {
            return keyring;
        }
        throw new InvalidDataException("No keyring was found in the client image.");
    }

    public static bool TryEject(ReadOnlySpan<byte> image, out Keyring keyring) {
        var start = 0;
        while (start < image.Length) {
            var found = image[start..].IndexOf(BLOB_FIRST_BYTE);
            if (found < 0) {
                break;
            }

            var candidate = start + found;
            if (TryParseBlob(image, candidate, out keyring)) {
                return true;
            }
            start = candidate + 1;
        }

        keyring = null!;
        return false;
    }

    /// <summary>
    /// Ejects the keyring from the first executable in a directory that contains one, reading each file only.
    /// </summary>
    /// <exception cref="InvalidDataException">No executable in the directory contains a keyring.</exception>
    public static Keyring EjectFromDirectory(string directory) {
        foreach (var path in Directory.EnumerateFiles(directory, CLIENT_IMAGE_SEARCH_PATTERN)) {
            if (TryEject(File.ReadAllBytes(path), out var keyring)) {
                return keyring;
            }
        }
        throw new InvalidDataException("No executable in the directory contains a keyring.");
    }

    /// <summary>
    /// Opens the public key of a slot for verifying signatures and encrypting to the server.
    /// </summary>
    public RsaKeySlot OpenSlot(int slot) => RsaKeySlot.FromPublicKey(Records[slot].PublicKey);

    private static bool TryParseBlob(ReadOnlySpan<byte> image, int blobStart, out Keyring keyring) {
        keyring = null!;

        var position = blobStart + 1;
        var spans = new (int Start, int Size)[KEY_COUNT];
        for (var i = 0; i < KEY_COUNT; i++) {
            if (position + RECORD_HEADER_SIZE > image.Length) {
                return false;
            }

            var size = RECORD_HEADER_SIZE + BinaryPrimitives.ReadUInt16LittleEndian(image[position..]);
            var cipher = image[position + RECORD_LENGTH_FIELD_SIZE];
            if (size < MIN_RECORD_SIZE || size > MAX_RECORD_SIZE || position + size > image.Length
                || !KeyringCipherFactory.IsKnown(cipher)) {
                return false;
            }

            spans[i] = (position, size);
            position += size;
        }

        var records = new List<KeyringRecord>(KEY_COUNT);
        foreach (var (start, size) in spans) {
            if (!TryDecodeRecord(image.Slice(start, size), out var record)) {
                return false;
            }
            records.Add(record);
        }

        keyring = new Keyring(image[blobStart..position].ToArray(), records);
        return true;
    }

    private static bool TryDecodeRecord(ReadOnlySpan<byte> record, out KeyringRecord decoded) {
        decoded = null!;

        var cipher = (KeyringCipher) record[RECORD_LENGTH_FIELD_SIZE];
        var keySize = KeyringCipherFactory.KeySize(cipher);
        var ivSize = KeyringCipherFactory.IvSize(cipher);
        var keyStart = RECORD_HEADER_SIZE;
        var lengthStart = keyStart + keySize + ivSize;
        var ciphertextStart = lengthStart + KEY_LENGTH_FIELD_SIZE;
        if (ciphertextStart >= record.Length) {
            return false;
        }

        var key = record.Slice(keyStart, keySize).ToArray();
        var iv = record.Slice(keyStart + keySize, ivSize).ToArray();
        var publicKeyLength = BinaryPrimitives.ReadUInt16LittleEndian(record[lengthStart..]);

        byte[] plain;
        try {
            plain = KeyringCipherFactory.DecryptCbc(cipher, key, iv, record[ciphertextStart..]);
        }
        catch (InvalidDataException) {
            return false;
        }

        var operand = (byte) (plain[0] ^ DER_SEQUENCE_TAG);
        if ((plain[1] ^ DER_LONG_LENGTH_TAG) != operand || publicKeyLength > plain.Length) {
            return false;
        }

        var publicKey = new byte[publicKeyLength];
        for (var i = 0; i < publicKey.Length; i++) {
            publicKey[i] = (byte) (plain[i] ^ operand);
        }

        try {
            using var _ = RsaKeySlot.FromPublicKey(publicKey);
        }
        catch (System.Security.Cryptography.CryptographicException) {
            return false;
        }

        decoded = new KeyringRecord(cipher, operand, key, iv, publicKey);
        return true;
    }

}
