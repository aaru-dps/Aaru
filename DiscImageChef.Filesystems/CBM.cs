// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CBM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commodore file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Commodore file system and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using Claunia.Encoding;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;
using Encoding = System.Text.Encoding;

namespace DiscImageChef.Filesystems
{
    public class CBM : IFilesystem
    {
        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public FileSystemType XmlFsType => xmlFsType;

        public string Name => "Commodore file system";
        public Guid Id => new Guid("D104744E-A376-450C-BAC0-1347C93F983B");
        public Encoding Encoding => currentEncoding;

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(partition.Start > 0) return false;

            if(imagePlugin.Info.SectorSize != 256) return false;

            if(imagePlugin.Info.Sectors != 683 && imagePlugin.Info.Sectors != 768 && imagePlugin.Info.Sectors != 1366 &&
               imagePlugin.Info.Sectors != 3200) return false;

            byte[] sector;

            if(imagePlugin.Info.Sectors == 3200)
            {
                sector = imagePlugin.ReadSector(1560);
                CommodoreHeader cbmHdr = new CommodoreHeader();
                IntPtr cbmHdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(cbmHdr));
                Marshal.Copy(sector, 0, cbmHdrPtr, Marshal.SizeOf(cbmHdr));
                cbmHdr = (CommodoreHeader)Marshal.PtrToStructure(cbmHdrPtr, typeof(CommodoreHeader));
                Marshal.FreeHGlobal(cbmHdrPtr);

                if(cbmHdr.diskDosVersion == 0x44 && cbmHdr.dosVersion == 0x33 && cbmHdr.diskVersion == 0x44)
                    return true;
            }
            else
            {
                sector = imagePlugin.ReadSector(357);
                CommodoreBAM cbmBam = new CommodoreBAM();
                IntPtr cbmBamPtr = Marshal.AllocHGlobal(Marshal.SizeOf(cbmBam));
                Marshal.Copy(sector, 0, cbmBamPtr, Marshal.SizeOf(cbmBam));
                cbmBam = (CommodoreBAM)Marshal.PtrToStructure(cbmBamPtr, typeof(CommodoreBAM));
                Marshal.FreeHGlobal(cbmBamPtr);

                if(cbmBam.dosVersion == 0x41 && (cbmBam.doubleSided == 0x00 || cbmBam.doubleSided == 0x80) &&
                   cbmBam.unused1 == 0x00 && cbmBam.directoryTrack == 0x12) return true;
            }

            return false;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
        {
            currentEncoding = new PETSCII();
            byte[] sector;

            StringBuilder sbInformation = new StringBuilder();

            sbInformation.AppendLine("Commodore file system");

            xmlFsType = new FileSystemType
            {
                Type = "Commodore file system",
                Clusters = (long)imagePlugin.Info.Sectors,
                ClusterSize = 256
            };

            if(imagePlugin.Info.Sectors == 3200)
            {
                sector = imagePlugin.ReadSector(1560);
                CommodoreHeader cbmHdr = new CommodoreHeader();
                IntPtr cbmHdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(cbmHdr));
                Marshal.Copy(sector, 0, cbmHdrPtr, Marshal.SizeOf(cbmHdr));
                cbmHdr = (CommodoreHeader)Marshal.PtrToStructure(cbmHdrPtr, typeof(CommodoreHeader));
                Marshal.FreeHGlobal(cbmHdrPtr);

                sbInformation.AppendFormat("Directory starts at track {0} sector {1}", cbmHdr.directoryTrack,
                                           cbmHdr.directorySector).AppendLine();
                sbInformation
                    .AppendFormat("Disk DOS Version: {0}", Encoding.ASCII.GetString(new[] {cbmHdr.diskDosVersion}))
                    .AppendLine();
                sbInformation.AppendFormat("DOS Version: {0}", Encoding.ASCII.GetString(new[] {cbmHdr.dosVersion}))
                             .AppendLine();
                sbInformation.AppendFormat("Disk Version: {0}", Encoding.ASCII.GetString(new[] {cbmHdr.diskVersion}))
                             .AppendLine();
                sbInformation.AppendFormat("Disk ID: {0}", cbmHdr.diskId).AppendLine();
                sbInformation.AppendFormat("Disk name: {0}", StringHandlers.CToString(cbmHdr.name, currentEncoding))
                             .AppendLine();

                xmlFsType.VolumeName = StringHandlers.CToString(cbmHdr.name, currentEncoding);
                xmlFsType.VolumeSerial = $"{cbmHdr.diskId}";
            }
            else
            {
                sector = imagePlugin.ReadSector(357);
                CommodoreBAM cbmBam = new CommodoreBAM();
                IntPtr cbmBamPtr = Marshal.AllocHGlobal(Marshal.SizeOf(cbmBam));
                Marshal.Copy(sector, 0, cbmBamPtr, Marshal.SizeOf(cbmBam));
                cbmBam = (CommodoreBAM)Marshal.PtrToStructure(cbmBamPtr, typeof(CommodoreBAM));
                Marshal.FreeHGlobal(cbmBamPtr);

                sbInformation.AppendFormat("Directory starts at track {0} sector {1}", cbmBam.directoryTrack,
                                           cbmBam.directorySector).AppendLine();
                sbInformation.AppendFormat("Disk DOS type: {0}",
                                           Encoding.ASCII.GetString(BitConverter.GetBytes(cbmBam.dosType)))
                             .AppendLine();
                sbInformation.AppendFormat("DOS Version: {0}", Encoding.ASCII.GetString(new[] {cbmBam.dosVersion}))
                             .AppendLine();
                sbInformation.AppendFormat("Disk ID: {0}", cbmBam.diskId).AppendLine();
                sbInformation.AppendFormat("Disk name: {0}", StringHandlers.CToString(cbmBam.name, currentEncoding))
                             .AppendLine();

                xmlFsType.VolumeName = StringHandlers.CToString(cbmBam.name, currentEncoding);
                xmlFsType.VolumeSerial = $"{cbmBam.diskId}";
            }

