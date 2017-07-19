// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DiscJuggler.cs
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.DiscImages
{
	// Support separate data files? Never seen a DiscJuggler image using them anyways...
	public class DiscJuggler : ImagePlugin
	{
		Stream imageStream;
		List<Session> sessions;
		List<Track> tracks;
		byte[] cdtext;
		Dictionary<uint, ulong> offsetmap;
		List<Partition> partitions;
		Dictionary<uint, byte> trackFlags;

		public DiscJuggler()
		{
			Name = "DiscJuggler";
			PluginUUID = new Guid("2444DBC6-CD35-424C-A227-39B0C4DB01B2");
			ImageInfo = new ImageInfo();
			ImageInfo.readableSectorTags = new List<SectorTagType>();
			ImageInfo.readableMediaTags = new List<MediaTagType>();
			ImageInfo.imageHasPartitions = true;
			ImageInfo.imageHasSessions = true;
			ImageInfo.imageVersion = null;
			ImageInfo.imageApplicationVersion = null;
			ImageInfo.imageName = null;
			ImageInfo.imageCreator = null;
			ImageInfo.mediaManufacturer = null;
			ImageInfo.mediaModel = null;
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
			imageStream = imageFilter.GetDataForkStream();

			imageStream.Seek(-4, SeekOrigin.End);
			byte[] dscLen_b = new byte[4];
			imageStream.Read(dscLen_b, 0, 4);
			int dscLen = BitConverter.ToInt32(dscLen_b, 0);

			DicConsole.DebugWriteLine("DiscJuggler plugin", "dscLen = {0}", dscLen);

			if(dscLen >= imageStream.Length)
				return false;

			byte[] descriptor = new byte[dscLen];
			imageStream.Seek(-dscLen, SeekOrigin.End);
			imageStream.Read(descriptor, 0, dscLen);

			// Sessions
			if(descriptor[0] > 99 || descriptor[0] == 0)
				return false;

			// Seems all sessions start with this data
			if(descriptor[1] != 0x00 || descriptor[3] != 0x00 || descriptor[4] != 0x00 ||
			   descriptor[5] != 0x00 || descriptor[6] != 0x00 || descriptor[7] != 0x00 ||
			   descriptor[8] != 0x00 || descriptor[9] != 0x00 || descriptor[10] != 0x01 ||
			   descriptor[11] != 0x00 || descriptor[12] != 0x00 || descriptor[13] != 0x00 ||
			   descriptor[14] != 0xFF || descriptor[15] != 0xFF)
				return false;

			// Too many tracks
			if(descriptor[2] > 99)
				return false;

			return true;
		}

		public override bool OpenImage(Filter imageFilter)
		{
			imageStream = imageFilter.GetDataForkStream();

			imageStream.Seek(-4, SeekOrigin.End);
			byte[] dscLen_b = new byte[4];
			imageStream.Read(dscLen_b, 0, 4);
			int dscLen = BitConverter.ToInt32(dscLen_b, 0);

			if(dscLen >= imageStream.Length)
				return false;

			byte[] descriptor = new byte[dscLen];
			imageStream.Seek(-dscLen, SeekOrigin.End);
			imageStream.Read(descriptor, 0, dscLen);

			// Sessions
			if(descriptor[0] > 99 || descriptor[0] == 0)
				return false;

			int position = 1;

			ushort sessionSequence = 0;
			sessions = new List<Session>();
			tracks = new List<Track>();
			partitions = new List<Partition>();
			offsetmap = new Dictionary<uint, ulong>();
			trackFlags = new Dictionary<uint, byte>();
			ushort mediumType;
			byte maxS = descriptor[0];

			DicConsole.DebugWriteLine("DiscJuggler plugin", "maxS = {0}", maxS);
			uint lastSessionTrack = 0;
			ulong currentOffset = 0;

			// Read sessions
			for(byte s = 0; s <= maxS; s++)
			{
				DicConsole.DebugWriteLine("DiscJuggler plugin", "s = {0}", s);

				// Seems all sessions start with this data
				if(descriptor[position + 0] != 0x00 || descriptor[position + 2] != 0x00 || descriptor[position + 3] != 0x00 ||
				   descriptor[position + 4] != 0x00 || descriptor[position + 5] != 0x00 || descriptor[position + 6] != 0x00 ||
				   descriptor[position + 7] != 0x00 || descriptor[position + 8] != 0x00 || descriptor[position + 9] != 0x01 ||
				   descriptor[position + 10] != 0x00 || descriptor[position + 11] != 0x00 || descriptor[position + 12] != 0x00 ||
				   descriptor[position + 13] != 0xFF || descriptor[position + 14] != 0xFF)
					return false;

				// Too many tracks
				if(descriptor[position + 1] > 99)
					return false;

				byte maxT = descriptor[position + 1];
				DicConsole.DebugWriteLine("DiscJuggler plugin", "maxT = {0}", maxT);

				sessionSequence++;
				Session session = new Session();
				session.SessionSequence = sessionSequence;
				session.EndTrack = uint.MinValue;
				session.StartTrack = uint.MaxValue;

				position += 15;
				bool addedATrack = false;

				// Read track
				for(byte t = 0; t < maxT; t++)
				{
					addedATrack = false;
					DicConsole.DebugWriteLine("DiscJuggler plugin", "t = {0}", t);
					Track track = new Track();

					// Skip unknown
					position += 16;

					byte[] trackFilename_b = new byte[descriptor[position]];
					position++;
					Array.Copy(descriptor, position, trackFilename_b, 0, trackFilename_b.Length);
					position += trackFilename_b.Length;
					track.TrackFile = Path.GetFileName(Encoding.Default.GetString(trackFilename_b));
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\tfilename = {0}", track.TrackFile);

					// Skip unknown
					position += 29;

					mediumType = BitConverter.ToUInt16(descriptor, position);
					position += 2;
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\tmediumType = {0}", mediumType);

					// Read indices
					track.Indexes = new Dictionary<int, ulong>();
					ushort maxI = BitConverter.ToUInt16(descriptor, position);
					position += 2;
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\tmaxI = {0}", maxI);
					for(ushort i = 0; i < maxI; i++)
					{
						uint index = BitConverter.ToUInt32(descriptor, position);
						track.Indexes.Add(i, index);
						position += 4;
						DicConsole.DebugWriteLine("DiscJuggler plugin", "\tindex[{1}] = {0}", index, i);
					}

					// Read CD-Text
					uint maxC = BitConverter.ToUInt32(descriptor, position);
					position += 4;
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\tmaxC = {0}", maxC);
					for(uint c = 0; c < maxC; c++)
					{
						for(int cb = 0; cb < 18; cb++)
						{
							int bLen = descriptor[position];
							position++;
							DicConsole.DebugWriteLine("DiscJuggler plugin", "\tc[{1}][{2}].Length = {0}", bLen, c, cb);
							if(bLen > 0)
							{
								byte[] textBlk = new byte[bLen];
								Array.Copy(descriptor, position, textBlk, 0, bLen);
								position += bLen;
								// Track title
								if(cb == 10)
								{
									track.TrackDescription = Encoding.Default.GetString(textBlk, 0, bLen);
									DicConsole.DebugWriteLine("DiscJuggler plugin", "\tTrack title = {0}", track.TrackDescription);
								}
							}
						}
					}

					position += 2;
					uint trackMode = BitConverter.ToUInt32(descriptor, position);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\ttrackMode = {0}", trackMode);
					position += 4;

					// Skip unknown
					position += 4;

					session.SessionSequence = (ushort)(BitConverter.ToUInt32(descriptor, position) + 1); 
					track.TrackSession = (ushort)(session.SessionSequence + 1);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\tsession = {0}", session.SessionSequence);
					position += 4;
					track.TrackSequence = BitConverter.ToUInt32(descriptor, position) + lastSessionTrack + 1;
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\ttrack = {1} + {2} + 1 = {0}", track.TrackSequence, BitConverter.ToUInt32(descriptor, position), lastSessionTrack);
					position += 4;
					track.TrackStartSector = BitConverter.ToUInt32(descriptor, position);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\ttrackStart = {0}", track.TrackStartSector);
					position += 4;
					uint trackLen = BitConverter.ToUInt32(descriptor, position);
					track.TrackEndSector = track.TrackStartSector + trackLen - 1;
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\ttrackEnd = {0}", track.TrackEndSector);
					position += 4;

					if(track.TrackSequence > session.EndTrack)
					{
						session.EndTrack = track.TrackSequence;
						session.EndSector = track.TrackEndSector;
					}
					if(track.TrackSequence < session.StartTrack)
					{
						session.StartTrack = track.TrackSequence;
						session.StartSector = track.TrackStartSector;
					}

					// Skip unknown
					position += 16;

					uint readMode = BitConverter.ToUInt32(descriptor, position);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\treadMode = {0}", readMode);
					position += 4;
					uint trackCtl = BitConverter.ToUInt32(descriptor, position);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\ttrackCtl = {0}", trackCtl);
					position += 4;

					// Skip unknown
					position += 9;

					byte[] isrc = new byte[12];
					Array.Copy(descriptor, position, isrc, 0, 12);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\tisrc = {0}", StringHandlers.CToString(isrc));
					position += 12;
					uint isrc_valid = BitConverter.ToUInt32(descriptor, position);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\tisrc_valid = {0}", isrc_valid);
					position += 4;

					// Skip unknown
					position += 87;

					byte sessionType = descriptor[position];
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\tsessionType = {0}", sessionType);
					position++;

					// Skip unknown
					position += 5;

					byte trackFollows = descriptor[position];
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\ttrackFollows = {0}", trackFollows);
					position += 2;

					uint endAddress = BitConverter.ToUInt32(descriptor, position);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "\tendAddress = {0}", endAddress);
					position += 4;

					// As to skip the lead-in
					bool firstTrack = currentOffset == 0;

					track.TrackSubchannelType = TrackSubchannelType.None;

					switch(trackMode)
					{
						// Audio
						case 0:
							if(ImageInfo.sectorSize < 2352)
								ImageInfo.sectorSize = 2352;
							track.TrackType = TrackType.Audio;
							track.TrackBytesPerSector = 2352;
							track.TrackRawBytesPerSector = 2352;
							switch(readMode)
							{
								case 2:
									if(firstTrack)
										currentOffset += 150 * (ulong)track.TrackRawBytesPerSector;
									track.TrackFileOffset = currentOffset;
									currentOffset += trackLen * (ulong)track.TrackRawBytesPerSector;
									break;
								case 3:
									if(firstTrack)
										currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 16);
									track.TrackFileOffset = currentOffset;
									track.TrackSubchannelFile = track.TrackFile;
									track.TrackSubchannelOffset = currentOffset;
									track.TrackSubchannelType = TrackSubchannelType.Q16Interleaved;
									currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 16);
									break;
								case 4:
									if(firstTrack)
										currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 96);
									track.TrackFileOffset = currentOffset;
									track.TrackSubchannelFile = track.TrackFile;
									track.TrackSubchannelOffset = currentOffset;
									track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
									currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 96);
									break;
								default:
									throw new ImageNotSupportedException(string.Format("Unknown read mode {0}", readMode));
							}
							break;
						// Mode 1 or DVD
						case 1:
							if(ImageInfo.sectorSize < 2048)
								ImageInfo.sectorSize = 2048;
							track.TrackType = TrackType.CDMode1;
							track.TrackBytesPerSector = 2048;
							switch(readMode)
							{
								case 0:
									track.TrackRawBytesPerSector = 2048;
									if(firstTrack)
										currentOffset += 150 * (ulong)track.TrackRawBytesPerSector;
									track.TrackFileOffset = currentOffset;
									currentOffset += trackLen * (ulong)track.TrackRawBytesPerSector;
									break;
								case 1:
									throw new ImageNotSupportedException(string.Format("Invalid read mode {0} for this track", readMode));
								case 2:
									track.TrackRawBytesPerSector = 2352;
									currentOffset += trackLen * (ulong)track.TrackRawBytesPerSector;
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_P))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_P);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_Q))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_Q);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
									break;
								case 3:
									track.TrackRawBytesPerSector = 2352;
									if(firstTrack)
										currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 16);
									track.TrackFileOffset = currentOffset;
									track.TrackSubchannelFile = track.TrackFile;
									track.TrackSubchannelOffset = currentOffset;
									track.TrackSubchannelType = TrackSubchannelType.Q16Interleaved;
									currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 16);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_P))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_P);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_Q))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_Q);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
									break;
								case 4:
									track.TrackRawBytesPerSector = 2352;
									if(firstTrack)
										currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 96);
									track.TrackFileOffset = currentOffset;
									track.TrackSubchannelFile = track.TrackFile;
									track.TrackSubchannelOffset = currentOffset;
									track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
									currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 96);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_P))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_P);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorECC_Q))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorECC_Q);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
								break;
								default:
									throw new ImageNotSupportedException(string.Format("Unknown read mode {0}", readMode));
							}
							break;
						// Mode 2
						case 2:
							if(ImageInfo.sectorSize < 2336)
								ImageInfo.sectorSize = 2336;
							track.TrackType = TrackType.CDMode2Formless;
							track.TrackBytesPerSector = 2336;
							switch(readMode)
							{
								case 0:
									throw new ImageNotSupportedException(string.Format("Invalid read mode {0} for this track", readMode));
								case 1:
									track.TrackRawBytesPerSector = 2336;
									if(firstTrack)
										currentOffset += 150 * (ulong)track.TrackRawBytesPerSector;
									track.TrackFileOffset = currentOffset;
									currentOffset += trackLen * (ulong)track.TrackRawBytesPerSector;
									break;
								case 2:
									track.TrackRawBytesPerSector = 2352;
									currentOffset += trackLen * (ulong)track.TrackRawBytesPerSector;
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
									break;
								case 3:
									track.TrackRawBytesPerSector = 2352;
									if(firstTrack)
										currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 16);
									track.TrackFileOffset = currentOffset;
									track.TrackSubchannelFile = track.TrackFile;
									track.TrackSubchannelOffset = currentOffset;
									track.TrackSubchannelType = TrackSubchannelType.Q16Interleaved;
									currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 16);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
									break;
								case 4:
									track.TrackRawBytesPerSector = 2352;
									if(firstTrack)
										currentOffset += 150 * (ulong)(track.TrackRawBytesPerSector + 96);
									track.TrackFileOffset = currentOffset;
									track.TrackSubchannelFile = track.TrackFile;
									track.TrackSubchannelOffset = currentOffset;
									track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
									currentOffset += trackLen * (ulong)(track.TrackRawBytesPerSector + 96);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
									if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
										ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
									break;
								default:
									throw new ImageNotSupportedException(string.Format("Unknown read mode {0}", readMode));
							}
							break;
						default:
							throw new ImageNotSupportedException(string.Format("Unknown track mode {0}", trackMode));
					}

					track.TrackFile = imageFilter.GetFilename();
					track.TrackFilter = imageFilter;
					if(track.TrackSubchannelType != TrackSubchannelType.None)
					{
						track.TrackSubchannelFile = imageFilter.GetFilename();
						track.TrackSubchannelFilter = imageFilter;
						if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubchannel))
							ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubchannel);
					}

					Partition partition = new Partition();
					partition.Description = track.TrackDescription;
					partition.Size = (ulong)(trackLen * track.TrackBytesPerSector);
					partition.Length = trackLen;
					ImageInfo.sectors += partition.Length;
					partition.Sequence = track.TrackSequence;
					partition.Offset = track.TrackFileOffset;
					partition.Start = track.TrackStartSector;
					partition.Type = track.TrackType.ToString();
					partitions.Add(partition);
					offsetmap.Add(track.TrackSequence, track.TrackStartSector);
					tracks.Add(track);
					trackFlags.Add(track.TrackSequence, (byte)(trackCtl & 0xFF));
					addedATrack = true;
				}

				if(addedATrack)
				{
					lastSessionTrack = session.EndTrack;
					sessions.Add(session);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "session.StartTrack = {0}", session.StartTrack);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "session.StartSector = {0}", session.StartSector);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "session.EndTrack = {0}", session.EndTrack);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "session.EndSector = {0}", session.EndSector);
					DicConsole.DebugWriteLine("DiscJuggler plugin", "session.SessionSequence = {0}", session.SessionSequence);
				}
			}

			// Skip unknown
			position += 16;

			DicConsole.DebugWriteLine("DiscJuggler plugin", "Current position = {0}", position);
			byte[] filename_b = new byte[descriptor[position]];
			position++;
			Array.Copy(descriptor, position, filename_b, 0, filename_b.Length);
			position += filename_b.Length;
			string filename = Path.GetFileName(Encoding.Default.GetString(filename_b));
			DicConsole.DebugWriteLine("DiscJuggler plugin", "filename = {0}", filename);

			// Skip unknown
			position += 29;

			mediumType = BitConverter.ToUInt16(descriptor, position);
			position += 2;
			DicConsole.DebugWriteLine("DiscJuggler plugin", "mediumType = {0}", mediumType);

			uint discSize = BitConverter.ToUInt32(descriptor, position);
			position += 4;
			DicConsole.DebugWriteLine("DiscJuggler plugin", "discSize = {0}", discSize);

			byte[] volid_b = new byte[descriptor[position]];
			position++;
			Array.Copy(descriptor, position, volid_b, 0, volid_b.Length);
			position += volid_b.Length;
			string volid = Path.GetFileName(Encoding.Default.GetString(volid_b));
			DicConsole.DebugWriteLine("DiscJuggler plugin", "volid = {0}", volid);

			// Skip unknown
			position += 9;

			byte[] mcn = new byte[13];
			Array.Copy(descriptor, position, mcn, 0, 13);
			DicConsole.DebugWriteLine("DiscJuggler plugin", "mcn = {0}", StringHandlers.CToString(mcn));
			position += 13;
			uint mcn_valid = BitConverter.ToUInt32(descriptor, position);
			DicConsole.DebugWriteLine("DiscJuggler plugin", "mcn_valid = {0}", mcn_valid);
			position += 4;

			uint cdtextLen = BitConverter.ToUInt32(descriptor, position);
			DicConsole.DebugWriteLine("DiscJuggler plugin", "cdtextLen = {0}", cdtextLen);
			position += 4;
			if(cdtextLen > 0)
			{
				cdtext = new byte[cdtextLen];
				Array.Copy(descriptor, position, cdtext, 0, cdtextLen);
				position += (int)cdtextLen;
				ImageInfo.readableMediaTags.Add(MediaTagType.CD_TEXT);
			}

			// Skip unknown
			position += 12;

			DicConsole.DebugWriteLine("DiscJuggler plugin", "End position = {0}", position);

			if(ImageInfo.mediaType == MediaType.CDROM)
			{
				bool data = false;
				bool mode2 = false;
				bool firstaudio = false;
				bool firstdata = false;
				bool audio = false;

				for(int i = 0; i < tracks.Count; i++)
				{
					// First track is audio
					firstaudio |= i == 0 && tracks[i].TrackType == TrackType.Audio;

					// First track is data
					firstdata |= i == 0 && tracks[i].TrackType != TrackType.Audio;

					// Any non first track is data
					data |= i != 0 && tracks[i].TrackType != TrackType.Audio;

					// Any non first track is audio
					audio |= i != 0 && tracks[i].TrackType == TrackType.Audio;

					switch(tracks[i].TrackType)
					{
						case TrackType.CDMode2Form1:
						case TrackType.CDMode2Form2:
						case TrackType.CDMode2Formless:
							mode2 = true;
							break;
					}
				}

				if(!data && !firstdata)
					ImageInfo.mediaType = MediaType.CDDA;
				else if(firstaudio && data && sessions.Count > 1 && mode2)
					ImageInfo.mediaType = MediaType.CDPLUS;
				else if((firstdata && audio) || mode2)
					ImageInfo.mediaType = MediaType.CDROMXA;
				else if(!audio)
					ImageInfo.mediaType = MediaType.CDROM;
				else
					ImageInfo.mediaType = MediaType.CD;
			}

			ImageInfo.imageApplication = "DiscJuggler";
			ImageInfo.imageSize = (ulong)imageFilter.GetDataForkLength();
			ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
			ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
			ImageInfo.xmlMediaType = XmlMediaType.OpticalDisc;

			return true;
		}

		static MediaType DecodeCDIMediumType(ushort type)
		{
			switch(type)
			{
				case 56:
					return MediaType.DVDROM;
				case 152:
					return MediaType.CDROM;
				default:
					return MediaType.Unknown;
			}
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

		public override byte[] ReadDiskTag(MediaTagType tag)
		{
			switch(tag)
			{
				case MediaTagType.CD_TEXT:
					{
						if(cdtext != null && cdtext.Length > 0)
							return cdtext;
						throw new FeatureNotPresentImageException("Image does not contain CD-TEXT information.");
					}
				default:
					throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
			}
		}

		public override byte[] ReadSector(ulong sectorAddress)
		{
			return ReadSectors(sectorAddress, 1);
		}

		public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
		{
			return ReadSectorsTag(sectorAddress, 1, tag);
		}

		public override byte[] ReadSector(ulong sectorAddress, uint track)
		{
			return ReadSectors(sectorAddress, 1, track);
		}

		public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
		{
			return ReadSectorsTag(sectorAddress, 1, track, tag);
		}

		public override byte[] ReadSectors(ulong sectorAddress, uint length)
		{
			foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
			{
				if(sectorAddress >= kvp.Value)
				{
					foreach(Track _track in tracks)
					{
						if(_track.TrackSequence == kvp.Key)
						{
							if(sectorAddress < _track.TrackEndSector)
								return ReadSectors((sectorAddress - kvp.Value), length, kvp.Key);
						}
					}
				}
			}

			throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));
		}

		public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
		{
			foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
			{
				if(sectorAddress >= kvp.Value)
				{
					foreach(Track _track in tracks)
					{
						if(_track.TrackSequence == kvp.Key)
						{
							if(sectorAddress < _track.TrackEndSector)
								return ReadSectorsTag((sectorAddress - kvp.Value), length, kvp.Key, tag);
						}
					}
				}
			}

			throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));
		}

		public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
		{
			Track _track = new Track();

			_track.TrackSequence = 0;

			foreach(Track __track in tracks)
			{
				if(__track.TrackSequence == track)
				{
					_track = __track;
					break;
				}
			}

			if(_track.TrackSequence == 0)
				throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

			if(length + sectorAddress > (_track.TrackEndSector))
				throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length + sectorAddress, _track.TrackEndSector));

			uint sector_offset;
			uint sector_size;
			uint sector_skip;

			switch(_track.TrackType)
			{
				case TrackType.Audio:
					{
						sector_offset = 0;
						sector_size = 2352;
						sector_skip = 0;
						break;
					}
				case TrackType.CDMode1:
					if(_track.TrackRawBytesPerSector == 2352)
					{
						sector_offset = 16;
						sector_size = 2048;
						sector_skip = 288;
					}
					else
					{
						sector_offset = 0;
						sector_size = 2048;
						sector_skip = 0;
					}
					break;
				case TrackType.CDMode2Formless:
					if(_track.TrackRawBytesPerSector == 2352)
					{
						sector_offset = 16;
						sector_size = 2336;
						sector_skip = 0;
					}
					else
					{
						sector_offset = 0;
						sector_size = 2336;
						sector_skip = 0;
					}
					break;
				default:
					throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
			}

			switch(_track.TrackSubchannelType)
			{
				case TrackSubchannelType.None:
					sector_skip += 0;
					break;
				case TrackSubchannelType.Q16Interleaved:
					sector_skip += 16;
					break;
				case TrackSubchannelType.PackedInterleaved:
					sector_skip += 96;
					break;
				default:
					throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
			}

			byte[] buffer = new byte[sector_size * length];

			imageStream.Seek((long)(_track.TrackFileOffset + sectorAddress * (ulong)_track.TrackRawBytesPerSector), SeekOrigin.Begin);
			if(sector_offset == 0 && sector_skip == 0)
				imageStream.Read(buffer, 0, buffer.Length);
			else
			{
				for(int i = 0; i < length; i++)
				{
					byte[] sector = new byte[sector_size];
					imageStream.Seek(sector_offset, SeekOrigin.Current);
					imageStream.Read(sector, 0, sector.Length);
					imageStream.Seek(sector_skip, SeekOrigin.Current);
					Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
				}
			}

			return buffer;
		}

		public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
		{
			Track _track = new Track();

			_track.TrackSequence = 0;

			foreach(Track __track in tracks)
			{
				if(__track.TrackSequence == track)
				{
					_track = __track;
					break;
				}
			}

			if(_track.TrackSequence == 0)
				throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

			if(length + sectorAddress > (_track.TrackEndSector))
				throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length + sectorAddress, _track.TrackEndSector));

			if(_track.TrackType == TrackType.Data)
				throw new ArgumentException("Unsupported tag requested", nameof(tag));

			byte[] buffer;

			switch(tag)
			{
				case SectorTagType.CDSectorECC:
				case SectorTagType.CDSectorECC_P:
				case SectorTagType.CDSectorECC_Q:
				case SectorTagType.CDSectorEDC:
				case SectorTagType.CDSectorHeader:
				case SectorTagType.CDSectorSubchannel:
				case SectorTagType.CDSectorSubHeader:
				case SectorTagType.CDSectorSync:
					break;
				case SectorTagType.CDTrackFlags:
					byte flag;
					if(trackFlags.TryGetValue(track, out flag))
						return new byte[] { flag };
					throw new ArgumentException("Unsupported tag requested", nameof(tag));
				default:
					throw new ArgumentException("Unsupported tag requested", nameof(tag));
			}

			uint sector_offset;
			uint sector_size;
			uint sector_skip;

			switch(_track.TrackType)
			{
				case TrackType.CDMode1:
					if(_track.TrackRawBytesPerSector != 2352)
						throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
					switch(tag)
					{
						case SectorTagType.CDSectorSync:
							{
								sector_offset = 0;
								sector_size = 12;
								sector_skip = 2340;
								break;
							}
						case SectorTagType.CDSectorHeader:
							{
								sector_offset = 12;
								sector_size = 4;
								sector_skip = 2336;
								break;
							}
						case SectorTagType.CDSectorSubHeader:
							throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
						case SectorTagType.CDSectorECC:
							{
								sector_offset = 2076;
								sector_size = 276;
								sector_skip = 0;
								break;
							}
						case SectorTagType.CDSectorECC_P:
							{
								sector_offset = 2076;
								sector_size = 172;
								sector_skip = 104;
								break;
							}
						case SectorTagType.CDSectorECC_Q:
							{
								sector_offset = 2248;
								sector_size = 104;
								sector_skip = 0;
								break;
							}
						case SectorTagType.CDSectorEDC:
							{
								sector_offset = 2064;
								sector_size = 4;
								sector_skip = 284;
								break;
							}
						case SectorTagType.CDSectorSubchannel:
							if(_track.TrackSubchannelType == TrackSubchannelType.None)
								throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
							if(_track.TrackSubchannelType == TrackSubchannelType.Q16Interleaved)
								throw new ArgumentException("Q16 subchannel not yet supported");

							sector_offset = 2352;
							sector_size = 96;
							sector_skip = 0;
							break;
						default:
							throw new ArgumentException("Unsupported tag requested", nameof(tag));
					}
					break;
				case TrackType.CDMode2Formless:
					if(_track.TrackRawBytesPerSector != 2352)
						throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
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
									sector_skip = 2328;
									break;
								}
							case SectorTagType.CDSectorEDC:
								{
									sector_offset = 2332;
									sector_size = 4;
									sector_skip = 0;
									break;
								}
							case SectorTagType.CDSectorSubchannel:
								if(_track.TrackSubchannelType == TrackSubchannelType.None)
									throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
								if(_track.TrackSubchannelType == TrackSubchannelType.Q16Interleaved)
									throw new ArgumentException("Q16 subchannel not yet supported");

								sector_offset = 2352;
								sector_size = 96;
								sector_skip = 0;
								break;
							default:
								throw new ArgumentException("Unsupported tag requested", nameof(tag));
						}
						break;
					}
				case TrackType.Audio:
					{
						switch(tag)
						{
							case SectorTagType.CDSectorSubchannel:
								if(_track.TrackSubchannelType == TrackSubchannelType.None)
									throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
								if(_track.TrackSubchannelType == TrackSubchannelType.Q16Interleaved)
									throw new ArgumentException("Q16 subchannel not yet supported");

								sector_offset = 2352;
								sector_size = 96;
								sector_skip = 0;
								break;
							default:
								throw new ArgumentException("Unsupported tag requested", nameof(tag));
						}
						break;
					}
				default:
					throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
			}

			switch(_track.TrackSubchannelType)
			{
				case TrackSubchannelType.None:
					sector_skip += 0;
					break;
				case TrackSubchannelType.Q16Interleaved:
					sector_skip += 16;
					break;
				case TrackSubchannelType.PackedInterleaved:
					sector_skip += 96;
					break;
				default:
					throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
			}

			buffer = new byte[sector_size * length];

			imageStream.Seek((long)(_track.TrackFileOffset + sectorAddress * (ulong)_track.TrackRawBytesPerSector), SeekOrigin.Begin);
			if(sector_offset == 0 && sector_skip == 0)
				imageStream.Read(buffer, 0, buffer.Length);
			else
			{
				for(int i = 0; i < length; i++)
				{
					byte[] sector = new byte[sector_size];
					imageStream.Seek(sector_offset, SeekOrigin.Current);
					imageStream.Read(sector, 0, sector.Length);
					imageStream.Seek(sector_skip, SeekOrigin.Current);
					Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
				}
			}

			return buffer;
		}

		public override byte[] ReadSectorLong(ulong sectorAddress)
		{
			return ReadSectorsLong(sectorAddress, 1);
		}

		public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
		{
			return ReadSectorsLong(sectorAddress, 1, track);
		}

		public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
		{
			foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
			{
				if(sectorAddress >= kvp.Value)
				{
					foreach(Track track in tracks)
					{
						if(track.TrackSequence == kvp.Key)
						{
							if((sectorAddress - kvp.Value) < (track.TrackEndSector - track.TrackStartSector))
								return ReadSectorsLong((sectorAddress - kvp.Value), length, kvp.Key);
						}
					}
				}
			}

			throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));
		}

		public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
		{
			Track _track = new Track();

			_track.TrackSequence = 0;

			foreach(Track __track in tracks)
			{
				if(__track.TrackSequence == track)
				{
					_track = __track;
					break;
				}
			}

			if(_track.TrackSequence == 0)
				throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

			if(length + sectorAddress > (_track.TrackEndSector))
				throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length + sectorAddress, _track.TrackEndSector));

			uint sector_offset = 0;
			uint sector_size = (uint)_track.TrackRawBytesPerSector;
			uint sector_skip = 0;

			switch(_track.TrackSubchannelType)
			{
				case TrackSubchannelType.None:
					sector_skip += 0;
					break;
				case TrackSubchannelType.Q16Interleaved:
					sector_skip += 16;
					break;
				case TrackSubchannelType.PackedInterleaved:
					sector_skip += 96;
					break;
				default:
					throw new FeatureSupportedButNotImplementedImageException("Unsupported subchannel type");
			}

			byte[] buffer = new byte[sector_size * length];

			imageStream.Seek((long)(_track.TrackFileOffset + sectorAddress * (ulong)_track.TrackRawBytesPerSector), SeekOrigin.Begin);
			if(sector_offset == 0 && sector_skip == 0)
				imageStream.Read(buffer, 0, buffer.Length);
			else
			{
				for(int i = 0; i < length; i++)
				{
					byte[] sector = new byte[sector_size];
					imageStream.Seek(sector_offset, SeekOrigin.Current);
					imageStream.Read(sector, 0, sector.Length);
					imageStream.Seek(sector_skip, SeekOrigin.Current);
					Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
				}
			}

			return buffer;
		}

		public override string GetImageFormat()
		{
			return "DiscJuggler";
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
			return ImageInfo.driveSerialNumber;
		}

		public override string GetMediaBarcode()
		{
			return ImageInfo.mediaBarcode;
		}

		public override string GetMediaPartNumber()
		{
			return ImageInfo.mediaPartNumber;
		}

		public override MediaType GetMediaType()
		{
			return ImageInfo.mediaType;
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
			return partitions;
		}

		public override List<Track> GetTracks()
		{
			return tracks;
		}

		public override List<Track> GetSessionTracks(Session session)
		{
			if(sessions.Contains(session))
			{
				return GetSessionTracks(session.SessionSequence);
			}
			throw new ImageNotSupportedException("Session does not exist in disc image");
		}

		public override List<Track> GetSessionTracks(ushort session)
		{
			List<Track> _tracks = new List<Track>();
			foreach(Track _track in tracks)
			{
				if(_track.TrackSession == session)
					_tracks.Add(_track);
			}

			return _tracks;
		}

		public override List<Session> GetSessions()
		{
			return sessions;
		}

		public override bool? VerifySector(ulong sectorAddress)
		{
			byte[] buffer = ReadSectorLong(sectorAddress);
			return Checksums.CDChecksums.CheckCDSector(buffer);
		}

		public override bool? VerifySector(ulong sectorAddress, uint track)
		{
			byte[] buffer = ReadSectorLong(sectorAddress, track);
			return Checksums.CDChecksums.CheckCDSector(buffer);
		}

		public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> FailingLBAs, out List<ulong> UnknownLBAs)
		{
			byte[] buffer = ReadSectorsLong(sectorAddress, length);
			int bps = (int)(buffer.Length / length);
			byte[] sector = new byte[bps];
			FailingLBAs = new List<ulong>();
			UnknownLBAs = new List<ulong>();

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
			byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
			int bps = (int)(buffer.Length / length);
			byte[] sector = new byte[bps];
			FailingLBAs = new List<ulong>();
			UnknownLBAs = new List<ulong>();

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
			return null;
		}	}
}
