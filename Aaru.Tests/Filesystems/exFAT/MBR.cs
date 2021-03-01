// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : exFAT.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.exFAT
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("exFAT") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "exFAT (MBR)");

        public override IFilesystem _plugin     => new Aaru.Filesystems.exFAT();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "linux.aif", "macosx_10.11.aif", "win10.aif", "winvista.aif", "linux_4.19_exfat_flashdrive.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            262144, 262144, 262144, 262144, 1024000
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
            32464, 32712, 32448, 32208, 15964
        };

        public override uint[] _clusterSize => new uint[]
        {
            4096, 4096, 4096, 4096, 32768
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new string[]
        {
            null, null, null, null, null
        };

        public override string[] _volumeSerial => new[]
        {
            "603565AC", "595AC21E", "20126663", "0AC5CA52", "636E083B"
        };
    }
}