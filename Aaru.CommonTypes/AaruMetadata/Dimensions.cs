// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dimensions.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Metadata.
//
// --[ Description ] ----------------------------------------------------------
//
//     Defines format for metadata.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

using System;
using Schemas;

namespace Aaru.CommonTypes.AaruMetadata;

public class Dimensions
{
    public double? Diameter  { get; set; }
    public double? Height    { get; set; }
    public double? Width     { get; set; }
    public double  Thickness { get; set; }

    [Obsolete("Will be removed in Aaru 7")]
    public static implicit operator Dimensions(DimensionsType cicm) => cicm is null
                                                                           ? null
                                                                           : new Dimensions
                                                                           {
                                                                               Diameter =
                                                                                   cicm.DiameterSpecified
                                                                                       ? cicm.Diameter
                                                                                       : null,
                                                                               Height = cicm.HeightSpecified
                                                                                   ? cicm.Height
                                                                                   : null,
                                                                               Width = cicm.WidthSpecified
                                                                                   ? cicm.Width
                                                                                   : null,
                                                                               Thickness = cicm.Thickness
                                                                           };

    /// <summary>Gets the physical dimensions, in metadata expected format, for a given media type</summary>
    /// <param name="mediaType">Media type</param>
    /// <returns>Dimensions metadata</returns>
    public static Dimensions FromMediaType(MediaType mediaType)
    {
        var dmns = new Dimensions();

        switch(mediaType)
        {
#region 5.25" floppy disk

            case MediaType.Apple32SS:
            case MediaType.Apple32DS:
            case MediaType.Apple33SS:
            case MediaType.Apple33DS:
            case MediaType.AppleFileWare:
            case MediaType.DOS_525_SS_DD_8:
            case MediaType.DOS_525_SS_DD_9:
            case MediaType.DOS_525_DS_DD_8:
            case MediaType.DOS_525_DS_DD_9:
            case MediaType.DOS_525_HD:
            case MediaType.XDF_525:
            case MediaType.ACORN_525_SS_SD_40:
            case MediaType.ACORN_525_SS_SD_80:
            case MediaType.ACORN_525_SS_DD_40:
            case MediaType.ACORN_525_SS_DD_80:
            case MediaType.ACORN_525_DS_DD:
            case MediaType.ATARI_525_SD:
            case MediaType.ATARI_525_ED:
            case MediaType.ATARI_525_DD:
            case MediaType.CBM_1540:
            case MediaType.CBM_1540_Ext:
            case MediaType.CBM_1571:
            case MediaType.ECMA_66:
            case MediaType.ECMA_70:
            case MediaType.NEC_525_HD:
            case MediaType.ECMA_78:
            case MediaType.ECMA_78_2:
            case MediaType.ECMA_99_8:
            case MediaType.ECMA_99_15:
            case MediaType.ECMA_99_26:
            case MediaType.FDFORMAT_525_DD:
            case MediaType.FDFORMAT_525_HD:
            case MediaType.MetaFloppy_Mod_I:
            case MediaType.MetaFloppy_Mod_II:
                // According to ECMA-99 et al
                dmns.Height = 133.3;

                dmns.Width = 133.3;

                dmns.Thickness = 1.65;

                return dmns;

#endregion 5.25" floppy disk

#region 3.5" floppy disk

            case MediaType.AppleSonySS:
            case MediaType.AppleSonyDS:
            case MediaType.DOS_35_SS_DD_8:
            case MediaType.DOS_35_SS_DD_9:
            case MediaType.DOS_35_DS_DD_8:
            case MediaType.DOS_35_DS_DD_9:
            case MediaType.DOS_35_HD:
            case MediaType.DOS_35_ED:
            case MediaType.DMF:
            case MediaType.DMF_82:
            case MediaType.XDF_35:
            case MediaType.ACORN_35_DS_DD:
            case MediaType.CBM_35_DD:
            case MediaType.CBM_AMIGA_35_DD:
            case MediaType.CBM_AMIGA_35_HD:
            case MediaType.FDFORMAT_35_DD:
            case MediaType.FDFORMAT_35_HD:
            case MediaType.NEC_35_HD_8:
            case MediaType.NEC_35_HD_15:
            case MediaType.Floptical:
            case MediaType.HiFD:
            case MediaType.UHD144:
            case MediaType.Apricot_35:
            case MediaType.FD32MB:
                // According to ECMA-100 et al
                dmns.Height = 94;

                dmns.Width = 90;

                dmns.Thickness = 3.3;

                return dmns;

#endregion 3.5" floppy disk

#region 8" floppy disk

            case MediaType.IBM23FD:
            case MediaType.IBM33FD_128:
            case MediaType.IBM33FD_256:
            case MediaType.IBM33FD_512:
            case MediaType.IBM43FD_128:
            case MediaType.IBM43FD_256:
            case MediaType.IBM53FD_256:
            case MediaType.IBM53FD_512:
            case MediaType.IBM53FD_1024:
            case MediaType.RX01:
            case MediaType.RX02:
            case MediaType.NEC_8_SD:
            case MediaType.NEC_8_DD:
            case MediaType.ECMA_54:
            case MediaType.ECMA_59:
            case MediaType.ECMA_69_8:
            case MediaType.ECMA_69_15:
            case MediaType.ECMA_69_26:
                // According to ECMA-59 et al
                dmns.Height = 203.2;

                dmns.Width = 203.2;

                dmns.Thickness = 1.65;

                return dmns;

#endregion 8" floppy disk

#region 356mm magneto optical

            case MediaType.ECMA_260:
            case MediaType.ECMA_260_Double:
                // According to ECMA-260 et al
                dmns.Height = 421.84;

                dmns.Width = 443.76;

                dmns.Thickness = 25.4;

                return dmns;

#endregion 356mm magneto optical

#region 300mm magneto optical

            case MediaType.ECMA_189:
            case MediaType.ECMA_190:
            case MediaType.ECMA_317:
                // According to ECMA-317 et al
                dmns.Height = 340;

                dmns.Width = 320;

                dmns.Thickness = 17;

                return dmns;

#endregion 300mm magneto optical

#region 5.25" magneto optical

            case MediaType.ECMA_153:
            case MediaType.ECMA_153_512:
            case MediaType.ECMA_183_512:
            case MediaType.ECMA_183:
            case MediaType.ECMA_184_512:
            case MediaType.ECMA_184:
            case MediaType.ECMA_195:
            case MediaType.ECMA_195_512:
            case MediaType.ECMA_238:
            case MediaType.ECMA_280:
            case MediaType.ECMA_322:
            case MediaType.ECMA_322_2k:
            case MediaType.UDO:
            case MediaType.UDO2:
            case MediaType.UDO2_WORM:
            case MediaType.ISO_15286:
            case MediaType.ISO_15286_1024:
            case MediaType.ISO_15286_512:
            case MediaType.ISO_10089:
            case MediaType.ISO_10089_512:
            case MediaType.ECMA_322_1k:
            case MediaType.ECMA_322_512:
            case MediaType.ISO_14517:
            case MediaType.ISO_14517_512:
                // According to ECMA-183 et al
                dmns.Height = 153;

                dmns.Width = 135;

                dmns.Thickness = 11;

                return dmns;

#endregion 5.25" magneto optical

#region 3.5" magneto optical

            case MediaType.ECMA_154:
            case MediaType.ECMA_201:
            case MediaType.ECMA_201_ROM:
            case MediaType.ECMA_223:
            case MediaType.ECMA_223_512:
            case MediaType.GigaMo:
            case MediaType.GigaMo2:
            case MediaType.ISO_15041_512:
                // According to ECMA-154 et al
                dmns.Height = 94;

                dmns.Width = 90;

                dmns.Thickness = 6;

                return dmns;

#endregion 3.5" magneto optical

            case MediaType.PD650:
            case MediaType.PD650_WORM:
                dmns.Height = 135;

                dmns.Width = 124;

                dmns.Thickness = 7.8;

                return dmns;
            case MediaType.ECMA_239:
                dmns.Height = 97;

                dmns.Width = 92;

                dmns.Thickness = 5;

                return dmns;
            case MediaType.MMCmicro:
                dmns.Height = 14;

                dmns.Width = 12;

                dmns.Thickness = 1.1;

                return dmns;
            case MediaType.MemoryStickMicro:
                dmns.Height = 15;

                dmns.Width = 12.5;

                dmns.Thickness = 1.2;

                return dmns;
            case MediaType.microSD:
                dmns.Height = 11;

                dmns.Width = 15;

                dmns.Thickness = 1;

                return dmns;
            case MediaType.miniSD:
                dmns.Height = 21.5;

                dmns.Width = 20;

                dmns.Thickness = 1.4;

                return dmns;
            case MediaType.QIC3010:
            case MediaType.QIC3020:
            case MediaType.QIC3080:
            case MediaType.QIC3095:
            case MediaType.QIC320:
            case MediaType.QIC40:
            case MediaType.QIC80:
                dmns.Height = 20;

                dmns.Width = 21.5;

                dmns.Thickness = 1.6;

                return dmns;
            case MediaType.RSMMC:
                dmns.Height = 18;

                dmns.Width = 24;

                dmns.Thickness = 1.4;

                return dmns;
            case MediaType.MMC:
                dmns.Height = 32;

                dmns.Width = 24;

                dmns.Thickness = 1.4;

                return dmns;
            case MediaType.SecureDigital:
                dmns.Height = 32;

                dmns.Width = 24;

                dmns.Thickness = 2.1;

                return dmns;
            case MediaType.xD:
                dmns.Height = 20;

                dmns.Width = 25;

                dmns.Thickness = 1.78;

                return dmns;
            case MediaType.XQD:
                dmns.Height = 38.5;

                dmns.Width = 29.8;

                dmns.Thickness = 3.8;

                return dmns;
            case MediaType.MemoryStickDuo:
            case MediaType.MemoryStickProDuo:
                dmns.Height = 20;

                dmns.Width = 31;

                dmns.Thickness = 1.6;

                return dmns;
            case MediaType.Nintendo3DSGameCard:
            case MediaType.NintendoDSGameCard:
            case MediaType.NintendoDSiGameCard:
                dmns.Height = 35;

                dmns.Width = 33;

                dmns.Thickness = 3.8;

                return dmns;
            case MediaType.DataPlay:
                dmns.Height = 42;

                dmns.Width = 33.5;

                dmns.Thickness = 3;

                return dmns;
            case MediaType.Microdrive:
                dmns.Height = 44;

                dmns.Width = 34;

                dmns.Thickness = 8;

                return dmns;
            case MediaType.ExpressCard34:
                dmns.Height = 75;

                dmns.Width = 34;

                dmns.Thickness = 5;

                return dmns;
            case MediaType.SmartMedia:
                dmns.Height = 45;

                dmns.Width = 37;

                dmns.Thickness = 0.76;

                return dmns;
            case MediaType.MiniCard:
                dmns.Height = 45;

                dmns.Width = 37;

                dmns.Thickness = 3.5;

                return dmns;
            case MediaType.PlayStationMemoryCard:
            case MediaType.PlayStationMemoryCard2:
                dmns.Height = 55.7;

                dmns.Width = 41.5;

                dmns.Thickness = 7;

                return dmns;
            case MediaType.CFast:
            case MediaType.CompactFlash:
                dmns.Height = 36;

                dmns.Width = 43;

                dmns.Thickness = 3.3;

                return dmns;
            case MediaType.CompactFlashType2:
                dmns.Height = 36;

                dmns.Width = 43;

                dmns.Thickness = 5;

                return dmns;
            case MediaType.ZXMicrodrive:
                dmns.Height = 36;

                dmns.Width = 43;

                dmns.Thickness = 5;

                return dmns;
            case MediaType.MemoryStick:
            case MediaType.MemoryStickPro:
                dmns.Height = 21;

                dmns.Width = 50;

                dmns.Thickness = 2.6;

                return dmns;
            case MediaType.PocketZip:
                dmns.Height = 54.5;

                dmns.Width = 50;

                dmns.Thickness = 2;

                return dmns;
            case MediaType.ExpressCard54:
                dmns.Height = 75;

                dmns.Width = 54;

                dmns.Thickness = 5;

                return dmns;
            case MediaType.PCCardTypeI:
                dmns.Height = 85.6;

                dmns.Width = 54;

                dmns.Thickness = 3.3;

                return dmns;
            case MediaType.PCCardTypeII:
                dmns.Height = 85.6;

                dmns.Width = 54;

                dmns.Thickness = 5;

                return dmns;
            case MediaType.PCCardTypeIII:
                dmns.Height = 85.6;

                dmns.Width = 54;

                dmns.Thickness = 10.5;

                return dmns;
            case MediaType.PCCardTypeIV:
                dmns.Height = 85.6;

                dmns.Width = 54;

                dmns.Thickness = 16;

                return dmns;
            case MediaType.DataStore:
                dmns.Height = 86.5;

                dmns.Width = 54;

                dmns.Thickness = 2.5;

                return dmns;
            case MediaType.VideoFloppy:
                dmns.Height = 54;

                dmns.Width = 60;

                dmns.Thickness = 3.5;

                return dmns;
            case MediaType.VXA1:
            case MediaType.VXA2:
            case MediaType.VXA3:
                dmns.Height = 95;

                dmns.Width = 62.5;

                dmns.Thickness = 15;

                return dmns;
            case MediaType.MiniDV:
                dmns.Height = 47.5;

                dmns.Width = 66;

                dmns.Thickness = 12;

                return dmns;
            case MediaType.Wafer:
                dmns.Height = 46.8;

                dmns.Width = 67.1;

                dmns.Thickness = 7.9;

                return dmns;
            case MediaType.NintendoDiskCard:
                dmns.Height = 76.2;

                dmns.Width = 71.12;

                dmns.Thickness = 0;

                return dmns;
            case MediaType.HiMD:
            case MediaType.MD:
            case MediaType.MDData:
            case MediaType.MDData2:
            case MediaType.MD60:
            case MediaType.MD74:
            case MediaType.MD80:
                dmns.Height = 68;

                dmns.Width = 71.5;

                dmns.Thickness = 4.8;

                return dmns;
            case MediaType.DAT160:
            case MediaType.DAT320:
            case MediaType.DAT72:
            case MediaType.DDS1:
            case MediaType.DDS2:
            case MediaType.DDS3:
            case MediaType.DDS4:
            case MediaType.DigitalAudioTape:
                dmns.Height = 54;

                dmns.Width = 73;

                dmns.Thickness = 10.5;

                return dmns;
            case MediaType.CompactFloppy:
                dmns.Height = 100;

                dmns.Width = 80;

                dmns.Thickness = 5;

                return dmns;
            case MediaType.DECtapeII:
                dmns.Height = 60;

                dmns.Width = 81;

                dmns.Thickness = 13;

                return dmns;
            case MediaType.Ditto:
                dmns.Height = 60;

                dmns.Width = 81;

                dmns.Thickness = 14;

                return dmns;
            case MediaType.DittoMax:
                dmns.Height = 126;

                dmns.Width = 81;

                dmns.Thickness = 14;

                return dmns;
            case MediaType.RDX:
            case MediaType.RDX320:
                dmns.Height = 119;

                dmns.Width = 87;

                dmns.Thickness = 23;

                return dmns;
            case MediaType.LS120:
            case MediaType.LS240:
                dmns.Height = 94;

                dmns.Width = 90;

                dmns.Thickness = 3.5;

                return dmns;
            case MediaType.Travan:
            case MediaType.Travan3:
            case MediaType.Travan4:
            case MediaType.Travan5:
            case MediaType.Travan7:
                dmns.Height = 72;

                dmns.Width = 92;

                dmns.Thickness = 15;

                return dmns;
            case MediaType.Travan1Ex:
                dmns.Height = 0;

                dmns.Width = 92;

                dmns.Thickness = 15;

                return dmns;
            case MediaType.Travan3Ex:
                dmns.Height = 0;

                dmns.Width = 92;

                dmns.Thickness = 15;

                return dmns;
            case MediaType.ADR2120:
            case MediaType.ADR260:
            case MediaType.ADR30:
            case MediaType.ADR50:
                dmns.Height = 129;

                dmns.Width = 93;

                dmns.Thickness = 14.5;

                return dmns;
            case MediaType.Data8:
            case MediaType.AIT1:
            case MediaType.AIT1Turbo:
            case MediaType.AIT2:
            case MediaType.AIT2Turbo:
            case MediaType.AIT3:
            case MediaType.AIT3Ex:
            case MediaType.AIT3Turbo:
            case MediaType.AIT4:
            case MediaType.AIT5:
            case MediaType.AITETurbo:
            case MediaType.Exatape106m:
            case MediaType.Exatape160mXL:
            case MediaType.Exatape112m:
            case MediaType.Exatape125m:
            case MediaType.Exatape150m:
            case MediaType.Exatape15m:
            case MediaType.Exatape170m:
            case MediaType.Exatape225m:
            case MediaType.Exatape22m:
            case MediaType.Exatape22mAME:
            case MediaType.Exatape28m:
            case MediaType.Exatape40m:
            case MediaType.Exatape45m:
            case MediaType.Exatape54m:
            case MediaType.Exatape75m:
            case MediaType.Exatape76m:
            case MediaType.Exatape80m:
                dmns.Height = 62.5;

                dmns.Width = 95;

                dmns.Thickness = 15;

                return dmns;
            case MediaType.EZ135:
            case MediaType.EZ230:
            case MediaType.SQ327:
                dmns.Height = 97;

                dmns.Width = 98;

                dmns.Thickness = 9.5;

                return dmns;
            case MediaType.SQ400:
            case MediaType.SQ800:
            case MediaType.SQ2000:
                dmns.Height = 137;

                dmns.Width = 137;

                dmns.Thickness = 12;

                return dmns;
            case MediaType.ZIP100:
            case MediaType.ZIP250:
            case MediaType.ZIP750:
                dmns.Height = 98.5;

                dmns.Width = 98;

                dmns.Thickness = 6.5;

                return dmns;
            case MediaType.Jaz:
            case MediaType.Jaz2:
                dmns.Height = 102;

                dmns.Width = 98;

                dmns.Thickness = 12;

                return dmns;
            case MediaType.Orb:
            case MediaType.Orb5:
                dmns.Height = 104;

                dmns.Width = 98;

                dmns.Thickness = 8;

                return dmns;
            case MediaType.SparQ:
                dmns.Height = 98;

                dmns.Width = 100;

                dmns.Thickness = 9.7;

                return dmns;
            case MediaType.ProfessionalDisc:
            case MediaType.ProfessionalDiscDual:
            case MediaType.ProfessionalDiscTriple:
            case MediaType.ProfessionalDiscQuad:
            case MediaType.PDD:
            case MediaType.PDD_WORM:
                dmns.Height = 130;

                dmns.Width = 128.5;

                dmns.Thickness = 9;

                return dmns;
            case MediaType.SLR1:
            case MediaType.SLR2:
            case MediaType.SLR3:
            case MediaType.SLR32:
            case MediaType.SLR32SL:
            case MediaType.SLR4:
            case MediaType.SLR5:
            case MediaType.SLR5SL:
            case MediaType.SLR6:
            case MediaType.SLRtape100:
            case MediaType.SLRtape140:
            case MediaType.SLRtape24:
            case MediaType.SLRtape24SL:
            case MediaType.SLRtape40:
            case MediaType.SLRtape50:
            case MediaType.SLRtape60:
            case MediaType.SLRtape7:
            case MediaType.SLRtape75:
            case MediaType.SLRtape7SL:
                dmns.Height = 150;

                dmns.Width = 100;

                dmns.Thickness = 18;

                return dmns;
            case MediaType.N64DD:
                dmns.Height = 103.124;

                dmns.Width = 101.092;

                dmns.Thickness = 10.16;

                return dmns;
            case MediaType.CompactTapeI:
            case MediaType.CompactTapeII:
            case MediaType.DLTtapeIII:
            case MediaType.DLTtapeIIIxt:
            case MediaType.DLTtapeIV:
            case MediaType.DLTtapeS4:
            case MediaType.SDLT1:
            case MediaType.SDLT2:
            case MediaType.VStapeI:
                dmns.Height = 105;

                dmns.Width = 105;

                dmns.Thickness = 25;

                return dmns;
            case MediaType.LTO:
            case MediaType.LTO2:
            case MediaType.LTO3:
            case MediaType.LTO3WORM:
            case MediaType.LTO4:
            case MediaType.LTO4WORM:
            case MediaType.LTO5:
            case MediaType.LTO5WORM:
            case MediaType.LTO6:
            case MediaType.LTO6WORM:
            case MediaType.LTO7:
            case MediaType.LTO7WORM:
                dmns.Height = 101.6;

                dmns.Width = 105.41;

                dmns.Thickness = 21.59;

                return dmns;
            case MediaType.IBM3480:
            case MediaType.IBM3490:
            case MediaType.IBM3490E:
            case MediaType.IBM3592:
                dmns.Height = 125.73;

                dmns.Width = 107.95;

                dmns.Thickness = 25.4;

                return dmns;
            case MediaType.T9840A:
            case MediaType.T9840B:
            case MediaType.T9840C:
            case MediaType.T9840D:
            case MediaType.T9940A:
            case MediaType.T9940B:
                dmns.Height = 124.46;

                dmns.Width = 109.22;

                dmns.Thickness = 25.4;

                return dmns;
            case MediaType.CompactCassette:
            case MediaType.Dcas25:
            case MediaType.Dcas85:
            case MediaType.Dcas103:
                dmns.Height = 63.5;

                dmns.Width = 128;

                dmns.Thickness = 12;

                return dmns;
            case MediaType.IBM3470:
                dmns.Height = 58.42;

                dmns.Width = 137.16;

                dmns.Thickness = 16.51;

                return dmns;
            case MediaType.MLR1:
            case MediaType.MLR3:
            case MediaType.MLR1SL:
                dmns.Height = 101.6;

                dmns.Width = 152.4;

                dmns.Thickness = 15.24;

                return dmns;
            case MediaType.QIC11:
            case MediaType.QIC120:
            case MediaType.QIC1350:
            case MediaType.QIC150:
            case MediaType.QIC24:
            case MediaType.QIC525:
                dmns.Height = 101.6;

                dmns.Width = 154.2;

                dmns.Thickness = 16.6;

                return dmns;
#pragma warning disable CS0612 // Type or member is obsolete
            case MediaType.Bernoulli:
#pragma warning restore CS0612 // Type or member is obsolete
            case MediaType.Bernoulli10:
            case MediaType.Bernoulli20:
                dmns.Height = 280;

                dmns.Width = 209;

                dmns.Thickness = 18;

                return dmns;
#pragma warning disable CS0612 // Type or member is obsolete
            case MediaType.Bernoulli2:
#pragma warning restore CS0612 // Type or member is obsolete
            case MediaType.BernoulliBox2_20:
            case MediaType.Bernoulli35:
            case MediaType.Bernoulli44:
            case MediaType.Bernoulli65:
            case MediaType.Bernoulli90:
            case MediaType.Bernoulli105:
            case MediaType.Bernoulli150:
            case MediaType.Bernoulli230:
                dmns.Height = 138;

                dmns.Width = 136;

                dmns.Thickness = 9;

                return dmns;
            case MediaType.DTF:
            case MediaType.DTF2:
                dmns.Height = 144.78;

                dmns.Width = 254;

                dmns.Thickness = 25.4;

                return dmns;
            case MediaType.LD:
            case MediaType.LDROM:
            case MediaType.LDROM2:
            case MediaType.MegaLD:
            case MediaType.LVROM:
                dmns.Diameter = 300;

                dmns.Thickness = 2.5;

                return dmns;
            case MediaType.CRVdisc:
                dmns.Height = 344;

                dmns.Width = 325;

                dmns.Thickness = 15.6;

                return dmns;
            case MediaType.REV35:
            case MediaType.REV70:
            case MediaType.REV120:
                dmns.Height = 74.8;

                dmns.Width = 76.8;

                dmns.Thickness = 10;

                return dmns;

#region CD/DVD/BD

            case MediaType.CDDA:
            case MediaType.CDG:
            case MediaType.CDEG:
            case MediaType.CDI:
            case MediaType.CDROM:
            case MediaType.CDROMXA:
            case MediaType.CDPLUS:
            case MediaType.CDMO:
            case MediaType.CDR:
            case MediaType.CDRW:
            case MediaType.CDMRW:
            case MediaType.VCD:
            case MediaType.SVCD:
            case MediaType.PCD:
            case MediaType.SACD:
            case MediaType.DDCD:
            case MediaType.DDCDR:
            case MediaType.DDCDRW:
            case MediaType.DTSCD:
            case MediaType.CDMIDI:
            case MediaType.CDV:
            case MediaType.CD:
            case MediaType.DVDROM:
            case MediaType.DVDR:
            case MediaType.DVDRW:
            case MediaType.DVDPR:
            case MediaType.DVDPRW:
            case MediaType.DVDPRWDL:
            case MediaType.DVDRDL:
            case MediaType.DVDPRDL:
            case MediaType.DVDRAM:
            case MediaType.DVDRWDL:
            case MediaType.DVDDownload:
            case MediaType.HDDVDROM:
            case MediaType.HDDVDRAM:
            case MediaType.HDDVDR:
            case MediaType.HDDVDRW:
            case MediaType.HDDVDRDL:
            case MediaType.HDDVDRWDL:
            case MediaType.BDROM:
            case MediaType.BDR:
            case MediaType.BDRE:
            case MediaType.BDRXL:
            case MediaType.BDREXL:
            case MediaType.PS1CD:
            case MediaType.PS2CD:
            case MediaType.PS2DVD:
            case MediaType.PS3DVD:
            case MediaType.PS3BD:
            case MediaType.PS4BD:
            case MediaType.PS5BD:
            case MediaType.XGD:
            case MediaType.XGD2:
            case MediaType.XGD3:
            case MediaType.XGD4:
            case MediaType.MEGACD:
            case MediaType.SATURNCD:
            case MediaType.GDROM:
            case MediaType.GDR:
            case MediaType.SuperCDROM2:
            case MediaType.JaguarCD:
            case MediaType.ThreeDO:
            case MediaType.WOD:
            case MediaType.WUOD:
            case MediaType.PCFX:
            case MediaType.CDIREADY:
            case MediaType.FMTOWNS:
            case MediaType.CDTV:
            case MediaType.CD32:
            case MediaType.Nuon:
            case MediaType.Playdia:
            case MediaType.Pippin:
            case MediaType.MilCD:
            case MediaType.CVD:
            case MediaType.UHDBD:
                dmns.Diameter = 120;

                dmns.Thickness = 1.2;

                return dmns;
            case MediaType.GOD:
                dmns.Diameter = 80;

                dmns.Thickness = 1.2;

                return dmns;
            case MediaType.VideoNow:
                dmns.Diameter = 85;

                dmns.Thickness = 1.2;

                return dmns;
            case MediaType.VideoNowColor:
            case MediaType.VideoNowXp:
                dmns.Diameter = 108;

                dmns.Thickness = 1.2;

                return dmns;

#endregion CD/DVD/BD

#region Apple Hard Disks

            // TODO: Find Apple Widget size
            case MediaType.AppleProfile:
                dmns.Height = 223.8;

                dmns.Width = 438.9;

                dmns.Thickness = 111.5;

                return dmns;
            case MediaType.AppleHD20:
                dmns.Height = 246.4;

                dmns.Width = 266.7;

                dmns.Thickness = 78.7;

                return dmns;

#endregion Apple Hard Disks

            case MediaType.UMD:
                dmns.Height = 64;

                dmns.Width = 63;

                dmns.Thickness = 4;

                return dmns;
            case MediaType.HF12:
            case MediaType.HF24:
                dmns.Height = 137.5;

                dmns.Width = 135.9;

                dmns.Thickness = 5.64;

                return dmns;
            default:
                return null;
        }
    }
}