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

using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Imcodec.IO;
using Imcodec.MessageLayer;

namespace Imcodec.Proxy;

/// <summary>
/// Streams every relayed message to a JSON capture in the project's capture format, one entry per
/// message, flushed as it is written so a crash keeps everything recorded so far.
/// </summary>
/// <remarks>
/// Fields whose names look like credentials, tokens or keys are always replaced by
/// <see cref="REDACTED_MARKER"/>, and so is the raw frame of any message that had one.
/// Safe to call from several connections at once.
/// </remarks>
public sealed class CaptureWriter : IDisposable {

    public const string CLIENT_SENDER = "client";
    public const string SERVER_SENDER = "server";
    public const string REDACTED_MARKER = "[REDACTED]";

    private const string TIMESTAMP_FORMAT = "yyyy-MM-ddTHH:mm:ss.ffffff";
    private const string CONTROL_FORMAT = "CONTROL";
    private const string MESSAGE_FORMAT = "DML";
    private const string VALUE_PROPERTY = "value";
    private const string FORMAT_PROPERTY = "format";
    private const byte CONTROL_SERVICE_ID = 0;
    private const byte LOGIN_SERVICE_ID = 7;
    private const byte FIRST_PRINTABLE_ASCII = 0x20;
    private const byte LAST_PRINTABLE_ASCII = 0x7E;

    private static readonly string[] CREDENTIAL_NAME_FRAGMENTS =
        ["pass", "key", "token", "ticket", "rec1", "auth", "crypto", "secret", "cookie", "signature"];

    private readonly FileStream _file;
    private readonly Utf8JsonWriter _json;
    private readonly Lock _gate = new();

    /// <summary>
    /// Creates or replaces the capture file and opens its entry array.
    /// </summary>
    public CaptureWriter(string path) {
        _file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        _json = new Utf8JsonWriter(_file, new JsonWriterOptions { Indented = true });
        _json.WriteStartArray();
        _json.Flush();
    }

    /// <summary>
    /// Records the messages of one frame, and an entry with the raw bytes for each message that did not decode.
    /// </summary>
    /// <param name="connection">The label of the connection the frame travelled on.</param>
    /// <param name="sender"><see cref="CLIENT_SENDER"/> or <see cref="SERVER_SENDER"/>.</param>
    /// <param name="frame">The plaintext frame.</param>
    /// <param name="messages">The messages that decoded.</param>
    /// <param name="undecoded">The service and message ids of the messages that did not.</param>
    internal void Record(string connection, string sender, byte[] frame, IReadOnlyList<IMessage> messages,
        IReadOnlyList<(byte ServiceId, byte MessageId)> undecoded) {
        var timestamp = DateTime.Now.ToString(TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
        lock (_gate) {
            foreach (var message in messages) {
                WriteDecoded(connection, sender, timestamp, frame, message);
            }
            foreach (var (serviceId, messageId) in undecoded) {
                WriteUndecoded(connection, sender, timestamp, frame, serviceId, messageId);
            }
            _json.Flush();
            _file.Flush();
        }
    }

    /// <summary>
    /// Closes the entry array and the file.
    /// </summary>
    public void Dispose() {
        lock (_gate) {
            _json.WriteEndArray();
            _json.Flush();
            _json.Dispose();
            _file.Dispose();
        }
    }

    private void WriteDecoded(string connection, string sender, string timestamp, byte[] frame, IMessage message) {
        var fields = message.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.CanWrite)
            .ToArray();
        var redacted = fields.Any(field => IsCredential(field.Name));

        WriteEntryStart(connection, sender, timestamp, message.ServiceId, message.MessageOrder);
        _json.WriteString("raw", redacted ? REDACTED_MARKER : PrettyHex(frame));
        _json.WriteStartObject("data");
        _json.WriteString(FORMAT_PROPERTY, message.ServiceId == CONTROL_SERVICE_ID ? CONTROL_FORMAT : MESSAGE_FORMAT);
        _json.WriteString("name", message.GetType().Name);
        _json.WriteStartObject("fields");
        foreach (var field in fields) {
            _json.WriteStartObject(field.Name);
            WriteFieldValue(field.GetValue(message), IsCredential(field.Name));
            _json.WriteEndObject();
        }
        _json.WriteEndObject();
        _json.WriteEndObject();
        _json.WriteEndObject();
    }

