using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from VMS File System Internals by Kirby McCoy
// ISBN: 1-55558-056-4
// With some hints from http://www.decuslib.com/DECUS/vmslt97b/gnusoftware/gccaxp/7_1/vms/hm2def.h

// Expects the home block to be always in sector #1 (does not check deltas)
// Assumes a sector size of 512 bytes (VMS does on HDDs and optical drives, dunno about M.O.)
// Book only describes ODS-2. Need to test ODS-1 and OSD-5
// There is an ODS with signature "DECFILES11A", yet to be seen

// Time is a 64 bit unsigned integer, tenths of microseconds since 1858/11/17 00:00:00.

namespace FileSystemIDandChk.Plugins
{
	class ODS : Plugin
	{
		public ODS(PluginBase Core)
        {
            base.Name = "Files-11 On-Disk Structure";
			base.PluginUUID = new Guid("de20633c-8021-4384-aeb0-83b0df14491f");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			byte[] magic_b = new byte[12];
			string magic;
			
			BinaryReader br = new BinaryReader(stream);
			
			br.BaseStream.Seek(0x200 + offset, SeekOrigin.Begin); // Seek to home block
			br.BaseStream.Seek(0x1F0, SeekOrigin.Current); // Seek to format
			
			br.BaseStream.Read(magic_b, 0, 12);
			magic = Encoding.ASCII.GetString(magic_b);
			
			if(magic == "DECFILE11A  " || magic == "DECFILE11B  ")
				return true;
			else
				return false;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			StringBuilder sb = new StringBuilder();
			BinaryReader br = new BinaryReader(stream);
			ODSHomeBlock homeblock = new ODSHomeBlock();
			byte[] temp_string = new byte[12];
			homeblock.min_class = new byte[20];
			homeblock.max_class = new byte[20];
			homeblock.reserved1 = new byte[302];
			
			br.BaseStream.Seek(0x200 + offset, SeekOrigin.Begin);
			
			homeblock.homelbn = br.ReadUInt32();
			homeblock.alhomelbn = br.ReadUInt32();
			homeblock.altidxlbn = br.ReadUInt32();
			homeblock.struclev = br.ReadUInt16();
			homeblock.cluster = br.ReadUInt16();
			homeblock.homevbn = br.ReadUInt16();
			homeblock.alhomevbn = br.ReadUInt16();
			homeblock.altidxvbn = br.ReadUInt16();
			homeblock.ibmapvbn = br.ReadUInt16();
			homeblock.ibmaplbn = br.ReadUInt32();
			homeblock.maxfiles = br.ReadUInt32();
			homeblock.ibmapsize = br.ReadUInt16();
			homeblock.resfiles = br.ReadUInt16();
			homeblock.devtype = br.ReadUInt16();
			homeblock.rvn = br.ReadUInt16();
			homeblock.setcount = br.ReadUInt16();
			homeblock.volchar = br.ReadUInt16();
			homeblock.volowner = br.ReadUInt32();
			homeblock.sec_mask = br.ReadUInt32();
			homeblock.protect = br.ReadUInt16();
			homeblock.fileprot = br.ReadUInt16();
			homeblock.recprot = br.ReadUInt16();
			homeblock.checksum1 = br.ReadUInt16();
			homeblock.credate = br.ReadUInt64();
			homeblock.window = br.ReadByte();
			homeblock.lru_lim = br.ReadByte();
			homeblock.extend = br.ReadUInt16();
			homeblock.retainmin = br.ReadUInt64();
			homeblock.retainmax = br.ReadUInt64();
			homeblock.revdate = br.ReadUInt64();
			homeblock.min_class = br.ReadBytes(20);
			homeblock.max_class = br.ReadBytes(20);
			homeblock.filetab_fid1 = br.ReadUInt16();
			homeblock.filetab_fid2 = br.ReadUInt16();
			homeblock.filetab_fid3 = br.ReadUInt16();
			homeblock.lowstruclev = br.ReadUInt16();
			homeblock.highstruclev = br.ReadUInt16();
			homeblock.copydate = br.ReadUInt64();
			homeblock.reserved1 = br.ReadBytes(302);
			homeblock.serialnum = br.ReadUInt32();
			temp_string = br.ReadBytes(12);
			homeblock.strucname = StringHandlers.CToString(temp_string);
			temp_string = br.ReadBytes(12);
			homeblock.volname = StringHandlers.CToString(temp_string);
			temp_string = br.ReadBytes(12);
			homeblock.ownername = StringHandlers.CToString(temp_string);
			temp_string = br.ReadBytes(12);
			homeblock.format = StringHandlers.CToString(temp_string);
			homeblock.reserved2 = br.ReadUInt16();
			homeblock.checksum2 = br.ReadUInt16();
			
			if((homeblock.struclev & 0xFF00) != 0x0200 || (homeblock.struclev & 0xFF) != 1 || homeblock.format != "DECFILE11B  ")
				sb.AppendLine("The following information may be incorrect for this volume.");
			if(homeblock.resfiles < 5 || homeblock.devtype != 0)
				sb.AppendLine("This volume may be corrupted.");
			
			sb.AppendFormat("Volume format is {0}", homeblock.format).AppendLine();
			sb.AppendFormat("Volume is Level {0} revision {1}", (homeblock.struclev&0xFF00)>>8, homeblock.struclev&0xFF).AppendLine();
			sb.AppendFormat("Lowest structure in the volume is Level {0}, revision {1}", (homeblock.lowstruclev&0xFF00)>>8, homeblock.lowstruclev&0xFF).AppendLine();
			sb.AppendFormat("Highest structure in the volume is Level {0}, revision {1}", (homeblock.highstruclev&0xFF00)>>8, homeblock.highstruclev&0xFF).AppendLine();
			sb.AppendFormat("{0} sectors per cluster ({1} bytes)", homeblock.cluster, homeblock.cluster*512).AppendLine();
			sb.AppendFormat("This home block is on sector {0} (cluster {1})", homeblock.homelbn, homeblock.homevbn).AppendLine();
			sb.AppendFormat("Secondary home block is on sector {0} (cluster {1})", homeblock.alhomelbn, homeblock.alhomevbn).AppendLine();
			sb.AppendFormat("Volume bitmap starts in sector {0} (cluster {1})", homeblock.ibmaplbn, homeblock.ibmapvbn).AppendLine();
			sb.AppendFormat("Volume bitmap runs for {0} sectors ({1} bytes)", homeblock.ibmapsize, homeblock.ibmapsize*512).AppendLine();
			sb.AppendFormat("Backup INDEXF.SYS;1 is in sector {0} (cluster {1})", homeblock.altidxlbn, homeblock.altidxvbn).AppendLine();
			sb.AppendFormat("{0} maximum files on the volume", homeblock.maxfiles).AppendLine();
			sb.AppendFormat("{0} reserved files", homeblock.resfiles).AppendLine();
			if(homeblock.rvn > 0 && homeblock.setcount > 0 && homeblock.strucname != "            ")
				sb.AppendFormat("Volume is {0} of {1} in set \"{2}\".", homeblock.rvn, homeblock.setcount, homeblock.strucname).AppendLine();
			sb.AppendFormat("Volume owner is \"{0}\" (ID 0x{1:X8})", homeblock.ownername, homeblock.volowner).AppendLine();
			sb.AppendFormat("Volume label: \"{0}\"", homeblock.volname).AppendLine();
			sb.AppendFormat("Drive serial number: 0x{0:X8}", homeblock.serialnum).AppendLine();
			sb.AppendFormat("Volume was created on {0}", DateHandlers.VMSToDateTime(homeblock.credate).ToString()).AppendLine();
			if(homeblock.revdate > 0)
				sb.AppendFormat("Volume was last modified on {0}", DateHandlers.VMSToDateTime(homeblock.revdate).ToString()).AppendLine();
			if(homeblock.copydate > 0)
				sb.AppendFormat("Volume copied on {0}", DateHandlers.VMSToDateTime(homeblock.copydate).ToString()).AppendLine();
			sb.AppendFormat("Checksums: 0x{0:X4} and 0x{1:X4}", homeblock.checksum1, homeblock.checksum2).AppendLine();
			sb.AppendLine("Flags:");
			sb.AppendFormat("Window: {0}", homeblock.window).AppendLine();
			sb.AppendFormat("Cached directores: {0}", homeblock.lru_lim).AppendLine();
			sb.AppendFormat("Default allocation: {0} blocks", homeblock.extend).AppendLine();
			if((homeblock.volchar & 0x01) == 0x01)
				sb.AppendLine("Readings should be verified");
			if((homeblock.volchar & 0x02) == 0x02)
				sb.AppendLine("Writings should be verified");
			if((homeblock.volchar & 0x04) == 0x04)
				sb.AppendLine("Files should be erased or overwritten when deleted");
			if((homeblock.volchar & 0x08) == 0x08)
				sb.AppendLine("Highwater mark is to be disabled");
			if((homeblock.volchar & 0x10) == 0x10)
				sb.AppendLine("Classification checks are enabled");
			sb.AppendLine("Volume permissions (r = read, w = write, c = create, d = delete)");
			sb.AppendLine("System, owner, group, world");
			// System
			if((homeblock.protect & 0x1000) == 0x1000)
				sb.Append("-");
			else
				sb.Append("r");
			if((homeblock.protect & 0x2000) == 0x2000)
				sb.Append("-");
			else
				sb.Append("w");
			if((homeblock.protect & 0x4000) == 0x4000)
				sb.Append("-");
			else
				sb.Append("c");
			if((homeblock.protect & 0x8000) == 0x8000)
				sb.Append("-");
			else
				sb.Append("d");
			// Owner
			if((homeblock.protect & 0x100) == 0x100)
				sb.Append("-");
			else
				sb.Append("r");
			if((homeblock.protect & 0x200) == 0x200)
				sb.Append("-");
			else
				sb.Append("w");
			if((homeblock.protect & 0x400) == 0x400)
				sb.Append("-");
			else
				sb.Append("c");
			if((homeblock.protect & 0x800) == 0x800)
				sb.Append("-");
			else
				sb.Append("d");
			// Group
			if((homeblock.protect & 0x10) == 0x10)
				sb.Append("-");
			else
				sb.Append("r");
			if((homeblock.protect & 0x20) == 0x20)
				sb.Append("-");
			else
				sb.Append("w");
			if((homeblock.protect & 0x40) == 0x40)
				sb.Append("-");
			else
				sb.Append("c");
			if((homeblock.protect & 0x80) == 0x80)
				sb.Append("-");
			else
				sb.Append("d");
			// World (other)
			if((homeblock.protect & 0x1) == 0x1)
				sb.Append("-");
			else
				sb.Append("r");
			if((homeblock.protect & 0x2) == 0x2)
				sb.Append("-");
			else
				sb.Append("w");
			if((homeblock.protect & 0x4) == 0x4)
				sb.Append("-");
			else
				sb.Append("c");
			if((homeblock.protect & 0x8) == 0x8)
				sb.Append("-");
			else
				sb.Append("d");
			
			sb.AppendLine();
			
			sb.AppendLine("Unknown structures:");
			sb.AppendFormat("Security mask: 0x{0:X8}", homeblock.sec_mask).AppendLine();
			sb.AppendFormat("File protection: 0x{0:X4}", homeblock.fileprot).AppendLine();
			sb.AppendFormat("Record protection: 0x{0:X4}", homeblock.recprot).AppendLine();
			
			information = sb.ToString();
		}
		
