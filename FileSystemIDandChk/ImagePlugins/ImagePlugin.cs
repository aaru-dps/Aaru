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
    public abstract class ImagePlugin
    {
        public string Name;
        public Guid PluginUUID;

        protected ImagePlugin()
        {
        }
        // Basic image handling functions
        public abstract bool IdentifyImage(string imagePath);
        // Returns true if the plugin can handle the given image file
        public abstract bool OpenImage(string imagePath);
        // Initialize internal plugin structures to handle image
        public abstract bool ImageHasPartitions();
        // Image has different partitions (sessions, tracks)
        // Image size functions
        public abstract UInt64 GetImageSize();
        // Returns image size, without headers, in bytes
        public abstract UInt64 GetSectors();
        // Returns image size in sectors
        public abstract UInt32 GetSectorSize();
        // Returns sector size in bytes (user data only)
        // Image reading functions
        public abstract byte[] ReadDiskTag(DiskTagType tag);
        // Gets a disk tag
        public abstract byte[] ReadSector(UInt64 sectorAddress);
        // Reads a sector (user data only)
        public abstract byte[] ReadSectorTag(UInt64 sectorAddress, SectorTagType tag);
        // Reads specified tag from sector
        public abstract byte[] ReadSector(UInt64 sectorAddress, UInt32 track);
        // Reads a sector (user data only), relative to track
        public abstract byte[] ReadSectorTag(UInt64 sectorAddress, UInt32 track, SectorTagType tag);
        // Reads specified tag from sector
        public abstract byte[] ReadSectors(UInt64 sectorAddress, UInt32 length);
        // Reads sector (user data only)
        public abstract byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, SectorTagType tag);
        // Reads specified tag from sector
        public abstract byte[] ReadSectors(UInt64 sectorAddress, UInt32 length, UInt32 track);
        // Reads a sector (user data only), relative to track
        public abstract byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, UInt32 track, SectorTagType tag);
        // Reads specified tag from sector, relative to track
        public abstract byte[] ReadSectorLong(UInt64 sectorAddress);
        // Reads a sector (user data + tags)
        public abstract byte[] ReadSectorLong(UInt64 sectorAddress, UInt32 track);
        // Reads a sector (user data + tags), relative to track
        public abstract byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length);
        // Reads sector (user data + tags)
        public abstract byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length, UInt32 track);
        // Reads sectors (user data + tags), relative to track
        // Image information functions
        public abstract string   GetImageFormat();
        // Gets image format
        public abstract string   GetImageVersion();
        // Gets format's version
        public abstract string   GetImageApplication();
        // Gets application that created this image
        public abstract string   GetImageApplicationVersion();
        // Gets application version
        public abstract string   GetImageCreator();
        // Gets image creator (person)
        public abstract DateTime GetImageCreationTime();
        // Gets image creation time
        public abstract DateTime GetImageLastModificationTime();
        // Gets image last modification time
        public abstract string   GetImageName();
        // Gets image name
        public abstract string   GetImageComments();
        // Gets image comments
        // Functions to get information from disk represented by image
        public abstract string   GetDiskManufacturer();
        // Gets disk manufacturer
        public abstract string   GetDiskModel();
        // Gets disk model
        public abstract string   GetDiskSerialNumber();
        // Gets disk serial number
        public abstract string   GetDiskBarcode();
        // Gets disk (or product)
        public abstract string   GetDiskPartNumber();
        // Gets disk part no. as manufacturer set
        public abstract DiskType GetDiskType();
        // Gets disk type
        public abstract int      GetDiskSequence();
        // Gets disk sequence number, 1-starting
        public abstract int      GetLastDiskSequence();
        // Gets last disk sequence number
        // Functions to get information from drive used to create image
        public abstract string GetDriveManufacturer();
        // Gets drive manufacturer
        public abstract string GetDriveModel();
        // Gets drive model
        public abstract string GetDriveSerialNumber();
        // Gets drive serial number
        // Partitioning functions
        public abstract List<PartPlugins.Partition> GetPartitions();
        // Returns disc partitions, tracks, sessions, as partition extents
        public abstract List<Track> GetTracks();
        // Returns disc track extents
        public abstract List<Track> GetSessionTracks(Session session);
        // Returns disc track extensts for a session
        public abstract List<Track> GetSessionTracks(UInt16 session);
        // Returns disc track extensts for a session
        public abstract List<Session> GetSessions();
        // Returns disc sessions
        // CD flags bitmask
        public const byte CDFlagsFourChannel = 0x20;
        public const byte CDFlagsDataTrack = 0x10;
        public const byte CDFlagsCopyPrevent = 0x08;
        public const byte CDFlagsPreEmphasis = 0x04;
    }
    // Disk types
    public enum DiskType
    {
        Unknown,
        // Somewhat standard Compact Disc formats
        // CD Digital Audio (Red Book)
        CDDA,
        // CD+G (Red Book)
        CDG,
        // CD+EG (Red Book)
        CDEG,
        // CD-i (Green Book)
        CDI,
        // CD-ROM (Yellow Book)
        CDROM,
        // CD-ROM XA (Yellow Book)
        CDROMXA,
        // CD+ (Blue Book)
        CDPLUS,
        // CD-MO (Orange Book)
        CDMO,
        // CD-Recordable (Orange Book)
        CDR,
        // CD-ReWritable (Orange Book)
        CDRW,
        // Mount-Rainier CD-RW
        CDMRW,
        // Video CD (White Book)
        VCD,
        // Super Video CD (White Book)
        SVCD,
        // Photo CD (Beige Book)
        PCD,
        // Super Audio CD (Scarlet Book)
        SACD,
        // Double-Density CD-ROM (Purple Book)
        DDCD,
        // DD CD-R (Purple Book)
        DDCDR,
        // DD CD-RW (Purple Book)
        DDCDRW,
        // DTS audio CD (non-standard)
        DTSCD,
        // CD-MIDI (Red Book)
        CDMIDI,
        // Any unknown or standard violating CD
        CD,
        // Standard DVD formats
        // DVD-ROM (applies to DVD Video and DVD Audio)
        DVDROM,
        // DVD-R
        DVDR,
        // DVD-RW
        DVDRW,
        // DVD+R
        DVDPR,
        // DVD+RW
        DVDPRW,
        // DVD+RW DL
        DVDPRWDL,
        // DVD-R DL
        DVDRDL,
        // DVD+R DL
        DVDPRDL,
        // DVD-RAM
        DVDRAM,
        // Standard HD-DVD formats
        // HD DVD-ROM (applies to HD DVD Video)
        HDDVDROM,
        // HD DVD-RAM
        HDDVDRAM,
        // HD DVD-R
        HDDVDR,
        // HD DVD-RW
        HDDVDRW,
        // Standard Blu-ray formats
        // BD-ROM (and BD Video)
        BDROM,
        // BD-R
        BDR,
        // BD-RE
        BDRE,
        // Rare or uncommon standards
        // Enhanced Versatile Disc
        EVD,
        // Forward Versatile Disc
        FVD,
        // Holographic Versatile Disc
        HVD,
        // China Blue High Definition
        CBHD,
        // High Definition Versatile Multilayer Disc
        HDVMD,
        // Versatile Compact Disc High Density
        VCDHD,
        // Pioneer LaserDisc
        LD,
        // Pioneer LaserDisc data
        LDROM,
        // Sony MiniDisc
        MD,
        // Sony Hi-MD
        HiMD,
        // Ultra Density Optical
        UDO,
        // Stacked Volumetric Optical Disc
        SVOD,
        // Five Dimensional disc
        FDDVD,
        // Propietary game discs
        // Sony PlayStation game CD
        PS1CD,
        // Sony PlayStation 2 game CD
        PS2CD,
        // Sony PlayStation 2 game DVD
        PS2DVD,
        // Sony PlayStation 3 game DVD
        PS3DVD,
        // Sony PlayStation 3 game Blu-ray
        PS3BD,
        // Sony PlayStation 4 game Blu-ray
        PS4BD,
        // Sony PlayStation Portable Universal Media Disc (ECMA-365)
        UMD,
        // Nintendo GameCube Optical Disc
        GOD,
        // Nintendo Wii Optical Disc
        WOD,
        // Nintendo Wii U Optical Disc
        WUOD,
        // Microsoft X-box Game Disc
        XGD,
        // Microsoft X-box 360 Game Disc
        XGD2,
        // Microsoft X-box 360 Game Disc
        XGD3,
        // Microsoft X-box One Game Disc
        XGD4,
        // Sega MegaCD
        MEGACD,
        // Sega Saturn disc
        SATURNCD,
        // Sega/Yamaha Gigabyte Disc
        GDROM,
        // Sega/Yamaha recordable Gigabyte Disc}}
        GDR,
        // Apple standard floppy formats
        // 5.25", SS, DD, 35 tracks, 13 spt, 256 bytes/sector, GCR
        Apple32SS,
        // 5.25", DS, DD, 35 tracks, 13 spt, 256 bytes/sector, GCR
        Apple32DS,
        // 5.25", SS, DD, 35 tracks, 16 spt, 256 bytes/sector, GCR
        Apple33SS,
        // 5.25", DS, DD, 35 tracks, 16 spt, 256 bytes/sector, GCR
        Apple33DS,
        // 3.5", SS, DD, 80 tracks, 8 to 12 spt, 512 bytes/sector, GCR
        AppleSonySS,
        // 3.5", DS, DD, 80 tracks, 8 to 12 spt, 512 bytes/sector, GCR
        AppleSonyDS,
        // 5.25", DS, ?D, ?? tracks, ?? spt, 512 bytes/sector, GCR, opposite side heads, aka Twiggy
        AppleFileWare,
        // IBM/Microsoft PC standard floppy formats
        // 5.25", SS, DD, 40 tracks, 8 spt, 512 bytes/sector, MFM
        DOS_525_SS_DD_8,
        // 5.25", SS, DD, 40 tracks, 9 spt, 512 bytes/sector, MFM
        DOS_525_SS_DD_9,
        // 5.25", DS, DD, 40 tracks, 8 spt, 512 bytes/sector, MFM
        DOS_525_DS_DD_8,
        // 5.25", DS, DD, 40 tracks, 9 spt, 512 bytes/sector, MFM
        DOS_525_DS_DD_9,
        // 5.25", DS, HD, 80 tracks, 15 spt, 512 bytes/sector, MFM
        DOS_525_HD,
        // 3.5", SS, DD, 80 tracks, 8 spt, 512 bytes/sector, MFM
        DOS_35_SS_DD_8,
        // 3.5", SS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM
        DOS_35_SS_DD_9,
        // 3.5", DS, DD, 80 tracks, 8 spt, 512 bytes/sector, MFM
        DOS_35_DS_DD_8,
        // 3.5", DS, DD, 80 tracks, 9 spt, 512 bytes/sector, MFM
        DOS_35_DS_DD_9,
        // 3.5", DS, HD, 80 tracks, 18 spt, 512 bytes/sector, MFM
        DOS_35_HD,
        // 3.5", DS, ED, 80 tracks, 36 spt, 512 bytes/sector, MFM
        DOS_35_ED,
        // Microsoft non standard floppy formats
        // 3.5", DS, DD, 80 tracks, 21 spt, 512 bytes/sector, MFM
        DMF,
        // 3.5", DS, DD, 82 tracks, 21 spt, 512 bytes/sector, MFM
        DMF_82,
        // IBM non standard floppy formats
        XDF_525,
        XDF_35,
        // IBM standard floppy formats
        // 8", SS, SD, 32 tracks, 8 spt, 319 bytes/sector, FM
        IBM23FD,
        // 8", SS, SD, 73 tracks, 26 spt, 128 bytes/sector, FM
        IBM33FD_128,
        // 8", SS, SD, 74 tracks, 15 spt, 256 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector
        IBM33FD_256,
        // 8", SS, SD, 74 tracks, 8 spt, 512 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector
        IBM33FD_512,
        // 8", DS, SD, 74 tracks, 26 spt, 128 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector
        IBM43FD_128,
        // 8", DS, SD, 74 tracks, 26 spt, 256 bytes/sector, FM, track 0 = 26 sectors, 128 bytes/sector
        IBM43FD_256,
        // 8", DS, DD, 74 tracks, 26 spt, 256 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 128 bytes/sector
        IBM53FD_256,
        // 8", DS, DD, 74 tracks, 15 spt, 512 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 128 bytes/sector
        IBM53FD_512,
        // 8", DS, DD, 74 tracks, 8 spt, 1024 bytes/sector, MFM, track 0 side 0 = 26 sectors, 128 bytes/sector, track 0 side 1 = 26 sectors, 128 bytes/sector
        IBM53FD_1024,
        // DEC standard floppy formats
        // 8", SS, DD, 77 tracks, 26 spt, 128 bytes/sector, FM
        RX01,
        // 8", SS, DD, 77 tracks, 26 spt, 256 bytes/sector, FM/MFM
        RX02
    };
    // Track (as partitioning element) types
    public enum TrackType
    {
        Audio,
        // Audio track
        Data,
        // Data track (not any of the below defined ones)
        CDMode1,
        // Data track, compact disc mode 1
        CDMode2Formless,
        // Data track, compact disc mode 2, formless
        CDMode2Form1,
        // Data track, compact disc mode 2, form 1
        CDMode2Form2
        // Data track, compact disc mode 2, form 2}}

    };
    // Track defining structure
    public struct Track
    {
        public UInt32 TrackSequence;
        // Track number, 1-started
        public TrackType TrackType;
        // Partition type
        public UInt64 TrackStartSector;
        // Track starting sector
        public UInt64 TrackEndSector;
        // Track ending sector
        public UInt64 TrackPregap;
        // Track pre-gap
        public UInt16 TrackSession;
        // Session this track belongs to
        public string TrackDescription;
        // Information that does not find space in this struct
        public Dictionary<int, UInt64> Indexes;
        // Indexes, 00 to 99 and sector offset
    }
    // Session defining structure
    public struct Session
    {
        public UInt16 SessionSequence;
        // Session number, 1-started
        public UInt32 StartTrack;
        // First track present on this session
        public UInt32 EndTrack;
        // Last track present on this session
        public UInt64 StartSector;
        // First sector present on this session
        public UInt64 EndSector;
        // Last sector present on this session
    }
    // Metadata present for each sector (aka, "tag")
    public enum SectorTagType
    {
        AppleSectorTag,
        // Apple's GCR sector tags, 12 bytes
        CDSectorSync,
        // Sync frame from CD sector, 12 bytes
        CDSectorHeader,
        // CD sector header, 4 bytes
        CDSectorSubHeader,
        // CD mode 2 sector subheader
        CDSectorEDC,
        // CD sector EDC, 4 bytes
        CDSectorECC_P,
        // CD sector ECC P, 172 bytes
        CDSectorECC_Q,
        // CD sector ECC Q, 104 bytes
        CDSectorECC,
        // CD sector ECC (P and Q), 276 bytes
        CDSectorSubchannel,
        // CD sector subchannel, 96 bytes
        CDTrackISRC,
        // CD track ISRC, string, 12 bytes
        CDTrackText,
        // CD track text, string, 13 bytes
        CDTrackFlags,
        // CD track flags, 1 byte
        DVD_CMI
        // DVD sector copyright information}}

    };
    // Metadata present for each disk
    public enum DiskTagType
    {
        CD_PMA,
        // CD PMA
        CD_ATIP,
        // CD Adress-Time-In-Pregroove
        CD_TEXT,
        // CD-Text
        CD_MCN,
        // CD Media Catalogue Number
        DVD_BCA,
        // DVD Burst Cutting Area
        DVD_PFI,
        // DVD Physical Format Information
        DVD_CMI,
        // DVD Copyright Management Information
        DVD_DMI
        // DVD Disc Manufacturer Information}}

    };
    // Feature is supported by image but not implemented yet
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
    // Feature is not supported by image
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
    // Feature is supported by image but not present on it
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
    // Feature is supported by image but not by the disc it represents
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
    // Corrupt, incorrect or unhandled feature found on image
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