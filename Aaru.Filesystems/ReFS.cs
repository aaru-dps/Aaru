// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ReFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Resilient File System plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Resilient File System and shows information.
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
    public class ReFS : IFilesystem
    {
        const    uint           FSRS          = 0x53525346;
        readonly byte[]         refsSignature = {0x52, 0x65, 0x46, 0x53, 0x00, 0x00, 0x00, 0x00};
        public   string         Name      => "Resilient File System plugin";
        public   Guid           Id        => new Guid("37766C4E-EBF5-4113-A712-B758B756ABD6");
        public   FileSystemType XmlFsType { get; private set; }
        public   Encoding       Encoding  { get; private set; }
        public   string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            uint sbSize = (uint)(Marshal.SizeOf<RefsVolumeHeader>() / imagePlugin.Info.SectorSize);
            if(Marshal.SizeOf<RefsVolumeHeader>() % imagePlugin.Info.SectorSize != 0) sbSize++;

            if(partition.Start + sbSize >= partition.End) return false;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);
            if(sector.Length < Marshal.SizeOf<RefsVolumeHeader>()) return false;

            RefsVolumeHeader refsVhdr = Marshal.ByteArrayToStructureLittleEndian<RefsVolumeHeader>(sector);

            return refsVhdr.identifier == FSRS && ArrayHelpers.ArrayIsNullOrEmpty(refsVhdr.mustBeZero) &&
                   refsVhdr.signature.SequenceEqual(refsSignature);
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = Encoding.UTF8;
            information = "";

            uint sbSize = (uint)(Marshal.SizeOf<RefsVolumeHeader>() / imagePlugin.Info.SectorSize);
            if(Marshal.SizeOf<RefsVolumeHeader>() % imagePlugin.Info.SectorSize != 0) sbSize++;

            if(partition.Start + sbSize >= partition.End) return;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);
            if(sector.Length < Marshal.SizeOf<RefsVolumeHeader>()) return;

            RefsVolumeHeader refsVhdr = Marshal.ByteArrayToStructureLittleEndian<RefsVolumeHeader>(sector);

            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.jump empty? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(refsVhdr.jump));
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.signature = {0}",
                                      StringHandlers.CToString(refsVhdr.signature));
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.mustBeZero empty? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(refsVhdr.mustBeZero));
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.identifier = {0}",
                                      StringHandlers.CToString(BitConverter.GetBytes(refsVhdr.identifier)));
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.length = {0}",         refsVhdr.length);
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.checksum = 0x{0:X4}",  refsVhdr.checksum);
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.sectors = {0}",        refsVhdr.sectors);
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.bytesPerSector = {0}", refsVhdr.bytesPerSector);
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.sectorsPerCluster = {0}",
                                      refsVhdr.sectorsPerCluster);
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.unknown1 zero? = {0}", refsVhdr.unknown1 == 0);
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.unknown2 zero? = {0}", refsVhdr.unknown2 == 0);
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.unknown3 zero? = {0}", refsVhdr.unknown3 == 0);
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.unknown4 zero? = {0}", refsVhdr.unknown4 == 0);
            DicConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.unknown5 empty? = {0}",
                                      ArrayHelpers.ArrayIsNullOrEmpty(refsVhdr.unknown5));

            if(refsVhdr.identifier != FSRS || !ArrayHelpers.ArrayIsNullOrEmpty(refsVhdr.mustBeZero) ||
               !refsVhdr.signature.SequenceEqual(refsSignature)) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Microsoft Resilient File System");
            sb.AppendFormat("Volume uses {0} bytes per sector", refsVhdr.bytesPerSector).AppendLine();
            sb.AppendFormat("Volume uses {0} sectors per cluster ({1} bytes)", refsVhdr.sectorsPerCluster,
                            refsVhdr.sectorsPerCluster * refsVhdr.bytesPerSector).AppendLine();
            sb.AppendFormat("Volume has {0} sectors ({1} bytes)", refsVhdr.sectors,
                            refsVhdr.sectors * refsVhdr.bytesPerSector).AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type        = "Resilient File System",
                ClusterSize = refsVhdr.bytesPerSector * refsVhdr.sectorsPerCluster,
                Clusters    = refsVhdr.sectors        / refsVhdr.sectorsPerCluster
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct RefsVolumeHeader
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public readonly byte[] jump;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] signature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public readonly byte[] mustBeZero;
            public readonly uint   identifier;
            public readonly ushort length;
            public readonly ushort checksum;
            public readonly ulong  sectors;
            public readonly uint   bytesPerSector;
            public readonly uint   sectorsPerCluster;
            public readonly uint   unknown1;
            public readonly uint   unknown2;
            public readonly ulong  unknown3;
            public readonly ulong  unknown4;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15872)]
            public readonly byte[] unknown5;
        }
    }
}