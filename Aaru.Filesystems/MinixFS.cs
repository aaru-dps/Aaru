// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MinixFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MINIX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the MINIX filesystem and shows information.
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

namespace Aaru.Filesystems;

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the MINIX filesystem</summary>
public sealed class MinixFS : IFilesystem
{
    /// <summary>Minix v1, 14 char filenames</summary>
    const ushort MINIX_MAGIC = 0x137F;
    /// <summary>Minix v1, 30 char filenames</summary>
    const ushort MINIX_MAGIC2 = 0x138F;
    /// <summary>Minix v2, 14 char filenames</summary>
    const ushort MINIX2_MAGIC = 0x2468;
    /// <summary>Minix v2, 30 char filenames</summary>
    const ushort MINIX2_MAGIC2 = 0x2478;
    /// <summary>Minix v3, 60 char filenames</summary>
    const ushort MINIX3_MAGIC = 0x4D5A;

    // Byteswapped
    /// <summary>Minix v1, 14 char filenames</summary>
    const ushort MINIX_CIGAM = 0x7F13;
    /// <summary>Minix v1, 30 char filenames</summary>
    const ushort MINIX_CIGAM2 = 0x8F13;
    /// <summary>Minix v2, 14 char filenames</summary>
    const ushort MINIX2_CIGAM = 0x6824;
    /// <summary>Minix v2, 30 char filenames</summary>
    const ushort MINIX2_CIGAM2 = 0x7824;
    /// <summary>Minix v3, 60 char filenames</summary>
    const ushort MINIX3_CIGAM = 0x5A4D;

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => "Minix Filesystem";
    /// <inheritdoc />
    public Guid Id => new("FE248C3B-B727-4AE5-A39F-79EA9A07D4B3");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        uint sector = 2;
        uint offset = 0;

        if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
        {
            sector = 0;
            offset = 0x400;
        }

