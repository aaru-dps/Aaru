// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    public class Cram : Filesystem
    {
        public Cram()
        {
            Name = "Cram filesystem";
            PluginUUID = new Guid("F8F6E46F-7A2A-48E3-9C0A-46AF4DC29E09");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public Cram(Encoding encoding)
        {
            Name = "Cram filesystem";
            PluginUUID = new Guid("F8F6E46F-7A2A-48E3-9C0A-46AF4DC29E09");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public Cram(DiscImages.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Cram filesystem";
            PluginUUID = new Guid("F8F6E46F-7A2A-48E3-9C0A-46AF4DC29E09");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        enum CramCompression : ushort
        {
            Zlib = 1,
            Lzma = 2,
            Lzo = 3,
            Xz = 4,
            Lz4 = 5
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CramSuperBlock
        {
            public uint magic;
            public uint size;
            public uint flags;
            public uint future;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] signature;
            public uint crc;
            public uint edition;
            public uint blocks;
            public uint files;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] name;
        }

        /// <summary>
        /// Identifier for Cram
        /// </summary>
        const uint Cram_MAGIC = 0x28CD3D45;
        const uint Cram_CIGAM = 0x453DCD28;

        public override bool Identify(DiscImages.ImagePlugin imagePlugin, Partition partition)
        {
            if(partition.Start >= partition.End) return false;

            byte[] sector = imagePlugin.ReadSector(partition.Start);

            uint magic = BitConverter.ToUInt32(sector, 0x00);

            return magic == Cram_MAGIC || magic == Cram_CIGAM;
        }

        public override void GetInformation(DiscImages.ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            byte[] sector = imagePlugin.ReadSector(partition.Start);
            uint magic = BitConverter.ToUInt32(sector, 0x00);

            CramSuperBlock crSb = new CramSuperBlock();
            bool littleEndian = true;

            switch(magic) {
                case Cram_MAGIC:
                    IntPtr crSbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(crSb));
                    Marshal.Copy(sector, 0, crSbPtr, Marshal.SizeOf(crSb));
                    crSb = (CramSuperBlock)Marshal.PtrToStructure(crSbPtr, typeof(CramSuperBlock));
                    Marshal.FreeHGlobal(crSbPtr);
                    break;
                case Cram_CIGAM:
                    crSb = BigEndianMarshal.ByteArrayToStructureBigEndian<CramSuperBlock>(sector);
                    littleEndian = false;
                    break;
            }

            StringBuilder sbInformation = new StringBuilder();

            sbInformation.AppendLine("Cram file system");
            if(littleEndian) sbInformation.AppendLine("Little-endian");
            else sbInformation.AppendLine("Big-endian");
            sbInformation.AppendFormat("Volume edition {0}", crSb.edition).AppendLine();
            sbInformation.AppendFormat("Volume name: {0}", StringHandlers.CToString(crSb.name, CurrentEncoding))
                         .AppendLine();
            sbInformation.AppendFormat("Volume has {0} bytes", crSb.size).AppendLine();
            sbInformation.AppendFormat("Volume has {0} blocks", crSb.blocks).AppendLine();
            sbInformation.AppendFormat("Volume has {0} files", crSb.files).AppendLine();

            information = sbInformation.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.VolumeName = StringHandlers.CToString(crSb.name, CurrentEncoding);
            xmlFSType.Type = "Cram file system";
            xmlFSType.Clusters = crSb.blocks;
            xmlFSType.Files = crSb.files;
            xmlFSType.FilesSpecified = true;
            xmlFSType.FreeClusters = 0;
            xmlFSType.FreeClustersSpecified = true;
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}