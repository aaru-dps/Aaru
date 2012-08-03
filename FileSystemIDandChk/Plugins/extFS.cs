using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class extFS : Plugin
	{
		public extFS(PluginBase Core)
        {
            base.Name = "Linux extended Filesystem";
			base.PluginUUID = new Guid("076CB3A2-08C2-4D69-BC8A-FCAA2E502BE2");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			byte[] magic_b = new byte[2];
			ushort magic;

			stream.Seek(0x400 + 56 + offset, SeekOrigin.Begin); // Here should reside magic number
			stream.Read(magic_b, 0, 2);
			magic = BitConverter.ToUInt16(magic_b, 0);
			
			if(magic == extFSMagic)
				return true;
			else
				return false;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			StringBuilder sb = new StringBuilder();

			BinaryReader br = new BinaryReader(stream);
			extFSSuperBlock ext_sb = new extFSSuperBlock();

			br.BaseStream.Seek(0x400 + offset, SeekOrigin.Begin);
			ext_sb.inodes = br.ReadUInt32();
			ext_sb.zones = br.ReadUInt32();
			ext_sb.firstfreeblk = br.ReadUInt32();
			ext_sb.freecountblk = br.ReadUInt32();
			ext_sb.firstfreeind = br.ReadUInt32();
			ext_sb.freecountind = br.ReadUInt32();
			ext_sb.firstdatazone = br.ReadUInt32();
			ext_sb.logzonesize = br.ReadUInt32();
			ext_sb.maxsize = br.ReadUInt32();

			sb.AppendLine("ext filesystem");
			sb.AppendFormat("{0} zones on volume", ext_sb.zones);
			sb.AppendFormat("{0} free blocks ({1} bytes)", ext_sb.freecountblk, ext_sb.freecountblk*1024);
			sb.AppendFormat("{0} inodes on volume, {1} free ({2}%)", ext_sb.inodes, ext_sb.freecountind, ext_sb.freecountind*100/ext_sb.inodes);
			sb.AppendFormat("First free inode is {0}", ext_sb.firstfreeind);
			sb.AppendFormat("First free block is {0}", ext_sb.firstfreeblk);
			sb.AppendFormat("First data zone is {0}", ext_sb.firstdatazone);
			sb.AppendFormat("Log zone size: {0}", ext_sb.logzonesize);
			sb.AppendFormat("Max zone size: {0}", ext_sb.maxsize);

			information = sb.ToString();
		}

		public const UInt16 extFSMagic = 0x137D;

		public struct extFSSuperBlock
		{
			public UInt32 inodes;        // inodes on volume
			public UInt32 zones;         // zones on volume
			public UInt32 firstfreeblk;  // first free block
			public UInt32 freecountblk;  // free blocks count
			public UInt32 firstfreeind;  // first free inode
			public UInt32 freecountind;  // free inodes count
			public UInt32 firstdatazone; // first data zone
			public UInt32 logzonesize;   // log zone size
			public UInt32 maxsize;       // max zone size
			public UInt32 reserved1;     // reserved
			public UInt32 reserved2;     // reserved
			public UInt32 reserved3;     // reserved
			public UInt32 reserved4;     // reserved
			public UInt32 reserved5;     // reserved
			public UInt16 magic;         // 0x137D (little endian)
		}
	}
}

