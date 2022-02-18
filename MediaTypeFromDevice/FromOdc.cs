// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FromOdc.cs
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;

namespace Aaru.CommonTypes
{
    public static partial class MediaTypeFromDevice
    {
        /// <summary>Gets the device type from a SCSI Optical Device</summary>
        /// <param name="mediumType">Medium type from MODE SENSE</param>
        /// <param name="blocks">Number of blocks in device</param>
        /// <param name="blockSize">Size in bytes of a block</param>
        /// <returns>Media type</returns>
        static MediaType GetFromOdc(byte mediumType, ulong blocks, uint blockSize)
        {
            if(mediumType != 0x01 &&
               mediumType != 0x02 &&
               mediumType != 0x03 &&
               mediumType != 0x05 &&
               mediumType != 0x07)
            {
                AaruConsole.DebugWriteLine("Media detection",
                                           "SCSI medium type is {0:X2}h, setting media type to unknown magneto-optical.",
                                           mediumType);

                return MediaType.UnknownMO;
            }

            switch(blockSize)
            {
                case 512:
                {
                    switch(blocks)
                    {
                        case 248826:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-154 / ISO 10090 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_154;
                        case 429975:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-201 / ISO 13963 conforming 3½\" embossed magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_201_ROM;
                        case 446325:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-201 / ISO 13963 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_201;
                        case 694929:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-223 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_223_512;
                        case 904995:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-183 / ISO 13481 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_183_512;
                        case 1041500:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 15041 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_15041_512;
                        case 1128772:
                        case 1163337:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-184 / ISO 13549 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_184_512;
                        case 1281856:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to WORM PD-650.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.PD650_WORM;
                        case 1298496:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to PD-650.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.PD650;
                        case 1644581:
                        case 1647371:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-195 / ISO 13842 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_195_512;
                        case 2244958:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 14517 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_14517_512;
                        default:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to unknown magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UnknownMO;
                    }
                }

                case 1024:
                {
                    switch(blocks)
                    {
                        case 314569:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 10089 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_10089;
                        case 371371:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-223 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_223;
                        case 498526:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-184 / ISO 13549 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_183;
                        case 603466:
                        case 637041:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-184 / ISO 13549 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_184;
                        case 936921:
                        case 948770:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-195 / ISO 13842 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_195;
                        case 1244621:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-238 / ISO 15486 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_238;
                        case 1273011:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 14517 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_14517;
                        case 2319786:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 15286 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_15286_1024;
                        case 4383356:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-322 / ISO 22092 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_322_1k;
                        case 14476734:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-260 / ISO 15898 conforming 356mm magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_260;
                        case 24445990:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-260 / ISO 15898 conforming 356mm magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_260_Double;
                        default:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to unknown magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UnknownMO;
                    }
                }

                case 2048:
                {
                    switch(blocks)
                    {
                        case 310352: // Found in real media
                        case 318988:
                        case 320332:
                        case 321100:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-239 / ISO 15498 conforming 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_239;
                        case 605846:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to GigaMO 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.GigaMo;
                        case 1063146:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to GigaMO 2 3½\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.GigaMo2;
                        case 1128134:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-280 / ISO 18093 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_280;
                        case 1263472:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ISO 15286 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ISO_15286;
                        case 2043664:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-322 / ISO 22092 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_322_2k;
                        case 7355716:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-317 / ISO 20162 conforming 300mm magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_317;
                        default:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to unknown magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UnknownMO;
                    }
                }

                case 4096:
                {
                    switch(blocks)
                    {
                        case 1095840:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to ECMA-322 / ISO 22092 conforming 5¼\" magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.ECMA_322;
                        default:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to unknown magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UnknownMO;
                    }
                }

                case 8192:
                {
                    switch(blocks)
                    {
                        case 1834348:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to UDO.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UDO;
                        case 3668759:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to WORM UDO2.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UDO2_WORM;
                        case 3669724:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to UDO2.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UDO2;
                        default:
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to unknown magneto-optical.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.UnknownMO;
                    }
                }

                default:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to unknown magneto-optical.",
                                               mediumType, blocks, blockSize);

                    return MediaType.UnknownMO;
            }
        }
    }
}