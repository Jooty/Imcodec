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

namespace Imcodec.ObjectProperty;

/// <summary>
/// Defines class capable of undergoing binary serialization.
/// </summary>
public abstract record PropertyClass {

    public abstract uint GetHash();

    /// <summary>
    /// Called before encoding the object to the binary stream.
    /// </summary>
    public virtual void OnPreEncode() { }

    /// <summary>
    /// Called after encoding the object to the binary stream.
    /// </summary>
    public virtual void OnPostEncode() { }

    /// <summary>
    /// Called before decoding the object from the binary stream.
    /// </summary>
    public virtual void OnPreDecode() { }

    /// <summary>
    /// Called after decoding the object from the binary stream.
    /// </summary>
    public virtual void OnPostDecode() { }

    /// <summary>
    /// Encodes the object properties using the specified <see cref="BitWriter"/>
    /// and <see cref="ObjectSerializer"/>.
    /// </summary>
    /// <param name="writer">The <see cref="BitWriter"/> used to write the
    /// encoded data.</param>
    /// <param name="serializer">The <see cref="ObjectSerializer"/> used to
    /// serialize the object properties.</param>
    /// <returns><c>true</c> if the encoding is successful;
    /// otherwise, <c>false</c>.</returns>
    public abstract bool Encode(BitWriter writer, ObjectSerializer serializer);

    /// <summary>
    /// Decodes the object properties using the specified <see cref="BitReader"/>
    /// and <see cref="ObjectSerializer"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BitReader"/> used for decoding.</param>
    /// <param name="serializer">The <see cref="ObjectSerializer"/> used for decoding.</param>
    /// <returns><c>true</c> if the decoding is successful for all properties;
    /// otherwise, <c>false</c>.</returns>
    public abstract bool Decode(BitReader reader, ObjectSerializer serializer);

}
