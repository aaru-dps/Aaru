// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DeviceReportV2.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : JSON metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains classes for a JSON device report.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using Newtonsoft.Json;

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable VirtualMemberCallInConstructor

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DiscImageChef.CommonTypes.Metadata
{
    public class DeviceReportV2
    {
        public DeviceReportV2() { }

        public DeviceReportV2(DeviceReport reportV1)
        {
            if(reportV1.USB != null)
                USB = new Usb(reportV1.USB);

            if(reportV1.FireWire != null)
                FireWire = new FireWire(reportV1.FireWire);

            if(reportV1.PCMCIA != null)
                PCMCIA = new Pcmcia(reportV1.PCMCIA);

            if(reportV1.CompactFlashSpecified)
                CompactFlash = reportV1.CompactFlash;

            if(reportV1.ATA != null)
            {
                ATA  = new Ata(reportV1.ATA);
                Type = DeviceType.ATA;
            }

            if(reportV1.SCSI != null)
            {
                SCSI = new Scsi(reportV1.SCSI);
                Type = DeviceType.SCSI;

                if(SCSI.ModeSense?.ModePages?.FirstOrDefault(p => p.page == 0x2A)?.value != null)
                {
                    if(SCSI.MultiMediaDevice != null)
                        SCSI.MultiMediaDevice.ModeSense2AData =
                            SCSI.ModeSense?.ModePages?.FirstOrDefault(p => p.page == 0x2A)?.value;
                    else if(SCSI.Inquiry?.PeripheralDeviceType == (byte)PeripheralDeviceTypes.MultiMediaDevice)
                        SCSI.MultiMediaDevice = new Mmc
                        {
                            ModeSense2AData = SCSI.ModeSense?.ModePages?.FirstOrDefault(p => p.page == 0x2A)?.value
                        };
                }
            }

            if(reportV1.ATAPI != null)
            {
                ATAPI = new Ata(reportV1.ATAPI);
                Type  = DeviceType.ATAPI;
            }

            if(reportV1.MultiMediaCard != null)
            {
                MultiMediaCard = new MmcSd(reportV1.MultiMediaCard);
                Type           = DeviceType.MMC;
            }

            if(reportV1.SecureDigital != null)
            {
                SecureDigital = new MmcSd(reportV1.SecureDigital);
                Type          = DeviceType.SecureDigital;
            }

            if(reportV1.SCSI?.Inquiry != null)
            {
                Manufacturer = reportV1.SCSI.Inquiry.VendorIdentification;
                Model        = reportV1.SCSI.Inquiry.ProductIdentification;
                Revision     = reportV1.SCSI.Inquiry.ProductRevisionLevel;
            }
            else if(reportV1.ATA   != null ||
                    reportV1.ATAPI != null)
            {
                ataType ata = reportV1.ATA ?? reportV1.ATAPI;

                string[] split = ata.Model.Split(' ');

                if(split.Length > 1)
                {
                    Manufacturer = split[0];
                    Model        = string.Join(" ", split, 1, split.Length - 1);
                }
                else
                    Model = ata.Model;

                Revision = ata.FirmwareRevision;
            }
        }

        [JsonIgnore]
        public int Id { get;                          set; }
        public virtual Usb      USB            { get; set; }
        public virtual FireWire FireWire       { get; set; }
        public virtual Pcmcia   PCMCIA         { get; set; }
        public         bool     CompactFlash   { get; set; }
        public virtual Ata      ATA            { get; set; }
        public virtual Ata      ATAPI          { get; set; }
        public virtual Scsi     SCSI           { get; set; }
        public virtual MmcSd    MultiMediaCard { get; set; }
        public virtual MmcSd    SecureDigital  { get; set; }

        public string     Manufacturer { get; set; }
        public string     Model        { get; set; }
        public string     Revision     { get; set; }
        public DeviceType Type         { get; set; }
    }

    public class Usb
    {
        public Usb() { }

        public Usb(usbType usb)
        {
            VendorID       = usb.VendorID;
            ProductID      = usb.ProductID;
            Manufacturer   = usb.Manufacturer;
            Product        = usb.Product;
            RemovableMedia = usb.RemovableMedia;
            Descriptors    = usb.Descriptors;
        }

        [JsonIgnore]
        public int Id { get;                set; }
        public ushort VendorID       { get; set; }
        public ushort ProductID      { get; set; }
        public string Manufacturer   { get; set; }
        public string Product        { get; set; }
        public bool   RemovableMedia { get; set; }
        public byte[] Descriptors    { get; set; }
    }

    public class FireWire
    {
        public FireWire() { }

        public FireWire(firewireType firewire)
        {
            VendorID       = firewire.VendorID;
            ProductID      = firewire.ProductID;
            Manufacturer   = firewire.Manufacturer;
            Product        = firewire.Product;
            RemovableMedia = firewire.RemovableMedia;
        }

        [JsonIgnore]
        public int Id { get; set; }
        [DisplayName("Vendor ID"), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "0x{0:X8}")]
        public uint VendorID { get; set; }
        [DisplayName("Product ID"), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "0x{0:X8}")]
        public uint ProductID { get; set; }
        [DisplayFormat(NullDisplayText = "Unknown")]
        public string Manufacturer { get; set; }
        [DisplayFormat(NullDisplayText = "Unknown")]
        public string Product { get; set; }
        [DisplayName("Is media removable?")]
        public bool RemovableMedia { get; set; }
    }

    public class Ata
    {
        public Ata() { }

        public Ata(ataType ata)
        {
            Identify = ata.Identify;

            if(ata.ReadCapabilities != null)
                ReadCapabilities = new TestedMedia(ata.ReadCapabilities, true);

            if(ata.RemovableMedias != null)
            {
                RemovableMedias = new List<TestedMedia>();

                foreach(testedMediaType ataRemovableMedia in ata.RemovableMedias)
                    RemovableMedias.Add(new TestedMedia(ataRemovableMedia, true));
            }

            if(Identify != null)
                return;

            var identifyDevice = new Identify.IdentifyDevice();

            if(ata.AdditionalPIDSpecified)
                identifyDevice.AdditionalPID = ata.AdditionalPID;

            if(ata.APIOSupportedSpecified)
                identifyDevice.APIOSupported = ata.APIOSupported;

            if(ata.ATAPIByteCountSpecified)
                identifyDevice.ATAPIByteCount = ata.ATAPIByteCount;

            if(ata.BufferTypeSpecified)
                identifyDevice.BufferType = ata.BufferType;

            if(ata.BufferSizeSpecified)
                identifyDevice.BufferSize = ata.BufferSize;

            if(ata.CapabilitiesSpecified)
                identifyDevice.Capabilities = ata.Capabilities;

            if(ata.Capabilities2Specified)
                identifyDevice.Capabilities2 = ata.Capabilities2;

            if(ata.Capabilities3Specified)
                identifyDevice.Capabilities3 = ata.Capabilities3;

            if(ata.CFAPowerModeSpecified)
                identifyDevice.CFAPowerMode = ata.CFAPowerMode;

            if(ata.CommandSetSpecified)
                identifyDevice.CommandSet = ata.CommandSet;

            if(ata.CommandSet2Specified)
                identifyDevice.CommandSet2 = ata.CommandSet2;

            if(ata.CommandSet3Specified)
                identifyDevice.CommandSet3 = ata.CommandSet3;

            if(ata.CommandSet4Specified)
                identifyDevice.CommandSet4 = ata.CommandSet4;

            if(ata.CommandSet5Specified)
                identifyDevice.CommandSet4 = ata.CommandSet4;

            if(ata.CurrentAAMSpecified)
                identifyDevice.CurrentAAM = ata.CurrentAAM;

            if(ata.CurrentAPMSpecified)
                identifyDevice.CurrentAPM = ata.CurrentAPM;

            if(ata.DataSetMgmtSpecified)
                identifyDevice.DataSetMgmt = ata.DataSetMgmt;

            if(ata.DataSetMgmtSizeSpecified)
                identifyDevice.DataSetMgmtSize = ata.DataSetMgmtSize;

            if(ata.DeviceFormFactorSpecified)
                identifyDevice.DeviceFormFactor = ata.DeviceFormFactor;

            if(ata.DMAActiveSpecified)
                identifyDevice.DMAActive = ata.DMAActive;

            if(ata.DMASupportedSpecified)
                identifyDevice.DMASupported = ata.DMASupported;

            if(ata.DMATransferTimingModeSpecified)
                identifyDevice.DMATransferTimingMode = ata.DMATransferTimingMode;

            if(ata.EnhancedSecurityEraseTimeSpecified)
                identifyDevice.EnhancedSecurityEraseTime = ata.EnhancedSecurityEraseTime;

            if(ata.EnabledCommandSetSpecified)
                identifyDevice.EnabledCommandSet = ata.EnabledCommandSet;

            if(ata.EnabledCommandSet2Specified)
                identifyDevice.EnabledCommandSet2 = ata.EnabledCommandSet2;

            if(ata.EnabledCommandSet3Specified)
                identifyDevice.EnabledCommandSet3 = ata.EnabledCommandSet3;

            if(ata.EnabledCommandSet4Specified)
                identifyDevice.EnabledCommandSet4 = ata.EnabledCommandSet4;

            if(ata.EnabledSATAFeaturesSpecified)
                identifyDevice.EnabledSATAFeatures = ata.EnabledSATAFeatures;

            if(ata.ExtendedUserSectorsSpecified)
                identifyDevice.ExtendedUserSectors = ata.ExtendedUserSectors;

            if(ata.FreeFallSensitivitySpecified)
                identifyDevice.FreeFallSensitivity = ata.FreeFallSensitivity;

            if(ata.FirmwareRevisionSpecified)
                identifyDevice.FirmwareRevision = ata.FirmwareRevision;

            if(ata.GeneralConfigurationSpecified)
                identifyDevice.GeneralConfiguration = ata.GeneralConfiguration;

            if(ata.HardwareResetResultSpecified)
                identifyDevice.HardwareResetResult = ata.HardwareResetResult;

            if(ata.InterseekDelaySpecified)
                identifyDevice.InterseekDelay = ata.InterseekDelay;

            if(ata.MajorVersionSpecified)
                identifyDevice.MajorVersion = ata.MajorVersion;

            if(ata.MasterPasswordRevisionCodeSpecified)
                identifyDevice.MasterPasswordRevisionCode = ata.MasterPasswordRevisionCode;

            if(ata.MaxDownloadMicroMode3Specified)
                identifyDevice.MaxDownloadMicroMode3 = ata.MaxDownloadMicroMode3;

            if(ata.MaxQueueDepthSpecified)
                identifyDevice.MaxQueueDepth = ata.MaxQueueDepth;

            if(ata.MDMAActiveSpecified)
                identifyDevice.MDMAActive = ata.MDMAActive;

            if(ata.MDMASupportedSpecified)
                identifyDevice.MDMASupported = ata.MDMASupported;

            if(ata.MinDownloadMicroMode3Specified)
                identifyDevice.MinDownloadMicroMode3 = ata.MinDownloadMicroMode3;

            if(ata.MinMDMACycleTimeSpecified)
                identifyDevice.MinMDMACycleTime = ata.MinMDMACycleTime;

            if(ata.MinorVersionSpecified)
                identifyDevice.MinorVersion = ata.MinorVersion;

            if(ata.MinPIOCycleTimeNoFlowSpecified)
                identifyDevice.MinPIOCycleTimeNoFlow = ata.MinPIOCycleTimeNoFlow;

            if(ata.MinPIOCycleTimeFlowSpecified)
                identifyDevice.MinPIOCycleTimeFlow = ata.MinPIOCycleTimeFlow;

            if(ata.ModelSpecified)
                identifyDevice.Model = ata.Model;

            if(ata.MultipleMaxSectorsSpecified)
                identifyDevice.MultipleMaxSectors = ata.MultipleMaxSectors;

            if(ata.MultipleSectorNumberSpecified)
                identifyDevice.MultipleSectorNumber = ata.MultipleSectorNumber;

            if(ata.NVCacheCapsSpecified)
                identifyDevice.NVCacheCaps = ata.NVCacheCaps;

            if(ata.NVCacheSizeSpecified)
                identifyDevice.NVCacheSize = ata.NVCacheSize;

            if(ata.NVCacheWriteSpeedSpecified)
                identifyDevice.NVCacheWriteSpeed = ata.NVCacheWriteSpeed;

            if(ata.NVEstimatedSpinUpSpecified)
                identifyDevice.NVEstimatedSpinUp = ata.NVEstimatedSpinUp;

            if(ata.PacketBusReleaseSpecified)
                identifyDevice.PacketBusRelease = ata.PacketBusRelease;

            if(ata.PIOTransferTimingModeSpecified)
                identifyDevice.PIOTransferTimingMode = ata.PIOTransferTimingMode;

            if(ata.RecommendedAAMSpecified)
                identifyDevice.RecommendedAAM = ata.RecommendedAAM;

            if(ata.RecommendedMDMACycleTimeSpecified)
                identifyDevice.RecMDMACycleTime = ata.RecommendedMDMACycleTime;

            if(ata.RemovableStatusSetSpecified)
                identifyDevice.RemovableStatusSet = ata.RemovableStatusSet;

            if(ata.SATACapabilitiesSpecified)
                identifyDevice.SATACapabilities = ata.SATACapabilities;

            if(ata.SATACapabilities2Specified)
                identifyDevice.SATACapabilities2 = ata.SATACapabilities2;

            if(ata.SATAFeaturesSpecified)
                identifyDevice.SATAFeatures = ata.SATAFeatures;

            if(ata.SCTCommandTransportSpecified)
                identifyDevice.SCTCommandTransport = ata.SCTCommandTransport;

            if(ata.SectorsPerCardSpecified)
                identifyDevice.SectorsPerCard = ata.SectorsPerCard;

            if(ata.SecurityEraseTimeSpecified)
                identifyDevice.SecurityEraseTime = ata.SecurityEraseTime;

            if(ata.SecurityStatusSpecified)
                identifyDevice.SecurityStatus = ata.SecurityStatus;

            if(ata.ServiceBusyClearSpecified)
                identifyDevice.ServiceBusyClear = ata.ServiceBusyClear;

            if(ata.SpecificConfigurationSpecified)
                identifyDevice.SpecificConfiguration = ata.SpecificConfiguration;

            if(ata.StreamAccessLatencySpecified)
                identifyDevice.StreamAccessLatency = ata.StreamAccessLatency;

            if(ata.StreamMinReqSizeSpecified)
                identifyDevice.StreamMinReqSize = ata.StreamMinReqSize;

            if(ata.StreamPerformanceGranularitySpecified)
                identifyDevice.StreamPerformanceGranularity = ata.StreamPerformanceGranularity;

            if(ata.StreamTransferTimeDMASpecified)
                identifyDevice.StreamTransferTimeDMA = ata.StreamTransferTimeDMA;

            if(ata.StreamTransferTimePIOSpecified)
                identifyDevice.StreamTransferTimePIO = ata.StreamTransferTimePIO;

            if(ata.TransportMajorVersionSpecified)
                identifyDevice.TransportMajorVersion = ata.TransportMajorVersion;

            if(ata.TransportMinorVersionSpecified)
                identifyDevice.TransportMinorVersion = ata.TransportMinorVersion;

            if(ata.TrustedComputingSpecified)
                identifyDevice.TrustedComputing = ata.TrustedComputing;

            if(ata.UDMAActiveSpecified)
                identifyDevice.UDMAActive = ata.UDMAActive;

            if(ata.UDMASupportedSpecified)
                identifyDevice.UDMASupported = ata.UDMASupported;

            if(ata.WRVModeSpecified)
                identifyDevice.WRVMode = ata.WRVMode;

            if(ata.WRVSectorCountMode3Specified)
                identifyDevice.WRVSectorCountMode3 = ata.WRVSectorCountMode3;

            if(ata.WRVSectorCountMode2Specified)
                identifyDevice.WRVSectorCountMode2 = ata.WRVSectorCountMode2;

            Identify = Decoders.ATA.Identify.Encode(identifyDevice);
        }

        public Identify.IdentifyDevice? IdentifyDevice => Decoders.ATA.Identify.Decode(Identify);

        [JsonIgnore]
        public int Id { get;                                     set; }
        public         byte[]            Identify         { get; set; }
        public virtual TestedMedia       ReadCapabilities { get; set; }
        public virtual List<TestedMedia> RemovableMedias  { get; set; }
    }

    public class Chs
    {
        public Chs() { }

        public Chs(chsType chs)
        {
            Cylinders = chs.Cylinders;
            Heads     = chs.Heads;
            Sectors   = chs.Sectors;
        }

        [JsonIgnore]
        public int Id { get;           set; }
        public ushort Cylinders { get; set; }
        public ushort Heads     { get; set; }
        public ushort Sectors   { get; set; }
    }

    public class Scsi
    {
        public Scsi() { }

        public Scsi(scsiType scsi)
        {
            InquiryData          = scsi.Inquiry.Data;
            SupportsModeSense6   = scsi.SupportsModeSense6;
            SupportsModeSense10  = scsi.SupportsModeSense10;
            SupportsModeSubpages = scsi.SupportsModeSubpages;

            if(scsi.ReadCapabilitiesSpecified &&
               scsi.ReadCapabilities != null)
                ReadCapabilities = new TestedMedia(scsi.ReadCapabilities, false);

            if(scsi.RemovableMedias != null)
            {
                RemovableMedias = new List<TestedMedia>();

                foreach(testedMediaType scsiRemovableMedia in scsi.RemovableMedias)
                    RemovableMedias.Add(new TestedMedia(scsiRemovableMedia, false));
            }

            ModeSense6Data  = scsi.ModeSense6Data;
            ModeSense10Data = scsi.ModeSense10Data;

            if(scsi.EVPDPages != null)
            {
                EVPDPages = new List<ScsiPage>();

                foreach(pageType evpdPage in scsi.EVPDPages)
                    EVPDPages.Add(new ScsiPage(evpdPage));
            }

            if(scsi.ModeSense != null)
                ModeSense = new ScsiMode(scsi.ModeSense);

            if(scsi.MultiMediaDevice != null)
                MultiMediaDevice = new Mmc(scsi.MultiMediaDevice);

            if(scsi.SequentialDevice != null)
                SequentialDevice = new Ssc(scsi.SequentialDevice);

            if(InquiryData != null)
                return;

            var inq = new Inquiry.SCSIInquiry();

            if(scsi.Inquiry.ANSIVersionSpecified)
                inq.ANSIVersion = scsi.Inquiry.ANSIVersion;

            if(scsi.Inquiry.ECMAVersionSpecified)
                inq.ECMAVersion = scsi.Inquiry.ECMAVersion;

            if(scsi.Inquiry.DeviceTypeModifierSpecified)
                inq.DeviceTypeModifier = scsi.Inquiry.DeviceTypeModifier;

            if(scsi.Inquiry.ISOVersionSpecified)
                inq.ISOVersion = scsi.Inquiry.ISOVersion;

            if(scsi.Inquiry.ProductIdentificationSpecified)
            {
                byte[] tmp = Encoding.ASCII.GetBytes(scsi.Inquiry.ProductIdentification);
                inq.ProductIdentification = new byte[tmp.Length + 1];
                Array.Copy(tmp, 0, inq.ProductIdentification, 0, tmp.Length);
            }

            if(scsi.Inquiry.ProductRevisionLevelSpecified)
            {
                byte[] tmp = Encoding.ASCII.GetBytes(scsi.Inquiry.ProductRevisionLevel);
                inq.ProductRevisionLevel = new byte[tmp.Length + 1];
                Array.Copy(tmp, 0, inq.ProductRevisionLevel, 0, tmp.Length);
            }

            if(scsi.Inquiry.ResponseDataFormatSpecified)
                inq.ResponseDataFormat = scsi.Inquiry.ResponseDataFormat;

            if(scsi.Inquiry.VendorIdentificationSpecified)
            {
                byte[] tmp = Encoding.ASCII.GetBytes(scsi.Inquiry.VendorIdentification);
                inq.VendorIdentification = new byte[tmp.Length + 1];
                Array.Copy(tmp, 0, inq.VendorIdentification, 0, tmp.Length);
            }

            inq.ACC                  = scsi.Inquiry.AccessControlCoordinator;
            inq.ACKREQQ              = scsi.Inquiry.ACKRequests;
            inq.AERC                 = scsi.Inquiry.AERCSupported;
            inq.Addr16               = scsi.Inquiry.Address16;
            inq.Addr32               = scsi.Inquiry.Address32;
            inq.TPGS                 = (byte)scsi.Inquiry.AsymmetricalLUNAccess;
            inq.BQue                 = scsi.Inquiry.BasicQueueing;
            inq.EncServ              = scsi.Inquiry.EnclosureServices;
            inq.HiSup                = scsi.Inquiry.HierarchicalLUN;
            inq.IUS                  = scsi.Inquiry.IUS;
            inq.Linked               = scsi.Inquiry.LinkedCommands;
            inq.MChngr               = scsi.Inquiry.MediumChanger;
            inq.MultiP               = scsi.Inquiry.MultiPortDevice;
            inq.NormACA              = scsi.Inquiry.NormalACA;
            inq.PeripheralDeviceType = (byte)scsi.Inquiry.PeripheralDeviceType;
            inq.PeripheralQualifier  = (byte)scsi.Inquiry.PeripheralQualifier;
            inq.Protect              = scsi.Inquiry.Protection;
            inq.QAS                  = scsi.Inquiry.QAS;
            inq.RelAddr              = scsi.Inquiry.RelativeAddressing;
            inq.RMB                  = scsi.Inquiry.Removable;
            inq.CmdQue               = scsi.Inquiry.TaggedCommandQueue;
            inq.TrmTsk               = scsi.Inquiry.TerminateTaskSupported;
            inq.ThreePC              = scsi.Inquiry.ThirdPartyCopy;
            inq.TranDis              = scsi.Inquiry.TranferDisable;
            inq.SftRe                = scsi.Inquiry.SoftReset;
            inq.Clocking             = (byte)scsi.Inquiry.SPIClocking;
            inq.SCCS                 = scsi.Inquiry.StorageArrayController;
            inq.Sync                 = scsi.Inquiry.SyncTransfer;
            inq.VersionDescriptors   = scsi.Inquiry.VersionDescriptors;
            inq.WBus16               = scsi.Inquiry.WideBus16;
            inq.WBus32               = scsi.Inquiry.WideBus32;

            InquiryData = Decoders.SCSI.Inquiry.Encode(inq);
        }

        public Inquiry.SCSIInquiry? Inquiry => Decoders.SCSI.Inquiry.Decode(InquiryData);

        [JsonIgnore]
        public int Id { get;                                              set; }
        public         byte[]            InquiryData               { get; set; }
        public virtual List<ScsiPage>    EVPDPages                 { get; set; }
        public         bool              SupportsModeSense6        { get; set; }
        public         bool              SupportsModeSense10       { get; set; }
        public         bool              SupportsModeSubpages      { get; set; }
        public virtual ScsiMode          ModeSense                 { get; set; }
        public virtual Mmc               MultiMediaDevice          { get; set; }
        public virtual TestedMedia       ReadCapabilities          { get; set; }
        public virtual List<TestedMedia> RemovableMedias           { get; set; }
        public virtual Ssc               SequentialDevice          { get; set; }
        public         byte[]            ModeSense6Data            { get; set; }
        public         byte[]            ModeSense10Data           { get; set; }
        public         byte[]            ModeSense6CurrentData     { get; set; }
        public         byte[]            ModeSense10CurrentData    { get; set; }
        public         byte[]            ModeSense6ChangeableData  { get; set; }
        public         byte[]            ModeSense10ChangeableData { get; set; }
    }

    public class ScsiMode
    {
        public ScsiMode() { }

        public ScsiMode(modeType mode)
        {
            if(mode.MediumTypeSpecified)
                MediumType = mode.MediumType;

            WriteProtected = mode.WriteProtected;

            if(mode.SpeedSpecified)
                Speed = mode.Speed;

            if(mode.BufferedModeSpecified)
                BufferedMode = mode.BufferedMode;

            BlankCheckEnabled = mode.BlankCheckEnabled;
            DPOandFUA         = mode.DPOandFUA;

            if(mode.ModePages != null)
            {
                ModePages = new List<ScsiPage>();

                foreach(modePageType modePage in mode.ModePages)
                    ModePages.Add(new ScsiPage(modePage));
            }

            if(mode.BlockDescriptors == null)
                return;

            BlockDescriptors = new List<BlockDescriptor>();

            foreach(blockDescriptorType blockDescriptor in mode.BlockDescriptors)
                BlockDescriptors.Add(new BlockDescriptor(blockDescriptor));
        }

        [JsonIgnore]
        public int Id { get;                                          set; }
        public         byte?                 MediumType        { get; set; }
        public         bool                  WriteProtected    { get; set; }
        public virtual List<BlockDescriptor> BlockDescriptors  { get; set; }
        public         byte?                 Speed             { get; set; }
        public         byte?                 BufferedMode      { get; set; }
        public         bool                  BlankCheckEnabled { get; set; }
        public         bool                  DPOandFUA         { get; set; }
        public virtual List<ScsiPage>        ModePages         { get; set; }
    }

    public class BlockDescriptor
    {
        public BlockDescriptor() { }

        public BlockDescriptor(blockDescriptorType descriptor)
        {
            Density = descriptor.Density;

            if(descriptor.BlocksSpecified)
                Blocks = descriptor.Blocks;

            if(descriptor.BlockLengthSpecified)
                BlockLength = descriptor.BlockLength;
        }

        [JsonIgnore]
        public int Id { get;             set; }
        public byte   Density     { get; set; }
        public ulong? Blocks      { get; set; }
        public uint?  BlockLength { get; set; }
    }

    public class ScsiPage
    {
        public ScsiPage() { }

        public ScsiPage(pageType evpdPage)
        {
            page  = evpdPage.page;
            value = evpdPage.value;
        }

        public ScsiPage(modePageType modePage)
        {
            page    = modePage.page;
            subpage = modePage.subpage;
            value   = modePage.value;
        }

        [JsonIgnore]
        public int Id { get;         set; }
        public byte   page    { get; set; }
        public byte?  subpage { get; set; }
        public byte[] value   { get; set; }
    }

    public class Mmc
    {
        public Mmc() { }

        public Mmc(mmcType mmc)
        {
            if(mmc.ModeSense2A != null)
                ModeSense2AData = Modes.EncodeModePage_2A(new Modes.ModePage_2A
                {
                    AccurateCDDA                     = mmc.ModeSense2A.AccurateCDDA, BCK = mmc.ModeSense2A.BCK,
                    BufferSize                       = mmc.ModeSense2A.BufferSize,
                    BUF                              = mmc.ModeSense2A.BufferUnderRunProtection,
                    Eject                            = mmc.ModeSense2A.CanEject,
                    Lock                             = mmc.ModeSense2A.CanLockMedia,
                    CDDACommand                      = mmc.ModeSense2A.CDDACommand,
                    Composite                        = mmc.ModeSense2A.CompositeAudioVideo,
                    CMRSupported                     = (ushort)(mmc.ModeSense2A.CSSandCPPMSupported ? 1 : 0),
                    CurrentSpeed                     = mmc.ModeSense2A.CurrentSpeed,
                    CurrentWriteSpeed                = mmc.ModeSense2A.CurrentWriteSpeed,
                    CurrentWriteSpeedSelected        = mmc.ModeSense2A.CurrentWriteSpeedSelected,
                    SDP                              = mmc.ModeSense2A.DeterministicSlotChanger,
                    DigitalPort1                     = mmc.ModeSense2A.DigitalPort1,
                    DigitalPort2                     = mmc.ModeSense2A.DigitalPort2,
                    LeadInPW                         = mmc.ModeSense2A.LeadInPW,
                    LoadingMechanism                 = mmc.ModeSense2A.LoadingMechanismType,
                    LockState                        = mmc.ModeSense2A.LockStatus,
                    LSBF                             = mmc.ModeSense2A.LSBF,
                    MaximumSpeed                     = mmc.ModeSense2A.MaximumSpeed,
                    MaxWriteSpeed                    = mmc.ModeSense2A.MaximumWriteSpeed,
                    AudioPlay                        = mmc.ModeSense2A.PlaysAudio,
                    PreventJumper                    = mmc.ModeSense2A.PreventJumperStatus,
                    RCK                              = mmc.ModeSense2A.RCK,
                    ReadBarcode                      = mmc.ModeSense2A.ReadsBarcode,
                    SCC                              = mmc.ModeSense2A.ReadsBothSides,
                    ReadCDR                          = mmc.ModeSense2A.ReadsCDR,
                    ReadCDRW                         = mmc.ModeSense2A.ReadsCDRW,
                    DeinterlaveSubchannel            = mmc.ModeSense2A.ReadsDeinterlavedSubchannel,
                    ReadDVDR                         = mmc.ModeSense2A.ReadsDVDR,
                    ReadDVDRAM                       = mmc.ModeSense2A.ReadsDVDRAM,
                    ReadDVDROM                       = mmc.ModeSense2A.ReadsDVDROM,
                    ISRC                             = mmc.ModeSense2A.ReadsISRC,
                    Mode2Form2                       = mmc.ModeSense2A.ReadsMode2Form2,
                    Mode2Form1                       = mmc.ModeSense2A.ReadsMode2Form1,
                    Method2                          = mmc.ModeSense2A.ReadsPacketCDR,
                    Subchannel                       = mmc.ModeSense2A.ReadsSubchannel,
                    UPC                              = mmc.ModeSense2A.ReadsUPC,
                    C2Pointer                        = mmc.ModeSense2A.ReturnsC2Pointers,
                    RotationControlSelected          = mmc.ModeSense2A.RotationControlSelected,
                    SeparateChannelMute              = mmc.ModeSense2A.SeparateChannelMute,
                    SeparateChannelVolume            = mmc.ModeSense2A.SeparateChannelVolume, SSS = mmc.ModeSense2A.SSS,
                    MultiSession                     = mmc.ModeSense2A.SupportsMultiSession,
                    SupportedVolumeLevels            = mmc.ModeSense2A.SupportedVolumeLevels,
                    TestWrite                        = mmc.ModeSense2A.TestWrite,
                    WriteCDR                         = mmc.ModeSense2A.WritesCDR,
                    WriteCDRW                        = mmc.ModeSense2A.WritesCDRW,
                    WriteDVDR                        = mmc.ModeSense2A.WritesDVDR,
                    WriteDVDRAM                      = mmc.ModeSense2A.WritesDVDRAM,
                    WriteSpeedPerformanceDescriptors = mmc.ModeSense2A.WriteSpeedPerformanceDescriptors
                });

            if(mmc.Features != null)
                Features = new MmcFeatures(mmc.Features);

            if(mmc.TestedMedia == null)
                return;

            TestedMedia = new List<TestedMedia>();

            foreach(testedMediaType mediaType in mmc.TestedMedia)
                TestedMedia.Add(new TestedMedia(mediaType, false));
        }

        [JsonIgnore]
        public int Id { get; set; }
        public virtual Modes.ModePage_2A ModeSense2A     => Modes.DecodeModePage_2A(ModeSense2AData);
        public virtual MmcFeatures       Features        { get; set; }
        public virtual List<TestedMedia> TestedMedia     { get; set; }
        public         byte[]            ModeSense2AData { get; set; }
    }

    public class MmcFeatures
    {
        public MmcFeatures() { }

        public MmcFeatures(mmcFeaturesType features)
        {
            if(features.PhysicalInterfaceStandardSpecified &&
               !features.PhysicalInterfaceStandardNumberSpecified)
                PhysicalInterfaceStandardNumber = (uint?)features.PhysicalInterfaceStandard;

            if(features.PhysicalInterfaceStandardNumberSpecified)
                PhysicalInterfaceStandardNumber = features.PhysicalInterfaceStandardNumber;

            if(features.AACSVersionSpecified)
                AACSVersion = features.AACSVersion;

            if(features.AGIDsSpecified)
                AGIDs = features.AGIDs;

            if(features.BindingNonceBlocksSpecified)
                BindingNonceBlocks = features.BindingNonceBlocks;

            if(features.CPRMVersionSpecified)
                CPRMVersion = features.CPRMVersion;

            if(features.CSSVersionSpecified)
                CSSVersion = features.CSSVersion;

            if(features.LoadingMechanismTypeSpecified)
                LoadingMechanismType = features.LoadingMechanismType;

            if(features.LogicalBlockSizeSpecified)
                LogicalBlockSize = features.LogicalBlockSize;

            if(features.BlocksPerReadableUnitSpecified)
                BlocksPerReadableUnit = features.BlocksPerReadableUnit;

            if(features.FirmwareDateSpecified)
                FirmwareDate = features.FirmwareDate;

            if(features.VolumeLevelsSpecified)
                VolumeLevels = features.VolumeLevels;

            BufferUnderrunFreeInDVD       = features.BufferUnderrunFreeInDVD;
            BufferUnderrunFreeInSAO       = features.BufferUnderrunFreeInSAO;
            BufferUnderrunFreeInTAO       = features.BufferUnderrunFreeInTAO;
            CanAudioScan                  = features.CanAudioScan;
            CanEject                      = features.CanEject;
            CanEraseSector                = features.CanEraseSector;
            CanExpandBDRESpareArea        = features.CanExpandBDRESpareArea;
            CanFormat                     = features.CanFormat;
            CanFormatBDREWithoutSpare     = features.CanFormatBDREWithoutSpare;
            CanFormatCert                 = features.CanFormatCert;
            CanFormatFRF                  = features.CanFormatFRF;
            CanFormatQCert                = features.CanFormatQCert;
            CanFormatRRM                  = features.CanFormatRRM;
            CanGenerateBindingNonce       = features.CanGenerateBindingNonce;
            CanLoad                       = features.CanLoad;
            CanMuteSeparateChannels       = features.CanMuteSeparateChannels;
            CanOverwriteSAOTrack          = features.CanOverwriteSAOTrack;
            CanOverwriteTAOTrack          = features.CanOverwriteTAOTrack;
            CanPlayCDAudio                = features.CanPlayCDAudio;
            CanPseudoOverwriteBDR         = features.CanPseudoOverwriteBDR;
            CanReadAllDualR               = features.CanReadAllDualR;
            CanReadAllDualRW              = features.CanReadAllDualRW;
            CanReadBD                     = features.CanReadBD;
            CanReadBDR                    = features.CanReadBDR;
            CanReadBDRE1                  = features.CanReadBDRE1;
            CanReadBDRE2                  = features.CanReadBDRE2;
            CanReadBDROM                  = features.CanReadBDROM;
            CanReadBluBCA                 = features.CanReadBluBCA;
            CanReadCD                     = features.CanReadCD;
            CanReadCDMRW                  = features.CanReadCDMRW;
            CanReadCPRM_MKB               = features.CanReadCPRM_MKB;
            CanReadDDCD                   = features.CanReadDDCD;
            CanReadDVD                    = features.CanReadDVD;
            CanReadDVDPlusMRW             = features.CanReadDVDPlusMRW;
            CanReadDVDPlusR               = features.CanReadDVDPlusR;
            CanReadDVDPlusRDL             = features.CanReadDVDPlusRDL;
            CanReadDVDPlusRW              = features.CanReadDVDPlusRW;
            CanReadDVDPlusRWDL            = features.CanReadDVDPlusRWDL;
            CanReadDriveAACSCertificate   = features.CanReadDriveAACSCertificate;
            CanReadHDDVD                  = features.CanReadHDDVD;
            CanReadHDDVDR                 = features.CanReadHDDVDR;
            CanReadHDDVDRAM               = features.CanReadHDDVDRAM;
            CanReadLeadInCDText           = features.CanReadLeadInCDText;
            CanReadOldBDR                 = features.CanReadOldBDR;
            CanReadOldBDRE                = features.CanReadOldBDRE;
            CanReadOldBDROM               = features.CanReadOldBDROM;
            CanReadSpareAreaInformation   = features.CanReadSpareAreaInformation;
            CanReportDriveSerial          = features.CanReportDriveSerial;
            CanReportMediaSerial          = features.CanReportMediaSerial;
            CanTestWriteDDCDR             = features.CanTestWriteDDCDR;
            CanTestWriteDVD               = features.CanTestWriteDVD;
            CanTestWriteInSAO             = features.CanTestWriteInSAO;
            CanTestWriteInTAO             = features.CanTestWriteInTAO;
            CanUpgradeFirmware            = features.CanUpgradeFirmware;
            CanWriteBD                    = features.CanWriteBD;
            CanWriteBDR                   = features.CanWriteBDR;
            CanWriteBDRE1                 = features.CanWriteBDRE1;
            CanWriteBDRE2                 = features.CanWriteBDRE2;
            CanWriteBusEncryptedBlocks    = features.CanWriteBusEncryptedBlocks;
            CanWriteCDMRW                 = features.CanWriteCDMRW;
            CanWriteCDRW                  = features.CanWriteCDRW;
            CanWriteCDRWCAV               = features.CanWriteCDRWCAV;
            CanWriteCDSAO                 = features.CanWriteCDSAO;
            CanWriteCDTAO                 = features.CanWriteCDTAO;
            CanWriteCSSManagedDVD         = features.CanWriteCSSManagedDVD;
            CanWriteDDCDR                 = features.CanWriteDDCDR;
            CanWriteDDCDRW                = features.CanWriteDDCDRW;
            CanWriteDVDPlusMRW            = features.CanWriteDVDPlusMRW;
            CanWriteDVDPlusR              = features.CanWriteDVDPlusR;
            CanWriteDVDPlusRDL            = features.CanWriteDVDPlusRDL;
            CanWriteDVDPlusRW             = features.CanWriteDVDPlusRW;
            CanWriteDVDPlusRWDL           = features.CanWriteDVDPlusRWDL;
            CanWriteDVDR                  = features.CanWriteDVDR;
            CanWriteDVDRDL                = features.CanWriteDVDRDL;
            CanWriteDVDRW                 = features.CanWriteDVDRW;
            CanWriteHDDVDR                = features.CanWriteHDDVDR;
            CanWriteHDDVDRAM              = features.CanWriteHDDVDRAM;
            CanWriteOldBDR                = features.CanWriteOldBDR;
            CanWriteOldBDRE               = features.CanWriteOldBDRE;
            CanWritePackedSubchannelInTAO = features.CanWritePackedSubchannelInTAO;
            CanWriteRWSubchannelInSAO     = features.CanWriteRWSubchannelInSAO;
            CanWriteRWSubchannelInTAO     = features.CanWriteRWSubchannelInTAO;
            CanWriteRaw                   = features.CanWriteRaw;
            CanWriteRawMultiSession       = features.CanWriteRawMultiSession;
            CanWriteRawSubchannelInTAO    = features.CanWriteRawSubchannelInTAO;
            ChangerIsSideChangeCapable    = features.ChangerIsSideChangeCapable;
            ChangerSlots                  = features.ChangerSlots;
            ChangerSupportsDiscPresent    = features.ChangerSupportsDiscPresent;
            DBML                          = features.DBML;
            DVDMultiRead                  = features.DVDMultiRead;
            EmbeddedChanger               = features.EmbeddedChanger;
            ErrorRecoveryPage             = features.ErrorRecoveryPage;
            Locked                        = features.Locked;
            MultiRead                     = features.MultiRead;
            PreventJumper                 = features.PreventJumper;
            SupportsAACS                  = features.SupportsAACS;
            SupportsBusEncryption         = features.SupportsBusEncryption;
            SupportsC2                    = features.SupportsC2;
            SupportsCPRM                  = features.SupportsCPRM;
            SupportsCSS                   = features.SupportsCSS;
            SupportsDAP                   = features.SupportsDAP;
            SupportsDeviceBusyEvent       = features.SupportsDeviceBusyEvent;
            SupportsHybridDiscs           = features.SupportsHybridDiscs;
            SupportsModePage1Ch           = features.SupportsModePage1Ch;
            SupportsOSSC                  = features.SupportsOSSC;
            SupportsPWP                   = features.SupportsPWP;
            SupportsSWPP                  = features.SupportsSWPP;
            SupportsSecurDisc             = features.SupportsSecurDisc;
            SupportsSeparateVolume        = features.SupportsSeparateVolume;
            SupportsVCPS                  = features.SupportsVCPS;
            SupportsWriteInhibitDCB       = features.SupportsWriteInhibitDCB;
            SupportsWriteProtectPAC       = features.SupportsWriteProtectPAC;
        }

        [JsonIgnore]
        public int Id { get;                                  set; }
        public byte?     AACSVersion                   { get; set; }
        public byte?     AGIDs                         { get; set; }
        public byte?     BindingNonceBlocks            { get; set; }
        public ushort?   BlocksPerReadableUnit         { get; set; }
        public bool      BufferUnderrunFreeInDVD       { get; set; }
        public bool      BufferUnderrunFreeInSAO       { get; set; }
        public bool      BufferUnderrunFreeInTAO       { get; set; }
        public bool      CanAudioScan                  { get; set; }
        public bool      CanEject                      { get; set; }
        public bool      CanEraseSector                { get; set; }
        public bool      CanExpandBDRESpareArea        { get; set; }
        public bool      CanFormat                     { get; set; }
        public bool      CanFormatBDREWithoutSpare     { get; set; }
        public bool      CanFormatCert                 { get; set; }
        public bool      CanFormatFRF                  { get; set; }
        public bool      CanFormatQCert                { get; set; }
        public bool      CanFormatRRM                  { get; set; }
        public bool      CanGenerateBindingNonce       { get; set; }
        public bool      CanLoad                       { get; set; }
        public bool      CanMuteSeparateChannels       { get; set; }
        public bool      CanOverwriteSAOTrack          { get; set; }
        public bool      CanOverwriteTAOTrack          { get; set; }
        public bool      CanPlayCDAudio                { get; set; }
        public bool      CanPseudoOverwriteBDR         { get; set; }
        public bool      CanReadAllDualR               { get; set; }
        public bool      CanReadAllDualRW              { get; set; }
        public bool      CanReadBD                     { get; set; }
        public bool      CanReadBDR                    { get; set; }
        public bool      CanReadBDRE1                  { get; set; }
        public bool      CanReadBDRE2                  { get; set; }
        public bool      CanReadBDROM                  { get; set; }
        public bool      CanReadBluBCA                 { get; set; }
        public bool      CanReadCD                     { get; set; }
        public bool      CanReadCDMRW                  { get; set; }
        public bool      CanReadCPRM_MKB               { get; set; }
        public bool      CanReadDDCD                   { get; set; }
        public bool      CanReadDVD                    { get; set; }
        public bool      CanReadDVDPlusMRW             { get; set; }
        public bool      CanReadDVDPlusR               { get; set; }
        public bool      CanReadDVDPlusRDL             { get; set; }
        public bool      CanReadDVDPlusRW              { get; set; }
        public bool      CanReadDVDPlusRWDL            { get; set; }
        public bool      CanReadDriveAACSCertificate   { get; set; }
        public bool      CanReadHDDVD                  { get; set; }
        public bool      CanReadHDDVDR                 { get; set; }
        public bool      CanReadHDDVDRAM               { get; set; }
        public bool      CanReadLeadInCDText           { get; set; }
        public bool      CanReadOldBDR                 { get; set; }
        public bool      CanReadOldBDRE                { get; set; }
        public bool      CanReadOldBDROM               { get; set; }
        public bool      CanReadSpareAreaInformation   { get; set; }
        public bool      CanReportDriveSerial          { get; set; }
        public bool      CanReportMediaSerial          { get; set; }
        public bool      CanTestWriteDDCDR             { get; set; }
        public bool      CanTestWriteDVD               { get; set; }
        public bool      CanTestWriteInSAO             { get; set; }
        public bool      CanTestWriteInTAO             { get; set; }
        public bool      CanUpgradeFirmware            { get; set; }
        public bool      CanWriteBD                    { get; set; }
        public bool      CanWriteBDR                   { get; set; }
        public bool      CanWriteBDRE1                 { get; set; }
        public bool      CanWriteBDRE2                 { get; set; }
        public bool      CanWriteBusEncryptedBlocks    { get; set; }
        public bool      CanWriteCDMRW                 { get; set; }
        public bool      CanWriteCDRW                  { get; set; }
        public bool      CanWriteCDRWCAV               { get; set; }
        public bool      CanWriteCDSAO                 { get; set; }
        public bool      CanWriteCDTAO                 { get; set; }
        public bool      CanWriteCSSManagedDVD         { get; set; }
        public bool      CanWriteDDCDR                 { get; set; }
        public bool      CanWriteDDCDRW                { get; set; }
        public bool      CanWriteDVDPlusMRW            { get; set; }
        public bool      CanWriteDVDPlusR              { get; set; }
        public bool      CanWriteDVDPlusRDL            { get; set; }
        public bool      CanWriteDVDPlusRW             { get; set; }
        public bool      CanWriteDVDPlusRWDL           { get; set; }
        public bool      CanWriteDVDR                  { get; set; }
        public bool      CanWriteDVDRDL                { get; set; }
        public bool      CanWriteDVDRW                 { get; set; }
        public bool      CanWriteHDDVDR                { get; set; }
        public bool      CanWriteHDDVDRAM              { get; set; }
        public bool      CanWriteOldBDR                { get; set; }
        public bool      CanWriteOldBDRE               { get; set; }
        public bool      CanWritePackedSubchannelInTAO { get; set; }
        public bool      CanWriteRWSubchannelInSAO     { get; set; }
        public bool      CanWriteRWSubchannelInTAO     { get; set; }
        public bool      CanWriteRaw                   { get; set; }
        public bool      CanWriteRawMultiSession       { get; set; }
        public bool      CanWriteRawSubchannelInTAO    { get; set; }
        public bool      ChangerIsSideChangeCapable    { get; set; }
        public byte      ChangerSlots                  { get; set; }
        public bool      ChangerSupportsDiscPresent    { get; set; }
        public byte?     CPRMVersion                   { get; set; }
        public byte?     CSSVersion                    { get; set; }
        public bool      DBML                          { get; set; }
        public bool      DVDMultiRead                  { get; set; }
        public bool      EmbeddedChanger               { get; set; }
        public bool      ErrorRecoveryPage             { get; set; }
        public DateTime? FirmwareDate                  { get; set; }
        public byte?     LoadingMechanismType          { get; set; }
        public bool      Locked                        { get; set; }
        public uint?     LogicalBlockSize              { get; set; }
        public bool      MultiRead                     { get; set; }
        public PhysicalInterfaces? PhysicalInterfaceStandard =>
            (PhysicalInterfaces?)PhysicalInterfaceStandardNumber;
        public uint?   PhysicalInterfaceStandardNumber { get; set; }
        public bool    PreventJumper                   { get; set; }
        public bool    SupportsAACS                    { get; set; }
        public bool    SupportsBusEncryption           { get; set; }
        public bool    SupportsC2                      { get; set; }
        public bool    SupportsCPRM                    { get; set; }
        public bool    SupportsCSS                     { get; set; }
        public bool    SupportsDAP                     { get; set; }
        public bool    SupportsDeviceBusyEvent         { get; set; }
        public bool    SupportsHybridDiscs             { get; set; }
        public bool    SupportsModePage1Ch             { get; set; }
        public bool    SupportsOSSC                    { get; set; }
        public bool    SupportsPWP                     { get; set; }
        public bool    SupportsSWPP                    { get; set; }
        public bool    SupportsSecurDisc               { get; set; }
        public bool    SupportsSeparateVolume          { get; set; }
        public bool    SupportsVCPS                    { get; set; }
        public bool    SupportsWriteInhibitDCB         { get; set; }
        public bool    SupportsWriteProtectPAC         { get; set; }
        public ushort? VolumeLevels                    { get; set; }
        public byte[]  BinaryData                      { get; set; }
    }

    public class TestedMedia
    {
        public Identify.IdentifyDevice? IdentifyDevice;

        public TestedMedia() { }

        public TestedMedia(testedMediaType mediaType, bool ata)
        {
            if(mediaType.BlocksSpecified)
                Blocks = mediaType.Blocks;

            if(mediaType.BlockSizeSpecified)
                BlockSize = mediaType.BlockSize;

            if(mediaType.CanReadAACSSpecified)
                CanReadAACS = mediaType.CanReadAACS;

            if(mediaType.CanReadADIPSpecified)
                CanReadADIP = mediaType.CanReadADIP;

            if(mediaType.CanReadATIPSpecified)
                CanReadATIP = mediaType.CanReadATIP;

            if(mediaType.CanReadBCASpecified)
                CanReadBCA = mediaType.CanReadBCA;

            if(mediaType.CanReadC2PointersSpecified)
                CanReadC2Pointers = mediaType.CanReadC2Pointers;

            if(mediaType.CanReadCMISpecified)
                CanReadCMI = mediaType.CanReadCMI;

            if(mediaType.CanReadCorrectedSubchannelSpecified)
                CanReadCorrectedSubchannel = mediaType.CanReadCorrectedSubchannel;

            if(mediaType.CanReadCorrectedSubchannelWithC2Specified)
                CanReadCorrectedSubchannelWithC2 = mediaType.CanReadCorrectedSubchannelWithC2;

            if(mediaType.CanReadDCBSpecified)
                CanReadDCB = mediaType.CanReadDCB;

            if(mediaType.CanReadDDSSpecified)
                CanReadDDS = mediaType.CanReadDDS;

            if(mediaType.CanReadDMISpecified)
                CanReadDMI = mediaType.CanReadDMI;

            if(mediaType.CanReadDiscInformationSpecified)
                CanReadDiscInformation = mediaType.CanReadDiscInformation;

            if(mediaType.CanReadFullTOCSpecified)
                CanReadFullTOC = mediaType.CanReadFullTOC;

            if(mediaType.CanReadHDCMISpecified)
                CanReadHDCMI = mediaType.CanReadHDCMI;

            if(mediaType.CanReadLayerCapacitySpecified)
                CanReadLayerCapacity = mediaType.CanReadLayerCapacity;

            if(mediaType.CanReadLeadInSpecified)
                CanReadFirstTrackPreGap = mediaType.CanReadLeadIn;

            if(mediaType.CanReadLeadOutSpecified)
                CanReadLeadOut = mediaType.CanReadLeadOut;

            if(mediaType.CanReadMediaIDSpecified)
                CanReadMediaID = mediaType.CanReadMediaID;

            if(mediaType.CanReadMediaSerialSpecified)
                CanReadMediaSerial = mediaType.CanReadMediaSerial;

            if(mediaType.CanReadPACSpecified)
                CanReadPAC = mediaType.CanReadPAC;

            if(mediaType.CanReadPFISpecified)
                CanReadPFI = mediaType.CanReadPFI;

            if(mediaType.CanReadPMASpecified)
                CanReadPMA = mediaType.CanReadPMA;

            if(mediaType.CanReadPQSubchannelSpecified)
                CanReadPQSubchannel = mediaType.CanReadPQSubchannel;

            if(mediaType.CanReadPQSubchannelWithC2Specified)
                CanReadPQSubchannelWithC2 = mediaType.CanReadPQSubchannelWithC2;

            if(mediaType.CanReadPRISpecified)
                CanReadPRI = mediaType.CanReadPRI;

            if(mediaType.CanReadRWSubchannelSpecified)
                CanReadRWSubchannel = mediaType.CanReadRWSubchannel;

            if(mediaType.CanReadRWSubchannelWithC2Specified)
                CanReadRWSubchannelWithC2 = mediaType.CanReadRWSubchannelWithC2;

            if(mediaType.CanReadRecordablePFISpecified)
                CanReadRecordablePFI = mediaType.CanReadRecordablePFI;

            if(mediaType.CanReadSpareAreaInformationSpecified)
                CanReadSpareAreaInformation = mediaType.CanReadSpareAreaInformation;

            if(mediaType.CanReadTOCSpecified)
                CanReadTOC = mediaType.CanReadTOC;

            if(mediaType.DensitySpecified)
                Density = mediaType.Density;

            if(mediaType.LongBlockSizeSpecified)
                LongBlockSize = mediaType.LongBlockSize;

            if(mediaType.ManufacturerSpecified)
                Manufacturer = mediaType.Manufacturer;

            if(mediaType.MediumTypeSpecified)
                MediumType = mediaType.MediumType;

            if(mediaType.ModelSpecified)
                Model = mediaType.Model;

            if(mediaType.SupportsHLDTSTReadRawDVDSpecified)
                SupportsHLDTSTReadRawDVD = mediaType.SupportsHLDTSTReadRawDVD;

            if(mediaType.SupportsNECReadCDDASpecified)
                SupportsNECReadCDDA = mediaType.SupportsNECReadCDDA;

            if(mediaType.SupportsPioneerReadCDDASpecified)
                SupportsPioneerReadCDDA = mediaType.SupportsPioneerReadCDDA;

            if(mediaType.SupportsPioneerReadCDDAMSFSpecified)
                SupportsPioneerReadCDDAMSF = mediaType.SupportsPioneerReadCDDAMSF;

            if(mediaType.SupportsPlextorReadCDDASpecified)
                SupportsPlextorReadCDDA = mediaType.SupportsPlextorReadCDDA;

            if(mediaType.SupportsPlextorReadRawDVDSpecified)
                SupportsPlextorReadRawDVD = mediaType.SupportsPlextorReadRawDVD;

            if(mediaType.SupportsRead10Specified)
                SupportsRead10 = mediaType.SupportsRead10;

            if(mediaType.SupportsRead12Specified)
                SupportsRead12 = mediaType.SupportsRead12;

            if(mediaType.SupportsRead16Specified)
                SupportsRead16 = mediaType.SupportsRead16;

            if(mediaType.SupportsReadSpecified)
            {
                if(ata)
                    SupportsReadSectors = mediaType.SupportsRead;
                else
                    SupportsRead6 = mediaType.SupportsRead;
            }

            if(mediaType.SupportsReadCapacity16Specified)
                SupportsReadCapacity16 = mediaType.SupportsReadCapacity16;

            if(mediaType.SupportsReadCapacitySpecified)
                SupportsReadCapacity = mediaType.SupportsReadCapacity;

            if(mediaType.SupportsReadCdSpecified)
                SupportsReadCd = mediaType.SupportsReadCd;

            if(mediaType.SupportsReadCdMsfSpecified)
                SupportsReadCdMsf = mediaType.SupportsReadCdMsf;

            if(mediaType.SupportsReadCdRawSpecified)
                SupportsReadCdRaw = mediaType.SupportsReadCdRaw;

            if(mediaType.SupportsReadCdMsfRawSpecified)
                SupportsReadCdMsfRaw = mediaType.SupportsReadCdMsfRaw;

            if(mediaType.SupportsReadLong16Specified)
                SupportsReadLong16 = mediaType.SupportsReadLong16;

            if(mediaType.SupportsReadLongSpecified)
                SupportsReadLong = mediaType.SupportsReadLong;

            if(mediaType.LBASectorsSpecified)
                LBASectors = mediaType.LBASectors;

            if(mediaType.LBA48SectorsSpecified)
                LBA48Sectors = mediaType.LBA48Sectors;

            if(mediaType.LogicalAlignmentSpecified)
                LogicalAlignment = mediaType.LogicalAlignment;

            if(mediaType.NominalRotationRateSpecified)
                NominalRotationRate = mediaType.NominalRotationRate;

            if(mediaType.PhysicalBlockSizeSpecified)
                PhysicalBlockSize = mediaType.PhysicalBlockSize;

            if(mediaType.SolidStateDeviceSpecified)
                SolidStateDevice = mediaType.SolidStateDevice;

            if(mediaType.UnformattedBPTSpecified)
                UnformattedBPT = mediaType.UnformattedBPT;

            if(mediaType.UnformattedBPSSpecified)
                UnformattedBPS = mediaType.UnformattedBPS;

            if(mediaType.SupportsReadDmaLbaSpecified)
                SupportsReadDmaLba = mediaType.SupportsReadDmaLba;

            if(mediaType.SupportsReadDmaRetryLbaSpecified)
                SupportsReadDmaRetryLba = mediaType.SupportsReadDmaRetryLba;

            if(mediaType.SupportsReadLbaSpecified)
                SupportsReadLba = mediaType.SupportsReadLba;

            if(mediaType.SupportsReadRetryLbaSpecified)
                SupportsReadRetryLba = mediaType.SupportsReadRetryLba;

            if(mediaType.SupportsReadLongLbaSpecified)
                SupportsReadLongLba = mediaType.SupportsReadLongLba;

            if(mediaType.SupportsReadLongRetryLbaSpecified)
                SupportsReadLongRetryLba = mediaType.SupportsReadLongRetryLba;

            if(mediaType.SupportsSeekLbaSpecified)
                SupportsSeekLba = mediaType.SupportsSeekLba;

            if(mediaType.SupportsReadDmaLba48Specified)
                SupportsReadDmaLba48 = mediaType.SupportsReadDmaLba48;

            if(mediaType.SupportsReadLba48Specified)
                SupportsReadLba48 = mediaType.SupportsReadLba48;

            if(mediaType.SupportsReadDmaSpecified)
                SupportsReadDma = mediaType.SupportsReadDma;

            if(mediaType.SupportsReadDmaRetrySpecified)
                SupportsReadDmaRetry = mediaType.SupportsReadDmaRetry;

            if(mediaType.SupportsReadRetrySpecified)
                SupportsReadRetry = mediaType.SupportsReadRetry;

            if(mediaType.SupportsReadLongRetrySpecified)
                SupportsReadLongRetry = mediaType.SupportsReadLongRetry;

            if(mediaType.SupportsSeekSpecified)
                SupportsSeek = mediaType.SupportsSeek;

            if(mediaType.CHS != null)
                CHS = new Chs(mediaType.CHS);

            if(mediaType.CurrentCHS != null)
                CurrentCHS = new Chs(mediaType.CurrentCHS);

            MediaIsRecognized = mediaType.MediaIsRecognized;
            MediumTypeName    = mediaType.MediumTypeName;
            ModeSense6Data    = mediaType.ModeSense6Data;
            ModeSense10Data   = mediaType.ModeSense10Data;
        }

        [JsonIgnore]
        public int Id { get;                                  set; }
        public byte[] IdentifyData                     { get; set; }
        public ulong? Blocks                           { get; set; }
        public uint?  BlockSize                        { get; set; }
        public bool?  CanReadAACS                      { get; set; }
        public bool?  CanReadADIP                      { get; set; }
        public bool?  CanReadATIP                      { get; set; }
        public bool?  CanReadBCA                       { get; set; }
        public bool?  CanReadC2Pointers                { get; set; }
        public bool?  CanReadCMI                       { get; set; }
        public bool?  CanReadCorrectedSubchannel       { get; set; }
        public bool?  CanReadCorrectedSubchannelWithC2 { get; set; }
        public bool?  CanReadDCB                       { get; set; }
        public bool?  CanReadDDS                       { get; set; }
        public bool?  CanReadDMI                       { get; set; }
        public bool?  CanReadDiscInformation           { get; set; }
        public bool?  CanReadFullTOC                   { get; set; }
        public bool?  CanReadHDCMI                     { get; set; }
        public bool?  CanReadLayerCapacity             { get; set; }
        public bool?  CanReadFirstTrackPreGap          { get; set; }
        public bool?  CanReadLeadIn                    { get; set; }
        public bool?  CanReadLeadOut                   { get; set; }
        public bool?  CanReadMediaID                   { get; set; }
        public bool?  CanReadMediaSerial               { get; set; }
        public bool?  CanReadPAC                       { get; set; }
        public bool?  CanReadPFI                       { get; set; }
        public bool?  CanReadPMA                       { get; set; }
        public bool?  CanReadPQSubchannel              { get; set; }
        public bool?  CanReadPQSubchannelWithC2        { get; set; }
        public bool?  CanReadPRI                       { get; set; }
        public bool?  CanReadRWSubchannel              { get; set; }
        public bool?  CanReadRWSubchannelWithC2        { get; set; }
        public bool?  CanReadRecordablePFI             { get; set; }
        public bool?  CanReadSpareAreaInformation      { get; set; }
        public bool?  CanReadTOC                       { get; set; }
        public byte?  Density                          { get; set; }
        public uint?  LongBlockSize                    { get; set; }
        public string Manufacturer                     { get; set; }
        public bool   MediaIsRecognized                { get; set; }
        public byte?  MediumType                       { get; set; }
        public string MediumTypeName                   { get; set; }
        public string Model                            { get; set; }
        public bool?  SupportsHLDTSTReadRawDVD         { get; set; }
        public bool?  SupportsNECReadCDDA              { get; set; }
        public bool?  SupportsPioneerReadCDDA          { get; set; }
        public bool?  SupportsPioneerReadCDDAMSF       { get; set; }
        public bool?  SupportsPlextorReadCDDA          { get; set; }
        public bool?  SupportsPlextorReadRawDVD        { get; set; }
        public bool?  SupportsRead10                   { get; set; }
        public bool?  SupportsRead12                   { get; set; }
        public bool?  SupportsRead16                   { get; set; }
        public bool?  SupportsRead6                    { get; set; }
        public bool?  SupportsReadCapacity16           { get; set; }
        public bool?  SupportsReadCapacity             { get; set; }
        public bool?  SupportsReadCd                   { get; set; }
        public bool?  SupportsReadCdMsf                { get; set; }
        public bool?  SupportsReadCdRaw                { get; set; }
        public bool?  SupportsReadCdMsfRaw             { get; set; }
        public bool?  SupportsReadLong16               { get; set; }
        public bool?  SupportsReadLong                 { get; set; }

        public byte[] ModeSense6Data  { get; set; }
        public byte[] ModeSense10Data { get; set; }

        public virtual Chs     CHS                 { get; set; }
        public virtual Chs     CurrentCHS          { get; set; }
        public         uint?   LBASectors          { get; set; }
        public         ulong?  LBA48Sectors        { get; set; }
        public         ushort? LogicalAlignment    { get; set; }
        public         ushort? NominalRotationRate { get; set; }
        public         uint?   PhysicalBlockSize   { get; set; }
        public         bool?   SolidStateDevice    { get; set; }
        public         ushort? UnformattedBPT      { get; set; }
        public         ushort? UnformattedBPS      { get; set; }

        public bool? SupportsReadDmaLba       { get; set; }
        public bool? SupportsReadDmaRetryLba  { get; set; }
        public bool? SupportsReadLba          { get; set; }
        public bool? SupportsReadRetryLba     { get; set; }
        public bool? SupportsReadLongLba      { get; set; }
        public bool? SupportsReadLongRetryLba { get; set; }
        public bool? SupportsSeekLba          { get; set; }

        public bool? SupportsReadDmaLba48 { get; set; }
        public bool? SupportsReadLba48    { get; set; }

        public bool? SupportsReadDma       { get; set; }
        public bool? SupportsReadDmaRetry  { get; set; }
        public bool? SupportsReadRetry     { get; set; }
        public bool? SupportsReadSectors   { get; set; }
        public bool? SupportsReadLongRetry { get; set; }
        public bool? SupportsSeek          { get; set; }

        public bool?  CanReadingIntersessionLeadIn  { get; set; }
        public bool?  CanReadingIntersessionLeadOut { get; set; }
        public byte[] IntersessionLeadInData        { get; set; }
        public byte[] IntersessionLeadOutData       { get; set; }

        [JsonIgnore]
        public int? AtaId { get; set; }

        #region SCSI data
        public byte[] Read6Data      { get; set; }
        public byte[] Read10Data     { get; set; }
        public byte[] Read12Data     { get; set; }
        public byte[] Read16Data     { get; set; }
        public byte[] ReadLong10Data { get; set; }
        public byte[] ReadLong16Data { get; set; }
        #endregion

        #region ATA data
        public byte[] ReadSectorsData      { get; set; }
        public byte[] ReadSectorsRetryData { get; set; }
        public byte[] ReadDmaData          { get; set; }
        public byte[] ReadDmaRetryData     { get; set; }
        public byte[] ReadLbaData          { get; set; }
        public byte[] ReadRetryLbaData     { get; set; }
        public byte[] ReadDmaLbaData       { get; set; }
        public byte[] ReadDmaRetryLbaData  { get; set; }
        public byte[] ReadLba48Data        { get; set; }
        public byte[] ReadDmaLba48Data     { get; set; }
        public byte[] ReadLongData         { get; set; }
        public byte[] ReadLongRetryData    { get; set; }
        public byte[] ReadLongLbaData      { get; set; }
        public byte[] ReadLongRetryLbaData { get; set; }
        #endregion

        #region CompactDisc data
        public byte[] TocData                       { get; set; }
        public byte[] FullTocData                   { get; set; }
        public byte[] AtipData                      { get; set; }
        public byte[] PmaData                       { get; set; }
        public byte[] ReadCdData                    { get; set; }
        public byte[] ReadCdMsfData                 { get; set; }
        public byte[] ReadCdFullData                { get; set; }
        public byte[] ReadCdMsfFullData             { get; set; }
        public byte[] Track1PregapData              { get; set; }
        public byte[] LeadInData                    { get; set; }
        public byte[] LeadOutData                   { get; set; }
        public byte[] C2PointersData                { get; set; }
        public byte[] PQSubchannelData              { get; set; }
        public byte[] RWSubchannelData              { get; set; }
        public byte[] CorrectedSubchannelData       { get; set; }
        public byte[] PQSubchannelWithC2Data        { get; set; }
        public byte[] RWSubchannelWithC2Data        { get; set; }
        public byte[] CorrectedSubchannelWithC2Data { get; set; }
        #endregion

        #region DVD data
        public byte[] PfiData         { get; set; }
        public byte[] DmiData         { get; set; }
        public byte[] CmiData         { get; set; }
        public byte[] DvdBcaData      { get; set; }
        public byte[] DvdAacsData     { get; set; }
        public byte[] DvdDdsData      { get; set; }
        public byte[] DvdSaiData      { get; set; }
        public byte[] PriData         { get; set; }
        public byte[] EmbossedPfiData { get; set; }
        public byte[] AdipData        { get; set; }
        public byte[] DcbData         { get; set; }
        public byte[] HdCmiData       { get; set; }
        public byte[] DvdLayerData    { get; set; }
        #endregion

        #region Blu-ray data
        public byte[] BluBcaData { get; set; }
        public byte[] BluDdsData { get; set; }
        public byte[] BluSaiData { get; set; }
        public byte[] BluDiData  { get; set; }
        public byte[] BluPacData { get; set; }
        #endregion

        #region Vendor data
        public byte[] PlextorReadCddaData    { get; set; }
        public byte[] PioneerReadCddaData    { get; set; }
        public byte[] PioneerReadCddaMsfData { get; set; }
        public byte[] NecReadCddaData        { get; set; }
        public byte[] PlextorReadRawDVDData  { get; set; }
        public byte[] HLDTSTReadRawDVDData   { get; set; }
        #endregion
    }

    public class Ssc
    {
        public Ssc() { }

        public Ssc(sscType ssc)
        {
            if(ssc.BlockSizeGranularitySpecified)
                BlockSizeGranularity = ssc.BlockSizeGranularity;

            if(ssc.MaxBlockLengthSpecified)
                MaxBlockLength = ssc.MaxBlockLength;

            if(ssc.MinBlockLengthSpecified)
                MinBlockLength = ssc.MinBlockLength;

            if(ssc.SupportedDensities != null)
                SupportedDensities = new List<SupportedDensity>(ssc.SupportedDensities);

            if(ssc.SupportedMediaTypes != null)
            {
                SupportedMediaTypes = new List<SscSupportedMedia>();

                foreach(SupportedMedia mediaType in ssc.SupportedMediaTypes)
                    SupportedMediaTypes.Add(new SscSupportedMedia(mediaType));
            }

            if(ssc.TestedMedia == null)
                return;

            TestedMedia = new List<TestedSequentialMedia>();

            foreach(SequentialMedia testedMedia in ssc.TestedMedia)
                TestedMedia.Add(new TestedSequentialMedia(testedMedia));
        }

        [JsonIgnore]
        public int Id { get;                     set; }
        public byte? BlockSizeGranularity { get; set; }
        public uint? MaxBlockLength       { get; set; }
        public uint? MinBlockLength       { get; set; }

        public virtual List<SupportedDensity>      SupportedDensities  { get; set; }
        public virtual List<SscSupportedMedia>     SupportedMediaTypes { get; set; }
        public virtual List<TestedSequentialMedia> TestedMedia         { get; set; }
    }

    public class TestedSequentialMedia
    {
        public TestedSequentialMedia() { }

        public TestedSequentialMedia(SequentialMedia media)
        {
            if(media.CanReadMediaSerialSpecified)
                CanReadMediaSerial = media.CanReadMediaSerial;

            if(media.DensitySpecified)
                Density = media.Density;

            Manufacturer      = media.Manufacturer;
            MediaIsRecognized = media.MediaIsRecognized;

            if(media.MediumTypeSpecified)
                MediumType = media.MediumType;

            MediumTypeName = media.MediumTypeName;
            Model          = media.Model;

            if(media.SupportedDensities != null)
                SupportedDensities = new List<SupportedDensity>(media.SupportedDensities);

            if(media.SupportedMediaTypes != null)
            {
                SupportedMediaTypes = new List<SscSupportedMedia>();

                foreach(SupportedMedia supportedMedia in media.SupportedMediaTypes)
                    SupportedMediaTypes.Add(new SscSupportedMedia(supportedMedia));
            }

            ModeSense6Data  = media.ModeSense6Data;
            ModeSense10Data = media.ModeSense10Data;
        }

        [JsonIgnore]
        public int Id { get;                                              set; }
        public         bool?                   CanReadMediaSerial  { get; set; }
        public         byte?                   Density             { get; set; }
        public         string                  Manufacturer        { get; set; }
        public         bool                    MediaIsRecognized   { get; set; }
        public         byte?                   MediumType          { get; set; }
        public         string                  MediumTypeName      { get; set; }
        public         string                  Model               { get; set; }
        public virtual List<SupportedDensity>  SupportedDensities  { get; set; }
        public virtual List<SscSupportedMedia> SupportedMediaTypes { get; set; }

        public byte[] ModeSense6Data  { get; set; }
        public byte[] ModeSense10Data { get; set; }
    }

    public class Pcmcia
    {
        public string[] AdditionalInformation;

        public Pcmcia() { }

        public Pcmcia(pcmciaType pcmcia)
        {
            AdditionalInformation = pcmcia.AdditionalInformation;
            CIS                   = pcmcia.CIS;
            Compliance            = pcmcia.Compliance;

            if(pcmcia.ManufacturerCodeSpecified)
                ManufacturerCode = pcmcia.ManufacturerCode;

            if(pcmcia.CardCodeSpecified)
                CardCode = pcmcia.CardCode;

            Manufacturer = pcmcia.Manufacturer;
            ProductName  = pcmcia.ProductName;
        }

        [JsonIgnore]
        public int Id { get;                   set; }
        public byte[]  CIS              { get; set; }
        public string  Compliance       { get; set; }
        public ushort? ManufacturerCode { get; set; }
        public ushort? CardCode         { get; set; }
        public string  Manufacturer     { get; set; }
        public string  ProductName      { get; set; }
    }

    public class MmcSd
    {
        public MmcSd() { }

        public MmcSd(mmcsdType mmcSd)
        {
            CID         = mmcSd.CID;
            CSD         = mmcSd.CSD;
            OCR         = mmcSd.OCR;
            SCR         = mmcSd.SCR;
            ExtendedCSD = mmcSd.ExtendedCSD;
        }

        [JsonIgnore]
        public int Id { get;             set; }
        public byte[] CID         { get; set; }
        public byte[] CSD         { get; set; }
        public byte[] OCR         { get; set; }
        public byte[] SCR         { get; set; }
        public byte[] ExtendedCSD { get; set; }
    }

    public class SscSupportedMedia
    {
        public SscSupportedMedia() { }

        public SscSupportedMedia(SupportedMedia media)
        {
            MediumType   = media.MediumType;
            Width        = media.Width;
            Length       = media.Length;
            Organization = media.Organization;
            Name         = media.Name;
            Description  = media.Description;

            if(media.DensityCodes == null)
                return;

            DensityCodes = new List<DensityCode>();

            foreach(int densityCode in media.DensityCodes)
                DensityCodes.Add(new DensityCode
                {
                    Code = densityCode
                });
        }

        [JsonIgnore]
        public int Id { get;                                 set; }
        public         byte              MediumType   { get; set; }
        public virtual List<DensityCode> DensityCodes { get; set; }
        public         ushort            Width        { get; set; }
        public         ushort            Length       { get; set; }
        public         string            Organization { get; set; }
        public         string            Name         { get; set; }
        public         string            Description  { get; set; }
    }

    public class DensityCode : IEquatable<DensityCode>
    {
        [JsonIgnore, Key]
        public int Id { get; set; }

        public int Code { get; set; }

        public bool Equals(DensityCode other)
        {
            if(ReferenceEquals(null, other))
                return false;

            if(ReferenceEquals(this, other))
                return true;

            return Code == other.Code;
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;

            if(ReferenceEquals(this, obj))
                return true;

            return obj.GetType() == GetType() && Equals((DensityCode)obj);
        }

        public override int GetHashCode() => Code;
    }
}