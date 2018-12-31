// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads CDRWin cuesheets (cue/bin).
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Exceptions;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;

namespace DiscImageChef.DiscImages
{
    public partial class CdrWin
    {
        public bool Open(IFilter imageFilter)
        {
            if(imageFilter == null) return false;

            cdrwinFilter = imageFilter;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                cueStream = new StreamReader(imageFilter.GetDataForkStream());
                int  lineNumber     = 0;
                bool intrack        = false;
                byte currentsession = 1;

                // Initialize all RegExs
                Regex regexSession    = new Regex(REGEX_SESSION);
                Regex regexDiskType   = new Regex(REGEX_MEDIA_TYPE);
                Regex regexLeadOut    = new Regex(REGEX_LEAD_OUT);
                Regex regexLba        = new Regex(REGEX_LBA);
                Regex regexDiskId     = new Regex(REGEX_DISC_ID);
                Regex regexBarCode    = new Regex(REGEX_BARCODE);
                Regex regexComment    = new Regex(REGEX_COMMENT);
                Regex regexCdText     = new Regex(REGEX_CDTEXT);
                Regex regexMcn        = new Regex(REGEX_MCN);
                Regex regexTitle      = new Regex(REGEX_TITLE);
                Regex regexGenre      = new Regex(REGEX_GENRE);
                Regex regexArranger   = new Regex(REGEX_ARRANGER);
                Regex regexComposer   = new Regex(REGEX_COMPOSER);
                Regex regexPerformer  = new Regex(REGEX_PERFORMER);
                Regex regexSongWriter = new Regex(REGEX_SONGWRITER);
                Regex regexFile       = new Regex(REGEX_FILE);
                Regex regexTrack      = new Regex(REGEX_TRACK);
                Regex regexIsrc       = new Regex(REGEX_ISRC);
                Regex regexIndex      = new Regex(REGEX_INDEX);
                Regex regexPregap     = new Regex(REGEX_PREGAP);
                Regex regexPostgap    = new Regex(REGEX_POSTGAP);
                Regex regexFlags      = new Regex(REGEX_FLAGS);

                // Initialize all RegEx matches
                Match matchTrack;

                // Initialize disc
                discimage = new CdrWinDisc
                {
                    Sessions = new List<Session>(), Tracks = new List<CdrWinTrack>(), Comment = ""
                };

                CdrWinTrack     currenttrack            = new CdrWinTrack {Indexes = new Dictionary<int, ulong>()};
                CdrWinTrackFile currentfile             = new CdrWinTrackFile();
                ulong           currentfileoffsetsector = 0;

                int trackCount = 0;

                while(cueStream.Peek() >= 0)
                {
                    lineNumber++;
                    string line = cueStream.ReadLine();

                    matchTrack = regexTrack.Match(line);
                    if(!matchTrack.Success) continue;

                    uint trackSeq = uint.Parse(matchTrack.Groups[1].Value);
                    if(trackCount + 1 != trackSeq)
                        throw new
                            FeatureUnsupportedImageException($"Found TRACK {trackSeq} out of order in line {lineNumber}");

                    trackCount++;
                }

                if(trackCount == 0) throw new FeatureUnsupportedImageException("No tracks found");

                CdrWinTrack[] cuetracks = new CdrWinTrack[trackCount];

                lineNumber = 0;
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                cueStream = new StreamReader(imageFilter.GetDataForkStream());

                FiltersList filtersList = new FiltersList();

                while(cueStream.Peek() >= 0)
                {
                    lineNumber++;
                    string line = cueStream.ReadLine();

                    Match matchSession  = regexSession.Match(line);
                    Match matchDiskType = regexDiskType.Match(line);
                    Match matchComment  = regexComment.Match(line);
                    Match matchLba      = regexLba.Match(line);
                    Match matchLeadOut  = regexLeadOut.Match(line);

                    if(matchDiskType.Success && !intrack)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM ORIGINAL MEDIA TYPE at line {0}",
                                                  lineNumber);
                        discimage.Disktypestr = matchDiskType.Groups[1].Value;
                    }
                    else if(matchDiskType.Success && intrack)
                        throw new
                            FeatureUnsupportedImageException($"Found REM ORIGINAL MEDIA TYPE field after a track in line {lineNumber}");
                    else if(matchSession.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM SESSION at line {0}", lineNumber);
                        currentsession = byte.Parse(matchSession.Groups[1].Value);

                        // What happens between sessions
                    }
                    else if(matchLba.Success)
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM MSF at line {0}", lineNumber);
                    else if(matchLeadOut.Success)
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM LEAD-OUT at line {0}", lineNumber);
                    else if(matchComment.Success)
                    {
                        DicConsole.DebugWriteLine("CDRWin plugin", "Found REM at line {0}", lineNumber);
                        if(discimage.Comment == "") discimage.Comment = matchComment.Groups[1].Value; // First comment
                        else
                            discimage.Comment +=
                                Environment.NewLine + matchComment.Groups[1].Value; // Append new comments as new lines
                    }
                    else
                    {
                        matchTrack = regexTrack.Match(line);
                        Match matchTitle      = regexTitle.Match(line);
                        Match matchSongWriter = regexSongWriter.Match(line);
                        Match matchPregap     = regexPregap.Match(line);
                        Match matchPostgap    = regexPostgap.Match(line);
                        Match matchPerformer  = regexPerformer.Match(line);
                        Match matchMcn        = regexMcn.Match(line);
                        Match matchIsrc       = regexIsrc.Match(line);
                        Match matchIndex      = regexIndex.Match(line);
                        Match matchGenre      = regexGenre.Match(line);
                        Match matchFlags      = regexFlags.Match(line);
                        Match matchFile       = regexFile.Match(line);
                        Match matchDiskId     = regexDiskId.Match(line);
                        Match matchComposer   = regexComposer.Match(line);
                        Match matchCdText     = regexCdText.Match(line);
                        Match matchBarCode    = regexBarCode.Match(line);
                        Match matchArranger   = regexArranger.Match(line);

                        if(matchArranger.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found ARRANGER at line {0}", lineNumber);
                            if(intrack) currenttrack.Arranger = matchArranger.Groups[1].Value;
                            else discimage.Arranger           = matchArranger.Groups[1].Value;
                        }
                        else if(matchBarCode.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found UPC_EAN at line {0}", lineNumber);
                            if(!intrack) discimage.Barcode = matchBarCode.Groups[1].Value;
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found barcode field in incorrect place at line {lineNumber}");
                        }
                        else if(matchCdText.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found CDTEXTFILE at line {0}", lineNumber);
                            if(!intrack) discimage.Cdtextfile = matchCdText.Groups[1].Value;
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found CD-Text file field in incorrect place at line {lineNumber}");
                        }
                        else if(matchComposer.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found COMPOSER at line {0}", lineNumber);
                            if(intrack) currenttrack.Composer = matchComposer.Groups[1].Value;
                            else discimage.Composer           = matchComposer.Groups[1].Value;
                        }
                        else if(matchDiskId.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found DISC_ID at line {0}", lineNumber);
                            if(!intrack) discimage.DiskId = matchDiskId.Groups[1].Value;
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found CDDB ID field in incorrect place at line {lineNumber}");
                        }
                        else if(matchFile.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found FILE at line {0}", lineNumber);

                            if(currenttrack.Sequence != 0)
                            {
                                currentfile.Sequence   = currenttrack.Sequence;
                                currenttrack.Trackfile = currentfile;
                                currenttrack.Sectors =
                                    ((ulong)currentfile.Datafilter.GetLength() - currentfile.Offset) /
                                    CdrWinTrackTypeToBytesPerSector(currenttrack.Tracktype);
                                cuetracks[currenttrack.Sequence - 1] = currenttrack;
                                intrack                              = false;
                                currenttrack                         = new CdrWinTrack();
                                currentfile                          = new CdrWinTrackFile();
                                filtersList                          = new FiltersList();
                            }

                            //currentfile = new CDRWinTrackFile();
                            string datafile = matchFile.Groups[1].Value;
                            currentfile.Filetype = matchFile.Groups[2].Value;

                            // Check if file path is quoted
                            if(datafile[0] == '"' && datafile[datafile.Length    - 1] == '"')
                                datafile = datafile.Substring(1, datafile.Length - 2); // Unquote it

                            currentfile.Datafilter = filtersList.GetFilter(datafile);

                            // Check if file exists
                            if(currentfile.Datafilter == null)
                                if(datafile[0] == '/' || datafile[0] == '/' && datafile[1] == '.') // UNIX absolute path
                                {
                                    Regex unixpath      = new Regex("^(.+)/([^/]+)$");
                                    Match unixpathmatch = unixpath.Match(datafile);

                                    if(unixpathmatch.Success)
                                    {
                                        currentfile.Datafilter = filtersList.GetFilter(unixpathmatch.Groups[1].Value);

                                        if(currentfile.Datafilter == null)
                                        {
                                            string path = imageFilter.GetParentFolder() + Path.PathSeparator +
                                                          unixpathmatch.Groups[1].Value;
                                            currentfile.Datafilter = filtersList.GetFilter(path);

                                            if(currentfile.Datafilter == null)
                                                throw new
                                                    FeatureUnsupportedImageException($"File \"{matchFile.Groups[1].Value}\" not found.");
                                        }
                                    }
                                    else
                                        throw new
                                            FeatureUnsupportedImageException($"File \"{matchFile.Groups[1].Value}\" not found.");
                                }
                                else if(datafile[1] == ':'  && datafile[2] == '\\' ||
                                        datafile[0] == '\\' && datafile[1] == '\\' ||
                                        datafile[0] == '.'  && datafile[1] == '\\') // Windows absolute path
                                {
                                    Regex winpath =
                                        new
                                            Regex("^(?:[a-zA-Z]\\:(\\\\|\\/)|file\\:\\/\\/|\\\\\\\\|\\.(\\/|\\\\))([^\\\\\\/\\:\\*\\?\\<\\>\\\"\\|]+(\\\\|\\/){0,1})+$");
                                    Match winpathmatch = winpath.Match(datafile);
                                    if(winpathmatch.Success)
                                    {
                                        currentfile.Datafilter = filtersList.GetFilter(winpathmatch.Groups[1].Value);

                                        if(currentfile.Datafilter == null)
                                        {
                                            string path = imageFilter.GetParentFolder() + Path.PathSeparator +
                                                          winpathmatch.Groups[1].Value;
                                            currentfile.Datafilter = filtersList.GetFilter(path);

                                            if(currentfile.Datafilter == null)
                                                throw new
                                                    FeatureUnsupportedImageException($"File \"{matchFile.Groups[1].Value}\" not found.");
                                        }
                                    }
                                    else
                                        throw new
                                            FeatureUnsupportedImageException($"File \"{matchFile.Groups[1].Value}\" not found.");
                                }
                                else
                                {
                                    string path = imageFilter.GetParentFolder() + Path.PathSeparator + datafile;
                                    currentfile.Datafilter = filtersList.GetFilter(path);

                                    if(currentfile.Datafilter == null)
                                        throw new
                                            FeatureUnsupportedImageException($"File \"{matchFile.Groups[1].Value}\" not found.");
                                }

                            // File does exist, process it
                            DicConsole.DebugWriteLine("CDRWin plugin", "File \"{0}\" found",
                                                      currentfile.Datafilter.GetFilename());

                            switch(currentfile.Filetype)
                            {
                                case CDRWIN_DISK_TYPE_LITTLE_ENDIAN: break;
                                case CDRWIN_DISK_TYPE_BIG_ENDIAN:
                                case CDRWIN_DISK_TYPE_AIFF:
                                case CDRWIN_DISK_TYPE_RIFF:
                                case CDRWIN_DISK_TYPE_MP3:
                                    throw new
                                        FeatureSupportedButNotImplementedImageException($"Unsupported file type {currentfile.Filetype}");
                                default:
                                    throw new
                                        FeatureUnsupportedImageException($"Unknown file type {currentfile.Filetype}");
                            }

                            currentfile.Offset   = 0;
                            currentfile.Sequence = 0;
                        }
                        else if(matchFlags.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found FLAGS at line {0}", lineNumber);
                            if(!intrack)
                                throw new
                                    FeatureUnsupportedImageException($"Found FLAGS field in incorrect place at line {lineNumber}");

                            currenttrack.FlagDcp  |= matchFlags.Groups["dcp"].Value  == "DCP";
                            currenttrack.Flag4ch  |= matchFlags.Groups["quad"].Value == "4CH";
                            currenttrack.FlagPre  |= matchFlags.Groups["pre"].Value  == "PRE";
                            currenttrack.FlagScms |= matchFlags.Groups["scms"].Value == "SCMS";
                        }
                        else if(matchGenre.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found GENRE at line {0}", lineNumber);
                            if(intrack) currenttrack.Genre = matchGenre.Groups[1].Value;
                            else discimage.Genre           = matchGenre.Groups[1].Value;
                        }
                        else if(matchIndex.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found INDEX at line {0}", lineNumber);
                            if(!intrack)
                                throw new FeatureUnsupportedImageException($"Found INDEX before a track {lineNumber}");

                            int   index  = int.Parse(matchIndex.Groups[1].Value);
                            ulong offset = CdrWinMsftoLba(matchIndex.Groups[2].Value);

                            if(index != 0 && index != 1 && currenttrack.Indexes.Count == 0)
                                throw new
                                    FeatureUnsupportedImageException($"Found INDEX {index} before INDEX 00 or INDEX 01");

                            if(index == 0 || index == 1 && !currenttrack.Indexes.ContainsKey(0))
                                if((int)(currenttrack.Sequence - 2) >= 0 && offset > 1)
                                {
                                    cuetracks[currenttrack.Sequence - 2].Sectors = offset - currentfileoffsetsector;
                                    currentfile.Offset +=
                                        cuetracks[currenttrack.Sequence - 2].Sectors *
                                        cuetracks[currenttrack.Sequence - 2].Bps;
                                    DicConsole.DebugWriteLine("CDRWin plugin", "Sets currentfile.offset to {0}",
                                                              currentfile.Offset);
                                    DicConsole.DebugWriteLine("CDRWin plugin",
                                                              "cuetracks[currenttrack.sequence-2].sectors = {0}",
                                                              cuetracks[currenttrack.Sequence - 2].Sectors);
                                    DicConsole.DebugWriteLine("CDRWin plugin",
                                                              "cuetracks[currenttrack.sequence-2].bps = {0}",
                                                              cuetracks[currenttrack.Sequence - 2].Bps);
                                }

                            if((index == 0 || index == 1 && !currenttrack.Indexes.ContainsKey(0)) &&
                               currenttrack.Sequence == 1)
                            {
                                DicConsole.DebugWriteLine("CDRWin plugin", "Sets currentfile.offset to {0}",
                                                          offset * currenttrack.Bps);
                                currentfile.Offset = offset * currenttrack.Bps;
                            }

                            currentfileoffsetsector = offset;
                            currenttrack.Indexes.Add(index, offset);
                        }
                        else if(matchIsrc.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found ISRC at line {0}", lineNumber);
                            if(!intrack)
                                throw new FeatureUnsupportedImageException($"Found ISRC before a track {lineNumber}");

                            currenttrack.Isrc = matchIsrc.Groups[1].Value;
                        }
                        else if(matchMcn.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found CATALOG at line {0}", lineNumber);
                            if(!intrack) discimage.Mcn = matchMcn.Groups[1].Value;
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found CATALOG field in incorrect place at line {lineNumber}");
                        }
                        else if(matchPerformer.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found PERFORMER at line {0}", lineNumber);
                            if(intrack) currenttrack.Performer = matchPerformer.Groups[1].Value;
                            else discimage.Performer           = matchPerformer.Groups[1].Value;
                        }
                        else if(matchPostgap.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found POSTGAP at line {0}", lineNumber);
                            if(intrack) currenttrack.Postgap = CdrWinMsftoLba(matchPostgap.Groups[1].Value);
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found POSTGAP field before a track at line {lineNumber}");
                        }
                        else if(matchPregap.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found PREGAP at line {0}", lineNumber);
                            if(intrack) currenttrack.Pregap = CdrWinMsftoLba(matchPregap.Groups[1].Value);
                            else
                                throw new
                                    FeatureUnsupportedImageException($"Found PREGAP field before a track at line {lineNumber}");
                        }
                        else if(matchSongWriter.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found SONGWRITER at line {0}", lineNumber);
                            if(intrack) currenttrack.Songwriter = matchSongWriter.Groups[1].Value;
                            else discimage.Songwriter           = matchSongWriter.Groups[1].Value;
                        }
                        else if(matchTitle.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found TITLE at line {0}", lineNumber);
                            if(intrack) currenttrack.Title = matchTitle.Groups[1].Value;
                            else discimage.Title           = matchTitle.Groups[1].Value;
                        }
                        else if(matchTrack.Success)
                        {
                            DicConsole.DebugWriteLine("CDRWin plugin", "Found TRACK at line {0}", lineNumber);
                            if(currentfile.Datafilter == null)
                                throw new
                                    FeatureUnsupportedImageException($"Found TRACK field before a file is defined at line {lineNumber}");

                            if(intrack)
                            {
                                if(currenttrack.Indexes.ContainsKey(0) && currenttrack.Pregap == 0)
                                    currenttrack.Indexes.TryGetValue(0, out currenttrack.Pregap);
                                currentfile.Sequence                 = currenttrack.Sequence;
                                currenttrack.Trackfile               = currentfile;
                                cuetracks[currenttrack.Sequence - 1] = currenttrack;
                            }

                            currenttrack = new CdrWinTrack
                            {
                                Indexes  = new Dictionary<int, ulong>(),
                                Sequence = uint.Parse(matchTrack.Groups[1].Value)
                            };
                            DicConsole.DebugWriteLine("CDRWin plugin", "Setting currenttrack.sequence to {0}",
                                                      currenttrack.Sequence);
                            currentfile.Sequence   = currenttrack.Sequence;
                            currenttrack.Bps       = CdrWinTrackTypeToBytesPerSector(matchTrack.Groups[2].Value);
                            currenttrack.Tracktype = matchTrack.Groups[2].Value;
                            currenttrack.Session   = currentsession;
                            intrack                = true;
                        }
                        else if(line == "") // Empty line, ignore it
                        { }
                        else // Non-empty unknown field
                            throw new
                                FeatureUnsupportedImageException($"Found unknown field defined at line {lineNumber}: \"{line}\"");
                    }
                }

