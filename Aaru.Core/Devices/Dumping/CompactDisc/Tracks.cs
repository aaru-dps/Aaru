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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Decoders.CD;
using Aaru.Devices;

namespace Aaru.Core.Devices.Dumping;

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
    public static Track[] GetCdTracks(Device dev, bool force, out long lastSector, Dictionary<int, long> leadOutStarts,
                                      Dictionary<MediaTagType, byte[]> mediaTags,
                                      ErrorMessageHandler stoppingErrorMessage, out FullTOC.CDFullTOC? toc,
                                      Dictionary<byte, byte> trackFlags, UpdateStatusHandler updateStatus)
    {
        byte[]      cmdBuf;            // Data buffer
        const uint  sectorSize = 2352; // Full sector size
        bool        sense;             // Sense indicator
        List<Track> trackList = [];    // Tracks in disc
        byte[]      tmpBuf;            // Temporary buffer
        toc        = null;
        lastSector = 0;
        TrackType leadoutTrackType = TrackType.Audio;

        // We discarded all discs that falsify a TOC before requesting a real TOC
        // No TOC, no CD (or an empty one)
        updateStatus?.Invoke(Localization.Core.Reading_full_TOC);
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

        updateStatus?.Invoke(Localization.Core.Building_track_map);

        if(toc.HasValue)
        {
            FullTOC.TrackDataDescriptor[] sortedTracks =
                toc.Value.TrackDescriptors.OrderBy(static track => track.POINT).ToArray();

            foreach(FullTOC.TrackDataDescriptor trk in sortedTracks.Where(static trk => trk.ADR is 1 or 4))
            {
                switch(trk.POINT)
                {
                    case >= 0x01 and <= 0x63:
                        trackList.Add(new Track
                        {
                            Sequence = trk.POINT,
                            Session  = trk.SessionNumber,
                            Type = (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                   (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                       ? TrackType.Data
                                       : TrackType.Audio,
                            StartSector =
                                (ulong)(trk.PHOUR * 3600 * 75 + trk.PMIN * 60 * 75 + trk.PSEC * 75 + trk.PFRAME - 150),
                            BytesPerSector    = (int)sectorSize,
                            RawBytesPerSector = (int)sectorSize
                        });

                        trackFlags?.Add(trk.POINT, trk.CONTROL);

                        break;
                    case 0xA2:
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
                        leadOutStarts?.Add(trk.SessionNumber, lastSector + 1);

                        break;
                    }
                    case 0xA0 when trk.ADR == 1:
                        leadoutTrackType =
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                            (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                ? TrackType.Data
                                : TrackType.Audio;

                        break;
                }
            }
        }
        else
        {
            updateStatus?.Invoke(Localization.Core.Cannot_read_RAW_TOC_requesting_processed_one);
            sense = dev.ReadToc(out cmdBuf, out _, false, 0, dev.Timeout, out _);

            TOC.CDTOC? oldToc = TOC.Decode(cmdBuf);

            if((sense || !oldToc.HasValue) && !force)
            {
                stoppingErrorMessage?.Invoke(Localization.Core
                                                         .Could_not_read_TOC_if_you_want_to_continue_use_force_and_will_try_from_LBA_0_to_360000);

                return null;
            }

            if(oldToc.HasValue)
            {
                foreach(TOC.CDTOCTrackDataDescriptor trk in oldToc.Value.TrackDescriptors
                                                                  .Where(static trk => trk.ADR is 1 or 4)
                                                                  .OrderBy(static t => t.TrackNumber))
                {
                    switch(trk.TrackNumber)
                    {
                        case >= 0x01 and <= 0x63:
                            trackList.Add(new Track
                            {
                                Sequence = trk.TrackNumber,
                                Session  = 1,
                                Type = (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                       (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                           ? TrackType.Data
                                           : TrackType.Audio,
                                StartSector       = trk.TrackStartAddress,
                                BytesPerSector    = (int)sectorSize,
                                RawBytesPerSector = (int)sectorSize
                            });

                            trackFlags?.Add(trk.TrackNumber, trk.CONTROL);

                            break;
                        case 0xAA:
                            leadoutTrackType =
                                (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrack ||
                                (TocControl)(trk.CONTROL & 0x0D) == TocControl.DataTrackIncremental
                                    ? TrackType.Data
                                    : TrackType.Audio;

                            lastSector = trk.TrackStartAddress - 1;

                            break;
                    }
                }
            }
        }

        if(trackList.Count == 0)
        {
            updateStatus?.Invoke(Localization.Core.No_tracks_found_adding_a_single_track_from_zero_to_Lead_Out);

            // The TOC gave us no type information for this synthetic track. Rather than trusting the
            // TrackType.Audio default (which would later cause the dump loop to skip all garbage
            // detection for the whole disc), sample sector 0 and only call it Audio if it actually
            // looks like audio. If the read fails or is inconclusive, prefer a data-like type: treating
            // audio as data merely runs unnecessary validation, while the reverse silently disables it.
            TrackType singleTrackType = leadoutTrackType;

            if(singleTrackType == TrackType.Audio)
            {
                sense = dev.ReadCd(out cmdBuf,
                                   out _,
                                   0,
                                   sectorSize,
                                   1,
                                   MmcSectorTypes.AllTypes,
                                   false,
                                   false,
                                   true,
                                   MmcHeaderCodes.AllHeaders,
                                   true,
                                   true,
                                   MmcErrorField.None,
                                   MmcSubchannel.None,
                                   dev.Timeout,
                                   out _);

                singleTrackType = !sense && (IsData(cmdBuf) || IsScrambledData(cmdBuf, 0, out _))
                                      ? TrackType.CdMode2Formless
                                      : TrackType.Audio;
            }

            trackList.Add(new Track
            {
                Sequence          = 1,
                Session           = 1,
                Type              = singleTrackType,
                StartSector       = 0,
                BytesPerSector    = (int)sectorSize,
                RawBytesPerSector = (int)sectorSize
            });

            trackFlags?.Add(1, (byte)(singleTrackType == TrackType.Audio ? 0 : 4));
        }

        if(lastSector != 0) return [.. trackList];

        sense = dev.ReadCapacity16(out cmdBuf, out _, dev.Timeout, out _);

        if(!sense)
        {
            var temp = new byte[8];

            Array.Copy(cmdBuf, 0, temp, 0, 8);
            Array.Reverse(temp);
            lastSector = (long)BitConverter.ToUInt64(temp, 0);
        }
        else
        {
            sense = dev.ReadCapacity(out cmdBuf, out _, dev.Timeout, out _);

            if(!sense) lastSector = (cmdBuf[0] << 24) + (cmdBuf[1] << 16) + (cmdBuf[2] << 8) + cmdBuf[3] & 0xFFFFFFFF;
        }

        if(lastSector > 0) return [.. trackList];

        if(!force)
        {
            stoppingErrorMessage?.Invoke(Localization.Core
                                                     .Could_not_find_Lead_Out_if_you_want_to_continue_use_force_option);


            return null;
        }

        updateStatus?.Invoke(Localization.Core.WARNING_Could_not_find_Lead_Out_start_will_try_to_read_up_to);

        lastSector = 360000;

        return [.. trackList];
    }

    void CheckTracksMode(bool      readcd, IEnumerable<Track> tracks, uint blockSize, MmcSubchannel supportedSubchannel,
                         MediaType dskType)
    {
        byte[] cmdBuf; // Data buffer
        bool   sense;  // Sense indicator

        // Check mode for tracks
        foreach(Track trk in tracks.Where(static t => t.Type != TrackType.Audio))
        {
            if(!readcd)
            {
                trk.Type = TrackType.CdMode1;

                continue;
            }

            UpdateStatus?.Invoke(string.Format(Localization.Core.Checking_mode_for_track_0, trk.Sequence));

            sense = _dev.ReadCd(out cmdBuf,
                                out _,
                                (uint)(trk.StartSector + trk.Pregap),
                                blockSize,
                                1,
                                MmcSectorTypes.AllTypes,
                                false,
                                false,
                                true,
                                MmcHeaderCodes.AllHeaders,
                                true,
                                true,
                                MmcErrorField.None,
                                supportedSubchannel,
                                _dev.Timeout,
                                out _);

            if(sense)
            {
                UpdateStatus?.Invoke(string.Format(Localization.Core.Unable_to_guess_mode_for_track_0_continuing,
                                                   trk.Sequence));

                continue;
            }

            var bufOffset = 0;

            while(cmdBuf[0  + bufOffset] != 0x00 ||
                  cmdBuf[1  + bufOffset] != 0xFF ||
                  cmdBuf[2  + bufOffset] != 0xFF ||
                  cmdBuf[3  + bufOffset] != 0xFF ||
                  cmdBuf[4  + bufOffset] != 0xFF ||
                  cmdBuf[5  + bufOffset] != 0xFF ||
                  cmdBuf[6  + bufOffset] != 0xFF ||
                  cmdBuf[7  + bufOffset] != 0xFF ||
                  cmdBuf[8  + bufOffset] != 0xFF ||
                  cmdBuf[9  + bufOffset] != 0xFF ||
                  cmdBuf[10 + bufOffset] != 0xFF ||
                  cmdBuf[11 + bufOffset] != 0x00)
            {
                if(bufOffset + 16 >= cmdBuf.Length) break;

                bufOffset++;
            }

            if(bufOffset + 18 >= cmdBuf.Length)
            {
                UpdateStatus?.Invoke(string.Format(Localization.Core.Unable_to_guess_mode_for_track_0_continuing,
                                                   trk.Sequence));

                continue;
            }

            switch(cmdBuf[15 + bufOffset])
            {
                case 1:
                case 0x61: // Scrambled
                    UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_is_MODE1, trk.Sequence));
                    trk.Type = TrackType.CdMode1;

                    break;
                case 2:
                case 0x62: // Scrambled
                    if(dskType is MediaType.CDI or MediaType.CDIREADY)
                    {
                        UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_is_MODE2, trk.Sequence));
                        trk.Type = TrackType.CdMode2Formless;

                        break;
                    }

                    if((cmdBuf[0x012 + bufOffset] & 0x20) == 0x20) // mode 2 form 2
                    {
                        UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_is_MODE2_FORM_2, trk.Sequence));
                        trk.Type = TrackType.CdMode2Form2;

                        break;
                    }

                    UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_is_MODE2_FORM_1, trk.Sequence));
                    trk.Type = TrackType.CdMode2Form1;

                    // These media type specifications do not legally allow mode 2 tracks to be present
                    if(dskType is MediaType.CDROM or MediaType.CDPLUS or MediaType.CDV) dskType = MediaType.CD;

                    break;
                default:
                    UpdateStatus?.Invoke(string.Format(Localization.Core.Track_0_is_unknown_mode_1,
                                                       trk.Sequence,
                                                       cmdBuf[15]));


                    break;
            }
        }
    }
}