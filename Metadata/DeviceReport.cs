// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DeviceReport.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// TODO: Re-enable CS1591 in this file

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs.Devices.ATA;
using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.CommonTypes.Structs.Devices.SCSI.Modes;


#pragma warning disable 1591

// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aaru.CommonTypes.Metadata;


// ReSharper disable once PartialTypeWithSinglePart
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                             IncludeFields = true)]
[JsonSerializable(typeof(DeviceReport))]
public partial class DeviceReportContext : JsonSerializerContext {}

public class DeviceReport
{
    [JsonIgnore]
    public int Id { get;                                                      set; }
    public virtual Usb                       USB                       { get; set; }
    public virtual FireWire                  FireWire                  { get; set; }
    public virtual Pcmcia                    PCMCIA                    { get; set; }
    public         bool                      CompactFlash              { get; set; }
    public virtual Ata                       ATA                       { get; set; }
    public virtual Ata                       ATAPI                     { get; set; }
    public virtual Scsi                      SCSI                      { get; set; }
    public virtual MmcSd                     MultiMediaCard            { get; set; }
    public virtual MmcSd                     SecureDigital             { get; set; }
    public virtual GdRomSwapDiscCapabilities GdRomSwapDiscCapabilities { get; set; }

    public string     Manufacturer { get; set; }
    public string     Model        { get; set; }
    public string     Revision     { get; set; }
    public DeviceType Type         { get; set; }
}

public class Usb
{
    [JsonIgnore]
    public int Id { get; set; }
    [DisplayName("Vendor ID"), DisplayFormat(DataFormatString = "0x{0:X4}")]
    public ushort VendorID { get; set; }
    [DisplayName("Product ID"), DisplayFormat(DataFormatString = "0x{0:X4}")]
    public ushort ProductID { get;    set; }
    public string Manufacturer { get; set; }
    public string Product      { get; set; }
    [DisplayName("Removable media")]
    public bool RemovableMedia { get; set; }
    public byte[] Descriptors { get;  set; }
}

public class FireWire
{
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
    public Identify.IdentifyDevice? IdentifyDevice => Structs.Devices.ATA.Identify.Decode(Identify);

    [JsonIgnore]
    public int Id { get;                                     set; }
    public         byte[]            Identify         { get; set; }
    public virtual TestedMedia       ReadCapabilities { get; set; }
    public virtual List<TestedMedia> RemovableMedias  { get; set; }
}

public class Chs
{
    [JsonIgnore]
    public int Id { get;           set; }
    public ushort Cylinders { get; set; }
    public ushort Heads     { get; set; }
    public ushort Sectors   { get; set; }
}

public class Scsi
{
    public Inquiry? Inquiry => Structs.Devices.SCSI.Inquiry.Decode(InquiryData);

    [JsonIgnore]
    public int Id { get; set; }
    [DisplayName("Data from INQUIRY command")]
    public byte[] InquiryData { get;               set; }
    public virtual List<ScsiPage> EVPDPages { get; set; }
    [DisplayName("Supports MODE SENSE(6)")]
    public bool SupportsModeSense6 { get; set; }
    [DisplayName("Supports MODE SENSE(10)")]
    public bool SupportsModeSense10 { get; set; }
    [DisplayName("Supports MODE SENSE with subpages")]
    public bool SupportsModeSubpages { get;                  set; }
    public virtual ScsiMode          ModeSense        { get; set; }
    public virtual Mmc               MultiMediaDevice { get; set; }
    public virtual TestedMedia       ReadCapabilities { get; set; }
    public virtual List<TestedMedia> RemovableMedias  { get; set; }
    public virtual Ssc               SequentialDevice { get; set; }
    [DisplayName("Data from MODE SENSE(6) command")]
    public byte[] ModeSense6Data { get; set; }
    [DisplayName("Data from MODE SENSE(10) command")]
    public byte[] ModeSense10Data { get; set; }
    [DisplayName("Data from MODE SENSE(6) command (current)")]
    public byte[] ModeSense6CurrentData { get; set; }
    [DisplayName("Data from MODE SENSE(10) command (current)")]
    public byte[] ModeSense10CurrentData { get; set; }
    [DisplayName("Data from MODE SENSE(6) command (changeable)")]
    public byte[] ModeSense6ChangeableData { get; set; }
    [DisplayName("Data from MODE SENSE(10) command (changeable)")]
    public byte[] ModeSense10ChangeableData { get; set; }

    [JsonIgnore]
    public int? SequentialDeviceId { get; set; }
}

public class ScsiMode
{
    [JsonIgnore]
    public int Id { get; set; }
    [DisplayName("Medium type code")]
    public byte? MediumType { get; set; }
    [DisplayName("Write protected")]
    public bool WriteProtected { get;                            set; }
    public virtual List<BlockDescriptor> BlockDescriptors { get; set; }
    public         byte?                 Speed            { get; set; }
    [DisplayName("Buffered mode")]
    public byte? BufferedMode { get; set; }
    [DisplayName("Blank check enabled")]
    public bool BlankCheckEnabled { get; set; }
    [DisplayName("DPO and FUA")]
    public bool DPOandFUA { get;                   set; }
    public virtual List<ScsiPage> ModePages { get; set; }
}

public class BlockDescriptor
{
    [JsonIgnore]
    public int Id { get;         set; }
    public byte   Density { get; set; }
    public ulong? Blocks  { get; set; }
    [DisplayName("Block length (bytes)")]
    public uint? BlockLength { get; set; }
}

public class ScsiPage
{
    [JsonIgnore]
    public int Id { get;         set; }
    public byte   page    { get; set; }
    public byte?  subpage { get; set; }
    public byte[] value   { get; set; }
}

public class Mmc
{
    [JsonIgnore]
    public int Id { get; set; }
    public virtual ModePage_2A       ModeSense2A     => ModePage_2A.Decode(ModeSense2AData);
    public virtual MmcFeatures       Features        { get; set; }
    public virtual List<TestedMedia> TestedMedia     { get; set; }
    public         byte[]            ModeSense2AData { get; set; }
    [JsonIgnore]
    public int? FeaturesId { get; set; }
}