		private struct ODSHomeBlock
		{
			public UInt32 homelbn;      // LBN of THIS home block
			public UInt32 alhomelbn;    // LBN of the secondary home block
			public UInt32 altidxlbn;    // LBN of backup INDEXF.SYS;1
			public UInt16 struclev;     // High byte contains filesystem version (1, 2 or 5), low byte contains revision (1)
			public UInt16 cluster;      // Number of blocks each bit of the volume bitmap represents
			public UInt16 homevbn;      // VBN of THIS home block
			public UInt16 alhomevbn;    // VBN of the secondary home block
			public UInt16 altidxvbn;    // VBN of backup INDEXF.SYS;1
			public UInt16 ibmapvbn;     // VBN of the bitmap
			public UInt32 ibmaplbn;     // LBN of the bitmap
			public UInt32 maxfiles;     // Max files on volume
			public UInt16 ibmapsize;    // Bitmap size in sectors
			public UInt16 resfiles;     // Reserved files, 5 at minimum
			public UInt16 devtype;      // Device type, ODS-2 defines it as always 0
			public UInt16 rvn;          // Relative volume number (number of the volume in a set)
			public UInt16 setcount;     // Total number of volumes in the set this volume is
			public UInt16 volchar;      // Flags
			public UInt32 volowner;     // User ID of the volume owner
			public UInt32 sec_mask;     // Security mask (??)
			public UInt16 protect;      // Volume permissions (system, owner, group and other)
			public UInt16 fileprot;     // Default file protection, unsupported in ODS-2
			public UInt16 recprot;      // Default file record protection
			public UInt16 checksum1;    // Checksum of all preceding entries
			public UInt64 credate;      // Creation date
			public byte   window;       // Window size (pointers for the window)
			public byte   lru_lim;      // Directories to be stored in cache
			public UInt16 extend;       // Default allocation size in blocks
			public UInt64 retainmin;    // Minimum file retention period
			public UInt64 retainmax;    // Maximum file retention period
			public UInt64 revdate;      // Last modification date
			public byte[] min_class;    // Minimum security class, 20 bytes
			public byte[] max_class;    // Maximum security class, 20 bytes
			public UInt16 filetab_fid1; // File lookup table FID
			public UInt16 filetab_fid2; // File lookup table FID
			public UInt16 filetab_fid3; // File lookup table FID
			public UInt16 lowstruclev;  // Lowest structure level on the volume
			public UInt16 highstruclev; // Highest structure level on the volume
			public UInt64 copydate;     // Volume copy date (??)
			public byte[] reserved1;    // 302 bytes
			public UInt32 serialnum;    // Physical drive serial number
			public string strucname;    // Name of the volume set, 12 bytes
			public string volname;      // Volume label, 12 bytes
			public string ownername;    // Name of the volume owner, 12 bytes
			public string format;       // ODS-2 defines it as "DECFILE11B", 12 bytes
			public UInt16 reserved2;    // Reserved
			public UInt16 checksum2;    // Checksum of preceding 255 words (16 bit units)
		}
	}
}

