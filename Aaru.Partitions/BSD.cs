// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BSD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages BSD disklabels.
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

/// <inheritdoc />
/// <summary>Implements decoding of BSD disklabels</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class BSD : IPartition
{
    const uint DISK_MAGIC = 0x82564557;
    const uint DISK_CIGAM = 0x57455682;
    /// <summary>Maximum size of a disklabel with 22 partitions</summary>
    const uint MAX_LABEL_SIZE = 500;
    const string MODULE_NAME = "BSD disklabel plugin";
    /// <summary>Known sector locations for BSD disklabel</summary>
    readonly ulong[] _labelLocations = [0, 1, 2, 9];
    /// <summary>Known byte offsets for BSD disklabel</summary>
    readonly uint[] _labelOffsets = [0, 9, 64, 128, 516];

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.BSD_Name;

    /// <inheritdoc />
    public Guid Id => new("246A6D93-4F1A-1F8A-344D-50187A5513A9");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        partitions = [];
        uint run = (MAX_LABEL_SIZE + _labelOffsets.Last()) / imagePlugin.Info.SectorSize;

        if((MAX_LABEL_SIZE + _labelOffsets.Last()) % imagePlugin.Info.SectorSize > 0) run++;

        var dl    = new DiskLabel();
        var found = false;

        foreach(ulong location in _labelLocations)
        {
            if(location + run + sectorOffset >= imagePlugin.Info.Sectors) return false;

            ErrorNumber errno = imagePlugin.ReadSectors(location + sectorOffset, run, out byte[] tmp);

            if(errno != ErrorNumber.NoError) continue;

            foreach(uint offset in _labelOffsets)
            {
                var sector = new byte[MAX_LABEL_SIZE];

                if(offset + MAX_LABEL_SIZE > tmp.Length) break;

                Array.Copy(tmp, offset, sector, 0, MAX_LABEL_SIZE);
                dl = Marshal.ByteArrayToStructureLittleEndian<DiskLabel>(sector);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization
                                              .BSD_GetInformation_dl_magic_on_sector_0_at_offset_1_equals_2_X8_expected_3_X8,
                                           location + sectorOffset,
                                           offset,
                                           dl.d_magic,
                                           DISK_MAGIC);

                if((dl.d_magic != DISK_MAGIC || dl.d_magic2 != DISK_MAGIC) &&
                   (dl.d_magic != DISK_CIGAM || dl.d_magic2 != DISK_CIGAM))
                    continue;

                found = true;

                break;
            }

            if(found) break;
        }

        if(!found) return false;

        if(dl is { d_magic: DISK_CIGAM, d_magic2: DISK_CIGAM }) dl = SwapDiskLabel(dl);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_type = {0}",           dl.d_type);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_subtype = {0}",        dl.d_subtype);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_typename = {0}",       StringHandlers.CToString(dl.d_typename));
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_packname = {0}",       StringHandlers.CToString(dl.d_packname));
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_secsize = {0}",        dl.d_secsize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_nsectors = {0}",       dl.d_nsectors);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_ntracks = {0}",        dl.d_ntracks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_ncylinders = {0}",     dl.d_ncylinders);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_secpercyl = {0}",      dl.d_secpercyl);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_secperunit = {0}",     dl.d_secperunit);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_sparespertrack = {0}", dl.d_sparespertrack);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_sparespercyl = {0}",   dl.d_sparespercyl);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_acylinders = {0}",     dl.d_acylinders);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_rpm = {0}",            dl.d_rpm);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_interleave = {0}",     dl.d_interleave);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_trackskew = {0}",      dl.d_trackskew);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_cylskeew = {0}",       dl.d_cylskeew);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_headswitch = {0}",     dl.d_headswitch);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_trkseek = {0}",        dl.d_trkseek);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_flags = {0}",          dl.d_flags);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_drivedata[0] = {0}",   dl.d_drivedata[0]);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_drivedata[1] = {0}",   dl.d_drivedata[1]);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_drivedata[2] = {0}",   dl.d_drivedata[2]);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_drivedata[3] = {0}",   dl.d_drivedata[3]);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_drivedata[4] = {0}",   dl.d_drivedata[4]);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_spare[0] = {0}",       dl.d_spare[0]);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_spare[1] = {0}",       dl.d_spare[1]);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_spare[2] = {0}",       dl.d_spare[2]);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_spare[3] = {0}",       dl.d_spare[3]);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_spare[4] = {0}",       dl.d_spare[4]);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_magic2 = 0x{0:X8}",    dl.d_magic2);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_checksum = 0x{0:X8}",  dl.d_checksum);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_npartitions = {0}",    dl.d_npartitions);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_bbsize = {0}",         dl.d_bbsize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_sbsize = {0}",         dl.d_sbsize);

        ulong counter         = 0;
        var   addSectorOffset = false;

        for(var i = 0; i < dl.d_npartitions && i < 22; i++)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_partitions[i].p_offset = {0}", dl.d_partitions[i].p_offset);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dl.d_partitions[i].p_size = {0}", dl.d_partitions[i].p_size);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "dl.d_partitions[i].p_fstype = {0} ({1})",
                                       dl.d_partitions[i].p_fstype,
                                       FSTypeToString(dl.d_partitions[i].p_fstype));

            var part = new Partition
            {
                Start    = dl.d_partitions[i].p_offset * dl.d_secsize / imagePlugin.Info.SectorSize,
                Offset   = dl.d_partitions[i].p_offset                * dl.d_secsize,
                Length   = dl.d_partitions[i].p_size * dl.d_secsize   / imagePlugin.Info.SectorSize,
                Size     = dl.d_partitions[i].p_size                  * dl.d_secsize,
                Type     = FSTypeToString(dl.d_partitions[i].p_fstype),
                Sequence = counter,
                Scheme   = Name
            };

            if(dl.d_partitions[i].p_fstype == fsType.Unused) continue;

            // Crude and dirty way to know if the disklabel is relative to its parent partition...
            if(dl.d_partitions[i].p_offset < sectorOffset && !addSectorOffset) addSectorOffset = true;

            if(addSectorOffset)
            {
                part.Start  += sectorOffset;
                part.Offset += sectorOffset * imagePlugin.Info.SectorSize;
            }

            AaruConsole.DebugWriteLine(MODULE_NAME, "part.start = {0}", part.Start);
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.BSD_GetInformation_Adding_it);
            partitions.Add(part);
            counter++;
        }

        return partitions.Count > 0;
    }

