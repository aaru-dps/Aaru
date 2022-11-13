// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT12.cs
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FAT12
{
    [TestFixture]
    public class Whole : ReadOnlyFilesystemTest
    {
        public Whole() : base("FAT12") {}

        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT12");

        public override IFilesystem Plugin     => new FAT();
        public override bool        Partitions => false;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile    = "concurrentdos_6.00_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "concurrentdos_6.00_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "concurrentdos_6.00_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.40_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.40_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.40_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.40_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.40_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                VolumeName  = "VOLUMELABEL",
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.40_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.40_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.41_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.41_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.41_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.41_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.41_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                VolumeName  = "VOLUMELABEL",
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.41_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.41_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512,
                SystemId    = "DIGITAL "
            },
            new FileSystemTest
            {
                TestFile    = "drdos_5.00_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_5.00_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_5.00_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_5.00_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_5.00_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                VolumeName  = "VOLUMELABEL",
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_5.00_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_5.00_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_6.00_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_6.00_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_6.00_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_6.00_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_6.00_mf2ed.img.lz",
                MediaType   = MediaType.ECMA_147,
                Sectors     = 5760,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2863,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_6.00_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_6.00_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_6.00_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.02_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BF63C69"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.02_dsdd8.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_8,
                Sectors      = 640,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 315,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BF70E75"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.02_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BF7185F"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.02_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BF80C4F"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.02_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BF90F1D"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.02_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1607282D"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.02_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BF72430"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.02_ssdd8.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_8,
                Sectors      = 320,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 313,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BF72F1E"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.03_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0C1A2013"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.03_dsdd8.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_8,
                Sectors      = 640,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 315,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0CE22B5B"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.03_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0CEA1D3E"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.03_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0CEE102F"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.03_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0CEE3760"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_7.03_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "16080521"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.00_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFD1977"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.00_dsdd8.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_8,
                Sectors      = 640,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 315,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFD2D3F"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.00_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFD3531"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.00_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFC3231"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.00_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFA1D58"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.00_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "16081A56"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.00_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFE0971"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.00_ssdd8.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_8,
                Sectors      = 320,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 313,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFE1423"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.10_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "DRDOS  7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "00000000"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.30_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                VolumeName  = "VOLUMELABEL",
                SystemId    = "MSDOS3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.30A_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.30A_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.30A_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "MSDOS3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.30A_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.30A_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                VolumeName  = "VOLUMELABEL",
                SystemId    = "MSDOS3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.30A_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "MSDOS3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.30A_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.31_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.31_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.31_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.31_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.31_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                VolumeName  = "VOLUMELABEL",
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.31_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.31_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "msdos_4.00_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "07200903"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_4.01_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSDOS4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "122C190A"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_4.01_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "msdos_4.01_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2480190A"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_4.01_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2D471909"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_4.01_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0F5A1908"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_4.01_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSDOS4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2F3D190A"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_4.01_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "msdos_5.00_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0B6018F8"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_5.00_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "msdos_5.00_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1E3518F8"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_5.00_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "285A18FB"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_5.00_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "231D18FE"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_5.00_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2159090B"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_5.00_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "316118F8"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_5.00_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "msdos_5.00a_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "383D090C"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.00_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "067B18F6"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_6.00_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.00_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "193418F6"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.00_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1F3A18F5"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.00_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "165318F3"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.00_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1A2C08FB"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.00_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "234918F6"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_6.00_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "265418ED"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_6.20_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0B7018EE"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "127418F0"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "137F18F2"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2C090907"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "185C18EE"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_6.20_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20rc1_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "064B18EB"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_6.20rc1_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20rc1_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "192518EB"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20rc1_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "244C18EA"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20rc1_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3C3118E7"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20rc1_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "344118E9"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.20rc1_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "267E18EB"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_6.20rc1_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.21_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2A41181B"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_6.21_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.21_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0641181C"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.21_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3B26181C"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.21_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "082518E2"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.21_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "214B0917"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.21_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "123F181C"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_6.21_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.22_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "317C1818"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_6.22_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.22_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0D3A1819"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.22_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3C251817"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.22_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "387A1815"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.22_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0F2808F3"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_6.22_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "18231819"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_6.22_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "msdos_7.10_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1156180A"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_7.10_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "msdos_7.10_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2951180A"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_7.10_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3057180B"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_7.10_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2B4A1811"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_7.10_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "344B180C"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_7.10_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "352D180A"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_7.10_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile    = "msdos_amstrad_3.20_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_amstrad_3.20_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile    = "msdos_amstrad_3.20_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_amstrad_3.20_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_amstrad_3.20_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_amstrad_3.20_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile    = "msdos_att_2.11_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "PSA 1.04"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_dell_3.30_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_dell_3.30_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile    = "msdos_dell_3.30_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_dell_3.30_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_dell_3.30_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_dell_3.30_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_dell_3.30_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile    = "msdos_epson_3.10_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "EPS 3.10"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_epson_3.10_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024,
                SystemId    = "EPS 3.10"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_epson_3.10_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "EPS 3.10"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_epson_3.20_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "IBM  3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_epson_3.20_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024,
                SystemId    = "IBM  3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_epson_3.20_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "IBM  3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_epson_3.20_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "IBM  3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_epson_3.20_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "IBM  3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_epson_3.20_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512,
                SystemId    = "IBM  3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hp_3.20_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hp_3.20_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hp_3.20_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hp_3.20_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hp_3.20_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2853,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hp_3.20_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hp_3.20_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hyonsung_3.21_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hyonsung_3.21_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hyonsung_3.21_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hyonsung_3.21_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hyonsung_3.21_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2853,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hyonsung_3.21_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_hyonsung_3.21_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile    = "msdos_kaypro_3.21_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_kaypro_3.21_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile    = "msdos_kaypro_3.21_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_kaypro_3.21_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_kaypro_3.21_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2853,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_kaypro_3.21_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "MSDOS3.2"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_kaypro_3.21_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile    = "msdos_olivetti_3.10_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "IBM  3.1"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_olivetti_3.10_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "IBM  3.1"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_olivetti_3.10_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "IBM  3.1"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_toshiba_3.30_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_toshiba_3.30_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile    = "msdos_toshiba_3.30_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_toshiba_3.30_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_toshiba_3.30_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_toshiba_3.30_ssdd.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_9,
                Sectors     = 360,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 351,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_toshiba_3.30_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "msdos_toshiba_4.01_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "T V4.00 ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0B2519E7"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_toshiba_4.01_dsdd8.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_8,
                Sectors      = 640,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 315,
                ClusterSize  = 1024,
                SystemId     = "T V4.00 ",
                VolumeName   = "NO NAME",
                VolumeSerial = "163419E7"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_toshiba_4.01_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "T V4.00 ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1E3119E7"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_toshiba_4.01_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "T V4.00 ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "133919E9"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_toshiba_4.01_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "T V4.00 ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "177419EA"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_toshiba_4.01_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "T V4.00 ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "317E19E7"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_toshiba_4.01_ssdd8.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_8,
                Sectors      = 320,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 313,
                ClusterSize  = 512,
                SystemId     = "T V4.00 ",
                VolumeName   = "NO NAME",
                VolumeSerial = "3B7319E7"
            },
            new FileSystemTest
            {
                TestFile     = "novelldos_7.00_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "NWDOS7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BE7254C"
            },
            new FileSystemTest
            {
                TestFile     = "novelldos_7.00_dsdd8.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_8,
                Sectors      = 640,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 315,
                ClusterSize  = 1024,
                SystemId     = "NWDOS7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BE73024"
            },
            new FileSystemTest
            {
                TestFile     = "novelldos_7.00_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "NWDOS7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BE7397C"
            },
            new FileSystemTest
            {
                TestFile     = "novelldos_7.00_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "NWDOS7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BE63635"
            },
            new FileSystemTest
            {
                TestFile     = "novelldos_7.00_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "NWDOS7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BE51661"
            },
            new FileSystemTest
            {
                TestFile     = "novelldos_7.00_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "NWDOS7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "161B1226"
            },
            new FileSystemTest
            {
                TestFile     = "novelldos_7.00_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "NWDOS7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BE80A5D"
            },
            new FileSystemTest
            {
                TestFile     = "novelldos_7.00_ssdd8.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_8,
                Sectors      = 320,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 313,
                ClusterSize  = 512,
                SystemId     = "NWDOS7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BE8144C"
            },
            new FileSystemTest
            {
                TestFile     = "opendos_7.01_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "OPENDOS7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BE93E2B"
            },
            new FileSystemTest
            {
                TestFile     = "opendos_7.01_dsdd8.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_8,
                Sectors      = 640,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 315,
                ClusterSize  = 1024,
                SystemId     = "OPENDOS7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BEA234D"
            },
            new FileSystemTest
            {
                TestFile     = "opendos_7.01_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "OPENDOS7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BEA325D"
            },
            new FileSystemTest
            {
                TestFile     = "opendos_7.01_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "OPENDOS7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BEB294F"
            },
            new FileSystemTest
            {
                TestFile     = "opendos_7.01_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "OPENDOS7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BEC2C2E"
            },
            new FileSystemTest
            {
                TestFile     = "opendos_7.01_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "OPENDOS7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "16090D37"
            },
            new FileSystemTest
            {
                TestFile     = "opendos_7.01_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "OPENDOS7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BEA3E60"
            },
            new FileSystemTest
            {
                TestFile     = "opendos_7.01_ssdd8.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_8,
                Sectors      = 320,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 313,
                ClusterSize  = 512,
                SystemId     = "OPENDOS7",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BEB0E26"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_2.00_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "IBM  2.0"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_2.10_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "IBM  2.0"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_2000_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM  7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2634100E"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_2000_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_2000_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM  7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3565100E"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_2000_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM  7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3B6B1012"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_2000_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM  7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3B2D1013"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_2000_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM  7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3E46090A"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_2000_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "IBM  7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "4136100E"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_2000_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_3.00_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "IBM  3.0"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_3.10_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "IBM  3.1"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_3.20_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2853,
                ClusterSize = 512,
                VolumeName  = "VOLUMELABEL",
                SystemId    = "IBM  3.2"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_3.30_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_3.30_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_4.00_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM  4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3C240FE3"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_4.00_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM  4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0E6409F3"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_4.01_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM  4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0F2F0A01"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.00_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "33260FF9"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_5.00_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.00_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "11550FFA"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.00_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "234F0FFB"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.00_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2F600FFC"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.00_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "31090904"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.00_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1D630FFA"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_5.00_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.02_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "06231000"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_5.02_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.02_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1A3E1000"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.02_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1F3B0FFF"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.02_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3D750FFD"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.02_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "09410902"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_5.02_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "26471000"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_5.02_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.10_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "25551004"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_6.10_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.10_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3E5F1004"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.10_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "142D1006"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.10_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "17541007"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.10_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "382408FE"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.10_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0D5E1005"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_6.10_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.30_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2B22100C"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_6.30_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.30_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3B47100C"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.30_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0C55100C"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.30_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1B80100A"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.30_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1F2A0901"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_6.30_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0A3A100D"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos_6.30_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "pcdos_7.00_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM  7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1407090D"
            },
            new FileSystemTest
            {
                TestFile     = "mkfs.vfat_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "mkfs.fat",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "20C279B1"
            },
            new FileSystemTest
            {
                TestFile     = "mkfs.vfat_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "mkfs.fat",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "20FD9501"
            },
            new FileSystemTest
            {
                TestFile     = "mkfs.vfat_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "mkfs.fat",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2132D70A"
            },
            new FileSystemTest
            {
                TestFile     = "mkfs.vfat_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "mkfs.fat",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2118F1AA"
            },
            new FileSystemTest
            {
                TestFile     = "mkfs.vfat_atari_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Clusters     = 1188,
                ClusterSize  = 1024,
                SystemId     = "mkdosf",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "83E030"
            },
            new FileSystemTest
            {
                TestFile     = "mkfs.vfat_atari_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "mkdosf",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "C53F06"
            },
            new FileSystemTest
            {
                TestFile     = "mkfs.vfat_atari_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "mkdosf",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "A154CD"
            },
            new FileSystemTest
            {
                TestFile     = "mkfs.vfat_atari_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Clusters     = 1427,
                ClusterSize  = 1024,
                SystemId     = "mkdosf",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "D54DEE"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.00_tandy_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "TAN 10.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9C170C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.00_tandy_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "TAN 10.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9BFB0C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.00_tandy_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "TAN 10.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9C13FC15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.00_tandy_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "TAN 10.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9BF99C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.10_ast_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66A42C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.10_ast_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 10.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "67696C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.10_ast_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66DEBC15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.10_ast_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 10.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66DC4C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.10_nokia_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "676B4C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.10_nokia_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 10.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "67768C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.10_nokia_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9C12DC15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.10_nokia_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 10.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66A74C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.21_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9C074C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.21_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66BCFC15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.21_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66C1AC15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.21_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66C7FC15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.30.1_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66C47C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.30.1_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66CBEC15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.30.1_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9C167C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.30.1_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9C147C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.30.1_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9C0FEC15"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.20_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5BF5E015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.20_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5BE61015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.20_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5C26F015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.20_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5D0CC815"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.30_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5C418015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.30_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5BE20015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.30_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5C7F1015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.30_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5D0DE815"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.307_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5C3BD015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.307_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5B807015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.307_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5BE69015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.307_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5C187015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.307_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5D14F815"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.514_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFCB414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.514_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6C6C414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.514_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6CCF414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.514_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6AF6414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.514_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5D490415"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.617_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6AEB414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.617_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1C00D414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.617_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1C03B414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.617_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6C90414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.617_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5D23B415"
            },
            new FileSystemTest
            {
                TestFile     = "os2_8.162_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6AF7414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_8.162_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6D63414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_8.162_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6A65414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_8.162_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5CFCB415"
            },
            new FileSystemTest
            {
                TestFile     = "os2_9.023_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6CD9414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_9.023_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFAD414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_9.023_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6DFF414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_9.023_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5CFB8415"
            },
            new FileSystemTest
            {
                TestFile     = "ecs_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "IBM 4.50",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6CA5814"
            },
            new FileSystemTest
            {
                TestFile     = "ecs_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "IBM 4.50",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6CBC814"
            },
            new FileSystemTest
            {
                TestFile     = "ecs_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "IBM 4.50",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6B81814"
            },
            new FileSystemTest
            {
                TestFile     = "ecs_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 4.50",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1C013814"
            },
            new FileSystemTest
            {
                TestFile     = "ecs20_mf2hd_fstester.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "IBM 4.50",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9BF37814"
            },
            new FileSystemTest
            {
                TestFile    = "win95_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "win95_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3B360D0D"
            },
            new FileSystemTest
            {
                TestFile     = "win95_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "24240D0D"
            },
            new FileSystemTest
            {
                TestFile     = "win95_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3C260D11"
            },
            new FileSystemTest
            {
                TestFile     = "win95_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "30050D10"
            },
            new FileSystemTest
            {
                TestFile     = "win95_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "275A0D11"
            },
            new FileSystemTest
            {
                TestFile    = "win95_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "win95_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3B100D0F"
            },
            new FileSystemTest
            {
                TestFile    = "win95osr2_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1C5B0D19"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "11510D19"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0F1F0D15"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "40200D17"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3D610D14"
            },
            new FileSystemTest
            {
                TestFile    = "win95osr2_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "280B0D19"
            },
            new FileSystemTest
            {
                TestFile    = "win95osr2.1_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.1_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1F3B0D1C"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.1_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "14470D1C"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.1_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1C510DE4"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.1_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2E250DE2"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.1_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "10640DE4"
            },
            new FileSystemTest
            {
                TestFile    = "win95osr2.1_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.1_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2B3E0D1C"
            },
            new FileSystemTest
            {
                TestFile    = "win95osr2.5_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.5_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "18190DFB"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.5_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0A240DFB"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.5_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1E320DE7"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.5_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "33230DE8"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.5_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "125B0DE7"
            },
            new FileSystemTest
            {
                TestFile    = "win95osr2.5_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.5_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "21410DFB"
            },
            new FileSystemTest
            {
                TestFile    = "win98_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "win98_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "40090E0F"
            },
            new FileSystemTest
            {
                TestFile     = "win98_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "28140E0F"
            },
            new FileSystemTest
            {
                TestFile     = "win98_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0E620E0A"
            },
            new FileSystemTest
            {
                TestFile     = "win98_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "14390E0D"
            },
            new FileSystemTest
            {
                TestFile     = "win98_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0E081246"
            },
            new FileSystemTest
            {
                TestFile    = "win98_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "win98_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "30600E10"
            },
            new FileSystemTest
            {
                TestFile    = "win98se_dsdd8.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_8,
                Sectors     = 640,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 315,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile     = "win98se_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1B550EEC"
            },
            new FileSystemTest
            {
                TestFile     = "win98se_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1B100EEB"
            },
            new FileSystemTest
            {
                TestFile     = "win98se_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "08410EE6"
            },
            new FileSystemTest
            {
                TestFile     = "win98se_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0E0F0EE8"
            },
            new FileSystemTest
            {
                TestFile     = "win98se_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "325D0EE4"
            },
            new FileSystemTest
            {
                TestFile    = "win98se_ssdd8.img.lz",
                MediaType   = MediaType.DOS_525_SS_DD_8,
                Sectors     = 320,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 313,
                ClusterSize = 512
            },
            new FileSystemTest
            {
                TestFile     = "win98se_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 351,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "13380EEC"
            },
            new FileSystemTest
            {
                TestFile     = "winme_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2F200F02"
            },
            new FileSystemTest
            {
                TestFile     = "winme_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "103A0F01"
            },
            new FileSystemTest
            {
                TestFile     = "winme_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2F1C0EFC"
            },
            new FileSystemTest
            {
                TestFile     = "winme_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "21570EFF"
            },
            new FileSystemTest
            {
                TestFile     = "winme_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "07040EFB"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.10_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "60EA50BC"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.10_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "6C857D51"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.10_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "4009440C"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.10_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "30761EDC"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.50_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0C478404"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.50_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "7CBEB35B"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.50_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "7C1E8DCB"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.50_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "ECB276AF"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.51_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "482D8681"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.51_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "8889C95E"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.51_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "54DE6C39"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.51_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "F47D2516"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_4_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "D8CAAC1F"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_4_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E0BB6D70"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_4_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "C08C3C60"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_4_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9C44B411"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_4_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "NO NAME",
                VolumeSerial = "D4F453A2"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_4_ssdd.img.lz",
                MediaType    = MediaType.DOS_525_SS_DD_9,
                Sectors      = 360,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 348,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "4CD82982"
            },
            new FileSystemTest
            {
                TestFile     = "win2000_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "4019989C"
            },
            new FileSystemTest
            {
                TestFile     = "win2000_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "78F30AF8"
            },
            new FileSystemTest
            {
                TestFile     = "win2000_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E4217DDE"
            },
            new FileSystemTest
            {
                TestFile     = "win2000_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "80B3B996"
            },
            new FileSystemTest
            {
                TestFile     = "win2000_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "28043527"
            },
            new FileSystemTest
            {
                TestFile     = "winvista_dsdd.img.lz",
                MediaType    = MediaType.DOS_525_DS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 354,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3C9F0BD2"
            },
            new FileSystemTest
            {
                TestFile     = "winvista_dshd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3A8E465C"
            },
            new FileSystemTest
            {
                TestFile     = "winvista_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "B2EFB822"
            },
            new FileSystemTest
            {
                TestFile     = "winvista_mf2ed.img.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3C30C632"
            },
            new FileSystemTest
            {
                TestFile     = "winvista_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "16DAB07A"
            },
            new FileSystemTest
            {
                TestFile     = "beos_r4.5_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "BeOS    ",
                VolumeName   = "VOLUME LABE",
                VolumeSerial = "00000000"
            },
            new FileSystemTest
            {
                TestFile     = "hatari_mf1dd.st.lz",
                MediaType    = MediaType.DOS_35_SS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Clusters     = 351,
                ClusterSize  = 1024,
                SystemId     = "NNNNNN",
                VolumeName   = "volumelabel",
                VolumeSerial = "A82270"
            },
            new FileSystemTest
            {
                TestFile     = "hatari_mf1dd_10.st.lz",
                MediaType    = MediaType.ATARI_35_SS_DD,
                Sectors      = 800,
                SectorSize   = 512,
                Clusters     = 391,
                ClusterSize  = 1024,
                SystemId     = "NNNNNN",
                VolumeName   = "volumelabel",
                VolumeSerial = "D08917"
            },
            new FileSystemTest
            {
                TestFile     = "hatari_mf1dd_11.st.lz",
                MediaType    = MediaType.ATARI_35_SS_DD_11,
                Sectors      = 880,
                SectorSize   = 512,
                Clusters     = 431,
                ClusterSize  = 1024,
                SystemId     = "NNNNNN",
                VolumeName   = "volumelabel",
                VolumeSerial = "37AD91"
            },
            new FileSystemTest
            {
                TestFile     = "hatari_mf2dd.st.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Clusters     = 711,
                ClusterSize  = 1024,
                SystemId     = "NNNNNN",
                VolumeName   = "volumelabel",
                VolumeSerial = "1ED910"
            },
            new FileSystemTest
            {
                TestFile     = "hatari_mf2dd_10.st.lz",
                MediaType    = MediaType.ATARI_35_DS_DD,
                Sectors      = 1600,
                SectorSize   = 512,
                Clusters     = 791,
                ClusterSize  = 1024,
                SystemId     = "NNNNNN",
                VolumeName   = "volumelabel",
                VolumeSerial = "299DFE"
            },
            new FileSystemTest
            {
                TestFile     = "hatari_mf2dd_11.st.lz",
                MediaType    = MediaType.ATARI_35_DS_DD_11,
                Sectors      = 1760,
                SectorSize   = 512,
                Clusters     = 871,
                ClusterSize  = 1024,
                SystemId     = "NNNNNN",
                VolumeName   = "volumelabel",
                VolumeSerial = "94AE59"
            },
            new FileSystemTest
            {
                TestFile     = "hatari_mf2ed.st.lz",
                MediaType    = MediaType.ECMA_147,
                Sectors      = 5760,
                SectorSize   = 512,
                Clusters     = 2863,
                ClusterSize  = 1024,
                SystemId     = "NNNNNN",
                VolumeName   = "volumelabel",
                VolumeSerial = "3A1757"
            },
            new FileSystemTest
            {
                TestFile     = "hatari_mf2hd.st.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Clusters     = 1423,
                ClusterSize  = 1024,
                SystemId     = "NNNNNN",
                VolumeName   = "volumelabel",
                VolumeSerial = "C08249"
            },
            new FileSystemTest
            {
                TestFile     = "tos_1.04_mf1dd.st.lz",
                MediaType    = MediaType.DOS_35_SS_DD_9,
                Sectors      = 720,
                SectorSize   = 512,
                Clusters     = 351,
                ClusterSize  = 1024,
                SystemId     = "NNNNNN",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2356F0"
            },
            new FileSystemTest
            {
                TestFile     = "tos_1.04_mf2dd.st.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Clusters     = 711,
                ClusterSize  = 1024,
                SystemId     = "NNNNNN",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "51C7A3"
            },
            new FileSystemTest
            {
                TestFile     = "netbsd_1.6_mf2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 713,
                ClusterSize  = 1024,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "EEB51A0C"
            },
            new FileSystemTest
            {
                TestFile     = "netbsd_1.6_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "CCFD1A06"
            },
            new FileSystemTest
            {
                TestFile    = "nextstep_3.3_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "NEXT    ",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "nextstep_3.3_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "NEXT    ",
                VolumeName  = "VOLUME LABE"
            },
            new FileSystemTest
            {
                TestFile    = "openstep_4.0_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "NEXT    ",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "openstep_4.0_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "NEXT    ",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "openstep_4.2_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "NEXT    ",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "openstep_4.2_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "NEXT    ",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "solaris_2.4_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "MSDOS3.3"
            },
            new FileSystemTest
            {
                TestFile    = "solaris_2.4_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "MSDOS3.3"
            },
            new FileSystemTest
            {
                TestFile    = "coherentunix_4.2.10_dsdd.img.lz",
                MediaType   = MediaType.DOS_525_DS_DD_9,
                Sectors     = 720,
                SectorSize  = 512,
                Clusters    = 354,
                ClusterSize = 1024,
                SystemId    = "COHERENT",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "coherentunix_4.2.10_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "COHERENT",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "coherentunix_4.2.10_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Clusters    = 713,
                ClusterSize = 1024,
                SystemId    = "COHERENT",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "coherentunix_4.2.10_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "COHERENT",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "scoopenserver_5.0.7hw_dshd.img.lz",
                MediaType   = MediaType.DOS_525_HD,
                Sectors     = 2400,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2371,
                ClusterSize = 512,
                SystemId    = "SCO BOOT"
            },
            new FileSystemTest
            {
                TestFile    = "scoopenserver_5.0.7hw_mf2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 1422,
                ClusterSize = 512,
                SystemId    = "SCO BOOT"
            },
            new FileSystemTest
            {
                TestFile    = "scoopenserver_5.0.7hw_mf2hd.img.lz",
                MediaType   = MediaType.DOS_35_HD,
                Sectors     = 2880,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2847,
                ClusterSize = 512,
                SystemId    = "SCO BOOT"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_epson_pc98_5.00_md2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 634,
                ClusterSize  = 1024,
                SystemId     = "EPSON5.0",
                VolumeName   = "NO NAME",
                VolumeSerial = "27021316"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_epson_pc98_5.00_md2hd.img.lz",
                MediaType    = MediaType.SHARP_525,
                Sectors      = 1232,
                SectorSize   = 1024,
                Bootable     = true,
                Clusters     = 1221,
                ClusterSize  = 1024,
                SystemId     = "EPSON5.0",
                VolumeName   = "NO NAME",
                VolumeSerial = "11021317"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_pc98_3.30_md2dd.img.lz",
                MediaType   = MediaType.DOS_35_DS_DD_9,
                Sectors     = 1440,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 634,
                ClusterSize = 1024,
                SystemId    = "NEC 2.00"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_pc98_3.30_md2hd.img.lz",
                MediaType   = MediaType.SHARP_525,
                Sectors     = 1232,
                SectorSize  = 1024,
                Bootable    = true,
                Clusters    = 1221,
                ClusterSize = 1024,
                SystemId    = "NEC 2.00"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_pc98_5.00_md2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 634,
                ClusterSize  = 1024,
                SystemId     = "NEC  5.0",
                VolumeName   = "NO NAME",
                VolumeSerial = "1002120E"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_pc98_5.00_md2hd.img.lz",
                MediaType    = MediaType.SHARP_525,
                Sectors      = 1232,
                SectorSize   = 1024,
                Bootable     = true,
                Clusters     = 1221,
                ClusterSize  = 1024,
                SystemId     = "NEC  5.0",
                VolumeName   = "NO NAME",
                VolumeSerial = "41021209"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_pc98_6.20_md2dd.img.lz",
                MediaType    = MediaType.DOS_35_DS_DD_9,
                Sectors      = 1440,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 634,
                ClusterSize  = 1024,
                SystemId     = "NEC  5.0",
                VolumeName   = "NO NAME",
                VolumeSerial = "3D021418"
            },
            new FileSystemTest
            {
                TestFile     = "msdos_pc98_6.20_md2hd.img.lz",
                MediaType    = MediaType.SHARP_525,
                Sectors      = 1232,
                SectorSize   = 1024,
                Bootable     = true,
                Clusters     = 1221,
                ClusterSize  = 1024,
                SystemId     = "NEC  5.0",
                VolumeName   = "NO NAME",
                VolumeSerial = "16021409"
            },
            new FileSystemTest
            {
                TestFile     = "geos12_md2hd.img.lz",
                MediaType    = MediaType.DOS_525_HD,
                Sectors      = 2400,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2371,
                ClusterSize  = 512,
                SystemId     = "GEOWORKS",
                VolumeName   = "GEOS12",
                VolumeSerial = "0000049C"
            },
            new FileSystemTest
            {
                TestFile     = "geos20_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "GEOWORKS",
                VolumeName   = "GEOS20",
                VolumeSerial = "8DC94C67"
            },
            new FileSystemTest
            {
                TestFile     = "geos31_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "GEOWORKS",
                VolumeName   = "GEOS32",
                VolumeSerial = "8E0D4C67"
            },
            new FileSystemTest
            {
                TestFile     = "geos32_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "GEOWORKS",
                VolumeName   = "NDO2000",
                VolumeSerial = "8EDB4C67"
            },
            new FileSystemTest
            {
                TestFile     = "geos41_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "GEOWORKS",
                VolumeName   = "GEOS41",
                VolumeSerial = "8D684C67"
            },
            new FileSystemTest
            {
                TestFile     = "beos_r5_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "BeOS    ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "00000000"
            },
            new FileSystemTest
            {
                TestFile     = "dflybsd_1.00_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "BSD  4.4",
                VolumeName   = "NO NAME",
                VolumeSerial = "3E8C1A1F"
            },
            new FileSystemTest
            {
                TestFile     = "netbsd_6.1.5_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2829,
                ClusterSize  = 512,
                SystemId     = "NetBSD  ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2EE71B0B"
            },
            new FileSystemTest
            {
                TestFile     = "netbsd_7.1_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2829,
                ClusterSize  = 512,
                SystemId     = "NetBSD  ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "80C21715"
            },
            new FileSystemTest
            {
                TestFile     = "openbsd_4.7_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "4E6B1F17"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.0_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Clusters     = 2847,
                ClusterSize  = 512,
                VolumeSerial = "670000"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.29_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609AC294"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.34_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609B8CD9"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.37_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "609D1849"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.38_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609BB0AA"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.2.17_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609C4FE6"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.2.20_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609C815D"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.4.18_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2847,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609CA596"
            }
        };
    }
}