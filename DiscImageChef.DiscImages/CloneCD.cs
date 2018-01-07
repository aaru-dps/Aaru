// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CloneCD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disc image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages CloneCD disc images.
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Filters;

namespace DiscImageChef.DiscImages
{
    // TODO: CloneCD stores subchannel deinterleaved
    public class CloneCd : IWritableImage
    {
        const string CCD_IDENTIFIER     = @"^\s*\[CloneCD\]";
        const string DISC_IDENTIFIER    = @"^\s*\[Disc\]";
        const string SESSION_IDENTIFIER = @"^\s*\[Session\s*(?<number>\d+)\]";
        const string ENTRY_IDENTIFIER   = @"^\s*\[Entry\s*(?<number>\d+)\]";
        const string TRACK_IDENTIFIER   = @"^\s*\[TRACK\s*(?<number>\d+)\]";
        const string CDTEXT_IDENTIFIER  = @"^\s*\[CDText\]";
        const string CCD_VERSION        = @"^\s*Version\s*=\s*(?<value>\d+)";
        const string DISC_ENTRIES       = @"^\s*TocEntries\s*=\s*(?<value>\d+)";
        const string DISC_SESSIONS      = @"^\s*Sessions\s*=\s*(?<value>\d+)";
        const string DISC_SCRAMBLED     = @"^\s*DataTracksScrambled\s*=\s*(?<value>\d+)";
        const string CDTEXT_LENGTH      = @"^\s*CDTextLength\s*=\s*(?<value>\d+)";
        const string DISC_CATALOG       = @"^\s*CATALOG\s*=\s*(?<value>\w+)";
        const string SESSION_PREGAP     = @"^\s*PreGapMode\s*=\s*(?<value>\d+)";
        const string SESSION_SUBCHANNEL = @"^\s*PreGapSubC\s*=\s*(?<value>\d+)";
        const string ENTRY_SESSION      = @"^\s*Session\s*=\s*(?<value>\d+)";
        const string ENTRY_POINT        = @"^\s*Point\s*=\s*(?<value>[\w+]+)";
        const string ENTRY_ADR          = @"^\s*ADR\s*=\s*(?<value>\w+)";
        const string ENTRY_CONTROL      = @"^\s*Control\s*=\s*(?<value>\w+)";
        const string ENTRY_TRACKNO      = @"^\s*TrackNo\s*=\s*(?<value>\d+)";
        const string ENTRY_AMIN         = @"^\s*AMin\s*=\s*(?<value>\d+)";
        const string ENTRY_ASEC         = @"^\s*ASec\s*=\s*(?<value>\d+)";
        const string ENTRY_AFRAME       = @"^\s*AFrame\s*=\s*(?<value>\d+)";
        const string ENTRY_ALBA         = @"^\s*ALBA\s*=\s*(?<value>-?\d+)";
        const string ENTRY_ZERO         = @"^\s*Zero\s*=\s*(?<value>\d+)";
        const string ENTRY_PMIN         = @"^\s*PMin\s*=\s*(?<value>\d+)";
        const string ENTRY_PSEC         = @"^\s*PSec\s*=\s*(?<value>\d+)";
        const string ENTRY_PFRAME       = @"^\s*PFrame\s*=\s*(?<value>\d+)";
        const string ENTRY_PLBA         = @"^\s*PLBA\s*=\s*(?<value>\d+)";
        const string CDTEXT_ENTRIES     = @"^\s*Entries\s*=\s*(?<value>\d+)";
        const string CDTEXT_ENTRY       = @"^\s*Entry\s*(?<number>\d+)\s*=\s*(?<value>([0-9a-fA-F]+\s*)+)";
        string       catalog; // TODO: Use it

        IFilter                 ccdFilter;
        byte[]                  cdtext;
        StreamReader            cueStream;
        IFilter                 dataFilter;
        Stream                  dataStream;
        StreamWriter            descriptorStream;
        byte[]                  fulltoc;
        ImageInfo               imageInfo;
        Dictionary<uint, ulong> offsetmap;
        bool                    scrambled;
        IFilter                 subFilter;
        Stream                  subStream;
        Dictionary<byte, byte>  trackFlags;
        string                  writingBaseName;

        public CloneCd()
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

        public string Name => "CloneCD";
        public Guid   Id   => new Guid("EE9C2975-2E79-427A-8EE9-F86F19165784");

        public string Format => "CloneCD";

        public List<Partition> Partitions { get; set; }

        public List<Track> Tracks { get; set; }

        public List<Session> Sessions { get; set; }

        public bool Identify(IFilter imageFilter)
        {
            ccdFilter = imageFilter;

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

                cueStream = new StreamReader(ccdFilter.GetDataForkStream());

                string line = cueStream.ReadLine();

                Regex hdr = new Regex(CCD_IDENTIFIER);

                Match hdm = hdr.Match(line ?? throw new InvalidOperationException());

                return hdm.Success;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", ccdFilter);
                DicConsole.ErrorWriteLine("Exception: {0}",                              ex.Message);
                DicConsole.ErrorWriteLine("Stack trace: {0}",                            ex.StackTrace);
                return false;
            }
        }

