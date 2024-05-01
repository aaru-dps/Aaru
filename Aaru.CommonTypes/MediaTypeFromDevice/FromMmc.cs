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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.Console;

namespace Aaru.CommonTypes;

/// <summary>Gets the media type from a real device</summary>
public static partial class MediaTypeFromDevice
{
    /// <summary>Gets the media type from an SCSI MultiMedia Commands compliant device</summary>
    /// <param name="model">Model string</param>
    /// <param name="mediumType">Medium type from MODE SENSE</param>
    /// <param name="densityCode">Density code from MODE SENSE</param>
    /// <param name="blocks">Number of blocks in media</param>
    /// <param name="blockSize">Size of a block in bytes</param>
    /// <param name="isUsb">Is the device USB attached</param>
    /// <param name="opticalDisc">Is the media an optical disc</param>
    /// <returns>Media type</returns>
    static MediaType GetFromMmc(string model, byte mediumType, byte densityCode, ulong blocks, uint blockSize,
                                bool   isUsb, bool opticalDisc)
    {
        switch(mediumType)
        {
            case 0x00:
                if(blockSize == 512)
                {
                    if(blocks == 1281856)
                    {
                        AaruConsole.DebugWriteLine(MODULE_NAME,
                                                   Localization
                                                      .SCSI_medium_type_is_0_media_has_1_blocks_of_2_bytes_setting_media_type_to_WORM_PD_650,
                                                   mediumType,
                                                   blocks,
                                                   blockSize);

                        return MediaType.PD650_WORM;
                    }

                    AaruConsole.DebugWriteLine(MODULE_NAME,
                                               Localization
                                                  .SCSI_medium_type_is_0_media_has_1_blocks_of_2_bytes_setting_media_type_to_PD_650,
                                               mediumType,
                                               blocks,
                                               blockSize);

                    return MediaType.PD650;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_medium_type_is_0_setting_media_type_to_Compact_Disc,
                                           mediumType);

                return MediaType.CD;
            case 0x01:
            case 0x05:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_medium_type_is_0_setting_media_type_to_CD_ROM,
                                           mediumType);

                return MediaType.CDROM;
            case 0x02:
            case 0x06:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization
                                              .SCSI_medium_type_is_0_setting_media_type_to_Compact_Disc_Digital_Audio,
                                           mediumType);

                return MediaType.CDDA;
            case 0x03:
            case 0x07:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_medium_type_is_0_setting_media_type_to_CD_Plus,
                                           mediumType);

                return MediaType.CDPLUS;
            case 0x04:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_medium_type_is_0_setting_media_type_to_Photo_CD,
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
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_medium_type_is_0_setting_media_type_to_CDR,
                                           mediumType);

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
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.SCSI_medium_type_is_0_setting_media_type_to_CDRW,
                                           mediumType);

                return MediaType.CDRW;
            case 0x40 when isUsb && !opticalDisc:
            case 0x41 when isUsb && !opticalDisc:
            case 0x42 when isUsb && !opticalDisc:
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization
                                              .SCSI_medium_type_is_0_and_device_is_USB_setting_media_type_to_Flash_Drive,
                                           mediumType);

                return MediaType.FlashDrive;
            case 0x80:
                if(model.ToLowerInvariant().StartsWith("ult", StringComparison.Ordinal))
                {
                    switch(densityCode)
                    {
                        case 0x42:
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization
                                                          .SCSI_medium_type_is_0_density_code_is_1_drive_starts_with_ult_setting_media_type_to_LTO2,
                                                       mediumType,
                                                       densityCode);

                            return MediaType.LTO2;
                        case 0x44:
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization
                                                          .SCSI_medium_type_is_0_density_code_is_1_drive_starts_with_ult_setting_media_type_to_LTO3,
                                                       mediumType,
                                                       densityCode);

                            return MediaType.LTO3;
                        case 0x46:
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization
                                                          .SCSI_medium_type_is_0_density_code_is_1_drive_starts_with_ult_setting_media_type_to_LTO4,
                                                       mediumType,
                                                       densityCode);

                            return MediaType.LTO4;
                        case 0x58:
                            AaruConsole.DebugWriteLine(MODULE_NAME,
                                                       Localization
                                                          .SCSI_medium_type_is_0_density_code_is_1_drive_starts_with_ult_setting_media_type_to_LTO5,
                                                       mediumType,
                                                       densityCode);

                            return MediaType.LTO5;
                    }
                }

                break;
        }

        return MediaType.Unknown;
    }
}