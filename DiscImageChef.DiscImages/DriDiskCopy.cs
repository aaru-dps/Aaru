// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DriDiskCopy.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Digital Research's DISKCOPY disk images.
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
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
	public class DriDiskCopy : ImagePlugin
	{
		#region Internal Structures

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct DRIFooter
		{
			/// <summary>Signature: "DiskImage 2.01 (C) 1990,1991 Digital Research Inc\0"</summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 51)]
			public byte[] signature;
			/// <summary>Information about the disk image, mostly imitates FAT BPB</summary>
			public DRIBPB bpb;
			/// <summary>Information about the disk image, mostly imitates FAT BPB, copy</summary>
			public DRIBPB bpbcopy;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct DRIBPB
		{
			/// <summary>Seems to be always 0x05</summary>
			public byte five;
			/// <summary>A drive code that corresponds (but it not equal to) CMOS drive types</summary>
			public DRIDriveCodes driveCode;
			/// <summary>Unknown seems to be always 2</summary>
			public ushort unknown;
			/// <summary>Cylinders</summary>
			public ushort cylinders;
			/// <summary>Seems to always be 0</summary>
			public byte unknown2;
			/// <summary>Bytes per sector</summary>
			public ushort bps;
			/// <summary>Sectors per cluster</summary>
			public byte spc;
			/// <summary>Sectors between BPB and FAT</summary>
			public ushort rsectors;
			/// <summary>How many FATs</summary>
			public byte fats_no;
			/// <summary>Entries in root directory</summary>
			public ushort root_entries;
			/// <summary>Total sectors</summary>
			public ushort sectors;
			/// <summary>Media descriptor</summary>
			public byte media_descriptor;
			/// <summary>Sectors per FAT</summary>
			public ushort spfat;
			/// <summary>Sectors per track</summary>
			public ushort sptrack;
			/// <summary>Heads</summary>
			public ushort heads;
			/// <summary>Hidden sectors before BPB</summary>
			public uint hsectors;
			/// <summary>Drive number</summary>
			public byte drive_no;
			/// <summary>Seems to be 0</summary>
			public ulong unknown3;
			/// <summary>Seems to be 0</summary>
			public byte unknown4;
			/// <summary>Sectors per track (again?)</summary>
			public ushort sptrack2;
			/// <summary>Seems to be 0</summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 144)]
			public byte[] unknown5;
		}

		#endregion

		#region Internal Constants

		/// <summary>
		/// Drive codes change according to CMOS stored valued
		/// </summary>
		enum DRIDriveCodes : byte
		{
			/// <summary>5.25" 360k</summary>
			md2dd = 0,
			/// <summary>5.25" 1.2M</summary>
			md2hd = 1,
			/// <summary>3.5" 720k</summary>
			mf2dd = 2,
			/// <summary>3.5" 1.44M</summary>
			mf2hd = 7,
			/// <summary>3.5" 2.88M</summary>
			mf2ed = 9
		}

		const string DRIRegEx = "DiskImage\\s(?<version>\\d+.\\d+)\\s\\(C\\)\\s\\d+\\,*\\d*\\s+Digital Research Inc";

		#endregion

		#region Internal variables

		/// <summary>Footer of opened image</summary>
		DRIFooter footer;
		/// <summary>Disk image file</summary>
		Filter driImageFilter;

		#endregion

		public DriDiskCopy()
		{
			Name = "Digital Research DiskCopy";
			PluginUUID = new Guid("9F0BE551-8BAB-4038-8B5A-691F1BF5FFF3");
			ImageInfo = new ImageInfo();
			ImageInfo.readableSectorTags = new List<SectorTagType>();
			ImageInfo.readableMediaTags = new List<MediaTagType>();
			ImageInfo.imageHasPartitions = false;
			ImageInfo.imageHasSessions = false;
			ImageInfo.imageApplication = "DiskCopy";
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

			if((stream.Length - Marshal.SizeOf(typeof(DRIFooter))) % 512 != 0)
				return false;

			byte[] buffer = new byte[Marshal.SizeOf(typeof(DRIFooter))];
			stream.Seek(-buffer.Length, SeekOrigin.End);
			stream.Read(buffer, 0, buffer.Length);

			DRIFooter tmp_footer = new DRIFooter();
			IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
			Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
			tmp_footer = (DRIFooter)Marshal.PtrToStructure(ftrPtr, typeof(DRIFooter));
			Marshal.FreeHGlobal(ftrPtr);

			string sig = StringHandlers.CToString(tmp_footer.signature);

			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.signature = \"{0}\"", sig);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.five = {0}", tmp_footer.bpb.five);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.driveCode = {0}", tmp_footer.bpb.driveCode);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown = {0}", tmp_footer.bpb.unknown);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.cylinders = {0}", tmp_footer.bpb.cylinders);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown2 = {0}", tmp_footer.bpb.unknown2);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.bps = {0}", tmp_footer.bpb.bps);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.spc = {0}", tmp_footer.bpb.spc);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.rsectors = {0}", tmp_footer.bpb.rsectors);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.fats_no = {0}", tmp_footer.bpb.fats_no);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.sectors = {0}", tmp_footer.bpb.sectors);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.media_descriptor = {0}", tmp_footer.bpb.media_descriptor);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.spfat = {0}", tmp_footer.bpb.spfat);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.sptrack = {0}", tmp_footer.bpb.sptrack);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.heads = {0}", tmp_footer.bpb.heads);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.hsectors = {0}", tmp_footer.bpb.hsectors);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.drive_no = {0}", tmp_footer.bpb.drive_no);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown3 = {0}", tmp_footer.bpb.unknown3);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.unknown4 = {0}", tmp_footer.bpb.unknown4);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "tmp_footer.bpb.sptrack2 = {0}", tmp_footer.bpb.sptrack2);
			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "ArrayHelpers.ArrayIsNullOrEmpty(tmp_footer.bpb.unknown5) = {0}", ArrayHelpers.ArrayIsNullOrEmpty(tmp_footer.bpb.unknown5));

			Regex RegexSignature = new Regex(DRIRegEx);
			Match MatchSignature = RegexSignature.Match(sig);

			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "MatchSignature.Success? = {0}", MatchSignature.Success);

			if(!MatchSignature.Success)
				return false;
			
			if(tmp_footer.bpb.sptrack * tmp_footer.bpb.cylinders * tmp_footer.bpb.heads != tmp_footer.bpb.sectors)
				return false;

			if((tmp_footer.bpb.sectors * tmp_footer.bpb.bps) + Marshal.SizeOf(tmp_footer) != stream.Length)
				return false;

			return true;
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();

			if((stream.Length - Marshal.SizeOf(typeof(DRIFooter))) % 512 != 0)
				return false;

			byte[] buffer = new byte[Marshal.SizeOf(typeof(DRIFooter))];
			stream.Seek(-buffer.Length, SeekOrigin.End);
			stream.Read(buffer, 0, buffer.Length);

			footer = new DRIFooter();
			IntPtr ftrPtr = Marshal.AllocHGlobal(buffer.Length);
			Marshal.Copy(buffer, 0, ftrPtr, buffer.Length);
			footer = (DRIFooter)Marshal.PtrToStructure(ftrPtr, typeof(DRIFooter));
			Marshal.FreeHGlobal(ftrPtr);

			string sig = StringHandlers.CToString(footer.signature);

			Regex RegexSignature = new Regex(DRIRegEx);
			Match MatchSignature = RegexSignature.Match(sig);

			if(!MatchSignature.Success)
				return false;

			if(footer.bpb.sptrack * footer.bpb.cylinders * footer.bpb.heads != footer.bpb.sectors)
				return false;

			if((footer.bpb.sectors * footer.bpb.bps) + Marshal.SizeOf(footer) != stream.Length)
				return false;

			ImageInfo.cylinders = footer.bpb.cylinders;
			ImageInfo.heads = footer.bpb.heads;
			ImageInfo.sectorsPerTrack = footer.bpb.sptrack;
			ImageInfo.sectors = footer.bpb.sectors;
			ImageInfo.sectorSize = footer.bpb.bps;
			ImageInfo.imageApplicationVersion = MatchSignature.Groups["version"].Value;

			driImageFilter = imageFilter;

			ImageInfo.imageSize = (ulong)(stream.Length - Marshal.SizeOf(footer));
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();

			DicConsole.DebugWriteLine("DRI DiskCopy plugin", "Image application = {0} version {1}", ImageInfo.imageApplication, ImageInfo.imageApplicationVersion);

			// Correct some incorrect data in images of NEC 2HD disks
			if(ImageInfo.cylinders == 77 && ImageInfo.heads == 2 && ImageInfo.sectorsPerTrack == 16 && ImageInfo.sectorSize == 512 && (footer.bpb.driveCode == DRIDriveCodes.md2hd || footer.bpb.driveCode == DRIDriveCodes.mf2hd))
			{
				ImageInfo.sectorsPerTrack = 8;
				ImageInfo.sectorSize = 1024;
			}

			switch(footer.bpb.driveCode)
			{
				case DRIDriveCodes.md2hd:
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
					else
						goto case DRIDriveCodes.md2dd;
					break;
				case DRIDriveCodes.md2dd:
					if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 512)
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
					else
						ImageInfo.mediaType = MediaType.Unknown;
					break;
				case DRIDriveCodes.mf2ed:
					if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 36 && ImageInfo.sectorSize == 512)
						ImageInfo.mediaType = MediaType.DOS_35_ED;
					else
						goto case DRIDriveCodes.mf2hd;
					break;
				case DRIDriveCodes.mf2hd:
					if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 18 && ImageInfo.sectorSize == 512)
						ImageInfo.mediaType = MediaType.DOS_35_HD;
					else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 21 && ImageInfo.sectorSize == 512)
						ImageInfo.mediaType = MediaType.DMF;
					else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 82 && ImageInfo.sectorsPerTrack == 21 && ImageInfo.sectorSize == 512)
						ImageInfo.mediaType = MediaType.DMF_82;
					else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 77 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 1024)
						ImageInfo.mediaType = MediaType.NEC_35_HD_8;
					else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 15 && ImageInfo.sectorSize == 512)
						ImageInfo.mediaType = MediaType.NEC_35_HD_15;
					else
						goto case DRIDriveCodes.mf2dd;
					break;
				case DRIDriveCodes.mf2dd:
					if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 9 && ImageInfo.sectorSize == 512)
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
					break;
				default:
					ImageInfo.mediaType = MediaType.Unknown;
					break;
			}

			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
			DicConsole.VerboseWriteLine("Digital Research DiskCopy image contains a disk of type {0}", ImageInfo.mediaType);

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

			Stream stream = driImageFilter.GetDataForkStream();
			stream.Seek((long)(sectorAddress * ImageInfo.sectorSize), SeekOrigin.Begin);
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
			return "Digital Research DiskCopy";
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

