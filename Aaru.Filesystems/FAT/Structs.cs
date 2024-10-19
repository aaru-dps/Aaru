// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft FAT filesystem plugin.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

// ReSharper disable NotAccessedField.Local

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class FAT
{
    const int UMSDOS_MAXNAME = 220;

#region Nested type: ApricotLabel

    /// <summary>Apricot Label.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ApricotLabel
    {
        /// <summary>Version of format which created disk</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] version;
        /// <summary>Operating system.</summary>
        public readonly byte operatingSystem;
        /// <summary>Software write protection.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool writeProtected;
        /// <summary>Copy protected.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool copyProtected;
        /// <summary>Boot type.</summary>
        public readonly byte bootType;
        /// <summary>Partitions.</summary>
        public readonly byte partitionCount;
        /// <summary>Is hard disk?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool winchester;
        /// <summary>Sector size.</summary>
        public readonly ushort sectorSize;
        /// <summary>Sectors per track.</summary>
        public readonly ushort spt;
        /// <summary>Tracks per side.</summary>
        public readonly uint cylinders;
        /// <summary>Sides.</summary>
        public readonly byte heads;
        /// <summary>Interleave factor.</summary>
        public readonly byte interleave;
        /// <summary>Skew factor.</summary>
        public readonly ushort skew;
        /// <summary>Sector where boot code starts.</summary>
        public readonly uint bootLocation;
        /// <summary>Size in sectors of boot code.</summary>
        public readonly ushort bootSize;
        /// <summary>Address at which to load boot code.</summary>
        public readonly uint bootAddress;
        /// <summary>Offset where to jump to boot.</summary>
        public readonly ushort bootOffset;
        /// <summary>Segment where to jump to boot.</summary>
        public readonly ushort bootSegment;
        /// <summary>First data sector.</summary>
        public readonly uint firstDataBlock;
        /// <summary>Generation.</summary>
        public readonly ushort generation;
        /// <summary>Copy count.</summary>
        public readonly ushort copyCount;
        /// <summary>Maximum number of copies.</summary>
        public readonly ushort maxCopies;
        /// <summary>Serial number.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] serialNumber;
        /// <summary>Part number.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] partNumber;
        /// <summary>Copyright.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public readonly byte[] copyright;
        /// <summary>BPB for whole disk.</summary>
        public ApricotParameterBlock mainBPB;
        /// <summary>Name of FONT file.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] fontName;
        /// <summary>Name of KEYBOARD file.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] keyboardName;
        /// <summary>Minor BIOS version.</summary>
        public readonly byte biosMinorVersion;
        /// <summary>Major BIOS version.</summary>
        public readonly byte biosMajorVersion;
        /// <summary>Diagnostics enabled?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool diagnosticsFlag;
        /// <summary>Printer device.</summary>
        public readonly byte prnDevice;
        /// <summary>Bell volume.</summary>
        public readonly byte bellVolume;
        /// <summary>Cache enabled?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool enableCache;
        /// <summary>Graphics enabled?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool enableGraphics;
        /// <summary>Length in sectors of DOS.</summary>
        public readonly byte dosLength;
        /// <summary>Length in sectors of FONT file.</summary>
        public readonly byte fontLength;
        /// <summary>Length in sectors of KEYBOARD file.</summary>
        public readonly byte keyboardLength;
        /// <summary>Starting sector of DOS.</summary>
        public readonly ushort dosStart;
        /// <summary>Starting sector of FONT file.</summary>
        public readonly ushort fontStart;
        /// <summary>Starting sector of KEYBOARD file.</summary>
        public readonly ushort keyboardStart;
        /// <summary>Keyboard click volume.</summary>
        public readonly byte keyboardVolume;
        /// <summary>Auto-repeat enabled?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool autorepeat;
        /// <summary>Auto-repeat lead-in.</summary>
        public readonly byte autorepeatLeadIn;
        /// <summary>Auto-repeat interval.</summary>
        public readonly byte autorepeatInterval;
        /// <summary>Microscreen mode.</summary>
        public readonly byte microscreenMode;
        /// <summary>Spare area for keyboard values expansion.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] spareKeyboard;
        /// <summary>Screen line mode.</summary>
        public readonly byte lineMode;
        /// <summary>Screen line width.</summary>
        public readonly byte lineWidth;
        /// <summary>Screen disabled?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool imageOff;
        /// <summary>Spare area for screen values expansion.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        public readonly byte[] spareScreen;
        /// <summary>TX baud rate.</summary>
        public readonly byte txBaudRate;
        /// <summary>RX baud rate.</summary>
        public readonly byte rxBaudRate;
        /// <summary>TX bits.</summary>
        public readonly byte txBits;
        /// <summary>RX bits.</summary>
        public readonly byte rxBits;
        /// <summary>Stop bits.</summary>
        public readonly byte stopBits;
        /// <summary>Parity enabled?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool parityCheck;
        /// <summary>Parity type.</summary>
        public readonly byte parityType;
        /// <summary>Xon/Xoff enabled on TX.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool txXonXoff;
        /// <summary>Xon/Xoff enabled on RX.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool rxXonXoff;
        /// <summary>Xon character.</summary>
        public readonly byte xonCharacter;
        /// <summary>Xoff character.</summary>
        public readonly byte xoffCharacter;
        /// <summary>Xon/Xoff buffer on RX.</summary>
        public readonly ushort rxXonXoffBuffer;
        /// <summary>DTR/DSR enabled?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool dtrDsr;
        /// <summary>CTS/RTS enabled?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool ctsRts;
        /// <summary>NULLs after CR.</summary>
        public readonly byte nullsAfterCr;
        /// <summary>NULLs after 0xFF.</summary>
        public readonly byte nullsAfterFF;
        /// <summary>Send LF after CR in serial port.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool lfAfterCRSerial;
        /// <summary>BIOS error report in serial port.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool biosErrorReportSerial;
        /// <summary>Spare area for serial port values expansion.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        public readonly byte[] spareSerial;
        /// <summary>Send LF after CR in parallel port.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool lfAfterCrParallel;
        /// <summary>Select line supported?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool selectLine;
        /// <summary>Paper empty supported?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool paperEmpty;
        /// <summary>Fault line supported?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool faultLine;
        /// <summary>BIOS error report in parallel port.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool biosErrorReportParallel;
        /// <summary>Spare area for parallel port values expansion.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] spareParallel;
        /// <summary>Spare area for Winchester values expansion.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public readonly byte[] spareWinchester;
        /// <summary>Parking enabled?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool parkingEnabled;
        /// <summary>Format protection?.</summary>
        [MarshalAs(UnmanagedType.U1)]
        public readonly bool formatProtection;
        /// <summary>Spare area for RAM disk values expansion.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] spareRamDisk;
        /// <summary>List of bad blocks.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly ushort[] badBlocks;
        /// <summary>Array of partition BPBs.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly ApricotParameterBlock[] partitions;
        /// <summary>Spare area.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 63)]
        public readonly byte[] spare;
        /// <summary>CP/M double side indicator?.</summary>
        public readonly bool cpmDoubleSided;
    }

#endregion

#region Nested type: ApricotParameterBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ApricotParameterBlock
    {
        /// <summary>Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>Reserved sectors between BPB and FAT</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte fats_no;
        /// <summary>Number of entries on root directory</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume</summary>
        public ushort sectors;
        /// <summary>Media descriptor</summary>
        public readonly byte media;
        /// <summary>Sectors per FAT</summary>
        public readonly ushort spfat;
        /// <summary>Disk type</summary>
        public readonly byte diskType;
        /// <summary>Volume starting sector</summary>
        public readonly ushort startSector;
    }

#endregion

#region Nested type: AtariParameterBlock

    /// <summary>BIOS Parameter Block as used by Atari ST GEMDOS on FAT12 volumes.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AtariParameterBlock
    {
        /// <summary>68000 BRA.S jump or x86 loop</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] jump;
        /// <summary>OEM Name, 6 bytes, space-padded, "Loader" for Atari ST boot loader</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] oem_name;
        /// <summary>Volume serial number</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] serial_no;
        /// <summary>Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>Reserved sectors between BPB and FAT (inclusive)</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte fats_no;
        /// <summary>Number of entries on root directory</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume</summary>
        public ushort sectors;
        /// <summary>Media descriptor, unused by GEMDOS</summary>
        public readonly byte media;
        /// <summary>Sectors per FAT</summary>
        public readonly ushort spfat;
        /// <summary>Sectors per track</summary>
        public readonly ushort sptrk;
        /// <summary>Heads</summary>
        public readonly ushort heads;
        /// <summary>Hidden sectors before BPB, unused by GEMDOS</summary>
        public readonly ushort hsectors;
        /// <summary>Word to be loaded in the cmdload system variable. Big-endian.</summary>
        public readonly ushort execflag;
        /// <summary>
        ///     Word indicating load mode. If zero, file named <see cref="fname" /> is located and loaded. It not, sectors
        ///     specified in <see cref="ssect" /> and <see cref="sectcnt" /> are loaded. Big endian.
        /// </summary>
        public readonly ushort ldmode;
        /// <summary>Starting sector of boot code.</summary>
        public readonly ushort ssect;
        /// <summary>Count of sectors of boot code.</summary>
        public readonly ushort sectcnt;
        /// <summary>Address where boot code should be loaded.</summary>
        public readonly ushort ldaaddr;
        /// <summary>Padding.</summary>
        public readonly ushort padding;
        /// <summary>Address where FAT and root directory sectors must be loaded.</summary>
        public readonly ushort fatbuf;
        /// <summary>Unknown.</summary>
        public readonly ushort unknown;
        /// <summary>Filename to be loaded for booting.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] fname;
        /// <summary>Reserved</summary>
        public readonly ushort reserved;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 455)]
        public readonly byte[] boot_code;
        /// <summary>Big endian word to make big endian sum of all sector words be equal to 0x1234 if disk is bootable.</summary>
        public readonly ushort checksum;
    }

