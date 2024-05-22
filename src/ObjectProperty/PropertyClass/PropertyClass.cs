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

namespace Imcodec.ObjectProperty.PropertyClass;

/// <summary>
/// Defines class capable of undergoing binary serialization.
/// </summary>
public abstract partial class PropertyClass {

    private readonly Dictionary<string, Func<Property>> _propertyGetters;

    public PropertyClass() {
        _propertyGetters = [];
    }

    /// <summary>
    /// Called before saving the object to the binary stream.
    /// </summary>
    public virtual void OnPreSave() {
        if (_propertyGetters.Count <= 0) {
            throw new InvalidOperationException("No properties to save. Ensure that the properties are registered before saving.");
        }
    }

    /// <summary>
    /// Called after saving the object to the binary stream.
    /// </summary>
    public virtual void OnPostSave() { }

    /// <summary>
    /// Called before loading the object from the binary stream.
    /// </summary>
    public virtual void OnPreLoad() { }

    /// <summary>
    /// Called after loading the object from the binary stream.
    /// </summary>
    public virtual void OnPostLoad() { }

    /// <summary>
    /// Registers a property with the specified name and getter function.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="getter">A function that returns the property value.</param>
    public void RegisterProperty(string name, Func<Property> getter) {
        // Ensure that this property is not already registered.
        if (_propertyGetters.ContainsKey(name)) {
            throw new InvalidOperationException($"Property '{name}' is already registered.");
        }

        _propertyGetters[name] = getter;
    }

    /// <summary>
    /// Unregisters a property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property to unregister.</param>
    public void UnregisterProperty(string name) {
        if (!_propertyGetters.Remove(name)) {
            throw new InvalidOperationException($"Property '{name}' is not registered.");
        }
    }

}
