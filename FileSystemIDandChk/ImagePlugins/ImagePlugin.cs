using System;
using System.IO;
using System.Collections.Generic;

namespace FileSystemIDandChk.ImagePlugins
{
    public abstract class ImagePlugin
    {
        public string Name;
        public Guid PluginUUID;

        protected ImagePlugin ()
        {
        }

        // Basic image handling functions
        public abstract bool IdentifyImage(string imagepath);  // Returns true if the plugin can handle the given image file
        public abstract bool OpenImage(string imagepath); // Initialize internal plugin structures to handle image
        public abstract bool ImageHasPartitions(); // Image has different partitions (sessions, tracks)

        // Image size functions
        public abstract UInt64 GetImageSize(); // Returns image size, without headers, in bytes
        public abstract UInt64 GetSectors(); // Returns image size in sectors
        public abstract UInt32 GetSectorSize(); // Returns sector size in bytes (user data only)

        // Image reading functions
        public abstract byte[] ReadDiskTag(DiskTagType tag); // Gets a disk tag
        public abstract byte[] ReadSector(UInt64 SectorAddress); // Reads a sector (user data only)
        public abstract byte[] ReadSectorTag(UInt64 SectorAddress, SectorTagType tag); // Reads specified tag from sector
        public abstract byte[] ReadSector(UInt64 SectorAddress, UInt32 track); // Reads a sector (user data only), relative to track
        public abstract byte[] ReadSectorTag(UInt64 SectorAddress, UInt32 track, SectorTagType tag); // Reads specified tag from sector
        public abstract byte[] ReadSectors(UInt64 SectorAddress, UInt32 length); // Reads sector (user data only)
        public abstract byte[] ReadSectorsTag(UInt64 SectorAddress, UInt32 length, SectorTagType tag); // Reads specified tag from sector
        public abstract byte[] ReadSectors(UInt64 SectorAddress, UInt32 length, UInt32 track); // Reads a sector (user data only), relative to track
        public abstract byte[] ReadSectorsTag(UInt64 SectorAddress, UInt32 length, UInt32 track, SectorTagType tag); // Reads specified tag from sector, relative to track
        public abstract byte[] ReadSectorLong(UInt64 SectorAddress); // Reads a sector (user data + tags)
        public abstract byte[] ReadSectorLong(UInt64 SectorAddress, UInt32 track); // Reads a sector (user data + tags), relative to track
        public abstract byte[] ReadSectorsLong(UInt64 SectorAddress, UInt32 length); // Reads sector (user data + tags)
        public abstract byte[] ReadSectorsLong(UInt64 SectorAddress, UInt32 length, UInt32 track); // Reads sectors (user data + tags), relative to track

        // Image information functions
        public abstract string   GetImageFormat();               // Gets image format
        public abstract string   GetImageVersion();              // Gets format's version
        public abstract string   GetImageApplication();          // Gets application that created this image
        public abstract string   GetImageApplicationVersion();   // Gets application version
        public abstract string   GetImageCreator();              // Gets image creator (person)
        public abstract DateTime GetImageCreationTime();         // Gets image creation time
        public abstract DateTime GetImageLastModificationTime(); // Gets image last modification time
        public abstract string   GetImageName();                 // Gets image name
        public abstract string   GetImageComments();             // Gets image comments

        // Functions to get information from disk represented by image
        public abstract string   GetDiskManufacturer(); // Gets disk manufacturer
        public abstract string   GetDiskModel();        // Gets disk model
        public abstract string   GetDiskSerialNumber(); // Gets disk serial number
        public abstract string   GetDiskBarcode();      // Gets disk (or product)
        public abstract string   GetDiskPartNumber();   // Gets disk part no. as manufacturer set
        public abstract DiskType GetDiskType();         // Gets disk type
        public abstract int      GetDiskSequence();     // Gets disk sequence number, 1-starting
        public abstract int      GetLastDiskSequence(); // Gets last disk sequence number

        // Functions to get information from drive used to create image
        public abstract string GetDriveManufacturer(); // Gets drive manufacturer
        public abstract string GetDriveModel();        // Gets drive model
        public abstract string GetDriveSerialNumber(); // Gets drive serial number

        // Partitioning functions
        public abstract List<PartPlugins.Partition> GetPartitions();   // Returns disc partitions, tracks, sessions, as partition extents
        public abstract List<Track> GetTracks();                       // Returns disc track extents
        public abstract List<Track> GetSessionTracks(Session Session); // Returns disc track extensts for a session
        public abstract List<Track> GetSessionTracks(UInt16 Session);  // Returns disc track extensts for a session
        public abstract List<Session> GetSessions();                   // Returns disc sessions

