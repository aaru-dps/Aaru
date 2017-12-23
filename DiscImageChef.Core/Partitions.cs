// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Linq;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.DiscImages;
using DiscImageChef.Partitions;

namespace DiscImageChef.Core
{
    /// <summary>
    ///     Implements methods for handling partitions
    /// </summary>
    public static class Partitions
    {
        /// <summary>
        ///     Gets a list of all partitions present in the specified image
        /// </summary>
        /// <param name="image">Image</param>
        /// <returns>List of found partitions</returns>
        public static List<Partition> GetAll(ImagePlugin image)
        {
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();
            List<Partition> foundPartitions = new List<Partition>();
            List<Partition> childPartitions = new List<Partition>();
            List<ulong> checkedLocations = new List<ulong>();

            // Getting all partitions from device (e.g. tracks)
            if(image.ImageInfo.ImageHasPartitions)
                foreach(Partition imagePartition in image.GetPartitions())
                {
                    foreach(PartitionPlugin partitionPlugin in plugins.PartPluginsList.Values)
                        if(partitionPlugin.GetInformation(image, out List<Partition> partitions, imagePartition.Start))
                        {
                            foundPartitions.AddRange(partitions);
                            DicConsole.DebugWriteLine("Partitions", "Found {0} @ {1}", partitionPlugin.Name,
                                                      imagePartition.Start);
                        }

                    checkedLocations.Add(imagePartition.Start);
                }
            // Getting all partitions at start of device
            else
            {
                foreach(PartitionPlugin partitionPlugin in plugins.PartPluginsList.Values)
                    if(partitionPlugin.GetInformation(image, out List<Partition> partitions, 0))
                    {
                        foundPartitions.AddRange(partitions);
                        DicConsole.DebugWriteLine("Partitions", "Found {0} @ 0", partitionPlugin.Name);
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

                List<Partition> childs = new List<Partition>();

                foreach(PartitionPlugin partitionPlugin in plugins.PartPluginsList.Values)
                {
                    DicConsole.DebugWriteLine("Partitions", "Trying {0} @ {1}", partitionPlugin.Name,
                                              foundPartitions[0].Start);
                    if(!partitionPlugin.GetInformation(image, out List<Partition> partitions, foundPartitions[0].Start)
                    ) continue;

                    DicConsole.DebugWriteLine("Partitions", "Found {0} @ {1}", partitionPlugin.Name,
                                              foundPartitions[0].Start);
                    childs.AddRange(partitions);
                }

                checkedLocations.Add(foundPartitions[0].Start);

                DicConsole.DebugWriteLine("Partitions", "Got {0} childs", childs.Count);

                if(childs.Count > 0)
                {
                    foundPartitions.RemoveAt(0);

                    foreach(Partition child in childs)
                        if(checkedLocations.Contains(child.Start)) childPartitions.Add(child);
                        else foundPartitions.Add(child);
                }
                else
                {
                    childPartitions.Add(foundPartitions[0]);
                    foundPartitions.RemoveAt(0);
                }

                DicConsole.DebugWriteLine("Partitions", "Got {0} parents", foundPartitions.Count);
                DicConsole.DebugWriteLine("Partitions", "Got {0} partitions", childPartitions.Count);
            }

            // Be sure that device partitions are not excluded if not mapped by any scheme...
            if(image.ImageInfo.ImageHasPartitions)
            {
                List<ulong> startLocations =
                    childPartitions.Select(detectedPartition => detectedPartition.Start).ToList();

                childPartitions.AddRange(image.GetPartitions()
                                              .Where(imagePartition =>
                                                         !startLocations.Contains(imagePartition.Start)));
            }

            Partition[] childArray = childPartitions
                .OrderBy(part => part.Start).ThenBy(part => part.Length).ThenBy(part => part.Scheme).ToArray();

            for(long i = 0; i < childArray.LongLength; i++) childArray[i].Sequence = (ulong)i;

            return childArray.ToList();
        }

        /// <summary>
        ///     Adds all partition schemes from the specified list of partitions to statistics
        /// </summary>
        /// <param name="partitions">List of partitions</param>
        public static void AddSchemesToStats(List<Partition> partitions)
        {
            if(partitions == null || partitions.Count == 0) return;

            List<string> schemes = new List<string>();

            foreach(Partition part in partitions.Where(part => !schemes.Contains(part.Scheme)))
                schemes.Add(part.Scheme);

            foreach(string scheme in schemes) Statistics.AddPartition(scheme);
        }
    }
}