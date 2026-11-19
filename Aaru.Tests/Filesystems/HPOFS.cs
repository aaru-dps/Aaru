// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPOFS.cs
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems;

[TestFixture]
public class Hpofs() : ReadOnlyFilesystemTest("hpofs")
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Filesystems", "High Performance Optical File System");

    public override IFilesystem Plugin     => new HPOFS();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests =>
    [
        new()
        {
            TestFile     = "rid1.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Clusters     = 2880,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUME LABEL",
            VolumeSerial = "AC226814"
        },
        new()
        {
            TestFile     = "rid10.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Clusters     = 2880,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUME LABEL",
            VolumeSerial = "AC160814"
        },
        new()
        {
            TestFile     = "rid66percent.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Clusters     = 2880,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUME LABEL",
            VolumeSerial = "AC306C14"
        },
        new()
        {
            TestFile     = "rid266.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Clusters     = 2880,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUME LABEL",
            VolumeSerial = "ABEF2C14"
        },
        new()
        {
            TestFile     = "hpofs_floppy.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Clusters     = 2880,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "NO NAME",
            VolumeSerial = "2C545C15"
        },
        new()
        {
            TestFile     = "hpofs_230m_r10_cp850_empty.img.lz",
            MediaType    = MediaType.ECMA_201,
            Sectors      = 446325,
            SectorSize   = 512,
            Clusters     = 446325,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "01RVN",
            VolumeSerial = "ACC36814"
        },
        new()
        {
            TestFile     = "hpofs_230m_r10_cp850_fstester_empty.img.lz",
            MediaType    = MediaType.ECMA_201,
            Sectors      = 446325,
            SectorSize   = 512,
            Clusters     = 446325,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "MLKJIHGFEDCBA9876SRETCARAHC23XAM",
            VolumeSerial = "ECA2E014"
        },
        new()
        {
            TestFile     = "hpofs_230m_r266_cp850_empty.img.lz",
            MediaType    = MediaType.ECMA_201,
            Sectors      = 446325,
            SectorSize   = 512,
            Clusters     = 446325,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "01RVN",
            VolumeSerial = "ACD0B814"
        },
        new()
        {
            TestFile     = "hpofs_230m_r266_cp850_fstester_empty.img.lz",
            MediaType    = MediaType.ECMA_201,
            Sectors      = 446325,
            SectorSize   = 512,
            Clusters     = 446325,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "MLKJIHGFEDCBA9876SRETCARAHC23XAM",
            VolumeSerial = "ECE34C14"
        },
        new()
        {
            TestFile     = "hpofs_230m_r266_cp500_files.img.lz",
            MediaType    = MediaType.ECMA_201,
            Sectors      = 446325,
            SectorSize   = 512,
            Clusters     = 446325,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            EncodingName = "IBM500",
            VolumeName   = "FILES_266",
            VolumeSerial = "EC8A9C14"
        },
        new()
        {
            TestFile     = "hpofs_230m_r1_cp850_empty.img.lz",
            MediaType    = MediaType.ECMA_201,
            Sectors      = 446325,
            SectorSize   = 512,
            Clusters     = 446325,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "1RBVN",
            VolumeSerial = "ACA20414"
        },
        new()
        {
            TestFile     = "hpofs_230m_r1_cp850_fstester_empty.img.lz",
            MediaType    = MediaType.ECMA_201,
            Sectors      = 446325,
            SectorSize   = 512,
            Clusters     = 446325,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "NO NAME",
            VolumeSerial = "ECC18414"
        },
        new()
        {
            TestFile     = "hpofs_230m_r257_cp850_empty.img.lz",
            MediaType    = MediaType.ECMA_201,
            Sectors      = 446325,
            SectorSize   = 512,
            Clusters     = 446325,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            VolumeName   = "752RBVN",
            VolumeSerial = "ACAAD814"
        },
        new()
        {
            TestFile     = "hpofs_230m_r257_cp500_files.img.lz",
            MediaType    = MediaType.ECMA_201,
            Sectors      = 446325,
            SectorSize   = 512,
            Clusters     = 446325,
            ClusterSize  = 512,
            SystemId     = "IBM 10.2",
            EncodingName = "IBM500",
            VolumeName   = "FILES_257",
            VolumeSerial = "EC898014"
        }
    ];
}