// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the Apple Lisa filesystem.
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
using System.Collections.Generic;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Console;
using Aaru.Decoders;
using Aaru.Helpers;
using Claunia.Encoding;
using Schemas;
using Encoding = System.Text.Encoding;

namespace Aaru.Filesystems.LisaFS
{
    public sealed partial class LisaFS
    {
        /// <inheritdoc />
        public Errno Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding,
                           Dictionary<string, string> options, string @namespace)
        {
            try
            {
                _device  = imagePlugin;
                Encoding = new LisaRoman();

                // Lisa OS is unable to work on disks without tags.
                // This code is designed like that.
                // However with some effort the code may be modified to ignore them.
                if(_device.Info.ReadableSectorTags?.Contains(SectorTagType.AppleSectorTag) != true)
                {
                    AaruConsole.DebugWriteLine("LisaFS plugin", "Underlying device does not support Lisa tags");

                    return Errno.InOutError;
                }

                // Minimal LisaOS disk is 3.5" single sided double density, 800 sectors
                if(_device.Info.Sectors < 800)
                {
                    AaruConsole.DebugWriteLine("LisaFS plugin", "Device is too small");

                    return Errno.InOutError;
                }

                // MDDF cannot be at end of device, of course
                _volumePrefix = _device.Info.Sectors;

                // LisaOS searches sectors until tag tells MDDF resides there, so we'll search 100 sectors
                for(ulong i = 0; i < 100; i++)
                {
                    DecodeTag(_device.ReadSectorTag(i, SectorTagType.AppleSectorTag), out LisaTag.PriamTag searchTag);

                    AaruConsole.DebugWriteLine("LisaFS plugin", "Sector {0}, file ID 0x{1:X4}", i, searchTag.FileId);

                    if(_volumePrefix    == _device.Info.Sectors &&
                       searchTag.FileId == FILEID_LOADER_SIGNED)
                        _volumePrefix = i - 1;

                    if(searchTag.FileId != FILEID_MDDF)
                        continue;

                    _devTagSize = _device.ReadSectorTag(i, SectorTagType.AppleSectorTag).Length;

                    byte[] sector = _device.ReadSector(i);
                    _mddf = new MDDF();
                    byte[] pString = new byte[33];

                    _mddf.fsversion = BigEndianBitConverter.ToUInt16(sector, 0x00);
                    _mddf.volid     = BigEndianBitConverter.ToUInt64(sector, 0x02);
                    _mddf.volnum    = BigEndianBitConverter.ToUInt16(sector, 0x0A);
                    Array.Copy(sector, 0x0C, pString, 0, 33);
                    _mddf.volname  = StringHandlers.PascalToString(pString, Encoding);
                    _mddf.unknown1 = sector[0x2D];
                    Array.Copy(sector, 0x2E, pString, 0, 33);

                    // Prevent garbage
                    _mddf.password       = pString[0] <= 32 ? StringHandlers.PascalToString(pString, Encoding) : "";
                    _mddf.unknown2       = sector[0x4F];
                    _mddf.machine_id     = BigEndianBitConverter.ToUInt32(sector, 0x50);
                    _mddf.master_copy_id = BigEndianBitConverter.ToUInt32(sector, 0x54);
                    uint lisaTime = BigEndianBitConverter.ToUInt32(sector, 0x58);
                    _mddf.dtvc                         = DateHandlers.LisaToDateTime(lisaTime);
                    lisaTime                           = BigEndianBitConverter.ToUInt32(sector, 0x5C);
                    _mddf.dtcc                         = DateHandlers.LisaToDateTime(lisaTime);
                    lisaTime                           = BigEndianBitConverter.ToUInt32(sector, 0x60);
                    _mddf.dtvb                         = DateHandlers.LisaToDateTime(lisaTime);
                    lisaTime                           = BigEndianBitConverter.ToUInt32(sector, 0x64);
                    _mddf.dtvs                         = DateHandlers.LisaToDateTime(lisaTime);
                    _mddf.unknown3                     = BigEndianBitConverter.ToUInt32(sector, 0x68);
                    _mddf.mddf_block                   = BigEndianBitConverter.ToUInt32(sector, 0x6C);
                    _mddf.volsize_minus_one            = BigEndianBitConverter.ToUInt32(sector, 0x70);
                    _mddf.volsize_minus_mddf_minus_one = BigEndianBitConverter.ToUInt32(sector, 0x74);
                    _mddf.vol_size                     = BigEndianBitConverter.ToUInt32(sector, 0x78);
                    _mddf.blocksize                    = BigEndianBitConverter.ToUInt16(sector, 0x7C);
                    _mddf.datasize                     = BigEndianBitConverter.ToUInt16(sector, 0x7E);
                    _mddf.unknown4                     = BigEndianBitConverter.ToUInt16(sector, 0x80);
                    _mddf.unknown5                     = BigEndianBitConverter.ToUInt32(sector, 0x82);
                    _mddf.unknown6                     = BigEndianBitConverter.ToUInt32(sector, 0x86);
                    _mddf.clustersize                  = BigEndianBitConverter.ToUInt16(sector, 0x8A);
                    _mddf.fs_size                      = BigEndianBitConverter.ToUInt32(sector, 0x8C);
                    _mddf.unknown7                     = BigEndianBitConverter.ToUInt32(sector, 0x90);
                    _mddf.srec_ptr                     = BigEndianBitConverter.ToUInt32(sector, 0x94);
                    _mddf.unknown9                     = BigEndianBitConverter.ToUInt16(sector, 0x98);
                    _mddf.srec_len                     = BigEndianBitConverter.ToUInt16(sector, 0x9A);
                    _mddf.unknown10                    = BigEndianBitConverter.ToUInt32(sector, 0x9C);
                    _mddf.unknown11                    = BigEndianBitConverter.ToUInt32(sector, 0xA0);
                    _mddf.unknown12                    = BigEndianBitConverter.ToUInt32(sector, 0xA4);
                    _mddf.unknown13                    = BigEndianBitConverter.ToUInt32(sector, 0xA8);
                    _mddf.unknown14                    = BigEndianBitConverter.ToUInt32(sector, 0xAC);
                    _mddf.filecount                    = BigEndianBitConverter.ToUInt16(sector, 0xB0);
                    _mddf.unknown15                    = BigEndianBitConverter.ToUInt32(sector, 0xB2);
                    _mddf.unknown16                    = BigEndianBitConverter.ToUInt32(sector, 0xB6);
                    _mddf.freecount                    = BigEndianBitConverter.ToUInt32(sector, 0xBA);
                    _mddf.unknown17                    = BigEndianBitConverter.ToUInt16(sector, 0xBE);
                    _mddf.unknown18                    = BigEndianBitConverter.ToUInt32(sector, 0xC0);
                    _mddf.overmount_stamp              = BigEndianBitConverter.ToUInt64(sector, 0xC4);
                    _mddf.serialization                = BigEndianBitConverter.ToUInt32(sector, 0xCC);
                    _mddf.unknown19                    = BigEndianBitConverter.ToUInt32(sector, 0xD0);
                    _mddf.unknown_timestamp            = BigEndianBitConverter.ToUInt32(sector, 0xD4);
                    _mddf.unknown20                    = BigEndianBitConverter.ToUInt32(sector, 0xD8);
                    _mddf.unknown21                    = BigEndianBitConverter.ToUInt32(sector, 0xDC);
                    _mddf.unknown22                    = BigEndianBitConverter.ToUInt32(sector, 0xE0);
                    _mddf.unknown23                    = BigEndianBitConverter.ToUInt32(sector, 0xE4);
                    _mddf.unknown24                    = BigEndianBitConverter.ToUInt32(sector, 0xE8);
                    _mddf.unknown25                    = BigEndianBitConverter.ToUInt32(sector, 0xEC);
                    _mddf.unknown26                    = BigEndianBitConverter.ToUInt32(sector, 0xF0);
                    _mddf.unknown27                    = BigEndianBitConverter.ToUInt32(sector, 0xF4);
                    _mddf.unknown28                    = BigEndianBitConverter.ToUInt32(sector, 0xF8);
                    _mddf.unknown29                    = BigEndianBitConverter.ToUInt32(sector, 0xFC);
                    _mddf.unknown30                    = BigEndianBitConverter.ToUInt32(sector, 0x100);
                    _mddf.unknown31                    = BigEndianBitConverter.ToUInt32(sector, 0x104);
                    _mddf.unknown32                    = BigEndianBitConverter.ToUInt32(sector, 0x108);
                    _mddf.unknown33                    = BigEndianBitConverter.ToUInt32(sector, 0x10C);
                    _mddf.unknown34                    = BigEndianBitConverter.ToUInt32(sector, 0x110);
                    _mddf.unknown35                    = BigEndianBitConverter.ToUInt32(sector, 0x114);
                    _mddf.backup_volid                 = BigEndianBitConverter.ToUInt64(sector, 0x118);
                    _mddf.label_size                   = BigEndianBitConverter.ToUInt16(sector, 0x120);
                    _mddf.fs_overhead                  = BigEndianBitConverter.ToUInt16(sector, 0x122);
                    _mddf.result_scavenge              = BigEndianBitConverter.ToUInt16(sector, 0x124);
                    _mddf.boot_code                    = BigEndianBitConverter.ToUInt16(sector, 0x126);
                    _mddf.boot_environ                 = BigEndianBitConverter.ToUInt16(sector, 0x6C);
                    _mddf.unknown36                    = BigEndianBitConverter.ToUInt32(sector, 0x12A);
                    _mddf.unknown37                    = BigEndianBitConverter.ToUInt32(sector, 0x12E);
                    _mddf.unknown38                    = BigEndianBitConverter.ToUInt32(sector, 0x132);
                    _mddf.vol_sequence                 = BigEndianBitConverter.ToUInt16(sector, 0x136);
                    _mddf.vol_left_mounted             = sector[0x138];

                    // Check that the MDDF is correct
                    if(_mddf.mddf_block       != i - _volumePrefix                                  ||
                       _mddf.vol_size         > _device.Info.Sectors                                ||
                       _mddf.vol_size - 1     != _mddf.volsize_minus_one                            ||
                       _mddf.vol_size - i - 1 != _mddf.volsize_minus_mddf_minus_one - _volumePrefix ||
                       _mddf.datasize         > _mddf.blocksize                                     ||
                       _mddf.blocksize        < _device.Info.SectorSize                             ||
                       _mddf.datasize         != _device.Info.SectorSize)
                    {
                        AaruConsole.DebugWriteLine("LisaFS plugin", "Incorrect MDDF found");

                        return Errno.InvalidArgument;
                    }

                    // Check MDDF version
                    switch(_mddf.fsversion)
                    {
                        case LISA_V1:
                            AaruConsole.DebugWriteLine("LisaFS plugin", "Mounting LisaFS v1");

                            break;
                        case LISA_V2:
                            AaruConsole.DebugWriteLine("LisaFS plugin", "Mounting LisaFS v2");

                            break;
                        case LISA_V3:
                            AaruConsole.DebugWriteLine("LisaFS plugin", "Mounting LisaFS v3");

                            break;
                        default:
                            AaruConsole.ErrorWriteLine("Cannot mount LisaFS version {0}", _mddf.fsversion.ToString());

                            return Errno.NotSupported;
                    }

                    // Initialize caches
                    _extentCache     = new Dictionary<short, ExtentFile>();
                    _systemFileCache = new Dictionary<short, byte[]>();
                    _fileCache       = new Dictionary<short, byte[]>();

                    //catalogCache = new Dictionary<short, List<CatalogEntry>>();
                    _fileSizeCache = new Dictionary<short, int>();

                    _mounted = true;

                    options ??= GetDefaultOptions();

                    if(options.TryGetValue("debug", out string debugString))
                        bool.TryParse(debugString, out _debug);

                    if(_debug)
                        _printedExtents = new List<short>();

                    // Read the S-Records file
                    Errno error = ReadSRecords();

                    if(error != Errno.NoError)
                    {
                        AaruConsole.ErrorWriteLine("Error {0} reading S-Records file.", error);

                        return error;
                    }

                    _directoryDtcCache = new Dictionary<short, DateTime>
                    {
                        {
                            DIRID_ROOT, _mddf.dtcc
                        }
                    };

                    // Read the Catalog File
                    error = ReadCatalog();

                    if(error != Errno.NoError)
                    {
                        AaruConsole.DebugWriteLine("LisaFS plugin", "Cannot read Catalog File, error {0}",
                                                   error.ToString());

                        _mounted = false;

                        return error;
                    }

                    // If debug, cache system files
                    if(_debug)
                    {
                        error = ReadSystemFile(FILEID_BOOT_SIGNED, out _);

                        if(error != Errno.NoError)
                        {
                            AaruConsole.DebugWriteLine("LisaFS plugin", "Unable to read boot blocks");
                            _mounted = false;

                            return error;
                        }

                        error = ReadSystemFile(FILEID_LOADER_SIGNED, out _);

                        if(error != Errno.NoError)
                        {
                            AaruConsole.DebugWriteLine("LisaFS plugin", "Unable to read boot loader");
                            _mounted = false;

                            return error;
                        }

                        error = ReadSystemFile((short)FILEID_MDDF, out _);

                        if(error != Errno.NoError)
                        {
                            AaruConsole.DebugWriteLine("LisaFS plugin", "Unable to read MDDF");
                            _mounted = false;

                            return error;
                        }

                        error = ReadSystemFile((short)FILEID_BITMAP, out _);

                        if(error != Errno.NoError)
                        {
                            AaruConsole.DebugWriteLine("LisaFS plugin", "Unable to read volume bitmap");
                            _mounted = false;

                            return error;
                        }

                        error = ReadSystemFile((short)FILEID_SRECORD, out _);

                        if(error != Errno.NoError)
                        {
                            AaruConsole.DebugWriteLine("LisaFS plugin", "Unable to read S-Records file");
                            _mounted = false;

                            return error;
                        }
                    }

                    // Create XML metadata for mounted filesystem
                    XmlFsType = new FileSystemType();

                    if(DateTime.Compare(_mddf.dtvb, DateHandlers.LisaToDateTime(0)) > 0)
                    {
                        XmlFsType.BackupDate          = _mddf.dtvb;
                        XmlFsType.BackupDateSpecified = true;
                    }

                    XmlFsType.Clusters    = _mddf.vol_size;
                    XmlFsType.ClusterSize = (uint)(_mddf.clustersize * _mddf.datasize);

                    if(DateTime.Compare(_mddf.dtvc, DateHandlers.LisaToDateTime(0)) > 0)
                    {
                        XmlFsType.CreationDate          = _mddf.dtvc;
                        XmlFsType.CreationDateSpecified = true;
                    }

                    XmlFsType.Dirty                 = _mddf.vol_left_mounted != 0;
                    XmlFsType.Files                 = _mddf.filecount;
                    XmlFsType.FilesSpecified        = true;
                    XmlFsType.FreeClusters          = _mddf.freecount;
                    XmlFsType.FreeClustersSpecified = true;
                    XmlFsType.Type                  = "LisaFS";
                    XmlFsType.VolumeName            = _mddf.volname;
                    XmlFsType.VolumeSerial          = $"{_mddf.volid:X16}";

                    return Errno.NoError;
                }

                AaruConsole.DebugWriteLine("LisaFS plugin", "Not a Lisa filesystem");

                return Errno.NotSupported;
            }
            catch(Exception ex)
            {
                AaruConsole.ErrorWriteLine("Exception {0}, {1}, {2}", ex.Message, ex.InnerException, ex.StackTrace);

                return Errno.InOutError;
            }
        }

