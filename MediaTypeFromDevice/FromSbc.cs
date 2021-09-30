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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.Console;

namespace Aaru.CommonTypes
{
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
            if(vendor.ToLowerInvariant() == "syquest" &&
               model.StartsWith("syjet", StringComparison.OrdinalIgnoreCase))
            {
                AaruConsole.DebugWriteLine("Media detection",
                                           "Drive manufacturer is SyQuest, drive model is SyJet, setting media type to SyJet.");

                return MediaType.SyJet;
            }

            switch(mediumType)
            {
                case 0x09:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-54 formatted 8\" floppy.",
                                               mediumType, blocks, blockSize);

                    return MediaType.ECMA_54;
                case 0x0A:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-59 formatted 8\" floppy.",
                                               mediumType, blocks, blockSize);

                    return MediaType.ECMA_59;
                case 0x0B:
                    switch(blockSize)
                    {
                        case 256:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-69 formatted 8\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_69_26;
                        case 512:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-69 formatted 8\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_69_15;
                        case 1024:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-69 formatted 8\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_69_8;
                    }

                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to unknown.",
                                               mediumType, blocks, blockSize);

                    return MediaType.Unknown;
                case 0x0E:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-66 formatted 5¼\" floppy.",
                                               mediumType, blocks, blockSize);

                    return MediaType.ECMA_66;
                case 0x12:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-70 formatted 5¼\" floppy.",
                                               mediumType, blocks, blockSize);

                    return MediaType.ECMA_70;
                case 0x16:
                    switch(blockSize)
                    {
                        case 256:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-78 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_78;
                        case 512:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-78 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_78_2;
                    }

                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to unknown.",
                                               mediumType, blocks, blockSize);

                    return MediaType.Unknown;
                case 0x1A:
                    switch(blockSize)
                    {
                        case 256:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-99 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_99_26;
                        case 512:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-99 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_99_15;
                        case 1024:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-99 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_99_8;
                    }

                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to unknown.",
                                               mediumType, blocks, blockSize);

                    return MediaType.Unknown;
                case 0x1E:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 3½\" double density floppy.",
                                               mediumType, blocks, blockSize);

                    return MediaType.DOS_35_DS_DD_9;
                case 0x41:
                    switch(blocks)
                    {
                        case 58620544:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to 120Gb REV.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.REV120;
                        case 34185728:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to 70Gb REV.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.REV70;
                        case 17090880:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to 35Gb REV.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.REV35;
                    }

                    break;
                case 0x93:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to PC-98 formatted 3½\" high density floppy (15 sectors).",
                                               mediumType, blocks, blockSize);

                    return MediaType.NEC_35_HD_15;
                case 0x94:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 3½\" high density floppy.",
                                               mediumType, blocks, blockSize);

                    return MediaType.DOS_35_HD;
            }

            switch(blockSize)
            {
                case 128:
                    switch(blocks)
                    {
                        case 720:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Atari formatted 5¼\" single density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ATARI_525_SD;
                        case 1040:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Atari formatted 5¼\" double density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ATARI_525_DD;
                        case 1898:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 8\" (33FD) floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.IBM33FD_128;
                        case 2002:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-54 formatted 8\" single density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_54;
                        case 3848:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 8\" (43FD) floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.IBM43FD_128;
                        case 4004:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-59 formatted 8\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_59;
                    }

                    break;
                case 256:
                    switch(blocks)
                    {
                        case 322:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-56 formatted 5¼\" double density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_66;
                        case 400:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Acorn formatted 5¼\" single density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ACORN_525_SS_SD_40;
                        case 455:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Apple DOS 3.2 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.Apple32SS;
                        case 560:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Apple DOS 3.3 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.Apple33SS;
                        case 640:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Acorn formatted 5¼\" double density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ACORN_525_SS_DD_40;
                        case 720:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Atari formatted 5¼\" double density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ATARI_525_DD;
                        case 800:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Acorn formatted 5¼\" double density floppy (80 tracks).",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ACORN_525_SS_SD_80;
                        case 910:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Apple DOS 3.2 formatted 5¼\" double sided floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.Apple32DS;
                        case 1120:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Apple DOS 3.3 formatted 5¼\" double sided floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.Apple33DS;
                        case 1121:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 8\" (33FD) floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.IBM33FD_256;
                        case 1280 when mediumType == 0x01:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Acorn formatted 5¼\" double density floppy with 80 tracks.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ACORN_525_SS_DD_80;
                        case 1280:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-70 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_70;
                        case 2002:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to DEC RX02 floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.RX02;
                        case 2560:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-78 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_78;
                        case 3848:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 8\" (53FD) floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.IBM53FD_256;
                        case 4004:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-99 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_99_26;
                        case 39168 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                        case 41004 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is IOMEGA, media has {0} blocks of 256 bytes, setting media type to 10Mb Bernoulli Box.",
                                                       blocks);

                            return MediaType.Bernoulli10;
                    }

                    break;
                case 319:
                    switch(blocks)
                    {
                        case 256:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 8\" (23FD) floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.IBM23FD;
                    }

                    break;
                case 512:
                    switch(blocks)
                    {
                        case 320:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 5¼\" double density single sided floppy (8 sectors).",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_525_SS_DD_8;
                        case 360:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 5¼\" double density single sided floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_525_SS_DD_9;
                        case 610:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 8\" (33FD) floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.IBM33FD_512;
                        case 630 when mediumType == 0x01:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Apricot formatted 3½\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.Apricot_35;
                        case 640 when mediumType == 0x01:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 3½\" double density single sided floppy (8 sectors).",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_35_SS_DD_8;
                        case 640:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 5¼\" double density floppy (8 sectors).",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_525_DS_DD_8;
                        case 720 when mediumType == 0x01:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 3½\" double density single sided floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_35_SS_DD_9;
                        case 720:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 5¼\" double density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_525_DS_DD_9;
                        case 800:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Apple formatted 3½\" double density single sided floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.AppleSonySS;
                        case 1280:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 3½\" double density floppy (8 sectors).",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_35_DS_DD_8;
                        case 1440:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 3½\" double density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_35_DS_DD_9;
                        case 1640:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to FDFORMAT formatted 3½\" double density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.FDFORMAT_35_DD;
                        case 1760:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Amiga formatted 3½\" double density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.CBM_AMIGA_35_DD;
                        case 2242:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 8\" (53FD) floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.IBM53FD_512;
                        case 2332:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-99 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_99_15;
                        case 2400:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 5¼\" high density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_525_HD;
                        case 2788:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to FDFORMAT formatted 5¼\" high density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.FDFORMAT_525_HD;
                        case 2880:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 3½\" high density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_35_HD;
                        case 3360:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Microsoft DMF formatted 3½\" high density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DMF;
                        case 3444:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to FDFORMAT formatted 3½\" high density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.FDFORMAT_35_HD;
                        case 3520:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Amiga formatted 3½\" high density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.CBM_AMIGA_35_HD;
                        case 5760:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 3½\" extra density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.DOS_35_ED;
                        case 40662 when mediumType == 0x20:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Floptical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.Floptical;
                        case 65536 when model.ToLowerInvariant().StartsWith("ls-", StringComparison.Ordinal):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive model is LS (SuperDisk), media has 65536 blocks of 512 bytes, setting media type to FD32MB.");

                            return MediaType.FD32MB;
                        case 78882 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is IOMEGA, media has 78882 blocks of 512 bytes, setting media type to PocketZIP.");

                            return MediaType.PocketZip;
                        case 86700 when vendor.ToLowerInvariant() == "syquest":
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is SyQuest, media has 86700 blocks of 512 bytes, setting media type to SQ400.");

                            return MediaType.SQ400;
                        case 87040 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is IOMEGA, media has 87040 blocks of 512 bytes, setting media type to 44Mb Bernoulli Box II.");

                            return MediaType.Bernoulli44;
                        case 173456 when vendor.ToLowerInvariant() == "syquest":
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is SyQuest, media has 173456 blocks of 512 bytes, setting media type to SQ800.");

                            return MediaType.SQ800;
                        case 175856 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is IOMEGA, media has 175856 blocks of 512 bytes, setting media type to 90Mb Bernoulli Box II.");

                            return MediaType.Bernoulli90;
                        case 196608 when model.ToLowerInvariant().StartsWith("zip", StringComparison.OrdinalIgnoreCase):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is IOMEGA, drive model is ZIP, media has 196608 blocks of 512 bytes, setting media type to 100Mb ZIP.");

                            return MediaType.ZIP100;

                        case 215440 when vendor.ToLowerInvariant() == "syquest":
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is SyQuest, media has 215440 blocks of 512 bytes, setting media type to SQ310.");

                            return MediaType.SQ310;
                        case 246528 when model.ToLowerInvariant().StartsWith("ls-", StringComparison.Ordinal):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive model is LS (SuperDisk), media has 246528 blocks of 512 bytes, setting media type to LS-120.");

                            return MediaType.LS120;
                        case 248826 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-154 / ISO 10090 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_154;
                        case 262144 when vendor.ToLowerInvariant() == "syquest":
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is SyQuest, media has 262144 blocks of 512 bytes, setting media type to EZ135.");

                            return MediaType.EZ135;
                        case 294918 when vendor.StartsWith("iomega", StringComparison.OrdinalIgnoreCase):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is IOMEGA, media has 294918 blocks of 512 bytes, setting media type to 150Mb Bernoulli Box II.");

                            return MediaType.Bernoulli150;
                        case 390696 when vendor.ToLowerInvariant() == "syquest":
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is SyQuest, media has 390696 blocks of 512 bytes, setting media type to SQ2000.");

                            return MediaType.SQ2000;
                        case 393380 when model.ToLowerInvariant().StartsWith("hifd", StringComparison.Ordinal):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive model is HiFD, media has 393380 blocks of 512 bytes, setting media type to HiFD.",
                                                       blocks, blockSize);

                            return MediaType.HiFD;
                        case 429975 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-201 / ISO 13963 conforming 3½\" embossed magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_201_ROM;
                        case 446325 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-201 / ISO 13963 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_201;
                        case 450560 when vendor.ToLowerInvariant() == "syquest":
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is SyQuest, media has 450560 blocks of 512 bytes, setting media type to EZ230.");

                            return MediaType.EZ230;
                        case 469504 when model.ToLowerInvariant().StartsWith("ls-", StringComparison.Ordinal):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive model is LS (SuperDisk), media has 469504 blocks of 512 bytes, setting media type to LS-240.");

                            return MediaType.LS240;
                        case 489532 when model.ToLowerInvariant().StartsWith("zip", StringComparison.OrdinalIgnoreCase):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is IOMEGA, drive model is ZIP, media has 489532 blocks of 512 bytes, setting media type to 250Mb ZIP.");

                            return MediaType.ZIP250;
                        case 524288 when vendor.ToLowerInvariant() == "syquest":
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is SyQuest, media has 524288 blocks of 512 bytes, setting media type to SQ327.");

                            return MediaType.SQ327;
                        case 694929 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-223 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_223_512;
                        case 904995 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-183 / ISO 13481 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_183_512;
                        case 1041500 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 15041 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_15041_512;
                        case 1128772 when mediumType == 0x01 || mediumType == 0x02:
                        case 1163337 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-184 / ISO 13549 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_184_512;
                        case 1281856 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to WORM PD-650.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.PD650_WORM;
                        case 1298496 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to PD-650.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.PD650;
                        case 1470500
                            when model.ToLowerInvariant().StartsWith("zip", StringComparison.OrdinalIgnoreCase):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is IOMEGA, drive model is ZIP, media has 489532 blocks of 512 bytes, setting media type to 250Mb ZIP.");

                            return MediaType.ZIP750;
                        case 1644581 when mediumType == 0x01 || mediumType == 0x02:
                        case 1647371 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-195 / ISO 13842 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_195_512;
                        case 1961069 when vendor.ToLowerInvariant() == "syquest":
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is SyQuest, media has 1961069 blocks of 512 bytes, setting media type to SparQ.");

                            return MediaType.SparQ;
                        case 2091050 when model.ToLowerInvariant().StartsWith("jaz", StringComparison.Ordinal):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is IOMEGA, drive model is JAZ, media has 2091050 blocks of 512 bytes, setting media type to 1Gb JAZ.");

                            return MediaType.Jaz;
                        case 2244958 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 14517 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_14517_512;
                        case 3915600 when model.ToLowerInvariant().StartsWith("jaz", StringComparison.Ordinal):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive manufacturer is IOMEGA, drive model is JAZ, media has 3915600 blocks of 512 bytes, setting media type to 2Gb JAZ.");

                            return MediaType.Jaz2;
                        case 4307184 when vendor.ToLowerInvariant().StartsWith("cws orb", StringComparison.Ordinal):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive model is Castlewood Orb, media has 4307184 blocks of 512 bytes, setting media type to Orb.");

                            return MediaType.Orb;
                        case 625134256 when model.ToLowerInvariant().StartsWith("rdx", StringComparison.Ordinal):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive model is LS (SuperDisk), media has {0} blocks of {1} bytes, setting media type to unknown.",
                                                       blocks, blockSize);

                            return MediaType.RDX320;
                    }

                    break;
                case 1024:
                {
                    switch(blocks)
                    {
                        case 800 when mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Acorn formatted 3½\" double density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ACORN_35_DS_DD;
                        case 1220:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to IBM formatted 8\" (53FD) floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.IBM53FD_1024;
                        case 1232 when model.ToLowerInvariant().StartsWith("ls-", StringComparison.Ordinal):
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "Drive model is LS (SuperDisk), media has 2880 blocks of 512 bytes, setting media type to PC-98 formatted 3½\" high density floppy.");

                            return MediaType.NEC_35_HD_8;
                        case 1232:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Sharp formatted 3½\" high density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.SHARP_35;
                        case 1268:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-69 formatted 8\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_69_8;
                        case 1280:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to PC-98 formatted 5¼\" high density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.NEC_525_HD;
                        case 1316:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-99 formatted 5¼\" floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_99_8;
                        case 1600 when mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Acorn formatted 3½\" high density floppy.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ACORN_35_DS_HD;
                        case 314569 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 10089 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_10089;
                        case 371371 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-223 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_223;
                        case 498526 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-183 / ISO 13481 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_183;
                        case 603466 when mediumType == 0x01 || mediumType == 0x02:
                        case 637041 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-184 / ISO 13549 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_184;
                        case 936921 when mediumType == 0x01 || mediumType == 0x02:
                        case 948770 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-195 / ISO 13842 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_195;
                        case 1244621 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-238 / ISO 15486 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_238;
                        case 1273011 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 14517 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_14517;
                        case 2319786 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 15286 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_15286_1024;
                        case 4383356 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-322 / ISO 22092 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_322_1k;
                        case 14476734 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-260 / ISO 15898 conforming 356mm magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_260;
                        case 24445990 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-260 / ISO 15898 conforming 356mm magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_260_Double;
                    }
                }

                    break;
                case 2048:
                {
                    switch(blocks)
                    {
                        case 112311:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to 60 minute MiniDisc.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.MD60;
                        case 138363:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to 74 minute MiniDisc.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.MD74;
                        case 149373:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to 80 minute MiniDisc.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.MD80;
                        case 310352 when mediumType == 0x01 || mediumType == 0x02: // Found in real media
                        case 318988 when mediumType == 0x01 || mediumType == 0x02:
                        case 320332 when mediumType == 0x01 || mediumType == 0x02:
                        case 321100 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-239 / ISO 15498 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_239;
                        case 494023:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to Sony HiMD.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.HiMD;
                        case 605846 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to GigaMO 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.GigaMo;
                        case 1063146 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to GigaMO 2 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.GigaMo2;
                        case 1128134 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-280 / ISO 18093 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_280;
                        case 1263472 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 15286 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_15286;
                        case 2043664 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-322 / ISO 22092 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_322_2k;
                        case 7355716 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-317 / ISO 20162 conforming 300mm magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_317;
                    }
                }

                    break;
                case 4096:
                {
                    switch(blocks)
                    {
                        case 1095840 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-322 / ISO 22092 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_322;
                    }
                }

                    break;
                case 8192:
                {
                    switch(blocks)
                    {
                        case 1834348 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to UDO.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UDO;
                        case 3668759 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to WORM UDO2.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UDO2_WORM;
                        case 3669724 when mediumType == 0x01 || mediumType == 0x02:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to UDO2.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UDO2;
                    }
                }

                    break;
            }

            return MediaType.Unknown;
        }
    }
}