                if(currenttrack.Sequence != 0)
                {
                    currentfile.Sequence   = currenttrack.Sequence;
                    currenttrack.Trackfile = currentfile;
                    currenttrack.Sectors = ((ulong)currentfile.Datafilter.GetLength() - currentfile.Offset) /
                                           CdrWinTrackTypeToBytesPerSector(currenttrack.Tracktype);
                    cuetracks[currenttrack.Sequence - 1] = currenttrack;
                }

                Session[] sessions = new Session[currentsession];
                for(int s = 1; s <= sessions.Length; s++)
                {
                    sessions[s - 1].SessionSequence = 1;

                    if(s > 1) sessions[s - 1].StartSector = sessions[s - 2].EndSector + 1;
                    else sessions[s      - 1].StartSector = 0;

                    ulong sessionSectors   = 0;
                    int   lastSessionTrack = 0;

                    for(int i = 0; i < cuetracks.Length; i++)
                        if(cuetracks[i].Session == s)
                        {
                            sessionSectors += cuetracks[i].Sectors;
                            if(i > lastSessionTrack) lastSessionTrack = i;
                        }

                    sessions[s - 1].EndTrack  = cuetracks[lastSessionTrack].Sequence;
                    sessions[s - 1].EndSector = sessionSectors - 1;
                }

