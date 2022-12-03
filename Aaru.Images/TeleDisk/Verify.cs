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
//     Verifies Sydex TeleDisk disk images.
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
// ****************************************************************************/

using System.Collections.Generic;

namespace Aaru.DiscImages;

public sealed partial class TeleDisk
{
    /// <inheritdoc />
    public bool? VerifyMediaImage() => _aDiskCrcHasFailed;

    /// <inheritdoc />
    public bool? VerifySector(ulong sectorAddress) => !_sectorsWhereCrcHasFailed.Contains(sectorAddress);

    /// <inheritdoc />
    public bool? VerifySectors(ulong sectorAddress, uint length, out List<ulong> failingLbas,
                               out List<ulong> unknownLbas)
    {
        failingLbas = new List<ulong>();
        unknownLbas = new List<ulong>();

        for(ulong i = sectorAddress; i < sectorAddress + length; i++)
            if(_sectorsWhereCrcHasFailed.Contains(sectorAddress))
                failingLbas.Add(sectorAddress);

        return failingLbas.Count <= 0;
    }
}