#endregion

#region Nested type: BiosParameterBlock2

    /// <summary>DOS 2.0 BIOS Parameter Block.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BiosParameterBlock2
    {
        /// <summary>x86 jump</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>Reserved sectors between BPB and FAT</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte fats_no;
        /// <summary>Number of entries on root directory</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume</summary>
        public ushort sectors;
        /// <summary>Media descriptor</summary>
        public readonly byte media;
        /// <summary>Sectors per FAT</summary>
        public readonly ushort spfat;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 486)]
        public readonly byte[] boot_code;
        /// <summary>0x55 0xAA if bootable.</summary>
        public readonly ushort boot_signature;
    }

#endregion

#region Nested type: BiosParameterBlock30

    /// <summary>DOS 3.0 BIOS Parameter Block.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BiosParameterBlock30
    {
        /// <summary>x86 jump</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>Reserved sectors between BPB and FAT</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte fats_no;
        /// <summary>Number of entries on root directory</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume</summary>
        public ushort sectors;
        /// <summary>Media descriptor</summary>
        public readonly byte media;
        /// <summary>Sectors per FAT</summary>
        public readonly ushort spfat;
        /// <summary>Sectors per track</summary>
        public readonly ushort sptrk;
        /// <summary>Heads</summary>
        public readonly ushort heads;
        /// <summary>Hidden sectors before BPB</summary>
        public readonly ushort hsectors;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)]
        public readonly byte[] boot_code;
        /// <summary>Always 0x55 0xAA.</summary>
        public readonly ushort boot_signature;
    }

