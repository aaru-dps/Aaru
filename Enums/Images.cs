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
// Copyright © 2011-2023 Natalia Portillo
// Copyright © 2020-2023 Rebecca Wallander
// ****************************************************************************/

// ReSharper disable UnusedMember.Global

using System;
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
    CdMode1 = 2,
    /// <summary>Data track, compact disc mode 2, formless</summary>
    CdMode2Formless = 3,
    /// <summary>Data track, compact disc mode 2, form 1</summary>
    CdMode2Form1 = 4,
    /// <summary>Data track, compact disc mode 2, form 2</summary>
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
    /// <summary>Apple's GCR sector tags, 12 bytes</summary>
    AppleSectorTag = 0,
    /// <summary>Sync frame from CD sector, 12 bytes</summary>
    CdSectorSync = 1,
    /// <summary>CD sector header, 4 bytes</summary>
    CdSectorHeader = 2,
    /// <summary>CD mode 2 sector subheader</summary>
    CdSectorSubHeader = 3,
    /// <summary>CD sector EDC, 4 bytes</summary>
    CdSectorEdc = 4,
    /// <summary>CD sector ECC P, 172 bytes</summary>
    CdSectorEccP = 5,
    /// <summary>CD sector ECC Q, 104 bytes</summary>
    CdSectorEccQ = 6,
    /// <summary>CD sector ECC (P and Q), 276 bytes</summary>
    CdSectorEcc = 7,
    /// <summary>CD sector subchannel, 96 bytes</summary>
    CdSectorSubchannel = 8,
    /// <summary>CD track ISRC, string, 12 bytes</summary>
    CdTrackIsrc = 9,
    /// <summary>CD track text, string, 13 bytes</summary>
    CdTrackText = 10,
    /// <summary>CD track flags, 1 byte</summary>
    CdTrackFlags = 11,
    /// <summary>DVD sector copyright information</summary>
    DvdCmi = 12,
    /// <summary>Floppy address mark (contents depend on underlying floppy format)</summary>
    FloppyAddressMark = 13,
    /// <summary>DVD sector title key, 5 bytes</summary>
    DvdTitleKey = 14,
    /// <summary>Decrypted DVD sector title key, 5 bytes</summary>
    DvdTitleKeyDecrypted = 15
}

