// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Pregap.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Calculates CompactDisc track pregaps.
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

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Core.Logging;
using Aaru.Devices;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    // TODO: Fix offset
    /// <summary>Reads the first track pregap from a CompactDisc</summary>
    /// <param name="blockSize">Block size in bytes</param>
    /// <param name="currentSpeed">Current speed</param>
    /// <param name="mediaTags">List of media tags</param>
    /// <param name="supportedSubchannel">Subchannel the drive can read</param>
    /// <param name="totalDuration">Total time spent sending commands to a drive</param>
    void ReadCdFirstTrackPregap(uint blockSize, ref double currentSpeed, Dictionary<MediaTagType, byte[]> mediaTags,
                                MmcSubchannel supportedSubchannel, ref double totalDuration)
    {
        bool     sense;                           // Sense indicator
        byte[]   cmdBuf;                          // Data buffer
        double   cmdDuration;                     // Command execution time
        DateTime timeSpeedStart;                  // Time of start for speed calculation
        ulong    sectorSpeedStart            = 0; // Used to calculate correct speed
        bool     gotFirstTrackPregap         = false;
        int      firstTrackPregapSectorsGood = 0;
        var      firstTrackPregapMs          = new MemoryStream();

        _dumpLog.WriteLine(Localization.Core.Reading_first_track_pregap);
        UpdateStatus?.Invoke(Localization.Core.Reading_first_track_pregap);
        InitProgress?.Invoke();
        timeSpeedStart = DateTime.UtcNow;

        for(int firstTrackPregapBlock = -150; firstTrackPregapBlock < 0 && _resume.NextBlock == 0;
            firstTrackPregapBlock++)
        {
            if(_aborted)
            {
                _dumpLog.WriteLine(Localization.Core.Aborted);
                UpdateStatus?.Invoke(Localization.Core.Aborted);

                break;
            }

            PulseProgress?.Invoke(string.Format(Localization.Core.Trying_to_read_first_track_pregap_sector_0_1_MiB_sec,
                                                firstTrackPregapBlock, currentSpeed));

            // ReSharper disable IntVariableOverflowInUncheckedContext
            sense = _dev.ReadCd(out cmdBuf, out _, (uint)firstTrackPregapBlock, blockSize, 1, MmcSectorTypes.AllTypes,
                                false, false, true, MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None,
                                supportedSubchannel, _dev.Timeout, out cmdDuration);

            // ReSharper restore IntVariableOverflowInUncheckedContext

            if(!sense &&
               !_dev.Error)
            {
                firstTrackPregapMs.Write(cmdBuf, 0, (int)blockSize);
                gotFirstTrackPregap = true;
                firstTrackPregapSectorsGood++;
                totalDuration += cmdDuration;
            }
            else
            {
                // Write empty data
                if(gotFirstTrackPregap)
                    firstTrackPregapMs.Write(new byte[blockSize], 0, (int)blockSize);
            }

            sectorSpeedStart++;

            double elapsed = (DateTime.UtcNow - timeSpeedStart).TotalSeconds;

            if(elapsed <= 0)
                continue;

            currentSpeed     = sectorSpeedStart * blockSize / (1048576 * elapsed);
            sectorSpeedStart = 0;
            timeSpeedStart   = DateTime.UtcNow;
        }

        if(firstTrackPregapSectorsGood > 0)
            mediaTags.Add(MediaTagType.CD_FirstTrackPregap, firstTrackPregapMs.ToArray());

        EndProgress?.Invoke();

        UpdateStatus?.Invoke(string.Format(Localization.Core.Got_0_first_track_pregap_sectors,
                                           firstTrackPregapSectorsGood));

        _dumpLog.WriteLine(Localization.Core.Got_0_first_track_pregap_sectors, firstTrackPregapSectorsGood);

        firstTrackPregapMs.Close();
    }

    /// <summary>Calculate track pregaps</summary>
    /// <param name="dev">Device</param>
    /// <param name="dumpLog">Dumping log</param>
    /// <param name="updateStatus">Progress update callback</param>
    /// <param name="tracks">List of tracks</param>
    /// <param name="supportsPqSubchannel">Set if drive supports reading PQ subchannel</param>
    /// <param name="supportsRwSubchannel">Set if drive supports reading RW subchannel</param>
    /// <param name="dbDev">Database entry for device</param>
    /// <param name="inexactPositioning">Set if we found the drive does not return the exact subchannel we requested</param>
    /// <param name="dumping">Set if dumping, otherwise media info</param>
    public static void SolveTrackPregaps(Device dev, DumpLog dumpLog, UpdateStatusHandler updateStatus, Track[] tracks,
                                         bool supportsPqSubchannel, bool supportsRwSubchannel,
                                         Database.Models.Device dbDev, out bool inexactPositioning, bool dumping)
    {
        bool                  sense  = true; // Sense indicator
        byte[]                subBuf = null;
        int                   posQ;
        uint                  retries;
        bool?                 bcd = null;
        byte[]                crc;
        Dictionary<uint, int> pregaps = new();
        inexactPositioning = false;

        if(!supportsPqSubchannel &&
           !supportsRwSubchannel)
            return;

        // Check if subchannel is BCD
        for(retries = 0; retries < 10; retries++)
        {
            sense = supportsRwSubchannel ? GetSectorForPregapRaw(dev, 11, dbDev, out subBuf, false)
                        : GetSectorForPregapQ16(dev, 11, out subBuf, false);

            if(sense)
                continue;

            bcd = (subBuf[9] & 0x10) > 0;

            break;
        }

        AaruConsole.DebugWriteLine("Pregap calculator", bcd switch
        {
            true  => Localization.Core.Subchannel_is_BCD,
            false => Localization.Core.Subchannel_is_not_BCD,
            _     => Localization.Core.Could_not_detect_drive_subchannel_BCD
        });

        if(bcd is null)
        {
            dumpLog?.WriteLine(Localization.Core.
                                            Could_not_detect_if_drive_subchannel_is_BCD_or_not_pregaps_could_not_be_calculated_dump_may_be_incorrect);

            updateStatus?.Invoke(Localization.Core.
                                              Could_not_detect_if_drive_subchannel_is_BCD_or_not_pregaps_could_not_be_calculated_dump_may_be_incorrect);

            return;
        }

        // Initialize the dictionary
        foreach(Track t in tracks)
            pregaps[t.Sequence] = 0;

        for(int t = 0; t < tracks.Length; t++)
        {
            Track track        = tracks[t];
            int   trackRetries = 0;

            // First track of each session has at least 150 sectors of pregap and is not always readable
            if(tracks.Where(trk => trk.Session == track.Session).MinBy(trk => trk.Sequence).Sequence == track.Sequence)
            {
                AaruConsole.DebugWriteLine("Pregap calculator", Localization.Core.Skipping_track_0, track.Sequence);

                if(track.Sequence > 1)
                    pregaps[track.Sequence] = 150;

                continue;
            }

            if(t                  > 0               &&
               tracks[t - 1].Type == tracks[t].Type &&
               dumping)
            {
                AaruConsole.DebugWriteLine("Pregap calculator", Localization.Core.Skipping_track_0, track.Sequence);

                continue;
            }

            if(dumping && dev.Manufacturer.ToLowerInvariant().StartsWith("plextor", StringComparison.Ordinal))
            {
                AaruConsole.DebugWriteLine("Pregap calculator",
                                           Localization.Core.Skipping_track_0_due_to_Plextor_firmware_bug,
                                           track.Sequence);

                continue;
            }

            AaruConsole.DebugWriteLine("Pregap calculator", Localization.Core.Track_0, track.Sequence);

            int   lba           = (int)track.StartSector - 1;
            bool  pregapFound   = false;
            Track previousTrack = tracks.FirstOrDefault(trk => trk.Sequence == track.Sequence - 1);

            bool goneBack                      = false;
            bool goFront                       = false;
            bool forward                       = false;
            bool crcOk                         = false;
            bool previousPregapIsPreviousTrack = false;

            // Check if pregap is 0
            for(retries = 0; retries < 10 && !pregapFound; retries++)
            {
                sense = supportsRwSubchannel
                            ? GetSectorForPregapRaw(dev, (uint)lba, dbDev, out subBuf, track.Type == TrackType.Audio)
                            : GetSectorForPregapQ16(dev, (uint)lba, out subBuf, track.Type        == TrackType.Audio);

                if(sense)
                {
                    AaruConsole.DebugWriteLine("Pregap calculator", Localization.Core.LBA_0_Try_1_Sense_2, lba,
                                               retries + 1, sense);

                    continue;
                }

                if(bcd == false)
                    BinaryToBcdQ(subBuf);

                CRC16CCITTContext.Data(subBuf, 10, out crc);

                AaruConsole.DebugWriteLine("Pregap calculator",
                                           Localization.Core.
                                                        LBA_0_Try_1_Sense_2_Q_3_4_5_6_7_8_9_10_11_12_CRC_13_14_Calculated_CRC_15_16,
                                           lba, retries + 1, sense, subBuf[0], subBuf[1], subBuf[2], subBuf[3],
                                           subBuf[4], subBuf[5], subBuf[6], subBuf[7], subBuf[8], subBuf[9], subBuf[10],
                                           subBuf[11], crc[0], crc[1]);

                crcOk = crc[0] == subBuf[10] && crc[1] == subBuf[11];

                // Try to do a simple correction
                if(!crcOk)
                {
                    // Data track cannot have 11xxb in CONTROL
                    if((subBuf[0] & 0x40) > 0)
                        subBuf[0] &= 0x7F;

                    // ADR only uses two bits
                    subBuf[0] &= 0xF3;

                    // Don't care about other Q modes
                    if((subBuf[0] & 0xF) == 1)
                    {
                        // ZERO only used in DDCD
                        subBuf[6] = 0;

                        // Fix BCD numbering
                        for(int i = 1; i < 10; i++)
                        {
                            if((subBuf[i] & 0xF0) > 0xA0)
                                subBuf[i] &= 0x7F;

                            if((subBuf[i] & 0x0F) > 0x0A)
                                subBuf[i] &= 0xF7;
                        }
                    }

                    CRC16CCITTContext.Data(subBuf, 10, out crc);

                    crcOk = crc[0] == subBuf[10] && crc[1] == subBuf[11];

                    if(crcOk)
                        AaruConsole.DebugWriteLine("Pregap calculator",
                                                   Localization.Core.
                                                                LBA_0_Try_1_Sense_2_Q_FIXED_3_4_5_6_7_8_9_10_11_12_CRC_13_14_Calculated_CRC_15_16,
                                                   lba, retries + 1, sense, subBuf[0], subBuf[1], subBuf[2], subBuf[3],
                                                   subBuf[4], subBuf[5], subBuf[6], subBuf[7], subBuf[8], subBuf[9],
                                                   subBuf[10], subBuf[11], crc[0], crc[1]);
                    else
                        continue;
                }

                BcdToBinaryQ(subBuf);

                // Q position
                if((subBuf[0] & 0xF) != 1)
                    continue;

                posQ = (subBuf[7] * 60 * 75) + (subBuf[8] * 75) + subBuf[9] - 150;

                if(subBuf[1] != track.Sequence - 1 ||
                   subBuf[2] == 0                  ||
                   posQ      != lba)
                    break;

                pregaps[track.Sequence] = 0;

                pregapFound = true;
            }

            if(pregapFound)
                continue;

            // Calculate pregap
            lba = (int)track.StartSector - 150;

            while(lba > (int)previousTrack.StartSector &&
                  lba <= (int)track.StartSector)
            {
                // Some drives crash if you try to read just before the previous read, so seek away first
                if(!forward)
                    sense = supportsRwSubchannel
                                ? GetSectorForPregapRaw(dev, (uint)lba - 10, dbDev, out subBuf,
                                                        track.Type == TrackType.Audio)
                                : GetSectorForPregapQ16(dev, (uint)lba - 10, out subBuf, track.Type == TrackType.Audio);

                for(retries = 0; retries < 10; retries++)
                {
                    sense = supportsRwSubchannel
                                ? GetSectorForPregapRaw(dev, (uint)lba, dbDev, out subBuf,
                                                        track.Type == TrackType.Audio)
                                : GetSectorForPregapQ16(dev, (uint)lba, out subBuf, track.Type == TrackType.Audio);

                    if(sense)
                        continue;

                    if(bcd == false)
                        BinaryToBcdQ(subBuf);

                    CRC16CCITTContext.Data(subBuf, 10, out crc);

                    AaruConsole.DebugWriteLine("Pregap calculator",
                                               Localization.Core.
                                                            LBA_0_Try_1_Sense_2_Q_3_4_5_6_7_8_9_10_11_12_CRC_13_14_Calculated_CRC_15_16,
                                               lba, retries + 1, sense, subBuf[0], subBuf[1], subBuf[2], subBuf[3],
                                               subBuf[4], subBuf[5], subBuf[6], subBuf[7], subBuf[8], subBuf[9],
                                               subBuf[10], subBuf[11], crc[0], crc[1]);

                    crcOk = crc[0] == subBuf[10] && crc[1] == subBuf[11];

                    // Try to do a simple correction
                    if(!crcOk)
                    {
                        // Data track cannot have 11xxb in CONTROL
                        if((subBuf[0] & 0x40) > 0)
                            subBuf[0] &= 0x7F;

                        // ADR only uses two bits
                        subBuf[0] &= 0xF3;

                        // Don't care about other Q modes
                        if((subBuf[0] & 0xF) == 1)
                        {
                            // ZERO only used in DDCD
                            subBuf[6] = 0;

                            // Fix BCD numbering
                            for(int i = 1; i < 10; i++)
                            {
                                if((subBuf[i] & 0xF0) > 0xA0)
                                    subBuf[i] &= 0x7F;

                                if((subBuf[i] & 0x0F) > 0x0A)
                                    subBuf[i] &= 0xF7;
                            }
                        }

                        CRC16CCITTContext.Data(subBuf, 10, out crc);

                        crcOk = crc[0] == subBuf[10] && crc[1] == subBuf[11];

                        if(crcOk)
                        {
                            AaruConsole.DebugWriteLine("Pregap calculator",
                                                       Localization.Core.
                                                                    LBA_0_Try_1_Sense_2_Q_FIXED_3_4_5_6_7_8_9_10_11_12_CRC_13_14_Calculated_CRC_15_16,
                                                       lba, retries + 1, sense, subBuf[0], subBuf[1], subBuf[2],
                                                       subBuf[3], subBuf[4], subBuf[5], subBuf[6], subBuf[7], subBuf[8],
                                                       subBuf[9], subBuf[10], subBuf[11], crc[0], crc[1]);

                            break;
                        }
                    }

                    if(crcOk)
                        break;
                }

                if(retries == 10)
                {
                    if(sense)
                    {
                        trackRetries++;

                        if(trackRetries >= 10)
                        {
                            if(pregaps[track.Sequence] == 0)
                            {
                                if((previousTrack.Type == TrackType.Audio && track.Type != TrackType.Audio) ||
                                   (previousTrack.Type != TrackType.Audio && track.Type == TrackType.Audio))
                                {
                                    dumpLog?.WriteLine(Localization.Core.
                                                                    Could_not_read_subchannel_for_this_track_supposing_hundred_fifty_sectors);

                                    updateStatus?.Invoke(Localization.Core.
                                                                      Could_not_read_subchannel_for_this_track_supposing_hundred_fifty_sectors);
                                }
                                else
                                {
                                    dumpLog?.WriteLine(Localization.Core.
                                                                    Could_not_read_subchannel_for_this_track_supposing_zero_sectors);

                                    updateStatus?.Invoke(Localization.Core.
                                                                      Could_not_read_subchannel_for_this_track_supposing_zero_sectors);
                                }
                            }
                            else
                            {
                                dumpLog?.
                                    WriteLine(string.
                                                  Format(Localization.Core.Could_not_read_subchannel_for_this_track_supposing_0_sectors,
                                                         pregaps[track.Sequence]));

                                updateStatus?.
                                    Invoke(string.
                                               Format(Localization.Core.Could_not_read_subchannel_for_this_track_supposing_0_sectors,
                                                      pregaps[track.Sequence]));
                            }

                            break;
                        }

                        dumpLog?.WriteLine(string.Format(Localization.Core.Could_not_read_subchannel_for_sector_0,
                                                         lba));

                        updateStatus?.Invoke(string.Format(Localization.Core.Could_not_read_subchannel_for_sector_0,
                                                           lba));

                        lba++;
                        forward = true;

                        continue;
                    }

                    dumpLog?.WriteLine(string.Format(Localization.Core.Could_not_get_correct_subchannel_for_sector_0,
                                                     lba));

                    updateStatus?.Invoke(string.Format(Localization.Core.Could_not_get_correct_subchannel_for_sector_0,
                                                       lba));
                }

                if(subBuf.All(b => b == 0))
                {
                    inexactPositioning = true;

                    AaruConsole.DebugWriteLine("Pregap calculator", Localization.Core.All_Q_empty_for_LBA_0, lba);

                    break;
                }

                BcdToBinaryQ(subBuf);

                // If it's not Q position
                if((subBuf[0] & 0xF) != 1)
                {
                    // This means we already searched back, so search forward
                    if(goFront)
                    {
                        lba++;
                        forward = true;

                        if(lba == (int)previousTrack.StartSector)
                            break;

                        continue;
                    }

                    // Search back
                    goneBack = true;
                    lba--;
                    forward = false;

                    continue;
                }

                // Previous track
                if(subBuf[1] < track.Sequence)
                {
                    lba++;
                    forward                       = true;
                    previousPregapIsPreviousTrack = true;

                    // Already gone back, so go forward
                    if(goneBack)
                        goFront = true;

                    continue;
                }

                // Same track, but not pregap
                if(subBuf[1] == track.Sequence &&
                   subBuf[2] > 0)
                {
                    lba--;
                    forward = false;

                    if(previousPregapIsPreviousTrack)
                        break;

                    continue;
                }

                previousPregapIsPreviousTrack = false;

                // Pregap according to Q position
                posQ = (subBuf[7] * 60 * 75) + (subBuf[8] * 75) + subBuf[9] - 150;
                int diff    = posQ                   - lba;
                int pregapQ = (int)track.StartSector - lba;

                if(diff != 0)
                {
                    AaruConsole.DebugWriteLine("Pregap calculator",
                                               Localization.Core.Invalid_Q_position_for_LBA_0_got_1, lba, posQ);

                    inexactPositioning = true;
                }

                // Received a Q post the LBA we wanted, just go back. If we are already going forward, break
                if(posQ > lba)
                {
                    if(forward)
                        break;

                    lba--;

                    continue;
                }

                // Bigger than known change, otherwise we found it
                if(pregapQ > pregaps[track.Sequence])
                {
                    // If CRC is not OK, only accept pregaps less than 10 sectors longer than previously now
                    if(crcOk || pregapQ - pregaps[track.Sequence] < 10)
                    {
                        AaruConsole.DebugWriteLine("Pregap calculator", Localization.Core.Pregap_for_track_0_1,
                                                   track.Sequence, pregapQ);

                        pregaps[track.Sequence] = pregapQ;
                    }

                    // We are going forward, so we have already been in the previous track, so add 1 to pregap and get out of here
                    else if(forward)
                    {
                        pregaps[track.Sequence]++;

                        break;
                    }
                }
                else if(pregapQ == pregaps[track.Sequence])
                    break;

                lba--;
                forward = false;
            }
        }

        foreach(Track trk in tracks)
        {
            trk.Pregap = (ulong)pregaps[trk.Sequence];

            // Do not reduce pregap, or starting position of session's first track
            if(tracks.Where(t => t.Session == trk.Session).MinBy(t => t.Sequence).Sequence == trk.Sequence)
                continue;

            if(dumping)
            {
                // Minus five, to ensure dumping will fix if there is a pregap LBA 0
                int red = 5;

                while(trk.Pregap > 0 &&
                      red        > 0)
                {
                    trk.Pregap--;
                    red--;
                }
            }

            trk.StartSector -= trk.Pregap;

        #if DEBUG
            dumpLog?.WriteLine(string.Format(Localization.Core.Track_0_pregap_is_1_sectors, trk.Sequence, trk.Pregap));

            updateStatus?.Invoke(string.Format(Localization.Core.Track_0_pregap_is_1_sectors, trk.Sequence,
                                               trk.Pregap));
        #endif
        }
    }

    /// <summary>Reads a RAW subchannel sector for pregap calculation</summary>
    /// <param name="dev">Device</param>
    /// <param name="lba">LBA</param>
    /// <param name="dbDev">Database entry for device</param>
    /// <param name="subBuf">Read subchannel</param>
    /// <param name="audioTrack">Set if it is an audio track</param>
    /// <returns><c>true</c> if read correctly, <c>false</c> otherwise</returns>
    static bool GetSectorForPregapRaw(Device dev, uint lba, Database.Models.Device dbDev, out byte[] subBuf,
                                      bool audioTrack)
    {
        byte[] cmdBuf;
        bool   sense;
        subBuf = null;

        if(audioTrack)
        {
            sense = dev.ReadCd(out cmdBuf, out _, lba, 2448, 1, MmcSectorTypes.Cdda, false, false, false,
                               MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout,
                               out _);

            if(sense)
                sense = dev.ReadCd(out cmdBuf, out _, lba, 2448, 1, MmcSectorTypes.AllTypes, false, false, true,
                                   MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Raw,
                                   dev.Timeout, out _);
        }
        else
        {
            sense = dev.ReadCd(out cmdBuf, out _, lba, 2448, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Raw,
                               dev.Timeout, out _);

            if(sense)
                sense = dev.ReadCd(out cmdBuf, out _, lba, 2448, 1, MmcSectorTypes.Cdda, false, false, false,
                                   MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout,
                                   out _);
        }

        if(!sense)
        {
            byte[] tmpBuf = new byte[96];
            Array.Copy(cmdBuf, 2352, tmpBuf, 0, 96);
            subBuf = DeinterleaveQ(tmpBuf);
        }
        else
        {
            sense = dev.ReadCd(out cmdBuf, out _, lba, 96, 1, MmcSectorTypes.AllTypes, false, false, false,
                               MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout,
                               out _);

            if(sense)
                sense = dev.ReadCd(out cmdBuf, out _, lba, 96, 1, MmcSectorTypes.Cdda, false, false, false,
                                   MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Raw,
                                   dev.Timeout, out _);

            if(!sense)
                subBuf = DeinterleaveQ(cmdBuf);
            else if(dbDev?.ATAPI?.RemovableMedias?.Any(d => d.SupportsPlextorReadCDDA == true) == true ||
                    dbDev?.SCSI?.RemovableMedias?.Any(d => d.SupportsPlextorReadCDDA  == true) == true ||
                    dev.Manufacturer.ToLowerInvariant()                                        == "plextor")
                sense = dev.PlextorReadCdDa(out cmdBuf, out _, lba, 96, 1, PlextorSubchannel.All, dev.Timeout, out _);

            {
                if(!sense)
                    subBuf = DeinterleaveQ(cmdBuf);
            }
        }

        return sense;
    }

    /// <summary>Reads a Q16 subchannel sector for pregap calculation</summary>
    /// <param name="dev">Device</param>
    /// <param name="lba">LBA</param>
    /// <param name="subBuf">Read subchannel</param>
    /// <param name="audioTrack">Set if it is an audio track</param>
    /// <returns><c>true</c> if read correctly, <c>false</c> otherwise</returns>
    static bool GetSectorForPregapQ16(Device dev, uint lba, out byte[] subBuf, bool audioTrack)
    {
        byte[] cmdBuf;
        bool   sense;
        subBuf = null;

        if(audioTrack)
        {
            sense = dev.ReadCd(out cmdBuf, out _, lba, 2368, 1, MmcSectorTypes.Cdda, false, false, false,
                               MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.Q16, dev.Timeout,
                               out _);

            if(sense)
                sense = dev.ReadCd(out cmdBuf, out _, lba, 2368, 1, MmcSectorTypes.AllTypes, false, false, true,
                                   MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Q16,
                                   dev.Timeout, out _);
        }
        else
        {
            sense = dev.ReadCd(out cmdBuf, out _, lba, 2368, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Q16,
                               dev.Timeout, out _);

            if(sense)
                sense = dev.ReadCd(out cmdBuf, out _, lba, 2368, 1, MmcSectorTypes.Cdda, false, false, false,
                                   MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.Q16, dev.Timeout,
                                   out _);
        }

        if(!sense)
        {
            subBuf = new byte[16];
            Array.Copy(cmdBuf, 2352, subBuf, 0, 16);
        }
        else
        {
            sense = dev.ReadCd(out cmdBuf, out _, lba, 16, 1, MmcSectorTypes.AllTypes, false, false, false,
                               MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Q16, dev.Timeout,
                               out _);

            if(sense)
                sense = dev.ReadCd(out cmdBuf, out _, lba, 16, 1, MmcSectorTypes.Cdda, false, false, false,
                                   MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Q16,
                                   dev.Timeout, out _);

            if(!sense)
                subBuf = cmdBuf;
        }

        return sense;
    }

    /// <summary>De-interleaves Q subchannel</summary>
    /// <param name="subchannel">Interleaved subchannel</param>
    /// <returns>De-interleaved Q subchannel</returns>
    static byte[] DeinterleaveQ(byte[] subchannel)
    {
        int[] q = new int[subchannel.Length / 8];

        // De-interlace Q subchannel
        for(int iq = 0; iq < subchannel.Length; iq += 8)
        {
            q[iq / 8] =  (subchannel[iq] & 0x40) << 1;
            q[iq / 8] += subchannel[iq + 1] & 0x40;
            q[iq / 8] += (subchannel[iq + 2] & 0x40) >> 1;
            q[iq / 8] += (subchannel[iq + 3] & 0x40) >> 2;
            q[iq / 8] += (subchannel[iq + 4] & 0x40) >> 3;
            q[iq / 8] += (subchannel[iq + 5] & 0x40) >> 4;
            q[iq / 8] += (subchannel[iq + 6] & 0x40) >> 5;
            q[iq / 8] += (subchannel[iq + 7] & 0x40) >> 6;
        }

        byte[] deQ = new byte[q.Length];

        for(int iq = 0; iq < q.Length; iq++)
            deQ[iq] = (byte)q[iq];

        return deQ;
    }

    /// <summary>In place converts Q subchannel from binary to BCD numbering</summary>
    /// <param name="q">Q subchannel</param>
    static void BinaryToBcdQ(byte[] q)
    {
        q[1] = (byte)(((q[1] / 10) << 4) + (q[1] % 10));
        q[2] = (byte)(((q[2] / 10) << 4) + (q[2] % 10));
        q[3] = (byte)(((q[3] / 10) << 4) + (q[3] % 10));
        q[4] = (byte)(((q[4] / 10) << 4) + (q[4] % 10));
        q[5] = (byte)(((q[5] / 10) << 4) + (q[5] % 10));
        q[6] = (byte)(((q[6] / 10) << 4) + (q[6] % 10));
        q[7] = (byte)(((q[7] / 10) << 4) + (q[7] % 10));
        q[8] = (byte)(((q[8] / 10) << 4) + (q[8] % 10));
        q[9] = (byte)(((q[9] / 10) << 4) + (q[9] % 10));
    }

    /// <summary>In place converts Q subchannel from BCD to binary numbering</summary>
    /// <param name="q">Q subchannel</param>
    static void BcdToBinaryQ(byte[] q)
    {
        q[1] = (byte)((q[1] / 16 * 10) + (q[1] & 0x0F));
        q[2] = (byte)((q[2] / 16 * 10) + (q[2] & 0x0F));
        q[3] = (byte)((q[3] / 16 * 10) + (q[3] & 0x0F));
        q[4] = (byte)((q[4] / 16 * 10) + (q[4] & 0x0F));
        q[5] = (byte)((q[5] / 16 * 10) + (q[5] & 0x0F));
        q[6] = (byte)((q[6] / 16 * 10) + (q[6] & 0x0F));
        q[7] = (byte)((q[7] / 16 * 10) + (q[7] & 0x0F));
        q[8] = (byte)((q[8] / 16 * 10) + (q[8] & 0x0F));
        q[9] = (byte)((q[9] / 16 * 10) + (q[9] & 0x0F));
    }
}