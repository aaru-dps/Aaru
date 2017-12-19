// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DART.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Apple Disk Archival/Retrieval Tool format.
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
using System.Text;
using System.Text.RegularExpressions;
using Claunia.RsrcFork;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.DiscImages
{
	public class DART : ImagePlugin
	{
		#region Internal constants
		// Disk types
		const byte kMacDisk = 1;
		const byte kLisaDisk = 2;
		const byte kAppleIIDisk = 3;
		const byte kMacHiDDisk = 16;
		const byte kMSDOSLowDDisk = 17;
		const byte kMSDOSHiDDisk = 18;

		// Compression types
		// "fast"
		const byte kRLECompress = 0;
		// "best"
		const byte kLZHCompress = 1;
		// DART <= 1.4
		const byte kNoCompress = 2;

		// Valid sizes
		const short kLisa400KSize = 400;
		const short kMac400KSize = 400;
		const short kMac800KSize = 800;
		const short kMac1440KSize = 1440;
		const short kApple800KSize = 800;
		const short kMSDOS720KSize = 720;
		const short kMSDOS1440KSize = 1440;

		// bLength array sizes
		const int blockArrayLenLow = 40;
		const int blockArrayLenHigh = 72;

		const int sectorsPerBlock = 40;
		const int sectorSize = 512;
		const int tagSectorSize = 12;
		const int dataSize = sectorsPerBlock * sectorSize;
		const int tagSize = sectorsPerBlock * tagSectorSize;
		const int bufferSize = (sectorsPerBlock * sectorSize) + (sectorsPerBlock * tagSectorSize);
		#endregion

		#region Internal Structures
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct DART_Header
		{
			public byte srcCmp;
			public byte srcType;
			public short srcSize;
		}
		#endregion

		// DART images are at most 1474560 bytes, so let's cache the whole
		byte[] dataCache;
		byte[] tagCache;
		uint dataChecksum;
		uint tagChecksum;

		public DART()
		{
			Name = "Apple Disk Archival/Retrieval Tool";
			PluginUUID = new Guid("B3E06BF8-F98D-4F9B-BBE2-342C373BAF3E");
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
			Stream stream = imageFilter.GetDataForkStream();

			if(stream.Length < 84)
				return false;

			DART_Header header = new DART_Header();
			stream.Seek(0, SeekOrigin.Begin);
			byte[] header_b = new byte[Marshal.SizeOf(header)];

			stream.Read(header_b, 0, Marshal.SizeOf(header));
			header = BigEndianMarshal.ByteArrayToStructureBigEndian<DART_Header>(header_b);

			if(header.srcCmp > kNoCompress)
				return false;

			int expectedMaxSize = 84 + (header.srcSize * 2 * 524);

			switch(header.srcType)
			{
				case kMacDisk:
					if(header.srcSize != kMac400KSize && header.srcSize != kMac800KSize)
						return false;
					break;
				case kLisaDisk:
					if(header.srcSize != kLisa400KSize)
						return false;
					break;
				case kAppleIIDisk:
					if(header.srcSize != kApple800KSize)
						return false;
					break;
				case kMacHiDDisk:
					if(header.srcSize != kMac1440KSize)
						return false;
					expectedMaxSize += 64;
					break;
				case kMSDOSLowDDisk:
					if(header.srcSize != kMSDOS720KSize)
						return false;
					break;
				case kMSDOSHiDDisk:
					if(header.srcSize != kMSDOS1440KSize)
						return false;
					expectedMaxSize += 64;
					break;
				default:
					return false;
			}

			if(stream.Length > expectedMaxSize)
				return false;

			return true;
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();

			if(stream.Length < 84)
				return false;

			DART_Header header = new DART_Header();
			stream.Seek(0, SeekOrigin.Begin);
			byte[] header_b = new byte[Marshal.SizeOf(header)];

			stream.Read(header_b, 0, Marshal.SizeOf(header));
			header = BigEndianMarshal.ByteArrayToStructureBigEndian<DART_Header>(header_b);

			if(header.srcCmp > kNoCompress)
				return false;

			int expectedMaxSize = 84 + (header.srcSize * 2 * 524);

			switch(header.srcType)
			{
				case kMacDisk:
					if(header.srcSize != kMac400KSize && header.srcSize != kMac800KSize)
						return false;
					break;
				case kLisaDisk:
					if(header.srcSize != kLisa400KSize)
						return false;
					break;
				case kAppleIIDisk:
					if(header.srcSize != kAppleIIDisk)
						return false;
					break;
				case kMacHiDDisk:
					if(header.srcSize != kMac1440KSize)
						return false;
					expectedMaxSize += 64;
					break;
				case kMSDOSLowDDisk:
					if(header.srcSize != kMSDOS720KSize)
						return false;
					break;
				case kMSDOSHiDDisk:
					if(header.srcSize != kMSDOS1440KSize)
						return false;
					expectedMaxSize += 64;
					break;
				default:
					return false;
			}

			if(stream.Length > expectedMaxSize)
				return false;

			short[] bLength;

			if(header.srcType == kMacHiDDisk || header.srcType == kMSDOSHiDDisk)
				bLength = new short[blockArrayLenHigh];
			else
				bLength = new short[blockArrayLenLow];

			byte[] tmpShort;
			for(int i = 0; i < bLength.Length; i++)
			{
				tmpShort = new byte[2];
				stream.Read(tmpShort, 0, 2);
				bLength[i] = BigEndianBitConverter.ToInt16(tmpShort, 0);
			}

			byte[] temp;
			byte[] buffer;

			MemoryStream dataMs = new MemoryStream();
			MemoryStream tagMs = new MemoryStream();

			for(int i = 0; i < bLength.Length; i++)
			{
				if(bLength[i] != 0)
				{
					buffer = new byte[bufferSize];
					if(bLength[i] == -1)
					{
						stream.Read(buffer, 0, bufferSize);
						dataMs.Write(buffer, 0, dataSize);
						tagMs.Write(buffer, dataSize, tagSize);
					}
					else if(header.srcCmp == kRLECompress)
					{
						temp = new byte[bLength[i] * 2];
						stream.Read(temp, 0, temp.Length);
						throw new ImageNotSupportedException("Compressed images not yet supported");
					}
					else
					{
						temp = new byte[bLength[i]];
						stream.Read(temp, 0, temp.Length);
						throw new ImageNotSupportedException("Compressed images not yet supported");
					}
				}
			}

			dataCache = dataMs.ToArray();
			if(header.srcType == kLisaDisk || header.srcType == kMacDisk || header.srcType == kAppleIIDisk)
			{
				ImageInfo.readableSectorTags.Add(SectorTagType.AppleSectorTag);
				tagCache = tagMs.ToArray();
			}

			try
			{
				if(imageFilter.HasResourceFork())
				{
					ResourceFork rsrcFork = new ResourceFork(imageFilter.GetResourceForkStream());
					// "vers"
					if(rsrcFork.ContainsKey(0x76657273))
					{
						Resource versRsrc = rsrcFork.GetResource(0x76657273);
						if(versRsrc != null)
						{
							byte[] vers = versRsrc.GetResource(versRsrc.GetIds()[0]);

							if(vers != null)
							{
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
							}
						}
					}

					// "dart"
					if(rsrcFork.ContainsKey(0x44415254))
					{
						Resource dartRsrc = rsrcFork.GetResource(0x44415254);
						if(dartRsrc != null)
						{
							string dArt = StringHandlers.PascalToString(dartRsrc.GetResource(dartRsrc.GetIds()[0]), Encoding.GetEncoding("macintosh"));
							string dArtRegEx = "(?<version>\\S+), tag checksum=\\$(?<tagchk>[0123456789ABCDEF]{8}), data checksum=\\$(?<datachk>[0123456789ABCDEF]{8})$";
							Regex dArtEx = new Regex(dArtRegEx);
							Match dArtMatch = dArtEx.Match(dArt);

							if(dArtMatch.Success)
							{
								ImageInfo.imageApplication = "DART";
								ImageInfo.imageApplicationVersion = dArtMatch.Groups["version"].Value;
								dataChecksum = Convert.ToUInt32(dArtMatch.Groups["datachk"].Value, 16);
								tagChecksum = Convert.ToUInt32(dArtMatch.Groups["tagchk"].Value, 16);
							}
						}
					}

					// "cksm"
					if(rsrcFork.ContainsKey(0x434B534D))
					{
						Resource cksmRsrc = rsrcFork.GetResource(0x434B534D);
						if(cksmRsrc != null)
						{
							if(cksmRsrc.ContainsId(1))
							{
								byte[] tagChk = cksmRsrc.GetResource(1);
								tagChecksum = BigEndianBitConverter.ToUInt32(tagChk, 0);
							}
							if(cksmRsrc.ContainsId(2))
							{
								byte[] dataChk = cksmRsrc.GetResource(1);
								dataChecksum = BigEndianBitConverter.ToUInt32(dataChk, 0);
							}
						}
					}
				}
			}
			catch(InvalidCastException) { }
			DicConsole.DebugWriteLine("DART plugin", "Image application = {0} version {1}", ImageInfo.imageApplication, ImageInfo.imageApplicationVersion);

			ImageInfo.sectors = (ulong)(header.srcSize * 2);
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
			ImageInfo.imageName = Path.GetFileNameWithoutExtension(imageFilter.GetFilename());
			ImageInfo.sectorSize = sectorSize;
			ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
			ImageInfo.imageSize = ImageInfo.sectors * sectorSize;
			if(header.srcCmp == kNoCompress)
				ImageInfo.imageVersion = "1.4";
			else
				ImageInfo.imageVersion = "1.5";

			switch(header.srcSize)
			{
				case kMac400KSize:
					ImageInfo.cylinders = 80;
					ImageInfo.heads = 1;
					ImageInfo.sectorsPerTrack = 10;
					ImageInfo.mediaType = MediaType.AppleSonySS;
					break;
				case kMac800KSize:
					ImageInfo.cylinders = 80;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 10;
					ImageInfo.mediaType = MediaType.AppleSonyDS;
					break;
				case kMSDOS720KSize:
					ImageInfo.cylinders = 80;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 9;
					ImageInfo.mediaType = MediaType.DOS_35_DS_DD_9;
					break;
				case kMac1440KSize:
					ImageInfo.cylinders = 80;
					ImageInfo.heads = 2;
					ImageInfo.sectorsPerTrack = 18;
					ImageInfo.mediaType = MediaType.DOS_35_HD;
					break;
			}


			return true;
		}

		public override byte[] ReadSector(ulong sectorAddress)
		{
			return ReadSectors(sectorAddress, 1);
		}

		public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
		{
			return ReadSectorsTag(sectorAddress, 1, tag);
		}

		public override byte[] ReadSectors(ulong sectorAddress, uint length)
		{
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

			if(sectorAddress + length > ImageInfo.sectors)
				throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

			byte[] buffer = new byte[length * ImageInfo.sectorSize];

			Array.Copy(dataCache, (int)sectorAddress * ImageInfo.sectorSize, buffer, 0, length * ImageInfo.sectorSize);

			return buffer;
		}

		public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
		{
			if(tag != SectorTagType.AppleSectorTag)
				throw new FeatureUnsupportedImageException(string.Format("Tag {0} not supported by image format", tag));

			if(tagCache == null || tagCache.Length == 0)
				throw new FeatureNotPresentImageException("Disk image does not have tags");

			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

			if(sectorAddress + length > ImageInfo.sectors)
				throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

			byte[] buffer = new byte[length * tagSectorSize];

			Array.Copy(tagCache, (int)sectorAddress * tagSectorSize, buffer, 0, length * tagSectorSize);

			return buffer;
		}

		public override byte[] ReadSectorLong(ulong sectorAddress)
		{
			return ReadSectorsLong(sectorAddress, 1);
		}

		public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
		{
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");

			if(sectorAddress + length > ImageInfo.sectors)
				throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than available");

			byte[] data = ReadSectors(sectorAddress, length);
			byte[] tags = ReadSectorsTag(sectorAddress, length, SectorTagType.AppleSectorTag);
			byte[] buffer = new byte[data.Length + tags.Length];

			for(uint i = 0; i < length; i++)
			{
				Array.Copy(data, i * (ImageInfo.sectorSize), buffer, i * (ImageInfo.sectorSize + tagSectorSize), ImageInfo.sectorSize);
				Array.Copy(tags, i * (tagSectorSize), buffer, i * (ImageInfo.sectorSize + tagSectorSize) + ImageInfo.sectorSize, tagSectorSize);
			}

			return buffer;
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
			return "Apple Disk Archival/Retrieval Tool";
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

		public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
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