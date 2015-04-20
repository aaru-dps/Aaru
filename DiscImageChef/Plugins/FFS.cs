/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : FFS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies BSD/UNIX FFS/UFS/UFS2 filesystems and shows information.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Text;
using DiscImageChef;

// Using information from Linux kernel headers
namespace DiscImageChef.Plugins
{
    public class FFSPlugin : Plugin
    {
        public FFSPlugin(PluginBase Core)
        {
            Name = "BSD Fast File System (aka UNIX File System, UFS)";
            PluginUUID = new Guid("CC90D342-05DB-48A8-988C-C1FE000034A3");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if ((2 + partitionStart) >= imagePlugin.GetSectors())
                return false;

            UInt32 magic;
            uint sb_size_in_sectors;
            byte[] ufs_sb_sectors;

            if (imagePlugin.GetSectorSize() == 2336 || imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
                sb_size_in_sectors = block_size / 2048;
            else
                sb_size_in_sectors = block_size / imagePlugin.GetSectorSize();

            if (imagePlugin.GetSectors() > (partitionStart + sb_start_floppy * sb_size_in_sectors + sb_size_in_sectors))
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_floppy * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if (magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    return true;
            }

            if (imagePlugin.GetSectors() > (partitionStart + sb_start_ufs1 * sb_size_in_sectors + sb_size_in_sectors))
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_ufs1 * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if (magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    return true;
            }

            if (imagePlugin.GetSectors() > (partitionStart + sb_start_ufs2 * sb_size_in_sectors + sb_size_in_sectors))
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_ufs2 * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if (magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    return true;
            }

            if (imagePlugin.GetSectors() > (partitionStart + sb_start_piggy * sb_size_in_sectors + sb_size_in_sectors))
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_piggy * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if (magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    return true;
            }

            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";
            StringBuilder sbInformation = new StringBuilder();

            UInt32 magic = 0;
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

            if (imagePlugin.GetSectorSize() == 2336 || imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
                sb_size_in_sectors = block_size / 2048;
            else
                sb_size_in_sectors = block_size / imagePlugin.GetSectorSize();
			
            if (imagePlugin.GetSectors() > (partitionStart + sb_start_floppy * sb_size_in_sectors + sb_size_in_sectors) && magic == 0)
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_floppy * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);
				
                if (magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    sb_offset = partitionStart + sb_start_floppy * sb_size_in_sectors;
                else
                    magic = 0;
            }

            if (imagePlugin.GetSectors() > (partitionStart + sb_start_ufs1 * sb_size_in_sectors + sb_size_in_sectors) && magic == 0)
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_ufs1 * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);
				
                if (magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    sb_offset = partitionStart + sb_start_ufs1 * sb_size_in_sectors;
                else
                    magic = 0;
            }

            if (imagePlugin.GetSectors() > (partitionStart + sb_start_ufs2 * sb_size_in_sectors + sb_size_in_sectors) && magic == 0)
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_ufs2 * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);
				
                if (magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    sb_offset = partitionStart + sb_start_ufs2 * sb_size_in_sectors;
                else
                    magic = 0;
            }

            if (imagePlugin.GetSectors() > (partitionStart + sb_start_piggy * sb_size_in_sectors + sb_size_in_sectors) && magic == 0)
            {
                ufs_sb_sectors = imagePlugin.ReadSectors(partitionStart + sb_start_piggy * sb_size_in_sectors, sb_size_in_sectors);
                magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);
				
                if (magic == UFS_MAGIC || magic == UFS_MAGIC_BW || magic == UFS2_MAGIC || magic == UFS_CIGAM || magic == UFS_BAD_MAGIC)
                    sb_offset = partitionStart + sb_start_piggy * sb_size_in_sectors;
                else
                    magic = 0;
            }

            if (magic == 0)
            {
                information = "Not a UFS filesystem, I shouldn't have arrived here!";
                return;
            }

            switch (magic)
            {
                case UFS_MAGIC:
                    sbInformation.AppendLine("UFS filesystem");
                    break;
                case UFS_MAGIC_BW:
                    sbInformation.AppendLine("BorderWare UFS filesystem");
                    break;
                case UFS2_MAGIC:
                    sbInformation.AppendLine("UFS2 filesystem");
                    break;
                case UFS_CIGAM:
                    sbInformation.AppendLine("Big-endian UFS filesystem");
                    break;
                case UFS_BAD_MAGIC:
                    sbInformation.AppendLine("Incompletely initialized UFS filesystem");
                    sbInformation.AppendLine("BEWARE!!! Following information may be completely wrong!");
                    break;
            }

            BigEndianBitConverter.IsLittleEndian = magic != UFS_CIGAM;  // Little-endian UFS
            // Are there any other cases to detect big-endian UFS?

            // Fun with seeking follows on superblock reading!
            UFSSuperBlock ufs_sb = new UFSSuperBlock();
            byte[] strings_b;
            ufs_sb_sectors = imagePlugin.ReadSectors(sb_offset, sb_size_in_sectors);

