using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.Helpers;
using FileAttributes = DiscImageChef.CommonTypes.Structs.FileAttributes;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        public Errno MapBlock(string path, long fileBlock, out long deviceBlock)
        {
            deviceBlock = 0;
            if(!mounted) return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DecodedDirectoryEntry entry);
            if(err != Errno.NoError) return err;

            if(entry.Flags.HasFlag(FileFlags.Directory) && !debug) return Errno.IsDirectory;

            deviceBlock = entry.Extent + fileBlock;

            return Errno.NoError;
        }

        public Errno GetAttributes(string path, out FileAttributes attributes)
        {
            attributes = new FileAttributes();
            if(!mounted) return Errno.AccessDenied;

            Errno err = Stat(path, out FileEntryInfo stat);

            if(err != Errno.NoError) return err;

            attributes = stat.Attributes;

            return Errno.NoError;
        }

        public Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            buf = null;
            if(!mounted) return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DecodedDirectoryEntry entry);
            if(err != Errno.NoError) return err;

            if(entry.Flags.HasFlag(FileFlags.Directory) && !debug) return Errno.IsDirectory;

            if(offset >= entry.Size) return Errno.InvalidArgument;

            if(size + offset >= entry.Size) size = entry.Size - offset;

            if(entry.Size == 0)
            {
                buf = new byte[0];
                return Errno.NoError;
            }

            // TODO: XA
            long firstSector    = offset                  / 2048;
            long offsetInSector = offset                  % 2048;
            long sizeInSectors  = (size + offsetInSector) / 2048;
            if((size + offsetInSector) % 2048 > 0) sizeInSectors++;

            MemoryStream ms = new MemoryStream();

            byte[] buffer = image.ReadSectors((ulong)(entry.Extent + firstSector), (uint)sizeInSectors);
            buf = new byte[size];
            Array.Copy(buffer, offsetInSector, buf, 0, size);

            return Errno.NoError;
        }

        public Errno Stat(string path, out FileEntryInfo stat)
        {
            stat = null;
            if(!mounted) return Errno.AccessDenied;

            Errno err = GetFileEntry(path, out DecodedDirectoryEntry entry);
            if(err != Errno.NoError) return err;

            stat = new FileEntryInfo
            {
                Attributes       = new FileAttributes(),
                Blocks           = entry.Size / 2048, // TODO: XA
                BlockSize        = 2048,
                Length           = entry.Size,
                Inode            = entry.Extent,
                Links            = 1,
                LastWriteTimeUtc = entry.Timestamp
            };

            if(entry.Size % 2048 > 0) stat.Blocks++;

            if(entry.Flags.HasFlag(FileFlags.Directory)) stat.Attributes |= FileAttributes.Directory;
            if(entry.Flags.HasFlag(FileFlags.Hidden)) stat.Attributes    |= FileAttributes.Hidden;

            if(entry.AssociatedFile is null || entry.AssociatedFile.Extent == 0 || entry.AssociatedFile.Size == 0)
                return Errno.NoError;

            // TODO: XA
            uint eaSizeInSectors = entry.AssociatedFile.Size / 2048;
            if(entry.AssociatedFile.Size % 2048 > 0) eaSizeInSectors++;

            byte[] ea = image.ReadSectors(entry.AssociatedFile.Extent, eaSizeInSectors);

            ExtendedAttributeRecord ear = Marshal.ByteArrayToStructureLittleEndian<ExtendedAttributeRecord>(ea);

            stat.UID = ear.owner;
            stat.GID = ear.group;

            stat.Mode = 0;
            if(ear.permissions.HasFlag(Permissions.GroupExecute)) stat.Mode += 8;
            if(ear.permissions.HasFlag(Permissions.GroupRead)) stat.Mode    += 32;
            if(ear.permissions.HasFlag(Permissions.OwnerExecute)) stat.Mode += 64;
            if(ear.permissions.HasFlag(Permissions.OwnerRead)) stat.Mode    += 256;
            if(ear.permissions.HasFlag(Permissions.OtherExecute)) stat.Mode += 1;
            if(ear.permissions.HasFlag(Permissions.OtherRead)) stat.Mode    += 4;

            stat.CreationTimeUtc  = DateHandlers.Iso9660ToDateTime(ear.creation_date);
            stat.LastWriteTimeUtc = DateHandlers.Iso9660ToDateTime(ear.modification_date);

            return Errno.NoError;
        }

        Errno GetFileEntry(string path, out DecodedDirectoryEntry entry)
        {
            entry = null;

            string cutPath = path.StartsWith("/")
                                 ? path.Substring(1).ToLower(CultureInfo.CurrentUICulture)
                                 : path.ToLower(CultureInfo.CurrentUICulture);
            string[] pieces = cutPath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            if(pieces.Length == 0) return Errno.InvalidArgument;

            string parentPath = string.Join("/", pieces, 0, pieces.Length - 1);

            if(!directoryCache.TryGetValue(parentPath, out _))
            {
                Errno err = ReadDir(parentPath, out _);

                if(err != Errno.NoError) return err;
            }

            Dictionary<string, DecodedDirectoryEntry> parent;

            if(pieces.Length == 1) parent = rootDirectoryCache;
            else if(!directoryCache.TryGetValue(parentPath, out parent)) return Errno.InvalidArgument;

            KeyValuePair<string, DecodedDirectoryEntry> dirent =
                parent.FirstOrDefault(t => t.Key.ToLower(CultureInfo.CurrentUICulture) == pieces[pieces.Length - 1]);

            if(string.IsNullOrEmpty(dirent.Key))
            {
                // TODO: RRIP
                if(!joliet && !pieces[pieces.Length - 1].EndsWith(";1", StringComparison.Ordinal))
                {
                    dirent = parent.FirstOrDefault(t => t.Key.ToLower(CultureInfo.CurrentUICulture) ==
                                                        pieces[pieces.Length - 1] + ";1");

                    if(string.IsNullOrEmpty(dirent.Key)) return Errno.NoSuchFile;
                }
                else return Errno.NoSuchFile;
            }

            entry = dirent.Value;
            return Errno.NoError;
        }
    }
}