/// <summary>Metadata present for each media.</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum MediaTagType
{
    /// <summary>CD table of contents</summary>
    CD_TOC = 0,
    /// <summary>CD session information</summary>
    CD_SessionInfo = 1,
    /// <summary>CD full table of contents</summary>
    CD_FullTOC = 2,
    /// <summary>CD PMA</summary>
    CD_PMA = 3,
    /// <summary>CD Address-Time-In-Pregroove</summary>
    CD_ATIP = 4,
    /// <summary>CD-Text</summary>
    CD_TEXT = 5,
    /// <summary>CD Media Catalogue Number</summary>
    CD_MCN = 6,
    /// <summary>DVD/HD DVD Physical Format Information</summary>
    DVD_PFI = 7,
    /// <summary>DVD Lead-in Copyright Management Information</summary>
    DVD_CMI = 8,
    /// <summary>DVD disc key</summary>
    DVD_DiscKey = 9,
    /// <summary>DVD/HD DVD Burst Cutting Area</summary>
    DVD_BCA = 10,
    /// <summary>DVD/HD DVD Lead-in Disc Manufacturer Information</summary>
    DVD_DMI = 11,
    /// <summary>Media identifier</summary>
    DVD_MediaIdentifier = 12,
    /// <summary>Media key block</summary>
    DVD_MKB = 13,
    /// <summary>DVD-RAM/HD DVD-RAM DDS information</summary>
    DVDRAM_DDS = 14,
    /// <summary>DVD-RAM/HD DVD-RAM Medium status</summary>
    DVDRAM_MediumStatus = 15,
    /// <summary>DVD-RAM/HD DVD-RAM Spare area information</summary>
    DVDRAM_SpareArea = 16,
    /// <summary>DVD-R/-RW/HD DVD-R RMD in last border-out</summary>
    DVDR_RMD = 17,
    /// <summary>Pre-recorded information from DVD-R/-RW lead-in</summary>
    DVDR_PreRecordedInfo = 18,
    /// <summary>DVD-R/-RW/HD DVD-R media identifier</summary>
    DVDR_MediaIdentifier = 19,
    /// <summary>DVD-R/-RW/HD DVD-R physical format information</summary>
    DVDR_PFI = 20,
    /// <summary>ADIP information</summary>
    DVD_ADIP = 21,
    /// <summary>HD DVD Lead-in copyright protection information</summary>
    HDDVD_CPI = 22,
    /// <summary>HD DVD-R Medium Status</summary>
    HDDVD_MediumStatus = 23,
    /// <summary>DVD+/-R DL Layer capacity</summary>
    DVDDL_LayerCapacity = 24,
    /// <summary>DVD-R DL Middle Zone start address</summary>
    DVDDL_MiddleZoneAddress = 25,
    /// <summary>DVD-R DL Jump Interval Size</summary>
    DVDDL_JumpIntervalSize = 26,
    /// <summary>DVD-R DL Start LBA of the manual layer jump</summary>
    DVDDL_ManualLayerJumpLBA = 27,
    /// <summary>Blu-ray Disc Information</summary>
    BD_DI = 28,
    /// <summary>Blu-ray Burst Cutting Area</summary>
    BD_BCA = 29,
    /// <summary>Blu-ray Disc Definition Structure</summary>
    BD_DDS = 30,
    /// <summary>Blu-ray Cartridge Status</summary>
    BD_CartridgeStatus = 31,
    /// <summary>Blu-ray Status of Spare Area</summary>
    BD_SpareArea = 32,
    /// <summary>AACS volume identifier</summary>
    AACS_VolumeIdentifier = 33,
    /// <summary>AACS pre-recorded media serial number</summary>
    AACS_SerialNumber = 34,
    /// <summary>AACS media identifier</summary>
    AACS_MediaIdentifier = 35,
    /// <summary>Lead-in AACS media key block</summary>
    AACS_MKB = 36,
    /// <summary>AACS data keys</summary>
    AACS_DataKeys = 37,
    /// <summary>LBA extents flagged for bus encryption by AACS</summary>
    AACS_LBAExtents = 38,
    /// <summary>CPRM media key block in Lead-in</summary>
    AACS_CPRM_MKB = 39,
    /// <summary>Recognized layer formats in hybrid discs</summary>
    Hybrid_RecognizedLayers = 40,
    /// <summary>Disc write protection status</summary>
    MMC_WriteProtection = 41,
    /// <summary>Disc standard information</summary>
    MMC_DiscInformation = 42,
    /// <summary>Disc track resources information</summary>
    MMC_TrackResourcesInformation = 43,
    /// <summary>BD-R Pseudo-overwrite information</summary>
    MMC_POWResourcesInformation = 44,
    /// <summary>SCSI INQUIRY response</summary>
    SCSI_INQUIRY = 45,
    /// <summary>SCSI MODE PAGE 2Ah</summary>
    SCSI_MODEPAGE_2A = 46,
    /// <summary>ATA IDENTIFY DEVICE response</summary>
    ATA_IDENTIFY = 47,
    /// <summary>ATA IDENTIFY PACKET DEVICE response</summary>
    ATAPI_IDENTIFY = 48,
    /// <summary>PCMCIA/CardBus Card Information Structure</summary>
    PCMCIA_CIS = 49,
    /// <summary>SecureDigital CID</summary>
    SD_CID = 50,
    /// <summary>SecureDigital CSD</summary>
    SD_CSD = 51,
    /// <summary>SecureDigital SCR</summary>
    SD_SCR = 52,
    /// <summary>SecureDigital OCR</summary>
    SD_OCR = 53,
    /// <summary>MultiMediaCard CID</summary>
    MMC_CID = 54,
    /// <summary>MultiMediaCard CSD</summary>
    MMC_CSD = 55,
    /// <summary>MultiMediaCard OCR</summary>
    MMC_OCR = 56,
    /// <summary>MultiMediaCard Extended CSD</summary>
    MMC_ExtendedCSD = 57,
    /// <summary>Xbox Security Sector</summary>
    Xbox_SecuritySector = 58,
    /// <summary>
    ///     On floppy disks, data in last cylinder usually in a different format that contains duplication or
    ///     manufacturing information
    /// </summary>
    Floppy_LeadOut = 59,
    /// <summary>DVD Disc Control Blocks</summary>
    DCB = 60,
    /// <summary>Compact Disc First Track Pregap</summary>
    CD_FirstTrackPregap = 61,
    /// <summary>Compact Disc Lead-out</summary>
    CD_LeadOut = 62,
    /// <summary>SCSI MODE SENSE (6)</summary>
    SCSI_MODESENSE_6 = 63,
    /// <summary>SCSI MODE SENSE (10)</summary>
    SCSI_MODESENSE_10 = 64,
    /// <summary>USB descriptors</summary>
    USB_Descriptors = 65,
    /// <summary>XGD unlocked DMI</summary>
    Xbox_DMI = 66,
    /// <summary>XDG unlocked PFI</summary>
    Xbox_PFI = 67,
    /// <summary>Compact Disc Lead-in</summary>
    CD_LeadIn = 68,
    /// <summary>8 bytes response that seems to define type of MiniDisc</summary>
    MiniDiscType = 69,
    /// <summary>4 bytes response to vendor command D5h</summary>
    MiniDiscD5 = 70,
    /// <summary>User TOC, contains fragments, track names, and can be from 1 to 3 sectors of 2336 bytes</summary>
    MiniDiscUTOC = 71,
    /// <summary>Not entirely clear kind of TOC that only appears on MD-DATA discs</summary>
    MiniDiscDTOC = 72,
    /// <summary>Decrypted DVD disc key</summary>
    DVD_DiscKey_Decrypted = 73
}

