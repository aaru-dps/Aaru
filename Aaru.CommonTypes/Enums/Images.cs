// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Images.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines enumerations to be used by disc image plugins.
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
// Copyright © 2011-2026 Natalia Portillo
// Copyright © 2020-2026 Rebecca Wallander
// ****************************************************************************/

// ReSharper disable UnusedMember.Global

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Aaru.CommonTypes.Enums;

/// <summary>Track (as partitioning element) types.</summary>
public enum TrackType : byte
{
    /// <summary>Audio track</summary>
    Audio = 0,
    /// <summary>Data track (not any of the below defined ones)</summary>
    Data = 1,
    /// <summary>Data track, compact disc mode 1</summary>
    [Description("MODE 1")]
    CdMode1 = 2,
    /// <summary>Data track, compact disc mode 2, formless</summary>
    [Description("MODE 2 (Formless)")]
    CdMode2Formless = 3,
    /// <summary>Data track, compact disc mode 2, form 1</summary>
    [Description("MODE 2 FORM 1")]
    CdMode2Form1 = 4,
    /// <summary>Data track, compact disc mode 2, form 2</summary>
    [Description("MODE 2 FORM 2")]
    CdMode2Form2 = 5
}

/// <summary>Type of subchannel in track</summary>
public enum TrackSubchannelType : byte
{
    /// <summary>Track does not has subchannel dumped, or it's not a CD</summary>
    None = 0,
    /// <summary>Subchannel is packed and error corrected</summary>
    Packed = 1,
    /// <summary>Subchannel is interleaved</summary>
    Raw = 2,
    /// <summary>Subchannel is packed and comes interleaved with main channel in same file</summary>
    PackedInterleaved = 3,
    /// <summary>Subchannel is interleaved and comes interleaved with main channel in same file</summary>
    RawInterleaved = 4,
    /// <summary>Only Q subchannel is stored as 16 bytes</summary>
    Q16 = 5,
    /// <summary>Only Q subchannel is stored as 16 bytes and comes interleaved with main channel in same file</summary>
    Q16Interleaved = 6
}

/// <summary>Metadata present for each sector (aka, "tag").</summary>
public enum SectorTagType
{
    /// <summary>Apple's Sony GCR sector tags, 12 bytes</summary>
    [Description("Sony GCR sector tags")]
    AppleSonyTag = 0,
    /// <summary>Sync frame from CD sector, 12 bytes</summary>
    [Description("Sync frame")]
    CdSectorSync = 1,
    /// <summary>CD sector header, 4 bytes</summary>
    [Description("Sector header")]
    CdSectorHeader = 2,
    /// <summary>CD mode 2 sector subheader</summary>
    [Description("MODE 2 Subheader")]
    CdSectorSubHeader = 3,
    /// <summary>CD sector EDC, 4 bytes</summary>
    [Description("Error Detection Code")]
    CdSectorEdc = 4,
    /// <summary>CD sector ECC P, 172 bytes</summary>
    [Description("Error Correction Code P")]
    CdSectorEccP = 5,
    /// <summary>CD sector ECC Q, 104 bytes</summary>
    [Description("Error Correction Code Q")]
    CdSectorEccQ = 6,
    /// <summary>CD sector ECC (P and Q), 276 bytes</summary>
    [Description("Error Correction Code")]
    CdSectorEcc = 7,
    /// <summary>CD sector subchannel, 96 bytes</summary>
    [Description("Subchannel")]
    CdSectorSubchannel = 8,
    /// <summary>CD track ISRC, string, 12 bytes</summary>
    [Description("International Standard Recording Code")]
    CdTrackIsrc = 9,
    /// <summary>CD track text, string, 13 bytes</summary>
    [Description("Track text")]
    CdTrackText = 10,
    /// <summary>CD track flags, 1 byte</summary>
    [Description("Track flags")]
    CdTrackFlags = 11,
    /// <summary>DVD sector copyright information</summary>
    [Description("Copyright Management Information")]
    DvdSectorCmi = 12,
    /// <summary>Floppy address mark (contents depend on underlying floppy format)</summary>
    [Description("Address mark")]
    FloppyAddressMark = 13,
    /// <summary>DVD sector title key, 5 bytes</summary>
    [Description("Title key")]
    DvdSectorTitleKey = 14,
    /// <summary>Decrypted DVD sector title key, 5 bytes</summary>
    [Description("Title key (Decrypted)")]
    DvdTitleKeyDecrypted = 15,
    /// <summary>DVD sector information, 1 bytes</summary>
    [Description("Sector information")]
    DvdSectorInformation = 16,
    /// <summary>DVD sector number, 3 bytes</summary>
    [Description("Sector number")]
    DvdSectorNumber = 17,
    /// <summary>DVD sector ID error detection, 2 bytes</summary>
    [Description("ID error detection")]
    DvdSectorIed = 18,
    /// <summary>DVD sector EDC, 4 bytes</summary>
    [Description("Error Detection Code")]
    DvdSectorEdc = 19,
    /// <summary>Apple's Profile sector tag, 20 bytes</summary>
    [Description("Profile sector tag")]
    AppleProfileTag = 20,
    /// <summary>Priam Data Tower sector tag, 24 bytes</summary>
    [Description("Priam Data Tower sector tag")]
    PriamDataTowerTag = 21,
    /// <summary>Blu-ray sector EDC, 4 bytes</summary>
    [Description("Blu-ray sector EDC")]
    BluRaySectorEdc = 22
}