public class MmcFeatures
{
    [JsonIgnore]
    public int Id { get; set; }
    [DisplayName("AACS version")]
    public byte? AACSVersion { get; set; }
    [DisplayName("AGIDs")]
    public byte? AGIDs { get; set; }
    [DisplayName("Binding nonce blocks")]
    public byte? BindingNonceBlocks { get; set; }
    [DisplayName("Blocks per readable unit")]
    public ushort? BlocksPerReadableUnit { get; set; }
    [DisplayName("Buffer under-run free in DVD writing")]
    public bool BufferUnderrunFreeInDVD { get; set; }
    [DisplayName("Buffer under-run free in SAO writing")]
    public bool BufferUnderrunFreeInSAO { get; set; }
    [DisplayName("Buffer under-run free in TAO writing")]
    public bool BufferUnderrunFreeInTAO { get; set; }
    [DisplayName("Can audio scan")]
    public bool CanAudioScan { get; set; }
    [DisplayName("Can eject")]
    public bool CanEject { get; set; }
    [DisplayName("Can erase sectors")]
    public bool CanEraseSector { get; set; }
    [DisplayName("Can expand BD-RE spare area")]
    public bool CanExpandBDRESpareArea { get; set; }
    [DisplayName("Can format media")]
    public bool CanFormat { get; set; }
    [DisplayName("Can format BD-RE without spare area")]
    public bool CanFormatBDREWithoutSpare { get; set; }
    [DisplayName("Can do a fully certified format")]
    public bool CanFormatCert { get; set; }
    [DisplayName("Can do a FRF format")]
    public bool CanFormatFRF { get; set; }
    [DisplayName("Can do a quick certified format")]
    public bool CanFormatQCert { get; set; }
    [DisplayName("Can do a RRM format")]
    public bool CanFormatRRM { get; set; }
    [DisplayName("Can generate binding nonce")]
    public bool CanGenerateBindingNonce { get; set; }
    [DisplayName("Can load")]
    public bool CanLoad { get; set; }
    [DisplayName("Can mute separate channels")]
    public bool CanMuteSeparateChannels { get; set; }
    [DisplayName("Can overwrite track in SAO")]
    public bool CanOverwriteSAOTrack { get; set; }
    [DisplayName("Can overwrite track in TAO")]
    public bool CanOverwriteTAOTrack { get; set; }
    [DisplayName("Can play CD-DA")]
    public bool CanPlayCDAudio { get; set; }
    [DisplayName("Can pseudo-overwrite BD-R")]
    public bool CanPseudoOverwriteBDR { get; set; }
    [DisplayName("Can read all dual-layer recordables")]
    public bool CanReadAllDualR { get; set; }
    [DisplayName("Can read all dual-layer rewritables")]
    public bool CanReadAllDualRW { get; set; }
    [DisplayName("Can read Blu-ray")]
    public bool CanReadBD { get; set; }
    [DisplayName("Can read BD-R")]
    public bool CanReadBDR { get; set; }
    [DisplayName("Can read BD-RE v1")]
    public bool CanReadBDRE1 { get; set; }
    [DisplayName("Can read BD-RE v2")]
    public bool CanReadBDRE2 { get; set; }
    [DisplayName("Can read BD-ROM")]
    public bool CanReadBDROM { get; set; }
    [DisplayName("Can read BCA from Blu-ray")]
    public bool CanReadBluBCA { get; set; }
    [DisplayName("Can read CD")]
    public bool CanReadCD { get; set; }
    [DisplayName("Can read CD-MRW")]
    public bool CanReadCDMRW { get; set; }
    [DisplayName("Can read CPRM's MKB")]
    public bool CanReadCPRM_MKB { get; set; }
    [DisplayName("Can read DDCD")]
    public bool CanReadDDCD { get; set; }
    [DisplayName("Can read DVD")]
    public bool CanReadDVD { get; set; }
    [DisplayName("Can read DVD+MRW")]
    public bool CanReadDVDPlusMRW { get; set; }
    [DisplayName("Can read DVD+R")]
    public bool CanReadDVDPlusR { get; set; }
    [DisplayName("Can read DVD+R DL")]
    public bool CanReadDVDPlusRDL { get; set; }
    [DisplayName("Can read DVD+RW")]
    public bool CanReadDVDPlusRW { get; set; }
    [DisplayName("Can read DVD+RW DL")]
    public bool CanReadDVDPlusRWDL { get; set; }
    [DisplayName("Can read drive's AACS certificate")]
    public bool CanReadDriveAACSCertificate { get; set; }
    [DisplayName("Can read HD DVD")]
    public bool CanReadHDDVD { get; set; }
    [DisplayName("Can read HD DVD-R")]
    public bool CanReadHDDVDR { get; set; }
    [DisplayName("Can read HD DVD-RAM")]
    public bool CanReadHDDVDRAM { get; set; }
    [DisplayName("Can read Lead-In's CD-TEXT")]
    public bool CanReadLeadInCDText { get; set; }
    [DisplayName("Can read old generation BD-R")]
    public bool CanReadOldBDR { get; set; }
    [DisplayName("Can read old generation BD-RE")]
    public bool CanReadOldBDRE { get; set; }
    [DisplayName("Can read old generation BD-ROM")]
    public bool CanReadOldBDROM { get; set; }
    [DisplayName("Can read spare area information")]
    public bool CanReadSpareAreaInformation { get; set; }
    [DisplayName("Can report drive serial number")]
    public bool CanReportDriveSerial { get; set; }
    [DisplayName("Can report media serial number")]
    public bool CanReportMediaSerial { get; set; }
    [DisplayName("Can test write DDCD-R")]
    public bool CanTestWriteDDCDR { get; set; }
    [DisplayName("Can test write DVD")]
    public bool CanTestWriteDVD { get; set; }
    [DisplayName("Can test write in SAO mode")]
    public bool CanTestWriteInSAO { get; set; }
    [DisplayName("Can test write in TAO mode")]
    public bool CanTestWriteInTAO { get; set; }
    [DisplayName("Can upgrade firmware")]
    public bool CanUpgradeFirmware { get; set; }
    [DisplayName("Can write Blu-ray")]
    public bool CanWriteBD { get; set; }
    [DisplayName("Can write BD-R")]
    public bool CanWriteBDR { get; set; }
    [DisplayName("Can write BD-RE v1")]
    public bool CanWriteBDRE1 { get; set; }
    [DisplayName("Can write BD-RE v2")]
    public bool CanWriteBDRE2 { get; set; }
    [DisplayName("Can write bus encrypted blocks")]
    public bool CanWriteBusEncryptedBlocks { get; set; }
    [DisplayName("Can write CD-MRW")]
    public bool CanWriteCDMRW { get; set; }
    [DisplayName("Can write CD-RW")]
    public bool CanWriteCDRW { get; set; }
    [DisplayName("Can write CD-RW CAV")]
    public bool CanWriteCDRWCAV { get; set; }
    [DisplayName("Can write CD in SAO mode")]
    public bool CanWriteCDSAO { get; set; }
    [DisplayName("Can write CD in TAO mode")]
    public bool CanWriteCDTAO { get; set; }
    [DisplayName("Can write CSS managed DVD")]
    public bool CanWriteCSSManagedDVD { get; set; }
    [DisplayName("Can write DDCD-R")]
    public bool CanWriteDDCDR { get; set; }
    [DisplayName("Can write DDCD-RW")]
    public bool CanWriteDDCDRW { get; set; }
    [DisplayName("Can write DVD+MRW")]
    public bool CanWriteDVDPlusMRW { get; set; }
    [DisplayName("Can write DVD+R")]
    public bool CanWriteDVDPlusR { get; set; }
    [DisplayName("Can write DVD+R DL")]
    public bool CanWriteDVDPlusRDL { get; set; }
    [DisplayName("Can write DVD+RW")]
    public bool CanWriteDVDPlusRW { get; set; }
    [DisplayName("Can write DVD+RW DL")]
    public bool CanWriteDVDPlusRWDL { get; set; }
    [DisplayName("Can write DVD-R")]
    public bool CanWriteDVDR { get; set; }
    [DisplayName("Can write DVD-R DL")]
    public bool CanWriteDVDRDL { get; set; }
    [DisplayName("Can write DVD-RW")]
    public bool CanWriteDVDRW { get; set; }
    [DisplayName("Can write HD DVD-R")]
    public bool CanWriteHDDVDR { get; set; }
    [DisplayName("Can write HD DVD-RAM")]
    public bool CanWriteHDDVDRAM { get; set; }
    [DisplayName("Can write old generation BD-R")]
    public bool CanWriteOldBDR { get; set; }
    [DisplayName("Can write old generation BD-RE")]
    public bool CanWriteOldBDRE { get; set; }
    [DisplayName("Can write packet subchannel in TAO")]
    public bool CanWritePackedSubchannelInTAO { get; set; }
    [DisplayName("Can write RW subchannel in SAO")]
    public bool CanWriteRWSubchannelInSAO { get; set; }
    [DisplayName("Can write RW subchannel in TAO")]
    public bool CanWriteRWSubchannelInTAO { get; set; }
    [DisplayName("Can write RAW-96 sectors")]
    public bool CanWriteRaw { get; set; }
    [DisplayName("Can write RAW-96 sectors in multisession")]
    public bool CanWriteRawMultiSession { get; set; }
    [DisplayName("Can write RAW-96 sectors in TAO")]
    public bool CanWriteRawSubchannelInTAO { get; set; }
    [DisplayName("Changer is side change capable")]
    public bool ChangerIsSideChangeCapable { get; set; }
    [DisplayName("Changer slots")]
    public byte ChangerSlots { get; set; }
    [DisplayName("Changer supports disc present")]
    public bool ChangerSupportsDiscPresent { get; set; }
    [DisplayName("CPRM version")]
    public byte? CPRMVersion { get; set; }
    [DisplayName("CSS version")]
    public byte? CSSVersion { get; set; }
    [DisplayName("DBML")]
    public bool DBML { get; set; }
    [DisplayName("DVD Multi-Read Specification")]
    public bool DVDMultiRead { get; set; }
    [DisplayName("Has an embedded changer")]
    public bool EmbeddedChanger { get; set; }
    [DisplayName("Has error recovery page")]
    public bool ErrorRecoveryPage { get; set; }
    [DisplayName("Firmware date")]
    public DateTime? FirmwareDate { get; set; }
    [DisplayName("Loading mechanism type")]
    public byte? LoadingMechanismType { get; set; }
    [DisplayName("Locked")]
    public bool Locked { get; set; }
    [DisplayName("Logical block size")]
    public uint? LogicalBlockSize { get; set; }
    [DisplayName("Multi-Read Specification")]
    public bool MultiRead { get; set; }
    [DisplayName("Physical interface standard")]
    public PhysicalInterfaces? PhysicalInterfaceStandard => (PhysicalInterfaces?)PhysicalInterfaceStandardNumber;
    [DisplayName("Physical interface standard number")]
    public uint? PhysicalInterfaceStandardNumber { get; set; }
    [DisplayName("Prevent eject jumper")]
    public bool PreventJumper { get; set; }
    [DisplayName("Supports AACS")]
    public bool SupportsAACS { get; set; }
    [DisplayName("Supports bus encryption")]
    public bool SupportsBusEncryption { get; set; }
    [DisplayName("Supports C2 pointers")]
    public bool SupportsC2 { get; set; }
    [DisplayName("Supports CPRM")]
    public bool SupportsCPRM { get; set; }
    [DisplayName("Supports CSS")]
    public bool SupportsCSS { get; set; }
    [DisplayName("Supports DAP")]
    public bool SupportsDAP { get; set; }
    [DisplayName("Supports device busy event")]
    public bool SupportsDeviceBusyEvent { get; set; }
    [DisplayName("Supports hybrid discs")]
    public bool SupportsHybridDiscs { get; set; }
    [DisplayName("Supports MODE PAGE 1Ch")]
    public bool SupportsModePage1Ch { get; set; }
    [DisplayName("Supports OSSC")]
    public bool SupportsOSSC { get; set; }
    [DisplayName("Supports PWP")]
    public bool SupportsPWP { get; set; }
    [DisplayName("Supports SWPP")]
    public bool SupportsSWPP { get; set; }
    [DisplayName("Supports SecurDisc")]
    public bool SupportsSecurDisc { get; set; }
    [DisplayName("Support separate volume levels")]
    public bool SupportsSeparateVolume { get; set; }
    [DisplayName("Supports VCPS")]
    public bool SupportsVCPS { get; set; }
    [DisplayName("Supports write inhibit DCB")]
    public bool SupportsWriteInhibitDCB { get; set; }
    [DisplayName("Supports write protect PAC")]
    public bool SupportsWriteProtectPAC { get; set; }
    [DisplayName("Volume levels")]
    public ushort? VolumeLevels { get; set; }
    [DisplayName("MMC FEATURES binary data")]
    public byte[] BinaryData { get; set; }
}

