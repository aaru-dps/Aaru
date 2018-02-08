DiscImageChef v4.0.99.0

Disc Image Chef (because "swiss-army-knife" is used too much)

Copyright © 2011-2018 Natalia Portillo <claunia@claunia.com>

[![Build Status](https://travis-ci.org/claunia/DiscImageChef.svg?branch=master)](https://travis-ci.org/claunia/DiscImageChef)[![Build status](https://ci.appveyor.com/api/projects/status/vim4c8h028pn5oys?svg=true)](https://ci.appveyor.com/project/claunia/discimagechef)

You can see statistics and device reports [here](http://discimagechef.claunia.com/Statistics.aspx)

DiscImageChef is a fully featured media dump management solution. You usually know media dumps
as disc images, disk images, tape images, etc.

With DiscImageChef you can analyze a media dump, extract files from it (for supported
filesystems), compare two of them, create them from real media using the appropriate drive,
create a sidecar metadata with information about the media dump, and a lot of other features
that commonly would require you to use separate applications.

To see last changes, check the [changelog](Changelog.md).
To see list of pending things to do, check the [TODO list](TODO.md).

If you want to contribute in any way please read the [contributing guide](CONTRIBUTING.md).

System requirements
===================
DiscImageChef should work under any operating system where there is [Mono](http://www.mono-project.com/)
or [.NET Framework](https://www.microsoft.com/net/download).
It has been tested using Mono 3.0 and .NET Framework 4.0. However recommended versions are
Mono 5.0 and .NET Framework 4.6. .NET Core is untested.

Usage
=====

DiscImageChef.exe 

And read help.

Or read the [wiki](https://github.com/claunia/DiscImageChef/wiki).

Features
========
* Analyzes a disk image getting information about the disk itself and analyzes partitions and filesystems inside them
* Can checksum the disks (and if optical disc, separate tracks) user-data (tags and metadata coming soon)
* Can compare two disk images, even different formats, for different sectors and/or metadata
* Can list and extract contents from filesystems that support that
* Can read several disk image formats.
* Can read several known sector by sector formats with variable bytes per sector.
* Can read standard sector by sector copies for optical and magnetic discs with constant bytes per sector.
* Can verify sectors or disk images if supported by the underlying format

Supported disk image formats
============================
* Alcohol 120% Media Descriptor Structure (.MDS/.MDF)
* Any 512 bytes/sector disk image format (sector by sector copy, aka raw)
* Apple 2IMG (used with Apple // emulators)
* Apple Disk Archival/Retrieval Tool (DART)
* Apple DiskCopy 4.2
* Apple II nibble images (NIB)
* Apple New Disk Image Format (NDIF, requires Resource Fork)
* Apple Universal Disk Image Format (UDIF), including obsolete (previous than DiskCopy 6) versions
* Apridisk disk image formats (for ACT Apricot disks)
* Anex86 disk images (.FDI for floppies, .HDI for hard disks)
* BlindWrite 4 TOC files (.BWT/.BWI/.BWS)
* BlindWrite 5/6 TOC files (.B5T/.B5I and .B6T/.B6I)
* CDRDAO TOC sheets
* CDRWin cue/bin cuesheets, including ones with ISOBuster extensions
* CisCopy disk image (aka DC-File, .DCF)
* CPCEMU Disk file and Extended Disk File
* CopyQM
* Dave Dunfield IMD
* Digital Research DiskCopy
* DiscJuggler images
* Dreamcast GDI
* HD-Copy disk images
* IBM SaveDskF
* MAME Compressed Hunks of Data (CHD)
* MAXI Disk disk images (HDK)
* Microsoft VHDX
* Most known sector by sector copies of floppies with 128, 256, 319 and 1024 bytes/sector.
* Most known sector by sector copies with different bytes/sector on track 0.
* Nero Burning ROM (both image formats)
* Parallels Hard Disk Image (HDD) version 2
* Partclone disk images
* Partimage disk images
* QEMU Copy-On-Write versions 1, 2 and 3 (QCOW and QCOW2)
* QEMU Enhanced Disk (QED)
* Quasi88 disk images (.D77/.D88)
* Ray Arachelian's Disk IMage (.DIM)
* RS-IDE hard disk images
* Sector by sector copies of Microsoft's DMF floppies
* Spectrum floppy disk image (.FDI)
* T98 hard disk images (.THD)
* T98-Next hard disk images (.NHD)
* TeleDisk
* VMware VMDK and COWD images
* Virtual98 disk images
* Virtual PC fixed size, dynamic size and differencing (undo) disk images
* VirtualBox Disk Image (VDI)
* X68k DIM disk image files (.DIM)
* XDF disk images (as created by IBM's XDFCOPY)

Supported partitioning schemes
==============================
* Acorn Linux and RISCiX partitions
* ACT Apricot partitions
* Amiga Rigid Disk Block (RDB)
* Apple Partition Map
* Atari AHDI and ICDPro
* BSD disklabels
* BSD slices inside MBR
* DEC disklabels
* DragonFly BSD 64-bit disklabel
* EFI GUID Partition Table (GPT)
* Human68k (Sharp X68000) partitions table
* Microsoft/IBM/Intel Master Boot Record (MBR)
* Minix subpartitions inside MBR
* NEC PC9800 partitions
* NeXT disklabel
* Plan9 partition table
* Rio Karma partitions
* SGI volume headers
* Solaris slices inside MBR
* Sun disklabel
* UNIX VTOC and disklabel
* UNIX VTOC inside MBR
* Xbox 360 hard coded partitions
* XENIX partition table

Supported file systems for read-only operations
===============================================
* Apple DOS file system
* Apple Lisa file system
* Apple Macintosh File System (MFS)
* CP/M file system
* U.C.S.D Pascal file system

Supported file systems for identification and information only
==============================================================
* 3DO Opera file system
* Acorn Advanced Disc Filing System
* Alexander Osipov DOS (AO-DOS for Electronika BK-0011) file system
* Amiga Fast File System v2, untested
* Amiga Fast File System, with international characters, directory cache and multi-user patches
* Amiga Original File System, with international characters, directory cache and multi-user patches
* Apple File System (preliminary detection until on-disk layout is stable)
* Apple Hierarchical File System (HFS)
* Apple Hierarchical File System+ (HFS+)
* Apple ProDOS / SOS file system
* AtheOS file system
* B-tree file system (btrfs)
* BSD Fast File System (FFS) / Unix File System (UFS)
* BSD Unix File System 2 (UFS2)
* BeOS filesystem
* CD-i file system
* Coherent UNIX file system
* Commodore 1540/1541/1571/1581 filesystems
* Cram file system
* DEC RT-11 file system
* DEC Files-11 (only checked with On Disk Structure 2, ODS-2)
* dump(8) (Old historic BSD, AIX, UFS and UFS2 types)
* ECMA-67: 130mm Flexible Disk Cartridge Labelling and File Structure for Information Interchange
* Flash-Friendly File System (F2FS)
* Fossil file system (from Plan9)
* HAMMER file system
* High Performance Optical File System (HPOFS)
* High Sierra Format
* HP Logical Interchange Format
* IBM Journaling File System (JFS)
* ISO9660
* Linux extended file system
* Linux extended file system 2
* Linux extended file system 3
* Linux extended file system 4
* Locus file system
* MicroDOS file system
* Microsoft 12-bit File Allocation Table (FAT12), including Atari ST extensions
* Microsoft 16-bit File Allocation Table (FAT16)
* Microsoft 32-bit File Allocation Table (FAT32), including FAT+ extension
* Microsoft Extended File Allocation Table (exFAT)
* Microsoft New Technology File System (NTFS)
* Microsoft/IBM High Performance File System (HPFS)
* Minix v2 file system
* Minix v3 file system
* NEC PC-Engine executable
* NEC PC-FX executable
* NILFS2
* OS-9 Random Block File
* Professional File System
* QNX4 and QNX6 filesystems
* Reiser file systems
* SGI Extent File System (EFS)
* SGI XFS
* SmartFileSystem
* SolarOS file system
* Squash file system
* UNICOS file system
* UNIX System V file system
* UNIX Version 7 file system
* Universal Disk Format (UDF)
* UnixWare boot file system
* VMware file system (VMFS)
* Veritas file system
* Xbox filesystems
* Xenix file system
* Xia filesystem
* Zettabyte File System (ZFS)

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
* Apple PCExchange (FINDER.DAT & RESOURCE.FRK)
* AppleDouble
* AppleSingle
* BZip2
* GZip
* LZip
* MacBinary I, II, III
* XZ

Partially supported disk image formats
======================================
These disk image formats cannot be read, but their contents can be checksummed on sidecar creation
* DiscFerret
* KryoFlux STREAM
* SuperCardPro