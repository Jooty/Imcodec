# Imcodec

## About
The "Imcodec" project began as a learning exercise to explore the mechanics and architecture of MMORPG server design. I have the utmost respect for the original developers and have taken steps to ensure this project does not infringe on their intellectual property. That being said, Imcodec has a BYOD (bring-your-own-data) philosophy and does not distribute any copyrighted game files. Users must obtain the original client and any necessary assets independently.

## Usage
Imcodec is a command line utility to unpack, deserialize, and understand the inner workings of Wizard101. 

Two of the most common usages will be:
* Unpacking "KIWAD" (KingsIsle's Wheres-All-The-Data) proprietary archive format
* Deserializing binary serialized files (known internally as Object Property)

## Installation
Imcodec is built with .NET 9.0 and packaged as a self-contained executable for Windows x64. Simply download the latest release and run the executable from the command line.

## Command Structure
Imcodec employs Cocona to provide a structured command-line interface with the following main commands:

### Archive Commands (`wad`)
```
imcodec wad unpack <archivePath> [outputPath] [--deser] [--verbose]
```

* `archivePath`: Path to the KIWAD archive file
* `outputPath`: Optional path to the output directory. If this is not present, defaults to the archive name in the same directory.
* `--deser`: Attempt to deserialize archive files
* `--verbose`: Enable detailed output (may impact performance on large archives)

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

## Building
1. Clone the repository
2. Imcodec uses compile-time source generation rather than runtime dynamic types to create strongly-typed C# classes from the game's data definitions:
    * Place a type dump in the JSON format in the `Imcodec.ObjectProperty/GeneratorInput` directory. These can be gathered using [wiztype](https://github.com/wizspoil/wiztype).
    * Place message definition XML files in the `Imcodec.MessageLayer/GeneratorInput` directory. These files (suffixed with `*Messages.xml`) can be gathered by unpacking the `Root.wad` archive using Imcodec itself
3. Any C# analyzer may be furious that source generated classes are not present. Simply run `dotnet build` once (at the project root) to run the source generators. 
4. Run `dotnet publish -c Release` to create a publishable version

## License
This project is licensed under the BSD 3-Clause License. See individual source files for the full license text.
