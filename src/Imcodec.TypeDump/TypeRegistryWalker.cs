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

using System.Numerics;
using System.Text.RegularExpressions;
using Imcodec.Cryptography;
using Imcodec.ObjectProperty.CodeGen.JSON;
using Iced.Intel;

namespace Imcodec.TypeDump;

internal sealed partial class TypeRegistryWalker(TypeSystemEmulator emulator) {

    private const int NODE_LEFT_OFFSET = 0x0;
    private const int NODE_PARENT_OFFSET = 0x8;
    private const int NODE_RIGHT_OFFSET = 0x10;
    private const int NODE_IS_NIL_OFFSET = 0x19;
    private const int NODE_VALUE_OFFSET = 0x28;
    private const int TYPE_NAME_OFFSET = 0x38;
    private const int TYPE_HASH_OFFSET = 0x58;
    private const int TYPE_POINTER_FLAG_OFFSET = 0x88;
    private const int TYPE_PROPERTY_LIST_OFFSET = 0x90;
    private const int LIST_SINGLETON_FLAG_OFFSET = 0x9;
    private const int LIST_PARENT_OFFSET = 0x18;
    private const int LIST_TYPE_OFFSET = 0x20;
    private const int LIST_PROPERTIES_BEGIN_OFFSET = 0x58;
    private const int LIST_PROPERTIES_END_OFFSET = 0x60;
    private const int LIST_NAME_OFFSET = 0xB8;
    private const int PROPERTY_LIST_ENTRY_SIZE = 16;
    private const int PROPERTY_OWNER_LIST_OFFSET = 0x38;
    private const int PROPERTY_ACCESSOR_OFFSET = 0x40;
    private const int PROPERTY_ID_OFFSET = 0x50;
    private const int PROPERTY_NAME_OFFSET = 0x58;
    private const int PROPERTY_HASH_OFFSET = 0x64;
    private const int PROPERTY_OFFSET_OFFSET = 0x68;
    private const int PROPERTY_TYPE_OFFSET = 0x70;
    private const int PROPERTY_FLAGS_OFFSET = 0x80;
    private const int PROPERTY_OPTIONS_BEGIN_OFFSET = 0x98;
    private const int PROPERTY_OPTIONS_END_OFFSET = 0xA0;
    private const int OPTION_ENTRY_SIZE = 0x48;
    private const int OPTION_VALUE_OFFSET = 0x20;
    private const int OPTION_NAME_OFFSET = 0x28;
    private const int ACCESSOR_NAME_SLOT_OFFSET = 8;
    private const int ACCESSOR_DYNAMIC_SLOT_OFFSET = 0x20;
    private const int INSTRUCTION_PEEK_LENGTH = 16;
    private const int MINIMUM_PLAUSIBLE_TYPE_COUNT = 5000;
    private const string ROOT_PROPERTY_CLASS_NAME = "class PropertyClass";

    private readonly Dictionary<ulong, (string? Name, bool? Dynamic)> _accessorCache = [];

    public List<string> Problems { get; } = [];

    public SortedSet<string> EnumTypesWithoutOptions { get; } = [];

    public JsonDumpManifest Walk(ulong nameRegistryGetter) {
        var registry = emulator.Call(nameRegistryGetter);
        var types = CollectTypes(emulator.U64(registry));
        var classes = new List<JsonDumpClass>();
        foreach (var type in types) {
            classes.Add(ReadClass(type));
        }

        Verify(classes);
        var manifest = new JsonDumpManifest { Version = 2 };
        foreach (var @class in classes.OrderBy(c => c.Hash)) {
            manifest.Classes[@class.Hash.ToString()] = @class;
        }

        return manifest;
    }

    private List<ulong> CollectTypes(ulong head) {
        var types = new List<ulong>();
        var seen = new HashSet<ulong>();
        var pending = new Stack<ulong>();
        pending.Push(emulator.U64(head + NODE_PARENT_OFFSET));
        while (pending.Count > 0) {
            var node = pending.Pop();
            if (!seen.Add(node) || emulator.U8(node + NODE_IS_NIL_OFFSET) != 0) {
                continue;
            }

            types.Add(emulator.U64(node + NODE_VALUE_OFFSET));
            pending.Push(emulator.U64(node + NODE_LEFT_OFFSET));
            pending.Push(emulator.U64(node + NODE_RIGHT_OFFSET));
        }

        return types;
    }

    private JsonDumpClass ReadClass(ulong type) {
        var @class = new JsonDumpClass {
            Name = emulator.StdString(type + TYPE_NAME_OFFSET),
            Hash = emulator.U32(type + TYPE_HASH_OFFSET),
            BaseClasses = [],
        };

        var list = emulator.U64(type + TYPE_PROPERTY_LIST_OFFSET);
        if (list == 0) {
            return @class;
        }

        for (var parent = emulator.U64(list + LIST_PARENT_OFFSET); parent != 0; parent = emulator.U64(parent + LIST_PARENT_OFFSET)) {
            @class.BaseClasses.Add(emulator.StdString(parent + LIST_NAME_OFFSET));
        }

        var begin = emulator.U64(list + LIST_PROPERTIES_BEGIN_OFFSET);
        var end = emulator.U64(list + LIST_PROPERTIES_END_OFFSET);
        for (var slot = begin; slot < end; slot += PROPERTY_LIST_ENTRY_SIZE) {
            var property = emulator.U64(slot);
            @class.Properties[emulator.CString(emulator.U64(property + PROPERTY_NAME_OFFSET))] = ReadProperty(property);
        }

        return @class;
    }

