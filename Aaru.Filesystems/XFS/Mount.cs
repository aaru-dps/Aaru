// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XFS filesystem plugin.
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
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements SGI's XFS</summary>
public sealed partial class XFS
{
    /// <summary>Unix file type mask from di_mode</summary>
    const ushort S_IFMT = 0xF000;

    /// <summary>Unix directory type</summary>
    const ushort S_IFDIR = 0x4000;

    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        AaruLogging.Debug(MODULE_NAME, "Mounting XFS volume");

        _imagePlugin = imagePlugin;
        _partition   = partition;
        _encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");

        if(imagePlugin.Info.SectorSize < 512)
        {
            AaruLogging.Debug(MODULE_NAME, "Sector size too small: {0}", imagePlugin.Info.SectorSize);

            return ErrorNumber.InvalidArgument;
        }

        // Read and validate the superblock
        ErrorNumber errno = ReadSuperblock();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading superblock: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Superblock read successfully");
        AaruLogging.Debug(MODULE_NAME, "Block size: {0} bytes", _superblock.blocksize);
        AaruLogging.Debug(MODULE_NAME, "AG blocks: {0}",        _superblock.agblocks);
        AaruLogging.Debug(MODULE_NAME, "AG count: {0}",         _superblock.agcount);
        AaruLogging.Debug(MODULE_NAME, "Inode size: {0} bytes", _superblock.inodesize);
        AaruLogging.Debug(MODULE_NAME, "Root inode: {0}",       _superblock.rootino);
        AaruLogging.Debug(MODULE_NAME, "Version: 0x{0:X4}",     _superblock.version);
        AaruLogging.Debug(MODULE_NAME, "Data blocks: {0}",      _superblock.dblocks);
        AaruLogging.Debug(MODULE_NAME, "Free data blocks: {0}", _superblock.fdblocks);
        AaruLogging.Debug(MODULE_NAME, "Allocated inodes: {0}", _superblock.icount);
        AaruLogging.Debug(MODULE_NAME, "Free inodes: {0}",      _superblock.ifree);
        AaruLogging.Debug(MODULE_NAME, "V3 inodes: {0}",        _v3Inodes);
        AaruLogging.Debug(MODULE_NAME, "Has ftype: {0}",        _hasFtype);

        // Load root directory
        errno = LoadRootDirectory();

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error loading root directory: {0}", errno);

