// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VirtualPC.cs
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
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images.VirtualPC
{
    [TestFixture]
    public class VirtualPc : BlockMediaImageTest
    {
public override string[] _testFiles => new[]
{/*
"vpc106b_fixed_150mb_fat16.lz",
"vpc213_fixed_50mb_fat16.lz",
"vpc303_fixed_30mb_fat16.lz",
"vpc30_fixed_30mb_fat16.lz",
"vpc4_fixed_130mb_fat16.lz",*/
"vpc504_dynamic_250mb.lz",
"vpc504_fixed_10mb.lz",
"vpc50_dynamic_250mb.lz",
"vpc50_fixed_10mb.lz",
"vpc601_dynamic_250mb.vhd.lz",
"vpc601_fixed_10mb.vhd.lz",
"vpc60_differencing_parent_250mb.vhd.lz",
"vpc60_dynamic_250mb.vhd.lz",
"vpc60_fixed_10mb.vhd.lz",
"vpc702_differencing_parent_250mb.vhd.lz",
"vpc702_dynamic_250mb.vhd.lz",
"vpc702_fixed_10mb.vhd.lz"
};

public override ulong[] _sectors => new ulong[]
{
// vpc106b_fixed_150mb_fat16.lz
//0,
// vpc213_fixed_50mb_fat16.lz
//0,
// vpc303_fixed_30mb_fat16.lz
//0,
// vpc30_fixed_30mb_fat16.lz
//0,
// vpc4_fixed_130mb_fat16.lz
//0,
// vpc504_dynamic_250mb.lz
511056,
// vpc504_fixed_10mb.lz
20468,
// vpc50_dynamic_250mb.lz
511056,
// vpc50_fixed_10mb.lz
20468,
// vpc601_dynamic_250mb.vhd.lz
511056,
// vpc601_fixed_10mb.vhd.lz
20468,
// vpc60_differencing_parent_250mb.vhd.lz
511056,
// vpc60_dynamic_250mb.vhd.lz
511056,
// vpc60_fixed_10mb.vhd.lz
20468,
// vpc702_differencing_parent_250mb.vhd.lz
511056,
// vpc702_dynamic_250mb.vhd.lz
511056,
// vpc702_fixed_10mb.vhd.lz
20468
};

public override uint[] _sectorSize => new uint[]
{
// vpc106b_fixed_150mb_fat16.lz
//512,
// vpc213_fixed_50mb_fat16.lz
//512,
// vpc303_fixed_30mb_fat16.lz
//512,
// vpc30_fixed_30mb_fat16.lz
//512,
// vpc4_fixed_130mb_fat16.lz
//512,
// vpc504_dynamic_250mb.lz
512,
// vpc504_fixed_10mb.lz
512,
// vpc50_dynamic_250mb.lz
512,
// vpc50_fixed_10mb.lz
512,
// vpc601_dynamic_250mb.vhd.lz
512,
// vpc601_fixed_10mb.vhd.lz
512,
// vpc60_differencing_parent_250mb.vhd.lz
512,
// vpc60_dynamic_250mb.vhd.lz
512,
// vpc60_fixed_10mb.vhd.lz
512,
// vpc702_differencing_parent_250mb.vhd.lz
512,
// vpc702_dynamic_250mb.vhd.lz
512,
// vpc702_fixed_10mb.vhd.lz
512
};

public override MediaType[] _mediaTypes => new[]
{
// vpc106b_fixed_150mb_fat16.lz
//MediaType.Unknown,
// vpc213_fixed_50mb_fat16.lz
//MediaType.Unknown,
// vpc303_fixed_30mb_fat16.lz
//MediaType.Unknown,
// vpc30_fixed_30mb_fat16.lz
//MediaType.Unknown,
// vpc4_fixed_130mb_fat16.lz
//MediaType.Unknown,
// vpc504_dynamic_250mb.lz
MediaType.Unknown,
// vpc504_fixed_10mb.lz
MediaType.Unknown,
// vpc50_dynamic_250mb.lz
MediaType.Unknown,
// vpc50_fixed_10mb.lz
MediaType.Unknown,
// vpc601_dynamic_250mb.vhd.lz
MediaType.Unknown,
// vpc601_fixed_10mb.vhd.lz
MediaType.Unknown,
// vpc60_differencing_parent_250mb.vhd.lz
MediaType.Unknown,
// vpc60_dynamic_250mb.vhd.lz
MediaType.Unknown,
// vpc60_fixed_10mb.vhd.lz
MediaType.Unknown,
// vpc702_differencing_parent_250mb.vhd.lz
MediaType.Unknown,
// vpc702_dynamic_250mb.vhd.lz
MediaType.Unknown,
// vpc702_fixed_10mb.vhd.lz
MediaType.Unknown
};

public override string[] _md5S => new[]
{
// vpc106b_fixed_150mb_fat16.lz
//"UNKNOWN",
// vpc213_fixed_50mb_fat16.lz
//"UNKNOWN",
// vpc303_fixed_30mb_fat16.lz
//"UNKNOWN",
// vpc30_fixed_30mb_fat16.lz
//"UNKNOWN",
// vpc4_fixed_130mb_fat16.lz
//"UNKNOWN",
// vpc504_dynamic_250mb.lz
"cbcee980986d980f6add1f9622a5f917",
// vpc504_fixed_10mb.lz
"b790693b1c94bed209ee1bb9d0b6a075",
// vpc50_dynamic_250mb.lz
"c0955193d302f5eae2138a3669c89669",
// vpc50_fixed_10mb.lz
"1c843b778d48a67b78e4ca65ab602673",
// vpc601_dynamic_250mb.vhd.lz
"3e3675037a8ec4b78ebafdc2b25e5ceb",
// vpc601_fixed_10mb.vhd.lz
"4b4e98a5bba2469382132f9289ae1c57",
// vpc60_differencing_parent_250mb.vhd.lz
"3e3675037a8ec4b78ebafdc2b25e5ceb",
// vpc60_dynamic_250mb.vhd.lz
"723b2ed575e0e87f253f672f39b3a49f",
// vpc60_fixed_10mb.vhd.lz
"4b4e98a5bba2469382132f9289ae1c57",
// vpc702_differencing_parent_250mb.vhd.lz
"0f6b4f4bb22f02e88e442638f803e4f4",
// vpc702_dynamic_250mb.vhd.lz
"3e3675037a8ec4b78ebafdc2b25e5ceb",
// vpc702_fixed_10mb.vhd.lz
"4b4e98a5bba2469382132f9289ae1c57"
};

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "VirtualPC");
        public override IMediaImage _plugin =>new Vhd();
   }
}