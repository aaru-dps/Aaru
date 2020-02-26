// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using Schemas;
using Marshal = DiscImageChef.Helpers.Marshal;

namespace DiscImageChef.Filesystems
{
    // TODO: Detect bootable
    public class UDF : IFilesystem
    {
        readonly byte[] UDF_Magic =
        {
            0x2A, 0x4F, 0x53, 0x54, 0x41, 0x20, 0x55, 0x44, 0x46, 0x20, 0x43, 0x6F, 0x6D, 0x70, 0x6C, 0x69, 0x61,
            0x6E, 0x74, 0x00, 0x00, 0x00, 0x00
        };

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "Universal Disk Format";
        public Guid           Id        => new Guid("83976FEC-A91B-464B-9293-56C719461BAB");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            // UDF needs at least that
            if(partition.End - partition.Start < 256) return false;

            // UDF needs at least that
            if(imagePlugin.Info.SectorSize < 512) return false;

            byte[]                        sector;
            AnchorVolumeDescriptorPointer anchor = new AnchorVolumeDescriptorPointer();
            // All positions where anchor may reside
            ulong[] positions   = {256, 512, partition.End - 256, partition.End};
            bool    anchorFound = false;

            foreach(ulong position in positions.Where(position => position + partition.Start < partition.End))
            {
                sector = imagePlugin.ReadSector(position);
                anchor = Marshal.ByteArrayToStructureLittleEndian<AnchorVolumeDescriptorPointer>(sector);

                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagIdentifier = {0}", anchor.tag.tagIdentifier);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.descriptorVersion = {0}",
                                          anchor.tag.descriptorVersion);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagChecksum = 0x{0:X2}", anchor.tag.tagChecksum);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.reserved = {0}",         anchor.tag.reserved);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagSerialNumber = {0}",
                                          anchor.tag.tagSerialNumber);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.descriptorCrc = 0x{0:X4}",
                                          anchor.tag.descriptorCrc);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.descriptorCrcLength = {0}",
                                          anchor.tag.descriptorCrcLength);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagLocation = {0}", anchor.tag.tagLocation);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.mainVolumeDescriptorSequenceExtent.length = {0}",
                                          anchor.mainVolumeDescriptorSequenceExtent.length);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.mainVolumeDescriptorSequenceExtent.location = {0}",
                                          anchor.mainVolumeDescriptorSequenceExtent.location);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.reserveVolumeDescriptorSequenceExtent.length = {0}",
                                          anchor.reserveVolumeDescriptorSequenceExtent.length);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.reserveVolumeDescriptorSequenceExtent.location = {0}",
                                          anchor.reserveVolumeDescriptorSequenceExtent.location);

                if(anchor.tag.tagIdentifier !=
                   TagIdentifier.AnchorVolumeDescriptorPointer ||
                   anchor.tag.tagLocation !=
                   position ||
                   anchor.mainVolumeDescriptorSequenceExtent.location + partition.Start >= partition.End) continue;

                anchorFound = true;
                break;
            }

            if(!anchorFound) return false;

            ulong count = 0;

            while(count < 256)
            {
                sector = imagePlugin.ReadSector(partition.Start + anchor.mainVolumeDescriptorSequenceExtent.location +
                                                count);
                TagIdentifier tagId    = (TagIdentifier)BitConverter.ToUInt16(sector, 0);
                uint          location = BitConverter.ToUInt32(sector, 0x0C);

                if(location == partition.Start + anchor.mainVolumeDescriptorSequenceExtent.location + count)
                {
                    if(tagId == TagIdentifier.TerminatingDescriptor) break;

                    if(tagId == TagIdentifier.LogicalVolumeDescriptor)
                    {
                        LogicalVolumeDescriptor lvd =
                            Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeDescriptor>(sector);

                        return UDF_Magic.SequenceEqual(lvd.domainIdentifier.identifier);
                    }
                }
                else break;

                count++;
            }

            return false;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            // UDF is always UTF-8
            Encoding = Encoding.UTF8;
            byte[] sector;

            StringBuilder sbInformation = new StringBuilder();

            sbInformation.AppendLine("Universal Disk Format");

            AnchorVolumeDescriptorPointer anchor = new AnchorVolumeDescriptorPointer();
            // All positions where anchor may reside
            ulong[] positions = {256, 512, partition.End - 256, partition.End};

            foreach(ulong position in positions)
            {
                sector = imagePlugin.ReadSector(position);
                anchor = Marshal.ByteArrayToStructureLittleEndian<AnchorVolumeDescriptorPointer>(sector);

                if(anchor.tag.tagIdentifier ==
                   TagIdentifier.AnchorVolumeDescriptorPointer &&
                   anchor.tag.tagLocation ==
                   position &&
                   anchor.mainVolumeDescriptorSequenceExtent.location + partition.Start < partition.End) break;
            }

            ulong count = 0;

            PrimaryVolumeDescriptor          pvd  = new PrimaryVolumeDescriptor();
            LogicalVolumeDescriptor          lvd  = new LogicalVolumeDescriptor();
            LogicalVolumeIntegrityDescriptor lvid = new LogicalVolumeIntegrityDescriptor();
            LogicalVolumeIntegrityDescriptorImplementationUse lvidiu =
                new LogicalVolumeIntegrityDescriptorImplementationUse();

            while(count < 256)
            {
                sector = imagePlugin.ReadSector(partition.Start + anchor.mainVolumeDescriptorSequenceExtent.location +
                                                count);
                TagIdentifier tagId    = (TagIdentifier)BitConverter.ToUInt16(sector, 0);
                uint          location = BitConverter.ToUInt32(sector, 0x0C);

                if(location == partition.Start + anchor.mainVolumeDescriptorSequenceExtent.location + count)
                {
                    if(tagId == TagIdentifier.TerminatingDescriptor) break;

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
                else break;

                count++;
            }

            sector = imagePlugin.ReadSector(lvd.integritySequenceExtent.location);
            lvid   = Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeIntegrityDescriptor>(sector);

            if(lvid.tag.tagIdentifier == TagIdentifier.LogicalVolumeIntegrityDescriptor &&
               lvid.tag.tagLocation   == lvd.integritySequenceExtent.location)
                lvidiu =
                    Marshal.ByteArrayToStructureLittleEndian<LogicalVolumeIntegrityDescriptorImplementationUse>(sector,
                                                                                                                (int)
                                                                                                                (lvid
                                                                                                                    .numberOfPartitions *
                                                                                                                 8 +
                                                                                                                 80),
                                                                                                                System
                                                                                                                   .Runtime
                                                                                                                   .InteropServices
                                                                                                                   .Marshal
                                                                                                                   .SizeOf(lvidiu));
            else lvid = new LogicalVolumeIntegrityDescriptor();

            sbInformation.AppendFormat("Volume is number {0} of {1}", pvd.volumeSequenceNumber,
                                       pvd.maximumVolumeSequenceNumber).AppendLine();
            sbInformation.AppendFormat("Volume set identifier: {0}",
                                       StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier)).AppendLine();
            sbInformation
               .AppendFormat("Volume name: {0}", StringHandlers.DecompressUnicode(lvd.logicalVolumeIdentifier))
               .AppendLine();
            sbInformation.AppendFormat("Volume uses {0} bytes per block", lvd.logicalBlockSize).AppendLine();
            sbInformation.AppendFormat("Volume was las written in {0}", EcmaToDateTime(lvid.recordingDateTime))
                         .AppendLine();
            sbInformation.AppendFormat("Volume contains {0} partitions", lvid.numberOfPartitions).AppendLine();
            sbInformation
               .AppendFormat("Volume contains {0} files and {1} directories", lvidiu.files, lvidiu.directories)
               .AppendLine();
            sbInformation.AppendFormat("Volume conforms to {0}",
                                       Encoding.GetString(lvd.domainIdentifier.identifier).TrimEnd('\u0000'))
                         .AppendLine();
            sbInformation.AppendFormat("Volume was last written by: {0}",
                                       Encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000'))
                         .AppendLine();
            sbInformation.AppendFormat("Volume requires UDF version {0}.{1:X2} to be read",
                                       Convert.ToInt32($"{(lvidiu.minimumReadUDF & 0xFF00) >> 8}", 10),
                                       Convert.ToInt32($"{lvidiu.minimumReadUDF & 0xFF}",          10)).AppendLine();
            sbInformation.AppendFormat("Volume requires UDF version {0}.{1:X2} to be written to",
                                       Convert.ToInt32($"{(lvidiu.minimumWriteUDF & 0xFF00) >> 8}", 10),
                                       Convert.ToInt32($"{lvidiu.minimumWriteUDF & 0xFF}",          10)).AppendLine();
            sbInformation.AppendFormat("Volume cannot be written by any UDF version higher than {0}.{1:X2}",
                                       Convert.ToInt32($"{(lvidiu.maximumWriteUDF & 0xFF00) >> 8}", 10),
                                       Convert.ToInt32($"{lvidiu.maximumWriteUDF & 0xFF}",          10)).AppendLine();

            XmlFsType = new FileSystemType
            {
                Type =
                    $"UDF v{Convert.ToInt32($"{(lvidiu.maximumWriteUDF & 0xFF00) >> 8}", 10)}.{Convert.ToInt32($"{lvidiu.maximumWriteUDF & 0xFF}", 10):X2}",
                ApplicationIdentifier =
                    Encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000'),
                ClusterSize               = lvd.logicalBlockSize,
                ModificationDate          = EcmaToDateTime(lvid.recordingDateTime),
                ModificationDateSpecified = true,
                Files                     = lvidiu.files,
                FilesSpecified            = true,
                VolumeName                = StringHandlers.DecompressUnicode(lvd.logicalVolumeIdentifier),
                VolumeSetIdentifier       = StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier),
                SystemIdentifier =
                    Encoding.GetString(pvd.implementationIdentifier.identifier).TrimEnd('\u0000')
            };
            XmlFsType.Clusters = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize /
                                 XmlFsType.ClusterSize;

            information = sbInformation.ToString();
        }

        static DateTime EcmaToDateTime(Timestamp timestamp) =>
            DateHandlers.EcmaToDateTime(timestamp.typeAndZone, timestamp.year, timestamp.month, timestamp.day,
                                        timestamp.hour, timestamp.minute, timestamp.second, timestamp.centiseconds,
                                        timestamp.hundredsMicroseconds, timestamp.microseconds);

        [Flags]
        enum EntityFlags : byte
        {
            Dirty     = 0x01,
            Protected = 0x02
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct EntityIdentifier
        {
            /// <summary>
            ///     Entity flags
            /// </summary>
            public readonly EntityFlags flags;
            /// <summary>
            ///     Structure identifier
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)]
            public readonly byte[] identifier;
            /// <summary>
            ///     Structure data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] identifierSuffix;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Timestamp
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DescriptorTag
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
        struct ExtentDescriptor
        {
            public readonly uint length;
            public readonly uint location;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CharacterSpecification
        {
            public readonly byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
            public readonly byte[] information;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AnchorVolumeDescriptorPointer
        {
            public readonly DescriptorTag    tag;
            public readonly ExtentDescriptor mainVolumeDescriptorSequenceExtent;
            public readonly ExtentDescriptor reserveVolumeDescriptorSequenceExtent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)]
            public readonly byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PrimaryVolumeDescriptor
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
        struct LogicalVolumeDescriptor
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
        struct LogicalVolumeIntegrityDescriptor
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
        struct LogicalVolumeIntegrityDescriptorImplementationUse
        {
            public readonly EntityIdentifier implementationId;
            public readonly uint             files;
            public readonly uint             directories;
            public readonly ushort           minimumReadUDF;
            public readonly ushort           minimumWriteUDF;
            public readonly ushort           maximumWriteUDF;
        }
    }
}