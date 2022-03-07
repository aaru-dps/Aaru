// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IPartition.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines methods to be used by partitioning scheme plugins and several
//     constants.
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

namespace Aaru.CommonTypes.Interfaces;

using System;
using System.Collections.Generic;

/// <summary>Abstract class to implement partitioning schemes interpreting plugins.</summary>
public interface IPartition
{
    /// <summary>Plugin name.</summary>
    string Name { get; }
    /// <summary>Plugin UUID.</summary>
    Guid Id { get; }
    /// <summary>Plugin author</summary>
    string Author { get; }

    /// <summary>Interprets a partitioning scheme.</summary>
    /// <returns><c>true</c>, if partitioning scheme is recognized, <c>false</c> otherwise.</returns>
    /// <param name="imagePlugin">Disk image.</param>
    /// <param name="partitions">Returns list of partitions.</param>
    /// <param name="sectorOffset">At which sector to start searching for the partition scheme.</param>
    bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset);
}