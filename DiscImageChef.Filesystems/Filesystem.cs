/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : Plugin.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Defines functions to be used by filesystem plugins and several constants.
 
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

namespace DiscImageChef.Filesystems
{
    /// <summary>
    /// Abstract class to implement filesystem plugins.
    /// </summary>
	public abstract class Filesystem
    {
        /// <summary>Plugin name.</summary>
        public string Name;
        /// <summary>Plugin UUID.</summary>
        public Guid PluginUUID;
        internal Schemas.FileSystemType xmlFSType;

        /// <summary>
        /// Information about the filesystem as expected by CICM Metadata XML
        /// </summary>
        /// <value>Information about the filesystem as expected by CICM Metadata XML</value>
        public Schemas.FileSystemType XmlFSType
        {
            get
            {
                return xmlFSType;
            }
        }

        protected Filesystem()
        {
        }

        /// <summary>
        /// Initializes a filesystem instance prepared for reading contents
        /// </summary>
        /// <param name="imagePlugin">Image plugin.</param>
        /// <param name="partitionStart">Partition start.</param>
        /// <param name="partitionEnd">Partition end.</param>
        protected Filesystem(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
        }

        /// <summary>
        /// Identifies the filesystem in the specified LBA
        /// </summary>
        /// <param name="imagePlugin">Disk image.</param>
        /// <param name="partitionStart">Partition start sector (LBA).</param>
        /// <param name="partitionEnd">Partition end sector (LBA).</param>
        /// <returns><c>true</c>, if the filesystem is recognized, <c>false</c> otherwise.</returns>
        public abstract bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd);

        /// <summary>
        /// Gets information about the identified filesystem.
        /// </summary>
        /// <param name="imagePlugin">Disk image.</param>
        /// <param name="partitionStart">Partition start sector (LBA).</param>
        /// <param name="partitionEnd">Partition end sector (LBA).</param>
        /// <param name="information">Filesystem information.</param>
        public abstract void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information);
    }
}