#endregion

    internal static string FSTypeToString(fsType typ)
    {
        return typ switch
               {
                   fsType.Unused                  => Localization.Unused_entry,
                   fsType.Swap                    => Localization.Swap_partition,
                   fsType.V6                      => Localization.UNIX_6th_Edition,
                   fsType.V7                      => Localization.UNIX_7th_Edition,
                   fsType.SystemV                 => Localization.UNIX_System_V,
                   fsType.V7_1K                   => Localization.UNIX_7th_Edition_with_1K_blocks,
                   fsType.V8                      => Localization.UNIX_8th_Edition_with_4K_blocks,
                   fsType.BSDFFS                  => Localization._4_2_BSD_Fast_File_System,
                   fsType.BSDLFS                  => Localization._4_4_LFS,
                   fsType.HPFS                    => Localization.HPFS,
                   fsType.ISO9660                 => Localization.ISO9660,
                   fsType.Boot or fsType.SysVBoot => Localization.Boot,
                   fsType.AFFS                    => Localization.Amiga_FFS,
                   fsType.HFS                     => Localization.Apple_HFS,
                   fsType.ADVfs                   => Localization.Digital_Advanced_File_System,
                   fsType.LSMpublic               => Localization.Digital_LSM_Public_Region,
                   fsType.LSMprivate              => Localization.Digital_LSM_Private_Region,
                   fsType.LSMsimple               => Localization.Digital_LSM_Simple_Disk,
                   fsType.CCD                     => Localization.Concatenated_disk,
                   fsType.JFS2                    => Localization.IBM_JFS2,
                   fsType.HAMMER                  => Localization.Hammer,
                   fsType.HAMMER2                 => Localization.Hammer2,
                   fsType.UDF                     => Localization.UDF,
                   fsType.EFS                     => Localization.EFS,
                   fsType.ZFS                     => Localization.ZFS,
                   fsType.NANDFS                  => Localization.FreeBSD_nandfs,
                   fsType.MSDOS                   => Localization.FAT,
                   fsType.Other                   => Localization.Other_or_unknown,
                   _                              => Localization.Unknown_partition_type
               };
    }

    static DiskLabel SwapDiskLabel(DiskLabel dl)
    {
        dl = (DiskLabel)Marshal.SwapStructureMembersEndian(dl);

        for(var i = 0; i < dl.d_drivedata.Length; i++) dl.d_drivedata[i] = Swapping.Swap(dl.d_drivedata[i]);

        for(var i = 0; i < dl.d_spare.Length; i++) dl.d_spare[i] = Swapping.Swap(dl.d_spare[i]);

        for(var i = 0; i < dl.d_partitions.Length; i++)
            dl.d_partitions[i] = (BSDPartition)Marshal.SwapStructureMembersEndian(dl.d_partitions[i]);

        return dl;
    }

