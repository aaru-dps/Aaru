using System;
using System.IO;
using System.Collections.Generic;

namespace FileSystemIDandChk.ImagePlugins
{
    public abstract class ImagePlugin
    {
        public string Name;
        public Guid PluginUUID;

        protected ImagePlugin (string ImagePath)
        {
        }

        // Basic image handling functions
        public abstract bool IdentifyImage();  // Returns true if the plugin can handle the given image file
        public abstract bool OpenImage(); // Initialize internal plugin structures to handle image
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
        public abstract string GetDiskManufacturer(); // Gets disk manufacturer
        public abstract string GetDiskModel();        // Gets disk model
        public abstract string GetDiskSerialNumber(); // Gets disk serial number
        public abstract string GetDiskBarcode();      // Gets disk (or product)
        public abstract string GetDiskPartNumber();   // Gets disk part no. as manufacturer set
        public abstract string GetDiskType();         // Gets disk type
        public abstract int    GetDiskSequence();     // Gets disk sequence number, 1-starting
        public abstract int    GetLastDiskSequence(); // Gets last disk sequence number

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
    }

    // Disk types
    public enum DiskType
    {
        Unknown
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
        AppleSectorTag,     // Apple's GCR sector tags
        CDSectorSync,       // Sync frame from CD sector
        CDSectorHeader,     // CD sector header
        CDSectorSubHeader,  // CD mode 2 sector subheader
        CDSectorEDC,        // CD sector EDC
        CDSectorECC_P,      // CD sector ECC P
        CDSectorECC_Q,      // CD sector ECC Q
        CDSectorECC,        // CD sector ECC (P and Q)
        CDSectorSubchannel, // CD sector subchannel
        CDTrackISRC,        // CD track ISRC
        CDTrackText,        // CD track text
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

}