// /***************************************************************************
// Aaru Data Preservation Suite
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
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.CommonTypes.Extents;

using System;
using System.Linq;
using Schemas;

/// <summary>Converts extents</summary>
public static class ExtentsConverter
{
    /// <summary>Converts unsigned long integer extents into XML based extents</summary>
    /// <param name="extents">Extents</param>
    /// <returns>XML based extents</returns>
    public static ExtentType[] ToMetadata(ExtentsULong extents)
    {
        if(extents == null)
            return null;

        Tuple<ulong, ulong>[] tuples = extents.ToArray();
        var                   array  = new ExtentType[tuples.Length];

        for(ulong i = 0; i < (ulong)array.LongLength; i++)
            array[i] = new ExtentType
            {
                Start = tuples[i].Item1,
                End   = tuples[i].Item2
            };

        return array;
    }

    /// <summary>Converts XML based extents into unsigned long integer extents</summary>
    /// <param name="extents">XML based extents</param>
    /// <returns>Extents</returns>
    public static ExtentsULong FromMetadata(ExtentType[] extents)
    {
        if(extents == null)
            return null;

        var tuples = extents.Select(extent => new Tuple<ulong, ulong>(extent.Start, extent.End)).ToList();

        return new ExtentsULong(tuples);
    }
}