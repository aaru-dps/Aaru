// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MediaTypeFromSCSI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef common types.
//
// --[ Description ] ----------------------------------------------------------
//
//     Lookups media type from SCSI informative values.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;

namespace DiscImageChef.CommonTypes
{
    #pragma warning disable RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
    public static class MediaTypeFromScsi
    {
        /// <summary>Tries to guess, from SCSI information, the media type of a device and/or its inserted media</summary>
        /// <param name="scsiPeripheralType">The SCSI Peripheral Type as indicated in the INQUIRY response</param>
        /// <param name="vendor">The vendor string of the device</param>
        /// <param name="model">The model string of the device</param>
        /// <param name="mediumType">The medium type byte from MODE SENSE</param>
        /// <param name="densityCode">The density type byte from MODE SENSE</param>
        /// <param name="blocks">How many blocks are on the media</param>
        /// <param name="blockSize">Size in bytes of each block</param>
        /// <returns></returns>
        public static MediaType Get(byte scsiPeripheralType, string vendor, string model, byte mediumType,
                                    byte densityCode, ulong blocks, uint blockSize)
        {
            switch(scsiPeripheralType)
            {
                // Direct access device
                case 0x00:
                // Simpilified access device
                case 0x0E:
                {
                    if(mediumType == 0x03 ||
                       mediumType == 0x05 ||
                       mediumType == 0x07)
                        goto case 0x07;

                    if(vendor.ToLowerInvariant() == "syquest")
                    {
                        if(blocks    == 173400 &&
                           blockSize == 256)
                            return MediaType.SQ400;

                        if(blockSize != 512)
                            return MediaType.Unknown;

                        if(model.ToLowerInvariant().StartsWith("syjet", StringComparison.Ordinal))
                            return MediaType.SyJet;

                        switch(blocks)
                        {
                            case 262144: return MediaType.EZ135;
                            case 524288: return MediaType.SQ327;
                        }

                        return MediaType.Unknown;
                    }

                    if(vendor.ToLowerInvariant().StartsWith("iomega", StringComparison.Ordinal) &&
                       (model.ToLowerInvariant().StartsWith("clik", StringComparison.Ordinal) ||
                        model.ToLowerInvariant().StartsWith("pocketzip", StringComparison.Ordinal)) &&
                       blockSize == 512                                                             &&
                       blocks    == 78882)
                        return MediaType.PocketZip;

                    if(model.ToLowerInvariant().StartsWith("zip", StringComparison.Ordinal))
                    {
                        if(blockSize != 512)
                            return MediaType.Unknown;

                        if(blocks == 196608)
                            return MediaType.ZIP100;

                        return blocks == 489532 ? MediaType.ZIP250 : MediaType.ZIP750;
                    }

                    if(model.ToLowerInvariant().StartsWith("jaz", StringComparison.Ordinal))
                    {
                        if(blockSize != 512)
                            return MediaType.Unknown;

                        if(blocks == 2091050)
                            return MediaType.Jaz;

                        return blocks == 3915600 ? MediaType.Jaz2 : MediaType.Unknown;
                    }

                    if(model.ToLowerInvariant().StartsWith("ls-", StringComparison.Ordinal))
                    {
                        if(blockSize == 512)
                        {
                            if(blocks == 469504)
                                return MediaType.LS240;

                            if(blocks == 246528)
                                return MediaType.LS120;

                            if(blocks == 65536)
                                return MediaType.FD32MB;

                            if(blocks == 2880)
                                return MediaType.DOS_35_HD;

                            if(blocks == 1440)
                                return MediaType.DOS_35_DS_DD_9;
                        }
                        else if(blockSize == 1024)
                        {
                            if(blocks == 1232)
                                return MediaType.NEC_35_HD_8;
                        }

                        return MediaType.Unknown;
                    }

                    if(model.ToLowerInvariant().StartsWith("rdx", StringComparison.Ordinal))
                    {
                        if(blockSize != 512)
                            return MediaType.Unknown;

                        return blocks == 625134256 ? MediaType.RDX320 : MediaType.RDX;
                    }

                    switch(mediumType)
                    {
                        case 0x01:
                            switch(blockSize)
                            {
                                case 128:
                                    switch(blocks)
                                    {
                                        case 720:  return MediaType.ATARI_525_SD;
                                        case 1040: return MediaType.ATARI_525_DD;
                                        case 1898: return MediaType.IBM33FD_128;
                                        case 2002: return MediaType.ECMA_54;
                                    }

                                    break;
                                case 256:
                                    switch(blocks)
                                    {
                                        case 322:  return MediaType.ECMA_66;
                                        case 400:  return MediaType.ACORN_525_SS_SD_40;
                                        case 455:  return MediaType.Apple32SS;
                                        case 560:  return MediaType.Apple33SS;
                                        case 640:  return MediaType.ACORN_525_SS_DD_40;
                                        case 720:  return MediaType.ATARI_525_DD;
                                        case 800:  return MediaType.ACORN_525_SS_SD_80;
                                        case 1121: return MediaType.IBM33FD_256;
                                        case 1280: return MediaType.ACORN_525_SS_DD_80;
                                        case 2002: return MediaType.RX02;
                                    }

                                    break;
                                case 319:
                                    switch(blocks)
                                    {
                                        case 256: return MediaType.IBM23FD;
                                    }

                                    break;
                                case 512:
                                    switch(blocks)
                                    {
                                        case 320:    return MediaType.DOS_525_DS_DD_8;
                                        case 360:    return MediaType.DOS_35_SS_DD_9;
                                        case 610:    return MediaType.IBM33FD_512;
                                        case 630:    return MediaType.Apricot_35;
                                        case 640:    return MediaType.DOS_35_SS_DD_8;
                                        case 720:    return MediaType.DOS_35_DS_DD_9;
                                        case 800:    return MediaType.AppleSonySS;
                                        case 248826: return MediaType.ECMA_154;
                                        case 429975: return MediaType.ECMA_201_ROM;
                                        case 446325: return MediaType.ECMA_201;
                                        case 694929: return MediaType.ECMA_223_512;
                                        case 904995: return MediaType.ECMA_183_512;
                                        case 1128772:
                                        case 1163337: return MediaType.ECMA_184_512;
                                        case 1281856: return MediaType.PD650_WORM;
                                        case 1298496: return MediaType.PD650;
                                        case 1644581:
                                        case 1647371: return MediaType.ECMA_195_512;
                                    }

                                    break;
                                case 1024:
                                {
                                    switch(blocks)
                                    {
                                        case 371371: return MediaType.ECMA_223;
                                        case 498526: return MediaType.ECMA_183;
                                        case 603466:
                                        case 637041: return MediaType.ECMA_184;
                                        case 936921:
                                        case 948770: return MediaType.ECMA_195;
                                        case 1244621:  return MediaType.ECMA_238;
                                        case 14476734: return MediaType.ECMA_260;
                                        case 24445990: return MediaType.ECMA_260_Double;
                                    }
                                }

                                    break;
                                case 2048:
                                {
                                    switch(blocks)
                                    {
                                        case 310352: // Found in real media
                                        case 318988:
                                        case 320332:
                                        case 321100: return MediaType.ECMA_239;
                                        case 605846:  return MediaType.GigaMo;
                                        case 1063146: return MediaType.GigaMo2;
                                        case 1128134: return MediaType.ECMA_280;
                                        case 2043664: return MediaType.ECMA_322_2k;
                                        case 7355716: return MediaType.ECMA_317;
                                    }
                                }

                                    break;
                                case 4096:
                                {
                                    switch(blocks)
                                    {
                                        case 1095840: return MediaType.ECMA_322;
                                    }
                                }

                                    break;
                                case 8192:
                                {
                                    switch(blocks)
                                    {
                                        case 1834348: return MediaType.UDO;
                                        case 3668759: return MediaType.UDO2_WORM;
                                        case 3669724: return MediaType.UDO2;
                                    }
                                }

                                    break;
                            }

                            return MediaType.Unknown;
                        case 0x02:
                            switch(blockSize)
                            {
                                case 128:
                                    switch(blocks)
                                    {
                                        case 3848: return MediaType.IBM43FD_128;
                                        case 4004: return MediaType.ECMA_59;
                                    }

                                    break;
                                case 256:
                                    switch(blocks)
                                    {
                                        case 910:  return MediaType.Apple32DS;
                                        case 1120: return MediaType.Apple33DS;
                                        case 1280: return MediaType.ECMA_70;
                                        case 2560: return MediaType.ECMA_78;
                                        case 3848: return MediaType.IBM53FD_256;
                                        case 4004: return MediaType.ECMA_99_26;
                                    }

                                    break;
                                case 512:
                                    switch(blocks)
                                    {
                                        case 640:    return MediaType.DOS_525_DS_DD_8;
                                        case 720:    return MediaType.DOS_525_DS_DD_9;
                                        case 1280:   return MediaType.DOS_35_DS_DD_8;
                                        case 1440:   return MediaType.DOS_35_DS_DD_9;
                                        case 1640:   return MediaType.FDFORMAT_35_DD;
                                        case 1760:   return MediaType.CBM_AMIGA_35_DD;
                                        case 2242:   return MediaType.IBM53FD_512;
                                        case 2332:   return MediaType.ECMA_99_15;
                                        case 2400:   return MediaType.DOS_525_HD;
                                        case 2788:   return MediaType.FDFORMAT_525_HD;
                                        case 2880:   return MediaType.DOS_35_HD;
                                        case 3360:   return MediaType.DMF;
                                        case 3444:   return MediaType.FDFORMAT_35_HD;
                                        case 3520:   return MediaType.CBM_AMIGA_35_HD;
                                        case 5760:   return MediaType.DOS_35_ED;
                                        case 248826: return MediaType.ECMA_154;
                                        case 429975: return MediaType.ECMA_201_ROM;
                                        case 446325: return MediaType.ECMA_201;
                                        case 694929: return MediaType.ECMA_223_512;
                                        case 904995: return MediaType.ECMA_183_512;
                                        case 1128772:
                                        case 1163337: return MediaType.ECMA_184_512;
                                        case 1281856: return MediaType.PD650_WORM;
                                        case 1298496: return MediaType.PD650;
                                        case 1644581:
                                        case 1647371: return MediaType.ECMA_195_512;
                                    }

                                    break;
                                case 1024:
                                    switch(blocks)
                                    {
                                        case 800:    return MediaType.ACORN_35_DS_DD;
                                        case 1600:   return MediaType.ACORN_35_DS_HD;
                                        case 1220:   return MediaType.IBM53FD_1024;
                                        case 1232:   return MediaType.SHARP_35;
                                        case 1268:   return MediaType.ECMA_69_8;
                                        case 1280:   return MediaType.NEC_525_HD;
                                        case 1316:   return MediaType.ECMA_99_8;
                                        case 371371: return MediaType.ECMA_223;
                                        case 498526: return MediaType.ECMA_183;
                                        case 603466:
                                        case 637041: return MediaType.ECMA_184;
                                        case 936921:
                                        case 948770: return MediaType.ECMA_195;
                                        case 1244621:  return MediaType.ECMA_238;
                                        case 14476734: return MediaType.ECMA_260;
                                        case 24445990: return MediaType.ECMA_260_Double;
                                    }

                                    break;
                                case 2048:
                                {
                                    switch(blocks)
                                    {
                                        case 310352: // Found in real media
                                        case 318988:
                                        case 320332:
                                        case 321100: return MediaType.ECMA_239;
                                        case 605846:  return MediaType.GigaMo;
                                        case 1063146: return MediaType.GigaMo2;
                                        case 1128134: return MediaType.ECMA_280;
                                        case 2043664: return MediaType.ECMA_322_2k;
                                        case 7355716: return MediaType.ECMA_317;
                                    }
                                }

                                    break;
                                case 4096:
                                {
                                    switch(blocks)
                                    {
                                        case 1095840: return MediaType.ECMA_322;
                                    }
                                }

                                    break;
                                case 8192:
                                {
                                    switch(blocks)
                                    {
                                        case 1834348: return MediaType.UDO;
                                        case 3668759: return MediaType.UDO2_WORM;
                                        case 3669724: return MediaType.UDO2;
                                    }
                                }

                                    break;
                            }

                            return MediaType.Unknown;
                        case 0x09: return MediaType.ECMA_54;
                        case 0x0A: return MediaType.ECMA_59;
                        case 0x0B:
                            switch(blockSize)
                            {
                                case 256:  return MediaType.ECMA_69_26;
                                case 512:  return MediaType.ECMA_69_15;
                                case 1024: return MediaType.ECMA_69_8;
                            }

                            return MediaType.Unknown;
                        case 0x0E: return MediaType.ECMA_66;
                        case 0x12: return MediaType.ECMA_70;
                        case 0x16:
                            switch(blockSize)
                            {
                                case 256: return MediaType.ECMA_78;
                                case 512: return MediaType.ECMA_78_2;
                            }

                            return MediaType.Unknown;
                        case 0x1A:
                            switch(blockSize)
                            {
                                case 256:  return MediaType.ECMA_99_26;
                                case 512:  return MediaType.ECMA_99_15;
                                case 1024: return MediaType.ECMA_99_8;
                            }

                            return MediaType.Unknown;
                        case 0x1E: return MediaType.DOS_35_DS_DD_9;
                        case 0x41:
                            switch(blocks)
                            {
                                case 58620544: return MediaType.REV120;
                                case 17090880: return MediaType.REV35;

                                // TODO: Unknown value
                                default: return MediaType.REV70;
                            }

                            break;
                        case 0x93: return MediaType.NEC_35_HD_15;
                        case 0x94: return MediaType.DOS_35_HD;
                    }

                    switch(blockSize)
                    {
                        case 128:
                        {
                            switch(blocks)
                            {
                                case 720:  return MediaType.ATARI_525_SD;
                                case 1040: return MediaType.ATARI_525_ED;
                                case 1898: return MediaType.IBM33FD_128;
                                case 2002: return MediaType.ECMA_54;
                                case 3848: return MediaType.IBM43FD_128;
                                case 4004: return MediaType.ECMA_59;
                            }
                        }

                            break;
                        case 256:
                        {
                            switch(blocks)
                            {
                                case 322:  return MediaType.ECMA_66;
                                case 400:  return MediaType.ACORN_525_SS_SD_40;
                                case 455:  return MediaType.Apple32SS;
                                case 560:  return MediaType.Apple33SS;
                                case 640:  return MediaType.ACORN_525_SS_DD_40;
                                case 720:  return MediaType.ATARI_525_DD;
                                case 800:  return MediaType.ACORN_525_SS_SD_80;
                                case 910:  return MediaType.Apple32DS;
                                case 1120: return MediaType.Apple33DS;
                                case 1121: return MediaType.IBM33FD_256;
                                case 1280: return MediaType.ECMA_70;
                                case 2002: return MediaType.RX02;
                                case 2560: return MediaType.ECMA_78;
                                case 3848: return MediaType.IBM53FD_256;
                                case 4004: return MediaType.ECMA_99_26;
                            }
                        }

                            break;
                        case 319:
                            switch(blocks)
                            {
                                case 256: return MediaType.IBM23FD;
                            }

                            break;
                        case 512:
                        {
                            switch(blocks)
                            {
                                case 320:  return MediaType.DOS_525_SS_DD_8;
                                case 360:  return MediaType.DOS_525_SS_DD_9;
                                case 610:  return MediaType.IBM33FD_512;
                                case 640:  return MediaType.DOS_525_DS_DD_8;
                                case 720:  return MediaType.DOS_525_DS_DD_9;
                                case 800:  return MediaType.AppleSonySS;
                                case 1280: return MediaType.DOS_35_DS_DD_8;
                                case 1440: return MediaType.DOS_35_DS_DD_9;
                                case 1600: return MediaType.ACORN_35_DS_DD;
                                case 1640: return MediaType.FDFORMAT_35_DD;
                                case 1760: return MediaType.CBM_AMIGA_35_DD;
                                case 2242: return MediaType.IBM53FD_512;
                                case 2332: return MediaType.ECMA_99_15;
                                case 2400: return MediaType.DOS_525_HD;
                                case 2788: return MediaType.FDFORMAT_525_HD;
                                case 2880: return MediaType.DOS_35_HD;
                                case 3360: return MediaType.DMF;
                                case 3444: return MediaType.FDFORMAT_35_HD;
                                case 3520: return MediaType.CBM_AMIGA_35_HD;
                                case 5760: return MediaType.DOS_35_ED;
                            }
                        }

                            break;
                        case 1024:
                        {
                            switch(blocks)
                            {
                                case 1220: return MediaType.IBM53FD_1024;
                                case 1232: return MediaType.SHARP_35;
                                case 1268: return MediaType.ECMA_69_8;
                                case 1280: return MediaType.NEC_525_HD;
                                case 1316: return MediaType.ECMA_99_8;
                            }
                        }

                            break;
                    }

                    return MediaType.Unknown;
                }

                // Sequential access device
                case 0x01:
                {
                    switch(mediumType)
                    {
                        case 0x00:
                            switch(densityCode)
                            {
                                case 0x04: return MediaType.QIC11;
                                case 0x05: return MediaType.QIC24;
                                case 0x09: return MediaType.IBM3490;
                                case 0x0F: return MediaType.QIC120;
                                case 0x10: return MediaType.QIC150;
                                case 0x13: return MediaType.DDS1;
                                case 0x24: return MediaType.DDS2;
                                case 0x25: return MediaType.DDS3;
                                case 0x26: return MediaType.DDS4;
                                case 0x28: return MediaType.IBM3490E;
                                case 0x40:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO;

                                    if(model.ToLowerInvariant().StartsWith("sdz", StringComparison.Ordinal))
                                        return MediaType.SAIT1;

                                    break;
                                }

                                case 0x41:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO2;

                                    break;
                                }

                                case 0x42:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO2;

                                    if(vendor.ToLowerInvariant() == "stk")
                                        return MediaType.T9840A;

                                    break;
                                }

                                case 0x43:
                                {
                                    if(vendor.ToLowerInvariant() == "stk")
                                        return MediaType.T9940A;

                                    break;
                                }

                                case 0x44:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO3;

                                    if(vendor.ToLowerInvariant() == "stk")
                                        return MediaType.T9940B;

                                    break;
                                }

                                case 0x45:
                                {
                                    if(vendor.ToLowerInvariant() == "stk")
                                        return MediaType.T9840C;

                                    break;
                                }

                                case 0x46:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO4;

                                    if(vendor.ToLowerInvariant() == "stk")
                                        return MediaType.T9840D;

                                    break;
                                }

