using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;

namespace Aaru.DiscImages
{
    public partial class CopyTape : IWritableTapeImage
    {
        long[]    blockPositionCache;
        ImageInfo imageInfo;
        Stream    imageStream;

        public CopyTape() => imageInfo = new ImageInfo
        {
            ReadableSectorTags    = new List<SectorTagType>(),
            ReadableMediaTags     = new List<MediaTagType>(),
            HasPartitions         = true,
            HasSessions           = true,
            Version               = null,
            ApplicationVersion    = null,
            MediaTitle            = null,
            Creator               = null,
            MediaManufacturer     = null,
            MediaModel            = null,
            MediaPartNumber       = null,
            MediaSequence         = 0,
            LastMediaSequence     = 0,
            DriveManufacturer     = null,
            DriveModel            = null,
            DriveSerialNumber     = null,
            DriveFirmwareRevision = null
        };
    }
}