        if(sector + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(sector + partition.Start, out byte[] minixSbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        // Optical media
        if(offset > 0)
        {
            var tmp = new byte[0x200];
            Array.Copy(minixSbSector, offset, tmp, 0, 0x200);
            minixSbSector = tmp;
        }

        var magic = BitConverter.ToUInt16(minixSbSector, 0x010);

        if(magic == MINIX_MAGIC   ||
           magic == MINIX_MAGIC2  ||
           magic == MINIX2_MAGIC  ||
           magic == MINIX2_MAGIC2 ||
           magic == MINIX_CIGAM   ||
           magic == MINIX_CIGAM2  ||
           magic == MINIX2_CIGAM  ||
           magic == MINIX2_CIGAM2)
            return true;

        magic = BitConverter.ToUInt16(minixSbSector, 0x018); // Here should reside magic number on Minix v3

        return magic == MINIX_MAGIC  || magic == MINIX2_MAGIC || magic == MINIX3_MAGIC || magic == MINIX_CIGAM ||
               magic == MINIX2_CIGAM || magic == MINIX3_CIGAM;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        var sb = new StringBuilder();

        uint sector = 2;
        uint offset = 0;

        if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc)
        {
            sector = 0;
            offset = 0x400;
        }

        var         minix3 = false;
        int         filenamesize;
        string      minixVersion;
        ErrorNumber errno = imagePlugin.ReadSector(sector + partition.Start, out byte[] minixSbSector);

        if(errno != ErrorNumber.NoError)
            return;

        // Optical media
        if(offset > 0)
        {
            var tmp = new byte[0x200];
            Array.Copy(minixSbSector, offset, tmp, 0, 0x200);
            minixSbSector = tmp;
        }

        var magic = BitConverter.ToUInt16(minixSbSector, 0x018);

        XmlFsType = new FileSystemType();

        bool littleEndian;

        if(magic == MINIX3_MAGIC ||
           magic == MINIX3_CIGAM ||
           magic == MINIX2_MAGIC ||
           magic == MINIX2_CIGAM ||
           magic == MINIX_MAGIC  ||
           magic == MINIX_CIGAM)
        {
            filenamesize = 60;
            littleEndian = magic != MINIX3_CIGAM || magic == MINIX2_CIGAM || magic == MINIX_CIGAM;

            switch(magic)
            {
                case MINIX3_MAGIC:
                case MINIX3_CIGAM:
                    minixVersion   = "Minix v3 filesystem";
                    XmlFsType.Type = "Minix v3";

                    break;
                case MINIX2_MAGIC:
                case MINIX2_CIGAM:
                    minixVersion   = "Minix 3 v2 filesystem";
                    XmlFsType.Type = "Minix 3 v2";

                    break;
                default:
                    minixVersion   = "Minix 3 v1 filesystem";
                    XmlFsType.Type = "Minix 3 v1";

                    break;
            }

            minix3 = true;
        }
        else
        {
            magic = BitConverter.ToUInt16(minixSbSector, 0x010);

            switch(magic)
            {
                case MINIX_MAGIC:
                    filenamesize   = 14;
                    minixVersion   = "Minix v1 filesystem";
                    littleEndian   = true;
                    XmlFsType.Type = "Minix v1";

                    break;
                case MINIX_MAGIC2:
                    filenamesize   = 30;
                    minixVersion   = "Minix v1 filesystem";
                    littleEndian   = true;
                    XmlFsType.Type = "Minix v1";

                    break;
                case MINIX2_MAGIC:
                    filenamesize   = 14;
                    minixVersion   = "Minix v2 filesystem";
                    littleEndian   = true;
                    XmlFsType.Type = "Minix v2";

                    break;
                case MINIX2_MAGIC2:
                    filenamesize   = 30;
                    minixVersion   = "Minix v2 filesystem";
                    littleEndian   = true;
                    XmlFsType.Type = "Minix v2";

                    break;
                case MINIX_CIGAM:
                    filenamesize   = 14;
                    minixVersion   = "Minix v1 filesystem";
                    littleEndian   = false;
                    XmlFsType.Type = "Minix v1";

                    break;
                case MINIX_CIGAM2:
                    filenamesize   = 30;
                    minixVersion   = "Minix v1 filesystem";
                    littleEndian   = false;
                    XmlFsType.Type = "Minix v1";

                    break;
                case MINIX2_CIGAM:
                    filenamesize   = 14;
                    minixVersion   = "Minix v2 filesystem";
                    littleEndian   = false;
                    XmlFsType.Type = "Minix v2";

                    break;
                case MINIX2_CIGAM2:
                    filenamesize   = 30;
                    minixVersion   = "Minix v2 filesystem";
                    littleEndian   = false;
                    XmlFsType.Type = "Minix v2";

                    break;
                default: return;
            }
        }

        if(minix3)
        {
            SuperBlock3 mnxSb = littleEndian ? Marshal.ByteArrayToStructureLittleEndian<SuperBlock3>(minixSbSector)
                                    : Marshal.ByteArrayToStructureBigEndian<SuperBlock3>(minixSbSector);

            if(magic != MINIX3_MAGIC &&
               magic != MINIX3_CIGAM)
                mnxSb.s_blocksize = 1024;

            sb.AppendLine(minixVersion);
            sb.AppendFormat("{0} chars in filename", filenamesize).AppendLine();

            if(mnxSb.s_zones > 0) // On V2
                sb.AppendFormat("{0} zones on volume ({1} bytes)", mnxSb.s_zones, mnxSb.s_zones * 1024).AppendLine();
            else
                sb.AppendFormat("{0} zones on volume ({1} bytes)", mnxSb.s_nzones, mnxSb.s_nzones * 1024).AppendLine();

            sb.AppendFormat("{0} bytes/block", mnxSb.s_blocksize).AppendLine();
            sb.AppendFormat("{0} inodes on volume", mnxSb.s_ninodes).AppendLine();

            sb.AppendFormat("{0} blocks on inode map ({1} bytes)", mnxSb.s_imap_blocks,
                            mnxSb.s_imap_blocks * mnxSb.s_blocksize).AppendLine();

            sb.AppendFormat("{0} blocks on zone map ({1} bytes)", mnxSb.s_zmap_blocks,
                            mnxSb.s_zmap_blocks * mnxSb.s_blocksize).AppendLine();

            sb.AppendFormat("First data zone: {0}", mnxSb.s_firstdatazone).AppendLine();

            //sb.AppendFormat("log2 of blocks/zone: {0}", mnx_sb.s_log_zone_size).AppendLine(); // Apparently 0
            sb.AppendFormat("{0} bytes maximum per file", mnxSb.s_max_size).AppendLine();
            sb.AppendFormat("On-disk filesystem version: {0}", mnxSb.s_disk_version).AppendLine();

            XmlFsType.ClusterSize = mnxSb.s_blocksize;
            XmlFsType.Clusters    = mnxSb.s_zones > 0 ? mnxSb.s_zones : mnxSb.s_nzones;
        }
        else
        {
            SuperBlock mnxSb = littleEndian ? Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(minixSbSector)
                                   : Marshal.ByteArrayToStructureBigEndian<SuperBlock>(minixSbSector);

            sb.AppendLine(minixVersion);
            sb.AppendFormat("{0} chars in filename", filenamesize).AppendLine();

            if(mnxSb.s_zones > 0) // On V2
                sb.AppendFormat("{0} zones on volume ({1} bytes)", mnxSb.s_zones, mnxSb.s_zones * 1024).AppendLine();
            else
                sb.AppendFormat("{0} zones on volume ({1} bytes)", mnxSb.s_nzones, mnxSb.s_nzones * 1024).AppendLine();

            sb.AppendFormat("{0} inodes on volume", mnxSb.s_ninodes).AppendLine();

            sb.AppendFormat("{0} blocks on inode map ({1} bytes)", mnxSb.s_imap_blocks, mnxSb.s_imap_blocks * 1024).
               AppendLine();

            sb.AppendFormat("{0} blocks on zone map ({1} bytes)", mnxSb.s_zmap_blocks, mnxSb.s_zmap_blocks * 1024).
               AppendLine();

            sb.AppendFormat("First data zone: {0}", mnxSb.s_firstdatazone).AppendLine();

            //sb.AppendFormat("log2 of blocks/zone: {0}", mnx_sb.s_log_zone_size).AppendLine(); // Apparently 0
            sb.AppendFormat("{0} bytes maximum per file", mnxSb.s_max_size).AppendLine();
            sb.AppendFormat("Filesystem state: {0:X4}", mnxSb.s_state).AppendLine();
            XmlFsType.ClusterSize = 1024;
            XmlFsType.Clusters    = mnxSb.s_zones > 0 ? mnxSb.s_zones : mnxSb.s_nzones;
        }

        information = sb.ToString();
    }