                for(int s = 1; s <= sessions.Length; s++) discimage.Sessions.Add(sessions[s - 1]);

                for(int t = 1; t <= cuetracks.Length; t++) discimage.Tracks.Add(cuetracks[t - 1]);

                discimage.Disktype = CdrWinIsoBusterDiscTypeToMediaType(discimage.Disktypestr);

                if(discimage.Disktype == MediaType.Unknown || discimage.Disktype == MediaType.CD)
                {
                    bool data       = false;
                    bool cdg        = false;
                    bool cdi        = false;
                    bool mode2      = false;
                    bool firstaudio = false;
                    bool firstdata  = false;
                    bool audio      = false;

                    for(int i = 0; i < discimage.Tracks.Count; i++)
                    {
                        // First track is audio
                        firstaudio |= i == 0 && discimage.Tracks[i].Tracktype == CDRWIN_TRACK_TYPE_AUDIO;

                        // First track is data
                        firstdata |= i == 0 && discimage.Tracks[i].Tracktype != CDRWIN_TRACK_TYPE_AUDIO;

                        // Any non first track is data
                        data |= i != 0 && discimage.Tracks[i].Tracktype != CDRWIN_TRACK_TYPE_AUDIO;

                        // Any non first track is audio
                        audio |= i != 0 && discimage.Tracks[i].Tracktype == CDRWIN_TRACK_TYPE_AUDIO;

                        switch(discimage.Tracks[i].Tracktype)
                        {
                            case CDRWIN_TRACK_TYPE_CDG:
                                cdg = true;
                                break;
                            case CDRWIN_TRACK_TYPE_CDI:
                            case CDRWIN_TRACK_TYPE_CDI_RAW:
                                cdi = true;
                                break;
                            case CDRWIN_TRACK_TYPE_MODE2_FORM1:
                            case CDRWIN_TRACK_TYPE_MODE2_FORM2:
                            case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                            case CDRWIN_TRACK_TYPE_MODE2_RAW:
                                mode2 = true;
                                break;
                        }
                    }

                    if(!data && !firstdata) discimage.Disktype = MediaType.CDDA;
                    else if(cdg) discimage.Disktype            = MediaType.CDG;
                    else if(cdi) discimage.Disktype            = MediaType.CDI;
                    else if(firstaudio && data && discimage.Sessions.Count > 1 && mode2)
                        discimage.Disktype                                  = MediaType.CDPLUS;
                    else if(firstdata && audio || mode2) discimage.Disktype = MediaType.CDROMXA;
                    else if(!audio) discimage.Disktype                      = MediaType.CDROM;
                    else discimage.Disktype                                 = MediaType.CD;
                }

