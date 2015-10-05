using System;

namespace DiscImageChef.CommonTypes
{
    /// <summary>
    /// Partition structure.
    /// </summary>
    public struct Partition
    {
        /// <summary>Partition number, 0-started</summary>
        public ulong PartitionSequence;
        /// <summary>Partition type</summary>
        public string PartitionType;
        /// <summary>Partition name (if the scheme supports it)</summary>
        public string PartitionName;
        /// <summary>Start of the partition, in bytes</summary>
        public ulong PartitionStart;
        /// <summary>LBA of partition start</summary>
        public ulong PartitionStartSector;
        /// <summary>Length in bytes of the partition</summary>
        public ulong PartitionLength;
        /// <summary>Length in sectors of the partition</summary>
        public ulong PartitionSectors;
        /// <summary>Information that does not find space in this struct</summary>
        public string PartitionDescription;
    }
}

