// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFS.cs
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

namespace Aaru.Tests.Filesystems.HFS
{
    [TestFixture]
    public class Whole : FilesystemTest
    {
        public Whole() : base("HFS") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS");

        public override IFilesystem _plugin     => new AppleHFS();
        public override bool        _partitions => false;

        public override string[] _testFiles => new[]
        {
            "macos_1.1_mf2dd.img.lz", "macos_2.0_mf2dd.img.lz", "macos_6.0.7_mf2dd.img.lz", "nextstep_3.3_mf2hd.img.lz",
            "openstep_4.0_mf2hd.img.lz", "openstep_4.2_mf2hd.img.lz", "rhapsody_dr1_mf2hd.img.lz",
            "ecs20_mf2hd_fstester.img.lz"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.DOS_35_HD,
            MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD
        };

        public override ulong[] _sectors => new ulong[]
        {
            1600, 1600, 1600, 2880, 2880, 2880, 2880, 2880
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            1594, 1594, 1594, 2874, 2874, 2874, 2874, 2874
        };

        public override uint[] _clusterSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "VOLUME LABEL"
        };

        public override string[] _volumeSerial => new string[]
        {
            null, null, null, null, null, null, null, null
        };
    }
}