    /// <summary>Superblock for Minix v1 and V2 filesystems</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        /// <summary>0x00, inodes on volume</summary>
        public readonly ushort s_ninodes;
        /// <summary>0x02, zones on volume</summary>
        public readonly ushort s_nzones;
        /// <summary>0x04, blocks on inode map</summary>
        public readonly short s_imap_blocks;
        /// <summary>0x06, blocks on zone map</summary>
        public readonly short s_zmap_blocks;
        /// <summary>0x08, first data zone</summary>
        public readonly ushort s_firstdatazone;
        /// <summary>0x0A, log2 of blocks/zone</summary>
        public readonly short s_log_zone_size;
        /// <summary>0x0C, max file size</summary>
        public readonly uint s_max_size;
        /// <summary>0x10, magic</summary>
        public readonly ushort s_magic;
        /// <summary>0x12, filesystem state</summary>
        public readonly ushort s_state;
        /// <summary>0x14, number of zones</summary>
        public readonly uint s_zones;
    }

    /// <summary>Superblock for Minix v3 filesystems</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SuperBlock3
    {
        /// <summary>0x00, inodes on volume</summary>
        public readonly uint s_ninodes;
        /// <summary>0x02, old zones on volume</summary>
        public readonly ushort s_nzones;
        /// <summary>0x06, blocks on inode map</summary>
        public readonly ushort s_imap_blocks;
        /// <summary>0x08, blocks on zone map</summary>
        public readonly ushort s_zmap_blocks;
        /// <summary>0x0A, first data zone</summary>
        public readonly ushort s_firstdatazone;
        /// <summary>0x0C, log2 of blocks/zone</summary>
        public readonly ushort s_log_zone_size;
        /// <summary>0x0E, padding</summary>
        public readonly ushort s_pad1;
        /// <summary>0x10, max file size</summary>
        public readonly uint s_max_size;
        /// <summary>0x14, number of zones</summary>
        public readonly uint s_zones;
        /// <summary>0x18, magic</summary>
        public readonly ushort s_magic;
        /// <summary>0x1A, padding</summary>
        public readonly ushort s_pad2;
        /// <summary>0x1C, bytes in a block</summary>
        public ushort s_blocksize;
        /// <summary>0x1E, on-disk structures version</summary>
        public readonly byte s_disk_version;
    }
}