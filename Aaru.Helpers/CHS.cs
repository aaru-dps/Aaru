// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CHS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Helpers for CHS<->LBA conversions
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

namespace Aaru.Helpers;

/// <summary>Helper operations to work with CHS values</summary>
public static class CHS
{
    /// <summary>Converts a CHS position to a LBA one</summary>
    /// <param name="cyl">Cylinder</param>
    /// <param name="head">Head</param>
    /// <param name="sector">Sector</param>
    /// <param name="maxHead">Number of heads</param>
    /// <param name="maxSector">Number of sectors per track</param>
    /// <returns></returns>
    public static uint ToLBA(uint cyl, uint head, uint sector, uint maxHead, uint maxSector)
    {
        // CHS sector numbers are 1-based; treat a (corrupt) sector 0 as sector 1 instead of underflowing
        uint sectorOffset = sector == 0 ? 0 : sector - 1;

        return maxHead == 0 || maxSector == 0
                   ? (cyl * 16      + head) * 63        + sectorOffset
                   : (cyl * maxHead + head) * maxSector + sectorOffset;
    }
}