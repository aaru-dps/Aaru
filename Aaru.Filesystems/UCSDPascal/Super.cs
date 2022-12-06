// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : U.C.S.D. Pascal filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the U.C.S.D. Pascal filesystem.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Claunia.Encoding;
using Schemas;
using Encoding = System.Text.Encoding;

namespace Aaru.Filesystems.UCSDPascal
{
    // Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
    public sealed partial class PascalPlugin
    {
        /// <inheritdoc />
        public Errno Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options, string @namespace)
        {
            _device  = imagePlugin;
            Encoding = encoding ?? new Apple2();

            options ??= GetDefaultOptions();

            if(options.TryGetValue("debug", out string debugString))
                bool.TryParse(debugString, out _debug);

            if(_device.Info.Sectors < 3)
                return Errno.InvalidArgument;

            _multiplier = (uint)(imagePlugin.Info.SectorSize == 256 ? 2 : 1);

            // Blocks 0 and 1 are boot code
            _catalogBlocks = _device.ReadSectors(_multiplier * 2, _multiplier);

            // On Apple //, it's little endian
            // TODO: Fix
            //BigEndianBitConverter.IsLittleEndian =
            //    multiplier == 2 ? !BitConverter.IsLittleEndian : BitConverter.IsLittleEndian;

            _mountedVolEntry.FirstBlock = BigEndianBitConverter.ToInt16(_catalogBlocks, 0x00);
            _mountedVolEntry.LastBlock  = BigEndianBitConverter.ToInt16(_catalogBlocks, 0x02);
            _mountedVolEntry.EntryType  = (PascalFileKind)BigEndianBitConverter.ToInt16(_catalogBlocks, 0x04);
            _mountedVolEntry.VolumeName = new byte[8];
            Array.Copy(_catalogBlocks, 0x06, _mountedVolEntry.VolumeName, 0, 8);
            _mountedVolEntry.Blocks   = BigEndianBitConverter.ToInt16(_catalogBlocks, 0x0E);
            _mountedVolEntry.Files    = BigEndianBitConverter.ToInt16(_catalogBlocks, 0x10);
            _mountedVolEntry.Dummy    = BigEndianBitConverter.ToInt16(_catalogBlocks, 0x12);
            _mountedVolEntry.LastBoot = BigEndianBitConverter.ToInt16(_catalogBlocks, 0x14);
            _mountedVolEntry.Tail     = BigEndianBitConverter.ToInt32(_catalogBlocks, 0x16);

            if(_mountedVolEntry.FirstBlock       != 0                                       ||
               _mountedVolEntry.LastBlock        <= _mountedVolEntry.FirstBlock             ||
               (ulong)_mountedVolEntry.LastBlock > (_device.Info.Sectors / _multiplier) - 2 ||
               (_mountedVolEntry.EntryType != PascalFileKind.Volume &&
                _mountedVolEntry.EntryType != PascalFileKind.Secure)                ||
               _mountedVolEntry.VolumeName[0] > 7                                   ||
               _mountedVolEntry.Blocks        < 0                                   ||
               (ulong)_mountedVolEntry.Blocks != _device.Info.Sectors / _multiplier ||
               _mountedVolEntry.Files         < 0)
                return Errno.InvalidArgument;

            _catalogBlocks = _device.ReadSectors(_multiplier * 2,
                                                 (uint)(_mountedVolEntry.LastBlock - _mountedVolEntry.FirstBlock - 2) *
                                                 _multiplier);

            int offset = 26;

            _fileEntries = new List<PascalFileEntry>();

            while(offset + 26 < _catalogBlocks.Length)
            {
                var entry = new PascalFileEntry
                {
                    Filename         = new byte[16],
                    FirstBlock       = BigEndianBitConverter.ToInt16(_catalogBlocks, offset + 0x00),
                    LastBlock        = BigEndianBitConverter.ToInt16(_catalogBlocks, offset + 0x02),
                    EntryType        = (PascalFileKind)BigEndianBitConverter.ToInt16(_catalogBlocks, offset + 0x04),
                    LastBytes        = BigEndianBitConverter.ToInt16(_catalogBlocks, offset + 0x16),
                    ModificationTime = BigEndianBitConverter.ToInt16(_catalogBlocks, offset + 0x18)
                };

                Array.Copy(_catalogBlocks, offset + 0x06, entry.Filename, 0, 16);

                if(entry.Filename[0] <= 15 &&
                   entry.Filename[0] > 0)
                    _fileEntries.Add(entry);

                offset += 26;
            }

            _bootBlocks = _device.ReadSectors(0, 2 * _multiplier);

            XmlFsType = new FileSystemType
            {
                Bootable       = !ArrayHelpers.ArrayIsNullOrEmpty(_bootBlocks),
                Clusters       = (ulong)_mountedVolEntry.Blocks,
                ClusterSize    = _device.Info.SectorSize,
                Files          = (ulong)_mountedVolEntry.Files,
                FilesSpecified = true,
                Type           = "UCSD Pascal",
                VolumeName     = StringHandlers.PascalToString(_mountedVolEntry.VolumeName, Encoding)
            };

            _mounted = true;

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno Unmount()
        {
            _mounted     = false;
            _fileEntries = null;

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno StatFs(out FileSystemInfo stat)
        {
            stat = new FileSystemInfo
            {
                Blocks         = (ulong)_mountedVolEntry.Blocks,
                FilenameLength = 16,
                Files          = (ulong)_mountedVolEntry.Files,
                FreeBlocks     = 0,
                PluginId       = Id,
                Type           = "UCSD Pascal"
            };

            stat.FreeBlocks =
                (ulong)(_mountedVolEntry.Blocks - (_mountedVolEntry.LastBlock - _mountedVolEntry.FirstBlock));

            foreach(PascalFileEntry entry in _fileEntries)
                stat.FreeBlocks -= (ulong)(entry.LastBlock - entry.FirstBlock);

            return Errno.NotImplemented;
        }
    }
}