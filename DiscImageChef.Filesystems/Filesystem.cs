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

        /// <summary>
        /// Initializates whatever internal structures the filesystem plugin needs to be able to read files and directories from the filesystem.
        /// </summary>
        public abstract Errno Mount();

        /// <summary>
        /// Frees all internal structures created by <see cref="Mount"/> 
        /// </summary>
        public abstract Errno Unmount();

        /// <summary>
        /// Maps a filesystem block from a file to a block from the underlying device.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="fileBlock">File block.</param>
        /// <param name="deviceBlock">Device block.</param>
        public abstract Errno MapBlock(string path, long fileBlock, ref long deviceBlock);

        /// <summary>
        /// Gets the attributes of a file or directory
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="attributes">File attributes.</param>
        public abstract Errno GetAttributes(string path, ref FileAttributes attributes);

        /// <summary>
        /// Lists all extended attributes, alternate data streams and forks of the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">Path.</param>
        /// <param name="xattrs">List of extended attributes, alternate data streams and forks.</param>
        public abstract Errno ListXAttr(string path, ref List<string> xattrs);

        /// <summary>
        /// Reads an extended attribute, alternate data stream or fork from the given file.
        /// </summary>
        /// <returns>The extended attribute, alternate data stream or fork name.</returns>
        /// <param name="path">File path.</param>
        /// <param name="xattr">Extendad attribute, alternate data stream or fork name.</param>
        /// <param name="buf">Buffer.</param>
        public abstract Errno GetXattr(string path, string xattr, ref byte[] buf);

        /// <summary>
        /// Reads data from a file (main/only data stream or data fork).
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="size">Bytes to read.</param>
        /// <param name="buf">Buffer.</param>
        public abstract Errno Read(string path, long offset, long size, ref byte[] buf);

        /// <summary>
        /// Lists contents from a directory.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <param name="contents">Directory contents.</param>
        public abstract Errno ReadDir(string path, ref List<string> contents);

        /// <summary>
        /// Gets information about the mounted volume.
        /// </summary>
        /// <param name="stat">Information about the mounted volume.</param>
        public abstract Errno StatFs(ref FileSystemInfo stat);

        /// <summary>
        /// Gets information about a file or directory.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="stat">File information.</param>
        public abstract Errno Stat(string path, ref FileEntryInfo stat);

        /// <summary>
        /// Solves a symbolic link.
        /// </summary>
        /// <param name="path">Link path.</param>
        /// <param name="dest">Link destination.</param>
        public abstract Errno ReadLink(string path, ref string dest);
    }
}