#endregion

#region Nested type: BiosParameterBlock32

    /// <summary>DOS 3.2 BIOS Parameter Block.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BiosParameterBlock32
    {
        /// <summary>x86 jump</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>Reserved sectors between BPB and FAT</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte fats_no;
        /// <summary>Number of entries on root directory</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume</summary>
        public ushort sectors;
        /// <summary>Media descriptor</summary>
        public readonly byte media;
        /// <summary>Sectors per FAT</summary>
        public readonly ushort spfat;
        /// <summary>Sectors per track</summary>
        public readonly ushort sptrk;
        /// <summary>Heads</summary>
        public readonly ushort heads;
        /// <summary>Hidden sectors before BPB</summary>
        public readonly ushort hsectors;
        /// <summary>Total sectors including hidden ones</summary>
        public readonly ushort total_sectors;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 478)]
        public readonly byte[] boot_code;
        /// <summary>Always 0x55 0xAA.</summary>
        public readonly ushort boot_signature;
    }

#endregion

#region Nested type: BiosParameterBlock33

    /// <summary>DOS 3.31 BIOS Parameter Block.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BiosParameterBlock33
    {
        /// <summary>x86 jump</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>Reserved sectors between BPB and FAT</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte fats_no;
        /// <summary>Number of entries on root directory</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume</summary>
        public ushort sectors;
        /// <summary>Media descriptor</summary>
        public readonly byte media;
        /// <summary>Sectors per FAT</summary>
        public readonly ushort spfat;
        /// <summary>Sectors per track</summary>
        public readonly ushort sptrk;
        /// <summary>Heads</summary>
        public readonly ushort heads;
        /// <summary>Hidden sectors before BPB</summary>
        public readonly uint hsectors;
        /// <summary>Sectors in volume if > 65535</summary>
        public uint big_sectors;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 474)]
        public readonly byte[] boot_code;
        /// <summary>Always 0x55 0xAA.</summary>
        public readonly ushort boot_signature;
    }

