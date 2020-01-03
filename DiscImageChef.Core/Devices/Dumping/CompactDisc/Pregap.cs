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
using DiscImageChef.CommonTypes.Enums;
using DiscImageChef.CommonTypes.Structs;
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
                                             Track[] tracks, bool supportsPqSubchannel, bool supportsRwSubchannel)
        {
            bool   sense;  // Sense indicator
            byte[] cmdBuf; // Data buffer

            if(!supportsPqSubchannel &&
               !supportsRwSubchannel)
                return;

            for(int i = 1; i < tracks.Length; i++)
            {
                uint lba           = (uint)tracks[i].TrackStartSector - 1;
                int  trackPregap   = 0;
                bool previousSense = false;

                // Check if pregap is 0
                if(supportsPqSubchannel)
                    sense = dev.ReadCd(out cmdBuf, out _, lba, 16, 1, MmcSectorTypes.AllTypes, false, false, false,
                                       MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Q16,
                                       dev.Timeout, out _);
                else
                {
                    sense = dev.ReadCd(out cmdBuf, out _, lba, 96, 1, MmcSectorTypes.AllTypes, false, false, false,
                                       MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Raw,
                                       dev.Timeout, out _);

                    if(!sense)
                        cmdBuf = DeinterleaveQ(cmdBuf);
                }

                if(!sense)
                {
                    // Q position
                    if((cmdBuf[0] & 0xF) == 1)
                    {
                        // Check if BCD or binary values, change to binary
                        int posQ = ((cmdBuf[7] * 60 * 75) + (cmdBuf[8] * 75) + cmdBuf[9]) - 150;

                        if(posQ > lba)
                        {
                            BcdToBinaryQ(cmdBuf);

                            posQ = ((cmdBuf[7] * 60 * 75) + (cmdBuf[8] * 75) + cmdBuf[9]) - 150;
                        }

                        if(cmdBuf[1] == tracks[i - 1].TrackSequence &&
                           cmdBuf[2] > 0                            &&
                           posQ      == lba)
                        {
                            trackPregap = 0;

                        #if DEBUG
                            dumpLog?.WriteLine($"Track {tracks[i].TrackSequence} pregap is {trackPregap} sectors");
                            updateStatus?.Invoke($"Track {tracks[i].TrackSequence} pregap is {trackPregap} sectors");
                        #endif

                            tracks[i].TrackPregap      =  (ulong)trackPregap;
                            tracks[i].TrackStartSector -= tracks[i].TrackPregap;

                            continue;
                        }
                    }
                }

                // Calculate pregap
                lba = (uint)tracks[i].TrackStartSector - 150;

                while(lba > tracks[i - 1].TrackStartSector)
                {
                    if(supportsPqSubchannel)
                        sense = dev.ReadCd(out cmdBuf, out _, lba, 16, 1, MmcSectorTypes.AllTypes, false, false, false,
                                           MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Q16,
                                           dev.Timeout, out _);
                    else
                    {
                        sense = dev.ReadCd(out cmdBuf, out _, lba, 96, 1, MmcSectorTypes.AllTypes, false, false, false,
                                           MmcHeaderCodes.None, false, false, MmcErrorField.None, MmcSubchannel.Raw,
                                           dev.Timeout, out _);

                        if(!sense)
                            cmdBuf = DeinterleaveQ(cmdBuf);
                    }

                    if(!sense)
                    {
                        // Q position
                        if((cmdBuf[0] & 0xF) != 1)
                        {
                            lba--;

                            continue;
                        }

                        // Check if BCD or binary values, change to binary
                        int posQ = ((cmdBuf[7] * 60 * 75) + (cmdBuf[8] * 75) + cmdBuf[9]) - 150;

                        if(posQ > lba)
                        {
                            BcdToBinaryQ(cmdBuf);

                            posQ = ((cmdBuf[7] * 60 * 75) + (cmdBuf[8] * 75) + cmdBuf[9]) - 150;
                        }

                        if(cmdBuf[1] != tracks[i].TrackSequence ||
                           cmdBuf[2] != 0)
                        {
                            lba++;
                            trackPregap = (int)(tracks[i].TrackStartSector - lba);

                            if(previousSense)
                                break;

                            continue;
                        }

                        int pregapQ = posQ > lba ? trackPregap : (cmdBuf[3] * 60 * 75) + (cmdBuf[4] * 75) + cmdBuf[5];

                        if(pregapQ > trackPregap)
                            trackPregap = pregapQ;
                        else
                        {
                            if(posQ == lba + 1)
                                trackPregap++;

                            break;
                        }

                        lba--;
                    }
                    else
                    {
                        previousSense = true;
                        lba--;
                    }
                }

            #if DEBUG
                dumpLog?.WriteLine($"Track {tracks[i].TrackSequence} pregap is {trackPregap} sectors");
                updateStatus?.Invoke($"Track {tracks[i].TrackSequence} pregap is {trackPregap} sectors");
            #endif

                tracks[i].TrackPregap      =  (ulong)trackPregap;
                tracks[i].TrackStartSector -= tracks[i].TrackPregap;
            }
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

            for(int iq = 0; iq < subchannel.Length; iq++)
            {
                deQ[iq] = (byte)q[iq];
            }

            return deQ;
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