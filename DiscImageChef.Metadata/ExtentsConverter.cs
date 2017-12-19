// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ExtentsConverter.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XML metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Converts extents to/from XML.
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

using System;
using System.Collections.Generic;
using Extents;
using Schemas;

namespace DiscImageChef.Metadata
{
    public static class ExtentsConverter
    {
        public static ExtentType[] ToMetadata(ExtentsULong extents)
        {
            if(extents == null) return null;

            Tuple<ulong, ulong>[] tuples = extents.ToArray();
            ExtentType[] array = new ExtentType[tuples.Length];

            for(ulong i = 0; i < (ulong)array.LongLength; i++)
                array[i] = new ExtentType {Start = tuples[i].Item1, End = tuples[i].Item2};

            return array;
        }

        public static ExtentsULong FromMetadata(ExtentType[] extents)
        {
            if(extents == null) return null;

            List<Tuple<ulong, ulong>> tuples = new List<Tuple<ulong, ulong>>();

            foreach(ExtentType extent in extents) tuples.Add(new Tuple<ulong, ulong>(extent.Start, extent.End));

            return new ExtentsULong(tuples);
        }
    }
}