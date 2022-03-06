// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Identify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes ATA IDENTIFY DEVICE response.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.Structs.Devices.SCSI;

namespace Aaru.Decoders.ATA;

// Information from following standards:
// T10-791D rev. 4c (ATA)
// T10-948D rev. 4c (ATA-2)
// T13-1153D rev. 18 (ATA/ATAPI-4)
// T13-1321D rev. 3 (ATA/ATAPI-5)
// T13-1410D rev. 3b (ATA/ATAPI-6)
// T13-1532D rev. 4b (ATA/ATAPI-7)
// T13-1699D rev. 3f (ATA8-ACS)
// T13-1699D rev. 4a (ATA8-ACS)
// T13-2015D rev. 2 (ACS-2)
// T13-2161D rev. 5 (ACS-3)
// CF+ & CF Specification rev. 1.4 (CFA)
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Identify
{
    public static string Prettify(byte[] IdentifyDeviceResponse)
    {
        if(IdentifyDeviceResponse.Length != 512)
            return null;

        CommonTypes.Structs.Devices.ATA.Identify.IdentifyDevice? decoded =
            CommonTypes.Structs.Devices.ATA.Identify.Decode(IdentifyDeviceResponse);

        return Prettify(decoded);
    }

    public static string Prettify(CommonTypes.Structs.Devices.ATA.Identify.IdentifyDevice? IdentifyDeviceResponse)
    {
        if(IdentifyDeviceResponse == null)
            return null;

        var sb = new StringBuilder();

        bool atapi = false;
        bool cfa   = false;

        CommonTypes.Structs.Devices.ATA.Identify.IdentifyDevice ATAID = IdentifyDeviceResponse.Value;

        if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.
                                                          NonMagnetic))
            if((ushort)ATAID.GeneralConfiguration != 0x848A)
                atapi = true;
            else
                cfa = true;

        if(atapi)
            sb.AppendLine("ATAPI device");
        else if(cfa)
            sb.AppendLine("CompactFlash device");
        else
            sb.AppendLine("ATA device");

        if(ATAID.Model != "")
            sb.AppendFormat("Model: {0}", ATAID.Model).AppendLine();

        if(ATAID.FirmwareRevision != "")
            sb.AppendFormat("Firmware revision: {0}", ATAID.FirmwareRevision).AppendLine();

        if(ATAID.SerialNumber != "")
            sb.AppendFormat("Serial #: {0}", ATAID.SerialNumber).AppendLine();

        if(ATAID.AdditionalPID != "")
            sb.AppendFormat("Additional product ID: {0}", ATAID.AdditionalPID).AppendLine();

        if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeSet) &&
           !ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeClear))
        {
            if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.
                                                            MediaSerial))
            {
                if(ATAID.MediaManufacturer != "")
                    sb.AppendFormat("Media manufacturer: {0}", ATAID.MediaManufacturer).AppendLine();

                if(ATAID.MediaSerial != "")
                    sb.AppendFormat("Media serial #: {0}", ATAID.MediaSerial).AppendLine();
            }

            if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.WWN))
                sb.AppendFormat("World Wide Name: {0:X16}", ATAID.WWN).AppendLine();
        }

        bool ata1 = false, ata2 = false, ata3 = false, ata4 = false, ata5 = false, ata6 = false, ata7 = false,
             acs  = false, acs2 = false, acs3 = false, acs4 = false;

        if((ushort)ATAID.MajorVersion == 0x0000 ||
           (ushort)ATAID.MajorVersion == 0xFFFF)
        {
            // Obsolete in ATA-2, if present, device supports ATA-1
            ata1 |=
                ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.
                                                               FastIDE) ||
                ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.
                                                               SlowIDE) ||
                ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.
                                                               UltraFastIDE);

            ata2 |= ATAID.ExtendedIdentify.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.ExtendedIdentifyBit.
                                                               Words64to70Valid);

            if(!ata1  &&
               !ata2  &&
               !atapi &&
               !cfa)
                ata2 = true;

            ata4 |= atapi;
            ata3 |= cfa;

            if(cfa && ata1)
                ata1 = false;

            if(cfa && ata2)
                ata2 = false;

            ata5 |= ATAID.Signature == 0xA5;
        }
        else
        {
            ata1 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.Ata1);
            ata2 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.Ata2);
            ata3 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.Ata3);
            ata4 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.AtaAtapi4);
            ata5 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.AtaAtapi5);
            ata6 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.AtaAtapi6);
            ata7 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.AtaAtapi7);
            acs  |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.Ata8ACS);
            acs2 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.ACS2);
            acs3 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.ACS3);
            acs4 |= ATAID.MajorVersion.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.MajorVersionBit.ACS4);
        }

        int maxatalevel = 0;
        int minatalevel = 255;
        sb.Append("Supported ATA versions: ");

        if(ata1)
        {
            sb.Append("ATA-1 ");
            maxatalevel = 1;
            minatalevel = 1;
        }

        if(ata2)
        {
            sb.Append("ATA-2 ");
            maxatalevel = 2;

            if(minatalevel > 2)
                minatalevel = 2;
        }

        if(ata3)
        {
            sb.Append("ATA-3 ");
            maxatalevel = 3;

            if(minatalevel > 3)
                minatalevel = 3;
        }

        if(ata4)
        {
            sb.Append("ATA/ATAPI-4 ");
            maxatalevel = 4;

            if(minatalevel > 4)
                minatalevel = 4;
        }

        if(ata5)
        {
            sb.Append("ATA/ATAPI-5 ");
            maxatalevel = 5;

            if(minatalevel > 5)
                minatalevel = 5;
        }

        if(ata6)
        {
            sb.Append("ATA/ATAPI-6 ");
            maxatalevel = 6;

            if(minatalevel > 6)
                minatalevel = 6;
        }

        if(ata7)
        {
            sb.Append("ATA/ATAPI-7 ");
            maxatalevel = 7;

            if(minatalevel > 7)
                minatalevel = 7;
        }

        if(acs)
        {
            sb.Append("ATA8-ACS ");
            maxatalevel = 8;

            if(minatalevel > 8)
                minatalevel = 8;
        }

        if(acs2)
        {
            sb.Append("ATA8-ACS2 ");
            maxatalevel = 9;

            if(minatalevel > 9)
                minatalevel = 9;
        }

        if(acs3)
        {
            sb.Append("ATA8-ACS3 ");
            maxatalevel = 10;

            if(minatalevel > 10)
                minatalevel = 10;
        }

        if(acs4)
        {
            sb.Append("ATA8-ACS4 ");
            maxatalevel = 11;

            if(minatalevel > 11)
                minatalevel = 11;
        }

        sb.AppendLine();

        sb.Append("Maximum ATA revision supported: ");

        if(maxatalevel >= 3)
            switch(ATAID.MinorVersion)
            {
                case 0x0000:
                case 0xFFFF:
                    sb.AppendLine("Minor ATA version not specified");

                    break;
                case 0x0001:
                    sb.AppendLine("ATA (ATA-1) X3T9.2 781D prior to revision 4");

                    break;
                case 0x0002:
                    sb.AppendLine("ATA-1 published, ANSI X3.221-1994");

                    break;
                case 0x0003:
                    sb.AppendLine("ATA (ATA-1) X3T9.2 781D revision 4");

                    break;
                case 0x0004:
                    sb.AppendLine("ATA-2 published, ANSI X3.279-1996");

                    break;
                case 0x0005:
                    sb.AppendLine("ATA-2 X3T10 948D prior to revision 2k");

                    break;
                case 0x0006:
                    sb.AppendLine("ATA-3 X3T10 2008D revision 1");

                    break;
                case 0x0007:
                    sb.AppendLine("ATA-2 X3T10 948D revision 2k");

                    break;
                case 0x0008:
                    sb.AppendLine("ATA-3 X3T10 2008D revision 0");

                    break;
                case 0x0009:
                    sb.AppendLine("ATA-2 X3T10 948D revision 3");

                    break;
                case 0x000A:
                    sb.AppendLine("ATA-3 published, ANSI X3.298-1997");

                    break;
                case 0x000B:
                    sb.AppendLine("ATA-3 X3T10 2008D revision 6");

                    break;
                case 0x000C:
                    sb.AppendLine("ATA-3 X3T13 2008D revision 7");

                    break;
                case 0x000D:
                    sb.AppendLine("ATA/ATAPI-4 X3T13 1153D revision 6");

                    break;
                case 0x000E:
                    sb.AppendLine("ATA/ATAPI-4 T13 1153D revision 13");

                    break;
                case 0x000F:
                    sb.AppendLine("ATA/ATAPI-4 X3T13 1153D revision 7");

                    break;
                case 0x0010:
                    sb.AppendLine("ATA/ATAPI-4 T13 1153D revision 18");

                    break;
                case 0x0011:
                    sb.AppendLine("ATA/ATAPI-4 T13 1153D revision 15");

                    break;
                case 0x0012:
                    sb.AppendLine("ATA/ATAPI-4 published, ANSI INCITS 317-1998");

                    break;
                case 0x0013:
                    sb.AppendLine("ATA/ATAPI-5 T13 1321D revision 3");

                    break;
                case 0x0014:
                    sb.AppendLine("ATA/ATAPI-4 T13 1153D revision 14");

                    break;
                case 0x0015:
                    sb.AppendLine("ATA/ATAPI-5 T13 1321D revision 1");

                    break;
                case 0x0016:
                    sb.AppendLine("ATA/ATAPI-5 published, ANSI INCITS 340-2000");

                    break;
                case 0x0017:
                    sb.AppendLine("ATA/ATAPI-4 T13 1153D revision 17");

                    break;
                case 0x0018:
                    sb.AppendLine("ATA/ATAPI-6 T13 1410D revision 0");

                    break;
                case 0x0019:
                    sb.AppendLine("ATA/ATAPI-6 T13 1410D revision 3a");

                    break;
                case 0x001A:
                    sb.AppendLine("ATA/ATAPI-7 T13 1532D revision 1");

                    break;
                case 0x001B:
                    sb.AppendLine("ATA/ATAPI-6 T13 1410D revision 2");

                    break;
                case 0x001C:
                    sb.AppendLine("ATA/ATAPI-6 T13 1410D revision 1");

                    break;
                case 0x001D:
                    sb.AppendLine("ATA/ATAPI-7 published ANSI INCITS 397-2005");

                    break;
                case 0x001E:
                    sb.AppendLine("ATA/ATAPI-7 T13 1532D revision 0");

                    break;
                case 0x001F:
                    sb.AppendLine("ACS-3 Revision 3b");

                    break;
                case 0x0021:
                    sb.AppendLine("ATA/ATAPI-7 T13 1532D revision 4a");

                    break;
                case 0x0022:
                    sb.AppendLine("ATA/ATAPI-6 published, ANSI INCITS 361-2002");

                    break;
                case 0x0027:
                    sb.AppendLine("ATA8-ACS revision 3c");

                    break;
                case 0x0028:
                    sb.AppendLine("ATA8-ACS revision 6");

                    break;
                case 0x0029:
                    sb.AppendLine("ATA8-ACS revision 4");

                    break;
                case 0x0031:
                    sb.AppendLine("ACS-2 Revision 2");

                    break;
                case 0x0033:
                    sb.AppendLine("ATA8-ACS Revision 3e");

                    break;
                case 0x0039:
                    sb.AppendLine("ATA8-ACS Revision 4c");

                    break;
                case 0x0042:
                    sb.AppendLine("ATA8-ACS Revision 3f");

                    break;
                case 0x0052:
                    sb.AppendLine("ATA8-ACS revision 3b");

                    break;
                case 0x006D:
                    sb.AppendLine("ACS-3 Revision 5");

                    break;
                case 0x0082:
                    sb.AppendLine("ACS-2 published, ANSI INCITS 482-2012");

                    break;
                case 0x0107:
                    sb.AppendLine("ATA8-ACS revision 2d");

                    break;
                case 0x0110:
                    sb.AppendLine("ACS-2 Revision 3");

                    break;
                case 0x011B:
                    sb.AppendLine("ACS-3 Revision 4");

                    break;
                default:
                    sb.AppendFormat("Unknown ATA revision 0x{0:X4}", ATAID.MinorVersion).AppendLine();

                    break;
            }

        switch((ATAID.TransportMajorVersion & 0xF000) >> 12)
        {
            case 0x0:
                sb.Append("Parallel ATA device: ");

                if((ATAID.TransportMajorVersion & 0x0002) == 0x0002)
                    sb.Append("ATA/ATAPI-7 ");

                if((ATAID.TransportMajorVersion & 0x0001) == 0x0001)
                    sb.Append("ATA8-APT ");

                sb.AppendLine();

                break;
            case 0x1:
                sb.Append("Serial ATA device: ");

                if((ATAID.TransportMajorVersion & 0x0001) == 0x0001)
                    sb.Append("ATA8-AST ");

                if((ATAID.TransportMajorVersion & 0x0002) == 0x0002)
                    sb.Append("SATA 1.0a ");

                if((ATAID.TransportMajorVersion & 0x0004) == 0x0004)
                    sb.Append("SATA II Extensions ");

                if((ATAID.TransportMajorVersion & 0x0008) == 0x0008)
                    sb.Append("SATA 2.5 ");

                if((ATAID.TransportMajorVersion & 0x0010) == 0x0010)
                    sb.Append("SATA 2.6 ");

                if((ATAID.TransportMajorVersion & 0x0020) == 0x0020)
                    sb.Append("SATA 3.0 ");

                if((ATAID.TransportMajorVersion & 0x0040) == 0x0040)
                    sb.Append("SATA 3.1 ");

                sb.AppendLine();

                break;
            case 0xE:
                sb.AppendLine("SATA Express device");

                break;
            default:
                sb.AppendFormat("Unknown transport type 0x{0:X1}", (ATAID.TransportMajorVersion & 0xF000) >> 12).
                   AppendLine();

                break;
        }

        if(atapi)
        {
            // Bits 12 to 8, SCSI Peripheral Device Type
            switch((PeripheralDeviceTypes)(((ushort)ATAID.GeneralConfiguration & 0x1F00) >> 8))
            {
                case PeripheralDeviceTypes.DirectAccess: //0x00,
                    sb.AppendLine("ATAPI Direct-access device");

                    break;
                case PeripheralDeviceTypes.SequentialAccess: //0x01,
                    sb.AppendLine("ATAPI Sequential-access device");

                    break;
                case PeripheralDeviceTypes.PrinterDevice: //0x02,
                    sb.AppendLine("ATAPI Printer device");

                    break;
                case PeripheralDeviceTypes.ProcessorDevice: //0x03,
                    sb.AppendLine("ATAPI Processor device");

                    break;
                case PeripheralDeviceTypes.WriteOnceDevice: //0x04,
                    sb.AppendLine("ATAPI Write-once device");

                    break;
                case PeripheralDeviceTypes.MultiMediaDevice: //0x05,
                    sb.AppendLine("ATAPI CD-ROM/DVD/etc device");

                    break;
                case PeripheralDeviceTypes.ScannerDevice: //0x06,
                    sb.AppendLine("ATAPI Scanner device");

                    break;
                case PeripheralDeviceTypes.OpticalDevice: //0x07,
                    sb.AppendLine("ATAPI Optical memory device");

                    break;
                case PeripheralDeviceTypes.MediumChangerDevice: //0x08,
                    sb.AppendLine("ATAPI Medium change device");

                    break;
                case PeripheralDeviceTypes.CommsDevice: //0x09,
                    sb.AppendLine("ATAPI Communications device");

                    break;
                case PeripheralDeviceTypes.PrePressDevice1: //0x0A,
                    sb.AppendLine("ATAPI Graphics arts pre-press device (defined in ASC IT8)");

                    break;
                case PeripheralDeviceTypes.PrePressDevice2: //0x0B,
                    sb.AppendLine("ATAPI Graphics arts pre-press device (defined in ASC IT8)");

                    break;
                case PeripheralDeviceTypes.ArrayControllerDevice: //0x0C,
                    sb.AppendLine("ATAPI Array controller device");

                    break;
                case PeripheralDeviceTypes.EnclosureServiceDevice: //0x0D,
                    sb.AppendLine("ATAPI Enclosure services device");

                    break;
                case PeripheralDeviceTypes.SimplifiedDevice: //0x0E,
                    sb.AppendLine("ATAPI Simplified direct-access device");

                    break;
                case PeripheralDeviceTypes.OCRWDevice: //0x0F,
                    sb.AppendLine("ATAPI Optical card reader/writer device");

                    break;
                case PeripheralDeviceTypes.BridgingExpander: //0x10,
                    sb.AppendLine("ATAPI Bridging Expanders");

                    break;
                case PeripheralDeviceTypes.ObjectDevice: //0x11,
                    sb.AppendLine("ATAPI Object-based Storage Device");

                    break;
                case PeripheralDeviceTypes.ADCDevice: //0x12,
                    sb.AppendLine("ATAPI Automation/Drive Interface");

                    break;
                case PeripheralDeviceTypes.WellKnownDevice: //0x1E,
                    sb.AppendLine("ATAPI Well known logical unit");

                    break;
                case PeripheralDeviceTypes.UnknownDevice: //0x1F
                    sb.AppendLine("ATAPI Unknown or no device type");

                    break;
                default:
                    sb.AppendFormat("ATAPI Unknown device type field value 0x{0:X2}",
                                    ((ushort)ATAID.GeneralConfiguration & 0x1F00) >> 8).AppendLine();

                    break;
            }

            // ATAPI DRQ behaviour
            switch(((ushort)ATAID.GeneralConfiguration & 0x60) >> 5)
            {
                case 0:
                    sb.AppendLine("Device shall set DRQ within 3 ms of receiving PACKET");

                    break;
                case 1:
                    sb.AppendLine("Device shall assert INTRQ when DRQ is set to one");

                    break;
                case 2:
                    sb.AppendLine("Device shall set DRQ within 50 µs of receiving PACKET");

                    break;
                default:
                    sb.AppendFormat("Unknown ATAPI DRQ behaviour code {0}",
                                    ((ushort)ATAID.GeneralConfiguration & 0x60) >> 5).AppendLine();

                    break;
            }

            // ATAPI PACKET size
            switch((ushort)ATAID.GeneralConfiguration & 0x03)
            {
                case 0:
                    sb.AppendLine("ATAPI device uses 12 byte command packet");

                    break;
                case 1:
                    sb.AppendLine("ATAPI device uses 16 byte command packet");

                    break;
                default:
                    sb.AppendFormat("Unknown ATAPI packet size code {0}",
                                    (ushort)ATAID.GeneralConfiguration & 0x03).AppendLine();

                    break;
            }
        }
        else if(!cfa)
        {
            if(minatalevel >= 5)
                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.IncompleteResponse))
                    sb.AppendLine("Incomplete identify response");

            if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.
                                                              NonMagnetic))
                sb.AppendLine("Device uses non-magnetic media");

            if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.GeneralConfigurationBit.
                                                              Removable))
                sb.AppendLine("Device is removable");

            if(minatalevel <= 5)
                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.Fixed))
                    sb.AppendLine("Device is fixed");

            if(ata1)
            {
                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.SlowIDE))
                    sb.AppendLine("Device transfer rate is <= 5 Mb/s");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.FastIDE))
                    sb.AppendLine("Device transfer rate is > 5 Mb/s but <= 10 Mb/s");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.UltraFastIDE))
                    sb.AppendLine("Device transfer rate is > 10 Mb/s");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.SoftSector))
                    sb.AppendLine("Device is soft sectored");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.HardSector))
                    sb.AppendLine("Device is hard sectored");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.NotMFM))
                    sb.AppendLine("Device is not MFM encoded");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.FormatGapReq))
                    sb.AppendLine("Format speed tolerance gap is required");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.TrackOffset))
                    sb.AppendLine("Track offset option is available");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.DataStrobeOffset))
                    sb.AppendLine("Data strobe offset option is available");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.RotationalSpeedTolerance))
                    sb.AppendLine("Rotational speed tolerance is higher than 0,5%");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.SpindleControl))
                    sb.AppendLine("Spindle motor control is implemented");

                if(ATAID.GeneralConfiguration.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                  GeneralConfigurationBit.HighHeadSwitch))
                    sb.AppendLine("Head switch time is bigger than 15 µs.");
            }
        }

        if(ATAID.NominalRotationRate != 0x0000 &&
           ATAID.NominalRotationRate != 0xFFFF)
            if(ATAID.NominalRotationRate == 0x0001)
                sb.AppendLine("Device does not rotate.");
            else
                sb.AppendFormat("Device rotate at {0} rpm", ATAID.NominalRotationRate).AppendLine();

        uint logicalSectorSize = 0;

        if(!atapi)
        {
            uint physicalSectorSize;

            if((ATAID.PhysLogSectorSize & 0x8000) == 0x0000 &&
               (ATAID.PhysLogSectorSize & 0x4000) == 0x4000)
            {
                if((ATAID.PhysLogSectorSize & 0x1000) == 0x1000)
                    if(ATAID.LogicalSectorWords <= 255 ||
                       ATAID.LogicalAlignment   == 0xFFFF)
                        logicalSectorSize = 512;
                    else
                        logicalSectorSize = ATAID.LogicalSectorWords * 2;
                else
                    logicalSectorSize = 512;

                if((ATAID.PhysLogSectorSize & 0x2000) == 0x2000)
                    physicalSectorSize = logicalSectorSize * (uint)Math.Pow(2, ATAID.PhysLogSectorSize & 0xF);
                else
                    physicalSectorSize = logicalSectorSize;
            }
            else
            {
                logicalSectorSize  = 512;
                physicalSectorSize = 512;
            }

            sb.AppendFormat("Physical sector size: {0} bytes", physicalSectorSize).AppendLine();
            sb.AppendFormat("Logical sector size: {0} bytes", logicalSectorSize).AppendLine();

            if(logicalSectorSize                 != physicalSectorSize &&
               (ATAID.LogicalAlignment & 0x8000) == 0x0000             &&
               (ATAID.LogicalAlignment & 0x4000) == 0x4000)
                sb.AppendFormat("Logical sector starts at offset {0} from physical sector",
                                ATAID.LogicalAlignment & 0x3FFF).AppendLine();

            if(minatalevel <= 5)
                if(ATAID.CurrentCylinders       > 0 &&
                   ATAID.CurrentHeads           > 0 &&
                   ATAID.CurrentSectorsPerTrack > 0)
                {
                    sb.AppendFormat("Cylinders: {0} max., {1} current", ATAID.Cylinders, ATAID.CurrentCylinders).
                       AppendLine();

                    sb.AppendFormat("Heads: {0} max., {1} current", ATAID.Heads, ATAID.CurrentHeads).AppendLine();

                    sb.AppendFormat("Sectors per track: {0} max., {1} current", ATAID.SectorsPerTrack,
                                    ATAID.CurrentSectorsPerTrack).AppendLine();

                    sb.AppendFormat("Sectors addressable in CHS mode: {0} max., {1} current",
                                    ATAID.Cylinders * ATAID.Heads * ATAID.SectorsPerTrack, ATAID.CurrentSectors).
                       AppendLine();
                }
                else
                {
                    sb.AppendFormat("Cylinders: {0}", ATAID.Cylinders).AppendLine();
                    sb.AppendFormat("Heads: {0}", ATAID.Heads).AppendLine();
                    sb.AppendFormat("Sectors per track: {0}", ATAID.SectorsPerTrack).AppendLine();

                    sb.AppendFormat("Sectors addressable in CHS mode: {0}",
                                    ATAID.Cylinders * ATAID.Heads * ATAID.SectorsPerTrack).AppendLine();
                }

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.LBASupport))
                sb.AppendFormat("{0} sectors in 28-bit LBA mode", ATAID.LBASectors).AppendLine();

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.LBA48))
                sb.AppendFormat("{0} sectors in 48-bit LBA mode", ATAID.LBA48Sectors).AppendLine();

            if(minatalevel <= 5)
                if(ATAID.CurrentSectors > 0)
                    sb.AppendFormat("Device size in CHS mode: {0} bytes, {1} Mb, {2} MiB",
                                    (ulong)ATAID.CurrentSectors                     * logicalSectorSize,
                                    (ulong)ATAID.CurrentSectors * logicalSectorSize / 1000 / 1000,
                                    (ulong)ATAID.CurrentSectors * 512               / 1024 / 1024).AppendLine();
                else
                {
                    ulong currentSectors = (ulong)(ATAID.Cylinders * ATAID.Heads * ATAID.SectorsPerTrack);

                    sb.AppendFormat("Device size in CHS mode: {0} bytes, {1} Mb, {2} MiB",
                                    currentSectors                     * logicalSectorSize,
                                    currentSectors * logicalSectorSize / 1000 / 1000,
                                    currentSectors * 512               / 1024 / 1024).AppendLine();
                }

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.LBASupport))
                if((ulong)ATAID.LBASectors * logicalSectorSize / 1024 / 1024 > 1000000)
                    sb.AppendFormat("Device size in 28-bit LBA mode: {0} bytes, {1} Tb, {2} TiB",
                                    (ulong)ATAID.LBASectors * logicalSectorSize,
                                    (ulong)ATAID.LBASectors * logicalSectorSize / 1000 / 1000 / 1000 / 1000,
                                    (ulong)ATAID.LBASectors * 512 / 1024 / 1024 / 1024 / 1024).AppendLine();
                else if((ulong)ATAID.LBASectors * logicalSectorSize / 1024 / 1024 > 1000)
                    sb.AppendFormat("Device size in 28-bit LBA mode: {0} bytes, {1} Gb, {2} GiB",
                                    (ulong)ATAID.LBASectors                     * logicalSectorSize,
                                    (ulong)ATAID.LBASectors * logicalSectorSize / 1000 / 1000 / 1000,
                                    (ulong)ATAID.LBASectors * 512               / 1024 / 1024 / 1024).AppendLine();
                else
                    sb.AppendFormat("Device size in 28-bit LBA mode: {0} bytes, {1} Mb, {2} MiB",
                                    (ulong)ATAID.LBASectors                     * logicalSectorSize,
                                    (ulong)ATAID.LBASectors * logicalSectorSize / 1000 / 1000,
                                    (ulong)ATAID.LBASectors * 512               / 1024 / 1024).AppendLine();

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.LBA48))
                if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.ExtSectors))
                    if(ATAID.ExtendedUserSectors * logicalSectorSize / 1024 / 1024 > 1000000)
                        sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Tb, {2} TiB",
                                        ATAID.ExtendedUserSectors                     * logicalSectorSize,
                                        ATAID.ExtendedUserSectors * logicalSectorSize / 1000 / 1000 / 1000 / 1000,
                                        ATAID.ExtendedUserSectors * logicalSectorSize / 1024 / 1024 / 1024 / 1024).
                           AppendLine();
                    else if(ATAID.ExtendedUserSectors * logicalSectorSize / 1024 / 1024 > 1000)
                        sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Gb, {2} GiB",
                                        ATAID.ExtendedUserSectors                     * logicalSectorSize,
                                        ATAID.ExtendedUserSectors * logicalSectorSize / 1000 / 1000 / 1000,
                                        ATAID.ExtendedUserSectors * logicalSectorSize / 1024 / 1024 / 1024).
                           AppendLine();
                    else
                        sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Mb, {2} MiB",
                                        ATAID.ExtendedUserSectors                     * logicalSectorSize,
                                        ATAID.ExtendedUserSectors * logicalSectorSize / 1000 / 1000,
                                        ATAID.ExtendedUserSectors * logicalSectorSize / 1024 / 1024).AppendLine();
                else
                {
                    if(ATAID.LBA48Sectors * logicalSectorSize / 1024 / 1024 > 1000000)
                        sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Tb, {2} TiB",
                                        ATAID.LBA48Sectors                     * logicalSectorSize,
                                        ATAID.LBA48Sectors * logicalSectorSize / 1000 / 1000 / 1000 / 1000,
                                        ATAID.LBA48Sectors * logicalSectorSize / 1024 / 1024 / 1024 / 1024).
                           AppendLine();
                    else if(ATAID.LBA48Sectors * logicalSectorSize / 1024 / 1024 > 1000)
                        sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Gb, {2} GiB",
                                        ATAID.LBA48Sectors                     * logicalSectorSize,
                                        ATAID.LBA48Sectors * logicalSectorSize / 1000 / 1000 / 1000,
                                        ATAID.LBA48Sectors * logicalSectorSize / 1024 / 1024 / 1024).AppendLine();
                    else
                        sb.AppendFormat("Device size in 48-bit LBA mode: {0} bytes, {1} Mb, {2} MiB",
                                        ATAID.LBA48Sectors                     * logicalSectorSize,
                                        ATAID.LBA48Sectors * logicalSectorSize / 1000 / 1000,
                                        ATAID.LBA48Sectors * logicalSectorSize / 1024 / 1024).AppendLine();
                }

            if(ata1 || cfa)
            {
                if(cfa)
                    sb.AppendFormat("{0} sectors in card", ATAID.SectorsPerCard).AppendLine();

                if(ATAID.UnformattedBPT > 0)
                    sb.AppendFormat("{0} bytes per unformatted track", ATAID.UnformattedBPT).AppendLine();

                if(ATAID.UnformattedBPS > 0)
                    sb.AppendFormat("{0} bytes per unformatted sector", ATAID.UnformattedBPS).AppendLine();
            }
        }

        if((ushort)ATAID.SpecificConfiguration != 0x0000 &&
           (ushort)ATAID.SpecificConfiguration != 0xFFFF)
            switch(ATAID.SpecificConfiguration)
            {
                case CommonTypes.Structs.Devices.ATA.Identify.SpecificConfigurationEnum.
                                 RequiresSetIncompleteResponse:
                    sb.AppendLine("Device requires SET FEATURES to spin up and IDENTIFY DEVICE response is incomplete.");

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.SpecificConfigurationEnum.RequiresSetCompleteResponse:
                    sb.AppendLine("Device requires SET FEATURES to spin up and IDENTIFY DEVICE response is complete.");

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.SpecificConfigurationEnum.
                                 NotRequiresSetIncompleteResponse:
                    sb.AppendLine("Device does not require SET FEATURES to spin up and IDENTIFY DEVICE response is incomplete.");

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.SpecificConfigurationEnum.
                                 NotRequiresSetCompleteResponse:
                    sb.AppendLine("Device does not require SET FEATURES to spin up and IDENTIFY DEVICE response is complete.");

                    break;
                default:
                    sb.AppendFormat("Unknown device specific configuration 0x{0:X4}",
                                    (ushort)ATAID.SpecificConfiguration).AppendLine();

                    break;
            }

        // Obsolete since ATA-2, however, it is yet used in ATA-8 devices
        if(ATAID.BufferSize != 0x0000 &&
           ATAID.BufferSize != 0xFFFF &&
           ATAID.BufferType != 0x0000 &&
           ATAID.BufferType != 0xFFFF)
            switch(ATAID.BufferType)
            {
                case 1:
                    sb.AppendFormat("{0} KiB of single ported single sector buffer", ATAID.BufferSize * 512 / 1024).
                       AppendLine();

                    break;
                case 2:
                    sb.AppendFormat("{0} KiB of dual ported multi sector buffer", ATAID.BufferSize * 512 / 1024).
                       AppendLine();

                    break;
                case 3:
                    sb.AppendFormat("{0} KiB of dual ported multi sector buffer with read caching",
                                    ATAID.BufferSize * 512 / 1024).AppendLine();

                    break;
                default:
                    sb.AppendFormat("{0} KiB of unknown type {1} buffer", ATAID.BufferSize * 512 / 1024,
                                    ATAID.BufferType).AppendLine();

                    break;
            }

        if(ATAID.EccBytes != 0x0000 &&
           ATAID.EccBytes != 0xFFFF)
            sb.AppendFormat("READ/WRITE LONG has {0} extra bytes", ATAID.EccBytes).AppendLine();

        sb.AppendLine();

        sb.Append("Device capabilities:");

        if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.
                                                  StandardStandbyTimer))
            sb.AppendLine().Append("Standby time values are standard");

        if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.IORDY))
        {
            sb.AppendLine().Append("IORDY is supported");

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.CanDisableIORDY))
                sb.Append(" and can be disabled");
        }

        if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.DMASupport))
            sb.AppendLine().Append("DMA is supported");

        if(ATAID.Capabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit2.MustBeSet) &&
           !ATAID.Capabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit2.MustBeClear))
            if(ATAID.Capabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit2.
                                                       SpecificStandbyTimer))
                sb.AppendLine().Append("Device indicates a specific minimum standby timer value");

        if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.MultipleValid))
        {
            sb.AppendLine().
               AppendFormat("A maximum of {0} sectors can be transferred per interrupt on READ/WRITE MULTIPLE",
                            ATAID.MultipleSectorNumber);

            sb.AppendLine().AppendFormat("Device supports setting a maximum of {0} sectors",
                                         ATAID.MultipleMaxSectors);
        }

        if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.
                                                  PhysicalAlignment1) ||
           ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.PhysicalAlignment0))
            sb.AppendLine().AppendFormat("Long Physical Alignment setting is {0}",
                                         (ushort)ATAID.Capabilities & 0x03);

        if(ata1)
            if(ATAID.TrustedComputing.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TrustedComputingBit.
                                                          TrustedComputing))
                sb.AppendLine().Append("Device supports doubleword I/O");

        if(atapi)
        {
            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.InterleavedDMA))
                sb.AppendLine().Append("ATAPI device supports interleaved DMA");

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.CommandQueue))
                sb.AppendLine().Append("ATAPI device supports command queueing");

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.
                                                      OverlapOperation))
                sb.AppendLine().Append("ATAPI device supports overlapped operations");

            if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.
                                                      RequiresATASoftReset))
                sb.AppendLine().Append("ATAPI device requires ATA software reset");
        }

        if(minatalevel <= 3)
        {
            sb.AppendLine().AppendFormat("PIO timing mode: {0}", ATAID.PIOTransferTimingMode);
            sb.AppendLine().AppendFormat("DMA timing mode: {0}", ATAID.DMATransferTimingMode);
        }

        sb.AppendLine().Append("Advanced PIO: ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
            sb.Append("PIO0 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
            sb.Append("PIO1 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
            sb.Append("PIO2 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
            sb.Append("PIO3 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
            sb.Append("PIO4 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
            sb.Append("PIO5 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
            sb.Append("PIO6 ");

        if(ATAID.APIOSupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
            sb.Append("PIO7 ");

        if(minatalevel <= 3 &&
           !atapi)
        {
            sb.AppendLine().Append("Single-word DMA: ");

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
            {
                sb.Append("DMA0 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
                    sb.Append("(active) ");
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
            {
                sb.Append("DMA1 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
                    sb.Append("(active) ");
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
            {
                sb.Append("DMA2 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
                    sb.Append("(active) ");
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
            {
                sb.Append("DMA3 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
                    sb.Append("(active) ");
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
            {
                sb.Append("DMA4 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
                    sb.Append("(active) ");
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
            {
                sb.Append("DMA5 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
                    sb.Append("(active) ");
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
            {
                sb.Append("DMA6 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
                    sb.Append("(active) ");
            }

            if(ATAID.DMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
            {
                sb.Append("DMA7 ");

                if(ATAID.DMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
                    sb.Append("(active) ");
            }
        }

        sb.AppendLine().Append("Multi-word DMA: ");

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
        {
            sb.Append("MDMA0 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
                sb.Append("(active) ");
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
        {
            sb.Append("MDMA1 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
                sb.Append("(active) ");
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
        {
            sb.Append("MDMA2 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
                sb.Append("(active) ");
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
        {
            sb.Append("MDMA3 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
                sb.Append("(active) ");
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
        {
            sb.Append("MDMA4 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
                sb.Append("(active) ");
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
        {
            sb.Append("MDMA5 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
                sb.Append("(active) ");
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
        {
            sb.Append("MDMA6 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
                sb.Append("(active) ");
        }

        if(ATAID.MDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
        {
            sb.Append("MDMA7 ");

            if(ATAID.MDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
                sb.Append("(active) ");
        }

        sb.AppendLine().Append("Ultra DMA: ");

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
        {
            sb.Append("UDMA0 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode0))
                sb.Append("(active) ");
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
        {
            sb.Append("UDMA1 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode1))
                sb.Append("(active) ");
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
        {
            sb.Append("UDMA2 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode2))
                sb.Append("(active) ");
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
        {
            sb.Append("UDMA3 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode3))
                sb.Append("(active) ");
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
        {
            sb.Append("UDMA4 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode4))
                sb.Append("(active) ");
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
        {
            sb.Append("UDMA5 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode5))
                sb.Append("(active) ");
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
        {
            sb.Append("UDMA6 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode6))
                sb.Append("(active) ");
        }

        if(ATAID.UDMASupported.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
        {
            sb.Append("UDMA7 ");

            if(ATAID.UDMAActive.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TransferMode.Mode7))
                sb.Append("(active) ");
        }

        if(ATAID.MinMDMACycleTime != 0 &&
           ATAID.RecMDMACycleTime != 0)
            sb.AppendLine().
               AppendFormat("At minimum {0} ns. transfer cycle time per word in MDMA, " + "{1} ns. recommended",
                            ATAID.MinMDMACycleTime, ATAID.RecMDMACycleTime);

        if(ATAID.MinPIOCycleTimeNoFlow != 0)
            sb.AppendLine().
               AppendFormat("At minimum {0} ns. transfer cycle time per word in PIO, " + "without flow control",
                            ATAID.MinPIOCycleTimeNoFlow);

        if(ATAID.MinPIOCycleTimeFlow != 0)
            sb.AppendLine().
               AppendFormat("At minimum {0} ns. transfer cycle time per word in PIO, " + "with IORDY flow control",
                            ATAID.MinPIOCycleTimeFlow);

        if(ATAID.MaxQueueDepth != 0)
            sb.AppendLine().AppendFormat("{0} depth of queue maximum", ATAID.MaxQueueDepth + 1);

        if(atapi)
        {
            if(ATAID.PacketBusRelease != 0)
                sb.AppendLine().AppendFormat("{0} ns. typical to release bus from receipt of PACKET",
                                             ATAID.PacketBusRelease);

            if(ATAID.ServiceBusyClear != 0)
                sb.AppendLine().AppendFormat("{0} ns. typical to clear BSY bit from receipt of SERVICE",
                                             ATAID.ServiceBusyClear);
        }

        if((ATAID.TransportMajorVersion & 0xF000) >> 12 == 0x1 ||
           (ATAID.TransportMajorVersion & 0xF000) >> 12 == 0xE)
        {
            if(!ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.Clear))
            {
                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                              Gen1Speed))
                    sb.AppendLine().Append("SATA 1.5Gb/s is supported");

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                              Gen2Speed))
                    sb.AppendLine().Append("SATA 3.0Gb/s is supported");

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                              Gen3Speed))
                    sb.AppendLine().Append("SATA 6.0Gb/s is supported");

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                              PowerReceipt))
                    sb.AppendLine().Append("Receipt of host initiated power management requests is supported");

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                              PHYEventCounter))
                    sb.AppendLine().Append("PHY Event counters are supported");

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                              HostSlumbTrans))
                    sb.AppendLine().Append("Supports host automatic partial to slumber transitions is supported");

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                              DevSlumbTrans))
                    sb.AppendLine().Append("Supports device automatic partial to slumber transitions is supported");

                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.NCQ))
                {
                    sb.AppendLine().Append("NCQ is supported");

                    if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                                  NCQPriority))
                        sb.AppendLine().Append("NCQ priority is supported");

                    if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                                  UnloadNCQ))
                        sb.AppendLine().Append("Unload is supported with outstanding NCQ commands");
                }
            }

            if(!ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit2.
                                                            Clear))
            {
                if(!ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                               Clear) &&
                   ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.NCQ))
                {
                    if(ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                   SATACapabilitiesBit2.NCQMgmt))
                        sb.AppendLine().Append("NCQ queue management is supported");

                    if(ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                   SATACapabilitiesBit2.NCQStream))
                        sb.AppendLine().Append("NCQ streaming is supported");
                }

                if(atapi)
                {
                    if(ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                   SATACapabilitiesBit2.HostEnvDetect))
                        sb.AppendLine().Append("ATAPI device supports host environment detection");

                    if(ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                   SATACapabilitiesBit2.DevAttSlimline))
                        sb.AppendLine().Append("ATAPI device supports attention on slimline connected devices");
                }

                //sb.AppendFormat("Negotiated speed = {0}", ((ushort)ATAID.SATACapabilities2 & 0x000E) >> 1);
            }
        }

        if(ATAID.InterseekDelay != 0x0000 &&
           ATAID.InterseekDelay != 0xFFFF)
            sb.AppendLine().AppendFormat("{0} microseconds of interseek delay for ISO-7779 acoustic testing",
                                         ATAID.InterseekDelay);

        if((ushort)ATAID.DeviceFormFactor != 0x0000 &&
           (ushort)ATAID.DeviceFormFactor != 0xFFFF)
            switch(ATAID.DeviceFormFactor)
            {
                case CommonTypes.Structs.Devices.ATA.Identify.DeviceFormFactorEnum.FiveAndQuarter:
                    sb.AppendLine().Append("Device nominal size is 5.25\"");

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.DeviceFormFactorEnum.ThreeAndHalf:
                    sb.AppendLine().Append("Device nominal size is 3.5\"");

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.DeviceFormFactorEnum.TwoAndHalf:
                    sb.AppendLine().Append("Device nominal size is 2.5\"");

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.DeviceFormFactorEnum.OnePointEight:
                    sb.AppendLine().Append("Device nominal size is 1.8\"");

                    break;
                case CommonTypes.Structs.Devices.ATA.Identify.DeviceFormFactorEnum.LessThanOnePointEight:
                    sb.AppendLine().Append("Device nominal size is smaller than 1.8\"");

                    break;
                default:
                    sb.AppendLine().AppendFormat("Device nominal size field value {0} is unknown",
                                                 ATAID.DeviceFormFactor);

                    break;
            }

        if(atapi)
            if(ATAID.ATAPIByteCount > 0)
                sb.AppendLine().AppendFormat("{0} bytes count limit for ATAPI", ATAID.ATAPIByteCount);

        if(cfa)
            if((ATAID.CFAPowerMode & 0x8000) == 0x8000)
            {
                sb.AppendLine().Append("CompactFlash device supports power mode 1");

                if((ATAID.CFAPowerMode & 0x2000) == 0x2000)
                    sb.AppendLine().Append("CompactFlash power mode 1 required for one or more commands");

                if((ATAID.CFAPowerMode & 0x1000) == 0x1000)
                    sb.AppendLine().Append("CompactFlash power mode 1 is disabled");

                sb.AppendLine().AppendFormat("CompactFlash device uses a maximum of {0} mA",
                                             ATAID.CFAPowerMode & 0x0FFF);
            }

        sb.AppendLine();

        sb.AppendLine().Append("Command set and features:");

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Nop))
        {
            sb.AppendLine().Append("NOP is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Nop))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.ReadBuffer))
        {
            sb.AppendLine().Append("READ BUFFER is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.ReadBuffer))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.WriteBuffer))
        {
            sb.AppendLine().Append("WRITE BUFFER is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.WriteBuffer))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.HPA))
        {
            sb.AppendLine().Append("Host Protected Area is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.HPA))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.DeviceReset))
        {
            sb.AppendLine().Append("DEVICE RESET is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.DeviceReset))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Service))
        {
            sb.AppendLine().Append("SERVICE interrupt is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Service))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Release))
        {
            sb.AppendLine().Append("Release is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Release))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.LookAhead))
        {
            sb.AppendLine().Append("Look-ahead read is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.LookAhead))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.WriteCache))
        {
            sb.AppendLine().Append("Write cache is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.WriteCache))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Packet))
        {
            sb.AppendLine().Append("PACKET is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.Packet))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.PowerManagement))
        {
            sb.AppendLine().Append("Power management is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.
                                                           PowerManagement))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.RemovableMedia))
        {
            sb.AppendLine().Append("Removable media feature set is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.
                                                           RemovableMedia))
                sb.Append(" and enabled");
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.SecurityMode))
        {
            sb.AppendLine().Append("Security mode is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.SecurityMode))
                sb.Append(" and enabled");
        }

        if(ATAID.Capabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit.LBASupport))
            sb.AppendLine().Append("28-bit LBA is supported");

        if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.MustBeSet) &&
           !ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.MustBeClear))
        {
            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.LBA48))
            {
                sb.AppendLine().Append("48-bit LBA is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.LBA48))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.FlushCache))
            {
                sb.AppendLine().Append("FLUSH CACHE is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                                FlushCache))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.FlushCacheExt))
            {
                sb.AppendLine().Append("FLUSH CACHE EXT is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                                FlushCacheExt))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.DCO))
            {
                sb.AppendLine().Append("Device Configuration Overlay feature set is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.DCO))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.AAM))
            {
                sb.AppendLine().Append("Automatic Acoustic Management is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.AAM))
                    sb.AppendFormat(" and enabled with value {0} (vendor recommends {1}", ATAID.CurrentAAM,
                                    ATAID.RecommendedAAM);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.SetMax))
            {
                sb.AppendLine().Append("SET MAX security extension is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.SetMax))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                     AddressOffsetReservedAreaBoot))
            {
                sb.AppendLine().Append("Address Offset Reserved Area Boot is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                                AddressOffsetReservedAreaBoot))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                     SetFeaturesRequired))
                sb.AppendLine().Append("SET FEATURES is required before spin-up");

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.PowerUpInStandby))
            {
                sb.AppendLine().Append("Power-up in standby is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                                PowerUpInStandby))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                     RemovableNotification))
            {
                sb.AppendLine().Append("Removable Media Status Notification is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                                RemovableNotification))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.APM))
            {
                sb.AppendLine().Append("Advanced Power Management is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.APM))
                    sb.AppendFormat(" and enabled with value {0}", ATAID.CurrentAPM);
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.CompactFlash))
            {
                sb.AppendLine().Append("CompactFlash feature set is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                                CompactFlash))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.RWQueuedDMA))
            {
                sb.AppendLine().Append("READ DMA QUEUED and WRITE DMA QUEUED are supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                                RWQueuedDMA))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.DownloadMicrocode))
            {
                sb.AppendLine().Append("DOWNLOAD MICROCODE is supported");

                if(ATAID.EnabledCommandSet2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit2.
                                                                DownloadMicrocode))
                    sb.Append(" and enabled");
            }
        }

        if(ATAID.CommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.SMART))
        {
            sb.AppendLine().Append("S.M.A.R.T. is supported");

            if(ATAID.EnabledCommandSet.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit.SMART))
                sb.Append(" and enabled");
        }

        if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit.
                                                         Supported))
            sb.AppendLine().Append("S.M.A.R.T. Command Transport is supported");

        if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeSet) &&
           !ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeClear))
        {
            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.SMARTSelfTest))
            {
                sb.AppendLine().Append("S.M.A.R.T. self-testing is supported");

                if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.
                                                                SMARTSelfTest))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.SMARTLog))
            {
                sb.AppendLine().Append("S.M.A.R.T. error logging is supported");

                if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.
                                                                SMARTLog))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.IdleImmediate))
            {
                sb.AppendLine().Append("IDLE IMMEDIATE with UNLOAD FEATURE is supported");

                if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.
                                                                IdleImmediate))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.WriteURG))
                sb.AppendLine().Append("URG bit is supported in WRITE STREAM DMA EXT and WRITE STREAM EXT");

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.ReadURG))
                sb.AppendLine().Append("URG bit is supported in READ STREAM DMA EXT and READ STREAM EXT");

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.WWN))
                sb.AppendLine().Append("Device has a World Wide Name");

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.FUAWriteQ))
            {
                sb.AppendLine().Append("WRITE DMA QUEUED FUA EXT is supported");

                if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.
                                                                FUAWriteQ))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.FUAWrite))
            {
                sb.AppendLine().Append("WRITE DMA FUA EXT and WRITE MULTIPLE FUA EXT are supported");

                if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.
                                                                FUAWrite))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.GPL))
            {
                sb.AppendLine().Append("General Purpose Logging is supported");

                if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.GPL))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.Streaming))
            {
                sb.AppendLine().Append("Streaming feature set is supported");

                if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.
                                                                Streaming))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MCPT))
            {
                sb.AppendLine().Append("Media Card Pass Through command set is supported");

                if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MCPT))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MediaSerial))
            {
                sb.AppendLine().Append("Media Serial is supported");

                if(ATAID.EnabledCommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.
                                                                MediaSerial))
                    sb.Append(" and valid");
            }
        }

        if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.MustBeSet) &&
           !ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.MustBeClear))
        {
            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.DSN))
            {
                sb.AppendLine().Append("DSN feature set is supported");

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.DSN))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.AMAC))
            {
                sb.AppendLine().Append("Accessible Max Address Configuration is supported");

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.AMAC))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.ExtPowerCond))
            {
                sb.AppendLine().Append("Extended Power Conditions are supported");

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.
                                                                ExtPowerCond))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.ExtStatusReport))
            {
                sb.AppendLine().Append("Extended Status Reporting is supported");

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.
                                                                ExtStatusReport))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.FreeFallControl))
            {
                sb.AppendLine().Append("Free-fall control feature set is supported");

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.
                                                                FreeFallControl))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.
                                                     SegmentedDownloadMicrocode))
            {
                sb.AppendLine().Append("Segmented feature in DOWNLOAD MICROCODE is supported");

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.
                                                                SegmentedDownloadMicrocode))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.RWDMAExtGpl))
            {
                sb.AppendLine().Append("READ/WRITE DMA EXT GPL are supported");

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.
                                                                RWDMAExtGpl))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.WriteUnc))
            {
                sb.AppendLine().Append("WRITE UNCORRECTABLE is supported");

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.
                                                                WriteUnc))
                    sb.Append(" and enabled");
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.WRV))
            {
                sb.AppendLine().Append("Write/Read/Verify is supported");

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.WRV))
                    sb.Append(" and enabled");

                sb.AppendLine().AppendFormat("{0} sectors for Write/Read/Verify mode 2", ATAID.WRVSectorCountMode2);
                sb.AppendLine().AppendFormat("{0} sectors for Write/Read/Verify mode 3", ATAID.WRVSectorCountMode3);

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.WRV))
                    sb.AppendLine().AppendFormat("Current Write/Read/Verify mode: {0}", ATAID.WRVMode);
            }

            if(ATAID.CommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.DT1825))
            {
                sb.AppendLine().Append("DT1825 is supported");

                if(ATAID.EnabledCommandSet4.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit4.DT1825))
                    sb.Append(" and enabled");
            }
        }

        if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.BlockErase))
            sb.AppendLine().Append("BLOCK ERASE EXT is supported");

        if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.Overwrite))
            sb.AppendLine().Append("OVERWRITE EXT is supported");

        if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.CryptoScramble))
            sb.AppendLine().Append("CRYPTO SCRAMBLE EXT is supported");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.DeviceConfDMA))
            sb.AppendLine().
               Append("DEVICE CONFIGURATION IDENTIFY DMA and DEVICE CONFIGURATION SET DMA are supported");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.ReadBufferDMA))
            sb.AppendLine().Append("READ BUFFER DMA is supported");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.WriteBufferDMA))
            sb.AppendLine().Append("WRITE BUFFER DMA is supported");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.DownloadMicroCodeDMA))
            sb.AppendLine().Append("DOWNLOAD MICROCODE DMA is supported");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.SetMaxDMA))
            sb.AppendLine().Append("SET PASSWORD DMA and SET UNLOCK DMA are supported");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.Ata28))
            sb.AppendLine().Append("Not all 28-bit commands are supported");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.CFast))
            sb.AppendLine().Append("Device follows CFast specification");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.IEEE1667))
            sb.AppendLine().Append("Device follows IEEE-1667");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.DeterministicTrim))
        {
            sb.AppendLine().Append("Read after TRIM is deterministic");

            if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.ReadZeroTrim))
                sb.AppendLine().Append("Read after TRIM returns empty data");
        }

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.
                                                 LongPhysSectorAligError))
            sb.AppendLine().Append("Device supports Long Physical Sector Alignment Error Reporting Control");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.Encrypted))
            sb.AppendLine().Append("Device encrypts all user data");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.AllCacheNV))
            sb.AppendLine().Append("Device's write cache is non-volatile");

        if(ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.ZonedBit0) ||
           ATAID.CommandSet5.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit5.ZonedBit1))
            sb.AppendLine().Append("Device is zoned");

        if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.Sanitize))
        {
            sb.AppendLine().Append("Sanitize feature set is supported");

            sb.AppendLine().
               Append(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.
                                                              SanitizeCommands)
                          ? "Sanitize commands are specified by ACS-3 or higher"
                          : "Sanitize commands are specified by ACS-2");

            if(ATAID.Capabilities3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CapabilitiesBit3.
                                                       SanitizeAntifreeze))
                sb.AppendLine().Append("SANITIZE ANTIFREEZE LOCK EXT is supported");
        }

        if(!ata1 &&
           maxatalevel >= 8)
            if(ATAID.TrustedComputing.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TrustedComputingBit.Set) &&
               !ATAID.TrustedComputing.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TrustedComputingBit.
                                                           Clear) &&
               ATAID.TrustedComputing.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.TrustedComputingBit.
                                                          TrustedComputing))
                sb.AppendLine().Append("Trusted Computing feature set is supported");

        if((ATAID.TransportMajorVersion & 0xF000) >> 12 == 0x1 ||
           (ATAID.TransportMajorVersion & 0xF000) >> 12 == 0xE)
        {
            if(!ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.Clear))
                if(ATAID.SATACapabilities.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit.
                                                              ReadLogDMAExt))
                    sb.AppendLine().Append("READ LOG DMA EXT is supported");

            if(!ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit2.
                                                            Clear))
                if(ATAID.SATACapabilities2.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATACapabilitiesBit2.
                                                               FPDMAQ))
                    sb.AppendLine().Append("RECEIVE FPDMA QUEUED and SEND FPDMA QUEUED are supported");

            if(!ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.Clear))
            {
                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                          NonZeroBufferOffset))
                {
                    sb.AppendLine().Append("Non-zero buffer offsets are supported");

                    if(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                                     NonZeroBufferOffset))
                        sb.Append(" and enabled");
                }

                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.DMASetup))
                {
                    sb.AppendLine().Append("DMA Setup auto-activation is supported");

                    if(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                                     DMASetup))
                        sb.Append(" and enabled");
                }

                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                          InitPowerMgmt))
                {
                    sb.AppendLine().Append("Device-initiated power management is supported");

                    if(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                                     InitPowerMgmt))
                        sb.Append(" and enabled");
                }

                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.InOrderData))
                {
                    sb.AppendLine().Append("In-order data delivery is supported");

                    if(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                                     InOrderData))
                        sb.Append(" and enabled");
                }

                if(!atapi)
                    if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                              HardwareFeatureControl))
                    {
                        sb.AppendLine().Append("Hardware Feature Control is supported");

                        if(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                         SATAFeaturesBit.HardwareFeatureControl))
                            sb.Append(" and enabled");
                    }

                if(atapi)
                    if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                              AsyncNotification))
                    {
                        sb.AppendLine().Append("Asynchronous notification is supported");

                        if(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                         SATAFeaturesBit.AsyncNotification))
                            sb.Append(" and enabled");
                    }

                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                          SettingsPreserve))
                {
                    sb.AppendLine().Append("Software Settings Preservation is supported");

                    if(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                                     SettingsPreserve))
                        sb.Append(" and enabled");
                }

                if(ATAID.SATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                          NCQAutoSense))
                    sb.AppendLine().Append("NCQ Autosense is supported");

                if(ATAID.EnabledSATAFeatures.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SATAFeaturesBit.
                                                                 EnabledSlumber))
                    sb.AppendLine().Append("Automatic Partial to Slumber transitions are enabled");
            }
        }

        if((ATAID.RemovableStatusSet & 0x03) > 0)
            sb.AppendLine().Append("Removable Media Status Notification feature set is supported");

        if(ATAID.FreeFallSensitivity != 0x00 &&
           ATAID.FreeFallSensitivity != 0xFF)
            sb.AppendLine().AppendFormat("Free-fall sensitivity set to {0}", ATAID.FreeFallSensitivity);

        if(ATAID.DataSetMgmt.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.DataSetMgmtBit.Trim))
            sb.AppendLine().Append("TRIM is supported");

        if(ATAID.DataSetMgmtSize > 0)
            sb.AppendLine().AppendFormat("DATA SET MANAGEMENT can receive a maximum of {0} blocks of 512 bytes",
                                         ATAID.DataSetMgmtSize);

        sb.AppendLine().AppendLine();

        if(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit.Supported))
        {
            sb.AppendLine("Security:");

            if(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit.Enabled))
            {
                sb.AppendLine("Security is enabled");

                sb.AppendLine(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                       SecurityStatusBit.Locked)
                                  ? "Security is locked" : "Security is not locked");

                sb.AppendLine(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                       SecurityStatusBit.Frozen)
                                  ? "Security is frozen" : "Security is not frozen");

                sb.AppendLine(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                       SecurityStatusBit.Expired)
                                  ? "Security count has expired" : "Security count has not expired");

                sb.AppendLine(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.
                                                                       SecurityStatusBit.Maximum)
                                  ? "Security level is maximum" : "Security level is high");
            }
            else
                sb.AppendLine("Security is not enabled");

            if(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit.Enhanced))
                sb.AppendLine("Supports enhanced security erase");

            sb.AppendFormat("{0} minutes to complete secure erase", ATAID.SecurityEraseTime * 2).AppendLine();

            if(ATAID.SecurityStatus.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SecurityStatusBit.Enhanced))
                sb.AppendFormat("{0} minutes to complete enhanced secure erase",
                                ATAID.EnhancedSecurityEraseTime * 2).AppendLine();

            sb.AppendFormat("Master password revision code: {0}", ATAID.MasterPasswordRevisionCode).AppendLine();
        }

        if(ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeSet)    &&
           !ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.MustBeClear) &&
           ATAID.CommandSet3.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.CommandSetBit3.Streaming))
        {
            sb.AppendLine().AppendLine("Streaming:");
            sb.AppendFormat("Minimum request size is {0}", ATAID.StreamMinReqSize);
            sb.AppendFormat("Streaming transfer time in PIO is {0}", ATAID.StreamTransferTimePIO);
            sb.AppendFormat("Streaming transfer time in DMA is {0}", ATAID.StreamTransferTimeDMA);
            sb.AppendFormat("Streaming access latency is {0}", ATAID.StreamAccessLatency);
            sb.AppendFormat("Streaming performance granularity is {0}", ATAID.StreamPerformanceGranularity);
        }

        if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit.
                                                         Supported))
        {
            sb.AppendLine().AppendLine("S.M.A.R.T. Command Transport (SCT):");

            if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit.
                                                             LongSectorAccess))
                sb.AppendLine("SCT Long Sector Address is supported");

            if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit.
                                                             WriteSame))
                sb.AppendLine("SCT Write Same is supported");

            if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit.
                                                             ErrorRecoveryControl))
                sb.AppendLine("SCT Error Recovery Control is supported");

            if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit.
                                                             FeaturesControl))
                sb.AppendLine("SCT Features Control is supported");

            if(ATAID.SCTCommandTransport.HasFlag(CommonTypes.Structs.Devices.ATA.Identify.SCTCommandTransportBit.
                                                             DataTables))
                sb.AppendLine("SCT Data Tables are supported");
        }

        if((ATAID.NVCacheCaps & 0x0010) == 0x0010)
        {
            sb.AppendLine().AppendLine("Non-Volatile Cache:");
            sb.AppendLine().AppendFormat("Version {0}", (ATAID.NVCacheCaps & 0xF000) >> 12).AppendLine();

            if((ATAID.NVCacheCaps & 0x0001) == 0x0001)
            {
                sb.Append("Power mode feature set is supported");

                if((ATAID.NVCacheCaps & 0x0002) == 0x0002)
                    sb.Append(" and enabled");

                sb.AppendLine();

                sb.AppendLine().AppendFormat("Version {0}", (ATAID.NVCacheCaps & 0x0F00) >> 8).AppendLine();
            }

            sb.AppendLine().AppendFormat("Non-Volatile Cache is {0} bytes", ATAID.NVCacheSize * logicalSectorSize).
               AppendLine();
        }

    #if DEBUG
        sb.AppendLine();

        if(ATAID.VendorWord9 != 0x0000 &&
           ATAID.VendorWord9 != 0xFFFF)
            sb.AppendFormat("Word 9: 0x{0:X4}", ATAID.VendorWord9).AppendLine();

        if((ATAID.VendorWord47 & 0x7F) != 0x7F &&
           (ATAID.VendorWord47 & 0x7F) != 0x00)
            sb.AppendFormat("Word 47 bits 15 to 8: 0x{0:X2}", ATAID.VendorWord47).AppendLine();

        if(ATAID.VendorWord51 != 0x00 &&
           ATAID.VendorWord51 != 0xFF)
            sb.AppendFormat("Word 51 bits 7 to 0: 0x{0:X2}", ATAID.VendorWord51).AppendLine();

        if(ATAID.VendorWord52 != 0x00 &&
           ATAID.VendorWord52 != 0xFF)
            sb.AppendFormat("Word 52 bits 7 to 0: 0x{0:X2}", ATAID.VendorWord52).AppendLine();

        if(ATAID.ReservedWord64 != 0x00 &&
           ATAID.ReservedWord64 != 0xFF)
            sb.AppendFormat("Word 64 bits 15 to 8: 0x{0:X2}", ATAID.ReservedWord64).AppendLine();

        if(ATAID.ReservedWord70 != 0x0000 &&
           ATAID.ReservedWord70 != 0xFFFF)
            sb.AppendFormat("Word 70: 0x{0:X4}", ATAID.ReservedWord70).AppendLine();

        if(ATAID.ReservedWord73 != 0x0000 &&
           ATAID.ReservedWord73 != 0xFFFF)
            sb.AppendFormat("Word 73: 0x{0:X4}", ATAID.ReservedWord73).AppendLine();

        if(ATAID.ReservedWord74 != 0x0000 &&
           ATAID.ReservedWord74 != 0xFFFF)
            sb.AppendFormat("Word 74: 0x{0:X4}", ATAID.ReservedWord74).AppendLine();

        if(ATAID.ReservedWord116 != 0x0000 &&
           ATAID.ReservedWord116 != 0xFFFF)
            sb.AppendFormat("Word 116: 0x{0:X4}", ATAID.ReservedWord116).AppendLine();

        for(int i = 0; i < ATAID.ReservedWords121.Length; i++)
            if(ATAID.ReservedWords121[i] != 0x0000 &&
               ATAID.ReservedWords121[i] != 0xFFFF)
                sb.AppendFormat("Word {1}: 0x{0:X4}", ATAID.ReservedWords121[i], 121 + i).AppendLine();

        for(int i = 0; i < ATAID.ReservedWords129.Length; i++)
            if(ATAID.ReservedWords129[i] != 0x0000 &&
               ATAID.ReservedWords129[i] != 0xFFFF)
                sb.AppendFormat("Word {1}: 0x{0:X4}", ATAID.ReservedWords129[i], 129 + i).AppendLine();

        for(int i = 0; i < ATAID.ReservedCFA.Length; i++)
            if(ATAID.ReservedCFA[i] != 0x0000 &&
               ATAID.ReservedCFA[i] != 0xFFFF)
                sb.AppendFormat("Word {1} (CFA): 0x{0:X4}", ATAID.ReservedCFA[i], 161 + i).AppendLine();

        if(ATAID.ReservedWord174 != 0x0000 &&
           ATAID.ReservedWord174 != 0xFFFF)
            sb.AppendFormat("Word 174: 0x{0:X4}", ATAID.ReservedWord174).AppendLine();

        if(ATAID.ReservedWord175 != 0x0000 &&
           ATAID.ReservedWord175 != 0xFFFF)
            sb.AppendFormat("Word 175: 0x{0:X4}", ATAID.ReservedWord175).AppendLine();

        if(ATAID.ReservedCEATAWord207 != 0x0000 &&
           ATAID.ReservedCEATAWord207 != 0xFFFF)
            sb.AppendFormat("Word 207 (CE-ATA): 0x{0:X4}", ATAID.ReservedCEATAWord207).AppendLine();

        if(ATAID.ReservedCEATAWord208 != 0x0000 &&
           ATAID.ReservedCEATAWord208 != 0xFFFF)
            sb.AppendFormat("Word 208 (CE-ATA): 0x{0:X4}", ATAID.ReservedCEATAWord208).AppendLine();

        if(ATAID.NVReserved != 0x00 &&
           ATAID.NVReserved != 0xFF)
            sb.AppendFormat("Word 219 bits 15 to 8: 0x{0:X2}", ATAID.NVReserved).AppendLine();

        if(ATAID.WRVReserved != 0x00 &&
           ATAID.WRVReserved != 0xFF)
            sb.AppendFormat("Word 220 bits 15 to 8: 0x{0:X2}", ATAID.WRVReserved).AppendLine();

        if(ATAID.ReservedWord221 != 0x0000 &&
           ATAID.ReservedWord221 != 0xFFFF)
            sb.AppendFormat("Word 221: 0x{0:X4}", ATAID.ReservedWord221).AppendLine();

        for(int i = 0; i < ATAID.ReservedCEATA224.Length; i++)
            if(ATAID.ReservedCEATA224[i] != 0x0000 &&
               ATAID.ReservedCEATA224[i] != 0xFFFF)
                sb.AppendFormat("Word {1} (CE-ATA): 0x{0:X4}", ATAID.ReservedCEATA224[i], 224 + i).AppendLine();

        for(int i = 0; i < ATAID.ReservedWords.Length; i++)
            if(ATAID.ReservedWords[i] != 0x0000 &&
               ATAID.ReservedWords[i] != 0xFFFF)
                sb.AppendFormat("Word {1}: 0x{0:X4}", ATAID.ReservedWords[i], 236 + i).AppendLine();
    #endif
        return sb.ToString();
    }
}