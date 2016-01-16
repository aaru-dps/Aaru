// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Dimensions.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using Schemas;

namespace DiscImageChef.Metadata
{
    public static class Dimensions
    {
        public static DimensionsType DimensionsFromDiskType(CommonTypes.DiskType dskType)
        {
            DimensionsType dmns = new DimensionsType();

            switch (dskType)
            {
                #region 5.25" floppy disk
                case DiscImageChef.CommonTypes.DiskType.Apple32SS:
                case DiscImageChef.CommonTypes.DiskType.Apple32DS:
                case DiscImageChef.CommonTypes.DiskType.Apple33SS:
                case DiscImageChef.CommonTypes.DiskType.Apple33DS:
                case DiscImageChef.CommonTypes.DiskType.AppleFileWare:
                case DiscImageChef.CommonTypes.DiskType.DOS_525_SS_DD_8:
                case DiscImageChef.CommonTypes.DiskType.DOS_525_SS_DD_9:
                case DiscImageChef.CommonTypes.DiskType.DOS_525_DS_DD_8:
                case DiscImageChef.CommonTypes.DiskType.DOS_525_DS_DD_9:
                case DiscImageChef.CommonTypes.DiskType.DOS_525_HD:
                case DiscImageChef.CommonTypes.DiskType.XDF_525:
                case DiscImageChef.CommonTypes.DiskType.ACORN_525_SS_SD_40:
                case DiscImageChef.CommonTypes.DiskType.ACORN_525_SS_SD_80:
                case DiscImageChef.CommonTypes.DiskType.ACORN_525_SS_DD_40:
                case DiscImageChef.CommonTypes.DiskType.ACORN_525_SS_DD_80:
                case DiscImageChef.CommonTypes.DiskType.ACORN_525_DS_DD:
                case DiscImageChef.CommonTypes.DiskType.ATARI_525_SD:
                case DiscImageChef.CommonTypes.DiskType.ATARI_525_ED:
                case DiscImageChef.CommonTypes.DiskType.ATARI_525_DD:
                case DiscImageChef.CommonTypes.DiskType.CBM_1540:
                case DiscImageChef.CommonTypes.DiskType.ECMA_66:
                case DiscImageChef.CommonTypes.DiskType.ECMA_70:
                case DiscImageChef.CommonTypes.DiskType.NEC_525_HD:
                case DiscImageChef.CommonTypes.DiskType.ECMA_78:
                case DiscImageChef.CommonTypes.DiskType.ECMA_78_2:
                case DiscImageChef.CommonTypes.DiskType.ECMA_99_8:
                case DiscImageChef.CommonTypes.DiskType.ECMA_99_15:
                case DiscImageChef.CommonTypes.DiskType.ECMA_99_26:
                case DiscImageChef.CommonTypes.DiskType.FDFORMAT_525_DD:
                case DiscImageChef.CommonTypes.DiskType.FDFORMAT_525_HD:
                case DiscImageChef.CommonTypes.DiskType.SHARP_525:
                    // According to ECMA-99 et al
                    dmns.Height = 133.3;
                    dmns.HeightSpecified = true;
                    dmns.Width = 133.3;
                    dmns.WidthSpecified = true;
                    dmns.Thickness = 1.65;
                    return dmns;
                #endregion 5.25" floppy disk

                #region 3.5" floppy disk
                case DiscImageChef.CommonTypes.DiskType.AppleSonySS:
                case DiscImageChef.CommonTypes.DiskType.AppleSonyDS:
                case DiscImageChef.CommonTypes.DiskType.DOS_35_SS_DD_8:
                case DiscImageChef.CommonTypes.DiskType.DOS_35_SS_DD_9:
                case DiscImageChef.CommonTypes.DiskType.DOS_35_DS_DD_8:
                case DiscImageChef.CommonTypes.DiskType.DOS_35_DS_DD_9:
                case DiscImageChef.CommonTypes.DiskType.DOS_35_HD:
                case DiscImageChef.CommonTypes.DiskType.DOS_35_ED:
                case DiscImageChef.CommonTypes.DiskType.DMF:
                case DiscImageChef.CommonTypes.DiskType.DMF_82:
                case DiscImageChef.CommonTypes.DiskType.XDF_35:
                case DiscImageChef.CommonTypes.DiskType.ACORN_35_DS_DD:
                case DiscImageChef.CommonTypes.DiskType.CBM_35_DD:
                case DiscImageChef.CommonTypes.DiskType.CBM_AMIGA_35_DD:
                case DiscImageChef.CommonTypes.DiskType.CBM_AMIGA_35_HD:
                case DiscImageChef.CommonTypes.DiskType.ECMA_100:
                case DiscImageChef.CommonTypes.DiskType.ECMA_125:
                case DiscImageChef.CommonTypes.DiskType.ECMA_147:
                case DiscImageChef.CommonTypes.DiskType.FDFORMAT_35_DD:
                case DiscImageChef.CommonTypes.DiskType.FDFORMAT_35_HD:
                case DiscImageChef.CommonTypes.DiskType.NEC_35_HD_8:
                case DiscImageChef.CommonTypes.DiskType.NEC_35_HD_15:
                case DiscImageChef.CommonTypes.DiskType.SHARP_35:
                    // According to ECMA-100 et al
                    dmns.Height = 94;
                    dmns.HeightSpecified = true;
                    dmns.Width = 90;
                    dmns.WidthSpecified = true;
                    dmns.Thickness = 3.3;
                    return dmns;
                #endregion 3.5" floppy disk

                #region 8" floppy disk
                case DiscImageChef.CommonTypes.DiskType.IBM23FD:
                case DiscImageChef.CommonTypes.DiskType.IBM33FD_128:
                case DiscImageChef.CommonTypes.DiskType.IBM33FD_256:
                case DiscImageChef.CommonTypes.DiskType.IBM33FD_512:
                case DiscImageChef.CommonTypes.DiskType.IBM43FD_128:
                case DiscImageChef.CommonTypes.DiskType.IBM43FD_256:
                case DiscImageChef.CommonTypes.DiskType.IBM53FD_256:
                case DiscImageChef.CommonTypes.DiskType.IBM53FD_512:
                case DiscImageChef.CommonTypes.DiskType.IBM53FD_1024:
                case DiscImageChef.CommonTypes.DiskType.RX01:
                case DiscImageChef.CommonTypes.DiskType.RX02:
                case DiscImageChef.CommonTypes.DiskType.NEC_8_SD:
                case DiscImageChef.CommonTypes.DiskType.NEC_8_DD:
                case DiscImageChef.CommonTypes.DiskType.ECMA_54:
                case DiscImageChef.CommonTypes.DiskType.ECMA_59:
                case DiscImageChef.CommonTypes.DiskType.ECMA_69_8:
                case DiscImageChef.CommonTypes.DiskType.ECMA_69_15:
                case DiscImageChef.CommonTypes.DiskType.ECMA_69_26:
                    // According to ECMA-59 et al
                    dmns.Height = 203.2;
                    dmns.HeightSpecified = true;
                    dmns.Width = 203.2;
                    dmns.WidthSpecified = true;
                    dmns.Thickness = 1.65;
                    return dmns;
                #endregion 8" floppy disk

                #region 5.25" magneto optical
                case DiscImageChef.CommonTypes.DiskType.ECMA_183_512:
                case DiscImageChef.CommonTypes.DiskType.ECMA_183_1024:
                case DiscImageChef.CommonTypes.DiskType.ECMA_184_512:
                case DiscImageChef.CommonTypes.DiskType.ECMA_184_1024:
                    // According to ECMA-183 et al
                    dmns.Height = 153;
                    dmns.HeightSpecified = true;
                    dmns.Width = 135;
                    dmns.WidthSpecified = true;
                    dmns.Thickness = 11;
                    return dmns;
                #endregion 5.25" magneto optical

                #region 3.5" magneto optical
                case DiscImageChef.CommonTypes.DiskType.ECMA_154:
                case DiscImageChef.CommonTypes.DiskType.ECMA_201:
                    // According to ECMA-154 et al
                    dmns.Height = 94;
                    dmns.HeightSpecified = true;
                    dmns.Width = 90;
                    dmns.WidthSpecified = true;
                    dmns.Thickness = 6;
                    return dmns;
                #endregion 3.5" magneto optical

                default:
                    return null;
            }
        }
    }
}

