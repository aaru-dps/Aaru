// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : StatsController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Fetches statistics for Razor views.
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
using Highsoft.Web.Mvc.Charts;
using Filter = DiscImageChef.Server.Models.Filter;
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

                    List<PieSeriesData> osPieData = new List<PieSeriesData>();

                    decimal totalOsCount = ctx.OperatingSystems.Sum(o => o.Count);
                    foreach(string os in ctx.OperatingSystems.Select(o => o.Name).Distinct().ToList())
                    {
                        decimal osCount = ctx.OperatingSystems.Where(o => o.Name == os).Sum(o => o.Count);

                        osPieData.Add(new PieSeriesData
                        {
                            Name =
                                DetectOS.GetPlatformName((PlatformID)Enum.Parse(typeof(PlatformID),
                                                                                os)),
                            Y        = (double?)(osCount / totalOsCount),
                            Sliced   = os == "Linux",
                            Selected = os == "Linux"
                        });
                    }

                    ViewData["osPieData"] = osPieData;

                    List<PieSeriesData> linuxPieData = new List<PieSeriesData>();

                    decimal linuxCount = ctx.OperatingSystems.Where(o => o.Name == PlatformID.Linux.ToString())
                                            .Sum(o => o.Count);
                    foreach(OperatingSystem version in
                        ctx.OperatingSystems.Where(o => o.Name == PlatformID.Linux.ToString()))
                        linuxPieData.Add(new PieSeriesData
                        {
                            Name =
                                $"{DetectOS.GetPlatformName(PlatformID.Linux, version.Version)}{(string.IsNullOrEmpty(version.Version) ? "" : " ")}{version.Version}",
                            Y = (double?)(version.Count / linuxCount)
                        });

                    ViewData["linuxPieData"] = linuxPieData;

                    List<PieSeriesData> macosPieData = new List<PieSeriesData>();

                    decimal macosCount = ctx.OperatingSystems.Where(o => o.Name == PlatformID.MacOSX.ToString())
                                            .Sum(o => o.Count);
                    foreach(OperatingSystem version in
                        ctx.OperatingSystems.Where(o => o.Name == PlatformID.MacOSX.ToString()))
                        macosPieData.Add(new PieSeriesData
                        {
                            Name =
                                $"{DetectOS.GetPlatformName(PlatformID.MacOSX, version.Version)}{(string.IsNullOrEmpty(version.Version) ? "" : " ")}{version.Version}",
                            Y = (double?)(version.Count / macosCount)
                        });

                    ViewData["macosPieData"] = macosPieData;

                    List<PieSeriesData> windowsPieData = new List<PieSeriesData>();

                    decimal windowsCount = ctx.OperatingSystems.Where(o => o.Name == PlatformID.Win32NT.ToString())
                                              .Sum(o => o.Count);
                    foreach(OperatingSystem version in
                        ctx.OperatingSystems.Where(o => o.Name == PlatformID.Win32NT.ToString()))
                        windowsPieData.Add(new PieSeriesData
                        {
                            Name =
                                $"{DetectOS.GetPlatformName(PlatformID.Win32NT, version.Version)}{(string.IsNullOrEmpty(version.Version) ? "" : " ")}{version.Version}",
                            Y = (double?)(version.Count / windowsCount)
                        });

                    ViewData["windowsPieData"] = windowsPieData;
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

                    decimal totalVersionCount = ctx.Versions.Sum(o => o.Count);

                    ViewData["versionsPieData"] = ctx.Versions.Select(version => new PieSeriesData
                    {
                        Name =
                            version.Value == "previous"
                                ? "Previous than 3.4.99.0"
                                : version.Value,
                        Y = (double?)(version.Count /
                                      totalVersionCount),
                        Sliced   = version.Value == "previous",
                        Selected = version.Value == "previous"
                    }).ToList();
                }

                if(ctx.Commands.Any())
                {
                    ViewBag.repCommands = ctx.Commands.OrderBy(c => c.Name).ToList();

                    decimal totalCommandCount = ctx.Commands.Sum(o => o.Count);

                    ViewData["commandsPieData"] = ctx
                                                 .Commands.Select(command => new PieSeriesData
                                                  {
                                                      Name = command.Name,
                                                      Y = (double?)(command.Count /
                                                                    totalCommandCount),
                                                      Sliced   = command.Name == "analyze",
                                                      Selected = command.Name == "analyze"
                                                  }).ToList();
                }

                if(ctx.Filters.Any())
                {
                    ViewBag.repFilters = ctx.Filters.OrderBy(filter => filter.Name).ToList();

                    List<PieSeriesData> filtersPieData = new List<PieSeriesData>();

                    decimal totalFiltersCount = ctx.Filters.Sum(o => o.Count);
                    foreach(Filter filter in ctx.Filters.ToList())
                        filtersPieData.Add(new PieSeriesData
                        {
                            Name     = filter.Name,
                            Y        = (double?)(filter.Count / totalFiltersCount),
                            Sliced   = filter.Name == "No filter",
                            Selected = filter.Name == "No filter"
                        });

                    ViewData["filtersPieData"] = filtersPieData;
                }

                if(ctx.MediaFormats.Any())
                {
                    ViewBag.repMediaImages = ctx.MediaFormats.OrderBy(filter => filter.Name).ToList();

                    List<PieSeriesData> formatsPieData = new List<PieSeriesData>();

                    decimal totalFormatsCount = ctx.MediaFormats.Sum(o => o.Count);
                    decimal top10FormatCount  = 0;

                    foreach(MediaFormat format in ctx.MediaFormats.OrderByDescending(o => o.Count).Take(10))
                    {
                        top10FormatCount += format.Count;

                        formatsPieData.Add(new PieSeriesData
                        {
                            Name = format.Name, Y = (double?)(format.Count / totalFormatsCount)
                        });
                    }

                    formatsPieData.Add(new PieSeriesData
                    {
                        Name = "Other",
                        Y = (double?)((totalFormatsCount - top10FormatCount) /
                                      totalFormatsCount),
                        Sliced   = true,
                        Selected = true
                    });

                    ViewData["formatsPieData"] = formatsPieData;
                }

                if(ctx.Partitions.Any())
                {
                    ViewBag.repPartitions = ctx.Partitions.OrderBy(filter => filter.Name).ToList();

                    List<PieSeriesData> partitionsPieData = new List<PieSeriesData>();

                    decimal totalPartitionsCount = ctx.Partitions.Sum(o => o.Count);
                    decimal top10PartitionCount  = 0;

                    foreach(Partition partition in ctx.Partitions.OrderByDescending(o => o.Count).Take(10))
                    {
                        top10PartitionCount += partition.Count;

                        partitionsPieData.Add(new PieSeriesData
                        {
                            Name = partition.Name,
                            Y    = (double?)(partition.Count / totalPartitionsCount)
                        });
                    }

                    partitionsPieData.Add(new PieSeriesData
                    {
                        Name = "Other",
                        Y = (double?)((totalPartitionsCount - top10PartitionCount) /
                                      totalPartitionsCount),
                        Sliced   = true,
                        Selected = true
                    });

                    ViewData["partitionsPieData"] = partitionsPieData;
                }

                if(ctx.Filesystems.Any())
                {
                    ViewBag.repFilesystems = ctx.Filesystems.OrderBy(filter => filter.Name).ToList();

                    List<PieSeriesData> filesystemsPieData = new List<PieSeriesData>();

                    decimal totalFilesystemsCount = ctx.Filesystems.Sum(o => o.Count);
                    decimal top10FilesystemCount  = 0;

                    foreach(Filesystem filesystem in ctx.Filesystems.OrderByDescending(o => o.Count).Take(10))
                    {
                        top10FilesystemCount += filesystem.Count;

                        filesystemsPieData.Add(new PieSeriesData
                        {
                            Name = filesystem.Name,
                            Y    = (double?)(filesystem.Count / totalFilesystemsCount)
                        });
                    }

                    filesystemsPieData.Add(new PieSeriesData
                    {
                        Name = "Other",
                        Y = (double?)((totalFilesystemsCount - top10FilesystemCount) /
                                      totalFilesystemsCount),
                        Sliced   = true,
                        Selected = true
                    });

                    ViewData["filesystemsPieData"] = filesystemsPieData;
                }

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
                    {
                        ViewBag.repRealMedia =
                            realMedia.OrderBy(media => media.Type).ThenBy(media => media.SubType).ToList();

                        List<PieSeriesData> realMediaPieData = new List<PieSeriesData>();

                        decimal totalRealMediaCount = realMedia.Sum(o => o.Count);
                        decimal top10RealMediaCount = 0;

                        foreach(MediaItem realMediaItem in realMedia.OrderByDescending(o => o.Count).Take(10))
                        {
                            top10RealMediaCount += realMediaItem.Count;

                            realMediaPieData.Add(new PieSeriesData
                            {
                                Name = $"{realMediaItem.Type} ({realMediaItem.SubType})",
                                Y    = (double?)(realMediaItem.Count / totalRealMediaCount)
                            });
                        }

                        realMediaPieData.Add(new PieSeriesData
                        {
                            Name = "Other",
                            Y = (double?)((totalRealMediaCount - top10RealMediaCount) /
                                          totalRealMediaCount),
                            Sliced   = true,
                            Selected = true
                        });

                        ViewData["realMediaPieData"] = realMediaPieData;
                    }

                    if(virtualMedia.Count > 0)
                    {
                        ViewBag.repVirtualMedia =
                            virtualMedia.OrderBy(media => media.Type).ThenBy(media => media.SubType).ToList();

                        List<PieSeriesData> virtualMediaPieData = new List<PieSeriesData>();

                        decimal totalVirtualMediaCount = virtualMedia.Sum(o => o.Count);
                        decimal top10VirtualMediaCount = 0;

                        foreach(MediaItem virtualMediaItem in virtualMedia.OrderByDescending(o => o.Count).Take(10))
                        {
                            top10VirtualMediaCount += virtualMediaItem.Count;

                            virtualMediaPieData.Add(new PieSeriesData
                            {
                                Name =
                                    $"{virtualMediaItem.Type} ({virtualMediaItem.SubType})",
                                Y = (double?)(virtualMediaItem.Count /
                                              totalVirtualMediaCount)
                            });
                        }

                        virtualMediaPieData.Add(new PieSeriesData
                        {
                            Name = "Other",
                            Y = (double?)
                                ((totalVirtualMediaCount - top10VirtualMediaCount) /
                                 totalVirtualMediaCount),
                            Sliced   = true,
                            Selected = true
                        });

                        ViewData["virtualMediaPieData"] = virtualMediaPieData;
                    }
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

                    ViewData["devicesBusPieData"] = (from deviceBus in devices.Select(d => d.Bus).Distinct()
                                                     let deviceBusCount = devices.Count(d => d.Bus == deviceBus)
                                                     select new PieSeriesData
                                                     {
                                                         Name = deviceBus,
                                                         Y    = deviceBusCount / (double)devices.Count
                                                     }).ToList();

                    ViewData["devicesManufacturerPieData"] =
                        (from manufacturer in
                             devices.Where(d => d.Manufacturer != null).Select(d => d.Manufacturer.ToLowerInvariant())
                                    .Distinct()
                         let manufacturerCount = devices.Count(d => d.Manufacturer?.ToLowerInvariant() == manufacturer)
                         select new PieSeriesData {Name = manufacturer, Y = manufacturerCount / (double)devices.Count})
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