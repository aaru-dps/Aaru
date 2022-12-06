// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Lisa filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple Lisa filesystem structures.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable NotAccessedField.Local

namespace Aaru.Filesystems.LisaFS
{
    public sealed partial class LisaFS
    {
        /// <summary>
        ///     The MDDF is the most import block on a Lisa FS volume. It describes the volume and its contents. On
        ///     initialization the memory where it resides is not emptied so it tends to contain a lot of garbage. This has
        ///     difficulted its reverse engineering.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct MDDF
        {
            /// <summary>0x00, Filesystem version</summary>
            public ushort fsversion;
            /// <summary>0x02, Volume ID</summary>
            public ulong volid;
            /// <summary>0x0A, Volume sequence number</summary>
            public ushort volnum;
            /// <summary>0x0C, Pascal string, 32+1 bytes, volume name</summary>
            public string volname;
            /// <summary>0x2D, unknown, possible padding</summary>
            public byte unknown1;
            /// <summary>0x2E, Pascal string, 32+1 bytes, password</summary>
            public string password;
            /// <summary>0x4F, unknown, possible padding</summary>
            public byte unknown2;
            /// <summary>0x50, Lisa serial number that init'ed this disk</summary>
            public uint machine_id;
            /// <summary>0x54, ID of the master copy ? no idea really</summary>
            public uint master_copy_id;
            /// <summary>0x58, Date of volume creation</summary>
            public DateTime dtvc;
            /// <summary>0x5C, Date...</summary>
            public DateTime dtcc;
            /// <summary>0x60, Date of volume backup</summary>
            public DateTime dtvb;
            /// <summary>0x64, Date of volume scavenging</summary>
            public DateTime dtvs;
            /// <summary>0x68, unknown</summary>
            public uint unknown3;
            /// <summary>0x6C, block the MDDF is residing on</summary>
            public uint mddf_block;
            /// <summary>0x70, volsize-1</summary>
            public uint volsize_minus_one;
            /// <summary>0x74, volsize-1-mddf_block</summary>
            public uint volsize_minus_mddf_minus_one;
            /// <summary>0x78, Volume size in blocks</summary>
            public uint vol_size;
            /// <summary>0x7C, Blocks size of underlying drive (data+tags)</summary>
            public ushort blocksize;
            /// <summary>0x7E, Data only block size</summary>
            public ushort datasize;
            /// <summary>0x80, unknown</summary>
            public ushort unknown4;
            /// <summary>0x82, unknown</summary>
            public uint unknown5;
            /// <summary>0x86, unknown</summary>
            public uint unknown6;
            /// <summary>0x8A, Size in sectors of filesystem clusters</summary>
            public ushort clustersize;
            /// <summary>0x8C, Filesystem size in blocks</summary>
            public uint fs_size;
            /// <summary>0x90, unknown</summary>
            public uint unknown7;
            /// <summary>0x94, Pointer to S-Records</summary>
            public uint srec_ptr;
            /// <summary>0x98, unknown</summary>
            public ushort unknown9;
            /// <summary>0x9A, S-Records length</summary>
            public ushort srec_len;
            /// <summary>0x9C, unknown</summary>
            public uint unknown10;
            /// <summary>0xA0, unknown</summary>
            public uint unknown11;
            /// <summary>0xA4, unknown</summary>
            public uint unknown12;
            /// <summary>0xA8, unknown</summary>
            public uint unknown13;
            /// <summary>0xAC, unknown</summary>
            public uint unknown14;
            /// <summary>0xB0, Files in volume</summary>
            public ushort filecount;
            /// <summary>0xB2, unknown</summary>
            public uint unknown15;
            /// <summary>0xB6, unknown</summary>
            public uint unknown16;
            /// <summary>0xBA, Free blocks</summary>
            public uint freecount;
            /// <summary>0xBE, unknown</summary>
            public ushort unknown17;
            /// <summary>0xC0, unknown</summary>
            public uint unknown18;
            /// <summary>0xC4, no idea</summary>
            public ulong overmount_stamp;
            /// <summary>0xCC, serialization, lisa serial number authorized to use blocked software on this volume</summary>
            public uint serialization;
            /// <summary>0xD0, unknown</summary>
            public uint unknown19;
            /// <summary>0xD4, unknown, possible timestamp</summary>
            public uint unknown_timestamp;
            /// <summary>0xD8, unknown</summary>
            public uint unknown20;
            /// <summary>0xDC, unknown</summary>
            public uint unknown21;
            /// <summary>0xE0, unknown</summary>
            public uint unknown22;
            /// <summary>0xE4, unknown</summary>
            public uint unknown23;
            /// <summary>0xE8, unknown</summary>
            public uint unknown24;
            /// <summary>0xEC, unknown</summary>
            public uint unknown25;
            /// <summary>0xF0, unknown</summary>
            public uint unknown26;
            /// <summary>0xF4, unknown</summary>
            public uint unknown27;
            /// <summary>0xF8, unknown</summary>
            public uint unknown28;
            /// <summary>0xFC, unknown</summary>
            public uint unknown29;
            /// <summary>0x100, unknown</summary>
            public uint unknown30;
            /// <summary>0x104, unknown</summary>
            public uint unknown31;
            /// <summary>0x108, unknown</summary>
            public uint unknown32;
            /// <summary>0x10C, unknown</summary>
            public uint unknown33;
            /// <summary>0x110, unknown</summary>
            public uint unknown34;
            /// <summary>0x114, unknown</summary>
            public uint unknown35;
            /// <summary>0x118, ID of volume where this volume was backed up</summary>
            public ulong backup_volid;
            /// <summary>0x120, Size of LisaInfo label</summary>
            public ushort label_size;
            /// <summary>0x122, not clear</summary>
            public ushort fs_overhead;
            /// <summary>0x124, Return code of Scavenger</summary>
            public ushort result_scavenge;
            /// <summary>0x126, No idea</summary>
            public ushort boot_code;
            /// <summary>0x128, No idea</summary>
            public ushort boot_environ;
            /// <summary>0x12A, unknown</summary>
            public uint unknown36;
            /// <summary>0x12E, unknown</summary>
            public uint unknown37;
            /// <summary>0x132, unknown</summary>
            public uint unknown38;
            /// <summary>0x136, Total volumes in sequence</summary>
            public ushort vol_sequence;
            /// <summary>0x138, Volume is dirty?</summary>
            public byte vol_left_mounted;
            /// <summary>Is password present? (On-disk position unknown)</summary>
            public byte passwd_present;
            /// <summary>Opened files (memory-only?) (On-disk position unknown)</summary>
            public uint opencount;
            /// <summary>No idea (On-disk position unknown)</summary>
            public uint copy_thread;

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

        /// <summary>
        ///     An entry in the catalog from V3. The first entry is bigger than the rest, may be a header, I have not needed
        ///     any of its values so I just ignored it. Each catalog is divided in 4-sector blocks, and if it needs more than a
        ///     block there are previous and next block pointers, effectively making the V3 catalog a double-linked list. Garbage
        ///     is not zeroed.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct CatalogEntry
        {
            /// <summary>0x00, seems to be 0x24 when the entry is valid</summary>
            public byte marker;
            /// <summary>0x01, parent directory ID for this file, 0 for root directory</summary>
            public ushort parentID;
            /// <summary>0x03, filename, 32-bytes, null-padded</summary>
            public byte[] filename;
            /// <summary>0x23, null-termination</summary>
            public byte terminator;
            /// <summary>
            ///     At 0x24 0x01 here for subdirectories, entries 48 bytes long 0x03 here for entries 64 bytes long 0x08 here for
            ///     entries 78 bytes long This is incomplete, may fail, mostly works...
            /// </summary>
            public byte fileType;
            /// <summary>0x25, lot of values found here, unknown</summary>
            public byte unknown;
            /// <summary>0x26, file ID, must be positive and bigger than 4</summary>
            public short fileID;
            /// <summary>0x28, creation date</summary>
            public uint dtc;
            /// <summary>0x2C, last modification date</summary>
            public uint dtm;
            /// <summary>0x30, file length in bytes</summary>
            public int length;
            /// <summary>0x34, file length in bytes, including wasted block space</summary>
            public int wasted;
            /// <summary>0x38, unknown</summary>
            public byte[] tail;
        }

        /// <summary>An extent indicating a start and a run of sectors.</summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct Extent
        {
            public int   start;
            public short length;
        }

        /// <summary>
        ///     The Extents File. There is one Extents File per each file stored on disk. The file ID present on the sectors
        ///     tags for the Extents File is the negated value of the file ID it represents. e.g. file = 5 (0x0005) extents = -5
        ///     (0xFFFB) It spans a single sector on V2 and V3 but 2 sectors on V1. It contains all information about a file, and
        ///     is indexed in the S-Records file. It also contains the label. Garbage is zeroed.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct ExtentFile
        {
            /// <summary>0x00, filename length</summary>
            public byte filenameLen;
            /// <summary>0x01, filename</summary>
            public byte[] filename;
            /// <summary>0x20, unknown</summary>
            public ushort unknown1;
            /// <summary>0x22, 8 bytes</summary>
            public ulong file_uid;
            /// <summary>0x2A, unknown</summary>
            public byte unknown2;
            /// <summary>0x2B, entry type? gets modified</summary>
            public byte etype;
            /// <summary>0x2C, file type</summary>
            public FileType ftype;
            /// <summary>0x2D, unknown</summary>
            public byte unknown3;
            /// <summary>0x2E, creation time</summary>
            public uint dtc;
            /// <summary>0x32, last access time</summary>
            public uint dta;
            /// <summary>0x36, modification time</summary>
            public uint dtm;
            /// <summary>0x3A, backup time</summary>
            public uint dtb;
            /// <summary>0x3E, scavenge time</summary>
            public uint dts;
            /// <summary>0x42, machine serial number</summary>
            public uint serial;
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
            public ushort release;
            /// <summary>0x5A, Build number</summary>
            public ushort build;
            /// <summary>0x5C, Compatibility level</summary>
            public ushort compatibility;
            /// <summary>0x5E, Revision level</summary>
            public ushort revision;
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
            /// <summary>0x80, 0x200 in v1, file length in blocks</summary>
            public int length;
            /// <summary>0x84, 0x204 in v1, unknown</summary>
            public int unknown9;
            /// <summary>
            ///     0x88, 0x208 in v1, extents, can contain up to 41 extents (85 in v1), dunno LisaOS maximum (never seen more
            ///     than 3)
            /// </summary>
            public Extent[] extents;
            /// <summary>0x17E, unknown, empty, padding?</summary>
            public short unknown10;
            /// <summary>
            ///     At 0x180, this is the label. While 1982 pre-release documentation says the label can be up to 448 bytes, v1
            ///     onward only have space for a 128 bytes one. Any application can write whatever they want in the label, however,
            ///     Lisa Office uses it to store its own information, something that will effectively overwrite any information a user
            ///     application wrote there. The information written here by Lisa Office is like the information Finder writes in the
            ///     FinderInfo structures, plus the non-unique name that is shown on the GUI. For this reason I called it LisaInfo. I
            ///     have not tried to reverse engineer it.
            /// </summary>
            public byte[] LisaInfo;
        }

        /// <summary>
        ///     The S-Records File is a hashtable of S-Records, where the hash is the file ID they belong to. The S-Records
        ///     File cannot be fragmented or grown, and it can easily become full before the 32766 file IDs are exhausted. Each
        ///     S-Record entry contains a block pointer to the Extents File that correspond to that file ID as well as the real
        ///     file size, the only important information about a file that's not inside the Extents File. It also contains a low
        ///     value (less than 0x200) variable field of unknown meaning and another one that seems to be flags, with values like
        ///     0, 1, 3 and 5.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct SRecord
        {
            /// <summary>0x00, block where ExtentsFile for this entry resides</summary>
            public uint extent_ptr;
            /// <summary>0x04, unknown</summary>
            public uint unknown;
            /// <summary>0x08, filesize in bytes</summary>
            public uint filesize;
            /// <summary>0x0C, some kind of flags, meaning unknown</summary>
            public ushort flags;
        }

        /// <summary>
        ///     The catalog entry for the V1 and V2 volume formats. It merely contains the file name, type and ID, plus a few
        ///     (mostly empty) unknown fields. Contrary to V3, it has no header and instead of being a double-linked list it is
        ///     fragmented using an Extents File. The Extents File position for the root catalog is then stored in the S-Records
        ///     File. Its entries are not filed sequentially denoting some kind of in-memory structure while at the same time
        ///     forcing LisaOS to read the whole catalog. That or I missed the pointers. Empty entries just contain a 0-len
        ///     filename. Garbage is not zeroed.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct CatalogEntryV2
        {
            /// <summary>0x00, filename, 32-bytes, null-padded</summary>
            public byte filenameLen;
            /// <summary>0x01, filename, 31-bytes</summary>
            public byte[] filename;
            /// <summary>0x21, unknown</summary>
            public byte unknown1;
            /// <summary>0x22, unknown</summary>
            public byte fileType;
            /// <summary>0x23, unknown</summary>
            public byte unknown2;
            /// <summary>0x24, unknown</summary>
            public short fileID;
            /// <summary>0x26, 16 bytes, unknown</summary>
            public byte[] unknown3;
        }
    }
}