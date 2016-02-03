// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Statistics.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or(at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright(C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DiscImageChef.Core
{
    [XmlRoot("DicStats", Namespace = "", IsNullable = false)]
    public class Stats
    {
        public CommandsStats Commands;
        [XmlArrayItem("Filesystem")]
        public List<NameValueStats> Filesystems;
        [XmlArrayItem("Scheme")]
        public List<NameValueStats> Partitions;
        [XmlArrayItem("Format")]
        public List<NameValueStats> MediaImages;
        [XmlArrayItem("Device", IsNullable = true)]
        public List<DeviceStats> Devices;
        [XmlArrayItem("Media")]
        public List<MediaStats> Medias;
        public BenchmarkStats Benchmark;
        public MediaScanStats MediaScan;
        public VerifyStats Verify;
    }

    public class CommandsStats
    {
        public long Analyze;
        public long Benchmark;
        public long Checksum;
        public long Compare;
        public long CreateSidecar;
        public long Decode;
        public long DeviceInfo;
        public long DeviceReport;
        public long DumpMedia;
        public long Entropy;
        public long Formats;
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
        public long Total;
        public long Error;
        public long Correct;
        public long Unverifiable;
    }

    public class TimeStats
    {
        public long LessThan3ms;
        public long LessThan10ms;
        public long LessThan50ms;
        public long LessThan150ms;
        public long LessThan500ms;
        public long MoreThan500ms;
    }

    public class MediaScanStats
    {
        public ScannedSectors Sectors;
        public TimeStats Times;
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
        [XmlElement("Checksum")]
        public List<ChecksumStats> Checksum;
        public double Entropy;
        public double All;
        public double Sequential;
        public long MaxMemory;
        public long MinMemory;
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
        public string Manufacturer;
        public string Model;
        public string Revision;
        public string Bus;

        [XmlIgnore]
        public bool ManufacturerSpecified;
    }

    public class NameValueStats
    {
        [XmlAttribute]
        public string name;
        [XmlText]
        public long Value;
    }

    public static class Statistics
    {
        public static Stats AllStats;
        public static Stats CurrentStats;

        public static void LoadStats()
        {
            if (File.Exists(Path.Combine(Settings.Settings.StatsPath, "Statistics.xml")))
            {
                AllStats = new Stats();
                CurrentStats = new Stats();

                XmlSerializer xs = new XmlSerializer(AllStats.GetType());
                StreamReader sr = new StreamReader(Path.Combine(Settings.Settings.StatsPath, "Statistics.xml"));
                AllStats = (Stats)xs.Deserialize(sr);
                sr.Close();
            }
            else if (Settings.Settings.Current.Stats != null)
            {
                AllStats = new Stats();
                CurrentStats = new Stats();
            }
            else
            {
                AllStats = null;
                CurrentStats = null;
            }
        }

        public static void SaveStats()
        {
            if (AllStats != null)
            {
                FileStream fs = new FileStream(Path.Combine(Settings.Settings.StatsPath, "Statistics.xml"), FileMode.Create);
                XmlSerializer xs = new XmlSerializer(AllStats.GetType());
                xs.Serialize(fs, AllStats);
                fs.Close();

                if (CurrentStats != null)
                {
                    string partial = string.Format("PartialStats_{0:yyyyMMddHHmmssfff}.xml", DateTime.UtcNow);

                    fs = new FileStream(Path.Combine(Settings.Settings.StatsPath, partial), FileMode.Create);
                    xs = new XmlSerializer(CurrentStats.GetType());
                    xs.Serialize(fs, CurrentStats);
                    fs.Close();
                }

                if (Settings.Settings.Current.Stats.ShareStats)
                    SubmitStats();
            }
        }

        public static void SubmitStats()
        {
            // TODO: Implement it
        }

        public static void AddCommand(string command)
        {
            if (Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.DeviceStats)
            {
                if (AllStats.Commands == null)
                    AllStats.Commands = new CommandsStats();

                if (CurrentStats.Commands == null)
                    CurrentStats.Commands = new CommandsStats();

                switch (command)
                {
                case "analyze":
                    AllStats.Commands.Analyze++;
                    CurrentStats.Commands.Analyze++;
                    break;
                case "benchmark":
                    AllStats.Commands.Benchmark++;
                    CurrentStats.Commands.Benchmark++;
                    break;
                case "checksum":
                    AllStats.Commands.Checksum++;
                    CurrentStats.Commands.Checksum++;
                    break;
                case "compare":
                    AllStats.Commands.Compare++;
                    CurrentStats.Commands.Compare++;
                    break;
                case "create-sidecar":
                    AllStats.Commands.CreateSidecar++;
                    CurrentStats.Commands.CreateSidecar++;
                    break;
                case "decode":
                    AllStats.Commands.Decode++;
                    CurrentStats.Commands.Decode++;
                    break;
                case "device-info":
                    AllStats.Commands.DeviceInfo++;
                    CurrentStats.Commands.DeviceInfo++;
                    break;
                case "device-report":
                    AllStats.Commands.DeviceReport++;
                    CurrentStats.Commands.DeviceReport++;
                    break;
                case "dump-media":
                    AllStats.Commands.DumpMedia++;
                    CurrentStats.Commands.DumpMedia++;
                    break;
                case "entropy":
                    AllStats.Commands.Entropy++;
                    CurrentStats.Commands.Entropy++;
                    break;
                case "formats":
                    AllStats.Commands.Formats++;
                    CurrentStats.Commands.Formats++;
                    break;
                case "media-info":
                    AllStats.Commands.MediaInfo++;
                    CurrentStats.Commands.MediaInfo++;
                    break;
                case "media-scan":
                    AllStats.Commands.MediaScan++;
                    CurrentStats.Commands.MediaScan++;
                    break;
                case "print-hex":
                    AllStats.Commands.PrintHex++;
                    CurrentStats.Commands.PrintHex++;
                    break;
                case "verify":
                    AllStats.Commands.Verify++;
                    CurrentStats.Commands.Verify++;
                    break;
                }
            }
        }

        public static void AddFilesystem(string filesystem)
        {
            if (Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.FilesystemStats)
            {
                if (AllStats.Filesystems == null)
                    AllStats.Filesystems = new List<NameValueStats>();
                if (CurrentStats.Filesystems == null)
                    CurrentStats.Filesystems = new List<NameValueStats>();

                NameValueStats old = null;
                foreach (NameValueStats nvs in AllStats.Filesystems)
                {
                    if (nvs.name == filesystem)
                    {
                        old = nvs;
                        break;
                    }
                }

                NameValueStats nw = new NameValueStats();
                if (old != null)
                {
                    nw.name = old.name;
                    nw.Value = old.Value + 1;
                    AllStats.Filesystems.Remove(old);
                }
                else
                {
                    nw.name = filesystem;
                    nw.Value = 1;
                }
                AllStats.Filesystems.Add(nw);

                old = null;
                foreach (NameValueStats nvs in CurrentStats.Filesystems)
                {
                    if (nvs.name == filesystem)
                    {
                        old = nvs;
                        break;
                    }
                }

                nw = new NameValueStats();
                if (old != null)
                {
                    nw.name = old.name;
                    nw.Value = old.Value + 1;
                    CurrentStats.Filesystems.Remove(old);
                }
                else
                {
                    nw.name = filesystem;
                    nw.Value = 1;
                }
                CurrentStats.Filesystems.Add(nw);
            }
        }

        public static void AddPartition(string partition)
        {
            if (Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.PartitionStats)
            {
                if (AllStats.Partitions == null)
                    AllStats.Partitions = new List<NameValueStats>();
                if (CurrentStats.Partitions == null)
                    CurrentStats.Partitions = new List<NameValueStats>();

                NameValueStats old = null;
                foreach (NameValueStats nvs in AllStats.Partitions)
                {
                    if (nvs.name == partition)
                    {
                        old = nvs;
                        break;
                    }
                }

                NameValueStats nw = new NameValueStats();
                if (old != null)
                {
                    nw.name = old.name;
                    nw.Value = old.Value + 1;
                    AllStats.Partitions.Remove(old);
                }
                else
                {
                    nw.name = partition;
                    nw.Value = 1;
                }
                AllStats.Partitions.Add(nw);

                old = null;
                foreach (NameValueStats nvs in CurrentStats.Partitions)
                {
                    if (nvs.name == partition)
                    {
                        old = nvs;
                        break;
                    }
                }

                nw = new NameValueStats();
                if (old != null)
                {
                    nw.name = old.name;
                    nw.Value = old.Value + 1;
                    CurrentStats.Partitions.Remove(old);
                }
                else
                {
                    nw.name = partition;
                    nw.Value = 1;
                }
                CurrentStats.Partitions.Add(nw);
            }
        }

        public static void AddMediaFormat(string format)
        {
            if (Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.MediaImageStats)
            {
                if (AllStats.MediaImages == null)
                    AllStats.MediaImages = new List<NameValueStats>();
                if (CurrentStats.MediaImages == null)
                    CurrentStats.MediaImages = new List<NameValueStats>();

                NameValueStats old = null;
                foreach (NameValueStats nvs in AllStats.MediaImages)
                {
                    if (nvs.name == format)
                    {
                        old = nvs;
                        break;
                    }
                }

                NameValueStats nw = new NameValueStats();
                if (old != null)
                {
                    nw.name = old.name;
                    nw.Value = old.Value + 1;
                    AllStats.MediaImages.Remove(old);
                }
                else
                {
                    nw.name = format;
                    nw.Value = 1;
                }
                AllStats.MediaImages.Add(nw);

                old = null;
                foreach (NameValueStats nvs in CurrentStats.MediaImages)
                {
                    if (nvs.name == format)
                    {
                        old = nvs;
                        break;
                    }
                }

                nw = new NameValueStats();
                if (old != null)
                {
                    nw.name = old.name;
                    nw.Value = old.Value + 1;
                    CurrentStats.MediaImages.Remove(old);
                }
                else
                {
                    nw.name = format;
                    nw.Value = 1;
                }
                CurrentStats.MediaImages.Add(nw);
            }
        }

        public static void AddDevice(Devices.Device dev)
        {
            if (Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.DeviceStats)
            {
                if (AllStats.Devices == null)
                    AllStats.Devices = new List<DeviceStats>();
                if (CurrentStats.Devices == null)
                    CurrentStats.Devices = new List<DeviceStats>();

                string deviceBus;
                if (dev.IsUSB)
                    deviceBus = "USB";
                else if (dev.IsFireWire)
                    deviceBus = "FireWire";
                else
                    deviceBus = dev.Type.ToString();

                DeviceStats old = null;
                foreach (DeviceStats ds in AllStats.Devices)
                {
                    if (ds.Manufacturer == dev.Manufacturer &&
                       ds.Model == dev.Model &&
                       ds.Revision == dev.Revision &&
                       ds.Bus == deviceBus)
                    {
                        old = ds;
                        break;
                    }
                }

                if (old != null)
                    AllStats.Devices.Remove(old);

                DeviceStats nw = new DeviceStats();
                nw.Model = dev.Model;
                nw.Manufacturer = dev.Manufacturer;
                nw.Revision = dev.Revision;
                nw.Bus = deviceBus;
                nw.ManufacturerSpecified = true;
                AllStats.Devices.Add(nw);

                old = null;
                foreach (DeviceStats ds in CurrentStats.Devices)
                {
                    if (ds.Manufacturer == dev.Manufacturer &&
                        ds.Model == dev.Model &&
                        ds.Revision == dev.Revision &&
                        ds.Bus == deviceBus)
                    {
                        old = ds;
                        break;
                    }
                }

                if (old != null)
                    CurrentStats.Devices.Remove(old);

                nw = new DeviceStats();
                nw.Model = dev.Model;
                nw.Manufacturer = dev.Manufacturer;
                nw.Revision = dev.Revision;
                nw.Bus = deviceBus;
                nw.ManufacturerSpecified = true;
                CurrentStats.Devices.Add(nw);
            }
        }

        public static void AddMedia(CommonTypes.MediaType type, bool real)
        {
            if (Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.MediaStats)
            {
                if (AllStats.Medias == null)
                    AllStats.Medias = new List<MediaStats>();
                if (CurrentStats.Medias == null)
                    CurrentStats.Medias = new List<MediaStats>();

                MediaStats old = null;
                foreach (MediaStats ms in AllStats.Medias)
                {
                    if (ms.real == real && ms.type == type.ToString())
                    {
                        old = ms;
                        break;
                    }
                }

                MediaStats nw = new MediaStats();
                if (old != null)
                {
                    nw.type = old.type;
                    nw.real = old.real;
                    nw.Value = old.Value + 1;
                    AllStats.Medias.Remove(old);
                }
                else
                {
                    nw.type = type.ToString();
                    nw.real = real;
                    nw.Value = 1;
                }
                AllStats.Medias.Add(nw);

                old = null;
                foreach (MediaStats ms in CurrentStats.Medias)
                {
                    if (ms.real == real && ms.type == type.ToString())
                    {
                        old = ms;
                        break;
                    }
                }

                nw = new MediaStats();
                if (old != null)
                {
                    nw.type = old.type;
                    nw.real = old.real;
                    nw.Value = old.Value + 1;
                    CurrentStats.Medias.Remove(old);
                }
                else
                {
                    nw.type = type.ToString();
                    nw.real = real;
                    nw.Value = 1;
                }
                CurrentStats.Medias.Add(nw);
            }
        }

        public static void AddBenchmark(Dictionary<string, double> checksums, double entropy, double all, double sequential, long maxMemory, long minMemory)
        {
            if (Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.BenchmarkStats)
            {
                CurrentStats.Benchmark = new BenchmarkStats();
                CurrentStats.Benchmark.Checksum = new List<ChecksumStats>();
                AllStats.Benchmark = new BenchmarkStats();
                AllStats.Benchmark.Checksum = new List<ChecksumStats>();

                foreach (KeyValuePair<string, double> kvp in checksums)
                {
                    ChecksumStats st = new ChecksumStats();
                    st.algorithm = kvp.Key;
                    st.Value = kvp.Value;
                    CurrentStats.Benchmark.Checksum.Add(st);
                    AllStats.Benchmark.Checksum.Add(st);
                }

                CurrentStats.Benchmark.All = all;
                CurrentStats.Benchmark.Entropy = entropy;
                CurrentStats.Benchmark.MaxMemory = maxMemory;
                CurrentStats.Benchmark.MinMemory = minMemory;
                CurrentStats.Benchmark.Sequential = sequential;

                AllStats.Benchmark.All = all;
                AllStats.Benchmark.Entropy = entropy;
                AllStats.Benchmark.MaxMemory = maxMemory;
                AllStats.Benchmark.MinMemory = minMemory;
                AllStats.Benchmark.Sequential = sequential;
            }
        }

        public static void AddVerify(bool? mediaVerified, long correct, long failed, long unknown, long total)
        {
            if (Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.VerifyStats)
            {
                if (CurrentStats.Verify == null)
                {
                    CurrentStats.Verify = new VerifyStats();
                    CurrentStats.Verify.MediaImages = new VerifiedItems();
                    CurrentStats.Verify.Sectors = new ScannedSectors();
                }

                if (AllStats.Verify == null)
                {
                    AllStats.Verify = new VerifyStats();
                    AllStats.Verify.MediaImages = new VerifiedItems();
                    AllStats.Verify.Sectors = new ScannedSectors();
                }

                if (mediaVerified.HasValue)
                {
                    if (mediaVerified.Value)
                    {
                        CurrentStats.Verify.MediaImages.Correct++;
                        AllStats.Verify.MediaImages.Correct++;
                    }
                    else
                    {
                        CurrentStats.Verify.MediaImages.Failed++;
                        AllStats.Verify.MediaImages.Failed++;
                    }
                }

                CurrentStats.Verify.Sectors.Correct += correct;
                CurrentStats.Verify.Sectors.Error += failed;
                CurrentStats.Verify.Sectors.Unverifiable += unknown;
                CurrentStats.Verify.Sectors.Total += total;

                AllStats.Verify.Sectors.Correct += correct;
                AllStats.Verify.Sectors.Error += failed;
                AllStats.Verify.Sectors.Unverifiable += unknown;
                AllStats.Verify.Sectors.Total += total;
            }
        }

        public static void AddMediaScan(long lessThan3ms, long lessThan10ms, long lessThan50ms, long lessThan150ms, long lessThan500ms, long moreThan500ms, long total, long error, long correct)
        {
            if (Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.MediaScanStats)
            {
                if (CurrentStats.MediaScan == null)
                {
                    CurrentStats.MediaScan = new MediaScanStats();
                    CurrentStats.MediaScan.Sectors = new ScannedSectors();
                    CurrentStats.MediaScan.Times = new TimeStats();
                }

                if (AllStats.MediaScan == null)
                {
                    AllStats.MediaScan = new MediaScanStats();
                    AllStats.MediaScan.Sectors = new ScannedSectors();
                    AllStats.MediaScan.Times = new TimeStats();
                }

                CurrentStats.MediaScan.Sectors.Correct += correct;
                CurrentStats.MediaScan.Sectors.Error += error;
                CurrentStats.MediaScan.Sectors.Total += total;
                CurrentStats.MediaScan.Times.LessThan3ms += lessThan3ms;
                CurrentStats.MediaScan.Times.LessThan10ms += lessThan10ms;
                CurrentStats.MediaScan.Times.LessThan50ms += lessThan50ms;
                CurrentStats.MediaScan.Times.LessThan150ms += lessThan150ms;
                CurrentStats.MediaScan.Times.LessThan500ms += lessThan500ms;
                CurrentStats.MediaScan.Times.MoreThan500ms += moreThan500ms;

                AllStats.MediaScan.Sectors.Correct += correct;
                AllStats.MediaScan.Sectors.Error += error;
                AllStats.MediaScan.Sectors.Total += total;
                AllStats.MediaScan.Times.LessThan3ms += lessThan3ms;
                AllStats.MediaScan.Times.LessThan10ms += lessThan10ms;
                AllStats.MediaScan.Times.LessThan50ms += lessThan50ms;
                AllStats.MediaScan.Times.LessThan150ms += lessThan150ms;
                AllStats.MediaScan.Times.LessThan500ms += lessThan500ms;
                AllStats.MediaScan.Times.MoreThan500ms += moreThan500ms;
            }
        }
    }
}

