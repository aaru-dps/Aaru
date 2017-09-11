// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CDRDAO.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages cdrdao cuesheets (toc/bin).
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
using System.Text.RegularExpressions;
using DiscImageChef.Console;
using DiscImageChef.CommonTypes;
using System.Text;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    // TODO: Doesn't support compositing from several files
    // TODO: Doesn't support silences that are not in files
    public class CDRDAO : ImagePlugin
    {
        #region Internal structures

        struct CDRDAOTrackFile
        {
            /// <summary>Track #</summary>
            public uint sequence;
            /// <summary>Filter of file containing track</summary>
            public Filter datafilter;
            /// <summary>Path of file containing track</summary>
            public string datafile;
            /// <summary>Offset of track start in file</summary>
            public ulong offset;
            /// <summary>Type of file</summary>
            public string filetype;
        }

        struct CDRDAOTrack
        {
            /// <summary>Track #</summary>
            public uint sequence;
            /// <summary>Track title (from CD-Text)</summary>
            public string title;
            /// <summary>Track genre (from CD-Text)</summary>
            public string genre;
            /// <summary>Track arranger (from CD-Text)</summary>
            public string arranger;
            /// <summary>Track composer (from CD-Text)</summary>
            public string composer;
            /// <summary>Track performer (from CD-Text)</summary>
            public string performer;
            /// <summary>Track song writer (from CD-Text)</summary>
            public string songwriter;
            /// <summary>Track ISRC</summary>
            public string isrc;
            /// <summary>Disk provider's message (from CD-Text)</summary>
            public string message;
            /// <summary>File struct for this track</summary>
            public CDRDAOTrackFile trackfile;
            /// <summary>Indexes on this track</summary>
            public Dictionary<int, ulong> indexes;
            /// <summary>Track pre-gap in sectors</summary>
            public ulong pregap;
            /// <summary>Track post-gap in sectors</summary>
            public ulong postgap;
            /// <summary>Digical Copy Permitted</summary>
            public bool flag_dcp;
            /// <summary>Track is quadraphonic</summary>
            public bool flag_4ch;
            /// <summary>Track has preemphasis</summary>
            public bool flag_pre;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors in track</summary>
            public ulong sectors;
            /// <summary>Starting sector in track</summary>
            public ulong startSector;
            /// <summary>Track type</summary>
            public string tracktype;
            public bool subchannel;
            public bool packedsubchannel;
        }

        struct CDRDAODisc
        {
            /// <summary>Disk title (from CD-Text)</summary>
            public string title;
            /// <summary>Disk genre (from CD-Text)</summary>
            public string genre;
            /// <summary>Disk arranger (from CD-Text)</summary>
            public string arranger;
            /// <summary>Disk composer (from CD-Text)</summary>
            public string composer;
            /// <summary>Disk performer (from CD-Text)</summary>
            public string performer;
            /// <summary>Disk song writer (from CD-Text)</summary>
            public string songwriter;
            /// <summary>Disk provider's message (from CD-Text)</summary>
            public string message;
            /// <summary>Media catalog number</summary>
            public string mcn;
            /// <summary>Disk type</summary>
            public MediaType disktype;
            /// <summary>Disk type string</summary>
            public string disktypestr;
            /// <summary>Disk CDDB ID</summary>
            public string disk_id;
            /// <summary>Disk UPC/EAN</summary>
            public string barcode;
            /// <summary>Tracks</summary>
            public List<CDRDAOTrack> tracks;
            /// <summary>Disk comment</summary>
            public string comment;
        }

        #endregion Internal structures

        #region Internal consts

        /// <summary>Audio track, 2352 bytes/sector</summary>
        const string CDRDAOTrackTypeAudio = "AUDIO";
        /// <summary>Mode 1 track, cooked, 2048 bytes/sector</summary>
        const string CDRDAOTrackTypeMode1 = "MODE1";
        /// <summary>Mode 1 track, raw, 2352 bytes/sector</summary>
        const string CDRDAOTrackTypeMode1Raw = "MODE1_RAW";
        /// <summary>Mode 2 mixed formless, cooked, 2336 bytes/sector</summary>
        const string CDRDAOTrackTypeMode2 = "MODE2";
        /// <summary>Mode 2 form 1 track, cooked, 2048 bytes/sector</summary>
        const string CDRDAOTrackTypeMode2Form1 = "MODE2_FORM1";
        /// <summary>Mode 2 form 2 track, cooked, 2324 bytes/sector</summary>
        const string CDRDAOTrackTypeMode2Form2 = "MODE2_FORM2";
        /// <summary>Mode 2 mixed forms track, cooked, 2336 bytes/sector</summary>
        const string CDRDAOTrackTypeMode2Mix = "MODE2_FORM_MIX";
        /// <summary>Mode 2 track, raw, 2352 bytes/sector</summary>
        const string CDRDAOTrackTypeMode2Raw = "MODE2_RAW";

        #endregion Internal consts

        #region Internal variables

        Filter imageFilter;
        StreamReader tocStream;
        Stream imageStream;
        /// <summary>Dictionary, index is track #, value is TrackFile</summary>
        Dictionary<uint, ulong> offsetmap;
        List<Partition> partitions;
        CDRDAODisc discimage;

        #endregion

        #region Parsing regexs

        const string CommentRegEx = "^\\s*\\/{2}(?<comment>.+)$";
        const string DiskTypeRegEx = "^\\s*(?<type>(CD_DA|CD_ROM_XA|CD_ROM|CD_I))";
        const string MCNRegEx = "^\\s*CATALOG\\s*\"(?<catalog>[\\d]{13,13})\"";
        const string TrackRegEx = "^\\s*TRACK\\s*(?<type>(AUDIO|MODE1_RAW|MODE1|MODE2_FORM1|MODE2_FORM2|MODE2_FORM_MIX|MODE2_RAW|MODE2))\\s*(?<subchan>(RW_RAW|RW))?";
        const string CopyRegEx = "^\\s*(?<no>NO)?\\s*COPY";
        const string EmphasisRegEx = "^\\s*(?<no>NO)?\\s*PRE_EMPHASIS";
        const string StereoRegEx = "^\\s*(?<num>(TWO|FOUR))_CHANNEL_AUDIO";
        const string ISRCRegEx = "^\\s*ISRC\\s*\"(?<isrc>[A-Z0-9]{5,5}[0-9]{7,7})\"";
        const string IndexRegEx = "^\\s*INDEX\\s*(?<address>\\d+:\\d+:\\d+)";
        const string PregapRegEx = "^\\s*START\\s*(?<address>\\d+:\\d+:\\d+)?";
        const string ZeroPregapRegEx = "^\\s*PREGAP\\s*(?<length>\\d+:\\d+:\\d+)";
        const string ZeroDataRegEx = "^\\s*ZERO\\s*(?<length>\\d+:\\d+:\\d+)";
        const string ZeroAudioRegEx = "^\\s*SILENCE\\s*(?<length>\\d+:\\d+:\\d+)";
        const string AudioFileRegEx = "^\\s*(AUDIO)?FILE\\s*\"(?<filename>.+)\"\\s*(#(?<base_offset>\\d+))?\\s*((?<start>[\\d]+:[\\d]+:[\\d]+)|(?<start_num>\\d+))\\s*(?<length>[\\d]+:[\\d]+:[\\d]+)?";
        const string FileRegEx = "^\\s*DATAFILE\\s*\"(?<filename>.+)\"\\s*(#(?<base_offset>\\d+))?\\s*(?<length>[\\d]+:[\\d]+:[\\d]+)?";

        // CD-Text
        const string TitleRegEx = "^\\s*TITLE\\s*\"(?<title>.+)\"";
        const string PerformerRegEx = "^\\s*PERFORMER\\s*\"(?<performer>.+)\"";
        const string SongwriterRegEx = "^\\s*SONGWRITER\\s*\"(?<songwriter>.+)\"";
        const string ComposerRegEx = "^\\s*COMPOSER\\s*\"(?<composer>.+)\"";
        const string ArrangerRegEx = "^\\s*ARRANGER\\s*\"(?<arranger>.+)\"";
        const string MessageRegEx = "^\\s*MESSAGE\\s*\"(?<message>.+)\"";
        const string DiscIDRegEx = "^\\s*DISC_ID\\s*\"(?<discid>.+)\"";
        const string UPCRegEx = "^\\s*UPC_EAN\\s*\"(?<catalog>[\\d]{13,13})\"";

        // Unused
        const string CDTextRegEx = "^\\s*CD_TEXT\\s*\\{";
        const string LanguageRegEx = "^\\s*LANGUAGE\\s*(?<code>\\d+)\\s*\\{";
        const string ClosureRegEx = "^\\s*\\}";
        const string LanguageMapRegEx = "^\\s*LANGUAGE_MAP\\s*\\{";
        const string LanguageMappingRegEx = "^\\s*(?<code>\\d+)\\s?\\:\\s?(?<language>\\d+|\\w+)";

        #endregion

        #region Public methods

        public CDRDAO()
        {
            Name = "CDRDAO tocfile";
            PluginUUID = new Guid("04D7BA12-1BE8-44D4-97A4-1B48A505463E");
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

        #endregion Public methods

        public override bool IdentifyImage(Filter imageFilter)
        {
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

				tocStream = new StreamReader(imageFilter.GetDataForkStream());
				string _line;

				Regex Cr = new Regex(CommentRegEx);
				Regex Dr = new Regex(DiskTypeRegEx);
				Match Dm;
				Match Cm;

				while(tocStream.Peek() >= 0)
				{
					_line = tocStream.ReadLine();

					Dm = Dr.Match(_line);
					Cm = Cr.Match(_line);

					// Skip comments at start of file
					if(Cm.Success)
						continue;

					return Dm.Success;
				}

				return false;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", this.imageFilter.GetFilename());
                DicConsole.ErrorWriteLine("Exception: {0}", ex.Message);
                DicConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);
                return false;
            }
        }

        public override bool OpenImage(Filter imageFilter)
        {
            if(imageFilter == null)
                return false;
            if(imageFilter == null)
                return false;

            this.imageFilter = imageFilter;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                tocStream = new StreamReader(imageFilter.GetDataForkStream());
                int line = 0;
                bool intrack = false;

                // Initialize all RegExs
                Regex RegexComment = new Regex(CommentRegEx);
                Regex RegexDiskType = new Regex(DiskTypeRegEx);
                Regex RegexMCN = new Regex(MCNRegEx);
                Regex RegexTrack = new Regex(TrackRegEx);
                Regex RegexCopy = new Regex(CopyRegEx);
                Regex RegexEmphasis = new Regex(EmphasisRegEx);
                Regex RegexStereo = new Regex(StereoRegEx);
                Regex RegexISRC = new Regex(ISRCRegEx);
                Regex RegexIndex = new Regex(IndexRegEx);
                Regex RegexPregap = new Regex(PregapRegEx);
                Regex RegexZeroPregap = new Regex(ZeroPregapRegEx);
                Regex RegexZeroData = new Regex(ZeroDataRegEx);
                Regex RegexZeroAudio = new Regex(ZeroAudioRegEx);
                Regex RegexAudioFile = new Regex(AudioFileRegEx);
                Regex RegexFile = new Regex(FileRegEx);
                Regex RegexTitle = new Regex(TitleRegEx);
                Regex RegexPerformer = new Regex(PerformerRegEx);
                Regex RegexSongwriter = new Regex(SongwriterRegEx);
                Regex RegexComposer = new Regex(ComposerRegEx);
                Regex RegexArranger = new Regex(ArrangerRegEx);
                Regex RegexMessage = new Regex(MessageRegEx);
                Regex RegexDiscID = new Regex(DiscIDRegEx);
                Regex RegexUPC = new Regex(UPCRegEx);
                Regex RegexCDText = new Regex(CDTextRegEx);
                Regex RegexLanguage = new Regex(LanguageRegEx);
                Regex RegexClosure = new Regex(ClosureRegEx);
                Regex RegexLanguageMap = new Regex(LanguageMapRegEx);
                Regex RegexLanguageMapping = new Regex(LanguageMappingRegEx);

                // Initialize all RegEx matches
                Match MatchComment;
                Match MatchDiskType;
                Match MatchMCN;
                Match MatchTrack;
                Match MatchCopy;
                Match MatchEmphasis;
                Match MatchStereo;
                Match MatchISRC;
                Match MatchIndex;
                Match MatchPregap;
                Match MatchZeroPregap;
                Match MatchZeroData;
                Match MatchZeroAudio;
                Match MatchAudioFile;
                Match MatchFile;
                Match MatchTitle;
                Match MatchPerformer;
                Match MatchSongwriter;
                Match MatchComposer;
                Match MatchArranger;
                Match MatchMessage;
                Match MatchDiscID;
                Match MatchUPC;
                Match MatchCDText;
                Match MatchLanguage;
                Match MatchClosure;
                Match MatchLanguageMap;
                Match MatchLanguageMapping;

                // Initialize disc
                discimage = new CDRDAODisc();
                discimage.tracks = new List<CDRDAOTrack>();
                discimage.comment = "";

                CDRDAOTrack currenttrack = new CDRDAOTrack();
                uint currentTrackNumber = 0;
                currenttrack.indexes = new Dictionary<int, ulong>();
                currenttrack.pregap = 0;
                ulong currentSector = 0;
                int nextindex = 2;
                StringBuilder commentBuilder = new StringBuilder();

                tocStream = new StreamReader(this.imageFilter.GetDataForkStream());
				string _line;

				while(tocStream.Peek() >= 0)
				{
					line++;
					_line = tocStream.ReadLine();

					MatchDiskType = RegexDiskType.Match(_line);
					MatchComment = RegexComment.Match(_line);

					// Skip comments at start of file
					if(MatchComment.Success)
						continue;

					if(!MatchDiskType.Success)
					{
						DicConsole.DebugWriteLine("CDRDAO plugin", "Not a CDRDAO TOC or TOC type not in line {0}.", line);
						return false;
					}

					break;
				}

                tocStream = new StreamReader(this.imageFilter.GetDataForkStream());
                FiltersList filtersList = new FiltersList();
				line = 0;

				tocStream.BaseStream.Position = 0;
				while(tocStream.Peek() >= 0)
                {
                    line++;
                    _line = tocStream.ReadLine();

                    MatchComment = RegexComment.Match(_line);
                    MatchDiskType = RegexDiskType.Match(_line);
                    MatchMCN = RegexMCN.Match(_line);
                    MatchTrack = RegexTrack.Match(_line);
                    MatchCopy = RegexCopy.Match(_line);
                    MatchEmphasis = RegexEmphasis.Match(_line);
                    MatchStereo = RegexStereo.Match(_line);
                    MatchISRC = RegexISRC.Match(_line);
                    MatchIndex = RegexIndex.Match(_line);
                    MatchPregap = RegexPregap.Match(_line);
                    MatchZeroPregap = RegexZeroPregap.Match(_line);
                    MatchZeroData = RegexZeroData.Match(_line);
                    MatchZeroAudio = RegexZeroAudio.Match(_line);
                    MatchAudioFile = RegexAudioFile.Match(_line);
                    MatchFile = RegexFile.Match(_line);
                    MatchTitle = RegexTitle.Match(_line);
                    MatchPerformer = RegexPerformer.Match(_line);
                    MatchSongwriter = RegexSongwriter.Match(_line);
                    MatchComposer = RegexComposer.Match(_line);
                    MatchArranger = RegexArranger.Match(_line);
                    MatchMessage = RegexMessage.Match(_line);
                    MatchDiscID = RegexDiscID.Match(_line);
                    MatchUPC = RegexUPC.Match(_line);
                    MatchCDText = RegexCDText.Match(_line);
                    MatchLanguage = RegexLanguage.Match(_line);
                    MatchClosure = RegexClosure.Match(_line);
                    MatchLanguageMap = RegexLanguageMap.Match(_line);
                    MatchLanguageMapping = RegexLanguageMapping.Match(_line);

                    if(MatchComment.Success)
                    {
                        // Ignore "// Track X" comments
                        if(!MatchComment.Groups["comment"].Value.StartsWith(" Track ", StringComparison.Ordinal))
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found comment \"{1}\" at line {0}", line, MatchComment.Groups["comment"].Value.Trim());
                            commentBuilder.AppendLine(MatchComment.Groups["comment"].Value.Trim());
                        }
                    }
                    else if(MatchDiskType.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found {1} at line {0}", line, MatchDiskType.Groups["type"].Value);
                        discimage.disktypestr = MatchDiskType.Groups["type"].Value;
                        switch(MatchDiskType.Groups["type"].Value)
                        {
                            case "CD_DA":
                                discimage.disktype = MediaType.CDDA;
                                break;
                            case "CD_ROM":
                                discimage.disktype = MediaType.CDROM;
                                break;
                            case "CD_ROM_XA":
                                discimage.disktype = MediaType.CDROMXA;
                                break;
                            case "CD_I":
                                discimage.disktype = MediaType.CDI;
                                break;
                            default:
                                discimage.disktype = MediaType.CD;
                                break;
                        }
                    }
                    else if(MatchMCN.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found CATALOG \"{1}\" at line {0}", line, MatchMCN.Groups["catalog"].Value);
                        discimage.mcn = MatchMCN.Groups["catalog"].Value;
                    }
                    else if(MatchTrack.Success)
                    {
                        if(MatchTrack.Groups["subchan"].Value == "")
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found TRACK type \"{1}\" with no subchannel at line {0}", line, MatchTrack.Groups["type"].Value);
                        else
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found TRACK type \"{1}\" subchannel {2} at line {0}", line, MatchTrack.Groups["type"].Value, MatchTrack.Groups["subchan"].Value);

                        if(intrack)
                        {
                            currentSector += currenttrack.sectors;
                            if(currenttrack.pregap != currenttrack.sectors && !currenttrack.indexes.ContainsKey(1))
                                currenttrack.indexes.Add(1, currenttrack.startSector + currenttrack.pregap);
                            discimage.tracks.Add(currenttrack);
                            currenttrack = new CDRDAOTrack();
                            currenttrack.indexes = new Dictionary<int, ulong>();
                            currenttrack.pregap = 0;
                            nextindex = 2;
                        }
                        currentTrackNumber++;
                        intrack = true;

                        switch(MatchTrack.Groups["type"].Value)
                        {
                            case "AUDIO":
                            case "MODE1_RAW":
                            case "MODE2_RAW":
                                currenttrack.bps = 2352;
                                break;
                            case "MODE1":
                            case "MODE2_FORM1":
                                currenttrack.bps = 2048;
                                break;
                            case "MODE2_FORM2":
                                currenttrack.bps = 2324;
                                break;
                            case "MODE2":
                            case "MODE2_FORM_MIX":
                                currenttrack.bps = 2336;
                                break;
                            default:
                                throw new NotSupportedException(string.Format("Track mode {0} is unsupported", MatchTrack.Groups["type"].Value));
                        }

                        switch(MatchTrack.Groups["subchan"].Value)
                        {
                            case "":
                                break;
                            case "RW":
                                currenttrack.packedsubchannel = true;
                                goto case "RW_RAW";
                            case "RW_RAW":
                                currenttrack.bps += 96;
                                currenttrack.subchannel = true;
                                break;
                            default:
                                throw new NotSupportedException(string.Format("Track subchannel mode {0} is unsupported", MatchTrack.Groups["subchan"].Value));
                        }

                        currenttrack.tracktype = MatchTrack.Groups["type"].Value;

                        currenttrack.sequence = currentTrackNumber;
                        currenttrack.startSector = currentSector;
                    }
                    else if(MatchCopy.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found {1} COPY at line {0}", line, MatchCopy.Groups["no"].Value);
                        currenttrack.flag_dcp |= intrack && MatchCopy.Groups["no"].Value == "";
                    }
                    else if(MatchEmphasis.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found {1} PRE_EMPHASIS at line {0}", line, MatchEmphasis.Groups["no"].Value);
                        currenttrack.flag_pre |= intrack && MatchCopy.Groups["no"].Value == "";
                    }
                    else if(MatchStereo.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found {1}_CHANNEL_AUDIO at line {0}", line, MatchStereo.Groups["num"].Value);
                        currenttrack.flag_4ch |= intrack && MatchCopy.Groups["num"].Value == "FOUR";
                    }
                    else if(MatchISRC.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found ISRC \"{1}\" at line {0}", line, MatchISRC.Groups["isrc"].Value);
                        if(intrack)
                            currenttrack.isrc = MatchISRC.Groups["isrc"].Value;
                    }
                    else if(MatchIndex.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found INDEX \"{1}\" at line {0}", line, MatchIndex.Groups["address"].Value);

                        string[] lengthString = MatchFile.Groups["length"].Value.Split(new char[] { ':' });
                        ulong nextIndexPos = ulong.Parse(lengthString[0]) * 60 * 75 + ulong.Parse(lengthString[1]) * 75 + ulong.Parse(lengthString[2]);
                        currenttrack.indexes.Add(nextindex, nextIndexPos + currenttrack.pregap + currenttrack.startSector);
                    }
                    else if(MatchPregap.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found START \"{1}\" at line {0}", line, MatchPregap.Groups["address"].Value);

                        currenttrack.indexes.Add(0, currenttrack.startSector);
                        if(MatchPregap.Groups["address"].Value != "")
                        {
                            string[] lengthString = MatchPregap.Groups["address"].Value.Split(new char[] { ':' });
                            currenttrack.pregap = ulong.Parse(lengthString[0]) * 60 * 75 + ulong.Parse(lengthString[1]) * 75 + ulong.Parse(lengthString[2]);
                        }
                        else
                            currenttrack.pregap = currenttrack.sectors;
                    }
                    else if(MatchZeroPregap.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found PREGAP \"{1}\" at line {0}", line, MatchZeroPregap.Groups["length"].Value);
                        currenttrack.indexes.Add(0, currenttrack.startSector);
                        string[] lengthString = MatchZeroPregap.Groups["length"].Value.Split(new char[] { ':' });
                        currenttrack.pregap = ulong.Parse(lengthString[0]) * 60 * 75 + ulong.Parse(lengthString[1]) * 75 + ulong.Parse(lengthString[2]);
                    }
                    else if(MatchZeroData.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found ZERO \"{1}\" at line {0}", line, MatchZeroData.Groups["length"].Value);
                        // Seems can be ignored as the data is still in the image
                    }
                    else if(MatchZeroAudio.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found SILENCE \"{1}\" at line {0}", line, MatchZeroAudio.Groups["length"].Value);
                        // Seems can be ignored as the data is still in the image
                    }
                    else if(MatchAudioFile.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found AUDIOFILE \"{1}\" at line {0}", line, MatchAudioFile.Groups["filename"].Value);

                        currenttrack.trackfile = new CDRDAOTrackFile();
                        currenttrack.trackfile.datafilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), MatchAudioFile.Groups["filename"].Value));
                        currenttrack.trackfile.datafile = MatchAudioFile.Groups["filename"].Value;
                        currenttrack.trackfile.offset = MatchAudioFile.Groups["base_offset"].Value != "" ? ulong.Parse(MatchAudioFile.Groups["base_offset"].Value) : 0;

                        currenttrack.trackfile.filetype = "BINARY";
                        currenttrack.trackfile.sequence = currentTrackNumber;

                        ulong startSectors = 0;

                        if(MatchAudioFile.Groups["start"].Value != "")
                        {
                            string[] startString = MatchAudioFile.Groups["start"].Value.Split(new char[] { ':' });
                            startSectors = ulong.Parse(startString[0]) * 60 * 75 + ulong.Parse(startString[1]) * 75 + ulong.Parse(startString[2]);
                        }

                        currenttrack.trackfile.offset += (startSectors * currenttrack.bps);

                        if(MatchAudioFile.Groups["length"].Value != "")
                        {
                            string[] lengthString = MatchAudioFile.Groups["length"].Value.Split(new char[] { ':' });
                            currenttrack.sectors = ulong.Parse(lengthString[0]) * 60 * 75 + ulong.Parse(lengthString[1]) * 75 + ulong.Parse(lengthString[2]);
                        }
                        else
                            currenttrack.sectors = ((ulong)currenttrack.trackfile.datafilter.GetDataForkLength() - currenttrack.trackfile.offset) / currenttrack.bps;
                    }
                    else if(MatchFile.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found DATAFILE \"{1}\" at line {0}", line, MatchFile.Groups["filename"].Value);

                        currenttrack.trackfile = new CDRDAOTrackFile();
                        currenttrack.trackfile.datafilter = filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(), MatchFile.Groups["filename"].Value));
                        currenttrack.trackfile.datafile = MatchAudioFile.Groups["filename"].Value;
                        currenttrack.trackfile.offset = MatchFile.Groups["base_offset"].Value != "" ? ulong.Parse(MatchFile.Groups["base_offset"].Value) : 0;

                        currenttrack.trackfile.filetype = "BINARY";
                        currenttrack.trackfile.sequence = currentTrackNumber;
                        if(MatchFile.Groups["length"].Value != "")
                        {
                            string[] lengthString = MatchFile.Groups["length"].Value.Split(new char[] { ':' });
                            currenttrack.sectors = ulong.Parse(lengthString[0]) * 60 * 75 + ulong.Parse(lengthString[1]) * 75 + ulong.Parse(lengthString[2]);
                        }
                        else
                            currenttrack.sectors = ((ulong)currenttrack.trackfile.datafilter.GetDataForkLength() - currenttrack.trackfile.offset) / currenttrack.bps;
                    }
                    else if(MatchTitle.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found TITLE \"{1}\" at line {0}", line, MatchTitle.Groups["title"].Value);
                        if(intrack)
                            currenttrack.title = MatchTitle.Groups["title"].Value;
                        else
                            discimage.title = MatchTitle.Groups["title"].Value;
                    }
                    else if(MatchPerformer.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found PERFORMER \"{1}\" at line {0}", line, MatchPerformer.Groups["performer"].Value);
                        if(intrack)
                            currenttrack.performer = MatchPerformer.Groups["performer"].Value;
                        else
                            discimage.performer = MatchPerformer.Groups["performer"].Value;
                    }
                    else if(MatchSongwriter.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found SONGWRITER \"{1}\" at line {0}", line, MatchSongwriter.Groups["songwriter"].Value);
                        if(intrack)
                            currenttrack.songwriter = MatchSongwriter.Groups["songwriter"].Value;
                        else
                            discimage.songwriter = MatchSongwriter.Groups["songwriter"].Value;
                    }
                    else if(MatchComposer.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found COMPOSER \"{1}\" at line {0}", line, MatchComposer.Groups["composer"].Value);
                        if(intrack)
                            currenttrack.composer = MatchComposer.Groups["composer"].Value;
                        else
                            discimage.composer = MatchComposer.Groups["composer"].Value;
                    }
                    else if(MatchArranger.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found ARRANGER \"{1}\" at line {0}", line, MatchArranger.Groups["arranger"].Value);
                        if(intrack)
                            currenttrack.arranger = MatchArranger.Groups["arranger"].Value;
                        else
                            discimage.arranger = MatchArranger.Groups["arranger"].Value;
                    }
                    else if(MatchMessage.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found MESSAGE \"{1}\" at line {0}", line, MatchMessage.Groups["message"].Value);
                        if(intrack)
                            currenttrack.message = MatchMessage.Groups["message"].Value;
                        else
                            discimage.message = MatchMessage.Groups["message"].Value;
                    }
                    else if(MatchDiscID.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found DISC_ID \"{1}\" at line {0}", line, MatchDiscID.Groups["discid"].Value);
                        if(!intrack)
                            discimage.disk_id = MatchDiscID.Groups["discid"].Value;
                    }
                    else if(MatchUPC.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found UPC_EAN \"{1}\" at line {0}", line, MatchUPC.Groups["catalog"].Value);
                        if(!intrack)
                            discimage.barcode = MatchUPC.Groups["catalog"].Value;
                    }
                    // Ignored fields
                    else if(MatchCDText.Success || MatchLanguage.Success || MatchClosure.Success ||
                        MatchLanguageMap.Success || MatchLanguageMapping.Success)
                    {

                    }
                    else if(_line == "") // Empty line, ignore it
                    {

                    }
                    // TODO: Regex CD-TEXT SIZE_INFO
                    /*
                    else // Non-empty unknown field
                    {
                        throw new FeatureUnsupportedImageException(string.Format("Found unknown field defined at line {0}: \"{1}\"", line, _line));
                    }
                    */
                }

                if(currenttrack.sequence != 0)
                {
                    if(currenttrack.pregap != currenttrack.sectors && !currenttrack.indexes.ContainsKey(1))
                        currenttrack.indexes.Add(1, currenttrack.startSector + currenttrack.pregap);

                    discimage.tracks.Add(currenttrack);
                }

                discimage.comment = commentBuilder.ToString();

                // DEBUG information
                DicConsole.DebugWriteLine("CDRDAO plugin", "Disc image parsing results");
                DicConsole.DebugWriteLine("CDRDAO plugin", "Disc CD-TEXT:");
                if(discimage.arranger == null)
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tArranger is not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tArranger: {0}", discimage.arranger);
                if(discimage.composer == null)
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tComposer is not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tComposer: {0}", discimage.composer);
                if(discimage.performer == null)
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPerformer is not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPerformer: {0}", discimage.performer);
                if(discimage.songwriter == null)
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tSongwriter is not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tSongwriter: {0}", discimage.songwriter);
                if(discimage.title == null)
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tTitle is not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tTitle: {0}", discimage.title);
                DicConsole.DebugWriteLine("CDRDAO plugin", "Disc information:");
                DicConsole.DebugWriteLine("CDRDAO plugin", "\tGuessed disk type: {0}", discimage.disktype);
                if(discimage.barcode == null)
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tBarcode not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tBarcode: {0}", discimage.barcode);
                if(discimage.disk_id == null)
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tDisc ID not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tDisc ID: {0}", discimage.disk_id);
                if(discimage.mcn == null)
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tMCN not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tMCN: {0}", discimage.mcn);
                if(string.IsNullOrEmpty(discimage.comment))
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tComment not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tComment: \"{0}\"", discimage.comment);

                DicConsole.DebugWriteLine("CDRDAO plugin", "Track information:");
                DicConsole.DebugWriteLine("CDRDAO plugin", "\tDisc contains {0} tracks", discimage.tracks.Count);
                for(int i = 0; i < discimage.tracks.Count; i++)
                {
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tTrack {0} information:", discimage.tracks[i].sequence);

                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\t{0} bytes per sector", discimage.tracks[i].bps);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tPregap: {0} sectors", discimage.tracks[i].pregap);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tData: {0} sectors starting at sector {1}", discimage.tracks[i].sectors, discimage.tracks[i].startSector);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tPostgap: {0} sectors", discimage.tracks[i].postgap);

                    if(discimage.tracks[i].flag_4ch)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tTrack is flagged as quadraphonic");
                    if(discimage.tracks[i].flag_dcp)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tTrack allows digital copy");
                    if(discimage.tracks[i].flag_pre)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tTrack has pre-emphasis applied");

                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tTrack resides in file {0}, type defined as {1}, starting at byte {2}",
					                          discimage.tracks[i].trackfile.datafilter.GetFilename(), discimage.tracks[i].trackfile.filetype, discimage.tracks[i].trackfile.offset);

                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tIndexes:");
                    foreach(KeyValuePair<int, ulong> kvp in discimage.tracks[i].indexes)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\t\tIndex {0} starts at sector {1}", kvp.Key, kvp.Value);

                    if(discimage.tracks[i].isrc == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tISRC is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tISRC: {0}", discimage.tracks[i].isrc);

                    if(discimage.tracks[i].arranger == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tArranger is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tArranger: {0}", discimage.tracks[i].arranger);
                    if(discimage.tracks[i].composer == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tComposer is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tComposer: {0}", discimage.tracks[i].composer);
                    if(discimage.tracks[i].performer == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tPerformer is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tPerformer: {0}", discimage.tracks[i].performer);
                    if(discimage.tracks[i].songwriter == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tSongwriter is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tSongwriter: {0}", discimage.tracks[i].songwriter);
                    if(discimage.tracks[i].title == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tTitle is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tTitle: {0}", discimage.tracks[i].title);
                }

                DicConsole.DebugWriteLine("CDRDAO plugin", "Building offset map");

                partitions = new List<Partition>();
                offsetmap = new Dictionary<uint, ulong>();

                ulong byte_offset = 0;
                ulong partitionSequence = 0;
                for(int i = 0; i < discimage.tracks.Count; i++)
                {
                    ulong index0_len = 0;

                    if(discimage.tracks[i].sequence == 1 && i != 0)
                        throw new ImageNotSupportedException("Unordered tracks");

                    Partition partition = new Partition();

                    // Index 01
                    partition.Description = string.Format("Track {0}.", discimage.tracks[i].sequence);
                    partition.Name = discimage.tracks[i].title;
                    partition.Start = discimage.tracks[i].startSector;
                    partition.Size = (discimage.tracks[i].sectors - index0_len) * discimage.tracks[i].bps;
                    partition.Length = (discimage.tracks[i].sectors - index0_len);
                    partition.Sequence = partitionSequence;
                    partition.Offset = byte_offset;
                    partition.Type = discimage.tracks[i].tracktype;

                    byte_offset += partition.Size;
                    partitionSequence++;

                    if(!offsetmap.ContainsKey(discimage.tracks[i].sequence))
                        offsetmap.Add(discimage.tracks[i].sequence, partition.Start);
                    else
                    {
                        ulong old_start;
                        offsetmap.TryGetValue(discimage.tracks[i].sequence, out old_start);

                        if(partition.Start < old_start)
                        {
                            offsetmap.Remove(discimage.tracks[i].sequence);
                            offsetmap.Add(discimage.tracks[i].sequence, partition.Start);
                        }
                    }

                    partitions.Add(partition);
                    partition = new Partition();
                }

                // Print partition map
                DicConsole.DebugWriteLine("CDRDAO plugin", "printing partition map");
                foreach(Partition partition in partitions)
                {
                    DicConsole.DebugWriteLine("CDRDAO plugin", "Partition sequence: {0}", partition.Sequence);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition name: {0}", partition.Name);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition description: {0}", partition.Description);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition type: {0}", partition.Type);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition starting sector: {0}", partition.Start);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition sectors: {0}", partition.Length);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition starting offset: {0}", partition.Offset);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition size in bytes: {0}", partition.Size);
                }

                foreach(CDRDAOTrack track in discimage.tracks)
                {
                    ImageInfo.imageSize += track.bps * track.sectors;
                    ImageInfo.sectors += track.sectors;
                }

                if(discimage.disktype == MediaType.CDG || discimage.disktype == MediaType.CDEG || discimage.disktype == MediaType.CDMIDI)
                    ImageInfo.sectorSize = 2448; // CD+G subchannels ARE user data, as CD+G are useless without them
                else if(discimage.disktype != MediaType.CDROMXA && discimage.disktype != MediaType.CDDA && discimage.disktype != MediaType.CDI && discimage.disktype != MediaType.CDPLUS)
                    ImageInfo.sectorSize = 2048; // Only data tracks
                else
                    ImageInfo.sectorSize = 2352; // All others

                if(discimage.mcn != null)
                    ImageInfo.readableMediaTags.Add(MediaTagType.CD_MCN);

                ImageInfo.imageApplication = "CDRDAO";

                ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
                ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();

                ImageInfo.imageComments = discimage.comment;
                ImageInfo.mediaSerialNumber = discimage.mcn;
                ImageInfo.mediaBarcode = discimage.barcode;
                ImageInfo.mediaType = discimage.disktype;

                ImageInfo.readableSectorTags.Add(SectorTagType.CDTrackFlags);

                foreach(CDRDAOTrack track in discimage.tracks)
                {
                    if(track.subchannel)
                    {
                        if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubchannel))
                            ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubchannel);
                    }

                    switch(track.tracktype)
                    {
                        case CDRDAOTrackTypeAudio:
                            {
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDTrackISRC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDTrackISRC);
                                break;
                            }
                        case CDRDAOTrackTypeMode2:
                        case CDRDAOTrackTypeMode2Mix:
                            {
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubHeader))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubHeader);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
                                break;
                            }
                        case CDRDAOTrackTypeMode2Raw:
                            {
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSync))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSync);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorHeader))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorHeader);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubHeader))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubHeader);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
                                break;
                            }
                        case CDRDAOTrackTypeMode1Raw:
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
                                break;
                            }
                    }
                }

                ImageInfo.xmlMediaType = XmlMediaType.OpticalDisc;

                DicConsole.VerboseWriteLine("CDRDAO image describes a disc of type {0}", ImageInfo.mediaType);
                if(!string.IsNullOrEmpty(ImageInfo.imageComments))
                    DicConsole.VerboseWriteLine("CDRDAO comments: {0}", ImageInfo.imageComments);

                return true;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter);
                DicConsole.ErrorWriteLine("Exception: {0}", ex.Message);
                DicConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);
                return false;
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
                case MediaTagType.CD_MCN:
                    {
                        if(discimage.mcn != null)
                        {
                            return Encoding.ASCII.GetBytes(discimage.mcn);
                        }
                        throw new FeatureNotPresentImageException("Image does not contain MCN information.");
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
                    foreach(CDRDAOTrack cdrdao_track in discimage.tracks)
                    {
                        if(cdrdao_track.sequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < cdrdao_track.sectors)
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
                    foreach(CDRDAOTrack cdrdao_track in discimage.tracks)
                    {
                        if(cdrdao_track.sequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < cdrdao_track.sectors)
                                return ReadSectorsTag((sectorAddress - kvp.Value), length, kvp.Key, tag);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), string.Format("Sector address {0} not found", sectorAddress));
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            CDRDAOTrack _track = new CDRDAOTrack();

            _track.sequence = 0;

            foreach(CDRDAOTrack cdrdao_track in discimage.tracks)
            {
                if(cdrdao_track.sequence == track)
                {
                    _track = cdrdao_track;
                    break;
                }
            }

            if(_track.sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > _track.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than present in track, won't cross tracks");

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.tracktype)
            {
                case CDRDAOTrackTypeMode1:
                case CDRDAOTrackTypeMode2Form1:
                    {
                        sector_offset = 0;
                        sector_size = 2048;
                        sector_skip = 0;
                        break;
                    }
                case CDRDAOTrackTypeMode2Form2:
                    {
                        sector_offset = 0;
                        sector_size = 2324;
                        sector_skip = 0;
                        break;
                    }
                case CDRDAOTrackTypeMode2:
                case CDRDAOTrackTypeMode2Mix:
                    {
                        sector_offset = 0;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                case CDRDAOTrackTypeAudio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                case CDRDAOTrackTypeMode1Raw:
                    {
                        sector_offset = 16;
                        sector_size = 2048;
                        sector_skip = 288;
                        break;
                    }
                case CDRDAOTrackTypeMode2Raw:
                    {
                        sector_offset = 16;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            if(_track.subchannel)
                sector_skip += 96;

            byte[] buffer = new byte[sector_size * length];

            imageStream = _track.trackfile.datafilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream.Seek((long)_track.trackfile.offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * length));
            else
            {
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                }
            }

            return buffer;
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            CDRDAOTrack _track = new CDRDAOTrack();

            _track.sequence = 0;

            foreach(CDRDAOTrack cdrdao_track in discimage.tracks)
            {
                if(cdrdao_track.sequence == track)
                {
                    _track = cdrdao_track;
                    break;
                }
            }

            if(_track.sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > _track.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than present in track, won't cross tracks");

            uint sector_offset;
            uint sector_size;
            uint sector_skip = 0;

            if(!_track.subchannel && tag == SectorTagType.CDSectorSubchannel)
                throw new ArgumentException("No tags in image for requested track", nameof(tag));

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
                    {
                        byte[] flags = new byte[1];

                        if(_track.tracktype != CDRDAOTrackTypeAudio)
                            flags[0] += 0x40;

                        if(_track.flag_dcp)
                            flags[0] += 0x20;

                        if(_track.flag_pre)
                            flags[0] += 0x10;

                        if(_track.flag_4ch)
                            flags[0] += 0x80;

                        return flags;
                    }
                case SectorTagType.CDTrackISRC:
                    return Encoding.UTF8.GetBytes(_track.isrc);
                default:
                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(_track.tracktype)
            {
                case CDRDAOTrackTypeMode1:
                case CDRDAOTrackTypeMode2Form1:
                    if(tag == SectorTagType.CDSectorSubchannel)
                    {
                        sector_offset = 2048;
                        sector_size = 96;
                        break;
                    }
                    throw new ArgumentException("No tags in image for requested track", nameof(tag));
                case CDRDAOTrackTypeMode2Form2:
                case CDRDAOTrackTypeMode2Mix:
                    if(tag == SectorTagType.CDSectorSubchannel)
                    {
                        sector_offset = 2336;
                        sector_size = 96;
                        break;
                    }
                    throw new ArgumentException("No tags in image for requested track", nameof(tag));
                case CDRDAOTrackTypeAudio:
                    if(tag == SectorTagType.CDSectorSubchannel)
                    {
                        sector_offset = 2352;
                        sector_size = 96;
                        break;
                    }
                    throw new ArgumentException("No tags in image for requested track", nameof(tag));
                case CDRDAOTrackTypeMode1Raw:
                    {
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
                            case SectorTagType.CDSectorSubchannel:
                                {
                                    sector_offset = 2352;
                                    sector_size = 96;
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
                    }
                case CDRDAOTrackTypeMode2Raw: // Requires reading sector
                    if(tag == SectorTagType.CDSectorSubchannel)
                    {
                        sector_offset = 2352;
                        sector_size = 96;
                        break;
                    }
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = _track.trackfile.datafilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream.Seek((long)_track.trackfile.offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * length));
            else
            {
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
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
                    foreach(CDRDAOTrack cdrdao_track in discimage.tracks)
                    {
                        if(cdrdao_track.sequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < cdrdao_track.sectors)
                                return ReadSectorsLong((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            CDRDAOTrack _track = new CDRDAOTrack();

            _track.sequence = 0;

            foreach(CDRDAOTrack cdrdao_track in discimage.tracks)
            {
                if(cdrdao_track.sequence == track)
                {
                    _track = cdrdao_track;
                    break;
                }
            }

            if(_track.sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > _track.sectors)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested more sectors than present in track, won't cross tracks");

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch(_track.tracktype)
            {
                case CDRDAOTrackTypeMode1:
                case CDRDAOTrackTypeMode2Form1:
                    {
                        sector_offset = 0;
                        sector_size = 2048;
                        sector_skip = 0;
                        break;
                    }
                case CDRDAOTrackTypeMode2Form2:
                    {
                        sector_offset = 0;
                        sector_size = 2324;
                        sector_skip = 0;
                        break;
                    }
                case CDRDAOTrackTypeMode2:
                case CDRDAOTrackTypeMode2Mix:
                    {
                        sector_offset = 0;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                case CDRDAOTrackTypeMode1Raw:
                case CDRDAOTrackTypeMode2Raw:
                case CDRDAOTrackTypeAudio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            if(_track.subchannel)
                sector_skip += 96;

            byte[] buffer = new byte[sector_size * length];

            imageStream = _track.trackfile.datafilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);

            br.BaseStream.Seek((long)_track.trackfile.offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);

            if(sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * length));
            else
            {
                for(int i = 0; i < length; i++)
                {
                    byte[] sector;
                    br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                    sector = br.ReadBytes((int)sector_size);
                    br.BaseStream.Seek(sector_skip, SeekOrigin.Current);

                    Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                }
            }

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "CDRDAO tocfile";
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

        public override string GetImageComments()
        {
            return ImageInfo.imageComments;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.mediaSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.mediaBarcode;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.mediaType;
        }

        public override List<Partition> GetPartitions()
        {
            return partitions;
        }

        public override List<Track> GetTracks()
        {
            List<Track> tracks = new List<Track>();

            foreach(CDRDAOTrack cdr_track in discimage.tracks)
            {
                Track _track = new Track();

                _track.Indexes = cdr_track.indexes;
                _track.TrackDescription = cdr_track.title;
                if(!cdr_track.indexes.TryGetValue(0, out _track.TrackStartSector))
                    cdr_track.indexes.TryGetValue(1, out _track.TrackStartSector);
                _track.TrackStartSector = cdr_track.startSector;
                _track.TrackEndSector = _track.TrackStartSector + cdr_track.sectors - 1;
                _track.TrackPregap = cdr_track.pregap;
                _track.TrackSession = 1;
                _track.TrackSequence = cdr_track.sequence;
                _track.TrackType = CDRDAOTrackTypeToTrackType(cdr_track.tracktype);
                _track.TrackFilter = cdr_track.trackfile.datafilter;
                _track.TrackFile = cdr_track.trackfile.datafilter.GetFilename();
                _track.TrackFileOffset = cdr_track.trackfile.offset;
                _track.TrackFileType = cdr_track.trackfile.filetype;
                _track.TrackRawBytesPerSector = cdr_track.bps;
                _track.TrackBytesPerSector = CDRDAOTrackTypeToCookedBytesPerSector(cdr_track.tracktype);
                if(cdr_track.subchannel)
                {
                    _track.TrackSubchannelType = cdr_track.packedsubchannel ? TrackSubchannelType.PackedInterleaved : TrackSubchannelType.RawInterleaved;
                    _track.TrackSubchannelFilter = cdr_track.trackfile.datafilter; 
                    _track.TrackSubchannelFile = cdr_track.trackfile.datafilter.GetFilename();
                    _track.TrackSubchannelOffset = cdr_track.trackfile.offset;
                }
                else
                    _track.TrackSubchannelType = TrackSubchannelType.None;

                tracks.Add(_track);
            }

            return tracks;
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            return GetSessionTracks(session.SessionSequence);
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            if(session == 1)
                return GetTracks();
            throw new ImageNotSupportedException("Session does not exist in disc image");
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

		#region Not implemented methods

		public override List<Session> GetSessions()
		{
			// TODO
			throw new NotImplementedException();
		}

		#endregion

		#region Private methods

		static ushort CDRDAOTrackTypeToBytesPerSector(string trackType)
        {
            switch(trackType)
            {
                case CDRDAOTrackTypeMode1:
                case CDRDAOTrackTypeMode2Form1:
                    return 2048;
                case CDRDAOTrackTypeMode2Form2:
                    return 2324;
                case CDRDAOTrackTypeMode2:
                case CDRDAOTrackTypeMode2Mix:
                    return 2336;
                case CDRDAOTrackTypeAudio:
                case CDRDAOTrackTypeMode1Raw:
                case CDRDAOTrackTypeMode2Raw:
                    return 2352;
                default:
                    return 0;
            }
        }

        static ushort CDRDAOTrackTypeToCookedBytesPerSector(string trackType)
        {
            switch(trackType)
            {
                case CDRDAOTrackTypeMode1:
                case CDRDAOTrackTypeMode2Form1:
                case CDRDAOTrackTypeMode1Raw:
                    return 2048;
                case CDRDAOTrackTypeMode2Form2:
                    return 2324;
                case CDRDAOTrackTypeMode2:
                case CDRDAOTrackTypeMode2Mix:
                case CDRDAOTrackTypeMode2Raw:
                    return 2336;
                case CDRDAOTrackTypeAudio:
                    return 2352;
                default:
                    return 0;
            }
        }

        static TrackType CDRDAOTrackTypeToTrackType(string trackType)
        {
            switch(trackType)
            {
                case CDRDAOTrackTypeMode1:
                case CDRDAOTrackTypeMode1Raw:
                    return TrackType.CDMode1;
                case CDRDAOTrackTypeMode2Form1:
                    return TrackType.CDMode2Form1;
                case CDRDAOTrackTypeMode2Form2:
                    return TrackType.CDMode2Form2;
                case CDRDAOTrackTypeMode2:
                case CDRDAOTrackTypeMode2Mix:
                case CDRDAOTrackTypeMode2Raw:
                    return TrackType.CDMode2Formless;
                case CDRDAOTrackTypeAudio:
                    return TrackType.Audio;
                default:
                    return TrackType.Data;
            }
        }

        #endregion

        #region Unsupported features

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

        public override string GetMediaPartNumber()
        {
            return ImageInfo.mediaPartNumber;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.mediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.mediaModel;
        }

        public override string GetImageName()
        {
            return ImageInfo.imageName;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.imageCreator;
        }

        #endregion
    }
}

