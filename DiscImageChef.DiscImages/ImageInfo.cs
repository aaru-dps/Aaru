using System;
using System.Collections.Generic;

namespace DiscImageChef.ImagePlugins
{
    public struct ImageInfo
    {
        public bool imageHasPartitions;
        public bool imageHasSessions;
        public UInt64 imageSize;
        public UInt64 sectors;
        public UInt32 sectorSize;
        public List<DiskTagType> readableDiskTags;
        public List<SectorTagType> readableSectorTags;
        public string imageVersion;
        public string imageApplication;
        public string imageApplicationVersion;
        public string imageCreator;
        public DateTime imageCreationTime;
        public DateTime imageLastModificationTime;
        public string imageName;
        public string imageComments;
        public string diskManufacturer;
        public string diskModel;
        public string diskSerialNumber;
        public string diskBarcode;
        public string diskPartNumber;
        public DiskType diskType;
        public int diskSequence;
        public int lastDiskSequence;
        public string driveManufacturer;
        public string driveModel;
        public string driveSerialNumber;
    }
}

