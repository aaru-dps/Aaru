// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Subchannel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles CompactDisc subchannel data.
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
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
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
        public static bool SupportsRwSubchannel(Device dev, DumpLog dumpLog, UpdateStatusHandler updateStatus)
        {
            dumpLog?.WriteLine("Checking if drive supports full raw subchannel reading...");
            updateStatus?.Invoke("Checking if drive supports full raw subchannel reading...");

            return !dev.ReadCd(out _, out _, 0, 2352 + 96, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Raw,
                               dev.Timeout, out _);
        }

        public static bool SupportsPqSubchannel(Device dev, DumpLog dumpLog, UpdateStatusHandler updateStatus)
        {
            dumpLog?.WriteLine("Checking if drive supports PQ subchannel reading...");
            updateStatus?.Invoke("Checking if drive supports PQ subchannel reading...");

            return !dev.ReadCd(out _, out _, 0, 2352 + 16, 1, MmcSectorTypes.AllTypes, false, false, true,
                               MmcHeaderCodes.AllHeaders, true, true, MmcErrorField.None, MmcSubchannel.Q16,
                               dev.Timeout, out _);
        }

        void WriteSubchannelToImage(MmcSubchannel supportedSubchannel, MmcSubchannel desiredSubchannel, byte[] sub,
                                    ulong sectorAddress, uint length, SubchannelLog subLog,
                                    Dictionary<byte, string> isrcs, byte currentTrack)
        {
            if(supportedSubchannel == MmcSubchannel.Q16)
                sub = Subchannel.ConvertQToRaw(sub);

            if(desiredSubchannel != MmcSubchannel.None)
                _outputPlugin.WriteSectorsTag(sub, sectorAddress, 1, SectorTagType.CdSectorSubchannel);

            subLog?.WriteEntry(sub, supportedSubchannel == MmcSubchannel.Raw, (long)sectorAddress, 1);

            byte[] deSub = Subchannel.Deinterleave(sub);

            // Check subchannel
            for(int subPos = 0; subPos < deSub.Length; subPos += 96)
            {
                // ISRC
                if((deSub[subPos + 12] & 0x3) == 3)
                {
                    byte[] q = new byte[12];
                    Array.Copy(deSub, subPos + 12, q, 0, 12);
                    string isrc = Subchannel.DecodeIsrc(q);

                    if(isrc == null ||
                       isrc == "000000000000")
                        continue;

                    if(!isrcs.ContainsKey(currentTrack))
                    {
                        _dumpLog?.WriteLine($"Found new ISRC {isrc} for track {currentTrack}.");
                        UpdateStatus?.Invoke($"Found new ISRC {isrc} for track {currentTrack}.");
                    }
                    else if(isrcs[currentTrack] != isrc)
                    {
                        CRC16CCITTContext.Data(q, 10, out byte[] crc);

                        if(crc[0] != q[10] ||
                           crc[1] != q[11])
                        {
                            continue;
                        }

                        _dumpLog?.
                            WriteLine($"ISRC for track {currentTrack} changed from {isrcs[currentTrack]} to {isrc}.");

                        UpdateStatus?.
                            Invoke($"ISRC for track {currentTrack} changed from {isrcs[currentTrack]} to {isrc}.");
                    }

                    isrcs[currentTrack] = isrc;
                }
            }
        }
    }
}