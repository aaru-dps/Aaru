// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Partitions.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Collections.Generic;
using DiscImageChef.CommonTypes;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using System.Linq;
using System.Net;
using DiscImageChef.Console;

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

            // Getting all partitions from device (e.g. tracks)
            if(image.ImageInfo.imageHasPartitions)
            {
                foreach(Partition imagePartition in image.GetPartitions())
                {
                    foreach(PartPlugin _partplugin in plugins.PartPluginsList.Values)
                    {
                        if(_partplugin.GetInformation(image, out List<Partition> _partitions, imagePartition.Start))
                            partitions.AddRange(_partitions);
                    }
                }
            }
            // Getting all partitions at start of device
            else
            {
                foreach(PartPlugin _partplugin in plugins.PartPluginsList.Values)
                {
                    if(_partplugin.GetInformation(image, out List<Partition> _partitions, 0))
                        partitions.AddRange(_partitions);
                }
            }

            while(partitions.Count > 0)
            {
                List<Partition> childs = new List<Partition>();

                foreach(PartPlugin _partplugin in plugins.PartPluginsList.Values)
                {
                    DicConsole.DebugWriteLine("Partitions", "Trying {0} @ {1}", _partplugin.Name, partitions[0].Start);
                    if(_partplugin.GetInformation(image, out List<Partition> _partitions, partitions[0].Start))
                        childs.AddRange(_partitions);
                }

                DicConsole.DebugWriteLine("Partitions", "Got {0} childs", childs.Count);

                if(childs.Count > 0)
                {
                    Partition father = partitions[0];

                    partitions.RemoveAt(0);

                    foreach(Partition child in childs)
                    {
                        if(child.Start == father.Start)
                            childPartitions.Add(father);
                        else
                        {
                            if(!partitions.Contains(child))
                                partitions.Add(child);
                        }
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

            Partition[] childArray = childPartitions.OrderBy(part => part.Start).ThenBy(part => part.Length).ThenBy(part => part.Scheme).ToArray();

            for(long i = 0; i < childArray.LongLength; i++)
                childArray[i].Sequence = (ulong)i;

            return childArray.ToList();
        }

        public static void AddSchemesToStats(List<Partition> partitions)
        {
            if(partitions == null || partitions.Count == 0)
                return;
            
            List<string> schemes = new List<string>();

            foreach(Partition part in partitions)
            {
                if(!schemes.Contains(part.Scheme))
                    schemes.Add(part.Scheme);
            }

            foreach(string scheme in schemes)
                Statistics.AddPartition(scheme);
        }
    }
}
