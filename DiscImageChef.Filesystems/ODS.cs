/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : ODS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies VMS filesystems and shows information.
 
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

// Information from VMS File System Internals by Kirby McCoy
// ISBN: 1-55558-056-4
// With some hints from http://www.decuslib.com/DECUS/vmslt97b/gnusoftware/gccaxp/7_1/vms/hm2def.h
// Expects the home block to be always in sector #1 (does not check deltas)
// Assumes a sector size of 512 bytes (VMS does on HDDs and optical drives, dunno about M.O.)
// Book only describes ODS-2. Need to test ODS-1 and ODS-5
// There is an ODS with signature "DECFILES11A", yet to be seen
// Time is a 64 bit unsigned integer, tenths of microseconds since 1858/11/17 00:00:00.
// TODO: Implement checksum
namespace DiscImageChef.Plugins
{
    class ODS : Plugin
    {
        public ODS()
        {
            Name = "Files-11 On-Disk Structure";
            PluginUUID = new Guid("de20633c-8021-4384-aeb0-83b0df14491f");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if ((2 + partitionStart) >= imagePlugin.GetSectors())
                return false;

            if (imagePlugin.GetSectorSize() < 512)
                return false;

            byte[] magic_b = new byte[12];
            string magic;
            byte[] hb_sector = imagePlugin.ReadSector(1 + partitionStart);

            Array.Copy(hb_sector, 0x1F0, magic_b, 0, 12);
            magic = Encoding.ASCII.GetString(magic_b);
			
            return magic == "DECFILE11A  " || magic == "DECFILE11B  ";
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";
			
            StringBuilder sb = new StringBuilder();
            ODSHomeBlock homeblock = new ODSHomeBlock();
            byte[] temp_string = new byte[12];
            homeblock.min_class = new byte[20];
            homeblock.max_class = new byte[20];
			
            byte[] hb_sector = imagePlugin.ReadSector(1 + partitionStart);
			
            homeblock.homelbn = BitConverter.ToUInt32(hb_sector, 0x000);
            homeblock.alhomelbn = BitConverter.ToUInt32(hb_sector, 0x004);
            homeblock.altidxlbn = BitConverter.ToUInt32(hb_sector, 0x008);
            homeblock.struclev = BitConverter.ToUInt16(hb_sector, 0x00C);
            homeblock.cluster = BitConverter.ToUInt16(hb_sector, 0x00E);
            homeblock.homevbn = BitConverter.ToUInt16(hb_sector, 0x010);
            homeblock.alhomevbn = BitConverter.ToUInt16(hb_sector, 0x012);
            homeblock.altidxvbn = BitConverter.ToUInt16(hb_sector, 0x014);
            homeblock.ibmapvbn = BitConverter.ToUInt16(hb_sector, 0x016);
            homeblock.ibmaplbn = BitConverter.ToUInt32(hb_sector, 0x018);
            homeblock.maxfiles = BitConverter.ToUInt32(hb_sector, 0x01C);
            homeblock.ibmapsize = BitConverter.ToUInt16(hb_sector, 0x020);
            homeblock.resfiles = BitConverter.ToUInt16(hb_sector, 0x022);
            homeblock.devtype = BitConverter.ToUInt16(hb_sector, 0x024);
            homeblock.rvn = BitConverter.ToUInt16(hb_sector, 0x026);
            homeblock.setcount = BitConverter.ToUInt16(hb_sector, 0x028);
            homeblock.volchar = BitConverter.ToUInt16(hb_sector, 0x02A);
            homeblock.volowner = BitConverter.ToUInt32(hb_sector, 0x02C);
            homeblock.sec_mask = BitConverter.ToUInt32(hb_sector, 0x030);
            homeblock.protect = BitConverter.ToUInt16(hb_sector, 0x034);
            homeblock.fileprot = BitConverter.ToUInt16(hb_sector, 0x036);
            homeblock.recprot = BitConverter.ToUInt16(hb_sector, 0x038);
            homeblock.checksum1 = BitConverter.ToUInt16(hb_sector, 0x03A);
            homeblock.credate = BitConverter.ToUInt64(hb_sector, 0x03C);
            homeblock.window = hb_sector[0x044];
            homeblock.lru_lim = hb_sector[0x045];
            homeblock.extend = BitConverter.ToUInt16(hb_sector, 0x046);
            homeblock.retainmin = BitConverter.ToUInt64(hb_sector, 0x048);
            homeblock.retainmax = BitConverter.ToUInt64(hb_sector, 0x050);
            homeblock.revdate = BitConverter.ToUInt64(hb_sector, 0x058);
            Array.Copy(hb_sector, 0x060, homeblock.min_class, 0, 20);
            Array.Copy(hb_sector, 0x074, homeblock.max_class, 0, 20);
            homeblock.filetab_fid1 = BitConverter.ToUInt16(hb_sector, 0x088);
            homeblock.filetab_fid2 = BitConverter.ToUInt16(hb_sector, 0x08A);
            homeblock.filetab_fid3 = BitConverter.ToUInt16(hb_sector, 0x08C);
            homeblock.lowstruclev = BitConverter.ToUInt16(hb_sector, 0x08E);
            homeblock.highstruclev = BitConverter.ToUInt16(hb_sector, 0x090);
            homeblock.copydate = BitConverter.ToUInt64(hb_sector, 0x092);
            homeblock.serialnum = BitConverter.ToUInt32(hb_sector, 0x1C8);
            Array.Copy(hb_sector, 0x1CC, temp_string, 0, 12);
            homeblock.strucname = StringHandlers.CToString(temp_string);
            Array.Copy(hb_sector, 0x1D8, temp_string, 0, 12);
            homeblock.volname = StringHandlers.CToString(temp_string);
            Array.Copy(hb_sector, 0x1E4, temp_string, 0, 12);
            homeblock.ownername = StringHandlers.CToString(temp_string);
            Array.Copy(hb_sector, 0x1F0, temp_string, 0, 12);
            homeblock.format = StringHandlers.CToString(temp_string);
            homeblock.checksum2 = BitConverter.ToUInt16(hb_sector, 0x1FE);
			
            if ((homeblock.struclev & 0xFF00) != 0x0200 || (homeblock.struclev & 0xFF) != 1 || homeblock.format != "DECFILE11B  ")
                sb.AppendLine("The following information may be incorrect for this volume.");
            if (homeblock.resfiles < 5 || homeblock.devtype != 0)
                sb.AppendLine("This volume may be corrupted.");
			
            sb.AppendFormat("Volume format is {0}", homeblock.format).AppendLine();
            sb.AppendFormat("Volume is Level {0} revision {1}", (homeblock.struclev & 0xFF00) >> 8, homeblock.struclev & 0xFF).AppendLine();
            sb.AppendFormat("Lowest structure in the volume is Level {0}, revision {1}", (homeblock.lowstruclev & 0xFF00) >> 8, homeblock.lowstruclev & 0xFF).AppendLine();
            sb.AppendFormat("Highest structure in the volume is Level {0}, revision {1}", (homeblock.highstruclev & 0xFF00) >> 8, homeblock.highstruclev & 0xFF).AppendLine();
            sb.AppendFormat("{0} sectors per cluster ({1} bytes)", homeblock.cluster, homeblock.cluster * 512).AppendLine();
            sb.AppendFormat("This home block is on sector {0} (cluster {1})", homeblock.homelbn, homeblock.homevbn).AppendLine();
            sb.AppendFormat("Secondary home block is on sector {0} (cluster {1})", homeblock.alhomelbn, homeblock.alhomevbn).AppendLine();
            sb.AppendFormat("Volume bitmap starts in sector {0} (cluster {1})", homeblock.ibmaplbn, homeblock.ibmapvbn).AppendLine();
            sb.AppendFormat("Volume bitmap runs for {0} sectors ({1} bytes)", homeblock.ibmapsize, homeblock.ibmapsize * 512).AppendLine();
            sb.AppendFormat("Backup INDEXF.SYS;1 is in sector {0} (cluster {1})", homeblock.altidxlbn, homeblock.altidxvbn).AppendLine();
            sb.AppendFormat("{0} maximum files on the volume", homeblock.maxfiles).AppendLine();
            sb.AppendFormat("{0} reserved files", homeblock.resfiles).AppendLine();
            if (homeblock.rvn > 0 && homeblock.setcount > 0 && homeblock.strucname != "            ")
                sb.AppendFormat("Volume is {0} of {1} in set \"{2}\".", homeblock.rvn, homeblock.setcount, homeblock.strucname).AppendLine();
            sb.AppendFormat("Volume owner is \"{0}\" (ID 0x{1:X8})", homeblock.ownername, homeblock.volowner).AppendLine();
            sb.AppendFormat("Volume label: \"{0}\"", homeblock.volname).AppendLine();
            sb.AppendFormat("Drive serial number: 0x{0:X8}", homeblock.serialnum).AppendLine();
            sb.AppendFormat("Volume was created on {0}", DateHandlers.VMSToDateTime(homeblock.credate)).AppendLine();
            if (homeblock.revdate > 0)
                sb.AppendFormat("Volume was last modified on {0}", DateHandlers.VMSToDateTime(homeblock.revdate)).AppendLine();
            if (homeblock.copydate > 0)
                sb.AppendFormat("Volume copied on {0}", DateHandlers.VMSToDateTime(homeblock.copydate)).AppendLine();
            sb.AppendFormat("Checksums: 0x{0:X4} and 0x{1:X4}", homeblock.checksum1, homeblock.checksum2).AppendLine();
            sb.AppendLine("Flags:");
            sb.AppendFormat("Window: {0}", homeblock.window).AppendLine();
            sb.AppendFormat("Cached directores: {0}", homeblock.lru_lim).AppendLine();
            sb.AppendFormat("Default allocation: {0} blocks", homeblock.extend).AppendLine();
            if ((homeblock.volchar & 0x01) == 0x01)
                sb.AppendLine("Readings should be verified");
            if ((homeblock.volchar & 0x02) == 0x02)
                sb.AppendLine("Writings should be verified");
            if ((homeblock.volchar & 0x04) == 0x04)
                sb.AppendLine("Files should be erased or overwritten when deleted");
            if ((homeblock.volchar & 0x08) == 0x08)
                sb.AppendLine("Highwater mark is to be disabled");
            if ((homeblock.volchar & 0x10) == 0x10)
                sb.AppendLine("Classification checks are enabled");
            sb.AppendLine("Volume permissions (r = read, w = write, c = create, d = delete)");
            sb.AppendLine("System, owner, group, world");
            // System
            if ((homeblock.protect & 0x1000) == 0x1000)
                sb.Append("-");
            else
                sb.Append("r");
            if ((homeblock.protect & 0x2000) == 0x2000)
                sb.Append("-");
            else
                sb.Append("w");
            if ((homeblock.protect & 0x4000) == 0x4000)
                sb.Append("-");
            else
                sb.Append("c");
            if ((homeblock.protect & 0x8000) == 0x8000)
                sb.Append("-");
            else
                sb.Append("d");
            // Owner
            if ((homeblock.protect & 0x100) == 0x100)
                sb.Append("-");
            else
                sb.Append("r");
            if ((homeblock.protect & 0x200) == 0x200)
                sb.Append("-");
            else
                sb.Append("w");
            if ((homeblock.protect & 0x400) == 0x400)
                sb.Append("-");
            else
                sb.Append("c");
            if ((homeblock.protect & 0x800) == 0x800)
                sb.Append("-");
            else
                sb.Append("d");
            // Group
            if ((homeblock.protect & 0x10) == 0x10)
                sb.Append("-");
            else
                sb.Append("r");
            if ((homeblock.protect & 0x20) == 0x20)
                sb.Append("-");
            else
                sb.Append("w");
            if ((homeblock.protect & 0x40) == 0x40)
                sb.Append("-");
            else
                sb.Append("c");
            if ((homeblock.protect & 0x80) == 0x80)
                sb.Append("-");
            else
                sb.Append("d");
            // World (other)
            if ((homeblock.protect & 0x1) == 0x1)
                sb.Append("-");
            else
                sb.Append("r");
            if ((homeblock.protect & 0x2) == 0x2)
                sb.Append("-");
            else
                sb.Append("w");
            if ((homeblock.protect & 0x4) == 0x4)
                sb.Append("-");
            else
                sb.Append("c");
            if ((homeblock.protect & 0x8) == 0x8)
                sb.Append("-");
            else
                sb.Append("d");
			
            sb.AppendLine();
			
            sb.AppendLine("Unknown structures:");
            sb.AppendFormat("Security mask: 0x{0:X8}", homeblock.sec_mask).AppendLine();
            sb.AppendFormat("File protection: 0x{0:X4}", homeblock.fileprot).AppendLine();
            sb.AppendFormat("Record protection: 0x{0:X4}", homeblock.recprot).AppendLine();
			
            information = sb.ToString();
        }

