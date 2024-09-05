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

using Imcodec.ObjectProperty.CodeGen.JSON;

namespace Imcodec.Test.CodeGen;

public class CodeGenTest {

    private const string JSON_DUMP_PATH = "CodeGen/Inputs/r756936_WizardDev.json";

    //[Fact]
    //public void JsonCodeGenTest() {
    //    var json = GetJsonDump();
    //    var compiler = new JsonToCsharpCompiler();
    //    var code = compiler.Compile(json);

    //    Assert.NotNull(code);
    //}

    private static string GetJsonDump() {
        // Check to see if the file exists.
        var filePath = $"{Directory.GetCurrentDirectory()}/{JSON_DUMP_PATH}";
        if (!File.Exists(filePath)) {
            throw new FileNotFoundException("The JSON dump file does not exist.");
        }

        return File.ReadAllText(filePath);
    }

}
