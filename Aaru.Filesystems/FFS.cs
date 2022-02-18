// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;
using time_t = System.Int32;
using ufs_daddr_t = System.Int32;

namespace Aaru.Filesystems
{
    // Using information from Linux kernel headers
    /// <inheritdoc />
    /// <summary>Implements detection of BSD Fast File System (FFS, aka UNIX File System)</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class FFSPlugin : IFilesystem
    {
        const uint block_size = 8192;

        // FreeBSD specifies starts at byte offsets 0, 8192, 65536 and 262144, but in other cases it's following sectors
        // Without bootcode
        const ulong sb_start_floppy = 0;

        // With bootcode
        const ulong sb_start_boot = 1;

        // Dunno, longer boot code
        const ulong sb_start_long_boot = 8;

        // Found on AT&T for MD-2D floppieslzio
        const ulong sb_start_att_dsdd = 14;

        // Found on hard disks (Atari UNIX e.g.)
        const ulong sb_start_piggy = 32;

        // MAGICs
        // UFS magic
        const uint UFS_MAGIC = 0x00011954;

        // Big-endian UFS magic
        const uint UFS_CIGAM = 0x54190100;

        // BorderWare UFS
        const uint UFS_MAGIC_BW = 0x0F242697;

        // Big-endian BorderWare UFS
        const uint UFS_CIGAM_BW = 0x9726240F;

        // UFS2 magic
        const uint UFS2_MAGIC = 0x19540119;

        // Big-endian UFS2 magic
        const uint UFS2_CIGAM = 0x19015419;

        // Incomplete newfs
        const uint UFS_BAD_MAGIC = 0x19960408;

        // Big-endian incomplete newfs
        const uint UFS_BAD_CIGAM = 0x08049619;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "BSD Fast File System (aka UNIX File System, UFS)";
        /// <inheritdoc />
        public Guid Id => new("CC90D342-05DB-48A8-988C-C1FE000034A3");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End)
                return false;

            uint sbSizeInSectors;

            if(imagePlugin.Info.SectorSize == 2336 ||
               imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448)
                sbSizeInSectors = block_size / 2048;
            else
                sbSizeInSectors = block_size / imagePlugin.Info.SectorSize;

            ulong[] locations =
            {
                sb_start_floppy, sb_start_boot, sb_start_long_boot, sb_start_piggy, sb_start_att_dsdd,
                8192   / imagePlugin.Info.SectorSize, 65536 / imagePlugin.Info.SectorSize,
                262144 / imagePlugin.Info.SectorSize
            };

            try
            {
                foreach(ulong loc in locations)
                {
                    if(partition.End > partition.Start + loc + sbSizeInSectors)
                    {
                        ErrorNumber errno =
                            imagePlugin.ReadSectors(partition.Start + loc, sbSizeInSectors, out byte[] ufsSbSectors);

                        if(errno != ErrorNumber.NoError)
                            continue;

                        uint magic = BitConverter.ToUInt32(ufsSbSectors, 0x055C);

                        if(magic == UFS_MAGIC     ||
                           magic == UFS_CIGAM     ||
                           magic == UFS_MAGIC_BW  ||
                           magic == UFS_CIGAM_BW  ||
                           magic == UFS2_MAGIC    ||
                           magic == UFS2_CIGAM    ||
                           magic == UFS_BAD_MAGIC ||
                           magic == UFS_BAD_CIGAM)
                            return true;
                    }
                }

                return false;
            }
            catch(Exception)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";
            var sbInformation = new StringBuilder();

            uint   magic = 0;
            uint   sb_size_in_sectors;
            byte[] ufs_sb_sectors;
            ulong  sb_offset     = partition.Start;
            bool   fs_type_42bsd = false;
            bool   fs_type_43bsd = false;
            bool   fs_type_44bsd = false;
            bool   fs_type_ufs   = false;
            bool   fs_type_ufs2  = false;
            bool   fs_type_sun   = false;
            bool   fs_type_sun86 = false;

            if(imagePlugin.Info.SectorSize == 2336 ||
               imagePlugin.Info.SectorSize == 2352 ||
               imagePlugin.Info.SectorSize == 2448)
                sb_size_in_sectors = block_size / 2048;
            else
                sb_size_in_sectors = block_size / imagePlugin.Info.SectorSize;

            ulong[] locations =
            {
                sb_start_floppy, sb_start_boot, sb_start_long_boot, sb_start_piggy, sb_start_att_dsdd,
                8192   / imagePlugin.Info.SectorSize, 65536 / imagePlugin.Info.SectorSize,
                262144 / imagePlugin.Info.SectorSize
            };

            ErrorNumber errno;

