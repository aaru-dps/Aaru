// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MINIX.cs
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

namespace Aaru.Tests.Filesystems.MINIX.V2
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base(null) {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "MINIX v2 filesystem (MBR)");
        public override IFilesystem _plugin     => new MinixFS();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "minix_3.1.2a.aif", "linux_4.19_minix2_flashdrive.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            1024000, 1024000
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false
        };

        public override long[] _clusters => new long[]
        {
            511055, 510976
        };

        public override uint[] _clusterSize => new uint[]
        {
            1024, 1024
        };
        public override string[] _oemId => null;

        public override string[] _type => new[]
        {
            "Minix 3 v2", "Minix v2"
        };
        public override string[] _volumeName => new string[]
        {
            null, null
        };
        public override string[] _volumeSerial => new string[]
        {
            null, null
        };
    }
}