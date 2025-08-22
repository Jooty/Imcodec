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
using System.Text;
using Imcodec.BCD.GeomParams;

namespace Imcodec.BCD;

/// <summary>
/// Representation of any geometric shape.
/// </summary>
public class ProxyGeometry {

    /// <summary>
    /// The name of the shape.
    /// /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The rotation matrix of the shape.
    /// </summary>
    public float[,] Rotation { get; set; } = new float[3, 3];

    /// <summary>
    /// The location vector of the shape.
    /// </summary>
    public float[] Location { get; set; } = new float[3];

    /// <summary>
    /// The scaling factor of the shape.
    /// </summary>
    public float Scale { get; set; }

    /// <summary>
    /// The material name for the shape.
    /// </summary>
    public string Material { get; set; } = string.Empty;

    /// <summary>
    /// Geometric shape parameters.
    /// </summary>
    public GeomParams.GeomParams Params { get; set; } = new MeshGeomParams();

    public static ProxyGeometry ReadFrom(BinaryReader reader, uint geometryType) {
        var geometry = new ProxyGeometry();

        uint nameLen = reader.ReadUInt32();
        byte[] nameBytes = reader.ReadBytes((int) nameLen);
        geometry.Name = Encoding.UTF8.GetString(nameBytes);

        // Read rotation matrix (3x3).
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                geometry.Rotation[i, j] = reader.ReadSingle();
            }
        }

        geometry.Location[0] = reader.ReadSingle();
        geometry.Location[1] = reader.ReadSingle();
        geometry.Location[2] = reader.ReadSingle();

        geometry.Scale = reader.ReadSingle();

        uint materialLen = reader.ReadUInt32();
        byte[] materialBytes = reader.ReadBytes((int) materialLen);
        geometry.Material = Encoding.UTF8.GetString(materialBytes);

        geometry.Params = GeomParams.GeomParams.ReadFrom(reader, geometryType);

        return geometry;
    }

    public void WriteTo(BinaryWriter writer) {
        byte[] nameBytes = Encoding.UTF8.GetBytes(Name);
        writer.Write((uint) nameBytes.Length);
        writer.Write(nameBytes);

        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++) {
                writer.Write(Rotation[i, j]);
            }
        }

        writer.Write(Location[0]);
        writer.Write(Location[1]);
        writer.Write(Location[2]);

        writer.Write(Scale);

        byte[] materialBytes = Encoding.UTF8.GetBytes(Material);
        writer.Write((uint) materialBytes.Length);
        writer.Write(materialBytes);

        Params.WriteTo(writer);
    }

}
