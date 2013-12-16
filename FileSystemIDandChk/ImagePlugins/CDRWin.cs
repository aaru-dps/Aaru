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
        private struct CDRWinTrackFile
        {
            public UInt32 sequence; // Track #
            public string datafile; // Path of file containing track
            public UInt64 offset;   // Offset of track start in file
            public string filetype; // Type of file
        }

        private struct CDRWinTrack
        {
            public UInt32           sequence;   // Track #
            public string           title;      // Track title (from CD-Text)
            public string           genre;      // Track genre (from CD-Text)
            public string           arranger;   // Track arranger (from CD-Text)
            public string           composer;   // Track composer (from CD-Text)
            public string           performer;  // Track performer (from CD-Text)
            public string           songwriter; // Track song writer (from CD-Text)
            public string           isrc;       // Track ISRC
            public CDRWinTrackFile  trackfile;  // File struct for this track
            public List<TrackIndex> indexes;    // Indexes on this track
            public UInt32           pregap;     // Track pre-gap in sectors
            public UInt32           postgap;    // Track post-gap in sectors
            public bool             flag_dcp;   // Digical Copy Permitted
            public bool             flag_4ch;   // Track is quadraphonic
            public bool             flag_pre;   // Track has preemphasis
            public bool             flag_scms;  // Track has SCMS
        }
#endregion

        private struct CDRWinDisc
        {
            public string            title;       // Disk title (from CD-Text)
            public string            genre;       // Disk genre (from CD-Text)
            public string            arranger;    // Disk arranger (from CD-Text)
            public string            composer;    // Disk composer (from CD-Text)
            public string            performer;   // Disk performer (from CD-Text)
            public string            songwriter;  // Disk song writer (from CD-Text)
            public string            mcn;         // Media catalog number
            public DiskType          disktype;    // Disk type
            public string            disktypestr; // Disk type string
            public string            disk_id;     // Disk CDDB ID
            public string            barcode;     // Disk UPC/EAN
            public List<Session>     sessions;    // Sessions
            public List<CDRWinTrack> tracks;      // Tracks
            public string            comment;     // Disk comment
            public string            cdtextfile;  // File containing CD-Text
        }

