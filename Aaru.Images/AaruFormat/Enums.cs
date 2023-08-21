// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations for Aaru Format disk images.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable UnusedMember.Local

namespace Aaru.DiscImages;

public sealed partial class AaruFormat
{
    /// <summary>List of known compression types</summary>
    enum CompressionType : ushort
    {
        /// <summary>Not compressed</summary>
        None = 0,
        /// <summary>LZMA</summary>
        Lzma = 1,
        /// <summary>FLAC</summary>
        Flac = 2,
        /// <summary>LZMA in Claunia Subchannel Transform processed data</summary>
        LzmaClauniaSubchannelTransform = 3
    }

    /// <summary>List of known data types</summary>
    enum DataType : ushort
    {
        /// <summary>No data</summary>
        NoData = 0,
        /// <summary>User data</summary>
        UserData = 1,
        /// <summary>CompactDisc partial Table of Contents</summary>
        CompactDiscPartialToc = 2,
        /// <summary>CompactDisc session information</summary>
        CompactDiscSessionInfo = 3,
        /// <summary>CompactDisc Table of Contents</summary>
        CompactDiscToc = 4,
        /// <summary>CompactDisc Power Management Area</summary>
        CompactDiscPma = 5,
        /// <summary>CompactDisc Absolute Time In Pregroove</summary>
        CompactDiscAtip = 6,
        /// <summary>CompactDisc Lead-in's CD-Text</summary>
        CompactDiscLeadInCdText = 7,
        /// <summary>DVD Physical Format Information</summary>
        DvdPfi = 8,
        /// <summary>DVD Lead-in's Copyright Management Information</summary>
        DvdLeadInCmi = 9,
        /// <summary>DVD Disc Key</summary>
        DvdDiscKey = 10,
        /// <summary>DVD Burst Cutting Area</summary>
        DvdBca = 11,
        /// <summary>DVD DMI</summary>
        DvdDmi = 12,
        /// <summary>DVD Media Identifier</summary>
        DvdMediaIdentifier = 13,
        /// <summary>DVD Media Key Block</summary>
        DvdMediaKeyBlock = 14,
        /// <summary>DVD-RAM Disc Definition Structure</summary>
        DvdRamDds = 15,
        /// <summary>DVD-RAM Medium Status</summary>
        DvdRamMediumStatus = 16,
        /// <summary>DVD-RAM Spare Area Information</summary>
        DvdRamSpareArea = 17,
        /// <summary>DVD-R RMD</summary>
        DvdRRmd = 18,
        /// <summary>DVD-R Pre-recorded Information</summary>
        DvdRPrerecordedInfo = 19,
        /// <summary>DVD-R Media Identifier</summary>
        DvdRMediaIdentifier = 20,
        /// <summary>DVD-R Physical Format Information</summary>
        DvdRPfi = 21,
        /// <summary>DVD ADress In Pregroove</summary>
        DvdAdip = 22,
        /// <summary>HD DVD Copy Protection Information</summary>
        HdDvdCpi = 23,
        /// <summary>HD DVD Medium Status</summary>
        HdDvdMediumStatus = 24,
        /// <summary>DVD DL Layer Capacity</summary>
        DvdDlLayerCapacity = 25,
        /// <summary>DVD DL Middle Zone Address</summary>
        DvdDlMiddleZoneAddress = 26,
        /// <summary>DVD DL Jump Interval Size</summary>
        DvdDlJumpIntervalSize = 27,
        /// <summary>DVD DL Manual Layer Jump LBA</summary>
        DvdDlManualLayerJumpLba = 28,
        /// <summary>Bluray Disc Information</summary>
        BlurayDi = 29,
        /// <summary>Bluray Burst Cutting Area</summary>
        BlurayBca = 30,
        /// <summary>Bluray Disc Definition Structure</summary>
        BlurayDds = 31,
        /// <summary>Bluray Cartridge Status</summary>
        BlurayCartridgeStatus = 32,
        /// <summary>Bluray Spare Area Information</summary>
        BluraySpareArea = 33,
        /// <summary>AACS Volume Identifier</summary>
        AacsVolumeIdentifier = 34,
        /// <summary>AACS Serial Number</summary>
        AacsSerialNumber = 35,
        /// <summary>AACS Media Identifier</summary>
        AacsMediaIdentifier = 36,
        /// <summary>AACS Media Key Block</summary>
        AacsMediaKeyBlock = 37,
        /// <summary>AACS Data Keys</summary>
        AacsDataKeys = 38,
        /// <summary>AACS LBA Extents</summary>
        AacsLbaExtents = 39,
        /// <summary>CPRM Media Key Block</summary>
        CprmMediaKeyBlock = 40,
        /// <summary>Recognized Layers</summary>
        HybridRecognizedLayers = 41,
        /// <summary>MMC Write Protection</summary>
        ScsiMmcWriteProtection = 42,
        /// <summary>MMC Disc Information</summary>
        ScsiMmcDiscInformation = 43,
        /// <summary>MMC Track Resources Information</summary>
        ScsiMmcTrackResourcesInformation = 44,
        /// <summary>MMC POW Resources Information</summary>
        ScsiMmcPowResourcesInformation = 45,
        /// <summary>SCSI INQUIRY RESPONSE</summary>
        ScsiInquiry = 46,
        /// <summary>SCSI MODE PAGE 2Ah</summary>
        ScsiModePage2A = 47,
        /// <summary>ATA IDENTIFY response</summary>
        AtaIdentify = 48,
        /// <summary>ATAPI IDENTIFY response</summary>
        AtapiIdentify = 49,
        /// <summary>PCMCIA CIS</summary>
        PcmciaCis = 50,
        /// <summary>SecureDigital CID</summary>
        SecureDigitalCid = 51,
        /// <summary>SecureDigital CSD</summary>
        SecureDigitalCsd = 52,
        /// <summary>SecureDigital SCR</summary>
        SecureDigitalScr = 53,
        /// <summary>SecureDigital OCR</summary>
        SecureDigitalOcr = 54,
        /// <summary>MultiMediaCard CID</summary>
        MultiMediaCardCid = 55,
        /// <summary>MultiMediaCard CSD</summary>
        MultiMediaCardCsd = 56,
        /// <summary>MultiMediaCard OCR</summary>
        MultiMediaCardOcr = 57,
        /// <summary>MultiMediaCard Extended CSD</summary>
        MultiMediaCardExtendedCsd = 58,
        /// <summary>Xbox Security Sector</summary>
        XboxSecuritySector = 59,
        /// <summary>Floppy Lead-out</summary>
        FloppyLeadOut = 60,
        /// <summary>Dvd Disc Control Block</summary>
        DvdDiscControlBlock = 61,
        /// <summary>CompactDisc First track pregap</summary>
        CompactDiscFirstTrackPregap = 62,
        /// <summary>CompactDisc Lead-out</summary>
        CompactDiscLeadOut = 63,
        /// <summary>SCSI MODE SENSE (6) response</summary>
        ScsiModeSense6 = 64,
        /// <summary>SCSI MODE SENSE (10) response</summary>
        ScsiModeSense10 = 65,
        /// <summary>USB descriptors</summary>
        UsbDescriptors = 66,
        /// <summary>Xbox DMI</summary>
        XboxDmi = 67,
        /// <summary>Xbox Physical Format Information</summary>
        XboxPfi = 68,
        /// <summary>CompactDisc sector prefix (sync, header</summary>
        CdSectorPrefix = 69,
        /// <summary>CompactDisc sector suffix (edc, ecc p, ecc q)</summary>
        CdSectorSuffix = 70,
        /// <summary>CompactDisc subchannel</summary>
        CdSectorSubchannel = 71,
        /// <summary>Apple Profile (20 byte) tag</summary>
        AppleProfileTag = 72,
        /// <summary>Apple Sony (12 byte) tag</summary>
        AppleSonyTag = 73,
        /// <summary>Priam Data Tower (24 byte) tag</summary>
        PriamDataTowerTag = 74,
        /// <summary>CompactDisc Media Catalogue Number (as in Lead-in), 13 bytes, ASCII</summary>
        CompactDiscMediaCatalogueNumber = 75,
        /// <summary>CompactDisc sector prefix (sync, header), only incorrect stored</summary>
        CdSectorPrefixCorrected = 76,
        /// <summary>CompactDisc sector suffix (edc, ecc p, ecc q), only incorrect stored</summary>
        CdSectorSuffixCorrected = 77,
        /// <summary>CompactDisc MODE 2 subheader</summary>
        CompactDiscMode2Subheader = 78,
        /// <summary>CompactDisc Lead-in</summary>
        CompactDiscLeadIn = 79,
        /// <summary>Decrypted DVD Disc Key</summary>
        DvdDiscKeyDecrypted = 80,
        /// <summary>DVD Copyright Management Information (CPR_MAI)</summary>
        DvdSectorCprMai = 81,
        /// <summary>Decrypted DVD Title Key</summary>
        DvdSectorTitleKeyDecrypted = 82,
        /// <summary>DVD Identification Data (ID)</summary>
        DvdSectorId = 83,
        /// <summary>DVD ID Error Detection Code (IED)</summary>
        DvdSectorIed = 84,
        /// <summary>DVD Error Detection Code (EDC)</summary>
        DvdSectorEdc = 85
    }

