using System;
using System.Web;
using System.Web.UI;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using DiscImageChef.Metadata;
using System.Collections.Generic;
using System.Net;
using static DiscImageChef.Decoders.ATA.Identify;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Server.App_Start;
using System.Web.Compilation;
using System.Web.Configuration;

namespace DiscImageChef.Server
{

    public partial class ViewReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                string manufacturer = Request.QueryString["manufacturer"];
                string model = Request.QueryString["model"];
                string revision = Request.QueryString["revision"];

                // Strip non-ascii, strip slashes and question marks
                if(manufacturer != null)
                    manufacturer = Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII, Encoding.UTF8.GetBytes(manufacturer))).Replace('/', '_').Replace('\\', '_').Replace('?', '_');
                if(model != null)
                    model = Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII, Encoding.UTF8.GetBytes(model))).Replace('/', '_').Replace('\\', '_').Replace('?', '_');
                if(revision != null)
                    revision = Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII, Encoding.UTF8.GetBytes(revision))).Replace('/', '_').Replace('\\', '_').Replace('?', '_');

                string xmlFile = null;
                if(!string.IsNullOrWhiteSpace(manufacturer)  && !string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(revision))
                    xmlFile = manufacturer + "_" + model + "_" + revision + ".xml";
                else if(!string.IsNullOrWhiteSpace(manufacturer) && !string.IsNullOrWhiteSpace(model))
                    xmlFile = manufacturer + "_" + model + ".xml";
                else if(!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(revision))
                    xmlFile = model + "_" + revision + ".xml";
                else if(!string.IsNullOrWhiteSpace(model))
                    xmlFile = model + ".xml";

                if(xmlFile==null || !File.Exists(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "Reports", xmlFile)))
                {
                    content.InnerHtml = "<b>Could not find the specified report</b>";
                    return;
                }

                lblManufacturer.Text = Request.QueryString["manufacturer"];
                lblModel.Text = Request.QueryString["model"];
                lblRevision.Text = Request.QueryString["revision"];

                DeviceReport report = new DeviceReport();
                XmlSerializer xs = new XmlSerializer(report.GetType());
                StreamReader sr = new StreamReader(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "Reports", xmlFile));
                report = (DeviceReport)xs.Deserialize(sr);
                sr.Close();

                if(report.USB != null)
                {
                    string usbVendorDescription = null;
                    string usbProductDescription = null;
                    GetUsbDescriptions(report.USB.VendorID, report.USB.ProductID, out usbVendorDescription, out usbProductDescription);

                    lblUsbManufacturer.Text = HttpUtility.HtmlEncode(report.USB.Manufacturer);
                    lblUsbProduct.Text = HttpUtility.HtmlEncode(report.USB.Product);
                    lblUsbVendor.Text = string.Format("0x{0:x4}", report.USB.VendorID);
                    if(usbVendorDescription != null)
                        lblUsbVendorDescription.Text = string.Format("({0})", HttpUtility.HtmlEncode(usbVendorDescription));
                    lblUsbProductId.Text = string.Format("0x{0:x4}", report.USB.ProductID);
                    if(usbProductDescription != null)
                        lblUsbProductDescription.Text = string.Format("({0})", HttpUtility.HtmlEncode(usbProductDescription));
                }
                else
                    divUsb.Visible = false;

                if(report.FireWire != null)
                {
                    lblFirewireManufacturer.Text = HttpUtility.HtmlEncode(report.FireWire.Manufacturer);
                    lblFirewireProduct.Text = HttpUtility.HtmlEncode(report.FireWire.Product);
                    lblFirewireVendor.Text = string.Format("0x{0:x8}", report.FireWire.VendorID);
                    lblFirewireProductId.Text = string.Format("0x{0:x8}", report.FireWire.ProductID);
                }
                else
                    divFirewire.Visible = false;

                if(report.PCMCIA != null)
                {
                    lblPcmciaManufacturer.Text = HttpUtility.HtmlEncode(report.PCMCIA.Manufacturer);
                    lblPcmciaProduct.Text = HttpUtility.HtmlEncode(report.PCMCIA.ProductName);
                    lblPcmciaManufacturerCode.Text = string.Format("0x{0:x4}", report.PCMCIA.ManufacturerCode);
                    lblPcmciaCardCode.Text = string.Format("0x{0:x4}", report.PCMCIA.CardCode);
                    lblPcmciaCompliance.Text = HttpUtility.HtmlEncode(report.PCMCIA.Compliance);
                    Decoders.PCMCIA.Tuple[] tuples = Decoders.PCMCIA.CIS.GetTuples(report.PCMCIA.CIS);
                    if(tuples != null)
                    {
                        Dictionary<string, string> decodedTuples = new Dictionary<string, string>();
                        foreach(Decoders.PCMCIA.Tuple tuple in tuples)
                        {
                            switch(tuple.Code)
                            {
                                case Decoders.PCMCIA.TupleCodes.CISTPL_NULL:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_END:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_MANFID:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_VERS_1:
                                    break;
                                case Decoders.PCMCIA.TupleCodes.CISTPL_DEVICEGEO:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_DEVICEGEO_A:
                                    Decoders.PCMCIA.DeviceGeometryTuple geom = Decoders.PCMCIA.CIS.DecodeDeviceGeometryTuple(tuple.Data);
                                    if(geom != null && geom.Geometries != null)
                                    {
                                        foreach(Decoders.PCMCIA.DeviceGeometry geometry in geom.Geometries)
                                        {
                                            decodedTuples.Add("Device width", string.Format("{0} bits", (1 << (geometry.CardInterface - 1)) * 8));
                                            decodedTuples.Add("Erase block", string.Format("{0} bytes", (1 << (geometry.EraseBlockSize - 1)) * (1 << (geometry.Interleaving - 1))));
                                            decodedTuples.Add("Read block", string.Format("{0} bytes", (1 << (geometry.ReadBlockSize - 1)) * (1 << (geometry.Interleaving - 1))));
                                            decodedTuples.Add("Write block", string.Format("{0} bytes", (1 << (geometry.WriteBlockSize - 1)) * (1 << (geometry.Interleaving - 1))));
                                            decodedTuples.Add("Partition alignment", string.Format("{0} bytes", (1 << (geometry.EraseBlockSize - 1)) * (1 << (geometry.Interleaving - 1)) * (1 << (geometry.Partitions - 1))));
                                        }
                                    }
                                    break;
                                case Decoders.PCMCIA.TupleCodes.CISTPL_ALTSTR:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_BAR:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_BATTERY:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_BYTEORDER:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_CFTABLE_ENTRY:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_CFTABLE_ENTRY_CB:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_CHECKSUM:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_CONFIG:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_CONFIG_CB:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_DATE:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_DEVICE:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_DEVICE_A:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_DEVICE_OA:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_DEVICE_OC:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_EXTDEVIC:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_FORMAT:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_FORMAT_A:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_FUNCE:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_FUNCID:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_GEOMETRY:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_INDIRECT:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_JEDEC_A:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_JEDEC_C:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_LINKTARGET:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_LONGLINK_A:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_LONGLINK_C:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_LONGLINK_CB:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_LONGLINK_MFC:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_NO_LINK:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_ORG:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_PWR_MGMNT:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_SPCL:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_SWIL:
                                case Decoders.PCMCIA.TupleCodes.CISTPL_VERS_2:
                                    decodedTuples.Add("Undecoded tuple ID", tuple.Code.ToString());
                                    break;
                                default:
                                    decodedTuples.Add("Unknown tuple ID", string.Format("0x{0:X2}", (byte)tuple.Code));
                                    break;

                            }
                        }

                        if(decodedTuples.Count > 0)
                        {
                            repPcmciaTuples.DataSource = decodedTuples;
                            repPcmciaTuples.DataBind();
                        }
                        else
                            repPcmciaTuples.Visible = false;
                    }
                    else
                        repPcmciaTuples.Visible = false;
                }
                else
                    divPcmcia.Visible = false;

                bool removable = true;
                testedMediaType[] testedMedia = null;
                bool ata = false;
                bool atapi = false;
                bool sscMedia = false;

                if(report.ATA != null || report.ATAPI != null)
                {
                    ata = true;
                    List<string> ataOneValue = new List<string>();
                    Dictionary<string, string> ataTwoValue = new Dictionary<string, string>();
                    ataType ataReport;

                    if(report.ATAPI != null)
                    {
                        lblAtapi.Text = "PI";
                        ataReport = report.ATAPI;
                        atapi = true;
                    }
                    else
                        ataReport = report.ATA;

                    bool cfa = report.CompactFlashSpecified && report.CompactFlash;

                    if(atapi && !cfa)
                        lblAtaDeviceType.Text = "ATAPI device";
                    else if(!atapi && cfa)
                        lblAtaDeviceType.Text = "CompactFlash device";
                    else
                        lblAtaDeviceType.Text = "ATA device";

                    Ata.Report(ataReport, cfa, atapi, ref removable, ref ataOneValue, ref ataTwoValue, ref testedMedia);

                    repAtaOne.DataSource = ataOneValue;
                    repAtaOne.DataBind();
                    repAtaTwo.DataSource = ataTwoValue;
                    repAtaTwo.DataBind();
                }
                else
                    divAta.Visible = false;

                if(report.SCSI != null)
                {
                    List<string> scsiOneValue = new List<string>();
                    Dictionary<string, string> modePages = new Dictionary<string, string>();
                    Dictionary<string, string> evpdPages = new Dictionary<string, string>();

                    if(VendorString.Prettify(report.SCSI.Inquiry.VendorIdentification) != report.SCSI.Inquiry.VendorIdentification)
                        lblScsiVendor.Text = string.Format("{0} ({1})", report.SCSI.Inquiry.VendorIdentification, VendorString.Prettify(report.SCSI.Inquiry.VendorIdentification));
                    else
                        lblScsiVendor.Text = report.SCSI.Inquiry.VendorIdentification;
                    lblScsiProduct.Text = report.SCSI.Inquiry.ProductIdentification;
                    lblScsiRevision.Text = report.SCSI.Inquiry.ProductRevisionLevel;

                    scsiOneValue.AddRange(ScsiInquiry.Report(report.SCSI.Inquiry));

                    if(report.SCSI.SupportsModeSense6)
                        scsiOneValue.Add("Device supports MODE SENSE (6)");
                    if(report.SCSI.SupportsModeSense10)
                        scsiOneValue.Add("Device supports MODE SENSE (10)");
                    if(report.SCSI.SupportsModeSubpages)
                        scsiOneValue.Add("Device supports MODE SENSE subpages");

                    if(report.SCSI.ModeSense != null)
                        ScsiModeSense.Report(report.SCSI.ModeSense, report.SCSI.Inquiry.VendorIdentification, report.SCSI.Inquiry.PeripheralDeviceType, ref scsiOneValue, ref modePages);

                    if(modePages.Count > 0)
                    {
                        repModeSense.DataSource = modePages;
                        repModeSense.DataBind();
                    }
                    else
                        divScsiModeSense.Visible = false;

                    if(report.SCSI.EVPDPages != null)
                        ScsiEvpd.Report(report.SCSI.EVPDPages, report.SCSI.Inquiry.VendorIdentification, ref evpdPages);

                    if(evpdPages.Count > 0)
                    {
                        repEvpd.DataSource = evpdPages;
                        repEvpd.DataBind();
                    }
                    else
                        divScsiEvpd.Visible = false;

                    divScsiMmcMode.Visible = false;
                    divScsiMmcFeatures.Visible = false;
                    divScsiSsc.Visible = false;

                    if(report.SCSI.MultiMediaDevice != null)
                    {
                        testedMedia = report.SCSI.MultiMediaDevice.TestedMedia;

                        if(report.SCSI.MultiMediaDevice.ModeSense2A != null)
                        {
                            List<string> mmcModeOneValue = new List<string>();
                            ScsiMmcMode.Report(report.SCSI.MultiMediaDevice.ModeSense2A, ref mmcModeOneValue);
                            if(mmcModeOneValue.Count > 0)
                            {
                                divScsiMmcMode.Visible = true;
                                repScsiMmcMode.DataSource = mmcModeOneValue;
                                repScsiMmcMode.DataBind();
                            }
                        }

                        if(report.SCSI.MultiMediaDevice.Features != null)
                        {
                            List<string> mmcFeaturesOneValue = new List<string>();
                            ScsiMmcFeatures.Report(report.SCSI.MultiMediaDevice.Features, ref mmcFeaturesOneValue);
                            if(mmcFeaturesOneValue.Count > 0)
                            {
                                divScsiMmcFeatures.Visible = true;
                                repScsiMmcFeatures.DataSource = mmcFeaturesOneValue;
                                repScsiMmcFeatures.DataBind();
                            }
                        }
                    }
                    else if(report.SCSI.SequentialDevice != null)
                    {
                        divScsiSsc.Visible = true;

                        if(report.SCSI.SequentialDevice.BlockSizeGranularitySpecified)
                            lblScsiSscGranularity.Text = report.SCSI.SequentialDevice.BlockSizeGranularity.ToString();
                        else
                            lblScsiSscGranularity.Text = "Unspecified";
                        
                        if(report.SCSI.SequentialDevice.MaxBlockLengthSpecified)
                            lblScsiSscMaxBlock.Text = report.SCSI.SequentialDevice.MaxBlockLength.ToString();
                        else
                            lblScsiSscMaxBlock.Text = "Unspecified";
                        
                        if(report.SCSI.SequentialDevice.MinBlockLengthSpecified)
                            lblScsiSscMinBlock.Text = report.SCSI.SequentialDevice.MinBlockLength.ToString();
                        else
                            lblScsiSscMinBlock.Text = "Unspecified";

                        if(report.SCSI.SequentialDevice.SupportedDensities != null)
                        {
                            repScsiSscDensities.DataSource = report.SCSI.SequentialDevice.SupportedDensities;
                            repScsiSscDensities.DataBind();
                        }
                        else
                            repScsiSscDensities.Visible = false;

                        if(report.SCSI.SequentialDevice.SupportedMediaTypes != null)
                        {
                            repScsiSscMedias.DataSource = report.SCSI.SequentialDevice.SupportedMediaTypes;
                            repScsiSscMedias.DataBind();
                        }
                        else
                            repScsiSscMedias.Visible = false;

                        if(report.SCSI.SequentialDevice.TestedMedia != null)
                        {
                            List<string> mediaOneValue = new List<string>();
                            SscTestedMedia.Report(report.SCSI.SequentialDevice.TestedMedia, ref mediaOneValue);
                            if(mediaOneValue.Count>0)
                            {
                                sscMedia = true;
                                repTestedMedia.DataSource = mediaOneValue;
                                repTestedMedia.DataBind();
                            }
                            else
                                divTestedMedia.Visible = false;
                        }
                        else
                            divTestedMedia.Visible = false;
                    }
                    else if(report.SCSI.ReadCapabilities != null)
                    {
                        removable = false;
                        scsiOneValue.Add("");

                        if(report.SCSI.ReadCapabilities.BlocksSpecified && report.SCSI.ReadCapabilities.BlockSizeSpecified)
                        {
                            scsiOneValue.Add(string.Format("Device has {0} blocks of {1} bytes each", report.SCSI.ReadCapabilities.Blocks, report.SCSI.ReadCapabilities.BlockSize));

                            if(((report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024) > 1000000)
                            {
                                scsiOneValue.Add(string.Format("Device size: {0} bytes, {1} Tb, {2:F2} TiB", report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize,
                                    (report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1000 / 1000 / 1000 / 1000, (double)(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024 / 1024 / 1024));
                            }
                            else if(((report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024) > 1000)
                            {
                                scsiOneValue.Add(string.Format("Device size: {0} bytes, {1} Gb, {2:F2} GiB", report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize,
                                    (report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1000 / 1000 / 1000, (double)(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024 / 1024));
                            }
                            else
                            {
                                scsiOneValue.Add(string.Format("Device size: {0} bytes, {1} Mb, {2:F2} MiB", report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize,
                                    (report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1000 / 1000, (double)(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024));
                            }
                        }

                        if(report.SCSI.ReadCapabilities.MediumTypeSpecified)
                            scsiOneValue.Add(string.Format("Medium type code: {0:X2}h", report.SCSI.ReadCapabilities.MediumType));
                        if(report.SCSI.ReadCapabilities.DensitySpecified)
                            scsiOneValue.Add(string.Format("Density code: {0:X2}h", report.SCSI.ReadCapabilities.Density));
                        if((report.SCSI.ReadCapabilities.SupportsReadLong || report.SCSI.ReadCapabilities.SupportsReadLong16) &&
                           report.SCSI.ReadCapabilities.LongBlockSizeSpecified)
                            scsiOneValue.Add(string.Format("Long block size: {0} bytes", report.SCSI.ReadCapabilities.LongBlockSize));
                        if(report.SCSI.ReadCapabilities.SupportsReadCapacity)
                            scsiOneValue.Add("Device supports READ CAPACITY (10) command.");
                        if(report.SCSI.ReadCapabilities.SupportsReadCapacity16)
                            scsiOneValue.Add("Device supports READ CAPACITY (16) command.");
                        if(report.SCSI.ReadCapabilities.SupportsRead)
                            scsiOneValue.Add("Device supports READ (6) command.");
                        if(report.SCSI.ReadCapabilities.SupportsRead10)
                            scsiOneValue.Add("Device supports READ (10) command.");
                        if(report.SCSI.ReadCapabilities.SupportsRead12)
                            scsiOneValue.Add("Device supports READ (12) command.");
                        if(report.SCSI.ReadCapabilities.SupportsRead16)
                            scsiOneValue.Add("Device supports READ (16) command.");
                        if(report.SCSI.ReadCapabilities.SupportsReadLong)
                            scsiOneValue.Add("Device supports READ LONG (10) command.");
                        if(report.SCSI.ReadCapabilities.SupportsReadLong16)
                            scsiOneValue.Add("Device supports READ LONG (16) command.");
                    }
                    else
                        testedMedia = report.SCSI.RemovableMedias;

                    repScsi.DataSource = scsiOneValue;
                    repScsi.DataBind();
                }
                else
                    divScsi.Visible = false;

                if(removable && !sscMedia && testedMedia!=null)
                {
                    List<string> mediaOneValue = new List<string>();
                    TestedMedia.Report(testedMedia, ata, ref mediaOneValue);
                    if(mediaOneValue.Count > 0)
                    {
                        sscMedia = true;
                        repTestedMedia.DataSource = mediaOneValue;
                        repTestedMedia.DataBind();
                    }
                    else
                        divTestedMedia.Visible = false;
                }
                else divTestedMedia.Visible &= sscMedia;
            }
            catch(Exception)
            {
                content.InnerHtml = "<b>Could not load device report</b>";
#if DEBUG
                throw;
#endif
            }
        }

        static void GetUsbDescriptions(ushort vendor, ushort product, out string vendorDescription, out string productDescription)
        {
            vendorDescription = null;
            productDescription = null;

            if(!File.Exists(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "usb.ids")))
                return;

            StreamReader tocStream = new StreamReader(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "usb.ids"));
            string _line;
            bool inManufacturer = false;
            ushort number;

            while(tocStream.Peek() >= 0)
            {
                _line = tocStream.ReadLine();

                if(_line.Length == 0 || _line[0] == '#')
                    continue;

                if(inManufacturer)
                {
                    // Finished with the manufacturer
                    if(_line[0] != '\t')
                        return;

                    number = Convert.ToUInt16(_line.Substring(1, 4), 16);

                    if(number == product)
                    {
                        productDescription = _line.Substring(7);
                        return;
                    }
                }
                else
                {
                    // Skip products
                    if(_line[0] == '\t')
                        continue;

                    try
                    {
                        number = Convert.ToUInt16(_line.Substring(0, 4), 16);
                    }
                    catch(FormatException)
                    {
                        continue;
                    }

                    if(number == vendor)
                    {
                        vendorDescription = _line.Substring(6);
                        inManufacturer = true;
                    }
                }
            }
        }
    }
}
