// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DiscImageChef.CommonTypes;
using DiscImageChef.ImagePlugins;

namespace DiscImageChef.PartPlugins
{
    public class BSD : PartPlugin
    {
        public const uint DISKMAGIC = 0x82564557;

        public BSD()
        {
            Name = "BSD disklabel";
            PluginUUID = new Guid("246A6D93-4F1A-1F8A-344D-50187A5513A9");
        }

        public override bool GetInformation(ImagePlugin imagePlugin, out List<Partition> partitions)
        {
            partitions = new List<Partition>();

            byte[] sector = imagePlugin.ReadSector(0);
            if(sector.Length < 512)
                return false;
            bool found = false;

            DiskLabel dl = GetDiskLabel(sector);

            if(dl.d_magic == DISKMAGIC || dl.d_magic2 == DISKMAGIC)
                found = true;
            else
            {
                sector = imagePlugin.ReadSector(1);

                dl = GetDiskLabel(sector);

                found |= (dl.d_magic == DISKMAGIC || dl.d_magic2 == DISKMAGIC);
            }

            if(found)
            {
                ulong counter = 0;

                foreach(BSDPartition entry in dl.d_partitions)
                {
                    Partition part = new Partition();
                    part.PartitionStartSector = entry.p_offset;
                    part.PartitionStart = (entry.p_offset * dl.d_secsize);
                    part.PartitionLength = entry.p_size;
                    part.PartitionSectors = (entry.p_size * dl.d_secsize);
                    part.PartitionType = fsTypeToString(entry.p_fstype);
                    part.PartitionSequence = counter;
                    if(entry.p_fstype != fsType.Unused)
                    {
                        partitions.Add(part);
                        counter++;
                    }
                }
            }

            return found;
        }

        /// <summary>Drive type</summary>
        public enum dType : ushort
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

        /// <summary>Filesystem type</summary>
        public enum fsType : byte
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

        /// <summary>
        /// Drive flags
        /// </summary>
        [Flags]
        public enum dFlags : uint
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DiskLabel
        {
            /// <summary><see cref="DISKMAGIC"/></summary>
            public uint d_magic;
            /// <summary><see cref="dType"/></summary>
            public dType d_type;
            /// <summary>Disk subtype</summary>
            public ushort d_subtype;
            /// <summary>Type name</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] d_typename;
            /// <summary>Pack identifier</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] d_packname;
            /// <summary>Bytes per sector</summary>
            public uint d_secsize;
            /// <summary>Sectors per track</summary>
            public uint d_nsectors;
            /// <summary>Tracks per cylinder</summary>
            public uint d_ntracks;
            /// <summary>Cylinders per unit</summary>
            public uint d_ncylinders;
            /// <summary>Sectors per cylinder</summary>
            public uint d_secpercyl;
            /// <summary>Sectors per unit</summary>
            public uint d_secperunit;
            /// <summary>Spare sectors per track</summary>
            public ushort d_sparespertrack;
            /// <summary>Spare sectors per cylinder</summary>
            public ushort d_sparespercyl;
            /// <summary>Alternate cylinders</summary>
            public uint d_acylinders;
            /// <summary>Rotational speed</summary>
            public ushort d_rpm;
            /// <summary>Hardware sector interleave</summary>
            public ushort d_interleave;
            /// <summary>Sector 0 skew per track</summary>
            public ushort d_trackskew;
            /// <summary>Sector 0 sker per cylinder</summary>
            public ushort d_cylskeew;
            /// <summary>Head switch time in microseconds</summary>
            public uint d_headswitch;
            /// <summary>Track to track seek in microseconds</summary>
            public uint d_trkseek;
            /// <summary><see cref="dFlags"/></summary>
            public dFlags d_flags;
            /// <summary>Drive-specific information</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public uint[] d_drivedata;
            /// <summary>Reserved</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            /// <summary></summary>
            public uint[] d_spare;
            /// <summary><see cref="DISKMAGIC"/> again</summary>
            public uint d_magic2;
            /// <summary>XOR of data</summary>
            public ushort d_checksum;
            /// <summary>How many partitions</summary>
            public ushort d_npartitions;
            /// <summary>Size of boot area in bytes</summary>
            public uint d_bbsize;
            /// <summary>Maximum size of superblock in bytes</summary>
            public uint d_sbsize;
            /// <summary>Partitions</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public BSDPartition[] d_partitions;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BSDPartition
        {
            /// <summary>Sectors in partition</summary>
            public uint p_size;
            /// <summary>Starting sector</summary>
            public uint p_offset;
            /// <summary>Fragment size</summary>
            public uint p_fsize;
            /// <summary>Filesystem type, <see cref="fsType"/></summary>
            public fsType p_fstype;
            /// <summary>Fragment size</summary>
            public byte p_frag;
            /// <summary>Cylinder per group</summary>
            public ushort p_cpg;
        }

        public static string fsTypeToString(fsType typ)
        {
            switch(typ)
            {
                case fsType.Unused:
                    return "Unused entry";
                case fsType.Swap:
                    return "Swap partition";
                case fsType.V6:
                    return "UNIX 6th Edition";
                case fsType.V7:
                    return "UNIX 7th Edition";
                case fsType.SystemV:
                    return "UNIX System V";
                case fsType.V7_1K:
                    return "UNIX 7th Edition with 1K blocks";
                case fsType.V8:
                    return "UNIX 8th Edition with 4K blocks";
                case fsType.BSDFFS:
                    return "4.2BSD Fast File System";
                case fsType.BSDLFS:
                    return "4.4LFS";
                case fsType.HPFS:
                    return "HPFS";
                case fsType.ISO9660:
                    return "ISO9660";
                case fsType.Boot:
                    return "Boot";
                case fsType.AFFS:
                    return "Amiga FFS";
                case fsType.HFS:
                    return "Apple HFS";
                case fsType.ADVfs:
                    return "Digital Advanced File System";
                case fsType.LSMpublic:
                    return "Digital LSM Public Region";
                case fsType.LSMprivate:
                    return "Digital LSM Private Region";
                case fsType.LSMsimple:
                    return "Digital LSM Simple Disk";
                case fsType.CCD:
                    return "Concatenated disk";
                case fsType.JFS2:
                    return "IBM JFS2";
                case fsType.HAMMER:
                    return "Hammer";
                case fsType.HAMMER2:
                    return "Hammer2";
                case fsType.UDF:
                    return "UDF";
                case fsType.EFS:
                    return "EFS";
                case fsType.ZFS:
                    return "ZFS";
                case fsType.NANDFS:
                    return "FreeBSD nandfs";
                case fsType.Other:
                    return "Other or unknown";
                default:
                    return "Unknown";
            }
        }

        public static DiskLabel GetDiskLabel(byte[] disklabel)
        {
            DiskLabel dl = new DiskLabel();
            IntPtr dlPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(disklabel, 0, dlPtr, 512);
            dl = (DiskLabel)Marshal.PtrToStructure(dlPtr, typeof(DiskLabel));
            Marshal.FreeHGlobal(dlPtr);
            return dl;
        }
    }
}