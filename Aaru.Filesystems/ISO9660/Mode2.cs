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
// Copyright © 2011-2020 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System.IO;

namespace Aaru.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        byte[] ReadSectors(ulong start, uint count)
        {
            MemoryStream ms = new MemoryStream();

            for(ulong sector = start; sector < start + count; sector++)
            {
                byte[] data = image.ReadSector(sector);

                switch(data.Length)
                {
                    case 2048:
                        // Mode 1 sector
                        ms.Write(data, 0, 2048);
                        break;
                    case 2352:
                        // Not a data sector
                        if(data[0] != 0    || data[1] != 0xFF || data[2] != 0xFF || data[3] != 0xFF ||
                           data[4] != 0xFF ||
                           data[5] != 0xFF || data[6]  != 0xFF || data[7]  != 0xFF || data[8] != 0xFF ||
                           data[9] != 0xFF || data[10] != 0xFF || data[11] != 0x00)
                        {
                            ms.Write(data, 0, 2352);
                            break;
                        }

                        switch(data[15])
                        {
                            // Mode 0 sector
                            case 0:
                                ms.Write(new byte[2048], 0, 2048);
                                break;
                            // Mode 1 sector
                            case 1:
                                ms.Write(data, 16, 2048);
                                break;
                            case 2:
                                // Mode 2 form 1 sector
                                if((data[16] & MODE2_FORM2) != 0)
                                {
                                    ms.Write(data, 24, 2048);
                                    break;
                                }

                                // Mode 2 form 2 sector
                                ms.Write(data, 24, 2324);
                                break;
                            // Unknown, audio?
                            default:
                                ms.Write(data, 0, 2352);
                                break;
                        }

                        break;
                    case 2336:
                        // Mode 2 form 1 sector
                        if((data[16] & MODE2_FORM2) == 0)
                        {
                            ms.Write(data, 8, 2048);
                            break;
                        }

                        // Mode 2 form 2 sector
                        ms.Write(data, 8, 2324);
                        break;
                    // Should not happen, but, just in case
                    default:
                        ms.Write(data, 0, data.Length);
                        break;
                }
            }

            return ms.ToArray();
        }
    }
}