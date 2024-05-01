// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FromSbc.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru common types.
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

using System;
using Aaru.Console;

namespace Aaru.CommonTypes;

public static partial class MediaTypeFromDevice
{
    /// <summary>Gets the media type from a SCSI Block Commands compliant device</summary>
    /// <param name="vendor">Vendor string</param>
    /// <param name="model">Model string</param>
    /// <param name="mediumType">Medium type from MODE SENSE</param>
    /// <param name="blocks">Number of blocks in device</param>
    /// <param name="blockSize">Size of a block in bytes</param>
    /// <returns>Media type</returns>
    static MediaType GetFromSbc(string vendor, string model, byte mediumType, ulong blocks, uint blockSize)
    {
        switch(mediumType)
        {
            case 0x09:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_Media_Type_Description_ECMA54,
                                           mediumType,
                                           blocks,
                                           blockSize);

                return MediaType.ECMA_54;
            case 0x0A:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_Media_Type_Description_ECMA59,
                                           mediumType,
                                           blocks,
                                           blockSize);

                return MediaType.ECMA_59;
            case 0x0B:
                switch(blockSize)
                {
                    case 256:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA69,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_69_26;
                    case 512:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA69,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_69_15;
                    case 1024:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA69,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_69_8;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_Media_Type_Description_Unknown,
                                           mediumType,
                                           blocks,
                                           blockSize);

