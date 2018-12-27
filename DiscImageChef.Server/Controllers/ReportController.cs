using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Decoders.PCMCIA;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Server.Models;
using Tuple = DiscImageChef.Decoders.PCMCIA.Tuple;

namespace DiscImageChef.Server.Controllers
{
    public class ReportController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("View", "Report", new RouteValueDictionary {{"id", 1}});
        }

        public ActionResult View(int? id)
        {
            if(id == null || id <= 0) return Content("Incorrect device report request");

            try
            {
                DicServerContext ctx    = new DicServerContext();
                Device           report = ctx.Devices.FirstOrDefault(d => d.Id == id);

                if(report is null) return Content("Cannot find requested report");

                ViewBag.lblManufacturer = report.Manufacturer;
                ViewBag.lblModel        = report.Model;
                ViewBag.lblRevision     = report.Revision;

                if(report.USB != null)
                {
                    string usbVendorDescription  = null;
                    string usbProductDescription = null;

                    UsbProduct dbProduct =
                        ctx.UsbProducts.FirstOrDefault(p => p.ProductId       == report.USB.ProductID &&
                                                            p.Vendor          != null                 &&
                                                            p.Vendor.VendorId == report.USB.VendorID);

                    if(dbProduct is null)
                    {
                        UsbVendor dbVendor = ctx.UsbVendors.FirstOrDefault(v => v.VendorId == report.USB.VendorID);

                        if(!(dbVendor is null)) usbVendorDescription = dbVendor.Vendor;
                    }
                    else
                    {
                        usbProductDescription = dbProduct.Product;
                        usbVendorDescription  = dbProduct.Vendor.Vendor;
                    }

                    ViewBag.UsbItem = new Item
                    {
                        Manufacturer = report.USB.Manufacturer,
                        Product      = report.USB.Product,
                        VendorDescription =
                            usbVendorDescription != null
                                ? $"0x{report.USB.VendorID:x4} ({usbVendorDescription})"
                                : $"0x{report.USB.VendorID:x4}",
                        ProductDescription = usbProductDescription != null
                                                 ? $"0x{report.USB.ProductID:x4} ({usbProductDescription})"
                                                 : $"0x{report.USB.ProductID:x4}"
                    };
                }

                if(report.FireWire != null)
                    ViewBag.FireWireItem = new Item
                    {
                        Manufacturer       = report.FireWire.Manufacturer,
                        Product            = report.FireWire.Product,
                        VendorDescription  = $"0x{report.FireWire.VendorID:x8}",
                        ProductDescription = $"0x{report.FireWire.ProductID:x8}"
                    };

                if(report.PCMCIA != null)
                {
                    ViewBag.PcmciaItem = new PcmciaItem
                    {
                        Manufacturer       = report.PCMCIA.Manufacturer,
                        Product            = report.PCMCIA.ProductName,
                        VendorDescription  = $"0x{report.PCMCIA.ManufacturerCode:x4}",
                        ProductDescription = $"0x{report.PCMCIA.CardCode:x4}",
                        Compliance         = report.PCMCIA.Compliance
                    };

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
                                    DeviceGeometryTuple geom = CIS.DecodeDeviceGeometryTuple(tuple.Data);
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

                        if(decodedTuples.Count > 0) ViewBag.repPcmciaTuples = decodedTuples;
                    }
                }

                bool              removable   = true;
                List<TestedMedia> testedMedia = null;
                bool              ata         = false;
                bool              atapi       = false;
                bool              sscMedia    = false;

                if(report.ATA != null || report.ATAPI != null)
                {
                    ata = true;
                    List<string>               ataOneValue = new List<string>();
                    Dictionary<string, string> ataTwoValue = new Dictionary<string, string>();
                    CommonTypes.Metadata.Ata   ataReport;

                    if(report.ATAPI != null)
                    {
                        ViewBag.AtaItem = "ATAPI";
                        ataReport       = report.ATAPI;
                        atapi           = true;
                    }
                    else
                    {
                        ViewBag.AtaItem = "ATA";
                        ataReport       = report.ATA;
                    }

                    bool cfa = report.CompactFlash;

                    if(atapi       && !cfa) ViewBag.lblAtaDeviceType = "ATAPI device";
                    else if(!atapi && cfa) ViewBag.lblAtaDeviceType  = "CompactFlash device";
                    else ViewBag.lblAtaDeviceType                    = "ATA device";

                    Ata.Report(ataReport, cfa, atapi, ref removable, ref ataOneValue, ref ataTwoValue, ref testedMedia);

                    ViewBag.repAtaOne = ataOneValue;
                    ViewBag.repAtaTwo = ataTwoValue;
                }

                if(report.SCSI != null)
                {
                    List<string>               scsiOneValue = new List<string>();
                    Dictionary<string, string> modePages    = new Dictionary<string, string>();
                    Dictionary<string, string> evpdPages    = new Dictionary<string, string>();

                    string vendorId = StringHandlers.CToString(report.SCSI.Inquiry?.VendorIdentification);
                    if(report.SCSI.Inquiry != null)
                    {
                        Inquiry.SCSIInquiry inq = report.SCSI.Inquiry.Value;
                        ViewBag.lblScsiVendor = VendorString.Prettify(vendorId) != vendorId
                                                    ? $"{vendorId} ({VendorString.Prettify(vendorId)})"
                                                    : vendorId;
                        ViewBag.lblScsiProduct  = StringHandlers.CToString(inq.ProductIdentification);
                        ViewBag.lblScsiRevision = StringHandlers.CToString(inq.ProductRevisionLevel);
                    }

                    scsiOneValue.AddRange(ScsiInquiry.Report(report.SCSI.Inquiry));

                    if(report.SCSI.SupportsModeSense6) scsiOneValue.Add("Device supports MODE SENSE (6)");
                    if(report.SCSI.SupportsModeSense10) scsiOneValue.Add("Device supports MODE SENSE (10)");
                    if(report.SCSI.SupportsModeSubpages) scsiOneValue.Add("Device supports MODE SENSE subpages");

                    if(report.SCSI.ModeSense != null)
                    {
                        PeripheralDeviceTypes devType = PeripheralDeviceTypes.DirectAccess;
                        if(report.SCSI.Inquiry != null)
                            devType = (PeripheralDeviceTypes)report.SCSI.Inquiry.Value.PeripheralDeviceType;
                        ScsiModeSense.Report(report.SCSI.ModeSense, vendorId, devType, ref scsiOneValue, ref modePages);
                    }

                    if(modePages.Count > 0) ViewBag.repModeSense = modePages;

                    if(report.SCSI.EVPDPages != null) ScsiEvpd.Report(report.SCSI.EVPDPages, vendorId, ref evpdPages);

                    if(evpdPages.Count > 0) ViewBag.repEvpd = evpdPages;

                    if(report.SCSI.MultiMediaDevice != null)
                    {
                        testedMedia = report.SCSI.MultiMediaDevice.TestedMedia;

                        if(report.SCSI.MultiMediaDevice.ModeSense2A != null)
                        {
                            List<string> mmcModeOneValue = new List<string>();
                            ScsiMmcMode.Report(report.SCSI.MultiMediaDevice.ModeSense2A, ref mmcModeOneValue);
                            if(mmcModeOneValue.Count > 0) ViewBag.repScsiMmcMode = mmcModeOneValue;
                        }

                        if(report.SCSI.MultiMediaDevice.Features != null)
                        {
                            List<string> mmcFeaturesOneValue = new List<string>();
                            ScsiMmcFeatures.Report(report.SCSI.MultiMediaDevice.Features, ref mmcFeaturesOneValue);
                            if(mmcFeaturesOneValue.Count > 0) ViewBag.repScsiMmcFeatures = mmcFeaturesOneValue;
                        }
                    }
                    else if(report.SCSI.SequentialDevice != null)
                    {
                        ViewBag.divScsiSscVisible = true;

                        ViewBag.lblScsiSscGranularity =
                            report.SCSI.SequentialDevice.BlockSizeGranularity?.ToString() ?? "Unspecified";

                        ViewBag.lblScsiSscMaxBlock =
                            report.SCSI.SequentialDevice.MaxBlockLength?.ToString() ?? "Unspecified";

                        ViewBag.lblScsiSscMinBlock =
                            report.SCSI.SequentialDevice.MinBlockLength?.ToString() ?? "Unspecified";

                        if(report.SCSI.SequentialDevice.SupportedDensities != null)
                            ViewBag.repScsiSscDensities = report.SCSI.SequentialDevice.SupportedDensities;

                        if(report.SCSI.SequentialDevice.SupportedMediaTypes != null)
                            ViewBag.repScsiSscMedias = report.SCSI.SequentialDevice.SupportedMediaTypes;

                        if(report.SCSI.SequentialDevice.TestedMedia != null)
                        {
                            List<string> mediaOneValue = new List<string>();
                            SscTestedMedia.Report(report.SCSI.SequentialDevice.TestedMedia, ref mediaOneValue);
                            if(mediaOneValue.Count > 0)
                            {
                                sscMedia               = true;
                                ViewBag.repTestedMedia = mediaOneValue;
                            }
                        }
                    }
                    else if(report.SCSI.ReadCapabilities != null)
                    {
                        removable = false;
                        scsiOneValue.Add("");

                        if(report.SCSI.ReadCapabilities.Blocks.HasValue &&
                           report.SCSI.ReadCapabilities.BlockSize.HasValue)
                        {
                            scsiOneValue
                               .Add($"Device has {report.SCSI.ReadCapabilities.Blocks} blocks of {report.SCSI.ReadCapabilities.BlockSize} bytes each");

                            if(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize / 1024 /
                               1024 > 1000000)
                                scsiOneValue
                                   .Add($"Device size: {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize} bytes, {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize / 1000 / 1000 / 1000 / 1000} Tb, {(double)(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024 / 1024 / 1024:F2} TiB");
                            else if(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize /
                                    1024                                                                         /
                                    1024 > 1000)
                                scsiOneValue
                                   .Add($"Device size: {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize} bytes, {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize / 1000 / 1000 / 1000} Gb, {(double)(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024 / 1024:F2} GiB");
                            else
                                scsiOneValue
                                   .Add($"Device size: {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize} bytes, {report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize / 1000 / 1000} Mb, {(double)(report.SCSI.ReadCapabilities.Blocks * report.SCSI.ReadCapabilities.BlockSize) / 1024 / 1024:F2} MiB");
                        }

                        if(report.SCSI.ReadCapabilities.MediumType.HasValue)
                            scsiOneValue.Add($"Medium type code: {report.SCSI.ReadCapabilities.MediumType:X2}h");
                        if(report.SCSI.ReadCapabilities.Density.HasValue)
                            scsiOneValue.Add($"Density code: {report.SCSI.ReadCapabilities.Density:X2}h");
                        if((report.SCSI.ReadCapabilities.SupportsReadLong   == true ||
                            report.SCSI.ReadCapabilities.SupportsReadLong16 == true) &&
                           report.SCSI.ReadCapabilities.LongBlockSize.HasValue)
                            scsiOneValue.Add($"Long block size: {report.SCSI.ReadCapabilities.LongBlockSize} bytes");
                        if(report.SCSI.ReadCapabilities.SupportsReadCapacity == true)
                            scsiOneValue.Add("Device supports READ CAPACITY (10) command.");
                        if(report.SCSI.ReadCapabilities.SupportsReadCapacity16 == true)
                            scsiOneValue.Add("Device supports READ CAPACITY (16) command.");
                        if(report.SCSI.ReadCapabilities.SupportsRead6 == true)
                            scsiOneValue.Add("Device supports READ (6) command.");
                        if(report.SCSI.ReadCapabilities.SupportsRead10 == true)
                            scsiOneValue.Add("Device supports READ (10) command.");
                        if(report.SCSI.ReadCapabilities.SupportsRead12 == true)
                            scsiOneValue.Add("Device supports READ (12) command.");
                        if(report.SCSI.ReadCapabilities.SupportsRead16 == true)
                            scsiOneValue.Add("Device supports READ (16) command.");
                        if(report.SCSI.ReadCapabilities.SupportsReadLong == true)
                            scsiOneValue.Add("Device supports READ LONG (10) command.");
                        if(report.SCSI.ReadCapabilities.SupportsReadLong16 == true)
                            scsiOneValue.Add("Device supports READ LONG (16) command.");
                    }
                    else testedMedia = report.SCSI.RemovableMedias;

                    ViewBag.repScsi = scsiOneValue;
                }

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

                    ViewBag.repMMC = mmcOneValue;
                }

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

                    ViewBag.repSD = sdOneValue;
                }

                if(removable && !sscMedia && testedMedia != null)
                {
                    List<string> mediaOneValue = new List<string>();
                    App_Start.TestedMedia.Report(testedMedia, ref mediaOneValue);
                    if(mediaOneValue.Count > 0) ViewBag.repTestedMedia = mediaOneValue;
                }
            }
            catch(Exception)
            {
                #if DEBUG
                throw;
                #endif
                return Content("Could not load device report");
            }

            return View();
        }
    }

    public class Item
    {
        public string Manufacturer;
        public string Product;
        public string ProductDescription;
        public string VendorDescription;
    }

    public class PcmciaItem : Item
    {
        public string Compliance;
    }
}