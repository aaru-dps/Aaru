using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

namespace FileSystemIDandChk.Plugins
{
	class SysVfs : Plugin
	{
		private const UInt32 XENIX_MAGIC = 0x002B5544;
		private const UInt32 XENIX_CIGAM = 0x44552B00;
		private const UInt32 SYSV_MAGIC  = 0xFD187E20;
		private const UInt32 SYSV_CIGAM  = 0xFD187E20;
		// Rest have no magic.
		// Per a Linux kernel, Coherent fs has following:
		private const string COH_FNAME = "nonamexxxxx ";
		private const string COH_FPACK = "nopackxxxxx\n";
		// SCO AFS
		private const UInt16 SCO_NFREE = 0xFFFF;
		// UNIX 7th Edition has nothing to detect it, so check for a valid filesystem is a must :(
		private const UInt16 V7_NICINOD = 100;
		private const UInt16 V7_NICFREE = 50;
		private const UInt32 V7_MAXSIZE = 0x00FFFFFF;

		public SysVfs(PluginBase Core)
        {
            base.Name = "UNIX System V filesystem";
			base.PluginUUID = new Guid("9B8D016A-8561-400E-A12A-A198283C211D");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			UInt32 magic;
			string s_fname, s_fpack;
			UInt16 s_nfree, s_ninode;
			UInt32 s_fsize;

			BinaryReader br = new BinaryReader(stream);

			/*for(int j = 0; j<=(br.BaseStream.Length/0x200); j++)
			{
				br.BaseStream.Seek(offset + j*0x200 + 0x1F8, SeekOrigin.Begin); // System V magic location
				magic = br.ReadUInt32();

				if(magic == SYSV_MAGIC || magic == SYSV_CIGAM)
					Console.WriteLine("0x{0:X8}: 0x{1:X8} FOUND", br.BaseStream.Position-4, magic);
				else
					Console.WriteLine("0x{0:X8}: 0x{1:X8}", br.BaseStream.Position-4, magic);
			}*/

			/*UInt32 number;
			br.BaseStream.Seek(offset+0x3A00, SeekOrigin.Begin);
			while((br.BaseStream.Position) <= (offset+0x3C00))
			{
				number = br.ReadUInt32();

				Console.WriteLine("@{0:X8}: 0x{1:X8} ({1})", br.BaseStream.Position-offset-4, number);
			}*/

			for(int i = 0; i<=4; i++) // Check on 0x0000, 0x0200, 0x0600, 0x0800 + offset
			{
				if((ulong)br.BaseStream.Length <= (ulong)(offset + i*0x200 + 0x400)) // Stream must be bigger than SB location + SB size + offset
					return false;

				br.BaseStream.Seek(offset + i*0x200 + 0x3F8, SeekOrigin.Begin); // XENIX magic location
				magic = br.ReadUInt32();

				if(magic == XENIX_MAGIC || magic == XENIX_CIGAM)
					return true;

				br.BaseStream.Seek(offset + i*0x200 + 0x1F8, SeekOrigin.Begin); // System V magic location
				magic = br.ReadUInt32();

				if(magic == SYSV_MAGIC || magic == SYSV_CIGAM)
					return true;

				br.BaseStream.Seek(offset + i*0x200 + 0x1E8, SeekOrigin.Begin); // Coherent UNIX s_fname location
				s_fname = StringHandlers.CToString(br.ReadBytes(6));
				s_fpack = StringHandlers.CToString(br.ReadBytes(6));

				if(s_fname == COH_FNAME || s_fpack == COH_FPACK)
					return true;

				// Now try to identify 7th edition
				br.BaseStream.Seek(offset + i*0x200 + 0x002, SeekOrigin.Begin);
				s_fsize = br.ReadUInt32();
				br.BaseStream.Seek(offset + i*0x200 + 0x006, SeekOrigin.Begin);
				s_nfree = br.ReadUInt16();
				br.BaseStream.Seek(offset + i*0x200 + 0x0D0, SeekOrigin.Begin);
				s_ninode = br.ReadUInt16();

				if(s_fsize > 0 && s_fsize < 0xFFFFFFFF && s_nfree > 0 && s_nfree < 0xFFFF && s_ninode > 0 && s_ninode < 0xFFFF)
				{
					if((s_fsize & 0xFF) == 0x00 && (s_nfree & 0xFF) == 0x00 && (s_ninode & 0xFF) == 0x00)
					{
						// Byteswap
						s_fsize = ((s_fsize & 0xFF)<<24) + ((s_fsize & 0xFF00)<<8) + ((s_fsize & 0xFF0000)>>8) + ((s_fsize & 0xFF000000)>>24);
						s_nfree = (UInt16)(s_nfree>>8);
						s_ninode = (UInt16)(s_ninode>>8);
					}

					if((s_fsize & 0xFF000000) == 0x00 && (s_nfree & 0xFF00) == 0x00 && (s_ninode & 0xFF00) == 0x00)
					{
						if(s_fsize < V7_MAXSIZE && s_nfree < V7_NICFREE && s_ninode < V7_NICINOD)
						{
							if((s_fsize * 1024) <= (br.BaseStream.Length-offset) || (s_fsize * 512) <= (br.BaseStream.Length-offset))
								return true;
						}
					}
				}
			}

			return false;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			StringBuilder sb = new StringBuilder();
			bool littleendian = true;
			EndianAwareBinaryReader eabr = new EndianAwareBinaryReader(stream, littleendian); // Start in little endian until we know what are we handling here
			int start;
			UInt32 magic;
			string s_fname, s_fpack;
			UInt16 s_nfree, s_ninode;
			UInt32 s_fsize;
			bool xenix = false;
			bool sysv = false;
			bool sysvr2 = false;
			bool sysvr4 = false;
			bool sys7th = false;
			bool coherent = false;

			for(start = 0; start<=4; start++) // Check on 0x0000, 0x0200, 0x0600, 0x0800 + offset
			{
				eabr.BaseStream.Seek(offset + start*0x200 + 0x3F8, SeekOrigin.Begin); // XENIX magic location
				magic = eabr.ReadUInt32();
			
				if(magic == XENIX_MAGIC)
				{
					littleendian = true;
					xenix = true;
					break;
				}
				else if(magic == XENIX_CIGAM)
				{
					littleendian = false;
					xenix = true;
					break;
				}

				eabr.BaseStream.Seek(offset + start*0x200 + 0x1F8, SeekOrigin.Begin); // System V magic location
				magic = eabr.ReadUInt32();
			
				if(magic == SYSV_MAGIC)
				{
					littleendian = true;
					sysv = true;
					break;
				}
				else if(magic == SYSV_CIGAM)
				{
					littleendian = false;
					sysv = true;
					break;
				}

				eabr.BaseStream.Seek(offset + start*0x200 + 0x1E8, SeekOrigin.Begin); // Coherent UNIX s_fname location
				s_fname = StringHandlers.CToString(eabr.ReadBytes(6));
				s_fpack = StringHandlers.CToString(eabr.ReadBytes(6));
			
				if(s_fname == COH_FNAME	|| s_fpack == COH_FPACK)
				{
					littleendian = true; // Coherent is in PDP endianness, use helper for that
					coherent = true;
					break;
				}

				// Now try to identify 7th edition
				eabr.BaseStream.Seek(offset + start*0x200 + 0x002, SeekOrigin.Begin);
				s_fsize = eabr.ReadUInt32();
				eabr.BaseStream.Seek(offset + start*0x200 + 0x006, SeekOrigin.Begin);
				s_nfree = eabr.ReadUInt16();
				eabr.BaseStream.Seek(offset + start*0x200 + 0x0D0, SeekOrigin.Begin);
				s_ninode = eabr.ReadUInt16();

				if(s_fsize > 0 && s_fsize < 0xFFFFFFFF && s_nfree > 0 && s_nfree < 0xFFFF && s_ninode > 0 && s_ninode < 0xFFFF)
				{
					bool byteswapped = false;
					if((s_fsize & 0xFF) == 0x00 && (s_nfree & 0xFF) == 0x00 && (s_ninode & 0xFF) == 0x00)
					{
						// Byteswap
						s_fsize = ((s_fsize & 0xFF)<<24) + ((s_fsize & 0xFF00)<<8) + ((s_fsize & 0xFF0000)>>8) + ((s_fsize & 0xFF000000)>>24);
						s_nfree = (UInt16)(s_nfree>>8);
						s_ninode = (UInt16)(s_ninode>>8);
						byteswapped = true;
					}
					
					if((s_fsize & 0xFF000000) == 0x00 && (s_nfree & 0xFF00) == 0x00 && (s_ninode & 0xFF00) == 0x00)
					{
						if(s_fsize < V7_MAXSIZE && s_nfree < V7_NICFREE && s_ninode < V7_NICINOD)
						{
							if((s_fsize * 1024) <= (eabr.BaseStream.Length-offset) || (s_fsize * 512) <= (eabr.BaseStream.Length-offset))
							{
								sys7th = true;
								littleendian = true;
								break;
							}
						}
					}
				}
			}
			if(!sys7th && !sysv && !coherent && !xenix)
				return;

			if(xenix)
			{
				eabr = new EndianAwareBinaryReader(stream, littleendian);
				XenixSuperBlock xnx_sb = new XenixSuperBlock();
				eabr.BaseStream.Seek(offset + start*0x200, SeekOrigin.Begin);
				xnx_sb.s_isize = eabr.ReadUInt16();
				xnx_sb.s_fsize = eabr.ReadUInt32();
				xnx_sb.s_nfree = eabr.ReadUInt16();
				eabr.BaseStream.Seek(400, SeekOrigin.Current); // Skip free block list
				xnx_sb.s_ninode = eabr.ReadUInt16();
				eabr.BaseStream.Seek(200, SeekOrigin.Current); // Skip free inode list
				xnx_sb.s_flock = eabr.ReadByte();
				xnx_sb.s_ilock = eabr.ReadByte();
				xnx_sb.s_fmod = eabr.ReadByte();
				xnx_sb.s_ronly = eabr.ReadByte();
				xnx_sb.s_time = eabr.ReadUInt32();
				xnx_sb.s_tfree = eabr.ReadUInt32();
				xnx_sb.s_tinode = eabr.ReadUInt16();
				xnx_sb.s_cylblks = eabr.ReadUInt16();
				xnx_sb.s_gapblks = eabr.ReadUInt16();
				xnx_sb.s_dinfo0 = eabr.ReadUInt16();
				xnx_sb.s_dinfo1 = eabr.ReadUInt16();
				xnx_sb.s_fname = StringHandlers.CToString(eabr.ReadBytes(6));
				xnx_sb.s_fpack = StringHandlers.CToString(eabr.ReadBytes(6));
				xnx_sb.s_clean = eabr.ReadByte();
				xnx_sb.s_magic = eabr.ReadUInt32();
				eabr.BaseStream.Seek(371, SeekOrigin.Current); // Skip fill zone
				xnx_sb.s_type = eabr.ReadUInt32();

				UInt32 bs = 512;
				sb.AppendLine("XENIX filesystem");
				switch(xnx_sb.s_type)
				{
				case 1:
					sb.AppendLine("512 bytes per block");
					break;
				case 2:
					sb.AppendLine("1024 bytes per block");
					bs=1024;
					break;
				case 3:
					sb.AppendLine("2048 bytes per block");
					bs=2048;
					break;
				default:
					sb.AppendFormat("Unknown s_type value: 0x{0:X8}", xnx_sb.s_type).AppendLine();
					break;
				}
				sb.AppendFormat("{0} zones on volume ({1} bytes)", xnx_sb.s_fsize, xnx_sb.s_fsize*bs).AppendLine();
				sb.AppendFormat("{0} free zones on volume ({1} bytes)", xnx_sb.s_tfree, xnx_sb.s_tfree*bs).AppendLine();
				sb.AppendFormat("{0} free blocks on list ({1} bytes)", xnx_sb.s_nfree, xnx_sb.s_nfree*bs).AppendLine();
				sb.AppendFormat("{0} blocks per cylinder ({1} bytes)", xnx_sb.s_cylblks, xnx_sb.s_cylblks*bs).AppendLine();
				sb.AppendFormat("{0} blocks per gap ({1} bytes)", xnx_sb.s_gapblks, xnx_sb.s_gapblks*bs).AppendLine();
				sb.AppendFormat("First data zone: {0}", xnx_sb.s_isize).AppendLine();
				sb.AppendFormat("{0} free inodes on volume", xnx_sb.s_tinode).AppendLine();
				sb.AppendFormat("{0} free inodes on list", xnx_sb.s_ninode).AppendLine();
				if(xnx_sb.s_flock > 0)
					sb.AppendLine("Free block list is locked");
				if(xnx_sb.s_ilock > 0)
					sb.AppendLine("inode cache is locked");
				if(xnx_sb.s_fmod > 0)
					sb.AppendLine("Superblock is being modified");
				if(xnx_sb.s_ronly > 0)
					sb.AppendLine("Volume is mounted read-only");
				sb.AppendFormat("Superblock last updated on {0}", DateHandlers.UNIXUnsignedToDateTime(xnx_sb.s_time)).AppendLine();
				sb.AppendFormat("Volume name: {0}", xnx_sb.s_fname).AppendLine();
				sb.AppendFormat("Pack name: {0}", xnx_sb.s_fpack).AppendLine();
				if(xnx_sb.s_clean == 0x46)
					sb.AppendLine("Volume is clean");
				else
					sb.AppendLine("Volume is dirty");
			}

			if(sysv)
			{
				eabr = new EndianAwareBinaryReader(stream, littleendian);
				UInt16 pad0, pad1, pad2, pad3;

				eabr.BaseStream.Seek(offset + start*0x200 + 0x002, SeekOrigin.Begin); // First padding
				pad0 = eabr.ReadUInt16();
				eabr.BaseStream.Seek(offset + start*0x200 + 0x00A, SeekOrigin.Begin); // Second padding
				pad1 = eabr.ReadUInt16();
				eabr.BaseStream.Seek(offset + start*0x200 + 0x0D6, SeekOrigin.Begin); // Third padding
				pad2 = eabr.ReadUInt16();
				eabr.BaseStream.Seek(offset + start*0x200 + 0x1B6, SeekOrigin.Begin); // Fourth padding
				pad3 = eabr.ReadUInt16();

				// This detection is not working as expected
				if(pad0 == 0 && pad1 == 0 && pad2 == 0)
					sysvr4 = true;
				else
					sysvr2 = true;

				SystemVRelease4SuperBlock sysv_sb = new SystemVRelease4SuperBlock();
				eabr.BaseStream.Seek(offset + start*0x200, SeekOrigin.Begin);
				sysv_sb.s_isize = eabr.ReadUInt16();
				if(sysvr4)
					eabr.BaseStream.Seek(2, SeekOrigin.Current); // Skip padding
				sysv_sb.s_fsize = eabr.ReadUInt32();
				sysv_sb.s_nfree = eabr.ReadUInt16();
				if(sysvr4)
					eabr.BaseStream.Seek(2, SeekOrigin.Current); // Skip padding
				eabr.BaseStream.Seek(200, SeekOrigin.Current); // Skip free block list
				sysv_sb.s_ninode = eabr.ReadUInt16();
				if(sysvr4)
					eabr.BaseStream.Seek(2, SeekOrigin.Current); // Skip padding
				eabr.BaseStream.Seek(200, SeekOrigin.Current); // Skip free inode list
				sysv_sb.s_flock = eabr.ReadByte();
				sysv_sb.s_ilock = eabr.ReadByte();
				sysv_sb.s_fmod = eabr.ReadByte();
				sysv_sb.s_ronly = eabr.ReadByte();
				sysv_sb.s_time = eabr.ReadUInt32();
				sysv_sb.s_cylblks = eabr.ReadUInt16();
				sysv_sb.s_gapblks = eabr.ReadUInt16();
				sysv_sb.s_dinfo0 = eabr.ReadUInt16();
				sysv_sb.s_dinfo1 = eabr.ReadUInt16();
				sysv_sb.s_tfree = eabr.ReadUInt32();
				sysv_sb.s_tinode = eabr.ReadUInt16();
				if(sysvr4 && pad3 == 0)
					eabr.BaseStream.Seek(2, SeekOrigin.Current); // Skip padding
				sysv_sb.s_fname = StringHandlers.CToString(eabr.ReadBytes(6));
				sysv_sb.s_fpack = StringHandlers.CToString(eabr.ReadBytes(6));
				if(sysvr4 && pad3 == 0)
					eabr.BaseStream.Seek(50, SeekOrigin.Current); // Skip fill zone
				else if(sysvr4)
					eabr.BaseStream.Seek(50, SeekOrigin.Current); // Skip fill zone
				else
					eabr.BaseStream.Seek(56, SeekOrigin.Current); // Skip fill zone
				sysv_sb.s_state = eabr.ReadUInt32();
				sysv_sb.s_magic = eabr.ReadUInt32();
				sysv_sb.s_type = eabr.ReadUInt32();

				UInt32 bs = 512;
				if(sysvr4)
					sb.AppendLine("System V Release 4 filesystem");
				else
					sb.AppendLine("System V Release 2 filesystem");
				switch(sysv_sb.s_type)
				{
				case 1:
					sb.AppendLine("512 bytes per block");
					break;
				case 2:
					sb.AppendLine("1024 bytes per block");
					bs = 1024;
					break;
				case 3:
					sb.AppendLine("2048 bytes per block");
					bs = 2048;
					break;
				default:
					sb.AppendFormat("Unknown s_type value: 0x{0:X8}", sysv_sb.s_type).AppendLine();
					break;
				}
				sb.AppendFormat("{0} zones on volume ({1} bytes)", sysv_sb.s_fsize, sysv_sb.s_fsize*bs).AppendLine();
				sb.AppendFormat("{0} free zones on volume ({1} bytes)", sysv_sb.s_tfree, sysv_sb.s_tfree*bs).AppendLine();
				sb.AppendFormat("{0} free blocks on list ({1} bytes)", sysv_sb.s_nfree, sysv_sb.s_nfree*bs).AppendLine();
				sb.AppendFormat("{0} blocks per cylinder ({1} bytes)", sysv_sb.s_cylblks, sysv_sb.s_cylblks*bs).AppendLine();
				sb.AppendFormat("{0} blocks per gap ({1} bytes)", sysv_sb.s_gapblks, sysv_sb.s_gapblks*bs).AppendLine();
				sb.AppendFormat("First data zone: {0}", sysv_sb.s_isize).AppendLine();
				sb.AppendFormat("{0} free inodes on volume", sysv_sb.s_tinode).AppendLine();
				sb.AppendFormat("{0} free inodes on list", sysv_sb.s_ninode).AppendLine();
				if(sysv_sb.s_flock > 0)
					sb.AppendLine("Free block list is locked");
				if(sysv_sb.s_ilock > 0)
					sb.AppendLine("inode cache is locked");
				if(sysv_sb.s_fmod > 0)
					sb.AppendLine("Superblock is being modified");
				if(sysv_sb.s_ronly > 0)
					sb.AppendLine("Volume is mounted read-only");
				sb.AppendFormat("Superblock last updated on {0}", DateHandlers.UNIXUnsignedToDateTime(sysv_sb.s_time)).AppendLine();
				sb.AppendFormat("Volume name: {0}", sysv_sb.s_fname).AppendLine();
				sb.AppendFormat("Pack name: {0}", sysv_sb.s_fpack).AppendLine();
				if(sysv_sb.s_state == (0x7C269D38 - sysv_sb.s_time))
					sb.AppendLine("Volume is clean");
				else
					sb.AppendLine("Volume is dirty");
			}

			if(coherent)
			{
				eabr = new EndianAwareBinaryReader(stream, true);
				CoherentSuperBlock coh_sb = new CoherentSuperBlock();
				eabr.BaseStream.Seek(offset + start*0x200, SeekOrigin.Begin);
				coh_sb.s_isize = eabr.ReadUInt16();
				coh_sb.s_fsize = Swapping.PDPFromLittleEndian(eabr.ReadUInt32());
				coh_sb.s_nfree = eabr.ReadUInt16();
				eabr.BaseStream.Seek(256, SeekOrigin.Current); // Skip free block list
				coh_sb.s_ninode = eabr.ReadUInt16();
				eabr.BaseStream.Seek(200, SeekOrigin.Current); // Skip free inode list
				coh_sb.s_flock = eabr.ReadByte();
				coh_sb.s_ilock = eabr.ReadByte();
				coh_sb.s_fmod = eabr.ReadByte();
				coh_sb.s_ronly = eabr.ReadByte();
				coh_sb.s_time = Swapping.PDPFromLittleEndian(eabr.ReadUInt32());
				coh_sb.s_tfree = Swapping.PDPFromLittleEndian(eabr.ReadUInt32());
				coh_sb.s_tinode = eabr.ReadUInt16();
				coh_sb.s_int_m = eabr.ReadUInt16();
				coh_sb.s_int_n = eabr.ReadUInt16();
				coh_sb.s_fname = StringHandlers.CToString(eabr.ReadBytes(6));
				coh_sb.s_fpack = StringHandlers.CToString(eabr.ReadBytes(6));

				sb.AppendLine("Coherent UNIX filesystem");
				sb.AppendFormat("{0} zones on volume ({1} bytes)", coh_sb.s_fsize, coh_sb.s_fsize*512).AppendLine();
				sb.AppendFormat("{0} free zones on volume ({1} bytes)", coh_sb.s_tfree, coh_sb.s_tfree*512).AppendLine();
				sb.AppendFormat("{0} free blocks on list ({1} bytes)", coh_sb.s_nfree, coh_sb.s_nfree*512).AppendLine();
				sb.AppendFormat("First data zone: {0}", coh_sb.s_isize).AppendLine();
				sb.AppendFormat("{0} free inodes on volume", coh_sb.s_tinode).AppendLine();
				sb.AppendFormat("{0} free inodes on list", coh_sb.s_ninode).AppendLine();
				if(coh_sb.s_flock > 0)
					sb.AppendLine("Free block list is locked");
				if(coh_sb.s_ilock > 0)
					sb.AppendLine("inode cache is locked");
				if(coh_sb.s_fmod > 0)
					sb.AppendLine("Superblock is being modified");
				if(coh_sb.s_ronly > 0)
					sb.AppendLine("Volume is mounted read-only");
				sb.AppendFormat("Superblock last updated on {0}", DateHandlers.UNIXUnsignedToDateTime(coh_sb.s_time)).AppendLine();
				sb.AppendFormat("Volume name: {0}", coh_sb.s_fname).AppendLine();
				sb.AppendFormat("Pack name: {0}", coh_sb.s_fpack).AppendLine();
			}

			if(sys7th)
			{
				eabr = new EndianAwareBinaryReader(stream, littleendian);
				UNIX7thEditionSuperBlock v7_sb = new UNIX7thEditionSuperBlock();
				eabr.BaseStream.Seek(offset + start*0x200, SeekOrigin.Begin);
				v7_sb.s_isize = eabr.ReadUInt16();
				v7_sb.s_fsize = eabr.ReadUInt32();
				v7_sb.s_nfree = eabr.ReadUInt16();
				eabr.BaseStream.Seek(200, SeekOrigin.Current); // Skip free block list
				v7_sb.s_ninode = eabr.ReadUInt16();
				eabr.BaseStream.Seek(200, SeekOrigin.Current); // Skip free inode list
				v7_sb.s_flock = eabr.ReadByte();
				v7_sb.s_ilock = eabr.ReadByte();
				v7_sb.s_fmod = eabr.ReadByte();
				v7_sb.s_ronly = eabr.ReadByte();
				v7_sb.s_time = eabr.ReadUInt32();
				v7_sb.s_tfree = eabr.ReadUInt32();
				v7_sb.s_tinode = eabr.ReadUInt16();
				v7_sb.s_int_m = eabr.ReadUInt16();
				v7_sb.s_int_n = eabr.ReadUInt16();
				v7_sb.s_fname = StringHandlers.CToString(eabr.ReadBytes(6));
				v7_sb.s_fpack = StringHandlers.CToString(eabr.ReadBytes(6));

				sb.AppendLine("UNIX 7th Edition filesystem");
				sb.AppendFormat("{0} zones on volume ({1} bytes)", v7_sb.s_fsize, v7_sb.s_fsize*512).AppendLine();
				sb.AppendFormat("{0} free zones on volume ({1} bytes)", v7_sb.s_tfree, v7_sb.s_tfree*512).AppendLine();
				sb.AppendFormat("{0} free blocks on list ({1} bytes)", v7_sb.s_nfree, v7_sb.s_nfree*512).AppendLine();
				sb.AppendFormat("First data zone: {0}", v7_sb.s_isize).AppendLine();
				sb.AppendFormat("{0} free inodes on volume", v7_sb.s_tinode).AppendLine();
				sb.AppendFormat("{0} free inodes on list", v7_sb.s_ninode).AppendLine();
				if(v7_sb.s_flock > 0)
					sb.AppendLine("Free block list is locked");
				if(v7_sb.s_ilock > 0)
					sb.AppendLine("inode cache is locked");
				if(v7_sb.s_fmod > 0)
					sb.AppendLine("Superblock is being modified");
				if(v7_sb.s_ronly > 0)
					sb.AppendLine("Volume is mounted read-only");
				sb.AppendFormat("Superblock last updated on {0}", DateHandlers.UNIXUnsignedToDateTime(v7_sb.s_time)).AppendLine();
				sb.AppendFormat("Volume name: {0}", v7_sb.s_fname).AppendLine();
				sb.AppendFormat("Pack name: {0}", v7_sb.s_fpack).AppendLine();
			}

			information = sb.ToString();
		}

