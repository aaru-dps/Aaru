// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Statistics.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles usage statistics.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Database;
using DiscImageChef.Database.Models;
using Device = DiscImageChef.Devices.Device;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using OperatingSystem = DiscImageChef.Database.Models.OperatingSystem;
using Version = DiscImageChef.CommonTypes.Interop.Version;

namespace DiscImageChef.Core
{
    /// <summary>
    ///     Handles anonymous usage statistics
    /// </summary>
    public static class Statistics
    {
        /// <summary>
        ///     Contains all known statistics
        /// </summary>
        public static Stats AllStats;
        /// <summary>
        ///     Contains statistics of current execution
        /// </summary>
        public static Stats CurrentStats;
        /// <summary>
        ///     Statistics file semaphore
        /// </summary>
        static bool submitStatsLock;

        static DicContext ctx = new DicContext();

        /// <summary>
        ///     Loads saved statistics from disk
        /// </summary>
        public static void LoadStats()
        {
            if(File.Exists(Path.Combine(Settings.Settings.StatsPath, "Statistics.xml")))
            {
                AllStats = new Stats();
                ctx.OperatingSystems.Add(new OperatingSystem
                {
                    Name         = DetectOS.GetRealPlatformID().ToString(),
                    Synchronized = false,
                    Version      = DetectOS.GetVersion()
                });
                CurrentStats = new Stats
                {
                    Versions = new List<NameValueStats>
                    {
                        new NameValueStats {name = Version.GetVersion(), Value = 1}
                    }
                };
                XmlSerializer xs = new XmlSerializer(AllStats.GetType());
                StreamReader  sr = new StreamReader(Path.Combine(Settings.Settings.StatsPath, "Statistics.xml"));
                AllStats = (Stats)xs.Deserialize(sr);
                sr.Close();
            }
            else if(Settings.Settings.Current.Stats != null)
            {
                AllStats = new Stats();
                ctx.OperatingSystems.Add(new OperatingSystem
                {
                    Name         = DetectOS.GetRealPlatformID().ToString(),
                    Synchronized = false,
                    Version      = DetectOS.GetVersion()
                });
                CurrentStats = new Stats
                {
                    Versions = new List<NameValueStats>
                    {
                        new NameValueStats {name = Version.GetVersion(), Value = 1}
                    }
                };
            }
            else
            {
                AllStats     = null;
                CurrentStats = null;
            }
        }

        /// <summary>
        ///     Saves statistics to disk
        /// </summary>
        public static void SaveStats()
        {
            ctx.SaveChanges();

            if(AllStats == null) return;

            if(AllStats.OperatingSystems != null)
            {
                long count = 0;

                OsStats old = null;
                foreach(OsStats nvs in AllStats.OperatingSystems.Where(nvs =>
                                                                           nvs.name == DetectOS
                                                                                      .GetRealPlatformID().ToString() &&
                                                                           nvs.version == DetectOS.GetVersion()))
                {
                    count = nvs.Value + 1;
                    old   = nvs;
                    break;
                }

                if(old != null) AllStats.OperatingSystems.Remove(old);

                count++;
                AllStats.OperatingSystems.Add(new OsStats
                {
                    name    = DetectOS.GetRealPlatformID().ToString(),
                    Value   = count,
                    version = DetectOS.GetVersion()
                });
            }
            else if(CurrentStats != null) AllStats.OperatingSystems = CurrentStats.OperatingSystems;

            if(AllStats.Versions != null)
            {
                long count = 0;

                NameValueStats old = null;
                foreach(NameValueStats nvs in AllStats.Versions.Where(nvs => nvs.name == Version.GetVersion()))
                {
                    count = nvs.Value + 1;
                    old   = nvs;
                    break;
                }

                if(old != null) AllStats.Versions.Remove(old);

                count++;
                AllStats.Versions.Add(new NameValueStats {name = Version.GetVersion(), Value = count});
            }
            else if(CurrentStats != null) AllStats.Versions = CurrentStats.Versions;

            FileStream fs = new FileStream(Path.Combine(Settings.Settings.StatsPath, "Statistics.xml"),
                                           FileMode.Create);
            XmlSerializer xs = new XmlSerializer(AllStats.GetType());
            xs.Serialize(fs, AllStats);
            fs.Close();

            if(CurrentStats != null)
            {
                string partial = $"PartialStats_{DateTime.UtcNow:yyyyMMddHHmmssfff}.xml";

                fs = new FileStream(Path.Combine(Settings.Settings.StatsPath, partial), FileMode.Create);
                xs = new XmlSerializer(CurrentStats.GetType());
                xs.Serialize(fs, CurrentStats);
                fs.Close();
            }

            if(Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.ShareStats) SubmitStats();
        }

