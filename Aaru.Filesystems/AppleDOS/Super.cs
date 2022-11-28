// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple DOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the Apple DOS filesystem.
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

using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Helpers;
using Claunia.Encoding;
using Schemas;
using Encoding = System.Text.Encoding;

namespace Aaru.Filesystems;

public sealed partial class AppleDOS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options, string @namespace)
    {
        _device  = imagePlugin;
        _start   = partition.Start;
        Encoding = encoding ?? new Apple2();

        if(_device.Info.Sectors != 455 &&
           _device.Info.Sectors != 560)
        {
            AaruConsole.DebugWriteLine("Apple DOS plugin", Localization.Incorrect_device_size);

            return ErrorNumber.InOutError;
        }

        if(_start > 0)
        {
            AaruConsole.DebugWriteLine("Apple DOS plugin", Localization.Partitions_are_not_supported);

            return ErrorNumber.InOutError;
        }

        if(_device.Info.SectorSize != 256)
        {
            AaruConsole.DebugWriteLine("Apple DOS plugin", Localization.Incorrect_sector_size);

            return ErrorNumber.InOutError;
        }

        _sectorsPerTrack = _device.Info.Sectors == 455 ? 13 : 16;

        // Read the VTOC
        ErrorNumber error = _device.ReadSector((ulong)(17 * _sectorsPerTrack), out _vtocBlocks);

        if(error != ErrorNumber.NoError)
            return error;

        _vtoc = Marshal.ByteArrayToStructureLittleEndian<Vtoc>(_vtocBlocks);

        _track1UsedByFiles = false;
        _track2UsedByFiles = false;
        _usedSectors       = 1;

        error = ReadCatalog();

        if(error != ErrorNumber.NoError)
        {
            AaruConsole.DebugWriteLine("Apple DOS plugin", Localization.Unable_to_read_catalog);

            return error;
        }

        error = CacheAllFiles();

        if(error != ErrorNumber.NoError)
        {
            AaruConsole.DebugWriteLine("Apple DOS plugin", Localization.Unable_cache_all_files);

            return error;
        }

        // Create XML metadata for mounted filesystem
        XmlFsType = new FileSystemType
        {
            Bootable              = true,
            Clusters              = _device.Info.Sectors,
            ClusterSize           = _vtoc.bytesPerSector,
            Files                 = (ulong)_catalogCache.Count,
            FilesSpecified        = true,
            FreeClustersSpecified = true,
            Type                  = FS_TYPE
        };

        XmlFsType.FreeClusters = XmlFsType.Clusters - _usedSectors;

        options ??= GetDefaultOptions();

        if(options.TryGetValue("debug", out string debugString))
            bool.TryParse(debugString, out _debug);

        _mounted = true;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        _mounted       = false;
        _extentCache   = null;
        _fileCache     = null;
        _catalogCache  = null;
        _fileSizeCache = null;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber StatFs(out FileSystemInfo stat)
    {
        stat = new FileSystemInfo
        {
            Blocks         = _device.Info.Sectors,
            FilenameLength = 30,
            Files          = (ulong)_catalogCache.Count,
            PluginId       = Id,
            Type           = FS_TYPE
        };

        stat.FreeFiles  = _totalFileEntries - stat.Files;
        stat.FreeBlocks = stat.Blocks       - _usedSectors;

        return ErrorNumber.NoError;
    }
}