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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Iso9660 : ReadOnlyFilesystemTest
    {
        public Iso9660() : base("ISO9660") {}

        public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "ISO9660");
        public override IFilesystem Plugin     => new ISO9660();
        public override bool        Partitions => false;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 946,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 946,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_dos_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 946,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 946,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_dos.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 244,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_hfs.aif",
                MediaType     = MediaType.CD,
                Sectors       = 1880,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 946,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 244,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_joliet_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 951,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 951,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Disk utils"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_joliet.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 249,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Disk utils"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_mac_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 946,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 946,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Disk utils"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_mac.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 244,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Disk utils"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_ver_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 946,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 946,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_ver_dos_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 946,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 946,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_ver_dos.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 244,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_ver.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 244,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_ver_joliet_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 951,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 951,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Disk utils"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660_ver_joliet.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 249,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Disk utils"
            },
            new FileSystemTest
            {
                TestFile      = "toast_3.5.7_iso9660.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 244,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_4.1.3_iso9660_hfs.aif",
                MediaType     = MediaType.CD,
                Sectors       = 1882,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 948,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "DISK_UTILS"
            },
            new FileSystemTest
            {
                TestFile      = "toast_4.0.3_iso9660_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 305,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 305,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "UNTITLED_CD"
            },
            new FileSystemTest
            {
                TestFile      = "toast_4.0.3_iso9660_dos_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 305,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 305,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "UNTITLED_CD"
            },
            new FileSystemTest
            {
                TestFile      = "toast_4.0.3_iso9660_dos.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 220,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "UNTITLED_CD"
            },
            new FileSystemTest
            {
                TestFile      = "toast_4.0.3_iso9660_hfs.aif",
                MediaType     = MediaType.CD,
                Sectors       = 954,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 954,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Untitled CD"
            },
            new FileSystemTest
            {
                TestFile      = "toast_4.0.3_iso9660.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 220,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "UNTITLED_CD"
            },
            new FileSystemTest
            {
                TestFile      = "toast_4.0.3_iso9660_joliet_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 323,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 323,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Untitled CD"
            },
            new FileSystemTest
            {
                TestFile      = "toast_4.0.3_iso9660_joliet.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 234,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Untitled CD"
            },
            new FileSystemTest
            {
                TestFile      = "toast_4.0.3_iso9660_mac_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 305,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 305,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Untitled CD"
            },
            new FileSystemTest
            {
                TestFile      = "toast_4.0.3_iso9660_mac.aif",
                MediaType     = MediaType.CD,
                Sectors       = 300,
                SectorSize    = 2048,
                ApplicationId = "TOAST ISO 9660 BUILDER COPYRIGHT (C) 1997 ADAPTEC, INC. - HAVE A NICE DAY",
                Clusters      = 220,
                ClusterSize   = 2048,
                SystemId      = "APPLE COMPUTER, INC., TYPE: 0002",
                VolumeName    = "Untitled CD"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_apple_rockrige.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3662,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3662,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_apple_xa.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3606,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3606,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_hybrid.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3800,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3800,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_hybrid_nopart.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3800,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3800,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_iso9660_level1_dirnest.aif",
                MediaType  = MediaType.CD,
                Sectors    = 2983,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 2983,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_iso9660_level1.aif",
                MediaType  = MediaType.CD,
                Sectors    = 2531,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 2531,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_iso9660_level2_dirnest.aif",
                MediaType  = MediaType.CD,
                Sectors    = 2983,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 2983,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_iso9660_level2.aif",
                MediaType  = MediaType.CD,
                Sectors    = 2531,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 2531,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_iso9660_level3_dirnest.aif",
                MediaType  = MediaType.CD,
                Sectors    = 2983,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 2983,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_iso9660_level3.aif",
                MediaType  = MediaType.CD,
                Sectors    = 2531,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 2531,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_iso9660_level4_dirnest.aif",
                MediaType  = MediaType.CD,
                Sectors    = 2894,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 2894,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_iso9660_level4.aif",
                MediaType  = MediaType.CD,
                Sectors    = 2894,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 2894,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_iso9660_udf_hybrid_dirnest.aif",
                MediaType  = MediaType.CD,
                Sectors    = 106589,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 106589,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_iso9660_udf_hybrid.aif",
                MediaType  = MediaType.CD,
                Sectors    = 105241,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 105241,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_joliet.aif",
                MediaType  = MediaType.CD,
                Sectors    = 5055,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 5055,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_joliet_level1.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3651,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3651,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_joliet_level2.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3651,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3651,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_joliet_level3.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3651,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3651,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_joliet_violating.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3651,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3651,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_level1.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3637,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3637,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_level2.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3637,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3637,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_level3.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3637,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3637,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_level4.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3689,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3689,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_rockridge_dirnest.aif",
                MediaType  = MediaType.CD,
                Sectors    = 7481,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 7481,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_rockridge.aif",
                MediaType  = MediaType.CD,
                Sectors    = 7487,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 7487,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_rockridge_old.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3693,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3693,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_rockridge_rational.aif",
                MediaType  = MediaType.CD,
                Sectors    = 7487,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 7487,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "CDROM"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_udf.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3925,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3925,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_violating.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3637,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3637,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_xa.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3637,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3637,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_zisofs.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3637,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3637,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile   = "mkisofs_zisofs_rockridge.aif",
                MediaType  = MediaType.CD,
                Sectors    = 3693,
                SectorSize = 2048,
                ApplicationId =
                    "MKISOFS ISO9660/HFS/UDF FILESYSTEM BUILDER & CDRECORD CD/DVD/BluRay CREATOR (C) 1993 E.YOUNGDALE (C) 1997 J.PEARSON/J.SCHILLING",
                Clusters    = 3693,
                ClusterSize = 2048,
                SystemId    = "LINUX",
                VolumeName  = "test"
            },
            new FileSystemTest
            {
                TestFile      = "neromax_iso_mode1_apple.aif",
                MediaType     = MediaType.CD,
                Sectors       = 389,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 389,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "Root"
            },
            new FileSystemTest
            {
                TestFile      = "neromax_iso_mode1_joliet.aif",
                MediaType     = MediaType.CD,
                Sectors       = 417,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 417,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "Root"
            },
            new FileSystemTest
            {
                TestFile      = "neromax_iso_mode1_level1.aif",
                MediaType     = MediaType.CD,
                Sectors       = 257,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 257,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "Root"
            },
            new FileSystemTest
            {
                TestFile      = "neromax_iso_mode1_level2.aif",
                MediaType     = MediaType.CD,
                Sectors       = 266,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 266,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "Root"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_hybrid.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3688,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3688,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_joliet.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3686,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3686,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_joliet_utf.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3686,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3686,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_joliet_violating.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3686,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3686,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_level1.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3673,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3673,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_level2.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3673,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3673,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_level3.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3673,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3673,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_level4.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3686,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3686,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_rockridge.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3675,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3675,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_violating.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3673,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3673,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_zisofs.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3673,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3673,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            },
            new FileSystemTest
            {
                TestFile      = "xorriso_zisofs_rockridge.aif",
                MediaType     = MediaType.CD,
                Sectors       = 3675,
                SectorSize    = 2048,
                ApplicationId = "",
                Clusters      = 3675,
                ClusterSize   = 2048,
                SystemId      = "",
                VolumeName    = "test"
            }
        };
    }
}