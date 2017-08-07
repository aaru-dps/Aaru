// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Fossil.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Fossil filesystem and shows information.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    public class Fossil : Filesystem
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FossilHeader
        {
            /// <summary>
            /// Magic number
            /// </summary>
            public uint magic;
            /// <summary>
            /// Header version
            /// </summary>
            public ushort version;
            /// <summary>
            /// Block size
            /// </summary>
            public ushort blockSize;
            /// <summary>
            /// Block containing superblock
            /// </summary>
            public uint super;
            /// <summary>
            /// Block containing labels
            /// </summary>
            public uint label;
            /// <summary>
            /// Where do data blocks start
            /// </summary>
            public uint data;
            /// <summary>
            /// How many data blocks does it have
            /// </summary>
            public uint end;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FossilSuperBlock
        {
            /// <summary>
            /// Magic number
            /// </summary>
            public uint magic;
            /// <summary>
            /// Header version
            /// </summary>
            public ushort version;
            /// <summary>
            /// file system low epoch
            /// </summary>
            public uint epochLow;
            /// <summary>
            /// file system high(active) epoch
            /// </summary>
            public uint epochHigh;
            /// <summary>
            /// next qid to allocate
            /// </summary>
            public ulong qid;
            /// <summary>
            /// data block number: root of active file system
            /// </summary>
            public int active;
            /// <summary>
            /// data block number: root of next file system to archive
            /// </summary>
            public int next;
            /// <summary>
            /// data block number: root of file system currently being archived
            /// </summary>
            public int current;
            /// <summary>
            /// Venti score of last successful archive
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] last;
            /// <summary>
            /// name of file system(just a comment)
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] name;
        }

        const uint Fossil_HdrMagic = 0x3776AE89;
        const uint Fossil_SbMagic = 0x2340A3B1;
        // Fossil header starts at 128KiB
        const ulong HeaderPos = 128 * 1024;

        public Fossil()
        {
            Name = "Fossil Filesystem Plugin";
            PluginUUID = new Guid("932BF104-43F6-494F-973C-45EF58A51DA9");
            CurrentEncoding = Encoding.UTF8;
        }

        public Fossil(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Fossil Filesystem Plugin";
            PluginUUID = new Guid("932BF104-43F6-494F-973C-45EF58A51DA9");
            // Technically everything on Plan 9 from Bell Labs is in UTF-8
            CurrentEncoding = Encoding.UTF8;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            ulong hdrSector = HeaderPos / imagePlugin.GetSectorSize();

            FossilHeader hdr = new FossilHeader();

            byte[] sector = imagePlugin.ReadSector(partition.Start + hdrSector);
            hdr = BigEndianMarshal.ByteArrayToStructureBigEndian<FossilHeader>(sector);

            DicConsole.DebugWriteLine("Fossil plugin", "magic at 0x{0:X8} (expected 0x{1:X8})", hdr.magic, Fossil_HdrMagic);

            return hdr.magic == Fossil_HdrMagic;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";
            if(imagePlugin.GetSectorSize() < 512)
                return;

            ulong hdrSector = HeaderPos / imagePlugin.GetSectorSize();

            FossilHeader hdr = new FossilHeader();

            byte[] sector = imagePlugin.ReadSector(partition.Start + hdrSector);
            hdr = BigEndianMarshal.ByteArrayToStructureBigEndian<FossilHeader>(sector);

            DicConsole.DebugWriteLine("Fossil plugin", "magic at 0x{0:X8} (expected 0x{1:X8})", hdr.magic, Fossil_HdrMagic);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Fossil");
            sb.AppendFormat("Filesystem version {0}", hdr.version).AppendLine();
            sb.AppendFormat("{0} bytes per block", hdr.blockSize).AppendLine();
            sb.AppendFormat("Superblock resides in block {0}", hdr.super).AppendLine();
            sb.AppendFormat("Labels resides in block {0}", hdr.label).AppendLine();
            sb.AppendFormat("Data starts at block {0}", hdr.data).AppendLine();
            sb.AppendFormat("Volume has {0} blocks", hdr.end).AppendLine();

            ulong sbLocation = (hdr.super * (hdr.blockSize / imagePlugin.GetSectorSize())) + partition.Start;

            xmlFSType = new Schemas.FileSystemType
            {
                Type = "Fossil filesystem",
                ClusterSize = hdr.blockSize,
                Clusters = hdr.end
            };

            if(sbLocation <= partition.End)
            {
                sector = imagePlugin.ReadSector(sbLocation);
                System.Console.WriteLine("Searching for superblock on sector {0}", sbLocation);
                FossilSuperBlock fsb = BigEndianMarshal.ByteArrayToStructureBigEndian<FossilSuperBlock>(sector);

                DicConsole.DebugWriteLine("Fossil plugin", "magic 0x{0:X8} (expected 0x{1:X8})", fsb.magic, Fossil_SbMagic);

                if(fsb.magic == Fossil_SbMagic)
                {
                    sb.AppendFormat("Epoch low {0}", fsb.epochLow).AppendLine();
                    sb.AppendFormat("Epoch high {0}", fsb.epochHigh).AppendLine();
                    sb.AppendFormat("Next QID {0}", fsb.qid).AppendLine();
                    sb.AppendFormat("Active root block {0}", fsb.active).AppendLine();
                    sb.AppendFormat("Next root block {0}", fsb.next).AppendLine();
                    sb.AppendFormat("Curren root block {0}", fsb.current).AppendLine();
                    sb.AppendFormat("Volume label: \"{0}\"", StringHandlers.CToString(fsb.name, CurrentEncoding)).AppendLine();
                    xmlFSType.VolumeName = StringHandlers.CToString(fsb.name, CurrentEncoding);
                }
            }

            information = sb.ToString();
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