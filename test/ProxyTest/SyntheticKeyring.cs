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
using Imcodec.Proxy;

namespace Imcodec.Test.ProxyTest;

internal sealed class SyntheticKeyring {

    private const byte LEA_CIPHER_ID = 5;
    private const byte OPERAND = 0x5A;
    private const int KEY_SIZE = 16;
    private const int BLOCK_SIZE = 16;

    private static readonly Lazy<SyntheticKeyring> SHARED = new(() => new SyntheticKeyring());

    public static SyntheticKeyring Shared => SHARED.Value;

    public RsaKeySlot[] Servers { get; }
    public byte[] Image { get; }
    public Keyring Keyring { get; }

    private SyntheticKeyring() {
        Servers = Enumerable.Range(0, Keyring.KEY_COUNT).Select(_ => RsaKeySlot.Generate()).ToArray();

        var image = new List<byte>(Enumerable.Repeat(Keyring.BLOB_FIRST_BYTE, 64));
        image.AddRange(new byte[200]);
        image.Add(Keyring.BLOB_FIRST_BYTE);
        for (var i = 0; i < Keyring.KEY_COUNT; i++) {
            image.AddRange(BuildRecord(i, Servers[i].ExportPublicKey()));
        }
        image.AddRange(new byte[100]);

        Image = image.ToArray();
        Keyring = Keyring.Eject(Image);
    }

    private static byte[] BuildRecord(int index, byte[] publicKey) {
        var key = Enumerable.Range(0, KEY_SIZE).Select(i => (byte) (index * 31 + i)).ToArray();
        var iv = Enumerable.Range(0, BLOCK_SIZE).Select(i => (byte) (index * 17 + i * 3)).ToArray();

        var plain = new byte[(publicKey.Length + BLOCK_SIZE - 1) / BLOCK_SIZE * BLOCK_SIZE];
        for (var i = 0; i < publicKey.Length; i++) {
            plain[i] = (byte) (publicKey[i] ^ OPERAND);
        }

        var cipher = new LeaBlockCipher(key);
        var ciphertext = new byte[plain.Length];
        var previous = iv;
        for (var offset = 0; offset < plain.Length; offset += BLOCK_SIZE) {
            var block = plain.AsSpan(offset, BLOCK_SIZE).ToArray();
            for (var i = 0; i < BLOCK_SIZE; i++) {
                block[i] ^= previous[i];
            }
            cipher.EncryptBlock(block, ciphertext.AsSpan(offset, BLOCK_SIZE));
            previous = ciphertext.AsSpan(offset, BLOCK_SIZE).ToArray();
        }

        var record = new List<byte>();
        var length = (ushort) (key.Length + iv.Length + ciphertext.Length + 2);
        record.AddRange(BitConverter.GetBytes(length));
        record.Add(LEA_CIPHER_ID);
        record.AddRange(key);
        record.AddRange(iv);
        record.AddRange(BitConverter.GetBytes((ushort) publicKey.Length));
        record.AddRange(ciphertext);
        return record.ToArray();
    }

}
