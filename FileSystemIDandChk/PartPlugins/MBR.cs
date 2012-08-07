using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using FileSystemIDandChk;

namespace FileSystemIDandChk.PartPlugins
{
	class MBR : PartPlugin
	{
		public MBR (PluginBase Core)
		{
            base.Name = "Master Boot Record";
			base.PluginUUID = new Guid("5E8A34E8-4F1A-59E6-4BF7-7EA647063A76");
		}
		
		public override bool GetInformation (FileStream stream, out List<Partition> partitions)
		{
			byte cyl_sect1, cyl_sect2; // For decoding cylinder and sector
			UInt16 signature;
			UInt32 serial;
			ulong counter = 0;
			
			partitions = new List<Partition>();

			BinaryReader br = new BinaryReader(stream);

			br.BaseStream.Seek(0x01FE, SeekOrigin.Begin);
			signature = br.ReadUInt16();

			if(signature != 0xAA55)
				return false; // Not MBR
			
			br.BaseStream.Seek(0x01B8, SeekOrigin.Begin);
			serial = br.ReadUInt32(); // Not useful right now
			
			br.BaseStream.Seek(0x01BE, SeekOrigin.Begin);
			for(int i = 0; i < 4; i ++)
			{
				MBRPartitionEntry entry = new MBRPartitionEntry();
				
				entry.status = br.ReadByte();
				entry.start_head = br.ReadByte();
				
				cyl_sect1 = br.ReadByte();
				cyl_sect2 = br.ReadByte();
				
				entry.start_sector = (byte)(cyl_sect1 & 0x3F);
				entry.start_cylinder = (ushort)(((cyl_sect1 & 0xC0) << 2) | cyl_sect2);
				
				entry.type = br.ReadByte();
				entry.end_head = br.ReadByte();

				cyl_sect1 = br.ReadByte();
				cyl_sect2 = br.ReadByte();
				
				entry.end_sector = (byte)(cyl_sect1 & 0x3F);
				entry.end_cylinder = (ushort)(((cyl_sect1 & 0xC0) << 2) | cyl_sect2);
				
				entry.lba_start = br.ReadUInt32();
				entry.lba_sectors = br.ReadUInt32();
				
				// Let's start the fun...
				
				bool valid = true;
				bool extended = false;
				bool disklabel = false;

				if(entry.status != 0x00 && entry.status != 0x80)
					return false; // Maybe a FAT filesystem
				if(entry.type == 0x00)
					valid = false;
				if(entry.type == 0xEE || entry.type == 0xEF)
					return false; // This is a GPT
				if(entry.type == 0x05 || entry.type == 0x0F || entry.type == 0x85)
				{
					valid = false;
					extended = true; // Extended partition
				}
				if(entry.type == 0x82 || entry.type == 0xBF || entry.type == 0xA5 || entry.type == 0xA6 || entry.type == 0xA9 ||
				   entry.type == 0xB7 || entry.type == 0x81 || entry.type == 0x63)
				{
					valid = false;
					disklabel = true;
				}
				
				if(disklabel)
				{
					long currentPos = br.BaseStream.Position;
					long disklabel_start = entry.lba_start * 512;
					
					br.BaseStream.Seek(disklabel_start, SeekOrigin.Begin);
					
					switch(entry.type)
					{
						case 0xA5:
						case 0xA6:
						case 0xA9:
						case 0xB7: // BSD disklabels
						{
							UInt32 magic;
							magic = br.ReadUInt32();
							
							if(magic == 0x82564557)
							{
								br.BaseStream.Seek(126, SeekOrigin.Current);
								UInt16 no_parts = br.ReadUInt16();
								br.BaseStream.Seek(8, SeekOrigin.Current);
							
								for(int j = 0; j < no_parts; j++)
								{
									Partition part = new Partition();
									byte bsd_type;
								
									part.PartitionLength = br.ReadUInt32()*512;
									part.PartitionStart = br.ReadUInt32()*512;
									br.BaseStream.Seek(4, SeekOrigin.Current);
									bsd_type = br.ReadByte();
									br.BaseStream.Seek(3, SeekOrigin.Current);
								
									part.PartitionType = String.Format("BSD: {0}", bsd_type);
									part.PartitionName = decodeBSDType(bsd_type);
								
									part.PartitionSequence = counter;
									part.PartitionDescription = "Partition inside a BSD disklabel.";
								
									if(bsd_type!=0)
									{
										partitions.Add(part);
										counter++;
									}
								}
							}
							else
								valid=true;
							break;
						}
						case 0x63: // UNIX disklabel
						{
							UInt32 magic;
							br.BaseStream.Seek(29*0x200 + 4, SeekOrigin.Current); // Starts on sector 29 of partition
							magic = br.ReadUInt32();
						
							if(magic == UNIXDiskLabel_MAGIC)
							{
								UNIXDiskLabel dl = new UNIXDiskLabel();
								bool isNewDL = false;

								dl.version = br.ReadUInt32();
								dl.serial = StringHandlers.CToString(br.ReadBytes(12));
								dl.cyls = br.ReadUInt32();
								dl.trks = br.ReadUInt32();
								dl.secs = br.ReadUInt32();
								dl.bps = br.ReadUInt32();
								dl.start = br.ReadUInt32();
								dl.unknown1 = br.ReadBytes(48);
								dl.alt_tbl = br.ReadUInt32();
								dl.alt_len = br.ReadUInt32();
								dl.phys_cyl = br.ReadUInt32();

								if(dl.phys_cyl != UNIXVTOC_MAGIC) // Old version VTOC starts here
								{
									isNewDL = true;
									dl.phys_trk = br.ReadUInt32();
									dl.phys_sec = br.ReadUInt32();
									dl.phys_bytes = br.ReadUInt32();
									dl.unknown2 = br.ReadUInt32();
									dl.unknown3 = br.ReadUInt32();
									dl.pad = br.ReadBytes(48);
								}
								else
									br.BaseStream.Seek(-4, SeekOrigin.Current); // Return to old VTOC magic

								UNIXVTOC vtoc = new UNIXVTOC();

								vtoc.magic = br.ReadUInt32();
								
								if(vtoc.magic == UNIXVTOC_MAGIC)
								{
									vtoc.version = br.ReadUInt32();
									vtoc.name = StringHandlers.CToString(br.ReadBytes(8));
									vtoc.slices = br.ReadUInt16();
									vtoc.unknown = br.ReadUInt16();
									vtoc.reserved = br.ReadBytes(40);
							
									for(int j = 0; j < vtoc.slices; j++)
									{
										UNIXVTOCEntry vtoc_ent = new UNIXVTOCEntry();

										vtoc_ent.tag = br.ReadUInt16();
										vtoc_ent.flags = br.ReadUInt16();
										vtoc_ent.start = br.ReadUInt32();
										vtoc_ent.length = br.ReadUInt32();

										if((vtoc_ent.flags & 0x200) == 0x200 && vtoc_ent.tag != UNIX_TAG_EMPTY && vtoc_ent.tag != UNIX_TAG_WHOLE)
										{
											Partition part = new Partition();
											part.PartitionStart = vtoc_ent.start * dl.bps;
											part.PartitionLength = vtoc_ent.length * dl.bps;
											part.PartitionSequence = counter;
											part.PartitionType = String.Format("UNIX: {0}", decodeUNIXTAG(vtoc_ent.tag, isNewDL));

											string info = "";

											if((vtoc_ent.flags & 0x01) == 0x01)
												info += " (do not mount)";
											if((vtoc_ent.flags & 0x10) == 0x10)
												info += " (do not mount)";

											part.PartitionDescription = "UNIX slice" + info + ".";
									
											partitions.Add(part);
											counter++;
										}
									}
								}
							}
							else
								valid = true;
							break;
						}
						case 0x82:
						case 0xBF: // Solaris disklabel
						{
							UInt32 magic;
							UInt32 version;
							br.BaseStream.Seek(12, SeekOrigin.Current);
							magic = br.ReadUInt32();
							version = br.ReadUInt32();
						
							if(magic == 0x600DDEEE && version == 1)
							{
								br.BaseStream.Seek(52, SeekOrigin.Current);
								for(int j = 0; j < 16; j++)
								{
									Partition part = new Partition();
									br.BaseStream.Seek(4, SeekOrigin.Current);
									part.PartitionStart = (entry.lba_start + br.ReadInt32()) * 512;
									part.PartitionLength = br.ReadInt32()*512;
									part.PartitionDescription = "Solaris slice.";
									
									part.PartitionSequence = counter;
								
									if(part.PartitionLength > 0)
									{
										partitions.Add(part);
										counter++;
									}
								}
							}
							else
								valid = true;
							break;
						}
					case 0x81: // Minix subpartitions
						{
							bool minix_subs = false;
							byte type;
						
							br.BaseStream.Seek(0x01BE, SeekOrigin.Current);
							for(int j = 0; j < 4; j++)
							{
								br.BaseStream.Seek(4, SeekOrigin.Current);
								type = br.ReadByte();
								
								if(type==0x81)
								{
									Partition part = new Partition();
									minix_subs = true;
									br.BaseStream.Seek(3, SeekOrigin.Current);
									part.PartitionDescription = "Minix subpartition";
									part.PartitionType = "Minix";
									part.PartitionStart = br.ReadUInt32()*512;
									part.PartitionLength = br.ReadUInt32()*512;
									part.PartitionSequence = counter;
									partitions.Add(part);
									counter++;
								}
							}
							if(!minix_subs)
								valid = true;
							
							break;
						}
						default:
							valid = true;
							break;
					}
					
					br.BaseStream.Seek(currentPos, SeekOrigin.Begin);
				}
				
				if(valid)
				{
					Partition part = new Partition();
					if(entry.lba_start > 0 && entry.lba_sectors > 0)
					{
						part.PartitionStart = (long)entry.lba_start * 512;
						part.PartitionLength = (long)entry.lba_sectors * 512;
					}
/*					else if(entry.start_head < 255 && entry.end_head < 255 &&
					        entry.start_sector > 0 && entry.start_sector < 64 &&
					        entry.end_sector > 0 && entry.end_sector < 64 &&
					        entry.start_cylinder < 1024 && entry.end_cylinder < 1024)
					{
						
					} */ // As we don't know the maxium cyl, head or sect of the device we need LBA
					else
						valid = false;
					
					if(valid)
					{
						part.PartitionType = String.Format("0x{0:X2}", entry.type);
						part.PartitionName = decodeMBRType(entry.type);
						part.PartitionSequence = counter;
						if(entry.status==0x80)
							part.PartitionDescription = "Partition is bootable.";
						else
							part.PartitionDescription = "";
						
						counter++;
						
						partitions.Add(part);
					}
				}
				
				if(extended) // Let's extend the fun
				{
					long pre_ext_Pos = br.BaseStream.Position;
					bool ext_valid = true;
					bool ext_disklabel = false;
					bool processing_extended = true;
					
					br.BaseStream.Seek((long)entry.lba_start*512, SeekOrigin.Begin);

					while(processing_extended)
					{
						br.BaseStream.Seek(0x01BE, SeekOrigin.Current);
						
						for(int l = 0; l < 2; l++)
						{
							bool ext_extended = false;
							
							MBRPartitionEntry entry2 = new MBRPartitionEntry();
					
							entry2.status = br.ReadByte();
							entry2.start_head = br.ReadByte();
							
							cyl_sect1 = br.ReadByte();
							cyl_sect2 = br.ReadByte();
							
							entry2.start_sector = (byte)(cyl_sect1 & 0x3F);
							entry2.start_cylinder = (ushort)(((cyl_sect1 & 0xC0) << 2) | cyl_sect2);
							
							entry2.type = br.ReadByte();
							entry2.end_head = br.ReadByte();
			
							cyl_sect1 = br.ReadByte();
							cyl_sect2 = br.ReadByte();
							
							entry2.end_sector = (byte)(cyl_sect1 & 0x3F);
							entry2.end_cylinder = (ushort)(((cyl_sect1 & 0xC0) << 2) | cyl_sect2);
							
							entry2.lba_start = br.ReadUInt32() + entry.lba_start;
							entry2.lba_sectors = br.ReadUInt32();
							
							// Let's start the fun...
							
							if(entry2.status != 0x00 && entry2.status != 0x80)
								ext_valid = false;
							if(entry2.type == 0x00)
								valid = false;
							if(entry2.type == 0x82 || entry2.type == 0xBF || entry2.type == 0xA5 || entry2.type == 0xA6 ||
							   entry2.type == 0xA9 || entry2.type == 0xB7 || entry2.type == 0x81 || entry2.type == 0x63)
							{
								ext_valid = false;
								ext_disklabel = true;
							}
							if(entry2.type == 0x05 || entry2.type == 0x0F || entry2.type == 0x85)
							{
								ext_valid = false;
								ext_disklabel = false;
								ext_extended = true; // Extended partition
							}
							else if(l==1)
								processing_extended=false;
							
							if(ext_disklabel)
							{
								long currentPos = br.BaseStream.Position;
								long disklabel_start = entry2.lba_start * 512;
								
								br.BaseStream.Seek(disklabel_start, SeekOrigin.Begin);
								
								switch(entry2.type)
								{
									case 0xA5:
									case 0xA6:
									case 0xA9:
									case 0xB7: // BSD disklabels
									{
										UInt32 magic;
										magic = br.ReadUInt32();
										
										if(magic == 0x82564557)
										{
											br.BaseStream.Seek(126, SeekOrigin.Current);
											UInt16 no_parts = br.ReadUInt16();
											br.BaseStream.Seek(8, SeekOrigin.Current);
										
											for(int j = 0; j < no_parts; j++)
											{
												Partition part = new Partition();
												byte bsd_type;
											
												part.PartitionLength = br.ReadUInt32()*512;
												part.PartitionStart = br.ReadUInt32()*512;
												br.BaseStream.Seek(4, SeekOrigin.Current);
												bsd_type = br.ReadByte();
												br.BaseStream.Seek(3, SeekOrigin.Current);
											
												part.PartitionType = String.Format("BSD: {0}", bsd_type);
												part.PartitionName = decodeBSDType(bsd_type);
											
												part.PartitionSequence = counter;
												part.PartitionDescription = "Partition inside a BSD disklabel.";
											
												if(bsd_type!=0)
												{
													partitions.Add(part);
													counter++;
												}
											}
										}
										else
											ext_valid=true;
										break;
									}
									case 0x63: // UNIX disklabel
									{
										UInt32 magic;
										br.BaseStream.Seek(29*0x200 + 4, SeekOrigin.Current); // Starts on sector 29 of partition
										magic = br.ReadUInt32();
										
										if(magic == UNIXDiskLabel_MAGIC)
										{
											UNIXDiskLabel dl = new UNIXDiskLabel();
											bool isNewDL = false;
											
											dl.version = br.ReadUInt32();
											dl.serial = StringHandlers.CToString(br.ReadBytes(12));
											dl.cyls = br.ReadUInt32();
											dl.trks = br.ReadUInt32();
											dl.secs = br.ReadUInt32();
											dl.bps = br.ReadUInt32();
											dl.start = br.ReadUInt32();
											dl.unknown1 = br.ReadBytes(48);
											dl.alt_tbl = br.ReadUInt32();
											dl.alt_len = br.ReadUInt32();
											dl.phys_cyl = br.ReadUInt32();
											
											if(dl.phys_cyl != UNIXVTOC_MAGIC) // Old version VTOC starts here
											{
												isNewDL = true;
												dl.phys_trk = br.ReadUInt32();
												dl.phys_sec = br.ReadUInt32();
												dl.phys_bytes = br.ReadUInt32();
												dl.unknown2 = br.ReadUInt32();
												dl.unknown3 = br.ReadUInt32();
												dl.pad = br.ReadBytes(48);
											}
											else
												br.BaseStream.Seek(-4, SeekOrigin.Current); // Return to old VTOC magic
											
											UNIXVTOC vtoc = new UNIXVTOC();
											
											vtoc.magic = br.ReadUInt32();
											
											if(vtoc.magic == UNIXVTOC_MAGIC)
											{
												vtoc.version = br.ReadUInt32();
												vtoc.name = StringHandlers.CToString(br.ReadBytes(8));
												vtoc.slices = br.ReadUInt16();
												vtoc.unknown = br.ReadUInt16();
												vtoc.reserved = br.ReadBytes(40);
												
												for(int j = 0; j < vtoc.slices; j++)
												{
													UNIXVTOCEntry vtoc_ent = new UNIXVTOCEntry();
													
													vtoc_ent.tag = br.ReadUInt16();
													vtoc_ent.flags = br.ReadUInt16();
													vtoc_ent.start = br.ReadUInt32();
													vtoc_ent.length = br.ReadUInt32();
													
													if((vtoc_ent.flags & 0x200) == 0x200 && vtoc_ent.tag != UNIX_TAG_EMPTY && vtoc_ent.tag != UNIX_TAG_WHOLE)
													{
														Partition part = new Partition();
														part.PartitionStart = vtoc_ent.start * dl.bps;
														part.PartitionLength = vtoc_ent.length * dl.bps;
														part.PartitionSequence = counter;
														part.PartitionType = String.Format("UNIX: {0}", decodeUNIXTAG(vtoc_ent.tag, isNewDL));
														
														string info = "";
														
														if((vtoc_ent.flags & 0x01) == 0x01)
															info += " (do not mount)";
														if((vtoc_ent.flags & 0x10) == 0x10)
															info += " (do not mount)";
														
														part.PartitionDescription = "UNIX slice" + info + ".";
														
														partitions.Add(part);
														counter++;
													}
												}
											}
										}
										else
											ext_valid = true;
										break;
									}
									case 0x82:
									case 0xBF: // Solaris disklabel
									{
										UInt32 magic;
										UInt32 version;
										br.BaseStream.Seek(12, SeekOrigin.Current);
										magic = br.ReadUInt32();
										version = br.ReadUInt32();
									
										if(magic == 0x600DDEEE && version == 1)
										{
											br.BaseStream.Seek(52, SeekOrigin.Current);
											for(int j = 0; j < 16; j++)
											{
												Partition part = new Partition();
												br.BaseStream.Seek(4, SeekOrigin.Current);
												part.PartitionStart = (entry2.lba_start + br.ReadInt32()) * 512;
												part.PartitionLength = br.ReadInt32()*512;
												part.PartitionDescription = "Solaris slice.";
												
												part.PartitionSequence = counter;
											
												if(part.PartitionLength > 0)
												{
													partitions.Add(part);
													counter++;
												}
											}
										}
										else
											ext_valid = true;
										break;
									}
								case 0x81: // Minix subpartitions
									{
										bool minix_subs = false;
										byte type;
									
										br.BaseStream.Seek(0x01BE, SeekOrigin.Current);
										for(int j = 0; j < 4; j++)
										{
											br.BaseStream.Seek(4, SeekOrigin.Current);
											type = br.ReadByte();
											
											if(type==0x81)
											{
												Partition part = new Partition();
												minix_subs = true;
												br.BaseStream.Seek(3, SeekOrigin.Current);
												part.PartitionDescription = "Minix subpartition";
												part.PartitionType = "Minix";
												part.PartitionStart = br.ReadUInt32()*512;
												part.PartitionLength = br.ReadUInt32()*512;
												part.PartitionSequence = counter;
												partitions.Add(part);
												counter++;
											}
										}
										if(!minix_subs)
											ext_valid = true;
										
										break;
									}
									default:
										ext_valid = true;
										break;
								}
								
								br.BaseStream.Seek(currentPos, SeekOrigin.Begin);
							}
							
							if(ext_valid)
							{
								Partition part = new Partition();
								if(entry2.lba_start > 0 && entry2.lba_sectors > 0)
								{
									part.PartitionStart = (long)entry2.lba_start * 512;
									part.PartitionLength = (long)entry2.lba_sectors * 512;
									Console.WriteLine("{0} start", entry2.lba_start);
								}
			/*					else if(entry2.start_head < 255 && entry2.end_head < 255 &&
								        entry2.start_sector > 0 && entry2.start_sector < 64 &&
								        entry2.end_sector > 0 && entry2.end_sector < 64 &&
								        entry2.start_cylinder < 1024 && entry2.end_cylinder < 1024)
								{
									
								} */ // As we don't know the maxium cyl, head or sect of the device we need LBA
								else
									ext_valid = false;
								
								if(ext_valid)
								{
									part.PartitionType = String.Format("0x{0:X2}", entry2.type);
									part.PartitionName = decodeMBRType(entry2.type);
									part.PartitionSequence = counter;
									if(entry2.status==0x80)
										part.PartitionDescription = "Partition is bootable.";
									else
										part.PartitionDescription = "";
									
									counter++;
									
									partitions.Add(part);
								}
							}
							
							if(ext_extended)
							{
								br.BaseStream.Seek((long)entry2.lba_start*512, SeekOrigin.Begin);
								break;
							}
						}
						
						br.BaseStream.Seek(2, SeekOrigin.Current);
					}
					
					br.BaseStream.Seek(pre_ext_Pos, SeekOrigin.Begin);
				}
			}
			
			// An empty MBR may exist, NeXT creates one and then hardcodes its disklabel
			if(partitions.Count==0)
				return false;
			else	
				return true;
		}
		