        /// <inheritdoc />
        public Errno Unmount()
        {
            _mounted         = false;
            _extentCache     = null;
            _systemFileCache = null;
            _fileCache       = null;
            _catalogCache    = null;
            _fileSizeCache   = null;
            _printedExtents  = null;
            _mddf            = new MDDF();
            _volumePrefix    = 0;
            _devTagSize      = 0;
            _srecords        = null;

            return Errno.NoError;
        }

        /// <inheritdoc />
        public Errno StatFs(out FileSystemInfo stat)
        {
            stat = null;

            if(!_mounted)
                return Errno.AccessDenied;

            stat = new FileSystemInfo
            {
                Blocks         = _mddf.vol_size,
                FilenameLength = (ushort)E_NAME,
                Files          = _mddf.filecount,
                FreeBlocks     = _mddf.freecount,
                Id =
                {
                    Serial64 = _mddf.volid,
                    IsLong   = true
                },
                PluginId = Id
            };

            stat.FreeFiles = FILEID_MAX - stat.Files;

            switch(_mddf.fsversion)
            {
                case LISA_V1:
                    stat.Type = "LisaFS v1";

                    break;
                case LISA_V2:
                    stat.Type = "LisaFS v2";

                    break;
                case LISA_V3:
                    stat.Type = "LisaFS v3";

                    break;
            }

            return Errno.NoError;
        }
    }
}