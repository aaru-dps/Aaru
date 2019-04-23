DiscImageChef v4.5.99.1693

Disc Image Chef (because "swiss-army-knife" is used too much)

Copyright © 2011-2019 Natalia Portillo <claunia@claunia.com>

[![Build status](https://dev.azure.com/DiscImageChef/DiscImageChef/_apis/build/status/DiscImageChef-.NET%20Desktop-CI)](https://dev.azure.com/DiscImageChef/DiscImageChef/_build/latest?definitionId=4)
[![Build Status](https://travis-ci.org/claunia/DiscImageChef.svg?branch=master)](https://travis-ci.org/claunia/DiscImageChef)
[![Build status](https://ci.appveyor.com/api/projects/status/vim4c8h028pn5oys?svg=true)](https://ci.appveyor.com/project/claunia/discimagechef)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fclaunia%2FDiscImageChef.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2Fclaunia%2FDiscImageChef?ref=badge_shield)

You can see statistics and device reports [here](http://discimagechef.claunia.com/Stats)

DiscImageChef is a fully featured media dump management solution. You usually know media dumps
as disc images, disk images, tape images, etc.

With DiscImageChef you can analyze a media dump, extract files from it (for supported
filesystems), compare two of them, create them from real media using the appropriate drive,
create a sidecar metadata with information about the media dump, and a lot of other features
that commonly would require you to use separate applications.

To see last changes, check the [changelog](Changelog.md).
To see list of pending things to do, check the [TODO list](TODO.md).

If you want to contribute in any way please read the [contributing guide](CONTRIBUTING.md).

Stable releases in [Github](https://github.com/discimagechef/DiscImageChef/releases).
CI beta builds [here](https://goo.gl/6dZPMq).  
[![Feature Requests](http://feathub.com/discimagechef/DiscImageChef?format=svg)](http://feathub.com/discimagechef/DiscImageChef)


System requirements
===================
DiscImageChef should work under any operating system where there is [Mono](http://www.mono-project.com/)
or [.NET Framework](https://www.microsoft.com/net/download).
It has been tested using Mono 5.0 and .NET Framework 4.6.1. However recommended versions are
Mono 5.8 and .NET Framework 4.7.1. .NET Core is unsupported.

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
* Can list and extract contents from supported filesystems
* Can read several disk image formats.
* Can read several known sector by sector formats with variable bytes per sector.
* Can read standard sector by sector copies for optical and magnetic discs with constant bytes per sector.
* Can verify sectors or disk images if supported by the underlying format
* Can dump media from ATA, ATAPI, SCSI, USB, FireWire and SDHCI drives (magnetic disks, optical discs, magnetoptical disks, flash devices, memory cards and tapes) to several supported image formats.
* Can convert between image formats.
* Include an opensource archival image format with compression and deduplication.
* Can create standard open XML metadata from existing images.
* Can measure readability and speed of media (same that can be dumped, MHDD style)
* Has an online database with drive capabilities, and can report the capabilities of any drive.
* Works on any operating system and architecture where Mono or .NET Framework is supported (drive access requires Windows, Linux or FreeBSD).
* Has a graphical interface (work in progress)

Supported disk image formats (read-only)
========================================
* Apple Disk Archival/Retrieval Tool (DART)
* Apple II nibble images (NIB)
* BlindWrite 4 TOC files (.BWT/.BWI/.BWS)
* BlindWrite 5/6 TOC files (.B5T/.B5I and .B6T/.B6I)
* CopyQM
* CPCEMU Disk file and Extended Disk File
* Dave Dunfield IMD
* DiscJuggler images
* Dreamcast GDI
* HD-Copy disk images
* MAME Compressed Hunks of Data (CHD)
* Microsoft VHDX
* Nero Burning ROM (both image formats)
* Partclone disk images
* Partimage disk images
* Quasi88 disk images (.D77/.D88)
* Spectrum floppy disk image (.FDI)
* TeleDisk
* X68k DIM disk image files (.DIM)

Supported disk image formats (read and write)
=============================================
* Alcohol 120% Media Descriptor Structure (.MDS/.MDF)
* Anex86 disk images (.FDI for floppies, .HDI for hard disks)
* Any 512 bytes/sector disk image format (sector by sector copy, aka raw)
* Apple 2IMG (used with Apple // emulators)
* Apple DiskCopy 4.2
* Apple ][ Interleaved Disk Image
* Apple Universal Disk Image Format (UDIF), including obsolete (previous than DiskCopy 6) versions
* Apridisk disk image formats (for ACT Apricot disks)
* Basic Lisa Utility
* CDRDAO TOC sheets
* CDRWin cue/bin cuesheets, including ones with ISOBuster extensions
* CisCopy disk image (aka DC-File, .DCF)
* CloneCD
* DataPackRat's d2f/f2d disk image format ("WC DISK IMAGE")
* Digital Research DiskCopy
* DiscImageChef format
* IBM SaveDskF
* MAXI Disk disk images (HDK)
* Most known sector by sector copies of floppies with 128, 256, 319 and 1024 bytes/sector.
* Most known sector by sector copies with different bytes/sector on track 0.
* Parallels Hard Disk Image (HDD) version 2
* QEMU Copy-On-Write versions 1, 2 and 3 (QCOW and QCOW2)
* QEMU Enhanced Disk (QED)
* Ray Arachelian's Disk IMage (.DIM)
* RS-IDE hard disk images
* Sector by sector copies of Microsoft's DMF floppies
* T98 hard disk images (.THD)
* T98-Next hard disk images (.NHD)
* Virtual98 disk images
* VirtualBox Disk Image (VDI)
* Virtual PC fixed size, dynamic size and differencing (undo) disk images
* VMware VMDK and COWD images
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
* Xbox filesystems

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
* BeOS filesystem
* BSD Fast File System (FFS) / Unix File System (UFS)
* BSD Unix File System 2 (UFS2)
* B-tree file system (btrfs)
* CD-i file system
* Coherent UNIX file system
* Commodore 1540/1541/1571/1581 filesystems
* Cram file system
* DEC Files-11 (only checked with On Disk Structure 2, ODS-2)
* DEC RT-11 file system
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
* Microsoft/IBM High Performance File System (HPFS)
* Microsoft New Technology File System (NTFS)
* Microsoft Resilient File System (ReFS)
* Minix v2 file system
* Minix v3 file system
* NEC PC-Engine executable
* NEC PC-FX executable
* NILFS2
* Nintendo optical filesystems (GameCube and Wii)
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
* Universal Disk Format (UDF)
* UNIX System V file system
* UNIX Version 7 file system
* UnixWare boot file system
* Veritas file system
* VMware file system (VMFS)
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


## License
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2Fclaunia%2FDiscImageChef.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2Fclaunia%2FDiscImageChef?ref=badge_large)