		private struct XenixSuperBlock
		{
			public UInt16 s_isize;   // 0x000, index of first data zone
			public UInt32 s_fsize;   // 0x002, total number of zones of this volume
			// the start of the free block list:
			public UInt16 s_nfree;   // 0x006, blocks in s_free, <=100
			public UInt32[] s_free;  // 0x008, 100 entries, first free block list chunk
			// the cache of free inodes:
			public UInt16 s_ninode;  // 0x198, number of inodes in s_inode, <= 100
			public UInt16[] s_inode; // 0x19A, 100 entries, some free inodes 
			public byte s_flock;     // 0x262, free block list manipulation lock
			public byte s_ilock;     // 0x263, inode cache manipulation lock
			public byte s_fmod;      // 0x264, superblock modification flag
			public byte s_ronly;     // 0x265, read-only mounted flag
			public UInt32 s_time;    // 0x266, time of last superblock update
			public UInt32 s_tfree;   // 0x26A, total number of free zones
			public UInt16 s_tinode;  // 0x26E, total number of free inodes
			public UInt16 s_cylblks; // 0x270, blocks per cylinder
			public UInt16 s_gapblks; // 0x272, blocks per gap
			public UInt16 s_dinfo0;  // 0x274, device information ??
			public UInt16 s_dinfo1;  // 0x276, device information ??
			public string s_fname;   // 0x278, 6 bytes, volume name
			public string s_fpack;   // 0x27E, 6 bytes, pack name
			public byte   s_clean;   // 0x284, 0x46 if volume is clean
			public byte[] s_fill;    // 0x285, 371 bytes
			public UInt32 s_magic;   // 0x3F8, magic
			public UInt32 s_type;    // 0x3FC, filesystem type (1 = 512 bytes/blk, 2 = 1024 bytes/blk, 3 = 2048 bytes/blk)
		}

