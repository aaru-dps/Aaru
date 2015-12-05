/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : SolarFS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies SolarOS filesystems and shows information.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Text;
using DiscImageChef;

// Based on FAT's BPB, cannot find a FAT or directory
using DiscImageChef.Console;


namespace DiscImageChef.Plugins
{
    class SolarFS : Plugin
    {
        public SolarFS()
        {
            Name = "Solar_OS filesystem";
            PluginUUID = new Guid("EA3101C1-E777-4B4F-B5A3-8C57F50F6E65");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if ((2 + partitionStart) >= imagePlugin.GetSectors())
                return false;

            byte signature; // 0x29
            string fs_type; // "SOL_FS  "

            byte[] bpb = imagePlugin.ReadSector(0 + partitionStart);

            byte[] fs_type_b = new byte[8];

            signature = bpb[0x25];
            Array.Copy(bpb, 0x35, fs_type_b, 0, 8);
            fs_type = StringHandlers.CToString(fs_type_b);

            if (signature == 0x29 && fs_type == "SOL_FS  ")
                return true;
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";
			
            StringBuilder sb = new StringBuilder();
            byte[] bpb_sector = imagePlugin.ReadSector(0 + partitionStart);
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
            BPB.vol_name = StringHandlers.CToString(bpb_strings);
            bpb_strings = new byte[8];
            Array.Copy(bpb_sector, 0x35, bpb_strings, 0, 8);
            BPB.fs_type = StringHandlers.CToString(bpb_strings);

            BPB.x86_jump = new byte[3];
            Array.Copy(bpb_sector, 0x00, BPB.x86_jump, 0, 3);
            BPB.unk1 = bpb_sector[0x0D];
            BPB.unk2 = BitConverter.ToUInt16(bpb_sector, 0x0E);
            BPB.unk3 = new byte[10];
            Array.Copy(bpb_sector, 0x1B, BPB.unk3, 0, 10);
            BPB.unk4 = BitConverter.ToUInt32(bpb_sector, 0x26);

            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.x86_jump: 0x{0:X2}{1:X2}{2:X2}", BPB.x86_jump[0], BPB.x86_jump[1], BPB.x86_jump[2]);
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
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.unk3: 0x{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}", BPB.unk3[0], BPB.unk3[1], BPB.unk3[2], BPB.unk3[3], BPB.unk3[4], BPB.unk3[5], BPB.unk3[6], BPB.unk3[7], BPB.unk3[8], BPB.unk3[9]);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.signature: 0x{0:X2}", BPB.signature);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.unk4: 0x{0:X8}", BPB.unk4);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.vol_name: \"{0}\"", BPB.vol_name);
            DicConsole.DebugWriteLine("SolarFS plugin", "BPB.fs_type: \"{0}\"", BPB.fs_type);

            sb.AppendLine("Solar_OS filesystem");
            sb.AppendFormat("Media descriptor: 0x{0:X2}", BPB.media).AppendLine();
            sb.AppendFormat("{0} bytes per sector", BPB.bps).AppendLine();
            if (imagePlugin.GetSectorSize() == 2336 || imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
            {
                if (BPB.bps != imagePlugin.GetSectorSize())
                {
                    sb.AppendFormat("WARNING: Filesystem describes a {0} bytes/sector, while device describes a {1} bytes/sector", BPB.bps, 2048).AppendLine();
                }
            }
            else if (BPB.bps != imagePlugin.GetSectorSize())
            {
                sb.AppendFormat("WARNING: Filesystem describes a {0} bytes/sector, while device describes a {1} bytes/sector", BPB.bps, imagePlugin.GetSectorSize()).AppendLine();
            }
            sb.AppendFormat("{0} sectors on volume ({1} bytes)", BPB.sectors, BPB.sectors * BPB.bps).AppendLine();
            if (BPB.sectors > imagePlugin.GetSectors())
                sb.AppendFormat("WARNING: Filesystem describes a {0} sectors volume, bigger than device ({1} sectors)", BPB.sectors, imagePlugin.GetSectors());
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

        public struct SolarOSParameterBlock
        {
            public byte[] x86_jump;
            // 0x00, x86 jump (3 bytes), jumps to 0x60
            public string OEMName;
            // 0x03, 8 bytes, "SOLAR_OS"
            public UInt16 bps;
            // 0x0B, Bytes per sector
            public byte unk1;
            // 0x0D, unknown, 0x01
            public UInt16 unk2;
            // 0x0E, unknown, 0x0201
            public UInt16 root_ent;
            // 0x10, Number of entries on root directory ? (no root directory found)
            public UInt16 sectors;
            // 0x12, Sectors in volume
            public byte media;
            // 0x14, Media descriptor
            public UInt16 spfat;
            // 0x15, Sectors per FAT ? (no FAT found)
            public UInt16 sptrk;
            // 0x17, Sectors per track
            public UInt16 heads;
            // 0x19, Heads
            public byte[] unk3;
            // 0x1B, unknown, 10 bytes, zero-filled
            public byte signature;
            // 0x25, 0x29
            public UInt32 unk4;
            // 0x26, unknown, zero-filled
            public string vol_name;
            // 0x2A, 11 bytes, volume name, space-padded
            public string fs_type;
            // 0x35, 8 bytes, "SOL_FS  "
        }
    }
}