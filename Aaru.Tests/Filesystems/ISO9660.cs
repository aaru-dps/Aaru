// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ISO9660.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef unit testing.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems.ISO9660;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class Iso9660
    {
        readonly string[] testfiles =
        {
            // Toast 3.5.7
            "toast_3.5.7_iso9660_apple.iso.lz", "toast_3.5.7_iso9660_dos_apple.iso.lz",
            "toast_3.5.7_iso9660_dos.iso.lz", "toast_3.5.7_iso9660_hfs.iso.lz", "toast_3.5.7_iso9660.iso.lz",
            "toast_3.5.7_iso9660_joliet_apple.iso.lz", "toast_3.5.7_iso9660_joliet.iso.lz",
            "toast_3.5.7_iso9660_mac_apple.iso.lz", "toast_3.5.7_iso9660_mac.iso.lz",
            "toast_3.5.7_iso9660_ver_apple.iso.lz", "toast_3.5.7_iso9660_ver_dos_apple.iso.lz",
            "toast_3.5.7_iso9660_ver_dos.iso.lz", "toast_3.5.7_iso9660_ver.iso.lz",
            "toast_3.5.7_iso9660_ver_joliet_apple.iso.lz", "toast_3.5.7_iso9660_ver_joliet.iso.lz",
            "toast_3.5.7_iso9660.iso.lz",
            // Toast 4.1.3
            "toast_4.1.3_iso9660_hfs.iso.lz",
            // Toast 4.0.3
            "toast_4.0.3_iso9660_apple.iso.lz", "toast_4.0.3_iso9660_dos_apple.iso.lz",
            "toast_4.0.3_iso9660_dos.iso.lz", "toast_4.0.3_iso9660_hfs.iso.lz", "toast_4.0.3_iso9660.iso.lz",
            "toast_4.0.3_iso9660_joliet_apple.iso.lz", "toast_4.0.3_iso9660_joliet.iso.lz",
            "toast_4.0.3_iso9660_mac_apple.iso.lz", "toast_4.0.3_iso9660_mac.iso.lz",
            // Toast 4.0.3 (CD-ROM XA)
            // "toast_4.0.3_iso9660_apple_xa.iso.lz","toast_4.0.3_iso9660_dos_apple_xa.iso.lz",
            // "toast_4.0.3_iso9660_dos_xa.iso.lz","toast_4.0.3_iso9660_joliet_apple_xa.iso.lz",
            // "toast_4.0.3_iso9660_joliet_xa.iso.lz","toast_4.0.3_iso9660_mac_apple_xa.iso.lz",
            // "toast_4.0.3_iso9660_mac_xa.iso.lz","toast_4.0.3_iso9660_xa.iso.lz",
            // "toast_4.0.3_iso9660_hfs_xa.iso.lz",
            // mkisofs
            "mkisofs_apple_rockrige.iso.lz", "mkisofs_apple_xa.iso.lz", "mkisofs_hybrid.iso.lz",
            "mkisofs_hybrid_nopart.iso.lz", "mkisofs_iso9660_level1_dirnest.iso.lz", "mkisofs_iso9660_level1.iso.lz",
            "mkisofs_iso9660_level2_dirnest.iso.lz", "mkisofs_iso9660_level2.iso.lz",
            "mkisofs_iso9660_level3_dirnest.iso.lz", "mkisofs_iso9660_level3.iso.lz",
            "mkisofs_iso9660_level4_dirnest.iso.lz", "mkisofs_iso9660_level4.iso.lz",
            "mkisofs_iso9660_udf_hybrid_dirnest.iso.lz", "mkisofs_iso9660_udf_hybrid.iso.lz", "mkisofs_joliet.iso.lz",
            "mkisofs_joliet_level1.iso.lz", "mkisofs_joliet_level2.iso.lz", "mkisofs_joliet_level3.iso.lz",
            "mkisofs_joliet_violating.iso.lz", "mkisofs_level1.iso.lz", "mkisofs_level2.iso.lz",
            "mkisofs_level3.iso.lz", "mkisofs_level4.iso.lz", "mkisofs_rockridge_dirnest.iso.lz",
            "mkisofs_rockridge.iso.lz", "mkisofs_rockridge_old.iso.lz", "mkisofs_rockridge_rational.iso.lz",
            "mkisofs_udf.iso.lz", "mkisofs_violating.iso.lz", "mkisofs_xa.iso.lz", "mkisofs_zisofs.iso.lz",
            "mkisofs_zisofs_rockridge.iso.lz",
            // Nero MAX
            "neromax_iso_mode1_apple.iso.lz", "neromax_iso_mode1_joliet.iso.lz", "neromax_iso_mode1_level1.iso.lz",
            "neromax_iso_mode1_level2.iso.lz", "neromax_iso_mode2_apple.iso.lz", "neromax_iso_mode2_joliet.iso.lz",
            "neromax_iso_mode2_level1.iso.lz", "neromax_iso_mode2_level2.iso.lz",
            // XorrISO
            "xorriso_hybrid.iso.lz", "xorriso_joliet.iso.lz", "xorriso_joliet_utf.iso.lz",
            "xorriso_joliet_violating.iso.lz", "xorriso_level1.iso.lz", "xorriso_level2.iso.lz",
            "xorriso_level3.iso.lz", "xorriso_level4.iso.lz", "xorriso_rockridge.iso.lz", "xorriso_violating.iso.lz",
            "xorriso_zisofs.iso.lz", "xorriso_zisofs_rockridge.iso.lz"
        };

        readonly MediaType[] mediatypes =
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
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            MediaType.CD,
            // XorrISO
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD,
            MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD, MediaType.CD
        };

        readonly ulong[] sectors =
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
            389, 417, 257, 266, 55, 56, 57, 58,
            // XorrISO
            59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70
        };

        readonly uint[] sectorsize =
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
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,
            // XorrISO
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048
        };

        readonly long[] clusters =
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
            389, 417, 257, 266, 55, 56, 57, 58,
            // XorrISO
            59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70
        };

        readonly int[] clustersize =
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
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048,
            // XorrISO
            2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048, 2048
        };

        readonly string[] volumename =
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
            "Root", "Root", "Root", "Root", "Root", "Root", "Root", "Root",
            // XorrISO
            "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "DISK_UTILS",
            "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "DISK_UTILS", "DISK_UTILS"
        };

        readonly string[] volumeserial =
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
            null, null, null, null, null, null, null, null,
            // XorrISO
            null, null, null, null, null, null, null, null, null, null, null, null
        };

        readonly string[] sysid =
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
            "", "", "", "", "", "", "", "",
            // XorrISO
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002",
            "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002", "APPLE COMPUTER, INC., TYPE: 0002"
        };

        readonly string[] appid =
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
            "", "", "", "", "", "", "", "",
            // XorrISO
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
            "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "iso9660", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true,          image.Open(filter),    $"{testfiles[i]}: Open()");
                Assert.AreEqual(mediatypes[i], image.Info.MediaType,  $"{testfiles[i]}: MediaType");
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    $"{testfiles[i]}: Sectors");
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, $"{testfiles[i]}: SectorSize");
                IFilesystem fs = new ISO9660();
                Partition wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), $"{testfiles[i]}: Identify()");
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     $"{testfiles[i]}: Clusters");
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  $"{testfiles[i]}: ClusterSize");
                Assert.AreEqual("ISO9660",       fs.XmlFsType.Type,         $"{testfiles[i]}: Type");
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   $"{testfiles[i]}: VolumeName");
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, $"{testfiles[i]}: VolumeSerial");
                Assert.AreEqual(sysid[i], fs.XmlFsType.SystemIdentifier,
                                $"{testfiles[i]}: SystemIdentifier");
                Assert.AreEqual(appid[i], fs.XmlFsType.ApplicationIdentifier,
                                $"{testfiles[i]}: ApplicationIdentifier");
            }
        }
    }
}