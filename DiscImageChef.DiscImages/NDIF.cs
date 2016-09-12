// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : NDIF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Apple New Disk Image Format.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Claunia.RsrcFork;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.DiscImages
{
	// TODO: Detect OS X encrypted images
	// TODO: Check checksum
	// TODO: Implement segments
	// TODO: Implement compression
	public class NDIF : ImagePlugin
	{
		#region Internal constants
		/// <summary>
		/// Resource OSType for NDIF is "bcem"
		/// </summary>
		const uint NDIF_Resource = 0x6263656D;
		/// <summary>
		/// Resource ID is always 128? Never found another
		/// </summary>
		const short NDIF_ResourceID = 128;
		#endregion

		#region Internal Structures
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct ChunkHeader
		{
			/// <summary>
			/// Version
			/// </summary>
			public short version;
			/// <summary>
			/// Filesystem ID
			/// </summary>
			public short driver;
			/// <summary>
			/// Disk image name, Str63 (Pascal string)
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
			public byte[] name;
			/// <summary>
			/// Sectors in image
			/// </summary>
			public uint sectors;
			/// <summary>
			/// Maximum number of sectors per chunk
			/// </summary>
			public uint maxSectorsPerChunk;
			/// <summary>
			/// Offset to add to every chunk offset
			/// </summary>
			public uint dataOffset;
			/// <summary>
			/// CRC28 of whole image
			/// </summary>
			public uint crc;
			/// <summary>
			/// Set to 1 if segmented
			/// </summary>
			public uint segmented;
			/// <summary>
			/// Unknown
			/// </summary>
			public uint p1;
			/// <summary>
			/// Unknown
			/// </summary>
			public uint p2;
			/// <summary>
			/// Unknown, spare?
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
			public uint[] unknown;
			/// <summary>
			/// Set to 1 by ShrinkWrap if image is encrypted
			/// </summary>
			public uint encrypted;
			/// <summary>
			/// Set by ShrinkWrap if image is encrypted, value is the same for same password
			/// </summary>
			public uint hash;
			/// <summary>
			/// How many chunks follow the header
			/// </summary>
			public uint chunks;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct BlockChunk
		{
			/// <summary>
			/// Starting sector, 3 bytes
			/// </summary>
			public uint sector;
			/// <summary>
			/// Chunk type
			/// </summary>
			public byte type;
			/// <summary>
			/// Offset in start of chunk
			/// </summary>
			public uint offset;
			/// <summary>
			/// Length in bytes of chunk
			/// </summary>
			public uint length;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct SegmentHeader
		{
			/// <summary>
			/// Segment #
			/// </summary>
			public ushort segment;
			/// <summary>
			/// How many segments
			/// </summary>
			public ushort segments;
			/// <summary>
			/// Seems to be a Guid, changes with different images, same for all segments of same image
			/// </summary>
			public Guid segmentId;
			/// <summary>
			/// Seems to be a CRC28 of this segment, unchecked
			/// </summary>
			public uint crc;
		}
		#endregion

		ChunkHeader header;
		Dictionary<ulong, BlockChunk> chunks;

		const byte ChunkType_NoCopy = 0;
		const byte ChunkType_Copy = 2;
		const byte ChunkType_KenCode = 0x80;
		const byte ChunkType_RLE = 0x81;
		const byte ChunkType_LZH = 0x82;
		const byte ChunkType_ADC = 0x83;
		/// <summary>
		/// Created by ShrinkWrap 3.5, dunno which version of the StuffIt algorithm it is using
		/// </summary>
		const byte ChunkType_StuffIt = 0xF0;
		const byte ChunkType_End = 0xFF;

		const byte ChunkType_CompressedMask = 0x80;

		const short Driver_OSX = -1;
		const short Driver_HFS = 0;
		const short Driver_ProDOS = 256;
		const short Driver_DOS = 18771;

		Dictionary<ulong, byte[]> sectorCache;
		const uint MaxCacheSize = 16777216;
		const uint sectorSize = 512;
		uint maxCachedSectors = MaxCacheSize / sectorSize;

		Stream imageStream;

		public NDIF()
		{
			Name = "Apple New Disk Image Format";
			PluginUUID = new Guid("5A7FF7D8-491E-458D-8674-5B5EADBECC24");
			ImageInfo = new ImageInfo();
			ImageInfo.readableSectorTags = new List<SectorTagType>();
			ImageInfo.readableMediaTags = new List<MediaTagType>();
			ImageInfo.imageHasPartitions = false;
			ImageInfo.imageHasSessions = false;
			ImageInfo.imageVersion = null;
			ImageInfo.imageApplication = null;
			ImageInfo.imageApplicationVersion = null;
			ImageInfo.imageCreator = null;
			ImageInfo.imageComments = null;
			ImageInfo.mediaManufacturer = null;
			ImageInfo.mediaModel = null;
			ImageInfo.mediaSerialNumber = null;
			ImageInfo.mediaBarcode = null;
			ImageInfo.mediaPartNumber = null;
			ImageInfo.mediaSequence = 0;
			ImageInfo.lastMediaSequence = 0;
			ImageInfo.driveManufacturer = null;
			ImageInfo.driveModel = null;
			ImageInfo.driveSerialNumber = null;
			ImageInfo.driveFirmwareRevision = null;
		}

		public override bool IdentifyImage(Filter imageFilter)
		{
			if(!imageFilter.HasResourceFork() || imageFilter.GetResourceForkLength() == 0)
				return false;

			ResourceFork rsrcFork;

			try
			{
				rsrcFork = new ResourceFork(imageFilter.GetResourceForkStream());
				if(!rsrcFork.ContainsKey(NDIF_Resource))
					return false;

				Resource rsrc = rsrcFork.GetResource(NDIF_Resource);

				if(rsrc.ContainsId(NDIF_ResourceID))
					return true;
			}
			catch(InvalidCastException)
			{
				return false;
			}

			return false;
		}

		public override bool OpenImage(Filter imageFilter)
		{
			if(!imageFilter.HasResourceFork() || imageFilter.GetResourceForkLength() == 0)
				return false;

			ResourceFork rsrcFork;
			Resource rsrc;
			short[] bcems;

			try
			{
				rsrcFork = new ResourceFork(imageFilter.GetResourceForkStream());
				if(!rsrcFork.ContainsKey(NDIF_Resource))
					return false;

				rsrc = rsrcFork.GetResource(NDIF_Resource);

				bcems = rsrc.GetIds();

				if(bcems == null || bcems.Length == 0)
					return false;
			}
			catch(InvalidCastException)
			{
				return false;
			}

			ImageInfo.sectors = 0;
			foreach(short id in bcems)
			{
				byte[] bcem = rsrc.GetResource(NDIF_ResourceID);

				if(bcem.Length < 128)
					return false;

				header = BigEndianMarshal.ByteArrayToStructureBigEndian<ChunkHeader>(bcem);

				DicConsole.DebugWriteLine("NDIF plugin", "footer.type = {0}", header.version);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.driver = {0}", header.driver);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.name = {0}", StringHandlers.PascalToString(header.name));
				DicConsole.DebugWriteLine("NDIF plugin", "footer.sectors = {0}", header.sectors);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.maxSectorsPerChunk = {0}", header.maxSectorsPerChunk);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.dataOffset = {0}", header.dataOffset);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.crc = 0x{0:X7}", header.crc);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.segmented = {0}", header.segmented);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.p1 = 0x{0:X8}", header.p1);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.p2 = 0x{0:X8}", header.p2);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.unknown[0] = 0x{0:X8}", header.unknown[0]);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.unknown[1] = 0x{0:X8}", header.unknown[1]);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.unknown[2] = 0x{0:X8}", header.unknown[2]);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.unknown[3] = 0x{0:X8}", header.unknown[3]);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.unknown[4] = 0x{0:X8}", header.unknown[4]);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.encrypted = {0}", header.encrypted);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.hash = 0x{0:X8}", header.hash);
				DicConsole.DebugWriteLine("NDIF plugin", "footer.chunks = {0}", header.chunks);

				// Block chunks and headers
				chunks = new Dictionary<ulong, BlockChunk>();

				BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

				for(int i = 0; i < header.chunks; i++)
				{
					// Obsolete read-only NDIF only prepended the header and then put the image without any kind of block references.
					// So let's falsify a block chunk
					BlockChunk bChnk = new BlockChunk();
					byte[] sector = new byte[4];
					Array.Copy(bcem, 128 + 0 + i * 12, sector, 1, 3);
					bChnk.sector = BigEndianBitConverter.ToUInt32(sector, 0);
					bChnk.type = bcem[128 + 3 + i * 12];
					bChnk.offset = BigEndianBitConverter.ToUInt32(bcem, 128 + 4 + i * 12);
					bChnk.length = BigEndianBitConverter.ToUInt32(bcem, 128 + 8 + i * 12);

					DicConsole.DebugWriteLine("NDIF plugin", "bHdr.chunk[{0}].type = 0x{1:X2}", i, bChnk.type);
					DicConsole.DebugWriteLine("NDIF plugin", "bHdr.chunk[{0}].sector = {1}", i, bChnk.sector);
					DicConsole.DebugWriteLine("NDIF plugin", "bHdr.chunk[{0}].offset = {1}", i, bChnk.offset);
					DicConsole.DebugWriteLine("NDIF plugin", "bHdr.chunk[{0}].length = {1}", i, bChnk.length);

					if(bChnk.type == ChunkType_End)
						break;

					bChnk.offset += header.dataOffset;
					bChnk.sector += (uint)ImageInfo.sectors;

					// TODO: Handle compressed chunks
					if((bChnk.type == ChunkType_KenCode))
						throw new ImageNotSupportedException("Chunks compressed with KenCode are not yet supported.");
					if((bChnk.type == ChunkType_RLE))
						throw new ImageNotSupportedException("Chunks compressed with RLE are not yet supported.");
					if((bChnk.type == ChunkType_LZH))
						throw new ImageNotSupportedException("Chunks compressed with LZH are not yet supported.");
					if((bChnk.type == ChunkType_ADC))
						throw new ImageNotSupportedException("Chunks compressed with ADC are not yet supported.");

					// TODO: Handle compressed chunks
					if((bChnk.type > ChunkType_Copy && bChnk.type < ChunkType_KenCode) ||
					   (bChnk.type > ChunkType_ADC && bChnk.type < ChunkType_StuffIt) ||
					   (bChnk.type > ChunkType_StuffIt && bChnk.type < ChunkType_End) ||
					   bChnk.type == 1)
						throw new ImageNotSupportedException(string.Format("Unsupported chunk type 0x{0:X8} found", bChnk.type));

					chunks.Add(bChnk.sector, bChnk);
				}

				ImageInfo.sectors += header.sectors;
			}

			if(header.segmented > 0)
				throw new ImageNotSupportedException("Segmented images are not yet supported.");

			if(header.encrypted > 0)
				throw new ImageNotSupportedException("Encrypted images are not yet supported.");

			switch(ImageInfo.sectors)
			{
				case 1440:
					ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;
					break;
				case 1600:
					ImageInfo.mediaType = MediaType.AppleSonyDS;
					break;
				case 2880:
					ImageInfo.mediaType = MediaType.DOS_35_HD;
					break;
				case 3360:
					ImageInfo.mediaType = MediaType.DMF;
					break;
				default:
					ImageInfo.mediaType = MediaType.GENERIC_HDD;
					break;
			}

			if(rsrcFork.ContainsKey(0x76657273))
			{
				Resource versRsrc = rsrcFork.GetResource(0x76657273);
				if(versRsrc != null)
				{
					byte[] vers = versRsrc.GetResource(versRsrc.GetIds()[0]);

					Resources.Version version = new Resources.Version(vers);

					string major;
					string minor;
					string release = null;
					string dev = null;
					string pre = null;

					major = string.Format("{0}", version.MajorVersion);
					minor = string.Format(".{0}", version.MinorVersion / 10);
					if(version.MinorVersion % 10 > 0)
						release = string.Format(".{0}", version.MinorVersion % 10);
					switch(version.DevStage)
					{
						case Resources.Version.DevelopmentStage.Alpha:
							dev = "a";
							break;
						case Resources.Version.DevelopmentStage.Beta:
							dev = "b";
							break;
						case Resources.Version.DevelopmentStage.PreAlpha:
							dev = "d";
							break;
					}

					if(dev == null && version.PreReleaseVersion > 0)
						dev = "f";

					if(dev != null)
						pre = string.Format("{0}", version.PreReleaseVersion);

					ImageInfo.imageApplicationVersion = string.Format("{0}{1}{2}{3}{4}", major, minor, release, dev, pre);
					ImageInfo.imageApplication = version.VersionString;
					ImageInfo.imageComments = version.VersionMessage;

					if(version.MajorVersion == 3)
						ImageInfo.imageApplication = "ShrinkWrap™";
					else if(version.MajorVersion == 6)
						ImageInfo.imageApplication = "DiskCopy";
				}
			}
			DicConsole.DebugWriteLine("NDIF plugin", "Image application = {0} version {1}", ImageInfo.imageApplication, ImageInfo.imageApplicationVersion);

			sectorCache = new Dictionary<ulong, byte[]>();
			imageStream = imageFilter.GetDataForkStream();

			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
			ImageInfo.imageName = StringHandlers.PascalToString(header.name);
			ImageInfo.sectorSize = sectorSize;
			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
			ImageInfo.imageSize = ImageInfo.sectors * sectorSize;
			ImageInfo.imageApplicationVersion = "6";
			ImageInfo.imageApplication = "Apple DiskCopy";

			return true;
		}

		public override byte[] ReadSector(ulong sectorAddress)
		{
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

			byte[] sector;

			if(sectorCache.TryGetValue(sectorAddress, out sector))
				return sector;

			BlockChunk currentChunk = new BlockChunk();
			bool chunkFound = false;
			ulong chunkStartSector = 0;

			foreach(KeyValuePair<ulong, BlockChunk> kvp in chunks)
			{
				if(sectorAddress >= kvp.Key)
				{
					currentChunk = kvp.Value;
					chunkFound = true;
					chunkStartSector = kvp.Key;
				}
			}

			long relOff = ((long)sectorAddress - (long)chunkStartSector) * sectorSize;

			if(relOff < 0)
				throw new ArgumentOutOfRangeException(nameof(relOff), string.Format("Got a negative offset for sector {0}. This should not happen.", sectorAddress));

			if(!chunkFound)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

			if(currentChunk.type == ChunkType_NoCopy)
			{
				sector = new byte[sectorSize];

				if(sectorCache.Count >= maxCachedSectors)
					sectorCache.Clear();

				sectorCache.Add(sectorAddress, sector);
				return sector;
			}

			if(currentChunk.type == ChunkType_Copy)
			{
				imageStream.Seek(currentChunk.offset + relOff, SeekOrigin.Begin);
				sector = new byte[sectorSize];
				imageStream.Read(sector, 0, sector.Length);

				if(sectorCache.Count >= maxCachedSectors)
					sectorCache.Clear();

				sectorCache.Add(sectorAddress, sector);
				return sector;
			}

			throw new ImageNotSupportedException(string.Format("Unsupported chunk type 0x{0:X8} found", currentChunk.type));
		}

		public override byte[] ReadSectors(ulong sectorAddress, uint length)
		{
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

			if(sectorAddress + length > ImageInfo.sectors)
				throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

			MemoryStream ms = new MemoryStream();

			for(uint i = 0; i < length; i++)
			{
				byte[] sector = ReadSector(sectorAddress + i);
				ms.Write(sector, 0, sector.Length);
			}

			return ms.ToArray();
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
			return "Apple New Disk Image Format";
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

		#region Unsupported features

		public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadDiskTag(MediaTagType tag)
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

