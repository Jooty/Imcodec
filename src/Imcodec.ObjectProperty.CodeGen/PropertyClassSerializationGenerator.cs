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

using System.Text;
using Imcodec.ObjectProperty.CodeGen.Definitions;

namespace Imcodec.ObjectProperty.CodeGen; 

internal static class PropertyClassSerializationGenerator {
    
    /// <summary>
    /// Generates serialization and deserialization methods for a PropertyClass.
    /// </summary>
    public static string GenerateSerializationMethods(PropertyClassDefinition classDefinition) {
        var sb = new StringBuilder();

        GenerateEncodeMethod(classDefinition, sb);
        sb.AppendLine();
        GenerateDecodeMethod(classDefinition, sb);
        sb.AppendLine();
        
        return sb.ToString();
    }
    
    private static void GenerateEncodeMethod(PropertyClassDefinition classDefinition, StringBuilder sb) {
        sb.AppendLine("\t/// <summary>");
        sb.AppendLine("\t/// Encodes this object's properties using the specified writer and serializer.");
        sb.AppendLine("\t/// </summary>");
        sb.AppendLine("\tinternal override bool Encode(BitWriter writer, ObjectSerializer serializer) {");
        sb.AppendLine("\t\tOnPreEncode();");
        sb.AppendLine();
        sb.AppendLine("\t\tif (serializer.Versionable) {");
        sb.AppendLine("\t\t\treturn EncodeVersionable(writer, serializer);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
        
        // Add property encodings.
        foreach (var property in classDefinition.Properties) {
            sb.AppendLine($"\t\t// Property: {property.Name} (Hash: {property.Hash}, Flags: {property.Flags})");
            sb.AppendLine($"\t\tif (IsPropertyEligibleForProcessing((PropertyFlags) {property.Flags}, serializer)) {{");
            
            sb.AppendLine(GeneratePropertyEncoding(property));
            
            sb.AppendLine("\t\t}");
            sb.AppendLine();
        }
        
        sb.AppendLine("\t\tOnPostEncode();");
        sb.AppendLine("\t\treturn true;");
        sb.AppendLine("\t}");
    }
    
    private static void GenerateDecodeMethod(PropertyClassDefinition classDefinition, StringBuilder sb) {
        sb.AppendLine("\t/// <summary>");
        sb.AppendLine("\t/// Decodes this object's properties using the specified reader and serializer.");
        sb.AppendLine("\t/// </summary>");
        sb.AppendLine("\tinternal override bool Decode(BitReader reader, ObjectSerializer serializer) {");
        sb.AppendLine("\t\tOnPreDecode();");
        sb.AppendLine();
        sb.AppendLine("\t\tif (serializer.Versionable) {");
        sb.AppendLine("\t\t\treturn DecodeVersionable(reader, serializer);");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
        
        // Add property decodings.
        foreach (var property in classDefinition.Properties) {
            sb.AppendLine($"\t\t// Property: {property.Name} (Hash: {property.Hash}, Flags: {property.Flags})");
            sb.AppendLine($"\t\tif (IsPropertyEligibleForProcessing((PropertyFlags) {property.Flags}, serializer)) {{");
            
            sb.AppendLine(GeneratePropertyDecoding(property));
            
            sb.AppendLine("\t\t}");
            sb.AppendLine();
        }
        
        sb.AppendLine("\t\tOnPostDecode();");
        sb.AppendLine("\t\treturn true;");
        sb.AppendLine("\t}");
    }
    
    private static string GeneratePropertyEncoding(PropertyDefinition property) {
        var sb = new StringBuilder();
        
        // Determine the encoding logic based on the property type.
        if (property.IsEnum) {
            sb.AppendLine("\t\t\t// Enum encoding");
            sb.AppendLine("\t\t\tif (serializer.SerializerFlags.HasFlag(SerializerFlags.StringEnums)) {");
            sb.AppendLine($"\t\t\t\twriter.WriteString({property.Name}.ToString());");
            sb.AppendLine("\t\t\t} else {");
            sb.AppendLine($"\t\t\t\twriter.WriteUInt32((uint)(int){property.Name});");
            sb.AppendLine("\t\t\t}");
        }
        else if (property.IsVector) {
            var elementType = ExtractElementType(property.CsharpType);
            sb.AppendLine("\t\t\t// Collection encoding");
            sb.AppendLine($"\t\t\tif ({property.Name} == null) {{");
            sb.AppendLine("\t\t\t\tWriteVectorSize(writer, 0, serializer);");
            sb.AppendLine("\t\t\t} else {");
            sb.AppendLine($"\t\t\t\tWriteVectorSize(writer, {property.Name}.Count, serializer);");
            sb.AppendLine($"\t\t\t\tforeach (var item in {property.Name}) {{");

            var writer = GetWriterMethodForType(elementType, "item");
            if (string.IsNullOrEmpty(writer)) {
                // A writer was not found. Assume the element is a PropertyClass.
                sb.AppendLine("\t\t\t\t\tserializer.PreWriteObject(writer, item);");
                sb.AppendLine("\t\t\t\t\tif (item != null) {");
                sb.AppendLine("\t\t\t\t\t\titem.Encode(writer, serializer);");
                sb.AppendLine("\t\t\t\t\t}");
            }
            else {
                sb.AppendLine($"\t\t\t\t\t{writer}");
            }
            
            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine("\t\t\t}");
        }
        else {
            var writer = GetWriterMethodForType(property.CsharpType, property.Name!);
            if (string.IsNullOrEmpty(writer)) {
                // A writer was not found. Assume the property is a PropertyClass.
                sb.AppendLine("\t\t\t// PropertyClass encoding");
                sb.AppendLine($"\t\t\tserializer.PreWriteObject(writer, {property.Name});");
                sb.AppendLine($"\t\t\tif ({property.Name} != null) {{");
                sb.AppendLine($"\t\t\t\t{property.Name}.Encode(writer, serializer);");
                sb.AppendLine("\t\t\t}");
            }
            else {
                sb.AppendLine($"\t\t\t{writer}");
            }
        }
        
        return sb.ToString();
    }
    
    private static string GeneratePropertyDecoding(PropertyDefinition property) {
        var sb = new StringBuilder();
        
        // Determine the decoding logic based on the property type.
        if (property.IsEnum) {
            sb.AppendLine("\t\t\t// Enum decoding");
            sb.AppendLine("\t\t\tif (serializer.SerializerFlags.HasFlag(SerializerFlags.StringEnums)) {");
            sb.AppendLine("\t\t\t\tvar rawEnumString = reader.ReadString();");
            sb.AppendLine("\t\t\t\tvar enumString = SanitizeStringEnum(rawEnumString);");
            sb.AppendLine($"\t\t\t\t{property.Name} = ({property.CsharpType})Enum.Parse(typeof({property.CsharpType}), enumString, true);");
            sb.AppendLine("\t\t\t} else {");
            sb.AppendLine($"\t\t\t\t{property.Name} = ({property.CsharpType})Enum.ToObject(typeof({property.CsharpType}), reader.ReadUInt32());");
            sb.AppendLine("\t\t\t}");
        }
        else if (property.IsVector) {
            var elementType = ExtractElementType(property.CsharpType);
            sb.AppendLine("\t\t\t// Collection decoding");
            sb.AppendLine($"\t\t\tvar len_{property.Name} = ReadVectorSize(reader, serializer);");
            sb.AppendLine($"\t\t\tif (len_{property.Name} <= 0) {{");
            sb.AppendLine($"\t\t\t\t{property.Name} = new {property.CsharpType}();");
            sb.AppendLine("\t\t\t} else {");
            sb.AppendLine($"\t\t\t\t{property.Name} = new {property.CsharpType}();");
            sb.AppendLine($"\t\t\t\tfor (int i = 0; i < len_{property.Name}; i++) {{");
            
            var reader = GetReaderMethodForType(elementType, $"var item_{property.Name}");
            if (string.IsNullOrEmpty(reader)) {
                // A reader was not found. Assume the element is a PropertyClass.
                sb.AppendLine($"\t\t\t\t\tserializer.PreloadObject(reader, out var item_{property.Name});");
                sb.AppendLine($"\t\t\t\t\tif (item_{property.Name} != null) {{");
                sb.AppendLine($"\t\t\t\t\t\titem_{property.Name}.Decode(reader, serializer);");
                sb.AppendLine("\t\t\t\t\t}");
                sb.AppendLine($"\t\t\t\t\t{property.Name}.Add(({elementType}) item_{property.Name});");
            }
            else {
                sb.AppendLine($"\t\t\t\t\t{reader}");
                sb.AppendLine($"\t\t\t\t\t{property.Name}.Add(item_{property.Name});");
            }
            
            sb.AppendLine("\t\t\t\t}");
            sb.AppendLine("\t\t\t}");
        }
        else {
            var reader = GetReaderMethodForType(property.CsharpType, property.Name!);
            if (string.IsNullOrEmpty(reader)) {
                // A reader was not found. Assume the property is a PropertyClass.
                sb.AppendLine("\t\t\t// PropertyClass decoding");
                sb.AppendLine($"\t\t\tserializer.PreloadObject(reader, out var item_{property.Name});");
                sb.AppendLine($"\t\t\tif ({property.Name} != null) {{");
                sb.AppendLine($"\t\t\t\t{property.Name}.Decode(reader, serializer);");
                sb.AppendLine("\t\t\t}");
            }
            else {
                sb.AppendLine($"\t\t\t{reader}");
            }
        }
        
        return sb.ToString();
    }
    
    private static string ExtractElementType(string collectionType) {
        // Extract element type from List<T> or T[].
        if (collectionType.StartsWith("List<") && collectionType.EndsWith(">")) {
            return collectionType.Substring(5, collectionType.Length - 6);
        }
        if (collectionType.EndsWith("[]")) {
            return collectionType.Substring(0, collectionType.Length - 2);
        }

        return "object";
    }
    
    private static string GetWriterMethodForType(string typeName, string valueVar) {
        return typeName switch {
            "byte" => $"writer.WriteUInt8({valueVar});",
            "sbyte" => $"writer.WriteInt8({valueVar});",
            "char" => $"writer.WriteUInt8(Convert.ToByte({valueVar}));",
            "bool" => $"writer.WriteBit({valueVar});",
            "short" => $"writer.WriteInt16({valueVar});",
            "ushort" => $"writer.WriteUInt16({valueVar});",
            "int" => $"writer.WriteInt32({valueVar});",
            "uint" => $"writer.WriteUInt32({valueVar});",
            "long" => $"writer.WriteInt64({valueVar});",
            "ulong" => $"writer.WriteUInt64({valueVar});",
            "float" => $"writer.WriteFloat({valueVar});",
            "double" => $"writer.WriteDouble({valueVar});",
            "string" or "ByteString" => $"writer.WriteString({valueVar});",
            "WideByteString" => $"writer.WriteWString({valueVar});",
            "Vector3" => $"writer.WriteVector3({valueVar});",
            "Quaternion" => $"writer.WriteQuaternion({valueVar});",
            "Matrix" => $"writer.WriteMatrix({valueVar});",
            "Color" => $"writer.WriteColor({valueVar});",
            "Rectangle" => $"writer.WriteRectangle({valueVar});",
            "RectangleF" => $"writer.WriteRectangleF({valueVar});",
            "Vector2" or "Point" => $"writer.WriteVector2({valueVar});",
            "GID" => $"writer.WriteUInt64({valueVar});",
            "Bui2" => $"writer.WriteBits({valueVar}, 2);",
            "Bui4" => $"writer.WriteBits({valueVar}, 4);",
            "Bui5" => $"writer.WriteBits({valueVar}, 5);",
            "Bui7" => $"writer.WriteBits({valueVar}, 7);",
            "S24" => $"writer.WriteBits({valueVar}, 24);",
            "U24" => $"writer.WriteBits({valueVar}, 24);",
            _ => ""
        };
    }

    private static string GetReaderMethodForType(string typeName, string variableName) {
        return typeName switch {
            "byte" => $"{variableName} = reader.ReadUInt8();",
            "sbyte" => $"{variableName} = reader.ReadInt8();",
            "char" => $"{variableName} = (char)reader.ReadUInt8();",
            "bool" => $"{variableName} = reader.ReadBit();",
            "short" => $"{variableName} = reader.ReadInt16();",
            "ushort" => $"{variableName} = reader.ReadUInt16();",
            "int" => $"{variableName} = reader.ReadInt32();",
            "uint" => $"{variableName} = reader.ReadUInt32();",
            "long" => $"{variableName} = reader.ReadInt64();",
            "ulong" => $"{variableName} = reader.ReadUInt64();",
            "float" => $"{variableName} = reader.ReadFloat();",
            "double" => $"{variableName} = reader.ReadDouble();",
            "string" or "ByteString" => $"{variableName} = reader.ReadString();",
            "WideByteString" => $"{variableName} = reader.ReadWString();",
            "Vector3" => $"{variableName} = reader.ReadVector3();",
            "Quaternion" => $"{variableName} = reader.ReadQuaternion();",
            "Matrix" => $"{variableName} = (Matrix) reader.ReadMatrix();",
            "Color" => $"{variableName} = reader.ReadColor();",
            "Rectangle" => $"{variableName} = reader.ReadRectangle();",
            "RectangleF" => $"{variableName} = reader.ReadRectangleF();",
            "Vector2" => $"{variableName} = reader.ReadVector2();",
            "Point" => $"{variableName} = (Point) reader.ReadVector2();",
            "GID" => $"{variableName} = reader.ReadUInt64();",
            "Bui2" => $"{variableName} = (Bui2) reader.ReadBits<byte>(2);",
            "Bui4" => $"{variableName} = (Bui4) reader.ReadBits<byte>(4);",
            "Bui5" => $"{variableName} = (Bui5) reader.ReadBits<byte>(5);",
            "Bui7" => $"{variableName} = (Bui7) reader.ReadBits<byte>(7);",
            "S24" => $"{variableName} = (S24) reader.ReadBits<int>(24);",
            "U24" => $"{variableName} = (U24) reader.ReadBits<uint>(24);",
            _ => ""
        };
    }
    
    public static string GenerateVersionableMethods(PropertyClassDefinition classDefinition) {
        var sb = new StringBuilder();
        
        // Generate EncodeVersionable method.
        sb.AppendLine("\t/// <summary>");
        sb.AppendLine("\t/// Encodes this object's properties using the specified writer and serializer in versionable format.");
        sb.AppendLine("\t/// </summary>");
        sb.AppendLine("\tprivate bool EncodeVersionable(BitWriter writer, ObjectSerializer serializer) {");
        sb.AppendLine("\t\tvar objectStart = writer.BitPos();");
        sb.AppendLine("\t\twriter.WriteUInt32(0); // Placeholder for the size");
        sb.AppendLine();
        
        // Add property encoding for versionable format.
        foreach (var property in classDefinition.Properties) {
            sb.AppendLine($"\t\t// Property: {property.Name} (Hash: {property.Hash}, Flags: {property.Flags})");
            sb.AppendLine($"\t\tif (IsPropertyEligibleForProcessing((PropertyFlags) {property.Flags}, serializer)) {{");
            sb.AppendLine("\t\t\t// Write the hash and size of the property");
            sb.AppendLine("\t\t\tvar sizeStart = writer.BitPos();");
            sb.AppendLine("\t\t\twriter.WriteUInt32(0); // Placeholder for the size");
            sb.AppendLine($"\t\t\twriter.WriteUInt32({property.Hash});");
            sb.AppendLine();
            sb.AppendLine("\t\t\tvar preEncodeBitPos = writer.BitPos();");
            
            // Generate encoding for this property.
            sb.AppendLine(GeneratePropertyEncoding(property).Replace("\t\t\t", "\t\t\t\t"));
            
            sb.AppendLine();
            sb.AppendLine("\t\t\t// Write the size of the property");
            sb.AppendLine("\t\t\tvar size = writer.BitPos() - preEncodeBitPos;");
            sb.AppendLine("\t\t\twriter.SeekBit(sizeStart);");
            sb.AppendLine("\t\t\twriter.WriteUInt32((uint)size);");
            sb.AppendLine("\t\t\twriter.SeekBit(preEncodeBitPos + size);");
            sb.AppendLine("\t\t}");
            sb.AppendLine();
        }
        
        sb.AppendLine("\t\t// Write the size of the object");
        sb.AppendLine("\t\tvar objectSize = writer.BitPos() - objectStart;");
        sb.AppendLine("\t\twriter.SeekBit(objectStart);");
        sb.AppendLine("\t\twriter.WriteUInt32((uint)objectSize);");
        sb.AppendLine("\t\twriter.SeekBit(objectStart + objectSize);");
        sb.AppendLine();
        sb.AppendLine("\t\treturn true;");
        sb.AppendLine("\t}");
        
        // Generate DecodeVersionable method.
        sb.AppendLine();
        sb.AppendLine("\t/// <summary>");
        sb.AppendLine("\t/// Decodes this object's properties using the specified reader and serializer in versionable format.");
        sb.AppendLine("\t/// </summary>");
        sb.AppendLine("\tprivate bool DecodeVersionable(BitReader reader, ObjectSerializer serializer) {");
        sb.AppendLine("\t\t// Properties may be out of order in the binary stream");
        sb.AppendLine("\t\tvar objectStart = reader.BitPos();");
        sb.AppendLine("\t\tvar objectSize = reader.ReadUInt32();");
        sb.AppendLine();
        sb.AppendLine("\t\twhile (reader.BitPos() - objectStart < objectSize) {");
        sb.AppendLine("\t\t\tvar propertyStart = reader.BitPos();");
        sb.AppendLine("\t\t\tvar propertySize = reader.ReadUInt32();");
        sb.AppendLine("\t\t\tvar propertyHash = reader.ReadUInt32();");
        sb.AppendLine();
        sb.AppendLine("\t\t\t// A property size of 0 would cause an infinite loop");
        sb.AppendLine("\t\t\tif (propertySize == 0) {");
        sb.AppendLine("\t\t\t\treturn false;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\t// Switch on property hash to find the right property");
        sb.AppendLine("\t\t\tswitch (propertyHash) {");
        
        // Generate case statements for each property.
        foreach (var property in classDefinition.Properties) {
            sb.AppendLine($"\t\t\t\tcase {property.Hash}: // {property.Name}");
            
            // Generate decoding for this property.
            sb.Append(GeneratePropertyDecoding(property).Replace("\t\t\t", "\t\t\t\t\t"));
            
            sb.AppendLine("\t\t\t\t\tbreak;");
            sb.AppendLine();
        }
        
        sb.AppendLine("\t\t\t\tdefault:");
        sb.AppendLine("\t\t\t\t\t// Unknown property, skip it");
        sb.AppendLine("\t\t\t\t\tbreak;");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t\t// Seek bit to the end of this property");
        sb.AppendLine("\t\t\treader.SeekBit((int) (propertyStart + propertySize));");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
        sb.AppendLine("\t\t// Seek bit to the end of this object");
        sb.AppendLine("\t\treader.SeekBit((int) (objectStart + objectSize));");
        sb.AppendLine("\t\treturn true;");
        sb.AppendLine("\t}");
        sb.AppendLine();
        
        return sb.ToString();
    }
    
    public static string GenerateHelperMethods() {
        var sb = new StringBuilder();
        
        sb.AppendLine("\t/// <summary>");
        sb.AppendLine("\t/// Determines if a property is eligible for processing based on its flags and the serializer settings.");
        sb.AppendLine("\t/// </summary>");
        sb.AppendLine("\tprivate static bool IsPropertyEligibleForProcessing(PropertyFlags propertyFlags, ObjectSerializer serializer) {");
        sb.AppendLine("\t\tvar serializerFlags = serializer.SerializerFlags;");
        sb.AppendLine("\t\tvar serializerMask = serializer.PropertyMask;");
        sb.AppendLine();
        sb.AppendLine("\t\t// Properties with the Prop_Encode flag set are always encoded");
        sb.AppendLine("\t\tvar alwaysEncode = serializerFlags.HasFlag(SerializerFlags.DirtyEncode) && propertyFlags.HasFlag(PropertyFlags.Prop_Encode);");
        sb.AppendLine();
        sb.AppendLine("\t\t// Check if the property mask is met and if the property is deprecated");
        sb.AppendLine("\t\tvar propertyMaskMet = (propertyFlags & serializerMask) == serializerMask;");
        sb.AppendLine("\t\tvar deprecated = propertyFlags.HasFlag(PropertyFlags.Prop_Deprecated);");
        sb.AppendLine();
        sb.AppendLine("\t\t// Skip properties that are not marked for serialization or are deprecated and not dirty encoded");
        sb.AppendLine("\t\treturn propertyMaskMet && (!deprecated || alwaysEncode);");
        sb.AppendLine("\t}");
        
        sb.AppendLine();
        sb.AppendLine("\t/// <summary>");
        sb.AppendLine("\t/// Writes the size of a vector to the bit writer.");
        sb.AppendLine("\t/// </summary>");
        sb.AppendLine("\tprivate static void WriteVectorSize(BitWriter writer, int size, ObjectSerializer serializer) {");
        sb.AppendLine("\t\tif (serializer.SerializerFlags.HasFlag(SerializerFlags.CompactLength)) {");
        sb.AppendLine("\t\t\tif (size < 127) {");
        sb.AppendLine("\t\t\t\twriter.WriteBits((byte)size, 7);");
        sb.AppendLine("\t\t\t} else {");
        sb.AppendLine("\t\t\t\twriter.WriteBit(true);");
        sb.AppendLine("\t\t\t\twriter.WriteUInt32((uint)size);");
        sb.AppendLine("\t\t\t}");
        sb.AppendLine("\t\t} else {");
        sb.AppendLine("\t\t\twriter.WriteUInt32((uint)size);");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        
        sb.AppendLine();
        sb.AppendLine("\t/// <summary>");
        sb.AppendLine("\t/// Reads the size of a vector from the bit reader.");
        sb.AppendLine("\t/// </summary>");
        sb.AppendLine("\tprivate static uint ReadVectorSize(BitReader reader, ObjectSerializer serializer) {");
        sb.AppendLine("\t\tif (serializer.SerializerFlags.HasFlag(SerializerFlags.CompactLength)) {");
        sb.AppendLine("\t\t\tvar sizeRedundant = reader.ReadBit();");
        sb.AppendLine("\t\t\treturn reader.ReadBits<uint>(sizeRedundant ? 31 : 7);");
        sb.AppendLine("\t\t} else {");
        sb.AppendLine("\t\t\treturn reader.ReadUInt32();");
        sb.AppendLine("\t\t}");
        sb.AppendLine("\t}");
        
        sb.AppendLine();
        sb.AppendLine("\t/// <summary>");
        sb.AppendLine("\t/// Sanitizes a string enum value for parsing.");
        sb.AppendLine("\t/// </summary>");
        sb.AppendLine("\tprivate static string SanitizeStringEnum(string enumString) {");
        sb.AppendLine("\t\t// Client inconsistency: The client will sometimes use '-' and '_' interchangably.");
        sb.AppendLine("\t\tenumString = enumString.Replace('-', '_');");
        sb.AppendLine();
        sb.AppendLine("\t\t// The client is written with C++. We'll replace the scope operator with the C# namespace");
        sb.AppendLine("\t\tenumString = enumString.Replace(\"::\", \".\");");
        sb.AppendLine("\t\tenumString = enumString.Substring(enumString.LastIndexOf('.') + 1);");
        sb.AppendLine();
        sb.AppendLine("\t\treturn enumString;");
        sb.AppendLine("\t}");
        sb.AppendLine();
        
        return sb.ToString();
    }

}