        /// <summary>
        ///     Submits statistics to DiscImageChef.Server
        /// </summary>
        public static void SubmitStats()
        {
            Thread submitThread = new Thread(() =>
            {
                if(submitStatsLock) return;

                submitStatsLock = true;

                IEnumerable<string> statsFiles =
                    Directory.EnumerateFiles(Settings.Settings.StatsPath, "PartialStats_*.xml",
                                             SearchOption.TopDirectoryOnly);

                foreach(string statsFile in statsFiles)
                    try
                    {
                        if(!File.Exists(statsFile)) continue;

                        Stats stats = new Stats();

                        // This can execute before debug console has been inited
                        #if DEBUG
                        System.Console.WriteLine("Uploading partial statistics file {0}", statsFile);
                        #else
                    DiscImageChef.Console.DicConsole.DebugWriteLine("Submit stats", "Uploading partial statistics file {0}", statsFile);
                        #endif

                        FileStream    fs = new FileStream(statsFile, FileMode.Open, FileAccess.Read);
                        XmlSerializer xs = new XmlSerializer(stats.GetType());
                        xs.Deserialize(fs); // Just to test validity of stats file
                        fs.Seek(0, SeekOrigin.Begin);

                        WebRequest request = WebRequest.Create("http://discimagechef.claunia.com/api/uploadstats");
                        ((HttpWebRequest)request).UserAgent =
                            $"DiscImageChef {typeof(Version).Assembly.GetName().Version}";
                        request.Method        = "POST";
                        request.ContentLength = fs.Length;
                        request.ContentType   = "application/xml";
                        Stream reqStream = request.GetRequestStream();
                        fs.CopyTo(reqStream);
                        reqStream.Close();
                        WebResponse response = request.GetResponse();

                        if(((HttpWebResponse)response).StatusCode != HttpStatusCode.OK) return;

                        Stream       data   = response.GetResponseStream();
                        StreamReader reader = new StreamReader(data ?? throw new InvalidOperationException());

                        string responseFromServer = reader.ReadToEnd();
                        data.Close();
                        response.Close();
                        fs.Close();
                        if(responseFromServer == "ok") File.Delete(statsFile);
                    }
                    catch(WebException)
                    {
                        // Can't connect to the server, postpone til next try
                        break;
                    }
                    catch
                    {
                        #if DEBUG
                        submitStatsLock = false;
                        throw;
                        #else
                        continue;
                        #endif
                    }

                submitStatsLock = false;
            });
            submitThread.Start();
        }

        /// <summary>
        ///     Adds the execution of a command to statistics
        /// </summary>
        /// <param name="command">Command</param>
        public static void AddCommand(string command)
        {
            if(string.IsNullOrWhiteSpace(command)) return;

            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.DeviceStats) return;

            ctx.Commands.Add(new Command {Name = command, Synchronized = false});
        }

        /// <summary>
        ///     Adds a new filesystem to statistics
        /// </summary>
        /// <param name="filesystem">Filesystem name</param>
        public static void AddFilesystem(string filesystem)
        {
            if(string.IsNullOrWhiteSpace(filesystem)) return;

            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.FilesystemStats) return;

