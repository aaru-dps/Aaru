# RPM spec file for Aaru
#
# To build this package, you must first create the source tarball:
#   tar --exclude-vcs --exclude="*/bin" --exclude="*/obj" -czf \
#       ~/rpmbuild/SOURCES/aaru-6.0.0~alpha17.tar.gz \
#       --transform="s,^,aaru-6.0.0~alpha17/," .
#
# Or use the provided scripts which do this automatically:
#   ./build.sh                    (builds all package types)
#   pkg/rpm/build-rpm.sh          (builds only RPM)
#
# Then run: rpmbuild -bb aaru.spec
#

Name:           aaru
Version:        6.0.0~beta.2
Release:        1%{?dist}
Summary:        Disc image management and creation tool

License:        GPL-3.0-or-later AND LGPL-2.1-or-later AND MIT
URL:            https://www.aaru.app
Source0:        %{name}-%{version}.tar.gz

# Runtime dependencies for self-contained .NET app
Requires:       libicu
Requires:       krb5-libs
Requires:       libunwind
Requires:       openssl-libs
Requires:       zlib

# Desktop integration
Requires:       shared-mime-info
Requires:       desktop-file-utils

# Don't strip .NET binaries
%global __strip /bin/true
%global _build_id_links none
%global debug_package %{nil}

# Skip automatic dependency detection for bundled .NET libraries
%global __requires_exclude ^lib.*\.so.*$
%global __provides_exclude ^lib.*\.so.*$

%description
Aaru (named after the Egyptian paradise where the righteous dwell eternally)
is the ultimate Data Preservation Suite — your all-in-one solution for digital
media preservation and archival.

This package is built for Red Hat Enterprise Linux 9+, Fedora 38+, openSUSE
Leap 15.5+, and compatible distributions.

Aaru is designed to assist you through the entire workflow of digital media
preservation — from the initial creation of disk images (commonly called
"dumping") all the way through long-term archival storage.

Key features include:
* Media dumping from various drives (magnetic, optical, flash, tapes)
* Hardware flexibility with ATA, ATAPI, SCSI, USB, FireWire support
* Image management (identify, compare, convert formats)
* Filesystem analysis and extraction
* Archive and game package support
* Both CLI and GUI interfaces
* AaruFormat archival format with comprehensive metadata

%prep
%setup -q

%build
# Determine architecture and map to .NET Runtime Identifier
%ifarch x86_64
DOTNET_RID=linux-x64
%endif
%ifarch aarch64
DOTNET_RID=linux-arm64
%endif
%ifarch armv7hl
DOTNET_RID=linux-arm
%endif

# Build self-contained .NET application
cd Aaru
dotnet publish -f net10.0 -c Release \
    --self-contained -r ${DOTNET_RID} \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true

%install
# Determine architecture and map to .NET Runtime Identifier
%ifarch x86_64
DOTNET_RID=linux-x64
%endif
%ifarch aarch64
DOTNET_RID=linux-arm64
%endif
%ifarch armv7hl
DOTNET_RID=linux-arm
%endif

# Install main binary
install -D -m 0755 Aaru/bin/Release/net10.0/${DOTNET_RID}/publish/aaru \
    %{buildroot}/opt/Aaru/aaru

# Install documentation
install -D -m 0644 README.md %{buildroot}/opt/Aaru/README.md
install -D -m 0644 Changelog.md %{buildroot}/opt/Aaru/Changelog.md
install -D -m 0644 CONTRIBUTING.md %{buildroot}/opt/Aaru/CONTRIBUTING.md
install -D -m 0644 LICENSE %{buildroot}/opt/Aaru/LICENSE
install -D -m 0644 LICENSE.MIT %{buildroot}/opt/Aaru/LICENSE.MIT
install -D -m 0644 LICENSE.LGPL %{buildroot}/opt/Aaru/LICENSE.LGPL

# Install MIME type
install -D -m 0644 Aaru/aaruformat.xml \
    %{buildroot}%{_datadir}/mime/packages/aaruformat.xml

