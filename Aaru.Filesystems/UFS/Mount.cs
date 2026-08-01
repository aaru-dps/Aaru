// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Mount.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNIX FIle System plugin.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class UFSPlugin
{
    /// <inheritdoc />
    public ErrorNumber Mount(IMediaImage                imagePlugin, Partition partition, Encoding encoding,
                             Dictionary<string, string> options,     string    @namespace)
    {
        _encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        _imagePlugin = imagePlugin;
        _partition   = partition;

        uint sbSizeInSectors;

        if(imagePlugin.Info.SectorSize is 2336 or 2352 or 2448)
            sbSizeInSectors = SBSIZE / 2048;
        else
            sbSizeInSectors = SBSIZE / imagePlugin.Info.SectorSize;

        ulong[] locations =
        [
            SBLOCK_FLOPPY, SBLOCK, SBLOCK_LONG_BOOT, SBLOCK_PIGGY, SBLOCK_ATT_DSDD,
            4096  / imagePlugin.Info.SectorSize, 8192   / imagePlugin.Info.SectorSize,
            65536 / imagePlugin.Info.SectorSize, 262144 / imagePlugin.Info.SectorSize
        ];

        uint  magic    = 0;
        ulong sbOffset = partition.Start;

        byte[] ufsSbSectors = null;

        foreach(ulong loc in locations.Where(loc => partition.End > partition.Start + loc + sbSizeInSectors))
        {
            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + loc,
                                                        false,
                                                        sbSizeInSectors,
                                                        out ufsSbSectors,
                                                        out _);

            if(errno != ErrorNumber.NoError) continue;

            magic = BitConverter.ToUInt32(ufsSbSectors, 0x055C);

            if(magic is UFS_MAGIC
                     or UFS_CIGAM
                     or UFS_MAGIC_BW
                     or UFS_CIGAM_BW
                     or UFS2_MAGIC
                     or UFS2_CIGAM
                     or UFS_BAD_MAGIC
                     or UFS_BAD_CIGAM
                     or FS_MAGIC_LFN
                     or FS_CIGAM_LFN
                     or FD_FSMAGIC
                     or FD_FSCIGAM
                     or FS_SEC_MAGIC
                     or FS_SEC_CIGAM
                     or MTB_UFS_MAGIC
                     or MTB_UFS_CIGAM)
            {
                sbOffset = partition.Start + loc;

                break;
            }

            magic = 0;
        }

        if(magic == 0 || ufsSbSectors is null) return ErrorNumber.InvalidArgument;

        // Determine endianness
        _bigEndian = magic is UFS_CIGAM
                           or UFS_CIGAM_BW
                           or UFS2_CIGAM
                           or UFS_BAD_CIGAM
                           or FS_CIGAM_LFN
                           or FD_FSCIGAM
                           or FS_SEC_CIGAM
                           or MTB_UFS_CIGAM;

        // Read the generic superblock (FreeBSD combined UFS1/UFS2 layout)
        SuperBlock sb = _bigEndian
                            ? Marshal.ByteArrayToStructureBigEndian<SuperBlock>(ufsSbSectors)
                            : Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(ufsSbSectors);

        if(_bigEndian)
        {
            sb.fs_old_cstotal.cs_nbfree  = Swapping.Swap(sb.fs_old_cstotal.cs_nbfree);
            sb.fs_old_cstotal.cs_ndir    = Swapping.Swap(sb.fs_old_cstotal.cs_ndir);
            sb.fs_old_cstotal.cs_nffree  = Swapping.Swap(sb.fs_old_cstotal.cs_nffree);
            sb.fs_old_cstotal.cs_nifree  = Swapping.Swap(sb.fs_old_cstotal.cs_nifree);
            sb.fs_cstotal.cs_numclusters = Swapping.Swap(sb.fs_cstotal.cs_numclusters);
            sb.fs_cstotal.cs_nbfree      = Swapping.Swap(sb.fs_cstotal.cs_nbfree);
            sb.fs_cstotal.cs_ndir        = Swapping.Swap(sb.fs_cstotal.cs_ndir);
            sb.fs_cstotal.cs_nffree      = Swapping.Swap(sb.fs_cstotal.cs_nffree);
            sb.fs_cstotal.cs_nifree      = Swapping.Swap(sb.fs_cstotal.cs_nifree);
        }

        bool isUfs2 = sb.fs_magic == UFS2_MAGIC;

        // Detect Solaris variant
        bool isSolaris = magic is MTB_UFS_MAGIC or MTB_UFS_CIGAM;

        if(!isUfs2 && !isSolaris)
        {
            SuperblockSun sbSun = _bigEndian
                                      ? Marshal.ByteArrayToStructureBigEndian<SuperblockSun>(ufsSbSectors)
                                      : Marshal.ByteArrayToStructureLittleEndian<SuperblockSun>(ufsSbSectors);

            unchecked
            {
                if(((uint)(sbSun.fs_state + sbSun.fs_time)  == FSOKAY ||
                    (uint)(sbSun.fs_time  - sbSun.fs_state) == FSOKAY) &&
                   (sbSun.fs_version > 0 || sbSun.fs_logbno != 0 || sbSun.fs_rolled is > 0 and <= 2))
                    isSolaris                                             = true;
                else if(sbSun.fs_clean is 0xFC or 0xFD or 0xFE) isSolaris = true;
            }
        }

        // Also read variant-specific superblocks for inodefmt/maxsymlinklen detection
        SuperBlock44BSD sb44bsd = _bigEndian
                                      ? Marshal.ByteArrayToStructureBigEndian<SuperBlock44BSD>(ufsSbSectors)
                                      : Marshal.ByteArrayToStructureLittleEndian<SuperBlock44BSD>(ufsSbSectors);

        // Determine inode format
        int inodefmt;
        int maxsymlinklen;

        if(isUfs2)
        {
            inodefmt      = 2; // FS_44INODEFMT
            maxsymlinklen = sb.fs_maxsymlinklen;
        }
        else if(sb44bsd.fs_inodefmt == InodeFormat.FS_44INODEFMT)
        {
            inodefmt      = 2;
            maxsymlinklen = sb44bsd.fs_maxsymlinklen;
        }
        else if(sb.fs_old_inodefmt == 2)
        {
            inodefmt      = 2;
            maxsymlinklen = sb.fs_maxsymlinklen;
        }
        else
        {
            inodefmt      = -1; // FS_42INODEFMT
            maxsymlinklen = 0;
        }

        // Build the unified superblock
        _superBlock = new UfsSuperBlock
        {
            fs_sblkno        = sb.fs_sblkno,
            fs_cblkno        = sb.fs_cblkno,
            fs_iblkno        = sb.fs_iblkno,
            fs_dblkno        = sb.fs_dblkno,
            fs_cgoffset      = sb.fs_old_cgoffset,
            fs_cgmask        = sb.fs_old_cgmask,
            fs_ncg           = sb.fs_ncg,
            fs_bsize         = sb.fs_bsize,
            fs_fsize         = sb.fs_fsize,
            fs_frag          = sb.fs_frag,
            fs_bmask         = sb.fs_bmask,
            fs_fmask         = sb.fs_fmask,
            fs_bshift        = sb.fs_bshift,
            fs_fshift        = sb.fs_fshift,
            fs_fragshift     = sb.fs_fragshift,
            fs_fsbtodb       = sb.fs_fsbtodb,
            fs_nindir        = sb.fs_nindir,
            fs_inopb         = sb.fs_inopb,
            fs_ipg           = sb.fs_ipg,
            fs_fpg           = sb.fs_fpg,
            fs_cssize        = sb.fs_cssize,
            fs_cgsize        = sb.fs_cgsize,
            fs_sbsize        = sb.fs_sbsize,
            fs_magic         = sb.fs_magic,
            fs_id_1          = sb.fs_id_1,
            fs_id_2          = sb.fs_id_2,
            fs_clean         = sb.fs_clean,
            fs_flags         = sb.fs_flags,
            fs_fsmnt         = StringHandlers.CToString(sb.fs_fsmnt, _encoding),
            fs_isUfs2        = isUfs2,
            fs_isSolaris     = isSolaris,
            fs_inodefmt      = inodefmt,
            fs_maxsymlinklen = maxsymlinklen,
            fs_old_spc       = sb.fs_old_spc,
            fs_old_cpg       = sb.fs_old_cpg,
            fs_old_ncyl      = sb.fs_old_ncyl
        };

        if(isUfs2)
        {
            // UFS2 uses 64-bit fields from the generic SuperBlock struct
            _superBlock.fs_size           = sb.fs_size;
            _superBlock.fs_dsize          = sb.fs_dsize;
            _superBlock.fs_csaddr         = sb.fs_csaddr;
            _superBlock.fs_time           = sb.fs_time;
            _superBlock.fs_cstotal_ndir   = sb.fs_cstotal.cs_ndir;
            _superBlock.fs_cstotal_nbfree = sb.fs_cstotal.cs_nbfree;
            _superBlock.fs_cstotal_nifree = sb.fs_cstotal.cs_nifree;
            _superBlock.fs_cstotal_nffree = sb.fs_cstotal.cs_nffree;
            _superBlock.fs_volname        = StringHandlers.CToString(sb.fs_volname, _encoding);
            _superBlock.fs_metaspace      = sb.fs_metaspace;
            _superBlock.fs_sblockloc      = sb.fs_sblockloc;
        }
        else
        {
            // UFS1 uses 32-bit fields stored in the fs_old_* positions
            _superBlock.fs_size           = sb.fs_old_size;
            _superBlock.fs_dsize          = sb.fs_old_dsize;
            _superBlock.fs_csaddr         = sb.fs_old_csaddr;
            _superBlock.fs_time           = sb.fs_old_time;
            _superBlock.fs_cstotal_ndir   = sb.fs_old_cstotal.cs_ndir;
            _superBlock.fs_cstotal_nbfree = sb.fs_old_cstotal.cs_nbfree;
            _superBlock.fs_cstotal_nifree = sb.fs_old_cstotal.cs_nifree;
            _superBlock.fs_cstotal_nffree = sb.fs_old_cstotal.cs_nffree;
            _superBlock.fs_volname        = "";
            _superBlock.fs_metaspace      = 0;
            _superBlock.fs_sblockloc      = 0;
        }

        // Validate by reading the root inode
        if(_superBlock.fs_isUfs2)
        {
            ErrorNumber errno = ReadInode2(UFS_ROOTINO, out Inode2 rootInode);

            if(errno != ErrorNumber.NoError) return errno;

            if((rootInode.di_mode & 0xF000) != 0x4000) // Not a directory
                return ErrorNumber.InvalidArgument;
        }
        else
        {
            ErrorNumber errno = ReadInode(UFS_ROOTINO, out Inode rootInode);

            if(errno != ErrorNumber.NoError) return errno;

            if((rootInode.di_mode & 0xF000) != 0x4000) return ErrorNumber.InvalidArgument;
        }

        // Cache root directory entries
        _directoryCache.Clear();
        _pathCache.Clear();
        _inodeBlockCache.Clear();
        ErrorNumber rootErr = ParseDirectory(UFS_ROOTINO, out _rootEntries);

        if(rootErr != ErrorNumber.NoError) return rootErr;

        // Detect FreeBSD UFS1 extended attributes (/.attribute/ directory)
        _hasFreeBsdExtattr = false;
        _extAttrDirInode   = 0;

        if(!_superBlock.fs_isUfs2 && !_superBlock.fs_isSolaris)
        {
            foreach(DirectoryEntryInfo entry in _rootEntries)
            {
                if(entry.Name != ".attribute") continue;

                _extAttrDirInode   = entry.Inode;
                _hasFreeBsdExtattr = true;

                break;
            }
        }

        _mounted = true;

        Metadata = new FileSystem
        {
            Type        = _superBlock.fs_isUfs2 ? FS_TYPE_UFS2 : FS_TYPE_UFS,
            ClusterSize = (uint)_superBlock.fs_fsize,
            Clusters    = (ulong)_superBlock.fs_size,
            VolumeName  = _superBlock.fs_volname
        };

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber Unmount()
    {
        _mounted           = false;
        _imagePlugin       = null;
        _superBlock        = null;
        _rootEntries       = null;
        _directoryCache.Clear();
        _pathCache.Clear();
        _inodeBlockCache.Clear();
        _hasFreeBsdExtattr = false;
        _extAttrDirInode   = 0;

        return ErrorNumber.NoError;
    }

    /// <inheritdoc />
    public ErrorNumber StatFs(out FileSystemInfo stat)
    {
        stat = null;

        if(!_mounted) return ErrorNumber.AccessDenied;

        stat = new FileSystemInfo
        {
            Blocks         = (ulong)_superBlock.fs_size,
            FilenameLength = MAXNAMLEN,
            Files          = (ulong)((long)_superBlock.fs_ncg * _superBlock.fs_ipg),
            FreeBlocks     = (ulong)_superBlock.fs_cstotal_nbfree,
            FreeFiles      = (ulong)_superBlock.fs_cstotal_nifree,
            PluginId       = Id,
            Type           = _superBlock.fs_isUfs2 ? FS_TYPE_UFS2 : FS_TYPE_UFS
        };

        stat.Id = new FileSystemId
        {
            IsInt    = true,
            Serial32 = (uint)(_superBlock.fs_id_1 & 0xFFFF | (_superBlock.fs_id_2 & 0xFFFF) << 16)
        };

        return ErrorNumber.NoError;
    }
}