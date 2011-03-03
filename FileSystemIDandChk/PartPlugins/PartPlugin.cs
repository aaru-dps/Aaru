using System;
using System.IO;
using System.Collections.Generic;

namespace FileSystemIDandChk.PartPlugins
{
	public abstract class PartPlugin
	{
		public string Name;
        public Guid PluginUUID;
		
		protected PartPlugin ()
		{
		}
		
        public abstract bool GetInformation(FileStream stream, out List<Partition> partitions);
	}
	
	public struct Partition
	{
		public ulong  PartitionSequence;    // Partition number, 0-started
		public string PartitionType;        // Partition type
		public string PartitionName;        // Partition name (if the scheme supports it)
		public long   PartitionStart;       // Start of the partition, in bytes
		public long   PartitionLength;      // Length in bytes of the partition
		public string PartitionDescription; // Information that does not find space in this struct
	}
}