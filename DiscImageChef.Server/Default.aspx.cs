using System;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.IO;
using DiscImageChef.Metadata;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.CodeDom;
using System.Linq;

namespace DiscImageChef.Server
{
    public partial class Default : System.Web.UI.Page
    {
        class MediaItem
        {
            public string Type { get; set; }
            public string SubType { get; set; }
            public long Count { get; set; }
        }

        class DeviceItem
        {
            public string Manufacturer { get; set; }
            public string Model { get; set; }
            public string Revision { get; set; }
            public string Bus { get; set; }
            public string ReportLink { get; set; }
        }

        Stats statistics;
        List<MediaItem> realMedia;
        List<MediaItem> virtualMedia;
        List<NameValueStats> operatingSystems;
        List<DeviceItem> devices;
        List<NameValueStats> versions;

        protected void Page_Load(object sender, EventArgs e)
        {
            lblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            try
            {
                if(!File.Exists(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "Statistics", "Statistics.xml")))
                {
#if DEBUG
                    content.InnerHtml = string.Format("<b>Sorry, cannot load data file \"{0}\"</b>", Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "Statistics", "Statistics.xml"));
#else
                    content.InnerHtml = "<b>Sorry, cannot load data file</b>";
#endif
                    return;
                }

                statistics = new Stats();

                XmlSerializer xs = new XmlSerializer(statistics.GetType());
                FileStream fs = WaitForFile(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "Statistics", "Statistics.xml"), FileMode.Open, FileAccess.Read, FileShare.Read);
                statistics = (Stats)xs.Deserialize(fs);
                fs.Close();

                if(statistics.OperatingSystems != null)
                {
                    operatingSystems = new List<NameValueStats>();
                    foreach(OsStats nvs in statistics.OperatingSystems)
                        operatingSystems.Add(new NameValueStats { name = string.Format("{0}{1}{2}", Interop.DetectOS.GetPlatformName((Interop.PlatformID)Enum.Parse(typeof(Interop.PlatformID), nvs.name), nvs.version),
                                                                                string.IsNullOrEmpty(nvs.version) ? "" : " ", nvs.version), Value = nvs.Value });
                        //operatingSystems.Add(new OsStats { name = Interop.DetectOS.GetPlatformName((Interop.PlatformID)Enum.Parse(typeof(Interop.PlatformID), nvs.name), nvs.version), Value = nvs.Value, version = nvs.version });
                    repOperatingSystems.DataSource = operatingSystems.OrderBy(os => os.name).ToList();
                    repOperatingSystems.DataBind();
                }
                else
                    divOperatingSystems.Visible = false;

                if(statistics.Versions != null)
                {
                    versions = new List<NameValueStats>();
                    foreach(NameValueStats nvs in statistics.Versions)
                    {
                        if(nvs.name == "previous")
                            versions.Add(new NameValueStats { name = "Previous than 3.4.99.0", Value = nvs.Value });
                        else
                            versions.Add(nvs);
                    }
                    repVersions.DataSource = versions.OrderBy(ver => ver.name).ToList();
                    repVersions.DataBind();
                }
                else
                    divVersions.Visible = false;

                if(statistics.Commands != null)
                {
                    lblAnalyze.Text = statistics.Commands.Analyze.ToString();
                    lblCompare.Text = statistics.Commands.Compare.ToString();
                    lblChecksum.Text = statistics.Commands.Checksum.ToString();
                    lblEntropy.Text = statistics.Commands.Entropy.ToString();
                    lblVerify.Text = statistics.Commands.Verify.ToString();
                    lblPrintHex.Text = statistics.Commands.PrintHex.ToString();
                    lblDecode.Text = statistics.Commands.Decode.ToString();
                    lblDeviceInfo.Text = statistics.Commands.DeviceInfo.ToString();
                    lblMediaInfo.Text = statistics.Commands.MediaInfo.ToString();
                    lblMediaScan.Text = statistics.Commands.MediaScan.ToString();
                    lblFormats.Text = statistics.Commands.Formats.ToString();
                    lblBenchmark.Text = statistics.Commands.Benchmark.ToString();
                    lblCreateSidecar.Text = statistics.Commands.CreateSidecar.ToString();
                    lblDumpMedia.Text = statistics.Commands.DumpMedia.ToString();
                    lblDeviceReport.Text = statistics.Commands.DeviceReport.ToString();
                    lblLs.Text = statistics.Commands.Ls.ToString();
                    lblExtractFiles.Text = statistics.Commands.ExtractFiles.ToString();
                    lblListDevices.Text = statistics.Commands.ListDevices.ToString();
                }
                else
                    divCommands.Visible = false;

                if(statistics.Filters != null)
                {
                    repFilters.DataSource = statistics.Filters.OrderBy(filter => filter.name).ToList();
                    repFilters.DataBind();
                }
                else
                    divFilters.Visible = false;

                if(statistics.MediaImages != null)
                {
                    repMediaImages.DataSource = statistics.MediaImages.OrderBy(filter => filter.name).ToList();
                    repMediaImages.DataBind();
                }
                else
                    divMediaImages.Visible = false;

