// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : StatsConverter.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads a statistics XML and stores it in the database context.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/
using System.Linq;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Server.Models;
using Version = DiscImageChef.Server.Models.Version;

namespace DiscImageChef.Server
{
    public static class StatsConverter
    {
        public static void Convert(Stats newStats)
        {
            DicServerContext ctx = new DicServerContext();

            if(newStats.Commands != null)
            {
                if(newStats.Commands.Analyze > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "analyze");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.Analyze, Name = "analyze"});
                    else existing.Count += newStats.Commands.Analyze;
                }

                if(newStats.Commands.Benchmark > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "benchmark");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.Benchmark, Name = "benchmark"});
                    else existing.Count += newStats.Commands.Benchmark;
                }

                if(newStats.Commands.Checksum > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "checksum");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.Checksum, Name = "checksum"});
                    else existing.Count += newStats.Commands.Checksum;
                }

                if(newStats.Commands.Compare > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "compare");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.Compare, Name = "compare"});
                    else existing.Count += newStats.Commands.Compare;
                }

                if(newStats.Commands.CreateSidecar > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "create-sidecar");

                    if(existing == null)
                        ctx.Commands.Add(new Command
                        {
                            Count = newStats.Commands.CreateSidecar, Name = "create-sidecar"
                        });
                    else existing.Count += newStats.Commands.CreateSidecar;
                }

                if(newStats.Commands.Decode > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "decode");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.Decode, Name = "decode"});
                    else existing.Count += newStats.Commands.Decode;
                }

                if(newStats.Commands.DeviceInfo > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "device-info");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.DeviceInfo, Name = "device-info"});
                    else existing.Count += newStats.Commands.DeviceInfo;
                }

                if(newStats.Commands.DeviceReport > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "device-report");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.DeviceReport, Name = "device-report"});
                    else existing.Count += newStats.Commands.DeviceReport;
                }

                if(newStats.Commands.DumpMedia > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "dump-media");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.DumpMedia, Name = "dump-media"});
                    else existing.Count += newStats.Commands.DumpMedia;
                }

                if(newStats.Commands.Entropy > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "entropy");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.Entropy, Name = "entropy"});
                    else existing.Count += newStats.Commands.Entropy;
                }

                if(newStats.Commands.Formats > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "formats");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.Formats, Name = "formats"});
                    else existing.Count += newStats.Commands.Formats;
                }

                if(newStats.Commands.MediaInfo > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "media-info");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.MediaInfo, Name = "media-info"});
                    else existing.Count += newStats.Commands.MediaInfo;
                }

                if(newStats.Commands.MediaScan > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "media-scan");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.MediaScan, Name = "media-scan"});
                    else existing.Count += newStats.Commands.MediaScan;
                }

                if(newStats.Commands.PrintHex > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "printhex");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.PrintHex, Name = "printhex"});
                    else existing.Count += newStats.Commands.PrintHex;
                }

                if(newStats.Commands.Verify > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "verify");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.Verify, Name = "verify"});
                    else existing.Count += newStats.Commands.Verify;
                }

                if(newStats.Commands.Ls > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "ls");

                    if(existing == null) ctx.Commands.Add(new Command {Count = newStats.Commands.Ls, Name = "ls"});
                    else existing.Count += newStats.Commands.Ls;
                }

                if(newStats.Commands.ExtractFiles > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "extract-files");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.ExtractFiles, Name = "extract-files"});
                    else existing.Count += newStats.Commands.ExtractFiles;
                }

                if(newStats.Commands.ListDevices > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "list-devices");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.ListDevices, Name = "list-devices"});
                    else existing.Count += newStats.Commands.ListDevices;
                }

                if(newStats.Commands.ListEncodings > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "list-encodings");

                    if(existing == null)
                        ctx.Commands.Add(new Command
                        {
                            Count = newStats.Commands.ListEncodings, Name = "list-encodings"
                        });
                    else existing.Count += newStats.Commands.ListEncodings;
                }

                if(newStats.Commands.ConvertImage > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "convert-image");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.ConvertImage, Name = "convert-image"});
                    else existing.Count += newStats.Commands.ConvertImage;
                }

                if(newStats.Commands.ImageInfo > 0)
                {
                    Command existing = ctx.Commands.FirstOrDefault(c => c.Name == "image-info");

                    if(existing == null)
                        ctx.Commands.Add(new Command {Count = newStats.Commands.ImageInfo, Name = "image-info"});
                    else existing.Count += newStats.Commands.ImageInfo;
                }
            }

            if(newStats.OperatingSystems != null)
                foreach(OsStats operatingSystem in newStats.OperatingSystems)
                {
                    if(string.IsNullOrWhiteSpace(operatingSystem.name) ||
                       string.IsNullOrWhiteSpace(operatingSystem.version)) continue;

                    OperatingSystem existing =
                        ctx.OperatingSystems.FirstOrDefault(c => c.Name    == operatingSystem.name &&
                                                                 c.Version == operatingSystem.version);

                    if(existing == null)
                        ctx.OperatingSystems.Add(new OperatingSystem
                        {
                            Count   = operatingSystem.Value,
                            Name    = operatingSystem.name,
                            Version = operatingSystem.version
                        });
                    else existing.Count += operatingSystem.Value;
                }
            else
            {
                OperatingSystem existing =
                    ctx.OperatingSystems.FirstOrDefault(c => c.Name == "Linux" && c.Version == null);

                if(existing == null) ctx.OperatingSystems.Add(new OperatingSystem {Count = 1, Name = "Linux"});
                else existing.Count++;
            }

            if(newStats.Versions != null)
                foreach(NameValueStats nvs in newStats.Versions)
                {
                    if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                    Version existing = ctx.Versions.FirstOrDefault(c => c.Value == nvs.name);

                    if(existing == null) ctx.Versions.Add(new Version {Count = nvs.Value, Value = nvs.name});
                    else existing.Count += nvs.Value;
                }
            else
            {
                Version existing = ctx.Versions.FirstOrDefault(c => c.Value == "previous");

                if(existing == null) ctx.Versions.Add(new Version {Count = 1, Value = "previous"});
                else existing.Count++;
            }

            if(newStats.Filesystems != null)
                foreach(NameValueStats nvs in newStats.Filesystems)
                {
                    if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                    Filesystem existing = ctx.Filesystems.FirstOrDefault(c => c.Name == nvs.name);

                    if(existing == null) ctx.Filesystems.Add(new Filesystem {Count = nvs.Value, Name = nvs.name});
                    else existing.Count += nvs.Value;
                }

            if(newStats.Partitions != null)
                foreach(NameValueStats nvs in newStats.Partitions)
                {
                    if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                    Partition existing = ctx.Partitions.FirstOrDefault(c => c.Name == nvs.name);

                    if(existing == null) ctx.Partitions.Add(new Partition {Count = nvs.Value, Name = nvs.name});
                    else existing.Count += nvs.Value;
                }

            if(newStats.MediaImages != null)
                foreach(NameValueStats nvs in newStats.MediaImages)
                {
                    if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                    MediaFormat existing = ctx.MediaFormats.FirstOrDefault(c => c.Name == nvs.name);

                    if(existing == null) ctx.MediaFormats.Add(new MediaFormat {Count = nvs.Value, Name = nvs.name});
                    else existing.Count += nvs.Value;
                }

            if(newStats.Filters != null)
                foreach(NameValueStats nvs in newStats.Filters)
                {
                    if(string.IsNullOrWhiteSpace(nvs.name)) continue;

                    Filter existing = ctx.Filters.FirstOrDefault(c => c.Name == nvs.name);

                    if(existing == null) ctx.Filters.Add(new Filter {Count = nvs.Value, Name = nvs.name});
                    else existing.Count += nvs.Value;
                }

            if(newStats.Devices != null)
                foreach(DeviceStats device in newStats.Devices)
                {
                    if(string.IsNullOrWhiteSpace(device.Model)) continue;

                    if(!ctx.DeviceStats.Any(c => c.Bus   == device.Bus   && c.Manufacturer == device.Manufacturer &&
                                                 c.Model == device.Model && c.Revision     == device.Revision))
                        ctx.DeviceStats.Add(new DeviceStat
                        {
                            Bus          = device.Bus,
                            Manufacturer = device.Manufacturer,
                            Model        = device.Model,
                            Revision     = device.Revision
                        });
                }

            if(newStats.Medias != null)
                foreach(MediaStats media in newStats.Medias)
                {
                    if(string.IsNullOrWhiteSpace(media.type)) continue;

                    Media existing = ctx.Medias.FirstOrDefault(c => c.Type == media.type && c.Real == media.real);

                    if(existing == null)
                        ctx.Medias.Add(new Media {Count = media.Value, Real = media.real, Type = media.type});
                    else existing.Count += media.Value;
                }

            ctx.SaveChanges();
        }
    }
}