// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Universal Disk Format plugin.
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

// TODO: Detect bootable
/// <inheritdoc />
/// <summary>Implements detection of the Universal Disk Format filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class UDF
{
#region Nested type: AnchorVolumeDescriptorPointer

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct AnchorVolumeDescriptorPointer
    {
        public readonly DescriptorTag    tag;
        public readonly ExtentDescriptor mainVolumeDescriptorSequenceExtent;
        public readonly ExtentDescriptor reserveVolumeDescriptorSequenceExtent;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)]
        public readonly byte[] reserved;
    }

#endregion

#region Nested type: CharacterSpecification

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CharacterSpecification
    {
        public readonly byte type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
        public readonly byte[] information;
    }

#endregion

#region Nested type: DescriptorTag

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DescriptorTag
    {
        public readonly TagIdentifier tagIdentifier;
        public readonly ushort        descriptorVersion;
        public readonly byte          tagChecksum;
        public readonly byte          reserved;
        public readonly ushort        tagSerialNumber;
        public readonly ushort        descriptorCrc;
        public readonly ushort        descriptorCrcLength;
        public readonly uint          tagLocation;
    }

#endregion

#region Nested type: EntityIdentifier

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct EntityIdentifier
    {
        /// <summary>Entity flags</summary>
        public readonly EntityFlags flags;
        /// <summary>Structure identifier</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
        public readonly byte[] identifier;
        /// <summary>Structure data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] identifierSuffix;
    }

#endregion

#region Nested type: ExtentDescriptor

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ExtentDescriptor
    {
        public readonly uint length;
        public readonly uint location;
    }

#endregion

#region Nested type: LogicalVolumeDescriptor

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct LogicalVolumeDescriptor
    {
        public readonly DescriptorTag          tag;
        public readonly uint                   volumeDescriptorSequenceNumber;
        public readonly CharacterSpecification descriptorCharacterSet;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] logicalVolumeIdentifier;
        public readonly uint             logicalBlockSize;
        public readonly EntityIdentifier domainIdentifier;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] logicalVolumeContentsUse;
        public readonly uint             mapTableLength;
        public readonly uint             numberOfPartitionMaps;
        public readonly EntityIdentifier implementationIdentifier;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] implementationUse;
        public readonly ExtentDescriptor integritySequenceExtent;
    }

#endregion

#region Nested type: LogicalVolumeIntegrityDescriptor

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct LogicalVolumeIntegrityDescriptor
    {
        public readonly DescriptorTag    tag;
        public readonly Timestamp        recordingDateTime;
        public readonly uint             integrityType;
        public readonly ExtentDescriptor nextIntegrityExtent;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] logicalVolumeContentsUse;
        public readonly uint numberOfPartitions;
        public readonly uint lengthOfImplementationUse;

        // Follows uint[numberOfPartitions] freeSpaceTable;
        // Follows uint[numberOfPartitions] sizeTable;
        // Follows byte[lengthOfImplementationUse] implementationUse;
    }

#endregion

#region Nested type: LogicalVolumeIntegrityDescriptorImplementationUse

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct LogicalVolumeIntegrityDescriptorImplementationUse
    {
        public readonly EntityIdentifier implementationId;
        public readonly uint             files;
        public readonly uint             directories;
        public readonly ushort           minimumReadUDF;
        public readonly ushort           minimumWriteUDF;
        public readonly ushort           maximumWriteUDF;
    }

#endregion

#region Nested type: PrimaryVolumeDescriptor

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct PrimaryVolumeDescriptor
    {
        public readonly DescriptorTag tag;
        public readonly uint          volumeDescriptorSequenceNumber;
        public readonly uint          primaryVolumeDescriptorNumber;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] volumeIdentifier;
        public readonly ushort volumeSequenceNumber;
        public readonly ushort maximumVolumeSequenceNumber;
        public readonly ushort interchangeLevel;
        public readonly ushort maximumInterchangeLevel;
        public readonly uint   characterSetList;
        public readonly uint   maximumCharacterSetList;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public readonly byte[] volumeSetIdentifier;
        public readonly CharacterSpecification descriptorCharacterSet;
        public readonly CharacterSpecification explanatoryCharacterSet;
        public readonly ExtentDescriptor       volumeAbstract;
        public readonly ExtentDescriptor       volumeCopyright;
        public readonly EntityIdentifier       applicationIdentifier;
        public readonly Timestamp              recordingDateTime;
        public readonly EntityIdentifier       implementationIdentifier;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public readonly byte[] implementationUse;
        public readonly uint   predecessorVolumeDescriptorSequenceLocation;
        public readonly ushort flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public readonly byte[] reserved;
    }

#endregion

#region Nested type: TagIdentifier

    enum TagIdentifier : ushort
    {
        PrimaryVolumeDescriptor           = 1,
        AnchorVolumeDescriptorPointer     = 2,
        VolumeDescriptorPointer           = 3,
        ImplementationUseVolumeDescriptor = 4,
        PartitionDescriptor               = 5,
        LogicalVolumeDescriptor           = 6,
        UnallocatedSpaceDescriptor        = 7,
        TerminatingDescriptor             = 8,
        LogicalVolumeIntegrityDescriptor  = 9
    }

#endregion

#region Nested type: Timestamp

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Timestamp
    {
        public readonly ushort typeAndZone;
        public readonly short  year;
        public readonly byte   month;
        public readonly byte   day;
        public readonly byte   hour;
        public readonly byte   minute;
        public readonly byte   second;
        public readonly byte   centiseconds;
        public readonly byte   hundredsMicroseconds;
        public readonly byte   microseconds;
    }

#endregion
}