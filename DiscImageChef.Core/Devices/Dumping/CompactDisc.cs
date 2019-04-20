// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CompactDisc.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Dumps CDs and DDCDs.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Extents;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Core.Media.Detection;
using DiscImageChef.Decoders.CD;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.MMC;
using DiscImageChef.Devices;
using Schemas;
using MediaType = DiscImageChef.CommonTypes.MediaType;
using PlatformID = DiscImageChef.CommonTypes.Interop.PlatformID;
using Session = DiscImageChef.Decoders.CD.Session;
using TrackType = DiscImageChef.CommonTypes.Enums.TrackType;

namespace DiscImageChef.Core.Devices.Dumping
{
    /// <summary>
    ///     Implement dumping Compact Discs
    /// </summary>
    // TODO: Barcode and pregaps
    partial class Dump
    {
        /// <summary>
        ///     Dumps a compact disc
        /// </summary>
        /// <param name="dskType">Disc type as detected in MMC layer</param>
        internal void CompactDisc(ref MediaType dskType, bool dumpFirstTrackPregap)
        {
            uint               subSize;
            DateTime           start;
            DateTime           end;
            bool               readcd;
            bool               read6         = false, read10 = false, read12 = false, read16 = false;
            bool               sense         = false;
            const uint         SECTOR_SIZE   = 2352;
            FullTOC.CDFullTOC? toc           = null;
            double             totalDuration = 0;
            double             currentSpeed  = 0;
            double             maxSpeed      = double.MinValue;
            double             minSpeed      = double.MaxValue;
            uint               blocksToRead  = 64;
            bool               aborted       = false;
            System.Console.CancelKeyPress += (sender, e) => e.Cancel = aborted = true;
            Dictionary<MediaTagType, byte[]> mediaTags = new Dictionary<MediaTagType, byte[]>();

            if(dumpRaw)
            {
                dumpLog.WriteLine("Raw CD dumping not yet implemented");
                StoppingErrorMessage?.Invoke("Raw CD dumping not yet implemented");
                return;
            }

            dskType = MediaType.CD;
            int sessions = 1;

            // We discarded all discs that falsify a TOC before requesting a real TOC
            // No TOC, no CD (or an empty one)
            dumpLog.WriteLine("Reading full TOC");
            UpdateStatus?.Invoke("Reading full TOC");
            bool tocSense = dev.ReadRawToc(out byte[] cmdBuf, out byte[] senseBuf, 0, dev.Timeout, out _);
            if(!tocSense)
            {
                toc = FullTOC.Decode(cmdBuf);
                if(toc.HasValue)
                {
                    byte[] tmpBuf = new byte[cmdBuf.Length - 2];
                    Array.Copy(cmdBuf, 2, tmpBuf, 0, cmdBuf.Length - 2);
                    mediaTags.Add(MediaTagType.CD_FullTOC, tmpBuf);

                    // ATIP exists on blank CDs
                    dumpLog.WriteLine("Reading ATIP");
                    UpdateStatus?.Invoke("Reading ATIP");
                    sense = dev.ReadAtip(out cmdBuf, out senseBuf, dev.Timeout, out _);
                    if(!sense)
                    {
                        ATIP.CDATIP? atip = ATIP.Decode(cmdBuf);
                        if(atip.HasValue)
                        {
                            // Only CD-R and CD-RW have ATIP
                            dskType = atip.Value.DiscType ? MediaType.CDRW : MediaType.CDR;

                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.CD_ATIP, tmpBuf);
                        }
                    }

                    dumpLog.WriteLine("Reading Disc Information");
                    UpdateStatus?.Invoke("Reading Disc Information");
                    sense = dev.ReadDiscInformation(out cmdBuf, out senseBuf,
                                                    MmcDiscInformationDataTypes.DiscInformation, dev.Timeout, out _);
                    if(!sense)
                    {
                        DiscInformation.StandardDiscInformation? discInfo = DiscInformation.Decode000b(cmdBuf);
                        if(discInfo.HasValue)
                            if(dskType == MediaType.CD)
                                switch(discInfo.Value.DiscType)
                                {
                                    case 0x10:
                                        dskType = MediaType.CDI;
                                        break;
                                    case 0x20:
                                        dskType = MediaType.CDROMXA;
                                        break;
                                }
                    }

                    int firstTrackLastSession = 0;

                    dumpLog.WriteLine("Reading Session Information");
                    UpdateStatus?.Invoke("Reading Session Information");
                    sense = dev.ReadSessionInfo(out cmdBuf, out senseBuf, dev.Timeout, out _);
                    if(!sense)
                    {
                        Session.CDSessionInfo? session = Session.Decode(cmdBuf);
                        if(session.HasValue)
                        {
                            sessions              = session.Value.LastCompleteSession;
                            firstTrackLastSession = session.Value.TrackDescriptors[0].TrackNumber;
                        }
                    }

                    if(dskType == MediaType.CD || dskType == MediaType.CDROMXA)
                    {
                        bool hasDataTrack                  = false;
                        bool hasAudioTrack                 = false;
                        bool allFirstSessionTracksAreAudio = true;
                        bool hasVideoTrack                 = false;

                        foreach(FullTOC.TrackDataDescriptor track in toc.Value.TrackDescriptors)
                        {
                            if(track.TNO == 1 && ((TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrack ||
                                                  (TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrackIncremental)
                            ) allFirstSessionTracksAreAudio &= firstTrackLastSession != 1;

                            if((TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrack ||
                               (TocControl)(track.CONTROL & 0x0D) == TocControl.DataTrackIncremental)
                            {
                                hasDataTrack                  =  true;
                                allFirstSessionTracksAreAudio &= track.POINT >= firstTrackLastSession;
                            }
                            else hasAudioTrack = true;

                            hasVideoTrack |= track.ADR == 4;
                        }

                        if(hasDataTrack && hasAudioTrack && allFirstSessionTracksAreAudio && sessions == 2)
                            dskType = MediaType.CDPLUS;
                        if(!hasDataTrack && hasAudioTrack && sessions == 1) dskType = MediaType.CDDA;
                        if(hasDataTrack && !hasAudioTrack && sessions == 1) dskType = MediaType.CDROM;
                        if(hasVideoTrack && !hasDataTrack && sessions == 1) dskType = MediaType.CDV;
                    }

                    dumpLog.WriteLine("Reading PMA");
                    UpdateStatus?.Invoke("Reading PMA");
                    sense = dev.ReadPma(out cmdBuf, out senseBuf, dev.Timeout, out _);
                    if(!sense)
                        if(PMA.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.CD_PMA, tmpBuf);
                        }

                    dumpLog.WriteLine("Reading CD-Text from Lead-In");
                    UpdateStatus?.Invoke("Reading CD-Text from Lead-In");
                    sense = dev.ReadCdText(out cmdBuf, out senseBuf, dev.Timeout, out _);
                    if(!sense)
                        if(CDTextOnLeadIn.Decode(cmdBuf).HasValue)
                        {
                            tmpBuf = new byte[cmdBuf.Length - 4];
                            Array.Copy(cmdBuf, 4, tmpBuf, 0, cmdBuf.Length - 4);
                            mediaTags.Add(MediaTagType.CD_TEXT, tmpBuf);
                        }
                }
            }

            // TODO: Add other detectors here
            dumpLog.WriteLine("Detecting disc type...");
            UpdateStatus?.Invoke("Detecting disc type...");
            byte[] videoNowColorFrame = new byte[9 * 2352];
            for(int i = 0; i < 9; i++)
            {
                sense = dev.ReadCd(out cmdBuf, out senseBuf, (uint)i, 2352, 1, MmcSectorTypes.AllTypes, false, false,
                                   true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.None,
                                   dev.Timeout, out _);

                if(sense || dev.Error)
                {
                    sense = dev.ReadCd(out cmdBuf, out senseBuf, (uint)i, 2352, 1, MmcSectorTypes.Cdda, false, false,
                                       true, MmcHeaderCodes.None, true, true, MmcErrorField.None, MmcSubchannel.None,
                                       dev.Timeout, out _);

                    if(sense || !dev.Error)
                    {
                        videoNowColorFrame = null;
                        break;
                    }
                }

                Array.Copy(cmdBuf, 0, videoNowColorFrame, i * 2352, 2352);
            }

            if(MMC.IsVideoNowColor(videoNowColorFrame)) dskType = MediaType.VideoNowColor;

            MmcSubchannel supportedSubchannel = MmcSubchannel.Raw;
            dumpLog.WriteLine("Checking if drive supports full raw subchannel reading...");
            UpdateStatus?.Invoke("Checking if drive supports full raw subchannel reading...");
            readcd = !dev.ReadCd(out byte[] readBuffer, out senseBuf, 0, SECTOR_SIZE + 96, 1, MmcSectorTypes.AllTypes,
                                 false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                 supportedSubchannel, dev.Timeout, out _);
            if(readcd)
            {
                dumpLog.WriteLine("Full raw subchannel reading supported...");
                UpdateStatus?.Invoke("Full raw subchannel reading supported...");
                subSize = 96;
            }
            else
            {
                supportedSubchannel = MmcSubchannel.Q16;
                dumpLog.WriteLine("Checking if drive supports PQ subchannel reading...");
                UpdateStatus?.Invoke("Checking if drive supports PQ subchannel reading...");
                readcd = !dev.ReadCd(out readBuffer, out senseBuf, 0, SECTOR_SIZE + 16, 1, MmcSectorTypes.AllTypes,
                                     false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                     supportedSubchannel, dev.Timeout, out _);

                if(readcd)
                {
                    dumpLog.WriteLine("PQ subchannel reading supported...");
                    dumpLog.WriteLine("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");
                    UpdateStatus?.Invoke("PQ subchannel reading supported...");
                    UpdateStatus
                      ?.Invoke("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");
                    subSize = 16;
                }
                else
                {
                    supportedSubchannel = MmcSubchannel.None;
                    dumpLog.WriteLine("Checking if drive supports reading without subchannel...");
                    UpdateStatus?.Invoke("Checking if drive supports reading without subchannel...");
                    readcd = !dev.ReadCd(out readBuffer, out senseBuf, 0, SECTOR_SIZE, 1, MmcSectorTypes.AllTypes,
                                         false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                         supportedSubchannel, dev.Timeout, out _);

                    if(!readcd)
                    {
                        dumpLog.WriteLine("Drive does not support READ CD, trying SCSI READ commands...");
                        ErrorMessage?.Invoke("Drive does not support READ CD, trying SCSI READ commands...");

                        dumpLog.WriteLine("Checking if drive supports READ(6)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(6)...");
                        read6 = !dev.Read6(out readBuffer, out senseBuf, 0, 2048, 1, dev.Timeout, out _);
                        dumpLog.WriteLine("Checking if drive supports READ(10)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(10)...");
                        read10 = !dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, 0, 2048, 0, 1,
                                             dev.Timeout, out _);
                        dumpLog.WriteLine("Checking if drive supports READ(12)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(12)...");
                        read12 = !dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, 0, 2048, 0, 1,
                                             false, dev.Timeout, out _);
                        dumpLog.WriteLine("Checking if drive supports READ(16)...");
                        UpdateStatus?.Invoke("Checking if drive supports READ(16)...");
                        read16 = !dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, 0, 2048, 0, 1, false,
                                             dev.Timeout, out _);

                        if(!read6 && !read10 && !read12 && !read16)
                        {
                            dumpLog.WriteLine("Cannot read from disc, not continuing...");
                            StoppingErrorMessage?.Invoke("Cannot read from disc, not continuing...");
                            return;
                        }

                        if(read6)
                        {
                            dumpLog.WriteLine("Drive supports READ(6)...");
                            UpdateStatus?.Invoke("Drive supports READ(6)...");
                        }

                        if(read10)
                        {
                            dumpLog.WriteLine("Drive supports READ(10)...");
                            UpdateStatus?.Invoke("Drive supports READ(10)...");
                        }

                        if(read12)
                        {
                            dumpLog.WriteLine("Drive supports READ(12)...");
                            UpdateStatus?.Invoke("Drive supports READ(12)...");
                        }

                        if(read16)
                        {
                            dumpLog.WriteLine("Drive supports READ(16)...");
                            UpdateStatus?.Invoke("Drive supports READ(16)...");
                        }
                    }

                    dumpLog.WriteLine("Drive can only read without subchannel...");
                    dumpLog.WriteLine("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");
                    UpdateStatus?.Invoke("Drive can only read without subchannel...");
                    UpdateStatus
                      ?.Invoke("WARNING: If disc says CD+G, CD+EG, CD-MIDI, CD Graphics or CD Enhanced Graphics, dump will be incorrect!");
                    subSize = 0;
                }
            }

