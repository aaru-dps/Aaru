// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PocketZip.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.Filters;
using Aaru.Images;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Devices;

[TestFixture]
public class PocketZip
{
    readonly string[] _testFiles =
    {
        "clik!.bin.lz", "pocketzip.bin.lz"
    };

    readonly MediaType[] _mediaTypes =
    {
        MediaType.PocketZip, MediaType.PocketZip
    };

    readonly ulong[] _sectors =
    {
        78882, 78882
    };

    readonly uint[] _sectorSize =
    {
        512, 512
    };

    readonly string _dataFolder = Path.Combine(Consts.TestFilesRoot, "Device test dumps", "PocketZIP");

    [Test]
    public void Info()
    {
        Environment.CurrentDirectory = _dataFolder;

        Assert.Multiple(() =>
        {
            for(var i = 0; i < _testFiles.Length; i++)
            {
                var filter = new LZip();
                filter.Open(_testFiles[i]);

                var         image  = new ZZZRawImage();
                ErrorNumber opened = image.Open(filter);

                Assert.AreEqual(ErrorNumber.NoError, opened, string.Format(Localization.Open_0, _testFiles[i]));

                if(opened != ErrorNumber.NoError) continue;

                using(new AssertionScope())
                {
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(_sectors[i],
                                        image.Info.Sectors,
                                        string.Format(Localization.Sectors_0, _testFiles[i]));

                        Assert.AreEqual(_sectorSize[i],
                                        image.Info.SectorSize,
                                        string.Format(Localization.Sector_size_0, _testFiles[i]));

                        Assert.AreEqual(_mediaTypes[i],
                                        image.Info.MediaType,
                                        string.Format(Localization.Media_type_0, _testFiles[i]));
                    });
                }
            }
        });
    }
}