# Install desktop file
install -D -m 0644 Aaru/aaru.desktop \
    %{buildroot}%{_datadir}/applications/aaru.desktop

# Install icons
install -D -m 0644 icons/32x32/aaru.png \
    %{buildroot}%{_datadir}/icons/hicolor/32x32/apps/aaru.png
install -D -m 0644 icons/64x64/aaru.png \
    %{buildroot}%{_datadir}/icons/hicolor/64x64/apps/aaru.png
install -D -m 0644 icons/128x128/aaru.png \
    %{buildroot}%{_datadir}/icons/hicolor/128x128/apps/aaru.png
install -D -m 0644 icons/256x256/aaru.png \
    %{buildroot}%{_datadir}/icons/hicolor/256x256/apps/aaru.png
install -D -m 0644 icons/512x512/aaru.png \
    %{buildroot}%{_datadir}/icons/hicolor/512x512/apps/aaru.png

# Create symlink in /usr/bin
install -d %{buildroot}%{_bindir}
ln -sf /opt/Aaru/aaru %{buildroot}%{_bindir}/aaru

%post
# Update icon cache
touch --no-create %{_datadir}/icons/hicolor &>/dev/null || :

# Update MIME database
update-mime-database %{_datadir}/mime &>/dev/null || :

# Update desktop database
update-desktop-database &>/dev/null || :

%postun
# Update icon cache
if [ $1 -eq 0 ] ; then
    touch --no-create %{_datadir}/icons/hicolor &>/dev/null
    gtk-update-icon-cache %{_datadir}/icons/hicolor &>/dev/null || :
fi

# Update MIME database
update-mime-database %{_datadir}/mime &>/dev/null || :

# Update desktop database
update-desktop-database &>/dev/null || :

%posttrans
gtk-update-icon-cache %{_datadir}/icons/hicolor &>/dev/null || :

%files
/opt/Aaru/aaru
/opt/Aaru/README.md
/opt/Aaru/Changelog.md
/opt/Aaru/CONTRIBUTING.md
/opt/Aaru/LICENSE
/opt/Aaru/LICENSE.MIT
/opt/Aaru/LICENSE.LGPL
%{_bindir}/aaru
%{_datadir}/mime/packages/aaruformat.xml
%{_datadir}/applications/aaru.desktop
%{_datadir}/icons/hicolor/32x32/apps/aaru.png
%{_datadir}/icons/hicolor/64x64/apps/aaru.png
%{_datadir}/icons/hicolor/128x128/apps/aaru.png
%{_datadir}/icons/hicolor/256x256/apps/aaru.png
%{_datadir}/icons/hicolor/512x512/apps/aaru.png

%changelog
* Thu Oct 08 2026 Natalia Portillo <claunia@claunia.com> - 6.0.0~beta.2-1
- New upstream beta release 6.0.0-beta.2
- Added Data Position Measurement, measured by default as the last phase
  of CD, DVD and Blu-ray dumps (disable with --dpm false), including Xbox
  Game Discs on OmniDrive and unlocked Kreon drives
- Added dpm command to measure Data Position Measurement of media already
  dumped to an AaruFormat image
- Added Data Position Measurement support to AaruFormat and Alcohol 120%
  images, BlindWrite 4/5 BWA files, image conversion and image info
- Added ISO9660 support for Rock Ridge sparse files, AAIP attributes and
  ACLs, and the Amiga, Apple and Acorn extensions
- Added UDF reading on raw MRW media
- Added HFS+ BSD permissions and Finder information
- Added VideoNow, VideoNow Color and VideoNow XP disc detection
- Added D88 old style images and Alcohol 120% unreadable sector lists
- Added option to ignore sectors not found when merging images
- Added consent prompt before sending crash reports
- Changed to libaaruformat 1.0.0-beta.2, bounding block caches by memory
  so reading large images no longer exhausts memory, and adding the
  repair-cd-arena tool
