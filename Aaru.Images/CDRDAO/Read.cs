// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Read.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads cdrdao cuesheets (toc/bin).
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Exceptions;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders.CD;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.DiscImages
{
    public sealed partial class Cdrdao
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            if(imageFilter == null)
                return false;

            _cdrdaoFilter = imageFilter;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                _tocStream = new StreamReader(imageFilter.GetDataForkStream());
                bool inTrack = false;

                // Initialize all RegExs
                var regexComment         = new Regex(REGEX_COMMENT);
                var regexDiskType        = new Regex(REGEX_DISCTYPE);
                var regexMcn             = new Regex(REGEX_MCN);
                var regexTrack           = new Regex(REGEX_TRACK);
                var regexCopy            = new Regex(REGEX_COPY);
                var regexEmphasis        = new Regex(REGEX_EMPHASIS);
                var regexStereo          = new Regex(REGEX_STEREO);
                var regexIsrc            = new Regex(REGEX_ISRC);
                var regexIndex           = new Regex(REGEX_INDEX);
                var regexPregap          = new Regex(REGEX_PREGAP);
                var regexZeroPregap      = new Regex(REGEX_ZERO_PREGAP);
                var regexZeroData        = new Regex(REGEX_ZERO_DATA);
                var regexZeroAudio       = new Regex(REGEX_ZERO_AUDIO);
                var regexAudioFile       = new Regex(REGEX_FILE_AUDIO);
                var regexFile            = new Regex(REGEX_FILE_DATA);
                var regexTitle           = new Regex(REGEX_TITLE);
                var regexPerformer       = new Regex(REGEX_PERFORMER);
                var regexSongwriter      = new Regex(REGEX_SONGWRITER);
                var regexComposer        = new Regex(REGEX_COMPOSER);
                var regexArranger        = new Regex(REGEX_ARRANGER);
                var regexMessage         = new Regex(REGEX_MESSAGE);
                var regexDiscId          = new Regex(REGEX_DISC_ID);
                var regexUpc             = new Regex(REGEX_UPC);
                var regexCdText          = new Regex(REGEX_CD_TEXT);
                var regexLanguage        = new Regex(REGEX_LANGUAGE);
                var regexClosure         = new Regex(REGEX_CLOSURE);
                var regexLanguageMap     = new Regex(REGEX_LANGUAGE_MAP);
                var regexLanguageMapping = new Regex(REGEX_LANGUAGE_MAPPING);

                // Initialize all RegEx matches
                Match matchComment;
                Match matchDiskType;

                // Initialize disc
                _discimage = new CdrdaoDisc
                {
                    Tracks  = new List<CdrdaoTrack>(),
                    Comment = ""
                };

                var  currentTrack       = new CdrdaoTrack();
                uint currentTrackNumber = 0;
                currentTrack.Indexes = new Dictionary<int, ulong>();
                currentTrack.Pregap  = 0;
                ulong currentSector  = 0;
                int   nextIndex      = 2;
                var   commentBuilder = new StringBuilder();

                _tocStream = new StreamReader(_cdrdaoFilter.GetDataForkStream());
                string line;
                int    lineNumber = 0;

                while(_tocStream.Peek() >= 0)
                {
                    lineNumber++;
                    line = _tocStream.ReadLine();

                    matchDiskType = regexDiskType.Match(line ?? throw new InvalidOperationException());
                    matchComment  = regexComment.Match(line);

                    // Skip comments at start of file
                    if(matchComment.Success)
                        continue;

                    if(!matchDiskType.Success)
                    {
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Not a CDRDAO TOC or TOC type not in line {0}.",
                                                   lineNumber);

                        return false;
                    }

                    break;
                }

                _tocStream = new StreamReader(_cdrdaoFilter.GetDataForkStream());
                lineNumber = 0;

                _tocStream.BaseStream.Position = 0;

                while(_tocStream.Peek() >= 0)
                {
                    lineNumber++;
                    line = _tocStream.ReadLine();

                    matchComment  = regexComment.Match(line ?? throw new InvalidOperationException());
                    matchDiskType = regexDiskType.Match(line);
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

                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found comment \"{1}\" at line {0}", lineNumber,
                                                   matchComment.Groups["comment"].Value.Trim());

                        commentBuilder.AppendLine(matchComment.Groups["comment"].Value.Trim());
                    }
                    else if(matchDiskType.Success)
                    {
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found {1} at line {0}", lineNumber,
                                                   matchDiskType.Groups["type"].Value);

                        _discimage.Disktypestr = matchDiskType.Groups["type"].Value;

                        switch(matchDiskType.Groups["type"].Value)
                        {
                            case "CD_DA":
                                _discimage.Disktype = MediaType.CDDA;

                                break;
                            case "CD_ROM":
                                _discimage.Disktype = MediaType.CDROM;

                                break;
                            case "CD_ROM_XA":
                                _discimage.Disktype = MediaType.CDROMXA;

                                break;
                            case "CD_I":
                                _discimage.Disktype = MediaType.CDI;

                                break;
                            default:
                                _discimage.Disktype = MediaType.CD;

                                break;
                        }
                    }
                    else if(matchMcn.Success)
                    {
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found CATALOG \"{1}\" at line {0}", lineNumber,
                                                   matchMcn.Groups["catalog"].Value);

                        _discimage.Mcn = matchMcn.Groups["catalog"].Value;
                    }
                    else if(matchTrack.Success)
                    {
                        if(matchTrack.Groups["subchan"].Value == "")
                            AaruConsole.DebugWriteLine("CDRDAO plugin",
                                                       "Found TRACK type \"{1}\" with no subchannel at line {0}",
                                                       lineNumber, matchTrack.Groups["type"].Value);
                        else
                            AaruConsole.DebugWriteLine("CDRDAO plugin",
                                                       "Found TRACK type \"{1}\" subchannel {2} at line {0}",
                                                       lineNumber, matchTrack.Groups["type"].Value,
                                                       matchTrack.Groups["subchan"].Value);

                        if(inTrack)
                        {
                            currentSector += currentTrack.Sectors;

                            if(currentTrack.Pregap != currentTrack.Sectors &&
                               !currentTrack.Indexes.ContainsKey(1))
                                currentTrack.Indexes.Add(1, currentTrack.StartSector + currentTrack.Pregap);

                            _discimage.Tracks.Add(currentTrack);

                            currentTrack = new CdrdaoTrack
                            {
                                Indexes = new Dictionary<int, ulong>(),
                                Pregap  = 0
                            };

                            nextIndex = 2;
                        }

                        currentTrackNumber++;
                        inTrack = true;

                        switch(matchTrack.Groups["type"].Value)
                        {
                            case "AUDIO":
                            case "MODE1_RAW":
                            case "MODE2_RAW":
                                currentTrack.Bps = 2352;

                                break;
                            case "MODE1":
                            case "MODE2_FORM1":
                                currentTrack.Bps = 2048;

                                break;
                            case "MODE2_FORM2":
                                currentTrack.Bps = 2324;

                                break;
                            case "MODE2":
                            case "MODE2_FORM_MIX":
                                currentTrack.Bps = 2336;

                                break;
                            default:
                                throw new
                                    NotSupportedException($"Track mode {matchTrack.Groups["type"].Value} is unsupported");
                        }

                        switch(matchTrack.Groups["subchan"].Value)
                        {
                            case "": break;
                            case "RW":
                                currentTrack.Packedsubchannel = true;
                                goto case "RW_RAW";
                            case "RW_RAW":
                                currentTrack.Subchannel = true;

                                break;
                            default:
                                throw new
                                    NotSupportedException($"Track subchannel mode {matchTrack.Groups["subchan"].Value} is unsupported");
                        }

                        currentTrack.Tracktype = matchTrack.Groups["type"].Value;

                        currentTrack.Sequence    = currentTrackNumber;
                        currentTrack.StartSector = currentSector;
                    }
                    else if(matchCopy.Success)
                    {
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found {1} COPY at line {0}", lineNumber,
                                                   matchCopy.Groups["no"].Value);

                        currentTrack.FlagDcp |= inTrack && matchCopy.Groups["no"].Value == "";
                    }
                    else if(matchEmphasis.Success)
                    {
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found {1} PRE_EMPHASIS at line {0}", lineNumber,
                                                   matchEmphasis.Groups["no"].Value);

                        currentTrack.FlagPre |= inTrack && matchEmphasis.Groups["no"].Value == "";
                    }
                    else if(matchStereo.Success)
                    {
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found {1}_CHANNEL_AUDIO at line {0}", lineNumber,
                                                   matchStereo.Groups["num"].Value);

                        currentTrack.Flag4Ch |= inTrack && matchStereo.Groups["num"].Value == "FOUR";
                    }
                    else if(matchIsrc.Success)
                    {
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found ISRC \"{1}\" at line {0}", lineNumber,
                                                   matchIsrc.Groups["isrc"].Value);

                        if(inTrack)
                            currentTrack.Isrc = matchIsrc.Groups["isrc"].Value;
                    }
                    else if(matchIndex.Success)
                    {
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found INDEX \"{1}\" at line {0}", lineNumber,
                                                   matchIndex.Groups["address"].Value);

                        string[] lengthString = matchFile.Groups["length"].Value.Split(':');

                        ulong nextIndexPos = (ulong.Parse(lengthString[0]) * 60 * 75) +
                                             (ulong.Parse(lengthString[1]) * 75)      + ulong.Parse(lengthString[2]);

                        currentTrack.Indexes.Add(nextIndex,
                                                 nextIndexPos + currentTrack.Pregap + currentTrack.StartSector);
                    }
                    else if(matchPregap.Success)
                    {
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found START \"{1}\" at line {0}", lineNumber,
                                                   matchPregap.Groups["address"].Value);

                        currentTrack.Indexes.Add(0, currentTrack.StartSector);

                        if(matchPregap.Groups["address"].Value != "")
                        {
                            string[] lengthString = matchPregap.Groups["address"].Value.Split(':');

                            currentTrack.Pregap = (ulong.Parse(lengthString[0]) * 60 * 75) +
                                                  (ulong.Parse(lengthString[1]) * 75) + ulong.Parse(lengthString[2]);
                        }
                        else
                            currentTrack.Pregap = currentTrack.Sectors;
                    }
                    else if(matchZeroPregap.Success)
                    {
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found PREGAP \"{1}\" at line {0}", lineNumber,
                                                   matchZeroPregap.Groups["length"].Value);

                        currentTrack.Indexes.Add(0, currentTrack.StartSector);
                        string[] lengthString = matchZeroPregap.Groups["length"].Value.Split(':');

                        currentTrack.Pregap = (ulong.Parse(lengthString[0]) * 60 * 75) +
                                              (ulong.Parse(lengthString[1]) * 75)      + ulong.Parse(lengthString[2]);
                    }
                    else if(matchZeroData.Success)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found ZERO \"{1}\" at line {0}", lineNumber,
                                                   matchZeroData.Groups["length"].Value);
                    else if(matchZeroAudio.Success)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "Found SILENCE \"{1}\" at line {0}", lineNumber,
                                                   matchZeroAudio.Groups["length"].Value);
                    else
                    {
                        FiltersList filtersList;

                        if(matchAudioFile.Success)
                        {
                            AaruConsole.DebugWriteLine("CDRDAO plugin", "Found AUDIOFILE \"{1}\" at line {0}",
                                                       lineNumber, matchAudioFile.Groups["filename"].Value);

                            filtersList = new FiltersList();

                            currentTrack.Trackfile = new CdrdaoTrackFile
                            {
                                Datafilter =
                                    filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                       matchAudioFile.Groups["filename"].Value)),
                                Datafile = matchAudioFile.Groups["filename"].Value,
                                Offset = matchAudioFile.Groups["base_offset"].Value != ""
                                             ? ulong.Parse(matchAudioFile.Groups["base_offset"].Value) : 0,
                                Filetype = "BINARY",
                                Sequence = currentTrackNumber
                            };

                            ulong startSectors = 0;

                            if(matchAudioFile.Groups["start"].Value != "")
                            {
                                string[] startString = matchAudioFile.Groups["start"].Value.Split(':');

                                startSectors = (ulong.Parse(startString[0]) * 60 * 75) +
                                               (ulong.Parse(startString[1]) * 75)      + ulong.Parse(startString[2]);
                            }

                            currentTrack.Trackfile.Offset += startSectors * currentTrack.Bps;

                            if(matchAudioFile.Groups["length"].Value != "")
                            {
                                string[] lengthString = matchAudioFile.Groups["length"].Value.Split(':');

                                currentTrack.Sectors = (ulong.Parse(lengthString[0]) * 60 * 75) +
                                                       (ulong.Parse(lengthString[1]) * 75)      +
                                                       ulong.Parse(lengthString[2]);
                            }
                            else
                                currentTrack.Sectors =
                                    ((ulong)currentTrack.Trackfile.Datafilter.GetDataForkLength() -
                                     currentTrack.Trackfile.Offset) / currentTrack.Bps;
                        }
                        else if(matchFile.Success)
                        {
                            AaruConsole.DebugWriteLine("CDRDAO plugin", "Found DATAFILE \"{1}\" at line {0}",
                                                       lineNumber, matchFile.Groups["filename"].Value);

                            filtersList = new FiltersList();

                            currentTrack.Trackfile = new CdrdaoTrackFile
                            {
                                Datafilter =
                                    filtersList.GetFilter(Path.Combine(imageFilter.GetParentFolder(),
                                                                       matchFile.Groups["filename"].Value)),
                                Datafile = matchAudioFile.Groups["filename"].Value,
                                Offset = matchFile.Groups["base_offset"].Value != ""
                                             ? ulong.Parse(matchFile.Groups["base_offset"].Value) : 0,
                                Filetype = "BINARY",
                                Sequence = currentTrackNumber
                            };

                            if(matchFile.Groups["length"].Value != "")
                            {
                                string[] lengthString = matchFile.Groups["length"].Value.Split(':');

                                currentTrack.Sectors = (ulong.Parse(lengthString[0]) * 60 * 75) +
                                                       (ulong.Parse(lengthString[1]) * 75)      +
                                                       ulong.Parse(lengthString[2]);
                            }
                            else
                                currentTrack.Sectors =
                                    ((ulong)currentTrack.Trackfile.Datafilter.GetDataForkLength() -
                                     currentTrack.Trackfile.Offset) / currentTrack.Bps;
                        }
                        else if(matchTitle.Success)
                        {
                            AaruConsole.DebugWriteLine("CDRDAO plugin", "Found TITLE \"{1}\" at line {0}", lineNumber,
                                                       matchTitle.Groups["title"].Value);

                            if(inTrack)
                                currentTrack.Title = matchTitle.Groups["title"].Value;
                            else
                                _discimage.Title = matchTitle.Groups["title"].Value;
                        }
                        else if(matchPerformer.Success)
                        {
                            AaruConsole.DebugWriteLine("CDRDAO plugin", "Found PERFORMER \"{1}\" at line {0}",
                                                       lineNumber, matchPerformer.Groups["performer"].Value);

                            if(inTrack)
                                currentTrack.Performer = matchPerformer.Groups["performer"].Value;
                            else
                                _discimage.Performer = matchPerformer.Groups["performer"].Value;
                        }
                        else if(matchSongwriter.Success)
                        {
                            AaruConsole.DebugWriteLine("CDRDAO plugin", "Found SONGWRITER \"{1}\" at line {0}",
                                                       lineNumber, matchSongwriter.Groups["songwriter"].Value);

                            if(inTrack)
                                currentTrack.Songwriter = matchSongwriter.Groups["songwriter"].Value;
                            else
                                _discimage.Songwriter = matchSongwriter.Groups["songwriter"].Value;
                        }
                        else if(matchComposer.Success)
                        {
                            AaruConsole.DebugWriteLine("CDRDAO plugin", "Found COMPOSER \"{1}\" at line {0}",
                                                       lineNumber, matchComposer.Groups["composer"].Value);

                            if(inTrack)
                                currentTrack.Composer = matchComposer.Groups["composer"].Value;
                            else
                                _discimage.Composer = matchComposer.Groups["composer"].Value;
                        }
                        else if(matchArranger.Success)
                        {
                            AaruConsole.DebugWriteLine("CDRDAO plugin", "Found ARRANGER \"{1}\" at line {0}",
                                                       lineNumber, matchArranger.Groups["arranger"].Value);

                            if(inTrack)
                                currentTrack.Arranger = matchArranger.Groups["arranger"].Value;
                            else
                                _discimage.Arranger = matchArranger.Groups["arranger"].Value;
                        }
                        else if(matchMessage.Success)
                        {
                            AaruConsole.DebugWriteLine("CDRDAO plugin", "Found MESSAGE \"{1}\" at line {0}", lineNumber,
                                                       matchMessage.Groups["message"].Value);

                            if(inTrack)
                                currentTrack.Message = matchMessage.Groups["message"].Value;
                            else
                                _discimage.Message = matchMessage.Groups["message"].Value;
                        }
                        else if(matchDiscId.Success)
                        {
                            AaruConsole.DebugWriteLine("CDRDAO plugin", "Found DISC_ID \"{1}\" at line {0}", lineNumber,
                                                       matchDiscId.Groups["discid"].Value);

                            if(!inTrack)
                                _discimage.DiskId = matchDiscId.Groups["discid"].Value;
                        }
                        else if(matchUpc.Success)
                        {
                            AaruConsole.DebugWriteLine("CDRDAO plugin", "Found UPC_EAN \"{1}\" at line {0}", lineNumber,
                                                       matchUpc.Groups["catalog"].Value);

                            if(!inTrack)
                                _discimage.Barcode = matchUpc.Groups["catalog"].Value;
                        }

                        // Ignored fields
                        else if(matchCdText.Success      ||
                                matchLanguage.Success    ||
                                matchClosure.Success     ||
                                matchLanguageMap.Success ||
                                matchLanguageMapping.Success) {}
                        else if(line == "") // Empty line, ignore it
                        {}
                    }

                    // TODO: Regex CD-TEXT SIZE_INFO
                    /*
                    else // Non-empty unknown field
                    {
                        throw new FeatureUnsupportedImageException(string.Format("Found unknown field defined at line {0}: \"{1}\"", line, _line));
                    }
                    */
                }

                if(currentTrack.Sequence != 0)
                {
                    if(currentTrack.Pregap != currentTrack.Sectors &&
                       !currentTrack.Indexes.ContainsKey(1))
                        currentTrack.Indexes.Add(1, currentTrack.StartSector + currentTrack.Pregap);

                    _discimage.Tracks.Add(currentTrack);
                }

                _discimage.Comment = commentBuilder.ToString();

                // DEBUG information
                AaruConsole.DebugWriteLine("CDRDAO plugin", "Disc image parsing results");
                AaruConsole.DebugWriteLine("CDRDAO plugin", "Disc CD-TEXT:");

                if(_discimage.Arranger == null)
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tArranger is not set.");
                else
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tArranger: {0}", _discimage.Arranger);

                if(_discimage.Composer == null)
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tComposer is not set.");
                else
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tComposer: {0}", _discimage.Composer);

                if(_discimage.Performer == null)
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tPerformer is not set.");
                else
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tPerformer: {0}", _discimage.Performer);

                if(_discimage.Songwriter == null)
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tSongwriter is not set.");
                else
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tSongwriter: {0}", _discimage.Songwriter);

                if(_discimage.Title == null)
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tTitle is not set.");
                else
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tTitle: {0}", _discimage.Title);

                AaruConsole.DebugWriteLine("CDRDAO plugin", "Disc information:");
                AaruConsole.DebugWriteLine("CDRDAO plugin", "\tGuessed disk type: {0}", _discimage.Disktype);

                if(_discimage.Barcode == null)
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tBarcode not set.");
                else
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tBarcode: {0}", _discimage.Barcode);

                if(_discimage.DiskId == null)
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tDisc ID not set.");
                else
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tDisc ID: {0}", _discimage.DiskId);

                if(_discimage.Mcn == null)
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tMCN not set.");
                else
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tMCN: {0}", _discimage.Mcn);

                if(string.IsNullOrEmpty(_discimage.Comment))
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tComment not set.");
                else
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tComment: \"{0}\"", _discimage.Comment);

                AaruConsole.DebugWriteLine("CDRDAO plugin", "Track information:");
                AaruConsole.DebugWriteLine("CDRDAO plugin", "\tDisc contains {0} tracks", _discimage.Tracks.Count);

                for(int i = 0; i < _discimage.Tracks.Count; i++)
                {
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tTrack {0} information:",
                                               _discimage.Tracks[i].Sequence);

                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\t{0} bytes per sector", _discimage.Tracks[i].Bps);
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tPregap: {0} sectors", _discimage.Tracks[i].Pregap);

                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tData: {0} sectors starting at sector {1}",
                                               _discimage.Tracks[i].Sectors, _discimage.Tracks[i].StartSector);

                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tPostgap: {0} sectors",
                                               _discimage.Tracks[i].Postgap);

                    if(_discimage.Tracks[i].Flag4Ch)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tTrack is flagged as quadraphonic");

                    if(_discimage.Tracks[i].FlagDcp)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tTrack allows digital copy");

                    if(_discimage.Tracks[i].FlagPre)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tTrack has pre-emphasis applied");

                    AaruConsole.DebugWriteLine("CDRDAO plugin",
                                               "\t\tTrack resides in file {0}, type defined as {1}, starting at byte {2}",
                                               _discimage.Tracks[i].Trackfile.Datafilter.GetFilename(),
                                               _discimage.Tracks[i].Trackfile.Filetype,
                                               _discimage.Tracks[i].Trackfile.Offset);

                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tIndexes:");

                    foreach(KeyValuePair<int, ulong> kvp in _discimage.Tracks[i].Indexes)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\t\tIndex {0} starts at sector {1}", kvp.Key,
                                                   kvp.Value);

                    if(_discimage.Tracks[i].Isrc == null)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tISRC is not set.");
                    else
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tISRC: {0}", _discimage.Tracks[i].Isrc);

                    if(_discimage.Tracks[i].Arranger == null)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tArranger is not set.");
                    else
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tArranger: {0}", _discimage.Tracks[i].Arranger);

                    if(_discimage.Tracks[i].Composer == null)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tComposer is not set.");
                    else
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tComposer: {0}", _discimage.Tracks[i].Composer);

                    if(_discimage.Tracks[i].Performer == null)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tPerformer is not set.");
                    else
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tPerformer: {0}",
                                                   _discimage.Tracks[i].Performer);

                    if(_discimage.Tracks[i].Songwriter == null)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tSongwriter is not set.");
                    else
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tSongwriter: {0}",
                                                   _discimage.Tracks[i].Songwriter);

                    if(_discimage.Tracks[i].Title == null)
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tTitle is not set.");
                    else
                        AaruConsole.DebugWriteLine("CDRDAO plugin", "\t\tTitle: {0}", _discimage.Tracks[i].Title);
                }

                AaruConsole.DebugWriteLine("CDRDAO plugin", "Building offset map");

                Partitions = new List<Partition>();
                _offsetmap = new Dictionary<uint, ulong>();

                ulong byteOffset        = 0;
                ulong partitionSequence = 0;

                for(int i = 0; i < _discimage.Tracks.Count; i++)
                {
                    ulong index0Len = 0;

                    if(_discimage.Tracks[i].Sequence == 1 &&
                       i                             != 0)
                        throw new ImageNotSupportedException("Unordered tracks");

                    // Index 01
                    var partition = new Partition
                    {
                        Description = $"Track {_discimage.Tracks[i].Sequence}.",
                        Name        = _discimage.Tracks[i].Title,
                        Start       = _discimage.Tracks[i].StartSector,
                        Size        = (_discimage.Tracks[i].Sectors - index0Len) * _discimage.Tracks[i].Bps,
                        Length      = _discimage.Tracks[i].Sectors - index0Len,
                        Sequence    = partitionSequence,
                        Offset      = byteOffset,
                        Type        = _discimage.Tracks[i].Tracktype
                    };

                    byteOffset += partition.Size;
                    partitionSequence++;

                    if(!_offsetmap.ContainsKey(_discimage.Tracks[i].Sequence))
                        _offsetmap.Add(_discimage.Tracks[i].Sequence, partition.Start);
                    else
                    {
                        _offsetmap.TryGetValue(_discimage.Tracks[i].Sequence, out ulong oldStart);

                        if(partition.Start < oldStart)
                        {
                            _offsetmap.Remove(_discimage.Tracks[i].Sequence);
                            _offsetmap.Add(_discimage.Tracks[i].Sequence, partition.Start);
                        }
                    }

                    Partitions.Add(partition);
                }

                // Print partition map
                AaruConsole.DebugWriteLine("CDRDAO plugin", "printing partition map");

                foreach(Partition partition in Partitions)
                {
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "Partition sequence: {0}", partition.Sequence);
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tPartition name: {0}", partition.Name);
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tPartition description: {0}", partition.Description);
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tPartition type: {0}", partition.Type);
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tPartition starting sector: {0}", partition.Start);
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tPartition sectors: {0}", partition.Length);
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tPartition starting offset: {0}", partition.Offset);
                    AaruConsole.DebugWriteLine("CDRDAO plugin", "\tPartition size in bytes: {0}", partition.Size);
                }

                foreach(CdrdaoTrack track in _discimage.Tracks)
                {
                    _imageInfo.ImageSize += track.Bps * track.Sectors;
                    _imageInfo.Sectors   += track.Sectors;
                }

                if(_discimage.Disktype != MediaType.CDG     &&
                   _discimage.Disktype != MediaType.CDEG    &&
                   _discimage.Disktype != MediaType.CDMIDI  &&
                   _discimage.Disktype != MediaType.CDROMXA &&
                   _discimage.Disktype != MediaType.CDDA    &&
                   _discimage.Disktype != MediaType.CDI     &&
                   _discimage.Disktype != MediaType.CDPLUS)
                    _imageInfo.SectorSize = 2048; // Only data tracks
                else
                    _imageInfo.SectorSize = 2352; // All others

                if(_discimage.Mcn != null)
                    _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);

                _imageInfo.Application = "CDRDAO";

                _imageInfo.CreationTime         = imageFilter.GetCreationTime();
                _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();

                _imageInfo.Comments          = _discimage.Comment;
                _imageInfo.MediaSerialNumber = _discimage.Mcn;
                _imageInfo.MediaBarcode      = _discimage.Barcode;
                _imageInfo.MediaType         = _discimage.Disktype;

                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                foreach(CdrdaoTrack track in _discimage.Tracks)
                {
                    if(track.Subchannel)
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                    switch(track.Tracktype)
                    {
                        case CDRDAO_TRACK_TYPE_AUDIO:
                        {
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);

                            break;
                        }
                        case CDRDAO_TRACK_TYPE_MODE2:
                        case CDRDAO_TRACK_TYPE_MODE2_MIX:
                        {
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                            break;
                        }
                        case CDRDAO_TRACK_TYPE_MODE2_RAW:
                        {
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                            break;
                        }
                        case CDRDAO_TRACK_TYPE_MODE1_RAW:
                        {
                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                            break;
                        }
                    }
                }

                _imageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                AaruConsole.VerboseWriteLine("CDRDAO image describes a disc of type {0}", _imageInfo.MediaType);

                if(!string.IsNullOrEmpty(_imageInfo.Comments))
                    AaruConsole.VerboseWriteLine("CDRDAO comments: {0}", _imageInfo.Comments);

                _sectorBuilder = new SectorBuilder();

                return true;
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter);
                AaruConsole.ErrorWriteLine("Exception: {0}", ex.Message);
                AaruConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);

                return false;
            }
        }

        /// <inheritdoc />
        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_MCN:
                {
                    if(_discimage.Mcn != null)
                        return Encoding.ASCII.GetBytes(_discimage.Mcn);

                    throw new FeatureNotPresentImageException("Image does not contain MCN information.");
                }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress) => ReadSectors(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag) => ReadSectorsTag(sectorAddress, 1, tag);

        /// <inheritdoc />
        public byte[] ReadSector(ulong sectorAddress, uint track) => ReadSectors(sectorAddress, 1, track);

        /// <inheritdoc />
        public byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag) =>
            ReadSectorsTag(sectorAddress, 1, track, tag);

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetmap where sectorAddress >= kvp.Value
                                                     from cdrdaoTrack in _discimage.Tracks
                                                     where cdrdaoTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrdaoTrack.Sectors select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetmap where sectorAddress >= kvp.Value
                                                     from cdrdaoTrack in _discimage.Tracks
                                                     where cdrdaoTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrdaoTrack.Sectors select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            var aaruTrack = new CdrdaoTrack
            {
                Sequence = 0
            };

            foreach(CdrdaoTrack cdrdaoTrack in _discimage.Tracks.Where(cdrdaoTrack => cdrdaoTrack.Sequence == track))
            {
                aaruTrack = cdrdaoTrack;

                break;
            }

            if(aaruTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > aaruTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;
            bool mode2 = false;

            switch(aaruTrack.Tracktype)
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
                    mode2        = true;
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
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            if(aaruTrack.Subchannel)
                sectorSkip += 96;

            byte[] buffer = new byte[sectorSize * length];

            _imageStream = aaruTrack.Trackfile.Datafilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.Trackfile.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(mode2)
            {
                var mode2Ms = new MemoryStream((int)(sectorSize * length));

                buffer = br.ReadBytes((int)((sectorSize + sectorSkip) * length));

                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    Array.Copy(buffer, (sectorSize + sectorSkip) * i, sector, 0, sectorSize);
                    sector = Sector.GetUserDataFromMode2(sector);
                    mode2Ms.Write(sector, 0, sector.Length);
                }

                buffer = mode2Ms.ToArray();
            }
            else if(sectorOffset == 0 &&
                    sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            // cdrdao audio tracks are endian swapped corresponding to Aaru
            if(aaruTrack.Tracktype != CDRDAO_TRACK_TYPE_AUDIO)
                return buffer;

            byte[] swapped = new byte[buffer.Length];

            for(long i = 0; i < buffer.Length; i += 2)
            {
                swapped[i] = buffer[i + 1];
                swapped[i             + 1] = buffer[i];
            }

            return swapped;
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(tag == SectorTagType.CdTrackFlags ||
               tag == SectorTagType.CdTrackIsrc)
                track = (uint)sectorAddress;

            var aaruTrack = new CdrdaoTrack
            {
                Sequence = 0
            };

            foreach(CdrdaoTrack cdrdaoTrack in _discimage.Tracks.Where(cdrdaoTrack => cdrdaoTrack.Sequence == track))
            {
                aaruTrack = cdrdaoTrack;

                break;
            }

            if(aaruTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > aaruTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip = 0;

            if(!aaruTrack.Subchannel &&
               tag == SectorTagType.CdSectorSubchannel)
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

                    if(aaruTrack.Tracktype != CDRDAO_TRACK_TYPE_AUDIO)
                        flags |= CdFlags.DataTrack;

                    if(aaruTrack.FlagDcp)
                        flags |= CdFlags.CopyPermitted;

                    if(aaruTrack.FlagPre)
                        flags |= CdFlags.PreEmphasis;

                    if(aaruTrack.Flag4Ch)
                        flags |= CdFlags.FourChannel;

                    return new[]
                    {
                        (byte)flags
                    };
                }
                case SectorTagType.CdTrackIsrc:
                    if(aaruTrack.Isrc == null)
                        return null;

                    return Encoding.UTF8.GetBytes(aaruTrack.Isrc);
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            switch(aaruTrack.Tracktype)
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

            _imageStream = aaruTrack.Trackfile.Datafilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.Trackfile.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
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

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        /// <inheritdoc />
        public byte[] ReadSectorLong(ulong sectorAddress, uint track) => ReadSectorsLong(sectorAddress, 1, track);

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetmap where sectorAddress >= kvp.Value
                                                     from cdrdaoTrack in _discimage.Tracks
                                                     where cdrdaoTrack.Sequence      == kvp.Key
                                                     where sectorAddress - kvp.Value < cdrdaoTrack.Sectors select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), "Sector address not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            var aaruTrack = new CdrdaoTrack
            {
                Sequence = 0
            };

            foreach(CdrdaoTrack cdrdaoTrack in _discimage.Tracks.Where(cdrdaoTrack => cdrdaoTrack.Sequence == track))
            {
                aaruTrack = cdrdaoTrack;

                break;
            }

            if(aaruTrack.Sequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length > aaruTrack.Sectors)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      "Requested more sectors than present in track, won't cross tracks");

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(aaruTrack.Tracktype)
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

            if(aaruTrack.Subchannel)
                sectorSkip += 96;

            byte[] buffer = new byte[sectorSize * length];

            _imageStream = aaruTrack.Trackfile.Datafilter.GetDataForkStream();
            var br = new BinaryReader(_imageStream);

            br.BaseStream.
               Seek((long)aaruTrack.Trackfile.Offset + (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                    SeekOrigin.Begin);

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
                buffer = br.ReadBytes((int)(sectorSize * length));
            else
                for(int i = 0; i < length; i++)
                {
                    br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                    byte[] sector = br.ReadBytes((int)sectorSize);
                    br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);

                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            switch(aaruTrack.Tracktype)
            {
                case CDRDAO_TRACK_TYPE_MODE1:
                {
                    byte[] fullSector = new byte[2352];
                    byte[] fullBuffer = new byte[2352 * length];

                    for(uint i = 0; i < length; i++)
                    {
                        Array.Copy(buffer, i * 2048, fullSector, 16, 2048);
                        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode1, (long)(sectorAddress + i));
                        _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode1);
                        Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                    }

                    buffer = fullBuffer;

                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE2_FORM1:
                {
                    byte[] fullSector = new byte[2352];
                    byte[] fullBuffer = new byte[2352 * length];

                    for(uint i = 0; i < length; i++)
                    {
                        Array.Copy(buffer, i * 2048, fullSector, 24, 2048);

                        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Form1,
                                                         (long)(sectorAddress + i));

                        _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode2Form1);
                        Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                    }

                    buffer = fullBuffer;

                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE2_FORM2:
                {
                    byte[] fullSector = new byte[2352];
                    byte[] fullBuffer = new byte[2352 * length];

                    for(uint i = 0; i < length; i++)
                    {
                        Array.Copy(buffer, i * 2324, fullSector, 24, 2324);

                        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Form2,
                                                         (long)(sectorAddress + i));

                        _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode2Form2);
                        Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                    }

                    buffer = fullBuffer;

                    break;
                }
                case CDRDAO_TRACK_TYPE_MODE2:
                case CDRDAO_TRACK_TYPE_MODE2_MIX:
                {
                    byte[] fullSector = new byte[2352];
                    byte[] fullBuffer = new byte[2352 * length];

                    for(uint i = 0; i < length; i++)
                    {
                        _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Formless,
                                                         (long)(sectorAddress + i));

                        Array.Copy(buffer, i                    * 2336, fullSector, 16, 2336);
                        Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                    }

                    buffer = fullBuffer;

                    break;
                }

                // cdrdao audio tracks are endian swapped corresponding to Aaru
                case CDRDAO_TRACK_TYPE_AUDIO:
                {
                    byte[] swapped = new byte[buffer.Length];

                    for(long i = 0; i < buffer.Length; i += 2)
                    {
                        swapped[i] = buffer[i + 1];
                        swapped[i             + 1] = buffer[i];
                    }

                    buffer = swapped;

                    break;
                }
            }

            return buffer;
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(Session session) => GetSessionTracks(session.SessionSequence);

        /// <inheritdoc />
        public List<Track> GetSessionTracks(ushort session)
        {
            if(session == 1)
                return Tracks;

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }
    }
}