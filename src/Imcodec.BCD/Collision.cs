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

using System.IO;

namespace Imcodec.BCD;

/// <summary>
/// Representation of an individual collision entry.
/// Describes a geometric shape and the associated metadata.
/// </summary>
public class Collision {

    /// <summary>
    /// The category flags for the shape.
    /// </summary>
    public CollisionFlags CategoryFlags { get; set; }

    /// <summary>
    /// The collision flags for the shape.
    /// </summary>
    public CollisionFlags CollisionFlags { get; set; }

    /// <summary>
    /// Additional data for mesh-based collisions, if any.
    /// </summary>
    public ProxyMesh? Mesh { get; set; }

    /// <summary>
    /// Universal geometric data for the collision shape.
    /// </summary>
    public ProxyGeometry Geometry { get; set; } = new ProxyGeometry();

    public static Collision ReadFrom(BinaryReader reader) {
        var collision = new Collision();

        var geometryType = reader.ReadUInt32();
        collision.CategoryFlags = (CollisionFlags) reader.ReadUInt32();
        collision.CollisionFlags = (CollisionFlags) reader.ReadUInt32();

        if (geometryType == 6) {
            collision.Mesh = ProxyMesh.ReadFrom(reader);
        }

        // Read geometry
        collision.Geometry = ProxyGeometry.ReadFrom(reader, geometryType);

        return collision;
    }

    public void WriteTo(BinaryWriter writer) {
        writer.Write(Geometry.Params.TypeId);
        writer.Write((uint) CategoryFlags);
        writer.Write((uint) CollisionFlags);

        if (Geometry.Params.TypeId == 6 && Mesh != null) {
            Mesh.WriteTo(writer);
        }

        Geometry.WriteTo(writer);
    }

}