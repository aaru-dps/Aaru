# [4.0.1.0] - 2017-12-25
## Added
### - Advanced Disc Filing System
- Added support for ADFS-S, ADFS-M, ADFS-L, ADFS-D, ADFS-E, ADFS-E+, ADFS-F, ADFS-F+ and ADFS-G.

### - Apple Partition Map
- Added support for decoding Driver Description Map.
- Added support for maps without Driver Description Map.
- Added support for old partition table.

### - Commands
- Added separate application to debug commands sent to devices.
- list-devices: Lists devices that can be used for device dependent commands.
- list-encodings: Lists supported character encodings.

### - Create Sidecar command
- Added support for hashing DiscFerret flux images.
- Added support for hashing KryoFlux STREAM flux images.
- Added support for hashing SuperCardPro flux images.
- Added support for tape dumps where each tape-file is a separate dumped file.
- Calculate checksum of contents not only of image file.
- Consider each optical disc track as a separate partition.
- Store superblock modification time on sidecar.
- Support tracks.
- Use dump drive information from images that support it.

### - Decoders
- Xbox DMI.
- Xbox Security Sectors.

### - Devices
- MMC.
- PCMCIA block devices.
- SCSI Streaming Devices (aka "tapes").
- SecureDigital.

### - Device commands
- Add ATA and SCSI commands support for FreeBSD.
- Add ATA commands support for Windows.
- Add retrieval of USB information on Windows.
- Add SecureDigital/MMC commands support for Windows.

### - Disc images.
- Alcohol 120%.
- Anex86.
- Apple DOS interleaved (.do).
- Apple New Disk Image Format (aka NDIF, aka img, aka DiskCopy 6).
- Apple Nibble (aka NIB).
- Apple ProDOS interleaved (.po).
- Apple Universal Disk Image Format (aka UDIF, aka dmg).
- BlindWrite 4.
- BlindWrite 5.
- CisCopy (aka DC-File or DCF).
- CloneCD.
- CopyQM.
- CPCEMU Disk File.
- CPCEMU Extended Disk File.
- D64.
- D71.
- D81.
- Digital Research's DiskCopy.
- DiscJuggler.
- HD-Copy.
- IBM SaveDskF.
- IMD.
- MAME Compressed Hunks of Data (aka CHD).
- Parallels Hard Disk Image (aka HDD).
- Partclone disk images
- Partimage disk images
- QEMU Copy-On-Write (aka QCOW).
- QEMU Copy-On-Write v2.
- QEMU Enhanced Disk (aka QED).
- Quasi88 (.D77/.D88).
- Ray Arachelian's Disk IMage (.DIM).
- RS-IDE hard disk images.
- Spectrum floppy disk image (.FDI)
- T98.
- VHDX.
- Virtual98.
- VMware.
- X68k .DIM.

### - DiskCopy 4.2 disk image
- Added support for invalid images that use little-endian values.
- Added support for images created by macOS that don't have a format byte set.
- Use resource fork to get DiskCopy version used to create them.

### - Dumping
- Added the ability to resume a partially done dump, even on a separate drive.
- Added the ability to skip dumping the Lead-in.
- Allow creation of a separate subchannel file.
- Create dump log.
- Dumping optical media creates an Alcohol 120% descriptor file.
- Raw dump of DVD with Matshita recorders.
- XGD with Kreon drives.

### - ext2/3/4 filesystem
- Added new superblock fields.
- Added support for devices with sectors bigger than 512 bytes.

### - FAT filesystem
- Added DEC Rainbow's hard-wired BPB.
- Added support for volumes with 256 bytes/sector.
- Added support for ACT Apricot BPB.
- Gets volume label, creation time and modification time from root directory if available.

