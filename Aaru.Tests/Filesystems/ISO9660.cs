// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ISO9660.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filesystems;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Iso9660
    {
        readonly string[] _testFiles =
        {
            // Toast 3.5.7
            "toast_3.5.7_iso9660_apple.aif", "toast_3.5.7_iso9660_dos_apple.aif", "toast_3.5.7_iso9660_dos.aif",
            "toast_3.5.7_iso9660_hfs.aif", "toast_3.5.7_iso9660.aif", "toast_3.5.7_iso9660_joliet_apple.aif",
            "toast_3.5.7_iso9660_joliet.aif", "toast_3.5.7_iso9660_mac_apple.aif", "toast_3.5.7_iso9660_mac.aif",
            "toast_3.5.7_iso9660_ver_apple.aif", "toast_3.5.7_iso9660_ver_dos_apple.aif",
            "toast_3.5.7_iso9660_ver_dos.aif", "toast_3.5.7_iso9660_ver.aif",
            "toast_3.5.7_iso9660_ver_joliet_apple.aif", "toast_3.5.7_iso9660_ver_joliet.aif", "toast_3.5.7_iso9660.aif",

            // Toast 4.1.3
            "toast_4.1.3_iso9660_hfs.aif",

            // Toast 4.0.3
            "toast_4.0.3_iso9660_apple.aif", "toast_4.0.3_iso9660_dos_apple.aif", "toast_4.0.3_iso9660_dos.aif",
            "toast_4.0.3_iso9660_hfs.aif", "toast_4.0.3_iso9660.aif", "toast_4.0.3_iso9660_joliet_apple.aif",
            "toast_4.0.3_iso9660_joliet.aif", "toast_4.0.3_iso9660_mac_apple.aif", "toast_4.0.3_iso9660_mac.aif",

            // Toast 4.0.3 (CD-ROM XA)
            // "toast_4.0.3_iso9660_apple_xa.iso.lz","toast_4.0.3_iso9660_dos_apple_xa.iso.lz",
            // "toast_4.0.3_iso9660_dos_xa.iso.lz","toast_4.0.3_iso9660_joliet_apple_xa.iso.lz",
            // "toast_4.0.3_iso9660_joliet_xa.iso.lz","toast_4.0.3_iso9660_mac_apple_xa.iso.lz",
            // "toast_4.0.3_iso9660_mac_xa.iso.lz","toast_4.0.3_iso9660_xa.iso.lz",
            // "toast_4.0.3_iso9660_hfs_xa.iso.lz",
            // mkisofs
            "mkisofs_apple_rockrige.aif", "mkisofs_apple_xa.aif", "mkisofs_hybrid.aif", "mkisofs_hybrid_nopart.aif",
            "mkisofs_iso9660_level1_dirnest.aif", "mkisofs_iso9660_level1.aif", "mkisofs_iso9660_level2_dirnest.aif",
            "mkisofs_iso9660_level2.aif", "mkisofs_iso9660_level3_dirnest.aif", "mkisofs_iso9660_level3.aif",
            "mkisofs_iso9660_level4_dirnest.aif", "mkisofs_iso9660_level4.aif",
            "mkisofs_iso9660_udf_hybrid_dirnest.aif", "mkisofs_iso9660_udf_hybrid.aif", "mkisofs_joliet.aif",
            "mkisofs_joliet_level1.aif", "mkisofs_joliet_level2.aif", "mkisofs_joliet_level3.aif",
            "mkisofs_joliet_violating.aif", "mkisofs_level1.aif", "mkisofs_level2.aif", "mkisofs_level3.aif",
            "mkisofs_level4.aif", "mkisofs_rockridge_dirnest.aif", "mkisofs_rockridge.aif", "mkisofs_rockridge_old.aif",
            "mkisofs_rockridge_rational.aif", "mkisofs_udf.aif", "mkisofs_violating.aif", "mkisofs_xa.aif",
            "mkisofs_zisofs.aif", "mkisofs_zisofs_rockridge.aif",

            // Nero MAX
            "neromax_iso_mode1_apple.aif", "neromax_iso_mode1_joliet.aif", "neromax_iso_mode1_level1.aif",
            "neromax_iso_mode1_level2.aif",

            // Nero MAX (CD-ROM XA)
            // "neromax_iso_mode2_apple.iso.lz", "neromax_iso_mode2_joliet.iso.lz",
            // "neromax_iso_mode2_level1.iso.lz", "neromax_iso_mode2_level2.iso.lz",

            // XorrISO
            "xorriso_hybrid.aif", "xorriso_joliet.aif", "xorriso_joliet_utf.aif", "xorriso_joliet_violating.aif",
            "xorriso_level1.aif", "xorriso_level2.aif", "xorriso_level3.aif", "xorriso_level4.aif",
            "xorriso_rockridge.aif", "xorriso_violating.aif", "xorriso_zisofs.aif", "xorriso_zisofs_rockridge.aif"
        };

        readonly MediaType[] _mediaTypes =
        {
            // Toast 3.5.7
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            MediaType.CD, MediaType.CD,

            // Toast 4.1.3
            MediaType.CD,

            // Toast 4.0.3
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            MediaType.CD, MediaType.CD,

            // Toast 4.0.3 (CD-ROM XA)
            // MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            // MediaType.CD, MediaType.CD,
            // mkisofs
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,

            // Nero MAX
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,

            // Nero MAX (CD-ROM XA)
            // MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,

            // XorrISO
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD
        };

        readonly ulong[] _sectors =
        {
            // Toast 3.5.7
            946, 946, 300, 1880, 300, 951, 300, 946, 300, 946, 946, 300, 300, 951, 300, 300,

            // Toast 4.1.3
            1882,

            // Toast 4.0.3
            305, 305, 300, 954, 300, 323, 300, 305, 300,

            // Toast 4.0.3 (CD-ROM XA)
            // 10, 11, 12, 13, 14, 15, 16, 17, 18,
            // mkisofs
            3662, 3606, 3800, 3800, 2983, 2531, 2983, 2531, 2983, 2531, 2894, 2894, 106589, 105241, 5055, 3651, 3651,
            3651, 3651, 3637, 3637, 3637, 3689, 7481, 7487, 3693, 7487, 3925, 3637, 3637, 3637, 3693,

            // Nero MAX
            389, 417, 257, 266,

            // Nero MAX (CD-ROM XA)
            // 55, 56, 57, 58,

            // XorrISO
            3688, 3686, 3686, 3686, 3673, 3673, 3673, 3686, 3675, 3673, 3673, 3675
        };

        readonly uint[] _sectorSize =
        {
            // Toast 3.5.7
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,

            // Toast 4.1.3
            2048,

            // Toast 4.0.3
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,

            // Toast 4.0.3 (CD-ROM XA)
            // 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,
            // mkisofs
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,

            // Nero MAX
            2048, 2048, 2048, 2048,

            // Nero MAX (CD-ROM XA)
            // 2048, 2048, 2048, 2048,

            // XorrISO
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048
        };

        readonly long[] _clusters =
        {
            // Toast 3.5.7
            946, 946, 244, 946, 244, 951, 249, 946, 244, 946, 946, 244, 244, 951, 249, 244,

            // Toast 4.1.3
            948,

            // Toast 4.0.3
            305, 305, 220, 954, 220, 323, 234, 305, 220,

            // Toast 4.0.3 (CD-ROM XA)
            // 10, 11, 12, 13, 14, 15, 16, 17, 18,
            // mkisofs
            3662, 3606, 3800, 3800, 2983, 2531, 2983, 2531, 2983, 2531, 2894, 2894, 106589, 105241, 5055, 3651, 3651,
            3651, 3651, 3637, 3637, 3637, 3689, 7481, 7487, 3693, 7487, 3925, 3637, 3637, 3637, 3693,

            // Nero MAX
            389, 417, 257, 266,

            // Nero MAX (CD-ROM XA)
            // 55, 56, 57, 58,

            // XorrISO
            3688, 3686, 3686, 3686, 3673, 3673, 3673, 3686, 3675, 3673, 3673, 3675
        };

        readonly int[] _clusterSize =
        {
            // Toast 3.5.7
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,

            // Toast 4.1.3
            2048,

            // Toast 4.0.3
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,

            // Toast 4.0.3 (CD-ROM XA)
            // 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,
            // mkisofs
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,

            // Nero MAX
            2048, 2048, 2048, 2048,

            // Nero MAX (CD-ROM XA)
            // 2048, 2048, 2048, 2048,

            // XorrISO
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048
        };

        readonly string[] _volumeName =
        {
            // Toast 3.5.7
            "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "Disk utils", "Disk utils",
            "Disk utils", "Disk utils", "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "Disk utils",
            "Disk utils", "DISK_UTILS",

            // Toast 4.1.3
            "DISK_UTILS",

            // Toast 4.0.3
            "UNTITLED_CD", "UNTITLED_CD", "UNTITLED_CD", "Untitled CD", "UNTITLED_CD", "Untitled CD", "Untitled CD",
            "Untitled CD", "Untitled CD",

            // Toast 4.0.3 (CD-ROM XA)
            // "UNTITLED_CD", "UNTITLED_CD", "UNTITLED_CD", "UNTITLED_CD", "UNTITLED_CD", "UNTITLED_CD", "UNTITLED_CD",
            // "UNTITLED_CD", "UNTITLED_CD",
            // mkisofs
            "test", "test", "test", "test", "CDROM", "CDROM", "CDROM", "CDROM", "CDROM", "CDROM", "CDROM", "CDROM",
            "CDROM", "CDROM", "CDROM", "test", "test", "test", "test", "test", "test", "test", "test", "CDROM", "CDROM",
            "test", "CDROM", "test", "test", "test", "test", "test",

            // Nero MAX
            "Root", "Root", "Root", "Root",

            // Nero MAX (CD-ROM XA)
            // "Root", "Root", "Root", "Root",

            // XorrISO
            "test", "test", "test", "test", "test", "test", "test", "test", "test", "test", "test", "test"
        };

        readonly string[] _volumeSerial =
        {
            // Toast 3.5.7
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,

            // Toast 4.1.3
            null,

            // Toast 4.0.3
            null, null, null, null, null, null, null, null, null, null,

            // Toast 4.0.3 (CD-ROM XA)
            // null, null, null, null, null, null, null, null,
            // mkisofs
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null,

            // Nero MAX
            null, null, null, null,

            // Nero MAX (CD-ROM XA)
            // null, null, null, null,

            // XorrISO
            null, null, null, null, null, null, null, null, null, null, null, null
        };

        readonly string[] _sysid =
        {
            // Toast 3.5.7
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002",

            // Toast 4.1.3
            "APPLE COMPUTER, INC., TYPE: 0002",

            // Toast 4.0.3
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",

            // Toast 4.0.3 (CD-ROM XA)
            // "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            // "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            // "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            // mkisofs
            "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX",
            "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX",
            "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX", "LINUX",

            // Nero MAX
            "", "", "", "",

            // Nero MAX (CD-ROM XA)
            // "", "", "", "",

            // XorrISO
            "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""
        };

        readonly string[] _appid =
        {
            // Toast 3.5.7
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",

            // Toast 4.1.3
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",

            // Toast 4.0.3
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",

            // Toast 4.0.3 (CD-ROM XA)
            // "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            // "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            // "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            // "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            // "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            // "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            // "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            // "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            // "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
            // mkisofs
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
            "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",

            // Nero MAX
            "", "", "", "",

            // Nero MAX (CD-ROM XA)
            // "", "", "", "",

            // XorrISO
            "", "", "", "", "", "", "", "", "", "", "", ""
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "ISO9660", _testFiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), $"{_testFiles[i]}: Open()");
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, $"{_testFiles[i]}: MediaType");
                Assert.AreEqual(_sectors[i], image.Info.Sectors, $"{_testFiles[i]}: Sectors");
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, $"{_testFiles[i]}: SectorSize");
                IFilesystem fs = new ISO9660();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), $"{_testFiles[i]}: Identify()");
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, $"{_testFiles[i]}: Clusters");
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, $"{_testFiles[i]}: ClusterSize");
                Assert.AreEqual("ISO9660", fs.XmlFsType.Type, $"{_testFiles[i]}: Type");
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, $"{_testFiles[i]}: VolumeName");
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, $"{_testFiles[i]}: VolumeSerial");
                Assert.AreEqual(_sysid[i], fs.XmlFsType.SystemIdentifier, $"{_testFiles[i]}: SystemIdentifier");

                Assert.AreEqual(_appid[i], fs.XmlFsType.ApplicationIdentifier,
                                $"{_testFiles[i]}: ApplicationIdentifier");
            }
        }
    }
}