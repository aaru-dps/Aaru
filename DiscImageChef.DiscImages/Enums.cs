// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
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

namespace DiscImageChef.DiscImages
{
    /// <summary>
    ///     Track (as partitioning element) types.
    /// </summary>
    public enum TrackType
    {
        /// <summary>Audio track</summary>
        Audio,
        /// <summary>Data track (not any of the below defined ones)</summary>
        Data,
        /// <summary>Data track, compact disc mode 1</summary>
        CdMode1,
        /// <summary>Data track, compact disc mode 2, formless</summary>
        CdMode2Formless,
        /// <summary>Data track, compact disc mode 2, form 1</summary>
        CdMode2Form1,
        /// <summary>Data track, compact disc mode 2, form 2</summary>
        CdMode2Form2
    }

    /// <summary>
    ///     Type of subchannel in track
    /// </summary>
    public enum TrackSubchannelType
    {
        /// <summary>
        ///     Track does not has subchannel dumped, or it's not a CD
        /// </summary>
        None,
        /// <summary>
        ///     Subchannel is packed and error corrected
        /// </summary>
        Packed,
        /// <summary>
        ///     Subchannel is interleaved
        /// </summary>
        Raw,
        /// <summary>
        ///     Subchannel is packed and comes interleaved with main channel in same file
        /// </summary>
        PackedInterleaved,
        /// <summary>
        ///     Subchannel is interleaved and comes interleaved with main channel in same file
        /// </summary>
        RawInterleaved,
        /// <summary>
        ///     Only Q subchannel is stored as 16 bytes
        /// </summary>
        Q16,
        /// <summary>
        ///     Only Q subchannel is stored as 16 bytes and comes interleaved with main channel in same file
        /// </summary>
        Q16Interleaved
    }

    /// <summary>
    ///     Metadata present for each sector (aka, "tag").
    /// </summary>
    public enum SectorTagType
    {
        /// <summary>Apple's GCR sector tags, 12 bytes</summary>
        AppleSectorTag,
        /// <summary>Sync frame from CD sector, 12 bytes</summary>
        CdSectorSync,
        /// <summary>CD sector header, 4 bytes</summary>
        CdSectorHeader,
        /// <summary>CD mode 2 sector subheader</summary>
        CdSectorSubHeader,
        /// <summary>CD sector EDC, 4 bytes</summary>
        CdSectorEdc,
        /// <summary>CD sector ECC P, 172 bytes</summary>
        CdSectorEccP,
        /// <summary>CD sector ECC Q, 104 bytes</summary>
        CdSectorEccQ,
        /// <summary>CD sector ECC (P and Q), 276 bytes</summary>
        CdSectorEcc,
        /// <summary>CD sector subchannel, 96 bytes</summary>
        CdSectorSubchannel,
        /// <summary>CD track ISRC, string, 12 bytes</summary>
        CdTrackIsrc,
        /// <summary>CD track text, string, 13 bytes</summary>
        CdTrackText,
        /// <summary>CD track flags, 1 byte</summary>
        CdTrackFlags,
        /// <summary>DVD sector copyright information</summary>
        DvdCmi,
        /// <summary>Floppy address mark (contents depend on underlying floppy format)</summary>
        FloppyAddressMark
    }

