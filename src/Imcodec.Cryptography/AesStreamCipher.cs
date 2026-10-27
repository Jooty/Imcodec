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
using System.Security.Cryptography;

namespace Imcodec.Cryptography;

/// <summary>
/// One direction of the session's AES-GCM stream. After every chunk of data a tag and a fresh nonce
/// are appended and the GCM state is restarted with that nonce.
/// </summary>
public sealed class AesStreamCipher : IDisposable {

    public const int CLIENT_CHUNK_BLOCKS = 0x100;
    public const int SERVER_CHUNK_BLOCKS = 0x1000;
    public const int BLOCK_SIZE = 16;
    public const int NONCE_SIZE = 16;
    public const int TAG_SIZE = 16;

    private readonly byte[] _key;
    private readonly int _chunkSize;
    private readonly Func<byte[]> _nextNonce;
    private GcmStream _encryptor;
    private GcmStream _decryptor;
    private int _encryptedInChunk;
    private int _decryptedInChunk;

    private AesStreamCipher(byte[] key, byte[] nonce, int chunkBlocks, Func<byte[]> nextNonce) {
        _key = key;
        _chunkSize = chunkBlocks * BLOCK_SIZE;
        _nextNonce = nextNonce;
        _encryptor = new GcmStream(key, nonce);
        _decryptor = new GcmStream(key, nonce);
    }

    /// <summary>
    /// The cipher the client uses for the data it sends.
    /// </summary>
    public static AesStreamCipher Client(byte[] key, byte[] nonce) => Create(key, nonce, CLIENT_CHUNK_BLOCKS);

    /// <summary>
    /// The cipher the server uses for the data it sends.
    /// </summary>
    public static AesStreamCipher Server(byte[] key, byte[] nonce) => Create(key, nonce, SERVER_CHUNK_BLOCKS);

    internal static AesStreamCipher Create(byte[] key, byte[] nonce, int chunkBlocks, Func<byte[]>? nextNonce = null) =>
        new(key, nonce, chunkBlocks, nextNonce ?? RandomNonce);

    /// <summary>
    /// Encrypts data, appending a tag and a new nonce each time a chunk fills up.
    /// </summary>
    public byte[] Encrypt(ReadOnlySpan<byte> data) {
        var output = new List<byte>(data.Length);
        while (!data.IsEmpty) {
            var take = Math.Min(data.Length, _chunkSize - _encryptedInChunk);
            var encrypted = new byte[take];
            _encryptor.Transform(data[..take], encrypted, encrypting: true);
            output.AddRange(encrypted);
            data = data[take..];
            _encryptedInChunk = (_encryptedInChunk + take) % _chunkSize;

            if (_encryptedInChunk == 0) {
                output.AddRange(_encryptor.FinishTag());
                var nonce = _nextNonce();
                output.AddRange(nonce);
                _encryptor.Dispose();
                _encryptor = new GcmStream(_key, nonce);
            }
        }
        return output.ToArray();
    }

    /// <summary>
    /// Decrypts data, checking the tag and switching to the new nonce each time a chunk ends.
    /// </summary>
    /// <exception cref="InvalidDataException">The data ends inside a chunk trailer.</exception>
    /// <exception cref="AuthenticationTagMismatchException">A chunk's tag does not match.</exception>
    public byte[] Decrypt(ReadOnlySpan<byte> data) {
        var output = new List<byte>(data.Length);
        while (!data.IsEmpty) {
            var take = Math.Min(data.Length, _chunkSize - _decryptedInChunk);
            var decrypted = new byte[take];
            _decryptor.Transform(data[..take], decrypted, encrypting: false);
            output.AddRange(decrypted);
            data = data[take..];
            _decryptedInChunk = (_decryptedInChunk + take) % _chunkSize;

            if (_decryptedInChunk == 0) {
                if (data.Length < TAG_SIZE + NONCE_SIZE) {
                    throw new InvalidDataException("The data ends before the chunk tag and nonce.");
                }

                if (!CryptographicOperations.FixedTimeEquals(_decryptor.FinishTag(), data[..TAG_SIZE])) {
                    throw new AuthenticationTagMismatchException();
                }

                var nonce = data.Slice(TAG_SIZE, NONCE_SIZE);
                _decryptor.Dispose();
                _decryptor = new GcmStream(_key, nonce);
                data = data[(TAG_SIZE + NONCE_SIZE)..];
            }
        }
        return output.ToArray();
    }

    public void Dispose() {
        _encryptor.Dispose();
        _decryptor.Dispose();
    }

    private static byte[] RandomNonce() => RandomNumberGenerator.GetBytes(NONCE_SIZE);
}
