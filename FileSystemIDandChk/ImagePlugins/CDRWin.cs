using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using FileSystemIDandChk;

namespace FileSystemIDandChk.ImagePlugins
{
    class CDRWin : ImagePlugin
    {
#region Internal structures
        private struct TrackFile
        {
            public UInt32 sequence; // Track #
            public string datafile; // Path of file containing track
            public UInt64 offset;   // Offset of track start in file
        }
#endregion

#region Internal variables
        private string imagePath;
        private FileStream imageStream;
        private List<Session> sessions;
        private List<Track> tracks;
        private Dictionary<UInt32, TrackFile> trackFiles; // Dictionary, index is track #, value is TrackFile
#region

#region Parsing regexs
        private const string SessionRegEx    = "REM\\s+SESSION\\s+(?<number>\\d+)$";
        private const string CommentRegEx    = "REM\\s+(?<comment>.+)$";
        private const string CDTextRegEx     = "CDTEXMAIN\\s+(?<filename>.+)$";
        private const string MCNRegEx        = "CATALOG\\s+(?<catalog>\\d{13})$";
        private const string TitleRegEx      = "TITLE\\s+(?<title>.+)$";
        private const string PerformerRegEx  = "PERFORMER\\s+(?<performer>.+)$";
        private const string SongWriterRegEx = "SONGWRITER\\s+(?<songwriter>.+)$";
        private const string FileRegEx       = "FILE\\s+(?<filename>.+)\\s+(?<type>\\S+)$";
        private const string TrackRegEx      = "TRACK\\s+(?<number>\\d+)\\s+(?<type>\\S+)$";
        private const string ISRCRegEx       = "ISRC\\s+(?<isrc>\\w{12})$";
        private const string IndexRegEx      = "INDEX\\s+(?<index>\\d+)\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        private const string PregapRegEx     = "PREGAP\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        private const string PostgapRegex    = "POSTGAP\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        private const string FlagsRegEx      = "FLAGS\\+(((?<dcp>DCP)|(?<4ch>4CH)|(?<pre>PRE)|(?<scms>SCMS))\\s*)+$";
#endregion

#region Methods
        public CDRWin (PluginBase Core)
        {
            base.Name = "CDRWin cuesheet handler";
            base.PluginUUID = new Guid("664568B2-15D4-4E64-8A7A-20BDA8B8386F");
            this.imagePath = "";
        }

        public CDRWin (PluginBase Core, string imagePath)
        {
            this.imagePath = imagePath;
        }

        // Due to .cue format, this method must parse whole file, ignoring errors (those will be thrown by OpenImage()).
        public override bool IdentifyImage()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }
        public override bool OpenImage()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }
        public override bool ImageHasPartitions()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override UInt64 GetImageSize()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }
        public override UInt64 GetSectors()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }
        public override UInt32 GetSectorSize()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadDiskTag(DiskTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSector(UInt64 SectorAddress)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorTag(UInt64 SectorAddress, SectorTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSector(UInt64 SectorAddress, UInt32 track)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorTag(UInt64 SectorAddress, UInt32 track, SectorTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectors(UInt64 SectorAddress, UInt32 length)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorsTag(UInt64 SectorAddress, UInt32 length, SectorTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectors(UInt64 SectorAddress, UInt32 length, UInt32 track)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorsTag(UInt64 SectorAddress, UInt32 length, UInt32 track, SectorTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorLong(UInt64 SectorAddress)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorLong(UInt64 SectorAddress, UInt32 track)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorsLong(UInt64 SectorAddress, UInt32 length)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorsLong(UInt64 SectorAddress, UInt32 length, UInt32 track)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string   GetImageFormat()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string   GetImageVersion()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string   GetImageApplication()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string   GetImageApplicationVersion()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override DateTime GetImageCreationTime()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override DateTime GetImageLastModificationTime()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string   GetImageComments()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string GetDiskSerialNumber()
        {
            return this.GetDiskBarcode();
        }

        public override string GetDiskBarcode()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override DiskType GetDiskType()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override List<PartPlugins.Partition> GetPartitions()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override List<Track> GetTracks()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override List<Track> GetSessionTracks(Session Session)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override List<Track> GetSessionTracks(UInt16 Session)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override List<Session> GetSessions()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }
#endregion

#region Unsupported features
        public override int    GetDiskSequence()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override int    GetLastDiskSequence()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDriveManufacturer()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDriveModel()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDriveSerialNumber()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDiskPartNumber()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDiskManufacturer()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDiskModel()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string   GetImageName()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string   GetImageCreator()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }
#endregion
    }
}

