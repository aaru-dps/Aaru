// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
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
    // Information from http://www.hp9845.net/9845/projects/hpdir/#lif_filesystem
    /// <inheritdoc />
    /// <summary>
    /// Implements detection of the LIF filesystem
    /// </summary>
    public sealed class LIF : IFilesystem
    {
        const uint LIF_MAGIC = 0x8000;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding       Encoding  { get; private set; }
        /// <inheritdoc />
        public string         Name      => "HP Logical Interchange Format Plugin";
        /// <inheritdoc />
        public Guid           Id        => new Guid("41535647-77A5-477B-9206-DA727ACDC704");
        /// <inheritdoc />
        public string         Author    => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 256)
                return false;

            byte[]      sector = imagePlugin.ReadSector(partition.Start);
            SystemBlock lifSb  = Marshal.ByteArrayToStructureBigEndian<SystemBlock>(sector);
            AaruConsole.DebugWriteLine("LIF plugin", "magic 0x{0:X8} (expected 0x{1:X8})", lifSb.magic, LIF_MAGIC);

            return lifSb.magic == LIF_MAGIC;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            if(imagePlugin.Info.SectorSize < 256)
                return;

            byte[]      sector = imagePlugin.ReadSector(partition.Start);
            SystemBlock lifSb  = Marshal.ByteArrayToStructureBigEndian<SystemBlock>(sector);

            if(lifSb.magic != LIF_MAGIC)
                return;

            var sb = new StringBuilder();

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
                Clusters              = partition.Size / 256,
                CreationDate          = DateHandlers.LifToDateTime(lifSb.creationDate),
                CreationDateSpecified = true,
                VolumeName            = StringHandlers.CToString(lifSb.volumeLabel, Encoding)
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct SystemBlock
        {
            public readonly ushort magic;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public readonly byte[] volumeLabel;
            public readonly uint   directoryStart;
            public readonly ushort lifId;
            public readonly ushort unused;
            public readonly uint   directorySize;
            public readonly ushort lifVersion;
            public readonly ushort unused2;
            public readonly uint   tracks;
            public readonly uint   heads;
            public readonly uint   sectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public readonly byte[] creationDate;
        }
    }
}