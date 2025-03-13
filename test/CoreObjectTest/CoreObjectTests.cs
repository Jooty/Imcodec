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

using Imcodec.ObjectProperty.TypeCache;

namespace Imcodec.CoreObject.Tests;

public class CoreObjectSerializerTest {

    private const string CoreObjectBlob
        = """
        70 00 00 00 78 DA 2B E6 3C CC CE C2 C0 C8 00 02 3B 97 F4 BB 
        27 4D 99 29 C6 72 6F 93 CF 17 7D 6F E5 C4 2B CB 97 33 E0 04 
        0D F6 20 9D A4 02 00 57 AC 0C A5
        """;

   [Fact]
	public void Serializer_Deserialize() {

      // Arrange
      var blobBytes = CoreObjectBlob.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
                     .Select(hex => Convert.ToByte(hex, 16))
                     .ToArray();
      var serializer = new CoreObjectSerializer();

      // Act
      if (!serializer.Deserialize<WizClientObjectItem>(blobBytes, 0u, out var coreObject)) {
         Assert.True(false, "Failed to serialize object.");
      }

      // Assert
      Assert.NotNull(coreObject);
      Assert.Equal(1, coreObject.m_fScale);
      Assert.NotNull(coreObject.m_inactiveBehaviors);
      Assert.Single(coreObject.m_inactiveBehaviors);

   }

}