#region Nested type: BSDPartition

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BSDPartition
    {
        /// <summary>Sectors in partition</summary>
        public readonly uint p_size;
        /// <summary>Starting sector</summary>
        public readonly uint p_offset;
        /// <summary>Fragment size</summary>
        public readonly uint p_fsize;
        /// <summary>Filesystem type, <see cref="fsType" /></summary>
        public readonly fsType p_fstype;
        /// <summary>Fragment size</summary>
        public readonly byte p_frag;
        /// <summary>Cylinder per group</summary>
        public readonly ushort p_cpg;
    }

#endregion

#region Nested type: dFlags

    /// <summary>Drive flags</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [Flags]
    enum dFlags : uint
    {
        /// <summary>Removable media</summary>
        Removable = 0x01,
        /// <summary>Drive supports ECC</summary>
        ECC = 0x02,
        /// <summary>Drive supports bad sector forwarding</summary>
        BadSectorForward = 0x04,
        /// <summary>Disk emulator</summary>
        RAMDisk = 0x08,
        /// <summary>Can do back to back transfer</summary>
        Chain = 0x10,
        /// <summary>Dynamic geometry device</summary>
        DynamicGeometry = 0x20
    }

#endregion

#region Nested type: DiskLabel

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DiskLabel
    {
        /// <summary>
        ///     <see cref="BSD.DISK_MAGIC" />
        /// </summary>
        public readonly uint d_magic;
        /// <summary>
        ///     <see cref="dType" />
        /// </summary>
        public readonly dType d_type;
        /// <summary>Disk subtype</summary>
        public readonly ushort d_subtype;
        /// <summary>Type name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] d_typename;
        /// <summary>Pack identifier</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] d_packname;
        /// <summary>Bytes per sector</summary>
        public readonly uint d_secsize;
        /// <summary>Sectors per track</summary>
        public readonly uint d_nsectors;
        /// <summary>Tracks per cylinder</summary>
        public readonly uint d_ntracks;
        /// <summary>Cylinders per unit</summary>
        public readonly uint d_ncylinders;
        /// <summary>Sectors per cylinder</summary>
        public readonly uint d_secpercyl;
        /// <summary>Sectors per unit</summary>
        public readonly uint d_secperunit;
        /// <summary>Spare sectors per track</summary>
        public readonly ushort d_sparespertrack;
        /// <summary>Spare sectors per cylinder</summary>
        public readonly ushort d_sparespercyl;
        /// <summary>Alternate cylinders</summary>
        public readonly uint d_acylinders;
        /// <summary>Rotational speed</summary>
        public readonly ushort d_rpm;
        /// <summary>Hardware sector interleave</summary>
        public readonly ushort d_interleave;
        /// <summary>Sector 0 skew per track</summary>
        public readonly ushort d_trackskew;
        /// <summary>Sector 0 sker per cylinder</summary>
        public readonly ushort d_cylskeew;
        /// <summary>Head switch time in microseconds</summary>
        public readonly uint d_headswitch;
        /// <summary>Track to track seek in microseconds</summary>
        public readonly uint d_trkseek;
        /// <summary>
        ///     <see cref="dFlags" />
        /// </summary>
        public readonly dFlags d_flags;
        /// <summary>Drive-specific information</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly uint[] d_drivedata;
        /// <summary>Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly uint[] d_spare;
        /// <summary><see cref="BSD.DISK_MAGIC" /> again</summary>
        public readonly uint d_magic2;
        /// <summary>XOR of data</summary>
        public readonly ushort d_checksum;
        /// <summary>How many partitions</summary>
        public readonly ushort d_npartitions;
        /// <summary>Size of boot area in bytes</summary>
        public readonly uint d_bbsize;
        /// <summary>Maximum size of superblock in bytes</summary>
        public readonly uint d_sbsize;
        /// <summary>Partitions</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public readonly BSDPartition[] d_partitions;
    }

#endregion