            foreach(ulong loc in locations.Where(loc => partition.End > partition.Start + loc + sb_size_in_sectors))
            {
                errno = imagePlugin.ReadSectors(partition.Start + loc, sb_size_in_sectors, out ufs_sb_sectors);

                if(errno != ErrorNumber.NoError)
                    continue;

                magic = BitConverter.ToUInt32(ufs_sb_sectors, 0x055C);

                if(magic == UFS_MAGIC     ||
                   magic == UFS_CIGAM     ||
                   magic == UFS_MAGIC_BW  ||
                   magic == UFS_CIGAM_BW  ||
                   magic == UFS2_MAGIC    ||
                   magic == UFS2_CIGAM    ||
                   magic == UFS_BAD_MAGIC ||
                   magic == UFS_BAD_CIGAM)
                {
                    sb_offset = partition.Start + loc;

                    break;
                }

                magic = 0;
            }

            if(magic == 0)
            {
                information = "Not a UFS filesystem, I shouldn't have arrived here!";

                return;
            }

            XmlFsType = new FileSystemType();

            switch(magic)
            {
                case UFS_MAGIC:
                    sbInformation.AppendLine("UFS filesystem");
                    XmlFsType.Type = "UFS";

                    break;
                case UFS_CIGAM:
                    sbInformation.AppendLine("Big-endian UFS filesystem");
                    XmlFsType.Type = "UFS";

                    break;
                case UFS_MAGIC_BW:
                    sbInformation.AppendLine("BorderWare UFS filesystem");
                    XmlFsType.Type = "UFS";

                    break;
                case UFS_CIGAM_BW:
                    sbInformation.AppendLine("Big-endian BorderWare UFS filesystem");
                    XmlFsType.Type = "UFS";

                    break;
                case UFS2_MAGIC:
                    sbInformation.AppendLine("UFS2 filesystem");
                    XmlFsType.Type = "UFS2";

                    break;
                case UFS2_CIGAM:
                    sbInformation.AppendLine("Big-endian UFS2 filesystem");
                    XmlFsType.Type = "UFS2";

                    break;
                case UFS_BAD_MAGIC:
                    sbInformation.AppendLine("Incompletely initialized UFS filesystem");
                    sbInformation.AppendLine("BEWARE!!! Following information may be completely wrong!");
                    XmlFsType.Type = "UFS";

                    break;
                case UFS_BAD_CIGAM:
                    sbInformation.AppendLine("Incompletely initialized big-endian UFS filesystem");
                    sbInformation.AppendLine("BEWARE!!! Following information may be completely wrong!");
                    XmlFsType.Type = "UFS";

                    break;
            }

            // Fun with seeking follows on superblock reading!
            errno = imagePlugin.ReadSectors(sb_offset, sb_size_in_sectors, out ufs_sb_sectors);

            if(errno != ErrorNumber.NoError)
                return;

            SuperBlock sb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(ufs_sb_sectors);

            SuperBlock bs_sfu = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(ufs_sb_sectors);

            if((bs_sfu.fs_magic == UFS_MAGIC     && sb.fs_magic == UFS_CIGAM)    ||
               (bs_sfu.fs_magic == UFS_MAGIC_BW  && sb.fs_magic == UFS_CIGAM_BW) ||
               (bs_sfu.fs_magic == UFS2_MAGIC    && sb.fs_magic == UFS2_CIGAM)   ||
               (bs_sfu.fs_magic == UFS_BAD_MAGIC && sb.fs_magic == UFS_BAD_CIGAM))
            {
                sb                           = bs_sfu;
                sb.fs_old_cstotal.cs_nbfree  = Swapping.Swap(sb.fs_old_cstotal.cs_nbfree);
                sb.fs_old_cstotal.cs_ndir    = Swapping.Swap(sb.fs_old_cstotal.cs_ndir);
                sb.fs_old_cstotal.cs_nffree  = Swapping.Swap(sb.fs_old_cstotal.cs_nffree);
                sb.fs_old_cstotal.cs_nifree  = Swapping.Swap(sb.fs_old_cstotal.cs_nifree);
                sb.fs_cstotal.cs_numclusters = Swapping.Swap(sb.fs_cstotal.cs_numclusters);
                sb.fs_cstotal.cs_nbfree      = Swapping.Swap(sb.fs_cstotal.cs_nbfree);
                sb.fs_cstotal.cs_ndir        = Swapping.Swap(sb.fs_cstotal.cs_ndir);
                sb.fs_cstotal.cs_nffree      = Swapping.Swap(sb.fs_cstotal.cs_nffree);
                sb.fs_cstotal.cs_nifree      = Swapping.Swap(sb.fs_cstotal.cs_nifree);
                sb.fs_cstotal.cs_spare[0]    = Swapping.Swap(sb.fs_cstotal.cs_spare[0]);
                sb.fs_cstotal.cs_spare[1]    = Swapping.Swap(sb.fs_cstotal.cs_spare[1]);
                sb.fs_cstotal.cs_spare[2]    = Swapping.Swap(sb.fs_cstotal.cs_spare[2]);
            }

