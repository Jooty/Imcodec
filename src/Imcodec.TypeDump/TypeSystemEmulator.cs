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
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnicornEngine;
using UnicornEngine.Const;

namespace Imcodec.TypeDump;

internal sealed partial class TypeSystemEmulator : IDisposable {

    private const long TRAP_BASE = 0x1000_0000;
    private const long TRAP_REGION_SIZE = 0x10_0000;
    private const long TRAP_SLOT_SIZE = 16;
    private const long STOP_ADDRESS = TRAP_BASE + TRAP_REGION_SIZE - 0x10;
    private const long HEAP_BASE = 0x2000_0000;
    private const long HEAP_SIZE = 0x4000_0000;
    private const long STACK_BASE = 0x7000_0000;
    private const long STACK_SIZE = 0x100_0000;
    private const long STACK_TOP_MARGIN = 0x1000;
    private const long TEB_BASE = 0x7E00_0000;
    private const long TEB_SIZE = 0x1_0000;
    private const int TEB_SELF_POINTER_OFFSET = 0x30;
    private const int TEB_TLS_ARRAY_OFFSET = 0x58;
    private const int TLS_EXTRA_BYTES = 64;
    private const int HEAP_ALLOCATION_ALIGNMENT = 16;
    private const long DEFAULT_INSTRUCTION_BUDGET = 20_000_000;
    private const int VARIADIC_ARGUMENT_LIST_STACK_OFFSET = 0x30;
    private const int STD_STRING_SIZE_OFFSET = 16;
    private const int STD_STRING_CAPACITY_OFFSET = 24;
    private const int STD_STRING_SMALL_BUFFER_SIZE = 16;
    private const int MAX_RECORDED_UNMAPPED_ACCESSES = 20;
    private const int CSTRING_CHUNK_SIZE = 256;
    private const int ASSET_REQUEST_OUT_SIZE_ARGUMENT_STACK_OFFSET = 0x28;
    private const string RUNTIME_LIBRARY_FILE_NAME = "vcruntime140.dll";

    private static readonly HashSet<string> ALLOCATE_IMPORTS = ["malloc", "_malloc_base", "??2@YAPEAX_K@Z"];
    private static readonly HashSet<string> ZERO_ALLOCATE_IMPORTS = ["calloc", "_calloc_base"];
    private static readonly HashSet<string> REALLOCATE_IMPORTS = ["realloc", "_realloc_base"];
    private static readonly HashSet<string> FREE_IMPORTS = ["free", "_free_base", "_aligned_free"];
    private static readonly HashSet<string> FORMAT_IMPORTS = ["__stdio_common_vsprintf", "__stdio_common_vsprintf_s"];
    private static readonly Dictionary<string, Func<char, bool>> CHARACTER_CLASS_IMPORTS = new() {
        ["isspace"] = c => c is ' ' or '\t' or '\n' or '\v' or '\f' or '\r',
        ["isalpha"] = char.IsAsciiLetter,
        ["isalnum"] = char.IsAsciiLetterOrDigit,
        ["isdigit"] = char.IsAsciiDigit,
        ["isxdigit"] = char.IsAsciiHexDigit,
        ["isupper"] = char.IsAsciiLetterUpper,
        ["islower"] = char.IsAsciiLetterLower,
        ["ispunct"] = c => c is > ' ' and < (char) 127 && !char.IsAsciiLetterOrDigit(c),
        ["isprint"] = c => c is >= ' ' and < (char) 127,
        ["iscntrl"] = c => c < ' ' || c == 127,
    };
    private static readonly HashSet<string> SUCCEED_IMPORTS = [
        "_crt_atexit", "_register_onexit_function", "atexit", "_initialize_onexit_table",
        "Sleep", "EnterCriticalSection", "LeaveCriticalSection", "AcquireSRWLockExclusive",
        "ReleaseSRWLockExclusive", "WakeAllConditionVariable", "SleepConditionVariableSRW",
        "InitializeCriticalSectionAndSpinCount", "SetLastError",
    ];
    private static readonly HashSet<string> FATAL_IMPORTS = [
        "_invalid_parameter_noinfo_noreturn", "abort", "terminate", "?_Xlength_error@std@@YAXPEBD@Z", "_CxxThrowException",
    ];

