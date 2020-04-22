# [5.0.1.2884] - 2020-04-23
- Fixes data loss on certain Compact Disc audio tracks when dumping in Aaru Format with compression enabled.

# [5.0.0.2879] - 2020-03-15
- First and most importantly, we got a rename. We're now Aaru, part of the Aaru Data Preservation Suite, that encompasses Aaru (previously DiscImageChef), 
Aaru.Server (previously DiscImageChef.Server), aaruformat (previously dicformat) and aaruremote.
- This release is dedicated to the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24.

## Added
### - Aaru Image Format
- Add ".aif" as a supported extension.
- Add CD Mode 2 sector reconstruction.
- Claunia Subchannel Transform algorithm. Makes subchannel compress 100% faster and 25% better.
- Compress VideoNow discs as data not audio.
- Support for CD-i Ready.
- Support for skipping storing CD prefixes and suffixes that are correct.
- Support writing logically block addressable tapes.
- Update template with block addressable tape types.

### - CDRWin cuesheet disc image
- Save metadata in CDRWin cuesheet.

### - Database
- Add entities for USB vendor and product IDs.
- Add entry for optimal count of sectors for multiple read in devices.
- Added new database system
- Enhance support and tweaks for devices depending on the parameters in the database
- Fill CompactDisc read offsets from AccurateRip list.
- Store and retrieve USB IDs from databases.
- Store device reports in the database instead of XML files
- Store statistics on database
- Submit pending statistics in background.

### - Device report
- Add check for Nintendo discs.
- Add new CompactDisc and Blu-ray variants.
- Add test for inter-session reading in multi-session discs.
- Change device report entry for Lead-in to first track pre-gap and add a new entry for proper Lead-in
- Check if data CDs can be read scrambled by READ CD command.
- Check sector 16 for MMC discs, as 0 is usually empty.
- Clear ATA IDENTIFY DEVICE private fields.
- Clear serial numbers.
- Create new more extensible device report format in JSON.
- Do not allow to be run without administrative privileges.
- Eject SCSI DirectAccess devices if removable.
- Save data when not in debug mode.
- Store not only default, but current and changeable modes in SCSI.
- Store read results in report and database.
- Support iomega REV disks.

### - Dumping
- Add default value for writable image options.
- Add option to not store paths and serial numbers when dumping.
- Add support for CD-i Ready.
- Add support for dumping MemoryStick from USB attached PlayStation Portable with CFW installed.
- Add support for dumping UMD from USB attached PlayStation Portable with CFW installed.
- Allow to abort anywhere
- Change --no-metadata to --metadata and --no-trim to --trim.
- Prevent dumping XGD without administrative privileges.
- Show error message if unsupported dump is tried.
- Show more information when dumping an XGD.
- Support fixing Compact Disc audio tracks offset using scrambled read commands and database.
- Support iomega REV disks.
- Support PD650 discs.

### - Filesystems
- Full read-only implementation of Xbox and Xbox 360 FAT filesystems

### - FAT filesystem
- Full read-only implementation.
- Support for Microsoft FASTFAT long file names.
- Support for OS/2 Extended Attributes.
- Support for OS/2 WorkPlace Shell long file names.
- Support for PCExchange filenames.
- Support for PCExchange Resource Fork.
- Support for Sharp X68000 extended filenames.

### - ISO9660 filesystem
- Full read-only implementation for CD-i filesystem.
- Full read-only implementation for High Sierra Format.
- Full read-only implementation for ISO 9660 (up to level 4).
- Support for AAIP.
- Support for Amiga RRIP.
- Support for Apple Extensions.
- Support for eXtended Architecture (XA).
- Support for Joliet extensions.
- Support for Rock Ridge Interchange Protocol.
- Support for Romeo variant.

### - Media image formats
- Add support for DataPackRat's f2d/d2f disk images.
- Full read/write support for CopyTape tape images.