		private struct SystemVRelease4SuperBlock
		{
			public UInt16 s_isize;   // 0x000, index of first data zone
			public UInt16 s_pad0;    // 0x002, padding
			public UInt32 s_fsize;   // 0x004, total number of zones of this volume
			// the start of the free block list:
			public UInt16 s_nfree;   // 0x008, blocks in s_free, <=100
			public UInt16 s_pad1;    // 0x00A, padding
			public UInt32[] s_free;  // 0x00C, 50 entries, first free block list chunk
			// the cache of free inodes:
			public UInt16 s_ninode;  // 0x0D4, number of inodes in s_inode, <= 100
			public UInt16 s_pad2;    // 0x0D6, padding
			public UInt16[] s_inode; // 0x0D8, 100 entries, some free inodes 
			public byte s_flock;     // 0x1A0, free block list manipulation lock
			public byte s_ilock;     // 0x1A1, inode cache manipulation lock
			public byte s_fmod;      // 0x1A2, superblock modification flag
			public byte s_ronly;     // 0x1A3, read-only mounted flag
			public UInt32 s_time;    // 0x1A4, time of last superblock update
			public UInt16 s_cylblks; // 0x1A8, blocks per cylinder
			public UInt16 s_gapblks; // 0x1AA, blocks per gap
			public UInt16 s_dinfo0;  // 0x1AC, device information ??
			public UInt16 s_dinfo1;  // 0x1AE, device information ??
			public UInt32 s_tfree;   // 0x1B0, total number of free zones
			public UInt16 s_tinode;  // 0x1B4, total number of free inodes
			public UInt16 s_pad3;    // 0x1B6, padding
			public string s_fname;   // 0x1B8, 6 bytes, volume name
			public string s_fpack;   // 0x1BE, 6 bytes, pack name
			public byte[] s_fill;    // 0x1C4, 48 bytes
			public UInt32 s_state;   // 0x1F4, if s_state == (0x7C269D38 - s_time) then filesystem is clean
			public UInt32 s_magic;   // 0x1F8, magic
			public UInt32 s_type;    // 0x1FC, filesystem type (1 = 512 bytes/blk, 2 = 1024 bytes/blk)
		}

