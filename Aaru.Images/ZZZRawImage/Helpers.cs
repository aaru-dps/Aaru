// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains helpers for raw image, that is, user data sector by sector copy.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes;

namespace Aaru.DiscImages;

public sealed partial class ZZZRawImage
{
    MediaType CalculateDiskType()
    {
        if(_rawDvd)
        {
            // TODO: Add all types
            return MediaType.DVDROM;
        }
        
        if(_imageInfo.SectorSize == 2048)
            return _imageInfo.Sectors switch
            {
                58620544    => MediaType.REV120,
                17090880    => MediaType.REV35,
                <= 360000   => MediaType.CD,
                <= 2295104  => MediaType.DVDPR,
                <= 2298496  => MediaType.DVDR,
                <= 4171712  => MediaType.DVDRDL,
                <= 4173824  => MediaType.DVDPRDL,
                <= 24438784 => MediaType.BDR,
                _           => _imageInfo.Sectors <= 62500864 ? MediaType.BDRXL : MediaType.Unknown
            };

        switch(_imageInfo.ImageSize)
        {
            case 80384:  return MediaType.ECMA_66;
            case 81664:  return MediaType.IBM23FD;
            case 92160:  return MediaType.ATARI_525_SD;
            case 102400: return MediaType.ACORN_525_SS_SD_40;
            case 116480: return MediaType.Apple32SS;
            case 133120: return MediaType.ATARI_525_ED;
            case 143360: return MediaType.Apple33SS;
            case 163840: return _imageInfo.SectorSize == 256 ? MediaType.ACORN_525_SS_DD_40 : MediaType.DOS_525_SS_DD_8;

            case 184320: return MediaType.DOS_525_SS_DD_9;
            case 204800: return MediaType.ACORN_525_SS_SD_80;
            case 232960: return MediaType.Apple32DS;
            case 242944: return MediaType.IBM33FD_128;
            case 256256: return MediaType.ECMA_54;
            case 286720: return MediaType.Apple33DS;
            case 287488: return MediaType.IBM33FD_256;
            case 306432: return MediaType.IBM33FD_512;
            case 315392: return MediaType.MetaFloppy_Mod_II;
            case 322560: return MediaType.Apricot_35;
            case 325632: return MediaType.ECMA_70;
            case 327680: return _imageInfo.SectorSize == 256 ? MediaType.ACORN_525_SS_DD_80 : MediaType.DOS_525_DS_DD_8;

            case 368640: return _extension == ".st" ? MediaType.DOS_35_SS_DD_9 : MediaType.DOS_525_DS_DD_9;

            case 409600: return _extension == ".st" ? MediaType.ATARI_35_SS_DD : MediaType.AppleSonySS;

            case 450560: return MediaType.ATARI_35_SS_DD_11;
            case 495872: return MediaType.IBM43FD_128;
            case 512512: return MediaType.ECMA_59;
            case 653312: return MediaType.ECMA_78;
            case 655360: return MediaType.ACORN_525_DS_DD;
            case 737280: return MediaType.DOS_35_DS_DD_9;
            case 819200:
                if(_imageInfo.SectorSize == 256)
                    return MediaType.CBM_35_DD;

                return _extension switch
                {
                    ".adf" or ".adl" when _imageInfo.SectorSize == 1024 => MediaType.ACORN_35_DS_DD,
                    ".st"                                               => MediaType.ATARI_35_DS_DD,
                    _                                                   => MediaType.AppleSonyDS
                };

            case 839680: return MediaType.FDFORMAT_35_DD;
            case 901120: return _extension == ".st" ? MediaType.ATARI_35_DS_DD_11 : MediaType.CBM_AMIGA_35_DD;

            case 988416:                                      return MediaType.IBM43FD_256;
            case 995072:                                      return MediaType.IBM53FD_256;
            case 1021696:                                     return MediaType.ECMA_99_26;
            case 1146624:                                     return MediaType.IBM53FD_512;
            case 1177344:                                     return MediaType.ECMA_99_15;
            case 1222400:                                     return MediaType.IBM53FD_1024;
            case 1228800:                                     return MediaType.DOS_525_HD;
            case 1255168:                                     return MediaType.ECMA_69_8;
            case 1261568:                                     return MediaType.NEC_525_HD;
            case 1304320:                                     return MediaType.ECMA_99_8;
            case 1427456:                                     return MediaType.FDFORMAT_525_HD;
            case 1474560:                                     return MediaType.DOS_35_HD;
            case 1638400:                                     return MediaType.ACORN_35_DS_HD;
            case 1720320:                                     return MediaType.DMF;
            case 1763328:                                     return MediaType.FDFORMAT_35_HD;
            case 1802240:                                     return MediaType.CBM_AMIGA_35_HD;
            case 1880064:                                     return MediaType.XDF_35;
            case 1884160:                                     return MediaType.XDF_35;
            case 2949120:                                     return MediaType.DOS_35_ED;
            case 9338880:                                     return MediaType.NEC_35_TD;
            case 20818944:                                    return MediaType.Floptical;
            case 33554432:                                    return MediaType.FD32MB;
            case 40387584:                                    return MediaType.PocketZip;
            case 100663296:                                   return MediaType.ZIP100;
            case 126222336:                                   return MediaType.LS120;
            case 127398912:                                   return MediaType.ECMA_154;
            case 201410560:                                   return MediaType.HiFD;
            case 228518400:                                   return MediaType.ECMA_201;
            case 240386048:                                   return MediaType.LS240;
            case 250640384:                                   return MediaType.ZIP250;
            case 481520640:                                   return MediaType.ECMA_183_512;
            case 533403648:                                   return MediaType.ECMA_183;
            case 596787200:                                   return MediaType.ECMA_184_512;
            case 654540800:                                   return MediaType.ECMA_184;
            case 656310272 when _imageInfo.SectorSize == 512: return MediaType.PD650;
            case 664829952 when _imageInfo.SectorSize == 512: return MediaType.PD650;
            case 1070617600:                                  return MediaType.Jaz;

            #region Commodore
            case 174848:
            case 175531: return MediaType.CBM_1540;
            case 196608:
            case 197376: return MediaType.CBM_1540_Ext;
            case 349696:
            case 351062: return MediaType.CBM_1571;
            #endregion Commodore

            default: return MediaType.GENERIC_HDD;
        }
    }
}