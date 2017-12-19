// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : T98.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages T98 disk images.
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
	public class T98 : ImagePlugin
	{
		public T98()
		{
			Name = "T98 Hard Disk Image";
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

		Filter t98ImageFilter;

		public override bool IdentifyImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);

			if(stream.Length % 256 != 0)
				return false;

			byte[] hdr_b = new byte[256];
			stream.Read(hdr_b, 0, hdr_b.Length);

			for(int i = 4; i < 256; i++)
			{
				if(hdr_b[i] != 0)
					return false;
			}

			int cylinders = BitConverter.ToInt32(hdr_b, 0);

			DicConsole.DebugWriteLine("T98 plugin", "cylinders = {0}", cylinders);

			// This format is expanding, so length can be smaller
			// Just grow it, I won't risk false positives...
			return stream.Length == (cylinders * 8 * 33 * 256) + 256;
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);

			if(stream.Length % 256 != 0)
				return false;

			byte[] hdr_b = new byte[256];
			stream.Read(hdr_b, 0, hdr_b.Length);

			for(int i = 4; i < 256; i++)
			{
				if(hdr_b[i] != 0)
					return false;
			}

			int cylinders = BitConverter.ToInt32(hdr_b, 0);

			ImageInfo.mediaType = MediaType.GENERIC_HDD;

			ImageInfo.imageSize = (ulong)(stream.Length - 256);
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
			ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
			ImageInfo.sectors = (ulong)((stream.Length / 256) - 1);
			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
			ImageInfo.sectorSize = 256;
			ImageInfo.cylinders = (uint)cylinders;
			ImageInfo.heads = 8;
			ImageInfo.sectorsPerTrack = 33;

			t98ImageFilter = imageFilter;

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
			return "T98 disk image";
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

			Stream stream = t98ImageFilter.GetDataForkStream();

			stream.Seek((long)(256 + sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);

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