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
/// Represents a message definition extracted from an XML document.
/// </summary>
public class MessageDefinition {

    /// <summary>
    /// Gets or sets the name of the message.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service ID for this message.
    /// </summary>
    public byte ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the message order (ID) within the service.
    /// </summary>
    public byte MessageOrder { get; set; }

    /// <summary>
    /// Gets or sets the access level required for this message.
    /// </summary>
    public byte AccessLevel { get; set; }

    /// <summary>
    /// Gets or sets the description of this message.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets the collection of fields in this message.
    /// </summary>
    public List<MessageFieldDefinition> Fields { get; } = new List<MessageFieldDefinition>();

}

/// <summary>
/// Represents a field definition within a message.
/// </summary>
public class MessageFieldDefinition {

    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized type identifier for this field.
    /// </summary>
    public string SerializedType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the C# type for this field.
    /// </summary>
    public string CSharpType { get; set; } = string.Empty;

}