// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CloneCD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages CloneCD disc images.
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
using System.Linq;
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.DiscImages
{
	public class CloneCD : ImagePlugin
    {
		#region Parsing regexs
		const string CCD_Identifier = "^\\s*\\[CloneCD\\]";
		const string Disc_Identifier = "^\\s*\\[Disc\\]";
		const string Session_Identifier = "^\\s*\\[Session\\s*(?<number>\\d+)\\]";
		const string Entry_Identifier = "^\\s*\\[Entry\\s*(?<number>\\d+)\\]";
		const string Track_Identifier = "^\\s*\\[TRACK\\s*(?<number>\\d+)\\]";
		const string CDText_Identifier = "^\\s*\\[CDText\\]";
		const string CCD_Version = "^\\s*Version\\s*=\\s*(?<value>\\d+)";
		const string Disc_Entries = "^\\s*TocEntries\\s*=\\s*(?<value>\\d+)";
		const string Disc_Sessions = "^\\s*Sessions\\s*=\\s*(?<value>\\d+)";
		const string Disc_Scrambled = "^\\s*DataTracksScrambled\\s*=\\s*(?<value>\\d+)";
		const string CDText_Length = "^\\s*CDTextLength\\s*=\\s*(?<value>\\d+)";
		const string Disc_Catalog = "^\\s*CATALOG\\s*=\\s*(?<value>\\w+)";
		const string Session_Pregap = "^\\s*PreGapMode\\s*=\\s*(?<value>\\d+)";
		const string Session_Subchannel = "^\\s*PreGapSubC\\s*=\\s*(?<value>\\d+)";
		const string Entry_Session = "^\\s*Session\\s*=\\s*(?<value>\\d+)";
		const string Entry_Point = "^\\s*Point\\s*=\\s*(?<value>[\\w+]+)";
		const string Entry_ADR = "^\\s*ADR\\s*=\\s*(?<value>\\w+)";
		const string Entry_Control = "^\\s*Control\\s*=\\s*(?<value>\\w+)";
		const string Entry_TrackNo = "^\\s*TrackNo\\s*=\\s*(?<value>\\d+)";
		const string Entry_AMin = "^\\s*AMin\\s*=\\s*(?<value>\\d+)";
		const string Entry_ASec = "^\\s*ASec\\s*=\\s*(?<value>\\d+)";
		const string Entry_AFrame = "^\\s*AFrame\\s*=\\s*(?<value>\\d+)";
		const string Entry_ALBA = "^\\s*ALBA\\s*=\\s*(?<value>-?\\d+)";
		const string Entry_Zero = "^\\s*Zero\\s*=\\s*(?<value>\\d+)";
		const string Entry_PMin = "^\\s*PMin\\s*=\\s*(?<value>\\d+)";
		const string Entry_PSec = "^\\s*PSec\\s*=\\s*(?<value>\\d+)";
		const string Entry_PFrame = "^\\s*PFrame\\s*=\\s*(?<value>\\d+)";
		const string Entry_PLBA ="^\\s*PLBA\\s*=\\s*(?<value>\\d+)";
		const string CDText_Entries = "^\\s*Entries\\s*=\\s*(?<value>\\d+)";
		const string CDText_Entry = "^\\s*Entry\\s*(?<number>\\d+)\\s*=\\s*(?<value>([0-9a-fA-F]+\\s*)+)";
		#endregion

		Filter imageFilter;
		Filter dataFilter;
		Filter subFilter;
		StreamReader cueStream;
		byte[] fulltoc;
		bool scrambled;
		string catalog;
		List<ImagePlugins.Session> sessions;
		List<Partition> partitions;
		List<Track> tracks;
		Stream dataStream;
		Stream subStream;
		Dictionary<uint, ulong> offsetmap;
		byte[] cdtext;

		public CloneCD()
		{
			Name = "CloneCD";
			PluginUUID = new Guid("EE9C2975-2E79-427A-8EE9-F86F19165784");
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
			this.imageFilter = imageFilter;

			try
			{
				imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
				byte[] testArray = new byte[512];
				imageFilter.GetDataForkStream().Read(testArray, 0, 512);
				imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
				// Check for unexpected control characters that shouldn't be present in a text file and can crash this plugin
				bool twoConsecutiveNulls = false;
				for(int i = 0; i < 512; i++)
				{
					if(i >= imageFilter.GetDataForkStream().Length)
						break;

					if(testArray[i] == 0)
					{
						if(twoConsecutiveNulls)
							return false;
						twoConsecutiveNulls = true;
					}
					else
						twoConsecutiveNulls = false;

					if(testArray[i] < 0x20 && testArray[i] != 0x0A && testArray[i] != 0x0D && testArray[i] != 0x00)
						return false;
				}

				cueStream = new StreamReader(this.imageFilter.GetDataForkStream());

				string _line = cueStream.ReadLine();

				Regex Hdr = new Regex(CCD_Identifier);

				Match Hdm;

				Hdm = Hdr.Match(_line);

				return Hdm.Success;
			}
			catch(Exception ex)
			{
				DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", this.imageFilter);
				DicConsole.ErrorWriteLine("Exception: {0}", ex.Message);
				DicConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);
				return false;
			}
		}

		public override bool OpenImage(Filter imageFilter)
		{
			if(imageFilter == null)
				return false;

			this.imageFilter = imageFilter;

			try
			{
				imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
				cueStream = new StreamReader(imageFilter.GetDataForkStream());
				int line = 0;

				Regex CCD_IdRegex = new Regex(CCD_Identifier);
				Regex Disc_IdRegex = new Regex(Disc_Identifier);
				Regex Sess_IdRegex = new Regex(Session_Identifier);
				Regex Entry_IdRegex = new Regex(Entry_Identifier);
				Regex Track_IdRegex = new Regex(Track_Identifier);
				Regex CDT_IdRegex = new Regex(CDText_Identifier);
				Regex CCD_VerRegex = new Regex(CCD_Version);
				Regex Disc_EntRegex = new Regex(Disc_Entries);
				Regex Disc_SessRegex = new Regex(Disc_Sessions);
				Regex Disc_ScrRegex = new Regex(Disc_Scrambled);
				Regex CDT_LenRegex = new Regex(CDText_Length);
				Regex Disc_CatRegex = new Regex(Disc_Catalog);
				Regex Sess_PregRegex = new Regex(Session_Pregap);
				Regex Sess_SubcRegex = new Regex(Session_Subchannel);
				Regex Ent_SessRegex = new Regex(Entry_Session);
				Regex Ent_PointRegex = new Regex(Entry_Point);
				Regex Ent_ADRRegex = new Regex(Entry_ADR);
				Regex Ent_CtrlRegex = new Regex(Entry_Control);
				Regex Ent_TNORegex = new Regex(Entry_TrackNo);
				Regex Ent_AMinRegex = new Regex(Entry_AMin);
				Regex Ent_ASecRegex = new Regex(Entry_ASec);
				Regex Ent_AFrameRegex = new Regex(Entry_AFrame);
				Regex Ent_ALBARegex = new Regex(Entry_ALBA);
				Regex Ent_ZeroRegex = new Regex(Entry_Zero);
				Regex Ent_PMinRegex = new Regex(Entry_PMin);
				Regex Ent_PSecRegex = new Regex(Entry_PSec);
				Regex Ent_PFrameRegex = new Regex(Entry_PFrame);
				Regex Ent_PLBARegex = new Regex(Entry_PLBA);
				Regex CDT_EntsRegex = new Regex(CDText_Entries);
				Regex CDT_EntRegex = new Regex(CDText_Entry);

				Match CCD_IdMatch;
				Match Disc_IdMatch;
				Match Sess_IdMatch;
				Match Entry_IdMatch;
				Match Track_IdMatch;
				Match CDT_IdMatch;
				Match CCD_VerMatch;
				Match Disc_EntMatch;
				Match Disc_SessMatch;
				Match Disc_ScrMatch;
				Match CDT_LenMatch;
				Match Disc_CatMatch;
				Match Sess_PregMatch;
				Match Sess_SubcMatch;
				Match Ent_SessMatch;
				Match Ent_PointMatch;
				Match Ent_ADRMatch;
				Match Ent_CtrlMatch;
				Match Ent_TNOMatch;
				Match Ent_AMinMatch;
				Match Ent_ASecMatch;
				Match Ent_AFrameMatch;
				Match Ent_ALBAMatch;
				Match Ent_ZeroMatch;
				Match Ent_PMinMatch;
				Match Ent_PSecMatch;
				Match Ent_PFrameMatch;
				Match Ent_PLBAMatch;
				Match CDT_EntsMatch;
				Match CDT_EntMatch;

				bool inCcd = false;
				bool inDisk = false;
				bool inSession = false;
				bool inEntry = false;
				bool inTrack = false;
				bool inCDText = false;
				MemoryStream cdtMs = new MemoryStream();
				int minSession = int.MaxValue;
				int maxSession = int.MinValue;
				FullTOC.TrackDataDescriptor currentEntry = new FullTOC.TrackDataDescriptor();
				List<FullTOC.TrackDataDescriptor> entries = new List<FullTOC.TrackDataDescriptor>();
				scrambled = false;
				catalog = null;

				while(cueStream.Peek() >= 0)
				{
					line++;
					string _line = cueStream.ReadLine();

					CCD_IdMatch = CCD_IdRegex.Match(_line);
					Disc_IdMatch = Disc_IdRegex.Match(_line);
					Sess_IdMatch = Sess_IdRegex.Match(_line);
					Entry_IdMatch = Entry_IdRegex.Match(_line);
					Track_IdMatch = Track_IdRegex.Match(_line);
					CDT_IdMatch = CDT_IdRegex.Match(_line);

					// [CloneCD]
					if(CCD_IdMatch.Success)
					{
						if(inDisk || inSession || inEntry || inTrack || inCDText)
							throw new FeatureUnsupportedImageException(string.Format("Found [CloneCD] out of order in line {0}", line));

						inCcd = true;
						inDisk = false;
						inSession = false;
						inEntry = false;
						inTrack = false;
						inCDText = false;
					}
					else if(Disc_IdMatch.Success || Sess_IdMatch.Success || Entry_IdMatch.Success || Track_IdMatch.Success || CDT_IdMatch.Success)
					{
						if(inEntry)
						{
							entries.Add(currentEntry);
							currentEntry = new FullTOC.TrackDataDescriptor();
						}

						inCcd = false;
						inDisk = Disc_IdMatch.Success;
						inSession = Sess_IdMatch.Success;
						inEntry = Entry_IdMatch.Success;
						inTrack = Track_IdMatch.Success;
						inCDText = CDT_IdMatch.Success;
					}
					else
					{
						if(inCcd)
						{
							CCD_VerMatch = CCD_VerRegex.Match(_line);

							if(CCD_VerMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found Version at line {0}", line);

								ImageInfo.imageVersion = CCD_VerMatch.Groups["value"].Value;
								if(ImageInfo.imageVersion != "2" && ImageInfo.imageVersion != "3")
									DicConsole.ErrorWriteLine("(CloneCD plugin): Warning! Unknown CCD image version {0}, may not work!", ImageInfo.imageVersion);
							}
						}
						else if(inDisk)
						{
							Disc_EntMatch = Disc_EntRegex.Match(_line);
							Disc_SessMatch = Disc_SessRegex.Match(_line);
							Disc_ScrMatch = Disc_ScrRegex.Match(_line);
							CDT_LenMatch = CDT_LenRegex.Match(_line);
							Disc_CatMatch = Disc_CatRegex.Match(_line);

							if(Disc_EntMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found TocEntries at line {0}", line);
							}
							else if(Disc_SessMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found Sessions at line {0}", line);
							}
							else if(Disc_ScrMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found DataTracksScrambled at line {0}", line);
								scrambled |= Disc_ScrMatch.Groups["value"].Value == "1";
							}
							else if(CDT_LenMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found CDTextLength at line {0}", line);
							}
							else if(Disc_CatMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found Catalog at line {0}", line);
								catalog = Disc_CatMatch.Groups["value"].Value;
							}
						}
						// TODO: Do not suppose here entries come sorted
						else if(inCDText)
						{
							CDT_EntsMatch = CDT_EntsRegex.Match(_line);
							CDT_EntMatch = CDT_EntRegex.Match(_line);

							if(CDT_EntsMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found CD-Text Entries at line {0}", line);
							}
							else if(CDT_EntMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found CD-Text Entry at line {0}", line);
								string[] bytes = CDT_EntMatch.Groups["value"].Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
								foreach(string byt in bytes)
									cdtMs.WriteByte(Convert.ToByte(byt, 16));
							}
						}
						// Is this useful?
						else if(inSession)
						{
							Sess_PregMatch = Sess_PregRegex.Match(_line);
							Sess_SubcMatch = Sess_SubcRegex.Match(_line);

							if(Sess_PregMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found PreGapMode at line {0}", line);
							}
							else if(Sess_SubcMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found PreGapSubC at line {0}", line);
							}
						}
						else if(inEntry)
						{
							Ent_SessMatch = Ent_SessRegex.Match(_line);
							Ent_PointMatch = Ent_PointRegex.Match(_line);
							Ent_ADRMatch = Ent_ADRRegex.Match(_line);
							Ent_CtrlMatch = Ent_CtrlRegex.Match(_line);
							Ent_TNOMatch = Ent_TNORegex.Match(_line);
							Ent_AMinMatch = Ent_AMinRegex.Match(_line);
							Ent_ASecMatch = Ent_ASecRegex.Match(_line);
							Ent_AFrameMatch = Ent_AFrameRegex.Match(_line);
							Ent_ALBAMatch = Ent_ALBARegex.Match(_line);
							Ent_ZeroMatch = Ent_ZeroRegex.Match(_line);
							Ent_PMinMatch = Ent_PMinRegex.Match(_line);
							Ent_PSecMatch = Ent_PSecRegex.Match(_line);
							Ent_PFrameMatch = Ent_PFrameRegex.Match(_line);
							Ent_PLBAMatch = Ent_PLBARegex.Match(_line);

							if(Ent_SessMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found Session at line {0}", line);
								currentEntry.SessionNumber = Convert.ToByte(Ent_SessMatch.Groups["value"].Value, 10);
								if(currentEntry.SessionNumber < minSession)
									minSession = currentEntry.SessionNumber;
								if(currentEntry.SessionNumber > maxSession)
									maxSession = currentEntry.SessionNumber;
							}
							else if(Ent_PointMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found Point at line {0}", line);
								currentEntry.POINT = Convert.ToByte(Ent_PointMatch.Groups["value"].Value, 16);
							}
							else if(Ent_ADRMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found ADR at line {0}", line);
								currentEntry.ADR = Convert.ToByte(Ent_ADRMatch.Groups["value"].Value, 16);
							}
							else if(Ent_CtrlMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found Control at line {0}", line);
								currentEntry.CONTROL = Convert.ToByte(Ent_CtrlMatch.Groups["value"].Value, 16);
							}
							else if(Ent_TNOMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found TrackNo at line {0}", line);
								currentEntry.TNO = Convert.ToByte(Ent_TNOMatch.Groups["value"].Value, 10);
							}
							else if(Ent_AMinMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found AMin at line {0}", line);
								currentEntry.Min = Convert.ToByte(Ent_AMinMatch.Groups["value"].Value, 10);
							}
							else if(Ent_ASecMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found ASec at line {0}", line);
								currentEntry.Sec = Convert.ToByte(Ent_ASecMatch.Groups["value"].Value, 10);
							}
							else if(Ent_AFrameMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found AFrame at line {0}", line);
								currentEntry.Frame = Convert.ToByte(Ent_AFrameMatch.Groups["value"].Value, 10);
							}
							else if(Ent_ALBAMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found ALBA at line {0}", line);
							}
							else if(Ent_ZeroMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found Zero at line {0}", line);
								currentEntry.Zero = Convert.ToByte(Ent_ZeroMatch.Groups["value"].Value, 10);
								currentEntry.HOUR = (byte)((currentEntry.Zero & 0xF0) >> 4);
								currentEntry.PHOUR = (byte)(currentEntry.Zero & 0x0F);
							}
							else if(Ent_PMinMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found PMin at line {0}", line);
								currentEntry.PMIN = Convert.ToByte(Ent_PMinMatch.Groups["value"].Value, 10);
							}
							else if(Ent_PSecMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found PSec at line {0}", line);
								currentEntry.PSEC = Convert.ToByte(Ent_PSecMatch.Groups["value"].Value, 10);
							}
							else if(Ent_PFrameMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found PFrame at line {0}", line);
								currentEntry.PFRAME = Convert.ToByte(Ent_PFrameMatch.Groups["value"].Value, 10);
							}
							else if(Ent_PLBAMatch.Success)
							{
								DicConsole.DebugWriteLine("CloneCD plugin", "Found PLBA at line {0}", line);
							}
						}
					}
				}

				if(inEntry)
					entries.Add(currentEntry);

				if(entries.Count == 0)
					throw new FeatureUnsupportedImageException("Did not find any track.");

				FullTOC.CDFullTOC toc;
				toc.TrackDescriptors = entries.ToArray();
				toc.LastCompleteSession = (byte)maxSession;
				toc.FirstCompleteSession = (byte)minSession;
				toc.DataLength = (ushort)(entries.Count * 11 + 2);
				MemoryStream tocMs = new MemoryStream();
				tocMs.Write(BigEndianBitConverter.GetBytes(toc.DataLength), 0, 2);
				tocMs.WriteByte(toc.FirstCompleteSession);
				tocMs.WriteByte(toc.LastCompleteSession);
				foreach(FullTOC.TrackDataDescriptor descriptor in toc.TrackDescriptors)
				{
					tocMs.WriteByte(descriptor.SessionNumber);
					tocMs.WriteByte((byte)((descriptor.ADR << 4) + descriptor.CONTROL));
					tocMs.WriteByte(descriptor.TNO);
					tocMs.WriteByte(descriptor.POINT);
					tocMs.WriteByte(descriptor.Min);
					tocMs.WriteByte(descriptor.Sec);
					tocMs.WriteByte(descriptor.Frame);
					tocMs.WriteByte(descriptor.Zero);
					tocMs.WriteByte(descriptor.PMIN);
					tocMs.WriteByte(descriptor.PSEC);
					tocMs.WriteByte(descriptor.PFRAME);
				}
				fulltoc = tocMs.ToArray();
				ImageInfo.readableMediaTags.Add(MediaTagType.CD_FullTOC);

				DicConsole.DebugWriteLine("CloneCD plugin", "{0}", FullTOC.Prettify(toc));

				string dataFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".img";
				string subFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".sub";

				FiltersList filtersList = new FiltersList();
				dataFilter = filtersList.GetFilter(dataFile);

				if(dataFilter == null)
					throw new Exception("Cannot open data file");

				filtersList = new FiltersList();
				subFilter = filtersList.GetFilter(subFile);

				int curSessionNo = 0;
				Track currentTrack = new Track();
				bool firstTrackInSession = true;
				tracks = new List<Track>();
				byte discType;
				ulong LeadOutStart = 0;

				dataStream = dataFilter.GetDataForkStream();
				if(subFilter != null)
					subStream = subFilter.GetDataForkStream();

				foreach(FullTOC.TrackDataDescriptor descriptor in entries)
				{
					if(descriptor.SessionNumber > curSessionNo)
					{
						curSessionNo = descriptor.SessionNumber;
						if(!firstTrackInSession)
						{
							currentTrack.TrackEndSector = LeadOutStart - 1;
							tracks.Add(currentTrack);
						}
						firstTrackInSession = true;
					}

					switch(descriptor.ADR)
					{
						case 1:
						case 4:
							switch(descriptor.POINT)
							{
								case 0xA0:
									discType = descriptor.PSEC;
									DicConsole.DebugWriteLine("CloneCD plugin", "Disc Type: {0}", discType);
									break;
								case 0xA2:
									LeadOutStart = GetLBA(descriptor.PHOUR, descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME);
									break;
								default:
									if(descriptor.POINT >= 0x01 && descriptor.POINT <= 0x63)
									{
										if(!firstTrackInSession)
										{
											currentTrack.TrackEndSector = GetLBA(descriptor.PHOUR, descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME) - 1;
											tracks.Add(currentTrack);
										}
										else
											firstTrackInSession = false;

										currentTrack = new Track();
										currentTrack.TrackBytesPerSector = 2352;
										currentTrack.TrackFile = dataFilter.GetFilename();
										currentTrack.TrackFileType = scrambled ? "SCRAMBLED" : "BINARY";
										currentTrack.TrackFilter = dataFilter;
										currentTrack.TrackRawBytesPerSector = 2352;
										currentTrack.TrackSequence = descriptor.POINT;
										currentTrack.TrackStartSector = GetLBA(descriptor.PHOUR, descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME);
										currentTrack.TrackFileOffset = currentTrack.TrackStartSector * 2352;
										currentTrack.TrackSession = descriptor.SessionNumber;

										// Need to check exact data type later
										if((TOC_CONTROL)(descriptor.CONTROL & 0x0D) == TOC_CONTROL.DataTrack ||
													   (TOC_CONTROL)(descriptor.CONTROL & 0x0D) == TOC_CONTROL.DataTrackIncremental)
											currentTrack.TrackType = TrackType.Data;
										else
											currentTrack.TrackType = TrackType.Audio;

										if(subFilter != null)
										{
											currentTrack.TrackSubchannelFile = subFilter.GetFilename();
											currentTrack.TrackSubchannelFilter = subFilter;
											currentTrack.TrackSubchannelOffset = currentTrack.TrackStartSector * 96;
											currentTrack.TrackSubchannelType = TrackSubchannelType.Raw;
										}
										else
											currentTrack.TrackSubchannelType = TrackSubchannelType.None;

										if(currentTrack.TrackType == TrackType.Data)
										{
											byte[] syncTest = new byte[12];
											byte[] sectTest = new byte[2352];
											dataStream.Seek((long)currentTrack.TrackFileOffset, SeekOrigin.Begin);
											dataStream.Read(sectTest, 0, 2352);
											Array.Copy(sectTest, 0, syncTest, 0, 12);

											if(Sector.SyncMark.SequenceEqual(syncTest))
											{
												if(scrambled)
													sectTest = Sector.Scramble(sectTest);

												if(sectTest[15] == 1)
												{
													currentTrack.TrackBytesPerSector = 2048;
													currentTrack.TrackType = TrackType.CDMode1;
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
													if(ImageInfo.sectorSize < 2048)
														ImageInfo.sectorSize = 2048;
												}
												else if(sectTest[15] == 2)
												{
													byte[] subHdr1 = new byte[4];
													byte[] subHdr2 = new byte[4];
													byte[] empHdr = new byte[4];

													Array.Copy(sectTest, 16, subHdr1, 0, 4);
													Array.Copy(sectTest, 20, subHdr2, 0, 4);

													if(subHdr1.SequenceEqual(subHdr2) && !empHdr.SequenceEqual(subHdr1))
													{
														if((subHdr1[2] & 0x20) == 0x20)
														{
															currentTrack.TrackBytesPerSector = 2324;
															currentTrack.TrackType = TrackType.CDMode2Form2;
															if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
																ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
															if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
																ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
															if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubHeader))
																ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubHeader);
															if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
																ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
															if(ImageInfo.sectorSize < 2324)
																ImageInfo.sectorSize = 2324;
														}
														else
														{
															currentTrack.TrackBytesPerSector = 2048;
															currentTrack.TrackType = TrackType.CDMode2Form1;
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
															if(ImageInfo.sectorSize < 2048)
																ImageInfo.sectorSize = 2048;
														}
													}
													else
													{
														currentTrack.TrackBytesPerSector = 2336;
														currentTrack.TrackType = TrackType.CDMode2Formless;
														if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
															ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
														if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
															ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
														if(ImageInfo.sectorSize < 2336)
															ImageInfo.sectorSize = 2336;
													}
												}
											}
										}
										else
										{
											if(ImageInfo.sectorSize < 2352)
												ImageInfo.sectorSize = 2352;
										}
									}
									break;
							}
							break;
						case 5:
							switch(descriptor.POINT)
							{
								case 0xC0:
									if(descriptor.PMIN == 97)
									{
										int type = descriptor.PFRAME % 10;
										int frm = descriptor.PFRAME - type;

										ImageInfo.mediaManufacturer = ATIP.ManufacturerFromATIP(descriptor.PSEC, frm);

										if(ImageInfo.mediaManufacturer != "")
											DicConsole.DebugWriteLine("CloneCD plugin", "Disc manufactured by: {0}", ImageInfo.mediaManufacturer);
									}
									break;
							}
							break;
						case 6:
							{
								uint id = (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame);
								DicConsole.DebugWriteLine("CloneCD plugin", "Disc ID: {0:X6}", id & 0x00FFFFFF);
								ImageInfo.mediaSerialNumber = string.Format("{0:X6}", id & 0x00FFFFFF);
								break;
							}
					}
				}
				if(!firstTrackInSession)
				{
					currentTrack.TrackEndSector = LeadOutStart - 1;
					tracks.Add(currentTrack);
				}

				if(subFilter != null && !ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubchannel))
						ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubchannel);

				sessions = new List<ImagePlugins.Session>();
				ImagePlugins.Session currentSession = new ImagePlugins.Session();
				currentSession.EndTrack = uint.MinValue;
				currentSession.StartTrack = uint.MaxValue;
				currentSession.SessionSequence = 1;
				partitions = new List<Partition>();
				offsetmap = new Dictionary<uint, ulong>();

				foreach(Track track in tracks)
				{
					if(track.TrackSession == currentSession.SessionSequence)
					{
						if(track.TrackSequence > currentSession.EndTrack)
						{
							currentSession.EndSector = track.TrackEndSector;
							currentSession.EndTrack = track.TrackSequence;
						}

						if(track.TrackSequence < currentSession.StartTrack)
						{
							currentSession.StartSector = track.TrackStartSector;
							currentSession.StartTrack = track.TrackSequence;
						}
					}
					else
					{
						sessions.Add(currentSession);
						currentSession = new ImagePlugins.Session();
						currentSession.EndTrack = uint.MinValue;
						currentSession.StartTrack = uint.MaxValue;
						currentSession.SessionSequence = track.TrackSession;
					}

					Partition partition = new Partition();
					partition.Description = track.TrackDescription;
					partition.Size = ((track.TrackEndSector - track.TrackStartSector) + 1) * (ulong)track.TrackRawBytesPerSector;
					partition.Length = (track.TrackEndSector - track.TrackStartSector) + 1;
					ImageInfo.sectors += partition.Length;
					partition.Sequence = track.TrackSequence;
					partition.Offset = track.TrackFileOffset;
					partition.Start = track.TrackStartSector;
					partition.Type = track.TrackType.ToString();
					partitions.Add(partition);
					offsetmap.Add(track.TrackSequence, track.TrackStartSector);
				}

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

				// TODO: Check format
				cdtext = cdtMs.ToArray();

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

				ImageInfo.imageApplication = "CloneCD";
				ImageInfo.imageSize = (ulong)imageFilter.GetDataForkLength();
				ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
				ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();
				ImageInfo.xmlMediaType = XmlMediaType.OpticalDisc;

				return true;
			}
			catch(Exception ex)
			{
				DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter.GetFilename());
				DicConsole.ErrorWriteLine("Exception: {0}", ex.Message);
				DicConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);
				return false;
			}
		}

		static ulong GetLBA(int hour, int minute, int second, int frame)
		{
			return (ulong)((hour * 60 * 60 * 75) + (minute * 60 * 75) + (second * 75) + frame - 150);
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
				case MediaTagType.CD_FullTOC:
					{
						return fulltoc;
					}
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
							if(sectorAddress <= _track.TrackEndSector)
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
							if(sectorAddress <= _track.TrackEndSector)
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

			if((length + sectorAddress) - 1 > _track.TrackEndSector)
				throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0} {2}) than present in track ({1}), won't cross tracks", length + sectorAddress, _track.TrackEndSector, sectorAddress));

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
					{
						sector_offset = 16;
						sector_size = 2048;
						sector_skip = 288;
						break;
					}
				case TrackType.CDMode2Formless:
					{
						sector_offset = 16;
						sector_size = 2336;
						sector_skip = 0;
						break;
					}
				case TrackType.CDMode2Form1:
					{
						sector_offset = 24;
						sector_size = 2048;
						sector_skip = 280;
						break;
					}
				case TrackType.CDMode2Form2:
					{
						sector_offset = 24;
						sector_size = 2324;
						sector_skip = 4;
						break;
					}
				default:
					throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
			}

			byte[] buffer = new byte[sector_size * length];

			dataStream.Seek((long)(_track.TrackFileOffset + sectorAddress * 2352), SeekOrigin.Begin);
			if(sector_offset == 0 && sector_skip == 0)
				dataStream.Read(buffer, 0, buffer.Length);
			else
			{
				for(int i = 0; i < length; i++)
				{
					byte[] sector = new byte[sector_size];
					dataStream.Seek(sector_offset, SeekOrigin.Current);
					dataStream.Read(sector, 0, sector.Length);
					dataStream.Seek(sector_skip, SeekOrigin.Current);
					Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
				}
			}

			return buffer;
		}

		// TODO: Flags
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

			if((length + sectorAddress) - 1 > (_track.TrackEndSector))
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
				case SectorTagType.CDSectorSubHeader:
				case SectorTagType.CDSectorSync:
					break;
				case SectorTagType.CDSectorSubchannel:
					buffer = new byte[96 * length];
					subStream.Seek((long)(_track.TrackSubchannelOffset + sectorAddress * 96), SeekOrigin.Begin);
					subStream.Read(buffer, 0, buffer.Length);
					return buffer;
				default:
					throw new ArgumentException("Unsupported tag requested", nameof(tag));
			}

			uint sector_offset;
			uint sector_size;
			uint sector_skip;

			switch(_track.TrackType)
			{
				case TrackType.CDMode1:
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
						default:
							throw new ArgumentException("Unsupported tag requested", nameof(tag));
					}
					break;
				case TrackType.CDMode2Formless:
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
							default:
								throw new ArgumentException("Unsupported tag requested", nameof(tag));
						}
						break;
					}
				case TrackType.CDMode2Form1:
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
							{
								sector_offset = 16;
								sector_size = 8;
								sector_skip = 2328;
								break;
							}
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
								sector_offset = 2072;
								sector_size = 4;
								sector_skip = 276;
								break;
							}
						default:
							throw new ArgumentException("Unsupported tag requested", nameof(tag));
					}
					break;
				case TrackType.CDMode2Form2:
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
							{
								sector_offset = 16;
								sector_size = 8;
								sector_skip = 2328;
								break;
							}
						case SectorTagType.CDSectorEDC:
							{
								sector_offset = 2348;
								sector_size = 4;
								sector_skip = 0;
								break;
							}
						default:
							throw new ArgumentException("Unsupported tag requested", nameof(tag));
					}
					break;
				case TrackType.Audio:
					{
						throw new ArgumentException("Unsupported tag requested", nameof(tag));
					}
				default:
					throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
			}

			buffer = new byte[sector_size * length];

			dataStream.Seek((long)(_track.TrackFileOffset + sectorAddress * 2352), SeekOrigin.Begin);
			if(sector_offset == 0 && sector_skip == 0)
				dataStream.Read(buffer, 0, buffer.Length);
			else
			{
				for(int i = 0; i < length; i++)
				{
					byte[] sector = new byte[sector_size];
					dataStream.Seek(sector_offset, SeekOrigin.Current);
					dataStream.Read(sector, 0, sector.Length);
					dataStream.Seek(sector_skip, SeekOrigin.Current);
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
							if((sectorAddress - kvp.Value) < ((track.TrackEndSector - track.TrackStartSector) + 1))
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

			if((length + sectorAddress) - 1 > (_track.TrackEndSector))
				throw new ArgumentOutOfRangeException(nameof(length), string.Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks", length + sectorAddress, _track.TrackEndSector));

			byte[] buffer = new byte[2352 * length];

			dataStream.Seek((long)(_track.TrackFileOffset + sectorAddress * 2352), SeekOrigin.Begin);
			dataStream.Read(buffer, 0, buffer.Length);

			return buffer;
		}

		public override string GetImageFormat()
		{
			return "CloneCD";
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

		public override List<Track> GetSessionTracks(ImagePlugins.Session session)
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

		public override List<ImagePlugins.Session> GetSessions()
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
		}
	}
}
