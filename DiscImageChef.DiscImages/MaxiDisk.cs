// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MaxiDisk.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages MaxiDisk images.
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
	public class MaxiDisk : ImagePlugin
	{
		#region Internal Structures

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct HdkHeader
		{
			public byte unknown;
			public byte diskType;
			public byte heads;
			public byte cylinders;
			public byte bytesPerSector;
			public byte sectorsPerTrack;
			public byte unknown2;
			public byte unknown3;
		}

		enum HdkDiskTypes : byte
		{
			Dos360 = 0,
			Maxi420 = 1,
			Dos720 = 2,
			Maxi800 = 3,
			Dos1200 = 4,
			Maxi1400 = 5,
			Dos1440 = 6,
			Mac1440 = 7,
			Maxi1600 = 8,
			Dmf = 9,
			Dos2880 = 10,
			Maxi3200 = 11
		}
		#endregion

		#region Internal variables

		/// <summary>Disk image file</summary>
		Filter hdkImageFilter;

		#endregion

		public MaxiDisk()
		{
			Name = "MAXI Disk image";
			PluginUUID = new Guid("D27D924A-7034-466E-ADE1-B81EF37E469E");
			ImageInfo = new ImageInfo();
			ImageInfo.readableSectorTags = new List<SectorTagType>();
			ImageInfo.readableMediaTags = new List<MediaTagType>();
			ImageInfo.imageHasPartitions = false;
			ImageInfo.imageHasSessions = false;
			ImageInfo.imageApplication = "MAXI Disk";
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
			Stream stream = imageFilter.GetDataForkStream();

			if(stream.Length < 8)
				return false;

			byte[] buffer = new byte[8];
		   	stream.Seek(0, SeekOrigin.Begin);
			stream.Read(buffer, 0, buffer.Length);

			HdkHeader tmp_header = new HdkHeader();
			IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
			Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
			tmp_header = (HdkHeader)Marshal.PtrToStructure(ftrPtr, typeof(HdkHeader));
			Marshal.FreeHGlobal(ftrPtr);

			DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown = {0}", tmp_header.unknown);
			DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.diskType = {0}", tmp_header.diskType);
			DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.heads = {0}", tmp_header.heads);
			DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.cylinders = {0}", tmp_header.cylinders);
			DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.bytesPerSector = {0}", tmp_header.bytesPerSector);
			DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.sectorsPerTrack = {0}", tmp_header.sectorsPerTrack);
			DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown2 = {0}", tmp_header.unknown2);
			DicConsole.DebugWriteLine("MAXI Disk plugin", "tmp_header.unknown3 = {0}", tmp_header.unknown3);

			// This is hardcoded
			// But its possible values are unknown...
			//if(tmp_header.diskType > 11)
			//	return false;

			// Only floppies supported
			if(tmp_header.heads == 0 || tmp_header.heads > 2)
				return false;

			// No floppies with more than this?
			if(tmp_header.cylinders > 90)
				return false;

			// Maximum supported bps is 16384
			if(tmp_header.bytesPerSector > 7)
				return false;

			int expectedFileSize = tmp_header.heads * tmp_header.cylinders * tmp_header.sectorsPerTrack * (128 << tmp_header.bytesPerSector) + 8;

			return expectedFileSize == stream.Length;
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();

			if(stream.Length < 8)
				return false;

			byte[] buffer = new byte[8];
			stream.Seek(0, SeekOrigin.Begin);
			stream.Read(buffer, 0, buffer.Length);

			HdkHeader tmp_header = new HdkHeader();
			IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
			Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
			tmp_header = (HdkHeader)Marshal.PtrToStructure(ftrPtr, typeof(HdkHeader));
			Marshal.FreeHGlobal(ftrPtr);

			// This is hardcoded
			// But its possible values are unknown...
			//if(tmp_header.diskType > 11)
			//	return false;

			// Only floppies supported
			if(tmp_header.heads == 0 || tmp_header.heads > 2)
				return false;

			// No floppies with more than this?
			if(tmp_header.cylinders > 90)
				return false;

			// Maximum supported bps is 16384
			if(tmp_header.bytesPerSector > 7)
				return false;

			int expectedFileSize = tmp_header.heads * tmp_header.cylinders * tmp_header.sectorsPerTrack * (128 << tmp_header.bytesPerSector) + 8;

			if(expectedFileSize != stream.Length)
				return false;

			ImageInfo.cylinders = tmp_header.cylinders;
			ImageInfo.heads = tmp_header.heads;
			ImageInfo.sectorsPerTrack = tmp_header.sectorsPerTrack;
			ImageInfo.sectors = (ulong)(tmp_header.heads * tmp_header.cylinders * tmp_header.sectorsPerTrack);
			ImageInfo.sectorSize = (uint)(128 << tmp_header.bytesPerSector);

			hdkImageFilter = imageFilter;

			ImageInfo.imageSize = (ulong)(stream.Length - 8);
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();

			if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 15 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_525_HD;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 16 && ImageInfo.sectorSize == 256)
				ImageInfo.mediaType = MediaType.ACORN_525_DS_DD;
			else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 16 && ImageInfo.sectorSize == 256)
				ImageInfo.mediaType = MediaType.ACORN_525_SS_DD_80;
			else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 10 && ImageInfo.sectorSize == 256)
				ImageInfo.mediaType = MediaType.ACORN_525_SS_SD_80;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 77 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 1024)
				ImageInfo.mediaType = MediaType.NEC_525_HD;
			else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_525_SS_DD_8;
			else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 9 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_525_SS_DD_9;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_525_DS_DD_8;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 9 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_525_DS_DD_9;
			else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 18 && ImageInfo.sectorSize == 128)
				ImageInfo.mediaType = MediaType.ATARI_525_SD;
			else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 26 && ImageInfo.sectorSize == 128)
				ImageInfo.mediaType = MediaType.ATARI_525_ED;
			else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 18 && ImageInfo.sectorSize == 256)
				ImageInfo.mediaType = MediaType.ATARI_525_DD;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 36 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_35_ED;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 18 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_35_HD;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 21 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DMF;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 82 && ImageInfo.sectorsPerTrack == 21 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DMF_82;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 77 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 1024)
				ImageInfo.mediaType = MediaType.NEC_35_HD_8;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 15 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.NEC_35_HD_15;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 9 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_35_DS_DD_8;
			else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 9 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_35_SS_DD_9;
			else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.DOS_35_SS_DD_8;
			else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 5 && ImageInfo.sectorSize == 1024)
				ImageInfo.mediaType = MediaType.ACORN_35_DS_DD;
			else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 70 && ImageInfo.sectorsPerTrack == 9 && ImageInfo.sectorSize == 512)
				ImageInfo.mediaType = MediaType.Apricot_35;
			else
				ImageInfo.mediaType = MediaType.Unknown;

			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;

			return true;
		}

		public override bool? VerifySector(ulong sectorAddress)
		{
			return null;
		}

		public override bool? VerifySector(ulong sectorAddress, uint track)
		{
			return null;
		}

		public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
		{
			FailingLBAs = new List<ulong>();
			UnknownLBAs = new List<ulong>();

			for(ulong i = sectorAddress; i < sectorAddress + length; i++)
				UnknownLBAs.Add(i);

			return null;
		}

		public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
		{
			FailingLBAs = new List<ulong>();
			UnknownLBAs = new List<ulong>();

			for(ulong i = sectorAddress; i < sectorAddress + length; i++)
				UnknownLBAs.Add(i);

			return null;
		}

		public override bool? VerifyMediaImage()
		{
			return null;
		}

		public override bool ImageHasPartitions()
		{
			return ImageInfo.imageHasPartitions;
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

		public override byte[] ReadSector(ulong sectorAddress)
		{
			return ReadSectors(sectorAddress, 1);
		}

		public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectors(ulong sectorAddress, uint length)
		{
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

			if(sectorAddress + length > ImageInfo.sectors)
				throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

			byte[] buffer = new byte[length * ImageInfo.sectorSize];

			Stream stream = hdkImageFilter.GetDataForkStream();
			stream.Seek((long)(8 + sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);
			stream.Read(buffer, 0, (int)(length * ImageInfo.sectorSize));

			return buffer;
		}

		public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorLong(ulong sectorAddress)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override string GetImageFormat()
		{
			return "MAXI Disk";
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

		public override MediaType GetMediaType()
		{
			return ImageInfo.mediaType;
		}

		#region Unsupported features

		public override byte[] ReadDiskTag(MediaTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override string GetImageCreator()
		{
			return ImageInfo.imageCreator;
		}

		public override string GetImageComments()
		{
			return ImageInfo.imageComments;
		}

		public override string GetMediaManufacturer()
		{
			return ImageInfo.mediaManufacturer;
		}

		public override string GetMediaModel()
		{
			return ImageInfo.mediaModel;
		}

		public override string GetMediaSerialNumber()
		{
			return ImageInfo.mediaSerialNumber;
		}

		public override string GetMediaBarcode()
		{
			return ImageInfo.mediaBarcode;
		}

		public override string GetMediaPartNumber()
		{
			return ImageInfo.mediaPartNumber;
		}

		public override int GetMediaSequence()
		{
			return ImageInfo.mediaSequence;
		}

		public override int GetLastDiskSequence()
		{
			return ImageInfo.lastMediaSequence;
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

		public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
		}

		#endregion Unsupported features

		#region Private methods

		private static uint DC42CheckSum(byte[] buffer)
		{
			uint dc42chk = 0;
			if((buffer.Length & 0x01) == 0x01)
				return 0xFFFFFFFF;

			for(uint i = 0; i < buffer.Length; i += 2)
			{
				dc42chk += (uint)(buffer[i] << 8);
				dc42chk += buffer[i + 1];
				dc42chk = (dc42chk >> 1) | (dc42chk << 31);
			}
			return dc42chk;
		}

		#endregion
	}
}

