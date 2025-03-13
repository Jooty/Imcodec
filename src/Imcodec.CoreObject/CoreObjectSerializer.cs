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
using Imcodec.ObjectProperty;

namespace Imcodec.CoreObject;

/// <summary>
/// An object serializer that serializes and deserializes
/// <see cref="CoreObject"/> objects.
/// <remarks>
/// Initializes a new instance of the <see cref="ObjectSerializer"/> class.
/// </remarks>
/// <param name="Versionable">States whether the object is versionable.</param>
/// <param name="Behaviors">States the behaviors of the serializer.</param>
/// <param name="typeRegistry">The type registry to use for serialization.</param>
/// <param name="UseServerTypeRegistry">States whether to use the server type registry.</param>
public sealed class CoreObjectSerializer(
    bool versionable = false,
    SerializerFlags behaviors = SerializerFlags.UseFlags | SerializerFlags.Compress,
    TypeRegistry? typeRegistry = null,
    bool UseServerTypeRegistry = true
) : ObjectSerializer(versionable, behaviors, typeRegistry, UseServerTypeRegistry) {

   private static readonly Dictionary<int, (byte, byte)> s_blockAndTypeMap = new() {
      { 350837933, (2, 2) }, // ClientObject
      { 766500222, (104, 2) }, // WizClientObject
      { 1653772158, (115, 9) }, // WizClientObjectItem
      { 1167581154, (106, 2) }, // WizClientPet
      { 2109552587, (108, 2) }, // WizClientMount
      { 398229815, (132, 9) }, // ClientReagentItem
      { 958775582, (131, 131) } // ClientRecipe
   };

   public override bool PreloadObject(BitReader inputBuffer, out PropertyClass? propertyClass) {
      propertyClass = null;
      var block = inputBuffer.ReadUInt8();
      var type = inputBuffer.ReadUInt8();
      var templateIdOrHash = inputBuffer.ReadUInt32();

      if (block == 0 && type == 0) {
         // This is not serialized as a CoreObject. It's a normal object, and dispatch it as such.
         propertyClass = DispatchType(templateIdOrHash);

         return propertyClass != null;
      }

      // We can dispatch the type based on the template ID.
      var hash = GetHashFromBlockAndType(block, type);
      propertyClass = DispatchType(hash);

      return propertyClass != null;
   }

   public override bool PreWriteObject(BitWriter writer, PropertyClass propertyClass) {
      if (propertyClass is null) {
         writer.WriteUInt8(0);
         writer.WriteUInt8(0);
         writer.WriteUInt32(0);

         return false;
      }

      var (block, type) = GetBlockAndType(propertyClass);

      writer.WriteUInt8(block);
      writer.WriteUInt8(type);

      // Write the template ID if this is a CoreObject. Otherwise, write the hash.
      if (propertyClass is ObjectProperty.TypeCache.CoreObject co) {
         var hash = GetHashFromBlockAndType(block, type);
         writer.WriteUInt32(hash);
      }
      else {
         writer.WriteUInt32(propertyClass.GetHash());
      }

      return true;
   }

   private static (byte, byte) GetBlockAndType(PropertyClass propClass) {
      if (propClass is null) {
         return (0, 0);
      }

      return ((byte, byte)) (s_blockAndTypeMap.TryGetValue((int) propClass.GetHash(), out var blockAndType) ? blockAndType : (0, 0));
   }

   private static uint GetHashFromBlockAndType(byte block, byte type) {
      foreach (var (key, value) in s_blockAndTypeMap) {
         if (value == (block, type)) {
            return (uint) key;
         }
      }

      return 0;
   }

}