    private readonly Unicorn _unicorn = new(Common.UC_ARCH_X86, Common.UC_MODE_64);
    private readonly Dictionary<long, string> _traps = [];
    private readonly Dictionary<long, Action<Unicorn>> _interceptors = [];
    private readonly CodeHook _trapHook;
    private readonly EventMemHook _unmappedHook;
    private long _nextTrap = TRAP_BASE;
    private long _heapTop = HEAP_BASE + 0x1000;
    private string? _fatalImport;

    public long InstructionBudget { get; set; } = DEFAULT_INSTRUCTION_BUDGET;

    public ClientImage Exe { get; }

    public ClientImage Runtime { get; }

    public Dictionary<string, int> UnknownImports { get; } = [];

    public Dictionary<ulong, string> Failures { get; } = [];

    public List<string> UnmappedAccesses { get; } = [];

    public Func<string, byte[]?>? AssetProvider { get; set; }

    public TypeSystemEmulator(ClientImage exe, ClientImage runtime) {
        Exe = exe;
        Runtime = runtime;
        _trapHook = OnTrap;
        _unmappedHook = OnUnmapped;

        _unicorn.MemMap(TRAP_BASE, TRAP_REGION_SIZE, Common.UC_PROT_ALL);
        _unicorn.MemWrite(TRAP_BASE, Enumerable.Repeat((byte) 0xC3, (int) TRAP_REGION_SIZE).ToArray());
        _unicorn.MemMap(HEAP_BASE, HEAP_SIZE, Common.UC_PROT_ALL);
        _unicorn.MemMap(STACK_BASE, STACK_SIZE, Common.UC_PROT_ALL);
        _unicorn.MemMap(TEB_BASE, TEB_SIZE, Common.UC_PROT_ALL);
        MapImage(exe);
        MapImage(runtime);
        BindImports(exe, preferRuntimeExports: true);
        BindImports(runtime, preferRuntimeExports: false);
        BuildThreadEnvironment();

        _unicorn.AddCodeHook(_trapHook, null, TRAP_BASE, TRAP_BASE + TRAP_REGION_SIZE - 1);
        _unicorn.AddEventMemHook(_unmappedHook, Common.UC_HOOK_MEM_UNMAPPED, null);
    }

