// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : D88.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Quasi88 disk images.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/
using System;
using System.IO;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.CommonTypes;
using System.Linq;
using System.Text;
using DiscImageChef.Filters;
using System.Runtime.InteropServices;
using DiscImageChef.Decoders.Floppy;

namespace DiscImageChef.ImagePlugins
{
	// Information from Quasi88's FORMAT.TXT file
	// Japanese comments copied from there
	public class D88 : ImagePlugin
	{
		#region Internal enumerations
		enum DiskType : byte
		{
			D2 = 0x00,
			DD2 = 0x10,
			HD2 = 0x20,
		}

		enum DensityType : byte
		{
			MFM = 0x00,
			FM = 0x40,
		}

		/// <summary>
		/// Status as returned by PC-98 BIOS
		/// ステータスは、PC-98x1 のBIOS が返してくるステータスで、
		/// </summary>
		enum StatusType : byte
		{
			/// <summary>
			/// Normal
			/// 正常
			/// </summary>
			Normal = 0x00,
			/// <summary>
			/// Deleted
			/// 正常(DELETED DATA)
			/// </summary>
			Deleted = 0x10,
			/// <summary>
			/// CRC error in address fields
			/// ID CRC エラー
			/// </summary>
			IDError = 0xA0,
			/// <summary>
			/// CRC error in data block
			/// データ CRC エラー
			/// </summary>
			DataError = 0xB0,
			/// <summary>
			/// Address mark not found
			/// アドレスマークなし
			/// </summary>
			AddressMarkNotFound = 0xE0,
			/// <summary>
			/// Data mark not found
			/// データマークなし
			/// </summary>
			DataMarkNotFound = 0xF0,
		}
		#endregion

		#region Internal constants
		readonly byte[] ReservedEmpty = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
		const byte ReadOnly = 0x10;
		#endregion

		#region Internal structures
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct D88Header
		{
			/// <summary>
			/// Disk name, nul-terminated ASCII
			/// ディスクの名前(ASCII + '\0')
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
			public byte[] name;
			/// <summary>
			/// Reserved
			/// ディスクの名前(ASCII + '\0')
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
			public byte[] reserved;
			/// <summary>
			/// Write protect status
			/// ライトプロテクト： 0x00 なし、0x10 あり
			/// </summary>
			public byte write_protect;
			/// <summary>
			/// Disk type
			/// ディスクの種類： 0x00 2D、 0x10 2DD、 0x20 2HD
			/// </summary>
			public DiskType disk_type;
			/// <summary>
			/// Disk image size
			/// ディスクのサイズ
			/// </summary>
			public int disk_size;
			/// <summary>
			/// Track pointers
			/// トラック部のオフセットテーブル 0 Track ～ 163 Track
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 164)]
			public int[] track_table;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct SectorHeader
		{
			/// <summary>
			/// Cylinder
			/// ID の C
			/// </summary>
			public byte c;
			/// <summary>
			/// Head
			/// ID の H
			/// </summary>
			public byte h;
			/// <summary>
			/// Sector number
			/// ID の R
			/// </summary>
			public byte r;
			/// <summary>
			/// Sector size
			/// ID の N
			/// </summary>
			public IBMSectorSizeCode n;
			/// <summary>
			/// Number of sectors in this track
			/// このトラック内に存在するセクタの数
			/// </summary>
			public short spt;
			/// <summary>
			/// Density: 0x00 MFM, 0x40 FM
			/// 記録密度： 0x00 倍密度、0x40 単密度
			/// </summary>
			public DensityType density;
			/// <summary>
			/// Deleted sector, 0x00 not deleted, 0x10 deleted
			/// DELETED MARK： 0x00 ノーマル、 0x10 DELETED
			/// </summary>
			public byte deleted_mark;
			/// <summary>
			/// Sector status
			/// ステータス
			/// </summary>
			public byte status;
			/// <summary>
			/// Reserved
			/// リザーブ
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
			public byte[] reserved;
			/// <summary>
			/// Size of data following this field
			/// このセクタ部のデータサイズ
			/// </summary>
			public short size_of_data;
		}
		#endregion

