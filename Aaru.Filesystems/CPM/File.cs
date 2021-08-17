// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Filesystems
{
    public sealed partial class CPM
    {
        /// <inheritdoc />
        public Errno GetAttributes(string path, out FileAttributes attributes)
        {
            attributes = new FileAttributes();

            if(!_mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            if(string.IsNullOrEmpty(pathElements[0]) ||
               string.Compare(pathElements[0], "/", StringComparison.OrdinalIgnoreCase) == 0)
            {
                attributes = new FileAttributes();
                attributes = FileAttributes.Directory;

                return Errno.NoError;
            }

            if(!_statCache.TryGetValue(pathElements[0].ToUpperInvariant(), out FileEntryInfo fInfo))
                return Errno.NoSuchFile;

            attributes = fInfo.Attributes;

            return Errno.NoError;
        }

        // TODO: Implementing this would require storing the interleaving
        /// <inheritdoc />
        public Errno MapBlock(string path, long fileBlock, out long deviceBlock)
        {
            deviceBlock = 0;

            return !_mounted ? Errno.AccessDenied : Errno.NotImplemented;
        }

        /// <inheritdoc />
        public Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            if(!_mounted)
                return Errno.AccessDenied;

            if(size == 0)
            {
                buf = new byte[0];

                return Errno.NoError;
            }

            if(offset < 0)
                return Errno.InvalidArgument;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            if(!_fileCache.TryGetValue(pathElements[0].ToUpperInvariant(), out byte[] file))
                return Errno.NoSuchFile;

            if(offset >= file.Length)
                return Errno.EINVAL;

            if(size + offset >= file.Length)
                size = file.Length - offset;

            buf = new byte[size];
            Array.Copy(file, offset, buf, 0, size);

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno ReadLink(string path, out string dest)
        {
            dest = null;

            return !_mounted ? Errno.AccessDenied : Errno.NotSupported;
        }

        /// <inheritdoc />
        public Errno Stat(string path, out FileEntryInfo stat)
        {
            stat = null;

            if(!_mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            if(!string.IsNullOrEmpty(path) &&
               string.Compare(path, "/", StringComparison.OrdinalIgnoreCase) != 0)
                return _statCache.TryGetValue(pathElements[0].ToUpperInvariant(), out stat) ? Errno.NoError
                           : Errno.NoSuchFile;

            stat = new FileEntryInfo
            {
                Attributes = FileAttributes.Directory,
                BlockSize  = XmlFsType.ClusterSize
            };

            if(_labelCreationDate != null)
                stat.CreationTime = DateHandlers.CpmToDateTime(_labelCreationDate);

            if(_labelUpdateDate != null)
                stat.StatusChangeTime = DateHandlers.CpmToDateTime(_labelUpdateDate);

            return Errno.NoError;
        }
    }
}