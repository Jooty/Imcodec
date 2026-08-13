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

using Imcodec.Cli;

namespace Imcodec.Test.CliTest;

public sealed class BlobDeserializationTest {

    private const string s_serializedObjectBlob
        = """
        43ACDA06010000005B268C0615005472696767657254657374466F72496D636F64
        65630100000000000000000000000001000000080046697265644F6E2100000000
        01000000090046697265644F666621000000000000000000000000000000000000
        00000000
        """;

    [Fact]
    public void TryDeserializeHexBlob_ReturnsDeserializedObject() {
        var json = Deserialization.TryDeserializeHexBlob(s_serializedObjectBlob);

        Assert.NotNull(json);
        Assert.Contains("\"_objectType\"", json);
        Assert.Contains("WizZoneTriggers", json);
    }

}
