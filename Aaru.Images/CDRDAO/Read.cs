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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders.CD;
using Session = Aaru.CommonTypes.Structs.Session;

namespace Aaru.Images;

public sealed partial class Cdrdao
{
#region IWritableOpticalImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        if(imageFilter == null) return ErrorNumber.NoSuchFile;

        _cdrdaoFilter = imageFilter;

        try
        {
            imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
            _tocStream = new StreamReader(imageFilter.GetDataForkStream());
            var inTrack = false;

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
                Tracks  = [],
                Comment = ""
            };

            var  currentTrack       = new CdrdaoTrack();
            uint currentTrackNumber = 0;
            currentTrack.Indexes = new Dictionary<int, ulong>();
            currentTrack.Pregap  = 0;
            ulong currentSector  = 0;
            var   nextIndex      = 2;
            var   commentBuilder = new StringBuilder();

            _tocStream = new StreamReader(_cdrdaoFilter.GetDataForkStream());
            string line;
            var    lineNumber = 0;

            while(_tocStream.Peek() >= 0)
            {
                lineNumber++;
                line = _tocStream.ReadLine();

                matchDiskType = regexDiskType.Match(line ?? "");
                matchComment  = regexComment.Match(line);

                // Skip comments at start of file
                if(matchComment.Success) continue;

                if(!matchDiskType.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Not_a_CDRDAO_TOC_or_TOC_type_not_in_line_0,
                                               lineNumber);

                    return ErrorNumber.InvalidArgument;
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

                matchComment  = regexComment.Match(line ?? "");
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
                    if(matchComment.Groups["comment"].Value.StartsWith(" Track ", StringComparison.Ordinal)) continue;

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_comment_1_at_line_0,
                                               lineNumber,
                                               matchComment.Groups["comment"].Value.Trim());

