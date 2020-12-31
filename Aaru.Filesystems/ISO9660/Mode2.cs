// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mode2.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles reading sectors in MODE 0, 1 and 2.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.IO;
using Aaru.Console;
using Aaru.Decoders.CD;

namespace Aaru.Filesystems
{
    public sealed partial class ISO9660
    {
        byte[] ReadSector(ulong sector, bool interleaved = false, byte fileNumber = 0)
        {
            ulong realSector;
            uint  sectorCount;

            sectorCount = (uint)_blockSize / 2048;

            if(_blockSize % 2048 > 0)
                sectorCount++;

            realSector = (sector * _blockSize) / 2048;

            ulong offset = (sector * _blockSize) % 2048;

            byte[] data;

            if(sectorCount == 1)
            {
                // TODO: No more exceptions
                try
                {
                    data = _image.ReadSectorLong(realSector);
                }
                catch
                {
                    data = _image.ReadSector(realSector);
                }

                if(_debug)
                {
                    switch(data.Length)
                    {
                        case 2048:
                            AaruConsole.DebugWriteLine("ISO9660 Plugin", "Sector {0}, Cooked, Mode 0/1 / Mode 2 Form 1",
                                                       realSector);

                            break;
                        case 2324:
                            AaruConsole.DebugWriteLine("ISO9660 Plugin", "Sector {0}, Cooked, Mode 2 Form 2",
                                                       realSector);

                            break;
                        case 2336:
                            AaruConsole.DebugWriteLine("ISO9660 Plugin",
                                                       "Sector {0}, Cooked, Mode 2 Form {1}, File Number {2}, Channel Number {3}, Submode {4}, Coding Information {5}",
                                                       realSector,
                                                       ((Mode2Submode)data[2]).HasFlag(Mode2Submode.Form2) ? 2 : 1,
                                                       data[0], data[1], (Mode2Submode)data[2], data[3]);

                            break;
                        case 2352 when data[0] != 0x00 || data[1] != 0xFF || data[2]  != 0xFF || data[3]  != 0xFF ||
                                       data[4] != 0xFF || data[5] != 0xFF || data[6]  != 0xFF || data[7]  != 0xFF ||
                                       data[8] != 0xFF || data[9] != 0xFF || data[10] != 0xFF || data[11] != 0x00:
                            AaruConsole.DebugWriteLine("ISO9660 Plugin", "Sector {0}, Raw, Audio", realSector);

                            break;
                        case 2352 when data[15] != 2:
                            AaruConsole.DebugWriteLine("ISO9660 Plugin",
                                                       "Sector {0} ({1:X2}:{2:X2}:{3:X2}), Raw, Mode {4}", realSector,
                                                       data[12], data[13], data[14], data[15]);

                            break;
                        case 2352:
                            AaruConsole.DebugWriteLine("ISO9660 Plugin",
                                                       "Sector {0} ({1:X2}:{2:X2}:{3:X2}), Raw, Mode 2 Form {4}, File Number {5}, Channel Number {6}, Submode {7}, Coding Information {8}",
                                                       realSector, data[12], data[13], data[14],
                                                       ((Mode2Submode)data[18]).HasFlag(Mode2Submode.Form2) ? 2 : 1,
                                                       data[16], data[17], (Mode2Submode)data[18], data[19]);

                            break;
                    }
                }

                if(_blockSize == 2048)
                    return Sector.GetUserData(data, interleaved, fileNumber);

                byte[] tmp = new byte[_blockSize];
                Array.Copy(Sector.GetUserData(data, interleaved, fileNumber), (int)offset, tmp, 0, _blockSize);

                return tmp;
            }
            else
            {
                var ms = new MemoryStream();

                for(uint i = 0; i < sectorCount; i++)
                {
                    ulong dstSector = realSector + 1;

                    // TODO: No more exceptions
                    try
                    {
                        data = _image.ReadSectorLong(dstSector);
                    }
                    catch
                    {
                        data = _image.ReadSector(dstSector);
                    }

                    if(_debug)
                    {
                        switch(data.Length)
                        {
                            case 2048:
                                AaruConsole.DebugWriteLine("ISO9660 Plugin",
                                                           "Sector {0}, Cooked, Mode 0/1 / Mode 2 Form 1", dstSector);

                                break;
                            case 2324:
                                AaruConsole.DebugWriteLine("ISO9660 Plugin", "Sector {0}, Cooked, Mode 2 Form 2",
                                                           dstSector);

                                break;
                            case 2336:
                                AaruConsole.DebugWriteLine("ISO9660 Plugin",
                                                           "Sector {0}, Cooked, Mode 2 Form {1}, File Number {2}, Channel Number {3}, Submode {4}, Coding Information {5}",
                                                           dstSector,
                                                           ((Mode2Submode)data[2]).HasFlag(Mode2Submode.Form2) ? 2 : 1,
                                                           data[0], data[1], (Mode2Submode)data[2], data[3]);

                                break;
                            case 2352 when data[0] != 0x00 || data[1] != 0xFF || data[2]  != 0xFF || data[3]  != 0xFF ||
                                           data[4] != 0xFF || data[5] != 0xFF || data[6]  != 0xFF || data[7]  != 0xFF ||
                                           data[8] != 0xFF || data[9] != 0xFF || data[10] != 0xFF || data[11] != 0x00:
                                AaruConsole.DebugWriteLine("ISO9660 Plugin", "Sector {0}, Raw, Audio", dstSector);

                                break;
                            case 2352 when data[15] != 2:
                                AaruConsole.DebugWriteLine("ISO9660 Plugin",
                                                           "Sector {0} ({1:X2}:{2:X2}:{3:X2}), Raw, Mode {4}",
                                                           dstSector, data[12], data[13], data[14], data[15]);

                                break;
                            case 2352:
                                AaruConsole.DebugWriteLine("ISO9660 Plugin",
                                                           "Sector {0} ({1:X2}:{2:X2}:{3:X2}), Raw, Mode 2 Form {4}, File Number {5}, Channel Number {6}, Submode {7}, Coding Information {8}",
                                                           dstSector, data[12], data[13], data[14],
                                                           ((Mode2Submode)data[18]).HasFlag(Mode2Submode.Form2) ? 2 : 1,
                                                           data[16], data[17], (Mode2Submode)data[18], data[19]);

                                break;
                        }
                    }

                    byte[] sectorData = Sector.GetUserData(data, interleaved, fileNumber);

                    ms.Write(sectorData, 0, sectorData.Length);
                }

                byte[] tmp = new byte[_blockSize];
                Array.Copy(Sector.GetUserData(ms.ToArray(), interleaved, fileNumber), 0, tmp, 0, _blockSize);

                return tmp;
            }
        }
    }
}