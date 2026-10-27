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

public class KeyringCipherTest {

    [Fact]
    public void Lea128_MatchesPublishedVector() {
        var cipher = new LeaBlockCipher(Convert.FromHexString("0f1e2d3c4b5a69788796a5b4c3d2e1f0"));
        var plain = Convert.FromHexString("101112131415161718191a1b1c1d1e1f");
        var expected = Convert.FromHexString("9fc84e3528c6c6185532c7a704648bfd");

        var encrypted = new byte[16];
        cipher.EncryptBlock(plain, encrypted);
        var decrypted = new byte[16];
        cipher.DecryptBlock(expected, decrypted);

        Assert.Equal(expected, encrypted);
        Assert.Equal(plain, decrypted);
    }

    [Fact]
    public void Speck64_96_MatchesPublishedVector() {
        var cipher = new SpeckBlockCipher(Convert.FromHexString("000102030809" + "0a0b10111213"));
        var plain = Convert.FromHexString("65616e73" + "20466174");
        var expected = Convert.FromHexString("6c947541" + "ec52799f");

        var encrypted = new byte[8];
        cipher.EncryptBlock(plain, encrypted);
        var decrypted = new byte[8];
        cipher.DecryptBlock(expected, decrypted);

        Assert.Equal(expected, encrypted);
        Assert.Equal(plain, decrypted);
    }

}
