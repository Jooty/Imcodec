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

using Imcodec.Proxy;

namespace Imcodec.Test.ProxyTest;

public class KeyringTest {

    private const string CLIENT_DIRECTORY_VARIABLE = "IMCODEC_CLIENT_DIR";
    private const int CORRUPTED_BYTE_INDEX = 302;

    [Fact]
    public void Eject_FindsAndDecodesSyntheticBlob() {
        var synthetic = SyntheticKeyring.Shared;

        var keyring = Keyring.Eject(synthetic.Image);

        Assert.Equal(Keyring.KEY_COUNT, keyring.Records.Count);
        for (var i = 0; i < Keyring.KEY_COUNT; i++) {
            Assert.Equal(synthetic.Servers[i].ExportPublicKey(), keyring.Records[i].PublicKey);
            Assert.Equal(KeyringCipher.Lea, keyring.Records[i].Cipher);
        }
        Assert.Equal(Keyring.BLOB_FIRST_BYTE, keyring.Raw[0]);
    }

    [Fact]
    public void Eject_ImageWithoutBlob_Throws() {
        Assert.Throws<InvalidDataException>(() => Keyring.Eject(new byte[4096]));
    }

    [Fact]
    public void Eject_CorruptedCiphertext_IsNotAccepted() {
        var image = (byte[]) SyntheticKeyring.Shared.Image.Clone();
        image[CORRUPTED_BYTE_INDEX] ^= 0xFF;

        Assert.False(Keyring.TryEject(image, out _));
    }

    [Fact]
    public void EjectFromDirectory_ReadsClientImageWhenAvailable() {
        var directory = Environment.GetEnvironmentVariable(CLIENT_DIRECTORY_VARIABLE);
        if (string.IsNullOrEmpty(directory)) {
            return;
        }

        var keyring = Keyring.EjectFromDirectory(directory);

        Assert.Equal(Keyring.KEY_COUNT, keyring.Records.Count);
        for (var slot = 0; slot < Keyring.KEY_COUNT; slot++) {
            using var key = keyring.OpenSlot(slot);
            Assert.NotEmpty(key.ExportPublicKey());
        }
    }

}
