// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : extFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux extended filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Linux extended filesystem and shows information.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Schemas;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the Linux extended filesystem</summary>

// ReSharper disable once InconsistentNaming
public sealed class extFS : IFilesystem
{
    const int SB_POS = 0x400;

    /// <summary>ext superblock magic</summary>
    const ushort EXT_MAGIC = 0x137D;

    const string FS_TYPE = "ext";

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.extFS_Name;
    /// <inheritdoc />
    public Guid Id => new("076CB3A2-08C2-4D69-BC8A-FCAA2E502BE2");
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        ulong sbSectorOff = SB_POS / imagePlugin.Info.SectorSize;
        uint  sbOff       = SB_POS % imagePlugin.Info.SectorSize;

        if(sbSectorOff + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(sbSectorOff + partition.Start, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        byte[] sb = new byte[512];

        if(sbOff + 512 > sbSector.Length)
            return false;

        Array.Copy(sbSector, sbOff, sb, 0, 512);

        ushort magic = BitConverter.ToUInt16(sb, 0x038);

        return magic == EXT_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        var sb = new StringBuilder();

        if(imagePlugin.Info.SectorSize < 512)
            return;

        ulong sbSectorOff = SB_POS / imagePlugin.Info.SectorSize;
        uint  sbOff       = SB_POS % imagePlugin.Info.SectorSize;

        if(sbSectorOff + partition.Start >= partition.End)
            return;

        ErrorNumber errno = imagePlugin.ReadSector(sbSectorOff + partition.Start, out byte[] sblock);

        if(errno != ErrorNumber.NoError)
            return;

        byte[] sbSector = new byte[512];
        Array.Copy(sblock, sbOff, sbSector, 0, 512);

        var extSb = new SuperBlock
        {
            inodes        = BitConverter.ToUInt32(sbSector, 0x000),
            zones         = BitConverter.ToUInt32(sbSector, 0x004),
            firstfreeblk  = BitConverter.ToUInt32(sbSector, 0x008),
            freecountblk  = BitConverter.ToUInt32(sbSector, 0x00C),
            firstfreeind  = BitConverter.ToUInt32(sbSector, 0x010),
            freecountind  = BitConverter.ToUInt32(sbSector, 0x014),
            firstdatazone = BitConverter.ToUInt32(sbSector, 0x018),
            logzonesize   = BitConverter.ToUInt32(sbSector, 0x01C),
            maxsize       = BitConverter.ToUInt32(sbSector, 0x020)
        };

        sb.AppendLine(Localization.ext_filesystem);
        sb.AppendFormat(Localization._0_zones_on_volume, extSb.zones);
        sb.AppendFormat(Localization._0_free_blocks_1_bytes, extSb.freecountblk, extSb.freecountblk * 1024);

        sb.AppendFormat(Localization._0_inodes_on_volume_1_free_2, extSb.inodes, extSb.freecountind,
                        extSb.freecountind * 100 / extSb.inodes);

        sb.AppendFormat(Localization.First_free_inode_is_0, extSb.firstfreeind);
        sb.AppendFormat(Localization.First_free_block_is_0, extSb.firstfreeblk);
        sb.AppendFormat(Localization.First_data_zone_is_0, extSb.firstdatazone);
        sb.AppendFormat(Localization.Log_zone_size_0, extSb.logzonesize);
        sb.AppendFormat(Localization.Max_zone_size_0, extSb.maxsize);

        XmlFsType = new FileSystemType
        {
            Type                  = FS_TYPE,
            FreeClusters          = extSb.freecountblk,
            FreeClustersSpecified = true,
            ClusterSize           = 1024,
            Clusters              = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize / 1024
        };

        information = sb.ToString();
    }

    /// <summary>ext superblock</summary>
    #pragma warning disable CS0649
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    struct SuperBlock
    {
        /// <summary>0x000, inodes on volume</summary>
        public uint inodes;
        /// <summary>0x004, zones on volume</summary>
        public uint zones;
        /// <summary>0x008, first free block</summary>
        public uint firstfreeblk;
        /// <summary>0x00C, free blocks count</summary>
        public uint freecountblk;
        /// <summary>0x010, first free inode</summary>
        public uint firstfreeind;
        /// <summary>0x014, free inodes count</summary>
        public uint freecountind;
        /// <summary>0x018, first data zone</summary>
        public uint firstdatazone;
        /// <summary>0x01C, log zone size</summary>
        public uint logzonesize;
        /// <summary>0x020, max zone size</summary>
        public uint maxsize;
        /// <summary>0x024, reserved</summary>
        public uint reserved1;
        /// <summary>0x028, reserved</summary>
        public uint reserved2;
        /// <summary>0x02C, reserved</summary>
        public uint reserved3;
        /// <summary>0x030, reserved</summary>
        public uint reserved4;
        /// <summary>0x034, reserved</summary>
        public uint reserved5;
        /// <summary>0x038, 0x137D (little endian)</summary>
        public ushort magic;
    }
    #pragma warning restore CS0649
}