            // Check if output format supports subchannels
            if(!outputPlugin.SupportedSectorTags.Contains(SectorTagType.CdSectorSubchannel) &&
               supportedSubchannel != MmcSubchannel.None)
            {
                if(!force)
                {
                    dumpLog.WriteLine("Output format does not support subchannels, continuing...");
                    UpdateStatus?.Invoke("Output format does not support subchannels, continuing...");
                }
                else
                {
                    dumpLog.WriteLine("Output format does not support subchannels, not continuing...");
                    StoppingErrorMessage?.Invoke("Output format does not support subchannels, not continuing...");
                    return;
                }

                supportedSubchannel = MmcSubchannel.None;
                subSize             = 0;
            }

            TrackSubchannelType subType;

            switch(supportedSubchannel)
            {
                case MmcSubchannel.None:
                    subType = TrackSubchannelType.None;
                    break;
                case MmcSubchannel.Raw:
                    subType = TrackSubchannelType.Raw;
                    break;
                case MmcSubchannel.Q16:
                    subType = TrackSubchannelType.Q16;
                    break;
                default:
                    dumpLog.WriteLine("Handling subchannel type {0} not supported, exiting...", supportedSubchannel);
                    StoppingErrorMessage
                      ?.Invoke($"Handling subchannel type {supportedSubchannel} not supported, exiting...");
                    return;
            }

            uint blockSize = SECTOR_SIZE + subSize;

            UpdateStatus?.Invoke("Building track map...");
            dumpLog.WriteLine("Building track map...");

            List<Track>            trackList      = new List<Track>();
            long                   lastSector     = 0;
            Dictionary<byte, byte> trackFlags     = new Dictionary<byte, byte>();
            TrackType              firstTrackType = TrackType.Audio;
            Dictionary<int, long>  leadOutStarts  = new Dictionary<int, long>();

            if(toc.HasValue)
            {
                FullTOC.TrackDataDescriptor[] sortedTracks =
                    toc.Value.TrackDescriptors.OrderBy(track => track.POINT).ToArray();

                foreach(FullTOC.TrackDataDescriptor trk in sortedTracks.Where(trk => trk.ADR == 1 || trk.ADR == 4))
                    if(trk.POINT >= 0x01 && trk.POINT <= 0x63)
                    {
                        trackList.Add(new Track
                        {
                            TrackSequence = trk.POINT,
                            TrackSession  = trk.SessionNumber,
                            TrackType =
                                (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                    ? TrackType.Data
                                    : TrackType.Audio,
                            TrackStartSector =
                                (ulong)(trk.PHOUR * 3600 * 75 + trk.PMIN * 60 * 75 + trk.PSEC * 75 +
                                        trk.PFRAME - 150),
                            TrackBytesPerSector    = (int)SECTOR_SIZE,
                            TrackRawBytesPerSector = (int)SECTOR_SIZE,
                            TrackSubchannelType    = subType
                        });
                        trackFlags.Add(trk.POINT, trk.CONTROL);
                    }
                    else if(trk.POINT == 0xA2)
                    {
                        int phour, pmin, psec, pframe;
                        if(trk.PFRAME == 0)
                        {
                            pframe = 74;

                            if(trk.PSEC == 0)
                            {
                                psec = 59;

                                if(trk.PMIN == 0)
                                {
                                    pmin  = 59;
                                    phour = trk.PHOUR - 1;
                                }
                                else
                                {
                                    pmin  = trk.PMIN - 1;
                                    phour = trk.PHOUR;
                                }
                            }
                            else
                            {
                                psec  = trk.PSEC - 1;
                                pmin  = trk.PMIN;
                                phour = trk.PHOUR;
                            }
                        }
                        else
                        {
                            pframe = trk.PFRAME - 1;
                            psec   = trk.PSEC;
                            pmin   = trk.PMIN;
                            phour  = trk.PHOUR;
                        }

                        lastSector = phour * 3600 * 75 + pmin * 60 * 75 + psec * 75 + pframe - 150;
                        leadOutStarts.Add(trk.SessionNumber, lastSector + 1);
                    }
                    else if(trk.POINT == 0xA0 && trk.ADR == 1)
                    {
                        switch(trk.PSEC)
                        {
                            case 0x10:
                                dskType = MediaType.CDI;
                                break;
                            case 0x20:
                                if(dskType == MediaType.CD) dskType = MediaType.CDROMXA;
                                break;
                        }

                        firstTrackType =
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                ? TrackType.Data
                                : TrackType.Audio;
                    }
            }
            else
            {
                UpdateStatus?.Invoke("Cannot read RAW TOC, requesting processed one...");
                dumpLog.WriteLine("Cannot read RAW TOC, requesting processed one...");
                tocSense = dev.ReadToc(out cmdBuf, out senseBuf, false, 0, dev.Timeout, out _);

                TOC.CDTOC? oldToc = TOC.Decode(cmdBuf);
                if((tocSense || !oldToc.HasValue) && !force)
                {
                    dumpLog.WriteLine("Could not read TOC, if you want to continue, use force, and will try from LBA 0 to 360000...");
                    StoppingErrorMessage
                      ?.Invoke("Could not read TOC, if you want to continue, use force, and will try from LBA 0 to 360000...");
                    return;
                }

                foreach(TOC.CDTOCTrackDataDescriptor trk in oldToc
                                                           .Value.TrackDescriptors.OrderBy(t => t.TrackNumber)
                                                           .Where(trk => trk.ADR == 1 || trk.ADR == 4))
                    if(trk.TrackNumber >= 0x01 && trk.TrackNumber <= 0x63)
                    {
                        trackList.Add(new Track
                        {
                            TrackSequence = trk.TrackNumber,
                            TrackSession  = 1,
                            TrackType =
                                (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                    ? TrackType.Data
                                    : TrackType.Audio,
                            TrackStartSector       = trk.TrackStartAddress,
                            TrackBytesPerSector    = (int)SECTOR_SIZE,
                            TrackRawBytesPerSector = (int)SECTOR_SIZE,
                            TrackSubchannelType    = subType
                        });
                        trackFlags.Add(trk.TrackNumber, trk.CONTROL);
                    }
                    else if(trk.TrackNumber == 0xAA)
                    {
                        firstTrackType =
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                ? TrackType.Data
                                : TrackType.Audio;
                        lastSector = trk.TrackStartAddress - 1;
                    }
            }

            if(trackList.Count == 0)
            {
                UpdateStatus?.Invoke("No tracks found, adding a single track from 0 to Lead-Out");
                dumpLog.WriteLine("No tracks found, adding a single track from 0 to Lead-Out");

                trackList.Add(new Track
                {
                    TrackSequence          = 1,
                    TrackSession           = 1,
                    TrackType              = firstTrackType,
                    TrackStartSector       = 0,
                    TrackBytesPerSector    = (int)SECTOR_SIZE,
                    TrackRawBytesPerSector = (int)SECTOR_SIZE,
                    TrackSubchannelType    = subType
                });
                trackFlags.Add(1, (byte)(firstTrackType == TrackType.Audio ? 0 : 4));
            }

            if(lastSector == 0)
            {
                sense = dev.ReadCapacity16(out readBuffer, out senseBuf, dev.Timeout, out _);
                if(!sense)
                {
                    byte[] temp = new byte[8];

                    Array.Copy(cmdBuf, 0, temp, 0, 8);
                    Array.Reverse(temp);
                    lastSector = (long)BitConverter.ToUInt64(temp, 0);
                    blockSize  = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + cmdBuf[7]);
                }
                else
                {
                    sense = dev.ReadCapacity(out cmdBuf, out senseBuf, dev.Timeout, out _);
                    if(!sense)
                    {
                        lastSector = (cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3];
                        blockSize  = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + cmdBuf[7]);
                    }
                }

                if(lastSector <= 0)
                {
                    if(!force)
                    {
                        StoppingErrorMessage
                          ?.Invoke("Could not find Lead-Out, if you want to continue use force option and will continue until 360000 sectors...");
                        dumpLog.WriteLine("Could not find Lead-Out, if you want to continue use force option and will continue until 360000 sectors...");
                        return;
                    }

                    UpdateStatus
                      ?.Invoke("WARNING: Could not find Lead-Out start, will try to read up to 360000 sectors, probably will fail before...");
                    dumpLog.WriteLine("WARNING: Could not find Lead-Out start, will try to read up to 360000 sectors, probably will fail before...");
                    lastSector = 360000;
                }
            }

