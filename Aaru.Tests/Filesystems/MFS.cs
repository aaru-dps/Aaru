// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MFS.cs
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
    public class Mfs : FilesystemTest
    {
        public Mfs() : base("MFS") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                                           "Macintosh File System");
        public override IFilesystem _plugin     => new AppleMFS();
        public override bool        _partitions => false;

        public override string[] _testFiles => new[]
        {
            "macos_0.1_mf1dd.img.lz", "macos_0.5_mf1dd.img.lz", "macos_1.1_mf1dd.img.lz", "macos_2.0_mf1dd.img.lz",
            "macos_6.0.7_mf1dd.img.lz"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleSonySS
        };

        public override ulong[] _sectors => new ulong[]
        {
            800, 800, 800, 800, 800
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            391, 391, 391, 391, 391
        };

        public override uint[] _clusterSize => new uint[]
        {
            1024, 1024, 1024, 1024, 1024
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label"
        };

        public override string[] _volumeSerial => new string[]
        {
            null, null, null, null, null, null, null
        };
    }
}