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

namespace DiscImageChef.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public partial class PascalPlugin : Filesystem
    {
        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            if(!mounted) return Errno.AccessDenied;

            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            PascalFileEntry entry;
            Errno error = GetFileEntry(path, out entry);

            if(error == Errno.NoError)
            {
                attributes = new FileAttributes();
                attributes = FileAttributes.File;
            }

            return error;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            byte[] file;
            Errno error;

            if(debug && (string.Compare(path, "$", StringComparison.InvariantCulture) == 0 ||
                         string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0))
                if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0) file = catalogBlocks;
                else file = bootBlocks;
            else
            {
                PascalFileEntry entry;
                error = GetFileEntry(path, out entry);

                if(error != Errno.NoError) return error;

                byte[] tmp = device.ReadSectors((ulong)entry.firstBlock, (uint)(entry.lastBlock - entry.firstBlock));
                file = new byte[(entry.lastBlock - entry.firstBlock - 1) * device.GetSectorSize() + entry.lastBytes];
                Array.Copy(tmp, 0, file, 0, file.Length);
            }

            if(offset >= file.Length) return Errno.EINVAL;

            if(size + offset >= file.Length) size = file.Length - offset;

            buf = new byte[size];

            Array.Copy(file, offset, buf, 0, size);

            return Errno.NoError;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            if(!mounted) return Errno.AccessDenied;

            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            if(debug)
                if(string.Compare(path, "$", StringComparison.InvariantCulture) == 0 ||
                   string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0)
                {
                    stat = new FileEntryInfo();
                    stat.Attributes = new FileAttributes();
                    stat.Attributes = FileAttributes.System;
                    stat.BlockSize = device.GetSectorSize();
                    stat.DeviceNo = 0;
                    stat.GID = 0;
                    stat.Inode = 0;
                    stat.Links = 1;
                    stat.Mode = 0x124;
                    stat.UID = 0;

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

            PascalFileEntry entry;
            Errno error = GetFileEntry(path, out entry);

            if(error != Errno.NoError) return error;

            stat = new FileEntryInfo();
            stat.Attributes = new FileAttributes();
            stat.Attributes = FileAttributes.File;
            stat.Blocks = entry.lastBlock - entry.firstBlock;
            stat.BlockSize = device.GetSectorSize();
            stat.DeviceNo = 0;
            stat.GID = 0;
            stat.Inode = 0;
            stat.LastWriteTimeUtc = DateHandlers.UCSDPascalToDateTime(entry.mtime);
            stat.Length = (entry.lastBlock - entry.firstBlock) * device.GetSectorSize() + entry.lastBytes;
            stat.Links = 1;
            stat.Mode = 0x124;
            stat.UID = 0;

            return Errno.NoError;
        }

        Errno GetFileEntry(string path, out PascalFileEntry entry)
        {
            entry = new PascalFileEntry();

            foreach(PascalFileEntry ent in fileEntries)
                if(string.Compare(path, StringHandlers.PascalToString(ent.filename, CurrentEncoding),
                                  StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    entry = ent;
                    return Errno.NoError;
                }

            return Errno.NoSuchFile;
        }
    }
}