        // CD flags bitmask
        public const byte CDFlagsFourChannel = 0x20;
        public const byte CDFlagsDataTrack   = 0x10;
        public const byte CDFlagsCopyPrevent = 0x08;
        public const byte CDFlagsPreEmphasis = 0x04;
    }

    // Disk types
    public enum DiskType
    {
        Unknown,
        // Somewhat standard Compact Disc formats
        CDDA,       // CD Digital Audio (Red Book)
        CDG,        // CD+G (Red Book)
        CDEG,       // CD+EG (Red Book)
        CDI,        // CD-i (Green Book)
        CDROM,      // CD-ROM (Yellow Book)
        CDROMXA,    // CD-ROM XA (Yellow Book)
        CDPLUS,     // CD+ (Blue Book)
        CDMO,       // CD-MO (Orange Book)
        CDR,        // CD-Recordable (Orange Book)
        CDRW,       // CD-ReWritable (Orange Book)
        CDMRW,      // Mount-Rainier CD-RW
        VCD,        // Video CD (White Book)
        SVCD,       // Super Video CD (White Book)
        PCD,        // Photo CD (Beige Book)
        SACD,       // Super Audio CD (Scarlet Book)
        DDCD,       // Double-Density CD-ROM (Purple Book)
        DDCDR,      // DD CD-R (Purple Book)
        DDCDRW,     // DD CD-RW (Purple Book)
        DTSCD,      // DTS audio CD (non-standard)
        CDMIDI,     // CD-MIDI (Red Book)
        CD,         // Any unknown or standard violating CD
        // Standard DVD formats
        DVDROM,     // DVD-ROM (applies to DVD Video and DVD Audio)
        DVDR,       // DVD-R
        DVDRW,      // DVD-RW
        DVDPR,      // DVD+R
        DVDPRW,     // DVD+RW
        DVDPRWDL,   // DVD+RW DL
        DVDRDL,     // DVD-R DL
        DVDPRDL,    // DVD+R DL
        DVDRAM,     // DVD-RAM
        // Standard HD-DVD formats
        HDDVDROM,   // HD DVD-ROM (applies to HD DVD Video)
        HDDVDRAM,   // HD DVD-RAM
        HDDVDR,     // HD DVD-R
        HDDVDRW,    // HD DVD-RW
        // Standard Blu-ray formats
        BDROM,      // BD-ROM (and BD Video)
        BDR,        // BD-R
        BDRE,       // BD-RE
        // Rare or uncommon standards
        EVD,        // Enhanced Versatile Disc
        FVD,        // Forward Versatile Disc
        HVD,        // Holographic Versatile Disc
        CBHD,       // China Blue High Definition
        HDVMD,      // High Definition Versatile Multilayer Disc
        VCDHD,      // Versatile Compact Disc High Density
        LD,         // Pioneer LaserDisc
        LDROM,      // Pioneer LaserDisc data
        MD,         // Sony MiniDisc
        HiMD,       // Sony Hi-MD
        UDO,        // Ultra Density Optical
        SVOD,       // Stacked Volumetric Optical Disc
        FDDVD,      // Five Dimensional disc
        // Propietary game discs
        PS1CD,      // Sony PlayStation game CD
        PS2CD,      // Sony PlayStation 2 game CD
        PS2DVD,     // Sony PlayStation 2 game DVD
        PS3DVD,     // Sony PlayStation 3 game DVD
        PS3BD,      // Sony PlayStation 3 game Blu-ray
        PS4BD,      // Sony PlayStation 4 game Blu-ray
        UMD,        // Sony PlayStation Portable Universal Media Disc (ECMA-365)
        GOD,        // Nintendo GameCube Optical Disc
        WOD,        // Nintendo Wii Optical Disc
        WUOD,       // Nintendo Wii U Optical Disc
        XGD,        // Microsoft X-box Game Disc
        XGD2,       // Microsoft X-box 360 Game Disc
        XGD3,       // Microsoft X-box 360 Game Disc
        XGD4,       // Microsoft X-box One Game Disc
        MEGACD,     // Sega MegaCD
        SATURNCD,   // Sega Saturn disc
        GDROM,      // Sega/Yamaha Gigabyte Disc
        GDR         // Sega/Yamaha recordable Gigabyte Disc
    };

    // Track (as partitioning element) types
    public enum TrackType
    {
        Audio,           // Audio track
        Data,            // Data track (not any of the below defined ones)
        CDMode1,         // Data track, compact disc mode 1
        CDMode2Formless, // Data track, compact disc mode 2, formless
        CDMode2Form1,    // Data track, compact disc mode 2, form 1
        CDMode2Form2     // Data track, compact disc mode 2, form 2
    };