    public static string FindRuntimeLibrary(string binDirectory)
        => Directory.EnumerateFiles(binDirectory)
            .FirstOrDefault(f => Path.GetFileName(f).Equals(RUNTIME_LIBRARY_FILE_NAME, StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException($"{RUNTIME_LIBRARY_FILE_NAME} was not found in {binDirectory}; the client's own runtime library is required.");

    public bool Run(ulong function, params ulong[] arguments)
        => Execute(function, arguments, out _);

    public ulong Call(ulong function, params ulong[] arguments) {
        if (!Execute(function, arguments, out var result)) {
            throw new InvalidOperationException($"Emulated call to {function:x} failed: {Failures.GetValueOrDefault(function)}");
        }

        return result;
    }

    public void InterceptAssetRequests(ulong requestFunction) {
        var slot = _nextTrap;
        _nextTrap += TRAP_SLOT_SIZE;
        _interceptors[slot] = ServeAssetRequest;
        byte[] jump = [0xFF, 0x25, 0, 0, 0, 0, .. BitConverter.GetBytes(slot)];
        WriteBytes(requestFunction, jump);
    }

    public ulong Allocate(long size) {
        var aligned = (size + HEAP_ALLOCATION_ALIGNMENT - 1) & ~(long) (HEAP_ALLOCATION_ALIGNMENT - 1);
        var address = (ulong) _heapTop;
        _heapTop += aligned + HEAP_ALLOCATION_ALIGNMENT;
        return address;
    }

    public long HeapBytesUsed
        => _heapTop - HEAP_BASE;

    public byte[] ReadBytes(ulong address, int length) {
        var buffer = new byte[length];
        _unicorn.MemRead((long) address, buffer);
        return buffer;
    }

    public void WriteBytes(ulong address, byte[] data)
        => _unicorn.MemWrite((long) address, data);

    public ulong U64(ulong address)
        => BinaryPrimitives.ReadUInt64LittleEndian(ReadBytes(address, 8));

    public uint U32(ulong address)
        => BinaryPrimitives.ReadUInt32LittleEndian(ReadBytes(address, 4));

    public int I32(ulong address)
        => BinaryPrimitives.ReadInt32LittleEndian(ReadBytes(address, 4));

    public byte U8(ulong address)
        => ReadBytes(address, 1)[0];

    public string CString(ulong address, int limit = 1 << 16) {
        var text = new List<byte>();
        while (text.Count < limit) {
            byte[] chunk;
            try {
                chunk = ReadBytes(address + (ulong) text.Count, CSTRING_CHUNK_SIZE);
            } catch (UnicornEngineException) {
                chunk = ReadBytes(address + (ulong) text.Count, 1);
            }

            var end = Array.IndexOf(chunk, (byte) 0);
            if (end >= 0) {
                text.AddRange(chunk.AsSpan(0, end).ToArray());
                break;
            }

            text.AddRange(chunk);
        }

        return Encoding.Latin1.GetString([.. text]);
    }

    public string StdString(ulong address) {
        var size = (int) U64(address + STD_STRING_SIZE_OFFSET);
        if (size == 0) {
            return string.Empty;
        }

        var capacity = U64(address + STD_STRING_CAPACITY_OFFSET);
        var data = capacity >= STD_STRING_SMALL_BUFFER_SIZE ? U64(address) : address;
        return Encoding.Latin1.GetString(ReadBytes(data, size));
    }

    public ulong WriteStdString(string text) {
        var bytes = Encoding.Latin1.GetBytes(text);
        var address = Allocate(32);
        var data = bytes.Length >= STD_STRING_SMALL_BUFFER_SIZE ? Allocate(bytes.Length + 1) : address;
        WriteBytes(data, [.. bytes, 0]);
        if (data != address) {
            WriteBytes(address, BitConverter.GetBytes(data));
        }

        WriteBytes(address + STD_STRING_SIZE_OFFSET, BitConverter.GetBytes((ulong) bytes.Length));
        WriteBytes(address + STD_STRING_CAPACITY_OFFSET, BitConverter.GetBytes((ulong) System.Math.Max(bytes.Length, STD_STRING_SMALL_BUFFER_SIZE - 1)));
        return address;
    }

    public void Dispose()
        => _unicorn.Dispose();

    private bool Execute(ulong function, ulong[] arguments, out ulong result) {
        ReadOnlySpan<int> argumentRegisters = [X86.UC_X86_REG_RCX, X86.UC_X86_REG_RDX, X86.UC_X86_REG_R8, X86.UC_X86_REG_R9];
        for (var i = 0; i < arguments.Length && i < argumentRegisters.Length; i++) {
            _unicorn.RegWrite(argumentRegisters[i], (long) arguments[i]);
        }

        var stackPointer = STACK_BASE + STACK_SIZE - STACK_TOP_MARGIN - 8;
        _unicorn.MemWrite(stackPointer, BitConverter.GetBytes(STOP_ADDRESS));
        _unicorn.RegWrite(X86.UC_X86_REG_RSP, stackPointer);
        _fatalImport = null;
        result = 0;
        try {
            _unicorn.EmuStart((long) function, STOP_ADDRESS, 0, InstructionBudget);
        } catch (UnicornEngineException exception) {
            Failures[function] = $"{exception.Message} at {_unicorn.RegRead(X86.UC_X86_REG_RIP):x}";
            return false;
        }

        var instructionPointer = _unicorn.RegRead(X86.UC_X86_REG_RIP);
        if (instructionPointer != STOP_ADDRESS) {
            Failures[function] = _fatalImport is null
                ? $"stopped at {instructionPointer:x}"
                : $"fatal import {_fatalImport} at {instructionPointer:x}";
            return false;
        }

        result = (ulong) _unicorn.RegRead(X86.UC_X86_REG_RAX);
        return true;
    }

    private void MapImage(ClientImage image) {
        _unicorn.MemMap((long) image.Base, (image.Memory.Length + 0xFFF) & ~0xFFFL, Common.UC_PROT_ALL);
        _unicorn.MemWrite((long) image.Base, image.Memory);
    }

    private void BindImports(ClientImage image, bool preferRuntimeExports) {
        foreach (var import in image.Imports) {
            long target;
            if (preferRuntimeExports
                    && import.Dll.Equals(RUNTIME_LIBRARY_FILE_NAME, StringComparison.OrdinalIgnoreCase)
                    && Runtime.Exports.TryGetValue(import.Name, out var exportRva)) {
                target = (long) Runtime.Base + exportRva;
            } else {
                target = _nextTrap;
                _nextTrap += TRAP_SLOT_SIZE;
                _traps[target] = import.Name;
            }

            _unicorn.MemWrite((long) image.Base + import.SlotRva, BitConverter.GetBytes(target));
        }
    }

    private void BuildThreadEnvironment() {
        var tls = Exe.Tls;
        var tlsArray = Allocate(64);
        if (tls is { } template) {
            var templateSize = (int) (template.EndAddress - template.StartAddress);
            var block = Allocate(templateSize + template.ZeroFillSize + TLS_EXTRA_BYTES);
            WriteBytes(block, ReadBytes(template.StartAddress, templateSize));
            WriteBytes(tlsArray, BitConverter.GetBytes(block));
            WriteBytes(template.IndexAddress, BitConverter.GetBytes(0u));
        }

        _unicorn.MemWrite(TEB_BASE + TEB_SELF_POINTER_OFFSET, BitConverter.GetBytes(TEB_BASE));
        _unicorn.MemWrite(TEB_BASE + TEB_TLS_ARRAY_OFFSET, BitConverter.GetBytes(tlsArray));
        _unicorn.RegWrite(X86.UC_X86_REG_GS_BASE, TEB_BASE);
    }

    private bool OnUnmapped(Unicorn unicorn, int type, long address, int size, long value, object? user) {
        if (UnmappedAccesses.Count < MAX_RECORDED_UNMAPPED_ACCESSES) {
            UnmappedAccesses.Add($"{address:x} at {unicorn.RegRead(X86.UC_X86_REG_RIP):x}");
        }

        return false;
    }

    private void ServeAssetRequest(Unicorn unicorn) {
        var name = StdString((ulong) unicorn.RegRead(X86.UC_X86_REG_RDX));
        var outData = (ulong) unicorn.RegRead(X86.UC_X86_REG_R8);
        var outSize = (ulong) unicorn.RegRead(X86.UC_X86_REG_R9);
        var stackPointer = (ulong) unicorn.RegRead(X86.UC_X86_REG_RSP);
        var contents = AssetProvider?.Invoke(name);
        if (contents is null) {
            unicorn.RegWrite(X86.UC_X86_REG_RAX, 0);
            return;
        }

        var buffer = Allocate(contents.Length + 1);
        WriteBytes(buffer, [.. contents, 0]);
        WriteBytes(outData, BitConverter.GetBytes(buffer));
        WriteBytes(outData + 8, BitConverter.GetBytes(0UL));
        WriteBytes(outSize, BitConverter.GetBytes(contents.Length));
        WriteBytes(U64(stackPointer + ASSET_REQUEST_OUT_SIZE_ARGUMENT_STACK_OFFSET), BitConverter.GetBytes(0UL));
        unicorn.RegWrite(X86.UC_X86_REG_RAX, 1);
    }

    private void OnTrap(Unicorn unicorn, long address, int size, object? user) {
        if (_interceptors.TryGetValue(address, out var interceptor)) {
            interceptor(unicorn);
            return;
        }

        if (!_traps.TryGetValue(address, out var name)) {
            return;
        }

        var rcx = (ulong) unicorn.RegRead(X86.UC_X86_REG_RCX);
        var rdx = (ulong) unicorn.RegRead(X86.UC_X86_REG_RDX);
        var r8 = (ulong) unicorn.RegRead(X86.UC_X86_REG_R8);
        var r9 = (ulong) unicorn.RegRead(X86.UC_X86_REG_R9);
        if (ALLOCATE_IMPORTS.Contains(name)) {
            SetResult(Allocate((long) rcx));
        } else if (ZERO_ALLOCATE_IMPORTS.Contains(name)) {
            SetResult(Allocate((long) (rcx * rdx)));
        } else if (name == "_aligned_malloc") {
            SetResult((Allocate((long) (rcx + rdx)) + rdx - 1) & ~(rdx - 1));
        } else if (REALLOCATE_IMPORTS.Contains(name)) {
            var block = Allocate((long) rdx);
            if (rcx != 0) {
                WriteBytes(block, ReadBytes(rcx, (int) rdx));
            }

            SetResult(block);
        } else if (FREE_IMPORTS.Contains(name)) {
            SetResult(0);
        } else if (CHARACTER_CLASS_IMPORTS.TryGetValue(name, out var isClass)) {
            SetResult(rcx < 256 && isClass((char) rcx) ? 1UL : 0UL);
        } else if (name == "tolower") {
            SetResult(rcx < 256 ? char.ToLowerInvariant((char) rcx) : rcx);
        } else if (name == "toupper") {
            SetResult(rcx < 256 ? char.ToUpperInvariant((char) rcx) : rcx);
        } else if (name is "strcmp" or "_stricmp") {
            var comparison = name == "strcmp" ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            SetResult((ulong) (long) string.Compare(CString(rcx), CString(rdx), comparison));
        } else if (name == "strlen") {
            SetResult((ulong) CString(rcx).Length);
        } else if (name == "strcpy_s") {
            WriteBytes(rcx, [.. Encoding.Latin1.GetBytes(CString(r8)), 0]);
            SetResult(0);
        } else if (name == "strncmp") {
            var left = CString(rcx);
            var right = CString(rdx);
            SetResult(left.Length >= (int) r8 && right.Length >= (int) r8
                ? (ulong) string.CompareOrdinal(left[..(int) r8], right[..(int) r8])
                : (ulong) string.CompareOrdinal(left, right));
        } else if (name == "InterlockedPushEntrySList") {
            var previous = U64(rcx);
            WriteBytes(rdx, BitConverter.GetBytes(previous));
            WriteBytes(rcx, BitConverter.GetBytes(rdx));
            SetResult(previous);
        } else if (name == "atoi") {
            SetResult((ulong) ParseLeadingInteger(CString(rcx)));
        } else if (FORMAT_IMPORTS.Contains(name)) {
            var stackPointer = (ulong) unicorn.RegRead(X86.UC_X86_REG_RSP);
            var text = FormatC(CString(r9), U64(stackPointer + VARIADIC_ARGUMENT_LIST_STACK_OFFSET));
            var data = Encoding.Latin1.GetBytes(text);
            var written = data.Take((int) System.Math.Max((long) r8 - 1, 0)).ToArray();
            if (rdx != 0) {
                WriteBytes(rdx, [.. written, 0]);
            }

            SetResult((ulong) text.Length);
        } else if (SUCCEED_IMPORTS.Contains(name)) {
            SetResult(name.StartsWith('_') || name == "atexit" ? 0UL : 1UL);
        } else if (FATAL_IMPORTS.Contains(name)) {
            _fatalImport = name;
            unicorn.EmuStop();
        } else {
            UnknownImports[name] = UnknownImports.GetValueOrDefault(name) + 1;
            SetResult(0);
        }

        void SetResult(ulong value)
            => unicorn.RegWrite(X86.UC_X86_REG_RAX, (long) value);
    }

    private string FormatC(string format, ulong argumentList) {
        var output = new StringBuilder();
        var position = 0;
        var slot = argumentList;
        foreach (var match in FormatSpecifier().Matches(format).Cast<Match>()) {
            output.Append(format, position, match.Index - position);
            position = match.Index + match.Length;
            var conversion = match.Groups[5].Value[0];
            if (conversion == '%') {
                output.Append('%');
                continue;
            }

            var value = U64(slot);
            slot += 8;
            var wide = match.Groups[4].Value is "ll" or "I64" or "z";
            switch (conversion) {
                case 'd' or 'i':
                    output.Append(wide ? (long) value : (int) value);
                    break;
                case 'u':
                    output.Append(wide ? value : (uint) value);
                    break;
                case 'x':
                    output.Append((wide ? value : (uint) value).ToString("x", CultureInfo.InvariantCulture));
                    break;
                case 'X':
                    output.Append((wide ? value : (uint) value).ToString("X", CultureInfo.InvariantCulture));
                    break;
                case 's':
                    output.Append(CString(value));
                    break;
                case 'c':
                    output.Append((char) (value & 0xFF));
                    break;
                default:
                    output.Append(BitConverter.UInt64BitsToDouble(value).ToString(CultureInfo.InvariantCulture));
                    break;
            }
        }

        output.Append(format, position, format.Length - position);
        return output.ToString();
    }

    private static int ParseLeadingInteger(string text) {
        var trimmed = text.Trim();
        var length = 0;
        while (length < trimmed.Length && (char.IsAsciiDigit(trimmed[length]) || (length == 0 && trimmed[length] is '+' or '-'))) {
            length++;
        }

        return int.TryParse(trimmed.AsSpan(0, length), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    [GeneratedRegex(@"%([-+ #0]*)(\d*)(?:\.(\d+))?(hh|h|ll|l|I64|z)?([diuxXsfgc%])")]
    private static partial Regex FormatSpecifier();

}
