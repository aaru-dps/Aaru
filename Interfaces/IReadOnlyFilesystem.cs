// /***************************************************************************
// Aaru Data Preservation Suite
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
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;

namespace Aaru.CommonTypes.Interfaces;

/// <inheritdoc />
/// <summary>Defines the interface to implement reading the contents of a filesystem</summary>
public interface IReadOnlyFilesystem : IFilesystem
{
    /// <summary>Information about the filesystem as expected by Aaru Metadata</summary>
    FileSystem Metadata { get; }

    /// <summary>Retrieves a list of options supported by the filesystem, with name, type and description</summary>
    IEnumerable<(string name, Type type, string description)> SupportedOptions { get; }

    /// <summary>Supported namespaces</summary>
    Dictionary<string, string> Namespaces { get; }

    /// <summary>
    ///     Initializes whatever internal structures the filesystem plugin needs to be able to read files and directories
    ///     from the filesystem.
    /// </summary>
    /// <param name="imagePlugin"></param>
    /// <param name="partition"></param>
    /// <param name="encoding">Which encoding to use for this filesystem.</param>
    /// <param name="options">Dictionary of key=value pairs containing options to pass to the filesystem</param>
    /// <param name="namespace">Filename namespace</param>
    ErrorNumber Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding,
                      Dictionary<string, string> options, string @namespace);

    /// <summary>Frees all internal structures created by <see cref="Mount" /></summary>
    ErrorNumber Unmount();

    /// <summary>Maps a filesystem block from a file to a block from the underlying device.</summary>
    /// <returns>Error number.</returns>
    /// <param name="path">File path.</param>
    /// <param name="fileBlock">File block.</param>
    /// <param name="deviceBlock">Device block.</param>
    ErrorNumber MapBlock(string path, long fileBlock, out long deviceBlock);

    /// <summary>Gets the attributes of a file or directory</summary>
    /// <returns>Error number.</returns>
    /// <param name="path">File path.</param>
    /// <param name="attributes">File attributes.</param>
    ErrorNumber GetAttributes(string path, out FileAttributes attributes);

    /// <summary>Lists all extended attributes, alternate data streams and forks of the given file.</summary>
    /// <returns>Error number.</returns>
    /// <param name="path">Path.</param>
    /// <param name="xattrs">List of extended attributes, alternate data streams and forks.</param>
    ErrorNumber ListXAttr(string path, out List<string> xattrs);

    /// <summary>Reads an extended attribute, alternate data stream or fork from the given file.</summary>
    /// <returns>Error number.</returns>
    /// <param name="path">File path.</param>
    /// <param name="xattr">Extended attribute, alternate data stream or fork name.</param>
    /// <param name="buf">Buffer.</param>
    ErrorNumber GetXattr(string path, string xattr, ref byte[] buf);

    /// <summary>Reads data from a file (main/only data stream or data fork).</summary>
    /// <param name="path">File path.</param>
    /// <param name="offset">Offset.</param>
    /// <param name="size">Bytes to read.</param>
    /// <param name="buf">Buffer.</param>
    ErrorNumber Read(string path, long offset, long size, ref byte[] buf);

    /// <summary>Lists contents from a directory.</summary>
    /// <param name="path">Directory path.</param>
    /// <param name="contents">Directory contents.</param>
    ErrorNumber ReadDir(string path, out List<string> contents);

    /// <summary>Gets information about the mounted volume.</summary>
    /// <param name="stat">Information about the mounted volume.</param>
    ErrorNumber StatFs(out FileSystemInfo stat);

    /// <summary>Gets information about a file or directory.</summary>
    /// <param name="path">File path.</param>
    /// <param name="stat">File information.</param>
    ErrorNumber Stat(string path, out FileEntryInfo stat);

    /// <summary>Solves a symbolic link.</summary>
    /// <param name="path">Link path.</param>
    /// <param name="dest">Link destination.</param>
    ErrorNumber ReadLink(string path, out string dest);

    /// <summary>Opens a file for reading.</summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="node">Represents the opened file and is needed for other file-related operations.</param>
    /// <returns>Error number</returns>
    ErrorNumber OpenFile(string path, out IFileNode node);

    /// <summary>Closes an file, freeing any private data allocated on opening.</summary>
    /// <param name="node">The file node.</param>
    /// <returns>Error number.</returns>
    ErrorNumber CloseFile(IFileNode node);
}

/// <summary>Represents an opened file from a filesystem</summary>
public interface IFileNode
{
    /// <summary>Path to the file</summary>
    string Path { get; }
    /// <summary>File length</summary>
    long Length { get; }
    /// <summary>Current position in file</summary>
    long Offset { get; }
}