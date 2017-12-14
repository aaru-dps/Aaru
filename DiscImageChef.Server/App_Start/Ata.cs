// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Ata.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Collections.Generic;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Metadata;
using static DiscImageChef.Decoders.ATA.Identify;

namespace DiscImageChef.Server.App_Start
{
    public static class Ata
    {
        public static void Report(ataType ataReport, bool cfa, bool atapi, ref bool removable, ref List<string> ataOneValue, ref Dictionary<string, string> ataTwoValue, ref testedMediaType[] testedMedia)
        {
            string tmpString;
            uint logicalsectorsize = 0;

            if(ataReport.ModelSpecified && !string.IsNullOrEmpty(ataReport.Model))
                ataTwoValue.Add("Model", ataReport.Model);
            if(ataReport.FirmwareRevisionSpecified && !string.IsNullOrEmpty(ataReport.FirmwareRevision))
                ataTwoValue.Add("Firmware revision", ataReport.FirmwareRevision);
            if(ataReport.AdditionalPIDSpecified && !string.IsNullOrEmpty(ataReport.AdditionalPID))
                ataTwoValue.Add("Additional product ID", ataReport.AdditionalPID);

            bool ata1 = false, ata2 = false, ata3 = false, ata4 = false, ata5 = false, ata6 = false, ata7 = false, acs = false, acs2 = false, acs3 = false, acs4 = false;

            if(ataReport.MajorVersionSpecified && ((ushort)ataReport.MajorVersion == 0x0000 || (ushort)ataReport.MajorVersion == 0xFFFF))
            {
                // Obsolete in ATA-2, if present, device supports ATA-1
                if(ataReport.GeneralConfigurationSpecified)
                    ata1 |= ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.FastIDE) ||
        ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.SlowIDE) ||
        ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.UltraFastIDE);

                ata2 |= ataReport.ExtendedIdentifySpecified;

                if(!ata1 && !ata2 && !atapi && !cfa)
                    ata2 = true;

                ata4 |= atapi;
                ata3 |= cfa;

