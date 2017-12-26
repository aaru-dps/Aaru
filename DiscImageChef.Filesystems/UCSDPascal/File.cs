// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : U.C.S.D. Pascal filesystem plugin.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;

namespace DiscImageChef.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public partial class PascalPlugin
    {
        public Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return !mounted ? Errno.AccessDenied : Errno.NotImplemented;
        }

        public Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            Errno error = GetFileEntry(path, out _);

            if(error != Errno.NoError) return error;

            attributes = new FileAttributes();
            attributes = FileAttributes.File;

            return error;
        }

        public Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            byte[] file;

            if(debug && (string.Compare(path, "$", StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0))
                file = string.Compare(path, "$", StringComparison.InvariantCulture) == 0 ? catalogBlocks : bootBlocks;
            else
            {
                Errno error = GetFileEntry(path, out PascalFileEntry entry);

                if(error != Errno.NoError) return error;

                byte[] tmp = device.ReadSectors((ulong)entry.firstBlock, (uint)(entry.lastBlock - entry.firstBlock));
                file = new byte[(entry.lastBlock - entry.firstBlock - 1) * device.Info.SectorSize + entry.lastBytes];
                Array.Copy(tmp, 0, file, 0, file.Length);
            }

            if(offset >= file.Length) return Errno.EINVAL;

            if(size + offset >= file.Length) size = file.Length - offset;

            buf = new byte[size];

            Array.Copy(file, offset, buf, 0, size);

            return Errno.NoError;
        }

        public Errno Stat(string path, ref FileEntryInfo stat)
        {
            if(!mounted) return Errno.AccessDenied;

            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            if(debug)
                if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0 ||
                   string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0)
                {
                    stat = new FileEntryInfo
                    {
                        Attributes = FileAttributes.System,
                        BlockSize = device.Info.SectorSize,
                        DeviceNo = 0,
                        GID = 0,
                        Inode = 0,
                        Links = 1,
                        Mode = 0x124,
                        UID = 0
                    };

                    if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0)
                    {
                        stat.Blocks = catalogBlocks.Length / stat.BlockSize + catalogBlocks.Length % stat.BlockSize;
                        stat.Length = catalogBlocks.Length;
                    }
                    else
                    {
                        stat.Blocks = bootBlocks.Length / stat.BlockSize + catalogBlocks.Length % stat.BlockSize;
                        stat.Length = bootBlocks.Length;
                    }

                    return Errno.NoError;
                }

            Errno error = GetFileEntry(path, out PascalFileEntry entry);

            if(error != Errno.NoError) return error;

            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.File,
                Blocks = entry.lastBlock - entry.firstBlock,
                BlockSize = device.Info.SectorSize,
                DeviceNo = 0,
                GID = 0,
                Inode = 0,
                LastWriteTimeUtc = DateHandlers.UcsdPascalToDateTime(entry.mtime),
                Length = (entry.lastBlock - entry.firstBlock) * device.Info.SectorSize + entry.lastBytes,
                Links = 1,
                Mode = 0x124,
                UID = 0
            };

            return Errno.NoError;
        }

        Errno GetFileEntry(string path, out PascalFileEntry entry)
        {
            entry = new PascalFileEntry();

            foreach(PascalFileEntry ent in fileEntries.Where(ent =>
                                                                 string.Compare(path,
                                                                                StringHandlers
                                                                                    .PascalToString(ent.filename,
                                                                                                    Encoding),
                                                                                StringComparison
                                                                                    .InvariantCultureIgnoreCase) == 0))
            {
                entry = ent;
                return Errno.NoError;
            }

            return Errno.NoSuchFile;
        }
    }
}