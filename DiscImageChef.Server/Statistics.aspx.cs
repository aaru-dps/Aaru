// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Statistics.aspx.cs
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
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using DiscImageChef.CommonTypes.Interop;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Server.Models;
using OperatingSystem = DiscImageChef.Server.Models.OperatingSystem;
using PlatformID = DiscImageChef.CommonTypes.Interop.PlatformID;
using Version = DiscImageChef.Server.Models.Version;

namespace DiscImageChef.Server
{
    /// <summary>
    ///     Renders a page with statistics, list of media type, devices, etc
    /// </summary>
    public partial class Statistics : Page
    {
        DicServerContext     ctx = new DicServerContext();
        List<DeviceItem>     devices;
        List<NameValueStats> operatingSystems;
        List<MediaItem>      realMedia;

        //Stats                statistics;
        List<NameValueStats> versions;
        List<MediaItem>      virtualMedia;

        protected void Page_Load(object sender, EventArgs e)
        {
            lblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            try
            {
                /*
                if(!File.Exists(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                             "Statistics", "Statistics.xml")))
                {
                    #if DEBUG
                    content.InnerHtml =
                        $"<b>Sorry, cannot load data file \"{Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "Statistics", "Statistics.xml")}\"</b>";
                    #else
                    content.InnerHtml = "<b>Sorry, cannot load data file</b>";
                    #endif
                    return;
                }

                statistics = new Stats();

                XmlSerializer xs = new XmlSerializer(statistics.GetType());
                FileStream fs =
                    WaitForFile(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "Statistics", "Statistics.xml"),
                                FileMode.Open, FileAccess.Read, FileShare.Read);
                statistics = (Stats)xs.Deserialize(fs);
                fs.Close();*/

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

                    repOperatingSystems.DataSource = operatingSystems.OrderBy(os => os.name).ToList();
                    repOperatingSystems.DataBind();
                }
                else divOperatingSystems.Visible = false;

                if(ctx.Versions.Any())
                {
                    versions = new List<NameValueStats>();
                    foreach(Version nvs in ctx.Versions)
                        versions.Add(new NameValueStats
                        {
                            name  = nvs.Value == "previous" ? "Previous than 3.4.99.0" : nvs.Value,
                            Value = nvs.Count
                        });

                    repVersions.DataSource = versions.OrderBy(ver => ver.name).ToList();
                    repVersions.DataBind();
                }
                else divVersions.Visible = false;

                if(ctx.Commands.Any())
                {
                    lblAnalyze.Text  = ctx.Commands.FirstOrDefault(c => c.Name == "analyze")?.Count.ToString()  ?? "0";
                    lblCompare.Text  = ctx.Commands.FirstOrDefault(c => c.Name == "compare")?.Count.ToString()  ?? "0";
                    lblChecksum.Text = ctx.Commands.FirstOrDefault(c => c.Name == "checksum")?.Count.ToString() ?? "0";
                    lblEntropy.Text  = ctx.Commands.FirstOrDefault(c => c.Name == "entropy")?.Count.ToString()  ?? "0";
                    lblVerify.Text   = ctx.Commands.FirstOrDefault(c => c.Name == "verify")?.Count.ToString()   ?? "0";
                    lblPrintHex.Text = ctx.Commands.FirstOrDefault(c => c.Name == "printhex")?.Count.ToString() ?? "0";
                    lblDecode.Text   = ctx.Commands.FirstOrDefault(c => c.Name == "decode")?.Count.ToString()   ?? "0";
                    lblDeviceInfo.Text = ctx.Commands.FirstOrDefault(c => c.Name == "device-info")?.Count.ToString() ??
                                         "0";
                    lblMediaInfo.Text = ctx.Commands.FirstOrDefault(c => c.Name == "media-info")?.Count.ToString() ??
                                        "0";
                    lblMediaScan.Text = ctx.Commands.FirstOrDefault(c => c.Name == "media-scan")?.Count.ToString() ??
                                        "0";
                    lblFormats.Text = ctx.Commands.FirstOrDefault(c => c.Name == "formats")?.Count.ToString() ?? "0";
                    lblBenchmark.Text =
                        ctx.Commands.FirstOrDefault(c => c.Name == "benchmark")?.Count.ToString() ?? "0";
                    lblCreateSidecar.Text =
                        ctx.Commands.FirstOrDefault(c => c.Name == "create-sidecar")?.Count.ToString() ?? "0";
                    lblDumpMedia.Text = ctx.Commands.FirstOrDefault(c => c.Name == "dump-media")?.Count.ToString() ??
                                        "0";
                    lblDeviceReport.Text =
                        ctx.Commands.FirstOrDefault(c => c.Name == "device-report")?.Count.ToString() ?? "0";
                    lblLs.Text = ctx.Commands.FirstOrDefault(c => c.Name == "ls")?.Count.ToString() ?? "0";
                    lblExtractFiles.Text =
                        ctx.Commands.FirstOrDefault(c => c.Name == "extract-files")?.Count.ToString() ?? "0";
                    lblListDevices.Text =
                        ctx.Commands.FirstOrDefault(c => c.Name == "list-devices")?.Count.ToString() ?? "0";
                    lblListEncodings.Text =
                        ctx.Commands.FirstOrDefault(c => c.Name == "list-encodings")?.Count.ToString() ?? "0";
                    lblConvertImage.Text =
                        ctx.Commands.FirstOrDefault(c => c.Name == "convert-image")?.Count.ToString() ?? "0";
                    lblImageInfo.Text = ctx.Commands.FirstOrDefault(c => c.Name == "image-info")?.Count.ToString() ??
                                        "0";
                }
                else divCommands.Visible = false;

                if(ctx.Filters.Any())
                {
                    repFilters.DataSource = ctx.Filters.OrderBy(filter => filter.Name).ToList();
                    repFilters.DataBind();
                }
                else divFilters.Visible = false;

                if(ctx.MediaFormats.Any())
                {
                    repMediaImages.DataSource = ctx.MediaFormats.OrderBy(filter => filter.Name).ToList();
                    repMediaImages.DataBind();
                }
                else divMediaImages.Visible = false;

                if(ctx.Partitions.Any())
                {
                    repPartitions.DataSource = ctx.Partitions.OrderBy(filter => filter.Name).ToList();
                    repPartitions.DataBind();
                }
                else divPartitions.Visible = false;

                if(ctx.Filesystems.Any())
                {
                    repFilesystems.DataSource = ctx.Filesystems.OrderBy(filter => filter.Name).ToList();
                    repFilesystems.DataBind();
                }
                else divFilesystems.Visible = false;

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
                        repRealMedia.DataSource =
                            realMedia.OrderBy(media => media.Type).ThenBy(media => media.SubType).ToList();
                        repRealMedia.DataBind();
                    }
                    else divRealMedia.Visible = false;

