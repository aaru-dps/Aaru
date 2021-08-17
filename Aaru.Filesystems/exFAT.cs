// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : exFAT.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft exFAT filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Microsoft exFAT filesystem and shows information.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    // Information from https://www.sans.org/reading-room/whitepapers/forensics/reverse-engineering-microsoft-exfat-file-system-33274
    /// <summary>
    /// Implements detection of the exFAT filesystem
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed class exFAT : IFilesystem
    {
        readonly Guid _oemFlashParameterGuid = new Guid("0A0C7E46-3399-4021-90C8-FA6D389C4BA2");

        readonly byte[] _signature =
        {
            0x45, 0x58, 0x46, 0x41, 0x54, 0x20, 0x20, 0x20
        };

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding       Encoding  { get; private set; }
        /// <inheritdoc />
        public string         Name      => "Microsoft Extended File Allocation Table";
        /// <inheritdoc />
        public Guid           Id        => new Guid("8271D088-1533-4CB3-AC28-D802B68BB95C");
        /// <inheritdoc />
        public string         Author    => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(12 + partition.Start >= partition.End)
                return false;

            byte[] vbrSector = imagePlugin.ReadSector(0 + partition.Start);

            if(vbrSector.Length < 512)
                return false;

            VolumeBootRecord vbr = Marshal.ByteArrayToStructureLittleEndian<VolumeBootRecord>(vbrSector);

            return _signature.SequenceEqual(vbr.signature);
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            var sb = new StringBuilder();
            XmlFsType = new FileSystemType();

            byte[]           vbrSector = imagePlugin.ReadSector(0 + partition.Start);
            VolumeBootRecord vbr       = Marshal.ByteArrayToStructureLittleEndian<VolumeBootRecord>(vbrSector);

            byte[] parametersSector = imagePlugin.ReadSector(9 + partition.Start);

            OemParameterTable parametersTable =
                Marshal.ByteArrayToStructureLittleEndian<OemParameterTable>(parametersSector);

            byte[]         chkSector = imagePlugin.ReadSector(11 + partition.Start);
            ChecksumSector chksector = Marshal.ByteArrayToStructureLittleEndian<ChecksumSector>(chkSector);

            sb.AppendLine("Microsoft exFAT");
            sb.AppendFormat("Partition offset: {0}", vbr.offset).AppendLine();

            sb.AppendFormat("Volume has {0} sectors of {1} bytes each for a total of {2} bytes", vbr.sectors,
                            1 << vbr.sectorShift, vbr.sectors * (ulong)(1 << vbr.sectorShift)).AppendLine();

            sb.AppendFormat("Volume uses clusters of {0} sectors ({1} bytes) each", 1 << vbr.clusterShift,
                            (1 << vbr.sectorShift) * (1 << vbr.clusterShift)).AppendLine();

            sb.AppendFormat("First FAT starts at sector {0} and runs for {1} sectors", vbr.fatOffset, vbr.fatLength).
               AppendLine();

            sb.AppendFormat("Volume uses {0} FATs", vbr.fats).AppendLine();

            sb.AppendFormat("Cluster heap starts at sector {0}, contains {1} clusters and is {2}% used",
                            vbr.clusterHeapOffset, vbr.clusterHeapLength, vbr.heapUsage).AppendLine();

            sb.AppendFormat("Root directory starts at cluster {0}", vbr.rootDirectoryCluster).AppendLine();

            sb.AppendFormat("Filesystem revision is {0}.{1:D2}", (vbr.revision & 0xFF00) >> 8, vbr.revision & 0xFF).
               AppendLine();

            sb.AppendFormat("Volume serial number: {0:X8}", vbr.volumeSerial).AppendLine();
            sb.AppendFormat("BIOS drive is {0:X2}h", vbr.drive).AppendLine();

            if(vbr.flags.HasFlag(VolumeFlags.SecondFatActive))
                sb.AppendLine("2nd FAT is in use");

            if(vbr.flags.HasFlag(VolumeFlags.VolumeDirty))
                sb.AppendLine("Volume is dirty");

            if(vbr.flags.HasFlag(VolumeFlags.MediaFailure))
                sb.AppendLine("Underlying media presented errors");

            int count = 1;

            foreach(OemParameter parameter in parametersTable.parameters)
            {
                if(parameter.OemParameterType == _oemFlashParameterGuid)
                {
                    sb.AppendFormat("OEM Parameters {0}:", count).AppendLine();
                    sb.AppendFormat("\t{0} bytes in erase block", parameter.eraseBlockSize).AppendLine();
                    sb.AppendFormat("\t{0} bytes per page", parameter.pageSize).AppendLine();
                    sb.AppendFormat("\t{0} spare blocks", parameter.spareBlocks).AppendLine();
                    sb.AppendFormat("\t{0} nanoseconds random access time", parameter.randomAccessTime).AppendLine();
                    sb.AppendFormat("\t{0} nanoseconds program time", parameter.programTime).AppendLine();
                    sb.AppendFormat("\t{0} nanoseconds read cycle time", parameter.readCycleTime).AppendLine();
                    sb.AppendFormat("\t{0} nanoseconds write cycle time", parameter.writeCycleTime).AppendLine();
                }
                else if(parameter.OemParameterType != Guid.Empty)
                    sb.AppendFormat("Found unknown parameter type {0}", parameter.OemParameterType).AppendLine();

                count++;
            }

            sb.AppendFormat("Checksum 0x{0:X8}", chksector.checksum[0]).AppendLine();

            XmlFsType.ClusterSize  = (uint)((1 << vbr.sectorShift) * (1 << vbr.clusterShift));
            XmlFsType.Clusters     = vbr.clusterHeapLength;
            XmlFsType.Dirty        = vbr.flags.HasFlag(VolumeFlags.VolumeDirty);
            XmlFsType.Type         = "exFAT";
            XmlFsType.VolumeSerial = $"{vbr.volumeSerial:X8}";

            information = sb.ToString();
        }

        [Flags]
        enum VolumeFlags : ushort
        {
            SecondFatActive = 1, VolumeDirty = 2, MediaFailure = 4,
            ClearToZero     = 8
        }

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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct OemParameterTable
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public readonly OemParameter[] parameters;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] padding;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ChecksumSector
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public readonly uint[] checksum;
        }
    }
}