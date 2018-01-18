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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    // TODO: Doesn't support compositing from several files
    // TODO: Doesn't support silences that are not in files
    public class Cdrdao : IWritableImage
    {
        /// <summary>Audio track, 2352 bytes/sector</summary>
        const string CDRDAO_TRACK_TYPE_AUDIO = "AUDIO";
        /// <summary>Mode 1 track, cooked, 2048 bytes/sector</summary>
        const string CDRDAO_TRACK_TYPE_MODE1 = "MODE1";
        /// <summary>Mode 1 track, raw, 2352 bytes/sector</summary>
        const string CDRDAO_TRACK_TYPE_MODE1_RAW = "MODE1_RAW";
        /// <summary>Mode 2 mixed formless, cooked, 2336 bytes/sector</summary>
        const string CDRDAO_TRACK_TYPE_MODE2 = "MODE2";
        /// <summary>Mode 2 form 1 track, cooked, 2048 bytes/sector</summary>
        const string CDRDAO_TRACK_TYPE_MODE2_FORM1 = "MODE2_FORM1";
        /// <summary>Mode 2 form 2 track, cooked, 2324 bytes/sector</summary>
        const string CDRDAO_TRACK_TYPE_MODE2_FORM2 = "MODE2_FORM2";
        /// <summary>Mode 2 mixed forms track, cooked, 2336 bytes/sector</summary>
        const string CDRDAO_TRACK_TYPE_MODE2_MIX = "MODE2_FORM_MIX";
        /// <summary>Mode 2 track, raw, 2352 bytes/sector</summary>
        const string CDRDAO_TRACK_TYPE_MODE2_RAW = "MODE2_RAW";

        const string REGEX_COMMENT    = @"^\s*\/{2}(?<comment>.+)$";
        const string REGEX_COPY       = @"^\s*(?<no>NO)?\s*COPY";
        const string REGEX_DISCTYPE   = @"^\s*(?<type>(CD_DA|CD_ROM_XA|CD_ROM|CD_I))";
        const string REGEX_EMPHASIS   = @"^\s*(?<no>NO)?\s*PRE_EMPHASIS";
        const string REGEX_FILE_AUDIO =
            @"^\s*(AUDIO)?FILE\s*""(?<filename>.+)""\s*(#(?<base_offset>\d+))?\s*((?<start>[\d]+:[\d]+:[\d]+)|(?<start_num>\d+))\s*(?<length>[\d]+:[\d]+:[\d]+)?";
        const string REGEX_FILE_DATA =
            @"^\s*DATAFILE\s*""(?<filename>.+)""\s*(#(?<base_offset>\d+))?\s*(?<length>[\d]+:[\d]+:[\d]+)?";
        const string REGEX_INDEX  = @"^\s*INDEX\s*(?<address>\d+:\d+:\d+)";
        const string REGEX_ISRC   = @"^\s*ISRC\s*""(?<isrc>[A-Z0-9]{5,5}[0-9]{7,7})""";
        const string REGEX_MCN    = @"^\s*CATALOG\s*""(?<catalog>[\d]{13,13})""";
        const string REGEX_PREGAP = @"^\s*START\s*(?<address>\d+:\d+:\d+)?";
        const string REGEX_STEREO = @"^\s*(?<num>(TWO|FOUR))_CHANNEL_AUDIO";
        const string REGEX_TRACK  =
            @"^\s*TRACK\s*(?<type>(AUDIO|MODE1_RAW|MODE1|MODE2_FORM1|MODE2_FORM2|MODE2_FORM_MIX|MODE2_RAW|MODE2))\s*(?<subchan>(RW_RAW|RW))?";
        const string REGEX_ZERO_AUDIO  = @"^\s*SILENCE\s*(?<length>\d+:\d+:\d+)";
        const string REGEX_ZERO_DATA   = @"^\s*ZERO\s*(?<length>\d+:\d+:\d+)";
        const string REGEX_ZERO_PREGAP = @"^\s*PREGAP\s*(?<length>\d+:\d+:\d+)";

        // CD-Text
        const string REGEX_ARRANGER   = @"^\s*ARRANGER\s*""(?<arranger>.+)""";
        const string REGEX_COMPOSER   = @"^\s*COMPOSER\s*""(?<composer>.+)""";
        const string REGEX_DISC_ID    = @"^\s*DISC_ID\s*""(?<discid>.+)""";
        const string REGEX_MESSAGE    = @"^\s*MESSAGE\s*""(?<message>.+)""";
        const string REGEX_PERFORMER  = @"^\s*PERFORMER\s*""(?<performer>.+)""";
        const string REGEX_SONGWRITER = @"^\s*SONGWRITER\s*""(?<songwriter>.+)""";
        const string REGEX_TITLE      = @"^\s*TITLE\s*""(?<title>.+)""";
        const string REGEX_UPC        = @"^\s*UPC_EAN\s*""(?<catalog>[\d]{13,13})""";

        // Unused
        const string REGEX_CD_TEXT          = @"^\s*CD_TEXT\s*\{";
        const string REGEX_CLOSURE          = @"^\s*\}";
        const string REGEX_LANGUAGE         = @"^\s*LANGUAGE\s*(?<code>\d+)\s*\{";
        const string REGEX_LANGUAGE_MAP     = @"^\s*LANGUAGE_MAP\s*\{";
        const string REGEX_LANGUAGE_MAPPING = @"^\s*(?<code>\d+)\s?\:\s?(?<language>\d+|\w+)";

        IFilter      cdrdaoFilter;
        StreamWriter descriptorStream;
        CdrdaoDisc   discimage;
        ImageInfo    imageInfo;
        Stream       imageStream;
        /// <summary>Dictionary, index is track #, value is TrackFile</summary>
        Dictionary<uint, ulong>      offsetmap;
        bool                         separateTracksWriting;
        StreamReader                 tocStream;
        Dictionary<byte, byte>       trackFlags;
        Dictionary<byte, string>     trackIsrcs;
        string                       writingBaseName;
        Dictionary<uint, FileStream> writingStreams;
        List<Track>                  writingTracks;

        public Cdrdao()
        {
            imageInfo = new ImageInfo
            {
                ReadableSectorTags    = new List<SectorTagType>(),
                ReadableMediaTags     = new List<MediaTagType>(),
                HasPartitions         = true,
                HasSessions           = true,
                Version               = null,
                ApplicationVersion    = null,
                MediaTitle            = null,
                Creator               = null,
                MediaManufacturer     = null,
                MediaModel            = null,
                MediaPartNumber       = null,
                MediaSequence         = 0,
                LastMediaSequence     = 0,
                DriveManufacturer     = null,
                DriveModel            = null,
                DriveSerialNumber     = null,
                DriveFirmwareRevision = null
            };
        }

        public ImageInfo Info => imageInfo;

        public string Name => "CDRDAO tocfile";
        public Guid   Id   => new Guid("04D7BA12-1BE8-44D4-97A4-1B48A505463E");

        public string Format => "CDRDAO tocfile";

        public List<Partition> Partitions { get; set; }

        public List<Track> Tracks
        {
            get
            {
                List<Track> tracks = new List<Track>();

                foreach(CdrdaoTrack cdrTrack in discimage.Tracks)
                {
                    Track dicTrack = new Track
                    {
                        Indexes                = cdrTrack.Indexes,
                        TrackDescription       = cdrTrack.Title,
                        TrackStartSector       = cdrTrack.StartSector,
                        TrackPregap            = cdrTrack.Pregap,
                        TrackSession           = 1,
                        TrackSequence          = cdrTrack.Sequence,
                        TrackType              = CdrdaoTrackTypeToTrackType(cdrTrack.Tracktype),
                        TrackFilter            = cdrTrack.Trackfile.Datafilter,
                        TrackFile              = cdrTrack.Trackfile.Datafilter.GetFilename(),
                        TrackFileOffset        = cdrTrack.Trackfile.Offset,
                        TrackFileType          = cdrTrack.Trackfile.Filetype,
                        TrackRawBytesPerSector = cdrTrack.Bps,
                        TrackBytesPerSector    = CdrdaoTrackTypeToCookedBytesPerSector(cdrTrack.Tracktype)
                    };

                    dicTrack.TrackEndSector = dicTrack.TrackStartSector + cdrTrack.Sectors - 1;
                    if(!cdrTrack.Indexes.TryGetValue(0, out dicTrack.TrackStartSector))
                        cdrTrack.Indexes.TryGetValue(1, out dicTrack.TrackStartSector);
                    if(cdrTrack.Subchannel)
                    {
                        dicTrack.TrackSubchannelType = cdrTrack.Packedsubchannel
                                                           ? TrackSubchannelType.PackedInterleaved
                                                           : TrackSubchannelType.RawInterleaved;
                        dicTrack.TrackSubchannelFilter = cdrTrack.Trackfile.Datafilter;
                        dicTrack.TrackSubchannelFile   = cdrTrack.Trackfile.Datafilter.GetFilename();
                        dicTrack.TrackSubchannelOffset = cdrTrack.Trackfile.Offset;
                    }
                    else dicTrack.TrackSubchannelType = TrackSubchannelType.None;

                    tracks.Add(dicTrack);
                }

                return tracks;
            }
        }

        public List<Session> Sessions => throw new NotImplementedException();

        public bool Identify(IFilter imageFilter)
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
                    if(i >= imageFilter.GetDataForkStream().Length) break;

                    if(testArray[i] == 0)
                    {
                        if(twoConsecutiveNulls) return false;

                        twoConsecutiveNulls = true;
                    }
                    else twoConsecutiveNulls = false;

                    if(testArray[i] < 0x20 && testArray[i] != 0x0A && testArray[i] != 0x0D && testArray[i] != 0x00)
                        return false;
                }

                tocStream = new StreamReader(imageFilter.GetDataForkStream());

                Regex cr = new Regex(REGEX_COMMENT);
                Regex dr = new Regex(REGEX_DISCTYPE);

                while(tocStream.Peek() >= 0)
                {
                    string line = tocStream.ReadLine();

                    Match dm = dr.Match(line ?? throw new InvalidOperationException());
                    Match cm = cr.Match(line);

                    // Skip comments at start of file
                    if(cm.Success) continue;

                    return dm.Success;
                }

                return false;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", cdrdaoFilter.GetFilename());
                DicConsole.ErrorWriteLine("Exception: {0}",                              ex.Message);
                DicConsole.ErrorWriteLine("Stack trace: {0}",                            ex.StackTrace);
                return false;
            }
        }

        public bool Open(IFilter imageFilter)
        {
            if(imageFilter == null) return false;

            cdrdaoFilter = imageFilter;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                tocStream    = new StreamReader(imageFilter.GetDataForkStream());
                bool intrack = false;

                // Initialize all RegExs
                Regex regexComment         = new Regex(REGEX_COMMENT);
                Regex regexDiskType        = new Regex(REGEX_DISCTYPE);
                Regex regexMcn             = new Regex(REGEX_MCN);
                Regex regexTrack           = new Regex(REGEX_TRACK);
                Regex regexCopy            = new Regex(REGEX_COPY);
                Regex regexEmphasis        = new Regex(REGEX_EMPHASIS);
                Regex regexStereo          = new Regex(REGEX_STEREO);
                Regex regexIsrc            = new Regex(REGEX_ISRC);
                Regex regexIndex           = new Regex(REGEX_INDEX);
                Regex regexPregap          = new Regex(REGEX_PREGAP);
                Regex regexZeroPregap      = new Regex(REGEX_ZERO_PREGAP);
                Regex regexZeroData        = new Regex(REGEX_ZERO_DATA);
                Regex regexZeroAudio       = new Regex(REGEX_ZERO_AUDIO);
                Regex regexAudioFile       = new Regex(REGEX_FILE_AUDIO);
                Regex regexFile            = new Regex(REGEX_FILE_DATA);
                Regex regexTitle           = new Regex(REGEX_TITLE);
                Regex regexPerformer       = new Regex(REGEX_PERFORMER);
                Regex regexSongwriter      = new Regex(REGEX_SONGWRITER);
                Regex regexComposer        = new Regex(REGEX_COMPOSER);
                Regex regexArranger        = new Regex(REGEX_ARRANGER);
                Regex regexMessage         = new Regex(REGEX_MESSAGE);
                Regex regexDiscId          = new Regex(REGEX_DISC_ID);
                Regex regexUpc             = new Regex(REGEX_UPC);
                Regex regexCdText          = new Regex(REGEX_CD_TEXT);
                Regex regexLanguage        = new Regex(REGEX_LANGUAGE);
                Regex regexClosure         = new Regex(REGEX_CLOSURE);
                Regex regexLanguageMap     = new Regex(REGEX_LANGUAGE_MAP);
                Regex regexLanguageMapping = new Regex(REGEX_LANGUAGE_MAPPING);

                // Initialize all RegEx matches
                Match matchComment;
                Match matchDiskType;

                // Initialize disc
                discimage = new CdrdaoDisc {Tracks = new List<CdrdaoTrack>(), Comment = ""};

                CdrdaoTrack currenttrack       = new CdrdaoTrack();
                uint        currentTrackNumber = 0;
                currenttrack.Indexes           = new Dictionary<int, ulong>();
                currenttrack.Pregap            = 0;
                ulong         currentSector    = 0;
                int           nextindex        = 2;
                StringBuilder commentBuilder   = new StringBuilder();

                tocStream = new StreamReader(cdrdaoFilter.GetDataForkStream());
                string line;
                int    lineNumber = 0;

                while(tocStream.Peek() >= 0)
                {
                    lineNumber++;
                    line = tocStream.ReadLine();

                    matchDiskType = regexDiskType.Match(line ?? throw new InvalidOperationException());
                    matchComment  = regexComment.Match(line);

                    // Skip comments at start of file
                    if(matchComment.Success) continue;

                    if(!matchDiskType.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Not a CDRDAO TOC or TOC type not in line {0}.",
                                                  lineNumber);
                        return false;
                    }

                    break;
                }

                tocStream  = new StreamReader(cdrdaoFilter.GetDataForkStream());
                lineNumber = 0;

                tocStream.BaseStream.Position = 0;
                while(tocStream.Peek() >= 0)
                {
                    lineNumber++;
                    line = tocStream.ReadLine();

                    matchComment               = regexComment.Match(line ?? throw new InvalidOperationException());
                    matchDiskType              = regexDiskType.Match(line);
                    Match matchMcn             = regexMcn.Match(line);
                    Match matchTrack           = regexTrack.Match(line);
                    Match matchCopy            = regexCopy.Match(line);
                    Match matchEmphasis        = regexEmphasis.Match(line);
                    Match matchStereo          = regexStereo.Match(line);
                    Match matchIsrc            = regexIsrc.Match(line);
                    Match matchIndex           = regexIndex.Match(line);
                    Match matchPregap          = regexPregap.Match(line);
                    Match matchZeroPregap      = regexZeroPregap.Match(line);
                    Match matchZeroData        = regexZeroData.Match(line);
                    Match matchZeroAudio       = regexZeroAudio.Match(line);
                    Match matchAudioFile       = regexAudioFile.Match(line);
                    Match matchFile            = regexFile.Match(line);
                    Match matchTitle           = regexTitle.Match(line);
                    Match matchPerformer       = regexPerformer.Match(line);
                    Match matchSongwriter      = regexSongwriter.Match(line);
                    Match matchComposer        = regexComposer.Match(line);
                    Match matchArranger        = regexArranger.Match(line);
                    Match matchMessage         = regexMessage.Match(line);
                    Match matchDiscId          = regexDiscId.Match(line);
                    Match matchUpc             = regexUpc.Match(line);
                    Match matchCdText          = regexCdText.Match(line);
                    Match matchLanguage        = regexLanguage.Match(line);
                    Match matchClosure         = regexClosure.Match(line);
                    Match matchLanguageMap     = regexLanguageMap.Match(line);
                    Match matchLanguageMapping = regexLanguageMapping.Match(line);

                    if(matchComment.Success)
                    {
                        // Ignore "// Track X" comments
                        if(matchComment.Groups["comment"].Value.StartsWith(" Track ", StringComparison.Ordinal))
                            continue;

                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found comment \"{1}\" at line {0}", lineNumber,
                                                  matchComment.Groups["comment"].Value.Trim());
                        commentBuilder.AppendLine(matchComment.Groups["comment"].Value.Trim());
                    }
                    else if(matchDiskType.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found {1} at line {0}", lineNumber,
                                                  matchDiskType.Groups["type"].Value);
                        discimage.Disktypestr = matchDiskType.Groups["type"].Value;
                        switch(matchDiskType.Groups["type"].Value)
                        {
                            case "CD_DA":
                                discimage.Disktype = MediaType.CDDA;
                                break;
                            case "CD_ROM":
                                discimage.Disktype = MediaType.CDROM;
                                break;
                            case "CD_ROM_XA":
                                discimage.Disktype = MediaType.CDROMXA;
                                break;
                            case "CD_I":
                                discimage.Disktype = MediaType.CDI;
                                break;
                            default:
                                discimage.Disktype = MediaType.CD;
                                break;
                        }
                    }
                    else if(matchMcn.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found CATALOG \"{1}\" at line {0}", lineNumber,
                                                  matchMcn.Groups["catalog"].Value);
                        discimage.Mcn = matchMcn.Groups["catalog"].Value;
                    }
                    else if(matchTrack.Success)
                    {
                        if(matchTrack.Groups["subchan"].Value == "")
                            DicConsole.DebugWriteLine("CDRDAO plugin",
                                                      "Found TRACK type \"{1}\" with no subchannel at line {0}",
                                                      lineNumber, matchTrack.Groups["type"].Value);
                        else
                            DicConsole.DebugWriteLine("CDRDAO plugin",
                                                      "Found TRACK type \"{1}\" subchannel {2} at line {0}", lineNumber,
                                                      matchTrack.Groups["type"].Value,
                                                      matchTrack.Groups["subchan"].Value);

                        if(intrack)
                        {
                            currentSector += currenttrack.Sectors;
                            if(currenttrack.Pregap != currenttrack.Sectors && !currenttrack.Indexes.ContainsKey(1))
                                currenttrack.Indexes.Add(1, currenttrack.StartSector + currenttrack.Pregap);
                            discimage.Tracks.Add(currenttrack);
                            currenttrack = new CdrdaoTrack {Indexes = new Dictionary<int, ulong>(), Pregap = 0};
                            nextindex    = 2;
                        }

                        currentTrackNumber++;
                        intrack = true;

                        switch(matchTrack.Groups["type"].Value)
                        {
                            case "AUDIO":
                            case "MODE1_RAW":
                            case "MODE2_RAW":
                                currenttrack.Bps = 2352;
                                break;
                            case "MODE1":
                            case "MODE2_FORM1":
                                currenttrack.Bps = 2048;
                                break;
                            case "MODE2_FORM2":
                                currenttrack.Bps = 2324;
                                break;
                            case "MODE2":
                            case "MODE2_FORM_MIX":
                                currenttrack.Bps = 2336;
                                break;
                            default:
                                throw new
                                    NotSupportedException($"Track mode {matchTrack.Groups["type"].Value} is unsupported");
                        }

                        switch(matchTrack.Groups["subchan"].Value)
                        {
                            case "": break;
                            case "RW":
                                currenttrack.Packedsubchannel = true;
                                goto case "RW_RAW";
                            case "RW_RAW":
                                currenttrack.Subchannel = true;
                                break;
                            default:
                                throw new
                                    NotSupportedException($"Track subchannel mode {matchTrack.Groups["subchan"].Value} is unsupported");
                        }

                        currenttrack.Tracktype = matchTrack.Groups["type"].Value;

                        currenttrack.Sequence    = currentTrackNumber;
                        currenttrack.StartSector = currentSector;
                    }
                    else if(matchCopy.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found {1} COPY at line {0}", lineNumber,
                                                  matchCopy.Groups["no"].Value);
                        currenttrack.FlagDcp |= intrack && matchCopy.Groups["no"].Value == "";
                    }
                    else if(matchEmphasis.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found {1} PRE_EMPHASIS at line {0}", lineNumber,
                                                  matchEmphasis.Groups["no"].Value);
                        currenttrack.FlagPre |= intrack && matchEmphasis.Groups["no"].Value == "";
                    }
                    else if(matchStereo.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found {1}_CHANNEL_AUDIO at line {0}", lineNumber,
                                                  matchStereo.Groups["num"].Value);
                        currenttrack.Flag_4Ch |= intrack && matchStereo.Groups["num"].Value == "FOUR";
                    }
                    else if(matchIsrc.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found ISRC \"{1}\" at line {0}", lineNumber,
                                                  matchIsrc.Groups["isrc"].Value);
                        if(intrack) currenttrack.Isrc = matchIsrc.Groups["isrc"].Value;
                    }
                    else if(matchIndex.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found INDEX \"{1}\" at line {0}", lineNumber,
                                                  matchIndex.Groups["address"].Value);

                        string[] lengthString = matchFile.Groups["length"].Value.Split(':');
                        ulong    nextIndexPos = ulong.Parse(lengthString[0]) * 60 * 75 +
                                                ulong.Parse(lengthString[1]) * 75      +
                                                ulong.Parse(lengthString[2]);
                        currenttrack.Indexes.Add(nextindex,
                                                 nextIndexPos + currenttrack.Pregap + currenttrack.StartSector);
                    }
                    else if(matchPregap.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found START \"{1}\" at line {0}", lineNumber,
                                                  matchPregap.Groups["address"].Value);

                        currenttrack.Indexes.Add(0, currenttrack.StartSector);
                        if(matchPregap.Groups["address"].Value != "")
                        {
                            string[] lengthString = matchPregap.Groups["address"].Value.Split(':');
                            currenttrack.Pregap   = ulong.Parse(lengthString[0]) * 60 * 75 +
                                                    ulong.Parse(lengthString[1]) * 75      +
                                                    ulong.Parse(lengthString[2]);
                        }
                        else currenttrack.Pregap = currenttrack.Sectors;
                    }
                    else if(matchZeroPregap.Success)
                    {
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found PREGAP \"{1}\" at line {0}", lineNumber,
                                                  matchZeroPregap.Groups["length"].Value);
                        currenttrack.Indexes.Add(0, currenttrack.StartSector);
                        string[] lengthString = matchZeroPregap.Groups["length"].Value.Split(':');
                        currenttrack.Pregap   = ulong.Parse(lengthString[0]) * 60 * 75 +
                                                ulong.Parse(lengthString[1]) * 75      + ulong.Parse(lengthString[2]);
                    }
                    else if(matchZeroData.Success)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found ZERO \"{1}\" at line {0}", lineNumber,
                                                  matchZeroData.Groups["length"].Value);
                    else if(matchZeroAudio.Success)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "Found SILENCE \"{1}\" at line {0}", lineNumber,
                                                  matchZeroAudio.Groups["length"].Value);
                    else
                    {
                        FiltersList filtersList;
                        if(matchAudioFile.Success)
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found AUDIOFILE \"{1}\" at line {0}",
                                                      lineNumber, matchAudioFile.Groups["filename"].Value);

                            filtersList            = new FiltersList();
                            currenttrack.Trackfile = new CdrdaoTrackFile
                            {
                                Datafilter =
                                    filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                       matchAudioFile.Groups["filename"].Value)),
                                Datafile = matchAudioFile.Groups["filename"].Value,
                                Offset   = matchAudioFile.Groups["base_offset"].Value != ""
                                               ? ulong.Parse(matchAudioFile.Groups["base_offset"].Value)
                                               : 0,
                                Filetype = "BINARY",
                                Sequence = currentTrackNumber
                            };

                            ulong startSectors = 0;

                            if(matchAudioFile.Groups["start"].Value != "")
                            {
                                string[] startString = matchAudioFile.Groups["start"].Value.Split(':');
                                startSectors         = ulong.Parse(startString[0]) * 60 * 75 +
                                                       ulong.Parse(startString[1]) * 75      +
                                                       ulong.Parse(startString[2]);
                            }

                            currenttrack.Trackfile.Offset += startSectors * currenttrack.Bps;

                            if(matchAudioFile.Groups["length"].Value != "")
                            {
                                string[] lengthString = matchAudioFile.Groups["length"].Value.Split(':');
                                currenttrack.Sectors  = ulong.Parse(lengthString[0]) * 60 * 75 +
                                                        ulong.Parse(lengthString[1]) * 75      +
                                                        ulong.Parse(lengthString[2]);
                            }
                            else
                                currenttrack.Sectors =
                                    ((ulong)currenttrack.Trackfile.Datafilter.GetDataForkLength() -
                                     currenttrack.Trackfile.Offset) / currenttrack.Bps;
                        }
                        else if(matchFile.Success)
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found DATAFILE \"{1}\" at line {0}", lineNumber,
                                                      matchFile.Groups["filename"].Value);

                            filtersList            = new FiltersList();
                            currenttrack.Trackfile = new CdrdaoTrackFile
                            {
                                Datafilter =
                                    filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                       matchFile.Groups["filename"].Value)),
                                Datafile = matchAudioFile.Groups["filename"].Value,
                                Offset   = matchFile.Groups["base_offset"].Value != ""
                                               ? ulong.Parse(matchFile.Groups["base_offset"].Value)
                                               : 0,
                                Filetype = "BINARY",
                                Sequence = currentTrackNumber
                            };

                            if(matchFile.Groups["length"].Value != "")
                            {
                                string[] lengthString = matchFile.Groups["length"].Value.Split(':');
                                currenttrack.Sectors  = ulong.Parse(lengthString[0]) * 60 * 75 +
                                                        ulong.Parse(lengthString[1]) * 75      +
                                                        ulong.Parse(lengthString[2]);
                            }
                            else
                                currenttrack.Sectors =
                                    ((ulong)currenttrack.Trackfile.Datafilter.GetDataForkLength() -
                                     currenttrack.Trackfile.Offset) / currenttrack.Bps;
                        }
                        else if(matchTitle.Success)
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found TITLE \"{1}\" at line {0}", lineNumber,
                                                      matchTitle.Groups["title"].Value);
                            if(intrack) currenttrack.Title = matchTitle.Groups["title"].Value;
                            else discimage.Title           = matchTitle.Groups["title"].Value;
                        }
                        else if(matchPerformer.Success)
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found PERFORMER \"{1}\" at line {0}",
                                                      lineNumber, matchPerformer.Groups["performer"].Value);
                            if(intrack) currenttrack.Performer = matchPerformer.Groups["performer"].Value;
                            else discimage.Performer           = matchPerformer.Groups["performer"].Value;
                        }
                        else if(matchSongwriter.Success)
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found SONGWRITER \"{1}\" at line {0}",
                                                      lineNumber, matchSongwriter.Groups["songwriter"].Value);
                            if(intrack) currenttrack.Songwriter = matchSongwriter.Groups["songwriter"].Value;
                            else discimage.Songwriter           = matchSongwriter.Groups["songwriter"].Value;
                        }
                        else if(matchComposer.Success)
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found COMPOSER \"{1}\" at line {0}", lineNumber,
                                                      matchComposer.Groups["composer"].Value);
                            if(intrack) currenttrack.Composer = matchComposer.Groups["composer"].Value;
                            else discimage.Composer           = matchComposer.Groups["composer"].Value;
                        }
                        else if(matchArranger.Success)
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found ARRANGER \"{1}\" at line {0}", lineNumber,
                                                      matchArranger.Groups["arranger"].Value);
                            if(intrack) currenttrack.Arranger = matchArranger.Groups["arranger"].Value;
                            else discimage.Arranger           = matchArranger.Groups["arranger"].Value;
                        }
                        else if(matchMessage.Success)
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found MESSAGE \"{1}\" at line {0}", lineNumber,
                                                      matchMessage.Groups["message"].Value);
                            if(intrack) currenttrack.Message = matchMessage.Groups["message"].Value;
                            else discimage.Message           = matchMessage.Groups["message"].Value;
                        }
                        else if(matchDiscId.Success)
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found DISC_ID \"{1}\" at line {0}", lineNumber,
                                                      matchDiscId.Groups["discid"].Value);
                            if(!intrack) discimage.DiskId = matchDiscId.Groups["discid"].Value;
                        }
                        else if(matchUpc.Success)
                        {
                            DicConsole.DebugWriteLine("CDRDAO plugin", "Found UPC_EAN \"{1}\" at line {0}", lineNumber,
                                                      matchUpc.Groups["catalog"].Value);
                            if(!intrack) discimage.Barcode = matchUpc.Groups["catalog"].Value;
                        }
                        // Ignored fields
                        else if(matchCdText.Success      || matchLanguage.Success || matchClosure.Success ||
                                matchLanguageMap.Success || matchLanguageMapping.Success) { }
                        else if(line == "") // Empty line, ignore it
                        { }
                    }

                    // TODO: Regex CD-TEXT SIZE_INFO
                    /*
                    else // Non-empty unknown field
                    {
                        throw new FeatureUnsupportedImageException(string.Format("Found unknown field defined at line {0}: \"{1}\"", line, _line));
                    }
                    */
                }

                if(currenttrack.Sequence != 0)
                {
                    if(currenttrack.Pregap != currenttrack.Sectors && !currenttrack.Indexes.ContainsKey(1))
                        currenttrack.Indexes.Add(1, currenttrack.StartSector + currenttrack.Pregap);

                    discimage.Tracks.Add(currenttrack);
                }

                discimage.Comment = commentBuilder.ToString();

                // DEBUG information
                DicConsole.DebugWriteLine("CDRDAO plugin",
                                          "Disc image parsing results");
                DicConsole.DebugWriteLine("CDRDAO plugin",                                "Disc CD-TEXT:");
                if(discimage.Arranger == null) DicConsole.DebugWriteLine("CDRDAO plugin", "\tArranger is not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tArranger: {0}",
                                              discimage.Arranger);
                if(discimage.Composer == null) DicConsole.DebugWriteLine("CDRDAO plugin", "\tComposer is not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tComposer: {0}",
                                              discimage.Composer);
                if(discimage.Performer == null) DicConsole.DebugWriteLine("CDRDAO plugin", "\tPerformer is not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPerformer: {0}",
                                              discimage.Performer);
                if(discimage.Songwriter == null) DicConsole.DebugWriteLine("CDRDAO plugin", "\tSongwriter is not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tSongwriter: {0}",
                                              discimage.Songwriter);
                if(discimage.Title == null) DicConsole.DebugWriteLine("CDRDAO plugin", "\tTitle is not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tTitle: {0}",
                                              discimage.Title);
                DicConsole.DebugWriteLine("CDRDAO plugin", "Disc information:");
                DicConsole.DebugWriteLine("CDRDAO plugin", "\tGuessed disk type: {0}",
                                          discimage.Disktype);
                if(discimage.Barcode == null) DicConsole.DebugWriteLine("CDRDAO plugin", "\tBarcode not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tBarcode: {0}",
                                              discimage.Barcode);
                if(discimage.DiskId == null) DicConsole.DebugWriteLine("CDRDAO plugin", "\tDisc ID not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tDisc ID: {0}",
                                              discimage.DiskId);
                if(discimage.Mcn == null) DicConsole.DebugWriteLine("CDRDAO plugin", "\tMCN not set.");
                else
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tMCN: {0}", discimage.Mcn);
                if(string.IsNullOrEmpty(discimage.Comment))
                    DicConsole.DebugWriteLine("CDRDAO plugin",  "\tComment not set.");
                else DicConsole.DebugWriteLine("CDRDAO plugin", "\tComment: \"{0}\"", discimage.Comment);

                DicConsole.DebugWriteLine("CDRDAO plugin", "Track information:");
                DicConsole.DebugWriteLine("CDRDAO plugin", "\tDisc contains {0} tracks", discimage.Tracks.Count);
                for(int i = 0; i < discimage.Tracks.Count; i++)
                {
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tTrack {0} information:",
                                              discimage.Tracks[i].Sequence);

                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\t{0} bytes per sector", discimage.Tracks[i].Bps);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tPregap: {0} sectors",  discimage.Tracks[i].Pregap);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tData: {0} sectors starting at sector {1}",
                                              discimage.Tracks[i].Sectors, discimage.Tracks[i].StartSector);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tPostgap: {0} sectors", discimage.Tracks[i].Postgap);

                    if(discimage.Tracks[i].Flag_4Ch)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tTrack is flagged as quadraphonic");
                    if(discimage.Tracks[i].FlagDcp)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tTrack allows digital copy");
                    if(discimage.Tracks[i].FlagPre)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tTrack has pre-emphasis applied");

                    DicConsole.DebugWriteLine("CDRDAO plugin",
                                              "\t\tTrack resides in file {0}, type defined as {1}, starting at byte {2}",
                                              discimage.Tracks[i].Trackfile.Datafilter.GetFilename(),
                                              discimage.Tracks[i].Trackfile.Filetype,
                                              discimage.Tracks[i].Trackfile.Offset);

                    DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tIndexes:");
                    foreach(KeyValuePair<int, ulong> kvp in discimage.Tracks[i].Indexes)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\t\tIndex {0} starts at sector {1}", kvp.Key,
                                                  kvp.Value);

                    if(discimage.Tracks[i].Isrc == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin",  "\t\tISRC is not set.");
                    else DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tISRC: {0}", discimage.Tracks[i].Isrc);

                    if(discimage.Tracks[i].Arranger == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin",  "\t\tArranger is not set.");
                    else DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tArranger: {0}", discimage.Tracks[i].Arranger);
                    if(discimage.Tracks[i].Composer == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin",  "\t\tComposer is not set.");
                    else DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tComposer: {0}", discimage.Tracks[i].Composer);
                    if(discimage.Tracks[i].Performer == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tPerformer is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tPerformer: {0}", discimage.Tracks[i].Performer);
                    if(discimage.Tracks[i].Songwriter == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tSongwriter is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tSongwriter: {0}",
                                                  discimage.Tracks[i].Songwriter);
                    if(discimage.Tracks[i].Title == null)
                        DicConsole.DebugWriteLine("CDRDAO plugin",  "\t\tTitle is not set.");
                    else DicConsole.DebugWriteLine("CDRDAO plugin", "\t\tTitle: {0}", discimage.Tracks[i].Title);
                }

                DicConsole.DebugWriteLine("CDRDAO plugin", "Building offset map");

                Partitions = new List<Partition>();
                offsetmap  = new Dictionary<uint, ulong>();

                ulong byteOffset        = 0;
                ulong partitionSequence = 0;
                for(int i = 0; i < discimage.Tracks.Count; i++)
                {
                    ulong index0Len = 0;

                    if(discimage.Tracks[i].Sequence == 1 && i != 0)
                        throw new ImageNotSupportedException("Unordered tracks");

                    // Index 01
                    Partition partition = new Partition
                    {
                        Description = $"Track {discimage.Tracks[i].Sequence}.",
                        Name        = discimage.Tracks[i].Title,
                        Start       = discimage.Tracks[i].StartSector,
                        Size        = (discimage.Tracks[i].Sectors - index0Len) * discimage.Tracks[i].Bps,
                        Length      = discimage.Tracks[i].Sectors  - index0Len,
                        Sequence    = partitionSequence,
                        Offset      = byteOffset,
                        Type        = discimage.Tracks[i].Tracktype
                    };

                    byteOffset += partition.Size;
                    partitionSequence++;

                    if(!offsetmap.ContainsKey(discimage.Tracks[i].Sequence))
                        offsetmap.Add(discimage.Tracks[i].Sequence, partition.Start);
                    else
                    {
                        offsetmap.TryGetValue(discimage.Tracks[i].Sequence, out ulong oldStart);

                        if(partition.Start < oldStart)
                        {
                            offsetmap.Remove(discimage.Tracks[i].Sequence);
                            offsetmap.Add(discimage.Tracks[i].Sequence, partition.Start);
                        }
                    }

                    Partitions.Add(partition);
                }

                // Print partition map
                DicConsole.DebugWriteLine("CDRDAO plugin", "printing partition map");
                foreach(Partition partition in Partitions)
                {
                    DicConsole.DebugWriteLine("CDRDAO plugin", "Partition sequence: {0}", partition.Sequence);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition name: {0}",   partition.Name);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition description: {0}",
                                              partition.Description);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition type: {0}",            partition.Type);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition starting sector: {0}", partition.Start);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition sectors: {0}",         partition.Length);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition starting offset: {0}", partition.Offset);
                    DicConsole.DebugWriteLine("CDRDAO plugin", "\tPartition size in bytes: {0}",   partition.Size);
                }

                foreach(CdrdaoTrack track in discimage.Tracks)
                {
                    imageInfo.ImageSize += track.Bps * track.Sectors;
                    imageInfo.Sectors   += track.Sectors;
                }

                if(discimage.Disktype != MediaType.CDG    && discimage.Disktype != MediaType.CDEG    &&
                   discimage.Disktype != MediaType.CDMIDI && discimage.Disktype != MediaType.CDROMXA &&
                   discimage.Disktype != MediaType.CDDA   && discimage.Disktype != MediaType.CDI     &&
                   discimage.Disktype != MediaType.CDPLUS) imageInfo.SectorSize = 2048; // Only data tracks
                else imageInfo.SectorSize                                       = 2352; // All others

                if(discimage.Mcn != null) imageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);

                imageInfo.Application = "CDRDAO";

                imageInfo.CreationTime         = imageFilter.GetCreationTime();
                imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

                imageInfo.Comments          = discimage.Comment;
                imageInfo.MediaSerialNumber = discimage.Mcn;
                imageInfo.MediaBarcode      = discimage.Barcode;
                imageInfo.MediaType         = discimage.Disktype;

                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                foreach(CdrdaoTrack track in discimage.Tracks)
                {
                    if(track.Subchannel)
                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                    switch(track.Tracktype)
                    {
                        case CDRDAO_TRACK_TYPE_AUDIO:
                        {
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);
                            break;
                        }
                        case CDRDAO_TRACK_TYPE_MODE2:
                        case CDRDAO_TRACK_TYPE_MODE2_MIX:
                        {
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                            break;
                        }
                        case CDRDAO_TRACK_TYPE_MODE2_RAW:
                        {
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                            break;
                        }
                        case CDRDAO_TRACK_TYPE_MODE1_RAW:
                        {
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                            break;
                        }
                    }
                }

                imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                DicConsole.VerboseWriteLine("CDRDAO image describes a disc of type {0}", imageInfo.MediaType);
                if(!string.IsNullOrEmpty(imageInfo.Comments))
                    DicConsole.VerboseWriteLine("CDRDAO comments: {0}", imageInfo.Comments);

                return true;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter);
                DicConsole.ErrorWriteLine("Exception: {0}",                              ex.Message);
                DicConsole.ErrorWriteLine("Stack trace: {0}",                            ex.StackTrace);
                return false;
            }
        }

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_MCN:
                {
                    if(discimage.Mcn != null) return Encoding.ASCII.GetBytes(discimage.Mcn);

                    throw new FeatureNotPresentImageException("Image does not contain MCN information.");
                }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        public byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public byte[] ReadSector(ulong sectorAddress, uint track)
        {
            return ReadSectors(sectorAddress, 1, track);
        }

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap
                                                     where sectorAddress >= kvp.Value
                                                     from cdrdaoTrack in discimage.Tracks
                                                     where cdrdaoTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrdaoTrack.Sectors
                                                     select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap
                                                     where sectorAddress >= kvp.Value
                                                     from cdrdaoTrack in discimage.Tracks
                                                     where cdrdaoTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrdaoTrack.Sectors
                                                     select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            CdrdaoTrack dicTrack = new CdrdaoTrack {Sequence = 0};

            foreach(CdrdaoTrack cdrdaoTrack in discimage.Tracks.Where(cdrdaoTrack => cdrdaoTrack.Sequence == track))
            {
                dicTrack = cdrdaoTrack;
                break;
            }

            if(dicTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > dicTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(dicTrack.Tracktype)
            {
                case CDRDAO_TRACK_TYPE_MODE1:
                case CDRDAO_TRACK_TYPE_MODE2_FORM1:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;
                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE2_FORM2:
                {
                    sectorOffset = 0;
                    sectorSize   = 2324;
                    sectorSkip   = 0;
                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE2:
                case CDRDAO_TRACK_TYPE_MODE2_MIX:
                {
                    sectorOffset = 0;
                    sectorSize   = 2336;
                    sectorSkip   = 0;
                    break;
                }
                case CDRDAO_TRACK_TYPE_AUDIO:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;
                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE1_RAW:
                {
                    sectorOffset = 16;
                    sectorSize   = 2048;
                    sectorSkip   = 288;
                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE2_RAW:
                {
                    sectorOffset = 16;
                    sectorSize   = 2336;
                    sectorSkip   = 0;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            if(dicTrack.Subchannel) sectorSkip += 96;

            byte[] buffer = new byte[sectorSize * length];

            imageStream     = dicTrack.Trackfile.Datafilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)dicTrack.Trackfile.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            CdrdaoTrack dicTrack = new CdrdaoTrack {Sequence = 0};

            foreach(CdrdaoTrack cdrdaoTrack in discimage.Tracks.Where(cdrdaoTrack => cdrdaoTrack.Sequence == track))
            {
                dicTrack = cdrdaoTrack;
                break;
            }

            if(dicTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > dicTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip = 0;

            if(!dicTrack.Subchannel && tag == SectorTagType.CdSectorSubchannel)
                throw new ArgumentException("No tags in image for requested track", nameof(tag));

            switch(tag)
            {
                case SectorTagType.CdSectorEcc:
                case SectorTagType.CdSectorEccP:
                case SectorTagType.CdSectorEccQ:
                case SectorTagType.CdSectorEdc:
                case SectorTagType.CdSectorHeader:
                case SectorTagType.CdSectorSubchannel:
                case SectorTagType.CdSectorSubHeader:
                case SectorTagType.CdSectorSync: break;
                case SectorTagType.CdTrackFlags:
                {
                    CdFlags flags = 0;

                    if(dicTrack.Tracktype != CDRDAO_TRACK_TYPE_AUDIO) flags |= CdFlags.DataTrack;

                    if(dicTrack.FlagDcp) flags |= CdFlags.CopyPermitted;

                    if(dicTrack.FlagPre) flags |= CdFlags.PreEmphasis;

                    if(dicTrack.Flag_4Ch) flags |= CdFlags.FourChannel;

                    return new[] {(byte)flags};
                }
                case SectorTagType.CdTrackIsrc:
                    if(dicTrack.Isrc == null) return null;

                    return Encoding.UTF8.GetBytes(dicTrack.Isrc);
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(dicTrack.Tracktype)
            {
                case CDRDAO_TRACK_TYPE_MODE1:
                case CDRDAO_TRACK_TYPE_MODE2_FORM1:
                    if(tag != SectorTagType.CdSectorSubchannel)
                        throw new ArgumentException("No tags in image for requested track", nameof(tag));

                    sectorOffset = 2048;
                    sectorSize   = 96;
                    break;
                case CDRDAO_TRACK_TYPE_MODE2_FORM2:
                case CDRDAO_TRACK_TYPE_MODE2_MIX:
                    if(tag != SectorTagType.CdSectorSubchannel)
                        throw new ArgumentException("No tags in image for requested track", nameof(tag));

                    sectorOffset = 2336;
                    sectorSize   = 96;
                    break;
                case CDRDAO_TRACK_TYPE_AUDIO:
                    if(tag != SectorTagType.CdSectorSubchannel)
                        throw new ArgumentException("No tags in image for requested track", nameof(tag));

                    sectorOffset = 2352;
                    sectorSize   = 96;
                    break;
                case CDRDAO_TRACK_TYPE_MODE1_RAW:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize   = 12;
                            sectorSkip   = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize   = 4;
                            sectorSkip   = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubchannel:
                        {
                            sectorOffset = 2352;
                            sectorSize   = 96;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize   = 276;
                            sectorSkip   = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize   = 172;
                            sectorSkip   = 104;
                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize   = 104;
                            sectorSkip   = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2064;
                            sectorSize   = 4;
                            sectorSkip   = 284;
                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE2_RAW: // Requires reading sector
                    if(tag != SectorTagType.CdSectorSubchannel)
                        throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");

                    sectorOffset = 2352;
                    sectorSize   = 96;
                    break;
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream     = dicTrack.Trackfile.Datafilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);
            br.BaseStream
              .Seek((long)dicTrack.Trackfile.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            return ReadSectorsLong(sectorAddress, 1, track);
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap
                                                     where sectorAddress >= kvp.Value
                                                     from cdrdaoTrack in discimage.Tracks
                                                     where cdrdaoTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrdaoTrack.Sectors
                                                     select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            CdrdaoTrack dicTrack = new CdrdaoTrack {Sequence = 0};

            foreach(CdrdaoTrack cdrdaoTrack in discimage.Tracks.Where(cdrdaoTrack => cdrdaoTrack.Sequence == track))
            {
                dicTrack = cdrdaoTrack;
                break;
            }

            if(dicTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > dicTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(dicTrack.Tracktype)
            {
                case CDRDAO_TRACK_TYPE_MODE1:
                case CDRDAO_TRACK_TYPE_MODE2_FORM1:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;
                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE2_FORM2:
                {
                    sectorOffset = 0;
                    sectorSize   = 2324;
                    sectorSkip   = 0;
                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE2:
                case CDRDAO_TRACK_TYPE_MODE2_MIX:
                {
                    sectorOffset = 0;
                    sectorSize   = 2336;
                    sectorSkip   = 0;
                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE1_RAW:
                case CDRDAO_TRACK_TYPE_MODE2_RAW:
                case CDRDAO_TRACK_TYPE_AUDIO:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            if(dicTrack.Subchannel) sectorSkip += 96;

            byte[] buffer = new byte[sectorSize * length];

            imageStream     = dicTrack.Trackfile.Datafilter.GetDataForkStream();
            BinaryReader br = new BinaryReader(imageStream);

            br.BaseStream
              .Seek((long)dicTrack.Trackfile.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(sectorOffset == 0 && sectorSkip == 0) buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);

                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public List<Track> GetSessionTracks(Session session)
        {
            return GetSessionTracks(session.SessionSequence);
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            if(session == 1) return Tracks;

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return CdChecksums.CheckCdSector(buffer);
        }

        public bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return CdChecksums.CheckCdSector(buffer);
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out                                   List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas   = new List<ulong>();
            unknownLbas   = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(unknownLbas.Count > 0) return null;

            return failingLbas.Count <= 0;
        }

        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out                                               List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas   = new List<ulong>();
            unknownLbas   = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);
                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);
                        break;
                }
            }

            if(unknownLbas.Count > 0) return null;

            return failingLbas.Count <= 0;
        }

        public bool? VerifyMediaImage()
        {
            return null;
        }

        // TODO: Decode CD-Text to text
        public IEnumerable<MediaTagType>  SupportedMediaTags  => new[] {MediaTagType.CD_MCN};
        public IEnumerable<SectorTagType> SupportedSectorTags =>
            new[]
            {
                SectorTagType.CdSectorEcc, SectorTagType.CdSectorEccP, SectorTagType.CdSectorEccQ,
                SectorTagType.CdSectorEdc, SectorTagType.CdSectorHeader, SectorTagType.CdSectorSubchannel,
                SectorTagType.CdSectorSubHeader, SectorTagType.CdSectorSync, SectorTagType.CdTrackFlags,
                SectorTagType.CdTrackIsrc
            };
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.CD, MediaType.CDDA, MediaType.CDEG, MediaType.CDG, MediaType.CDI, MediaType.CDMIDI,
                MediaType.CDMRW, MediaType.CDPLUS, MediaType.CDR, MediaType.CDROM, MediaType.CDROMXA, MediaType.CDRW,
                MediaType.CDV, MediaType.DDCD, MediaType.DDCDR, MediaType.DDCDRW, MediaType.JaguarCD, MediaType.MEGACD,
                MediaType.PD650, MediaType.PD650_WORM, MediaType.PS1CD, MediaType.PS2CD, MediaType.SuperCDROM2,
                MediaType.SVCD, MediaType.SATURNCD, MediaType.ThreeDO, MediaType.VCD, MediaType.VCDHD
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new[] {("separate", typeof(bool), "Write each track to a separate file.")};
        public IEnumerable<string> KnownExtensions => new[] {".toc"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(options != null)
            {
                if(options.TryGetValue("separate", out string tmpValue))
                {
                    if(!bool.TryParse(tmpValue, out separateTracksWriting))
                    {
                        ErrorMessage = "Invalid value for split option";
                        return false;
                    }

                    if(separateTracksWriting)
                    {
                        ErrorMessage = "Separate tracksnot yet implemented";
                        return false;
                    }
                }
            }
            else separateTracksWriting = false;

            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            // TODO: Separate tracks
            try
            {
                writingBaseName  = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                descriptorStream = new StreamWriter(path, false, Encoding.ASCII);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            discimage = new CdrdaoDisc {Disktype = mediaType, Tracks = new List<CdrdaoTrack>()};

            trackFlags = new Dictionary<byte, byte>();
            trackIsrcs = new Dictionary<byte, string>();

            IsWriting    = true;
            ErrorMessage = null;
            return true;
        }

        public bool WriteMediaTag(byte[] data, MediaTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            switch(tag)
            {
                case MediaTagType.CD_MCN:
                    discimage.Mcn = Encoding.ASCII.GetString(data);
                    return true;
                default:
                    ErrorMessage = $"Unsupported media tag {tag}";
                    return false;
            }
        }

        public bool WriteSector(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            FileStream trackStream = writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

            if(trackStream == null)
            {
                ErrorMessage = $"Can't found file containing {sectorAddress}";
                return false;
            }

            if(track.TrackBytesPerSector != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Invalid write mode for this sector";
                return false;
            }

            if(data.Length != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(track.TrackSubchannelType != TrackSubchannelType.None)
            {
                ErrorMessage = "Invalid subchannel mode for this sector";
                return false;
            }

            trackStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector),
                             SeekOrigin.Begin);
            trackStream.Write(data, 0, data.Length);

            return true;
        }

        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            FileStream trackStream = writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

            if(trackStream == null)
            {
                ErrorMessage = $"Can't found file containing {sectorAddress}";
                return false;
            }

            if(track.TrackBytesPerSector != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Invalid write mode for this sector";
                return false;
            }

            if(sectorAddress + length > track.TrackEndSector + 1)
            {
                ErrorMessage = "Can't cross tracks";
                return false;
            }

            if(data.Length % track.TrackRawBytesPerSector != 0)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            if(track.TrackSubchannelType != TrackSubchannelType.None)
            {
                ErrorMessage = "Invalid subchannel mode for this sector";
                return false;
            }

            trackStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector),
                             SeekOrigin.Begin);
            trackStream.Write(data, 0, data.Length);

            return true;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            FileStream trackStream = writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

            if(trackStream == null)
            {
                ErrorMessage = $"Can't found file containing {sectorAddress}";
                return false;
            }

            if(data.Length != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            uint subchannelSize = (uint)(track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0);

            trackStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + subchannelSize)),
                             SeekOrigin.Begin);
            trackStream.Write(data, 0, data.Length);

            return true;
        }

        public bool WriteSectorsLong(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            FileStream trackStream = writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

            if(trackStream == null)
            {
                ErrorMessage = $"Can't found file containing {sectorAddress}";
                return false;
            }

            if(sectorAddress + length > track.TrackEndSector + 1)
            {
                ErrorMessage = "Can't cross tracks";
                return false;
            }

            if(data.Length % track.TrackRawBytesPerSector != 0)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            uint subchannelSize = (uint)(track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0);

            for(uint i = 0; i < length; i++)
            {
                trackStream.Seek((long)(track.TrackFileOffset + (i + sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + subchannelSize)),
                                 SeekOrigin.Begin);
                trackStream.Write(data, (int)(i * track.TrackRawBytesPerSector), track.TrackRawBytesPerSector);
            }

            return true;
        }

        public bool SetTracks(List<Track> tracks)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            if(tracks == null || tracks.Count == 0)
            {
                ErrorMessage = "Invalid tracks sent";
                return false;
            }

            ulong currentOffset = 0;
            writingTracks       = new List<Track>();
            foreach(Track track in tracks.OrderBy(t => t.TrackSequence))
            {
                if(track.TrackSubchannelType == TrackSubchannelType.Q16 ||
                   track.TrackSubchannelType == TrackSubchannelType.Q16Interleaved)
                {
                    ErrorMessage =
                        $"Unsupported subchannel type {track.TrackSubchannelType} for track {track.TrackSequence}";
                    return false;
                }

                Track newTrack     = track;
                newTrack.TrackFile = separateTracksWriting
                                         ? writingBaseName + $"_track{track.TrackSequence:D2}.bin"
                                         : writingBaseName + ".bin";
                newTrack.TrackFileOffset = separateTracksWriting ? 0 : currentOffset;
                writingTracks.Add(newTrack);
                currentOffset += (ulong)newTrack.TrackRawBytesPerSector *
                                 (newTrack.TrackEndSector - newTrack.TrackStartSector + 1);

                if(track.TrackSubchannelType != TrackSubchannelType.None)
                    currentOffset += 96 * (newTrack.TrackEndSector - newTrack.TrackStartSector + 1);
            }

            writingStreams = new Dictionary<uint, FileStream>();
            if(separateTracksWriting)
                foreach(Track track in writingTracks)
                    writingStreams.Add(track.TrackSequence,
                                       new FileStream(track.TrackFile, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                                                      FileShare.None));
            else
            {
                FileStream jointstream = new FileStream(writingBaseName + ".bin", FileMode.OpenOrCreate,
                                                        FileAccess.ReadWrite, FileShare.None);
                foreach(Track track in writingTracks) writingStreams.Add(track.TrackSequence, jointstream);
            }

            return true;
        }

        public bool Close()
        {
            if(!IsWriting)
            {
                ErrorMessage = "Image is not opened for writing";
                return false;
            }

            if(separateTracksWriting)
                foreach(FileStream writingStream in writingStreams.Values)
                {
                    writingStream.Flush();
                    writingStream.Close();
                }
            else
            {
                writingStreams.First().Value.Flush();
                writingStreams.First().Value.Close();
            }

            bool data  = writingTracks.Count(t => t.TrackType != TrackType.Audio) > 0;
            bool mode2 = writingTracks.Count(t => t.TrackType == TrackType.CdMode2Form1 ||
                                                  t.TrackType == TrackType.CdMode2Form2 ||
                                                  t.TrackType == TrackType.CdMode2Formless) > 0;

            if(mode2) descriptorStream.WriteLine("CD_ROM_XA");
            else if(data)
                descriptorStream.WriteLine("CD_ROM");
            else
                descriptorStream.WriteLine("CD_DA");

            if(!string.IsNullOrWhiteSpace(discimage.Comment))
            {
                string[] commentLines = discimage.Comment.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
                foreach(string line in commentLines) descriptorStream.WriteLine("// {0}", line);
            }

            descriptorStream.WriteLine();

            if(!string.IsNullOrEmpty(discimage.Mcn)) descriptorStream.WriteLine("CATALOG {0}", discimage.Mcn);

            foreach(Track track in writingTracks)
            {
                descriptorStream.WriteLine();
                descriptorStream.WriteLine("// Track {0}", track.TrackSequence);

                string subchannelType;

                switch(track.TrackSubchannelType)
                {
                    case TrackSubchannelType.Packed:
                    case TrackSubchannelType.PackedInterleaved:
                        subchannelType = " RW";
                        break;
                    case TrackSubchannelType.Raw:
                    case TrackSubchannelType.RawInterleaved:
                        subchannelType = " RW_RAW";
                        break;
                    default:
                        subchannelType = "";
                        break;
                }

                descriptorStream.WriteLine("TRACK {0}{1}", GetTrackMode(track), subchannelType);

                trackFlags.TryGetValue((byte)track.TrackSequence, out byte flagsByte);

                CdFlags flags = (CdFlags)flagsByte;

                descriptorStream.WriteLine("{0}COPY", flags.HasFlag(CdFlags.CopyPermitted) ? "" : "NO ");

                if(track.TrackType == TrackType.Audio)
                {
                    descriptorStream.WriteLine("{0}PRE_EMPHASIS", flags.HasFlag(CdFlags.PreEmphasis) ? "" : "NO ");
                    descriptorStream.WriteLine("{0}_CHANNEL_AUDIO",
                                               flags.HasFlag(CdFlags.FourChannel) ? "FOUR" : "TWO");
                }

                if(trackIsrcs.TryGetValue((byte)track.TrackSequence, out string isrc))
                    descriptorStream.WriteLine("ISRC {0}", isrc);

                (byte minute, byte second, byte frame)
                    msf = LbaToMsf(track.TrackEndSector - track.TrackStartSector + 1);

                descriptorStream.WriteLine("DATAFILE \"{0}\" #{1} {2:D2}:{3:D2}:{4:D2} // length in bytes: {5}",
                                           Path.GetFileName(track.TrackFile), track.TrackFileOffset, msf.minute,
                                           msf.second, msf.frame,
                                           (track.TrackEndSector                - track.TrackStartSector + 1) *
                                           (ulong)(track.TrackRawBytesPerSector +
                                                   (track.TrackSubchannelType != TrackSubchannelType.None ? 96 : 0)));

                descriptorStream.WriteLine();
            }

            descriptorStream.Flush();
            descriptorStream.Close();

            IsWriting    = false;
            ErrorMessage = "";
            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
            discimage.Barcode = metadata.MediaBarcode;
            discimage.Comment = metadata.Comments;
            return true;
        }

        public bool SetGeometry(uint cylinders, uint heads, uint sectorsPerTrack)
        {
            ErrorMessage = "Unsupported feature";
            return false;
        }

        public bool WriteSectorTag(byte[] data, ulong sectorAddress, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            switch(tag)
            {
                case SectorTagType.CdTrackFlags:
                {
                    if(data.Length != 1)
                    {
                        ErrorMessage = "Incorrect data size for track flags";
                        return false;
                    }

                    trackFlags.Add((byte)track.TrackSequence, data[0]);

                    return true;
                }
                case SectorTagType.CdTrackIsrc:
                {
                    if(data != null) trackIsrcs.Add((byte)track.TrackSequence, Encoding.UTF8.GetString(data));
                    return true;
                }
                case SectorTagType.CdSectorSubchannel:
                {
                    if(track.TrackSubchannelType == 0)
                    {
                        ErrorMessage =
                            $"Trying to write subchannel to track {track.TrackSequence}, that does not have subchannel";
                        return false;
                    }

                    if(data.Length != 96)
                    {
                        ErrorMessage = "Incorrect data size for subchannel";
                        return false;
                    }

                    FileStream trackStream = writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

                    if(trackStream == null)
                    {
                        ErrorMessage = $"Can't found file containing {sectorAddress}";
                        return false;
                    }

                    trackStream
                       .Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96)) + track.TrackRawBytesPerSector,
                             SeekOrigin.Begin);
                    trackStream.Write(data, 0, data.Length);

                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";
                    return false;
            }
        }

        public bool WriteSectorsTag(byte[] data, ulong sectorAddress, uint length, SectorTagType tag)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                writingTracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                                    sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            switch(tag)
            {
                case SectorTagType.CdTrackFlags:
                case SectorTagType.CdTrackIsrc: return WriteSectorTag(data, sectorAddress, tag);
                case SectorTagType.CdSectorSubchannel:
                {
                    if(track.TrackSubchannelType == 0)
                    {
                        ErrorMessage =
                            $"Trying to write subchannel to track {track.TrackSequence}, that does not have subchannel";
                        return false;
                    }

                    if(data.Length % 96 != 0)
                    {
                        ErrorMessage = "Incorrect data size for subchannel";
                        return false;
                    }

                    FileStream trackStream = writingStreams.FirstOrDefault(kvp => kvp.Key == track.TrackSequence).Value;

                    if(trackStream == null)
                    {
                        ErrorMessage = $"Can't found file containing {sectorAddress}";
                        return false;
                    }

                    for(uint i = 0; i < length; i++)
                    {
                        trackStream
                           .Seek((long)(track.TrackFileOffset + (i + sectorAddress - track.TrackStartSector) * (ulong)(track.TrackRawBytesPerSector + 96)) + track.TrackRawBytesPerSector,
                                 SeekOrigin.Begin);
                        trackStream.Write(data, (int)(i * 96), 96);
                    }

                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";
                    return false;
            }
        }

        static ushort CdrdaoTrackTypeToBytesPerSector(string trackType)
        {
            switch(trackType)
            {
                case CDRDAO_TRACK_TYPE_MODE1:
                case CDRDAO_TRACK_TYPE_MODE2_FORM1: return 2048;
                case CDRDAO_TRACK_TYPE_MODE2_FORM2: return 2324;
                case CDRDAO_TRACK_TYPE_MODE2:
                case CDRDAO_TRACK_TYPE_MODE2_MIX: return 2336;
                case CDRDAO_TRACK_TYPE_AUDIO:
                case CDRDAO_TRACK_TYPE_MODE1_RAW:
                case CDRDAO_TRACK_TYPE_MODE2_RAW: return 2352;
                default:                          return 0;
            }
        }

        static ushort CdrdaoTrackTypeToCookedBytesPerSector(string trackType)
        {
            switch(trackType)
            {
                case CDRDAO_TRACK_TYPE_MODE1:
                case CDRDAO_TRACK_TYPE_MODE2_FORM1:
                case CDRDAO_TRACK_TYPE_MODE1_RAW:   return 2048;
                case CDRDAO_TRACK_TYPE_MODE2_FORM2: return 2324;
                case CDRDAO_TRACK_TYPE_MODE2:
                case CDRDAO_TRACK_TYPE_MODE2_MIX:
                case CDRDAO_TRACK_TYPE_MODE2_RAW: return 2336;
                case CDRDAO_TRACK_TYPE_AUDIO:     return 2352;
                default:                          return 0;
            }
        }

        static TrackType CdrdaoTrackTypeToTrackType(string trackType)
        {
            switch(trackType)
            {
                case CDRDAO_TRACK_TYPE_MODE1:
                case CDRDAO_TRACK_TYPE_MODE1_RAW:   return TrackType.CdMode1;
                case CDRDAO_TRACK_TYPE_MODE2_FORM1: return TrackType.CdMode2Form1;
                case CDRDAO_TRACK_TYPE_MODE2_FORM2: return TrackType.CdMode2Form2;
                case CDRDAO_TRACK_TYPE_MODE2:
                case CDRDAO_TRACK_TYPE_MODE2_MIX:
                case CDRDAO_TRACK_TYPE_MODE2_RAW: return TrackType.CdMode2Formless;
                case CDRDAO_TRACK_TYPE_AUDIO:     return TrackType.Audio;
                default:                          return TrackType.Data;
            }
        }

        static (byte minute, byte second, byte frame) LbaToMsf(ulong sector)
        {
            return ((byte)(sector / 75 / 60), (byte)(sector / 75 % 60), (byte)(sector % 75));
        }

        static string GetTrackMode(Track track)
        {
            switch(track.TrackType)
            {
                case TrackType.Audio when track.TrackRawBytesPerSector == 2352:
                    return CDRDAO_TRACK_TYPE_AUDIO;
                case TrackType.Data:
                    return CDRDAO_TRACK_TYPE_MODE1;
                case TrackType.CdMode1 when track.TrackRawBytesPerSector == 2352:
                    return CDRDAO_TRACK_TYPE_MODE1_RAW;
                case TrackType.CdMode2Formless when track.TrackRawBytesPerSector != 2352:
                    return CDRDAO_TRACK_TYPE_MODE2;
                case TrackType.CdMode2Form1 when track.TrackRawBytesPerSector != 2352:
                    return CDRDAO_TRACK_TYPE_MODE2_FORM1;
                case TrackType.CdMode2Form2 when track.TrackRawBytesPerSector != 2352:
                    return CDRDAO_TRACK_TYPE_MODE2_FORM2;
                case TrackType.CdMode2Formless when track.TrackRawBytesPerSector == 2352:
                case TrackType.CdMode2Form1 when track.TrackRawBytesPerSector    == 2352:
                case TrackType.CdMode2Form2 when track.TrackRawBytesPerSector    == 2352:
                    return CDRDAO_TRACK_TYPE_MODE2_RAW;
                default: return CDRDAO_TRACK_TYPE_MODE1;
            }
        }

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        struct CdrdaoTrackFile
        {
            /// <summary>Track #</summary>
            public uint Sequence;
            /// <summary>Filter of file containing track</summary>
            public IFilter Datafilter;
            /// <summary>Path of file containing track</summary>
            public string Datafile;
            /// <summary>Offset of track start in file</summary>
            public ulong Offset;
            /// <summary>Type of file</summary>
            public string Filetype;
        }

        #pragma warning disable 169
        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        struct CdrdaoTrack
        {
            /// <summary>Track #</summary>
            public uint Sequence;
            /// <summary>Track title (from CD-Text)</summary>
            public string Title;
            /// <summary>Track genre (from CD-Text)</summary>
            public string Genre;
            /// <summary>Track arranger (from CD-Text)</summary>
            public string Arranger;
            /// <summary>Track composer (from CD-Text)</summary>
            public string Composer;
            /// <summary>Track performer (from CD-Text)</summary>
            public string Performer;
            /// <summary>Track song writer (from CD-Text)</summary>
            public string Songwriter;
            /// <summary>Track ISRC</summary>
            public string Isrc;
            /// <summary>Disk provider's message (from CD-Text)</summary>
            public string Message;
            /// <summary>File struct for this track</summary>
            public CdrdaoTrackFile Trackfile;
            /// <summary>Indexes on this track</summary>
            public Dictionary<int, ulong> Indexes;
            /// <summary>Track pre-gap in sectors</summary>
            public ulong Pregap;
            /// <summary>Track post-gap in sectors</summary>
            public ulong Postgap;
            /// <summary>Digical Copy Permitted</summary>
            public bool FlagDcp;
            /// <summary>Track is quadraphonic</summary>
            public bool Flag_4Ch;
            /// <summary>Track has preemphasis</summary>
            public bool FlagPre;
            /// <summary>Bytes per sector</summary>
            public ushort Bps;
            /// <summary>Sectors in track</summary>
            public ulong Sectors;
            /// <summary>Starting sector in track</summary>
            public ulong StartSector;
            /// <summary>Track type</summary>
            public string Tracktype;
            public bool   Subchannel;
            public bool   Packedsubchannel;
        }

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        struct CdrdaoDisc
        {
            /// <summary>Disk title (from CD-Text)</summary>
            public string Title;
            /// <summary>Disk genre (from CD-Text)</summary>
            public string Genre;
            /// <summary>Disk arranger (from CD-Text)</summary>
            public string Arranger;
            /// <summary>Disk composer (from CD-Text)</summary>
            public string Composer;
            /// <summary>Disk performer (from CD-Text)</summary>
            public string Performer;
            /// <summary>Disk song writer (from CD-Text)</summary>
            public string Songwriter;
            /// <summary>Disk provider's message (from CD-Text)</summary>
            public string Message;
            /// <summary>Media catalog number</summary>
            public string Mcn;
            /// <summary>Disk type</summary>
            public MediaType Disktype;
            /// <summary>Disk type string</summary>
            public string Disktypestr;
            /// <summary>Disk CDDB ID</summary>
            public string DiskId;
            /// <summary>Disk UPC/EAN</summary>
            public string Barcode;
            /// <summary>Tracks</summary>
            public List<CdrdaoTrack> Tracks;
            /// <summary>Disk comment</summary>
            public string Comment;
        }
        #pragma warning restore 169
    }
}