// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft exFAT filesystem plugin.
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

namespace Aaru.Filesystems;

// Information from https://www.sans.org/reading-room/whitepapers/forensics/reverse-engineering-microsoft-exfat-file-system-33274
/// <inheritdoc />
/// <summary>Implements detection of the exFAT filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]

// ReSharper disable once InconsistentNaming
public sealed partial class exFAT
{
#region Nested type: ChecksumSector

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ChecksumSector
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly uint[] checksum;
    }

#endregion

#region Nested type: OemParameter

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct OemParameter
    {
        public readonly Guid OemParameterType;
        public readonly uint eraseBlockSize;
        public readonly uint pageSize;
        public readonly uint spareBlocks;
        public readonly uint randomAccessTime;
        public readonly uint programTime;
        public readonly uint readCycleTime;
        public readonly uint writeCycleTime;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] reserved;
    }

#endregion

#region Nested type: OemParameterTable

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct OemParameterTable
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly OemParameter[] parameters;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] padding;
    }

#endregion

#region Nested type: VolumeBootRecord

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct VolumeBootRecord
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] signature;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 53)]
        public readonly byte[] zero;
        public readonly ulong       offset;
        public readonly ulong       sectors;
        public readonly uint        fatOffset;
        public readonly uint        fatLength;
        public readonly uint        clusterHeapOffset;
        public readonly uint        clusterHeapLength;
        public readonly uint        rootDirectoryCluster;
        public readonly uint        volumeSerial;
        public readonly ushort      revision;
        public readonly VolumeFlags flags;
        public readonly byte        sectorShift;
        public readonly byte        clusterShift;
        public readonly byte        fats;
        public readonly byte        drive;
        public readonly byte        heapUsage;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 53)]
        public readonly byte[] reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 53)]
        public readonly byte[] bootCode;
        public readonly ushort bootSignature;
    }

#endregion
}