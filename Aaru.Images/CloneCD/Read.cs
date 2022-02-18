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
//     Reads CloneCD disc images.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed partial class CloneCd
    {
        /// <inheritdoc />
        public bool Open(IFilter imageFilter)
        {
            if(imageFilter == null)
                return false;

            _ccdFilter = imageFilter;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                _cueStream = new StreamReader(imageFilter.GetDataForkStream());
                int lineNumber = 0;

                var ccdIdRegex     = new Regex(CCD_IDENTIFIER);
                var discIdRegex    = new Regex(DISC_IDENTIFIER);
                var sessIdRegex    = new Regex(SESSION_IDENTIFIER);
                var entryIdRegex   = new Regex(ENTRY_IDENTIFIER);
                var trackIdRegex   = new Regex(TRACK_IDENTIFIER);
                var cdtIdRegex     = new Regex(CDTEXT_IDENTIFIER);
                var ccdVerRegex    = new Regex(CCD_VERSION);
                var discEntRegex   = new Regex(DISC_ENTRIES);
                var discSessRegex  = new Regex(DISC_SESSIONS);
                var discScrRegex   = new Regex(DISC_SCRAMBLED);
                var cdtLenRegex    = new Regex(CDTEXT_LENGTH);
                var discCatRegex   = new Regex(DISC_CATALOG);
                var sessPregRegex  = new Regex(SESSION_PREGAP);
                var sessSubcRegex  = new Regex(SESSION_SUBCHANNEL);
                var entSessRegex   = new Regex(ENTRY_SESSION);
                var entPointRegex  = new Regex(ENTRY_POINT);
                var entAdrRegex    = new Regex(ENTRY_ADR);
                var entCtrlRegex   = new Regex(ENTRY_CONTROL);
                var entTnoRegex    = new Regex(ENTRY_TRACKNO);
                var entAMinRegex   = new Regex(ENTRY_AMIN);
                var entASecRegex   = new Regex(ENTRY_ASEC);
                var entAFrameRegex = new Regex(ENTRY_AFRAME);
                var entAlbaRegex   = new Regex(ENTRY_ALBA);
                var entZeroRegex   = new Regex(ENTRY_ZERO);
                var entPMinRegex   = new Regex(ENTRY_PMIN);
                var entPSecRegex   = new Regex(ENTRY_PSEC);
                var entPFrameRegex = new Regex(ENTRY_PFRAME);
                var entPlbaRegex   = new Regex(ENTRY_PLBA);
                var cdtEntsRegex   = new Regex(CDTEXT_ENTRIES);
                var cdtEntRegex    = new Regex(CDTEXT_ENTRY);
                var trkModeRegex   = new Regex(TRACK_MODE);
                var trkIndexRegex  = new Regex(TRACK_INDEX);

                bool                                    inCcd = false;
                bool                                    inDisk = false;
                bool                                    inSession = false;
                bool                                    inEntry = false;
                bool                                    inTrack = false;
                bool                                    inCdText = false;
                var                                     cdtMs = new MemoryStream();
                int                                     minSession = int.MaxValue;
                int                                     maxSession = int.MinValue;
                var                                     currentEntry = new FullTOC.TrackDataDescriptor();
                byte                                    currentTrackEntry = 0;
                Dictionary<byte, int>                   trackModes = new Dictionary<byte, int>();
                Dictionary<byte, Dictionary<byte, int>> trackIndexes = new Dictionary<byte, Dictionary<byte, int>>();
                List<FullTOC.TrackDataDescriptor>       entries = new List<FullTOC.TrackDataDescriptor>();
                _scrambled = false;
                _catalog   = null;

                while(_cueStream.Peek() >= 0)
                {
                    lineNumber++;
                    string line = _cueStream.ReadLine();

                    Match ccdIdMatch   = ccdIdRegex.Match(line);
                    Match discIdMatch  = discIdRegex.Match(line);
                    Match sessIdMatch  = sessIdRegex.Match(line);
                    Match entryIdMatch = entryIdRegex.Match(line);
                    Match trackIdMatch = trackIdRegex.Match(line);
                    Match cdtIdMatch   = cdtIdRegex.Match(line);

                    // [CloneCD]
                    if(ccdIdMatch.Success)
                    {
                        if(inDisk    ||
                           inSession ||
                           inEntry   ||
                           inTrack   ||
                           inCdText)
                            throw new
                                FeatureUnsupportedImageException($"Found [CloneCD] out of order in line {lineNumber}");

                        inCcd     = true;
                        inDisk    = false;
                        inSession = false;
                        inEntry   = false;
                        inTrack   = false;
                        inCdText  = false;
                    }
                    else if(discIdMatch.Success  ||
                            sessIdMatch.Success  ||
                            entryIdMatch.Success ||
                            trackIdMatch.Success ||
                            cdtIdMatch.Success)
                    {
                        if(inEntry)
                        {
                            entries.Add(currentEntry);
                            currentEntry = new FullTOC.TrackDataDescriptor();
                        }

                        inCcd     = false;
                        inDisk    = discIdMatch.Success;
                        inSession = sessIdMatch.Success;
                        inEntry   = entryIdMatch.Success;
                        inTrack   = trackIdMatch.Success;
                        inCdText  = cdtIdMatch.Success;

                        if(inTrack)
                            currentTrackEntry = Convert.ToByte(trackIdMatch.Groups["number"].Value, 10);
                    }
                    else
                    {
                        if(inCcd)
                        {
                            Match ccdVerMatch = ccdVerRegex.Match(line);

                            if(!ccdVerMatch.Success)
                                continue;

                            AaruConsole.DebugWriteLine("CloneCD plugin", "Found Version at line {0}", lineNumber);

                            _imageInfo.Version = ccdVerMatch.Groups["value"].Value;

                            if(_imageInfo.Version != "2" &&
                               _imageInfo.Version != "3")
                                AaruConsole.
                                    ErrorWriteLine("(CloneCD plugin): Warning! Unknown CCD image version {0}, may not work!",
                                                   _imageInfo.Version);
                        }
                        else if(inDisk)
                        {
                            Match discEntMatch  = discEntRegex.Match(line);
                            Match discSessMatch = discSessRegex.Match(line);
                            Match discScrMatch  = discScrRegex.Match(line);
                            Match cdtLenMatch   = cdtLenRegex.Match(line);
                            Match discCatMatch  = discCatRegex.Match(line);

                            if(discEntMatch.Success)
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found TocEntries at line {0}",
                                                           lineNumber);
                            else if(discSessMatch.Success)
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found Sessions at line {0}", lineNumber);
                            else if(discScrMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found DataTracksScrambled at line {0}",
                                                           lineNumber);

                                _scrambled |= discScrMatch.Groups["value"].Value == "1";
                            }
                            else if(cdtLenMatch.Success)
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found CDTextLength at line {0}",
                                                           lineNumber);
                            else if(discCatMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found Catalog at line {0}", lineNumber);
                                _catalog = discCatMatch.Groups["value"].Value;
                            }
                        }

                        // TODO: Do not suppose here entries come sorted
                        else if(inCdText)
                        {
                            Match cdtEntsMatch = cdtEntsRegex.Match(line);
                            Match cdtEntMatch  = cdtEntRegex.Match(line);

                            if(cdtEntsMatch.Success)
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found CD-Text Entries at line {0}",
                                                           lineNumber);
                            else if(cdtEntMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found CD-Text Entry at line {0}",
                                                           lineNumber);

                                string[] bytes = cdtEntMatch.Groups["value"].Value.Split(new[]
                                {
                                    ' '
                                }, StringSplitOptions.RemoveEmptyEntries);

                                foreach(string byt in bytes)
                                    cdtMs.WriteByte(Convert.ToByte(byt, 16));
                            }
                        }

                        // Is this useful?
                        else if(inSession)
                        {
                            Match sessPregMatch = sessPregRegex.Match(line);
                            Match sessSubcMatch = sessSubcRegex.Match(line);

                            if(sessPregMatch.Success)
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found PreGapMode at line {0}",
                                                           lineNumber);
                            else if(sessSubcMatch.Success)
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found PreGapSubC at line {0}",
                                                           lineNumber);
                        }
                        else if(inEntry)
                        {
                            Match entSessMatch   = entSessRegex.Match(line);
                            Match entPointMatch  = entPointRegex.Match(line);
                            Match entAdrMatch    = entAdrRegex.Match(line);
                            Match entCtrlMatch   = entCtrlRegex.Match(line);
                            Match entTnoMatch    = entTnoRegex.Match(line);
                            Match entAMinMatch   = entAMinRegex.Match(line);
                            Match entASecMatch   = entASecRegex.Match(line);
                            Match entAFrameMatch = entAFrameRegex.Match(line);
                            Match entAlbaMatch   = entAlbaRegex.Match(line);
                            Match entZeroMatch   = entZeroRegex.Match(line);
                            Match entPMinMatch   = entPMinRegex.Match(line);
                            Match entPSecMatch   = entPSecRegex.Match(line);
                            Match entPFrameMatch = entPFrameRegex.Match(line);
                            Match entPlbaMatch   = entPlbaRegex.Match(line);

                            if(entSessMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found Session at line {0}", lineNumber);
                                currentEntry.SessionNumber = Convert.ToByte(entSessMatch.Groups["value"].Value, 10);

                                if(currentEntry.SessionNumber < minSession)
                                    minSession = currentEntry.SessionNumber;

                                if(currentEntry.SessionNumber > maxSession)
                                    maxSession = currentEntry.SessionNumber;
                            }
                            else if(entPointMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found Point at line {0}", lineNumber);
                                currentEntry.POINT = Convert.ToByte(entPointMatch.Groups["value"].Value, 16);
                            }
                            else if(entAdrMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found ADR at line {0}", lineNumber);
                                currentEntry.ADR = Convert.ToByte(entAdrMatch.Groups["value"].Value, 16);
                            }
                            else if(entCtrlMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found Control at line {0}", lineNumber);
                                currentEntry.CONTROL = Convert.ToByte(entCtrlMatch.Groups["value"].Value, 16);
                            }
                            else if(entTnoMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found TrackNo at line {0}", lineNumber);
                                currentEntry.TNO = Convert.ToByte(entTnoMatch.Groups["value"].Value, 10);
                            }
                            else if(entAMinMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found AMin at line {0}", lineNumber);
                                currentEntry.Min = Convert.ToByte(entAMinMatch.Groups["value"].Value, 10);
                            }
                            else if(entASecMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found ASec at line {0}", lineNumber);
                                currentEntry.Sec = Convert.ToByte(entASecMatch.Groups["value"].Value, 10);
                            }
                            else if(entAFrameMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found AFrame at line {0}", lineNumber);
                                currentEntry.Frame = Convert.ToByte(entAFrameMatch.Groups["value"].Value, 10);
                            }
                            else if(entAlbaMatch.Success)
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found ALBA at line {0}", lineNumber);
                            else if(entZeroMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found Zero at line {0}", lineNumber);
                                currentEntry.Zero  = Convert.ToByte(entZeroMatch.Groups["value"].Value, 10);
                                currentEntry.HOUR  = (byte)((currentEntry.Zero & 0xF0) >> 4);
                                currentEntry.PHOUR = (byte)(currentEntry.Zero & 0x0F);
                            }
                            else if(entPMinMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found PMin at line {0}", lineNumber);
                                currentEntry.PMIN = Convert.ToByte(entPMinMatch.Groups["value"].Value, 10);
                            }
                            else if(entPSecMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found PSec at line {0}", lineNumber);
                                currentEntry.PSEC = Convert.ToByte(entPSecMatch.Groups["value"].Value, 10);
                            }
                            else if(entPFrameMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found PFrame at line {0}", lineNumber);
                                currentEntry.PFRAME = Convert.ToByte(entPFrameMatch.Groups["value"].Value, 10);
                            }
                            else if(entPlbaMatch.Success)
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found PLBA at line {0}", lineNumber);
                        }
                        else if(inTrack)
                        {
                            Match trkModeMatch  = trkModeRegex.Match(line);
                            Match trkIndexMatch = trkIndexRegex.Match(line);

                            if(trkModeMatch.Success &&
                               currentTrackEntry > 0)
                                trackModes[currentTrackEntry] = Convert.ToByte(trkModeMatch.Groups["value"].Value, 10);
                            else if(trkIndexMatch.Success &&
                                    currentTrackEntry > 0)
                            {
                                byte indexNo  = Convert.ToByte(trkIndexMatch.Groups["index"].Value, 10);
                                int  indexLba = Convert.ToInt32(trkIndexMatch.Groups["lba"].Value, 10);

                                if(!trackIndexes.TryGetValue(currentTrackEntry, out _))
                                    trackIndexes[currentTrackEntry] = new Dictionary<byte, int>();

                                trackIndexes[currentTrackEntry][indexNo] = indexLba;
                            }
                        }
                    }
                }

                if(inEntry)
                    entries.Add(currentEntry);

                if(entries.Count == 0)
                    throw new FeatureUnsupportedImageException("Did not find any track.");

                FullTOC.CDFullTOC toc;
                toc.TrackDescriptors     = entries.ToArray();
                toc.LastCompleteSession  = (byte)maxSession;
                toc.FirstCompleteSession = (byte)minSession;
                toc.DataLength           = (ushort)((entries.Count * 11) + 2);
                var tocMs = new MemoryStream();
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

                _fullToc = tocMs.ToArray();
                _imageInfo.ReadableMediaTags.Add(MediaTagType.CD_FullTOC);

                string dataFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".img";
                string subFile  = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".sub";

                var filtersList = new FiltersList();
                _dataFilter = filtersList.GetFilter(dataFile);

                if(_dataFilter == null)
                    throw new Exception("Cannot open data file");

                filtersList = new FiltersList();
                _subFilter  = filtersList.GetFilter(subFile);

                int  curSessionNo        = 0;
                var  currentTrack        = new Track();
                bool firstTrackInSession = true;
                Tracks = new List<Track>();
                ulong leadOutStart = 0;

                _dataStream = _dataFilter.GetDataForkStream();

                if(_subFilter != null)
                    _subStream = _subFilter.GetDataForkStream();

                _trackFlags = new Dictionary<byte, byte>();

                foreach(FullTOC.TrackDataDescriptor descriptor in entries)
                {
                    if(descriptor.SessionNumber > curSessionNo)
                    {
                        curSessionNo = descriptor.SessionNumber;

                        if(!firstTrackInSession)
                        {
                            currentTrack.TrackEndSector = leadOutStart - 1;
                            Tracks.Add(currentTrack);
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
                                    byte discType = descriptor.PSEC;
                                    AaruConsole.DebugWriteLine("CloneCD plugin", "Disc Type: {0}", discType);

                                    break;
                                case 0xA2:
                                    leadOutStart = GetLba(descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME);

                                    break;
                                default:
                                    if(descriptor.POINT >= 0x01 &&
                                       descriptor.POINT <= 0x63)
                                    {
                                        if(!firstTrackInSession)
                                        {
                                            currentTrack.TrackEndSector =
                                                GetLba(descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME) - 1;

                                            Tracks.Add(currentTrack);
                                        }

                                        currentTrack = new Track
                                        {
                                            TrackBytesPerSector    = 2352,
                                            TrackFile              = _dataFilter.GetFilename(),
                                            TrackFileType          = _scrambled ? "SCRAMBLED" : "BINARY",
                                            TrackFilter            = _dataFilter,
                                            TrackRawBytesPerSector = 2352,
                                            TrackSequence          = descriptor.POINT,
                                            TrackStartSector =
                                                GetLba(descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME),
                                            TrackSession = descriptor.SessionNumber
                                        };

                                        if(descriptor.POINT == 1)
                                        {
                                            currentTrack.TrackPregap      = currentTrack.TrackStartSector + 150;
                                            currentTrack.Indexes[0]       = -150;
                                            currentTrack.Indexes[1]       = (int)currentTrack.TrackStartSector;
                                            currentTrack.TrackStartSector = 0;
                                        }
                                        else
                                        {
                                            if(firstTrackInSession)
                                            {
                                                currentTrack.TrackPregap = 150;

                                                if(currentTrack.TrackStartSector > 0)
                                                {
                                                    currentTrack.Indexes[0] = (int)currentTrack.TrackStartSector - 150;

                                                    if(currentTrack.Indexes[0] < 0)
                                                        currentTrack.Indexes[0] = 0;
                                                }

                                                currentTrack.Indexes[1]       =  (int)currentTrack.TrackStartSector;
                                                currentTrack.TrackStartSector -= 150;
                                            }
                                            else
                                                currentTrack.Indexes[1] = (int)currentTrack.TrackStartSector;
                                        }

                                        firstTrackInSession = false;

                                        // Need to check exact data type later
                                        if((TocControl)(descriptor.CONTROL & 0x0D) == TocControl.DataTrack ||
                                           (TocControl)(descriptor.CONTROL & 0x0D) == TocControl.DataTrackIncremental)
                                            currentTrack.TrackType = TrackType.Data;
                                        else
                                            currentTrack.TrackType = TrackType.Audio;

                                        if(!_trackFlags.ContainsKey(descriptor.POINT))
                                            _trackFlags.Add(descriptor.POINT, descriptor.CONTROL);

                                        if(_subFilter != null)
                                        {
                                            currentTrack.TrackSubchannelFile   = _subFilter.GetFilename();
                                            currentTrack.TrackSubchannelFilter = _subFilter;
                                            currentTrack.TrackSubchannelType   = TrackSubchannelType.Raw;
                                        }
                                        else
                                            currentTrack.TrackSubchannelType = TrackSubchannelType.None;
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
                                        int frm  = descriptor.PFRAME - type;

                                        _imageInfo.MediaManufacturer = ATIP.ManufacturerFromATIP(descriptor.PSEC, frm);

                                        if(_imageInfo.MediaManufacturer != "")
                                            AaruConsole.DebugWriteLine("CloneCD plugin", "Disc manufactured by: {0}",
                                                                       _imageInfo.MediaManufacturer);
                                    }

                                    break;
                            }

                            break;
                        case 6:
                        {
                            uint id = (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame);
                            AaruConsole.DebugWriteLine("CloneCD plugin", "Disc ID: {0:X6}", id & 0x00FFFFFF);
                            _imageInfo.MediaSerialNumber = $"{id                               & 0x00FFFFFF:X6}";

                            break;
                        }
                    }
                }

                if(!firstTrackInSession)
                {
                    currentTrack.TrackEndSector = leadOutStart - 1;
                    Tracks.Add(currentTrack);
                }

                Track[] tmpTracks = Tracks.OrderBy(t => t.TrackSequence).ToArray();

                ulong currentDataOffset       = 0;
                ulong currentSubchannelOffset = 0;

                foreach(Track tmpTrack in tmpTracks)
                {
                    tmpTrack.TrackFileOffset = currentDataOffset;

                    currentDataOffset += 2352 * (tmpTrack.TrackEndSector - (ulong)tmpTrack.Indexes[1] + 1);

                    if(_subFilter != null)
                    {
                        tmpTrack.TrackSubchannelOffset = currentSubchannelOffset;

                        currentSubchannelOffset += 96 * (tmpTrack.TrackEndSector - (ulong)tmpTrack.Indexes[1] + 1);
                    }

                    if(tmpTrack.Indexes.TryGetValue(0, out int idx0))
                    {
                        if(idx0 < 0)
                        {
                            tmpTrack.TrackFileOffset       = 0;
                            tmpTrack.TrackSubchannelOffset = 0;
                        }
                        else
                        {
                            int indexDifference = tmpTrack.Indexes[1] - idx0;
                            tmpTrack.TrackFileOffset -= (ulong)(2352 * indexDifference);

                            if(_subFilter != null)
                                tmpTrack.TrackSubchannelOffset -= (ulong)(96 * indexDifference);
                        }
                    }

                    if(trackModes.TryGetValue((byte)tmpTrack.TrackSequence, out int trackMode))
                        tmpTrack.TrackType = trackMode switch
                        {
                            0 => TrackType.Audio,
                            1 => TrackType.CdMode1,
                            2 => TrackType.CdMode2Formless,
                            _ => TrackType.Data
                        };

                    if(trackIndexes.TryGetValue((byte)tmpTrack.TrackSequence, out Dictionary<byte, int> indexes))
                    {
                        foreach((byte index, int value) in indexes.OrderBy(i => i.Key).
                                                                   Where(trackIndex => trackIndex.Key > 1))

                            // Untested as of 20210711
                            tmpTrack.Indexes[index] = value;
                    }

                    if(tmpTrack.TrackType == TrackType.Data)
                    {
                        for(int s = 225; s < 750; s++)
                        {
                            byte[] syncTest = new byte[12];
                            byte[] sectTest = new byte[2352];

                            long pos = (long)tmpTrack.TrackFileOffset + (s * 2352);

                            if(pos >= _dataStream.Length + 2352 ||
                               s   >= (int)(tmpTrack.TrackEndSector - tmpTrack.TrackStartSector))
                                break;

                            _dataStream.Seek(pos, SeekOrigin.Begin);
                            _dataStream.Read(sectTest, 0, 2352);
                            Array.Copy(sectTest, 0, syncTest, 0, 12);

                            if(!Sector.SyncMark.SequenceEqual(syncTest))
                                continue;

                            if(_scrambled)
                                sectTest = Sector.Scramble(sectTest);

                            if(sectTest[15] == 1)
                            {
                                tmpTrack.TrackBytesPerSector = 2048;
                                tmpTrack.TrackType           = TrackType.CdMode1;

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                                if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                                if(_imageInfo.SectorSize < 2048)
                                    _imageInfo.SectorSize = 2048;

                                break;
                            }

                            if(sectTest[15] != 2)
                                continue;

                            byte[] subHdr1 = new byte[4];
                            byte[] subHdr2 = new byte[4];
                            byte[] empHdr  = new byte[4];

                            Array.Copy(sectTest, 16, subHdr1, 0, 4);
                            Array.Copy(sectTest, 20, subHdr2, 0, 4);

                            if(subHdr1.SequenceEqual(subHdr2) &&
                               !empHdr.SequenceEqual(subHdr1))
                                if((subHdr1[2] & 0x20) == 0x20)
                                {
                                    tmpTrack.TrackBytesPerSector = 2324;
                                    tmpTrack.TrackType           = TrackType.CdMode2Form2;

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                                    if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                        _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                                    if(_imageInfo.SectorSize < 2324)
                                        _imageInfo.SectorSize = 2324;

                                    break;
                                }
                                else
                                {
                                    tmpTrack.TrackBytesPerSector = 2048;
                                    tmpTrack.TrackType           = TrackType.CdMode2Form1;

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

                                    if(_imageInfo.SectorSize < 2048)
                                        _imageInfo.SectorSize = 2048;

                                    break;
                                }

                            tmpTrack.TrackBytesPerSector = 2336;
                            tmpTrack.TrackType           = TrackType.CdMode2Formless;

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                            if(!_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                            if(_imageInfo.SectorSize < 2336)
                                _imageInfo.SectorSize = 2336;

                            break;
                        }
                    }
                    else
                    {
                        if(_imageInfo.SectorSize < 2352)
                            _imageInfo.SectorSize = 2352;
                    }
                }

                Tracks = tmpTracks.ToList();

                if(_subFilter != null &&
                   !_imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                    _imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                _imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                Sessions = new List<Session>();

                var currentSession = new Session
                {
                    EndTrack        = uint.MinValue,
                    StartTrack      = uint.MaxValue,
                    SessionSequence = 1
                };

                Partitions = new List<Partition>();
                _offsetMap = new Dictionary<uint, ulong>();

                foreach(Track track in Tracks)
                {
                    if(track.TrackEndSector + 1 > _imageInfo.Sectors)
                        _imageInfo.Sectors = track.TrackEndSector + 1;

                    if(track.TrackSession == currentSession.SessionSequence)
                    {
                        if(track.TrackSequence > currentSession.EndTrack)
                        {
                            currentSession.EndSector = track.TrackEndSector;
                            currentSession.EndTrack  = track.TrackSequence;
                        }

                        if(track.TrackSequence < currentSession.StartTrack)
                        {
                            currentSession.StartSector = track.TrackStartSector;
                            currentSession.StartTrack  = track.TrackSequence;
                        }
                    }
                    else
                    {
                        Sessions.Add(currentSession);

                        currentSession = new Session
                        {
                            EndTrack        = track.TrackSequence,
                            StartTrack      = track.TrackSequence,
                            StartSector     = track.TrackStartSector,
                            EndSector       = track.TrackEndSector,
                            SessionSequence = track.TrackSession
                        };
                    }

                    var partition = new Partition
                    {
                        Description = track.TrackDescription,
                        Size = (track.TrackEndSector - (ulong)track.Indexes[1] + 1) *
                               (ulong)track.TrackRawBytesPerSector,
                        Length   = track.TrackEndSector - (ulong)track.Indexes[1] + 1,
                        Sequence = track.TrackSequence,
                        Offset   = track.TrackFileOffset,
                        Start    = (ulong)track.Indexes[1],
                        Type     = track.TrackType.ToString()
                    };

                    Partitions.Add(partition);
                    _offsetMap.Add(track.TrackSequence, track.TrackStartSector);
                }

                Sessions.Add(currentSession);

                bool data       = false;
                bool mode2      = false;
                bool firstAudio = false;
                bool firstData  = false;
                bool audio      = false;

                for(int i = 0; i < Tracks.Count; i++)
                {
                    // First track is audio
                    firstAudio |= i == 0 && Tracks[i].TrackType == TrackType.Audio;

                    // First track is data
                    firstData |= i == 0 && Tracks[i].TrackType != TrackType.Audio;

                    // Any non first track is data
                    data |= i != 0 && Tracks[i].TrackType != TrackType.Audio;

                    // Any non first track is audio
                    audio |= i != 0 && Tracks[i].TrackType == TrackType.Audio;

                    switch(Tracks[i].TrackType)
                    {
                        case TrackType.CdMode2Form1:
                        case TrackType.CdMode2Form2:
                        case TrackType.CdMode2Formless:
                            mode2 = true;

                            break;
                    }
                }

                // TODO: Check format
                _cdtext = cdtMs.ToArray();

                if(!data &&
                   !firstData)
                    _imageInfo.MediaType = MediaType.CDDA;
                else if(firstAudio         &&
                        data               &&
                        Sessions.Count > 1 &&
                        mode2)
                    _imageInfo.MediaType = MediaType.CDPLUS;
                else if((firstData && audio) || mode2)
                    _imageInfo.MediaType = MediaType.CDROMXA;
                else if(!audio)
                    _imageInfo.MediaType = MediaType.CDROM;
                else
                    _imageInfo.MediaType = MediaType.CD;

                _imageInfo.Application          = "CloneCD";
                _imageInfo.ImageSize            = (ulong)imageFilter.GetDataForkLength();
                _imageInfo.CreationTime         = imageFilter.GetCreationTime();
                _imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
                _imageInfo.XmlMediaType         = XmlMediaType.OpticalDisc;

                return true;
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter.GetFilename());
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
                case MediaTagType.CD_FullTOC: return _fullToc;
                case MediaTagType.CD_TEXT:
                {
                    if(_cdtext        != null &&
                       _cdtext.Length > 0)
                        return _cdtext;

                    throw new FeatureNotPresentImageException("Image does not contain CD-TEXT information.");
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
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress                                 - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector + 1 select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in _offsetMap.Where(kvp => sectorAddress >= kvp.Value).
                                                                SelectMany(kvp => Tracks, (kvp, track) => new
                                                                {
                                                                    kvp,
                                                                    track
                                                                }).Where(t => t.track.TrackSequence == t.kvp.Key).
                                                                Where(t => sectorAddress - t.kvp.Value <
                                                                           t.track.TrackEndSector -
                                                                           t.track.TrackStartSector + 1).
                                                                Select(t => t.kvp))
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track linqTrack in Tracks.Where(linqTrack => linqTrack.TrackSequence == track))
            {
                aaruTrack = linqTrack;

                break;
            }

            if(aaruTrack is null)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress - 1 > aaruTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string.
                                                          Format("Requested more sectors ({0} {2}) than present in track ({1}), won't cross tracks",
                                                                 length + sectorAddress, aaruTrack.TrackEndSector,
                                                                 sectorAddress));

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;
            bool mode2 = false;

            switch(aaruTrack.TrackType)
            {
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                case TrackType.CdMode1:
                {
                    sectorOffset = 16;
                    sectorSize   = 2048;
                    sectorSkip   = 288;

                    break;
                }
                case TrackType.CdMode2Formless:
                case TrackType.CdMode2Form1:
                case TrackType.CdMode2Form2:
                {
                    mode2        = true;
                    sectorOffset = 0;
                    sectorSize   = 2352;
                    sectorSkip   = 0;

                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            _dataStream.Seek((long)(aaruTrack.TrackFileOffset + (sectorAddress * 2352)), SeekOrigin.Begin);

            if(mode2)
            {
                var mode2Ms = new MemoryStream((int)(sectorSize * length));

                _dataStream.Read(buffer, 0, buffer.Length);

                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    Array.Copy(buffer, sectorSize * i, sector, 0, sectorSize);
                    sector = Sector.GetUserDataFromMode2(sector);
                    mode2Ms.Write(sector, 0, sector.Length);
                }

                buffer = mode2Ms.ToArray();
            }
            else if(sectorOffset == 0 &&
                    sectorSkip   == 0)
                _dataStream.Read(buffer, 0, buffer.Length);
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    _dataStream.Seek(sectorOffset, SeekOrigin.Current);
                    _dataStream.Read(sector, 0, sector.Length);
                    _dataStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        /// <inheritdoc />
        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            if(tag == SectorTagType.CdTrackFlags)
                track = (uint)sectorAddress;

            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track linqTrack in Tracks.Where(linqTrack => linqTrack.TrackSequence == track))
            {
                aaruTrack = linqTrack;

                break;
            }

            if(aaruTrack is null)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress - 1 > aaruTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector}), won't cross tracks");

            if(aaruTrack.TrackType == TrackType.Data)
                throw new ArgumentException("Unsupported tag requested", nameof(tag));

            byte[] buffer;

            switch(tag)
            {
                case SectorTagType.CdSectorEcc:
                case SectorTagType.CdSectorEccP:
                case SectorTagType.CdSectorEccQ:
                case SectorTagType.CdSectorEdc:
                case SectorTagType.CdSectorHeader:
                case SectorTagType.CdSectorSubHeader:
                case SectorTagType.CdSectorSync: break;
                case SectorTagType.CdTrackFlags:
                    return _trackFlags.TryGetValue((byte)aaruTrack.TrackSequence, out byte flags) ? new[]
                    {
                        flags
                    } : new byte[1];
                case SectorTagType.CdSectorSubchannel:
                    buffer = new byte[96 * length];
                    _subStream.Seek((long)(aaruTrack.TrackSubchannelOffset + (sectorAddress * 96)), SeekOrigin.Begin);
                    _subStream.Read(buffer, 0, buffer.Length);

                    return Subchannel.Interleave(buffer);
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(aaruTrack.TrackType)
            {
                case TrackType.CdMode1:
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
                case TrackType.CdMode2Formless:
                {
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        case SectorTagType.CdSectorHeader:
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
                case TrackType.CdMode2Form1:
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
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sectorOffset = 16;
                            sectorSize   = 8;
                            sectorSkip   = 2328;

                            break;
                        }
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
                            sectorOffset = 2072;
                            sectorSize   = 4;
                            sectorSkip   = 276;

                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case TrackType.CdMode2Form2:
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
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sectorOffset = 16;
                            sectorSize   = 8;
                            sectorSkip   = 2328;

                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2348;
                            sectorSize   = 4;
                            sectorSkip   = 0;

                            break;
                        }
                        default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
                    }

                    break;
                case TrackType.Audio:
                {
                    throw new ArgumentException("Unsupported tag requested", nameof(tag));
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            buffer = new byte[sectorSize * length];

            _dataStream.Seek((long)(aaruTrack.TrackFileOffset + (sectorAddress * 2352)), SeekOrigin.Begin);

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
                _dataStream.Read(buffer, 0, buffer.Length);
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    _dataStream.Seek(sectorOffset, SeekOrigin.Current);
                    _dataStream.Read(sector, 0, sector.Length);
                    _dataStream.Seek(sectorSkip, SeekOrigin.Current);
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
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in _offsetMap where sectorAddress     >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress                                 - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector + 1 select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        /// <inheritdoc />
        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            var aaruTrack = new Track
            {
                TrackSequence = 0
            };

            foreach(Track linqTrack in Tracks.Where(linqTrack => linqTrack.TrackSequence == track))
            {
                aaruTrack = linqTrack;

                break;
            }

            if(aaruTrack is null)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress - 1 > aaruTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector}), won't cross tracks");

            byte[] buffer = new byte[2352 * length];

            _dataStream.Seek((long)(aaruTrack.TrackFileOffset + (sectorAddress * 2352)), SeekOrigin.Begin);
            _dataStream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(Session session)
        {
            if(Sessions.Contains(session))
                return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        /// <inheritdoc />
        public List<Track> GetSessionTracks(ushort session) =>
            Tracks.Where(track => track.TrackSession == session).ToList();
    }
}