        public bool Open(IFilter imageFilter)
        {
            if(imageFilter == null) return false;

            ccdFilter = imageFilter;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                cueStream      = new StreamReader(imageFilter.GetDataForkStream());
                int lineNumber = 0;

                Regex ccdIdRegex     = new Regex(CCD_IDENTIFIER);
                Regex discIdRegex    = new Regex(DISC_IDENTIFIER);
                Regex sessIdRegex    = new Regex(SESSION_IDENTIFIER);
                Regex entryIdRegex   = new Regex(ENTRY_IDENTIFIER);
                Regex trackIdRegex   = new Regex(TRACK_IDENTIFIER);
                Regex cdtIdRegex     = new Regex(CDTEXT_IDENTIFIER);
                Regex ccdVerRegex    = new Regex(CCD_VERSION);
                Regex discEntRegex   = new Regex(DISC_ENTRIES);
                Regex discSessRegex  = new Regex(DISC_SESSIONS);
                Regex discScrRegex   = new Regex(DISC_SCRAMBLED);
                Regex cdtLenRegex    = new Regex(CDTEXT_LENGTH);
                Regex discCatRegex   = new Regex(DISC_CATALOG);
                Regex sessPregRegex  = new Regex(SESSION_PREGAP);
                Regex sessSubcRegex  = new Regex(SESSION_SUBCHANNEL);
                Regex entSessRegex   = new Regex(ENTRY_SESSION);
                Regex entPointRegex  = new Regex(ENTRY_POINT);
                Regex entAdrRegex    = new Regex(ENTRY_ADR);
                Regex entCtrlRegex   = new Regex(ENTRY_CONTROL);
                Regex entTnoRegex    = new Regex(ENTRY_TRACKNO);
                Regex entAMinRegex   = new Regex(ENTRY_AMIN);
                Regex entASecRegex   = new Regex(ENTRY_ASEC);
                Regex entAFrameRegex = new Regex(ENTRY_AFRAME);
                Regex entAlbaRegex   = new Regex(ENTRY_ALBA);
                Regex entZeroRegex   = new Regex(ENTRY_ZERO);
                Regex entPMinRegex   = new Regex(ENTRY_PMIN);
                Regex entPSecRegex   = new Regex(ENTRY_PSEC);
                Regex entPFrameRegex = new Regex(ENTRY_PFRAME);
                Regex entPlbaRegex   = new Regex(ENTRY_PLBA);
                Regex cdtEntsRegex   = new Regex(CDTEXT_ENTRIES);
                Regex cdtEntRegex    = new Regex(CDTEXT_ENTRY);

                bool                              inCcd        = false;
                bool                              inDisk       = false;
                bool                              inSession    = false;
                bool                              inEntry      = false;
                bool                              inTrack      = false;
                bool                              inCdText     = false;
                MemoryStream                      cdtMs        = new MemoryStream();
                int                               minSession   = int.MaxValue;
                int                               maxSession   = int.MinValue;
                FullTOC.TrackDataDescriptor       currentEntry = new FullTOC.TrackDataDescriptor();
                List<FullTOC.TrackDataDescriptor> entries      = new List<FullTOC.TrackDataDescriptor>();
                scrambled                                      = false;
                catalog                                        = null;

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
                        if(inDisk || inSession || inEntry || inTrack || inCdText)
                            throw new
                                FeatureUnsupportedImageException($"Found [CloneCD] out of order in line {lineNumber}");

                        inCcd     = true;
                        inDisk    = false;
                        inSession = false;
                        inEntry   = false;
                        inTrack   = false;
                        inCdText  = false;
                    }
                    else if(discIdMatch.Success  || sessIdMatch.Success || entryIdMatch.Success ||
                            trackIdMatch.Success || cdtIdMatch.Success)
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

                            if(!ccdVerMatch.Success) continue;

                            DicConsole.DebugWriteLine("CloneCD plugin", "Found Version at line {0}", lineNumber);

