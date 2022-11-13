// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleNIB.cs
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Tests.Images;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

[TestFixture]
public class AppleNib : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "Nibbles");
    public override IMediaImage Plugin    => new DiscImages.AppleNib();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "dos32.nib.lz",
            MediaType  = MediaType.Apple32SS,
            Sectors    = 455,
            SectorSize = 256,
            Md5        = "76f8fe4c5bc1976f99641ad7cdf53109",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 455
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "dos33.nib.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "0ffcbd4180306192726926b43755db2f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 560
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal.nib.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "4c4926103a32ac15f7e430ec3ced4be5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos.nib.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "11ef56c80c94347d2e3f921d5c36c8de"
        }
    };
}