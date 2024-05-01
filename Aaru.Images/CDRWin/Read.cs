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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Aaru.CommonTypes;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;
using Session = Aaru.CommonTypes.Structs.Session;
using Track = Aaru.CommonTypes.Structs.Track;
using TrackType = Aaru.CommonTypes.Enums.TrackType;

namespace Aaru.Images;

public sealed partial class CdrWin
{
#region IWritableOpticalImage Members

    /// <inheritdoc />
    public ErrorNumber Open(IFilter imageFilter)
    {
        if(imageFilter == null) return ErrorNumber.InvalidArgument;

        _cdrwinFilter = imageFilter;

        try
        {
            imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
            _cueStream = new StreamReader(imageFilter.GetDataForkStream());
            var  lineNumber     = 0;
            var  inTrack        = false;
            byte currentSession = 1;

            // Initialize all RegExs
            var regexSession                = new Regex(REGEX_SESSION);
            var regexDiskType               = new Regex(REGEX_MEDIA_TYPE);
            var regexLeadOut                = new Regex(REGEX_LEAD_OUT);
            var regexLba                    = new Regex(REGEX_LBA);
            var regexDiskId                 = new Regex(REGEX_DISC_ID);
            var regexBarCode                = new Regex(REGEX_BARCODE);
            var regexComment                = new Regex(REGEX_COMMENT);
            var regexCdText                 = new Regex(REGEX_CDTEXT);
            var regexMcn                    = new Regex(REGEX_MCN);
            var regexTitle                  = new Regex(REGEX_TITLE);
            var regexGenre                  = new Regex(REGEX_GENRE);
            var regexArranger               = new Regex(REGEX_ARRANGER);
            var regexComposer               = new Regex(REGEX_COMPOSER);
            var regexPerformer              = new Regex(REGEX_PERFORMER);
            var regexSongWriter             = new Regex(REGEX_SONGWRITER);
            var regexFile                   = new Regex(REGEX_FILE);
            var regexTrack                  = new Regex(REGEX_TRACK);
            var regexIsrc                   = new Regex(REGEX_ISRC);
            var regexIndex                  = new Regex(REGEX_INDEX);
            var regexPregap                 = new Regex(REGEX_PREGAP);
            var regexPostgap                = new Regex(REGEX_POSTGAP);
            var regexFlags                  = new Regex(REGEX_FLAGS);
            var regexApplication            = new Regex(REGEX_APPLICATION);
            var regexTruripDisc             = new Regex(REGEX_TRURIP_DISC_HASHES);
            var regexTruripDiscCrc32        = new Regex(REGEX_TRURIP_DISC_CRC32);
            var regexTruripDiscMd5          = new Regex(REGEX_TRURIP_DISC_MD5);
            var regexTruripDiscSha1         = new Regex(REGEX_TRURIP_DISC_SHA1);
            var regexTruripTrack            = new Regex(REGEX_TRURIP_TRACK_METHOD);
            var regexTruripTrackCrc32       = new Regex(REGEX_TRURIP_TRACK_CRC32);
            var regexTruripTrackMd5         = new Regex(REGEX_TRURIP_TRACK_MD5);
            var regexTruripTrackSha1        = new Regex(REGEX_TRURIP_TRACK_SHA1);
            var regexTruripTrackUnknownHash = new Regex(REGEX_TRURIP_TRACK_UNKNOWN);
            var regexDicMediaType           = new Regex(REGEX_DIC_MEDIA_TYPE);
            var regexApplicationVersion     = new Regex(REGEX_APPLICATION_VERSION);
            var regexDumpExtent             = new Regex(REGEX_DUMP_EXTENT);
            var regexAaruMediaType          = new Regex(REGEX_AARU_MEDIA_TYPE);
            var regexRedumpSdArea           = new Regex(REGEX_REDUMP_SD_AREA);
            var regexRedumpHdArea           = new Regex(REGEX_REDUMP_HD_AREA);

            // Initialize all RegEx matches
            Match matchTrack;

            // Initialize disc
            _discImage = new CdrWinDisc
            {
                Sessions   = new List<Session>(),
                Tracks     = new List<CdrWinTrack>(),
                Comment    = "",
                DiscHashes = new Dictionary<string, string>()
            };

            var currentTrack = new CdrWinTrack
            {
                Indexes = new SortedDictionary<ushort, int>()
            };

            var  currentFile             = new CdrWinTrackFile();
            long currentFileOffsetSector = 0;

            var trackCount = 0;

            Dictionary<byte, int> leadouts = new();

            while(_cueStream.Peek() >= 0)
            {
                lineNumber++;
                string line = _cueStream.ReadLine();

                matchTrack = regexTrack.Match(line);

                if(!matchTrack.Success) continue;

                var trackSeq = uint.Parse(matchTrack.Groups[1].Value);

                if(trackCount + 1 != trackSeq)
                {
                    AaruConsole.ErrorWriteLine(string.Format(Localization.Found_TRACK_0_out_of_order_in_line_1,
                                                             trackSeq,
                                                             lineNumber));

                    return ErrorNumber.InvalidArgument;
                }

                trackCount++;
            }

            if(trackCount == 0)
            {
                AaruConsole.ErrorWriteLine(Localization.No_tracks_found);

                return ErrorNumber.InvalidArgument;
            }

            var cueTracks = new CdrWinTrack[trackCount];

            lineNumber = 0;
            imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
            _cueStream = new StreamReader(imageFilter.GetDataForkStream());

            var inTruripDiscHash      = false;
            var inTruripTrackHash     = false;
            var firstTrackInSession   = false;
            var currentEmptyPregap    = 0;
            var cumulativeEmptyPregap = 0;

            const ulong gdRomSession2Offset = 45000;

            while(_cueStream.Peek() >= 0)
            {
                lineNumber++;
                string line = _cueStream.ReadLine();

                Match matchSession            = regexSession.Match(line);
                Match matchDiskType           = regexDiskType.Match(line);
                Match matchComment            = regexComment.Match(line);
                Match matchLba                = regexLba.Match(line);
                Match matchLeadOut            = regexLeadOut.Match(line);
                Match matchApplication        = regexApplication.Match(line);
                Match matchTruripDisc         = regexTruripDisc.Match(line);
                Match matchTruripTrack        = regexTruripTrack.Match(line);
                Match matchDicMediaType       = regexDicMediaType.Match(line);
                Match matchApplicationVersion = regexApplicationVersion.Match(line);
                Match matchDumpExtent         = regexDumpExtent.Match(line);
                Match matchAaruMediaType      = regexAaruMediaType.Match(line);
                Match matchRedumpSdArea       = regexRedumpSdArea.Match(line);
                Match matchRedumpHdArea       = regexRedumpHdArea.Match(line);

                if(inTruripDiscHash)
                {
                    Match matchTruripDiscCrc32 = regexTruripDiscCrc32.Match(line);
                    Match matchTruripDiscMd5   = regexTruripDiscMd5.Match(line);
                    Match matchTruripDiscSha1  = regexTruripDiscSha1.Match(line);

                    if(matchTruripDiscCrc32.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_REM_CRC32_at_line_0, lineNumber);
                        _discImage.DiscHashes.Add("crc32", matchTruripDiscCrc32.Groups[1].Value.ToLowerInvariant());

                        continue;
                    }

                    if(matchTruripDiscMd5.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_REM_MD5_at_line_0, lineNumber);
                        _discImage.DiscHashes.Add("md5", matchTruripDiscMd5.Groups[1].Value.ToLowerInvariant());

                        continue;
                    }

                    if(matchTruripDiscSha1.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_REM_SHA1_at_line_0, lineNumber);
                        _discImage.DiscHashes.Add("sha1", matchTruripDiscSha1.Groups[1].Value.ToLowerInvariant());

                        continue;
                    }
                }
                else if(inTruripTrackHash)
                {
                    Match matchTruripTrackCrc32       = regexTruripTrackCrc32.Match(line);
                    Match matchTruripTrackMd5         = regexTruripTrackMd5.Match(line);
                    Match matchTruripTrackSha1        = regexTruripTrackSha1.Match(line);
                    Match matchTruripTrackUnknownHash = regexTruripTrackUnknownHash.Match(line);

                    if(matchTruripTrackCrc32.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_CRC32_for_1_2_at_line_0,
                                                   lineNumber,
                                                   matchTruripTrackCrc32.Groups[1].Value == "Trk"
                                                       ? Localization.track
                                                       : Localization.gap,
                                                   matchTruripTrackCrc32.Groups[2].Value);

                        continue;
                    }

