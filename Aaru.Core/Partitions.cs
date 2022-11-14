// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Partitions.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Logic to find partitions
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Core;

using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;

/// <summary>Implements methods for handling partitions</summary>
public static class Partitions
{
    /// <summary>Gets a list of all partitions present in the specified image</summary>
    /// <param name="image">Image</param>
    /// <returns>List of found partitions</returns>
    public static List<Partition> GetAll(IMediaImage image)
    {
        PluginBase plugins          = GetPluginBase.Instance;
        var        foundPartitions  = new List<Partition>();
        var        childPartitions  = new List<Partition>();
        var        checkedLocations = new List<ulong>();

        var tapeImage          = image as ITapeImage;
        var partitionableImage = image as IPartitionableMediaImage;

        // Create partitions from image files
        if(tapeImage?.Files != null)
            foreach(TapeFile tapeFile in tapeImage.Files)
            {
                foreach(IPartition partitionPlugin in plugins.PartPluginsList.Values)
                    if(partitionPlugin.GetInformation(image, out List<Partition> partitions, tapeFile.FirstBlock))
                    {
                        foundPartitions.AddRange(partitions);

                        AaruConsole.DebugWriteLine("Partitions", "Found {0} @ {1}", partitionPlugin.Name,
                                                   tapeFile.FirstBlock);
                    }

                checkedLocations.Add(tapeFile.FirstBlock);
            }

        // Getting all partitions from device (e.g. tracks)
        if(partitionableImage?.Partitions != null)
            foreach(Partition imagePartition in partitionableImage.Partitions)
            {
                foreach(IPartition partitionPlugin in plugins.PartPluginsList.Values)
                    if(partitionPlugin.GetInformation(image, out List<Partition> partitions, imagePartition.Start))
                    {
                        foundPartitions.AddRange(partitions);

                        AaruConsole.DebugWriteLine("Partitions", "Found {0} @ {1}", partitionPlugin.Name,
                                                   imagePartition.Start);
                    }

                checkedLocations.Add(imagePartition.Start);
            }

        // Getting all partitions at start of device
        if(!checkedLocations.Contains(0))
        {
            foreach(IPartition partitionPlugin in plugins.PartPluginsList.Values)
                if(partitionPlugin.GetInformation(image, out List<Partition> partitions, 0))
                {
                    foundPartitions.AddRange(partitions);
                    AaruConsole.DebugWriteLine("Partitions", "Found {0} @ 0", partitionPlugin.Name);
                }

            checkedLocations.Add(0);
        }

        while(foundPartitions.Count > 0)
        {
            if(checkedLocations.Contains(foundPartitions[0].Start))
            {
                childPartitions.Add(foundPartitions[0]);
                foundPartitions.RemoveAt(0);

                continue;
            }

            var children = new List<Partition>();

            foreach(IPartition partitionPlugin in plugins.PartPluginsList.Values)
            {
                AaruConsole.DebugWriteLine("Partitions", "Trying {0} @ {1}", partitionPlugin.Name,
                                           foundPartitions[0].Start);

                if(!partitionPlugin.GetInformation(image, out List<Partition> partitions, foundPartitions[0].Start))
                    continue;

                AaruConsole.DebugWriteLine("Partitions", "Found {0} @ {1}", partitionPlugin.Name,
                                           foundPartitions[0].Start);

                children.AddRange(partitions);
            }

            checkedLocations.Add(foundPartitions[0].Start);

            AaruConsole.DebugWriteLine("Partitions", "Got {0} children", children.Count);

            if(children.Count > 0)
            {
                foundPartitions.RemoveAt(0);

                foreach(Partition child in children)
                    if(checkedLocations.Contains(child.Start))
                        childPartitions.Add(child);
                    else
                        foundPartitions.Add(child);
            }
            else
            {
                childPartitions.Add(foundPartitions[0]);
                foundPartitions.RemoveAt(0);
            }

            AaruConsole.DebugWriteLine("Partitions", "Got {0} parents", foundPartitions.Count);
            AaruConsole.DebugWriteLine("Partitions", "Got {0} partitions", childPartitions.Count);
        }

        // Be sure that device partitions are not excluded if not mapped by any scheme...
        if(tapeImage is not null)
        {
            var startLocations = childPartitions.Select(detectedPartition => detectedPartition.Start).ToList();

            if(tapeImage.Files != null)
                childPartitions.AddRange(tapeImage.Files.Where(f => !startLocations.Contains(f.FirstBlock)).
                                                   Select(tapeFile => new Partition
                                                   {
                                                       Start    = tapeFile.FirstBlock,
                                                       Length   = tapeFile.LastBlock - tapeFile.FirstBlock + 1,
                                                       Sequence = tapeFile.File
                                                   }));
        }

        if(partitionableImage is not null)
        {
            var startLocations = childPartitions.Select(detectedPartition => detectedPartition.Start).ToList();

            if(partitionableImage.Partitions != null)
                childPartitions.AddRange(partitionableImage.Partitions.Where(imagePartition =>
                                                                                 !startLocations.
                                                                                     Contains(imagePartition.Start)));
        }

        Partition[] childArray = childPartitions.OrderBy(part => part.Start).ThenBy(part => part.Length).
                                                 ThenBy(part => part.Scheme).ToArray();

        for(long i = 0; i < childArray.LongLength; i++)
            childArray[i].Sequence = (ulong)i;

        return childArray.ToList();
    }

    /// <summary>Adds all partition schemes from the specified list of partitions to statistics</summary>
    /// <param name="partitions">List of partitions</param>
    public static void AddSchemesToStats(List<Partition> partitions)
    {
        if(partitions       == null ||
           partitions.Count == 0)
            return;

        var schemes = new List<string>();

        foreach(Partition part in partitions.Where(part => !schemes.Contains(part.Scheme)))
            schemes.Add(part.Scheme);

        foreach(string scheme in schemes)
            Statistics.AddPartition(scheme);
    }
}