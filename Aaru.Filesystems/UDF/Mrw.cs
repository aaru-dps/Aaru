// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mrw.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Universal Disk Format plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

public sealed partial class UDF
{
    // Mount Rainier (MRW) discs closed in non-compatible mode keep the physical layout on media:
    // a header area (GAA, DTAs and the compatibility bridge ISO9660) followed by the user data
    // area where every 136 data packets (of 32 sectors) are followed by 8 spare packets.
    const ulong MRW_HEADER_SECTORS = 1280;
    const ulong MRW_DATA_SECTORS   = 4352;
    const ulong MRW_SPARE_SECTORS  = 256;
    const ulong MRW_PACKET_SECTORS = MRW_DATA_SECTORS + MRW_SPARE_SECTORS;

    /// <summary>Translates a user data area LBA to the physical sector on a raw (non-compatible) MRW medium.</summary>
    static ulong MrwTranslate(ulong lba) => MRW_HEADER_SECTORS + lba + MRW_SPARE_SECTORS * (lba / MRW_DATA_SECTORS);

    /// <summary>Calculates how many user data sectors fit in a raw (non-compatible) MRW medium.</summary>
    static ulong MrwUserSectors(ulong physicalSectors)
    {
        if(physicalSectors <= MRW_HEADER_SECTORS) return 0;

        ulong dataArea    = physicalSectors - MRW_HEADER_SECTORS;
        ulong fullPackets = dataArea / MRW_PACKET_SECTORS;

        return fullPackets * MRW_DATA_SECTORS + Math.Min(dataArea - fullPackets * MRW_PACKET_SECTORS, MRW_DATA_SECTORS);
    }

    /// <summary>Checks whether the image is big enough to contain a raw (non-compatible) MRW layout.</summary>
    static bool MrwPossible(IMediaImage imagePlugin) =>
        imagePlugin.Info.SectorSize is 2048 or 2352 && imagePlugin.Info.Sectors > MRW_HEADER_SECTORS + 32;

    /// <summary>Reads a single user data area sector, translating the address when the medium is raw MRW.</summary>
    static ErrorNumber ReadMrwAwareSector(IMediaImage imagePlugin, bool mrw, ulong sectorAddress, out byte[] buffer) =>
        imagePlugin.ReadSector(mrw ? MrwTranslate(sectorAddress) : sectorAddress, false, out buffer, out _);

    /// <summary>
    ///     Reads several user data area sectors, translating addresses and splitting reads at spare area
    ///     boundaries when the medium is raw MRW.
    /// </summary>
    ErrorNumber ReadMrwAwareSectors(ulong sectorAddress, uint count, out byte[] buffer)
    {
        if(!_mrw) return _imagePlugin.ReadSectors(sectorAddress, false, count, out buffer, out _);

        buffer = new byte[count * _sectorSize];
        var offset = 0;

        while(count > 0)
        {
            var chunk = (uint)Math.Min(count, MRW_DATA_SECTORS - sectorAddress % MRW_DATA_SECTORS);

            ErrorNumber errno = _imagePlugin.ReadSectors(MrwTranslate(sectorAddress),
                                                         false,
                                                         chunk,
                                                         out byte[] chunkBuffer,
                                                         out _);

            if(errno != ErrorNumber.NoError)
            {
                buffer = null;

                return errno;
            }

            Array.Copy(chunkBuffer, 0, buffer, offset, chunkBuffer.Length);
            offset        += chunkBuffer.Length;
            sectorAddress += chunk;
            count         -= chunk;
        }

        return ErrorNumber.NoError;
    }
}
