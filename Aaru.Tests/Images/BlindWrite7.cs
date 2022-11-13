// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BlindWrite6.cs
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
public class BlindWrite7 : OpticalMediaImageTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "BlindWrite 7");
    public override IMediaImage Plugin => new DiscImages.BlindWrite5();

    public override OpticalImageTestExpected[] Tests => new[]
    {
        new OpticalImageTestExpected
        {
            TestFile   = "report_cdr.B6T",
            MediaType  = MediaType.CDR,
            Sectors    = 254265,
            SectorSize = 2048,
            Md5        = "86b8a763ef6522fccf97f743d7bf4fa3",
            LongMd5    = "a292359cce05849dec1d06ae471ecf9e",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 254264,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 63562,
                            ClusterSize = 8192,
                            Type        = "HFS",
                            VolumeName  = "Winpower"
                        },
                        new FileSystemTest
                        {
                            Clusters    = 254265,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "Winpower"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile   = "report_cdrom.B6T",
            MediaType  = MediaType.CDROM,
            Sectors    = 254265,
            SectorSize = 2048,
            Md5        = "bf4bbec517101d0d6f45d2e4d50cb875",
            LongMd5    = "3d3f9cf7d1ba2249b1e7960071e5af46",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 254264,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Clusters    = 63562,
                            ClusterSize = 8192,
                            Type        = "HFS",
                            VolumeName  = "Winpower"
                        },
                        new FileSystemTest
                        {
                            Clusters    = 254265,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "Winpower"
                        }
                    }
                }
            }
        },
        new OpticalImageTestExpected
        {
            TestFile   = "report_cdrw.B6T",
            MediaType  = MediaType.CDRW,
            Sectors    = 308224,
            SectorSize = 2048,
            Md5        = "1e55aa420ca8f8ea77d5b597c9cfc19b",
            LongMd5    = "3af5f943ddb9427d9c63a4ce3b704db9",
            Tracks = new[]
            {
                new TrackInfoTestExpected
                {
                    Session = 1,
                    Start   = 0,
                    End     = 308223,
                    Pregap  = 150,
                    Flags   = 4,
                    Number  = 1,
                    FileSystems = new[]
                    {
                        new FileSystemTest
                        {
                            Bootable    = true,
                            Clusters    = 308224,
                            ClusterSize = 2048,
                            Type        = "ISO9660",
                            VolumeName  = "ARCH_201901"
                        }
                    }
                }
            }
        }
    };
}