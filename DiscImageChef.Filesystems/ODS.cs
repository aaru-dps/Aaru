// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ODS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Files-11 On-Disk Structure and shows information.
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
using DiscImageChef.Console;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    // Information from VMS File System Internals by Kirby McCoy
    // ISBN: 1-55558-056-4
    // With some hints from http://www.decuslib.com/DECUS/vmslt97b/gnusoftware/gccaxp/7_1/vms/hm2def.h
    // Expects the home block to be always in sector #1 (does not check deltas)
    // Assumes a sector size of 512 bytes (VMS does on HDDs and optical drives, dunno about M.O.)
    // Book only describes ODS-2. Need to test ODS-1 and ODS-5
    // There is an ODS with signature "DECFILES11A", yet to be seen
    // Time is a 64 bit unsigned integer, tenths of microseconds since 1858/11/17 00:00:00.
    // TODO: Implement checksum
    public class ODS : IFilesystem
    {
        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public FileSystemType XmlFsType => xmlFsType;

        public Encoding Encoding => currentEncoding;
        public string Name => "Files-11 On-Disk Structure";
        public Guid Id => new Guid("de20633c-8021-4384-aeb0-83b0df14491f");

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End) return false;

            if(imagePlugin.Info.SectorSize < 512) return false;

            byte[] magicB = new byte[12];
            byte[] hbSector = imagePlugin.ReadSector(1 + partition.Start);

            Array.Copy(hbSector, 0x1F0, magicB, 0, 12);
            string magic = Encoding.ASCII.GetString(magicB);

            DicConsole.DebugWriteLine("Files-11 plugin", "magic: \"{0}\"", magic);

            if(magic == "DECFILE11A  " || magic == "DECFILE11B  ") return true;

            // Optical disc
            if(imagePlugin.Info.XmlMediaType != XmlMediaType.OpticalDisc) return false;

            if(hbSector.Length < 0x400) return false;

            hbSector = imagePlugin.ReadSector(partition.Start);

            Array.Copy(hbSector, 0x3F0, magicB, 0, 12);
            magic = Encoding.ASCII.GetString(magicB);

            DicConsole.DebugWriteLine("Files-11 plugin", "unaligned magic: \"{0}\"", magic);

            return magic == "DECFILE11A  " || magic == "DECFILE11B  ";
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
        {
            currentEncoding = encoding ?? Encoding.GetEncoding("iso-8859-1");
            information = "";

            StringBuilder sb = new StringBuilder();

            byte[] hbSector = imagePlugin.ReadSector(1 + partition.Start);

            GCHandle handle = GCHandle.Alloc(hbSector, GCHandleType.Pinned);
            OdsHomeBlock homeblock =
                (OdsHomeBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(OdsHomeBlock));
            handle.Free();

            // Optical disc
            if(imagePlugin.Info.XmlMediaType == XmlMediaType.OpticalDisc &&
               StringHandlers.CToString(homeblock.format) != "DECFILE11A  " &&
               StringHandlers.CToString(homeblock.format) != "DECFILE11B  ")
            {
                if(hbSector.Length < 0x400) return;

                byte[] tmp = imagePlugin.ReadSector(partition.Start);
                hbSector = new byte[0x200];
                Array.Copy(tmp, 0x200, hbSector, 0, 0x200);

                handle = GCHandle.Alloc(hbSector, GCHandleType.Pinned);
                homeblock = (OdsHomeBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(OdsHomeBlock));
                handle.Free();

                if(StringHandlers.CToString(homeblock.format) != "DECFILE11A  " &&
                   StringHandlers.CToString(homeblock.format) != "DECFILE11B  ") return;
            }

            if((homeblock.struclev & 0xFF00) != 0x0200 || (homeblock.struclev & 0xFF) != 1 ||
               StringHandlers.CToString(homeblock.format) != "DECFILE11B  ")
                sb.AppendLine("The following information may be incorrect for this volume.");
            if(homeblock.resfiles < 5 || homeblock.devtype != 0) sb.AppendLine("This volume may be corrupted.");

            sb.AppendFormat("Volume format is {0}",
                            StringHandlers.SpacePaddedToString(homeblock.format, currentEncoding)).AppendLine();
            sb.AppendFormat("Volume is Level {0} revision {1}", (homeblock.struclev & 0xFF00) >> 8,
                            homeblock.struclev & 0xFF).AppendLine();
            sb.AppendFormat("Lowest structure in the volume is Level {0}, revision {1}",
                            (homeblock.lowstruclev & 0xFF00) >> 8, homeblock.lowstruclev & 0xFF).AppendLine();
            sb.AppendFormat("Highest structure in the volume is Level {0}, revision {1}",
                            (homeblock.highstruclev & 0xFF00) >> 8, homeblock.highstruclev & 0xFF).AppendLine();
            sb.AppendFormat("{0} sectors per cluster ({1} bytes)", homeblock.cluster, homeblock.cluster * 512)
              .AppendLine();
            sb.AppendFormat("This home block is on sector {0} (VBN {1})", homeblock.homelbn, homeblock.homevbn)
              .AppendLine();
            sb.AppendFormat("Secondary home block is on sector {0} (VBN {1})", homeblock.alhomelbn, homeblock.alhomevbn)
              .AppendLine();
            sb.AppendFormat("Volume bitmap starts in sector {0} (VBN {1})", homeblock.ibmaplbn, homeblock.ibmapvbn)
              .AppendLine();
            sb.AppendFormat("Volume bitmap runs for {0} sectors ({1} bytes)", homeblock.ibmapsize,
                            homeblock.ibmapsize * 512).AppendLine();
            sb.AppendFormat("Backup INDEXF.SYS;1 is in sector {0} (VBN {1})", homeblock.altidxlbn, homeblock.altidxvbn)
              .AppendLine();
            sb.AppendFormat("{0} maximum files on the volume", homeblock.maxfiles).AppendLine();
            sb.AppendFormat("{0} reserved files", homeblock.resfiles).AppendLine();
            if(homeblock.rvn > 0 && homeblock.setcount > 0 &&
               StringHandlers.CToString(homeblock.strucname) != "            ")
                sb.AppendFormat("Volume is {0} of {1} in set \"{2}\".", homeblock.rvn, homeblock.setcount,
                                StringHandlers.SpacePaddedToString(homeblock.strucname, currentEncoding)).AppendLine();
            sb.AppendFormat("Volume owner is \"{0}\" (ID 0x{1:X8})",
                            StringHandlers.SpacePaddedToString(homeblock.ownername, currentEncoding),
                            homeblock.volowner).AppendLine();
            sb.AppendFormat("Volume label: \"{0}\"",
                            StringHandlers.SpacePaddedToString(homeblock.volname, currentEncoding)).AppendLine();
            sb.AppendFormat("Drive serial number: 0x{0:X8}", homeblock.serialnum).AppendLine();
            sb.AppendFormat("Volume was created on {0}", DateHandlers.VmsToDateTime(homeblock.credate)).AppendLine();
            if(homeblock.revdate > 0)
                sb.AppendFormat("Volume was last modified on {0}", DateHandlers.VmsToDateTime(homeblock.revdate))
                  .AppendLine();
            if(homeblock.copydate > 0)
                sb.AppendFormat("Volume copied on {0}", DateHandlers.VmsToDateTime(homeblock.copydate)).AppendLine();
            sb.AppendFormat("Checksums: 0x{0:X4} and 0x{1:X4}", homeblock.checksum1, homeblock.checksum2).AppendLine();
            sb.AppendLine("Flags:");
            sb.AppendFormat("Window: {0}", homeblock.window).AppendLine();
            sb.AppendFormat("Cached directores: {0}", homeblock.lru_lim).AppendLine();
            sb.AppendFormat("Default allocation: {0} blocks", homeblock.extend).AppendLine();
            if((homeblock.volchar & 0x01) == 0x01) sb.AppendLine("Readings should be verified");
            if((homeblock.volchar & 0x02) == 0x02) sb.AppendLine("Writings should be verified");
            if((homeblock.volchar & 0x04) == 0x04) sb.AppendLine("Files should be erased or overwritten when deleted");
            if((homeblock.volchar & 0x08) == 0x08) sb.AppendLine("Highwater mark is to be disabled");
            if((homeblock.volchar & 0x10) == 0x10) sb.AppendLine("Classification checks are enabled");
            sb.AppendLine("Volume permissions (r = read, w = write, c = create, d = delete)");
            sb.AppendLine("System, owner, group, world");
            // System
            sb.Append((homeblock.protect & 0x1000) == 0x1000 ? "-" : "r");
            sb.Append((homeblock.protect & 0x2000) == 0x2000 ? "-" : "w");
            sb.Append((homeblock.protect & 0x4000) == 0x4000 ? "-" : "c");
            sb.Append((homeblock.protect & 0x8000) == 0x8000 ? "-" : "d");
            // Owner
            sb.Append((homeblock.protect & 0x100) == 0x100 ? "-" : "r");
            sb.Append((homeblock.protect & 0x200) == 0x200 ? "-" : "w");
            sb.Append((homeblock.protect & 0x400) == 0x400 ? "-" : "c");
            sb.Append((homeblock.protect & 0x800) == 0x800 ? "-" : "d");
            // Group
            sb.Append((homeblock.protect & 0x10) == 0x10 ? "-" : "r");
            sb.Append((homeblock.protect & 0x20) == 0x20 ? "-" : "w");
            sb.Append((homeblock.protect & 0x40) == 0x40 ? "-" : "c");
            sb.Append((homeblock.protect & 0x80) == 0x80 ? "-" : "d");
            // World (other)
            sb.Append((homeblock.protect & 0x1) == 0x1 ? "-" : "r");
            sb.Append((homeblock.protect & 0x2) == 0x2 ? "-" : "w");
            sb.Append((homeblock.protect & 0x4) == 0x4 ? "-" : "c");
            sb.Append((homeblock.protect & 0x8) == 0x8 ? "-" : "d");

            sb.AppendLine();

            sb.AppendLine("Unknown structures:");
            sb.AppendFormat("Security mask: 0x{0:X8}", homeblock.sec_mask).AppendLine();
            sb.AppendFormat("File protection: 0x{0:X4}", homeblock.fileprot).AppendLine();
            sb.AppendFormat("Record protection: 0x{0:X4}", homeblock.recprot).AppendLine();

            xmlFsType = new FileSystemType
            {
                Type = "FILES-11",
                ClusterSize = homeblock.cluster * 512,
                Clusters = (long)partition.Size / (homeblock.cluster * 512),
                VolumeName = StringHandlers.SpacePaddedToString(homeblock.volname, currentEncoding),
                VolumeSerial = $"{homeblock.serialnum:X8}"
            };
            if(homeblock.credate > 0)
            {
                xmlFsType.CreationDate = DateHandlers.VmsToDateTime(homeblock.credate);
                xmlFsType.CreationDateSpecified = true;
            }
            if(homeblock.revdate > 0)
            {
                xmlFsType.ModificationDate = DateHandlers.VmsToDateTime(homeblock.revdate);
                xmlFsType.ModificationDateSpecified = true;
            }

            information = sb.ToString();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct OdsHomeBlock
        {
            /// <summary>0x000, LBN of THIS home block</summary>
            public uint homelbn;
            /// <summary>0x004, LBN of the secondary home block</summary>
            public uint alhomelbn;
            /// <summary>0x008, LBN of backup INDEXF.SYS;1</summary>
            public uint altidxlbn;
            /// <summary>0x00C, High byte contains filesystem version (1, 2 or 5), low byte contains revision (1)</summary>
            public ushort struclev;
            /// <summary>0x00E, Number of blocks each bit of the volume bitmap represents</summary>
            public ushort cluster;
            /// <summary>0x010, VBN of THIS home block</summary>
            public ushort homevbn;
            /// <summary>0x012, VBN of the secondary home block</summary>
            public ushort alhomevbn;
            /// <summary>0x014, VBN of backup INDEXF.SYS;1</summary>
            public ushort altidxvbn;
            /// <summary>0x016, VBN of the bitmap</summary>
            public ushort ibmapvbn;
            /// <summary>0x018, LBN of the bitmap</summary>
            public uint ibmaplbn;
            /// <summary>0x01C, Max files on volume</summary>
            public uint maxfiles;
            /// <summary>0x020, Bitmap size in sectors</summary>
            public ushort ibmapsize;
            /// <summary>0x022, Reserved files, 5 at minimum</summary>
            public ushort resfiles;
            /// <summary>0x024, Device type, ODS-2 defines it as always 0</summary>
            public ushort devtype;
            /// <summary>0x026, Relative volume number (number of the volume in a set)</summary>
            public ushort rvn;
            /// <summary>0x028, Total number of volumes in the set this volume is</summary>
            public ushort setcount;
            /// <summary>0x02A, Flags</summary>
            public ushort volchar;
            /// <summary>0x02C, User ID of the volume owner</summary>
            public uint volowner;
            /// <summary>0x030, Security mask (??)</summary>
            public uint sec_mask;
            /// <summary>0x034, Volume permissions (system, owner, group and other)</summary>
            public ushort protect;
            /// <summary>0x036, Default file protection, unsupported in ODS-2</summary>
            public ushort fileprot;
            /// <summary>0x038, Default file record protection</summary>
            public ushort recprot;
            /// <summary>0x03A, Checksum of all preceding entries</summary>
            public ushort checksum1;
            /// <summary>0x03C, Creation date</summary>
            public ulong credate;
            /// <summary>0x044, Window size (pointers for the window)</summary>
            public byte window;
            /// <summary>0x045, Directories to be stored in cache</summary>
            public byte lru_lim;
            /// <summary>0x046, Default allocation size in blocks</summary>
            public ushort extend;
            /// <summary>0x048, Minimum file retention period</summary>
            public ulong retainmin;
            /// <summary>0x050, Maximum file retention period</summary>
            public ulong retainmax;
            /// <summary>0x058, Last modification date</summary>
            public ulong revdate;
            /// <summary>0x060, Minimum security class, 20 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] min_class;
            /// <summary>0x074, Maximum security class, 20 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] max_class;
            /// <summary>0x088, File lookup table FID</summary>
            public ushort filetab_fid1;
            /// <summary>0x08A, File lookup table FID</summary>
            public ushort filetab_fid2;
            /// <summary>0x08C, File lookup table FID</summary>
            public ushort filetab_fid3;
            /// <summary>0x08E, Lowest structure level on the volume</summary>
            public ushort lowstruclev;
            /// <summary>0x090, Highest structure level on the volume</summary>
            public ushort highstruclev;
            /// <summary>0x092, Volume copy date (??)</summary>
            public ulong copydate;
            /// <summary>0x09A, 302 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 302)] public byte[] reserved1;
            /// <summary>0x1C8, Physical drive serial number</summary>
            public uint serialnum;
            /// <summary>0x1CC, Name of the volume set, 12 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] strucname;
            /// <summary>0x1D8, Volume label, 12 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] volname;
            /// <summary>0x1E4, Name of the volume owner, 12 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] ownername;
            /// <summary>0x1F0, ODS-2 defines it as "DECFILE11B", 12 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] format;
            /// <summary>0x1FC, Reserved</summary>
            public ushort reserved2;
            /// <summary>0x1FE, Checksum of preceding 255 words (16 bit units)</summary>
            public ushort checksum2;
        }
    }
}