            Track[] tracks                                                      = trackList.ToArray();
            for(int t = 1; t < tracks.Length; t++) tracks[t - 1].TrackEndSector = tracks[t].TrackStartSector - 1;

            tracks[tracks.Length              - 1].TrackEndSector = (ulong)lastSector;
            ulong blocks = (ulong)(lastSector + 1);

            if(blocks == 0)
            {
                StoppingErrorMessage?.Invoke("Cannot dump blank media.");
                return;
            }

            ExtentsULong leadOutExtents = new ExtentsULong();

            if(leadOutStarts.Any())
            {
                UpdateStatus?.Invoke("Solving lead-outs...");
                foreach(KeyValuePair<int, long> leadOuts in leadOutStarts)
                    for(int i = 0; i < tracks.Length; i++)
                    {
                        if(tracks[i].TrackSession != leadOuts.Key) continue;

                        if(tracks[i].TrackEndSector >= (ulong)leadOuts.Value)
                            tracks[i].TrackEndSector = (ulong)leadOuts.Value - 1;
                    }

                ExtentsULong dataExtents = new ExtentsULong();
                foreach(Track trk in tracks) dataExtents.Add(trk.TrackStartSector, trk.TrackEndSector);

                Tuple<ulong, ulong>[] dataExtentsArray = dataExtents.ToArray();
                for(int i = 0; i < dataExtentsArray.Length - 1; i++)
                    leadOutExtents.Add(dataExtentsArray[i].Item2 + 1, dataExtentsArray[i + 1].Item1 - 1);
            }

            // Check if output format supports all disc tags we have retrieved so far
            foreach(MediaTagType tag in mediaTags.Keys)
            {
                if(outputPlugin.SupportedMediaTags.Contains(tag)) continue;

                if(!force)
                {
                    dumpLog.WriteLine("Output format does not support {0}, continuing...", tag);
                    ErrorMessage?.Invoke($"Output format does not support {tag}, continuing...");
                }
                else
                {
                    dumpLog.WriteLine("Output format does not support {0}, not continuing...", tag);
                    StoppingErrorMessage?.Invoke($"Output format does not support {tag}, not continuing...");
                    return;
                }
            }

            // Check for hidden data before start of track 1
            if(tracks.First(t => t.TrackSequence == 1).TrackStartSector > 0 && readcd)
            {
                dumpLog.WriteLine("First track starts after sector 0, checking for a hidden track...");
                UpdateStatus?.Invoke("First track starts after sector 0, checking for a hidden track...");

                sense = dev.ReadCd(out readBuffer, out senseBuf, 0, blockSize, 1, MmcSectorTypes.AllTypes, false, false,
                                   true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, supportedSubchannel,
                                   dev.Timeout, out _);

                if(dev.Error || sense)
                {
                    dumpLog.WriteLine("Could not read sector 0, continuing...");
                    UpdateStatus?.Invoke("Could not read sector 0, continuing...");
                }
                else
                {
                    byte[] syncMark = {0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00};
                    byte[] cdiMark  = {0x01, 0x43, 0x44, 0x2D};
                    byte[] testMark = new byte[12];
                    Array.Copy(readBuffer, 0, testMark, 0, 12);

                    bool hiddenData = syncMark.SequenceEqual(testMark) &&
                                      (readBuffer[0xF] == 0 || readBuffer[0xF] == 1 || readBuffer[0xF] == 2);

                    if(hiddenData && readBuffer[0xF] == 2)
                    {
                        sense = dev.ReadCd(out readBuffer, out senseBuf, 16, blockSize, 1, MmcSectorTypes.AllTypes,
                                           false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                           MmcErrorField.None, supportedSubchannel, dev.Timeout, out _);

                        if(!dev.Error && !sense)
                        {
                            testMark = new byte[4];
                            Array.Copy(readBuffer, 24, testMark, 0, 4);
                            if(cdiMark.SequenceEqual(testMark)) dskType = MediaType.CDIREADY;
                        }

                        List<Track> trkList = new List<Track>
                        {
                            new Track
                            {
                                TrackSequence          = 0,
                                TrackSession           = 1,
                                TrackType              = hiddenData ? TrackType.Data : TrackType.Audio,
                                TrackStartSector       = 0,
                                TrackBytesPerSector    = (int)SECTOR_SIZE,
                                TrackRawBytesPerSector = (int)SECTOR_SIZE,
                                TrackSubchannelType    = subType,
                                TrackEndSector         = tracks.First(t => t.TrackSequence == 1).TrackStartSector - 1
                            }
                        };

                        trkList.AddRange(tracks);
                        tracks = trkList.ToArray();
                    }
                }
            }

            // Check mode for tracks
            for(int t = 0; t < tracks.Length; t++)
            {
                if(!readcd)
                {
                    tracks[t].TrackType = TrackType.CdMode1;
                    continue;
                }

                if(tracks[t].TrackType == TrackType.Audio) continue;

                dumpLog.WriteLine("Checking mode for track {0}...", tracks[t].TrackSequence);
                UpdateStatus?.Invoke($"Checking mode for track {tracks[t].TrackSequence}...");

                readcd = !dev.ReadCd(out readBuffer, out senseBuf, (uint)tracks[t].TrackStartSector, blockSize, 1,
                                     MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                     MmcErrorField.None, supportedSubchannel, dev.Timeout, out _);

                if(!readcd)
                {
                    dumpLog.WriteLine("Unable to guess mode for track {0}, continuing...", tracks[t].TrackSequence);
                    UpdateStatus?.Invoke($"Unable to guess mode for track {tracks[t].TrackSequence}, continuing...");
                    continue;
                }

                switch(readBuffer[15])
                {
                    case 1:
                        UpdateStatus?.Invoke($"Track {tracks[t].TrackSequence} is MODE1");
                        dumpLog.WriteLine("Track {0} is MODE1", tracks[t].TrackSequence);
                        tracks[t].TrackType = TrackType.CdMode1;
                        break;
                    case 2:
                        if(dskType == MediaType.CDI || dskType == MediaType.CDIREADY)
                        {
                            UpdateStatus?.Invoke($"Track {tracks[t].TrackSequence} is MODE2");
                            dumpLog.WriteLine("Track {0} is MODE2", tracks[t].TrackSequence);
                            tracks[t].TrackType = TrackType.CdMode2Formless;
                            break;
                        }

                        if((readBuffer[0x012] & 0x20) == 0x20) // mode 2 form 2
                        {
                            UpdateStatus?.Invoke($"Track {tracks[t].TrackSequence} is MODE2 FORM 2");
                            dumpLog.WriteLine("Track {0} is MODE2 FORM 2", tracks[t].TrackSequence);
                            tracks[t].TrackType = TrackType.CdMode2Form2;
                            break;
                        }

                        UpdateStatus?.Invoke($"Track {tracks[t].TrackSequence} is MODE2 FORM 1");
                        dumpLog.WriteLine("Track {0} is MODE2 FORM 1", tracks[t].TrackSequence);
                        tracks[t].TrackType = TrackType.CdMode2Form1;
                        break;
                    default:
                        UpdateStatus?.Invoke($"Track {tracks[t].TrackSequence} is unknown mode {readBuffer[15]}");
                        dumpLog.WriteLine("Track {0} is unknown mode {1}", tracks[t].TrackSequence, readBuffer[15]);
                        break;
                }
            }