#region Nested type: dType

    /// <summary>Drive type</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    enum dType : ushort
    {
        /// <summary>SMD, XSMD</summary>
        SMD = 1,
        /// <summary>MSCP</summary>
        MSCP = 2,
        /// <summary>Other DEC (rk, rl)</summary>
        DEC = 3,
        /// <summary>SCSI</summary>
        SCSI = 4,
        /// <summary>ESDI</summary>
        ESDI = 5,
        /// <summary>ST506 et al</summary>
        ST506 = 6,
        /// <summary>CS/80 on HP-IB</summary>
        HPIB = 7,
        /// <summary>HP Fiber-link</summary>
        HPFL = 8,
        /// <summary>Floppy</summary>
        FLOPPY = 10,
        /// <summary>Concatenated disk</summary>
        CCD = 11,
        /// <summary>uvnode pseudo-disk</summary>
        VND = 12,
        /// <summary>DiskOnChip</summary>
        DOC2K = 13,
        /// <summary>ATAPI</summary>
        ATAPI = 13,
        /// <summary>CMU RAIDframe</summary>
        RAID = 14,
        /// <summary>Logical disk</summary>
        LD = 15,
        /// <summary>IBM JFS 2</summary>
        JFS2 = 16,
        /// <summary>Cryptographic pseudo-disk</summary>
        CGD = 17,
        /// <summary>Vinum volume</summary>
        VINUM = 18,
        /// <summary>Flash memory devices</summary>
        FLASH = 19,
        /// <summary>Device-mapper pseudo-disk devices</summary>
        DM = 20,
        /// <summary>Rump virtual disk</summary>
        RUMPD = 21,
        /// <summary>Memory disk</summary>
        MD = 22
    }

#endregion

#region Nested type: fsType

    /// <summary>Filesystem type</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal enum fsType : byte
    {
        /// <summary>Unused entry</summary>
        Unused = 0,
        /// <summary>Swap partition</summary>
        Swap = 1,
        /// <summary>UNIX 6th Edition</summary>
        V6 = 2,
        /// <summary>UNIX 7th Edition</summary>
        V7 = 3,
        /// <summary>UNIX System V</summary>
        SystemV = 4,
        /// <summary>UNIX 7th Edition with 1K blocks</summary>
        V7_1K = 5,
        /// <summary>UNIX 8th Edition with 4K blocks</summary>
        V8 = 6,
        /// <summary>4.2BSD Fast File System</summary>
        BSDFFS = 7,
        /// <summary>MS-DOS filesystem</summary>
        MSDOS = 8,
        /// <summary>4.4LFS</summary>
        BSDLFS = 9,
        /// <summary>In use, unknown or unsupported</summary>
        Other = 10,
        /// <summary>HPFS</summary>
        HPFS = 11,
        /// <summary>ISO9660</summary>
        ISO9660 = 12,
        /// <summary>Boot partition</summary>
        Boot = 13,
        /// <summary>Amiga FFS</summary>
        AFFS = 14,
        /// <summary>Apple HFS</summary>
        HFS = 15,
        /// <summary>Acorn ADFS</summary>
        FileCore = 16,
        /// <summary>Digital Advanced File System</summary>
        ADVfs = 16,
        /// <summary>Digital LSM Public Region</summary>
        LSMpublic = 17,
        /// <summary>Linux ext2</summary>
        ext2 = 17,
        /// <summary>Digital LSM Private Region</summary>
        LSMprivate = 18,
        /// <summary>NTFS</summary>
        NTFS = 18,
        /// <summary>Digital LSM Simple Disk</summary>
        LSMsimple = 19,
        /// <summary>RAIDframe component</summary>
        RAID = 19,
        /// <summary>Concatenated disk component</summary>
        CCD = 20,
        /// <summary>IBM JFS2</summary>
        JFS2 = 21,
        /// <summary>Apple UFS</summary>
        AppleUFS = 22,
        /// <summary>Hammer filesystem</summary>
        HAMMER = 22,
        /// <summary>Hammer2 filesystem</summary>
        HAMMER2 = 23,
        /// <summary>UDF</summary>
        UDF = 24,
        /// <summary>System V Boot filesystem</summary>
        SysVBoot = 25,
        /// <summary>EFS</summary>
        EFS = 26,
        /// <summary>ZFS</summary>
        ZFS = 27,
        /// <summary>NiLFS</summary>
        NILFS = 27,
        /// <summary>Cryptographic disk</summary>
        CGD = 28,
        /// <summary>MINIX v3</summary>
        MINIX = 29,
        /// <summary>FreeBSD nandfs</summary>
        NANDFS = 30
    }

#endregion
}