    /// <summary>
    ///     Metadata present for each media.
    /// </summary>
    public enum MediaTagType
    {
        /// <summary>CD table of contents</summary>
        CD_TOC,
        /// <summary>CD session information</summary>
        CD_SessionInfo,
        /// <summary>CD full table of contents</summary>
        CD_FullTOC,
        /// <summary>CD PMA</summary>
        CD_PMA,
        /// <summary>CD Adress-Time-In-Pregroove</summary>
        CD_ATIP,
        /// <summary>CD-Text</summary>
        CD_TEXT,
        /// <summary>CD Media Catalogue Number</summary>
        CD_MCN,
        /// <summary>DVD/HD DVD Physical Format Information</summary>
        DVD_PFI,
        /// <summary>DVD Lead-in Copyright Management Information</summary>
        DVD_CMI,
        /// <summary>DVD disc key</summary>
        DVD_DiscKey,
        /// <summary>DVD/HD DVD Burst Cutting Area</summary>
        DVD_BCA,
        /// <summary>DVD/HD DVD Lead-in Disc Manufacturer Information</summary>
        DVD_DMI,
        /// <summary>Media identifier</summary>
        DVD_MediaIdentifier,
        /// <summary>Media key block</summary>
        DVD_MKB,
        /// <summary>DVD-RAM/HD DVD-RAM DDS information</summary>
        DVDRAM_DDS,
        /// <summary>DVD-RAM/HD DVD-RAM Medium status</summary>
        DVDRAM_MediumStatus,
        /// <summary>DVD-RAM/HD DVD-RAM Spare area information</summary>
        DVDRAM_SpareArea,
        /// <summary>DVD-R/-RW/HD DVD-R RMD in last border-out</summary>
        DVDR_RMD,
        /// <summary>Pre-recorded information from DVD-R/-RW lead-in</summary>
        DVDR_PreRecordedInfo,
        /// <summary>DVD-R/-RW/HD DVD-R media identifier</summary>
        DVDR_MediaIdentifier,
        /// <summary>DVD-R/-RW/HD DVD-R physical format information</summary>
        DVDR_PFI,
        /// <summary>ADIP information</summary>
        DVD_ADIP,
        /// <summary>HD DVD Lead-in copyright protection information</summary>
        HDDVD_CPI,
        /// <summary>HD DVD-R Medium Status</summary>
        HDDVD_MediumStatus,
        /// <summary>DVD+/-R DL Layer capacity</summary>
        DVDDL_LayerCapacity,
        /// <summary>DVD-R DL Middle Zone start address</summary>
        DVDDL_MiddleZoneAddress,
        /// <summary>DVD-R DL Jump Interval Size</summary>
        DVDDL_JumpIntervalSize,
        /// <summary>DVD-R DL Start LBA of the manual layer jump</summary>
        DVDDL_ManualLayerJumpLBA,
        /// <summary>Blu-ray Disc Information</summary>
        BD_DI,
        /// <summary>Blu-ray Burst Cutting Area</summary>
        BD_BCA,
        /// <summary>Blu-ray Disc Definition Structure</summary>
        BD_DDS,
        /// <summary>Blu-ray Cartridge Status</summary>
        BD_CartridgeStatus,
        /// <summary>Blu-ray Status of Spare Area</summary>
        BD_SpareArea,
        /// <summary>AACS volume identifier</summary>
        AACS_VolumeIdentifier,
        /// <summary>AACS pre-recorded media serial number</summary>
        AACS_SerialNumber,
        /// <summary>AACS media identifier</summary>
        AACS_MediaIdentifier,
        /// <summary>Lead-in AACS media key block</summary>
        AACS_MKB,
        /// <summary>AACS data keys</summary>
        AACS_DataKeys,
        /// <summary>LBA extents flagged for bus encryption by AACS</summary>
        AACS_LBAExtents,
        /// <summary>CPRM media key block in Lead-in</summary>
        AACS_CPRM_MKB,
        /// <summary>Recognized layer formats in hybrid discs</summary>
        Hybrid_RecognizedLayers,
        /// <summary>Disc write protection status</summary>
        MMC_WriteProtection,
        /// <summary>Disc standard information</summary>
        MMC_DiscInformation,
        /// <summary>Disc track resources information</summary>
        MMC_TrackResourcesInformation,
        /// <summary>BD-R Pseudo-overwrite information</summary>
        MMC_POWResourcesInformation,
        /// <summary>SCSI INQUIRY response</summary>
        SCSI_INQUIRY,
        /// <summary>SCSI MODE PAGE 2Ah</summary>
        SCSI_MODEPAGE_2A,
        /// <summary>ATA IDENTIFY DEVICE response</summary>
        ATA_IDENTIFY,
        /// <summary>ATA IDENTIFY PACKET DEVICE response</summary>
        ATAPI_IDENTIFY,
        /// <summary>PCMCIA/CardBus Card Information Structure</summary>
        PCMCIA_CIS,
        /// <summary>SecureDigital CID</summary>
        SD_CID,
        /// <summary>SecureDigital CSD</summary>
        SD_CSD,
        /// <summary>SecureDigital SCR</summary>
        SD_SCR,
        /// <summary>SecureDigital OCR</summary>
        SD_OCR,
        /// <summary>MultiMediaCard CID</summary>
        MMC_CID,
        /// <summary>MultiMediaCard CSD</summary>
        MMC_CSD,
        /// <summary>MultiMediaCard OCR</summary>
        MMC_OCR,
        /// <summary>MultiMediaCard Extended CSD</summary>
        MMC_ExtendedCSD,
        /// <summary>Xbox Security Sector</summary>
        Xbox_SecuritySector,
        /// <summary>
        ///     On floppy disks, data in last cylinder usually in a different format that contains duplication or
        ///     manufacturing information
        /// </summary>
        Floppy_LeadOut
    }

    /// <summary>
    ///     Enumeration of media types defined in CICM metadata
    /// </summary>
    public enum XmlMediaType
    {
        /// <summary>
        ///     Purely optical discs
        /// </summary>
        OpticalDisc,
        /// <summary>
        ///     Media that is physically block-based or abstracted like that
        /// </summary>
        BlockMedia,
        /// <summary>
        ///     Media that can be accessed by-byte or by-bit, like chips
        /// </summary>
        LinearMedia,
        /// <summary>
        ///     Media that can only store data when it is modulated to audio
        /// </summary>
        AudioMedia
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
}