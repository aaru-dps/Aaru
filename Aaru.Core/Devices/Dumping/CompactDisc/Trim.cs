// /***************************************************************************
// Aaru Data Preservation Suite
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Extents;
using Aaru.Devices;
using Schemas;

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

namespace Aaru.Core.Devices.Dumping
{
    partial class Dump
    {
        void TrimCdUserData(ExtentsULong audioExtents, uint blockSize, DumpHardwareType currentTry,
                            ExtentsULong extents, bool newTrim, int offsetBytes, bool read6, bool read10, bool read12,
                            bool read16, bool readcd, int sectorsForOffset, uint subSize,
                            MmcSubchannel supportedSubchannel, bool supportsLongSectors, ref double totalDuration)
        {
            DateTime          start;
            DateTime          end;
            bool              sense       = true; // Sense indicator
            byte[]            cmdBuf      = null; // Data buffer
            double            cmdDuration = 0;    // Command execution time
            const uint        sectorSize  = 2352; // Full sector size
            byte[]            tmpBuf;             // Temporary buffer
            PlextorSubchannel supportedPlextorSubchannel;

            switch(supportedSubchannel)
            {
                case MmcSubchannel.None:
                    supportedPlextorSubchannel = PlextorSubchannel.None;

                    break;
                case MmcSubchannel.Raw:
                    supportedPlextorSubchannel = PlextorSubchannel.All;

                    break;
                case MmcSubchannel.Q16:
                    supportedPlextorSubchannel = PlextorSubchannel.Q16;

                    break;
                case MmcSubchannel.Rw:
                    supportedPlextorSubchannel = PlextorSubchannel.Pack;

                    break;
                default:
                    supportedPlextorSubchannel = PlextorSubchannel.None;

                    break;
            }

            if(_resume.BadBlocks.Count <= 0 ||
               _aborted                     ||
               !_trim                       ||
               !newTrim)
                return;

            start = DateTime.UtcNow;
            UpdateStatus?.Invoke("Trimming bad sectors");
            _dumpLog.WriteLine("Trimming bad sectors");

            ulong[] tmpArray = _resume.BadBlocks.ToArray();
            InitProgress?.Invoke();

            foreach(ulong badSector in tmpArray)
            {
                if(_aborted)
                {
                    currentTry.Extents = ExtentsConverter.ToMetadata(extents);
                    UpdateStatus?.Invoke("Aborted!");
                    _dumpLog.WriteLine("Aborted!");

                    break;
                }

                PulseProgress?.Invoke($"Trimming sector {badSector}");

                byte sectorsToTrim   = 1;
                uint badSectorToRead = (uint)badSector;

                if(_fixOffset                       &&
                   audioExtents.Contains(badSector) &&
                   offsetBytes != 0)
                {
                    if(offsetBytes > 0)
                    {
                        badSectorToRead -= (uint)sectorsForOffset;
                    }

                    sectorsToTrim = (byte)(sectorsForOffset + 1);
                }

                if(_supportsPlextorD8 && audioExtents.Contains(badSector))
                {
                    sense = _dev.PlextorReadCdDa(out cmdBuf, out _, badSectorToRead, blockSize, sectorsToTrim,
                                                 supportedPlextorSubchannel, 0, out cmdDuration);

                    if(sense)
                    {
                        // As a workaround for some firmware bugs, seek far away.
                        _dev.PlextorReadCdDa(out _, out _, badSectorToRead - 32, blockSize, sectorsToTrim,
                                             supportedPlextorSubchannel, 0, out _);

                        sense = _dev.PlextorReadCdDa(out cmdBuf, out _, badSectorToRead, blockSize, sectorsToTrim,
                                                     supportedPlextorSubchannel, _dev.Timeout, out cmdDuration);
                    }

                    totalDuration += cmdDuration;
                }
                else if(readcd)
                    sense = _dev.ReadCd(out cmdBuf, out _, badSectorToRead, blockSize, sectorsToTrim,
                                        MmcSectorTypes.AllTypes, false, false, true, MmcHeaderCodes.AllHeaders, true,
                                        true, MmcErrorField.None, supportedSubchannel, _dev.Timeout, out cmdDuration);
                else if(read16)
                    sense = _dev.Read16(out cmdBuf, out _, 0, false, true, false, badSectorToRead, blockSize, 0,
                                        sectorsToTrim, false, _dev.Timeout, out cmdDuration);
                else if(read12)
                    sense = _dev.Read12(out cmdBuf, out _, 0, false, true, false, false, badSectorToRead, blockSize, 0,
                                        sectorsToTrim, false, _dev.Timeout, out cmdDuration);
                else if(read10)
                    sense = _dev.Read10(out cmdBuf, out _, 0, false, true, false, false, badSectorToRead, blockSize, 0,
                                        sectorsToTrim, _dev.Timeout, out cmdDuration);
                else if(read6)
                    sense = _dev.Read6(out cmdBuf, out _, badSectorToRead, blockSize, sectorsToTrim, _dev.Timeout,
                                       out cmdDuration);

                totalDuration += cmdDuration;

                if(sense || _dev.Error)
                    continue;

                if(!sense &&
                   !_dev.Error)
                {
                    _resume.BadBlocks.Remove(badSector);
                    extents.Add(badSector);
                }

                // Because one block has been partially used to fix the offset
                if(_fixOffset                       &&
                   audioExtents.Contains(badSector) &&
                   offsetBytes != 0)
                {
                    int offsetFix = offsetBytes < 0 ? (int)(sectorSize - (offsetBytes * -1)) : offsetBytes;

                    if(supportedSubchannel != MmcSubchannel.None)
                    {
                        // De-interleave subchannel
                        byte[] data = new byte[sectorSize * sectorsToTrim];
                        byte[] sub  = new byte[subSize    * sectorsToTrim];

                        for(int b = 0; b < sectorsToTrim; b++)
                        {
                            Array.Copy(cmdBuf, (int)(0 + (b * blockSize)), data, sectorSize * b, sectorSize);

                            Array.Copy(cmdBuf, (int)(sectorSize + (b * blockSize)), sub, subSize * b, subSize);
                        }

                        tmpBuf = new byte[sectorSize * (sectorsToTrim - sectorsForOffset)];
                        Array.Copy(data, offsetFix, tmpBuf, 0, tmpBuf.Length);
                        data = tmpBuf;

                        // Re-interleave subchannel
                        cmdBuf = new byte[blockSize * sectorsToTrim];

                        for(int b = 0; b < sectorsToTrim; b++)
                        {
                            Array.Copy(data, sectorSize * b, cmdBuf, (int)(0 + (b * blockSize)), sectorSize);

                            Array.Copy(sub, subSize * b, cmdBuf, (int)(sectorSize + (b * blockSize)), subSize);
                        }
                    }
                    else
                    {
                        tmpBuf = new byte[blockSize * (sectorsToTrim - sectorsForOffset)];
                        Array.Copy(cmdBuf, offsetFix, tmpBuf, 0, tmpBuf.Length);
                        cmdBuf = tmpBuf;
                    }
                }

                if(supportedSubchannel != MmcSubchannel.None)
                {
                    byte[] data = new byte[sectorSize];
                    byte[] sub  = new byte[subSize];
                    Array.Copy(cmdBuf, 0, data, 0, sectorSize);
                    Array.Copy(cmdBuf, sectorSize, sub, 0, subSize);
                    _outputPlugin.WriteSectorLong(data, badSector);
                    _outputPlugin.WriteSectorTag(sub, badSector, SectorTagType.CdSectorSubchannel);
                }
                else
                {
                    if(supportsLongSectors)
                    {
                        _outputPlugin.WriteSectorLong(cmdBuf, badSector);
                    }
                    else
                    {
                        if(cmdBuf.Length % sectorSize == 0)
                        {
                            byte[] data = new byte[2048];
                            Array.Copy(cmdBuf, 16, data, 0, 2048);

                            _outputPlugin.WriteSector(data, badSector);
                        }
                        else
                        {
                            _outputPlugin.WriteSectorLong(cmdBuf, badSector);
                        }
                    }
                }
            }

            EndProgress?.Invoke();
            end = DateTime.UtcNow;
            UpdateStatus?.Invoke($"Trimming finished in {(end - start).TotalSeconds} seconds.");
            _dumpLog.WriteLine("Trimming finished in {0} seconds.", (end - start).TotalSeconds);
        }
    }
}