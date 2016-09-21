// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CHD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages MAME Compressed Hunks of Data disk images.
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
using System.Text.RegularExpressions;
using Claunia.RsrcFork;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using System.Linq;
using System.Text;
using SharpCompress.Compressor.Deflate;
using SharpCompress.Compressor;
using System.Reflection;
using System.Reflection.Emit;

namespace DiscImageChef.ImagePlugins
{
	// TODO: Implement PCMCIA support
	class CHD : ImagePlugin
	{
		#region Internal Structures

		enum CHDCompression : uint
		{
			None = 0,
			Zlib = 1,
			ZlibPlus = 2,
			AV = 3
		}

		enum CHDFlags : uint
		{
			HasParent = 1,
			Writable = 2
		}

		enum CHDV3EntryFlags : byte
		{
			/// <summary>Invalid</summary>
			Invalid = 0,
			/// <summary>Compressed with primary codec</summary>
			Compressed = 1,
			/// <summary>Uncompressed</summary>
			Uncompressed = 2,
			/// <summary>Use offset as data</summary>
			Mini = 3,
			/// <summary>Same as another hunk in file</summary>
			SelfHunk = 4,
			/// <summary>Same as another hunk in parent</summary>
			ParentHunk = 5,
			/// <summary>Compressed with secondary codec (FLAC)</summary>
			SecondCompressed = 6
		}

		enum CHDOldTrackType : uint
		{
			Mode1 = 0,
			Mode1_Raw,
			Mode2,
			Mode2Form1,
			Mode2Form2,
			Mode2FormMix,
			Mode2Raw,
			Audio
		}

		enum CHDOldSubType : uint
		{
			Cooked = 0,
			Raw,
			None
		}

