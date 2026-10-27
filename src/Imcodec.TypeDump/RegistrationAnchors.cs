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

using System.Buffers.Binary;
using Iced.Intel;

namespace Imcodec.TypeDump;

/// <summary>
/// Locates, by scanning the executable's code and tables rather than by any fixed address, the few
/// entry points the emulator needs: the static initializer table, the per-type getter stubs, the
/// type-name registry getter, and the initializer that fills runtime enum options. Scanning instead
/// of hard-coding addresses keeps the dumper working across client patches.
/// </summary>
internal sealed class RegistrationAnchors {

    private const string STATIC_INITIALIZER_TABLE_IMPORT = "_initterm";
    private const int ENTRY_POINT_DECODE_LIMIT = 64;
    private const int MAIN_FUNCTION_FALLBACK_LENGTH = 0x4000;
    private const int INITIALIZER_TABLE_ENTRY_SIZE = 8;
    private const int INITIALIZER_STUB_LENGTH = 18;
    private const int INITIALIZER_STUB_CALL_END_OFFSET = 9;
    private const int INITIALIZER_STUB_JUMP_END_OFFSET = 18;
    private const int NEAR_CALL_LENGTH = 5;
    private const byte NEAR_CALL_OPCODE = 0xE8;
    private const double COMMON_CALLEE_SHARE = 0.95;
    private const int REGISTRY_ANCHOR_LENGTH = 20;
    private const int JUMP_THROUGH_IMPORT_LENGTH = 6;
    private const int MINIMUM_REGISTRY_ANCHOR_HITS = 1;
    private const int RECENT_INSTRUCTION_WINDOW = 16;
    private const int RIP_RELATIVE_LEA_LENGTH = 7;
    private const string RACE_INITIALIZER_LOGGED_FUNCTION_NAME = "RaceManager::InitializeRaces";

    private static readonly byte[] INITIALIZER_STUB_PREFIX = [0x48, 0x83, 0xEC, 0x28, 0xE8];
    private static readonly byte[] INITIALIZER_STUB_MIDDLE = [0x48, 0x83, 0xC4, 0x28, 0xE9];
    private static readonly byte[] REGISTRY_ANCHOR_TAIL = [0x48, 0x3B, 0x38, 0x74, 0x12];

    private readonly ClientImage _exe;

    public (ulong Start, ulong End) InitializerTable { get; private set; }

    public int InitializerSlotCount { get; private set; }

    public int StubCount { get; private set; }

    public ulong StubSharedCallee { get; private set; }

    public ulong[] StubTargets { get; private set; } = [];

    public ulong PropertyListInitialize { get; private set; }

    public ulong[] AllGetters { get; private set; } = [];

    public ulong NameRegistryGetter { get; private set; }

    public ulong? RuntimeEnumInitializer { get; private set; }

    public ulong? AssetRequestFunction { get; private set; }

    private RegistrationAnchors(ClientImage exe)
        => _exe = exe;

    public static RegistrationAnchors Find(ClientImage exe) {
        var anchors = new RegistrationAnchors(exe);
        anchors.FindInitializerTables();
        anchors.FindStubsAndInitialize();
        anchors.FindNameRegistryGetter();
        anchors.FindRuntimeEnumInitializer();
        return anchors;
    }

    public IEnumerable<ulong> ReadTable((ulong Start, ulong End) table) {
        for (var slot = table.Start; slot < table.End; slot += INITIALIZER_TABLE_ENTRY_SIZE) {
            var entry = _exe.U64((uint) (slot - _exe.Base));
            if (entry != 0) {
                yield return entry;
            }
        }
    }

