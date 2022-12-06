// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Opera filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the Opera filesystem.
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

using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems
{
    public sealed partial class OperaFS
    {
        /// <inheritdoc />
        public Errno Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options, string @namespace)
        {
            // TODO: Find correct default encoding
            Encoding = Encoding.ASCII;

            options ??= GetDefaultOptions();

            if(options.TryGetValue("debug", out string debugString))
                bool.TryParse(debugString, out _debug);

            byte[] sbSector = imagePlugin.ReadSector(0 + partition.Start);

            SuperBlock sb = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sbSector);

            if(sb.record_type    != 1 ||
               sb.record_version != 1)
                return Errno.InvalidArgument;

            if(Encoding.ASCII.GetString(sb.sync_bytes) != SYNC)
                return Errno.InvalidArgument;

            if(imagePlugin.Info.SectorSize == 2336 ||
               imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448)
                _volumeBlockSizeRatio = sb.block_size / 2048;
            else
                _volumeBlockSizeRatio = sb.block_size / imagePlugin.Info.SectorSize;

            XmlFsType = new FileSystemType
            {
                Type         = "Opera",
                VolumeName   = StringHandlers.CToString(sb.volume_label, Encoding),
                ClusterSize  = sb.block_size,
                Clusters     = sb.block_count,
                Bootable     = true,
                VolumeSerial = $"{sb.volume_id:X8}"
            };

            _statfs = new FileSystemInfo
            {
                Blocks         = sb.block_count,
                FilenameLength = MAX_NAME,
                FreeBlocks     = 0,
                Id = new FileSystemId
                {
                    IsInt    = true,
                    Serial32 = sb.volume_id
                },
                PluginId = Id,
                Type     = "Opera"
            };

            _image = imagePlugin;
            int firstRootBlock = BigEndianBitConverter.ToInt32(sbSector, Marshal.SizeOf<SuperBlock>());
            _rootDirectoryCache = DecodeDirectory(firstRootBlock);
            _directoryCache     = new Dictionary<string, Dictionary<string, DirectoryEntryWithPointers>>();
            _mounted            = true;

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno Unmount()
        {
            if(!_mounted)
                return Errno.AccessDenied;

            _mounted = false;

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno StatFs(out FileSystemInfo stat)
        {
            stat = null;

            if(!_mounted)
                return Errno.AccessDenied;

            stat = _statfs.ShallowCopy();

            return Errno.NoError;
        }
    }
}