/// <summary>Enumeration of media types defined in CICM metadata</summary>
public enum XmlMediaType : byte
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
    CanStoreDataTracks = 0x03,
    /// <summary>Can store pregaps without needing to interpret the subchannel?</summary>
    CanStorePregaps = 0x04,
    /// <summary>Can store indexes without needing to interpret the subchannel?</summary>
    CanStoreIndexes = 0x08,
    /// <summary>Can store raw P to W subchannel data?</summary>
    CanStoreSubchannelRw = 0x10,
    /// <summary>Can store more than one session?</summary>
    CanStoreSessions = 0x20,
    /// <summary>Can store track ISRCs without needing to interpret the subchannel?</summary>
    CanStoreIsrc = 0x40,
    /// <summary>Can store Lead-In's CD-TEXT?</summary>
    CanStoreCdText = 0x80,
    /// <summary>Can store the MCN without needing to interpret the subchannel?</summary>
    CanStoreMcn = 0x100,
    /// <summary>Can store the whole 2352 bytes of a sector?</summary>
    CanStoreRawData = 0x200,
    /// <summary>Can store more than 1 session in media that is not CD based (DVD et al)?</summary>
    CanStoreNotCdSessions = 0x2000,
    /// <summary>Can store more than 1 track in media that is not CD based (DVD et al)?</summary>
    CanStoreNotCdTracks = 0x4000,
    /// <summary>Can store hidden tracks with a type different from track 1?</summary>
    CanStoreHiddenTracks = 0x8000,

    // TODO: Implement
    /// <summary>Can store scrambled data?</summary>
    CanStoreScrambledData = 0x400,
    /// <summary>Can store only the user area of a sector (2048, 2324, etc)?</summary>
    CanStoreCookedData = 0x800,
    /// <summary>Can store more than 1 track?</summary>
    CanStoreMultipleTracks = 0x1000
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