            AaruConsole.DebugWriteLine("FFS plugin", "sb offset: 0x{0:X8}", sb_offset);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_rlink: 0x{0:X8}", sb.fs_rlink);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_sblkno: 0x{0:X8}", sb.fs_sblkno);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_cblkno: 0x{0:X8}", sb.fs_cblkno);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_iblkno: 0x{0:X8}", sb.fs_iblkno);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_dblkno: 0x{0:X8}", sb.fs_dblkno);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_size: 0x{0:X8}", sb.fs_size);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_dsize: 0x{0:X8}", sb.fs_dsize);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_ncg: 0x{0:X8}", sb.fs_ncg);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_bsize: 0x{0:X8}", sb.fs_bsize);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_fsize: 0x{0:X8}", sb.fs_fsize);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_frag: 0x{0:X8}", sb.fs_frag);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_minfree: 0x{0:X8}", sb.fs_minfree);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_bmask: 0x{0:X8}", sb.fs_bmask);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_fmask: 0x{0:X8}", sb.fs_fmask);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_bshift: 0x{0:X8}", sb.fs_bshift);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_fshift: 0x{0:X8}", sb.fs_fshift);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_maxcontig: 0x{0:X8}", sb.fs_maxcontig);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_maxbpg: 0x{0:X8}", sb.fs_maxbpg);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_fragshift: 0x{0:X8}", sb.fs_fragshift);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_fsbtodb: 0x{0:X8}", sb.fs_fsbtodb);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_sbsize: 0x{0:X8}", sb.fs_sbsize);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_csmask: 0x{0:X8}", sb.fs_csmask);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_csshift: 0x{0:X8}", sb.fs_csshift);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_nindir: 0x{0:X8}", sb.fs_nindir);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_inopb: 0x{0:X8}", sb.fs_inopb);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_optim: 0x{0:X8}", sb.fs_optim);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_id_1: 0x{0:X8}", sb.fs_id_1);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_id_2: 0x{0:X8}", sb.fs_id_2);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_csaddr: 0x{0:X8}", sb.fs_csaddr);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_cssize: 0x{0:X8}", sb.fs_cssize);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_cgsize: 0x{0:X8}", sb.fs_cgsize);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_ipg: 0x{0:X8}", sb.fs_ipg);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_fpg: 0x{0:X8}", sb.fs_fpg);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_fmod: 0x{0:X2}", sb.fs_fmod);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_clean: 0x{0:X2}", sb.fs_clean);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_ronly: 0x{0:X2}", sb.fs_ronly);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_flags: 0x{0:X2}", sb.fs_flags);
            AaruConsole.DebugWriteLine("FFS plugin", "fs_magic: 0x{0:X8}", sb.fs_magic);

            if(sb.fs_magic == UFS2_MAGIC)
                fs_type_ufs2 = true;
            else
            {
                const uint
                    SunOSEpoch =
                        0x1A54C580; // We are supposing there cannot be a Sun's fs created before 1/1/1982 00:00:00

                fs_type_43bsd =
                    true; // There is no way of knowing this is the version, but there is of knowing it is not.

                if(sb.fs_link > 0)
                {
                    fs_type_42bsd = true; // It was used in 4.2BSD
                    fs_type_43bsd = false;
                }

                if((sb.fs_maxfilesize & 0xFFFFFFFF)                                    > SunOSEpoch &&
                   DateHandlers.UnixUnsignedToDateTime(sb.fs_maxfilesize & 0xFFFFFFFF) < DateTime.Now)
                {
                    fs_type_42bsd = false;
                    fs_type_sun   = true;
                    fs_type_43bsd = false;
                }

                // This is for sure, as it is shared with a sectors/track with non-x86 SunOS, Epoch is absurdly high for that
                if(sb.fs_old_npsect                              > SunOSEpoch &&
                   DateHandlers.UnixToDateTime(sb.fs_old_npsect) < DateTime.Now)
                {
                    fs_type_42bsd = false;
                    fs_type_sun86 = true;
                    fs_type_sun   = false;
                    fs_type_43bsd = false;
                }

                if(sb.fs_cgrotor       > 0x00000000 &&
                   (uint)sb.fs_cgrotor < 0xFFFFFFFF)
                {
                    fs_type_42bsd = false;
                    fs_type_sun   = false;
                    fs_type_sun86 = false;
                    fs_type_ufs   = true;
                    fs_type_43bsd = false;
                }

                // 4.3BSD code does not use these fields, they are always set up to 0
                fs_type_43bsd &= sb.fs_id_2 == 0 && sb.fs_id_1 == 0;

                // This is the only 4.4BSD inode format
                fs_type_44bsd |= sb.fs_old_inodefmt == 2;
            }

            if(!fs_type_ufs2)
            {
                sbInformation.AppendLine("There are a lot of variants of UFS using overlapped values on same fields");

                sbInformation.
                    AppendLine("I will try to guess which one it is, but unless it's UFS2, I may be surely wrong");
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

            if(fs_type_42bsd)
                sbInformation.AppendFormat("Linked list of filesystems: 0x{0:X8}", sb.fs_link).AppendLine();

            sbInformation.AppendFormat("Superblock LBA: {0}", sb.fs_sblkno).AppendLine();
            sbInformation.AppendFormat("Cylinder-block LBA: {0}", sb.fs_cblkno).AppendLine();
            sbInformation.AppendFormat("inode-block LBA: {0}", sb.fs_iblkno).AppendLine();
            sbInformation.AppendFormat("First data block LBA: {0}", sb.fs_dblkno).AppendLine();
            sbInformation.AppendFormat("Cylinder group offset in cylinder: {0}", sb.fs_old_cgoffset).AppendLine();

            sbInformation.AppendFormat("Volume last written on {0}", DateHandlers.UnixToDateTime(sb.fs_old_time)).
                          AppendLine();

            XmlFsType.ModificationDate          = DateHandlers.UnixToDateTime(sb.fs_old_time);
            XmlFsType.ModificationDateSpecified = true;

            sbInformation.AppendFormat("{0} blocks in volume ({1} bytes)", sb.fs_old_size,
                                       (long)sb.fs_old_size * sb.fs_fsize).AppendLine();

            XmlFsType.Clusters    = (ulong)sb.fs_old_size;
            XmlFsType.ClusterSize = (uint)sb.fs_fsize;

            sbInformation.AppendFormat("{0} data blocks in volume ({1} bytes)", sb.fs_old_dsize,
                                       (long)sb.fs_old_dsize * sb.fs_fsize).AppendLine();

            sbInformation.AppendFormat("{0} cylinder groups in volume", sb.fs_ncg).AppendLine();
            sbInformation.AppendFormat("{0} bytes in a basic block", sb.fs_bsize).AppendLine();
            sbInformation.AppendFormat("{0} bytes in a frag block", sb.fs_fsize).AppendLine();
            sbInformation.AppendFormat("{0} frags in a block", sb.fs_frag).AppendLine();
            sbInformation.AppendFormat("{0}% of blocks must be free", sb.fs_minfree).AppendLine();
            sbInformation.AppendFormat("{0}ms for optimal next block", sb.fs_old_rotdelay).AppendLine();

            sbInformation.AppendFormat("disk rotates {0} times per second ({1}rpm)", sb.fs_old_rps, sb.fs_old_rps * 60).
                          AppendLine();

            /*          sbInformation.AppendFormat("fs_bmask: 0x{0:X8}", sb.fs_bmask).AppendLine();
                        sbInformation.AppendFormat("fs_fmask: 0x{0:X8}", sb.fs_fmask).AppendLine();
                        sbInformation.AppendFormat("fs_bshift: 0x{0:X8}", sb.fs_bshift).AppendLine();
                        sbInformation.AppendFormat("fs_fshift: 0x{0:X8}", sb.fs_fshift).AppendLine();*/
            sbInformation.AppendFormat("{0} contiguous blocks at maximum", sb.fs_maxcontig).AppendLine();
            sbInformation.AppendFormat("{0} blocks per cylinder group at maximum", sb.fs_maxbpg).AppendLine();
            sbInformation.AppendFormat("Superblock is {0} bytes", sb.fs_sbsize).AppendLine();
            sbInformation.AppendFormat("NINDIR: 0x{0:X8}", sb.fs_nindir).AppendLine();
            sbInformation.AppendFormat("INOPB: 0x{0:X8}", sb.fs_inopb).AppendLine();
            sbInformation.AppendFormat("NSPF: 0x{0:X8}", sb.fs_old_nspf).AppendLine();

            switch(sb.fs_optim)
            {
                case 0:
                    sbInformation.AppendLine("Filesystem will minimize allocation time");

                    break;
                case 1:
                    sbInformation.AppendLine("Filesystem will minimize volume fragmentation");

                    break;
                default:
                    sbInformation.AppendFormat("Unknown optimization value: 0x{0:X8}", sb.fs_optim).AppendLine();

                    break;
            }

            if(fs_type_sun)
                sbInformation.AppendFormat("{0} sectors/track", sb.fs_old_npsect).AppendLine();
            else if(fs_type_sun86)
                sbInformation.AppendFormat("Volume state on {0}", DateHandlers.UnixToDateTime(sb.fs_old_npsect)).
                              AppendLine();

            sbInformation.AppendFormat("Hardware sector interleave: {0}", sb.fs_old_interleave).AppendLine();
            sbInformation.AppendFormat("Sector 0 skew: {0}/track", sb.fs_old_trackskew).AppendLine();

            if(!fs_type_43bsd &&
               sb.fs_id_1 > 0 &&
               sb.fs_id_2 > 0)
                sbInformation.AppendFormat("Volume ID: 0x{0:X8}{1:X8}", sb.fs_id_1, sb.fs_id_2).AppendLine();
            else if(fs_type_43bsd  &&
                    sb.fs_id_1 > 0 &&
                    sb.fs_id_2 > 0)
            {
                sbInformation.AppendFormat("{0} µsec for head switch", sb.fs_id_1).AppendLine();
                sbInformation.AppendFormat("{0} µsec for track-to-track seek", sb.fs_id_2).AppendLine();
            }

            sbInformation.AppendFormat("Cylinder group summary LBA: {0}", sb.fs_old_csaddr).AppendLine();
            sbInformation.AppendFormat("{0} bytes in cylinder group summary", sb.fs_cssize).AppendLine();
            sbInformation.AppendFormat("{0} bytes in cylinder group", sb.fs_cgsize).AppendLine();
            sbInformation.AppendFormat("{0} tracks/cylinder", sb.fs_old_ntrak).AppendLine();
            sbInformation.AppendFormat("{0} sectors/track", sb.fs_old_nsect).AppendLine();
            sbInformation.AppendFormat("{0} sectors/cylinder", sb.fs_old_spc).AppendLine();
            sbInformation.AppendFormat("{0} cylinder in volume", sb.fs_old_ncyl).AppendLine();
            sbInformation.AppendFormat("{0} cylinders/group", sb.fs_old_cpg).AppendLine();
            sbInformation.AppendFormat("{0} inodes per cylinder group", sb.fs_ipg).AppendLine();
            sbInformation.AppendFormat("{0} blocks per group", sb.fs_fpg / sb.fs_frag).AppendLine();
            sbInformation.AppendFormat("{0} directories", sb.fs_old_cstotal.cs_ndir).AppendLine();

            sbInformation.AppendFormat("{0} free blocks ({1} bytes)", sb.fs_old_cstotal.cs_nbfree,
                                       (long)sb.fs_old_cstotal.cs_nbfree * sb.fs_fsize).AppendLine();

            XmlFsType.FreeClusters          = (ulong)sb.fs_old_cstotal.cs_nbfree;
            XmlFsType.FreeClustersSpecified = true;
            sbInformation.AppendFormat("{0} free inodes", sb.fs_old_cstotal.cs_nifree).AppendLine();
            sbInformation.AppendFormat("{0} free frags", sb.fs_old_cstotal.cs_nffree).AppendLine();

            if(sb.fs_fmod == 1)
            {
                sbInformation.AppendLine("Superblock is under modification");
                XmlFsType.Dirty = true;
            }

            if(sb.fs_clean == 1)
                sbInformation.AppendLine("Volume is clean");

            if(sb.fs_ronly == 1)
                sbInformation.AppendLine("Volume is read-only");

            sbInformation.AppendFormat("Volume flags: 0x{0:X2}", sb.fs_flags).AppendLine();

            if(fs_type_ufs)
                sbInformation.AppendFormat("Volume last mounted on \"{0}\"", StringHandlers.CToString(sb.fs_fsmnt)).
                              AppendLine();
            else if(fs_type_ufs2)
            {
                sbInformation.AppendFormat("Volume last mounted on \"{0}\"", StringHandlers.CToString(sb.fs_fsmnt)).
                              AppendLine();

                sbInformation.AppendFormat("Volume name: \"{0}\"", StringHandlers.CToString(sb.fs_volname)).
                              AppendLine();

                XmlFsType.VolumeName = StringHandlers.CToString(sb.fs_volname);
                sbInformation.AppendFormat("Volume ID: 0x{0:X16}", sb.fs_swuid).AppendLine();

                //xmlFSType.VolumeSerial = string.Format("{0:X16}", sb.fs_swuid);
                sbInformation.AppendFormat("Last searched cylinder group: {0}", sb.fs_cgrotor).AppendLine();
                sbInformation.AppendFormat("{0} contiguously allocated directories", sb.fs_contigdirs).AppendLine();
                sbInformation.AppendFormat("Standard superblock LBA: {0}", sb.fs_sblkno).AppendLine();
                sbInformation.AppendFormat("{0} directories", sb.fs_cstotal.cs_ndir).AppendLine();

                sbInformation.AppendFormat("{0} free blocks ({1} bytes)", sb.fs_cstotal.cs_nbfree,
                                           sb.fs_cstotal.cs_nbfree * sb.fs_fsize).AppendLine();

                XmlFsType.FreeClusters          = (ulong)sb.fs_cstotal.cs_nbfree;
                XmlFsType.FreeClustersSpecified = true;
                sbInformation.AppendFormat("{0} free inodes", sb.fs_cstotal.cs_nifree).AppendLine();
                sbInformation.AppendFormat("{0} free frags", sb.fs_cstotal.cs_nffree).AppendLine();
                sbInformation.AppendFormat("{0} free clusters", sb.fs_cstotal.cs_numclusters).AppendLine();

                sbInformation.AppendFormat("Volume last written on {0}", DateHandlers.UnixToDateTime(sb.fs_time)).
                              AppendLine();

                XmlFsType.ModificationDate          = DateHandlers.UnixToDateTime(sb.fs_time);
                XmlFsType.ModificationDateSpecified = true;

                sbInformation.AppendFormat("{0} blocks ({1} bytes)", sb.fs_size, sb.fs_size * sb.fs_fsize).AppendLine();

                XmlFsType.Clusters = (ulong)sb.fs_size;

                sbInformation.AppendFormat("{0} data blocks ({1} bytes)", sb.fs_dsize, sb.fs_dsize * sb.fs_fsize).
                              AppendLine();

                sbInformation.AppendFormat("Cylinder group summary area LBA: {0}", sb.fs_csaddr).AppendLine();
                sbInformation.AppendFormat("{0} blocks pending of being freed", sb.fs_pendingblocks).AppendLine();
                sbInformation.AppendFormat("{0} inodes pending of being freed", sb.fs_pendinginodes).AppendLine();
            }

            if(fs_type_sun)
                sbInformation.AppendFormat("Volume state on {0}", DateHandlers.UnixToDateTime(sb.fs_old_npsect)).
                              AppendLine();
            else if(fs_type_sun86)
                sbInformation.AppendFormat("{0} sectors/track", sb.fs_state).AppendLine();
            else if(fs_type_44bsd)
            {
                sbInformation.AppendFormat("{0} blocks on cluster summary array", sb.fs_contigsumsize).AppendLine();

                sbInformation.AppendFormat("Maximum length of a symbolic link: {0}", sb.fs_maxsymlinklen).AppendLine();

                sbInformation.AppendFormat("A file can be {0} bytes at max", sb.fs_maxfilesize).AppendLine();

                sbInformation.AppendFormat("Volume state on {0}", DateHandlers.UnixToDateTime(sb.fs_state)).
                              AppendLine();
            }

            if(sb.fs_old_nrpos > 0)
                sbInformation.AppendFormat("{0} rotational positions", sb.fs_old_nrpos).AppendLine();

            if(sb.fs_old_rotbloff > 0)
                sbInformation.AppendFormat("{0} blocks per rotation", sb.fs_old_rotbloff).AppendLine();

            information = sbInformation.ToString();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct csum
        {
            /// <summary>number of directories</summary>
            public int cs_ndir;
            /// <summary>number of free blocks</summary>
            public int cs_nbfree;
            /// <summary>number of free inodes</summary>
            public int cs_nifree;
            /// <summary>number of free frags</summary>
            public int cs_nffree;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct csum_total
        {
            /// <summary>number of directories</summary>
            public long cs_ndir;
            /// <summary>number of free blocks</summary>
            public long cs_nbfree;
            /// <summary>number of free inodes</summary>
            public long cs_nifree;
            /// <summary>number of free frags</summary>
            public long cs_nffree;
            /// <summary>number of free clusters</summary>
            public long cs_numclusters;
            /// <summary>future expansion</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly long[] cs_spare;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SuperBlock
        {
            /// <summary>linked list of file systems</summary>
            public readonly uint fs_link;
            /// <summary>used for incore super blocks on Sun: uint fs_rolled; // logging only: fs fully rolled</summary>
            public readonly uint fs_rlink;
            /// <summary>addr of super-block in filesys</summary>
            public readonly int fs_sblkno;
            /// <summary>offset of cyl-block in filesys</summary>
            public readonly int fs_cblkno;
            /// <summary>offset of inode-blocks in filesys</summary>
            public readonly int fs_iblkno;
            /// <summary>offset of first data after cg</summary>
            public readonly int fs_dblkno;
            /// <summary>cylinder group offset in cylinder</summary>
            public readonly int fs_old_cgoffset;
            /// <summary>used to calc mod fs_ntrak</summary>
            public readonly int fs_old_cgmask;
            /// <summary>last time written</summary>
            public readonly int fs_old_time;
            /// <summary>number of blocks in fs</summary>
            public readonly int fs_old_size;
            /// <summary>number of data blocks in fs</summary>
            public readonly int fs_old_dsize;
            /// <summary>number of cylinder groups</summary>
            public readonly int fs_ncg;
            /// <summary>size of basic blocks in fs</summary>
            public readonly int fs_bsize;
            /// <summary>size of frag blocks in fs</summary>
            public readonly int fs_fsize;
            /// <summary>number of frags in a block in fs</summary>
            public readonly int fs_frag;
            /* these are configuration parameters */
            /// <summary>minimum percentage of free blocks</summary>
            public readonly int fs_minfree;
            /// <summary>num of ms for optimal next block</summary>
            public readonly int fs_old_rotdelay;
            /// <summary>disk revolutions per second</summary>
            public readonly int fs_old_rps;
            /* these fields can be computed from the others */
            /// <summary>``blkoff'' calc of blk offsets</summary>
            public readonly int fs_bmask;
            /// <summary>``fragoff'' calc of frag offsets</summary>
            public readonly int fs_fmask;
            /// <summary>``lblkno'' calc of logical blkno</summary>
            public readonly int fs_bshift;
            /// <summary>``numfrags'' calc number of frags</summary>
            public readonly int fs_fshift;
            /* these are configuration parameters */
            /// <summary>max number of contiguous blks</summary>
            public readonly int fs_maxcontig;
            /// <summary>max number of blks per cyl group</summary>
            public readonly int fs_maxbpg;
            /* these fields can be computed from the others */
            /// <summary>block to frag shift</summary>
            public readonly int fs_fragshift;
            /// <summary>fsbtodb and dbtofsb shift constant</summary>
            public readonly int fs_fsbtodb;
            /// <summary>actual size of super block</summary>
            public readonly int fs_sbsize;
            /// <summary>csum block offset</summary>
            public readonly int fs_csmask;
            /// <summary>csum block number</summary>
            public readonly int fs_csshift;
            /// <summary>value of NINDIR</summary>
            public readonly int fs_nindir;
            /// <summary>value of INOPB</summary>
            public readonly uint fs_inopb;
            /// <summary>value of NSPF</summary>
            public readonly int fs_old_nspf;
            /* yet another configuration parameter */
            /// <summary>optimization preference, see below On SVR: int fs_state; // file system state</summary>
            public readonly int fs_optim;
            /// <summary># sectors/track including spares</summary>
            public readonly int fs_old_npsect;
            /// <summary>hardware sector interleave</summary>
            public readonly int fs_old_interleave;
            /// <summary>sector 0 skew, per track On A/UX: int fs_state; // file system state</summary>
            public readonly int fs_old_trackskew;
            /// <summary>unique filesystem id On old: int fs_headswitch; // head switch time, usec</summary>
            public readonly int fs_id_1;
            /// <summary>unique filesystem id On old: int fs_trkseek; // track-to-track seek, usec</summary>
            public readonly int fs_id_2;
            /* sizes determined by number of cylinder groups and their sizes */
            /// <summary>blk addr of cyl grp summary area</summary>
            public readonly int fs_old_csaddr;
            /// <summary>size of cyl grp summary area</summary>
            public readonly int fs_cssize;
            /// <summary>cylinder group size</summary>
            public readonly int fs_cgsize;
            /* these fields are derived from the hardware */
            /// <summary>tracks per cylinder</summary>
            public readonly int fs_old_ntrak;
            /// <summary>sectors per track</summary>
            public readonly int fs_old_nsect;
            /// <summary>sectors per cylinder</summary>
            public readonly int fs_old_spc;
            /* this comes from the disk driver partitioning */
            /// <summary>cylinders in filesystem</summary>
            public readonly int fs_old_ncyl;
            /* these fields can be computed from the others */
            /// <summary>cylinders per group</summary>
            public readonly int fs_old_cpg;
            /// <summary>inodes per group</summary>
            public readonly int fs_ipg;
            /// <summary>blocks per group * fs_frag</summary>
            public readonly int fs_fpg;
            /* this data must be re-computed after crashes */
            /// <summary>cylinder summary information</summary>
            public csum fs_old_cstotal;
            /* these fields are cleared at mount time */
            /// <summary>super block modified flag</summary>
            public readonly sbyte fs_fmod;
            /// <summary>filesystem is clean flag</summary>
            public readonly sbyte fs_clean;
            /// <summary>mounted read-only flag</summary>
            public readonly sbyte fs_ronly;
            /// <summary>old FS_ flags</summary>
            public readonly sbyte fs_old_flags;
            /// <summary>name mounted on</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 468)]
            public readonly byte[] fs_fsmnt;
            /// <summary>volume name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] fs_volname;
            /// <summary>system-wide uid</summary>
            public readonly ulong fs_swuid;
            /// <summary>due to alignment of fs_swuid</summary>
            public readonly int fs_pad;
            /* these fields retain the current block allocation info */
            /// <summary>last cg searched</summary>
            public readonly int fs_cgrotor;
            /// <summary>padding; was list of fs_cs buffers</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
            public readonly uint[] fs_ocsp;
            /// <summary>(u) # of contig. allocated dirs</summary>
            public readonly uint fs_contigdirs;
            /// <summary>(u) cg summary info buffer</summary>
            public readonly uint fs_csp;
            /// <summary>(u) max cluster in each cyl group</summary>
            public readonly uint fs_maxcluster;
            /// <summary>(u) used by snapshots to track fs</summary>
            public readonly uint fs_active;
            /// <summary>cyl per cycle in postbl</summary>
            public readonly int fs_old_cpc;
            /// <summary>maximum blocking factor permitted</summary>
            public readonly int fs_maxbsize;
            /// <summary>number of unreferenced inodes</summary>
            public readonly long fs_unrefs;
            /// <summary>size of underlying GEOM provider</summary>
            public readonly long fs_providersize;
            /// <summary>size of area reserved for metadata</summary>
            public readonly long fs_metaspace;
            /// <summary>old rotation block list head</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public readonly long[] fs_sparecon64;
            /// <summary>byte offset of standard superblock</summary>
            public readonly long fs_sblockloc;
            /// <summary>(u) cylinder summary information</summary>
            public csum_total fs_cstotal;
            /// <summary>last time written</summary>
            public readonly long fs_time;
            /// <summary>number of blocks in fs</summary>
            public readonly long fs_size;
            /// <summary>number of data blocks in fs</summary>
            public readonly long fs_dsize;
            /// <summary>blk addr of cyl grp summary area</summary>
            public readonly long fs_csaddr;
            /// <summary>(u) blocks being freed</summary>
            public readonly long fs_pendingblocks;
            /// <summary>(u) inodes being freed</summary>
            public readonly uint fs_pendinginodes;
            /// <summary>list of snapshot inode numbers</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public readonly uint[] fs_snapinum;
            /// <summary>expected average file size</summary>
            public readonly uint fs_avgfilesize;
            /// <summary>expected # of files per directory</summary>
            public readonly uint fs_avgfpdir;
            /// <summary>save real cg size to use fs_bsize</summary>
            public readonly int fs_save_cgsize;
            /// <summary>Last mount or fsck time.</summary>
            public readonly long fs_mtime;
            /// <summary>SUJ free list</summary>
            public readonly int fs_sujfree;
            /// <summary>reserved for future constants</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
            public readonly int[] fs_sparecon32;
            /// <summary>see FS_ flags below</summary>
            public readonly int fs_flags;
            /// <summary>size of cluster summary array</summary>
            public readonly int fs_contigsumsize;
            /// <summary>max length of an internal symlink</summary>
            public readonly int fs_maxsymlinklen;
            /// <summary>format of on-disk inodes</summary>
            public readonly int fs_old_inodefmt;
            /// <summary>maximum representable file size</summary>
            public readonly ulong fs_maxfilesize;
            /// <summary>~fs_bmask for use with 64-bit size</summary>
            public readonly long fs_qbmask;
            /// <summary>~fs_fmask for use with 64-bit size</summary>
            public readonly long fs_qfmask;
            /// <summary>validate fs_clean field</summary>
            public readonly int fs_state;
            /// <summary>format of positional layout tables</summary>
            public readonly int fs_old_postblformat;
            /// <summary>number of rotational positions</summary>
            public readonly int fs_old_nrpos;
            /// <summary>(short) rotation block list head</summary>
            public readonly int fs_old_postbloff;
            /// <summary>(uchar_t) blocks for each rotation</summary>
            public readonly int fs_old_rotbloff;
            /// <summary>magic number</summary>
            public readonly uint fs_magic;
            /// <summary>list of blocks for each rotation</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public readonly byte[] fs_rotbl;
        }
    }
}