                return MediaType.Unknown;
            case 0x0E:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_Media_Type_Description_ECMA66,
                                           mediumType,
                                           blocks,
                                           blockSize);

                return MediaType.ECMA_66;
            case 0x12:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_Media_Type_Description_ECMA70,
                                           mediumType,
                                           blocks,
                                           blockSize);

                return MediaType.ECMA_70;
            case 0x16:
                switch(blockSize)
                {
                    case 256:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA78,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_78;
                    case 512:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA78,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_78_2;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_Media_Type_Description_Unknown,
                                           mediumType,
                                           blocks,
                                           blockSize);

                return MediaType.Unknown;
            case 0x1A:
                switch(blockSize)
                {
                    case 256:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA99,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_99_26;
                    case 512:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA99,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_99_15;
                    case 1024:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA99,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_99_8;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_Media_Type_Description_Unknown,
                                           mediumType,
                                           blocks,
                                           blockSize);

                return MediaType.Unknown;
            case 0x1E:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_Media_Type_Description_IBM_MF2DD,
                                           mediumType,
                                           blocks,
                                           blockSize);

                return MediaType.DOS_35_DS_DD_9;
            case 0x41:
                switch(blocks)
                {
                    case 58620544:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_REV120,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.REV120;
                    case 34185728:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_REV70,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.REV70;
                    case 17090880:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_REV35,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.REV35;
                }

                break;
            case 0x93:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_Media_Type_Description_PC98_MF2HD,
                                           mediumType,
                                           blocks,
                                           blockSize);

                return MediaType.NEC_35_HD_15;
            case 0x94:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_Media_Type_Description_IBM_MF2HD,
                                           mediumType,
                                           blocks,
                                           blockSize);

                return MediaType.DOS_35_HD;
        }

        switch(blockSize)
        {
            case 128:
                switch(blocks)
                {
                    case 720:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Atari_MD1SD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ATARI_525_SD;
                    case 1040:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Atari_MD1DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ATARI_525_DD;
                    case 1898:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_33FD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.IBM33FD_128;
                    case 2002:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA54,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_54;
                    case 3848:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_43FD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.IBM43FD_128;
                    case 4004:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA59,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_59;
                }

                break;
            case 256:
                switch(blocks)
                {
                    case 322:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA56,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_66;
                    case 400:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Acorn_MD1SD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ACORN_525_SS_SD_40;
                    case 455:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Apple_DOS32,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.Apple32SS;
                    case 560:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Apple_DOS33,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.Apple33SS;
                    case 640:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Acorn_MD1DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ACORN_525_SS_DD_40;
                    case 720:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Atari_MD1DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ATARI_525_DD;
                    case 800:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Acorn_MD1DD_80,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ACORN_525_SS_SD_80;
                    case 910:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Apple_DOS32_DS,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.Apple32DS;
                    case 1120:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Apple_DOS33_DS,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.Apple33DS;
                    case 1121:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_33FD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.IBM33FD_256;
                    case 1232:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_MetaFloppy,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.MetaFloppy_Mod_II;
                    case 1280 when mediumType == 0x01:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Acorn_MD1DD_80,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ACORN_525_SS_DD_80;
                    case 1280:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA70,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_70;
                    case 2002:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_DEC_RX02,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.RX02;
                    case 2560:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA78,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_78;
                    case 3848:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_53FD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.IBM53FD_256;
                    case 4004:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA99,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_99_26;
                    case 39168 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                    case 41004 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Bernoulli10,
                                                   blocks);

                        return MediaType.Bernoulli10;
                    case 46956:
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_HF12);

                        return MediaType.HF12;
                    case 78936:
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_HF24);

                        return MediaType.HF12;
                }

                break;
            case 319:
                switch(blocks)
                {
                    case 256:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_23FD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.IBM23FD;
                }

                break;
            case 512:
                switch(blocks)
                {
                    case 320:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MD1DD_8,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_525_SS_DD_8;
                    case 360:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MD1DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_525_SS_DD_9;
                    case 610:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_33FD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.IBM33FD_512;
                    case 630 when mediumType == 0x01:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Apricot_MF2DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.Apricot_35;
                    case 640 when mediumType == 0x01:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MF1DD_8,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_35_SS_DD_8;
                    case 640:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MD2DD_8,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_525_DS_DD_8;
                    case 720 when mediumType == 0x01:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MF1DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_35_SS_DD_9;
                    case 720:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MD2DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_525_DS_DD_9;
                    case 800:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Apple_MF1DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.AppleSonySS;
                    case 1280:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MF2DD_8,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_35_DS_DD_8;
                    case 1440:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MF2DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_35_DS_DD_9;
                    case 1640:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_FDFORMAT_MF2DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.FDFORMAT_35_DD;
                    case 1760:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Amiga_MF2DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.CBM_AMIGA_35_DD;
                    case 2242:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_53FD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.IBM53FD_512;
                    case 2332:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA99,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_99_15;
                    case 2400:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MD2HD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_525_HD;
                    case 2788:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_FDFORMAT_MD2HD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.FDFORMAT_525_HD;
                    case 2880:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MF2HD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_35_HD;
                    case 3360:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_DMF_MF2HD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DMF;
                    case 3444:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_FDFORMAT_MF2HD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.FDFORMAT_35_HD;
                    case 3520:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Amiga_MF2HD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.CBM_AMIGA_35_HD;
                    case 5760:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_MF2ED,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.DOS_35_ED;
                    case 40662 when mediumType == 0x20:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Floptical,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.Floptical;
                    case 65536 when model.ToLowerInvariant().StartsWith("ls-", StringComparison.Ordinal):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_FD32MB);

                        return MediaType.FD32MB;
                    case 78882 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_PocketZIP);

                        return MediaType.PocketZip;
                    case 86700 when vendor.Equals("syquest", StringComparison.InvariantCultureIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_SQ400);

                        return MediaType.SQ400;
                    case 87040 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_Bernoulli2_44);

                        return MediaType.Bernoulli44;
                    case 173456 when vendor.Equals("syquest", StringComparison.InvariantCultureIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_SQ800);

                        return MediaType.SQ800;
                    case 175856 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_Bernoulli2_90);

                        return MediaType.Bernoulli90;
                    case 196608 when model.ToLowerInvariant().StartsWith("zip", StringComparison.OrdinalIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_ZIP100);

                        return MediaType.ZIP100;

                    case 215440 when vendor.Equals("syquest", StringComparison.InvariantCultureIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_SQ310);

                        return MediaType.SQ310;
                    case 246528 when model.ToLowerInvariant().StartsWith("ls-", StringComparison.Ordinal):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_LS120);

                        return MediaType.LS120;
                    case 248826 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA154,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_154;
                    case 262144 when vendor.Equals("syquest", StringComparison.InvariantCultureIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_EZ135);

                        return MediaType.EZ135;
                    case 294918 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Bernoulli2_150);

                        return MediaType.Bernoulli150;
                    case 390696 when vendor.Equals("syquest", StringComparison.InvariantCultureIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_SQ2000);

                        return MediaType.SQ2000;
                    case 393380 when model.ToLowerInvariant().StartsWith("hifd", StringComparison.Ordinal):
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_HiFD,
                                                   blocks,
                                                   blockSize);

                        return MediaType.HiFD;
                    case 429975 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA201_embossed,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_201_ROM;
                    case 446325 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA201,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_201;
                    case 450560 when vendor.Equals("syquest", StringComparison.InvariantCultureIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_EZ230);

                        return MediaType.EZ230;
                    case 469504 when model.ToLowerInvariant().StartsWith("ls-", StringComparison.Ordinal):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_LS240);

                        return MediaType.LS240;
                    case 489532 when model.ToLowerInvariant().StartsWith("zip", StringComparison.OrdinalIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_ZIP250);

                        return MediaType.ZIP250;
                    case 524288 when vendor.Equals("syquest", StringComparison.InvariantCultureIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_SQ327);

                        return MediaType.SQ327;
                    case 694929 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA223,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_223_512;
                    case 904995 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA183,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_183_512;
                    case 1041500 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ISO15041,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ISO_15041_512;
                    case 1128772 when mediumType is 0x01 or 0x02:
                    case 1163337 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA184,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_184_512;
                    case 1281856 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization
                                                      .SCSI_medium_type_is_0_media_has_1_blocks_of_2_bytes_setting_media_type_to_WORM_PD_650,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.PD650_WORM;
                    case 1298496 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization
                                                      .SCSI_medium_type_is_0_media_has_1_blocks_of_2_bytes_setting_media_type_to_PD_650,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.PD650;
                    case 1470500 when model.ToLowerInvariant().StartsWith("zip", StringComparison.OrdinalIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_ZIP250);

                        return MediaType.ZIP750;
                    case 1644581 when mediumType is 0x01 or 0x02:
                    case 1647371 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA195,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_195_512;
                    case 1961069 when vendor.Equals("syquest", StringComparison.InvariantCultureIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization
                                                      .Drive_manufacturer_is_SyQuest_media_has_1961069_blocks_of_512_bytes_setting_media_type_to_SparQ);

                        return MediaType.SparQ;
                    case 2091050 when model.ToLowerInvariant().StartsWith("jaz", StringComparison.Ordinal):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_JAZ);

                        return MediaType.Jaz;
                    case 2244958 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ISO14517,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ISO_14517_512;
                    case 2929800 when vendor.Equals("syquest", StringComparison.InvariantCultureIgnoreCase):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_SyJet);

                        return MediaType.SyJet;
                    case 3915600 when model.ToLowerInvariant().StartsWith("jaz", StringComparison.Ordinal):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_JAZ2);

                        return MediaType.Jaz2;
                    case 4307184 when vendor.ToLowerInvariant().StartsWith("cws orb", StringComparison.Ordinal):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_Orb);

                        return MediaType.Orb;
                    case 625134256 when model.ToLowerInvariant().StartsWith("rdx", StringComparison.Ordinal):
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_RDX320,
                                                   blocks,
                                                   blockSize);

                        return MediaType.RDX320;
                }

                break;
            case 1024:
            {
                switch(blocks)
                {
                    case 800 when mediumType == 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Acorn_MF2DD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ACORN_35_DS_DD;
                    case 1220:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_IBM_53FD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.IBM53FD_1024;
                    case 1232 when model.ToLowerInvariant().StartsWith("ls-", StringComparison.Ordinal):
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_LS_PC98_MF2HD);

                        return MediaType.NEC_35_HD_8;
                    case 1232:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Sharp_MF2HD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.SHARP_35;
                    case 1268:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA69,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_69_8;
                    case 1280:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_PC98_MD2HD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.NEC_525_HD;
                    case 1316:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA99,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_99_8;
                    case 1600 when mediumType == 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_Acorn_MF2HD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ACORN_35_DS_HD;
                    case 314569 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ISO10089,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ISO_10089;
                    case 371371 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA223,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_223;
                    case 498526 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA183,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_183;
                    case 603466 when mediumType is 0x01 or 0x02:
                    case 637041 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA184,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_184;
                    case 936921 when mediumType is 0x01 or 0x02:
                    case 948770 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA195,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_195;
                    case 1244621 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA238,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_238;
                    case 1273011 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ISO14517,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ISO_14517;
                    case 2319786 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ISO15286,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ISO_15286_1024;
                    case 4383356 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA322,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_322_1k;
                    case 14476734 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA260,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_260;
                    case 24445990 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA260,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_260_Double;
                }
            }

                break;
            case 2048:
            {
                switch(blocks)
                {
                    case 112311:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_MD60,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.MD60;
                    case 138363:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_MD74,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.MD74;
                    case 149373:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_MD80,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.MD80;
                    case 310352 when mediumType is 0x01 or 0x02: // Found in real media
                    case 318988 when mediumType is 0x01 or 0x02:
                    case 320332 when mediumType is 0x01 or 0x02:
                    case 321100 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA239,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_239;
                    case 494023:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_HiMD,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.HiMD;
                    case 605846 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_GigaMO,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.GigaMo;
                    case 1063146 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_GigaMO2,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.GigaMo2;
                    case 1128134 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA280,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_280;
                    case 1263472 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ISO15286,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ISO_15286;
                    case 2043664 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA322,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_322_2k;
                    case 7355716 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA317,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_317;
                }
            }

                break;
            case 4096:
            {
                switch(blocks)
                {
                    case 1095840 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_ECMA322,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.ECMA_322;
                }
            }

                break;
            case 8192:
            {
                switch(blocks)
                {
                    case 1834348 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_UDO,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.UDO;
                    case 3668759 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_WORM_UDO2,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.UDO2_WORM;
                    case 3669724 when mediumType is 0x01 or 0x02:
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization.SCSI_Media_Type_Description_UDO2,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.UDO2;
                }
            }

                break;
        }

        return MediaType.Unknown;
    }
}