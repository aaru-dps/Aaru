// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Opera filesystem plugin.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

public sealed partial class OperaFS
{
#region IReadOnlyFilesystem Members

    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        // TODO: Find correct default encoding
        _encoding = Encoding.ASCII;

        options ??= GetDefaultOptions();

        if(options.TryGetValue("debug", out string debugString)) bool.TryParse(debugString, out _debug);

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] sbSector);

        if(errno != ErrorNumber.NoError) return errno;

        SuperBlock sb = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sbSector);

        if(sb.record_type != 1 || sb.record_version != 1) return ErrorNumber.InvalidArgument;

        if(Encoding.ASCII.GetString(sb.sync_bytes) != SYNC) return ErrorNumber.InvalidArgument;

        if(imagePlugin.Info.SectorSize is 2336 or 2352 or 2448)
            _volumeBlockSizeRatio = sb.block_size / 2048;
        else
            _volumeBlockSizeRatio = sb.block_size / imagePlugin.Info.SectorSize;

        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            VolumeName   = StringHandlers.CToString(sb.volume_label, _encoding),
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
            Type     = FS_TYPE
        };

        _image = imagePlugin;
        var firstRootBlock = BigEndianBitConverter.ToInt32(sbSector, Marshal.SizeOf<SuperBlock>());
        _rootDirectoryCache = DecodeDirectory(firstRootBlock);
        _directoryCache     = new Dictionary<string, Dictionary<string, DirectoryEntryWithPointers>>();
        _mounted            = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _mounted = false;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber StatFs(out FileSystemInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        stat = _statfs.ShallowCopy();

        return ErrorNumber.NoError;
    }

#endregion
}