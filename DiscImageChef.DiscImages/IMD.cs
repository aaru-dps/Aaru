// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IMD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Sydex IMD disc images.
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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using System.Text;
using System.Linq;

namespace DiscImageChef.ImagePlugins
{
	public class IMD : ImagePlugin
	{
		#region Internal enumerations
		enum TransferRate : byte
		{
			/// <summary>500 kbps in FM mode</summary>
			FiveHundred = 0,
			/// <summary>300 kbps in FM mode</summary>
			ThreeHundred = 1,
			/// <summary>250 kbps in FM mode</summary>
			TwoHundred = 2,
			/// <summary>500 kbps in MFM mode</summary>
			FiveHundredMFM = 3,
			/// <summary>300 kbps in MFM mode</summary>
			ThreeHundredMFM = 4,
			/// <summary>250 kbps in MFM mode</summary>
			TwoHundredMFM = 5
		}

		enum SectorType : byte
		{
			Unavailable = 0,
			Normal = 1,
			Compressed = 2,
			Deleted = 3,
			CompressedDeleted = 4,
			Error = 5,
			CompressedError = 6,
			DeletedError = 7,
			CompressedDeletedError = 8
		}
		#endregion Internal enumerations

		#region Internal Constants
		const byte SectorCylinderMapMask = 0x80;
		const byte SectorHeadMapMask = 0x40;
		const byte CommentEnd = 0x1A;
		const string HeaderRegEx = "IMD (?<version>\\d.\\d+):\\s+(?<day>\\d+)\\/\\s*(?<month>\\d+)\\/(?<year>\\d+)\\s+(?<hour>\\d+):(?<minute>\\d+):(?<second>\\d+)\\r\\n";
		#endregion Internal Constants

		#region Internal variables
		List<byte[]> sectorsData;
		#endregion Internal variables

		public IMD()
		{
			Name = "Dunfield's IMD";
			PluginUUID = new Guid("0D67162E-38A3-407D-9B1A-CF40080A48CB");
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

		#region Public methods
		public override bool IdentifyImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);
			if(stream.Length < 31)
				return false;

			byte[] hdr = new byte[31];
			stream.Read(hdr, 0, 31);

			Regex Hr = new Regex(HeaderRegEx);
			Match Hm = Hr.Match(Encoding.ASCII.GetString(hdr));

			return Hm.Success;
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);

			MemoryStream cmt = new MemoryStream();
			stream.Seek(0x1F, SeekOrigin.Begin);
			for(uint i = 0; i < stream.Length; i++)
			{
				byte b = (byte)stream.ReadByte();
				if(b == 0x1A)
					break;
				cmt.WriteByte(b);
			}
			ImageInfo.imageComments = StringHandlers.CToString(cmt.ToArray());
			sectorsData = new List<byte[]>();

			byte currentCylinder = 0;
			ImageInfo.cylinders = 1;
			ImageInfo.heads = 1;
			ulong currentLba = 0;

			TransferRate mode = TransferRate.TwoHundred;

			while(stream.Position + 5 < stream.Length)
			{
				mode = (TransferRate)stream.ReadByte();
				byte cylinder = (byte)stream.ReadByte();
				byte head = (byte)stream.ReadByte();
				byte spt = (byte)stream.ReadByte();
				byte n = (byte)stream.ReadByte();
				byte[] idmap = new byte[spt];
				byte[] cylmap = new byte[spt];
				byte[] headmap = new byte[spt];
				ushort[] bps = new ushort[spt];

				if(cylinder != currentCylinder)
				{
					currentCylinder = cylinder;
					ImageInfo.cylinders++;
				}

				if((head & 1) == 1)
					ImageInfo.heads = 2;

				stream.Read(idmap, 0, idmap.Length);
				if((head & SectorCylinderMapMask) == SectorCylinderMapMask)
					stream.Read(cylmap, 0, cylmap.Length);
				if((head & SectorHeadMapMask) == SectorHeadMapMask)
					stream.Read(headmap, 0, headmap.Length);
				if(n == 0xFF)
				{
					byte[] bpsbytes = new byte[spt * 2];
					stream.Read(bpsbytes, 0, bpsbytes.Length);
					for(int i = 0; i < spt; i++)
						bps[i] = BitConverter.ToUInt16(bpsbytes, i * 2);
				}
				else
				{
					for(int i = 0; i < spt; i++)
						bps[i] = (ushort)(128 << n);
				}

				if(spt > ImageInfo.sectorsPerTrack)
					ImageInfo.sectorsPerTrack = spt;

				SortedDictionary<byte, byte[]> track = new SortedDictionary<byte, byte[]>();

				for(int i = 0; i < spt; i++)
				{
					SectorType type = (SectorType)stream.ReadByte();
					byte[] data = new byte[bps[i]];

					// TODO; Handle disks with different bps in track 0
					if(bps[i] > ImageInfo.sectorSize)
						ImageInfo.sectorSize = bps[i];

					switch(type)
					{
						case SectorType.Unavailable:
							if(!track.ContainsKey(idmap[i]))
								track.Add(idmap[i], data);
							break;
						case SectorType.Normal:
						case SectorType.Deleted:
						case SectorType.Error:
						case SectorType.DeletedError:
							stream.Read(data, 0, data.Length);
							if(!track.ContainsKey(idmap[i]))
								track.Add(idmap[i], data);
							ImageInfo.imageSize += (ulong)data.Length;
							break;
						case SectorType.Compressed:
						case SectorType.CompressedDeleted:
						case SectorType.CompressedError:
						case SectorType.CompressedDeletedError:
							byte filling = (byte)stream.ReadByte();
							ArrayHelpers.ArrayFill(data, filling);
							if(!track.ContainsKey(idmap[i]))
								track.Add(idmap[i], data);
							break;
						default:
							throw new ImageNotSupportedException(string.Format("Invalid sector type {0}", (byte)type));
					}
				}

				foreach(KeyValuePair<byte, byte[]> kvp in track)
				{
					sectorsData.Add(kvp.Value);
					currentLba++;
				}
			}

