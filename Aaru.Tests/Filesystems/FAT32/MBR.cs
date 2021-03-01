// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT32.cs
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FAT32
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("FAT32") {}

        public override string      _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT32 (MBR)");
        public override IFilesystem _plugin     => new FAT();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "drdos_7.03.aif", "drdos_8.00.aif", "msdos_7.10.aif", "macosx_10.11.aif", "win10.aif", "win2000.aif",
            "win95osr2.1.aif", "win95osr2.5.aif", "win95osr2.aif", "win98se.aif", "win98.aif", "winme.aif",
            "winvista.aif", "beos_r4.5.aif", "linux.aif", "aros.aif", "freebsd_6.1.aif", "freebsd_7.0.aif",
            "freebsd_8.2.aif", "freedos_1.2.aif", "ecs20_fstester.aif", "linux_2.2_umsdos32_flashdrive.aif",
            "linux_4.19_fat32_msdos_flashdrive.aif", "linux_4.19_vfat32_flashdrive.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            8388608, 8388608, 8388608, 4194304, 4194304, 8388608, 4194304, 4194304, 4194304, 4194304, 4194304, 4194304,
            4194304, 4194304, 262144, 4194304, 4194304, 4194304, 4194304, 8388608, 1024000, 1024000, 1024000, 1024000
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false, true, true,
            true, true, true, true, true, true
        };

        public override long[] _clusters => new long[]
        {
            1048233, 1048233, 1048233, 524287, 524016, 1048233, 524152, 524152, 524152, 524112, 524112, 524112, 523520,
            1048560, 260096, 524160, 524112, 524112, 65514, 1048233, 127744, 127882, 127744, 127744
        };

        public override uint[] _clusterSize => new uint[]
        {
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 2048, 512, 4096, 4096, 4096,
            32768, 4096, 4096, 4096, 4096, 4096
        };
        public override string[] _oemId => new[]
        {
            "DRDOS7.X", "IBM  7.1", "MSWIN4.1", "BSD  4.4", "MSDOS5.0", "MSDOS5.0", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1",
            "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSDOS5.0", "BeOS    ", "mkfs.fat", "MSWIN4.1", "BSD  4.4", "BSD  4.4",
            "BSD4.4  ", "FRDOS4.1", "mkfs.fat", "mkdosfs", "mkfs.fat", "mkfs.fat"
        };
        public override string[] _type => null;

        public override string[] _volumeName => new[]
        {
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VolumeLabel", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "Volume labe",
            "DICSETTER", "DICSETTER", "DICSETTER"
        };

        public override string[] _volumeSerial => new[]
        {
            "5955996C", "1BFB1A43", "3B331809", "42D51EF1", "48073346", "EC62E6DE", "2A310DE4", "0C140DFC", "3E310D18",
            "0D3D0EED", "0E131162", "3F500F02", "82EB4C04", "00000000", "B488C502", "5CAC9B4E", "41540E0E", "4E600E0F",
            "26E20E0F", "3E0C1BE8", "63084BBA", "5CC7908D", "D1290612", "79BCA86E"
        };
    }
}