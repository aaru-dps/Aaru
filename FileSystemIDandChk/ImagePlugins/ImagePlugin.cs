/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : ImagePlugin.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Disc image plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Defines functions to be used by disc image plugins and several constants.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Collections.Generic;

namespace FileSystemIDandChk.ImagePlugins
{
    /// <summary>
    /// Abstract class to implement disk image reading plugins.
    /// </summary>
    public abstract class ImagePlugin
    {
        /// <summary>Plugin name.</summary>
        public string Name;
        /// <summary>Plugin UUID.</summary>
        public Guid PluginUUID;

        protected ImagePlugin()
        {
        }

        // Basic image handling functions

        /// <summary>
        /// Identifies the image.
        /// </summary>
        /// <returns><c>true</c>, if image was identified, <c>false</c> otherwise.</returns>
        /// <param name="imagePath">Image path.</param>
        public abstract bool IdentifyImage(string imagePath);

        /// <summary>
        /// Opens the image.
        /// </summary>
        /// <returns><c>true</c>, if image was opened, <c>false</c> otherwise.</returns>
        /// <param name="imagePath">Image path.</param>
        public abstract bool OpenImage(string imagePath);

        /// <summary>
        /// Asks the disk image plugin if the image contains partitions
        /// </summary>
        /// <returns><c>true</c>, if the image contains partitions, <c>false</c> otherwise.</returns>
        public abstract bool ImageHasPartitions();

        // Image size functions

        /// <summary>
        /// Gets the size of the image, without headers.
        /// </summary>
        /// <returns>The image size.</returns>
        public abstract UInt64 GetImageSize();

        /// <summary>
        /// Gets the number of sectors in the image.
        /// </summary>
        /// <returns>Sectors in image.</returns>
        public abstract UInt64 GetSectors();

        /// <summary>
        /// Returns the size of the biggest sector, counting user data only.
        /// </summary>
        /// <returns>Biggest sector size (user data only).</returns>
        public abstract UInt32 GetSectorSize();

        // Image reading functions

        /// <summary>
        /// Reads a disk tag.
        /// </summary>
        /// <returns>Disk tag</returns>
        /// <param name="tag">Tag type to read.</param>
        public abstract byte[] ReadDiskTag(DiskTagType tag);

        // Gets a disk tag
        /// <summary>
        /// Reads a sector's user data.
        /// </summary>
        /// <returns>The sector's user data.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        public abstract byte[] ReadSector(UInt64 sectorAddress);

        /// <summary>
        /// Reads a sector's tag.
        /// </summary>
        /// <returns>The sector's tag.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        /// <param name="tag">Tag type.</param>
        public abstract byte[] ReadSectorTag(UInt64 sectorAddress, SectorTagType tag);

        /// <summary>
        /// Reads a sector's user data, relative to track.
        /// </summary>
        /// <returns>The sector's user data.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        public abstract byte[] ReadSector(UInt64 sectorAddress, UInt32 track);

        /// <summary>
        /// Reads a sector's tag, relative to track.
        /// </summary>
        /// <returns>The sector's tag.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        /// <param name="tag">Tag type.</param>
        public abstract byte[] ReadSectorTag(UInt64 sectorAddress, UInt32 track, SectorTagType tag);

        /// <summary>
        /// Reads user data from several sectors.
        /// </summary>
        /// <returns>The sectors user data.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        public abstract byte[] ReadSectors(UInt64 sectorAddress, UInt32 length);

        /// <summary>
        /// Reads tag from several sectors.
        /// </summary>
        /// <returns>The sectors tag.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="tag">Tag type.</param>
        public abstract byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, SectorTagType tag);

        /// <summary>
        /// Reads user data from several sectors, relative to track.
        /// </summary>
        /// <returns>The sectors user data.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        public abstract byte[] ReadSectors(UInt64 sectorAddress, UInt32 length, UInt32 track);

