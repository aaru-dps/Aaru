// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Apridisk.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages ACT Apricot disk images.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
	public class Apridisk : ImagePlugin
	{
		#region Internal enumerations
		enum RecordType : uint
		{
			Deleted = 0xE31D0000,
			Sector = 0xE31D0001,
			Comment = 0xE31D0002,
			Creator = 0xE31D0003,
		}

		enum CompressType : ushort
		{
			Uncompresed = 0x9E90,
			Compressed = 0x3E5A,
		}
		#endregion

		#region Internal constants
		readonly byte[] signature = {
			0x41, 0x43, 0x54, 0x20, 0x41, 0x70, 0x72, 0x69, 0x63, 0x6F, 0x74, 0x20, 0x64, 0x69, 0x73, 0x6B,
			0x20, 0x69, 0x6D, 0x61, 0x67, 0x65, 0x1A, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
		};

		#endregion

		#region Internal structures
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct ApridiskRecord
		{
			public RecordType type;
			public CompressType compression;
			public ushort headerSize;
			public uint dataSize;
			public byte head;
			public byte sector;
			public ushort cylinder;
		}
		#endregion

		// Cylinder by head, sector data matrix
		byte[][][][] sectorsData;

		public Apridisk()
		{
			Name = "ACT Apricot Disk Image";
			PluginUUID = new Guid("43408CF3-6DB3-449F-A779-2B0E497C5B14");
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

			if(stream.Length < signature.Length)
				return false;

			byte[] sig_b = new byte[signature.Length];
			stream.Read(sig_b, 0, signature.Length);

			return sig_b.SequenceEqual(signature);
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);

			// Skip signature
			stream.Seek(signature.Length, SeekOrigin.Begin);

			int totalCylinders = -1;
			int totalHeads = -1;
			int maxSector = - 1;
			int recordSize = Marshal.SizeOf(typeof(ApridiskRecord));

			// Count cylinders
			while(stream.Position < stream.Length)
			{
				ApridiskRecord record = new ApridiskRecord();
				byte[] rec_b = new byte[recordSize];
				stream.Read(rec_b, 0, recordSize);

				GCHandle handle = GCHandle.Alloc(rec_b, GCHandleType.Pinned);
				record = (ApridiskRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ApridiskRecord));
				handle.Free();

				switch(record.type)
				{
					// Deleted record, just skip it
					case RecordType.Deleted:
						DicConsole.DebugWriteLine("Apridisk plugin", "Found deleted record at {0}", stream.Position);
						stream.Seek((record.headerSize - recordSize) + record.dataSize, SeekOrigin.Current);
						break;
					case RecordType.Comment:
						DicConsole.DebugWriteLine("Apridisk plugin", "Found comment record at {0}", stream.Position);
						stream.Seek(record.headerSize - recordSize, SeekOrigin.Current);
						byte[] comment_b = new byte[record.dataSize];
						stream.Read(comment_b, 0, comment_b.Length);
						ImageInfo.imageComments = StringHandlers.CToString(comment_b);
						DicConsole.DebugWriteLine("Apridisk plugin", "Comment: \"{0}\"", ImageInfo.imageComments);
						break;
					case RecordType.Creator:
						DicConsole.DebugWriteLine("Apridisk plugin", "Found creator record at {0}", stream.Position);
						stream.Seek(record.headerSize - recordSize, SeekOrigin.Current);
						byte[] creator_b = new byte[record.dataSize];
						stream.Read(creator_b, 0, creator_b.Length);
						ImageInfo.imageCreator = StringHandlers.CToString(creator_b);
						DicConsole.DebugWriteLine("Apridisk plugin", "Creator: \"{0}\"", ImageInfo.imageCreator);
						break;
					case RecordType.Sector:
						if(record.compression != CompressType.Compressed && record.compression != CompressType.Uncompresed)
							throw new ImageNotSupportedException(string.Format("Found record with unknown compression type 0x{0:X4} at {1}", (ushort)record.compression, stream.Position));
						
						DicConsole.DebugWriteLine("Apridisk plugin", "Found {4} sector record at {0} for cylinder {1} head {2} sector {3}", stream.Position, record.cylinder, record.head,
						                          record.sector, record.compression == CompressType.Compressed ? "compressed" : "uncompressed");

						if(record.cylinder > totalCylinders)
							totalCylinders = record.cylinder;
						if(record.head > totalHeads)
							totalHeads = record.head;
						if(record.sector > maxSector)
							maxSector = record.sector;

						stream.Seek((record.headerSize - recordSize) + record.dataSize, SeekOrigin.Current);
						break;
					default:
						throw new ImageNotSupportedException(string.Format("Found record with unknown type 0x{0:X8} at {1}", (uint)record.type, stream.Position));
				}
			}

			totalCylinders++;
			totalHeads++;

			if(totalCylinders <= 0 || totalHeads <= 0)
				throw new ImageNotSupportedException("No cylinders or heads found");

			sectorsData = new byte[totalCylinders][][][];
			// Total sectors per track
			uint[][] spts = new uint[totalCylinders][];

			ImageInfo.cylinders = (ushort)totalCylinders;
			ImageInfo.heads = (byte)totalHeads;

			DicConsole.DebugWriteLine("Apridisk plugin", "Found {0} cylinders and {1} heads with a maximum sector number of {2}", totalCylinders, totalHeads, maxSector);

			// Create heads
			for(int i = 0; i < totalCylinders; i++)
			{
				sectorsData[i] = new byte[totalHeads][][];
				spts[i] = new uint[totalHeads];

				for(int j = 0; j < totalHeads; j++)
					sectorsData[i][j] = new byte[maxSector + 1][];
			}

			ImageInfo.sectorSize = uint.MaxValue;

			ulong headersizes = 0;

			// Read sectors
			stream.Seek(signature.Length, SeekOrigin.Begin);
			while(stream.Position < stream.Length)
			{
				ApridiskRecord record = new ApridiskRecord();
				byte[] rec_b = new byte[recordSize];
				stream.Read(rec_b, 0, recordSize);

				GCHandle handle = GCHandle.Alloc(rec_b, GCHandleType.Pinned);
				record = (ApridiskRecord)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ApridiskRecord));
				handle.Free();

				switch(record.type)
				{
					// Not sector record, just skip it
					case RecordType.Deleted:
					case RecordType.Comment:
					case RecordType.Creator:
						stream.Seek((record.headerSize - recordSize) + record.dataSize, SeekOrigin.Current);
						headersizes += record.headerSize + record.dataSize;
						break;
					case RecordType.Sector:
						stream.Seek(record.headerSize - recordSize, SeekOrigin.Current);

						byte[] data = new byte[record.dataSize];
						stream.Read(data, 0, data.Length);

						spts[record.cylinder][record.head]++;
						uint realLength = record.dataSize;

						if(record.compression == CompressType.Compressed)
							realLength = Decompress(data, out sectorsData[record.cylinder][record.head][record.sector]);
						else
							sectorsData[record.cylinder][record.head][record.sector] = data;

						if(realLength < ImageInfo.sectorSize)
							ImageInfo.sectorSize = realLength;

						headersizes += record.headerSize + record.dataSize;

						break;
				}
			}

			DicConsole.DebugWriteLine("Apridisk plugin", "Found a minimum of {0} bytes per sector", ImageInfo.sectorSize);

			// Count sectors per track
			uint spt = uint.MaxValue;
			for(ushort cyl = 0; cyl < ImageInfo.cylinders; cyl++)
			{
				for(ushort head = 0; head < ImageInfo.heads; head++)
				{
					if(spts[cyl][head] < spt)
						spt = spts[cyl][head];
				}
			}
			ImageInfo.sectorsPerTrack = spt;

			DicConsole.DebugWriteLine("Apridisk plugin", "Found a minimum of {0} sectors per track", ImageInfo.sectorsPerTrack);

			if(ImageInfo.cylinders == 70 && ImageInfo.heads == 1 && ImageInfo.sectorsPerTrack == 9)
				ImageInfo.mediaType = MediaType.Apricot_35;
			else if(ImageInfo.cylinders == 80 && ImageInfo.heads == 1 && ImageInfo.sectorsPerTrack == 9)
				ImageInfo.mediaType = MediaType.DOS_35_SS_DD_9;
			else if(ImageInfo.cylinders == 80 && ImageInfo.heads == 2 && ImageInfo.sectorsPerTrack == 9)
				ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;

			ImageInfo.imageSize = (ulong)stream.Length - headersizes;
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
			ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
			ImageInfo.sectors = ImageInfo.cylinders * ImageInfo.heads * ImageInfo.sectorsPerTrack;
			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;

			/*
			FileStream debugFs = new FileStream("debug.img", FileMode.CreateNew, FileAccess.Write);
			for(ulong i = 0; i < ImageInfo.sectors; i++)
				debugFs.Write(ReadSector(i), 0, (int)ImageInfo.sectorSize);
			debugFs.Dispose();
			*/

			return true;
		}

		static uint Decompress(byte[] compressed, out byte[] decompressed)
		{
			int readp = 0;
			ushort blklen;
			uint u_len = 0;
			int c_len = compressed.Length;
			MemoryStream buffer = new MemoryStream();

			u_len = 0;

			while(c_len >= 3)
			{
				blklen = BitConverter.ToUInt16(compressed, readp);
				readp += 2;

				for(int i = 0; i < blklen; i++)
					buffer.WriteByte(compressed[readp]);
				
				u_len += blklen;
				readp++;
				c_len -= 3;
			}

			decompressed = buffer.ToArray();
			return u_len;
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
			return "ACT Apricot disk image";
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
			(ushort cylinder, byte head, byte sector) = LbaToChs(sectorAddress);

			if(cylinder >= sectorsData.Length)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

			if(head >= sectorsData[cylinder].Length)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

			if(sector > sectorsData[cylinder][head].Length)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

			return sectorsData[cylinder][head][sector];
		}

		public override byte[] ReadSectors(ulong sectorAddress, uint length)
		{
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

			if(sectorAddress + length > ImageInfo.sectors)
				throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

			MemoryStream buffer = new MemoryStream();
			for(uint i = 0; i < length; i++)
			{
				byte[] sector = ReadSector(sectorAddress + i);
				buffer.Write(sector, 0, sector.Length);
			}

			return buffer.ToArray();
		}

		(ushort cylinder, byte head, byte sector) LbaToChs(ulong lba)
		{
			ushort cylinder = (ushort)(lba / (ImageInfo.heads * ImageInfo.sectorsPerTrack));
			byte head = (byte)((lba / ImageInfo.sectorsPerTrack) % ImageInfo.heads);
			byte sector = (byte)((lba % ImageInfo.sectorsPerTrack) + 1);

			return (cylinder, head, sector);
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