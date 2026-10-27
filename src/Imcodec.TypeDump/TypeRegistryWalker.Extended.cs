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

using UnicornEngine;

namespace Imcodec.TypeDump;

internal sealed partial class TypeRegistryWalker {

    private const int TYPE_ID_OFFSET = 0x30;
    private const int TYPE_SIZE_OFFSET = 0x60;
    private const int TYPE_REFERENCE_FLAG_OFFSET = 0x89;
    private const int TYPE_SKIP_PROBE_SLOT_OFFSET = 0x8;
    private const int TYPE_CONSTRUCT_SLOT_OFFSET = 0x20;
    private const int LIST_BASE_OFFSET_OFFSET = 0x10;
    private const int LIST_METHODS_BEGIN_OFFSET = 0x70;
    private const int LIST_METHODS_END_OFFSET = 0x78;
    private const int METHOD_NAME_OFFSET = 0x38;
    private const int METHOD_DELEGATE_OFFSET = 0x58;
    private const int METHOD_FLAGS_OFFSET = 0x68;
    private const int METHOD_DESCRIPTION_OFFSET = 0x70;
    private const int DELEGATE_RETURN_TYPE_SLOT_OFFSET = 0x8;
    private const int DELEGATE_ARGUMENT_TYPES_SLOT_OFFSET = 0x10;
    private const int PROPERTY_NOTE_OFFSET = 0x88;
    private const int PROPERTY_PERSISTENCE_INFO_OFFSET = 0x90;
    private const int PROPERTY_ALTERNATE_HASHES_OFFSET = 0xD0;
    private const int LIST_NODE_NEXT_OFFSET = 0x0;
    private const int LIST_NODE_VALUE_OFFSET = 0x10;
    private const int ACCESSOR_COUNT_SLOT_OFFSET = 0x10;
    private const int ACCESSOR_ELEMENT_SLOT_OFFSET = 0x18;
    private const int OBJECT_CONVERT_SLOT_OFFSET = 0x20;
    private const int POINTER_VECTOR_SIZE = 24;
    private const int POINTER_SIZE = 8;
    private const int PROBE_BUFFER_SIZE = 0x8000;
    private const int PROBE_ITERATION_LIMIT = 1 << 16;
    private const long PROBE_INSTRUCTION_BUDGET = 5_000_000;
    private const string STRING_TYPE_NAME = "std::string";
    private const string PRIMARY_KEY_HINT = "primaryKey";
    private const string NULLABLE_HINT = "nullable";
    private const string DEFAULT_VALUE_HINT = "defaultValue";

    private static readonly byte[] PROBE_FILL_BYTES = [0xA5, 0x5A];

    private readonly Dictionary<ulong, List<ExtendedClass>> _classesByList = [];

    public IReadOnlyList<ulong> TypeAddresses { get; private set; } = [];

    public ExtendedManifest ReadExtended() {
        var manifest = new ExtendedManifest();
        var sorted = new List<(uint Hash, ExtendedClass Class)>();
        foreach (var type in TypeAddresses) {
            sorted.Add((emulator.U32(type + TYPE_HASH_OFFSET), ReadExtendedClass(type)));
        }

        foreach (var (hash, @class) in sorted.OrderBy(entry => entry.Hash)) {
            manifest.Classes[hash.ToString()] = @class;
        }

        ProbeDefaults();
        return manifest;
    }

    private ExtendedClass ReadExtendedClass(ulong type) {
        var list = emulator.U64(type + TYPE_PROPERTY_LIST_OFFSET);
        var @class = new ExtendedClass {
            TypeId = emulator.I32(type + TYPE_ID_OFFSET),
            Size = emulator.U32(type + TYPE_SIZE_OFFSET),
            Pointer = emulator.U8(type + TYPE_POINTER_FLAG_OFFSET) != 0,
            Reference = emulator.U8(type + TYPE_REFERENCE_FLAG_OFFSET) != 0,
            Singleton = list != 0 && emulator.U8(list + LIST_SINGLETON_FLAG_OFFSET) != 0,
            BaseOffset = list == 0 ? 0 : (long) emulator.U64(list + LIST_BASE_OFFSET_OFFSET),
        };

        if (list == 0) {
            return @class;
        }

        if (!_classesByList.TryGetValue(list, out var sharing)) {
            _classesByList[list] = sharing = [];
        }

        sharing.Add(@class);
        var begin = emulator.U64(list + LIST_PROPERTIES_BEGIN_OFFSET);
        var end = emulator.U64(list + LIST_PROPERTIES_END_OFFSET);
        for (var slot = begin; slot < end; slot += PROPERTY_LIST_ENTRY_SIZE) {
            var property = emulator.U64(slot);
            var name = emulator.CString(emulator.U64(property + PROPERTY_NAME_OFFSET));
            @class.Properties[name] = ReadExtendedProperty(property, name);
        }

        var methodsBegin = emulator.U64(list + LIST_METHODS_BEGIN_OFFSET);
        var methodsEnd = emulator.U64(list + LIST_METHODS_END_OFFSET);
        for (var slot = methodsBegin; slot < methodsEnd; slot += PROPERTY_LIST_ENTRY_SIZE) {
            @class.Methods.Add(ReadMethod(emulator.U64(slot)));
        }

        return @class;
    }

