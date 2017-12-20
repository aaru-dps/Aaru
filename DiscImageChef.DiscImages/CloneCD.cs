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
using System.Text.RegularExpressions;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Filters;
using DiscImageChef.DiscImages;

namespace DiscImageChef.DiscImages
{
    public class CloneCd : ImagePlugin
    {
        #region Parsing regexs
        const string CCD_IDENTIFIER = "^\\s*\\[CloneCD\\]";
        const string DISC_IDENTIFIER = "^\\s*\\[Disc\\]";
        const string SESSION_IDENTIFIER = "^\\s*\\[Session\\s*(?<number>\\d+)\\]";
        const string ENTRY_IDENTIFIER = "^\\s*\\[Entry\\s*(?<number>\\d+)\\]";
        const string TRACK_IDENTIFIER = "^\\s*\\[TRACK\\s*(?<number>\\d+)\\]";
        const string CDTEXT_IDENTIFIER = "^\\s*\\[CDText\\]";
        const string CCD_VERSION = "^\\s*Version\\s*=\\s*(?<value>\\d+)";
        const string DISC_ENTRIES = "^\\s*TocEntries\\s*=\\s*(?<value>\\d+)";
        const string DISC_SESSIONS = "^\\s*Sessions\\s*=\\s*(?<value>\\d+)";
        const string DISC_SCRAMBLED = "^\\s*DataTracksScrambled\\s*=\\s*(?<value>\\d+)";
        const string CDTEXT_LENGTH = "^\\s*CDTextLength\\s*=\\s*(?<value>\\d+)";
        const string DISC_CATALOG = "^\\s*CATALOG\\s*=\\s*(?<value>\\w+)";
        const string SESSION_PREGAP = "^\\s*PreGapMode\\s*=\\s*(?<value>\\d+)";
        const string SESSION_SUBCHANNEL = "^\\s*PreGapSubC\\s*=\\s*(?<value>\\d+)";
        const string ENTRY_SESSION = "^\\s*Session\\s*=\\s*(?<value>\\d+)";
        const string ENTRY_POINT = "^\\s*Point\\s*=\\s*(?<value>[\\w+]+)";
        const string ENTRY_ADR = "^\\s*ADR\\s*=\\s*(?<value>\\w+)";
        const string ENTRY_CONTROL = "^\\s*Control\\s*=\\s*(?<value>\\w+)";
        const string ENTRY_TRACKNO = "^\\s*TrackNo\\s*=\\s*(?<value>\\d+)";
        const string ENTRY_AMIN = "^\\s*AMin\\s*=\\s*(?<value>\\d+)";
        const string ENTRY_ASEC = "^\\s*ASec\\s*=\\s*(?<value>\\d+)";
        const string ENTRY_AFRAME = "^\\s*AFrame\\s*=\\s*(?<value>\\d+)";
        const string ENTRY_ALBA = "^\\s*ALBA\\s*=\\s*(?<value>-?\\d+)";
        const string ENTRY_ZERO = "^\\s*Zero\\s*=\\s*(?<value>\\d+)";
        const string ENTRY_PMIN = "^\\s*PMin\\s*=\\s*(?<value>\\d+)";
        const string ENTRY_PSEC = "^\\s*PSec\\s*=\\s*(?<value>\\d+)";
        const string ENTRY_PFRAME = "^\\s*PFrame\\s*=\\s*(?<value>\\d+)";
        const string ENTRY_PLBA = "^\\s*PLBA\\s*=\\s*(?<value>\\d+)";
        const string CDTEXT_ENTRIES = "^\\s*Entries\\s*=\\s*(?<value>\\d+)";
        const string CDTEXT_ENTRY = "^\\s*Entry\\s*(?<number>\\d+)\\s*=\\s*(?<value>([0-9a-fA-F]+\\s*)+)";
        #endregion

        Filter imageFilter;
        Filter dataFilter;
        Filter subFilter;
        StreamReader cueStream;
        byte[] fulltoc;
        bool scrambled;
        string catalog;
        List<DiscImages.Session> sessions;
        List<Partition> partitions;
        List<Track> tracks;
        Stream dataStream;
        Stream subStream;
        Dictionary<uint, ulong> offsetmap;
        byte[] cdtext;

        public CloneCd()
        {
            Name = "CloneCD";
            PluginUuid = new Guid("EE9C2975-2E79-427A-8EE9-F86F19165784");
            ImageInfo = new ImageInfo();
            ImageInfo.ReadableSectorTags = new List<SectorTagType>();
            ImageInfo.ReadableMediaTags = new List<MediaTagType>();
            ImageInfo.ImageHasPartitions = true;
            ImageInfo.ImageHasSessions = true;
            ImageInfo.ImageVersion = null;
            ImageInfo.ImageApplicationVersion = null;
            ImageInfo.ImageName = null;
            ImageInfo.ImageCreator = null;
            ImageInfo.MediaManufacturer = null;
            ImageInfo.MediaModel = null;
            ImageInfo.MediaPartNumber = null;
            ImageInfo.MediaSequence = 0;
            ImageInfo.LastMediaSequence = 0;
            ImageInfo.DriveManufacturer = null;
            ImageInfo.DriveModel = null;
            ImageInfo.DriveSerialNumber = null;
            ImageInfo.DriveFirmwareRevision = null;
        }

