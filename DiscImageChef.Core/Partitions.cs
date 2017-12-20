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
    public static class Partitions
    {
        public static List<Partition> GetAll(ImagePlugin image)
        {
            PluginBase plugins = new PluginBase();
            plugins.RegisterAllPlugins();
            List<Partition> partitions = new List<Partition>();
            List<Partition> childPartitions = new List<Partition>();
            List<ulong> checkedLocations = new List<ulong>();

            // Getting all partitions from device (e.g. tracks)
            if(image.ImageInfo.ImageHasPartitions)
            {
                foreach(Partition imagePartition in image.GetPartitions())
                {
                    foreach(PartitionPlugin _partplugin in plugins.PartPluginsList.Values)
                    {
                        if(_partplugin.GetInformation(image, out List<Partition> _partitions, imagePartition.Start))
                        {
                            partitions.AddRange(_partitions);
                            DicConsole.DebugWriteLine("Partitions", "Found {0} @ {1}", _partplugin.Name,
                                                      imagePartition.Start);
                        }
                    }

                    checkedLocations.Add(imagePartition.Start);
                }
            }
            // Getting all partitions at start of device
            else
            {
                foreach(PartitionPlugin _partplugin in plugins.PartPluginsList.Values)
                {
                    if(_partplugin.GetInformation(image, out List<Partition> _partitions, 0))
                    {
                        partitions.AddRange(_partitions);
                        DicConsole.DebugWriteLine("Partitions", "Found {0} @ 0", _partplugin.Name);
                    }
                }

                checkedLocations.Add(0);
            }

            while(partitions.Count > 0)
            {
                if(checkedLocations.Contains(partitions[0].Start))
                {
                    childPartitions.Add(partitions[0]);
                    partitions.RemoveAt(0);
                    continue;
                }

                List<Partition> childs = new List<Partition>();

                foreach(PartitionPlugin _partplugin in plugins.PartPluginsList.Values)
                {
                    DicConsole.DebugWriteLine("Partitions", "Trying {0} @ {1}", _partplugin.Name, partitions[0].Start);
                    if(_partplugin.GetInformation(image, out List<Partition> _partitions, partitions[0].Start))
                    {
                        DicConsole.DebugWriteLine("Partitions", "Found {0} @ {1}", _partplugin.Name,
                                                  partitions[0].Start);
                        childs.AddRange(_partitions);
                    }
                }

                checkedLocations.Add(partitions[0].Start);

                DicConsole.DebugWriteLine("Partitions", "Got {0} childs", childs.Count);

                if(childs.Count > 0)
                {
                    partitions.RemoveAt(0);

                    foreach(Partition child in childs)
                    {
                        if(checkedLocations.Contains(child.Start)) childPartitions.Add(child);
                        else partitions.Add(child);
                    }
                }
                else
                {
                    childPartitions.Add(partitions[0]);
                    partitions.RemoveAt(0);
                }

                DicConsole.DebugWriteLine("Partitions", "Got {0} parents", partitions.Count);
                DicConsole.DebugWriteLine("Partitions", "Got {0} partitions", childPartitions.Count);
            }

            // Be sure that device partitions are not excluded if not mapped by any scheme...
            if(image.ImageInfo.ImageHasPartitions)
            {
                List<ulong> startLocations = new List<ulong>();

                foreach(Partition detectedPartition in childPartitions) startLocations.Add(detectedPartition.Start);

                foreach(Partition imagePartition in image.GetPartitions())
                {
                    if(!startLocations.Contains(imagePartition.Start)) childPartitions.Add(imagePartition);
                }
            }

            Partition[] childArray = childPartitions
                .OrderBy(part => part.Start).ThenBy(part => part.Length).ThenBy(part => part.Scheme).ToArray();

            for(long i = 0; i < childArray.LongLength; i++) childArray[i].Sequence = (ulong)i;

            return childArray.ToList();
        }

        public static void AddSchemesToStats(List<Partition> partitions)
        {
            if(partitions == null || partitions.Count == 0) return;

            List<string> schemes = new List<string>();

            foreach(Partition part in partitions) { if(!schemes.Contains(part.Scheme)) schemes.Add(part.Scheme); }

            foreach(string scheme in schemes) Statistics.AddPartition(scheme);
        }
    }
}