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
        struct MDDF
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

        struct Tag
        {
            /// <summary>0x00 version</summary>
            public UInt16 version;
            /// <summary>0x02 unknown</summary>
            public UInt16 unknown;
            /// <summary>0x04 File ID. Negative numbers are extents for the file with same absolute value number</summary>
            public Int16 fileID;
            /// <summary>Only in 20 bytes tag at 0x06, mask 0x8000 if valid tag</summary>
            public UInt16 usedBytes;
            /// <summary>Only in 20 bytes tag at 0x08, 3 bytes</summary>
            public UInt32 absoluteBlock;
            /// <summary>Only in 20 bytes tag at 0x0B, checksum byte</summary>
            public byte checksum;
            /// <summary>0x06 in 12 bytes tag, 0x0C in 20 bytes tag, relative block</summary>
            public UInt16 relBlock;
            /// <summary>
            /// Next block for this file.
            /// In 12 bytes tag at 0x08, 2 bytes, 0x8000 bit seems always set, 0x07FF means this is last block.
            /// In 20 bytes tag at 0x0E, 3 bytes, 0xFFFFFF means this is last block.
            /// </summary>
            public UInt32 nextBlock;
            /// <summary>
            /// Previous block for this file.
            /// In 12 bytes tag at 0x0A, 2 bytes, 0x07FF means this is first block.
            /// In 20 bytes tag at 0x11, 3 bytes, 0xFFFFFF means this is first block.
            /// </summary>
            public UInt32 prevBlock;

            /// <summary>On-memory value for easy first block search.</summary>
            public bool isFirst;
            /// <summary>On-memory value for easy last block search.</summary>
            public bool isLast;
        }

        struct CatalogEntry
        {
            /// <summary>0x00, seems to be 0x24 when the entry is valid</summary>
            public byte marker;
            /// <summary>0x01, seems to be always zero</summary>
            public ushort zero;
            /// <summary>0x03, filename, 32-bytes, null-padded</summary>
            public byte[] filename;
            /// <summary>0x23, null-termination</summary>
            public byte terminator;
            /// <summary>
            /// At 0x24
            /// 0x03 here for entries 64 bytes long
            /// 0x08 here for entries 78 bytes long
            /// 0x7C here for entries 50 bytes long
            /// This is incomplete, may fail, mostly works...
            /// </summary>
            public byte fileType;
            /// <summary>0x25, lot of values found here, unknown</summary>
            public byte unknown;
            /// <summary>0x26, file ID, must be positive and bigger than 4</summary>
            public Int16 fileID;
            /// <summary>0x28, creation date</summary>
            public UInt32 dtc;
            /// <summary>0x2C, last modification date</summary>
            public UInt32 dtm;
            /// <summary>0x30, file length in bytes</summary>
            public Int32 length;
            /// <summary>0x34, file length in bytes, including wasted block space</summary>
            public Int32 wasted;
            /// <summary>0x38, unknown</summary>
            public byte[] tail;
        }

        struct Extent
        {
            public Int32 start;
            public Int16 length;
        }

        struct ExtentFile
        {
            /// <summary>0x00, filename length</summary>
            public byte filenameLen;
            /// <summary>0x01, filename</summary>
            public byte[] filename;
            /// <summary>0x20, unknown</summary>
            public ushort unknown1;
            /// <summary>0x22, 8 bytes</summary>
            public UInt64 file_uid;
            /// <summary>0x2A, unknown</summary>
            public byte unknown2;
            /// <summary>0x2B, entry type? gets modified</summary>
            public byte etype;
            /// <summary>0x2C, file type</summary>
            public FileType ftype;
            /// <summary>0x2D, unknown</summary>
            public byte unknown3;
            /// <summary>0x2E, creation time</summary>
            public UInt32 dtc;
            /// <summary>0x32, last access time</summary>
            public UInt32 dta;
            /// <summary>0x36, modification time</summary>
            public UInt32 dtm;
            /// <summary>0x3A, backup time</summary>
            public UInt32 dtb;
            /// <summary>0x3E, scavenge time</summary>
            public UInt32 dts;
            /// <summary>0x42, machine serial number</summary>
            public UInt32 serial;
            /// <summary>0x46, unknown</summary>
            public byte unknown4;
            /// <summary>0x47, locked file</summary>
            public byte locked;
            /// <summary>0x48, protected file</summary>
            public byte protect;
            /// <summary>0x49, master file</summary>
            public byte master;
            /// <summary>0x4A, scavenged file</summary>
            public byte scavenged;
            /// <summary>0x4B, file closed by os</summary>
            public byte closed;
            /// <summary>0x4C, file left open</summary>
            public byte open;
            /// <summary>0x4D, 11 bytes, unknown</summary>
            public byte[] unknown5;
            /// <summary>0x58, Release number</summary>
            public UInt16 release;
            /// <summary>0x5A, Build number</summary>
            public UInt16 build;
            /// <summary>0x5C, Compatibility level</summary>
            public UInt16 compatibility;
            /// <summary>0x5E, Revision level</summary>
            public UInt16 revision;
            /// <summary>0x60, unknown</summary>
            public ushort unknown6;
            /// <summary>0x62, 0x08 set if password is valid</summary>
            public byte password_valid;
            /// <summary>0x63, 8 bytes, scrambled password</summary>
            public byte[] password;
            /// <summary>0x6B, 3 bytes, unknown</summary>
            public byte[] unknown7;
            /// <summary>0x6E, filesystem overhead</summary>
            public ushort overhead;
            /// <summary>0x70, 16 bytes, unknown</summary>
            public byte[] unknown8;
            /// <summary>0x80, file length in blocks</summary>
            public Int32 length;
            /// <summary>0x84, unknown</summary>
            public Int32 unknown9;
            /// <summary>0x88, extents, can contain up to 41 extents, dunno LisaOS maximum (never seen more than 3)</summary>
            public Extent[] extents;
            /// <summary>0x17E, unknown, empty, padding?</summary>
            public short unknown10;
            /// <summary>0x180, 128 bytes</summary>
            public byte[] LisaInfo;
        }
    }
}

