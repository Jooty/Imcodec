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

using System.Text.Encodings.Web;
using System.Text.Json;
using Imcodec.ObjectProperty.CodeGen.JSON;
using Imcodec.Wad;

namespace Imcodec.TypeDump;

/// <summary>
/// Extracts the client's property-class type system straight from its executable: it emulates the
/// startup code that registers every type, then walks the registry left behind in emulated memory
/// and writes it out as the JSON manifest the code generator consumes.
/// </summary>
public static class TypeDumper {

    private const int JSON_INDENT_SIZE = 2;
    private const long RUNTIME_ENUM_INSTRUCTION_BUDGET = 4_000_000_000;
    private const string ARCHIVE_FILE_PATTERN = "*.wad";
    private const string BINARY_DIRECTORY_NAME = "Bin";
    private const string EXECUTABLE_FILE_PATTERN = "*.exe";

    /// <summary>
    /// Produces the type manifest for the client installed at <paramref name="clientDirectory"/>.
    /// Progress and any emulation failures are reported to <paramref name="log"/>.
    /// </summary>
    public static JsonDumpManifest Dump(string clientDirectory, TextWriter? log = null) {
        var binDirectory = Path.Combine(clientDirectory, BINARY_DIRECTORY_NAME);
        if (!Directory.Exists(binDirectory)) {
            binDirectory = clientDirectory;
        }

        // The game client is the largest executable in the folder; smaller helpers sit beside it.
        var executable = new DirectoryInfo(binDirectory).EnumerateFiles(EXECUTABLE_FILE_PATTERN)
            .OrderByDescending(f => f.Length)
            .FirstOrDefault()
            ?? throw new FileNotFoundException($"No executable was found in {binDirectory}.");
        var exe = ClientImage.Load(executable.FullName);
        var runtime = ClientImage.Load(TypeSystemEmulator.FindRuntimeLibrary(binDirectory));
        var anchors = RegistrationAnchors.Find(exe);
        log?.WriteLine($"initializer slots {anchors.InitializerSlotCount}, getter stubs {anchors.StubCount}, " +
            $"stub targets {anchors.StubTargets.Length}, getters {anchors.AllGetters.Length}");

        // The static initializers register every type and its properties; the getters that follow
        // fill in the per-type detail the initializers leave for first access.
        using var emulator = new TypeSystemEmulator(exe, runtime);
        var started = DateTime.UtcNow;
        foreach (var function in anchors.ReadTable(anchors.InitializerTable)) {
            emulator.Run(function);
        }

        foreach (var getter in anchors.AllGetters) {
            emulator.Run(getter);
        }

        log?.WriteLine($"registration emulated in {(DateTime.UtcNow - started).TotalSeconds:F1}s, " +
            $"{emulator.Failures.Count} failed calls, {emulator.HeapBytesUsed >> 20} MiB heap");
        foreach (var (function, reason) in emulator.Failures.Take(20)) {
            log?.WriteLine($"  failed {function:x}: {reason}");
        }

        // One initializer loads its enum options from a data file instead of hard-coding them, so it
        // needs the client's archives. Bridge its asset requests to the real files on disk and give it
        // a far larger instruction budget, since it parses a whole document rather than a few structs.
        if (anchors.RuntimeEnumInitializer is { } initializer && anchors.AssetRequestFunction is { } request) {
            emulator.AssetProvider = name => FindArchivedFile(clientDirectory, name);
            emulator.InterceptAssetRequests(request);
            emulator.InstructionBudget = RUNTIME_ENUM_INSTRUCTION_BUDGET;
            var succeeded = emulator.Run(initializer, 0);
            log?.WriteLine($"runtime enum options {(succeeded ? "filled" : "failed: " + emulator.Failures.GetValueOrDefault(initializer))}");
        } else {
            log?.WriteLine("runtime enum initializer not found; its enum options stay empty");
        }

        var walker = new TypeRegistryWalker(emulator);
        var manifest = walker.Walk(anchors.NameRegistryGetter);
        log?.WriteLine($"{manifest.Classes.Count} types, {walker.EnumTypesWithoutOptions.Count} enum types with no options (runtime-filled suspects)");
        if (walker.Problems.Count > 0) {
            throw new InvalidDataException(
                $"The type dump failed its self-check ({walker.Problems.Count} problems): {string.Join("; ", walker.Problems.Take(10))}");
        }

        return manifest;
    }

    /// <summary>
    /// Writes <paramref name="manifest"/> to <paramref name="stream"/>. The relaxed encoder and the
    /// explicit newline reproduce the manifest format exactly, so the output can be diffed byte for byte.
    /// </summary>
    public static void WriteJson(JsonDumpManifest manifest, Stream stream, string newLine = "\n") {
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {
            Indented = true,
            IndentSize = JSON_INDENT_SIZE,
            NewLine = newLine,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });

        writer.WriteStartObject();
        writer.WriteNumber("version", manifest.Version);
        writer.WriteStartObject("classes");
        foreach (var (key, @class) in manifest.Classes) {
            writer.WriteStartObject(key);
            writer.WriteString("name", @class.Name);
            writer.WriteStartArray("bases");
            foreach (var baseClass in @class.BaseClasses ?? []) {
                writer.WriteStringValue(baseClass);
            }

            writer.WriteEndArray();
            writer.WriteNumber("hash", @class.Hash);
            writer.WriteStartObject("properties");
            foreach (var (name, property) in @class.Properties) {
                WriteProperty(writer, name, property);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static byte[]? FindArchivedFile(string clientDirectory, string name) {
        // Shortest archive name first, then ordinal, so the same file wins no matter how the
        // filesystem happens to order the enumeration.
        var archives = Directory.EnumerateFiles(clientDirectory, ARCHIVE_FILE_PATTERN, SearchOption.AllDirectories)
            .OrderBy(path => Path.GetFileName(path).Length)
            .ThenBy(path => path, StringComparer.Ordinal);
        foreach (var path in archives) {
            using var stream = File.OpenRead(path);
            var archive = ArchiveParser.Parse(stream);
            var entry = archive?.Files.Keys.FirstOrDefault(key => key.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (entry is not null) {
                return archive!.OpenFile(entry)?.ToArray();
            }
        }

        return null;
    }

    private static void WriteProperty(Utf8JsonWriter writer, string name, JsonDumpProperty property) {
        writer.WriteStartObject(name);
        writer.WriteString("type", property.Type);
        writer.WriteNumber("id", property.Id);
        writer.WriteNumber("offset", property.Offset);
        writer.WriteNumber("flags", property.Flags);
        writer.WriteString("container", property.Container);
        writer.WriteBoolean("dynamic", property.Dynamic);
        writer.WriteBoolean("singleton", property.Singleton);
        writer.WriteBoolean("pointer", property.Pointer);
        writer.WriteNumber("hash", property.Hash);
        if (property.EnumOptions.Count > 0) {
            writer.WriteStartObject("enum_options");
            foreach (var (option, value) in property.EnumOptions) {
                if (value is uint number) {
                    writer.WriteNumber(option, number);
                } else {
                    writer.WriteString(option, (string) value);
                }
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }

}