            bool supportsLongSectors = true;

            if(outputPlugin.Id == new Guid("12345678-AAAA-BBBB-CCCC-123456789000"))
            {
                if(tracks.Length > 1)
                {
                    StoppingErrorMessage?.Invoke("Output format does not support more than 1 track, not continuing...");
                    dumpLog.WriteLine("Output format does not support more than 1 track, not continuing...");
                    return;
                }

                if(tracks.Any(t => t.TrackType == TrackType.Audio))
                {
                    StoppingErrorMessage?.Invoke("Output format does not support audio tracks, not continuing...");
                    dumpLog.WriteLine("Output format does not support audio tracks, not continuing...");
                    return;
                }

                if(tracks.Any(t => t.TrackType != TrackType.CdMode1))
                {
                    StoppingErrorMessage?.Invoke("Output format only supports MODE 1 tracks, not continuing...");
                    dumpLog.WriteLine("Output format only supports MODE 1 tracks, not continuing...");
                    return;
                }

                supportsLongSectors = false;
            }

            // Check if something prevents from dumping the first track pregap
            if(dumpFirstTrackPregap && readcd)
            {
                if(dev.PlatformId == PlatformID.FreeBSD)
                {
                    if(force)
                    {
                        dumpLog.WriteLine("FreeBSD panics when reading CD first track pregap, see upstream bug #224253. continuing");
                        ErrorMessage
                          ?.Invoke("FreeBSD panics when reading CD first track pregap, see upstream bug #224253. continuing");
                    }
                    else
                    {
                        dumpLog.WriteLine("FreeBSD panics when reading CD first track pregap, see upstream bug #224253. Not continuing");
                        StoppingErrorMessage
                          ?.Invoke("FreeBSD panics when reading CD first track pregap, see upstream bug #224253. Not continuing");
                        return;
                    }

                    dumpFirstTrackPregap = false;
                }

                if(!outputPlugin.SupportedMediaTags.Contains(MediaTagType.CD_FirstTrackPregap))
                {
                    if(force)
                    {
                        dumpLog.WriteLine("Output format does not support CD first track pregap, continuing...");
                        ErrorMessage?.Invoke("Output format does not support CD first track pregap, continuing...");
                    }
                    else
                    {
                        dumpLog.WriteLine("Output format does not support CD first track pregap, not continuing...");
                        StoppingErrorMessage
                          ?.Invoke("Output format does not support CD first track pregap, not continuing...");
                        return;
                    }

                    dumpFirstTrackPregap = false;
                }
            }

            DumpHardwareType currentTry = null;
            ExtentsULong     extents    = null;
            ResumeSupport.Process(true, true, blocks, dev.Manufacturer, dev.Model, dev.Serial, dev.PlatformId,
                                  ref resume, ref currentTry, ref extents);
            if(currentTry == null || extents == null)
            {
                StoppingErrorMessage?.Invoke("Could not process resume file, not continuing...");
                return;
            }

            DateTime timeSpeedStart   = DateTime.UtcNow;
            ulong    sectorSpeedStart = 0;

            // Try to read the first track pregap
            if(dumpFirstTrackPregap && readcd)
            {
                bool         gotFirstTrackPregap         = false;
                int          firstTrackPregapSectorsGood = 0;
                MemoryStream firstTrackPregapMs          = new MemoryStream();

                readBuffer = null;

                dumpLog.WriteLine("Reading first track pregap");
                UpdateStatus?.Invoke("Reading first track pregap");
                InitProgress?.Invoke();
                for(int firstTrackPregapBlock = -150; firstTrackPregapBlock < 0 && resume.NextBlock == 0;
                    firstTrackPregapBlock++)
                {
                    if(aborted)
                    {
                        dumpLog.WriteLine("Aborted!");
                        UpdateStatus?.Invoke("Aborted!");
                        break;
                    }

                    PulseProgress
                      ?.Invoke($"\rTrying to read first track pregap sector {firstTrackPregapBlock} ({currentSpeed:F3} MiB/sec.)");

                    sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)firstTrackPregapBlock, blockSize, 1,
                                       MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                       true, MmcErrorField.None, supportedSubchannel, dev.Timeout,
                                       out double cmdDuration);

                    if(!sense && !dev.Error)
                    {
                        firstTrackPregapMs.Write(readBuffer, 0, (int)blockSize);
                        gotFirstTrackPregap = true;
                        firstTrackPregapSectorsGood++;
                    }
                    else
                    {
                        // Write empty data
                        if(gotFirstTrackPregap) firstTrackPregapMs.Write(new byte[blockSize], 0, (int)blockSize);
                    }

                    sectorSpeedStart++;

                    double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;
                    if(elapsed < 1) continue;