                                case 0x4A:
                                {
                                    if(vendor.ToLowerInvariant() == "stk")
                                        return MediaType.T10000A;

                                    break;
                                }

                                case 0x4B:
                                {
                                    if(vendor.ToLowerInvariant() == "stk")
                                        return MediaType.T10000B;

                                    break;
                                }

                                case 0x4C:
                                {
                                    if(vendor.ToLowerInvariant() == "stk")
                                        return MediaType.T10000C;

                                    break;
                                }

                                case 0x4D:
                                {
                                    if(vendor.ToLowerInvariant() == "stk")
                                        return MediaType.T10000D;

                                    break;
                                }

                                case 0x58:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO5;

                                    break;
                                }

                                // Used by some HP drives for all generations
                                case 0x8C:
                                {
                                    return MediaType.DDS1;

                                    break;
                                }
                            }

                            break;
                        case 0x01:
                        {
                            switch(densityCode)
                            {
                                case 0x44:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO3WORM;

                                    break;
                                }

                                case 0x46:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO4WORM;

                                    break;
                                }

                                case 0x58:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO5WORM;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x18:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO;

                                    break;
                                }

                                case 0x40:
                                {
                                    return MediaType.LTO;
                                }
                            }
                        }

                            break;
                        case 0x28:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO2;

                                    break;
                                }

                                case 0x42: return MediaType.LTO2;
                            }
                        }

                            break;
                        case 0x33:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x25:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dat", StringComparison.Ordinal))
                                        return MediaType.DDS3;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x34:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x26:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dat", StringComparison.Ordinal))
                                        return MediaType.DDS4;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x35:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x47:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dat", StringComparison.Ordinal))
                                        return MediaType.DAT72;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x38:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x44:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO3;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x3C:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x44:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO3WORM;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x48:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x46:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO4;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x4C:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x46:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO4WORM;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x50:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x24:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dat", StringComparison.Ordinal))
                                        return MediaType.DDS2;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x58:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x58:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO5;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x5C:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x58:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO5WORM;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x68:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x5A:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO6;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x6C:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x5A:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO6WORM;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x78:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x5C:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO7;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x7C:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x5C:
                                {
                                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                        return MediaType.LTO7WORM;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x81:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape15m;

                                    if(vendor.ToLowerInvariant() == "ibm")
                                        return MediaType.IBM3592;

                                    if(model.ToLowerInvariant().StartsWith("vxa", StringComparison.Ordinal))
                                        return MediaType.VXA1;

                                    break;
                                }

                                case 0x14:
                                case 0x15:
                                case 0x27:
                                case 0x8C:
                                case 0x90:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape15m;

                                    break;
                                }

                                case 0x29:
                                case 0x2A:
                                {
                                    if(vendor.ToLowerInvariant() == "ibm")
                                        return MediaType.IBM3592;

                                    break;
                                }

                                case 0x80:
                                {
                                    if(model.ToLowerInvariant().StartsWith("vxa", StringComparison.Ordinal))
                                        return MediaType.VXA1;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x82:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape28m;

                                    if(vendor.ToLowerInvariant() == "ibm")
                                        return MediaType.IBM3592;

                                    break;
                                }

                                case 0x0A:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal))
                                        return MediaType.CompactTapeI;

                                    break;
                                }

                                case 0x14:
                                case 0x15:
                                case 0x27:
                                case 0x8C:
                                case 0x90:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape28m;

                                    break;
                                }

                                case 0x16:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal))
                                        return MediaType.CompactTapeII;

                                    break;
                                }

                                case 0x29:
                                case 0x2A:
                                {
                                    if(vendor.ToLowerInvariant() == "ibm")
                                        return MediaType.IBM3592;

                                    break;
                                }

                                case 0x81:
                                {
                                    if(model.ToLowerInvariant().StartsWith("vxa", StringComparison.Ordinal))
                                        return MediaType.VXA2;

                                    break;
                                }

                                case 0x82:
                                {
                                    if(model.ToLowerInvariant().StartsWith("vxa", StringComparison.Ordinal))
                                        return MediaType.VXA3;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x83:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape54m;

                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal))
                                        return MediaType.DLTtapeIII;

                                    break;
                                }

                                case 0x14:
                                case 0x15:
                                case 0x27:
                                case 0x8C:
                                case 0x90:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape54m;

                                    break;
                                }

                                case 0x17:
                                case 0x18:
                                case 0x19:
                                case 0x80:
                                case 0x81:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal))
                                        return MediaType.DLTtapeIII;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x84:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape80m;

                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal))
                                        return MediaType.DLTtapeIIIxt;

                                    break;
                                }

                                case 0x14:
                                case 0x15:
                                case 0x27:
                                case 0x8C:
                                case 0x90:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape80m;

                                    break;
                                }

                                case 0x19:
                                case 0x80:
                                case 0x81:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal))
                                        return MediaType.DLTtapeIIIxt;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x85:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape106m;

                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal)  ||
                                       model.ToLowerInvariant().StartsWith("sdlt", StringComparison.Ordinal) ||
                                       model.ToLowerInvariant().StartsWith("superdlt", StringComparison.Ordinal))
                                        return MediaType.DLTtapeIV;

                                    if(model.ToLowerInvariant().StartsWith("stt", StringComparison.Ordinal))
                                        return MediaType.Travan5;

                                    break;
                                }

                                case 0x14:
                                case 0x15:
                                case 0x27:
                                case 0x8C:
                                case 0x90:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape106m;

                                    break;
                                }

                                case 0x1A:
                                case 0x1B:
                                case 0x40:
                                case 0x41:
                                case 0x82:
                                case 0x83:
                                case 0x84:
                                case 0x85:
                                case 0x86:
                                case 0x87:
                                case 0x88:
                                case 0x89:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal)  ||
                                       model.ToLowerInvariant().StartsWith("sdlt", StringComparison.Ordinal) ||
                                       model.ToLowerInvariant().StartsWith("superdlt", StringComparison.Ordinal))
                                        return MediaType.DLTtapeIV;

                                    break;
                                }

                                case 0x46:
                                {
                                    if(model.ToLowerInvariant().StartsWith("stt", StringComparison.Ordinal))
                                        return MediaType.Travan5;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x86:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x90:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape160mXL;

                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal)  ||
                                       model.ToLowerInvariant().StartsWith("sdlt", StringComparison.Ordinal) ||
                                       model.ToLowerInvariant().StartsWith("superdlt", StringComparison.Ordinal))
                                        return MediaType.SDLT1;

                                    break;
                                }

                                case 0x8C:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape160mXL;

                                    break;
                                }

                                case 0x91:
                                case 0x92:
                                case 0x93:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal)  ||
                                       model.ToLowerInvariant().StartsWith("sdlt", StringComparison.Ordinal) ||
                                       model.ToLowerInvariant().StartsWith("superdlt", StringComparison.Ordinal))
                                        return MediaType.SDLT1;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x87:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x4A:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal)  ||
                                       model.ToLowerInvariant().StartsWith("sdlt", StringComparison.Ordinal) ||
                                       model.ToLowerInvariant().StartsWith("superdlt", StringComparison.Ordinal))
                                        return MediaType.SDLT2;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x90:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x50:
                                case 0x98:
                                case 0x99:
                                {
                                    if(model.ToLowerInvariant().StartsWith("dlt", StringComparison.Ordinal)  ||
                                       model.ToLowerInvariant().StartsWith("sdlt", StringComparison.Ordinal) ||
                                       model.ToLowerInvariant().StartsWith("superdlt", StringComparison.Ordinal))
                                        return MediaType.VStapeI;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0x95:
                        {
                            if(model.ToLowerInvariant().StartsWith("stt", StringComparison.Ordinal))
                                return MediaType.Travan7;
                        }

                            break;
                        case 0xC1:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x14:
                                case 0x15:
                                case 0x8C:
                                case 0x90:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape22m;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0xC2:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x14:
                                case 0x15:
                                case 0x27:
                                case 0x8C:
                                case 0x90:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape40m;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0xC3:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x14:
                                case 0x15:
                                case 0x27:
                                case 0x8C:
                                case 0x90:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape76m;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0xC4:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x14:
                                case 0x15:
                                case 0x27:
                                case 0x8C:
                                case 0x90:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape112m;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0xD1:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x27:
                                case 0x28:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape22mAME;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0xD2:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x27:
                                case 0x28:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape170m;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0xD3:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x27:
                                case 0x28:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape125m;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0xD4:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x27:
                                case 0x28:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape45m;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0xD5:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x27:
                                case 0x28:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape225m;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0xD6:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x27:
                                case 0x28:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape150m;

                                    break;
                                }
                            }
                        }

                            break;
                        case 0xD7:
                        {
                            switch(densityCode)
                            {
                                case 0x00:
                                case 0x27:
                                case 0x28:
                                {
                                    if(model.ToLowerInvariant().StartsWith("exb", StringComparison.Ordinal))
                                        return MediaType.Exatape75m;

                                    break;
                                }
                            }
                        }

                            break;
                    }

                    return MediaType.Unknown;
                }

                // Write-once device
                case 0x04:
                // Optical device
                case 0x07:
                {
                    if(mediumType != 0x01 &&
                       mediumType != 0x02 &&
                       mediumType != 0x03 &&
                       mediumType != 0x05 &&
                       mediumType != 0x07)
                        return MediaType.UnknownMO;

                    switch(blockSize)
                    {
                        case 512:
                        {
                            switch(blocks)
                            {
                                case 248826: return MediaType.ECMA_154;
                                case 429975: return MediaType.ECMA_201_ROM;
                                case 446325: return MediaType.ECMA_201;
                                case 694929: return MediaType.ECMA_223_512;
                                case 904995: return MediaType.ECMA_183_512;
                                case 1128772:
                                case 1163337: return MediaType.ECMA_184_512;
                                case 1281856: return MediaType.PD650_WORM;
                                case 1298496: return MediaType.PD650;
                                case 1644581:
                                case 1647371: return MediaType.ECMA_195_512;
                                default: return MediaType.UnknownMO;
                            }
                        }

                        case 1024:
                        {
                            switch(blocks)
                            {
                                case 371371: return MediaType.ECMA_223;
                                case 498526: return MediaType.ECMA_183;
                                case 603466:
                                case 637041: return MediaType.ECMA_184;
                                case 936921:
                                case 948770: return MediaType.ECMA_195;
                                case 1244621:  return MediaType.ECMA_238;
                                case 14476734: return MediaType.ECMA_260;
                                case 24445990: return MediaType.ECMA_260_Double;
                                default:       return MediaType.UnknownMO;
                            }
                        }

                        case 2048:
                        {
                            switch(blocks)
                            {
                                case 310352: // Found in real media
                                case 318988:
                                case 320332:
                                case 321100: return MediaType.ECMA_239;
                                case 605846:  return MediaType.GigaMo;
                                case 1063146: return MediaType.GigaMo2;
                                case 1128134: return MediaType.ECMA_280;
                                case 2043664: return MediaType.ECMA_322_2k;
                                case 7355716: return MediaType.ECMA_317;
                                default:      return MediaType.UnknownMO;
                            }
                        }

                        case 4096:
                        {
                            switch(blocks)
                            {
                                case 1095840: return MediaType.ECMA_322;
                                default:      return MediaType.UnknownMO;
                            }
                        }

                        case 8192:
                        {
                            switch(blocks)
                            {
                                case 1834348: return MediaType.UDO;
                                case 3668759: return MediaType.UDO2_WORM;
                                case 3669724: return MediaType.UDO2;
                                default:      return MediaType.UnknownMO;
                            }
                        }

                        default: return MediaType.UnknownMO;
                    }
                }

                // MultiMedia Device
                case 0x05:
                {
                    switch(mediumType)
                    {
                        case 0x00:
                            return blockSize == 512 ? blocks == 1281856
                                                          ? MediaType.PD650_WORM
                                                          : MediaType.PD650 : MediaType.CD;
                        case 0x01:
                        case 0x05: return MediaType.CDROM;
                        case 0x02:
                        case 0x06: return MediaType.CDDA;
                        case 0x03:
                        case 0x07: return MediaType.CDPLUS;
                        case 0x04: return MediaType.PCD;
                        case 0x10:
                        case 0x11:
                        case 0x12:
                        case 0x13:
                        case 0x14:
                        case 0x15:
                        case 0x16:
                        case 0x17:
                        case 0x18: return MediaType.CDR;
                        case 0x20:
                        case 0x21:
                        case 0x22:
                        case 0x23:
                        case 0x24:
                        case 0x25:
                        case 0x26:
                        case 0x27:
                        case 0x28: return MediaType.CDRW;
                        case 0x80:
                            if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                                switch(densityCode)
                                {
                                    case 0x42: return MediaType.LTO2;
                                    case 0x44: return MediaType.LTO3;
                                    case 0x46: return MediaType.LTO4;
                                    case 0x58: return MediaType.LTO5;
                                }

                            break;
                    }
                }

                    break;

                // Host managed zoned block device
                case 0x14:
                {
                    return MediaType.Zone_HDD;
                }
            }

            return MediaType.Unknown;
        }
    }
    #pragma warning restore RECS0063 // Warns when a culture-aware 'StartsWith' call is used by default.
}