    /// <summary>List of known blocks types</summary>
    enum BlockType : uint
    {
        /// <summary>Block containing data</summary>
        DataBlock = 0x4B4C4244,
        /// <summary>Block containing a deduplication table</summary>
        DeDuplicationTable = 0x2A544444,
        /// <summary>Block containing the index</summary>
        Index = 0x58444E49,
        /// <summary>Block containing the index</summary>
        Index2 = 0x32584449,
        /// <summary>Block containing logical geometry</summary>
        GeometryBlock = 0x4D4F4547,
        /// <summary>Block containing metadata</summary>
        MetadataBlock = 0x4154454D,
        /// <summary>Block containing optical disc tracks</summary>
        TracksBlock = 0x534B5254,
        /// <summary>Block containing CICM XML metadata</summary>
        CicmBlock = 0x4D434943,
        /// <summary>Block containing contents checksums</summary>
        ChecksumBlock = 0x4D534B43,
        /// <summary>TODO: Block containing data position measurements</summary>
        DataPositionMeasurementBlock = 0x2A4D5044,
        /// <summary>TODO: Block containing a snapshot index</summary>
        SnapshotBlock = 0x50414E53,
        /// <summary>TODO: Block containing how to locate the parent image</summary>
        ParentBlock = 0x544E5250,
        /// <summary>Block containing an array of hardware used to create the image</summary>
        DumpHardwareBlock = 0x2A504D44,
        /// <summary>Block containing list of files for a tape image</summary>
        TapeFileBlock = 0x454C4654,
        /// <summary>Block containing list of partitions for a tape image</summary>
        TapePartitionBlock = 0x54425054,
        /// <summary>Block containing list of indexes for Compact Disc tracks</summary>
        CompactDiscIndexesBlock = 0x58494443,
        /// <summary>Block containing JSON version of Aaru Metadata</summary>
        AaruMetadataJsonBlock = 0x444D534A
    }

    enum ChecksumAlgorithm : byte
    {
        Invalid = 0, Md5     = 1, Sha1 = 2,
        Sha256  = 3, SpamSum = 4
    }

    enum CdFixFlags : uint
    {
        NotDumped    = 0x10000000, Correct         = 0x20000000, Mode2Form1Ok = 0x30000000,
        Mode2Form2Ok = 0x40000000, Mode2Form2NoCrc = 0x50000000
    }
}