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
public sealed class CoreObjectSerializer(bool Versionable = false,
                    SerializerFlags Behaviors = SerializerFlags.UseFlags | SerializerFlags.Compress,
                    TypeRegistry? typeRegistry = null) : ObjectSerializer(Versionable, Behaviors, typeRegistry) {

   protected override bool PreloadObject(BitReader inputBuffer, out PropertyClass? propertyClass) {
      propertyClass = null;
      var block = inputBuffer.ReadUInt8();
      var type = inputBuffer.ReadUInt8();
      var templateIdOrHash = inputBuffer.ReadUInt32();

      if (block == 0 && type == 0) {
         // This is not serialized as a CoreObject. It's a normal object, and dispatch it as such.
         propertyClass = DispatchType(templateIdOrHash);

         return propertyClass != null;
      }

      // Otherwise, treat it as a CoreObject.
      // We can dispatch the type based on the template ID.
      return false; // todo: fixme
   }

   protected override bool PreWriteObject(BitWriter writer, PropertyClass propertyClass) {
      if (propertyClass is null) {
         writer.WriteUInt8(0);
         writer.WriteUInt8(0);
         writer.WriteUInt32(0);

         return true;
      }

      var (block, type) = GetBlockAndType(propertyClass);

      writer.WriteUInt8(block);
      writer.WriteUInt8(type);

      // Write the template ID if this is a CoreObject. Otherwise, write the hash.
      if (propertyClass is CoreObject co) {
         writer.WriteUInt32((uint) (co.m_coreTemplate.m_templateID & 0xFFFFFFFF));
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

      return propClass.GetHash() switch {
         350837933 => // ClientObject
             (2, 2),
         766500222 => // WizClientObject
             (104, 2),
         1653772158 => // WizClientObjectItem
             (115, 9),
         1167581154 => // WizClientPet
             (106, 2),
         2109552587 => // WizClientMount
             (108, 2),
         398229815 => // ClientReagentItem
             (132, 9),
         958775582 => // ClientRecipe
             (131, 131),
         _ => (0, 0)
      };
   }

}