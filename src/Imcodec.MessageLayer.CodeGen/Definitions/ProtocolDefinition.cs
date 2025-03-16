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

namespace Imcodec.MessageLayer.CodeGen.Definitions;

/// <summary>
/// Represents a protocol definition extracted from an XML document.
/// </summary>
public class ProtocolDefinition {

    /// <summary>
    /// Gets or sets the service ID for this protocol.
    /// </summary>
    public byte ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the protocol type identifier.
    /// </summary>
    public string ProtocolType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable description of this protocol.
    /// </summary>
    public string ProtocolDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of this protocol.
    /// </summary>
    public int ProtocolVersion { get; set; }

    /// <summary>
    /// Gets or sets the file name this protocol was loaded from.
    /// </summary>
    public string SourceFileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the collection of messages in this protocol.
    /// </summary>
    public List<MessageDefinition> Messages { get; } = new List<MessageDefinition>();

    /// <summary>
    /// Gets the name that will be used for the generated protocol class.
    /// </summary>
    public string ClassName => $"{ProtocolType.ToUpper()}_{ServiceId}_PROTOCOL";

}