                    currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                    sectorSpeedStart = 0;
                    timeSpeedStart   = DateTime.UtcNow;
                }

                if(firstTrackPregapSectorsGood > 0)
                    mediaTags.Add(MediaTagType.CD_FirstTrackPregap, firstTrackPregapMs.ToArray());

                EndProgress?.Invoke();
                UpdateStatus?.Invoke($"Got {firstTrackPregapSectorsGood} first track pregap sectors.");
                dumpLog.WriteLine("Got {0} first track pregap sectors.", firstTrackPregapSectorsGood);

                firstTrackPregapMs.Close();
            }

            // Try how many blocks are readable at once
            while(true)
            {
                if(readcd)
                {
                    sense = dev.ReadCd(out readBuffer, out senseBuf, 0, blockSize, blocksToRead,
                                       MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                       true, MmcErrorField.None, supportedSubchannel, dev.Timeout, out _);
                    if(dev.Error || sense) blocksToRead /= 2;
                }
                else if(read16)
                {
                    sense = dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, 0, blockSize, 0,
                                       blocksToRead, false, dev.Timeout, out _);
                    if(dev.Error || sense) blocksToRead /= 2;
                }
                else if(read12)
                {
                    sense = dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0,
                                       blocksToRead, false, dev.Timeout, out _);
                    if(dev.Error || sense) blocksToRead /= 2;
                }
                else if(read10)
                {
                    sense = dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, 0, blockSize, 0,
                                       (ushort)blocksToRead, dev.Timeout, out _);
                    if(dev.Error || sense) blocksToRead /= 2;
                }
                else if(read6)
                {
                    sense = dev.Read6(out readBuffer, out senseBuf, 0, blockSize, (byte)blocksToRead, dev.Timeout,
                                      out _);
                    if(dev.Error || sense) blocksToRead /= 2;
                }

                if(!dev.Error || blocksToRead == 1) break;
            }

            if(dev.Error || sense)
            {
                dumpLog.WriteLine("Device error {0} trying to guess ideal transfer length.", dev.LastError);
                StoppingErrorMessage?.Invoke($"Device error {dev.LastError} trying to guess ideal transfer length.");
                return;
            }

            dumpLog.WriteLine("Reading {0} sectors at a time.",              blocksToRead);
            dumpLog.WriteLine("Device reports {0} blocks ({1} bytes).",      blocks, blocks * blockSize);
            dumpLog.WriteLine("Device can read {0} blocks at a time.",       blocksToRead);
            dumpLog.WriteLine("Device reports {0} bytes per logical block.", blockSize);
            dumpLog.WriteLine("SCSI device type: {0}.",                      dev.ScsiType);
            dumpLog.WriteLine("Media identified as {0}.",                    dskType);

            UpdateStatus?.Invoke($"Reading {blocksToRead} sectors at a time.");
            UpdateStatus?.Invoke($"Device reports {blocks} blocks ({blocks * blockSize} bytes).");
            UpdateStatus?.Invoke($"Device can read {blocksToRead} blocks at a time.");
            UpdateStatus?.Invoke($"Device reports {blockSize} bytes per logical block.");
            UpdateStatus?.Invoke($"SCSI device type: {dev.ScsiType}.");
            UpdateStatus?.Invoke($"Media identified as {dskType}.");

            MhddLog mhddLog = new MhddLog(outputPrefix + ".mhddlog.bin", dev, blocks, blockSize, blocksToRead);
            IbgLog  ibgLog  = new IbgLog(outputPrefix  + ".ibg", 0x0008);
            bool ret = outputPlugin.Create(outputPath, dskType, formatOptions, blocks,
                                           supportsLongSectors ? blockSize : 2048);

            // Cannot create image
            if(!ret)
            {
                dumpLog.WriteLine("Error creating output image, not continuing.");
                dumpLog.WriteLine(outputPlugin.ErrorMessage);
                StoppingErrorMessage?.Invoke("Error creating output image, not continuing." + Environment.NewLine +
                                             outputPlugin.ErrorMessage);
                return;
            }

            // Send tracklist to output plugin. This may fail if subchannel is set but unsupported.
            ret = (outputPlugin as IWritableOpticalImage).SetTracks(tracks.ToList());
            if(!ret && supportedSubchannel == MmcSubchannel.None)
            {
                dumpLog.WriteLine("Error sending tracks to output image, not continuing.");
                dumpLog.WriteLine(outputPlugin.ErrorMessage);
                StoppingErrorMessage?.Invoke("Error sending tracks to output image, not continuing." +
                                             Environment.NewLine                                     +
                                             outputPlugin.ErrorMessage);
                return;
            }

            // If a subchannel is supported, check if output plugin allows us to write it.
            if(supportedSubchannel != MmcSubchannel.None)
            {
                dev.ReadCd(out readBuffer, out senseBuf, 0, blockSize, 1, MmcSectorTypes.AllTypes, false, false, true,
                           MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, supportedSubchannel, dev.Timeout,
                           out _);

                byte[] tmpBuf = new byte[subSize];
                Array.Copy(readBuffer, SECTOR_SIZE, tmpBuf, 0, subSize);

                ret = outputPlugin.WriteSectorTag(tmpBuf, 0, SectorTagType.CdSectorSubchannel);

                if(!ret)
                {
                    if(force)
                    {
                        dumpLog.WriteLine("Error writing subchannel to output image, {0}continuing...",
                                          force ? "" : "not ");
                        dumpLog.WriteLine(outputPlugin.ErrorMessage);
                        ErrorMessage?.Invoke("Error writing subchannel to output image, continuing..." +
                                             Environment.NewLine                                       +
                                             outputPlugin.ErrorMessage);
                    }
                    else
                    {
                        StoppingErrorMessage?.Invoke("Error writing subchannel to output image, not continuing..." +
                                                     Environment.NewLine                                           +
                                                     outputPlugin.ErrorMessage);
                        return;
                    }

                    supportedSubchannel = MmcSubchannel.None;
                    subSize             = 0;
                    blockSize           = SECTOR_SIZE + subSize;
                    for(int t = 0; t < tracks.Length; t++) tracks[t].TrackSubchannelType = TrackSubchannelType.None;
                    ret = (outputPlugin as IWritableOpticalImage).SetTracks(tracks.ToList());
                    if(!ret)
                    {
                        dumpLog.WriteLine("Error sending tracks to output image, not continuing.");
                        dumpLog.WriteLine(outputPlugin.ErrorMessage);
                        StoppingErrorMessage?.Invoke("Error sending tracks to output image, not continuing..." +
                                                     Environment.NewLine                                       +
                                                     outputPlugin.ErrorMessage);
                        return;
                    }
                }
            }

            // Set track flags
            foreach(KeyValuePair<byte, byte> kvp in trackFlags)
            {
                Track track = tracks.FirstOrDefault(t => t.TrackSequence == kvp.Key);

                if(track.TrackSequence == 0) continue;

                dumpLog.WriteLine("Setting flags for track {0}...", track.TrackSequence);
                UpdateStatus?.Invoke($"Setting flags for track {track.TrackSequence}...");
                outputPlugin.WriteSectorTag(new[] {kvp.Value}, track.TrackStartSector, SectorTagType.CdTrackFlags);
            }

            // Set MCN
            sense = dev.ReadMcn(out string mcn, out _, out _, dev.Timeout, out _);
            if(!sense && mcn != null && mcn != "0000000000000")
                if(outputPlugin.WriteMediaTag(Encoding.ASCII.GetBytes(mcn), MediaTagType.CD_MCN))
                {
                    UpdateStatus?.Invoke($"Setting disc Media Catalogue Number to {mcn}");
                    dumpLog.WriteLine("Setting disc Media Catalogue Number to {0}", mcn);
                }

            // Set ISRCs
            foreach(Track trk in tracks)
            {
                sense = dev.ReadIsrc((byte)trk.TrackSequence, out string isrc, out _, out _, dev.Timeout, out _);
                if(sense || isrc == null || isrc == "000000000000") continue;

                if(!outputPlugin.WriteSectorTag(Encoding.ASCII.GetBytes(isrc), trk.TrackStartSector,
                                                SectorTagType.CdTrackIsrc)) continue;

                UpdateStatus?.Invoke($"Setting ISRC for track {trk.TrackSequence} to {isrc}");
                dumpLog.WriteLine("Setting ISRC for track {0} to {1}", trk.TrackSequence, isrc);
            }

            if(resume.NextBlock > 0)
            {
                UpdateStatus?.Invoke($"Resuming from block {resume.NextBlock}.");
                dumpLog.WriteLine("Resuming from block {0}.", resume.NextBlock);
            }

            double imageWriteDuration = 0;

            if(skip < blocksToRead) skip = blocksToRead;
            bool newTrim                 = false;

            #if DEBUG
            foreach(Track trk in tracks)
                UpdateStatus
                  ?.Invoke($"Track {trk.TrackSequence} starts at LBA {trk.TrackStartSector} and ends at LBA {trk.TrackEndSector}");
            #endif

            if(dskType == MediaType.CDIREADY)
            {
                dumpLog.WriteLine("There will be thousand of errors between track 0 and track 1, that is normal and you can ignore them.");
                UpdateStatus
                  ?.Invoke("There will be thousand of errors between track 0 and track 1, that is normal and you can ignore them.");
            }

            // Start reading
            start            = DateTime.UtcNow;
            currentSpeed     = 0;
            sectorSpeedStart = 0;
            timeSpeedStart   = DateTime.UtcNow;
            InitProgress?.Invoke();
            for(int t = 0; t < tracks.Length; t++)
            {
                dumpLog.WriteLine("Reading track {0}", tracks[t].TrackSequence);
                if(resume.NextBlock < tracks[t].TrackStartSector) resume.NextBlock = tracks[t].TrackStartSector;

                for(ulong i = resume.NextBlock; i <= tracks[t].TrackEndSector; i += blocksToRead)
                {
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        UpdateStatus?.Invoke("Aborted!");
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    double cmdDuration = 0;

                    if(tracks[t].TrackEndSector + 1 - i < blocksToRead)
                        blocksToRead = (uint)(tracks[t].TrackEndSector + 1 - i);

                    #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                    #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    UpdateProgress
                      ?.Invoke(string.Format("\rReading sector {0} of {1} at track {3} ({2:F3} MiB/sec.)", i, blocks, currentSpeed, tracks[t].TrackSequence),
                               (long)i, (long)blocks);

                    if(readcd)
                    {
                        sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)i, blockSize, blocksToRead,
                                           MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                           true, MmcErrorField.None, supportedSubchannel, dev.Timeout, out cmdDuration);
                        totalDuration += cmdDuration;
                    }
                    else if(read16)
                        sense = dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, i, blockSize, 0,
                                           blocksToRead, false, dev.Timeout, out cmdDuration);
                    else if(read12)
                        sense = dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)i,
                                           blockSize, 0, blocksToRead, false, dev.Timeout, out cmdDuration);
                    else if(read10)
                        sense = dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)i,
                                           blockSize, 0, (ushort)blocksToRead, dev.Timeout, out cmdDuration);
                    else if(read6)
                        sense = dev.Read6(out readBuffer, out senseBuf, (uint)i, blockSize, (byte)blocksToRead,
                                          dev.Timeout, out cmdDuration);

                    if(!sense && !dev.Error)
                    {
                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                        extents.Add(i, blocksToRead, true);
                        DateTime writeStart = DateTime.Now;
                        if(supportedSubchannel != MmcSubchannel.None)
                        {
                            byte[] data = new byte[SECTOR_SIZE * blocksToRead];
                            byte[] sub  = new byte[subSize     * blocksToRead];

                            for(int b = 0; b < blocksToRead; b++)
                            {
                                Array.Copy(readBuffer, (int)(0 + b * blockSize), data, SECTOR_SIZE * b,
                                           SECTOR_SIZE);
                                Array.Copy(readBuffer, (int)(SECTOR_SIZE + b * blockSize), sub, subSize * b,
                                           subSize);
                            }

                            outputPlugin.WriteSectorsLong(data, i, blocksToRead);
                            outputPlugin.WriteSectorsTag(sub, i, blocksToRead, SectorTagType.CdSectorSubchannel);
                        }
                        else
                        {
                            if(supportsLongSectors) outputPlugin.WriteSectorsLong(readBuffer, i, blocksToRead);
                            else
                            {
                                if(readBuffer.Length % 2352 == 0)
                                {
                                    byte[] data = new byte[2048 * blocksToRead];

                                    for(int b = 0; b < blocksToRead; b++)
                                        Array.Copy(readBuffer, (int)(16 + b * blockSize), data, 2048 * b, 2048);

                                    outputPlugin.WriteSectors(data, i, blocksToRead);
                                }
                                else outputPlugin.WriteSectorsLong(readBuffer, i, blocksToRead);
                            }
                        }

                        imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                    }
                    else
                    {
                        // TODO: Reset device after X errors
                        if(stopOnError) return; // TODO: Return more cleanly

                        if(i + skip > blocks) skip = (uint)(blocks - i);

                        // Write empty data
                        DateTime writeStart = DateTime.Now;
                        if(supportedSubchannel != MmcSubchannel.None)
                        {
                            outputPlugin.WriteSectorsLong(new byte[SECTOR_SIZE * skip], i, skip);
                            outputPlugin.WriteSectorsTag(new byte[subSize * skip], i, skip,
                                                         SectorTagType.CdSectorSubchannel);
                        }
                        else
                        {
                            if(supportsLongSectors) outputPlugin.WriteSectorsLong(new byte[blockSize * skip], i, skip);
                            else
                            {
                                if(readBuffer.Length % 2352 == 0)
                                    outputPlugin.WriteSectors(new byte[2048           * skip], i, skip);
                                else outputPlugin.WriteSectorsLong(new byte[blockSize * skip], i, skip);
                            }
                        }

                        imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                        for(ulong b = i; b < i + skip; b++) resume.BadBlocks.Add(b);

                        DicConsole.DebugWriteLine("Dump-Media", "READ error:\n{0}", Sense.PrettifySense(senseBuf));
                        mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                        ibgLog.Write(i, 0);
                        dumpLog.WriteLine("Skipping {0} blocks from errored block {1}.", skip, i);
                        i       += skip - blocksToRead;
                        newTrim =  true;
                    }

                    sectorSpeedStart += blocksToRead;

                    resume.NextBlock = i + blocksToRead;

                    double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;
                    if(elapsed < 1) continue;

                    currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
                    sectorSpeedStart = 0;
                    timeSpeedStart   = DateTime.UtcNow;
                }
            }

            EndProgress?.Invoke();

            // TODO: Enable when underlying images support lead-outs
            /*
            if(persistent)
            {
                UpdateStatus?.Invoke("Reading lead-outs");
                dumpLog.WriteLine("Reading lead-outs");

                InitProgress?.Invoke();
                foreach(Tuple<ulong, ulong> leadout in leadOutExtents.ToArray())
                for(ulong i = leadout.Item1; i <= leadout.Item2; i++)
                {
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    double cmdDuration = 0;

                    #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                    if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                    if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                    #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                    PulseProgress?.Invoke(string.Format("\rReading sector {0} at lead-out ({1:F3} MiB/sec.)", i, blocks,
                                     currentSpeed));

                    if(readcd)
                    {
                        sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)i, blockSize, 1,
                                           MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders,
                                           true, true, MmcErrorField.None, supportedSubchannel, dev.Timeout,
                                           out cmdDuration);
                        totalDuration += cmdDuration;
                    }
                    else if(read16)
                        sense = dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, i, blockSize, 0, 1,
                                           false, dev.Timeout, out cmdDuration);
                    else if(read12)
                        sense = dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)i,
                                           blockSize, 0, 1, false, dev.Timeout, out cmdDuration);
                    else if(read10)
                        sense = dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)i,
                                           blockSize, 0, 1, dev.Timeout, out cmdDuration);
                    else if(read6)
                        sense = dev.Read6(out readBuffer, out senseBuf, (uint)i, blockSize, 1, dev.Timeout,
                                          out cmdDuration);

                    if(!sense && !dev.Error)
                    {
                        mhddLog.Write(i, cmdDuration);
                        ibgLog.Write(i, currentSpeed * 1024);
                        extents.Add(i, blocksToRead, true);
                        leadOutExtents.Remove(i);
                        DateTime writeStart = DateTime.Now;
                        if(supportedSubchannel != MmcSubchannel.None)
                        {
                            byte[] data = new byte[SECTOR_SIZE * blocksToRead];
                            byte[] sub  = new byte[subSize     * blocksToRead];

                            for(int b = 0; b < blocksToRead; b++)
                            {
                                Array.Copy(readBuffer, (int)(0 + b * blockSize), data, SECTOR_SIZE * b,
                                           SECTOR_SIZE);
                                Array.Copy(readBuffer, (int)(SECTOR_SIZE + b * blockSize), sub, subSize * b,
                                           subSize);
                            }

                            outputPlugin.WriteSectorsLong(data, i, blocksToRead);
                            outputPlugin.WriteSectorsTag(sub, i, blocksToRead, SectorTagType.CdSectorSubchannel);
                        }
                        else outputPlugin.WriteSectors(readBuffer, i, blocksToRead);

                        imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                    }
                    else
                    {
                        // TODO: Reset device after X errors
                        if(stopOnError) return; // TODO: Return more cleanly

                        // Write empty data
                        DateTime writeStart = DateTime.Now;
                        if(supportedSubchannel != MmcSubchannel.None)
                        {
                            outputPlugin.WriteSectorsLong(new byte[SECTOR_SIZE * skip], i, 1);
                            outputPlugin.WriteSectorsTag(new byte[subSize * skip], i, 1,
                                                         SectorTagType.CdSectorSubchannel);
                        }
                        else outputPlugin.WriteSectors(new byte[blockSize * skip], i, 1);

                        imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                        mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                        ibgLog.Write(i, 0);
                    }

                    double newSpeed =
                        (double)blockSize * blocksToRead / 1048576 / (cmdDuration / 1000);
                    if(!double.IsInfinity(newSpeed)) currentSpeed = newSpeed;
                    resume.NextBlock = i + 1;
                }

                EndProgress?.Invoke();
            }*/

            end = DateTime.UtcNow;
            mhddLog.Close();
            ibgLog.Close(dev, blocks, blockSize, (end - start).TotalSeconds, currentSpeed * 1024,
                         blockSize * (double)(blocks + 1) / 1024                          / (totalDuration / 1000),
                         devicePath);
            UpdateStatus?.Invoke($"Dump finished in {(end - start).TotalSeconds} seconds.");
            UpdateStatus
              ?.Invoke($"Average dump speed {(double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000):F3} KiB/sec.");
            UpdateStatus
              ?.Invoke($"Average write speed {(double)blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration:F3} KiB/sec.");
            dumpLog.WriteLine("Dump finished in {0} seconds.", (end - start).TotalSeconds);
            dumpLog.WriteLine("Average dump speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / (totalDuration / 1000));
            dumpLog.WriteLine("Average write speed {0:F3} KiB/sec.",
                              (double)blockSize * (double)(blocks + 1) / 1024 / imageWriteDuration);

            #region Compact Disc Error trimming
            if(resume.BadBlocks.Count > 0 && !aborted && !notrim && newTrim)
            {
                start = DateTime.UtcNow;
                UpdateStatus?.Invoke("Trimming bad sectors");
                dumpLog.WriteLine("Trimming bad sectors");

                ulong[] tmpArray = resume.BadBlocks.ToArray();
                InitProgress?.Invoke();
                foreach(ulong badSector in tmpArray)
                {
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        UpdateStatus?.Invoke("Aborted!");
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    PulseProgress?.Invoke($"\rTrimming sector {badSector}");

                    double cmdDuration = 0;

                    if(readcd)
                        sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)badSector, blockSize, 1,
                                           MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                           true, MmcErrorField.None, supportedSubchannel, dev.Timeout, out cmdDuration);
                    else if(read16)
                        sense = dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, badSector, blockSize, 0,
                                           blocksToRead, false, dev.Timeout, out cmdDuration);
                    else if(read12)
                        sense = dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)badSector,
                                           blockSize, 0, blocksToRead, false, dev.Timeout, out cmdDuration);
                    else if(read10)
                        sense = dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)badSector,
                                           blockSize, 0, (ushort)blocksToRead, dev.Timeout, out cmdDuration);
                    else if(read6)
                        sense = dev.Read6(out readBuffer, out senseBuf, (uint)badSector, blockSize, (byte)blocksToRead,
                                          dev.Timeout, out cmdDuration);

                    totalDuration += cmdDuration;

                    if(sense || dev.Error) continue;

                    if(!sense && !dev.Error)
                    {
                        resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                    }

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        byte[] data = new byte[SECTOR_SIZE];
                        byte[] sub  = new byte[subSize];
                        Array.Copy(readBuffer, 0,           data, 0, SECTOR_SIZE);
                        Array.Copy(readBuffer, SECTOR_SIZE, sub,  0, subSize);
                        outputPlugin.WriteSectorLong(data, badSector);
                        outputPlugin.WriteSectorTag(sub, badSector, SectorTagType.CdSectorSubchannel);
                    }
                    else
                    {
                        if(supportsLongSectors) outputPlugin.WriteSectorLong(readBuffer, badSector);
                        else
                        {
                            if(readBuffer.Length % 2352 == 0)
                            {
                                byte[] data = new byte[2048];

                                for(int b = 0; b < blocksToRead; b++) Array.Copy(readBuffer, 16, data, 0, 2048);

                                outputPlugin.WriteSector(data, badSector);
                            }
                            else outputPlugin.WriteSectorLong(readBuffer, badSector);
                        }
                    }
                }

                EndProgress?.Invoke();
                end = DateTime.UtcNow;
                UpdateStatus?.Invoke($"Trimmming finished in {(end - start).TotalSeconds} seconds.");
                dumpLog.WriteLine("Trimmming finished in {0} seconds.", (end - start).TotalSeconds);
            }
            #endregion Compact Disc Error trimming

            #region Compact Disc Error handling
            if(resume.BadBlocks.Count > 0 && !aborted && retryPasses > 0)
            {
                int  pass              = 1;
                bool forward           = true;
                bool runningPersistent = false;

                Modes.ModePage? currentModePage = null;
                byte[]          md6;
                byte[]          md10;

                if(persistent)
                {
                    Modes.ModePage_01_MMC pgMmc;

                    sense = dev.ModeSense6(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                           dev.Timeout, out _);
                    if(sense)
                    {
                        sense = dev.ModeSense10(out readBuffer, out _, false, ScsiModeSensePageControl.Current, 0x01,
                                                dev.Timeout, out _);

                        if(!sense)
                        {
                            Modes.DecodedMode? dcMode10 =
                                Modes.DecodeMode10(readBuffer, PeripheralDeviceTypes.MultiMediaDevice);

                            if(dcMode10.HasValue)
                                foreach(Modes.ModePage modePage in dcMode10.Value.Pages)
                                    if(modePage.Page == 0x01 && modePage.Subpage == 0x00)
                                        currentModePage = modePage;
                        }
                    }
                    else
                    {
                        Modes.DecodedMode? dcMode6 =
                            Modes.DecodeMode6(readBuffer, PeripheralDeviceTypes.MultiMediaDevice);

                        if(dcMode6.HasValue)
                            foreach(Modes.ModePage modePage in dcMode6.Value.Pages)
                                if(modePage.Page == 0x01 && modePage.Subpage == 0x00)
                                    currentModePage = modePage;
                    }

                    if(currentModePage == null)
                    {
                        pgMmc = new Modes.ModePage_01_MMC {PS = false, ReadRetryCount = 32, Parameter = 0x00};
                        currentModePage = new Modes.ModePage
                        {
                            Page = 0x01, Subpage = 0x00, PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                        };
                    }

                    pgMmc = new Modes.ModePage_01_MMC {PS = false, ReadRetryCount = 255, Parameter = 0x20};
                    Modes.DecodedMode md = new Modes.DecodedMode
                    {
                        Header = new Modes.ModeHeader(),
                        Pages = new[]
                        {
                            new Modes.ModePage
                            {
                                Page         = 0x01,
                                Subpage      = 0x00,
                                PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                            }
                        }
                    };
                    md6  = Modes.EncodeMode6(md, dev.ScsiType);
                    md10 = Modes.EncodeMode10(md, dev.ScsiType);

                    UpdateStatus?.Invoke("Sending MODE SELECT to drive (return damaged blocks).");
                    dumpLog.WriteLine("Sending MODE SELECT to drive (return damaged blocks).");
                    sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out _);
                    if(sense) sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out _);

                    if(sense)
                    {
                        UpdateStatus
                          ?.Invoke("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");
                        DicConsole.DebugWriteLine("Error: {0}", Sense.PrettifySense(senseBuf));
                        dumpLog.WriteLine("Drive did not accept MODE SELECT command for persistent error reading, try another drive.");
                    }
                    else runningPersistent = true;
                }

                InitProgress?.Invoke();
                cdRepeatRetry:
                ulong[]     tmpArray              = resume.BadBlocks.ToArray();
                List<ulong> sectorsNotEvenPartial = new List<ulong>();
                foreach(ulong badSector in tmpArray)
                {
                    if(aborted)
                    {
                        currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                        dumpLog.WriteLine("Aborted!");
                        break;
                    }

                    PulseProgress?.Invoke(string.Format("\rRetrying sector {0}, pass {1}, {3}{2}", badSector, pass,
                                                        forward ? "forward" : "reverse",
                                                        runningPersistent ? "recovering partial data, " : ""));

                    if(readcd)
                    {
                        sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)badSector, blockSize, 1,
                                           MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                           true, MmcErrorField.None, supportedSubchannel, dev.Timeout,
                                           out double cmdDuration);
                        totalDuration += cmdDuration;
                    }

                    if(sense || dev.Error)
                    {
                        if(!runningPersistent) continue;

                        FixedSense? decSense = Sense.DecodeFixed(senseBuf);

                        // MEDIUM ERROR, retry with ignore error below
                        if(decSense.HasValue && decSense.Value.ASC == 0x11)
                            if(!sectorsNotEvenPartial.Contains(badSector))
                                sectorsNotEvenPartial.Add(badSector);
                    }

                    if(!sense && !dev.Error)
                    {
                        resume.BadBlocks.Remove(badSector);
                        extents.Add(badSector);
                        UpdateStatus?.Invoke($"Correctly retried sector {badSector} in pass {pass}.");
                        dumpLog.WriteLine("Correctly retried sector {0} in pass {1}.", badSector, pass);
                        sectorsNotEvenPartial.Remove(badSector);
                    }

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        byte[] data = new byte[SECTOR_SIZE];
                        byte[] sub  = new byte[subSize];
                        Array.Copy(readBuffer, 0,           data, 0, SECTOR_SIZE);
                        Array.Copy(readBuffer, SECTOR_SIZE, sub,  0, subSize);
                        outputPlugin.WriteSectorLong(data, badSector);
                        outputPlugin.WriteSectorTag(sub, badSector, SectorTagType.CdSectorSubchannel);
                    }
                    else outputPlugin.WriteSectorLong(readBuffer, badSector);
                }

                if(pass < retryPasses && !aborted && resume.BadBlocks.Count > 0)
                {
                    pass++;
                    forward = !forward;
                    resume.BadBlocks.Sort();
                    resume.BadBlocks.Reverse();
                    goto cdRepeatRetry;
                }

                EndProgress?.Invoke();

                // Try to ignore read errors, on some drives this allows to recover partial even if damaged data
                if(persistent && sectorsNotEvenPartial.Count > 0)
                {
                    Modes.ModePage_01_MMC pgMmc =
                        new Modes.ModePage_01_MMC {PS = false, ReadRetryCount = 255, Parameter = 0x01};
                    Modes.DecodedMode md = new Modes.DecodedMode
                    {
                        Header = new Modes.ModeHeader(),
                        Pages = new[]
                        {
                            new Modes.ModePage
                            {
                                Page         = 0x01,
                                Subpage      = 0x00,
                                PageResponse = Modes.EncodeModePage_01_MMC(pgMmc)
                            }
                        }
                    };
                    md6  = Modes.EncodeMode6(md, dev.ScsiType);
                    md10 = Modes.EncodeMode10(md, dev.ScsiType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive (ignore error correction).");
                    sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out _);
                    if(sense) sense = dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out _);

                    if(!sense)
                    {
                        runningPersistent = true;

                        InitProgress?.Invoke();
                        foreach(ulong badSector in sectorsNotEvenPartial)
                        {
                            if(aborted)
                            {
                                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                dumpLog.WriteLine("Aborted!");
                                break;
                            }

                            PulseProgress?.Invoke($"\rTrying to get partial data for sector {badSector}");

                            if(readcd)
                            {
                                sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)badSector, blockSize, 1,
                                                   MmcSectorTypes.AllTypes, false, false, true,
                                                   MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                                   supportedSubchannel, dev.Timeout, out double cmdDuration);
                                totalDuration += cmdDuration;
                            }

                            if(sense || dev.Error) continue;

                            dumpLog.WriteLine("Got partial data for sector {0} in pass {1}.", badSector, pass);

                            if(supportedSubchannel != MmcSubchannel.None)
                            {
                                byte[] data = new byte[SECTOR_SIZE];
                                byte[] sub  = new byte[subSize];
                                Array.Copy(readBuffer, 0,           data, 0, SECTOR_SIZE);
                                Array.Copy(readBuffer, SECTOR_SIZE, sub,  0, subSize);
                                outputPlugin.WriteSectorLong(data, badSector);
                                outputPlugin.WriteSectorTag(sub, badSector, SectorTagType.CdSectorSubchannel);
                            }
                            else outputPlugin.WriteSectorLong(readBuffer, badSector);
                        }

                        EndProgress?.Invoke();
                    }
                }

                if(runningPersistent && currentModePage.HasValue)
                {
                    // TODO: Enable when underlying images support lead-outs
                    /*
                    dumpLog.WriteLine("Retrying lead-outs");

                    InitProgress?.Invoke();
                    foreach(Tuple<ulong, ulong> leadout in leadOutExtents.ToArray())
                        for(ulong i = leadout.Item1; i <= leadout.Item2; i++)
                        {
                            if(aborted)
                            {
                                currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                                dumpLog.WriteLine("Aborted!");
                                break;
                            }

                            double cmdDuration = 0;

                            #pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
                            if(currentSpeed > maxSpeed && currentSpeed != 0) maxSpeed = currentSpeed;
                            if(currentSpeed < minSpeed && currentSpeed != 0) minSpeed = currentSpeed;
                            #pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator

                            PulseProgress?.Invoke(string.Format("\rReading sector {0} at lead-out ({1:F3} MiB/sec.)", i,
                                                  blocks, currentSpeed));

                            if(readcd)
                            {
                                sense = dev.ReadCd(out readBuffer, out senseBuf, (uint)i, blockSize, 1,
                                                   MmcSectorTypes.AllTypes, false, false, true,
                                                   MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                                   supportedSubchannel, dev.Timeout, out cmdDuration);
                                totalDuration += cmdDuration;
                            }
                            else if(read16)
                                sense = dev.Read16(out readBuffer, out senseBuf, 0, false, true, false, i, blockSize, 0,
                                                   1, false, dev.Timeout, out cmdDuration);
                            else if(read12)
                                sense = dev.Read12(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)i,
                                                   blockSize, 0, 1, false, dev.Timeout, out cmdDuration);
                            else if(read10)
                                sense = dev.Read10(out readBuffer, out senseBuf, 0, false, true, false, false, (uint)i,
                                                   blockSize, 0, 1, dev.Timeout, out cmdDuration);
                            else if(read6)
                                sense = dev.Read6(out readBuffer, out senseBuf, (uint)i, blockSize, 1, dev.Timeout,
                                                  out cmdDuration);

                            if(!sense && !dev.Error)
                            {
                                mhddLog.Write(i, cmdDuration);
                                ibgLog.Write(i, currentSpeed * 1024);
                                extents.Add(i, blocksToRead, true);
                                leadOutExtents.Remove(i);
                                DateTime writeStart = DateTime.Now;
                                if(supportedSubchannel != MmcSubchannel.None)
                                {
                                    byte[] data = new byte[SECTOR_SIZE * blocksToRead];
                                    byte[] sub  = new byte[subSize     * blocksToRead];

                                    for(int b = 0; b < blocksToRead; b++)
                                    {
                                        Array.Copy(readBuffer, (int)(0 + b * blockSize), data, SECTOR_SIZE * b,
                                                   SECTOR_SIZE);
                                        Array.Copy(readBuffer, (int)(SECTOR_SIZE + b * blockSize), sub, subSize * b,
                                                   subSize);
                                    }

                                    outputPlugin.WriteSectorsLong(data, i, blocksToRead);
                                    outputPlugin.WriteSectorsTag(sub, i, blocksToRead,
                                                                 SectorTagType.CdSectorSubchannel);
                                }
                                else outputPlugin.WriteSectors(readBuffer, i, blocksToRead);

                                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;
                            }
                            else
                            {
                                // TODO: Reset device after X errors
                                if(stopOnError) return; // TODO: Return more cleanly

                                // Write empty data
                                DateTime writeStart = DateTime.Now;
                                if(supportedSubchannel != MmcSubchannel.None)
                                {
                                    outputPlugin.WriteSectorsLong(new byte[SECTOR_SIZE * skip], i, 1);
                                    outputPlugin.WriteSectorsTag(new byte[subSize * skip], i, 1,
                                                                 SectorTagType.CdSectorSubchannel);
                                }
                                else outputPlugin.WriteSectors(new byte[blockSize * skip], i, 1);

                                imageWriteDuration += (DateTime.Now - writeStart).TotalSeconds;

                                mhddLog.Write(i, cmdDuration < 500 ? 65535 : cmdDuration);

                                ibgLog.Write(i, 0);
                            }

                            double newSpeed =
                                (double)blockSize * blocksToRead / 1048576 / (cmdDuration / 1000);
                            if(!double.IsInfinity(newSpeed)) currentSpeed = newSpeed;
                        }

                    EndProgress?.Invoke();
                    */

                    Modes.DecodedMode md = new Modes.DecodedMode
                    {
                        Header = new Modes.ModeHeader(), Pages = new[] {currentModePage.Value}
                    };
                    md6  = Modes.EncodeMode6(md, dev.ScsiType);
                    md10 = Modes.EncodeMode10(md, dev.ScsiType);

                    dumpLog.WriteLine("Sending MODE SELECT to drive (return device to previous status).");
                    sense = dev.ModeSelect(md6, out senseBuf, true, false, dev.Timeout, out _);
                    if(sense) dev.ModeSelect10(md10, out senseBuf, true, false, dev.Timeout, out _);
                }

                EndProgress?.Invoke();
            }
            #endregion Compact Disc Error handling

            // Write media tags to image
            if(!aborted)
                foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
                {
                    ret = outputPlugin.WriteMediaTag(tag.Value, tag.Key);

                    if(ret || force) continue;

                    // Cannot write tag to image
                    dumpLog.WriteLine($"Cannot write tag {tag.Key}.");
                    StoppingErrorMessage?.Invoke(outputPlugin.ErrorMessage);
                    return;
                }

            resume.BadBlocks.Sort();
            foreach(ulong bad in resume.BadBlocks) dumpLog.WriteLine("Sector {0} could not be read.", bad);
            currentTry.Extents = ExtentsConverter.ToMetadata(extents);

            outputPlugin.SetDumpHardware(resume.Tries);
            if(preSidecar != null) outputPlugin.SetCicmMetadata(preSidecar);
            dumpLog.WriteLine("Closing output file.");
            UpdateStatus?.Invoke("Closing output file.");
            DateTime closeStart = DateTime.Now;
            outputPlugin.Close();
            DateTime closeEnd = DateTime.Now;
            UpdateStatus?.Invoke($"Closed in {(closeEnd - closeStart).TotalSeconds} seconds.");

            if(aborted)
            {
                dumpLog.WriteLine("Aborted!");
                return;
            }

            double totalChkDuration = 0;
            if(!nometadata)
            {
                dumpLog.WriteLine("Creating sidecar.");
                FiltersList filters     = new FiltersList();
                IFilter     filter      = filters.GetFilter(outputPath);
                IMediaImage inputPlugin = ImageFormat.Detect(filter);
                if(!inputPlugin.Open(filter))
                {
                    StoppingErrorMessage?.Invoke("Could not open created image.");
                    return;
                }

                DateTime         chkStart = DateTime.UtcNow;
                CICMMetadataType sidecar  = Sidecar.Create(inputPlugin, outputPath, filter.Id, encoding);
                end = DateTime.UtcNow;

                totalChkDuration = (end - chkStart).TotalMilliseconds;
                dumpLog.WriteLine("Sidecar created in {0} seconds.", (end - chkStart).TotalSeconds);
                dumpLog.WriteLine("Average checksum speed {0:F3} KiB/sec.",
                                  (double)blockSize * (double)(blocks + 1) / 1024 / (totalChkDuration / 1000));

                if(preSidecar != null)
                {
                    preSidecar.OpticalDisc = sidecar.OpticalDisc;
                    sidecar                = preSidecar;
                }

                List<(ulong start, string type)> filesystems = new List<(ulong start, string type)>();
                if(sidecar.OpticalDisc[0].Track != null)
                    filesystems.AddRange(from xmlTrack in sidecar.OpticalDisc[0].Track
                                         where xmlTrack.FileSystemInformation != null
                                         from partition in xmlTrack.FileSystemInformation
                                         where partition.FileSystems != null
                                         from fileSystem in partition.FileSystems
                                         select ((ulong)partition.StartSector, fileSystem.Type));

                if(filesystems.Count > 0)
                    foreach(var filesystem in filesystems.Select(o => new {o.start, o.type}).Distinct())
                        dumpLog.WriteLine("Found filesystem {0} at sector {1}", filesystem.type, filesystem.start);

                sidecar.OpticalDisc[0].Dimensions = Dimensions.DimensionsFromMediaType(dskType);
                CommonTypes.Metadata.MediaType.MediaTypeToString(dskType, out string xmlDskTyp,
                                                                 out string xmlDskSubTyp);
                sidecar.OpticalDisc[0].DiscType          = xmlDskTyp;
                sidecar.OpticalDisc[0].DiscSubType       = xmlDskSubTyp;
                sidecar.OpticalDisc[0].DumpHardwareArray = resume.Tries.ToArray();

                foreach(KeyValuePair<MediaTagType, byte[]> tag in mediaTags)
                    if(outputPlugin.SupportedMediaTags.Contains(tag.Key))
                        AddMediaTagToSidecar(outputPath, tag, ref sidecar);

                UpdateStatus?.Invoke("Writing metadata sidecar");

                FileStream xmlFs = new FileStream(outputPrefix + ".cicm.xml", FileMode.Create);

                XmlSerializer xmlSer = new XmlSerializer(typeof(CICMMetadataType));
                xmlSer.Serialize(xmlFs, sidecar);
                xmlFs.Close();
            }

            UpdateStatus?.Invoke("");
            UpdateStatus
              ?.Invoke($"Took a total of {(end - start).TotalSeconds:F3} seconds ({totalDuration / 1000:F3} processing commands, {totalChkDuration / 1000:F3} checksumming, {imageWriteDuration:F3} writing, {(closeEnd - closeStart).TotalSeconds:F3} closing).");
            UpdateStatus
              ?.Invoke($"Average speed: {(double)blockSize * (double)(blocks + 1) / 1048576 / (totalDuration / 1000):F3} MiB/sec.");
            UpdateStatus?.Invoke($"Fastest speed burst: {maxSpeed:F3} MiB/sec.");
            UpdateStatus?.Invoke($"Slowest speed burst: {minSpeed:F3} MiB/sec.");
            UpdateStatus?.Invoke($"{resume.BadBlocks.Count} sectors could not be read.");
            UpdateStatus?.Invoke("");

            Statistics.AddMedia(dskType, true);
        }
    }
}