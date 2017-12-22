// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ViewReport.aspx.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Renders a device report.
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
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Xml.Serialization;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Metadata;
using DiscImageChef.Server.App_Start;
using Tuple = DiscImageChef.Decoders.PCMCIA.Tuple;

namespace DiscImageChef.Server
{
    public partial class ViewReport : Page
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
                    manufacturer = Encoding
                        .ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII,
                                                          Encoding.UTF8.GetBytes(manufacturer))).Replace('/', '_')
                        .Replace('\\', '_').Replace('?', '_');
                if(model != null)
                    model = Encoding
                        .ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII, Encoding.UTF8.GetBytes(model)))
                        .Replace('/', '_').Replace('\\', '_').Replace('?', '_');
                if(revision != null)
                    revision = Encoding
                        .ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII,
                                                          Encoding.UTF8.GetBytes(revision))).Replace('/', '_')
                        .Replace('\\', '_').Replace('?', '_');

                string xmlFile = null;
                if(!string.IsNullOrWhiteSpace(manufacturer) && !string.IsNullOrWhiteSpace(model) &&
                   !string.IsNullOrWhiteSpace(revision)) xmlFile = manufacturer + "_" + model + "_" + revision + ".xml";
                else if(!string.IsNullOrWhiteSpace(manufacturer) && !string.IsNullOrWhiteSpace(model))
                    xmlFile = manufacturer + "_" + model + ".xml";
                else if(!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(revision))
                    xmlFile = model + "_" + revision + ".xml";
                else if(!string.IsNullOrWhiteSpace(model)) xmlFile = model + ".xml";

                if(xmlFile == null ||
                   !File.Exists(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "Reports", xmlFile)))
                {
                    content.InnerHtml = "<b>Could not find the specified report</b>";
                    return;
                }

                lblManufacturer.Text = Request.QueryString["manufacturer"];
                lblModel.Text = Request.QueryString["model"];
                lblRevision.Text = Request.QueryString["revision"];

                DeviceReport report = new DeviceReport();
                XmlSerializer xs = new XmlSerializer(report.GetType());
                StreamReader sr =
                    new StreamReader(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "Reports",
                                                  xmlFile));
                report = (DeviceReport)xs.Deserialize(sr);
                sr.Close();

                if(report.USB != null)
                {
                    GetUsbDescriptions(report.USB.VendorID, report.USB.ProductID, out string usbVendorDescription,
                                       out string usbProductDescription);

                    lblUsbManufacturer.Text = HttpUtility.HtmlEncode(report.USB.Manufacturer);
                    lblUsbProduct.Text = HttpUtility.HtmlEncode(report.USB.Product);
                    lblUsbVendor.Text = $"0x{report.USB.VendorID:x4}";
                    if(usbVendorDescription != null)
                        lblUsbVendorDescription.Text = $"({HttpUtility.HtmlEncode(usbVendorDescription)})";
                    lblUsbProductId.Text = $"0x{report.USB.ProductID:x4}";
                    if(usbProductDescription != null)
                        lblUsbProductDescription.Text = $"({HttpUtility.HtmlEncode(usbProductDescription)})";
                }
                else divUsb.Visible = false;

                if(report.FireWire != null)
                {
                    lblFirewireManufacturer.Text = HttpUtility.HtmlEncode(report.FireWire.Manufacturer);
                    lblFirewireProduct.Text = HttpUtility.HtmlEncode(report.FireWire.Product);
                    lblFirewireVendor.Text = $"0x{report.FireWire.VendorID:x8}";
                    lblFirewireProductId.Text = $"0x{report.FireWire.ProductID:x8}";
                }
                else divFirewire.Visible = false;

                if(report.PCMCIA != null)
                {
                    lblPcmciaManufacturer.Text = HttpUtility.HtmlEncode(report.PCMCIA.Manufacturer);
                    lblPcmciaProduct.Text = HttpUtility.HtmlEncode(report.PCMCIA.ProductName);
                    lblPcmciaManufacturerCode.Text = $"0x{report.PCMCIA.ManufacturerCode:x4}";
                    lblPcmciaCardCode.Text = $"0x{report.PCMCIA.CardCode:x4}";
                    lblPcmciaCompliance.Text = HttpUtility.HtmlEncode(report.PCMCIA.Compliance);
                    Tuple[] tuples = CIS.GetTuples(report.PCMCIA.CIS);
                    if(tuples != null)
                    {
                        Dictionary<string, string> decodedTuples = new Dictionary<string, string>();
                        foreach(Tuple tuple in tuples)
                            switch(tuple.Code)
                            {
                                case TupleCodes.CISTPL_NULL:
                                case TupleCodes.CISTPL_END:
                                case TupleCodes.CISTPL_MANFID:
                                case TupleCodes.CISTPL_VERS_1: break;
                                case TupleCodes.CISTPL_DEVICEGEO:
                                case TupleCodes.CISTPL_DEVICEGEO_A:
                                    DeviceGeometryTuple geom =
                                        CIS.DecodeDeviceGeometryTuple(tuple.Data);
                                    if(geom?.Geometries != null)
                                        foreach(DeviceGeometry geometry in geom.Geometries)
                                        {
                                            decodedTuples.Add("Device width",
                                                              $"{(1 << (geometry.CardInterface - 1)) * 8} bits");
                                            decodedTuples.Add("Erase block",
                                                              $"{(1 << (geometry.EraseBlockSize - 1)) * (1 << (geometry.Interleaving - 1))} bytes");
                                            decodedTuples.Add("Read block",
                                                              $"{(1 << (geometry.ReadBlockSize - 1)) * (1 << (geometry.Interleaving - 1))} bytes");
                                            decodedTuples.Add("Write block",
                                                              $"{(1 << (geometry.WriteBlockSize - 1)) * (1 << (geometry.Interleaving - 1))} bytes");
                                            decodedTuples.Add("Partition alignment",
                                                              $"{(1 << (geometry.EraseBlockSize - 1)) * (1 << (geometry.Interleaving - 1)) * (1 << (geometry.Partitions - 1))} bytes");
                                        }

                                    break;
                                case TupleCodes.CISTPL_ALTSTR:
                                case TupleCodes.CISTPL_BAR:
                                case TupleCodes.CISTPL_BATTERY:
                                case TupleCodes.CISTPL_BYTEORDER:
                                case TupleCodes.CISTPL_CFTABLE_ENTRY:
                                case TupleCodes.CISTPL_CFTABLE_ENTRY_CB:
                                case TupleCodes.CISTPL_CHECKSUM:
                                case TupleCodes.CISTPL_CONFIG:
                                case TupleCodes.CISTPL_CONFIG_CB:
                                case TupleCodes.CISTPL_DATE:
                                case TupleCodes.CISTPL_DEVICE:
                                case TupleCodes.CISTPL_DEVICE_A:
                                case TupleCodes.CISTPL_DEVICE_OA:
                                case TupleCodes.CISTPL_DEVICE_OC:
                                case TupleCodes.CISTPL_EXTDEVIC:
                                case TupleCodes.CISTPL_FORMAT:
                                case TupleCodes.CISTPL_FORMAT_A:
                                case TupleCodes.CISTPL_FUNCE:
                                case TupleCodes.CISTPL_FUNCID:
                                case TupleCodes.CISTPL_GEOMETRY:
                                case TupleCodes.CISTPL_INDIRECT:
                                case TupleCodes.CISTPL_JEDEC_A:
                                case TupleCodes.CISTPL_JEDEC_C:
                                case TupleCodes.CISTPL_LINKTARGET:
                                case TupleCodes.CISTPL_LONGLINK_A:
                                case TupleCodes.CISTPL_LONGLINK_C:
                                case TupleCodes.CISTPL_LONGLINK_CB:
                                case TupleCodes.CISTPL_LONGLINK_MFC:
                                case TupleCodes.CISTPL_NO_LINK:
                                case TupleCodes.CISTPL_ORG:
                                case TupleCodes.CISTPL_PWR_MGMNT:
                                case TupleCodes.CISTPL_SPCL:
                                case TupleCodes.CISTPL_SWIL:
                                case TupleCodes.CISTPL_VERS_2:
                                    decodedTuples.Add("Undecoded tuple ID", tuple.Code.ToString());
                                    break;
                                default:
                                    decodedTuples.Add("Unknown tuple ID", $"0x{(byte)tuple.Code:X2}");
                                    break;
                            }

                        if(decodedTuples.Count > 0)
                        {
                            repPcmciaTuples.DataSource = decodedTuples;
                            repPcmciaTuples.DataBind();
                        }
                        else repPcmciaTuples.Visible = false;
                    }
                    else repPcmciaTuples.Visible = false;
                }
                else divPcmcia.Visible = false;

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
                    else ataReport = report.ATA;

                    bool cfa = report.CompactFlashSpecified && report.CompactFlash;

                    if(atapi && !cfa) lblAtaDeviceType.Text = "ATAPI device";
                    else if(!atapi && cfa) lblAtaDeviceType.Text = "CompactFlash device";
                    else lblAtaDeviceType.Text = "ATA device";

                    Ata.Report(ataReport, cfa, atapi, ref removable, ref ataOneValue, ref ataTwoValue, ref testedMedia);

                    repAtaOne.DataSource = ataOneValue;
                    repAtaOne.DataBind();
                    repAtaTwo.DataSource = ataTwoValue;
                    repAtaTwo.DataBind();
                }
                else divAta.Visible = false;

                if(report.SCSI != null)
                {
                    List<string> scsiOneValue = new List<string>();
                    Dictionary<string, string> modePages = new Dictionary<string, string>();
                    Dictionary<string, string> evpdPages = new Dictionary<string, string>();

                    lblScsiVendor.Text = VendorString.Prettify(report.SCSI.Inquiry.VendorIdentification) !=
                                         report.SCSI.Inquiry.VendorIdentification ? $"{report.SCSI.Inquiry.VendorIdentification} ({VendorString.Prettify(report.SCSI.Inquiry.VendorIdentification)})" : report.SCSI.Inquiry.VendorIdentification;
                    lblScsiProduct.Text = report.SCSI.Inquiry.ProductIdentification;
                    lblScsiRevision.Text = report.SCSI.Inquiry.ProductRevisionLevel;

                    scsiOneValue.AddRange(ScsiInquiry.Report(report.SCSI.Inquiry));

                    if(report.SCSI.SupportsModeSense6) scsiOneValue.Add("Device supports MODE SENSE (6)");
                    if(report.SCSI.SupportsModeSense10) scsiOneValue.Add("Device supports MODE SENSE (10)");
                    if(report.SCSI.SupportsModeSubpages) scsiOneValue.Add("Device supports MODE SENSE subpages");

                    if(report.SCSI.ModeSense != null)
                        ScsiModeSense.Report(report.SCSI.ModeSense, report.SCSI.Inquiry.VendorIdentification,
                                             report.SCSI.Inquiry.PeripheralDeviceType, ref scsiOneValue, ref modePages);

                    if(modePages.Count > 0)
                    {
                        repModeSense.DataSource = modePages;
                        repModeSense.DataBind();
                    }
                    else divScsiModeSense.Visible = false;

                    if(report.SCSI.EVPDPages != null)
                        ScsiEvpd.Report(report.SCSI.EVPDPages, report.SCSI.Inquiry.VendorIdentification, ref evpdPages);

                    if(evpdPages.Count > 0)
                    {
                        repEvpd.DataSource = evpdPages;
                        repEvpd.DataBind();
                    }
                    else divScsiEvpd.Visible = false;

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

                        lblScsiSscGranularity.Text = report.SCSI.SequentialDevice.BlockSizeGranularitySpecified ? report.SCSI.SequentialDevice.BlockSizeGranularity.ToString() : "Unspecified";

                        lblScsiSscMaxBlock.Text = report.SCSI.SequentialDevice.MaxBlockLengthSpecified ? report.SCSI.SequentialDevice.MaxBlockLength.ToString() : "Unspecified";

                        lblScsiSscMinBlock.Text = report.SCSI.SequentialDevice.MinBlockLengthSpecified ? report.SCSI.SequentialDevice.MinBlockLength.ToString() : "Unspecified";

                        if(report.SCSI.SequentialDevice.SupportedDensities != null)
                        {
                            repScsiSscDensities.DataSource = report.SCSI.SequentialDevice.SupportedDensities;
                            repScsiSscDensities.DataBind();
                        }
                        else repScsiSscDensities.Visible = false;

                        if(report.SCSI.SequentialDevice.SupportedMediaTypes != null)
                        {
                            repScsiSscMedias.DataSource = report.SCSI.SequentialDevice.SupportedMediaTypes;
                            repScsiSscMedias.DataBind();
                        }
                        else repScsiSscMedias.Visible = false;

                        if(report.SCSI.SequentialDevice.TestedMedia != null)
                        {
                            List<string> mediaOneValue = new List<string>();
                            SscTestedMedia.Report(report.SCSI.SequentialDevice.TestedMedia, ref mediaOneValue);
                            if(mediaOneValue.Count > 0)
                            {
                                sscMedia = true;
                                repTestedMedia.DataSource = mediaOneValue;
                                repTestedMedia.DataBind();
                            }
                            else divTestedMedia.Visible = false;
                        }
                        else divTestedMedia.Visible = false;
                    }
                    else if(report.SCSI.ReadCapabilities != null)
                    {
                        removable = false;
                        scsiOneValue.Add("");

                        if(report.SCSI.ReadCapabilities.BlocksSpecified &&
                           report.SCSI.ReadCapabilities.BlockSizeSpecified)
                        {
                            scsiOneValue.Add($"Device has {report.SCSI.ReadCapabilities.Blocks} blocks of {report.SCSI.ReadCapabilities.BlockSize} bytes each");

                            if(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize / 1024 /
                               1024 > 1000000)
                                scsiOneValue.Add($"Device size: {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize} bytes, {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize / 1000 / 1000 / 1000 / 1000} Tb, {(double)(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024 / 1024 / 1024:F2} TiB");
                            else if(
                                report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize / 1024 /
                                1024 > 1000)
                                scsiOneValue.Add($"Device size: {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize} bytes, {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize / 1000 / 1000 / 1000} Gb, {(double)(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024 / 1024:F2} GiB");
                            else
                                scsiOneValue.Add($"Device size: {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize} bytes, {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize / 1000 / 1000} Mb, {(double)(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024:F2} MiB");
                        }

                        if(report.SCSI.ReadCapabilities.MediumTypeSpecified)
                            scsiOneValue.Add($"Medium type code: {report.SCSI.ReadCapabilities.MediumType:X2}h");
                        if(report.SCSI.ReadCapabilities.DensitySpecified)
                            scsiOneValue.Add($"Density code: {report.SCSI.ReadCapabilities.Density:X2}h");
                        if((report.SCSI.ReadCapabilities.SupportsReadLong ||
                            report.SCSI.ReadCapabilities.SupportsReadLong16) &&
                           report.SCSI.ReadCapabilities.LongBlockSizeSpecified)
                            scsiOneValue.Add($"Long block size: {report.SCSI.ReadCapabilities.LongBlockSize} bytes");
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
                    else testedMedia = report.SCSI.RemovableMedias;

                    repScsi.DataSource = scsiOneValue;
                    repScsi.DataBind();
                }
                else divScsi.Visible = false;

                if(report.MultiMediaCard != null)
                {
                    List<string> mmcOneValue = new List<string>();

                    if(report.MultiMediaCard.CID != null)
                    {
                        mmcOneValue.Add(Decoders.MMC.Decoders.PrettifyCID(report.MultiMediaCard.CID)
                                                .Replace("\n", "<br/>"));
                        mmcOneValue.Add("");
                    }

                    if(report.MultiMediaCard.CSD != null)
                    {
                        mmcOneValue.Add(Decoders.MMC.Decoders.PrettifyCSD(report.MultiMediaCard.CSD)
                                                .Replace("\n", "<br/>"));
                        mmcOneValue.Add("");
                    }

                    if(report.MultiMediaCard.ExtendedCSD != null)
                    {
                        mmcOneValue.Add(Decoders.MMC.Decoders.PrettifyExtendedCSD(report.MultiMediaCard.ExtendedCSD)
                                                .Replace("\n", "<br/>"));
                        mmcOneValue.Add("");
                    }

                    if(report.MultiMediaCard.OCR != null)
                    {
                        mmcOneValue.Add(Decoders.MMC.Decoders.PrettifyCSD(report.MultiMediaCard.OCR)
                                                .Replace("\n", "<br/>"));
                        mmcOneValue.Add("");
                    }

                    repMMC.DataSource = mmcOneValue;
                    repMMC.DataBind();
                }
                else divMMC.Visible = false;

                if(report.SecureDigital != null)
                {
                    List<string> sdOneValue = new List<string>();

                    if(report.SecureDigital.CID != null)
                    {
                        sdOneValue.Add(Decoders.SecureDigital.Decoders.PrettifyCID(report.SecureDigital.CID)
                                               .Replace("\n", "<br/>"));
                        sdOneValue.Add("");
                    }

                    if(report.SecureDigital.CSD != null)
                    {
                        sdOneValue.Add(Decoders.SecureDigital.Decoders.PrettifyCSD(report.SecureDigital.CSD)
                                               .Replace("\n", "<br/>"));
                        sdOneValue.Add("");
                    }

                    if(report.SecureDigital.SCR != null)
                    {
                        sdOneValue.Add(Decoders.SecureDigital.Decoders.PrettifySCR(report.SecureDigital.SCR)
                                               .Replace("\n", "<br/>"));
                        sdOneValue.Add("");
                    }

                    if(report.SecureDigital.OCR != null)
                    {
                        sdOneValue.Add(Decoders.SecureDigital.Decoders.PrettifyCSD(report.SecureDigital.OCR)
                                               .Replace("\n", "<br/>"));
                        sdOneValue.Add("");
                    }

                    repSD.DataSource = sdOneValue;
                    repSD.DataBind();
                }
                else divSD.Visible = false;

                if(removable && !sscMedia && testedMedia != null)
                {
                    List<string> mediaOneValue = new List<string>();
                    TestedMedia.Report(testedMedia, ata, ref mediaOneValue);
                    if(mediaOneValue.Count > 0)
                    {
                        divTestedMedia.Visible = true;
                        repTestedMedia.DataSource = mediaOneValue;
                        repTestedMedia.DataBind();
                    }
                    else divTestedMedia.Visible = false;
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

        static void GetUsbDescriptions(ushort vendor, ushort product, out string vendorDescription,
                                       out string productDescription)
        {
            vendorDescription = null;
            productDescription = null;

            if(!File.Exists(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "usb.ids"))) return;

            StreamReader tocStream =
                new StreamReader(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(), "usb.ids"));
            bool inManufacturer = false;

            while(tocStream.Peek() >= 0)
            {
                string line = tocStream.ReadLine();

                if(line == null) break;

                if(line.Length == 0 || line[0] == '#') continue;

                ushort number;
                if(inManufacturer)
                {
                    // Finished with the manufacturer
                    if(line[0] != '\t') return;

                    number = Convert.ToUInt16(line.Substring(1, 4), 16);

                    if(number != product) continue;

                    productDescription = line.Substring(7);
                    return;
                }
                // Skip products
                if(line[0] == '\t') continue;

                try { number = Convert.ToUInt16(line.Substring(0, 4), 16); }
                catch(FormatException) { continue; }

                if(number != vendor) continue;

                vendorDescription = line.Substring(6);
                inManufacturer = true;
            }
        }
    }
}