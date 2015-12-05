/***************************************************************************
The Disc Image Chef
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
using DiscImageChef.CommonTypes;

namespace DiscImageChef.ImagePlugins
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
        /// <summary>Image information</summary>
        public ImageInfo ImageInfo;

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
        public abstract List<CommonTypes.Partition> GetPartitions();

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


        /// <summary>
        /// Verifies a sector.
        /// </summary>
        /// <returns>True if correct, false if incorrect, null if uncheckable.</returns>
        /// <param name="sectorAddress">Sector address (LBA).</param>
        public abstract bool? VerifySector(UInt64 sectorAddress);

        /// <summary>
        /// Verifies a sector, relative to track.
        /// </summary>
        /// <returns>True if correct, false if incorrect, null if uncheckable.</returns>
        /// <param name="sectorAddress">Sector address (relative LBA).</param>
        /// <param name="track">Track.</param>
        public abstract bool? VerifySector(UInt64 sectorAddress, UInt32 track);

        /// <summary>
        /// Verifies several sectors.
        /// </summary>
        /// <returns>True if all are correct, false if any is incorrect, null if any is uncheckable.</returns>
        /// <param name="sectorAddress">Starting sector address (LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="FailingLBAs">List of incorrect sectors</param>
        /// <param name="UnknownLBAs">List of uncheckable sectors</param>
        public abstract bool? VerifySectors(UInt64 sectorAddress, UInt32 length, out List<UInt64> FailingLBAs, out List<UInt64> UnknownLBAs);

        /// <summary>
        /// Verifies several sectors, relative to track.
        /// </summary>
        /// <returns>True if all are correct, false if any is incorrect, null if any is uncheckable.</returns>
        /// <param name="sectorAddress">Starting sector address (relative LBA).</param>
        /// <param name="length">How many sectors to read.</param>
        /// <param name="track">Track.</param>
        /// <param name="FailingLBAs">List of incorrect sectors</param>
        /// <param name="UnknownLBAs">List of uncheckable sectors</param>
        public abstract bool? VerifySectors(UInt64 sectorAddress, UInt32 length, UInt32 track, out List<UInt64> FailingLBAs, out List<UInt64> UnknownLBAs);

        /// <summary>
        /// Verifies disk image internal checksum.
        /// </summary>
        /// <returns>True if correct, false if incorrect, null if there is no internal checksum available</returns>
        public abstract bool? VerifyDiskImage();


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
        /// <summary>ATA IDENTIFY DEVICE response</summary>
        ATA_IDENTIFY,
        /// <summary>ATA IDENTIFY PACKET DEVICE response</summary>
        ATAPI_IDENTIFY
    };

    public enum XmlMediaType
    {
        OpticalDisc,
        BlockMedia,
        LinearMedia
    }

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