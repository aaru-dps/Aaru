DiscImageChef v2.20
===================

Disc Image Chef (because "swiss-army-knife" is used too much)

Copyright © 2011-2014 Natalia Portillo <claunia@claunia.com>

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

Supported partitioning schemes
==============================
* Microsoft/IBM/Intel Master Boot Record (MBR)
* BSD slices inside MBR
* Solaris slices inside MBR
* Minix subpartitions inside MBR
* UNIX VTOC inside MBR
* Apple Partition Map
* NeXT disklabel

Supported file systems
======================
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
* Microsoft 32-bit File Allocation Table (FAT32)
* BSD Fast File System (FFS) / Unix File System (UFS)
* BSD Unix File System 2 (UFS2)
* Microsoft/IBM High Performance File System (HPFS)
* ISO9660
* Lisa Office System file system
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

Supported checksums
===================
* Adler-32
* CRC-16
* CRC-32
* CRC-64
* MD5
* RMD160
* SHA-1
* SHA-2 (256, 384 and 512 bits)

Changelog
=========

See Changelog file.

To-Do
=====

See TODO file.