            return errno;
        }

        AaruLogging.Debug(MODULE_NAME, "Root directory loaded with {0} entries", _rootDirectoryCache.Count);

        // Build metadata
        // V1-V3 use only sb_fname (6 bytes), V4+ may use sb_fname + sb_fpack (12 bytes)
        var versionNum = (ushort)(_superblock.version & XFS_SB_VERSION_NUMBITS);

        byte[] volumeNameBytes;

        if(versionNum >= XFS_SB_VERSION_4)
        {
            volumeNameBytes = new byte[12];
            Array.Copy(_superblock.fname, 0, volumeNameBytes, 0, 6);
            Array.Copy(_superblock.fpack, 0, volumeNameBytes, 6, 6);
        }
        else
            volumeNameBytes = _superblock.fname;

        string volumeName = StringHandlers.CToString(volumeNameBytes, _encoding);

        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            VolumeName   = volumeName,
            ClusterSize  = _superblock.blocksize,
            Clusters     = _superblock.dblocks,
            FreeClusters = _superblock.fdblocks,
            Files        = _superblock.icount - _superblock.ifree,
            Dirty        = _superblock.inprogress > 0,
            VolumeSerial = _superblock.uuid.ToString()
        };

        _mounted = true;

        AaruLogging.Debug(MODULE_NAME, "Volume mounted successfully");

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        if(!_mounted) return ErrorNumber.AccessDenied;

        AaruLogging.Debug(MODULE_NAME, "Unmounting volume");

        _rootDirectoryCache.Clear();
        _inodeCache.Clear();
        _directoryCache.Clear();
        _mounted          = false;
        _imagePlugin      = null;
        _partition        = default(Partition);
        _superblock       = default(Superblock);
        _encoding         = null;
        _v3Inodes         = false;
        _hasFtype         = false;
        _isDirV1          = false;
        _dirBlockFsBlocks = 0;
        Metadata          = null;

        AaruLogging.Debug(MODULE_NAME, "Volume unmounted");

        return ErrorNumber.NoError;
    }

    /// <summary>Reads and validates the XFS superblock</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber ReadSuperblock()
    {
        AaruLogging.Debug(MODULE_NAME, "Reading superblock...");

        int sbStructSize = Marshal.SizeOf<Superblock>();

        // The XFS superblock is at block 0 of AG 0, i.e. at byte offset 0 of the partition.
        // Handle the optical disc misalignment case as Info.cs does.
        if(_imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
        {
            var sbSize = (uint)((sbStructSize + 0x400) / _imagePlugin.Info.SectorSize);

            if((sbStructSize + 0x400) % _imagePlugin.Info.SectorSize != 0) sbSize++;

            ErrorNumber errno = _imagePlugin.ReadSectors(_partition.Start, false, sbSize, out byte[] sector, out _);

            if(errno != ErrorNumber.NoError) return errno;

            if(sector.Length < sbStructSize) return ErrorNumber.InvalidArgument;

            var sbpiece = new byte[sbStructSize];

            foreach(int location in new[]
                    {
                        0, 0x200, 0x400
                    })
            {
                if(location + sbStructSize > sector.Length) continue;

                Array.Copy(sector, location, sbpiece, 0, sbStructSize);

                _superblock = Marshal.ByteArrayToStructureBigEndian<Superblock>(sbpiece);

                if(_superblock.magicnum == XFS_MAGIC) break;
            }
        }
        else
        {
            foreach(int i in new[]
                    {
                        0, 1, 2
                    })
            {
                var location = (ulong)i;

                var sbSize = (uint)(sbStructSize / _imagePlugin.Info.SectorSize);

                if(sbStructSize % _imagePlugin.Info.SectorSize != 0) sbSize++;

                ErrorNumber errno =
                    _imagePlugin.ReadSectors(_partition.Start + location, false, sbSize, out byte[] sector, out _);

                if(errno != ErrorNumber.NoError) continue;

                if(sector.Length < sbStructSize) return ErrorNumber.InvalidArgument;

                _superblock = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

                if(_superblock.magicnum == XFS_MAGIC) break;
            }
        }

        if(_superblock.magicnum != XFS_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid magic: 0x{0:X8}, expected 0x{1:X8}",
                              _superblock.magicnum,
                              XFS_MAGIC);

            return ErrorNumber.InvalidArgument;
        }

        // Validate basic superblock sanity
        if(_superblock.blocksize                               == 0  ||
           (_superblock.blocksize & _superblock.blocksize - 1) != 0  ||
           _superblock.blocksize                               < 512 ||
           _superblock.blocksize                               > 65536)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid block size: {0}", _superblock.blocksize);

            return ErrorNumber.InvalidArgument;
        }

        if(_superblock.inodesize < 256 || _superblock.inodesize > _superblock.blocksize)
        {
            AaruLogging.Debug(MODULE_NAME, "Invalid inode size: {0}", _superblock.inodesize);

            return ErrorNumber.InvalidArgument;
        }

        if(_superblock.agcount == 0 || _superblock.agblocks == 0)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Invalid AG parameters: agcount={0}, agblocks={1}",
                              _superblock.agcount,
                              _superblock.agblocks);

            return ErrorNumber.InvalidArgument;
        }

        // Determine version features
        var versionNum = (ushort)(_superblock.version & XFS_SB_VERSION_NUMBITS);

        _v3Inodes = versionNum == XFS_SB_VERSION_5;

        // Directory format v1 is used for V1/V2/V3 superblocks, and V4 without DIRV2BIT
        _isDirV1 = versionNum switch
                   {
                       XFS_SB_VERSION_5 => false,
                       XFS_SB_VERSION_4 => (_superblock.version & XFS_SB_VERSION_DIRV2BIT) == 0,
                       _                => true
                   };

        // Dir v1 always uses 1 filesystem block per directory block; dir v2 uses 1 << dirblklog
        _dirBlockFsBlocks = _isDirV1 ? 1U : 1U << _superblock.dirblklog;

        // ftype is supported if v5 with FTYPE incompat, or v4 with features2 FTYPE bit
        // Dir v1 never has ftype
        if(_isDirV1)
            _hasFtype = false;
        else if(_v3Inodes)
            _hasFtype = (_superblock.features_incompat & XFS_SB_FEAT_INCOMPAT_FTYPE) != 0;
        else
            _hasFtype = (_superblock.features2 & XFS_SB_VERSION2_FTYPE) != 0;

        AaruLogging.Debug(MODULE_NAME, "Dir V1: {0}",              _isDirV1);
        AaruLogging.Debug(MODULE_NAME, "Dir block fs blocks: {0}", _dirBlockFsBlocks);

        return ErrorNumber.NoError;
    }

    /// <summary>Loads the root directory and caches its contents</summary>
    /// <returns>Error number indicating success or failure</returns>
    ErrorNumber LoadRootDirectory()
    {
        AaruLogging.Debug(MODULE_NAME, "Loading root directory...");

        _rootDirectoryCache.Clear();
        _inodeCache.Clear();
        _directoryCache.Clear();

        // Read the root inode
        ErrorNumber errno = ReadInode(_superblock.rootino, out Dinode rootInode);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME, "Error reading root inode: {0}", errno);

            return errno;
        }

        // Validate that root inode is a directory
        if((rootInode.di_mode & S_IFMT) != S_IFDIR)
        {
            AaruLogging.Debug(MODULE_NAME, "Root inode is not a directory (mode=0x{0:X4})", rootInode.di_mode);

            return ErrorNumber.InvalidArgument;
        }

        // Validate inode magic
        if(rootInode.di_magic != XFS_DINODE_MAGIC)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Root inode has invalid magic: 0x{0:X4}, expected 0x{1:X4}",
                              rootInode.di_magic,
                              XFS_DINODE_MAGIC);

            return ErrorNumber.InvalidArgument;
        }

        _inodeCache[_superblock.rootino] = rootInode;

        AaruLogging.Debug(MODULE_NAME, "Root inode mode: 0x{0:X4}",         rootInode.di_mode);
        AaruLogging.Debug(MODULE_NAME, "Root inode format: {0}",            rootInode.di_format);
        AaruLogging.Debug(MODULE_NAME, "Root inode size: {0} bytes",        rootInode.di_size);
        AaruLogging.Debug(MODULE_NAME, "Root inode version: {0}",           rootInode.di_version);
        AaruLogging.Debug(MODULE_NAME, "Root inode data fork extents: {0}", rootInode.di_nextents);

        // Parse directory contents based on format
        errno = GetDirectoryContents(_superblock.rootino, rootInode, out Dictionary<string, ulong> rootEntries);

        if(errno != ErrorNumber.NoError)
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Error reading directory contents (format={0}): {1}",
                              rootInode.di_format,
                              errno);

            return errno;
        }

        // Copy into root directory cache
        foreach(KeyValuePair<string, ulong> entry in rootEntries) _rootDirectoryCache[entry.Key] = entry.Value;

        AaruLogging.Debug(MODULE_NAME, "Cached {0} root directory entries", _rootDirectoryCache.Count);

        return ErrorNumber.NoError;
    }
}