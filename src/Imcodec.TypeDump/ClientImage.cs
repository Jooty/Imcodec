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
using System.Reflection.PortableExecutable;
using System.Text;

namespace Imcodec.TypeDump;

internal sealed class ClientImage {

    private const int RUNTIME_FUNCTION_SIZE = 12;
    private const int IMPORT_DESCRIPTOR_SIZE = 20;
    private const byte UNWIND_FLAG_CHAIN_INFO = 4;
    private const ulong IMPORT_ORDINAL_FLAG = 1UL << 63;

    private readonly uint[] _functionBegins;
    private readonly uint[] _functionEnds;
    private readonly uint[] _functionUnwinds;

    public ulong Base { get; }

    public byte[] Memory { get; }

    public uint EntryPointRva { get; }

    public IReadOnlyList<ImportSlot> Imports { get; }

    public Dictionary<string, uint> Exports { get; }

    public TlsTemplate? Tls { get; }

    public IReadOnlyList<SectionRange> Sections { get; }

    public readonly record struct ImportSlot(string Dll, string Name, uint SlotRva);

    public readonly record struct SectionRange(string Name, uint Rva, uint Size, bool Executable);

    public readonly record struct TlsTemplate(ulong StartAddress, ulong EndAddress, ulong IndexAddress, uint ZeroFillSize);

    private ClientImage(byte[] memory, PEHeaders headers) {
        Memory = memory;
        Base = headers.PEHeader!.ImageBase;
        EntryPointRva = (uint) headers.PEHeader.AddressOfEntryPoint;
        Sections = [.. headers.SectionHeaders.Select(s => new SectionRange(
            s.Name, (uint) s.VirtualAddress, (uint) s.VirtualSize,
            (s.SectionCharacteristics & System.Reflection.PortableExecutable.SectionCharacteristics.MemExecute) != 0))];
        Imports = ReadImports((uint) headers.PEHeader.ImportTableDirectory.RelativeVirtualAddress);
        Exports = ReadExports((uint) headers.PEHeader.ExportTableDirectory.RelativeVirtualAddress);
        Tls = ReadTls((uint) headers.PEHeader.ThreadLocalStorageTableDirectory.RelativeVirtualAddress);
        (_functionBegins, _functionEnds, _functionUnwinds) = ReadFunctionTable(
            (uint) headers.PEHeader.ExceptionTableDirectory.RelativeVirtualAddress,
            (uint) headers.PEHeader.ExceptionTableDirectory.Size);
    }

    public static ClientImage Load(string path) {
        var file = File.ReadAllBytes(path);
        using var reader = new PEReader(new MemoryStream(file));
        var headers = reader.PEHeaders;
        var optional = headers.PEHeader
            ?? throw new InvalidDataException($"{path} is not a PE image.");
        var memory = new byte[optional.SizeOfImage];
        Array.Copy(file, memory, System.Math.Min(optional.SizeOfHeaders, file.Length));
        foreach (var section in headers.SectionHeaders) {
            var length = System.Math.Min(section.SizeOfRawData, section.VirtualSize == 0 ? section.SizeOfRawData : section.VirtualSize);
            if (section.PointerToRawData + length > file.Length) {
                length = System.Math.Max(0, file.Length - section.PointerToRawData);
            }

            Array.Copy(file, section.PointerToRawData, memory, section.VirtualAddress, length);
        }

        return new ClientImage(memory, headers);
    }

    public uint U32(uint rva)
        => BinaryPrimitives.ReadUInt32LittleEndian(Memory.AsSpan((int) rva));

    public ulong U64(uint rva)
        => BinaryPrimitives.ReadUInt64LittleEndian(Memory.AsSpan((int) rva));

    public string CString(uint rva) {
        var end = Array.IndexOf(Memory, (byte) 0, (int) rva);
        return Encoding.Latin1.GetString(Memory, (int) rva, end - (int) rva);
    }

    public bool TryFindFunction(uint rva, out uint begin, out uint end) {
        begin = end = 0;
        var index = Array.BinarySearch(_functionBegins, rva);
        if (index < 0) {
            index = ~index - 1;
        }

        if (index < 0 || rva >= _functionEnds[index]) {
            return false;
        }

        for (var hops = 0; hops < 16; hops++) {
            var unwind = _functionUnwinds[index];
            if (unwind == 0 || (Memory[unwind] >> 3 & UNWIND_FLAG_CHAIN_INFO) == 0) {
                break;
            }

            var codeCount = Memory[unwind + 2];
            var chained = unwind + 4 + (uint) (((codeCount + 1) & ~1) * 2);
            var parent = Array.BinarySearch(_functionBegins, U32(chained));
            if (parent < 0) {
                break;
            }

            index = parent;
        }

        begin = _functionBegins[index];
        end = _functionEnds[index];
        return true;
    }

    private List<ImportSlot> ReadImports(uint directoryRva) {
        var imports = new List<ImportSlot>();
        if (directoryRva == 0) {
            return imports;
        }

        for (var descriptor = directoryRva; U32(descriptor + 12) != 0; descriptor += IMPORT_DESCRIPTOR_SIZE) {
            var dll = CString(U32(descriptor + 12));
            var nameThunks = U32(descriptor) != 0 ? U32(descriptor) : U32(descriptor + 16);
            var slots = U32(descriptor + 16);
            for (uint i = 0; U64(nameThunks + i * 8) != 0; i++) {
                var thunk = U64(nameThunks + i * 8);
                var name = (thunk & IMPORT_ORDINAL_FLAG) != 0
                    ? $"ord{thunk & 0xFFFF}"
                    : CString((uint) thunk + 2);
                imports.Add(new ImportSlot(dll, name, slots + i * 8));
            }
        }

        return imports;
    }

    private Dictionary<string, uint> ReadExports(uint directoryRva) {
        var exports = new Dictionary<string, uint>();
        if (directoryRva == 0) {
            return exports;
        }

        var nameCount = U32(directoryRva + 24);
        var functions = U32(directoryRva + 28);
        var names = U32(directoryRva + 32);
        var ordinals = U32(directoryRva + 36);
        for (uint i = 0; i < nameCount; i++) {
            var ordinal = BinaryPrimitives.ReadUInt16LittleEndian(Memory.AsSpan((int) (ordinals + i * 2)));
            exports[CString(U32(names + i * 4))] = U32(functions + ordinal * 4u);
        }

        return exports;
    }

    private TlsTemplate? ReadTls(uint directoryRva)
        => directoryRva == 0
            ? null
            : new TlsTemplate(U64(directoryRva), U64(directoryRva + 8), U64(directoryRva + 16), U32(directoryRva + 32));

    private (uint[] Begins, uint[] Ends, uint[] Unwinds) ReadFunctionTable(uint directoryRva, uint size) {
        var count = (int) (size / RUNTIME_FUNCTION_SIZE);
        var entries = new (uint Begin, uint End, uint Unwind)[count];
        for (var i = 0; i < count; i++) {
            var rva = directoryRva + (uint) (i * RUNTIME_FUNCTION_SIZE);
            entries[i] = (U32(rva), U32(rva + 4), U32(rva + 8));
        }

        Array.Sort(entries, (a, b) => a.Begin.CompareTo(b.Begin));
        return (
            [.. entries.Select(e => e.Begin)],
            [.. entries.Select(e => e.End)],
            [.. entries.Select(e => e.Unwind)]);
    }

}