            information = sbInformation.ToString();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CommodoreBAM
        {
            /// <summary>
            ///     Track where directory starts
            /// </summary>
            public byte directoryTrack;
            /// <summary>
            ///     Sector where directory starts
            /// </summary>
            public byte directorySector;
            /// <summary>
            ///     Disk DOS version, 0x41
            /// </summary>
            public byte dosVersion;
            /// <summary>
            ///     Set to 0x80 if 1571, 0x00 if not
            /// </summary>
            public byte doubleSided;
            /// <summary>
            ///     Block allocation map
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 140)] public byte[] bam;
            /// <summary>
            ///     Disk name
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] name;
            /// <summary>
            ///     Filled with 0xA0
            /// </summary>
            public ushort fill1;
            /// <summary>
            ///     Disk ID
            /// </summary>
            public ushort diskId;
            /// <summary>
            ///     Filled with 0xA0
            /// </summary>
            public byte fill2;
            /// <summary>
            ///     DOS type
            /// </summary>
            public ushort dosType;
            /// <summary>
            ///     Filled with 0xA0
            /// </summary>
            public uint fill3;
            /// <summary>
            ///     Unused
            /// </summary>
            public byte unused1;
            /// <summary>
            ///     Block allocation map for Dolphin DOS extended tracks
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] dolphinBam;
            /// <summary>
            ///     Block allocation map for Speed DOS extended tracks
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] speedBam;
            /// <summary>
            ///     Unused
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)] public byte[] unused2;
            /// <summary>
            ///     Free sector count for second side in 1571
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)] public byte[] freeCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CommodoreHeader
        {
            /// <summary>
            ///     Track where directory starts
            /// </summary>
            public byte directoryTrack;
            /// <summary>
            ///     Sector where directory starts
            /// </summary>
            public byte directorySector;
            /// <summary>
            ///     Disk DOS version, 0x44
            /// </summary>
            public byte diskDosVersion;
            /// <summary>
            ///     Unusued
            /// </summary>
            public byte unused1;
            /// <summary>
            ///     Disk name
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] name;
            /// <summary>
            ///     Filled with 0xA0
            /// </summary>
            public ushort fill1;
            /// <summary>
            ///     Disk ID
            /// </summary>
            public ushort diskId;
            /// <summary>
            ///     Filled with 0xA0
            /// </summary>
            public byte fill2;
            /// <summary>
            ///     DOS version ('3')
            /// </summary>
            public byte dosVersion;
            /// <summary>
            ///     Disk version ('D')
            /// </summary>
            public byte diskVersion;
            /// <summary>
            ///     Filled with 0xA0
            /// </summary>
            public short fill3;
        }
    }
}