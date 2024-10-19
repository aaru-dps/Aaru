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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;

namespace Aaru.Core;

/// <summary>Implements methods for handling partitions</summary>
public static class Partitions
{
    const string MODULE_NAME = "Partitions";

    /// <summary>Gets a list of all partitions present in the specified image</summary>
    /// <param name="image">Image</param>
    /// <returns>List of found partitions</returns>
    public static List<Partition> GetAll(IMediaImage image)
    {
        PluginRegister  plugins          = PluginRegister.Singleton;
        List<Partition> foundPartitions  = [];
        List<Partition> childPartitions  = [];
        List<ulong>     checkedLocations = [];

        var tapeImage          = image as ITapeImage;
        var partitionableImage = image as IPartitionableMediaImage;

        // Create partitions from image files
        if(tapeImage?.Files != null)
        {
            foreach(TapeFile tapeFile in tapeImage.Files)
            {
                foreach(IPartition plugin in plugins.Partitions.Values)
                {
                    if(plugin is null) continue;

                    if(!plugin.GetInformation(image, out List<Partition> partitions, tapeFile.FirstBlock)) continue;

                    foundPartitions.AddRange(partitions);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Core.Found_0_at_1,
                                               plugin.Name,
                                               tapeFile.FirstBlock);
                }

                checkedLocations.Add(tapeFile.FirstBlock);
            }
        }

        // Getting all partitions from device (e.g. tracks)
        if(partitionableImage?.Partitions != null)
        {
            foreach(Partition imagePartition in partitionableImage.Partitions)
            {
                foreach(IPartition plugin in plugins.Partitions.Values)
                {
                    if(plugin is null) continue;

                    if(!plugin.GetInformation(image, out List<Partition> partitions, imagePartition.Start)) continue;

                    foundPartitions.AddRange(partitions);

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization.Core.Found_0_at_1,
                                               plugin.Name,
                                               imagePartition.Start);
                }

                checkedLocations.Add(imagePartition.Start);
            }
        }

        // Getting all partitions at start of device
        if(!checkedLocations.Contains(0))
        {
            foreach(IPartition plugin in plugins.Partitions.Values)
            {
                if(plugin is null) continue;

                if(!plugin.GetInformation(image, out List<Partition> partitions, 0)) continue;

                foundPartitions.AddRange(partitions);
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Found_0_at_zero, plugin.Name);
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

            List<Partition> children = [];

            foreach(IPartition plugin in plugins.Partitions.Values)
            {
                if(plugin is null) continue;

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.Core.Trying_0_at_1,
                                           plugin.Name,
                                           foundPartitions[0].Start);

                if(!plugin.GetInformation(image, out List<Partition> partitions, foundPartitions[0].Start)) continue;

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.Core.Found_0_at_1,
                                           plugin.Name,
                                           foundPartitions[0].Start);

                children.AddRange(partitions);
            }

            checkedLocations.Add(foundPartitions[0].Start);

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Got_0_children, children.Count);

            if(children.Count > 0)
            {
                foundPartitions.RemoveAt(0);

                foreach(Partition child in children)
                {
                    if(checkedLocations.Contains(child.Start))
                        childPartitions.Add(child);
                    else
                        foundPartitions.Add(child);
                }
            }
            else
            {
                childPartitions.Add(foundPartitions[0]);
                foundPartitions.RemoveAt(0);
            }

            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Got_0_parents,    foundPartitions.Count);
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Core.Got_0_partitions, childPartitions.Count);
        }

        // Be sure that device partitions are not excluded if not mapped by any scheme...
        if(tapeImage is not null)
        {
            var startLocations = childPartitions.Select(detectedPartition => detectedPartition.Start).ToList();

            if(tapeImage.Files != null)
            {
                childPartitions.AddRange(tapeImage.Files.Where(f => !startLocations.Contains(f.FirstBlock))
                                                  .Select(tapeFile => new Partition
                                                   {
                                                       Start    = tapeFile.FirstBlock,
                                                       Length   = tapeFile.LastBlock - tapeFile.FirstBlock + 1,
                                                       Sequence = tapeFile.File
                                                   }));
            }
        }

        if(partitionableImage is not null)
        {
            var startLocations = childPartitions.Select(detectedPartition => detectedPartition.Start).ToList();

            if(partitionableImage.Partitions != null)
            {
                childPartitions.AddRange(partitionableImage.Partitions.Where(imagePartition =>
                                                                                 !startLocations.Contains(imagePartition
                                                                                    .Start)));
            }
        }

        Partition[] childArray = childPartitions.OrderBy(part => part.Start)
                                                .ThenBy(part => part.Length)
                                                .ThenBy(part => part.Scheme)
                                                .ToArray();

        for(long i = 0; i < childArray.LongLength; i++) childArray[i].Sequence = (ulong)i;

        return childArray.ToList();
    }

    /// <summary>Adds all partition schemes from the specified list of partitions to statistics</summary>
    /// <param name="partitions">List of partitions</param>
    public static void AddSchemesToStats(List<Partition> partitions)
    {
        if(partitions == null || partitions.Count == 0) return;

        List<string> schemes = [];

        foreach(Partition part in partitions.Where(part => !schemes.Contains(part.Scheme))) schemes.Add(part.Scheme);

        foreach(string scheme in schemes) Statistics.AddPartition(scheme);
    }
}