/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : PartPlugin.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Partitioning scheme plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Defines functions to be used by partitioning scheme plugins and several constants.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Collections.Generic;

namespace FileSystemIDandChk.PartPlugins
{
    public abstract class PartPlugin
    {
        public string Name;
        public Guid PluginUUID;

        protected PartPlugin()
        {
        }

        public abstract bool GetInformation(ImagePlugins.ImagePlugin imagePlugin, out List<Partition> partitions);
    }

    public struct Partition
    {
        public ulong PartitionSequence;
        // Partition number, 0-started
        public string PartitionType;
        // Partition type
        public string PartitionName;
        // Partition name (if the scheme supports it)
        public ulong PartitionStart;
        // Start of the partition, in bytes
        public ulong PartitionStartSector;
        // LBA of partition start
        public ulong PartitionLength;
        // Length in bytes of the partition
        public ulong PartitionSectors;
        // Length in sectors of the partition
        public string PartitionDescription;
        // Information that does not find space in this struct
    }
}