#region Internal consts
        // Type for FILE entity
        private const string CDRWinDiskTypeLittleEndian = "BINARY";     // Data as-is in little-endian
        private const string CDRWinDiskTypeBigEndian    = "MOTOROLA";   // Data as-is in big-endian
        private const string CDRWinDiskTypeAIFF         = "AIFF";       // Audio in Apple AIF file
        private const string CDRWinDiskTypeRIFF         = "WAVE";       // Audio in Microsoft WAV file
        private const string CDRWinDiskTypeMP3          = "MP3";        // Audio in MP3 file

        // Type for TRACK entity
        private const string CDRWinTrackTypeAudio           = "AUDIO";      // Audio track, 2352 bytes/sector
        private const string CDRWinTrackTypeCDG             = "CDG";        // CD+G track, 2448 bytes/sector (audio+subchannel)
        private const string CDRWinTrackTypeMode1           = "MODE1/2048"; // Mode 1 track, cooked, 2048 bytes/sector
        private const string CDRWinTrackTypeMode1Raw        = "MODE1/2352"; // Mode 1 track, raw, 2352 bytes/sector
        private const string CDRWinTrackTypeMode2Form1      = "MODE2/2048"; // Mode 2 form 1 track, cooked, 2048 bytes/sector
        private const string CDRWinTrackTypeMode2Form2      = "MODE2/2324"; // Mode 2 form 2 track, cooked, 2324 bytes/sector
        private const string CDRWinTrackTypeMode2Formless   = "MODE2/2336"; // Mode 2 formless track, cooked, 2336 bytes/sector
        private const string CDRWinTrackTypeMode2Raw        = "MODE2/2352"; // Mode 2 track, raw, 2352 bytes/sector
        private const string CDRWinTrackTypeCDI             = "CDI/2336";   // CD-i track, cooked, 2336 bytes/sector
        private const string CDRWinTrackTypeCDIRaw          = "CDI/2352";   // CD-i track, raw, 2352 bytes/sector

        // Type for REM ORIGINAL MEDIA-TYPE entity
        private const string CDRWinDiskTypeCD           = "CD";             // DiskType.CD
        private const string CDRWinDiskTypeCDRW         = "CD-RW";          // DiskType.CDRW
        private const string CDRWinDiskTypeCDMRW        = "CD-MRW";         // DiskType.CDMRW
        private const string CDRWinDiskTypeCDMRW2       = "CD-(MRW)";       // DiskType.CDMRW
        private const string CDRWinDiskTypeDVD          = "DVD";            // DiskType.DVDROM
        private const string CDRWinDiskTypeDVDPMRW      = "DVD+MRW";        // DiskType.DVDPRW
        private const string CDRWinDiskTypeDVDPMRW2     = "DVD+(MRW)";      // DiskType.DVDPRW
        private const string CDRWinDiskTypeDVDPMRWDL    = "DVD+MRW DL";     // DiskType.DVDPRWDL
        private const string CDRWinDiskTypeDVDPMRWDL2   = "DVD+(MRW) DL";   // DiskType.DVDPRWDL
        private const string CDRWinDiskTypeDVDPR        = "DVD+R";          // DiskType.DVDPR
        private const string CDRWinDiskTypeDVDPRDL      = "DVD+R DL";       // DiskType.DVDPRDL
        private const string CDRWinDiskTypeDVDPRW       = "DVD+RW";         // DiskType.DVDPRW
        private const string CDRWinDiskTypeDVDPRWDL     = "DVD+RW DL";      // DiskType.DVDPRWDL
        private const string CDRWinDiskTypeDVDPVR       = "DVD+VR";         // DiskType.DVDPR
        private const string CDRWinDiskTypeDVDRAM       = "DVD-RAM";        // DiskType.DVDRAM
        private const string CDRWinDiskTypeDVDR         = "DVD-R";          // DiskType.DVDR
        private const string CDRWinDiskTypeDVDRDL       = "DVD-R DL";       // DiskType.DVDRDL
        private const string CDRWinDiskTypeDVDRW        = "DVD-RW";         // DiskType.DVDRW
        private const string CDRWinDiskTypeDVDRWDL      = "DVD-RW DL";      // DiskType.DVDRWDL
        private const string CDRWinDiskTypeDVDVR        = "DVD-VR";         // DiskType.DVDR
        private const string CDRWinDiskTypeDVDRW2       = "DVDRW";          // DiskType.DVDRW
        private const string CDRWinDiskTypeHDDVD        = "HD DVD";         // DiskType.HDDVDROM
        private const string CDRWinDiskTypeHDDVDRAM     = "HD DVD-RAM";     // DiskType.HDDVDRAM
        private const string CDRWinDiskTypeHDDVDR       = "HD DVD-R";       // DiskType.HDDVDR
        private const string CDRWinDiskTypeHDDVDRDL     = "HD DVD-R DL";    // DiskType.HDDVDR
        private const string CDRWinDiskTypeHDDVDRW      = "HD DVD-RW";      // DiskType.HDDVDRW
        private const string CDRWinDiskTypeHDDVDRWDL    = "HD DVD-RW DL";   // DiskType.HDDVDRW
        private const string CDRWinDiskTypeBD           = "BD";             // DiskType.BDROM
        private const string CDRWinDiskTypeBDR          = "BD-R";           // DiskType.BDR
        private const string CDRWinDiskTypeBDRE         = "BD-RE";          // DiskType.BDRE
        private const string CDRWinDiskTypeBDRDL        = "BD-R DL";        // DiskType.BDR
        private const string CDRWinDiskTypeBDREDL       = "BD-RE DL";       // DiskType.BDRE

#endregion

#region Internal variables
        private bool initialized;
        private string imagePath;
        private StreamReader cueStream;
        private FileStream imageStream;
        private Dictionary<UInt32, CDRWinTrackFile> trackFiles; // Dictionary, index is track #, value is TrackFile
        private CDRWinDisc discimage;
#endregion

