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

namespace Imcodec.ObjectProperty.Attributes;

/// <summary>
/// Marks a property for automatic serialization by the property system.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class PropertyFieldAttribute : Attribute {

    /// <summary>
    /// The hash value of the property.
    /// </summary>
    public uint Hash { get; }

    /// <summary>
    /// The flags that define the property's behavior.
    /// </summary>
    public uint Flags { get; }

    /// <summary>
    /// Creates a new AutoProperty attribute.
    /// </summary>
    /// <param name="hash">The hash value of the property.</param>
    /// <param name="flags">The flags that define the property's behavior.</param>
    public PropertyFieldAttribute(uint hash, uint flags) {
        Hash = hash;
        Flags = flags;
    }

}