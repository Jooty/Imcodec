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
using Imcodec.ObjectProperty.TypeCache;

namespace Imcodec.ObjectProperty;

[Flags]
public enum SerializerFlags {

    /// <summary>
    /// States the serializer should use no flags.
    /// </summary>
    None,

    /// <summary>
    /// States the serializer should use these flags for deserialization.
    /// </summary>
    UseFlags = 1 << 0,

    /// <summary>
    /// States the serializer should use compact length prefixes.
    /// </summary>
    CompactLength = 1 << 1,

    /// <summary>
    /// States the serializer should use string enums.
    /// </summary>
    StringEnums = 1 << 2,

    /// <summary>
    /// States the serializer should use ZLib compression.
    /// </summary>
    Compress = 1 << 3,

    /// <summary>
    /// Properties are dirty encoded.
    /// </summary>
    DirtyEncode = 1 << 4,

}

/// <summary>
/// An object serializer that serializes and deserializes
/// <see cref="PropertyClass"/> objects.
/// <remarks>
/// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
/// </remarks>
/// <param name="Versionable">States whether the object is versionable.</param>
/// <param name="Behaviors">States the behaviors of the serializer.</param>
/// <param name="typeRegistry">The type registry to use for serialization.</param>
/// <param name="UseServerTypeRegistry">States whether to use the server type registry.</param>
public partial class ObjectSerializer(bool Versionable = true,
                        SerializerFlags Behaviors = SerializerFlags.None,
                        TypeRegistry? typeRegistry = null,
                        bool UseServerTypeRegistry = true) {

    /// <summary>
    /// States whether the object is versionable. If true, <see cref="Property"/>
    /// data is prefixed with a hash of the <see cref="PropertyClass"/> type as
    /// well as the size of the data in bits.
    /// </summary>
    public bool Versionable { get; set; } = Versionable;

    /// <summary>
    /// States the behaviors of the serializer.
    /// </summary>
    public SerializerFlags SerializerFlags { get; set; } = Behaviors;

    /// <summary>
    /// The property flags to use for serialization.
    /// </summary>
    public PropertyFlags PropertyMask { get; set; }
        = PropertyFlags.Prop_Transmit | PropertyFlags.Prop_AuthorityTransmit;

    /// <summary>
    /// The type registry to dispatch types from.
    /// </summary>
    public TypeRegistry TypeRegistry { get; set; } = typeRegistry ?? s_defaultTypeRegistry;

    /// <summary>
    /// States whether to use the server type registry. These are types not available 
    /// from the client data and have been manually recreated.
    /// </summary>
    public bool UseServerTypeRegistry { get; set; } = UseServerTypeRegistry;

    private static readonly ClientGeneratedTypeRegistry s_defaultTypeRegistry = new();
    private static readonly ServerGeneratedTypeRegistry s_serverTypeRegistry = new();

    /// <summary>
    /// Serializes the specified <see cref="PropertyClass"/> object using
    /// the provided property mask.
    /// </summary>
    /// <param name="input">The <see cref="PropertyClass"/> object to serialize.</param>
    /// <param name="propertyMask">The property mask to use for serialization.</param>
    /// <param name="output">The serialized byte array output.</param>
    /// <returns><c>true</c> if the serialization is successful; otherwise, <c>false</c>.</returns>
    public virtual bool Serialize(PropertyClass input,
                                  uint propertyMask,
                                  out ByteString? output) {
        var castedFlags = (PropertyFlags) propertyMask;

        return Serialize(input, castedFlags, out output);
    }

    /// <summary>
    /// Serializes the specified <see cref="PropertyClass"/> object using
    /// the provided <see cref="PropertyFlags"/> mask.
    /// </summary>
    /// <param name="input">The <see cref="PropertyClass"/> object to serialize.</param>
    /// <param name="propertyMask">The <see cref="PropertyFlags"/> mask to
    /// apply during serialization.</param>
    /// <param name="output">The serialized byte array output.</param>
    public virtual bool Serialize(PropertyClass input,
                                  PropertyFlags propertyMask,
                                  out ByteString? output) {
        output = default;
        this.PropertyMask = propertyMask;
        var writer = new BitWriter();

        // If the flags request it, ensure our BitWriter is writing with compact lengths.
        if (SerializerFlags.HasFlag(SerializerFlags.CompactLength)) {
            writer.WithCompactLengths();
        }

        if (!PreWriteObject(writer, input)) {
            return false;
        }

        // Tell the property class to encode its properties.
        if (!input.Encode(writer, this)) {
            return false;
        }

        // If the behaviors flag is set to use compression,
        // compress the output buffer.
        if (SerializerFlags.HasFlag(SerializerFlags.Compress)) {
            writer = Compress(writer);
        }

        output = writer.GetData();

        return true;
    }

    /// <summary>
    /// Deserializes the input buffer into an instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="inputBuffer">The input buffer containing the serialized data.</param>
    /// <param name="propertyMask">The property mask to use for serialization.</param>
    /// <param name="output">When this method returns, contains the deserialized
    /// object of type <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the deserialization is successful; otherwise,
    public virtual bool Deserialize<T>(byte[] inputBuffer,
                                       uint propertyMask,
                                       out T? output) where T : PropertyClass {
        var castedFlags = (PropertyFlags) propertyMask;
        
        return Deserialize(inputBuffer, castedFlags, out output);
    }

    /// <summary>
    /// Deserializes the input buffer into an instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="inputBuffer">The input buffer containing the serialized data.</param>
    /// <param name=""propertyMask"">The property flags to use for serialization.</param>
    /// <param name="output">When this method returns, contains the deserialized
    /// object of type <typeparamref name="T"/>.</param>
    /// <returns><c>true</c> if the deserialization is successful; otherwise,
    /// <c>false</c>.</returns>
    public virtual bool Deserialize<T>(byte[] inputBuffer,
                                       PropertyFlags propertyMask,
                                       out T? output) where T : PropertyClass {
        output = default;
        this.PropertyMask = propertyMask;
        var reader = new BitReader(inputBuffer);

        // If the behaviors flag is set to use compression, decompress the input buffer.
        if (SerializerFlags.HasFlag(SerializerFlags.Compress)) {
            reader = Decompress(reader);
        }

        // If the flags request it, ensure our BitReader is reading with compact lengths.
        if (SerializerFlags.HasFlag(SerializerFlags.CompactLength)) {
            reader.WithCompactLengths();
        }

        if (!PreloadObject(reader, out var propertyClass)) {
            return false;
        }

        propertyClass?.Decode(reader, this);

        output = (T) propertyClass!;

        return true;
    }

    /// <summary>
    /// Preloads an object from the input buffer based on the provided hash value.
    /// </summary>
    /// <param name="inputBuffer">The input buffer containing the serialized data.</param>
    /// <param name="propertyClass">The loaded property class, if found.</param>
    /// <returns><c>true</c> if the object was preloaded successfully; otherwise, <c>false</c>.</returns>
    public virtual bool PreloadObject(BitReader inputBuffer,
                                         out PropertyClass? propertyClass) {
        var hash = inputBuffer.ReadUInt32();
        if (hash == 0) {
            propertyClass = null;

            return false;
        }

        propertyClass = DispatchType(hash);

        return propertyClass != null;
    }

    /// <summary>
    /// Writes the object identifier to the specified <see cref="BitWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="BitWriter"/> to write to.</param>
    /// <param name="propertyClass">The <see cref="PropertyClass"/> to write.</param>
    /// <returns><c>true</c> if the object identifier was written successfully; otherwise, <c>false</c>.</returns>
    public virtual bool PreWriteObject(BitWriter writer,
                                          PropertyClass propertyClass) {
        if (propertyClass == null) {
            writer.WriteUInt32(0);

            return false;
        }

        writer.WriteUInt32(propertyClass.GetHash());

        return true;
    }

    /// <summary>
    /// Compresses the data using the specified <see cref="BitWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="BitWriter"/> containing the data
    /// to compress.</param>
    /// <returns>A <see cref="BitWriter"/> containing the compressed data.</returns>
    protected virtual BitWriter Compress(BitWriter writer) {
        var writerData = writer.GetData();
        var uncompressedSize = writerData.Length;
        var compressedData = Compression.Compress(writerData);

        var deflatedBuffer = new byte[sizeof(int) + compressedData.Length];

        var sizeSpan = new Span<byte>(deflatedBuffer, 0, sizeof(int));
        BitConverter.TryWriteBytes(sizeSpan, uncompressedSize);

        compressedData.CopyTo(new Span<byte>(deflatedBuffer, sizeof(int), compressedData.Length));

        return new BitWriter(deflatedBuffer);
    }

    /// <summary>
    /// Decompresses the data using the specified <see cref="BitReader"/>.
    /// </summary>
    /// <param name="inputBuffer">The cref="BitReader"/> containing
    /// the compressed data.</param>
    /// <returns>A <see cref="BitReader"/> containing the decompressed data.</returns>
    protected virtual BitReader Decompress(BitReader inputBuffer) {
        // Read the uncompressed length from the first 4 bytes of the input buffer.
        // The rest of the buffer is the compressed data.
        var uncompressedLength = inputBuffer.ReadInt32();
        var decompressedData = Compression.Decompress(inputBuffer.GetData()[4..]);

        return decompressedData.Length != uncompressedLength
            ? throw new Exception("Decompressed data length does not match the recorded length.")
            : new BitReader(decompressedData);
    }

    /// <summary>
    /// Dispatches the type based on the provided hash value.
    /// </summary>
    /// <param name="hash">The hash value to dispatch.</param>
    /// <returns>The dispatched <see cref="PropertyClass"/> type, if found; otherwise, <c>null</c>.</returns>
    protected PropertyClass? DispatchType(uint hash) {
        var lookupType = TypeRegistry.LookupType(hash);
        if (lookupType == null) {
            // If the type is not found in the client type registry,
            // search the server type registry if flags permit.
            if (UseServerTypeRegistry) {
                lookupType = s_serverTypeRegistry.LookupType(hash);

                if (lookupType != null) {
                    return (PropertyClass) Activator.CreateInstance(lookupType)!;
                }
            }

            return null;
        }

        return (PropertyClass) Activator.CreateInstance(lookupType)!;
    }

}
