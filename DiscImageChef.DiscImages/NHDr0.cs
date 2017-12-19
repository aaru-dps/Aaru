// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : NHDr0.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages NHD r0 disk images.
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
	// Info from http://www.geocities.jp/t98next/nhdr0.txt
	public class NHDr0 : ImagePlugin
	{
		#region Internal structures
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct NHDr0Header
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
			public byte[] szFileID;
			public byte reserved1;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
			public byte[] szComment;
			public int dwHeadSize;
			public int dwCylinder;
			public short wHead;
			public short wSect;
			public short wSectLen;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
			public byte[] reserved2;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xE0)]
			public byte[] reserved3;
		}
		#endregion

		readonly byte[] signature = { 0x54, 0x39, 0x38, 0x48, 0x44, 0x44, 0x49, 0x4D, 0x41, 0x47, 0x45, 0x2E, 0x52, 0x30, 0x00 };

		public NHDr0()
		{
			Name = "T98-Next NHD r0 Disk Image";
			PluginUUID = new Guid("6ECACD0A-8F4D-4465-8815-AEA000D370E3");
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

		NHDr0Header nhdhdr;
		Filter nhdImageFilter;

		public override bool IdentifyImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);
			// Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
			Encoding shiftjis = Encoding.GetEncoding("shift_jis");

			nhdhdr = new NHDr0Header();

			if(stream.Length < Marshal.SizeOf(nhdhdr))
				return false;

			byte[] hdr_b = new byte[Marshal.SizeOf(nhdhdr)];
			stream.Read(hdr_b, 0, hdr_b.Length);

			GCHandle handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
			nhdhdr = (NHDr0Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(NHDr0Header));
			handle.Free();

			if(!nhdhdr.szFileID.SequenceEqual(signature))
				return false;

			DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.szFileID = \"{0}\"", StringHandlers.CToString(nhdhdr.szFileID, shiftjis));
			DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.reserved1 = {0}", nhdhdr.reserved1);
			DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.szComment = \"{0}\"", StringHandlers.CToString(nhdhdr.szComment, shiftjis));
			DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.dwHeadSize = {0}", nhdhdr.dwHeadSize);
			DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.dwCylinder = {0}", nhdhdr.dwCylinder);
			DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wHead = {0}", nhdhdr.wHead);
			DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wSect = {0}", nhdhdr.wSect);
			DicConsole.DebugWriteLine("NHDr0 plugin", "nhdhdr.wSectLen = {0}", nhdhdr.wSectLen);

			return true;
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);
			// Even if comment is supposedly ASCII, I'm pretty sure most emulators allow Shift-JIS to be used :p
			Encoding shiftjis = Encoding.GetEncoding("shift_jis");

			nhdhdr = new NHDr0Header();

			if(stream.Length < Marshal.SizeOf(nhdhdr))
				return false;

			byte[] hdr_b = new byte[Marshal.SizeOf(nhdhdr)];
			stream.Read(hdr_b, 0, hdr_b.Length);

			GCHandle handle = GCHandle.Alloc(hdr_b, GCHandleType.Pinned);
			nhdhdr = (NHDr0Header)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(NHDr0Header));
			handle.Free();

			ImageInfo.mediaType = MediaType.GENERIC_HDD;


			ImageInfo.imageSize = (ulong)(stream.Length - nhdhdr.dwHeadSize);
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
			ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
			ImageInfo.sectors = (ulong)(nhdhdr.dwCylinder * nhdhdr.wHead * nhdhdr.wSect);
			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
			ImageInfo.sectorSize = (uint)nhdhdr.wSectLen;
			ImageInfo.cylinders = (uint)nhdhdr.dwCylinder;
			ImageInfo.heads = (uint)nhdhdr.wHead;
			ImageInfo.sectorsPerTrack = (uint)nhdhdr.wSect;
			ImageInfo.imageComments = StringHandlers.CToString(nhdhdr.szComment, shiftjis);

			nhdImageFilter = imageFilter;

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
			return "NHDr0 disk image";
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

			Stream stream = nhdImageFilter.GetDataForkStream();

			stream.Seek((long)((ulong)nhdhdr.dwHeadSize + sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);

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