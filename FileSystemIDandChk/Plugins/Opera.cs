using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class OperaFS : Plugin
	{
		public OperaFS(PluginBase Core)
        {
            base.Name = "Opera Filesystem Plugin";
            base.PluginUUID = new Guid("0ec84ec7-eae6-4196-83fe-943b3fe46dbd");
        }
		
		public override bool Identify(FileStream fileStream, long offset)
		{
            fileStream.Seek(0 + offset, SeekOrigin.Begin);

            byte record_type;
            byte[] sync_bytes = new byte[5];
            byte record_version;
			
			record_type = (byte)fileStream.ReadByte();
            fileStream.Read(sync_bytes, 0, 5);
			record_version = (byte)fileStream.ReadByte();
			
			if (record_type != 1 || record_version != 1)
                return false;
            if(Encoding.ASCII.GetString(sync_bytes) != "ZZZZZ")
                return false;
			
			return true;
		}
		
		public override void GetInformation (FileStream fileStream, long offset, out string information)
		{
			information = "";
            StringBuilder SuperBlockMetadata = new StringBuilder();

            fileStream.Seek(0 + offset, SeekOrigin.Begin);

			EndianAwareBinaryReader eabr = new EndianAwareBinaryReader(fileStream, false); // BigEndian
			OperaSuperBlock sb = new OperaSuperBlock();
			byte[] cString;

			sb.record_type = eabr.ReadByte();
			sb.sync_bytes = eabr.ReadBytes(5);
			sb.record_version = eabr.ReadByte();
			sb.volume_flags = eabr.ReadByte();
			cString = eabr.ReadBytes(32);
			sb.volume_comment = StringHandlers.CToString(cString);
			cString = eabr.ReadBytes(32);
			sb.volume_label = StringHandlers.CToString(cString);
			sb.volume_id = eabr.ReadInt32();
			sb.block_size = eabr.ReadInt32();
			sb.block_count = eabr.ReadInt32();
			sb.root_dirid = eabr.ReadInt32();
			sb.rootdir_blocks = eabr.ReadInt32();
			sb.rootdir_bsize = eabr.ReadInt32();
			sb.last_root_copy = eabr.ReadInt32();

			if (sb.record_type != 1 || sb.record_version != 1)
                return;
			if(Encoding.ASCII.GetString(sb.sync_bytes) != "ZZZZZ")
                return;

			if (sb.volume_comment.Length == 0)
				sb.volume_comment = "Not set.";

			if (sb.volume_label.Length == 0)
				sb.volume_label = "Not set.";

            SuperBlockMetadata.AppendFormat("Opera filesystem disc.").AppendLine();
			SuperBlockMetadata.AppendFormat("Volume label: {0}", sb.volume_label).AppendLine();
			SuperBlockMetadata.AppendFormat("Volume comment: {0}", sb.volume_comment).AppendLine();
			SuperBlockMetadata.AppendFormat("Volume identifier: 0x{0:X8}", sb.volume_id).AppendLine();
			SuperBlockMetadata.AppendFormat("Block size: {0} bytes", sb.block_size).AppendLine();
			SuperBlockMetadata.AppendFormat("Volume size: {0} blocks, {1} bytes", sb.block_count, sb.block_size*sb.block_count).AppendLine();
			SuperBlockMetadata.AppendFormat("Root directory identifier: 0x{0:X8}", sb.root_dirid).AppendLine();
			SuperBlockMetadata.AppendFormat("Root directory block size: {0} bytes", sb.rootdir_bsize).AppendLine();
			SuperBlockMetadata.AppendFormat("Root directory size: {0} blocks, {1} bytes", sb.rootdir_blocks, sb.rootdir_bsize*sb.rootdir_blocks).AppendLine();
			SuperBlockMetadata.AppendFormat("Last root directory copy: {0}", sb.last_root_copy).AppendLine();

            information = SuperBlockMetadata.ToString();
		}

		private struct OperaSuperBlock
		{
			public byte   record_type;    // Record type, must be 1
			public byte[] sync_bytes;     // 5 bytes, "ZZZZZ" = new byte[5];
			public byte   record_version; // Record version, must be 1
			public byte   volume_flags;   // Volume flags
			public string volume_comment; // 32 bytes, volume comment
			public string volume_label;   // 32 bytes, volume label
			public Int32  volume_id;      // Volume ID
			public Int32  block_size;     // Block size in bytes
			public Int32  block_count;    // Blocks in volume
			public Int32  root_dirid;     // Root directory ID
			public Int32  rootdir_blocks; // Root directory blocks
			public Int32  rootdir_bsize;  // Root directory block size
			public Int32  last_root_copy; // Last root directory copy
		}
	}
}

