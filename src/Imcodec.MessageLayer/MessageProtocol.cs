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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Imcodec.MessageLayer;

/// <summary>
/// Base class for message protocols that register and dispatch message handlers.
/// </summary>
public abstract class MessageProtocol {

   /// <summary>
   /// Gets the unique service identifier for this protocol.
   /// </summary>
   public abstract byte ServiceId { get; }

   /// <summary>
   /// Gets the protocol type identifier.
   /// </summary>
   public abstract string ProtocolType { get; }

   /// <summary>
   /// Gets the human-readable description of this protocol.
   /// </summary>
   public abstract string ProtocolDescription { get; }
   
   /// <summary>
   /// Gets the version number of this protocol implementation.
   /// </summary>
   public abstract int ProtocolVersion { get; }

   internal abstract IMessage Dispatch(byte messageId);

}