/// <summary>Metadata present for each media.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum MediaTagType
{
    /// <summary>CD table of contents</summary>
    [Description("Table of contents")]
    CD_TOC = 0,
    /// <summary>CD session information</summary>
    [Description("Session information")]
    CD_SessionInfo = 1,
    /// <summary>CD full table of contents</summary>
    [Description("Full table of contents")]
    CD_FullTOC = 2,
    /// <summary>CD PMA</summary>
    [Description("Program Memory Area")]
    CD_PMA = 3,
    /// <summary>CD Address-Time-In-Pregroove</summary>
    [Description("Address-Time-In-Pregroove")]
    CD_ATIP = 4,
    /// <summary>CD-Text</summary>
    [Description("CD-Text")]
    CD_TEXT = 5,
    /// <summary>CD Media Catalogue Number</summary>
    [Description("Media Catalogue Number")]
    CD_MCN = 6,
    /// <summary>DVD/HD DVD Physical Format Information</summary>
    [Description("Physical Format Information")]
    DVD_PFI = 7,
    /// <summary>DVD Lead-in Copyright Management Information</summary>
    [Description("Lead-in Copyright Management Information")]
    DVD_CMI = 8,
    /// <summary>DVD disc key</summary>
    [Description("Disc key")]
    DVD_DiscKey = 9,
    /// <summary>DVD/HD DVD Burst Cutting Area</summary>
    [Description("Burst Cutting Area")]
    DVD_BCA = 10,
    /// <summary>DVD/HD DVD Lead-in Disc Manufacturer Information</summary>
    [Description("Lead-in Disc Manufacturer Information")]
    DVD_DMI = 11,
    /// <summary>Media identifier</summary>
    [Description("Media identifier")]
    DVD_MediaIdentifier = 12,
    /// <summary>Media key block</summary>
    [Description("Media key block")]
    DVD_MKB = 13,
    /// <summary>DVD-RAM/HD DVD-RAM DDS information</summary>
    [Description("DDS information")]
    DVDRAM_DDS = 14,
    /// <summary>DVD-RAM/HD DVD-RAM Medium status</summary>
    [Description("Medium status")]
    DVDRAM_MediumStatus = 15,
    /// <summary>DVD-RAM/HD DVD-RAM Spare area information</summary>
    [Description("Spare area information")]
    DVDRAM_SpareArea = 16,
    /// <summary>DVD-R/-RW/HD DVD-R RMD in last border-out</summary>
    [Description("RMD in last border-out")]
    DVDR_RMD = 17,
    /// <summary>Pre-recorded information from DVD-R/-RW lead-in</summary>
    [Description("Pre-recorded information from lead-in")]
    DVDR_PreRecordedInfo = 18,
    /// <summary>DVD-R/-RW/HD DVD-R media identifier</summary>
    [Description("Media identifier")]
    DVDR_MediaIdentifier = 19,
    /// <summary>DVD-R/-RW/HD DVD-R physical format information</summary>
    [Description("Physical format information")]
    DVDR_PFI = 20,
    /// <summary>ADIP information</summary>
    [Description("ADIP information")]
    DVD_ADIP = 21,
    /// <summary>HD DVD Lead-in copyright protection information</summary>
    [Description("Lead-in copyright protection information")]
    HDDVD_CPI = 22,
    /// <summary>HD DVD-R Medium Status</summary>
    [Description("Medium Status")]
    HDDVD_MediumStatus = 23,
    /// <summary>DVD+/-R DL Layer capacity</summary>
    [Description("Layer capacity")]
    DVDDL_LayerCapacity = 24,
    /// <summary>DVD-R DL Middle Zone start address</summary>
    [Description("Middle Zone start address")]
    DVDDL_MiddleZoneAddress = 25,
    /// <summary>DVD-R DL Jump Interval Size</summary>
    [Description("Jump Interval Size")]
    DVDDL_JumpIntervalSize = 26,
    /// <summary>DVD-R DL Start LBA of the manual layer jump</summary>
    [Description("Start LBA of the manual layer jump")]
    DVDDL_ManualLayerJumpLBA = 27,
    /// <summary>Blu-ray Disc Information</summary>
    [Description("Disc Information")]
    BD_DI = 28,
    /// <summary>Blu-ray Burst Cutting Area</summary>
    [Description("Burst Cutting Area")]
    BD_BCA = 29,
    /// <summary>Blu-ray Disc Definition Structure</summary>
    [Description("Disc Definition Structure")]
    BD_DDS = 30,
    /// <summary>Blu-ray Cartridge Status</summary>
    [Description("Cartridge Status")]
    BD_CartridgeStatus = 31,
    /// <summary>Blu-ray Status of Spare Area</summary>
    [Description("Status of Spare Area")]
    BD_SpareArea = 32,
    /// <summary>AACS volume identifier</summary>
    [Description("Volume identifier")]
    AACS_VolumeIdentifier = 33,
    /// <summary>AACS pre-recorded media serial number</summary>
    [Description("Pre-recorded media serial number")]
    AACS_SerialNumber = 34,
    /// <summary>AACS media identifier</summary>
    [Description("Media identifier")]
    AACS_MediaIdentifier = 35,
    /// <summary>Lead-in AACS media key block</summary>
    [Description("Lead-in media key block")]
    AACS_MKB = 36,
    /// <summary>AACS data keys</summary>
    [Description("AACS Data keys")]
    AACS_DataKeys = 37,
    /// <summary>LBA extents flagged for bus encryption by AACS</summary>
    [Description("LBA extents flagged for bus encryption")]
    AACS_LBAExtents = 38,
    /// <summary>CPRM media key block in Lead-in</summary>
    [Description("CPRM media key block in Lead-in")]
    AACS_CPRM_MKB = 39,
    /// <summary>Recognized layer formats in hybrid discs</summary>
    [Description("Recognized layer formats in hybrid discs")]
    Hybrid_RecognizedLayers = 40,
    /// <summary>Disc write protection status</summary>
    [Description("Write protection status")]
    MMC_WriteProtection = 41,
    /// <summary>Disc standard information</summary>
    [Description("Standard information")]
    MMC_DiscInformation = 42,
    /// <summary>Disc track resources information</summary>
    [Description("Track resources information")]
    MMC_TrackResourcesInformation = 43,
    /// <summary>BD-R Pseudo-overwrite information</summary>
    [Description("Pseudo-overwrite information")]
    MMC_POWResourcesInformation = 44,
    /// <summary>SCSI INQUIRY response</summary>
    [Description("INQUIRY response")]
    SCSI_INQUIRY = 45,
    /// <summary>SCSI MODE PAGE 2Ah</summary>
    [Description("MODE PAGE 2Ah")]
    SCSI_MODEPAGE_2A = 46,
    /// <summary>ATA IDENTIFY DEVICE response</summary>
    [Description("IDENTIFY DEVICE response")]
    ATA_IDENTIFY = 47,
    /// <summary>ATA IDENTIFY PACKET DEVICE response</summary>
    [Description("IDENTIFY PACKET DEVICE response")]
    ATAPI_IDENTIFY = 48,
    /// <summary>PCMCIA/CardBus Card Information Structure</summary>
    [Description("Card Information Structure")]
    PCMCIA_CIS = 49,
    /// <summary>SecureDigital CID</summary>
    [Description("SD Card Information Data")]
    SD_CID = 50,
    /// <summary>SecureDigital CSD</summary>
    [Description("SD Card Specific Data")]
    SD_CSD = 51,
    /// <summary>SecureDigital SCR</summary>
    [Description("SD Specific Conditions Register")]
    SD_SCR = 52,
    /// <summary>SecureDigital OCR</summary>
    [Description("Operating Conditions Register")]
    SD_OCR = 53,
    /// <summary>MultiMediaCard CID</summary>
    [Description("MMC Card Information Data")]
    MMC_CID = 54,
    /// <summary>MultiMediaCard CSD</summary>
    [Description("MMC Card Specific Data")]
    MMC_CSD = 55,
    /// <summary>MultiMediaCard OCR</summary>
    [Description("MMC Operating Conditions Register")]
    MMC_OCR = 56,
    /// <summary>MultiMediaCard Extended CSD</summary>
    [Description("MMC Extended CSD")]
    MMC_ExtendedCSD = 57,
    /// <summary>Xbox Security Sector</summary>
    [Description("Xbox Security Sector")]
    Xbox_SecuritySector = 58,
    /// <summary>
    ///     On floppy disks, data in last cylinder usually in a different format that contains duplication or
    ///     manufacturing information
    /// </summary>
    [Description("Lead-Out")]
    Floppy_LeadOut = 59,
    /// <summary>DVD Disc Control Blocks</summary>
    [Description("Disc Control Blocks")]
    DCB = 60,
    /// <summary>Compact Disc First Track Pregap</summary>
    [Description("First Track Pregap")]
    CD_FirstTrackPregap = 61,
    /// <summary>Compact Disc Lead-out</summary>
    [Description("Lead-out")]
    CD_LeadOut = 62,
    /// <summary>SCSI MODE SENSE (6)</summary>
    [Description("MODE SENSE (6)")]
    SCSI_MODESENSE_6 = 63,
    /// <summary>SCSI MODE SENSE (10)</summary>
    [Description("MODE SENSE (10)")]
    SCSI_MODESENSE_10 = 64,
    /// <summary>USB descriptors</summary>
    [Description("USB descriptors")]
    USB_Descriptors = 65,
    /// <summary>XGD unlocked DMI</summary>
    [Description("Unlocked DMI")]
    Xbox_DMI = 66,
    /// <summary>XDG unlocked PFI</summary>
    [Description("Unlocked PFI")]
    Xbox_PFI = 67,
    /// <summary>Compact Disc Lead-in</summary>
    [Description("Lead-in")]
    CD_LeadIn = 68,
    /// <summary>8 bytes response that seems to define type of MiniDisc</summary>
    [Description("MiniDisc type definition")]
    MiniDiscType = 69,
    /// <summary>4 bytes response to vendor command D5h</summary>
    [Description("Vendor command D5h response")]
    MiniDiscD5 = 70,
    /// <summary>User TOC, contains fragments, track names, and can be from 1 to 3 sectors of 2336 bytes</summary>
    [Description("MD-DATA User TOC")]
    MiniDiscUTOC = 71,
    /// <summary>Not entirely clear kind of TOC that only appears on MD-DATA discs</summary>
    [Description("MD-DATA DTOC")]
    MiniDiscDTOC = 72,
    /// <summary>Decrypted DVD disc key</summary>
    [Description("Disc key (Decrypted)")]
    DVD_DiscKey_Decrypted = 73,
    /// <summary>Physical Format Information for the 2nd layer of dual-layer DVDs</summary>
    [Description("Physical Format Information (2nd Layer)")]
    DVD_PFI_2ndLayer = 74,
    /// <summary>Floppy write protection status</summary>
    [Description("Write protection status")]
    Floppy_WriteProtection = 75,
    /// <summary>Nintendo Wii U Disc Key</summary>
    [Description("Nintendo Wii U Disc Key")]
    WiiUDiscKey = 76,
    /// <summary>PS3 derived disc key (16 bytes)</summary>
    [Description("PS3 derived disc key")]
    PS3_DiscKey = 77,
    /// <summary>PS3 data 1 key (16 bytes, from disc)</summary>
    [Description("PS3 data1 key")]
    PS3_Data1 = 78,
    /// <summary>PS3 data 2 key (16 bytes, from disc)</summary>
    [Description("PS3 data2 key")]
    PS3_Data2 = 79,
    /// <summary>PS3 PIC data (115 bytes, from disc lead-in)</summary>
    [Description("PS3 PIC data")]
    PS3_PIC = 80,
    /// <summary>PS3 encryption region map (serialized from sector 0)</summary>
    [Description("PS3 encryption region map")]
    PS3_EncryptionMap = 81,
    ///<summary>Nintendo Wii U partition-to-key mapping with regions</summary>
    [Description("Nintendo Wii U partition-to-key mapping with regions")]
    WiiUPartitionKeyMap = 82,
    ///<summary>Nintendo Wii partition-to-key mapping with regions</summary>
    [Description("Nintendo Wii partition-to-key mapping with regions")]
    WiiPartitionKeyMap = 83,
    ///<summary>Nintendo GameCube/Wii junk region map with LFG seeds</summary>
    [Description("Nintendo GameCube/Wii junk region map with LFG seeds")]
    NgcwJunkMap = 84,
    /// <summary>AACS Media Key</summary>
    [Description("AACS Media Key")]
    AacsMediaKey = 85,
    /// <summary>AACS Volume Unique Key</summary>
    [Description("AACS Volume Unique Key")]
    AacsVolumeUniqueKey = 86
}

