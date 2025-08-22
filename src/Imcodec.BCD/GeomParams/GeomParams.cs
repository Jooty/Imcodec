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
using System.IO;

namespace Imcodec.BCD.GeomParams;

/// <summary>
/// Extra parameters for the encoded geometric shape.
/// </summary>
public abstract class GeomParams {
    
    public abstract uint TypeId { get; }

    public static GeomParams ReadFrom(BinaryReader reader, uint typeId) {
        return typeId switch {
            0 => new BoxGeomParams {
                Length = reader.ReadSingle(),
                Width = reader.ReadSingle(),
                Depth = reader.ReadSingle()
            },
            1 => new RayGeomParams {
                Position = reader.ReadSingle(),
                Direction = reader.ReadSingle(),
                Length = reader.ReadSingle()
            },
            2 => new SphereGeomParams {
                Radius = reader.ReadSingle()
            },
            3 => new CylinderGeomParams {
                Radius = reader.ReadSingle(),
                Length = reader.ReadSingle()
            },
            4 => new TubeGeomParams {
                Radius = reader.ReadSingle(),
                Length = reader.ReadSingle()
            },
            5 => new PlaneGeomParams {
                Normal = new float[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() },
                Distance = reader.ReadSingle()
            },
            6 => new MeshGeomParams(),
            _ => throw new InvalidDataException($"Unknown geometry type: {typeId}")
        };
    }

    public abstract void WriteTo(BinaryWriter writer);

}