    private ExtendedProperty ReadExtendedProperty(ulong property, string name) {
        var note = emulator.CString(emulator.U64(property + PROPERTY_NOTE_OFFSET));
        var persistenceInfo = emulator.CString(emulator.U64(property + PROPERTY_PERSISTENCE_INFO_OFFSET));
        var hints = ParsePersistenceHints(persistenceInfo);
        var (_, dynamic) = ReadAccessor(emulator.U64(property + PROPERTY_ACCESSOR_OFFSET));
        var result = new ExtendedProperty {
            // The client falls back to the property name when no note was given, which says nothing.
            Note = note == name ? null : note,
            PersistenceInfo = persistenceInfo.Length == 0 ? null : persistenceInfo,
            PrimaryKey = hints.TryGetValue(PRIMARY_KEY_HINT, out var primaryKey) ? IsTruthy(primaryKey) : null,
            Nullable = hints.TryGetValue(NULLABLE_HINT, out var nullable) ? IsTruthy(nullable) : null,
            PersistenceDefault = hints.GetValueOrDefault(DEFAULT_VALUE_HINT),
            Count = dynamic == false ? ReadStaticCount(emulator.U64(property + PROPERTY_ACCESSOR_OFFSET)) : null,
        };

        // std::list of ints: a sentinel node whose next pointer walks the nodes until it comes back around.
        var sentinel = emulator.U64(property + PROPERTY_ALTERNATE_HASHES_OFFSET);
        var node = emulator.U64(sentinel + LIST_NODE_NEXT_OFFSET);
        for (var guard = 0; node != sentinel && guard < PROBE_ITERATION_LIMIT; guard++) {
            result.AlternateHashes.Add(emulator.U32(node + LIST_NODE_VALUE_OFFSET));
            node = emulator.U64(node + LIST_NODE_NEXT_OFFSET);
        }

        return result;
    }

    private uint? ReadStaticCount(ulong accessor) {
        var function = emulator.U64(emulator.U64(accessor) + ACCESSOR_COUNT_SLOT_OFFSET);
        return emulator.TryCall(function, [accessor, 0], [], out var count) ? (uint) count : null;
    }

    private ExtendedMethod ReadMethod(ulong method) {
        var delegateObject = emulator.U64(method + METHOD_DELEGATE_OFFSET);
        var result = new ExtendedMethod {
            Name = emulator.StdString(method + METHOD_NAME_OFFSET),
            Flags = emulator.I32(method + METHOD_FLAGS_OFFSET),
            Description = emulator.StdString(method + METHOD_DESCRIPTION_OFFSET),
            ReturnType = delegateObject == 0 ? null : TypeNameOf(CallDelegate(delegateObject, DELEGATE_RETURN_TYPE_SLOT_OFFSET)),
        };

        if (delegateObject == 0) {
            return result;
        }

        var vector = emulator.Allocate(POINTER_VECTOR_SIZE);
        emulator.WriteBytes(vector, new byte[POINTER_VECTOR_SIZE]);
        CallDelegate(delegateObject, DELEGATE_ARGUMENT_TYPES_SLOT_OFFSET, vector);
        var begin = emulator.U64(vector);
        var end = emulator.U64(vector + POINTER_SIZE);
        for (var slot = begin; begin != 0 && slot < end; slot += POINTER_SIZE) {
            result.Arguments.Add(TypeNameOf(emulator.U64(slot)) ?? string.Empty);
        }

        return result;
    }

    private ulong CallDelegate(ulong delegateObject, int slotOffset, params ulong[] extraArguments) {
        var function = emulator.U64(emulator.U64(delegateObject) + (ulong) slotOffset);
        return emulator.TryCall(function, [delegateObject, .. extraArguments], [], out var result) ? result : 0;
    }

    private string? TypeNameOf(ulong type)
        => type == 0 ? null : emulator.StdString(type + TYPE_NAME_OFFSET);