### - Media information
- Add another value for Mitsubishi Chemical ATIP frame number. (97:34:22)
- Detect 3DO discs.
- Detect Atari Jaguar CD discs.
- Detect audio MiniDisc.
- Detect Bandai Playdia discs.
- Detect Castlewood Orb 2.2Gb.
- Detect CD-i Ready.
- Detect EZFlyer 230MB.
- Detect Fujitsu FM-Towns discs.
- Detect Hasbro VideoNow Color detection.
- Detect Hi-MD formatted 60 minutes MiniDisc.
- Detect Hi-MD formatted 74 minutes MiniDisc.
- Detect iomega REV, REV70 and REV120.
- Detect media types also in ATA.
- Detect NEC PC-Engine discs.
- Detect NEC PC-FX discs.
- Detect Sega CD / Mega CD.
- Detect Sega Dreamcast GD-ROM.
- Detect Sega MilCD discs.
- Detect Sega Saturn CD.
- Detect Sony PlayStation 2 discs (CD and DVD).
- Detect Sony PlayStation 3 discs (DVD and Blu-ray).
- Detect Sony PlayStation 4 Blu-ray discs.
- Detect SparQ carts in SCSI devices.
- Detect SparQ media in ATA drive.
- Detect SyQuest SQ2000 and SQ800.
- Detect SyQuest SQ310.
- Detect TR-4 and TR-5.

### - Media types
- Add Amiga CD32
- Add Amiga CDTV.
- Add another DDS1 SCSI medium type.
- Add Bandai Pippin.
- Add Bandai Playdia
- Add CD-i Ready.
- Add dimensions for Iomega REV.
- Add Fujitsu FM-Towns.
- Add Hasbro VideoNow.
- Add HP codes for DDS.
- Add Nuon
- Add PD650.
- Add SEAGATE code for DDS-2.
- Add Sega MilCD.

### - Metadata sidecar
- List and hash filesystem contents when creating a sidecar.

### - Opera filesystem
- Full read-only implementation.

### - SCSI response decoders
- Add encoder for ATA IDENTIFY (PACKET) DEVICE.
- Add encoder for SCSI INQUIRY.
- Add encoder for SCSI MODE PAGE 2Ah.

## Fixed
### - Aaru Image Format
- Don't initialize LZMA when compression is disabled.
- Ensure all LZMA allocations are freed when closed.
- Fix double negation options
- Fix reading MODE2 sectors with incorrect EDC/ECC correctly.

### - Alcohol 120% disc image
- Fix media size calculation when reading Alcohol images with several pregaps.

### - Apple Hierarchical File System
- Fix interpretation of the Apple boot block.

### - CDRWin cuesheet disc image
- Fix pregap reading in CDRWin format.
- Fix pregap writing in CDRWin format.

### - Checksum
- Optimize SpamSum

### - CPCEMU Disk-File and Extended CPC Disk-File disk image
- Fix images not recognized as such.

### - CP/M File System
- Fix the CPM filesystem detection and file listing

### - Device report
- Allow ASC 28h in streaming device report.
- Eject media once reported.
- Fix SCSI Streaming Command device reporting.
- On streaming device report do not LOAD as the tape is already in loaded state once inserted in the drive, and some old drives get confused.
- Retry 50 times as tapes can take long to be ready.

### - Devices
- Allow opening read-only devices on Linux.
- Allows opening some devices in non-administrator mode.
- Close device when finished command execution.
- Correct detection of errors sending ATA commands.
- Correct detection of Plextor features.
- Correct showing EVPD page number.
- Do not search for floppy mode page when mode sense returned no pages.
- Get serial number using MMC GET CONFIGURATION for optical drives.
- In Windows, close the device handle, to prevent an exception being raised.

### - Dreamcast GDI disc image
- Fix reading pregap in GDI images.

