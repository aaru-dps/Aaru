// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Professional File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Professional File System and shows information.
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

// ReSharper disable UnusedType.Local

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
/// <summary>Implements detection of the Professional File System</summary>
public sealed class PFS : IFilesystem
{
    /// <summary>Identifier for AFS (PFS v1)</summary>
    const uint AFS_DISK = 0x41465301;
    /// <summary>Identifier for PFS v2</summary>
    const uint PFS2_DISK = 0x50465302;
    /// <summary>Identifier for PFS v3</summary>
    const uint PFS_DISK = 0x50465301;
    /// <summary>Identifier for multi-user AFS</summary>
    const uint MUAF_DISK = 0x6D754146;
    /// <summary>Identifier for multi-user PFS</summary>
    const uint MUPFS_DISK = 0x6D755046;

    const string FS_TYPE = "pfs";

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.PFS_Name;
    /// <inheritdoc />
    public Guid Id => new("68DE769E-D957-406A-8AE4-3781CA8CDA77");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Length < 3)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(2 + partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        uint magic = BigEndianBitConverter.ToUInt32(sector, 0x00);

        return magic is AFS_DISK or PFS2_DISK or PFS_DISK or MUAF_DISK or MUPFS_DISK;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        information = "";
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");
        ErrorNumber errno = imagePlugin.ReadSector(2 + partition.Start, out byte[] rootBlockSector);

        if(errno != ErrorNumber.NoError)
            return;

        RootBlock rootBlock = Marshal.ByteArrayToStructureBigEndian<RootBlock>(rootBlockSector);

        var sbInformation = new StringBuilder();
        XmlFsType = new FileSystemType();

        switch(rootBlock.diskType)
        {
            case AFS_DISK:
            case MUAF_DISK:
                sbInformation.Append(Localization.Professional_File_System_v1);
                XmlFsType.Type = FS_TYPE;

                break;
            case PFS2_DISK:
                sbInformation.Append(Localization.Professional_File_System_v2);
                XmlFsType.Type = FS_TYPE;

                break;
            case PFS_DISK:
            case MUPFS_DISK:
                sbInformation.Append(Localization.Professional_File_System_v3);
                XmlFsType.Type = FS_TYPE;

                break;
        }

        if(rootBlock.diskType is MUAF_DISK or MUPFS_DISK)
            sbInformation.Append(Localization.with_multi_user_support);

        sbInformation.AppendLine();

        sbInformation.
            AppendFormat(Localization.Volume_name_0, StringHandlers.PascalToString(rootBlock.diskname, Encoding)).
            AppendLine();

        sbInformation.
            AppendFormat(Localization.Volume_has_0_free_sectors_of_1, rootBlock.blocksfree, rootBlock.diskSize).
            AppendLine();

        sbInformation.AppendFormat(Localization.Volume_created_on_0,
                                   DateHandlers.AmigaToDateTime(rootBlock.creationday, rootBlock.creationminute,
                                                                rootBlock.creationtick)).AppendLine();

        if(rootBlock.extension > 0)
            sbInformation.AppendFormat(Localization.Root_block_extension_resides_at_block_0, rootBlock.extension).
                          AppendLine();

        information = sbInformation.ToString();

        XmlFsType.CreationDate =
            DateHandlers.AmigaToDateTime(rootBlock.creationday, rootBlock.creationminute, rootBlock.creationtick);

        XmlFsType.CreationDateSpecified = true;
        XmlFsType.FreeClusters          = rootBlock.blocksfree;
        XmlFsType.FreeClustersSpecified = true;
        XmlFsType.Clusters              = rootBlock.diskSize;
        XmlFsType.ClusterSize           = imagePlugin.Info.SectorSize;
        XmlFsType.VolumeName            = StringHandlers.PascalToString(rootBlock.diskname, Encoding);
    }

    /// <summary>Boot block, first 2 sectors</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BootBlock
    {
        /// <summary>"PFS\1" disk type</summary>
        public readonly uint diskType;
        /// <summary>Boot code, til completion</summary>
        public readonly byte[] bootCode;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RootBlock
    {
        /// <summary>Disk type</summary>
        public readonly uint diskType;
        /// <summary>Options</summary>
        public readonly uint options;
        /// <summary>Current datestamp</summary>
        public readonly uint datestamp;
        /// <summary>Volume creation day</summary>
        public readonly ushort creationday;
        /// <summary>Volume creation minute</summary>
        public readonly ushort creationminute;
        /// <summary>Volume creation tick</summary>
        public readonly ushort creationtick;
        /// <summary>AmigaDOS protection bits</summary>
        public readonly ushort protection;
        /// <summary>Volume label (Pascal string)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] diskname;
        /// <summary>Last reserved block</summary>
        public readonly uint lastreserved;
        /// <summary>First reserved block</summary>
        public readonly uint firstreserved;
        /// <summary>Free reserved blocks</summary>
        public readonly uint reservedfree;
        /// <summary>Size of reserved blocks in bytes</summary>
        public readonly ushort reservedblocksize;
        /// <summary>Blocks in rootblock, including bitmap</summary>
        public readonly ushort rootblockclusters;
        /// <summary>Free blocks</summary>
        public readonly uint blocksfree;
        /// <summary>Blocks that must be always free</summary>
        public readonly uint alwaysfree;
        /// <summary>Current bitmapfield number for allocation</summary>
        public readonly uint rovingPointer;
        /// <summary>Pointer to deldir</summary>
        public readonly uint delDirPtr;
        /// <summary>Disk size in sectors</summary>
        public readonly uint diskSize;
        /// <summary>Rootblock extension</summary>
        public readonly uint extension;
        /// <summary>Unused</summary>
        public readonly uint unused;
    }
}