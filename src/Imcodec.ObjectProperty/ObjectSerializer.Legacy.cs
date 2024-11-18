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
/// [Deprecated] Use Versionable property instead.
/// This enum maintains compatibility with legacy code.
/// </summary>
[Obsolete("Use bool Versionable property instead")]
public enum Mode {

    /// <summary>
    /// Compact mode - maps to Versionable = false
    /// </summary>
    Compact,
    
    /// <summary>
    /// Verbose mode - maps to Versionable = true
    /// </summary>
    Verbose

}

public partial class ObjectSerializer {

    // Backwards compatibility properties
    /// <summary>
    /// [Deprecated] Use SerializerFlags instead.
    /// This property maintains compatibility with legacy code.
    /// </summary>
    [Obsolete("Use SerializerFlags property instead")]
    public SerializerFlags BehaviorFlags {
        get => SerializerFlags;
        set => SerializerFlags = value;
    }

    /// <summary>
    /// [Deprecated] Use Versionable instead.
    /// This property maintains compatibility with legacy code that expects SerializerMode.
    /// </summary>
    [Obsolete("Use Versionable property instead. true maps to Mode.Verbose, false to Mode.Compact")]
    public Mode SerializerMode {
        get => Versionable ? Mode.Verbose : Mode.Compact;
        set => Versionable = value == Mode.Verbose;
    }

    /// <summary>
    /// [Deprecated] Use the primary constructor instead.
    /// This constructor maintains compatibility with legacy code.
    /// </summary>
    [Obsolete("Use ObjectSerializer(bool, SerializerFlags, TypeRegistry?) constructor instead")]
    public ObjectSerializer() : this(true, SerializerFlags.None) { }

    // Backwards compatibility methods
    /// <summary>
    /// [Deprecated] Use SerializerFlags property instead.
    /// This method maintains compatibility with legacy code.
    /// </summary>
    [Obsolete("Use SerializerFlags property directly")]
    public virtual ObjectSerializer OnBehaviors(SerializerFlags flags) {
        SerializerFlags = flags;
        return this;
    }

    /// <summary>
    /// [Deprecated] Use Versionable property instead.
    /// This method maintains compatibility with legacy code.
    /// </summary>
    [Obsolete("Use Versionable property directly")]
    public virtual ObjectSerializer OnMode(Mode mode) {
        Versionable = mode == Mode.Verbose;
        return this;
    }

    /// <summary>
    /// [Deprecated] Use PropertyMask property instead.
    /// This method maintains compatibility with legacy code.
    /// </summary>
    [Obsolete("Use PropertyMask property directly")]
    public virtual ObjectSerializer OnPropertyMask(PropertyFlags flags) {
        PropertyMask = flags;
        return this;
    }

    /// <summary>
    /// [Deprecated] Use Serialize method instead.
    /// This method maintains compatibility with legacy code.
    /// </summary>
    [Obsolete("Use Serialize method instead")]
    public ByteString Serialize(PropertyClass propertyClass) {
        if (Serialize(propertyClass, PropertyMask, out var output)) {
            return new ByteString(output!);
        }
        
        throw new Exception("Serialization failed");
    }

    /// <summary>
    /// [Deprecated] Use Deserialize method instead.
    /// This method maintains compatibility with legacy code.
    /// </summary>
    [Obsolete("Use Deserialize<T> method instead")]
    public PropertyClass? Deserialize(byte[] buffer) {
        if (Deserialize<PropertyClass>(buffer, PropertyMask, out var output)) {
            return output;
        }

        return null;
    }

}