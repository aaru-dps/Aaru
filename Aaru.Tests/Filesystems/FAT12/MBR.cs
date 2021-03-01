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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FAT12
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("FAT12") {}

        public override string      _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT12 (MBR)");
        public override IFilesystem _plugin     => new FAT();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "compaqmsdos331.aif", "drdos_3.40.aif", "drdos_3.41.aif", "drdos_5.00.aif", "drdos_6.00.aif",
            "drdos_7.02.aif", "drdos_7.03.aif", "drdos_8.00.aif", "msdos331.aif", "msdos401.aif", "msdos500.aif",
            "msdos600.aif", "msdos620rc1.aif", "msdos620.aif", "msdos621.aif", "msdos622.aif", "msdos710.aif",
            "novelldos_7.00.aif", "opendos_7.01.aif", "pcdos2000.aif", "pcdos200.aif", "pcdos210.aif", "pcdos300.aif",
            "pcdos310.aif", "pcdos330.aif", "pcdos400.aif", "pcdos500.aif", "pcdos502.aif", "pcdos610.aif",
            "pcdos630.aif", "toshibamsdos330.aif", "toshibamsdos401.aif", "msos2_1.21.aif", "msos2_1.30.1.aif",
            "multiuserdos_7.22r4.aif", "os2_1.20.aif", "os2_1.30.aif", "os2_6.307.aif", "os2_6.514.aif",
            "os2_6.617.aif", "os2_8.162.aif", "os2_9.023.aif", "ecs.aif", "macosx_10.11.aif", "win10.aif",
            "win2000.aif", "win95.aif", "win95osr2.1.aif", "win95osr2.5.aif", "win95osr2.aif", "win98.aif",
            "win98se.aif", "winme.aif", "winnt_3.10.aif", "winnt_3.50.aif", "winnt_3.51.aif", "winnt_4.00.aif",
            "winvista.aif", "beos_r4.5.aif", "linux.aif", "freebsd_6.1.aif", "freebsd_7.0.aif", "freebsd_8.2.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            8192, 30720, 28672, 28672, 28672, 28672, 28672, 28672, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
            16384, 28672, 28672, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 8192,
            8192, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384,
            16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384, 16384,
            16384, 16384
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };
        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
            true, true, true, true, true, true, true, true, true
        };

        public override long[] _clusters => new long[]
        {
            1000, 3654, 3520, 3520, 3520, 3520, 3520, 3520, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 2008, 3520,
            3520, 4024, 4031, 4031, 4024, 4024, 4024, 4024, 4024, 4024, 4024, 4024, 1000, 1000, 2008, 2008, 2008, 2008,
            2008, 2008, 2008, 2008, 2008, 2008, 1890, 4079, 3552, 4088, 2008, 2008, 2008, 2008, 2044, 2044, 2044, 4016,
            2044, 2044, 4016, 3072, 2040, 3584, 2044, 2044, 2044
        };

        public override uint[] _clusterSize => new uint[]
        {
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 2048, 2048, 2048, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 2048,
            4096, 4096, 2048, 2048, 4096, 2048, 4096, 4096, 4096
        };

        public override string[] _oemId => new[]
        {
            "IBM  3.3", "IBM  3.2", "IBM  3.2", "IBM  3.3", "IBM  3.3", "IBM  3.3", "DRDOS  7", "IBM  5.0", "IBM  3.3",
            "MSDOS4.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSWIN4.1", "IBM  3.3",
            "IBM  3.3", "IBM  7.0", "IBM  2.0", "IBM  2.0", "IBM  3.0", "IBM  3.1", "IBM  3.3", "IBM  4.0", "IBM  5.0",
            "IBM  5.0", "IBM  6.0", "IBM  6.0", "T V3.30 ", "T V4.00 ", "IBM 10.2", "IBM 10.2", "IBM  3.2", "IBM 10.2",
            "IBM 10.2", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 4.50", "BSD  4.4", "MSDOS5.0",
            "MSDOS5.0", "MSWIN4.0", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSDOS5.0",
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "BeOS    ", "mkfs.fat", "BSD  4.4", "BSD  4.4", "BSD4.4  "
        };

        public override string[] _type => null;

        public override string[] _volumeName => new[]
        {
            null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", null, null, null,
            null, null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VolumeLabel", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL"
        };

        public override string[] _volumeSerial => new[]
        {
            null, null, null, null, null, null, null, "1BFB1273", null, "407D1907", "345D18FB", "332518F4", "395718E9",
            "076718EF", "1371181B", "23281816", "2F781809", null, null, "294F100F", null, null, null, null, null,
            "0F340FE4", "1A5E0FF9", "1D2F0FFE", "076C1004", "2C481009", null, "3C2319E8", "66CC3C15", "66A54C15", null,
            "5C578015", "5B845015", "5C4BF015", "E6B5F414", "E6B15414", "E6A41414", "E6A39414", "E6B0B814", "26A21EF4",
            "74F4921D", "C4B64D11", "29200D0C", "234F0DE4", "074C0DFC", "33640D18", "0E121460", "094C0EED", "38310F02",
            "50489A1B", "2CE52101", "94313E7E", "BC184FE6", "BAD08A1E", "00000000", "8D418102", "8FC80E0A", "34FA0E0B",
            "02140E0B"
        };
    }
}