### - Dumping
- Check which LOCATE version is supported regardless of the next block on resume.
- Correct device not ready error messages on dumping SCSI.
- Fix Compact Disc type detection
- Fix detecting tape block size when tape reports a lower minimum size.
- Fix printing of sense in SSC dump.
- Fix speed calculation on.
- Handle errors when dumping SSC.
- Handle when SSC drive does not report block size for first block.
- Prevent showing option to dump first pregap on FreeBSD where it crashes the system.
- Save tape files when dumping SSC media.
- Save tape partitions when dumping SSC media.
- Set image's tape mode when dumping SSC.
- Show message indicating that audio MiniDisc cannot be dumped.
- Store MODE responses from SSC dumping in output image.
- Support resume in SSC dumping.
- Try to detect if the Kreon drive has not locked correctly, and try to use cold values if they look as possibly valid.
- Use output plugin when dumpìng SSC.
- When SCSI device is becoming ready, wait more, as tapes can take a long time to become ready.

### - Filesystems
- Fix extracting file from filesystems with subdirectories.
- Fix listing files walking thru subdirectories.
- Stylize output when listing files.

### - IBM Journaled File System
- Fix decoding of volume label.

### - Image analysis
- Fix crash in partitions enumeration.
- Treat tape files as partitions.

### - Image comparison
- Do not compare metadata between two images.

### - Image conversion
- Checking if input tracks is null when converting image.
- Fix showing sector where conversion fails.

### - Image filters
- Fix bzip2 initialization.

### - Image verification
- Fix verify command when image can represent optical media, but doesn't.

### - ISO9660 filesystem
- Do not set ISO identifiers in XML metadata if they are empty.
- Fix reading application identifier from ISO9660.
- Fix trimming of null character and spaces in Joliet volume descriptor.

### - Macintosh File System
- Correct behaviour when path starts with directory separator.
- Fix interpretation of the Apple boot block.

### - Media information
- Fix media type detection from SBC devices.
- Fix support for 128Mb 3.5" magneto optical.

### - Metadata sidecar
- Calculate tape hashes in smaller chunks as tapes can have huge blocks.
- Fix error creating sidecar with DVD's CMI.
- Fix media type sidecar on DVD based console discs.
- Fix setting application identifier for metadata sidecar.
- Fix when USB descriptors are null at sidecar creation after dump.

### - Nero Burning ROM disc image
- Fix offset by 1 that prevented reading the last sector of every track.

### - RAW (sector by sector) disk image
- Do not allow CDs with more than one tracks, or non-mode1 tracks to be written as raw images (.iso).
- Fix dumping CDs in raw image format (.iso).

### - SCSI response decoders
- Protect against null mode pages.

### - SecureDigital devices
- Fix overflow on SecureDigital CSD v2.0 size calculation.

### - VirtualPC disk image
- Conversion optimizations make opening images up to 38 times faster.

## Changes
- Add binary packages for major targets.
- Add mime database file for Linux systems to correctly recognize aaruformat images.
- Add support to use devices remotely with Aaruremote.
- Change command line to a cleaner and more natural system.
- Complete CompactDisc dumping rewrite, allowing fixing audio tracks offset, more correct audio track dumping, workarounds firmware bugs from several common drives and gives more preservation-quality dumps.
- Deprecate Mono and .NET Framework.
- Fix null reference exception on verify.
- Fix overflow with small sectors in Apple Partition Map.
- Fix overflow with small sectors in BSD disklabel.
- Fix progress crashing when terminal window changes size.
- Get device information from database when dumping Compact Disc.
- Hide device commands on unsupported platforms.
- Remove RIPEMD160.
- Separate CRC16 IBM and CRC16 CCITT contexts, use cached tables.
- Use .NET Core.

# [4.5.1.1692] - 2018-07-19
## Fixed
### - Alcohol 120% disc image
- Correct writing images of Compact Disc >= 60 min
- Correct writing MODE2 tracks to image
- Correct writing TOC to image
- Generation of multisession images
- Generation of pregaps changing tracks

# [4.5.0.1663] - 2018-06-24
## Added
### - Alcohol 120% disc image
- 010editor template.
- Support for creating images.

### - Apple New Disk Image Format
- Support RLE compressed images.

### - Blindwrite 4 disc image
- 010editor template.
- Information about why this format cannot support writing.

### - Blindwrite 5 disc image
- 010editor template.
- Information about why this format cannot support writing.

### - DART disk image
- Support RLE compressed images.

