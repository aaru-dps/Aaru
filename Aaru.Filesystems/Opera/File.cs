using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Aaru.CommonTypes.Structs;

namespace Aaru.Filesystems
{
    public partial class OperaFS
    {
        public Errno MapBlock(string path, long fileBlock, out long deviceBlock)
        {
            deviceBlock = 0;

            if(!mounted)
                return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DirectoryEntryWithPointers entry);

            if(err != Errno.NoError)
                return err;

            if((entry.entry.flags & FLAGS_MASK) == (uint)FileFlags.Directory &&
               !debug)
                return Errno.IsDirectory;

            deviceBlock = entry.pointers[0] + fileBlock;

            return Errno.NoError;
        }

        public Errno GetAttributes(string path, out FileAttributes attributes)
        {
            attributes = new FileAttributes();

            if(!mounted)
                return Errno.AccessDenied;

            Errno err = Stat(path, out FileEntryInfo stat);

            if(err != Errno.NoError)
                return err;

            attributes = stat.Attributes;

            return Errno.NoError;
        }

        public Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            buf = null;

            if(!mounted)
                return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DirectoryEntryWithPointers entry);

            if(err != Errno.NoError)
                return err;

            if((entry.entry.flags & FLAGS_MASK) == (uint)FileFlags.Directory &&
               !debug)
                return Errno.IsDirectory;

            if(entry.pointers.Length < 1)
                return Errno.InvalidArgument;

            if(entry.entry.byte_count == 0)
            {
                buf = new byte[0];

                return Errno.NoError;
            }

            if(offset >= entry.entry.byte_count)
                return Errno.InvalidArgument;

            if(size + offset >= entry.entry.byte_count)
                size = entry.entry.byte_count - offset;

            long firstBlock    = offset                 / entry.entry.block_size;
            long offsetInBlock = offset                 % entry.entry.block_size;
            long sizeInBlocks  = (size + offsetInBlock) / entry.entry.block_size;

            if((size + offsetInBlock) % entry.entry.block_size > 0)
                sizeInBlocks++;

            uint fileBlockSizeRatio;

            if(image.Info.SectorSize == 2336 ||
               image.Info.SectorSize == 2352 ||
               image.Info.SectorSize == 2448)
                fileBlockSizeRatio = entry.entry.block_size / 2048;
            else
                fileBlockSizeRatio = entry.entry.block_size / image.Info.SectorSize;

            byte[] buffer = image.ReadSectors((ulong)(entry.pointers[0] + (firstBlock * fileBlockSizeRatio)),
                                              (uint)(sizeInBlocks * fileBlockSizeRatio));

            buf = new byte[size];
            Array.Copy(buffer, offsetInBlock, buf, 0, size);

            return Errno.NoError;
        }

        public Errno Stat(string path, out FileEntryInfo stat)
        {
            stat = null;

            if(!mounted)
                return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DirectoryEntryWithPointers entryWithPointers);

            if(err != Errno.NoError)
                return err;

            DirectoryEntry entry = entryWithPointers.entry;

            stat = new FileEntryInfo
            {
                Attributes = new FileAttributes(), Blocks = entry.block_count, BlockSize = entry.block_size,
                Length     = entry.byte_count, Inode      = entry.id,
                Links      = (ulong)entryWithPointers.pointers.Length
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

            if(!directoryCache.TryGetValue(parentPath, out _))
            {
                Errno err = ReadDir(parentPath, out _);

                if(err != Errno.NoError)
                    return err;
            }

            Dictionary<string, DirectoryEntryWithPointers> parent;

            if(pieces.Length == 1)
                parent = rootDirectoryCache;
            else if(!directoryCache.TryGetValue(parentPath, out parent))
                return Errno.InvalidArgument;

            KeyValuePair<string, DirectoryEntryWithPointers> dirent =
                parent.FirstOrDefault(t => t.Key.ToLower(CultureInfo.CurrentUICulture) == pieces[pieces.Length - 1]);

            if(string.IsNullOrEmpty(dirent.Key))
                return Errno.NoSuchFile;

            entry = dirent.Value;

            return Errno.NoError;
        }
    }
}