// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CompareBytes.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Compares two byte arrays.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Helpers;

public static partial class ArrayHelpers
{
    /// <summary>Compares two byte arrays</summary>
    /// <param name="different"><c>true</c> if they are different in any way</param>
    /// <param name="sameSize"><c>true</c> if they have the same size</param>
    /// <param name="compareArray1">Left array</param>
    /// <param name="compareArray2">Right array</param>
    public static void CompareBytes(out bool different, out bool sameSize, byte[] compareArray1, byte[] compareArray2)
    {
        different = false;
        sameSize  = true;

        long leastBytes;

        if(compareArray1.LongLength < compareArray2.LongLength)
        {
            sameSize   = false;
            leastBytes = compareArray1.LongLength;
        }
        else if(compareArray1.LongLength > compareArray2.LongLength)
        {
            sameSize   = false;
            leastBytes = compareArray2.LongLength;
        }
        else
            leastBytes = compareArray1.LongLength;

        for(long i = 0; i < leastBytes; i++)
            if(compareArray1[i] != compareArray2[i])
            {
                different = true;

                return;
            }
    }
}