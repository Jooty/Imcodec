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

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Imcodec.BuildTools;

/// <summary>
/// Runs <see cref="GeneratorInputBootstrapper"/> in-process from the MSBuild generator-input targets, so
/// the build produces its own generator inputs without launching a separate executable.
/// </summary>
public sealed class GenerateGeneratorInputsTask : Microsoft.Build.Utilities.Task {

    private const string MESSAGE_INPUT_KIND = "messages";
    private const string TYPE_INPUT_KIND = "types";

    [Required]
    public string InputKind { get; set; } = string.Empty;

    [Required]
    public string ClientDirectory { get; set; } = string.Empty;

    [Required]
    public string OutputDirectory { get; set; } = string.Empty;

    [Output]
    public ITaskItem[] WrittenFiles { get; set; } = [];

    public override bool Execute() {
        try {
            if (string.Equals(InputKind, MESSAGE_INPUT_KIND, StringComparison.OrdinalIgnoreCase)) {
                var written = GeneratorInputBootstrapper.ExtractMessageDefinitions(ClientDirectory, OutputDirectory);
                WrittenFiles = [.. written.Select(path => (ITaskItem) new TaskItem(path))];
                Log.LogMessage(MessageImportance.High,
                    $"imcodec: extracted {written.Count} message definitions to {OutputDirectory}");

                return true;
            }

            if (string.Equals(InputKind, TYPE_INPUT_KIND, StringComparison.OrdinalIgnoreCase)) {
                var dumpPath = GeneratorInputBootstrapper.DumpTypes(ClientDirectory, OutputDirectory);
                WrittenFiles = [new TaskItem(dumpPath)];
                Log.LogMessage(MessageImportance.High, $"imcodec: wrote type dump to {dumpPath}");

                return true;
            }

            Log.LogError($"imcodec: unknown generator input kind '{InputKind}'.");

            return false;
        }
        catch (NotSupportedException ex) {
            // The type dumper is not part of this build; let the target fall back to a hand-dropped dump.
            Log.LogMessage(MessageImportance.High, $"imcodec: {ex.Message}");

            return true;
        }
        catch (Exception ex) {
            Log.LogError($"imcodec: {ex.Message}");

            return false;
        }
    }

}