                // DEBUG information
                DicConsole.DebugWriteLine("CDRWin plugin", "Disc image parsing results");
                DicConsole.DebugWriteLine("CDRWin plugin", "Disc CD-TEXT:");
                if(discimage.Arranger == null) DicConsole.DebugWriteLine("CDRWin plugin", "\tArranger is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tArranger: {0}",
                                              discimage.Arranger);
                if(discimage.Composer == null) DicConsole.DebugWriteLine("CDRWin plugin", "\tComposer is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tComposer: {0}",
                                              discimage.Composer);
                if(discimage.Genre == null) DicConsole.DebugWriteLine("CDRWin plugin", "\tGenre is not set.");
                else DicConsole.DebugWriteLine("CDRWin plugin",                        "\tGenre: {0}", discimage.Genre);
                if(discimage.Performer == null) DicConsole.DebugWriteLine("CDRWin plugin", "\tPerformer is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPerformer: {0}",
                                              discimage.Performer);
                if(discimage.Songwriter == null) DicConsole.DebugWriteLine("CDRWin plugin", "\tSongwriter is not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tSongwriter: {0}",
                                              discimage.Songwriter);
                if(discimage.Title == null) DicConsole.DebugWriteLine("CDRWin plugin", "\tTitle is not set.");
                else DicConsole.DebugWriteLine("CDRWin plugin",                        "\tTitle: {0}", discimage.Title);
                if(discimage.Cdtextfile == null)
                    DicConsole.DebugWriteLine("CDRWin plugin",  "\tCD-TEXT binary file not set.");
                else DicConsole.DebugWriteLine("CDRWin plugin", "\tCD-TEXT binary file: {0}", discimage.Cdtextfile);
                DicConsole.DebugWriteLine("CDRWin plugin", "Disc information:");
                if(discimage.Disktypestr == null)
                    DicConsole.DebugWriteLine("CDRWin plugin",  "\tISOBuster disc type not set.");
                else DicConsole.DebugWriteLine("CDRWin plugin", "\tISOBuster disc type: {0}", discimage.Disktypestr);
                DicConsole.DebugWriteLine("CDRWin plugin", "\tGuessed disk type: {0}", discimage.Disktype);
                if(discimage.Barcode == null) DicConsole.DebugWriteLine("CDRWin plugin", "\tBarcode not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tBarcode: {0}",
                                              discimage.Barcode);
                if(discimage.DiskId == null) DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc ID not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc ID: {0}",
                                              discimage.DiskId);
                if(discimage.Mcn == null) DicConsole.DebugWriteLine("CDRWin plugin", "\tMCN not set.");
                else DicConsole.DebugWriteLine("CDRWin plugin",                      "\tMCN: {0}", discimage.Mcn);
                if(discimage.Comment == null) DicConsole.DebugWriteLine("CDRWin plugin", "\tComment not set.");
                else
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tComment: \"{0}\"",
                                              discimage.Comment);
                DicConsole.DebugWriteLine("CDRWin plugin", "Session information:");
                DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc contains {0} sessions", discimage.Sessions.Count);
                for(int i = 0; i < discimage.Sessions.Count; i++)
                {
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tSession {0} information:", i + 1);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tStarting track: {0}",
                                              discimage.Sessions[i].StartTrack);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tStarting sector: {0}",
                                              discimage.Sessions[i].StartSector);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tEnding track: {0}", discimage.Sessions[i].EndTrack);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tEnding sector: {0}",
                                              discimage.Sessions[i].EndSector);
                }

                DicConsole.DebugWriteLine("CDRWin plugin", "Track information:");
                DicConsole.DebugWriteLine("CDRWin plugin", "\tDisc contains {0} tracks", discimage.Tracks.Count);
                for(int i = 0; i < discimage.Tracks.Count; i++)
                {
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tTrack {0} information:",
                                              discimage.Tracks[i].Sequence);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\t{0} bytes per sector", discimage.Tracks[i].Bps);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPregap: {0} sectors",  discimage.Tracks[i].Pregap);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tData: {0} sectors",    discimage.Tracks[i].Sectors);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPostgap: {0} sectors", discimage.Tracks[i].Postgap);

                    if(discimage.Tracks[i].Flag4ch)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack is flagged as quadraphonic");
                    if(discimage.Tracks[i].FlagDcp)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack allows digital copy");
                    if(discimage.Tracks[i].FlagPre)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack has pre-emphasis applied");
                    if(discimage.Tracks[i].FlagScms) DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTrack has SCMS");

                    DicConsole.DebugWriteLine("CDRWin plugin",
                                              "\t\tTrack resides in file {0}, type defined as {1}, starting at byte {2}",
                                              discimage.Tracks[i].Trackfile.Datafilter.GetFilename(),
                                              discimage.Tracks[i].Trackfile.Filetype,
                                              discimage.Tracks[i].Trackfile.Offset);

                    DicConsole.DebugWriteLine("CDRWin plugin", "\t\tIndexes:");
                    foreach(KeyValuePair<int, ulong> kvp in discimage.Tracks[i].Indexes)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\t\tIndex {0} starts at sector {1}", kvp.Key,
                                                  kvp.Value);

                    if(discimage.Tracks[i].Isrc == null)
                        DicConsole.DebugWriteLine("CDRWin plugin",  "\t\tISRC is not set.");
                    else DicConsole.DebugWriteLine("CDRWin plugin", "\t\tISRC: {0}", discimage.Tracks[i].Isrc);

                    if(discimage.Tracks[i].Arranger == null)
                        DicConsole.DebugWriteLine("CDRWin plugin",  "\t\tArranger is not set.");
                    else DicConsole.DebugWriteLine("CDRWin plugin", "\t\tArranger: {0}", discimage.Tracks[i].Arranger);
                    if(discimage.Tracks[i].Composer == null)
                        DicConsole.DebugWriteLine("CDRWin plugin",  "\t\tComposer is not set.");
                    else DicConsole.DebugWriteLine("CDRWin plugin", "\t\tComposer: {0}", discimage.Tracks[i].Composer);
                    if(discimage.Tracks[i].Genre == null)
                        DicConsole.DebugWriteLine("CDRWin plugin",  "\t\tGenre is not set.");
                    else DicConsole.DebugWriteLine("CDRWin plugin", "\t\tGenre: {0}", discimage.Tracks[i].Genre);
                    if(discimage.Tracks[i].Performer == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPerformer is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tPerformer: {0}", discimage.Tracks[i].Performer);
                    if(discimage.Tracks[i].Songwriter == null)
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tSongwriter is not set.");
                    else
                        DicConsole.DebugWriteLine("CDRWin plugin", "\t\tSongwriter: {0}",
                                                  discimage.Tracks[i].Songwriter);
                    if(discimage.Tracks[i].Title == null)
                        DicConsole.DebugWriteLine("CDRWin plugin",  "\t\tTitle is not set.");
                    else DicConsole.DebugWriteLine("CDRWin plugin", "\t\tTitle: {0}", discimage.Tracks[i].Title);
                }

                DicConsole.DebugWriteLine("CDRWin plugin", "Building offset map");

                Partitions = new List<Partition>();

                ulong byteOffset        = 0;
                ulong sectorOffset      = 0;
                ulong partitionSequence = 0;
                ulong indexZeroOffset   = 0;
                ulong indexOneOffset    = 0;
                bool  indexZero         = false;

                offsetmap = new Dictionary<uint, ulong>();

                for(int i = 0; i < discimage.Tracks.Count; i++)
                {
                    ulong index0Len = 0;

                    if(discimage.Tracks[i].Sequence == 1 && i != 0)
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

                    indexZero |= discimage.Tracks[i].Indexes.TryGetValue(0, out indexZeroOffset);

                    if(!discimage.Tracks[i].Indexes.TryGetValue(1, out indexOneOffset))
                        throw new ImageNotSupportedException($"Track {discimage.Tracks[i].Sequence} lacks index 01");

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
                    partition.Description = $"Track {discimage.Tracks[i].Sequence}.";
                    partition.Name        = discimage.Tracks[i].Title;
                    partition.Start       = sectorOffset;
                    partition.Size        = (discimage.Tracks[i].Sectors - index0Len) * discimage.Tracks[i].Bps;
                    partition.Length      = discimage.Tracks[i].Sectors - index0Len;
                    partition.Sequence    = partitionSequence;
                    partition.Offset      = byteOffset;
                    partition.Type        = discimage.Tracks[i].Tracktype;

                    sectorOffset += partition.Length;
                    byteOffset   += partition.Size;
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

                // Print offset map
                DicConsole.DebugWriteLine("CDRWin plugin", "printing partition map");
                foreach(Partition partition in Partitions)
                {
                    DicConsole.DebugWriteLine("CDRWin plugin", "Partition sequence: {0}", partition.Sequence);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition name: {0}",   partition.Name);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition description: {0}",
                                              partition.Description);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition type: {0}",            partition.Type);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition starting sector: {0}", partition.Start);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition sectors: {0}",         partition.Length);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition starting offset: {0}", partition.Offset);
                    DicConsole.DebugWriteLine("CDRWin plugin", "\tPartition size in bytes: {0}",   partition.Size);
                }

                foreach(CdrWinTrack track in discimage.Tracks) imageInfo.ImageSize += track.Bps * track.Sectors;
                foreach(CdrWinTrack track in discimage.Tracks) imageInfo.Sectors   += track.Sectors;

                if(discimage.Disktype != MediaType.CDROMXA && discimage.Disktype != MediaType.CDDA   &&
                   discimage.Disktype != MediaType.CDI     && discimage.Disktype != MediaType.CDPLUS &&
                   discimage.Disktype != MediaType.CDG     && discimage.Disktype != MediaType.CDEG   &&
                   discimage.Disktype != MediaType.CDMIDI) imageInfo.SectorSize = 2048; // Only data tracks
                else imageInfo.SectorSize                                       = 2352; // All others

                if(discimage.Mcn        != null) imageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);
                if(discimage.Cdtextfile != null) imageInfo.ReadableMediaTags.Add(MediaTagType.CD_TEXT);

                // Detect ISOBuster extensions
                if(discimage.Disktypestr    != null || discimage.Comment.ToLower().Contains("isobuster") ||
                   discimage.Sessions.Count > 1) imageInfo.Application = "ISOBuster";
                else imageInfo.Application                             = "CDRWin";

                imageInfo.CreationTime         = imageFilter.GetCreationTime();
                imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

                imageInfo.Comments          = discimage.Comment;
                imageInfo.MediaSerialNumber = discimage.Mcn;
                imageInfo.MediaBarcode      = discimage.Barcode;
                imageInfo.MediaType         = discimage.Disktype;

                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                foreach(CdrWinTrack track in discimage.Tracks)
                    switch(track.Tracktype)
                    {
                        case CDRWIN_TRACK_TYPE_AUDIO:
                        {
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);
                            break;
                        }
                        case CDRWIN_TRACK_TYPE_CDG:
                        {
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                            break;
                        }
                        case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                        case CDRWIN_TRACK_TYPE_CDI:
                        {
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);
                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                            break;
                        }
                        case CDRWIN_TRACK_TYPE_MODE2_RAW:
                        case CDRWIN_TRACK_TYPE_CDI_RAW:
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
                        case CDRWIN_TRACK_TYPE_MODE1_RAW:
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

                imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                DicConsole.VerboseWriteLine("CDRWIN image describes a disc of type {0}", imageInfo.MediaType);
                if(!string.IsNullOrEmpty(imageInfo.Comments))
                    DicConsole.VerboseWriteLine("CDRWIN comments: {0}", imageInfo.Comments);

                return true;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter.GetFilename());
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
                case MediaTagType.CD_TEXT:
                {
                    if(discimage.Cdtextfile != null)
                        // TODO: Check that binary text file exists, open it, read it, send it to caller.
                        throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");

                    throw new FeatureNotPresentImageException("Image does not contain CD-TEXT information.");
                }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        public byte[] ReadSector(ulong sectorAddress, uint track) => ReadSectors(sectorAddress, 1, track);

        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag) =>
            ReadSectorsTag(sectorAddress, 1, track, tag);

        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap
                                                     where sectorAddress >= kvp.Value
                                                     from cdrwinTrack in discimage.Tracks
                                                     where cdrwinTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrwinTrack.Sectors
                                                     select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap
                                                     where sectorAddress >= kvp.Value
                                                     from cdrwinTrack in discimage.Tracks
                                                     where cdrwinTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrwinTrack.Sectors
                                                     select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            CdrWinTrack dicTrack = new CdrWinTrack {Sequence = 0};

            foreach(CdrWinTrack cdrwinTrack in discimage.Tracks.Where(cdrwinTrack => cdrwinTrack.Sequence == track))
            {
                dicTrack = cdrwinTrack;
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
                case CDRWIN_TRACK_TYPE_MODE1:
                case CDRWIN_TRACK_TYPE_MODE2_FORM1:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;
                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_FORM2:
                {
                    sectorOffset = 0;
                    sectorSize   = 2324;
                    sectorSkip   = 0;
                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                case CDRWIN_TRACK_TYPE_CDI:
                {
                    sectorOffset = 0;
                    sectorSize   = 2336;
                    sectorSkip   = 0;
                    break;
                }
                case CDRWIN_TRACK_TYPE_AUDIO:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;
                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE1_RAW:
                {
                    sectorOffset = 16;
                    sectorSize   = 2048;
                    sectorSkip   = 288;
                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_RAW:
                {
                    sectorOffset = 16;
                    sectorSize   = 2336;
                    sectorSkip   = 0;
                    break;
                }
                case CDRWIN_TRACK_TYPE_CDI_RAW:
                {
                    sectorOffset = 16;
                    sectorSize   = 2336;
                    sectorSkip   = 0;
                    break;
                }
                case CDRWIN_TRACK_TYPE_CDG:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 96;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = dicTrack.Trackfile.Datafilter.GetDataForkStream();
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
            CdrWinTrack dicTrack = new CdrWinTrack {Sequence = 0};

            foreach(CdrWinTrack cdrwinTrack in discimage.Tracks.Where(cdrwinTrack => cdrwinTrack.Sequence == track))
            {
                dicTrack = cdrwinTrack;
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

                    if(dicTrack.Tracktype != CDRWIN_TRACK_TYPE_AUDIO && dicTrack.Tracktype != CDRWIN_TRACK_TYPE_CDG)
                        flags |= CdFlags.DataTrack;

                    if(dicTrack.FlagDcp) flags |= CdFlags.CopyPermitted;

                    if(dicTrack.FlagPre) flags |= CdFlags.PreEmphasis;

                    if(dicTrack.Flag4ch) flags |= CdFlags.FourChannel;

                    return new[] {(byte)flags};
                }
                case SectorTagType.CdTrackIsrc:
                    if(dicTrack.Isrc == null) return null;

                    return Encoding.UTF8.GetBytes(dicTrack.Isrc);
                case SectorTagType.CdTrackText:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(dicTrack.Tracktype)
            {
                case CDRWIN_TRACK_TYPE_MODE1:
                case CDRWIN_TRACK_TYPE_MODE2_FORM1:
                case CDRWIN_TRACK_TYPE_MODE2_FORM2:
                    throw new ArgumentException("No tags in image for requested track", nameof(tag));
                case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                case CDRWIN_TRACK_TYPE_CDI:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
                        case SectorTagType.CdSectorSubchannel:
                        case SectorTagType.CdSectorEcc:
                        case SectorTagType.CdSectorEccP:
                        case SectorTagType.CdSectorEccQ:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sectorOffset = 0;
                            sectorSize   = 8;
                            sectorSkip   = 2328;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2332;
                            sectorSize   = 4;
                            sectorSkip   = 0;
                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                }
                case CDRWIN_TRACK_TYPE_AUDIO:
                    throw new ArgumentException("There are no tags on audio tracks", nameof(tag));
                case CDRWIN_TRACK_TYPE_MODE1_RAW:
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
                case CDRWIN_TRACK_TYPE_MODE2_RAW: // Requires reading sector
                case CDRWIN_TRACK_TYPE_CDI_RAW:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not yet implemented");
                case CDRWIN_TRACK_TYPE_CDG:
                {
                    if(tag != SectorTagType.CdSectorSubchannel)
                        throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));

                    sectorOffset = 2352;
                    sectorSize   = 96;
                    sectorSkip   = 0;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = dicTrack.Trackfile.Datafilter.GetDataForkStream();
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

        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        public byte[] ReadSectorLong(ulong sectorAddress, uint track) => ReadSectorsLong(sectorAddress, 1, track);

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap
                                                     where sectorAddress >= kvp.Value
                                                     from cdrwinTrack in discimage.Tracks
                                                     where cdrwinTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrwinTrack.Sectors
                                                     select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            CdrWinTrack dicTrack = new CdrWinTrack {Sequence = 0};

            foreach(CdrWinTrack cdrwinTrack in discimage.Tracks.Where(cdrwinTrack => cdrwinTrack.Sequence == track))
            {
                dicTrack = cdrwinTrack;
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
                case CDRWIN_TRACK_TYPE_MODE1:
                case CDRWIN_TRACK_TYPE_MODE2_FORM1:
                {
                    sectorOffset = 0;
                    sectorSize   = 2048;
                    sectorSkip   = 0;
                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_FORM2:
                {
                    sectorOffset = 0;
                    sectorSize   = 2324;
                    sectorSkip   = 0;
                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                case CDRWIN_TRACK_TYPE_CDI:
                {
                    sectorOffset = 0;
                    sectorSize   = 2336;
                    sectorSkip   = 0;
                    break;
                }
                case CDRWIN_TRACK_TYPE_MODE1_RAW:
                case CDRWIN_TRACK_TYPE_MODE2_RAW:
                case CDRWIN_TRACK_TYPE_CDI_RAW:
                case CDRWIN_TRACK_TYPE_AUDIO:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;
                    break;
                }
                case CDRWIN_TRACK_TYPE_CDG:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 96;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            imageStream = dicTrack.Trackfile.Datafilter.GetDataForkStream();
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
            if(discimage.Sessions.Contains(session)) return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            List<Track> tracks = new List<Track>();

            foreach(CdrWinTrack cdrTrack in discimage.Tracks)
                if(cdrTrack.Session == session)
                {
                    Track dicTrack = new Track
                    {
                        Indexes                = cdrTrack.Indexes,
                        TrackDescription       = cdrTrack.Title,
                        TrackPregap            = cdrTrack.Pregap,
                        TrackSession           = cdrTrack.Session,
                        TrackSequence          = cdrTrack.Sequence,
                        TrackType              = CdrWinTrackTypeToTrackType(cdrTrack.Tracktype),
                        TrackFile              = cdrTrack.Trackfile.Datafilter.GetFilename(),
                        TrackFilter            = cdrTrack.Trackfile.Datafilter,
                        TrackFileOffset        = cdrTrack.Trackfile.Offset,
                        TrackFileType          = cdrTrack.Trackfile.Filetype,
                        TrackRawBytesPerSector = cdrTrack.Bps,
                        TrackBytesPerSector    = CdrWinTrackTypeToCookedBytesPerSector(cdrTrack.Tracktype)
                    };

                    if(!cdrTrack.Indexes.TryGetValue(0, out dicTrack.TrackStartSector))
                        cdrTrack.Indexes.TryGetValue(1, out dicTrack.TrackStartSector);
                    dicTrack.TrackEndSector = dicTrack.TrackStartSector + cdrTrack.Sectors - 1;
                    if(cdrTrack.Tracktype == CDRWIN_TRACK_TYPE_CDG)
                    {
                        dicTrack.TrackSubchannelFilter = cdrTrack.Trackfile.Datafilter;
                        dicTrack.TrackSubchannelFile   = cdrTrack.Trackfile.Datafilter.GetFilename();
                        dicTrack.TrackSubchannelOffset = cdrTrack.Trackfile.Offset;
                        dicTrack.TrackSubchannelType   = TrackSubchannelType.RawInterleaved;
                    }
                    else dicTrack.TrackSubchannelType = TrackSubchannelType.None;

                    tracks.Add(dicTrack);
                }

            return tracks;
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

        public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

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

        public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

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
    }
}