        struct ODSHomeBlock
        {
            public UInt32 homelbn;
            // 0x000, LBN of THIS home block
            public UInt32 alhomelbn;
            // 0x004, LBN of the secondary home block
            public UInt32 altidxlbn;
            // 0x008, LBN of backup INDEXF.SYS;1
            public UInt16 struclev;
            // 0x00C, High byte contains filesystem version (1, 2 or 5), low byte contains revision (1)
            public UInt16 cluster;
            // 0x00E, Number of blocks each bit of the volume bitmap represents
            public UInt16 homevbn;
            // 0x010, VBN of THIS home block
            public UInt16 alhomevbn;
            // 0x012, VBN of the secondary home block
            public UInt16 altidxvbn;
            // 0x014, VBN of backup INDEXF.SYS;1
            public UInt16 ibmapvbn;
            // 0x016, VBN of the bitmap
            public UInt32 ibmaplbn;
            // 0x018, LBN of the bitmap
            public UInt32 maxfiles;
            // 0x01C, Max files on volume
            public UInt16 ibmapsize;
            // 0x020, Bitmap size in sectors
            public UInt16 resfiles;
            // 0x022, Reserved files, 5 at minimum
            public UInt16 devtype;
            // 0x024, Device type, ODS-2 defines it as always 0
            public UInt16 rvn;
            // 0x026, Relative volume number (number of the volume in a set)
            public UInt16 setcount;
            // 0x028, Total number of volumes in the set this volume is
            public UInt16 volchar;
            // 0x02A, Flags
            public UInt32 volowner;
            // 0x02C, User ID of the volume owner
            public UInt32 sec_mask;
            // 0x030, Security mask (??)
            public UInt16 protect;
            // 0x034, Volume permissions (system, owner, group and other)
            public UInt16 fileprot;
            // 0x036, Default file protection, unsupported in ODS-2
            public UInt16 recprot;
            // 0x038, Default file record protection
            public UInt16 checksum1;
            // 0x03A, Checksum of all preceding entries
            public UInt64 credate;
            // 0x03C, Creation date
            public byte window;
            // 0x044, Window size (pointers for the window)
            public byte lru_lim;
            // 0x045, Directories to be stored in cache
            public UInt16 extend;
            // 0x046, Default allocation size in blocks
            public UInt64 retainmin;
            // 0x048, Minimum file retention period
            public UInt64 retainmax;
            // 0x050, Maximum file retention period
            public UInt64 revdate;
            // 0x058, Last modification date
            public byte[] min_class;
            // 0x060, Minimum security class, 20 bytes
            public byte[] max_class;
            // 0x074, Maximum security class, 20 bytes
            public UInt16 filetab_fid1;
            // 0x088, File lookup table FID
            public UInt16 filetab_fid2;
            // 0x08A, File lookup table FID
            public UInt16 filetab_fid3;
            // 0x08C, File lookup table FID
            public UInt16 lowstruclev;
            // 0x08E, Lowest structure level on the volume
            public UInt16 highstruclev;
            // 0x090, Highest structure level on the volume
            public UInt64 copydate;
            // 0x092, Volume copy date (??)
            public byte[] reserved1;
            // 0x09A, 302 bytes
            public UInt32 serialnum;
            // 0x1C8, Physical drive serial number
            public string strucname;
            // 0x1CC, Name of the volume set, 12 bytes
            public string volname;
            // 0x1D8, Volume label, 12 bytes
            public string ownername;
            // 0x1E4, Name of the volume owner, 12 bytes
            public string format;
            // 0x1F0, ODS-2 defines it as "DECFILE11B", 12 bytes
            public UInt16 reserved2;
            // 0x1FC, Reserved
            public UInt16 checksum2;
            // 0x1FE, Checksum of preceding 255 words (16 bit units)
        }
    }
}