#endregion

#region Nested type: BiosParameterBlockEbpb

    /// <summary>DOS 4.0 or higher BIOS Parameter Block.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BiosParameterBlockEbpb
    {
        /// <summary>x86 jump</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] jump;
        /// <summary>OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] oem_name;
        /// <summary>Bytes per sector</summary>
        public ushort bps;
        /// <summary>Sectors per cluster</summary>
        public byte spc;
        /// <summary>Reserved sectors between BPB and FAT</summary>
        public ushort rsectors;
        /// <summary>Number of FATs</summary>
        public byte fats_no;
        /// <summary>Number of entries on root directory</summary>
        public ushort root_ent;
        /// <summary>Sectors in volume</summary>
        public ushort sectors;
        /// <summary>Media descriptor</summary>
        public byte media;
        /// <summary>Sectors per FAT</summary>
        public ushort spfat;
        /// <summary>Sectors per track</summary>
        public ushort sptrk;
        /// <summary>Heads</summary>
        public ushort heads;
        /// <summary>Hidden sectors before BPB</summary>
        public uint hsectors;
        /// <summary>Sectors in volume if > 65535</summary>
        public uint big_sectors;
        /// <summary>Drive number</summary>
        public byte drive_no;
        /// <summary>Volume flags</summary>
        public byte flags;
        /// <summary>EPB signature, 0x29</summary>
        public byte signature;
        /// <summary>Volume serial number</summary>
        public uint serial_no;
        /// <summary>Volume label, 11 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] volume_label;
        /// <summary>Filesystem type, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] fs_type;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 448)]
        public byte[] boot_code;
        /// <summary>Always 0x55 0xAA.</summary>
        public ushort boot_signature;
    }

#endregion

#region Nested type: BiosParameterBlockShortEbpb

    /// <summary>DOS 3.4 BIOS Parameter Block.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BiosParameterBlockShortEbpb
    {
        /// <summary>x86 jump</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>Reserved sectors between BPB and FAT</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte fats_no;
        /// <summary>Number of entries on root directory</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume</summary>
        public ushort sectors;
        /// <summary>Media descriptor</summary>
        public readonly byte media;
        /// <summary>Sectors per FAT</summary>
        public readonly ushort spfat;
        /// <summary>Sectors per track</summary>
        public readonly ushort sptrk;
        /// <summary>Heads</summary>
        public readonly ushort heads;
        /// <summary>Hidden sectors before BPB</summary>
        public readonly uint hsectors;
        /// <summary>Sectors in volume if > 65535</summary>
        public uint big_sectors;
        /// <summary>Drive number</summary>
        public readonly byte drive_no;
        /// <summary>Volume flags</summary>
        public readonly byte flags;
        /// <summary>EPB signature, 0x28</summary>
        public readonly byte signature;
        /// <summary>Volume serial number</summary>
        public readonly uint serial_no;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 467)]
        public readonly byte[] boot_code;
        /// <summary>Always 0x55 0xAA.</summary>
        public readonly ushort boot_signature;
    }

