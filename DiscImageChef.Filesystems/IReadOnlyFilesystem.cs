// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IReadOnlyFilesystem.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filesystem plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Interface for filesystem plugins that offer read-only support of their
//     contents.
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
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Filesystems
{
    /// <summary>
    ///     Interface to implement filesystem plugins.
    /// </summary>
    public interface IReadOnlyFilesystem : IFilesystem
    {
        /// <summary>
        ///     Initializates whatever internal structures the filesystem plugin needs to be able to read files and directories
        ///     from the filesystem.
        /// </summary>
        /// <param name="imagePlugin"></param>
        /// <param name="partition"></param>
        /// <param name="encoding">Which encoding to use for this filesystem.</param>
        /// <param name="options">Dictionary of key=value pairs containing options to pass to the filesystem</param>
        Errno Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                    Dictionary<string, string> options);

        /// <summary>
        ///     Frees all internal structures created by
        ///     <see cref="Mount" />
        /// </summary>
        Errno Unmount();

        /// <summary>
        ///     Maps a filesystem block from a file to a block from the underlying device.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="fileBlock">File block.</param>
        /// <param name="deviceBlock">Device block.</param>
        Errno MapBlock(string path, long fileBlock, out long deviceBlock);

        /// <summary>
        ///     Gets the attributes of a file or directory
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="attributes">File attributes.</param>
        Errno GetAttributes(string path, out FileAttributes attributes);

        /// <summary>
        ///     Lists all extended attributes, alternate data streams and forks of the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">Path.</param>
        /// <param name="xattrs">List of extended attributes, alternate data streams and forks.</param>
        Errno ListXAttr(string path, out List<string> xattrs);

        /// <summary>
        ///     Reads an extended attribute, alternate data stream or fork from the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="xattr">Extendad attribute, alternate data stream or fork name.</param>
        /// <param name="buf">Buffer.</param>
        Errno GetXattr(string path, string xattr, ref byte[] buf);

        /// <summary>
        ///     Reads data from a file (main/only data stream or data fork).
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="size">Bytes to read.</param>
        /// <param name="buf">Buffer.</param>
        Errno Read(string path, long offset, long size, ref byte[] buf);

        /// <summary>
        ///     Lists contents from a directory.
        /// </summary>
        /// <param name="path">Directory path.</param>
        /// <param name="contents">Directory contents.</param>
        Errno ReadDir(string path, out List<string> contents);

        /// <summary>
        ///     Gets information about the mounted volume.
        /// </summary>
        /// <param name="stat">Information about the mounted volume.</param>
        Errno StatFs(out FileSystemInfo stat);

        /// <summary>
        ///     Gets information about a file or directory.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="stat">File information.</param>
        Errno Stat(string path, out FileEntryInfo stat);

        /// <summary>
        ///     Solves a symbolic link.
        /// </summary>
        /// <param name="path">Link path.</param>
        /// <param name="dest">Link destination.</param>
        Errno ReadLink(string path, out string dest);
        
        /// <summary>
        /// Retrieves a list of options supported by the filesystem, with name, type and description 
        /// </summary>
        (string name, Type type, string description)[] ListOptions();
    }
}