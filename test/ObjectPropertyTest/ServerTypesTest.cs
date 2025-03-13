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

using Imcodec.CoreObject;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;

namespace Imcodec.Test.ObjectPropertyTest;

public sealed class ServerTypesTest {

    private static readonly string s_serverTypesObjectSerializer
        = """
        43ACDA06010000005B268C0615005472696767657254657374466F72496D636F64
        65630100000000000000000000000001000000080046697265644F6E2100000000
        01000000090046697265644F666621000000000000000000000000000000000000
        00000000
        """;
    private static readonly string s_serverTypesCoreObjectSerializer
        = """
        000043ACDA060100000000005B268C0615005472696767657254657374466F7249
        6D636F6465630100000000000000000000000001000000080046697265644F6E21
        0000000001000000090046697265644F6666210000000000000000000000000000
        00000000000000000000000000000000000000
        """;

    [Fact]
    public void TryDeserializeServerTypes() {
        var serializer = new ObjectSerializer(false, SerializerFlags.None);
        var byteBlob = ConvertFromHexString(s_serverTypesObjectSerializer);
        var deserializeSuccess = serializer.Deserialize<WizZoneTriggers>(byteBlob, (PropertyFlags) 31, out var serverTypes);

        Assert.True(deserializeSuccess);
        Assert.NotNull(serverTypes);
    }

    [Fact]
    public void TryDeserializeServerTypesCore() {
        var serializer = new CoreObjectSerializer(false, SerializerFlags.None);
        var byteBlob = ConvertFromHexString(s_serverTypesCoreObjectSerializer);
        var deserializeSuccess = serializer.Deserialize<WizZoneTriggers>(byteBlob, (PropertyFlags) 31, out var serverTypes);

        Assert.True(deserializeSuccess);
        Assert.NotNull(serverTypes);
    }

    private static byte[] ConvertFromHexString(string hexString) {
        string cleanHex = new string([.. hexString.Where(c => !char.IsWhiteSpace(c))]);

        return [.. Enumerable.Range(0, cleanHex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(cleanHex.Substring(x, 2), 16))];
    }

}