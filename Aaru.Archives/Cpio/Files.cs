// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Files.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CPIO plugin.
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

public sealed partial class Cpio
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

        length = _entries[entryNumber].CompressedSize;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetUncompressedSize(int entryNumber, out long length)
    {
        length = -1;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        length = _entries[entryNumber].Size;

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

        FileAttributes attributes = entry.FileType switch
                                    {
                                        CpioFileType.Directory   => FileAttributes.Directory,
                                        CpioFileType.Symlink     => FileAttributes.Symlink,
                                        CpioFileType.CharDevice  => FileAttributes.CharDevice,
                                        CpioFileType.BlockDevice => FileAttributes.BlockDevice,
                                        CpioFileType.Fifo        => FileAttributes.FIFO,
                                        CpioFileType.Socket      => FileAttributes.Socket,
                                        _                        => FileAttributes.File
                                    };

        stat = new FileEntryInfo
        {
            Length           = entry.Size,
            Attributes       = attributes,
            LastWriteTimeUtc = entry.LastWriteTimeUtc,
            Mode             = entry.Mode,
            UID              = entry.Uid,
            GID              = entry.Gid,
            Inode            = entry.Inode,
            Links            = entry.Nlink
        };

        if(entry.FileType is CpioFileType.CharDevice or CpioFileType.BlockDevice)
            stat.DeviceNo = (ulong)entry.RdevMajor << 32 | entry.RdevMinor;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber GetEntry(int entryNumber, out IFilter filter)
    {
        filter = null;

        if(!Opened) return ErrorNumber.NotOpened;

        if(entryNumber < 0 || entryNumber >= _entries.Count) return ErrorNumber.OutOfRange;

        Entry entry = _entries[entryNumber];

        // Non-regular entries have no extractable data
        if(entry.FileType is CpioFileType.Directory
                          or CpioFileType.CharDevice
                          or CpioFileType.BlockDevice
                          or CpioFileType.Fifo
                          or CpioFileType.Socket)
        {
            filter = new ZZZNoFilter();
            ErrorNumber emptyErrno = filter.Open(new MemoryStream([]));

            if(emptyErrno != ErrorNumber.NoError) return emptyErrno;

            return ErrorNumber.NoError;
        }

        Stream stream;

        if(entry.Size == 0)
            stream = new MemoryStream([]);
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