### - Filesystems
- Apple DOS.
- CP/M.
- Detecting AO-DOS.
- Detecting AtheOS.
- Detecting CD-i.
- Detecting Commodore 1540/1541/1571/1581.
- Detecting cram.
- Detecting Cray UNICOS.
- Detecting dump(8) (Old historic BSD, AIX, UFS and UFS2 types).
- Detecting ECMA-67.
- Detecting exFAT.
- Detecting Extent File System (aka SGI EFS).
- Detecting F2FS.
- Detecting FAT+.
- Detecting fossil.
- Detecting HAMMER.
- Detecting High Sierra Format.
- Detecting HP Logical Interchange Format.
- Detecting IBM JFS.
- Detecting Locus.
- Detecting MicroDOS file system.
- Detecting NILFS2.
- Detecting OS-9 Random Block File (aka RBF).
- Detecting Professional File System (aka PFS).
- Detecting QNX 4.
- Detecting QNX 6.
- Detecting Reiser.
- Detecting Reiser4.
- Detecting RT-11.
- Detecting SmartFileSystem (aka SFS, aka Standard File System).
- Detecting Squash.
- Detecting Universal Disk Format (aka UDF).
- Detecting Veritas.
- Detecting VMware.
- Detecting Xbox.
- Detecting XFS.
- Detecting Zettabyte File System (aka ZFS).
- UCSD Pascal.

### - Filters
- AppleDouble.
- Apple PCExchange.
- AppleSingle.
- BZIP2.
- GZIP.
- LZIP.
- MacBinary.
- XZ.

### - GUID Partition Table
- New type GUIDs.

### - ISO9660 filesystem
- Added detection of AAIP extensions.
- Added detection of Apple extensions.
- Added detection of EFI Platform ID for El Torito.
- Added detection of RRIP extensions.
- Added detection of SUSP extensions.
- Added detection of XA extensions.
- Added detection of ziso extensions.

### - Lisa filesystem
- Full read-only support. 

### - Media types
- DDS, DDS-2, DDS-3, DDS-4.
- HiFD.
- IOMEGA Clik! (aka PocketZip).
- IOMEGA JAZ.
- LS-120, LS-240, FD32MB.
- NEC floppies.
- Old DEC hard disks
- SHARP floppies.
- XGD3.

### - Partitions
- Acorn FileCore.
- ACT Apricot.
- BSD disklabels.
- DEC disklabels.
- DragonFly BSD.
- Human68k.
- MINIX subpartitions.
- NEC PC-9800.
- Plan9 partition table.
- Rio Karma.
- SGI Disk Volume Headers.
- UNIX hardwired partition tables.
- UNIX VTOC.
- XENIX partition table.

### - SCSI decoding
- Handling of EVPDs smaller than length field.
- Handling of modes 02h, 04h and 1Ch smaller than expected.
- Prettyfying of mode 0Bh.

### - SmartFileSystem
- Added support for version 2.

### - Statistics
- Added version and operating system statistics.

### - Sun disklabel
- Added bound checks.
- Added support for 16-entries VTOC.
- Added support for pre-VTOC disklabels.
- Corrected structures for 8-entries VTOC.

### - System V filesystem
- Added COHERENT offsets.
- Check for it starting on second cylinder.
- Corrected cluster size calculation.
- Corrected detection between Release 2 and Release 4.
- Corrected Release 2 superblock parameters.
- Enlarged NICFREE for Version 7.

### TeleDisk images
- Added support for Advanced Compression.
- Added support for floppy lead-out.
- Added variable sectors per track support.

## Fixes
### - AmigaDOS filesystem
- Corrected checksum calculation.
- Corrected cluster size calculation.
- Corrected root block location.
- Corrected support for AROS i386 variant that has a PC bootblock before the AmigaDOS bootblock itself.
- Detection on hard disks or with clusters bigger than 1 sector.
- Tested FFS2.

### - Apple Partition Map
- Added bound checks.
- Added support for decoding Driver Description Map.
- Added support for maps without Driver Description Map.
- Added support for old partition table.
- Corrected partition start when map it's not on start of device.
- Corrected support for misaligned maps, like on CDs.
- Cut partitions that span outside the device.

### - cdrdao
- Audio track matching.
- Corrected images that start with comments.
- Prevent reading binary files.

### - CDRWin
- CD-Text detection.
- CD+G data return.
- Fixed composer parsing.
- Prevent reading binary files.

### - CP/M filesystem
- Corrected cluster count calculation.
- Corrected directory location on CP/M-86.
- Corrected sector reading.
- Skip media types that were never used as a CP/M disk.

### - Create Sidecar command
- Corrected creation when path is absolute.

### - Device commands
- Do not send SCSI INQUIRY to non-SCSI paths on Linux.

### - Device reports
- Call ATA READ LONG last, as it confuses some drives.
- Try SCSI READ LONG (10) until max block size (65535).