		private struct SystemVRelease2SuperBlock
		{
			public UInt16 s_isize;   // 0x000, index of first data zone
			public UInt32 s_fsize;   // 0x002, total number of zones of this volume
			// the start of the free block list:
			public UInt16 s_nfree;   // 0x006, blocks in s_free, <=100
			public UInt32[] s_free;  // 0x008, 50 entries, first free block list chunk
			// the cache of free inodes:
			public UInt16 s_ninode;  // 0x0D0, number of inodes in s_inode, <= 100
			public UInt16[] s_inode; // 0x0D2, 100 entries, some free inodes 
			public byte s_flock;     // 0x19A, free block list manipulation lock
			public byte s_ilock;     // 0x19B, inode cache manipulation lock
			public byte s_fmod;      // 0x19C, superblock modification flag
			public byte s_ronly;     // 0x19D, read-only mounted flag
			public UInt32 s_time;    // 0x19E, time of last superblock update
			public UInt16 s_cylblks; // 0x1A2, blocks per cylinder
			public UInt16 s_gapblks; // 0x1A4, blocks per gap
			public UInt16 s_dinfo0;  // 0x1A6, device information ??
			public UInt16 s_dinfo1;  // 0x1A8, device information ??
			public UInt32 s_tfree;   // 0x1AA, total number of free zones
			public UInt16 s_tinode;  // 0x1AE, total number of free inodes
			public string s_fname;   // 0x1B0, 6 bytes, volume name
			public string s_fpack;   // 0x1B6, 6 bytes, pack name
			public byte[] s_fill;    // 0x1BC, 56 bytes
			public UInt32 s_state;   // 0x1F4, if s_state == (0x7C269D38 - s_time) then filesystem is clean
			public UInt32 s_magic;   // 0x1F8, magic
			public UInt32 s_type;    // 0x1FC, filesystem type (1 = 512 bytes/blk, 2 = 1024 bytes/blk)
		}

