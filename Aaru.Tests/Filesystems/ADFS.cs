// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ADFS.cs
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
    public class Adfs : FilesystemTest
    {
        public Adfs() : base("Acorn Advanced Disc Filing System") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                                           "Acorn Advanced Disc Filing System");

        public override IFilesystem _plugin     => new AcornADFS();
        public override bool        _partitions => false;

        public override string[] _testFiles => new[]
        {
            "adfs_d.adf.lz", "adfs_e.adf.lz", "adfs_f.adf.lz", "adfs_e+.adf.lz", "adfs_f+.adf.lz", "adfs_s.adf.lz",
            "adfs_m.adf.lz", "adfs_l.adf.lz", "hdd_old.hdf.lz", "hdd_new.hdf.lz"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_HD, MediaType.ACORN_35_DS_DD,
            MediaType.ACORN_35_DS_HD, MediaType.ACORN_525_SS_DD_40, MediaType.ACORN_525_SS_DD_80,
            MediaType.ACORN_525_DS_DD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            800, 800, 1600, 800, 1600, 640, 1280, 2560, 78336, 78336
        };

        public override uint[] _sectorSize => new uint[]
        {
            1024, 1024, 1024, 1024, 1024, 256, 256, 256, 256, 256
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false, false, false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            800, 800, 1600, 800, 1600, 640, 1280, 2560, 78336, 78336
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override uint[] _clusterSize => new uint[]
        {
            1024, 1024, 1024, 1024, 1024, 256, 256, 256, 256, 256
        };

        public override string[] _volumeName => new[]
        {
            "ADFSD", "ADFSE     ", null, "ADFSE+    ", null, "$", "$", "$", "VolLablOld", null
        };

        public override string[] _volumeSerial => new[]
        {
            "3E48", "E13A", null, "1142", null, "F20D", "D6CA", "0CA6", "080E", null
        };
    }
}