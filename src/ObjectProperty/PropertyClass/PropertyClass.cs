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
public abstract class PropertyClass {

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

    /// <summary>
    /// Registers a property with the specified name, flags, and accessors.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <param name="flags">The flags associated with the property.</param>
    /// <param name="noTransfer">A boolean value indicating whether the property should be transferred.</param>
    /// <param name="getValue">A function that retrieves the value of the property.</param>
    /// <param name="setValue">An action that sets the value of the property.</param>
    /// <returns><c>true</c> if the property is successfully registered; otherwise, <c>false</c>.</returns>
    internal bool RegisterProperty<T>(string name, PropertyFlags flags, bool noTransfer, Func<T> getValue, Action<T> setValue) {
        if (_propertyReflections.ContainsKey(name)) {
            return false;
        }

        _propertyReflections[name] = new Property<T>(flags, noTransfer, getValue, setValue);
        return true;
    }

    /// <summary>
    /// Unregisters a property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to unregister.</param>
    /// <returns><c>true</c> if the property was successfully unregistered; otherwise, <c>false</c>.</returns>
    internal bool UnregisterProperty(string name) => _propertyReflections.Remove(name);

    internal IProperty this[string name] {
        get {
            if (_propertyReflections.TryGetValue(name, out var property)) {
                return property;
            }

            return null;
        }
    }

}
