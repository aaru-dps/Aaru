Aaru Data Preservation Suite v6.0.0-beta.2

Aaru

Copyright © 2011-2026 Natalia Portillo <claunia@claunia.com>

You can see user documentation [here](https://www.aaru.app)

What is Aaru?
=============
**Aaru** (named after the Egyptian paradise where the righteous dwell eternally) is the ultimate **Data Preservation
Suite** — your all-in-one solution for digital media preservation and archival.

Aaru is designed to assist you through the entire workflow of digital media preservation — from the initial creation of
disk images (commonly called "dumping") all the way through long-term archival storage.

### 🔧 Key Features

#### Media Dumping

Aaru can dump media from virtually any drive you have:

- **Magnetic disks** (floppy disks, hard drives)
- **Optical discs** (CDs, DVDs, Blu-rays)
- **Magneto-optical disks** (MO discs)
- **Flash devices** (USB drives, SSDs)
- **Memory cards** (SD, CF, and more)
- **Tapes** (various formats)

It works with drives connected via **ATA, ATAPI, SCSI, USB, FireWire, and SDHCI** interfaces, producing archival-grade
images in multiple supported formats.

#### Hardware Flexibility

You don't need specialized hardware. Aaru will always try to extract the most accurate, archival-quality image from your
media using whatever drive you have available. All parameters are fully customizable, with sensible and battle-tested
defaults.

#### Image Management

- **Identify** existing disk images and get detailed information
- **Compare** two dumps to verify integrity or find differences
- **Convert** between supported formats without any data loss

#### Filesystem Analysis & Extraction

Aaru recognizes dozens of filesystems and can display detailed information about them. For many filesystems, you can
extract the complete contents — including all files, extended attributes, and alternate data streams.

#### Archive & Game Package Support

Beyond disk images, Aaru supports compressed archives and game packages, enabling you to view and extract their contents
as part of your preservation workflow.

#### Dual Interface

Not comfortable with command lines? Aaru includes a modern, fully-featured **graphical user interface** that makes
preservation accessible to everyone.

#### AaruFormat — The Archival Standard

We provide our own archival-oriented format, **AaruFormat**, specifically designed for preservation. It stores:

- All data from your media
- Comprehensive metadata
- Full audit information

#### Metadata Sidecar Generation

Aaru can generate metadata sidecars in an open **JSON format**, perfect for integration with external systems and
third-party software. These sidecars contain all extractable information from any supported image format.

System requirements
===================
Aaru is created using .NET 10 and can be compiled with all the major IDEs. To run it you require to use one of the
stable releases, or build it yourself.

Usage
=====

aaru.exe

And read help.

Or read the [documentation](https://www.aaru.app).

Media image formats we can read
===============================

* A2R Disk Image 2.x and 3.x
* Apple Disk Archival/Retrieval Tool (DART)
* Apple II nibble images (NIB)
* BlindWrite 4 TOC files (.BWT/.BWI/.BWS)
* BlindWrite 5/6 TOC files (.B5T/.B5I and .B6T/.B6I)
* CopyQM
* CPCEMU Disk file and Extended Disk File
* Crunchdisk
* Dave Dunfield IMD
* DiscJuggler images
* Disk eXPress (DXP)
* Dreamcast GDI
* Easy CD Creator images (.CIF)
* Expert Witness Format (EWF)
* HD-Copy disk images
* HxCStream
* MagicISO (.UIF)
* MAME Compressed Hunks of Data (CHD)
* Microsoft VHDX
* Nero Burning ROM (both image formats)
* Partclone disk images
* Partimage disk images
* PowerISO (.DAA/.GBI)
* Quasi88 disk images (.D77/.D88)
* Quick Release Sector Transfer (QRST)
* Software Pirates SNATCH-IT / Central Point COPY II-PC (.CP2)
* Spectrum floppy disk image (.FDI)
* SuperCardPro
* Sydex CopyQM+ Self-eXtracting Disk (.SXD)
* TeleDisk
* The Duplicator
* UltraISO (.ISZ)
* WinOnCD (.C2D)
* WUX
* X68k DIM disk image files (.DIM)

Media image formats we can write to
===================================

* A2R Disk Image 3.x
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
* CopyTape
* DataPackRat's d2f/f2d disk image format ("WC DISK IMAGE")
* Digital Research DiskCopy
* DiskDupe (DDI)
* Aaru Format
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

Supported archives
==================

* ACE (.ACE)
* AR (.a)
* ARC (.ARC)
* ARJ (.ARJ)
* ARJZ (.ARZ)
* Compact Pro (.CPT)
* CPIO (.cpio)
* DiskDoubler (.DD)
* Expert Witness Format (EWF)
* HA (.HA)
* LARC (.LZS)
* LHA (.LZH)
* PAK (.PAK)
* PMARC (.PMA)
* RAR (.RAR)
* StuffIt (.SIT)
* StuffIt X (.SITX)
* Symbian Installation File (.SIS)
* TAR (.TAR)
* Xbox 360 Secure Transacted File System (STFS)
* ZIP (.ZIP)
* ZOO (.ZOO)

Supported partitioning schemes
==============================

* Acorn Linux and RISCiX partitions
* ACT Apricot partitions
* Amiga Rigid Disk Block (RDB)
* APA (Sony PlayStation 2 partitions)
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

Fully supported file-systems (identify and extraction)
======================================================

* 3DO Opera file system
* Acer File System (AFS)
* Acorn Advanced Disc Filing System
* Alexander Osipov DOS (AO-DOS for Electronika BK-0011) file system
* Amiga Fast File System, with international characters, directory cache and multi-user patches
* Amiga Original File System, with international characters, directory cache and multi-user patches
* Apple DOS file system
* Apple Lisa file system
* Apple Hierarchical File System (HFS)
* Apple Hierarchical File System+ (HFS+)
* Apple Macintosh File System (MFS)
* Apple ProDOS / SOS file system
* AtheOS file system
* B-tree file system (btrfs)
* BeOS filesystem
* BeOS (old) filesystem
* BSD Fast File System (FFS) / Unix File System (UFS)
* BSD Unix File System 2 (UFS2)
* CD-i file system
* Commodore 1540/1541/1571/1581 filesystems
* CP/M file system
* Coherent UNIX file system
* Cram file system
* DEC Files-11 (aka ODS)
* DEC RT-11 file system
* ECMA-67: 130mm Flexible Disk Cartridge Labelling and File Structure for Information Interchange
* Extended Acer File System (EAFS)
* Flash-Friendly File System (F2FS)
* GDFX aka Xbox DVD File System (XDVDFS)
* High Sierra Format
* High Performance Optical File System (HPOFS)
* HP Logical Interchange Format
* IBM Journaling File System (JFS)
* ISO9660, including Apple, Amiga, Rock Ridge, Joliet and Romeo extensions
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
* Minix v1/v2/v3 file systems
* NILFS2
* Nintendo optical filesystems (GameCube and Wii)
* OS-9 Random Block File
* PlayStation File System
* Professional File System
* QNX4 filesystems
* QNX6 filesystems
* Reiser v3 file systems
* Reiser v4 file system
* SGI Extent File System (EFS)
* SGI XFS
* SmartFileSystem
* Squash file system
* U.C.S.D Pascal file system
* Universal Disk Format (UDF)
* UNIX boot file system
* UNIX System V file system
* UNIX Version 7 file system
* Veritas file system
* Xbox filesystems
* Xenix file system
* Xia filesystem

Supported file systems for identification and information only
==============================================================

* Amiga Fast File System v2, untested
* Apple File System (preliminary detection until on-disk layout is stable)
* dump(8) (Old historic BSD, AIX, UFS and UFS2 types)
* Fossil file system (from Plan9)
* HAMMER file system
* Microsoft Resilient File System (ReFS)
* NEC PC-Engine executable
* NEC PC-FX executable
* SolarOS file system
* UNICOS file system
* VMware file system (VMFS)
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
* SHA-1
* SHA-2 (256, 384 and 512 bits)
* SpamSum (fuzzy hashing)

Supported filters
=================

* Apple PCExchange (FINDER.DAT & RESOURCE.FRK)
* AppleDouble
* AppleSingle
* BinHex 4.0
* BZip2
* GZip
* LZip
* MacBinary I, II, III
* XZ
* ZStandard

Partially supported disk image formats
======================================
These disk image formats cannot be read, but their contents can be checksummed on sidecar creation

* DiscFerret
* KryoFlux STREAM

License
=======
Aaru is licensed under the GNU General Public License v2 license. Some components may be licensed under different
licenses; see their respective documentation for details.