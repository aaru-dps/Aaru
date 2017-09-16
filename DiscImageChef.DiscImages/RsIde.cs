// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RsIde.cs
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
using DiscImageChef.CommonTypes;
using System.Linq;
using DiscImageChef.Filters;
using System.Runtime.InteropServices;
using static DiscImageChef.Decoders.ATA.Identify;

namespace DiscImageChef.ImagePlugins
{
	public class RsIde : ImagePlugin
	{
		public RsIde()
		{
			Name = "RS-IDE Hard Disk Image";
			PluginUUID = new Guid("47C3E78D-2BE2-4BA5-AA6B-FEE27C86FC65");
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

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct RsIdeHeader
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
			public byte[] magic;
			public byte revision;
			public RsIdeFlags flags;
			public ushort dataOff;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
			public byte[] reserved;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 106)]
			public byte[] identify;
		}

		[Flags]
		enum RsIdeFlags : byte
		{
			HalfSectors = 1
		}

		Filter RsIdeImageFilter;
		ushort dataOff;
		readonly byte[] signature = { 0x52, 0x53, 0x2D, 0x49, 0x44, 0x45, 0x1A };
		byte[] identify;

		public override bool IdentifyImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);

			byte[] magic = new byte[7];
			stream.Read(magic, 0, magic.Length);

			return magic.SequenceEqual(signature);
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);

			byte[] hdr_b = new byte[Marshal.SizeOf(typeof(RsIdeHeader))];
			stream.Read(hdr_b, 0, hdr_b.Length);

			IntPtr hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RsIdeHeader)));
			Marshal.Copy(hdr_b, 0, hdrPtr, Marshal.SizeOf(typeof(RsIdeHeader)));
			RsIdeHeader hdr = (RsIdeHeader)Marshal.PtrToStructure(hdrPtr, typeof(RsIdeHeader));
			Marshal.FreeHGlobal(hdrPtr);

			if(!hdr.magic.SequenceEqual(signature))
				return false;

			dataOff = hdr.dataOff;

			ImageInfo.mediaType = MediaType.GENERIC_HDD;
			ImageInfo.sectorSize = (uint)(hdr.flags.HasFlag(RsIdeFlags.HalfSectors) ? 256 : 512);
			ImageInfo.imageSize = (ulong)(stream.Length - dataOff);
			ImageInfo.sectors = ImageInfo.imageSize / ImageInfo.sectorSize;
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
			ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
			ImageInfo.imageVersion = string.Format("{0}.{1}", hdr.revision >> 8, hdr.revision & 0x0F);

			if(!ArrayHelpers.ArrayIsNullOrEmpty(hdr.identify))
			{
				identify = new byte[512];
				Array.Copy(hdr.identify, 0, identify, 0, hdr.identify.Length);
				IdentifyDevice? ataId = Decode(identify);

				if(ataId.HasValue)
				{
					ImageInfo.readableMediaTags.Add(MediaTagType.ATA_IDENTIFY);
					ImageInfo.cylinders = ataId.Value.Cylinders;
					ImageInfo.heads = ataId.Value.Heads;
					ImageInfo.sectorsPerTrack = ataId.Value.SectorsPerCard;
					ImageInfo.driveFirmwareRevision = ataId.Value.FirmwareRevision;
					ImageInfo.driveModel = ataId.Value.Model;
					ImageInfo.driveSerialNumber = ataId.Value.SerialNumber;
					ImageInfo.mediaSerialNumber = ataId.Value.MediaSerial;
					ImageInfo.mediaManufacturer = ataId.Value.MediaManufacturer;
				}
			}

			if(ImageInfo.cylinders == 0 || ImageInfo.heads == 0 || ImageInfo.sectorsPerTrack == 0)
			{
				ImageInfo.cylinders = (uint)((ImageInfo.sectors / 16) / 63);
				ImageInfo.heads = 16;
				ImageInfo.sectorsPerTrack = 63;
			}

			RsIdeImageFilter = imageFilter;

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
			return "RS-IDE disk image";
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

			Stream stream = RsIdeImageFilter.GetDataForkStream();

			stream.Seek((long)(dataOff + sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);

			stream.Read(buffer, 0, (int)(length * ImageInfo.sectorSize));

			return buffer;
		}

		#region Unsupported features

		public override byte[] ReadDiskTag(MediaTagType tag)
		{
			if(ImageInfo.readableMediaTags.Contains(tag) && tag == MediaTagType.ATA_IDENTIFY)
			{
				byte[] buffer = new byte[512];
				Array.Copy(identify, 0, buffer, 0, 512);
				return buffer;
			}

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
			return ImageInfo.mediaManufacturer;
		}

		public override string GetMediaModel()
		{
			return null;
		}

		public override string GetMediaSerialNumber()
		{
			return ImageInfo.mediaSerialNumber;
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
			return ImageInfo.driveManufacturer;
		}

		public override string GetDriveModel()
		{
			return ImageInfo.driveModel;
		}

		public override string GetDriveSerialNumber()
		{
			return ImageInfo.driveSerialNumber;
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