                            imageInfo.Version = ccdVerMatch.Groups["value"].Value;
                            if(imageInfo.Version != "2" && imageInfo.Version != "3")
                                DicConsole
                                   .ErrorWriteLine("(CloneCD plugin): Warning! Unknown CCD image version {0}, may not work!",
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
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found TocEntries at line {0}", lineNumber);
                            else if(discSessMatch.Success)
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Sessions at line {0}", lineNumber);
                            else if(discScrMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found DataTracksScrambled at line {0}",
                                                          lineNumber);
                                scrambled |= discScrMatch.Groups["value"].Value == "1";
                            }
                            else if(cdtLenMatch.Success)
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found CDTextLength at line {0}",
                                                          lineNumber);
                            else if(discCatMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Catalog at line {0}", lineNumber);
                                catalog = discCatMatch.Groups["value"].Value;
                            }
                        }
                        // TODO: Do not suppose here entries come sorted
                        else if(inCdText)
                        {
                            Match cdtEntsMatch = cdtEntsRegex.Match(line);
                            Match cdtEntMatch  = cdtEntRegex.Match(line);

                            if(cdtEntsMatch.Success)
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found CD-Text Entries at line {0}",
                                                          lineNumber);
                            else if(cdtEntMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found CD-Text Entry at line {0}",
                                                          lineNumber);
                                string[] bytes = cdtEntMatch.Groups["value"].Value.Split(new[] {' '},
                                                                                         StringSplitOptions
                                                                                            .RemoveEmptyEntries);
                                foreach(string byt in bytes) cdtMs.WriteByte(Convert.ToByte(byt, 16));
                            }
                        }
                        // Is this useful?
                        else if(inSession)
                        {
                            Match sessPregMatch = sessPregRegex.Match(line);
                            Match sessSubcMatch = sessSubcRegex.Match(line);

                            if(sessPregMatch.Success)
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PreGapMode at line {0}", lineNumber);
                            else if(sessSubcMatch.Success)
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PreGapSubC at line {0}", lineNumber);
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
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Session at line {0}", lineNumber);
                                currentEntry.SessionNumber =
                                    Convert.ToByte(entSessMatch.Groups["value"].Value, 10);
                                if(currentEntry.SessionNumber < minSession) minSession = currentEntry.SessionNumber;
                                if(currentEntry.SessionNumber > maxSession) maxSession = currentEntry.SessionNumber;
                            }
                            else if(entPointMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Point at line {0}", lineNumber);
                                currentEntry.POINT = Convert.ToByte(entPointMatch.Groups["value"].Value, 16);
                            }
                            else if(entAdrMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found ADR at line {0}", lineNumber);
                                currentEntry.ADR = Convert.ToByte(entAdrMatch.Groups["value"].Value, 16);
                            }
                            else if(entCtrlMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Control at line {0}", lineNumber);
                                currentEntry.CONTROL = Convert.ToByte(entCtrlMatch.Groups["value"].Value, 16);
                            }
                            else if(entTnoMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found TrackNo at line {0}", lineNumber);
                                currentEntry.TNO = Convert.ToByte(entTnoMatch.Groups["value"].Value, 10);
                            }
                            else if(entAMinMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found AMin at line {0}", lineNumber);
                                currentEntry.Min = Convert.ToByte(entAMinMatch.Groups["value"].Value, 10);
                            }
                            else if(entASecMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found ASec at line {0}", lineNumber);
                                currentEntry.Sec = Convert.ToByte(entASecMatch.Groups["value"].Value, 10);
                            }
                            else if(entAFrameMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found AFrame at line {0}", lineNumber);
                                currentEntry.Frame = Convert.ToByte(entAFrameMatch.Groups["value"].Value, 10);
                            }
                            else if(entAlbaMatch.Success)
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found ALBA at line {0}", lineNumber);
                            else if(entZeroMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Zero at line {0}", lineNumber);
                                currentEntry.Zero  = Convert.ToByte(entZeroMatch.Groups["value"].Value, 10);
                                currentEntry.HOUR  = (byte)((currentEntry.Zero & 0xF0) >> 4);
                                currentEntry.PHOUR = (byte)(currentEntry.Zero  & 0x0F);
                            }
                            else if(entPMinMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PMin at line {0}", lineNumber);
                                currentEntry.PMIN = Convert.ToByte(entPMinMatch.Groups["value"].Value, 10);
                            }
                            else if(entPSecMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PSec at line {0}", lineNumber);
                                currentEntry.PSEC = Convert.ToByte(entPSecMatch.Groups["value"].Value, 10);
                            }
                            else if(entPFrameMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PFrame at line {0}", lineNumber);
                                currentEntry.PFRAME = Convert.ToByte(entPFrameMatch.Groups["value"].Value, 10);
                            }
                            else if(entPlbaMatch.Success)
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PLBA at line {0}", lineNumber);
                        }
                    }
                }

                if(inEntry) entries.Add(currentEntry);

                if(entries.Count == 0) throw new FeatureUnsupportedImageException("Did not find any track.");

                FullTOC.CDFullTOC toc;
                toc.TrackDescriptors     = entries.ToArray();
                toc.LastCompleteSession  = (byte)maxSession;
                toc.FirstCompleteSession = (byte)minSession;
                toc.DataLength           = (ushort)(entries.Count * 11 + 2);
                MemoryStream tocMs       = new MemoryStream();
                tocMs.Write(BigEndianBitConverter.GetBytes(toc.DataLength), 0, 2);
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

                DicConsole.DebugWriteLine("CloneCD plugin", "{0}", FullTOC.Prettify(toc));

                string dataFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".img";
                string subFile  = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".sub";

                FiltersList filtersList = new FiltersList();
                dataFilter              = filtersList.GetFilter(dataFile);

                if(dataFilter == null) throw new Exception("Cannot open data file");

                filtersList = new FiltersList();
                subFilter   = filtersList.GetFilter(subFile);

                int   curSessionNo        = 0;
                Track currentTrack        = new Track();
                bool  firstTrackInSession = true;
                Tracks                    = new List<Track>();
                ulong leadOutStart        = 0;

                dataStream                      = dataFilter.GetDataForkStream();
                if(subFilter != null) subStream = subFilter.GetDataForkStream();
                trackFlags                      = new Dictionary<byte, byte>();

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
                                    DicConsole.DebugWriteLine("CloneCD plugin", "Disc Type: {0}", discType);
                                    break;
                                case 0xA2:
                                    leadOutStart = GetLba(descriptor.PHOUR, descriptor.PMIN, descriptor.PSEC,
                                                          descriptor.PFRAME);
                                    break;
                                default:
                                    if(descriptor.POINT >= 0x01 && descriptor.POINT <= 0x63)
                                    {
                                        if(!firstTrackInSession)
                                        {
                                            currentTrack.TrackEndSector =
                                                GetLba(descriptor.PHOUR, descriptor.PMIN, descriptor.PSEC,
                                                       descriptor.PFRAME) - 1;
                                            Tracks.Add(currentTrack);
                                        }
                                        else firstTrackInSession = false;

                                        currentTrack = new Track
                                        {
                                            TrackBytesPerSector    = 2352,
                                            TrackFile              = dataFilter.GetFilename(),
                                            TrackFileType          = scrambled ? "SCRAMBLED" : "BINARY",
                                            TrackFilter            = dataFilter,
                                            TrackRawBytesPerSector = 2352,
                                            TrackSequence          = descriptor.POINT,
                                            TrackStartSector       =
                                                GetLba(descriptor.PHOUR, descriptor.PMIN, descriptor.PSEC,
                                                       descriptor.PFRAME),
                                            TrackSession = descriptor.SessionNumber
                                        };
                                        currentTrack.TrackFileOffset = currentTrack.TrackStartSector * 2352;

                                        // Need to check exact data type later
                                        if((TocControl)(descriptor.CONTROL & 0x0D) == TocControl.DataTrack ||
                                           (TocControl)(descriptor.CONTROL & 0x0D) == TocControl.DataTrackIncremental)
                                            currentTrack.TrackType  = TrackType.Data;
                                        else currentTrack.TrackType = TrackType.Audio;

                                        if(!trackFlags.ContainsKey(descriptor.POINT))
                                            trackFlags.Add(descriptor.POINT, descriptor.CONTROL);

                                        if(subFilter != null)
                                        {
                                            currentTrack.TrackSubchannelFile   = subFilter.GetFilename();
                                            currentTrack.TrackSubchannelFilter = subFilter;
                                            currentTrack.TrackSubchannelOffset = currentTrack.TrackStartSector * 96;
                                            currentTrack.TrackSubchannelType   = TrackSubchannelType.Raw;
                                        }
                                        else currentTrack.TrackSubchannelType = TrackSubchannelType.None;

                                        if(currentTrack.TrackType == TrackType.Data)
                                        {
                                            byte[] syncTest = new byte[12];
                                            byte[] sectTest = new byte[2352];
                                            dataStream.Seek((long)currentTrack.TrackFileOffset, SeekOrigin.Begin);
                                            dataStream.Read(sectTest, 0, 2352);
                                            Array.Copy(sectTest, 0, syncTest, 0, 12);

                                            if(Sector.SyncMark.SequenceEqual(syncTest))
                                            {
                                                if(scrambled) sectTest = Sector.Scramble(sectTest);

                                                if(sectTest[15] == 1)
                                                {
                                                    currentTrack.TrackBytesPerSector = 2048;
                                                    currentTrack.TrackType           = TrackType.CdMode1;
                                                    if(!imageInfo.ReadableSectorTags
                                                                 .Contains(SectorTagType.CdSectorSync))
                                                        imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                                                    if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                 .CdSectorHeader))
                                                        imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                                                    if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc)
                                                    ) imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);
                                                    if(!imageInfo.ReadableSectorTags
                                                                 .Contains(SectorTagType.CdSectorEccP))
                                                        imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);
                                                    if(!imageInfo.ReadableSectorTags
                                                                 .Contains(SectorTagType.CdSectorEccQ))
                                                        imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);
                                                    if(!imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc)
                                                    ) imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                                                    if(imageInfo.SectorSize < 2048) imageInfo.SectorSize = 2048;
                                                }
                                                else if(sectTest[15] == 2)
                                                {
                                                    byte[] subHdr1 = new byte[4];
                                                    byte[] subHdr2 = new byte[4];
                                                    byte[] empHdr  = new byte[4];

                                                    Array.Copy(sectTest, 16, subHdr1, 0, 4);
                                                    Array.Copy(sectTest, 20, subHdr2, 0, 4);

                                                    if(subHdr1.SequenceEqual(subHdr2) && !empHdr.SequenceEqual(subHdr1))
                                                        if((subHdr1[2] & 0x20) == 0x20)
                                                        {
                                                            currentTrack.TrackBytesPerSector = 2324;
                                                            currentTrack.TrackType           = TrackType.CdMode2Form2;
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorSync))
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorSync);
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorHeader)
                                                            )
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorHeader);
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorSubHeader)
                                                            )
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorSubHeader);
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorEdc))
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorEdc);
                                                            if(imageInfo.SectorSize < 2324) imageInfo.SectorSize = 2324;
                                                        }
                                                        else
                                                        {
                                                            currentTrack.TrackBytesPerSector = 2048;
                                                            currentTrack.TrackType           = TrackType.CdMode2Form1;
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorSync))
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorSync);
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorHeader)
                                                            )
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorHeader);
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorSubHeader)
                                                            )
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorSubHeader);
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorEcc))
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorEcc);
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorEccP))
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorEccP);
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorEccQ))
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorEccQ);
                                                            if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                         .CdSectorEdc))
                                                                imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                    .CdSectorEdc);
                                                            if(imageInfo.SectorSize < 2048) imageInfo.SectorSize = 2048;
                                                        }
                                                    else
                                                    {
                                                        currentTrack.TrackBytesPerSector = 2336;
                                                        currentTrack.TrackType           = TrackType.CdMode2Formless;
                                                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                     .CdSectorSync))
                                                            imageInfo.ReadableSectorTags
                                                                     .Add(SectorTagType.CdSectorSync);
                                                        if(!imageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                     .CdSectorHeader))
                                                            imageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                .CdSectorHeader);
                                                        if(imageInfo.SectorSize < 2336) imageInfo.SectorSize = 2336;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if(imageInfo.SectorSize < 2352) imageInfo.SectorSize = 2352;
                                        }
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
                                            DicConsole.DebugWriteLine("CloneCD plugin", "Disc manufactured by: {0}",
                                                                      imageInfo.MediaManufacturer);
                                    }

                                    break;
                            }

                            break;
                        case 6:
                        {
                            uint id = (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame);
                            DicConsole.DebugWriteLine("CloneCD plugin", "Disc ID: {0:X6}", id & 0x00FFFFFF);
                            imageInfo.MediaSerialNumber = $"{id                               & 0x00FFFFFF:X6}";
                            break;
                        }
                    }
                }

                if(!firstTrackInSession)
                {
                    currentTrack.TrackEndSector = leadOutStart - 1;
                    Tracks.Add(currentTrack);
                }

                if(subFilter != null && !imageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                    imageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                imageInfo.ReadableSectorTags.Add(SectorTagType.CdTrackFlags);

                Sessions               = new List<Session>();
                Session currentSession = new Session
                {
                    EndTrack        = uint.MinValue,
                    StartTrack      = uint.MaxValue,
                    SessionSequence = 1
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
                            EndTrack        = uint.MinValue,
                            StartTrack      = uint.MaxValue,
                            SessionSequence = track.TrackSession
                        };
                    }

                    Partition partition = new Partition
                    {
                        Description = track.TrackDescription,
                        Size        =
                            (track.TrackEndSector - track.TrackStartSector + 1) *
                            (ulong)track.TrackRawBytesPerSector,
                        Length   = track.TrackEndSector - track.TrackStartSector + 1,
                        Sequence = track.TrackSequence,
                        Offset   = track.TrackFileOffset,
                        Start    = track.TrackStartSector,
                        Type     = track.TrackType.ToString()
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

                if(!data           && !firstdata) imageInfo.MediaType = MediaType.CDDA;
                else if(firstaudio && data && Sessions.Count > 1 && mode2)
                    imageInfo.MediaType = MediaType.CDPLUS;
                else if(firstdata && audio || mode2)
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
                case MediaTagType.CD_FullTOC: { return fulltoc; }
                case MediaTagType.CD_TEXT:
                {
                    if(cdtext != null && cdtext.Length > 0) return cdtext;

                    throw new FeatureNotPresentImageException("Image does not contain CD-TEXT information.");
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
                                                     from track in Tracks
                                                     where track.TrackSequence == kvp.Key
                                                     where sectorAddress       <= track.TrackEndSector
                                                     select kvp)
                return ReadSectors(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        public byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in from kvp in offsetmap
                                                     where sectorAddress >= kvp.Value
                                                     from track in Tracks
                                                     where track.TrackSequence == kvp.Key
                                                     where sectorAddress       <= track.TrackEndSector
                                                     select kvp)
                return ReadSectorsTag(sectorAddress - kvp.Value, length, kvp.Key, tag);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        public byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            Track dicTrack = new Track {TrackSequence = 0};

            foreach(Track linqTrack in Tracks.Where(linqTrack => linqTrack.TrackSequence == track))
            {
                dicTrack = linqTrack;
                break;
            }

            if(dicTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress - 1 > dicTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                         .Format("Requested more sectors ({0} {2}) than present in track ({1}), won't cross tracks",
                                                                 length + sectorAddress, dicTrack.TrackEndSector,
                                                                 sectorAddress));

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(dicTrack.TrackType)
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
                {
                    sectorOffset = 16;
                    sectorSize   = 2336;
                    sectorSkip   = 0;
                    break;
                }
                case TrackType.CdMode2Form1:
                {
                    sectorOffset = 24;
                    sectorSize   = 2048;
                    sectorSkip   = 280;
                    break;
                }
                case TrackType.CdMode2Form2:
                {
                    sectorOffset = 24;
                    sectorSize   = 2324;
                    sectorSkip   = 4;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            dataStream.Seek((long)(dicTrack.TrackFileOffset + sectorAddress * 2352), SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) dataStream.Read(buffer, 0, buffer.Length);
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
            Track dicTrack = new Track {TrackSequence = 0};

            foreach(Track linqTrack in Tracks.Where(linqTrack => linqTrack.TrackSequence == track))
            {
                dicTrack = linqTrack;
                break;
            }

            if(dicTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress - 1 > dicTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({dicTrack.TrackEndSector}), won't cross tracks");

            if(dicTrack.TrackType == TrackType.Data)
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
                    return !trackFlags.TryGetValue((byte)dicTrack.TrackSequence, out byte flags)
                               ? new[] {flags}
                               : new byte[1];
                case SectorTagType.CdSectorSubchannel:
                    buffer = new byte[96                                                 * length];
                    subStream.Seek((long)(dicTrack.TrackSubchannelOffset + sectorAddress * 96), SeekOrigin.Begin);
                    subStream.Read(buffer, 0, buffer.Length);
                    return buffer;
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(dicTrack.TrackType)
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
                case TrackType.Audio: { throw new ArgumentException("Unsupported tag requested", nameof(tag)); }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            buffer = new byte[sectorSize * length];

            dataStream.Seek((long)(dicTrack.TrackFileOffset + sectorAddress * 2352), SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) dataStream.Read(buffer, 0, buffer.Length);
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
                                                     from track in Tracks
                                                     where track.TrackSequence              == kvp.Key
                                                     where sectorAddress        - kvp.Value <
                                                           track.TrackEndSector - track.TrackStartSector + 1
                                                     select kvp)
                return ReadSectorsLong(sectorAddress - kvp.Value, length, kvp.Key);

            throw new ArgumentOutOfRangeException(nameof(sectorAddress), $"Sector address {sectorAddress} not found");
        }

        public byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            Track dicTrack = new Track {TrackSequence = 0};

            foreach(Track linqTrack in Tracks.Where(linqTrack => linqTrack.TrackSequence == track))
            {
                dicTrack = linqTrack;
                break;
            }

            if(dicTrack.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if(length + sectorAddress - 1 > dicTrack.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      $"Requested more sectors ({length + sectorAddress}) than present in track ({dicTrack.TrackEndSector}), won't cross tracks");

            byte[] buffer = new byte[2352 * length];

            dataStream.Seek((long)(dicTrack.TrackFileOffset + sectorAddress * 2352), SeekOrigin.Begin);
            dataStream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public List<Track> GetSessionTracks(Session session)
        {
            if(Sessions.Contains(session)) return GetSessionTracks(session.SessionSequence);

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public List<Track> GetSessionTracks(ushort session)
        {
            return Tracks.Where(track => track.TrackSession == session).ToList();
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

        public IEnumerable<MediaTagType>  SupportedMediaTags  => new[] {MediaTagType.CD_MCN, MediaTagType.CD_FullTOC};
        public IEnumerable<SectorTagType> SupportedSectorTags =>
            new[]
            {
                SectorTagType.CdSectorEcc, SectorTagType.CdSectorEccP, SectorTagType.CdSectorEccQ,
                SectorTagType.CdSectorEdc, SectorTagType.CdSectorHeader, SectorTagType.CdSectorSubHeader,
                SectorTagType.CdSectorSync, SectorTagType.CdTrackFlags, SectorTagType.CdSectorSubchannel
            };
        public IEnumerable<MediaType> SupportedMediaTypes =>
            new[]
            {
                MediaType.CD, MediaType.CDDA, MediaType.CDEG, MediaType.CDG, MediaType.CDI, MediaType.CDMIDI,
                MediaType.CDMRW, MediaType.CDPLUS, MediaType.CDR, MediaType.CDROM, MediaType.CDROMXA, MediaType.CDRW,
                MediaType.CDV, MediaType.DDCD, MediaType.DDCDR, MediaType.DDCDRW, MediaType.DTSCD, MediaType.JaguarCD,
                MediaType.MEGACD, MediaType.PS1CD, MediaType.PS2CD, MediaType.SuperCDROM2, MediaType.SVCD,
                MediaType.SATURNCD, MediaType.ThreeDO, MediaType.VCD, MediaType.VCDHD
            };
        public IEnumerable<(string name, Type type, string description)> SupportedOptions =>
            new (string name, Type type, string description)[] { };
        public IEnumerable<string> KnownExtensions => new[] {".ccd"};
        public bool                IsWriting       { get; private set; }
        public string              ErrorMessage    { get; private set; }

        public bool Create(string path, MediaType mediaType, Dictionary<string, string> options, ulong sectors,
                           uint   sectorSize)
        {
            if(!SupportedMediaTypes.Contains(mediaType))
            {
                ErrorMessage = $"Unsupport media format {mediaType}";
                return false;
            }

            imageInfo = new ImageInfo {MediaType = mediaType, SectorSize = sectorSize, Sectors = sectors};

            try
            {
                writingBaseName  = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
                descriptorStream = new StreamWriter(path, false, Encoding.ASCII);
                dataStream       = new FileStream(writingBaseName + ".img", FileMode.CreateNew, FileAccess.ReadWrite,
                                                  FileShare.None);
            }
            catch(IOException e)
            {
                ErrorMessage = $"Could not create new image file, exception {e.Message}";
                return false;
            }

            imageInfo.MediaType = mediaType;

            trackFlags = new Dictionary<byte, byte>();

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
                    catalog = Encoding.ASCII.GetString(data);
                    return true;
                case MediaTagType.CD_FullTOC:
                    fulltoc = data;
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

            // TODO: Implement ECC generation
            ErrorMessage = "This format requires sectors to be raw. Generating ECC is not yet implemented";
            return false;
        }

        public bool WriteSectors(byte[] data, ulong sectorAddress, uint length)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            // TODO: Implement ECC generation
            ErrorMessage = "This format requires sectors to be raw. Generating ECC is not yet implemented";
            return false;
        }

        public bool WriteSectorLong(byte[] data, ulong sectorAddress)
        {
            if(!IsWriting)
            {
                ErrorMessage = "Tried to write on a non-writable image";
                return false;
            }

            Track track =
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                             sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
                return false;
            }

            if(data.Length != track.TrackRawBytesPerSector)
            {
                ErrorMessage = "Incorrect data size";
                return false;
            }

            dataStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector),
                            SeekOrigin.Begin);
            dataStream.Write(data, 0, data.Length);

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
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
                                             sectorAddress <= trk.TrackEndSector);

            if(track.TrackSequence == 0)
            {
                ErrorMessage = $"Can't found track containing {sectorAddress}";
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

            dataStream.Seek((long)(track.TrackFileOffset + (sectorAddress - track.TrackStartSector) * (ulong)track.TrackRawBytesPerSector),
                            SeekOrigin.Begin);
            dataStream.Write(data, 0, data.Length);

            return true;
        }

        public bool SetTracks(List<Track> tracks)
        {
            ulong currentDataOffset       = 0;
            ulong currentSubchannelOffset = 0;

            Tracks = new List<Track>();
            foreach(Track track in tracks.OrderBy(t => t.TrackSequence))
            {
                Track newTrack = track;
                uint  subchannelSize;
                switch(track.TrackSubchannelType)
                {
                    case TrackSubchannelType.None:
                        subchannelSize = 0;
                        break;
                    case TrackSubchannelType.Raw:
                    case TrackSubchannelType.RawInterleaved:
                        subchannelSize = 96;
                        break;
                    default:
                        ErrorMessage = $"Unsupported subchannel type {track.TrackSubchannelType}";
                        return false;
                }

                newTrack.TrackFileOffset       = currentDataOffset;
                newTrack.TrackSubchannelOffset = currentSubchannelOffset;

                currentDataOffset += (ulong)newTrack.TrackRawBytesPerSector *
                                     (newTrack.TrackEndSector                        - newTrack.TrackStartSector + 1);
                currentSubchannelOffset += subchannelSize * (newTrack.TrackEndSector - newTrack.TrackStartSector + 1);

                Tracks.Add(newTrack);
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

            dataStream.Flush();
            dataStream.Close();

            subStream?.Flush();
            subStream?.Close();

            FullTOC.CDFullTOC? nullableToc              = null;
            FullTOC.CDFullTOC  toc                      =
                new FullTOC.CDFullTOC {TrackDescriptors = new FullTOC.TrackDataDescriptor[0]};

            // Easy, just decode the real toc
            if(fulltoc != null) nullableToc = FullTOC.Decode(fulltoc);

            // Not easy, create a toc from scratch
            if(nullableToc == null)
            {
                toc                                                = new FullTOC.CDFullTOC();
                Dictionary<byte, byte> sessionEndingTrack          = new Dictionary<byte, byte>();
                toc.FirstCompleteSession                           = byte.MaxValue;
                toc.LastCompleteSession                            = byte.MinValue;
                List<FullTOC.TrackDataDescriptor> trackDescriptors = new List<FullTOC.TrackDataDescriptor>();
                byte                              currentTrack     = 0;

                foreach(Track track in Tracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence))
                {
                    if(track.TrackSession < toc.FirstCompleteSession)
                        toc.FirstCompleteSession = (byte)track.TrackSession;

                    if(track.TrackSession <= toc.LastCompleteSession)
                    {
                        currentTrack = (byte)track.TrackSequence;
                        continue;
                    }

                    if(toc.LastCompleteSession > 0) sessionEndingTrack.Add(toc.LastCompleteSession, currentTrack);

                    toc.LastCompleteSession = (byte)track.TrackSession;
                }

                byte currentSession = 0;

                foreach(Track track in Tracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence))
                {
                    FullTOC.TrackDataDescriptor trackDescriptor;
                    trackFlags.TryGetValue((byte)track.TrackSequence, out byte trackControl);

                    if(trackControl == 0 && track.TrackType != TrackType.Audio) trackControl = (byte)CdFlags.DataTrack;

                    // Lead-Out
                    if(track.TrackSession > currentSession && currentSession != 0)
                    {
                        (byte hour, byte minute, byte second, byte frame) leadoutAmsf =
                            LbaToMsf(track.TrackStartSector - 150);
                        (byte hour, byte minute, byte second, byte frame) leadoutPmsf =
                            LbaToMsf(Tracks.OrderBy(t => t.TrackSession).ThenBy(t => t.TrackSequence).Last()
                                           .TrackStartSector);

                        // Lead-out
                        trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                        {
                            SessionNumber = currentSession,
                            POINT         = 0xB0,
                            ADR           = 5,
                            CONTROL       = 0,
                            HOUR          = leadoutAmsf.hour,
                            Min           = leadoutAmsf.minute,
                            Sec           = leadoutAmsf.second,
                            Frame         = leadoutAmsf.frame,
                            PHOUR         = leadoutPmsf.hour,
                            PMIN          = leadoutPmsf.minute,
                            PSEC          = leadoutPmsf.second,
                            PFRAME        = leadoutPmsf.frame
                        });

                        // This seems to be constant? It should not exist on CD-ROM but CloneCD creates them anyway
                        // Format seems like ATIP, but ATIP should not be as 0xC0 in TOC...
                        trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                        {
                            SessionNumber = currentSession,
                            POINT         = 0xC0,
                            ADR           = 5,
                            CONTROL       = 0,
                            Min           = 128,
                            PMIN          = 97,
                            PSEC          = 25
                        });
                    }

                    // Lead-in
                    if(track.TrackSession > currentSession)
                    {
                        currentSession = (byte)track.TrackSession;
                        sessionEndingTrack.TryGetValue(currentSession, out byte endingTrackNumber);
                        (byte hour, byte minute, byte second, byte frame) leadinPmsf =
                            LbaToMsf(Tracks.FirstOrDefault(t => t.TrackSequence == endingTrackNumber).TrackEndSector +
                                     1);

                        // Starting track
                        trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                        {
                            SessionNumber = currentSession,
                            POINT         = 0xA0,
                            ADR           = 1,
                            CONTROL       = trackControl,
                            PMIN          = (byte)track.TrackSequence
                        });

                        // Ending track
                        trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                        {
                            SessionNumber = currentSession,
                            POINT         = 0xA1,
                            ADR           = 1,
                            CONTROL       = trackControl,
                            PMIN          = endingTrackNumber
                        });

                        // Lead-out start
                        trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                        {
                            SessionNumber = currentSession,
                            POINT         = 0xA2,
                            ADR           = 1,
                            CONTROL       = trackControl,
                            PHOUR         = leadinPmsf.hour,
                            PMIN          = leadinPmsf.minute,
                            PSEC          = leadinPmsf.second,
                            PFRAME        = leadinPmsf.frame
                        });
                    }

                    (byte hour, byte minute, byte second, byte frame) pmsf = LbaToMsf(track.TrackStartSector);

                    // Track
                    trackDescriptors.Add(new FullTOC.TrackDataDescriptor
                    {
                        SessionNumber = (byte)track.TrackSession,
                        POINT         = (byte)track.TrackSequence,
                        ADR           = 1,
                        CONTROL       = trackControl,
                        PHOUR         = pmsf.hour,
                        PMIN          = pmsf.minute,
                        PSEC          = pmsf.second,
                        PFRAME        = pmsf.frame
                    });
                }

                toc.TrackDescriptors = trackDescriptors.ToArray();
            }
            else toc = nullableToc.Value;

            descriptorStream.WriteLine("[CloneCD]");
            descriptorStream.WriteLine("Version=2");
            descriptorStream.WriteLine("[Disc]");
            descriptorStream.WriteLine("TocEntries={0}", toc.TrackDescriptors.Length);
            descriptorStream.WriteLine("Sessions={0}",   toc.LastCompleteSession);
            descriptorStream.WriteLine("DataTracksScrambled=0");
            descriptorStream.WriteLine("CDTextLength=0");
            if(!string.IsNullOrEmpty(catalog)) descriptorStream.WriteLine("CATALOG={0}", catalog);
            for(int i = 1; i <= toc.LastCompleteSession; i++)
            {
                descriptorStream.WriteLine("[Session {0}]", i);
                descriptorStream.WriteLine("PreGapMode=0");
                descriptorStream.WriteLine("PreGapSubC=0");
            }

            for(int i = 0; i < toc.TrackDescriptors.Length; i++)
            {
                long alba =
                    MsfToLba((toc.TrackDescriptors[i].HOUR, toc.TrackDescriptors[i].Min, toc.TrackDescriptors[i].Sec,
                             toc.TrackDescriptors[i].Frame));
                long plba =
                    MsfToLba((toc.TrackDescriptors[i].PHOUR, toc.TrackDescriptors[i].PMIN, toc.TrackDescriptors[i].PSEC,
                             toc.TrackDescriptors[i].PFRAME));

                descriptorStream.WriteLine("[Entry {0}]",      i);
                descriptorStream.WriteLine("Session={0}",      toc.TrackDescriptors[i].SessionNumber);
                descriptorStream.WriteLine("Point=0x{0:x2}",   toc.TrackDescriptors[i].POINT);
                descriptorStream.WriteLine("ADR=0x{0:x2}",     toc.TrackDescriptors[i].ADR);
                descriptorStream.WriteLine("Control=0x{0:x2}", toc.TrackDescriptors[i].CONTROL);
                descriptorStream.WriteLine("TrackNo={0}",      toc.TrackDescriptors[i].TNO);
                descriptorStream.WriteLine("AMin={0}",         toc.TrackDescriptors[i].Min);
                descriptorStream.WriteLine("ASec={0}",         toc.TrackDescriptors[i].Sec);
                descriptorStream.WriteLine("AFrame={0}",       toc.TrackDescriptors[i].Frame);
                descriptorStream.WriteLine("ALBA={0}",         toc.TrackDescriptors[i].POINT == 0xC0 ? 125850 : alba);
                descriptorStream.WriteLine("Zero={0}",
                                           ((toc.TrackDescriptors[i].HOUR & 0x0F) << 4) +
                                           (toc.TrackDescriptors[i].PHOUR & 0x0F));
                descriptorStream.WriteLine("PMin={0}",   toc.TrackDescriptors[i].PMIN);
                descriptorStream.WriteLine("PSec={0}",   toc.TrackDescriptors[i].PSEC);
                descriptorStream.WriteLine("PFrame={0}", toc.TrackDescriptors[i].PFRAME);
                descriptorStream.WriteLine("PLBA={0}",   toc.TrackDescriptors[i].POINT == 0xC0 ? -11775 : plba);
            }

            descriptorStream.Flush();
            descriptorStream.Close();

            ErrorMessage = "";
            return true;
        }

        public bool SetMetadata(ImageInfo metadata)
        {
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
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
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

                    if(subStream == null)
                        try
                        {
                            subStream = new FileStream(writingBaseName + ".sub", FileMode.CreateNew,
                                                       FileAccess.ReadWrite, FileShare.None);
                        }
                        catch(IOException e)
                        {
                            ErrorMessage = $"Could not create subchannel file, exception {e.Message}";
                            return false;
                        }

                    subStream.Seek((long)(track.TrackSubchannelOffset + (sectorAddress - track.TrackStartSector) * 96),
                                   SeekOrigin.Begin);
                    subStream.Write(data, 0, data.Length);

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
                Tracks.FirstOrDefault(trk => sectorAddress >= trk.TrackStartSector &&
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

                    if(subStream == null)
                        try
                        {
                            subStream = new FileStream(writingBaseName + ".sub", FileMode.CreateNew,
                                                       FileAccess.ReadWrite, FileShare.None);
                        }
                        catch(IOException e)
                        {
                            ErrorMessage = $"Could not create subchannel file, exception {e.Message}";
                            return false;
                        }

                    subStream.Seek((long)(track.TrackSubchannelOffset + (sectorAddress - track.TrackStartSector) * 96),
                                   SeekOrigin.Begin);
                    subStream.Write(data, 0, data.Length);

                    return true;
                }
                default:
                    ErrorMessage = $"Unsupported tag type {tag}";
                    return false;
            }
        }

        static ulong GetLba(int hour, int minute, int second, int frame)
        {
            return (ulong)(hour * 60 * 60 * 75 + minute * 60 * 75 + second * 75 + frame - 150);
        }

        static long MsfToLba((byte hour, byte minute, byte second, byte frame) msf)
        {
            return msf.hour * 60 * 60 * 75 + msf.minute * 60 * 75 + msf.second * 75 + msf.frame - 150;
        }

        static (byte hour, byte minute, byte second, byte frame) LbaToMsf(ulong sector)
        {
            return ((byte)((sector + 150) / 75 / 60 / 60), (byte)((sector + 150) / 75 / 60 % 60),
                (byte)((sector     + 150) / 75 % 60), (byte)((sector      + 150) % 75));
        }
    }
}