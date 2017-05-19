// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BSD Fast File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the BSD Fast File System and shows information.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    // Using information from Linux kernel headers
    public class FFSPlugin : Filesystem
    {
        public FFSPlugin()
        {
            Name = "BSD Fast File System (aka UNIX File System, UFS)";
            PluginUUID = new Guid("CC90D342-05DB-48A8-988C-C1FE000034A3");
        }

        public FFSPlugin(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            Name = "BSD Fast File System (aka UNIX File System, UFS)";
            PluginUUID = new Guid("CC90D342-05DB-48A8-988C-C1FE000034A3");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if((2 + partitionStart) >= partitionEnd)
                return false;

            uint magic;
            uint sb_size_in_sectors;
            byte[] ufs_sb_sectors;

            if(imagePlugin.GetSectorSize() == 2336 || imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
                sb_size_in_sectors = block_size / 2048;
            else
                sb_size_in_sectors = block_size / imagePlugin.GetSectorSize();

            if(partitionEnd > (partitionStart + sb_start_floppy * sb_size_in_sectors + sb_size_in_sectors))
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_floppy * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    return true;
            }

            if(partitionEnd > (partitionStart + sb_start_ufs1 * sb_size_in_sectors + sb_size_in_sectors))
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_ufs1 * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    return true;
            }

            if(partitionEnd > (partitionStart + sb_start_ufs2 * sb_size_in_sectors + sb_size_in_sectors))
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_ufs2 * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    return true;
            }

            if(partitionEnd > (partitionStart + sb_start_piggy * sb_size_in_sectors + sb_size_in_sectors))
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_piggy * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    return true;
            }

            if(partitionEnd > (partitionStart + sb_start_atari / imagePlugin.GetSectorSize() + sb_size_in_sectors))
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + (sb_start_atari / imagePlugin.GetSectorSize()), sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    return true;
            }

            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";
            StringBuilder sbInformation = new StringBuilder();

            uint magic = 0;
            uint sb_size_in_sectors;
            byte[] ufs_sb_sectors;
            ulong sb_offset = partitionStart;
            bool fs_type_42bsd = false;
            bool fs_type_43bsd = false;
            bool fs_type_44bsd = false;
            bool fs_type_ufs = false;
            bool fs_type_ufs2 = false;
            bool fs_type_sun = false;
            bool fs_type_sun86 = false;

            if(imagePlugin.GetSectorSize() == 2336 || imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
                sb_size_in_sectors = block_size / 2048;
            else
                sb_size_in_sectors = block_size / imagePlugin.GetSectorSize();

            if(partitionEnd > (partitionStart + sb_start_floppy * sb_size_in_sectors + sb_size_in_sectors) && magic == 0)
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_floppy * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    sb_offset = partitionStart + sb_start_floppy * sb_size_in_sectors;
                else
                    magic = 0;
            }

            if(partitionEnd > (partitionStart + sb_start_ufs1 * sb_size_in_sectors + sb_size_in_sectors) && magic == 0)
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_ufs1 * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    sb_offset = partitionStart + sb_start_ufs1 * sb_size_in_sectors;
                else
                    magic = 0;
            }

            if(partitionEnd > (partitionStart + sb_start_ufs2 * sb_size_in_sectors + sb_size_in_sectors) && magic == 0)
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_ufs2 * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    sb_offset = partitionStart + sb_start_ufs2 * sb_size_in_sectors;
                else
                    magic = 0;
            }

            if(partitionEnd > (partitionStart + sb_start_piggy * sb_size_in_sectors + sb_size_in_sectors) && magic == 0)
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_piggy * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    sb_offset = partitionStart + sb_start_piggy * sb_size_in_sectors;
                else
                    magic = 0;
            }

            if(partitionEnd > (partitionStart + sb_start_atari / imagePlugin.GetSectorSize() + sb_size_in_sectors) && magic == 0)
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_atari / imagePlugin.GetSectorSize(), sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    sb_offset = partitionStart + sb_start_atari / imagePlugin.GetSectorSize();
                else
                    magic = 0;
            }

            if(magic == 0)
            {
                information = "Not a UFS filesystem, I shouldn't have arrived here!";
                return;
            }

            xmlFSType = new Schemas.FileSystemType();

            switch(magic)
            {
                case UFS_MAGIC:
                    sbInformation.AppendLine("UFS filesystem");
                    xmlFSType.Type = "UFS";
                    break;
                case UFS_MAGIC_BW:
                    sbInformation.AppendLine("BorderWare UFS filesystem");
                    xmlFSType.Type = "UFS";
                    break;
                case UFS2_MAGIC:
                    sbInformation.AppendLine("UFS2 filesystem");
                    xmlFSType.Type = "UFS2";
                    break;
                case UFS_CIGAM:
                    sbInformation.AppendLine("Big-endian UFS filesystem");
                    xmlFSType.Type = "UFS";
                    break;
                case UFS_BAD_MAGIC:
                    sbInformation.AppendLine("Incompletely initialized UFS filesystem");
                    sbInformation.AppendLine("BEWARE!!! Following information may be completely wrong!");
                    xmlFSType.Type = "UFS";
                    break;
            }

            BigEndianBitConverter.IsLittleEndian = magic != UFS_CIGAM;  // Little-endian UFS
            // Are there any other cases to detect big-endian UFS?

            // Fun with seeking follows on superblock reading!
            UFSSuperBlock ufs_sb = new UFSSuperBlock();
            byte[] strings_b;
            ufs_sb_sectors = imagePlugin.ReadSectors(sb_offset, sb_size_in_sectors);

            ufs_sb.fs_link_42bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0000); /// <summary>0x0000
            ufs_sb.fs_state_sun = ufs_sb.fs_link_42bsd;
            ufs_sb.fs_rlink = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0004);     /// <summary>0x0004 UNUSED
            ufs_sb.fs_sblkno = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0008);    /// <summary>0x0008 addr of super-block in filesys
            ufs_sb.fs_cblkno = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x000C);    /// <summary>0x000C offset of cyl-block in filesys
            ufs_sb.fs_iblkno = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0010);    /// <summary>0x0010 offset of inode-blocks in filesys
            ufs_sb.fs_dblkno = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0014);    /// <summary>0x0014 offset of first data after cg
            ufs_sb.fs_cgoffset = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0018);  /// <summary>0x0018 cylinder group offset in cylinder
            ufs_sb.fs_cgmask = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x001C);    /// <summary>0x001C used to calc mod fs_ntrak
            ufs_sb.fs_time_t = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0020);    /// <summary>0x0020 last time written -- time_t
            ufs_sb.fs_size = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0024);      /// <summary>0x0024 number of blocks in fs
            ufs_sb.fs_dsize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0028);     /// <summary>0x0028 number of data blocks in fs
            ufs_sb.fs_ncg = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x002C);       /// <summary>0x002C number of cylinder groups
            ufs_sb.fs_bsize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0030);     /// <summary>0x0030 size of basic blocks in fs
            ufs_sb.fs_fsize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0034);     /// <summary>0x0034 size of frag blocks in fs
            ufs_sb.fs_frag = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0038);      /// <summary>0x0038 number of frags in a block in fs
            // these are configuration parameters
            ufs_sb.fs_minfree = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x003C);   /// <summary>0x003C minimum percentage of free blocks
            ufs_sb.fs_rotdelay = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0040);  /// <summary>0x0040 num of ms for optimal next block
            ufs_sb.fs_rps = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0044);       /// <summary>0x0044 disk revolutions per second
            // these fields can be computed from the others
            ufs_sb.fs_bmask = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0048);     /// <summary>0x0048 ``blkoff'' calc of blk offsets
            ufs_sb.fs_fmask = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x004C);     /// <summary>0x004C ``fragoff'' calc of frag offsets
            ufs_sb.fs_bshift = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0050);    /// <summary>0x0050 ``lblkno'' calc of logical blkno
            ufs_sb.fs_fshift = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0054);    /// <summary>0x0054 ``numfrags'' calc number of frags
            // these are configuration parameters
            ufs_sb.fs_maxcontig = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0058); /// <summary>0x0058 max number of contiguous blks
            ufs_sb.fs_maxbpg = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x005C);    /// <summary>0x005C max number of blks per cyl group
            // these fields can be computed from the others
            ufs_sb.fs_fragshift = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0060); /// <summary>0x0060 block to frag shift
            ufs_sb.fs_fsbtodb = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0064);   /// <summary>0x0064 fsbtodb and dbtofsb shift constant
            ufs_sb.fs_sbsize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0068);    /// <summary>0x0068 actual size of super block
            ufs_sb.fs_csmask = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x006C);    /// <summary>0x006C csum block offset
            ufs_sb.fs_csshift = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0070);   /// <summary>0x0070 csum block number
            ufs_sb.fs_nindir = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0074);    /// <summary>0x0074 value of NINDIR
            ufs_sb.fs_inopb = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0078);     /// <summary>0x0078 value of INOPB
            ufs_sb.fs_nspf = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x007C);      /// <summary>0x007C value of NSPF
            // yet another configuration parameter
            ufs_sb.fs_optim = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0080);     /// <summary>0x0080 optimization preference, see below
            // these fields are derived from the hardware
            #region Sun
            ufs_sb.fs_npsect_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0084);               /// <summary>0x0084 # sectors/track including spares
            #endregion Sun
            #region Sunx86
            ufs_sb.fs_state_t_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0084);              /// <summary>0x0084 file system state time stamp
            #endregion Sunx86
            #region COMMON
            ufs_sb.fs_interleave = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0088);               /// <summary>0x0088 hardware sector interleave
            ufs_sb.fs_trackskew = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x008C);                /// <summary>0x008C sector 0 skew, per track
            #endregion COMMON
            // a unique id for this filesystem (currently unused and unmaintained)
            // In 4.3 Tahoe this space is used by fs_headswitch and fs_trkseek
            // Neither of those fields is used in the Tahoe code right now but
            // there could be problems if they are.                           
            #region COMMON
            ufs_sb.fs_id_1 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0090);                     /// <summary>0x0090
            ufs_sb.fs_id_2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0094);                     /// <summary>0x0094
            #endregion COMMON
            #region 43BSD
            ufs_sb.fs_headswitch_43bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0090);         /// <summary>0x0090
            ufs_sb.fs_trkseek_43bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0094);            /// <summary>0x0094
            #endregion 43BSD
            #region COMMON
            // sizes determined by number of cylinder groups and their sizes
            ufs_sb.fs_csaddr = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0098);                   /// <summary>0x0098 blk addr of cyl grp summary area
            ufs_sb.fs_cssize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x009C);                   /// <summary>0x009C size of cyl grp summary area
            ufs_sb.fs_cgsize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00A0);                   /// <summary>0x00A0 cylinder group size
            // these fields are derived from the hardware
            ufs_sb.fs_ntrak = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00A4);                    /// <summary>0x00A4 tracks per cylinder
            ufs_sb.fs_nsect = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00A8);                    /// <summary>0x00A8 sectors per track
            ufs_sb.fs_spc = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00AC);                      /// <summary>0x00AC sectors per cylinder
            // this comes from the disk driver partitioning
            ufs_sb.fs_ncyl = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00B0);                     /// <summary>0x00B0 cylinders in file system
            // these fields can be computed from the others
            ufs_sb.fs_cpg = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00B4);                      /// <summary>0x00B4 cylinders per group
            ufs_sb.fs_ipg = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00B8);                      /// <summary>0x00B8 inodes per cylinder group
            ufs_sb.fs_fpg = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00BC);                      /// <summary>0x00BC blocks per group * fs_frag
            // this data must be re-computed after crashes
            // struct ufs_csum fs_cstotal = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0000); // cylinder summary information
            ufs_sb.fs_cstotal_ndir = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00C0);             /// <summary>0x00C0 number of directories
            ufs_sb.fs_cstotal_nbfree = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00C4);           /// <summary>0x00C4 number of free blocks
            ufs_sb.fs_cstotal_nifree = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00C8);           /// <summary>0x00C8 number of free inodes
            ufs_sb.fs_cstotal_nffree = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00CC);           /// <summary>0x00CC number of free frags
            // these fields are cleared at mount time
            ufs_sb.fs_fmod = ufs_sb_sectors[0x00D0];                       /// <summary>0x00D0 super block modified flag
            ufs_sb.fs_clean = ufs_sb_sectors[0x00D1];                      /// <summary>0x00D1 file system is clean flag
            ufs_sb.fs_ronly = ufs_sb_sectors[0x00D2];                      /// <summary>0x00D2 mounted read-only flag
            ufs_sb.fs_flags = ufs_sb_sectors[0x00D3];                      /// <summary>0x00D3
            #endregion COMMON
            #region UFS1
            strings_b = new byte[512];
            Array.Copy(ufs_sb_sectors, 0x00D4, strings_b, 0, 512);
            ufs_sb.fs_fsmnt_ufs1 = StringHandlers.CToString(strings_b);               /// <summary>0x00D4, 512 bytes, name mounted on
            ufs_sb.fs_cgrotor_ufs1 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0000);             /// <summary>0x02D4 last cg searched
            ufs_sb.fs_cs_ufs1 = new byte[124];
            Array.Copy(ufs_sb_sectors, 0x02D8, ufs_sb.fs_cs_ufs1, 0, 124); /// <summary>0x02D8, 124 bytes, uints, list of fs_cs info buffers
            ufs_sb.fs_maxcluster_ufs1 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0354);          /// <summary>0x0354
            ufs_sb.fs_cpc_ufs1 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0358);                 /// <summary>0x0358 cyl per cycle in postbl
            ufs_sb.fs_opostbl_ufs1 = new byte[256];
            Array.Copy(ufs_sb_sectors, 0x035C, ufs_sb.fs_opostbl_ufs1, 0, 256); /// <summary>0x035C, 256 bytes, [16][8] matrix of ushorts, old rotation block list head
            #endregion UFS1
            #region UFS2
            strings_b = new byte[468];
            Array.Copy(ufs_sb_sectors, 0x00D4, strings_b, 0, 468);
            ufs_sb.fs_fsmnt_ufs2 = StringHandlers.CToString(strings_b);               /// <summary>0x00D4, 468 bytes, name mounted on
            strings_b = new byte[32];
            Array.Copy(ufs_sb_sectors, 0x02A8, strings_b, 0, 32);
            ufs_sb.fs_volname_ufs2 = StringHandlers.CToString(strings_b);             /// <summary>0x02A8, 32 bytes, volume name
            ufs_sb.fs_swuid_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x02C8);               /// <summary>0x02C8 system-wide uid
            ufs_sb.fs_pad_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x02D0);                 /// <summary>0x02D0 due to alignment of fs_swuid
            ufs_sb.fs_cgrotor_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x02D4);             /// <summary>0x02D4 last cg searched
            ufs_sb.fs_ocsp_ufs2 = new byte[112];
            Array.Copy(ufs_sb_sectors, 0x02D8, ufs_sb.fs_ocsp_ufs2, 0, 112); /// <summary>0x02D8, 112 bytes, uints, list of fs_cs info buffers
            ufs_sb.fs_contigdirs_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0348);          /// <summary>0x0348 # of contiguously allocated dirs
            ufs_sb.fs_csp_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x034C);                 /// <summary>0x034C cg summary info buffer for fs_cs
            ufs_sb.fs_maxcluster_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0350);          /// <summary>0x0350
            ufs_sb.fs_active_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0354);              /// <summary>0x0354 used by snapshots to track fs
            ufs_sb.fs_old_cpc_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0358);             /// <summary>0x0358 cyl per cycle in postbl
            ufs_sb.fs_maxbsize_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x035C);            /// <summary>0x035C maximum blocking factor permitted
            ufs_sb.fs_sparecon64_ufs2 = new byte[136];
            Array.Copy(ufs_sb_sectors, 0x0360, ufs_sb.fs_sparecon64_ufs2, 0, 136); /// <summary>0x0360, 136 bytes, ulongs, old rotation block list head
            ufs_sb.fs_sblockloc_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x03E8);           /// <summary>0x03E8 byte offset of standard superblock
            //cylinder summary information*/
            ufs_sb.fs_cstotal_ndir_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x03F0);        /// <summary>0x03F0 number of directories
            ufs_sb.fs_cstotal_nbfree_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x03F8);      /// <summary>0x03F8 number of free blocks
            ufs_sb.fs_cstotal_nifree_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0400);      /// <summary>0x0400 number of free inodes
            ufs_sb.fs_cstotal_nffree_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0408);      /// <summary>0x0408 number of free frags
            ufs_sb.fs_cstotal_numclusters_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0410); /// <summary>0x0410 number of free clusters
            ufs_sb.fs_cstotal_spare0_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0418);      /// <summary>0x0418 future expansion
            ufs_sb.fs_cstotal_spare1_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0420);      /// <summary>0x0420 future expansion
            ufs_sb.fs_cstotal_spare2_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0428);      /// <summary>0x0428 future expansion
            ufs_sb.fs_time_sec_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0430);            /// <summary>0x0430 last time written
            ufs_sb.fs_time_usec_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0434);           /// <summary>0x0434 last time written
            ufs_sb.fs_size_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0438);                /// <summary>0x0438 number of blocks in fs
            ufs_sb.fs_dsize_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0440);               /// <summary>0x0440 number of data blocks in fs
            ufs_sb.fs_csaddr_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0448);              /// <summary>0x0448 blk addr of cyl grp summary area
            ufs_sb.fs_pendingblocks_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0450);       /// <summary>0x0450 blocks in process of being freed
            ufs_sb.fs_pendinginodes_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0458);       /// <summary>0x0458 inodes in process of being freed
            #endregion UFS2
            #region Sun
            ufs_sb.fs_sparecon_sun = new byte[212];
            Array.Copy(ufs_sb_sectors, 0x045C, ufs_sb.fs_sparecon_sun, 0, 212); /// <summary>0x045C, 212 bytes, reserved for future constants
            ufs_sb.fs_reclaim_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0530);              /// <summary>0x0530
            ufs_sb.fs_sparecon2_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0534);            /// <summary>0x0534
            ufs_sb.fs_state_t_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0538);              /// <summary>0x0538 file system state time stamp
            ufs_sb.fs_qbmask0_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x053C);              /// <summary>0x053C ~usb_bmask
            ufs_sb.fs_qbmask1_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0540);              /// <summary>0x0540 ~usb_bmask
            ufs_sb.fs_qfmask0_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0544);              /// <summary>0x0544 ~usb_fmask
            ufs_sb.fs_qfmask1_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0548);              /// <summary>0x0548 ~usb_fmask
            #endregion Sun
            #region Sunx86
            ufs_sb.fs_sparecon_sun86 = new byte[212];
            Array.Copy(ufs_sb_sectors, 0x045C, ufs_sb.fs_sparecon_sun86, 0, 212); /// <summary>0x045C, 212 bytes, reserved for future constants
            ufs_sb.fs_reclaim_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0530);            /// <summary>0x0530
            ufs_sb.fs_sparecon2_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0534);          /// <summary>0x0534
            ufs_sb.fs_npsect_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0538);             /// <summary>0x0538 # sectors/track including spares
            ufs_sb.fs_qbmask0_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x053C);            /// <summary>0x053C ~usb_bmask
            ufs_sb.fs_qbmask1_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0540);            /// <summary>0x0540 ~usb_bmask
            ufs_sb.fs_qfmask0_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0544);            /// <summary>0x0544 ~usb_fmask
            ufs_sb.fs_qfmask1_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0548);            /// <summary>0x0548 ~usb_fmask
            #endregion Sunx86
            #region 44BSD
            ufs_sb.fs_sparecon_44bsd = new byte[200];
            Array.Copy(ufs_sb_sectors, 0x045C, ufs_sb.fs_sparecon_44bsd, 0, 200); /// <summary>0x045C, 200 bytes
            ufs_sb.fs_contigsumsize_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0524);      /// <summary>0x0524 size of cluster summary array
            ufs_sb.fs_maxsymlinklen_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0528);      /// <summary>0x0528 max length of an internal symlink
            ufs_sb.fs_inodefmt_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x052C);           /// <summary>0x052C format of on-disk inodes
            ufs_sb.fs_maxfilesize0_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0530);       /// <summary>0x0530 max representable file size
            ufs_sb.fs_maxfilesize1_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0534);       /// <summary>0x0534 max representable file size
            ufs_sb.fs_qbmask0_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0538);            /// <summary>0x0538 ~usb_bmask
            ufs_sb.fs_qbmask1_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x053C);            /// <summary>0x053C ~usb_bmask
            ufs_sb.fs_qfmask0_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0540);            /// <summary>0x0540 ~usb_fmask
            ufs_sb.fs_qfmask1_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0544);            /// <summary>0x0544 ~usb_fmask
            ufs_sb.fs_state_t_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0548);              /// <summary>0x0548 file system state time stamp
            #endregion 44BSD
            ufs_sb.fs_postblformat = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x054C);             /// <summary>0x054C format of positional layout tables
            ufs_sb.fs_nrpos = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0550);                    /// <summary>0x0550 number of rotational positions
            ufs_sb.fs_postbloff = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0554);                /// <summary>0x0554 (__s16) rotation block list head
            ufs_sb.fs_rotbloff = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0558);                 /// <summary>0x0558 (__u8) blocks for each rotation
            ufs_sb.fs_magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);                    /// <summary>0x055C magic number
            ufs_sb.fs_space = ufs_sb_sectors[0x0560];                    /// <summary>0x0560 list of blocks for each rotation

            DicConsole.DebugWriteLine("FFS plugin", "ufs_sb offset: 0x{0:X8}", sb_offset);
            DicConsole.DebugWriteLine("FFS plugin", "fs_link_42bsd: 0x{0:X8}", ufs_sb.fs_link_42bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_state_sun: 0x{0:X8}", ufs_sb.fs_state_sun);
            DicConsole.DebugWriteLine("FFS plugin", "fs_rlink: 0x{0:X8}", ufs_sb.fs_rlink);
            DicConsole.DebugWriteLine("FFS plugin", "fs_sblkno: 0x{0:X8}", ufs_sb.fs_sblkno);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cblkno: 0x{0:X8}", ufs_sb.fs_cblkno);
            DicConsole.DebugWriteLine("FFS plugin", "fs_iblkno: 0x{0:X8}", ufs_sb.fs_iblkno);
            DicConsole.DebugWriteLine("FFS plugin", "fs_dblkno: 0x{0:X8}", ufs_sb.fs_dblkno);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cgoffset: 0x{0:X8}", ufs_sb.fs_cgoffset);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cgmask: 0x{0:X8}", ufs_sb.fs_cgmask);
            DicConsole.DebugWriteLine("FFS plugin", "fs_time_t: 0x{0:X8}", ufs_sb.fs_time_t);
            DicConsole.DebugWriteLine("FFS plugin", "fs_size: 0x{0:X8}", ufs_sb.fs_size);
            DicConsole.DebugWriteLine("FFS plugin", "fs_dsize: 0x{0:X8}", ufs_sb.fs_dsize);
            DicConsole.DebugWriteLine("FFS plugin", "fs_ncg: 0x{0:X8}", ufs_sb.fs_ncg);
            DicConsole.DebugWriteLine("FFS plugin", "fs_bsize: 0x{0:X8}", ufs_sb.fs_bsize);
            DicConsole.DebugWriteLine("FFS plugin", "fs_fsize: 0x{0:X8}", ufs_sb.fs_fsize);
            DicConsole.DebugWriteLine("FFS plugin", "fs_frag: 0x{0:X8}", ufs_sb.fs_frag);
            DicConsole.DebugWriteLine("FFS plugin", "fs_minfree: 0x{0:X8}", ufs_sb.fs_minfree);
            DicConsole.DebugWriteLine("FFS plugin", "fs_rotdelay: 0x{0:X8}", ufs_sb.fs_rotdelay);
            DicConsole.DebugWriteLine("FFS plugin", "fs_rps: 0x{0:X8}", ufs_sb.fs_rps);
            DicConsole.DebugWriteLine("FFS plugin", "fs_bmask: 0x{0:X8}", ufs_sb.fs_bmask);
            DicConsole.DebugWriteLine("FFS plugin", "fs_fmask: 0x{0:X8}", ufs_sb.fs_fmask);
            DicConsole.DebugWriteLine("FFS plugin", "fs_bshift: 0x{0:X8}", ufs_sb.fs_bshift);
            DicConsole.DebugWriteLine("FFS plugin", "fs_fshift: 0x{0:X8}", ufs_sb.fs_fshift);
            DicConsole.DebugWriteLine("FFS plugin", "fs_maxcontig: 0x{0:X8}", ufs_sb.fs_maxcontig);
            DicConsole.DebugWriteLine("FFS plugin", "fs_maxbpg: 0x{0:X8}", ufs_sb.fs_maxbpg);
            DicConsole.DebugWriteLine("FFS plugin", "fs_fragshift: 0x{0:X8}", ufs_sb.fs_fragshift);
            DicConsole.DebugWriteLine("FFS plugin", "fs_fsbtodb: 0x{0:X8}", ufs_sb.fs_fsbtodb);
            DicConsole.DebugWriteLine("FFS plugin", "fs_sbsize: 0x{0:X8}", ufs_sb.fs_sbsize);
            DicConsole.DebugWriteLine("FFS plugin", "fs_csmask: 0x{0:X8}", ufs_sb.fs_csmask);
            DicConsole.DebugWriteLine("FFS plugin", "fs_csshift: 0x{0:X8}", ufs_sb.fs_csshift);
            DicConsole.DebugWriteLine("FFS plugin", "fs_nindir: 0x{0:X8}", ufs_sb.fs_nindir);
            DicConsole.DebugWriteLine("FFS plugin", "fs_inopb: 0x{0:X8}", ufs_sb.fs_inopb);
            DicConsole.DebugWriteLine("FFS plugin", "fs_nspf: 0x{0:X8}", ufs_sb.fs_nspf);
            DicConsole.DebugWriteLine("FFS plugin", "fs_optim: 0x{0:X8}", ufs_sb.fs_optim);
            DicConsole.DebugWriteLine("FFS plugin", "fs_npsect_sun: 0x{0:X8}", ufs_sb.fs_npsect_sun);
            DicConsole.DebugWriteLine("FFS plugin", "fs_state_t_sun86: 0x{0:X8}", ufs_sb.fs_state_t_sun86);
            DicConsole.DebugWriteLine("FFS plugin", "fs_interleave: 0x{0:X8}", ufs_sb.fs_interleave);
            DicConsole.DebugWriteLine("FFS plugin", "fs_trackskew: 0x{0:X8}", ufs_sb.fs_trackskew);
            DicConsole.DebugWriteLine("FFS plugin", "fs_id_1: 0x{0:X8}", ufs_sb.fs_id_1);
            DicConsole.DebugWriteLine("FFS plugin", "fs_id_2: 0x{0:X8}", ufs_sb.fs_id_2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_headswitch_43bsd: 0x{0:X8}", ufs_sb.fs_headswitch_43bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_trkseek_43bsd: 0x{0:X8}", ufs_sb.fs_trkseek_43bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_csaddr: 0x{0:X8}", ufs_sb.fs_csaddr);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cssize: 0x{0:X8}", ufs_sb.fs_cssize);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cgsize: 0x{0:X8}", ufs_sb.fs_cgsize);
            DicConsole.DebugWriteLine("FFS plugin", "fs_ntrak: 0x{0:X8}", ufs_sb.fs_ntrak);
            DicConsole.DebugWriteLine("FFS plugin", "fs_nsect: 0x{0:X8}", ufs_sb.fs_nsect);
            DicConsole.DebugWriteLine("FFS plugin", "fs_spc: 0x{0:X8}", ufs_sb.fs_spc);
            DicConsole.DebugWriteLine("FFS plugin", "fs_ncyl: 0x{0:X8}", ufs_sb.fs_ncyl);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cpg: 0x{0:X8}", ufs_sb.fs_cpg);
            DicConsole.DebugWriteLine("FFS plugin", "fs_ipg: 0x{0:X8}", ufs_sb.fs_ipg);
            DicConsole.DebugWriteLine("FFS plugin", "fs_fpg: 0x{0:X8}", ufs_sb.fs_fpg);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_ndir: 0x{0:X8}", ufs_sb.fs_cstotal_ndir);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_nbfree: 0x{0:X8}", ufs_sb.fs_cstotal_nbfree);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_nifree: 0x{0:X8}", ufs_sb.fs_cstotal_nifree);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_nffree: 0x{0:X8}", ufs_sb.fs_cstotal_nffree);
            DicConsole.DebugWriteLine("FFS plugin", "fs_fmod: 0x{0:X2}", ufs_sb.fs_fmod);
            DicConsole.DebugWriteLine("FFS plugin", "fs_clean: 0x{0:X2}", ufs_sb.fs_clean);
            DicConsole.DebugWriteLine("FFS plugin", "fs_ronly: 0x{0:X2}", ufs_sb.fs_ronly);
            DicConsole.DebugWriteLine("FFS plugin", "fs_flags: 0x{0:X2}", ufs_sb.fs_flags);
            DicConsole.DebugWriteLine("FFS plugin", "fs_fsmnt_ufs1: {0}", ufs_sb.fs_fsmnt_ufs1);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cgrotor_ufs1: 0x{0:X8}", ufs_sb.fs_cgrotor_ufs1);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cs_ufs1: 0x{0:X}", ufs_sb.fs_cs_ufs1);
            DicConsole.DebugWriteLine("FFS plugin", "fs_maxcluster_ufs1: 0x{0:X8}", ufs_sb.fs_maxcluster_ufs1);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cpc_ufs1: 0x{0:X8}", ufs_sb.fs_cpc_ufs1);
            DicConsole.DebugWriteLine("FFS plugin", "fs_opostbl_ufs1: 0x{0:X}", ufs_sb.fs_opostbl_ufs1);
            DicConsole.DebugWriteLine("FFS plugin", "fs_fsmnt_ufs2: {0}", ufs_sb.fs_fsmnt_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_volname_ufs2: {0}", ufs_sb.fs_volname_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_swuid_ufs2: 0x{0:X16}", ufs_sb.fs_swuid_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_pad_ufs2: 0x{0:X8}", ufs_sb.fs_pad_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cgrotor_ufs2: 0x{0:X8}", ufs_sb.fs_cgrotor_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_ocsp_ufs2: 0x{0:X}", ufs_sb.fs_ocsp_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_contigdirs_ufs2: 0x{0:X8}", ufs_sb.fs_contigdirs_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_csp_ufs2: 0x{0:X8}", ufs_sb.fs_csp_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_maxcluster_ufs2: 0x{0:X8}", ufs_sb.fs_maxcluster_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_active_ufs2: 0x{0:X8}", ufs_sb.fs_active_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_old_cpc_ufs2: 0x{0:X8}", ufs_sb.fs_old_cpc_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_maxbsize_ufs2: 0x{0:X8}", ufs_sb.fs_maxbsize_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_sparecon64_ufs2: 0x{0:X}", ufs_sb.fs_sparecon64_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_sblockloc_ufs2: 0x{0:X16}", ufs_sb.fs_sblockloc_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_ndir_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_ndir_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_nbfree_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_nbfree_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_nifree_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_nifree_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_nffree_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_nffree_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_numclusters_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_numclusters_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_spare0_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_spare0_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_spare1_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_spare1_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_cstotal_spare2_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_spare2_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_time_sec_ufs2: 0x{0:X8}", ufs_sb.fs_time_sec_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_time_usec_ufs2: 0x{0:X8}", ufs_sb.fs_time_usec_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_size_ufs2: 0x{0:X16}", ufs_sb.fs_size_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_dsize_ufs2: 0x{0:X16}", ufs_sb.fs_dsize_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_csaddr_ufs2: 0x{0:X16}", ufs_sb.fs_csaddr_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_pendingblocks_ufs2: 0x{0:X16}", ufs_sb.fs_pendingblocks_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_pendinginodes_ufs2: 0x{0:X8}", ufs_sb.fs_pendinginodes_ufs2);
            DicConsole.DebugWriteLine("FFS plugin", "fs_sparecon_sun: 0x{0:X}", ufs_sb.fs_sparecon_sun);
            DicConsole.DebugWriteLine("FFS plugin", "fs_reclaim_sun: 0x{0:X8}", ufs_sb.fs_reclaim_sun);
            DicConsole.DebugWriteLine("FFS plugin", "fs_sparecon2_sun: 0x{0:X8}", ufs_sb.fs_sparecon2_sun);
            DicConsole.DebugWriteLine("FFS plugin", "fs_state_t_sun: 0x{0:X8}", ufs_sb.fs_state_t_sun);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qbmask0_sun: 0x{0:X8}", ufs_sb.fs_qbmask0_sun);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qbmask1_sun: 0x{0:X8}", ufs_sb.fs_qbmask1_sun);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qfmask0_sun: 0x{0:X8}", ufs_sb.fs_qfmask0_sun);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qfmask1_sun: 0x{0:X8}", ufs_sb.fs_qfmask1_sun);
            DicConsole.DebugWriteLine("FFS plugin", "fs_sparecon_sun86: 0x{0:X}", ufs_sb.fs_sparecon_sun86);
            DicConsole.DebugWriteLine("FFS plugin", "fs_reclaim_sun86: 0x{0:X8}", ufs_sb.fs_reclaim_sun86);
            DicConsole.DebugWriteLine("FFS plugin", "fs_sparecon2_sun86: 0x{0:X8}", ufs_sb.fs_sparecon2_sun86);
            DicConsole.DebugWriteLine("FFS plugin", "fs_npsect_sun86: 0x{0:X8}", ufs_sb.fs_npsect_sun86);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qbmask0_sun86: 0x{0:X8}", ufs_sb.fs_qbmask0_sun86);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qbmask1_sun86: 0x{0:X8}", ufs_sb.fs_qbmask1_sun86);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qfmask0_sun86: 0x{0:X8}", ufs_sb.fs_qfmask0_sun86);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qfmask1_sun86: 0x{0:X8}", ufs_sb.fs_qfmask1_sun86);
            DicConsole.DebugWriteLine("FFS plugin", "fs_sparecon_44bsd: 0x{0:X}", ufs_sb.fs_sparecon_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_contigsumsize_44bsd: 0x{0:X8}", ufs_sb.fs_contigsumsize_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_maxsymlinklen_44bsd: 0x{0:X8}", ufs_sb.fs_maxsymlinklen_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_inodefmt_44bsd: 0x{0:X8}", ufs_sb.fs_inodefmt_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_maxfilesize0_44bsd: 0x{0:X8}", ufs_sb.fs_maxfilesize0_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_maxfilesize1_44bsd: 0x{0:X8}", ufs_sb.fs_maxfilesize1_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qbmask0_44bsd: 0x{0:X8}", ufs_sb.fs_qbmask0_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qbmask1_44bsd: 0x{0:X8}", ufs_sb.fs_qbmask1_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qfmask0_44bsd: 0x{0:X8}", ufs_sb.fs_qfmask0_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_qfmask1_44bsd: 0x{0:X8}", ufs_sb.fs_qfmask1_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_state_t_44bsd: 0x{0:X8}", ufs_sb.fs_state_t_44bsd);
            DicConsole.DebugWriteLine("FFS plugin", "fs_postblformat: 0x{0:X8}", ufs_sb.fs_postblformat);
            DicConsole.DebugWriteLine("FFS plugin", "fs_nrpos: 0x{0:X8}", ufs_sb.fs_nrpos);
            DicConsole.DebugWriteLine("FFS plugin", "fs_postbloff: 0x{0:X8}", ufs_sb.fs_postbloff);
            DicConsole.DebugWriteLine("FFS plugin", "fs_rotbloff: 0x{0:X8}", ufs_sb.fs_rotbloff);
            DicConsole.DebugWriteLine("FFS plugin", "fs_magic: 0x{0:X8}", ufs_sb.fs_magic);
            DicConsole.DebugWriteLine("FFS plugin", "fs_space: 0x{0:X2}", ufs_sb.fs_space);

            sbInformation.AppendLine("There are a lot of variants of UFS using overlapped values on same fields");
            sbInformation.AppendLine("I will try to guess which one it is, but unless it's UFS2, I may be surely wrong");

            if(ufs_sb.fs_magic == UFS2_MAGIC)
            {
                fs_type_ufs2 = true;
            }
            else
            {
                const uint SunOSEpoch = 0x1A54C580; // We are supposing there cannot be a Sun's fs created before 1/1/1982 00:00:00

                fs_type_43bsd = true; // There is no way of knowing this is the version, but there is of knowing it is not.

                if(ufs_sb.fs_link_42bsd > 0)
                {
                    fs_type_42bsd = true; // It was used in 4.2BSD
                    fs_type_43bsd = false;
                }

                if(ufs_sb.fs_state_t_sun > SunOSEpoch && DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_state_t_sun) < DateTime.Now)
                {
                    fs_type_42bsd = false;
                    fs_type_sun = true;
                    fs_type_43bsd = false;
                }

                // This is for sure, as it is shared with a sectors/track with non-x86 SunOS, Epoch is absurdly high for that
                if(ufs_sb.fs_state_t_sun86 > SunOSEpoch && DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_state_t_sun) < DateTime.Now)
                {
                    fs_type_42bsd = false;
                    fs_type_sun86 = true;
                    fs_type_sun = false;
                    fs_type_43bsd = false;
                }

                if(ufs_sb.fs_cgrotor_ufs1 > 0x00000000 && ufs_sb.fs_cgrotor_ufs1 < 0xFFFFFFFF)
                {
                    fs_type_42bsd = false;
                    fs_type_sun = false;
                    fs_type_sun86 = false;
                    fs_type_ufs = true;
                    fs_type_43bsd = false;
                }

                // 4.3BSD code does not use these fields, they are always set up to 0
                fs_type_43bsd &= ufs_sb.fs_trkseek_43bsd == 0 && ufs_sb.fs_headswitch_43bsd == 0;

                // This is the only 4.4BSD inode format
                fs_type_44bsd |= ufs_sb.fs_inodefmt_44bsd == 2;
            }

            if(fs_type_42bsd)
                sbInformation.AppendLine("Guessed as 42BSD FFS");
            if(fs_type_43bsd)
                sbInformation.AppendLine("Guessed as 43BSD FFS");
            if(fs_type_44bsd)
                sbInformation.AppendLine("Guessed as 44BSD FFS");
            if(fs_type_sun)
                sbInformation.AppendLine("Guessed as SunOS FFS");
            if(fs_type_sun86)
                sbInformation.AppendLine("Guessed as SunOS/x86 FFS");
            if(fs_type_ufs)
                sbInformation.AppendLine("Guessed as UFS");
            if(fs_type_ufs2)
                sbInformation.AppendLine("Guessed as UFS2");

            if(fs_type_42bsd)
                sbInformation.AppendFormat("Linked list of filesystems: 0x{0:X8}", ufs_sb.fs_link_42bsd).AppendLine();
            else if(fs_type_sun)
                sbInformation.AppendFormat("Filesystem state flag: 0x{0:X8}", ufs_sb.fs_state_sun).AppendLine();
            sbInformation.AppendFormat("Superblock LBA: {0}", ufs_sb.fs_sblkno).AppendLine();
            sbInformation.AppendFormat("Cylinder-block LBA: {0}", ufs_sb.fs_cblkno).AppendLine();
            sbInformation.AppendFormat("inode-block LBA: {0}", ufs_sb.fs_iblkno).AppendLine();
            sbInformation.AppendFormat("First data block LBA: {0}", ufs_sb.fs_dblkno).AppendLine();
            sbInformation.AppendFormat("Cylinder group offset in cylinder: {0}", ufs_sb.fs_cgoffset).AppendLine();
            sbInformation.AppendFormat("Volume last written on {0}", DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_time_t)).AppendLine();
            sbInformation.AppendFormat("{0} blocks in volume ({1} bytes)", ufs_sb.fs_size, ufs_sb.fs_size * 1024L).AppendLine();
            xmlFSType.Clusters = ufs_sb.fs_size;
            xmlFSType.ClusterSize = (int)ufs_sb.fs_bsize;
            sbInformation.AppendFormat("{0} data blocks in volume ({1} bytes)", ufs_sb.fs_dsize, ufs_sb.fs_dsize * 1024L).AppendLine();
            sbInformation.AppendFormat("{0} cylinder groups in volume", ufs_sb.fs_ncg).AppendLine();
            sbInformation.AppendFormat("{0} bytes in a basic block", ufs_sb.fs_bsize).AppendLine();
            sbInformation.AppendFormat("{0} bytes in a frag block", ufs_sb.fs_fsize).AppendLine();
            sbInformation.AppendFormat("{0} frags in a block", ufs_sb.fs_frag).AppendLine();
            sbInformation.AppendFormat("{0}% of blocks must be free", ufs_sb.fs_minfree).AppendLine();
            sbInformation.AppendFormat("{0}ms for optimal next block", ufs_sb.fs_rotdelay).AppendLine();
            sbInformation.AppendFormat("disk rotates {0} times per second ({1}rpm)", ufs_sb.fs_rps, ufs_sb.fs_rps * 60).AppendLine();
            /*          sbInformation.AppendFormat("fs_bmask: 0x{0:X8}", ufs_sb.fs_bmask).AppendLine();
                        sbInformation.AppendFormat("fs_fmask: 0x{0:X8}", ufs_sb.fs_fmask).AppendLine();
                        sbInformation.AppendFormat("fs_bshift: 0x{0:X8}", ufs_sb.fs_bshift).AppendLine();
                        sbInformation.AppendFormat("fs_fshift: 0x{0:X8}", ufs_sb.fs_fshift).AppendLine();*/
            sbInformation.AppendFormat("{0} contiguous blocks at maximum", ufs_sb.fs_maxcontig).AppendLine();
            sbInformation.AppendFormat("{0} blocks per cylinder group at maximum", ufs_sb.fs_maxbpg).AppendLine();
            sbInformation.AppendFormat("Superblock is {0} bytes", ufs_sb.fs_sbsize).AppendLine();
            sbInformation.AppendFormat("NINDIR: 0x{0:X8}", ufs_sb.fs_nindir).AppendLine();
            sbInformation.AppendFormat("INOPB: 0x{0:X8}", ufs_sb.fs_inopb).AppendLine();
            sbInformation.AppendFormat("NSPF: 0x{0:X8}", ufs_sb.fs_nspf).AppendLine();
            if(ufs_sb.fs_optim == 0)
                sbInformation.AppendLine("Filesystem will minimize allocation time");
            else if(ufs_sb.fs_optim == 1)
                sbInformation.AppendLine("Filesystem will minimize volume fragmentation");
            else
                sbInformation.AppendFormat("Unknown optimization value: 0x{0:X8}", ufs_sb.fs_optim).AppendLine();
            if(fs_type_sun)
                sbInformation.AppendFormat("{0} sectors/track", ufs_sb.fs_npsect_sun).AppendLine();
            else if(fs_type_sun86)
                sbInformation.AppendFormat("Volume state on {0}", DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_state_t_sun86)).AppendLine();
            sbInformation.AppendFormat("Hardware sector interleave: {0}", ufs_sb.fs_interleave).AppendLine();
            sbInformation.AppendFormat("Sector 0 skew: {0}/track", ufs_sb.fs_trackskew).AppendLine();
            if(!fs_type_43bsd && ufs_sb.fs_id_1 > 0 && ufs_sb.fs_id_2 > 0)
            {
                sbInformation.AppendFormat("Volume ID: 0x{0:X8}{1:X8}", ufs_sb.fs_id_1, ufs_sb.fs_id_2).AppendLine();
                xmlFSType.VolumeSerial = string.Format("{0:X8}{1:x8}", ufs_sb.fs_id_1, ufs_sb.fs_id_2);
            }
            else if(fs_type_43bsd && ufs_sb.fs_headswitch_43bsd > 0 && ufs_sb.fs_trkseek_43bsd > 0)
            {
                sbInformation.AppendFormat("{0} µsec for head switch", ufs_sb.fs_headswitch_43bsd).AppendLine();
                sbInformation.AppendFormat("{0} µsec for track-to-track seek", ufs_sb.fs_trkseek_43bsd).AppendLine();
            }
            sbInformation.AppendFormat("Cylinder group summary LBA: {0}", ufs_sb.fs_csaddr).AppendLine();
            sbInformation.AppendFormat("{0} bytes in cylinder group summary", ufs_sb.fs_cssize).AppendLine();
            sbInformation.AppendFormat("{0} bytes in cylinder group", ufs_sb.fs_cgsize).AppendLine();
            sbInformation.AppendFormat("{0} tracks/cylinder", ufs_sb.fs_ntrak).AppendLine();
            sbInformation.AppendFormat("{0} sectors/track", ufs_sb.fs_nsect).AppendLine();
            sbInformation.AppendFormat("{0} sectors/cylinder", ufs_sb.fs_spc).AppendLine();
            sbInformation.AppendFormat("{0} cylinder in volume", ufs_sb.fs_ncyl).AppendLine();
            sbInformation.AppendFormat("{0} cylinders/group", ufs_sb.fs_cpg).AppendLine();
            sbInformation.AppendFormat("{0} inodes per cylinder group", ufs_sb.fs_ipg).AppendLine();
            sbInformation.AppendFormat("{0} blocks per group", ufs_sb.fs_fpg / ufs_sb.fs_frag).AppendLine();
            sbInformation.AppendFormat("{0} directories", ufs_sb.fs_cstotal_ndir).AppendLine();
            sbInformation.AppendFormat("{0} free blocks ({1} bytes)", ufs_sb.fs_cstotal_nbfree, ufs_sb.fs_cstotal_nbfree * ufs_sb.fs_bsize).AppendLine();
            xmlFSType.FreeClusters = ufs_sb.fs_cstotal_nbfree;
            xmlFSType.FreeClustersSpecified = true;
            sbInformation.AppendFormat("{0} free inodes", ufs_sb.fs_cstotal_nifree).AppendLine();
            sbInformation.AppendFormat("{0} free frags", ufs_sb.fs_cstotal_nffree).AppendLine();
            if(ufs_sb.fs_fmod == 1)
            {
                sbInformation.AppendLine("Superblock is under modification");
                xmlFSType.Dirty = true;
            }
            if(ufs_sb.fs_clean == 1)
                sbInformation.AppendLine("Volume is clean");
            if(ufs_sb.fs_ronly == 1)
                sbInformation.AppendLine("Volume is read-only");
            sbInformation.AppendFormat("Volume flags: 0x{0:X2}", ufs_sb.fs_flags).AppendLine();
            if(fs_type_ufs)
            {
                sbInformation.AppendFormat("Volume last mounted on \"{0}\"", ufs_sb.fs_fsmnt_ufs1).AppendLine();
                sbInformation.AppendFormat("Last searched cylinder group: {0}", ufs_sb.fs_cgrotor_ufs1).AppendLine();
            }
            else if(fs_type_ufs2)
            {
                sbInformation.AppendFormat("Volume last mounted on \"{0}\"", ufs_sb.fs_fsmnt_ufs2).AppendLine();
                sbInformation.AppendFormat("Volume name: \"{0}\"", ufs_sb.fs_volname_ufs2).AppendLine();
                xmlFSType.VolumeName = ufs_sb.fs_volname_ufs2;
                sbInformation.AppendFormat("Volume ID: 0x{0:X16}", ufs_sb.fs_swuid_ufs2).AppendLine();
                xmlFSType.VolumeSerial = string.Format("{0:X16}", ufs_sb.fs_swuid_ufs2);
                sbInformation.AppendFormat("Last searched cylinder group: {0}", ufs_sb.fs_cgrotor_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} contiguously allocated directories", ufs_sb.fs_contigdirs_ufs2).AppendLine();
                sbInformation.AppendFormat("Standard superblock LBA: {0}", ufs_sb.fs_sblockloc_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} directories", ufs_sb.fs_cstotal_ndir_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} free blocks ({1} bytes)", ufs_sb.fs_cstotal_nbfree_ufs2, ufs_sb.fs_cstotal_nbfree_ufs2 * ufs_sb.fs_bsize).AppendLine();
                xmlFSType.FreeClusters = (long)ufs_sb.fs_cstotal_nbfree_ufs2;
                xmlFSType.FreeClustersSpecified = true;
                sbInformation.AppendFormat("{0} free inodes", ufs_sb.fs_cstotal_nifree_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} free frags", ufs_sb.fs_cstotal_nffree_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} free clusters", ufs_sb.fs_cstotal_numclusters_ufs2).AppendLine();
                sbInformation.AppendFormat("Volume last written on {0}", DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_time_sec_ufs2)).AppendLine();
                xmlFSType.ModificationDate = DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_time_sec_ufs2);
                xmlFSType.ModificationDateSpecified = true;
                sbInformation.AppendFormat("{0} blocks ({1} bytes)", ufs_sb.fs_size_ufs2, ufs_sb.fs_size_ufs2 * ufs_sb.fs_bsize).AppendLine();
                xmlFSType.Clusters = (long)ufs_sb.fs_dsize_ufs2;
                sbInformation.AppendFormat("{0} data blocks ({1} bytes)", ufs_sb.fs_dsize_ufs2, ufs_sb.fs_dsize_ufs2 * ufs_sb.fs_bsize).AppendLine();
                sbInformation.AppendFormat("Cylinder group summary area LBA: {0}", ufs_sb.fs_csaddr_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} blocks pending of being freed", ufs_sb.fs_pendingblocks_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} inodes pending of being freed", ufs_sb.fs_pendinginodes_ufs2).AppendLine();
            }
            if(fs_type_sun)
            {
                sbInformation.AppendFormat("Volume state on {0}", DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_state_t_sun)).AppendLine();
            }
            else if(fs_type_sun86)
            {
                sbInformation.AppendFormat("{0} sectors/track", ufs_sb.fs_npsect_sun86).AppendLine();
            }
            else if(fs_type_44bsd)
            {
                sbInformation.AppendFormat("{0} blocks on cluster summary array", ufs_sb.fs_contigsumsize_44bsd).AppendLine();
                sbInformation.AppendFormat("Maximum length of a symbolic link: {0}", ufs_sb.fs_maxsymlinklen_44bsd).AppendLine();
                ulong bsd44_maxfilesize = ((ulong)ufs_sb.fs_maxfilesize0_44bsd) * 0x100000000 + ufs_sb.fs_maxfilesize1_44bsd;
                sbInformation.AppendFormat("A file can be {0} bytes at max", bsd44_maxfilesize).AppendLine();
                sbInformation.AppendFormat("Volume state on {0}", DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_state_t_44bsd)).AppendLine();
            }
            sbInformation.AppendFormat("{0} rotational positions", ufs_sb.fs_nrpos).AppendLine();
            sbInformation.AppendFormat("{0} blocks per rotation", ufs_sb.fs_rotbloff).AppendLine();

            information = sbInformation.ToString();
        }

        const uint block_size = 8192;

        // As specified in FreeBSD source code, FFS/UFS can start in any of four places
        // For floppies, start at offset 0
        const ulong sb_start_floppy = 0;
        // For normal devices, start at offset 8192
        const ulong sb_start_ufs1 = 1;
        // For UFS2, start at offset 65536
        const ulong sb_start_ufs2 = 8;
        // Atari strange starting for Atari UNIX, in bytes not blocks
        const ulong sb_start_atari = 110080;
        // For piggy devices (?), start at offset 262144
        const ulong sb_start_piggy = 32;

        // MAGICs
        // UFS magic
        const uint UFS_MAGIC = 0x00011954;
        // BorderWare UFS
        const uint UFS_MAGIC_BW = 0x0f242697;
        // UFS2 magic
        const uint UFS2_MAGIC = 0x19540119;
        // byteswapped
        const uint UFS_CIGAM = 0x54190100;
        // Incomplete newfs
        const uint UFS_BAD_MAGIC = 0x19960408;

        /// <summary>
        /// On-disk superblock is quite a mixture of all the UFS/FFS variants
        /// There is no clear way to detect which one is correct
        /// And as C# does not support unions this struct will clearly appear quite dirty :p
        /// To clean up things a little, comment starts with relative superblock offset of field
        /// Biggest sized supleblock would be 1377 bytes
        /// </summary>
        public struct UFSSuperBlock
        {
            #region 42BSD

            /// <summary>0x0000 linked list of file systems</summary>
            public uint fs_link_42bsd;

            #endregion

            #region Sun

            /// <summary>0x0000 file system state flag</summary>
            public uint fs_state_sun;

            #endregion

            #region COMMON

            /// <summary>0x0004 used for incore super blocks</summary>
            public uint fs_rlink;
            /// <summary>0x0008 addr of super-block in filesys</summary>
            public uint fs_sblkno;
            /// <summary>0x000C offset of cyl-block in filesys</summary>
            public uint fs_cblkno;
            /// <summary>0x0010 offset of inode-blocks in filesys</summary>
            public uint fs_iblkno;
            /// <summary>0x0014 offset of first data after cg</summary>
            public uint fs_dblkno;
            /// <summary>0x0018 cylinder group offset in cylinder</summary>
            public uint fs_cgoffset;
            /// <summary>0x001C used to calc mod fs_ntrak</summary>
            public uint fs_cgmask;
            /// <summary>0x0020 last time written -- time_t</summary>
            public uint fs_time_t;
            /// <summary>0x0024 number of blocks in fs</summary>
            public uint fs_size;
            /// <summary>0x0028 number of data blocks in fs</summary>
            public uint fs_dsize;
            /// <summary>0x002C number of cylinder groups</summary>
            public uint fs_ncg;
            /// <summary>0x0030 size of basic blocks in fs</summary>
            public uint fs_bsize;
            /// <summary>0x0034 size of frag blocks in fs</summary>
            public uint fs_fsize;
            /// <summary>0x0038 number of frags in a block in fs</summary>
            public uint fs_frag;

            // these are configuration parameters
            /// <summary>0x003C minimum percentage of free blocks</summary>
            public uint fs_minfree;
            /// <summary>0x0040 num of ms for optimal next block</summary>
            public uint fs_rotdelay;
            /// <summary>0x0044 disk revolutions per second</summary>
            public uint fs_rps;

            // these fields can be computed from the others
            /// <summary>0x0048 ``blkoff'' calc of blk offsets</summary>
            public uint fs_bmask;
            /// <summary>0x004C ``fragoff'' calc of frag offsets</summary>
            public uint fs_fmask;
            /// <summary>0x0050 ``lblkno'' calc of logical blkno</summary>
            public uint fs_bshift;
            /// <summary>0x0054 ``numfrags'' calc number of frags</summary>
            public uint fs_fshift;

            // these are configuration parameters
            /// <summary>0x0058 max number of contiguous blks</summary>
            public uint fs_maxcontig;
            /// <summary>0x005C max number of blks per cyl group</summary>
            public uint fs_maxbpg;

            // these fields can be computed from the others
            /// <summary>0x0060 block to frag shift</summary>
            public uint fs_fragshift;
            /// <summary>0x0064 fsbtodb and dbtofsb shift constant</summary>
            public uint fs_fsbtodb;
            /// <summary>0x0068 actual size of super block</summary>
            public uint fs_sbsize;
            /// <summary>0x006C csum block offset</summary>
            public uint fs_csmask;
            /// <summary>0x0070 csum block number</summary>
            public uint fs_csshift;
            /// <summary>0x0074 value of NINDIR</summary>
            public uint fs_nindir;
            /// <summary>0x0078 value of INOPB</summary>
            public uint fs_inopb;
            /// <summary>0x007C value of NSPF</summary>
            public uint fs_nspf;

            // yet another configuration parameter
            /// <summary>0x0080 optimization preference, see below</summary>
            public uint fs_optim;

            #endregion COMMON


            #region Sun

            // these fields are derived from the hardware
            /// <summary>0x0084 # sectors/track including spares</summary>
            public uint fs_npsect_sun;

            #endregion Sun

            #region Sunx86

            /// <summary>0x0084 file system state time stamp</summary>
            public uint fs_state_t_sun86;

            #endregion Sunx86

            #region COMMON

            /// <summary>0x0088 hardware sector interleave</summary>
            public uint fs_interleave;
            /// <summary>0x008C sector 0 skew, per track</summary>
            public uint fs_trackskew;

            #endregion COMMON


            #region COMMON

            // a unique id for this filesystem (currently unused and unmaintained)
            // In 4.3 Tahoe this space is used by fs_headswitch and fs_trkseek
            // Neither of those fields is used in the Tahoe code right now but
            // there could be problems if they are.

            /// <summary>0x0090</summary>
            public uint fs_id_1;
            /// <summary>0x0094</summary>
            public uint fs_id_2;

            #endregion COMMON

            #region 43BSD

            /// <summary>0x0090 head switch time, usec</summary>
            public uint fs_headswitch_43bsd;
            /// <summary>0x0094 track-to-track seek, usec</summary>
            public uint fs_trkseek_43bsd;

            #endregion 43BSD

            #region COMMON

            // sizes determined by number of cylinder groups and their sizes
            /// <summary>0x0098 blk addr of cyl grp summary area</summary>
            public uint fs_csaddr;
            /// <summary>0x009C size of cyl grp summary area</summary>
            public uint fs_cssize;
            /// <summary>0x00A0 cylinder group size</summary>
            public uint fs_cgsize;

            // these fields are derived from the hardware
            /// <summary>0x00A4 tracks per cylinder</summary>
            public uint fs_ntrak;
            /// <summary>0x00A8 sectors per track</summary>
            public uint fs_nsect;
            /// <summary>0x00AC sectors per cylinder</summary>
            public uint fs_spc;

            // this comes from the disk driver partitioning
            /// <summary>0x00B0 cylinders in file system</summary>
            public uint fs_ncyl;

            // these fields can be computed from the others
            /// <summary>0x00B4 cylinders per group</summary>
            public uint fs_cpg;
            /// <summary>0x00B8 inodes per cylinder group</summary>
            public uint fs_ipg;
            /// <summary>0x00BC blocks per group * fs_frag</summary>
            public uint fs_fpg;

            // this data must be re-computed after crashes
            // struct ufs_csum fs_cstotal;  // cylinder summary information
            /// <summary>0x00C0 number of directories</summary>
            public uint fs_cstotal_ndir;
            /// <summary>0x00C4 number of free blocks</summary>
            public uint fs_cstotal_nbfree;
            /// <summary>0x00C8 number of free inodes</summary>
            public uint fs_cstotal_nifree;
            /// <summary>0x00CC number of free frags</summary>
            public uint fs_cstotal_nffree;

            // these fields are cleared at mount time
            /// <summary>0x00D0 super block modified flag</summary>
            public byte fs_fmod;
            /// <summary>0x00D1 file system is clean flag</summary>
            public byte fs_clean;
            /// <summary>0x00D2 mounted read-only flag</summary>
            public byte fs_ronly;
            /// <summary>0x00D3</summary>
            public byte fs_flags;

            #endregion common

            #region UFS1

            /// <summary>0x00D4, 512 bytes, name mounted on</summary>
            public string fs_fsmnt_ufs1;
            /// <summary>0x02D4 last cg searched</summary>
            public uint fs_cgrotor_ufs1;
            /// <summary>0x02D8, 124 bytes, uints, list of fs_cs info buffers</summary>
            public byte[] fs_cs_ufs1;
            /// <summary>0x0354</summary>
            public uint fs_maxcluster_ufs1;
            /// <summary>0x0358 cyl per cycle in postbl</summary>
            public uint fs_cpc_ufs1;
            /// <summary>0x035C, 256 bytes, [16][8] matrix of ushorts, old rotation block list head</summary>
            public byte[] fs_opostbl_ufs1;

            #endregion UFS1

            #region UFS2

            /// <summary>0x00D4, 468 bytes, name mounted on</summary>
            public string fs_fsmnt_ufs2;
            /// <summary>0x02A8, 32 bytes, volume name</summary>
            public string fs_volname_ufs2;
            /// <summary>0x02C8 system-wide uid</summary>
            public ulong fs_swuid_ufs2;
            /// <summary>0x02D0 due to alignment of fs_swuid</summary>
            public uint fs_pad_ufs2;
            /// <summary>0x02D4 last cg searched</summary>
            public uint fs_cgrotor_ufs2;
            /// <summary>0x02D8, 112 bytes, uints, list of fs_cs info buffers</summary>
            public byte[] fs_ocsp_ufs2;
            /// <summary>0x0348 # of contiguously allocated dirs</summary>
            public uint fs_contigdirs_ufs2;
            /// <summary>0x034C cg summary info buffer for fs_cs</summary>
            public uint fs_csp_ufs2;
            /// <summary>0x0350</summary>
            public uint fs_maxcluster_ufs2;
            /// <summary>0x0354 used by snapshots to track fs</summary>
            public uint fs_active_ufs2;
            /// <summary>0x0358 cyl per cycle in postbl</summary>
            public uint fs_old_cpc_ufs2;
            /// <summary>0x035C maximum blocking factor permitted</summary>
            public uint fs_maxbsize_ufs2;
            /// <summary>0x0360, 136 bytes, ulongs, old rotation block list head</summary>
            public byte[] fs_sparecon64_ufs2;
            /// <summary>0x03E8 byte offset of standard superblock</summary>
            public ulong fs_sblockloc_ufs2;

            /// <summary>0x03F0 number of directories</summary>
            public ulong fs_cstotal_ndir_ufs2;
            /// <summary>0x03F8 number of free blocks</summary>
            public ulong fs_cstotal_nbfree_ufs2;
            /// <summary>0x0400 number of free inodes</summary>
            public ulong fs_cstotal_nifree_ufs2;
            /// <summary>0x0408 number of free frags</summary>
            public ulong fs_cstotal_nffree_ufs2;
            /// <summary>0x0410 number of free clusters</summary>
            public ulong fs_cstotal_numclusters_ufs2;
            /// <summary>0x0418 future expansion</summary>
            public ulong fs_cstotal_spare0_ufs2;
            /// <summary>0x0420 future expansion</summary>
            public ulong fs_cstotal_spare1_ufs2;
            /// <summary>0x0428 future expansion</summary>
            public ulong fs_cstotal_spare2_ufs2;
            /// <summary>0x0430 last time written</summary>
            public uint fs_time_sec_ufs2;
            /// <summary>0x0434 last time written</summary>
            public uint fs_time_usec_ufs2;
            /// <summary>0x0438 number of blocks in fs</summary>
            public ulong fs_size_ufs2;
            /// <summary>0x0440 number of data blocks in fs</summary>
            public ulong fs_dsize_ufs2;
            /// <summary>0x0448 blk addr of cyl grp summary area</summary>
            public ulong fs_csaddr_ufs2;
            /// <summary>0x0450 blocks in process of being freed</summary>
            public ulong fs_pendingblocks_ufs2;
            /// <summary>0x0458 inodes in process of being freed</summary>
            public uint fs_pendinginodes_ufs2;

            #endregion UFS2

            #region Sun

            /// <summary>0x045C, 212 bytes, reserved for future constants</summary>
            public byte[] fs_sparecon_sun;
            /// <summary>0x0530</summary>
            public uint fs_reclaim_sun;
            /// <summary>0x0534</summary>
            public uint fs_sparecon2_sun;
            /// <summary>0x0538 file system state time stamp</summary>
            public uint fs_state_t_sun;
            /// <summary>0x053C ~usb_bmask</summary>
            public uint fs_qbmask0_sun;
            /// <summary>0x0540 ~usb_bmask</summary>
            public uint fs_qbmask1_sun;
            /// <summary>0x0544 ~usb_fmask</summary>
            public uint fs_qfmask0_sun;
            /// <summary>0x0548 ~usb_fmask</summary>
            public uint fs_qfmask1_sun;

            #endregion Sun

            #region Sunx86

            /// <summary>0x045C, 212 bytes, reserved for future constants</summary>
            public byte[] fs_sparecon_sun86;
            /// <summary>0x0530</summary>
            public uint fs_reclaim_sun86;
            /// <summary>0x0534</summary>
            public uint fs_sparecon2_sun86;
            /// <summary>0x0538 # sectors/track including spares</summary>
            public uint fs_npsect_sun86;
            /// <summary>0x053C ~usb_bmask</summary>
            public uint fs_qbmask0_sun86;
            /// <summary>0x0540 ~usb_bmask</summary>
            public uint fs_qbmask1_sun86;
            /// <summary>0x0544 ~usb_fmask</summary>
            public uint fs_qfmask0_sun86;
            /// <summary>0x0548 ~usb_fmask</summary>
            public uint fs_qfmask1_sun86;

            #endregion Sunx86

            #region 44BSD

            /// <summary>0x045C, 200 bytes</summary>
            public byte[] fs_sparecon_44bsd;
            /// <summary>0x0524 size of cluster summary array</summary>
            public uint fs_contigsumsize_44bsd;
            /// <summary>0x0528 max length of an internal symlink</summary>
            public uint fs_maxsymlinklen_44bsd;
            /// <summary>0x052C format of on-disk inodes</summary>
            public uint fs_inodefmt_44bsd;
            /// <summary>0x0530 max representable file size</summary>
            public uint fs_maxfilesize0_44bsd;
            /// <summary>0x0534 max representable file size</summary>
            public uint fs_maxfilesize1_44bsd;
            /// <summary>0x0538 ~usb_bmask</summary>
            public uint fs_qbmask0_44bsd;
            /// <summary>0x053C ~usb_bmask</summary>
            public uint fs_qbmask1_44bsd;
            /// <summary>0x0540 ~usb_fmask</summary>
            public uint fs_qfmask0_44bsd;
            /// <summary>0x0544 ~usb_fmask</summary>
            public uint fs_qfmask1_44bsd;
            /// <summary>0x0548 file system state time stamp</summary>
            public uint fs_state_t_44bsd;

            #endregion 44BSD

            /// <summary>0x054C format of positional layout tables</summary>
            public uint fs_postblformat;
            /// <summary>0x0550 number of rotational positions</summary>
            public uint fs_nrpos;
            /// <summary>0x0554 (__s16) rotation block list head</summary>
            public uint fs_postbloff;
            /// <summary>0x0558 (__u8) blocks for each rotation</summary>
            public uint fs_rotbloff;
            /// <summary>0x055C magic number</summary>
            public uint fs_magic;
            /// <summary>0x0560 list of blocks for each rotation</summary>
            public byte fs_space;
            // 0x0561
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}