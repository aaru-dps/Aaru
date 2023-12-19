// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FromScsi.cs
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
    const string MODULE_NAME = "Media detection";

    /// <summary>Tries to guess, from SCSI information, the media type of a device and/or its inserted media</summary>
    /// <param name="scsiPeripheralType">The SCSI Peripheral Type as indicated in the INQUIRY response</param>
    /// <param name="vendor">The vendor string of the device</param>
    /// <param name="model">The model string of the device</param>
    /// <param name="mediumType">The medium type byte from MODE SENSE</param>
    /// <param name="densityCode">The density type byte from MODE SENSE</param>
    /// <param name="blocks">How many blocks are on the media</param>
    /// <param name="blockSize">Size in bytes of each block</param>
    /// <param name="isUsb">Device is USB</param>
    /// <param name="opticalDisc">Is media an optical disc?</param>
    /// <returns>The media type</returns>
    public static MediaType GetFromScsi(byte scsiPeripheralType, string vendor, string model, byte mediumType,
                                        byte densityCode, ulong blocks, uint blockSize, bool isUsb, bool opticalDisc)
    {
        switch(scsiPeripheralType)
        {
            // Direct access device
            case 0x00:
            // Simplified access device
            case 0x0E:
                if(mediumType is 0x03 or 0x05 or 0x07)
                    goto case 0x07;

                return GetFromSbc(vendor, model, mediumType, blocks, blockSize);

            // Sequential access device
            case 0x01:
                return GetFromSsc(vendor, model, mediumType, densityCode);

            // Write-once device
            case 0x04:
            // Optical device
            case 0x07:
                return GetFromOdc(mediumType, blocks, blockSize);

            // MultiMedia Device
            case 0x05:
                return GetFromMmc(model, mediumType, densityCode, blocks, blockSize, isUsb, opticalDisc);

            // MD DATA drives
            case 0x10 when model.StartsWith("MDM", StringComparison.Ordinal) ||
                           model.StartsWith("MDH", StringComparison.Ordinal):
                if(blockSize == 2048)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_MDDATA,
                                               scsiPeripheralType, blocks, blockSize);

                    return MediaType.MDData;
                }

                switch(blocks)
                {
                    case 57312:
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_MD60_MDDATA,
                                                   scsiPeripheralType, blocks, blockSize);

                        return MediaType.MD60;
                    case 70464:
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_MD74_MDDATA,
                                                   scsiPeripheralType, blocks, blockSize);

                        return MediaType.MD74;
                    case 76096:
                        AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_MD80_MDDATA,
                                                   scsiPeripheralType, blocks, blockSize);

                        return MediaType.MD80;
                }

                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_MD60_MDDATA,
                                           scsiPeripheralType, blocks, blockSize);

                return MediaType.MD;

            // Host managed zoned block device
            case 0x14:
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.SCSI_Media_Type_Description_ZBC_Host_Managed,
                                           scsiPeripheralType, blocks, blockSize);

                return MediaType.Zone_HDD;
        }

        return MediaType.Unknown;
    }
}