using Aaru.CommonTypes;
using Aaru.CommonTypes.Structs;

namespace Aaru.Tests
{
    /// <summary>Class to define expected data when testing media info</summary>
    public class MediaInfoTest
    {
        /// <summary>Expected media type</summary>
        public MediaType MediaType;
        /// <summary>Expected number of sectors in media</summary>
        public ulong Sectors;
        /// <summary>Expected media sector size</summary>
        public uint SectorSize;
        /// <summary>File that contains the image to test</summary>
        public string TestFile;

        public override string ToString() => TestFile;
    }

    /// <summary>Class to define expected data when testing filesystem info</summary>
    public class FileSystemTest : MediaInfoTest
    {
        /// <summary>Application ID</summary>
        public string ApplicationId;
        /// <summary>Can the volume boot?</summary>
        public bool Bootable;
        /// <summary>Clusters in volume</summary>
        public long Clusters;
        /// <summary>Bytes per cluster</summary>
        public uint ClusterSize;
        /// <summary>System or OEM ID</summary>
        public string SystemId;
        /// <summary>Filesystem type. null if always the same, as defined in test class</summary>
        public string Type;
        /// <summary>Volume name</summary>
        public string VolumeName;
        /// <summary>Volume serial number or set identifier</summary>
        public string VolumeSerial;
    }

    public class BlockImageTestExpected : MediaInfoTest
    {
        public string MD5;
    }

    public class TrackInfoTestExpected
    {
        public ulong  End;
        public byte?  Flags;
        public ulong? Pregap;
        public int    Session;
        public ulong  Start;
    }

    public class OpticalImageTestExpected : BlockImageTestExpected
    {
        public string                  LongMD5;
        public string                  SubchannelMD5;
        public TrackInfoTestExpected[] Tracks;
    }

    public class TapeImageTestExpected : BlockImageTestExpected
    {
        public TapeFile[]      Files;
        public TapePartition[] Partitions;
    }

    public class PartitionTest
    {
        public Partition[] Partitions;
        /// <summary>File that contains the partition scheme to test</summary>
        public string TestFile;
    }
}