public class TestedMedia
{
    public Identify.IdentifyDevice? IdentifyDevice;

    [JsonIgnore]
    public int Id { get; set; }
    [DisplayName("IDENTIFY DEVICE data")]
    public byte[] IdentifyData { get; set; }
    [DisplayName("Blocks")]
    public ulong? Blocks { get; set; }
    [DisplayName("Bytes per block")]
    public uint? BlockSize { get; set; }
    [DisplayName("Can read AACS")]
    public bool? CanReadAACS { get; set; }
    [DisplayName("Can read ADIP")]
    public bool? CanReadADIP { get; set; }
    [DisplayName("Can read ATIP")]
    public bool? CanReadATIP { get; set; }
    [DisplayName("Can read BCA")]
    public bool? CanReadBCA { get; set; }
    [DisplayName("Can read C2 pointers")]
    public bool? CanReadC2Pointers { get; set; }
    [DisplayName("Can read Copyright Management Information")]
    public bool? CanReadCMI { get; set; }
    [DisplayName("Can read corrected subchannel")]
    public bool? CanReadCorrectedSubchannel { get; set; }
    [DisplayName("Can read corrected subchannel with C2 pointers")]
    public bool? CanReadCorrectedSubchannelWithC2 { get; set; }
    [DisplayName("Can read DCBs")]
    public bool? CanReadDCB { get; set; }
    [DisplayName("Can read DDS")]
    public bool? CanReadDDS { get; set; }
    [DisplayName("Can read DMI")]
    public bool? CanReadDMI { get; set; }
    [DisplayName("Can read disc information")]
    public bool? CanReadDiscInformation { get; set; }
    [DisplayName("Can read full TOC")]
    public bool? CanReadFullTOC { get; set; }
    [DisplayName("Can read HD-DVD Copyright Management Information")]
    public bool? CanReadHDCMI { get; set; }
    [DisplayName("Can read layer capacity")]
    public bool? CanReadLayerCapacity { get; set; }
    [DisplayName("Can read into first track pregap")]
    public bool? CanReadFirstTrackPreGap { get; set; }
    [DisplayName("Can read into Lead-In")]
    public bool? CanReadLeadIn { get; set; }
    [DisplayName("Can read into Lead-Out")]
    public bool? CanReadLeadOut { get; set; }
    [DisplayName("Can read media ID")]
    public bool? CanReadMediaID { get; set; }
    [DisplayName("Can read media serial number")]
    public bool? CanReadMediaSerial { get; set; }
    [DisplayName("Can read PAC")]
    public bool? CanReadPAC { get; set; }
    [DisplayName("Can read PFI")]
    public bool? CanReadPFI { get; set; }
    [DisplayName("Can read PMA")]
    public bool? CanReadPMA { get; set; }
    [DisplayName("Can read PQ subchannel")]
    public bool? CanReadPQSubchannel { get; set; }
    [DisplayName("Can read PQ subchannel with C2 pointers")]
    public bool? CanReadPQSubchannelWithC2 { get; set; }
    [DisplayName("Can read pre-recorded information")]
    public bool? CanReadPRI { get; set; }
    [DisplayName("Can read RW subchannel")]
    public bool? CanReadRWSubchannel { get; set; }
    [DisplayName("Can read RW subchannel with C2 pointers")]
    public bool? CanReadRWSubchannelWithC2 { get; set; }
    [DisplayName("Can read recordable PFI")]
    public bool? CanReadRecordablePFI { get; set; }
    [DisplayName("Can read spare area information")]
    public bool? CanReadSpareAreaInformation { get; set; }
    [DisplayName("Can read TOC")]
    public bool? CanReadTOC { get; set; }
    [DisplayName("Density code")]
    public byte? Density { get; set; }
    [DisplayName("Bytes per block in READ LONG commands")]
    public uint? LongBlockSize { get; set; }
    [DisplayName("Media manufacturer")]
    public string Manufacturer { get; set; }
    [DisplayName("Media recognized by drive?")]
    public bool MediaIsRecognized { get; set; }
    [DisplayName("Medium type code")]
    public byte? MediumType { get; set; }
    [DisplayName("Media type")]
    public string MediumTypeName { get; set; }
    [DisplayName("Media model")]
    public string Model { get; set; }
    [DisplayName("Can read scrambled DVD sectors using HL-DT-ST cache trick")]
    public bool? SupportsHLDTSTReadRawDVD { get; set; }
    [DisplayName("Supports NEC READ CD-DA command")]
    public bool? SupportsNECReadCDDA { get; set; }
    [DisplayName("Supports Pioneer READ CD-DA command")]
    public bool? SupportsPioneerReadCDDA { get; set; }
    [DisplayName("Supports Pioneer READ CD-DA MSF command")]
    public bool? SupportsPioneerReadCDDAMSF { get; set; }
    [DisplayName("Supports Plextor READ CD-DA command")]
    public bool? SupportsPlextorReadCDDA { get; set; }
    [DisplayName("Can read scrambled DVD sectors using Plextor vendor command")]
    public bool? SupportsPlextorReadRawDVD { get; set; }
    [DisplayName("Supports READ(10) command")]
    public bool? SupportsRead10 { get; set; }
    [DisplayName("Supports READ(12) command")]
    public bool? SupportsRead12 { get; set; }
    [DisplayName("Supports READ(16) command")]
    public bool? SupportsRead16 { get; set; }
    [DisplayName("Supports READ(6) command")]
    public bool? SupportsRead6 { get; set; }
    [DisplayName("Supports READ CAPACITY(16) command")]
    public bool? SupportsReadCapacity16 { get; set; }
    [DisplayName("Supports READ CAPACITY command")]
    public bool? SupportsReadCapacity { get; set; }
    [DisplayName("Supports READ CD command")]
    public bool? SupportsReadCd { get; set; }
    [DisplayName("Supports READ CD MSF command")]
    public bool? SupportsReadCdMsf { get; set; }
    [DisplayName("Supports full sector in READ CD command")]
    public bool? SupportsReadCdRaw { get; set; }
    [DisplayName("Supports full sector in READ CD MSF command")]
    public bool? SupportsReadCdMsfRaw { get; set; }
    [DisplayName("Supports READ LONG(16) command")]
    public bool? SupportsReadLong16 { get; set; }
    [DisplayName("Supports READ LONG command")]
    public bool? SupportsReadLong { get; set; }