                if(statistics.Partitions != null)
                {
                    repPartitions.DataSource = statistics.Partitions.OrderBy(filter => filter.name).ToList();
                    repPartitions.DataBind();
                }
                else
                    divPartitions.Visible = false;

                if(statistics.Filesystems != null)
                {
                    repFilesystems.DataSource = statistics.Filesystems.OrderBy(filter => filter.name).ToList();
                    repFilesystems.DataBind();
                }
                else
                    divFilesystems.Visible = false;

                if(statistics.Medias != null)
                {
                    realMedia = new List<MediaItem>();
                    virtualMedia = new List<MediaItem>();
                    foreach(MediaStats nvs in statistics.Medias)
                    {
                        string type;
                        string subtype;

                        MediaType.MediaTypeToString((CommonTypes.MediaType)Enum.Parse(typeof(CommonTypes.MediaType), nvs.type), out type, out subtype);

                        if(nvs.real)
                            realMedia.Add(new MediaItem { Type = type, SubType = subtype, Count = nvs.Value });
                        else
                            virtualMedia.Add(new MediaItem { Type = type, SubType = subtype, Count = nvs.Value });
                    }

                    if(realMedia.Count > 0)
                    {
                        repRealMedia.DataSource = realMedia.OrderBy(media => media.Type).ThenBy(media => media.SubType).ToList();
                        repRealMedia.DataBind();
                    }
                    else
                        divRealMedia.Visible = false;

                    if(virtualMedia.Count > 0)
                    {
                        repVirtualMedia.DataSource = virtualMedia.OrderBy(media => media.Type).ThenBy(media => media.SubType).ToList();
                        repVirtualMedia.DataBind();
                    }
                    else
                        divVirtualMedia.Visible = false;
                }
                else
                {
                    divRealMedia.Visible = false;
                    divVirtualMedia.Visible = false;
                }

                if(statistics.Devices != null)
                {
                    devices = new List<DeviceItem>();
                    foreach(DeviceStats device in statistics.Devices)
                    {
                        string url = null;
                        string xmlFile;
                        if(!string.IsNullOrWhiteSpace(device.Manufacturer) && !string.IsNullOrWhiteSpace(device.Model) && !string.IsNullOrWhiteSpace(device.Revision))
                        {
                            xmlFile = device.Manufacturer + "_" + device.Model + "_" + device.Revision + ".xml";
                            url = string.Format("ViewReport.aspx?manufacturer={0}&model={1}&revision={2}",
                                                HttpUtility.UrlPathEncode(device.Manufacturer), HttpUtility.UrlPathEncode(device.Model), HttpUtility.UrlPathEncode(device.Revision));
                        }
                        else if(!string.IsNullOrWhiteSpace(device.Manufacturer) && !string.IsNullOrWhiteSpace(device.Model))
                        {
                            xmlFile = device.Manufacturer + "_" + device.Model + ".xml";
                            url = string.Format("ViewReport.aspx?manufacturer={0}&model={1}",
                                                HttpUtility.UrlPathEncode(device.Manufacturer), HttpUtility.UrlPathEncode(device.Model));
                        }
                        else if(!string.IsNullOrWhiteSpace(device.Model) && !string.IsNullOrWhiteSpace(device.Revision))
                        {
                            xmlFile = device.Model + "_" + device.Revision + ".xml";
                            url = string.Format("ViewReport.aspx?model={0}&revision={1}",
                                                HttpUtility.UrlPathEncode(device.Model), HttpUtility.UrlPathEncode(device.Revision));
                        }
                        else
                        {
                            xmlFile = device.Model + ".xml";
                            url = string.Format("ViewReport.aspx?model={0}",
                                                HttpUtility.UrlPathEncode(device.Model));
                        }

                        xmlFile = xmlFile.Replace('/', '_').Replace('\\', '_').Replace('?', '_');

                        if(!File.Exists(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "Reports", xmlFile)))
                        {
                            url = null;
                        }

                        devices.Add(new DeviceItem()
                        {
                            Manufacturer = device.Manufacturer,
                            Model = device.Model,
                            Revision = device.Revision,
                            Bus = device.Bus,
                            ReportLink = url == null ? "No" : string.Format("<a href=\"{0}\" target=\"_blank\">Yes</a>", url)
                        });
                    }
                    repDevices.DataSource = devices.OrderBy(device => device.Manufacturer).ThenBy(device => device.Model).ThenBy(device => device.Revision).ThenBy(device => device.Bus).ToList();
                    repDevices.DataBind();
                }
                else
                    divDevices.Visible = false;

            }
            catch(Exception)
            {
                content.InnerHtml = "<b>Could not load statistics</b>";
#if DEBUG
                throw;
#endif
            }
        }

        FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
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
                    if(fs != null)
                    {
                        fs.Dispose();
                    }
                    System.Threading.Thread.Sleep(50);
                }
            }

            return null;
        }
    }
}