            ctx.Filesystems.Add(new Filesystem {Name = filesystem, Synchronized = false});
        }

        /// <summary>
        ///     Adds a new partition scheme to statistics
        /// </summary>
        /// <param name="partition">Partition scheme name</param>
        internal static void AddPartition(string partition)
        {
            if(string.IsNullOrWhiteSpace(partition)) return;

            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.PartitionStats) return;

            ctx.Partitions.Add(new Partition {Name = partition, Synchronized = false});
        }

        /// <summary>
        ///     Adds a new filter to statistics
        /// </summary>
        /// <param name="filter">Filter name</param>
        public static void AddFilter(string filter)
        {
            if(string.IsNullOrWhiteSpace(filter)) return;

            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.FilterStats) return;

            ctx.Filters.Add(new Filter {Name = filter, Synchronized = false});

            if(AllStats.Filters     == null) AllStats.Filters     = new List<NameValueStats>();
            if(CurrentStats.Filters == null) CurrentStats.Filters = new List<NameValueStats>();

            NameValueStats old = AllStats.Filters.FirstOrDefault(nvs => nvs.name == filter);

            NameValueStats nw = new NameValueStats();
            if(old != null)
            {
                nw.name  = old.name;
                nw.Value = old.Value + 1;
                AllStats.Filters.Remove(old);
            }
            else
            {
                nw.name  = filter;
                nw.Value = 1;
            }

            AllStats.Filters.Add(nw);

            old = CurrentStats.Filters.FirstOrDefault(nvs => nvs.name == filter);

            nw = new NameValueStats();
            if(old != null)
            {
                nw.name  = old.name;
                nw.Value = old.Value + 1;
                CurrentStats.Filters.Remove(old);
            }
            else
            {
                nw.name  = filter;
                nw.Value = 1;
            }

            CurrentStats.Filters.Add(nw);
        }

        /// <summary>
        ///     Ads a new media image to statistics
        /// </summary>
        /// <param name="format">Media image name</param>
        public static void AddMediaFormat(string format)
        {
            if(string.IsNullOrWhiteSpace(format)) return;

            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.MediaImageStats) return;

            ctx.MediaFormats.Add(new MediaFormat {Name = format, Synchronized = false});
        }

        /// <summary>
        ///     Adds a new device to statistics
        /// </summary>
        /// <param name="dev">Device</param>
        public static void AddDevice(Device dev)
        {
            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.DeviceStats) return;

            if(AllStats.Devices     == null) AllStats.Devices     = new List<DeviceStats>();
            if(CurrentStats.Devices == null) CurrentStats.Devices = new List<DeviceStats>();

            string deviceBus;
            if(dev.IsUsb) deviceBus           = "USB";
            else if(dev.IsFireWire) deviceBus = "FireWire";
            else deviceBus                    = dev.Type.ToString();

            ctx.SeenDevices.Add(new DeviceStat
            {
                Bus          = deviceBus,
                Manufacturer = dev.Manufacturer,
                Model        = dev.Model,
                Revision     = dev.Revision,
                Synchronized = false
            });
        }

        /// <summary>
        ///     Adds a new media type to statistics
        /// </summary>
        /// <param name="type">Media type</param>
        /// <param name="real">Set if media was found on a real device, otherwise found on a media image</param>
        public static void AddMedia(MediaType type, bool real)
        {
            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.MediaStats) return;

            ctx.Medias.Add(new Database.Models.Media {Real = real, Synchronized = false, Type = type.ToString()});
        }

        /// <summary>
        ///     Adds benchmark results to statistics
        /// </summary>
        /// <param name="checksums">Checksum times</param>
        /// <param name="entropy">Entropy times</param>
        /// <param name="all">Time for all running togheter</param>
        /// <param name="sequential">Time for sequential running</param>
        /// <param name="maxMemory">Maximum used memory</param>
        /// <param name="minMemory">Minimum used memory</param>
        public static void AddBenchmark(Dictionary<string, double> checksums,  double entropy,   double all,
                                        double                     sequential, long   maxMemory, long   minMemory)
        {
            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.BenchmarkStats) return;

            CurrentStats.Benchmark = new BenchmarkStats {Checksum = new List<ChecksumStats>()};
            AllStats.Benchmark     = new BenchmarkStats {Checksum = new List<ChecksumStats>()};

            foreach(ChecksumStats st in checksums.Select(kvp => new ChecksumStats
            {
                algorithm = kvp.Key, Value = kvp.Value
            }))
            {
                CurrentStats.Benchmark.Checksum.Add(st);
                AllStats.Benchmark.Checksum.Add(st);
            }

            CurrentStats.Benchmark.All        = all;
            CurrentStats.Benchmark.Entropy    = entropy;
            CurrentStats.Benchmark.MaxMemory  = maxMemory;
            CurrentStats.Benchmark.MinMemory  = minMemory;
            CurrentStats.Benchmark.Sequential = sequential;

            AllStats.Benchmark.All        = all;
            AllStats.Benchmark.Entropy    = entropy;
            AllStats.Benchmark.MaxMemory  = maxMemory;
            AllStats.Benchmark.MinMemory  = minMemory;
            AllStats.Benchmark.Sequential = sequential;
        }

        /// <summary>
        ///     Adds a new media image verification to statistics
        /// </summary>
        /// <param name="mediaVerified">Set if media was correctly verified</param>
        /// <param name="correct">How many sectors where verified correctly</param>
        /// <param name="failed">How many sectors failed verification</param>
        /// <param name="unknown">How many sectors could not be verified</param>
        /// <param name="total">Total sectors verified</param>
        public static void AddVerify(bool? mediaVerified, long correct, long failed, long unknown, long total)
        {
            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.VerifyStats) return;

            if(CurrentStats.Verify == null)
                CurrentStats.Verify =
                    new VerifyStats {MediaImages = new VerifiedItems(), Sectors = new ScannedSectors()};

            if(AllStats.Verify == null)
                AllStats.Verify = new VerifyStats {MediaImages = new VerifiedItems(), Sectors = new ScannedSectors()};

            if(mediaVerified.HasValue)
                if(mediaVerified.Value)
                {
                    CurrentStats.Verify.MediaImages.Correct++;
                    AllStats.Verify.MediaImages.Correct++;
                }
                else
                {
                    CurrentStats.Verify.MediaImages.Failed++;
                    AllStats.Verify.MediaImages.Failed++;
                }

            CurrentStats.Verify.Sectors.Correct      += correct;
            CurrentStats.Verify.Sectors.Error        += failed;
            CurrentStats.Verify.Sectors.Unverifiable += unknown;
            CurrentStats.Verify.Sectors.Total        += total;

            AllStats.Verify.Sectors.Correct      += correct;
            AllStats.Verify.Sectors.Error        += failed;
            AllStats.Verify.Sectors.Unverifiable += unknown;
            AllStats.Verify.Sectors.Total        += total;
        }

        /// <summary>
        ///     Adds a new media scan to statistics
        /// </summary>
        /// <param name="lessThan3Ms">Sectors &lt;3ms</param>
        /// <param name="lessThan10Ms">Sectors &gt;3ms and &lt;10ms</param>
        /// <param name="lessThan50Ms">Sectors &gt;10ms and &lt;50ms</param>
        /// <param name="lessThan150Ms">Sectors &gt;50ms and &lt;150ms</param>
        /// <param name="lessThan500Ms">Sectors &gt;150ms and &lt;500ms</param>
        /// <param name="moreThan500Ms">Sectors &gt;500ms</param>
        /// <param name="total">Total sectors</param>
        /// <param name="error">Errored sectors</param>
        /// <param name="correct">Correct sectors</param>
        public static void AddMediaScan(long lessThan3Ms,   long lessThan10Ms,  long lessThan50Ms, long lessThan150Ms,
                                        long lessThan500Ms, long moreThan500Ms, long total,        long error,
                                        long correct)
        {
            if(lessThan3Ms   < 0) throw new ArgumentOutOfRangeException(nameof(lessThan3Ms));
            if(lessThan10Ms  < 0) throw new ArgumentOutOfRangeException(nameof(lessThan10Ms));
            if(lessThan50Ms  < 0) throw new ArgumentOutOfRangeException(nameof(lessThan50Ms));
            if(lessThan150Ms < 0) throw new ArgumentOutOfRangeException(nameof(lessThan150Ms));
            if(lessThan500Ms < 0) throw new ArgumentOutOfRangeException(nameof(lessThan500Ms));
            if(moreThan500Ms < 0) throw new ArgumentOutOfRangeException(nameof(moreThan500Ms));
            if(total         < 0) throw new ArgumentOutOfRangeException(nameof(total));
            if(error         < 0) throw new ArgumentOutOfRangeException(nameof(error));
            if(correct       < 0) throw new ArgumentOutOfRangeException(nameof(correct));

            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.MediaScanStats) return;

            if(CurrentStats.MediaScan == null)
                CurrentStats.MediaScan = new MediaScanStats {Sectors = new ScannedSectors(), Times = new TimeStats()};

            if(AllStats.MediaScan == null)
                AllStats.MediaScan = new MediaScanStats {Sectors = new ScannedSectors(), Times = new TimeStats()};

            CurrentStats.MediaScan.Sectors.Correct     += correct;
            CurrentStats.MediaScan.Sectors.Error       += error;
            CurrentStats.MediaScan.Sectors.Total       += total;
            CurrentStats.MediaScan.Times.LessThan3ms   += lessThan3Ms;
            CurrentStats.MediaScan.Times.LessThan10ms  += lessThan10Ms;
            CurrentStats.MediaScan.Times.LessThan50ms  += lessThan50Ms;
            CurrentStats.MediaScan.Times.LessThan150ms += lessThan150Ms;
            CurrentStats.MediaScan.Times.LessThan500ms += lessThan500Ms;
            CurrentStats.MediaScan.Times.MoreThan500ms += moreThan500Ms;

            AllStats.MediaScan.Sectors.Correct     += correct;
            AllStats.MediaScan.Sectors.Error       += error;
            AllStats.MediaScan.Sectors.Total       += total;
            AllStats.MediaScan.Times.LessThan3ms   += lessThan3Ms;
            AllStats.MediaScan.Times.LessThan10ms  += lessThan10Ms;
            AllStats.MediaScan.Times.LessThan50ms  += lessThan50Ms;
            AllStats.MediaScan.Times.LessThan150ms += lessThan150Ms;
            AllStats.MediaScan.Times.LessThan500ms += lessThan500Ms;
            AllStats.MediaScan.Times.MoreThan500ms += moreThan500Ms;
        }
    }
}