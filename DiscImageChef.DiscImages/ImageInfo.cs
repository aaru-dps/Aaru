using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.ImagePlugins
{
    public struct ImageInfo
    {
        public bool imageHasPartitions;
        public bool imageHasSessions;
        public UInt64 imageSize;
        public UInt64 sectors;
        public UInt32 sectorSize;
        public List<MediaTagType> readableMediaTags;
        public List<SectorTagType> readableSectorTags;
        public string imageVersion;
        public string imageApplication;
        public string imageApplicationVersion;
        public string imageCreator;
        public DateTime imageCreationTime;
        public DateTime imageLastModificationTime;
        public string imageName;
        public string imageComments;
        public string mediaManufacturer;
        public string mediaModel;
        public string mediaSerialNumber;
        public string mediaBarcode;
        public string mediaPartNumber;
        public MediaType mediaType;
        public int mediaSequence;
        public int lastMediaSequence;
        public string driveManufacturer;
        public string driveModel;
        public string driveSerialNumber;
        public XmlMediaType xmlMediaType;
    }
}

