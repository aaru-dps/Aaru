// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : StatisticsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Renders statistics and links to reports.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Server.Models;
using OperatingSystem = DiscImageChef.Server.Models.OperatingSystem;
using PlatformID = DiscImageChef.CommonTypes.Interop.PlatformID;
using Version = DiscImageChef.Server.Models.Version;

namespace DiscImageChef.Server.Controllers
{
    /// <summary>
    ///     Renders a page with statistics, list of media type, devices, etc
    /// </summary>
    public class StatsController : Controller
    {
        DicServerContext     ctx = new DicServerContext();
        List<DeviceItem>     devices;
        List<NameValueStats> operatingSystems;
        List<MediaItem>      realMedia;
        List<NameValueStats> versions;
        List<MediaItem>      virtualMedia;

        public ActionResult Index()
        {
            ViewBag.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            try
            {
                if(
                    System.IO.File
                          .Exists(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                               "Statistics", "Statistics.xml")))
                    try
                    {
                        Stats statistics = new Stats();

                        XmlSerializer xs = new XmlSerializer(statistics.GetType());
                        FileStream fs =
                            WaitForFile(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "Statistics", "Statistics.xml"),
                                        FileMode.Open, FileAccess.Read, FileShare.Read);
                        statistics = (Stats)xs.Deserialize(fs);
                        fs.Close();

                        StatsConverter.Convert(statistics);

                        System.IO.File
                              .Delete(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                                   "Statistics", "Statistics.xml"));
                    }
                    catch(XmlException)
                    {
                        // Do nothing
                    }

                if(ctx.OperatingSystems.Any())
                {
                    operatingSystems = new List<NameValueStats>();
                    foreach(OperatingSystem nvs in ctx.OperatingSystems)
                        operatingSystems.Add(new NameValueStats
                        {
                            name =
                                $"{DetectOS.GetPlatformName((PlatformID)Enum.Parse(typeof(PlatformID), nvs.Name), nvs.Version)}{(string.IsNullOrEmpty(nvs.Version) ? "" : " ")}{nvs.Version}",
                            Value = nvs.Count
                        });

                    ViewBag.repOperatingSystems = operatingSystems.OrderBy(os => os.name).ToList();
                }

                if(ctx.Versions.Any())
                {
                    versions = new List<NameValueStats>();
                    foreach(Version nvs in ctx.Versions)
                        versions.Add(new NameValueStats
                        {
                            name  = nvs.Value == "previous" ? "Previous than 3.4.99.0" : nvs.Value,
                            Value = nvs.Count
                        });

                    ViewBag.repVersions = versions.OrderBy(ver => ver.name).ToList();
                }

                if(ctx.Commands.Any())
                {
                    ViewBag.lblAnalyze = ctx.Commands.FirstOrDefault(c => c.Name == "analyze")?.Count.ToString() ?? "0";
                    ViewBag.lblCompare = ctx.Commands.FirstOrDefault(c => c.Name == "compare")?.Count.ToString() ?? "0";
                    ViewBag.lblChecksum =
                        ctx.Commands.FirstOrDefault(c => c.Name == "checksum")?.Count.ToString() ?? "0";
                    ViewBag.lblEntropy = ctx.Commands.FirstOrDefault(c => c.Name == "entropy")?.Count.ToString() ?? "0";
                    ViewBag.lblVerify  = ctx.Commands.FirstOrDefault(c => c.Name == "verify")?.Count.ToString()  ?? "0";
                    ViewBag.lblPrintHex =
                        ctx.Commands.FirstOrDefault(c => c.Name == "printhex")?.Count.ToString() ?? "0";
                    ViewBag.lblDecode = ctx.Commands.FirstOrDefault(c => c.Name == "decode")?.Count.ToString() ?? "0";
                    ViewBag.lblDeviceInfo =
                        ctx.Commands.FirstOrDefault(c => c.Name == "device-info")?.Count.ToString() ?? "0";
                    ViewBag.lblMediaInfo = ctx.Commands.FirstOrDefault(c => c.Name == "media-info")?.Count.ToString() ??
                                           "0";
                    ViewBag.lblMediaScan = ctx.Commands.FirstOrDefault(c => c.Name == "media-scan")?.Count.ToString() ??
                                           "0";
                    ViewBag.lblFormats = ctx.Commands.FirstOrDefault(c => c.Name == "formats")?.Count.ToString() ?? "0";
                    ViewBag.lblBenchmark =
                        ctx.Commands.FirstOrDefault(c => c.Name == "benchmark")?.Count.ToString() ?? "0";
                    ViewBag.lblCreateSidecar =
                        ctx.Commands.FirstOrDefault(c => c.Name == "create-sidecar")?.Count.ToString() ?? "0";
                    ViewBag.lblDumpMedia = ctx.Commands.FirstOrDefault(c => c.Name == "dump-media")?.Count.ToString() ??
                                           "0";
                    ViewBag.lblDeviceReport =
                        ctx.Commands.FirstOrDefault(c => c.Name == "device-report")?.Count.ToString() ?? "0";
                    ViewBag.lblLs = ctx.Commands.FirstOrDefault(c => c.Name == "ls")?.Count.ToString() ?? "0";
                    ViewBag.lblExtractFiles =
                        ctx.Commands.FirstOrDefault(c => c.Name == "extract-files")?.Count.ToString() ?? "0";
                    ViewBag.lblListDevices =
                        ctx.Commands.FirstOrDefault(c => c.Name == "list-devices")?.Count.ToString() ?? "0";
                    ViewBag.lblListEncodings =
                        ctx.Commands.FirstOrDefault(c => c.Name == "list-encodings")?.Count.ToString() ?? "0";
                    ViewBag.lblConvertImage =
                        ctx.Commands.FirstOrDefault(c => c.Name == "convert-image")?.Count.ToString() ?? "0";
                    ViewBag.lblImageInfo = ctx.Commands.FirstOrDefault(c => c.Name == "image-info")?.Count.ToString() ??
                                           "0";
                }

