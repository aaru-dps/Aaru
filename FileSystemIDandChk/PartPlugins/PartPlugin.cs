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
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Collections.Generic;

namespace FileSystemIDandChk.PartPlugins
{
    /// <summary>
    /// Abstract class to implement partitioning schemes interpreting plugins.
    /// </summary>
    public abstract class PartPlugin
    {
        /// <summary>Plugin name.</summary>
        public string Name;
        /// <summary>Plugin UUID.</summary>
        public Guid PluginUUID;

        protected PartPlugin()
        {
        }

        /// <summary>
        /// Interprets a partitioning scheme.
        /// </summary>
        /// <returns><c>true</c>, if partitioning scheme is recognized, <c>false</c> otherwise.</returns>
        /// <param name="imagePlugin">Disk image.</param>
        /// <param name="partitions">Returns list of partitions.</param>
        public abstract bool GetInformation(ImagePlugins.ImagePlugin imagePlugin, out List<Partition> partitions);
    }

    /// <summary>
    /// Partition structure.
    /// </summary>
    public struct Partition
    {
        /// <summary>Partition number, 0-started</summary>
        public ulong PartitionSequence;
        /// <summary>Partition type</summary>
        public string PartitionType;
        /// <summary>Partition name (if the scheme supports it)</summary>
        public string PartitionName;
        /// <summary>Start of the partition, in bytes</summary>
        public ulong PartitionStart;
        /// <summary>LBA of partition start</summary>
        public ulong PartitionStartSector;
        /// <summary>Length in bytes of the partition</summary>
        public ulong PartitionLength;
        /// <summary>Length in sectors of the partition</summary>
        public ulong PartitionSectors;
        /// <summary>Information that does not find space in this struct</summary>
        public string PartitionDescription;
    }
}