    [DisplayName("Data from MODE SENSE(6) command")]
    public byte[] ModeSense6Data { get; set; }
    [DisplayName("Data from MODE SENSE(10) command")]
    public byte[] ModeSense10Data { get; set; }

    public virtual Chs CHS        { get; set; }
    public virtual Chs CurrentCHS { get; set; }
    [DisplayName("Sectors in 28-bit LBA mode")]
    public uint? LBASectors { get; set; }
    [DisplayName("Sectors in 48-bit LBA mode")]
    public ulong? LBA48Sectors { get; set; }
    [DisplayName("Logical alignment")]
    public ushort? LogicalAlignment { get; set; }
    [DisplayName("Nominal rotation rate")]
    public ushort? NominalRotationRate { get; set; }
    [DisplayName("Bytes per block, physical")]
    public uint? PhysicalBlockSize { get; set; }
    [DisplayName("Is it a SSD?")]
    public bool? SolidStateDevice { get; set; }
    [DisplayName("Bytes per unformatted track")]
    public ushort? UnformattedBPT { get; set; }
    [DisplayName("Bytes per unformatted sector")]
    public ushort? UnformattedBPS { get; set; }

    [DisplayName("Supports READ DMA (LBA) command")]
    public bool? SupportsReadDmaLba { get; set; }
    [DisplayName("Supports READ DMA RETRY (LBA) command")]
    public bool? SupportsReadDmaRetryLba { get; set; }
    [DisplayName("Supports READ SECTORS (LBA) command")]
    public bool? SupportsReadLba { get; set; }
    [DisplayName("Supports READ SECTORS RETRY (LBA) command")]
    public bool? SupportsReadRetryLba { get; set; }
    [DisplayName("Supports READ SECTORS LONG (LBA) command")]
    public bool? SupportsReadLongLba { get; set; }
    [DisplayName("Supports READ SECTORS LONG RETRY (LBA) command")]
    public bool? SupportsReadLongRetryLba { get; set; }
    [DisplayName("Supports SEEK (LBA) command")]
    public bool? SupportsSeekLba { get; set; }

