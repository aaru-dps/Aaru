// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Squash.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Squash file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Squash file system and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the squash filesystem</summary>
public sealed class Squash : IFilesystem
{
    /// <summary>Identifier for Squash</summary>
    const uint SQUASH_MAGIC = 0x73717368;
    const uint SQUASH_CIGAM = 0x68737173;

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => "Squash filesystem";
    /// <inheritdoc />
    public Guid Id => new("F8F6E46F-7A2A-48E3-9C0A-46AF4DC29E09");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        uint magic = BitConverter.ToUInt32(sector, 0x00);

        return magic == SQUASH_MAGIC || magic == SQUASH_CIGAM;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                               Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.UTF8;
        information = "";
        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        uint magic = BitConverter.ToUInt32(sector, 0x00);

        var  sqSb         = new SuperBlock();
        bool littleEndian = true;

        switch(magic)
        {
            case SQUASH_MAGIC:
                sqSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

                break;
            case SQUASH_CIGAM:
                sqSb         = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sector);
                littleEndian = false;

                break;
        }

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine("Squash file system");
        sbInformation.AppendLine(littleEndian ? "Little-endian" : "Big-endian");
        sbInformation.AppendFormat("Volume version {0}.{1}", sqSb.s_major, sqSb.s_minor).AppendLine();
        sbInformation.AppendFormat("Volume has {0} bytes", sqSb.bytes_used).AppendLine();
        sbInformation.AppendFormat("Volume has {0} bytes per block", sqSb.block_size).AppendLine();

        sbInformation.AppendFormat("Volume created on {0}", DateHandlers.UnixUnsignedToDateTime(sqSb.mkfs_time)).
                      AppendLine();

        sbInformation.AppendFormat("Volume has {0} inodes", sqSb.inodes).AppendLine();

        switch(sqSb.compression)
        {
            case (ushort)SquashCompression.Lz4:
                sbInformation.AppendLine("Volume is compressed using LZ4");

                break;
            case (ushort)SquashCompression.Lzo:
                sbInformation.AppendLine("Volume is compressed using LZO");

                break;
            case (ushort)SquashCompression.Lzma:
                sbInformation.AppendLine("Volume is compressed using LZMA");

                break;
            case (ushort)SquashCompression.Xz:
                sbInformation.AppendLine("Volume is compressed using XZ");

                break;
            case (ushort)SquashCompression.Zlib:
                sbInformation.AppendLine("Volume is compressed using GZIP");

                break;
            case (ushort)SquashCompression.Zstd:
                sbInformation.AppendLine("Volume is compressed using Zstandard");

                break;
            default:
                sbInformation.AppendFormat("Volume is compressed using unknown algorithm {0}", sqSb.compression).
                              AppendLine();

                break;
        }

        information = sbInformation.ToString();

        XmlFsType = new FileSystemType
        {
            Type = "Squash file system",
            CreationDate = DateHandlers.UnixUnsignedToDateTime(sqSb.mkfs_time),
            CreationDateSpecified = true,
            Clusters = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize / sqSb.block_size,
            ClusterSize = sqSb.block_size,
            Files = sqSb.inodes,
            FilesSpecified = true,
            FreeClusters = 0,
            FreeClustersSpecified = true
        };
    }

    enum SquashCompression : ushort
    {
        Zlib = 1, Lzma = 2, Lzo  = 3,
        Xz   = 4, Lz4  = 5, Zstd = 6
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        public readonly uint   magic;
        public readonly uint   inodes;
        public readonly uint   mkfs_time;
        public readonly uint   block_size;
        public readonly uint   fragments;
        public readonly ushort compression;
        public readonly ushort block_log;
        public readonly ushort flags;
        public readonly ushort no_ids;
        public readonly ushort s_major;
        public readonly ushort s_minor;
        public readonly ulong  root_inode;
        public readonly ulong  bytes_used;
        public readonly ulong  id_table_start;
        public readonly ulong  xattr_id_table_start;
        public readonly ulong  inode_table_start;
        public readonly ulong  directory_table_start;
        public readonly ulong  fragment_table_start;
        public readonly ulong  lookup_table_start;
    }
}