                    if(virtualMedia.Count > 0)
                    {
                        repVirtualMedia.DataSource =
                            virtualMedia.OrderBy(media => media.Type).ThenBy(media => media.SubType).ToList();
                        repVirtualMedia.DataBind();
                    }
                    else divVirtualMedia.Visible = false;
                }
                else
                {
                    divRealMedia.Visible    = false;
                    divVirtualMedia.Visible = false;
                }

                if(ctx.DeviceStats != null)
                {
                    devices = new List<DeviceItem>();
                    foreach(DeviceStat device in ctx.DeviceStats)
                    {
                        string url;
                        string xmlFile;
                        if(!string.IsNullOrWhiteSpace(device.Manufacturer) &&
                           !string.IsNullOrWhiteSpace(device.Model)        &&
                           !string.IsNullOrWhiteSpace(device.Revision))
                        {
                            xmlFile = device.Manufacturer + "_" + device.Model + "_" + device.Revision + ".xml";
                            url =
                                $"ViewReport.aspx?manufacturer={HttpUtility.UrlPathEncode(device.Manufacturer)}&model={HttpUtility.UrlPathEncode(device.Model)}&revision={HttpUtility.UrlPathEncode(device.Revision)}";
                        }
                        else if(!string.IsNullOrWhiteSpace(device.Manufacturer) &&
                                !string.IsNullOrWhiteSpace(device.Model))
                        {
                            xmlFile = device.Manufacturer + "_" + device.Model + ".xml";
                            url =
                                $"ViewReport.aspx?manufacturer={HttpUtility.UrlPathEncode(device.Manufacturer)}&model={HttpUtility.UrlPathEncode(device.Model)}";
                        }
                        else if(!string.IsNullOrWhiteSpace(device.Model) && !string.IsNullOrWhiteSpace(device.Revision))
                        {
                            xmlFile = device.Model + "_" + device.Revision + ".xml";
                            url =
                                $"ViewReport.aspx?model={HttpUtility.UrlPathEncode(device.Model)}&revision={HttpUtility.UrlPathEncode(device.Revision)}";
                        }
                        else
                        {
                            xmlFile = device.Model + ".xml";
                            url     = $"ViewReport.aspx?model={HttpUtility.UrlPathEncode(device.Model)}";
                        }

                        xmlFile = xmlFile.Replace('/', '_').Replace('\\', '_').Replace('?', '_');

                        if(!File.Exists(Path.Combine(HostingEnvironment.MapPath("~"), "Reports", xmlFile))) url = null;

                        devices.Add(new DeviceItem
                        {
                            Manufacturer = device.Manufacturer,
                            Model        = device.Model,
                            Revision     = device.Revision,
                            Bus          = device.Bus,
                            ReportLink =
                                url == null ? "No" : $"<a href=\"{url}\" target=\"_blank\">Yes</a>"
                        });
                    }

                    repDevices.DataSource = devices.OrderBy(device => device.Manufacturer)
                                                   .ThenBy(device => device.Model).ThenBy(device => device.Revision)
                                                   .ThenBy(device => device.Bus).ToList();
                    repDevices.DataBind();
                }
                else divDevices.Visible = false;
            }
            catch(Exception)
            {
                content.InnerHtml = "<b>Could not load statistics</b>";
                #if DEBUG
                throw;
                #endif
            }
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

        class MediaItem
        {
            public string Type    { get; set; }
            public string SubType { get; set; }
            public long   Count   { get; set; }
        }

        class DeviceItem
        {
            public string Manufacturer { get; set; }
            public string Model        { get; set; }
            public string Revision     { get; set; }
            public string Bus          { get; set; }
            public string ReportLink   { get; set; }
        }
    }
}