#region Parsing regexs
        private const string SessionRegEx    = "REM\\s+SESSION\\s+(?<number>\\d+)$";
        private const string DiskTypeRegEx   = "REM\\s+ORIGINAL MEDIA-TYPE:\\s+(?<mediatype>.+)$";
        private const string LeadOutRegEx    = "REM\\s+LEAD-OUT\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        private const string LBARegEx        = "REM MSF:\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)\\s+=\\s+LBA:\\s+(?<lba>[\\d]+)$"; // Not checked
        private const string DiskIDRegEx     = "DISC_ID\\s+(?<diskid>[\\da-f]{8})$";
        private const string BarCodeRegEx    = "UPC_EAN\\s+(?<barcode>[\\d]{12,13})$";
        private const string CommentRegEx    = "REM\\s+(?<comment>.+)$";
        private const string CDTextRegEx     = "CDTEXTFILE\\s+(?<filename>.+)$";
        private const string MCNRegEx        = "CATALOG\\s+(?<catalog>\\d{13})$";
        private const string TitleRegEx      = "TITLE\\s+(?<title>.+)$";
        private const string GenreRegEx      = "GENRE\\s+(?<genre>.+)$";
        private const string ArrangerRegEx   = "ARRANGER\\s+(?<arranger>.+)$";
        private const string ComposerRegEx   = "COMPOSER\\s+(?<composer>.+)$";
        private const string PerformerRegEx  = "PERFORMER\\s+(?<performer>.+)$";
        private const string SongWriterRegEx = "SONGWRITER\\s+(?<songwriter>.+)$";
        private const string FileRegEx       = "FILE\\s+(?<filename>.+)\\s+(?<type>\\S+)$";
        private const string TrackRegEx      = "TRACK\\s+(?<number>\\d+)\\s+(?<type>\\S+)$";
        private const string ISRCRegEx       = "ISRC\\s+(?<isrc>\\w{12})$";
        private const string IndexRegEx      = "INDEX\\s+(?<index>\\d+)\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        private const string PregapRegEx     = "PREGAP\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        private const string PostgapRegex    = "POSTGAP\\s+(?<msf>[\\d]+:[\\d]+:[\\d]+)$";
        private const string FlagsRegEx      = "FLAGS\\s+(((?<dcp>DCP)|(?<quad>4CH)|(?<pre>PRE)|(?<scms>SCMS))\\s*)+$";
#endregion

