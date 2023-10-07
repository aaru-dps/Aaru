// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IArchive.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
//
// Component      : Archives.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines the interface for implementing archive plugins.
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
// Copyright © 2018-2023 Michael Drüing
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
using FileAttributes = System.IO.FileAttributes;

namespace Aaru.CommonTypes.Interfaces;

/// <summary>Supported archive features</summary>
[Flags]
public enum ArchiveSupportedFeature : uint
{
    /// <summary>The archive supports filenames for its entries. If this flag is not set, files can only be accessed by number.</summary>
    SupportsFilenames = 1 << 0,
    /// <summary>
    ///     The archive supports compression. If this flag is not set, compressed and uncompressed lengths are always the
    ///     same.
    /// </summary>
    SupportsCompression = 1 << 1,
    /// <summary>
    ///     The archive supports subdirectories. If this flag is not set, all filenames are guaranteed to not contain any
    ///     "/" character.
    /// </summary>
    SupportsSubdirectories = 1 << 2,
    /// <summary>
    ///     The archive supports explicit entries for directories (like Zip, for example). If this flag is not set,
    ///     directories are implicit by the relative name of the files.
    /// </summary>
    HasExplicitDirectories = 1 << 3,
    /// <summary>The archive stores a timestamp with each entry if this flag is set.</summary>
    HasEntryTimestamp = 1 << 4,
    /// <summary>If this flag is set, individual files or the whole archive might be encrypted or password-protected.</summary>
    SupportsProtection = 1 << 5, // TODO: not implemented yet

    /// <summary>If this flag is set, the archive supports returning extended attributes (Xattrs) for each entry.</summary>
    SupportsXAttrs = 1 << 6
}

/// <summary>Defines the interface to handle an archive (e.g. ZIP, WAD, etc)</summary>
public interface IArchive
{
    /// <summary>Descriptive name of the plugin</summary>
    string Name { get; }

    /// <summary>Unique UUID of the plugin</summary>
    Guid Id { get; }

    /// <summary>Plugin author</summary>
    string Author { get; }

    /// <summary>
    ///     Returns true if the archive has a file/stream/buffer currently opened and no
    ///     <see cref="M:Aaru.Filters.Filter.Close" /> has been issued.
    /// </summary>
    /// <value><c>true</c> if the archive is opened, <c>false</c> otherwise.</value>
    bool Opened { get; }

    /// <summary>Return a bitfield indicating the features supported by this archive type.</summary>
    /// <value>The <c>ArchiveSupportedFeature</c> bitfield.</value>
    /// <remarks>
    ///     If the archive is not opened, this returns the capabilities of the archive format, otherwise it returns the
    ///     capabilities in use by the currently opened archive.
    /// </remarks>
    ArchiveSupportedFeature ArchiveFeatures { get; }

    /// <summary>Gets the number of entries (i.e. files) that are contained in this archive.</summary>
    /// <remarks>
    ///     Entries in this context can also mean directories or volume labels, for some types of archives that store
    ///     these explicitly. Do not rely on all entries being regular files!
    /// </remarks>
    /// <value>The number of files.</value>
    int NumberOfEntries { get; }

    /// <summary>Identifies if the specified filter contains data recognizable by this archive instance</summary>
    /// <param name="filter">Filter that contains the archive. This allows use to handle .tar.gz and similars.</param>
    bool Identify(IFilter filter);

    /// <summary>Opens the specified stream with this archive instance</summary>
    /// <param name="filter">Filter that contains the archive. This allows use to handle .tar.gz and similars.</param>
    /// <param name="encoding">The encoding codepage to use for this archive.</param>
    ErrorNumber Open(IFilter filter, Encoding encoding);

    /// <summary>Closes the archive.</summary>
    void Close();

    /// <summary>Gets the file name (and path) of the given entry in the archive.</summary>
    /// <remarks>
    ///     The path components are separated by a forward slash "/". <br /> The path should not start with a leading
    ///     slash (i.e. it should be relative, not absolute).
    /// </remarks>
    /// <seealso cref="Stat" />
    /// <param name="entryNumber">The entry in the archive for which to return the file name.</param>
    /// <param name="fileName">The file name, with (relative) path</param>
    /// <returns>Error number.</returns>
    ErrorNumber GetFilename(int entryNumber, out string fileName);

