// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ArrayIsEmpty.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;

namespace DiscImageChef
{
    public static partial class ArrayHelpers
    {
        public static bool ArrayIsNullOrWhiteSpace(byte[] array)
        {
            if (array == null)
                return true;

            foreach (byte b in array)
                if (b != 0x00 && b != 0x20)
                    return false;

            return true;
        }

        public static bool ArrayIsNullOrEmpty(byte[] array)
        {
            if (array == null)
                return true;

            foreach (byte b in array)
                if (b != 0x00)
                    return false;

            return true;
        }
    }
}

