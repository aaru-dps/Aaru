// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Filesystems;

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

// Information from VMS File System Internals by Kirby McCoy
// ISBN: 1-55558-056-4
// With some hints from http://www.decuslib.com/DECUS/vmslt97b/gnusoftware/gccaxp/7_1/vms/hm2def.h
// Expects the home block to be always in sector #1 (does not check deltas)
// Assumes a sector size of 512 bytes (VMS does on HDDs and optical drives, dunno about M.O.)
// Book only describes ODS-2. Need to test ODS-1 and ODS-5
// There is an ODS with signature "DECFILES11A", yet to be seen
// Time is a 64 bit unsigned integer, tenths of microseconds since 1858/11/17 00:00:00.
// TODO: Implement checksum
/// <inheritdoc />
/// <summary>Implements detection of DEC's On-Disk Structure, aka the ODS filesystem</summary>
public sealed class ODS : IFilesystem
{
    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => "Files-11 On-Disk Structure";
    /// <inheritdoc />
    public Guid Id => new("de20633c-8021-4384-aeb0-83b0df14491f");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        if(imagePlugin.Info.SectorSize < 512)
            return false;

        var         magicB = new byte[12];
        ErrorNumber errno  = imagePlugin.ReadSector(1 + partition.Start, out byte[] hbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        Array.Copy(hbSector, 0x1F0, magicB, 0, 12);
        string magic = Encoding.ASCII.GetString(magicB);

        AaruConsole.DebugWriteLine("Files-11 plugin", "magic: \"{0}\"", magic);

        if(magic is "DECFILE11A  " or "DECFILE11B  ")
            return true;

        // Optical disc
        if(imagePlugin.Info.XmlMediaType != XmlMediaType.OpticalDisc)
            return false;

        if(hbSector.Length < 0x400)
            return false;

        errno = imagePlugin.ReadSector(partition.Start, out hbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        Array.Copy(hbSector, 0x3F0, magicB, 0, 12);
        magic = Encoding.ASCII.GetString(magicB);

        AaruConsole.DebugWriteLine("Files-11 plugin", "unaligned magic: \"{0}\"", magic);

        return magic is "DECFILE11A  " or "DECFILE11B  ";
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-1");
        information = "";

        var sb = new StringBuilder();

        ErrorNumber errno = imagePlugin.ReadSector(1 + partition.Start, out byte[] hbSector);

        if(errno != ErrorNumber.NoError)
            return;

        HomeBlock homeblock = Marshal.ByteArrayToStructureLittleEndian<HomeBlock>(hbSector);

        // Optical disc
        if(imagePlugin.Info.XmlMediaType              == XmlMediaType.OpticalDisc &&
           StringHandlers.CToString(homeblock.format) != "DECFILE11A  "           &&
           StringHandlers.CToString(homeblock.format) != "DECFILE11B  ")
        {
            if(hbSector.Length < 0x400)
                return;

            errno = imagePlugin.ReadSector(partition.Start, out byte[] tmp);

            if(errno != ErrorNumber.NoError)
                return;

            hbSector = new byte[0x200];
            Array.Copy(tmp, 0x200, hbSector, 0, 0x200);

            homeblock = Marshal.ByteArrayToStructureLittleEndian<HomeBlock>(hbSector);

            if(StringHandlers.CToString(homeblock.format) != "DECFILE11A  " &&
               StringHandlers.CToString(homeblock.format) != "DECFILE11B  ")
                return;
        }

        if((homeblock.struclev & 0xFF00)              != 0x0200 ||
           (homeblock.struclev & 0xFF)                != 1      ||
           StringHandlers.CToString(homeblock.format) != "DECFILE11B  ")
            sb.AppendLine("The following information may be incorrect for this volume.");

        if(homeblock.resfiles < 5 ||
           homeblock.devtype  != 0)
            sb.AppendLine("This volume may be corrupted.");

        sb.AppendFormat("Volume format is {0}", StringHandlers.SpacePaddedToString(homeblock.format, Encoding)).
           AppendLine();

        sb.AppendFormat("Volume is Level {0} revision {1}", (homeblock.struclev & 0xFF00) >> 8,
                        homeblock.struclev & 0xFF).AppendLine();

        sb.AppendFormat("Lowest structure in the volume is Level {0}, revision {1}",
                        (homeblock.lowstruclev & 0xFF00) >> 8, homeblock.lowstruclev & 0xFF).AppendLine();

        sb.AppendFormat("Highest structure in the volume is Level {0}, revision {1}",
                        (homeblock.highstruclev & 0xFF00) >> 8, homeblock.highstruclev & 0xFF).AppendLine();

        sb.AppendFormat("{0} sectors per cluster ({1} bytes)", homeblock.cluster, homeblock.cluster * 512).AppendLine();

        sb.AppendFormat("This home block is on sector {0} (VBN {1})", homeblock.homelbn, homeblock.homevbn).
           AppendLine();

        sb.AppendFormat("Secondary home block is on sector {0} (VBN {1})", homeblock.alhomelbn, homeblock.alhomevbn).
           AppendLine();

        sb.AppendFormat("Volume bitmap starts in sector {0} (VBN {1})", homeblock.ibmaplbn, homeblock.ibmapvbn).
           AppendLine();

        sb.AppendFormat("Volume bitmap runs for {0} sectors ({1} bytes)", homeblock.ibmapsize,
                        homeblock.ibmapsize * 512).AppendLine();

        sb.AppendFormat("Backup INDEXF.SYS;1 is in sector {0} (VBN {1})", homeblock.altidxlbn, homeblock.altidxvbn).
           AppendLine();

        sb.AppendFormat("{0} maximum files on the volume", homeblock.maxfiles).AppendLine();
        sb.AppendFormat("{0} reserved files", homeblock.resfiles).AppendLine();

        if(homeblock.rvn                                 > 0 &&
           homeblock.setcount                            > 0 &&
           StringHandlers.CToString(homeblock.strucname) != "            ")
            sb.AppendFormat("Volume is {0} of {1} in set \"{2}\".", homeblock.rvn, homeblock.setcount,
                            StringHandlers.SpacePaddedToString(homeblock.strucname, Encoding)).AppendLine();

        sb.AppendFormat("Volume owner is \"{0}\" (ID 0x{1:X8})",
                        StringHandlers.SpacePaddedToString(homeblock.ownername, Encoding), homeblock.volowner).
           AppendLine();

        sb.AppendFormat("Volume label: \"{0}\"", StringHandlers.SpacePaddedToString(homeblock.volname, Encoding)).
           AppendLine();

        sb.AppendFormat("Drive serial number: 0x{0:X8}", homeblock.serialnum).AppendLine();
        sb.AppendFormat("Volume was created on {0}", DateHandlers.VmsToDateTime(homeblock.credate)).AppendLine();

        if(homeblock.revdate > 0)
            sb.AppendFormat("Volume was last modified on {0}", DateHandlers.VmsToDateTime(homeblock.revdate)).
               AppendLine();

        if(homeblock.copydate > 0)
            sb.AppendFormat("Volume copied on {0}", DateHandlers.VmsToDateTime(homeblock.copydate)).AppendLine();

        sb.AppendFormat("Checksums: 0x{0:X4} and 0x{1:X4}", homeblock.checksum1, homeblock.checksum2).AppendLine();
        sb.AppendLine("Flags:");
        sb.AppendFormat("Window: {0}", homeblock.window).AppendLine();
        sb.AppendFormat("Cached directores: {0}", homeblock.lru_lim).AppendLine();
        sb.AppendFormat("Default allocation: {0} blocks", homeblock.extend).AppendLine();

        if((homeblock.volchar & 0x01) == 0x01)
            sb.AppendLine("Readings should be verified");

        if((homeblock.volchar & 0x02) == 0x02)
            sb.AppendLine("Writings should be verified");

        if((homeblock.volchar & 0x04) == 0x04)
            sb.AppendLine("Files should be erased or overwritten when deleted");

        if((homeblock.volchar & 0x08) == 0x08)
            sb.AppendLine("Highwater mark is to be disabled");

        if((homeblock.volchar & 0x10) == 0x10)
            sb.AppendLine("Classification checks are enabled");

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

        XmlFsType = new FileSystemType
        {
            Type         = "FILES-11",
            ClusterSize  = (uint)(homeblock.cluster * 512),
            Clusters     = partition.Size / (ulong)(homeblock.cluster * 512),
            VolumeName   = StringHandlers.SpacePaddedToString(homeblock.volname, Encoding),
            VolumeSerial = $"{homeblock.serialnum:X8}"
        };

        if(homeblock.credate > 0)
        {
            XmlFsType.CreationDate          = DateHandlers.VmsToDateTime(homeblock.credate);
            XmlFsType.CreationDateSpecified = true;
        }

        if(homeblock.revdate > 0)
        {
            XmlFsType.ModificationDate          = DateHandlers.VmsToDateTime(homeblock.revdate);
            XmlFsType.ModificationDateSpecified = true;
        }

        information = sb.ToString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HomeBlock
    {
        /// <summary>0x000, LBN of THIS home block</summary>
        public readonly uint homelbn;
        /// <summary>0x004, LBN of the secondary home block</summary>
        public readonly uint alhomelbn;
        /// <summary>0x008, LBN of backup INDEXF.SYS;1</summary>
        public readonly uint altidxlbn;
        /// <summary>0x00C, High byte contains filesystem version (1, 2 or 5), low byte contains revision (1)</summary>
        public readonly ushort struclev;
        /// <summary>0x00E, Number of blocks each bit of the volume bitmap represents</summary>
        public readonly ushort cluster;
        /// <summary>0x010, VBN of THIS home block</summary>
        public readonly ushort homevbn;
        /// <summary>0x012, VBN of the secondary home block</summary>
        public readonly ushort alhomevbn;
        /// <summary>0x014, VBN of backup INDEXF.SYS;1</summary>
        public readonly ushort altidxvbn;
        /// <summary>0x016, VBN of the bitmap</summary>
        public readonly ushort ibmapvbn;
        /// <summary>0x018, LBN of the bitmap</summary>
        public readonly uint ibmaplbn;
        /// <summary>0x01C, Max files on volume</summary>
        public readonly uint maxfiles;
        /// <summary>0x020, Bitmap size in sectors</summary>
        public readonly ushort ibmapsize;
        /// <summary>0x022, Reserved files, 5 at minimum</summary>
        public readonly ushort resfiles;
        /// <summary>0x024, Device type, ODS-2 defines it as always 0</summary>
        public readonly ushort devtype;
        /// <summary>0x026, Relative volume number (number of the volume in a set)</summary>
        public readonly ushort rvn;
        /// <summary>0x028, Total number of volumes in the set this volume is</summary>
        public readonly ushort setcount;
        /// <summary>0x02A, Flags</summary>
        public readonly ushort volchar;
        /// <summary>0x02C, User ID of the volume owner</summary>
        public readonly uint volowner;
        /// <summary>0x030, Security mask (??)</summary>
        public readonly uint sec_mask;
        /// <summary>0x034, Volume permissions (system, owner, group and other)</summary>
        public readonly ushort protect;
        /// <summary>0x036, Default file protection, unsupported in ODS-2</summary>
        public readonly ushort fileprot;
        /// <summary>0x038, Default file record protection</summary>
        public readonly ushort recprot;
        /// <summary>0x03A, Checksum of all preceding entries</summary>
        public readonly ushort checksum1;
        /// <summary>0x03C, Creation date</summary>
        public readonly ulong credate;
        /// <summary>0x044, Window size (pointers for the window)</summary>
        public readonly byte window;
        /// <summary>0x045, Directories to be stored in cache</summary>
        public readonly byte lru_lim;
        /// <summary>0x046, Default allocation size in blocks</summary>
        public readonly ushort extend;
        /// <summary>0x048, Minimum file retention period</summary>
        public readonly ulong retainmin;
        /// <summary>0x050, Maximum file retention period</summary>
        public readonly ulong retainmax;
        /// <summary>0x058, Last modification date</summary>
        public readonly ulong revdate;
        /// <summary>0x060, Minimum security class, 20 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] min_class;
        /// <summary>0x074, Maximum security class, 20 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] max_class;
        /// <summary>0x088, File lookup table FID</summary>
        public readonly ushort filetab_fid1;
        /// <summary>0x08A, File lookup table FID</summary>
        public readonly ushort filetab_fid2;
        /// <summary>0x08C, File lookup table FID</summary>
        public readonly ushort filetab_fid3;
        /// <summary>0x08E, Lowest structure level on the volume</summary>
        public readonly ushort lowstruclev;
        /// <summary>0x090, Highest structure level on the volume</summary>
        public readonly ushort highstruclev;
        /// <summary>0x092, Volume copy date (??)</summary>
        public readonly ulong copydate;
        /// <summary>0x09A, 302 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 302)]
        public readonly byte[] reserved1;
        /// <summary>0x1C8, Physical drive serial number</summary>
        public readonly uint serialnum;
        /// <summary>0x1CC, Name of the volume set, 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] strucname;
        /// <summary>0x1D8, Volume label, 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] volname;
        /// <summary>0x1E4, Name of the volume owner, 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] ownername;
        /// <summary>0x1F0, ODS-2 defines it as "DECFILE11B", 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] format;
        /// <summary>0x1FC, Reserved</summary>
        public readonly ushort reserved2;
        /// <summary>0x1FE, Checksum of preceding 255 words (16 bit units)</summary>
        public readonly ushort checksum2;
    }
}