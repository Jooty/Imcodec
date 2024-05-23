/*
BSD 3-Clause License

Copyright (c) 2024, Revive101

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

using Imcodec.ObjectProperty.PropertyClass;

namespace Imcodec.Test.PropertyClassTest;

public class PropClassTest : PropertyClass {
    public byte ByteTest { get; set; } = 127;

    public PropClassTest() {
        this["ByteTest"] = new Property<byte>(PropertyFlags.Prop_Public, false, ByteTest);
    }
}

public class CheckReflectionData {

    [Fact]
    public void CheckPropClassReflections_Byte() {
        var propClassTest = new PropClassTest();
        var bytePropReflection = propClassTest["ByteTest"] as Property<byte>;

        Assert.NotNull(bytePropReflection);
        Assert.False(bytePropReflection.NoTransfer);
        Assert.Equal(127, bytePropReflection.Value);

        // Now change the value of the property
        bytePropReflection.Value = 255;
        Assert.Equal(255, bytePropReflection.Value);

        var secondBytePropReflection = propClassTest["ByteTest"] as Property<byte>;
        Assert.Equal(255, secondBytePropReflection.Value);
    }

}
