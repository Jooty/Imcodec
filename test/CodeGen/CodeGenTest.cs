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

using Imcodec.ObjectProperty.CodeGen;
using Imcodec.ObjectProperty.CodeGen.JSON;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Imcodec.Test.CodeGen;

public class CodeGenTest {

    private const string JSON_DUMP_PATH = "CodeGen/Inputs/r756936_WizardDev.json";

    [Fact]
    public void JsonCodeGenTest() {
        var jsonDump = GetJsonDump();
        Assert.NotNull(jsonDump);

        // Create a compilation of our JSON dump.
        var compilation = CreateCompilation(jsonDump);

        var generator = new SourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        var runDriver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // We can now assert things about the resulting compilation:
        // See also: https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators
        Debug.Assert(diagnostics.IsEmpty); // there were no diagnostics created by the generators
        Debug.Assert(outputCompilation.SyntaxTrees.Count() >= 2); // Syntax trees are the amount of files we added to the context.
    }

    [Fact]
    public void DumpManifestTest() {
        var jsonDump = GetJsonDump();
        Assert.NotNull(jsonDump);

        var compiler = new JsonToCsharpCompiler();
        var classDefinitions = compiler.Compile(jsonDump);

        // Write the class definitions to a file. Serialize the classes as json.
        var json = JsonSerializer.Serialize(classDefinitions);
        File.WriteAllText($"{Directory.GetCurrentDirectory()}/CodeGen/Outputs/JsonToCsharpCompiler.json", json);

        Assert.NotEmpty(classDefinitions);
    }

    private static string? GetJsonDump() {
        // Check to see if the file exists.
        var filePath = $"{Directory.GetCurrentDirectory()}/{JSON_DUMP_PATH}";
        if (!File.Exists(filePath)) {
            return null;
        }

        return File.ReadAllText(filePath);
    }

    private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                [CSharpSyntaxTree.ParseText(source)],
                [MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location)],
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

}
