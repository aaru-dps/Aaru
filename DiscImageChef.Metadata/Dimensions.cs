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
using Schemas;

namespace DiscImageChef.Metadata
{
    public static class Dimensions
    {
        public static DimensionsType DimensionsFromMediaType(CommonTypes.MediaType dskType)
        {
            DimensionsType dmns = new DimensionsType();

            switch (dskType)
            {
                #region 5.25" floppy disk
                case CommonTypes.MediaType.Apple32SS:
                case CommonTypes.MediaType.Apple32DS:
                case CommonTypes.MediaType.Apple33SS:
                case CommonTypes.MediaType.Apple33DS:
                case CommonTypes.MediaType.AppleFileWare:
                case CommonTypes.MediaType.DOS_525_SS_DD_8:
                case CommonTypes.MediaType.DOS_525_SS_DD_9:
                case CommonTypes.MediaType.DOS_525_DS_DD_8:
                case CommonTypes.MediaType.DOS_525_DS_DD_9:
                case CommonTypes.MediaType.DOS_525_HD:
                case CommonTypes.MediaType.XDF_525:
                case CommonTypes.MediaType.ACORN_525_SS_SD_40:
                case CommonTypes.MediaType.ACORN_525_SS_SD_80:
                case CommonTypes.MediaType.ACORN_525_SS_DD_40:
                case CommonTypes.MediaType.ACORN_525_SS_DD_80:
                case CommonTypes.MediaType.ACORN_525_DS_DD:
                case CommonTypes.MediaType.ATARI_525_SD:
                case CommonTypes.MediaType.ATARI_525_ED:
                case CommonTypes.MediaType.ATARI_525_DD:
                case CommonTypes.MediaType.CBM_1540:
                case CommonTypes.MediaType.ECMA_66:
                case CommonTypes.MediaType.ECMA_70:
                case CommonTypes.MediaType.NEC_525_HD:
                case CommonTypes.MediaType.ECMA_78:
                case CommonTypes.MediaType.ECMA_78_2:
                case CommonTypes.MediaType.ECMA_99_8:
                case CommonTypes.MediaType.ECMA_99_15:
                case CommonTypes.MediaType.ECMA_99_26:
                case CommonTypes.MediaType.FDFORMAT_525_DD:
                case CommonTypes.MediaType.FDFORMAT_525_HD:
                case CommonTypes.MediaType.SHARP_525:
                    // According to ECMA-99 et al
                    dmns.Height = 133.3;
                    dmns.HeightSpecified = true;
                    dmns.Width = 133.3;
                    dmns.WidthSpecified = true;
                    dmns.Thickness = 1.65;
                    return dmns;
                #endregion 5.25" floppy disk

                #region 3.5" floppy disk
                case CommonTypes.MediaType.AppleSonySS:
                case CommonTypes.MediaType.AppleSonyDS:
                case CommonTypes.MediaType.DOS_35_SS_DD_8:
                case CommonTypes.MediaType.DOS_35_SS_DD_9:
                case CommonTypes.MediaType.DOS_35_DS_DD_8:
                case CommonTypes.MediaType.DOS_35_DS_DD_9:
                case CommonTypes.MediaType.DOS_35_HD:
                case CommonTypes.MediaType.DOS_35_ED:
                case CommonTypes.MediaType.DMF:
                case CommonTypes.MediaType.DMF_82:
                case CommonTypes.MediaType.XDF_35:
                case CommonTypes.MediaType.ACORN_35_DS_DD:
                case CommonTypes.MediaType.CBM_35_DD:
                case CommonTypes.MediaType.CBM_AMIGA_35_DD:
                case CommonTypes.MediaType.CBM_AMIGA_35_HD:
                case CommonTypes.MediaType.ECMA_100:
                case CommonTypes.MediaType.ECMA_125:
                case CommonTypes.MediaType.ECMA_147:
                case CommonTypes.MediaType.FDFORMAT_35_DD:
                case CommonTypes.MediaType.FDFORMAT_35_HD:
                case CommonTypes.MediaType.NEC_35_HD_8:
                case CommonTypes.MediaType.NEC_35_HD_15:
                case CommonTypes.MediaType.SHARP_35:
                    // According to ECMA-100 et al
                    dmns.Height = 94;
                    dmns.HeightSpecified = true;
                    dmns.Width = 90;
                    dmns.WidthSpecified = true;
                    dmns.Thickness = 3.3;
                    return dmns;
                #endregion 3.5" floppy disk

                #region 8" floppy disk
                case CommonTypes.MediaType.IBM23FD:
                case CommonTypes.MediaType.IBM33FD_128:
                case CommonTypes.MediaType.IBM33FD_256:
                case CommonTypes.MediaType.IBM33FD_512:
                case CommonTypes.MediaType.IBM43FD_128:
                case CommonTypes.MediaType.IBM43FD_256:
                case CommonTypes.MediaType.IBM53FD_256:
                case CommonTypes.MediaType.IBM53FD_512:
                case CommonTypes.MediaType.IBM53FD_1024:
                case CommonTypes.MediaType.RX01:
                case CommonTypes.MediaType.RX02:
                case CommonTypes.MediaType.NEC_8_SD:
                case CommonTypes.MediaType.NEC_8_DD:
                case CommonTypes.MediaType.ECMA_54:
                case CommonTypes.MediaType.ECMA_59:
                case CommonTypes.MediaType.ECMA_69_8:
                case CommonTypes.MediaType.ECMA_69_15:
                case CommonTypes.MediaType.ECMA_69_26:
                    // According to ECMA-59 et al
                    dmns.Height = 203.2;
                    dmns.HeightSpecified = true;
                    dmns.Width = 203.2;
                    dmns.WidthSpecified = true;
                    dmns.Thickness = 1.65;
                    return dmns;
                #endregion 8" floppy disk

                #region 5.25" magneto optical
                case CommonTypes.MediaType.ECMA_183_512:
                case CommonTypes.MediaType.ECMA_183_1024:
                case CommonTypes.MediaType.ECMA_184_512:
                case CommonTypes.MediaType.ECMA_184_1024:
                    // According to ECMA-183 et al
                    dmns.Height = 153;
                    dmns.HeightSpecified = true;
                    dmns.Width = 135;
                    dmns.WidthSpecified = true;
                    dmns.Thickness = 11;
                    return dmns;
                #endregion 5.25" magneto optical

                #region 3.5" magneto optical
                case CommonTypes.MediaType.ECMA_154:
                case CommonTypes.MediaType.ECMA_201:
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

