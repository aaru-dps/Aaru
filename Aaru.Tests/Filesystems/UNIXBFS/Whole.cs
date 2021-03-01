// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UNIXBFS.cs
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

namespace Aaru.Tests.Filesystems.UNIXBFS
{
    [TestFixture]
    public class Whole : FilesystemTest
    {
        public Whole() : base("BFS") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Boot File System");

        public override IFilesystem _plugin     => new BFS();
        public override bool        _partitions => false;

        public override string[] _testFiles => new[]
        {
            "amix_mf2dd.adf.lz", "att_unix_svr4v2.1_dsdd.img.lz", "att_unix_svr4v2.1_dshd.img.lz",
            "att_unix_svr4v2.1_mf2dd.img.lz", "att_unix_svr4v2.1_mf2hd.img.lz"
        };

        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.CBM_AMIGA_35_DD, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD
        };

        public override ulong[] _sectors => new ulong[]
        {
            1760, 720, 2400, 1440, 2880
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
            1760, 720, 2400, 1440, 2880
        };

        public override uint[] _clusterSize => new uint[]
        {
            512, 512, 512, 512, 512
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "Label", null, null, null, null
        };

        public override string[] _volumeSerial => new string[]
        {
            null, null, null, null, null
        };
    }
}