// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ArrayIsEmpty.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods for detecting an empty array.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

namespace DiscImageChef
{
    public static partial class ArrayHelpers
    {
        public static bool ArrayIsNullOrWhiteSpace(byte[] array)
        {
            if(array == null)
                return true;

            foreach(byte b in array)
                if(b != 0x00 && b != 0x20)
                    return false;

            return true;
        }

        public static bool ArrayIsNullOrEmpty(byte[] array)
        {
            if(array == null)
                return true;

            foreach(byte b in array)
                if(b != 0x00)
                    return false;

            return true;
        }
    }
}