		private string decodeBSDType(byte type)
		{
			switch(type)
			{
				case 1:
					return "Swap";
				case 2:
					return "UNIX Version 6";
				case 3:
					return "UNIX Version 7";
				case 4:
					return "System V";
				case 5:
					return "4.1BSD";
				case 6:
					return "UNIX Eigth Edition";
				case 7:
					return "4.2BSD";
				case 8:
					return "MS-DOS";
				case 9:
					return "4.4LFS";
				case 11:
					return "HPFS";
				case 12:
					return "ISO9660";
				case 13:
					return "Boot";
				case 14:
					return "Amiga FFS";
				case 15:
					return "Apple HFS";
				default:
					return "Unknown";
			}
		}
		
		private string decodeMBRType(byte type)
		{
			switch(type)
			{
				case 0x01:
					return "FAT12";
				case 0x02:
					return "XENIX root";
				case 0x03:
					return "XENIX /usr";
				case 0x04:
					return "FAT16 < 32 MiB";
				case 0x05:
					return "Extended";
				case 0x06:
					return "FAT16";
				case 0x07:
					return "IFS (HPFS/NTFS)";
				case 0x08:
					return "AIX boot, OS/2, Commodore DOS";
				case 0x09:
					return "AIX data, Coherent, QNX";
				case 0x0A:
					return "Coherent swap, OPUS, OS/2 Boot Manager";
				case 0x0B:
					return "FAT32";
				case 0x0C:
					return "FAT32 (LBA)";
				case 0x0E:
					return "FAT16 (LBA)";
				case 0x0F:
					return "Extended (LBA)";
				case 0x10:
					return "OPUS";
				case 0x11:
					return "Hidden FAT12";
				case 0x12:
					return "Compaq diagnostics, recovery partition";
				case 0x14:
					return "Hidden FAT16 < 32 MiB, AST-DOS";
				case 0x16:
					return "Hidden FAT16";
				case 0x17:
					return "Hidden IFS (HPFS/NTFS)";
				case 0x18:
					return "AST-Windows swap";
				case 0x19:
					return "Willowtech Photon coS";
				case 0x1B:
					return "Hidden FAT32";
				case 0x1C:
					return "Hidden FAT32 (LBA)";
				case 0x1E:
					return "Hidden FAT16 (LBA)";
				case 0x20:
					return "Willowsoft Overture File System";
				case 0x21:
					return "Oxygen FSo2";
				case 0x22:
					return "Oxygen Extended ";
				case 0x23:
					return "SpeedStor reserved";
				case 0x24:
					return "NEC-DOS";
				case 0x26:
					return "SpeedStor reserved";
				case 0x27:
					return "Hidden NTFS";
				case 0x31:
					return "SpeedStor reserved";
				case 0x33:
					return "SpeedStor reserved";
				case 0x34:
					return "SpeedStor reserved";
				case 0x36:
					return "SpeedStor reserved";
				case 0x38:
					return "Theos";
				case 0x39:
					return "Plan 9";
				case 0x3C:
					return "Partition Magic";
				case 0x3D:
					return "Hidden NetWare";
				case 0x40:
					return "VENIX 80286";
				case 0x41:
					return "PReP Boot";
				case 0x42:
					return "Secure File System";
				case 0x43:
					return "PTS-DOS";
				case 0x45:
					return "Priam, EUMEL/Elan";
				case 0x46:
					return "EUMEL/Elan";
				case 0x47:
					return "EUMEL/Elan";
				case 0x48:
					return "EUMEL/Elan";
				case 0x4A:
					return "ALFS/THIN lightweight filesystem for DOS";
				case 0x4D:
					return "QNX 4";
				case 0x4E:
					return "QNX 4";
				case 0x4F:
					return "QNX 4, Oberon";
				case 0x50:
					return "Ontrack DM, R/O, FAT";
				case 0x51:
					return "Ontrack DM, R/W, FAT";
				case 0x52:
					return "CP/M, Microport UNIX";
				case 0x53:
					return "Ontrack DM 6";
				case 0x54:
					return "Ontrack DM 6";
				case 0x55:
					return "EZ-Drive";
				case 0x56:
					return "Golden Bow VFeature";
				case 0x5C:
					return "Priam EDISK";
				case 0x61:
					return "SpeedStor";
				case 0x63:
					return "GNU Hurd, System V, 386/ix";
				case 0x64:
					return "NetWare 286";
				case 0x65:
					return "NetWare";
				case 0x66:
					return "NetWare 386";
				case 0x67:
					return "NetWare";
				case 0x68:
					return "NetWare";
				case 0x69:
					return "NetWare NSS";
				case 0x70:
					return "DiskSecure Multi-Boot";
				case 0x75:
					return "IBM PC/IX";
				case 0x80:
					return "Old MINIX";
				case 0x81:
					return "MINIX, Old Linux";
				case 0x82:
					return "Linux swap, Solaris";
				case 0x83:
					return "Linux";
				case 0x84:
					return "Hidden by OS/2, APM hibernation";
				case 0x85:
					return "Linux extended";
				case 0x86:
					return "NT Stripe Set";
				case 0x87:
					return "NT Stripe Set";
				case 0x88:
					return "Linux Plaintext";
				case 0x8E:
					return "Linux LVM";
				case 0x93:
					return "Amoeba, Hidden Linux";
				case 0x94:
					return "Amoeba bad blocks";
				case 0x99:
					return "Mylex EISA SCSI";
				case 0x9F:
					return "BSD/OS";
				case 0xA0:
					return "Hibernation";
				case 0xA1:
					return "HP Volume Expansion";
				case 0xA3:
					return "HP Volume Expansion";
				case 0xA4:
					return "HP Volume Expansion";
				case 0xA5:
					return "FreeBSD";
				case 0xA6:
					return "OpenBSD";
				case 0xA7:
					return "NeXTStep";
				case 0xA8:
					return "Apple UFS";
				case 0xA9:
					return "NetBSD";
				case 0xAA:
					return "Olivetti DOS FAT12";
				case 0xAB:
					return "Apple Boot";
				case 0xAF:
					return "Apple HFS";
				case 0xB0:
					return "BootStar";
				case 0xB1:
					return "HP Volume Expansion";
				case 0xB3:
					return "HP Volume Expansion";
				case 0xB4:
					return "HP Volume Expansion";
				case 0xB6:
					return "HP Volume Expansion";
				case 0xB7:
					return "BSDi";
				case 0xB8:
					return "BSDi swap";
				case 0xBB:
					return "PTS BootWizard";
				case 0xBE:
					return "Solaris boot";
				case 0xBF:
					return "Solaris";
				case 0xC0:
					return "Novell DOS, DR-DOS secured";
				case 0xC1:
					return "DR-DOS secured FAT12";
				case 0xC2:
					return "DR-DOS reserved";
				case 0xC3:
					return "DR-DOS reserved";
				case 0xC4:
					return "DR-DOS secured FAT16 < 32 MiB";
				case 0xC6:
					return "DR-DOS secured FAT16";
				case 0xC7:
					return "Syrinx";
				case 0xC8:
					return "DR-DOS reserved";
				case 0xC9:
					return "DR-DOS reserved";
				case 0xCA:
					return "DR-DOS reserved";
				case 0xCB:
					return "DR-DOS secured FAT32";
				case 0xCC:
					return "DR-DOS secured FAT32 (LBA)";
				case 0xCD:
					return "DR-DOS reserved";
				case 0xCE:
					return "DR-DOS secured FAT16 (LBA)";
				case 0xCF:
					return "DR-DOS secured extended (LBA)";
				case 0xD0:
					return "Multiuser DOS secured FAT12";
				case 0xD1:
					return "Multiuser DOS secured FAT12";
				case 0xD4:
					return "Multiuser DOS secured FAT16 < 32 MiB";
				case 0xD5:
					return "Multiuser DOS secured extended";
				case 0xD6:
					return "Multiuser DOS secured FAT16";
				case 0xD8:
					return "CP/M";
				case 0xDA:
					return "Filesystem-less data";
				case 0xDB:
					return "CP/M, CCP/M, CTOS";
				case 0xDE:
					return "Dell partition";
				case 0xDF:
					return "BootIt EMBRM";
				case 0xE1:
					return "SpeedStor";
				case 0xE2:
					return "DOS read/only";
				case 0xE3:
					return "SpeedStor";
				case 0xE4:
					return "SpeedStor";
				case 0xE5:
					return "Tandy DOS";
				case 0xE6:
					return "SpeedStor";
				case 0xEB:
					return "BeOS";
				case 0xED:
					return "Spryt*x";
				case 0xEE:
					return "Guid Partition Table";
				case 0xEF:
					return "EFI system partition";
				case 0xF0:
					return "Linux boot";
				case 0xF1:
					return "SpeedStor";
				case 0xF2:
					return "DOS 3.3 secondary, Unisys DOS";
				case 0xF3:
					return "SpeedStor";
				case 0xF4:
					return "SpeedStor";
				case 0xF5:
					return "Prologue";
				case 0xF6:
					return "SpeedStor";
				case 0xFB:
					return "VMWare VMFS";
				case 0xFC:
					return "VMWare VMKCORE";
				case 0xFD:
					return "Linux RAID, FreeDOS";
				case 0xFE:
					return "SpeedStor, LANStep, PS/2 IML";
				case 0xFF:
					return "Xenix bad block";
				default:
					return "Unknown";
			}
		}                        
		
