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
using Version = DiscImageChef.Database.Models.Version;

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
                ctx.Versions.Add(new Version {Value = CommonTypes.Interop.Version.GetVersion(), Synchronized = false});
                CurrentStats = new Stats();
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
                ctx.Versions.Add(new Version {Value = CommonTypes.Interop.Version.GetVersion(), Synchronized = false});
                CurrentStats = new Stats();
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
                foreach(NameValueStats nvs in AllStats.Versions.Where(nvs => nvs.name == CommonTypes
                                                                                        .Interop.Version.GetVersion()))
                {
                    count = nvs.Value + 1;
                    old   = nvs;
                    break;
                }

                if(old != null) AllStats.Versions.Remove(old);

                count++;
                AllStats.Versions.Add(new NameValueStats
                {
                    name = CommonTypes.Interop.Version.GetVersion(), Value = count
                });
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
                            $"DiscImageChef {typeof(CommonTypes.Interop.Version).Assembly.GetName().Version}";
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
    }
}