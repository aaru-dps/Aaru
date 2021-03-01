// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPFS.cs
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

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Hpfs : FilesystemTest
    {
        public Hpfs() : base("HPFS") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                                           "High Performance File System");
        public override IFilesystem _plugin     => new HPFS();
        public override bool        _partitions => true;
        public override string[] _testFiles => new[]
        {
            "ecs.aif", "msos2_1.21.aif", "msos2_1.30.1.aif", "os2_1.20.aif", "os2_1.30.aif", "os2_6.307.aif",
            "os2_6.514.aif", "os2_6.617.aif", "os2_8.162.aif", "os2_9.023.aif", "winnt_3.10.aif", "winnt_3.50.aif",
            "ecs20_fstester.aif"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD
        };
        public override ulong[] _sectors => new ulong[]
        {
            262144, 1024000, 1024000, 1024000, 1024000, 1024000, 262144, 262144, 262144, 262144, 262144, 262144, 1024000
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };
        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            true, true, true, true, true, true, true, true, true, true, true, true, true
        };

        public override long[] _clusters => new long[]
        {
            261072, 1023056, 1023056, 1023056, 1023056, 1023056, 262016, 262016, 262016, 262016, 262016, 262112, 1022112
        };

        public override uint[] _clusterSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };
        public override string[] _oemId => new[]
        {
            "IBM 4.50", "OS2 10.1", "OS2 10.0", "OS2 10.0", "OS2 10.0", "OS2 20.0", "OS2 20.0", "OS2 20.1", "OS2 20.0",
            "OS2 20.0", "MSDOS5.0", "MSDOS5.0", "IBM 4.50"
        };
        public override string[] _type => null;

        public override string[] _volumeName => new[]
        {
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUME LABE"
        };

        public override string[] _volumeSerial => new[]
        {
            "2BBBD814", "AC0DDC15", "ABEB2C15", "6C4EE015", "6C406015", "6C49B015", "2BCEB414", "2C157414", "2BF55414",
            "2BE31414", "E851CB14", "A4EDC29C", "AC096014"
        };
    }
}