                if(cfa && ata1)
                    ata1 = false;
                if(cfa && ata2)
                    ata2 = false;

            }
            else
            {
                if(ataReport.MajorVersionSpecified)
                {
                    ata1 |= ataReport.MajorVersion.HasFlag(MajorVersionBit.Ata1);
                    ata2 |= ataReport.MajorVersion.HasFlag(MajorVersionBit.Ata2);
                    ata3 |= ataReport.MajorVersion.HasFlag(MajorVersionBit.Ata3);
                    ata4 |= ataReport.MajorVersion.HasFlag(MajorVersionBit.AtaAtapi4);
                    ata5 |= ataReport.MajorVersion.HasFlag(MajorVersionBit.AtaAtapi5);
                    ata6 |= ataReport.MajorVersion.HasFlag(MajorVersionBit.AtaAtapi6);
                    ata7 |= ataReport.MajorVersion.HasFlag(MajorVersionBit.AtaAtapi7);
                    acs |= ataReport.MajorVersion.HasFlag(MajorVersionBit.Ata8ACS);
                    acs2 |= ataReport.MajorVersion.HasFlag(MajorVersionBit.ACS2);
                    acs3 |= ataReport.MajorVersion.HasFlag(MajorVersionBit.ACS3);
                    acs4 |= ataReport.MajorVersion.HasFlag(MajorVersionBit.ACS4);
                }
            }

            int maxatalevel = 0;
            int minatalevel = 255;
            tmpString = "";
            if(ata1)
            {
                tmpString += "ATA-1 ";
                maxatalevel = 1;
                if(minatalevel > 1)
                    minatalevel = 1;
            }
            if(ata2)
            {
                tmpString += "ATA-2 ";
                maxatalevel = 2;
                if(minatalevel > 2)
                    minatalevel = 2;
            }
            if(ata3)
            {
                tmpString += "ATA-3 ";
                maxatalevel = 3;
                if(minatalevel > 3)
                    minatalevel = 3;
            }
            if(ata4)
            {
                tmpString += "ATA/ATAPI-4 ";
                maxatalevel = 4;
                if(minatalevel > 4)
                    minatalevel = 4;
            }
            if(ata5)
            {
                tmpString += "ATA/ATAPI-5 ";
                maxatalevel = 5;
                if(minatalevel > 5)
                    minatalevel = 5;
            }
            if(ata6)
            {
                tmpString += "ATA/ATAPI-6 ";
                maxatalevel = 6;
                if(minatalevel > 6)
                    minatalevel = 6;
            }
            if(ata7)
            {
                tmpString += "ATA/ATAPI-7 ";
                maxatalevel = 7;
                if(minatalevel > 7)
                    minatalevel = 7;
            }
            if(acs)
            {
                tmpString += "ATA8-ACS ";
                maxatalevel = 8;
                if(minatalevel > 8)
                    minatalevel = 8;
            }
            if(acs2)
            {
                tmpString += "ATA8-ACS2 ";
                maxatalevel = 9;
                if(minatalevel > 9)
                    minatalevel = 9;
            }
            if(acs3)
            {
                tmpString += "ATA8-ACS3 ";
                maxatalevel = 10;
                if(minatalevel > 10)
                    minatalevel = 10;
            }
            if(acs4)
            {
                tmpString += "ATA8-ACS4 ";
                maxatalevel = 11;
                if(minatalevel > 11)
                    minatalevel = 11;
            }
            if(tmpString != "")
                ataTwoValue.Add("Supported ATA versions", tmpString);

            if(maxatalevel >= 3 && ataReport.MinorVersionSpecified)
            {
                switch(ataReport.MinorVersion)
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
                        tmpString = string.Format("Unknown ATA revision 0x{0:X4}", ataReport.MinorVersion);
                        break;
                }
                ataTwoValue.Add("Maximum ATA revision supported", tmpString);
            }

            if(ataReport.TransportMajorVersionSpecified)
            {
                tmpString = "";
                switch((ataReport.TransportMajorVersion & 0xF000) >> 12)
                {
                    case 0x0:
                        if((ataReport.TransportMajorVersion & 0x0002) == 0x0002)
                            tmpString += "ATA/ATAPI-7 ";
                        if((ataReport.TransportMajorVersion & 0x0001) == 0x0001)
                            tmpString += "ATA8-APT ";
                        ataTwoValue.Add("Parallel ATA device", tmpString);
                        break;
                    case 0x1:
                        if((ataReport.TransportMajorVersion & 0x0001) == 0x0001)
                            tmpString += "ATA8-AST ";
                        if((ataReport.TransportMajorVersion & 0x0002) == 0x0002)
                            tmpString += "SATA 1.0a ";
                        if((ataReport.TransportMajorVersion & 0x0004) == 0x0004)
                            tmpString += "SATA II Extensions ";
                        if((ataReport.TransportMajorVersion & 0x0008) == 0x0008)
                            tmpString += "SATA 2.5 ";
                        if((ataReport.TransportMajorVersion & 0x0010) == 0x0010)
                            tmpString += "SATA 2.6 ";
                        if((ataReport.TransportMajorVersion & 0x0020) == 0x0020)
                            tmpString += "SATA 3.0 ";
                        if((ataReport.TransportMajorVersion & 0x0040) == 0x0040)
                            tmpString += "SATA 3.1 ";
                        ataTwoValue.Add("Serial ATA device: ", tmpString);
                        break;
                    case 0xE:
                        ataTwoValue.Add("SATA Express device", "No version");
                        break;
                    default:
                        ataTwoValue.Add("Unknown transport type", string.Format("0x{0:X1}", (ataReport.TransportMajorVersion & 0xF000) >> 12));
                        break;
                }
            }

            if(atapi && ataReport.GeneralConfigurationSpecified)
            {
                // Bits 12 to 8, SCSI Peripheral Device Type
                switch((Decoders.SCSI.PeripheralDeviceTypes)(((ushort)ataReport.GeneralConfiguration & 0x1F00) >> 8))
                {
                    case Decoders.SCSI.PeripheralDeviceTypes.DirectAccess: //0x00,
                        ataOneValue.Add("ATAPI Direct-access device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.SequentialAccess: //0x01,
                        ataOneValue.Add("ATAPI Sequential-access device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.PrinterDevice: //0x02,
                        ataOneValue.Add("ATAPI Printer device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.ProcessorDevice: //0x03,
                        ataOneValue.Add("ATAPI Processor device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.WriteOnceDevice: //0x04,
                        ataOneValue.Add("ATAPI Write-once device");
                        break;
                    case PeripheralDeviceTypes.MultiMediaDevice: //0x05,
                        ataOneValue.Add("ATAPI CD-ROM/DVD/etc device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.ScannerDevice: //0x06,
                        ataOneValue.Add("ATAPI Scanner device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.OpticalDevice: //0x07,
                        ataOneValue.Add("ATAPI Optical memory device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.MediumChangerDevice: //0x08,
                        ataOneValue.Add("ATAPI Medium change device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.CommsDevice: //0x09,
                        ataOneValue.Add("ATAPI Communications device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.PrePressDevice1: //0x0A,
                        ataOneValue.Add("ATAPI Graphics arts pre-press device (defined in ASC IT8)");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.PrePressDevice2: //0x0B,
                        ataOneValue.Add("ATAPI Graphics arts pre-press device (defined in ASC IT8)");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.ArrayControllerDevice: //0x0C,
                        ataOneValue.Add("ATAPI Array controller device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.EnclosureServiceDevice: //0x0D,
                        ataOneValue.Add("ATAPI Enclosure services device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.SimplifiedDevice: //0x0E,
                        ataOneValue.Add("ATAPI Simplified direct-access device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.OCRWDevice: //0x0F,
                        ataOneValue.Add("ATAPI Optical card reader/writer device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.BridgingExpander: //0x10,
                        ataOneValue.Add("ATAPI Bridging Expanders");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.ObjectDevice: //0x11,
                        ataOneValue.Add("ATAPI Object-based Storage Device");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.ADCDevice: //0x12,
                        ataOneValue.Add("ATAPI Automation/Drive Interface");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.WellKnownDevice: //0x1E,
                        ataOneValue.Add("ATAPI Well known logical unit");
                        break;
                    case Decoders.SCSI.PeripheralDeviceTypes.UnknownDevice: //0x1F
                        ataOneValue.Add("ATAPI Unknown or no device type");
                        break;
                    default:
                        ataOneValue.Add(string.Format("ATAPI Unknown device type field value 0x{0:X2}", ((ushort)ataReport.GeneralConfiguration & 0x1F00) >> 8));
                        break;
                }

                // ATAPI DRQ behaviour
                switch(((ushort)ataReport.GeneralConfiguration & 0x60) >> 5)
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
                        ataOneValue.Add(string.Format("Unknown ATAPI DRQ behaviour code {0}", ((ushort)ataReport.GeneralConfiguration & 0x60) >> 5));
                        break;
                }

                // ATAPI PACKET size
                switch((ushort)ataReport.GeneralConfiguration & 0x03)
                {
                    case 0:
                        ataOneValue.Add("ATAPI device uses 12 byte command packet");
                        break;
                    case 1:
                        ataOneValue.Add("ATAPI device uses 16 byte command packet");
                        break;
                    default:
                        ataOneValue.Add(string.Format("Unknown ATAPI packet size code {0}", (ushort)ataReport.GeneralConfiguration & 0x03));
                        break;
                }

            }
            else if(!cfa && ataReport.GeneralConfigurationSpecified)
            {
                if(minatalevel >= 5)
                {
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.IncompleteResponse))
                        ataOneValue.Add("Incomplete identify response");
                }
                if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.NonMagnetic))
                    ataOneValue.Add("Device uses non-magnetic media");

                if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.Removable))
                    ataOneValue.Add("Device is removable");

                if(minatalevel <= 5)
                {
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.Fixed))
                        ataOneValue.Add("Device is fixed");
                }

                if(ata1)
                {
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.SlowIDE))
                        ataOneValue.Add("Device transfer rate is <= 5 Mb/s");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.FastIDE))
                        ataOneValue.Add("Device transfer rate is > 5 Mb/s but <= 10 Mb/s");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.UltraFastIDE))
                        ataOneValue.Add("Device transfer rate is > 10 Mb/s");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.SoftSector))
                        ataOneValue.Add("Device is soft sectored");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.HardSector))
                        ataOneValue.Add("Device is hard sectored");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.NotMFM))
                        ataOneValue.Add("Device is not MFM encoded");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.FormatGapReq))
                        ataOneValue.Add("Format speed tolerance gap is required");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.TrackOffset))
                        ataOneValue.Add("Track offset option is available");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.DataStrobeOffset))
                        ataOneValue.Add("Data strobe offset option is available");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.RotationalSpeedTolerance))
                        ataOneValue.Add("Rotational speed tolerance is higher than 0,5%");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.SpindleControl))
                        ataOneValue.Add("Spindle motor control is implemented");
                    if(ataReport.GeneralConfiguration.HasFlag(GeneralConfigurationBit.HighHeadSwitch))
                        ataOneValue.Add("Head switch time is bigger than 15 µs.");
                }
            }

            if(ataReport.SpecificConfigurationSpecified &&
               (ushort)ataReport.SpecificConfiguration != 0x0000 &&
       (ushort)ataReport.SpecificConfiguration != 0xFFFF)
            {
                switch(ataReport.SpecificConfiguration)
                {
                    case SpecificConfigurationEnum.RequiresSetIncompleteResponse:
                        ataOneValue.Add("Device requires SET FEATURES to spin up and IDENTIFY DEVICE response is incomplete.");
                        break;
                    case SpecificConfigurationEnum.RequiresSetCompleteResponse:
                        ataOneValue.Add("Device requires SET FEATURES to spin up and IDENTIFY DEVICE response is complete.");
                        break;
                    case SpecificConfigurationEnum.NotRequiresSetIncompleteResponse:
                        ataOneValue.Add("Device does not require SET FEATURES to spin up and IDENTIFY DEVICE response is incomplete.");
                        break;
                    case SpecificConfigurationEnum.NotRequiresSetCompleteResponse:
                        ataOneValue.Add("Device does not require SET FEATURES to spin up and IDENTIFY DEVICE response is complete.");
                        break;
                    default:
                        ataOneValue.Add(string.Format("Unknown device specific configuration 0x{0:X4}", (ushort)ataReport.SpecificConfiguration));
                        break;
                }
            }

            // Obsolete since ATA-2, however, it is yet used in ATA-8 devices
            if(ataReport.BufferSizeSpecified && ataReport.BufferTypeSpecified &&
                ataReport.BufferSize != 0x0000 && ataReport.BufferSize != 0xFFFF &&
        ataReport.BufferType != 0x0000 && ataReport.BufferType != 0xFFFF)
            {
                switch(ataReport.BufferType)
                {
                    case 1:
                        ataOneValue.Add(string.Format("{0} KiB of single ported single sector buffer", (ataReport.BufferSize * logicalsectorsize) / 1024));
                        break;
                    case 2:
                        ataOneValue.Add(string.Format("{0} KiB of dual ported multi sector buffer", (ataReport.BufferSize * logicalsectorsize) / 1024));
                        break;
                    case 3:
                        ataOneValue.Add(string.Format("{0} KiB of dual ported multi sector buffer with read caching", (ataReport.BufferSize * logicalsectorsize) / 1024));
                        break;
                    default:
                        ataOneValue.Add(string.Format("{0} KiB of unknown type {1} buffer", (ataReport.BufferSize * logicalsectorsize) / 1024, ataReport.BufferType));
                        break;
                }
            }

            if(ataReport.CapabilitiesSpecified)
            {
                ataOneValue.Add("<i>Device capabilities:</i>");
                if(ataReport.Capabilities.HasFlag(CapabilitiesBit.StandardStanbyTimer))
                    ataOneValue.Add("Standby time values are standard");
                if(ataReport.Capabilities.HasFlag(CapabilitiesBit.IORDY))
                {
                    if(ataReport.Capabilities.HasFlag(CapabilitiesBit.CanDisableIORDY))
                        ataOneValue.Add("IORDY is supported and can be disabled");
                    else
                        ataOneValue.Add("IORDY is supported");
                }
                if(ataReport.Capabilities.HasFlag(CapabilitiesBit.DMASupport))
                    ataOneValue.Add("DMA is supported");
                if(ataReport.Capabilities.HasFlag(CapabilitiesBit.PhysicalAlignment1) ||
                    ataReport.Capabilities.HasFlag(CapabilitiesBit.PhysicalAlignment0))
                {
                    ataOneValue.Add(string.Format("Long Physical Alignment setting is {0}", (ushort)ataReport.Capabilities & 0x03));
                }
                if(atapi)
                {
                    if(ataReport.Capabilities.HasFlag(CapabilitiesBit.InterleavedDMA))
                        ataOneValue.Add("ATAPI device supports interleaved DMA");
                    if(ataReport.Capabilities.HasFlag(CapabilitiesBit.CommandQueue))
                        ataOneValue.Add("ATAPI device supports command queueing");
                    if(ataReport.Capabilities.HasFlag(CapabilitiesBit.OverlapOperation))
                        ataOneValue.Add("ATAPI device supports overlapped operations");
                    if(ataReport.Capabilities.HasFlag(CapabilitiesBit.RequiresATASoftReset))
                        ataOneValue.Add("ATAPI device requires ATA software reset");
                }
            }

            if(ataReport.Capabilities2Specified)
            {
                if(ataReport.Capabilities2.HasFlag(CapabilitiesBit2.MustBeSet) &&
                    !ataReport.Capabilities2.HasFlag(CapabilitiesBit2.MustBeClear))
                {
                    if(ataReport.Capabilities2.HasFlag(CapabilitiesBit2.SpecificStandbyTimer))
                        ataOneValue.Add("Device indicates a specific minimum standby timer value");
                }
            }

            if(ataReport.Capabilities3Specified)
            {
                if(ataReport.Capabilities3.HasFlag(CapabilitiesBit3.MultipleValid))
                {
                    ataOneValue.Add(string.Format("A maximum of {0} sectors can be transferred per interrupt on READ/WRITE MULTIPLE", ataReport.MultipleSectorNumber));
                    ataOneValue.Add(string.Format("Device supports setting a maximum of {0} sectors", ataReport.MultipleMaxSectors));
                }
            }

            if(ata1 && ataReport.TrustedComputingSpecified)
            {
                if(ataReport.TrustedComputing.HasFlag(TrustedComputingBit.TrustedComputing))
                    ataOneValue.Add("Device supports doubleword I/O");
            }

            if(minatalevel <= 3)
            {
                if(ataReport.PIOTransferTimingModeSpecified)
                    ataTwoValue.Add("PIO timing mode", string.Format("{0}", ataReport.PIOTransferTimingMode));
                if(ataReport.DMATransferTimingModeSpecified)
                    ataTwoValue.Add("DMA timing mode", string.Format("{0}", ataReport.DMATransferTimingMode));
            }

            if(ataReport.APIOSupportedSpecified)
            {
                tmpString = "";

                if(ataReport.APIOSupported.HasFlag(TransferMode.Mode0))
                {
                    tmpString += "PIO0 ";
                }
                if(ataReport.APIOSupported.HasFlag(TransferMode.Mode1))
                {
                    tmpString += "PIO1 ";
                }
                if(ataReport.APIOSupported.HasFlag(TransferMode.Mode2))
                {
                    tmpString += "PIO2 ";
                }
                if(ataReport.APIOSupported.HasFlag(TransferMode.Mode3))
                {
                    tmpString += "PIO3 ";
                }
                if(ataReport.APIOSupported.HasFlag(TransferMode.Mode4))
                {
                    tmpString += "PIO4 ";
                }
                if(ataReport.APIOSupported.HasFlag(TransferMode.Mode5))
                {
                    tmpString += "PIO5 ";
                }
                if(ataReport.APIOSupported.HasFlag(TransferMode.Mode6))
                {
                    tmpString += "PIO6 ";
                }
                if(ataReport.APIOSupported.HasFlag(TransferMode.Mode7))
                {
                    tmpString += "PIO7 ";
                }

                ataTwoValue.Add("Advanced PIO", tmpString);
            }

            if(minatalevel <= 3 && !atapi && ataReport.DMASupportedSpecified)
            {
                tmpString = "";
                if(ataReport.DMASupported.HasFlag(TransferMode.Mode0))
                {
                    tmpString += "DMA0 ";
                    if(ataReport.DMAActive.HasFlag(TransferMode.Mode0) && ataReport.DMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.DMASupported.HasFlag(TransferMode.Mode1))
                {
                    tmpString += "DMA1 ";
                    if(ataReport.DMAActive.HasFlag(TransferMode.Mode1) && ataReport.DMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.DMASupported.HasFlag(TransferMode.Mode2))
                {
                    tmpString += "DMA2 ";
                    if(ataReport.DMAActive.HasFlag(TransferMode.Mode2) && ataReport.DMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.DMASupported.HasFlag(TransferMode.Mode3))
                {
                    tmpString += "DMA3 ";
                    if(ataReport.DMAActive.HasFlag(TransferMode.Mode3) && ataReport.DMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.DMASupported.HasFlag(TransferMode.Mode4))
                {
                    tmpString += "DMA4 ";
                    if(ataReport.DMAActive.HasFlag(TransferMode.Mode4) && ataReport.DMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.DMASupported.HasFlag(TransferMode.Mode5))
                {
                    tmpString += "DMA5 ";
                    if(ataReport.DMAActive.HasFlag(TransferMode.Mode5) && ataReport.DMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.DMASupported.HasFlag(TransferMode.Mode6))
                {
                    tmpString += "DMA6 ";
                    if(ataReport.DMAActive.HasFlag(TransferMode.Mode6) && ataReport.DMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.DMASupported.HasFlag(TransferMode.Mode7))
                {
                    tmpString += "DMA7 ";
                    if(ataReport.DMAActive.HasFlag(TransferMode.Mode7) && ataReport.DMAActiveSpecified)
                        tmpString += "(active) ";
                }
                ataTwoValue.Add("Single-word DMA", tmpString);
            }

            if(ataReport.MDMASupportedSpecified)
            {
                tmpString = "";
                if(ataReport.MDMASupported.HasFlag(TransferMode.Mode0))
                {
                    tmpString += "MDMA0 ";
                    if(ataReport.MDMAActive.HasFlag(TransferMode.Mode0) && ataReport.MDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.MDMASupported.HasFlag(TransferMode.Mode1))
                {
                    tmpString += "MDMA1 ";
                    if(ataReport.MDMAActive.HasFlag(TransferMode.Mode1) && ataReport.MDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.MDMASupported.HasFlag(TransferMode.Mode2))
                {
                    tmpString += "MDMA2 ";
                    if(ataReport.MDMAActive.HasFlag(TransferMode.Mode2) && ataReport.MDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.MDMASupported.HasFlag(TransferMode.Mode3))
                {
                    tmpString += "MDMA3 ";
                    if(ataReport.MDMAActive.HasFlag(TransferMode.Mode3) && ataReport.MDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.MDMASupported.HasFlag(TransferMode.Mode4))
                {
                    tmpString += "MDMA4 ";
                    if(ataReport.MDMAActive.HasFlag(TransferMode.Mode4) && ataReport.MDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.MDMASupported.HasFlag(TransferMode.Mode5))
                {
                    tmpString += "MDMA5 ";
                    if(ataReport.MDMAActive.HasFlag(TransferMode.Mode5) && ataReport.MDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.MDMASupported.HasFlag(TransferMode.Mode6))
                {
                    tmpString += "MDMA6 ";
                    if(ataReport.MDMAActive.HasFlag(TransferMode.Mode6) && ataReport.MDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.MDMASupported.HasFlag(TransferMode.Mode7))
                {
                    tmpString += "MDMA7 ";
                    if(ataReport.MDMAActive.HasFlag(TransferMode.Mode7) && ataReport.MDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                ataTwoValue.Add("Multi-word DMA", tmpString);
            }

            if(ataReport.UDMASupportedSpecified)
            {
                tmpString = "";
                if(ataReport.UDMASupported.HasFlag(TransferMode.Mode0))
                {
                    tmpString += "UDMA0 ";
                    if(ataReport.UDMAActive.HasFlag(TransferMode.Mode0) && ataReport.UDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.UDMASupported.HasFlag(TransferMode.Mode1))
                {
                    tmpString += "UDMA1 ";
                    if(ataReport.UDMAActive.HasFlag(TransferMode.Mode1) && ataReport.UDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.UDMASupported.HasFlag(TransferMode.Mode2))
                {
                    tmpString += "UDMA2 ";
                    if(ataReport.UDMAActive.HasFlag(TransferMode.Mode2) && ataReport.UDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.UDMASupported.HasFlag(TransferMode.Mode3))
                {
                    tmpString += "UDMA3 ";
                    if(ataReport.UDMAActive.HasFlag(TransferMode.Mode3) && ataReport.UDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.UDMASupported.HasFlag(TransferMode.Mode4))
                {
                    tmpString += "UDMA4 ";
                    if(ataReport.UDMAActive.HasFlag(TransferMode.Mode4) && ataReport.UDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.UDMASupported.HasFlag(TransferMode.Mode5))
                {
                    tmpString += "UDMA5 ";
                    if(ataReport.UDMAActive.HasFlag(TransferMode.Mode5) && ataReport.UDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.UDMASupported.HasFlag(TransferMode.Mode6))
                {
                    tmpString += "UDMA6 ";
                    if(ataReport.UDMAActive.HasFlag(TransferMode.Mode6) && ataReport.UDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                if(ataReport.UDMASupported.HasFlag(TransferMode.Mode7))
                {
                    tmpString += "UDMA7 ";
                    if(ataReport.UDMAActive.HasFlag(TransferMode.Mode7) && ataReport.UDMAActiveSpecified)
                        tmpString += "(active) ";
                }
                ataTwoValue.Add("Ultra DMA", tmpString);
            }

            if(ataReport.MinMDMACycleTime != 0 && ataReport.RecommendedMDMACycleTime != 0)
            {
                ataOneValue.Add(string.Format("At minimum {0} ns. transfer cycle time per word in MDMA, " +
                                              "{1} ns. recommended", ataReport.MinMDMACycleTime, ataReport.RecommendedMDMACycleTime));
            }
            if(ataReport.MinPIOCycleTimeNoFlow != 0)
            {
                ataOneValue.Add(string.Format("At minimum {0} ns. transfer cycle time per word in PIO, " +
                                              "without flow control", ataReport.MinPIOCycleTimeNoFlow));
            }
            if(ataReport.MinPIOCycleTimeFlow != 0)
            {
                ataOneValue.Add(string.Format("At minimum {0} ns. transfer cycle time per word in PIO, " +
                                              "with IORDY flow control", ataReport.MinPIOCycleTimeFlow));
            }

            if(ataReport.MaxQueueDepth != 0)
            {
                ataOneValue.Add(string.Format("{0} depth of queue maximum", ataReport.MaxQueueDepth + 1));
            }

            if(atapi)
            {
                if(ataReport.PacketBusRelease != 0)
                    ataOneValue.Add(string.Format("{0} ns. typical to release bus from receipt of PACKET", ataReport.PacketBusRelease));
                if(ataReport.ServiceBusyClear != 0)
                    ataOneValue.Add(string.Format("{0} ns. typical to clear BSY bit from receipt of SERVICE", ataReport.ServiceBusyClear));
            }

            if(ataReport.TransportMajorVersionSpecified &&
                (((ataReport.TransportMajorVersion & 0xF000) >> 12) == 0x1 ||
                 ((ataReport.TransportMajorVersion & 0xF000) >> 12) == 0xE))
            {
                if(!ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.Clear))
                {
                    if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.Gen1Speed))
                    {
                        ataOneValue.Add(string.Format("SATA 1.5Gb/s is supported"));
                    }
                    if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.Gen2Speed))
                    {
                        ataOneValue.Add(string.Format("SATA 3.0Gb/s is supported"));
                    }
                    if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.Gen3Speed))
                    {
                        ataOneValue.Add(string.Format("SATA 6.0Gb/s is supported"));
                    }
                    if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.PowerReceipt))
                    {
                        ataOneValue.Add(string.Format("Receipt of host initiated power management requests is supported"));
                    }
                    if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.PHYEventCounter))
                    {
                        ataOneValue.Add(string.Format("PHY Event counters are supported"));
                    }
                    if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.HostSlumbTrans))
                    {
                        ataOneValue.Add(string.Format("Supports host automatic partial to slumber transitions is supported"));
                    }
                    if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.DevSlumbTrans))
                    {
                        ataOneValue.Add(string.Format("Supports device automatic partial to slumber transitions is supported"));
                    }
                    if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.NCQ))
                    {
                        ataOneValue.Add(string.Format("NCQ is supported"));

                        if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.NCQPriority))
                        {
                            ataOneValue.Add(string.Format("NCQ priority is supported"));
                        }
                        if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.UnloadNCQ))
                        {
                            ataOneValue.Add(string.Format("Unload is supported with outstanding NCQ commands"));
                        }
                    }
                }

                if(ataReport.SATACapabilities2Specified && !ataReport.SATACapabilities2.HasFlag(SATACapabilitiesBit2.Clear))
                {
                    if(ataReport.SATACapabilitiesSpecified &&
                            !ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.Clear) &&
                        ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.NCQ))
                    {
                        if(ataReport.SATACapabilities2.HasFlag(SATACapabilitiesBit2.NCQMgmt))
                        {
                            ataOneValue.Add(string.Format("NCQ queue management is supported"));
                        }
                        if(ataReport.SATACapabilities2.HasFlag(SATACapabilitiesBit2.NCQStream))
                        {
                            ataOneValue.Add(string.Format("NCQ streaming is supported"));
                        }
                    }

                    if(ataReport.SATACapabilities2Specified && atapi)
                    {
                        if(ataReport.SATACapabilities2.HasFlag(SATACapabilitiesBit2.HostEnvDetect))
                        {
                            ataOneValue.Add(string.Format("ATAPI device supports host environment detection"));
                        }
                        if(ataReport.SATACapabilities2.HasFlag(SATACapabilitiesBit2.DevAttSlimline))
                        {
                            ataOneValue.Add(string.Format("ATAPI device supports attention on slimline connected devices"));
                        }
                    }
                }
            }

            if(ataReport.InterseekDelay != 0x0000 && ataReport.InterseekDelay != 0xFFFF)
            {
                ataOneValue.Add(string.Format("{0} microseconds of interseek delay for ISO-7779 accoustic testing", ataReport.InterseekDelay));
            }

            if((ushort)ataReport.DeviceFormFactor != 0x0000 && (ushort)ataReport.DeviceFormFactor != 0xFFFF)
            {
                switch(ataReport.DeviceFormFactor)
                {
                    case DeviceFormFactorEnum.FiveAndQuarter:
                        ataOneValue.Add("Device nominal size is 5.25\"");
                        break;
                    case DeviceFormFactorEnum.ThreeAndHalf:
                        ataOneValue.Add("Device nominal size is 3.5\"");
                        break;
                    case DeviceFormFactorEnum.TwoAndHalf:
                        ataOneValue.Add("Device nominal size is 2.5\"");
                        break;
                    case DeviceFormFactorEnum.OnePointEight:
                        ataOneValue.Add("Device nominal size is 1.8\"");
                        break;
                    case DeviceFormFactorEnum.LessThanOnePointEight:
                        ataOneValue.Add("Device nominal size is smaller than 1.8\"");
                        break;
                    default:
                        ataOneValue.Add(string.Format("Device nominal size field value {0} is unknown", ataReport.DeviceFormFactor));
                        break;
                }
            }

            if(atapi)
            {
                if(ataReport.ATAPIByteCount > 0)
                    ataOneValue.Add(string.Format("{0} bytes count limit for ATAPI", ataReport.ATAPIByteCount));
            }

            if(cfa)
            {
                if((ataReport.CFAPowerMode & 0x8000) == 0x8000)
                {
                    ataOneValue.Add("CompactFlash device supports power mode 1");
                    if((ataReport.CFAPowerMode & 0x2000) == 0x2000)
                        ataOneValue.Add("CompactFlash power mode 1 required for one or more commands");
                    if((ataReport.CFAPowerMode & 0x1000) == 0x1000)
                        ataOneValue.Add("CompactFlash power mode 1 is disabled");

                    ataOneValue.Add(string.Format("CompactFlash device uses a maximum of {0} mA", (ataReport.CFAPowerMode & 0x0FFF)));
                }
            }

            if(ataReport.CommandSetSpecified || ataReport.CommandSet2Specified || ataReport.CommandSet3Specified || ataReport.CommandSet4Specified || ataReport.CommandSet5Specified)
                ataOneValue.Add("<i>Command set and features:</i>");
            if(ataReport.CommandSetSpecified)
            {
                if(ataReport.CommandSet.HasFlag(CommandSetBit.Nop))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.Nop) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("NOP is supported and enabled");
                    else
                        ataOneValue.Add("NOP is supported");
                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.ReadBuffer))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.ReadBuffer) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("READ BUFFER is supported and enabled");
                    else
                        ataOneValue.Add("READ BUFFER is supported");
                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.WriteBuffer))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.WriteBuffer) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("WRITE BUFFER is supported and enabled");
                    else
                        ataOneValue.Add("WRITE BUFFER is supported");

                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.HPA))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.HPA) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("Host Protected Area is supported and enabled");
                    else
                        ataOneValue.Add("Host Protected Area is supported");

                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.DeviceReset))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.DeviceReset) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("DEVICE RESET is supported and enabled");
                    else
                        ataOneValue.Add("DEVICE RESET is supported");

                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.Service))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.Service) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("SERVICE interrupt is supported and enabled");
                    else
                        ataOneValue.Add("SERVICE interrupt is supported");

                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.Release))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.Release) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("Release is supported and enabled");
                    else
                        ataOneValue.Add("Release is supported");

                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.LookAhead))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.LookAhead) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("Look-ahead read is supported and enabled");
                    else
                        ataOneValue.Add("Look-ahead read is supported");

                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.WriteCache))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.WriteCache) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("Write cache is supported and enabled");
                    else
                        ataOneValue.Add("Write cache is supported");

                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.Packet))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.Packet) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("PACKET is supported and enabled");
                    else
                        ataOneValue.Add("PACKET is supported");

                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.PowerManagement))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.PowerManagement) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("Power management is supported and enabled");
                    else
                        ataOneValue.Add("Power management is supported");

                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.RemovableMedia))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.RemovableMedia) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("Removable media feature set is supported and enabled");
                    else
                        ataOneValue.Add("Removable media feature set is supported");

                }
                if(ataReport.CommandSet.HasFlag(CommandSetBit.SecurityMode))
                {
                    if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.SecurityMode) && ataReport.EnabledCommandSetSpecified)
                        ataOneValue.Add("Security mode is supported and enabled");
                    else
                        ataOneValue.Add("Security mode is supported");

                }
                if(ataReport.Capabilities.HasFlag(CapabilitiesBit.LBASupport))
                    ataOneValue.Add("28-bit LBA is supported");
            }

            if(ataReport.CommandSet2Specified && ataReport.CommandSet2.HasFlag(CommandSetBit2.MustBeSet) &&
                    !ataReport.CommandSet2.HasFlag(CommandSetBit2.MustBeClear))
            {
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.LBA48))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.LBA48) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("48-bit LBA is supported and enabled");
                    else
                        ataOneValue.Add("48-bit LBA is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.FlushCache))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.FlushCache) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("FLUSH CACHE is supported and enabled");
                    else
                        ataOneValue.Add("FLUSH CACHE is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.FlushCacheExt))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.FlushCacheExt) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("FLUSH CACHE EXT is supported and enabled");
                    else
                        ataOneValue.Add("FLUSH CACHE EXT is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.DCO))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.DCO) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("Device Configuration Overlay feature set is supported and enabled");
                    else
                        ataOneValue.Add("Device Configuration Overlay feature set is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.AAM))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.AAM) && ataReport.EnabledCommandSet2Specified)
                    {
                        ataOneValue.Add(string.Format("Automatic Acoustic Management is supported and enabled with value {0} (vendor recommends {1}",
                                                      ataReport.CurrentAAM, ataReport.RecommendedAAM));
                    }
                    else
                        ataOneValue.Add("Automatic Acoustic Management is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.SetMax))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.SetMax) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("SET MAX security extension is supported and enabled");
                    else
                        ataOneValue.Add("SET MAX security extension is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.AddressOffsetReservedAreaBoot))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.AddressOffsetReservedAreaBoot) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("Address Offset Reserved Area Boot is supported and enabled");
                    else
                        ataOneValue.Add("Address Offset Reserved Area Boot is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.SetFeaturesRequired))
                {
                    ataOneValue.Add("SET FEATURES is required before spin-up");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.PowerUpInStandby))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.PowerUpInStandby) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("Power-up in standby is supported and enabled");
                    else
                        ataOneValue.Add("Power-up in standby is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.RemovableNotification))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.RemovableNotification) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("Removable Media Status Notification is supported and enabled");
                    else
                        ataOneValue.Add("Removable Media Status Notification is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.APM))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.APM) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add(string.Format("Advanced Power Management is supported and enabled with value {0}", ataReport.CurrentAPM));
                    else
                        ataOneValue.Add("Advanced Power Management is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.CompactFlash))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.CompactFlash) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("CompactFlash feature set is supported and enabled");
                    else
                        ataOneValue.Add("CompactFlash feature set is supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.RWQueuedDMA))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.RWQueuedDMA) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("READ DMA QUEUED and WRITE DMA QUEUED are supported and enabled");
                    else
                        ataOneValue.Add("READ DMA QUEUED and WRITE DMA QUEUED are supported");
                }
                if(ataReport.CommandSet2.HasFlag(CommandSetBit2.DownloadMicrocode))
                {
                    if(ataReport.EnabledCommandSet2.HasFlag(CommandSetBit2.DownloadMicrocode) && ataReport.EnabledCommandSet2Specified)
                        ataOneValue.Add("DOWNLOAD MICROCODE is supported and enabled");
                    else
                        ataOneValue.Add("DOWNLOAD MICROCODE is supported");
                }
            }

            if(ataReport.CommandSet.HasFlag(CommandSetBit.SMART) && ataReport.CommandSetSpecified)
            {
                if(ataReport.EnabledCommandSet.HasFlag(CommandSetBit.SMART) && ataReport.EnabledCommandSetSpecified)
                    ataOneValue.Add("S.M.A.R.T. is supported and enabled");
                else
                    ataOneValue.Add("S.M.A.R.T. is supported");
            }

            if(ataReport.SCTCommandTransport.HasFlag(SCTCommandTransportBit.Supported) && ataReport.SCTCommandTransportSpecified)
                ataOneValue.Add("S.M.A.R.T. Command Transport is supported");

            if(ataReport.CommandSet3Specified &&
                ataReport.CommandSet3.HasFlag(CommandSetBit3.MustBeSet) &&
       !ataReport.CommandSet3.HasFlag(CommandSetBit3.MustBeClear))
            {
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.SMARTSelfTest))
                {
                    if(ataReport.EnabledCommandSet3.HasFlag(CommandSetBit3.SMARTSelfTest) && ataReport.EnabledCommandSet3Specified)
                        ataOneValue.Add("S.M.A.R.T. self-testing is supported and enabled");
                    else
                        ataOneValue.Add("S.M.A.R.T. self-testing is supported");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.SMARTLog))
                {
                    if(ataReport.EnabledCommandSet3.HasFlag(CommandSetBit3.SMARTLog) && ataReport.EnabledCommandSet3Specified)
                        ataOneValue.Add("S.M.A.R.T. error logging is supported and enabled");
                    else
                        ataOneValue.Add("S.M.A.R.T. error logging is supported");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.IdleImmediate))
                {
                    if(ataReport.EnabledCommandSet3.HasFlag(CommandSetBit3.IdleImmediate) && ataReport.EnabledCommandSet3Specified)
                        ataOneValue.Add("IDLE IMMEDIATE with UNLOAD FEATURE is supported and enabled");
                    else
                        ataOneValue.Add("IDLE IMMEDIATE with UNLOAD FEATURE is supported");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.WriteURG))
                {
                    ataOneValue.Add("URG bit is supported in WRITE STREAM DMA EXT and WRITE STREAM EXT");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.ReadURG))
                {
                    ataOneValue.Add("URG bit is supported in READ STREAM DMA EXT and READ STREAM EXT");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.WWN))
                {
                    ataOneValue.Add("Device has a World Wide Name");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.FUAWriteQ))
                {
                    if(ataReport.EnabledCommandSet3.HasFlag(CommandSetBit3.FUAWriteQ) && ataReport.EnabledCommandSet3Specified)
                        ataOneValue.Add("WRITE DMA QUEUED FUA EXT is supported and enabled");
                    else
                        ataOneValue.Add("WRITE DMA QUEUED FUA EXT is supported");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.FUAWrite))
                {
                    if(ataReport.EnabledCommandSet3.HasFlag(CommandSetBit3.FUAWrite) && ataReport.EnabledCommandSet3Specified)
                        ataOneValue.Add("WRITE DMA FUA EXT and WRITE MULTIPLE FUA EXT are supported and enabled");
                    else
                        ataOneValue.Add("WRITE DMA FUA EXT and WRITE MULTIPLE FUA EXT are supported");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.GPL))
                {
                    if(ataReport.EnabledCommandSet3.HasFlag(CommandSetBit3.GPL) && ataReport.EnabledCommandSet3Specified)
                        ataOneValue.Add("General Purpose Logging is supported and enabled");
                    else
                        ataOneValue.Add("General Purpose Logging is supported");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.Streaming))
                {
                    if(ataReport.EnabledCommandSet3.HasFlag(CommandSetBit3.Streaming) && ataReport.EnabledCommandSet3Specified)
                        ataOneValue.Add("Streaming feature set is supported and enabled");
                    else
                        ataOneValue.Add("Streaming feature set is supported");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.MCPT) && ataReport.EnabledCommandSet3Specified)
                {
                    if(ataReport.EnabledCommandSet3.HasFlag(CommandSetBit3.MCPT))
                        ataOneValue.Add("Media Card Pass Through command set is supported and enabled");
                    else
                        ataOneValue.Add("Media Card Pass Through command set is supported");
                }
                if(ataReport.CommandSet3.HasFlag(CommandSetBit3.MediaSerial) && ataReport.EnabledCommandSet3Specified)
                {
                    if(ataReport.EnabledCommandSet3.HasFlag(CommandSetBit3.MediaSerial))
                        ataOneValue.Add("Media Serial is supported and valid");
                    else
                        ataOneValue.Add("Media Serial is supported");
                }
            }

            if(ataReport.CommandSet4Specified &&
               ataReport.CommandSet4.HasFlag(CommandSetBit4.MustBeSet) &&
        !ataReport.CommandSet4.HasFlag(CommandSetBit4.MustBeClear))
            {
                if(ataReport.CommandSet4.HasFlag(CommandSetBit4.DSN))
                {
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.DSN) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add("DSN feature set is supported and enabled");
                    else
                        ataOneValue.Add("DSN feature set is supported");
                }
                if(ataReport.CommandSet4.HasFlag(CommandSetBit4.AMAC))
                {
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.AMAC) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add("Accessible Max Address Configuration is supported and enabled");
                    else
                        ataOneValue.Add("Accessible Max Address Configuration is supported");
                }
                if(ataReport.CommandSet4.HasFlag(CommandSetBit4.ExtPowerCond))
                {
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.ExtPowerCond) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add("Extended Power Conditions are supported and enabled");
                    else
                        ataOneValue.Add("Extended Power Conditions are supported");
                }
                if(ataReport.CommandSet4.HasFlag(CommandSetBit4.ExtStatusReport))
                {
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.ExtStatusReport) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add("Extended Status Reporting is supported and enabled");
                    else
                        ataOneValue.Add("Extended Status Reporting is supported");
                }
                if(ataReport.CommandSet4.HasFlag(CommandSetBit4.FreeFallControl))
                {
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.FreeFallControl) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add("Free-fall control feature set is supported and enabled");
                    else
                        ataOneValue.Add("Free-fall control feature set is supported");
                }
                if(ataReport.CommandSet4.HasFlag(CommandSetBit4.SegmentedDownloadMicrocode))
                {
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.SegmentedDownloadMicrocode) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add("Segmented feature in DOWNLOAD MICROCODE is supported and enabled");
                    else
                        ataOneValue.Add("Segmented feature in DOWNLOAD MICROCODE is supported");
                }
                if(ataReport.CommandSet4.HasFlag(CommandSetBit4.RWDMAExtGpl))
                {
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.RWDMAExtGpl) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add("READ/WRITE DMA EXT GPL are supported and enabled");
                    else
                        ataOneValue.Add("READ/WRITE DMA EXT GPL are supported");
                }
                if(ataReport.CommandSet4.HasFlag(CommandSetBit4.WriteUnc))
                {
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.WriteUnc) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add("WRITE UNCORRECTABLE is supported and enabled");
                    else
                        ataOneValue.Add("WRITE UNCORRECTABLE is supported");
                }
                if(ataReport.CommandSet4.HasFlag(CommandSetBit4.WRV))
                {
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.WRV) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add("Write/Read/Verify is supported and enabled");
                    else
                        ataOneValue.Add("Write/Read/Verify is supported");
                    ataOneValue.Add(string.Format("{0} sectors for Write/Read/Verify mode 2", ataReport.WRVSectorCountMode2));
                    ataOneValue.Add(string.Format("{0} sectors for Write/Read/Verify mode 3", ataReport.WRVSectorCountMode3));
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.WRV) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add(string.Format("Current Write/Read/Verify mode: {0}", ataReport.WRVMode));
                }
                if(ataReport.CommandSet4.HasFlag(CommandSetBit4.DT1825))
                {
                    if(ataReport.EnabledCommandSet4.HasFlag(CommandSetBit4.DT1825) && ataReport.EnabledCommandSet4Specified)
                        ataOneValue.Add("DT1825 is supported and enabled");
                    else
                        ataOneValue.Add("DT1825 is supported");
                }
            }

            if(ataReport.Capabilities3Specified)
            {
                if(ataReport.Capabilities3.HasFlag(CapabilitiesBit3.BlockErase))
                    ataOneValue.Add("BLOCK ERASE EXT is supported");
                if(ataReport.Capabilities3.HasFlag(CapabilitiesBit3.Overwrite))
                    ataOneValue.Add("OVERWRITE EXT is supported");
                if(ataReport.Capabilities3.HasFlag(CapabilitiesBit3.CryptoScramble))
                    ataOneValue.Add("CRYPTO SCRAMBLE EXT is supported");
            }

            if(ataReport.CommandSet5Specified)
            {
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.DeviceConfDMA))
                {
                    ataOneValue.Add("DEVICE CONFIGURATION IDENTIFY DMA and DEVICE CONFIGURATION SET DMA are supported");
                }
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.ReadBufferDMA))
                {
                    ataOneValue.Add("READ BUFFER DMA is supported");
                }
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.WriteBufferDMA))
                {
                    ataOneValue.Add("WRITE BUFFER DMA is supported");
                }
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.DownloadMicroCodeDMA))
                {
                    ataOneValue.Add("DOWNLOAD MICROCODE DMA is supported");
                }
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.SetMaxDMA))
                {
                    ataOneValue.Add("SET PASSWORD DMA and SET UNLOCK DMA are supported");
                }
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.Ata28))
                {
                    ataOneValue.Add("Not all 28-bit commands are supported");
                }

                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.CFast))
                {
                    ataOneValue.Add("Device follows CFast specification");
                }
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.IEEE1667))
                {
                    ataOneValue.Add("Device follows IEEE-1667");
                }

                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.DeterministicTrim))
                {
                    ataOneValue.Add("Read after TRIM is deterministic");
                    if(ataReport.CommandSet5.HasFlag(CommandSetBit5.ReadZeroTrim))
                    {
                        ataOneValue.Add("Read after TRIM returns empty data");
                    }
                }
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.LongPhysSectorAligError))
                {
                    ataOneValue.Add("Device supports Long Physical Sector Alignment Error Reporting Control");
                }
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.Encrypted))
                {
                    ataOneValue.Add("Device encrypts all user data");
                }
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.AllCacheNV))
                {
                    ataOneValue.Add("Device's write cache is non-volatile");
                }
                if(ataReport.CommandSet5.HasFlag(CommandSetBit5.ZonedBit0) ||
                    ataReport.CommandSet5.HasFlag(CommandSetBit5.ZonedBit1))
                {
                    ataOneValue.Add("Device is zoned");
                }
            }

            if(ataReport.Capabilities3Specified)
            {
                if(ataReport.Capabilities3.HasFlag(CapabilitiesBit3.Sanitize))
                {
                    ataOneValue.Add("Sanitize feature set is supported");
                    if(ataReport.Capabilities3.HasFlag(CapabilitiesBit3.SanitizeCommands))
                        ataOneValue.Add("Sanitize commands are specified by ACS-3 or higher");
                    else
                        ataOneValue.Add("Sanitize commands are specified by ACS-2");

                    if(ataReport.Capabilities3.HasFlag(CapabilitiesBit3.SanitizeAntifreeze))
                        ataOneValue.Add("SANITIZE ANTIFREEZE LOCK EXT is supported");
                }
            }

            if(!ata1 && maxatalevel >= 8 && ataReport.TrustedComputingSpecified)
            {
                if(ataReport.TrustedComputing.HasFlag(TrustedComputingBit.Set) &&
                    !ataReport.TrustedComputing.HasFlag(TrustedComputingBit.Clear) &&
                    ataReport.TrustedComputing.HasFlag(TrustedComputingBit.TrustedComputing))
                    ataOneValue.Add("Trusted Computing feature set is supported");
            }

            if(ataReport.TransportMajorVersionSpecified &&
                (((ataReport.TransportMajorVersion & 0xF000) >> 12) == 0x1 ||
                 ((ataReport.TransportMajorVersion & 0xF000) >> 12) == 0xE))
            {
                if(ataReport.SATACapabilitiesSpecified)
                {
                    if(!ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.Clear))
                    {
                        if(ataReport.SATACapabilities.HasFlag(SATACapabilitiesBit.ReadLogDMAExt))
                            ataOneValue.Add("READ LOG DMA EXT is supported");
                    }
                }

                if(ataReport.SATACapabilities2Specified)
                {
                    if(!ataReport.SATACapabilities2.HasFlag(SATACapabilitiesBit2.Clear))
                    {
                        if(ataReport.SATACapabilities2.HasFlag(SATACapabilitiesBit2.FPDMAQ))
                            ataOneValue.Add("RECEIVE FPDMA QUEUED and SEND FPDMA QUEUED are supported");
                    }
                }

                if(ataReport.SATAFeaturesSpecified)
                {
                    if(!ataReport.SATAFeatures.HasFlag(SATAFeaturesBit.Clear))
                    {
                        if(ataReport.SATAFeatures.HasFlag(SATAFeaturesBit.NonZeroBufferOffset))
                        {
                            if(ataReport.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.NonZeroBufferOffset) && ataReport.EnabledSATAFeaturesSpecified)
                                ataOneValue.Add("Non-zero buffer offsets are supported and enabled");
                            else
                                ataOneValue.Add("Non-zero buffer offsets are supported");
                        }
                        if(ataReport.SATAFeatures.HasFlag(SATAFeaturesBit.DMASetup))
                        {
                            if(ataReport.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.DMASetup) && ataReport.EnabledSATAFeaturesSpecified)
                                ataOneValue.Add("DMA Setup auto-activation is supported and enabled");
                            else
                                ataOneValue.Add("DMA Setup auto-activation is supported");
                        }
                        if(ataReport.SATAFeatures.HasFlag(SATAFeaturesBit.InitPowerMgmt))
                        {
                            if(ataReport.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.InitPowerMgmt) && ataReport.EnabledSATAFeaturesSpecified)
                                ataOneValue.Add("Device-initiated power management is supported and enabled");
                            else
                                ataOneValue.Add("Device-initiated power management is supported");
                        }
                        if(ataReport.SATAFeatures.HasFlag(SATAFeaturesBit.InOrderData))
                        {
                            if(ataReport.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.InOrderData) && ataReport.EnabledSATAFeaturesSpecified)
                                ataOneValue.Add("In-order data delivery is supported and enabled");
                            else
                                ataOneValue.Add("In-order data delivery is supported");
                        }
                        if(!atapi)
                        {
                            if(ataReport.SATAFeatures.HasFlag(SATAFeaturesBit.HardwareFeatureControl))
                            {
                                if(ataReport.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.HardwareFeatureControl) && ataReport.EnabledSATAFeaturesSpecified)
                                    ataOneValue.Add("Hardware Feature Control is supported and enabled");
                                else
                                    ataOneValue.Add("Hardware Feature Control is supported");
                            }
                        }
                        if(atapi)
                        {
                            if(ataReport.SATAFeatures.HasFlag(SATAFeaturesBit.AsyncNotification) && ataReport.EnabledSATAFeaturesSpecified)
                            {
                                if(ataReport.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.AsyncNotification) && ataReport.EnabledSATAFeaturesSpecified)
                                    ataOneValue.Add("Asynchronous notification is supported");
                                else
                                    ataOneValue.Add("Asynchronous notification is supported");
                            }
                        }
                        if(ataReport.SATAFeatures.HasFlag(SATAFeaturesBit.SettingsPreserve))
                        {
                            if(ataReport.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.SettingsPreserve) && ataReport.EnabledSATAFeaturesSpecified)
                                ataOneValue.Add("Software Settings Preservation is supported");
                            else
                                ataOneValue.Add("Software Settings Preservation is supported");
                        }
                        if(ataReport.SATAFeatures.HasFlag(SATAFeaturesBit.NCQAutoSense))
                        {
                            ataOneValue.Add("NCQ Autosense is supported");
                        }
                        if(ataReport.EnabledSATAFeatures.HasFlag(SATAFeaturesBit.EnabledSlumber))
                        {
                            ataOneValue.Add("Automatic Partial to Slumber transitions are enabled");
                        }
                    }
                }
            }
            if((ataReport.RemovableStatusSet & 0x03) > 0)
            {
                ataOneValue.Add("Removable Media Status Notification feature set is supported");
            }

            if(ataReport.FreeFallSensitivity != 0x00 && ataReport.FreeFallSensitivity != 0xFF)
            {
                ataOneValue.Add(string.Format("Free-fall sensitivity set to {0}", ataReport.FreeFallSensitivity));
            }

            if(ataReport.DataSetMgmtSpecified && ataReport.DataSetMgmt.HasFlag(DataSetMgmtBit.Trim))
                ataOneValue.Add("TRIM is supported");
            if(ataReport.DataSetMgmtSizeSpecified && ataReport.DataSetMgmtSize > 0)
            {
                ataOneValue.Add(string.Format("DATA SET MANAGEMENT can receive a maximum of {0} blocks of 512 bytes", ataReport.DataSetMgmtSize));
            }

            if(ataReport.SecurityStatusSpecified && ataReport.SecurityStatus.HasFlag(SecurityStatusBit.Supported))
            {
                ataOneValue.Add("<i>Security:</i>");
                if(ataReport.SecurityStatus.HasFlag(SecurityStatusBit.Enabled))
                {
                    ataOneValue.Add("Security is enabled");
                    if(ataReport.SecurityStatus.HasFlag(SecurityStatusBit.Locked))
                        ataOneValue.Add("Security is locked");
                    else
                        ataOneValue.Add("Security is not locked");

                    if(ataReport.SecurityStatus.HasFlag(SecurityStatusBit.Frozen))
                        ataOneValue.Add("Security is frozen");
                    else
                        ataOneValue.Add("Security is not frozen");

                    if(ataReport.SecurityStatus.HasFlag(SecurityStatusBit.Expired))
                        ataOneValue.Add("Security count has expired");
                    else
                        ataOneValue.Add("Security count has notexpired");

                    if(ataReport.SecurityStatus.HasFlag(SecurityStatusBit.Maximum))
                        ataOneValue.Add("Security level is maximum");
                    else
                        ataOneValue.Add("Security level is high");
                }
                else
                    ataOneValue.Add("Security is not enabled");

                if(ataReport.SecurityStatus.HasFlag(SecurityStatusBit.Enhanced))
                    ataOneValue.Add("Supports enhanced security erase");

                ataOneValue.Add(string.Format("{0} minutes to complete secure erase", ataReport.SecurityEraseTime * 2));
                if(ataReport.SecurityStatus.HasFlag(SecurityStatusBit.Enhanced))
                    ataOneValue.Add(string.Format("{0} minutes to complete enhanced secure erase", ataReport.EnhancedSecurityEraseTime * 2));

                ataOneValue.Add(string.Format("Master password revision code: {0}", ataReport.MasterPasswordRevisionCode));
            }

            if(ataReport.CommandSet3Specified &&
                ataReport.CommandSet3.HasFlag(CommandSetBit3.MustBeSet) &&
!ataReport.CommandSet3.HasFlag(CommandSetBit3.MustBeClear) &&
ataReport.CommandSet3.HasFlag(CommandSetBit3.Streaming))
            {
                ataOneValue.Add("<i>Streaming:</i>");
                ataOneValue.Add(string.Format("Minimum request size is {0}", ataReport.StreamMinReqSize));
                ataOneValue.Add(string.Format("Streaming transfer time in PIO is {0}", ataReport.StreamTransferTimePIO));
                ataOneValue.Add(string.Format("Streaming transfer time in DMA is {0}", ataReport.StreamTransferTimeDMA));
                ataOneValue.Add(string.Format("Streaming access latency is {0}", ataReport.StreamAccessLatency));
                ataOneValue.Add(string.Format("Streaming performance granularity is {0}", ataReport.StreamPerformanceGranularity));
            }

            if(ataReport.SCTCommandTransportSpecified && ataReport.SCTCommandTransport.HasFlag(SCTCommandTransportBit.Supported))
            {
                ataOneValue.Add("<i>S.M.A.R.T. Command Transport (SCT):</i>");
                if(ataReport.SCTCommandTransport.HasFlag(SCTCommandTransportBit.LongSectorAccess))
                    ataOneValue.Add("SCT Long Sector Address is supported");
                if(ataReport.SCTCommandTransport.HasFlag(SCTCommandTransportBit.WriteSame))
                    ataOneValue.Add("SCT Write Same is supported");
                if(ataReport.SCTCommandTransport.HasFlag(SCTCommandTransportBit.ErrorRecoveryControl))
                    ataOneValue.Add("SCT Error Recovery Control is supported");
                if(ataReport.SCTCommandTransport.HasFlag(SCTCommandTransportBit.FeaturesControl))
                    ataOneValue.Add("SCT Features Control is supported");
                if(ataReport.SCTCommandTransport.HasFlag(SCTCommandTransportBit.DataTables))
                    ataOneValue.Add("SCT Data Tables are supported");
            }

            if(ataReport.NVCacheCapsSpecified && (ataReport.NVCacheCaps & 0x0010) == 0x0010)
            {
                ataOneValue.Add("<i>Non-Volatile Cache:</i>");
                ataOneValue.Add(string.Format("Version {0}", (ataReport.NVCacheCaps & 0xF000) >> 12));
                if((ataReport.NVCacheCaps & 0x0001) == 0x0001)
                {
                    if((ataReport.NVCacheCaps & 0x0002) == 0x0002)
                        ataOneValue.Add("Power mode feature set is supported and enabled");
                    else
                        ataOneValue.Add("Power mode feature set is supported");

                    ataOneValue.Add(string.Format("Version {0}", (ataReport.NVCacheCaps & 0x0F00) >> 8));
                }
                ataOneValue.Add(string.Format("Non-Volatile Cache is {0} bytes", ataReport.NVCacheSize * logicalsectorsize));
            }

            if(ataReport.ReadCapabilities != null)
            {
                removable = false;
                ataOneValue.Add("");

                if(ataReport.ReadCapabilities.NominalRotationRateSpecified &&
                   ataReport.ReadCapabilities.NominalRotationRate != 0x0000 &&
                   ataReport.ReadCapabilities.NominalRotationRate != 0xFFFF)
                {
                    if(ataReport.ReadCapabilities.NominalRotationRate == 0x0001)
                        ataOneValue.Add("Device does not rotate.");
                    else
                        ataOneValue.Add(string.Format("Device rotates at {0} rpm", ataReport.ReadCapabilities.NominalRotationRate));
                }

                if(!atapi)
                {
                    if(ataReport.ReadCapabilities.BlockSizeSpecified)
                    {
                        ataTwoValue.Add("Logical sector size", string.Format("{0} bytes", ataReport.ReadCapabilities.BlockSize));
                        logicalsectorsize = ataReport.ReadCapabilities.BlockSize;
                    }
                    if(ataReport.ReadCapabilities.PhysicalBlockSizeSpecified)
                        ataTwoValue.Add("Physical sector size", string.Format("{0} bytes", ataReport.ReadCapabilities.PhysicalBlockSize));
                    if(ataReport.ReadCapabilities.LongBlockSizeSpecified)
                        ataTwoValue.Add("READ LONG sector size", string.Format("{0} bytes", ataReport.ReadCapabilities.LongBlockSize));


                    if(ataReport.ReadCapabilities.BlockSizeSpecified &&
                       ataReport.ReadCapabilities.PhysicalBlockSizeSpecified &&
                       (ataReport.ReadCapabilities.BlockSize != ataReport.ReadCapabilities.PhysicalBlockSize) &&
                        (ataReport.ReadCapabilities.LogicalAlignment & 0x8000) == 0x0000 &&
                        (ataReport.ReadCapabilities.LogicalAlignment & 0x4000) == 0x4000)
                    {
                        ataOneValue.Add(string.Format("Logical sector starts at offset {0} from physical sector", ataReport.ReadCapabilities.LogicalAlignment & 0x3FFF));
                    }

                    if(ataReport.ReadCapabilities.CHS != null &&
                       ataReport.ReadCapabilities.CurrentCHS != null)
                    {
                        int currentSectors = ataReport.ReadCapabilities.CurrentCHS.Cylinders * ataReport.ReadCapabilities.CurrentCHS.Heads * ataReport.ReadCapabilities.CurrentCHS.Sectors;
                        ataTwoValue.Add("Cylinders", string.Format("{0} max., {1} current", ataReport.ReadCapabilities.CHS.Cylinders, ataReport.ReadCapabilities.CurrentCHS.Cylinders));
                        ataTwoValue.Add("Heads", string.Format("{0} max., {1} current", ataReport.ReadCapabilities.CHS.Heads, ataReport.ReadCapabilities.CurrentCHS.Heads));
                        ataTwoValue.Add("Sectors per track", string.Format("{0} max., {1} current", ataReport.ReadCapabilities.CHS.Sectors, ataReport.ReadCapabilities.CurrentCHS.Sectors));
                        ataTwoValue.Add("Sectors addressable in CHS mode", string.Format("{0} max., {1} current", ataReport.ReadCapabilities.CHS.Cylinders * ataReport.ReadCapabilities.CHS.Heads * ataReport.ReadCapabilities.CHS.Sectors,
                                                                                         currentSectors));
                        ataTwoValue.Add("Device size in CHS mode", string.Format("{0} bytes, {1} Mb, {2:F2} MiB", (ulong)currentSectors * logicalsectorsize,
                            ((ulong)currentSectors * logicalsectorsize) / 1000 / 1000, (double)((ulong)currentSectors * logicalsectorsize) / 1024 / 1024));
                    }
                    else if(ataReport.ReadCapabilities.CHS != null)
                    {
                        int currentSectors = ataReport.ReadCapabilities.CHS.Cylinders * ataReport.ReadCapabilities.CHS.Heads * ataReport.ReadCapabilities.CHS.Sectors;
                        ataTwoValue.Add("Cylinders", string.Format("{0}", ataReport.ReadCapabilities.CHS.Cylinders));
                        ataTwoValue.Add("Heads", string.Format("{0}", ataReport.ReadCapabilities.CHS.Heads));
                        ataTwoValue.Add("Sectors per track", string.Format("{0}", ataReport.ReadCapabilities.CHS.Sectors));
                        ataTwoValue.Add("Sectors addressable in CHS mode", string.Format("{0}", currentSectors));
                        ataTwoValue.Add("Device size in CHS mode", string.Format("{0} bytes, {1} Mb, {2:F2} MiB", (ulong)currentSectors * logicalsectorsize,
                            ((ulong)currentSectors * logicalsectorsize) / 1000 / 1000, (double)((ulong)currentSectors * logicalsectorsize) / 1024 / 1024));
                    }

                    if(ataReport.ReadCapabilities.LBASectorsSpecified)
                    {
                        ataTwoValue.Add("Sectors addressable in sectors in 28-bit LBA mode", string.Format("{0}", ataReport.ReadCapabilities.LBASectors));

                        if((((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1024 / 1024) > 1000000)
                        {
                            ataTwoValue.Add("Device size in 28-bit LBA mode", string.Format("{0} bytes, {1} Tb, {2:F2} TiB", (ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize,
                                ((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1000 / 1000 / 1000 / 1000, (double)((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1024 / 1024 / 1024 / 1024));
                        }
                        else if((((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1024 / 1024) > 1000)
                        {
                            ataTwoValue.Add("Device size in 28-bit LBA mode", string.Format("{0} bytes, {1} Gb, {2:F2} GiB", (ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize,
                                ((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1000 / 1000 / 1000, (double)((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1024 / 1024 / 1024));
                        }
                        else
                        {
                            ataTwoValue.Add("Device size in 28-bit LBA mode", string.Format("{0} bytes, {1} Mb, {2:F2} MiB", (ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize,
                                ((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1000 / 1000, (double)((ulong)ataReport.ReadCapabilities.LBASectors * logicalsectorsize) / 1024 / 1024));
                        }
                    }

                    if(ataReport.ReadCapabilities.LBA48SectorsSpecified)
                    {
                        ataTwoValue.Add("Sectors addressable in sectors in 48-bit LBA mode", string.Format("{0}", ataReport.ReadCapabilities.LBA48Sectors));

                        if(((ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1024 / 1024) > 1000000)
                        {
                            ataTwoValue.Add("Device size in 48-bit LBA mode", string.Format("{0} bytes, {1} Tb, {2:F2} TiB", ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize,
                                (ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1000 / 1000 / 1000 / 1000, (double)(ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1024 / 1024 / 1024 / 1024));
                        }
                        else if(((ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1024 / 1024) > 1000)
                        {
                            ataTwoValue.Add("Device size in 48-bit LBA mode", string.Format("{0} bytes, {1} Gb, {2:F2} GiB", ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize,
                                (ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1000 / 1000 / 1000, (double)(ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1024 / 1024 / 1024));
                        }
                        else
                        {
                            ataTwoValue.Add("Device size in 48-bit LBA mode", string.Format("{0} bytes, {1} Mb, {2:F2} MiB", ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize,
                                (ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1000 / 1000, (double)(ataReport.ReadCapabilities.LBA48Sectors * logicalsectorsize) / 1024 / 1024));
                        }
                    }

                    if(ata1 || cfa)
                    {
                        if(ataReport.ReadCapabilities.UnformattedBPT > 0)
                            ataTwoValue.Add("Bytes per unformatted track", string.Format("{0}", ataReport.ReadCapabilities.UnformattedBPT));
                        if(ataReport.ReadCapabilities.UnformattedBPS > 0)
                            ataTwoValue.Add("Bytes per unformatted sector", string.Format("{0}", ataReport.ReadCapabilities.UnformattedBPS));
                    }
                }

                if(ataReport.ReadCapabilities.SupportsRead)
                    ataOneValue.Add("Device supports READ SECTOR(S) command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsReadRetry)
                    ataOneValue.Add("Device supports READ SECTOR(S) RETRY command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsReadDma)
                    ataOneValue.Add("Device supports READ DMA command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsReadDmaRetry)
                    ataOneValue.Add("Device supports READ DMA RETRY command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsReadLong)
                    ataOneValue.Add("Device supports READ LONG command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsReadLongRetry)
                    ataOneValue.Add("Device supports READ LONG RETRY command in CHS mode");
                
                if(ataReport.ReadCapabilities.SupportsReadLba)
                    ataOneValue.Add("Device supports READ SECTOR(S) command in 28-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadRetryLba)
                    ataOneValue.Add("Device supports READ SECTOR(S) RETRY command in 28-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadDmaLba)
                    ataOneValue.Add("Device supports READ DMA command in 28-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadDmaRetryLba)
                    ataOneValue.Add("Device supports READ DMA RETRY command in 28-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadLongLba)
                    ataOneValue.Add("Device supports READ LONG command in 28-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadLongRetryLba)
                    ataOneValue.Add("Device supports READ LONG RETRY command in 28-bit LBA mode");

                if(ataReport.ReadCapabilities.SupportsReadLba48)
                    ataOneValue.Add("Device supports READ SECTOR(S) command in 48-bit LBA mode");
                if(ataReport.ReadCapabilities.SupportsReadDmaLba48)
                    ataOneValue.Add("Device supports READ DMA command in 48-bit LBA mode");

                if(ataReport.ReadCapabilities.SupportsSeek)
                    ataOneValue.Add("Device supports SEEK command in CHS mode");
                if(ataReport.ReadCapabilities.SupportsSeekLba)
                    ataOneValue.Add("Device supports SEEK command in 28-bit LBA mode");
            }
            else
                testedMedia = ataReport.RemovableMedias;
        }
    }
}
