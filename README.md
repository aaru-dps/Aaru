DiscImageChef v3.1.0.0
======================

Disc Image Chef (because "swiss-army-knife" is used too much)

Copyright © 2011-2016 Natalia Portillo <claunia@claunia.com>

[![Build Status](https://travis-ci.org/claunia/DiscImageChef.svg?branch=master)](https://travis-ci.org/claunia/DiscImageChef)[![Build status](https://ci.appveyor.com/api/projects/status/vim4c8h028pn5oys?svg=true)](https://ci.appveyor.com/project/claunia/discimagechef)

Usage
=====

DiscImageChef.exe 

And read help.

Works under any operating system where there is Mono or .NET Framework. Tested with Mono 3.0.

Features
========

* Can read several disk image formats.
* Can read standard sector by sector copies for optical and magnetic discs with constant bytes per sector.
* Can read several known sector by sector formats with variable bytes per sector.
* Analyzes a disk image getting information about the disk itself and analyzes partitions and filesystems inside them
* Can compare two disk images, even different formats, for different sectors and/or metadata
* Can verify sectors or disk images if supported by the underlying format
* Can checksum the disks (and if optical disc, separate tracks) user-data (tags and metadata coming soon)
* Can list and extract contents from filesystems that support that

Supported disk image formats
============================
* Any 512 bytes/sector disk image format (sector by sector copy, aka raw)
* Most known sector by sector copies of floppies with 128, 256, 319 and 1024 bytes/sector.
* Most known sector by sector copies with different bytes/sector on track 0.
* XDF disk images (as created by IBM's XDFCOPY)
* Sector by sector copies of Microsoft's DMF floppies
* CDRWin cue/bin cuesheets, including ones with ISOBuster extensions
* Apple DiskCopy 4.2
* TeleDisk (without compression)
* Nero Burning ROM (both image formats)
* Apple 2IMG (used with Apple // emulators)
* Virtual PC fixed size, dynamic size and differencing (undo) disk images
* CDRDAO TOC sheets
* Dreamcast GDI
* CopyQM
* Alcohol 120% Media Descriptor Structure (.MDS/.MDF)
* BlindWrite 4 TOC files (.BWT/.BWI/.BWS)
* BlindWrite 5/6 TOC files (.B5T/.B5I and .B6T/.B6I)
* X68k DIM disk image files (.DIM)
* CPCEMU Disk file and Extended Disk File
* QEMU Copy-On-Write versions 1, 2 and 3 (QCOW and QCOW2)
* QEMU Enhanced Disk (QED)
* Parallels Hard Disk Image (HDD) version 2
* VirtualBox Disk Image (VDI)
* Microsoft VHDX
* VMware VMDK and COWD images
* Apple Universal Disk Image Format (UDIF), including obsolete (previous than DiskCopy 6) versions
* Apple New Disk Image Format (NDIF, requires Resource Fork)
* Apple Disk Archival/Retrieval Tool (DART)

Supported partitioning schemes
==============================
* Microsoft/IBM/Intel Master Boot Record (MBR)
* BSD slices inside MBR
* Solaris slices inside MBR
* Minix subpartitions inside MBR
* UNIX VTOC inside MBR
* Apple Partition Map
* NeXT disklabel
* Amiga Rigid Disk Block (RDB)
* Atari AHDI and ICDPro
* Sun disklabel
* EFI GUID Partition Table (GPT)
* Acorn Linux and RISCiX partitions
* BSD disklabels
* DragonFly BSD 64-bit disklabel
* DEC disklabels
* NEC PC9800 partitions
* SGI volume headers
* Rio Karma partitions
* UNIX VTOC and disklabel
* Human68k (Sharp X68000) partitions table

Supported file systems for read-only operations
===============================================
* Apple Lisa file system
* Apple Macintosh File System (MFS)
* U.C.S.D Pascal file system
* CP/M file system

Supported file systems for identification and information only
==============================================================
* Apple Hierarchical File System (HFS)
* Apple Hierarchical File System+ (HFS+)
* Apple ProDOS / SOS file system
* BeOS filesystem
* Linux extended file system
* Linux extended file system 2
* Linux extended file system 3
* Linux extended file system 4
* Microsoft 12-bit File Allocation Table (FAT12), including Atari ST extensions
* Microsoft 16-bit File Allocation Table (FAT16)
* Microsoft 32-bit File Allocation Table (FAT32), including FAT+ extension
* BSD Fast File System (FFS) / Unix File System (UFS)
* BSD Unix File System 2 (UFS2)
* Microsoft/IBM High Performance File System (HPFS)
* ISO9660
* Minix v2 file system
* Minix v3 file system
* Microsoft New Technology File System (NTFS)
* DEC Files-11 (only checked with On Disk Structure 2, ODS-2)
* 3DO Opera file system
* NEC PC-Engine file system
* SolarOS file system
* UNIX System V file system
* UNIX Version 7 file system
* Xenix file system
* Coherent UNIX file system
* UnixWare boot file system
* Amiga Original File System, with international characters, directory cache and multi-user patches
* Amiga Fast File System, with international characters, directory cache and multi-user patches
* Amiga Fast File System v2, untested
* Acorn Advanced Disc Filing System
* B-tree file system (btrfs)
* Apple File System (preliminary detection until on-disk layout is stable)
* Microsoft Extended File Allocation Table (exFAT)
* QNX4 and QNX6 filesystems
* IBM Journaling File System (JFS)
* Flash-Friendly File System (F2FS)
* NILFS2
* Reiser file systems
* SGI XFS
* Professional File System
* SmartFileSystem
* VMware file system (VMFS)

Supported checksums
===================
* Adler-32
* CRC-16
* CRC-32
* CRC-64
* Fletcher-16
* Fletcher-32
* MD5
* RMD160
* SHA-1
* SHA-2 (256, 384 and 512 bits)
* SpamSum (fuzzy hashing)

Supported filters
=================
* GZip
* AppleDouble
* AppleSingle
* MacBinary I, II, III
* BZip2
* Apple PCExchange (FINDER.DAT & RESOURCE.FRK)

Changelog
=========

See Changelog file.

To-Do
=====

See TODO file.