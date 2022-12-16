// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Statistics.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XML metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Define XML for statistics.
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

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Aaru.CommonTypes.Metadata;

/// <summary>Statistics</summary>
[XmlRoot("DicStats", Namespace = "", IsNullable = false)]
public class Stats
{
    /// <summary>Executed commands</summary>
    public CommandsStats Commands;
    /// <summary>Operating systems Aaru has run from</summary>
    [XmlArrayItem("OperatingSystem")]
    public List<OsStats> OperatingSystems { get; set; }
    /// <summary>Aaru versions</summary>
    [XmlArrayItem("Version")]
    public List<NameValueStats> Versions { get; set; }
    /// <summary>Detected filesystems</summary>
    [XmlArrayItem("Filesystem")]
    public List<NameValueStats> Filesystems { get; set; }
    /// <summary>Detected partitioning schemes</summary>
    [XmlArrayItem("Scheme")]
    public List<NameValueStats> Partitions { get; set; }
    /// <summary>Media image formats</summary>
    [XmlArrayItem("Format")]
    public List<NameValueStats> MediaImages { get; set; }
    /// <summary>Used filters</summary>
    [XmlArrayItem("Filter", IsNullable = true)]
    public List<NameValueStats> Filters { get; set; }
    /// <summary>Found devices</summary>
    [XmlArrayItem("Device", IsNullable = true)]
    public List<DeviceStats> Devices { get; set; }
    /// <summary>Found media types, real, and in image</summary>
    [XmlArrayItem("Media")]
    public List<MediaStats> Medias { get; set; }
    /// <summary>Benchmark statistics</summary>
    public BenchmarkStats Benchmark { get; set; }
    /// <summary>Media scanning statistics</summary>
    public MediaScanStats MediaScan { get; set; }
    /// <summary>Image verification statistics</summary>
    public VerifyStats Verify { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                             IncludeFields = true)]
[JsonSerializable(typeof(StatsDto))]
// ReSharper disable once PartialTypeWithSinglePart
public partial class StatsDtoContext : JsonSerializerContext {}