#region Methods
        public CDRWin (PluginBase Core)
        {
            base.Name = "CDRWin cuesheet handler";
            base.PluginUUID = new Guid("664568B2-15D4-4E64-8A7A-20BDA8B8386F");
            this.imagePath = "";
        }

        // Due to .cue format, this method must parse whole file, ignoring errors (those will be thrown by OpenImage()).
        public override bool IdentifyImage(string imagePath)
        {
            this.imagePath = imagePath;

            try
            {
                this.cueStream = new StreamReader(this.imagePath);
                int line = 0;

                while (this.cueStream.Peek() >= 0) 
                {
                    line++;
                    string _line = this.cueStream.ReadLine();

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

                    if(!Sm.Success && !Rm.Success && !Cm.Success && !Fm.Success)
                        return false;
                    else
                        return true;
                }

                return false;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception trying to identify image file {0}", this.imagePath);
                Console.WriteLine("Exception: {0}", ex.Message);
                Console.WriteLine("Stack trace: {0}", ex.StackTrace);
                return false;
            }
        }

        public override bool OpenImage(string filename)
        {
            if (filename == "")
                return false;

            this.imagePath = imagePath;

            try
            {
                this.cueStream = new StreamReader(this.imagePath);
                int line = 0;
                bool intrack = false;
                string currentfile = "";
                string currentfileformat = "";
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
                this.discimage = new CDRWinDisc();
                this.discimage.sessions = new List<Session>();
                this.discimage.tracks = new List<CDRWinTrack>();

                CDRWinTrack currenttrack = new CDRWinTrack();

                while (this.cueStream.Peek() >= 0) 
                {
                    line++;
                    string _line = this.cueStream.ReadLine();

                    MatchSession = RegexSession.Match(_line);
                    MatchDiskType = RegexDiskType.Match(_line);
                    MatchComment = RegexComment.Match(_line);
                    MatchLBA = RegexLBA.Match(_line);   // Unhandled, just ignored
                    MatchLeadOut = RegexLeadOut.Match(_line); // Unhandled, just ignored

                    if(MatchDiskType.Success && intrack == false)
                    {
                        Console.WriteLine("DEBUG (CDRWin plugin): Found REM ORIGINAL MEDIA TYPE at line {0}", line);
                        this.discimage.disktypestr = MatchDiskType.Groups[1].Value;
                    }
                    else if(MatchSession.Success && intrack == false)
                    {
                        Console.WriteLine("DEBUG (CDRWin plugin): Found REM SESSION at line {0}", line);
                        currentsession = Byte.Parse(MatchSession.Groups[1].Value);

                        // What happens between sessions
                    }
                    else if(MatchLBA.Success)
                    {
                        Console.WriteLine("DEBUG (CDRWin plugin): Found REM MSF at line {0}", line);
                        // Just ignored
                    }
                    else if(MatchLeadOut.Success)
                    {
                        Console.WriteLine("DEBUG (CDRWin plugin): Found REM LEAD-OUT at line {0}", line);
                        // Just ignored
                    }
                    else if(MatchComment.Success && intrack == false)
                    {
                        Console.WriteLine("DEBUG (CDRWin plugin): Found REM at line {0}", line);
                        this.discimage.comment = MatchComment.Groups[1].Value;
                    }
                    else if((MatchComment.Success || MatchSession.Success || MatchDiskType.Success) && intrack == true)
                    {
                        throw new FeatureUnsupportedImageException(String.Format("Found comment/session/disktype field after a track in line {0}", line));
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
                            Console.WriteLine("DEBUG (CDRWin plugin): Found ARRANGER at line {0}", line);
                            if(intrack == true)
                                currenttrack.arranger = MatchArranger.Groups[1].Value;
                            else
                                this.discimage.arranger = MatchArranger.Groups[1].Value;
                        }
                        else if(MatchBarCode.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found CATALOG at line {0}", line);
                            if(intrack == false)
                                this.discimage.barcode = MatchBarCode.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found barcode field in incorrect place at line {0}", line));
                        }
                        else if(MatchCDText.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found CDTEXTFILE at line {0}", line);
                            if(intrack == false)
                                this.discimage.cdtextfile = MatchCDText.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found CD-Text file field in incorrect place at line {0}", line));
                        }
                        else if(MatchComposer.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found COMPOSER at line {0}", line);
                            if(intrack == true)
                                currenttrack.arranger = MatchComposer.Groups[1].Value;
                            else
                                this.discimage.arranger = MatchComposer.Groups[1].Value;
                        }
                        else if(MatchDiskID.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found DISC_ID at line {0}", line);
                            if(intrack == false)
                                this.discimage.disk_id = MatchDiskID.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found CDDB ID field in incorrect place at line {0}", line));
                        }
                        else if(MatchFile.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found FILE at line {0}", line);
                            currentfile = MatchFile.Groups[1].Value;
                            currentfileformat = MatchFile.Groups[2].Value;
                        }
                        else if(MatchFlags.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found FLAGS at line {0}", line);
                            if(intrack == false)
                                throw new FeatureUnsupportedImageException(String.Format("Found FLAGS field in incorrect place at line {0}", line));
                            else
                            {
                                // Not yet implemented
                            }
                        }
                        else if(MatchGenre.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found GENRE at line {0}", line);
                            if(intrack == true)
                                currenttrack.genre = MatchGenre.Groups[1].Value;
                            else
                                this.discimage.genre = MatchGenre.Groups[1].Value;
                        }
                        else if(MatchIndex.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found INDEX at line {0}", line);
                            if(intrack == false)
                                throw new FeatureUnsupportedImageException(String.Format("Found INDEX before a track {0}", line));
                            else
                            {
                                // Not yet implemented
                            }
                        }
                        else if(MatchISRC.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found ISRC at line {0}", line);
                            if(intrack == false)
                                throw new FeatureUnsupportedImageException(String.Format("Found ISRC before a track {0}", line));
                            else
                                currenttrack.isrc = MatchISRC.Groups[1].Value;
                        }
                        else if(MatchMCN.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found CATALOG at line {0}", line);
                            if(intrack == false)
                                this.discimage.mcn = MatchMCN.Groups[1].Value;
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found CATALOG field in incorrect place at line {0}", line));
                        }
                        else if(MatchPerformer.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found PERFORMER at line {0}", line);
                            if(intrack == true)
                                currenttrack.performer = MatchPerformer.Groups[1].Value;
                            else
                                this.discimage.performer = MatchPerformer.Groups[1].Value;
                        }
                        else if(MatchPostgap.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found POSTGAP at line {0}", line);
                            if(intrack == true)
                            {
                                // Not yet implemented
                            }
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found POSTGAP field before a track at line {0}", line));
                        }
                        else if(MatchPregap.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found PREGAP at line {0}", line);
                            if(intrack == true)
                            {
                                // Not yet implemented
                            }
                            else
                                throw new FeatureUnsupportedImageException(String.Format("Found PREGAP field before a track at line {0}", line));
                        }
                        else if(MatchSongWriter.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found SONGWRITER at line {0}", line);
                            if(intrack == true)
                                currenttrack.songwriter = MatchSongWriter.Groups[1].Value;
                            else
                                this.discimage.songwriter = MatchSongWriter.Groups[1].Value;
                        }
                        else if(MatchTitle.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found TITLE at line {0}", line);
                            if(intrack == true)
                                currenttrack.title = MatchTitle.Groups[1].Value;
                            else
                                this.discimage.title = MatchTitle.Groups[1].Value;
                        }
                        else if(MatchTrack.Success)
                        {
                            Console.WriteLine("DEBUG (CDRWin plugin): Found TRACK at line {0}", line);
                            if(currentfile == "")
                                throw new FeatureUnsupportedImageException(String.Format("Found TRACK field before a file is defined at line {0}", line));
                            else
                            {
                                if(intrack == true)
                                {
                                    this.discimage.tracks.Add(currenttrack);
                                }
                                currenttrack = new CDRWinTrack();
                                intrack = true;

                                // TODO
                            }
                        }
                    }
                }

                return false;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception trying to identify image file {0}", this.imagePath);
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
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }
        public override UInt64 GetSectors()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }
        public override UInt32 GetSectorSize()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadDiskTag(DiskTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSector(UInt64 SectorAddress)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorTag(UInt64 SectorAddress, SectorTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSector(UInt64 SectorAddress, UInt32 track)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorTag(UInt64 SectorAddress, UInt32 track, SectorTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectors(UInt64 SectorAddress, UInt32 length)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorsTag(UInt64 SectorAddress, UInt32 length, SectorTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectors(UInt64 SectorAddress, UInt32 length, UInt32 track)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorsTag(UInt64 SectorAddress, UInt32 length, UInt32 track, SectorTagType tag)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorLong(UInt64 SectorAddress)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorLong(UInt64 SectorAddress, UInt32 track)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorsLong(UInt64 SectorAddress, UInt32 length)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override byte[] ReadSectorsLong(UInt64 SectorAddress, UInt32 length, UInt32 track)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string   GetImageFormat()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string   GetImageVersion()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string   GetImageApplication()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string   GetImageApplicationVersion()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override DateTime GetImageCreationTime()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override DateTime GetImageLastModificationTime()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string   GetImageComments()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override string GetDiskSerialNumber()
        {
            return this.GetDiskBarcode();
        }

        public override string GetDiskBarcode()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override DiskType GetDiskType()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override List<PartPlugins.Partition> GetPartitions()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override List<Track> GetTracks()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override List<Track> GetSessionTracks(Session Session)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override List<Track> GetSessionTracks(UInt16 Session)
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }

        public override List<Session> GetSessions()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
        }
#endregion

#region Unsupported features
        public override int    GetDiskSequence()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override int    GetLastDiskSequence()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDriveManufacturer()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDriveModel()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDriveSerialNumber()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDiskPartNumber()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDiskManufacturer()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string GetDiskModel()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string   GetImageName()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }

        public override string   GetImageCreator()
        {
            throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
        }
#endregion
    }
}

