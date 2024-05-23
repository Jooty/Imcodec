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

using Imcodec.IO;

namespace Imcodec.ObjectProperty.PropertyClass;

/// <summary>
/// Defines class capable of undergoing binary serialization.
/// </summary>
public abstract partial class PropertyClass {

    internal readonly Dictionary<string, IProperty> _propertyReflections = [];

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

    internal bool Decode(BitReader reader) {
        OnPreDecode();
        foreach (var property in _propertyReflections.Values) {
            var castedProp = property as Property<object>
                ?? throw new Exception($"Failed to cast property to {typeof(Property<object>).Name}");

            if (castedProp.NoTransfer) {
                continue;
            }

            if (!castedProp.Decode(reader)) {
                return false;
            }
        }
        OnPostDecode();

        return true;
    }

    internal IProperty this[string name] {
        get => _propertyReflections[name];
        set => _propertyReflections[name] = value;
    }

}