    /// <summary>
    /// Returns every near-call site and its target in a code range. This is a raw opcode scan, so it
    /// can match a 0xE8 byte that is really operand data; callers lean on frequency to drown out the noise.
    /// </summary>
    public List<(uint Offset, uint Target)> NearCalls(uint beginRva, uint endRva) {
        var calls = new List<(uint Offset, uint Target)>();
        var memory = _exe.Memory.AsSpan();
        var offset = (int) beginRva;
        while (offset < endRva) {
            var found = memory[offset..(int) endRva].IndexOf(NEAR_CALL_OPCODE);
            if (found < 0 || offset + found + NEAR_CALL_LENGTH > memory.Length) {
                break;
            }

            var site = offset + found;
            var relative = BinaryPrimitives.ReadInt32LittleEndian(memory[(site + 1)..]);
            calls.Add(((uint) site, (uint) (site + NEAR_CALL_LENGTH + relative)));
            offset = site + 1;
        }

        return calls;
    }

    private void FindInitializerTables() {
        var thunkImports = _exe.Imports
            .Where(i => i.Name == STATIC_INITIALIZER_TABLE_IMPORT)
            .ToDictionary(i => i.SlotRva, i => i.Name);
        var mainRva = FollowEntryPointJump();
        var endRva = _exe.TryFindFunction(mainRva, out _, out var functionEnd)
            ? functionEnd
            : mainRva + MAIN_FUNCTION_FALLBACK_LENGTH;

        // Startup calls _initterm(begin, end) to run the static initializers. Walk the startup function
        // tracking the addresses the last lea loads into rcx and rdx, and grab them as the table bounds
        // at the point execution reaches the call through the _initterm import thunk.
        ulong rcx = 0, rdx = 0;
        foreach (var instruction in Decode(mainRva, endRva)) {
            if (instruction.Code == Code.Lea_r64_m && instruction.MemoryBase == Register.RIP) {
                if (instruction.Op0Register == Register.RCX) {
                    rcx = instruction.IPRelativeMemoryAddress;
                } else if (instruction.Op0Register == Register.RDX) {
                    rdx = instruction.IPRelativeMemoryAddress;
                }
            } else if (instruction.FlowControl == FlowControl.Call && instruction.Op0Kind == OpKind.NearBranch64) {
                var slot = ImportSlotBehindThunk((uint) (instruction.NearBranchTarget - _exe.Base));
                if (slot is { } importSlot && thunkImports.ContainsKey(importSlot)) {
                    InitializerTable = (rcx, rdx);
                }
            }
        }

        if (InitializerTable.Start == 0 || InitializerTable.End <= InitializerTable.Start) {
            throw new InvalidOperationException("The static initializer table was not found from the entry point.");
        }
    }

    private uint FollowEntryPointJump() {
        var rva = _exe.EntryPointRva;
        foreach (var instruction in Decode(rva, rva + ENTRY_POINT_DECODE_LIMIT * 8).Take(ENTRY_POINT_DECODE_LIMIT)) {
            // The PE entry point is a short thunk that jumps to the real startup function; follow it.
            if (instruction.FlowControl == FlowControl.UnconditionalBranch && instruction.Op0Kind == OpKind.NearBranch64) {
                return (uint) (instruction.NearBranchTarget - _exe.Base);
            }
        }

        return rva;
    }

    private uint? ImportSlotBehindThunk(uint thunkRva) {
        // An import thunk is `jmp [rip+disp]` (FF 25); resolve the rip-relative displacement to the
        // import slot it reads, or give up if the bytes are not that thunk.
        if (thunkRva + JUMP_THROUGH_IMPORT_LENGTH > _exe.Memory.Length
                || _exe.Memory[thunkRva] != 0xFF || _exe.Memory[thunkRva + 1] != 0x25) {
            return null;
        }

        return (uint) (thunkRva + JUMP_THROUGH_IMPORT_LENGTH + (int) _exe.U32(thunkRva + 2));
    }

