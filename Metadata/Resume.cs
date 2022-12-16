// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Resume.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XML metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines XML format of a dump resume file.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Aaru.CommonTypes.AaruMetadata;

namespace Aaru.CommonTypes.Metadata;

[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                             IncludeFields = true)]
[JsonSerializable(typeof(ResumeJson))]
// ReSharper disable once PartialTypeWithSinglePart
public partial class ResumeJsonContext : JsonSerializerContext {}

public class ResumeJson
{
    [JsonPropertyName("AaruResume")]
    public Resume Resume { get; set; }
}

/// <summary>Information that allows to resume a dump</summary>
[Serializable, XmlRoot("DicResume", Namespace = "", IsNullable = false)]
public class Resume
{
    /// <summary>List of blocks that returned an error on reading</summary>
    [XmlArrayItem("Block")]
    public List<ulong> BadBlocks;
    /// <summary>Date/time this resume file was created</summary>
    [XmlElement(DataType = "dateTime")]
    public DateTime CreationDate;
    /// <summary>Last block on media</summary>
    public ulong LastBlock;
    /// <summary>Date/time this resume file was last written to</summary>
    [XmlElement(DataType = "dateTime")]
    public DateTime LastWriteDate;
    /// <summary>Next block to read</summary>
    public ulong NextBlock;
    /// <summary>Is media removable?</summary>
    public bool Removable;
    /// <summary>Is media a tape?</summary>
    public bool Tape;
    /// <summary>List of CD subchannels that did not read correctly</summary>
    [XmlArrayItem("Block")]
    public List<int> BadSubchannels;
    /// <summary>Extents of BLANK sectors for magneto-opticals</summary>
    [XmlArrayItem("Extent")]
    public Extent[] BlankExtents;
    /// <summary>Title keys that has not been read</summary>
    [XmlArrayItem("Block")]
    public List<ulong> MissingTitleKeys;
    /// <summary>List of dump tries</summary>
    [XmlArrayItem("DumpTry")]
    public List<DumpHardware> Tries;
}