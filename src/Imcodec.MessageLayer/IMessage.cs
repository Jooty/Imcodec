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

using Imcodec.IO;

namespace Imcodec.MessageLayer;

/// <summary>
/// Interface for all message types within the message layer.
/// </summary>
public interface IMessage {

    /// <summary>
    /// Gets the message order value used to identify the message within its service.
    /// </summary>
    public byte MessageOrder { get; }

    /// <summary>
    /// Gets the service ID that this message belongs to.
    /// </summary>
    public byte ServiceId { get; }

    /// <summary>
    /// Gets the access level required to process this message.
    /// </summary>
    public byte AccessLevel { get; }

    /// <summary>
    /// Encodes the message into a byte array.
    /// </summary>
    /// <param name="writer">The writer to encode the message into.</param>
    void Encode(BitWriter writer);

    /// <summary>
    /// Decodes the message from a byte array.
    /// </summary>
    /// <param name="reader">The reader to decode the message from.</param>
    void Decode(BitReader reader);

}