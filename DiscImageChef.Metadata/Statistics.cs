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
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Xml.Serialization;

namespace DiscImageChef.Metadata
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
        public List<MediaStats> Medias { get; set; }
        public BenchmarkStats Benchmark { get; set; }
        public MediaScanStats MediaScan { get; set; }
        public VerifyStats Verify { get; set; }
    }

    public class CommandsStats
    {
        public long Analyze;
        public long Benchmark;
        public long Checksum;
        public long Compare;
        public long CreateSidecar;
        public long ConvertImage;
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
        public VerifiedItems MediaImages;
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
        public TimeStats Times;
    }

    public class ChecksumStats
    {
        [XmlAttribute] public string algorithm;
        [XmlText] public double Value;
    }

    public class BenchmarkStats
    {
        public double All;
        [XmlElement("Checksum")] public List<ChecksumStats> Checksum;
        public double Entropy;
        public long MaxMemory;
        public long MinMemory;
        public double Sequential;
    }

    public class MediaStats
    {
        [XmlAttribute] public bool real;
        [XmlAttribute] public string type;
        [XmlText] public long Value;
    }

    public class DeviceStats
    {
        [XmlIgnore] public bool ManufacturerSpecified;
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Revision { get; set; }
        public string Bus { get; set; }
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