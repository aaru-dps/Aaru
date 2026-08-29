// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Files-11 On-Disk Structure plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     On-disk structures for the Files-11 On-Disk Structure.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

public sealed partial class ODS
{
#region Nested type: HomeBlock

    /// <summary>ODS Home Block, 512 bytes</summary>
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
        /// <summary>0x02A, Volume characteristics flags</summary>
        public readonly VolumeCharacteristics volchar;
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

#endregion

#region Nested type: FileId

    /// <summary>File Identifier (FID), 6 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct FileId
    {
        /// <summary>File number</summary>
        public readonly ushort num;
        /// <summary>File sequence number</summary>
        public readonly ushort seq;
        /// <summary>Relative volume number</summary>
        public readonly byte rvn;
        /// <summary>File number extension (high byte)</summary>
        public readonly byte nmx;
    }

#endregion

#region Nested type: UserIdentificationCode

    /// <summary>VMS User Identification Code (UIC), 4 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct UserIdentificationCode
    {
        /// <summary>Member number</summary>
        public readonly ushort member;
        /// <summary>Group number</summary>
        public readonly ushort group;
    }

#endregion

#region Nested type: FatBlock

    /// <summary>FAT Block number (high word, low word format), 4 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct FatBlock
    {
        /// <summary>High order word</summary>
        public readonly ushort high;
        /// <summary>Low order word</summary>
        public readonly ushort low;

        /// <summary>Gets the full 32-bit block number</summary>
        public uint Value => (uint)(high << 16) | low;
    }

#endregion

#region Nested type: FileAttributes

    /// <summary>File Attributes (FAT), 32 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct FileAttributes
    {
        /// <summary>0x00, Record type (low nibble) and file organization (high nibble)</summary>
        public readonly byte rtype;
        /// <summary>0x01, Record attributes</summary>
        public readonly RecordAttributeFlags rattrib;
        /// <summary>0x02, Record size in bytes</summary>
        public readonly ushort rsize;
        /// <summary>0x04, Highest allocated VBN</summary>
        public readonly FatBlock hiblk;
        /// <summary>0x08, End of file VBN</summary>
        public readonly FatBlock efblk;
        /// <summary>0x0C, First free byte in EFBLK</summary>
        public readonly ushort ffbyte;
        /// <summary>0x0E, Bucket size in blocks</summary>
        public readonly byte bktsize;
        /// <summary>0x0F, Size in bytes of fixed length control for VFC records</summary>
        public readonly byte vfcsize;
        /// <summary>0x10, Maximum record size in bytes</summary>
        public readonly ushort maxrec;
        /// <summary>0x12, Default extend quantity</summary>
        public readonly ushort defext;
        /// <summary>0x14, Global buffer count (original word)</summary>
        public readonly ushort gbc;
        /// <summary>0x16, Flags for record attribute area</summary>
        public readonly GlobalBufferCountFlags recattr_flags;
        /// <summary>0x17, Fill byte</summary>
        public readonly byte fill_0;
        /// <summary>0x18, Longword implementation of global buffer count</summary>
        public readonly uint gbc32;
        /// <summary>0x1C, Spare space documented as unused in I/O REF</summary>
        public readonly ushort fill_1;
        /// <summary>0x1E, Default version limit for directory file</summary>
        public readonly ushort versions;

        /// <summary>Gets the record type</summary>
        public RecordTypeValue RecordType => (RecordTypeValue)(rtype & 0x0F);

        /// <summary>Gets the file organization</summary>
        public FileOrganization Organization => (FileOrganization)(rtype >> 4 & 0x0F);
    }

#endregion

#region Nested type: FileHeader

