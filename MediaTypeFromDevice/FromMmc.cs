// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FromMmc.cs
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
    /// <summary>
    /// Gets the media type from a real device
    /// </summary>
    public static partial class MediaTypeFromDevice
    {
        /// <summary>
        /// Gets the media type from an SCSI MultiMedia Commands compliant device
        /// </summary>
        /// <param name="model">Model string</param>
        /// <param name="mediumType">Medium type from MODE SENSE</param>
        /// <param name="densityCode">Density code from MODE SENSE</param>
        /// <param name="blocks">Number of blocks in media</param>
        /// <param name="blockSize">Size of a block in bytes</param>
        /// <param name="isUsb">Is the device USB attached</param>
        /// <param name="opticalDisc">Is the media an optical disc</param>
        /// <returns>Media type</returns>
        static MediaType GetFromMmc(string model, byte mediumType, byte densityCode, ulong blocks, uint blockSize,
                                    bool isUsb, bool opticalDisc)
        {
            switch(mediumType)
            {
                case 0x00:
                    if(blockSize == 512)
                        if(blocks == 1281856)
                        {
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to WORM PD-650.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.PD650_WORM;
                        }
                        else
                        {
                            AaruConsole.DebugWriteLine("Media detection",
                                                       "SCSI medium type is {0:X2}h, media has {1} blocks of {2} bytes, setting media type to PD-650.",
                                                       mediumType, blocks, blockSize);

                            return MediaType.PD650;
                        }
                    else
                    {
                        AaruConsole.DebugWriteLine("Media detection",
                                                   "SCSI medium type is {0:X2}h, setting media type to Compact Disc.",
                                                   mediumType);

                        return MediaType.CD;
                    }
                case 0x01:
                case 0x05:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, setting media type to CD-ROM.",
                                               mediumType);

                    return MediaType.CDROM;
                case 0x02:
                case 0x06:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, setting media type to Compact Disc Digital Audio.",
                                               mediumType);

                    return MediaType.CDDA;
                case 0x03:
                case 0x07:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, setting media type to CD+.", mediumType);

                    return MediaType.CDPLUS;
                case 0x04:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, setting media type to Photo CD.",
                                               mediumType);

                    return MediaType.PCD;
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17:
                case 0x18:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, setting media type to CD-R.", mediumType);

                    return MediaType.CDR;
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                case 0x24:
                case 0x25:
                case 0x26:
                case 0x27:
                case 0x28:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h, setting media type to CD-RW.", mediumType);

                    return MediaType.CDRW;
                case 0x40 when isUsb && !opticalDisc:
                case 0x41 when isUsb && !opticalDisc:
                case 0x42 when isUsb && !opticalDisc:
                    AaruConsole.DebugWriteLine("Media detection",
                                               "SCSI medium type is {0:X2}h and device is USB, setting media type to Flash Drive.",
                                               mediumType);

                    return MediaType.FlashDrive;
                case 0x80:
                    if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                        switch(densityCode)
                        {
                            case 0x42:
                                AaruConsole.DebugWriteLine("Media detection",
                                                           "SCSI medium type is {0:X2}h, density code is {1:X2}h, drive starts with \"ult\", setting media type to LTO-2.",
                                                           mediumType, densityCode);

                                return MediaType.LTO2;
                            case 0x44:
                                AaruConsole.DebugWriteLine("Media detection",
                                                           "SCSI medium type is {0:X2}h, density code is {1:X2}h, drive starts with \"ult\", setting media type to LTO-2.",
                                                           mediumType, densityCode);

                                return MediaType.LTO3;
                            case 0x46:
                                AaruConsole.DebugWriteLine("Media detection",
                                                           "SCSI medium type is {0:X2}h, density code is {1:X2}h, drive starts with \"ult\", setting media type to LTO-2.",
                                                           mediumType, densityCode);

                                return MediaType.LTO4;
                            case 0x58:
                                AaruConsole.DebugWriteLine("Media detection",
                                                           "SCSI medium type is {0:X2}h, density code is {1:X2}h, drive starts with \"ult\", setting media type to LTO-2.",
                                                           mediumType, densityCode);

                                return MediaType.LTO5;
                        }

                    break;
            }

            return MediaType.Unknown;
        }
    }
}