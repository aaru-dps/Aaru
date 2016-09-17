// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CDRWin.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages CDRWin cuesheets (cue/bin).
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

// TODO: Implement track flags
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using DiscImageChef.Console;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filters;

namespace DiscImageChef.ImagePlugins
{
    class CDRWin : ImagePlugin
    {
        #region Internal structures

        struct CDRWinTrackFile
        {
            /// <summary>Track #</summary>
            public uint sequence;
            /// <summary>Filter of file containing track</summary>
            public Filter datafilter;
            /// <summary>Offset of track start in file</summary>
            public ulong offset;
            /// <summary>Type of file</summary>
            public string filetype;
        }

        struct CDRWinTrack
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
            /// <summary>File struct for this track</summary>
            public CDRWinTrackFile trackfile;
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
            /// <summary>Track has SCMS</summary>
            public bool flag_scms;
            /// <summary>Bytes per sector</summary>
            public ushort bps;
            /// <summary>Sectors in track</summary>
            public ulong sectors;
            /// <summary>Track type</summary>
            public string tracktype;
            /// <summary>Track session</summary>
            public ushort session;
        }

        #endregion

        struct CDRWinDisc
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
            /// <summary>Sessions</summary>
            public List<Session> sessions;
            /// <summary>Tracks</summary>
            public List<CDRWinTrack> tracks;
            /// <summary>Disk comment</summary>
            public string comment;
            /// <summary>File containing CD-Text</summary>
            public string cdtextfile;
        }

        #region Internal consts

        // Type for FILE entity
        /// <summary>Data as-is in little-endian</summary>
        const string CDRWinDiskTypeLittleEndian = "BINARY";
        /// <summary>Data as-is in big-endian</summary>
        const string CDRWinDiskTypeBigEndian = "MOTOROLA";
        /// <summary>Audio in Apple AIF file</summary>
        const string CDRWinDiskTypeAIFF = "AIFF";
        /// <summary>Audio in Microsoft WAV file</summary>
        const string CDRWinDiskTypeRIFF = "WAVE";
        /// <summary>Audio in MP3 file</summary>
        const string CDRWinDiskTypeMP3 = "MP3";

        // Type for TRACK entity
        /// <summary>Audio track, 2352 bytes/sector</summary>
        const string CDRWinTrackTypeAudio = "AUDIO";
        /// <summary>CD+G track, 2448 bytes/sector (audio+subchannel)</summary>
        const string CDRWinTrackTypeCDG = "CDG";
        /// <summary>Mode 1 track, cooked, 2048 bytes/sector</summary>
        const string CDRWinTrackTypeMode1 = "MODE1/2048";
        /// <summary>Mode 1 track, raw, 2352 bytes/sector</summary>
        const string CDRWinTrackTypeMode1Raw = "MODE1/2352";
        /// <summary>Mode 2 form 1 track, cooked, 2048 bytes/sector</summary>
        const string CDRWinTrackTypeMode2Form1 = "MODE2/2048";
        /// <summary>Mode 2 form 2 track, cooked, 2324 bytes/sector</summary>
        const string CDRWinTrackTypeMode2Form2 = "MODE2/2324";
        /// <summary>Mode 2 formless track, cooked, 2336 bytes/sector</summary>
        const string CDRWinTrackTypeMode2Formless = "MODE2/2336";
        /// <summary>Mode 2 track, raw, 2352 bytes/sector</summary>
        const string CDRWinTrackTypeMode2Raw = "MODE2/2352";
        /// <summary>CD-i track, cooked, 2336 bytes/sector</summary>
        const string CDRWinTrackTypeCDI = "CDI/2336";
        /// <summary>CD-i track, raw, 2352 bytes/sector</summary>
        const string CDRWinTrackTypeCDIRaw = "CDI/2352";

        // Type for REM ORIGINAL MEDIA-TYPE entity
        /// <summary>DiskType.CD</summary>
        const string CDRWinDiskTypeCD = "CD";
        /// <summary>DiskType.CDRW</summary>
        const string CDRWinDiskTypeCDRW = "CD-RW";
        /// <summary>DiskType.CDMRW</summary>
        const string CDRWinDiskTypeCDMRW = "CD-MRW";
        /// <summary>DiskType.CDMRW</summary>
        const string CDRWinDiskTypeCDMRW2 = "CD-(MRW)";
        /// <summary>DiskType.DVDROM</summary>
        const string CDRWinDiskTypeDVD = "DVD";
        /// <summary>DiskType.DVDPRW</summary>
        const string CDRWinDiskTypeDVDPMRW = "DVD+MRW";
        /// <summary>DiskType.DVDPRW</summary>
        const string CDRWinDiskTypeDVDPMRW2 = "DVD+(MRW)";
        /// <summary>DiskType.DVDPRWDL</summary>
        const string CDRWinDiskTypeDVDPMRWDL = "DVD+MRW DL";
        /// <summary>DiskType.DVDPRWDL</summary>
        const string CDRWinDiskTypeDVDPMRWDL2 = "DVD+(MRW) DL";
        /// <summary>DiskType.DVDPR</summary>
        const string CDRWinDiskTypeDVDPR = "DVD+R";
        /// <summary>DiskType.DVDPRDL</summary>
        const string CDRWinDiskTypeDVDPRDL = "DVD+R DL";
        /// <summary>DiskType.DVDPRW</summary>
        const string CDRWinDiskTypeDVDPRW = "DVD+RW";
        /// <summary>DiskType.DVDPRWDL</summary>
        const string CDRWinDiskTypeDVDPRWDL = "DVD+RW DL";
        /// <summary>DiskType.DVDPR</summary>
        const string CDRWinDiskTypeDVDPVR = "DVD+VR";
        /// <summary>DiskType.DVDRAM</summary>
        const string CDRWinDiskTypeDVDRAM = "DVD-RAM";
        /// <summary>DiskType.DVDR</summary>
        const string CDRWinDiskTypeDVDR = "DVD-R";
        /// <summary>DiskType.DVDRDL</summary>
        const string CDRWinDiskTypeDVDRDL = "DVD-R DL";
        /// <summary>DiskType.DVDRW</summary>
        const string CDRWinDiskTypeDVDRW = "DVD-RW";
        /// <summary>DiskType.DVDRWDL</summary>
        const string CDRWinDiskTypeDVDRWDL = "DVD-RW DL";
        /// <summary>DiskType.DVDR</summary>
        const string CDRWinDiskTypeDVDVR = "DVD-VR";
        /// <summary>DiskType.DVDRW</summary>
        const string CDRWinDiskTypeDVDRW2 = "DVDRW";
        /// <summary>DiskType.HDDVDROM</summary>
        const string CDRWinDiskTypeHDDVD = "HD DVD";
        /// <summary>DiskType.HDDVDRAM</summary>
        const string CDRWinDiskTypeHDDVDRAM = "HD DVD-RAM";
        /// <summary>DiskType.HDDVDR</summary>
        const string CDRWinDiskTypeHDDVDR = "HD DVD-R";
        /// <summary>DiskType.HDDVDR</summary>
        const string CDRWinDiskTypeHDDVDRDL = "HD DVD-R DL";
        /// <summary>DiskType.HDDVDRW</summary>
        const string CDRWinDiskTypeHDDVDRW = "HD DVD-RW";
        /// <summary>DiskType.HDDVDRW</summary>
        const string CDRWinDiskTypeHDDVDRWDL = "HD DVD-RW DL";
        /// <summary>DiskType.BDROM</summary>
        const string CDRWinDiskTypeBD = "BD";
        /// <summary>DiskType.BDR</summary>
        const string CDRWinDiskTypeBDR = "BD-R";
        /// <summary>DiskType.BDRE</summary>
        const string CDRWinDiskTypeBDRE = "BD-RE";
        /// <summary>DiskType.BDR</summary>
        const string CDRWinDiskTypeBDRDL = "BD-R DL";
        /// <summary>DiskType.BDRE</summary>
        const string CDRWinDiskTypeBDREDL = "BD-RE DL";

        #endregion

        #region Internal variables

        Filter imageFilter;
        StreamReader cueStream;
        Stream imageStream;
        /// <summary>Dictionary, index is track #, value is TrackFile</summary>
        Dictionary<uint, ulong> offsetmap;
        CDRWinDisc discimage;
        List<Partition> partitions;

        #endregion

        #region Parsing regexs

        const string SessionRegEx = "\\bREM\\s+SESSION\\s+(?<number>\\d+).*$";
        const string DiskTypeRegEx = "\\bREM\\s+ORIGINAL MEDIA-TYPE:\\s+(?<mediatype>.+)$";
        const string LeadOutRegEx = "\\bREM\\s+LEAD-OUT\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        // Not checked
        const string LBARegEx = "\\bREM MSF:\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)\\s+=\\s+LBA:\\s+(?<lba>[\\d]+)$";
        const string DiskIDRegEx = "\\bDISC_ID\\s+(?<diskid>[\\da-f]{8})$";
        const string BarCodeRegEx = "\\bUPC_EAN\\s+(?<barcode>[\\d]{12,13})$";
        const string CommentRegEx = "\\bREM\\s+(?<comment>.+)$";
        const string CDTextRegEx = "\\bCDTEXTFILE\\s+(?<filename>.+)$";
        const string MCNRegEx = "\\bCATALOG\\s+(?<catalog>\\d{13})$";
        const string TitleRegEx = "\\bTITLE\\s+(?<title>.+)$";
        const string GenreRegEx = "\\bGENRE\\s+(?<genre>.+)$";
        const string ArrangerRegEx = "\\bARRANGER\\s+(?<arranger>.+)$";
        const string ComposerRegEx = "\\bCOMPOSER\\s+(?<composer>.+)$";
        const string PerformerRegEx = "\\bPERFORMER\\s+(?<performer>.+)$";
        const string SongWriterRegEx = "\\bSONGWRITER\\s+(?<songwriter>.+)$";
        const string FileRegEx = "\\bFILE\\s+(?<filename>.+)\\s+(?<type>\\S+)$";
        const string TrackRegEx = "\\bTRACK\\s+(?<number>\\d+)\\s+(?<type>\\S+)$";
        const string ISRCRegEx = "\\bISRC\\s+(?<isrc>\\w{12})$";
        const string IndexRegEx = "\\bINDEX\\s+(?<index>\\d+)\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        const string PregapRegEx = "\\bPREGAP\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        const string PostgapRegex = "\\bPOSTGAP\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        const string FlagsRegEx = "\\bFLAGS\\s+(((?<dcp>DCP)|(?<quad>4CH)|(?<pre>PRE)|(?<scms>SCMS))\\s*)+$";

        #endregion

        #region Methods

        public CDRWin()
        {
            Name = "CDRWin cuesheet";
            PluginUUID = new Guid("664568B2-15D4-4E64-8A7A-20BDA8B8386F");
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

        // Due to .cue format, this method must parse whole file, ignoring errors (those will be thrown by OpenImage()).
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
				foreach(byte b in testArray)
				{
					if(b < 0x20 && b != 0x0A && b != 0x0D)
						return false;
				}

				cueStream = new StreamReader(this.imageFilter.GetDataForkStream());
                int line = 0;

                while(cueStream.Peek() >= 0)
                {
                    line++;
                    string _line = cueStream.ReadLine();

                    Regex Sr = new Regex(SessionRegEx);
                    Regex Rr = new Regex(CommentRegEx);
                    Regex Cr = new Regex(MCNRegEx);
                    Regex Fr = new Regex(FileRegEx);
                    Regex Tr = new Regex(CDTextRegEx);

                    Match Sm;
                    Match Rm;
                    Match Cm;
                    Match Fm;
                    Match Tm;

                    // First line must be SESSION, REM, CATALOG,  FILE or CDTEXTFILE.
                    Sm = Sr.Match(_line);
                    Rm = Rr.Match(_line);
                    Cm = Cr.Match(_line);
                    Fm = Fr.Match(_line);
                    Tm = Tr.Match(_line);

                    if(!Sm.Success && !Rm.Success && !Cm.Success && !Fm.Success && !Tm.Success)
                        return false;
                    return true;
                }

                return false;
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
                bool intrack = false;
                byte currentsession = 1;

                // Initialize all RegExs
                Regex RegexSession = new Regex(SessionRegEx);
                Regex RegexDiskType = new Regex(DiskTypeRegEx);
                Regex RegexLeadOut = new Regex(LeadOutRegEx);
                Regex RegexLBA = new Regex(LBARegEx);
                Regex RegexDiskID = new Regex(DiskIDRegEx);
                Regex RegexBarCode = new Regex(BarCodeRegEx);
                Regex RegexComment = new Regex(CommentRegEx);
                Regex RegexCDText = new Regex(CDTextRegEx);
                Regex RegexMCN = new Regex(MCNRegEx);
                Regex RegexTitle = new Regex(TitleRegEx);
                Regex RegexGenre = new Regex(GenreRegEx);
                Regex RegexArranger = new Regex(ArrangerRegEx);
                Regex RegexComposer = new Regex(ComposerRegEx);
                Regex RegexPerformer = new Regex(PerformerRegEx);
                Regex RegexSongWriter = new Regex(SongWriterRegEx);
                Regex RegexFile = new Regex(FileRegEx);
                Regex RegexTrack = new Regex(TrackRegEx);
                Regex RegexISRC = new Regex(ISRCRegEx);
                Regex RegexIndex = new Regex(IndexRegEx);
                Regex RegexPregap = new Regex(PregapRegEx);
                Regex RegexPostgap = new Regex(PostgapRegex);
                Regex RegexFlags = new Regex(FlagsRegEx);

                // Initialize all RegEx matches
                Match MatchSession;
                Match MatchDiskType;
                Match MatchLeadOut;
                Match MatchLBA;
                Match MatchDiskID;
                Match MatchBarCode;
                Match MatchComment;
                Match MatchCDText;
                Match MatchMCN;
                Match MatchTitle;
                Match MatchGenre;
                Match MatchArranger;
                Match MatchComposer;
                Match MatchPerformer;
                Match MatchSongWriter;
                Match MatchFile;
                Match MatchTrack;
                Match MatchISRC;
                Match MatchIndex;
                Match MatchPregap;
                Match MatchPostgap;
                Match MatchFlags;

                // Initialize disc
                discimage = new CDRWinDisc();
                discimage.sessions = new List<Session>();
                discimage.tracks = new List<CDRWinTrack>();
                discimage.comment = "";

                CDRWinTrack currenttrack = new CDRWinTrack();
                currenttrack.indexes = new Dictionary<int, ulong>();
                CDRWinTrackFile currentfile = new CDRWinTrackFile();
                ulong currentfileoffsetsector = 0;

                CDRWinTrack[] cuetracks;
                int track_count = 0;

                while(cueStream.Peek() >= 0)
                {
                    line++;
                    string _line = cueStream.ReadLine();

                    MatchTrack = RegexTrack.Match(_line);
                    if(MatchTrack.Success)
                    {
                        uint track_seq = uint.Parse(MatchTrack.Groups[1].Value);
                        if(track_count + 1 != track_seq)
                            throw new FeatureUnsupportedImageException(string.Format("Found TRACK {0} out of order in line {1}", track_seq, line));

                        track_count++;
                    }
                }

                if(track_count == 0)
                    throw new FeatureUnsupportedImageException("No tracks found");

                cuetracks = new CDRWinTrack[track_count];

                line = 0;
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                cueStream = new StreamReader(imageFilter.GetDataForkStream());

                FiltersList filtersList = new FiltersList();

                while(cueStream.Peek() >= 0)
                {
                    line++;
                    string _line = cueStream.ReadLine();

                    MatchSession = RegexSession.Match(_line);
                    MatchDiskType = RegexDiskType.Match(_line);
                    MatchComment = RegexComment.Match(_line);
                    MatchLBA = RegexLBA.Match(_line);   // Unhandled, just ignored
                    MatchLeadOut = RegexLeadOut.Match(_line); // Unhandled, just ignored

                    if(MatchDiskType.Success && !intrack)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM ORIGINAL MEDIA TYPE at line {0}", line);
                        discimage.disktypestr = MatchDiskType.Groups[1].Value;
                    }
                    else if(MatchDiskType.Success && intrack)
                    {
                        throw new FeatureUnsupportedImageException(string.Format("Found REM ORIGINAL MEDIA TYPE field after a track in line {0}", line));
                    }
                    else if(MatchSession.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM SESSION at line {0}", line);
                        currentsession = byte.Parse(MatchSession.Groups[1].Value);

                        // What happens between sessions
                    }
                    else if(MatchLBA.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM MSF at line {0}", line);
                        // Just ignored
                    }
                    else if(MatchLeadOut.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM LEAD-OUT at line {0}", line);
                        // Just ignored
                    }
                    else if(MatchComment.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM at line {0}", line);
                        if(discimage.comment == "")
                            discimage.comment = MatchComment.Groups[1].Value; // First comment
                        else
                            discimage.comment += Environment.NewLine + MatchComment.Groups[1].Value; // Append new comments as new lines
                    }
                    else
                    {
                        MatchTrack = RegexTrack.Match(_line);
                        MatchTitle = RegexTitle.Match(_line);
                        MatchSongWriter = RegexSongWriter.Match(_line);
                        MatchPregap = RegexPregap.Match(_line);
                        MatchPostgap = RegexPostgap.Match(_line);
                        MatchPerformer = RegexPerformer.Match(_line);
                        MatchMCN = RegexMCN.Match(_line);
                        MatchISRC = RegexISRC.Match(_line);
                        MatchIndex = RegexIndex.Match(_line);
                        MatchGenre = RegexGenre.Match(_line);
                        MatchFlags = RegexFlags.Match(_line);
                        MatchFile = RegexFile.Match(_line);
                        MatchDiskID = RegexDiskID.Match(_line);
                        MatchComposer = RegexComposer.Match(_line);
                        MatchCDText = RegexCDText.Match(_line);
                        MatchBarCode = RegexBarCode.Match(_line);
                        MatchArranger = RegexArranger.Match(_line);

                        if(MatchArranger.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found ARRANGER at line {0}", line);
                            if(intrack)
                                currenttrack.arranger = MatchArranger.Groups[1].Value;
                            else
                                discimage.arranger = MatchArranger.Groups[1].Value;
                        }
                        else if(MatchBarCode.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found UPC_EAN at line {0}", line);
                            if(!intrack)
                                discimage.barcode = MatchBarCode.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(string.Format("Found barcode field in incorrect place at line {0}", line));
                        }
                        else if(MatchCDText.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found CDTEXTFILE at line {0}", line);
                            if(!intrack)
                                discimage.cdtextfile = MatchCDText.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(string.Format("Found CD-Text file field in incorrect place at line {0}", line));
                        }
                        else if(MatchComposer.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found COMPOSER at line {0}", line);
                            if(intrack)
                                currenttrack.arranger = MatchComposer.Groups[1].Value;
                            else
                                discimage.arranger = MatchComposer.Groups[1].Value;
                        }
                        else if(MatchDiskID.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found DISC_ID at line {0}", line);
                            if(!intrack)
                                discimage.disk_id = MatchDiskID.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(string.Format("Found CDDB ID field in incorrect place at line {0}", line));
                        }
                        else if(MatchFile.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found FILE at line {0}", line);

                            if(currenttrack.sequence != 0)
                            {
                                currentfile.sequence = currenttrack.sequence;
                                currenttrack.trackfile = currentfile;
                                currenttrack.sectors = ((ulong)currentfile.datafilter.GetLength() - currentfile.offset) / CDRWinTrackTypeToBytesPerSector(currenttrack.tracktype);
                                cuetracks[currenttrack.sequence - 1] = currenttrack;
                                intrack = false;
                                currenttrack = new CDRWinTrack();
                            }


                            string datafile = MatchFile.Groups[1].Value;
                            currentfile.filetype = MatchFile.Groups[2].Value;

                            // Check if file path is quoted
                            if(datafile[0] == '"' && datafile[datafile.Length - 1] == '"')
                            {
                                datafile = datafile.Substring(1, datafile.Length - 2); // Unquote it
                            }

                            currentfile.datafilter = filtersList.GetFilter(datafile);

                            // Check if file exists
                            if(currentfile.datafilter == null)
                            {
                                if(datafile[0] == '/' || (datafile[0] == '/' && datafile[1] == '.')) // UNIX absolute path
                                {
                                    Regex unixpath = new Regex("^(.+)/([^/]+)$");
                                    Match unixpathmatch = unixpath.Match(datafile);

                                    if(unixpathmatch.Success)
                                    {
                                        currentfile.datafilter = filtersList.GetFilter(unixpathmatch.Groups[1].Value);

                                        if(currentfile.datafilter == null)
                                        {
                                            string path = imageFilter.GetParentFolder() + Path.PathSeparator + unixpathmatch.Groups[1].Value;
                                            currentfile.datafilter = filtersList.GetFilter(path);

                                            if(currentfile.datafilter == null)
                                                throw new FeatureUnsupportedImageException(string.Format("File \"{0}\" not found.", MatchFile.Groups[1].Value));
                                        }
                                    }
                                    else
                                        throw new FeatureUnsupportedImageException(string.Format("File \"{0}\" not found.", MatchFile.Groups[1].Value));
                                }
                                else if((datafile[1] == ':' && datafile[2] == '\\') ||
                                        (datafile[0] == '\\' && datafile[1] == '\\') ||
                                        ((datafile[0] == '.' && datafile[1] == '\\'))) // Windows absolute path
                                {
                                    Regex winpath = new Regex("^(?:[a-zA-Z]\\:(\\\\|\\/)|file\\:\\/\\/|\\\\\\\\|\\.(\\/|\\\\))([^\\\\\\/\\:\\*\\?\\<\\>\\\"\\|]+(\\\\|\\/){0,1})+$");
                                    Match winpathmatch = winpath.Match(datafile);
                                    if(winpathmatch.Success)
                                    {
                                        currentfile.datafilter = filtersList.GetFilter(winpathmatch.Groups[1].Value);

                                        if(currentfile.datafilter == null)
                                        {
                                            string path = imageFilter.GetParentFolder() + Path.PathSeparator + winpathmatch.Groups[1].Value;
                                            currentfile.datafilter = filtersList.GetFilter(path);

                                            if(currentfile.datafilter == null)
                                                throw new FeatureUnsupportedImageException(string.Format("File \"{0}\" not found.", MatchFile.Groups[1].Value));
                                        }
                                    }
                                    else
                                        throw new FeatureUnsupportedImageException(string.Format("File \"{0}\" not found.", MatchFile.Groups[1].Value));
                                }
                                else
                                {
                                    string path = imageFilter.GetParentFolder() + Path.PathSeparator + datafile;
                                    currentfile.datafilter = filtersList.GetFilter(path);

                                    if(currentfile.datafilter == null)
                                        throw new FeatureUnsupportedImageException(string.Format("File \"{0}\" not found.", MatchFile.Groups[1].Value));
                                }
                            }

                            // File does exist, process it
                            DicConsole.DebugWriteLine("CDRWin plugin", "File \"{0}\" found", currentfile.datafilter);

                            switch(currentfile.filetype)
                            {
                                case CDRWinDiskTypeLittleEndian:
                                    break;
                                case CDRWinDiskTypeBigEndian:
                                case CDRWinDiskTypeAIFF:
                                case CDRWinDiskTypeRIFF:
                                case CDRWinDiskTypeMP3:
                                    throw new FeatureSupportedButNotImplementedImageException(string.Format("Unsupported file type {0}", currentfile.filetype));
                                default:
                                    throw new FeatureUnsupportedImageException(string.Format("Unknown file type {0}", currentfile.filetype));
                            }

                            currentfile.offset = 0;
                            currentfile.sequence = 0;
                        }
                        else if(MatchFlags.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found FLAGS at line {0}", line);
                            if(!intrack)
                                throw new FeatureUnsupportedImageException(string.Format("Found FLAGS field in incorrect place at line {0}", line));

                            currenttrack.flag_dcp |= MatchFile.Groups["dcp"].Value == "DCP";
                            currenttrack.flag_4ch |= MatchFile.Groups["quad"].Value == "4CH";
                            currenttrack.flag_pre |= MatchFile.Groups["pre"].Value == "PRE";
                            currenttrack.flag_scms |= MatchFile.Groups["scms"].Value == "SCMS";
                        }
                        else if(MatchGenre.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found GENRE at line {0}", line);
                            if(intrack)
                                currenttrack.genre = MatchGenre.Groups[1].Value;
                            else
                                discimage.genre = MatchGenre.Groups[1].Value;
                        }
                        else if(MatchIndex.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found INDEX at line {0}", line);
                            if(!intrack)
                                throw new FeatureUnsupportedImageException(string.Format("Found INDEX before a track {0}", line));
                            else
                            {
                                int index = int.Parse(MatchIndex.Groups[1].Value);
                                ulong offset = CDRWinMSFToLBA(MatchIndex.Groups[2].Value);

                                if((index != 0 && index != 1) && currenttrack.indexes.Count == 0)
                                    throw new FeatureUnsupportedImageException(string.Format("Found INDEX {0} before INDEX 00 or INDEX 01", index));

                                if((index == 0 || (index == 1 && !currenttrack.indexes.ContainsKey(0))))
                                {
                                    if((int)(currenttrack.sequence - 2) >= 0 && offset > 1)
                                    {
                                        cuetracks[currenttrack.sequence - 2].sectors = offset - currentfileoffsetsector;
                                        currentfile.offset += cuetracks[currenttrack.sequence - 2].sectors * cuetracks[currenttrack.sequence - 2].bps;
                                        DicConsole.DebugWriteLine("CDRWin plugin", "Sets currentfile.offset to {0} at line 553", currentfile.offset);
                                        DicConsole.DebugWriteLine("CDRWin plugin", "cuetracks[currenttrack.sequence-2].sectors = {0}", cuetracks[currenttrack.sequence - 2].sectors);
                                        DicConsole.DebugWriteLine("CDRWin plugin", "cuetracks[currenttrack.sequence-2].bps = {0}", cuetracks[currenttrack.sequence - 2].bps);
                                    }
                                }

                                if((index == 0 || (index == 1 && !currenttrack.indexes.ContainsKey(0))) && currenttrack.sequence == 1)
                                {
                                    DicConsole.DebugWriteLine("CDRWin plugin", "Sets currentfile.offset to {0} at line 559", offset * currenttrack.bps);
                                    currentfile.offset = offset * currenttrack.bps;
                                }

                                currentfileoffsetsector = offset;
                                currenttrack.indexes.Add(index, offset);
                            }
                        }
                        else if(MatchISRC.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found ISRC at line {0}", line);
                            if(!intrack)
                                throw new FeatureUnsupportedImageException(string.Format("Found ISRC before a track {0}", line));
                            currenttrack.isrc = MatchISRC.Groups[1].Value;
                        }
                        else if(MatchMCN.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found CATALOG at line {0}", line);
                            if(!intrack)
                                discimage.mcn = MatchMCN.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(string.Format("Found CATALOG field in incorrect place at line {0}", line));
                        }
                        else if(MatchPerformer.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found PERFORMER at line {0}", line);
                            if(intrack)
                                currenttrack.performer = MatchPerformer.Groups[1].Value;
                            else
                                discimage.performer = MatchPerformer.Groups[1].Value;
                        }
                        else if(MatchPostgap.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found POSTGAP at line {0}", line);
                            if(intrack)
                            {
                                currenttrack.postgap = CDRWinMSFToLBA(MatchPostgap.Groups[1].Value);
                            }
                            else
                                throw new FeatureUnsupportedImageException(string.Format("Found POSTGAP field before a track at line {0}", line));
                        }
                        else if(MatchPregap.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found PREGAP at line {0}", line);
                            if(intrack)
                            {
                                currenttrack.pregap = CDRWinMSFToLBA(MatchPregap.Groups[1].Value);
                            }
                            else
                                throw new FeatureUnsupportedImageException(string.Format("Found PREGAP field before a track at line {0}", line));
                        }
                        else if(MatchSongWriter.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found SONGWRITER at line {0}", line);
                            if(intrack)
                                currenttrack.songwriter = MatchSongWriter.Groups[1].Value;
                            else
                                discimage.songwriter = MatchSongWriter.Groups[1].Value;
                        }
                        else if(MatchTitle.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found TITLE at line {0}", line);
                            if(intrack)
                                currenttrack.title = MatchTitle.Groups[1].Value;
                            else
                                discimage.title = MatchTitle.Groups[1].Value;
                        }
                        else if(MatchTrack.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found TRACK at line {0}", line);
                            if(currentfile.datafilter == null)
                                throw new FeatureUnsupportedImageException(string.Format("Found TRACK field before a file is defined at line {0}", line));
                            if(intrack)
                            {
                                if(currenttrack.indexes.ContainsKey(0) && currenttrack.pregap == 0)
                                {
                                    currenttrack.indexes.TryGetValue(0, out currenttrack.pregap);
                                }
                                currentfile.sequence = currenttrack.sequence;
                                currenttrack.trackfile = currentfile;
                                cuetracks[currenttrack.sequence - 1] = currenttrack;
                            }
                            currenttrack = new CDRWinTrack();
                            currenttrack.indexes = new Dictionary<int, ulong>();
                            currenttrack.sequence = uint.Parse(MatchTrack.Groups[1].Value);
                            DicConsole.DebugWriteLine("CDRWin plugin", "Setting currenttrack.sequence to {0}", currenttrack.sequence);
                            currentfile.sequence = currenttrack.sequence;
                            currenttrack.bps = CDRWinTrackTypeToBytesPerSector(MatchTrack.Groups[2].Value);
                            currenttrack.tracktype = MatchTrack.Groups[2].Value;
                            currenttrack.session = currentsession;
                            intrack = true;
                        }
                        else if(_line == "") // Empty line, ignore it
                        {

                        }
                        else // Non-empty unknown field
                        {
                            throw new FeatureUnsupportedImageException(string.Format("Found unknown field defined at line {0}: \"{1}\"", line, _line));
                        }
                    }
                }

                if(currenttrack.sequence != 0)
                {
                    currentfile.sequence = currenttrack.sequence;
                    currenttrack.trackfile = currentfile;
                    currenttrack.sectors = ((ulong)currentfile.datafilter.GetLength() - currentfile.offset) / CDRWinTrackTypeToBytesPerSector(currenttrack.tracktype);
                    cuetracks[currenttrack.sequence - 1] = currenttrack;
                }

                Session[] _sessions = new Session[currentsession];
                for(int s = 1; s <= _sessions.Length; s++)
                {
                    _sessions[s - 1].SessionSequence = 1;

                    if(s > 1)
                        _sessions[s - 1].StartSector = _sessions[s - 2].EndSector + 1;
                    else
                        _sessions[s - 1].StartSector = 0;

                    ulong session_sectors = 0;
                    int last_session_track = 0;

                    for(int i = 0; i < cuetracks.Length; i++)
                    {
                        if(cuetracks[i].session == s)
                        {
                            session_sectors += cuetracks[i].sectors;
                            if(i > last_session_track)
                                last_session_track = i;
                        }
                    }

                    _sessions[s - 1].EndTrack = cuetracks[last_session_track].sequence;
                    _sessions[s - 1].EndSector = session_sectors - 1;
                }

                for(int s = 1; s <= _sessions.Length; s++)
                    discimage.sessions.Add(_sessions[s - 1]);

                for(int t = 1; t <= cuetracks.Length; t++)
                    discimage.tracks.Add(cuetracks[t - 1]);

                discimage.disktype = CDRWinIsoBusterDiscTypeToMediaType(discimage.disktypestr);

                if(discimage.disktype == MediaType.Unknown || discimage.disktype == MediaType.CD)
                {
                    bool data = false;
                    bool cdg = false;
                    bool cdi = false;
                    bool mode2 = false;
                    bool firstaudio = false;
                    bool firstdata = false;
                    bool audio = false;

                    for(int i = 0; i < discimage.tracks.Count; i++)
                    {
                        // First track is audio
                        firstaudio |= i == 0 && discimage.tracks[i].tracktype == CDRWinTrackTypeAudio;

                        // First track is data
                        firstdata |= i == 0 && discimage.tracks[i].tracktype != CDRWinTrackTypeAudio;

                        // Any non first track is data
                        data |= i != 0 && discimage.tracks[i].tracktype != CDRWinTrackTypeAudio;

                        // Any non first track is audio
                        audio |= i != 0 && discimage.tracks[i].tracktype == CDRWinTrackTypeAudio;

                        switch(discimage.tracks[i].tracktype)
                        {
                            case CDRWinTrackTypeCDG:
                                cdg = true;
                                break;
                            case CDRWinTrackTypeCDI:
                            case CDRWinTrackTypeCDIRaw:
                                cdi = true;
                                break;
                            case CDRWinTrackTypeMode2Form1:
                            case CDRWinTrackTypeMode2Form2:
                            case CDRWinTrackTypeMode2Formless:
                            case CDRWinTrackTypeMode2Raw:
                                mode2 = true;
                                break;
                        }
                    }

                    if(!data && !firstdata)
                        discimage.disktype = MediaType.CDDA;
                    else if(cdg)
                        discimage.disktype = MediaType.CDG;
                    else if(cdi)
                        discimage.disktype = MediaType.CDI;
                    else if(firstaudio && data && discimage.sessions.Count > 1 && mode2)
                        discimage.disktype = MediaType.CDPLUS;
                    else if((firstdata && audio) || mode2)
                        discimage.disktype = MediaType.CDROMXA;
                    else if(!audio)
                        discimage.disktype = MediaType.CDROM;
                    else
                        discimage.disktype = MediaType.CD;
                }

                // DEBUG information
                DicConsole.DebugWriteLine("CDRWin plugin", "Disc image parsing results");
                DicConsole.DebugWriteLine("CDRWin plugin", "Disc CD-TEXT:");
                if(discimage.arranger == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tArranger is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tArranger: {0}", discimage.arranger);
                if(discimage.composer == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tComposer is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tComposer: {0}", discimage.composer);
                if(discimage.genre == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tGenre is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tGenre: {0}", discimage.genre);
                if(discimage.performer == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPerformer is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPerformer: {0}", discimage.performer);
                if(discimage.songwriter == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tSongwriter is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tSongwriter: {0}", discimage.songwriter);
                if(discimage.title == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tTitle is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tTitle: {0}", discimage.title);
                if(discimage.cdtextfile == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tCD-TEXT binary file not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tCD-TEXT binary file: {0}", discimage.cdtextfile);
                DicConsole.DebugWriteLine("CDRWin plugin", "Disc information:");
                if(discimage.disktypestr == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tISOBuster disc type not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tISOBuster disc type: {0}", discimage.disktypestr);
                DicConsole.DebugWriteLine("CDRWin plugin", "\tGuessed disk type: {0}", discimage.disktype);
                if(discimage.barcode == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tBarcode not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tBarcode: {0}", discimage.barcode);
                if(discimage.disk_id == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc ID not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc ID: {0}", discimage.disk_id);
                if(discimage.mcn == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tMCN not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tMCN: {0}", discimage.mcn);
                if(discimage.comment == null)
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tComment not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tComment: \"{0}\"", discimage.comment);
                DicConsole.DebugWriteLine("CDRWin plugin", "Session information:");
                DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc contains {0} sessions", discimage.sessions.Count);
                for(int i = 0; i < discimage.sessions.Count; i++)
                {
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tSession {0} information:", i + 1);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tStarting track: {0}", discimage.sessions[i].StartTrack);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tStarting sector: {0}", discimage.sessions[i].StartSector);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tEnding track: {0}", discimage.sessions[i].EndTrack);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tEnding sector: {0}", discimage.sessions[i].EndSector);
                }
                DicConsole.DebugWriteLine("CDRWin plugin", "Track information:");
                DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc contains {0} tracks", discimage.tracks.Count);
                for(int i = 0; i < discimage.tracks.Count; i++)
                {
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tTrack {0} information:", discimage.tracks[i].sequence);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\t{0} bytes per sector", discimage.tracks[i].bps);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPregap: {0} sectors", discimage.tracks[i].pregap);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tData: {0} sectors", discimage.tracks[i].sectors);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPostgap: {0} sectors", discimage.tracks[i].postgap);

                    if(discimage.tracks[i].flag_4ch)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack is flagged as quadraphonic");
                    if(discimage.tracks[i].flag_dcp)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack allows digital copy");
                    if(discimage.tracks[i].flag_pre)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack has pre-emphasis applied");
                    if(discimage.tracks[i].flag_scms)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack has SCMS");

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack resides in file {0}, type defined as {1}, starting at byte {2}",
                        discimage.tracks[i].trackfile.datafilter, discimage.tracks[i].trackfile.filetype, discimage.tracks[i].trackfile.offset);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tIndexes:");
                    foreach(KeyValuePair<int, ulong> kvp in discimage.tracks[i].indexes)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\t\tIndex {0} starts at sector {1}", kvp.Key, kvp.Value);

                    if(discimage.tracks[i].isrc == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tISRC is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tISRC: {0}", discimage.tracks[i].isrc);

                    if(discimage.tracks[i].arranger == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tArranger is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tArranger: {0}", discimage.tracks[i].arranger);
                    if(discimage.tracks[i].composer == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tComposer is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tComposer: {0}", discimage.tracks[i].composer);
                    if(discimage.tracks[i].genre == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tGenre is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tGenre: {0}", discimage.tracks[i].genre);
                    if(discimage.tracks[i].performer == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPerformer is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPerformer: {0}", discimage.tracks[i].performer);
                    if(discimage.tracks[i].songwriter == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tSongwriter is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tSongwriter: {0}", discimage.tracks[i].songwriter);
                    if(discimage.tracks[i].title == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTitle is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTitle: {0}", discimage.tracks[i].title);
                }

                DicConsole.DebugWriteLine("CDRWin plugin", "Building offset map");

                partitions = new List<Partition>();

                ulong byte_offset = 0;
                ulong sector_offset = 0;
                ulong partitionSequence = 0;
                ulong index_zero_offset = 0;
                ulong index_one_offset = 0;
                bool index_zero = false;

                offsetmap = new Dictionary<uint, ulong>();

                for(int i = 0; i < discimage.tracks.Count; i++)
                {
                    ulong index0_len = 0;

                    if(discimage.tracks[i].sequence == 1 && i != 0)
                        throw new ImageNotSupportedException("Unordered tracks");

                    Partition partition = new Partition();

                    /*if(discimage.tracks[i].pregap > 0)
                    {
                        partition.PartitionDescription = string.Format("Track {0} pregap.", discimage.tracks[i].sequence);
                        partition.PartitionName = discimage.tracks[i].title;
                        partition.PartitionStartSector = sector_offset;
                        partition.PartitionLength = discimage.tracks[i].pregap * discimage.tracks[i].bps;
                        partition.PartitionSectors = discimage.tracks[i].pregap;
                        partition.PartitionSequence = partitionSequence;
                        partition.PartitionStart = byte_offset;
                        partition.PartitionType = discimage.tracks[i].tracktype;

                        sector_offset += partition.PartitionSectors;
                        byte_offset += partition.PartitionLength;
                        partitionSequence++;

                        if(!offsetmap.ContainsKey(discimage.tracks[i].sequence))
                            offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                        else
                        {
                            ulong old_start;
                            offsetmap.TryGetValue(discimage.tracks[i].sequence, out old_start);

                            if(partition.PartitionStartSector < old_start)
                            {
                                offsetmap.Remove(discimage.tracks[i].sequence);
                                offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                            }
                        }

                        partitions.Add(partition);
                        partition = new Partition();
                    }*/

                    index_zero |= discimage.tracks[i].indexes.TryGetValue(0, out index_zero_offset);

                    if(!discimage.tracks[i].indexes.TryGetValue(1, out index_one_offset))
                        throw new ImageNotSupportedException(string.Format("Track {0} lacks index 01", discimage.tracks[i].sequence));

                    /*if(index_zero && index_one_offset > index_zero_offset)
                    {
                        partition.PartitionDescription = string.Format("Track {0} index 00.", discimage.tracks[i].sequence);
                        partition.PartitionName = discimage.tracks[i].title;
                        partition.PartitionStartSector = sector_offset;
                        partition.PartitionLength = (index_one_offset - index_zero_offset) * discimage.tracks[i].bps;
                        partition.PartitionSectors = index_one_offset - index_zero_offset;
                        partition.PartitionSequence = partitionSequence;
                        partition.PartitionStart = byte_offset;
                        partition.PartitionType = discimage.tracks[i].tracktype;

                        sector_offset += partition.PartitionSectors;
                        byte_offset += partition.PartitionLength;
                        index0_len = partition.PartitionSectors;
                        partitionSequence++;

                        if(!offsetmap.ContainsKey(discimage.tracks[i].sequence))
                            offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                        else
                        {
                            ulong old_start;
                            offsetmap.TryGetValue(discimage.tracks[i].sequence, out old_start);

                            if(partition.PartitionStartSector < old_start)
                            {
                                offsetmap.Remove(discimage.tracks[i].sequence);
                                offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                            }
                        }

                        partitions.Add(partition);
                        partition = new Partition();
                    }*/

                    // Index 01
                    partition.PartitionDescription = string.Format("Track {0}.", discimage.tracks[i].sequence);
                    partition.PartitionName = discimage.tracks[i].title;
                    partition.PartitionStartSector = sector_offset;
                    partition.PartitionLength = (discimage.tracks[i].sectors - index0_len) * discimage.tracks[i].bps;
                    partition.PartitionSectors = (discimage.tracks[i].sectors - index0_len);
                    partition.PartitionSequence = partitionSequence;
                    partition.PartitionStart = byte_offset;
                    partition.PartitionType = discimage.tracks[i].tracktype;

                    sector_offset += partition.PartitionSectors;
                    byte_offset += partition.PartitionLength;
                    partitionSequence++;

                    if(!offsetmap.ContainsKey(discimage.tracks[i].sequence))
                        offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                    else
                    {
                        ulong old_start;
                        offsetmap.TryGetValue(discimage.tracks[i].sequence, out old_start);

                        if(partition.PartitionStartSector < old_start)
                        {
                            offsetmap.Remove(discimage.tracks[i].sequence);
                            offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                        }
                    }

                    partitions.Add(partition);
                    partition = new Partition();
                }

                // Print offset map
                DicConsole.DebugWriteLine("CDRWin plugin", "printing partition map");
                foreach(Partition partition in partitions)
                {
                    DicConsole.DebugWriteLine("CDRWin plugin", "Partition sequence: {0}", partition.PartitionSequence);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition name: {0}", partition.PartitionName);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition description: {0}", partition.PartitionDescription);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition type: {0}", partition.PartitionType);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition starting sector: {0}", partition.PartitionStartSector);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition sectors: {0}", partition.PartitionSectors);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition starting offset: {0}", partition.PartitionStart);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition size in bytes: {0}", partition.PartitionLength);
                }

                foreach(CDRWinTrack track in discimage.tracks)
                    ImageInfo.imageSize += track.bps * track.sectors;
                foreach(CDRWinTrack track in discimage.tracks)
                    ImageInfo.sectors += track.sectors;

                if(discimage.disktype == MediaType.CDG || discimage.disktype == MediaType.CDEG || discimage.disktype == MediaType.CDMIDI)
                    ImageInfo.sectorSize = 2448; // CD+G subchannels ARE user data, as CD+G are useless without them
                else if(discimage.disktype != MediaType.CDROMXA && discimage.disktype != MediaType.CDDA && discimage.disktype != MediaType.CDI && discimage.disktype != MediaType.CDPLUS)
                    ImageInfo.sectorSize = 2048; // Only data tracks
                else
                    ImageInfo.sectorSize = 2352; // All others

                if(discimage.mcn != null)
                    ImageInfo.readableMediaTags.Add(MediaTagType.CD_MCN);
                if(discimage.cdtextfile != null)
                    ImageInfo.readableMediaTags.Add(MediaTagType.CD_TEXT);

                // Detect ISOBuster extensions
                if(discimage.disktypestr != null || discimage.comment.ToLower().Contains("isobuster") || discimage.sessions.Count > 1)
                    ImageInfo.imageApplication = "ISOBuster";
                else
                    ImageInfo.imageApplication = "CDRWin";

                ImageInfo.imageCreationTime = imageFilter.GetCreationTime();
                ImageInfo.imageLastModificationTime = imageFilter.GetLastWriteTime();

                ImageInfo.imageComments = discimage.comment;
                ImageInfo.mediaSerialNumber = discimage.mcn;
                ImageInfo.mediaBarcode = discimage.barcode;
                ImageInfo.mediaType = discimage.disktype;

                ImageInfo.readableSectorTags.Add(SectorTagType.CDTrackFlags);

                foreach(CDRWinTrack track in discimage.tracks)
                {
                    switch(track.tracktype)
                    {
                        case CDRWinTrackTypeAudio:
                            {
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDTrackISRC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDTrackISRC);
                                break;
                            }
                        case CDRWinTrackTypeCDG:
                            {
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDTrackISRC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDTrackISRC);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubchannel))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubchannel);
                                break;
                            }
                        case CDRWinTrackTypeMode2Formless:
                        case CDRWinTrackTypeCDI:
                            {
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorSubHeader))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorSubHeader);
                                if(!ImageInfo.readableSectorTags.Contains(SectorTagType.CDSectorEDC))
                                    ImageInfo.readableSectorTags.Add(SectorTagType.CDSectorEDC);
                                break;
                            }
                        case CDRWinTrackTypeMode2Raw:
                        case CDRWinTrackTypeCDIRaw:
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
                        case CDRWinTrackTypeMode1Raw:
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

                DicConsole.VerboseWriteLine("CDRWIN image describes a disc of type {0}", ImageInfo.mediaType);
                if(!string.IsNullOrEmpty(ImageInfo.imageComments))
                    DicConsole.VerboseWriteLine("CDRWIN comments: {0}", ImageInfo.imageComments);

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
                case MediaTagType.CD_TEXT:
                    {
                        if(discimage.cdtextfile != null)
                            // TODO: Check that binary text file exists, open it, read it, send it to caller.
                            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
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
                    foreach(CDRWinTrack cdrwin_track in discimage.tracks)
                    {
                        if(cdrwin_track.sequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < cdrwin_track.sectors)
                                return ReadSectors((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(CDRWinTrack cdrwin_track in discimage.tracks)
                    {
                        if(cdrwin_track.sequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < cdrwin_track.sectors)
                                return ReadSectorsTag((sectorAddress - kvp.Value), length, kvp.Key, tag);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            CDRWinTrack _track = new CDRWinTrack();

            _track.sequence = 0;

            foreach(CDRWinTrack cdrwin_track in discimage.tracks)
            {
                if(cdrwin_track.sequence == track)
                {
                    _track = cdrwin_track;
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
                case CDRWinTrackTypeMode1:
                case CDRWinTrackTypeMode2Form1:
                    {
                        sector_offset = 0;
                        sector_size = 2048;
                        sector_skip = 0;
                        break;
                    }
                case CDRWinTrackTypeMode2Form2:
                    {
                        sector_offset = 0;
                        sector_size = 2324;
                        sector_skip = 0;
                        break;
                    }
                case CDRWinTrackTypeMode2Formless:
                case CDRWinTrackTypeCDI:
                    {
                        sector_offset = 0;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                case CDRWinTrackTypeAudio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                case CDRWinTrackTypeMode1Raw:
                    {
                        sector_offset = 16;
                        sector_size = 2048;
                        sector_skip = 288;
                        break;
                    }
                case CDRWinTrackTypeMode2Raw:
                    {
                        sector_offset = 16;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                case CDRWinTrackTypeCDIRaw:
                    {
                        sector_offset = 16;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                case CDRWinTrackTypeCDG:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 96;
                        break;
                    }
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

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            CDRWinTrack _track = new CDRWinTrack();

            _track.sequence = 0;

            foreach(CDRWinTrack cdrwin_track in discimage.tracks)
            {
                if(cdrwin_track.sequence == track)
                {
                    _track = cdrwin_track;
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

                        if(_track.tracktype != CDRWinTrackTypeAudio && _track.tracktype != CDRWinTrackTypeCDG)
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
                case SectorTagType.CDTrackText:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                default:
                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(_track.tracktype)
            {
                case CDRWinTrackTypeMode1:
                case CDRWinTrackTypeMode2Form1:
                case CDRWinTrackTypeMode2Form2:
                    throw new ArgumentException("No tags in image for requested track", nameof(tag));
                case CDRWinTrackTypeMode2Formless:
                case CDRWinTrackTypeCDI:
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
                case CDRWinTrackTypeAudio:
                    throw new ArgumentException("There are no tags on audio tracks", nameof(tag));
                case CDRWinTrackTypeMode1Raw:
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
                case CDRWinTrackTypeMode2Raw: // Requires reading sector
                case CDRWinTrackTypeCDIRaw:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                case CDRWinTrackTypeCDG:
                    {
                        if(tag != SectorTagType.CDSectorSubchannel)
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));

                        sector_offset = 2352;
                        sector_size = 96;
                        sector_skip = 0;
                        break;
                    }
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
                    foreach(CDRWinTrack cdrwin_track in discimage.tracks)
                    {
                        if(cdrwin_track.sequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < cdrwin_track.sectors)
                                return ReadSectorsLong((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            CDRWinTrack _track = new CDRWinTrack();

            _track.sequence = 0;

            foreach(CDRWinTrack cdrwin_track in discimage.tracks)
            {
                if(cdrwin_track.sequence == track)
                {
                    _track = cdrwin_track;
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
                case CDRWinTrackTypeMode1:
                case CDRWinTrackTypeMode2Form1:
                    {
                        sector_offset = 0;
                        sector_size = 2048;
                        sector_skip = 0;
                        break;
                    }
                case CDRWinTrackTypeMode2Form2:
                    {
                        sector_offset = 0;
                        sector_size = 2324;
                        sector_skip = 0;
                        break;
                    }
                case CDRWinTrackTypeMode2Formless:
                case CDRWinTrackTypeCDI:
                    {
                        sector_offset = 0;
                        sector_size = 2336;
                        sector_skip = 0;
                        break;
                    }
                case CDRWinTrackTypeMode1Raw:
                case CDRWinTrackTypeMode2Raw:
                case CDRWinTrackTypeCDIRaw:
                case CDRWinTrackTypeAudio:
                    {
                        sector_offset = 0;
                        sector_size = 2352;
                        sector_skip = 0;
                        break;
                    }
                case CDRWinTrackTypeCDG:
                    {
                        sector_offset = 0;
                        sector_size = 2448;
                        sector_skip = 0;
                        break;
                    }
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

        public override string GetImageFormat()
        {
            return "CDRWin CUESheet";
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

            ulong previousStartSector = 0;

            foreach(CDRWinTrack cdr_track in discimage.tracks)
            {
                Track _track = new Track();

                _track.Indexes = cdr_track.indexes;
                _track.TrackDescription = cdr_track.title;
                if(!cdr_track.indexes.TryGetValue(0, out _track.TrackStartSector))
                    cdr_track.indexes.TryGetValue(1, out _track.TrackStartSector);
                _track.TrackStartSector = previousStartSector;
                _track.TrackEndSector = _track.TrackStartSector + cdr_track.sectors - 1;
                _track.TrackPregap = cdr_track.pregap;
                _track.TrackSession = cdr_track.session;
                _track.TrackSequence = cdr_track.sequence;
                _track.TrackType = CDRWinTrackTypeToTrackType(cdr_track.tracktype);
                _track.TrackFile = cdr_track.trackfile.datafilter.GetFilename();
                _track.TrackFilter = cdr_track.trackfile.datafilter;
                _track.TrackFileOffset = cdr_track.trackfile.offset;
                _track.TrackFileType = cdr_track.trackfile.filetype;
                _track.TrackRawBytesPerSector = cdr_track.bps;
                _track.TrackBytesPerSector = CDRWinTrackTypeToCookedBytesPerSector(cdr_track.tracktype);
                if(cdr_track.bps == 2448)
                {
                    _track.TrackSubchannelFilter = cdr_track.trackfile.datafilter;
                    _track.TrackSubchannelFile = cdr_track.trackfile.datafilter.GetFilename();
                    _track.TrackSubchannelOffset = cdr_track.trackfile.offset;
                    _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                }
                else
                    _track.TrackSubchannelType = TrackSubchannelType.None;

                tracks.Add(_track);
                previousStartSector = _track.TrackEndSector + 1;
            }

            return tracks;
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            if(discimage.sessions.Contains(session))
            {
                return GetSessionTracks(session.SessionSequence);
            }
            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            List<Track> tracks = new List<Track>();

            foreach(CDRWinTrack cdr_track in discimage.tracks)
            {
                if(cdr_track.session == session)
                {
                    Track _track = new Track();

                    _track.Indexes = cdr_track.indexes;
                    _track.TrackDescription = cdr_track.title;
                    if(!cdr_track.indexes.TryGetValue(0, out _track.TrackStartSector))
                        cdr_track.indexes.TryGetValue(1, out _track.TrackStartSector);
                    _track.TrackEndSector = _track.TrackStartSector + cdr_track.sectors - 1;
                    _track.TrackPregap = cdr_track.pregap;
                    _track.TrackSession = cdr_track.session;
                    _track.TrackSequence = cdr_track.sequence;
                    _track.TrackType = CDRWinTrackTypeToTrackType(cdr_track.tracktype);
                    _track.TrackFile = cdr_track.trackfile.datafilter.GetFilename();
                    _track.TrackFilter = cdr_track.trackfile.datafilter;
                    _track.TrackFileOffset = cdr_track.trackfile.offset;
                    _track.TrackFileType = cdr_track.trackfile.filetype;
                    _track.TrackRawBytesPerSector = cdr_track.bps;
                    _track.TrackBytesPerSector = CDRWinTrackTypeToCookedBytesPerSector(cdr_track.tracktype);
                    if(cdr_track.bps == 2448)
                    {
                        _track.TrackSubchannelFilter = cdr_track.trackfile.datafilter;
                        _track.TrackSubchannelFile = cdr_track.trackfile.datafilter.GetFilename();
                        _track.TrackSubchannelOffset = cdr_track.trackfile.offset;
                        _track.TrackSubchannelType = TrackSubchannelType.RawInterleaved;
                    }
                    else
                        _track.TrackSubchannelType = TrackSubchannelType.None;

                    tracks.Add(_track);
                }
            }

            return tracks;
        }

        public override List<Session> GetSessions()
        {
            return discimage.sessions;
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

        #endregion

        #region Private methods

        static ulong CDRWinMSFToLBA(string MSF)
        {
            string[] MSFElements;
            ulong minute, second, frame, sectors;

            MSFElements = MSF.Split(':');
            minute = ulong.Parse(MSFElements[0]);
            second = ulong.Parse(MSFElements[1]);
            frame = ulong.Parse(MSFElements[2]);

            sectors = (minute * 60 * 75) + (second * 75) + frame;

            return sectors;
        }

        static ushort CDRWinTrackTypeToBytesPerSector(string trackType)
        {
            switch(trackType)
            {
                case CDRWinTrackTypeMode1:
                case CDRWinTrackTypeMode2Form1:
                    return 2048;
                case CDRWinTrackTypeMode2Form2:
                    return 2324;
                case CDRWinTrackTypeMode2Formless:
                case CDRWinTrackTypeCDI:
                    return 2336;
                case CDRWinTrackTypeAudio:
                case CDRWinTrackTypeMode1Raw:
                case CDRWinTrackTypeMode2Raw:
                case CDRWinTrackTypeCDIRaw:
                    return 2352;
                case CDRWinTrackTypeCDG:
                    return 2448;
                default:
                    return 0;
            }
        }

        static ushort CDRWinTrackTypeToCookedBytesPerSector(string trackType)
        {
            switch(trackType)
            {
                case CDRWinTrackTypeMode1:
                case CDRWinTrackTypeMode2Form1:
                case CDRWinTrackTypeMode1Raw:
                    return 2048;
                case CDRWinTrackTypeMode2Form2:
                    return 2324;
                case CDRWinTrackTypeMode2Formless:
                case CDRWinTrackTypeCDI:
                case CDRWinTrackTypeMode2Raw:
                case CDRWinTrackTypeCDIRaw:
                    return 2336;
                case CDRWinTrackTypeAudio:
                    return 2352;
                case CDRWinTrackTypeCDG:
                    return 2448;
                default:
                    return 0;
            }
        }

        static TrackType CDRWinTrackTypeToTrackType(string trackType)
        {
            switch(trackType)
            {
                case CDRWinTrackTypeMode1:
                case CDRWinTrackTypeMode1Raw:
                    return TrackType.CDMode1;
                case CDRWinTrackTypeMode2Form1:
                    return TrackType.CDMode2Form1;
                case CDRWinTrackTypeMode2Form2:
                    return TrackType.CDMode2Form2;
                case CDRWinTrackTypeCDIRaw:
                case CDRWinTrackTypeCDI:
                case CDRWinTrackTypeMode2Raw:
                case CDRWinTrackTypeMode2Formless:
                    return TrackType.CDMode2Formless;
                case CDRWinTrackTypeAudio:
                case CDRWinTrackTypeCDG:
                    return TrackType.Audio;
                default:
                    return TrackType.Data;
            }
        }

        static MediaType CDRWinIsoBusterDiscTypeToMediaType(string discType)
        {
            switch(discType)
            {
                case CDRWinDiskTypeCD:
                    return MediaType.CD;
                case CDRWinDiskTypeCDRW:
                case CDRWinDiskTypeCDMRW:
                case CDRWinDiskTypeCDMRW2:
                    return MediaType.CDRW;
                case CDRWinDiskTypeDVD:
                    return MediaType.DVDROM;
                case CDRWinDiskTypeDVDPRW:
                case CDRWinDiskTypeDVDPMRW:
                case CDRWinDiskTypeDVDPMRW2:
                    return MediaType.DVDPRW;
                case CDRWinDiskTypeDVDPRWDL:
                case CDRWinDiskTypeDVDPMRWDL:
                case CDRWinDiskTypeDVDPMRWDL2:
                    return MediaType.DVDPRWDL;
                case CDRWinDiskTypeDVDPR:
                case CDRWinDiskTypeDVDPVR:
                    return MediaType.DVDPR;
                case CDRWinDiskTypeDVDPRDL:
                    return MediaType.DVDPRDL;
                case CDRWinDiskTypeDVDRAM:
                    return MediaType.DVDRAM;
                case CDRWinDiskTypeDVDVR:
                case CDRWinDiskTypeDVDR:
                    return MediaType.DVDR;
                case CDRWinDiskTypeDVDRDL:
                    return MediaType.DVDRDL;
                case CDRWinDiskTypeDVDRW:
                case CDRWinDiskTypeDVDRWDL:
                case CDRWinDiskTypeDVDRW2:
                    return MediaType.DVDRW;
                case CDRWinDiskTypeHDDVD:
                    return MediaType.HDDVDROM;
                case CDRWinDiskTypeHDDVDRAM:
                    return MediaType.HDDVDRAM;
                case CDRWinDiskTypeHDDVDR:
                case CDRWinDiskTypeHDDVDRDL:
                    return MediaType.HDDVDR;
                case CDRWinDiskTypeHDDVDRW:
                case CDRWinDiskTypeHDDVDRWDL:
                    return MediaType.HDDVDRW;
                case CDRWinDiskTypeBD:
                    return MediaType.BDROM;
                case CDRWinDiskTypeBDR:
                case CDRWinDiskTypeBDRDL:
                    return MediaType.BDR;
                case CDRWinDiskTypeBDRE:
                case CDRWinDiskTypeBDREDL:
                    return MediaType.BDRE;
                default:
                    return MediaType.Unknown;
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