    /// <summary>File Header (ODS-2/5), 512 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct FileHeader
    {
        /// <summary>0x00, Offset in words to ident area</summary>
        public readonly byte idoffset;
        /// <summary>0x01, Offset in words to map area</summary>
        public readonly byte mpoffset;
        /// <summary>0x02, Offset in words to access control area</summary>
        public readonly byte acoffset;
        /// <summary>0x03, Offset in words to reserved area</summary>
        public readonly byte rsoffset;
        /// <summary>0x04, Segment number (extension file header)</summary>
        public readonly ushort seg_num;
        /// <summary>0x06, Structure level</summary>
        public readonly ushort struclev;
        /// <summary>0x08, File ID</summary>
        public readonly FileId fid;
        /// <summary>0x0E, Extension file ID</summary>
        public readonly FileId ext_fid;
        /// <summary>0x14, Record attributes (FAT)</summary>
        public readonly FileAttributes recattr;
        /// <summary>0x34, File characteristics</summary>
        public readonly FileCharacteristicFlags filechar;
        /// <summary>0x38, Record protection</summary>
        public readonly ushort recprot;
        /// <summary>0x3A, Number of map area words in use</summary>
        public readonly byte map_inuse;
        /// <summary>0x3B, Access mode</summary>
        public readonly byte acc_mode;
        /// <summary>0x3C, File owner UIC</summary>
        public readonly UserIdentificationCode fileowner;
        /// <summary>0x40, File protection</summary>
        public readonly ushort fileprot;
        /// <summary>0x42, Back link (directory containing this file)</summary>
        public readonly FileId backlink;
        /// <summary>0x48, Journal flags</summary>
        public readonly byte journal;
        /// <summary>0x49, RU active</summary>
        public readonly byte ru_active;
        /// <summary>0x4A, Link count (hardlinks)</summary>
        public readonly ushort linkcount;
        /// <summary>0x4C, Highwater mark</summary>
        public readonly uint highwater;
        /// <summary>0x50, Reserved area, 430 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 430)]
        public readonly byte[] reserved;
        /// <summary>0x1FE, Checksum</summary>
        public readonly ushort checksum;
    }

#endregion

#region Nested type: FileIdent2

    /// <summary>File Ident Area (ODS-2), 120 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct FileIdent2
    {
        /// <summary>0x00, Filename (20 bytes, space padded)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] filename;
        /// <summary>0x14, Revision number</summary>
        public readonly ushort revision;
        /// <summary>0x16, Creation date</summary>
        public readonly ulong credate;
        /// <summary>0x1E, Revision date</summary>
        public readonly ulong revdate;
        /// <summary>0x26, Expiration date</summary>
        public readonly ulong expdate;
        /// <summary>0x2E, Backup date</summary>
        public readonly ulong bakdate;
        /// <summary>0x36, Filename extension (66 bytes)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 66)]
        public readonly byte[] filenamext;
    }

#endregion

#region Nested type: FileIdent5

    /// <summary>File Ident Area (ODS-5), 324 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct FileIdent5
    {
        /// <summary>0x00, Control byte (name type: 0=ODS2, 1=ISL1, 3=UCS2)</summary>
        public readonly FileIdentControl control;
        /// <summary>0x01, Filename length</summary>
        public readonly byte namelen;
        /// <summary>0x02, Revision number</summary>
        public readonly ushort revision;
        /// <summary>0x04, Creation date</summary>
        public readonly ulong credate;
        /// <summary>0x0C, Revision date</summary>
        public readonly ulong revdate;
        /// <summary>0x14, Expiration date</summary>
        public readonly ulong expdate;
        /// <summary>0x1C, Backup date</summary>
        public readonly ulong bakdate;
        /// <summary>0x24, Access date</summary>
        public readonly ulong accdate;
        /// <summary>0x2C, Attribute change date</summary>
        public readonly ulong attdate;
        /// <summary>0x34, Extended record attributes</summary>
        public readonly ulong ex_recattr;
        /// <summary>0x3C, Hint low quadword</summary>
        public readonly ulong hint_lo_qw;
        /// <summary>0x44, Hint high quadword</summary>
        public readonly ulong hint_hi_qw;
        /// <summary>0x4C, Filename (44 bytes)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 44)]
        public readonly byte[] filename;
        /// <summary>0x78, Filename extension (204 bytes)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 204)]
        public readonly byte[] filenamext;
    }

#endregion

#region Nested type: RetrievalPointerFormat0

    /// <summary>Retrieval Pointer Format 0 (placement control), 2 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RetrievalPointerFormat0
    {
        /// <summary>Packed word: word0(14 bits) + format(2 bits)</summary>
        public readonly ushort value;

        /// <summary>Format code (should be 0)</summary>
        public byte Format => (byte)(value >> 14 & 0x03);

        /// <summary>Control word</summary>
        public ushort Word0 => (ushort)(value & 0x3FFF);
    }

#endregion

#region Nested type: RetrievalPointerFormat1

