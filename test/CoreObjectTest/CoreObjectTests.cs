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
using Imcodec.ObjectProperty.TypeCache;

namespace Imcodec.Test.CoreObjectTest;

public class CoreObjectSerializerTest {

    private const string CoreObjectBlob
        = """
        6A 00 00 00 78 DA 2B E6 AC EB 9F 94 C4 40 12 68 B0 67 20 03 00 00 29 62 03 3D
        """;

    [Fact]
    public void SerializerSerialize() {
        // Arrange
        var serializer = new CoreObjectSerializer();
        var coreObject = new WizClientObjectItem {
            m_fScale = 1,
        };

        // Act
        if (!serializer.Serialize(coreObject, 1, out var bytes)) {
            Assert.True(false, "Failed to serialize object.");
        }

        // Assert
        Assert.Equal(CoreObjectBlob, string.Join(" ", ((byte[]) bytes).Select(static b => b.ToString("X2"))));
    }

}