### - DiskCopy 4.2
- Corrected track order for Lisa and Macintosh Twiggy.

### - Dreamcast GDI images
- Prevent reading binary files.

### - Dumping
- Calculation of streaming device dumping speed.
- Corrected dumping CD-R and CD-RW.
- Optical media with 2048 bytes/sector now get ".iso" file extension.
- Retry when SCSI devices return reset status.
- Streaming Devices now store block size changes in metadata sidecar.
- Wait for SCSI devices to exit ASC 28h (MEDIUM CHANGE) status.

### - ext2/3/4 filesystem
- Use os type as XML system identifier.

### - FAT filesystem
- Behaviour with some non-compliant media descriptors.
- Corrected 5.25" MD1DD detection.
- Corrected boot code detection.
- Corrected misaligned volumes on optical media.
- Rewritten to better detect Atari, MSX, *-DOS and ANDOS variants.
- Use OEM name as XML system identifier.

### - Guid Partition Table
- Added bound checks.
- Corrected misaligned tables on optical media.
- Corrected when table is smaller than one sector.

### - HFS filesystem
- Corrected detection of a PowerPC only bootable volume (no boot sector).
- Corrected misaligned volumes on optical media.
- Corrected volume serial number case.

### - HFS+ filesystem
- Corrected misaligned volumes on optical media.
- Corrected misalignment of fields in Volume Header.
- Use last mount version as XML system identifier.

### - HPFS filesystem
- Corrected cluster size.
- Detect boot code.
- Show NT flags.
- Use OEM name as XML system identifier.

### - ISO9660 filesystem
- Complete rewrite.
- Check that date fields start with a number.

### - Master Boot Record partitioning scheme
- Check real presence of a GPT.
- Corrected infinite looping on extended partitions.
- Remove disklabels support.
- Support misaligned MBRs on optical media.
- Support NEC extensions.
- Support OnTrack extensions.

### - MINIX filesystem
- Added support for v1 and v2 created on MINIX 3.
- Corrected misaligned volumes on optical media.

### - Nero Burning ROM
- Corrected track handling.
- Corrected typo on parsing v2 images.
- Disc types.
- Do not identify positively if footer version is unknown.
- Lead-In handling.
- Mode2 RAW sectors.
- Session count.

### - NeXT partition table
- Added missing fields.
- Corrected offsets.
- Cut partitions that span outside the device.

### - ODS filesystem
- Corrected cluster size calculation.
- Corrected misaligned volumes on optical media.

### - ProDOS filesystem
- Corrected cluster size calculation.
- Corrected misaligned volumes on optical media.
- Volume size.

### - Rigid Disk Block partition scheme
- Corrected AMIX mappings.

### - SCSI decoding
- Handling of EVPDs smaller than length field.
- Handling of modes 02h, 04h and 1Ch smaller than expected.
- Prevented overflow on MMC FEATURES decoding.
- Prevented overflow on SCSI MODE PAGE decoding.

### - SmartFileSystem
- Added support for version 2.

### - Sun disklabel
- Added bound checks.
- Corrected structures for 8-entries VTOC.

### - System V filesystem
- Check for it starting on second cylinder.
- Corrected cluster size calculation.
- Corrected detection between Release 2 and Release 4.
- Corrected Release 2 superblock parameters.
- Enlarged NICFREE for Version 7.

### - UFS filesystem
- Corrected superblock locations.
- Move superblock to a single structure and marshal it, corrects detection of several variants.

## Changes
- Added a public changelog.
- Added a side application to create device reports under Linux without a .NET environment.
- Added operating system version statistics.
- Added partitioning scheme name to partition structures.
- Added several internal tests to prevent regression on changes.
- Added support for different character encodings.
- Added support for filters.
- Added support for nested partitioning schemes.
- Added support for propagating disk geometry, needed by PC-98 partitions and old MBRs.
- Better support for decoding multibyte encodings from C, Pascal and space-padded strings.
- Changed handling of compressed files, using temporary files and caching.
- Corrected casting on big-endian marshalling that was failing on some .NET Framework versions.
- Corrected filter list reuse.
- Disabled EDC check on CDs because it is not working (TODO).
- Filesystems now have access to full partition structure.
- Filters no longer return their own extension when requested for filename.
- Moved Claunia.RsrcFork to NuGet.
- Priam tags.
- Support drive firmware inside disc images.
- Support subchannel with only Q channel.

