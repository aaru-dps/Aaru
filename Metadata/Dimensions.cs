// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dimensions.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : XML metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Gets physical dimensions of a device/media based on its media type.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using Schemas;

#pragma warning disable 612

namespace Aaru.CommonTypes.Metadata
{
    /// <summary>Physical dimensions for media types</summary>
    public static class Dimensions
    {
        /// <summary>Gets the physical dimensions, in metadata expected format, for a given media type</summary>
        /// <param name="dskType">Media type</param>
        /// <returns>Dimensions metadata</returns>
        public static DimensionsType DimensionsFromMediaType(CommonTypes.MediaType dskType)
        {
            var dmns = new DimensionsType();

            switch(dskType)
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
                case CommonTypes.MediaType.CBM_1540_Ext:
                case CommonTypes.MediaType.CBM_1571:
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
                case CommonTypes.MediaType.MetaFloppy_Mod_I:
                case CommonTypes.MediaType.MetaFloppy_Mod_II:
                    // According to ECMA-99 et al
                    dmns.Height          = 133.3;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 133.3;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1.65;

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
                case CommonTypes.MediaType.FDFORMAT_35_DD:
                case CommonTypes.MediaType.FDFORMAT_35_HD:
                case CommonTypes.MediaType.NEC_35_HD_8:
                case CommonTypes.MediaType.NEC_35_HD_15:
                case CommonTypes.MediaType.Floptical:
                case CommonTypes.MediaType.HiFD:
                case CommonTypes.MediaType.UHD144:
                case CommonTypes.MediaType.Apricot_35:
                case CommonTypes.MediaType.FD32MB:
                    // According to ECMA-100 et al
                    dmns.Height          = 94;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 90;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 3.3;

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
                    dmns.Height          = 203.2;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 203.2;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1.65;

                    return dmns;
                #endregion 8" floppy disk

                #region 356mm magneto optical
                case CommonTypes.MediaType.ECMA_260:
                case CommonTypes.MediaType.ECMA_260_Double:
                    // According to ECMA-260 et al
                    dmns.Height          = 421.84;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 443.76;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 25.4;

                    return dmns;
                #endregion 356mm magneto optical

                #region 300mm magneto optical
                case CommonTypes.MediaType.ECMA_189:
                case CommonTypes.MediaType.ECMA_190:
                case CommonTypes.MediaType.ECMA_317:
                    // According to ECMA-317 et al
                    dmns.Height          = 340;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 320;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 17;

                    return dmns;
                #endregion 300mm magneto optical

                #region 5.25" magneto optical
                case CommonTypes.MediaType.ECMA_153:
                case CommonTypes.MediaType.ECMA_153_512:
                case CommonTypes.MediaType.ECMA_183_512:
                case CommonTypes.MediaType.ECMA_183:
                case CommonTypes.MediaType.ECMA_184_512:
                case CommonTypes.MediaType.ECMA_184:
                case CommonTypes.MediaType.ECMA_195:
                case CommonTypes.MediaType.ECMA_195_512:
                case CommonTypes.MediaType.ECMA_238:
                case CommonTypes.MediaType.ECMA_280:
                case CommonTypes.MediaType.ECMA_322:
                case CommonTypes.MediaType.ECMA_322_2k:
                case CommonTypes.MediaType.UDO:
                case CommonTypes.MediaType.UDO2:
                case CommonTypes.MediaType.UDO2_WORM:
                case CommonTypes.MediaType.ISO_15286:
                case CommonTypes.MediaType.ISO_15286_1024:
                case CommonTypes.MediaType.ISO_15286_512:
                case CommonTypes.MediaType.ISO_10089:
                case CommonTypes.MediaType.ISO_10089_512:
                case CommonTypes.MediaType.ECMA_322_1k:
                case CommonTypes.MediaType.ECMA_322_512:
                case CommonTypes.MediaType.ISO_14517:
                case CommonTypes.MediaType.ISO_14517_512:
                    // According to ECMA-183 et al
                    dmns.Height          = 153;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 135;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 11;

                    return dmns;
                #endregion 5.25" magneto optical

                #region 3.5" magneto optical
                case CommonTypes.MediaType.ECMA_154:
                case CommonTypes.MediaType.ECMA_201:
                case CommonTypes.MediaType.ECMA_201_ROM:
                case CommonTypes.MediaType.ECMA_223:
                case CommonTypes.MediaType.ECMA_223_512:
                case CommonTypes.MediaType.GigaMo:
                case CommonTypes.MediaType.GigaMo2:
                case CommonTypes.MediaType.ISO_15041_512:
                    // According to ECMA-154 et al
                    dmns.Height          = 94;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 90;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 6;

                    return dmns;
                #endregion 3.5" magneto optical

                case CommonTypes.MediaType.PD650:
                case CommonTypes.MediaType.PD650_WORM:
                    dmns.Height          = 135;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 124;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 7.8;

                    return dmns;
                case CommonTypes.MediaType.ECMA_239:
                    dmns.Height          = 97;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 92;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 5;

                    return dmns;
                case CommonTypes.MediaType.MMCmicro:
                    dmns.Height          = 14;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 12;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1.1;

                    return dmns;
                case CommonTypes.MediaType.MemoryStickMicro:
                    dmns.Height          = 15;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 12.5;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1.2;

                    return dmns;
                case CommonTypes.MediaType.microSD:
                    dmns.Height          = 11;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 15;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1;

                    return dmns;
                case CommonTypes.MediaType.miniSD:
                    dmns.Height          = 21.5;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 20;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1.4;

                    return dmns;
                case CommonTypes.MediaType.QIC3010:
                case CommonTypes.MediaType.QIC3020:
                case CommonTypes.MediaType.QIC3080:
                case CommonTypes.MediaType.QIC3095:
                case CommonTypes.MediaType.QIC320:
                case CommonTypes.MediaType.QIC40:
                case CommonTypes.MediaType.QIC80:
                    dmns.Height          = 20;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 21.5;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1.6;

                    return dmns;
                case CommonTypes.MediaType.RSMMC:
                    dmns.Height          = 18;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 24;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1.4;

                    return dmns;
                case CommonTypes.MediaType.MMC:
                    dmns.Height          = 32;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 24;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1.4;

                    return dmns;
                case CommonTypes.MediaType.SecureDigital:
                    dmns.Height          = 32;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 24;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 2.1;

                    return dmns;
                case CommonTypes.MediaType.xD:
                    dmns.Height          = 20;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 25;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1.78;

                    return dmns;
                case CommonTypes.MediaType.XQD:
                    dmns.Height          = 38.5;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 29.8;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 3.8;

                    return dmns;
                case CommonTypes.MediaType.MemoryStickDuo:
                case CommonTypes.MediaType.MemoryStickProDuo:
                    dmns.Height          = 20;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 31;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 1.6;

                    return dmns;
                case CommonTypes.MediaType.Nintendo3DSGameCard:
                case CommonTypes.MediaType.NintendoDSGameCard:
                case CommonTypes.MediaType.NintendoDSiGameCard:
                    dmns.Height          = 35;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 33;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 3.8;

                    return dmns;
                case CommonTypes.MediaType.DataPlay:
                    dmns.Height          = 42;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 33.5;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 3;

                    return dmns;
                case CommonTypes.MediaType.Microdrive:
                    dmns.Height          = 44;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 34;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 8;

                    return dmns;
                case CommonTypes.MediaType.ExpressCard34:
                    dmns.Height          = 75;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 34;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 5;

                    return dmns;
                case CommonTypes.MediaType.SmartMedia:
                    dmns.Height          = 45;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 37;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 0.76;

                    return dmns;
                case CommonTypes.MediaType.MiniCard:
                    dmns.Height          = 45;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 37;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 3.5;

                    return dmns;
                case CommonTypes.MediaType.PlayStationMemoryCard:
                case CommonTypes.MediaType.PlayStationMemoryCard2:
                    dmns.Height          = 55.7;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 41.5;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 7;

                    return dmns;
                case CommonTypes.MediaType.CFast:
                case CommonTypes.MediaType.CompactFlash:
                    dmns.Height          = 36;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 43;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 3.3;

                    return dmns;
                case CommonTypes.MediaType.CompactFlashType2:
                    dmns.Height          = 36;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 43;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 5;

                    return dmns;
                case CommonTypes.MediaType.ZXMicrodrive:
                    dmns.Height          = 36;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 43;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 5;

                    return dmns;
                case CommonTypes.MediaType.MemoryStick:
                case CommonTypes.MediaType.MemoryStickPro:
                    dmns.Height          = 21;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 50;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 2.6;

                    return dmns;
                case CommonTypes.MediaType.PocketZip:
                    dmns.Height          = 54.5;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 50;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 2;

                    return dmns;
                case CommonTypes.MediaType.ExpressCard54:
                    dmns.Height          = 75;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 54;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 5;

                    return dmns;
                case CommonTypes.MediaType.PCCardTypeI:
                    dmns.Height          = 85.6;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 54;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 3.3;

                    return dmns;
                case CommonTypes.MediaType.PCCardTypeII:
                    dmns.Height          = 85.6;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 54;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 5;

                    return dmns;
                case CommonTypes.MediaType.PCCardTypeIII:
                    dmns.Height          = 85.6;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 54;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 10.5;

                    return dmns;
                case CommonTypes.MediaType.PCCardTypeIV:
                    dmns.Height          = 85.6;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 54;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 16;

                    return dmns;
                case CommonTypes.MediaType.DataStore:
                    dmns.Height          = 86.5;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 54;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 2.5;

                    return dmns;
                case CommonTypes.MediaType.VideoFloppy:
                    dmns.Height          = 54;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 60;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 3.5;

                    return dmns;
                case CommonTypes.MediaType.VXA1:
                case CommonTypes.MediaType.VXA2:
                case CommonTypes.MediaType.VXA3:
                    dmns.Height          = 95;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 62.5;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 15;

                    return dmns;
                case CommonTypes.MediaType.MiniDV:
                    dmns.Height          = 47.5;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 66;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 12;

                    return dmns;
                case CommonTypes.MediaType.Wafer:
                    dmns.Height          = 46.8;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 67.1;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 7.9;

                    return dmns;
                case CommonTypes.MediaType.NintendoDiskCard:
                    dmns.Height          = 76.2;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 71.12;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 0;

                    return dmns;
                case CommonTypes.MediaType.HiMD:
                case CommonTypes.MediaType.MD:
                case CommonTypes.MediaType.MDData:
                case CommonTypes.MediaType.MDData2:
                case CommonTypes.MediaType.MD60:
                case CommonTypes.MediaType.MD74:
                case CommonTypes.MediaType.MD80:
                    dmns.Height          = 68;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 71.5;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 4.8;

                    return dmns;
                case CommonTypes.MediaType.DAT160:
                case CommonTypes.MediaType.DAT320:
                case CommonTypes.MediaType.DAT72:
                case CommonTypes.MediaType.DDS1:
                case CommonTypes.MediaType.DDS2:
                case CommonTypes.MediaType.DDS3:
                case CommonTypes.MediaType.DDS4:
                case CommonTypes.MediaType.DigitalAudioTape:
                    dmns.Height          = 54;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 73;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 10.5;

                    return dmns;
                case CommonTypes.MediaType.CompactFloppy:
                    dmns.Height          = 100;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 80;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 5;

                    return dmns;
                case CommonTypes.MediaType.DECtapeII:
                    dmns.Height          = 60;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 81;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 13;

                    return dmns;
                case CommonTypes.MediaType.Ditto:
                    dmns.Height          = 60;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 81;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 14;

                    return dmns;
                case CommonTypes.MediaType.DittoMax:
                    dmns.Height          = 126;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 81;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 14;

                    return dmns;
                case CommonTypes.MediaType.RDX:
                case CommonTypes.MediaType.RDX320:
                    dmns.Height          = 119;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 87;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 23;

                    return dmns;
                case CommonTypes.MediaType.LS120:
                case CommonTypes.MediaType.LS240:
                    dmns.Height          = 94;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 90;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 3.5;

                    return dmns;
                case CommonTypes.MediaType.Travan:
                case CommonTypes.MediaType.Travan3:
                case CommonTypes.MediaType.Travan4:
                case CommonTypes.MediaType.Travan5:
                case CommonTypes.MediaType.Travan7:
                    dmns.Height          = 72;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 92;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 15;

                    return dmns;
                case CommonTypes.MediaType.Travan1Ex:
                    dmns.Height          = 0;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 92;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 15;

                    return dmns;
                case CommonTypes.MediaType.Travan3Ex:
                    dmns.Height          = 0;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 92;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 15;

                    return dmns;
                case CommonTypes.MediaType.ADR2120:
                case CommonTypes.MediaType.ADR260:
                case CommonTypes.MediaType.ADR30:
                case CommonTypes.MediaType.ADR50:
                    dmns.Height          = 129;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 93;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 14.5;

                    return dmns;
                case CommonTypes.MediaType.Data8:
                case CommonTypes.MediaType.AIT1:
                case CommonTypes.MediaType.AIT1Turbo:
                case CommonTypes.MediaType.AIT2:
                case CommonTypes.MediaType.AIT2Turbo:
                case CommonTypes.MediaType.AIT3:
                case CommonTypes.MediaType.AIT3Ex:
                case CommonTypes.MediaType.AIT3Turbo:
                case CommonTypes.MediaType.AIT4:
                case CommonTypes.MediaType.AIT5:
                case CommonTypes.MediaType.AITETurbo:
                case CommonTypes.MediaType.Exatape106m:
                case CommonTypes.MediaType.Exatape160mXL:
                case CommonTypes.MediaType.Exatape112m:
                case CommonTypes.MediaType.Exatape125m:
                case CommonTypes.MediaType.Exatape150m:
                case CommonTypes.MediaType.Exatape15m:
                case CommonTypes.MediaType.Exatape170m:
                case CommonTypes.MediaType.Exatape225m:
                case CommonTypes.MediaType.Exatape22m:
                case CommonTypes.MediaType.Exatape22mAME:
                case CommonTypes.MediaType.Exatape28m:
                case CommonTypes.MediaType.Exatape40m:
                case CommonTypes.MediaType.Exatape45m:
                case CommonTypes.MediaType.Exatape54m:
                case CommonTypes.MediaType.Exatape75m:
                case CommonTypes.MediaType.Exatape76m:
                case CommonTypes.MediaType.Exatape80m:
                    dmns.Height          = 62.5;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 95;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 15;

                    return dmns;
                case CommonTypes.MediaType.EZ135:
                case CommonTypes.MediaType.EZ230:
                case CommonTypes.MediaType.SQ327:
                    dmns.Height          = 97;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 98;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 9.5;

                    return dmns;
                case CommonTypes.MediaType.SQ400:
                case CommonTypes.MediaType.SQ800:
                case CommonTypes.MediaType.SQ2000:
                    dmns.Height          = 137;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 137;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 12;

                    return dmns;
                case CommonTypes.MediaType.ZIP100:
                case CommonTypes.MediaType.ZIP250:
                case CommonTypes.MediaType.ZIP750:
                    dmns.Height          = 98.5;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 98;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 6.5;

                    return dmns;
                case CommonTypes.MediaType.Jaz:
                case CommonTypes.MediaType.Jaz2:
                    dmns.Height          = 102;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 98;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 12;

                    return dmns;
                case CommonTypes.MediaType.Orb:
                case CommonTypes.MediaType.Orb5:
                    dmns.Height          = 104;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 98;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 8;

                    return dmns;
                case CommonTypes.MediaType.SparQ:
                    dmns.Height          = 98;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 100;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 9.7;

                    return dmns;
                case CommonTypes.MediaType.ProfessionalDisc:
                case CommonTypes.MediaType.ProfessionalDiscDual:
                case CommonTypes.MediaType.ProfessionalDiscTriple:
                case CommonTypes.MediaType.ProfessionalDiscQuad:
                case CommonTypes.MediaType.PDD:
                case CommonTypes.MediaType.PDD_WORM:
                    dmns.Height          = 130;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 128.5;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 9;

                    return dmns;
                case CommonTypes.MediaType.SLR1:
                case CommonTypes.MediaType.SLR2:
                case CommonTypes.MediaType.SLR3:
                case CommonTypes.MediaType.SLR32:
                case CommonTypes.MediaType.SLR32SL:
                case CommonTypes.MediaType.SLR4:
                case CommonTypes.MediaType.SLR5:
                case CommonTypes.MediaType.SLR5SL:
                case CommonTypes.MediaType.SLR6:
                case CommonTypes.MediaType.SLRtape100:
                case CommonTypes.MediaType.SLRtape140:
                case CommonTypes.MediaType.SLRtape24:
                case CommonTypes.MediaType.SLRtape24SL:
                case CommonTypes.MediaType.SLRtape40:
                case CommonTypes.MediaType.SLRtape50:
                case CommonTypes.MediaType.SLRtape60:
                case CommonTypes.MediaType.SLRtape7:
                case CommonTypes.MediaType.SLRtape75:
                case CommonTypes.MediaType.SLRtape7SL:
                    dmns.Height          = 150;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 100;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 18;

                    return dmns;
                case CommonTypes.MediaType.N64DD:
                    dmns.Height          = 103.124;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 101.092;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 10.16;

                    return dmns;
                case CommonTypes.MediaType.CompactTapeI:
                case CommonTypes.MediaType.CompactTapeII:
                case CommonTypes.MediaType.DLTtapeIII:
                case CommonTypes.MediaType.DLTtapeIIIxt:
                case CommonTypes.MediaType.DLTtapeIV:
                case CommonTypes.MediaType.DLTtapeS4:
                case CommonTypes.MediaType.SDLT1:
                case CommonTypes.MediaType.SDLT2:
                case CommonTypes.MediaType.VStapeI:
                    dmns.Height          = 105;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 105;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 25;

                    return dmns;
                case CommonTypes.MediaType.LTO:
                case CommonTypes.MediaType.LTO2:
                case CommonTypes.MediaType.LTO3:
                case CommonTypes.MediaType.LTO3WORM:
                case CommonTypes.MediaType.LTO4:
                case CommonTypes.MediaType.LTO4WORM:
                case CommonTypes.MediaType.LTO5:
                case CommonTypes.MediaType.LTO5WORM:
                case CommonTypes.MediaType.LTO6:
                case CommonTypes.MediaType.LTO6WORM:
                case CommonTypes.MediaType.LTO7:
                case CommonTypes.MediaType.LTO7WORM:
                    dmns.Height          = 101.6;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 105.41;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 21.59;

                    return dmns;
                case CommonTypes.MediaType.IBM3480:
                case CommonTypes.MediaType.IBM3490:
                case CommonTypes.MediaType.IBM3490E:
                case CommonTypes.MediaType.IBM3592:
                    dmns.Height          = 125.73;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 107.95;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 25.4;

                    return dmns;
                case CommonTypes.MediaType.T9840A:
                case CommonTypes.MediaType.T9840B:
                case CommonTypes.MediaType.T9840C:
                case CommonTypes.MediaType.T9840D:
                case CommonTypes.MediaType.T9940A:
                case CommonTypes.MediaType.T9940B:
                    dmns.Height          = 124.46;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 109.22;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 25.4;

                    return dmns;
                case CommonTypes.MediaType.CompactCassette:
                case CommonTypes.MediaType.Dcas25:
                case CommonTypes.MediaType.Dcas85:
                case CommonTypes.MediaType.Dcas103:
                    dmns.Height          = 63.5;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 128;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 12;

                    return dmns;
                case CommonTypes.MediaType.IBM3470:
                    dmns.Height          = 58.42;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 137.16;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 16.51;

                    return dmns;
                case CommonTypes.MediaType.MLR1:
                case CommonTypes.MediaType.MLR3:
                case CommonTypes.MediaType.MLR1SL:
                    dmns.Height          = 101.6;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 152.4;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 15.24;

                    return dmns;
                case CommonTypes.MediaType.QIC11:
                case CommonTypes.MediaType.QIC120:
                case CommonTypes.MediaType.QIC1350:
                case CommonTypes.MediaType.QIC150:
                case CommonTypes.MediaType.QIC24:
                case CommonTypes.MediaType.QIC525:
                    dmns.Height          = 101.6;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 154.2;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 16.6;

                    return dmns;
                case CommonTypes.MediaType.Bernoulli:
                case CommonTypes.MediaType.Bernoulli10:
                case CommonTypes.MediaType.Bernoulli20:
                    dmns.Height          = 280;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 209;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 18;

                    return dmns;
                case CommonTypes.MediaType.Bernoulli2:
                case CommonTypes.MediaType.BernoulliBox2_20:
                case CommonTypes.MediaType.Bernoulli35:
                case CommonTypes.MediaType.Bernoulli44:
                case CommonTypes.MediaType.Bernoulli65:
                case CommonTypes.MediaType.Bernoulli90:
                case CommonTypes.MediaType.Bernoulli105:
                case CommonTypes.MediaType.Bernoulli150:
                case CommonTypes.MediaType.Bernoulli230:
                    dmns.Height          = 138;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 136;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 9;

                    return dmns;
                case CommonTypes.MediaType.DTF:
                case CommonTypes.MediaType.DTF2:
                    dmns.Height          = 144.78;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 254;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 25.4;

                    return dmns;
                case CommonTypes.MediaType.LD:
                case CommonTypes.MediaType.LDROM:
                case CommonTypes.MediaType.LDROM2:
                case CommonTypes.MediaType.MegaLD:
                case CommonTypes.MediaType.LVROM:
                    dmns.Diameter          = 300;
                    dmns.DiameterSpecified = true;
                    dmns.Thickness         = 2.5;

                    return dmns;
                case CommonTypes.MediaType.CRVdisc:
                    dmns.Height          = 344;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 325;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 15.6;

                    return dmns;
                case CommonTypes.MediaType.REV35:
                case CommonTypes.MediaType.REV70:
                case CommonTypes.MediaType.REV120:
                    dmns.Height          = 74.8;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 76.8;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 10;

                    return dmns;

                #region CD/DVD/BD
                case CommonTypes.MediaType.CDDA:
                case CommonTypes.MediaType.CDG:
                case CommonTypes.MediaType.CDEG:
                case CommonTypes.MediaType.CDI:
                case CommonTypes.MediaType.CDROM:
                case CommonTypes.MediaType.CDROMXA:
                case CommonTypes.MediaType.CDPLUS:
                case CommonTypes.MediaType.CDMO:
                case CommonTypes.MediaType.CDR:
                case CommonTypes.MediaType.CDRW:
                case CommonTypes.MediaType.CDMRW:
                case CommonTypes.MediaType.VCD:
                case CommonTypes.MediaType.SVCD:
                case CommonTypes.MediaType.PCD:
                case CommonTypes.MediaType.SACD:
                case CommonTypes.MediaType.DDCD:
                case CommonTypes.MediaType.DDCDR:
                case CommonTypes.MediaType.DDCDRW:
                case CommonTypes.MediaType.DTSCD:
                case CommonTypes.MediaType.CDMIDI:
                case CommonTypes.MediaType.CDV:
                case CommonTypes.MediaType.CD:
                case CommonTypes.MediaType.DVDROM:
                case CommonTypes.MediaType.DVDR:
                case CommonTypes.MediaType.DVDRW:
                case CommonTypes.MediaType.DVDPR:
                case CommonTypes.MediaType.DVDPRW:
                case CommonTypes.MediaType.DVDPRWDL:
                case CommonTypes.MediaType.DVDRDL:
                case CommonTypes.MediaType.DVDPRDL:
                case CommonTypes.MediaType.DVDRAM:
                case CommonTypes.MediaType.DVDRWDL:
                case CommonTypes.MediaType.DVDDownload:
                case CommonTypes.MediaType.HDDVDROM:
                case CommonTypes.MediaType.HDDVDRAM:
                case CommonTypes.MediaType.HDDVDR:
                case CommonTypes.MediaType.HDDVDRW:
                case CommonTypes.MediaType.HDDVDRDL:
                case CommonTypes.MediaType.HDDVDRWDL:
                case CommonTypes.MediaType.BDROM:
                case CommonTypes.MediaType.BDR:
                case CommonTypes.MediaType.BDRE:
                case CommonTypes.MediaType.BDRXL:
                case CommonTypes.MediaType.BDREXL:
                case CommonTypes.MediaType.PS1CD:
                case CommonTypes.MediaType.PS2CD:
                case CommonTypes.MediaType.PS2DVD:
                case CommonTypes.MediaType.PS3DVD:
                case CommonTypes.MediaType.PS3BD:
                case CommonTypes.MediaType.PS4BD:
                case CommonTypes.MediaType.PS5BD:
                case CommonTypes.MediaType.XGD:
                case CommonTypes.MediaType.XGD2:
                case CommonTypes.MediaType.XGD3:
                case CommonTypes.MediaType.XGD4:
                case CommonTypes.MediaType.MEGACD:
                case CommonTypes.MediaType.SATURNCD:
                case CommonTypes.MediaType.GDROM:
                case CommonTypes.MediaType.GDR:
                case CommonTypes.MediaType.SuperCDROM2:
                case CommonTypes.MediaType.JaguarCD:
                case CommonTypes.MediaType.ThreeDO:
                case CommonTypes.MediaType.WOD:
                case CommonTypes.MediaType.WUOD:
                case CommonTypes.MediaType.PCFX:
                case CommonTypes.MediaType.CDIREADY:
                case CommonTypes.MediaType.FMTOWNS:
                case CommonTypes.MediaType.CDTV:
                case CommonTypes.MediaType.CD32:
                case CommonTypes.MediaType.Nuon:
                case CommonTypes.MediaType.Playdia:
                case CommonTypes.MediaType.Pippin:
                case CommonTypes.MediaType.MilCD:
                case CommonTypes.MediaType.CVD:
                case CommonTypes.MediaType.UHDBD:
                    dmns.Diameter          = 120;
                    dmns.DiameterSpecified = true;
                    dmns.Thickness         = 1.2;

                    return dmns;
                case CommonTypes.MediaType.GOD:
                    dmns.Diameter          = 80;
                    dmns.DiameterSpecified = true;
                    dmns.Thickness         = 1.2;

                    return dmns;
                case CommonTypes.MediaType.VideoNow:
                    dmns.Diameter          = 85;
                    dmns.DiameterSpecified = true;
                    dmns.Thickness         = 1.2;

                    return dmns;
                case CommonTypes.MediaType.VideoNowColor:
                case CommonTypes.MediaType.VideoNowXp:
                    dmns.Diameter          = 108;
                    dmns.DiameterSpecified = true;
                    dmns.Thickness         = 1.2;

                    return dmns;
                #endregion CD/DVD/BD

                #region Apple Hard Disks
                // TODO: Find Apple Widget size
                case CommonTypes.MediaType.AppleProfile:
                    dmns.Height          = 223.8;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 438.9;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 111.5;

                    return dmns;
                case CommonTypes.MediaType.AppleHD20:
                    dmns.Height          = 246.4;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 266.7;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 78.7;

                    return dmns;
                #endregion Apple Hard Disks

                case CommonTypes.MediaType.UMD:
                    dmns.Height          = 64;
                    dmns.HeightSpecified = true;
                    dmns.Width           = 63;
                    dmns.WidthSpecified  = true;
                    dmns.Thickness       = 4;

                    return dmns;
                default: return null;
            }
        }
    }
}