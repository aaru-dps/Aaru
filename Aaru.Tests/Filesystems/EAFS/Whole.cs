// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : EAFS.cs
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

namespace Aaru.Tests.Filesystems.EAFS
{
    [TestFixture]
    public class Whole : FilesystemTest
    {
        public Whole() : base("Extended Acer Fast Filesystem") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "EAFS");

        public override IFilesystem _plugin     => new SysVfs();
        public override bool        _partitions => false;

        public override string[] _testFiles => new[]
        {
            "scoopenserver_5.0.7hw_dmf.img.lz", "scoopenserver_5.0.7hw_dshd.img.lz",
            "scoopenserver_5.0.7hw_mf2dd.img.lz", "scoopenserver_5.0.7hw_mf2ed.img.lz",
            "scoopenserver_5.0.7hw_mf2hd.img.lz"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.DMF, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD
        };

        public override ulong[] _sectors => new ulong[]
        {
            3360, 2400, 1440, 5760, 2880
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512
        };
        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            1680, 1200, 720, 2880, 1440
        };

        public override uint[] _clusterSize => new uint[]
        {
            1024, 1024, 1024, 1024, 1024
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "", "", "", "", ""
        };

        public override string[] _volumeSerial => new string[]
        {
            null, null, null, null, null
        };
    }
}