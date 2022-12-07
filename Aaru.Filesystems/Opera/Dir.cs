// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dir.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Opera filesystem plugin.
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.Helpers;

namespace Aaru.Filesystems;

public sealed partial class OperaFS
{
    /// <inheritdoc />
    public ErrorNumber ReadDir(string path, out List<string> contents)
    {
        contents = null;

        if(!_mounted)
            return ErrorNumber.AccessDenied;

        if(string.IsNullOrWhiteSpace(path) ||
           path == "/")
        {
            contents = _rootDirectoryCache.Keys.ToList();

            return ErrorNumber.NoError;
        }

        string cutPath = path.StartsWith("/", StringComparison.Ordinal)
                             ? path[1..].ToLower(CultureInfo.CurrentUICulture)
                             : path.ToLower(CultureInfo.CurrentUICulture);

        if(_directoryCache.TryGetValue(cutPath, out Dictionary<string, DirectoryEntryWithPointers> currentDirectory))
        {
            contents = currentDirectory.Keys.ToList();

            return ErrorNumber.NoError;
        }

        string[] pieces = cutPath.Split(new[]
        {
            '/'
        }, StringSplitOptions.RemoveEmptyEntries);

        KeyValuePair<string, DirectoryEntryWithPointers> entry =
            _rootDirectoryCache.FirstOrDefault(t => t.Key.ToLower(CultureInfo.CurrentUICulture) == pieces[0]);

        if(string.IsNullOrEmpty(entry.Key))
            return ErrorNumber.NoSuchFile;

        if((entry.Value.Entry.flags & FLAGS_MASK) != (int)FileFlags.Directory)
            return ErrorNumber.NotDirectory;

        string currentPath = pieces[0];

        currentDirectory = _rootDirectoryCache;

        for(int p = 0; p < pieces.Length; p++)
        {
            entry = currentDirectory.FirstOrDefault(t => t.Key.ToLower(CultureInfo.CurrentUICulture) == pieces[p]);

            if(string.IsNullOrEmpty(entry.Key))
                return ErrorNumber.NoSuchFile;

            if((entry.Value.Entry.flags & FLAGS_MASK) != (int)FileFlags.Directory)
                return ErrorNumber.NotDirectory;

            currentPath = p == 0 ? pieces[0] : $"{currentPath}/{pieces[p]}";

            if(_directoryCache.TryGetValue(currentPath, out currentDirectory))
                continue;

            if(entry.Value.Pointers.Length < 1)
                return ErrorNumber.InvalidArgument;

            currentDirectory = DecodeDirectory((int)entry.Value.Pointers[0]);

            _directoryCache.Add(currentPath, currentDirectory);
        }

        contents = currentDirectory?.Keys.ToList();

        return ErrorNumber.NoError;
    }

    Dictionary<string, DirectoryEntryWithPointers> DecodeDirectory(int firstBlock)
    {
        Dictionary<string, DirectoryEntryWithPointers> entries = new();

        int nextBlock = firstBlock;

        DirectoryHeader header;

        do
        {
            ErrorNumber errno = _image.ReadSectors((ulong)(nextBlock * _volumeBlockSizeRatio), _volumeBlockSizeRatio,
                                                   out byte[] data);

            if(errno != ErrorNumber.NoError)
                break;

            header    = Marshal.ByteArrayToStructureBigEndian<DirectoryHeader>(data);
            nextBlock = header.next_block + firstBlock;

            int off = (int)header.first_used;

            var entry = new DirectoryEntry();

            while(off + _directoryEntrySize < data.Length)
            {
                entry = Marshal.ByteArrayToStructureBigEndian<DirectoryEntry>(data, off, _directoryEntrySize);
                string name = StringHandlers.CToString(entry.name, Encoding);

                var entryWithPointers = new DirectoryEntryWithPointers
                {
                    Entry    = entry,
                    Pointers = new uint[entry.last_copy + 1]
                };

                for(int i = 0; i <= entry.last_copy; i++)
                    entryWithPointers.Pointers[i] =
                        BigEndianBitConverter.ToUInt32(data, off + _directoryEntrySize + (i * 4));

                entries.Add(name, entryWithPointers);

                if((entry.flags & (uint)FileFlags.LastEntry)        != 0 ||
                   (entry.flags & (uint)FileFlags.LastEntryInBlock) != 0)
                    break;

                off += (int)(_directoryEntrySize + ((entry.last_copy + 1) * 4));
            }

            if((entry.flags & (uint)FileFlags.LastEntry) != 0)
                break;
        } while(header.next_block != -1);

        return entries;
    }
}