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
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Decoders.CD;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace DiscImageChef.Core.Devices.Dumping
{
    partial class Dump
    {
        /// <summary>Reads the TOC, processes it, returns the track list and last sector</summary>
        /// <param name="blockSize">Size of the read sector in bytes</param>
        /// <param name="dskType">Disc type</param>
        /// <param name="lastSector">Last sector number</param>
        /// <param name="leadOutStarts">Lead-out starts</param>
        /// <param name="mediaTags">Media tags</param>
        /// <param name="toc">Full CD TOC</param>
        /// <param name="trackFlags">Track flags</param>
        /// <param name="subType">Track subchannel type</param>
        /// <returns>List of tracks</returns>
        Track[] GetCdTracks(ref uint blockSize, MediaType dskType, out long lastSector,
                            Dictionary<int, long> leadOutStarts, Dictionary<MediaTagType, byte[]> mediaTags,
                            out FullTOC.CDFullTOC? toc, Dictionary<byte, byte> trackFlags, TrackSubchannelType subType)
        {
            byte[]      cmdBuf     = null;              // Data buffer
            const uint  sectorSize = 2352;              // Full sector size
            bool        sense      = true;              // Sense indicator
            List<Track> trackList  = new List<Track>(); // Tracks in disc
            byte[]      tmpBuf;                         // Temporary buffer
            toc        = null;
            lastSector = 0;
            TrackType leadoutTrackType = TrackType.Audio;

            // We discarded all discs that falsify a TOC before requesting a real TOC
            // No TOC, no CD (or an empty one)
            _dumpLog.WriteLine("Reading full TOC");
            UpdateStatus?.Invoke("Reading full TOC");
            sense = _dev.ReadRawToc(out cmdBuf, out _, 0, _dev.Timeout, out _);

            if(!sense)
            {
                toc = FullTOC.Decode(cmdBuf);

                if(toc.HasValue)
                {
                    tmpBuf = new byte[cmdBuf.Length - 2];
                    Array.Copy(cmdBuf, 2, tmpBuf, 0, cmdBuf.Length - 2);
                    mediaTags.Add(MediaTagType.CD_FullTOC, tmpBuf);
                }
            }

            UpdateStatus?.Invoke("Building track map...");
            _dumpLog.WriteLine("Building track map...");

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
                            TrackSequence = trk.POINT, TrackSession = trk.SessionNumber,
                            TrackType = (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                        (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                            ? TrackType.Data : TrackType.Audio,
                            TrackStartSector =
                                (ulong)(((trk.PHOUR * 3600 * 75) + (trk.PMIN * 60 * 75) + (trk.PSEC * 75) +
                                         trk.PFRAME) - 150),
                            TrackBytesPerSector    = (int)sectorSize,
                            TrackRawBytesPerSector = (int)sectorSize,
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

                        lastSector = ((phour * 3600 * 75) + (pmin * 60 * 75) + (psec * 75) + pframe) - 150;
                        leadOutStarts.Add(trk.SessionNumber, lastSector                    + 1);
                    }
                    else if(trk.POINT == 0xA0 &&
                            trk.ADR   == 1)
                    {
                        switch(trk.PSEC)
                        {
                            case 0x10:
                                dskType = MediaType.CDI;

                                break;
                            case 0x20:
                                if(dskType == MediaType.CD ||
                                   dskType == MediaType.CDROM)
                                    dskType = MediaType.CDROMXA;

                                break;
                        }

                        leadoutTrackType =
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental ? TrackType.Data
                                : TrackType.Audio;
                    }
            }
            else
            {
                UpdateStatus?.Invoke("Cannot read RAW TOC, requesting processed one...");
                _dumpLog.WriteLine("Cannot read RAW TOC, requesting processed one...");
                sense = _dev.ReadToc(out cmdBuf, out _, false, 0, _dev.Timeout, out _);

                TOC.CDTOC? oldToc = TOC.Decode(cmdBuf);

                if((sense || !oldToc.HasValue) &&
                   !_force)
                {
                    _dumpLog.WriteLine("Could not read TOC, if you want to continue, use force, and will try from LBA 0 to 360000...");

                    StoppingErrorMessage?.
                        Invoke("Could not read TOC, if you want to continue, use force, and will try from LBA 0 to 360000...");

                    return null;
                }

                foreach(TOC.CDTOCTrackDataDescriptor trk in oldToc.
                                                            Value.TrackDescriptors.OrderBy(t => t.TrackNumber).
                                                            Where(trk => trk.ADR == 1 || trk.ADR == 4))
                    if(trk.TrackNumber >= 0x01 &&
                       trk.TrackNumber <= 0x63)
                    {
                        trackList.Add(new Track
                        {
                            TrackSequence = trk.TrackNumber, TrackSession = 1,
                            TrackType = (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                        (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                            ? TrackType.Data : TrackType.Audio,
                            TrackStartSector       = trk.TrackStartAddress, TrackBytesPerSector = (int)sectorSize,
                            TrackRawBytesPerSector = (int)sectorSize, TrackSubchannelType       = subType
                        });

                        trackFlags.Add(trk.TrackNumber, trk.CONTROL);
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
                UpdateStatus?.Invoke("No tracks found, adding a single track from 0 to Lead-Out");
                _dumpLog.WriteLine("No tracks found, adding a single track from 0 to Lead-Out");

                trackList.Add(new Track
                {
                    TrackSequence       = 1, TrackSession = 1, TrackType = leadoutTrackType,
                    TrackStartSector    = 0,
                    TrackBytesPerSector = (int)sectorSize, TrackRawBytesPerSector = (int)sectorSize,
                    TrackSubchannelType = subType
                });

                trackFlags.Add(1, (byte)(leadoutTrackType == TrackType.Audio ? 0 : 4));
            }

            if(lastSector == 0)
            {
                sense = _dev.ReadCapacity16(out cmdBuf, out _, _dev.Timeout, out _);

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
                    sense = _dev.ReadCapacity(out cmdBuf, out _, _dev.Timeout, out _);

                    if(!sense)
                    {
                        lastSector = (cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3];
                        blockSize  = (uint)((cmdBuf[5] << 24) + (cmdBuf[5] << 16) + (cmdBuf[6] << 8) + cmdBuf[7]);
                    }
                }

                if(lastSector <= 0)
                {
                    if(!_force)
                    {
                        StoppingErrorMessage?.
                            Invoke("Could not find Lead-Out, if you want to continue use force option and will continue until 360000 sectors...");

                        _dumpLog.
                            WriteLine("Could not find Lead-Out, if you want to continue use force option and will continue until 360000 sectors...");

                        return null;
                    }

                    UpdateStatus?.
                        Invoke("WARNING: Could not find Lead-Out start, will try to read up to 360000 sectors, probably will fail before...");

                    _dumpLog.WriteLine("WARNING: Could not find Lead-Out start, will try to read up to 360000 sectors, probably will fail before...");
                    lastSector = 360000;
                }
            }

            return trackList.ToArray();
        }
    }
}