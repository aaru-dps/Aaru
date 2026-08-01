// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Professional File System plugin.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Aaru.Logging;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
public sealed partial class PFS
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting PFS volume");

        _imagePlugin = imagePlugin;
        _partition   = partition;
        _encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");

        // Read root block (sector 2 of partition)
        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + ROOTBLOCK, false, out byte[] rootBlockData, out _);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root block: {0}", errno);

            return errno;
        }

        _rootBlock = Marshal.ByteArrayToStructureBigEndian<RootBlock>(rootBlockData);

        // Validate root block disk type
        if(_rootBlock.diskType is not PFS_DISK and not PFS2_DISK and not AFS_DISK and not MUAF_DISK and not MUPFS_DISK)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid root block disk type: 0x{0:X8}", _rootBlock.diskType);

            return ErrorNumber.InvalidArgument;
        }

        // Store filesystem parameters
        _reservedBlockSize = _rootBlock.reservedblocksize;
        _blockSize         = imagePlugin.Info.SectorSize;

        // If reserved block size is larger than sector size, re-read with correct size
        if(_reservedBlockSize > _blockSize)
        {
            uint sectorsPerReservedBlock = _reservedBlockSize / _blockSize;

            errno = imagePlugin.ReadSectors(partition.Start + ROOTBLOCK,
                                            false,
                                            sectorsPerReservedBlock,
                                            out rootBlockData,
                                            out _);

            if(errno != ErrorNumber.NoError)
            {
                AaruLogging.Debug(MODULE_NAME, "Error re-reading root block: {0}", errno);

                return errno;
            }

            _rootBlock = Marshal.ByteArrayToStructureBigEndian<RootBlock>(rootBlockData);
        }

        _firstReserved = _rootBlock.firstreserved;
        _lastReserved  = _rootBlock.lastreserved;
        _modeFlags     = _rootBlock.options;
        _isMultiUser   = _rootBlock.diskType is MUAF_DISK or MUPFS_DISK;

        AaruLogging.Debug(MODULE_NAME, "Disk type: 0x{0:X8}",            _rootBlock.diskType);
        AaruLogging.Debug(MODULE_NAME, "Reserved block size: {0} bytes", _reservedBlockSize);
        AaruLogging.Debug(MODULE_NAME, "First reserved: {0}",            _firstReserved);
        AaruLogging.Debug(MODULE_NAME, "Last reserved: {0}",             _lastReserved);
        AaruLogging.Debug(MODULE_NAME, "Mode flags: {0}",                _modeFlags);
        AaruLogging.Debug(MODULE_NAME, "Multi-user: {0}",                _isMultiUser);

        // Extract volume name (Pascal string)
        if(_rootBlock.diskname?[0] > 0)
        {
            int nameLength = Math.Min((int)_rootBlock.diskname[0], DNSIZE - 1);
            _volumeName = _encoding.GetString(_rootBlock.diskname, 1, nameLength);
        }
        else
            _volumeName = string.Empty;

        AaruLogging.Debug(MODULE_NAME, "Volume name: {0}", _volumeName);

        // Calculate anodes per block
        _anodesPerBlock = (ushort)((_reservedBlockSize - 16) / 12); // 16 = AnodeBlock header, 12 = sizeof(Anode)

        // Determine if using split anode mode
        _splitAnodeMode = _modeFlags.HasFlag(ModeFlags.SplittedAnodes);

        AaruLogging.Debug(MODULE_NAME, "Anodes per block: {0}",   _anodesPerBlock);
        AaruLogging.Debug(MODULE_NAME, "Split anode mode: {0}",   _splitAnodeMode);
        AaruLogging.Debug(MODULE_NAME, "Dir extension: {0}",      _modeFlags.HasFlag(ModeFlags.DirExtension));
        AaruLogging.Debug(MODULE_NAME, "Large file support: {0}", _modeFlags.HasFlag(ModeFlags.LargeFile));

        // Check for rootblock extension
        if(_modeFlags.HasFlag(ModeFlags.Extension) && _rootBlock.extension != 0)
        {
            errno = ReadReservedBlock(_rootBlock.extension, out byte[] extData);

            if(errno == ErrorNumber.NoError)
            {
                _rootBlockExtension = Marshal.ByteArrayToStructureBigEndian<RootBlockExtension>(extData);

                if(_rootBlockExtension.id == EXTENSIONID)
                {
                    _hasExtension    = true;
                    _filenameSize    = _rootBlockExtension.fnsize > 0 ? _rootBlockExtension.fnsize : (ushort)30;
                    _largeDirSupport = _modeFlags.HasFlag(ModeFlags.SuperIndex);

                    AaruLogging.Debug(MODULE_NAME, "Root block extension found");
                    AaruLogging.Debug(MODULE_NAME, "Filename size: {0}",     _filenameSize);
                    AaruLogging.Debug(MODULE_NAME, "Large dir support: {0}", _largeDirSupport);
                    AaruLogging.Debug(MODULE_NAME, "PFS2 version: 0x{0:X8}", _rootBlockExtension.pfs2version);
                }
            }
        }

        if(!_hasExtension) _filenameSize = 30; // Default PFS filename size

        // Load root directory (anode 5 = ANODE_ROOTDIR)
        errno = LoadRootDirectory();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading root directory: {0}", errno);

            return errno;
        }

        // Build metadata
        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            VolumeName   = _volumeName,
            ClusterSize  = _blockSize,
            Clusters     = partition.Length,
            FreeClusters = _rootBlock.blocksfree,
            CreationDate = DateHandlers.AmigaToDateTime(_rootBlock.creationday,
                                                        _rootBlock.creationminute,
                                                        _rootBlock.creationtick)
        };

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME, "Volume mounted successfully");
        AaruLogging.Debug(MODULE_NAME, "Loaded {0} root directory entries", _rootDirectoryCache.Count);

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        _rootDirectoryCache.Clear();
        _directoryCache.Clear();
        _mounted = false;

        return ErrorNumber.NoError;
    }


    /// <summary>Loads the root directory contents into cache</summary>
    /// <returns>Error code indicating success or failure</returns>
    ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory");

        _rootDirectoryCache.Clear();
        _directoryCache.Clear();

        // Get the root directory anode (ANODE_ROOTDIR = 5)
        ErrorNumber errno = GetAnode(ANODE_ROOTDIR, out Anode rootAnode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error getting root directory anode: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME,
                          "Root anode: blocknr={0}, clustersize={1}, next={2}",
                          rootAnode.blocknr,
                          rootAnode.clustersize,
                          rootAnode.next);

        // Read directory blocks starting from the root anode
        errno = ReadDirectoryBlocks(rootAnode, _rootDirectoryCache);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root directory blocks: {0}", errno);

            return errno;
        }

        return ErrorNumber.NoError;
    }
}