                    commentBuilder.AppendLine(matchComment.Groups["comment"].Value.Trim());
                }
                else if(matchDiskType.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_1_at_line_0,
                                               lineNumber,
                                               matchDiskType.Groups["type"].Value);

                    _discimage.Disktypestr = matchDiskType.Groups["type"].Value;

                    _discimage.Disktype = matchDiskType.Groups["type"].Value switch
                                          {
                                              "CD_DA"     => MediaType.CDDA,
                                              "CD_ROM"    => MediaType.CDROM,
                                              "CD_ROM_XA" => MediaType.CDROMXA,
                                              "CD_I"      => MediaType.CDI,
                                              _           => MediaType.CD
                                          };
                }
                else if(matchMcn.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_CATALOG_1_at_line_0,
                                               lineNumber,
                                               matchMcn.Groups["catalog"].Value);

                    _discimage.Mcn = matchMcn.Groups["catalog"].Value;
                }
                else if(matchTrack.Success)
                {
                    if(matchTrack.Groups["subchan"].Value == "")
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_TRACK_type_1_with_no_subchannel_at_line_0,
                                                   lineNumber,
                                                   matchTrack.Groups["type"].Value);
                    }
                    else
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_TRACK_type_1_subchannel_2_at_line_0,
                                                   lineNumber,
                                                   matchTrack.Groups["type"].Value,
                                                   matchTrack.Groups["subchan"].Value);
                    }

                    if(inTrack)
                    {
                        currentSector += currentTrack.Sectors;

                        if(currentTrack.Pregap != currentTrack.Sectors)
                            currentTrack.Indexes.TryAdd(1, currentTrack.StartSector + currentTrack.Pregap);

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
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization.Track_mode_0_is_unsupported,
                                                                     matchTrack.Groups["type"].Value));

                            return ErrorNumber.NotSupported;
                        }
                    }

                    switch(matchTrack.Groups["subchan"].Value)
                    {
                        case "":
                            break;
                        case "RW":
                            currentTrack.Packedsubchannel = true;
                            goto case "RW_RAW";
                        case "RW_RAW":
                            currentTrack.Subchannel = true;

                            break;
                        default:
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                        .Track_subchannel_mode_0_is_unsupported,
                                                                     matchTrack.Groups["subchan"].Value));

                            return ErrorNumber.NotSupported;
                        }
                    }

                    currentTrack.Tracktype = matchTrack.Groups["type"].Value;

                    currentTrack.Sequence    = currentTrackNumber;
                    currentTrack.StartSector = currentSector;
                }
                else if(matchCopy.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_1_COPY_at_line_0,
                                               lineNumber,
                                               matchCopy.Groups["no"].Value);

                    currentTrack.FlagDcp |= inTrack && matchCopy.Groups["no"].Value == "";
                }
                else if(matchEmphasis.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_1_PRE_EMPHASIS_at_line_0,
                                               lineNumber,
                                               matchEmphasis.Groups["no"].Value);

                    currentTrack.FlagPre |= inTrack && matchEmphasis.Groups["no"].Value == "";
                }
                else if(matchStereo.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_1_CHANNEL_AUDIO_at_line_0,
                                               lineNumber,
                                               matchStereo.Groups["num"].Value);

                    currentTrack.Flag4Ch |= inTrack && matchStereo.Groups["num"].Value == "FOUR";
                }
                else if(matchIsrc.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_ISRC_1_at_line_0,
                                               lineNumber,
                                               matchIsrc.Groups["isrc"].Value);

                    if(inTrack) currentTrack.Isrc = matchIsrc.Groups["isrc"].Value;
                }
                else if(matchIndex.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_INDEX_1_at_line_0,
                                               lineNumber,
                                               matchIndex.Groups["address"].Value);

                    string[] lengthString = matchFile.Groups["length"].Value.Split(':');

                    ulong nextIndexPos = ulong.Parse(lengthString[0]) * 60 * 75 +
                                         ulong.Parse(lengthString[1]) * 75      +
                                         ulong.Parse(lengthString[2]);

                    currentTrack.Indexes.Add(nextIndex, nextIndexPos + currentTrack.Pregap + currentTrack.StartSector);
                }
                else if(matchPregap.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_START_1_at_line_0,
                                               lineNumber,
                                               matchPregap.Groups["address"].Value);

                    currentTrack.Indexes.Add(0, currentTrack.StartSector);

                    if(matchPregap.Groups["address"].Value != "")
                    {
                        string[] lengthString = matchPregap.Groups["address"].Value.Split(':');

                        currentTrack.Pregap = ulong.Parse(lengthString[0]) * 60 * 75 +
                                              ulong.Parse(lengthString[1]) * 75      +
                                              ulong.Parse(lengthString[2]);
                    }
                    else
                        currentTrack.Pregap = currentTrack.Sectors;
                }
                else if(matchZeroPregap.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_PREGAP_1_at_line_0,
                                               lineNumber,
                                               matchZeroPregap.Groups["length"].Value);

                    currentTrack.Indexes.Add(0, currentTrack.StartSector);
                    string[] lengthString = matchZeroPregap.Groups["length"].Value.Split(':');

                    currentTrack.Pregap = ulong.Parse(lengthString[0]) * 60 * 75 +
                                          ulong.Parse(lengthString[1]) * 75      +
                                          ulong.Parse(lengthString[2]);
                }
                else if(matchZeroData.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_ZERO_1_at_line_0,
                                               lineNumber,
                                               matchZeroData.Groups["length"].Value);
                }
                else if(matchZeroAudio.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_SILENCE_1_at_line_0,
                                               lineNumber,
                                               matchZeroAudio.Groups["length"].Value);
                }
                else
                {
                    if(matchAudioFile.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_AUDIOFILE_1_at_line_0,
                                                   lineNumber,
                                                   matchAudioFile.Groups["filename"].Value);


                        currentTrack.Trackfile = new CdrdaoTrackFile
                        {
                            Datafilter =
                                PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                                    matchAudioFile.Groups["filename"]
                                                                                       .Value)),
                            Datafile = matchAudioFile.Groups["filename"].Value,
                            Offset = matchAudioFile.Groups["base_offset"].Value != ""
                                         ? ulong.Parse(matchAudioFile.Groups["base_offset"].Value)
                                         : 0,
                            Filetype = "BINARY",
                            Sequence = currentTrackNumber
                        };

                        ulong startSectors = 0;

                        if(matchAudioFile.Groups["start"].Value != "")
                        {
                            string[] startString = matchAudioFile.Groups["start"].Value.Split(':');

                            startSectors = ulong.Parse(startString[0]) * 60 * 75 +
                                           ulong.Parse(startString[1]) * 75      +
                                           ulong.Parse(startString[2]);
                        }

                        currentTrack.Trackfile.Offset += startSectors * currentTrack.Bps;

                        if(matchAudioFile.Groups["length"].Value != "")
                        {
                            string[] lengthString = matchAudioFile.Groups["length"].Value.Split(':');

                            currentTrack.Sectors = ulong.Parse(lengthString[0]) * 60 * 75 +
                                                   ulong.Parse(lengthString[1]) * 75      +
                                                   ulong.Parse(lengthString[2]);
                        }
                        else
                        {
                            currentTrack.Sectors =
                                ((ulong)currentTrack.Trackfile.Datafilter.DataForkLength -
                                 currentTrack.Trackfile.Offset) /
                                currentTrack.Bps;
                        }
                    }
                    else if(matchFile.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_DATAFILE_1_at_line_0,
                                                   lineNumber,
                                                   matchFile.Groups["filename"].Value);

                        currentTrack.Trackfile = new CdrdaoTrackFile
                        {
                            Datafilter =
                                PluginRegister.Singleton.GetFilter(Path.Combine(imageFilter.ParentFolder,
                                                                                    matchFile.Groups["filename"]
                                                                                       .Value)),
                            Datafile = matchAudioFile.Groups["filename"].Value,
                            Offset = matchFile.Groups["base_offset"].Value != ""
                                         ? ulong.Parse(matchFile.Groups["base_offset"].Value)
                                         : 0,
                            Filetype = "BINARY",
                            Sequence = currentTrackNumber
                        };

                        if(matchFile.Groups["length"].Value != "")
                        {
                            string[] lengthString = matchFile.Groups["length"].Value.Split(':');

                            currentTrack.Sectors = ulong.Parse(lengthString[0]) * 60 * 75 +
                                                   ulong.Parse(lengthString[1]) * 75      +
                                                   ulong.Parse(lengthString[2]);
                        }
                        else
                        {
                            currentTrack.Sectors =
                                ((ulong)currentTrack.Trackfile.Datafilter.DataForkLength -
                                 currentTrack.Trackfile.Offset) /
                                currentTrack.Bps;
                        }
                    }
                    else if(matchTitle.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_TITLE_1_at_line_0,
                                                   lineNumber,
                                                   matchTitle.Groups["title"].Value);

                        if(inTrack)
                            currentTrack.Title = matchTitle.Groups["title"].Value;
                        else
                            _discimage.Title = matchTitle.Groups["title"].Value;
                    }
                    else if(matchPerformer.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_PERFORMER_1_at_line_0,
                                                   lineNumber,
                                                   matchPerformer.Groups["performer"].Value);

                        if(inTrack)
                            currentTrack.Performer = matchPerformer.Groups["performer"].Value;
                        else
                            _discimage.Performer = matchPerformer.Groups["performer"].Value;
                    }
                    else if(matchSongwriter.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_SONGWRITER_1_at_line_0,
                                                   lineNumber,
                                                   matchSongwriter.Groups["songwriter"].Value);

                        if(inTrack)
                            currentTrack.Songwriter = matchSongwriter.Groups["songwriter"].Value;
                        else
                            _discimage.Songwriter = matchSongwriter.Groups["songwriter"].Value;
                    }
                    else if(matchComposer.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_COMPOSER_1_at_line_0,
                                                   lineNumber,
                                                   matchComposer.Groups["composer"].Value);

                        if(inTrack)
                            currentTrack.Composer = matchComposer.Groups["composer"].Value;
                        else
                            _discimage.Composer = matchComposer.Groups["composer"].Value;
                    }
                    else if(matchArranger.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_ARRANGER_1_at_line_0,
                                                   lineNumber,
                                                   matchArranger.Groups["arranger"].Value);

                        if(inTrack)
                            currentTrack.Arranger = matchArranger.Groups["arranger"].Value;
                        else
                            _discimage.Arranger = matchArranger.Groups["arranger"].Value;
                    }
                    else if(matchMessage.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_MESSAGE_1_at_line_0,
                                                   lineNumber,
                                                   matchMessage.Groups["message"].Value);

                        if(inTrack)
                            currentTrack.Message = matchMessage.Groups["message"].Value;
                        else
                            _discimage.Message = matchMessage.Groups["message"].Value;
                    }
                    else if(matchDiscId.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_DISC_ID_1_at_line_0,
                                                   lineNumber,
                                                   matchDiscId.Groups["discid"].Value);

                        if(!inTrack) _discimage.DiskId = matchDiscId.Groups["discid"].Value;
                    }
                    else if(matchUpc.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_UPC_EAN_1_at_line_0,
                                                   lineNumber,
                                                   matchUpc.Groups["catalog"].Value);

                        if(!inTrack) _discimage.Barcode = matchUpc.Groups["catalog"].Value;
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
                    AaruConsole.ErrorWriteLine(string.Format("Found unknown field defined at line {0}: \"{1}\"", line, _line));
                    return ErrorNumber.NotSupported;
                }
                */
            }

            if(currentTrack.Sequence != 0)
            {
                if(currentTrack.Pregap != currentTrack.Sectors)
                    currentTrack.Indexes.TryAdd(1, currentTrack.StartSector + currentTrack.Pregap);

                _discimage.Tracks.Add(currentTrack);
            }

            _discimage.Comment = commentBuilder.ToString();

            // DEBUG information
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Disc_image_parsing_results);
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Disc_CD_TEXT);

            if(_discimage.Arranger == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Arranger_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Arranger_0, _discimage.Arranger);

            if(_discimage.Composer == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Composer_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Composer_0, _discimage.Composer);

            if(_discimage.Performer == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Performer_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Performer_0, _discimage.Performer);

            if(_discimage.Songwriter == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Songwriter_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Songwriter_0, _discimage.Songwriter);

            if(_discimage.Title == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Title_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Title_0, _discimage.Title);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Disc_information);
            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Guessed_disk_type_0, _discimage.Disktype);

            if(_discimage.Barcode == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Barcode_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Barcode_0, _discimage.Barcode);

            if(_discimage.DiskId == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Disc_ID_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Disc_ID_0, _discimage.DiskId);

            if(_discimage.Mcn == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.MCN_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.MCN_0, _discimage.Mcn);

            if(string.IsNullOrEmpty(_discimage.Comment))
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Comment_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Comment_0, _discimage.Comment);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Track_information);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "\t" + Localization.Disc_contains_0_tracks,
                                       _discimage.Tracks.Count);

            for(var i = 0; i < _discimage.Tracks.Count; i++)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t" + Localization.Track_0_information,
                                           _discimage.Tracks[i].Sequence);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization._0_bytes_per_sector,
                                           _discimage.Tracks[i].Bps);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization.Pregap_0_sectors,
                                           _discimage.Tracks[i].Pregap);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization.Data_0_sectors_starting_at_sector_1,
                                           _discimage.Tracks[i].Sectors,
                                           _discimage.Tracks[i].StartSector);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization.Postgap_0_sectors,
                                           _discimage.Tracks[i].Postgap);

                if(_discimage.Tracks[i].Flag4Ch)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Track_is_flagged_as_quadraphonic);

                if(_discimage.Tracks[i].FlagDcp)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Track_allows_digital_copy);

                if(_discimage.Tracks[i].FlagPre)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Track_has_pre_emphasis_applied);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" +
                                           Localization.Track_resides_in_file_0_type_defined_as_1_starting_at_byte_2,
                                           _discimage.Tracks[i].Trackfile.Datafilter.Filename,
                                           _discimage.Tracks[i].Trackfile.Filetype,
                                           _discimage.Tracks[i].Trackfile.Offset);

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Indexes);

                foreach(KeyValuePair<int, ulong> kvp in _discimage.Tracks[i].Indexes)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "\t\t\t" + Localization.Index_0_starts_at_sector_1,
                                               kvp.Key,
                                               kvp.Value);
                }

                if(_discimage.Tracks[i].Isrc == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.ISRC_is_not_set);
                else
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.ISRC_0, _discimage.Tracks[i].Isrc);

                if(_discimage.Tracks[i].Arranger == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Arranger_is_not_set);
                else
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "\t\t" + Localization.Arranger_0,
                                               _discimage.Tracks[i].Arranger);
                }

                if(_discimage.Tracks[i].Composer == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Composer_is_not_set);
                else
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "\t\t" + Localization.Composer_0,
                                               _discimage.Tracks[i].Composer);
                }

                if(_discimage.Tracks[i].Performer == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Performer_is_not_set);
                else
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "\t\t" + Localization.Performer_0,
                                               _discimage.Tracks[i].Performer);
                }

                if(_discimage.Tracks[i].Songwriter == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Songwriter_is_not_set);
                else
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "\t\t" + Localization.Songwriter_0,
                                               _discimage.Tracks[i].Songwriter);
                }

                if(_discimage.Tracks[i].Title == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Title_is_not_set);
                else
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Title_0, _discimage.Tracks[i].Title);
            }

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Building_offset_map);

            Partitions = [];
            _offsetmap = new Dictionary<uint, ulong>();

            ulong byteOffset        = 0;
            ulong partitionSequence = 0;

            for(var i = 0; i < _discimage.Tracks.Count; i++)
            {
                if(_discimage.Tracks[i].Sequence == 1 && i != 0)
                {
                    AaruConsole.ErrorWriteLine(Localization.Unordered_tracks);

                    return ErrorNumber.NotSupported;
                }

                // Index 01
                var partition = new Partition
                {
                    Description = string.Format(Localization.Track_0, _discimage.Tracks[i].Sequence),
                    Name        = _discimage.Tracks[i].Title,
                    Start       = _discimage.Tracks[i].StartSector,
                    Size        = _discimage.Tracks[i].Sectors * _discimage.Tracks[i].Bps,
                    Length      = _discimage.Tracks[i].Sectors,
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
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.printing_partition_map);

            foreach(Partition partition in Partitions)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Partition_sequence_0,    partition.Sequence);
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_name_0, partition.Name);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t" + Localization.Partition_description_0,
                                           partition.Description);

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_type_0, partition.Type);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t" + Localization.Partition_starting_sector_0,
                                           partition.Start);

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_sectors_0, partition.Length);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t" + Localization.Partition_starting_offset_0,
                                           partition.Offset);

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Partition_size_in_bytes_0, partition.Size);
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

            if(_discimage.Mcn != null) _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);

            _imageInfo.Application = "CDRDAO";

            _imageInfo.CreationTime         = imageFilter.CreationTime;
            _imageInfo.LastModificationTime = imageFilter.LastWriteTime;

            _imageInfo.Comments          = _discimage.Comment;
            _imageInfo.MediaSerialNumber = _discimage.Mcn;
            _imageInfo.MediaBarcode      = _discimage.Barcode;
            _imageInfo.MediaType         = _discimage.Disktype;

            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

            foreach(CdrdaoTrack track in _discimage.Tracks)
            {
                if(track.Subchannel)
                {
                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);
                }

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

            _imageInfo.MetadataMediaType = MetadataMediaType.OpticalDisc;

            AaruConsole.VerboseWriteLine(Localization.CDRDAO_image_describes_a_disc_of_type_0, _imageInfo.MediaType);

            if(!string.IsNullOrEmpty(_imageInfo.Comments))
                AaruConsole.VerboseWriteLine(Localization.CDRDAO_comments_0, _imageInfo.Comments);

            _sectorBuilder = new SectorBuilder();

            return ErrorNumber.NoError;
        }
        catch(Exception ex)
        {
            AaruConsole.ErrorWriteLine(Localization.Exception_trying_to_identify_image_file_0, imageFilter);
            AaruConsole.WriteException(ex);

            return ErrorNumber.UnexpectedException;
        }
    }

    /// <inheritdoc />
    public ErrorNumber ReadMediaTag(MediaTagType tag, out byte[] buffer)
    {
        buffer = null;

        switch(tag)
        {
            case MediaTagType.CD_MCN:
            {
                if(_discimage.Mcn == null) return ErrorNumber.NoData;

                buffer = Encoding.ASCII.GetBytes(_discimage.Mcn);

                return ErrorNumber.NoError;
            }
            default:
                return ErrorNumber.NotSupported;
        }
    }

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, out byte[] buffer) => ReadSectors(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSector(ulong sectorAddress, uint track, out byte[] buffer) =>
        ReadSectors(sectorAddress, 1, track, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag, out byte[] buffer) =>
        ReadSectorsTag(sectorAddress, 1, track, tag, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetmap
                                                 where sectorAddress >= kvp.Value
                                                 from cdrdaoTrack in _discimage.Tracks
                                                 where cdrdaoTrack.Sequence      == kvp.Key
                                                 where sectorAddress - kvp.Value < cdrdaoTrack.Sectors
                                                 select kvp)
            return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetmap
                                                 where sectorAddress >= kvp.Value
                                                 from cdrdaoTrack in _discimage.Tracks
                                                 where cdrdaoTrack.Sequence      == kvp.Key
                                                 where sectorAddress - kvp.Value < cdrdaoTrack.Sectors
                                                 select kvp)
            return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, uint track, out byte[] buffer)
    {
        buffer = null;

        var aaruTrack = new CdrdaoTrack
        {
            Sequence = 0
        };

        foreach(CdrdaoTrack cdrdaoTrack in _discimage.Tracks.Where(cdrdaoTrack => cdrdaoTrack.Sequence == track))
        {
            aaruTrack = cdrdaoTrack;

            break;
        }

        if(aaruTrack.Sequence == 0) return ErrorNumber.SectorNotFound;

        if(length > aaruTrack.Sectors) return ErrorNumber.OutOfRange;

        uint sectorOffset;
        uint sectorSize;
        uint sectorSkip;
        var  mode2 = false;

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
            default:
                return ErrorNumber.NotSupported;
        }

        if(aaruTrack.Subchannel) sectorSkip += 96;

        buffer = new byte[sectorSize * length];

        _imageStream = aaruTrack.Trackfile.Datafilter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.Seek((long)aaruTrack.Trackfile.Offset +
                           (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                           SeekOrigin.Begin);

        if(mode2)
        {
            var mode2Ms = new MemoryStream((int)(sectorSize * length));

            buffer = br.ReadBytes((int)((sectorSize + sectorSkip) * length));

            for(var i = 0; i < length; i++)
            {
                var sector = new byte[sectorSize];
                Array.Copy(buffer, (sectorSize + sectorSkip) * i, sector, 0, sectorSize);
                sector = Sector.GetUserDataFromMode2(sector);
                mode2Ms.Write(sector, 0, sector.Length);
            }

            buffer = mode2Ms.ToArray();
        }
        else if(sectorOffset == 0 && sectorSkip == 0)
            buffer = br.ReadBytes((int)(sectorSize * length));
        else
        {
            for(var i = 0; i < length; i++)
            {
                br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                byte[] sector = br.ReadBytes((int)sectorSize);
                br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
            }
        }

        // cdrdao audio tracks are endian swapped corresponding to Aaru
        if(aaruTrack.Tracktype != CDRDAO_TRACK_TYPE_AUDIO) return ErrorNumber.NoError;

        var swapped = new byte[buffer.Length];

        for(long i = 0; i < buffer.Length; i += 2)
        {
            swapped[i] = buffer[i + 1];
            swapped[i             + 1] = buffer[i];
        }

        buffer = swapped;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(tag is SectorTagType.CdTrackFlags or SectorTagType.CdTrackIsrc) track = (uint)sectorAddress;

        var aaruTrack = new CdrdaoTrack
        {
            Sequence = 0
        };

        foreach(CdrdaoTrack cdrdaoTrack in _discimage.Tracks.Where(cdrdaoTrack => cdrdaoTrack.Sequence == track))
        {
            aaruTrack = cdrdaoTrack;

            break;
        }

        if(aaruTrack.Sequence == 0) return ErrorNumber.SectorNotFound;

        if(length > aaruTrack.Sectors) return ErrorNumber.OutOfRange;

        uint sectorOffset = 0;
        uint sectorSize   = 0;
        uint sectorSkip   = 0;

        if(!aaruTrack.Subchannel && tag == SectorTagType.CdSectorSubchannel) return ErrorNumber.NoData;

        switch(tag)
        {
            case SectorTagType.CdSectorEcc:
            case SectorTagType.CdSectorEccP:
            case SectorTagType.CdSectorEccQ:
            case SectorTagType.CdSectorEdc:
            case SectorTagType.CdSectorHeader:
            case SectorTagType.CdSectorSubchannel:
            case SectorTagType.CdSectorSubHeader:
            case SectorTagType.CdSectorSync:
                break;
            case SectorTagType.CdTrackFlags:
            {
                CdFlags flags = 0;

                if(aaruTrack.Tracktype != CDRDAO_TRACK_TYPE_AUDIO) flags |= CdFlags.DataTrack;

                if(aaruTrack.FlagDcp) flags |= CdFlags.CopyPermitted;

                if(aaruTrack.FlagPre) flags |= CdFlags.PreEmphasis;

                if(aaruTrack.Flag4Ch) flags |= CdFlags.FourChannel;

                buffer = [(byte)flags];

                return ErrorNumber.NoError;
            }
            case SectorTagType.CdTrackIsrc:
                if(aaruTrack.Isrc == null) return ErrorNumber.NoData;

                buffer = Encoding.UTF8.GetBytes(aaruTrack.Isrc);

                return ErrorNumber.NoError;
            default:
                return ErrorNumber.NotSupported;
        }

        switch(aaruTrack.Tracktype)
        {
            case CDRDAO_TRACK_TYPE_MODE1:
            case CDRDAO_TRACK_TYPE_MODE2_FORM1:
                if(tag != SectorTagType.CdSectorSubchannel) return ErrorNumber.NoData;

                sectorOffset = 2048;
                sectorSize   = 96;

                break;
            case CDRDAO_TRACK_TYPE_MODE2_FORM2:
            case CDRDAO_TRACK_TYPE_MODE2_MIX:
                if(tag != SectorTagType.CdSectorSubchannel) return ErrorNumber.NoData;

                sectorOffset = 2336;
                sectorSize   = 96;

                break;
            case CDRDAO_TRACK_TYPE_AUDIO:
                if(tag != SectorTagType.CdSectorSubchannel) return ErrorNumber.NoData;

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
                        return ErrorNumber.NotSupported;
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
                }

                break;
            }
            case CDRDAO_TRACK_TYPE_MODE2_RAW: // Requires reading sector
                if(tag != SectorTagType.CdSectorSubchannel) return ErrorNumber.NotImplemented;

                sectorOffset = 2352;
                sectorSize   = 96;

                break;
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        _imageStream = aaruTrack.Trackfile.Datafilter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.Seek((long)aaruTrack.Trackfile.Offset +
                           (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                           SeekOrigin.Begin);

        if(sectorOffset == 0 && sectorSkip == 0)
            buffer = br.ReadBytes((int)(sectorSize * length));
        else
        {
            for(var i = 0; i < length; i++)
            {
                br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                byte[] sector = br.ReadBytes((int)sectorSize);
                br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);
                Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, out byte[] buffer) =>
        ReadSectorsLong(sectorAddress, 1, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorLong(ulong sectorAddress, uint track, out byte[] buffer) =>
        ReadSectorsLong(sectorAddress, 1, track, out buffer);

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, out byte[] buffer)
    {
        buffer = null;

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetmap
                                                 where sectorAddress >= kvp.Value
                                                 from cdrdaoTrack in _discimage.Tracks
                                                 where cdrdaoTrack.Sequence      == kvp.Key
                                                 where sectorAddress - kvp.Value < cdrdaoTrack.Sectors
                                                 select kvp)
            return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, uint track, out byte[] buffer)
    {
        buffer = null;

        var aaruTrack = new CdrdaoTrack
        {
            Sequence = 0
        };

        foreach(CdrdaoTrack cdrdaoTrack in _discimage.Tracks.Where(cdrdaoTrack => cdrdaoTrack.Sequence == track))
        {
            aaruTrack = cdrdaoTrack;

            break;
        }

        if(aaruTrack.Sequence == 0) return ErrorNumber.SectorNotFound;

        if(length > aaruTrack.Sectors) return ErrorNumber.OutOfRange;

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
            default:
                return ErrorNumber.NotSupported;
        }

        if(aaruTrack.Subchannel) sectorSkip += 96;

        buffer = new byte[sectorSize * length];

        _imageStream = aaruTrack.Trackfile.Datafilter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.Seek((long)aaruTrack.Trackfile.Offset + (long)(sectorAddress * (sectorSize + sectorSkip)),
                           SeekOrigin.Begin);

        if(sectorSkip == 0)
            buffer = br.ReadBytes((int)(sectorSize * length));
        else
        {
            for(var i = 0; i < length; i++)
            {
                br.BaseStream.Seek(sectorOffset, SeekOrigin.Current);
                byte[] sector = br.ReadBytes((int)sectorSize);
                br.BaseStream.Seek(sectorSkip, SeekOrigin.Current);

                Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
            }
        }

        switch(aaruTrack.Tracktype)
        {
            case CDRDAO_TRACK_TYPE_MODE1:
            {
                var fullSector = new byte[2352];
                var fullBuffer = new byte[2352 * length];

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
                var fullSector = new byte[2352];
                var fullBuffer = new byte[2352 * length];

                for(uint i = 0; i < length; i++)
                {
                    Array.Copy(buffer, i * 2048, fullSector, 24, 2048);

                    _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Form1, (long)(sectorAddress + i));

                    _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode2Form1);
                    Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                }

                buffer = fullBuffer;

                break;
            }
            case CDRDAO_TRACK_TYPE_MODE2_FORM2:
            {
                var fullSector = new byte[2352];
                var fullBuffer = new byte[2352 * length];

                for(uint i = 0; i < length; i++)
                {
                    Array.Copy(buffer, i * 2324, fullSector, 24, 2324);

                    _sectorBuilder.ReconstructPrefix(ref fullSector, TrackType.CdMode2Form2, (long)(sectorAddress + i));

                    _sectorBuilder.ReconstructEcc(ref fullSector, TrackType.CdMode2Form2);
                    Array.Copy(fullSector, 0, fullBuffer, i * 2352, 2352);
                }

                buffer = fullBuffer;

                break;
            }
            case CDRDAO_TRACK_TYPE_MODE2:
            case CDRDAO_TRACK_TYPE_MODE2_MIX:
            {
                var fullSector = new byte[2352];
                var fullBuffer = new byte[2352 * length];

                for(uint i = 0; i < length; i++)
                {
                    _sectorBuilder.ReconstructPrefix(ref fullSector,
                                                     TrackType.CdMode2Formless,
                                                     (long)(sectorAddress + i));

                    Array.Copy(buffer,     i * 2336, fullSector, 16,       2336);
                    Array.Copy(fullSector, 0,        fullBuffer, i * 2352, 2352);
                }

                buffer = fullBuffer;

                break;
            }

            // cdrdao audio tracks are endian swapped corresponding to Aaru
            case CDRDAO_TRACK_TYPE_AUDIO:
            {
                var swapped = new byte[buffer.Length];

                for(long i = 0; i < buffer.Length; i += 2)
                {
                    swapped[i] = buffer[i + 1];
                    swapped[i             + 1] = buffer[i];
                }

                buffer = swapped;

                break;
            }
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) => GetSessionTracks(session.Sequence);

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) => session == 1 ? Tracks : null;

#endregion
}