// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SolarFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SolarOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the SolarOS filesystem and shows information.
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
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.Console;

namespace DiscImageChef.Filesystems
{
    // Based on FAT's BPB, cannot find a FAT or directory
    public class SolarFS : Filesystem
    {
        public SolarFS()
        {
            Name = "Solar_OS filesystem";
            PluginUUID = new Guid("EA3101C1-E777-4B4F-B5A3-8C57F50F6E65");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public SolarFS(Encoding encoding)
        {
            Name = "Solar_OS filesystem";
            PluginUUID = new Guid("EA3101C1-E777-4B4F-B5A3-8C57F50F6E65");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public SolarFS(DiscImages.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Solar_OS filesystem";
            PluginUUID = new Guid("EA3101C1-E777-4B4F-B5A3-8C57F50F6E65");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public override bool Identify(DiscImages.ImagePlugin imagePlugin, Partition partition)
        {
            if((2 + partition.Start) >= partition.End) return false;

            byte signature; /// <summary>0x29
            string fs_type; // "SOL_FS  "

            byte[] bpb = imagePlugin.ReadSector(0 + partition.Start);

            byte[] fs_type_b = new byte[8];

            signature = bpb[0x25];
            Array.Copy(bpb, 0x35, fs_type_b, 0, 8);
            fs_type = StringHandlers.CToString(fs_type_b);

            if(signature == 0x29 && fs_type == "SOL_FS  ") return true;

            return false;
        }

        public override void GetInformation(DiscImages.ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();
            byte[] bpb_sector = imagePlugin.ReadSector(0 + partition.Start);
            byte[] bpb_strings;

            SolarOSParameterBlock BPB = new SolarOSParameterBlock();

            bpb_strings = new byte[8];
            Array.Copy(bpb_sector, 0x03, bpb_strings, 0, 8);
            BPB.OEMName = StringHandlers.CToString(bpb_strings);
            BPB.bps = BitConverter.ToUInt16(bpb_sector, 0x0B);
            BPB.root_ent = BitConverter.ToUInt16(bpb_sector, 0x10);
            BPB.sectors = BitConverter.ToUInt16(bpb_sector, 0x12);
            BPB.media = bpb_sector[0x14];
            BPB.spfat = BitConverter.ToUInt16(bpb_sector, 0x15);
            BPB.sptrk = BitConverter.ToUInt16(bpb_sector, 0x17);
            BPB.heads = BitConverter.ToUInt16(bpb_sector, 0x19);
            BPB.signature = bpb_sector[0x25];
            bpb_strings = new byte[8];
            Array.Copy(bpb_sector, 0x2A, bpb_strings, 0, 11);
            BPB.vol_name = StringHandlers.CToString(bpb_strings, CurrentEncoding);
            bpb_strings = new byte[8];
            Array.Copy(bpb_sector, 0x35, bpb_strings, 0, 8);
            BPB.fs_type = StringHandlers.CToString(bpb_strings, CurrentEncoding);

            BPB.x86_jump = new byte[3];
            Array.Copy(bpb_sector, 0x00, BPB.x86_jump, 0, 3);
            BPB.unk1 = bpb_sector[0x0D];
            BPB.unk2 = BitConverter.ToUInt16(bpb_sector, 0x0E);
            BPB.unk3 = new byte[10];
            Array.Copy(bpb_sector, 0x1B, BPB.unk3, 0, 10);
            BPB.unk4 = BitConverter.ToUInt32(bpb_sector, 0x26);

            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.x86_jump: 0x{0:X2}{1:X2}{2:X2}", BPB.x86_jump[0],
                                      BPB.x86_jump[1], BPB.x86_jump[2]);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.OEMName: \"{0}\"", BPB.OEMName);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.bps: {0}", BPB.bps);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.unk1: 0x{0:X2}", BPB.unk1);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.unk2: 0x{0:X4}", BPB.unk2);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.root_ent: {0}", BPB.root_ent);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.sectors: {0}", BPB.sectors);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.media: 0x{0:X2}", BPB.media);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.spfat: {0}", BPB.spfat);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.sptrk: {0}", BPB.sptrk);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.heads: {0}", BPB.heads);
            DicConsole.DebugWriteLine("SolarFS plugin",
                                      "BPB.unk3: 0x{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}",
                                      BPB.unk3[0], BPB.unk3[1], BPB.unk3[2], BPB.unk3[3], BPB.unk3[4], BPB.unk3[5],
                                      BPB.unk3[6], BPB.unk3[7], BPB.unk3[8], BPB.unk3[9]);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.signature: 0x{0:X2}", BPB.signature);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.unk4: 0x{0:X8}", BPB.unk4);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.vol_name: \"{0}\"", BPB.vol_name);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.fs_type: \"{0}\"", BPB.fs_type);

            sb.AppendLine("Solar_OS filesystem");
            sb.AppendFormat("Media descriptor: 0x{0:X2}", BPB.media).AppendLine();
            sb.AppendFormat("{0} bytes per sector", BPB.bps).AppendLine();
            if(imagePlugin.GetSectorSize() == 2336 || imagePlugin.GetSectorSize() == 2352 ||
               imagePlugin.GetSectorSize() == 2448)
            {
                if(BPB.bps != imagePlugin.GetSectorSize())
                {
                    sb
                        .AppendFormat("WARNING: Filesystem describes a {0} bytes/sector, while device describes a {1} bytes/sector",
                                      BPB.bps, 2048).AppendLine();
                }
            }
            else if(BPB.bps != imagePlugin.GetSectorSize())
            {
                sb
                    .AppendFormat("WARNING: Filesystem describes a {0} bytes/sector, while device describes a {1} bytes/sector",
                                  BPB.bps, imagePlugin.GetSectorSize()).AppendLine();
            }
            sb.AppendFormat("{0} sectors on volume ({1} bytes)", BPB.sectors, BPB.sectors * BPB.bps).AppendLine();
            if(BPB.sectors > imagePlugin.GetSectors())
                sb.AppendFormat("WARNING: Filesystem describes a {0} sectors volume, bigger than device ({1} sectors)",
                                BPB.sectors, imagePlugin.GetSectors());
            sb.AppendFormat("{0} heads", BPB.heads).AppendLine();
            sb.AppendFormat("{0} sectors per track", BPB.sptrk).AppendLine();
            sb.AppendFormat("Volume name: {0}", BPB.vol_name).AppendLine();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "SolarFS";
            xmlFSType.Clusters = BPB.sectors;
            xmlFSType.ClusterSize = BPB.bps;
            xmlFSType.VolumeName = BPB.vol_name;

            information = sb.ToString();
        }

        struct SolarOSParameterBlock
        {
            /// <summary>0x00, x86 jump (3 bytes), jumps to 0x60</summary>
            public byte[] x86_jump;
            /// <summary>0x03, 8 bytes, "SOLAR_OS"</summary>
            public string OEMName;
            /// <summary>0x0B, Bytes per sector</summary>
            public ushort bps;
            /// <summary>0x0D, unknown, 0x01</summary>
            public byte unk1;
            /// <summary>0x0E, unknown, 0x0201</summary>
            public ushort unk2;
            /// <summary>0x10, Number of entries on root directory ? (no root directory found)</summary>
            public ushort root_ent;
            /// <summary>0x12, Sectors in volume</summary>
            public ushort sectors;
            /// <summary>0x14, Media descriptor</summary>
            public byte media;
            /// <summary>0x15, Sectors per FAT ? (no FAT found)</summary>
            public ushort spfat;
            /// <summary>0x17, Sectors per track</summary>
            public ushort sptrk;
            /// <summary>0x19, Heads</summary>
            public ushort heads;
            /// <summary>0x1B, unknown, 10 bytes, zero-filled</summary>
            public byte[] unk3;
            /// <summary>0x25, 0x29</summary>
            public byte signature;
            /// <summary>0x26, unknown, zero-filled</summary>
            public uint unk4;
            /// <summary>0x2A, 11 bytes, volume name, space-padded</summary>
            public string vol_name;
            /// <summary>0x35, 8 bytes, "SOL_FS  "</summary>
            public string fs_type;
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