		public struct MBRPartitionEntry
		{
			public byte   status;         // Partition status, 0x80 or 0x00, else invalid
			public byte   start_head;     // Starting head [0,254]
			public byte   start_sector;   // Starting sector [1,63]
			public UInt16 start_cylinder; // Starting cylinder [0,1023]
			public byte   type;           // Partition type
			public byte   end_head;       // Ending head [0,254]
			public byte   end_sector;     // Ending sector [1,63]
			public UInt16 end_cylinder;   // Ending cylinder [0,1023]
			public UInt32 lba_start;      // Starting absolute sector
			public UInt32 lba_sectors;    // Total sectors	
		}

		private const UInt32 UNIXDiskLabel_MAGIC = 0xCA5E600D;
		private const UInt32 UNIXVTOC_MAGIC      = 0x600DDEEE; // Same as Solaris VTOC

		private struct UNIXDiskLabel
		{
			public UInt32 type;       // Drive type, seems always 0
			public UInt32 magic;      // UNIXDiskLabel_MAGIC
			public UInt32 version;    // Only seen 1
			public string serial;     // 12 bytes, serial number of the device
			public UInt32 cyls;       // data cylinders per device
			public UInt32 trks;       // data tracks per cylinder
			public UInt32 secs;       // data sectors per track
			public UInt32 bps;        // data bytes per sector
			public UInt32 start;      // first sector of this partition
			public byte[] unknown1;   // 48 bytes
			public UInt32 alt_tbl;    // byte offset of alternate table
			public UInt32 alt_len;    // byte length of alternate table
			// From onward here, is not on old version
			public UInt32 phys_cyl;   // physical cylinders per device
			public UInt32 phys_trk;   // physical tracks per cylinder
			public UInt32 phys_sec;   // physical sectors per track
			public UInt32 phys_bytes; // physical bytes per sector
			public UInt32 unknown2;   //
			public UInt32 unknown3;   //
			public byte[] pad;        // 32bytes
		}

