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
// Copyright © 2018-2019 Michael Drüing
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/



// ReSharper disable UnusedMember.Global

namespace Aaru.CommonTypes.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes.Enums;

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

    /// <summary>Identifies if the specified path contains data recognizable by this archive instance</summary>
    /// <param name="path">Path.</param>
    bool Identify(string path);

    /// <summary>Identifies if the specified stream contains data recognizable by this archive instance</summary>
    /// <param name="stream">Stream.</param>
    bool Identify(Stream stream);

    /// <summary>Identifies if the specified buffer contains data recognizable by this archive instance</summary>
    /// <param name="buffer">Buffer.</param>
    bool Identify(byte[] buffer);

    /// <summary>Opens the specified path with this archive instance</summary>
    /// <param name="path">Path.</param>
    ErrorNumber Open(string path);

    /// <summary>Opens the specified stream with this archive instance</summary>
    /// <param name="stream">Stream.</param>
    ErrorNumber Open(Stream stream);

    /// <summary>Opens the specified buffer with this archive instance</summary>
    /// <param name="buffer">Buffer.</param>
    ErrorNumber Open(byte[] buffer);

    /// <summary>
    ///     Returns true if the archive has a file/stream/buffer currently opened and no
    ///     <see cref="M:Aaru.Filters.Filter.Close" /> has been issued.
    /// </summary>
    bool IsOpened();

    /// <summary>Closes all opened streams.</summary>
    void Close();

    /// <summary>Return a bitfield indicating the features supported by this archive type.</summary>
    /// <returns>The <c>ArchiveSupportedFeature</c> bitfield.</returns>
    /// <remarks>
    ///     This should be a constant, tied to the archive type, not to the particular features used by the
    ///     currently-opened archive file.
    /// </remarks>
    ArchiveSupportedFeature GetArchiveFeatures();

    /// <summary>Gets the number of entries (i.e. files) that are contained in this archive.</summary>
    /// <remarks>
    ///     Entries in this context can also mean directories or volume labels, for some types of archives that store
    ///     these explicitly. Do not rely on all entries being regular files!
    /// </remarks>
    /// <returns>The number of files.</returns>
    int GetNumberOfEntries();

    /// <summary>Gets the file name (and path) of the given entry in the archive.</summary>
    /// <remarks>
    ///     The path components are separated by a forward slash "/". <br /> The path should not start with a leading
    ///     slash (i.e. it should be relative, not absolute).
    /// </remarks>
    /// <seealso cref="Stat(int)" />
    /// <param name="entryNumber">The entry in the archive for which to return the file name.</param>
    /// <returns>The file name, with (relative) path</returns>
    string GetFilename(int entryNumber);

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
    /// <returns>The number of the entry corresponding to the given path, or -1 if the path does not exist.</returns>
    int GetEntryNumber(string fileName, bool caseInsensitiveMatch);

    /// <summary>Gets the (compressed) size of the given entry.</summary>
    /// <param name="entryNumber">The entry for which to get the compressed size.</param>
    /// <returns>The compressed size of the entry, or 0 if the entry is not a regular file.</returns>
    /// <remarks>The return value is equal to the return value of <c>GetUncompressedSize()</c> if the file is not compressed.</remarks>
    /// <seealso cref="GetUncompressedSize(int)" />
    long GetCompressedSize(int entryNumber);

    /// <summary>Gets the uncompressed size of the given entry.</summary>
    /// <param name="entryNumber">The entry for which to get the uncompressed size.</param>
    /// <returns>The uncompressed size of the entry, or 0 if the entry is not a regular file.</returns>
    /// <remarks>The return value is equal to the return value of <c>GetCompressedSize()</c> if the file is not compressed.</remarks>
    /// <seealso cref="GetCompressedSize(int)" />
    long GetUncompressedSize(int entryNumber);

    /// <summary>Gets the attributes of a file or directory.</summary>
    /// <seealso cref="Stat(int)" />
    /// <returns>Error number.</returns>
    /// <param name="entryNumber">The entry in the archive for which to retrieve the attributes.</param>
    /// <returns>File attributes, or zero if the archive does not support attributes.</returns>
    FileAttributes GetAttributes(int entryNumber);

    /// <summary>Lists all extended attributes, alternate data streams and forks of the given file.</summary>
    /// <param name="entryNumber">The entry in the archive for which to retrieve the list of attributes.</param>
    /// <returns>List of extended attributes, alternate data streams and forks.</returns>
    List<string> GetXAttrs(int entryNumber);

    /// <summary>Reads an extended attribute, alternate data stream or fork from the given file.</summary>
    /// <returns>Error number.</returns>
    /// <param name="entryNumber">The entry in the archive for which to retrieve the XAttr.</param>
    /// <param name="xattr">Extended attribute, alternate data stream or fork name.</param>
    /// <returns>Buffer with the XAttr data.</returns>
    ErrorNumber GetXattr(int entryNumber, string xattr, out byte[] buffer);

    /// <summary>Gets information about an entry in the archive.</summary>
    /// <remarks>Note that some of the data might be incomplete or not available at all, depending on the type of archive.</remarks>
    /// <seealso cref="GetAttributes(int)" />
    /// <seealso cref="GetFilename(int)" />
    /// <param name="entryNumber">The entry int he archive for which to get the information</param>
    /// <returns>The available information about the entry in the archive</returns>
    FileSystemInfo Stat(int entryNumber);

    /// <summary>
    ///     Returns the Filter for the given entry. It will return <c>null</c> if the entry in question is not a regular
    ///     file (i.e. directory, volume label, etc.)
    /// </summary>
    /// <param name="entryNumber">The entry for which the Filter should be returned.</param>
    /// <returns>The Filter for the given entry.</returns>
    IFilter GetEntry(int entryNumber);
}