        /// <summary>
        /// Reads tag from several sectors, relative to track.
        /// </summary>
        /// <returns>The sectors tag.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        /// <param name="tag">Tag type.</param>
        public abstract byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, UInt32 track, SectorTagType tag);

        /// <summary>
        /// Reads a complete sector (user data + all tags).
        /// </summary>
        /// <returns>The complete sector. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        public abstract byte[] ReadSectorLong(UInt64 sectorAddress);

        /// <summary>
        /// Reads a complete sector (user data + all tags), relative to track.
        /// </summary>
        /// <returns>The complete sector. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        public abstract byte[] ReadSectorLong(UInt64 sectorAddress, UInt32 track);

        /// <summary>
        /// Reads several complete sector (user data + all tags).
        /// </summary>
        /// <returns>The complete sectors. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        public abstract byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length);

        /// <summary>
        /// Reads several complete sector (user data + all tags), relative to track.
        /// </summary>
        /// <returns>The complete sectors. Format depends on disk type.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        public abstract byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length, UInt32 track);

        // Image information functions

        /// <summary>
        /// Gets the image format.
        /// </summary>
        /// <returns>The image format.</returns>
        public abstract string   GetImageFormat();

        /// <summary>
        /// Gets the image version.
        /// </summary>
        /// <returns>The image version.</returns>
        public abstract string   GetImageVersion();

        /// <summary>
        /// Gets the application that created the image.
        /// </summary>
        /// <returns>The application that created the image.</returns>
        public abstract string   GetImageApplication();

        /// <summary>
        /// Gets the version of the application that created the image.
        /// </summary>
        /// <returns>The version of the application that created the image.</returns>
        public abstract string   GetImageApplicationVersion();

        /// <summary>
        /// Gets the image creator.
        /// </summary>
        /// <returns>Who created the image.</returns>
        public abstract string   GetImageCreator();

        /// <summary>
        /// Gets the image creation time.
        /// </summary>
        /// <returns>The image creation time.</returns>
        public abstract DateTime GetImageCreationTime();

        /// <summary>
        /// Gets the image last modification time.
        /// </summary>
        /// <returns>The image last modification time.</returns>
        public abstract DateTime GetImageLastModificationTime();

        /// <summary>
        /// Gets the name of the image.
        /// </summary>
        /// <returns>The image name.</returns>
        public abstract string   GetImageName();

        /// <summary>
        /// Gets the image comments.
        /// </summary>
        /// <returns>The image comments.</returns>
        public abstract string   GetImageComments();

        // Functions to get information from disk represented by image

        /// <summary>
        /// Gets the disk manufacturer.
        /// </summary>
        /// <returns>The disk manufacturer.</returns>
        public abstract string   GetDiskManufacturer();

        /// <summary>
        /// Gets the disk model.
        /// </summary>
        /// <returns>The disk model.</returns>
        public abstract string   GetDiskModel();

        /// <summary>
        /// Gets the disk serial number.
        /// </summary>
        /// <returns>The disk serial number.</returns>
        public abstract string   GetDiskSerialNumber();

        /// <summary>
        /// Gets the disk (or product) barcode.
        /// </summary>
        /// <returns>The disk barcode.</returns>
        public abstract string   GetDiskBarcode();

        /// <summary>
        /// Gets the disk part number.
        /// </summary>
        /// <returns>The disk part number.</returns>
        public abstract string   GetDiskPartNumber();

        /// <summary>
        /// Gets the type of the disk.
        /// </summary>
        /// <returns>The disk type.</returns>
        public abstract DiskType GetDiskType();

        /// <summary>
        /// Gets the disk sequence.
        /// </summary>
        /// <returns>The disk sequence, starting at 1.</returns>
        public abstract int      GetDiskSequence();

        /// <summary>
        /// Gets the last disk in the sequence.
        /// </summary>
        /// <returns>The last disk in the sequence.</returns>
        public abstract int      GetLastDiskSequence();

        // Functions to get information from drive used to create image

        /// <summary>
        /// Gets the manufacturer of the drive used to create the image.
        /// </summary>
        /// <returns>The drive manufacturer.</returns>
        public abstract string GetDriveManufacturer();

        /// <summary>
        /// Gets the model of the drive used to create the image.
        /// </summary>
        /// <returns>The drive model.</returns>
        public abstract string GetDriveModel();

        /// <summary>
        /// Gets the serial number of the drive used to create the image.
        /// </summary>
        /// <returns>The drive serial number.</returns>
        public abstract string GetDriveSerialNumber();

        // Partitioning functions

        /// <summary>
        /// Gets an array partitions. Typically only useful for optical disc
        /// images where each track and index means a different partition, as
        /// reads can be relative to them.
        /// </summary>
        /// <returns>The partitions.</returns>
        public abstract List<PartPlugins.Partition> GetPartitions();

        /// <summary>
        /// Gets the disc track extents (start, length).
        /// </summary>
        /// <returns>The track extents.</returns>
        public abstract List<Track> GetTracks();

        /// <summary>
        /// Gets the disc track extents for a specified session.
        /// </summary>
        /// <returns>The track exents for that session.</returns>
        /// <param name="session">Session.</param>
        public abstract List<Track> GetSessionTracks(Session session);

        /// <summary>
        /// Gets the disc track extents for a specified session.
        /// </summary>
        /// <returns>The track exents for that session.</returns>
        /// <param name="session">Session.</param>
        public abstract List<Track> GetSessionTracks(UInt16 session);

        /// <summary>
        /// Gets the sessions (optical discs only).
        /// </summary>
        /// <returns>The sessions.</returns>
        public abstract List<Session> GetSessions();
        // Returns disc sessions

        // CD flags bitmask

        /// <summary>Track is quadraphonic.</summary>
        public const byte CDFlagsFourChannel = 0x20;
        /// <summary>Track is non-audio (data).</summary>
        public const byte CDFlagsDataTrack = 0x10;
        /// <summary>Track is copy protected.</summary>
        public const byte CDFlagsCopyPrevent = 0x08;
        /// <summary>Track has pre-emphasis.</summary>
        public const byte CDFlagsPreEmphasis = 0x04;
    }

    // Disk types
    public enum DiskType
    {
        /// <summary>Unknown disk type</summary>
        Unknown,

        // Somewhat standard Compact Disc formats
        /// <summary>CD Digital Audio (Red Book)</summary>
        CDDA,
        /// <summary>CD+G (Red Book)</summary>
        CDG,
        /// <summary>CD+EG (Red Book)</summary>
        CDEG,
        /// <summary>CD-i (Green Book)</summary>
        CDI,
        /// <summary>CD-ROM (Yellow Book)</summary>
        CDROM,
        /// <summary>CD-ROM XA (Yellow Book)</summary>
        CDROMXA,
        /// <summary>CD+ (Blue Book)</summary>
        CDPLUS,
        /// <summary>CD-MO (Orange Book)</summary>
        CDMO,
        /// <summary>CD-Recordable (Orange Book)</summary>
        CDR,
        /// <summary>CD-ReWritable (Orange Book)</summary>
        CDRW,
        /// <summary>Mount-Rainier CD-RW</summary>
        CDMRW,
        /// <summary>Video CD (White Book)</summary>
        VCD,
        /// <summary>Super Video CD (White Book)</summary>
        SVCD,
        /// <summary>Photo CD (Beige Book)</summary>
        PCD,
        /// <summary>Super Audio CD (Scarlet Book)</summary>
        SACD,
        /// <summary>Double-Density CD-ROM (Purple Book)</summary>
        DDCD,
        /// <summary>DD CD-R (Purple Book)</summary>
        DDCDR,
        /// <summary>DD CD-RW (Purple Book)</summary>
        DDCDRW,
        /// <summary>DTS audio CD (non-standard)</summary>
        DTSCD,
        /// <summary>CD-MIDI (Red Book)</summary>
        CDMIDI,
        /// <summary>Any unknown or standard violating CD</summary>
        CD,

        // Standard DVD formats
        /// <summary>DVD-ROM (applies to DVD Video and DVD Audio)</summary>
        DVDROM,
        /// <summary>DVD-R</summary>
        DVDR,
        /// <summary>DVD-RW</summary>
        DVDRW,
        /// <summary>DVD+R</summary>
        DVDPR,
        /// <summary>DVD+RW</summary>
        DVDPRW,
        /// <summary>DVD+RW DL</summary>
        DVDPRWDL,
        /// <summary>DVD-R DL</summary>
        DVDRDL,
        /// <summary>DVD+R DL</summary>
        DVDPRDL,
        /// <summary>DVD-RAM</summary>
        DVDRAM,

        // Standard HD-DVD formats
        /// <summary>HD DVD-ROM (applies to HD DVD Video)</summary>
        HDDVDROM,
        /// <summary>HD DVD-RAM</summary>
        HDDVDRAM,
        /// <summary>HD DVD-R</summary>
        HDDVDR,
        /// <summary>HD DVD-RW</summary>
        HDDVDRW,

        // Standard Blu-ray formats
        /// <summary>BD-ROM (and BD Video)</summary>
        BDROM,
        /// <summary>BD-R</summary>
        BDR,
        /// <summary>BD-RE</summary>
        BDRE,
        /// <summary>BD-R XL</summary>
        BDRXL,
        /// <summary>BD-RE XL</summary>
        BDREXL,

        // Rare or uncommon standards
        /// <summary>Enhanced Versatile Disc</summary>
        EVD,
        /// <summary>Forward Versatile Disc</summary>
        FVD,
        /// <summary>Holographic Versatile Disc</summary>
        HVD,
        /// <summary>China Blue High Definition</summary>
        CBHD,
        /// <summary>High Definition Versatile Multilayer Disc</summary>
        HDVMD,
        /// <summary>Versatile Compact Disc High Density</summary>
        VCDHD,
        /// <summary>Pioneer LaserDisc</summary>
        LD,
        /// <summary>Pioneer LaserDisc data</summary>
        LDROM,
        /// <summary>Sony MiniDisc</summary>
        MD,
        /// <summary>Sony Hi-MD</summary>
        HiMD,
        /// <summary>Ultra Density Optical</summary>
        UDO,
        /// <summary>Stacked Volumetric Optical Disc</summary>
        SVOD,
        /// <summary>Five Dimensional disc</summary>
        FDDVD,

        // Propietary game discs
        /// <summary>Sony PlayStation game CD</summary>
        PS1CD,
        /// <summary>Sony PlayStation 2 game CD</summary>
        PS2CD,
        /// <summary>Sony PlayStation 2 game DVD</summary>
        PS2DVD,
        /// <summary>Sony PlayStation 3 game DVD</summary>
        PS3DVD,
        /// <summary>Sony PlayStation 3 game Blu-ray</summary>
        PS3BD,
        /// <summary>Sony PlayStation 4 game Blu-ray</summary>
        PS4BD,
        /// <summary>Sony PlayStation Portable Universal Media Disc (ECMA-365)</summary>
        UMD,
        /// <summary>Nintendo GameCube Optical Disc</summary>
        GOD,
        /// <summary>Nintendo Wii Optical Disc</summary>
        WOD,
        /// <summary>Nintendo Wii U Optical Disc</summary>
        WUOD,
        /// <summary>Microsoft X-box Game Disc</summary>
        XGD,
        /// <summary>Microsoft X-box 360 Game Disc</summary>
        XGD2,
        /// <summary>Microsoft X-box 360 Game Disc</summary>
        XGD3,
        /// <summary>Microsoft X-box One Game Disc</summary>
        XGD4,
        /// <summary>Sega MegaCD</summary>
        MEGACD,
        /// <summary>Sega Saturn disc</summary>
        SATURNCD,
        /// <summary>Sega/Yamaha Gigabyte Disc</summary>
        GDROM,
        /// <summary>Sega/Yamaha recordable Gigabyte Disc}}</summary>
        GDR,

        // Apple standard floppy format
        /// <summary>5.25", SS, DD, 35 tracks, 13 spt, 256 bytes/sector, GCR</summary>
        Apple32SS,
        /// <summary>5.25", DS, DD, 35 tracks, 13 spt, 256 bytes/sector, GCR</summary>
        Apple32DS,
        /// <summary>5.25", SS, DD, 35 tracks, 16 spt, 256 bytes/sector, GCR</summary>
        Apple33SS,
        /// <summary>5.25", DS, DD, 35 tracks, 16 spt, 256 bytes/sector, GCR</summary>
        Apple33DS,
        /// <summary>3.5", SS, DD, 80 tracks, 8 to 12 spt, 512 bytes/sector, GCR</summary>
        AppleSonySS,
        /// <summary>3.5", DS, DD, 80 tracks, 8 to 12 spt, 512 bytes/sector, GCR</summary>
        AppleSonyDS,
        /// <summary>5.25", DS, ?D, ?? tracks, ?? spt, 512 bytes/sector, GCR, opposite side heads, aka Twiggy</summary>
        AppleFileWare,

        // IBM/Microsoft PC standard floppy formats
        /// <summary>5.25", SS, DD, 40 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_525_SS_DD_8,
        /// <summary>5.25", SS, DD, 40 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_525_SS_DD_9,
        /// <summary>5.25", DS, DD, 40 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_525_DS_DD_8,
        /// <summary>5.25", DS, DD, 40 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_525_DS_DD_9,
        /// <summary>5.25", DS, HD, 80 tracks, 15 spt, 512 bytes/sector, MFM</summary>
        DOS_525_HD,
        /// <summary>3.5", SS, DD, 80 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_35_SS_DD_8,
        /// <summary>3.5", SS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_35_SS_DD_9,
        /// <summary>3.5", DS, DD, 80 tracks, 8 spt, 512 bytes/sector, MFM</summary>
        DOS_35_DS_DD_8,
        /// <summary>3.5", DS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        DOS_35_DS_DD_9,
        /// <summary>3.5", DS, HD, 80 tracks, 18 spt, 512 bytes/sector, MFM</summary>
        DOS_35_HD,
        /// <summary>3.5", DS, ED, 80 tracks, 36 spt, 512 bytes/sector, MFM</summary>
        DOS_35_ED,

        // Microsoft non standard floppy formats
        /// <summary>3.5", DS, DD, 80 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        DMF,
        /// <summary>3.5", DS, DD, 82 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        DMF_82,

        // IBM non standard floppy formats
        XDF_525,
        XDF_35,

        // IBM standard floppy formats
        /// <summary>8", SS, SD, 32 tracks, 8 spt, 319 bytes/sector, FM</summary>
        IBM23FD,
        /// <summary>8", SS, SD, 73 tracks, 26 spt, 128 bytes/sector, FM</summary>
        IBM33FD_128,
        /// <summary>8", SS, SD, 74 tracks, 15 spt, 256 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM33FD_256,
        /// <summary>8", SS, SD, 74 tracks, 8 spt, 512 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM33FD_512,
        /// <summary>8", DS, SD, 74 tracks, 26 spt, 128 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM43FD_128,
        /// <summary>8", DS, SD, 74 tracks, 26 spt, 256 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector</summary>
        IBM43FD_256,
        /// <summary>8", DS, DD, 74 tracks, 26 spt, 256 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        IBM53FD_256,
        /// <summary>8", DS, DD, 74 tracks, 15 spt, 512 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        IBM53FD_512,
        /// <summary>8", DS, DD, 74 tracks, 8 spt, 1024 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        IBM53FD_1024,

        // DEC standard floppy formats
        /// <summary>8", SS, DD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        RX01,
        /// <summary>8", SS, DD, 77 tracks, 26 spt, 256 bytes/sector, FM/MFM</summary>
        RX02,

        // Acorn standard floppy formats
        /// <summary>5,25", SS, SD, 40 tracks, 10 spt, 256 bytes/sector, FM</summary>
        ACORN_525_SS_SD_40,
        /// <summary>5,25", SS, SD, 80 tracks, 10 spt, 256 bytes/sector, FM</summary>
        ACORN_525_SS_SD_80,
        /// <summary>5,25", SS, DD, 40 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        ACORN_525_SS_DD_40,
        /// <summary>5,25", SS, DD, 80 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        ACORN_525_SS_DD_80,
        /// <summary>5,25", DS, DD, 80 tracks, 16 spt, 256 bytes/sector, MFM</summary>
        ACORN_525_DS_DD,

        // Atari standard floppy formats
        /// <summary>5,25", SS, SD, 40 tracks, 18 spt, 128 bytes/sector, FM</summary>
        ATARI_525_SD,
        /// <summary>5,25", SS, ED, 40 tracks, 26 spt, 128 bytes/sector, MFM</summary>
        ATARI_525_ED,
        /// <summary>5,25", SS, DD, 40 tracks, 18 spt, 256 bytes/sector, MFM</summary>
        ATARI_525_DD,

        // Commodore standard floppy formats
        /// <summary>3,5", DS, DD, 80 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        CBM_35_DD,
        /// <summary>3,5", DS, DD, 80 tracks, 11 spt, 512 bytes/sector, MFM (Amiga)</summary>
        CBM_AMIGA_35_DD,
        /// <summary>3,5", DS, HD, 80 tracks, 22 spt, 512 bytes/sector, MFM (Amiga)</summary>
        CBM_AMIGA_35_HD,

        // NEC standard floppy formats
        /// <summary>8", SS, SD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        NEC_8_SD,
        /// <summary>8", DS, DD, 77 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        NEC_8_DD,
        /// <summary>5,25", DS, HD, 80 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        NEC_525_HD,
        /// <summary>3,5", DS, HD, 80 tracks, 8 spt, 1024 bytes/sector, MFM</summary>
        NEC_35_HD_8,
        /// <summary>3,5", DS, HD, 80 tracks, 15 spt, 512 bytes/sector, MFM</summary>
        NEC_35_HD_15,

        // SHARP standard floppy formats
        /// <summary>5,25", DS, DD, 77 tracks, 8 spt, 1024 bytes/sector, FM</summary>
        SHARP_525,
        /// <summary>3,5", DS, DD, 77 tracks, 8 spt, 1024 bytes/sector, FM</summary>
        SHARP_35,

        // ECMA standards
        /// <summary>5,25", DS, DD, 80 tracks, 8 spt, 1024 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_99_8,
        /// <summary>5,25", DS, DD, 77 tracks, 15 spt, 512 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_99_15,
        /// <summary>5,25", DS, DD, 77 tracks, 26 spt, 256 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_99_26,
        /// <summary>3,5", DS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM</summary>
        ECMA_100,
        /// <summary>3,5", DS, HD, 80 tracks, 18 spt, 512 bytes/sector, MFM</summary>
        ECMA_125,
        /// <summary>3,5", DS, ED, 80 tracks, 36 spt, 512 bytes/sector, MFM</summary>
        ECMA_147,
        /// <summary>8", SS, SD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        ECMA_54,
        /// <summary>8", DS, SD, 77 tracks, 26 spt, 128 bytes/sector, FM</summary>
        ECMA_59,
        /// <summary>5,25", SS, DD, 35 tracks, 9 spt, 256 bytes/sector, FM, track 0 side 0 = 16 sectors, 128 bytes/sector</summary>
        ECMA_66,
        /// <summary>8", DS, DD, 77 tracks, 8 spt, 1024 bytes/sector, FM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_69_8,
        /// <summary>8", DS, DD, 77 tracks, 15 spt, 512 bytes/sector, FM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_69_15,
        /// <summary>8", DS, DD, 77 tracks, 26 spt, 256 bytes/sector, FM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 256 bytes/sector</summary>
        ECMA_69_26,
        /// <summary>5,25", DS, DD, 40 tracks, 16 spt, 256 bytes/sector, FM, track 0 side 0 = 16 sectors, 128 bytes/sector, track 0 side 1 = 16 sectors, 256 bytes/sector</summary>
        ECMA_70,
        /// <summary>5,25", DS, DD, 80 tracks, 16 spt, 256 bytes/sector, FM, track 0 side 0 = 16 sectors, 128 bytes/sector, track 0 side 1 = 16 sectors, 256 bytes/sector</summary>
        ECMA_78,
        /// <summary>5,25", DS, DD, 80 tracks, 9 spt, 512 bytes/sector, FM</summary>
        ECMA_78_2,
        /// <summary>3,5", M.O., 250000 sectors, 512 bytes/sector</summary>
        ECMA_154,
        /// <summary>5,25", M.O., 940470 sectors, 512 bytes/sector</summary>
        ECMA_183_512,
        /// <summary>5,25", M.O., 520902 sectors, 1024 bytes/sector</summary>
        ECMA_183_1024,
        /// <summary>5,25", M.O., 1165600 sectors, 512 bytes/sector</summary>
        ECMA_184_512,
        /// <summary>5,25", M.O., 639200 sectors, 1024 bytes/sector</summary>
        ECMA_184_1024,
        /// <summary>3,5", M.O., 448500 sectors, 512 bytes/sector</summary>
        ECMA_201,

        // FDFORMAT, non-standard floppy formats
        /// <summary>5,25", DS, DD, 82 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_525_DD,
        /// <summary>5,25", DS, HD, 82 tracks, 17 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_525_HD,
        /// <summary>5,25", DS, DD, 82 tracks, 10 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_35_DD,
        /// <summary>5,25", DS, HD, 82 tracks, 21 spt, 512 bytes/sector, MFM</summary>
        FDFORMAT_35_HD,

        // Generic hard disks
        GENERIC_HDD
    };

    /// <summary>
    /// Track (as partitioning element) types.
    /// </summary>
    public enum TrackType
    {
        /// <summary>Audio track</summary>
        Audio,
        /// <summary>Data track (not any of the below defined ones)</summary>
        Data,
        /// <summary>Data track, compact disc mode 1</summary>
        CDMode1,
        /// <summary>Data track, compact disc mode 2, formless</summary>
        CDMode2Formless,
        /// <summary>Data track, compact disc mode 2, form 1</summary>
        CDMode2Form1,
        /// <summary>Data track, compact disc mode 2, form 2</summary>
        CDMode2Form2
    };

    /// <summary>
    /// Track defining structure.
    /// </summary>
    public struct Track
    {
        /// <summary>Track number, 1-started</summary>
        public UInt32 TrackSequence;
        /// <summary>Partition type</summary>
        public TrackType TrackType;
        /// <summary>Track starting sector</summary>
        public UInt64 TrackStartSector;
        /// <summary>Track ending sector</summary>
        public UInt64 TrackEndSector;
        /// <summary>Track pre-gap</summary>
        public UInt64 TrackPregap;
        /// <summary>Session this track belongs to</summary>
        public UInt16 TrackSession;
        /// <summary>Information that does not find space in this struct</summary>
        public string TrackDescription;
        /// <summary>Indexes, 00 to 99 and sector offset</summary>
        public Dictionary<int, UInt64> Indexes;
    }

    /// <summary>
    /// Session defining structure.
    /// </summary>
    public struct Session
    {
        /// <summary>Session number, 1-started</summary>
        public UInt16 SessionSequence;
        /// <summary>First track present on this session</summary>
        public UInt32 StartTrack;
        /// <summary>Last track present on this session</summary>
        public UInt32 EndTrack;
        /// <summary>First sector present on this session</summary>
        public UInt64 StartSector;
        /// <summary>Last sector present on this session</summary>
        public UInt64 EndSector;
    }

    /// <summary>
    /// Metadata present for each sector (aka, "tag").
    /// </summary>
    public enum SectorTagType
    {
        /// <summary>Apple's GCR sector tags, 12 bytes</summary>
        AppleSectorTag,
        /// <summary>Sync frame from CD sector, 12 bytes</summary>
        CDSectorSync,
        /// <summary>CD sector header, 4 bytes</summary>
        CDSectorHeader,
        /// <summary>CD mode 2 sector subheader</summary>
        CDSectorSubHeader,
        /// <summary>CD sector EDC, 4 bytes</summary>
        CDSectorEDC,
        /// <summary>CD sector ECC P, 172 bytes</summary>
        CDSectorECC_P,
        /// <summary>CD sector ECC Q, 104 bytes</summary>
        CDSectorECC_Q,
        /// <summary>CD sector ECC (P and Q), 276 bytes</summary>
        CDSectorECC,
        /// <summary>CD sector subchannel, 96 bytes</summary>
        CDSectorSubchannel,
        /// <summary>CD track ISRC, string, 12 bytes</summary>
        CDTrackISRC,
        /// <summary>CD track text, string, 13 bytes</summary>
        CDTrackText,
        /// <summary>CD track flags, 1 byte</summary>
        CDTrackFlags,
        /// <summary>DVD sector copyright information</summary>
        DVD_CMI
    };

    /// <summary>
    /// Metadata present for each disk.
    /// </summary>
    public enum DiskTagType
    {
        /// <summary>CD PMA</summary>
        CD_PMA,
        /// <summary>CD Adress-Time-In-Pregroove</summary>
        CD_ATIP,
        /// <summary>CD-Text</summary>
        CD_TEXT,
        /// <summary>CD Media Catalogue Number</summary>
        CD_MCN,
        /// <summary>DVD Burst Cutting Area</summary>
        DVD_BCA,
        /// <summary>DVD Physical Format Information</summary>
        DVD_PFI,
        /// <summary>DVD Copyright Management Information</summary>
        DVD_CMI,
        /// <summary>DVD Disc Manufacturer Information</summary>
        DVD_DMI
    };

    /// <summary>
    /// Feature is supported by image but not implemented yet.
    /// </summary>
    [Serializable]
    public class FeatureSupportedButNotImplementedImageException : Exception
    {
        public FeatureSupportedButNotImplementedImageException(string message, Exception inner) : base(message, inner)
        {
        }

        public FeatureSupportedButNotImplementedImageException(string message) : base(message)
        {
        }

        protected FeatureSupportedButNotImplementedImageException(System.Runtime.Serialization.SerializationInfo info,
                                                                  System.Runtime.Serialization.StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
        }
    }

    /// <summary>
    /// Feature is not supported by image.
    /// </summary>
    [Serializable]
    public class FeatureUnsupportedImageException : Exception
    {
        public FeatureUnsupportedImageException(string message, Exception inner) : base(message, inner)
        {
        }

        public FeatureUnsupportedImageException(string message) : base(message)
        {
        }

        protected FeatureUnsupportedImageException(System.Runtime.Serialization.SerializationInfo info,
                                                   System.Runtime.Serialization.StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
        }
    }

    /// <summary>
    /// Feature is supported by image but not present on it.
    /// </summary>
    [Serializable]
    public class FeatureNotPresentImageException : Exception
    {
        public FeatureNotPresentImageException(string message, Exception inner) : base(message, inner)
        {
        }

        public FeatureNotPresentImageException(string message) : base(message)
        {
        }

        protected FeatureNotPresentImageException(System.Runtime.Serialization.SerializationInfo info,
                                                  System.Runtime.Serialization.StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
        }
    }

    /// <summary>
    /// Feature is supported by image but not by the disc it represents.
    /// </summary>
    [Serializable]
    public class FeaturedNotSupportedByDiscImageException : Exception
    {
        public FeaturedNotSupportedByDiscImageException(string message, Exception inner) : base(message, inner)
        {
        }

        public FeaturedNotSupportedByDiscImageException(string message) : base(message)
        {
        }

        protected FeaturedNotSupportedByDiscImageException(System.Runtime.Serialization.SerializationInfo info,
                                                           System.Runtime.Serialization.StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
        }
    }

    /// <summary>
    /// Corrupt, incorrect or unhandled feature found on image
    /// </summary>
    [Serializable]
    public class ImageNotSupportedException : Exception
    {
        public ImageNotSupportedException(string message, Exception inner) : base(message, inner)
        {
        }

        public ImageNotSupportedException(string message) : base(message)
        {
        }

        protected ImageNotSupportedException(System.Runtime.Serialization.SerializationInfo info,
                                             System.Runtime.Serialization.StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
        }
    }
}