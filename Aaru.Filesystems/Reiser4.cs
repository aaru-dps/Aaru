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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    public class Reiser4 : IFilesystem
    {
        const uint REISER4_SUPER_OFFSET = 0x10000;

        readonly byte[] reiser4_magic =
        {
            0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "Reiser4 Filesystem Plugin";
        public Guid           Id        => new Guid("301F2D00-E8D5-4F04-934E-81DFB21D15BA");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512) return false;

            uint sbAddr            = REISER4_SUPER_OFFSET / imagePlugin.Info.SectorSize;
            if(sbAddr == 0) sbAddr = 1;

            uint sbSize = (uint)(Marshal.SizeOf<Reiser4_Superblock>() / imagePlugin.Info.SectorSize);
            if(Marshal.SizeOf<Reiser4_Superblock>() % imagePlugin.Info.SectorSize != 0) sbSize++;

            if(partition.Start + sbAddr + sbSize >= partition.End) return false;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf<Reiser4_Superblock>()) return false;

            Reiser4_Superblock reiserSb = Marshal.ByteArrayToStructureLittleEndian<Reiser4_Superblock>(sector);

            return reiser4_magic.SequenceEqual(reiserSb.magic);
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";
            if(imagePlugin.Info.SectorSize < 512) return;

            uint sbAddr            = REISER4_SUPER_OFFSET / imagePlugin.Info.SectorSize;
            if(sbAddr == 0) sbAddr = 1;

            uint sbSize = (uint)(Marshal.SizeOf<Reiser4_Superblock>() / imagePlugin.Info.SectorSize);
            if(Marshal.SizeOf<Reiser4_Superblock>() % imagePlugin.Info.SectorSize != 0) sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf<Reiser4_Superblock>()) return;

            Reiser4_Superblock reiserSb = Marshal.ByteArrayToStructureLittleEndian<Reiser4_Superblock>(sector);

            if(!reiser4_magic.SequenceEqual(reiserSb.magic)) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Reiser 4 filesystem");
            sb.AppendFormat("{0} bytes per block", reiserSb.blocksize).AppendLine();
            sb.AppendFormat("Volume disk format: {0}", reiserSb.diskformat).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", reiserSb.uuid).AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(reiserSb.label, Encoding)).AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type         = "Reiser 4 filesystem",
                ClusterSize  = reiserSb.blocksize,
                Clusters     = (partition.End - partition.Start) * imagePlugin.Info.SectorSize / reiserSb.blocksize,
                VolumeName   = StringHandlers.CToString(reiserSb.label, Encoding),
                VolumeSerial = reiserSb.uuid.ToString()
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Reiser4_Superblock
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] magic;
            public readonly ushort diskformat;
            public readonly ushort blocksize;
            public readonly Guid   uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] label;
        }
    }
}