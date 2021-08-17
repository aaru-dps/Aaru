// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Cram.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Cram file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Cram file system and shows information.
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
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

// ReSharper disable UnusedMember.Local

namespace Aaru.Filesystems
{
    /// <summary>
    /// Implements detection of the CRAM filesystem
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    public sealed class Cram : IFilesystem
    {
        /// <summary>Identifier for Cram</summary>
        const uint CRAM_MAGIC = 0x28CD3D45;
        const uint CRAM_CIGAM = 0x453DCD28;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding       Encoding  { get; private set; }
        /// <inheritdoc />
        public string         Name      => "Cram filesystem";
        /// <inheritdoc />
        public Guid           Id        => new Guid("F8F6E46F-7A2A-48E3-9C0A-46AF4DC29E09");
        /// <inheritdoc />
        public string         Author    => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(partition.Start >= partition.End)
                return false;

            byte[] sector = imagePlugin.ReadSector(partition.Start);

            uint magic = BitConverter.ToUInt32(sector, 0x00);

            return magic == CRAM_MAGIC || magic == CRAM_CIGAM;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding = encoding ?? Encoding.GetEncoding("iso-8859-15");
            byte[] sector = imagePlugin.ReadSector(partition.Start);
            uint   magic  = BitConverter.ToUInt32(sector, 0x00);

            var  crSb         = new SuperBlock();
            bool littleEndian = true;

            switch(magic)
            {
                case CRAM_MAGIC:
                    crSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

                    break;
                case CRAM_CIGAM:
                    crSb         = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sector);
                    littleEndian = false;

                    break;
            }

            var sbInformation = new StringBuilder();

            sbInformation.AppendLine("Cram file system");
            sbInformation.AppendLine(littleEndian ? "Little-endian" : "Big-endian");
            sbInformation.AppendFormat("Volume edition {0}", crSb.edition).AppendLine();
            sbInformation.AppendFormat("Volume name: {0}", StringHandlers.CToString(crSb.name, Encoding)).AppendLine();
            sbInformation.AppendFormat("Volume has {0} bytes", crSb.size).AppendLine();
            sbInformation.AppendFormat("Volume has {0} blocks", crSb.blocks).AppendLine();
            sbInformation.AppendFormat("Volume has {0} files", crSb.files).AppendLine();

            information = sbInformation.ToString();

            XmlFsType = new FileSystemType
            {
                VolumeName            = StringHandlers.CToString(crSb.name, Encoding),
                Type                  = "Cram file system",
                Clusters              = crSb.blocks,
                Files                 = crSb.files,
                FilesSpecified        = true,
                FreeClusters          = 0,
                FreeClustersSpecified = true
            };
        }

        enum CramCompression : ushort
        {
            Zlib = 1, Lzma = 2, Lzo = 3,
            Xz   = 4, Lz4  = 5
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct SuperBlock
        {
            public readonly uint magic;
            public readonly uint size;
            public readonly uint flags;
            public readonly uint future;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] signature;
            public readonly uint crc;
            public readonly uint edition;
            public readonly uint blocks;
            public readonly uint files;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] name;
        }
    }
}