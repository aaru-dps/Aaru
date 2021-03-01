// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFSPlus.cs
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

// ReSharper disable CheckNamespace

namespace Aaru.Tests.Filesystems.HFSPlus
{
    // Mising Darwin 6.0.2 wrapped
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("HFS+") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS+ (MBR)");

        public override IFilesystem _plugin     => new AppleHFSPlus();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "macosx_10.11.aif", "macosx_10.11_journal.aif", "linux.aif", "linux_journal.aif", "darwin_1.3.1.aif",
            "darwin_1.3.1_wrapped.aif", "darwin_1.4.1.aif", "darwin_1.4.1_wrapped.aif", "darwin_6.0.2.aif",
            "darwin_8.0.1_journal.aif", "darwin_8.0.1.aif", "darwin_8.0.1_wrapped.aif", "linux_4.19_hfs+_flashdrive.aif"
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
            303104, 352256, 262144, 262144, 819200, 614400, 819200, 614400, 819200, 1228800, 819200, 614400, 1024000
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            false, false, false, false, false, false, false, false, false, false, false, false, false
        };

        public override long[] _clusters => new long[]
        {
            37878, 44021, 32512, 32512, 102178, 76708, 102178, 76708, 102178, 153592, 102392, 76774, 127744
        };

        public override uint[] _clusterSize => new uint[]
        {
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096
        };
        public override string[] _oemId => new[]
        {
            "10.0", "HFSJ", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "H+Lx"
        };
        public override string[] _type => null;

        public override string[] _volumeName => new string[]
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null
        };

        public override string[] _volumeSerial => new[]
        {
            "C84F550907D13F50", "016599F88029F73D", null, null, null, null, null, null, null, "F92964F9B3F64ABB",
            "A8FAC484A0A2B177", "D5D5BF1346AD2B8D", "B9BAC6856878A404"
        };
    }
}