    private JsonDumpProperty ReadProperty(ulong property) {
        var type = emulator.U64(property + PROPERTY_TYPE_OFFSET);
        var (container, dynamic) = ReadAccessor(emulator.U64(property + PROPERTY_ACCESSOR_OFFSET));
        var typeName = emulator.StdString(type + TYPE_NAME_OFFSET);
        var result = new JsonDumpProperty {
            Type = typeName,
            Id = (uint) emulator.I32(property + PROPERTY_ID_OFFSET),
            Offset = (uint) emulator.I32(property + PROPERTY_OFFSET_OFFSET),
            Flags = (uint) emulator.I32(property + PROPERTY_FLAGS_OFFSET),
            Container = container,
            Dynamic = dynamic ?? false,
            Singleton = emulator.U8(emulator.U64(property + PROPERTY_OWNER_LIST_OFFSET) + LIST_SINGLETON_FLAG_OFFSET) != 0,
            Pointer = emulator.U8(type + TYPE_POINTER_FLAG_OFFSET) != 0,
            Hash = emulator.U32(property + PROPERTY_HASH_OFFSET),
        };

        if (container is null || dynamic is null) {
            Problems.Add($"unreadable container for a property of type {typeName}");
        }

        var begin = emulator.U64(property + PROPERTY_OPTIONS_BEGIN_OFFSET);
        var end = emulator.U64(property + PROPERTY_OPTIONS_END_OFFSET);
        if (begin != 0) {
            for (var option = begin; option < end; option += OPTION_ENTRY_SIZE) {
                var text = emulator.StdString(option);
                result.EnumOptions[emulator.StdString(option + OPTION_NAME_OFFSET)] =
                    ToWizTypeEnumValue(text, emulator.U32(option + OPTION_VALUE_OFFSET));
            }
        }

        return result;
    }

    private (string? Name, bool? Dynamic) ReadAccessor(ulong accessor) {
        if (accessor == 0) {
            return (null, null);
        }

        var vtable = emulator.U64(accessor);
        if (_accessorCache.TryGetValue(vtable, out var cached)) {
            return cached;
        }

        var nameInstruction = DecodeFirst(emulator.U64(vtable + ACCESSOR_NAME_SLOT_OFFSET));
        string? name = nameInstruction.Code == Code.Lea_r64_m && nameInstruction.MemoryBase == Register.RIP
            ? emulator.CString(nameInstruction.IPRelativeMemoryAddress)
            : null;
        var dynamicInstruction = DecodeFirst(emulator.U64(vtable + ACCESSOR_DYNAMIC_SLOT_OFFSET));
        bool? dynamic = dynamicInstruction.Code switch {
            Code.Mov_r8_imm8 => dynamicInstruction.Immediate8 != 0,
            Code.Xor_r8_rm8 or Code.Xor_rm8_r8 => false,
            _ => null,
        };

        return _accessorCache[vtable] = (name, dynamic);
    }

    private Instruction DecodeFirst(ulong address) {
        var decoder = Decoder.Create(64, emulator.ReadBytes(address, INSTRUCTION_PEEK_LENGTH));
        decoder.IP = address;
        return decoder.Decode();
    }

    private void Verify(List<JsonDumpClass> classes) {
        if (classes.Count < MINIMUM_PLAUSIBLE_TYPE_COUNT) {
            Problems.Add($"only {classes.Count} types were registered");
        }

        if (!classes.Any(c => c.Name == ROOT_PROPERTY_CLASS_NAME)) {
            Problems.Add($"{ROOT_PROPERTY_CLASS_NAME} was not registered");
        }

        foreach (var @class in classes) {
            if (StringHash.Compute(@class.Name!) != @class.Hash) {
                Problems.Add($"type hash mismatch for {@class.Name}");
            }

            foreach (var (name, property) in @class.Properties) {
                if (StringHash.HashPropertyName(name, property.Type!) != property.Hash) {
                    Problems.Add($"property hash mismatch for {@class.Name}.{name}");
                }
            }

            foreach (var property in @class.Properties.Values.Where(p => p.Type!.StartsWith("enum ") && p.EnumOptions.Count == 0)) {
                EnumTypesWithoutOptions.Add(property.Type!);
            }
        }
    }

    private static object ToWizTypeEnumValue(string text, uint storedValue) {
        if (text.Length == 0) {
            return storedValue;
        }

        var trimmed = text.Trim(' ', '\t', '\n', '\r', '\f', '\v');
        if (!IntegerLiteral().IsMatch(trimmed)) {
            return text;
        }

        var value = BigInteger.Parse(trimmed.Replace("_", string.Empty));
        return (uint) (value & uint.MaxValue);
    }

    [GeneratedRegex("^[+-]?[0-9]+(_[0-9]+)*$")]
    private static partial Regex IntegerLiteral();

}