    private void FindStubsAndInitialize() {
        var entries = ReadTable(InitializerTable).ToList();
        InitializerSlotCount = entries.Count;
        // A getter stub has the fixed shape `sub rsp,28 / call <shared> / add rsp,28 / jmp <getter>`.
        // Match it by those bytes, then read the shared callee it calls and the getter it tails into.
        var sharedCallees = new Dictionary<ulong, int>();
        var targets = new HashSet<ulong>();
        foreach (var entry in entries) {
            var rva = (int) (entry - _exe.Base);
            var code = _exe.Memory.AsSpan(rva, INITIALIZER_STUB_LENGTH);
            if (!code.StartsWith(INITIALIZER_STUB_PREFIX)
                    || !code[(INITIALIZER_STUB_PREFIX.Length + 4)..].StartsWith(INITIALIZER_STUB_MIDDLE)) {
                continue;
            }

            StubCount++;
            var callee = _exe.Base + (ulong) (rva + INITIALIZER_STUB_CALL_END_OFFSET + BinaryPrimitives.ReadInt32LittleEndian(code[INITIALIZER_STUB_PREFIX.Length..]));
            sharedCallees[callee] = sharedCallees.GetValueOrDefault(callee) + 1;
            targets.Add(_exe.Base + (ulong) (rva + INITIALIZER_STUB_JUMP_END_OFFSET + BinaryPrimitives.ReadInt32LittleEndian(code[(INITIALIZER_STUB_PREFIX.Length + 4 + INITIALIZER_STUB_MIDDLE.Length)..])));
        }

        if (targets.Count == 0) {
            throw new InvalidOperationException("No getter stubs were found in the static initializer table.");
        }

        StubSharedCallee = sharedCallees.MaxBy(p => p.Value).Key;
        StubTargets = [.. targets.Order()];

        // Nearly every stub target calls the same handful of helpers. Of those, the one reached from the
        // fewest call sites across the image is the property-list initializer, because only real getters
        // call it; its call sites are then the getters the stubs did not already point us at.
        var calleeFrequency = new Dictionary<uint, int>();
        foreach (var target in StubTargets) {
            var targetRva = (uint) (target - _exe.Base);
            if (!_exe.TryFindFunction(targetRva, out var begin, out var end)) {
                continue;
            }

            foreach (var callee in NearCalls(begin, end).Select(c => c.Target).Distinct()) {
                calleeFrequency[callee] = calleeFrequency.GetValueOrDefault(callee) + 1;
            }
        }

        var candidates = calleeFrequency
            .Where(p => p.Value >= StubTargets.Length * COMMON_CALLEE_SHARE)
            .Select(p => p.Key)
            .ToHashSet();
        var callSites = candidates.ToDictionary(c => c, _ => new List<uint>());
        foreach (var section in _exe.Sections.Where(s => s.Executable)) {
            foreach (var (site, target) in NearCalls(section.Rva, section.Rva + section.Size)) {
                if (callSites.TryGetValue(target, out var sites)) {
                    sites.Add(site);
                }
            }
        }

        var initialize = callSites.MinBy(p => p.Value.Count);
        PropertyListInitialize = _exe.Base + initialize.Key;
        var getters = new HashSet<ulong>(StubTargets);
        foreach (var site in initialize.Value) {
            if (_exe.TryFindFunction(site, out var begin, out _)) {
                getters.Add(_exe.Base + begin);
            }
        }

        AllGetters = [.. getters.Order()];
    }

    private void FindNameRegistryGetter() {
        // The call that fetches the type-name registry is followed by a recognisable signature: a
        // `mov rbp, imm64` (48 BD) and, ten bytes on, a fixed compare-and-branch tail. Count the calls
        // whose return site carries it; the most frequent target is the getter.
        var hits = new Dictionary<uint, int>();
        foreach (var section in _exe.Sections.Where(s => s.Executable)) {
            foreach (var (site, target) in NearCalls(section.Rva, section.Rva + section.Size)) {
                var anchor = site + NEAR_CALL_LENGTH;
                if (anchor + REGISTRY_ANCHOR_LENGTH > _exe.Memory.Length) {
                    continue;
                }

                var code = _exe.Memory.AsSpan((int) anchor, REGISTRY_ANCHOR_LENGTH);
                if (code[0] == 0x48 && code[1] == 0xBD && code[10..].StartsWith(REGISTRY_ANCHOR_TAIL)) {
                    hits[target] = hits.GetValueOrDefault(target) + 1;
                }
            }
        }

        if (hits.Count < MINIMUM_REGISTRY_ANCHOR_HITS) {
            throw new InvalidOperationException("The type name registry anchor was not found.");
        }

        NameRegistryGetter = _exe.Base + hits.MaxBy(p => p.Value).Key;
    }