- Changed to Aaru.Compression.Native and Aaru.Checksums.Native
  6.0.0-beta.2, with multithreaded LZMA and Zstandard
- Changed decompression errors to fail reads instead of returning
  corrupted data
- Changed dumps, scans, conversions, merges and sidecar creation to report
  the abort reason
- Changed drive speed settings to be clamped to the requested speed
- Changed total dump time to include trimming, retrying and DPM
- Changed image merging to allow different sector counts and to prefer
  the resume file next to the image
- Changed image formats to find data files next to their descriptor
  instead of in the current working directory
- Changed most filesystems to cache directories and paths, ISO9660
  directories with 100000 files no longer take hours
- Changed Spanish translation to Castilian Spanish
- Removed DPM media tag type
- Fixed crashes and hangs on crafted, truncated or corrupt archives,
  images, partitions and filesystems
- Fixed resumed CD dumps in AaruFormat overwriting sector prefix and
  suffix data from earlier sessions
- Fixed ATA, MMC, SCSI, SD/MMC, Linux, Windows and remote device commands
- Fixed CD dumping ISRC, BCD subchannel detection, C2 flags and resuming
- Fixed VDI, WCDiskImage and EWF v2 images failing to open, and VHD/VHDX
  differencing images not finding their parent
- Fixed ISO9660 Rock Ridge on Joliet discs, symbolic links, timestamps
  and zisofs files
- Fixed HFS and HFS+ file types and creators being byte-reversed
- Fixed partition bounds and size/length swaps in many schemes
- Fixed GUI file extraction, empty saved device files, startup hangs and
  leaked handles
- Fixed TUI keyboard navigation and dialogs
- Fixed opening lzip compressed files
- Built for RHEL 9+, Fedora 38+, openSUSE Leap 15.5+
- Multi-architecture support (x86_64, aarch64, armv7hl)

* Wed Jul 15 2026 Natalia Portillo <claunia@claunia.com> - 6.0.0~beta.1-1
- New upstream beta release 6.0.0-beta.1
- Added full OmniDrive support for dumping CD, DVD, Blu-ray, XGD1/2/3,
  GameCube, Wii and PlayStation media, including HyperSpeed and raw
  Blu-ray reading
- Added C2 secure audio reading with concealed sector convergence repair
  and --c2-repair option
- Added HD DVD AACS and Blu-ray decryption support
- Added transparent decryption pipelines for PS3, Wii/GameCube and Wii U
  conversion, with IRD parsing, junk detection and media tag injection
- Added image analyze command with support for more than 50 filesystems
- Added write-metadata command for AaruFormat images
- Added erasure coding support to AaruFormat
- Added readable filesystems: GDFX/XDVDFS, PlayStation File System
- Added Sony APA partitioning scheme
- Added archive formats: ACE, AR, ARJ, ARJZ, Compact Pro, CPIO,
  DiskDoubler, EWF, LHA/LARC/PMARC, RAR, StuffIt, StuffIt 5, StuffIt X,
  TAR, ZIP
- Added Apple compression algorithms: KenCode, LZH, StuffIt (ShrinkWrap)
- Added compression streams for ZIP, StuffIt, StuffIt X and DiskDoubler
- Added disk image formats: CrunchDisk, Disk eXPress, EWF, MagicISO UIF,
  Nintendo Wii U WUX, QRST, Redumper DVD/BD, SNATCH-IT, Sydex CopyQM+
  SXD, The Duplicator, partclone v0002
- Added preliminary KryoFlux support and flux image merging
- Added BinHex 4.0 and Zstandard filters
- Added remaining time estimation, reverse error retry, SafeDisc sector
  skip and optional lead-out dumping options
- Added volume selection for file listing and extraction
- Added long sectors option to image checksum command
- Changed CD ECC code, reworked to correct more errors
- Changed Linux device access to prefer SCSI generic device if available
- Changed DiskCopy 4.2 Twiggy sector ordering with spare bad block track
  support
- Changed NDIF, UDIF and DART to support LZH, KenCode and StuffIt
  compressed images
