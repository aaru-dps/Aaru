// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Ata.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes ATA information from reports.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using DiscImageChef.CommonTypes.Metadata;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Decoders.SCSI;

namespace DiscImageChef.Server
{
    public static class Ata
    {
        /// <summary>
        ///     Takes the ATA part of a device report and prints it as a list of values and another list of key=value pairs to be
        ///     sequenced by ASP.NET in the rendering
        /// </summary>
        /// <param name="ataReport">ATA part of a device report</param>
        /// <param name="cfa"><c>true</c> if compact flash device</param>
        /// <param name="atapi"><c>true</c> if atapi device</param>
        /// <param name="removable"><c>true</c> if removable device</param>
        /// <param name="ataOneValue">List to put values on</param>
        /// <param name="ataTwoValue">List to put key=value pairs on</param>
        /// <param name="testedMedia">List of tested media</param>
        public static void Report(CommonTypes.Metadata.Ata ataReport, bool cfa, bool atapi,
                                  ref bool                 removable,
                                  ref List<string>         ataOneValue, ref Dictionary<string, string> ataTwoValue,
                                  ref List<TestedMedia>    testedMedia)
        {
            uint logicalsectorsize = 0;

            Identify.IdentifyDevice? ataIdentifyNullable = Identify.Decode(ataReport.Identify);
            if(!ataIdentifyNullable.HasValue) return;

            Identify.IdentifyDevice ataIdentify = ataIdentifyNullable.Value;

            if(!string.IsNullOrEmpty(ataIdentify.Model)) ataTwoValue.Add("Model", ataIdentify.Model);
            if(!string.IsNullOrEmpty(ataIdentify.FirmwareRevision))
                ataTwoValue.Add("Firmware revision", ataIdentify.FirmwareRevision);
            if(!string.IsNullOrEmpty(ataIdentify.AdditionalPID))
                ataTwoValue.Add("Additional product ID", ataIdentify.AdditionalPID);

            bool ata1 = false,
                 ata2 = false,
                 ata3 = false,
                 ata4 = false,
                 ata5 = false,
                 ata6 = false,
                 ata7 = false,
                 acs  = false,
                 acs2 = false,
                 acs3 = false,
                 acs4 = false;

            if((ushort)ataIdentify.MajorVersion == 0x0000 || (ushort)ataIdentify.MajorVersion == 0xFFFF)
            {
                // Obsolete in ATA-2, if present, device supports ATA-1
                ata1 |= ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.FastIDE) ||
                        ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.SlowIDE) ||
                        ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.UltraFastIDE);

                ata2 |= ataIdentify.ExtendedIdentify.HasFlag(Identify.ExtendedIdentifyBit.Words54to58Valid) ||
                        ataIdentify.ExtendedIdentify.HasFlag(Identify.ExtendedIdentifyBit.Words64to70Valid) ||
                        ataIdentify.ExtendedIdentify.HasFlag(Identify.ExtendedIdentifyBit.Word88Valid);

                if(!ata1 && !ata2 && !atapi && !cfa) ata2 = true;

                ata4 |= atapi;
                ata3 |= cfa;

