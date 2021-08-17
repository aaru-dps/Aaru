// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    /// <inheritdoc />
    /// <summary>
    /// Implements detection of Microsoft's Resilient filesystem (ReFS)
    /// </summary>
    public sealed class ReFS : IFilesystem
    {
        const uint FSRS = 0x53525346;
        readonly byte[] _signature =
        {
            0x52, 0x65, 0x46, 0x53, 0x00, 0x00, 0x00, 0x00
        };
        /// <inheritdoc />
        public string         Name      => "Resilient File System plugin";
        /// <inheritdoc />
        public Guid           Id        => new Guid("37766C4E-EBF5-4113-A712-B758B756ABD6");
        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding       Encoding  { get; private set; }
        /// <inheritdoc />
        public string         Author    => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            uint sbSize = (uint)(Marshal.SizeOf<VolumeHeader>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<VolumeHeader>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            if(partition.Start + sbSize >= partition.End)
                return false;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);

            if(sector.Length < Marshal.SizeOf<VolumeHeader>())
                return false;

            VolumeHeader vhdr = Marshal.ByteArrayToStructureLittleEndian<VolumeHeader>(sector);

            return vhdr.identifier == FSRS && ArrayHelpers.ArrayIsNullOrEmpty(vhdr.mustBeZero) &&
                   vhdr.signature.SequenceEqual(_signature);
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = Encoding.UTF8;
            information = "";

            uint sbSize = (uint)(Marshal.SizeOf<VolumeHeader>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<VolumeHeader>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            if(partition.Start + sbSize >= partition.End)
                return;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);

            if(sector.Length < Marshal.SizeOf<VolumeHeader>())
                return;

            VolumeHeader vhdr = Marshal.ByteArrayToStructureLittleEndian<VolumeHeader>(sector);

            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.jump empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(vhdr.jump));

            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.signature = {0}",
                                       StringHandlers.CToString(vhdr.signature));

            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.mustBeZero empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(vhdr.mustBeZero));

            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.identifier = {0}",
                                       StringHandlers.CToString(BitConverter.GetBytes(vhdr.identifier)));

            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.length = {0}", vhdr.length);
            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.checksum = 0x{0:X4}", vhdr.checksum);
            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.sectors = {0}", vhdr.sectors);
            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.bytesPerSector = {0}", vhdr.bytesPerSector);

            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.sectorsPerCluster = {0}", vhdr.sectorsPerCluster);

            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.unknown1 zero? = {0}", vhdr.unknown1 == 0);
            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.unknown2 zero? = {0}", vhdr.unknown2 == 0);
            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.unknown3 zero? = {0}", vhdr.unknown3 == 0);
            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.unknown4 zero? = {0}", vhdr.unknown4 == 0);

            AaruConsole.DebugWriteLine("ReFS plugin", "VolumeHeader.unknown5 empty? = {0}",
                                       ArrayHelpers.ArrayIsNullOrEmpty(vhdr.unknown5));

            if(vhdr.identifier != FSRS                           ||
               !ArrayHelpers.ArrayIsNullOrEmpty(vhdr.mustBeZero) ||
               !vhdr.signature.SequenceEqual(_signature))
                return;

            var sb = new StringBuilder();

            sb.AppendLine("Microsoft Resilient File System");
            sb.AppendFormat("Volume uses {0} bytes per sector", vhdr.bytesPerSector).AppendLine();

            sb.AppendFormat("Volume uses {0} sectors per cluster ({1} bytes)", vhdr.sectorsPerCluster,
                            vhdr.sectorsPerCluster * vhdr.bytesPerSector).AppendLine();

            sb.AppendFormat("Volume has {0} sectors ({1} bytes)", vhdr.sectors, vhdr.sectors * vhdr.bytesPerSector).
               AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type        = "Resilient File System",
                ClusterSize = vhdr.bytesPerSector * vhdr.sectorsPerCluster,
                Clusters    = vhdr.sectors        / vhdr.sectorsPerCluster
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct VolumeHeader
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