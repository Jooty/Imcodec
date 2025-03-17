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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Imcodec.MessageLayer.CodeGen.Definitions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Imcodec.MessageLayer.CodeGen;

/// <summary>
/// Provides functionality to read XML message definitions from a Root.wad archive.
/// </summary>
internal static class XmlMessageReader {
    private const string MetadataNodePrefix = "_";
    private const string MetadataDescriptionName = "_MsgDescription";
    private const string MetadataAccessLevelName = "_MsgAccessLvl";
    private const string MetadataOrderName = "_MsgOrder";
    private const string MetadataTypeName = "_MsgType";
    private const string ProtocolInfoNodeName = "_ProtocolInfo";
    private const string ServiceIdName = "_ServiceID";
    private const string ProtocolTypeName = "_ProtocolType";
    private const string ProtocolDescriptionName = "_ProtocolDescription";
    private const string ProtocolVersionName = "_ProtocolVersion";
    private const string RecordNodeName = "RECORD";
    private const string UnknownProtocolType = "UNKNOWN_PROTOCOL_TYPE";

    private static readonly Dictionary<string, string> s_cSharpTypeMap = new() {
        { "BYT", "sbyte" },
        { "BOOL", "bool" },
        { "UBYT", "byte" },
        { "UBYTE", "byte" },
        { "SHRT", "short" },
        { "USHRT", "ushort" },
        { "USHORT", "ushort" },
        { "INT", "int" },
        { "UINT", "uint" },
        { "STR", "string" },
        { "WSTR", "WideByteString" },
        { "FLT", "float" },
        { "DBL", "double" },
        { "GID", "ulong" }
    };

    public static ProtocolDefinition? ExtractProtocolDefinition(AdditionalText xmlFile, SourceProductionContext context) {
        var xml = new XmlDocument();
        using (var stringReader = new StringReader(xmlFile!.GetText()!.ToString())) {
            xml.Load(stringReader);
        }

        return ExtractProtocolDefinition(xml, xmlFile.Path, context);
    }

    public static ProtocolDefinition? ExtractProtocolDefinition(XmlDocument xmlDocument, string fileName, SourceProductionContext context) {
        var messagesNode = xmlDocument.DocumentElement;
        if (messagesNode == null) {
            return null;
        }

        // Extract protocol information
        var protocolInfo = GetProtocolInformation(messagesNode, context);
        if (protocolInfo == null) {
            return null;
        }

        var protocol = new ProtocolDefinition {
            ServiceId = protocolInfo.Value.ServiceId,
            ProtocolType = protocolInfo.Value.ProtocolType,
            ProtocolDescription = protocolInfo.Value.ProtocolDescription,
            ProtocolVersion = protocolInfo.Value.ProtocolVersion,
            SourceFileName = fileName
        };

        // Extract messages
        var xmlRecords = GetXmlRecordsFromProtocol(messagesNode);

        foreach (var xmlRecord in xmlRecords) {
            var message = ExtractMessageDefinition(xmlRecord, protocol.ServiceId);
            if (message != null) {
                protocol.Messages.Add(message);
            }
        }

        // Set the message order for each message.
        for (var i = 0; i < protocol.Messages.Count; i++) {
            protocol.Messages[i].MessageOrder = (byte) (i + 1);
        }

        return protocol;
    }

    private static MessageDefinition? ExtractMessageDefinition(XmlNode messageNode, byte serviceId) {
        // Skip metadata nodes
        if (messageNode.Name.StartsWith(MetadataNodePrefix) || messageNode.NodeType == XmlNodeType.Comment) {
            return null;
        }

        var recordNode = messageNode.SelectSingleNode(RecordNodeName);
        if (recordNode == null) {
            return null;
        }

        var message = new MessageDefinition {
            Name = messageNode.Name,
            ServiceId = serviceId
        };

        // Extract message metadata
        var msgOrderNode = recordNode.SelectSingleNode(MetadataOrderName) ?? recordNode.SelectSingleNode(MetadataTypeName);
        if (msgOrderNode != null) {
            message.MessageOrder = byte.Parse(msgOrderNode.InnerText);
        }

        var accessLevelNode = recordNode.SelectSingleNode(MetadataAccessLevelName);
        if (accessLevelNode != null) {
            message.AccessLevel = byte.Parse(accessLevelNode.InnerText);
        }

        var descriptionNode = recordNode.SelectSingleNode(MetadataDescriptionName);
        if (descriptionNode != null) {
            message.Description = descriptionNode.InnerText;
        }

        // Extract message fields
        foreach (XmlNode fieldNode in recordNode.ChildNodes) {
            if (fieldNode.NodeType != XmlNodeType.Element || fieldNode.Name.StartsWith(MetadataNodePrefix)) {
                continue;
            }

            var serializedType = GetDataTypeFromXmlElement(fieldNode as XmlElement);
            if (string.IsNullOrEmpty(serializedType) || !s_cSharpTypeMap.TryGetValue(serializedType, out var csharpType)) {
                continue;
            }

            message.Fields.Add(new MessageFieldDefinition {
                Name = fieldNode.Name,
                SerializedType = serializedType,
                CSharpType = csharpType
            });
        }

        return message;
    }