    private void WriteUndecoded(string connection, string sender, string timestamp, byte[] frame, byte serviceId, byte messageId) {
        var mayHoldCredentials = serviceId is CONTROL_SERVICE_ID or LOGIN_SERVICE_ID;
        WriteEntryStart(connection, sender, timestamp, serviceId, messageId);
        _json.WriteString("raw", mayHoldCredentials ? REDACTED_MARKER : PrettyHex(frame));
        _json.WriteNull("data");
        _json.WriteEndObject();
    }

    private void WriteEntryStart(string connection, string sender, string timestamp, byte serviceId, byte messageId) {
        _json.WriteStartObject();
        _json.WriteString("sender", sender);
        _json.WriteString("timestamp", timestamp);
        _json.WriteString("connection", connection);
        _json.WriteNumber("service_id", serviceId);
        _json.WriteNumber("message_id", messageId);
    }

    private void WriteFieldValue(object? value, bool redact) {
        var format = FieldFormat(value);
        if (redact) {
            _json.WriteString(VALUE_PROPERTY, REDACTED_MARKER);
            _json.WriteString(FORMAT_PROPERTY, format);
            return;
        }

        switch (value) {
            case ulong number: _json.WriteNumber(VALUE_PROPERTY, number); break;
            case uint number: _json.WriteNumber(VALUE_PROPERTY, number); break;
            case int number: _json.WriteNumber(VALUE_PROPERTY, number); break;
            case ushort number: _json.WriteNumber(VALUE_PROPERTY, number); break;
            case short number: _json.WriteNumber(VALUE_PROPERTY, number); break;
            case byte number: _json.WriteNumber(VALUE_PROPERTY, number); break;
            case sbyte number: _json.WriteNumber(VALUE_PROPERTY, number); break;
            case long number: _json.WriteNumber(VALUE_PROPERTY, number); break;
            case float number when float.IsFinite(number): _json.WriteNumber(VALUE_PROPERTY, number); break;
            case double number when double.IsFinite(number): _json.WriteNumber(VALUE_PROPERTY, number); break;
            case bool flag: _json.WriteBoolean(VALUE_PROPERTY, flag); break;
            case ByteString text: _json.WriteString(VALUE_PROPERTY, IsPrintable(text) ? (string) text : PrettyHex(text)); break;
            case byte[] bytes: _json.WriteString(VALUE_PROPERTY, PrettyHex(bytes)); break;
            case null: _json.WriteNull(VALUE_PROPERTY); break;
            default: _json.WriteString(VALUE_PROPERTY, Convert.ToString(value, CultureInfo.InvariantCulture)); break;
        }
        _json.WriteString(FORMAT_PROPERTY, format);
    }

    private static string FieldFormat(object? value) => value switch {
        ulong => "GID",
        uint => "UINT",
        int => "INT",
        ushort => "USHRT",
        short => "SHRT",
        byte => "UBYT",
        sbyte => "BYT",
        long => "INT64",
        float => "FLT",
        double => "DBL",
        bool => "BOOL",
        ByteString text => IsPrintable(text) ? "STR:ascii" : "STR:hex",
        WideByteString => "WSTR",
        byte[] => "pretty bytes",
        null => "null",
        _ => value.GetType().Name,
    };

    private static bool IsCredential(string fieldName)
        => CREDENTIAL_NAME_FRAGMENTS.Any(fragment => fieldName.Contains(fragment, StringComparison.OrdinalIgnoreCase));

    private static bool IsPrintable(ByteString text) {
        byte[]? bytes = text;
        return bytes is null || bytes.All(b => b is >= FIRST_PRINTABLE_ASCII and <= LAST_PRINTABLE_ASCII);
    }

    private static string PrettyHex(byte[]? bytes)
        => bytes is null ? string.Empty : string.Join(' ', bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));

}
