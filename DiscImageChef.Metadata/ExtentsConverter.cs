// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ExtentsConverter.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;
using System.Collections.Generic;
using Extents;
using Schemas;

namespace DiscImageChef.Metadata
{
    public static class ExtentsConverter
    {
        public static ExtentType[] ToMetadata(ExtentsInt extents)
        {
            if(extents == null)
                return null;
            
            Tuple<int, int>[] tuples = extents.ToArray();
            ExtentType[] array = new ExtentType[tuples.Length];

            for(int i = 0; i < array.Length; i++)
                array[i] = new ExtentType { Start = tuples[i].Item1, End = tuples[i].Item2 };

            return array;
        }

        public static ExtentsInt FromMetadata(ExtentType[] extents)
        {
            if(extents == null)
                return null;

            List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

            foreach(ExtentType extent in extents)
                tuples.Add(new Tuple<int, int>(extent.Start, extent.End));

            return new ExtentsInt(tuples);
        }
    }
}
