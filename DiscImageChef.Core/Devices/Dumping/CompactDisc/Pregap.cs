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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Console;
using DiscImageChef.Core.Logging;
using DiscImageChef.Devices;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace DiscImageChef.Core.Devices.Dumping
{
    partial class Dump
    {
        // TODO: Fix offset
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

            _dumpLog.WriteLine("Reading first track pregap");
            UpdateStatus?.Invoke("Reading first track pregap");
            InitProgress?.Invoke();
            timeSpeedStart = DateTime.UtcNow;

            for(int firstTrackPregapBlock = -150; firstTrackPregapBlock < 0 && _resume.NextBlock == 0;
                firstTrackPregapBlock++)
            {
                if(_aborted)
                {
                    _dumpLog.WriteLine("Aborted!");
                    UpdateStatus?.Invoke("Aborted!");

                    break;
                }

                PulseProgress?.
                    Invoke($"Trying to read first track pregap sector {firstTrackPregapBlock} ({currentSpeed:F3} MiB/sec.)");

                sense = _dev.ReadCd(out cmdBuf, out _, (uint)firstTrackPregapBlock, blockSize, 1,
                                    MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true, true,
                                    MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);

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

                if(elapsed < 1)
                    continue;

                currentSpeed     = (sectorSpeedStart * blockSize) / (1048576 * elapsed);
                sectorSpeedStart = 0;
                timeSpeedStart   = DateTime.UtcNow;
            }

            if(firstTrackPregapSectorsGood > 0)
                mediaTags.Add(MediaTagType.CD_FirstTrackPregap, firstTrackPregapMs.ToArray());

            EndProgress?.Invoke();
            UpdateStatus?.Invoke($"Got {firstTrackPregapSectorsGood} first track pregap sectors.");
            _dumpLog.WriteLine("Got {0} first track pregap sectors.", firstTrackPregapSectorsGood);

            firstTrackPregapMs.Close();
        }

        public static void SolveTrackPregaps(Device dev, DumpLog dumpLog, UpdateStatusHandler updateStatus,
                                             Track[] tracks, bool supportsPqSubchannel, bool supportsRwSubchannel,
                                             Database.Models.Device dbDev, out bool inexactPositioning)
        {
            bool                  sense  = true; // Sense indicator
            byte[]                subBuf = null;
            int                   posQ;
            uint                  retries;
            bool?                 bcd = null;
            byte[]                crc;
            Dictionary<uint, int> pregaps = new Dictionary<uint, int>();
            inexactPositioning = false;

            if(!supportsPqSubchannel &&
               !supportsRwSubchannel)
                return;

            // Check if subchannel is BCD
            for(retries = 0; retries < 10; retries++)
            {
                sense = supportsRwSubchannel ? GetSectorForPregapRaw(dev, 11, dbDev, out subBuf)
                            : GetSectorForPregapQ16(dev, 11, dbDev, out subBuf);

                if(sense)
                    continue;

                bcd = (subBuf[9] & 0x10) > 0;

                break;
            }

            DicConsole.DebugWriteLine("Pregap calculator", bcd == true
                                                               ? "Subchannel is BCD"
                                                               : bcd == false
                                                                   ? "Subchannel is not BCD"
                                                                   : "Could not detect drive subchannel BCD");

            if(bcd is null)
            {
                dumpLog?.WriteLine("Could not detect if drive subchannel is BCD or not, pregaps could not be calculated, dump may be incorrect...");

                updateStatus?.
                    Invoke("Could not detect if drive subchannel is BCD or not, pregaps could not be calculated, dump may be incorrect...");

                return;
            }

            // Initialize the dictionary
            for(int i = 0; i < tracks.Length; i++)
                pregaps[tracks[i].TrackSequence] = 0;

            foreach(Track track in tracks)
            {
                // First track of each session has at least 150 sectors of pregap and is not readable always
                if(tracks.Where(t => t.TrackSession == track.TrackSession).OrderBy(t => t.TrackSequence).
                          FirstOrDefault().TrackSequence == track.TrackSequence)
                {
                    DicConsole.DebugWriteLine("Pregap calculator", "Skipping track {0}", track.TrackSequence);

                    if(track.TrackSequence > 1)
                        pregaps[track.TrackSequence] = 150;

                    continue;
                }

                DicConsole.DebugWriteLine("Pregap calculator", "Track {0}", track.TrackSequence);

                int   lba           = (int)track.TrackStartSector - 1;
                bool  pregapFound   = false;
                Track previousTrack = tracks.FirstOrDefault(t => t.TrackSequence == track.TrackSequence - 1);

                bool goneBack = false;
                bool goFront  = false;
                bool forward  = false;

                // Check if pregap is 0
                for(retries = 0; retries < 10; retries++)
                {
                    sense = supportsRwSubchannel ? GetSectorForPregapRaw(dev, (uint)lba, dbDev, out subBuf)
                                : GetSectorForPregapQ16(dev, (uint)lba, dbDev, out subBuf);

                    if(sense)
                    {
                        DicConsole.DebugWriteLine("Pregap calculator", "LBA: {0}, Try {1}, Sense {2}", lba, retries + 1,
                                                  sense);

                        continue;
                    }

                    if(bcd == false)
                        BinaryToBcdQ(subBuf);

                    CRC16CCITTContext.Data(subBuf, 10, out crc);

                    DicConsole.DebugWriteLine("Pregap calculator",
                                              "LBA: {0}, Try {1}, Sense {2}, Q: {3:X2} {4:X2} {5:X2} {6:X2} {7:X2} {8:X2} {9:X2} {10:X2} {11:X2} {12:X2} CRC 0x{13:X2}{14:X2}, Calculated CRC: 0x{15:X2}{16:X2}",
                                              lba, retries + 1, sense, subBuf[0], subBuf[1], subBuf[2], subBuf[3],
                                              subBuf[4], subBuf[5], subBuf[6], subBuf[7], subBuf[8], subBuf[9],
                                              subBuf[10], subBuf[11], crc[0], crc[1]);

                    if(crc[0] != subBuf[10] ||
                       crc[1] != subBuf[11])
                        continue;

                    BcdToBinaryQ(subBuf);

                    // Q position
                    if((subBuf[0] & 0xF) != 1)
                        continue;

                    posQ = ((subBuf[7] * 60 * 75) + (subBuf[8] * 75) + subBuf[9]) - 150;

                    if(subBuf[1] != track.TrackSequence - 1 ||
                       subBuf[2] == 0                       ||
                       posQ      != lba)
                        break;

                    pregaps[track.TrackSequence] = 0;

                    pregapFound = true;
                }

                if(pregapFound)
                    continue;

                // Calculate pregap
                lba = (int)track.TrackStartSector - 150;

                while(lba > (int)previousTrack.TrackStartSector &&
                      lba <= (int)track.TrackStartSector)
                {
                    // Some drives crash if you try to read just before the previous read, so seek away first
                    if(!forward)
                        sense = supportsRwSubchannel ? GetSectorForPregapRaw(dev, (uint)lba - 10, dbDev, out subBuf)
                                    : GetSectorForPregapQ16(dev, (uint)lba                  - 10, dbDev, out subBuf);

                    bool crcOk = false;

                    for(retries = 0; retries < 10; retries++)
                    {
                        sense = supportsRwSubchannel ? GetSectorForPregapRaw(dev, (uint)lba, dbDev, out subBuf)
                                    : GetSectorForPregapQ16(dev, (uint)lba, dbDev, out subBuf);

                        if(sense)
                            continue;

                        if(bcd == false)
                            BinaryToBcdQ(subBuf);

                        CRC16CCITTContext.Data(subBuf, 10, out crc);

                        DicConsole.DebugWriteLine("Pregap calculator",
                                                  "LBA: {0}, Try {1}, Sense {2}, Q: {3:X2} {4:X2} {5:X2} {6:X2} {7:X2} {8:X2} {9:X2} {10:X2} {11:X2} {12:X2} CRC 0x{13:X2}{14:X2}, Calculated CRC: 0x{15:X2}{16:X2}",
                                                  lba, retries + 1, sense, subBuf[0], subBuf[1], subBuf[2], subBuf[3],
                                                  subBuf[4], subBuf[5], subBuf[6], subBuf[7], subBuf[8], subBuf[9],
                                                  subBuf[10], subBuf[11], crc[0], crc[1]);

                        crcOk = crc[0] == subBuf[10] && crc[1] == subBuf[11];

                        if(crcOk)
                            break;
                    }

                    if(retries == 10)
                    {
                        dumpLog?.WriteLine($"Could not get correct subchannel for sector {lba}");
                        updateStatus?.Invoke($"Could not get correct subchannel for sector {lba}");

                        if(sense)
                        {
                            lba++;
                            forward = true;

                            continue;
                        }
                    }

                    if(subBuf.All(b => b == 0))
                    {
                        inexactPositioning = true;

                        DicConsole.DebugWriteLine("Pregap calculator", "All Q empty for LBA {0}", lba);

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

                            if(lba == (int)previousTrack.TrackStartSector)
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
                    if(subBuf[1] < track.TrackSequence)
                    {
                        lba++;
                        forward = true;

                        // Already gone back, so go forward
                        if(goneBack)
                            goFront = true;

                        continue;
                    }

                    // Same track, but not pregap
                    if(subBuf[1] == track.TrackSequence &&
                       subBuf[2] > 0)
                    {
                        lba--;
                        forward = false;

                        continue;
                    }

                    // Pregap according to Q position
                    int pregapQ = (subBuf[3] * 60 * 75) + (subBuf[4] * 75) + subBuf[5] + 1;
                    posQ = ((subBuf[7] * 60 * 75) + (subBuf[8] * 75)                   + subBuf[9]) - 150;
                    int diff = posQ                                                    - lba;

                    if(diff != 0)
                    {
                        DicConsole.DebugWriteLine("Pregap calculator", "Invalid Q position for LBA {0}, got {1}", lba,
                                                  posQ);

                        inexactPositioning = true;
                    }

                    // Bigger than known change, otherwise we found it
                    if(pregapQ > pregaps[track.TrackSequence])
                    {
                        // If CRC is not OK, only accept pregaps less than 10 sectors longer than previously now
                        if((crcOk || pregapQ - pregaps[track.TrackSequence] < 10))
                        {
                            DicConsole.DebugWriteLine("Pregap calculator", "Pregap for track {0}: {1}",
                                                      track.TrackSequence, pregapQ);

                            pregaps[track.TrackSequence] = pregapQ;
                        }
                        // We are going forward, so we have already been in the previous track, so add 1 to pregap and get out of here
                        else if(forward)
                        {
                            pregaps[track.TrackSequence]++;

                            break;
                        }
                    }
                    else if(pregapQ == pregaps[track.TrackSequence])
                        break;

                    lba--;
                    forward = false;
                }
            }

            for(int i = 0; i < tracks.Length; i++)
            {
                tracks[i].TrackPregap      =  (ulong)pregaps[tracks[i].TrackSequence];
                tracks[i].TrackStartSector -= tracks[i].TrackPregap;

            #if DEBUG
                dumpLog?.WriteLine($"Track {tracks[i].TrackSequence} pregap is {tracks[i].TrackPregap} sectors");
                updateStatus?.Invoke($"Track {tracks[i].TrackSequence} pregap is {tracks[i].TrackPregap} sectors");
            #endif
            }
        }

        static bool GetSectorForPregapRaw(Device dev, uint lba, Database.Models.Device dbDev, out byte[] subBuf)
        {
            byte[] cmdBuf;
            bool   sense;
            subBuf = null;

            sense = dev.ReadCd(out cmdBuf, out _, lba, 2448, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Raw,
                               dev.Timeout, out _);

            if(sense)
                sense = dev.ReadCd(out cmdBuf, out _, lba, 2448, 1, MmcSectorTypes.Cdda, false, false, false,
                                   MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.Raw, dev.Timeout,
                                   out _);

            if(!sense)
            {
                byte[] tmpBuf = new byte[96];
                Array.Copy(cmdBuf, 2352, tmpBuf, 0, 96);
                subBuf = DeinterleaveQ(tmpBuf);
            }
            else
            {
                sense = dev.ReadCd(out cmdBuf, out _, lba, 96, 1, MmcSectorTypes.AllTypes, false, false, false,
                                   MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Raw,
                                   dev.Timeout, out _);

                if(sense)
                    sense = dev.ReadCd(out cmdBuf, out _, lba, 96, 1, MmcSectorTypes.Cdda, false, false, false,
                                       MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Raw,
                                       dev.Timeout, out _);

                if(!sense)
                {
                    subBuf = DeinterleaveQ(cmdBuf);
                }
                else if(dbDev?.ATAPI?.RemovableMedias?.Any(d => d.SupportsPlextorReadCDDA == true) == true ||
                        dbDev?.SCSI?.RemovableMedias?.Any(d => d.SupportsPlextorReadCDDA  == true) == true ||
                        dev.Manufacturer.ToLowerInvariant()                                        == "plextor")
                    sense = dev.PlextorReadCdDa(out cmdBuf, out _, lba, 2448, 1, PlextorSubchannel.All, dev.Timeout,
                                                out _);

                {
                    if(!sense)
                    {
                        byte[] tmpBuf = new byte[96];
                        Array.Copy(cmdBuf, 0, tmpBuf, 0, 96);
                        subBuf = DeinterleaveQ(tmpBuf);
                    }
                }
            }

            return sense;
        }

        static bool GetSectorForPregapQ16(Device dev, uint lba, Database.Models.Device dbDev, out byte[] subBuf)
        {
            byte[] cmdBuf;
            bool   sense;
            subBuf = null;

            sense = dev.ReadCd(out cmdBuf, out _, lba, 2368, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Q16,
                               dev.Timeout, out _);

            if(sense)
                sense = dev.ReadCd(out cmdBuf, out _, lba, 2368, 1, MmcSectorTypes.Cdda, false, false, false,
                                   MmcHeaderCodes.None, true, false, MmcErrorField.None, MmcSubchannel.Q16, dev.Timeout,
                                   out _);

            if(!sense)
            {
                subBuf = new byte[16];
                Array.Copy(cmdBuf, 2352, subBuf, 0, 16);
            }
            else
            {
                sense = dev.ReadCd(out cmdBuf, out _, lba, 16, 1, MmcSectorTypes.AllTypes, false, false, false,
                                   MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Q16,
                                   dev.Timeout, out _);

                if(sense)
                    sense = dev.ReadCd(out cmdBuf, out _, lba, 16, 1, MmcSectorTypes.Cdda, false, false, false,
                                       MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Q16,
                                       dev.Timeout, out _);

                if(!sense)
                    subBuf = cmdBuf;
            }

            return sense;
        }

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
            {
                deQ[iq] = (byte)q[iq];
            }

            return deQ;
        }

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

        static void BcdToBinaryQ(byte[] q)
        {
            q[1] = (byte)(((q[1] / 16) * 10) + (q[1] & 0x0F));
            q[2] = (byte)(((q[2] / 16) * 10) + (q[2] & 0x0F));
            q[3] = (byte)(((q[3] / 16) * 10) + (q[3] & 0x0F));
            q[4] = (byte)(((q[4] / 16) * 10) + (q[4] & 0x0F));
            q[5] = (byte)(((q[5] / 16) * 10) + (q[5] & 0x0F));
            q[6] = (byte)(((q[6] / 16) * 10) + (q[6] & 0x0F));
            q[7] = (byte)(((q[7] / 16) * 10) + (q[7] & 0x0F));
            q[8] = (byte)(((q[8] / 16) * 10) + (q[8] & 0x0F));
            q[9] = (byte)(((q[9] / 16) * 10) + (q[9] & 0x0F));
        }
    }
}