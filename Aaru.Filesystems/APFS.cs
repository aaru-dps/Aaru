// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : APFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Apple filesystem and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Apple File System (APFS)</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class APFS : IFilesystem
{
    const uint APFS_CONTAINER_MAGIC = 0x4253584E; // "NXSB"
    const uint APFS_VOLUME_MAGIC    = 0x42535041; // "APSB"

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.APFS_Name;
    /// <inheritdoc />
    public Guid Id => new("A4060F9D-2909-42E2-9D95-DB31FA7EA797");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        ContainerSuperBlock nxSb;

        try
        {
            nxSb = Marshal.ByteArrayToStructureLittleEndian<ContainerSuperBlock>(sector);
        }
        catch
        {
            return false;
        }

        return nxSb.magic == APFS_CONTAINER_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding = Encoding.UTF8;
        var sbInformation = new StringBuilder();
        XmlFsType   = new FileSystemType();
        information = "";

        if(partition.Start >= partition.End)
            return;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        ContainerSuperBlock nxSb;

        try
        {
            nxSb = Marshal.ByteArrayToStructureLittleEndian<ContainerSuperBlock>(sector);
        }
        catch
        {
            return;
        }

        if(nxSb.magic != APFS_CONTAINER_MAGIC)
            return;

        sbInformation.AppendLine(Localization.Apple_File_System);
        sbInformation.AppendLine();
        sbInformation.AppendFormat(Localization._0_bytes_per_block, nxSb.blockSize).AppendLine();

        sbInformation.AppendFormat(Localization.Container_has_0_bytes_in_1_blocks,
                                   nxSb.containerBlocks * nxSb.blockSize, nxSb.containerBlocks).AppendLine();

        information = sbInformation.ToString();

        XmlFsType = new FileSystemType
        {
            Bootable    = false,
            Clusters    = nxSb.containerBlocks,
            ClusterSize = nxSb.blockSize,
            Type        = FS_TYPE
        };
    }

    const string FS_TYPE = "apfs";

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ContainerSuperBlock
    {
        public readonly ulong unknown1; // Varies between copies of the superblock
        public readonly ulong unknown2;
        public readonly ulong unknown3; // Varies by 1 between copies of the superblock
        public readonly ulong unknown4;
        public readonly uint  magic;
        public readonly uint  blockSize;
        public readonly ulong containerBlocks;
    }
}