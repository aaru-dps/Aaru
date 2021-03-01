// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFSX.cs
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

namespace Aaru.Tests.Filesystems.HFSX
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("HFSX") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFSX (MBR)");

        public override IFilesystem _plugin     => new AppleHFSPlus();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "macosx_10.11.aif", "macosx_10.11_journal.aif", "linux.aif", "linux_journal.aif",
            "darwin_8.0.1_journal.aif", "darwin_8.0.1.aif", "linux_4.19_hfsx_flashdrive.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            393216, 409600, 262144, 262144, 1638400, 1433600, 1024000
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
            49140, 51187, 32512, 32512, 204792, 179192, 127744
        };

        public override uint[] _clusterSize => new uint[]
        {
            4096, 4096, 4096, 4096, 4096, 4096, 4096
        };
        public override string[] _oemId => new[]
        {
            "10.0", "HFSJ", "10.0", "10.0", "10.0", "10.0", "H+Lx"
        };
        public override string[] _type => null;

        public override string[] _volumeName => new string[]
        {
            null, null, null, null, null, null, null
        };

        public override string[] _volumeSerial => new[]
        {
            "C2BCCCE6DE5BC98D", "AC54CD78C75CC30F", null, null, "7559DD01BCFADD9A", "AEA39CFBBF14C0FF",
            "5E4A8781D3C9286C"
        };
    }
}