    [DisplayName("Supports READ DMA EXT command")]
    public bool? SupportsReadDmaLba48 { get; set; }
    [DisplayName("Supports READ SECTORS EXT command")]
    public bool? SupportsReadLba48 { get; set; }

    [DisplayName("Supports READ DMA command")]
    public bool? SupportsReadDma { get; set; }
    [DisplayName("Supports READ DMA RETRY command")]
    public bool? SupportsReadDmaRetry { get; set; }
    [DisplayName("Supports READ SECTORS RETRY command")]
    public bool? SupportsReadRetry { get; set; }
    [DisplayName("Supports READ SECTORS command")]
    public bool? SupportsReadSectors { get; set; }
    [DisplayName("Supports READ SECTORS LONG RETRY command")]
    public bool? SupportsReadLongRetry { get; set; }
    [DisplayName("Supports SEEK command")]
    public bool? SupportsSeek { get; set; }

    [DisplayName("Can read into inter-session Lead-In")]
    public bool? CanReadingIntersessionLeadIn { get; set; }
    [DisplayName("Can read into inter-session Lead-Out")]
    public bool? CanReadingIntersessionLeadOut { get; set; }
    [DisplayName("Data from inter-session Lead-In")]
    public byte[] IntersessionLeadInData { get; set; }
    [DisplayName("Data from inter-session Lead-Out")]
    public byte[] IntersessionLeadOutData { get; set; }

    [DisplayName("Can read scrambled data using READ CD command")]
    public bool? CanReadCdScrambled { get; set; }
    [DisplayName("Data from scrambled READ CD command")]
    public byte[] ReadCdScrambledData { get; set; }

    [DisplayName("Can read from cache using F1h command subcommand 06h")]
    public bool? CanReadF1_06 { get; set; }
    [DisplayName("Can read from cache using F1h command subcommand 06h")]
    public byte[] ReadF1_06Data { get; set; }
    [DisplayName("Can read from cache using F1h command subcommand 06h targeting Lead-Out")]
    public bool? CanReadF1_06LeadOut { get; set; }
    [DisplayName("Can read from cache using F1h command subcommand 06h targeting Lead-Out")]
    public byte[] ReadF1_06LeadOutData { get; set; }

    [JsonIgnore]
    public int? AtaId { get; set; }
    [JsonIgnore]
    public int? ScsiId { get; set; }
    [JsonIgnore]
    public int? MmcId { get; set; }

    #region SCSI data
    [DisplayName("Data from READ(6) command")]
    public byte[] Read6Data { get; set; }
    [DisplayName("Data from READ(10) command")]
    public byte[] Read10Data { get; set; }
    [DisplayName("Data from READ(12) command")]
    public byte[] Read12Data { get; set; }
    [DisplayName("Data from READ(16) command")]
    public byte[] Read16Data { get; set; }
    [DisplayName("Data from READ LONG(10) command")]
    public byte[] ReadLong10Data { get; set; }
    [DisplayName("Data from READ LONG(16) command")]
    public byte[] ReadLong16Data { get; set; }
    #endregion

    #region ATA data
    [DisplayName("Data from READ SECTORS command")]
    public byte[] ReadSectorsData { get; set; }
    [DisplayName("Data from READ SECTORS RETRY command")]
    public byte[] ReadSectorsRetryData { get; set; }
    [DisplayName("Data from READ DMA command")]
    public byte[] ReadDmaData { get; set; }
    [DisplayName("Data from READ DMA RETRY command")]
    public byte[] ReadDmaRetryData { get; set; }
    [DisplayName("Data from READ SECTORS (LBA) command")]
    public byte[] ReadLbaData { get; set; }
    [DisplayName("Data from READ SECTORS RETRY (LBA) command")]
    public byte[] ReadRetryLbaData { get; set; }
    [DisplayName("Data from READ DMA (LBA) command")]
    public byte[] ReadDmaLbaData { get; set; }
    [DisplayName("Data from READ DMA RETRY (LBA) command")]
    public byte[] ReadDmaRetryLbaData { get; set; }
    [DisplayName("Data from READ SECTORS EXT command")]
    public byte[] ReadLba48Data { get; set; }
    [DisplayName("Data from READ DMA EXT command")]
    public byte[] ReadDmaLba48Data { get; set; }
    [DisplayName("Data from READ SECTORS LONG command")]
    public byte[] ReadLongData { get; set; }
    [DisplayName("Data from READ SECTORS LONG RETRY command")]
    public byte[] ReadLongRetryData { get; set; }
    [DisplayName("Data from READ SECTORS LONG (LBA) command")]
    public byte[] ReadLongLbaData { get; set; }
    [DisplayName("Data from READ SECTORS LONG RETRY (LBA) command")]
    public byte[] ReadLongRetryLbaData { get; set; }
    #endregion