		private struct UNIXVTOC
		{
			public UInt32 magic;     // UNIXVTOC_MAGIC
			public UInt32 version;   // 1
			public string name;      // 8 bytes
			public UInt32 slices;    // # of slices
			public UInt32 unknown;   //
			public byte[] reserved;  // 40 bytes
		}

		private struct UNIXVTOCEntry
		{
			public UInt16 tag;    // TAG
			public UInt16 flags;  // Flags (see below)
			public UInt32 start;  // Start sector
			public UInt32 length; // Length of slice in sectors
		}

		private const UInt16 UNIX_TAG_EMPTY      = 0x0000; // empty
		private const UInt16 UNIX_TAG_BOOT       = 0x0001; // boot
		private const UInt16 UNIX_TAG_ROOT       = 0x0002; // root
		private const UInt16 UNIX_TAG_SWAP       = 0x0003; // swap
		private const UInt16 UNIX_TAG_USER       = 0x0004; // /usr
		private const UInt16 UNIX_TAG_WHOLE      = 0x0005; // whole disk
		private const UInt16 UNIX_TAG_STAND      = 0x0006; // stand partition ??
		private const UInt16 UNIX_TAG_ALT_S      = 0x0006; // alternate sector space
		private const UInt16 UNIX_TAG_VAR        = 0x0007; // /var
		private const UInt16 UNIX_TAG_OTHER      = 0x0007; // non UNIX
		private const UInt16 UNIX_TAG_HOME       = 0x0008; // /home
		private const UInt16 UNIX_TAG_ALT_T      = 0x0008; // alternate track space
		private const UInt16 UNIX_TAG_ALT_ST     = 0x0009; // alternate sector track
		private const UInt16 UNIX_TAG_NEW_STAND  = 0x0009; // stand partition ??
		private const UInt16 UNIX_TAG_CACHE      = 0x000A; // cache
		private const UInt16 UNIX_TAG_NEW_VAR    = 0x000A; // /var
		private const UInt16 UNIX_TAG_RESERVED   = 0x000B; // reserved
		private const UInt16 UNIX_TAG_NEW_HOME   = 0x000B; // /home
		private const UInt16 UNIX_TAG_DUMP       = 0x000C; // dump partition
		private const UInt16 UNIX_TAG_NEW_ALT_ST = 0x000D; // alternate sector track
		private const UInt16 UNIX_TAG_VM_PUBLIC  = 0x000E; // volume mgt public partition
		private const UInt16 UNIX_TAG_VM_PRIVATE = 0x000F; // volume mgt private partition

