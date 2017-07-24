// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UNIX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages UNIX VTOC and disklabels.
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
using DiscImageChef.CommonTypes;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.PartPlugins
{
    public class UNIX : PartPlugin
    {
        public const uint UNIXDiskLabel_MAGIC = 0xCA5E600D;
        public const uint UNIXVTOC_MAGIC = 0x600DDEEE;

        public UNIX()
        {
            Name = "UNIX VTOC and disklabel";
            PluginUUID = new Guid("6D35A66F-8D77-426F-A562-D88F6A1F1702");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions, ulong sectorOffset)
        {
            partitions = new List<Partition>();

            uint magic;
            byte[] unix_dl_sector = null;
            bool magic_found = false;

            foreach(ulong i in new ulong[] {0, 1, 8, 29})
            {
                if(i + sectorOffset >= imagePlugin.GetSectors())
                    break;
                
                unix_dl_sector = imagePlugin.ReadSector(i + sectorOffset);
                magic = BitConverter.ToUInt32(unix_dl_sector, 4);
                if(magic == UNIXDiskLabel_MAGIC)
                {
                    magic_found = true;
                    break;
                }
            }

            if(!magic_found)
                return false;

            UNIXDiskLabel dl = new UNIXDiskLabel();
            UNIXVTOC vtoc = new UNIXVTOC(); // old/new
            bool isNewDL = false;
            int vtocoffset = 0;
            ulong counter = 0;

            vtoc.magic = BitConverter.ToUInt32(unix_dl_sector, 172);
            if(vtoc.magic == UNIXVTOC_MAGIC)
            {
                isNewDL = true;
                vtocoffset = 72;
            }
            else
            {
                vtoc.magic = BitConverter.ToUInt32(unix_dl_sector, 172);
                if(vtoc.magic != UNIXDiskLabel_MAGIC)
                {
                    return false;
                }
            }

            dl.version = BitConverter.ToUInt32(unix_dl_sector, 8); // 8
            byte[] dl_serial = new byte[12];
            Array.Copy(unix_dl_sector, 12, dl_serial, 0, 12);
            dl.serial = StringHandlers.CToString(dl_serial); // 12
            dl.cyls = BitConverter.ToUInt32(unix_dl_sector, 24); // 24
            dl.trks = BitConverter.ToUInt32(unix_dl_sector, 28); // 28
            dl.secs = BitConverter.ToUInt32(unix_dl_sector, 32); // 32
            dl.bps = BitConverter.ToUInt32(unix_dl_sector, 36); // 36
            dl.start = BitConverter.ToUInt32(unix_dl_sector, 40); // 40
            dl.alt_tbl = BitConverter.ToUInt32(unix_dl_sector, 92); // 92
            dl.alt_len = BitConverter.ToUInt32(unix_dl_sector, 96); // 96

            if(isNewDL) // Old version VTOC starts here
            {
                dl.phys_cyl = BitConverter.ToUInt32(unix_dl_sector, 100); // 100
                dl.phys_trk = BitConverter.ToUInt32(unix_dl_sector, 104); // 104
                dl.phys_sec = BitConverter.ToUInt32(unix_dl_sector, 108); // 108
                dl.phys_bytes = BitConverter.ToUInt32(unix_dl_sector, 112); // 112
                dl.unknown2 = BitConverter.ToUInt32(unix_dl_sector, 116); // 116
                dl.unknown3 = BitConverter.ToUInt32(unix_dl_sector, 120); // 120
            }

            if(vtoc.magic == UNIXVTOC_MAGIC)
            {
                vtoc.version = BitConverter.ToUInt32(unix_dl_sector, 104 + vtocoffset); // 104/176
                byte[] vtoc_name = new byte[8];
                Array.Copy(unix_dl_sector, 108 + vtocoffset, vtoc_name, 0, 8);
                vtoc.name = StringHandlers.CToString(vtoc_name); // 108/180
                vtoc.slices = BitConverter.ToUInt16(unix_dl_sector, 116 + vtocoffset); // 116/188
                vtoc.unknown = BitConverter.ToUInt16(unix_dl_sector, 118 + vtocoffset); // 118/190

                // TODO: What if number of slices overlaps sector (>23)?
                for(int j = 0; j < vtoc.slices; j++)
                {
                    UNIXVTOCEntry vtoc_ent = new UNIXVTOCEntry
                    {
                        tag = (UNIX_TAG)BitConverter.ToUInt16(unix_dl_sector, 160 + vtocoffset + j * 12 + 0), // 160/232 + j*12
                        flags = BitConverter.ToUInt16(unix_dl_sector, 160 + vtocoffset + j * 12 + 2), // 162/234 + j*12
                        start = BitConverter.ToUInt32(unix_dl_sector, 160 + vtocoffset + j * 12 + 6), // 166/238 + j*12
                        length = BitConverter.ToUInt32(unix_dl_sector, 160 + vtocoffset + j * 12 + 10) // 170/242 + j*12
                    };
                    if((vtoc_ent.flags & 0x200) == 0x200 && vtoc_ent.tag != UNIX_TAG.EMPTY && vtoc_ent.tag != UNIX_TAG.WHOLE)
                    {
                        Partition part = new Partition
                        {
                            Start = vtoc_ent.start,
                            Length = vtoc_ent.length,
                            Offset = vtoc_ent.start * dl.bps,
                            Size = vtoc_ent.length * dl.bps,
                            Sequence = counter,
                            Type = string.Format("UNIX: {0}", decodeUNIXTAG(vtoc_ent.tag, isNewDL)),
                            Scheme = Name
                        };
                        string info = "";

                        if((vtoc_ent.flags & 0x01) == 0x01)
                            info += " (do not mount)";
                        if((vtoc_ent.flags & 0x10) == 0x10)
                            info += " (do not mount)";

                        part.Description = "UNIX slice" + info + ".";

                        partitions.Add(part);
                        counter++;
                    }
                }
            }

            return true;
        }

        // Same as Solaris VTOC
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UNIXDiskLabel
        {
            /// <summary>Drive type, seems always 0</summary>
            public uint type;
            /// <summary>UNIXDiskLabel_MAGIC</summary>
            public uint magic;
            /// <summary>Only seen 1</summary>
            public uint version;
            /// <summary>12 bytes, serial number of the device</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public string serial;
            /// <summary>data cylinders per device</summary>
            public uint cyls;
            /// <summary>data tracks per cylinder</summary>
            public uint trks;
            /// <summary>data sectors per track</summary>
            public uint secs;
            /// <summary>data bytes per sector</summary>
            public uint bps;
            /// <summary>first sector of this partition</summary>
            public uint start;
            /// <summary>48 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] unknown1;
            /// <summary>byte offset of alternate table</summary>
            public uint alt_tbl;
            /// <summary>byte length of alternate table</summary>
            public uint alt_len;
            // From onward here, is not on old version
            /// <summary>physical cylinders per device</summary>
            public uint phys_cyl;
            /// <summary>physical tracks per cylinder</summary>
            public uint phys_trk;
            /// <summary>physical sectors per track</summary>
            public uint phys_sec;
            /// <summary>physical bytes per sector</summary>
            public uint phys_bytes;
            /// <summary></summary>
            public uint unknown2;
            /// <summary></summary>
            public uint unknown3;
            /// <summary>32bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] pad;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UNIXVTOC
        {
            /// <summary>UNIXVTOC_MAGIC</summary>
            public uint magic;
            /// <summary>1</summary>
            public uint version;
            /// <summary>8 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public string name;
            /// <summary># of slices</summary>
            public ushort slices;
            /// <summary></summary>
            public ushort unknown;
            /// <summary>40 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct UNIXVTOCEntry
        {
            /// <summary>TAG</summary>
            public UNIX_TAG tag;
            public ushort flags;
            /// <summary>Flags (see below)</summary>
            public uint start;
            /// <summary>Start sector</summary>
            public uint length;
            /// <summary>Length of slice in sectors</summary>
        }

        public enum UNIX_TAG : ushort
        {
            /// <summary>empty</summary>
            EMPTY = 0x0000,
            /// <summary>boot</summary>
            BOOT = 0x0001,
            /// <summary>root</summary>
            ROOT = 0x0002,
            /// <summary>swap</summary>
            SWAP = 0x0003,
            /// <summary>/usr</summary>
            USER = 0x0004,
            /// <summary>whole disk</summary>
            WHOLE = 0x0005,
            /// <summary>stand partition ??</summary>
            STAND = 0x0006,
            /// <summary>alternate sector space</summary>
            ALT_S = 0x0006,
            /// <summary>/var</summary>
            VAR = 0x0007,
            /// <summary>non UNIX</summary>
            OTHER = 0x0007,
            /// <summary>/home</summary>
            HOME = 0x0008,
            /// <summary>alternate track space</summary>
            ALT_T = 0x0008,
            /// <summary>alternate sector track</summary>
            ALT_ST = 0x0009,
            /// <summary>stand partition ??</summary>
            NEW_STAND = 0x0009,
            /// <summary>cache</summary>
            CACHE = 0x000A,
            /// <summary>/var</summary>
            NEW_VAR = 0x000A,
            /// <summary>reserved</summary>
            RESERVED = 0x000B,
            /// <summary>/home</summary>
            NEW_HOME = 0x000B,
            /// <summary>dump partition</summary>
            DUMP = 0x000C,
            /// <summary>alternate sector track</summary>
            NEW_ALT_ST = 0x000D,
            /// <summary>volume mgt public partition</summary>
            VM_PUBLIC = 0x000E,
            /// <summary>volume mgt private partition</summary>
            VM_PRIVATE = 0x000F
        }

        public static string decodeUNIXTAG(UNIX_TAG type, bool isNew)
        {
            switch(type)
            {
                case UNIX_TAG.EMPTY:
                    return "Unused";
                case UNIX_TAG.BOOT:
                    return "Boot";
                case UNIX_TAG.ROOT:
                    return "/";
                case UNIX_TAG.SWAP:
                    return "Swap";
                case UNIX_TAG.USER:
                    return "/usr";
                case UNIX_TAG.WHOLE:
                    return "Whole disk";
                case UNIX_TAG.STAND:
                    return isNew ? "Stand" : "Alternate sector space";
                case UNIX_TAG.VAR:
                    return isNew ? "/var" : "non UNIX";
                case UNIX_TAG.HOME:
                    return isNew ? "/home" : "Alternate track space";
                case UNIX_TAG.ALT_ST:
                    return isNew ? "Alternate sector track" : "Stand";
                case UNIX_TAG.CACHE:
                    return isNew ? "Cache" : "/var";
                case UNIX_TAG.RESERVED:
                    return isNew ? "Reserved" : "/home";
                case UNIX_TAG.DUMP:
                    return "dump";
                case UNIX_TAG.NEW_ALT_ST:
                    return "Alternate sector track";
                case UNIX_TAG.VM_PUBLIC:
                    return "volume mgt public partition";
                case UNIX_TAG.VM_PRIVATE:
                    return "volume mgt private partition";
                default:
                    return string.Format("Unknown TAG: 0x{0:X4}", type);
            }
        }

    }
}