/// <summary>Enumeration of media types defined in metadata</summary>
public enum MetadataMediaType : byte
{
    /// <summary>Purely optical discs</summary>
    OpticalDisc = 0,
    /// <summary>Media that is physically block-based or abstracted like that</summary>
    BlockMedia = 1,
    /// <summary>Media that can be accessed by-byte or by-bit, like chips</summary>
    LinearMedia = 2,
    /// <summary>Media that can only store data when it is modulated to audio</summary>
    AudioMedia = 3
}

/// <summary> CD flags bitmask</summary>
[Flags]
public enum CdFlags : byte
{
    /// <summary>Track is quadraphonic.</summary>
    FourChannel = 0x08,
    /// <summary>Track is non-audio (data).</summary>
    DataTrack = 0x04,
    /// <summary>Track is copy protected.</summary>
    CopyPermitted = 0x02,
    /// <summary>Track has pre-emphasis.</summary>
    PreEmphasis = 0x01
}

/// <summary>Status of a requested floppy sector</summary>
[Flags]
public enum FloppySectorStatus : byte
{
    /// <summary>Both address mark and data checksums are correct.</summary>
    Correct = 0x01,
    /// <summary>Data checksum is incorrect.</summary>
    DataError = 0x02,
    /// <summary>Address mark checksum is incorrect.</summary>
    AddressMarkError = 0x04,
    /// <summary>There is another sector in the same track/head with same sector id.</summary>
    Duplicated = 0x08,
    /// <summary>Sector data section is not magnetized.</summary>
    Demagnetized = 0x10,
    /// <summary>Sector data section has a physically visible hole.</summary>
    Hole = 0x20,
    /// <summary>There is no address mark containing the requested sector id in the track/head.</summary>
    NotFound = 0x40
}

