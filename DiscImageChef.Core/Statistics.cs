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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Database;
using DiscImageChef.Database.Models;
using Newtonsoft.Json;
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
        ///     Statistics file semaphore
        /// </summary>
        static bool submitStatsLock;

        /// <summary>
        ///     Loads saved statistics from disk
        /// </summary>
        public static void LoadStats()
        {
            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);

            if(File.Exists(Path.Combine(Settings.Settings.StatsPath, "Statistics.xml")))
                try
                {
                    Stats         allStats = new Stats();
                    XmlSerializer xs       = new XmlSerializer(allStats.GetType());
                    StreamReader sr =
                        new StreamReader(Path.Combine(Settings.Settings.StatsPath, "Statistics.xml"));
                    allStats = (Stats)xs.Deserialize(sr);
                    sr.Close();

                    if(allStats.Commands != null)
                    {
                        if(allStats.Commands.Analyze > 0)
                        {
                            Command command = ctx.Commands.FirstOrDefault(c => c.Name == "analyze" && c.Synchronized) ??
                                              new Command {Name = "analyze", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.Analyze;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.Benchmark > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "benchmark" && c.Synchronized) ??
                                new Command {Name = "benchmark", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.Benchmark;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.Checksum > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "checksum" && c.Synchronized) ??
                                new Command {Name = "checksum", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.Checksum;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.Compare > 0)
                        {
                            Command command = ctx.Commands.FirstOrDefault(c => c.Name == "compare" && c.Synchronized) ??
                                              new Command {Name = "compare", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.Compare;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.ConvertImage > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "convert-image" && c.Synchronized) ??
                                new Command {Name = "convert-image", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.ConvertImage;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.CreateSidecar > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "create-sidecar" && c.Synchronized) ??
                                new Command {Name = "create-sidecar", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.CreateSidecar;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.Decode > 0)
                        {
                            Command command = ctx.Commands.FirstOrDefault(c => c.Name == "decode" && c.Synchronized) ??
                                              new Command {Name = "decode", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.Decode;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.DeviceInfo > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "device-info" && c.Synchronized) ??
                                new Command {Name = "device-info", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.DeviceInfo;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.DeviceReport > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "device-report" && c.Synchronized) ??
                                new Command {Name = "device-report", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.DeviceReport;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.DumpMedia > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "dump-media" && c.Synchronized) ??
                                new Command {Name = "dump-media", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.DumpMedia;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.Entropy > 0)
                        {
                            Command command = ctx.Commands.FirstOrDefault(c => c.Name == "entropy" && c.Synchronized) ??
                                              new Command {Name = "entropy", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.Entropy;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.ExtractFiles > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "extract-files" && c.Synchronized) ??
                                new Command {Name = "extract-files", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.ExtractFiles;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.Formats > 0)
                        {
                            Command command = ctx.Commands.FirstOrDefault(c => c.Name == "formats" && c.Synchronized) ??
                                              new Command {Name = "formats", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.Formats;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.ImageInfo > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "image-info" && c.Synchronized) ??
                                new Command {Name = "image-info", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.ImageInfo;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.ListDevices > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "list-devices" && c.Synchronized) ??
                                new Command {Name = "list-devices", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.ListDevices;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.ListEncodings > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "list-encodings" && c.Synchronized) ??
                                new Command {Name = "list-encodings", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.ListEncodings;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.Ls > 0)
                        {
                            Command command = ctx.Commands.FirstOrDefault(c => c.Name == "ls" && c.Synchronized) ??
                                              new Command {Name = "ls", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.Ls;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.MediaInfo > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "media-info" && c.Synchronized) ??
                                new Command {Name = "media-info", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.MediaInfo;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.MediaScan > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "media-scan" && c.Synchronized) ??
                                new Command {Name = "media-scan", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.MediaScan;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.PrintHex > 0)
                        {
                            Command command =
                                ctx.Commands.FirstOrDefault(c => c.Name == "printhex" && c.Synchronized) ??
                                new Command {Name = "printhex", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.PrintHex;
                            ctx.Commands.Update(command);
                        }

                        if(allStats.Commands.Verify > 0)
                        {
                            Command command = ctx.Commands.FirstOrDefault(c => c.Name == "verify" && c.Synchronized) ??
                                              new Command {Name = "verify", Synchronized = true};

                            command.Count += (ulong)allStats.Commands.Verify;
                            ctx.Commands.Update(command);
                        }
                    }

                    if(allStats.OperatingSystems != null)
                        foreach(OsStats operatingSystem in allStats.OperatingSystems)
                        {
                            if(string.IsNullOrWhiteSpace(operatingSystem.name) ||
                               string.IsNullOrWhiteSpace(operatingSystem.version)) continue;

                            OperatingSystem existing =
                                ctx.OperatingSystems.FirstOrDefault(c => c.Name    == operatingSystem.name    &&
                                                                         c.Version == operatingSystem.version &&
                                                                         c.Synchronized) ?? new OperatingSystem
                                {
                                    Name         = operatingSystem.name,
                                    Version      = operatingSystem.version,
                                    Synchronized = true
                                };

                            existing.Count += (ulong)operatingSystem.Value;
                            ctx.OperatingSystems.Update(existing);
                        }

                    if(allStats.Versions != null)
                        foreach(NameValueStats nvs in allStats.Versions)
                        {
                            if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                            Version existing =
                                ctx.Versions.FirstOrDefault(c => c.Value == nvs.name && c.Synchronized) ??
                                new Version {Value = nvs.name, Synchronized = true};

                            existing.Count += (ulong)nvs.Value;
                            ctx.Versions.Update(existing);
                        }

                    if(allStats.Filesystems != null)
                        foreach(NameValueStats nvs in allStats.Filesystems)
                        {
                            if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                            Filesystem existing =
                                ctx.Filesystems.FirstOrDefault(c => c.Name == nvs.name && c.Synchronized) ??
                                new Filesystem {Name = nvs.name, Synchronized = true};

                            existing.Count += (ulong)nvs.Value;
                            ctx.Filesystems.Update(existing);
                        }

                    if(allStats.Partitions != null)
                        foreach(NameValueStats nvs in allStats.Partitions)
                        {
                            if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                            Partition existing =
                                ctx.Partitions.FirstOrDefault(c => c.Name == nvs.name && c.Synchronized) ??
                                new Partition {Name = nvs.name, Synchronized = true};

                            existing.Count += (ulong)nvs.Value;
                            ctx.Partitions.Update(existing);
                        }

                    if(allStats.Filesystems != null)
                        foreach(NameValueStats nvs in allStats.Filesystems)
                        {
                            if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                            Filesystem existing =
                                ctx.Filesystems.FirstOrDefault(c => c.Name == nvs.name && c.Synchronized) ??
                                new Filesystem {Name = nvs.name, Synchronized = true};

                            existing.Count += (ulong)nvs.Value;
                            ctx.Filesystems.Update(existing);
                        }

                    if(allStats.MediaImages != null)
                        foreach(NameValueStats nvs in allStats.MediaImages)
                        {
                            if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                            MediaFormat existing =
                                ctx.MediaFormats.FirstOrDefault(c => c.Name == nvs.name && c.Synchronized) ??
                                new MediaFormat {Name = nvs.name, Synchronized = true};

                            existing.Count += (ulong)nvs.Value;
                            ctx.MediaFormats.Update(existing);
                        }

                    if(allStats.Filters != null)
                        foreach(NameValueStats nvs in allStats.Filters)
                        {
                            if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                            Filter existing = ctx.Filters.FirstOrDefault(c => c.Name == nvs.name && c.Synchronized) ??
                                              new Filter {Name = nvs.name, Synchronized = true};

                            existing.Count += (ulong)nvs.Value;
                            ctx.Filters.Update(existing);
                        }

                    if(allStats.Devices != null)
                        foreach(DeviceStats device in allStats.Devices)
                        {
                            if(ctx.SeenDevices.Any(d => d.Manufacturer == device.Manufacturer &&
                                                        d.Model        == device.Model        &&
                                                        d.Revision     == device.Revision     &&
                                                        d.Bus          == device.Bus)) continue;

                            ctx.SeenDevices.Add(new DeviceStat
                            {
                                Bus          = device.Bus,
                                Manufacturer = device.Manufacturer,
                                Model        = device.Model,
                                Revision     = device.Revision,
                                Synchronized = true
                            });
                        }

                    if(allStats.Medias != null)
                        foreach(MediaStats media in allStats.Medias)
                        {
                            if(string.IsNullOrWhiteSpace(media.type)) continue;

                            Database.Models.Media existing =
                                ctx.Medias.FirstOrDefault(c => c.Type == media.type && c.Real == media.real &&
                                                               c.Synchronized) ?? new Database.Models.Media
                                {
                                    Type = media.type, Real = media.real, Synchronized = true
                                };

                            existing.Count += (ulong)media.Value;
                            ctx.Medias.Update(existing);
                        }

                    ctx.SaveChanges();
                    File.Delete(Path.Combine(Settings.Settings.StatsPath, "Statistics.xml"));
                }
                catch
                {
                    // Do not care about it
                }

            if(Settings.Settings.Current.Stats == null) return;

            ctx.OperatingSystems.Add(new OperatingSystem
            {
                Name         = DetectOS.GetRealPlatformID().ToString(),
                Synchronized = false,
                Version      = DetectOS.GetVersion(),
                Count        = 1
            });

            ctx.Versions.Add(new Version
            {
                Value = CommonTypes.Interop.Version.GetVersion(), Synchronized = false, Count = 1
            });
            ctx.SaveChanges();
        }

        /// <summary>
        ///     Saves statistics to disk
        /// </summary>
        public static void SaveStats()
        {
            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);
            ctx.SaveChanges();
            if(Settings.Settings.Current.Stats != null && Settings.Settings.Current.Stats.ShareStats) SubmitStats();
        }

        /// <summary>
        ///     Submits statistics to DiscImageChef.Server
        /// </summary>
        public static void SubmitStats()
        {
            Thread submitThread = new Thread(() =>
            {
                DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);

                if(ctx.Commands.Any(c => !c.Synchronized)    || ctx.Filesystems.Any(c => !c.Synchronized)      ||
                   ctx.Filters.Any(c => !c.Synchronized)     || ctx.MediaFormats.Any(c => !c.Synchronized)     ||
                   ctx.Partitions.Any(c => !c.Synchronized)  || ctx.Medias.Any(c => !c.Synchronized)           ||
                   ctx.SeenDevices.Any(c => !c.Synchronized) || ctx.OperatingSystems.Any(c => !c.Synchronized) ||
                   ctx.Versions.Any(c => !c.Synchronized))
                {
                    StatsDto dto = new StatsDto();

                    if(ctx.Commands.Any(c => !c.Synchronized))
                    {
                        dto.Commands = new List<NameValueStats>();
                        foreach(string nvs in ctx.Commands.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                            dto.Commands.Add(new NameValueStats
                            {
                                name = nvs,
                                Value = ctx.Commands.LongCount(c => !c.Synchronized &&
                                                                    c.Name == nvs)
                            });
                    }

                    if(ctx.Filesystems.Any(c => !c.Synchronized))
                    {
                        dto.Filesystems = new List<NameValueStats>();
                        foreach(string nvs in ctx.Filesystems.Where(c => !c.Synchronized).Select(c => c.Name).Distinct()
                        )
                            dto.Filesystems.Add(new NameValueStats
                            {
                                name = nvs,
                                Value = ctx.Filesystems.LongCount(c => !c.Synchronized &&
                                                                       c.Name == nvs)
                            });
                    }

                    if(ctx.Filters.Any(c => !c.Synchronized))
                    {
                        dto.Filters = new List<NameValueStats>();
                        foreach(string nvs in ctx.Filters.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                            dto.Filters.Add(new NameValueStats
                            {
                                name  = nvs,
                                Value = ctx.Filters.LongCount(c => !c.Synchronized && c.Name == nvs)
                            });
                    }

                    if(ctx.MediaFormats.Any(c => !c.Synchronized))
                    {
                        dto.MediaFormats = new List<NameValueStats>();
                        foreach(string nvs in ctx.Commands.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                            dto.MediaFormats.Add(new NameValueStats
                            {
                                name = nvs,
                                Value = ctx.MediaFormats.LongCount(c => !c.Synchronized &&
                                                                        c.Name == nvs)
                            });
                    }

                    if(ctx.Partitions.Any(c => !c.Synchronized))
                    {
                        dto.Partitions = new List<NameValueStats>();
                        foreach(string nvs in ctx.Partitions.Where(c => !c.Synchronized).Select(c => c.Name).Distinct())
                            dto.Partitions.Add(new NameValueStats
                            {
                                name = nvs,
                                Value = ctx.Partitions.LongCount(c => !c.Synchronized &&
                                                                      c.Name == nvs)
                            });
                    }

                    if(ctx.Versions.Any(c => !c.Synchronized))
                    {
                        dto.Versions = new List<NameValueStats>();
                        foreach(string nvs in ctx.Versions.Where(c => !c.Synchronized).Select(c => c.Value).Distinct())
                            dto.Versions.Add(new NameValueStats
                            {
                                name = nvs,
                                Value = ctx.Versions.LongCount(c => !c.Synchronized &&
                                                                    c.Value == nvs)
                            });
                    }

                    if(ctx.Medias.Any(c => !c.Synchronized))
                    {
                        dto.Medias = new List<MediaStats>();
                        foreach(string media in ctx.Medias.Where(c => !c.Synchronized).Select(c => c.Type).Distinct())
                        {
                            if(ctx.Medias.Any(c => !c.Synchronized && c.Type == media && c.Real))
                                dto.Medias.Add(new MediaStats
                                {
                                    real = true,
                                    type = media,
                                    Value = ctx.Medias.LongCount(c => !c.Synchronized &&
                                                                      c.Type == media && c.Real)
                                });
                            if(ctx.Medias.Any(c => !c.Synchronized && c.Type == media && !c.Real))
                                dto.Medias.Add(new MediaStats
                                {
                                    real = false,
                                    type = media,
                                    Value = ctx.Medias.LongCount(c => !c.Synchronized &&
                                                                      c.Type == media && !c.Real)
                                });
                        }
                    }

                    if(ctx.SeenDevices.Any(c => !c.Synchronized))
                    {
                        dto.Devices = new List<DeviceStats>();
                        foreach(DeviceStat device in ctx.SeenDevices.Where(c => !c.Synchronized))
                            dto.Devices.Add(new DeviceStats
                            {
                                Bus                   = device.Bus,
                                Manufacturer          = device.Manufacturer,
                                ManufacturerSpecified = !(device.Manufacturer is null),
                                Model                 = device.Model,
                                Revision              = device.Revision
                            });
                    }

                    if(ctx.OperatingSystems.Any(c => !c.Synchronized))
                    {
                        dto.OperatingSystems = new List<OsStats>();
                        foreach(string osName in ctx.OperatingSystems.Where(c => !c.Synchronized).Select(c => c.Name)
                                                    .Distinct())
                        {
                            foreach(string osVersion in
                                ctx.OperatingSystems.Where(c => !c.Synchronized && c.Name == osName)
                                   .Select(c => c.Version).Distinct())
                                dto.OperatingSystems.Add(new OsStats
                                {
                                    name    = osName,
                                    version = osVersion,
                                    Value =
                                        ctx.OperatingSystems.LongCount(c =>
                                                                           !c
                                                                              .Synchronized &&
                                                                           c.Name ==
                                                                           osName &&
                                                                           c.Version ==
                                                                           osVersion)
                                });
                        }
                    }

                    try
                    {
                        #if DEBUG
                        System.Console.WriteLine("Uploading statistics");
                        #else
                            DiscImageChef.Console.DicConsole.DebugWriteLine("Submit stats", "Uploading statistics");
                        #endif

                        string json = JsonConvert.SerializeObject(dto, Formatting.Indented,
                                                                  new JsonSerializerSettings
                                                                  {
                                                                      NullValueHandling = NullValueHandling.Ignore
                                                                  });
                        byte[]     jsonBytes = Encoding.UTF8.GetBytes(json);
                        WebRequest request   = WebRequest.Create("http://discimagechef.claunia.com/api/uploadstatsv2");
                        ((HttpWebRequest)request).UserAgent =
                            $"DiscImageChef {typeof(Version).Assembly.GetName().Version}";
                        request.Method        = "POST";
                        request.ContentLength = jsonBytes.Length;
                        request.ContentType   = "application/json";
                        Stream reqStream = request.GetRequestStream();
                        reqStream.Write(jsonBytes, 0, jsonBytes.Length);
                        //jsonStream.CopyTo(reqStream);
                        reqStream.Close();
                        WebResponse response = request.GetResponse();

                        if(((HttpWebResponse)response).StatusCode != HttpStatusCode.OK) return;

                        Stream       data   = response.GetResponseStream();
                        StreamReader reader = new StreamReader(data ?? throw new InvalidOperationException());

                        string result = reader.ReadToEnd();
                        data.Close();
                        response.Close();

                        if(result != "ok") return;

                        if(ctx.Commands.Any(c => !c.Synchronized))
                            foreach(string nvs in ctx.Commands.Where(c => !c.Synchronized).Select(c => c.Name)
                                                     .Distinct())
                            {
                                Command existing = ctx.Commands.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                                   new Command {Name = nvs, Synchronized = true};

                                existing.Count += (ulong)ctx.Commands.LongCount(c => !c.Synchronized && c.Name == nvs);
                                ctx.Commands.Update(existing);
                                ctx.Commands.RemoveRange(ctx.Commands.Where(c => !c.Synchronized && c.Name == nvs));
                            }

                        if(ctx.Filesystems.Any(c => !c.Synchronized))
                            foreach(string nvs in ctx.Filesystems.Where(c => !c.Synchronized).Select(c => c.Name)
                                                     .Distinct())
                            {
                                Filesystem existing =
                                    ctx.Filesystems.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                    new Filesystem {Name = nvs, Synchronized = true};

                                existing.Count +=
                                    (ulong)ctx.Filesystems.LongCount(c => !c.Synchronized && c.Name == nvs);
                                ctx.Filesystems.Update(existing);
                                ctx.Filesystems.RemoveRange(ctx.Filesystems.Where(c => !c.Synchronized &&
                                                                                       c.Name == nvs));
                            }

                        if(ctx.Filters.Any(c => !c.Synchronized))
                            foreach(string nvs in ctx.Filters.Where(c => !c.Synchronized).Select(c => c.Name).Distinct()
                            )
                            {
                                Filter existing = ctx.Filters.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                                  new Filter {Name = nvs, Synchronized = true};

                                existing.Count += (ulong)ctx.Filters.LongCount(c => !c.Synchronized && c.Name == nvs);
                                ctx.Filters.Update(existing);
                                ctx.Filters.RemoveRange(ctx.Filters.Where(c => !c.Synchronized && c.Name == nvs));
                            }

                        if(ctx.MediaFormats.Any(c => !c.Synchronized))
                            foreach(string nvs in ctx.MediaFormats.Where(c => !c.Synchronized).Select(c => c.Name)
                                                     .Distinct())
                            {
                                MediaFormat existing =
                                    ctx.MediaFormats.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                    new MediaFormat {Name = nvs, Synchronized = true};

                                existing.Count +=
                                    (ulong)ctx.MediaFormats.LongCount(c => !c.Synchronized && c.Name == nvs);
                                ctx.MediaFormats.Update(existing);
                                ctx.MediaFormats.RemoveRange(ctx.MediaFormats.Where(c => !c.Synchronized &&
                                                                                         c.Name == nvs));
                            }

                        if(ctx.Partitions.Any(c => !c.Synchronized))
                            foreach(string nvs in ctx.Partitions.Where(c => !c.Synchronized).Select(c => c.Name)
                                                     .Distinct())
                            {
                                Partition existing =
                                    ctx.Partitions.FirstOrDefault(c => c.Synchronized && c.Name == nvs) ??
                                    new Partition {Name = nvs, Synchronized = true};

                                existing.Count +=
                                    (ulong)ctx.Partitions.LongCount(c => !c.Synchronized && c.Name == nvs);
                                ctx.Partitions.Update(existing);
                                ctx.Partitions.RemoveRange(ctx.Partitions.Where(c => !c.Synchronized && c.Name == nvs));
                            }

                        if(ctx.Versions.Any(c => !c.Synchronized))
                            foreach(string nvs in ctx.Versions.Where(c => !c.Synchronized).Select(c => c.Value)
                                                     .Distinct())
                            {
                                Version existing = ctx.Versions.FirstOrDefault(c => c.Synchronized && c.Value == nvs) ??
                                                   new Version {Value = nvs, Synchronized = true};

                                existing.Count += (ulong)ctx.Versions.LongCount(c => !c.Synchronized && c.Value == nvs);
                                ctx.Versions.Update(existing);
                                ctx.Versions.RemoveRange(ctx.Versions.Where(c => !c.Synchronized && c.Value == nvs));
                            }

                        if(ctx.Medias.Any(c => !c.Synchronized))
                            foreach(string media in ctx.Medias.Where(c => !c.Synchronized).Select(c => c.Type)
                                                       .Distinct())
                            {
                                if(ctx.Medias.Any(c => !c.Synchronized && c.Type == media && c.Real))
                                {
                                    Database.Models.Media existing =
                                        ctx.Medias.FirstOrDefault(c => c.Synchronized && c.Type == media && c.Real) ??
                                        new Database.Models.Media {Synchronized = true, Type = media, Real = true};

                                    existing.Count +=
                                        (ulong)ctx.Medias.LongCount(c => !c.Synchronized && c.Type == media && c.Real);
                                    ctx.Medias.Update(existing);
                                    ctx.Medias.RemoveRange(ctx.Medias.Where(c => !c.Synchronized && c.Type == media &&
                                                                                 c.Real));
                                }

                                if(!ctx.Medias.Any(c => !c.Synchronized && c.Type == media && !c.Real)) continue;

                                {
                                    Database.Models.Media existing =
                                        ctx.Medias.FirstOrDefault(c => c.Synchronized && c.Type == media && !c.Real) ??
                                        new Database.Models.Media {Synchronized = true, Type = media, Real = false};

                                    existing.Count +=
                                        (ulong)ctx.Medias.LongCount(c => !c.Synchronized && c.Type == media && !c.Real);
                                    ctx.Medias.Update(existing);
                                    ctx.Medias.RemoveRange(ctx.Medias.Where(c => !c.Synchronized && c.Type == media &&
                                                                                 !c.Real));
                                }
                            }

                        if(ctx.SeenDevices.Any(c => !c.Synchronized))
                            foreach(DeviceStat device in ctx.SeenDevices.Where(c => !c.Synchronized))
                            {
                                device.Synchronized = true;
                                ctx.Update(device);
                            }

                        if(ctx.OperatingSystems.Any(c => !c.Synchronized))
                            foreach(string osName in ctx.OperatingSystems.Where(c => !c.Synchronized)
                                                        .Select(c => c.Name).Distinct())
                            {
                                foreach(string osVersion in ctx
                                                           .OperatingSystems
                                                           .Where(c => !c.Synchronized && c.Name == osName)
                                                           .Select(c => c.Version).Distinct())
                                {
                                    OperatingSystem existing =
                                        ctx.OperatingSystems.FirstOrDefault(c => c.Synchronized && c.Name == osName &&
                                                                                 c.Version ==
                                                                                 osVersion) ??
                                        new OperatingSystem {Synchronized = true, Version = osVersion, Name = osName};

                                    existing.Count +=
                                        (ulong)ctx.OperatingSystems.LongCount(c => !c.Synchronized     &&
                                                                                   c.Name    == osName &&
                                                                                   c.Version == osVersion);
                                    ctx.OperatingSystems.Update(existing);
                                    ctx.OperatingSystems.RemoveRange(ctx.OperatingSystems.Where(c => !c.Synchronized  &&
                                                                                                     c.Name == osName &&
                                                                                                     c.Version ==
                                                                                                     osVersion));
                                }
                            }
                    }
                    catch(WebException)
                    {
                        // Can't connect to the server, do nothing
                    }
                    // ReSharper disable once RedundantCatchClause
                    catch
                    {
                        #if DEBUG
                        if(Debugger.IsAttached) throw;
                        #endif
                    }
                }

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
                        if(!Debugger.IsAttached) continue;

                        submitStatsLock = false;
                        throw;
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

            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);
            ctx.Commands.Add(new Command {Name = command, Synchronized = false, Count = 1});
            ctx.SaveChanges();
        }

        /// <summary>
        ///     Adds a new filesystem to statistics
        /// </summary>
        /// <param name="filesystem">Filesystem name</param>
        public static void AddFilesystem(string filesystem)
        {
            if(string.IsNullOrWhiteSpace(filesystem)) return;

            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.FilesystemStats) return;

            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);
            ctx.Filesystems.Add(new Filesystem {Name = filesystem, Synchronized = false, Count = 1});
            ctx.SaveChanges();
        }

        /// <summary>
        ///     Adds a new partition scheme to statistics
        /// </summary>
        /// <param name="partition">Partition scheme name</param>
        internal static void AddPartition(string partition)
        {
            if(string.IsNullOrWhiteSpace(partition)) return;

            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.PartitionStats) return;

            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);
            ctx.Partitions.Add(new Partition {Name = partition, Synchronized = false, Count = 1});
            ctx.SaveChanges();
        }

        /// <summary>
        ///     Adds a new filter to statistics
        /// </summary>
        /// <param name="filter">Filter name</param>
        public static void AddFilter(string filter)
        {
            if(string.IsNullOrWhiteSpace(filter)) return;

            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.FilterStats) return;

            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);
            ctx.Filters.Add(new Filter {Name = filter, Synchronized = false, Count = 1});
            ctx.SaveChanges();
        }

        /// <summary>
        ///     Ads a new media image to statistics
        /// </summary>
        /// <param name="format">Media image name</param>
        public static void AddMediaFormat(string format)
        {
            if(string.IsNullOrWhiteSpace(format)) return;

            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.MediaImageStats) return;

            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);
            ctx.MediaFormats.Add(new MediaFormat {Name = format, Synchronized = false, Count = 1});
            ctx.SaveChanges();
        }

        /// <summary>
        ///     Adds a new device to statistics
        /// </summary>
        /// <param name="dev">Device</param>
        public static void AddDevice(Device dev)
        {
            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.DeviceStats) return;

            string deviceBus;
            if(dev.IsUsb) deviceBus           = "USB";
            else if(dev.IsFireWire) deviceBus = "FireWire";
            else deviceBus                    = dev.Type.ToString();

            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);
            ctx.SeenDevices.Add(new DeviceStat
            {
                Bus          = deviceBus,
                Manufacturer = dev.Manufacturer,
                Model        = dev.Model,
                Revision     = dev.Revision,
                Synchronized = false
            });
            ctx.SaveChanges();
        }

        /// <summary>
        ///     Adds a new media type to statistics
        /// </summary>
        /// <param name="type">Media type</param>
        /// <param name="real">Set if media was found on a real device, otherwise found on a media image</param>
        public static void AddMedia(MediaType type, bool real)
        {
            if(Settings.Settings.Current.Stats == null || !Settings.Settings.Current.Stats.MediaStats) return;

            DicContext ctx = DicContext.Create(Settings.Settings.LocalDbPath);
            ctx.Medias.Add(new Database.Models.Media
            {
                Real = real, Synchronized = false, Type = type.ToString(), Count = 1
            });
            ctx.SaveChanges();
        }
    }
}