# Imcodec

## About
The "Imcodec" project began as a learning exercise to explore the mechanics and architecture of MMORPG server design. I have the utmost respect for the original developers and have taken steps to ensure this project does not infringe on their intellectual property. That being said, Imcodec has a BYOD (bring-your-own-data) philosophy and does not distribute any copyrighted game files. Users must obtain the original client and any necessary assets independently.

## Usage
Imcodec is a command line utility to unpack, deserialize, and understand the inner workings of the game client.

Two of the most common usages will be:
* Unpacking the client's proprietary WAD archive format
* Deserializing binary serialized files (known internally as Object Property)

## Installation
Imcodec is built with .NET 10.0 and packaged as a self-contained executable for Windows x64. Simply download the latest release and run the executable from the command line.

## Building
Imcodec uses compile-time source generation rather than runtime dynamic types: it generates strongly-typed C# classes from the client's own data definitions. The generators need two inputs, and in keeping with the bring-your-own-data philosophy neither is distributed with the source:

* **Message definitions** - the `*Messages*.xml` protocol files.
* **A type dump** - a JSON description of the client's property classes.

### One-command build
Point `ImcodecClientDir` at your client install and build. The build extracts the message definitions straight from the client and produces the type dump for you, then compiles everything in the same pass:

```
ImcodecClientDir=<client install directory> dotnet build Imcodec.sln
```

`ImcodecClientDir` may be an environment variable (as above) or an MSBuild property (`dotnet build -p:ImcodecClientDir=<client install directory>`). The extraction and the type dump are both handled by a small, input-free build-tools step, so a fresh clone builds in one command without any generated code having to exist first. The type dump is produced by reading the client executable directly; the client is never launched.

> The type dumper is a sibling of the build-tools step and is built alongside it. In the unusual case that it is absent from your tree, the message definitions are still extracted automatically and the build tells you to supply the type dump yourself - see the fallback below.

### Supplying the inputs yourself
If you would rather provide the inputs by hand, drop them into the generator input folders and build normally:

* message definition XML files in `src/Imcodec.MessageLayer/GeneratorInput`
* a type dump JSON in `src/Imcodec.ObjectProperty/GeneratorInput`

```
dotnet build Imcodec.sln
```

To keep the inputs outside the repository - for example to share one type dump across several worktrees - set `ImcodecMessageDefinitionDir` and/or `ImcodecTypeDumpDir` to the folders that hold them.

When the inputs are missing and no client is configured, the build fails fast with a single, actionable error (`IMCB001` for message definitions, `IMCB002` for the type dump) instead of a flood of compiler errors.

### Refreshing after a client update
The same extraction is available from the CLI, which is the easiest way to refresh the inputs after the client updates:

```
imcodec bootstrap all --client <client install directory>
```

`--client` defaults to the `ImcodecClientDir` environment variable. Use `imcodec bootstrap messages` or `imcodec bootstrap types` to refresh just one input.

### Continuous integration
`Imcodec.BuildTools.slnf` is a solution filter over the projects that need no client data. `dotnet build Imcodec.BuildTools.slnf` builds them with no inputs at all, which is useful for CI.

### Publishing
Run `dotnet publish -c Release` to create a publishable, self-contained executable.

## Command Structure

### Bootstrap Commands (`bootstrap`)
Generate or refresh the source generator inputs from a client install.

```
imcodec bootstrap all [--client <dir>] [--messages-out <dir>] [--types-out <dir>]
imcodec bootstrap messages [--client <dir>] [--out <dir>]
imcodec bootstrap types [--client <dir>] [--out <dir>] [--extended] [--verbose]
```

* `--client`: Client install directory. Defaults to the `ImcodecClientDir` environment variable.
* `--out` / `--messages-out` / `--types-out`: Output directories. Default to the generator input folders.
* `--extended` (`types` only): also emit the extended per-type and per-property detail (methods, notes, persistence hints, default values, static array counts, alternate hashes, and type sizes/flags). The generator ignores this section; it is there for inspecting the type system. Without the flag the dump is the plain manifest the generator consumes.
* `--verbose` (`types` only): report the executable emulation progress to standard error.

The type dump is produced in-process by emulating the client's type registration - the client is never launched. `imcodec bootstrap types` is the way to inspect the type system directly; `bootstrap all` and the one-command build run the same dumper to produce the generator input.

### Archive Commands (`wad`)
```
imcodec wad unpack <archivePath> [outputPath] [--deser] [--verbose]
```

* `archivePath`: Path to the WAD archive file
* `outputPath`: Optional path to the output directory. If this is not present, defaults to the archive name in the same directory.
* `--deser`: Attempt to deserialize archive files
* `--verbose`: Enable detailed output (may impact performance on large archives)

```
imcodec wad list <archivePath> [--verbose]
```

* `archivePath`: Path to the WAD archive file
* `--verbose`: Also show each entry's uncompressed and compressed size

Lists the entries in an archive without extracting them; nothing is inflated.

### Object Property Commands (`op`)
```
imcodec op file <inputPath> [outputPath]
```

* `inputPath`: Path to the binary file to deserialize
* `outputPath`: Optional path to save deserialized JSON (defaults to input path + "_deser.json" suffix)

```
imcodec op blob <hexblob>
```

