// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;

namespace Aaru.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public sealed partial class PascalPlugin
    {
        /// <inheritdoc />
        public Errno MapBlock(string path, long fileBlock, out long deviceBlock)
        {
            deviceBlock = 0;

            return !_mounted ? Errno.AccessDenied : Errno.NotImplemented;
        }

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

            Errno error = GetFileEntry(path, out _);

            if(error != Errno.NoError)
                return error;

            attributes = FileAttributes.File;

            return error;
        }

        /// <inheritdoc />
        public Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            if(!_mounted)
                return Errno.AccessDenied;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            byte[] file;

            if(_debug && (string.Compare(path, "$", StringComparison.InvariantCulture)     == 0 ||
                          string.Compare(path, "$Boot", StringComparison.InvariantCulture) == 0))
                file = string.Compare(path, "$", StringComparison.InvariantCulture) == 0 ? _catalogBlocks : _bootBlocks;
            else
            {
                Errno error = GetFileEntry(path, out PascalFileEntry entry);

                if(error != Errno.NoError)
                    return error;

                byte[] tmp = _device.ReadSectors((ulong)entry.FirstBlock                    * _multiplier,
                                                 (uint)(entry.LastBlock - entry.FirstBlock) * _multiplier);

                file = new byte[((entry.LastBlock - entry.FirstBlock - 1) * _device.Info.SectorSize * _multiplier) +
                                entry.LastBytes];

                Array.Copy(tmp, 0, file, 0, file.Length);
            }

            if(offset >= file.Length)
                return Errno.EINVAL;

            if(size + offset >= file.Length)
                size = file.Length - offset;

            buf = new byte[size];

            Array.Copy(file, offset, buf, 0, size);

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno Stat(string path, out FileEntryInfo stat)
        {
            stat = null;

            string[] pathElements = path.Split(new[]
            {
                '/'
            }, StringSplitOptions.RemoveEmptyEntries);

            if(pathElements.Length != 1)
                return Errno.NotSupported;

            if(_debug)
                if(string.Compare(path, "$", StringComparison.InvariantCulture)     == 0 ||
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
                        stat.Blocks = (_catalogBlocks.Length / stat.BlockSize) +
                                      (_catalogBlocks.Length % stat.BlockSize);

                        stat.Length = _catalogBlocks.Length;
                    }
                    else
                    {
                        stat.Blocks = (_bootBlocks.Length / stat.BlockSize) + (_catalogBlocks.Length % stat.BlockSize);
                        stat.Length = _bootBlocks.Length;
                    }

                    return Errno.NoError;
                }

            Errno error = GetFileEntry(path, out PascalFileEntry entry);

            if(error != Errno.NoError)
                return error;

            stat = new FileEntryInfo
            {
                Attributes       = FileAttributes.File,
                Blocks           = entry.LastBlock - entry.FirstBlock,
                BlockSize        = _device.Info.SectorSize * _multiplier,
                LastWriteTimeUtc = DateHandlers.UcsdPascalToDateTime(entry.ModificationTime),
                Length = ((entry.LastBlock - entry.FirstBlock) * _device.Info.SectorSize * _multiplier) +
                         entry.LastBytes,
                Links = 1
            };

            return Errno.NoError;
        }

        Errno GetFileEntry(string path, out PascalFileEntry entry)
        {
            entry = new PascalFileEntry();

            foreach(PascalFileEntry ent in _fileEntries.Where(ent =>
                                                                  string.Compare(path,
                                                                      StringHandlers.PascalToString(ent.Filename,
                                                                          Encoding),
                                                                      StringComparison.
                                                                          InvariantCultureIgnoreCase) == 0))
            {
                entry = ent;

                return Errno.NoError;
            }

            return Errno.NoSuchFile;
        }
    }
}