#endregion

#region Nested type: CompleteDirectoryEntry

    sealed class CompleteDirectoryEntry
    {
        public DirectoryEntry       Dirent;
        public DirectoryEntry       Fat32Ea;
        public HumanDirectoryEntry  HumanDirent;
        public string               HumanName;
        public string               Lfn;
        public UmsdosDirectoryEntry LinuxDirent;
        public string               LinuxName;
        public string               Longname;
        public string               Shortname;

        public override string ToString()
        {
            // This ensures UMSDOS takes preference when present
            if(!string.IsNullOrEmpty(LinuxName)) return LinuxName;

            // This ensures LFN takes preference when eCS is in use
            if(!string.IsNullOrEmpty(Lfn)) return Lfn;

            // This ensures Humans takes preference when present
            if(!string.IsNullOrEmpty(HumanName)) return HumanName;

            return !string.IsNullOrEmpty(Longname) ? Longname : Shortname;
        }
    }

#endregion

#region Nested type: DirectoryEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] filename;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] extension;
        public readonly FatAttributes attributes;
        public readonly CaseInfo      caseinfo;
        public readonly byte          ctime_ms;
        public readonly ushort        ctime;
        public readonly ushort        cdate;
        public readonly ushort        adate;
        public readonly ushort        ea_handle;
        public readonly ushort        mtime;
        public readonly ushort        mdate;
        public readonly ushort        start_cluster;
        public readonly uint          size;
    }

#endregion

#region Nested type: EaHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct EaHeader
    {
        public readonly ushort  magic;
        public readonly ushort  cluster;
        public readonly EaFlags flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] filename;
        public readonly uint   unknown;
        public readonly ushort zero;
    }

#endregion

#region Nested type: Fat32ParameterBlock

    /// <summary>FAT32 Parameter Block</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Fat32ParameterBlock
    {
        /// <summary>x86 jump</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>Bytes per sector</summary>
        public ushort bps;
        /// <summary>Sectors per cluster</summary>
        public byte spc;
        /// <summary>Reserved sectors between BPB and FAT</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte fats_no;
        /// <summary>Number of entries on root directory, set to 0</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume, set to 0</summary>
        public ushort sectors;
        /// <summary>Media descriptor</summary>
        public readonly byte media;
        /// <summary>Sectors per FAT, set to 0</summary>
        public readonly ushort spfat;
        /// <summary>Sectors per track</summary>
        public ushort sptrk;
        /// <summary>Heads</summary>
        public readonly ushort heads;
        /// <summary>Hidden sectors before BPB</summary>
        public uint hsectors;
        /// <summary>Sectors in volume</summary>
        public uint big_sectors;
        /// <summary>Sectors per FAT</summary>
        public uint big_spfat;
        /// <summary>FAT flags</summary>
        public readonly ushort mirror_flags;
        /// <summary>FAT32 version</summary>
        public readonly ushort version;
        /// <summary>Cluster of root directory</summary>
        public readonly uint root_cluster;
        /// <summary>Sector of FSINFO structure</summary>
        public readonly ushort fsinfo_sector;
        /// <summary>Sector of FAT32PB backup</summary>
        public readonly ushort backup_sector;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] reserved;
        /// <summary>Drive number</summary>
        public readonly byte drive_no;
        /// <summary>Volume flags</summary>
        public readonly byte flags;
        /// <summary>Signature, should be 0x29</summary>
        public readonly byte signature;
        /// <summary>Volume serial number</summary>
        public readonly uint serial_no;
        /// <summary>Volume label, 11 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] volume_label;
        /// <summary>Filesystem type, 8 bytes, space-padded, must be "FAT32   "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] fs_type;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 419)]
        public readonly byte[] boot_code;
        /// <summary>Always 0x55 0xAA.</summary>
        public readonly ushort boot_signature;
    }

#endregion

