// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : U.C.S.D. Pascal filesystem plugin.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Filesystems;

// Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
public sealed partial class PascalPlugin
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber GetAttributes(string path, out FileAttributes attributes)
    {
        attributes = new FileAttributes();

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        ErrorNumber error = GetFileEntry(path, out _);

        if(error != ErrorNumber.NoError)
            return error;

        attributes = FileAttributes.File;

        return error;
    }

    /// <inheritdoc />
    public ErrorNumber OpenFile(string path, out IFileNode node)
    {
        node = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        string[] pathElements = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        byte[] file;

        if(_debug && (string.Compare(path, "$",     StringComparison.InvariantCulture) == 0 ||
                      string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0))
            file = string.Compare(path, "$", StringComparison.InvariantCulture) == 0 ? _catalogBlocks : _bootBlocks;
        else
        {
            ErrorNumber error = GetFileEntry(path, out PascalFileEntry entry);

            if(error != ErrorNumber.NoError)
                return error;

            error = _device.ReadSectors((ulong)entry.FirstBlock                    * _multiplier,
                                        (uint)(entry.LastBlock - entry.FirstBlock) * _multiplier, out byte[] tmp);

            if(error != ErrorNumber.NoError)
                return error;

            file = new byte[(entry.LastBlock - entry.FirstBlock - 1) * _device.Info.SectorSize * _multiplier +
                            entry.LastBytes];

            Array.Copy(tmp, 0, file, 0, file.Length);
        }

        node = new PascalFileNode
        {
            Path   = path,
            Length = file.Length,
            Offset = 0,
            _cache = file
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber CloseFile(IFileNode node)
    {
        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(node is not PascalFileNode mynode)
            return ErrorNumber.InvalidArgument;

        mynode._cache = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber ReadFile(IFileNode node, long length, byte[] buffer, out long read)
    {
        read = 0;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(buffer is null ||
           buffer.Length < length)
            return ErrorNumber.InvalidArgument;

        if(node is not PascalFileNode mynode)
            return ErrorNumber.InvalidArgument;

        read = length;

        if(length + mynode.Offset >= mynode.Length)
            read = mynode.Length - mynode.Offset;

        Array.Copy(mynode._cache, mynode.Offset, buffer, 0, read);
        mynode.Offset += read;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Stat(string path, out FileEntryInfo stat)
    {
        stat = null;

        string[] pathElements = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

        if(pathElements.Length != 1)
            return ErrorNumber.NotSupported;

        if(_debug)
        {
            if(string.Compare(path, "$",     StringComparison.InvariantCulture) == 0 ||
               string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0)
            {
                stat = new FileEntryInfo
                {
                    Attributes = FileAttributes.System,
                    BlockSize  = _device.Info.SectorSize * _multiplier,
                    Links      = 1
                };

                if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0)
                {
                    stat.Blocks = _catalogBlocks.Length / stat.BlockSize + _catalogBlocks.Length % stat.BlockSize;

                    stat.Length = _catalogBlocks.Length;
                }
                else
                {
                    stat.Blocks = _bootBlocks.Length / stat.BlockSize + _catalogBlocks.Length % stat.BlockSize;
                    stat.Length = _bootBlocks.Length;
                }

                return ErrorNumber.NoError;
            }
        }

        ErrorNumber error = GetFileEntry(path, out PascalFileEntry entry);

        if(error != ErrorNumber.NoError)
            return error;

        stat = new FileEntryInfo
        {
            Attributes = FileAttributes.File,
            Blocks = entry.LastBlock - entry.FirstBlock,
            BlockSize = _device.Info.SectorSize * _multiplier,
            LastWriteTimeUtc = DateHandlers.UcsdPascalToDateTime(entry.ModificationTime),
            Length = (entry.LastBlock - entry.FirstBlock) * _device.Info.SectorSize * _multiplier + entry.LastBytes,
            Links = 1
        };

        return ErrorNumber.NoError;
    }

#endregion

    ErrorNumber GetFileEntry(string path, out PascalFileEntry entry)
    {
        entry = new PascalFileEntry();

        foreach(PascalFileEntry ent in _fileEntries.Where(ent =>
                                                              string.Compare(path,
                                                                             StringHandlers.PascalToString(ent.Filename,
                                                                                 _encoding),
                                                                             StringComparison.
                                                                                 InvariantCultureIgnoreCase) == 0))
        {
            entry = ent;

            return ErrorNumber.NoError;
        }

        return ErrorNumber.NoSuchFile;
    }
}