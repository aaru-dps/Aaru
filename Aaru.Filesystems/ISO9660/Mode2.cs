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
// Copyright © 2011-2023 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Decoders.CD;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    ErrorNumber ReadSector(ulong sector, out byte[] buffer, bool interleaved = false, byte fileNumber = 0)
    {
        ErrorNumber errno;
        buffer = null;

        uint sectorCount = (uint)_blockSize / 2048;

        if(_blockSize % 2048 > 0)
            sectorCount++;

        ulong realSector = sector * _blockSize / 2048;

        ulong offset = sector * _blockSize % 2048;

        byte[] data;

        if(sectorCount == 1)
        {
            errno = _image.ReadSectorLong(realSector, out data);

            if(errno != ErrorNumber.NoError)
                errno = _image.ReadSector(realSector, out data);

            if(errno != ErrorNumber.NoError)
                return errno;

            if(_debug)
            {
                switch(data.Length)
                {
                    case 2048:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.tor_Sector_0_Cooked_Mode_zero_one_Mode_two_Form_one,
                                                   realSector);

                        break;
                    case 2324:
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.tor_Sector_0_Cooked_Mode_two_Form_two,
                                                   realSector);

                        break;
                    case 2336:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.
                                                       tor_Sector_0_Cooked_Mode_two_Form_1_File_Number_2_Channel_Number_3_Submode_4_Coding_Information_5,
                                                   realSector,
                                                   ((Mode2Submode)data[2]).HasFlag(Mode2Submode.Form2) ? 2 : 1, data[0],
                                                   data[1], (Mode2Submode)data[2], data[3]);

                        break;
                    case 2352 when data[0]  != 0x00 ||
                                   data[1]  != 0xFF ||
                                   data[2]  != 0xFF ||
                                   data[3]  != 0xFF ||
                                   data[4]  != 0xFF ||
                                   data[5]  != 0xFF ||
                                   data[6]  != 0xFF ||
                                   data[7]  != 0xFF ||
                                   data[8]  != 0xFF ||
                                   data[9]  != 0xFF ||
                                   data[10] != 0xFF ||
                                   data[11] != 0x00:
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.tor_Sector_0_Raw_Audio, realSector);

                        break;
                    case 2352 when data[15] != 2:
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.tor_Sector_0_1_2_3_Raw_Mode_4, realSector,
                                                   data[12], data[13], data[14], data[15]);

                        break;
                    case 2352:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.
                                                       tor_Sector_0_1_2_3_Raw_Mode_two_Form_4_File_Number_5_Channel_Number_6_Submode_7_Coding_Information_8,
                                                   realSector, data[12], data[13], data[14],
                                                   ((Mode2Submode)data[18]).HasFlag(Mode2Submode.Form2) ? 2 : 1,
                                                   data[16], data[17], (Mode2Submode)data[18], data[19]);

                        break;
                }
            }

            if(_blockSize == 2048)
            {
                buffer = Sector.GetUserData(data, interleaved, fileNumber);

                return ErrorNumber.NoError;
            }

            var tmp = new byte[_blockSize];
            Array.Copy(Sector.GetUserData(data, interleaved, fileNumber), (int)offset, tmp, 0, _blockSize);

            buffer = tmp;

            return errno;
        }
        else
        {
            var ms = new MemoryStream();

            for(uint i = 0; i < sectorCount; i++)
            {
                ulong dstSector = realSector + 1;

                errno = _image.ReadSectorLong(dstSector, out data);

                if(errno != ErrorNumber.NoError)
                    errno = _image.ReadSector(dstSector, out data);

                if(errno != ErrorNumber.NoError)
                    return errno;

                if(_debug)
                {
                    switch(data.Length)
                    {
                        case 2048:
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization.tor_Sector_0_Cooked_Mode_zero_one_Mode_two_Form_one,
                                                       dstSector);

                            break;
                        case 2324:
                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.tor_Sector_0_Cooked_Mode_two_Form_two,
                                                       dstSector);

                            break;
                        case 2336:
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization.
                                                           tor_Sector_0_Cooked_Mode_two_Form_1_File_Number_2_Channel_Number_3_Submode_4_Coding_Information_5,
                                                       dstSector,
                                                       ((Mode2Submode)data[2]).HasFlag(Mode2Submode.Form2) ? 2 : 1,
                                                       data[0], data[1], (Mode2Submode)data[2], data[3]);

                            break;
                        case 2352 when data[0]  != 0x00 ||
                                       data[1]  != 0xFF ||
                                       data[2]  != 0xFF ||
                                       data[3]  != 0xFF ||
                                       data[4]  != 0xFF ||
                                       data[5]  != 0xFF ||
                                       data[6]  != 0xFF ||
                                       data[7]  != 0xFF ||
                                       data[8]  != 0xFF ||
                                       data[9]  != 0xFF ||
                                       data[10] != 0xFF ||
                                       data[11] != 0x00:
                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.tor_Sector_0_Raw_Audio, dstSector);

                            break;
                        case 2352 when data[15] != 2:
                            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.tor_Sector_0_1_2_3_Raw_Mode_4,
                                                       dstSector, data[12], data[13], data[14], data[15]);

                            break;
                        case 2352:
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization.
                                                           tor_Sector_0_1_2_3_Raw_Mode_two_Form_4_File_Number_5_Channel_Number_6_Submode_7_Coding_Information_8,
                                                       dstSector, data[12], data[13], data[14],
                                                       ((Mode2Submode)data[18]).HasFlag(Mode2Submode.Form2) ? 2 : 1,
                                                       data[16], data[17], (Mode2Submode)data[18], data[19]);

                            break;
                    }
                }

                byte[] sectorData = Sector.GetUserData(data, interleaved, fileNumber);

                ms.Write(sectorData, 0, sectorData.Length);
            }

            var tmp = new byte[_blockSize];
            Array.Copy(Sector.GetUserData(ms.ToArray(), interleaved, fileNumber), 0, tmp, 0, _blockSize);
            buffer = tmp;

            return ErrorNumber.NoError;
        }
    }
}