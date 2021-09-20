// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Verify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Verifies BlindWrite 5 disc images.
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
// ****************************************************************************/

using System;
using System.Collections.Generic;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;

namespace Aaru.DiscImages
{
    public sealed partial class BlindWrite5
    {
        /// <inheritdoc />
        public bool? VerifySector(ulong sectorAddress)
        {
            ErrorNumber errno = ReadSectorLong(sectorAddress, out byte[] buffer);

            return errno != ErrorNumber.NoError ? null : CdChecksums.CheckCdSector(buffer);
        }

        /// <inheritdoc />
        public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();
            ErrorNumber errno = ReadSectorsLong(sectorAddress, length, out byte[] buffer);

            if(errno != ErrorNumber.NoError)
                return null;

            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);

                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);

                        break;
                }
            }

            if(unknownLbas.Count > 0)
                return null;

            return failingLbas.Count <= 0;
        }

        /// <inheritdoc />
        public bool? VerifySectors(ulong sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                                   out List<ulong> unknownLbas)
        {
            byte[] buffer = ReadSectorsLong(sectorAddress, length, track);
            int    bps    = (int)(buffer.Length / length);
            byte[] sector = new byte[bps];
            failingLbas = new List<ulong>();
            unknownLbas = new List<ulong>();

            for(int i = 0; i < length; i++)
            {
                Array.Copy(buffer, i * bps, sector, 0, bps);
                bool? sectorStatus = CdChecksums.CheckCdSector(sector);

                switch(sectorStatus)
                {
                    case null:
                        unknownLbas.Add((ulong)i + sectorAddress);

                        break;
                    case false:
                        failingLbas.Add((ulong)i + sectorAddress);

                        break;
                }
            }

            if(unknownLbas.Count > 0)
                return null;

            return failingLbas.Count <= 0;
        }
    }
}