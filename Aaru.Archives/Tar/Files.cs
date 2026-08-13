// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Files.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Tar plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Filters;
using Aaru.Helpers.IO;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;

namespace Aaru.Archives;

public sealed partial class Tar
{
#region IArchive Members

    /// <inheritdoc />
    public ErrorNumber GetFilename(int entryNumber, out string fileName)
    {
        fileName = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        fileName = _entries[entryNumber].Filename;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetCompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        Entry entry = _entries[entryNumber];
        length = entry.Size;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetUncompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        Entry entry = _entries[entryNumber];
        length = entry.IsSparse ? entry.RealSize : entry.Size;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetEntryNumber(string fileName, bool caseInsensitiveMatch, out int entryNumber)
    {
        entryNumber = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        StringComparison comparison = caseInsensitiveMatch
                                          ? StringComparison.CurrentCultureIgnoreCase
                                          : StringComparison.CurrentCulture;

        for(var i = 0; i < _entries.Count; i++)
        {
            if(_entries[i].Filename.Equals(fileName, comparison))
            {
                entryNumber = i;

                return ErrorNumber.NoError;
            }
        }

        return ErrorNumber.NoSuchFile;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(int entryNumber, out FileEntryInfo stat)
    {
        stat = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        Entry entry = _entries[entryNumber];

        long fileSize = entry.IsSparse ? entry.RealSize : entry.Size;

        FileAttributes attributes = entry.Type switch
                                    {
                                        TypeFlag.Directory   => FileAttributes.Directory,
                                        TypeFlag.SymLink     => FileAttributes.Symlink,
                                        TypeFlag.CharDevice  => FileAttributes.CharDevice,
                                        TypeFlag.BlockDevice => FileAttributes.BlockDevice,
                                        TypeFlag.Fifo        => FileAttributes.FIFO,
                                        TypeFlag.HardLink    => FileAttributes.File,
                                        TypeFlag.GnuSparse   => FileAttributes.File | FileAttributes.Sparse,
                                        _                    => FileAttributes.File
                                    };

        stat = new FileEntryInfo
        {
            Length           = fileSize,
            Attributes       = attributes,
            Blocks           = fileSize / BLOCK_SIZE + (fileSize % BLOCK_SIZE != 0 ? 1 : 0),
            BlockSize        = BLOCK_SIZE,
            LastWriteTimeUtc = entry.LastWriteTimeUtc,
            AccessTimeUtc    = entry.AccessTimeUtc,
            CreationTimeUtc  = entry.CreationTimeUtc,
            Mode             = entry.Mode,
            UID              = entry.Uid,
            GID              = entry.Gid
        };

        if(entry.DevMajor != 0 || entry.DevMinor != 0) stat.DeviceNo = (ulong)entry.DevMajor << 32 | entry.DevMinor;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetEntry(int entryNumber, out IFilter filter)
    {
        filter = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        Entry entry = _entries[entryNumber];

        // Directories, devices, FIFOs, and links have no data
        if(entry.Type is TypeFlag.Directory
                      or TypeFlag.CharDevice
                      or TypeFlag.BlockDevice
                      or TypeFlag.Fifo
                      or TypeFlag.HardLink
                      or TypeFlag.SymLink)
        {
            filter = new ZZZNoFilter();
            ErrorNumber emptyErrno = filter.Open(new MemoryStream([]));

            if(emptyErrno != ErrorNumber.NoError) return emptyErrno;

            return ErrorNumber.NoError;
        }

        Stream stream;

        if(entry.Size == 0)
            stream = new MemoryStream([]);
        else if(entry.IsSparse && entry.SparseRegions is { Count: > 0 })
        {
            // Validate against the file size, OffsetStream throws on invalid bounds
            if(entry.Size < 0 || entry.DataOffset < 0 || entry.DataOffset + entry.Size > _stream.Length)
                return ErrorNumber.InvalidArgument;

            Stream sourceStream = new OffsetStream(new NonClosableStream(_stream),
                                                   entry.DataOffset,
                                                   entry.DataOffset + entry.Size - 1);

            stream = new ForcedSeekStream<TarSparseStream>(entry.RealSize,
                                                           sourceStream,
                                                           entry.SparseRegions,
                                                           entry.RealSize);
        }
        else
        {
            // Validate against the file size, OffsetStream throws on invalid bounds
            if(entry.Size < 0 || entry.DataOffset < 0 || entry.DataOffset + entry.Size > _stream.Length)
                return ErrorNumber.InvalidArgument;

            stream = new OffsetStream(new NonClosableStream(_stream),
                                      entry.DataOffset,
                                      entry.DataOffset + entry.Size - 1);
        }

        filter = new ZZZNoFilter();
        ErrorNumber errno = filter.Open(stream);

        if(errno == ErrorNumber.NoError) return ErrorNumber.NoError;

        stream.Close();

        return errno;
    }

#endregion
}