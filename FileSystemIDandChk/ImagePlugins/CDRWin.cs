/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : CDRWin.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Disc image plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Manages CDRWin cuesheets (cue/bin).
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

// TODO: Implement track flags
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using FileSystemIDandChk;

namespace FileSystemIDandChk.ImagePlugins
{
    class CDRWin : ImagePlugin
    {
        #region Internal structures

        struct CDRWinTrackFile
        {
            public UInt32 sequence;
            // Track #
            public string datafile;
            // Path of file containing track
            public UInt64 offset;
            // Offset of track start in file
            public string filetype;
            // Type of file
        }

        struct CDRWinTrack
        {
            public UInt32 sequence;
            // Track #
            public string title;
            // Track title (from CD-Text)
            public string genre;
            // Track genre (from CD-Text)
            public string arranger;
            // Track arranger (from CD-Text)
            public string composer;
            // Track composer (from CD-Text)
            public string performer;
            // Track performer (from CD-Text)
            public string songwriter;
            // Track song writer (from CD-Text)
            public string isrc;
            // Track ISRC
            public CDRWinTrackFile trackfile;
            // File struct for this track
            public Dictionary<int, UInt64> indexes;
            // Indexes on this track
            public UInt64 pregap;
            // Track pre-gap in sectors
            public UInt64 postgap;
            // Track post-gap in sectors
            public bool flag_dcp;
            // Digical Copy Permitted
            public bool flag_4ch;
            // Track is quadraphonic
            public bool flag_pre;
            // Track has preemphasis
            public bool flag_scms;
            // Track has SCMS
            public UInt16 bps;
            // Bytes per sector
            public UInt64 sectors;
            // Sectors in track
            public string tracktype;
            // Track type
            public UInt16 session;
            // Track session
        }

        #endregion

        struct CDRWinDisc
        {
            public string title;
            // Disk title (from CD-Text)
            public string genre;
            // Disk genre (from CD-Text)
            public string arranger;
            // Disk arranger (from CD-Text)
            public string composer;
            // Disk composer (from CD-Text)
            public string performer;
            // Disk performer (from CD-Text)
            public string songwriter;
            // Disk song writer (from CD-Text)
            public string mcn;
            // Media catalog number
            public DiskType disktype;
            // Disk type
            public string disktypestr;
            // Disk type string
            public string disk_id;
            // Disk CDDB ID
            public string barcode;
            // Disk UPC/EAN
            public List<Session> sessions;
            // Sessions
            public List<CDRWinTrack> tracks;
            // Tracks
            public string comment;
            // Disk comment
            public string cdtextfile;
            // File containing CD-Text
        }

        #region Internal consts

        // Type for FILE entity
        const string CDRWinDiskTypeLittleEndian = "BINARY";
        // Data as-is in little-endian
        const string CDRWinDiskTypeBigEndian = "MOTOROLA";
        // Data as-is in big-endian
        const string CDRWinDiskTypeAIFF = "AIFF";
        // Audio in Apple AIF file
        const string CDRWinDiskTypeRIFF = "WAVE";
        // Audio in Microsoft WAV file
        const string CDRWinDiskTypeMP3 = "MP3";
        // Audio in MP3 file
        // Type for TRACK entity
        const string CDRWinTrackTypeAudio = "AUDIO";
        // Audio track, 2352 bytes/sector
        const string CDRWinTrackTypeCDG = "CDG";
        // CD+G track, 2448 bytes/sector (audio+subchannel)
        const string CDRWinTrackTypeMode1 = "MODE1/2048";
        // Mode 1 track, cooked, 2048 bytes/sector
        const string CDRWinTrackTypeMode1Raw = "MODE1/2352";
        // Mode 1 track, raw, 2352 bytes/sector
        const string CDRWinTrackTypeMode2Form1 = "MODE2/2048";
        // Mode 2 form 1 track, cooked, 2048 bytes/sector
        const string CDRWinTrackTypeMode2Form2 = "MODE2/2324";
        // Mode 2 form 2 track, cooked, 2324 bytes/sector
        const string CDRWinTrackTypeMode2Formless = "MODE2/2336";
        // Mode 2 formless track, cooked, 2336 bytes/sector
        const string CDRWinTrackTypeMode2Raw = "MODE2/2352";
        // Mode 2 track, raw, 2352 bytes/sector
        const string CDRWinTrackTypeCDI = "CDI/2336";
        // CD-i track, cooked, 2336 bytes/sector
        const string CDRWinTrackTypeCDIRaw = "CDI/2352";
        // CD-i track, raw, 2352 bytes/sector
        // Type for REM ORIGINAL MEDIA-TYPE entity
        const string CDRWinDiskTypeCD = "CD";
        // DiskType.CD
        const string CDRWinDiskTypeCDRW = "CD-RW";
        // DiskType.CDRW
        const string CDRWinDiskTypeCDMRW = "CD-MRW";
        // DiskType.CDMRW
        const string CDRWinDiskTypeCDMRW2 = "CD-(MRW)";
        // DiskType.CDMRW
        const string CDRWinDiskTypeDVD = "DVD";
        // DiskType.DVDROM
        const string CDRWinDiskTypeDVDPMRW = "DVD+MRW";
        // DiskType.DVDPRW
        const string CDRWinDiskTypeDVDPMRW2 = "DVD+(MRW)";
        // DiskType.DVDPRW
        const string CDRWinDiskTypeDVDPMRWDL = "DVD+MRW DL";
        // DiskType.DVDPRWDL
        const string CDRWinDiskTypeDVDPMRWDL2 = "DVD+(MRW) DL";
        // DiskType.DVDPRWDL
        const string CDRWinDiskTypeDVDPR = "DVD+R";
        // DiskType.DVDPR
        const string CDRWinDiskTypeDVDPRDL = "DVD+R DL";
        // DiskType.DVDPRDL
        const string CDRWinDiskTypeDVDPRW = "DVD+RW";
        // DiskType.DVDPRW
        const string CDRWinDiskTypeDVDPRWDL = "DVD+RW DL";
        // DiskType.DVDPRWDL
        const string CDRWinDiskTypeDVDPVR = "DVD+VR";
        // DiskType.DVDPR
        const string CDRWinDiskTypeDVDRAM = "DVD-RAM";
        // DiskType.DVDRAM
        const string CDRWinDiskTypeDVDR = "DVD-R";
        // DiskType.DVDR
        const string CDRWinDiskTypeDVDRDL = "DVD-R DL";
        // DiskType.DVDRDL
        const string CDRWinDiskTypeDVDRW = "DVD-RW";
        // DiskType.DVDRW
        const string CDRWinDiskTypeDVDRWDL = "DVD-RW DL";
        // DiskType.DVDRWDL
        const string CDRWinDiskTypeDVDVR = "DVD-VR";
        // DiskType.DVDR
        const string CDRWinDiskTypeDVDRW2 = "DVDRW";
        // DiskType.DVDRW
        const string CDRWinDiskTypeHDDVD = "HD DVD";
        // DiskType.HDDVDROM
        const string CDRWinDiskTypeHDDVDRAM = "HD DVD-RAM";
        // DiskType.HDDVDRAM
        const string CDRWinDiskTypeHDDVDR = "HD DVD-R";
        // DiskType.HDDVDR
        const string CDRWinDiskTypeHDDVDRDL = "HD DVD-R DL";
        // DiskType.HDDVDR
        const string CDRWinDiskTypeHDDVDRW = "HD DVD-RW";
        // DiskType.HDDVDRW
        const string CDRWinDiskTypeHDDVDRWDL = "HD DVD-RW DL";
        // DiskType.HDDVDRW
        const string CDRWinDiskTypeBD = "BD";
        // DiskType.BDROM
        const string CDRWinDiskTypeBDR = "BD-R";
        // DiskType.BDR
        const string CDRWinDiskTypeBDRE = "BD-RE";
        // DiskType.BDRE
        const string CDRWinDiskTypeBDRDL = "BD-R DL";
        // DiskType.BDR
        const string CDRWinDiskTypeBDREDL = "BD-RE DL";
        // DiskType.BDRE