### - Decoders
- Added Blu-ray DI decoders.
- Support decoding 2048 bytes PFI.

### - Devices
- On Linux try to open in read/write and exclusive mode, if not retry in readonly.
- On Linux use direct SG_IO.
- Workaround some Blu-ray drives not reporting correct size on READ DISC STRUCTURE.

### - DiscJuggler disc image
- Information about why this format cannot support writing.

### - Dumping
- Added support for CD drives that don't return a TOC.
- Added support for CD drives that don't support READ CD command.
- Added support for Compact Disc that don't report tracks.
- Add support for dumping media in any of the now supported writable formats.
- Dump ISRC.
- Dump MCN.
- Fix reading PW subchannels.
- Separate trimming from error retry.
- When dumping CDs in persistent mode, try disabling L-EC check if drive doesn't support TB bit, or doesn't return data with TB bit enabled.
- When dumping, print bad sectors to dump log.

### - FAT filesystem
- Add list of known boot sector hashes.
- Support Human68k FAT16 BPB.

### - Filesystems
- Detecting High Performance Optical File System (HPOFS).
- Detecting Microsoft Resilient filesystem (ReFS).
- Detecting PC-FX executable tracks.
- Detecting Xia filesystem.

### - Apple 2IMG disk image
- Support for creating images.

### - Anex86 disk image
- Support for creating images.

### - Apple II interleaved disk image
- Support for creating images.

### - Apple Universal Disk Image Format
- Support for creating images.
- Support RLE compressed images.

### - Apridisk disk image
- Support for creating images.

### - Basic Lisa Utility disk image
- Support for creating images.

### - cdrdao disc image
- Support for creating images.

### - CDRWin cuesheet disc image
- Support for creating images.

### - CisCopy disk image
- Support for creating images.

### - CloneCD disc image
- Support for creating images.

### - Digital Research DISKCOPY disk image
- Support for creating images.

### - DiskCopy 4.2 disk image
- Support for creating images.

### - IBM SaveDskF disk image
- Support for creating images.

### - MaxiDisk disk image
- Support for creating images.

### - NHDr0 disk image
- Support for creating images.

### - Parallels disk image
- Support for creating images.

### - QEMU Copy-On-Write disk image
- Support for creating images.

### - QEMU Copy-On-Write v2 disk image
- Support for creating images.

### - QEMU Enhanced Disk image
- Support for creating images.

### - RAW (sector by sector) disk image
- Added geometry and size for ZIP100 and ZIP250.
- Support 2448 bytes/sector and 2352 bytes/sector CD images.
- Support media tags.

### - Ray Arachelian's disk image
- Support for creating images.

### - RS-IDE disk image
- Support for creating images.

### - T98 Hard Disk Image
- Support for creating images.

### - Virtual98 disk image
- Support for creating images.

### - VirtualBox disk image
- Added image type and flags.
- Support for creating images.

### - VirtualPC disk image
- Support for creating images.

### - VMware disk image
- Support for creating images.

## Fixes
### - Apple DOS filesystem
- Use Apple II character set encoding.

### - Apple ProDOS filesystem
- Use Apple IIc character set encoding.

### - BlindWrite 4 disc image
- Fix incorrect pregap calculation preventing images from showing correct data.

### - CICM metadata
- Can now get dump hardware information from images.

### - cdrdao disc image
- Fix audio track endian.
- Fix when disc catalog number uses whole ASCII and not only numeric digits.

### - CDRWin disc image
- Fix when disc catalog number uses whole ASCII and not only numeric digits.

### - CloneCD disc image
- Fix when disc catalog number uses whole ASCII and not only numeric digits.

### - Checksums
- Correct CD ECC.
- Correct CD EDC.
- Fix CRC16 returning a 32-bit value.
- Fix CRC64 endian.
- Fix Fletcher-16.
- Fix Fletcher-32.

### - Create sidecar
- Add filesystems only to the appropriate partition and track.
- Fix CD first track pregap, TOC and XGD tags.
- Fix diameter setting.
- Fix SCSI MODE SENSE.
- Fix USB descriptors.

