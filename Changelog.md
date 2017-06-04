# [4.0.0.0-beta] - 2017-06-04
## Added
### - Create Sidecar command
- Use dump drive information from images that support it.

### - Decoders
- Xbox DMI

### - Devices
- MMC
- PCMCIA block devices
- SCSI Streaming Devices (aka "tapes")
- SecureDigital

### - Disc images.
- Alcohol 120%.
- Apple New Disk Image Format (aka NDIF, aka img, aka DiskCopy 6).
- Apple Nibble (aka NIB).
- Apple Universal Disk Image Format (aka UDIF, aka dmg).
- BlindWrite 4.
- BlindWrite 5.
- CloneCD.
- CopyQM.
- CPCEMU Disk File.
- CPCEMU Extended Disk File.
- D64.
- D71.
- D81.
- DiscJuggler.
- MAME Compressed Hunks of Data (aka CHD).
- Parallels Hard Disk Image (aka HDD).
- QEMU Copy-On-Write (aka QCOW).
- QEMU Copy-On-Write v2.
- QEMU Enhanced Disk (aka QED).
- VHDX.
- VMware.
- X68k .DIM.

### - DiskCopy 4.2 disk image
- Use resource fork to get DiskCopy version used to create them.

### - Dumping
- Raw dump of DVD with Matshita recorders
- XGD with Kreon drives

### - Filesystems
- Apple DOS.
- CP/M.
- Detecting Commodore 1540/1541/1571/1581.
- Detecting cram.
- Detecting ECMA-67.
- Detecting exFAT.
- Detecting F2FS.
- Detecting FAT+.
- Detecting IBM JFS.
- Detecting NILFS2.
- Detecting Professional File System (aka PFS).
- Detecting QNX 4.
- Detecting QNX 6.
- Detecting Reiser.
- Detecting Reiser4.
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
- Apple PCExchange
- AppleDouble
- AppleSingle
- BZIP2
- GZIP
- MacBinary

### - GUID Partition Table
- New type GUIDs.

### - Lisa filesystem
- Full read-only support 

### - Media types
- DDS, DDS-2, DDS-3, DDS-4.
- IOMEGA Clik! (aka PocketZip)
- NEC floppies.
- SHARP floppies.
- XGD3

### - Partitions
- Acorn FileCore.
- BSD disklabels.
- DEC disklabels.
- DragonFly BSD.
- Human68k.
- NEC PC-9800.
- Rio Karma.
- SGI Disk Volume Headers.
- UNIX disklabels.

### - Statistics
- Added version and operating system statistics.

## Fixes
### - AmigaDOS filesystem
- Detection on hard disks or with clusters bigger than 1 sector.

### - cdrdao
- Audio track matching
- Prevent reading binary files.

### - CDRWin
- CD-Text detection
- CD+G data return.
- Prevent reading binary files.

### - Device reports
- Call ATA READ LONG last, as it confuses some drives
- Try SCSI READ LONG (10) until max block size (65535)

### - DiskCopy 4.2
- Track order for Lisa and Macintosh Twiggy

### - Dreamcast GDI images
- Prevent reading binary files.

### - Dumping
- Streaming Devices now store block size changes in metadata sidecar.
- Calculation of streaming device dumping speed.
- Optical media with 2048 bytes/sector now get ".iso" file extension.

### - FAT filesystem
- Behaviour with some non-compliant media descriptors.

### - Nero Burning ROM
- Disc types
- Lead-In handling
- Mode2 RAW sectors
- Session count

### - ProDOS filesystem
- Volume size.

### - SCSI decoding
- Handling of modes 02h and 04h smaller than expected.
- Handling of EVPDs smaller than length field.

## Changes
- Added a public changelog.
- Added support for filters.
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