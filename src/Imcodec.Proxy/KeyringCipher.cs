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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace Imcodec.Proxy;

internal interface IBlockDecryptor {

    int BlockSize { get; }

    void DecryptBlock(ReadOnlySpan<byte> input, Span<byte> output);

}

/// <summary>
/// The block ciphers the keyring records are obfuscated with, numbered as in the record's cipher byte.
/// </summary>
public enum KeyringCipher : byte {

    Speck64 = 1,
    Aria = 2,
    Camellia = 3,
    Seed = 4,
    Lea = 5,

}

internal static class KeyringCipherFactory {

    private const int SPECK_KEY_SIZE = 12;
    private const int SPECK_IV_SIZE = 8;
    private const int STANDARD_KEY_SIZE = 16;
    private const int STANDARD_IV_SIZE = 16;

    public static int KeySize(KeyringCipher cipher) => cipher == KeyringCipher.Speck64 ? SPECK_KEY_SIZE : STANDARD_KEY_SIZE;

    public static int IvSize(KeyringCipher cipher) => cipher == KeyringCipher.Speck64 ? SPECK_IV_SIZE : STANDARD_IV_SIZE;

    public static bool IsKnown(byte cipher) => Enum.IsDefined((KeyringCipher) cipher);

    public static byte[] DecryptCbc(KeyringCipher cipher, byte[] key, byte[] iv, ReadOnlySpan<byte> ciphertext) {
        var engine = Create(cipher, key);
        var blockSize = engine.BlockSize;
        if (ciphertext.Length % blockSize != 0) {
            throw new InvalidDataException("The keyring ciphertext is not a whole number of blocks.");
        }

        var plaintext = new byte[ciphertext.Length];
        var previous = iv;
        for (var offset = 0; offset < ciphertext.Length; offset += blockSize) {
            var block = ciphertext.Slice(offset, blockSize);
            var output = plaintext.AsSpan(offset, blockSize);
            engine.DecryptBlock(block, output);
            for (var i = 0; i < blockSize; i++) {
                output[i] ^= previous[i];
            }
            previous = block.ToArray();
        }
        return plaintext;
    }

    public static IBlockDecryptor Create(KeyringCipher cipher, byte[] key) => cipher switch {
        KeyringCipher.Speck64 => new SpeckBlockCipher(key),
        KeyringCipher.Lea => new LeaBlockCipher(key),
        KeyringCipher.Aria => new BouncyDecryptor(new AriaEngine(), key),
        KeyringCipher.Camellia => new BouncyDecryptor(new CamelliaEngine(), key),
        KeyringCipher.Seed => new BouncyDecryptor(new SeedEngine(), key),
        _ => throw new NotSupportedException($"Unknown keyring cipher {cipher}."),
    };

    private sealed class BouncyDecryptor : IBlockDecryptor {

        private readonly IBlockCipher _engine;

        public int BlockSize => _engine.GetBlockSize();

        public BouncyDecryptor(IBlockCipher engine, byte[] key) {
            _engine = engine;
            _engine.Init(false, new KeyParameter(key));
        }

        public void DecryptBlock(ReadOnlySpan<byte> input, Span<byte> output) => _engine.ProcessBlock(input, output);

    }

}