# [3.0.0.0] - 2016-07-19
## Added
### - Commands
- benchmark: Tests speed for checksum algorithms.
- create-sidecar: Creates an XML sidecar with metadata.
- decode: Decodes and prints a disk tag present on the image.
- device-info: Prints device information.
- dump-media: Dumps media to a disk image.
- entropy: Calculates disk entropy.
- media-info: Prints media information.
- scan-media: Scans media for errors.

### - Checksums
- Adler-32
- SpamSum

### - Devices
- ATA on Linux.
- ATA on Windows (untested).
- FireWire on Linux.
- SCSI on Linux.
- SCSI on Windows (untested).
- USB on Linux.

### - Disc images
- Apple 2IMG.
- CDRDAO.
- Dreamcast GDI.
- VirtualPC.

### - Fast File System (FFS)
- Atari UNIX variant.

### - Filesystems
- Acorn ADFS.
- AmigaDOS.
- Apple File System, aka APFS.
- Apple ProDOS.
- btrfs.
- Nintendo Gamecube.
- Nintendo Wii.

### - Partitions
- Amiga Rigid Disk Block (aka RDB).
- Atari.
- Sun.
- (U)EFI GPT.

## Changes
### - PrintHex command
- Allow to print several sectors.

## Fixes
### - Be filesystem
- Endianness.
- Support for Be CDs.

### - CDRWin disk image
- Behaviour on .NET Framework.
- Detection of CD-ROM XA.
- Flags.
- Partition calculations.

### - Fast File System (FFS)
- False positives with 7th Edition.

### - ISO9660
- Dreamcast IP.BIN decoding.
- Sega CD IP.BIN decoding.

### - System V Filesystem
- Big endian support

# [2.20] - 2014-08-28
## Added
### - Checksums
- Reed Solomon.

## Fixes
### - Apple Partition Map
- Disks with 2048 bytes/sector but a 512 bytes/sector map.

### - HFS
- Disks with 2048 bytes/sector but a 512 bytes/sector filesystem.

# [2.10] - 2014-08-25
## Added
### - Checksums
- CD EDC and ECC.
- CRC16.

### - Commands
- Verify: Verifies disk image contents, if supported.

### - Disc images
- Nero Burning ROM.

# [2.0] - 2014-07-03
## Added
### - Commands
- analyze: Gives informatio about disk image contents as well as detecting partitions and filesystems.
- checksum: Generates CRC32, CRC64, RIPEMD160, MD5, SHA1, SHA256, SHA384 and SHA512 checksums of disk image contents.
- compare: Compares two media images.
- printhex: Prints a hexadecimal output of a sector.

### - Disc images
- RAW (sector by sector).

### - Media types
- BD-R.
- BD-RE XL.
- FDFORMAT.

## Fixes
### - FAT filesystem
- Workaround FAT12 without BIOS Parameter Block.

### - MBR partitions
- Do not search for them on disks with less than 512 bytes/sector.

### - ODS-11 filesystem
- Do not search for them on disks with less than 512 bytes/sector.

# [1.10] - 2014-04-21
## Added
### - Disc images
- Sydex TeleDisk.

# [1.0] - 2014-04-17
## Added
### - Filesystems
- Detecting BeFS.
- Detecting ext.
- Detecting ext2.
- Detecting ext3.
- Detecting ext4.
- Detecting FAT12.
- Detecting FAT16.
- Detecting FAT32.
- Detecting FFS.
- Detecting HFS+.
- Detecting HFS.
- Detecting HPFS.
- Detecting ISO9660.
- Detecting LisaFS.
- Detecting MFS.
- Detecting MinixFS.
- Detecting NTFS.
- Detecting ODS-11.
- Detecting Opera.
- Detecting PCEngine.
- Detecting SolarFS.
- Detecting System V.
- Detecting UFS.
- Detecting UFS2.
- Detecting UnixWare boot.

### - Disc images
- Apple DiskCopy 4.2.
- CDRWin.

### - Partitions
- Apple Partition Map (aka APM).
- Master Boot Record (aka MBR).
- NeXT disklabels.