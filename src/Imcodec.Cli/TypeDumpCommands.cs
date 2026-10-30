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

using Cocona;

namespace Imcodec.Cli;

/// <summary>
/// Dumps the client's property class types for inspection, from wherever the command is run.
/// </summary>
public sealed class TypeDumpCommands {

    private const string CURRENT_DIRECTORY_PATH = ".";

    /// <summary>
    /// Dumps the client's property class types to <c>typedump.json</c>. The dumper runs in-process;
    /// <c>--extended</c> adds the extra per-type and per-property detail that is useful for inspecting
    /// the type system.
    /// </summary>
    [PrimaryCommand]
    public int Run(
            [Option("client", Description = "Client install directory. Defaults to the ImcodecClientDir environment variable.")] string? client = null,
            [Option("out", Description = "Output directory.")] string outputDirectory = CURRENT_DIRECTORY_PATH,
            [Option("extended", Description = "Also emit the extended per-type and per-property detail.")] bool extended = false,
            [Option("verbose", Description = "Report emulation progress to standard error.")] bool verbose = false)
        => BootstrapCommands.DumpTypes(client, outputDirectory, extended, verbose);

}
