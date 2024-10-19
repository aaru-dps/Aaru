// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for Microsoft Hyper-V disk images.
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

using System;
using System.Runtime.InteropServices;

namespace Aaru.Images;

public sealed partial class Vhdx
{
#region Nested type: FileParameters

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FileParameters
    {
        /// <summary>Block size in bytes</summary>
        public uint blockSize;
        /// <summary>Flags</summary>
        public uint flags;
    }

#endregion

#region Nested type: Header

#pragma warning disable 649
#pragma warning disable 169
    struct Header
    {
        /// <summary>Signature, <see cref="Vhdx.VHDX_HEADER_SIG" /></summary>
        public uint Signature;
        /// <summary>CRC-32C of whole 4096 bytes header with this field set to 0</summary>
        public uint Checksum;
        /// <summary>Sequence number</summary>
        public ulong Sequence;
        /// <summary>Unique identifier for file contents, must be changed on first write to metadata</summary>
        public Guid FileWriteGuid;
        /// <summary>Unique identifier for disk contents, must be changed on first write to metadata or data</summary>
        public Guid DataWriteGuid;
        /// <summary>Unique identifier for log entries</summary>
        public Guid LogGuid;
        /// <summary>Version of log format</summary>
        public ushort LogVersion;
        /// <summary>Version of VHDX format</summary>
        public ushort Version;
        /// <summary>Length in bytes of the log</summary>
        public uint LogLength;
        /// <summary>Offset from image start to the log</summary>
        public ulong LogOffset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4016)]
        public byte[] Reserved;
    }
#pragma warning restore 649
#pragma warning restore 169

#endregion

#region Nested type: Identifier

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Identifier
    {
        /// <summary>Signature, <see cref="Vhdx.VHDX_SIGNATURE" /></summary>
        public readonly ulong signature;
        /// <summary>UTF-16 string containing creator</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public readonly byte[] creator;
    }

#endregion

#region Nested type: MetadataTableEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct MetadataTableEntry
    {
        /// <summary>Metadata ID</summary>
        public readonly Guid itemId;
        /// <summary>Offset relative to start of metadata region</summary>
        public readonly uint offset;
        /// <summary>Length in bytes</summary>
        public readonly uint length;
        /// <summary>Flags</summary>
        public readonly uint flags;
        /// <summary>Reserved</summary>
        public readonly uint reserved;
    }

#endregion

#region Nested type: MetadataTableHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct MetadataTableHeader
    {
        /// <summary>Signature</summary>
        public readonly ulong signature;
        /// <summary>Reserved</summary>
        public readonly ushort reserved;
        /// <summary>How many entries are in the table</summary>
        public readonly ushort entries;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly uint[] reserved2;
    }

#endregion

#region Nested type: ParentLocatorEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ParentLocatorEntry
    {
        /// <summary>Offset from metadata to key</summary>
        public readonly uint keyOffset;
        /// <summary>Offset from metadata to value</summary>
        public readonly uint valueOffset;
        /// <summary>Size of key</summary>
        public readonly ushort keyLength;
        /// <summary>Size of value</summary>
        public readonly ushort valueLength;
    }

#endregion

#region Nested type: ParentLocatorHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ParentLocatorHeader
    {
        /// <summary>Type of parent virtual disk</summary>
        public readonly Guid locatorType;
        public readonly ushort reserved;
        /// <summary>How many KVPs are in this parent locator</summary>
        public readonly ushort keyValueCount;
    }

#endregion

#region Nested type: RegionTableEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RegionTableEntry
    {
        /// <summary>Object identifier</summary>
        public readonly Guid guid;
        /// <summary>Offset in image of the object</summary>
        public readonly ulong offset;
        /// <summary>Length in bytes of the object</summary>
        public readonly uint length;
        /// <summary>Flags</summary>
        public readonly uint flags;
    }

#endregion

#region Nested type: RegionTableHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RegionTableHeader
    {
        /// <summary>Signature, <see cref="Vhdx.VHDX_REGION_SIG" /></summary>
        public readonly uint signature;
        /// <summary>CRC-32C of whole 64Kb table with this field set to 0</summary>
        public readonly uint checksum;
        /// <summary>How many entries follow this table</summary>
        public readonly uint entries;
        /// <summary>Reserved</summary>
        public readonly uint reserved;
    }

#endregion
}