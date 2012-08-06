using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Based on FAT's BPB, cannot find a FAT or directory

namespace FileSystemIDandChk.Plugins
{
	class SolarFS : Plugin
	{
		public SolarFS(PluginBase Core)
        {
            base.Name = "Solar_OS filesystem";
			base.PluginUUID = new Guid("EA3101C1-E777-4B4F-B5A3-8C57F50F6E65");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			byte signature; // 0x29
			string fs_type; // "SOL_FS  "

			BinaryReader br = new BinaryReader(stream);

			br.BaseStream.Seek(0x25 + offset, SeekOrigin.Begin); // FATs, 1 or 2, maybe 0, never bigger
			signature = br.ReadByte();
			br.BaseStream.Seek(0x35 + offset, SeekOrigin.Begin); // Media Descriptor if present is in 0x15
			fs_type = StringHandlers.CToString(br.ReadBytes(8));

			if(signature == 0x29 && fs_type == "SOL_FS  ")
				return true;
			else
				return false;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			StringBuilder sb = new StringBuilder();
			BinaryReader br = new BinaryReader(stream);

			SolarOSParameterBlock BPB = new SolarOSParameterBlock();

			br.BaseStream.Seek(offset, SeekOrigin.Begin);
			BPB.x86_jump = br.ReadBytes(3);
			BPB.OEMName = StringHandlers.CToString(br.ReadBytes(8));
			BPB.bps = br.ReadUInt16();
			BPB.unk1 = br.ReadByte();
			BPB.unk2 = br.ReadUInt16();
			BPB.root_ent = br.ReadUInt16();
			BPB.sectors = br.ReadUInt16();
			BPB.media = br.ReadByte();
			BPB.spfat = br.ReadUInt16();
			BPB.sptrk = br.ReadUInt16();
			BPB.heads = br.ReadUInt16();
			BPB.unk3 = br.ReadBytes(10);
			BPB.signature = br.ReadByte();
			BPB.unk4 = br.ReadUInt32();
			BPB.vol_name = StringHandlers.CToString(br.ReadBytes(11));
			BPB.fs_type = StringHandlers.CToString(br.ReadBytes(8));

			if(MainClass.isDebug)
			{
				Console.WriteLine("(SolarFS) BPB.x86_jump: 0x{0:X2}{1:X2}{2:X2}", BPB.x86_jump[0], BPB.x86_jump[1], BPB.x86_jump[2]);
				Console.WriteLine("(SolarFS) BPB.OEMName: \"{0}\"", BPB.OEMName);
				Console.WriteLine("(SolarFS) BPB.bps: {0}", BPB.bps);
				Console.WriteLine("(SolarFS) BPB.unk1: 0x{0:X2}", BPB.unk1);
				Console.WriteLine("(SolarFS) BPB.unk2: 0x{0:X4}", BPB.unk2);
				Console.WriteLine("(SolarFS) BPB.root_ent: {0}", BPB.root_ent);
				Console.WriteLine("(SolarFS) BPB.sectors: {0}", BPB.sectors);
				Console.WriteLine("(SolarFS) BPB.media: 0x{0:X2}", BPB.media);
				Console.WriteLine("(SolarFS) BPB.spfat: {0}", BPB.spfat);
				Console.WriteLine("(SolarFS) BPB.sptrk: {0}", BPB.sptrk);
				Console.WriteLine("(SolarFS) BPB.heads: {0}", BPB.heads);
				Console.WriteLine("(SolarFS) BPB.unk3: 0x{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}", BPB.unk3[0], BPB.unk3[1], BPB.unk3[2], BPB.unk3[3], BPB.unk3[4], BPB.unk3[5], BPB.unk3[6], BPB.unk3[7], BPB.unk3[8], BPB.unk3[9]);
				Console.WriteLine("(SolarFS) BPB.signature: 0x{0:X2}", BPB.signature);
				Console.WriteLine("(SolarFS) BPB.unk4: 0x{0:X8}", BPB.unk4);
				Console.WriteLine("(SolarFS) BPB.vol_name: \"{0}\"", BPB.vol_name);
				Console.WriteLine("(SolarFS) BPB.fs_type: \"{0}\"", BPB.fs_type);
			}

			sb.AppendLine("Solar_OS filesystem");
			sb.AppendFormat("Media descriptor: 0x{0:X2}", BPB.media).AppendLine();
			sb.AppendFormat("{0} bytes per sector", BPB.bps).AppendLine();
			sb.AppendFormat("{0} sectors on volume ({1} bytes)", BPB.sectors, BPB.sectors*BPB.bps).AppendLine();
			sb.AppendFormat("{0} heads", BPB.heads).AppendLine();
			sb.AppendFormat("{0} sectors per track", BPB.sptrk).AppendLine();
			sb.AppendFormat("Volume name: {0}", BPB.vol_name).AppendLine();

			information = sb.ToString();
		}
		
		public struct SolarOSParameterBlock
		{
			public byte[] x86_jump;    // 0x00, x86 jump (3 bytes), jumps to 0x60
			public string OEMName;     // 0x03, 8 bytes, "SOLAR_OS"
			public UInt16 bps;         // 0x0B, Bytes per sector
			public byte   unk1;        // 0x0D, unknown, 0x01
			public UInt16 unk2;        // 0x0E, unknown, 0x0201
			public UInt16 root_ent;    // 0x10, Number of entries on root directory ? (no root directory found)
			public UInt16 sectors;     // 0x12, Sectors in volume
			public byte   media;       // 0x14, Media descriptor
			public UInt16 spfat;       // 0x15, Sectors per FAT ? (no FAT found)
			public UInt16 sptrk;       // 0x17, Sectors per track
			public UInt16 heads;       // 0x19, Heads
			public byte[] unk3;        // 0x1B, unknown, 10 bytes, zero-filled
			public byte   signature;   // 0x25, 0x29
			public UInt32 unk4;        // 0x26, unknown, zero-filled
			public string vol_name;    // 0x2A, 11 bytes, volume name, space-padded
			public string fs_type;     // 0x35, 8 bytes, "SOL_FS  "
		}
	}
}

