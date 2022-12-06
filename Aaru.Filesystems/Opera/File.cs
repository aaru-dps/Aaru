// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Opera filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods to handle files.
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
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems
{
    public sealed partial class OperaFS
    {
        /// <inheritdoc />
        public Errno MapBlock(string path, long fileBlock, out long deviceBlock)
        {
            deviceBlock = 0;

            if(!_mounted)
                return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DirectoryEntryWithPointers entry);

            if(err != Errno.NoError)
                return err;

            if((entry.Entry.flags & FLAGS_MASK) == (uint)FileFlags.Directory &&
               !_debug)
                return Errno.IsDirectory;

            deviceBlock = entry.Pointers[0] + fileBlock;

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno GetAttributes(string path, out FileAttributes attributes)
        {
            attributes = new FileAttributes();

            if(!_mounted)
                return Errno.AccessDenied;

            Errno err = Stat(path, out FileEntryInfo stat);

            if(err != Errno.NoError)
                return err;

            attributes = stat.Attributes;

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            buf = null;

            if(!_mounted)
                return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DirectoryEntryWithPointers entry);

            if(err != Errno.NoError)
                return err;

            if((entry.Entry.flags & FLAGS_MASK) == (uint)FileFlags.Directory &&
               !_debug)
                return Errno.IsDirectory;

            if(entry.Pointers.Length < 1)
                return Errno.InvalidArgument;

            if(entry.Entry.byte_count == 0)
            {
                buf = Array.Empty<byte>();

                return Errno.NoError;
            }

            if(offset >= entry.Entry.byte_count)
                return Errno.InvalidArgument;

            if(size + offset >= entry.Entry.byte_count)
                size = entry.Entry.byte_count - offset;

            long firstBlock    = offset                 / entry.Entry.block_size;
            long offsetInBlock = offset                 % entry.Entry.block_size;
            long sizeInBlocks  = (size + offsetInBlock) / entry.Entry.block_size;

            if((size + offsetInBlock) % entry.Entry.block_size > 0)
                sizeInBlocks++;

            uint fileBlockSizeRatio;

            if(_image.Info.SectorSize == 2336 ||
               _image.Info.SectorSize == 2352 ||
               _image.Info.SectorSize == 2448)
                fileBlockSizeRatio = entry.Entry.block_size / 2048;
            else
                fileBlockSizeRatio = entry.Entry.block_size / _image.Info.SectorSize;

            byte[] buffer = _image.ReadSectors((ulong)(entry.Pointers[0] + (firstBlock * fileBlockSizeRatio)),
                                               (uint)(sizeInBlocks * fileBlockSizeRatio));

            buf = new byte[size];
            Array.Copy(buffer, offsetInBlock, buf, 0, size);

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno Stat(string path, out FileEntryInfo stat)
        {
            stat = null;

            if(!_mounted)
                return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DirectoryEntryWithPointers entryWithPointers);

            if(err != Errno.NoError)
                return err;

            DirectoryEntry entry = entryWithPointers.Entry;

            stat = new FileEntryInfo
            {
                Attributes = new FileAttributes(),
                Blocks     = entry.block_count,
                BlockSize  = entry.block_size,
                Length     = entry.byte_count,
                Inode      = entry.id,
                Links      = (ulong)entryWithPointers.Pointers.Length
            };

            var flags = (FileFlags)(entry.flags & FLAGS_MASK);

            if(flags == FileFlags.Directory)
                stat.Attributes |= FileAttributes.Directory;

            if(flags == FileFlags.Special)
                stat.Attributes |= FileAttributes.Device;

            return Errno.NoError;
        }

        Errno GetFileEntry(string path, out DirectoryEntryWithPointers entry)
        {
            entry = null;

            string cutPath = path.StartsWith("/", StringComparison.Ordinal)
                                 ? path.Substring(1).ToLower(CultureInfo.CurrentUICulture)
                                 : path.ToLower(CultureInfo.CurrentUICulture);

            string[] pieces = cutPath.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pieces.Length == 0)
                return Errno.InvalidArgument;

            string parentPath = string.Join("/", pieces, 0, pieces.Length - 1);

            if(!_directoryCache.TryGetValue(parentPath, out _))
            {
                Errno err = ReadDir(parentPath, out _);

                if(err != Errno.NoError)
                    return err;
            }

            Dictionary<string, DirectoryEntryWithPointers> parent;

            if(pieces.Length == 1)
                parent = _rootDirectoryCache;
            else if(!_directoryCache.TryGetValue(parentPath, out parent))
                return Errno.InvalidArgument;

            KeyValuePair<string, DirectoryEntryWithPointers> dirent =
                parent.FirstOrDefault(t => t.Key.ToLower(CultureInfo.CurrentUICulture) == pieces[^1]);

            if(string.IsNullOrEmpty(dirent.Key))
                return Errno.NoSuchFile;

            entry = dirent.Value;

            return Errno.NoError;
        }
    }
}