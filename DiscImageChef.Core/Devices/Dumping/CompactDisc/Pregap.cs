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
using DiscImageChef.CommonTypes.Enums;
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
    }
}