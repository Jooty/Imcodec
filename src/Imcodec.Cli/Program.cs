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

[HasSubCommands(typeof(TypeDumpCommands), commandName: "typedump", Description = "Dump the client's property class types for inspection.")]
[HasSubCommands(typeof(BootstrapCommands), commandName: "bootstrap", Description = "Generate or refresh the source generator inputs from a client install.")]
[HasSubCommands(typeof(ArchiveCommands), commandName: "wad", Description = "Archive related commands.")]
[HasSubCommands(typeof(ObjectPropertyCommands), commandName: "op", Description = "Serialization related commands.")]
[HasSubCommands(typeof(BcdCommands), commandName: "bcd", Description = "Binary Collision Data (BCD) file commands.")]
[HasSubCommands(typeof(PoiCommands), commandName: "poi", Description = "Point of Interest (POI) file commands.")]
[HasSubCommands(typeof(ProxyCommands), commandName: "proxy", Description = "Relay a client to the upstream servers.")]
public class Program {

    public static void Main(string[] args)
        => CoconaApp.Run<Program>(args);

}
