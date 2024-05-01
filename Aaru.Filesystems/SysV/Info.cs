// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNIX System V filesystem plugin.
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

// ReSharper disable NotAccessedField.Local

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the UNIX System V filesystem</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class SysVfs
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End) return false;

        byte sb_size_in_sectors;

        if(imagePlugin.Info.SectorSize <=
           0x400) // Check if underlying device sector size is smaller than SuperBlock size
            sb_size_in_sectors = (byte)(0x400 / imagePlugin.Info.SectorSize);
        else
            sb_size_in_sectors = 1; // If not a single sector can store it

        if(partition.End <=
           partition.Start               +
           4 * (ulong)sb_size_in_sectors +
           sb_size_in_sectors) // Device must be bigger than SB location + SB size + offset
            return false;

        // Sectors in a cylinder
        var spc = (int)(imagePlugin.Info.Heads * imagePlugin.Info.SectorsPerTrack);

        // Superblock can start on 0x000, 0x200, 0x600 and 0x800, not aligned, so we assume 16 (128 bytes/sector) sectors as a safe value
        int[] locations =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,

            // Superblock can also skip one cylinder (for boot)
            spc
        };

        foreach(int i in locations.TakeWhile(i => (ulong)i + partition.Start + sb_size_in_sectors <
                                                  imagePlugin.Info.Sectors))
        {
            ErrorNumber errno =
                imagePlugin.ReadSectors((ulong)i + partition.Start, sb_size_in_sectors, out byte[] sb_sector);

            if(errno != ErrorNumber.NoError || sb_sector.Length < 0x400) continue;

            var magic = BitConverter.ToUInt32(sb_sector, 0x3F8);

            if(magic is XENIX_MAGIC or XENIX_CIGAM or SYSV_MAGIC or SYSV_CIGAM) return true;

            magic = BitConverter.ToUInt32(sb_sector, 0x1F8); // System V magic location

            if(magic is SYSV_MAGIC or SYSV_CIGAM) return true;

            magic = BitConverter.ToUInt32(sb_sector, 0x1F0); // XENIX 3 magic location

            if(magic is XENIX_MAGIC or XENIX_CIGAM) return true;

            var coherent_string = new byte[6];
            Array.Copy(sb_sector, 0x1E4, coherent_string, 0, 6); // Coherent UNIX s_fname location
            string s_fname = StringHandlers.CToString(coherent_string);
            Array.Copy(sb_sector, 0x1EA, coherent_string, 0, 6); // Coherent UNIX s_fpack location
            string s_fpack = StringHandlers.CToString(coherent_string);

            if(s_fname == COH_FNAME && s_fpack == COH_FPACK ||
               s_fname == COH_XXXXX && s_fpack == COH_XXXXX ||
               s_fname == COH_XXXXS && s_fpack == COH_XXXXN)
                return true;

            // Now try to identify 7th edition
            var s_fsize  = BitConverter.ToUInt32(sb_sector, 0x002);
            var s_nfree  = BitConverter.ToUInt16(sb_sector, 0x006);
            var s_ninode = BitConverter.ToUInt16(sb_sector, 0x0D0);

            if(s_fsize is <= 0 or >= 0xFFFFFFFF || s_nfree is <= 0 or >= 0xFFFF || s_ninode is <= 0 or >= 0xFFFF)
                continue;

            if((s_fsize & 0xFF) == 0x00 && (s_nfree & 0xFF) == 0x00 && (s_ninode & 0xFF) == 0x00)
            {
                // Byteswap
                s_fsize = ((s_fsize & 0xFF)       << 24) +
                          ((s_fsize & 0xFF00)     << 8)  +
                          ((s_fsize & 0xFF0000)   >> 8)  +
                          ((s_fsize & 0xFF000000) >> 24);

                s_nfree  = (ushort)(s_nfree  >> 8);
                s_ninode = (ushort)(s_ninode >> 8);
            }

            if((s_fsize & 0xFF000000) != 0x00 || (s_nfree & 0xFF00) != 0x00 || (s_ninode & 0xFF00) != 0x00) continue;

            if(s_fsize >= V7_MAXSIZE || s_nfree >= V7_NICFREE || s_ninode >= V7_NICINOD) continue;

            if(s_fsize * 1024 == (partition.End - partition.Start) * imagePlugin.Info.SectorSize ||
               s_fsize * 512  == (partition.End - partition.Start) * imagePlugin.Info.SectorSize)
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.GetEncoding("iso-8859-15");
        information =   "";
        metadata    =   new FileSystem();

        var    sb        = new StringBuilder();
        var    bigEndian = false; // Start in little endian until we know what are we handling here
        var    start     = 0;
        var    xenix     = false;
        var    sysv      = false;
        var    sys7th    = false;
        var    coherent  = false;
        var    xenix3    = false;
        byte[] sb_sector;
        byte   sb_size_in_sectors;
        var    offset = 0;

        if(imagePlugin.Info.SectorSize <=
           0x400) // Check if underlying device sector size is smaller than SuperBlock size
            sb_size_in_sectors = (byte)(0x400 / imagePlugin.Info.SectorSize);
        else
            sb_size_in_sectors = 1; // If not a single sector can store it

        // Sectors in a cylinder
        var spc = (int)(imagePlugin.Info.Heads * imagePlugin.Info.SectorsPerTrack);

        // Superblock can start on 0x000, 0x200, 0x600 and 0x800, not aligned, so we assume 16 (128 bytes/sector) sectors as a safe value
        int[] locations =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,

            // Superblock can also skip one cylinder (for boot)
            spc
        };

        ErrorNumber errno;

        foreach(int i in locations)
        {
            errno = imagePlugin.ReadSectors((ulong)i + partition.Start, sb_size_in_sectors, out sb_sector);

            if(errno != ErrorNumber.NoError) continue;

            var magic = BitConverter.ToUInt32(sb_sector, 0x3F8);

            if(magic is XENIX_MAGIC or SYSV_MAGIC)
            {
                if(magic == SYSV_MAGIC)
                {
                    sysv   = true;
                    offset = 0x200;
                }
                else
                    xenix = true;

                start = i;

                break;
            }

            if(magic is XENIX_CIGAM or SYSV_CIGAM)
            {
                bigEndian = true; // Big endian

                if(magic == SYSV_CIGAM)
                {
                    sysv   = true;
                    offset = 0x200;
                }
                else
                    xenix = true;

                start = i;

                break;
            }

            magic = BitConverter.ToUInt32(sb_sector, 0x1F0); // XENIX 3 magic location

            if(magic == XENIX_MAGIC)
            {
                xenix3 = true;
                start  = i;

                break;
            }

            if(magic == XENIX_CIGAM)
            {
                bigEndian = true; // Big endian
                xenix3    = true;
                start     = i;

                break;
            }

            magic = BitConverter.ToUInt32(sb_sector, 0x1F8); // XENIX magic location

            if(magic == SYSV_MAGIC)
            {
                sysv  = true;
                start = i;

                break;
            }

            if(magic == SYSV_CIGAM)
            {
                bigEndian = true; // Big endian
                sysv      = true;
                start     = i;

                break;
            }

            var coherent_string = new byte[6];
            Array.Copy(sb_sector, 0x1E4, coherent_string, 0, 6); // Coherent UNIX s_fname location
            string s_fname = StringHandlers.CToString(coherent_string, encoding);
            Array.Copy(sb_sector, 0x1EA, coherent_string, 0, 6); // Coherent UNIX s_fpack location
            string s_fpack = StringHandlers.CToString(coherent_string, encoding);

            if(s_fname == COH_FNAME && s_fpack == COH_FPACK ||
               s_fname == COH_XXXXX && s_fpack == COH_XXXXX ||
               s_fname == COH_XXXXS && s_fpack == COH_XXXXN)
            {
                coherent = true;
                start    = i;

                break;
            }

            // Now try to identify 7th edition
            var s_fsize  = BitConverter.ToUInt32(sb_sector, 0x002);
            var s_nfree  = BitConverter.ToUInt16(sb_sector, 0x006);
            var s_ninode = BitConverter.ToUInt16(sb_sector, 0x0D0);

            if(s_fsize is <= 0 or >= 0xFFFFFFFF || s_nfree is <= 0 or >= 0xFFFF || s_ninode is <= 0 or >= 0xFFFF)
                continue;

            if((s_fsize & 0xFF) == 0x00 && (s_nfree & 0xFF) == 0x00 && (s_ninode & 0xFF) == 0x00)
            {
                // Byteswap
                s_fsize = ((s_fsize & 0xFF)       << 24) +
                          ((s_fsize & 0xFF00)     << 8)  +
                          ((s_fsize & 0xFF0000)   >> 8)  +
                          ((s_fsize & 0xFF000000) >> 24);

                s_nfree  = (ushort)(s_nfree  >> 8);
                s_ninode = (ushort)(s_ninode >> 8);
            }

            if((s_fsize & 0xFF000000) != 0x00 || (s_nfree & 0xFF00) != 0x00 || (s_ninode & 0xFF00) != 0x00) continue;

            if(s_fsize >= V7_MAXSIZE || s_nfree >= V7_NICFREE || s_ninode >= V7_NICINOD) continue;

            if(s_fsize * 1024 != (partition.End - partition.Start) * imagePlugin.Info.SectorSize &&
               s_fsize * 512  != (partition.End - partition.Start) * imagePlugin.Info.SectorSize)
                continue;

            sys7th = true;
            start  = i;

            break;
        }

        if(!sys7th && !sysv && !coherent && !xenix && !xenix3) return;

        metadata = new FileSystem();

        if(xenix || xenix3)
        {
            var xenix_strings = new byte[6];
            var xnx_sb        = new XenixSuperBlock();
            errno = imagePlugin.ReadSectors((ulong)start + partition.Start, sb_size_in_sectors, out sb_sector);

            if(errno != ErrorNumber.NoError) return;

            if(xenix3)
            {
                xnx_sb.s_isize   = BitConverter.ToUInt16(sb_sector, 0x000);
                xnx_sb.s_fsize   = BitConverter.ToUInt32(sb_sector, 0x002);
                xnx_sb.s_nfree   = BitConverter.ToUInt16(sb_sector, 0x006);
                xnx_sb.s_ninode  = BitConverter.ToUInt16(sb_sector, 0x0D0);
                xnx_sb.s_flock   = sb_sector[0x19A];
                xnx_sb.s_ilock   = sb_sector[0x19B];
                xnx_sb.s_fmod    = sb_sector[0x19C];
                xnx_sb.s_ronly   = sb_sector[0x19D];
                xnx_sb.s_time    = BitConverter.ToInt32(sb_sector, 0x19E);
                xnx_sb.s_tfree   = BitConverter.ToUInt32(sb_sector, 0x1A2);
                xnx_sb.s_tinode  = BitConverter.ToUInt16(sb_sector, 0x1A6);
                xnx_sb.s_cylblks = BitConverter.ToUInt16(sb_sector, 0x1A8);
                xnx_sb.s_gapblks = BitConverter.ToUInt16(sb_sector, 0x1AA);
                xnx_sb.s_dinfo0  = BitConverter.ToUInt16(sb_sector, 0x1AC);
                xnx_sb.s_dinfo1  = BitConverter.ToUInt16(sb_sector, 0x1AE);
                Array.Copy(sb_sector, 0x1B0, xenix_strings, 0, 6);
                xnx_sb.s_fname = StringHandlers.CToString(xenix_strings, encoding);
                Array.Copy(sb_sector, 0x1B6, xenix_strings, 0, 6);
                xnx_sb.s_fpack = StringHandlers.CToString(xenix_strings, encoding);
                xnx_sb.s_clean = sb_sector[0x1BC];
                xnx_sb.s_magic = BitConverter.ToUInt32(sb_sector, 0x1F0);
                xnx_sb.s_type  = BitConverter.ToUInt32(sb_sector, 0x1F4);
            }
            else
            {
                xnx_sb.s_isize   = BitConverter.ToUInt16(sb_sector, 0x000);
                xnx_sb.s_fsize   = BitConverter.ToUInt32(sb_sector, 0x002);
                xnx_sb.s_nfree   = BitConverter.ToUInt16(sb_sector, 0x006);
                xnx_sb.s_ninode  = BitConverter.ToUInt16(sb_sector, 0x198);
                xnx_sb.s_flock   = sb_sector[0x262];
                xnx_sb.s_ilock   = sb_sector[0x263];
                xnx_sb.s_fmod    = sb_sector[0x264];
                xnx_sb.s_ronly   = sb_sector[0x265];
                xnx_sb.s_time    = BitConverter.ToInt32(sb_sector, 0x266);
                xnx_sb.s_tfree   = BitConverter.ToUInt32(sb_sector, 0x26A);
                xnx_sb.s_tinode  = BitConverter.ToUInt16(sb_sector, 0x26E);
                xnx_sb.s_cylblks = BitConverter.ToUInt16(sb_sector, 0x270);
                xnx_sb.s_gapblks = BitConverter.ToUInt16(sb_sector, 0x272);
                xnx_sb.s_dinfo0  = BitConverter.ToUInt16(sb_sector, 0x274);
                xnx_sb.s_dinfo1  = BitConverter.ToUInt16(sb_sector, 0x276);
                Array.Copy(sb_sector, 0x278, xenix_strings, 0, 6);
                xnx_sb.s_fname = StringHandlers.CToString(xenix_strings, encoding);
                Array.Copy(sb_sector, 0x27E, xenix_strings, 0, 6);
                xnx_sb.s_fpack = StringHandlers.CToString(xenix_strings, encoding);
                xnx_sb.s_clean = sb_sector[0x284];
                xnx_sb.s_magic = BitConverter.ToUInt32(sb_sector, 0x3F8);
                xnx_sb.s_type  = BitConverter.ToUInt32(sb_sector, 0x3FC);
            }

            if(bigEndian)
            {
                xnx_sb.s_isize   = Swapping.Swap(xnx_sb.s_isize);
                xnx_sb.s_fsize   = Swapping.Swap(xnx_sb.s_fsize);
                xnx_sb.s_nfree   = Swapping.Swap(xnx_sb.s_nfree);
                xnx_sb.s_ninode  = Swapping.Swap(xnx_sb.s_ninode);
                xnx_sb.s_time    = Swapping.Swap(xnx_sb.s_time);
                xnx_sb.s_tfree   = Swapping.Swap(xnx_sb.s_tfree);
                xnx_sb.s_tinode  = Swapping.Swap(xnx_sb.s_tinode);
                xnx_sb.s_cylblks = Swapping.Swap(xnx_sb.s_cylblks);
                xnx_sb.s_gapblks = Swapping.Swap(xnx_sb.s_gapblks);
                xnx_sb.s_dinfo0  = Swapping.Swap(xnx_sb.s_dinfo0);
                xnx_sb.s_dinfo1  = Swapping.Swap(xnx_sb.s_dinfo1);
                xnx_sb.s_magic   = Swapping.Swap(xnx_sb.s_magic);
                xnx_sb.s_type    = Swapping.Swap(xnx_sb.s_type);
            }

            uint bs = 512;
            sb.AppendLine(Localization.XENIX_filesystem);
            metadata.Type = FS_TYPE_XENIX;

            switch(xnx_sb.s_type)
            {
                case 1:
                    sb.AppendLine(Localization._512_bytes_per_block);
                    metadata.ClusterSize = 512;

                    break;
                case 2:
                    sb.AppendLine(Localization._1024_bytes_per_block);
                    bs                   = 1024;
                    metadata.ClusterSize = 1024;

                    break;
                case 3:
                    sb.AppendLine(Localization._2048_bytes_per_block);
                    bs                   = 2048;
                    metadata.ClusterSize = 2048;

                    break;
                default:
                    sb.AppendFormat(Localization.Unknown_s_type_value_0, xnx_sb.s_type).AppendLine();

                    break;
            }

            if(imagePlugin.Info.SectorSize is 2336 or 2352 or 2448)
            {
                if(bs != 2048)
                {
                    sb.AppendFormat(Localization
                                       .WARNING_Filesystem_indicates_0_bytes_block_while_device_indicates_1_bytes_block,
                                    bs,
                                    2048)
                      .AppendLine();
                }
            }
            else
            {
                if(bs != imagePlugin.Info.SectorSize)
                {
                    sb.AppendFormat(Localization
                                       .WARNING_Filesystem_indicates_0_bytes_block_while_device_indicates_1_bytes_block,
                                    bs,
                                    imagePlugin.Info.SectorSize)
                      .AppendLine();
                }
            }

            sb.AppendFormat(Localization._0_zones_in_volume_1_bytes, xnx_sb.s_fsize, xnx_sb.s_fsize * bs).AppendLine();

            sb.AppendFormat(Localization._0_free_zones_on_volume_1_bytes, xnx_sb.s_tfree, xnx_sb.s_tfree * bs)
              .AppendLine();

            sb.AppendFormat(Localization._0_free_blocks_on_list_1_bytes, xnx_sb.s_nfree, xnx_sb.s_nfree * bs)
              .AppendLine();

            sb.AppendFormat(Localization._0_blocks_per_cylinder_1_bytes, xnx_sb.s_cylblks, xnx_sb.s_cylblks * bs)
              .AppendLine();

            sb.AppendFormat(Localization._0_blocks_per_gap_1_bytes, xnx_sb.s_gapblks, xnx_sb.s_gapblks * bs)
              .AppendLine();

            sb.AppendFormat(Localization.First_data_zone_0,        xnx_sb.s_isize).AppendLine();
            sb.AppendFormat(Localization._0_free_inodes_on_volume, xnx_sb.s_tinode).AppendLine();
            sb.AppendFormat(Localization._0_free_inodes_on_list,   xnx_sb.s_ninode).AppendLine();

            if(xnx_sb.s_flock > 0) sb.AppendLine(Localization.Free_block_list_is_locked);

            if(xnx_sb.s_ilock > 0) sb.AppendLine(Localization.inode_cache_is_locked);

            if(xnx_sb.s_fmod > 0) sb.AppendLine(Localization.Superblock_is_being_modified);

            if(xnx_sb.s_ronly > 0) sb.AppendLine(Localization.Volume_is_mounted_read_only);

            sb.AppendFormat(Localization.Superblock_last_updated_on_0, DateHandlers.UnixToDateTime(xnx_sb.s_time))
              .AppendLine();

            if(xnx_sb.s_time != 0) metadata.ModificationDate = DateHandlers.UnixToDateTime(xnx_sb.s_time);

            sb.AppendFormat(Localization.Volume_name_0, xnx_sb.s_fname).AppendLine();
            metadata.VolumeName = xnx_sb.s_fname;
            sb.AppendFormat(Localization.Pack_name_0, xnx_sb.s_fpack).AppendLine();

            if(xnx_sb.s_clean == 0x46)
                sb.AppendLine(Localization.Volume_is_clean);
            else
            {
                sb.AppendLine(Localization.Volume_is_dirty);
                metadata.Dirty = true;
            }
        }

        if(sysv)
        {
            errno = imagePlugin.ReadSectors((ulong)start + partition.Start, sb_size_in_sectors, out sb_sector);

            if(errno != ErrorNumber.NoError) return;

            var sysv_strings = new byte[6];

            var sysv_sb = new SystemVRelease4SuperBlock
            {
                s_type = BitConverter.ToUInt32(sb_sector, 0x1FC + offset)
            };

            if(bigEndian) sysv_sb.s_type = Swapping.Swap(sysv_sb.s_type);

            uint bs = 512;

            switch(sysv_sb.s_type)
            {
                case 1:
                    metadata.ClusterSize = 512;

                    break;
                case 2:
                    bs                   = 1024;
                    metadata.ClusterSize = 1024;

                    break;
                case 3:
                    bs                   = 2048;
                    metadata.ClusterSize = 2048;

                    break;
                default:
                    sb.AppendFormat(Localization.Unknown_s_type_value_0, sysv_sb.s_type).AppendLine();

                    break;
            }

            sysv_sb.s_fsize = BitConverter.ToUInt32(sb_sector, 0x002 + offset);

            if(bigEndian) sysv_sb.s_fsize = Swapping.Swap(sysv_sb.s_fsize);

            bool sysvr4 = sysv_sb.s_fsize * bs <= 0 || sysv_sb.s_fsize * bs != partition.Size;

            if(sysvr4)
            {
                sysv_sb.s_isize   = BitConverter.ToUInt16(sb_sector, 0x000 + offset);
                sysv_sb.s_state   = BitConverter.ToUInt32(sb_sector, 0x1F4 + offset);
                sysv_sb.s_magic   = BitConverter.ToUInt32(sb_sector, 0x1F8 + offset);
                sysv_sb.s_fsize   = BitConverter.ToUInt32(sb_sector, 0x004 + offset);
                sysv_sb.s_nfree   = BitConverter.ToUInt16(sb_sector, 0x008 + offset);
                sysv_sb.s_ninode  = BitConverter.ToUInt16(sb_sector, 0x0D4 + offset);
                sysv_sb.s_flock   = sb_sector[0x1A0                        + offset];
                sysv_sb.s_ilock   = sb_sector[0x1A1                        + offset];
                sysv_sb.s_fmod    = sb_sector[0x1A2                        + offset];
                sysv_sb.s_ronly   = sb_sector[0x1A3                        + offset];
                sysv_sb.s_time    = BitConverter.ToUInt32(sb_sector, 0x1A4 + offset);
                sysv_sb.s_cylblks = BitConverter.ToUInt16(sb_sector, 0x1A8 + offset);
                sysv_sb.s_gapblks = BitConverter.ToUInt16(sb_sector, 0x1AA + offset);
                sysv_sb.s_dinfo0  = BitConverter.ToUInt16(sb_sector, 0x1AC + offset);
                sysv_sb.s_dinfo1  = BitConverter.ToUInt16(sb_sector, 0x1AE + offset);
                sysv_sb.s_tfree   = BitConverter.ToUInt32(sb_sector, 0x1B0 + offset);
                sysv_sb.s_tinode  = BitConverter.ToUInt16(sb_sector, 0x1B4 + offset);
                Array.Copy(sb_sector, 0x1B6 + offset, sysv_strings, 0, 6);
                sysv_sb.s_fname = StringHandlers.CToString(sysv_strings, encoding);
                Array.Copy(sb_sector, 0x1BC + offset, sysv_strings, 0, 6);
                sysv_sb.s_fpack = StringHandlers.CToString(sysv_strings, encoding);
                sb.AppendLine(Localization.System_V_Release_4_filesystem);
                metadata.Type = FS_TYPE_SVR4;
            }
            else
            {
                sysv_sb.s_isize   = BitConverter.ToUInt16(sb_sector, 0x000 + offset);
                sysv_sb.s_state   = BitConverter.ToUInt32(sb_sector, 0x1F4 + offset);
                sysv_sb.s_magic   = BitConverter.ToUInt32(sb_sector, 0x1F8 + offset);
                sysv_sb.s_fsize   = BitConverter.ToUInt32(sb_sector, 0x002 + offset);
                sysv_sb.s_nfree   = BitConverter.ToUInt16(sb_sector, 0x006 + offset);
                sysv_sb.s_ninode  = BitConverter.ToUInt16(sb_sector, 0x0D0 + offset);
                sysv_sb.s_flock   = sb_sector[0x19A                        + offset];
                sysv_sb.s_ilock   = sb_sector[0x19B                        + offset];
                sysv_sb.s_fmod    = sb_sector[0x19C                        + offset];
                sysv_sb.s_ronly   = sb_sector[0x19D                        + offset];
                sysv_sb.s_time    = BitConverter.ToUInt32(sb_sector, 0x19E + offset);
                sysv_sb.s_cylblks = BitConverter.ToUInt16(sb_sector, 0x1A2 + offset);
                sysv_sb.s_gapblks = BitConverter.ToUInt16(sb_sector, 0x1A4 + offset);
                sysv_sb.s_dinfo0  = BitConverter.ToUInt16(sb_sector, 0x1A6 + offset);
                sysv_sb.s_dinfo1  = BitConverter.ToUInt16(sb_sector, 0x1A8 + offset);
                sysv_sb.s_tfree   = BitConverter.ToUInt32(sb_sector, 0x1AA + offset);
                sysv_sb.s_tinode  = BitConverter.ToUInt16(sb_sector, 0x1AE + offset);
                Array.Copy(sb_sector, 0x1B0 + offset, sysv_strings, 0, 6);
                sysv_sb.s_fname = StringHandlers.CToString(sysv_strings, encoding);
                Array.Copy(sb_sector, 0x1B6 + offset, sysv_strings, 0, 6);
                sysv_sb.s_fpack = StringHandlers.CToString(sysv_strings, encoding);
                sb.AppendLine(Localization.System_V_Release_2_filesystem);
                metadata.Type = FS_TYPE_SVR2;
            }

            if(bigEndian)
            {
                sysv_sb.s_isize   = Swapping.Swap(sysv_sb.s_isize);
                sysv_sb.s_state   = Swapping.Swap(sysv_sb.s_state);
                sysv_sb.s_magic   = Swapping.Swap(sysv_sb.s_magic);
                sysv_sb.s_fsize   = Swapping.Swap(sysv_sb.s_fsize);
                sysv_sb.s_nfree   = Swapping.Swap(sysv_sb.s_nfree);
                sysv_sb.s_ninode  = Swapping.Swap(sysv_sb.s_ninode);
                sysv_sb.s_time    = Swapping.Swap(sysv_sb.s_time);
                sysv_sb.s_cylblks = Swapping.Swap(sysv_sb.s_cylblks);
                sysv_sb.s_gapblks = Swapping.Swap(sysv_sb.s_gapblks);
                sysv_sb.s_dinfo0  = Swapping.Swap(sysv_sb.s_dinfo0);
                sysv_sb.s_dinfo1  = Swapping.Swap(sysv_sb.s_dinfo1);
                sysv_sb.s_tfree   = Swapping.Swap(sysv_sb.s_tfree);
                sysv_sb.s_tinode  = Swapping.Swap(sysv_sb.s_tinode);
            }

            sb.AppendFormat(Localization._0_bytes_per_block, bs).AppendLine();

            metadata.Clusters = sysv_sb.s_fsize;

            sb.AppendFormat(Localization._0_zones_in_volume_1_bytes, sysv_sb.s_fsize, sysv_sb.s_fsize * bs)
              .AppendLine();

            sb.AppendFormat(Localization._0_free_zones_on_volume_1_bytes, sysv_sb.s_tfree, sysv_sb.s_tfree * bs)
              .AppendLine();

            sb.AppendFormat(Localization._0_free_blocks_on_list_1_bytes, sysv_sb.s_nfree, sysv_sb.s_nfree * bs)
              .AppendLine();

            sb.AppendFormat(Localization._0_blocks_per_cylinder_1_bytes, sysv_sb.s_cylblks, sysv_sb.s_cylblks * bs)
              .AppendLine();

            sb.AppendFormat(Localization._0_blocks_per_gap_1_bytes, sysv_sb.s_gapblks, sysv_sb.s_gapblks * bs)
              .AppendLine();

            sb.AppendFormat(Localization.First_data_zone_0,        sysv_sb.s_isize).AppendLine();
            sb.AppendFormat(Localization._0_free_inodes_on_volume, sysv_sb.s_tinode).AppendLine();
            sb.AppendFormat(Localization._0_free_inodes_on_list,   sysv_sb.s_ninode).AppendLine();

            if(sysv_sb.s_flock > 0) sb.AppendLine(Localization.Free_block_list_is_locked);

            if(sysv_sb.s_ilock > 0) sb.AppendLine(Localization.inode_cache_is_locked);

            if(sysv_sb.s_fmod > 0) sb.AppendLine(Localization.Superblock_is_being_modified);

            if(sysv_sb.s_ronly > 0) sb.AppendLine(Localization.Volume_is_mounted_read_only);

            sb.AppendFormat(Localization.Superblock_last_updated_on_0,
                            DateHandlers.UnixUnsignedToDateTime(sysv_sb.s_time))
              .AppendLine();

            if(sysv_sb.s_time != 0) metadata.ModificationDate = DateHandlers.UnixUnsignedToDateTime(sysv_sb.s_time);

            sb.AppendFormat(Localization.Volume_name_0, sysv_sb.s_fname).AppendLine();
            metadata.VolumeName = sysv_sb.s_fname;
            sb.AppendFormat(Localization.Pack_name_0, sysv_sb.s_fpack).AppendLine();

            if(sysv_sb.s_state == 0x7C269D38 - sysv_sb.s_time)
                sb.AppendLine(Localization.Volume_is_clean);
            else
            {
                sb.AppendLine(Localization.Volume_is_dirty);
                metadata.Dirty = true;
            }
        }

        if(coherent)
        {
            errno = imagePlugin.ReadSectors((ulong)start + partition.Start, sb_size_in_sectors, out sb_sector);

            if(errno != ErrorNumber.NoError) return;

            var coh_sb      = new CoherentSuperBlock();
            var coh_strings = new byte[6];

            coh_sb.s_isize  = BitConverter.ToUInt16(sb_sector, 0x000);
            coh_sb.s_fsize  = Swapping.PDPFromLittleEndian(BitConverter.ToUInt32(sb_sector, 0x002));
            coh_sb.s_nfree  = BitConverter.ToUInt16(sb_sector, 0x006);
            coh_sb.s_ninode = BitConverter.ToUInt16(sb_sector, 0x108);
            coh_sb.s_flock  = sb_sector[0x1D2];
            coh_sb.s_ilock  = sb_sector[0x1D3];
            coh_sb.s_fmod   = sb_sector[0x1D4];
            coh_sb.s_ronly  = sb_sector[0x1D5];
            coh_sb.s_time   = Swapping.PDPFromLittleEndian(BitConverter.ToUInt32(sb_sector, 0x1D6));
            coh_sb.s_tfree  = Swapping.PDPFromLittleEndian(BitConverter.ToUInt32(sb_sector, 0x1DA));
            coh_sb.s_tinode = BitConverter.ToUInt16(sb_sector, 0x1DE);
            coh_sb.s_int_m  = BitConverter.ToUInt16(sb_sector, 0x1E0);
            coh_sb.s_int_n  = BitConverter.ToUInt16(sb_sector, 0x1E2);
            Array.Copy(sb_sector, 0x1E4, coh_strings, 0, 6);
            coh_sb.s_fname = StringHandlers.CToString(coh_strings, encoding);
            Array.Copy(sb_sector, 0x1EA, coh_strings, 0, 6);
            coh_sb.s_fpack = StringHandlers.CToString(coh_strings, encoding);

            metadata.Type        = FS_TYPE_COHERENT;
            metadata.ClusterSize = 512;
            metadata.Clusters    = coh_sb.s_fsize;

            sb.AppendLine(Localization.Coherent_UNIX_filesystem);

            if(imagePlugin.Info.SectorSize != 512)
            {
                sb.AppendFormat(Localization
                                   .WARNING_Filesystem_indicates_0_bytes_block_while_device_indicates_1_bytes_block,
                                512,
                                2048)
                  .AppendLine();
            }

            sb.AppendFormat(Localization._0_zones_in_volume_1_bytes, coh_sb.s_fsize, coh_sb.s_fsize * 512).AppendLine();

            sb.AppendFormat(Localization._0_free_zones_on_volume_1_bytes, coh_sb.s_tfree, coh_sb.s_tfree * 512)
              .AppendLine();

            sb.AppendFormat(Localization._0_free_blocks_on_list_1_bytes, coh_sb.s_nfree, coh_sb.s_nfree * 512)
              .AppendLine();

            sb.AppendFormat(Localization.First_data_zone_0,        coh_sb.s_isize).AppendLine();
            sb.AppendFormat(Localization._0_free_inodes_on_volume, coh_sb.s_tinode).AppendLine();
            sb.AppendFormat(Localization._0_free_inodes_on_list,   coh_sb.s_ninode).AppendLine();

            if(coh_sb.s_flock > 0) sb.AppendLine(Localization.Free_block_list_is_locked);

            if(coh_sb.s_ilock > 0) sb.AppendLine(Localization.inode_cache_is_locked);

            if(coh_sb.s_fmod > 0) sb.AppendLine(Localization.Superblock_is_being_modified);

            if(coh_sb.s_ronly > 0) sb.AppendLine(Localization.Volume_is_mounted_read_only);

            sb.AppendFormat(Localization.Superblock_last_updated_on_0,
                            DateHandlers.UnixUnsignedToDateTime(coh_sb.s_time))
              .AppendLine();

            if(coh_sb.s_time != 0) metadata.ModificationDate = DateHandlers.UnixUnsignedToDateTime(coh_sb.s_time);

            sb.AppendFormat(Localization.Volume_name_0, coh_sb.s_fname).AppendLine();
            metadata.VolumeName = coh_sb.s_fname;
            sb.AppendFormat(Localization.Pack_name_0, coh_sb.s_fpack).AppendLine();
        }

        if(sys7th)
        {
            errno = imagePlugin.ReadSectors((ulong)start + partition.Start, sb_size_in_sectors, out sb_sector);

            if(errno != ErrorNumber.NoError) return;

            var v7_sb        = new UNIX7thEditionSuperBlock();
            var sys7_strings = new byte[6];

            v7_sb.s_isize  = BitConverter.ToUInt16(sb_sector, 0x000);
            v7_sb.s_fsize  = BitConverter.ToUInt32(sb_sector, 0x002);
            v7_sb.s_nfree  = BitConverter.ToUInt16(sb_sector, 0x006);
            v7_sb.s_ninode = BitConverter.ToUInt16(sb_sector, 0x0D0);
            v7_sb.s_flock  = sb_sector[0x19A];
            v7_sb.s_ilock  = sb_sector[0x19B];
            v7_sb.s_fmod   = sb_sector[0x19C];
            v7_sb.s_ronly  = sb_sector[0x19D];
            v7_sb.s_time   = BitConverter.ToUInt32(sb_sector, 0x19E);
            v7_sb.s_tfree  = BitConverter.ToUInt32(sb_sector, 0x1A2);
            v7_sb.s_tinode = BitConverter.ToUInt16(sb_sector, 0x1A6);
            v7_sb.s_int_m  = BitConverter.ToUInt16(sb_sector, 0x1A8);
            v7_sb.s_int_n  = BitConverter.ToUInt16(sb_sector, 0x1AA);
            Array.Copy(sb_sector, 0x1AC, sys7_strings, 0, 6);
            v7_sb.s_fname = StringHandlers.CToString(sys7_strings, encoding);
            Array.Copy(sb_sector, 0x1B2, sys7_strings, 0, 6);
            v7_sb.s_fpack = StringHandlers.CToString(sys7_strings, encoding);

            metadata.Type        = FS_TYPE_UNIX7;
            metadata.ClusterSize = 512;
            metadata.Clusters    = v7_sb.s_fsize;
            sb.AppendLine(Localization.UNIX_7th_Edition_filesystem);

            if(imagePlugin.Info.SectorSize != 512)
            {
                sb.AppendFormat(Localization
                                   .WARNING_Filesystem_indicates_0_bytes_block_while_device_indicates_1_bytes_block,
                                512,
                                2048)
                  .AppendLine();
            }

            sb.AppendFormat(Localization._0_zones_in_volume_1_bytes, v7_sb.s_fsize, v7_sb.s_fsize * 512).AppendLine();

            sb.AppendFormat(Localization._0_free_zones_on_volume_1_bytes, v7_sb.s_tfree, v7_sb.s_tfree * 512)
              .AppendLine();

            sb.AppendFormat(Localization._0_free_blocks_on_list_1_bytes, v7_sb.s_nfree, v7_sb.s_nfree * 512)
              .AppendLine();

            sb.AppendFormat(Localization.First_data_zone_0,        v7_sb.s_isize).AppendLine();
            sb.AppendFormat(Localization._0_free_inodes_on_volume, v7_sb.s_tinode).AppendLine();
            sb.AppendFormat(Localization._0_free_inodes_on_list,   v7_sb.s_ninode).AppendLine();

            if(v7_sb.s_flock > 0) sb.AppendLine(Localization.Free_block_list_is_locked);

            if(v7_sb.s_ilock > 0) sb.AppendLine(Localization.inode_cache_is_locked);

            if(v7_sb.s_fmod > 0) sb.AppendLine(Localization.Superblock_is_being_modified);

            if(v7_sb.s_ronly > 0) sb.AppendLine(Localization.Volume_is_mounted_read_only);

            sb.AppendFormat(Localization.Superblock_last_updated_on_0,
                            DateHandlers.UnixUnsignedToDateTime(v7_sb.s_time))
              .AppendLine();

            if(v7_sb.s_time != 0) metadata.ModificationDate = DateHandlers.UnixUnsignedToDateTime(v7_sb.s_time);

            sb.AppendFormat(Localization.Volume_name_0, v7_sb.s_fname).AppendLine();
            metadata.VolumeName = v7_sb.s_fname;
            sb.AppendFormat(Localization.Pack_name_0, v7_sb.s_fpack).AppendLine();
        }

        information = sb.ToString();
    }

#endregion
}