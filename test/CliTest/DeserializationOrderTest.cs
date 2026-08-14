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
using Imcodec.ObjectProperty.TypeCache;
using Newtonsoft.Json;

namespace Imcodec.Test.CliTest;

public sealed class DeserializationOrderTest {

    [Fact]
    public void BaseClassPropertiesAreSerializedBeforeDerivedProperties() {
        var spellTemplate = new SpellTemplate {
            m_name = "Test Spell",
            m_description = "Test Description"
        };

        var settings = new JsonSerializerSettings {
            ContractResolver = new BaseFirstContractResolver()
        };
        var json = JsonConvert.SerializeObject(spellTemplate, Formatting.Indented, settings);

        // m_behaviors is declared on CoreTemplate, the base class of SpellTemplate.
        var firstPropertyStart = json.IndexOf('{') + 1;
        var firstPropertyEnd = json.IndexOf(':', firstPropertyStart);
        var firstProperty = json[firstPropertyStart..firstPropertyEnd].Trim();

        Assert.Equal("\"m_behaviors\"", firstProperty);
        Assert.True(
            json.IndexOf("\"m_behaviors\"", StringComparison.Ordinal)
                < json.IndexOf("\"m_name\"", StringComparison.Ordinal));
    }

}
