// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IFilesystem.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Filesystem plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Interface for filesystem plugins.
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
using Schemas;

namespace DiscImageChef.Filesystems
{
    /// <summary>
    ///     Interface to implement filesystem plugins.
    /// </summary>
    public interface IFilesystem
    {
        Encoding Encoding { get; }
        /// <summary>Plugin name.</summary>
        string Name { get; }
        /// <summary>Plugin UUID.</summary>
        Guid Id { get; }
        /// <summary>
        ///     Information about the filesystem as expected by CICM Metadata XML
        /// </summary>
        /// <value>Information about the filesystem as expected by CICM Metadata XML</value>
        FileSystemType XmlFsType { get; }

        /// <summary>
        ///     Identifies the filesystem in the specified LBA
        /// </summary>
        /// <param name="imagePlugin">Disk image.</param>
        /// <param name="partition">Partition.</param>
        /// <returns><c>true</c>, if the filesystem is recognized, <c>false</c> otherwise.</returns>
        bool Identify(IMediaImage imagePlugin, Partition partition);

        /// <summary>
        ///     Gets information about the identified filesystem.
        /// </summary>
        /// <param name="imagePlugin">Disk image.</param>
        /// <param name="partition">Partition.</param>
        /// <param name="information">Filesystem information.</param>
        /// <param name="encoding">Which encoding to use for this filesystem.</param>
        void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding);

        /// <summary>
        ///     Initializates whatever internal structures the filesystem plugin needs to be able to read files and directories
        ///     from the filesystem.
        /// </summary>
        /// <param name="imagePlugin"></param>
        /// <param name="partition"></param>
        /// <param name="encoding">Which encoding to use for this filesystem.</param>
        /// <param name="debug">If <c>true</c> enable debug.</param>
        Errno Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding, bool debug);

        /// <summary>
        ///     Frees all internal structures created by
        ///     <see
        ///         cref="M:DiscImageChef.Filesystems.Filesystem.Mount(DiscImageChef.DiscImages.IMediaImage,DiscImageChef.CommonTypes.Partition,System.Text.Encoding,System.Boolean)" />
        /// </summary>
        Errno Unmount();

        /// <summary>
        ///     Maps a filesystem block from a file to a block from the underlying device.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="fileBlock">File block.</param>
        /// <param name="deviceBlock">Device block.</param>
        Errno MapBlock(string path, long fileBlock, ref long deviceBlock);

        /// <summary>
        ///     Gets the attributes of a file or directory
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">File path.</param>
        /// <param name="attributes">File attributes.</param>
        Errno GetAttributes(string path, ref FileAttributes attributes);

        /// <summary>
        ///     Lists all extended attributes, alternate data streams and forks of the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="path">Path.</param>
        /// <param name="xattrs">List of extended attributes, alternate data streams and forks.</param>
        Errno ListXAttr(string path, ref List<string> xattrs);

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
        Errno ReadDir(string path, ref List<string> contents);

        /// <summary>
        ///     Gets information about the mounted volume.
        /// </summary>
        /// <param name="stat">Information about the mounted volume.</param>
        Errno StatFs(ref FileSystemInfo stat);

        /// <summary>
        ///     Gets information about a file or directory.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="stat">File information.</param>
        Errno Stat(string path, ref FileEntryInfo stat);

        /// <summary>
        ///     Solves a symbolic link.
        /// </summary>
        /// <param name="path">Link path.</param>
        /// <param name="dest">Link destination.</param>
        Errno ReadLink(string path, ref string dest);
    }
}