#region Nested type: Fat32ParameterBlockShort

    /// <summary>FAT32 Parameter Block</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Fat32ParameterBlockShort
    {
        /// <summary>x86 jump</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>Reserved sectors between BPB and FAT</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte fats_no;
        /// <summary>Number of entries on root directory, set to 0</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume, set to 0</summary>
        public ushort sectors;
        /// <summary>Media descriptor</summary>
        public readonly byte media;
        /// <summary>Sectors per FAT, set to 0</summary>
        public readonly ushort spfat;
        /// <summary>Sectors per track</summary>
        public readonly ushort sptrk;
        /// <summary>Heads</summary>
        public readonly ushort heads;
        /// <summary>Hidden sectors before BPB</summary>
        public readonly uint hsectors;
        /// <summary>Sectors in volume</summary>
        public uint big_sectors;
        /// <summary>Sectors per FAT</summary>
        public readonly uint big_spfat;
        /// <summary>FAT flags</summary>
        public readonly ushort mirror_flags;
        /// <summary>FAT32 version</summary>
        public readonly ushort version;
        /// <summary>Cluster of root directory</summary>
        public readonly uint root_cluster;
        /// <summary>Sector of FSINFO structure</summary>
        public readonly ushort fsinfo_sector;
        /// <summary>Sector of FAT32PB backup</summary>
        public readonly ushort backup_sector;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] reserved;
        /// <summary>Drive number</summary>
        public readonly byte drive_no;
        /// <summary>Volume flags</summary>
        public readonly byte flags;
        /// <summary>Signature, should be 0x28</summary>
        public readonly byte signature;
        /// <summary>Volume serial number</summary>
        public readonly uint serial_no;
        /// <summary>Volume label, 11 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] reserved2;
        /// <summary>Sectors in volume if <see cref="big_sectors" /> equals 0</summary>
        public ulong huge_sectors;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 420)]
        public readonly byte[] boot_code;
        /// <summary>Always 0x55 0xAA.</summary>
        public readonly ushort boot_signature;
    }

#endregion

#region Nested type: FatDirNode

    sealed class FatDirNode : IDirNode
    {
        internal CompleteDirectoryEntry[] Entries;
        internal int                      Position;

#region IDirNode Members

        /// <inheritdoc />
        public string Path { get; init; }

#endregion
    }

#endregion

#region Nested type: FatFileNode

    sealed class FatFileNode : IFileNode
    {
        internal uint[] Clusters;

#region IFileNode Members

        /// <inheritdoc />
        public string Path { get; init; }

        /// <inheritdoc />
        public long Length { get; init; }

        /// <inheritdoc />
        public long Offset { get; set; }

#endregion
    }

#endregion

#region Nested type: FsInfoSector

    /// <summary>FAT32 FS Information Sector</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct FsInfoSector
    {
        /// <summary>Signature must be <see cref="FAT.FSINFO_SIGNATURE1" /></summary>
        public readonly uint signature1;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)]
        public readonly byte[] reserved1;
        /// <summary>Signature must be <see cref="FAT.FSINFO_SIGNATURE2" /></summary>
        public readonly uint signature2;
        /// <summary>Free clusters</summary>
        public readonly uint free_clusters;
        /// <summary>  cated cluster</summary>
        public readonly uint last_cluster;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] reserved2;
        /// <summary>Signature must be <see cref="FAT.FSINFO_SIGNATURE3" /></summary>
        public readonly uint signature3;
    }

#endregion

#region Nested type: HumanDirectoryEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HumanDirectoryEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] name1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] extension;
        public readonly FatAttributes attributes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] name2;
        public readonly ushort mtime;
        public readonly ushort mdate;
        public readonly ushort start_cluster;
        public readonly uint   size;
    }

#endregion

#region Nested type: HumanParameterBlock

    /// <summary>Human68k Parameter Block, big endian, 512 bytes even on 256 bytes/sector.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct HumanParameterBlock
    {
        /// <summary>68k bra.S</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly byte[] jump;
        /// <summary>OEM Name, 16 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] oem_name;
        /// <summary>Bytes per cluster</summary>
        public readonly ushort bpc;
        /// <summary>Unknown, seen 1, 2 and 16</summary>
        public readonly byte unknown1;
        /// <summary>Unknown, always 512?</summary>
        public readonly ushort unknown2;
        /// <summary>Unknown, always 1?</summary>
        public readonly byte unknown3;
        /// <summary>Number of entries on root directory</summary>
        public readonly ushort root_ent;
        /// <summary>Clusters, set to 0 if more than 65536</summary>
        public readonly ushort clusters;
        /// <summary>Media descriptor</summary>
        public readonly byte media;
        /// <summary>Clusters per FAT, set to 0</summary>
        public readonly byte cpfat;
        /// <summary>Clustersin volume</summary>
        public readonly uint big_clusters;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 478)]
        public readonly byte[] boot_code;
    }