		private struct UNIX7thEditionSuperBlock
		{
			public UInt16 s_isize;   // 0x000, index of first data zone
			public UInt32 s_fsize;   // 0x002, total number of zones of this volume
			// the start of the free block list:
			public UInt16 s_nfree;   // 0x006, blocks in s_free, <=100
			public UInt32[] s_free;  // 0x008, 50 entries, first free block list chunk
			// the cache of free inodes:
			public UInt16 s_ninode;  // 0x0D0, number of inodes in s_inode, <= 100
			public UInt16[] s_inode; // 0x0D2, 100 entries, some free inodes 
			public byte s_flock;     // 0x19A, free block list manipulation lock
			public byte s_ilock;     // 0x19B, inode cache manipulation lock
			public byte s_fmod;      // 0x19C, superblock modification flag
			public byte s_ronly;     // 0x19D, read-only mounted flag
			public UInt32 s_time;    // 0x19E, time of last superblock update
			public UInt32 s_tfree;   // 0x1A2, total number of free zones
			public UInt16 s_tinode;  // 0x1A6, total number of free inodes
			public UInt16 s_int_m;   // 0x1A8, interleave factor
			public UInt16 s_int_n;   // 0x1AA, interleave factor
			public string s_fname;   // 0x1AC, 6 bytes, volume name
			public string s_fpack;   // 0x1B2, 6 bytes, pack name
		}

