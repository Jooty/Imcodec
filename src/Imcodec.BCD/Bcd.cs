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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Imcodec.BCD;

/// <summary>
/// Representation of a BCD file.
/// Crate for parsing and writing Binary Collision Data (BCD) files.
/// As the name suggests, this format describes geometric collision
/// shapes for zones and is used for physics.
/// </summary>
public class Bcd {

    /// <summary>
    /// A list of all Collision objects in the file.
    /// </summary>
    public List<Collision> Collisions { get; set; } = new List<Collision>();

    /// <summary>
    /// Attempts to parse a BCD file from a given Stream.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <returns>A parsed BCD object</returns>
    /// <exception cref="InvalidDataException">Thrown when the file format is invalid</exception>
    /// <exception cref="EndOfStreamException">Thrown when the stream ends unexpectedly</exception>
    public static Bcd Parse(Stream stream) {
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

        var bcd = new Bcd();
        var collisionCount = reader.ReadUInt32();

        for (int i = 0; i < collisionCount; i++) {
            bcd.Collisions.Add(Collision.ReadFrom(reader));
        }

        return bcd;
    }

    /// <summary>
    /// Parses a BCD file from a file path.
    /// </summary>
    /// <param name="filePath">Path to the BCD file</param>
    /// <returns>A parsed BCD object</returns>
    public static Bcd ParseFromFile(string filePath) {
        using var fileStream = File.OpenRead(filePath);
        
        return Parse(fileStream);
    }

    /// <summary>
    /// Writes the BCD data to the given Stream.
    /// </summary>
    /// <param name="stream">The stream to write to</param>
    /// <exception cref="IOException">Thrown when writing fails</exception>
    public void Write(Stream stream) {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

        writer.Write((uint) Collisions.Count);

        foreach (var collision in Collisions) {
            collision.WriteTo(writer);
        }
    }

    /// <summary>
    /// Writes the BCD data to a file.
    /// </summary>
    /// <param name="filePath">Path where to save the BCD file</param>
    public void WriteToFile(string filePath) {
        using var fileStream = File.Create(filePath);
        Write(fileStream);
    }

}