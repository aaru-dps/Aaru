// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Plan9.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Plan9 partitions.
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

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Partitions;

// This is the most stupid or the most intelligent partition scheme ever done, pick or take
// At sector 1 from offset, text resides (yes, TEXT) in following format:
// "part type start end\n"
// One line per partition, start and end relative to offset
// e.g.: "part nvram 10110 10112\npart fossil 10112 3661056\n"
/// <inheritdoc />
/// <summary>Implements decoding of Plan-9 partitions</summary>
public sealed class Plan9 : IPartition
{
#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.Plan9_Name;

    /// <inheritdoc />
    public Guid Id => new("F0BF4FFC-056E-4E7C-8B65-4EAEE250ADD9");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = new List<Partition>();

        if(sectorOffset + 2 >= imagePlugin.Info.Sectors) return false;

        ErrorNumber errno = imagePlugin.ReadSector(sectorOffset + 1, out byte[] sector);

        if(errno != ErrorNumber.NoError) return false;

        // While all of Plan9 is supposedly UTF-8, it uses ASCII strcmp for reading its partition table
        string[] really = StringHandlers.CToString(sector).Split('\n');

        foreach(string[] tokens in really.TakeWhile(part => part.Length >= 5 && part[..5] == "part ")
                                         .Select(part => part.Split(' '))
                                         .TakeWhile(tokens => tokens.Length == 4))
        {
            if(!ulong.TryParse(tokens[2], out ulong start) || !ulong.TryParse(tokens[3], out ulong end)) break;

            var part = new Partition
            {
                Length   = end - start + 1,
                Offset   = (start + sectorOffset) * imagePlugin.Info.SectorSize,
                Scheme   = Name,
                Sequence = (ulong)partitions.Count,
                Size     = (end - start + 1) * imagePlugin.Info.SectorSize,
                Start    = start + sectorOffset,
                Type     = tokens[1]
            };

            partitions.Add(part);
        }

        return partitions.Count > 0;
    }

#endregion
}