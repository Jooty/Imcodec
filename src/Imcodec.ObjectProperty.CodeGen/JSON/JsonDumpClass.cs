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

using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Imcodec.ObjectProperty.CodeGen.JSON {
    [DebuggerDisplay("{Name}")]
    public class JsonDumpClass {

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("bases")]
        public IList<string>? BaseClasses { get; set; }

        [JsonPropertyName("hash")]
        public uint Hash { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, JsonDumpProperty> Properties { get; set; }
            = new Dictionary<string, JsonDumpProperty>();

    }
}