                if(cfa && ata1) ata1 = false;
                if(cfa && ata2) ata2 = false;
            }
            else
            {
                ata1 |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.Ata1);
                ata2 |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.Ata2);
                ata3 |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.Ata3);
                ata4 |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.AtaAtapi4);
                ata5 |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.AtaAtapi5);
                ata6 |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.AtaAtapi6);
                ata7 |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.AtaAtapi7);
                acs  |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.Ata8ACS);
                acs2 |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.ACS2);
                acs3 |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.ACS3);
                acs4 |= ataIdentify.MajorVersion.HasFlag(Identify.MajorVersionBit.ACS4);
            }

            int    maxatalevel = 0;
            int    minatalevel = 255;
            string tmpString   = "";
            if(ata1)
            {
                tmpString   += "ATA-1 ";
                maxatalevel =  1;
                if(minatalevel > 1) minatalevel = 1;
            }

            if(ata2)
            {
                tmpString   += "ATA-2 ";
                maxatalevel =  2;
                if(minatalevel > 2) minatalevel = 2;
            }

            if(ata3)
            {
                tmpString   += "ATA-3 ";
                maxatalevel =  3;
                if(minatalevel > 3) minatalevel = 3;
            }

            if(ata4)
            {
                tmpString   += "ATA/ATAPI-4 ";
                maxatalevel =  4;
                if(minatalevel > 4) minatalevel = 4;
            }

            if(ata5)
            {
                tmpString   += "ATA/ATAPI-5 ";
                maxatalevel =  5;
                if(minatalevel > 5) minatalevel = 5;
            }

            if(ata6)
            {
                tmpString   += "ATA/ATAPI-6 ";
                maxatalevel =  6;
                if(minatalevel > 6) minatalevel = 6;
            }

            if(ata7)
            {
                tmpString   += "ATA/ATAPI-7 ";
                maxatalevel =  7;
                if(minatalevel > 7) minatalevel = 7;
            }

            if(acs)
            {
                tmpString   += "ATA8-ACS ";
                maxatalevel =  8;
                if(minatalevel > 8) minatalevel = 8;
            }

            if(acs2)
            {
                tmpString   += "ATA8-ACS2 ";
                maxatalevel =  9;
                if(minatalevel > 9) minatalevel = 9;
            }

            if(acs3)
            {
                tmpString   += "ATA8-ACS3 ";
                maxatalevel =  10;
                if(minatalevel > 10) minatalevel = 10;
            }

            if(acs4)
            {
                tmpString   += "ATA8-ACS4 ";
                maxatalevel =  11;
                if(minatalevel > 11) minatalevel = 11;
            }

            if(tmpString != "") ataTwoValue.Add("Supported ATA versions", tmpString);

            if(maxatalevel >= 3)
            {
                switch(ataIdentify.MinorVersion)
                {
                    case 0x0000:
                    case 0xFFFF:
                        tmpString = "Minor ATA version not specified";
                        break;
                    case 0x0001:
                        tmpString = "ATA (ATA-1) X3T9.2 781D prior to revision 4";
                        break;
                    case 0x0002:
                        tmpString = "ATA-1 published, ANSI X3.221-1994";
                        break;
                    case 0x0003:
                        tmpString = "ATA (ATA-1) X3T9.2 781D revision 4";
                        break;
                    case 0x0004:
                        tmpString = "ATA-2 published, ANSI X3.279-1996";
                        break;
                    case 0x0005:
                        tmpString = "ATA-2 X3T10 948D prior to revision 2k";
                        break;
                    case 0x0006:
                        tmpString = "ATA-3 X3T10 2008D revision 1";
                        break;
                    case 0x0007:
                        tmpString = "ATA-2 X3T10 948D revision 2k";
                        break;
                    case 0x0008:
                        tmpString = "ATA-3 X3T10 2008D revision 0";
                        break;
                    case 0x0009:
                        tmpString = "ATA-2 X3T10 948D revision 3";
                        break;
                    case 0x000A:
                        tmpString = "ATA-3 published, ANSI X3.298-1997";
                        break;
                    case 0x000B:
                        tmpString = "ATA-3 X3T10 2008D revision 6";
                        break;
                    case 0x000C:
                        tmpString = "ATA-3 X3T13 2008D revision 7";
                        break;
                    case 0x000D:
                        tmpString = "ATA/ATAPI-4 X3T13 1153D revision 6";
                        break;
                    case 0x000E:
                        tmpString = "ATA/ATAPI-4 T13 1153D revision 13";
                        break;
                    case 0x000F:
                        tmpString = "ATA/ATAPI-4 X3T13 1153D revision 7";
                        break;
                    case 0x0010:
                        tmpString = "ATA/ATAPI-4 T13 1153D revision 18";
                        break;
                    case 0x0011:
                        tmpString = "ATA/ATAPI-4 T13 1153D revision 15";
                        break;
                    case 0x0012:
                        tmpString = "ATA/ATAPI-4 published, ANSI INCITS 317-1998";
                        break;
                    case 0x0013:
                        tmpString = "ATA/ATAPI-5 T13 1321D revision 3";
                        break;
                    case 0x0014:
                        tmpString = "ATA/ATAPI-4 T13 1153D revision 14";
                        break;
                    case 0x0015:
                        tmpString = "ATA/ATAPI-5 T13 1321D revision 1";
                        break;
                    case 0x0016:
                        tmpString = "ATA/ATAPI-5 published, ANSI INCITS 340-2000";
                        break;
                    case 0x0017:
                        tmpString = "ATA/ATAPI-4 T13 1153D revision 17";
                        break;
                    case 0x0018:
                        tmpString = "ATA/ATAPI-6 T13 1410D revision 0";
                        break;
                    case 0x0019:
                        tmpString = "ATA/ATAPI-6 T13 1410D revision 3a";
                        break;
                    case 0x001A:
                        tmpString = "ATA/ATAPI-7 T13 1532D revision 1";
                        break;
                    case 0x001B:
                        tmpString = "ATA/ATAPI-6 T13 1410D revision 2";
                        break;
                    case 0x001C:
                        tmpString = "ATA/ATAPI-6 T13 1410D revision 1";
                        break;
                    case 0x001D:
                        tmpString = "ATA/ATAPI-7 published ANSI INCITS 397-2005";
                        break;
                    case 0x001E:
                        tmpString = "ATA/ATAPI-7 T13 1532D revision 0";
                        break;
                    case 0x001F:
                        tmpString = "ACS-3 Revision 3b";
                        break;
                    case 0x0021:
                        tmpString = "ATA/ATAPI-7 T13 1532D revision 4a";
                        break;
                    case 0x0022:
                        tmpString = "ATA/ATAPI-6 published, ANSI INCITS 361-2002";
                        break;
                    case 0x0027:
                        tmpString = "ATA8-ACS revision 3c";
                        break;
                    case 0x0028:
                        tmpString = "ATA8-ACS revision 6";
                        break;
                    case 0x0029:
                        tmpString = "ATA8-ACS revision 4";
                        break;
                    case 0x0031:
                        tmpString = "ACS-2 Revision 2";
                        break;
                    case 0x0033:
                        tmpString = "ATA8-ACS Revision 3e";
                        break;
                    case 0x0039:
                        tmpString = "ATA8-ACS Revision 4c";
                        break;
                    case 0x0042:
                        tmpString = "ATA8-ACS Revision 3f";
                        break;
                    case 0x0052:
                        tmpString = "ATA8-ACS revision 3b";
                        break;
                    case 0x006D:
                        tmpString = "ACS-3 Revision 5";
                        break;
                    case 0x0082:
                        tmpString = "ACS-2 published, ANSI INCITS 482-2012";
                        break;
                    case 0x0107:
                        tmpString = "ATA8-ACS revision 2d";
                        break;
                    case 0x0110:
                        tmpString = "ACS-2 Revision 3";
                        break;
                    case 0x011B:
                        tmpString = "ACS-3 Revision 4";
                        break;
                    default:
                        tmpString = $"Unknown ATA revision 0x{ataIdentify.MinorVersion:X4}";
                        break;
                }

                ataTwoValue.Add("Maximum ATA revision supported", tmpString);
            }

            tmpString = "";
            switch((ataIdentify.TransportMajorVersion & 0xF000) >> 12)
            {
                case 0x0:
                    if((ataIdentify.TransportMajorVersion & 0x0002) == 0x0002) tmpString += "ATA/ATAPI-7 ";
                    if((ataIdentify.TransportMajorVersion & 0x0001) == 0x0001) tmpString += "ATA8-APT ";
                    ataTwoValue.Add("Parallel ATA device", tmpString);
                    break;
                case 0x1:
                    if((ataIdentify.TransportMajorVersion & 0x0001) == 0x0001) tmpString += "ATA8-AST ";
                    if((ataIdentify.TransportMajorVersion & 0x0002) == 0x0002) tmpString += "SATA 1.0a ";
                    if((ataIdentify.TransportMajorVersion & 0x0004) == 0x0004) tmpString += "SATA II Extensions ";
                    if((ataIdentify.TransportMajorVersion & 0x0008) == 0x0008) tmpString += "SATA 2.5 ";
                    if((ataIdentify.TransportMajorVersion & 0x0010) == 0x0010) tmpString += "SATA 2.6 ";
                    if((ataIdentify.TransportMajorVersion & 0x0020) == 0x0020) tmpString += "SATA 3.0 ";
                    if((ataIdentify.TransportMajorVersion & 0x0040) == 0x0040) tmpString += "SATA 3.1 ";
                    ataTwoValue.Add("Serial ATA device: ", tmpString);
                    break;
                case 0xE:
                    ataTwoValue.Add("SATA Express device", "No version");
                    break;
                default:
                    ataTwoValue.Add("Unknown transport type",
                                    $"0x{(ataIdentify.TransportMajorVersion & 0xF000) >> 12:X1}");
                    break;
            }

            if(atapi)
            {
                // Bits 12 to 8, SCSI Peripheral Device Type
                switch((PeripheralDeviceTypes)(((ushort)ataIdentify.GeneralConfiguration & 0x1F00) >> 8))
                {
                    case PeripheralDeviceTypes.DirectAccess: //0x00,
                        ataOneValue.Add("ATAPI Direct-access device");
                        break;
                    case PeripheralDeviceTypes.SequentialAccess: //0x01,
                        ataOneValue.Add("ATAPI Sequential-access device");
                        break;
                    case PeripheralDeviceTypes.PrinterDevice: //0x02,
                        ataOneValue.Add("ATAPI Printer device");
                        break;
                    case PeripheralDeviceTypes.ProcessorDevice: //0x03,
                        ataOneValue.Add("ATAPI Processor device");
                        break;
                    case PeripheralDeviceTypes.WriteOnceDevice: //0x04,
                        ataOneValue.Add("ATAPI Write-once device");
                        break;
                    case PeripheralDeviceTypes.MultiMediaDevice: //0x05,
                        ataOneValue.Add("ATAPI CD-ROM/DVD/etc device");
                        break;
                    case PeripheralDeviceTypes.ScannerDevice: //0x06,
                        ataOneValue.Add("ATAPI Scanner device");
                        break;
                    case PeripheralDeviceTypes.OpticalDevice: //0x07,
                        ataOneValue.Add("ATAPI Optical memory device");
                        break;
                    case PeripheralDeviceTypes.MediumChangerDevice: //0x08,
                        ataOneValue.Add("ATAPI Medium change device");
                        break;
                    case PeripheralDeviceTypes.CommsDevice: //0x09,
                        ataOneValue.Add("ATAPI Communications device");
                        break;
                    case PeripheralDeviceTypes.PrePressDevice1: //0x0A,
                        ataOneValue.Add("ATAPI Graphics arts pre-press device (defined in ASC IT8)");
                        break;
                    case PeripheralDeviceTypes.PrePressDevice2: //0x0B,
                        ataOneValue.Add("ATAPI Graphics arts pre-press device (defined in ASC IT8)");
                        break;
                    case PeripheralDeviceTypes.ArrayControllerDevice: //0x0C,
                        ataOneValue.Add("ATAPI Array controller device");
                        break;
                    case PeripheralDeviceTypes.EnclosureServiceDevice: //0x0D,
                        ataOneValue.Add("ATAPI Enclosure services device");
                        break;
                    case PeripheralDeviceTypes.SimplifiedDevice: //0x0E,
                        ataOneValue.Add("ATAPI Simplified direct-access device");
                        break;
                    case PeripheralDeviceTypes.OCRWDevice: //0x0F,
                        ataOneValue.Add("ATAPI Optical card reader/writer device");
                        break;
                    case PeripheralDeviceTypes.BridgingExpander: //0x10,
                        ataOneValue.Add("ATAPI Bridging Expanders");
                        break;
                    case PeripheralDeviceTypes.ObjectDevice: //0x11,
                        ataOneValue.Add("ATAPI Object-based Storage Device");
                        break;
                    case PeripheralDeviceTypes.ADCDevice: //0x12,
                        ataOneValue.Add("ATAPI Automation/Drive Interface");
                        break;
                    case PeripheralDeviceTypes.WellKnownDevice: //0x1E,
                        ataOneValue.Add("ATAPI Well known logical unit");
                        break;
                    case PeripheralDeviceTypes.UnknownDevice: //0x1F
                        ataOneValue.Add("ATAPI Unknown or no device type");
                        break;
                    default:
                        ataOneValue
                           .Add($"ATAPI Unknown device type field value 0x{((ushort)ataIdentify.GeneralConfiguration & 0x1F00) >> 8:X2}");
                        break;
                }

                // ATAPI DRQ behaviour
                switch(((ushort)ataIdentify.GeneralConfiguration & 0x60) >> 5)
                {
                    case 0:
                        ataOneValue.Add("Device shall set DRQ within 3 ms of receiving PACKET");
                        break;
                    case 1:
                        ataOneValue.Add("Device shall assert INTRQ when DRQ is set to one");
                        break;
                    case 2:
                        ataOneValue.Add("Device shall set DRQ within 50 µs of receiving PACKET");
                        break;
                    default:
                        ataOneValue
                           .Add($"Unknown ATAPI DRQ behaviour code {((ushort)ataIdentify.GeneralConfiguration & 0x60) >> 5}");
                        break;
                }

                // ATAPI PACKET size
                switch((ushort)ataIdentify.GeneralConfiguration & 0x03)
                {
                    case 0:
                        ataOneValue.Add("ATAPI device uses 12 byte command packet");
                        break;
                    case 1:
                        ataOneValue.Add("ATAPI device uses 16 byte command packet");
                        break;
                    default:
                        ataOneValue
                           .Add($"Unknown ATAPI packet size code {(ushort)ataIdentify.GeneralConfiguration & 0x03}");
                        break;
                }
            }
            else if(!cfa)
            {
                if(minatalevel >= 5)
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.IncompleteResponse))
                        ataOneValue.Add("Incomplete identify response");
                if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.NonMagnetic))
                    ataOneValue.Add("Device uses non-magnetic media");

                if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.Removable))
                    ataOneValue.Add("Device is removable");

                if(minatalevel <= 5)
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.Fixed))
                        ataOneValue.Add("Device is fixed");

                if(ata1)
                {
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.SlowIDE))
                        ataOneValue.Add("Device transfer rate is <= 5 Mb/s");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.FastIDE))
                        ataOneValue.Add("Device transfer rate is > 5 Mb/s but <= 10 Mb/s");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.UltraFastIDE))
                        ataOneValue.Add("Device transfer rate is > 10 Mb/s");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.SoftSector))
                        ataOneValue.Add("Device is soft sectored");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.HardSector))
                        ataOneValue.Add("Device is hard sectored");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.NotMFM))
                        ataOneValue.Add("Device is not MFM encoded");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.FormatGapReq))
                        ataOneValue.Add("Format speed tolerance gap is required");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.TrackOffset))
                        ataOneValue.Add("Track offset option is available");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.DataStrobeOffset))
                        ataOneValue.Add("Data strobe offset option is available");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit
                                                                        .RotationalSpeedTolerance))
                        ataOneValue.Add("Rotational speed tolerance is higher than 0,5%");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.SpindleControl))
                        ataOneValue.Add("Spindle motor control is implemented");
                    if(ataIdentify.GeneralConfiguration.HasFlag(Identify.GeneralConfigurationBit.HighHeadSwitch))
                        ataOneValue.Add("Head switch time is bigger than 15 µs.");
                }
            }

            if((ushort)ataIdentify.SpecificConfiguration != 0x0000 &&
               (ushort)ataIdentify.SpecificConfiguration != 0xFFFF)
                switch(ataIdentify.SpecificConfiguration)
                {
                    case Identify.SpecificConfigurationEnum.RequiresSetIncompleteResponse:
                        ataOneValue
                           .Add("Device requires SET FEATURES to spin up and IDENTIFY DEVICE response is incomplete.");
                        break;
                    case Identify.SpecificConfigurationEnum.RequiresSetCompleteResponse:
                        ataOneValue
                           .Add("Device requires SET FEATURES to spin up and IDENTIFY DEVICE response is complete.");
                        break;
                    case Identify.SpecificConfigurationEnum.NotRequiresSetIncompleteResponse:
                        ataOneValue
                           .Add("Device does not require SET FEATURES to spin up and IDENTIFY DEVICE response is incomplete.");
                        break;
                    case Identify.SpecificConfigurationEnum.NotRequiresSetCompleteResponse:
                        ataOneValue
                           .Add("Device does not require SET FEATURES to spin up and IDENTIFY DEVICE response is complete.");
                        break;
                    default:
                        ataOneValue
                           .Add($"Unknown device specific configuration 0x{(ushort)ataIdentify.SpecificConfiguration:X4}");
                        break;
                }

            // Obsolete since ATA-2, however, it is yet used in ATA-8 devices
            if(ataIdentify.BufferSize != 0x0000 && ataIdentify.BufferSize != 0xFFFF &&
               ataIdentify.BufferType != 0x0000 && ataIdentify.BufferType != 0xFFFF)
                switch(ataIdentify.BufferType)
                {
                    case 1:
                        ataOneValue
                           .Add($"{ataIdentify.BufferSize * logicalsectorsize / 1024} KiB of single ported single sector buffer");
                        break;
                    case 2:
                        ataOneValue
                           .Add($"{ataIdentify.BufferSize * logicalsectorsize / 1024} KiB of dual ported multi sector buffer");
                        break;
                    case 3:
                        ataOneValue
                           .Add($"{ataIdentify.BufferSize * logicalsectorsize / 1024} KiB of dual ported multi sector buffer with read caching");
                        break;
                    default:
                        ataOneValue
                           .Add($"{ataIdentify.BufferSize * logicalsectorsize / 1024} KiB of unknown type {ataIdentify.BufferType} buffer");
                        break;
                }

            ataOneValue.Add("<i>Device capabilities:</i>");
            if(ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.StandardStanbyTimer))
                ataOneValue.Add("Standby time values are standard");
            if(ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.IORDY))
                ataOneValue.Add(ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.CanDisableIORDY)
                                    ? "IORDY is supported and can be disabled"
                                    : "IORDY is supported");
            if(ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.DMASupport))
                ataOneValue.Add("DMA is supported");
            if(ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.PhysicalAlignment1) ||
               ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.PhysicalAlignment0))
                ataOneValue.Add($"Long Physical Alignment setting is {(ushort)ataIdentify.Capabilities & 0x03}");
            if(atapi)
            {
                if(ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.InterleavedDMA))
                    ataOneValue.Add("ATAPI device supports interleaved DMA");
                if(ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.CommandQueue))
                    ataOneValue.Add("ATAPI device supports command queueing");
                if(ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.OverlapOperation))
                    ataOneValue.Add("ATAPI device supports overlapped operations");
                if(ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.RequiresATASoftReset))
                    ataOneValue.Add("ATAPI device requires ATA software reset");
            }

            if(ataIdentify.Capabilities2.HasFlag(Identify.CapabilitiesBit2.MustBeSet) &&
               !ataIdentify.Capabilities2.HasFlag(Identify.CapabilitiesBit2.MustBeClear))
                if(ataIdentify.Capabilities2.HasFlag(Identify.CapabilitiesBit2.SpecificStandbyTimer))
                    ataOneValue.Add("Device indicates a specific minimum standby timer value");

            if(ataIdentify.Capabilities3.HasFlag(Identify.CapabilitiesBit3.MultipleValid))
            {
                ataOneValue.Add($"A maximum of {ataIdentify.MultipleSectorNumber} sectors can be transferred per interrupt on READ/WRITE MULTIPLE");
                ataOneValue.Add($"Device supports setting a maximum of {ataIdentify.MultipleMaxSectors} sectors");
            }

            if(ata1)
                if(ataIdentify.TrustedComputing.HasFlag(Identify.TrustedComputingBit.TrustedComputing))
                    ataOneValue.Add("Device supports doubleword I/O");

            if(minatalevel <= 3)
            {
                if(ataIdentify.PIOTransferTimingMode > 0)
                    ataTwoValue.Add("PIO timing mode", $"{ataIdentify.PIOTransferTimingMode}");
                if(ataIdentify.DMATransferTimingMode > 0)
                    ataTwoValue.Add("DMA timing mode", $"{ataIdentify.DMATransferTimingMode}");
            }

            tmpString = "";

            if(ataIdentify.APIOSupported.HasFlag(Identify.TransferMode.Mode0)) tmpString += "PIO0 ";
            if(ataIdentify.APIOSupported.HasFlag(Identify.TransferMode.Mode1)) tmpString += "PIO1 ";
            if(ataIdentify.APIOSupported.HasFlag(Identify.TransferMode.Mode2)) tmpString += "PIO2 ";
            if(ataIdentify.APIOSupported.HasFlag(Identify.TransferMode.Mode3)) tmpString += "PIO3 ";
            if(ataIdentify.APIOSupported.HasFlag(Identify.TransferMode.Mode4)) tmpString += "PIO4 ";
            if(ataIdentify.APIOSupported.HasFlag(Identify.TransferMode.Mode5)) tmpString += "PIO5 ";
            if(ataIdentify.APIOSupported.HasFlag(Identify.TransferMode.Mode6)) tmpString += "PIO6 ";
            if(ataIdentify.APIOSupported.HasFlag(Identify.TransferMode.Mode7)) tmpString += "PIO7 ";

            if(!string.IsNullOrEmpty(tmpString)) ataTwoValue.Add("Advanced PIO", tmpString);

            if(minatalevel <= 3 && !atapi)
            {
                tmpString = "";
                if(ataIdentify.DMASupported.HasFlag(Identify.TransferMode.Mode0))
                {
                    tmpString += "DMA0 ";
                    if(ataIdentify.DMAActive.HasFlag(Identify.TransferMode.Mode0)) tmpString += "(active) ";
                }

                if(ataIdentify.DMASupported.HasFlag(Identify.TransferMode.Mode1))
                {
                    tmpString += "DMA1 ";
                    if(ataIdentify.DMAActive.HasFlag(Identify.TransferMode.Mode1)) tmpString += "(active) ";
                }

                if(ataIdentify.DMASupported.HasFlag(Identify.TransferMode.Mode2))
                {
                    tmpString += "DMA2 ";
                    if(ataIdentify.DMAActive.HasFlag(Identify.TransferMode.Mode2)) tmpString += "(active) ";
                }

                if(ataIdentify.DMASupported.HasFlag(Identify.TransferMode.Mode3))
                {
                    tmpString += "DMA3 ";
                    if(ataIdentify.DMAActive.HasFlag(Identify.TransferMode.Mode3)) tmpString += "(active) ";
                }

                if(ataIdentify.DMASupported.HasFlag(Identify.TransferMode.Mode4))
                {
                    tmpString += "DMA4 ";
                    if(ataIdentify.DMAActive.HasFlag(Identify.TransferMode.Mode4)) tmpString += "(active) ";
                }

                if(ataIdentify.DMASupported.HasFlag(Identify.TransferMode.Mode5))
                {
                    tmpString += "DMA5 ";
                    if(ataIdentify.DMAActive.HasFlag(Identify.TransferMode.Mode5)) tmpString += "(active) ";
                }

                if(ataIdentify.DMASupported.HasFlag(Identify.TransferMode.Mode6))
                {
                    tmpString += "DMA6 ";
                    if(ataIdentify.DMAActive.HasFlag(Identify.TransferMode.Mode6)) tmpString += "(active) ";
                }

                if(ataIdentify.DMASupported.HasFlag(Identify.TransferMode.Mode7))
                {
                    tmpString += "DMA7 ";
                    if(ataIdentify.DMAActive.HasFlag(Identify.TransferMode.Mode7)) tmpString += "(active) ";
                }

                if(!string.IsNullOrEmpty(tmpString)) ataTwoValue.Add("Single-word DMA", tmpString);
            }

            tmpString = "";
            if(ataIdentify.MDMASupported.HasFlag(Identify.TransferMode.Mode0))
            {
                tmpString += "MDMA0 ";
                if(ataIdentify.MDMAActive.HasFlag(Identify.TransferMode.Mode0)) tmpString += "(active) ";
            }

            if(ataIdentify.MDMASupported.HasFlag(Identify.TransferMode.Mode1))
            {
                tmpString += "MDMA1 ";
                if(ataIdentify.MDMAActive.HasFlag(Identify.TransferMode.Mode1)) tmpString += "(active) ";
            }

            if(ataIdentify.MDMASupported.HasFlag(Identify.TransferMode.Mode2))
            {
                tmpString += "MDMA2 ";
                if(ataIdentify.MDMAActive.HasFlag(Identify.TransferMode.Mode2)) tmpString += "(active) ";
            }

            if(ataIdentify.MDMASupported.HasFlag(Identify.TransferMode.Mode3))
            {
                tmpString += "MDMA3 ";
                if(ataIdentify.MDMAActive.HasFlag(Identify.TransferMode.Mode3)) tmpString += "(active) ";
            }

            if(ataIdentify.MDMASupported.HasFlag(Identify.TransferMode.Mode4))
            {
                tmpString += "MDMA4 ";
                if(ataIdentify.MDMAActive.HasFlag(Identify.TransferMode.Mode4)) tmpString += "(active) ";
            }

            if(ataIdentify.MDMASupported.HasFlag(Identify.TransferMode.Mode5))
            {
                tmpString += "MDMA5 ";
                if(ataIdentify.MDMAActive.HasFlag(Identify.TransferMode.Mode5)) tmpString += "(active) ";
            }

            if(ataIdentify.MDMASupported.HasFlag(Identify.TransferMode.Mode6))
            {
                tmpString += "MDMA6 ";
                if(ataIdentify.MDMAActive.HasFlag(Identify.TransferMode.Mode6)) tmpString += "(active) ";
            }

            if(ataIdentify.MDMASupported.HasFlag(Identify.TransferMode.Mode7))
            {
                tmpString += "MDMA7 ";
                if(ataIdentify.MDMAActive.HasFlag(Identify.TransferMode.Mode7)) tmpString += "(active) ";
            }

            if(!string.IsNullOrEmpty(tmpString)) ataTwoValue.Add("Multi-word DMA", tmpString);

            tmpString = "";
            if(ataIdentify.UDMASupported.HasFlag(Identify.TransferMode.Mode0))
            {
                tmpString += "UDMA0 ";
                if(ataIdentify.UDMAActive.HasFlag(Identify.TransferMode.Mode0)) tmpString += "(active) ";
            }

            if(ataIdentify.UDMASupported.HasFlag(Identify.TransferMode.Mode1))
            {
                tmpString += "UDMA1 ";
                if(ataIdentify.UDMAActive.HasFlag(Identify.TransferMode.Mode1)) tmpString += "(active) ";
            }

            if(ataIdentify.UDMASupported.HasFlag(Identify.TransferMode.Mode2))
            {
                tmpString += "UDMA2 ";
                if(ataIdentify.UDMAActive.HasFlag(Identify.TransferMode.Mode2)) tmpString += "(active) ";
            }

            if(ataIdentify.UDMASupported.HasFlag(Identify.TransferMode.Mode3))
            {
                tmpString += "UDMA3 ";
                if(ataIdentify.UDMAActive.HasFlag(Identify.TransferMode.Mode3)) tmpString += "(active) ";
            }

            if(ataIdentify.UDMASupported.HasFlag(Identify.TransferMode.Mode4))
            {
                tmpString += "UDMA4 ";
                if(ataIdentify.UDMAActive.HasFlag(Identify.TransferMode.Mode4)) tmpString += "(active) ";
            }

            if(ataIdentify.UDMASupported.HasFlag(Identify.TransferMode.Mode5))
            {
                tmpString += "UDMA5 ";
                if(ataIdentify.UDMAActive.HasFlag(Identify.TransferMode.Mode5)) tmpString += "(active) ";
            }

            if(ataIdentify.UDMASupported.HasFlag(Identify.TransferMode.Mode6))
            {
                tmpString += "UDMA6 ";
                if(ataIdentify.UDMAActive.HasFlag(Identify.TransferMode.Mode6)) tmpString += "(active) ";
            }

            if(ataIdentify.UDMASupported.HasFlag(Identify.TransferMode.Mode7))
            {
                tmpString += "UDMA7 ";
                if(ataIdentify.UDMAActive.HasFlag(Identify.TransferMode.Mode7)) tmpString += "(active) ";
            }

            if(!string.IsNullOrEmpty(tmpString)) ataTwoValue.Add("Ultra DMA", tmpString);

            if(ataIdentify.MinMDMACycleTime != 0 && ataIdentify.RecMDMACycleTime != 0)
                ataOneValue.Add($"At minimum {ataIdentify.MinMDMACycleTime} ns. transfer cycle time per word in MDMA, " +
                                $"{ataIdentify.RecMDMACycleTime} ns. recommended");
            if(ataIdentify.MinPIOCycleTimeNoFlow != 0)
                ataOneValue.Add($"At minimum {ataIdentify.MinPIOCycleTimeNoFlow} ns. transfer cycle time per word in PIO, " +
                                "without flow control");
            if(ataIdentify.MinPIOCycleTimeFlow != 0)
                ataOneValue.Add($"At minimum {ataIdentify.MinPIOCycleTimeFlow} ns. transfer cycle time per word in PIO, " +
                                "with IORDY flow control");

            if(ataIdentify.MaxQueueDepth != 0)
                ataOneValue.Add($"{ataIdentify.MaxQueueDepth + 1} depth of queue maximum");

            if(atapi)
            {
                if(ataIdentify.PacketBusRelease != 0)
                    ataOneValue
                       .Add($"{ataIdentify.PacketBusRelease} ns. typical to release bus from receipt of PACKET");
                if(ataIdentify.ServiceBusyClear != 0)
                    ataOneValue
                       .Add($"{ataIdentify.ServiceBusyClear} ns. typical to clear BSY bit from receipt of SERVICE");
            }

            if((ataIdentify.TransportMajorVersion & 0xF000) >> 12 == 0x1 ||
               (ataIdentify.TransportMajorVersion & 0xF000) >> 12 == 0xE)
            {
                if(!ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.Clear))
                {
                    if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.Gen1Speed))
                        ataOneValue.Add("SATA 1.5Gb/s is supported");
                    if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.Gen2Speed))
                        ataOneValue.Add("SATA 3.0Gb/s is supported");
                    if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.Gen3Speed))
                        ataOneValue.Add("SATA 6.0Gb/s is supported");
                    if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.PowerReceipt))
                        ataOneValue.Add("Receipt of host initiated power management requests is supported");
                    if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.PHYEventCounter))
                        ataOneValue.Add("PHY Event counters are supported");
                    if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.HostSlumbTrans))
                        ataOneValue.Add("Supports host automatic partial to slumber transitions is supported");
                    if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.DevSlumbTrans))
                        ataOneValue.Add("Supports device automatic partial to slumber transitions is supported");
                    if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.NCQ))
                    {
                        ataOneValue.Add("NCQ is supported");

                        if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.NCQPriority))
                            ataOneValue.Add("NCQ priority is supported");
                        if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.UnloadNCQ))
                            ataOneValue.Add("Unload is supported with outstanding NCQ commands");
                    }
                }

                if(!ataIdentify.SATACapabilities2.HasFlag(Identify.SATACapabilitiesBit2.Clear))
                {
                    if(!ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.Clear) &&
                       ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.NCQ))
                    {
                        if(ataIdentify.SATACapabilities2.HasFlag(Identify.SATACapabilitiesBit2.NCQMgmt))
                            ataOneValue.Add("NCQ queue management is supported");
                        if(ataIdentify.SATACapabilities2.HasFlag(Identify.SATACapabilitiesBit2.NCQStream))
                            ataOneValue.Add("NCQ streaming is supported");
                    }

                    if(atapi)
                    {
                        if(ataIdentify.SATACapabilities2.HasFlag(Identify.SATACapabilitiesBit2.HostEnvDetect))
                            ataOneValue.Add("ATAPI device supports host environment detection");
                        if(ataIdentify.SATACapabilities2.HasFlag(Identify.SATACapabilitiesBit2.DevAttSlimline))
                            ataOneValue.Add("ATAPI device supports attention on slimline connected devices");
                    }
                }
            }

            if(ataIdentify.InterseekDelay != 0x0000 && ataIdentify.InterseekDelay != 0xFFFF)
                ataOneValue.Add($"{ataIdentify.InterseekDelay} microseconds of interseek delay for ISO-7779 accoustic testing");

            if((ushort)ataIdentify.DeviceFormFactor != 0x0000 && (ushort)ataIdentify.DeviceFormFactor != 0xFFFF)
                switch(ataIdentify.DeviceFormFactor)
                {
                    case Identify.DeviceFormFactorEnum.FiveAndQuarter:
                        ataOneValue.Add("Device nominal size is 5.25\"");
                        break;
                    case Identify.DeviceFormFactorEnum.ThreeAndHalf:
                        ataOneValue.Add("Device nominal size is 3.5\"");
                        break;
                    case Identify.DeviceFormFactorEnum.TwoAndHalf:
                        ataOneValue.Add("Device nominal size is 2.5\"");
                        break;
                    case Identify.DeviceFormFactorEnum.OnePointEight:
                        ataOneValue.Add("Device nominal size is 1.8\"");
                        break;
                    case Identify.DeviceFormFactorEnum.LessThanOnePointEight:
                        ataOneValue.Add("Device nominal size is smaller than 1.8\"");
                        break;
                    default:
                        ataOneValue.Add($"Device nominal size field value {ataIdentify.DeviceFormFactor} is unknown");
                        break;
                }

            if(atapi)
                if(ataIdentify.ATAPIByteCount > 0)
                    ataOneValue.Add($"{ataIdentify.ATAPIByteCount} bytes count limit for ATAPI");

            if(cfa)
                if((ataIdentify.CFAPowerMode & 0x8000) == 0x8000)
                {
                    ataOneValue.Add("CompactFlash device supports power mode 1");
                    if((ataIdentify.CFAPowerMode & 0x2000) == 0x2000)
                        ataOneValue.Add("CompactFlash power mode 1 required for one or more commands");
                    if((ataIdentify.CFAPowerMode & 0x1000) == 0x1000)
                        ataOneValue.Add("CompactFlash power mode 1 is disabled");

                    ataOneValue.Add($"CompactFlash device uses a maximum of {ataIdentify.CFAPowerMode & 0x0FFF} mA");
                }

            ataOneValue.Add("<i>Command set and features:</i>");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.Nop))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.Nop)
                                    ? "NOP is supported and enabled"
                                    : "NOP is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.ReadBuffer))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.ReadBuffer)
                                    ? "READ BUFFER is supported and enabled"
                                    : "READ BUFFER is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.WriteBuffer))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.WriteBuffer)
                                    ? "WRITE BUFFER is supported and enabled"
                                    : "WRITE BUFFER is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.HPA))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.HPA)
                                    ? "Host Protected Area is supported and enabled"
                                    : "Host Protected Area is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.DeviceReset))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.DeviceReset)
                                    ? "DEVICE RESET is supported and enabled"
                                    : "DEVICE RESET is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.Service))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.Service)
                                    ? "SERVICE interrupt is supported and enabled"
                                    : "SERVICE interrupt is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.Release))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.Release)
                                    ? "Release is supported and enabled"
                                    : "Release is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.LookAhead))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.LookAhead)
                                    ? "Look-ahead read is supported and enabled"
                                    : "Look-ahead read is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.WriteCache))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.WriteCache)
                                    ? "Write cache is supported and enabled"
                                    : "Write cache is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.Packet))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.Packet)
                                    ? "PACKET is supported and enabled"
                                    : "PACKET is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.PowerManagement))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.PowerManagement)
                                    ? "Power management is supported and enabled"
                                    : "Power management is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.RemovableMedia))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.RemovableMedia)
                                    ? "Removable media feature set is supported and enabled"
                                    : "Removable media feature set is supported");
            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.SecurityMode))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.SecurityMode)
                                    ? "Security mode is supported and enabled"
                                    : "Security mode is supported");
            if(ataIdentify.Capabilities.HasFlag(Identify.CapabilitiesBit.LBASupport))
                ataOneValue.Add("28-bit LBA is supported");

            if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.MustBeSet) &&
               !ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.MustBeClear))
            {
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.LBA48))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.LBA48)
                                        ? "48-bit LBA is supported and enabled"
                                        : "48-bit LBA is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.FlushCache))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.FlushCache)
                                        ? "FLUSH CACHE is supported and enabled"
                                        : "FLUSH CACHE is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.FlushCacheExt))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.FlushCacheExt)
                                        ? "FLUSH CACHE EXT is supported and enabled"
                                        : "FLUSH CACHE EXT is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.DCO))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.DCO)
                                        ? "Device Configuration Overlay feature set is supported and enabled"
                                        : "Device Configuration Overlay feature set is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.AAM))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.AAM)
                                        ? $"Automatic Acoustic Management is supported and enabled with value {ataIdentify.CurrentAAM} (vendor recommends {ataIdentify.RecommendedAAM}"
                                        : "Automatic Acoustic Management is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.SetMax))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.SetMax)
                                        ? "SET MAX security extension is supported and enabled"
                                        : "SET MAX security extension is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.AddressOffsetReservedAreaBoot))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2
                                                                                   .AddressOffsetReservedAreaBoot)
                                        ? "Address Offset Reserved Area Boot is supported and enabled"
                                        : "Address Offset Reserved Area Boot is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.SetFeaturesRequired))
                    ataOneValue.Add("SET FEATURES is required before spin-up");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.PowerUpInStandby))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.PowerUpInStandby)
                                        ? "Power-up in standby is supported and enabled"
                                        : "Power-up in standby is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.RemovableNotification))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2
                                                                                   .RemovableNotification)
                                        ? "Removable Media Status Notification is supported and enabled"
                                        : "Removable Media Status Notification is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.APM))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.APM)
                                        ? $"Advanced Power Management is supported and enabled with value {ataIdentify.CurrentAPM}"
                                        : "Advanced Power Management is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.CompactFlash))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.CompactFlash)
                                        ? "CompactFlash feature set is supported and enabled"
                                        : "CompactFlash feature set is supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.RWQueuedDMA))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.RWQueuedDMA)
                                        ? "READ DMA QUEUED and WRITE DMA QUEUED are supported and enabled"
                                        : "READ DMA QUEUED and WRITE DMA QUEUED are supported");
                if(ataIdentify.CommandSet2.HasFlag(Identify.CommandSetBit2.DownloadMicrocode))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet2.HasFlag(Identify.CommandSetBit2.DownloadMicrocode)
                                        ? "DOWNLOAD MICROCODE is supported and enabled"
                                        : "DOWNLOAD MICROCODE is supported");
            }

            if(ataIdentify.CommandSet.HasFlag(Identify.CommandSetBit.SMART))
                ataOneValue.Add(ataIdentify.EnabledCommandSet.HasFlag(Identify.CommandSetBit.SMART)
                                    ? "S.M.A.R.T. is supported and enabled"
                                    : "S.M.A.R.T. is supported");

            if(ataIdentify.SCTCommandTransport.HasFlag(Identify.SCTCommandTransportBit.Supported))
                ataOneValue.Add("S.M.A.R.T. Command Transport is supported");

            if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeSet) &&
               !ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeClear))
            {
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.SMARTSelfTest))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.SMARTSelfTest)
                                        ? "S.M.A.R.T. self-testing is supported and enabled"
                                        : "S.M.A.R.T. self-testing is supported");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.SMARTLog))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.SMARTLog)
                                        ? "S.M.A.R.T. error logging is supported and enabled"
                                        : "S.M.A.R.T. error logging is supported");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.IdleImmediate))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.IdleImmediate)
                                        ? "IDLE IMMEDIATE with UNLOAD FEATURE is supported and enabled"
                                        : "IDLE IMMEDIATE with UNLOAD FEATURE is supported");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.WriteURG))
                    ataOneValue.Add("URG bit is supported in WRITE STREAM DMA EXT and WRITE STREAM EXT");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.ReadURG))
                    ataOneValue.Add("URG bit is supported in READ STREAM DMA EXT and READ STREAM EXT");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.WWN))
                    ataOneValue.Add("Device has a World Wide Name");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.FUAWriteQ))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.FUAWriteQ)
                                        ? "WRITE DMA QUEUED FUA EXT is supported and enabled"
                                        : "WRITE DMA QUEUED FUA EXT is supported");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.FUAWrite))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.FUAWrite)
                                        ? "WRITE DMA FUA EXT and WRITE MULTIPLE FUA EXT are supported and enabled"
                                        : "WRITE DMA FUA EXT and WRITE MULTIPLE FUA EXT are supported");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.GPL))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.GPL)
                                        ? "General Purpose Logging is supported and enabled"
                                        : "General Purpose Logging is supported");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.Streaming))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.Streaming)
                                        ? "Streaming feature set is supported and enabled"
                                        : "Streaming feature set is supported");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.MCPT))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.MCPT)
                                        ? "Media Card Pass Through command set is supported and enabled"
                                        : "Media Card Pass Through command set is supported");
                if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.MediaSerial))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet3.HasFlag(Identify.CommandSetBit3.MediaSerial)
                                        ? "Media Serial is supported and valid"
                                        : "Media Serial is supported");
            }

            if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.MustBeSet) &&
               !ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.MustBeClear))
            {
                if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.DSN))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4.DSN)
                                        ? "DSN feature set is supported and enabled"
                                        : "DSN feature set is supported");
                if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.AMAC))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4.AMAC)
                                        ? "Accessible Max Address Configuration is supported and enabled"
                                        : "Accessible Max Address Configuration is supported");
                if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.ExtPowerCond))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4.ExtPowerCond)
                                        ? "Extended Power Conditions are supported and enabled"
                                        : "Extended Power Conditions are supported");
                if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.ExtStatusReport))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4.ExtStatusReport)
                                        ? "Extended Status Reporting is supported and enabled"
                                        : "Extended Status Reporting is supported");
                if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.FreeFallControl))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4.FreeFallControl)
                                        ? "Free-fall control feature set is supported and enabled"
                                        : "Free-fall control feature set is supported");
                if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.SegmentedDownloadMicrocode))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4
                                                                                   .SegmentedDownloadMicrocode)
                                        ? "Segmented feature in DOWNLOAD MICROCODE is supported and enabled"
                                        : "Segmented feature in DOWNLOAD MICROCODE is supported");
                if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.RWDMAExtGpl))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4.RWDMAExtGpl)
                                        ? "READ/WRITE DMA EXT GPL are supported and enabled"
                                        : "READ/WRITE DMA EXT GPL are supported");
                if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.WriteUnc))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4.WriteUnc)
                                        ? "WRITE UNCORRECTABLE is supported and enabled"
                                        : "WRITE UNCORRECTABLE is supported");
                if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.WRV))
                {
                    ataOneValue.Add(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4.WRV)
                                        ? "Write/Read/Verify is supported and enabled"
                                        : "Write/Read/Verify is supported");
                    ataOneValue.Add($"{ataIdentify.WRVSectorCountMode2} sectors for Write/Read/Verify mode 2");
                    ataOneValue.Add($"{ataIdentify.WRVSectorCountMode3} sectors for Write/Read/Verify mode 3");
                    if(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4.WRV))
                        ataOneValue.Add($"Current Write/Read/Verify mode: {ataIdentify.WRVMode}");
                }

                if(ataIdentify.CommandSet4.HasFlag(Identify.CommandSetBit4.DT1825))
                    ataOneValue.Add(ataIdentify.EnabledCommandSet4.HasFlag(Identify.CommandSetBit4.DT1825)
                                        ? "DT1825 is supported and enabled"
                                        : "DT1825 is supported");
            }

            if(true)
            {
                if(ataIdentify.Capabilities3.HasFlag(Identify.CapabilitiesBit3.BlockErase))
                    ataOneValue.Add("BLOCK ERASE EXT is supported");
                if(ataIdentify.Capabilities3.HasFlag(Identify.CapabilitiesBit3.Overwrite))
                    ataOneValue.Add("OVERWRITE EXT is supported");
                if(ataIdentify.Capabilities3.HasFlag(Identify.CapabilitiesBit3.CryptoScramble))
                    ataOneValue.Add("CRYPTO SCRAMBLE EXT is supported");
            }

            if(true)
            {
                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.DeviceConfDMA))
                    ataOneValue.Add("DEVICE CONFIGURATION IDENTIFY DMA and DEVICE CONFIGURATION SET DMA are supported");
                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.ReadBufferDMA))
                    ataOneValue.Add("READ BUFFER DMA is supported");
                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.WriteBufferDMA))
                    ataOneValue.Add("WRITE BUFFER DMA is supported");
                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.DownloadMicroCodeDMA))
                    ataOneValue.Add("DOWNLOAD MICROCODE DMA is supported");
                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.SetMaxDMA))
                    ataOneValue.Add("SET PASSWORD DMA and SET UNLOCK DMA are supported");
                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.Ata28))
                    ataOneValue.Add("Not all 28-bit commands are supported");

                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.CFast))
                    ataOneValue.Add("Device follows CFast specification");
                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.IEEE1667))
                    ataOneValue.Add("Device follows IEEE-1667");

                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.DeterministicTrim))
                {
                    ataOneValue.Add("Read after TRIM is deterministic");
                    if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.ReadZeroTrim))
                        ataOneValue.Add("Read after TRIM returns empty data");
                }

                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.LongPhysSectorAligError))
                    ataOneValue.Add("Device supports Long Physical Sector Alignment Error Reporting Control");
                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.Encrypted))
                    ataOneValue.Add("Device encrypts all user data");
                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.AllCacheNV))
                    ataOneValue.Add("Device's write cache is non-volatile");
                if(ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.ZonedBit0) ||
                   ataIdentify.CommandSet5.HasFlag(Identify.CommandSetBit5.ZonedBit1))
                    ataOneValue.Add("Device is zoned");
            }

            if(true)
                if(ataIdentify.Capabilities3.HasFlag(Identify.CapabilitiesBit3.Sanitize))
                {
                    ataOneValue.Add("Sanitize feature set is supported");
                    ataOneValue.Add(ataIdentify.Capabilities3.HasFlag(Identify.CapabilitiesBit3.SanitizeCommands)
                                        ? "Sanitize commands are specified by ACS-3 or higher"
                                        : "Sanitize commands are specified by ACS-2");

                    if(ataIdentify.Capabilities3.HasFlag(Identify.CapabilitiesBit3.SanitizeAntifreeze))
                        ataOneValue.Add("SANITIZE ANTIFREEZE LOCK EXT is supported");
                }

            if(!ata1 && maxatalevel >= 8)
                if(ataIdentify.TrustedComputing.HasFlag(Identify.TrustedComputingBit.Set)    &&
                   !ataIdentify.TrustedComputing.HasFlag(Identify.TrustedComputingBit.Clear) &&
                   ataIdentify.TrustedComputing.HasFlag(Identify.TrustedComputingBit.TrustedComputing))
                    ataOneValue.Add("Trusted Computing feature set is supported");

            if((ataIdentify.TransportMajorVersion & 0xF000) >> 12 == 0x1 ||
               (ataIdentify.TransportMajorVersion & 0xF000) >> 12 == 0xE)
            {
                if(true)
                    if(!ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.Clear))
                        if(ataIdentify.SATACapabilities.HasFlag(Identify.SATACapabilitiesBit.ReadLogDMAExt))
                            ataOneValue.Add("READ LOG DMA EXT is supported");

                if(true)
                    if(!ataIdentify.SATACapabilities2.HasFlag(Identify.SATACapabilitiesBit2.Clear))
                        if(ataIdentify.SATACapabilities2.HasFlag(Identify.SATACapabilitiesBit2.FPDMAQ))
                            ataOneValue.Add("RECEIVE FPDMA QUEUED and SEND FPDMA QUEUED are supported");

                if(true)
                    if(!ataIdentify.SATAFeatures.HasFlag(Identify.SATAFeaturesBit.Clear))
                    {
                        if(ataIdentify.SATAFeatures.HasFlag(Identify.SATAFeaturesBit.NonZeroBufferOffset))
                            ataOneValue.Add(ataIdentify.EnabledSATAFeatures.HasFlag(Identify.SATAFeaturesBit
                                                                                            .NonZeroBufferOffset)
                                                ? "Non-zero buffer offsets are supported and enabled"
                                                : "Non-zero buffer offsets are supported");
                        if(ataIdentify.SATAFeatures.HasFlag(Identify.SATAFeaturesBit.DMASetup))
                            ataOneValue.Add(ataIdentify.EnabledSATAFeatures.HasFlag(Identify.SATAFeaturesBit.DMASetup)
                                                ? "DMA Setup auto-activation is supported and enabled"
                                                : "DMA Setup auto-activation is supported");
                        if(ataIdentify.SATAFeatures.HasFlag(Identify.SATAFeaturesBit.InitPowerMgmt))
                            ataOneValue.Add(ataIdentify.EnabledSATAFeatures.HasFlag(Identify.SATAFeaturesBit
                                                                                            .InitPowerMgmt)
                                                ? "Device-initiated power management is supported and enabled"
                                                : "Device-initiated power management is supported");
                        if(ataIdentify.SATAFeatures.HasFlag(Identify.SATAFeaturesBit.InOrderData))
                            ataOneValue.Add(ataIdentify.EnabledSATAFeatures
                                                       .HasFlag(Identify.SATAFeaturesBit.InOrderData)
                                                ? "In-order data delivery is supported and enabled"
                                                : "In-order data delivery is supported");
                        if(!atapi)
                            if(ataIdentify.SATAFeatures.HasFlag(Identify.SATAFeaturesBit.HardwareFeatureControl))
                                ataOneValue.Add(ataIdentify.EnabledSATAFeatures.HasFlag(Identify.SATAFeaturesBit
                                                                                                .HardwareFeatureControl)
                                                    ? "Hardware Feature Control is supported and enabled"
                                                    : "Hardware Feature Control is supported");
                        if(atapi)
                            if(ataIdentify.SATAFeatures.HasFlag(Identify.SATAFeaturesBit.AsyncNotification))
                                if(ataIdentify.EnabledSATAFeatures.HasFlag(Identify.SATAFeaturesBit.AsyncNotification))
                                    ataOneValue.Add("Asynchronous notification is supported");
                                else
                                    ataOneValue.Add("Asynchronous notification is supported");
                        if(ataIdentify.SATAFeatures.HasFlag(Identify.SATAFeaturesBit.SettingsPreserve))
                            if(ataIdentify.EnabledSATAFeatures.HasFlag(Identify.SATAFeaturesBit.SettingsPreserve))
                                ataOneValue.Add("Software Settings Preservation is supported");
                            else
                                ataOneValue.Add("Software Settings Preservation is supported");
                        if(ataIdentify.SATAFeatures.HasFlag(Identify.SATAFeaturesBit.NCQAutoSense))
                            ataOneValue.Add("NCQ Autosense is supported");
                        if(ataIdentify.EnabledSATAFeatures.HasFlag(Identify.SATAFeaturesBit.EnabledSlumber))
                            ataOneValue.Add("Automatic Partial to Slumber transitions are enabled");
                    }
            }

            if((ataIdentify.RemovableStatusSet & 0x03) > 0)
                ataOneValue.Add("Removable Media Status Notification feature set is supported");

            if(ataIdentify.FreeFallSensitivity != 0x00 && ataIdentify.FreeFallSensitivity != 0xFF)
                ataOneValue.Add($"Free-fall sensitivity set to {ataIdentify.FreeFallSensitivity}");

            if(ataIdentify.DataSetMgmt.HasFlag(Identify.DataSetMgmtBit.Trim)) ataOneValue.Add("TRIM is supported");
            if(ataIdentify.DataSetMgmtSize > 0)
                ataOneValue.Add($"DATA SET MANAGEMENT can receive a maximum of {ataIdentify.DataSetMgmtSize} blocks of 512 bytes");

            if(ataIdentify.SecurityStatus.HasFlag(Identify.SecurityStatusBit.Supported))
            {
                ataOneValue.Add("<i>Security:</i>");
                if(ataIdentify.SecurityStatus.HasFlag(Identify.SecurityStatusBit.Enabled))
                {
                    ataOneValue.Add("Security is enabled");
                    ataOneValue.Add(ataIdentify.SecurityStatus.HasFlag(Identify.SecurityStatusBit.Locked)
                                        ? "Security is locked"
                                        : "Security is not locked");

                    ataOneValue.Add(ataIdentify.SecurityStatus.HasFlag(Identify.SecurityStatusBit.Frozen)
                                        ? "Security is frozen"
                                        : "Security is not frozen");

                    ataOneValue.Add(ataIdentify.SecurityStatus.HasFlag(Identify.SecurityStatusBit.Expired)
                                        ? "Security count has expired"
                                        : "Security count has notexpired");

                    ataOneValue.Add(ataIdentify.SecurityStatus.HasFlag(Identify.SecurityStatusBit.Maximum)
                                        ? "Security level is maximum"
                                        : "Security level is high");
                }
                else ataOneValue.Add("Security is not enabled");

                if(ataIdentify.SecurityStatus.HasFlag(Identify.SecurityStatusBit.Enhanced))
                    ataOneValue.Add("Supports enhanced security erase");

                ataOneValue.Add($"{ataIdentify.SecurityEraseTime * 2} minutes to complete secure erase");
                if(ataIdentify.SecurityStatus.HasFlag(Identify.SecurityStatusBit.Enhanced))
                    ataOneValue
                       .Add($"{ataIdentify.EnhancedSecurityEraseTime * 2} minutes to complete enhanced secure erase");

                ataOneValue.Add($"Master password revision code: {ataIdentify.MasterPasswordRevisionCode}");
            }

            if(ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeSet)    &&
               !ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.MustBeClear) &&
               ataIdentify.CommandSet3.HasFlag(Identify.CommandSetBit3.Streaming))
            {
                ataOneValue.Add("<i>Streaming:</i>");
                ataOneValue.Add($"Minimum request size is {ataIdentify.StreamMinReqSize}");
                ataOneValue.Add($"Streaming transfer time in PIO is {ataIdentify.StreamTransferTimePIO}");
                ataOneValue.Add($"Streaming transfer time in DMA is {ataIdentify.StreamTransferTimeDMA}");
                ataOneValue.Add($"Streaming access latency is {ataIdentify.StreamAccessLatency}");
                ataOneValue.Add($"Streaming performance granularity is {ataIdentify.StreamPerformanceGranularity}");
            }

            if(ataIdentify.SCTCommandTransport.HasFlag(Identify.SCTCommandTransportBit.Supported))
            {
                ataOneValue.Add("<i>S.M.A.R.T. Command Transport (SCT):</i>");
                if(ataIdentify.SCTCommandTransport.HasFlag(Identify.SCTCommandTransportBit.LongSectorAccess))
                    ataOneValue.Add("SCT Long Sector Address is supported");
                if(ataIdentify.SCTCommandTransport.HasFlag(Identify.SCTCommandTransportBit.WriteSame))
                    ataOneValue.Add("SCT Write Same is supported");
                if(ataIdentify.SCTCommandTransport.HasFlag(Identify.SCTCommandTransportBit.ErrorRecoveryControl))
                    ataOneValue.Add("SCT Error Recovery Control is supported");
                if(ataIdentify.SCTCommandTransport.HasFlag(Identify.SCTCommandTransportBit.FeaturesControl))
                    ataOneValue.Add("SCT Features Control is supported");
                if(ataIdentify.SCTCommandTransport.HasFlag(Identify.SCTCommandTransportBit.DataTables))
                    ataOneValue.Add("SCT Data Tables are supported");
            }

            if((ataIdentify.NVCacheCaps & 0x0010) == 0x0010)
            {
                ataOneValue.Add("<i>Non-Volatile Cache:</i>");
                ataOneValue.Add($"Version {(ataIdentify.NVCacheCaps & 0xF000) >> 12}");
                if((ataIdentify.NVCacheCaps & 0x0001) == 0x0001)
                {
                    ataOneValue.Add((ataIdentify.NVCacheCaps & 0x0002) == 0x0002
                                        ? "Power mode feature set is supported and enabled"
                                        : "Power mode feature set is supported");

                    ataOneValue.Add($"Version {(ataIdentify.NVCacheCaps & 0x0F00) >> 8}");
                }

                ataOneValue.Add($"Non-Volatile Cache is {ataIdentify.NVCacheSize * logicalsectorsize} bytes");
            }

            if(ataReport.ReadCapabilities != null)
            {
                removable = false;
                ataOneValue.Add("");

                if(ataReport.ReadCapabilities.NominalRotationRate != null   &&
                   ataReport.ReadCapabilities.NominalRotationRate != 0x0000 &&
                   ataReport.ReadCapabilities.NominalRotationRate != 0xFFFF)
                    ataOneValue.Add(ataReport.ReadCapabilities.NominalRotationRate == 0x0001
                                        ? "Device does not rotate."
                                        : $"Device rotates at {ataReport.ReadCapabilities.NominalRotationRate} rpm");

                if(!atapi)
                {
                    if(ataReport.ReadCapabilities.BlockSize != null)
                    {
                        ataTwoValue.Add("Logical sector size", $"{ataReport.ReadCapabilities.BlockSize} bytes");
                        logicalsectorsize = ataReport.ReadCapabilities.BlockSize.Value;
                    }

                    if(ataReport.ReadCapabilities.PhysicalBlockSize != null)
                        ataTwoValue.Add("Physical sector size",
                                        $"{ataReport.ReadCapabilities.PhysicalBlockSize} bytes");
                    if(ataReport.ReadCapabilities.LongBlockSize != null)
                        ataTwoValue.Add("READ LONG sector size", $"{ataReport.ReadCapabilities.LongBlockSize} bytes");

                    if(ataReport.ReadCapabilities.BlockSize         != null &&
                       ataReport.ReadCapabilities.PhysicalBlockSize != null &&
                       ataReport.ReadCapabilities.BlockSize.Value !=
                       ataReport.ReadCapabilities.PhysicalBlockSize.Value               &&
                       (ataReport.ReadCapabilities.LogicalAlignment & 0x8000) == 0x0000 &&
                       (ataReport.ReadCapabilities.LogicalAlignment & 0x4000) == 0x4000)
                        ataOneValue
                           .Add($"Logical sector starts at offset {ataReport.ReadCapabilities.LogicalAlignment & 0x3FFF} from physical sector");

                    if(ataReport.ReadCapabilities.CHS != null && ataReport.ReadCapabilities.CurrentCHS != null)
                    {
                        int currentSectors = ataReport.ReadCapabilities.CurrentCHS.Cylinders *
                                             ataReport.ReadCapabilities.CurrentCHS.Heads     *
                                             ataReport.ReadCapabilities.CurrentCHS.Sectors;
                        ataTwoValue.Add("Cylinders",
                                        $"{ataReport.ReadCapabilities.CHS.Cylinders} max., {ataReport.ReadCapabilities.CurrentCHS.Cylinders} current");
                        ataTwoValue.Add("Heads",
                                        $"{ataReport.ReadCapabilities.CHS.Heads} max., {ataReport.ReadCapabilities.CurrentCHS.Heads} current");
                        ataTwoValue.Add("Sectors per track",
                                        $"{ataReport.ReadCapabilities.CHS.Sectors} max., {ataReport.ReadCapabilities.CurrentCHS.Sectors} current");
                        ataTwoValue.Add("Sectors addressable in CHS mode",
                                        $"{ataReport.ReadCapabilities.CHS.Cylinders * ataReport.ReadCapabilities.CHS.Heads * ataReport.ReadCapabilities.CHS.Sectors} max., {currentSectors} current");
                        ataTwoValue.Add("Device size in CHS mode",
                                        $"{(ulong)currentSectors * logicalsectorsize} bytes, {(ulong)currentSectors * logicalsectorsize / 1000 / 1000} Mb, {(double)((ulong)currentSectors * logicalsectorsize) / 1024 / 1024:F2} MiB");
                    }
                    else if(ataReport.ReadCapabilities.CHS != null)
                    {
                        int currentSectors = ataReport.ReadCapabilities.CHS.Cylinders *
                                             ataReport.ReadCapabilities.CHS.Heads     *
                                             ataReport.ReadCapabilities.CHS.Sectors;
                        ataTwoValue.Add("Cylinders",
                                        $"{ataReport.ReadCapabilities.CHS.Cylinders}");
                        ataTwoValue.Add("Heads",                           $"{ataReport.ReadCapabilities.CHS.Heads}");
                        ataTwoValue.Add("Sectors per track",               $"{ataReport.ReadCapabilities.CHS.Sectors}");
                        ataTwoValue.Add("Sectors addressable in CHS mode", $"{currentSectors}");
                        ataTwoValue.Add("Device size in CHS mode",
                                        $"{(ulong)currentSectors * logicalsectorsize} bytes, {(ulong)currentSectors * logicalsectorsize / 1000 / 1000} Mb, {(double)((ulong)currentSectors * logicalsectorsize) / 1024 / 1024:F2} MiB");
                    }

                    if(ataReport.ReadCapabilities.LBASectors != null)
                    {
                        ataTwoValue.Add("Sectors addressable in sectors in 28-bit LBA mode",
                                        $"{ataReport.ReadCapabilities.LBASectors}");

                        if((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize / 1024 / 1024 > 1000000)
                            ataTwoValue.Add("Device size in 28-bit LBA mode",
                                            $"{(ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize} bytes, {(ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize / 1000 / 1000 / 1000 / 1000} Tb, {(double)((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1024 / 1024 / 1024 / 1024:F2} TiB");
                        else if((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize / 1024 / 1024 > 1000)
                            ataTwoValue.Add("Device size in 28-bit LBA mode",
                                            $"{(ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize} bytes, {(ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize / 1000 / 1000 / 1000} Gb, {(double)((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1024 / 1024 / 1024:F2} GiB");
                        else
                            ataTwoValue.Add("Device size in 28-bit LBA mode",
                                            $"{(ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize} bytes, {(ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize / 1000 / 1000} Mb, {(double)((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1024 / 1024:F2} MiB");
                    }

                    if(ataReport.ReadCapabilities.LBA48Sectors != null)
                    {
                        ataTwoValue.Add("Sectors addressable in sectors in 48-bit LBA mode",
                                        $"{ataReport.ReadCapabilities.LBA48Sectors}");

                        if(ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize / 1024 / 1024 > 1000000)
                            ataTwoValue.Add("Device size in 48-bit LBA mode",
                                            $"{ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize} bytes, {ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize / 1000 / 1000 / 1000 / 1000} Tb, {(double)(ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1024 / 1024 / 1024 / 1024:F2} TiB");
                        else if(ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize / 1024 / 1024 > 1000)
                            ataTwoValue.Add("Device size in 48-bit LBA mode",
                                            $"{ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize} bytes, {ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize / 1000 / 1000 / 1000} Gb, {(double)(ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1024 / 1024 / 1024:F2} GiB");
                        else
                            ataTwoValue.Add("Device size in 48-bit LBA mode",
                                            $"{ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize} bytes, {ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize / 1000 / 1000} Mb, {(double)(ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1024 / 1024:F2} MiB");
                    }

                    if(ata1 || cfa)
                    {
                        if(ataReport.ReadCapabilities.UnformattedBPT > 0)
                            ataTwoValue.Add("Bytes per unformatted track",
                                            $"{ataReport.ReadCapabilities.UnformattedBPT}");
                        if(ataReport.ReadCapabilities.UnformattedBPS > 0)
                            ataTwoValue.Add("Bytes per unformatted sector",
                                            $"{ataReport.ReadCapabilities.UnformattedBPS}");
                    }
                }

                if(ataReport.ReadCapabilities.SupportsReadSectors == true)
                    ataOneValue.Add("Device supports READ SECTOR(S) command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsReadRetry == true)
                    ataOneValue.Add("Device supports READ SECTOR(S) RETRY command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsReadDma == true)
                    ataOneValue.Add("Device supports READ DMA command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsReadDmaRetry == true)
                    ataOneValue.Add("Device supports READ DMA RETRY command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsReadLong == true)
                    ataOneValue.Add("Device supports READ LONG command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsReadLongRetry == true)
                    ataOneValue.Add("Device supports READ LONG RETRY command in CHS mode");

                if(ataReport.ReadCapabilities.SupportsReadLba == true)
                    ataOneValue.Add("Device supports READ SECTOR(S) command in 28-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadRetryLba == true)
                    ataOneValue.Add("Device supports READ SECTOR(S) RETRY command in 28-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadDmaLba == true)
                    ataOneValue.Add("Device supports READ DMA command in 28-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadDmaRetryLba == true)
                    ataOneValue.Add("Device supports READ DMA RETRY command in 28-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadLongLba == true)
                    ataOneValue.Add("Device supports READ LONG command in 28-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadLongRetryLba == true)
                    ataOneValue.Add("Device supports READ LONG RETRY command in 28-bit LBA mode");

                if(ataReport.ReadCapabilities.SupportsReadLba48 == true)
                    ataOneValue.Add("Device supports READ SECTOR(S) command in 48-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadDmaLba48 == true)
                    ataOneValue.Add("Device supports READ DMA command in 48-bit LBA mode");

                if(ataReport.ReadCapabilities.SupportsSeek == true)
                    ataOneValue.Add("Device supports SEEK command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsSeekLba == true)
                    ataOneValue.Add("Device supports SEEK command in 28-bit LBA mode");
            }
            else testedMedia = ataReport.RemovableMedias;
        }
    }
}