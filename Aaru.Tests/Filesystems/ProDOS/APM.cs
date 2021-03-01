// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ProDOS.cs
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

namespace Aaru.Tests.Filesystems.ProDOS
{
    [TestFixture]
    public class APM : FilesystemTest
    {
        public APM() : base("ProDOS") {}

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "ProDOS filesystem (APM)");

        public override IFilesystem _plugin     => new ProDOSPlugin();
        public override bool        _partitions => true;

        public override string[] _testFiles => new[]
        {
            "macos_7.5.3.aif", "macos_7.6.aif", "macos_8.0.aif", "macos_8.1.aif", "macos_9.0.4.aif", "macos_9.1.aif",
            "macos_9.2.1.aif", "macos_9.2.2.aif"
        };
        public override MediaType[] _mediaTypes => new[]
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        public override ulong[] _sectors => new ulong[]
        {
            49152, 49152, 49152, 49152, 49152, 49152, 49152, 49152
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
            48438, 48438, 48438, 48438, 46326, 46326, 46326, 46326
        };

        public override uint[] _clusterSize => new uint[]
        {
            512, 512, 512, 512, 512, 512, 512, 512
        };
        public override string[] _oemId => null;
        public override string[] _type  => null;

        public override string[] _volumeName => new[]
        {
            "VOLUME.LABEL", "VOLUME.LABEL", "VOLUME.LABEL", "VOLUME.LABEL", "VOLUME.LABEL", "VOLUME.LABEL",
            "VOLUME.LABEL", "VOLUME.LABEL"
        };

        public override string[] _volumeSerial => new string[]
        {
            null, null, null, null, null, null, null, null
        };
    }
}