        #endregion

        #region Internal variables

        string imagePath;
        StreamReader cueStream;
        FileStream imageStream;
        Dictionary<UInt32, UInt64> offsetmap;
        // Dictionary, index is track #, value is TrackFile
        CDRWinDisc discimage;
        List<PartPlugins.Partition> partitions;

        #endregion

        #region Parsing regexs

        const string SessionRegEx = "REM\\s+SESSION\\s+(?<number>\\d+).*$";
        const string DiskTypeRegEx = "REM\\s+ORIGINAL MEDIA-TYPE:\\s+(?<mediatype>.+)$";
        const string LeadOutRegEx = "REM\\s+LEAD-OUT\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        const string LBARegEx = "REM MSF:\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)\\s+=\\s+LBA:\\s+(?<lba>[\\d]+)$";
        // Not checked
        const string DiskIDRegEx = "DISC_ID\\s+(?<diskid>[\\da-f]{8})$";
        const string BarCodeRegEx = "UPC_EAN\\s+(?<barcode>[\\d]{12,13})$";
        const string CommentRegEx = "REM\\s+(?<comment>.+)$";
        const string CDTextRegEx = "CDTEXTFILE\\s+(?<filename>.+)$";
        const string MCNRegEx = "CATALOG\\s+(?<catalog>\\d{13})$";
        const string TitleRegEx = "TITLE\\s+(?<title>.+)$";
        const string GenreRegEx = "GENRE\\s+(?<genre>.+)$";
        const string ArrangerRegEx = "ARRANGER\\s+(?<arranger>.+)$";
        const string ComposerRegEx = "COMPOSER\\s+(?<composer>.+)$";
        const string PerformerRegEx = "PERFORMER\\s+(?<performer>.+)$";
        const string SongWriterRegEx = "SONGWRITER\\s+(?<songwriter>.+)$";
        const string FileRegEx = "FILE\\s+(?<filename>.+)\\s+(?<type>\\S+)$";
        const string TrackRegEx = "TRACK\\s+(?<number>\\d+)\\s+(?<type>\\S+)$";
        const string ISRCRegEx = "ISRC\\s+(?<isrc>\\w{12})$";
        const string IndexRegEx = "INDEX\\s+(?<index>\\d+)\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        const string PregapRegEx = "PREGAP\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        const string PostgapRegex = "POSTGAP\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        const string FlagsRegEx = "FLAGS\\s+(((?<dcp>DCP)|(?<quad>4CH)|(?<pre>PRE)|(?<scms>SCMS))\\s*)+$";

        #endregion

        #region Methods

