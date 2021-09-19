// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Super.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Macintosh File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles mounting and umounting the Apple Macintosh File System.
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
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems
{
    // Information from Inside Macintosh Volume II
    public sealed partial class AppleMFS
    {
        const int BYTES_BEFORE_BLOCK_MAP = 64;

        /// <inheritdoc />
        public ErrorNumber Mount(IMediaImage imagePlugin, Partition partition, Encoding encoding,
                                 Dictionary<string, string> options, string @namespace)
        {
            _device         = imagePlugin;
            _partitionStart = partition.Start;
            Encoding        = encoding ?? Encoding.GetEncoding("macintosh");
            ErrorNumber errno;

            options ??= GetDefaultOptions();

            if(options.TryGetValue("debug", out string debugString))
                bool.TryParse(debugString, out _debug);

            _volMdb = new MasterDirectoryBlock();

            errno = _device.ReadSector(2 + _partitionStart, out _mdbBlocks);

            if(errno != ErrorNumber.NoError)
                return errno;

            errno = _device.ReadSector(0 + _partitionStart, out _bootBlocks);

            if(errno != ErrorNumber.NoError)
                return errno;

            _volMdb.drSigWord = BigEndianBitConverter.ToUInt16(_mdbBlocks, 0x000);

            if(_volMdb.drSigWord != MFS_MAGIC)
                return ErrorNumber.InvalidArgument;

            _volMdb.drCrDate   = BigEndianBitConverter.ToUInt32(_mdbBlocks, 0x002);
            _volMdb.drLsBkUp   = BigEndianBitConverter.ToUInt32(_mdbBlocks, 0x006);
            _volMdb.drAtrb     = (AppleCommon.VolumeAttributes)BigEndianBitConverter.ToUInt16(_mdbBlocks, 0x00A);
            _volMdb.drNmFls    = BigEndianBitConverter.ToUInt16(_mdbBlocks, 0x00C);
            _volMdb.drDirSt    = BigEndianBitConverter.ToUInt16(_mdbBlocks, 0x00E);
            _volMdb.drBlLen    = BigEndianBitConverter.ToUInt16(_mdbBlocks, 0x010);
            _volMdb.drNmAlBlks = BigEndianBitConverter.ToUInt16(_mdbBlocks, 0x012);
            _volMdb.drAlBlkSiz = BigEndianBitConverter.ToUInt32(_mdbBlocks, 0x014);
            _volMdb.drClpSiz   = BigEndianBitConverter.ToUInt32(_mdbBlocks, 0x018);
            _volMdb.drAlBlSt   = BigEndianBitConverter.ToUInt16(_mdbBlocks, 0x01C);
            _volMdb.drNxtFNum  = BigEndianBitConverter.ToUInt32(_mdbBlocks, 0x01E);
            _volMdb.drFreeBks  = BigEndianBitConverter.ToUInt16(_mdbBlocks, 0x022);
            _volMdb.drVNSiz    = _mdbBlocks[0x024];
            byte[] variableSize = new byte[_volMdb.drVNSiz + 1];
            Array.Copy(_mdbBlocks, 0x024, variableSize, 0, _volMdb.drVNSiz + 1);
            _volMdb.drVN = StringHandlers.PascalToString(variableSize, Encoding);

            errno = _device.ReadSectors(_volMdb.drDirSt + _partitionStart, _volMdb.drBlLen, out _directoryBlocks);

            if(errno != ErrorNumber.NoError)
                return errno;

            int bytesInBlockMap = (_volMdb.drNmAlBlks * 12 / 8) + (_volMdb.drNmAlBlks * 12 % 8);
            int bytesInWholeMdb = bytesInBlockMap               + BYTES_BEFORE_BLOCK_MAP;

            int sectorsInWholeMdb = (bytesInWholeMdb / (int)_device.Info.SectorSize) +
                                    (bytesInWholeMdb % (int)_device.Info.SectorSize);

            errno = _device.ReadSectors(_partitionStart + 2, (uint)sectorsInWholeMdb, out byte[] wholeMdb);

            if(errno != ErrorNumber.NoError)
                return errno;

            _blockMapBytes = new byte[bytesInBlockMap];
            Array.Copy(wholeMdb, BYTES_BEFORE_BLOCK_MAP, _blockMapBytes, 0, _blockMapBytes.Length);

            int offset = 0;
            _blockMap = new uint[_volMdb.drNmAlBlks + 2 + 1];

            for(int i = 2; i < _volMdb.drNmAlBlks + 2; i += 8)
            {
                uint tmp1 = 0;
                uint tmp2 = 0;
                uint tmp3 = 0;

                if(offset + 4 <= _blockMapBytes.Length)
                    tmp1 = BigEndianBitConverter.ToUInt32(_blockMapBytes, offset);

                if(offset + 4 + 4 <= _blockMapBytes.Length)
                    tmp2 = BigEndianBitConverter.ToUInt32(_blockMapBytes, offset + 4);

                if(offset + 8 + 4 <= _blockMapBytes.Length)
                    tmp3 = BigEndianBitConverter.ToUInt32(_blockMapBytes, offset + 8);

                if(i < _blockMap.Length)
                    _blockMap[i] = (tmp1 & 0xFFF00000) >> 20;

                if(i + 2 < _blockMap.Length)
                    _blockMap[i + 1] = (tmp1 & 0xFFF00) >> 8;

                if(i + 3 < _blockMap.Length)
                    _blockMap[i + 2] = ((tmp1 & 0xFF) << 4) + ((tmp2 & 0xF0000000) >> 28);

                if(i + 4 < _blockMap.Length)
                    _blockMap[i + 3] = (tmp2 & 0xFFF0000) >> 16;

                if(i + 5 < _blockMap.Length)
                    _blockMap[i + 4] = (tmp2 & 0xFFF0) >> 4;

                if(i + 6 < _blockMap.Length)
                    _blockMap[i + 5] = ((tmp2 & 0xF) << 8) + ((tmp3 & 0xFF000000) >> 24);

                if(i + 7 < _blockMap.Length)
                    _blockMap[i + 6] = (tmp3 & 0xFFF000) >> 12;

                if(i + 8 < _blockMap.Length)
                    _blockMap[i + 7] = tmp3 & 0xFFF;

                offset += 12;
            }

            if(_device.Info.ReadableSectorTags.Contains(SectorTagType.AppleSectorTag))
            {
                _mdbTags  = _device.ReadSectorTag(2 + _partitionStart, SectorTagType.AppleSectorTag);
                _bootTags = _device.ReadSectorTag(0 + _partitionStart, SectorTagType.AppleSectorTag);

                _directoryTags = _device.ReadSectorsTag(_volMdb.drDirSt + _partitionStart, _volMdb.drBlLen,
                                                        SectorTagType.AppleSectorTag);

                _bitmapTags = _device.ReadSectorsTag(_partitionStart + 2, (uint)sectorsInWholeMdb,
                                                     SectorTagType.AppleSectorTag);
            }

            _sectorsPerBlock = (int)(_volMdb.drAlBlkSiz / _device.Info.SectorSize);

            if(!FillDirectory())
                return ErrorNumber.InvalidArgument;

            _mounted = true;

            ushort bbSig = BigEndianBitConverter.ToUInt16(_bootBlocks, 0x000);

            if(bbSig != AppleCommon.BB_MAGIC)
                _bootBlocks = null;

            XmlFsType = new FileSystemType();

            if(_volMdb.drLsBkUp > 0)
            {
                XmlFsType.BackupDate          = DateHandlers.MacToDateTime(_volMdb.drLsBkUp);
                XmlFsType.BackupDateSpecified = true;
            }

            XmlFsType.Bootable    = bbSig == AppleCommon.BB_MAGIC;
            XmlFsType.Clusters    = _volMdb.drNmAlBlks;
            XmlFsType.ClusterSize = _volMdb.drAlBlkSiz;

            if(_volMdb.drCrDate > 0)
            {
                XmlFsType.CreationDate          = DateHandlers.MacToDateTime(_volMdb.drCrDate);
                XmlFsType.CreationDateSpecified = true;
            }

            XmlFsType.Files                 = _volMdb.drNmFls;
            XmlFsType.FilesSpecified        = true;
            XmlFsType.FreeClusters          = _volMdb.drFreeBks;
            XmlFsType.FreeClustersSpecified = true;
            XmlFsType.Type                  = "MFS";
            XmlFsType.VolumeName            = _volMdb.drVN;

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber Unmount()
        {
            _mounted      = false;
            _idToFilename = null;
            _idToEntry    = null;
            _filenameToId = null;
            _bootBlocks   = null;

            return ErrorNumber.NoError;
        }

        /// <inheritdoc />
        public ErrorNumber StatFs(out FileSystemInfo stat)
        {
            stat = new FileSystemInfo
            {
                Blocks         = _volMdb.drNmAlBlks,
                FilenameLength = 255,
                Files          = _volMdb.drNmFls,
                FreeBlocks     = _volMdb.drFreeBks,
                PluginId       = Id,
                Type           = "Apple MFS"
            };

            stat.FreeFiles = uint.MaxValue - stat.Files;

            return ErrorNumber.NoError;
        }
    }
}