			ImageInfo.imageApplication = "IMD";
			// TODO: The header is the date of dump or the date of the application compilation?
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
			ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
			ImageInfo.imageComments = StringHandlers.CToString(cmt.ToArray());
			ImageInfo.sectors = currentLba;
			ImageInfo.mediaType = MediaType.Unknown;

			switch(mode)
			{
					case TransferRate.TwoHundred:
					case TransferRate.ThreeHundred:
						if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 10 && ImageInfo.sectorSize == 256)
							ImageInfo.mediaType = MediaType.ACORN_525_SS_SD_40;
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 10 && ImageInfo.sectorSize == 256)
							ImageInfo.mediaType = MediaType.ACORN_525_SS_SD_80;
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 18 && ImageInfo.sectorSize == 128)
							ImageInfo.mediaType = MediaType.ATARI_525_SD;
						break;
					case TransferRate.FiveHundred:
						if(ImageInfo.heads == 1 && ImageInfo.cylinders == 32 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 319)
							ImageInfo.mediaType = MediaType.IBM23FD;
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 73 && ImageInfo.sectorsPerTrack == 26 && ImageInfo.sectorSize == 128)
							ImageInfo.mediaType = MediaType.IBM23FD;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 77 && ImageInfo.sectorsPerTrack == 26 && ImageInfo.sectorSize == 128)
							ImageInfo.mediaType = MediaType.NEC_8_SD;
						break;
					case TransferRate.TwoHundredMFM:
					case TransferRate.ThreeHundredMFM:
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
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 16 && ImageInfo.sectorSize == 256)
							ImageInfo.mediaType = MediaType.ACORN_525_SS_DD_40;
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 16 && ImageInfo.sectorSize == 256)
							ImageInfo.mediaType = MediaType.ACORN_525_SS_DD_80;
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 40 && ImageInfo.sectorsPerTrack == 18 && ImageInfo.sectorSize == 256)
						ImageInfo.mediaType = MediaType.ATARI_525_DD;
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 10 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.RX50;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 9 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.DOS_35_DS_DD_8;
						if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 9 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.DOS_35_SS_DD_9;
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.DOS_35_SS_DD_8;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 5 && ImageInfo.sectorSize == 1024)
							ImageInfo.mediaType = MediaType.ACORN_35_DS_DD;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 82 && ImageInfo.sectorsPerTrack == 10 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.FDFORMAT_35_DD;
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 70 && ImageInfo.sectorsPerTrack == 9 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.Apricot_35;
						break;
					case TransferRate.FiveHundredMFM:
						if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 18 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.DOS_35_HD;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 21 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.DMF;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 82 && ImageInfo.sectorsPerTrack == 21 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.DMF_82;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 23 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.XDF_35;
					else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 15 && ImageInfo.sectorSize == 512)
						ImageInfo.mediaType = MediaType.DOS_525_HD;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 10 && ImageInfo.sectorSize == 1024)
							ImageInfo.mediaType = MediaType.ACORN_35_DS_HD;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 77 && ImageInfo.sectorsPerTrack == 8 && ImageInfo.sectorSize == 1024)
							ImageInfo.mediaType = MediaType.NEC_525_HD;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 9 && ImageInfo.sectorSize == 1024)
							ImageInfo.mediaType = MediaType.SHARP_525_9;
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 10 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.ATARI_35_SS_DD;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 10 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.ATARI_35_DS_DD;
						else if(ImageInfo.heads == 1 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 11 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.ATARI_35_SS_DD_11;
						else if(ImageInfo.heads == 2 && ImageInfo.cylinders == 80 && ImageInfo.sectorsPerTrack == 11 && ImageInfo.sectorSize == 512)
							ImageInfo.mediaType = MediaType.ATARI_35_DS_DD_11;
						break;
				default:
					ImageInfo.mediaType = MediaType.Unknown;
					break;
			}

			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;

			DicConsole.VerboseWriteLine("IMD image contains a disk of type {0}", ImageInfo.mediaType);
			if(!string.IsNullOrEmpty(ImageInfo.imageComments))
				DicConsole.VerboseWriteLine("IMD comments: {0}", ImageInfo.imageComments);

			/*
			FileStream debugFs = new FileStream("debug.img", FileMode.CreateNew, FileAccess.Write);
			for(ulong i = 0; i < ImageInfo.sectors; i++)
				debugFs.Write(ReadSector(i), 0, (int)ImageInfo.sectorSize);
			debugFs.Dispose();
			*/

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

		public override string GetImageFormat()
		{
			return "IMageDisk";
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
		#endregion Public methods

		#region Unsupported features

		public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
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

		public override byte[] ReadDiskTag(MediaTagType tag)
		{
			throw new FeatureUnsupportedImageException("Feature not supported by image format");
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

	}
}

