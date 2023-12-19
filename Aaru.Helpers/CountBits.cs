// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CountBits.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Counts bits in a number.
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

namespace Aaru.Helpers;

/// <summary>Helper operations to count bits</summary>
public static class CountBits
{
    /// <summary>Counts the number of bits set to <c>true</c> in a number</summary>
    /// <param name="number">Number</param>
    /// <returns>Bits set to <c>true</c></returns>
    public static int Count(uint number)
    {
        number -= number >> 1 & 0x55555555;
        number =  (number & 0x33333333) + (number >> 2 & 0x33333333);

        return (int)((number + (number >> 4) & 0x0F0F0F0F) * 0x01010101 >> 24);
    }
}