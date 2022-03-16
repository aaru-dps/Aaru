// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SmartFileSystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the SmartFileSystem and shows information.
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
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

/// <inheritdoc />
/// <summary>Implements detection of the Smart File System</summary>
public sealed class SFS : IFilesystem
{
    /// <summary>Identifier for SFS v1</summary>
    const uint SFS_MAGIC = 0x53465300;
    /// <summary>Identifier for SFS v2</summary>
    const uint SFS2_MAGIC = 0x53465302;

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => "SmartFileSystem";
    /// <inheritdoc />
    public Guid Id => new("26550C19-3671-4A2D-BC2F-F20CEB7F48DC");
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

        var magic = BigEndianBitConverter.ToUInt32(sector, 0x00);

        return magic is SFS_MAGIC or SFS2_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        information = "";
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");
        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] rootBlockSector);

        if(errno != ErrorNumber.NoError)
            return;

        RootBlock rootBlock = Marshal.ByteArrayToStructureBigEndian<RootBlock>(rootBlockSector);

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine("SmartFileSystem");

        sbInformation.AppendFormat("Volume version {0}", rootBlock.version).AppendLine();

        sbInformation.AppendFormat("Volume starts on device byte {0} and ends on byte {1}", rootBlock.firstbyte,
                                   rootBlock.lastbyte).AppendLine();

        sbInformation.
            AppendFormat("Volume has {0} blocks of {1} bytes each", rootBlock.totalblocks, rootBlock.blocksize).
            AppendLine();

        sbInformation.AppendFormat("Volume created on {0}",
                                   DateHandlers.UnixUnsignedToDateTime(rootBlock.datecreated).AddYears(8)).AppendLine();

        sbInformation.AppendFormat("Bitmap starts in block {0}", rootBlock.bitmapbase).AppendLine();

        sbInformation.AppendFormat("Admin space container starts in block {0}", rootBlock.adminspacecontainer).
                      AppendLine();

        sbInformation.AppendFormat("Root object container starts in block {0}", rootBlock.rootobjectcontainer).
                      AppendLine();

        sbInformation.AppendFormat("Root node of the extent B-tree resides in block {0}", rootBlock.extentbnoderoot).
                      AppendLine();

        sbInformation.AppendFormat("Root node of the object B-tree resides in block {0}", rootBlock.objectnoderoot).
                      AppendLine();

        if(rootBlock.bits.HasFlag(Flags.CaseSensitive))
            sbInformation.AppendLine("Volume is case sensitive");

        if(rootBlock.bits.HasFlag(Flags.RecycledFolder))
            sbInformation.AppendLine("Volume moves deleted files to a recycled folder");

        information = sbInformation.ToString();

        XmlFsType = new FileSystemType
        {
            CreationDate          = DateHandlers.UnixUnsignedToDateTime(rootBlock.datecreated).AddYears(8),
            CreationDateSpecified = true,
            Clusters              = rootBlock.totalblocks,
            ClusterSize           = rootBlock.blocksize,
            Type                  = "SmartFileSystem"
        };
    }

    [Flags]
    enum Flags : byte
    {
        RecycledFolder = 64,
        CaseSensitive  = 128
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RootBlock
    {
        public readonly uint   blockId;
        public readonly uint   blockChecksum;
        public readonly uint   blockSelfPointer;
        public readonly ushort version;
        public readonly ushort sequence;
        public readonly uint   datecreated;
        public readonly Flags  bits;
        public readonly byte   padding1;
        public readonly ushort padding2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly uint[] reserved1;
        public readonly ulong firstbyte;
        public readonly ulong lastbyte;
        public readonly uint  totalblocks;
        public readonly uint  blocksize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly uint[] reserved2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly uint[] reserved3;
        public readonly uint bitmapbase;
        public readonly uint adminspacecontainer;
        public readonly uint rootobjectcontainer;
        public readonly uint extentbnoderoot;
        public readonly uint objectnoderoot;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly uint[] reserved4;
    }
}