		private struct CoherentSuperBlock
		{
			public UInt16 s_isize;   // 0x000, index of first data zone
			public UInt32 s_fsize;   // 0x002, total number of zones of this volume
			// the start of the free block list:
			public UInt16 s_nfree;   // 0x006, blocks in s_free, <=100
			public UInt32[] s_free;  // 0x008, 64 entries, first free block list chunk
			// the cache of free inodes:
			public UInt16 s_ninode;  // 0x108, number of inodes in s_inode, <= 100
			public UInt16[] s_inode; // 0x10A, 100 entries, some free inodes 
			public byte s_flock;     // 0x1D2, free block list manipulation lock
			public byte s_ilock;     // 0x1D3, inode cache manipulation lock
			public byte s_fmod;      // 0x1D4, superblock modification flag
			public byte s_ronly;     // 0x1D5, read-only mounted flag
			public UInt32 s_time;    // 0x1D6, time of last superblock update
			public UInt32 s_tfree;   // 0x1DE, total number of free zones
			public UInt16 s_tinode;  // 0x1E2, total number of free inodes
			public UInt16 s_int_m;   // 0x1E4, interleave factor
			public UInt16 s_int_n;   // 0x1E6, interleave factor
			public string s_fname;   // 0x1E8, 6 bytes, volume name
			public string s_fpack;   // 0x1EE, 6 bytes, pack name
			public UInt32 s_unique;  // 0x1F4, zero-filled
		}
	}
}

