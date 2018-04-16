// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : IArchive.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Archives.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface for an Archive.
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
// Copyright © 2018 Michael Drüing
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;

namespace DiscImageChef.Archives
{
    public interface IArchive
    {
        /// <summary>Descriptive name of the plugin</summary>
        string Name { get; }

        /// <summary>Unique UUID of the plugin</summary>
        Guid Id { get; }

        /// <summary>
        ///     Identifies if the specified path contains data recognizable by this archive instance
        /// </summary>
        /// <param name="path">Path.</param>
        bool Identify(string path);

        /// <summary>
        ///     Identifies if the specified stream contains data recognizable by this archive instance
        /// </summary>
        /// <param name="stream">Stream.</param>
        bool Identify(Stream stream);

        /// <summary>
        ///     Identifies if the specified buffer contains data recognizable by this archive instance
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        bool Identify(byte[] buffer);

        /// <summary>
        ///     Opens the specified path with this archive instance
        /// </summary>
        /// <param name="path">Path.</param>
        void Open(string path);

        /// <summary>
        ///     Opens the specified stream with this archive instance
        /// </summary>
        /// <param name="stream">Stream.</param>
        void Open(Stream stream);

        /// <summary>
        ///     Opens the specified buffer with this archive instance
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        void Open(byte[] buffer);

        /// <summary>
        ///     Returns true if the archive has a file/stream/buffer currently opened and no
        ///     <see cref="M:DiscImageChef.Filters.Filter.Close" /> has been issued.
        /// </summary>
        bool IsOpened();

        /// <summary>
        ///     Closes all opened streams.
        /// </summary>
        void Close();

        /// <summary>
        ///     Gets length of file referenced by ths archive.
        /// </summary>
        /// <returns>The length.</returns>
        long GetLength();

        /// <summary>
        ///     Gets the number of entries (i.e. files) that are contained in this archive.
        /// </summary>
        /// <remarks>
        ///     Entries in this context can also mean directories or volume labels, for some types of
        ///     archives that store these explicitly. Do not rely on all entries being regular files!
        /// </remarks>
        /// <returns>The number of files.</returns>
        int GetNumberOfEntries();

        /// <summary>
        ///     Gets the file name (and path) of the given entry in the archive.
        /// </summary>
        /// <remarks>
        ///     The path components are separated by a forward slash "/". <br />
        ///     The path should not start with a leading slash (i.e. it should be relative, not absolute).
        /// </remarks>
        /// <seealso cref="Stat(int)"/>
        /// <param name="entryNumber">The entry in the archive for which to return the file name.</param>
        /// <returns>The file name, with (relative) path</returns>
        string GetFilename(int entryNumber);

        /// <summary>
        ///     Gets the attributes of a file or directory.
        /// </summary>
        /// <seealso cref="Stat(int)"/>
        /// <returns>Error number.</returns>
        /// <param name="entryNumber">The entry in the archive for which to retreive the attributes.</param>
        /// <returns>File attributes, or zero if the archive does not support attributes.</returns>
        FileAttributes GetAttributes(int entryNumber);

        /// <summary>
        ///     Lists all extended attributes, alternate data streams and forks of the given file.
        /// </summary>
        /// <param name="entryNumber">The entry in the archive for which to retreive the list of attributes.</param>
        /// <returns>List of extended attributes, alternate data streams and forks.</returns>
        List<string> GetXAttr(int entryNumber);

        /// <summary>
        ///     Reads an extended attribute, alternate data stream or fork from the given file.
        /// </summary>
        /// <returns>Error number.</returns>
        /// <param name="entryNumber">The entry in the archive for which to retreive the XAttr.</param>
        /// <param name="xattr">Extended attribute, alternate data stream or fork name.</param>
        /// <returns>Buffer with the XAttr data.</returns>
        byte[] GetXattr(int entryNumber, string xattr);

        /// <summary>
        ///     Gets information about an entry in the archive.
        /// </summary>
        /// <remarks>
        ///     Note that some of the data might be incomplete or not available at all, depending on the type of
        ///     archive.
        /// </remarks>
        /// <seealso cref="GetAttributes(int)"/>
        /// <seealso cref="GetFilename(int)"/>
        /// <param name="entryNumber">The entry int he archive for which to get the information</param>
        /// <returns>The available information about the entry in the archive</returns>
        FileSystemInfo Stat(int entryNumber);

        /// <summary>
        ///     Returns the data stream of the given entry. It will return <c>null</c> if the entry in question
        ///     is not a regular file stream (i.e. directory, volume label, etc.)
        /// </summary>
        /// <param name="entryNumber">The entry for which the data stream should be returned.</param>
        /// <returns>The stream for the given entry.</returns>
        Stream GetStream(int entryNumber);
    }
}
