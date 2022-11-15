// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UDF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Universal Disk Format plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Universal Disk Format and shows information.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

// TODO: Detect bootable
/// <inheritdoc />
/// <summary>Implements detection of the Universal Disk Format filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class UDF : IFilesystem
{
    readonly byte[] _magic =
    {
        0x2A, 0x4F, 0x53, 0x54, 0x41, 0x20, 0x55, 0x44, 0x46, 0x20, 0x43, 0x6F, 0x6D, 0x70, 0x6C, 0x69, 0x61, 0x6E,
        0x74, 0x00, 0x00, 0x00, 0x00
    };

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => "Universal Disk Format";
    /// <inheritdoc />
    public Guid Id => new("83976FEC-A91B-464B-9293-56C719461BAB");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        // UDF needs at least that
        if(partition.End - partition.Start < 256)
            return false;

        // UDF needs at least that
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        byte[] sector;
        var    anchor = new AnchorVolumeDescriptorPointer();

        // All positions where anchor may reside, with the ratio between 512 and 2048bps
        ulong[][] positions =
        {
            new ulong[]
            {
                256, 1
            },
            new ulong[]
            {
                512, 1
            },
            new ulong[]
            {
                partition.End - 256, 1
            },
            new ulong[]
            {
                partition.End, 1
            },
            new ulong[]
            {
                1024, 4
            },
            new ulong[]
            {
                2048, 4
            },
            new ulong[]
            {
                partition.End - 1024, 4
            },
            new ulong[]
            {
                partition.End - 4, 4
            }
        };

        bool anchorFound = false;
        uint ratio       = 1;
        sector = null;

        foreach(ulong[] position in from position in
                                        positions.Where(position =>
                                                            position[0] + partition.Start + position[1] <=
                                                            partition.End && position[0] < partition.End) let errno =
                                        imagePlugin.ReadSectors(position[0], (uint)position[1], out sector)
                                    where errno == ErrorNumber.NoError select position)
        {
            anchor = Marshal.ByteArrayToStructureLittleEndian<AnchorVolumeDescriptorPointer>(sector);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagIdentifier = {0}", anchor.tag.tagIdentifier);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.tag.descriptorVersion = {0}",
                                       anchor.tag.descriptorVersion);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagChecksum = 0x{0:X2}", anchor.tag.tagChecksum);
            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.tag.reserved = {0}", anchor.tag.reserved);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagSerialNumber = {0}", anchor.tag.tagSerialNumber);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.tag.descriptorCrc = 0x{0:X4}", anchor.tag.descriptorCrc);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.tag.descriptorCrcLength = {0}",
                                       anchor.tag.descriptorCrcLength);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagLocation = {0}", anchor.tag.tagLocation);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.mainVolumeDescriptorSequenceExtent.length = {0}",
                                       anchor.mainVolumeDescriptorSequenceExtent.length);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.mainVolumeDescriptorSequenceExtent.location = {0}",
                                       anchor.mainVolumeDescriptorSequenceExtent.location);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.reserveVolumeDescriptorSequenceExtent.length = {0}",
                                       anchor.reserveVolumeDescriptorSequenceExtent.length);

            AaruConsole.DebugWriteLine("UDF Plugin", "anchor.reserveVolumeDescriptorSequenceExtent.location = {0}",
                                       anchor.reserveVolumeDescriptorSequenceExtent.location);

            if(anchor.tag.tagIdentifier != TagIdentifier.AnchorVolumeDescriptorPointer ||
               anchor.tag.tagLocation != position[0] / position[1] ||
               (anchor.mainVolumeDescriptorSequenceExtent.location * position[1]) + partition.Start >= partition.End)
                continue;

            anchorFound = true;
            ratio       = (uint)position[1];

            break;
        }

        if(!anchorFound)
            return false;

        ulong count = 0;

        while(count < 256)
        {
            ErrorNumber errno =
                imagePlugin.
                    ReadSectors(partition.Start + (anchor.mainVolumeDescriptorSequenceExtent.location * ratio) + (count * ratio),
                                ratio, out sector);

            if(errno != ErrorNumber.NoError)
            {
                count++;

                continue;
            }

            var  tagId    = (TagIdentifier)BitConverter.ToUInt16(sector, 0);
            uint location = BitConverter.ToUInt32(sector, 0x0C);

            if(location == (partition.Start / ratio) + anchor.mainVolumeDescriptorSequenceExtent.location + count)
            {
                if(tagId == TagIdentifier.TerminatingDescriptor)
                    break;

                if(tagId == TagIdentifier.LogicalVolumeDescriptor)
                {
                    LogicalVolumeDescriptor lvd =
                        Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeDescriptor>(sector);

                    return _magic.SequenceEqual(lvd.domainIdentifier.identifier);
                }
            }
            else
                break;

            count++;
        }

        return false;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        information = "";
        ErrorNumber errno;

        // UDF is always UTF-8
        Encoding = Encoding.UTF8;
        byte[] sector;

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine("Universal Disk Format");

        var anchor = new AnchorVolumeDescriptorPointer();

        // All positions where anchor may reside, with the ratio between 512 and 2048bps
        ulong[][] positions =
        {
            new ulong[]
            {
                256, 1
            },
            new ulong[]
            {
                512, 1
            },
            new ulong[]
            {
                partition.End - 256, 1
            },
            new ulong[]
            {
                partition.End, 1
            },
            new ulong[]
            {
                1024, 4
            },
            new ulong[]
            {
                2048, 4
            },
            new ulong[]
            {
                partition.End - 1024, 4
            },
            new ulong[]
            {
                partition.End - 4, 4
            }
        };

        uint ratio = 1;

        foreach(ulong[] position in positions)
        {
            errno = imagePlugin.ReadSectors(position[0], (uint)position[1], out sector);

            if(errno != ErrorNumber.NoError)
                continue;

            anchor = Marshal.ByteArrayToStructureLittleEndian<AnchorVolumeDescriptorPointer>(sector);

            if(anchor.tag.tagIdentifier != TagIdentifier.AnchorVolumeDescriptorPointer ||
               anchor.tag.tagLocation != position[0] / position[1] ||
               anchor.mainVolumeDescriptorSequenceExtent.location + partition.Start >= partition.End)
                continue;

            ratio = (uint)position[1];

            break;
        }

        ulong count = 0;

        var pvd    = new PrimaryVolumeDescriptor();
        var lvd    = new LogicalVolumeDescriptor();
        var lvidiu = new LogicalVolumeIntegrityDescriptorImplementationUse();

        while(count < 256)
        {
            errno =
                imagePlugin.
                    ReadSectors(partition.Start + (anchor.mainVolumeDescriptorSequenceExtent.location * ratio) + (count * ratio),
                                ratio, out sector);

            if(errno != ErrorNumber.NoError)
                continue;

            var  tagId    = (TagIdentifier)BitConverter.ToUInt16(sector, 0);
            uint location = BitConverter.ToUInt32(sector, 0x0C);

            if(location == (partition.Start / ratio) + anchor.mainVolumeDescriptorSequenceExtent.location + count)
            {
                if(tagId == TagIdentifier.TerminatingDescriptor)
                    break;

                switch(tagId)
                {
                    case TagIdentifier.LogicalVolumeDescriptor:
                        lvd = Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeDescriptor>(sector);

                        break;
                    case TagIdentifier.PrimaryVolumeDescriptor:
                        pvd = Marshal.ByteArrayToStructureLittleEndian<PrimaryVolumeDescriptor>(sector);

                        break;
                }
            }
            else
                break;

            count++;
        }

        errno = imagePlugin.ReadSectors(lvd.integritySequenceExtent.location * ratio, ratio, out sector);

        if(errno != ErrorNumber.NoError)
            return;

        LogicalVolumeIntegrityDescriptor lvid =
            Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeIntegrityDescriptor>(sector);

        if(lvid.tag.tagIdentifier == TagIdentifier.LogicalVolumeIntegrityDescriptor &&
           lvid.tag.tagLocation   == lvd.integritySequenceExtent.location)
            lvidiu =
                Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeIntegrityDescriptorImplementationUse>(sector,
                    (int)((lvid.numberOfPartitions * 8) + 80),
                    System.Runtime.InteropServices.Marshal.SizeOf(lvidiu));
        else
            lvid = new LogicalVolumeIntegrityDescriptor();

        sbInformation.AppendFormat("Volume is number {0} of {1}", pvd.volumeSequenceNumber,
                                   pvd.maximumVolumeSequenceNumber).AppendLine();

        sbInformation.AppendFormat("Volume set identifier: {0}",
                                   StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier)).AppendLine();

        sbInformation.AppendFormat("Volume name: {0}", StringHandlers.DecompressUnicode(lvd.logicalVolumeIdentifier)).
                      AppendLine();

        sbInformation.AppendFormat("Volume uses {0} bytes per block", lvd.logicalBlockSize).AppendLine();

        sbInformation.AppendFormat("Volume was last written in {0}", EcmaToDateTime(lvid.recordingDateTime)).
                      AppendLine();

        sbInformation.AppendFormat("Volume contains {0} partitions", lvid.numberOfPartitions).AppendLine();

        sbInformation.AppendFormat("Volume contains {0} files and {1} directories", lvidiu.files, lvidiu.directories).
                      AppendLine();

        sbInformation.AppendFormat("Volume conforms to {0}",
                                   Encoding.GetString(lvd.domainIdentifier.identifier).TrimEnd('\u0000')).AppendLine();

        sbInformation.AppendFormat("Volume was last written by: {0}",
                                   Encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000')).
                      AppendLine();

        sbInformation.AppendFormat("Volume requires UDF version {0}.{1:X2} to be read",
                                   Convert.ToInt32($"{(lvidiu.minimumReadUDF & 0xFF00) >> 8}", 10),
                                   Convert.ToInt32($"{lvidiu.minimumReadUDF & 0xFF}", 10)).AppendLine();

        sbInformation.AppendFormat("Volume requires UDF version {0}.{1:X2} to be written to",
                                   Convert.ToInt32($"{(lvidiu.minimumWriteUDF & 0xFF00) >> 8}", 10),
                                   Convert.ToInt32($"{lvidiu.minimumWriteUDF & 0xFF}", 10)).AppendLine();

        sbInformation.AppendFormat("Volume cannot be written by any UDF version higher than {0}.{1:X2}",
                                   Convert.ToInt32($"{(lvidiu.maximumWriteUDF & 0xFF00) >> 8}", 10),
                                   Convert.ToInt32($"{lvidiu.maximumWriteUDF & 0xFF}", 10)).AppendLine();

        XmlFsType = new FileSystemType
        {
            Type = $"UDF v{Convert.ToInt32($"{(lvidiu.maximumWriteUDF & 0xFF00) >> 8}", 10)}.{
                Convert.ToInt32($"{lvidiu.maximumWriteUDF & 0xFF}", 10):X2}",
            ApplicationIdentifier     = Encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000'),
            ClusterSize               = lvd.logicalBlockSize,
            ModificationDate          = EcmaToDateTime(lvid.recordingDateTime),
            ModificationDateSpecified = true,
            Files                     = lvidiu.files,
            FilesSpecified            = true,
            VolumeName                = StringHandlers.DecompressUnicode(lvd.logicalVolumeIdentifier),
            VolumeSetIdentifier       = StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier),
            VolumeSerial              = StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier),
            SystemIdentifier          = Encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000')
        };

        XmlFsType.Clusters = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize /
                             XmlFsType.ClusterSize;

        information = sbInformation.ToString();
    }

    static DateTime EcmaToDateTime(Timestamp timestamp) => DateHandlers.EcmaToDateTime(timestamp.typeAndZone,
        timestamp.year, timestamp.month, timestamp.day, timestamp.hour, timestamp.minute, timestamp.second,
        timestamp.centiseconds, timestamp.hundredsMicroseconds, timestamp.microseconds);

    [Flags]
    enum EntityFlags : byte
    {
        Dirty = 0x01, Protected = 0x02
    }

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

    enum TagIdentifier : ushort
    {
        PrimaryVolumeDescriptor           = 1, AnchorVolumeDescriptorPointer = 2, VolumeDescriptorPointer          = 3,
        ImplementationUseVolumeDescriptor = 4, PartitionDescriptor           = 5, LogicalVolumeDescriptor          = 6,
        UnallocatedSpaceDescriptor        = 7, TerminatingDescriptor         = 8, LogicalVolumeIntegrityDescriptor = 9
    }

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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ExtentDescriptor
    {
        public readonly uint length;
        public readonly uint location;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct CharacterSpecification
    {
        public readonly byte type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
        public readonly byte[] information;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct AnchorVolumeDescriptorPointer
    {
        public readonly DescriptorTag    tag;
        public readonly ExtentDescriptor mainVolumeDescriptorSequenceExtent;
        public readonly ExtentDescriptor reserveVolumeDescriptorSequenceExtent;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)]
        public readonly byte[] reserved;
    }

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
}