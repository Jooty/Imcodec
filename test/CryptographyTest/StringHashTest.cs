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

using Imcodec.Cryptography;

namespace Imcodec.Test.CryptographyTest;

public class StringHashTest {

    private static readonly Dictionary<string, string> s_hashAnswerKey = new() {
        { "Hello, world!", "804074800" },
        { "Lorem ipsum dolor sit amet", "249896619" },
        { "quas percipit ne mel, ea porro libris vel.", "2144719378" },
        { "Vix rebum lorem voluptatum eu, tation salutandi corrumpit ei est. "
            + "Suas harum graeci sit te, nibh mollis ne mea. Vel ea idque posidonium, nec dolore albucius ei. "
            + "No usu diceret percipit disputando, ludus noluisse usu ea.", "1586065034" }
    };

    [Fact]
    public void TestStringHash() {
        foreach (var (question, answer) in s_hashAnswerKey) {
            var hash = StringHash.Compute(question);
            var hashString = hash.ToString();

            Assert.Equal(answer, hashString);
        }
    }

}