    /// <summary>
    ///     Gets the entry number for a particular file path in the archive. <c>fileName</c> is the relative path of the
    ///     file in the archive. If the file cannot be found, -1 is returned.
    /// </summary>
    /// <remarks>
    ///     The path should be relative (no leading slash), using regular slashes as path separator, and be normalized,
    ///     i.e. no "foo//bar" or "foo/../bar" path components.
    /// </remarks>
    /// <param name="fileName">The relative path for which to get the entry number.</param>
    /// <param name="caseInsensitiveMatch">If set, do a case insensitive matching and return the first file that matches.</param>
    /// <param name="entryNumber">The number of the entry corresponding to the given path, or -1 if the path does not exist.</param>
    /// <returns>Error number.</returns>
    ErrorNumber GetEntryNumber(string fileName, bool caseInsensitiveMatch, out int entryNumber);

    /// <summary>Gets the (compressed) size of the given entry.</summary>
    /// <param name="entryNumber">The entry for which to get the compressed size.</param>
    /// <param name="length">The compressed size of the entry, or 0 if the entry is not a regular file.</param>
    /// <returns>Error number.</returns>
    /// <remarks>The return value is equal to the return value of <c>GetUncompressedSize()</c> if the file is not compressed.</remarks>
    /// <seealso cref="GetUncompressedSize" />
    ErrorNumber GetCompressedSize(int entryNumber, out long length);

    /// <summary>Gets the uncompressed size of the given entry.</summary>
    /// <param name="entryNumber">The entry for which to get the uncompressed size.</param>
    /// <param name="length">The uncompressed size of the entry, or 0 if the entry is not a regular file.</param>
    /// <returns>Error number.</returns>
    /// <remarks>The return value is equal to the return value of <c>GetCompressedSize()</c> if the file is not compressed.</remarks>
    /// <seealso cref="GetCompressedSize" />
    ErrorNumber GetUncompressedSize(int entryNumber, out long length);

    /// <summary>Gets the attributes of a file or directory.</summary>
    /// <param name="entryNumber">The entry in the archive for which to retrieve the attributes.</param>
    /// <param name="attributes">File attributes, or zero if the archive does not support attributes.</param>
    /// <returns>Error number.</returns>
    /// <seealso cref="Stat" />
    ErrorNumber GetAttributes(int entryNumber, out FileAttributes attributes);

    /// <summary>Lists all extended attributes, alternate data streams and forks of the given file.</summary>
    /// <param name="entryNumber">The entry in the archive for which to retrieve the list of attributes.</param>
    /// <param name="xattrs">List of extended attributes, alternate data streams and forks.</param>
    /// <returns>Error number.</returns>
    ErrorNumber ListXAttr(int entryNumber, out List<string> xattrs);

    /// <summary>Reads an extended attribute, alternate data stream or fork from the given file.</summary>
    /// <param name="entryNumber">The entry in the archive for which to retrieve the XAttr.</param>
    /// <param name="xattr">Extended attribute, alternate data stream or fork name.</param>
    /// <param name="buffer">Buffer where the extended attribute data will be stored.</param>
    /// <returns>Error number.</returns>
    ErrorNumber GetXattr(int entryNumber, string xattr, ref byte[] buffer);

    /// <summary>Gets information about an entry in the archive.</summary>
    /// <remarks>Note that some of the data might be incomplete or not available at all, depending on the type of archive.</remarks>
    /// <param name="entryNumber">The entry int he archive for which to get the information</param>
    /// <param name="stat">The available information about the entry in the archive</param>
    /// <returns>Error number.</returns>
    /// <seealso cref="GetAttributes" />
    /// <seealso cref="GetFilename" />
    ErrorNumber Stat(int entryNumber, out FileEntryInfo stat);

    /// <summary>
    ///     Returns the Filter for the given entry. It will return <c>null</c> if the entry in question is not a regular
    ///     file (i.e. directory, volume label, etc.)
    /// </summary>
    /// <param name="entryNumber">The entry for which the Filter should be returned.</param>
    /// <param name="filter">The Filter for the given entry.</param>
    /// <returns>Error number.</returns>
    ErrorNumber GetEntry(int entryNumber, out IFilter filter);

    /// <summary>
    ///     Gets user readable information about the archive. The exact contents depend on the archive plugin implementation.
    /// </summary>
    /// <param name="filter">Filter that handles the archive.</param>
    /// <param name="encoding">The encoding codepage to use with the archive.</param>
    /// <param name="information">Variable that holds the user readable information.</param>
    void GetInformation(IFilter filter, Encoding encoding, out string information);
}