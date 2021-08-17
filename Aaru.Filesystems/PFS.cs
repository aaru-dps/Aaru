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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems
{
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

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "Professional File System";
        /// <inheritdoc />
        public Guid Id => new Guid("68DE769E-D957-406A-8AE4-3781CA8CDA77");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(partition.Length < 3)
                return false;

            byte[] sector = imagePlugin.ReadSector(2 + partition.Start);

            uint magic = BigEndianBitConverter.ToUInt32(sector, 0x00);

            return magic == AFS_DISK || magic == PFS2_DISK || magic == PFS_DISK || magic == MUAF_DISK ||
                   magic == MUPFS_DISK;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding = encoding ?? Encoding.GetEncoding("iso-8859-1");
            byte[]    rootBlockSector = imagePlugin.ReadSector(2 + partition.Start);
            RootBlock rootBlock       = Marshal.ByteArrayToStructureBigEndian<RootBlock>(rootBlockSector);

            var sbInformation = new StringBuilder();
            XmlFsType = new FileSystemType();

            switch(rootBlock.diskType)
            {
                case AFS_DISK:
                case MUAF_DISK:
                    sbInformation.Append("Professional File System v1");
                    XmlFsType.Type = "PFS v1";

                    break;
                case PFS2_DISK:
                    sbInformation.Append("Professional File System v2");
                    XmlFsType.Type = "PFS v2";

                    break;
                case PFS_DISK:
                case MUPFS_DISK:
                    sbInformation.Append("Professional File System v3");
                    XmlFsType.Type = "PFS v3";

                    break;
            }

            if(rootBlock.diskType == MUAF_DISK ||
               rootBlock.diskType == MUPFS_DISK)
                sbInformation.Append(", with multi-user support");

            sbInformation.AppendLine();

            sbInformation.AppendFormat("Volume name: {0}", StringHandlers.PascalToString(rootBlock.diskname, Encoding)).
                          AppendLine();

            sbInformation.AppendFormat("Volume has {0} free sectors of {1}", rootBlock.blocksfree, rootBlock.diskSize).
                          AppendLine();

            sbInformation.AppendFormat("Volume created on {0}",
                                       DateHandlers.AmigaToDateTime(rootBlock.creationday, rootBlock.creationminute,
                                                                    rootBlock.creationtick)).AppendLine();

            if(rootBlock.extension > 0)
                sbInformation.AppendFormat("Root block extension resides at block {0}", rootBlock.extension).
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
}