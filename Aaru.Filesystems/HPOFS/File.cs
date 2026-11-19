// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class HPOFS
{
    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out string _, out Entry entry);

        if(errno != ErrorNumber.NoError) return errno;

        // Root without a record
        if(entry is null)
        {
            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.Directory,
                BlockSize  = _bytesPerSector,
                Links      = 1
            };

            return ErrorNumber.NoError;
        }

        stat = new FileEntryInfo
        {
            Attributes = entry.IsDirectory ? FileAttributes.Directory : FileAttributes.File,
            BlockSize  = _bytesPerSector,
            Inode      = entry.Record.fileId,
            Links      = 1
        };

        if(entry.Record.creationDate != 0 || entry.Record.creationTime != 0)
            stat.CreationTime = DateHandlers.DosToDateTime(entry.Record.creationDate, entry.Record.creationTime);

        if(entry.Record.writeDate != 0 || entry.Record.writeTime != 0)
            stat.LastWriteTime = DateHandlers.DosToDateTime(entry.Record.writeDate, entry.Record.writeTime);

        if(entry.Subfiles.Length >= SUBFILE_DATA)
        {
            SubfileDescriptor data = entry.Subfiles[SUBFILE_DATA - 1];
            stat.Length = data.size;
            stat.Blocks = data.allocated / _bytesPerSector;
        }

        byte attributes = entry.Record.attributes;

        if((attributes & 0x01) != 0) stat.Attributes |= FileAttributes.ReadOnly;
        if((attributes & 0x02) != 0) stat.Attributes |= FileAttributes.Hidden;
        if((attributes & 0x04) != 0) stat.Attributes |= FileAttributes.System;
        if((attributes & 0x20) != 0) stat.Attributes |= FileAttributes.Archive;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        ErrorNumber errno = GetEntry(path, out string _, out Entry entry);

        if(errno != ErrorNumber.NoError) return errno;

        if(entry is null || entry.IsDirectory) return ErrorNumber.IsDirectory;

        Extent[] extents = [];
        long     length  = 0;

        if(entry.Subfiles.Length >= SUBFILE_DATA)
        {
            SubfileDescriptor data = entry.Subfiles[SUBFILE_DATA - 1];

            errno = GetExtents(data, SUBFILE_DATA, out extents);

            if(errno != ErrorNumber.NoError) return errno;

            length = data.size;
        }

        node = new HpofsFileNode
        {
            Path    = path,
            Length  = length,
            Offset  = 0,
            Extents = extents
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not HpofsFileNode hpofsNode) return ErrorNumber.InvalidArgument;

        hpofsNode.Extents = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted) return ErrorNumber.AccessDenied;

        if(node is not HpofsFileNode hpofsNode || hpofsNode.Extents is null) return ErrorNumber.InvalidArgument;

        if(buffer is null || buffer.Length < length || length < 0) return ErrorNumber.InvalidArgument;

        if(hpofsNode.Offset < 0) return ErrorNumber.InvalidArgument;

        if(hpofsNode.Offset >= hpofsNode.Length) return ErrorNumber.NoError;

        length = Math.Min(length, hpofsNode.Length - hpofsNode.Offset);

        if(length == 0) return ErrorNumber.NoError;

        ErrorNumber errno = ReadFromExtents(hpofsNode.Extents, (ulong)hpofsNode.Offset, (uint)length, out byte[] data);

        if(errno != ErrorNumber.NoError) return errno;

        Array.Copy(data, 0, buffer, 0, length);
        read              =  length;
        hpofsNode.Offset += length;

        return ErrorNumber.NoError;
    }
}