### - DART disk image
- Fixed endian.

### - Devices
- Fix sending READ LONG commands to ATA devices.
- Fixed crashing with some rogue SCSI MMC firmwares.

### - Dumping
- Correctly detect CD-i, CD+ and CD-ROM XA.
- Correctly detect Mode 2 Form 1 and Form 2.
- Do not retry when retry passes are zero.
- Do not try to read multisession lead-out/lead-in as they result in errors that are not really there.
- Get correct track flags.
- Retry only the number of times requested.
- Return drive to previous error correction status.
- Send error recovery MODE before retrying sectors.

### - HDCopy disk image
- Fix sector calculation.

### - Image comparison
- Fix when sessions are null.

### - Image verification
- Corrected status printing.

### - ISO9660 filesystem
- Do not try to read past partition if El Torito indicates image goes beyond limits.
- Fix when root directory is outside of device.
- Skip null terminated strings in ISO9660 fields.

### - Lisa filesystem
- Corrected character set encoding.

### - Macintosh filesystem
- Corrected character set encoding.

### - PC-98 Partition Table
- Prevent some FAT BPBs to false positive as PC-98 partition tables.

### - RT-11 filesystem
- Use Radix-50 character set encoding.

### - System V filesystem
- Fix partition bounds.

### - VirtualPC disk image
- Corrected reading non-allocated blocks.

## Changes
- Added command to convert disc images.
- Added command to get information about an image and its contents.
- Added D/CAS-25, D/CAS-85 and D/CAS-103 formats.
- Added IRC notifications for Travis CI.
- Added measured dimensions from an UMD.
- Added media types for NEO GEO CD, PC-FX.
- Added new image format designed to store as much information about media as a drive returns: dicformat.
- Added numeric values to media types.
- Added project to create test filesystems on 16-bit OS/2.
- Added project to create test filesystems on 32-bit OS/2.
- Added project to create test filesystems on DOS.
- Added project to create test filesystems on Mac OS.
- Added size of 640MiB magneto-optical disk.
- Added support for writing disc images.
- Compliant with GDPR.
- Corrected floptical geometry to data according to IRIX.
- Do not assume pointers are 32-bit in several Windows device calls.
- Fixed when statistics settings are null.
- Minimum .NET Framework version is now 4.6.1.
- Sort verbs list.
- Support newest XDG Base Directory Specification for Linux.

# [4.0.1.0] - 2018-01-06
## Fixes
### Apple DOS and ProDOS interleaved disk images
- Fixed interleaving values.

### Apple Nibble disk image
- Fixed detection of DOS vs ProDOS sector order.

### Apple 2IMG disk image
- Fixed deinterleaving of DOS and ProDOS sector order.
- Fixed denibblizing images.

### Apple ProDOS filesystem
- Fixed detection on Apple II disks.

### UCSD Pascal filesystem
- Added support for Apple II variants (two physical sectors per logical sector and little endian fields).

# [4.0.0.0] - 2017-12-25
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

[5.0.1.2884]: https://github.com/aaru-dps/Aaru/releases/tag/v5.0.1.2884
[5.0.0.2879]: https://github.com/aaru-dps/Aaru/releases/tag/v5.0.0.2879
[4.5.1.1692]: https://github.com/aaru-dps/Aaru/releases/tag/v4.5.1.1692
[4.5.0.1663]: https://github.com/aaru-dps/Aaru/releases/tag/v4.5.0.1663
[4.0.1.0]: https://github.com/aaru-dps/Aaru/releases/tag/v4.0.1.0
[4.0.0.0]: https://github.com/aaru-dps/Aaru/releases/tag/v4.0.0.0
[3.0.0.0]: https://github.com/aaru-dps/Aaru/releases/tag/v3.0.0.0
[2.20]: https://github.com/aaru-dps/Aaru/releases/tag/v2.2
[2.10]: https://github.com/aaru-dps/Aaru/releases/tag/v2.1
[2.0]: https://github.com/aaru-dps/Aaru/releases/tag/v2.0