                    if(matchTruripTrackMd5.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_MD5_for_1_2_at_line_0,
                                                   lineNumber,
                                                   matchTruripTrackMd5.Groups[1].Value == "Trk"
                                                       ? Localization.track
                                                       : Localization.gap,
                                                   matchTruripTrackMd5.Groups[2].Value);

                        continue;
                    }

                    if(matchTruripTrackSha1.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Found_SHA1_for_1_2_at_line_0,
                                                   lineNumber,
                                                   matchTruripTrackSha1.Groups[1].Value == "Trk"
                                                       ? Localization.track
                                                       : Localization.gap,
                                                   matchTruripTrackSha1.Groups[2].Value);

                        continue;
                    }

                    if(matchTruripTrackUnknownHash.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization
                                                      .Found_unknown_hash_for_1_2_at_line_0_Please_report_this_disc_image,
                                                   lineNumber,
                                                   matchTruripTrackUnknownHash.Groups[1].Value == "Trk"
                                                       ? Localization.track
                                                       : Localization.gap,
                                                   matchTruripTrackUnknownHash.Groups[2].Value);

                        continue;
                    }
                }

                inTruripDiscHash  = false;
                inTruripTrackHash = false;

                if(matchDumpExtent.Success                                                      &&
                   !inTrack                                                                     &&
                   ulong.TryParse(matchDumpExtent.Groups["start"].Value, out ulong extentStart) &&
                   ulong.TryParse(matchDumpExtent.Groups["end"].Value,   out ulong extentEnd))
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_REM_METADATA_DUMP_EXTENT_at_line_0,
                                               lineNumber);

                    DumpHardware ??= new List<DumpHardware>();

                    DumpHardware existingDump =
                        DumpHardware.FirstOrDefault(d =>
                                                        d.Manufacturer ==
                                                        matchDumpExtent.Groups["manufacturer"].Value           &&
                                                        d.Model    == matchDumpExtent.Groups["model"].Value    &&
                                                        d.Firmware == matchDumpExtent.Groups["firmware"].Value &&
                                                        d.Serial   == matchDumpExtent.Groups["serial"].Value   &&
                                                        d.Software.Name ==
                                                        matchDumpExtent.Groups["application"].Value                   &&
                                                        d.Software.Version == matchDumpExtent.Groups["version"].Value &&
                                                        d.Software.OperatingSystem ==
                                                        matchDumpExtent.Groups["os"].Value);

                    if(existingDump is null)
                    {
                        DumpHardware.Add(new DumpHardware
                        {
                            Extents = new List<Extent>
                            {
                                new()
                                {
                                    Start = extentStart,
                                    End   = extentEnd
                                }
                            },
                            Firmware     = matchDumpExtent.Groups["firmware"].Value,
                            Manufacturer = matchDumpExtent.Groups["manufacturer"].Value,
                            Model        = matchDumpExtent.Groups["model"].Value,
                            Serial       = matchDumpExtent.Groups["serial"].Value,
                            Software = new Software
                            {
                                Name            = matchDumpExtent.Groups["application"].Value,
                                Version         = matchDumpExtent.Groups["version"].Value,
                                OperatingSystem = matchDumpExtent.Groups["os"].Value
                            }
                        });
                    }
                    else
                    {
                        existingDump.Extents = new List<Extent>(existingDump.Extents)
                            {
                                new()
                                {
                                    Start = extentStart,
                                    End   = extentEnd
                                }
                            }.OrderBy(e => e.Start)
                             .ToList();
                    }
                }
                else if(matchDicMediaType.Success && !inTrack)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_REM_METADATA_DIC_MEDIA_TYPE_at_line_0,
                                               lineNumber);

                    _discImage.AaruMediaType = matchDicMediaType.Groups[1].Value;
                }
                else if(matchAaruMediaType.Success && !inTrack)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_REM_METADATA_AARU_MEDIA_TYPE_at_line_0,
                                               lineNumber);

                    _discImage.AaruMediaType = matchAaruMediaType.Groups[1].Value;
                }
                else if(matchDiskType.Success && !inTrack)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_REM_ORIGINAL_MEDIA_TYPE_at_line_0,
                                               lineNumber);

                    _discImage.OriginalMediaType = matchDiskType.Groups[1].Value;
                }
                else if(matchSession.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_REM_SESSION_at_line_0, lineNumber);
                    currentSession      = byte.Parse(matchSession.Groups[1].Value);
                    firstTrackInSession = true;
                }
                else if(matchRedumpSdArea.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_REM_SINGLE_DENSITY_AREA_at_line_0,
                                               lineNumber);

                    _discImage.IsRedumpGigadisc = true;
                    firstTrackInSession         = true;
                }
                else if(matchRedumpHdArea.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_REM_HIGH_DENSITY_AREA_at_line_0,
                                               lineNumber);

                    _discImage.IsRedumpGigadisc = true;
                    currentSession              = 2;
                    firstTrackInSession         = true;
                }
                else if(matchLba.Success)
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_REM_MSF_at_line_0, lineNumber);
                else if(matchLeadOut.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_REM_LEAD_OUT_at_line_0, lineNumber);
                    leadouts[currentSession] = CdrWinMsfToLba(matchLeadOut.Groups[1].Value);
                }
                else if(matchApplication.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_REM_Ripping_Tool_at_line_0, lineNumber);

                    _imageInfo.Application = matchApplication.Groups[1].Value;
                }
                else if(matchApplicationVersion.Success && !inTrack)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_REM_Ripping_Tool_Version_at_line_0,
                                               lineNumber);

                    _imageInfo.ApplicationVersion = matchApplicationVersion.Groups[1].Value;
                }
                else if(matchTruripDisc.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_REM_DISC_HASHES_at_line_0, lineNumber);

                    inTruripDiscHash = true;
                }
                else if(matchTruripTrack.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Found_REM_Gap_Append_Method_1_2_HASHES_at_line_0,
                                               lineNumber,
                                               matchTruripTrack.Groups[1].Value,
                                               matchTruripTrack.Groups[2].Value);

                    inTruripTrackHash   = true;
                    _discImage.IsTrurip = true;
                }
                else if(matchComment.Success)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_REM_at_line_0, lineNumber);

                    if(_discImage.Comment == "")
                        _discImage.Comment = matchComment.Groups[1].Value; // First comment
                    else
                    {
                        _discImage.Comment +=
                            Environment.NewLine + matchComment.Groups[1].Value; // Append new comments as new lines
                    }
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
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_ARRANGER_at_line_0, lineNumber);

                        if(inTrack)
                            currentTrack.Arranger = matchArranger.Groups[1].Value;
                        else
                            _discImage.Arranger = matchArranger.Groups[1].Value;
                    }
                    else if(matchBarCode.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_UPC_EAN_at_line_0, lineNumber);

                        if(!inTrack)
                            _discImage.Barcode = matchBarCode.Groups[1].Value;
                        else
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                        .Found_barcode_field_in_incorrect_place_at_line_0,
                                                                     lineNumber));

                            return ErrorNumber.InvalidArgument;
                        }
                    }
                    else if(matchCdText.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_CDTEXTFILE_at_line_0, lineNumber);

                        if(!inTrack)
                            _discImage.CdTextFile = matchCdText.Groups[1].Value;
                        else
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                        .Found_CD_Text_file_field_in_incorrect_place_at_line_0,
                                                                     lineNumber));

                            return ErrorNumber.InvalidArgument;
                        }
                    }
                    else if(matchComposer.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_COMPOSER_at_line_0, lineNumber);

                        if(inTrack)
                            currentTrack.Composer = matchComposer.Groups[1].Value;
                        else
                            _discImage.Composer = matchComposer.Groups[1].Value;
                    }
                    else if(matchDiskId.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_DISC_ID_at_line_0, lineNumber);

                        if(!inTrack)
                            _discImage.DiscId = matchDiskId.Groups[1].Value;
                        else
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                        .Found_CDDB_ID_field_in_incorrect_place_at_line_0,
                                                                     lineNumber));

                            return ErrorNumber.InvalidArgument;
                        }
                    }
                    else if(matchFile.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_FILE_at_line_0, lineNumber);

                        if(currentTrack.Sequence != 0)
                        {
                            currentFile.Sequence   = currentTrack.Sequence;
                            currentTrack.TrackFile = currentFile;

                            _negativeEnd = currentFile.DataFilter.Length - (long)currentFile.Offset < 0;

                            currentTrack.Sectors = ((ulong)currentFile.DataFilter.Length - currentFile.Offset) /
                                                   CdrWinTrackTypeToBytesPerSector(currentTrack.TrackType);

                            cueTracks[currentTrack.Sequence - 1] = currentTrack;
                            inTrack                              = false;
                            currentTrack                         = new CdrWinTrack();
                            currentFile                          = new CdrWinTrackFile();
                        }

                        string datafile = matchFile.Groups[1].Value;
                        currentFile.FileType = matchFile.Groups[2].Value;

                        // Check if file path is quoted
                        if(datafile[0] == '"' && datafile[^1] == '"')
                            datafile = datafile.Substring(1, datafile.Length - 2); // Unquote it

                        currentFile.DataFilter = PluginRegister.Singleton.GetFilter(datafile);

                        // Check if file exists
                        if(currentFile.DataFilter == null)
                        {
                            if(datafile[0] == '/' || datafile[0] == '/' && datafile[1] == '.') // UNIX absolute path
                            {
                                var   unixPath      = new Regex("^(.+)/([^/]+)$");
                                Match unixPathMatch = unixPath.Match(datafile);

                                if(unixPathMatch.Success)
                                {
                                    currentFile.DataFilter =
                                        PluginRegister.Singleton.GetFilter(unixPathMatch.Groups[1].Value);

                                    if(currentFile.DataFilter == null)
                                    {
                                        string path = imageFilter.ParentFolder +
                                                      Path.PathSeparator       +
                                                      unixPathMatch.Groups[1].Value;

                                        currentFile.DataFilter = PluginRegister.Singleton.GetFilter(path);

                                        if(currentFile.DataFilter == null)
                                        {
                                            AaruConsole.ErrorWriteLine(string.Format(Localization.File_0_not_found,
                                                                           matchFile.Groups[1].Value));

                                            return ErrorNumber.NoSuchFile;
                                        }
                                    }
                                }
                                else
                                {
                                    AaruConsole.ErrorWriteLine(string.Format(Localization.File_0_not_found,
                                                                             matchFile.Groups[1].Value));

                                    return ErrorNumber.NoSuchFile;
                                }
                            }
                            else if(datafile[1] == ':'  && datafile[2] == '\\' ||
                                    datafile[0] == '\\' && datafile[1] == '\\' ||
                                    datafile[0] == '.'  && datafile[1] == '\\') // Windows absolute path
                            {
                                var winPath =
                                    new
                                        Regex("^(?:[a-zA-Z]\\:(\\\\|\\/)|file\\:\\/\\/|\\\\\\\\|\\.(\\/|\\\\))([^\\\\\\/\\:\\*\\?\\<\\>\\\"\\|]+(\\\\|\\/){0,1})+$");

                                Match winPathMatch = winPath.Match(datafile);

                                if(winPathMatch.Success)
                                {
                                    currentFile.DataFilter =
                                        PluginRegister.Singleton.GetFilter(winPathMatch.Groups[1].Value);

                                    if(currentFile.DataFilter == null)
                                    {
                                        string path = imageFilter.ParentFolder +
                                                      Path.PathSeparator       +
                                                      winPathMatch.Groups[1].Value;

                                        currentFile.DataFilter = PluginRegister.Singleton.GetFilter(path);

                                        if(currentFile.DataFilter == null)
                                        {
                                            AaruConsole.ErrorWriteLine(string.Format(Localization.File_0_not_found,
                                                                           matchFile.Groups[1].Value));

                                            return ErrorNumber.NoSuchFile;
                                        }
                                    }
                                }
                                else
                                {
                                    AaruConsole.ErrorWriteLine(string.Format(Localization.File_0_not_found,
                                                                             matchFile.Groups[1].Value));

                                    return ErrorNumber.NoSuchFile;
                                }
                            }
                            else
                            {
                                string path = imageFilter.ParentFolder + Path.PathSeparator + datafile;
                                currentFile.DataFilter = PluginRegister.Singleton.GetFilter(path);

                                if(currentFile.DataFilter == null)
                                {
                                    AaruConsole.ErrorWriteLine(string.Format(Localization.File_0_not_found,
                                                                             matchFile.Groups[1].Value));

                                    return ErrorNumber.NoSuchFile;
                                }
                            }
                        }

                        // File does exist, process it
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.File_0_found,
                                                   currentFile.DataFilter.Filename);

                        switch(currentFile.FileType)
                        {
                            case CDRWIN_DISK_TYPE_LITTLE_ENDIAN:
                                break;
                            case CDRWIN_DISK_TYPE_BIG_ENDIAN:
                            case CDRWIN_DISK_TYPE_AIFF:
                            case CDRWIN_DISK_TYPE_RIFF:
                            case CDRWIN_DISK_TYPE_MP3:
                                AaruConsole.ErrorWriteLine(string.Format(Localization.Unsupported_file_type_0,
                                                                         currentFile.FileType));

                                return ErrorNumber.NotImplemented;
                            default:
                                AaruConsole.ErrorWriteLine(string.Format(Localization.Unknown_file_type_0,
                                                                         currentFile.FileType));

                                return ErrorNumber.InvalidArgument;
                        }

                        currentFile.Offset   = 0;
                        currentFile.Sequence = 0;
                    }
                    else if(matchFlags.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_FLAGS_at_line_0, lineNumber);

                        if(!inTrack)
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                        .Found_FLAGS_field_in_incorrect_place_at_line_0,
                                                                     lineNumber));

                            return ErrorNumber.InvalidArgument;
                        }

                        currentTrack.FlagDcp  |= matchFlags.Groups["dcp"].Value  == "DCP";
                        currentTrack.Flag4Ch  |= matchFlags.Groups["quad"].Value == "4CH";
                        currentTrack.FlagPre  |= matchFlags.Groups["pre"].Value  == "PRE";
                        currentTrack.FlagScms |= matchFlags.Groups["scms"].Value == "SCMS";
                    }
                    else if(matchGenre.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_GENRE_at_line_0, lineNumber);

                        if(inTrack)
                            currentTrack.Genre = matchGenre.Groups[1].Value;
                        else
                            _discImage.Genre = matchGenre.Groups[1].Value;
                    }
                    else if(matchIndex.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_INDEX_at_line_0, lineNumber);

                        if(!inTrack)
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization.Found_INDEX_before_a_track_0,
                                                                     lineNumber));

                            return ErrorNumber.InvalidArgument;
                        }

                        var index  = ushort.Parse(matchIndex.Groups[1].Value);
                        int offset = CdrWinMsfToLba(matchIndex.Groups[2].Value) + cumulativeEmptyPregap;

                        if(index != 0 && index != 1 && currentTrack.Indexes.Count == 0)
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                        .Found_INDEX_0_before_INDEX_00_or_INDEX_01,
                                                                     index));

                            return ErrorNumber.InvalidArgument;
                        }

                        if(index == 0 || index == 1 && !currentTrack.Indexes.ContainsKey(0))
                        {
                            if((int)(currentTrack.Sequence - 2) >= 0 && offset > 1)
                            {
                                cueTracks[currentTrack.Sequence - 2].Sectors =
                                    (ulong)(offset - (int)currentFileOffsetSector);

                                currentFile.Offset += cueTracks[currentTrack.Sequence - 2].Sectors *
                                                      cueTracks[currentTrack.Sequence - 2].Bps;

                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                           Localization.Sets_currentFile_offset_to_0,
                                                           currentFile.Offset);

                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                           "cueTracks[currentTrack.sequence-2].sectors = {0}",
                                                           cueTracks[currentTrack.Sequence - 2].Sectors);

                                AaruConsole.DebugWriteLine(MODULE_NAME,
                                                           "cueTracks[currentTrack.sequence-2].bps = {0}",
                                                           cueTracks[currentTrack.Sequence - 2].Bps);
                            }
                        }

                        if((index == 0 || index == 1 && !currentTrack.Indexes.ContainsKey(0)) &&
                           currentTrack.Sequence == 1)
                        {
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization.Sets_currentFile_offset_to_0,
                                                       offset * currentTrack.Bps);

                            currentFile.Offset = (ulong)(offset * currentTrack.Bps);
                        }

                        if(currentTrack.Indexes.Count == 0)
                        {
                            if(firstTrackInSession && index != 0 && offset > 150)
                            {
                                currentTrack.Indexes[0] =  offset - 150;
                                firstTrackInSession     =  false;
                                currentFileOffsetSector =  offset - 150;
                                currentFile.Offset      -= (ulong)(150 * currentTrack.Bps);
                            }
                            else
                                currentFileOffsetSector = offset;
                        }

                        if(index == 1 && currentEmptyPregap > 0 && !currentTrack.Indexes.ContainsKey(0))
                        {
                            currentTrack.Indexes[0] =  offset;
                            currentFile.Offset      -= (ulong)(currentEmptyPregap * currentTrack.Bps);
                            offset                  += currentEmptyPregap;
                            cumulativeEmptyPregap   += currentEmptyPregap;
                            currentEmptyPregap      =  0;
                        }

                        currentTrack.Indexes.Add(index, offset);
                    }
                    else if(matchIsrc.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_ISRC_at_line_0, lineNumber);

                        if(!inTrack)
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization.Found_ISRC_before_a_track_0,
                                                                     lineNumber));

                            return ErrorNumber.InvalidArgument;
                        }

                        currentTrack.Isrc = matchIsrc.Groups[1].Value;
                    }
                    else if(matchMcn.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_CATALOG_at_line_0, lineNumber);

                        if(!inTrack)
                            _discImage.Mcn = matchMcn.Groups[1].Value;
                        else
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                        .Found_CATALOG_field_in_incorrect_place_at_line_0,
                                                                     lineNumber));

                            return ErrorNumber.InvalidArgument;
                        }
                    }
                    else if(matchPerformer.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_PERFORMER_at_line_0, lineNumber);

                        if(inTrack)
                            currentTrack.Performer = matchPerformer.Groups[1].Value;
                        else
                            _discImage.Performer = matchPerformer.Groups[1].Value;
                    }
                    else if(matchPostgap.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_POSTGAP_at_line_0, lineNumber);

                        if(inTrack)
                            currentTrack.Postgap = CdrWinMsfToLba(matchPostgap.Groups[1].Value);
                        else
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                        .Found_POSTGAP_field_before_a_track_at_line_0,
                                                                     lineNumber));

                            return ErrorNumber.InvalidArgument;
                        }
                    }
                    else if(matchPregap.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_PREGAP_at_line_0, lineNumber);

                        if(!inTrack)
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                        .Found_PREGAP_field_before_a_track_at_line_0,
                                                                     lineNumber));

                            return ErrorNumber.InvalidArgument;
                        }

                        currentEmptyPregap = CdrWinMsfToLba(matchPregap.Groups[1].Value);
                    }
                    else if(matchSongWriter.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_SONGWRITER_at_line_0, lineNumber);

                        if(inTrack)
                            currentTrack.Songwriter = matchSongWriter.Groups[1].Value;
                        else
                            _discImage.Songwriter = matchSongWriter.Groups[1].Value;
                    }
                    else if(matchTitle.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_TITLE_at_line_0, lineNumber);

                        if(inTrack)
                            currentTrack.Title = matchTitle.Groups[1].Value;
                        else
                            _discImage.Title = matchTitle.Groups[1].Value;
                    }
                    else if(matchTrack.Success)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_TRACK_at_line_0, lineNumber);

                        if(currentFile.DataFilter == null)
                        {
                            AaruConsole.ErrorWriteLine(string.Format(Localization
                                                                        .Found_TRACK_field_before_a_file_is_defined_at_line_0,
                                                                     lineNumber));

                            return ErrorNumber.InvalidArgument;
                        }

                        if(inTrack)
                        {
                            if(currentTrack.Indexes.ContainsKey(0) && currentTrack.Pregap == 0)
                                currentTrack.Indexes.TryGetValue(0, out currentTrack.Pregap);

                            currentFile.Sequence                 = currentTrack.Sequence;
                            currentTrack.TrackFile               = currentFile;
                            cueTracks[currentTrack.Sequence - 1] = currentTrack;
                        }

                        currentTrack = new CdrWinTrack
                        {
                            Indexes  = new SortedDictionary<ushort, int>(),
                            Sequence = uint.Parse(matchTrack.Groups[1].Value)
                        };

                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.Setting_currentTrack_sequence_to_0,
                                                   currentTrack.Sequence);

                        currentFile.Sequence   = currentTrack.Sequence;
                        currentTrack.Bps       = CdrWinTrackTypeToBytesPerSector(matchTrack.Groups[2].Value);
                        currentTrack.TrackType = matchTrack.Groups[2].Value.ToUpperInvariant();
                        currentTrack.Session   = currentSession;
                        inTrack                = true;
                    }
                    else if(line.Contains("INDEX 01 00:-2:00"))
                    {
                        AaruConsole.ErrorWriteLine(Localization
                                                      .This_image_from_PowerISO_is_damaged_beyond_possible_recovery_Will_not_open);

                        return ErrorNumber.InvalidArgument;
                    }
                    else if(line == "") // Empty line, ignore it
                    {}
                    else // Non-empty unknown field
                    {
                        AaruConsole.ErrorWriteLine(string.Format(Localization.Found_unknown_field_defined_at_line_0_1,
                                                                 lineNumber,
                                                                 line));

                        return ErrorNumber.NotSupported;
                    }
                }
            }

            if(currentTrack.Sequence != 0)
            {
                currentFile.Sequence   = currentTrack.Sequence;
                currentTrack.TrackFile = currentFile;

                _negativeEnd = currentFile.DataFilter.Length - (long)currentFile.Offset < 0;

                currentTrack.Sectors = ((ulong)currentFile.DataFilter.Length - currentFile.Offset) /
                                       CdrWinTrackTypeToBytesPerSector(currentTrack.TrackType);

                cueTracks[currentTrack.Sequence - 1] = currentTrack;
            }

            if(_negativeEnd)
            {
                string firstFile = cueTracks[0].TrackFile.DataFilter.Path;

                if(cueTracks.Any(t => t.Session > 1 || t.TrackFile.DataFilter.Path != firstFile) ||
                   cueTracks[0].Indexes.ContainsKey(0))
                {
                    AaruConsole.ErrorWriteLine(Localization
                                                  .The_data_files_are_not_correct_according_to_the_cuesheet_file_cannot_continue_with_this_file);

                    return ErrorNumber.InvalidArgument;
                }

                cueTracks[0].Pregap = cueTracks[0].Indexes[1];
                int reduceOffset = cueTracks[0].Pregap * cueTracks[0].Bps;

                foreach(CdrWinTrack track in cueTracks) track.TrackFile.Offset -= (ulong)reduceOffset;

                if(currentFile.DataFilter.Length - (long)cueTracks[currentTrack.Sequence - 1].TrackFile.Offset < 0)
                {
                    AaruConsole.ErrorWriteLine(Localization
                                                  .The_data_files_are_not_correct_according_to_the_cuesheet_file_cannot_continue_with_this_file);

                    return ErrorNumber.InvalidArgument;
                }

                cueTracks[currentTrack.Sequence - 1].Sectors =
                    ((ulong)currentFile.DataFilter.Length - cueTracks[currentTrack.Sequence - 1].TrackFile.Offset) /
                    cueTracks[currentTrack.Sequence - 1].Bps;

                cueTracks[0].Indexes[0] =  0;
                _lostPregap             =  (uint)cueTracks[0].Pregap;
                cueTracks[0].Sectors    += _lostPregap;

                AaruConsole.ErrorWriteLine(Localization
                                              .The_data_files_are_missing_a_pregap_or_hidden_track_contents_will_do_best_effort_to_make_the_rest_of_the_image_readable);
            }

            var sessions = new Session[currentSession];

            for(var s = 1; s <= sessions.Length; s++)
            {
                var firstTrackRead = false;
                sessions[s - 1].Sequence = (ushort)s;

                ulong sessionSectors   = 0;
                var   lastSessionTrack = 0;
                var   firstSessionTrk  = 0;

                for(var i = 0; i < cueTracks.Length; i++)
                {
                    if(cueTracks[i].Session != s) continue;

                    if(!firstTrackRead)
                    {
                        firstSessionTrk = i;
                        firstTrackRead  = true;
                    }

                    sessionSectors += cueTracks[i].Sectors;

                    if(i > lastSessionTrack) lastSessionTrack = i;
                }

                if(s > 1)
                {
                    if(_discImage.IsRedumpGigadisc)
                        sessions[s - 1].StartSector = gdRomSession2Offset;
                    else
                        sessions[s - 1].StartSector = sessions[s - 2].EndSector + 1;
                }
                else
                    sessions[s - 1].StartSector = 0;

                sessions[s - 1].StartTrack = cueTracks[firstSessionTrk].Sequence;
                sessions[s - 1].EndTrack   = cueTracks[lastSessionTrack].Sequence;

                if(leadouts.TryGetValue((byte)s, out int leadout))
                {
                    sessions[s - 1].EndSector = (ulong)(leadout - 1);

                    if(!cueTracks[lastSessionTrack].Indexes.TryGetValue(0, out int startSector))
                        cueTracks[lastSessionTrack].Indexes.TryGetValue(1, out startSector);

                    cueTracks[lastSessionTrack].Sectors = (ulong)(leadout - startSector);
                }
                else
                    sessions[s - 1].EndSector = sessions[s - 1].StartSector + sessionSectors - 1;

                CdrWinTrack firstSessionTrack = cueTracks.OrderBy(t => t.Sequence).First(t => t.Session == s);

                firstSessionTrack.Indexes.TryGetValue(0, out firstSessionTrack.Pregap);

                if(firstSessionTrack.Pregap < 150) firstSessionTrack.Pregap = 150;

                if(cueTracks.Any(i => i.TrackFile.DataFilter.Filename !=
                                      cueTracks.First().TrackFile.DataFilter.Filename))
                    continue;

                if(firstSessionTrack.Indexes.TryGetValue(0, out int sessionStart))
                {
                    sessions[s - 1].StartSector = (ulong)sessionStart;

                    continue;
                }

                if(firstSessionTrack.Indexes.TryGetValue(1, out sessionStart))
                    sessions[s - 1].StartSector = (ulong)sessionStart;
            }

            for(var t = 1; t <= cueTracks.Length; t++)
            {
                if(cueTracks[t - 1].Indexes.TryGetValue(0, out int idx0) &&
                   cueTracks[t - 1].Indexes.TryGetValue(1, out int idx1))
                    cueTracks[t - 1].Pregap = idx1 - idx0;

                _discImage.Tracks.Add(cueTracks[t - 1]);
            }

            // MagicISO writes 2048 bytes per data, sets Cuesheet as 2352, and fill the images with empty data.
            if(_discImage.Tracks.Count == 1 && _discImage.Tracks[0].TrackType == "MODE1/2352")
            {
                Stream track1Stream = _discImage.Tracks[0].TrackFile.DataFilter.GetDataForkStream();

                var foundSync = true;

                var rnd = new Random();

                // We check 32 random positions, to prevent coincidence of data
                for(var i = 0; i < 32; i++)
                {
                    int next = rnd.Next(cueTracks[^1].Indexes[1]);

                    track1Stream.Position = next * 2352;
                    var data = new byte[16];
                    track1Stream.EnsureRead(data, 0, 16);

                    // If the position is not MODEx/2352, it can't be a correct Cuesheet.
                    if(data[0x000] == 0x00 &&
                       data[0x001] == 0xFF &&
                       data[0x002] == 0xFF &&
                       data[0x003] == 0xFF &&
                       data[0x004] == 0xFF &&
                       data[0x005] == 0xFF &&
                       data[0x006] == 0xFF &&
                       data[0x007] == 0xFF &&
                       data[0x008] == 0xFF &&
                       data[0x009] == 0xFF &&
                       data[0x00A] == 0xFF &&
                       data[0x00B] == 0x00)
                        continue;

                    foundSync = false;

                    break;
                }

                if(!foundSync)
                {
                    _discImage.Tracks[0].Pregap    = 0;
                    _discImage.Tracks[0].Bps       = 2048;
                    _discImage.Tracks[0].TrackType = CDRWIN_TRACK_TYPE_MODE1;
                    _imageInfo.Application         = "MagicISO";
                    _discImage.MediaType           = MediaType.DVDROM;

                    AaruConsole.ErrorWriteLine(Localization
                                                  .This_image_is_most_probably_corrupted_beyond_repair_It_is_highly_recommended_to_dump_it_with_another_software);
                }
            }

            if(!string.IsNullOrWhiteSpace(_discImage.AaruMediaType) &&
               Enum.TryParse(_discImage.AaruMediaType, true, out MediaType mediaType))
                _discImage.MediaType = mediaType;
            else if(_discImage.IsRedumpGigadisc)
                _discImage.MediaType = MediaType.GDROM;
            else if(_discImage.MediaType == MediaType.Unknown)
                _discImage.MediaType = CdrWinIsoBusterDiscTypeToMediaType(_discImage.OriginalMediaType);

            if(_discImage.MediaType is MediaType.Unknown or MediaType.CD)
            {
                var data       = false;
                var cdg        = false;
                var cdi        = false;
                var mode2      = false;
                var firstAudio = false;
                var firstData  = false;
                var audio      = false;

                for(var i = 0; i < _discImage.Tracks.Count; i++)
                {
                    // First track is audio
                    firstAudio |= i == 0 && _discImage.Tracks[i].TrackType == CDRWIN_TRACK_TYPE_AUDIO;

                    // First track is data
                    firstData |= i == 0 && _discImage.Tracks[i].TrackType != CDRWIN_TRACK_TYPE_AUDIO;

                    // Any non first track is data
                    data |= i != 0 && _discImage.Tracks[i].TrackType != CDRWIN_TRACK_TYPE_AUDIO;

                    // Any non first track is audio
                    audio |= i != 0 && _discImage.Tracks[i].TrackType == CDRWIN_TRACK_TYPE_AUDIO;

                    switch(_discImage.Tracks[i].TrackType)
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

                if(!data && !firstData)
                    _discImage.MediaType = MediaType.CDDA;
                else if(cdg)
                    _discImage.MediaType = MediaType.CDG;
                else if(cdi)
                    _discImage.MediaType = MediaType.CDI;
                else if(firstAudio && data && sessions.Length > 1 && mode2)
                    _discImage.MediaType = MediaType.CDPLUS;
                else if(firstData && audio || mode2)
                    _discImage.MediaType = MediaType.CDROMXA;
                else if(!audio)
                    _discImage.MediaType = MediaType.CDROM;
                else
                    _discImage.MediaType = MediaType.CD;
            }

            // DEBUG information
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Disc_image_parsing_results);
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Disc_CD_TEXT);

            if(_discImage.Arranger == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Arranger_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Arranger_0, _discImage.Arranger);

            if(_discImage.Composer == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Composer_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Composer_0, _discImage.Composer);

            if(_discImage.Genre == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Genre_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Genre_0, _discImage.Genre);

            if(_discImage.Performer == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Performer_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Performer_0, _discImage.Performer);

            if(_discImage.Songwriter == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Songwriter_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Songwriter_0, _discImage.Songwriter);

            if(_discImage.Title == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Title_is_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Title_0, _discImage.Title);

            if(_discImage.CdTextFile == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.CD_TEXT_binary_file_not_set);
            else
            {
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t" + Localization.CD_TEXT_binary_file_0,
                                           _discImage.CdTextFile);
            }

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Disc_information);

            if(_discImage.OriginalMediaType == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.ISOBuster_disc_type_not_set);
            else
            {
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t" + Localization.ISOBuster_disc_type_0,
                                           _discImage.OriginalMediaType);
            }

            AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Guessed_disk_type_0, _discImage.MediaType);

            if(_discImage.Barcode == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Barcode_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Barcode_0, _discImage.Barcode);

            if(_discImage.DiscId == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Disc_ID_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Disc_ID_0, _discImage.DiscId);

            if(_discImage.Mcn == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.MCN_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.MCN_0, _discImage.Mcn);

            if(_discImage.Comment == null)
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Comment_not_set);
            else
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Comment_0, _discImage.Comment);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Track_information);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "\t" + Localization.Disc_contains_0_tracks,
                                       _discImage.Tracks.Count);

            foreach(CdrWinTrack t in _discImage.Tracks)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Track_0_information, t.Sequence);

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization._0_bytes_per_sector, t.Bps);
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Pregap_0_sectors,    t.Pregap);
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Data_0_sectors,      t.Sectors);

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Postgap_0_sectors, t.Postgap);

                if(t.Flag4Ch)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Track_is_flagged_as_quadraphonic);

                if(t.FlagDcp) AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Track_allows_digital_copy);

                if(t.FlagPre)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Track_has_pre_emphasis_applied);

                if(t.FlagScms) AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Track_has_SCMS);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" +
                                           Localization.Track_resides_in_file_0_type_defined_as_1_starting_at_byte_2,
                                           t.TrackFile.DataFilter.Filename,
                                           t.TrackFile.FileType,
                                           t.TrackFile.Offset);

                AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Indexes);

                foreach(KeyValuePair<ushort, int> kvp in t.Indexes)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               "\t\t\t" + Localization.Index_0_starts_at_sector_1,
                                               kvp.Key,
                                               kvp.Value);
                }

                if(t.Isrc == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.ISRC_is_not_set);
                else
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.ISRC_0, t.Isrc);

                if(t.Arranger == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Arranger_is_not_set);
                else
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Arranger_0, t.Arranger);

                if(t.Composer == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Composer_is_not_set);
                else
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Composer_0, t.Composer);

                if(t.Genre == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Genre_is_not_set);
                else
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Genre_0, t.Genre);

                if(t.Performer == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Performer_is_not_set);
                else
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Performer_0, t.Performer);

                if(t.Songwriter == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Songwriter_is_not_set);
                else
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Songwriter_0, t.Songwriter);

                if(t.Title == null)
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Title_is_not_set);
                else
                    AaruConsole.DebugWriteLine(MODULE_NAME, "\t\t" + Localization.Title_0, t.Title);
            }

            foreach(CdrWinTrack track in _discImage.Tracks) _imageInfo.ImageSize += track.Bps * track.Sectors;

            var currentSector          = 0;
            var currentFileStartSector = 0;
            var currentFilePath        = "";
            firstTrackInSession = true;

            foreach(CdrWinTrack track in _discImage.Tracks)
            {
                if(track.TrackFile.DataFilter.BasePath != currentFilePath)
                {
                    if(_discImage.IsRedumpGigadisc && track.Session == 2 && firstTrackInSession)
                    {
                        currentSector = (int)gdRomSession2Offset - track.Pregap;
                        track.Indexes.Add(0, 0);
                        track.Indexes[1]    =  track.Pregap;
                        track.Sectors       += (ulong)track.Pregap;
                        firstTrackInSession =  false;
                    }

                    currentFileStartSector = currentSector;
                    currentFilePath        = track.TrackFile.DataFilter.BasePath;
                }

                SortedDictionary<ushort, int> newIndexes = new();

                foreach(KeyValuePair<ushort, int> index in track.Indexes)
                    newIndexes[index.Key] = index.Value + currentFileStartSector;

                track.Indexes = newIndexes;

                currentSector += (int)track.Sectors;
            }

            for(var s = 0; s < sessions.Length; s++)
            {
                if(!_discImage.Tracks[(int)sessions[s].StartTrack - 1]
                              .Indexes.TryGetValue(0, out int sessionTrackStart))
                    _discImage.Tracks[(int)sessions[s].StartTrack - 1].Indexes.TryGetValue(1, out sessionTrackStart);

                sessions[s].StartSector = (ulong)(sessionTrackStart > 0 ? sessionTrackStart : 0);

                if(!_discImage.Tracks[(int)sessions[s].EndTrack - 1].Indexes.TryGetValue(0, out sessionTrackStart))
                    _discImage.Tracks[(int)sessions[s].EndTrack - 1].Indexes.TryGetValue(1, out sessionTrackStart);

                sessions[s].EndSector =  (ulong)(sessionTrackStart > 0 ? sessionTrackStart : 0);
                sessions[s].EndSector += _discImage.Tracks[(int)sessions[s].EndTrack - 1].Sectors - 1;
            }

            for(var s = 1; s <= sessions.Length; s++) _discImage.Sessions.Add(sessions[s - 1]);

            _imageInfo.Sectors = _discImage.Sessions.MaxBy(s => s.EndSector).EndSector + 1;

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Session_information);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "\t" + Localization.Disc_contains_0_sessions,
                                       _discImage.Sessions.Count);

            for(var i = 0; i < _discImage.Sessions.Count; i++)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, "\t" + Localization.Session_0_information, i + 1);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization.Starting_track_0,
                                           _discImage.Sessions[i].StartTrack);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization.Starting_sector_0,
                                           _discImage.Sessions[i].StartSector);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization.Ending_track_0,
                                           _discImage.Sessions[i].EndTrack);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "\t\t" + Localization.Ending_sector_0,
                                           _discImage.Sessions[i].EndSector);
            }

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Building_offset_map);

            Partitions = new List<Partition>();

            ulong partitionSequence = 0;

            _offsetMap = new Dictionary<uint, ulong>();

            for(var i = 0; i < _discImage.Tracks.Count; i++)
            {
                if(_discImage.Tracks[i].Sequence == 1 && i != 0)
                {
                    AaruConsole.ErrorWriteLine(Localization.Unordered_tracks);

                    return ErrorNumber.InvalidArgument;
                }

                var partition = new Partition();

                if(!_discImage.Tracks[i].Indexes.TryGetValue(1, out _))
                {
                    AaruConsole.ErrorWriteLine(string.Format(Localization.Track_0_lacks_index_01,
                                                             _discImage.Tracks[i].Sequence));

                    return ErrorNumber.InvalidArgument;
                }

                // Index 01
                partition.Description = string.Format(Localization.Track_0, _discImage.Tracks[i].Sequence);
                partition.Name        = _discImage.Tracks[i].Title;
                partition.Start       = (ulong)_discImage.Tracks[i].Indexes[1];
                partition.Size        = _discImage.Tracks[i].Sectors * _discImage.Tracks[i].Bps;
                partition.Length      = _discImage.Tracks[i].Sectors;
                partition.Sequence    = partitionSequence;
                partition.Offset      = partition.Start * 2352;
                partition.Type        = _discImage.Tracks[i].TrackType;

                partitionSequence++;

                if(_discImage.IsRedumpGigadisc && _discImage.Tracks[i].Sequence == 3)
                {
                    _offsetMap.Add(_discImage.Tracks[i].Sequence,
                                   gdRomSession2Offset - (ulong)_discImage.Tracks[i].Pregap);
                }
                else if(_discImage.Tracks[i].Indexes.TryGetValue(0, out int idx0))
                    _offsetMap.Add(_discImage.Tracks[i].Sequence, (ulong)idx0);
                else if(_discImage.Tracks[i].Sequence > 1)
                {
                    _offsetMap.Add(_discImage.Tracks[i].Sequence,
                                   (ulong)(_discImage.Tracks[i].Indexes[1] - _discImage.Tracks[i].Pregap));
                }
                else
                    _offsetMap.Add(_discImage.Tracks[i].Sequence, (ulong)_discImage.Tracks[i].Indexes[1]);

                Partitions.Add(partition);
            }

            // Print offset map
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

            if(_discImage.MediaType != MediaType.CDROMXA &&
               _discImage.MediaType != MediaType.CDDA    &&
               _discImage.MediaType != MediaType.CDI     &&
               _discImage.MediaType != MediaType.CDPLUS  &&
               _discImage.MediaType != MediaType.CDG     &&
               _discImage.MediaType != MediaType.CDEG    &&
               _discImage.MediaType != MediaType.CDMIDI  &&
               _discImage.MediaType != MediaType.GDROM)
                _imageInfo.SectorSize = 2048; // Only data tracks
            else
                _imageInfo.SectorSize = 2352; // All others

            if(_discImage.Mcn != null) _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_MCN);

            if(_discImage.CdTextFile != null) _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_TEXT);

            if(_imageInfo.Application is null)
            {
                if(_discImage.IsTrurip)
                    _imageInfo.Application = "trurip";

                else if(_discImage.IsRedumpGigadisc)
                    _imageInfo.Application = "Redump.org";

                // Detect ISOBuster extensions
                else if(_discImage.OriginalMediaType != null                                                  ||
                        _discImage.Comment.Contains("isobuster", StringComparison.InvariantCultureIgnoreCase) ||
                        sessions.Length > 1)
                    _imageInfo.Application = "ISOBuster";
                else
                    _imageInfo.Application = "CDRWin";
            }

            _imageInfo.CreationTime         = imageFilter.CreationTime;
            _imageInfo.LastModificationTime = imageFilter.LastWriteTime;

            _imageInfo.Comments          = _discImage.Comment;
            _imageInfo.MediaSerialNumber = _discImage.Mcn;
            _imageInfo.MediaBarcode      = _discImage.Barcode;
            _imageInfo.MediaType         = _discImage.MediaType;

            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

            foreach(CdrWinTrack track in _discImage.Tracks)
            {
                switch(track.TrackType)
                {
                    case CDRWIN_TRACK_TYPE_AUDIO:
                    {
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);

                        break;
                    }
                    case CDRWIN_TRACK_TYPE_CDG:
                    {
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdTrackIsrc))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackIsrc);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                        break;
                    }
                    case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
                    case CDRWIN_TRACK_TYPE_CDI:
                    {
                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                        if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                            _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                        break;
                    }
                    case CDRWIN_TRACK_TYPE_MODE2_RAW:
                    case CDRWIN_TRACK_TYPE_CDI_RAW:
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
                    case CDRWIN_TRACK_TYPE_MODE1_RAW:
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

            AaruConsole.VerboseWriteLine(Localization.CDRWIN_image_describes_a_disc_of_type_0, _imageInfo.MediaType);

            if(!string.IsNullOrEmpty(_imageInfo.Comments))
                AaruConsole.VerboseWriteLine(Localization.CDRWIN_comments_0, _imageInfo.Comments);

            _sectorBuilder = new SectorBuilder();

            var mediaTypeAsInt = (int)_discImage.MediaType;

            _isCd = mediaTypeAsInt is >= 10 and <= 39
                                   or 112
                                   or 113
                                   or >= 150 and <= 152
                                   or 154
                                   or 155
                                   or >= 171 and <= 179
                                   or >= 740 and <= 749;

            if(currentSession > 1 && leadouts.Count == 0 && !_discImage.IsRedumpGigadisc && _isCd)
            {
                AaruConsole.ErrorWriteLine(Localization
                                              .This_image_is_missing_vital_multi_session_data_and_cannot_be_read_correctly);

                return ErrorNumber.InvalidArgument;
            }

            if(_discImage.Tracks.All(t => t.Isrc == null))
                _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdTrackIsrc);

            if(_isCd) return ErrorNumber.NoError;

            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorSync);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorHeader);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorSubHeader);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorEcc);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorEccP);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorEccQ);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdSectorEdc);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdTrackFlags);
            _imageInfo.ReadableSectorTags.Remove(SectorTagType.CdTrackIsrc);

            sessions = _discImage.Sessions.ToArray();

            foreach(CdrWinTrack track in _discImage.Tracks)
            {
                track.Indexes.Remove(0);
                track.Pregap = 0;

                for(var s = 0; s < sessions.Length; s++)
                {
                    if(sessions[s].Sequence <= 1 || track.Sequence != sessions[s].StartTrack) continue;

                    track.TrackFile.Offset  += 307200;
                    track.Sectors           -= 150;
                    sessions[s].StartSector =  (ulong)track.Indexes[1];
                }
            }

            _discImage.Sessions = sessions.ToList();

            return ErrorNumber.NoError;
        }
        catch(Exception ex)
        {
            AaruConsole.ErrorWriteLine(Localization.Exception_trying_to_identify_image_file_0, imageFilter.Filename);
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
                if(_discImage.Mcn == null) return ErrorNumber.NoData;

                buffer = Encoding.ASCII.GetBytes(_discImage.Mcn);

                return ErrorNumber.NoError;
            }
            case MediaTagType.CD_TEXT:
            {
                // TODO: Check binary text file exists, open it, read it, send it to caller.
                return _discImage.CdTextFile != null ? ErrorNumber.NotImplemented : ErrorNumber.NoData;
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

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from cdrwinTrack in _discImage.Tracks
                                                 where cdrwinTrack.Sequence      == kvp.Key
                                                 where sectorAddress - kvp.Value < cdrwinTrack.Sectors
                                                 select kvp)
            return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag, out byte[] buffer)
    {
        buffer = null;

        if(tag is SectorTagType.CdTrackFlags or SectorTagType.CdTrackIsrc)
            return ReadSectorsTag(sectorAddress, length, 0, tag, out buffer);

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from cdrwinTrack in _discImage.Tracks
                                                 where cdrwinTrack.Sequence      == kvp.Key
                                                 where sectorAddress - kvp.Value < cdrwinTrack.Sectors
                                                 select kvp)
            return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectors(ulong sectorAddress, uint length, uint track, out byte[] buffer)
    {
        buffer = null;
        CdrWinTrack aaruTrack = _discImage.Tracks.FirstOrDefault(cdrwinTrack => cdrwinTrack.Sequence == track);

        if(aaruTrack is null) return ErrorNumber.SectorNotFound;

        if(length > aaruTrack.Sectors) return ErrorNumber.OutOfRange;

        uint sectorOffset;
        uint sectorSize;
        uint sectorSkip;
        var  mode2 = false;

        switch(aaruTrack.TrackType)
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
                mode2        = true;
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
            case CDRWIN_TRACK_TYPE_CDI_RAW:
            {
                mode2        = true;
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
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        // If it's the lost pregap
        if(track == 1 && _lostPregap > 0)
        {
            if(sectorAddress < _lostPregap)
            {
                // If we need to mix lost with present data
                if(sectorAddress + length <= _lostPregap) return ErrorNumber.NoError;

                ulong pregapPos = _lostPregap - sectorAddress;

                ErrorNumber errno = ReadSectors(_lostPregap, (uint)(length - pregapPos), track, out byte[] presentData);

                if(errno != ErrorNumber.NoError) return errno;

                Array.Copy(presentData, 0, buffer, (long)(pregapPos * sectorSize), presentData.Length);

                return ErrorNumber.NoError;
            }

            sectorAddress -= _lostPregap;
        }

        if(_discImage.IsRedumpGigadisc                      &&
           aaruTrack.Session    == 2                        &&
           aaruTrack.Indexes[0] >= 45000 - aaruTrack.Pregap &&
           aaruTrack.Indexes[1] <= 45000)
        {
            if(sectorAddress < (ulong)(aaruTrack.Indexes[1] - aaruTrack.Indexes[0]))
            {
                if(sectorAddress + length + (ulong)aaruTrack.Indexes[0] <= (ulong)aaruTrack.Indexes[1])
                    return ErrorNumber.NoError;

                ulong pregapPos = (ulong)(aaruTrack.Indexes[1] - aaruTrack.Indexes[0]) - sectorAddress;

                ErrorNumber errno = ReadSectors((ulong)(aaruTrack.Indexes[1] - aaruTrack.Indexes[0]),
                                                (uint)(length                - pregapPos),
                                                track,
                                                out byte[] presentData);

                if(errno != ErrorNumber.NoError) return errno;

                Array.Copy(presentData, 0, buffer, (long)(pregapPos * sectorSize), presentData.Length);

                return ErrorNumber.NoError;
            }

            sectorAddress -= (ulong)(aaruTrack.Indexes[1] - aaruTrack.Indexes[0]);
        }

        _imageStream = aaruTrack.TrackFile.DataFilter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.Seek((long)aaruTrack.TrackFile.Offset +
                           (long)(sectorAddress * (sectorOffset + sectorSize + sectorSkip)),
                           SeekOrigin.Begin);

        if(mode2)
        {
            var mode2Ms = new MemoryStream((int)(sectorSize * length));

            buffer = br.ReadBytes((int)(sectorSize * length));

            for(var i = 0; i < length; i++)
            {
                var sector = new byte[sectorSize];
                Array.Copy(buffer, sectorSize * i, sector, 0, sectorSize);
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

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsTag(ulong      sectorAddress, uint length, uint track, SectorTagType tag,
                                      out byte[] buffer)
    {
        buffer = null;

        if(tag is SectorTagType.CdTrackFlags or SectorTagType.CdTrackIsrc) track = (uint)sectorAddress;

        CdrWinTrack aaruTrack = _discImage.Tracks.FirstOrDefault(cdrwinTrack => cdrwinTrack.Sequence == track);

        if(aaruTrack is null) return ErrorNumber.SectorNotFound;

        if(length > aaruTrack.Sectors) return ErrorNumber.OutOfRange;

        uint sectorOffset = 0;
        uint sectorSize   = 0;
        uint sectorSkip   = 0;

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

                if(aaruTrack.TrackType != CDRWIN_TRACK_TYPE_AUDIO && aaruTrack.TrackType != CDRWIN_TRACK_TYPE_CDG)
                    flags |= CdFlags.DataTrack;

                if(aaruTrack.FlagDcp) flags |= CdFlags.CopyPermitted;

                if(aaruTrack.FlagPre) flags |= CdFlags.PreEmphasis;

                if(aaruTrack.Flag4Ch) flags |= CdFlags.FourChannel;

                buffer = new[]
                {
                    (byte)flags
                };

                return ErrorNumber.NoError;
            }
            case SectorTagType.CdTrackIsrc:
                if(aaruTrack.Isrc == null) return ErrorNumber.NoData;

                buffer = Encoding.UTF8.GetBytes(aaruTrack.Isrc);

                return ErrorNumber.NoError;
            case SectorTagType.CdTrackText:
                return ErrorNumber.NotImplemented;
            default:
                return ErrorNumber.NotSupported;
        }

        switch(aaruTrack.TrackType)
        {
            case CDRWIN_TRACK_TYPE_MODE1:
            case CDRWIN_TRACK_TYPE_MODE2_FORM1:
            case CDRWIN_TRACK_TYPE_MODE2_FORM2:
                if(tag != SectorTagType.CdSectorSubchannel                                   ||
                   !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel) ||
                   _discImage.Tracks.All(t => t.TrackType != CDRWIN_TRACK_TYPE_CDG))
                    return ErrorNumber.NoData;

                buffer = new byte[length * 96];

                return ErrorNumber.NoError;

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
                        if(tag != SectorTagType.CdSectorSubchannel                                   ||
                           !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel) ||
                           _discImage.Tracks.All(t => t.TrackType != CDRWIN_TRACK_TYPE_CDG))
                            return ErrorNumber.NotSupported;

                        buffer = new byte[length * 96];

                        return ErrorNumber.NoError;

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
                }

                break;
            }
            case CDRWIN_TRACK_TYPE_AUDIO:
                return ErrorNumber.NoData;
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
                        if(tag != SectorTagType.CdSectorSubchannel                                   ||
                           !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel) ||
                           _discImage.Tracks.All(t => t.TrackType != CDRWIN_TRACK_TYPE_CDG))
                            return ErrorNumber.NotSupported;

                        buffer = new byte[length * 96];

                        return ErrorNumber.NoError;

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
            case CDRWIN_TRACK_TYPE_MODE2_RAW: // Requires reading sector
            case CDRWIN_TRACK_TYPE_CDI_RAW:
                return ErrorNumber.NotImplemented;
            case CDRWIN_TRACK_TYPE_CDG:
            {
                if(tag != SectorTagType.CdSectorSubchannel) return ErrorNumber.NotSupported;

                sectorOffset = 2352;
                sectorSize   = 96;
                sectorSkip   = 0;

                break;
            }
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        // If it's the lost pregap
        if(track == 1 && _lostPregap > 0)
        {
            if(sectorAddress < _lostPregap)
            {
                // If we need to mix lost with present data
                if(sectorAddress + length <= _lostPregap) return ErrorNumber.NoError;

                ulong pregapPos = _lostPregap - sectorAddress;

                ErrorNumber errno = ReadSectors(_lostPregap, (uint)(length - pregapPos), track, out byte[] presentData);

                if(errno != ErrorNumber.NoError) return errno;

                Array.Copy(presentData, 0, buffer, (long)(pregapPos * sectorSize), presentData.Length);

                return ErrorNumber.NoError;
            }

            sectorAddress -= _lostPregap;
        }

        _imageStream = aaruTrack.TrackFile.DataFilter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.Seek((long)aaruTrack.TrackFile.Offset +
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

        foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap
                                                 where sectorAddress >= kvp.Value
                                                 from cdrwinTrack in _discImage.Tracks
                                                 where cdrwinTrack.Sequence      == kvp.Key
                                                 where sectorAddress - kvp.Value < cdrwinTrack.Sectors
                                                 select kvp)
            return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key, out buffer);

        return ErrorNumber.SectorNotFound;
    }

    /// <inheritdoc />
    public ErrorNumber ReadSectorsLong(ulong sectorAddress, uint length, uint track, out byte[] buffer)
    {
        buffer = null;

        if(!_isCd) return ReadSectors(sectorAddress, length, track, out buffer);

        CdrWinTrack aaruTrack = _discImage.Tracks.FirstOrDefault(cdrwinTrack => cdrwinTrack.Sequence == track);

        if(aaruTrack is null) return ErrorNumber.SectorNotFound;

        if(length > aaruTrack.Sectors) return ErrorNumber.OutOfRange;

        uint sectorOffset;
        uint sectorSize;
        uint sectorSkip;

        switch(aaruTrack.TrackType)
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
            default:
                return ErrorNumber.NotSupported;
        }

        buffer = new byte[sectorSize * length];

        // If it's the lost pregap
        if(track == 1 && _lostPregap > 0)
        {
            if(sectorAddress < _lostPregap)
            {
                // If we need to mix lost with present data
                if(sectorAddress + length <= _lostPregap) return ErrorNumber.NoError;

                ulong pregapPos = _lostPregap - sectorAddress;

                ErrorNumber errno = ReadSectors(_lostPregap, (uint)(length - pregapPos), track, out byte[] presentData);

                if(errno != ErrorNumber.NoError) return errno;

                Array.Copy(presentData, 0, buffer, (long)(pregapPos * sectorSize), presentData.Length);

                return ErrorNumber.NoError;
            }

            sectorAddress -= _lostPregap;
        }

        if(_discImage.IsRedumpGigadisc                      &&
           aaruTrack.Session    == 2                        &&
           aaruTrack.Indexes[0] >= 45000 - aaruTrack.Pregap &&
           aaruTrack.Indexes[1] <= 45000)
        {
            if(sectorAddress < (ulong)(aaruTrack.Indexes[1] - aaruTrack.Indexes[0]))
            {
                if(sectorAddress + length + (ulong)aaruTrack.Indexes[0] <= (ulong)aaruTrack.Indexes[1])
                    return ErrorNumber.NoError;

                ulong pregapPos = (ulong)(aaruTrack.Indexes[1] - aaruTrack.Indexes[0]) - sectorAddress;

                ErrorNumber errno = ReadSectorsLong((ulong)(aaruTrack.Indexes[1] - aaruTrack.Indexes[0]),
                                                    (uint)(length                - pregapPos),
                                                    track,
                                                    out byte[] presentData);

                if(errno != ErrorNumber.NoError) return errno;

                Array.Copy(presentData, 0, buffer, (long)(pregapPos * sectorSize), presentData.Length);

                return ErrorNumber.NoError;
            }

            sectorAddress -= (ulong)(aaruTrack.Indexes[1] - aaruTrack.Indexes[0]);
        }

        _imageStream = aaruTrack.TrackFile.DataFilter.GetDataForkStream();
        var br = new BinaryReader(_imageStream);

        br.BaseStream.Seek((long)aaruTrack.TrackFile.Offset + (long)(sectorAddress * (sectorSize + sectorSkip)),
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

        switch(aaruTrack.TrackType)
        {
            case CDRWIN_TRACK_TYPE_MODE1:
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
            case CDRWIN_TRACK_TYPE_MODE2_FORM1:
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
            case CDRWIN_TRACK_TYPE_MODE2_FORM2:
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
            case CDRWIN_TRACK_TYPE_MODE2_FORMLESS:
            case CDRWIN_TRACK_TYPE_CDI:
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
        }

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public List<Track> GetSessionTracks(Session session) =>
        _discImage.Sessions.Contains(session) ? GetSessionTracks(session.Sequence) : null;

    /// <inheritdoc />
    public List<Track> GetSessionTracks(ushort session) =>
        Tracks.Where(t => t.Session == session).OrderBy(t => t.Sequence).ToList();

#endregion
}