- Changed NDIF to verify CRC
- Changed Partclone and Partimage to verify images with CRC32 checksums
- Changed SaveDskF to support LZMW decompression
- Removed DotNetZip dependency
- Fixed loading CDRWin BIN/CUE images outside current working directory
- Fixed HFS by completely rewriting Catalog and Extents trees
- Fixed HFS+ extended attributes and Private Data directory listing
- Fixed ISO9660 multiple extent reading and malformed directory records
- Fixed UDF 2.50 reading and continuation extents
- Fixed ADFS superblock layout and big directory parsing
- Fixed volume parameter in extract and list files commands
- Fixed GUI device list on Linux, filesystem info panel scrolling and
  repeated subdirectory filling
- Fixed NTFS guard against too big non-resident data sizes
- Fixed LZip crash
- Built for RHEL 9+, Fedora 38+, openSUSE Leap 15.5+
- Multi-architecture support (x86_64, aarch64, armv7hl)

* Wed Mar 11 2026 Natalia Portillo <claunia@claunia.com> - 6.0.0~alpha.19-1
- New upstream alpha release 6.0.0-alpha.19
- Added command to generate JSON schema for Aaru metadata files
- Added LZO compression algorithms
- Added LZVN compression algorithm with native version
- Added XZ buffer compression and decompression
- Added support for reading DVD lead-in and lead-out
- Added support for OmniDrive DVD reading
- Added PS5 DI/BCA dumping
- Added many fully readable filesystems: ADFS, Acer, AO-DOS, AmigaDOS,
  AtheOS, btrfs, BeFS, Coherent, Cram, DEC Files-11, DEC RT-11, ECMA-67,
  exFAT, ext/ext2/ext3/ext4, F2FS, HFS, HFS+, HPOFS, HP LIF, HPFS, JFS,
  Locus, MicroDOS, Minix, NTFS, NILFS2, Nintendo optical, OS-9 RBF, PFS,
  ProDOS, QNX4, QNX6, Reiser v3/v4, SGI EFS, SmartFileSystem, Squash,
  System V, UDF, UNIX 7th Edition, UNIX boot (bfs), UFS, Veritas, Xia,
  XENIX, XFS
- Added CHD compressed compact disc and CHD v5 image support
- Added Easy CD Creator (.CIF) disc image support
- Added PowerISO / gBurner disc image support
- Added UltraISO disc image support
- Added WinOnCD disc image support
- Added RISC OS system area support for ISO9660
- Added full zisofs support for ISO9660
- Added zlib and lzfse compressed files support for HFS+
- Added support for reading all extended attributes in HFS+
- Added Wii U Disc Key and PS3 related media tags
- Added AIX minidisk partition traversing and enumeration
- Added big endian and PDP endianness swapping source generators
- Changed device reports to try other commands if one fails
- Changed DVD raw reading to dynamically figure out buffer size
- Changed dumping to only re-read pregap sectors on track type changes
- Changed FFS filesystem name to UFS with improved detection
- Changed GUI to show image path on all tabs and negative sectors in viewer
- Enhanced Pascal filesystem with marshalable structures and consistency checks
- Refactored to use source generator based big endian marshaller
- Changed sidecar to disable default SpamSum and use pathless filenames
- Fixed CD-Text processing from images in image info command
- Fixed Spectre.Console exception propagation
- Fixed divide by zero error
- Fixed markup escaping in CD-Text logging
- Fixed sidecar creation for files with brackets or braces
- Fixed listing files with braces
- Fixed Blu-ray disc info logging crash with braces or brackets
- Fixed missing markup end tag
- Fixed handling of invalid ISO9660 dates
- Fixed parallel track path in PFI
- Fixed volume name sanitization replacing '/' and '\' with '_'
- Fixed barcode casting in GUI
- Fixed wrongly using marked up path in image metadata editor
- Built for RHEL 9+, Fedora 38+, openSUSE Leap 15.5+
- Multi-architecture support (x86_64, aarch64, armv7hl)

