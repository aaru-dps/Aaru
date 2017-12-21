// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : File.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CP/M filesystem plugin.
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

namespace DiscImageChef.Filesystems.CPM
{
    partial class CPM : Filesystem
    {
        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            FileEntryInfo fInfo;

            if(string.IsNullOrEmpty(pathElements[0]) ||
               string.Compare(pathElements[0], "/", StringComparison.OrdinalIgnoreCase) == 0)
            {
                attributes = new FileAttributes();
                attributes = FileAttributes.Directory;
                return Errno.NoError;
            }

            if(!statCache.TryGetValue(pathElements[0].ToUpperInvariant(), out fInfo)) return Errno.NoSuchFile;

            attributes = fInfo.Attributes;
            return Errno.NoError;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            if(!mounted) return Errno.AccessDenied;

            // TODO: Implementing this would require storing the interleaving
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            if(!mounted) return Errno.AccessDenied;

            if(size == 0)
            {
                buf = new byte[0];
                return Errno.NoError;
            }

            if(offset < 0) return Errno.InvalidArgument;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            byte[] file;

            if(!fileCache.TryGetValue(pathElements[0].ToUpperInvariant(), out file)) return Errno.NoSuchFile;

            if(offset >= file.Length) return Errno.EINVAL;

            if(size + offset >= file.Length) size = file.Length - offset;

            buf = new byte[size];
            Array.Copy(file, offset, buf, 0, size);
            return Errno.NoError;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            if(!mounted) return Errno.AccessDenied;

            return Errno.NotSupported;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            if(!mounted) return Errno.AccessDenied;

            string[] pathElements = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if(pathElements.Length != 1) return Errno.NotSupported;

            if(string.IsNullOrEmpty(path) || string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if(labelCreationDate != null) stat.CreationTime = DateHandlers.CPMToDateTime(labelCreationDate);
                if(labelUpdateDate != null) stat.StatusChangeTime = DateHandlers.CPMToDateTime(labelUpdateDate);
                stat.Attributes = FileAttributes.Directory;
                stat.BlockSize = xmlFSType.ClusterSize;
                return Errno.NoError;
            }

            if(statCache.TryGetValue(pathElements[0].ToUpperInvariant(), out stat)) return Errno.NoError;

            return Errno.NoSuchFile;
        }
    }
}