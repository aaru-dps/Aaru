DiscImageChef v2.00
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

* Supports reading CDRWin cue/bin cuesheets, Apple DiskCopy 4.2 and TeleDisk disk images.
* Supports reading all raw (sector by sector copy) disk images with a multiple of 512 bytes/sector, and a few known formats that are 256, 128 and variable bytes per sector.
* Supports traversing MBR, Apple and NeXT partitioning schemes.
* Identifies HFS, HFS+, MFS, BeFS, ext/2/3/4, FAT12/16/32, FFS/UFS/UFS2, HPFS, ISO9660, LisaFS, MinixFS, NTFS, ODS11, Opera, PCEngine, SolarFS, System V and UnixWare boot filesystem.
* Analyzes a disk image getting information about the disk itself and analyzes partitions and filesystems inside them
* Can compare two disk images, even different formats, for different sectors and/or metadata
* Can verify sectors or disk images if supported by the underlying format (well, it will be able to in version 2.1)
* Can checksum the disks (and if optical disc, separate tracks) user-data (tags and metadata coming soon)
* Supports CRC32 and CRC64 cyclic redundance checksums as well as MD5, RMD160, SHA1, SHA256, SHA384 and SHA512 hashes.

Changelog
=========

See Changelog file.

To-Do
=====

See TODO file.