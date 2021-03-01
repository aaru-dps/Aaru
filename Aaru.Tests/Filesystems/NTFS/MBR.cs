// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NTFS.cs
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

namespace Aaru.Tests.Filesystems.NTFS
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("NTFS") {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "New Technology File System (MBR)");

        public override IFilesystem _plugin     => new Aaru.Filesystems.NTFS();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "win10.aif", "win2000.aif", "winnt_3.10.aif", "winnt_3.50.aif", "winnt_3.51.aif", "winnt_4.00.aif",
            "winvista.aif", "linux.aif", "haiku_hrev51259.aif", "linux_4.19_ntfs3g_flashdrive.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            524288, 2097152, 1024000, 524288, 524288, 524288, 524288, 262144, 2097152, 1024000
        };

        public override uint[] _sectorSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        public override string[] _appId => null;
        public override bool[] _bootable => new[]
        {
            true, true, true, true, true, true, true, true, true, true
        };

        public override long[] _clusters => new long[]
        {
            65263, 1046511, 1023057, 524256, 524256, 524096, 64767, 32511, 261887, 127743
        };

        public override uint[] _clusterSize => new uint[]
        {
            4096, 1024, 512, 512, 512, 512, 4096, 4096, 4096, 4096
        };

        public override string[] _oemId => new string[]
        {
            null, null, null, null, null, null, null, null, null, null
        };
        public override string[] _type => null;

        public override string[] _volumeName => new string[]
        {
            null, null, null, null, null, null, null, null, null, null
        };

        public override string[] _volumeSerial => new[]
        {
            "C46C1B3C6C1B28A6", "8070C8EC70C8E9CC", "10CC6AC6CC6AA5A6", "7A14F50014F4BFE5", "24884447884419A6",
            "822C288D2C287E73", "E20AF54B0AF51D6B", "065BB96B7C1BCFDA", "46EC796749C6FA66", "1FC3802B52F9611C"
        };
    }
}