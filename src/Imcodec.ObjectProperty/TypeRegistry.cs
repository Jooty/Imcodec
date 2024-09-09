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

namespace Imcodec.ObjectProperty;

/// <summary>
/// A registry for types to be used in serialization and deserialization.
/// </summary>
public abstract class TypeRegistry {

    /// <summary>
    /// Register a type to be used in serialization and deserialization.
    /// </summary>
    /// <param name="hash">The type to register.</param>
    /// <param name="t">The type to register.</param>
    public abstract void RegisterType(uint hash, System.Type t);

    /// <summary>
    /// Lookup a type by its hash.
    /// </summary>
    /// <param name="hash">The hash to lookup.</param>
    /// <param name="dispatchedType">The type that was found.</param>
    public abstract System.Type? LookupType(uint hash);

}
