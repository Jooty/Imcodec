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

public class ClientSignatureTest {

    private const string SYNTHETIC_DUMP =
        "0a00000000010203040506070809060000003233343536372500000005121f2c394653606d7a8794a1aebbc8d5e2effc091623303d4a5764717e8b98a5b2bfccd9";

    public static TheoryData<uint, uint, uint, uint> ReferenceAnswers => new() {
        { 0x200, 0x0, 0x12345678, 2657274799 },
        { 0x1200, 0x1, 0xDEADBEEF, 989959654 },
        { 0x0, 0x2, 0x8, 1360047892 },
        { 0x20200, 0xABCDEFD7, 0x4A0A8, 111838057 },
        { 0x1000, 0x11, 0xFFFFFFFF, 4052619027 },
        { 0x20, 0x0, 0x0, 0 },
    };

    [Theory]
    [MemberData(nameof(ReferenceAnswers))]
    public void AnswerChallenge_MatchesReferenceImplementation(uint control, uint spec, uint key, uint expected) {
        var signature = new ClientSignature(Convert.FromHexString(SYNTHETIC_DUMP));

        Assert.Equal(expected, signature.AnswerChallenge(control, spec, key));
    }

    [Fact]
    public void AnswerChallenge_FromParameterBytes_ReadsLittleEndianFields() {
        var signature = new ClientSignature(Convert.FromHexString(SYNTHETIC_DUMP));
        var parameters = new byte[] { 0x00, 0x02, 0, 0, 0, 0, 0, 0, 0x78, 0x56, 0x34, 0x12, 0xFF };

        Assert.Equal(2657274799u, signature.AnswerChallenge(parameters));
    }

    [Fact]
    public void Constructor_TruncatedSection_Throws() {
        var dump = Convert.FromHexString(SYNTHETIC_DUMP);

        Assert.Throws<InvalidDataException>(() => new ClientSignature(dump.AsSpan(0, dump.Length - 1)));
    }
}