/// <summary>Types of floppy disks</summary>
public enum FloppyTypes : byte
{
    /// <summary>8" floppy</summary>
    Floppy,
    /// <summary>5.25" floppy</summary>
    MiniFloppy,
    /// <summary>3.5" floppy</summary>
    MicroFloppy,
    /// <summary>3" floppy</summary>
    CompactFloppy,
    /// <summary>5.25" twiggy</summary>
    FileWare,
    /// <summary>2.5" quickdisk</summary>
    QuickDisk
}

/// <summary>Enumeration of floppy densities</summary>
public enum FloppyDensities : byte
{
    /// <summary>Standard coercivity (about 300Oe as found in 8" and 5.25"-double-density disks).</summary>
    Standard,
    /// <summary>Double density coercivity (about 600Oe as found in 5.25" HD and 3.5" DD disks).</summary>
    Double,
    /// <summary>High density coercivity (about 700Oe as found in 3.5" HD disks).</summary>
    High,
    /// <summary>Extended density coercivity (about 750Oe as found in 3.5" ED disks).</summary>
    Extended
}

/// <summary>Capabilities for optical media image formats</summary>
[Flags]
public enum OpticalImageCapabilities : ulong
{
    /// <summary>Can store Red Book audio tracks?</summary>
    CanStoreAudioTracks = 0x01,
    /// <summary>Can store CD-V analogue video tracks?</summary>
    CanStoreVideoTracks = 0x02,
    /// <summary>Can store Yellow Book data tracks?</summary>
    CanStoreDataTracks = 0x04,
    /// <summary>Can store pregaps without needing to interpret the subchannel?</summary>
    CanStorePregaps = 0x08,
    /// <summary>Can store indexes without needing to interpret the subchannel?</summary>
    CanStoreIndexes = 0x10,
    /// <summary>Can store raw P to W subchannel data?</summary>
    CanStoreSubchannelRw = 0x20,
    /// <summary>Can store more than one session?</summary>
    CanStoreSessions = 0x40,
    /// <summary>Can store track ISRCs without needing to interpret the subchannel?</summary>
    CanStoreIsrc = 0x80,
    /// <summary>Can store Lead-In's CD-TEXT?</summary>
    CanStoreCdText = 0x100,
    /// <summary>Can store the MCN without needing to interpret the subchannel?</summary>
    CanStoreMcn = 0x200,
    /// <summary>Can store the whole 2352 bytes of a sector?</summary>
    CanStoreRawData = 0x400,