        public override bool IdentifyImage(Filter imageFilter)
        {
            this.imageFilter = imageFilter;

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

                cueStream = new StreamReader(this.imageFilter.GetDataForkStream());

                string _line = cueStream.ReadLine();

                Regex hdr = new Regex(CCD_IDENTIFIER);

                Match hdm;

                hdm = hdr.Match(_line);

                return hdm.Success;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", this.imageFilter);
                DicConsole.ErrorWriteLine("Exception: {0}", ex.Message);
                DicConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);
                return false;
            }
        }

        public override bool OpenImage(Filter imageFilter)
        {
            if(imageFilter == null) return false;

            this.imageFilter = imageFilter;

            try
            {
                imageFilter.GetDataForkStream().Seek(0, SeekOrigin.Begin);
                cueStream = new StreamReader(imageFilter.GetDataForkStream());
                int line = 0;

                Regex ccdIdRegex = new Regex(CCD_IDENTIFIER);
                Regex discIdRegex = new Regex(DISC_IDENTIFIER);
                Regex sessIdRegex = new Regex(SESSION_IDENTIFIER);
                Regex entryIdRegex = new Regex(ENTRY_IDENTIFIER);
                Regex trackIdRegex = new Regex(TRACK_IDENTIFIER);
                Regex cdtIdRegex = new Regex(CDTEXT_IDENTIFIER);
                Regex ccdVerRegex = new Regex(CCD_VERSION);
                Regex discEntRegex = new Regex(DISC_ENTRIES);
                Regex discSessRegex = new Regex(DISC_SESSIONS);
                Regex discScrRegex = new Regex(DISC_SCRAMBLED);
                Regex cdtLenRegex = new Regex(CDTEXT_LENGTH);
                Regex discCatRegex = new Regex(DISC_CATALOG);
                Regex sessPregRegex = new Regex(SESSION_PREGAP);
                Regex sessSubcRegex = new Regex(SESSION_SUBCHANNEL);
                Regex entSessRegex = new Regex(ENTRY_SESSION);
                Regex entPointRegex = new Regex(ENTRY_POINT);
                Regex entAdrRegex = new Regex(ENTRY_ADR);
                Regex entCtrlRegex = new Regex(ENTRY_CONTROL);
                Regex entTnoRegex = new Regex(ENTRY_TRACKNO);
                Regex entAMinRegex = new Regex(ENTRY_AMIN);
                Regex entASecRegex = new Regex(ENTRY_ASEC);
                Regex entAFrameRegex = new Regex(ENTRY_AFRAME);
                Regex entAlbaRegex = new Regex(ENTRY_ALBA);
                Regex entZeroRegex = new Regex(ENTRY_ZERO);
                Regex entPMinRegex = new Regex(ENTRY_PMIN);
                Regex entPSecRegex = new Regex(ENTRY_PSEC);
                Regex entPFrameRegex = new Regex(ENTRY_PFRAME);
                Regex entPlbaRegex = new Regex(ENTRY_PLBA);
                Regex cdtEntsRegex = new Regex(CDTEXT_ENTRIES);
                Regex cdtEntRegex = new Regex(CDTEXT_ENTRY);

                Match ccdIdMatch;
                Match discIdMatch;
                Match sessIdMatch;
                Match entryIdMatch;
                Match trackIdMatch;
                Match cdtIdMatch;
                Match ccdVerMatch;
                Match discEntMatch;
                Match discSessMatch;
                Match discScrMatch;
                Match cdtLenMatch;
                Match discCatMatch;
                Match sessPregMatch;
                Match sessSubcMatch;
                Match entSessMatch;
                Match entPointMatch;
                Match entAdrMatch;
                Match entCtrlMatch;
                Match entTnoMatch;
                Match entAMinMatch;
                Match entASecMatch;
                Match entAFrameMatch;
                Match entAlbaMatch;
                Match entZeroMatch;
                Match entPMinMatch;
                Match entPSecMatch;
                Match entPFrameMatch;
                Match entPlbaMatch;
                Match cdtEntsMatch;
                Match cdtEntMatch;

                bool inCcd = false;
                bool inDisk = false;
                bool inSession = false;
                bool inEntry = false;
                bool inTrack = false;
                bool inCdText = false;
                MemoryStream cdtMs = new MemoryStream();
                int minSession = int.MaxValue;
                int maxSession = int.MinValue;
                FullTOC.TrackDataDescriptor currentEntry = new FullTOC.TrackDataDescriptor();
                List<FullTOC.TrackDataDescriptor> entries = new List<FullTOC.TrackDataDescriptor>();
                scrambled = false;
                catalog = null;

                while(cueStream.Peek() >= 0)
                {
                    line++;
                    string _line = cueStream.ReadLine();

                    ccdIdMatch = ccdIdRegex.Match(_line);
                    discIdMatch = discIdRegex.Match(_line);
                    sessIdMatch = sessIdRegex.Match(_line);
                    entryIdMatch = entryIdRegex.Match(_line);
                    trackIdMatch = trackIdRegex.Match(_line);
                    cdtIdMatch = cdtIdRegex.Match(_line);

                    // [CloneCD]
                    if(ccdIdMatch.Success)
                    {
                        if(inDisk || inSession || inEntry || inTrack || inCdText)
                            throw new
                                FeatureUnsupportedImageException(string
                                                                     .Format("Found [CloneCD] out of order in line {0}",
                                                                             line));

                        inCcd = true;
                        inDisk = false;
                        inSession = false;
                        inEntry = false;
                        inTrack = false;
                        inCdText = false;
                    }
                    else if(discIdMatch.Success || sessIdMatch.Success || entryIdMatch.Success ||
                            trackIdMatch.Success || cdtIdMatch.Success)
                    {
                        if(inEntry)
                        {
                            entries.Add(currentEntry);
                            currentEntry = new FullTOC.TrackDataDescriptor();
                        }

                        inCcd = false;
                        inDisk = discIdMatch.Success;
                        inSession = sessIdMatch.Success;
                        inEntry = entryIdMatch.Success;
                        inTrack = trackIdMatch.Success;
                        inCdText = cdtIdMatch.Success;
                    }
                    else
                    {
                        if(inCcd)
                        {
                            ccdVerMatch = ccdVerRegex.Match(_line);

                            if(ccdVerMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Version at line {0}", line);

                                ImageInfo.ImageVersion = ccdVerMatch.Groups["value"].Value;
                                if(ImageInfo.ImageVersion != "2" && ImageInfo.ImageVersion != "3")
                                    DicConsole
                                        .ErrorWriteLine("(CloneCD plugin): Warning! Unknown CCD image version {0}, may not work!",
                                                        ImageInfo.ImageVersion);
                            }
                        }
                        else if(inDisk)
                        {
                            discEntMatch = discEntRegex.Match(_line);
                            discSessMatch = discSessRegex.Match(_line);
                            discScrMatch = discScrRegex.Match(_line);
                            cdtLenMatch = cdtLenRegex.Match(_line);
                            discCatMatch = discCatRegex.Match(_line);

                            if(discEntMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found TocEntries at line {0}", line);
                            }
                            else if(discSessMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Sessions at line {0}", line);
                            }
                            else if(discScrMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found DataTracksScrambled at line {0}",
                                                          line);
                                scrambled |= discScrMatch.Groups["value"].Value == "1";
                            }
                            else if(cdtLenMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found CDTextLength at line {0}", line);
                            }
                            else if(discCatMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Catalog at line {0}", line);
                                catalog = discCatMatch.Groups["value"].Value;
                            }
                        }
                        // TODO: Do not suppose here entries come sorted
                        else if(inCdText)
                        {
                            cdtEntsMatch = cdtEntsRegex.Match(_line);
                            cdtEntMatch = cdtEntRegex.Match(_line);

                            if(cdtEntsMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found CD-Text Entries at line {0}", line);
                            }
                            else if(cdtEntMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found CD-Text Entry at line {0}", line);
                                string[] bytes = cdtEntMatch.Groups["value"].Value.Split(new char[] {' '},
                                                                                          StringSplitOptions
                                                                                              .RemoveEmptyEntries);
                                foreach(string byt in bytes) cdtMs.WriteByte(Convert.ToByte(byt, 16));
                            }
                        }
                        // Is this useful?
                        else if(inSession)
                        {
                            sessPregMatch = sessPregRegex.Match(_line);
                            sessSubcMatch = sessSubcRegex.Match(_line);

                            if(sessPregMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PreGapMode at line {0}", line);
                            }
                            else if(sessSubcMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PreGapSubC at line {0}", line);
                            }
                        }
                        else if(inEntry)
                        {
                            entSessMatch = entSessRegex.Match(_line);
                            entPointMatch = entPointRegex.Match(_line);
                            entAdrMatch = entAdrRegex.Match(_line);
                            entCtrlMatch = entCtrlRegex.Match(_line);
                            entTnoMatch = entTnoRegex.Match(_line);
                            entAMinMatch = entAMinRegex.Match(_line);
                            entASecMatch = entASecRegex.Match(_line);
                            entAFrameMatch = entAFrameRegex.Match(_line);
                            entAlbaMatch = entAlbaRegex.Match(_line);
                            entZeroMatch = entZeroRegex.Match(_line);
                            entPMinMatch = entPMinRegex.Match(_line);
                            entPSecMatch = entPSecRegex.Match(_line);
                            entPFrameMatch = entPFrameRegex.Match(_line);
                            entPlbaMatch = entPlbaRegex.Match(_line);

                            if(entSessMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Session at line {0}", line);
                                currentEntry.SessionNumber = Convert.ToByte(entSessMatch.Groups["value"].Value, 10);
                                if(currentEntry.SessionNumber < minSession) minSession = currentEntry.SessionNumber;
                                if(currentEntry.SessionNumber > maxSession) maxSession = currentEntry.SessionNumber;
                            }
                            else if(entPointMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Point at line {0}", line);
                                currentEntry.POINT = Convert.ToByte(entPointMatch.Groups["value"].Value, 16);
                            }
                            else if(entAdrMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found ADR at line {0}", line);
                                currentEntry.ADR = Convert.ToByte(entAdrMatch.Groups["value"].Value, 16);
                            }
                            else if(entCtrlMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Control at line {0}", line);
                                currentEntry.CONTROL = Convert.ToByte(entCtrlMatch.Groups["value"].Value, 16);
                            }
                            else if(entTnoMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found TrackNo at line {0}", line);
                                currentEntry.TNO = Convert.ToByte(entTnoMatch.Groups["value"].Value, 10);
                            }
                            else if(entAMinMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found AMin at line {0}", line);
                                currentEntry.Min = Convert.ToByte(entAMinMatch.Groups["value"].Value, 10);
                            }
                            else if(entASecMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found ASec at line {0}", line);
                                currentEntry.Sec = Convert.ToByte(entASecMatch.Groups["value"].Value, 10);
                            }
                            else if(entAFrameMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found AFrame at line {0}", line);
                                currentEntry.Frame = Convert.ToByte(entAFrameMatch.Groups["value"].Value, 10);
                            }
                            else if(entAlbaMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found ALBA at line {0}", line);
                            }
                            else if(entZeroMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found Zero at line {0}", line);
                                currentEntry.Zero = Convert.ToByte(entZeroMatch.Groups["value"].Value, 10);
                                currentEntry.HOUR = (byte)((currentEntry.Zero & 0xF0) >> 4);
                                currentEntry.PHOUR = (byte)(currentEntry.Zero & 0x0F);
                            }
                            else if(entPMinMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PMin at line {0}", line);
                                currentEntry.PMIN = Convert.ToByte(entPMinMatch.Groups["value"].Value, 10);
                            }
                            else if(entPSecMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PSec at line {0}", line);
                                currentEntry.PSEC = Convert.ToByte(entPSecMatch.Groups["value"].Value, 10);
                            }
                            else if(entPFrameMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PFrame at line {0}", line);
                                currentEntry.PFRAME = Convert.ToByte(entPFrameMatch.Groups["value"].Value, 10);
                            }
                            else if(entPlbaMatch.Success)
                            {
                                DicConsole.DebugWriteLine("CloneCD plugin", "Found PLBA at line {0}", line);
                            }
                        }
                    }
                }

                if(inEntry) entries.Add(currentEntry);

                if(entries.Count == 0) throw new FeatureUnsupportedImageException("Did not find any track.");

                FullTOC.CDFullTOC toc;
                toc.TrackDescriptors = entries.ToArray();
                toc.LastCompleteSession = (byte)maxSession;
                toc.FirstCompleteSession = (byte)minSession;
                toc.DataLength = (ushort)(entries.Count * 11 + 2);
                MemoryStream tocMs = new MemoryStream();
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
                ImageInfo.ReadableMediaTags.Add(MediaTagType.CD_FullTOC);

                DicConsole.DebugWriteLine("CloneCD plugin", "{0}", FullTOC.Prettify(toc));

                string dataFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".img";
                string subFile = Path.GetFileNameWithoutExtension(imageFilter.GetBasePath()) + ".sub";

                FiltersList filtersList = new FiltersList();
                dataFilter = filtersList.GetFilter(dataFile);

                if(dataFilter == null) throw new Exception("Cannot open data file");

                filtersList = new FiltersList();
                subFilter = filtersList.GetFilter(subFile);

                int curSessionNo = 0;
                Track currentTrack = new Track();
                bool firstTrackInSession = true;
                tracks = new List<Track>();
                byte discType;
                ulong leadOutStart = 0;

                dataStream = dataFilter.GetDataForkStream();
                if(subFilter != null) subStream = subFilter.GetDataForkStream();

                foreach(FullTOC.TrackDataDescriptor descriptor in entries)
                {
                    if(descriptor.SessionNumber > curSessionNo)
                    {
                        curSessionNo = descriptor.SessionNumber;
                        if(!firstTrackInSession)
                        {
                            currentTrack.TrackEndSector = leadOutStart - 1;
                            tracks.Add(currentTrack);
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
                                    discType = descriptor.PSEC;
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
                                            tracks.Add(currentTrack);
                                        }
                                        else firstTrackInSession = false;

                                        currentTrack = new Track();
                                        currentTrack.TrackBytesPerSector = 2352;
                                        currentTrack.TrackFile = dataFilter.GetFilename();
                                        currentTrack.TrackFileType = scrambled ? "SCRAMBLED" : "BINARY";
                                        currentTrack.TrackFilter = dataFilter;
                                        currentTrack.TrackRawBytesPerSector = 2352;
                                        currentTrack.TrackSequence = descriptor.POINT;
                                        currentTrack.TrackStartSector =
                                            GetLba(descriptor.PHOUR, descriptor.PMIN, descriptor.PSEC,
                                                   descriptor.PFRAME);
                                        currentTrack.TrackFileOffset = currentTrack.TrackStartSector * 2352;
                                        currentTrack.TrackSession = descriptor.SessionNumber;

                                        // Need to check exact data type later
                                        if((TOC_CONTROL)(descriptor.CONTROL & 0x0D) == TOC_CONTROL.DataTrack ||
                                           (TOC_CONTROL)(descriptor.CONTROL & 0x0D) == TOC_CONTROL.DataTrackIncremental)
                                            currentTrack.TrackType = TrackType.Data;
                                        else currentTrack.TrackType = TrackType.Audio;

                                        if(subFilter != null)
                                        {
                                            currentTrack.TrackSubchannelFile = subFilter.GetFilename();
                                            currentTrack.TrackSubchannelFilter = subFilter;
                                            currentTrack.TrackSubchannelOffset = currentTrack.TrackStartSector * 96;
                                            currentTrack.TrackSubchannelType = TrackSubchannelType.Raw;
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
                                                    currentTrack.TrackType = TrackType.CdMode1;
                                                    if(!ImageInfo.ReadableSectorTags
                                                                 .Contains(SectorTagType.CdSectorSync))
                                                        ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSync);
                                                    if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                  .CdSectorHeader))
                                                        ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorHeader);
                                                    if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEcc)
                                                    ) ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEcc);
                                                    if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                  .CdSectorEccP))
                                                        ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccP);
                                                    if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                  .CdSectorEccQ))
                                                        ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEccQ);
                                                    if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorEdc)
                                                    ) ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorEdc);
                                                    if(ImageInfo.SectorSize < 2048) ImageInfo.SectorSize = 2048;
                                                }
                                                else if(sectTest[15] == 2)
                                                {
                                                    byte[] subHdr1 = new byte[4];
                                                    byte[] subHdr2 = new byte[4];
                                                    byte[] empHdr = new byte[4];

                                                    Array.Copy(sectTest, 16, subHdr1, 0, 4);
                                                    Array.Copy(sectTest, 20, subHdr2, 0, 4);

                                                    if(subHdr1.SequenceEqual(subHdr2) && !empHdr.SequenceEqual(subHdr1))
                                                    {
                                                        if((subHdr1[2] & 0x20) == 0x20)
                                                        {
                                                            currentTrack.TrackBytesPerSector = 2324;
                                                            currentTrack.TrackType = TrackType.CdMode2Form2;
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorSync)
                                                            )
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorSync);
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorHeader)
                                                            )
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorHeader);
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorSubHeader)
                                                            )
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorSubHeader);
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorEdc))
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorEdc);
                                                            if(ImageInfo.SectorSize < 2324) ImageInfo.SectorSize = 2324;
                                                        }
                                                        else
                                                        {
                                                            currentTrack.TrackBytesPerSector = 2048;
                                                            currentTrack.TrackType = TrackType.CdMode2Form1;
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorSync)
                                                            )
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorSync);
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorHeader)
                                                            )
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorHeader);
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorSubHeader)
                                                            )
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorSubHeader);
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorEcc))
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorEcc);
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorEccP)
                                                            )
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorEccP);
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorEccQ)
                                                            )
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorEccQ);
                                                            if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                          .CdSectorEdc))
                                                                ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                     .CdSectorEdc);
                                                            if(ImageInfo.SectorSize < 2048) ImageInfo.SectorSize = 2048;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        currentTrack.TrackBytesPerSector = 2336;
                                                        currentTrack.TrackType = TrackType.CdMode2Formless;
                                                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                      .CdSectorSync))
                                                            ImageInfo.ReadableSectorTags
                                                                     .Add(SectorTagType.CdSectorSync);
                                                        if(!ImageInfo.ReadableSectorTags.Contains(SectorTagType
                                                                                                      .CdSectorHeader))
                                                            ImageInfo.ReadableSectorTags.Add(SectorTagType
                                                                                                 .CdSectorHeader);
                                                        if(ImageInfo.SectorSize < 2336) ImageInfo.SectorSize = 2336;
                                                    }
                                                }
                                            }
                                        }
                                        else { if(ImageInfo.SectorSize < 2352) ImageInfo.SectorSize = 2352; }
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
                                        int frm = descriptor.PFRAME - type;

                                        ImageInfo.MediaManufacturer = ATIP.ManufacturerFromATIP(descriptor.PSEC, frm);

                                        if(ImageInfo.MediaManufacturer != "")
                                            DicConsole.DebugWriteLine("CloneCD plugin", "Disc manufactured by: {0}",
                                                                      ImageInfo.MediaManufacturer);
                                    }
                                    break;
                            }

                            break;
                        case 6:
                        {
                            uint id = (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame);
                            DicConsole.DebugWriteLine("CloneCD plugin", "Disc ID: {0:X6}", id & 0x00FFFFFF);
                            ImageInfo.MediaSerialNumber = string.Format("{0:X6}", id & 0x00FFFFFF);
                            break;
                        }
                    }
                }

                if(!firstTrackInSession)
                {
                    currentTrack.TrackEndSector = leadOutStart - 1;
                    tracks.Add(currentTrack);
                }

                if(subFilter != null && !ImageInfo.ReadableSectorTags.Contains(SectorTagType.CdSectorSubchannel))
                    ImageInfo.ReadableSectorTags.Add(SectorTagType.CdSectorSubchannel);

                sessions = new List<DiscImages.Session>();
                DiscImages.Session currentSession = new DiscImages.Session();
                currentSession.EndTrack = uint.MinValue;
                currentSession.StartTrack = uint.MaxValue;
                currentSession.SessionSequence = 1;
                partitions = new List<Partition>();
                offsetmap = new Dictionary<uint, ulong>();

                foreach(Track track in tracks)
                {
                    if(track.TrackSession == currentSession.SessionSequence)
                    {
                        if(track.TrackSequence > currentSession.EndTrack)
                        {
                            currentSession.EndSector = track.TrackEndSector;
                            currentSession.EndTrack = track.TrackSequence;
                        }

                        if(track.TrackSequence < currentSession.StartTrack)
                        {
                            currentSession.StartSector = track.TrackStartSector;
                            currentSession.StartTrack = track.TrackSequence;
                        }
                    }
                    else
                    {
                        sessions.Add(currentSession);
                        currentSession = new DiscImages.Session();
                        currentSession.EndTrack = uint.MinValue;
                        currentSession.StartTrack = uint.MaxValue;
                        currentSession.SessionSequence = track.TrackSession;
                    }

                    Partition partition = new Partition();
                    partition.Description = track.TrackDescription;
                    partition.Size = ((track.TrackEndSector - track.TrackStartSector) + 1) *
                                     (ulong)track.TrackRawBytesPerSector;
                    partition.Length = (track.TrackEndSector - track.TrackStartSector) + 1;
                    ImageInfo.Sectors += partition.Length;
                    partition.Sequence = track.TrackSequence;
                    partition.Offset = track.TrackFileOffset;
                    partition.Start = track.TrackStartSector;
                    partition.Type = track.TrackType.ToString();
                    partitions.Add(partition);
                    offsetmap.Add(track.TrackSequence, track.TrackStartSector);
                }

                bool data = false;
                bool mode2 = false;
                bool firstaudio = false;
                bool firstdata = false;
                bool audio = false;

                for(int i = 0; i < tracks.Count; i++)
                {
                    // First track is audio
                    firstaudio |= i == 0 && tracks[i].TrackType == TrackType.Audio;

                    // First track is data
                    firstdata |= i == 0 && tracks[i].TrackType != TrackType.Audio;

                    // Any non first track is data
                    data |= i != 0 && tracks[i].TrackType != TrackType.Audio;

                    // Any non first track is audio
                    audio |= i != 0 && tracks[i].TrackType == TrackType.Audio;

                    switch(tracks[i].TrackType)
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

                if(!data && !firstdata) ImageInfo.MediaType = MediaType.CDDA;
                else if(firstaudio && data && sessions.Count > 1 && mode2) ImageInfo.MediaType = MediaType.CDPLUS;
                else if((firstdata && audio) || mode2) ImageInfo.MediaType = MediaType.CDROMXA;
                else if(!audio) ImageInfo.MediaType = MediaType.CDROM;
                else ImageInfo.MediaType = MediaType.CD;

                ImageInfo.ImageApplication = "CloneCD";
                ImageInfo.ImageSize = (ulong)imageFilter.GetDataForkLength();
                ImageInfo.ImageCreationTime = imageFilter.GetCreationTime();
                ImageInfo.ImageLastModificationTime = imageFilter.GetLastWriteTime();
                ImageInfo.XmlMediaType = XmlMediaType.OpticalDisc;

                return true;
            }
            catch(Exception ex)
            {
                DicConsole.ErrorWriteLine("Exception trying to identify image file {0}", imageFilter.GetFilename());
                DicConsole.ErrorWriteLine("Exception: {0}", ex.Message);
                DicConsole.ErrorWriteLine("Stack trace: {0}", ex.StackTrace);
                return false;
            }
        }

        static ulong GetLba(int hour, int minute, int second, int frame)
        {
            return (ulong)((hour * 60 * 60 * 75) + (minute * 60 * 75) + (second * 75) + frame - 150);
        }

        public override bool ImageHasPartitions()
        {
            return ImageInfo.ImageHasPartitions;
        }

        public override ulong GetImageSize()
        {
            return ImageInfo.ImageSize;
        }

        public override ulong GetSectors()
        {
            return ImageInfo.Sectors;
        }

        public override uint GetSectorSize()
        {
            return ImageInfo.SectorSize;
        }

        public override byte[] ReadDiskTag(MediaTagType tag)
        {
            switch(tag)
            {
                case MediaTagType.CD_FullTOC:
                {
                    return fulltoc;
                }
                case MediaTagType.CD_TEXT:
                {
                    if(cdtext != null && cdtext.Length > 0) return cdtext;

                    throw new FeatureNotPresentImageException("Image does not contain CD-TEXT information.");
                }
                default:
                    throw new FeatureSupportedButNotImplementedImageException("Feature not supported by image format");
            }
        }

        public override byte[] ReadSector(ulong sectorAddress)
        {
            return ReadSectors(sectorAddress, 1);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, tag);
        }

        public override byte[] ReadSector(ulong sectorAddress, uint track)
        {
            return ReadSectors(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorTag(ulong sectorAddress, uint track, SectorTagType tag)
        {
            return ReadSectorsTag(sectorAddress, 1, track, tag);
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(Track _track in tracks)
                    {
                        if(_track.TrackSequence == kvp.Key)
                        {
                            if(sectorAddress <= _track.TrackEndSector)
                                return ReadSectors((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                  string.Format("Sector address {0} not found", sectorAddress));
        }

        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, SectorTagType tag)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(Track _track in tracks)
                    {
                        if(_track.TrackSequence == kvp.Key)
                        {
                            if(sectorAddress <= _track.TrackEndSector)
                                return ReadSectorsTag((sectorAddress - kvp.Value), length, kvp.Key, tag);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                  string.Format("Sector address {0} not found", sectorAddress));
        }

        public override byte[] ReadSectors(ulong sectorAddress, uint length, uint track)
        {
            Track _track = new Track();

            _track.TrackSequence = 0;

            foreach(Track __track in tracks)
            {
                if(__track.TrackSequence == track)
                {
                    _track = __track;
                    break;
                }
            }

            if(_track.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if((length + sectorAddress) - 1 > _track.TrackEndSector)
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                          .Format("Requested more sectors ({0} {2}) than present in track ({1}), won't cross tracks",
                                                                  length + sectorAddress, _track.TrackEndSector,
                                                                  sectorAddress));

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(_track.TrackType)
            {
                case TrackType.Audio:
                {
                    sectorOffset = 0;
                    sectorSize = 2352;
                    sectorSkip = 0;
                    break;
                }
                case TrackType.CdMode1:
                {
                    sectorOffset = 16;
                    sectorSize = 2048;
                    sectorSkip = 288;
                    break;
                }
                case TrackType.CdMode2Formless:
                {
                    sectorOffset = 16;
                    sectorSize = 2336;
                    sectorSkip = 0;
                    break;
                }
                case TrackType.CdMode2Form1:
                {
                    sectorOffset = 24;
                    sectorSize = 2048;
                    sectorSkip = 280;
                    break;
                }
                case TrackType.CdMode2Form2:
                {
                    sectorOffset = 24;
                    sectorSize = 2324;
                    sectorSkip = 4;
                    break;
                }
                default: throw new FeatureSupportedButNotImplementedImageException("Unsupported track type");
            }

            byte[] buffer = new byte[sectorSize * length];

            dataStream.Seek((long)(_track.TrackFileOffset + sectorAddress * 2352), SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) dataStream.Read(buffer, 0, buffer.Length);
            else
            {
                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    dataStream.Seek(sectorOffset, SeekOrigin.Current);
                    dataStream.Read(sector, 0, sector.Length);
                    dataStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }
            }

            return buffer;
        }

        // TODO: Flags
        public override byte[] ReadSectorsTag(ulong sectorAddress, uint length, uint track, SectorTagType tag)
        {
            Track _track = new Track();

            _track.TrackSequence = 0;

            foreach(Track __track in tracks)
            {
                if(__track.TrackSequence == track)
                {
                    _track = __track;
                    break;
                }
            }

            if(_track.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if((length + sectorAddress) - 1 > (_track.TrackEndSector))
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                          .Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks",
                                                                  length + sectorAddress, _track.TrackEndSector));

            if(_track.TrackType == TrackType.Data)
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
                case SectorTagType.CdSectorSubchannel:
                    buffer = new byte[96 * length];
                    subStream.Seek((long)(_track.TrackSubchannelOffset + sectorAddress * 96), SeekOrigin.Begin);
                    subStream.Read(buffer, 0, buffer.Length);
                    return buffer;
                default: throw new ArgumentException("Unsupported tag requested", nameof(tag));
            }

            uint sectorOffset;
            uint sectorSize;
            uint sectorSkip;

            switch(_track.TrackType)
            {
                case TrackType.CdMode1:
                    switch(tag)
                    {
                        case SectorTagType.CdSectorSync:
                        {
                            sectorOffset = 0;
                            sectorSize = 12;
                            sectorSkip = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize = 4;
                            sectorSkip = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                            throw new ArgumentException("Unsupported tag requested for this track", nameof(tag));
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize = 276;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize = 172;
                            sectorSkip = 104;
                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize = 104;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2064;
                            sectorSize = 4;
                            sectorSkip = 284;
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
                            sectorSize = 8;
                            sectorSkip = 2328;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2332;
                            sectorSize = 4;
                            sectorSkip = 0;
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
                            sectorSize = 12;
                            sectorSkip = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize = 4;
                            sectorSkip = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sectorOffset = 16;
                            sectorSize = 8;
                            sectorSkip = 2328;
                            break;
                        }
                        case SectorTagType.CdSectorEcc:
                        {
                            sectorOffset = 2076;
                            sectorSize = 276;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEccP:
                        {
                            sectorOffset = 2076;
                            sectorSize = 172;
                            sectorSkip = 104;
                            break;
                        }
                        case SectorTagType.CdSectorEccQ:
                        {
                            sectorOffset = 2248;
                            sectorSize = 104;
                            sectorSkip = 0;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2072;
                            sectorSize = 4;
                            sectorSkip = 276;
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
                            sectorSize = 12;
                            sectorSkip = 2340;
                            break;
                        }
                        case SectorTagType.CdSectorHeader:
                        {
                            sectorOffset = 12;
                            sectorSize = 4;
                            sectorSkip = 2336;
                            break;
                        }
                        case SectorTagType.CdSectorSubHeader:
                        {
                            sectorOffset = 16;
                            sectorSize = 8;
                            sectorSkip = 2328;
                            break;
                        }
                        case SectorTagType.CdSectorEdc:
                        {
                            sectorOffset = 2348;
                            sectorSize = 4;
                            sectorSkip = 0;
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

            dataStream.Seek((long)(_track.TrackFileOffset + sectorAddress * 2352), SeekOrigin.Begin);
            if(sectorOffset == 0 && sectorSkip == 0) dataStream.Read(buffer, 0, buffer.Length);
            else
            {
                for(int i = 0; i < length; i++)
                {
                    byte[] sector = new byte[sectorSize];
                    dataStream.Seek(sectorOffset, SeekOrigin.Current);
                    dataStream.Read(sector, 0, sector.Length);
                    dataStream.Seek(sectorSkip, SeekOrigin.Current);
                    Array.Copy(sector, 0, buffer, i * sectorSize, sectorSize);
                }
            }

            return buffer;
        }

        public override byte[] ReadSectorLong(ulong sectorAddress)
        {
            return ReadSectorsLong(sectorAddress, 1);
        }

        public override byte[] ReadSectorLong(ulong sectorAddress, uint track)
        {
            return ReadSectorsLong(sectorAddress, 1, track);
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length)
        {
            foreach(KeyValuePair<uint, ulong> kvp in offsetmap)
            {
                if(sectorAddress >= kvp.Value)
                {
                    foreach(Track track in tracks)
                    {
                        if(track.TrackSequence == kvp.Key)
                        {
                            if((sectorAddress - kvp.Value) < ((track.TrackEndSector - track.TrackStartSector) + 1))
                                return ReadSectorsLong((sectorAddress - kvp.Value), length, kvp.Key);
                        }
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(sectorAddress),
                                                  string.Format("Sector address {0} not found", sectorAddress));
        }

        public override byte[] ReadSectorsLong(ulong sectorAddress, uint length, uint track)
        {
            Track _track = new Track();

            _track.TrackSequence = 0;

            foreach(Track __track in tracks)
            {
                if(__track.TrackSequence == track)
                {
                    _track = __track;
                    break;
                }
            }

            if(_track.TrackSequence == 0)
                throw new ArgumentOutOfRangeException(nameof(track), "Track does not exist in disc image");

            if((length + sectorAddress) - 1 > (_track.TrackEndSector))
                throw new ArgumentOutOfRangeException(nameof(length),
                                                      string
                                                          .Format("Requested more sectors ({0}) than present in track ({1}), won't cross tracks",
                                                                  length + sectorAddress, _track.TrackEndSector));

            byte[] buffer = new byte[2352 * length];

            dataStream.Seek((long)(_track.TrackFileOffset + sectorAddress * 2352), SeekOrigin.Begin);
            dataStream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public override string GetImageFormat()
        {
            return "CloneCD";
        }

        public override string GetImageVersion()
        {
            return ImageInfo.ImageVersion;
        }

        public override string GetImageApplication()
        {
            return ImageInfo.ImageApplication;
        }

        public override string GetImageApplicationVersion()
        {
            return ImageInfo.ImageApplicationVersion;
        }

        public override string GetImageCreator()
        {
            return ImageInfo.ImageCreator;
        }

        public override DateTime GetImageCreationTime()
        {
            return ImageInfo.ImageCreationTime;
        }

        public override DateTime GetImageLastModificationTime()
        {
            return ImageInfo.ImageLastModificationTime;
        }

        public override string GetImageName()
        {
            return ImageInfo.ImageName;
        }

        public override string GetImageComments()
        {
            return ImageInfo.ImageComments;
        }

        public override string GetMediaManufacturer()
        {
            return ImageInfo.MediaManufacturer;
        }

        public override string GetMediaModel()
        {
            return ImageInfo.MediaModel;
        }

        public override string GetMediaSerialNumber()
        {
            return ImageInfo.DriveSerialNumber;
        }

        public override string GetMediaBarcode()
        {
            return ImageInfo.MediaBarcode;
        }

        public override string GetMediaPartNumber()
        {
            return ImageInfo.MediaPartNumber;
        }

        public override MediaType GetMediaType()
        {
            return ImageInfo.MediaType;
        }

        public override int GetMediaSequence()
        {
            return ImageInfo.MediaSequence;
        }

        public override int GetLastDiskSequence()
        {
            return ImageInfo.LastMediaSequence;
        }

        public override string GetDriveManufacturer()
        {
            return ImageInfo.DriveManufacturer;
        }

        public override string GetDriveModel()
        {
            return ImageInfo.DriveModel;
        }

        public override string GetDriveSerialNumber()
        {
            return ImageInfo.DriveSerialNumber;
        }

        public override List<Partition> GetPartitions()
        {
            return partitions;
        }

        public override List<Track> GetTracks()
        {
            return tracks;
        }

        public override List<Track> GetSessionTracks(DiscImages.Session session)
        {
            if(sessions.Contains(session)) { return GetSessionTracks(session.SessionSequence); }

            throw new ImageNotSupportedException("Session does not exist in disc image");
        }

        public override List<Track> GetSessionTracks(ushort session)
        {
            List<Track> tracks = new List<Track>();
            foreach(Track _track in this.tracks) { if(_track.TrackSession == session) tracks.Add(_track); }

            return tracks;
        }

        public override List<DiscImages.Session> GetSessions()
        {
            return sessions;
        }

        public override bool? VerifySector(ulong sectorAddress)
        {
            byte[] buffer = ReadSectorLong(sectorAddress);
            return Checksums.CdChecksums.CheckCdSector(buffer);
        }

        public override bool? VerifySector(ulong sectorAddress, uint track)
        {
            byte[] buffer = ReadSectorLong(sectorAddress, track);
            return Checksums.CdChecksums.CheckCdSector(buffer);
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CdChecksums.CheckCdSector(sector);

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
            if(failingLbas.Count > 0) return false;

            return true;
        }

        public override bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                            out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int bps = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = Checksums.CdChecksums.CheckCdSector(sector);

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
            if(failingLbas.Count > 0) return false;

            return true;
        }

        public override bool? VerifyMediaImage()
        {
            return null;
        }
    }
}