            ufs_sb.fs_link_42bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0000); // 0x0000
            ufs_sb.fs_state_sun = ufs_sb.fs_link_42bsd;
            ufs_sb.fs_rlink = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0004);     // 0x0004 UNUSED
            ufs_sb.fs_sblkno = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0008);    // 0x0008 addr of super-block in filesys
            ufs_sb.fs_cblkno = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x000C);    // 0x000C offset of cyl-block in filesys
            ufs_sb.fs_iblkno = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0010);    // 0x0010 offset of inode-blocks in filesys
            ufs_sb.fs_dblkno = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0014);    // 0x0014 offset of first data after cg
            ufs_sb.fs_cgoffset = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0018);  // 0x0018 cylinder group offset in cylinder
            ufs_sb.fs_cgmask = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x001C);    // 0x001C used to calc mod fs_ntrak
            ufs_sb.fs_time_t = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0020);    // 0x0020 last time written -- time_t
            ufs_sb.fs_size = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0024);      // 0x0024 number of blocks in fs
            ufs_sb.fs_dsize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0028);     // 0x0028 number of data blocks in fs
            ufs_sb.fs_ncg = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x002C);       // 0x002C number of cylinder groups
            ufs_sb.fs_bsize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0030);     // 0x0030 size of basic blocks in fs
            ufs_sb.fs_fsize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0034);     // 0x0034 size of frag blocks in fs
            ufs_sb.fs_frag = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0038);      // 0x0038 number of frags in a block in fs
            // these are configuration parameters
            ufs_sb.fs_minfree = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x003C);   // 0x003C minimum percentage of free blocks
            ufs_sb.fs_rotdelay = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0040);  // 0x0040 num of ms for optimal next block
            ufs_sb.fs_rps = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0044);       // 0x0044 disk revolutions per second
            // these fields can be computed from the others
            ufs_sb.fs_bmask = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0048);     // 0x0048 ``blkoff'' calc of blk offsets
            ufs_sb.fs_fmask = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x004C);     // 0x004C ``fragoff'' calc of frag offsets
            ufs_sb.fs_bshift = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0050);    // 0x0050 ``lblkno'' calc of logical blkno
            ufs_sb.fs_fshift = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0054);    // 0x0054 ``numfrags'' calc number of frags
            // these are configuration parameters
            ufs_sb.fs_maxcontig = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0058); // 0x0058 max number of contiguous blks
            ufs_sb.fs_maxbpg = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x005C);    // 0x005C max number of blks per cyl group
            // these fields can be computed from the others
            ufs_sb.fs_fragshift = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0060); // 0x0060 block to frag shift
            ufs_sb.fs_fsbtodb = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0064);   // 0x0064 fsbtodb and dbtofsb shift constant
            ufs_sb.fs_sbsize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0068);    // 0x0068 actual size of super block
            ufs_sb.fs_csmask = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x006C);    // 0x006C csum block offset
            ufs_sb.fs_csshift = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0070);   // 0x0070 csum block number
            ufs_sb.fs_nindir = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0074);    // 0x0074 value of NINDIR
            ufs_sb.fs_inopb = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0078);     // 0x0078 value of INOPB
            ufs_sb.fs_nspf = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x007C);      // 0x007C value of NSPF
            // yet another configuration parameter
            ufs_sb.fs_optim = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0080);     // 0x0080 optimization preference, see below
            // these fields are derived from the hardware
            #region Sun
            ufs_sb.fs_npsect_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0084);               // 0x0084 # sectors/track including spares
            #endregion Sun
            #region Sunx86
            ufs_sb.fs_state_t_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0084);              // 0x0084 file system state time stamp
            #endregion Sunx86
            #region COMMON
            ufs_sb.fs_interleave = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0088);               // 0x0088 hardware sector interleave
            ufs_sb.fs_trackskew = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x008C);                // 0x008C sector 0 skew, per track
            #endregion COMMON
            // a unique id for this filesystem (currently unused and unmaintained)
            // In 4.3 Tahoe this space is used by fs_headswitch and fs_trkseek
            // Neither of those fields is used in the Tahoe code right now but
            // there could be problems if they are.                           
            #region COMMON
            ufs_sb.fs_id_1 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0090);                     // 0x0090
            ufs_sb.fs_id_2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0094);                     // 0x0094
            #endregion COMMON
            #region 43BSD
            ufs_sb.fs_headswitch_43bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0090);         // 0x0090
            ufs_sb.fs_trkseek_43bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0094);            // 0x0094
            #endregion 43BSD
            #region COMMON
            // sizes determined by number of cylinder groups and their sizes
            ufs_sb.fs_csaddr = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0098);                   // 0x0098 blk addr of cyl grp summary area
            ufs_sb.fs_cssize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x009C);                   // 0x009C size of cyl grp summary area
            ufs_sb.fs_cgsize = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00A0);                   // 0x00A0 cylinder group size
            // these fields are derived from the hardware
            ufs_sb.fs_ntrak = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00A4);                    // 0x00A4 tracks per cylinder
            ufs_sb.fs_nsect = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00A8);                    // 0x00A8 sectors per track
            ufs_sb.fs_spc = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00AC);                      // 0x00AC sectors per cylinder
            // this comes from the disk driver partitioning
            ufs_sb.fs_ncyl = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00B0);                     // 0x00B0 cylinders in file system
            // these fields can be computed from the others
            ufs_sb.fs_cpg = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00B4);                      // 0x00B4 cylinders per group
            ufs_sb.fs_ipg = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00B8);                      // 0x00B8 inodes per cylinder group
            ufs_sb.fs_fpg = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00BC);                      // 0x00BC blocks per group * fs_frag
            // this data must be re-computed after crashes
            // struct ufs_csum fs_cstotal = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0000);	// cylinder summary information
            ufs_sb.fs_cstotal_ndir = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00C0);             // 0x00C0 number of directories
            ufs_sb.fs_cstotal_nbfree = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00C4);           // 0x00C4 number of free blocks
            ufs_sb.fs_cstotal_nifree = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00C8);           // 0x00C8 number of free inodes
            ufs_sb.fs_cstotal_nffree = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x00CC);           // 0x00CC number of free frags
            // these fields are cleared at mount time
            ufs_sb.fs_fmod = ufs_sb_sectors[0x00D0];                       // 0x00D0 super block modified flag
            ufs_sb.fs_clean = ufs_sb_sectors[0x00D1];                      // 0x00D1 file system is clean flag
            ufs_sb.fs_ronly = ufs_sb_sectors[0x00D2];                      // 0x00D2 mounted read-only flag
            ufs_sb.fs_flags = ufs_sb_sectors[0x00D3];                      // 0x00D3
            #endregion COMMON
            #region UFS1
            strings_b = new byte[512];
            Array.Copy(ufs_sb_sectors, 0x00D4, strings_b, 0, 512);
            ufs_sb.fs_fsmnt_ufs1 = StringHandlers.CToString(strings_b);               // 0x00D4, 512 bytes, name mounted on
            ufs_sb.fs_cgrotor_ufs1 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0000);             // 0x02D4 last cg searched
            Array.Copy(ufs_sb_sectors, 0x02D8, ufs_sb.fs_cs_ufs1, 0, 124); // 0x02D8, 124 bytes, UInt32s, list of fs_cs info buffers
            ufs_sb.fs_maxcluster_ufs1 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0354);          // 0x0354
            ufs_sb.fs_cpc_ufs1 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0358);                 // 0x0358 cyl per cycle in postbl
            Array.Copy(ufs_sb_sectors, 0x035C, ufs_sb.fs_opostbl_ufs1, 0, 256); // 0x035C, 256 bytes, [16][8] matrix of UInt16s, old rotation block list head
            #endregion UFS1
            #region UFS2
            strings_b = new byte[468];
            Array.Copy(ufs_sb_sectors, 0x00D4, strings_b, 0, 468);
            ufs_sb.fs_fsmnt_ufs2 = StringHandlers.CToString(strings_b);               // 0x00D4, 468 bytes, name mounted on
            strings_b = new byte[32];
            Array.Copy(ufs_sb_sectors, 0x02A8, strings_b, 0, 32);
            ufs_sb.fs_volname_ufs2 = StringHandlers.CToString(strings_b);             // 0x02A8, 32 bytes, volume name
            ufs_sb.fs_swuid_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x02C8);               // 0x02C8 system-wide uid
            ufs_sb.fs_pad_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x02D0);                 // 0x02D0 due to alignment of fs_swuid
            ufs_sb.fs_cgrotor_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x02D4);             // 0x02D4 last cg searched
            Array.Copy(ufs_sb_sectors, 0x02D8, ufs_sb.fs_ocsp_ufs2, 0, 112); // 0x02D8, 112 bytes, UInt32s, list of fs_cs info buffers
            ufs_sb.fs_contigdirs_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0348);          // 0x0348 # of contiguously allocated dirs
            ufs_sb.fs_csp_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x034C);                 // 0x034C cg summary info buffer for fs_cs
            ufs_sb.fs_maxcluster_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0350);          // 0x0350
            ufs_sb.fs_active_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0354);              // 0x0354 used by snapshots to track fs
            ufs_sb.fs_old_cpc_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0358);             // 0x0358 cyl per cycle in postbl
            ufs_sb.fs_maxbsize_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x035C);            // 0x035C maximum blocking factor permitted
            Array.Copy(ufs_sb_sectors, 0x0360, ufs_sb.fs_sparecon64_ufs2, 0, 136); // 0x0360, 136 bytes, UInt64s, old rotation block list head
            ufs_sb.fs_sblockloc_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x03E8);           // 0x03E8 byte offset of standard superblock
            //cylinder summary information*/
            ufs_sb.fs_cstotal_ndir_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x03F0);        // 0x03F0 number of directories
            ufs_sb.fs_cstotal_nbfree_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x03F8);      // 0x03F8 number of free blocks
            ufs_sb.fs_cstotal_nifree_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0400);      // 0x0400 number of free inodes
            ufs_sb.fs_cstotal_nffree_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0408);      // 0x0408 number of free frags
            ufs_sb.fs_cstotal_numclusters_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0410); // 0x0410 number of free clusters
            ufs_sb.fs_cstotal_spare0_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0418);      // 0x0418 future expansion
            ufs_sb.fs_cstotal_spare1_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0420);      // 0x0420 future expansion
            ufs_sb.fs_cstotal_spare2_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0428);      // 0x0428 future expansion
            ufs_sb.fs_time_sec_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0430);            // 0x0430 last time written
            ufs_sb.fs_time_usec_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0434);           // 0x0434 last time written
            ufs_sb.fs_size_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0438);                // 0x0438 number of blocks in fs
            ufs_sb.fs_dsize_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0440);               // 0x0440 number of data blocks in fs
            ufs_sb.fs_csaddr_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0448);              // 0x0448 blk addr of cyl grp summary area
            ufs_sb.fs_pendingblocks_ufs2 = BigEndianBitConverter.ToUInt64(ufs_sb_sectors, 0x0450);       // 0x0450 blocks in process of being freed
            ufs_sb.fs_pendinginodes_ufs2 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0458);       // 0x0458 inodes in process of being freed
            #endregion UFS2
            #region Sun
            Array.Copy(ufs_sb_sectors, 0x045C, ufs_sb.fs_sparecon_sun, 0, 212); // 0x045C, 212 bytes, reserved for future constants
            ufs_sb.fs_reclaim_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0530);              // 0x0530
            ufs_sb.fs_sparecon2_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0534);            // 0x0534
            ufs_sb.fs_state_t_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0538);              // 0x0538 file system state time stamp
            ufs_sb.fs_qbmask0_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x053C);              // 0x053C ~usb_bmask
            ufs_sb.fs_qbmask1_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0540);              // 0x0540 ~usb_bmask
            ufs_sb.fs_qfmask0_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0544);              // 0x0544 ~usb_fmask
            ufs_sb.fs_qfmask1_sun = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0548);              // 0x0548 ~usb_fmask
            #endregion Sun
            #region Sunx86
            Array.Copy(ufs_sb_sectors, 0x045C, ufs_sb.fs_sparecon_sun86, 0, 212); // 0x045C, 212 bytes, reserved for future constants
            ufs_sb.fs_reclaim_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0530);            // 0x0530
            ufs_sb.fs_sparecon2_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0534);          // 0x0534
            ufs_sb.fs_npsect_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0538);             // 0x0538 # sectors/track including spares
            ufs_sb.fs_qbmask0_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x053C);            // 0x053C ~usb_bmask
            ufs_sb.fs_qbmask1_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0540);            // 0x0540 ~usb_bmask
            ufs_sb.fs_qfmask0_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0544);            // 0x0544 ~usb_fmask
            ufs_sb.fs_qfmask1_sun86 = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0548);            // 0x0548 ~usb_fmask
            #endregion Sunx86
            #region 44BSD
            Array.Copy(ufs_sb_sectors, 0x045C, ufs_sb.fs_sparecon_44bsd, 0, 200); // 0x045C, 200 bytes
            ufs_sb.fs_contigsumsize_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0524);      // 0x0524 size of cluster summary array
            ufs_sb.fs_maxsymlinklen_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0528);      // 0x0528 max length of an internal symlink
            ufs_sb.fs_inodefmt_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x052C);           // 0x052C format of on-disk inodes
            ufs_sb.fs_maxfilesize0_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0530);       // 0x0530 max representable file size
            ufs_sb.fs_maxfilesize1_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0534);       // 0x0534 max representable file size
            ufs_sb.fs_qbmask0_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0538);            // 0x0538 ~usb_bmask
            ufs_sb.fs_qbmask1_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x053C);            // 0x053C ~usb_bmask
            ufs_sb.fs_qfmask0_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0540);            // 0x0540 ~usb_fmask
            ufs_sb.fs_qfmask1_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0544);            // 0x0544 ~usb_fmask
            ufs_sb.fs_state_t_44bsd = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0548);              // 0x0548 file system state time stamp
            #endregion 44BSD
            ufs_sb.fs_postblformat = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x054C);             // 0x054C format of positional layout tables
            ufs_sb.fs_nrpos = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0550);                    // 0x0550 number of rotational positions
            ufs_sb.fs_postbloff = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0554);                // 0x0554 (__s16) rotation block list head
            ufs_sb.fs_rotbloff = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x0558);                 // 0x0558 (__u8) blocks for each rotation
            ufs_sb.fs_magic = BigEndianBitConverter.ToUInt32(ufs_sb_sectors, 0x055C);                    // 0x055C magic number
            ufs_sb.fs_space = ufs_sb_sectors[0x0560];                    // 0x0560 list of blocks for each rotation

            if (MainClass.isDebug)
            {
                Console.WriteLine("ufs_sb offset: 0x{0:X8}", sb_offset);
                Console.WriteLine("fs_link_42bsd: 0x{0:X8}", ufs_sb.fs_link_42bsd);
                Console.WriteLine("fs_state_sun: 0x{0:X8}", ufs_sb.fs_state_sun);
                Console.WriteLine("fs_rlink: 0x{0:X8}", ufs_sb.fs_rlink);
                Console.WriteLine("fs_sblkno: 0x{0:X8}", ufs_sb.fs_sblkno);
                Console.WriteLine("fs_cblkno: 0x{0:X8}", ufs_sb.fs_cblkno);
                Console.WriteLine("fs_iblkno: 0x{0:X8}", ufs_sb.fs_iblkno);
                Console.WriteLine("fs_dblkno: 0x{0:X8}", ufs_sb.fs_dblkno);
                Console.WriteLine("fs_cgoffset: 0x{0:X8}", ufs_sb.fs_cgoffset);
                Console.WriteLine("fs_cgmask: 0x{0:X8}", ufs_sb.fs_cgmask);
                Console.WriteLine("fs_time_t: 0x{0:X8}", ufs_sb.fs_time_t);
                Console.WriteLine("fs_size: 0x{0:X8}", ufs_sb.fs_size);
                Console.WriteLine("fs_dsize: 0x{0:X8}", ufs_sb.fs_dsize);
                Console.WriteLine("fs_ncg: 0x{0:X8}", ufs_sb.fs_ncg);
                Console.WriteLine("fs_bsize: 0x{0:X8}", ufs_sb.fs_bsize);
                Console.WriteLine("fs_fsize: 0x{0:X8}", ufs_sb.fs_fsize);
                Console.WriteLine("fs_frag: 0x{0:X8}", ufs_sb.fs_frag);
                Console.WriteLine("fs_minfree: 0x{0:X8}", ufs_sb.fs_minfree);
                Console.WriteLine("fs_rotdelay: 0x{0:X8}", ufs_sb.fs_rotdelay);
                Console.WriteLine("fs_rps: 0x{0:X8}", ufs_sb.fs_rps);
                Console.WriteLine("fs_bmask: 0x{0:X8}", ufs_sb.fs_bmask);
                Console.WriteLine("fs_fmask: 0x{0:X8}", ufs_sb.fs_fmask);
                Console.WriteLine("fs_bshift: 0x{0:X8}", ufs_sb.fs_bshift);
                Console.WriteLine("fs_fshift: 0x{0:X8}", ufs_sb.fs_fshift);
                Console.WriteLine("fs_maxcontig: 0x{0:X8}", ufs_sb.fs_maxcontig);
                Console.WriteLine("fs_maxbpg: 0x{0:X8}", ufs_sb.fs_maxbpg);
                Console.WriteLine("fs_fragshift: 0x{0:X8}", ufs_sb.fs_fragshift);
                Console.WriteLine("fs_fsbtodb: 0x{0:X8}", ufs_sb.fs_fsbtodb);
                Console.WriteLine("fs_sbsize: 0x{0:X8}", ufs_sb.fs_sbsize);
                Console.WriteLine("fs_csmask: 0x{0:X8}", ufs_sb.fs_csmask);
                Console.WriteLine("fs_csshift: 0x{0:X8}", ufs_sb.fs_csshift);
                Console.WriteLine("fs_nindir: 0x{0:X8}", ufs_sb.fs_nindir);
                Console.WriteLine("fs_inopb: 0x{0:X8}", ufs_sb.fs_inopb);
                Console.WriteLine("fs_nspf: 0x{0:X8}", ufs_sb.fs_nspf);
                Console.WriteLine("fs_optim: 0x{0:X8}", ufs_sb.fs_optim);
                Console.WriteLine("fs_npsect_sun: 0x{0:X8}", ufs_sb.fs_npsect_sun);
                Console.WriteLine("fs_state_t_sun86: 0x{0:X8}", ufs_sb.fs_state_t_sun86);
                Console.WriteLine("fs_interleave: 0x{0:X8}", ufs_sb.fs_interleave);
                Console.WriteLine("fs_trackskew: 0x{0:X8}", ufs_sb.fs_trackskew);
                Console.WriteLine("fs_id_1: 0x{0:X8}", ufs_sb.fs_id_1);
                Console.WriteLine("fs_id_2: 0x{0:X8}", ufs_sb.fs_id_2);
                Console.WriteLine("fs_headswitch_43bsd: 0x{0:X8}", ufs_sb.fs_headswitch_43bsd);
                Console.WriteLine("fs_trkseek_43bsd: 0x{0:X8}", ufs_sb.fs_trkseek_43bsd);
                Console.WriteLine("fs_csaddr: 0x{0:X8}", ufs_sb.fs_csaddr);
                Console.WriteLine("fs_cssize: 0x{0:X8}", ufs_sb.fs_cssize);
                Console.WriteLine("fs_cgsize: 0x{0:X8}", ufs_sb.fs_cgsize);
                Console.WriteLine("fs_ntrak: 0x{0:X8}", ufs_sb.fs_ntrak);
                Console.WriteLine("fs_nsect: 0x{0:X8}", ufs_sb.fs_nsect);
                Console.WriteLine("fs_spc: 0x{0:X8}", ufs_sb.fs_spc);
                Console.WriteLine("fs_ncyl: 0x{0:X8}", ufs_sb.fs_ncyl);
                Console.WriteLine("fs_cpg: 0x{0:X8}", ufs_sb.fs_cpg);
                Console.WriteLine("fs_ipg: 0x{0:X8}", ufs_sb.fs_ipg);
                Console.WriteLine("fs_fpg: 0x{0:X8}", ufs_sb.fs_fpg);
                Console.WriteLine("fs_cstotal_ndir: 0x{0:X8}", ufs_sb.fs_cstotal_ndir);
                Console.WriteLine("fs_cstotal_nbfree: 0x{0:X8}", ufs_sb.fs_cstotal_nbfree);
                Console.WriteLine("fs_cstotal_nifree: 0x{0:X8}", ufs_sb.fs_cstotal_nifree);
                Console.WriteLine("fs_cstotal_nffree: 0x{0:X8}", ufs_sb.fs_cstotal_nffree);
                Console.WriteLine("fs_fmod: 0x{0:X2}", ufs_sb.fs_fmod);
                Console.WriteLine("fs_clean: 0x{0:X2}", ufs_sb.fs_clean);
                Console.WriteLine("fs_ronly: 0x{0:X2}", ufs_sb.fs_ronly);
                Console.WriteLine("fs_flags: 0x{0:X2}", ufs_sb.fs_flags);
                Console.WriteLine("fs_fsmnt_ufs1: {0}", ufs_sb.fs_fsmnt_ufs1);
                Console.WriteLine("fs_cgrotor_ufs1: 0x{0:X8}", ufs_sb.fs_cgrotor_ufs1);
                Console.WriteLine("fs_cs_ufs1: 0x{0:X}", ufs_sb.fs_cs_ufs1);
                Console.WriteLine("fs_maxcluster_ufs1: 0x{0:X8}", ufs_sb.fs_maxcluster_ufs1);
                Console.WriteLine("fs_cpc_ufs1: 0x{0:X8}", ufs_sb.fs_cpc_ufs1);
                Console.WriteLine("fs_opostbl_ufs1: 0x{0:X}", ufs_sb.fs_opostbl_ufs1);
                Console.WriteLine("fs_fsmnt_ufs2: {0}", ufs_sb.fs_fsmnt_ufs2);
                Console.WriteLine("fs_volname_ufs2: {0}", ufs_sb.fs_volname_ufs2);
                Console.WriteLine("fs_swuid_ufs2: 0x{0:X16}", ufs_sb.fs_swuid_ufs2);
                Console.WriteLine("fs_pad_ufs2: 0x{0:X8}", ufs_sb.fs_pad_ufs2);
                Console.WriteLine("fs_cgrotor_ufs2: 0x{0:X8}", ufs_sb.fs_cgrotor_ufs2);
                Console.WriteLine("fs_ocsp_ufs2: 0x{0:X}", ufs_sb.fs_ocsp_ufs2);
                Console.WriteLine("fs_contigdirs_ufs2: 0x{0:X8}", ufs_sb.fs_contigdirs_ufs2);
                Console.WriteLine("fs_csp_ufs2: 0x{0:X8}", ufs_sb.fs_csp_ufs2);
                Console.WriteLine("fs_maxcluster_ufs2: 0x{0:X8}", ufs_sb.fs_maxcluster_ufs2);
                Console.WriteLine("fs_active_ufs2: 0x{0:X8}", ufs_sb.fs_active_ufs2);
                Console.WriteLine("fs_old_cpc_ufs2: 0x{0:X8}", ufs_sb.fs_old_cpc_ufs2);
                Console.WriteLine("fs_maxbsize_ufs2: 0x{0:X8}", ufs_sb.fs_maxbsize_ufs2);
                Console.WriteLine("fs_sparecon64_ufs2: 0x{0:X}", ufs_sb.fs_sparecon64_ufs2);
                Console.WriteLine("fs_sblockloc_ufs2: 0x{0:X16}", ufs_sb.fs_sblockloc_ufs2);
                Console.WriteLine("fs_cstotal_ndir_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_ndir_ufs2);
                Console.WriteLine("fs_cstotal_nbfree_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_nbfree_ufs2);
                Console.WriteLine("fs_cstotal_nifree_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_nifree_ufs2);
                Console.WriteLine("fs_cstotal_nffree_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_nffree_ufs2);
                Console.WriteLine("fs_cstotal_numclusters_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_numclusters_ufs2);
                Console.WriteLine("fs_cstotal_spare0_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_spare0_ufs2);
                Console.WriteLine("fs_cstotal_spare1_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_spare1_ufs2);
                Console.WriteLine("fs_cstotal_spare2_ufs2: 0x{0:X16}", ufs_sb.fs_cstotal_spare2_ufs2);
                Console.WriteLine("fs_time_sec_ufs2: 0x{0:X8}", ufs_sb.fs_time_sec_ufs2);
                Console.WriteLine("fs_time_usec_ufs2: 0x{0:X8}", ufs_sb.fs_time_usec_ufs2);
                Console.WriteLine("fs_size_ufs2: 0x{0:X16}", ufs_sb.fs_size_ufs2);
                Console.WriteLine("fs_dsize_ufs2: 0x{0:X16}", ufs_sb.fs_dsize_ufs2);
                Console.WriteLine("fs_csaddr_ufs2: 0x{0:X16}", ufs_sb.fs_csaddr_ufs2);
                Console.WriteLine("fs_pendingblocks_ufs2: 0x{0:X16}", ufs_sb.fs_pendingblocks_ufs2);
                Console.WriteLine("fs_pendinginodes_ufs2: 0x{0:X8}", ufs_sb.fs_pendinginodes_ufs2);
                Console.WriteLine("fs_sparecon_sun: 0x{0:X}", ufs_sb.fs_sparecon_sun);
                Console.WriteLine("fs_reclaim_sun: 0x{0:X8}", ufs_sb.fs_reclaim_sun);
                Console.WriteLine("fs_sparecon2_sun: 0x{0:X8}", ufs_sb.fs_sparecon2_sun);
                Console.WriteLine("fs_state_t_sun: 0x{0:X8}", ufs_sb.fs_state_t_sun);
                Console.WriteLine("fs_qbmask0_sun: 0x{0:X8}", ufs_sb.fs_qbmask0_sun);
                Console.WriteLine("fs_qbmask1_sun: 0x{0:X8}", ufs_sb.fs_qbmask1_sun);
                Console.WriteLine("fs_qfmask0_sun: 0x{0:X8}", ufs_sb.fs_qfmask0_sun);
                Console.WriteLine("fs_qfmask1_sun: 0x{0:X8}", ufs_sb.fs_qfmask1_sun);
                Console.WriteLine("fs_sparecon_sun86: 0x{0:X}", ufs_sb.fs_sparecon_sun86);
                Console.WriteLine("fs_reclaim_sun86: 0x{0:X8}", ufs_sb.fs_reclaim_sun86);
                Console.WriteLine("fs_sparecon2_sun86: 0x{0:X8}", ufs_sb.fs_sparecon2_sun86);
                Console.WriteLine("fs_npsect_sun86: 0x{0:X8}", ufs_sb.fs_npsect_sun86);
                Console.WriteLine("fs_qbmask0_sun86: 0x{0:X8}", ufs_sb.fs_qbmask0_sun86);
                Console.WriteLine("fs_qbmask1_sun86: 0x{0:X8}", ufs_sb.fs_qbmask1_sun86);
                Console.WriteLine("fs_qfmask0_sun86: 0x{0:X8}", ufs_sb.fs_qfmask0_sun86);
                Console.WriteLine("fs_qfmask1_sun86: 0x{0:X8}", ufs_sb.fs_qfmask1_sun86);
                Console.WriteLine("fs_sparecon_44bsd: 0x{0:X}", ufs_sb.fs_sparecon_44bsd);
                Console.WriteLine("fs_contigsumsize_44bsd: 0x{0:X8}", ufs_sb.fs_contigsumsize_44bsd);
                Console.WriteLine("fs_maxsymlinklen_44bsd: 0x{0:X8}", ufs_sb.fs_maxsymlinklen_44bsd);
                Console.WriteLine("fs_inodefmt_44bsd: 0x{0:X8}", ufs_sb.fs_inodefmt_44bsd);
                Console.WriteLine("fs_maxfilesize0_44bsd: 0x{0:X8}", ufs_sb.fs_maxfilesize0_44bsd);
                Console.WriteLine("fs_maxfilesize1_44bsd: 0x{0:X8}", ufs_sb.fs_maxfilesize1_44bsd);
                Console.WriteLine("fs_qbmask0_44bsd: 0x{0:X8}", ufs_sb.fs_qbmask0_44bsd);
                Console.WriteLine("fs_qbmask1_44bsd: 0x{0:X8}", ufs_sb.fs_qbmask1_44bsd);
                Console.WriteLine("fs_qfmask0_44bsd: 0x{0:X8}", ufs_sb.fs_qfmask0_44bsd);
                Console.WriteLine("fs_qfmask1_44bsd: 0x{0:X8}", ufs_sb.fs_qfmask1_44bsd);
                Console.WriteLine("fs_state_t_44bsd: 0x{0:X8}", ufs_sb.fs_state_t_44bsd);
                Console.WriteLine("fs_postblformat: 0x{0:X8}", ufs_sb.fs_postblformat);
                Console.WriteLine("fs_nrpos: 0x{0:X8}", ufs_sb.fs_nrpos);
                Console.WriteLine("fs_postbloff: 0x{0:X8}", ufs_sb.fs_postbloff);
                Console.WriteLine("fs_rotbloff: 0x{0:X8}", ufs_sb.fs_rotbloff);
                Console.WriteLine("fs_magic: 0x{0:X8}", ufs_sb.fs_magic);
                Console.WriteLine("fs_space: 0x{0:X2}", ufs_sb.fs_space);
            }

            sbInformation.AppendLine("There are a lot of variants of UFS using overlapped values on same fields");
            sbInformation.AppendLine("I will try to guess which one it is, but unless it's UFS2, I may be surely wrong");

            if (ufs_sb.fs_magic == UFS2_MAGIC)
            {
                fs_type_ufs2 = true;
            }
            else
            {
                const UInt32 SunOSEpoch = 0x1A54C580; // We are supposing there cannot be a Sun's fs created before 1/1/1982 00:00:00

                fs_type_43bsd = true; // There is no way of knowing this is the version, but there is of knowing it is not.

                if (ufs_sb.fs_link_42bsd > 0)
                {
                    fs_type_42bsd = true; // It was used in 4.2BSD
                    fs_type_43bsd = false;
                }

                if (ufs_sb.fs_state_t_sun > SunOSEpoch && DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_state_t_sun) < DateTime.Now)
                {
                    fs_type_42bsd = false;
                    fs_type_sun = true;
                    fs_type_43bsd = false;
                }

                // This is for sure, as it is shared with a sectors/track with non-x86 SunOS, Epoch is absurdly high for that
                if (ufs_sb.fs_state_t_sun86 > SunOSEpoch && DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_state_t_sun) < DateTime.Now)
                {
                    fs_type_42bsd = false;
                    fs_type_sun86 = true;
                    fs_type_sun = false;
                    fs_type_43bsd = false;
                }

                if (ufs_sb.fs_cgrotor_ufs1 > 0x00000000 && ufs_sb.fs_cgrotor_ufs1 < 0xFFFFFFFF)
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

            if (fs_type_42bsd)
                sbInformation.AppendLine("Guessed as 42BSD FFS");
            if (fs_type_43bsd)
                sbInformation.AppendLine("Guessed as 43BSD FFS");
            if (fs_type_44bsd)
                sbInformation.AppendLine("Guessed as 44BSD FFS");
            if (fs_type_sun)
                sbInformation.AppendLine("Guessed as SunOS FFS");
            if (fs_type_sun86)
                sbInformation.AppendLine("Guessed as SunOS/x86 FFS");
            if (fs_type_ufs)
                sbInformation.AppendLine("Guessed as UFS");
            if (fs_type_ufs2)
                sbInformation.AppendLine("Guessed as UFS2");

            if (fs_type_42bsd)
                sbInformation.AppendFormat("Linked list of filesystems: 0x{0:X8}", ufs_sb.fs_link_42bsd).AppendLine();
            else if (fs_type_sun)
                sbInformation.AppendFormat("Filesystem state flag: 0x{0:X8}", ufs_sb.fs_state_sun).AppendLine();
            sbInformation.AppendFormat("Superblock LBA: {0}", ufs_sb.fs_sblkno).AppendLine();
            sbInformation.AppendFormat("Cylinder-block LBA: {0}", ufs_sb.fs_cblkno).AppendLine();
            sbInformation.AppendFormat("inode-block LBA: {0}", ufs_sb.fs_iblkno).AppendLine();
            sbInformation.AppendFormat("First data block LBA: {0}", ufs_sb.fs_dblkno).AppendLine();
            sbInformation.AppendFormat("Cylinder group offset in cylinder: {0}", ufs_sb.fs_cgoffset).AppendLine();
            sbInformation.AppendFormat("Volume last written on {0}", DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_time_t)).AppendLine();
            sbInformation.AppendFormat("{0} blocks in volume ({1} bytes)", ufs_sb.fs_size, ufs_sb.fs_size * ufs_sb.fs_bsize).AppendLine();
            sbInformation.AppendFormat("{0} data blocks in volume ({1} bytes)", ufs_sb.fs_dsize, ufs_sb.fs_dsize * ufs_sb.fs_bsize).AppendLine();
            sbInformation.AppendFormat("{0} cylinder groups in volume", ufs_sb.fs_ncg).AppendLine();
            sbInformation.AppendFormat("{0} bytes in a basic block", ufs_sb.fs_bsize).AppendLine();
            sbInformation.AppendFormat("{0} bytes in a frag block", ufs_sb.fs_fsize).AppendLine();
            sbInformation.AppendFormat("{0} frags in a block", ufs_sb.fs_frag).AppendLine();
            sbInformation.AppendFormat("{0}% of blocks must be free", ufs_sb.fs_minfree).AppendLine();
            sbInformation.AppendFormat("{0}ms for optimal next block", ufs_sb.fs_rotdelay).AppendLine();
            sbInformation.AppendFormat("disk rotates {0} times per second ({1}rpm)", ufs_sb.fs_rps, ufs_sb.fs_rps * 60).AppendLine();
/*			sbInformation.AppendFormat("fs_bmask: 0x{0:X8}", ufs_sb.fs_bmask).AppendLine();
			sbInformation.AppendFormat("fs_fmask: 0x{0:X8}", ufs_sb.fs_fmask).AppendLine();
			sbInformation.AppendFormat("fs_bshift: 0x{0:X8}", ufs_sb.fs_bshift).AppendLine();
			sbInformation.AppendFormat("fs_fshift: 0x{0:X8}", ufs_sb.fs_fshift).AppendLine();*/
            sbInformation.AppendFormat("{0} contiguous blocks at maximum", ufs_sb.fs_maxcontig).AppendLine();
            sbInformation.AppendFormat("{0} blocks per cylinder group at maximum", ufs_sb.fs_maxbpg).AppendLine();
            sbInformation.AppendFormat("Superblock is {0} bytes", ufs_sb.fs_sbsize).AppendLine();
            sbInformation.AppendFormat("NINDIR: 0x{0:X8}", ufs_sb.fs_nindir).AppendLine();
            sbInformation.AppendFormat("INOPB: 0x{0:X8}", ufs_sb.fs_inopb).AppendLine();
            sbInformation.AppendFormat("NSPF: 0x{0:X8}", ufs_sb.fs_nspf).AppendLine();
            if (ufs_sb.fs_optim == 0)
                sbInformation.AppendLine("Filesystem will minimize allocation time");
            else if (ufs_sb.fs_optim == 1)
                sbInformation.AppendLine("Filesystem will minimize volume fragmentation");
            else
                sbInformation.AppendFormat("Unknown optimization value: 0x{0:X8}", ufs_sb.fs_optim).AppendLine();
            if (fs_type_sun)
                sbInformation.AppendFormat("{0} sectors/track", ufs_sb.fs_npsect_sun).AppendLine();
            else if (fs_type_sun86)
                sbInformation.AppendFormat("Volume state on {0}", DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_state_t_sun86)).AppendLine();
            sbInformation.AppendFormat("Hardware sector interleave: {0}", ufs_sb.fs_interleave).AppendLine();
            sbInformation.AppendFormat("Sector 0 skew: {0}/track", ufs_sb.fs_trackskew).AppendLine();
            if (!fs_type_43bsd && ufs_sb.fs_id_1 > 0 && ufs_sb.fs_id_2 > 0)
                sbInformation.AppendFormat("Volume ID: 0x{0:X8}{1:X8}", ufs_sb.fs_id_1, ufs_sb.fs_id_2).AppendLine();
            else if (fs_type_43bsd && ufs_sb.fs_headswitch_43bsd > 0 && ufs_sb.fs_trkseek_43bsd > 0)
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
            sbInformation.AppendFormat("{0} free inodes", ufs_sb.fs_cstotal_nifree).AppendLine();
            sbInformation.AppendFormat("{0} free frags", ufs_sb.fs_cstotal_nffree).AppendLine();
            if (ufs_sb.fs_fmod == 1)
                sbInformation.AppendLine("Superblock is under modification");
            if (ufs_sb.fs_clean == 1)
                sbInformation.AppendLine("Volume is clean");
            if (ufs_sb.fs_ronly == 1)
                sbInformation.AppendLine("Volume is read-only");
            sbInformation.AppendFormat("Volume flags: 0x{0:X2}", ufs_sb.fs_flags).AppendLine();
            if (fs_type_ufs)
            {
                sbInformation.AppendFormat("Volume last mounted on \"{0}\"", ufs_sb.fs_fsmnt_ufs1).AppendLine();
                sbInformation.AppendFormat("Last searched cylinder group: {0}", ufs_sb.fs_cgrotor_ufs1).AppendLine();
            }
            else if (fs_type_ufs2)
            {
                sbInformation.AppendFormat("Volume last mounted on \"{0}\"", ufs_sb.fs_fsmnt_ufs2).AppendLine();
                sbInformation.AppendFormat("Volume name: \"{0}\"", ufs_sb.fs_volname_ufs2).AppendLine();
                sbInformation.AppendFormat("Volume ID: 0x{0:X16}", ufs_sb.fs_swuid_ufs2).AppendLine();
                sbInformation.AppendFormat("Last searched cylinder group: {0}", ufs_sb.fs_cgrotor_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} contiguously allocated directories", ufs_sb.fs_contigdirs_ufs2).AppendLine();
                sbInformation.AppendFormat("Standard superblock LBA: {0}", ufs_sb.fs_sblockloc_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} directories", ufs_sb.fs_cstotal_ndir_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} free blocks ({1} bytes)", ufs_sb.fs_cstotal_nbfree_ufs2, ufs_sb.fs_cstotal_nbfree_ufs2 * ufs_sb.fs_bsize).AppendLine();
                sbInformation.AppendFormat("{0} free inodes", ufs_sb.fs_cstotal_nifree_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} free frags", ufs_sb.fs_cstotal_nffree_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} free clusters", ufs_sb.fs_cstotal_numclusters_ufs2).AppendLine();
                sbInformation.AppendFormat("Volume last written on {0}", DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_time_sec_ufs2)).AppendLine();
                sbInformation.AppendFormat("{0} blocks ({1} bytes)", ufs_sb.fs_size_ufs2, ufs_sb.fs_size_ufs2 * ufs_sb.fs_bsize).AppendLine();
                sbInformation.AppendFormat("{0} data blocks ({1} bytes)", ufs_sb.fs_dsize_ufs2, ufs_sb.fs_dsize_ufs2 * ufs_sb.fs_bsize).AppendLine();
                sbInformation.AppendFormat("Cylinder group summary area LBA: {0}", ufs_sb.fs_csaddr_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} blocks pending of being freed", ufs_sb.fs_pendingblocks_ufs2).AppendLine();
                sbInformation.AppendFormat("{0} inodes pending of being freed", ufs_sb.fs_pendinginodes_ufs2).AppendLine();
            }
            if (fs_type_sun)
            {
                sbInformation.AppendFormat("Volume state on {0}", DateHandlers.UNIXUnsignedToDateTime(ufs_sb.fs_state_t_sun)).AppendLine();
            }
            else if (fs_type_sun86)
            {
                sbInformation.AppendFormat("{0} sectors/track", ufs_sb.fs_npsect_sun86).AppendLine();
            }
            else if (fs_type_44bsd)
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
        const ulong sb_start_floppy = 0;
        // For floppies, start at offset 0
        const ulong sb_start_ufs1 = 1;
        // For normal devices, start at offset 8192
        const ulong sb_start_ufs2 = 8;
        // For UFS2, start at offset 65536
        const ulong sb_start_piggy = 32;
        // For piggy devices (?), start at offset 262144
        // MAGICs
        const UInt32 UFS_MAGIC = 0x00011954;
        // UFS magic
        const UInt32 UFS_MAGIC_BW = 0x0f242697;
        // BorderWare UFS
        const UInt32 UFS2_MAGIC = 0x19540119;
        // UFS2 magic
        const UInt32 UFS_CIGAM = 0x54190100;
        // byteswapped
        const UInt32 UFS_BAD_MAGIC = 0x19960408;
        // Incomplete newfs
        // On-disk superblock is quite a mixture of all the UFS/FFS variants
        // There is no clear way to detect which one is correct
        // And as C# does not support unions this struct will clearly appear quite dirty :p
        // To clean up things a little, comment starts with relative superblock offset of field
        // Biggest sized supleblock would be 1377 bytes
        public struct UFSSuperBlock
        {
            #region 42BSD

            public UInt32 fs_link_42bsd;
            // 0x0000 linked list of file systems

            #endregion

            #region Sun

            public UInt32 fs_state_sun;
            // 0x0000 file system state flag

            #endregion

            #region COMMON

            public UInt32 fs_rlink;
            // 0x0004 used for incore super blocks
            public UInt32 fs_sblkno;
            // 0x0008 addr of super-block in filesys
            public UInt32 fs_cblkno;
            // 0x000C offset of cyl-block in filesys
            public UInt32 fs_iblkno;
            // 0x0010 offset of inode-blocks in filesys
            public UInt32 fs_dblkno;
            // 0x0014 offset of first data after cg
            public UInt32 fs_cgoffset;
            // 0x0018 cylinder group offset in cylinder
            public UInt32 fs_cgmask;
            // 0x001C used to calc mod fs_ntrak
            public UInt32 fs_time_t;
            // 0x0020 last time written -- time_t
            public UInt32 fs_size;
            // 0x0024 number of blocks in fs
            public UInt32 fs_dsize;
            // 0x0028 number of data blocks in fs
            public UInt32 fs_ncg;
            // 0x002C number of cylinder groups
            public UInt32 fs_bsize;
            // 0x0030 size of basic blocks in fs
            public UInt32 fs_fsize;
            // 0x0034 size of frag blocks in fs
            public UInt32 fs_frag;
            // 0x0038 number of frags in a block in fs
            // these are configuration parameters
            public UInt32 fs_minfree;
            // 0x003C minimum percentage of free blocks
            public UInt32 fs_rotdelay;
            // 0x0040 num of ms for optimal next block
            public UInt32 fs_rps;
            // 0x0044 disk revolutions per second
            // these fields can be computed from the others
            public UInt32 fs_bmask;
            // 0x0048 ``blkoff'' calc of blk offsets
            public UInt32 fs_fmask;
            // 0x004C ``fragoff'' calc of frag offsets
            public UInt32 fs_bshift;
            // 0x0050 ``lblkno'' calc of logical blkno
            public UInt32 fs_fshift;
            // 0x0054 ``numfrags'' calc number of frags
            // these are configuration parameters
            public UInt32 fs_maxcontig;
            // 0x0058 max number of contiguous blks
            public UInt32 fs_maxbpg;
            // 0x005C max number of blks per cyl group
            // these fields can be computed from the others
            public UInt32 fs_fragshift;
            // 0x0060 block to frag shift
            public UInt32 fs_fsbtodb;
            // 0x0064 fsbtodb and dbtofsb shift constant
            public UInt32 fs_sbsize;
            // 0x0068 actual size of super block
            public UInt32 fs_csmask;
            // 0x006C csum block offset
            public UInt32 fs_csshift;
            // 0x0070 csum block number
            public UInt32 fs_nindir;
            // 0x0074 value of NINDIR
            public UInt32 fs_inopb;
            // 0x0078 value of INOPB
            public UInt32 fs_nspf;
            // 0x007C value of NSPF
            // yet another configuration parameter
            public UInt32 fs_optim;
            // 0x0080 optimization preference, see below

            #endregion COMMON

            // these fields are derived from the hardware

            #region Sun

            public UInt32 fs_npsect_sun;
            // 0x0084 # sectors/track including spares

            #endregion Sun

            #region Sunx86

            public UInt32 fs_state_t_sun86;
            // 0x0084 file system state time stamp

            #endregion Sunx86

            #region COMMON

            public UInt32 fs_interleave;
            // 0x0088 hardware sector interleave
            public UInt32 fs_trackskew;
            // 0x008C sector 0 skew, per track

            #endregion COMMON

            // a unique id for this filesystem (currently unused and unmaintained)
            // In 4.3 Tahoe this space is used by fs_headswitch and fs_trkseek
            // Neither of those fields is used in the Tahoe code right now but
            // there could be problems if they are.

            #region COMMON

            public UInt32 fs_id_1;
            // 0x0090
            public UInt32 fs_id_2;
            // 0x0094

            #endregion COMMON

            #region 43BSD

            public UInt32 fs_headswitch_43bsd;
            // 0x0090 head switch time, usec
            public UInt32 fs_trkseek_43bsd;
            // 0x0094 track-to-track seek, usec

            #endregion 43BSD

            #region COMMON

            // sizes determined by number of cylinder groups and their sizes
            public UInt32 fs_csaddr;
            // 0x0098 blk addr of cyl grp summary area
            public UInt32 fs_cssize;
            // 0x009C size of cyl grp summary area
            public UInt32 fs_cgsize;
            // 0x00A0 cylinder group size
            // these fields are derived from the hardware
            public UInt32 fs_ntrak;
            // 0x00A4 tracks per cylinder
            public UInt32 fs_nsect;
            // 0x00A8 sectors per track
            public UInt32 fs_spc;
            // 0x00AC sectors per cylinder
            // this comes from the disk driver partitioning
            public UInt32 fs_ncyl;
            // 0x00B0 cylinders in file system
            // these fields can be computed from the others
            public UInt32 fs_cpg;
            // 0x00B4 cylinders per group
            public UInt32 fs_ipg;
            // 0x00B8 inodes per cylinder group
            public UInt32 fs_fpg;
            // 0x00BC blocks per group * fs_frag
            // this data must be re-computed after crashes
            // struct ufs_csum fs_cstotal;	// cylinder summary information
            public UInt32 fs_cstotal_ndir;
            // 0x00C0 number of directories
            public UInt32 fs_cstotal_nbfree;
            // 0x00C4 number of free blocks
            public UInt32 fs_cstotal_nifree;
            // 0x00C8 number of free inodes
            public UInt32 fs_cstotal_nffree;
            // 0x00CC number of free frags
            // these fields are cleared at mount time
            public byte fs_fmod;
            // 0x00D0 super block modified flag
            public byte fs_clean;
            // 0x00D1 file system is clean flag
            public byte fs_ronly;
            // 0x00D2 mounted read-only flag
            public byte fs_flags;
            // 0x00D3

            #endregion common

            #region UFS1

            public string fs_fsmnt_ufs1;
            // 0x00D4, 512 bytes, name mounted on
            public UInt32 fs_cgrotor_ufs1;
            // 0x02D4 last cg searched
            public byte[] fs_cs_ufs1;
            // 0x02D8, 124 bytes, UInt32s, list of fs_cs info buffers
            public UInt32 fs_maxcluster_ufs1;
            // 0x0354
            public UInt32 fs_cpc_ufs1;
            // 0x0358 cyl per cycle in postbl
            public byte[] fs_opostbl_ufs1;
            // 0x035C, 256 bytes, [16][8] matrix of UInt16s, old rotation block list head

            #endregion UFS1

            #region UFS2

            public string fs_fsmnt_ufs2;
            // 0x00D4, 468 bytes, name mounted on
            public string fs_volname_ufs2;
            // 0x02A8, 32 bytes, volume name
            public UInt64 fs_swuid_ufs2;
            // 0x02C8 system-wide uid
            public UInt32 fs_pad_ufs2;
            // 0x02D0 due to alignment of fs_swuid
            public UInt32 fs_cgrotor_ufs2;
            // 0x02D4 last cg searched
            public byte[] fs_ocsp_ufs2;
            // 0x02D8, 112 bytes, UInt32s, list of fs_cs info buffers
            public UInt32 fs_contigdirs_ufs2;
            // 0x0348 # of contiguously allocated dirs
            public UInt32 fs_csp_ufs2;
            // 0x034C cg summary info buffer for fs_cs
            public UInt32 fs_maxcluster_ufs2;
            // 0x0350
            public UInt32 fs_active_ufs2;
            // 0x0354 used by snapshots to track fs
            public UInt32 fs_old_cpc_ufs2;
            // 0x0358 cyl per cycle in postbl
            public UInt32 fs_maxbsize_ufs2;
            // 0x035C maximum blocking factor permitted
            public byte[] fs_sparecon64_ufs2;
            // 0x0360, 136 bytes, UInt64s, old rotation block list head
            public UInt64 fs_sblockloc_ufs2;
            // 0x03E8 byte offset of standard superblock
            //cylinder summary information*/
            public UInt64 fs_cstotal_ndir_ufs2;
            // 0x03F0 number of directories
            public UInt64 fs_cstotal_nbfree_ufs2;
            // 0x03F8 number of free blocks
            public UInt64 fs_cstotal_nifree_ufs2;
            // 0x0400 number of free inodes
            public UInt64 fs_cstotal_nffree_ufs2;
            // 0x0408 number of free frags
            public UInt64 fs_cstotal_numclusters_ufs2;
            // 0x0410 number of free clusters
            public UInt64 fs_cstotal_spare0_ufs2;
            // 0x0418 future expansion
            public UInt64 fs_cstotal_spare1_ufs2;
            // 0x0420 future expansion
            public UInt64 fs_cstotal_spare2_ufs2;
            // 0x0428 future expansion
            public UInt32 fs_time_sec_ufs2;
            // 0x0430 last time written
            public UInt32 fs_time_usec_ufs2;
            // 0x0434 last time written
            public UInt64 fs_size_ufs2;
            // 0x0438 number of blocks in fs
            public UInt64 fs_dsize_ufs2;
            // 0x0440 number of data blocks in fs
            public UInt64 fs_csaddr_ufs2;
            // 0x0448 blk addr of cyl grp summary area
            public UInt64 fs_pendingblocks_ufs2;
            // 0x0450 blocks in process of being freed
            public UInt32 fs_pendinginodes_ufs2;
            // 0x0458 inodes in process of being freed

            #endregion UFS2

            #region Sun

            public byte[] fs_sparecon_sun;
            // 0x045C, 212 bytes, reserved for future constants
            public UInt32 fs_reclaim_sun;
            // 0x0530
            public UInt32 fs_sparecon2_sun;
            // 0x0534
            public UInt32 fs_state_t_sun;
            // 0x0538 file system state time stamp
            public UInt32 fs_qbmask0_sun;
            // 0x053C ~usb_bmask
            public UInt32 fs_qbmask1_sun;
            // 0x0540 ~usb_bmask
            public UInt32 fs_qfmask0_sun;
            // 0x0544 ~usb_fmask
            public UInt32 fs_qfmask1_sun;
            // 0x0548 ~usb_fmask

            #endregion Sun

            #region Sunx86

            public byte[] fs_sparecon_sun86;
            // 0x045C, 212 bytes, reserved for future constants
            public UInt32 fs_reclaim_sun86;
            // 0x0530
            public UInt32 fs_sparecon2_sun86;
            // 0x0534
            public UInt32 fs_npsect_sun86;
            // 0x0538 # sectors/track including spares
            public UInt32 fs_qbmask0_sun86;
            // 0x053C ~usb_bmask
            public UInt32 fs_qbmask1_sun86;
            // 0x0540 ~usb_bmask
            public UInt32 fs_qfmask0_sun86;
            // 0x0544 ~usb_fmask
            public UInt32 fs_qfmask1_sun86;
            // 0x0548 ~usb_fmask

            #endregion Sunx86

            #region 44BSD

            public byte[] fs_sparecon_44bsd;
            // 0x045C, 200 bytes
            public UInt32 fs_contigsumsize_44bsd;
            // 0x0524 size of cluster summary array
            public UInt32 fs_maxsymlinklen_44bsd;
            // 0x0528 max length of an internal symlink
            public UInt32 fs_inodefmt_44bsd;
            // 0x052C format of on-disk inodes
            public UInt32 fs_maxfilesize0_44bsd;
            // 0x0530 max representable file size
            public UInt32 fs_maxfilesize1_44bsd;
            // 0x0534 max representable file size
            public UInt32 fs_qbmask0_44bsd;
            // 0x0538 ~usb_bmask
            public UInt32 fs_qbmask1_44bsd;
            // 0x053C ~usb_bmask
            public UInt32 fs_qfmask0_44bsd;
            // 0x0540 ~usb_fmask
            public UInt32 fs_qfmask1_44bsd;
            // 0x0544 ~usb_fmask
            public UInt32 fs_state_t_44bsd;
            // 0x0548 file system state time stamp

            #endregion 44BSD

            public UInt32 fs_postblformat;
            // 0x054C format of positional layout tables
            public UInt32 fs_nrpos;
            // 0x0550 number of rotational positions
            public UInt32 fs_postbloff;
            // 0x0554 (__s16) rotation block list head
            public UInt32 fs_rotbloff;
            // 0x0558 (__u8) blocks for each rotation
            public UInt32 fs_magic;
            // 0x055C magic number
            public byte fs_space;
            // 0x0560 list of blocks for each rotation
            // 0x0561
        }
    }
}