    #region CompactDisc data
    [DisplayName("Data from READ TOC command")]
    public byte[] TocData { get; set; }
    [DisplayName("Data from READ FULL TOC command")]
    public byte[] FullTocData { get; set; }
    [DisplayName("Data from READ ATIP command")]
    public byte[] AtipData { get; set; }
    [DisplayName("Data from READ PMA command")]
    public byte[] PmaData { get; set; }
    [DisplayName("Data from READ CD command")]
    public byte[] ReadCdData { get; set; }
    [DisplayName("Data from READ CD MSF command")]
    public byte[] ReadCdMsfData { get; set; }
    [DisplayName("Data from READ CD (full sector) command")]
    public byte[] ReadCdFullData { get; set; }
    [DisplayName("Data from READ CD MSF (full sector) command")]
    public byte[] ReadCdMsfFullData { get; set; }
    [DisplayName("Data from track 1 pregap")]
    public byte[] Track1PregapData { get; set; }
    [DisplayName("Data from Lead-In")]
    public byte[] LeadInData { get; set; }
    [DisplayName("Data from Lead-Out")]
    public byte[] LeadOutData { get; set; }
    [DisplayName("Data from reading C2 pointers")]
    public byte[] C2PointersData { get; set; }
    [DisplayName("Data from reading with PQ subchannels")]
    public byte[] PQSubchannelData { get; set; }
    [DisplayName("Data from reading with RW subchannels")]
    public byte[] RWSubchannelData { get; set; }
    [DisplayName("Data from reading with corrected subchannels")]
    public byte[] CorrectedSubchannelData { get; set; }
    [DisplayName("Data from reading with PQ subchannels and C2 pointers")]
    public byte[] PQSubchannelWithC2Data { get; set; }
    [DisplayName("Data from reading with RW subchannels and C2 pointers")]
    public byte[] RWSubchannelWithC2Data { get; set; }
    [DisplayName("Data from reading with corrected subchannels and C2 pointers")]
    public byte[] CorrectedSubchannelWithC2Data { get; set; }
    #endregion

    #region DVD data
    [DisplayName("Data from PFI")]
    public byte[] PfiData { get; set; }
    [DisplayName("Data from DMI")]
    public byte[] DmiData { get; set; }
    [DisplayName("Data from DVD's Copyright Management Information")]
    public byte[] CmiData { get; set; }
    [DisplayName("Data from DVD's BCA")]
    public byte[] DvdBcaData { get; set; }
    [DisplayName("Data from DVD's AACS")]
    public byte[] DvdAacsData { get; set; }
    [DisplayName("Data from DVD's DDS")]
    public byte[] DvdDdsData { get; set; }
    [DisplayName("Data from DVD's Spare Area Information")]
    public byte[] DvdSaiData { get; set; }
    [DisplayName("Data from DVD's pre-recorded information")]
    public byte[] PriData { get; set; }
    [DisplayName("Data from embossed PFI")]
    public byte[] EmbossedPfiData { get; set; }
    [DisplayName("Data from ADIP")]
    public byte[] AdipData { get; set; }
    [DisplayName("Data from DCBs")]
    public byte[] DcbData { get; set; }
    [DisplayName("Data from HD-DVD's Copyright Management Information")]
    public byte[] HdCmiData { get; set; }
    [DisplayName("Data from DVD's layer information")]
    public byte[] DvdLayerData { get; set; }
    #endregion

    #region Blu-ray data
    [DisplayName("Data from Blu-ray's BCA")]
    public byte[] BluBcaData { get; set; }
    [DisplayName("Data from Blu-ray's DDS")]
    public byte[] BluDdsData { get; set; }
    [DisplayName("Data from Blu-ray's Spare Area Information")]
    public byte[] BluSaiData { get; set; }
    [DisplayName("Data from Blu-ray's Disc Information")]
    public byte[] BluDiData { get; set; }
    [DisplayName("Data from Blu-ray's PAC")]
    public byte[] BluPacData { get; set; }
    #endregion

    #region Vendor data
    [DisplayName("Data from Plextor's READ CD-DA command")]
    public byte[] PlextorReadCddaData { get; set; }
    [DisplayName("Data from Pioneer's READ CD-DA command")]
    public byte[] PioneerReadCddaData { get; set; }
    [DisplayName("Data from Pioneer's READ CD-DA MSF command")]
    public byte[] PioneerReadCddaMsfData { get; set; }
    [DisplayName("Data from NEC's READ CD-DA command")]
    public byte[] NecReadCddaData { get; set; }
    [DisplayName("Data from Plextor's scrambled DVD reading command")]
    public byte[] PlextorReadRawDVDData { get; set; }
    [DisplayName("Data from HL-DT-ST's scrambled DVD reading trick")]
    public byte[] HLDTSTReadRawDVDData { get; set; }
    #endregion
}

public class Ssc
{
    [JsonIgnore]
    public int Id { get; set; }
    [DisplayName("Block size granularity")]
    public byte? BlockSizeGranularity { get; set; }
    [DisplayName("Maximum block length")]
    public uint? MaxBlockLength { get; set; }
    [DisplayName("Minimum block length")]
    public uint? MinBlockLength { get; set; }

    public virtual List<SupportedDensity>      SupportedDensities  { get; set; }
    public virtual List<SscSupportedMedia>     SupportedMediaTypes { get; set; }
    public virtual List<TestedSequentialMedia> TestedMedia         { get; set; }
}

public class TestedSequentialMedia
{
    [JsonIgnore]
    public int Id { get; set; }
    [DisplayName("Can read media serial?")]
    public bool? CanReadMediaSerial { get; set; }
    [DisplayName("Density code")]
    public byte? Density { get;       set; }
    public string Manufacturer { get; set; }
    [DisplayName("Media recognized by drive?")]
    public bool MediaIsRecognized { get; set; }
    [DisplayName("Medium type code")]
    public byte? MediumType { get; set; }
    [DisplayName("Medium type")]
    public string MediumTypeName { get;                               set; }
    public         string                  Model               { get; set; }
    public virtual List<SupportedDensity>  SupportedDensities  { get; set; }
    public virtual List<SscSupportedMedia> SupportedMediaTypes { get; set; }

    public byte[] ModeSense6Data  { get; set; }
    public byte[] ModeSense10Data { get; set; }

    [JsonIgnore]
    public int? SscId { get; set; }
}

public class Pcmcia
{
    public string[] AdditionalInformation;

    [JsonIgnore]
    public int Id { get;            set; }
    public byte[] CIS        { get; set; }
    public string Compliance { get; set; }
    [DisplayName("Manufacturer code")]
    public ushort? ManufacturerCode { get; set; }
    [DisplayName("Card code")]
    public ushort? CardCode { get;    set; }
    public string Manufacturer { get; set; }
    [DisplayName("Product name")]
    public string ProductName { get; set; }
}

