// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Filesystems
{
    // Information from https://www.sans.org/reading-room/whitepapers/forensics/reverse-engineering-microsoft-exfat-file-system-33274
    public class exFAT : Filesystem
    {
        public exFAT()
        {
            Name = "Microsoft Extended File Allocation Table";
            PluginUUID = new Guid("8271D088-1533-4CB3-AC28-D802B68BB95C");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public exFAT(Encoding encoding)
        {
            Name = "Microsoft Extended File Allocation Table";
            PluginUUID = new Guid("8271D088-1533-4CB3-AC28-D802B68BB95C");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public exFAT(ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Microsoft Extended File Allocation Table";
            PluginUUID = new Guid("8271D088-1533-4CB3-AC28-D802B68BB95C");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugin imagePlugin, Partition partition)
        {
            if(12 + partition.Start >= partition.End) return false;

            byte[] vbrSector = imagePlugin.ReadSector(0 + partition.Start);
            if(vbrSector.Length < 512) return false;

            VolumeBootRecord vbr = new VolumeBootRecord();
            IntPtr vbrPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(vbrSector, 0, vbrPtr, 512);
            vbr = (VolumeBootRecord)Marshal.PtrToStructure(vbrPtr, typeof(VolumeBootRecord));
            Marshal.FreeHGlobal(vbrPtr);

            return Signature.SequenceEqual(vbr.signature);
        }

        public override void GetInformation(ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();
            xmlFSType = new Schemas.FileSystemType();

            byte[] vbrSector = imagePlugin.ReadSector(0 + partition.Start);
            VolumeBootRecord vbr = new VolumeBootRecord();
            IntPtr vbrPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(vbrSector, 0, vbrPtr, 512);
            vbr = (VolumeBootRecord)Marshal.PtrToStructure(vbrPtr, typeof(VolumeBootRecord));
            Marshal.FreeHGlobal(vbrPtr);

            byte[] parametersSector = imagePlugin.ReadSector(9 + partition.Start);
            OemParameterTable parametersTable = new OemParameterTable();
            IntPtr parametersPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(parametersSector, 0, parametersPtr, 512);
            parametersTable = (OemParameterTable)Marshal.PtrToStructure(parametersPtr, typeof(OemParameterTable));
            Marshal.FreeHGlobal(parametersPtr);

            byte[] chkSector = imagePlugin.ReadSector(11 + partition.Start);
            ChecksumSector chksector = new ChecksumSector();
            IntPtr chkPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(chkSector, 0, chkPtr, 512);
            chksector = (ChecksumSector)Marshal.PtrToStructure(chkPtr, typeof(ChecksumSector));
            Marshal.FreeHGlobal(chkPtr);

            sb.AppendLine("Microsoft exFAT");
            sb.AppendFormat("Partition offset: {0}", vbr.offset).AppendLine();
            sb.AppendFormat("Volume has {0} sectors of {1} bytes each for a total of {2} bytes", vbr.sectors,
                            1 << vbr.sectorShift, vbr.sectors * (ulong)(1 << vbr.sectorShift)).AppendLine();
            sb.AppendFormat("Volume uses clusters of {0} sectors ({1} bytes) each", 1 << vbr.clusterShift,
                            (1 << vbr.sectorShift) * (1 << vbr.clusterShift)).AppendLine();
            sb.AppendFormat("First FAT starts at sector {0} and runs for {1} sectors", vbr.fatOffset, vbr.fatLength)
              .AppendLine();
            sb.AppendFormat("Volume uses {0} FATs", vbr.fats).AppendLine();
            sb.AppendFormat("Cluster heap starts at sector {0}, contains {1} clusters and is {2}% used",
                            vbr.clusterHeapOffset, vbr.clusterHeapLength, vbr.heapUsage).AppendLine();
            sb.AppendFormat("Root directory starts at cluster {0}", vbr.rootDirectoryCluster).AppendLine();
            sb.AppendFormat("Filesystem revision is {0}.{1:D2}", (vbr.revision & 0xFF00) >> 8, vbr.revision & 0xFF)
              .AppendLine();
            sb.AppendFormat("Volume serial number: {0:X8}", vbr.volumeSerial).AppendLine();
            sb.AppendFormat("BIOS drive is {0:X2}h", vbr.drive).AppendLine();
            if(vbr.flags.HasFlag(VolumeFlags.SecondFATActive)) sb.AppendLine("2nd FAT is in use");
            if(vbr.flags.HasFlag(VolumeFlags.VolumeDirty)) sb.AppendLine("Volume is dirty");
            if(vbr.flags.HasFlag(VolumeFlags.MediaFailure)) sb.AppendLine("Underlying media presented errors");

            int count = 1;
            foreach(OemParameter parameter in parametersTable.parameters)
            {
                if(parameter.OemParameterType == OEM_FLASH_PARAMETER_GUID)
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

            xmlFSType.ClusterSize = (1 << vbr.sectorShift) * (1 << vbr.clusterShift);
            xmlFSType.Clusters = vbr.clusterHeapLength;
            xmlFSType.Dirty = vbr.flags.HasFlag(VolumeFlags.VolumeDirty);
            xmlFSType.Type = "exFAT";
            xmlFSType.VolumeSerial = string.Format("{0:X8}", vbr.volumeSerial);

            information = sb.ToString();
        }

        readonly byte[] Signature = {0x45, 0x58, 0x46, 0x41, 0x54, 0x20, 0x20, 0x20};
        readonly Guid OEM_FLASH_PARAMETER_GUID = new Guid("0A0C7E46-3399-4021-90C8-FA6D389C4BA2");

        [Flags]
        enum VolumeFlags : ushort
        {
            SecondFATActive = 1,
            VolumeDirty = 2,
            MediaFailure = 4,
            ClearToZero = 8
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct VolumeBootRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public byte[] jump;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] signature;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 53)] public byte[] zero;
            public ulong offset;
            public ulong sectors;
            public uint fatOffset;
            public uint fatLength;
            public uint clusterHeapOffset;
            public uint clusterHeapLength;
            public uint rootDirectoryCluster;
            public uint volumeSerial;
            public ushort revision;
            public VolumeFlags flags;
            public byte sectorShift;
            public byte clusterShift;
            public byte fats;
            public byte drive;
            public byte heapUsage;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 53)] public byte[] reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 53)] public byte[] bootCode;
            public ushort bootSignature;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OemParameter
        {
            public Guid OemParameterType;
            public uint eraseBlockSize;
            public uint pageSize;
            public uint spareBlocks;
            public uint randomAccessTime;
            public uint programTime;
            public uint readCycleTime;
            public uint writeCycleTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OemParameterTable
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public OemParameter[] parameters;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] padding;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChecksumSector
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public uint[] checksum;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            throw new NotImplementedException();
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            throw new NotImplementedException();
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            throw new NotImplementedException();
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            throw new NotImplementedException();
        }

        public override Errno Mount()
        {
            throw new NotImplementedException();
        }

        public override Errno Mount(bool debug)
        {
            throw new NotImplementedException();
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            throw new NotImplementedException();
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            throw new NotImplementedException();
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            throw new NotImplementedException();
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            throw new NotImplementedException();
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            throw new NotImplementedException();
        }

        public override Errno Unmount()
        {
            throw new NotImplementedException();
        }
    }
}