		private string decodeUNIXTAG(UInt16 type, bool isNew)
		{
			switch(type)
			{
			case UNIX_TAG_EMPTY:
				return "Unused";
			case UNIX_TAG_BOOT:
				return "Boot";
			case UNIX_TAG_ROOT:
				return "/";
			case UNIX_TAG_SWAP:
				return "Swap";
			case UNIX_TAG_USER:
				return "/usr";
			case UNIX_TAG_WHOLE:
				return "Whole disk";
			case UNIX_TAG_STAND:
				if(isNew)
					return "Stand";
				else
					return "Alternate sector space";
			case UNIX_TAG_VAR:
				if(isNew)
					return "/var";
				else
					return "non UNIX";
			case UNIX_TAG_HOME:
				if(isNew)
					return "/home";
				else
					return "Alternate track space";
			case UNIX_TAG_ALT_ST:
				if(isNew)
					return "Alternate sector track";
				else
					return "Stand";
			case UNIX_TAG_CACHE:
				if(isNew)
					return "Cache";
				else
					return "/var";
			case UNIX_TAG_RESERVED:
				if(isNew)
					return "Reserved";
				else
					return "/home";
			case UNIX_TAG_DUMP:
				return "dump";
			case UNIX_TAG_NEW_ALT_ST:
				return "Alternate sector track";
			case UNIX_TAG_VM_PUBLIC:
				return "volume mgt public partition";
			case UNIX_TAG_VM_PRIVATE:
				return "volume mgt private partition";
			default:
				return String.Format ("Unknown TAG: 0x{0:X4}", type);
			}
		}
	}
}