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
using System.Text;
using Imcodec.Cryptography;

namespace Imcodec.Test.CryptographyTest;

public class Fnv1aTest {

    [Theory]
    [InlineData("", 0x811C9DC5u)]
    [InlineData("a", 0xE40C292Cu)]
    [InlineData("foobar", 0xBF9CF968u)]
    public void Hash_MatchesPublishedVectors(string input, uint expected) {
        Assert.Equal(expected, Fnv1a.Hash(Encoding.ASCII.GetBytes(input)));
    }
}