public class MmcSd
{
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

    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => Code;
}

public class GdRomSwapDiscCapabilities
{
    [JsonIgnore, Key]
    public int Id { get; set; }

    public bool   RecognizedSwapDisc              { get; set; }
    public bool   TestCrashed                     { get; set; }
    public byte   SwapDiscLeadOutPMIN             { get; set; }
    public byte   SwapDiscLeadOutPSEC             { get; set; }
    public byte   SwapDiscLeadOutPFRAM            { get; set; }
    public int    SwapDiscLeadOutStart            { get; set; }
    public bool   Lba0Readable                    { get; set; }
    public byte[] Lba0Data                        { get; set; }
    public byte[] Lba0Sense                       { get; set; }
    public string Lba0DecodedSense                { get; set; }
    public bool   Lba0ScrambledReadable           { get; set; }
    public byte[] Lba0ScrambledData               { get; set; }
    public byte[] Lba0ScrambledSense              { get; set; }
    public string Lba0ScrambledDecodedSense       { get; set; }
    public bool   Lba44990Readable                { get; set; }
    public byte[] Lba44990Data                    { get; set; }
    public byte[] Lba44990Sense                   { get; set; }
    public string Lba44990DecodedSense            { get; set; }
    public int    Lba44990ReadableCluster         { get; set; }
    public bool   Lba45000Readable                { get; set; }
    public byte[] Lba45000Data                    { get; set; }
    public byte[] Lba45000Sense                   { get; set; }
    public string Lba45000DecodedSense            { get; set; }
    public int    Lba45000ReadableCluster         { get; set; }
    public bool   Lba50000Readable                { get; set; }
    public byte[] Lba50000Data                    { get; set; }
    public byte[] Lba50000Sense                   { get; set; }
    public string Lba50000DecodedSense            { get; set; }
    public int    Lba50000ReadableCluster         { get; set; }
    public bool   Lba100000Readable               { get; set; }
    public byte[] Lba100000Data                   { get; set; }
    public byte[] Lba100000Sense                  { get; set; }
    public string Lba100000DecodedSense           { get; set; }
    public int    Lba100000ReadableCluster        { get; set; }
    public bool   Lba400000Readable               { get; set; }
    public byte[] Lba400000Data                   { get; set; }
    public byte[] Lba400000Sense                  { get; set; }
    public string Lba400000DecodedSense           { get; set; }
    public int    Lba400000ReadableCluster        { get; set; }
    public bool   Lba450000Readable               { get; set; }
    public byte[] Lba450000Data                   { get; set; }
    public byte[] Lba450000Sense                  { get; set; }
    public string Lba450000DecodedSense           { get; set; }
    public int    Lba450000ReadableCluster        { get; set; }
    public bool   Lba44990PqReadable              { get; set; }
    public byte[] Lba44990PqData                  { get; set; }
    public byte[] Lba44990PqSense                 { get; set; }
    public string Lba44990PqDecodedSense          { get; set; }
    public int    Lba44990PqReadableCluster       { get; set; }
    public bool   Lba45000PqReadable              { get; set; }
    public byte[] Lba45000PqData                  { get; set; }
    public byte[] Lba45000PqSense                 { get; set; }
    public string Lba45000PqDecodedSense          { get; set; }
    public int    Lba45000PqReadableCluster       { get; set; }
    public bool   Lba50000PqReadable              { get; set; }
    public byte[] Lba50000PqData                  { get; set; }
    public byte[] Lba50000PqSense                 { get; set; }
    public string Lba50000PqDecodedSense          { get; set; }
    public int    Lba50000PqReadableCluster       { get; set; }
    public bool   Lba100000PqReadable             { get; set; }
    public byte[] Lba100000PqData                 { get; set; }
    public byte[] Lba100000PqSense                { get; set; }
    public string Lba100000PqDecodedSense         { get; set; }
    public int    Lba100000PqReadableCluster      { get; set; }
    public bool   Lba400000PqReadable             { get; set; }
    public byte[] Lba400000PqData                 { get; set; }
    public byte[] Lba400000PqSense                { get; set; }
    public string Lba400000PqDecodedSense         { get; set; }
    public int    Lba400000PqReadableCluster      { get; set; }
    public bool   Lba450000PqReadable             { get; set; }
    public byte[] Lba450000PqData                 { get; set; }
    public byte[] Lba450000PqSense                { get; set; }
    public string Lba450000PqDecodedSense         { get; set; }
    public int    Lba450000PqReadableCluster      { get; set; }
    public bool   Lba44990RwReadable              { get; set; }
    public byte[] Lba44990RwData                  { get; set; }
    public byte[] Lba44990RwSense                 { get; set; }
    public string Lba44990RwDecodedSense          { get; set; }
    public int    Lba44990RwReadableCluster       { get; set; }
    public bool   Lba45000RwReadable              { get; set; }
    public byte[] Lba45000RwData                  { get; set; }
    public byte[] Lba45000RwSense                 { get; set; }
    public string Lba45000RwDecodedSense          { get; set; }
    public int    Lba45000RwReadableCluster       { get; set; }
    public bool   Lba50000RwReadable              { get; set; }
    public byte[] Lba50000RwData                  { get; set; }
    public byte[] Lba50000RwSense                 { get; set; }
    public string Lba50000RwDecodedSense          { get; set; }
    public int    Lba50000RwReadableCluster       { get; set; }
    public bool   Lba100000RwReadable             { get; set; }
    public byte[] Lba100000RwData                 { get; set; }
    public byte[] Lba100000RwSense                { get; set; }
    public string Lba100000RwDecodedSense         { get; set; }
    public int    Lba100000RwReadableCluster      { get; set; }
    public bool   Lba400000RwReadable             { get; set; }
    public byte[] Lba400000RwData                 { get; set; }
    public byte[] Lba400000RwSense                { get; set; }
    public string Lba400000RwDecodedSense         { get; set; }
    public int    Lba400000RwReadableCluster      { get; set; }
    public bool   Lba450000RwReadable             { get; set; }
    public byte[] Lba450000RwData                 { get; set; }
    public byte[] Lba450000RwSense                { get; set; }
    public string Lba450000RwDecodedSense         { get; set; }
    public int    Lba450000RwReadableCluster      { get; set; }
    public bool   Lba44990AudioReadable           { get; set; }
    public byte[] Lba44990AudioData               { get; set; }
    public byte[] Lba44990AudioSense              { get; set; }
    public string Lba44990AudioDecodedSense       { get; set; }
    public int    Lba44990AudioReadableCluster    { get; set; }
    public bool   Lba45000AudioReadable           { get; set; }
    public byte[] Lba45000AudioData               { get; set; }
    public byte[] Lba45000AudioSense              { get; set; }
    public string Lba45000AudioDecodedSense       { get; set; }
    public int    Lba45000AudioReadableCluster    { get; set; }
    public bool   Lba50000AudioReadable           { get; set; }
    public byte[] Lba50000AudioData               { get; set; }
    public byte[] Lba50000AudioSense              { get; set; }
    public string Lba50000AudioDecodedSense       { get; set; }
    public int    Lba50000AudioReadableCluster    { get; set; }
    public bool   Lba100000AudioReadable          { get; set; }
    public byte[] Lba100000AudioData              { get; set; }
    public byte[] Lba100000AudioSense             { get; set; }
    public string Lba100000AudioDecodedSense      { get; set; }
    public int    Lba100000AudioReadableCluster   { get; set; }
    public bool   Lba400000AudioReadable          { get; set; }
    public byte[] Lba400000AudioData              { get; set; }
    public byte[] Lba400000AudioSense             { get; set; }
    public string Lba400000AudioDecodedSense      { get; set; }
    public int    Lba400000AudioReadableCluster   { get; set; }
    public bool   Lba450000AudioReadable          { get; set; }
    public byte[] Lba450000AudioData              { get; set; }
    public byte[] Lba450000AudioSense             { get; set; }
    public string Lba450000AudioDecodedSense      { get; set; }
    public int    Lba450000AudioReadableCluster   { get; set; }
    public bool   Lba44990AudioPqReadable         { get; set; }
    public byte[] Lba44990AudioPqData             { get; set; }
    public byte[] Lba44990AudioPqSense            { get; set; }
    public string Lba44990AudioPqDecodedSense     { get; set; }
    public int    Lba44990AudioPqReadableCluster  { get; set; }
    public bool   Lba45000AudioPqReadable         { get; set; }
    public byte[] Lba45000AudioPqData             { get; set; }
    public byte[] Lba45000AudioPqSense            { get; set; }
    public string Lba45000AudioPqDecodedSense     { get; set; }
    public int    Lba45000AudioPqReadableCluster  { get; set; }
    public bool   Lba50000AudioPqReadable         { get; set; }
    public byte[] Lba50000AudioPqData             { get; set; }
    public byte[] Lba50000AudioPqSense            { get; set; }
    public string Lba50000AudioPqDecodedSense     { get; set; }
    public int    Lba50000AudioPqReadableCluster  { get; set; }
    public bool   Lba100000AudioPqReadable        { get; set; }
    public byte[] Lba100000AudioPqData            { get; set; }
    public byte[] Lba100000AudioPqSense           { get; set; }
    public string Lba100000AudioPqDecodedSense    { get; set; }
    public int    Lba100000AudioPqReadableCluster { get; set; }
    public bool   Lba400000AudioPqReadable        { get; set; }
    public byte[] Lba400000AudioPqData            { get; set; }
    public byte[] Lba400000AudioPqSense           { get; set; }
    public string Lba400000AudioPqDecodedSense    { get; set; }
    public int    Lba400000AudioPqReadableCluster { get; set; }
    public bool   Lba450000AudioPqReadable        { get; set; }
    public byte[] Lba450000AudioPqData            { get; set; }
    public byte[] Lba450000AudioPqSense           { get; set; }
    public string Lba450000AudioPqDecodedSense    { get; set; }
    public int    Lba450000AudioPqReadableCluster { get; set; }
    public bool   Lba44990AudioRwReadable         { get; set; }
    public byte[] Lba44990AudioRwData             { get; set; }
    public byte[] Lba44990AudioRwSense            { get; set; }
    public string Lba44990AudioRwDecodedSense     { get; set; }
    public int    Lba44990AudioRwReadableCluster  { get; set; }
    public bool   Lba45000AudioRwReadable         { get; set; }
    public byte[] Lba45000AudioRwData             { get; set; }
    public byte[] Lba45000AudioRwSense            { get; set; }
    public string Lba45000AudioRwDecodedSense     { get; set; }
    public int    Lba45000AudioRwReadableCluster  { get; set; }
    public bool   Lba50000AudioRwReadable         { get; set; }
    public byte[] Lba50000AudioRwData             { get; set; }
    public byte[] Lba50000AudioRwSense            { get; set; }
    public string Lba50000AudioRwDecodedSense     { get; set; }
    public int    Lba50000AudioRwReadableCluster  { get; set; }
    public bool   Lba100000AudioRwReadable        { get; set; }
    public byte[] Lba100000AudioRwData            { get; set; }
    public byte[] Lba100000AudioRwSense           { get; set; }
    public string Lba100000AudioRwDecodedSense    { get; set; }
    public int    Lba100000AudioRwReadableCluster { get; set; }
    public bool   Lba400000AudioRwReadable        { get; set; }
    public byte[] Lba400000AudioRwData            { get; set; }
    public byte[] Lba400000AudioRwSense           { get; set; }
    public string Lba400000AudioRwDecodedSense    { get; set; }
    public int    Lba400000AudioRwReadableCluster { get; set; }
    public bool   Lba450000AudioRwReadable        { get; set; }
    public byte[] Lba450000AudioRwData            { get; set; }
    public byte[] Lba450000AudioRwSense           { get; set; }
    public string Lba450000AudioRwDecodedSense    { get; set; }
    public int    Lba450000AudioRwReadableCluster { get; set; }
    public uint   MinimumReadableSectorInHdArea   { get; set; }
    public uint   MaximumReadableSectorInHdArea   { get; set; }
    public byte[] MaximumReadablePqInHdArea       { get; set; }
    public byte[] MaximumReadableRwInHdArea       { get; set; }
}

public class SupportedDensity
{
    [JsonIgnore]
    public int Id { get; set; }
    [DisplayName("Primary density code")]
    public byte PrimaryCode { get; set; }
    [DisplayName("Secondary density code")]
    public byte SecondaryCode { get; set; }
    public bool Writable  { get;     set; }
    public bool Duplicate { get;     set; }
    [DisplayName("Default density code")]
    public bool DefaultDensity { get; set; }
    [DisplayName("Bits per mm")]
    public uint BitsPerMm { get; set; }
    public ushort Width  { get;  set; }
    public ushort Tracks { get;  set; }
    [DisplayName("Nominal capacity (MiB)")]
    public uint Capacity { get;       set; }
    public string Organization { get; set; }
    public string Name         { get; set; }
    public string Description  { get; set; }
}