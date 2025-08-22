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

using System.Collections.Generic;
using System.IO;

namespace Imcodec.BCD;

/// <summary>
/// Representation of an arbitrary mesh shape.
/// </summary>
public class ProxyMesh {

    /// <summary>
    /// A dynamic list of vertices in the mesh.
    /// </summary>
    public List<float[]> Vertices { get; set; } = new List<float[]>();

    /// <summary>
    /// A dynamic list of faces in the mesh.
    /// </summary>
    public List<Face> Faces { get; set; } = new List<Face>();

    public static ProxyMesh ReadFrom(BinaryReader reader) {
        var mesh = new ProxyMesh();

        uint vertexCount = reader.ReadUInt32();
        uint faceCount = reader.ReadUInt32();

        for (int i = 0; i < vertexCount; i++) {
            mesh.Vertices.Add(new float[]
            {
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
            });
        }

        for (int i = 0; i < faceCount; i++) {
            var faceVector = new uint[]
            {
                    reader.ReadUInt32(),
                    reader.ReadUInt32(),
                    reader.ReadUInt32()
            };

            var normal = new float[]
            {
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
            };

            mesh.Faces.Add(new Face(faceVector, normal));
        }

        return mesh;
    }

    public void WriteTo(BinaryWriter writer) {
        writer.Write((uint) Vertices.Count);
        writer.Write((uint) Faces.Count);

        foreach (var vertex in Vertices) {
            writer.Write(vertex[0]);
            writer.Write(vertex[1]);
            writer.Write(vertex[2]);
        }

        foreach (var face in Faces) {
            writer.Write(face.FaceVector[0]);
            writer.Write(face.FaceVector[1]);
            writer.Write(face.FaceVector[2]);
            writer.Write(face.Normal[0]);
            writer.Write(face.Normal[1]);
            writer.Write(face.Normal[2]);
        }
    }

}