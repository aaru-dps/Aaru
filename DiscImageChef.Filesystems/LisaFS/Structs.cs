// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
namespace DiscImageChef.Filesystems.LisaFS
{
    partial class LisaFS : Filesystem
    {
        struct Lisa_MDDF
        {
            /// <summary>0x00, Filesystem version</summary>
            public UInt16 fsversion;
            /// <summary>0x02, Volume ID</summary>
            public UInt64 volid;
            /// <summary>0x0A, Volume sequence number</summary>
            public UInt16 volnum;
            /// <summary>0x0C, Pascal string, 32+1 bytes, volume name</summary>
            public string volname;
            /// <summary>0x2D, unknown, possible padding</summary>
            public byte unknown1;
            /// <summary>0x2E, Pascal string, 32+1 bytes, password</summary>
            public string password;
            /// <summary>0x4F, unknown, possible padding</summary>
            public byte unknown2;
            /// <summary>0x50, Lisa serial number that init'ed this disk</summary>
            public UInt32 machine_id;
            /// <summary>0x54, ID of the master copy ? no idea really</summary>
            public UInt32 master_copy_id;
            /// <summary>0x58, Date of volume creation</summary>
            public DateTime dtvc;
            /// <summary>0x5C, Date...</summary>
            public DateTime dtcc;
            /// <summary>0x60, Date of volume backup</summary>
            public DateTime dtvb;
            /// <summary>0x64, Date of volume scavenging</summary>
            public DateTime dtvs;
            /// <summary>0x68, unknown</summary>
            public UInt32 unknown3;
            /// <summary>0x6C, block the MDDF is residing on</summary>
            public UInt32 mddf_block;
            /// <summary>0x70, volsize-1</summary>
            public UInt32 volsize_minus_one;
            /// <summary>0x74, volsize-1-mddf_block</summary>
            public UInt32 volsize_minus_mddf_minus_one;
            /// <summary>0x78, Volume size in blocks</summary>
            public UInt32 vol_size;
            /// <summary>0x7C, Blocks size of underlying drive (data+tags)</summary>
            public UInt16 blocksize;
            /// <summary>0x7E, Data only block size</summary>
            public UInt16 datasize;
            /// <summary>0x80, unknown</summary>
            public UInt16 unknown4;
            /// <summary>0x82, unknown</summary>
            public UInt32 unknown5;
            /// <summary>0x86, unknown</summary>
            public UInt32 unknown6;
            /// <summary>0x8A, Size in sectors of filesystem clusters</summary>
            public UInt16 clustersize;
            /// <summary>0x8C, Filesystem size in blocks</summary>
            public UInt32 fs_size;
            /// <summary>0x90, unknown</summary>
            public UInt32 unknown7;
            /// <summary>0x94, unknown</summary>
            public UInt32 unknown8;
            /// <summary>0x98, unknown</summary>
            public UInt32 unknown9;
            /// <summary>0x9C, unknown</summary>
            public UInt32 unknown10;
            /// <summary>0xA0, unknown</summary>
            public UInt32 unknown11;
            /// <summary>0xA4, unknown</summary>
            public UInt32 unknown12;
            /// <summary>0xA8, unknown</summary>
            public UInt32 unknown13;
            /// <summary>0xAC, unknown</summary>
            public UInt32 unknown14;
            /// <summary>0xB0, Files in volume</summary>
            public UInt16 filecount;
            /// <summary>0xB2, unknown</summary>
            public UInt32 unknown15;
            /// <summary>0xB6, unknown</summary>
            public UInt32 unknown16;
            /// <summary>0xBA, Free blocks</summary>
            public UInt32 freecount;
            /// <summary>0xBE, unknown</summary>
            public UInt16 unknown17;
            /// <summary>0xC0, unknown</summary>
            public UInt32 unknown18;
            /// <summary>0xC4, no idea</summary>
            public UInt64 overmount_stamp;
            /// <summary>0xCC, serialization, lisa serial number authorized to use blocked software on this volume</summary>
            public UInt32 serialization;
            /// <summary>0xD0, unknown</summary>
            public UInt32 unknown19;
            /// <summary>0xD4, unknown, possible timestamp</summary>
            public UInt32 unknown_timestamp;
            /// <summary>0xD8, unknown</summary>
            public UInt32 unknown20;
            /// <summary>0xDC, unknown</summary>
            public UInt32 unknown21;
            /// <summary>0xE0, unknown</summary>
            public UInt32 unknown22;
            /// <summary>0xE4, unknown</summary>
            public UInt32 unknown23;
            /// <summary>0xE8, unknown</summary>
            public UInt32 unknown24;
            /// <summary>0xEC, unknown</summary>
            public UInt32 unknown25;
            /// <summary>0xF0, unknown</summary>
            public UInt32 unknown26;
            /// <summary>0xF4, unknown</summary>
            public UInt32 unknown27;
            /// <summary>0xF8, unknown</summary>
            public UInt32 unknown28;
            /// <summary>0xFC, unknown</summary>
            public UInt32 unknown29;
            /// <summary>0x100, unknown</summary>
            public UInt32 unknown30;
            /// <summary>0x104, unknown</summary>
            public UInt32 unknown31;
            /// <summary>0x108, unknown</summary>
            public UInt32 unknown32;
            /// <summary>0x10C, unknown</summary>
            public UInt32 unknown33;
            /// <summary>0x110, unknown</summary>
            public UInt32 unknown34;
            /// <summary>0x114, unknown</summary>
            public UInt32 unknown35;
            /// <summary>0x118, ID of volume where this volume was backed up</summary>
            public UInt64 backup_volid;
            /// <summary>0x120, Size of LisaInfo label</summary>
            public UInt16 label_size;
            /// <summary>0x122, not clear</summary>
            public UInt16 fs_overhead;
            /// <summary>0x124, Return code of Scavenger</summary>
            public UInt16 result_scavenge;
            /// <summary>0x126, No idea</summary>
            public UInt16 boot_code;
            /// <summary>0x128, No idea</summary>
            public UInt16 boot_environ;
            /// <summary>0x12A, unknown</summary>
            public UInt32 unknown36;
            /// <summary>0x12E, unknown</summary>
            public UInt32 unknown37;
            /// <summary>0x132, unknown</summary>
            public UInt32 unknown38;
            /// <summary>0x136, Total volumes in sequence</summary>
            public UInt16 vol_sequence;
            /// <summary>0x138, Volume is dirty?</summary>
            public byte vol_left_mounted;
            /// <summary>Is password present? (On-disk position unknown)</summary>
            public byte passwd_present;
            /// <summary>Opened files (memory-only?) (On-disk position unknown)</summary>
            public UInt32 opencount;
            /// <summary>No idea (On-disk position unknown)</summary>
            public UInt32 copy_thread;
            // Flags are boolean, but Pascal seems to use them as full unsigned 8 bit values
            /// <summary>No idea (On-disk position unknown)</summary>
            public byte privileged;
            /// <summary>Read-only volume (On-disk position unknown)</summary>
            public byte write_protected;
            /// <summary>Master disk (On-disk position unknown)</summary>
            public byte master;
            /// <summary>Copy disk (On-disk position unknown)</summary>
            public byte copy;
            /// <summary>No idea (On-disk position unknown)</summary>
            public byte copy_flag;
            /// <summary>No idea (On-disk position unknown)</summary>
            public byte scavenge_flag;
        }

        struct Lisa_Tag
        {
            /// <summary>0x00 Unknown</summary>
            public UInt32 unknown1;
            /// <summary>0x04 File ID</summary>
            public UInt16 fileID;
            /// <summary>0x06 Unknown</summary>
            public UInt16 unknown2;
            /// <summary>0x08 Unknown</summary>
            public UInt32 unknown3;
        }
    }
}

