// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Tracks.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Calculates CompactDisc tracks.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Logging;
using Aaru.Decoders.CD;
using Aaru.Devices;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping
{
    partial class Dump
    {
        /// <summary>Reads the TOC, processes it, returns the track list and last sector</summary>
        /// <param name="dev">Device</param>
        /// <param name="dumpLog">Dump log</param>
        /// <param name="force">Force dump enabled</param>
        /// <param name="lastSector">Last sector number</param>
        /// <param name="leadOutStarts">Lead-out starts</param>
        /// <param name="mediaTags">Media tags</param>
        /// <param name="stoppingErrorMessage">Stopping error message handler</param>
        /// <param name="toc">Full CD TOC</param>
        /// <param name="trackFlags">Track flags</param>
        /// <param name="updateStatus">Update status handler</param>
        /// <returns>List of tracks</returns>
        public static Track[] GetCdTracks(Device dev, DumpLog dumpLog, bool force, out long lastSector,
                                          Dictionary<int, long> leadOutStarts,
                                          Dictionary<MediaTagType, byte[]> mediaTags,
                                          ErrorMessageHandler stoppingErrorMessage, out FullTOC.CDFullTOC? toc,
                                          Dictionary<byte, byte> trackFlags, UpdateStatusHandler updateStatus)
        {
            byte[]      cmdBuf;                        // Data buffer
            const uint  sectorSize = 2352;             // Full sector size
            bool        sense;                         // Sense indicator
            List<Track> trackList = new List<Track>(); // Tracks in disc
            byte[]      tmpBuf;                        // Temporary buffer
            toc        = null;
            lastSector = 0;
            TrackType leadoutTrackType = TrackType.Audio;

            // We discarded all discs that falsify a TOC before requesting a real TOC
            // No TOC, no CD (or an empty one)
            dumpLog?.WriteLine("Reading full TOC");
            updateStatus?.Invoke("Reading full TOC");
            sense = dev.ReadRawToc(out cmdBuf, out _, 0, dev.Timeout, out _);

            if(!sense)
            {
                toc = FullTOC.Decode(cmdBuf);

                if(toc.HasValue)
                {
                    tmpBuf = new byte[cmdBuf.Length - 2];
                    Array.Copy(cmdBuf, 2, tmpBuf, 0, cmdBuf.Length - 2);
                    mediaTags?.Add(MediaTagType.CD_FullTOC, tmpBuf);
                }
            }

            updateStatus?.Invoke("Building track map...");
            dumpLog?.WriteLine("Building track map...");

            if(toc.HasValue)
            {
                FullTOC.TrackDataDescriptor[] sortedTracks =
                    toc.Value.TrackDescriptors.OrderBy(track => track.POINT).ToArray();

                foreach(FullTOC.TrackDataDescriptor trk in sortedTracks.Where(trk => trk.ADR == 1 || trk.ADR == 4))
                    if(trk.POINT >= 0x01 &&
                       trk.POINT <= 0x63)
                    {
                        trackList.Add(new Track
                        {
                            TrackSequence = trk.POINT,
                            TrackSession  = trk.SessionNumber,
                            TrackType = (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                        (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                            ? TrackType.Data : TrackType.Audio,
                            TrackStartSector =
                                (ulong)((trk.PHOUR * 3600 * 75) + (trk.PMIN * 60 * 75) + (trk.PSEC * 75) + trk.PFRAME -
                                        150),
                            TrackBytesPerSector    = (int)sectorSize,
                            TrackRawBytesPerSector = (int)sectorSize
                        });

                        trackFlags?.Add(trk.POINT, trk.CONTROL);
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

                        lastSector = (phour * 3600 * 75) + (pmin * 60 * 75) + (psec * 75) + pframe - 150;
                        leadOutStarts?.Add(trk.SessionNumber, lastSector + 1);
                    }
                    else if(trk.POINT == 0xA0 &&
                            trk.ADR   == 1)
                    {
                        leadoutTrackType =
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental ? TrackType.Data
                                : TrackType.Audio;
                    }
            }
            else
            {
                updateStatus?.Invoke("Cannot read RAW TOC, requesting processed one...");
                dumpLog?.WriteLine("Cannot read RAW TOC, requesting processed one...");
                sense = dev.ReadToc(out cmdBuf, out _, false, 0, dev.Timeout, out _);

                TOC.CDTOC? oldToc = TOC.Decode(cmdBuf);

                if((sense || !oldToc.HasValue) &&
                   !force)
                {
                    dumpLog?.WriteLine("Could not read TOC, if you want to continue, use force, and will try from LBA 0 to 360000...");

                    stoppingErrorMessage?.
                        Invoke("Could not read TOC, if you want to continue, use force, and will try from LBA 0 to 360000...");

                    return null;
                }

                if(oldToc.HasValue)
                    foreach(TOC.CDTOCTrackDataDescriptor trk in oldToc.Value.TrackDescriptors.
                                                                       OrderBy(t => t.TrackNumber).
                                                                       Where(trk => trk.ADR == 1 || trk.ADR == 4))
                        if(trk.TrackNumber >= 0x01 &&
                           trk.TrackNumber <= 0x63)
                        {
                            trackList.Add(new Track
                            {
                                TrackSequence = trk.TrackNumber,
                                TrackSession  = 1,
                                TrackType = (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                                ? TrackType.Data : TrackType.Audio,
                                TrackStartSector       = trk.TrackStartAddress,
                                TrackBytesPerSector    = (int)sectorSize,
                                TrackRawBytesPerSector = (int)sectorSize
                            });

                            trackFlags?.Add(trk.TrackNumber, trk.CONTROL);
                        }
                        else if(trk.TrackNumber == 0xAA)
                        {
                            leadoutTrackType =
                                (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental ? TrackType.Data
                                    : TrackType.Audio;

                            lastSector = trk.TrackStartAddress - 1;
                        }
            }

            if(trackList.Count == 0)
            {
                updateStatus?.Invoke("No tracks found, adding a single track from 0 to Lead-Out");
                dumpLog?.WriteLine("No tracks found, adding a single track from 0 to Lead-Out");

                trackList.Add(new Track
                {
                    TrackSequence          = 1,
                    TrackSession           = 1,
                    TrackType              = leadoutTrackType,
                    TrackStartSector       = 0,
                    TrackBytesPerSector    = (int)sectorSize,
                    TrackRawBytesPerSector = (int)sectorSize
                });

                trackFlags?.Add(1, (byte)(leadoutTrackType == TrackType.Audio ? 0 : 4));
            }

            if(lastSector != 0)
                return trackList.ToArray();

            sense = dev.ReadCapacity16(out cmdBuf, out _, dev.Timeout, out _);

            if(!sense)
            {
                byte[] temp = new byte[8];

                Array.Copy(cmdBuf, 0, temp, 0, 8);
                Array.Reverse(temp);
                lastSector = (long)BitConverter.ToUInt64(temp, 0);
            }
            else
            {
                sense = dev.ReadCapacity(out cmdBuf, out _, dev.Timeout, out _);

                if(!sense)
                    lastSector = ((cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3]) & 0xFFFFFFFF;
            }

            if(lastSector > 0)
                return trackList.ToArray();

            if(!force)
            {
                stoppingErrorMessage?.
                    Invoke("Could not find Lead-Out, if you want to continue use force option and will continue until 360000 sectors...");

                dumpLog?.WriteLine("Could not find Lead-Out, if you want to continue use force option and will continue until 360000 sectors...");

                return null;
            }

            updateStatus?.
                Invoke("WARNING: Could not find Lead-Out start, will try to read up to 360000 sectors, probably will fail before...");

            dumpLog?.WriteLine("WARNING: Could not find Lead-Out start, will try to read up to 360000 sectors, probably will fail before...");
            lastSector = 360000;

            return trackList.ToArray();
        }
    }
}