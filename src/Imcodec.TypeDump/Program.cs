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

using Imcodec.TypeDump;

const string USAGE = "usage: imcodec-typedump <client directory> <output json> [--crlf]";

if (args.Length < 2) {
    Console.Error.WriteLine(USAGE);
    return 2;
}

var manifest = TypeDumper.Dump(args[0], Console.Error);
using var output = File.Create(args[1]);
TypeDumper.WriteJson(manifest, output, args.Contains("--crlf") ? "\r\n" : "\n");
Console.Error.WriteLine($"wrote {manifest.Classes.Count} classes to {args[1]}");
return 0;
