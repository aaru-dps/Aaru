// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Anex86.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
	public class Anex86 : ImagePlugin
	{
		#region Internal structures
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct Anex86Header
		{
			public int unknown;
			public int unknown2;
			public int hdrSize;
			public int dskSize;
			public int bps;
			public int spt;
			public int heads;
			public int cylinders;
		}
		#endregion

		public Anex86()
		{
			Name = "Anex86 Disk Image";
			PluginUUID = new Guid("0410003E-6E7B-40E6-9328-BA5651ADF6B7");
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

		Anex86Header fdihdr;
		Filter anexImageFilter;

		public override bool IdentifyImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);

			fdihdr = new Anex86Header();

			if(stream.Length < Marshal.SizeOf(fdihdr))
				return false;

			byte[] hdr_b = new byte[Marshal.SizeOf(fdihdr)];
			stream.Read(hdr_b, 0, hdr_b.Length);

			GCHandle handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
			fdihdr = (Anex86Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Anex86Header));
			handle.Free();

			DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.unknown = {0}", fdihdr.unknown);
			DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.unknown2 = {0}", fdihdr.unknown2);
			DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.hdrSize = {0}", fdihdr.hdrSize);
			DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.dskSize = {0}", fdihdr.dskSize);
			DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.bps = {0}", fdihdr.bps);
			DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.spt = {0}", fdihdr.spt);
			DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.heads = {0}", fdihdr.heads);
			DicConsole.DebugWriteLine("Anex86 plugin", "fdihdr.cylinders = {0}", fdihdr.cylinders);

			return stream.Length == fdihdr.hdrSize + fdihdr.dskSize && fdihdr.dskSize == fdihdr.bps * fdihdr.spt * fdihdr.heads * fdihdr.cylinders;
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);

			fdihdr = new Anex86Header();

			if(stream.Length < Marshal.SizeOf(fdihdr))
				return false;

			byte[] hdr_b = new byte[Marshal.SizeOf(fdihdr)];
			stream.Read(hdr_b, 0, hdr_b.Length);

			GCHandle handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
			fdihdr = (Anex86Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Anex86Header));
			handle.Free();

			ImageInfo.mediaType = MediaType.GENERIC_HDD;

			switch(fdihdr.cylinders)
			{
				case 40:
					switch(fdihdr.bps)
					{
						case 512:
							switch(fdihdr.spt)
							{
								case 8:
									if(fdihdr.heads == 1)
										ImageInfo.mediaType = MediaType.DOS_525_SS_DD_8;
									else if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.DOS_525_DS_DD_8;
									break;
								case 9:
									if(fdihdr.heads == 1)
										ImageInfo.mediaType = MediaType.DOS_525_SS_DD_9;
									else if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.DOS_525_DS_DD_9;
									break;
							}
							break;
					}
					break;
				case 77:
					switch(fdihdr.bps)
					{
						case 128:
							switch(fdihdr.spt)
							{
								case 26:
									if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.NEC_8_SD;
									break;
							}
							break;
						case 256:
							switch(fdihdr.spt)
							{
								case 26:
									if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.NEC_8_DD;
									break;
							}
							break;
						case 1024:
							switch(fdihdr.spt)
							{
								case 8:
									if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.NEC_525_HD;
									break;
							}
							break;
					}
					break;
				case 80:
					switch(fdihdr.bps)
					{
						case 256:
							switch(fdihdr.spt)
							{
								case 16:
									if(fdihdr.heads == 1)
										ImageInfo.mediaType = MediaType.NEC_525_SS;
									else if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.NEC_525_DS;
									break;
							}
							break;
						case 512:
							switch(fdihdr.spt)
							{
								case 8:
									if(fdihdr.heads == 1)
										ImageInfo.mediaType = MediaType.DOS_35_SS_DD_8;
									else if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.DOS_35_DS_DD_8;
									break;
								case 9:
									if(fdihdr.heads == 1)
										ImageInfo.mediaType = MediaType.DOS_35_SS_DD_9;
									else if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;
									break;
								case 15:
									if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.NEC_35_HD_15;
									break;
								case 18:
									if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.DOS_35_HD;
									break;
								case 36:
									if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.DOS_35_ED;
									break;
							}
							break;
					}
					break;
				case 240:
					switch(fdihdr.bps)
					{
						case 512:
							switch(fdihdr.spt)
							{
								case 38:
									if(fdihdr.heads == 2)
										ImageInfo.mediaType = MediaType.NEC_35_TD;
									break;
							}
							break;
					}
					break;
			}
			DicConsole.DebugWriteLine("Anex86 plugin", "MediaType: {0}", ImageInfo.mediaType);

			ImageInfo.imageSize = (ulong)fdihdr.dskSize;
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
			ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
			ImageInfo.sectors = (ulong)(fdihdr.cylinders * fdihdr.heads * fdihdr.spt);
			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
			ImageInfo.sectorSize = (uint)fdihdr.bps;

			anexImageFilter = imageFilter;

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
			return "Anex86 disk image";
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

			byte[] buffer = new byte[length * ImageInfo.sectorSize];

			Stream stream = anexImageFilter.GetDataForkStream();

			stream.Seek((long)((ulong)fdihdr.hdrSize + sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);

			stream.Read(buffer, 0, (int)(length * ImageInfo.sectorSize));

			return buffer;
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