    // TODO: Implement
    /// <summary>Can store scrambled data?</summary>
    CanStoreScrambledData = 0x800,
    /// <summary>Can store only the user area of a sector (2048, 2324, etc)?</summary>
    CanStoreCookedData = 0x1000,
    /// <summary>Can store more than 1 track?</summary>
    CanStoreMultipleTracks = 0x2000,
    /// <summary>Can store more than 1 session in media that is not CD based (DVD et al)?</summary>
    CanStoreNotCdSessions = 0x4000,
    /// <summary>Can store more than 1 track in media that is not CD based (DVD et al)?</summary>
    CanStoreNotCdTracks = 0x8000,
    /// <summary>Can store hidden tracks with a type different from track 1?</summary>
    CanStoreHiddenTracks = 0x10000,
    /// <summary>Can store negative sectors (sectors before LBA 0)?</summary>
    CanStoreNegativeSectors = 0x20000,
    /// <summary>Can store overflow sectors (sectors after media reported size)?</summary>
    CanStoreOverflowSectors = 0x40000
}

/// <summary>Enumeration of linear memory device types</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum LinearMemoryType
{
    /// <summary>Unknown device type</summary>
    Unknown = 0,
    /// <summary>Read-only memory</summary>
    ROM = 1,
    /// <summary>Read-write memory, power-off persistent, used to save data</summary>
    SaveRAM = 2,
    /// <summary>Read-write volatile memory</summary>
    WorkRAM = 3,
    /// <summary>NOR flash memory</summary>
    NOR = 4,
    /// <summary>NAND flash memory</summary>
    NAND = 5,
    /// <summary>Memory mapper device</summary>
    Mapper = 6,
    /// <summary>Processor, CPU, DSP, etc</summary>
    Processor = 7,
    /// <summary>Programmable Array Logic</summary>
    PAL = 8,
    /// <summary>Generic Array Logic</summary>
    GAL = 9,
    /// <summary>Electronically Erasable Programmable Read Only Memory</summary>
    EEPROM = 10,
    /// <summary>Read-only memory, character</summary>
    CharacterROM = 11,
    /// <summary>Read-write volatile memory for character</summary>
    CharacterRAM = 12,
    /// <summary>Trainer, or hack</summary>
    Trainer = 13
}