// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : LIF.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : HP Logical Interchange Format plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the HP Logical Interchange Format and shows information.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using Schemas;

namespace DiscImageChef.Filesystems
{
    // Information from http://www.hp9845.net/9845/projects/hpdir/#lif_filesystem
    public class LIF : IFilesystem
    {
        const uint LIF_MAGIC = 0x8000;

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "HP Logical Interchange Format Plugin";
        public Guid           Id        => new Guid("41535647-77A5-477B-9206-DA727ACDC704");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 256) return false;

            byte[]         sector = imagePlugin.ReadSector(partition.Start);
            LifSystemBlock lifSb  = BigEndianMarshal.ByteArrayToStructureBigEndian<LifSystemBlock>(sector);
            DicConsole.DebugWriteLine("LIF plugin", "magic 0x{0:X8} (expected 0x{1:X8})", lifSb.magic, LIF_MAGIC);

            return lifSb.magic == LIF_MAGIC;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            if(imagePlugin.Info.SectorSize < 256) return;

            byte[]         sector = imagePlugin.ReadSector(partition.Start);
            LifSystemBlock lifSb  = BigEndianMarshal.ByteArrayToStructureBigEndian<LifSystemBlock>(sector);

            if(lifSb.magic != LIF_MAGIC) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("HP Logical Interchange Format");
            sb.AppendFormat("Directory starts at cluster {0}", lifSb.directoryStart).AppendLine();
            sb.AppendFormat("LIF identifier: {0}", lifSb.lifId).AppendLine();
            sb.AppendFormat("Directory size: {0} clusters", lifSb.directorySize).AppendLine();
            sb.AppendFormat("LIF version: {0}", lifSb.lifVersion).AppendLine();
            // How is this related to volume size? I have only CDs to test and makes no sense there
            sb.AppendFormat("{0} tracks", lifSb.tracks).AppendLine();
            sb.AppendFormat("{0} heads", lifSb.heads).AppendLine();
            sb.AppendFormat("{0} sectors", lifSb.sectors).AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(lifSb.volumeLabel, Encoding)).AppendLine();
            sb.AppendFormat("Volume created on {0}", DateHandlers.LifToDateTime(lifSb.creationDate)).AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type                  = "HP Logical Interchange Format",
                ClusterSize           = 256,
                Clusters              = (long)(partition.Size / 256),
                CreationDate          = DateHandlers.LifToDateTime(lifSb.creationDate),
                CreationDateSpecified = true,
                VolumeName            = StringHandlers.CToString(lifSb.volumeLabel, Encoding)
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct LifSystemBlock
        {
            public ushort magic;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] volumeLabel;
            public uint   directoryStart;
            public ushort lifId;
            public ushort unused;
            public uint   directorySize;
            public ushort lifVersion;
            public ushort unused2;
            public uint   tracks;
            public uint   heads;
            public uint   sectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] creationDate;
        }
    }
}