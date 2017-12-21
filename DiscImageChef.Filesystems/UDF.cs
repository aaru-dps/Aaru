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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Filesystems
{
    // TODO: Detect bootable
    public class UDF : Filesystem
    {
        public UDF()
        {
            Name = "Universal Disk Format";
            PluginUUID = new Guid("83976FEC-A91B-464B-9293-56C719461BAB");
            CurrentEncoding = Encoding.UTF8;
        }

        public UDF(Encoding encoding)
        {
            Name = "Universal Disk Format";
            PluginUUID = new Guid("83976FEC-A91B-464B-9293-56C719461BAB");
            // UDF is always UTF-8
            CurrentEncoding = Encoding.UTF8;
        }

        public UDF(ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Universal Disk Format";
            PluginUUID = new Guid("83976FEC-A91B-464B-9293-56C719461BAB");
            // UDF is always UTF-8
            CurrentEncoding = Encoding.UTF8;
        }

        readonly byte[] UDF_Magic =
        {
            0x2A, 0x4F, 0x53, 0x54, 0x41, 0x20, 0x55, 0x44, 0x46, 0x20, 0x43, 0x6F, 0x6D, 0x70, 0x6C, 0x69, 0x61, 0x6E,
            0x74, 0x00, 0x00, 0x00, 0x00
        };

        [Flags]
        enum EntityFlags : byte
        {
            Dirty = 0x01,
            Protected = 0x02
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct EntityIdentifier
        {
            /// <summary>
            /// Entity flags
            /// </summary>
            public EntityFlags flags;
            /// <summary>
            /// Structure identifier
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 23)] public byte[] identifier;
            /// <summary>
            /// Structure data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] identifierSuffix;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Timestamp
        {
            public ushort typeAndZone;
            public short year;
            public byte month;
            public byte day;
            public byte hour;
            public byte minute;
            public byte second;
            public byte centiseconds;
            public byte hundredsMicroseconds;
            public byte microseconds;
        }

        enum TagIdentifier : ushort
        {
            PrimaryVolumeDescriptor = 1,
            AnchorVolumeDescriptorPointer = 2,
            VolumeDescriptorPointer = 3,
            ImplementationUseVolumeDescriptor = 4,
            PartitionDescriptor = 5,
            LogicalVolumeDescriptor = 6,
            UnallocatedSpaceDescriptor = 7,
            TerminatingDescriptor = 8,
            LogicalVolumeIntegrityDescriptor = 9
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DescriptorTag
        {
            public TagIdentifier tagIdentifier;
            public ushort descriptorVersion;
            public byte tagChecksum;
            public byte reserved;
            public ushort tagSerialNumber;
            public ushort descriptorCrc;
            public ushort descriptorCrcLength;
            public uint tagLocation;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ExtentDescriptor
        {
            public uint length;
            public uint location;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CharacterSpecification
        {
            public byte type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)] public byte[] information;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AnchorVolumeDescriptorPointer
        {
            public DescriptorTag tag;
            public ExtentDescriptor mainVolumeDescriptorSequenceExtent;
            public ExtentDescriptor reserveVolumeDescriptorSequenceExtent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)] public byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PrimaryVolumeDescriptor
        {
            public DescriptorTag tag;
            public uint volumeDescriptorSequenceNumber;
            public uint primaryVolumeDescriptorNumber;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] volumeIdentifier;
            public ushort volumeSequenceNumber;
            public ushort maximumVolumeSequenceNumber;
            public ushort interchangeLevel;
            public ushort maximumInterchangeLevel;
            public uint characterSetList;
            public uint maximumCharacterSetList;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] volumeSetIdentifier;
            public CharacterSpecification descriptorCharacterSet;
            public CharacterSpecification explanatoryCharacterSet;
            public ExtentDescriptor volumeAbstract;
            public ExtentDescriptor volumeCopyright;
            public EntityIdentifier applicationIdentifier;
            public Timestamp recordingDateTime;
            public EntityIdentifier implementationIdentifier;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] implementationUse;
            public uint predecessorVolumeDescriptorSequenceLocation;
            public ushort flags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)] public byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct LogicalVolumeDescriptor
        {
            public DescriptorTag tag;
            public uint volumeDescriptorSequenceNumber;
            public CharacterSpecification descriptorCharacterSet;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] logicalVolumeIdentifier;
            public uint logicalBlockSize;
            public EntityIdentifier domainIdentifier;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] logicalVolumeContentsUse;
            public uint mapTableLength;
            public uint numberOfPartitionMaps;
            public EntityIdentifier implementationIdentifier;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] implementationUse;
            public ExtentDescriptor integritySequenceExtent;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct LogicalVolumeIntegrityDescriptor
        {
            public DescriptorTag tag;
            public Timestamp recordingDateTime;
            public uint integrityType;
            public ExtentDescriptor nextIntegrityExtent;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] logicalVolumeContentsUse;
            public uint numberOfPartitions;
            public uint lengthOfImplementationUse;
            // Follows uint[numberOfPartitions] freeSpaceTable;
            // Follows uint[numberOfPartitions] sizeTable;
            // Follows byte[lengthOfImplementationUse] implementationUse;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct LogicalVolumeIntegrityDescriptorImplementationUse
        {
            public EntityIdentifier implementationId;
            public uint files;
            public uint directories;
            public ushort minimumReadUDF;
            public ushort minimumWriteUDF;
            public ushort maximumWriteUDF;
        }

        public override bool Identify(DiscImages.ImagePlugin imagePlugin, Partition partition)
        {
            // UDF needs at least that
            if(partition.End - partition.Start < 256) return false;

            // UDF needs at least that
            if(imagePlugin.ImageInfo.SectorSize < 512) return false;

            byte[] sector;
            AnchorVolumeDescriptorPointer anchor = new AnchorVolumeDescriptorPointer();
            // All positions where anchor may reside
            ulong[] positions = {256, 512, partition.End - 256, partition.End};
            bool anchorFound = false;

            foreach(ulong position in positions)
            {
                if(position + partition.Start >= partition.End) continue;

                sector = imagePlugin.ReadSector(position);
                anchor = new AnchorVolumeDescriptorPointer();
                IntPtr anchorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(anchor));
                Marshal.Copy(sector, 0, anchorPtr, Marshal.SizeOf(anchor));
                anchor =
                    (AnchorVolumeDescriptorPointer)Marshal.PtrToStructure(anchorPtr,
                                                                          typeof(AnchorVolumeDescriptorPointer));
                Marshal.FreeHGlobal(anchorPtr);

                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagIdentifier = {0}", anchor.tag.tagIdentifier);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.descriptorVersion = {0}",
                                          anchor.tag.descriptorVersion);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagChecksum = 0x{0:X2}", anchor.tag.tagChecksum);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.reserved = {0}", anchor.tag.reserved);
                DicConsole.DebugWriteLine("UDF Plugin", "anchor.tag.tagSerialNumber = {0}", anchor.tag.tagSerialNumber);
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

                if(anchor.tag.tagIdentifier != TagIdentifier.AnchorVolumeDescriptorPointer ||
                   anchor.tag.tagLocation != position ||
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
                TagIdentifier tagId = (TagIdentifier)BitConverter.ToUInt16(sector, 0);
                uint location = BitConverter.ToUInt32(sector, 0x0C);

                if(location == partition.Start + anchor.mainVolumeDescriptorSequenceExtent.location + count)
                {
                    if(tagId == TagIdentifier.TerminatingDescriptor) break;

                    if(tagId == TagIdentifier.LogicalVolumeDescriptor)
                    {
                        LogicalVolumeDescriptor lvd = new LogicalVolumeDescriptor();
                        IntPtr lvdPtr = Marshal.AllocHGlobal(Marshal.SizeOf(lvd));
                        Marshal.Copy(sector, 0, lvdPtr, Marshal.SizeOf(lvd));
                        lvd = (LogicalVolumeDescriptor)Marshal.PtrToStructure(lvdPtr, typeof(LogicalVolumeDescriptor));
                        Marshal.FreeHGlobal(lvdPtr);

                        return UDF_Magic.SequenceEqual(lvd.domainIdentifier.identifier);
                    }
                }
                else break;

                count++;
            }

            return false;
        }

        public override void GetInformation(DiscImages.ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            byte[] sector;

            StringBuilder sbInformation = new StringBuilder();

            sbInformation.AppendLine("Universal Disk Format");

            AnchorVolumeDescriptorPointer anchor = new AnchorVolumeDescriptorPointer();
            // All positions where anchor may reside
            ulong[] positions = {256, 512, partition.End - 256, partition.End};

            foreach(ulong position in positions)
            {
                sector = imagePlugin.ReadSector(position);
                anchor = new AnchorVolumeDescriptorPointer();
                IntPtr anchorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(anchor));
                Marshal.Copy(sector, 0, anchorPtr, Marshal.SizeOf(anchor));
                anchor =
                    (AnchorVolumeDescriptorPointer)Marshal.PtrToStructure(anchorPtr,
                                                                          typeof(AnchorVolumeDescriptorPointer));
                Marshal.FreeHGlobal(anchorPtr);

                if(anchor.tag.tagIdentifier == TagIdentifier.AnchorVolumeDescriptorPointer &&
                   anchor.tag.tagLocation == position &&
                   anchor.mainVolumeDescriptorSequenceExtent.location + partition.Start < partition.End) break;
            }

            ulong count = 0;

            PrimaryVolumeDescriptor pvd = new PrimaryVolumeDescriptor();
            LogicalVolumeDescriptor lvd = new LogicalVolumeDescriptor();
            LogicalVolumeIntegrityDescriptor lvid = new LogicalVolumeIntegrityDescriptor();
            LogicalVolumeIntegrityDescriptorImplementationUse lvidiu =
                new LogicalVolumeIntegrityDescriptorImplementationUse();

            while(count < 256)
            {
                sector = imagePlugin.ReadSector(partition.Start + anchor.mainVolumeDescriptorSequenceExtent.location +
                                                count);
                TagIdentifier tagId = (TagIdentifier)BitConverter.ToUInt16(sector, 0);
                uint location = BitConverter.ToUInt32(sector, 0x0C);

                if(location == partition.Start + anchor.mainVolumeDescriptorSequenceExtent.location + count)
                {
                    if(tagId == TagIdentifier.TerminatingDescriptor) break;

                    switch(tagId) {
                        case TagIdentifier.LogicalVolumeDescriptor:
                            IntPtr lvdPtr = Marshal.AllocHGlobal(Marshal.SizeOf(lvd));
                            Marshal.Copy(sector, 0, lvdPtr, Marshal.SizeOf(lvd));
                            lvd = (LogicalVolumeDescriptor)Marshal.PtrToStructure(lvdPtr, typeof(LogicalVolumeDescriptor));
                            Marshal.FreeHGlobal(lvdPtr);
                            break;
                        case TagIdentifier.PrimaryVolumeDescriptor:
                            IntPtr pvdPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pvd));
                            Marshal.Copy(sector, 0, pvdPtr, Marshal.SizeOf(pvd));
                            pvd = (PrimaryVolumeDescriptor)Marshal.PtrToStructure(pvdPtr, typeof(PrimaryVolumeDescriptor));
                            Marshal.FreeHGlobal(pvdPtr);
                            break;
                    }
                }
                else break;

                count++;
            }

            sector = imagePlugin.ReadSector(lvd.integritySequenceExtent.location);
            IntPtr lvidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(lvid));
            Marshal.Copy(sector, 0, lvidPtr, Marshal.SizeOf(lvid));
            lvid =
                (LogicalVolumeIntegrityDescriptor)
                Marshal.PtrToStructure(lvidPtr, typeof(LogicalVolumeIntegrityDescriptor));
            Marshal.FreeHGlobal(lvidPtr);

            if(lvid.tag.tagIdentifier == TagIdentifier.LogicalVolumeIntegrityDescriptor &&
               lvid.tag.tagLocation == lvd.integritySequenceExtent.location)
            {
                IntPtr lvidiuPtr = Marshal.AllocHGlobal(Marshal.SizeOf(lvidiu));
                Marshal.Copy(sector, (int)(lvid.numberOfPartitions * 8 + 80), lvidiuPtr, Marshal.SizeOf(lvidiu));
                lvidiu =
                    (LogicalVolumeIntegrityDescriptorImplementationUse)Marshal.PtrToStructure(lvidiuPtr,
                                                                                              typeof(
                                                                                                  LogicalVolumeIntegrityDescriptorImplementationUse
                                                                                              ));
                Marshal.FreeHGlobal(lvidiuPtr);
            }
            else lvid = new LogicalVolumeIntegrityDescriptor();

            sbInformation.AppendFormat("Volume is number {0} of {1}", pvd.volumeSequenceNumber,
                                       pvd.maximumVolumeSequenceNumber).AppendLine();
            sbInformation.AppendFormat("Volume set identifier: {0}",
                                       StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier)).AppendLine();
            sbInformation
                .AppendFormat("Volume name: {0}", StringHandlers.DecompressUnicode(lvd.logicalVolumeIdentifier))
                .AppendLine();
            sbInformation.AppendFormat("Volume uses {0} bytes per block", lvd.logicalBlockSize).AppendLine();
            sbInformation.AppendFormat("Volume was las written in {0}", ECMAToDateTime(lvid.recordingDateTime))
                         .AppendLine();
            sbInformation.AppendFormat("Volume contains {0} partitions", lvid.numberOfPartitions).AppendLine();
            sbInformation
                .AppendFormat("Volume contains {0} files and {1} directories", lvidiu.files, lvidiu.directories)
                .AppendLine();
            sbInformation.AppendFormat("Volume conforms to {0}",
                                       CurrentEncoding
                                           .GetString(lvd.domainIdentifier.identifier).TrimEnd(new char[] {'\u0000'}))
                         .AppendLine();
            sbInformation.AppendFormat("Volume was last written by: {0}",
                                       CurrentEncoding
                                           .GetString(pvd.implementationIdentifier.identifier)
                                           .TrimEnd(new char[] {'\u0000'})).AppendLine();
            sbInformation.AppendFormat("Volume requires UDF version {0}.{1:X2} to be read",
                                       Convert.ToInt32(string.Format("{0}", (lvidiu.minimumReadUDF & 0xFF00) >> 8), 10),
                                       Convert.ToInt32(string.Format("{0}", lvidiu.minimumReadUDF & 0xFF), 10))
                         .AppendLine();
            sbInformation.AppendFormat("Volume requires UDF version {0}.{1:X2} to be written to",
                                       Convert.ToInt32(string.Format("{0}", (lvidiu.minimumWriteUDF & 0xFF00) >> 8),
                                                       10),
                                       Convert.ToInt32(string.Format("{0}", lvidiu.minimumWriteUDF & 0xFF), 10))
                         .AppendLine();
            sbInformation.AppendFormat("Volume cannot be written by any UDF version higher than {0}.{1:X2}",
                                       Convert.ToInt32(string.Format("{0}", (lvidiu.maximumWriteUDF & 0xFF00) >> 8),
                                                       10),
                                       Convert.ToInt32(string.Format("{0}", lvidiu.maximumWriteUDF & 0xFF), 10))
                         .AppendLine();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = string.Format("UDF v{0}.{1:X2}",
                                           Convert.ToInt32(string.Format("{0}", (lvidiu.maximumWriteUDF & 0xFF00) >> 8),
                                                           10),
                                           Convert.ToInt32(string.Format("{0}", lvidiu.maximumWriteUDF & 0xFF), 10));
            xmlFSType.ApplicationIdentifier = CurrentEncoding
                .GetString(pvd.implementationIdentifier.identifier).TrimEnd(new char[] {'\u0000'});
            xmlFSType.ClusterSize = (int)lvd.logicalBlockSize;
            xmlFSType.Clusters = (long)((partition.End - partition.Start + 1) * imagePlugin.ImageInfo.SectorSize /
                                        (ulong)xmlFSType.ClusterSize);
            xmlFSType.ModificationDate = ECMAToDateTime(lvid.recordingDateTime);
            xmlFSType.ModificationDateSpecified = true;
            xmlFSType.Files = lvidiu.files;
            xmlFSType.FilesSpecified = true;
            xmlFSType.VolumeName = StringHandlers.DecompressUnicode(lvd.logicalVolumeIdentifier);
            xmlFSType.VolumeSetIdentifier = StringHandlers.DecompressUnicode(pvd.volumeSetIdentifier);
            xmlFSType.SystemIdentifier = CurrentEncoding
                .GetString(pvd.implementationIdentifier.identifier).TrimEnd(new char[] {'\u0000'});

            information = sbInformation.ToString();
        }

        static DateTime ECMAToDateTime(Timestamp timestamp)
        {
            return DateHandlers.ECMAToDateTime(timestamp.typeAndZone, timestamp.year, timestamp.month, timestamp.day,
                                               timestamp.hour, timestamp.minute, timestamp.second,
                                               timestamp.centiseconds, timestamp.hundredsMicroseconds,
                                               timestamp.microseconds);
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}