    /// <summary>Retrieval Pointer Format 1 (8-bit count, 22-bit LBN), 4 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RetrievalPointerFormat1
    {
        /// <summary>Block count</summary>
        public readonly byte count;
        /// <summary>Packed byte: highlbn(6 bits) + format(2 bits)</summary>
        public readonly byte highlbn_format;
        /// <summary>Low 16 bits of LBN</summary>
        public readonly ushort lowlbn;

        /// <summary>Format code (should be 1)</summary>
        public byte Format => (byte)(highlbn_format >> 6 & 0x03);

        /// <summary>High 6 bits of LBN</summary>
        public byte HighLbn => (byte)(highlbn_format & 0x3F);

        /// <summary>Full LBN value</summary>
        public uint Lbn => (uint)(HighLbn << 16) | lowlbn;
    }

#endregion

#region Nested type: RetrievalPointerFormat2

    /// <summary>Retrieval Pointer Format 2 (14-bit count, 32-bit LBN), 6 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RetrievalPointerFormat2
    {
        /// <summary>Packed word: count(14 bits) + format(2 bits)</summary>
        public readonly ushort count_format;
        /// <summary>LBN</summary>
        public readonly uint lbn;

        /// <summary>Format code (should be 2)</summary>
        public byte Format => (byte)(count_format >> 14 & 0x03);

        /// <summary>Block count</summary>
        public ushort Count => (ushort)(count_format & 0x3FFF);
    }

#endregion

#region Nested type: RetrievalPointerFormat3

    /// <summary>Retrieval Pointer Format 3 (30-bit count, 32-bit LBN), 8 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct RetrievalPointerFormat3
    {
        /// <summary>Packed word: highcount(14 bits) + format(2 bits)</summary>
        public readonly ushort highcount_format;
        /// <summary>Low 16 bits of count</summary>
        public readonly ushort lowcount;
        /// <summary>LBN</summary>
        public readonly uint lbn;

        /// <summary>Format code (should be 3)</summary>
        public byte Format => (byte)(highcount_format >> 14 & 0x03);

        /// <summary>Full block count</summary>
        public uint Count => (uint)((highcount_format & 0x3FFF) << 16) | lowcount;
    }

#endregion

#region Nested type: DirectoryRecord

    /// <summary>Directory Record header, 6+ bytes (variable length)</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryRecord
    {
        /// <summary>0x00, Size of directory record in bytes</summary>
        public readonly ushort size;
        /// <summary>0x02, Version number (for this entry)</summary>
        public readonly ushort version;
        /// <summary>0x04, Directory flags</summary>
        public readonly DirectoryRecordFlags flags;
        /// <summary>0x05, Name count (length of name)</summary>
        public readonly byte namecount;

        // Followed by: name[namecount] (word padded)
        // Then: DirectoryEntry structures

        /// <summary>Gets the record type</summary>
        public DirectoryRecordType RecordType => (DirectoryRecordType)((byte)flags & 0x07);

        /// <summary>Gets the name type</summary>
        public DirectoryNameType NameType => (DirectoryNameType)((byte)flags >> 3 & 0x07);
    }

#endregion

#region Nested type: DirectoryEntry

    /// <summary>Directory Entry (version/FID pair), 8 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryEntry
    {
        /// <summary>0x00, Version number</summary>
        public readonly ushort version;
        /// <summary>0x02, File ID</summary>
        public readonly FileId fid;
    }

#endregion

#region Nested type: StorageControlBlock

    /// <summary>Storage Control Block (SCB), 512 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct StorageControlBlock
    {
        /// <summary>0x00, Structure level</summary>
        public readonly ushort struclev;
        /// <summary>0x02, Cluster factor</summary>
        public readonly ushort cluster;
        /// <summary>0x04, Volume size in blocks</summary>
        public readonly uint volsize;
        /// <summary>0x08, Block size</summary>
        public readonly uint blksize;
        /// <summary>0x0C, Sectors per track</summary>
        public readonly uint sectors;
        /// <summary>0x10, Tracks per cylinder</summary>
        public readonly uint tracks;
        /// <summary>0x14, Cylinders</summary>
        public readonly uint cylinder;
        /// <summary>0x18, Status flags</summary>
        public readonly uint status;
        /// <summary>0x1C, Status flags 2</summary>
        public readonly uint status2;
        /// <summary>0x20, Write count</summary>
        public readonly ushort writecnt;
        /// <summary>0x22, Volume lock name, 12 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] volockname;
        /// <summary>0x2E, Mount time</summary>
        public readonly ulong mounttime;
        /// <summary>0x36, Reserved, 456 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 456)]
        public readonly byte[] not_used;
        /// <summary>0x1FE, Checksum</summary>
        public readonly ushort checksum;
    }

#endregion
}