    // Track defining structure
    public struct Track
    {
        public UInt32    TrackSequence;    // Track number, 1-started
        public TrackType TrackType;        // Partition type
        public UInt64    TrackStartSector; // Track starting sector
        public UInt64    TrackEndSector;   // Track ending sector
        public UInt64    TrackPregap;      // Track pre-gap
        public UInt16    TrackSession;     // Session this track belongs to
        public string    TrackDescription; // Information that does not find space in this struct
    }

    // Track index (subpartitioning)
    public struct TrackIndex
    {
        public byte IndexSequence; // Index number (00 to 99)
        public UInt64 IndexOffset; // Index sector
    }

    // Session defining structure
    public struct Session
    {
        public UInt16 SessionSequence; // Session number, 1-started
        public UInt32 StartTrack;      // First track present on this session
        public UInt32 EndTrack;        // Last track present on this session
        public UInt64 StartSector;     // First sector present on this session
        public UInt64 EndSector;       // Last sector present on this session
    }

    // Metadata present for each sector (aka, "tag")
    public enum SectorTagType
    {
        AppleSectorTag,     // Apple's GCR sector tags, 20 bytes
        CDSectorSync,       // Sync frame from CD sector, 12 bytes
        CDSectorHeader,     // CD sector header, 4 bytes
        CDSectorSubHeader,  // CD mode 2 sector subheader
        CDSectorEDC,        // CD sector EDC, 4 bytes
        CDSectorECC_P,      // CD sector ECC P, 172 bytes
        CDSectorECC_Q,      // CD sector ECC Q, 104 bytes
        CDSectorECC,        // CD sector ECC (P and Q), 276 bytes
        CDSectorSubchannel, // CD sector subchannel, 96 bytes
        CDTrackISRC,        // CD track ISRC, string, 12 bytes
        CDTrackText,        // CD track text, string, 13 bytes
        CDTrackFlags,       // CD track flags, 1 byte
        DVD_CMI             // DVD sector copyright information
    };

    // Metadata present for each disk
    public enum DiskTagType
    {
        CD_PMA,  // CD PMA
        CD_ATIP, // CD Adress-Time-In-Pregroove
        CD_TEXT, // CD-Text
        CD_MCN,  // CD Media Catalogue Number
        DVD_BCA, // DVD Burst Cutting Area
        DVD_PFI, // DVD Physical Format Information
        DVD_CMI, // DVD Copyright Management Information
        DVD_DMI  // DVD Disc Manufacturer Information
    };

    // Feature is supported by image but not implemented yet
    [Serializable()]
    public class FeatureSupportedButNotImplementedImageException : System.Exception
    {
        public FeatureSupportedButNotImplementedImageException() : base() { }
        public FeatureSupportedButNotImplementedImageException(string message) : base(message) { }
        public FeatureSupportedButNotImplementedImageException(string message, System.Exception inner) : base(message, inner) { }

        protected FeatureSupportedButNotImplementedImageException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

    // Feature is not supported by image
    [Serializable()]
    public class FeatureUnsupportedImageException : System.Exception
    {
        public FeatureUnsupportedImageException() : base() { }
        public FeatureUnsupportedImageException(string message) : base(message) { }
        public FeatureUnsupportedImageException(string message, System.Exception inner) : base(message, inner) { }

        protected FeatureUnsupportedImageException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

    // Feature is supported by image but not present on it
    [Serializable()]
    public class FeatureNotPresentImageException : System.Exception
    {
        public FeatureNotPresentImageException() : base() { }
        public FeatureNotPresentImageException(string message) : base(message) { }
        public FeatureNotPresentImageException(string message, System.Exception inner) : base(message, inner) { }

        protected FeatureNotPresentImageException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

    // Feature is supported by image but not by the disc it represents
    [Serializable()]
    public class FeaturedNotSupportedByDiscImageException : System.Exception
    {
        public FeaturedNotSupportedByDiscImageException() : base() { }
        public FeaturedNotSupportedByDiscImageException(string message) : base(message) { }
        public FeaturedNotSupportedByDiscImageException(string message, System.Exception inner) : base(message, inner) { }

        protected FeaturedNotSupportedByDiscImageException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }

    // Corrupt, incorrect or unhandled feature found on image
    [Serializable()]
    public class ImageNotSupportedException : System.Exception
    {
        public ImageNotSupportedException() : base() { }
        public ImageNotSupportedException(string message) : base(message) { }
        public ImageNotSupportedException(string message, System.Exception inner) : base(message, inner) { }

        protected ImageNotSupportedException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}