    private void FindRuntimeEnumInitializer() {
        // The initializer that fills runtime enum options logs its own symbol name on entry. Find that
        // string literal, then the functions that load its address with a rip-relative lea; the one we
        // want is whichever of them also calls the asset loader (FindAssetRequestCallee).
        var needle = System.Text.Encoding.ASCII.GetBytes(RACE_INITIALIZER_LOGGED_FUNCTION_NAME + "\0");
        var literals = new HashSet<uint>();
        var memory = _exe.Memory.AsSpan();
        for (var offset = 0; offset < memory.Length;) {
            var found = memory[offset..].IndexOf(needle);
            if (found < 0) {
                break;
            }

            literals.Add((uint) (offset + found));
            offset += found + needle.Length;
        }

        var functions = new HashSet<uint>();
        foreach (var section in _exe.Sections.Where(s => s.Executable)) {
            var code = memory.Slice((int) section.Rva, (int) section.Size);
            for (var offset = 0; offset + RIP_RELATIVE_LEA_LENGTH <= code.Length;) {
                var found = code[(offset + 1)..].IndexOf((byte) 0x8D);
                if (found < 0) {
                    break;
                }

                var site = offset + found;
                offset = site + 1;
                // Keep only a real rip-relative lea: REX.W or REX.R prefix (48/4C) and a modrm whose
                // mod/rm bits select [rip+disp32] (the low bits are 05).
                if (code[site] is not (0x48 or 0x4C) || (code[site + 2] & 0xC7) != 0x05) {
                    continue;
                }

                var target = section.Rva + site + RIP_RELATIVE_LEA_LENGTH + BinaryPrimitives.ReadInt32LittleEndian(code[(site + 3)..]);
                if (literals.Contains((uint) target) && _exe.TryFindFunction(section.Rva + (uint) site, out var begin, out _)) {
                    functions.Add(begin);
                }
            }
        }

        foreach (var function in functions.Order()) {
            if (FindAssetRequestCallee(function) is { } callee) {
                RuntimeEnumInitializer = _exe.Base + function;
                AssetRequestFunction = _exe.Base + callee;
                return;
            }
        }
    }

    private uint? FindAssetRequestCallee(uint functionRva) {
        if (!_exe.TryFindFunction(functionRva, out var begin, out var end)) {
            return null;
        }

        // The asset loader is called with the data-file name moved into rcx from a rip-relative global,
        // so return the first call that a recent `mov rcx, [rip+...]` sets up.
        var recent = new Queue<Instruction>();
        foreach (var instruction in Decode(begin, end)) {
            if (instruction.FlowControl == FlowControl.Call && instruction.Op0Kind == OpKind.NearBranch64
                    && recent.Any(i => i.Code == Code.Mov_r64_rm64 && i.Op0Register == Register.RCX && i.MemoryBase == Register.RIP)) {
                return (uint) (instruction.NearBranchTarget - _exe.Base);
            }

            recent.Enqueue(instruction);
            if (recent.Count > RECENT_INSTRUCTION_WINDOW) {
                recent.Dequeue();
            }
        }

        return null;
    }

    private IEnumerable<Instruction> Decode(uint beginRva, uint endRva) {
        var reader = new ByteArrayCodeReader(_exe.Memory, (int) beginRva, (int) (endRva - beginRva));
        var decoder = Decoder.Create(64, reader);
        decoder.IP = _exe.Base + beginRva;
        while (reader.CanReadByte) {
            decoder.Decode(out var instruction);
            if (instruction.IsInvalid) {
                yield break;
            }

            yield return instruction;
        }
    }

}