    private void ProbeDefaults() {
        var stringType = TypeAddresses.FirstOrDefault(type => emulator.StdString(type + TYPE_NAME_OFFSET) == STRING_TYPE_NAME);
        if (stringType == 0) {
            return;
        }

        emulator.InstructionBudget = PROBE_INSTRUCTION_BUDGET;
        var buffers = new[] { emulator.Allocate(PROBE_BUFFER_SIZE), emulator.Allocate(PROBE_BUFFER_SIZE) };
        foreach (var (list, sharing) in _classesByList) {
            // The client builds two instances, one per fill pattern, and only trusts a default that
            // both produce identically. It skips singletons, pointer types, and types that opt out.
            var type = emulator.U64(list + LIST_TYPE_OFFSET);
            if (type == 0 || emulator.U8(type + TYPE_POINTER_FLAG_OFFSET) != 0
                    || emulator.U8(list + LIST_SINGLETON_FLAG_OFFSET) != 0 || SkipsProbe(type)) {
                continue;
            }

            var baseOffset = (long) emulator.U64(list + LIST_BASE_OFFSET_OFFSET);
            var objects = new ulong[buffers.Length];
            var constructed = true;
            for (var i = 0; i < buffers.Length && constructed; i++) {
                emulator.WriteBytes(buffers[i], Enumerable.Repeat(PROBE_FILL_BYTES[i], PROBE_BUFFER_SIZE).ToArray());
                var construct = emulator.U64(emulator.U64(type) + TYPE_CONSTRUCT_SLOT_OFFSET);
                constructed = emulator.TryCall(construct, [type, buffers[i]], [], out _);
                objects[i] = (ulong) ((long) buffers[i] - baseOffset);
            }

            if (!constructed) {
                continue;
            }

            var begin = emulator.U64(list + LIST_PROPERTIES_BEGIN_OFFSET);
            var end = emulator.U64(list + LIST_PROPERTIES_END_OFFSET);
            for (var slot = begin; slot < end; slot += PROPERTY_LIST_ENTRY_SIZE) {
                var property = emulator.U64(slot);
                try {
                    var first = ConvertToText(objects[0], property, stringType);
                    if (first is not null && first == ConvertToText(objects[1], property, stringType)) {
                        var name = emulator.CString(emulator.U64(property + PROPERTY_NAME_OFFSET));
                        foreach (var @class in sharing) {
                            @class.Properties[name].Default = first;
                        }
                    }
                } catch (UnicornEngineException) {
                    // A property whose instance memory is not readable simply has no default.
                }
            }
        }
    }

    private bool SkipsProbe(ulong type)
        => !emulator.TryCall(emulator.U64(emulator.U64(type) + TYPE_SKIP_PROBE_SLOT_OFFSET), [type], [], out var result) || (result & 0xFF) != 0;

    private string? ConvertToText(ulong instance, ulong property, ulong stringType) {
        var propertyType = emulator.U64(property + PROPERTY_TYPE_OFFSET);

        // Leaf values only, and fixed-size containers: a dynamic container has no element to read yet.
        if (emulator.U64(propertyType + TYPE_PROPERTY_LIST_OFFSET) != 0) {
            return null;
        }

        var accessor = emulator.U64(property + PROPERTY_ACCESSOR_OFFSET);
        if (accessor == 0 || ReadAccessor(accessor).Dynamic != false) {
            return null;
        }

        var locate = emulator.U64(emulator.U64(accessor) + ACCESSOR_ELEMENT_SLOT_OFFSET);
        var address = (ulong) ((long) instance + emulator.I32(property + PROPERTY_OFFSET_OFFSET));
        if (!emulator.TryCall(locate, [accessor, address, 0], [], out var element)) {
            return null;
        }

        var convert = emulator.U64(emulator.U64(instance) + OBJECT_CONVERT_SLOT_OFFSET);
        var text = emulator.WriteStdString(string.Empty);

        // The client passes the source type, the property, and two zero flags on the stack.
        return emulator.TryCall(convert, [instance, text, stringType, element], [propertyType, property, 0, 0], out var ok) && (ok & 0xFF) != 0
            ? emulator.StdString(text)
            : null;
    }

    private static Dictionary<string, string> ParsePersistenceHints(string info) {
        var hints = new Dictionary<string, string>();
        foreach (var token in info.Split((char[]?) null, StringSplitOptions.RemoveEmptyEntries)) {
            var separator = token.IndexOf('=');
            if (separator > 0) {
                hints[token[..separator]] = token[(separator + 1)..];
            }
        }

        return hints;
    }

    private static bool IsTruthy(string value)
        => value is not ("0" or "false" or "False");

}