/// <summary>DTO for statistics</summary>
[SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
public class StatsDto
{
    /// <summary>Executed commands</summary>
    public List<NameValueStats> Commands { get; set; }
    /// <summary>Operating systems Aaru has run from</summary>
    public List<OsStats> OperatingSystems { get; set; }
    /// <summary>Aaru versions</summary>
    public List<NameValueStats> Versions { get; set; }
    /// <summary>Detected filesystems</summary>
    public List<NameValueStats> Filesystems { get; set; }
    /// <summary>Detected partitioning schemes</summary>
    public List<NameValueStats> Partitions { get; set; }
    /// <summary>Media image formats</summary>
    public List<NameValueStats> MediaFormats { get; set; }
    /// <summary>Used filters</summary>
    public List<NameValueStats> Filters { get; set; }
    /// <summary>Found devices</summary>
    public List<DeviceStats> Devices { get; set; }
    /// <summary>Found media types, real, and in image</summary>
    public List<MediaStats> Medias { get; set; }
    /// <summary>Remote applications</summary>
    public List<OsStats> RemoteApplications { get; set; }
    /// <summary>Remote application architectures</summary>
    public List<NameValueStats> RemoteArchitectures { get; set; }
    /// <summary>Operating systems where a remote application has been running</summary>
    public List<OsStats> RemoteOperatingSystems { get; set; }
}

/// <summary>Command execution statistics</summary>
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public class CommandsStats
{
    /// <summary>Number of times the filesystem info command has been used</summary>
    public long Analyze;
    /// <summary>Number of times the benchmark command has been used</summary>
    public long Benchmark;
    /// <summary>Number of times the image checksum command has been used</summary>
    public long Checksum;
    /// <summary>Number of times the image compare command has been used</summary>
    public long Compare;
    /// <summary>Number of times the image convert command has been used</summary>
    public long ConvertImage;
    /// <summary>Number of times the image create-sidecar command has been used</summary>
    public long CreateSidecar;
    /// <summary>Number of times the image decode command has been used</summary>
    public long Decode;
    /// <summary>Number of times the device info command has been used</summary>
    public long DeviceInfo;
    /// <summary>Number of times the device report command has been used</summary>
    public long DeviceReport;
    /// <summary>Number of times the media dump command has been used</summary>
    public long DumpMedia;
    /// <summary>Number of times the image entropy command has been used</summary>
    public long Entropy;
    /// <summary>Number of times the filesystem extract command has been used</summary>
    public long ExtractFiles;
    /// <summary>Number of times the list formats command has been used</summary>
    public long Formats;
    /// <summary>Number of times the image info command has been used</summary>
    public long ImageInfo;
    /// <summary>Number of times the device list command has been used</summary>
    public long ListDevices;
    /// <summary>Number of times the list encodings command has been used</summary>
    public long ListEncodings;
    /// <summary>Number of times the filesystem ls command has been used</summary>
    public long Ls;
    /// <summary>Number of times the media info command has been used</summary>
    public long MediaInfo;
    /// <summary>Number of times the media scan command has been used</summary>
    public long MediaScan;
    /// <summary>Number of times the image printhex command has been used</summary>
    public long PrintHex;
    /// <summary>Number of times the image verify command has been used</summary>
    public long Verify;
}

/// <summary>Statistics of verified media</summary>
public class VerifiedItems
{
    /// <summary>Number of correct images</summary>
    public long Correct;
    /// <summary>Number of failed images</summary>
    public long Failed;
}

/// <summary>Verification statistics</summary>
public class VerifyStats
{
    /// <summary>Image verification statistics</summary>
    public VerifiedItems MediaImages;
    /// <summary>Image contents verification statistics</summary>
    public ScannedSectors Sectors;
}

/// <summary>Image contents verification statistics</summary>
public class ScannedSectors
{
    /// <summary>Sectors found to be correct</summary>
    public long Correct;
    /// <summary>Sectors found to be incorrect</summary>
    public long Error;
    /// <summary>Total number of verified sectors</summary>
    public long Total;
    /// <summary>Total number of sectors that could not be verified</summary>
    public long Unverifiable;
}

/// <summary>Media scanning time statistics</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class TimeStats
{
    /// <summary>Number of sectors that took more than 3ms but less than 100ms to read</summary>
    public long LessThan10ms;
    /// <summary>Number of sectors that took more than 50ms but less than 150ms to read</summary>
    public long LessThan150ms;
    /// <summary>Number of sectors that took less than 3ms to read</summary>
    public long LessThan3ms;
    /// <summary>Number of sectors that took more than 150ms but less than 500ms to read</summary>
    public long LessThan500ms;
    /// <summary>Number of sectors that took more than 10ms but less than 50ms to read</summary>
    public long LessThan50ms;
    /// <summary>Number of sectors that took more than 500ms to read</summary>
    public long MoreThan500ms;
}

/// <summary>Media scanning statistics</summary>
public class MediaScanStats
{
    /// <summary>Statistics of scanned sectors</summary>
    public ScannedSectors Sectors;
    /// <summary>Scan time statistics</summary>
    public TimeStats Times;
}

/// <summary>Checksum type statistics</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ChecksumStats
{
    /// <summary>Checksum algorithm</summary>
    [XmlAttribute]
    public string algorithm;
    /// <summary>Time taken to execute algorithm</summary>
    [XmlText]
    public double Value;
}

/// <summary>Benchmark statistics</summary>
public class BenchmarkStats
{
    /// <summary>Total time taken to run the checksum algorithms in parallel</summary>
    public double All;
    /// <summary>List of time taken by each checksum algorithm</summary>
    [XmlElement("Checksum")]
    public List<ChecksumStats> Checksum;
    /// <summary>Time taken to benchmark entropy calculation</summary>
    public double Entropy;
    /// <summary>Maximum amount of memory used while running the benchmark</summary>
    public long MaxMemory;
    /// <summary>Minimum amount of memory used while running the benchmark</summary>
    public long MinMemory;
    /// <summary>Total time taken to run the checksum algorithms sequentially</summary>
    public double Sequential;
}

/// <summary>Media statistics</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MediaStats
{
    /// <summary>Found in a real device?</summary>
    [XmlAttribute]
    public bool real;
    /// <summary>Media type</summary>
    [XmlAttribute]
    public string type;
    /// <summary>Number of times it has been found</summary>
    [XmlText]
    public long Value;
}

/// <summary>Device statistics</summary>
public class DeviceStats
{
    /// <summary>Is manufacturer null?</summary>
    [XmlIgnore]
    public bool ManufacturerSpecified;
    /// <summary>Manufacturer string</summary>
    public string Manufacturer { get; set; }
    /// <summary>Model string</summary>
    public string Model { get; set; }
    /// <summary>Revision or firmware version</summary>
    public string Revision { get; set; }
    /// <summary>Bus the device was attached to</summary>
    public string Bus { get; set; }
}

/// <summary>Name=value pair statistics</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class NameValueStats
{
    /// <summary>Name</summary>
    [XmlAttribute]
    public string name { get; set; }
    /// <summary>Number of times it has been used/found</summary>
    [XmlText]
    public long Value { get; set; }
}

/// <summary>Operating system statistics</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class OsStats
{
    /// <summary>Operating system name</summary>
    [XmlAttribute]
    public string name { get; set; }
    /// <summary>Operating system version</summary>
    [XmlAttribute]
    public string version { get; set; }
    /// <summary>Number of times Aaru run on it</summary>
    [XmlText]
    public long Value { get; set; }
}