#endregion

#region Nested type: LfnEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct LfnEntry
    {
        public readonly byte sequence;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] name1;
        public readonly FatAttributes attributes;
        public readonly byte          type;
        public readonly byte          checksum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] name2;
        public readonly ushort start_cluster;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] name3;
    }

#endregion

#region Nested type: MsxParameterBlock

    /// <summary>BIOS Parameter Block as used by MSX-DOS 2.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct MsxParameterBlock
    {
        /// <summary>x86 loop</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>Reserved sectors between BPB and FAT (inclusive)</summary>
        public readonly ushort rsectors;
        /// <summary>Number of FATs</summary>
        public readonly byte fats_no;
        /// <summary>Number of entries on root directory</summary>
        public readonly ushort root_ent;
        /// <summary>Sectors in volume</summary>
        public ushort sectors;
        /// <summary>Media descriptor</summary>
        public readonly byte media;
        /// <summary>Sectors per FAT</summary>
        public readonly ushort spfat;
        /// <summary>Sectors per track</summary>
        public readonly ushort sptrk;
        /// <summary>Heads</summary>
        public readonly ushort heads;
        /// <summary>Hidden sectors before BPB</summary>
        public readonly ushort hsectors;
        /// <summary>Jump for MSX-DOS 1 boot code</summary>
        public readonly ushort msxdos_jmp;
        /// <summary>Set to "VOL_ID" by MSX-DOS 2</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] vol_id;
        /// <summary>Bigger than 0 if there are deleted files (MSX-DOS 2)</summary>
        public readonly byte undelete_flag;
        /// <summary>Volume serial number (MSX-DOS 2)</summary>
        public readonly uint serial_no;
        /// <summary>Reserved (MSX-DOS 2)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] reserved;
        /// <summary>Jump for MSX-DOS 2 boot code (MSX-DOS 2)</summary>
        public readonly ushort msxdos2_jmp;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 460)]
        public readonly byte[] boot_code;
        /// <summary>Always 0x55 0xAA.</summary>
        public readonly ushort boot_signature;
    }

#endregion

#region Nested type: UmsdosDirectoryEntry

    /// <summary>This structure is 256 bytes large, depending on the name, only part of it is written to disk</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct UmsdosDirectoryEntry
    {
        /// <summary>if == 0, then this entry is not used</summary>
        public readonly byte name_len;
        /// <summary>UMSDOS_xxxx</summary>
        public readonly UmsdosFlags flags;
        /// <summary>How many hard links point to this entry</summary>
        public readonly ushort nlink;
        /// <summary>Owner user id</summary>
        public readonly int uid;
        /// <summary>Group id</summary>
        public readonly int gid;
        /// <summary>Access time</summary>
        public readonly int atime;
        /// <summary>Last modification time</summary>
        public readonly int mtime;
        /// <summary>Creation time</summary>
        public readonly int ctime;
        /// <summary>major and minor number of a device</summary>
        public readonly uint rdev;
        /*  */
        /// <summary>Standard UNIX permissions bits + type of</summary>
        public readonly ushort mode;
        /// <summary>unused bytes for future extensions</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] spare;
        /// <summary>
        ///     Not '\0' terminated but '\0' padded, so it will allow for adding news fields in this record by reducing the
        ///     size of name[]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = UMSDOS_MAXNAME)]
        public readonly byte[] name;
    }

#endregion

#region Nested type: UmsdosFlags

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum UmsdosFlags : byte
    {
        /// <summary>Never show this entry in directory search</summary>
        UMSDOS_HIDDEN = 1,
        /// <summary>It is a (pseudo) hard link</summary>
        UMSDOS_HLINK = 2
    }

#endregion
}