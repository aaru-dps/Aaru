// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Fossil.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Fossil filesystem plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Fossil filesystem and shows information.
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
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection for the Plan-9 Fossil on-disk filesystem</summary>
public sealed class Fossil : IFilesystem
{
    const uint FOSSIL_HDR_MAGIC = 0x3776AE89;
    const uint FOSSIL_SB_MAGIC  = 0x2340A3B1;

    // Fossil header starts at 128KiB
    const ulong HEADER_POS = 128 * 1024;

    const string FS_TYPE = "fossil";

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.Fossil_Name;
    /// <inheritdoc />
    public Guid Id => new("932BF104-43F6-494F-973C-45EF58A51DA9");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        ulong hdrSector = HEADER_POS / imagePlugin.Info.SectorSize;

        if(partition.Start + hdrSector > imagePlugin.Info.Sectors)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + hdrSector, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        Header hdr = Marshal.ByteArrayToStructureBigEndian<Header>(sector);

        AaruConsole.DebugWriteLine("Fossil plugin", Localization.magic_at_0_expected_1, hdr.magic, FOSSIL_HDR_MAGIC);

        return hdr.magic == FOSSIL_HDR_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        // Technically everything on Plan 9 from Bell Labs is in UTF-8
        Encoding    = Encoding.UTF8;
        information = "";

        if(imagePlugin.Info.SectorSize < 512)
            return;

        ulong hdrSector = HEADER_POS / imagePlugin.Info.SectorSize;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + hdrSector, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        Header hdr = Marshal.ByteArrayToStructureBigEndian<Header>(sector);

        AaruConsole.DebugWriteLine("Fossil plugin", Localization.magic_at_0_expected_1, hdr.magic, FOSSIL_HDR_MAGIC);

        var sb = new StringBuilder();

        sb.AppendLine(Localization.Fossil_filesystem);
        sb.AppendFormat(Localization.Filesystem_version_0, hdr.version).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_block, hdr.blockSize).AppendLine();
        sb.AppendFormat(Localization.Superblock_resides_in_block_0, hdr.super).AppendLine();
        sb.AppendFormat(Localization.Labels_resides_in_block_0, hdr.label).AppendLine();
        sb.AppendFormat(Localization.Data_starts_at_block_0, hdr.data).AppendLine();
        sb.AppendFormat(Localization.Volume_has_0_blocks, hdr.end).AppendLine();

        ulong sbLocation = (hdr.super * (hdr.blockSize / imagePlugin.Info.SectorSize)) + partition.Start;

        XmlFsType = new FileSystemType
        {
            Type        = FS_TYPE,
            ClusterSize = hdr.blockSize,
            Clusters    = hdr.end
        };

        if(sbLocation <= partition.End)
        {
            imagePlugin.ReadSector(sbLocation, out sector);
            SuperBlock fsb = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sector);

            AaruConsole.DebugWriteLine("Fossil plugin", Localization.magic_0_expected_1, fsb.magic, FOSSIL_SB_MAGIC);

            if(fsb.magic == FOSSIL_SB_MAGIC)
            {
                sb.AppendFormat(Localization.Epoch_low_0, fsb.epochLow).AppendLine();
                sb.AppendFormat(Localization.Epoch_high_0, fsb.epochHigh).AppendLine();
                sb.AppendFormat(Localization.Next_QID_0, fsb.qid).AppendLine();
                sb.AppendFormat(Localization.Active_root_block_0, fsb.active).AppendLine();
                sb.AppendFormat(Localization.Next_root_block_0, fsb.next).AppendLine();
                sb.AppendFormat(Localization.Current_root_block_0, fsb.current).AppendLine();
                sb.AppendFormat(Localization.Volume_label_0, StringHandlers.CToString(fsb.name, Encoding)).AppendLine();
                XmlFsType.VolumeName = StringHandlers.CToString(fsb.name, Encoding);
            }
        }

        information = sb.ToString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Header
    {
        /// <summary>Magic number</summary>
        public readonly uint magic;
        /// <summary>Header version</summary>
        public readonly ushort version;
        /// <summary>Block size</summary>
        public readonly ushort blockSize;
        /// <summary>Block containing superblock</summary>
        public readonly uint super;
        /// <summary>Block containing labels</summary>
        public readonly uint label;
        /// <summary>Where do data blocks start</summary>
        public readonly uint data;
        /// <summary>How many data blocks does it have</summary>
        public readonly uint end;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        /// <summary>Magic number</summary>
        public readonly uint magic;
        /// <summary>Header version</summary>
        public readonly ushort version;
        /// <summary>file system low epoch</summary>
        public readonly uint epochLow;
        /// <summary>file system high(active) epoch</summary>
        public readonly uint epochHigh;
        /// <summary>next qid to allocate</summary>
        public readonly ulong qid;
        /// <summary>data block number: root of active file system</summary>
        public readonly int active;
        /// <summary>data block number: root of next file system to archive</summary>
        public readonly int next;
        /// <summary>data block number: root of file system currently being archived</summary>
        public readonly int current;
        /// <summary>Venti score of last successful archive</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] last;
        /// <summary>name of file system(just a comment)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] name;
    }
}