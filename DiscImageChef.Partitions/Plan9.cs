// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Partitions
{
    // This is the most stupid or the most intelligent partition scheme ever done, pick or take
    // At sector 1 from offset, text resides (yes, TEXT) in following format:
    // "part type start end\n"
    // One line per partition, start and end relative to offset
    // e.g.: "part nvram 10110 10112\npart fossil 10112 3661056\n"
    public class Plan9 : PartitionPlugin
    {
        public Plan9()
        {
            Name = "Plan9 partition table";
            PluginUuid = new Guid("F0BF4FFC-056E-4E7C-8B65-4EAEE250ADD9");
        }

        public override bool GetInformation(DiscImages.ImagePlugin imagePlugin, out List<Partition> partitions,
                                            ulong sectorOffset)
        {
            partitions = new List<Partition>();

            if(sectorOffset + 2 >= imagePlugin.GetSectors()) return false;

            byte[] sector = imagePlugin.ReadSector(sectorOffset + 1);
            // While all of Plan9 is supposedly UTF-8, it uses ASCII strcmp for reading its partition table
            string[] really = StringHandlers.CToString(sector).Split(new[] {'\n'});

            foreach(string part in really)
            {
                if(part.Length < 5 || part.Substring(0, 5) != "part ") break;

                string[] tokens = part.Split(new[] {' '});

                if(tokens.Length != 4) break;

                if(!ulong.TryParse(tokens[2], out ulong start) || !ulong.TryParse(tokens[3], out ulong end)) break;

                Partition _part = new Partition
                {
                    Length = (end - start) + 1,
                    Offset = (start + sectorOffset) * imagePlugin.GetSectorSize(),
                    Scheme = Name,
                    Sequence = (ulong)partitions.Count,
                    Size = ((end - start) + 1) * imagePlugin.GetSectorSize(),
                    Start = start + sectorOffset,
                    Type = tokens[1]
                };

                partitions.Add(_part);
            }

            return partitions.Count > 0;
        }
    }
}