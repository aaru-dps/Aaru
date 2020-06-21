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
// Copyright © 2011-2020 Natalia Portillo
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
    public partial class CloneCd
    {
        public bool Open(IFilter imageFilter)
        {
            if(imageFilter == null)
                return false;

            ccdFilter = imageFilter;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                cueStream = new StreamReader(imageFilter.GetDataForkStream());
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

                bool                              inCcd        = false;
                bool                              inDisk       = false;
                bool                              inSession    = false;
                bool                              inEntry      = false;
                bool                              inTrack      = false;
                bool                              inCdText     = false;
                var                               cdtMs        = new MemoryStream();
                int                               minSession   = int.MaxValue;
                int                               maxSession   = int.MinValue;
                var                               currentEntry = new FullTOC.TrackDataDescriptor();
                List<FullTOC.TrackDataDescriptor> entries      = new List<FullTOC.TrackDataDescriptor>();
                scrambled = false;
                catalog   = null;

                while(cueStream.Peek() >= 0)
                {
                    lineNumber++;
                    string line = cueStream.ReadLine();

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
                    }
                    else
                    {
                        if(inCcd)
                        {
                            Match ccdVerMatch = ccdVerRegex.Match(line);

                            if(!ccdVerMatch.Success)
                                continue;

                            AaruConsole.DebugWriteLine("CloneCD plugin", "Found Version at line {0}", lineNumber);

                            imageInfo.Version = ccdVerMatch.Groups["value"].Value;

                            if(imageInfo.Version != "2" &&
                               imageInfo.Version != "3")
                                AaruConsole.
                                    ErrorWriteLine("(CloneCD plugin): Warning! Unknown CCD image version {0}, may not work!",
                                                   imageInfo.Version);
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

                                scrambled |= discScrMatch.Groups["value"].Value == "1";
                            }
                            else if(cdtLenMatch.Success)
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found CDTextLength at line {0}",
                                                           lineNumber);
                            else if(discCatMatch.Success)
                            {
                                AaruConsole.DebugWriteLine("CloneCD plugin", "Found Catalog at line {0}", lineNumber);
                                catalog = discCatMatch.Groups["value"].Value;
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

                fulltoc = tocMs.ToArray();
                imageInfo.ReadableMediaTags.Add(MediaTagType.CD_FullTOC);

                string dataFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".img";
                string subFile  = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".sub";

                var filtersList = new FiltersList();
                dataFilter = filtersList.GetFilter(dataFile);

                if(dataFilter == null)
                    throw new Exception("Cannot open data file");

                filtersList = new FiltersList();
                subFilter   = filtersList.GetFilter(subFile);

                int  curSessionNo        = 0;
                var  currentTrack        = new Track();
                bool firstTrackInSession = true;
                Tracks = new List<Track>();
                ulong leadOutStart = 0;

                dataStream = dataFilter.GetDataForkStream();

                if(subFilter != null)
                    subStream = subFilter.GetDataForkStream();

                trackFlags = new Dictionary<byte, byte>();

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
                                        else
                                            firstTrackInSession = false;

                                        currentTrack = new Track
                                        {
                                            TrackBytesPerSector = 2352, TrackFile = dataFilter.GetFilename(),
                                            TrackFileType       = scrambled ? "SCRAMBLED" : "BINARY",
                                            TrackFilter         = dataFilter, TrackRawBytesPerSector = 2352,
                                            TrackSequence       = descriptor.POINT,
                                            TrackStartSector =
                                                GetLba(descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME),
                                            TrackSession = descriptor.SessionNumber
                                        };

                                        // Need to check exact data type later
                                        if((TocControl)(descriptor.CONTROL & 0x0D) == TocControl.DataTrack ||
                                           (TocControl)(descriptor.CONTROL & 0x0D) == TocControl.DataTrackIncremental)
                                            currentTrack.TrackType = TrackType.Data;
                                        else
                                            currentTrack.TrackType = TrackType.Audio;

                                        if(!trackFlags.ContainsKey(descriptor.POINT))
                                            trackFlags.Add(descriptor.POINT, descriptor.CONTROL);

                                        if(subFilter != null)
                                        {
                                            currentTrack.TrackSubchannelFile   = subFilter.GetFilename();
                                            currentTrack.TrackSubchannelFilter = subFilter;
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

                                        imageInfo.MediaManufacturer = ATIP.ManufacturerFromATIP(descriptor.PSEC, frm);

                                        if(imageInfo.MediaManufacturer != "")
                                            AaruConsole.DebugWriteLine("CloneCD plugin", "Disc manufactured by: {0}",
                                                                       imageInfo.MediaManufacturer);
                                    }

                                    break;
                            }

                            break;
                        case 6:
                        {
                            uint id = (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame);
                            AaruConsole.DebugWriteLine("CloneCD plugin", "Disc ID: {0:X6}", id & 0x00FFFFFF);
                            imageInfo.MediaSerialNumber = $"{id                                & 0x00FFFFFF:X6}";

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

                for(int i = 0; i < tmpTracks.Length; i++)
                {
                    tmpTracks[i].TrackFileOffset = currentDataOffset;

                    currentDataOffset += 2352 * ((tmpTracks[i].TrackEndSector - tmpTracks[i].TrackStartSector) + 1);

                    if(subFilter != null)
                    {
                        tmpTracks[i].TrackSubchannelOffset = currentSubchannelOffset;

                        currentSubchannelOffset +=
                            96 * ((tmpTracks[i].TrackEndSector - tmpTracks[i].TrackStartSector) + 1);
                    }

                    if(tmpTracks[i].TrackType == TrackType.Data)
                    {
                        for(int s = 0; s < 750; s++)
                        {
                            byte[] syncTest = new byte[12];
                            byte[] sectTest = new byte[2352];

                            long pos = (long)tmpTracks[i].TrackFileOffset + (s * 2352);

                            if(pos >= dataStream.Length + 2352 ||
                               s   >= (int)(tmpTracks[i].TrackEndSector - tmpTracks[i].TrackStartSector))
                                break;

                            dataStream.Seek(pos, SeekOrigin.Begin);
                            dataStream.Read(sectTest, 0, 2352);
                            Array.Copy(sectTest, 0, syncTest, 0, 12);

                            if(!Sector.SyncMark.SequenceEqual(syncTest))
                                continue;

                            if(scrambled)
                                sectTest = Sector.Scramble(sectTest);

                            if(sectTest[15] == 1)
                            {
                                tmpTracks[i].TrackBytesPerSector = 2048;
                                tmpTracks[i].TrackType           = TrackType.CdMode1;

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccP))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEccQ))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                                if(imageInfo.SectorSize < 2048)
                                    imageInfo.SectorSize = 2048;

                                break;
                            }

                            if(sectTest[15] == 2)
                            {
                                byte[] subHdr1 = new byte[4];
                                byte[] subHdr2 = new byte[4];
                                byte[] empHdr  = new byte[4];

                                Array.Copy(sectTest, 16, subHdr1, 0, 4);
                                Array.Copy(sectTest, 20, subHdr2, 0, 4);

                                if(subHdr1.SequenceEqual(subHdr2) &&
                                   !empHdr.SequenceEqual(subHdr1))
                                    if((subHdr1[2] & 0x20) == 0x20)
                                    {
                                        tmpTracks[i].TrackBytesPerSector = 2324;
                                        tmpTracks[i].TrackType           = TrackType.CdMode2Form2;

                                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubHeader))
                                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubHeader);

                                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc))
                                            imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);

                                        if(imageInfo.SectorSize < 2324)
                                            imageInfo.SectorSize = 2324;

                                        break;
                                    }
                                    else
                                    {
                                        tmpTracks[i].TrackBytesPerSector = 2048;
                                        tmpTracks[i].TrackType           = TrackType.CdMode2Form1;

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

                                        if(imageInfo.SectorSize < 2048)
                                            imageInfo.SectorSize = 2048;

                                        break;
                                    }

                                tmpTracks[i].TrackBytesPerSector = 2336;
                                tmpTracks[i].TrackType           = TrackType.CdMode2Formless;

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSync))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);

                                if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorHeader))
                                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);

                                if(imageInfo.SectorSize < 2336)
                                    imageInfo.SectorSize = 2336;

                                break;
                            }
                        }
                    }
                    else
                    {
                        if(imageInfo.SectorSize < 2352)
                            imageInfo.SectorSize = 2352;
                    }
                }

                Tracks = tmpTracks.ToList();

                if(subFilter != null &&
                   !imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                Sessions = new List<Session>();

                var currentSession = new Session
                {
                    EndTrack = uint.MinValue, StartTrack = uint.MaxValue, SessionSequence = 1
                };

                Partitions = new List<Partition>();
                offsetmap  = new Dictionary<uint, ulong>();

                foreach(Track track in Tracks)
                {
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
                            EndTrack = uint.MinValue, StartTrack = uint.MaxValue, SessionSequence = track.TrackSession
                        };
                    }

                    var partition = new Partition
                    {
                        Description = track.TrackDescription,
                        Size = ((track.TrackEndSector - track.TrackStartSector) + 1) *
                               (ulong)track.TrackRawBytesPerSector,
                        Length = (track.TrackEndSector - track.TrackStartSector) + 1, Sequence = track.TrackSequence,
                        Offset = track.TrackFileOffset, Start                                  = track.TrackStartSector,
                        Type   = track.TrackType.ToString()
                    };

                    imageInfo.Sectors += partition.Length;
                    Partitions.Add(partition);
                    offsetmap.Add(track.TrackSequence, track.TrackStartSector);
                }

                bool data       = false;
                bool mode2      = false;
                bool firstaudio = false;
                bool firstdata  = false;
                bool audio      = false;

                for(int i = 0; i < Tracks.Count; i++)
                {
                    // First track is audio
                    firstaudio |= i == 0 && Tracks[i].TrackType == TrackType.Audio;

                    // First track is data
                    firstdata |= i == 0 && Tracks[i].TrackType != TrackType.Audio;

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
                cdtext = cdtMs.ToArray();

                if(!data &&
                   !firstdata)
                    imageInfo.MediaType = MediaType.CDDA;
                else if(firstaudio         &&
                        data               &&
                        Sessions.Count > 1 &&
                        mode2)
                    imageInfo.MediaType = MediaType.CDPLUS;
                else if((firstdata && audio) || mode2)
                    imageInfo.MediaType = MediaType.CDROMXA;
                else if(!audio)
                    imageInfo.MediaType = MediaType.CDROM;
                else
                    imageInfo.MediaType = MediaType.CD;

                imageInfo.Application          = "CloneCD";
                imageInfo.ImageSize            = (ulong)imageFilter.GetDataForkLength();
                imageInfo.CreationTime         = imageFilter.GetCreationTime();
                imageInfo.LastModificationTime = imageFilter.GetLastWriteTime();
                imageInfo.XmlMediaType         = XmlMediaType.OpticalDisc;

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

        public byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_FullTOC: return fulltoc;
                case MediaTagType.CD_TEXT:
                {
                    if(cdtext        != null &&
                       cdtext.Length > 0)
                        return cdtext;

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
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap where sectorAddress >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress <= track.TrackEndSector select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap where sectorAddress >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress <= track.TrackEndSector select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

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

            if((length + sectorAddress) - 1 > aaruTrack.TrackEndSector)
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

            dataStream.Seek((long)(aaruTrack.TrackFileOffset + (sectorAddress * 2352)), SeekOrigin.Begin);

            if(mode2)
            {
                var mode2Ms = new MemoryStream((int)(sectorSize * length));

                dataStream.Read(buffer, 0, buffer.Length);

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
                dataStream.Read(buffer, 0, buffer.Length);
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    dataStream.Seek(sectorOffset, SeekOrigin.Current);
                    dataStream.Read(sector, 0, sector.Length);
                    dataStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

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

            if((length + sectorAddress) - 1 > aaruTrack.TrackEndSector)
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
                    return !trackFlags.TryGetValue((byte)aaruTrack.TrackSequence, out byte flags) ? new[]
                    {
                        flags
                    } : new byte[1];
                case SectorTagType.CdSectorSubchannel:
                    buffer = new byte[96 * length];
                    subStream.Seek((long)(aaruTrack.TrackSubchannelOffset + (sectorAddress * 96)), SeekOrigin.Begin);
                    subStream.Read(buffer, 0, buffer.Length);

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

            dataStream.Seek((long)(aaruTrack.TrackFileOffset + (sectorAddress * 2352)), SeekOrigin.Begin);

            if(sectorOffset == 0 &&
               sectorSkip   == 0)
                dataStream.Read(buffer, 0, buffer.Length);
            else
                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    dataStream.Seek(sectorOffset, SeekOrigin.Current);
                    dataStream.Read(sector, 0, sector.Length);
                    dataStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }

            return buffer;
        }

        public byte[] ReadSectorLong(ulong sectorAddress) => ReadSectorsLong(sectorAddress, 1);

        public byte[] ReadSectorLong(ulong sectorAddress, uint track) => ReadSectorsLong(sectorAddress, 1, track);

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap where sectorAddress      >= kvp.Value
                                                     from track in Tracks where track.TrackSequence == kvp.Key
                                                     where sectorAddress                                   - kvp.Value <
                                                           (track.TrackEndSector - track.TrackStartSector) + 1
                                                     select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

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

            if((length + sectorAddress) - 1 > aaruTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({aaruTrack.TrackEndSector}), won't cross tracks");

            byte[] buffer = new byte[2352 * length];

            dataStream.Seek((long)(aaruTrack.TrackFileOffset + (sectorAddress * 2352)), SeekOrigin.Begin);
            dataStream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public List<Track> GetSessionTracks(Session session)
        {
            if(Sessions.Contains(session))
                return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public List<Track> GetSessionTracks(ushort session) =>
            Tracks.Where(track => track.TrackSession == session).ToList();
    }
}