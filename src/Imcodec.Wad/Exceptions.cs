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

namespace Imcodec.Wad;

/// <summary>
/// Exception to represent invalid archive format errors.
/// </summary>
/// <param name="message">The error message.</param>
public class InvalidArchiveFormatException(string message) : Exception(message) { }

/// <summary>
/// Exception to represent a file not found in the archive.
/// </summary>
/// <param name="fileName">The name of the file that was not found.</param>
public class FileNotFoundInArchiveException(string fileName)
    : Exception($"File '{fileName}' was not found in the archive.") { }