        public CDRWin(PluginBase Core)
        {
            Name = "CDRWin cuesheet handler";
            PluginUUID = new Guid("664568B2-15D4-4E64-8A7A-20BDA8B8386F");
            imagePath = "";
        }
        // Due to .cue format, this method must parse whole file, ignoring errors (those will be thrown by OpenImage()).
        public override bool IdentifyImage(string imagePath)
        {
            this.imagePath = imagePath;

            try
            {
                cueStream = new StreamReader(this.imagePath);
                int line = 0;

                while (cueStream.Peek() >= 0)
                {
                    line++;
                    string _line = cueStream.ReadLine();

                    Regex Sr = new Regex(SessionRegEx);
                    Regex Rr = new Regex(CommentRegEx);
                    Regex Cr = new Regex(MCNRegEx);
                    Regex Fr = new Regex(FileRegEx);

                    Match Sm;
                    Match Rm;
                    Match Cm;
                    Match Fm;

                    // First line must be SESSION, REM, CATALOG or FILE.
                    Sm = Sr.Match(_line);
                    Rm = Rr.Match(_line);
                    Cm = Cr.Match(_line);
                    Fm = Fr.Match(_line);

                    if (!Sm.Success && !Rm.Success && !Cm.Success && !Fm.Success)
                        return false;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception trying to identify image file {0}", this.imagePath);
                Console.WriteLine("Exception: {0}", ex.Message);
                Console.WriteLine("Stack trace: {0}", ex.StackTrace);
                return false;
            }
        }

        public override bool OpenImage(string imagePath)
        {
            if (imagePath == null)
                return false;
            if (imagePath == "")
                return false;

            this.imagePath = imagePath;

            try
            {
                cueStream = new StreamReader(imagePath);
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

                while (cueStream.Peek() >= 0)
                {
                    line++;
                    string _line = cueStream.ReadLine();

                    MatchTrack = RegexTrack.Match(_line);
                    if (MatchTrack.Success)
                    {
                        uint track_seq = uint.Parse(MatchTrack.Groups[1].Value);
                        if (track_count + 1 != track_seq)
                            throw new FeatureUnsupportedImageException(String.Format("Found TRACK {0} out of order in line {1}", track_seq, line));

                        track_count++;
                    }
                }

                if (track_count == 0)
                    throw new FeatureUnsupportedImageException("No tracks found");

                cuetracks = new CDRWinTrack[track_count];

                line = 0;
                cueStream.BaseStream.Seek(0, SeekOrigin.Begin);

                while (cueStream.Peek() >= 0)
                {
                    line++;
                    string _line = cueStream.ReadLine();

                    MatchSession = RegexSession.Match(_line);
                    MatchDiskType = RegexDiskType.Match(_line);
                    MatchComment = RegexComment.Match(_line);
                    MatchLBA = RegexLBA.Match(_line);   // Unhandled, just ignored
                    MatchLeadOut = RegexLeadOut.Match(_line); // Unhandled, just ignored

                    if (MatchDiskType.Success && !intrack)
                    {
                        if (MainClass.isDebug)
                            Console.WriteLine("DEBUG (CDRWin plugin): Found REM ORIGINAL MEDIA TYPE at line {0}", line);
                        discimage.disktypestr = MatchDiskType.Groups[1].Value;
                    }
                    else if (MatchDiskType.Success && intrack)
                    {
                        throw new FeatureUnsupportedImageException(String.Format("Found REM ORIGINAL MEDIA TYPE field after a track in line {0}", line));
                    }
                    else if (MatchSession.Success)
                    {
                        if (MainClass.isDebug)
                            Console.WriteLine("DEBUG (CDRWin plugin): Found REM SESSION at line {0}", line);
                        currentsession = Byte.Parse(MatchSession.Groups[1].Value);

                        // What happens between sessions
                    }
                    else if (MatchLBA.Success)
                    {
                        if (MainClass.isDebug)
                            Console.WriteLine("DEBUG (CDRWin plugin): Found REM MSF at line {0}", line);
                        // Just ignored
                    }
                    else if (MatchLeadOut.Success)
                    {
                        if (MainClass.isDebug)
                            Console.WriteLine("DEBUG (CDRWin plugin): Found REM LEAD-OUT at line {0}", line);
                        // Just ignored
                    }
                    else if (MatchComment.Success)
                    {
                        if (MainClass.isDebug)
                            Console.WriteLine("DEBUG (CDRWin plugin): Found REM at line {0}", line);
                        if (discimage.comment == "")
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

                        if (MatchArranger.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found ARRANGER at line {0}", line);
                            if (intrack)
                                currenttrack.arranger = MatchArranger.Groups[1].Value;
                            else
                                discimage.arranger = MatchArranger.Groups[1].Value;
                        }
                        else if (MatchBarCode.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found UPC_EAN at line {0}", line);
                            if (!intrack)
                                discimage.barcode = MatchBarCode.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found barcode field in incorrect place at line {0}", line));
                        }
                        else if (MatchCDText.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found CDTEXTFILE at line {0}", line);
                            if (!intrack)
                                discimage.cdtextfile = MatchCDText.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found CD-Text file field in incorrect place at line {0}", line));
                        }
                        else if (MatchComposer.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found COMPOSER at line {0}", line);
                            if (intrack)
                                currenttrack.arranger = MatchComposer.Groups[1].Value;
                            else
                                discimage.arranger = MatchComposer.Groups[1].Value;
                        }
                        else if (MatchDiskID.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found DISC_ID at line {0}", line);
                            if (!intrack)
                                discimage.disk_id = MatchDiskID.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found CDDB ID field in incorrect place at line {0}", line));
                        }
                        else if (MatchFile.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found FILE at line {0}", line);

                            if (currenttrack.sequence != 0)
                            {
                                currentfile.sequence = currenttrack.sequence;
                                currenttrack.trackfile = currentfile;
                                FileInfo finfo = new FileInfo(currentfile.datafile);
                                currenttrack.sectors = ((ulong)finfo.Length - currentfile.offset) / CDRWinTrackTypeToBytesPerSector(currenttrack.tracktype);
                                cuetracks[currenttrack.sequence - 1] = currenttrack;
                                intrack = false;
                                currenttrack = new CDRWinTrack();
                            }


                            currentfile.datafile = MatchFile.Groups[1].Value;
                            currentfile.filetype = MatchFile.Groups[2].Value;

                            // Check if file path is quoted
                            if (currentfile.datafile[0] == '"' && currentfile.datafile[currentfile.datafile.Length - 1] == '"')
                            {
                                currentfile.datafile = currentfile.datafile.Substring(1, currentfile.datafile.Length - 2); // Unquote it
                            }

                            // Check if file exists
                            if (!File.Exists(currentfile.datafile))
                            {
                                if (currentfile.datafile[0] == '/' || (currentfile.datafile[0] == '/' && currentfile.datafile[1] == '.')) // UNIX absolute path
                                {
                                    Regex unixpath = new Regex("^(.+)/([^/]+)$");
                                    Match unixpathmatch = unixpath.Match(currentfile.datafile);

                                    if (unixpathmatch.Success)
                                    {
                                        currentfile.datafile = unixpathmatch.Groups[1].Value;

                                        if (!File.Exists(currentfile.datafile))
                                        {
                                            string path = Path.GetPathRoot(imagePath);
                                            currentfile.datafile = path + Path.PathSeparator + currentfile.datafile;

                                            if (!File.Exists(currentfile.datafile))
                                                throw new FeatureUnsupportedImageException(String.Format("File \"{0}\" not found.", MatchFile.Groups[1].Value));
                                        }
                                    }
                                    else
                                        throw new FeatureUnsupportedImageException(String.Format("File \"{0}\" not found.", MatchFile.Groups[1].Value));
                                }
                                else if ((currentfile.datafile[1] == ':' && currentfile.datafile[2] == '\\') ||
                                         (currentfile.datafile[0] == '\\' && currentfile.datafile[1] == '\\') ||
                                         ((currentfile.datafile[0] == '.' && currentfile.datafile[1] == '\\'))) // Windows absolute path
                                {
                                    Regex winpath = new Regex("^(?:[a-zA-Z]\\:(\\\\|\\/)|file\\:\\/\\/|\\\\\\\\|\\.(\\/|\\\\))([^\\\\\\/\\:\\*\\?\\<\\>\\\"\\|]+(\\\\|\\/){0,1})+$");
                                    Match winpathmatch = winpath.Match(currentfile.datafile);
                                    if (winpathmatch.Success)
                                    {
                                        currentfile.datafile = winpathmatch.Groups[1].Value;

                                        if (!File.Exists(currentfile.datafile))
                                        {
                                            string path = Path.GetPathRoot(imagePath);
                                            currentfile.datafile = path + Path.PathSeparator + currentfile.datafile;

                                            if (!File.Exists(currentfile.datafile))
                                                throw new FeatureUnsupportedImageException(String.Format("File \"{0}\" not found.", MatchFile.Groups[1].Value));
                                        }
                                    }
                                    else
                                        throw new FeatureUnsupportedImageException(String.Format("File \"{0}\" not found.", MatchFile.Groups[1].Value));
                                }
                                else
                                {
                                    string path = Path.GetDirectoryName(imagePath);
                                    currentfile.datafile = path + Path.DirectorySeparatorChar + currentfile.datafile;

                                    if (!File.Exists(currentfile.datafile))
                                        throw new FeatureUnsupportedImageException(String.Format("File \"{0}\" not found.", MatchFile.Groups[1].Value));
                                }
                            }

                            // File does exist, process it
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): File \"{0}\" found", currentfile.datafile);

                            switch (currentfile.filetype)
                            {
                                case CDRWinDiskTypeLittleEndian:
                                    break;
                                case CDRWinDiskTypeBigEndian:
                                case CDRWinDiskTypeAIFF:
                                case CDRWinDiskTypeRIFF:
                                case CDRWinDiskTypeMP3:
                                    throw new FeatureSupportedButNotImplementedImageException(String.Format("Unsupported file type {0}", currentfile.filetype));
                                default:
                                    throw new FeatureUnsupportedImageException(String.Format("Unknown file type {0}", currentfile.filetype));
                            }

                            currentfile.offset = 0;
                            currentfile.sequence = 0;
                        }
                        else if (MatchFlags.Success)
                        {
                            // TODO: Implement FLAGS support.
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found FLAGS at line {0}", line);
                            if (!intrack)
                                throw new FeatureUnsupportedImageException(String.Format("Found FLAGS field in incorrect place at line {0}", line));
                        }
                        else if (MatchGenre.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found GENRE at line {0}", line);
                            if (intrack)
                                currenttrack.genre = MatchGenre.Groups[1].Value;
                            else
                                discimage.genre = MatchGenre.Groups[1].Value;
                        }
                        else if (MatchIndex.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found INDEX at line {0}", line);
                            if (!intrack)
                                throw new FeatureUnsupportedImageException(String.Format("Found INDEX before a track {0}", line));
                            else
                            {
                                int index = int.Parse(MatchIndex.Groups[1].Value);
                                ulong offset = CDRWinMSFToLBA(MatchIndex.Groups[2].Value);

                                if ((index != 0 && index != 1) && currenttrack.indexes.Count == 0)
                                    throw new FeatureUnsupportedImageException(String.Format("Found INDEX {0} before INDEX 00 or INDEX 01", index));

                                if ((index == 0 || (index == 1 && !currenttrack.indexes.ContainsKey(0))))
                                {
                                    if ((int)(currenttrack.sequence - 2) >= 0 && offset > 1)
                                    {
                                        cuetracks[currenttrack.sequence - 2].sectors = offset - currentfileoffsetsector;
                                        currentfile.offset += cuetracks[currenttrack.sequence - 2].sectors * cuetracks[currenttrack.sequence - 2].bps;
                                        if (MainClass.isDebug)
                                        {
                                            Console.WriteLine("DEBUG (CDRWin plugin): Sets currentfile.offset to {0} at line 553", currentfile.offset);
                                            Console.WriteLine("DEBUG (CDRWin plugin): cuetracks[currenttrack.sequence-2].sectors = {0}", cuetracks[currenttrack.sequence - 2].sectors);
                                            Console.WriteLine("DEBUG (CDRWin plugin): cuetracks[currenttrack.sequence-2].bps = {0}", cuetracks[currenttrack.sequence - 2].bps);
                                        }
                                    }
                                }

                                if ((index == 0 || (index == 1 && !currenttrack.indexes.ContainsKey(0))) && currenttrack.sequence == 1)
                                {
                                    if (MainClass.isDebug)
                                        Console.WriteLine("DEBUG (CDRWin plugin): Sets currentfile.offset to {0} at line 559", offset * currenttrack.bps);
                                    currentfile.offset = offset * currenttrack.bps;
                                }
                                    
                                currentfileoffsetsector = offset;
                                currenttrack.indexes.Add(index, offset);
                            }
                        }
                        else if (MatchISRC.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found ISRC at line {0}", line);
                            if (!intrack)
                                throw new FeatureUnsupportedImageException(String.Format("Found ISRC before a track {0}", line));
                            currenttrack.isrc = MatchISRC.Groups[1].Value;
                        }
                        else if (MatchMCN.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found CATALOG at line {0}", line);
                            if (!intrack)
                                discimage.mcn = MatchMCN.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found CATALOG field in incorrect place at line {0}", line));
                        }
                        else if (MatchPerformer.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found PERFORMER at line {0}", line);
                            if (intrack)
                                currenttrack.performer = MatchPerformer.Groups[1].Value;
                            else
                                discimage.performer = MatchPerformer.Groups[1].Value;
                        }
                        else if (MatchPostgap.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found POSTGAP at line {0}", line);
                            if (intrack)
                            {
                                currenttrack.postgap = CDRWinMSFToLBA(MatchPostgap.Groups[1].Value);
                            }
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found POSTGAP field before a track at line {0}", line));
                        }
                        else if (MatchPregap.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found PREGAP at line {0}", line);
                            if (intrack)
                            {
                                currenttrack.pregap = CDRWinMSFToLBA(MatchPregap.Groups[1].Value);
                            }
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found PREGAP field before a track at line {0}", line));
                        }
                        else if (MatchSongWriter.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found SONGWRITER at line {0}", line);
                            if (intrack)
                                currenttrack.songwriter = MatchSongWriter.Groups[1].Value;
                            else
                                discimage.songwriter = MatchSongWriter.Groups[1].Value;
                        }
                        else if (MatchTitle.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found TITLE at line {0}", line);
                            if (intrack)
                                currenttrack.title = MatchTitle.Groups[1].Value;
                            else
                                discimage.title = MatchTitle.Groups[1].Value;
                        }
                        else if (MatchTrack.Success)
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Found TRACK at line {0}", line);
                            if (currentfile.datafile == "")
                                throw new FeatureUnsupportedImageException(String.Format("Found TRACK field before a file is defined at line {0}", line));
                            if (intrack)
                            {
                                if (currenttrack.indexes.ContainsKey(0) && currenttrack.pregap == 0)
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
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (CDRWin plugin): Setting currenttrack.sequence to {0}", currenttrack.sequence);
                            currentfile.sequence = currenttrack.sequence;
                            currenttrack.bps = CDRWinTrackTypeToBytesPerSector(MatchTrack.Groups[2].Value);
                            currenttrack.tracktype = MatchTrack.Groups[2].Value;
                            currenttrack.session = currentsession;
                            intrack = true;
                        }
                        else if (_line == "") // Empty line, ignore it
                        {

                        }
                        else // Non-empty unknown field
                        {
                            throw new FeatureUnsupportedImageException(String.Format("Found unknown field defined at line {0}: \"{1}\"", line, _line));
                        }
                    }
                }

                if (currenttrack.sequence != 0)
                {
                    currentfile.sequence = currenttrack.sequence;
                    currenttrack.trackfile = currentfile;
                    FileInfo finfo = new FileInfo(currentfile.datafile);
                    currenttrack.sectors = ((ulong)finfo.Length - currentfile.offset) / CDRWinTrackTypeToBytesPerSector(currenttrack.tracktype);
                    cuetracks[currenttrack.sequence - 1] = currenttrack;
                }

                Session[] _sessions = new Session[currentsession];
                for (int s = 1; s <= _sessions.Length; s++)
                {
                    _sessions[s - 1].SessionSequence = 1;

                    if (s > 1)
                        _sessions[s - 1].StartSector = _sessions[s - 2].EndSector + 1;
                    else
                        _sessions[s - 1].StartSector = 0;

                    ulong session_sectors = 0;
                    int last_session_track = 0;

                    for (int i = 0; i < cuetracks.Length; i++)
                    {
                        if (cuetracks[i].session == s)
                        {
                            session_sectors += cuetracks[i].sectors;
                            if (i > last_session_track)
                                last_session_track = i;
                        }
                    }

                    _sessions[s - 1].EndTrack = cuetracks[last_session_track].sequence;
                    _sessions[s - 1].EndSector = session_sectors - 1;
                }

                for (int s = 1; s <= _sessions.Length; s++)
                    discimage.sessions.Add(_sessions[s - 1]);

                for (int t = 1; t <= cuetracks.Length; t++)
                    discimage.tracks.Add(cuetracks[t - 1]);

                discimage.disktype = CDRWinIsoBusterDiscTypeToDiskType(discimage.disktypestr);

                if (discimage.disktype == DiskType.Unknown || discimage.disktype == DiskType.CD)
                {
                    bool data = false;
                    bool cdg = false;
                    bool cdi = false;
                    bool mode2 = false;
                    bool firstaudio = false;
                    bool firstdata = false;
                    bool audio = false;

                    for (int i = 0; i < discimage.tracks.Count; i++)
                    {
                        // First track is audio
                        firstaudio |= i == 0 && discimage.tracks[i].tracktype == CDRWinTrackTypeAudio;

                        // First track is data
                        firstdata |= i == 0 && discimage.tracks[i].tracktype != CDRWinTrackTypeAudio;

                        // Any non first track is data
                        data |= i != 0 && discimage.tracks[i].tracktype != CDRWinTrackTypeAudio;

                        // Any non first track is audio
                        audio |= i != 0 && discimage.tracks[i].tracktype == CDRWinTrackTypeAudio;

                        switch (discimage.tracks[i].tracktype)
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

                    if (!data && !firstdata)
                        discimage.disktype = DiskType.CDDA;
                    else if (cdg)
                        discimage.disktype = DiskType.CDG;
                    else if (cdi)
                        discimage.disktype = DiskType.CDI;
                    else if (firstaudio && data && discimage.sessions.Count > 1 && mode2)
                        discimage.disktype = DiskType.CDPLUS;
                    else if ((firstdata && !data) || mode2)
                        discimage.disktype = DiskType.CDROMXA;
                    else if (!audio)
                        discimage.disktype = DiskType.CDROM;
                    else
                        discimage.disktype = DiskType.CD;
                }

                if (MainClass.isDebug)
                {
                    // DEBUG information
                    Console.WriteLine("DEBUG (CDRWin plugin): Disc image parsing results");
                    Console.WriteLine("DEBUG (CDRWin plugin): Disc CD-TEXT:");
                    if (discimage.arranger == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tArranger is not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tArranger: {0}", discimage.arranger);
                    if (discimage.composer == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tComposer is not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tComposer: {0}", discimage.composer);
                    if (discimage.genre == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tGenre is not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tGenre: {0}", discimage.genre);
                    if (discimage.performer == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tPerformer is not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tPerformer: {0}", discimage.performer);
                    if (discimage.songwriter == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tSongwriter is not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tSongwriter: {0}", discimage.songwriter);
                    if (discimage.title == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tTitle is not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tTitle: {0}", discimage.title);
                    if (discimage.cdtextfile == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tCD-TEXT binary file not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tCD-TEXT binary file: {0}", discimage.cdtextfile);
                    Console.WriteLine("DEBUG (CDRWin plugin): Disc information:");
                    if (discimage.disktypestr == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tISOBuster disc type not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tISOBuster disc type: {0}", discimage.disktypestr);
                    Console.WriteLine("DEBUG (CDRWin plugin): \tGuessed disk type: {0}", discimage.disktype);
                    if (discimage.barcode == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tBarcode not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tBarcode: {0}", discimage.barcode);
                    if (discimage.disk_id == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tDisc ID not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tDisc ID: {0}", discimage.disk_id);
                    if (discimage.mcn == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tMCN not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tMCN: {0}", discimage.mcn);
                    if (discimage.comment == null)
                        Console.WriteLine("DEBUG (CDRWin plugin): \tComment not set.");
                    else
                        Console.WriteLine("DEBUG (CDRWin plugin): \tComment: \"{0}\"", discimage.comment);
                    Console.WriteLine("DEBUG (CDRWin plugin): Session information:");
                    Console.WriteLine("DEBUG (CDRWin plugin): \tDisc contains {0} sessions", discimage.sessions.Count);
                    for (int i = 0; i < discimage.sessions.Count; i++)
                    {
                        Console.WriteLine("DEBUG (CDRWin plugin): \tSession {0} information:", i + 1);
                        Console.WriteLine("DEBUG (CDRWin plugin): \t\tStarting track: {0}", discimage.sessions[i].StartTrack);
                        Console.WriteLine("DEBUG (CDRWin plugin): \t\tStarting sector: {0}", discimage.sessions[i].StartSector);
                        Console.WriteLine("DEBUG (CDRWin plugin): \t\tEnding track: {0}", discimage.sessions[i].EndTrack);
                        Console.WriteLine("DEBUG (CDRWin plugin): \t\tEnding sector: {0}", discimage.sessions[i].EndSector);
                    }
                    Console.WriteLine("DEBUG (CDRWin plugin): Track information:");
                    Console.WriteLine("DEBUG (CDRWin plugin): \tDisc contains {0} tracks", discimage.tracks.Count);
                    for (int i = 0; i < discimage.tracks.Count; i++)
                    {
                        Console.WriteLine("DEBUG (CDRWin plugin): \tTrack {0} information:", discimage.tracks[i].sequence);

                        Console.WriteLine("DEBUG (CDRWin plugin): \t\t{0} bytes per sector", discimage.tracks[i].bps);
                        Console.WriteLine("DEBUG (CDRWin plugin): \t\tPregap: {0} sectors", discimage.tracks[i].pregap);
                        Console.WriteLine("DEBUG (CDRWin plugin): \t\tData: {0} sectors", discimage.tracks[i].sectors);
                        Console.WriteLine("DEBUG (CDRWin plugin): \t\tPostgap: {0} sectors", discimage.tracks[i].postgap);

                        if (discimage.tracks[i].flag_4ch)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tTrack is flagged as quadraphonic");
                        if (discimage.tracks[i].flag_dcp)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tTrack allows digital copy");
                        if (discimage.tracks[i].flag_pre)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tTrack has pre-emphasis applied");
                        if (discimage.tracks[i].flag_scms)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tTrack has SCMS");

                        Console.WriteLine("DEBUG (CDRWin plugin): \t\tTrack resides in file {0}, type defined as {1}, starting at byte {2}",
                            discimage.tracks[i].trackfile.datafile, discimage.tracks[i].trackfile.filetype, discimage.tracks[i].trackfile.offset);

                        Console.WriteLine("DEBUG (CDRWin plugin): \t\tIndexes:");
                        foreach (KeyValuePair<int, ulong> kvp in discimage.tracks[i].indexes)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\t\tIndex {0} starts at sector {1}", kvp.Key, kvp.Value);

                        if (discimage.tracks[i].isrc == null)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tISRC is not set.");
                        else
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tISRC: {0}", discimage.tracks[i].isrc);

                        if (discimage.tracks[i].arranger == null)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tArranger is not set.");
                        else
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tArranger: {0}", discimage.tracks[i].arranger);
                        if (discimage.tracks[i].composer == null)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tComposer is not set.");
                        else
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tComposer: {0}", discimage.tracks[i].composer);
                        if (discimage.tracks[i].genre == null)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tGenre is not set.");
                        else
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tGenre: {0}", discimage.tracks[i].genre);
                        if (discimage.tracks[i].performer == null)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tPerformer is not set.");
                        else
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tPerformer: {0}", discimage.tracks[i].performer);
                        if (discimage.tracks[i].songwriter == null)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tSongwriter is not set.");
                        else
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tSongwriter: {0}", discimage.tracks[i].songwriter);
                        if (discimage.tracks[i].title == null)
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tTitle is not set.");
                        else
                            Console.WriteLine("DEBUG (CDRWin plugin): \t\tTitle: {0}", discimage.tracks[i].title);
                    }
                }

                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (CDRWin plugin): Building offset map");

                partitions = new List<FileSystemIDandChk.PartPlugins.Partition>();

                ulong byte_offset = 0;
                ulong sector_offset = 0;
                ulong partitionSequence = 0;
                ulong index_zero_offset = 0;
                ulong index_one_offset = 0;
                bool index_zero = false;

                offsetmap = new Dictionary<uint, ulong>();

                for (int i = 0; i < discimage.tracks.Count; i++)
                {
                    if (discimage.tracks[i].sequence == 1 && i != 0)
                        throw new ImageNotSupportedException("Unordered tracks");

                    PartPlugins.Partition partition = new FileSystemIDandChk.PartPlugins.Partition();

                    if (discimage.tracks[i].pregap > 0)
                    {
                        partition.PartitionDescription = String.Format("Track {0} pregap.", discimage.tracks[i].sequence);
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

                        if (!offsetmap.ContainsKey(discimage.tracks[i].sequence))
                            offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                        else
                        {
                            ulong old_start;
                            offsetmap.TryGetValue(discimage.tracks[i].sequence, out old_start);

                            if (partition.PartitionStartSector < old_start)
                            {
                                offsetmap.Remove(discimage.tracks[i].sequence);
                                offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                            }
                        }

                        partitions.Add(partition);
                        partition = new FileSystemIDandChk.PartPlugins.Partition();
                    }

                    index_zero |= discimage.tracks[i].indexes.TryGetValue(0, out index_zero_offset);

                    if (!discimage.tracks[i].indexes.TryGetValue(1, out index_one_offset))
                        throw new ImageNotSupportedException(String.Format("Track {0} lacks index 01", discimage.tracks[i].sequence));

                    if (index_zero && index_one_offset > index_zero_offset)
                    {
                        partition.PartitionDescription = String.Format("Track {0} index 00.", discimage.tracks[i].sequence);
                        partition.PartitionName = discimage.tracks[i].title;
                        partition.PartitionStartSector = sector_offset;
                        partition.PartitionLength = (index_one_offset - index_zero_offset) * discimage.tracks[i].bps;
                        partition.PartitionSectors = index_one_offset - index_zero_offset;
                        partition.PartitionSequence = partitionSequence;
                        partition.PartitionStart = byte_offset;
                        partition.PartitionType = discimage.tracks[i].tracktype;

                        sector_offset += partition.PartitionSectors;
                        byte_offset += partition.PartitionLength;
                        partitionSequence++;

                        if (!offsetmap.ContainsKey(discimage.tracks[i].sequence))
                            offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                        else
                        {
                            ulong old_start;
                            offsetmap.TryGetValue(discimage.tracks[i].sequence, out old_start);

                            if (partition.PartitionStartSector < old_start)
                            {
                                offsetmap.Remove(discimage.tracks[i].sequence);
                                offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                            }
                        }

                        partitions.Add(partition);
                        partition = new FileSystemIDandChk.PartPlugins.Partition();
                    }

                    // Index 01
                    partition.PartitionDescription = String.Format("Track {0}.", discimage.tracks[i].sequence);
                    partition.PartitionName = discimage.tracks[i].title;
                    partition.PartitionStartSector = sector_offset;
                    partition.PartitionLength = (discimage.tracks[i].sectors - (index_one_offset - index_zero_offset)) * discimage.tracks[i].bps;
                    partition.PartitionSectors = (discimage.tracks[i].sectors - (index_one_offset - index_zero_offset));
                    partition.PartitionSequence = partitionSequence;
                    partition.PartitionStart = byte_offset;
                    partition.PartitionType = discimage.tracks[i].tracktype;

                    sector_offset += partition.PartitionSectors;
                    byte_offset += partition.PartitionLength;
                    partitionSequence++;

                    if (!offsetmap.ContainsKey(discimage.tracks[i].sequence))
                        offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                    else
                    {
                        ulong old_start;
                        offsetmap.TryGetValue(discimage.tracks[i].sequence, out old_start);

                        if (partition.PartitionStartSector < old_start)
                        {
                            offsetmap.Remove(discimage.tracks[i].sequence);
                            offsetmap.Add(discimage.tracks[i].sequence, partition.PartitionStartSector);
                        }
                    }

                    partitions.Add(partition);
                    partition = new FileSystemIDandChk.PartPlugins.Partition();
                }

                // Print offset map
                if (MainClass.isDebug)
                {
                    Console.WriteLine("DEBUG (CDRWin plugin) printing partition map");
                    foreach (FileSystemIDandChk.PartPlugins.Partition partition in partitions)
                    {
                        Console.WriteLine("DEBUG (CDRWin plugin): Partition sequence: {0}", partition.PartitionSequence);
                        Console.WriteLine("DEBUG (CDRWin plugin): \tPartition name: {0}", partition.PartitionName);
                        Console.WriteLine("DEBUG (CDRWin plugin): \tPartition description: {0}", partition.PartitionDescription);
                        Console.WriteLine("DEBUG (CDRWin plugin): \tPartition type: {0}", partition.PartitionType);
                        Console.WriteLine("DEBUG (CDRWin plugin): \tPartition starting sector: {0}", partition.PartitionStartSector);
                        Console.WriteLine("DEBUG (CDRWin plugin): \tPartition sectors: {0}", partition.PartitionSectors);
                        Console.WriteLine("DEBUG (CDRWin plugin): \tPartition starting offset: {0}", partition.PartitionStart);
                        Console.WriteLine("DEBUG (CDRWin plugin): \tPartition size in bytes: {0}", partition.PartitionLength);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception trying to identify image file {0}", imagePath);
                Console.WriteLine("Exception: {0}", ex.Message);
                Console.WriteLine("Stack trace: {0}", ex.StackTrace);
                return false;
            }
        }

        public override bool ImageHasPartitions()
        {
            // Even if they only have 1 track, there is a partition (track 1)
            return true;
        }

        public override UInt64 GetImageSize()
        {
            UInt64 size;

            size = 0;
            foreach (CDRWinTrack track in discimage.tracks)
                size += track.bps * track.sectors;

            return size;
        }

        public override UInt64 GetSectors()
        {
            UInt64 sectors;

            sectors = 0;
            foreach (CDRWinTrack track in discimage.tracks)
                sectors += track.sectors;

            return sectors;
        }

        public override UInt32 GetSectorSize()
        {
            if (discimage.disktype == DiskType.CDG || discimage.disktype == DiskType.CDEG || discimage.disktype == DiskType.CDMIDI)
                return 2448; // CD+G subchannels ARE user data, as CD+G are useless without them
            if (discimage.disktype != DiskType.CDROMXA && discimage.disktype != DiskType.CDDA && discimage.disktype != DiskType.CDI && discimage.disktype != DiskType.CDPLUS)
                return 2048; // Only data tracks
            return 2352; // All others
        }

        public override byte[] ReadDiskTag(DiskTagType tag)
        {
            switch (tag)
            {
                case DiskTagType.CD_MCN:
                    {
                        if (discimage.mcn != null)
                        {
                            return Encoding.ASCII.GetBytes(discimage.mcn);
                        }
                        throw new FeatureNotPresentImageException("Image does not contain MCN information.");
                    }
                case DiskTagType.CD_TEXT:
                    {
                        if (discimage.cdtextfile != null)
                            // TODO: Check that binary text file exists, open it, read it, send it to caller.
                            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                        throw new FeatureNotPresentImageException("Image does not contain CD-TEXT information.");
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        public override byte[] ReadSector(UInt64 sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(UInt64 sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public override byte[] ReadSector(UInt64 sectorAddress, UInt32 track)
        {
            return ReadSectors(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorTag(UInt64 sectorAddress, UInt32 track, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        public override byte[] ReadSectors(UInt64 sectorAddress, UInt32 length)
        {
            foreach (KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if (sectorAddress >= kvp.Value)
                {
                    foreach (CDRWinTrack cdrwin_track in discimage.tracks)
                    {
                        if (cdrwin_track.sequence == kvp.Key)
                        {
                            if ((sectorAddress - kvp.Value) < cdrwin_track.sectors)
                                return ReadSectors((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException("sectorAddress", "Sector address not found");
        }

        public override byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, SectorTagType tag)
        {
            foreach (KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if (sectorAddress >= kvp.Value)
                {
                    foreach (CDRWinTrack cdrwin_track in discimage.tracks)
                    {
                        if (cdrwin_track.sequence == kvp.Key)
                        {
                            if ((sectorAddress - kvp.Value) < cdrwin_track.sectors)
                                return ReadSectorsTag((sectorAddress - kvp.Value), length, kvp.Key, tag);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException("sectorAddress", "Sector address not found");
        }

        public override byte[] ReadSectors(UInt64 sectorAddress, UInt32 length, UInt32 track)
        {
            CDRWinTrack _track = new CDRWinTrack();

            _track.sequence = 0;

            foreach (CDRWinTrack cdrwin_track in discimage.tracks)
            {
                if (cdrwin_track.sequence == track)
                {
                    _track = cdrwin_track;
                    break;
                }
            }

            if (_track.sequence == 0)
                throw new ArgumentOutOfRangeException("track", "Track does not exist in disc image");

            if (length > _track.sectors)
                throw new ArgumentOutOfRangeException("length", "Requested more sectors than present in track, won't cross tracks");

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch (_track.tracktype)
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

            imageStream = new FileStream(_track.trackfile.datafile, FileMode.Open, FileAccess.Read);
            using (BinaryReader br = new BinaryReader(imageStream))
            {
                br.BaseStream.Seek((long)_track.trackfile.offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
                if (sector_offset == 0 && sector_skip == 0)
                    buffer = br.ReadBytes((int)(sector_size * length));
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        byte[] sector;
                        br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                        sector = br.ReadBytes((int)sector_size);
                        br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                        Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                    }
                }
            }



            return buffer;
        }

        public override byte[] ReadSectorsTag(UInt64 sectorAddress, UInt32 length, UInt32 track, SectorTagType tag)
        {
            CDRWinTrack _track = new CDRWinTrack();

            _track.sequence = 0;

            foreach (CDRWinTrack cdrwin_track in discimage.tracks)
            {
                if (cdrwin_track.sequence == track)
                {
                    _track = cdrwin_track;
                    break;
                }
            }

            if (_track.sequence == 0)
                throw new ArgumentOutOfRangeException("track", "Track does not exist in disc image");

            if (length > _track.sectors)
                throw new ArgumentOutOfRangeException("length", "Requested more sectors than present in track, won't cross tracks");

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch (tag)
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

                        if (_track.tracktype != CDRWinTrackTypeAudio && _track.tracktype != CDRWinTrackTypeCDG)
                            flags[0] += 0x40;

                        if (_track.flag_dcp)
                            flags[0] += 0x20;

                        if (_track.flag_pre)
                            flags[0] += 0x10;

                        return flags;
                    }
                case SectorTagType.CDTrackISRC:
                    return Encoding.UTF8.GetBytes(_track.isrc);
                case SectorTagType.CDTrackText:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                default:
                    throw new ArgumentException("Unsupported tag requested", "tag");
            }

            switch (_track.tracktype)
            {
                case CDRWinTrackTypeMode1:
                case CDRWinTrackTypeMode2Form1:
                case CDRWinTrackTypeMode2Form2:
                    throw new ArgumentException("No tags in image for requested track", "tag");
                case CDRWinTrackTypeMode2Formless:
                case CDRWinTrackTypeCDI:
                    {
                        switch (tag)
                        {
                            case SectorTagType.CDSectorSync:
                            case SectorTagType.CDSectorHeader:
                            case SectorTagType.CDSectorSubchannel:
                            case SectorTagType.CDSectorECC:
                            case SectorTagType.CDSectorECC_P:
                            case SectorTagType.CDSectorECC_Q:
                                throw new ArgumentException("Unsupported tag requested for this track", "tag");
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
                                throw new ArgumentException("Unsupported tag requested", "tag");
                        }
                        break;
                    }
                case CDRWinTrackTypeAudio:
                    throw new ArgumentException("There are no tags on audio tracks", "tag");
                case CDRWinTrackTypeMode1Raw:
                    {
                        switch (tag)
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
                                throw new ArgumentException("Unsupported tag requested for this track", "tag");
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
                                throw new ArgumentException("Unsupported tag requested", "tag");
                        }
                        break;
                    }
                case CDRWinTrackTypeMode2Raw: // Requires reading sector
                case CDRWinTrackTypeCDIRaw:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                case CDRWinTrackTypeCDG:
                    {
                        if (tag != SectorTagType.CDSectorSubchannel)
                            throw new ArgumentException("Unsupported tag requested for this track", "tag");

                        sector_offset = 2352;
                        sector_size = 96;
                        sector_skip = 0;
                        break;
                    }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sector_size * length];

            imageStream = new FileStream(_track.trackfile.datafile, FileMode.Open, FileAccess.Read);
            using (BinaryReader br = new BinaryReader(imageStream))
            {
                br.BaseStream.Seek((long)_track.trackfile.offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);
                if (sector_offset == 0 && sector_skip == 0)
                    buffer = br.ReadBytes((int)(sector_size * length));
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        byte[] sector;
                        br.BaseStream.Seek(sector_offset, SeekOrigin.Current);
                        sector = br.ReadBytes((int)sector_size);
                        br.BaseStream.Seek(sector_skip, SeekOrigin.Current);
                        Array.Copy(sector, 0, buffer, i * sector_size, sector_size);
                    }
                }
            }



            return buffer;
        }

        public override byte[] ReadSectorLong(UInt64 sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public override byte[] ReadSectorLong(UInt64 sectorAddress, UInt32 track)
        {
            return ReadSectorsLong(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length)
        {
            foreach (KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if (sectorAddress >= kvp.Value)
                {
                    foreach (CDRWinTrack cdrwin_track in discimage.tracks)
                    {
                        if (cdrwin_track.sequence == kvp.Key)
                        {
                            if ((sectorAddress - kvp.Value) < cdrwin_track.sectors)
                                return ReadSectorsLong((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException("sectorAddress", "Sector address not found");
        }

        public override byte[] ReadSectorsLong(UInt64 sectorAddress, UInt32 length, UInt32 track)
        {
            CDRWinTrack _track = new CDRWinTrack();

            _track.sequence = 0;

            foreach (CDRWinTrack cdrwin_track in discimage.tracks)
            {
                if (cdrwin_track.sequence == track)
                {
                    _track = cdrwin_track;
                    break;
                }
            }

            if (_track.sequence == 0)
                throw new ArgumentOutOfRangeException("track", "Track does not exist in disc image");

            if (length > _track.sectors)
                throw new ArgumentOutOfRangeException("length", "Requested more sectors than present in track, won't cross tracks");

            uint sector_offset;
            uint sector_size;
            uint sector_skip;

            switch (_track.tracktype)
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

            imageStream = new FileStream(_track.trackfile.datafile, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(imageStream);

            br.BaseStream.Seek((long)_track.trackfile.offset + (long)(sectorAddress * (sector_offset + sector_size + sector_skip)), SeekOrigin.Begin);

            if (sector_offset == 0 && sector_skip == 0)
                buffer = br.ReadBytes((int)(sector_size * length));
            else
            {
                for (int i = 0; i < length; i++)
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

        public override string   GetImageFormat()
        {
            return "CDRWin CUESheet";
        }

        public override string   GetImageVersion()
        {
            return null;
        }

        public override string   GetImageApplication()
        {
            // Detect ISOBuster extensions
            if (discimage.disktypestr != null || discimage.comment.ToLower().Contains("isobuster") || discimage.sessions.Count > 1)
                return "ISOBuster";
            return "CDRWin";
        }

        public override string   GetImageApplicationVersion()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override DateTime GetImageCreationTime()
        {
            FileInfo fi = new FileInfo(discimage.tracks[0].trackfile.datafile);

            return fi.CreationTimeUtc;
        }

        public override DateTime GetImageLastModificationTime()
        {
            FileInfo fi = new FileInfo(discimage.tracks[0].trackfile.datafile);

            return fi.LastWriteTimeUtc;
        }

        public override string   GetImageComments()
        {
            return discimage.comment;
        }

        public override string GetDiskSerialNumber()
        {
            return discimage.mcn;
        }

        public override string GetDiskBarcode()
        {
            return discimage.barcode;
        }

        public override DiskType GetDiskType()
        {
            return discimage.disktype;
        }

        public override List<PartPlugins.Partition> GetPartitions()
        {
            return partitions;
        }

        public override List<Track> GetTracks()
        {
            List<Track> tracks = new List<Track>();

            foreach (CDRWinTrack cdr_track in discimage.tracks)
            {
                Track _track = new Track();

                _track.Indexes = cdr_track.indexes;
                _track.TrackDescription = cdr_track.title;
                if (!cdr_track.indexes.TryGetValue(0, out _track.TrackStartSector))
                    cdr_track.indexes.TryGetValue(1, out _track.TrackStartSector);
                _track.TrackEndSector = _track.TrackStartSector + cdr_track.sectors - 1;
                _track.TrackPregap = cdr_track.pregap;
                _track.TrackSession = cdr_track.session;
                _track.TrackSequence = cdr_track.sequence;
                _track.TrackType = CDRWinTrackTypeToTrackType(cdr_track.tracktype);

                tracks.Add(_track);
            }

            return tracks;
        }

        public override List<Track> GetSessionTracks(Session session)
        {
            if (discimage.sessions.Contains(session))
            {
                return GetSessionTracks(session.SessionSequence);
            }
            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public override List<Track> GetSessionTracks(UInt16 session)
        {
            List<Track> tracks = new List<Track>();

            foreach (CDRWinTrack cdr_track in discimage.tracks)
            {
                if (cdr_track.session == session)
                {
                    Track _track = new Track();

                    _track.Indexes = cdr_track.indexes;
                    _track.TrackDescription = cdr_track.title;
                    if (!cdr_track.indexes.TryGetValue(0, out _track.TrackStartSector))
                        cdr_track.indexes.TryGetValue(1, out _track.TrackStartSector);
                    _track.TrackEndSector = _track.TrackStartSector + cdr_track.sectors - 1;
                    _track.TrackPregap = cdr_track.pregap;
                    _track.TrackSession = cdr_track.session;
                    _track.TrackSequence = cdr_track.sequence;
                    _track.TrackType = CDRWinTrackTypeToTrackType(cdr_track.tracktype);

                    tracks.Add(_track);
                }
            }

            return tracks;
        }

        public override List<Session> GetSessions()
        {
            return discimage.sessions;
        }

        #endregion

        #region Private methods

        static UInt64 CDRWinMSFToLBA(string MSF)
        {
            string[] MSFElements;
            UInt64 minute, second, frame, sectors;

            MSFElements = MSF.Split(':');
            minute = UInt64.Parse(MSFElements[0]);
            second = UInt64.Parse(MSFElements[1]);
            frame = UInt64.Parse(MSFElements[2]);

            sectors = (minute * 60 * 75) + (second * 75) + frame;

            return sectors;
        }

        static UInt16 CDRWinTrackTypeToBytesPerSector(string trackType)
        {
            switch (trackType)
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

        static TrackType CDRWinTrackTypeToTrackType(string trackType)
        {
            switch (trackType)
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

        static DiskType CDRWinIsoBusterDiscTypeToDiskType(string discType)
        {
            switch (discType)
            {
                case CDRWinDiskTypeCD:
                    return DiskType.CD;
                case CDRWinDiskTypeCDRW:
                case CDRWinDiskTypeCDMRW:
                case CDRWinDiskTypeCDMRW2:
                    return DiskType.CDRW;
                case CDRWinDiskTypeDVD:
                    return DiskType.DVDROM;
                case CDRWinDiskTypeDVDPRW:
                case CDRWinDiskTypeDVDPMRW:
                case CDRWinDiskTypeDVDPMRW2:
                    return DiskType.DVDPRW;
                case CDRWinDiskTypeDVDPRWDL:
                case CDRWinDiskTypeDVDPMRWDL:
                case CDRWinDiskTypeDVDPMRWDL2:
                    return DiskType.DVDPRWDL;
                case CDRWinDiskTypeDVDPR:
                case CDRWinDiskTypeDVDPVR:
                    return DiskType.DVDPR;
                case CDRWinDiskTypeDVDPRDL:
                    return DiskType.DVDPRDL;
                case CDRWinDiskTypeDVDRAM:
                    return DiskType.DVDRAM;
                case CDRWinDiskTypeDVDVR:
                case CDRWinDiskTypeDVDR:
                    return DiskType.DVDR;
                case CDRWinDiskTypeDVDRDL:
                    return DiskType.DVDRDL;
                case CDRWinDiskTypeDVDRW:
                case CDRWinDiskTypeDVDRWDL:
                case CDRWinDiskTypeDVDRW2:
                    return DiskType.DVDRW;
                case CDRWinDiskTypeHDDVD:
                    return DiskType.HDDVDROM;
                case CDRWinDiskTypeHDDVDRAM:
                    return DiskType.HDDVDRAM;
                case CDRWinDiskTypeHDDVDR:
                case CDRWinDiskTypeHDDVDRDL:
                    return DiskType.HDDVDR;
                case CDRWinDiskTypeHDDVDRW:
                case CDRWinDiskTypeHDDVDRWDL:
                    return DiskType.HDDVDRW;
                case CDRWinDiskTypeBD:
                    return DiskType.BDROM;
                case CDRWinDiskTypeBDR:
                case CDRWinDiskTypeBDRDL:
                    return DiskType.BDR;
                case CDRWinDiskTypeBDRE:
                case CDRWinDiskTypeBDREDL:
                    return DiskType.BDRE;
                default:
                    return DiskType.Unknown;
            }
        }

        #endregion

        #region Unsupported features

        public override int    GetDiskSequence()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override int    GetLastDiskSequence()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetDriveManufacturer()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetDriveModel()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetDriveSerialNumber()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetDiskPartNumber()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetDiskManufacturer()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string GetDiskModel()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string   GetImageName()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        public override string   GetImageCreator()
        {
            throw new FeatureUnsupportedImageException("Feature not supported by image format");
        }

        #endregion
    }
}