* `hexBlob`: Hexadecimal string representing binary data to deserialize

#### Object Property Output Format
When deserializing files, Imcodec produces JSON output with metadata including:
* Original filename
* Serialization flags
* Class name & hash

### Binary Collision Data Commands (`bcd`)
Binary Collision Data (`.bcd`) files contain geometric collision shapes for zones.

```
imcodec bcd parse <inputPath> [outputPath] [--verbose]
```

* `inputPath`: Path to the BCD file to parse
* `outputPath`: Optional path to save JSON output (defaults to input path with .json extension)
* `--verbose`: Show detailed information about each collision object during parsing

```
imcodec bcd create <inputPath> [outputPath] [--verbose]
```

* `inputPath`: Path to JSON file (previously exported from BCD) to convert back to BCD format
* `outputPath`: Optional path to save BCD output (defaults to input path with .bcd extension)
* `--verbose`: Enable detailed output during creation

```
imcodec bcd info <inputPath>
```

* `inputPath`: Path to the BCD file to inspect
* Displays comprehensive statistics including:
  * File size and collision object count
  * Geometry type distribution (Box, Sphere, Cylinder, Mesh, etc.)
  * Collision and category flag usage
  * Mesh statistics (vertices, faces, averages)

#### BCD Output Format
When parsing BCD files, Imcodec produces JSON output with metadata including:
* Original filename and parsing timestamp
* Total collision object count
* Complete collision data with geometry parameters, transformation matrices, and mesh data
* Collision flags (Object, Walkable, Hitscan, Water, Trigger, etc.)

**Note**: BCD files within WAD archives are automatically detected and can be deserialized using the `--deser` flag with archive commands.

### Point of Interest Commands (`poi`)
Point of Interest (`.poi`) files describe a zone's navigation data: zone names, goals, teleporters, and mobs.

```
imcodec poi parse <inputPath> [outputPath] [--verbose]
```

* `inputPath`: Path to the POI file to parse
* `outputPath`: Optional path to save JSON output (defaults to input path with .json extension)
* `--verbose`: Show a summary of each parsed section

```
imcodec poi info <inputPath>
```

* `inputPath`: Path to the POI file to inspect
* Displays a section summary (zone names, goals, interactive goals, teleporters, goal adjectives, zone mobs) along with the zone list, teleporter destinations, and zone mobs.

**Note**: POI files within WAD archives are automatically detected and can be deserialized using the `--deser` flag with archive commands.

## Proxy

The proxy is a man-in-the-middle between a game client and the live servers. It does not hook the client or read its memory. The client talks to the proxy in plaintext, using the same unencrypted session mode the server side uses, while the proxy performs the encrypted handshake toward the live servers itself and derives the real session keys. It then decrypts, logs, re-encrypts and forwards traffic in both directions, turning the bytes into typed Imcodec messages.

> **Do not point this at the live servers.** Doing so is absolutely against the game's Terms of Service and may get your account banned. Imcodec is not responsible if you are banned or for any other consequence of its use.

### Usage

```
imcodec proxy --client <dir> [--wine <cmd>] [--upstream <host:port>] [--listen <port>] [--out <capture path>]
```

* `--client`: the client install directory; the proxy reads the server public keys from it and starts the patcher from it
* `--wine`: the command that runs the patcher on Linux (default: `wine`, inheriting your `WINEPREFIX`); not used on Windows
* `--upstream`: overrides the upstream login endpoint read from the client
* `--listen`: the local port the client connects to (default: any free port, printed at startup)
* `--out`: write a capture of every relayed message to this file

The proxy starts the install's own patcher, the single `*.exe` in the install root that a shortcut would run, and you log in there as usual; the proxy never sees your password. When the patcher launches the client (a new process that names one of the `Bin/*.exe` files and carries `-L`; other processes such as the bug reporter are left alone), the proxy catches it before it reaches the network by freezing it, reading the login endpoint off its command line, and relaunching it pointed at the local port instead. The login token on the command line is passed through unchanged and never logged. On Linux the patcher runs under `--wine`, which is required; on Windows it runs directly; other systems are not supported. The proxy exits when the client exits, and Ctrl+C tears everything down.

The proxy listens on `127.0.0.1` and opens one upstream connection for each client that connects. Toward the upstream server it runs the encrypted handshake with the keys from the client install; toward the client it offers an unencrypted session. Frames are relayed byte for byte in both directions until either side closes. Pointing the client at the local port is up to you.

When an upstream message hands the client to another server (character selection, server transfers), the proxy starts another local listener for that server, rewrites the address in the typed message to `127.0.0.1:<local port>` and forwards it. A server named again reuses its listener. Only TCP is relayed.

The capture is a JSON array in the project's capture format, one entry per message with `sender` (`client` or `server`), `timestamp`, `connection` (`login` or the handed-off server's `host:port`), `service_id`, `message_id`, `raw` (the frame in hex) and `data` (the typed fields, or `null` when Imcodec cannot decode the message). Entries are flushed as they are written. Fields whose names look like credentials, tokens or keys are always replaced by `[REDACTED]`, as is the raw frame of any message that had one; there is no option to turn this off.

## License
This project is licensed under the BSD 3-Clause License. See individual source files for the full license text.
