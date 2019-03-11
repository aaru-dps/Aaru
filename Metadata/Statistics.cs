// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Xml.Serialization;

namespace DiscImageChef.CommonTypes.Metadata
{
    [XmlRoot("DicStats", Namespace = "", IsNullable = false)]
    public class Stats
    {
        public CommandsStats Commands;
        [XmlArrayItem("OperatingSystem")]
        public List<OsStats> OperatingSystems { get; set; }
        [XmlArrayItem("Version")]
        public List<NameValueStats> Versions { get; set; }
        [XmlArrayItem("Filesystem")]
        public List<NameValueStats> Filesystems { get; set; }
        [XmlArrayItem("Scheme")]
        public List<NameValueStats> Partitions { get; set; }
        [XmlArrayItem("Format")]
        public List<NameValueStats> MediaImages { get; set; }
        [XmlArrayItem("Filter", IsNullable = true)]
        public List<NameValueStats> Filters { get; set; }
        [XmlArrayItem("Device", IsNullable = true)]
        public List<DeviceStats> Devices { get; set; }
        [XmlArrayItem("Media")]
        public List<MediaStats> Medias { get;  set; }
        public BenchmarkStats Benchmark { get; set; }
        public MediaScanStats MediaScan { get; set; }
        public VerifyStats    Verify    { get; set; }
    }

    public class StatsDto
    {
        public List<NameValueStats> Commands         { get; set; }
        public List<OsStats>        OperatingSystems { get; set; }
        public List<NameValueStats> Versions         { get; set; }
        public List<NameValueStats> Filesystems      { get; set; }
        public List<NameValueStats> Partitions       { get; set; }
        public List<NameValueStats> MediaFormats     { get; set; }
        public List<NameValueStats> Filters          { get; set; }
        public List<DeviceStats>    Devices          { get; set; }
        public List<MediaStats>     Medias           { get; set; }
    }

    public class CommandsStats
    {
        public long Analyze;
        public long Benchmark;
        public long Checksum;
        public long Compare;
        public long ConvertImage;
        public long CreateSidecar;
        public long Decode;
        public long DeviceInfo;
        public long DeviceReport;
        public long DumpMedia;
        public long Entropy;
        public long ExtractFiles;
        public long Formats;
        public long ImageInfo;
        public long ListDevices;
        public long ListEncodings;
        public long Ls;
        public long MediaInfo;
        public long MediaScan;
        public long PrintHex;
        public long Verify;
    }

    public class VerifiedItems
    {
        public long Correct;
        public long Failed;
    }

    public class VerifyStats
    {
        public VerifiedItems  MediaImages;
        public ScannedSectors Sectors;
    }

    public class ScannedSectors
    {
        public long Correct;
        public long Error;
        public long Total;
        public long Unverifiable;
    }

    public class TimeStats
    {
        public long LessThan10ms;
        public long LessThan150ms;
        public long LessThan3ms;
        public long LessThan500ms;
        public long LessThan50ms;
        public long MoreThan500ms;
    }

    public class MediaScanStats
    {
        public ScannedSectors Sectors;
        public TimeStats      Times;
    }

    public class ChecksumStats
    {
        [XmlAttribute]
        public string algorithm;
        [XmlText]
        public double Value;
    }

    public class BenchmarkStats
    {
        public double All;
        [XmlElement("Checksum")]
        public List<ChecksumStats> Checksum;
        public double Entropy;
        public long   MaxMemory;
        public long   MinMemory;
        public double Sequential;
    }

    public class MediaStats
    {
        [XmlAttribute]
        public bool real;
        [XmlAttribute]
        public string type;
        [XmlText]
        public long Value;
    }

    public class DeviceStats
    {
        [XmlIgnore]
        public bool ManufacturerSpecified;
        public string Manufacturer { get; set; }
        public string Model        { get; set; }
        public string Revision     { get; set; }
        public string Bus          { get; set; }
    }

    public class NameValueStats
    {
        [XmlAttribute]
        public string name { get; set; }
        [XmlText]
        public long Value { get; set; }
    }

    public class OsStats
    {
        [XmlAttribute]
        public string name { get; set; }
        [XmlAttribute]
        public string version { get; set; }
        [XmlText]
        public long Value { get; set; }
    }
}