		List<byte[]> sectorsData;

		public D88()
		{
			Name = "D88 Disk Image";
			PluginUUID = new Guid("669EDC77-EC41-4720-A88C-49C38CFFBAA0");
			ImageInfo = new ImageInfo()
			{
				readableSectorTags = new List<SectorTagType>(),
				readableMediaTags = new List<MediaTagType>(),
				imageHasPartitions = false,
				imageHasSessions = false,
				imageVersion = null,
				imageApplication = null,
				imageApplicationVersion = null,
				imageCreator = null,
				imageComments = null,
				mediaManufacturer = null,
				mediaModel = null,
				mediaSerialNumber = null,
				mediaBarcode = null,
				mediaPartNumber = null,
				mediaSequence = 0,
				lastMediaSequence = 0,
				driveManufacturer = null,
				driveModel = null,
				driveSerialNumber = null,
				driveFirmwareRevision = null
			};
		}

		public override bool IdentifyImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);
			// Even if disk name is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
			Encoding shiftjis = Encoding.GetEncoding("shift_jis");

			D88Header d88hdr = new D88Header();

			if(stream.Length < Marshal.SizeOf(d88hdr))
				return false;

			byte[] hdr_b = new byte[Marshal.SizeOf(d88hdr)];
			stream.Read(hdr_b, 0, hdr_b.Length);

			GCHandle handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
			d88hdr = (D88Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(D88Header));
			handle.Free();

			DicConsole.DebugWriteLine("D88 plugin", "d88hdr.name = \"{0}\"", StringHandlers.CToString(d88hdr.name, shiftjis));
			DicConsole.DebugWriteLine("D88 plugin", "d88hdr.reserved is empty? = {0}", d88hdr.reserved.SequenceEqual(ReservedEmpty));
			DicConsole.DebugWriteLine("D88 plugin", "d88hdr.write_protect = 0x{0:X2}", d88hdr.write_protect);
			DicConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_type = {0} ({1})", d88hdr.disk_type, (byte)d88hdr.disk_type);
			DicConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_size = {0}", d88hdr.disk_size);

			if(d88hdr.disk_size != stream.Length)
				return false;

			if(d88hdr.disk_type != DiskType.D2 && d88hdr.disk_type != DiskType.DD2 && d88hdr.disk_type != DiskType.HD2)
				return false;

			if(!d88hdr.reserved.SequenceEqual(ReservedEmpty))
				return false;

			int counter = 0;
			for(int i = 0; i < d88hdr.track_table.Length; i++)
			{
				if(d88hdr.track_table[i] > 0)
					counter++;

				if(d88hdr.track_table[i] < 0 || d88hdr.track_table[i] > stream.Length)
					return false;
			}

			DicConsole.DebugWriteLine("D88 plugin", "{0} tracks", counter);

			return counter > 0;
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);
			// Even if disk name is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
			Encoding shiftjis = Encoding.GetEncoding("shift_jis");

			D88Header d88hdr = new D88Header();

			if(stream.Length < Marshal.SizeOf(d88hdr))
				return false;

			byte[] hdr_b = new byte[Marshal.SizeOf(d88hdr)];
			stream.Read(hdr_b, 0, hdr_b.Length);

			GCHandle handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
			d88hdr = (D88Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(D88Header));
			handle.Free();

			DicConsole.DebugWriteLine("D88 plugin", "d88hdr.name = \"{0}\"", StringHandlers.CToString(d88hdr.name, shiftjis));
			DicConsole.DebugWriteLine("D88 plugin", "d88hdr.reserved is empty? = {0}", d88hdr.reserved.SequenceEqual(ReservedEmpty));
			DicConsole.DebugWriteLine("D88 plugin", "d88hdr.write_protect = 0x{0:X2}", d88hdr.write_protect);
			DicConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_type = {0} ({1})", d88hdr.disk_type, (byte)d88hdr.disk_type);
			DicConsole.DebugWriteLine("D88 plugin", "d88hdr.disk_size = {0}", d88hdr.disk_size);

			if(d88hdr.disk_size != stream.Length)
				return false;

			if(d88hdr.disk_type != DiskType.D2 && d88hdr.disk_type != DiskType.DD2 && d88hdr.disk_type != DiskType.HD2)
				return false;

			if(!d88hdr.reserved.SequenceEqual(ReservedEmpty))
				return false;

			int trkCounter = 0;
			for(int i = 0; i < d88hdr.track_table.Length; i++)
			{
				if(d88hdr.track_table[i] > 0)
					trkCounter++;

				if(d88hdr.track_table[i] < 0 || d88hdr.track_table[i] > stream.Length)
					return false;
			}

			DicConsole.DebugWriteLine("D88 plugin", "{0} tracks", trkCounter);

			if(trkCounter == 0)
				return false;

			SectorHeader sechdr = new SectorHeader();
			hdr_b = new byte[Marshal.SizeOf(sechdr)];
			stream.Seek(d88hdr.track_table[0], SeekOrigin.Begin);
			stream.Read(hdr_b, 0, hdr_b.Length);

			handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
			sechdr = (SectorHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SectorHeader));
			handle.Free();

			DicConsole.DebugWriteLine("D88 plugin", "sechdr.c = {0}", sechdr.c);
			DicConsole.DebugWriteLine("D88 plugin", "sechdr.h = {0}", sechdr.h);
			DicConsole.DebugWriteLine("D88 plugin", "sechdr.r = {0}", sechdr.r);
			DicConsole.DebugWriteLine("D88 plugin", "sechdr.n = {0}", sechdr.n);
			DicConsole.DebugWriteLine("D88 plugin", "sechdr.spt = {0}", sechdr.spt);
			DicConsole.DebugWriteLine("D88 plugin", "sechdr.density = {0}", sechdr.density);
			DicConsole.DebugWriteLine("D88 plugin", "sechdr.deleted_mark = {0}", sechdr.deleted_mark);
			DicConsole.DebugWriteLine("D88 plugin", "sechdr.status = {0}", sechdr.status);
			DicConsole.DebugWriteLine("D88 plugin", "sechdr.size_of_data = {0}", sechdr.size_of_data);

			short spt = sechdr.spt;
			IBMSectorSizeCode bps = sechdr.n;
			bool allEqual = true;
			sectorsData = new List<byte[]>();

			for(int i = 0; i < trkCounter; i++)
			{
				stream.Seek(d88hdr.track_table[i], SeekOrigin.Begin);
				stream.Read(hdr_b, 0, hdr_b.Length);
				SortedDictionary<byte, byte[]> sectors = new SortedDictionary<byte, byte[]>();

				handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
				sechdr = (SectorHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SectorHeader));
				handle.Free();

				if(sechdr.spt != spt || sechdr.n != bps)
				{
					DicConsole.DebugWriteLine("D88 plugin", "Disk tracks are not same size. spt = {0} (expected {1}), bps = {2} (expected {3}) at track {4} sector {5}", sechdr.spt, spt, sechdr.n, bps, i, 0);
					allEqual = false;
				}

				short maxJ = sechdr.spt;
				byte[] sec_b;
				for(short j = 1; j < maxJ; j++)
				{
					sec_b = new byte[sechdr.size_of_data];
					stream.Read(sec_b, 0, sec_b.Length);
					sectors.Add(sechdr.r, sec_b);
					stream.Read(hdr_b, 0, hdr_b.Length);

					handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
					sechdr = (SectorHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SectorHeader));
					handle.Free();

					if(sechdr.spt != spt || sechdr.n != bps)
					{
						DicConsole.DebugWriteLine("D88 plugin", "Disk tracks are not same size. spt = {0} (expected {1}), bps = {2} (expected {3}) at track {4} sector {5}", sechdr.spt, spt, sechdr.n, bps, i, j, sechdr.deleted_mark);
						allEqual = false;
					}
				}

				sec_b = new byte[sechdr.size_of_data];
				stream.Read(sec_b, 0, sec_b.Length);
				sectors.Add(sechdr.r, sec_b);

				foreach(KeyValuePair<byte, byte[]> kvp in sectors)
					sectorsData.Add(kvp.Value);
			}

			DicConsole.DebugWriteLine("D88 plugin", "{0} sectors", sectorsData.Count());

			/*
			FileStream debugStream = new FileStream("debug.img", FileMode.CreateNew, FileAccess.ReadWrite);
			for(int i = 0; i < sectorsData.Count; i++)
				debugStream.Write(sectorsData[i], 0, sectorsData[i].Length);
			debugStream.Close();
			*/

			ImageInfo.mediaType = MediaType.Unknown;
			if(allEqual)
			{
				if(trkCounter == 154 && spt == 26 && bps == IBMSectorSizeCode.EighthKilo)
					ImageInfo.mediaType = MediaType.NEC_8_SD;
				else if(bps == IBMSectorSizeCode.QuarterKilo)
				{
					if(trkCounter == 80 && spt == 16)
						ImageInfo.mediaType = MediaType.NEC_525_SS;
					else if(trkCounter == 154 && spt == 26)
						ImageInfo.mediaType = MediaType.NEC_8_DD;
					else if(trkCounter == 160 && spt == 16)
						ImageInfo.mediaType = MediaType.NEC_525_DS;
				}
				else if(trkCounter == 154 && spt == 8 && bps == IBMSectorSizeCode.Kilo)
					ImageInfo.mediaType = MediaType.NEC_525_HD;
				else if(bps == IBMSectorSizeCode.HalfKilo)
				{
					switch(d88hdr.track_table.Length)
					{
						case 40:
							{
								switch(spt)
								{
									case 8:
										ImageInfo.mediaType = MediaType.DOS_525_SS_DD_8;
										break;
									case 9:
										ImageInfo.mediaType = MediaType.DOS_525_SS_DD_9;
										break;
								}
							}
							break;
						case 80:
							{
								switch(spt)
								{
									case 8:
										ImageInfo.mediaType = MediaType.DOS_525_DS_DD_8;
										break;
									case 9:
										ImageInfo.mediaType = MediaType.DOS_525_DS_DD_9;
										break;
								}
							}
							break;
						case 160:
							{
								switch(spt)
								{
									case 15:
										ImageInfo.mediaType = MediaType.NEC_35_HD_15;
										break;
									case 9:
										ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;
										break;
									case 18:
										ImageInfo.mediaType = MediaType.DOS_35_HD;
										break;
									case 36:
										ImageInfo.mediaType = MediaType.DOS_35_ED;
										break;
								}
							}
							break;
						case 480:
							if(spt == 38)
								ImageInfo.mediaType = MediaType.NEC_35_TD;
							break;
					}
				}
			}

			DicConsole.DebugWriteLine("D88 plugin", "MediaType: {0}", ImageInfo.mediaType);

			ImageInfo.imageSize = (ulong)d88hdr.disk_size;
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
			ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
			ImageInfo.sectors = (ulong)sectorsData.Count;
			ImageInfo.imageComments = StringHandlers.CToString(d88hdr.name, shiftjis);
			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
			ImageInfo.sectorSize = (uint)(128 << (int)bps);

			switch(ImageInfo.mediaType)
			{
				case MediaType.NEC_525_SS:
					ImageInfo.cylinders = 80;
					ImageInfo.heads = 1;
					ImageInfo.sectorsPerTrack = 16;
					break;
				case MediaType.NEC_8_SD:
				case MediaType.NEC_8_DD:
					ImageInfo.cylinders = 77;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 26;
					break;
				case MediaType.NEC_525_DS:
					ImageInfo.cylinders = 80;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 16;
					break;
				case MediaType.NEC_525_HD:
					ImageInfo.cylinders = 77;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 8;
					break;
				case MediaType.DOS_525_SS_DD_8:
					ImageInfo.cylinders = 40;
					ImageInfo.heads = 1;
					ImageInfo.sectorsPerTrack = 8;
					break;
				case MediaType.DOS_525_SS_DD_9:
					ImageInfo.cylinders = 40;
					ImageInfo.heads = 1;
					ImageInfo.sectorsPerTrack = 9;
					break;
				case MediaType.DOS_525_DS_DD_8:
					ImageInfo.cylinders = 40;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 8;
					break;
				case MediaType.DOS_525_DS_DD_9:
					ImageInfo.cylinders = 40;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 9;
					break;
				case MediaType.NEC_35_HD_15:
					ImageInfo.cylinders = 80;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 15;
					break;
				case MediaType.DOS_35_DS_DD_9:
					ImageInfo.cylinders = 80;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 9;
					break;
				case MediaType.DOS_35_HD:
					ImageInfo.cylinders = 80;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 18;
					break;
				case MediaType.DOS_35_ED:
					ImageInfo.cylinders = 80;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 36;
					break;
				case MediaType.NEC_35_TD:
					ImageInfo.cylinders = 240;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 38;
					break;
			}

			return true;
		}

		public override bool ImageHasPartitions()
		{
			return false;
		}

		public override ulong GetImageSize()
		{
			return ImageInfo.imageSize;
		}

		public override ulong GetSectors()
		{
			return ImageInfo.sectors;
		}

		public override uint GetSectorSize()
		{
			return ImageInfo.sectorSize;
		}

		public override string GetImageFormat()
		{
			return "D88 disk image";
		}

		public override string GetImageVersion()
		{
			return ImageInfo.imageVersion;
		}

		public override string GetImageApplication()
		{
			return ImageInfo.imageApplication;
		}

		public override string GetImageApplicationVersion()
		{
			return ImageInfo.imageApplicationVersion;
		}

		public override string GetImageCreator()
		{
			return ImageInfo.imageCreator;
		}

		public override DateTime GetImageCreationTime()
		{
			return ImageInfo.imageCreationTime;
		}

		public override DateTime GetImageLastModificationTime()
		{
			return ImageInfo.imageLastModificationTime;
		}

		public override string GetImageName()
		{
			return ImageInfo.imageName;
		}

		public override string GetImageComments()
		{
			return ImageInfo.imageComments;
		}

		public override MediaType GetMediaType()
		{
			return ImageInfo.mediaType;
		}

		public override byte[] ReadSector(ulong sectorAddress)
		{
			return ReadSectors(sectorAddress, 1);
		}

		public override byte[] ReadSectors(ulong sectorAddress, uint length)
		{
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

			if(sectorAddress + length > ImageInfo.sectors)
				throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

			MemoryStream buffer = new MemoryStream();
			for(int i = 0; i < length; i++)
				buffer.Write(sectorsData[(int)sectorAddress + i], 0, sectorsData[(int)sectorAddress + i].Length);

			return buffer.ToArray();
		}

		#region Unsupported features

		public override byte[] ReadDiskTag(MediaTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSector(ulong sectorAddress, uint track)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorLong(ulong sectorAddress)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override string GetMediaManufacturer()
		{
			return null;
		}

		public override string GetMediaModel()
		{
			return null;
		}

		public override string GetMediaSerialNumber()
		{
			return null;
		}

		public override string GetMediaBarcode()
		{
			return null;
		}

		public override string GetMediaPartNumber()
		{
			return null;
		}

		public override int GetMediaSequence()
		{
			return 0;
		}

		public override int GetLastDiskSequence()
		{
			return 0;
		}

		public override string GetDriveManufacturer()
		{
			return null;
		}

		public override string GetDriveModel()
		{
			return null;
		}

		public override string GetDriveSerialNumber()
		{
			return null;
		}

		public override List<Partition> GetPartitions()
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override List<Track> GetTracks()
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override List<Track> GetSessionTracks(Session session)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override List<Track> GetSessionTracks(ushort session)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override List<Session> GetSessions()
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override bool? VerifySector(ulong sectorAddress)
		{
			return null;
		}

		public override bool? VerifySector(ulong sectorAddress, uint track)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
		{
			FailingLBAs = new List<ulong>();
			UnknownLBAs = new List<ulong>();
			for(ulong i = 0; i < ImageInfo.sectors; i++)
				UnknownLBAs.Add(i);
			return null;
		}

		public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override bool? VerifyMediaImage()
		{
			return null;
		}

		#endregion
	}
}