                if(ctx.Filters.Any()) ViewBag.repFilters = ctx.Filters.OrderBy(filter => filter.Name).ToList();

                if(ctx.MediaFormats.Any())
                    ViewBag.repMediaImages = ctx.MediaFormats.OrderBy(filter => filter.Name).ToList();

                if(ctx.Partitions.Any()) ViewBag.repPartitions = ctx.Partitions.OrderBy(filter => filter.Name).ToList();

                if(ctx.Filesystems.Any())
                    ViewBag.repFilesystems = ctx.Filesystems.OrderBy(filter => filter.Name).ToList();

                if(ctx.Medias.Any())
                {
                    realMedia    = new List<MediaItem>();
                    virtualMedia = new List<MediaItem>();
                    foreach(Media nvs in ctx.Medias)
                        try
                        {
                            MediaType
                               .MediaTypeToString((CommonTypes.MediaType)Enum.Parse(typeof(CommonTypes.MediaType), nvs.Type),
                                                  out string type, out string subtype);

                            if(nvs.Real)
                                realMedia.Add(new MediaItem {Type     = type, SubType = subtype, Count = nvs.Count});
                            else virtualMedia.Add(new MediaItem {Type = type, SubType = subtype, Count = nvs.Count});
                        }
                        catch
                        {
                            if(nvs.Real)
                                realMedia.Add(new MediaItem {Type     = nvs.Type, SubType = null, Count = nvs.Count});
                            else virtualMedia.Add(new MediaItem {Type = nvs.Type, SubType = null, Count = nvs.Count});
                        }

                    if(realMedia.Count > 0)
                        ViewBag.repRealMedia =
                            realMedia.OrderBy(media => media.Type).ThenBy(media => media.SubType).ToList();

                    if(virtualMedia.Count > 0)
                        ViewBag.repVirtualMedia =
                            virtualMedia.OrderBy(media => media.Type).ThenBy(media => media.SubType).ToList();
                }

                if(ctx.DeviceStats.Any())
                {
                    devices = new List<DeviceItem>();
                    foreach(DeviceStat device in ctx.DeviceStats.ToList())
                    {
                        string xmlFile;
                        if(!string.IsNullOrWhiteSpace(device.Manufacturer) &&
                           !string.IsNullOrWhiteSpace(device.Model)        &&
                           !string.IsNullOrWhiteSpace(device.Revision))
                            xmlFile = device.Manufacturer + "_" + device.Model + "_" + device.Revision + ".xml";
                        else if(!string.IsNullOrWhiteSpace(device.Manufacturer) &&
                                !string.IsNullOrWhiteSpace(device.Model))
                            xmlFile = device.Manufacturer + "_" + device.Model + ".xml";
                        else if(!string.IsNullOrWhiteSpace(device.Model) && !string.IsNullOrWhiteSpace(device.Revision))
                            xmlFile  = device.Model + "_" + device.Revision + ".xml";
                        else xmlFile = device.Model                         + ".xml";

                        xmlFile = xmlFile.Replace('/', '_').Replace('\\', '_').Replace('?', '_');

                        if(System.IO.File.Exists(Path.Combine(HostingEnvironment.MapPath("~"), "Reports", xmlFile)))
                        {
                            DeviceReport deviceReport = new DeviceReport();

                            XmlSerializer xs = new XmlSerializer(deviceReport.GetType());
                            FileStream fs =
                                WaitForFile(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "Reports", xmlFile),
                                            FileMode.Open, FileAccess.Read, FileShare.Read);
                            deviceReport = (DeviceReport)xs.Deserialize(fs);
                            fs.Close();

                            DeviceReportV2 deviceReportV2 = new DeviceReportV2(deviceReport);

                            device.Report = ctx.Devices.Add(new Device(deviceReportV2));
                            ctx.SaveChanges();

                            System.IO.File
                                  .Delete(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                                       "Reports", xmlFile));
                        }

                        devices.Add(new DeviceItem
                        {
                            Manufacturer = device.Manufacturer,
                            Model        = device.Model,
                            Revision     = device.Revision,
                            Bus          = device.Bus,
                            ReportId = device.Report != null && device.Report.Id != 0
                                           ? device.Report.Id
                                           : 0
                        });
                    }

                    ViewBag.repDevices = devices.OrderBy(device => device.Manufacturer).ThenBy(device => device.Model)
                                                .ThenBy(device => device.Revision).ThenBy(device => device.Bus)
                                                .ToList();
                }
            }
            catch(Exception)
            {
                #if DEBUG
                throw;
                #endif
                return Content("Could not read statistics");
            }

            return View();
        }

        static FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for(int numTries = 0; numTries < 100; numTries++)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(fullPath, mode, access, share);
                    return fs;
                }
                catch(IOException)
                {
                    fs?.Dispose();
                    Thread.Sleep(50);
                }
            }

            return null;
        }
    }
}