    private static (byte ServiceId, string ProtocolType, string ProtocolDescription, int ProtocolVersion)? GetProtocolInformation(
        XmlNode messagesNode,
        SourceProductionContext context) {

        var protocolInfo = messagesNode.SelectSingleNode($"./{ProtocolInfoNodeName}/{RecordNodeName}");
        if (protocolInfo == null) {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("IMCODEC001", "Protocol Info Missing", "Protocol info is missing in the XML file.", "Usage", DiagnosticSeverity.Error, true),
                Location.None));

            return null;
        }

        // Try direct child selection using various methods.
        var serviceIdNode = protocolInfo.SelectSingleNode(ServiceIdName);
        if (serviceIdNode == null) {
            serviceIdNode = protocolInfo.SelectSingleNode("ServiceID");
        }

        if (serviceIdNode == null) {
            foreach (XmlNode child in protocolInfo.ChildNodes) {
                if (child.Name.Equals("ServiceID", StringComparison.OrdinalIgnoreCase)) {
                    serviceIdNode = child;
                    break;
                }
            }
        }

        if (serviceIdNode == null) {
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("IMCODEC002", "Service ID Missing", "Service ID is missing in the XML file.", "Usage", DiagnosticSeverity.Error, true),
                Location.None));

            return null;
        }

        var protocolTypeNode = protocolInfo.SelectSingleNode(ProtocolTypeName) ??
                              protocolInfo.SelectSingleNode("ProtocolType");
        var protocolDescNode = protocolInfo.SelectSingleNode(ProtocolDescriptionName) ??
                              protocolInfo.SelectSingleNode("ProtocolDescription");
        var protocolVersionNode = protocolInfo.SelectSingleNode(ProtocolVersionName) ??
                                 protocolInfo.SelectSingleNode("ProtocolVersion");

        return (
            byte.Parse(serviceIdNode.InnerText),
            protocolTypeNode?.InnerText ?? UnknownProtocolType,
            protocolDescNode?.InnerText ?? string.Empty,
            protocolVersionNode != null ? int.Parse(protocolVersionNode.InnerText) : 1
        );
    }

    private static List<XmlNode> GetXmlRecordsFromProtocol(XmlNode messagesNode) {
        var records = new List<XmlNode>();

        foreach (XmlNode node in messagesNode.ChildNodes) {
            if (node.NodeType == XmlNodeType.Element && !node.Name.StartsWith(MetadataNodePrefix)) {
                records.Add(node);
            }
        }

        // If the protocol needs ordinal sorting, sort the records by name
        if (DoesProtocolNeedOrdering(records)) {
            var sortedNodes = new XmlNode[records.Count];

            // Copy everything to the new array.
            // Array.Copy does NOT work here, as the XmlNode object is only accessable via the iterator.
            for (int i = 0; i < records.Count; i++) {
                sortedNodes[i] = records[i];
            }

            Array.Sort(sortedNodes, static (x, y) => string.CompareOrdinal(x.Name, y.Name));

            return [.. sortedNodes];
        }

        return records;
    }

    private static bool DoesProtocolNeedOrdering(List<XmlNode> protocolRecords) {
        if (protocolRecords.Count <= 1) {
            return false;
        }

        // Check the first actual message record (not protocol info).
        var firstRecord = protocolRecords
            .FirstOrDefault(n => n.Name != ProtocolInfoNodeName)
            ?.SelectSingleNode(RecordNodeName);

        if (firstRecord == null) {
            return false;
        }

        // If any record has _MsgType or _MsgOrder, we don't need to sort.
        return !firstRecord.ChildNodes.Cast<XmlNode>()
            .Any(static n => n.Name is MetadataOrderName or MetadataTypeName);
    }

    private static string GetDataTypeFromXmlElement(XmlElement? element) {
        if (element == null) {
            return string.Empty;
        }

        // Check for different attribute names that can indicate type
        foreach (var attr in new[] { "TYPE", "TPYE", "TYP" }) {
            if (element.HasAttribute(attr)) {
                return element.GetAttribute(attr);
            }
        }

        // Special case for GlobalID
        if (element.Name == "GlobalID") {
            return "GID";
        }

        return string.Empty;
    }
}