		// Hunks are represented in a 64 bit integer with 44 bit as offset, 20 bits as length
		// Sectors are fixed at 512 bytes/sector
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CHDHeaderV1
		{
			/// <summary>
			/// Magic identifier, 'MComprHD'
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] tag;
			/// <summary>
			/// Length of header
			/// </summary>
			public uint length;
			/// <summary>
			/// Image format version
			/// </summary>
			public uint version;
			/// <summary>
			/// Image flags, <see cref="CHDFlags"/>
			/// </summary>
			public uint flags;
			/// <summary>
			/// Compression algorithm, <see cref="CHDCompression"/> 
			/// </summary>
			public uint compression;
			/// <summary>
			/// Sectors per hunk
			/// </summary>
			public uint hunksize;
			/// <summary>
			/// Total # of hunk in image
			/// </summary>
			public uint totalhunks;
			/// <summary>
			/// Cylinders on disk
			/// </summary>
			public uint cylinders;
			/// <summary>
			/// Heads per cylinder
			/// </summary>
			public uint heads;
			/// <summary>
			/// Sectors per track
			/// </summary>
			public uint sectors;
			/// <summary>
			/// MD5 of raw data
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] md5;
			/// <summary>
			/// MD5 of parent file
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] parentmd5;
		}

		// Hunks are represented in a 64 bit integer with 44 bit as offset, 20 bits as length
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CHDHeaderV2
		{
			/// <summary>
			/// Magic identifier, 'MComprHD'
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] tag;
			/// <summary>
			/// Length of header
			/// </summary>
			public uint length;
			/// <summary>
			/// Image format version
			/// </summary>
			public uint version;
			/// <summary>
			/// Image flags, <see cref="CHDFlags"/>
			/// </summary>
			public uint flags;
			/// <summary>
			/// Compression algorithm, <see cref="CHDCompression"/> 
			/// </summary>
			public uint compression;
			/// <summary>
			/// Sectors per hunk
			/// </summary>
			public uint hunksize;
			/// <summary>
			/// Total # of hunk in image
			/// </summary>
			public uint totalhunks;
			/// <summary>
			/// Cylinders on disk
			/// </summary>
			public uint cylinders;
			/// <summary>
			/// Heads per cylinder
			/// </summary>
			public uint heads;
			/// <summary>
			/// Sectors per track
			/// </summary>
			public uint sectors;
			/// <summary>
			/// MD5 of raw data
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] md5;
			/// <summary>
			/// MD5 of parent file
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] parentmd5;
			/// <summary>
			/// Bytes per sector
			/// </summary>
			public uint seclen;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CHDHeaderV3
		{
			/// <summary>
			/// Magic identifier, 'MComprHD'
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] tag;
			/// <summary>
			/// Length of header
			/// </summary>
			public uint length;
			/// <summary>
			/// Image format version
			/// </summary>
			public uint version;
			/// <summary>
			/// Image flags, <see cref="CHDFlags"/>
			/// </summary>
			public uint flags;
			/// <summary>
			/// Compression algorithm, <see cref="CHDCompression"/> 
			/// </summary>
			public uint compression;
			/// <summary>
			/// Total # of hunk in image
			/// </summary>
			public uint totalhunks;
			/// <summary>
			/// Total bytes in image
			/// </summary>
			public ulong logicalbytes;
			/// <summary>
			/// Offset to first metadata blob
			/// </summary>
			public ulong metaoffset;
			/// <summary>
			/// MD5 of raw data
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] md5;
			/// <summary>
			/// MD5 of parent file
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] parentmd5;
			/// <summary>
			/// Bytes per hunk
			/// </summary>
			public uint hunkbytes;
			/// <summary>
			/// SHA1 of raw data
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] sha1;
			/// <summary>
			/// SHA1 of parent file
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] parentsha1;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CHDMapV3Entry
		{
			/// <summary>
			/// Offset to hunk from start of image
			/// </summary>
			public ulong offset;
			/// <summary>
			/// CRC32 of uncompressed hunk
			/// </summary>
			public uint crc;
			/// <summary>
			/// Lower 16 bits of length
			/// </summary>
			public ushort lengthLsb;
			/// <summary>
			/// Upper 8 bits of length
			/// </summary>
			public byte length;
			/// <summary>
			/// Hunk flags
			/// </summary>
			public byte flags;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CHDTrackOld
		{
			public uint type;
			public uint subType;
			public uint dataSize;
			public uint subSize;
			public uint frames;
			public uint extraFrames;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CHDHeaderV4
		{
			/// <summary>
			/// Magic identifier, 'MComprHD'
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] tag;
			/// <summary>
			/// Length of header
			/// </summary>
			public uint length;
			/// <summary>
			/// Image format version
			/// </summary>
			public uint version;
			/// <summary>
			/// Image flags, <see cref="CHDFlags"/>
			/// </summary>
			public uint flags;
			/// <summary>
			/// Compression algorithm, <see cref="CHDCompression"/> 
			/// </summary>
			public uint compression;
			/// <summary>
			/// Total # of hunk in image
			/// </summary>
			public uint totalhunks;
			/// <summary>
			/// Total bytes in image
			/// </summary>
			public ulong logicalbytes;
			/// <summary>
			/// Offset to first metadata blob
			/// </summary>
			public ulong metaoffset;
			/// <summary>
			/// Bytes per hunk
			/// </summary>
			public uint hunkbytes;
			/// <summary>
			/// SHA1 of raw+meta data
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] sha1;
			/// <summary>
			/// SHA1 of parent file
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] parentsha1;
			/// <summary>
			/// SHA1 of raw data
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] rawsha1;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CHDHeaderV5
		{
			/// <summary>
			/// Magic identifier, 'MComprHD'
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public byte[] tag;
			/// <summary>
			/// Length of header
			/// </summary>
			public uint length;
			/// <summary>
			/// Image format version
			/// </summary>
			public uint version;
			/// <summary>
			/// Compressor 0
			/// </summary>
			public uint compressor0;
			/// <summary>
			/// Compressor 1
			/// </summary>
			public uint compressor1;
			/// <summary>
			/// Compressor 2
			/// </summary>
			public uint compressor2;
			/// <summary>
			/// Compressor 3
			/// </summary>
			public uint compressor3;
			/// <summary>
			/// Total bytes in image
			/// </summary>
			public ulong logicalbytes;
			/// <summary>
			/// Offset to hunk map
			/// </summary>
			public ulong mapoffset;
			/// <summary>
			/// Offset to first metadata blob
			/// </summary>
			public ulong metaoffset;
			/// <summary>
			/// Bytes per hunk
			/// </summary>
			public uint hunkbytes;
			/// <summary>
			/// Bytes per unit within hunk
			/// </summary>
			public uint unitbytes;
			/// <summary>
			/// SHA1 of raw data
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] rawsha1;
			/// <summary>
			/// SHA1 of raw+meta data
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] sha1;
			/// <summary>
			/// SHA1 of parent file
			/// </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] parentsha1;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CHDCompressedMapHeaderV5
		{
			/// <summary>
			/// Length of compressed map
			/// </summary>
			public uint length;
			/// <summary>
			/// Offset of first block (48 bits) and CRC16 of map (16 bits)
			/// </summary>
			public ulong startAndCrc;
			/// <summary>
			/// Bits used to encode compressed length on map entry
			/// </summary>
			public byte bitsUsedToEncodeCompLength;
			/// <summary>
			/// Bits used to encode self-refs
			/// </summary>
			public byte bitsUsedToEncodeSelfRefs;
			/// <summary>
			/// Bits used to encode parent unit refs
			/// </summary>
			public byte bitsUsedToEncodeParentUnits;
			public byte reserved;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CHDMapV5Entry
		{
			/// <summary>
			/// Compression (8 bits) and length (24 bits)
			/// </summary>
			public uint compAndLength;
			/// <summary>
			/// Offset (48 bits) and CRC (16 bits)
			/// </summary>
			public ulong offsetAndCrc;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct CHDMetadataHeader
		{
			public uint tag;
			public uint flagsAndLength;
			public ulong next;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct HunkSector
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
			public ulong[] hunkEntry;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct HunkSectorSmall
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
			public uint[] hunkEntry;
		}
		#endregion

		#region Internal Constants

		/// <summary>"MComprHD"</summary>
		readonly byte[] chdTag = { 0x4D, 0x43, 0x6F, 0x6D, 0x70, 0x72, 0x48, 0x44 };
		/// <summary>"GDDD"</summary>
		const uint hardDiskMetadata = 0x47444444;
		/// <summary>"IDNT"</summary>
		const uint hardDiskIdentMetadata = 0x49444E54;
		/// <summary>"KEY "</summary>
		const uint hardDiskKeyMetadata = 0x4B455920;
		/// <summary>"CIS "</summary>
		const uint pcmciaCisMetadata = 0x43495320;
		/// <summary>"CHCD"</summary>
		const uint cdromOldMetadata = 0x43484344;
		/// <summary>"CHTR"</summary>
		const uint cdromTrackMetadata = 0x43485452;
		/// <summary>"CHT2"</summary>
		const uint cdromTrackMetadata2 = 0x43485432;
		/// <summary>"CHGT"</summary>
		const uint gdromOldMetadata = 0x43484754;
		/// <summary>"CHGD"</summary>
		const uint gdromMetadata = 0x43484744;
		/// <summary>"AVAV"</summary>
		const uint avMetadata = 0x41564156;
		/// <summary>"AVLD"</summary>
		const uint avLaserDiscMetadata = 0x41564C44;

		const string hardDiskMetadataRegEx = "CYLS:(?<cylinders>\\d+),HEADS:(?<heads>\\d+),SECS:(?<sectors>\\d+),BPS:(?<bps>\\d+)";
		const string CdromMetadataRegEx = "TRACK:(?<track>\\d+) TYPE:(?<track_type>\\S+) SUBTYPE:(?<sub_type>\\S+) FRAMES:(?<frames>\\d+)";
		const string CdromMetadata2RegEx = "TRACK:(?<track>\\d+) TYPE:(?<track_type>\\S+) SUBTYPE:(?<sub_type>\\S+) FRAMES:(?<frames>\\d+) PREGAP:(?<pregap>\\d+) PGTYPE:(?<pgtype>\\S+) PGSUB:(?<pgsub>\\S+) POSTGAP:(?<postgap>\\d+)";
		const string GdromMetadataRegEx = "TRACK:(?<track>\\d+) TYPE:(?<track_type>\\S+) SUBTYPE:(?<sub_type>\\S+) FRAMES:(?<frames>\\d+) PAD:(?<pad>\\d+) PREGAP:(?<pregap>\\d+) PGTYPE:(?<pgtype>\\S+) PGSUB:(?<pgsub>\\S+) POSTGAP:(?<postgap>\\d+)";

		const string TrackTypeMode1 = "MODE1";
		const string TrackTypeMode1_2k = "MODE1/2048";
		const string TrackTypeMode1Raw = "MODE1_RAW";
		const string TrackTypeMode1Raw_2k = "MODE1/2352";
		const string TrackTypeMode2 = "MODE2";
		const string TrackTypeMode2_2k = "MODE2/2336";
		const string TrackTypeMode2F1 = "MODE2_FORM1";
		const string TrackTypeMode2F1_2k = "MODE2/2048";
		const string TrackTypeMode2F2 = "MODE2_FORM2";
		const string TrackTypeMode2F2_2k = "MODE2/2324";
		const string TrackTypeMode2FM = "MODE2_FORM_MIX";
		const string TrackTypeMode2Raw = "MODE2_RAW";
		const string TrackTypeMode2Raw_2k = "MODE2/2352";
		const string TrackTypeAudio = "AUDIO";

		const string SubTypeCooked = "RW";
		const string SubTypeRaw = "RW_RAW";
		const string SubTypeNone = "NONE";

		#endregion

		#region Internal variables

		ulong[] hunkTable;
		uint[] hunkTableSmall;
		uint hdrCompression;
		uint hdrCompression1;
		uint hdrCompression2;
		uint hdrCompression3;
		Stream imageStream;
		uint sectorsPerHunk;
		byte[] hunkMap;
		uint mapVersion;
		uint bytesPerHunk;
		uint totalHunks;
		byte[] expectedChecksum;
		bool isCdrom;
		bool isHdd;
		bool isGdrom;
		bool swapAudio;

		const int MaxCacheSize = 16777216;
		int maxBlockCache;
		int maxSectorCache;

		Dictionary<ulong, byte[]> sectorCache;
		Dictionary<ulong, byte[]> hunkCache;

		Dictionary<uint, Track> tracks;
		List<Partition> partitions;
		Dictionary<ulong, uint> offsetmap;

		byte[] identify;

		#endregion

		public CHD()
		{
			Name = "MAME Compressed Hunks of Data";
			PluginUUID = new Guid("0D50233A-08BD-47D4-988B-27EAA0358597");
			ImageInfo = new ImageInfo();
			ImageInfo.readableSectorTags = new List<SectorTagType>();
			ImageInfo.readableMediaTags = new List<MediaTagType>();
			ImageInfo.imageHasPartitions = false;
			ImageInfo.imageHasSessions = false;
			ImageInfo.imageApplication = "MAME";
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
			stream.Seek(0, SeekOrigin.Begin);
			byte[] magic = new byte[8];
			stream.Read(magic, 0, 8);

			return chdTag.SequenceEqual(magic);
		}

		public override bool OpenImage(Filter imageFilter)
		{
			Stream stream = imageFilter.GetDataForkStream();
			stream.Seek(0, SeekOrigin.Begin);
			byte[] buffer = new byte[8];
			byte[] magic = new byte[8];
			stream.Read(magic, 0, 8);
			if(!chdTag.SequenceEqual(magic))
				return false;
			// Read length
			buffer = new byte[4];
			stream.Read(buffer, 0, 4);
			uint length = BitConverter.ToUInt32(buffer.Reverse().ToArray(), 0);
			buffer = new byte[4];
			stream.Read(buffer, 0, 4);
			uint version = BitConverter.ToUInt32(buffer.Reverse().ToArray(), 0);

			buffer = new byte[length];
			stream.Seek(0, SeekOrigin.Begin);
			stream.Read(buffer, 0, (int)length);

			ulong nextMetaOff = 0;

			switch(version)
			{
				case 1:
					{
						CHDHeaderV1 hdrV1 = BigEndianMarshal.ByteArrayToStructureBigEndian<CHDHeaderV1>(buffer);

						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.tag = \"{0}\"", Encoding.ASCII.GetString(hdrV1.tag));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.length = {0} bytes", hdrV1.length);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.version = {0}", hdrV1.version);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.flags = {0}", (CHDFlags)hdrV1.flags);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.compression = {0}", (CHDCompression)hdrV1.compression);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.hunksize = {0}", hdrV1.hunksize);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.totalhunks = {0}", hdrV1.totalhunks);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.cylinders = {0}", hdrV1.cylinders);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.heads = {0}", hdrV1.heads);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.sectors = {0}", hdrV1.sectors);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.md5 = {0}", ArrayHelpers.ByteArrayToHex(hdrV1.md5));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV1.parentmd5 = {0}", ArrayHelpers.ArrayIsNullOrEmpty(hdrV1.parentmd5) ? "null" : ArrayHelpers.ByteArrayToHex(hdrV1.parentmd5));

						DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
						DateTime start = DateTime.UtcNow;

						hunkTable = new ulong[hdrV1.totalhunks];

						uint hunkSectorCount = (uint)Math.Ceiling(((double)hdrV1.totalhunks * 8) / 512);

						byte[] hunkSectorBytes = new byte[512];
						HunkSector hunkSector = new HunkSector();

						for(int i = 0; i < hunkSectorCount; i++)
						{
							stream.Read(hunkSectorBytes, 0, 512);
							// This does the big-endian trick but reverses the order of elements also
							Array.Reverse(hunkSectorBytes);
							GCHandle handle = GCHandle.Alloc(hunkSectorBytes, GCHandleType.Pinned);
							hunkSector = (HunkSector)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HunkSector));
							handle.Free();
							// This restores the order of elements
							Array.Reverse(hunkSector.hunkEntry);
							if(hunkTable.Length >= (i * 512) / 8 + 512 / 8)
								Array.Copy(hunkSector.hunkEntry, 0, hunkTable, (i * 512) / 8, 512 / 8);
							else
								Array.Copy(hunkSector.hunkEntry, 0, hunkTable, (i * 512) / 8, hunkTable.Length - (i * 512) / 8);
						}
						DateTime end = DateTime.UtcNow;
						System.Console.WriteLine("Took {0} seconds", (end - start).TotalSeconds);

						ImageInfo.mediaType = MediaType.GENERIC_HDD;
						ImageInfo.sectors = hdrV1.hunksize * hdrV1.totalhunks;
						ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
						ImageInfo.sectorSize = 512;
						ImageInfo.imageVersion = "1";
						ImageInfo.imageSize = ImageInfo.sectorSize * hdrV1.hunksize * hdrV1.totalhunks;

						totalHunks = hdrV1.totalhunks;
						sectorsPerHunk = hdrV1.hunksize;
						hdrCompression = hdrV1.compression;
						mapVersion = 1;
						isHdd = true;

						break;
					}
				case 2:
					{
						CHDHeaderV2 hdrV2 = BigEndianMarshal.ByteArrayToStructureBigEndian<CHDHeaderV2>(buffer);

						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.tag = \"{0}\"", Encoding.ASCII.GetString(hdrV2.tag));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.length = {0} bytes", hdrV2.length);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.version = {0}", hdrV2.version);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.flags = {0}", (CHDFlags)hdrV2.flags);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.compression = {0}", (CHDCompression)hdrV2.compression);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.hunksize = {0}", hdrV2.hunksize);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.totalhunks = {0}", hdrV2.totalhunks);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.cylinders = {0}", hdrV2.cylinders);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.heads = {0}", hdrV2.heads);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.sectors = {0}", hdrV2.sectors);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.md5 = {0}", ArrayHelpers.ByteArrayToHex(hdrV2.md5));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.parentmd5 = {0}", ArrayHelpers.ArrayIsNullOrEmpty(hdrV2.parentmd5) ? "null" : ArrayHelpers.ByteArrayToHex(hdrV2.parentmd5));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV2.seclen = {0}", hdrV2.seclen);

						DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
						DateTime start = DateTime.UtcNow;

						hunkTable = new ulong[hdrV2.totalhunks];

						// How many sectors uses the BAT
						uint hunkSectorCount = (uint)Math.Ceiling(((double)hdrV2.totalhunks * 8) / 512);

						byte[] hunkSectorBytes = new byte[512];
						HunkSector hunkSector = new HunkSector();

						for(int i = 0; i < hunkSectorCount; i++)
						{
							stream.Read(hunkSectorBytes, 0, 512);
							// This does the big-endian trick but reverses the order of elements also
							Array.Reverse(hunkSectorBytes);
							GCHandle handle = GCHandle.Alloc(hunkSectorBytes, GCHandleType.Pinned);
							hunkSector = (HunkSector)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HunkSector));
							handle.Free();
							// This restores the order of elements
							Array.Reverse(hunkSector.hunkEntry);
							if(hunkTable.Length >= (i * 512) / 8 + 512 / 8)
								Array.Copy(hunkSector.hunkEntry, 0, hunkTable, (i * 512) / 8, 512 / 8);
							else
								Array.Copy(hunkSector.hunkEntry, 0, hunkTable, (i * 512) / 8, hunkTable.Length - (i * 512) / 8);
						}
						DateTime end = DateTime.UtcNow;
						System.Console.WriteLine("Took {0} seconds", (end - start).TotalSeconds);

						ImageInfo.mediaType = MediaType.GENERIC_HDD;
						ImageInfo.sectors = hdrV2.hunksize * hdrV2.totalhunks;
						ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
						ImageInfo.sectorSize = hdrV2.seclen;
						ImageInfo.imageVersion = "2";
						ImageInfo.imageSize = ImageInfo.sectorSize * hdrV2.hunksize * hdrV2.totalhunks;

						totalHunks = hdrV2.totalhunks;
						sectorsPerHunk = hdrV2.hunksize;
						hdrCompression = hdrV2.compression;
						mapVersion = 1;
						isHdd = true;

						break;
					}
				case 3:
					{
						CHDHeaderV3 hdrV3 = BigEndianMarshal.ByteArrayToStructureBigEndian<CHDHeaderV3>(buffer);

						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.tag = \"{0}\"", Encoding.ASCII.GetString(hdrV3.tag));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.length = {0} bytes", hdrV3.length);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.version = {0}", hdrV3.version);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.flags = {0}", (CHDFlags)hdrV3.flags);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.compression = {0}", (CHDCompression)hdrV3.compression);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.totalhunks = {0}", hdrV3.totalhunks);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.logicalbytes = {0}", hdrV3.logicalbytes);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.metaoffset = {0}", hdrV3.metaoffset);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.md5 = {0}", ArrayHelpers.ByteArrayToHex(hdrV3.md5));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.parentmd5 = {0}", ArrayHelpers.ArrayIsNullOrEmpty(hdrV3.parentmd5) ? "null" : ArrayHelpers.ByteArrayToHex(hdrV3.parentmd5));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.hunkbytes = {0}", hdrV3.hunkbytes);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.sha1 = {0}", ArrayHelpers.ByteArrayToHex(hdrV3.sha1));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV3.parentsha1 = {0}", ArrayHelpers.ArrayIsNullOrEmpty(hdrV3.parentsha1) ? "null" : ArrayHelpers.ByteArrayToHex(hdrV3.parentsha1));

						DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
						DateTime start = DateTime.UtcNow;

						hunkMap = new byte[hdrV3.totalhunks * 16];
						stream.Read(hunkMap, 0, hunkMap.Length);

						DateTime end = DateTime.UtcNow;
						System.Console.WriteLine("Took {0} seconds", (end - start).TotalSeconds);

						nextMetaOff = hdrV3.metaoffset;

						ImageInfo.imageSize = hdrV3.logicalbytes;
						ImageInfo.imageVersion = "3";

						totalHunks = hdrV3.totalhunks;
						bytesPerHunk = hdrV3.hunkbytes;
						hdrCompression = hdrV3.compression;
						mapVersion = 3;

						break;
					}
				case 4:
					{
						CHDHeaderV4 hdrV4 = BigEndianMarshal.ByteArrayToStructureBigEndian<CHDHeaderV4>(buffer);

						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.tag = \"{0}\"", Encoding.ASCII.GetString(hdrV4.tag));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.length = {0} bytes", hdrV4.length);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.version = {0}", hdrV4.version);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.flags = {0}", (CHDFlags)hdrV4.flags);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.compression = {0}", (CHDCompression)hdrV4.compression);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.totalhunks = {0}", hdrV4.totalhunks);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.logicalbytes = {0}", hdrV4.logicalbytes);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.metaoffset = {0}", hdrV4.metaoffset);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.hunkbytes = {0}", hdrV4.hunkbytes);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.sha1 = {0}", ArrayHelpers.ByteArrayToHex(hdrV4.sha1));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.parentsha1 = {0}", ArrayHelpers.ArrayIsNullOrEmpty(hdrV4.parentsha1) ? "null" : ArrayHelpers.ByteArrayToHex(hdrV4.parentsha1));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV4.rawsha1 = {0}", ArrayHelpers.ByteArrayToHex(hdrV4.rawsha1));

						DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
						DateTime start = DateTime.UtcNow;

						hunkMap = new byte[hdrV4.totalhunks * 16];
						stream.Read(hunkMap, 0, hunkMap.Length);

						DateTime end = DateTime.UtcNow;
						System.Console.WriteLine("Took {0} seconds", (end - start).TotalSeconds);

						nextMetaOff = hdrV4.metaoffset;

						ImageInfo.imageSize = hdrV4.logicalbytes;
						ImageInfo.imageVersion = "4";

						totalHunks = hdrV4.totalhunks;
						bytesPerHunk = hdrV4.hunkbytes;
						hdrCompression = hdrV4.compression;
						mapVersion = 3;

						break;
					}
				case 5:
					{
						CHDHeaderV5 hdrV5 = BigEndianMarshal.ByteArrayToStructureBigEndian<CHDHeaderV5>(buffer);

						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.tag = \"{0}\"", Encoding.ASCII.GetString(hdrV5.tag));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.length = {0} bytes", hdrV5.length);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.version = {0}", hdrV5.version);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor0 = \"{0}\"", Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(hdrV5.compressor0)));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor1 = \"{0}\"", Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(hdrV5.compressor1)));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor2 = \"{0}\"", Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(hdrV5.compressor2)));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.compressor3 = \"{0}\"", Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(hdrV5.compressor3)));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.logicalbytes = {0}", hdrV5.logicalbytes);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.mapoffset = {0}", hdrV5.mapoffset);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.metaoffset = {0}", hdrV5.metaoffset);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.hunkbytes = {0}", hdrV5.hunkbytes);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.unitbytes = {0}", hdrV5.unitbytes);
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.sha1 = {0}", ArrayHelpers.ByteArrayToHex(hdrV5.sha1));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.parentsha1 = {0}", ArrayHelpers.ArrayIsNullOrEmpty(hdrV5.parentsha1) ? "null" : ArrayHelpers.ByteArrayToHex(hdrV5.parentsha1));
						DicConsole.DebugWriteLine("CHD plugin", "hdrV5.rawsha1 = {0}", ArrayHelpers.ByteArrayToHex(hdrV5.rawsha1));

						// TODO: Implement compressed CHD v5
						if(hdrV5.compressor0 == 0)
						{
							DicConsole.DebugWriteLine("CHD plugin", "Reading Hunk map.");
							DateTime start = DateTime.UtcNow;

							hunkTableSmall = new uint[hdrV5.logicalbytes / hdrV5.hunkbytes];

							uint hunkSectorCount = (uint)Math.Ceiling(((double)hunkTableSmall.Length * 4) / 512);

							byte[] hunkSectorBytes = new byte[512];
							HunkSectorSmall hunkSector = new HunkSectorSmall();

							stream.Seek((long)hdrV5.mapoffset, SeekOrigin.Begin);

							for(int i = 0; i < hunkSectorCount; i++)
							{
								stream.Read(hunkSectorBytes, 0, 512);
								// This does the big-endian trick but reverses the order of elements also
								Array.Reverse(hunkSectorBytes);
								GCHandle handle = GCHandle.Alloc(hunkSectorBytes, GCHandleType.Pinned);
								hunkSector = (HunkSectorSmall)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(HunkSectorSmall));
								handle.Free();
								// This restores the order of elements
								Array.Reverse(hunkSector.hunkEntry);
								if(hunkTableSmall.Length >= (i * 512) / 4 + 512 / 4)
									Array.Copy(hunkSector.hunkEntry, 0, hunkTableSmall, (i * 512) / 4, 512 / 4);
								else
									Array.Copy(hunkSector.hunkEntry, 0, hunkTableSmall, (i * 512) / 4, hunkTableSmall.Length - (i * 512) / 4);
							}
							DateTime end = DateTime.UtcNow;
							System.Console.WriteLine("Took {0} seconds", (end - start).TotalSeconds);
						}
						else
							throw new ImageNotSupportedException("Cannot read compressed CHD version 5");

						nextMetaOff = hdrV5.metaoffset;

						ImageInfo.imageSize = hdrV5.logicalbytes;
						ImageInfo.imageVersion = "5";

						totalHunks = (uint)(hdrV5.logicalbytes / hdrV5.hunkbytes);
						bytesPerHunk = hdrV5.hunkbytes;
						hdrCompression = hdrV5.compressor0;
						hdrCompression1 = hdrV5.compressor1;
						hdrCompression2 = hdrV5.compressor2;
						hdrCompression3 = hdrV5.compressor3;
						mapVersion = 5;

						break;
					}
				default:
					throw new ImageNotSupportedException(string.Format("Unsupported CHD version {0}", version));
			}

			if(mapVersion >= 3)
			{
				byte[] meta;
				isCdrom = false;
				isHdd = false;
				isGdrom = false;
				swapAudio = false;
				tracks = new Dictionary<uint, Track>();

				DicConsole.DebugWriteLine("CHD plugin", "Reading metadata.");

				ulong currentSector = 0;
				uint currentTrack = 1;

				while(nextMetaOff > 0)
				{
					byte[] hdrBytes = new byte[16];
					stream.Seek((long)nextMetaOff, SeekOrigin.Begin);
					stream.Read(hdrBytes, 0, hdrBytes.Length);
					CHDMetadataHeader header = BigEndianMarshal.ByteArrayToStructureBigEndian<CHDMetadataHeader>(hdrBytes);
					meta = new byte[header.flagsAndLength & 0xFFFFFF];
					stream.Read(meta, 0, meta.Length);
					DicConsole.DebugWriteLine("CHD plugin", "Found metadata \"{0}\"", Encoding.ASCII.GetString(BigEndianBitConverter.GetBytes(header.tag)));

					switch(header.tag)
					{
						// "GDDD"
						case hardDiskMetadata:
							if(isCdrom || isGdrom)
								throw new ImageNotSupportedException("Image cannot be a hard disk and a C/GD-ROM at the same time, aborting.");
							
							string gddd = StringHandlers.CToString(meta);
							Regex gdddRegEx = new Regex(hardDiskMetadataRegEx);
							Match gdddMatch = gdddRegEx.Match(gddd);
							if(gdddMatch.Success)
							{
								isHdd = true;
								ImageInfo.sectorSize = uint.Parse(gdddMatch.Groups["bps"].Value);
							}
							break;
						// "CHCD"
						case cdromOldMetadata:
							if(isHdd)
								throw new ImageNotSupportedException("Image cannot be a hard disk and a CD-ROM at the same time, aborting.");

							if(isGdrom)
								throw new ImageNotSupportedException("Image cannot be a GD-ROM and a CD-ROM at the same time, aborting.");

							uint _tracks = BigEndianBitConverter.ToUInt32(meta, 0);

							// Byteswapped
							if(_tracks > 99)
							{
								BigEndianBitConverter.IsLittleEndian = !BitConverter.IsLittleEndian;
								_tracks = BigEndianBitConverter.ToUInt32(meta, 0);
							}

							currentSector = 0;

							for(uint i = 0; i < _tracks; i++)
							{
								CHDTrackOld _trk = new CHDTrackOld();
								_trk.type = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 0));
								_trk.subType = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 4));
								_trk.dataSize = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 8));
								_trk.subSize = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 12));
								_trk.frames = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 16));
								_trk.extraFrames = BigEndianBitConverter.ToUInt32(meta, (int)(4 + i * 24 + 20));

								Track _track = new Track();
								switch((CHDOldTrackType)_trk.type)
								{
									case CHDOldTrackType.Audio:
										_track.TrackBytesPerSector = 2352;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.Audio;
										break;
									case CHDOldTrackType.Mode1:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2048;
										_track.TrackType = TrackType.CDMode1;
										break;
									case CHDOldTrackType.Mode1_Raw:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.CDMode1;
										break;
									case CHDOldTrackType.Mode2:
									case CHDOldTrackType.Mode2FormMix:
										_track.TrackBytesPerSector = 2336;
										_track.TrackRawBytesPerSector = 2336;
										_track.TrackType = TrackType.CDMode2Formless;
										break;
									case CHDOldTrackType.Mode2Form1:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2048;
										_track.TrackType = TrackType.CDMode2Form1;
										break;
									case CHDOldTrackType.Mode2Form2:
										_track.TrackBytesPerSector = 2324;
										_track.TrackRawBytesPerSector = 2324;
										_track.TrackType = TrackType.CDMode2Form2;
										break;
									case CHDOldTrackType.Mode2Raw:
										_track.TrackBytesPerSector = 2336;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.CDMode2Formless;
										break;
									default:
										throw new ImageNotSupportedException(string.Format("Unsupported track type {0}", _trk.type));
								}

								switch((CHDOldSubType)_trk.subType)
								{
									case CHDOldSubType.Cooked:
										_track.TrackSubchannelFile = imageFilter.GetFilename();
										_track.TrackSubchannelType = TrackSubchannelType.PackedInterleaved;
										_track.TrackSubchannelFilter = imageFilter;
										break;
									case CHDOldSubType.None:
										_track.TrackSubchannelType = TrackSubchannelType.None;
										break;
									case CHDOldSubType.Raw:
										_track.TrackSubchannelFile = imageFilter.GetFilename();
										_track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
										_track.TrackSubchannelFilter = imageFilter;
										break;
									default:
										throw new ImageNotSupportedException(string.Format("Unsupported subchannel type {0}", _trk.type));
								}

								_track.Indexes = new Dictionary<int, ulong>();
								_track.TrackDescription = string.Format("Track {0}", i + 1);
								_track.TrackEndSector = currentSector + _trk.frames - 1;
								_track.TrackFile = imageFilter.GetFilename();
								_track.TrackFileType = "BINARY";
								_track.TrackFilter = imageFilter;
								_track.TrackStartSector = currentSector;
								_track.TrackSequence = i + 1;
								_track.TrackSession = 1;
								currentSector += _trk.frames + _trk.extraFrames;
								tracks.Add(_track.TrackSequence, _track);
							}

							BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
							isCdrom = true;

							break;
						// "CHTR"
						case cdromTrackMetadata:
							if(isHdd)
								throw new ImageNotSupportedException("Image cannot be a hard disk and a CD-ROM at the same time, aborting.");

							if(isGdrom)
								throw new ImageNotSupportedException("Image cannot be a GD-ROM and a CD-ROM at the same time, aborting.");

							string chtr = StringHandlers.CToString(meta);
							Regex chtrRegEx = new Regex(CdromMetadataRegEx);
							Match chtrMatch = chtrRegEx.Match(chtr);
							if(chtrMatch.Success)
							{
								isCdrom = true;

								uint trackNo = uint.Parse(chtrMatch.Groups["track"].Value);
								uint frames = uint.Parse(chtrMatch.Groups["frames"].Value);
								string subtype = chtrMatch.Groups["sub_type"].Value;
								string tracktype = chtrMatch.Groups["track_type"].Value;

								if(trackNo != currentTrack)
									throw new ImageNotSupportedException("Unsorted tracks, cannot proceed.");

								Track _track = new Track();
								switch(tracktype)
								{
									case TrackTypeAudio:
										_track.TrackBytesPerSector = 2352;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.Audio;
										break;
									case TrackTypeMode1:
									case TrackTypeMode1_2k:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2048;
										_track.TrackType = TrackType.CDMode1;
										break;
									case TrackTypeMode1Raw:
									case TrackTypeMode1Raw_2k:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.CDMode1;
										break;
									case TrackTypeMode2:
									case TrackTypeMode2_2k:
									case TrackTypeMode2FM:
										_track.TrackBytesPerSector = 2336;
										_track.TrackRawBytesPerSector = 2336;
										_track.TrackType = TrackType.CDMode2Formless;
										break;
									case TrackTypeMode2F1:
									case TrackTypeMode2F1_2k:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2048;
										_track.TrackType = TrackType.CDMode2Form1;
										break;
									case TrackTypeMode2F2:
									case TrackTypeMode2F2_2k:
										_track.TrackBytesPerSector = 2324;
										_track.TrackRawBytesPerSector = 2324;
										_track.TrackType = TrackType.CDMode2Form2;
										break;
									case TrackTypeMode2Raw:
									case TrackTypeMode2Raw_2k:
										_track.TrackBytesPerSector = 2336;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.CDMode2Formless;
										break;
									default:
										throw new ImageNotSupportedException(string.Format("Unsupported track type {0}", tracktype));
								}

								switch(subtype)
								{
									case SubTypeCooked:
										_track.TrackSubchannelFile = imageFilter.GetFilename();
										_track.TrackSubchannelType = TrackSubchannelType.PackedInterleaved;
										_track.TrackSubchannelFilter = imageFilter;
										break;
									case SubTypeNone:
										_track.TrackSubchannelType = TrackSubchannelType.None;
										break;
									case SubTypeRaw:
										_track.TrackSubchannelFile = imageFilter.GetFilename();
										_track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
										_track.TrackSubchannelFilter = imageFilter;
										break;
									default:
										throw new ImageNotSupportedException(string.Format("Unsupported subchannel type {0}", subtype));
								}

								_track.Indexes = new Dictionary<int, ulong>();
								_track.TrackDescription = string.Format("Track {0}", trackNo);
								_track.TrackEndSector = currentSector + frames - 1;
								_track.TrackFile = imageFilter.GetFilename();
								_track.TrackFileType = "BINARY";
								_track.TrackFilter = imageFilter;
								_track.TrackStartSector = currentSector;
								_track.TrackSequence = trackNo;
								_track.TrackSession = 1;
								currentSector += frames;
								currentTrack++;
								tracks.Add(_track.TrackSequence, _track);
							}
							break;
						// "CHT2"
						case cdromTrackMetadata2:
							if(isHdd)
								throw new ImageNotSupportedException("Image cannot be a hard disk and a CD-ROM at the same time, aborting.");

							if(isGdrom)
								throw new ImageNotSupportedException("Image cannot be a GD-ROM and a CD-ROM at the same time, aborting.");

							string cht2 = StringHandlers.CToString(meta);
							Regex cht2RegEx = new Regex(CdromMetadata2RegEx);
							Match cht2Match = cht2RegEx.Match(cht2);
							if(cht2Match.Success)
							{
								isCdrom = true;

								uint trackNo = uint.Parse(cht2Match.Groups["track"].Value);
								uint frames = uint.Parse(cht2Match.Groups["frames"].Value);
								string subtype = cht2Match.Groups["sub_type"].Value;
								string tracktype = cht2Match.Groups["track_type"].Value;
								// TODO: Check pregap and postgap behaviour
								uint pregap = uint.Parse(cht2Match.Groups["pregap"].Value);
								string pregapType = cht2Match.Groups["pgtype"].Value;
								string pregapSubType = cht2Match.Groups["pgsub"].Value;
								uint postgap = uint.Parse(cht2Match.Groups["postgap"].Value);

								if(trackNo != currentTrack)
									throw new ImageNotSupportedException("Unsorted tracks, cannot proceed.");

								Track _track = new Track();
								switch(tracktype)
								{
									case TrackTypeAudio:
										_track.TrackBytesPerSector = 2352;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.Audio;
										break;
									case TrackTypeMode1:
									case TrackTypeMode1_2k:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2048;
										_track.TrackType = TrackType.CDMode1;
										break;
									case TrackTypeMode1Raw:
									case TrackTypeMode1Raw_2k:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.CDMode1;
										break;
									case TrackTypeMode2:
									case TrackTypeMode2_2k:
									case TrackTypeMode2FM:
										_track.TrackBytesPerSector = 2336;
										_track.TrackRawBytesPerSector = 2336;
										_track.TrackType = TrackType.CDMode2Formless;
										break;
									case TrackTypeMode2F1:
									case TrackTypeMode2F1_2k:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2048;
										_track.TrackType = TrackType.CDMode2Form1;
										break;
									case TrackTypeMode2F2:
									case TrackTypeMode2F2_2k:
										_track.TrackBytesPerSector = 2324;
										_track.TrackRawBytesPerSector = 2324;
										_track.TrackType = TrackType.CDMode2Form2;
										break;
									case TrackTypeMode2Raw:
									case TrackTypeMode2Raw_2k:
										_track.TrackBytesPerSector = 2336;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.CDMode2Formless;
										break;
									default:
										throw new ImageNotSupportedException(string.Format("Unsupported track type {0}", tracktype));
								}

								switch(subtype)
								{
									case SubTypeCooked:
										_track.TrackSubchannelFile = imageFilter.GetFilename();
										_track.TrackSubchannelType = TrackSubchannelType.PackedInterleaved;
										_track.TrackSubchannelFilter = imageFilter;
										break;
									case SubTypeNone:
										_track.TrackSubchannelType = TrackSubchannelType.None;
										break;
									case SubTypeRaw:
										_track.TrackSubchannelFile = imageFilter.GetFilename();
										_track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
										_track.TrackSubchannelFilter = imageFilter;
										break;
									default:
										throw new ImageNotSupportedException(string.Format("Unsupported subchannel type {0}", subtype));
								}

								_track.Indexes = new Dictionary<int, ulong>();
								_track.TrackDescription = string.Format("Track {0}", trackNo);
								_track.TrackEndSector = currentSector + frames - 1;
								_track.TrackFile = imageFilter.GetFilename();
								_track.TrackFileType = "BINARY";
								_track.TrackFilter = imageFilter;
								_track.TrackStartSector = currentSector;
								_track.TrackSequence = trackNo;
								_track.TrackSession = 1;
								currentSector += frames;
								currentTrack++;
								tracks.Add(_track.TrackSequence, _track);
							}
							break;
						// "CHGT"
						case gdromOldMetadata:
							swapAudio = true;
							goto case gdromMetadata;
						// "CHGD"
						case gdromMetadata:
							if(isHdd)
								throw new ImageNotSupportedException("Image cannot be a hard disk and a GD-ROM at the same time, aborting.");

							if(isCdrom)
								throw new ImageNotSupportedException("Image cannot be a CD-ROM and a GD-ROM at the same time, aborting.");

							string chgd = StringHandlers.CToString(meta);
							Regex chgdRegEx = new Regex(GdromMetadataRegEx);
							Match chgdMatch = chgdRegEx.Match(chgd);
							if(chgdMatch.Success)
							{
								isGdrom = true;

								uint trackNo = uint.Parse(chgdMatch.Groups["track"].Value);
								uint frames = uint.Parse(chgdMatch.Groups["frames"].Value);
								string subtype = chgdMatch.Groups["sub_type"].Value;
								string tracktype = chgdMatch.Groups["track_type"].Value;
								// TODO: Check pregap, postgap and pad behaviour
								uint pregap = uint.Parse(chgdMatch.Groups["pregap"].Value);
								string pregapType = chgdMatch.Groups["pgtype"].Value;
								string pregapSubType = chgdMatch.Groups["pgsub"].Value;
								uint postgap = uint.Parse(chgdMatch.Groups["postgap"].Value);
								uint pad = uint.Parse(chgdMatch.Groups["pad"].Value);

								if(trackNo != currentTrack)
									throw new ImageNotSupportedException("Unsorted tracks, cannot proceed.");

								Track _track = new Track();
								switch(tracktype)
								{
									case TrackTypeAudio:
										_track.TrackBytesPerSector = 2352;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.Audio;
										break;
									case TrackTypeMode1:
									case TrackTypeMode1_2k:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2048;
										_track.TrackType = TrackType.CDMode1;
										break;
									case TrackTypeMode1Raw:
									case TrackTypeMode1Raw_2k:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.CDMode1;
										break;
									case TrackTypeMode2:
									case TrackTypeMode2_2k:
									case TrackTypeMode2FM:
										_track.TrackBytesPerSector = 2336;
										_track.TrackRawBytesPerSector = 2336;
										_track.TrackType = TrackType.CDMode2Formless;
										break;
									case TrackTypeMode2F1:
									case TrackTypeMode2F1_2k:
										_track.TrackBytesPerSector = 2048;
										_track.TrackRawBytesPerSector = 2048;
										_track.TrackType = TrackType.CDMode2Form1;
										break;
									case TrackTypeMode2F2:
									case TrackTypeMode2F2_2k:
										_track.TrackBytesPerSector = 2324;
										_track.TrackRawBytesPerSector = 2324;
										_track.TrackType = TrackType.CDMode2Form2;
										break;
									case TrackTypeMode2Raw:
									case TrackTypeMode2Raw_2k:
										_track.TrackBytesPerSector = 2336;
										_track.TrackRawBytesPerSector = 2352;
										_track.TrackType = TrackType.CDMode2Formless;
										break;
									default:
										throw new ImageNotSupportedException(string.Format("Unsupported track type {0}", tracktype));
								}

								switch(subtype)
								{
									case SubTypeCooked:
										_track.TrackSubchannelFile = imageFilter.GetFilename();
										_track.TrackSubchannelType = TrackSubchannelType.PackedInterleaved;
										_track.TrackSubchannelFilter = imageFilter;
										break;
									case SubTypeNone:
										_track.TrackSubchannelType = TrackSubchannelType.None;
										break;
									case SubTypeRaw:
										_track.TrackSubchannelFile = imageFilter.GetFilename();
										_track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
										_track.TrackSubchannelFilter = imageFilter;
										break;
									default:
										throw new ImageNotSupportedException(string.Format("Unsupported subchannel type {0}", subtype));
								}

								_track.Indexes = new Dictionary<int, ulong>();
								_track.TrackDescription = string.Format("Track {0}", trackNo);
								_track.TrackEndSector = currentSector + frames - 1;
								_track.TrackFile = imageFilter.GetFilename();
								_track.TrackFileType = "BINARY";
								_track.TrackFilter = imageFilter;
								_track.TrackStartSector = currentSector;
								_track.TrackSequence = trackNo;
								_track.TrackSession = (ushort)(trackNo > 2 ? 2 : 1);
								currentSector += frames;
								currentTrack++;
								tracks.Add(_track.TrackSequence, _track);
							}
							break;
						// "IDNT"
						case hardDiskIdentMetadata:
							Decoders.ATA.Identify.IdentifyDevice? idnt = Decoders.ATA.Identify.Decode(meta);
							if(idnt.HasValue)
							{
								ImageInfo.mediaManufacturer = idnt.Value.MediaManufacturer;
								ImageInfo.mediaSerialNumber = idnt.Value.MediaSerial;
								ImageInfo.driveModel = idnt.Value.Model;
								ImageInfo.driveSerialNumber = idnt.Value.SerialNumber;
								ImageInfo.driveFirmwareRevision = idnt.Value.FirmwareRevision;
							}
							identify = meta;
							if(!ImageInfo.readableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
								ImageInfo.readableMediaTags.Add(MediaTagType.ATA_IDENTIFY);
							break;
					}

					nextMetaOff = header.next;
				}

				if(isHdd)
				{
					sectorsPerHunk = bytesPerHunk / ImageInfo.sectorSize;
					ImageInfo.sectors = ImageInfo.imageSize / ImageInfo.sectorSize;
					ImageInfo.mediaType = MediaType.GENERIC_HDD;
					ImageInfo.xmlMediaType = XmlMediaType.BlockMedia;
				}
				else if(isCdrom)
				{
					// Hardcoded on MAME for CD-ROM
					sectorsPerHunk = 8;
					ImageInfo.mediaType = MediaType.CDROM;
					ImageInfo.xmlMediaType = XmlMediaType.OpticalDisc;

					foreach(Track _trk in tracks.Values)
						ImageInfo.sectors += (_trk.TrackEndSector - _trk.TrackStartSector + 1);
				}
				else if(isGdrom)
				{
					// Hardcoded on MAME for GD-ROM
					sectorsPerHunk = 8;
					ImageInfo.mediaType = MediaType.GDROM;
					ImageInfo.xmlMediaType = XmlMediaType.OpticalDisc;

					foreach(Track _trk in tracks.Values)
						ImageInfo.sectors += (_trk.TrackEndSector - _trk.TrackStartSector + 1);
				}
				else
					throw new ImageNotSupportedException("Image does not represent a known media, aborting");
			}

			if(isCdrom || isGdrom)
			{
				offsetmap = new Dictionary<ulong, uint>();
				partitions = new List<Partition>();
				ulong partPos = 0;
				foreach(Track _track in tracks.Values)
				{
					Partition partition = new Partition();
					partition.PartitionDescription = _track.TrackDescription;
					partition.PartitionLength = (_track.TrackEndSector - _track.TrackStartSector + 1) * (ulong)_track.TrackRawBytesPerSector;
					partition.PartitionSectors = (_track.TrackEndSector - _track.TrackStartSector + 1);
					partition.PartitionSequence = _track.TrackSequence;
					partition.PartitionStart = partPos;
					partition.PartitionStartSector = _track.TrackStartSector;
					partition.PartitionType = _track.TrackType.ToString();
					partPos += partition.PartitionSectors;
					offsetmap.Add(_track.TrackStartSector, _track.TrackSequence);

					if(_track.TrackSubchannelType != TrackSubchannelType.None)
					{
						if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubchannel))
							ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubchannel);
					}

					switch(_track.TrackType)
					{
						case TrackType.CDMode1:
						case TrackType.CDMode2Form1:
							if(_track.TrackRawBytesPerSector == 2352)
							{
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubHeader))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubHeader);
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC);
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_P))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_P);
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_Q))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_Q);
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
							}
							break;
						case TrackType.CDMode2Form2:
							if(_track.TrackRawBytesPerSector == 2352)
							{
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubHeader))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubHeader);
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
							}
							break;
						case TrackType.CDMode2Formless:
							if(_track.TrackRawBytesPerSector == 2352)
							{
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
								if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
									ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
							}
							break;
					}

					if(_track.TrackBytesPerSector > ImageInfo.sectorSize)
						ImageInfo.sectorSize = (uint)_track.TrackBytesPerSector;

					partitions.Add(partition);
				}

				ImageInfo.imageHasPartitions = true;
				ImageInfo.imageHasSessions = true;
			}

			maxBlockCache = (int)(MaxCacheSize / (ImageInfo.sectorSize * sectorsPerHunk));
			maxSectorCache = (int)(MaxCacheSize / ImageInfo.sectorSize);

			imageStream = stream;

			sectorCache = new Dictionary<ulong, byte[]>();
			hunkCache = new Dictionary<ulong, byte[]>();

			return true;
		}

		Track GetTrack(ulong sector)
		{
			Track track = new Track();
			foreach(KeyValuePair<ulong, uint> kvp in offsetmap)
			{
				if(sector >= kvp.Key)
					tracks.TryGetValue(kvp.Value, out track);
			}
			return track;
		}

		ulong GetAbsoluteSector(ulong relativeSector, uint track)
		{
			Track _track = new Track();
			tracks.TryGetValue(track, out _track);
			return _track.TrackStartSector + relativeSector;
		}

		byte[] GetHunk(ulong hunkNo)
		{
			byte[] hunk;

			if(!hunkCache.TryGetValue(hunkNo, out hunk))
			{
				switch(mapVersion)
				{
					case 1:
						ulong offset = (hunkTable[hunkNo] & 0x00000FFFFFFFFFFF);
						ulong length = hunkTable[hunkNo] >> 44;

						byte[] compHunk = new byte[length];
						imageStream.Seek((long)offset, SeekOrigin.Begin);
						imageStream.Read(compHunk, 0, compHunk.Length);

						if(length == (sectorsPerHunk * ImageInfo.sectorSize))
						{
							hunk = compHunk;
						}
						else if((CHDCompression)hdrCompression > CHDCompression.Zlib)
							throw new ImageNotSupportedException(string.Format("Unsupported compression {0}", (CHDCompression)hdrCompression));
						else
						{
							DeflateStream zStream = new DeflateStream(new MemoryStream(compHunk), CompressionMode.Decompress);
							hunk = new byte[sectorsPerHunk * ImageInfo.sectorSize];
							int read = zStream.Read(hunk, 0, (int)(sectorsPerHunk * ImageInfo.sectorSize));
							if(read != sectorsPerHunk * ImageInfo.sectorSize)
								throw new IOException(string.Format("Unable to decompress hunk correctly, got {0} bytes, expected {1}", read, sectorsPerHunk * ImageInfo.sectorSize));
							zStream.Close();
							zStream = null;
						}
						break;
					case 3:
						byte[] entryBytes = new byte[16];
						Array.Copy(hunkMap, (int)(hunkNo * 16), entryBytes, 0, 16);
						CHDMapV3Entry entry = BigEndianMarshal.ByteArrayToStructureBigEndian<CHDMapV3Entry>(entryBytes);
						switch((CHDV3EntryFlags)(entry.flags & 0x0F))
						{
							case CHDV3EntryFlags.Invalid:
								throw new ArgumentException("Invalid hunk found.");
							case CHDV3EntryFlags.Compressed:
								switch((CHDCompression)hdrCompression)
								{
									case CHDCompression.None:
										goto uncompressedV3;
									case CHDCompression.Zlib:
									case CHDCompression.ZlibPlus:
										if(isHdd)
										{
											byte[] zHunk = new byte[(entry.lengthLsb << 16) + entry.lengthLsb];
											imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
											imageStream.Read(zHunk, 0, zHunk.Length);
											DeflateStream zStream = new DeflateStream(new MemoryStream(zHunk), CompressionMode.Decompress);
											hunk = new byte[bytesPerHunk];
											int read = zStream.Read(hunk, 0, (int)(bytesPerHunk));
											if(read != bytesPerHunk)
												throw new IOException(string.Format("Unable to decompress hunk correctly, got {0} bytes, expected {1}", read, bytesPerHunk));
											zStream.Close();
											zStream = null;
										}
										// TODO: Guess wth is MAME doing with these hunks
										else
											throw new ImageNotSupportedException("Compressed CD/GD-ROM hunks are not yet supported");
										break;
									case CHDCompression.AV:
										throw new ImageNotSupportedException(string.Format("Unsupported compression {0}", (CHDCompression)hdrCompression));
								}
								break;
							case CHDV3EntryFlags.Uncompressed:
								uncompressedV3:
								hunk = new byte[bytesPerHunk];
								imageStream.Seek((long)entry.offset, SeekOrigin.Begin);
								imageStream.Read(hunk, 0, hunk.Length);
								break;
							case CHDV3EntryFlags.Mini:
								hunk = new byte[bytesPerHunk];
								byte[] mini = new byte[8];
								mini = BigEndianBitConverter.GetBytes(entry.offset);
								for(int i = 0; i < bytesPerHunk; i++)
									hunk[i] = mini[i % 8];
								break;
							case CHDV3EntryFlags.SelfHunk:
								return GetHunk(entry.offset);
							case CHDV3EntryFlags.ParentHunk:
								throw new ImageNotSupportedException("Parent images are not supported");
							case CHDV3EntryFlags.SecondCompressed:
								throw new ImageNotSupportedException("FLAC is not supported");
							default:
								throw new ImageNotSupportedException(string.Format("Hunk type {0} is not supported", entry.flags & 0xF));
						}
						break;
					case 5:
						if(hdrCompression == 0)
						{
							hunk = new byte[bytesPerHunk];
							imageStream.Seek(hunkTableSmall[hunkNo] * bytesPerHunk, SeekOrigin.Begin);
							imageStream.Read(hunk, 0, hunk.Length);
						}
						else
							throw new ImageNotSupportedException("Compressed v5 hunks not yet supported");
						break;
					default:
						throw new ImageNotSupportedException(string.Format("Unsupported hunk map version {0}", mapVersion));
				}

				if(hunkCache.Count >= maxBlockCache)
					hunkCache.Clear();

				hunkCache.Add(hunkNo, hunk);
			}

			return hunk;
		}

		public override bool? VerifySector(ulong sectorAddress)
		{
			if(isHdd)
				return null;

			byte[] buffer = ReadSectorLong(sectorAddress);
			return Checksums.CDChecksums.CheckCDSector(buffer);
		}

		public override bool? VerifySector(ulong sectorAddress, uint track)
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");
			
			return VerifySector(GetAbsoluteSector(sectorAddress, track));
		}

		public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
		{
			UnknownLBAs = new List<ulong>();
			FailingLBAs = new List<ulong>();
			if(isHdd)
				return null;

			byte[] buffer = ReadSectorsLong(sectorAddress, length);
			int bps = (int)(buffer.Length / length);
			byte[] sector = new byte[bps];

			for(int i = 0; i < length; i++)
			{
				Array.Copy(buffer, i * bps, sector, 0, bps);
				bool? sectorStatus = Checksums.CDChecksums.CheckCDSector(sector);

				switch(sectorStatus)
				{
					case null:
						UnknownLBAs.Add((ulong)i + sectorAddress);
						break;
					case false:
						FailingLBAs.Add((ulong)i + sectorAddress);
						break;
				}
			}

			if(UnknownLBAs.Count > 0)
				return null;
			if(FailingLBAs.Count > 0)
				return false;
			return true;
		}

		public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
		{
			UnknownLBAs = new List<ulong>();
			FailingLBAs = new List<ulong>();
			if(isHdd)
				return null;

			byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
			int bps = (int)(buffer.Length / length);
			byte[] sector = new byte[bps];

			for(int i = 0; i < length; i++)
			{
				Array.Copy(buffer, i * bps, sector, 0, bps);
				bool? sectorStatus = Checksums.CDChecksums.CheckCDSector(sector);

				switch(sectorStatus)
				{
					case null:
						UnknownLBAs.Add((ulong)i + sectorAddress);
						break;
					case false:
						FailingLBAs.Add((ulong)i + sectorAddress);
						break;
				}
			}

			if(UnknownLBAs.Count > 0)
				return null;
			if(FailingLBAs.Count > 0)
				return false;
			return true;
		}

		public override bool? VerifyMediaImage()
		{
			byte[] calculated;
			if(mapVersion >= 3)
			{
				Checksums.SHA1Context sha1Ctx = new Checksums.SHA1Context();
				sha1Ctx.Init();
				for(uint i = 0; i < totalHunks; i++)
					sha1Ctx.Update(GetHunk(i));
				calculated = sha1Ctx.Final();
			}
			else
			{
				Checksums.MD5Context md5Ctx = new Checksums.MD5Context();
				md5Ctx.Init();
				for(uint i = 0; i < totalHunks; i++)
					md5Ctx.Update(GetHunk(i));
				calculated = md5Ctx.Final();
			}

			return expectedChecksum.SequenceEqual(calculated);
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
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

			byte[] sector;
			Track track = new Track();

			if(!sectorCache.TryGetValue(sectorAddress, out sector))
			{
				uint sectorSize;

				if(isHdd)
					sectorSize = ImageInfo.sectorSize;
				else
				{
					track = GetTrack(sectorAddress);
					sectorSize = (uint)track.TrackRawBytesPerSector;
				}

				ulong hunkNo = sectorAddress / sectorsPerHunk;
				ulong secOff = (sectorAddress * sectorSize) % (sectorsPerHunk * sectorSize);

				byte[] hunk = GetHunk(hunkNo);

				sector = new byte[ImageInfo.sectorSize];
				Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

				if(sectorCache.Count >= maxSectorCache)
					sectorCache.Clear();

				sectorCache.Add(sectorAddress, sector);
			}

			if(isHdd)
				return sector;

			uint sector_offset;
			uint sector_size;

			switch(track.TrackType)
			{
				case TrackType.CDMode1:
				case TrackType.CDMode2Form1:
					{
						if(track.TrackRawBytesPerSector == 2352)
						{
							sector_offset = 16;
							sector_size = 2048;
						}
						else
						{
							sector_offset = 0;
							sector_size = 2048;
						}
						break;
					}
				case TrackType.CDMode2Form2:
					{
						if(track.TrackRawBytesPerSector == 2352)
						{
							sector_offset = 16;
							sector_size = 2324;
						}
						else
						{
							sector_offset = 0;
							sector_size = 2324;
						}
						break;
					}
				case TrackType.CDMode2Formless:
					{
						if(track.TrackRawBytesPerSector == 2352)
						{
							sector_offset = 16;
							sector_size = 2336;
						}
						else
						{
							sector_offset = 0;
							sector_size = 2336;
						}
						break;
					}
				case TrackType.Audio:
					{
						sector_offset = 0;
						sector_size = 2352;
						break;
					}
				default:
					throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
			}

			byte[] buffer = new byte[sector_size];

			if(track.TrackType == TrackType.Audio && swapAudio)
			{
				for(int i = 0; i < 2352; i += 2)
				{
					buffer[i + 1] = sector[i];
					buffer[i] = sector[i + 1];
				}
			}
			else
				Array.Copy(sector, sector_offset, buffer, 0, sector_size);

			return buffer;
		}

		public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
		{
			if(isHdd)
				throw new FeatureNotPresentImageException("Hard disk images do not have sector tags");

			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

			byte[] sector;
			Track track = new Track();

			if(!sectorCache.TryGetValue(sectorAddress, out sector))
			{
				uint sectorSize;

				track = GetTrack(sectorAddress);
				sectorSize = (uint)track.TrackRawBytesPerSector;

				ulong hunkNo = sectorAddress / sectorsPerHunk;
				ulong secOff = (sectorAddress * sectorSize) % (sectorsPerHunk * sectorSize);

				byte[] hunk = GetHunk(hunkNo);

				sector = new byte[ImageInfo.sectorSize];
				Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

				if(sectorCache.Count >= maxSectorCache)
					sectorCache.Clear();

				sectorCache.Add(sectorAddress, sector);
			}

			if(isHdd)
				return sector;

			uint sector_offset;
			uint sector_size;

			if(tag == SectorTagType.CDSectorSubchannel)
			{
				if(track.TrackSubchannelType == TrackSubchannelType.None)
					throw new FeatureNotPresentImageException("Requested sector does not contain subchannel");
				else if(track.TrackSubchannelType == TrackSubchannelType.RawInterleaved)
				{
					sector_offset = (uint)track.TrackRawBytesPerSector;
					sector_size = 96;
				}
				else
					throw new FeatureSupportedButNotImplementedImageException(string.Format("Unsupported subchannel type {0}", track.TrackSubchannelType));
			}
			else
			{
				switch(track.TrackType)
				{
					case TrackType.CDMode1:
					case TrackType.CDMode2Form1:
						{
							if(track.TrackRawBytesPerSector == 2352)
							{
								switch(tag)
								{
									case SectorTagType.CDSectorSync:
										{
											sector_offset = 0;
											sector_size = 12;
											break;
										}
									case SectorTagType.CDSectorHeader:
										{
											sector_offset = 12;
											sector_size = 4;
											break;
										}
									case SectorTagType.CDSectorSubHeader:
										throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
									case SectorTagType.CDSectorECC:
										{
											sector_offset = 2076;
											sector_size = 276;
											break;
										}
									case SectorTagType.CDSectorECC_P:
										{
											sector_offset = 2076;
											sector_size = 172;
											break;
										}
									case SectorTagType.CDSectorECC_Q:
										{
											sector_offset = 2248;
											sector_size = 104;
											break;
										}
									case SectorTagType.CDSectorEDC:
										{
											sector_offset = 2064;
											sector_size = 4;
											break;
										}
									default:
										throw new ArgumentException("Unsupported tag requested", nameof(tag));
								}
							}
							else
								throw new FeatureNotPresentImageException("Requested sector does not contain tags");
							break;
						}
					case TrackType.CDMode2Form2:
						{
							if(track.TrackRawBytesPerSector == 2352)
							{
								switch(tag)
								{
									case SectorTagType.CDSectorSync:
										{
											sector_offset = 0;
											sector_size = 12;
											break;
										}
									case SectorTagType.CDSectorHeader:
										{
											sector_offset = 12;
											sector_size = 4;
											break;
										}
									case SectorTagType.CDSectorSubHeader:
										{
											sector_offset = 16;
											sector_size = 8;
											break;
										}
									case SectorTagType.CDSectorEDC:
										{
											sector_offset = 2348;
											sector_size = 4;
											break;
										}
									default:
										throw new ArgumentException("Unsupported tag requested", nameof(tag));
								}
							}
							else
							{
								switch(tag)
								{
									case SectorTagType.CDSectorSync:
									case SectorTagType.CDSectorHeader:
									case SectorTagType.CDSectorSubchannel:
									case SectorTagType.CDSectorECC:
									case SectorTagType.CDSectorECC_P:
									case SectorTagType.CDSectorECC_Q:
										throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
									case SectorTagType.CDSectorSubHeader:
										{
											sector_offset = 0;
											sector_size = 8;
											break;
										}
									case SectorTagType.CDSectorEDC:
										{
											sector_offset = 2332;
											sector_size = 4;
											break;
										}
									default:
										throw new ArgumentException("Unsupported tag requested", nameof(tag));
								}
							}
							break;
						}
					case TrackType.CDMode2Formless:
						{
							if(track.TrackRawBytesPerSector == 2352)
							{
								switch(tag)
								{
									case SectorTagType.CDSectorSync:
									case SectorTagType.CDSectorHeader:
									case SectorTagType.CDSectorECC:
									case SectorTagType.CDSectorECC_P:
									case SectorTagType.CDSectorECC_Q:
										throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
									case SectorTagType.CDSectorSubHeader:
										{
											sector_offset = 0;
											sector_size = 8;
											break;
										}
									case SectorTagType.CDSectorEDC:
										{
											sector_offset = 2332;
											sector_size = 4;
											break;
										}
									default:
										throw new ArgumentException("Unsupported tag requested", nameof(tag));
								}
							}
							else
								throw new FeatureNotPresentImageException("Requested sector does not contain tags");
							break;
						}
					case TrackType.Audio:
						throw new FeatureNotPresentImageException("Requested sector does not contain tags");
					default:
						throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
				}
			}

			byte[] buffer = new byte[sector_size];

			if(track.TrackType == TrackType.Audio && swapAudio)
			{
				for(int i = 0; i < 2352; i += 2)
				{
					buffer[i + 1] = sector[i];
					buffer[i] = sector[i + 1];
				}
			}
			else
				Array.Copy(sector, sector_offset, buffer, 0, sector_size);

			if(track.TrackType == TrackType.Audio && swapAudio)
			{
				for(int i = 0; i < 2352; i += 2)
				{
					buffer[i + 1] = sector[i];
					buffer[i] = sector[i + 1];
				}
			}
			else
				Array.Copy(sector, sector_offset, buffer, 0, sector_size);

			return buffer;
		}

		public override byte[] ReadSectors(ulong sectorAddress, uint length)
		{
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

			if(sectorAddress + length > ImageInfo.sectors)
				throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than available ({1})", sectorAddress + length, ImageInfo.sectors));

			MemoryStream ms = new MemoryStream();

			for(uint i = 0; i < length; i++)
			{
				byte[] sector = ReadSector(sectorAddress + i);
				ms.Write(sector, 0, sector.Length);
			}

			return ms.ToArray();
		}

		public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
		{
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

			if(sectorAddress + length > ImageInfo.sectors)
				throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than available ({1})", sectorAddress + length, ImageInfo.sectors));

			MemoryStream ms = new MemoryStream();

			for(uint i = 0; i < length; i++)
			{
				byte[] sector = ReadSectorTag(sectorAddress + i, tag);
				ms.Write(sector, 0, sector.Length);
			}

			return ms.ToArray();
		}

		public override byte[] ReadSectorLong(ulong sectorAddress)
		{
			if(isHdd)
				return ReadSector(sectorAddress);

			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

			byte[] sector;
			Track track = new Track();

			if(!sectorCache.TryGetValue(sectorAddress, out sector))
			{
				uint sectorSize;

				track = GetTrack(sectorAddress);
				sectorSize = (uint)track.TrackRawBytesPerSector;

				ulong hunkNo = sectorAddress / sectorsPerHunk;
				ulong secOff = (sectorAddress * sectorSize) % (sectorsPerHunk * sectorSize);

				byte[] hunk = GetHunk(hunkNo);

				sector = new byte[ImageInfo.sectorSize];
				Array.Copy(hunk, (int)secOff, sector, 0, sector.Length);

				if(sectorCache.Count >= maxSectorCache)
					sectorCache.Clear();

				sectorCache.Add(sectorAddress, sector);
			}

			byte[] buffer = new byte[track.TrackRawBytesPerSector];

			if(track.TrackType == TrackType.Audio && swapAudio)
			{
				for(int i = 0; i < 2352; i += 2)
				{
					buffer[i + 1] = sector[i];
					buffer[i] = sector[i + 1];
				}
			}
			else
				Array.Copy(sector, 0, buffer, 0, track.TrackRawBytesPerSector);

			return buffer;
		}

		public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
		{
			if(sectorAddress > ImageInfo.sectors - 1)
				throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));

			if(sectorAddress + length > ImageInfo.sectors)
				throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than available ({1})", sectorAddress + length, ImageInfo.sectors));

			MemoryStream ms = new MemoryStream();

			for(uint i = 0; i < length; i++)
			{
				byte[] sector = ReadSectorLong(sectorAddress + i);
				ms.Write(sector, 0, sector.Length);
			}

			return ms.ToArray();
		}

		public override string GetImageFormat()
		{
			return "Compressed Hunks of Data";
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
			if(ImageInfo.readableMediaTags.Contains(MediaTagType.ATA_IDENTIFY))
				return identify;

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
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

			return partitions;
		}

		public override List<Track> GetTracks()
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

			List<Track> _trks = new List<Track>();
			foreach(Track track in tracks.Values)
				_trks.Add(track);

			return _trks;
		}

		public override List<Track> GetSessionTracks(Session session)
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

			return GetSessionTracks(session.SessionSequence);
		}

		public override List<Track> GetSessionTracks(ushort session)
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

			List<Track> _trks = new List<Track>();
			foreach(Track track in tracks.Values)
			{
				if(track.TrackSession == session)
					_trks.Add(track);
			}

			return _trks;
		}

		public override List<Session> GetSessions()
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical sessions on a hard disk image");

			throw new NotImplementedException();
		}

		public override byte[] ReadSector(ulong sectorAddress, uint track)
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

			return ReadSector(GetAbsoluteSector(sectorAddress, track));
		}

		public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

			return ReadSectorTag(GetAbsoluteSector(sectorAddress, track), tag);
		}

		public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

			return ReadSectors(GetAbsoluteSector(sectorAddress, track), length);
		}

		public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

			return ReadSectorsTag(GetAbsoluteSector(sectorAddress, track), length, tag);
		}

		public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

			return ReadSectorLong(GetAbsoluteSector(sectorAddress, track));
		}

		public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
		{
			if(isHdd)
				throw new FeaturedNotSupportedByDiscImageException("Cannot access optical tracks on a hard disk image");

			return ReadSectorLong(GetAbsoluteSector(sectorAddress, track), length);
		}

		#endregion Unsupported features
	}
}

