// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Reiser4.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser4 filesystem plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Reiser4 filesystem and shows information.
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public class Reiser4 : IFilesystem
    {
        const uint REISER4_SUPER_OFFSET = 0x10000;

        readonly byte[] Reiser4_Magic =
            {0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

        public FileSystemType XmlFsType { get; private set; }
        public Encoding Encoding { get; private set; }
        public string Name => "Reiser4 Filesystem Plugin";
        public Guid Id => new Guid("301F2D00-E8D5-4F04-934E-81DFB21D15BA");

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512) return false;

            uint sbAddr = REISER4_SUPER_OFFSET / imagePlugin.Info.SectorSize;
            if(sbAddr == 0) sbAddr = 1;

            Reiser4_Superblock reiserSb = new Reiser4_Superblock();

            uint sbSize = (uint)(Marshal.SizeOf(reiserSb) / imagePlugin.Info.SectorSize);
            if(Marshal.SizeOf(reiserSb) % imagePlugin.Info.SectorSize != 0) sbSize++;

            if(partition.Start + sbAddr + sbSize >= partition.End) return false;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf(reiserSb)) return false;

            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(reiserSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(reiserSb));
            reiserSb = (Reiser4_Superblock)Marshal.PtrToStructure(sbPtr, typeof(Reiser4_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            return Reiser4_Magic.SequenceEqual(reiserSb.magic);
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";
            if(imagePlugin.Info.SectorSize < 512) return;

            uint sbAddr = REISER4_SUPER_OFFSET / imagePlugin.Info.SectorSize;
            if(sbAddr == 0) sbAddr = 1;

            Reiser4_Superblock reiserSb = new Reiser4_Superblock();

            uint sbSize = (uint)(Marshal.SizeOf(reiserSb) / imagePlugin.Info.SectorSize);
            if(Marshal.SizeOf(reiserSb) % imagePlugin.Info.SectorSize != 0) sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf(reiserSb)) return;

            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(reiserSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(reiserSb));
            reiserSb = (Reiser4_Superblock)Marshal.PtrToStructure(sbPtr, typeof(Reiser4_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            if(!Reiser4_Magic.SequenceEqual(reiserSb.magic)) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Reiser 4 filesystem");
            sb.AppendFormat("{0} bytes per block", reiserSb.blocksize).AppendLine();
            sb.AppendFormat("Volume disk format: {0}", reiserSb.diskformat).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", reiserSb.uuid).AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(reiserSb.label, Encoding)).AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type = "Reiser 4 filesystem",
                ClusterSize = reiserSb.blocksize,
                Clusters = (long)((partition.End - partition.Start) * imagePlugin.Info.SectorSize / reiserSb.blocksize),
                VolumeName = StringHandlers.CToString(reiserSb.label, Encoding),
                VolumeSerial = reiserSb.uuid.ToString()
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Reiser4_Superblock
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] magic;
            public ushort diskformat;
            public ushort blocksize;
            public Guid uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] label;
        }
    }
}