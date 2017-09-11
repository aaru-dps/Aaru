// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Modes.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI modes.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace DiscImageChef.Decoders.SCSI
{
    public static partial class Modes
    {
        public struct BlockDescriptor
        {
            public DensityType Density;
            public ulong Blocks;
            public uint BlockLength;
        }

        public struct ModeHeader
        {
            public MediumTypes MediumType;
            public bool WriteProtected;
            public BlockDescriptor[] BlockDescriptors;
            public byte Speed;
            public byte BufferedMode;
            public bool EBC;
            public bool DPOFUA;
        }

        public static ModeHeader? DecodeModeHeader6(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            if(modeResponse == null || modeResponse.Length < 4 || modeResponse.Length < modeResponse[0] + 1)
                return null;

            ModeHeader header = new ModeHeader();
            header.MediumType = (MediumTypes)modeResponse[1];

            if(modeResponse[3] > 0)
            {
                header.BlockDescriptors = new BlockDescriptor[modeResponse[3] / 8];
                for(int i = 0; i < header.BlockDescriptors.Length; i++)
                {
                    header.BlockDescriptors[i].Density = (DensityType)modeResponse[0 + i * 8 + 4];
                    header.BlockDescriptors[i].Blocks += (ulong)(modeResponse[1 + i * 8 + 4] << 16);
                    header.BlockDescriptors[i].Blocks += (ulong)(modeResponse[2 + i * 8 + 4] << 8);
                    header.BlockDescriptors[i].Blocks += modeResponse[3 + i * 8 + 4];
                    header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[5 + i * 8 + 4] << 16);
                    header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[6 + i * 8 + 4] << 8);
                    header.BlockDescriptors[i].BlockLength += modeResponse[7 + i * 8 + 4];
                }
            }

            if(deviceType == PeripheralDeviceTypes.DirectAccess || deviceType == PeripheralDeviceTypes.MultiMediaDevice)
            {
                header.WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
                header.DPOFUA = ((modeResponse[2] & 0x10) == 0x10);
            }

            if(deviceType == PeripheralDeviceTypes.SequentialAccess)
            {
                header.WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
                header.Speed = (byte)(modeResponse[2] & 0x0F);
                header.BufferedMode = (byte)((modeResponse[2] & 0x70) >> 4);
            }

            if(deviceType == PeripheralDeviceTypes.PrinterDevice)
                header.BufferedMode = (byte)((modeResponse[2] & 0x70) >> 4);

            if(deviceType == PeripheralDeviceTypes.OpticalDevice)
            {
                header.WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
                header.EBC = ((modeResponse[2] & 0x01) == 0x01);
                header.DPOFUA = ((modeResponse[2] & 0x10) == 0x10);
            }

            return header;
        }

        public static string PrettifyModeHeader6(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            return PrettifyModeHeader(DecodeModeHeader6(modeResponse, deviceType), deviceType);
        }

        public static string GetMediumTypeDescription(MediumTypes type)
        {
            switch(type)
            {
                case MediumTypes.ECMA54:
                    return "ECMA-54: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on One Side";
                case MediumTypes.ECMA59:
                    return "ECMA-59 & ANSI X3.121-1984: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on Both Sides";
                case MediumTypes.ECMA69:
                    return "ECMA-69: 200 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides";
                case MediumTypes.ECMA66:
                    return "ECMA-66: 130 mm Flexible Disk Cartridge using Two-Frequency Recording at 7958 ftprad on One Side";
                case MediumTypes.ECMA70:
                    return "ECMA-70 & ANSI X3.125-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 1,9 Tracks per mm";
                case MediumTypes.ECMA78:
                    return "ECMA-78 & ANSI X3.126-1986: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 3,8 Tracks per mm";
                case MediumTypes.ECMA99:
                    return "ECMA-99 & ISO 8630-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides; 3,8 Tracks per mm";
                case MediumTypes.ECMA100:
                    return "ECMA-100 & ANSI X3.137: 90 mm Flexible Disk Cartridge using MFM Recording at 7859 ftprad on Both Sides; 5,3 Tracks per mm";
                // Most probably they will never appear, but magneto-opticals use these codes
                /*
                case MediumTypes.Unspecified_SS:
                    return "Unspecified single sided flexible disk";
                case MediumTypes.Unspecified_DS:
                    return "Unspecified double sided flexible disk";
                */
                case MediumTypes.X3_73:
                    return "ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 1 side";
                case MediumTypes.X3_73_DS:
                    return "ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 2 sides";
                case MediumTypes.X3_82:
                    return "ANSI X3.80-1980: 130 mm, 3979 ftprad, 1,9 Tracks per mm, 1 side";
                case MediumTypes.Type3Floppy:
                    return "3.5-inch, 135 tpi, 12362 bits/radian, double-sided MFM (aka 1.25Mb)";
                case MediumTypes.HDFloppy:
                    return "3.5-inch, 135 tpi, 15916 bits/radian, double-sided MFM (aka 1.44Mb)";
                case MediumTypes.ReadOnly:
                    return "a Read-only optical";
                case MediumTypes.WORM:
                    return "a Write-once Read-many optical";
                case MediumTypes.Erasable:
                    return "a Erasable optical";
                case MediumTypes.RO_WORM:
                    return "a combination of read-only and write-once optical";
                // These magneto-opticals were never manufactured
                /*
                case MediumTypes.RO_RW:
                    return "a combination of read-only and erasable optical";
                    break;
                case MediumTypes.WORM_RW:
                    return "a combination of write-once and erasable optical";
                */
                case MediumTypes.DOW:
                    return "a direct-overwrite optical";
                default:
                    return string.Format("Unknown medium type 0x{0:X2}", (byte)type);
            }
        }

        public static string PrettifyModeHeader(ModeHeader? header, PeripheralDeviceTypes deviceType)
        {
            if(!header.HasValue)
                return null;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Mode Sense Header:");

            switch(deviceType)
            {
                #region Direct access device mode header
                case PeripheralDeviceTypes.DirectAccess:
                    {
                        if(header.Value.MediumType != MediumTypes.Default)
                            sb.AppendFormat("\tMedium is {0}", GetMediumTypeDescription(header.Value.MediumType)).AppendLine();

                        if(header.Value.WriteProtected)
                            sb.AppendLine("\tMedium is write protected");

                        if(header.Value.DPOFUA)
                            sb.AppendLine("\tDrive supports DPO and FUA bits");

                        if(header.Value.BlockDescriptors != null)
                        {
                            foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                            {
                                string density = "";
                                switch(descriptor.Density)
                                {
                                    case DensityType.Default:
                                        break;
                                    case DensityType.Flux7958:
                                        density = "7958 flux transitions per radian";
                                        break;
                                    case DensityType.Flux13262:
                                        density = "13262 flux transitions per radian";
                                        break;
                                    case DensityType.Flux15916:
                                        density = "15916 flux transitions per radian";
                                        break;
                                    default:
                                        density = string.Format("with unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                        break;
                                }

                                if(density != "")
                                {
                                    if(descriptor.Blocks == 0)
                                        sb.AppendFormat("\tAll remaining blocks have {0} and are {1} bytes each", density, descriptor.BlockLength).AppendLine();
                                    else
                                        sb.AppendFormat("\t{0} blocks have {1} and are {2} bytes each", descriptor.Blocks, density, descriptor.BlockLength).AppendLine();
                                }
                                else
                                {
                                    if(descriptor.Blocks == 0)
                                        sb.AppendFormat("\tAll remaining blocks are {0} bytes each", descriptor.BlockLength).AppendLine();
                                    else
                                        sb.AppendFormat("\t{0} blocks are {1} bytes each", descriptor.Blocks, descriptor.BlockLength).AppendLine();
                                }
                            }
                        }

                        break;
                    }
                #endregion Direct access device mode header
                #region Sequential access device mode header
                case PeripheralDeviceTypes.SequentialAccess:
                    {
                        switch(header.Value.BufferedMode)
                        {
                            case 0:
                                sb.AppendLine("\tDevice writes directly to media");
                                break;
                            case 1:
                                sb.AppendLine("\tDevice uses a write cache");
                                break;
                            case 2:
                                sb.AppendLine("\tDevice uses a write cache but doesn't return until cache is flushed");
                                break;
                            default:
                                sb.AppendFormat("\tUnknown buffered mode code 0x{0:X2}", header.Value.BufferedMode).AppendLine();
                                break;
                        }

                        if(header.Value.Speed == 0)
                            sb.AppendLine("\tDevice uses default speed");
                        else
                            sb.AppendFormat("\tDevice uses speed {0}", header.Value.Speed).AppendLine();

                        if(header.Value.WriteProtected)
                            sb.AppendLine("\tMedium is write protected");

                        string medium = "";

                        switch(header.Value.MediumType)
                        {
                            case MediumTypes.Default:
                                medium = "undefined";
                                break;
                            case MediumTypes.Tape12:
                                medium = "6,3 mm tape with 12 tracks at 394 ftpmm or DC-9250";
                                break;
                            case MediumTypes.Tape24:
                                medium = "6,3 mm tape with 24 tracks at 394 ftpmm or MLR1-26GBSL";
                                break;
                            case MediumTypes.LTOWORM:
                                medium = "LTO Ultrium WORM or cleaning cartridge";
                                break;
                            case MediumTypes.LTO:
                                medium = "LTO Ultrium";
                                break;
                            case MediumTypes.LTO2:
                                medium = "LTO Ultrium-2";
                                break;
                            case MediumTypes.DC2900SL:
                                medium = "DC-2900SL";
                                break;
                            case MediumTypes.MLR1:
                                medium = "MLR1-26GB or DDS-3";
                                break;
                            case MediumTypes.DC9200:
                                medium = "DC-9200 or DDS-4";
                                break;
                            case MediumTypes.DAT72:
                                medium = "DAT-72";
                                break;
                            case MediumTypes.LTO3:
                                medium = "LTO Ultrium-3";
                                break;
                            case MediumTypes.LTO3WORM:
                                medium = "LTO Ultrium-3 WORM";
                                break;
                            case MediumTypes.DDSCleaning:
                                medium = "DDS cleaning cartridge";
                                break;
                            case MediumTypes.SLR32:
                                medium = "SLR-32";
                                break;
                            case MediumTypes.SLRtape50:
                                medium = "SLRtape-50";
                                break;
                            case MediumTypes.LTO4:
                                medium = "LTO Ultrium-4";
                                break;
                            case MediumTypes.LTO4WORM:
                                medium = "LTO Ultrium-4 WORM";
                                break;
                            case MediumTypes.SLRtape50SL:
                                medium = "SLRtape-50 SL";
                                break;
                            case MediumTypes.SLR32SL:
                                medium = "SLR-32SL";
                                break;
                            case MediumTypes.SLR5:
                                medium = "SLR-5";
                                break;
                            case MediumTypes.SLR5SL:
                                medium = "SLR-5SL";
                                break;
                            case MediumTypes.LTO5:
                                medium = "LTO Ultrium-5";
                                break;
                            case MediumTypes.LTO5WORM:
                                medium = "LTO Ultrium-5 WORM";
                                break;
                            case MediumTypes.SLRtape7:
                                medium = "SLRtape-7";
                                break;
                            case MediumTypes.SLRtape7SL:
                                medium = "SLRtape-7 SL";
                                break;
                            case MediumTypes.SLRtape24:
                                medium = "SLRtape-24";
                                break;
                            case MediumTypes.SLRtape24SL:
                                medium = "SLRtape-24 SL";
                                break;
                            case MediumTypes.LTO6:
                                medium = "LTO Ultrium-6";
                                break;
                            case MediumTypes.LTO6WORM:
                                medium = "LTO Ultrium-6 WORM";
                                break;
                            case MediumTypes.SLRtape140:
                                medium = "SLRtape-140";
                                break;
                            case MediumTypes.SLRtape40:
                                medium = "SLRtape-40";
                                break;
                            case MediumTypes.SLRtape60:
                                medium = "SLRtape-60 or SLRtape-75";
                                break;
                            case MediumTypes.SLRtape100:
                                medium = "SLRtape-100";
                                break;
                            case MediumTypes.SLR40_60_100:
                                medium = "SLR-40, SLR-60 or SLR-100";
                                break;
                            case MediumTypes.LTO7:
                                medium = "LTO Ultrium-7";
                                break;
                            case MediumTypes.LTO7WORM:
                                medium = "LTO Ultrium-7 WORM";
                                break;
                            case MediumTypes.LTOCD:
                                medium = "LTO Ultrium";
                                break;
                            case MediumTypes.Exatape15m:
                                medium = "Exatape 15m, IBM MagStar or VXA";
                                break;
                            case MediumTypes.CT1:
                                medium = "CompactTape I, Exatape 28m, CompactTape II, VXA-2 or VXA-3";
                                break;
                            case MediumTypes.Exatape54m:
                                medium = "Exatape 54m or DLTtape III";
                                break;
                            case MediumTypes.Exatape80m:
                                medium = "Exatape 80m or DLTtape IIIxt";
                                break;
                            case MediumTypes.Exatape106m:
                                medium = "Exatape 106m, DLTtape IV or Travan 5";
                                break;
                            case MediumTypes.Exatape106mXL:
                                medium = "Exatape 160m XL or Super DLTtape I";
                                break;
                            case MediumTypes.SDLT2:
                                medium = "Super DLTtape II";
                                break;
                            case MediumTypes.VStapeI:
                                medium = "VStape I";
                                break;
                            case MediumTypes.DLTtapeS4:
                                medium = "DLTtape S4";
                                break;
                            case MediumTypes.Travan7:
                                medium = "Travan 7";
                                break;
                            case MediumTypes.Exatape22m:
                                medium = "Exatape 22m";
                                break;
                            case MediumTypes.Exatape40m:
                                medium = "Exatape 40m";
                                break;
                            case MediumTypes.Exatape76m:
                                medium = "Exatape 76m";
                                break;
                            case MediumTypes.Exatape112m:
                                medium = "Exatape 112m";
                                break;
                            case MediumTypes.Exatape22mAME:
                                medium = "Exatape 22m AME";
                                break;
                            case MediumTypes.Exatape170m:
                                medium = "Exatape 170m";
                                break;
                            case MediumTypes.Exatape125m:
                                medium = "Exatape 125m";
                                break;
                            case MediumTypes.Exatape45m:
                                medium = "Exatape 45m";
                                break;
                            case MediumTypes.Exatape225m:
                                medium = "Exatape 225m";
                                break;
                            case MediumTypes.Exatape150m:
                                medium = "Exatape 150m";
                                break;
                            case MediumTypes.Exatape75m:
                                medium = "Exatape 75m";
                                break;
                            default:
                                medium = string.Format("unknown medium type 0x{0:X2}", (byte)header.Value.MediumType);
                                break;
                        }

                        sb.AppendFormat("\tMedium is {0}", medium).AppendLine();

                        if(header.Value.BlockDescriptors != null)
                        {
                            foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                            {
                                string density = "";
                                switch(header.Value.MediumType)
                                {
                                    case MediumTypes.Default:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    break;
                                                case DensityType.ECMA62:
                                                    density = "ECMA-62 & ANSI X3.22-1983: 12,7 mm 9-Track Magnetic Tape, 32 ftpmm, NRZI, 32 cpmm";
                                                    break;
                                                case DensityType.ECMA62_Phase:
                                                    density = "ECMA-62 & ANSI X3.39-1986: 12,7 mm 9-Track Magnetic Tape, 126 ftpmm, Phase Encoding, 63 cpmm";
                                                    break;
                                                case DensityType.ECMA62_GCR:
                                                    density = "ECMA-62 & ANSI X3.54-1986: 12,7 mm 9-Track Magnetic Tape, 356 ftpmm, NRZI, 245 cpmm GCR";
                                                    break;
                                                case DensityType.ECMA79:
                                                    density = "ECMA-79 & ANSI X3.116-1986: 6,30 mm Magnetic Tape Cartridge, 252 ftpmm, MFM";
                                                    break;
                                                case DensityType.IBM3480:
                                                    density = "Draft ECMA & ANSI X3B5/87-099: 12,7 mm 18-Track Magnetic Tape Cartridge, 1944 ftpmm, IFM, GCR (IBM 3480, 3490, 3490E)";
                                                    break;
                                                case DensityType.ECMA46:
                                                    density = "ECMA-46 & ANSI X3.56-1986: 6,30 mm Magnetic Tape Cartridge, Phase Encoding, 63 bpmm";
                                                    break;
                                                case DensityType.ECMA98:
                                                    density = "ECMA-98: 6,30 mm Magnetic Tape Cartridge, NRZI, 394 ftpmm";
                                                    break;
                                                case DensityType.X3_136:
                                                    density = "ANXI X3.136-1986: 6,3 mm 4 or 9-Track Magnetic Tape Cartridge, 315 bpmm, GCR (QIC-24)";
                                                    break;
                                                case DensityType.X3_157:
                                                    density = "ANXI X3.157-1987: 12,7 mm 9-Track Magnetic Tape, 126 bpmm, Phase Encoding";
                                                    break;
                                                case DensityType.X3_158:
                                                    density = "ANXI X3.158-1987: 3,81 mm 4-Track Magnetic Tape Cassette, 315 bpmm, GCR";
                                                    break;
                                                case DensityType.X3B5_86:
                                                    density = "ANXI X3B5/86-199: 12,7 mm 22-Track Magnetic Tape Cartridge, 262 bpmm, MFM";
                                                    break;
                                                case DensityType.HiTC1:
                                                    density = "HI-TC1: 12,7 mm 24-Track Magnetic Tape Cartridge, 500 bpmm, GCR";
                                                    break;
                                                case DensityType.HiTC2:
                                                    density = "HI-TC2: 12,7 mm 24-Track Magnetic Tape Cartridge, 999 bpmm, GCR";
                                                    break;
                                                case DensityType.QIC120:
                                                    density = "QIC-120: 6,3 mm 15-Track Magnetic Tape Cartridge, 394 bpmm, GCR";
                                                    break;
                                                case DensityType.QIC150:
                                                    density = "QIC-150: 6,3 mm 18-Track Magnetic Tape Cartridge, 394 bpmm, GCR";
                                                    break;
                                                case DensityType.QIC320:
                                                    density = "QIC-320: 6,3 mm 26-Track Magnetic Tape Cartridge, 630 bpmm, GCR";
                                                    break;
                                                case DensityType.QIC1350:
                                                    density = "QIC-1350: 6,3 mm 30-Track Magnetic Tape Cartridge, 2034 bpmm, RLL";
                                                    break;
                                                case DensityType.X3B5_88:
                                                    density = "ANXI X3B5/88-185A: 3,81 mm Magnetic Tape Cassette, 2400 bpmm, DDS";
                                                    break;
                                                case DensityType.X3_202:
                                                    density = "ANXI X3.202-1991: 8 mm Magnetic Tape Cassette, 1703 bpmm, RLL";
                                                    break;
                                                case DensityType.ECMA_TC17:
                                                    density = "ECMA TC17: 8 mm Magnetic Tape Cassette, 1789 bpmm, RLL";
                                                    break;
                                                case DensityType.X3_193:
                                                    density = "ANXI X3.193-1990: 12,7 mm 48-Track Magnetic Tape Cartridge, 394 bpmm, MFM";
                                                    break;
                                                case DensityType.X3B5_91:
                                                    density = "ANXI X3B5/97-174: 12,7 mm 48-Track Magnetic Tape Cartridge, 1673 bpmm, MFM";
                                                    break;
                                                case DensityType.QIC11:
                                                    density = "QIC-11";
                                                    break;
                                                case DensityType.IBM3490E:
                                                    density = "IBM 3490E";
                                                    break;
                                                case DensityType.LTO1:
                                                    //case DensityType.SAIT1:
                                                    density = "LTO Ultrium or Super AIT-1";
                                                    break;
                                                case DensityType.LTO2Old:
                                                    density = "LTO Ultrium-2";
                                                    break;
                                                case DensityType.LTO2:
                                                    //case DensityType.T9840:
                                                    density = "LTO Ultrium-2 or T9840";
                                                    break;
                                                case DensityType.T9940:
                                                    density = "T9940";
                                                    break;
                                                case DensityType.LTO3:
                                                    //case DensityType.T9940:
                                                    density = "LTO Ultrium-3 or T9940";
                                                    break;
                                                case DensityType.T9840C:
                                                    density = "T9840C";
                                                    break;
                                                case DensityType.LTO4:
                                                    //case DensityType.T9840D:
                                                    density = "LTO Ultrium-4 or T9840D";
                                                    break;
                                                case DensityType.T10000A:
                                                    density = "T10000A";
                                                    break;
                                                case DensityType.T10000B:
                                                    density = "T10000B";
                                                    break;
                                                case DensityType.T10000C:
                                                    density = "T10000C";
                                                    break;
                                                case DensityType.T10000D:
                                                    density = "T10000D";
                                                    break;
                                                case DensityType.AIT1:
                                                    density = "AIT-1";
                                                    break;
                                                case DensityType.AIT2:
                                                    density = "AIT-2";
                                                    break;
                                                case DensityType.AIT3:
                                                    density = "AIT-3";
                                                    break;
                                                case DensityType.DDS2:
                                                    density = "DDS-2";
                                                    break;
                                                case DensityType.DDS3:
                                                    density = "DDS-3";
                                                    break;
                                                case DensityType.DDS4:
                                                    density = "DDS-4";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.LTOWORM:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "LTO Ultrium cleaning cartridge";
                                                    break;
                                                case DensityType.LTO3:
                                                    density = "LTO Ultrium-3 WORM";
                                                    break;
                                                case DensityType.LTO4:
                                                    density = "LTO Ultrium-4 WORM";
                                                    break;
                                                case DensityType.LTO5:
                                                    density = "LTO Ultrium-5 WORM";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.LTO:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.LTO1:
                                                    density = "LTO Ultrium";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.LTO2:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.LTO2:
                                                    density = "LTO Ultrium-2";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.DDS3:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "MLR1-26GB";
                                                    break;
                                                case DensityType.DDS3:
                                                    density = "DDS-3";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.DDS4:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "DC-9200";
                                                    break;
                                                case DensityType.DDS4:
                                                    density = "DDS-4";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.DAT72:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.DAT72:
                                                    density = "DAT-72";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.LTO3:
                                    case MediumTypes.LTO3WORM:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.LTO3:
                                                    density = "LTO Ultrium-3";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.DDSCleaning:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "DDS cleaning cartridge";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.LTO4:
                                    case MediumTypes.LTO4WORM:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.LTO4:
                                                    density = "LTO Ultrium-4";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.LTO5:
                                    case MediumTypes.LTO5WORM:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.LTO5:
                                                    density = "LTO Ultrium-5";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.LTO6:
                                    case MediumTypes.LTO6WORM:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.LTO6:
                                                    density = "LTO Ultrium-6";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.LTO7:
                                    case MediumTypes.LTO7WORM:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.LTO7:
                                                    density = "LTO Ultrium-7";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.LTOCD:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.LTO2:
                                                    density = "LTO Ultrium-2 in CD emulation mode";
                                                    break;
                                                case DensityType.LTO3:
                                                    density = "LTO Ultrium-3 in CD emulation mode";
                                                    break;
                                                case DensityType.LTO4:
                                                    density = "LTO Ultrium-4 in CD emulation mode";
                                                    break;
                                                case DensityType.LTO5:
                                                    density = "LTO Ultrium-5 in CD emulation mode";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape15m:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Ex8200:
                                                    density = "EXB-8200";
                                                    break;
                                                case DensityType.Ex8200c:
                                                    density = "EXB-8200 compressed";
                                                    break;
                                                case DensityType.Ex8500:
                                                    density = "EXB-8500";
                                                    break;
                                                case DensityType.Ex8500c:
                                                    density = "EXB-8500 compressed";
                                                    break;
                                                case DensityType.Mammoth:
                                                    density = "Mammoth";
                                                    break;
                                                case DensityType.IBM3590:
                                                    density = "IBM 3590";
                                                    break;
                                                case DensityType.IBM3590E:
                                                    density = "IBM 3590E";
                                                    break;
                                                case DensityType.VXA1:
                                                    density = "VXA-1";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape28m:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Ex8200:
                                                    density = "EXB-8200";
                                                    break;
                                                case DensityType.Ex8200c:
                                                    density = "EXB-8200 compressed";
                                                    break;
                                                case DensityType.Ex8500:
                                                    density = "EXB-8500";
                                                    break;
                                                case DensityType.Ex8500c:
                                                    density = "EXB-8500 compressed";
                                                    break;
                                                case DensityType.Mammoth:
                                                    density = "Mammoth";
                                                    break;
                                                case DensityType.CT1:
                                                    density = "CompactTape I";
                                                    break;
                                                case DensityType.CT2:
                                                    density = "CompactTape II";
                                                    break;
                                                case DensityType.IBM3590:
                                                    density = "IBM 3590 extended";
                                                    break;
                                                case DensityType.IBM3590E:
                                                    density = "IBM 3590E extended";
                                                    break;
                                                case DensityType.VXA2:
                                                    density = "VXA-2";
                                                    break;
                                                case DensityType.VXA3:
                                                    density = "VXA-3";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape54m:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Ex8200:
                                                    density = "EXB-8200";
                                                    break;
                                                case DensityType.Ex8200c:
                                                    density = "EXB-8200 compressed";
                                                    break;
                                                case DensityType.Ex8500:
                                                    density = "EXB-8500";
                                                    break;
                                                case DensityType.Ex8500c:
                                                    density = "EXB-8500 compressed";
                                                    break;
                                                case DensityType.Mammoth:
                                                    density = "Mammoth";
                                                    break;
                                                case DensityType.DLT3_42k:
                                                    density = "DLTtape III at 42500 bpi";
                                                    break;
                                                case DensityType.DLT3_56t:
                                                    density = "DLTtape III with 56 tracks";
                                                    break;
                                                case DensityType.DLT3_62k:
                                                case DensityType.DLT3_62kAlt:
                                                    density = "DLTtape III at 62500 bpi";
                                                    break;
                                                case DensityType.DLT3c:
                                                    density = "DLTtape III compressed";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape80m:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Ex8200:
                                                    density = "EXB-8200";
                                                    break;
                                                case DensityType.Ex8200c:
                                                    density = "EXB-8200 compressed";
                                                    break;
                                                case DensityType.Ex8500:
                                                    density = "EXB-8500";
                                                    break;
                                                case DensityType.Ex8500c:
                                                    density = "EXB-8500 compressed";
                                                    break;
                                                case DensityType.Mammoth:
                                                    density = "Mammoth";
                                                    break;
                                                case DensityType.DLT3_62k:
                                                case DensityType.DLT3_62kAlt:
                                                    density = "DLTtape IIIxt";
                                                    break;
                                                case DensityType.DLT3c:
                                                    density = "DLTtape IIIxt compressed";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape106m:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Ex8200:
                                                    density = "EXB-8200";
                                                    break;
                                                case DensityType.Ex8200c:
                                                    density = "EXB-8200 compressed";
                                                    break;
                                                case DensityType.Ex8500:
                                                    density = "EXB-8500";
                                                    break;
                                                case DensityType.Ex8500c:
                                                    density = "EXB-8500 compressed";
                                                    break;
                                                case DensityType.Mammoth:
                                                    density = "Mammoth";
                                                    break;
                                                case DensityType.DLT4:
                                                case DensityType.DLT4Alt:
                                                    density = "DLTtape IV";
                                                    break;
                                                case DensityType.DLT4_123k:
                                                case DensityType.DLT4_123kAlt:
                                                    density = "DLTtape IV at 123090 bpi";
                                                    break;
                                                case DensityType.DLT4_98k:
                                                    density = "DLTtape IV at 98250 bpi";
                                                    break;
                                                case DensityType.Travan5:
                                                    density = "Travan 5";
                                                    break;
                                                case DensityType.DLT4c:
                                                    density = "DLTtape IV compressed";
                                                    break;
                                                case DensityType.DLT4_85k:
                                                    density = "DLTtape IV at 85937 bpi";
                                                    break;
                                                case DensityType.DLT4c_85k:
                                                    density = "DLTtape IV at 85937 bpi compressed";
                                                    break;
                                                case DensityType.DLT4c_123k:
                                                    density = "DLTtape IV at 123090 bpi compressed";
                                                    break;
                                                case DensityType.DLT4c_98k:
                                                    density = "DLTtape IV at 98250 bpi compressed";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape106mXL:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Ex8200:
                                                    density = "EXB-8200";
                                                    break;
                                                case DensityType.Ex8200c:
                                                    density = "EXB-8200 compressed";
                                                    break;
                                                case DensityType.Ex8500:
                                                    density = "EXB-8500";
                                                    break;
                                                case DensityType.Ex8500c:
                                                    density = "EXB-8500 compressed";
                                                    break;
                                                case DensityType.Mammoth:
                                                    density = "Mammoth";
                                                    break;
                                                case DensityType.SDLT1_133k:
                                                case DensityType.SDLT1_133kAlt:
                                                    density = "Super DLTtape I at 133000 bpi";
                                                    break;
                                                case DensityType.SDLT1:
                                                    //case DensityType.SDLT1Alt:
                                                    density = "Super DLTtape I";
                                                    break;
                                                case DensityType.SDLT1c:
                                                    density = "Super DLTtape I compressed";
                                                    break;
                                                /*case DensityType.SDLT1_133kAlt:
                                                    density = "Super DLTtape I at 133000 bpi compressed";
                                                    break;*/
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SDLT2:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.SDLT2:
                                                    density = "Super DLTtape II";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.VStapeI:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.VStape1:
                                                case DensityType.VStape1Alt:
                                                    density = "VStape I";
                                                    break;
                                                case DensityType.VStape1c:
                                                    density = "VStape I compressed";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.DLTtapeS4:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.DLTS4:
                                                    density = "DLTtape S4";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape22m:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Ex8200:
                                                    density = "EXB-8200";
                                                    break;
                                                case DensityType.Ex8200c:
                                                    density = "EXB-8200 compressed";
                                                    break;
                                                case DensityType.Ex8500:
                                                    density = "EXB-8500";
                                                    break;
                                                case DensityType.Ex8500c:
                                                    density = "EXB-8500 compressed";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape40m:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Ex8200:
                                                    density = "EXB-8200";
                                                    break;
                                                case DensityType.Ex8200c:
                                                    density = "EXB-8200 compressed";
                                                    break;
                                                case DensityType.Ex8500:
                                                    density = "EXB-8500";
                                                    break;
                                                case DensityType.Ex8500c:
                                                    density = "EXB-8500 compressed";
                                                    break;
                                                case DensityType.Mammoth:
                                                    density = "Mammoth";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape76m:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Ex8200:
                                                    density = "EXB-8200";
                                                    break;
                                                case DensityType.Ex8200c:
                                                    density = "EXB-8200 compressed";
                                                    break;
                                                case DensityType.Ex8500:
                                                    density = "EXB-8500";
                                                    break;
                                                case DensityType.Ex8500c:
                                                    density = "EXB-8500 compressed";
                                                    break;
                                                case DensityType.Mammoth:
                                                    density = "Mammoth";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape112m:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Ex8200:
                                                    density = "EXB-8200";
                                                    break;
                                                case DensityType.Ex8200c:
                                                    density = "EXB-8200 compressed";
                                                    break;
                                                case DensityType.Ex8500:
                                                    density = "EXB-8500";
                                                    break;
                                                case DensityType.Ex8500c:
                                                    density = "EXB-8500 compressed";
                                                    break;
                                                case DensityType.Mammoth:
                                                    density = "Mammoth";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.Exatape22mAME:
                                    case MediumTypes.Exatape170m:
                                    case MediumTypes.Exatape125m:
                                    case MediumTypes.Exatape45m:
                                    case MediumTypes.Exatape225m:
                                    case MediumTypes.Exatape150m:
                                    case MediumTypes.Exatape75m:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Mammoth:
                                                    density = "Mammoth";
                                                    break;
                                                case DensityType.Mammoth2:
                                                    density = "Mammoth-2";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.DC2900SL:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "DC-2900SL";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.DC9250:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "DC-9250";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLR32:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLR-32";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.MLR1SL:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "MRL1-26GBSL";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLRtape50:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLRtape-50";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLRtape50SL:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLRtape-50 SL";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLR32SL:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLR-32 SL";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLR5:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLR-5";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLR5SL:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLR-5 SL";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLRtape7:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLRtape-7";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLRtape7SL:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLRtape-7 SL";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLRtape24:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLRtape-24";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLRtape24SL:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLRtape-24 SL";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLRtape140:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLRtape-140";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLRtape40:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLRtape-40";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLRtape60:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLRtape-60 or SLRtape-75";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLRtape100:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLRtape-100";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    case MediumTypes.SLR40_60_100:
                                        {
                                            switch(descriptor.Density)
                                            {
                                                case DensityType.Default:
                                                    density = "SLR40, SLR60 or SLR100";
                                                    break;
                                                default:
                                                    density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                                    break;
                                            }
                                        }
                                        break;
                                    default:
                                        density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                        break;
                                }

                                if(density != "")
                                {
                                    if(descriptor.Blocks == 0)
                                    {
                                        if(descriptor.BlockLength == 0)
                                            sb.AppendFormat("\tAll remaining blocks conform to {0} and have a variable length", density).AppendLine();
                                        else
                                            sb.AppendFormat("\tAll remaining blocks conform to {0} and are {1} bytes each", density, descriptor.BlockLength).AppendLine();
                                    }
                                    else
                                    {
                                        if(descriptor.BlockLength == 0)
                                            sb.AppendFormat("\t{0} blocks conform to {1} and have a variable length", descriptor.Blocks, density).AppendLine();
                                        else
                                            sb.AppendFormat("\t{0} blocks conform to {1} and are {2} bytes each", descriptor.Blocks, density, descriptor.BlockLength).AppendLine();
                                    }
                                }
                                else
                                {
                                    if(descriptor.Blocks == 0)
                                    {
                                        if(descriptor.BlockLength == 0)
                                            sb.AppendFormat("\tAll remaining blocks have a variable length").AppendLine();
                                        else
                                            sb.AppendFormat("\tAll remaining blocks are {0} bytes each", descriptor.BlockLength).AppendLine();
                                    }
                                    else
                                    {
                                        if(descriptor.BlockLength == 0)
                                            sb.AppendFormat("\t{0} blocks have a variable length", descriptor.Blocks).AppendLine();
                                        else
                                            sb.AppendFormat("\t{0} blocks are {1} bytes each", descriptor.Blocks, descriptor.BlockLength).AppendLine();
                                    }
                                }
                            }
                        }

                        break;
                    }
                #endregion Sequential access device mode header
                #region Printer device mode header
                case PeripheralDeviceTypes.PrinterDevice:
                    {
                        switch(header.Value.BufferedMode)
                        {
                            case 0:
                                sb.AppendLine("\tDevice prints directly");
                                break;
                            case 1:
                                sb.AppendLine("\tDevice uses a print cache");
                                break;
                            default:
                                sb.AppendFormat("\tUnknown buffered mode code 0x{0:X2}", header.Value.BufferedMode).AppendLine();
                                break;
                        }
                        break;
                    }
                #endregion Printer device mode header
                #region Optical device mode header
                case PeripheralDeviceTypes.OpticalDevice:
                    {
                        if(header.Value.MediumType != MediumTypes.Default)
                        {
                            sb.Append("\tMedium is ");

                            switch(header.Value.MediumType)
                            {
                                case MediumTypes.ReadOnly:
                                    sb.AppendLine("a Read-only optical");
                                    break;
                                case MediumTypes.WORM:
                                    sb.AppendLine("a Write-once Read-many optical");
                                    break;
                                case MediumTypes.Erasable:
                                    sb.AppendLine("a Erasable optical");
                                    break;
                                case MediumTypes.RO_WORM:
                                    sb.AppendLine("a combination of read-only and write-once optical");
                                    break;
                                case MediumTypes.RO_RW:
                                    sb.AppendLine("a combination of read-only and erasable optical");
                                    break;
                                case MediumTypes.WORM_RW:
                                    sb.AppendLine("a combination of write-once and erasable optical");
                                    break;
                                case MediumTypes.DOW:
                                    sb.AppendLine("a direct-overwrite optical");
                                    break;
                                default:
                                    sb.AppendFormat("an unknown medium type 0x{0:X2}", header.Value.MediumType).AppendLine();
                                    break;
                            }
                        }

                        if(header.Value.WriteProtected)
                            sb.AppendLine("\tMedium is write protected");
                        if(header.Value.EBC)
                            sb.AppendLine("\tBlank checking during write is enabled");
                        if(header.Value.DPOFUA)
                            sb.AppendLine("\tDrive supports DPO and FUA bits");

                        if(header.Value.BlockDescriptors != null)
                        {
                            foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                            {
                                string density = "";
                                switch(descriptor.Density)
                                {
                                    case DensityType.Default:
                                        break;
                                    case DensityType.ISO10090:
                                        density = "ISO/IEC 10090: 86 mm Read/Write single-sided optical disc with 12500 tracks";
                                        break;
                                    case DensityType.D581:
                                        density = "89 mm Read/Write double-sided optical disc with 12500 tracks";
                                        break;
                                    case DensityType.X3_212:
                                        density = "ANSI X3.212: 130 mm Read/Write double-sided optical disc with 18750 tracks";
                                        break;
                                    case DensityType.X3_191:
                                        density = "ANSI X3.191: 130 mm Write-Once double-sided optical disc with 30000 tracks";
                                        break;
                                    case DensityType.X3_214:
                                        density = "ANSI X3.214: 130 mm Write-Once double-sided optical disc with 20000 tracks";
                                        break;
                                    case DensityType.X3_211:
                                        density = "ANSI X3.211: 130 mm Write-Once double-sided optical disc with 18750 tracks";
                                        break;
                                    case DensityType.D407:
                                        density = "200 mm optical disc";
                                        break;
                                    case DensityType.ISO13614:
                                        density = "ISO/IEC 13614: 300 mm double-sided optical disc";
                                        break;
                                    case DensityType.X3_200:
                                        density = "ANSI X3.200: 356 mm double-sided optical disc with 56350 tracks";
                                        break;
                                    default:
                                        density = string.Format("unknown density code 0x{0:X2}", (byte)descriptor.Density);
                                        break;
                                }

                                if(density != "")
                                {
                                    if(descriptor.Blocks == 0)
                                    {
                                        if(descriptor.BlockLength == 0)
                                            sb.AppendFormat("\tAll remaining blocks are {0} and have a variable length", density).AppendLine();
                                        else
                                            sb.AppendFormat("\tAll remaining blocks are {0} and are {1} bytes each", density, descriptor.BlockLength).AppendLine();
                                    }
                                    else
                                    {
                                        if(descriptor.BlockLength == 0)
                                            sb.AppendFormat("\t{0} blocks are {1} and have a variable length", descriptor.Blocks, density).AppendLine();
                                        else
                                            sb.AppendFormat("\t{0} blocks are {1} and are {2} bytes each", descriptor.Blocks, density, descriptor.BlockLength).AppendLine();
                                    }
                                }
                                else
                                {
                                    if(descriptor.Blocks == 0)
                                    {
                                        if(descriptor.BlockLength == 0)
                                            sb.AppendFormat("\tAll remaining blocks have a variable length").AppendLine();
                                        else
                                            sb.AppendFormat("\tAll remaining blocks are {0} bytes each", descriptor.BlockLength).AppendLine();
                                    }
                                    else
                                    {
                                        if(descriptor.BlockLength == 0)
                                            sb.AppendFormat("\t{0} blocks have a variable length", descriptor.Blocks).AppendLine();
                                        else
                                            sb.AppendFormat("\t{0} blocks are {1} bytes each", descriptor.Blocks, descriptor.BlockLength).AppendLine();
                                    }
                                }
                            }
                        }

                        break;
                    }
                #endregion Optical device mode header
                #region Multimedia device mode header
                case PeripheralDeviceTypes.MultiMediaDevice:
                    {
                        sb.Append("\tMedium is ");

                        switch(header.Value.MediumType)
                        {
                            case MediumTypes.CDROM:
                                sb.AppendLine("120 mm CD-ROM");
                                break;
                            case MediumTypes.CDDA:
                                sb.AppendLine("120 mm Compact Disc Digital Audio");
                                break;
                            case MediumTypes.MixedCD:
                                sb.AppendLine("120 mm Compact Disc with data and audio");
                                break;
                            case MediumTypes.CDROM_80:
                                sb.AppendLine("80 mm CD-ROM");
                                break;
                            case MediumTypes.CDDA_80:
                                sb.AppendLine("80 mm Compact Disc Digital Audio");
                                break;
                            case MediumTypes.MixedCD_80:
                                sb.AppendLine("80 mm Compact Disc with data and audio");
                                break;
                            case MediumTypes.Unknown_CD:
                                sb.AppendLine("Unknown medium type");
                                break;
                            case MediumTypes.HybridCD:
                                sb.AppendLine("120 mm Hybrid disc (Photo CD)");
                                break;
                            case MediumTypes.Unknown_CDR:
                                sb.AppendLine("Unknown size CD-R");
                                break;
                            case MediumTypes.CDR:
                                sb.AppendLine("120 mm CD-R with data only");
                                break;
                            case MediumTypes.CDR_DA:
                                sb.AppendLine("120 mm CD-R with audio only");
                                break;
                            case MediumTypes.CDR_Mixed:
                                sb.AppendLine("120 mm CD-R with data and audio");
                                break;
                            case MediumTypes.HybridCDR:
                                sb.AppendLine("120 mm Hybrid CD-R (Photo CD)");
                                break;
                            case MediumTypes.CDR_80:
                                sb.AppendLine("80 mm CD-R with data only");
                                break;
                            case MediumTypes.CDR_DA_80:
                                sb.AppendLine("80 mm CD-R with audio only");
                                break;
                            case MediumTypes.CDR_Mixed_80:
                                sb.AppendLine("80 mm CD-R with data and audio");
                                break;
                            case MediumTypes.HybridCDR_80:
                                sb.AppendLine("80 mm Hybrid CD-R (Photo CD)");
                                break;
                            case MediumTypes.Unknown_CDRW:
                                sb.AppendLine("Unknown size CD-RW");
                                break;
                            case MediumTypes.CDRW:
                                sb.AppendLine("120 mm CD-RW with data only");
                                break;
                            case MediumTypes.CDRW_DA:
                                sb.AppendLine("120 mm CD-RW with audio only");
                                break;
                            case MediumTypes.CDRW_Mixed:
                                sb.AppendLine("120 mm CD-RW with data and audio");
                                break;
                            case MediumTypes.HybridCDRW:
                                sb.AppendLine("120 mm Hybrid CD-RW (Photo CD)");
                                break;
                            case MediumTypes.CDRW_80:
                                sb.AppendLine("80 mm CD-RW with data only");
                                break;
                            case MediumTypes.CDRW_DA_80:
                                sb.AppendLine("80 mm CD-RW with audio only");
                                break;
                            case MediumTypes.CDRW_Mixed_80:
                                sb.AppendLine("80 mm CD-RW with data and audio");
                                break;
                            case MediumTypes.HybridCDRW_80:
                                sb.AppendLine("80 mm Hybrid CD-RW (Photo CD)");
                                break;
                            case MediumTypes.Unknown_HD:
                                sb.AppendLine("Unknown size HD disc");
                                break;
                            case MediumTypes.HD:
                                sb.AppendLine("120 mm HD disc");
                                break;
                            case MediumTypes.HD_80:
                                sb.AppendLine("80 mm HD disc");
                                break;
                            case MediumTypes.NoDisc:
                                sb.AppendLine("No disc inserted, tray closed or caddy inserted");
                                break;
                            case MediumTypes.TrayOpen:
                                sb.AppendLine("Tray open or no caddy inserted");
                                break;
                            case MediumTypes.MediumError:
                                sb.AppendLine("Tray closed or caddy inserted but medium error");
                                break;
                            case MediumTypes.UnknownBlockDevice:
                                sb.AppendLine("Unknown block device");
                                break;
                            case MediumTypes.ReadOnlyBlockDevice:
                                sb.AppendLine("Read-only block device");
                                break;
                            case MediumTypes.ReadWriteBlockDevice:
                                sb.AppendLine("Read/Write block device");
                                break;
                            case MediumTypes.LTOCD:
                                sb.AppendLine("LTO in CD-ROM emulation mode");
                                break;
                            default:
                                sb.AppendFormat("Unknown medium type 0x{0:X2}", header.Value.MediumType).AppendLine();
                                break;
                        }

                        if(header.Value.WriteProtected)
                            sb.AppendLine("\tMedium is write protected");

                        if(header.Value.DPOFUA)
                            sb.AppendLine("\tDrive supports DPO and FUA bits");

                        if(header.Value.BlockDescriptors != null)
                        {
                            foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                            {
                                string density = "";
                                switch(descriptor.Density)
                                {
                                    case DensityType.Default:
                                        break;
                                    case DensityType.User:
                                        density = "user data only";
                                        break;
                                    case DensityType.UserAuxiliary:
                                        density = "user data plus auxiliary data";
                                        break;
                                    case DensityType.UserAuxiliaryTag:
                                        density = "4-byte tag, user data plus auxiliary data";
                                        break;
                                    case DensityType.Audio:
                                        density = "audio information only";
                                        break;
                                    case DensityType.LTO2:
                                        density = "LTO Ultrium-2";
                                        break;
                                    case DensityType.LTO3:
                                        density = "LTO Ultrium-3";
                                        break;
                                    case DensityType.LTO4:
                                        density = "LTO Ultrium-4";
                                        break;
                                    case DensityType.LTO5:
                                        density = "LTO Ultrium-5";
                                        break;
                                    default:
                                        density = string.Format("with unknown density code 0x{0:X2}", descriptor.Density);
                                        break;
                                }

                                if(density != "")
                                {
                                    if(descriptor.Blocks == 0)
                                        sb.AppendFormat("\tAll remaining blocks have {0} and are {1} bytes each", density, descriptor.BlockLength).AppendLine();
                                    else
                                        sb.AppendFormat("\t{0} blocks have {1} and are {2} bytes each", descriptor.Blocks, density, descriptor.BlockLength).AppendLine();
                                }
                                else
                                {
                                    if(descriptor.Blocks == 0)
                                        sb.AppendFormat("\tAll remaining blocks are {0} bytes each", descriptor.BlockLength).AppendLine();
                                    else
                                        sb.AppendFormat("\t{0} blocks are {1} bytes each", descriptor.Blocks, descriptor.BlockLength).AppendLine();
                                }
                            }
                        }

                        break;
                    }
                    #endregion Multimedia device mode header
            }

            return sb.ToString();
        }

        public static ModeHeader? DecodeModeHeader10(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            if(modeResponse == null || modeResponse.Length < 8)
                return null;

            ushort modeLength;
            ushort blockDescLength;

            modeLength = (ushort)((modeResponse[0] << 8) + modeResponse[1]);
            blockDescLength = (ushort)((modeResponse[6] << 8) + modeResponse[7]);

            if(modeResponse.Length < modeLength)
                return null;

            ModeHeader header = new ModeHeader();
            header.MediumType = (MediumTypes)modeResponse[2];

            bool longLBA = (modeResponse[4] & 0x01) == 0x01;

            if(blockDescLength > 0)
            {
                if(longLBA)
                {
                    header.BlockDescriptors = new BlockDescriptor[blockDescLength / 16];
                    for(int i = 0; i < header.BlockDescriptors.Length; i++)
                    {
                        header.BlockDescriptors[i] = new BlockDescriptor();
                        header.BlockDescriptors[i].Density = DensityType.Default;
                        byte[] temp = new byte[8];
                        temp[0] = modeResponse[7 + i * 16 + 8];
                        temp[1] = modeResponse[6 + i * 16 + 8];
                        temp[2] = modeResponse[5 + i * 16 + 8];
                        temp[3] = modeResponse[4 + i * 16 + 8];
                        temp[4] = modeResponse[3 + i * 16 + 8];
                        temp[5] = modeResponse[2 + i * 16 + 8];
                        temp[6] = modeResponse[1 + i * 16 + 8];
                        temp[7] = modeResponse[0 + i * 16 + 8];
                        header.BlockDescriptors[i].Blocks = BitConverter.ToUInt64(temp, 0);
                        header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[15 + i * 16 + 8] << 24);
                        header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[14 + i * 16 + 8] << 16);
                        header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[13 + i * 16 + 8] << 8);
                        header.BlockDescriptors[i].BlockLength += modeResponse[12 + i * 16 + 8];
                    }
                }
                else
                {
                    header.BlockDescriptors = new BlockDescriptor[blockDescLength / 8];
                    for(int i = 0; i < header.BlockDescriptors.Length; i++)
                    {
                        header.BlockDescriptors[i] = new BlockDescriptor();
                        if(deviceType != PeripheralDeviceTypes.DirectAccess)
                        {
                            header.BlockDescriptors[i].Density = (DensityType)modeResponse[0 + i * 8 + 8];
                        }
                        else
                        {
                            header.BlockDescriptors[i].Density = DensityType.Default;
                            header.BlockDescriptors[i].Blocks += (ulong)(modeResponse[0 + i * 8 + 8] << 24);
                        }
                        header.BlockDescriptors[i].Blocks += (ulong)(modeResponse[1 + i * 8 + 8] << 16);
                        header.BlockDescriptors[i].Blocks += (ulong)(modeResponse[2 + i * 8 + 8] << 8);
                        header.BlockDescriptors[i].Blocks += modeResponse[3 + i * 8 + 8];
                        header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[5 + i * 8 + 8] << 16);
                        header.BlockDescriptors[i].BlockLength += (uint)(modeResponse[6 + i * 8 + 8] << 8);
                        header.BlockDescriptors[i].BlockLength += modeResponse[7 + i * 8 + 8];
                    }
                }
            }

            if(deviceType == PeripheralDeviceTypes.DirectAccess || deviceType == PeripheralDeviceTypes.MultiMediaDevice)
            {
                header.WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
                header.DPOFUA = ((modeResponse[3] & 0x10) == 0x10);
            }

            if(deviceType == PeripheralDeviceTypes.SequentialAccess)
            {
                header.WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
                header.Speed = (byte)(modeResponse[3] & 0x0F);
                header.BufferedMode = (byte)((modeResponse[3] & 0x70) >> 4);
            }

            if(deviceType == PeripheralDeviceTypes.PrinterDevice)
                header.BufferedMode = (byte)((modeResponse[3] & 0x70) >> 4);

            if(deviceType == PeripheralDeviceTypes.OpticalDevice)
            {
                header.WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
                header.EBC = ((modeResponse[3] & 0x01) == 0x01);
                header.DPOFUA = ((modeResponse[3] & 0x10) == 0x10);
            }

            return header;
        }

        public static string PrettifyModeHeader10(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            return PrettifyModeHeader(DecodeModeHeader10(modeResponse, deviceType), deviceType);
        }

        #region Mode Page 0x0A: Control mode page

        /// <summary>
        /// Control mode page
        /// Page code 0x0A
        /// 8 bytes in SCSI-2
        /// 12 bytes in SPC-1, SPC-2, SPC-3, SPC-4, SPC-5
        /// </summary>
        public struct ModePage_0A
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// If set, target shall report log exception conditions
            /// </summary>
            public bool RLEC;
            /// <summary>
            /// Queue algorithm modifier
            /// </summary>
            public byte QueueAlgorithm;
            /// <summary>
            /// If set all remaining suspended I/O processes shall be aborted after the contingent allegiance condition or extended contingent allegiance condition
            /// </summary>
            public byte QErr;
            /// <summary>
            /// Tagged queuing is disabled
            /// </summary>
            public bool DQue;
            /// <summary>
            /// Extended Contingent Allegiance is enabled
            /// </summary>
            public bool EECA;
            /// <summary>
            /// Target may issue an asynchronous event notification upon completing its initialization
            /// </summary>
            public bool RAENP;
            /// <summary>
            /// Target may issue an asynchronous event notification instead of a unit attention condition
            /// </summary>
            public bool UAAENP;
            /// <summary>
            /// Target may issue an asynchronous event notification instead of a deferred error
            /// </summary>
            public bool EAENP;
            /// <summary>
            /// Minimum time in ms after initialization before attempting asynchronous event notifications
            /// </summary>
            public ushort ReadyAENHoldOffPeriod;

            /// <summary>
            /// Global logging target save disabled
            /// </summary>
            public bool GLTSD;
            /// <summary>
            /// CHECK CONDITION should be reported rather than a long busy condition
            /// </summary>
            public bool RAC;
            /// <summary>
            /// Software write protect is active
            /// </summary>
            public bool SWP;
            /// <summary>
            /// Maximum time in 100 ms units allowed to remain busy. 0xFFFF == unlimited.
            /// </summary>
            public ushort BusyTimeoutPeriod;

            /// <summary>
            /// Task set type
            /// </summary>
            public byte TST;
            /// <summary>
            /// Tasks aborted by other initiator's actions should be terminated with TASK ABORTED
            /// </summary>
            public bool TAS;
            /// <summary>
            /// Action to be taken when a medium is inserted
            /// </summary>
            public byte AutoloadMode;
            /// <summary>
            /// Time in seconds to complete an extended self-test
            /// </summary>
            public byte ExtendedSelfTestCompletionTime;

            /// <summary>
            /// All tasks received in nexus with ACA ACTIVE is set and an ACA condition is established shall terminate
            /// </summary>
            public bool TMF_ONLY;
            /// <summary>
            /// Device shall return descriptor format sense data when returning sense data in the same transactions as a CHECK CONDITION
            /// </summary>
            public bool D_SENSE;
            /// <summary>
            /// Unit attention interlocks control
            /// </summary>
            public byte UA_INTLCK_CTRL;
            /// <summary>
            /// LOGICAL BLOCK APPLICATION TAG should not be modified
            /// </summary>
            public bool ATO;

            /// <summary>
            /// Protector information checking is disabled
            /// </summary>
            public bool DPICZ;
            /// <summary>
            /// No unit attention on release
            /// </summary>
            public bool NUAR;
            /// <summary>
            /// Application Tag mode page is enabled
            /// </summary>
            public bool ATMPE;
            /// <summary>
            /// Abort any write command without protection information
            /// </summary>
            public bool RWWP;
            /// <summary>
            /// Supportes block lengths and protection information
            /// </summary>
            public bool SBLP;
        }

        public static ModePage_0A? DecodeModePage_0A(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x0A)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            ModePage_0A decoded = new ModePage_0A();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.RLEC |= (pageResponse[2] & 0x01) == 0x01;

            decoded.QueueAlgorithm = (byte)((pageResponse[3] & 0xF0) >> 4);
            decoded.QErr = (byte)((pageResponse[3] & 0x06) >> 1);

            decoded.DQue |= (pageResponse[3] & 0x01) == 0x01;
            decoded.EECA |= (pageResponse[4] & 0x80) == 0x80;
            decoded.RAENP |= (pageResponse[4] & 0x04) == 0x04;
            decoded.UAAENP |= (pageResponse[4] & 0x02) == 0x02;
            decoded.EAENP |= (pageResponse[4] & 0x01) == 0x01;

            decoded.ReadyAENHoldOffPeriod = (ushort)((pageResponse[6] << 8) + pageResponse[7]);

            if(pageResponse.Length < 10)
                return decoded;

            // SPC-1
            decoded.GLTSD |= (pageResponse[2] & 0x02) == 0x02;
            decoded.RAC |= (pageResponse[4] & 0x40) == 0x40;
            decoded.SWP |= (pageResponse[4] & 0x08) == 0x08;

            decoded.BusyTimeoutPeriod = (ushort)((pageResponse[8] << 8) + pageResponse[9]);

            // SPC-2
            decoded.TST = (byte)((pageResponse[2] & 0xE0) >> 5);
            decoded.TAS |= (pageResponse[4] & 0x80) == 0x80;
            decoded.AutoloadMode = (byte)(pageResponse[5] & 0x07);
            decoded.BusyTimeoutPeriod = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

            // SPC-3
            decoded.TMF_ONLY |= (pageResponse[2] & 0x10) == 0x10;
            decoded.D_SENSE |= (pageResponse[2] & 0x04) == 0x04;
            decoded.UA_INTLCK_CTRL = (byte)((pageResponse[4] & 0x30) >> 4);
            decoded.TAS |= (pageResponse[5] & 0x40) == 0x40;
            decoded.ATO |= (pageResponse[5] & 0x80) == 0x80;

            // SPC-5
            decoded.DPICZ |= (pageResponse[2] & 0x08) == 0x08;
            decoded.NUAR |= (pageResponse[3] & 0x08) == 0x08;
            decoded.ATMPE |= (pageResponse[5] & 0x20) == 0x20;
            decoded.RWWP |= (pageResponse[5] & 0x10) == 0x10;
            decoded.SBLP |= (pageResponse[5] & 0x08) == 0x08;

            return decoded;
        }

        public static string PrettifyModePage_0A(byte[] pageResponse)
        {
            return PrettifyModePage_0A(DecodeModePage_0A(pageResponse));
        }

        public static string PrettifyModePage_0A(ModePage_0A? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_0A page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Control mode page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");
            if(page.RLEC)
                sb.AppendLine("\tIf set, target shall report log exception conditions");
            if(page.DQue)
                sb.AppendLine("\tTagged queuing is disabled");
            if(page.EECA)
                sb.AppendLine("\tExtended Contingent Allegiance is enabled");
            if(page.RAENP)
                sb.AppendLine("\tTarget may issue an asynchronous event notification upon completing its initialization");
            if(page.UAAENP)
                sb.AppendLine("\tTarget may issue an asynchronous event notification instead of a unit attention condition");
            if(page.EAENP)
                sb.AppendLine("\tTarget may issue an asynchronous event notification instead of a deferred error");
            if(page.GLTSD)
                sb.AppendLine("\tGlobal logging target save disabled");
            if(page.RAC)
                sb.AppendLine("\tCHECK CONDITION should be reported rather than a long busy condition");
            if(page.SWP)
                sb.AppendLine("\tSoftware write protect is active");
            if(page.TAS)
                sb.AppendLine("\tTasks aborted by other initiator's actions should be terminated with TASK ABORTED");
            if(page.TMF_ONLY)
                sb.AppendLine("\tAll tasks received in nexus with ACA ACTIVE is set and an ACA condition is established shall terminate");
            if(page.D_SENSE)
                sb.AppendLine("\tDevice shall return descriptor format sense data when returning sense data in the same transactions as a CHECK CONDITION");
            if(page.ATO)
                sb.AppendLine("\tLOGICAL BLOCK APPLICATION TAG should not be modified");
            if(page.DPICZ)
                sb.AppendLine("\tProtector information checking is disabled");
            if(page.NUAR)
                sb.AppendLine("\tNo unit attention on release");
            if(page.ATMPE)
                sb.AppendLine("\tApplication Tag mode page is enabled");
            if(page.RWWP)
                sb.AppendLine("\tAbort any write command without protection information");
            if(page.SBLP)
                sb.AppendLine("\tSupportes block lengths and protection information");

            switch(page.TST)
            {
                case 0:
                    sb.AppendLine("\tThe logical unit maintains one task set for all nexuses");
                    break;
                case 1:
                    sb.AppendLine("\tThe logical unit maintains separate task sets for each nexus");
                    break;
                default:
                    sb.AppendFormat("\tUnknown Task set type {0}", page.TST).AppendLine();
                    break;
            }

            switch(page.QueueAlgorithm)
            {
                case 0:
                    sb.AppendLine("\tCommands should be sent strictly ordered");
                    break;
                case 1:
                    sb.AppendLine("\tCommands can be reordered in any manner");
                    break;
                default:
                    sb.AppendFormat("\tUnknown Queue Algorithm Modifier {0}", page.QueueAlgorithm).AppendLine();
                    break;
            }

            switch(page.QErr)
            {
                case 0:
                    sb.AppendLine("\tIf ACA is established, the task set commands shall resume after it is cleared, otherwise they shall terminate with CHECK CONDITION");
                    break;
                case 1:
                    sb.AppendLine("\tAll the affected commands in the task set shall be aborted when CHECK CONDITION is returned");
                    break;
                case 3:
                    sb.AppendLine("\tAffected commands in the task set belonging with the CHECK CONDITION nexus shall be aborted");
                    break;
                default:
                    sb.AppendLine("\tReserved QErr value 2 is set");
                    break;
            }

            switch(page.UA_INTLCK_CTRL)
            {
                case 0:
                    sb.AppendLine("\tLUN shall clear unit attention condition reported in the same nexus");
                    break;
                case 2:
                    sb.AppendLine("\tLUN shall not clear unit attention condition reported in the same nexus");
                    break;
                case 3:
                    sb.AppendLine("\tLUN shall not clear unit attention condition reported in the same nexus and shall establish a unit attention condition for the initiator");
                    break;
                default:
                    sb.AppendLine("\tReserved UA_INTLCK_CTRL value 1 is set");
                    break;
            }

            switch(page.AutoloadMode)
            {
                case 0:
                    sb.AppendLine("\tOn medium insertion, it shall be loaded for full access");
                    break;
                case 1:
                    sb.AppendLine("\tOn medium insertion, it shall be loaded for auxiliary memory access only");
                    break;
                case 2:
                    sb.AppendLine("\tOn medium insertion, it shall not be loaded");
                    break;
                default:
                    sb.AppendFormat("\tReserved autoload mode {0} set", page.AutoloadMode).AppendLine();
                    break;
            }

            if(page.ReadyAENHoldOffPeriod > 0)
                sb.AppendFormat("\t{0} ms before attempting asynchronous event notifications after initialization", page.ReadyAENHoldOffPeriod).AppendLine();

            if(page.BusyTimeoutPeriod > 0)
            {
                if(page.BusyTimeoutPeriod == 0xFFFF)
                    sb.AppendLine("\tThere is no limit on the maximum time that is allowed to remain busy");
                else
                    sb.AppendFormat("\tA maximum of {0} ms are allowed to remain busy", page.BusyTimeoutPeriod * 100).AppendLine();
            }

            if(page.ExtendedSelfTestCompletionTime > 0)
                sb.AppendFormat("\t{0} seconds to complete extended self-test", page.ExtendedSelfTestCompletionTime);

            return sb.ToString();
        }

        #endregion Mode Page 0x0A: Control mode page

        #region Mode Page 0x02: Disconnect-reconnect page

        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x02
        /// 16 bytes in SCSI-2, SPC-1, SPC-2, SPC-3, SPC-4, SPC-5
        /// </summary>
        public struct ModePage_02
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// How full should be the buffer prior to attempting a reselection
            /// </summary>
            public byte BufferFullRatio;
            /// <summary>
            /// How empty should be the buffer prior to attempting a reselection
            /// </summary>
            public byte BufferEmptyRatio;
            /// <summary>
            /// Max. time in 100 µs increments that the target is permitted to assert BSY without a REQ/ACK
            /// </summary>
            public ushort BusInactivityLimit;
            /// <summary>
            /// Min. time in 100 µs increments to wait after releasing the bus before attempting reselection
            /// </summary>
            public ushort DisconnectTimeLimit;
            /// <summary>
            /// Max. time in 100 µs increments allowed to use the bus before disconnecting, if granted the privilege and not restricted by <see cref="DTDC"/> 
            /// </summary>
            public ushort ConnectTimeLimit;
            /// <summary>
            /// Maximum amount of data before disconnecting in 512 bytes increments
            /// </summary>
            public ushort MaxBurstSize;
            /// <summary>
            /// Data transfer disconnect control
            /// </summary>
            public byte DTDC;

            /// <summary>
            /// Target shall not transfer data for a command during the same interconnect tenancy
            /// </summary>
            public bool DIMM;
            /// <summary>
            /// Wether to use fair or unfair arbitration when requesting an interconnect tenancy
            /// </summary>
            public byte FairArbitration;
            /// <summary>
            /// Max. ammount of data in 512 bytes increments that may be transferred for a command along with the command
            /// </summary>
            public ushort FirstBurstSize;
            /// <summary>
            /// Target is allowed to re-order the data transfer
            /// </summary>
            public bool EMDP;
        }

        public static ModePage_02? DecodeModePage_02(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x02)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 12)
                return null;

            ModePage_02 decoded = new ModePage_02();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.BufferFullRatio = pageResponse[2];
            decoded.BufferEmptyRatio = pageResponse[3];
            decoded.BusInactivityLimit = (ushort)((pageResponse[4] << 8) + pageResponse[5]);
            decoded.DisconnectTimeLimit = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.ConnectTimeLimit = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.MaxBurstSize = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

            if(pageResponse.Length >= 13)
            {
                decoded.EMDP |= (pageResponse[12] & 0x80) == 0x80;
                decoded.DIMM |= (pageResponse[12] & 0x08) == 0x08;
                decoded.FairArbitration = (byte)((pageResponse[12] & 0x70) >> 4);
                decoded.DTDC = (byte)(pageResponse[12] & 0x07);
            }

            if(pageResponse.Length >= 16)
                decoded.FirstBurstSize = (ushort)((pageResponse[14] << 8) + pageResponse[15]);

            return decoded;
        }

        public static string PrettifyModePage_02(byte[] pageResponse)
        {
            return PrettifyModePage_02(DecodeModePage_02(pageResponse));
        }

        public static string PrettifyModePage_02(ModePage_02? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_02 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Disconnect-Reconnect mode page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");
            if(page.BufferFullRatio > 0)
                sb.AppendFormat("\t{0} ratio of buffer that shall be full prior to attempting a reselection", page.BufferFullRatio).AppendLine();
            if(page.BufferEmptyRatio > 0)
                sb.AppendFormat("\t{0} ratio of buffer that shall be empty prior to attempting a reselection", page.BufferEmptyRatio).AppendLine();
            if(page.BusInactivityLimit > 0)
                sb.AppendFormat("\t{0} µs maximum permitted to assert BSY without a REQ/ACK handshake", page.BusInactivityLimit * 100).AppendLine();
            if(page.DisconnectTimeLimit > 0)
                sb.AppendFormat("\t{0} µs maximum permitted wait after releasing the bus before attempting reselection", page.DisconnectTimeLimit * 100).AppendLine();
            if(page.ConnectTimeLimit > 0)
                sb.AppendFormat("\t{0} µs allowed to use the bus before disconnecting, if granted the privilege and not restricted", page.ConnectTimeLimit * 100).AppendLine();
            if(page.MaxBurstSize > 0)
                sb.AppendFormat("\t{0} bytes maximum can be transferred before disconnecting", page.MaxBurstSize * 512).AppendLine();
            if(page.FirstBurstSize > 0)
                sb.AppendFormat("\t{0} bytes maximum can be transferred for a command along with the disconnect command", page.FirstBurstSize * 512).AppendLine();

            if(page.DIMM)
                sb.AppendLine("\tTarget shall not transfer data for a command during the same interconnect tenancy");
            if(page.EMDP)
                sb.AppendLine("\tTarget is allowed to re-order the data transfer");

            switch(page.DTDC)
            {
                case 0:
                    sb.AppendLine("\tData transfer disconnect control is not used");
                    break;
                case 1:
                    sb.AppendLine("\tAll data for a command shall be transferred within a single interconnect tenancy");
                    break;
                case 3:
                    sb.AppendLine("\tAll data and the response for a command shall be transferred within a single interconnect tenancy");
                    break;
                default:
                    sb.AppendFormat("\tReserved data transfer disconnect control value {0}", page.DTDC).AppendLine();
                    break;
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x02: Disconnect-reconnect page

        #region Mode Page 0x08: Caching page

        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x08
        /// 12 bytes in SCSI-2
        /// 20 bytes in SBC-1, SBC-2, SBC-3
        /// </summary>
        public struct ModePage_08
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// <c>true</c> if write cache is enabled
            /// </summary>
            public bool WCE;
            /// <summary>
            /// Multiplication factor
            /// </summary>
            public bool MF;
            /// <summary>
            /// <c>true</c> if read cache is enabled
            /// </summary>
            public bool RCD;
            /// <summary>
            /// Advices on reading-cache retention priority
            /// </summary>
            public byte DemandReadRetentionPrio;
            /// <summary>
            /// Advices on writing-cache retention priority
            /// </summary>
            public byte WriteRetentionPriority;
            /// <summary>
            /// If requested read blocks are more than this, no pre-fetch is done
            /// </summary>
            public ushort DisablePreFetch;
            /// <summary>
            /// Minimum pre-fetch
            /// </summary>
            public ushort MinimumPreFetch;
            /// <summary>
            /// Maximum pre-fetch
            /// </summary>
            public ushort MaximumPreFetch;
            /// <summary>
            /// Upper limit on maximum pre-fetch value
            /// </summary>
            public ushort MaximumPreFetchCeiling;

            /// <summary>
            /// Manual cache controlling
            /// </summary>
            public bool IC;
            /// <summary>
            /// Abort pre-fetch
            /// </summary>
            public bool ABPF;
            /// <summary>
            /// Caching analysis permitted
            /// </summary>
            public bool CAP;
            /// <summary>
            /// Pre-fetch over discontinuities
            /// </summary>
            public bool Disc;
            /// <summary>
            /// <see cref="CacheSegmentSize"/> is to be used to control caching segmentation
            /// </summary>
            public bool Size;
            /// <summary>
            /// Force sequential write
            /// </summary>
            public bool FSW;
            /// <summary>
            /// Logical block cache segment size
            /// </summary>
            public bool LBCSS;
            /// <summary>
            /// Disable read-ahead
            /// </summary>
            public bool DRA;
            /// <summary>
            /// How many segments should the cache be divided upon
            /// </summary>
            public byte CacheSegments;
            /// <summary>
            /// How many bytes should the cache be divided upon
            /// </summary>
            public ushort CacheSegmentSize;
            /// <summary>
            /// How many bytes should be used as a buffer when all other cached data cannot be evicted
            /// </summary>
            public uint NonCacheSegmentSize;

            public bool NV_DIS;
        }

        public static ModePage_08? DecodeModePage_08(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x08)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 12)
                return null;

            ModePage_08 decoded = new ModePage_08();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.WCE |= (pageResponse[2] & 0x04) == 0x04;
            decoded.MF |= (pageResponse[2] & 0x02) == 0x02;
            decoded.RCD |= (pageResponse[2] & 0x01) == 0x01;

            decoded.DemandReadRetentionPrio = (byte)((pageResponse[3] & 0xF0) >> 4);
            decoded.WriteRetentionPriority = (byte)(pageResponse[3] & 0x0F);
            decoded.DisablePreFetch = (ushort)((pageResponse[4] << 8) + pageResponse[5]);
            decoded.MinimumPreFetch = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.MaximumPreFetch = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.MaximumPreFetchCeiling = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

            if(pageResponse.Length < 20)
                return decoded;

            decoded.IC |= (pageResponse[2] & 0x80) == 0x80;
            decoded.ABPF |= (pageResponse[2] & 0x40) == 0x40;
            decoded.CAP |= (pageResponse[2] & 0x20) == 0x20;
            decoded.Disc |= (pageResponse[2] & 0x10) == 0x10;
            decoded.Size |= (pageResponse[2] & 0x08) == 0x08;

            decoded.FSW |= (pageResponse[12] & 0x80) == 0x80;
            decoded.LBCSS |= (pageResponse[12] & 0x40) == 0x40;
            decoded.DRA |= (pageResponse[12] & 0x20) == 0x20;

            decoded.CacheSegments = pageResponse[13];
            decoded.CacheSegmentSize = (ushort)((pageResponse[14] << 8) + pageResponse[15]);
            decoded.NonCacheSegmentSize = (uint)((pageResponse[17] << 16) + (pageResponse[18] << 8) + pageResponse[19]);

            decoded.NV_DIS |= (pageResponse[12] & 0x01) == 0x01;

            return decoded;
        }

        public static string PrettifyModePage_08(byte[] pageResponse)
        {
            return PrettifyModePage_08(DecodeModePage_08(pageResponse));
        }

        public static string PrettifyModePage_08(ModePage_08? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_08 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Caching mode page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");
            if(page.RCD)
                sb.AppendLine("\tRead-cache is enabled");
            if(page.WCE)
                sb.AppendLine("\tWrite-cache is enabled");

            switch(page.DemandReadRetentionPrio)
            {
                case 0:
                    sb.AppendLine("\tDrive does not distinguish between cached read data");
                    break;
                case 1:
                    sb.AppendLine("\tData put by READ commands should be evicted from cache sooner than data put in read cache by other means");
                    break;
                case 0xF:
                    sb.AppendLine("\tData put by READ commands should not be evicted if there is data cached by other means that can be evicted");
                    break;
                default:
                    sb.AppendFormat("\tUnknown demand read retention priority value {0}", page.DemandReadRetentionPrio).AppendLine();
                    break;
            }

            switch(page.WriteRetentionPriority)
            {
                case 0:
                    sb.AppendLine("\tDrive does not distinguish between cached write data");
                    break;
                case 1:
                    sb.AppendLine("\tData put by WRITE commands should be evicted from cache sooner than data put in write cache by other means");
                    break;
                case 0xF:
                    sb.AppendLine("\tData put by WRITE commands should not be evicted if there is data cached by other means that can be evicted");
                    break;
                default:
                    sb.AppendFormat("\tUnknown demand write retention priority value {0}", page.DemandReadRetentionPrio).AppendLine();
                    break;
            }

            if(page.DRA)
                sb.AppendLine("\tRead-ahead is disabled");
            else
            {
                if(page.MF)
                    sb.AppendLine("\tPre-fetch values indicate a block multiplier");

                if(page.DisablePreFetch == 0)
                    sb.AppendLine("\tNo pre-fetch will be done");
                else
                {
                    sb.AppendFormat("\tPre-fetch will be done for READ commands of {0} blocks or less", page.DisablePreFetch).AppendLine();

                    if(page.MinimumPreFetch > 0)
                        sb.AppendFormat("At least {0} blocks will be always pre-fetched", page.MinimumPreFetch).AppendLine();
                    if(page.MaximumPreFetch > 0)
                        sb.AppendFormat("\tA maximum of {0} blocks will be pre-fetched", page.MaximumPreFetch).AppendLine();
                    if(page.MaximumPreFetchCeiling > 0)
                        sb.AppendFormat("\tA maximum of {0} blocks will be pre-fetched even if it is commanded to pre-fetch more", page.MaximumPreFetchCeiling).AppendLine();

                    if(page.IC)
                        sb.AppendLine("\tDevice should use number of cache segments or cache segment size for caching");
                    if(page.ABPF)
                        sb.AppendLine("\tPre-fetch should be aborted upong receiving a new command");
                    if(page.CAP)
                        sb.AppendLine("\tCaching analysis is permitted");
                    if(page.Disc)
                        sb.AppendLine("\tPre-fetch can continue across discontinuities (such as cylinders or tracks)");
                }
            }

            if(page.FSW)
                sb.AppendLine("\tDrive should not reorder the sequence of write commands to be faster");

            if(page.Size)
            {
                if(page.CacheSegmentSize > 0)
                {
                    if(page.LBCSS)
                        sb.AppendFormat("\tDrive cache segments should be {0} blocks long", page.CacheSegmentSize).AppendLine();
                    else
                        sb.AppendFormat("\tDrive cache segments should be {0} bytes long", page.CacheSegmentSize).AppendLine();
                }
            }
            else
            {
                if(page.CacheSegments > 0)
                    sb.AppendFormat("\tDrive should have {0} cache segments", page.CacheSegments).AppendLine();
            }

            if(page.NonCacheSegmentSize > 0)
                sb.AppendFormat("\tDrive shall allocate {0} bytes to buffer even when all cached data cannot be evicted", page.NonCacheSegmentSize).AppendLine();

            if(page.NV_DIS)
                sb.AppendLine("\tNon-Volatile cache is disabled");

            return sb.ToString();
        }

        #endregion Mode Page 0x08: Caching page

        #region Mode Page 0x05: Flexible disk page

        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x05
        /// 32 bytes in SCSI-2, SBC-1
        /// </summary>
        public struct ModePage_05
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Data rate of peripheral device on kbit/s
            /// </summary>
            public ushort TransferRate;
            /// <summary>
            /// Heads for reading and/or writing
            /// </summary>
            public byte Heads;
            /// <summary>
            /// Sectors per revolution per head
            /// </summary>
            public byte SectorsPerTrack;
            /// <summary>
            /// Bytes of data per sector
            /// </summary>
            public ushort BytesPerSector;
            /// <summary>
            /// Cylinders used for data storage
            /// </summary>
            public ushort Cylinders;
            /// <summary>
            /// Cylinder where write precompensation starts
            /// </summary>
            public ushort WritePrecompCylinder;
            /// <summary>
            /// Cylinder where write current reduction starts
            /// </summary>
            public ushort WriteReduceCylinder;
            /// <summary>
            /// Step rate in 100 μs units
            /// </summary>
            public ushort DriveStepRate;
            /// <summary>
            /// Width of step pulse in μs
            /// </summary>
            public byte DriveStepPulse;
            /// <summary>
            /// Head settle time in 100 μs units
            /// </summary>
            public ushort HeadSettleDelay;
            /// <summary>
            /// If <see cref="TRDY"/> is <c>true</c>, specified in 1/10s of a
            /// second the time waiting for read status before aborting medium
            /// access. Otherwise, indicates time to way before medimum access
            /// after motor on signal is asserted.
            /// </summary>
            public byte MotorOnDelay;
            /// <summary>
            /// Time in 1/10s of a second to wait before releasing the motor on
            /// signal after an idle condition. 0xFF means to never release the
            /// signal
            /// </summary>
            public byte MotorOffDelay;
            /// <summary>
            /// Specifies if a signal indicates that the medium is ready to be accessed
            /// </summary>
            public bool TRDY;
            /// <summary>
            /// If <c>true</c> sectors start with one. Otherwise, they start with zero.
            /// </summary>
            public bool SSN;
            /// <summary>
            /// If <c>true</c> specifies that motor on shall remain released.
            /// </summary>
            public bool MO;
            /// <summary>
            /// Number of additional step pulses per cylinder.
            /// </summary>
            public byte SPC;
            /// <summary>
            /// Write compensation value
            /// </summary>
            public byte WriteCompensation;
            /// <summary>
            /// Head loading time in ms.
            /// </summary>
            public byte HeadLoadDelay;
            /// <summary>
            /// Head unloading time in ms.
            /// </summary>
            public byte HeadUnloadDelay;
            /// <summary>
            /// Description of shugart's bus pin 34 usage
            /// </summary>
            public byte Pin34;
            /// <summary>
            /// Description of shugart's bus pin 2 usage
            /// </summary>
            public byte Pin2;
            /// <summary>
            /// Description of shugart's bus pin 4 usage
            /// </summary>
            public byte Pin4;
            /// <summary>
            /// Description of shugart's bus pin 1 usage
            /// </summary>
            public byte Pin1;
            /// <summary>
            /// Medium speed in rpm
            /// </summary>
            public ushort MediumRotationRate;
        }

        public static ModePage_05? DecodeModePage_05(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x05)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 32)
                return null;

            ModePage_05 decoded = new ModePage_05();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.TransferRate = (ushort)((pageResponse[2] << 8) + pageResponse[3]);
            decoded.Heads = pageResponse[4];
            decoded.SectorsPerTrack = pageResponse[5];
            decoded.BytesPerSector = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.Cylinders = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.WritePrecompCylinder = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.WriteReduceCylinder = (ushort)((pageResponse[12] << 8) + pageResponse[13]);
            decoded.DriveStepRate = (ushort)((pageResponse[14] << 8) + pageResponse[15]);
            decoded.DriveStepPulse = pageResponse[16];
            decoded.HeadSettleDelay = (ushort)((pageResponse[17] << 8) + pageResponse[18]);
            decoded.MotorOnDelay = pageResponse[19];
            decoded.MotorOffDelay = pageResponse[20];
            decoded.TRDY |= (pageResponse[21] & 0x80) == 0x80;
            decoded.SSN |= (pageResponse[21] & 0x40) == 0x40;
            decoded.MO |= (pageResponse[21] & 0x20) == 0x20;
            decoded.SPC = (byte)(pageResponse[22] & 0x0F);
            decoded.WriteCompensation = pageResponse[23];
            decoded.HeadLoadDelay = pageResponse[24];
            decoded.HeadUnloadDelay = pageResponse[25];
            decoded.Pin34 = (byte)((pageResponse[26] & 0xF0) >> 4);
            decoded.Pin2 = (byte)(pageResponse[26] & 0x0F);
            decoded.Pin4 = (byte)((pageResponse[27] & 0xF0) >> 4);
            decoded.Pin1 = (byte)(pageResponse[27] & 0x0F);
            decoded.MediumRotationRate = (ushort)((pageResponse[28] << 8) + pageResponse[29]);

            return decoded;
        }

        public static string PrettifyModePage_05(byte[] pageResponse)
        {
            return PrettifyModePage_05(DecodeModePage_05(pageResponse));
        }

        public static string PrettifyModePage_05(ModePage_05? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_05 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Flexible disk page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\tTransfer rate: {0} kbit/s", page.TransferRate).AppendLine();
            sb.AppendFormat("\t{0} heads", page.Heads).AppendLine();
            sb.AppendFormat("\t{0} cylinders", page.Cylinders).AppendLine();
            sb.AppendFormat("\t{0} sectors per track", page.SectorsPerTrack).AppendLine();
            sb.AppendFormat("\t{0} bytes per sector", page.BytesPerSector).AppendLine();
            if(page.WritePrecompCylinder < page.Cylinders)
                sb.AppendFormat("\tWrite pre-compensation starts at cylinder {0}", page.WritePrecompCylinder).AppendLine();
            if(page.WriteReduceCylinder < page.Cylinders)
                sb.AppendFormat("\tWrite current reduction starts at cylinder {0}", page.WriteReduceCylinder).AppendLine();
            if(page.DriveStepRate > 0)
                sb.AppendFormat("\tDrive steps in {0} μs", (uint)page.DriveStepRate * 100).AppendLine();
            if(page.DriveStepPulse > 0)
                sb.AppendFormat("\tEach step pulse is {0} ms", page.DriveStepPulse).AppendLine();
            if(page.HeadSettleDelay > 0)
                sb.AppendFormat("\tHeads settles in {0} μs", (uint)page.HeadSettleDelay * 100).AppendLine();

            if(!page.TRDY)
                sb.AppendFormat("\tTarget shall wait {0} seconds before attempting to access the medium after motor on is asserted",
                    (double)page.MotorOnDelay * 10).AppendLine();
            else
                sb.AppendFormat("\tTarget shall wait {0} seconds after drive is ready before aborting medium access attemps",
                    (double)page.MotorOnDelay * 10).AppendLine();

            if(page.MotorOffDelay != 0xFF)
                sb.AppendFormat("\tTarget shall wait {0} seconds before releasing the motor on signal after becoming idle",
                    (double)page.MotorOffDelay * 10).AppendLine();
            else
                sb.AppendLine("\tTarget shall never release the motor on signal");

            if(page.TRDY)
                sb.AppendLine("\tThere is a drive ready signal");
            if(page.SSN)
                sb.AppendLine("\tSectors start at 1");
            if(page.MO)
                sb.AppendLine("\tThe motor on signal shall remain released");

            sb.AppendFormat("\tDrive needs to do {0} step pulses per cylinder", page.SPC + 1).AppendLine();

            if(page.WriteCompensation > 0)
                sb.AppendFormat("\tWrite pre-compensation is {0}", page.WriteCompensation).AppendLine();
            if(page.HeadLoadDelay > 0)
                sb.AppendFormat("\tHead takes {0} ms to load", page.HeadLoadDelay).AppendLine();
            if(page.HeadUnloadDelay > 0)
                sb.AppendFormat("\tHead takes {0} ms to unload", page.HeadUnloadDelay).AppendLine();

            if(page.MediumRotationRate > 0)
                sb.AppendFormat("\tMedium rotates at {0} rpm", page.MediumRotationRate).AppendLine();

            switch(page.Pin34 & 0x07)
            {
                case 0:
                    sb.AppendLine("\tPin 34 is unconnected");
                    break;
                case 1:
                    sb.Append("\tPin 34 indicates drive is ready when active ");
                    if((page.Pin34 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                case 2:
                    sb.Append("\tPin 34 indicates disk has changed when active ");
                    if((page.Pin34 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                default:
                    sb.AppendFormat("\tPin 34 indicates unknown function {0} when active ", page.Pin34 & 0x07);
                    if((page.Pin34 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
            }

            switch(page.Pin4 & 0x07)
            {
                case 0:
                    sb.AppendLine("\tPin 4 is unconnected");
                    break;
                case 1:
                    sb.Append("\tPin 4 indicates drive is in use when active ");
                    if((page.Pin4 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                case 2:
                    sb.Append("\tPin 4 indicates eject when active ");
                    if((page.Pin4 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                case 3:
                    sb.Append("\tPin 4 indicates head load when active ");
                    if((page.Pin4 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                default:
                    sb.AppendFormat("\tPin 4 indicates unknown function {0} when active ", page.Pin4 & 0x07);
                    if((page.Pin4 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
            }

            switch(page.Pin2 & 0x07)
            {
                case 0:
                    sb.AppendLine("\tPin 2 is unconnected");
                    break;
                default:
                    sb.AppendFormat("\tPin 2 indicates unknown function {0} when active ", page.Pin2 & 0x07);
                    if((page.Pin2 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
            }

            switch(page.Pin1 & 0x07)
            {
                case 0:
                    sb.AppendLine("\tPin 1 is unconnected");
                    break;
                case 1:
                    sb.Append("\tPin 1 indicates disk change reset when active ");
                    if((page.Pin1 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
                default:
                    sb.AppendFormat("\tPin 1 indicates unknown function {0} when active ", page.Pin1 & 0x07);
                    if((page.Pin1 & 0x08) == 0x08)
                        sb.Append("high");
                    else
                        sb.Append("low");
                    break;
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x05: Flexible disk page

        #region Mode Page 0x03: Format device page

        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x03
        /// 24 bytes in SCSI-2, SBC-1
        /// </summary>
        public struct ModePage_03
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Tracks per zone to use in dividing the capacity for the purpose of allocating alternate sectors
            /// </summary>
            public ushort TracksPerZone;
            /// <summary>
            /// Number of sectors per zone that shall be reserved for defect handling
            /// </summary>
            public ushort AltSectorsPerZone;
            /// <summary>
            /// Number of tracks per zone that shall be reserved for defect handling
            /// </summary>
            public ushort AltTracksPerZone;
            /// <summary>
            /// Number of tracks per LUN that shall be reserved for defect handling
            /// </summary>
            public ushort AltTracksPerLun;
            /// <summary>
            /// Number of physical sectors per track
            /// </summary>
            public ushort SectorsPerTrack;
            /// <summary>
            /// Bytes per physical sector
            /// </summary>
            public ushort BytesPerSector;
            /// <summary>
            /// Interleave value, target dependent
            /// </summary>
            public ushort Interleave;
            /// <summary>
            /// Sectors between last block of one track and first block of the next
            /// </summary>
            public ushort TrackSkew;
            /// <summary>
            /// Sectors between last block of a cylinder and first block of the next one
            /// </summary>
            public ushort CylinderSkew;
            /// <summary>
            /// Soft-sectored
            /// </summary>
            public bool SSEC;
            /// <summary>
            /// Hard-sectored
            /// </summary>
            public bool HSEC;
            /// <summary>
            /// Removable
            /// </summary>
            public bool RMB;
            /// <summary>
            /// If set, address are allocated progressively in a surface before going to the next.
            /// Otherwise, it goes by cylinders
            /// </summary>
            public bool SURF;
        }

        public static ModePage_03? DecodeModePage_03(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x03)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 24)
                return null;

            ModePage_03 decoded = new ModePage_03();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.TracksPerZone = (ushort)((pageResponse[2] << 8) + pageResponse[3]);
            decoded.AltSectorsPerZone = (ushort)((pageResponse[4] << 8) + pageResponse[5]);
            decoded.AltTracksPerZone = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.AltTracksPerLun = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.SectorsPerTrack = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.BytesPerSector = (ushort)((pageResponse[12] << 8) + pageResponse[13]);
            decoded.Interleave = (ushort)((pageResponse[14] << 8) + pageResponse[15]);
            decoded.TrackSkew = (ushort)((pageResponse[16] << 8) + pageResponse[17]);
            decoded.CylinderSkew = (ushort)((pageResponse[18] << 8) + pageResponse[19]);
            decoded.SSEC |= (pageResponse[20] & 0x80) == 0x80;
            decoded.HSEC |= (pageResponse[20] & 0x40) == 0x40;
            decoded.RMB |= (pageResponse[20] & 0x20) == 0x20;
            decoded.SURF |= (pageResponse[20] & 0x10) == 0x10;

            return decoded;
        }

        public static string PrettifyModePage_03(byte[] pageResponse)
        {
            return PrettifyModePage_03(DecodeModePage_03(pageResponse));
        }

        public static string PrettifyModePage_03(ModePage_03? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_03 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Format device page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\t{0} tracks per zone to use in dividing the capacity for the purpose of allocating alternate sectors", page.TracksPerZone).AppendLine();
            sb.AppendFormat("\t{0} sectors per zone that shall be reserved for defect handling", page.AltSectorsPerZone).AppendLine();
            sb.AppendFormat("\t{0} tracks per zone that shall be reserved for defect handling", page.AltTracksPerZone).AppendLine();
            sb.AppendFormat("\t{0} tracks per LUN that shall be reserved for defect handling", page.AltTracksPerLun).AppendLine();
            sb.AppendFormat("\t{0} physical sectors per track", page.SectorsPerTrack).AppendLine();
            sb.AppendFormat("\t{0} Bytes per physical sector", page.BytesPerSector).AppendLine();
            sb.AppendFormat("\tTarget-dependent interleave value is {0}", page.Interleave).AppendLine();
            sb.AppendFormat("\t{0} sectors between last block of one track and first block of the next", page.TrackSkew).AppendLine();
            sb.AppendFormat("\t{0} sectors between last block of a cylinder and first block of the next one", page.CylinderSkew).AppendLine();
            if(page.SSEC)
                sb.AppendLine("\tDrive supports soft-sectoring format");
            if(page.HSEC)
                sb.AppendLine("\tDrive supports hard-sectoring format");
            if(page.RMB)
                sb.AppendLine("\tDrive media is removable");
            if(page.SURF)
                sb.AppendLine("\tSector addressing is progressively incremented in one surface before going to the next");
            else
                sb.AppendLine("\tSector addressing is progressively incremented in one cylinder before going to the next");

            return sb.ToString();
        }

        #endregion Mode Page 0x03: Format device page

        #region Mode Page 0x0B: Medium types supported page

        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x0B
        /// 8 bytes in SCSI-2
        /// </summary>
        public struct ModePage_0B
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public MediumTypes MediumType1;
            public MediumTypes MediumType2;
            public MediumTypes MediumType3;
            public MediumTypes MediumType4;
        }

        public static ModePage_0B? DecodeModePage_0B(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x0B)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            ModePage_0B decoded = new ModePage_0B();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.MediumType1 = (MediumTypes)pageResponse[4];
            decoded.MediumType2 = (MediumTypes)pageResponse[5];
            decoded.MediumType3 = (MediumTypes)pageResponse[6];
            decoded.MediumType4 = (MediumTypes)pageResponse[7];

            return decoded;
        }

        public static string PrettifyModePage_0B(byte[] pageResponse)
        {
            return PrettifyModePage_0B(DecodeModePage_0B(pageResponse));
        }

        public static string PrettifyModePage_0B(ModePage_0B? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_0B page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Medium types supported page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.MediumType1 != MediumTypes.Default)
                sb.AppendFormat("Supported medium type one: {0}", GetMediumTypeDescription(page.MediumType1)).AppendLine();
            if(page.MediumType2 != MediumTypes.Default)
                sb.AppendFormat("Supported medium type two: {0}", GetMediumTypeDescription(page.MediumType2)).AppendLine();
            if(page.MediumType3 != MediumTypes.Default)
                sb.AppendFormat("Supported medium type three: {0}", GetMediumTypeDescription(page.MediumType3)).AppendLine();
            if(page.MediumType4 != MediumTypes.Default)
                sb.AppendFormat("Supported medium type four: {0}", GetMediumTypeDescription(page.MediumType4)).AppendLine();

            return sb.ToString();
        }

        #endregion Mode Page 0x0B: Medium types supported page

        #region Mode Page 0x0C: Notch page

        // TODO: Implement this page

        #endregion Mode Page 0x0C: Notch page

        #region Mode Page 0x01: Read-write error recovery page

        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x01
        /// 12 bytes in SCSI-2, SBC-1, SBC-2
        /// </summary>
        public struct ModePage_01
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Automatic Write Reallocation Enabled
            /// </summary>
            public bool AWRE;
            /// <summary>
            /// Automatic Read Reallocation Enabled
            /// </summary>
            public bool ARRE;
            /// <summary>
            /// Transfer block
            /// </summary>
            public bool TB;
            /// <summary>
            /// Read continuous
            /// </summary>
            public bool RC;
            /// <summary>
            /// Enable early recovery
            /// </summary>
            public bool EER;
            /// <summary>
            /// Post error reporting
            /// </summary>
            public bool PER;
            /// <summary>
            /// Disable transfer on error
            /// </summary>
            public bool DTE;
            /// <summary>
            /// Disable correction
            /// </summary>
            public bool DCR;
            /// <summary>
            /// How many times to retry a read operation
            /// </summary>
            public byte ReadRetryCount;
            /// <summary>
            /// How many bits of largest data burst error is maximum to apply error correction on it
            /// </summary>
            public byte CorrectionSpan;
            /// <summary>
            /// Offset to move the heads
            /// </summary>
            public sbyte HeadOffsetCount;
            /// <summary>
            /// Incremental position to which the recovered data strobe shall be adjusted
            /// </summary>
            public sbyte DataStrobeOffsetCount;
            /// <summary>
            /// How many times to retry a write operation
            /// </summary>
            public byte WriteRetryCount;
            /// <summary>
            /// Maximum time in ms to use in data error recovery procedures
            /// </summary>
            public ushort RecoveryTimeLimit;

            /// <summary>
            /// Logical block provisioning error reporting is enabled
            /// </summary>
            public bool LBPERE;
        }

        public static ModePage_01? DecodeModePage_01(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x01)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            ModePage_01 decoded = new ModePage_01();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.AWRE |= (pageResponse[2] & 0x80) == 0x80;
            decoded.ARRE |= (pageResponse[2] & 0x40) == 0x40;
            decoded.TB |= (pageResponse[2] & 0x20) == 0x20;
            decoded.RC |= (pageResponse[2] & 0x10) == 0x10;
            decoded.EER |= (pageResponse[2] & 0x08) == 0x08;
            decoded.PER |= (pageResponse[2] & 0x04) == 0x04;
            decoded.DTE |= (pageResponse[2] & 0x02) == 0x02;
            decoded.DCR |= (pageResponse[2] & 0x01) == 0x01;

            decoded.ReadRetryCount = pageResponse[3];
            decoded.CorrectionSpan = pageResponse[4];
            decoded.HeadOffsetCount = (sbyte)pageResponse[5];
            decoded.DataStrobeOffsetCount = (sbyte)pageResponse[6];

            if(pageResponse.Length < 12)
                return decoded;

            decoded.WriteRetryCount = pageResponse[8];
            decoded.RecoveryTimeLimit = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.LBPERE |= (pageResponse[7] & 0x80) == 0x80;

            return decoded;
        }

        public static string PrettifyModePage_01(byte[] pageResponse)
        {
            return PrettifyModePage_01(DecodeModePage_01(pageResponse));
        }

        public static string PrettifyModePage_01(ModePage_01? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_01 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Read-write error recovery page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.AWRE)
                sb.AppendLine("\tAutomatic write reallocation is enabled");
            if(page.ARRE)
                sb.AppendLine("\tAutomatic read reallocation is enabled");
            if(page.TB)
                sb.AppendLine("\tData not recovered within limits shall be transferred back before a CHECK CONDITION");
            if(page.RC)
                sb.AppendLine("\tDrive will transfer the entire requested length without delaying to perform error recovery");
            if(page.EER)
                sb.AppendLine("\tDrive will use the most expedient form of error recovery first");
            if(page.PER)
                sb.AppendLine("\tDrive shall report recovered errors");
            if(page.DTE)
                sb.AppendLine("\tTransfer will be terminated upon error detection");
            if(page.DCR)
                sb.AppendLine("\tError correction is disabled");
            if(page.ReadRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat read operations {0} times", page.ReadRetryCount).AppendLine();
            if(page.WriteRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat write operations {0} times", page.WriteRetryCount).AppendLine();
            if(page.RecoveryTimeLimit > 0)
                sb.AppendFormat("\tDrive will employ a maximum of {0} ms to recover data", page.RecoveryTimeLimit).AppendLine();
            if(page.LBPERE)
                sb.AppendLine("Logical block provisioning error reporting is enabled");

            return sb.ToString();
        }

        #endregion Mode Page 0x01: Read-write error recovery page

        #region Mode Page 0x04: Rigid disk drive geometry page

        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x04
        /// 24 bytes in SCSI-2, SBC-1
        /// </summary>
        public struct ModePage_04
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Cylinders used for data storage
            /// </summary>
            public uint Cylinders;
            /// <summary>
            /// Heads for reading and/or writing
            /// </summary>
            public byte Heads;
            /// <summary>
            /// Cylinder where write precompensation starts
            /// </summary>
            public uint WritePrecompCylinder;
            /// <summary>
            /// Cylinder where write current reduction starts
            /// </summary>
            public uint WriteReduceCylinder;
            /// <summary>
            /// Step rate in 100 ns units
            /// </summary>
            public ushort DriveStepRate;
            /// <summary>
            /// Cylinder where the heads park
            /// </summary>
            public int LandingCylinder;
            /// <summary>
            /// Rotational position locking
            /// </summary>
            public byte RPL;
            /// <summary>
            /// Rotational skew to apply when synchronized
            /// </summary>
            public byte RotationalOffset;
            /// <summary>
            /// Medium speed in rpm
            /// </summary>
            public ushort MediumRotationRate;
        }

        public static ModePage_04? DecodeModePage_04(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x04)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 20)
                return null;

            ModePage_04 decoded = new ModePage_04();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.Cylinders = (uint)((pageResponse[2] << 16) + (pageResponse[3] << 8) + pageResponse[4]);
            decoded.Heads = pageResponse[5];
            decoded.WritePrecompCylinder = (uint)((pageResponse[6] << 16) + (pageResponse[7] << 8) + pageResponse[8]);
            decoded.WriteReduceCylinder = (uint)((pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);
            decoded.DriveStepRate = (ushort)((pageResponse[12] << 8) + pageResponse[13]);
            decoded.LandingCylinder = ((pageResponse[14] << 16) + (pageResponse[15] << 8) + pageResponse[16]);
            decoded.RPL = (byte)(pageResponse[17] & 0x03);
            decoded.RotationalOffset = pageResponse[18];

            if(pageResponse.Length >= 22)
                decoded.MediumRotationRate = (ushort)((pageResponse[20] << 8) + pageResponse[21]);

            return decoded;
        }

        public static string PrettifyModePage_04(byte[] pageResponse)
        {
            return PrettifyModePage_04(DecodeModePage_04(pageResponse));
        }

        public static string PrettifyModePage_04(ModePage_04? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_04 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Rigid disk drive geometry page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\t{0} heads", page.Heads).AppendLine();
            sb.AppendFormat("\t{0} cylinders", page.Cylinders).AppendLine();
            if(page.WritePrecompCylinder < page.Cylinders)
                sb.AppendFormat("\tWrite pre-compensation starts at cylinder {0}", page.WritePrecompCylinder).AppendLine();
            if(page.WriteReduceCylinder < page.Cylinders)
                sb.AppendFormat("\tWrite current reduction starts at cylinder {0}", page.WriteReduceCylinder).AppendLine();
            if(page.DriveStepRate > 0)
                sb.AppendFormat("\tDrive steps in {0} ns", (uint)page.DriveStepRate * 100).AppendLine();

            sb.AppendFormat("\tHeads park in cylinder {0}", page.LandingCylinder).AppendLine();

            if(page.MediumRotationRate > 0)
                sb.AppendFormat("\tMedium rotates at {0} rpm", page.MediumRotationRate).AppendLine();

            switch(page.RPL)
            {
                case 0:
                    sb.AppendLine("\tSpindle synchronization is disable or unsupported");
                    break;
                case 1:
                    sb.AppendLine("\tTarget operates as a synchronized-spindle slave");
                    break;
                case 2:
                    sb.AppendLine("\tTarget operates as a synchronized-spindle master");
                    break;
                case 3:
                    sb.AppendLine("\tTarget operates as a synchronized-spindle master control");
                    break;
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x04: Rigid disk drive geometry page

        #region Mode Page 0x07: Verify error recovery page

        /// <summary>
        /// Disconnect-reconnect page
        /// Page code 0x07
        /// 12 bytes in SCSI-2, SBC-1, SBC-2
        /// </summary>
        public struct ModePage_07
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Enable early recovery
            /// </summary>
            public bool EER;
            /// <summary>
            /// Post error reporting
            /// </summary>
            public bool PER;
            /// <summary>
            /// Disable transfer on error
            /// </summary>
            public bool DTE;
            /// <summary>
            /// Disable correction
            /// </summary>
            public bool DCR;
            /// <summary>
            /// How many times to retry a verify operation
            /// </summary>
            public byte VerifyRetryCount;
            /// <summary>
            /// How many bits of largest data burst error is maximum to apply error correction on it
            /// </summary>
            public byte CorrectionSpan;
            /// <summary>
            /// Maximum time in ms to use in data error recovery procedures
            /// </summary>
            public ushort RecoveryTimeLimit;
        }

        public static ModePage_07? DecodeModePage_07(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x07)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 12)
                return null;

            ModePage_07 decoded = new ModePage_07();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.EER |= (pageResponse[2] & 0x08) == 0x08;
            decoded.PER |= (pageResponse[2] & 0x04) == 0x04;
            decoded.DTE |= (pageResponse[2] & 0x02) == 0x02;
            decoded.DCR |= (pageResponse[2] & 0x01) == 0x01;

            decoded.VerifyRetryCount = pageResponse[3];
            decoded.CorrectionSpan = pageResponse[4];
            decoded.RecoveryTimeLimit = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

            return decoded;
        }

        public static string PrettifyModePage_07(byte[] pageResponse)
        {
            return PrettifyModePage_07(DecodeModePage_07(pageResponse));
        }

        public static string PrettifyModePage_07(ModePage_07? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_07 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Verify error recovery page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.EER)
                sb.AppendLine("\tDrive will use the most expedient form of error recovery first");
            if(page.PER)
                sb.AppendLine("\tDrive shall report recovered errors");
            if(page.DTE)
                sb.AppendLine("\tTransfer will be terminated upon error detection");
            if(page.DCR)
                sb.AppendLine("\tError correction is disabled");
            if(page.VerifyRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat verify operations {0} times", page.VerifyRetryCount).AppendLine();
            if(page.RecoveryTimeLimit > 0)
                sb.AppendFormat("\tDrive will employ a maximum of {0} ms to recover data", page.RecoveryTimeLimit).AppendLine();

            return sb.ToString();
        }

        #endregion Mode Page 0x07: Verify error recovery page

        #region Mode Page 0x10: Device configuration page

        /// <summary>
        /// Device configuration page
        /// Page code 0x10
        /// 16 bytes in SCSI-2, SSC-1, SSC-2, SSC-3
        /// </summary>
        public struct ModePage_10_SSC
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Used in mode select to change partition to one specified in <see cref="ActivePartition"/> 
            /// </summary>
            public bool CAP;
            /// <summary>
            /// Used in mode select to change format to one specified in <see cref="ActiveFormat"/> 
            /// </summary>
            public bool CAF;
            /// <summary>
            /// Active format, vendor-specific
            /// </summary>
            public byte ActiveFormat;
            /// <summary>
            /// Current logical partition
            /// </summary>
            public byte ActivePartition;
            /// <summary>
            /// How full the buffer shall be before writing to medium
            /// </summary>
            public byte WriteBufferFullRatio;
            /// <summary>
            /// How empty the buffer shall be before reading more data from the medium
            /// </summary>
            public byte ReadBufferEmptyRatio;
            /// <summary>
            /// Delay in 100 ms before buffered data is forcefully written to the medium even before buffer is full
            /// </summary>
            public ushort WriteDelayTime;
            /// <summary>
            /// Drive supports recovering data from buffer
            /// </summary>
            public bool DBR;
            /// <summary>
            /// Medium has block IDs
            /// </summary>
            public bool BIS;
            /// <summary>
            /// Drive recognizes and reports setmarks
            /// </summary>
            public bool RSmk;
            /// <summary>
            /// Drive selects best speed
            /// </summary>
            public bool AVC;
            /// <summary>
            /// If drive should stop pre-reading on filemarks
            /// </summary>
            public byte SOCF;
            /// <summary>
            /// If set, recovered buffer data is LIFO, otherwise, FIFO
            /// </summary>
            public bool RBO;
            /// <summary>
            /// Report early warnings
            /// </summary>
            public bool REW;
            /// <summary>
            /// Inter-block gap
            /// </summary>
            public byte GapSize;
            /// <summary>
            /// End-of-Data format
            /// </summary>
            public byte EODDefined;
            /// <summary>
            /// EOD generation enabled
            /// </summary>
            public bool EEG;
            /// <summary>
            /// Synchronize data to medium on early warning
            /// </summary>
            public bool SEW;
            /// <summary>
            /// Bytes to reduce buffer size on early warning
            /// </summary>
            public uint BufferSizeEarlyWarning;
            /// <summary>
            /// Selected data compression algorithm
            /// </summary>
            public byte SelectedCompression;

            /// <summary>
            /// Soft write protect
            /// </summary>
            public bool SWP;
            /// <summary>
            /// Associated write protect
            /// </summary>
            public bool ASOCWP;
            /// <summary>
            /// Persistent write protect
            /// </summary>
            public bool PERSWP;
            /// <summary>
            /// Permanent write protect
            /// </summary>
            public bool PRMWP;

            public bool BAML;
            public bool BAM;
            public byte RewindOnReset;

            /// <summary>
            /// How drive shall respond to detection of compromised WORM medium integrity
            /// </summary>
            public byte WTRE;
            /// <summary>
            /// Respond to commands only if a reservation exists
            /// </summary>
            public bool OIR;
        }

        public static ModePage_10_SSC? DecodeModePage_10_SSC(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x10)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 16)
                return null;

            ModePage_10_SSC decoded = new ModePage_10_SSC();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.CAP |= (pageResponse[2] & 0x40) == 0x40;
            decoded.CAF |= (pageResponse[2] & 0x20) == 0x20;
            decoded.ActiveFormat = (byte)(pageResponse[2] & 0x1F);
            decoded.ActivePartition = pageResponse[3];
            decoded.WriteBufferFullRatio = pageResponse[4];
            decoded.ReadBufferEmptyRatio = pageResponse[5];
            decoded.WriteDelayTime = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.DBR |= (pageResponse[8] & 0x80) == 0x80;
            decoded.BIS |= (pageResponse[8] & 0x40) == 0x40;
            decoded.RSmk |= (pageResponse[8] & 0x20) == 0x20;
            decoded.AVC |= (pageResponse[8] & 0x10) == 0x10;
            decoded.RBO |= (pageResponse[8] & 0x02) == 0x02;
            decoded.REW |= (pageResponse[8] & 0x01) == 0x01;
            decoded.EEG |= (pageResponse[10] & 0x10) == 0x10;
            decoded.SEW |= (pageResponse[10] & 0x08) == 0x08;
            decoded.SOCF = (byte)((pageResponse[8] & 0x0C) >> 2);
            decoded.BufferSizeEarlyWarning = (uint)((pageResponse[11] << 16) + (pageResponse[12] << 8) + pageResponse[13]);
            decoded.SelectedCompression = pageResponse[14];

            decoded.SWP |= (pageResponse[10] & 0x04) == 0x04;
            decoded.ASOCWP |= (pageResponse[15] & 0x04) == 0x04;
            decoded.PERSWP |= (pageResponse[15] & 0x02) == 0x02;
            decoded.PRMWP |= (pageResponse[15] & 0x01) == 0x01;

            decoded.BAML |= (pageResponse[10] & 0x02) == 0x02;
            decoded.BAM |= (pageResponse[10] & 0x01) == 0x01;

            decoded.RewindOnReset = (byte)((pageResponse[15] & 0x18) >> 3);

            decoded.OIR |= (pageResponse[15] & 0x20) == 0x20;
            decoded.WTRE = (byte)((pageResponse[15] & 0xC0) >> 6);

            return decoded;
        }

        public static string PrettifyModePage_10_SSC(byte[] pageResponse)
        {
            return PrettifyModePage_10_SSC(DecodeModePage_10_SSC(pageResponse));
        }

        public static string PrettifyModePage_10_SSC(ModePage_10_SSC? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_10_SSC page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Device configuration page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\tActive format: {0}", page.ActiveFormat).AppendLine();
            sb.AppendFormat("\tActive partition: {0}", page.ActivePartition).AppendLine();
            sb.AppendFormat("\tWrite buffer shall have a full ratio of {0} before being flushed to medium", page.WriteBufferFullRatio).AppendLine();
            sb.AppendFormat("\tRead buffer shall have an empty ratio of {0} before more data is read from medium", page.ReadBufferEmptyRatio).AppendLine();
            sb.AppendFormat("\tDrive will delay {0} ms before buffered data is forcefully written to the medium even before buffer is full", page.WriteDelayTime * 100).AppendLine();
            if(page.DBR)
            {
                sb.AppendLine("\tDrive supports recovering data from buffer");
                if(page.RBO)
                    sb.AppendLine("\tRecovered buffer data comes in LIFO order");
                else
                    sb.AppendLine("\tRecovered buffer data comes in FIFO order");
            }
            if(page.BIS)
                sb.AppendLine("\tMedium supports block IDs");
            if(page.RSmk)
                sb.AppendLine("\tDrive reports setmarks");
            switch(page.SOCF)
            {
                case 0:
                    sb.AppendLine("\tDrive will pre-read until buffer is full");
                    break;
                case 1:
                    sb.AppendLine("\tDrive will pre-read until one filemark is detected");
                    break;
                case 2:
                    sb.AppendLine("\tDrive will pre-read until two filemark is detected");
                    break;
                case 3:
                    sb.AppendLine("\tDrive will pre-read until three filemark is detected");
                    break;
            }

            if(page.REW)
            {
                sb.AppendLine("\tDrive reports early warnings");
                if(page.SEW)
                    sb.AppendLine("\tDrive will synchronize buffer to medium on early warnings");
            }

            switch(page.GapSize)
            {
                case 0:
                    break;
                case 1:
                    sb.AppendLine("\tInter-block gap is long enough to support update in place");
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    sb.AppendFormat("\tInter-block gap is {0} times the device's defined gap size", page.GapSize).AppendLine();
                    break;
                default:
                    sb.AppendFormat("\tInter-block gap is unknown value {0}", page.GapSize).AppendLine();
                    break;
            }

            if(page.EEG)
                sb.AppendLine("\tDrive generates end-of-data");

            switch(page.SelectedCompression)
            {
                case 0:
                    sb.AppendLine("\tDrive does not use compression");
                    break;
                case 1:
                    sb.AppendLine("\tDrive uses default compression");
                    break;
                default:
                    sb.AppendFormat("\tDrive uses unknown compression {0}", page.SelectedCompression).AppendLine();
                    break;
            }

            if(page.SWP)
                sb.AppendLine("\tSoftware write protect is enabled");
            if(page.ASOCWP)
                sb.AppendLine("\tAssociated write protect is enabled");
            if(page.PERSWP)
                sb.AppendLine("\tPersistent write protect is enabled");
            if(page.PRMWP)
                sb.AppendLine("\tPermanent write protect is enabled");

            if(page.BAML)
            {
                if(page.BAM)
                    sb.AppendLine("\tDrive operates using explicit address mode");
                else
                    sb.AppendLine("\tDrive operates using implicit address mode");
            }

            switch(page.RewindOnReset)
            {
                case 1:
                    sb.AppendLine("\tDrive shall position to beginning of default data partition on reset");
                    break;
                case 2:
                    sb.AppendLine("\tDrive shall maintain its position on reset");
                    break;
            }

            switch(page.WTRE)
            {
                case 1:
                    sb.AppendLine("\tDrive will do nothing on WORM tampered medium");
                    break;
                case 2:
                    sb.AppendLine("\tDrive will return CHECK CONDITION on WORM tampered medium");
                    break;
            }

            if(page.OIR)
                sb.AppendLine("\tDrive will only respond to commands if it has received a reservation");

            return sb.ToString();
        }

        #endregion Mode Page 0x10: Device configuration page

        #region Mode Page 0x0E: CD-ROM audio control parameters page

        /// <summary>
        /// CD-ROM audio control parameters
        /// Page code 0x0E
        /// 16 bytes in SCSI-2, MMC-1, MMC-2, MMC-3
        /// </summary>
        public struct ModePage_0E
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Return status as soon as playback operation starts
            /// </summary>
            public bool Immed;
            /// <summary>
            /// Stop on track crossing
            /// </summary>
            public bool SOTC;
            /// <summary>
            /// Indicates <see cref="BlocksPerSecondOfAudio"/> is valid
            /// </summary>
            public bool APRVal;
            /// <summary>
            /// Multiplier for <see cref="BlocksPerSecondOfAudio"/>
            /// </summary>
            public byte LBAFormat;
            /// <summary>
            /// LBAs per second of audio
            /// </summary>
            public ushort BlocksPerSecondOfAudio;
            /// <summary>
            /// Channels output on this port
            /// </summary>
            public byte OutputPort0ChannelSelection;
            /// <summary>
            /// Volume level for this port
            /// </summary>
            public byte OutputPort0Volume;
            /// <summary>
            /// Channels output on this port
            /// </summary>
            public byte OutputPort1ChannelSelection;
            /// <summary>
            /// Volume level for this port
            /// </summary>
            public byte OutputPort1Volume;
            /// <summary>
            /// Channels output on this port
            /// </summary>
            public byte OutputPort2ChannelSelection;
            /// <summary>
            /// Volume level for this port
            /// </summary>
            public byte OutputPort2Volume;
            /// <summary>
            /// Channels output on this port
            /// </summary>
            public byte OutputPort3ChannelSelection;
            /// <summary>
            /// Volume level for this port
            /// </summary>
            public byte OutputPort3Volume;
        }

        public static ModePage_0E? DecodeModePage_0E(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x0E)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 16)
                return null;

            ModePage_0E decoded = new ModePage_0E();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.Immed |= (pageResponse[2] & 0x04) == 0x04;
            decoded.SOTC |= (pageResponse[2] & 0x02) == 0x02;
            decoded.APRVal |= (pageResponse[5] & 0x80) == 0x80;
            decoded.LBAFormat = (byte)(pageResponse[5] & 0x0F);
            decoded.BlocksPerSecondOfAudio = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.OutputPort0ChannelSelection = (byte)(pageResponse[8] & 0x0F);
            decoded.OutputPort0Volume = pageResponse[9];
            decoded.OutputPort1ChannelSelection = (byte)(pageResponse[10] & 0x0F);
            decoded.OutputPort1Volume = pageResponse[11];
            decoded.OutputPort2ChannelSelection = (byte)(pageResponse[12] & 0x0F);
            decoded.OutputPort2Volume = pageResponse[13];
            decoded.OutputPort3ChannelSelection = (byte)(pageResponse[14] & 0x0F);
            decoded.OutputPort3Volume = pageResponse[15];

            return decoded;
        }

        public static string PrettifyModePage_0E(byte[] pageResponse)
        {
            return PrettifyModePage_0E(DecodeModePage_0E(pageResponse));
        }

        public static string PrettifyModePage_0E(ModePage_0E? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_0E page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI CD-ROM audio control parameters page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");
            if(page.Immed)
                sb.AppendLine("\tDrive will return from playback command immediately");
            else
                sb.AppendLine("\tDrive will return from playback command when playback ends");
            if(page.SOTC)
                sb.AppendLine("\tDrive will stop playback on track end");

            if(page.APRVal)
            {
                double blocks;
                if(page.LBAFormat == 8)
                    blocks = page.BlocksPerSecondOfAudio * (1 / 256);
                else
                    blocks = page.BlocksPerSecondOfAudio;

                sb.AppendFormat("\tThere are {0} blocks per each second of audio", blocks).AppendLine();
            }

            if(page.OutputPort0ChannelSelection > 0)
            {
                sb.Append("\tOutput port 0 has channels ");
                if((page.OutputPort0ChannelSelection & 0x01) == 0x01)
                    sb.Append("0 ");
                if((page.OutputPort0ChannelSelection & 0x02) == 0x02)
                    sb.Append("1 ");
                if((page.OutputPort0ChannelSelection & 0x04) == 0x04)
                    sb.Append("2 ");
                if((page.OutputPort0ChannelSelection & 0x08) == 0x08)
                    sb.Append("3 ");

                switch(page.OutputPort0Volume)
                {
                    case 0:
                        sb.AppendLine("muted");
                        break;
                    case 0xFF:
                        sb.AppendLine("at maximum volume");
                        break;
                    default:
                        sb.AppendFormat("at volume {0}", page.OutputPort0Volume).AppendLine();
                        break;
                }
            }

            if(page.OutputPort1ChannelSelection > 0)
            {
                sb.Append("\tOutput port 1 has channels ");
                if((page.OutputPort1ChannelSelection & 0x01) == 0x01)
                    sb.Append("0 ");
                if((page.OutputPort1ChannelSelection & 0x02) == 0x02)
                    sb.Append("1 ");
                if((page.OutputPort1ChannelSelection & 0x04) == 0x04)
                    sb.Append("2 ");
                if((page.OutputPort1ChannelSelection & 0x08) == 0x08)
                    sb.Append("3 ");

                switch(page.OutputPort1Volume)
                {
                    case 0:
                        sb.AppendLine("muted");
                        break;
                    case 0xFF:
                        sb.AppendLine("at maximum volume");
                        break;
                    default:
                        sb.AppendFormat("at volume {0}", page.OutputPort1Volume).AppendLine();
                        break;
                }
            }

            if(page.OutputPort2ChannelSelection > 0)
            {
                sb.Append("\tOutput port 2 has channels ");
                if((page.OutputPort2ChannelSelection & 0x01) == 0x01)
                    sb.Append("0 ");
                if((page.OutputPort2ChannelSelection & 0x02) == 0x02)
                    sb.Append("1 ");
                if((page.OutputPort2ChannelSelection & 0x04) == 0x04)
                    sb.Append("2 ");
                if((page.OutputPort2ChannelSelection & 0x08) == 0x08)
                    sb.Append("3 ");

                switch(page.OutputPort2Volume)
                {
                    case 0:
                        sb.AppendLine("muted");
                        break;
                    case 0xFF:
                        sb.AppendLine("at maximum volume");
                        break;
                    default:
                        sb.AppendFormat("at volume {0}", page.OutputPort2Volume).AppendLine();
                        break;
                }
            }

            if(page.OutputPort3ChannelSelection > 0)
            {
                sb.Append("\tOutput port 3 has channels ");
                if((page.OutputPort3ChannelSelection & 0x01) == 0x01)
                    sb.Append("0 ");
                if((page.OutputPort3ChannelSelection & 0x02) == 0x02)
                    sb.Append("1 ");
                if((page.OutputPort3ChannelSelection & 0x04) == 0x04)
                    sb.Append("2 ");
                if((page.OutputPort3ChannelSelection & 0x08) == 0x08)
                    sb.Append("3 ");

                switch(page.OutputPort3Volume)
                {
                    case 0:
                        sb.AppendLine("muted");
                        break;
                    case 0xFF:
                        sb.AppendLine("at maximum volume");
                        break;
                    default:
                        sb.AppendFormat("at volume {0}", page.OutputPort3Volume).AppendLine();
                        break;
                }
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x0E: CD-ROM audio control parameters page

        #region Mode Page 0x0D: CD-ROM parameteres page

        /// <summary>
        /// CD-ROM parameteres page
        /// Page code 0x0D
        /// 8 bytes in SCSI-2, MMC-1, MMC-2, MMC-3
        /// </summary>
        public struct ModePage_0D
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Time the drive shall remain in hold track state after seek or read
            /// </summary>
            public byte InactivityTimerMultiplier;
            /// <summary>
            /// Seconds per Minute
            /// </summary>
            public ushort SecondsPerMinute;
            /// <summary>
            /// Frames per Second
            /// </summary>
            public ushort FramesPerSecond;
        }

        public static ModePage_0D? DecodeModePage_0D(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x0D)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            ModePage_0D decoded = new ModePage_0D();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.InactivityTimerMultiplier = (byte)(pageResponse[3] & 0xF);
            decoded.SecondsPerMinute = (ushort)((pageResponse[4] << 8) + pageResponse[5]);
            decoded.FramesPerSecond = (ushort)((pageResponse[6] << 8) + pageResponse[7]);

            return decoded;
        }

        public static string PrettifyModePage_0D(byte[] pageResponse)
        {
            return PrettifyModePage_0D(DecodeModePage_0D(pageResponse));
        }

        public static string PrettifyModePage_0D(ModePage_0D? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_0D page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI CD-ROM parameters page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");
            switch(page.InactivityTimerMultiplier)
            {
                case 0:
                    sb.AppendLine("\tDrive will remain in track hold state a vendor-specified time after a seek or read");
                    break;
                case 1:
                    sb.AppendLine("\tDrive will remain in track hold state 125 ms after a seek or read");
                    break;
                case 2:
                    sb.AppendLine("\tDrive will remain in track hold state 250 ms after a seek or read");
                    break;
                case 3:
                    sb.AppendLine("\tDrive will remain in track hold state 500 ms after a seek or read");
                    break;
                case 4:
                    sb.AppendLine("\tDrive will remain in track hold state 1 second after a seek or read");
                    break;
                case 5:
                    sb.AppendLine("\tDrive will remain in track hold state 2 seconds after a seek or read");
                    break;
                case 6:
                    sb.AppendLine("\tDrive will remain in track hold state 4 seconds after a seek or read");
                    break;
                case 7:
                    sb.AppendLine("\tDrive will remain in track hold state 8 seconds after a seek or read");
                    break;
                case 8:
                    sb.AppendLine("\tDrive will remain in track hold state 16 seconds after a seek or read");
                    break;
                case 9:
                    sb.AppendLine("\tDrive will remain in track hold state 32 seconds after a seek or read");
                    break;
                case 10:
                    sb.AppendLine("\tDrive will remain in track hold state 1 minute after a seek or read");
                    break;
                case 11:
                    sb.AppendLine("\tDrive will remain in track hold state 2 minutes after a seek or read");
                    break;
                case 12:
                    sb.AppendLine("\tDrive will remain in track hold state 4 minutes after a seek or read");
                    break;
                case 13:
                    sb.AppendLine("\tDrive will remain in track hold state 8 minutes after a seek or read");
                    break;
                case 14:
                    sb.AppendLine("\tDrive will remain in track hold state 16 minutes after a seek or read");
                    break;
                case 15:
                    sb.AppendLine("\tDrive will remain in track hold state 32 minutes after a seek or read");
                    break;
            }

            if(page.SecondsPerMinute > 0)
                sb.AppendFormat("\tEach minute has {0} seconds", page.SecondsPerMinute).AppendLine();
            if(page.FramesPerSecond > 0)
                sb.AppendFormat("\tEach second has {0} frames", page.FramesPerSecond).AppendLine();

            return sb.ToString();
        }

        #endregion Mode Page 0x0D: CD-ROM parameteres page

        #region Mode Page 0x01: Read error recovery page for MultiMedia Devices

        /// <summary>
        /// Read error recovery page for MultiMedia Devices
        /// Page code 0x01
        /// 8 bytes in SCSI-2, MMC-1
        /// 12 bytes in MMC-2, MMC-3
        /// </summary>
        public struct ModePage_01_MMC
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Error recovery parameter
            /// </summary>
            public byte Parameter;
            /// <summary>
            /// How many times to retry a read operation
            /// </summary>
            public byte ReadRetryCount;
            /// <summary>
            /// How many times to retry a write operation
            /// </summary>
            public byte WriteRetryCount;
            /// <summary>
            /// Maximum time in ms to use in data error recovery procedures
            /// </summary>
            public ushort RecoveryTimeLimit;
        }

        public static ModePage_01_MMC? DecodeModePage_01_MMC(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x01)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            ModePage_01_MMC decoded = new ModePage_01_MMC();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.Parameter = pageResponse[2];
            decoded.ReadRetryCount = pageResponse[3];

            if(pageResponse.Length < 12)
                return decoded;

            decoded.WriteRetryCount = pageResponse[8];
            decoded.RecoveryTimeLimit = (ushort)((pageResponse[10] << 8) + pageResponse[11]);

            return decoded;
        }

        public static string PrettifyModePage_01_MMC(byte[] pageResponse)
        {
            return PrettifyModePage_01_MMC(DecodeModePage_01_MMC(pageResponse));
        }

        public static string PrettifyModePage_01_MMC(ModePage_01_MMC? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_01_MMC page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Read error recovery page for MultiMedia Devices:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");
            if(page.ReadRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat read operations {0} times", page.ReadRetryCount).AppendLine();

            string AllUsed = "\tAll available recovery procedures will be used.\n";
            string CIRCRetriesUsed = "\tOnly retries and CIRC are used.\n";
            string RetriesUsed = "\tOnly retries are used.\n";
            string RecoveredNotReported = "\tRecovered errors will not be reported.\n";
            string RecoveredReported = "\tRecovered errors will be reported.\n";
            string RecoveredAbort = "\tRecovered errors will be reported and aborted with CHECK CONDITION.\n";
            string UnrecECCAbort = "\tUnrecovered ECC errors will return CHECK CONDITION.";
            string UnrecCIRCAbort = "\tUnrecovered CIRC errors will return CHECK CONDITION.";
            string UnrecECCNotAbort = "\tUnrecovered ECC errors will not abort the transfer.";
            string UnrecCIRCNotAbort = "\tUnrecovered CIRC errors will not abort the transfer.";
            string UnrecECCAbortData = "\tUnrecovered ECC errors will return CHECK CONDITION and the uncorrected data.";
            string UnrecCIRCAbortData = "\tUnrecovered CIRC errors will return CHECK CONDITION and the uncorrected data.";

            switch(page.Parameter)
            {
                case 0x00:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCAbort);
                    break;
                case 0x01:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCAbort);
                    break;
                case 0x04:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCAbort);
                    break;
                case 0x05:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCAbort);
                    break;
                case 0x06:
                    sb.AppendLine(AllUsed + RecoveredAbort + UnrecECCAbort);
                    break;
                case 0x07:
                    sb.AppendLine(RetriesUsed + RecoveredAbort + UnrecCIRCAbort);
                    break;
                case 0x10:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCNotAbort);
                    break;
                case 0x11:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCNotAbort);
                    break;
                case 0x14:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCNotAbort);
                    break;
                case 0x15:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCNotAbort);
                    break;
                case 0x20:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCAbortData);
                    break;
                case 0x21:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCAbortData);
                    break;
                case 0x24:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCAbortData);
                    break;
                case 0x25:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCAbortData);
                    break;
                case 0x26:
                    sb.AppendLine(AllUsed + RecoveredAbort + UnrecECCAbortData);
                    break;
                case 0x27:
                    sb.AppendLine(RetriesUsed + RecoveredAbort + UnrecCIRCAbortData);
                    break;
                case 0x30:
                    goto case 0x10;
                case 0x31:
                    goto case 0x11;
                case 0x34:
                    goto case 0x14;
                case 0x35:
                    goto case 0x15;
                default:
                    sb.AppendFormat("Unknown recovery parameter 0x{0:X2}", page.Parameter).AppendLine();
                    break;
            }

            if(page.WriteRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat write operations {0} times", page.WriteRetryCount).AppendLine();
            if(page.RecoveryTimeLimit > 0)
                sb.AppendFormat("\tDrive will employ a maximum of {0} ms to recover data", page.RecoveryTimeLimit).AppendLine();

            return sb.ToString();
        }

        #endregion Mode Page 0x01: Read error recovery page for MultiMedia Devices

        #region Mode Page 0x07: Verify error recovery page for MultiMedia Devices

        /// <summary>
        /// Verify error recovery page for MultiMedia Devices
        /// Page code 0x07
        /// 8 bytes in SCSI-2, MMC-1
        /// </summary>
        public struct ModePage_07_MMC
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Error recovery parameter
            /// </summary>
            public byte Parameter;
            /// <summary>
            /// How many times to retry a verify operation
            /// </summary>
            public byte VerifyRetryCount;
        }

        public static ModePage_07_MMC? DecodeModePage_07_MMC(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x07)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            ModePage_07_MMC decoded = new ModePage_07_MMC();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.Parameter = pageResponse[2];
            decoded.VerifyRetryCount = pageResponse[3];

            return decoded;
        }

        public static string PrettifyModePage_07_MMC(byte[] pageResponse)
        {
            return PrettifyModePage_07_MMC(DecodeModePage_07_MMC(pageResponse));
        }

        public static string PrettifyModePage_07_MMC(ModePage_07_MMC? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_07_MMC page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Verify error recovery page for MultiMedia Devices:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");
            if(page.VerifyRetryCount > 0)
                sb.AppendFormat("\tDrive will repeat verify operations {0} times", page.VerifyRetryCount).AppendLine();

            string AllUsed = "\tAll available recovery procedures will be used.\n";
            string CIRCRetriesUsed = "\tOnly retries and CIRC are used.\n";
            string RetriesUsed = "\tOnly retries are used.\n";
            string RecoveredNotReported = "\tRecovered errors will not be reported.\n";
            string RecoveredReported = "\tRecovered errors will be reported.\n";
            string RecoveredAbort = "\tRecovered errors will be reported and aborted with CHECK CONDITION.\n";
            string UnrecECCAbort = "\tUnrecovered ECC errors will return CHECK CONDITION.";
            string UnrecCIRCAbort = "\tUnrecovered CIRC errors will return CHECK CONDITION.";
            string UnrecECCNotAbort = "\tUnrecovered ECC errors will not abort the transfer.";
            string UnrecCIRCNotAbort = "\tUnrecovered CIRC errors will not abort the transfer.";
            string UnrecECCAbortData = "\tUnrecovered ECC errors will return CHECK CONDITION and the uncorrected data.";
            string UnrecCIRCAbortData = "\tUnrecovered CIRC errors will return CHECK CONDITION and the uncorrected data.";

            switch(page.Parameter)
            {
                case 0x00:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCAbort);
                    break;
                case 0x01:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCAbort);
                    break;
                case 0x04:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCAbort);
                    break;
                case 0x05:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCAbort);
                    break;
                case 0x06:
                    sb.AppendLine(AllUsed + RecoveredAbort + UnrecECCAbort);
                    break;
                case 0x07:
                    sb.AppendLine(RetriesUsed + RecoveredAbort + UnrecCIRCAbort);
                    break;
                case 0x10:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCNotAbort);
                    break;
                case 0x11:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCNotAbort);
                    break;
                case 0x14:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCNotAbort);
                    break;
                case 0x15:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCNotAbort);
                    break;
                case 0x20:
                    sb.AppendLine(AllUsed + RecoveredNotReported + UnrecECCAbortData);
                    break;
                case 0x21:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredNotReported + UnrecCIRCAbortData);
                    break;
                case 0x24:
                    sb.AppendLine(AllUsed + RecoveredReported + UnrecECCAbortData);
                    break;
                case 0x25:
                    sb.AppendLine(CIRCRetriesUsed + RecoveredReported + UnrecCIRCAbortData);
                    break;
                case 0x26:
                    sb.AppendLine(AllUsed + RecoveredAbort + UnrecECCAbortData);
                    break;
                case 0x27:
                    sb.AppendLine(RetriesUsed + RecoveredAbort + UnrecCIRCAbortData);
                    break;
                case 0x30:
                    goto case 0x10;
                case 0x31:
                    goto case 0x11;
                case 0x34:
                    goto case 0x14;
                case 0x35:
                    goto case 0x15;
                default:
                    sb.AppendFormat("Unknown recovery parameter 0x{0:X2}", page.Parameter).AppendLine();
                    break;
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x07: Verify error recovery page for MultiMedia Devices

        #region Mode Page 0x06: Optical memory page

        /// <summary>
        /// Optical memory page
        /// Page code 0x06
        /// 4 bytes in SCSI-2
        /// </summary>
        public struct ModePage_06
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Report updated block read
            /// </summary>
            public bool RUBR;
        }

        public static ModePage_06? DecodeModePage_06(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x06)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 4)
                return null;

            ModePage_06 decoded = new ModePage_06();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.RUBR |= (pageResponse[2] & 0x01) == 0x01;

            return decoded;
        }

        public static string PrettifyModePage_06(byte[] pageResponse)
        {
            return PrettifyModePage_06(DecodeModePage_06(pageResponse));
        }

        public static string PrettifyModePage_06(ModePage_06? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_06 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI optical memory:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");
            if(page.RUBR)
                sb.AppendLine("\tOn reading an updated block drive will return RECOVERED ERROR");

            return sb.ToString();
        }

        #endregion Mode Page 0x06: Optical memory page

        #region Mode Page 0x2A: CD-ROM capabilities page

        /// <summary>
        /// CD-ROM capabilities page
        /// Page code 0x2A
        /// 16 bytes in OB-U0077C
        /// 20 bytes in SFF-8020i
        /// 22 bytes in MMC-1
        /// 26 bytes in MMC-2
        /// Variable bytes in MMC-3
        /// </summary>
        public struct ModePage_2A
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Drive supports multi-session and/or Photo-CD
            /// </summary>
            public bool MultiSession;
            /// <summary>
            /// Drive is capable of reading sectors in Mode 2 Form 2 format
            /// </summary>
            public bool Mode2Form2;
            /// <summary>
            /// Drive is capable of reading sectors in Mode 2 Form 1 format
            /// </summary>
            public bool Mode2Form1;
            /// <summary>
            /// Drive is capable of playing audio
            /// </summary>
            public bool AudioPlay;
            /// <summary>
            /// Drive can return the ISRC
            /// </summary>
            public bool ISRC;
            /// <summary>
            /// Drive can return the media catalogue number
            /// </summary>
            public bool UPC;
            /// <summary>
            /// Drive can return C2 pointers
            /// </summary>
            public bool C2Pointer;
            /// <summary>
            /// Drive can read, deinterlave and correct R-W subchannels
            /// </summary>
            public bool DeinterlaveSubchannel;
            /// <summary>
            /// Drive can read interleaved and uncorrected R-W subchannels
            /// </summary>
            public bool Subchannel;
            /// <summary>
            /// Drive can continue from a loss of streaming on audio reading
            /// </summary>
            public bool AccurateCDDA;
            /// <summary>
            /// Audio can be read as digital data
            /// </summary>
            public bool CDDACommand;
            /// <summary>
            /// Loading Mechanism Type
            /// </summary>
            public byte LoadingMechanism;
            /// <summary>
            /// Drive can eject discs
            /// </summary>
            public bool Eject;
            /// <summary>
            /// Drive's optional prevent jumper status
            /// </summary>
            public bool PreventJumper;
            /// <summary>
            /// Current lock status
            /// </summary>
            public bool LockState;
            /// <summary>
            /// Drive can lock media
            /// </summary>
            public bool Lock;
            /// <summary>
            /// Each channel can be muted independently
            /// </summary>
            public bool SeparateChannelMute;
            /// <summary>
            /// Each channel's volume can be controlled independently
            /// </summary>
            public bool SeparateChannelVolume;
            /// <summary>
            /// Maximum drive speed in Kbytes/second
            /// </summary>
            public ushort MaximumSpeed;
            /// <summary>
            /// Supported volume levels
            /// </summary>
            public ushort SupportedVolumeLevels;
            /// <summary>
            /// Buffer size in Kbytes
            /// </summary>
            public ushort BufferSize;
            /// <summary>
            /// Current drive speed in Kbytes/second
            /// </summary>
            public ushort CurrentSpeed;

            public bool Method2;
            public bool ReadCDRW;
            public bool ReadCDR;
            public bool WriteCDRW;
            public bool WriteCDR;
            public bool DigitalPort2;
            public bool DigitalPort1;
            public bool Composite;
            public bool SSS;
            public bool SDP;
            public byte Length;
            public bool LSBF;
            public bool RCK;
            public bool BCK;

            public bool TestWrite;
            public ushort MaxWriteSpeed;
            public ushort CurrentWriteSpeed;

            public bool ReadBarcode;

            public bool ReadDVDRAM;
            public bool ReadDVDR;
            public bool ReadDVDROM;
            public bool WriteDVDRAM;
            public bool WriteDVDR;
            public bool LeadInPW;
            public bool SCC;
            public ushort CMRSupported;

            public bool BUF;
            public byte RotationControlSelected;
            public ushort CurrentWriteSpeedSelected;
            public ModePage_2A_WriteDescriptor[] WriteSpeedPerformanceDescriptors;
        }

        public struct ModePage_2A_WriteDescriptor
        {
            public byte RotationControl;
            public ushort WriteSpeed;
        }

        public static ModePage_2A? DecodeModePage_2A(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x2A)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 16)
                return null;

            ModePage_2A decoded = new ModePage_2A();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.AudioPlay |= (pageResponse[4] & 0x01) == 0x01;
            decoded.Mode2Form1 |= (pageResponse[4] & 0x10) == 0x10;
            decoded.Mode2Form2 |= (pageResponse[4] & 0x20) == 0x20;
            decoded.MultiSession |= (pageResponse[4] & 0x40) == 0x40;

            decoded.CDDACommand |= (pageResponse[5] & 0x01) == 0x01;
            decoded.AccurateCDDA |= (pageResponse[5] & 0x02) == 0x02;
            decoded.Subchannel |= (pageResponse[5] & 0x04) == 0x04;
            decoded.DeinterlaveSubchannel |= (pageResponse[5] & 0x08) == 0x08;
            decoded.C2Pointer |= (pageResponse[5] & 0x10) == 0x10;
            decoded.UPC |= (pageResponse[5] & 0x20) == 0x20;
            decoded.ISRC |= (pageResponse[5] & 0x40) == 0x40;

            decoded.LoadingMechanism = (byte)((pageResponse[6] & 0xE0) >> 5);
            decoded.Lock |= (pageResponse[6] & 0x01) == 0x01;
            decoded.LockState |= (pageResponse[6] & 0x02) == 0x02;
            decoded.PreventJumper |= (pageResponse[6] & 0x04) == 0x04;
            decoded.Eject |= (pageResponse[6] & 0x08) == 0x08;

            decoded.SeparateChannelVolume |= (pageResponse[7] & 0x01) == 0x01;
            decoded.SeparateChannelMute |= (pageResponse[7] & 0x02) == 0x02;

            decoded.MaximumSpeed = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.SupportedVolumeLevels = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.BufferSize = (ushort)((pageResponse[12] << 8) + pageResponse[13]);
            decoded.CurrentSpeed = (ushort)((pageResponse[14] << 8) + pageResponse[15]);

            if(pageResponse.Length < 20)
                return decoded;

            decoded.Method2 |= (pageResponse[2] & 0x04) == 0x04;
            decoded.ReadCDRW |= (pageResponse[2] & 0x02) == 0x02;
            decoded.ReadCDR |= (pageResponse[2] & 0x01) == 0x01;

            decoded.WriteCDRW |= (pageResponse[3] & 0x02) == 0x02;
            decoded.WriteCDR |= (pageResponse[3] & 0x01) == 0x01;

            decoded.Composite |= (pageResponse[4] & 0x02) == 0x02;
            decoded.DigitalPort1 |= (pageResponse[4] & 0x04) == 0x04;
            decoded.DigitalPort2 |= (pageResponse[4] & 0x08) == 0x08;

            decoded.SDP |= (pageResponse[7] & 0x04) == 0x04;
            decoded.SSS |= (pageResponse[7] & 0x08) == 0x08;

            decoded.Length = (byte)((pageResponse[17] & 0x30) >> 4);
            decoded.LSBF |= (pageResponse[17] & 0x08) == 0x08;
            decoded.RCK |= (pageResponse[17] & 0x04) == 0x04;
            decoded.BCK |= (pageResponse[17] & 0x02) == 0x02;

            if(pageResponse.Length < 22)
                return decoded;

            decoded.TestWrite |= (pageResponse[3] & 0x04) == 0x04;
            decoded.MaxWriteSpeed = (ushort)((pageResponse[18] << 8) + pageResponse[19]);
            decoded.CurrentWriteSpeed = (ushort)((pageResponse[20] << 8) + pageResponse[21]);

            decoded.ReadBarcode |= (pageResponse[5] & 0x80) == 0x80;

            if(pageResponse.Length < 26)
                return decoded;

            decoded.ReadDVDRAM |= (pageResponse[2] & 0x20) == 0x20;
            decoded.ReadDVDR |= (pageResponse[2] & 0x10) == 0x10;
            decoded.ReadDVDROM |= (pageResponse[2] & 0x08) == 0x08;

            decoded.WriteDVDRAM |= (pageResponse[3] & 0x20) == 0x20;
            decoded.WriteDVDR |= (pageResponse[3] & 0x10) == 0x10;

            decoded.LeadInPW |= (pageResponse[3] & 0x20) == 0x20;
            decoded.SCC |= (pageResponse[3] & 0x10) == 0x10;

            decoded.CMRSupported = (ushort)((pageResponse[22] << 8) + pageResponse[23]);

            if(pageResponse.Length < 32)
                return decoded;

            decoded.BUF |= (pageResponse[4] & 0x80) == 0x80;
            decoded.RotationControlSelected = (byte)(pageResponse[27] & 0x03);
            decoded.CurrentWriteSpeedSelected = (ushort)((pageResponse[28] << 8) + pageResponse[29]);

            ushort descriptors = (ushort)((pageResponse.Length - 32) / 4);
            decoded.WriteSpeedPerformanceDescriptors = new ModePage_2A_WriteDescriptor[descriptors];

            for(int i = 0; i < descriptors; i++)
            {
                decoded.WriteSpeedPerformanceDescriptors[i] = new ModePage_2A_WriteDescriptor();
                decoded.WriteSpeedPerformanceDescriptors[i].RotationControl = (byte)(pageResponse[1 + 32 + i * 4] & 0x07);
                decoded.WriteSpeedPerformanceDescriptors[i].WriteSpeed = (ushort)((pageResponse[2 + 32 + i * 4] << 8) + pageResponse[3 + 32 + i * 4]);
            }

            return decoded;
        }

        public static string PrettifyModePage_2A(byte[] pageResponse)
        {
            return PrettifyModePage_2A(DecodeModePage_2A(pageResponse));
        }

        public static string PrettifyModePage_2A(ModePage_2A? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_2A page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI CD-ROM capabilities page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.AudioPlay)
                sb.AppendLine("\tDrive can play audio");
            if(page.Mode2Form1)
                sb.AppendLine("\tDrive can read sectors in Mode 2 Form 1 format");
            if(page.Mode2Form2)
                sb.AppendLine("\tDrive can read sectors in Mode 2 Form 2 format");
            if(page.MultiSession)
                sb.AppendLine("\tDrive supports multi-session discs and/or Photo-CD");

            if(page.CDDACommand)
                sb.AppendLine("\tDrive can read digital audio");
            if(page.AccurateCDDA)
                sb.AppendLine("\tDrive can continue from streaming loss");
            if(page.Subchannel)
                sb.AppendLine("\tDrive can read uncorrected and interleaved R-W subchannels");
            if(page.DeinterlaveSubchannel)
                sb.AppendLine("\tDrive can read, deinterleave and correct R-W subchannels");
            if(page.C2Pointer)
                sb.AppendLine("\tDrive supports C2 pointers");
            if(page.UPC)
                sb.AppendLine("\tDrive can read Media Catalogue Number");
            if(page.ISRC)
                sb.AppendLine("\tDrive can read ISRC");

            switch(page.LoadingMechanism)
            {
                case 0:
                    sb.AppendLine("\tDrive uses media caddy");
                    break;
                case 1:
                    sb.AppendLine("\tDrive uses a tray");
                    break;
                case 2:
                    sb.AppendLine("\tDrive is pop-up");
                    break;
                case 4:
                    sb.AppendLine("\tDrive is a changer with individually changeable discs");
                    break;
                case 5:
                    sb.AppendLine("\tDrive is a changer using cartridges");
                    break;
                default:
                    sb.AppendFormat("\tDrive uses unknown loading mechanism type {0}", page.LoadingMechanism).AppendLine();
                    break;
            }

            if(page.Lock)
                sb.AppendLine("\tDrive can lock media");
            if(page.PreventJumper)
            {
                sb.AppendLine("\tDrive power ups locked");
                if(page.LockState)
                    sb.AppendLine("\tDrive is locked, media cannot be ejected or inserted");
                else
                    sb.AppendLine("\tDrive is not locked, media can be ejected and inserted");
            }
            else
            {
                if(page.LockState)
                    sb.AppendLine("\tDrive is locked, media cannot be ejected, but if empty, can be inserted");
                else
                    sb.AppendLine("\tDrive is not locked, media can be ejected and inserted");
            }
            if(page.Eject)
                sb.AppendLine("\tDrive can eject media");

            if(page.SeparateChannelMute)
                sb.AppendLine("\tEach channel can be muted independently");
            if(page.SeparateChannelVolume)
                sb.AppendLine("\tEach channel's volume can be controlled independently");

            if(page.SupportedVolumeLevels > 0)
                sb.AppendFormat("\tDrive supports {0} volume levels", page.SupportedVolumeLevels).AppendLine();
            if(page.BufferSize > 0)
                sb.AppendFormat("\tDrive has {0} Kbyte of buffer", page.BufferSize).AppendLine();
            if(page.MaximumSpeed > 0)
                sb.AppendFormat("\tDrive's maximum reading speed is {0} Kbyte/sec.", page.MaximumSpeed).AppendLine();
            if(page.CurrentSpeed > 0)
                sb.AppendFormat("\tDrive's current reading speed is {0} Kbyte/sec.", page.CurrentSpeed).AppendLine();

            if(page.ReadCDR)
            {
                if(page.WriteCDR)
                    sb.AppendLine("\tDrive can read and write CD-R");
                else
                    sb.AppendLine("\tDrive can read CD-R");

                if(page.Method2)
                    sb.AppendLine("\tDrive supports reading CD-R packet media");
            }

            if(page.ReadCDRW)
            {
                if(page.WriteCDRW)
                    sb.AppendLine("\tDrive can read and write CD-RW");
                else
                    sb.AppendLine("\tDrive can read CD-RW");
            }

            if(page.ReadDVDROM)
                sb.AppendLine("\tDrive can read DVD-ROM");
            if(page.ReadDVDR)
            {
                if(page.WriteDVDR)
                    sb.AppendLine("\tDrive can read and write DVD-R");
                else
                    sb.AppendLine("\tDrive can read DVD-R");
            }
            if(page.ReadDVDRAM)
            {
                if(page.WriteDVDRAM)
                    sb.AppendLine("\tDrive can read and write DVD-RAM");
                else
                    sb.AppendLine("\tDrive can read DVD-RAM");
            }

            if(page.Composite)
                sb.AppendLine("\tDrive can deliver a composite audio and video data stream");
            if(page.DigitalPort1)
                sb.AppendLine("\tDrive supports IEC-958 digital output on port 1");
            if(page.DigitalPort2)
                sb.AppendLine("\tDrive supports IEC-958 digital output on port 2");

            if(page.SDP)
                sb.AppendLine("\tDrive contains a changer that can report the exact contents of the slots");
            if(page.CurrentWriteSpeedSelected > 0)
            {
                if(page.RotationControlSelected == 0)
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec. in CLV mode", page.CurrentWriteSpeedSelected).AppendLine();
                else if(page.RotationControlSelected == 1)
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec. in pure CAV mode", page.CurrentWriteSpeedSelected).AppendLine();
            }
            else
            {
                if(page.MaxWriteSpeed > 0)
                    sb.AppendFormat("\tDrive's maximum writing speed is {0} Kbyte/sec.", page.MaxWriteSpeed).AppendLine();
                if(page.CurrentWriteSpeed > 0)
                    sb.AppendFormat("\tDrive's current writing speed is {0} Kbyte/sec.", page.CurrentWriteSpeed).AppendLine();
            }

            if(page.WriteSpeedPerformanceDescriptors != null)
            {
                foreach(ModePage_2A_WriteDescriptor descriptor in page.WriteSpeedPerformanceDescriptors)
                {
                    if(descriptor.WriteSpeed > 0)
                    {
                        if(descriptor.RotationControl == 0)
                            sb.AppendFormat("\tDrive supports writing at {0} Kbyte/sec. in CLV mode", descriptor.WriteSpeed).AppendLine();
                        else if(descriptor.RotationControl == 1)
                            sb.AppendFormat("\tDrive supports writing at is {0} Kbyte/sec. in pure CAV mode", descriptor.WriteSpeed).AppendLine();
                    }
                }
            }

            if(page.TestWrite)
                sb.AppendLine("\tDrive supports test writing");

            if(page.ReadBarcode)
                sb.AppendLine("\tDrive can read barcode");

            if(page.SCC)
                sb.AppendLine("\tDrive can read both sides of a disc");
            if(page.LeadInPW)
                sb.AppendLine("\tDrive an read raw R-W subchannel from the Lead-In");

            if(page.CMRSupported == 1)
                sb.AppendLine("\tDrive supports DVD CSS and/or DVD CPPM");

            if(page.BUF)
                sb.AppendLine("\tDrive supports buffer under-run free recording");

            return sb.ToString();
        }

        #endregion Mode Page 0x2A: CD-ROM capabilities page

        #region Mode Page 0x1C: Informational exceptions control page

        /// <summary>
        /// Informational exceptions control page
        /// Page code 0x1C
        /// 12 bytes in SPC-1, SPC-2, SPC-3, SPC-4
        /// </summary>
        public struct ModePage_1C
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Informational exception operations should not affect performance
            /// </summary>
            public bool Perf;
            /// <summary>
            /// Disable informational exception operations
            /// </summary>
            public bool DExcpt;
            /// <summary>
            /// Create a test device failure at next interval time
            /// </summary>
            public bool Test;
            /// <summary>
            /// Log informational exception conditions
            /// </summary>
            public bool LogErr;
            /// <summary>
            /// Method of reporting informational exceptions
            /// </summary>
            public byte MRIE;
            /// <summary>
            /// 100 ms period to report an informational exception condition
            /// </summary>
            public uint IntervalTimer;
            /// <summary>
            /// How many times to report informational exceptions
            /// </summary>
            public uint ReportCount;

            /// <summary>
            /// Enable background functions
            /// </summary>
            public bool EBF;
            /// <summary>
            /// Warning reporting enabled
            /// </summary>
            public bool EWasc;

            /// <summary>
            /// Enable reporting of background self-test errors
            /// </summary>
            public bool EBACKERR;
        }

        public static ModePage_1C? DecodeModePage_1C(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x1C)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            ModePage_1C decoded = new ModePage_1C();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.Perf |= (pageResponse[2] & 0x80) == 0x80;
            decoded.DExcpt |= (pageResponse[2] & 0x08) == 0x08;
            decoded.Test |= (pageResponse[2] & 0x04) == 0x04;
            decoded.LogErr |= (pageResponse[2] & 0x01) == 0x01;

            decoded.MRIE = (byte)(pageResponse[3] & 0x0F);

            decoded.IntervalTimer = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) + pageResponse[7]);

            decoded.EBF |= (pageResponse[2] & 0x20) == 0x20;
            decoded.EWasc |= (pageResponse[2] & 0x10) == 0x10;

            decoded.EBACKERR |= (pageResponse[2] & 0x02) == 0x02;

            if(pageResponse.Length >= 12)
                decoded.ReportCount = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);

            return decoded;
        }

        public static string PrettifyModePage_1C(byte[] pageResponse)
        {
            return PrettifyModePage_1C(DecodeModePage_1C(pageResponse));
        }

        public static string PrettifyModePage_1C(ModePage_1C? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_1C page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Informational exceptions control page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.DExcpt)
                sb.AppendLine("\tInformational exceptions are disabled");
            else
            {
                sb.AppendLine("\tInformational exceptions are enabled");

                switch(page.MRIE)
                {
                    case 0:
                        sb.AppendLine("\tNo reporting of informational exception condition");
                        break;
                    case 1:
                        sb.AppendLine("\tAsynchronous event reporting of informational exceptions");
                        break;
                    case 2:
                        sb.AppendLine("\tGenerate unit attention on informational exceptions");
                        break;
                    case 3:
                        sb.AppendLine("\tConditionally generate recovered error on informational exceptions");
                        break;
                    case 4:
                        sb.AppendLine("\tUnconditionally generate recovered error on informational exceptions");
                        break;
                    case 5:
                        sb.AppendLine("\tGenerate no sense on informational exceptions");
                        break;
                    case 6:
                        sb.AppendLine("\tOnly report informational exception condition on request");
                        break;
                    default:
                        sb.AppendFormat("\tUnknown method of reporting {0}", page.MRIE).AppendLine();
                        break;
                }

                if(page.Perf)
                    sb.AppendLine("\tInformational exceptions reporting should not affect drive performance");
                if(page.Test)
                    sb.AppendLine("\tA test informational exception will raise on next timer");
                if(page.LogErr)
                    sb.AppendLine("\tDrive shall log informational exception conditions");

                if(page.IntervalTimer > 0)
                {
                    if(page.IntervalTimer == 0xFFFFFFFF)
                        sb.AppendLine("\tTimer interval is vendor-specific");
                    else
                        sb.AppendFormat("\tTimer interval is {0} ms", page.IntervalTimer * 100).AppendLine();
                }

                if(page.ReportCount > 0)
                    sb.AppendFormat("\tInformational exception conditions will be reported a maximum of {0} times", page.ReportCount);
            }

            if(page.EWasc)
                sb.AppendLine("\tWarning reporting is enabled");
            if(page.EBF)
                sb.AppendLine("\tBackground functions are enabled");
            if(page.EBACKERR)
                sb.AppendLine("\tDrive will report background self-test errors");

            return sb.ToString();
        }

        #endregion Mode Page 0x1C: Informational exceptions control page

        #region Mode Page 0x1A: Power condition page

        /// <summary>
        /// Power condition page
        /// Page code 0x1A
        /// 12 bytes in SPC-1, SPC-2, SPC-3, SPC-4
        /// 40 bytes in SPC-5
        /// </summary>
        public struct ModePage_1A
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Idle timer activated
            /// </summary>
            public bool Idle;
            /// <summary>
            /// Standby timer activated
            /// </summary>
            public bool Standby;
            /// <summary>
            /// Idle timer
            /// </summary>
            public uint IdleTimer;
            /// <summary>
            /// Standby timer
            /// </summary>
            public uint StandbyTimer;

            /// <summary>
            /// Interactions between background functions and power management
            /// </summary>
            public byte PM_BG_Precedence;
            /// <summary>
            /// Standby timer Y activated
            /// </summary>
            public bool Standby_Y;
            /// <summary>
            /// Idle timer B activated
            /// </summary>
            public bool Idle_B;
            /// <summary>
            /// Idle timer C activated
            /// </summary>
            public bool Idle_C;
            /// <summary>
            /// Idle timer B
            /// </summary>
            public uint IdleTimer_B;
            /// <summary>
            /// Idle timer C
            /// </summary>
            public uint IdleTimer_C;
            /// <summary>
            /// Standby timer Y
            /// </summary>
            public uint StandbyTimer_Y;
            public byte CCF_Idle;
            public byte CCF_Standby;
            public byte CCF_Stopped;
        }

        public static ModePage_1A? DecodeModePage_1A(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x1A)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 12)
                return null;

            ModePage_1A decoded = new ModePage_1A();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.Standby |= (pageResponse[3] & 0x01) == 0x01;
            decoded.Idle |= (pageResponse[3] & 0x02) == 0x02;

            decoded.IdleTimer = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) + pageResponse[7]);
            decoded.StandbyTimer = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);

            if(pageResponse.Length < 40)
                return decoded;

            decoded.PM_BG_Precedence = (byte)((pageResponse[2] & 0xC0) >> 6);
            decoded.Standby_Y |= (pageResponse[2] & 0x01) == 0x01;
            decoded.Idle_B |= (pageResponse[3] & 0x04) == 0x04;
            decoded.Idle_C |= (pageResponse[3] & 0x08) == 0x08;

            decoded.IdleTimer_B = (uint)((pageResponse[12] << 24) + (pageResponse[13] << 16) + (pageResponse[14] << 8) + pageResponse[15]);
            decoded.IdleTimer_C = (uint)((pageResponse[16] << 24) + (pageResponse[17] << 16) + (pageResponse[18] << 8) + pageResponse[19]);
            decoded.StandbyTimer_Y = (uint)((pageResponse[20] << 24) + (pageResponse[21] << 16) + (pageResponse[22] << 8) + pageResponse[23]);

            decoded.CCF_Idle = (byte)((pageResponse[39] & 0xC0) >> 6);
            decoded.CCF_Standby = (byte)((pageResponse[39] & 0x30) >> 4);
            decoded.CCF_Stopped = (byte)((pageResponse[39] & 0x0C) >> 2);

            return decoded;
        }

        public static string PrettifyModePage_1A(byte[] pageResponse)
        {
            return PrettifyModePage_1A(DecodeModePage_1A(pageResponse));
        }

        public static string PrettifyModePage_1A(ModePage_1A? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_1A page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Power condition page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if((page.Standby && page.StandbyTimer > 0) ||
                (page.Standby_Y && page.StandbyTimer_Y > 0))
            {
                if(page.Standby && page.StandbyTimer > 0)
                    sb.AppendFormat("\tStandby timer Z is set to {0} ms", page.StandbyTimer * 100).AppendLine();
                if(page.Standby_Y && page.StandbyTimer_Y > 0)
                    sb.AppendFormat("\tStandby timer Y is set to {0} ms", page.StandbyTimer_Y * 100).AppendLine();
            }
            else
                sb.AppendLine("\tDrive will not enter standy mode");

            if((page.Idle && page.IdleTimer > 0) ||
                (page.Idle_B && page.IdleTimer_B > 0) ||
                (page.Idle_C && page.IdleTimer_C > 0))
            {
                if(page.Idle && page.IdleTimer > 0)
                    sb.AppendFormat("\tIdle timer A is set to {0} ms", page.IdleTimer * 100).AppendLine();
                if(page.Idle_B && page.IdleTimer_B > 0)
                    sb.AppendFormat("\tIdle timer B is set to {0} ms", page.IdleTimer_B * 100).AppendLine();
                if(page.Idle_C && page.IdleTimer_C > 0)
                    sb.AppendFormat("\tIdle timer C is set to {0} ms", page.IdleTimer_C * 100).AppendLine();
            }
            else
                sb.AppendLine("\tDrive will not enter idle mode");

            switch(page.PM_BG_Precedence)
            {
                case 0:
                    break;
                case 1:
                    sb.AppendLine("\tPerforming background functions take precedence over maintaining low power conditions");
                    break;
                case 2:
                    sb.AppendLine("\tMaintaining low power conditions take precedence over performing background functions");
                    break;
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x1A: Power condition page

        #region Mode Page 0x0A subpage 0x01: Control Extension mode page

        /// <summary>
        /// Control Extension mode page
        /// Page code 0x0A
        /// Subpage code 0x01
        /// 32 bytes in SPC-3, SPC-4, SPC-5
        /// </summary>
        public struct ModePage_0A_S01
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Timestamp outside this standard
            /// </summary>
            public bool TCMOS;
            /// <summary>
            /// SCSI precedence
            /// </summary>
            public bool SCSIP;
            /// <summary>
            /// Implicit Asymmetric Logical Unit Access Enabled
            /// </summary>
            public bool IALUAE;
            /// <summary>
            /// Initial task priority
            /// </summary>
            public byte InitialPriority;

            /// <summary>
            /// Device life control disabled
            /// </summary>
            public bool DLC;
            /// <summary>
            /// Maximum size of SENSE data in bytes
            /// </summary>
            public byte MaximumSenseLength;
        }

        public static ModePage_0A_S01? DecodeModePage_0A_S01(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) != 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x0A)
                return null;

            if(pageResponse[1] != 0x01)
                return null;

            if(((pageResponse[2] << 8) + pageResponse[3] + 4) != pageResponse.Length)
                return null;

            if(pageResponse.Length < 32)
                return null;

            ModePage_0A_S01 decoded = new ModePage_0A_S01();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.IALUAE |= (pageResponse[4] & 0x01) == 0x01;
            decoded.SCSIP |= (pageResponse[4] & 0x02) == 0x02;
            decoded.TCMOS |= (pageResponse[4] & 0x04) == 0x04;

            decoded.InitialPriority = (byte)(pageResponse[5] & 0x0F);

            return decoded;
        }

        public static string PrettifyModePage_0A_S01(byte[] pageResponse)
        {
            return PrettifyModePage_0A_S01(DecodeModePage_0A_S01(pageResponse));
        }

        public static string PrettifyModePage_0A_S01(ModePage_0A_S01? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_0A_S01 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Control extension page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.TCMOS)
            {
                sb.Append("\tTimestamp can be initialized by methods outside of the SCSI standards");

                if(page.SCSIP)
                    sb.Append(", but SCSI's SET TIMESTAMP shall take precedence over them");

                sb.AppendLine();
            }

            if(page.IALUAE)
                sb.AppendLine("\tImplicit Asymmetric Logical Unit Access is enabled");

            sb.AppendFormat("\tInitial priority is {0}", page.InitialPriority).AppendLine();

            if(page.DLC)
                sb.AppendLine("\tDevice will not degrade performance to extend its life");

            if(page.MaximumSenseLength > 0)
                sb.AppendFormat("\tMaximum sense data would be {0} bytes", page.MaximumSenseLength).AppendLine();

            return sb.ToString();
        }

        #endregion Mode Page 0x0A subpage 0x01: Control Extension mode page

        #region Mode Page 0x1A subpage 0x01: Power Consumption mode page

        /// <summary>
        /// Power Consumption mode page
        /// Page code 0x1A
        /// Subpage code 0x01
        /// 16 bytes in SPC-5
        /// </summary>
        public struct ModePage_1A_S01
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Active power level
            /// </summary>
            public byte ActiveLevel;
            /// <summary>
            /// Power Consumption VPD identifier in use
            /// </summary>
            public byte PowerConsumptionIdentifier;
        }

        public static ModePage_1A_S01? DecodeModePage_1A_S01(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) != 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x1A)
                return null;

            if(pageResponse[1] != 0x01)
                return null;

            if(((pageResponse[2] << 8) + pageResponse[3] + 4) != pageResponse.Length)
                return null;

            if(pageResponse.Length < 16)
                return null;

            ModePage_1A_S01 decoded = new ModePage_1A_S01();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.ActiveLevel = (byte)(pageResponse[6] & 0x03);
            decoded.PowerConsumptionIdentifier = pageResponse[7];

            return decoded;
        }

        public static string PrettifyModePage_1A_S01(byte[] pageResponse)
        {
            return PrettifyModePage_1A_S01(DecodeModePage_1A_S01(pageResponse));
        }

        public static string PrettifyModePage_1A_S01(ModePage_1A_S01? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_1A_S01 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Power Consumption page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            switch(page.ActiveLevel)
            {
                case 0:
                    sb.AppendFormat("\tDevice power consumption is dictated by identifier {0} of Power Consumption VPD", page.PowerConsumptionIdentifier).AppendLine();
                    break;
                case 1:
                    sb.AppendLine("\tDevice is in highest relative power consumption level");
                    break;
                case 2:
                    sb.AppendLine("\tDevice is in intermediate relative power consumption level");
                    break;
                case 3:
                    sb.AppendLine("\tDevice is in lowest relative power consumption level");
                    break;
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x1A subpage 0x01: Power Consumption mode page

        #region Mode Page 0x10: XOR control mode page

        /// <summary>
        /// XOR control mode page
        /// Page code 0x10
        /// 24 bytes in SBC-1, SBC-2
        /// </summary>
        public struct ModePage_10
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Disables XOR operations
            /// </summary>
            public bool XORDIS;
            /// <summary>
            /// Maximum transfer length in blocks for a XOR command
            /// </summary>
            public uint MaxXorWrite;
            /// <summary>
            /// Maximum regenerate length in blocks
            /// </summary>
            public uint MaxRegenSize;
            /// <summary>
            /// Maximum transfer length in blocks for READ during a rebuild
            /// </summary>
            public uint MaxRebuildRead;
            /// <summary>
            /// Minimum time in ms between READs during a rebuild
            /// </summary>
            public ushort RebuildDelay;
        }

        public static ModePage_10? DecodeModePage_10(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x10)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 24)
                return null;

            ModePage_10 decoded = new ModePage_10();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.XORDIS |= (pageResponse[2] & 0x02) == 0x02;
            decoded.MaxXorWrite = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) + pageResponse[7]);
            decoded.MaxRegenSize = (uint)((pageResponse[12] << 24) + (pageResponse[13] << 16) + (pageResponse[14] << 8) + pageResponse[15]);
            decoded.MaxRebuildRead = (uint)((pageResponse[16] << 24) + (pageResponse[17] << 16) + (pageResponse[18] << 8) + pageResponse[19]);
            decoded.RebuildDelay = (ushort)((pageResponse[22] << 8) + pageResponse[23]);

            return decoded;
        }

        public static string PrettifyModePage_10(byte[] pageResponse)
        {
            return PrettifyModePage_10(DecodeModePage_10(pageResponse));
        }

        public static string PrettifyModePage_10(ModePage_10? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_10 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI XOR control mode page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.XORDIS)
                sb.AppendLine("\tXOR operations are disabled");
            else
            {
                if(page.MaxXorWrite > 0)
                    sb.AppendFormat("\tDrive accepts a maximum of {0} blocks in a single XOR WRITE command", page.MaxXorWrite).AppendLine();
                if(page.MaxRegenSize > 0)
                    sb.AppendFormat("\tDrive accepts a maximum of {0} blocks in a REGENERATE command", page.MaxRegenSize).AppendLine();
                if(page.MaxRebuildRead > 0)
                    sb.AppendFormat("\tDrive accepts a maximum of {0} blocks in a READ command during rebuild", page.MaxRebuildRead).AppendLine();
                if(page.RebuildDelay > 0)
                    sb.AppendFormat("\tDrive needs a minimum of {0} ms between READ commands during rebuild", page.RebuildDelay).AppendLine();
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x10: XOR control mode page

        #region Mode Page 0x1C subpage 0x01: Background Control mode page

        /// <summary>
        /// Background Control mode page
        /// Page code 0x1A
        /// Subpage code 0x01
        /// 16 bytes in SPC-5
        /// </summary>
        public struct ModePage_1C_S01
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Suspend on log full
            /// </summary>
            public bool S_L_Full;
            /// <summary>
            /// Log only when intervention required
            /// </summary>
            public bool LOWIR;
            /// <summary>
            /// Enable background medium scan
            /// </summary>
            public bool En_Bms;
            /// <summary>
            /// Enable background pre-scan
            /// </summary>
            public bool En_Ps;
            /// <summary>
            /// Time in hours between background medium scans
            /// </summary>
            public ushort BackgroundScanInterval;
            /// <summary>
            /// Maximum time in hours for a background pre-scan to complete
            /// </summary>
            public ushort BackgroundPrescanTimeLimit;
            /// <summary>
            /// Minimum time in ms being idle before resuming a background scan
            /// </summary>
            public ushort MinIdleBeforeBgScan;
            /// <summary>
            /// Maximum time in ms to start processing commands while performing a background scan
            /// </summary>
            public ushort MaxTimeSuspendBgScan;
        }

        public static ModePage_1C_S01? DecodeModePage_1C_S01(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) != 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x1C)
                return null;

            if(pageResponse[1] != 0x01)
                return null;

            if(((pageResponse[2] << 8) + pageResponse[3] + 4) != pageResponse.Length)
                return null;

            if(pageResponse.Length < 16)
                return null;

            ModePage_1C_S01 decoded = new ModePage_1C_S01();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.S_L_Full |= (pageResponse[4] & 0x04) == 0x04;
            decoded.LOWIR |= (pageResponse[4] & 0x02) == 0x02;
            decoded.En_Bms |= (pageResponse[4] & 0x01) == 0x01;
            decoded.En_Ps |= (pageResponse[5] & 0x01) == 0x01;

            decoded.BackgroundScanInterval = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.BackgroundPrescanTimeLimit = (ushort)((pageResponse[8] << 8) + pageResponse[9]);
            decoded.MinIdleBeforeBgScan = (ushort)((pageResponse[10] << 8) + pageResponse[11]);
            decoded.MaxTimeSuspendBgScan = (ushort)((pageResponse[12] << 8) + pageResponse[13]);

            return decoded;
        }

        public static string PrettifyModePage_1C_S01(byte[] pageResponse)
        {
            return PrettifyModePage_1C_S01(DecodeModePage_1C_S01(pageResponse));
        }

        public static string PrettifyModePage_1C_S01(ModePage_1C_S01? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_1C_S01 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Background Control page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.S_L_Full)
                sb.AppendLine("\tBackground scans will be halted if log is full");
            if(page.LOWIR)
                sb.AppendLine("\tBackground scans will only be logged if they require intervention");
            if(page.En_Bms)
                sb.AppendLine("\tBackground medium scans are enabled");
            if(page.En_Ps)
                sb.AppendLine("\tBackground pre-scans are enabled");

            if(page.BackgroundScanInterval > 0)
                sb.AppendFormat("\t{0} hours shall be between the start of a background scan operation and the next", page.BackgroundScanInterval).AppendLine();

            if(page.BackgroundPrescanTimeLimit > 0)
                sb.AppendFormat("\tBackgroun pre-scan operations can take a maximum of {0} hours", page.BackgroundPrescanTimeLimit).AppendLine();

            if(page.MinIdleBeforeBgScan > 0)
                sb.AppendFormat("\tAt least {0} ms must be idle before resuming a suspended background scan operation", page.MinIdleBeforeBgScan).AppendLine();

            if(page.MaxTimeSuspendBgScan > 0)
                sb.AppendFormat("\tAt most {0} ms must be before suspending a background scan operation and processing received commands", page.MaxTimeSuspendBgScan).AppendLine();

            return sb.ToString();
        }

        #endregion Mode Page 0x1C subpage 0x01: Background Control mode page

        #region Mode Page 0x0F: Data compression page

        /// <summary>
        /// Data compression page
        /// Page code 0x0F
        /// 16 bytes in SSC-1, SSC-2, SSC-3
        /// </summary>
        public struct ModePage_0F
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Data compression enabled
            /// </summary>
            public bool DCE;
            /// <summary>
            /// Data compression capable
            /// </summary>
            public bool DCC;
            /// <summary>
            /// Data decompression enabled
            /// </summary>
            public bool DDE;
            /// <summary>
            /// Report exception on decompression
            /// </summary>
            public byte RED;
            /// <summary>
            /// Compression algorithm
            /// </summary>
            public uint CompressionAlgo;
            /// <summary>
            /// Decompression algorithm
            /// </summary>
            public uint DecompressionAlgo;
        }

        public static ModePage_0F? DecodeModePage_0F(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x0F)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 16)
                return null;

            ModePage_0F decoded = new ModePage_0F();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.DCE |= (pageResponse[2] & 0x80) == 0x80;
            decoded.DCC |= (pageResponse[2] & 0x40) == 0x40;
            decoded.DDE |= (pageResponse[3] & 0x80) == 0x80;
            decoded.RED = (byte)((pageResponse[3] & 0x60) >> 5);

            decoded.CompressionAlgo = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) + pageResponse[7]);
            decoded.DecompressionAlgo = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);

            return decoded;
        }

        public static string PrettifyModePage_0F(byte[] pageResponse)
        {
            return PrettifyModePage_0F(DecodeModePage_0F(pageResponse));
        }

        public static string PrettifyModePage_0F(ModePage_0F? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_0F page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Data compression page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.DCC)
            {
                sb.AppendLine("\tDrive supports data compression");
                if(page.DCE)
                {
                    sb.Append("\tData compression is enabled with ");
                    switch(page.CompressionAlgo)
                    {
                        case 3:
                            sb.AppendLine("IBM ALDC with 512 byte buffer");
                            break;
                        case 4:
                            sb.AppendLine("IBM ALDC with 1024 byte buffer");
                            break;
                        case 5:
                            sb.AppendLine("IBM ALDC with 2048 byte buffer");
                            break;
                        case 0x10:
                            sb.AppendLine("IBM IDRC");
                            break;
                        case 0x20:
                            sb.AppendLine("DCLZ");
                            break;
                        case 0xFF:
                            sb.AppendLine("an unregistered compression algorithm");
                            break;
                        default:
                            sb.AppendFormat("an unknown algorithm coded {0}", page.CompressionAlgo).AppendLine();
                            break;
                    }
                }
                if(page.DDE)
                {
                    sb.AppendLine("\tData decompression is enabled");
                    if(page.DecompressionAlgo == 0)
                        sb.AppendLine("\tLast data read was uncompressed");
                    else
                    {
                        sb.Append("\tLast data read was compressed with ");
                        switch(page.CompressionAlgo)
                        {
                            case 3:
                                sb.AppendLine("IBM ALDC with 512 byte buffer");
                                break;
                            case 4:
                                sb.AppendLine("IBM ALDC with 1024 byte buffer");
                                break;
                            case 5:
                                sb.AppendLine("IBM ALDC with 2048 byte buffer");
                                break;
                            case 0x10:
                                sb.AppendLine("IBM IDRC");
                                break;
                            case 0x20:
                                sb.AppendLine("DCLZ");
                                break;
                            case 0xFF:
                                sb.AppendLine("an unregistered compression algorithm");
                                break;
                            default:
                                sb.AppendFormat("an unknown algorithm coded {0}", page.CompressionAlgo).AppendLine();
                                break;
                        }
                    }
                }

                sb.AppendFormat("\tReport exception on compression is set to {0}", page.RED).AppendLine();
            }
            else
                sb.AppendLine("\tDrive does not support data compression");

            return sb.ToString();
        }

        #endregion Mode Page 0x0F: Data compression page

        #region Mode Page 0x1B: Removable Block Access Capabilities page

        /// <summary>
        /// Removable Block Access Capabilities page
        /// Page code 0x1B
        /// 12 bytes in INF-8070
        /// </summary>
        public struct ModePage_1B
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Supports reporting progress of format
            /// </summary>
            public bool SRFP;
            /// <summary>
            /// Non-CD Optical Device
            /// </summary>
            public bool NCD;
            /// <summary>
            /// Phase change dual device supporting a CD and a Non-CD Optical devices
            /// </summary>
            public bool SML;
            /// <summary>
            /// Total number of LUNs
            /// </summary>
            public byte TLUN;
            /// <summary>
            /// System Floppy Type device
            /// </summary>
            public bool SFLP;
        }

        public static ModePage_1B? DecodeModePage_1B(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x1B)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 12)
                return null;

            ModePage_1B decoded = new ModePage_1B();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.SFLP |= (pageResponse[2] & 0x80) == 0x80;
            decoded.SRFP |= (pageResponse[2] & 0x40) == 0x40;
            decoded.NCD |= (pageResponse[3] & 0x80) == 0x80;
            decoded.SML |= (pageResponse[3] & 0x40) == 0x40;

            decoded.TLUN = (byte)(pageResponse[3] & 0x07);

            return decoded;
        }

        public static string PrettifyModePage_1B(byte[] pageResponse)
        {
            return PrettifyModePage_1B(DecodeModePage_1B(pageResponse));
        }

        public static string PrettifyModePage_1B(ModePage_1B? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_1B page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Removable Block Access Capabilities page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.SFLP)
                sb.AppendLine("\tDrive can be used as a system floppy device");
            if(page.SRFP)
                sb.AppendLine("\tDrive supports reporting progress of format");
            if(page.NCD)
                sb.AppendLine("\tDrive is a Non-CD Optical Device");
            if(page.SML)
                sb.AppendLine("\tDevice is a dual device supporting CD and Non-CD Optical");
            if(page.TLUN > 0)
                sb.AppendFormat("\tDrive supports {0} LUNs", page.TLUN).AppendLine();

            return sb.ToString();
        }

        #endregion Mode Page 0x1B: Removable Block Access Capabilities page

        #region Mode Page 0x1C: Timer & Protect page

        /// <summary>
        /// Timer &amp; Protect page
        /// Page code 0x1C
        /// 8 bytes in INF-8070
        /// </summary>
        public struct ModePage_1C_SFF
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Time the device shall remain in the current state after seek, read or write operation
            /// </summary>
            public byte InactivityTimeMultiplier;
            /// <summary>
            /// Disabled until power cycle
            /// </summary>
            public bool DISP;
            /// <summary>
            /// Software Write Protect until Power-down
            /// </summary>
            public bool SWPP;
        }

        public static ModePage_1C_SFF? DecodeModePage_1C_SFF(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x1C)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            ModePage_1C_SFF decoded = new ModePage_1C_SFF();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.DISP |= (pageResponse[2] & 0x02) == 0x02;
            decoded.SWPP |= (pageResponse[3] & 0x01) == 0x01;

            decoded.InactivityTimeMultiplier = (byte)(pageResponse[3] & 0x0F);

            return decoded;
        }

        public static string PrettifyModePage_1C_SFF(byte[] pageResponse)
        {
            return PrettifyModePage_1C_SFF(DecodeModePage_1C_SFF(pageResponse));
        }

        public static string PrettifyModePage_1C_SFF(ModePage_1C_SFF? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_1C_SFF page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Timer & Protect page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.DISP)
                sb.AppendLine("\tDrive is disabled until power is cycled");
            if(page.SWPP)
                sb.AppendLine("\tDrive is software write-protected until powered down");

            switch(page.InactivityTimeMultiplier)
            {
                case 0:
                    sb.AppendLine("\tDrive will remain in same status a vendor-specified time after a seek, read or write operation");
                    break;
                case 1:
                    sb.AppendLine("\tDrive will remain in same status 125 ms after a seek, read or write operation");
                    break;
                case 2:
                    sb.AppendLine("\tDrive will remain in same status 250 ms after a seek, read or write operation");
                    break;
                case 3:
                    sb.AppendLine("\tDrive will remain in same status 500 ms after a seek, read or write operation");
                    break;
                case 4:
                    sb.AppendLine("\tDrive will remain in same status 1 second after a seek, read or write operation");
                    break;
                case 5:
                    sb.AppendLine("\tDrive will remain in same status 2 seconds after a seek, read or write operation");
                    break;
                case 6:
                    sb.AppendLine("\tDrive will remain in same status 4 seconds after a seek, read or write operation");
                    break;
                case 7:
                    sb.AppendLine("\tDrive will remain in same status 8 seconds after a seek, read or write operation");
                    break;
                case 8:
                    sb.AppendLine("\tDrive will remain in same status 16 seconds after a seek, read or write operation");
                    break;
                case 9:
                    sb.AppendLine("\tDrive will remain in same status 32 seconds after a seek, read or write operation");
                    break;
                case 10:
                    sb.AppendLine("\tDrive will remain in same status 1 minute after a seek, read or write operation");
                    break;
                case 11:
                    sb.AppendLine("\tDrive will remain in same status 2 minutes after a seek, read or write operation");
                    break;
                case 12:
                    sb.AppendLine("\tDrive will remain in same status 4 minutes after a seek, read or write operation");
                    break;
                case 13:
                    sb.AppendLine("\tDrive will remain in same status 8 minutes after a seek, read or write operation");
                    break;
                case 14:
                    sb.AppendLine("\tDrive will remain in same status 16 minutes after a seek, read or write operation");
                    break;
                case 15:
                    sb.AppendLine("\tDrive will remain in same status 32 minutes after a seek, read or write operation");
                    break;
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x1C: Timer & Protect page

        #region Mode Page 0x00: Drive Operation Mode page

        /// <summary>
        /// Drive Operation Mode page
        /// Page code 0x00
        /// 4 bytes in INF-8070
        /// </summary>
        public struct ModePage_00_SFF
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Select LUN Mode
            /// </summary>
            public bool SLM;
            /// <summary>
            /// Select LUN for rewritable
            /// </summary>
            public bool SLR;
            /// <summary>
            /// Disable verify for WRITE
            /// </summary>
            public bool DVW;
            /// <summary>
            /// Disable deferred error
            /// </summary>
            public bool DDE;
        }

        public static ModePage_00_SFF? DecodeModePage_00_SFF(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x00)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 4)
                return null;

            ModePage_00_SFF decoded = new ModePage_00_SFF();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.SLM |= (pageResponse[2] & 0x80) == 0x80;
            decoded.SLR |= (pageResponse[2] & 0x40) == 0x40;
            decoded.DVW |= (pageResponse[2] & 0x20) == 0x20;

            decoded.DDE |= (pageResponse[3] & 0x10) == 0x10;

            return decoded;
        }

        public static string PrettifyModePage_00_SFF(byte[] pageResponse)
        {
            return PrettifyModePage_00_SFF(DecodeModePage_00_SFF(pageResponse));
        }

        public static string PrettifyModePage_00_SFF(ModePage_00_SFF? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_00_SFF page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Drive Operation Mode page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.DVW)
                sb.AppendLine("\tVerifying after writing is disabled");
            if(page.DDE)
                sb.AppendLine("\tDrive will abort when a writing error is detected");

            if(page.SLM)
            {
                sb.Append("\tDrive has two LUNs with rewritable being ");
                if(page.SLM)
                    sb.AppendLine("LUN 1");
                else
                    sb.AppendLine("LUN 0");
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x00: Drive Operation Mode page

        public struct ModePage
        {
            public byte Page;
            public byte Subpage;
            public byte[] PageResponse;
        }

        public struct DecodedMode
        {
            public ModeHeader Header;
            public ModePage[] Pages;
        }

        public static DecodedMode? DecodeMode6(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            ModeHeader? hdr = DecodeModeHeader6(modeResponse, deviceType);
            if(!hdr.HasValue)
                return null;

            DecodedMode decoded = new DecodedMode();
            decoded.Header = hdr.Value;
            int blkDrLength = 0;
            if(decoded.Header.BlockDescriptors != null)
                blkDrLength = decoded.Header.BlockDescriptors.Length;

            int offset = 4 + blkDrLength * 8;
            int length = modeResponse[0] + 1;

            if(length != modeResponse.Length)
                return decoded;

            List<ModePage> listpages = new List<ModePage>();

            while(offset < modeResponse.Length)
            {
                bool isSubpage = (modeResponse[offset] & 0x40) == 0x40;
                ModePage pg = new ModePage();
                byte pageNo = (byte)(modeResponse[offset] & 0x3F);

                if(pageNo == 0)
                {
                    pg.PageResponse = new byte[modeResponse.Length - offset];
                    Array.Copy(modeResponse, offset, pg.PageResponse, 0, pg.PageResponse.Length);
                    pg.Page = 0;
                    pg.Subpage = 0;
                    offset += pg.PageResponse.Length;
                }
                else
                {
                    if(isSubpage)
                    {
                        if (offset + 3 >= modeResponse.Length)
                            break;

                        pg.PageResponse = new byte[(modeResponse[offset + 2] << 8) + modeResponse[offset + 3] + 4];
                        if((pg.PageResponse.Length + offset) > modeResponse.Length)
                            return decoded;
                        Array.Copy(modeResponse, offset, pg.PageResponse, 0, pg.PageResponse.Length);
                        pg.Page = (byte)(modeResponse[offset] & 0x3F);
                        pg.Subpage = modeResponse[offset + 1];
                        offset += pg.PageResponse.Length;
                    }
                    else
                    {
                        if (offset + 1 >= modeResponse.Length)
                            break;
                        
                        pg.PageResponse = new byte[modeResponse[offset + 1] + 2];
                        if((pg.PageResponse.Length + offset) > modeResponse.Length)
                            return decoded;
                        Array.Copy(modeResponse, offset, pg.PageResponse, 0, pg.PageResponse.Length);
                        pg.Page = (byte)(modeResponse[offset] & 0x3F);
                        pg.Subpage = 0;
                        offset += pg.PageResponse.Length;
                    }
                }

                listpages.Add(pg);
            }

            decoded.Pages = listpages.ToArray();

            return decoded;
        }

        public static DecodedMode? DecodeMode10(byte[] modeResponse, PeripheralDeviceTypes deviceType)
        {
            ModeHeader? hdr = DecodeModeHeader10(modeResponse, deviceType);
            if(!hdr.HasValue)
                return null;

            DecodedMode decoded = new DecodedMode();
            decoded.Header = hdr.Value;
            bool longlba = (modeResponse[4] & 0x01) == 0x01;
            int offset;
            int blkDrLength = 0;
            if(decoded.Header.BlockDescriptors != null)
                blkDrLength = decoded.Header.BlockDescriptors.Length;

            if(longlba)
                offset = 8 + blkDrLength * 16;
            else
                offset = 8 + blkDrLength * 8;
            int length = (modeResponse[0] << 8);
            length += modeResponse[1];
            length += 2;

            if(length != modeResponse.Length)
                return decoded;

            List<ModePage> listpages = new List<ModePage>();

            while(offset < modeResponse.Length)
            {
                bool isSubpage = (modeResponse[offset] & 0x40) == 0x40;
                ModePage pg = new ModePage();
                byte pageNo = (byte)(modeResponse[offset] & 0x3F);

                if(pageNo == 0)
                {
                    pg.PageResponse = new byte[modeResponse.Length - offset];
                    Array.Copy(modeResponse, offset, pg.PageResponse, 0, pg.PageResponse.Length);
                    pg.Page = 0;
                    pg.Subpage = 0;
                    offset += pg.PageResponse.Length;
                }
                else
                {
                    if(isSubpage)
                    {
                        pg.PageResponse = new byte[(modeResponse[offset + 2] << 8) + modeResponse[offset + 3] + 4];

                        if((pg.PageResponse.Length + offset) > modeResponse.Length)
                            return decoded;

                        Array.Copy(modeResponse, offset, pg.PageResponse, 0, pg.PageResponse.Length);
                        pg.Page = (byte)(modeResponse[offset] & 0x3F);
                        pg.Subpage = modeResponse[offset + 1];
                        offset += pg.PageResponse.Length;
                    }
                    else
                    {
                        pg.PageResponse = new byte[modeResponse[offset + 1] + 2];

                        if((pg.PageResponse.Length + offset) > modeResponse.Length)
                            return decoded;

                        Array.Copy(modeResponse, offset, pg.PageResponse, 0, pg.PageResponse.Length);
                        pg.Page = (byte)(modeResponse[offset] & 0x3F);
                        pg.Subpage = 0;
                        offset += pg.PageResponse.Length;
                    }
                }

                listpages.Add(pg);
            }

            decoded.Pages = listpages.ToArray();

            return decoded;
        }

        #region Fujitsu Mode Page 0x3E: Verify Control page
        public enum Fujitsu_VerifyModes : byte
        {
            /// <summary>
            /// Always verify after writing
            /// </summary>
            Always = 0,
            /// <summary>
            /// Never verify after writing
            /// </summary>
            Never = 1,
            /// <summary>
            /// Verify after writing depending on condition
            /// </summary>
            Depends = 2,
            Reserved = 4
        }

        public struct Fujitsu_ModePage_3E
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// If set, AV data support mode is applied
            /// </summary>
            public bool audioVisualMode;
            /// <summary>
            /// If set the test write operation is restricted
            /// </summary>
            public bool streamingMode;
            public byte Reserved1;
            /// <summary>
            /// Verify mode for WRITE commands
            /// </summary>
            public Fujitsu_VerifyModes verifyMode;
            public byte Reserved2;
            /// <summary>
            /// Device type provided in response to INQUIRY
            /// </summary>
            public PeripheralDeviceTypes devType;
            public byte[] Reserved3;
        }

        public static Fujitsu_ModePage_3E? DecodeFujitsuModePage_3E(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x3E)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 8)
                return null;

            Fujitsu_ModePage_3E decoded = new Fujitsu_ModePage_3E();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.audioVisualMode |= (pageResponse[2] & 0x80) == 0x80;
            decoded.streamingMode |= (pageResponse[2] & 0x40) == 0x40;
            decoded.Reserved1 = (byte)((pageResponse[2] & 0x3C) >> 2);
            decoded.verifyMode = (Fujitsu_VerifyModes)(pageResponse[2] & 0x03);

            decoded.Reserved2 = (byte)((pageResponse[3] & 0xE0) >> 5);
            decoded.devType = (PeripheralDeviceTypes)(pageResponse[3] & 0x1F);

            decoded.Reserved3 = new byte[4];
            Array.Copy(pageResponse, 4, decoded.Reserved3, 0, 4);

            return decoded;
        }

        public static string PrettifyFujitsuModePage_3E(byte[] pageResponse)
        {
            return PrettifyFujitsuModePage_3E(DecodeFujitsuModePage_3E(pageResponse));
        }

        public static string PrettifyFujitsuModePage_3E(Fujitsu_ModePage_3E? modePage)
        {
            if(!modePage.HasValue)
                return null;

            Fujitsu_ModePage_3E page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Fujitsu Verify Control Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.audioVisualMode)
                sb.AppendLine("\tAudio/Visual data support mode is applied");
            if(page.streamingMode)
                sb.AppendLine("\tTest write operation is restricted during read or write operations.");

            switch(page.verifyMode)
            {
                case Fujitsu_VerifyModes.Always:
                    sb.AppendLine("\tAlways apply the verify operation");
                    break;
                case Fujitsu_VerifyModes.Never:
                    sb.AppendLine("\tNever apply the verify operation");
                    break;
                case Fujitsu_VerifyModes.Depends:
                    sb.AppendLine("\tApply the verify operation depending on the condition");
                    break;
            }

            sb.AppendFormat("\tThe device type that would be provided in the INQUIRY response is {0}",
                page.devType).AppendLine();

            return sb.ToString();
        }

        #endregion Fujitsu Mode Page 0x3E: Verify Control page

        #region Mode Page 0x11: Medium partition page (1)

        public enum PartitionSizeUnitOfMeasures : byte
        {
            /// <summary>
            /// Partition size is measures in bytes
            /// </summary>
            Bytes = 0,
            /// <summary>
            /// Partition size is measures in Kilobytes
            /// </summary>
            Kilobytes = 1,
            /// <summary>
            /// Partition size is measures in Megabytes
            /// </summary>
            Megabytes = 2,
            /// <summary>
            /// Partition size is 10eUNITS bytes
            /// </summary>
            Exponential = 3
        }

        public enum MediumFormatRecognitionValues : byte
        {
            /// <summary>
            /// Logical unit is incapable of format or partition recognition
            /// </summary>
            Incapable = 0,
            /// <summary>
            /// Logical unit is capable of format recognition only
            /// </summary>
            FormatCapable = 1,
            /// <summary>
            /// Logical unit is capable of partition recognition only
            /// </summary>
            PartitionCapable = 2,
            /// <summary>
            /// Logical unit is capable of both format and partition recognition
            /// </summary>
            Capable = 3
        }

        /// <summary>
        /// Medium partition page(1)
        /// Page code 0x11
        /// </summary>
        public struct ModePage_11
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Maximum number of additional partitions supported
            /// </summary>
            public byte MaxAdditionalPartitions;
            /// <summary>
            /// Number of additional partitions to be defined for a volume
            /// </summary>
            public byte AdditionalPartitionsDefined;
            /// <summary>
            /// Device defines partitions based on its fixed definition
            /// </summary>
            public bool FDP;
            /// <summary>
            /// Device should divide medium according to the additional partitions defined field using sizes defined by device
            /// </summary>
            public bool SDP;
            /// <summary>
            /// Initiator defines number and size of partitions
            /// </summary>
            public bool IDP;
            /// <summary>
            /// Defines the unit on which the partition sizes are defined
            /// </summary>
            public PartitionSizeUnitOfMeasures PSUM;
            public bool POFM;
            public bool CLEAR;
            public bool ADDP;
            /// <summary>
            /// Defines the capabilities for the unit to recognize media partitions and format
            /// </summary>
            public MediumFormatRecognitionValues MediumFormatRecognition;
            public byte PartitionUnits;
            /// <summary>
            /// Array of partition sizes in units defined above
            /// </summary>
            public ushort[] PartitionSizes;
        }

        public static ModePage_11? DecodeModePage_11(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x11)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            ModePage_11 decoded = new ModePage_11();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.MaxAdditionalPartitions = pageResponse[2];
            decoded.AdditionalPartitionsDefined = pageResponse[3];
            decoded.FDP |= (pageResponse[4] & 0x80) == 0x80;
            decoded.SDP |= (pageResponse[4] & 0x40) == 0x40;
            decoded.IDP |= (pageResponse[4] & 0x20) == 0x20;
            decoded.PSUM = (PartitionSizeUnitOfMeasures)((pageResponse[4] & 0x18) >> 3);
            decoded.POFM |= (pageResponse[4] & 0x04) == 0x04;
            decoded.CLEAR |= (pageResponse[4] & 0x02) == 0x02;
            decoded.ADDP |= (pageResponse[4] & 0x01) == 0x01;
            decoded.PartitionUnits = (byte)(pageResponse[6] & 0x0F);
            decoded.MediumFormatRecognition = (MediumFormatRecognitionValues)pageResponse[5];
            decoded.PartitionSizes = new ushort[(pageResponse.Length - 8) / 2];

            for(int i = 8; i < pageResponse.Length; i+=2)
            {
                decoded.PartitionSizes[(i - 8) / 2] = (ushort)(pageResponse[i] << 8);
                decoded.PartitionSizes[(i - 8) / 2] += pageResponse[i+1];
            }

            return decoded;
        }

        public static string PrettifyModePage_11(byte[] pageResponse)
        {
            return PrettifyModePage_11(DecodeModePage_11(pageResponse));
        }

        public static string PrettifyModePage_11(ModePage_11? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_11 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI medium partition page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\t{0} maximum additional partitions", page.MaxAdditionalPartitions).AppendLine();
            sb.AppendFormat("\t{0} additional partitions defined", page.AdditionalPartitionsDefined).AppendLine();

            if(page.FDP)
                sb.AppendLine("\tPartitions are fixed under device definitions");
            if(page.SDP)
                sb.AppendLine("\tNumber of partitions can be defined but their size is defined by the device");
            if(page.IDP)
                sb.AppendLine("\tNumber and size of partitions can be manually defined");
            if(page.POFM)
                sb.AppendLine("\tPartition parameters will not be applied until a FORMAT MEDIUM command is received");
            if(!page.CLEAR && !page.ADDP)
                sb.AppendLine("\tDevice may erase any or all partitions on MODE SELECT for partitioning");
            else if(page.CLEAR && !page.ADDP)
                sb.AppendLine("\tDevice shall erase all partitions on MODE SELECT for partitioning");
            else if(!page.CLEAR && page.ADDP)
                sb.AppendLine("\tDevice shall not erase any partition on MODE SELECT for partitioning");
            else if(page.CLEAR && page.ADDP)
                sb.AppendLine("\tDevice shall erase all partitions differing on size on MODE SELECT for partitioning");

            string measure = "";

            switch(page.PSUM)
            {
                case PartitionSizeUnitOfMeasures.Bytes:
                    sb.AppendLine("\tPartitions are defined in bytes");
                    measure = "bytes";
                    break;
                case PartitionSizeUnitOfMeasures.Kilobytes:
                    sb.AppendLine("\tPartitions are defined in kilobytes");
                    measure = "kilobytes";
                    break;
                case PartitionSizeUnitOfMeasures.Megabytes:
                    sb.AppendLine("\tPartitions are defined in megabytes");
                    measure = "megabytes";
                    break;
                case PartitionSizeUnitOfMeasures.Exponential:
                    sb.AppendFormat("\tPartitions are defined in units of {0} bytes", Math.Pow(10, page.PartitionUnits)).AppendLine();
                    measure = string.Format("units of {0} bytes", Math.Pow(10, page.PartitionUnits));
                    break;
                default:
                    sb.AppendFormat("\tUnknown partition size unit code {0}", (byte)page.PSUM).AppendLine();
                    measure = "units";
                    break;
            }

            switch(page.MediumFormatRecognition)
            {
                case MediumFormatRecognitionValues.Capable:
                    sb.AppendLine("\tDevice is capable of recognizing both medium partitions and format");
                    break;
                case MediumFormatRecognitionValues.FormatCapable:
                    sb.AppendLine("\tDevice is capable of recognizing medium format");
                    break;
                case MediumFormatRecognitionValues.PartitionCapable:
                    sb.AppendLine("\tDevice is capable of recognizing medium partitions");
                    break;
                case MediumFormatRecognitionValues.Incapable:
                    sb.AppendLine("\tDevice is not capable of recognizing neither medium partitions nor format");
                    break;
                default:
                    sb.AppendFormat("\tUnknown medium recognition code {0}", (byte)page.MediumFormatRecognition).AppendLine();
                    break;
            }

            sb.AppendFormat("\tMedium has defined {0} partitions", page.PartitionSizes.Length).AppendLine();

            for(int i = 0; i < page.PartitionSizes.Length; i++)
            {
                if(page.PartitionSizes[i] == 0)
                {
                    if(page.PartitionSizes.Length == 1)
                        sb.AppendLine("\tDevice recognizes one single partition spanning whole medium");
                    else
                        sb.AppendFormat("\tPartition {0} runs for rest of medium", i).AppendLine();
                }
                else
                    sb.AppendFormat("\tPartition {0} is {1} {2} long", i, page.PartitionSizes[i], measure).AppendLine();
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x11: Medium partition page (1)

        #region Mode Pages 0x12, 0x13, 0x14: Medium partition page (2-4)

        /// <summary>
        /// Medium partition page (2-4)
        /// Page codes 0x12, 0x13 and 0x14
        /// </summary>
        public struct ModePage_12_13_14
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            /// <summary>
            /// Array of partition sizes in units defined in mode page 11
            /// </summary>
            public ushort[] PartitionSizes;
        }

        public static ModePage_12_13_14? DecodeModePage_12_13_14(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x12 &&
                (pageResponse[0] & 0x3F) != 0x13 &&
                (pageResponse[0] & 0x3F) != 0x14)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 2)
                return null;

            ModePage_12_13_14 decoded = new ModePage_12_13_14();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

            decoded.PartitionSizes = new ushort[(pageResponse.Length - 2) / 2];

            for(int i = 2; i < pageResponse.Length; i += 2)
            {
                decoded.PartitionSizes[(i - 2) / 2] = (ushort)(pageResponse[i] << 8);
                decoded.PartitionSizes[(i - 2) / 2] += pageResponse[i + 1];
            }

            return decoded;
        }

        public static string PrettifyModePage_12_13_14(byte[] pageResponse)
        {
            return PrettifyModePage_12_13_14(DecodeModePage_12_13_14(pageResponse));
        }

        public static string PrettifyModePage_12_13_14(ModePage_12_13_14? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_12_13_14 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI medium partition page (extra):");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\tMedium has defined {0} partitions", page.PartitionSizes.Length).AppendLine();

            for(int i = 0; i < page.PartitionSizes.Length; i++)
                sb.AppendFormat("\tPartition {0} is {1} units long", i, page.PartitionSizes[i]).AppendLine();

            return sb.ToString();
        }

        #endregion Mode Pages 0x12, 0x13, 0x14: Medium partition page (2-4)

        #region Certance Mode Page 0x21: Drive Capabilities Control Mode page
        public struct Certance_ModePage_21
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public byte OperatingSystemsSupport;
            public byte FirmwareTestControl2;
            public byte ExtendedPOSTMode;
            public byte InquiryStringControl;
            public byte FirmwareTestControl;
            public byte DataCompressionControl;
            public bool HostUnloadOverride;
            public byte AutoUnloadMode;
        }

        public static Certance_ModePage_21? DecodeCertanceModePage_21(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x21)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 9)
                return null;

            Certance_ModePage_21 decoded = new Certance_ModePage_21();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.OperatingSystemsSupport = pageResponse[2];
            decoded.FirmwareTestControl2 = pageResponse[3];
            decoded.ExtendedPOSTMode = pageResponse[4];
            decoded.InquiryStringControl = pageResponse[5];
            decoded.FirmwareTestControl = pageResponse[6];
            decoded.DataCompressionControl = pageResponse[7];
            decoded.HostUnloadOverride |= (pageResponse[8] & 0x80) == 0x80;
            decoded.AutoUnloadMode = (byte)(pageResponse[8] & 0x7F);

            return decoded;
        }

        public static string PrettifyCertanceModePage_21(byte[] pageResponse)
        {
            return PrettifyCertanceModePage_21(DecodeCertanceModePage_21(pageResponse));
        }

        public static string PrettifyCertanceModePage_21(Certance_ModePage_21? modePage)
        {
            if(!modePage.HasValue)
                return null;

            Certance_ModePage_21 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Certance Drive Capabilities Control Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            switch(page.OperatingSystemsSupport)
            {
                case 0:
                    sb.AppendLine("\tOperating systems support is standard LTO");
                    break;
                default:
                    sb.AppendFormat("\tOperating systems support is unknown code {0}", page.OperatingSystemsSupport).AppendLine();
                    break;
            }

            if(page.FirmwareTestControl == page.FirmwareTestControl2)
            {
                switch(page.FirmwareTestControl)
                {
                    case 0:
                        sb.AppendLine("\tFactory test code is disabled");
                        break;
                    case 1:
                        sb.AppendLine("\tFactory test code 1 is disabled");
                        break;
                    case 2:
                        sb.AppendLine("\tFactory test code 2 is disabled");
                        break;
                    default:
                        sb.AppendFormat("\tUnknown factory test code {0}", page.FirmwareTestControl).AppendLine();
                        break;
                }
            }

            switch(page.ExtendedPOSTMode)
            {
                case 0:
                    sb.AppendLine("\tPower-On Self-Test is enabled");
                    break;
                case 1:
                    sb.AppendLine("\tPower-On Self-Test is disable");
                    break;
                default:
                    sb.AppendFormat("\tUnknown Power-On Self-Test code {0}", page.ExtendedPOSTMode).AppendLine();
                    break;
            }

            switch(page.DataCompressionControl)
            {
                case 0:
                    sb.AppendLine("\tCompression is controlled using mode pages 0Fh and 10h");
                    break;
                case 1:
                    sb.AppendLine("\tCompression is enabled and not controllable");
                    break;
                case 2:
                    sb.AppendLine("\tCompression is disabled and not controllable");
                    break;
                default:
                    sb.AppendFormat("\tUnknown compression control code {0}", page.DataCompressionControl).AppendLine();
                    break;
            }

            if(page.HostUnloadOverride)
                sb.AppendLine("\tSCSI UNLOAD command will not eject the cartridge");

            sb.Append("\tHow should tapes be unloaded in a power cycle, tape incompatibility, firmware download or cleaning end: ");
            switch(page.AutoUnloadMode)
            {
                case 0:
                    sb.AppendLine("\tTape will stay threaded at beginning");
                    break;
                case 1:
                    sb.AppendLine("\tTape will be unthreaded");
                    break;
                case 2:
                    sb.AppendLine("\tTape will be unthreaded and unloaded");
                    break;
                case 3:
                    sb.AppendLine("\tData tapes will be threaded at beginning, rest will be unloaded");
                    break;
                default:
                    sb.AppendFormat("\tUnknown auto unload code {0}", page.AutoUnloadMode).AppendLine();
                    break;
            }

            return sb.ToString();
        }

        #endregion Certance Mode Page 0x21: Drive Capabilities Control Mode page

        #region Certance Mode Page 0x22: Interface Control Mode Page
        public struct Certance_ModePage_22
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public byte BaudRate;
            public byte CmdFwd;
            public bool StopBits;
            public byte Alerts;
            public byte PortATransportType;
            public byte PortAPresentSelectionID;
            public byte NextSelectionID;
            public byte JumperedSelectionID;
            public byte TargetInitiatedBusControl;
            public bool PortAEnabled;
            public bool PortAEnabledOnPower;
        }

        public static Certance_ModePage_22? DecodeCertanceModePage_22(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x22)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 16)
                return null;

            Certance_ModePage_22 decoded = new Certance_ModePage_22();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.BaudRate = pageResponse[2];
            decoded.CmdFwd = (byte)((pageResponse[3] & 0x18) >> 3);
            decoded.StopBits |= (pageResponse[3] & 0x04) == 0x04;
            decoded.CmdFwd = (byte)((pageResponse[3] & 0x03));
            decoded.PortATransportType = pageResponse[4];
            decoded.PortAPresentSelectionID = pageResponse[7];
            decoded.NextSelectionID = pageResponse[12];
            decoded.JumperedSelectionID = pageResponse[13];
            decoded.TargetInitiatedBusControl = pageResponse[14];
            decoded.PortAEnabled |= (pageResponse[15] & 0x10) == 0x10;
            decoded.PortAEnabledOnPower |= (pageResponse[15] & 0x04) == 0x04;

            return decoded;
        }

        public static string PrettifyCertanceModePage_22(byte[] pageResponse)
        {
            return PrettifyCertanceModePage_22(DecodeCertanceModePage_22(pageResponse));
        }

        public static string PrettifyCertanceModePage_22(Certance_ModePage_22? modePage)
        {
            if(!modePage.HasValue)
                return null;

            Certance_ModePage_22 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Certance Interface Control Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            switch(page.BaudRate)
            {
                case 0:
                case 1:
                case 2:
                    sb.AppendLine("\tLibrary interface will operate at 9600 baud on next reset");
                    break;
                case 3:
                    sb.AppendLine("\tLibrary interface will operate at 19200 baud on next reset");
                    break;
                case 4:
                    sb.AppendLine("\tLibrary interface will operate at 38400 baud on next reset");
                    break;
                case 5:
                    sb.AppendLine("\tLibrary interface will operate at 57600 baud on next reset");
                    break;
                case 6:
                    sb.AppendLine("\tLibrary interface will operate at 115200 baud on next reset");
                    break;
                default:
                    sb.AppendFormat("\tUnknown library interface baud rate code {0}", page.BaudRate).AppendLine();
                    break;
            }

            if(page.StopBits)
                sb.AppendLine("Library interface transmits 2 stop bits per byte");
            else
                sb.AppendLine("Library interface transmits 1 stop bits per byte");

            switch(page.CmdFwd)
            {
                case 0:
                    sb.AppendLine("\tCommand forwarding is disabled");
                    break;
                case 1:
                    sb.AppendLine("\tCommand forwarding is enabled");
                    break;
                default:
                    sb.AppendFormat("\tUnknown command forwarding code {0}", page.CmdFwd).AppendLine();
                    break;
            }

            switch(page.PortATransportType)
            {
                case 0:
                    sb.AppendLine("\tPort A link is down");
                    break;
                case 3:
                    sb.AppendLine("\tPort A uses Parallel SCSI Ultra-160 interface");
                    break;
                default:
                    sb.AppendFormat("\tUnknown port A transport type code {0}", page.PortATransportType).AppendLine();
                    break;
            }

            if(page.PortATransportType > 0)
                sb.AppendFormat("\tDrive responds to SCSI ID {0}", page.PortAPresentSelectionID).AppendLine();

            sb.AppendFormat("\tDrive will respond to SCSI ID {0} on Port A enabling", page.NextSelectionID).AppendLine();
            sb.AppendFormat("\tDrive jumpers choose SCSI ID {0}", page.JumperedSelectionID).AppendLine();

            if(page.PortAEnabled)
                sb.AppendLine("\tSCSI port is enabled");
            else
                sb.AppendLine("\tSCSI port is disabled");

            if(page.PortAEnabledOnPower)
                sb.AppendLine("\tSCSI port will be enabled on next power up");
            else
                sb.AppendLine("\tSCSI port will be disabled on next power up");

            return sb.ToString();
        }

        #endregion Certance Mode Page 0x22: Interface Control Mode Page

        #region Mode Page 0x1D: Medium Configuration Mode Page
        public struct ModePage_1D
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public bool WORMM;
            public byte WormModeLabelRestrictions;
            public byte WormModeFilemarkRestrictions;
        }

        public static ModePage_1D? DecodeModePage_1D(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x1D)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 32)
                return null;

            ModePage_1D decoded = new ModePage_1D();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.WORMM |= (pageResponse[2] & 0x01) == 0x01;
            decoded.WormModeLabelRestrictions = pageResponse[4];
            decoded.WormModeFilemarkRestrictions = pageResponse[5];

            return decoded;
        }

        public static string PrettifyModePage_1D(byte[] pageResponse)
        {
            return PrettifyModePage_1D(DecodeModePage_1D(pageResponse));
        }

        public static string PrettifyModePage_1D(ModePage_1D? modePage)
        {
            if(!modePage.HasValue)
                return null;

            ModePage_1D page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SCSI Medium Configuration Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.WORMM)
                sb.AppendLine("\tDrive is operating in WORM mode");

            switch(page.WormModeLabelRestrictions)
            {
                case 0:
                    sb.AppendLine("\tDrive does not allow any logical blocks to be overwritten");
                    break;
                case 1:
                    sb.AppendLine("\tDrive allows a tape header to be overwritten");
                    break;
                case 2:
                    sb.AppendLine("\tDrive allows all format labels to be overwritten");
                    break;
                default:
                    sb.AppendFormat("\tUnknown WORM mode label restrictions code {0}", page.WormModeLabelRestrictions).AppendLine();
                    break;
            }

            switch(page.WormModeFilemarkRestrictions)
            {
                case 2:
                    sb.AppendLine("\tDrive allows any number of filemarks immediately preceding EOD to be overwritten except filemark closes to BOP");
                    break;
                case 3:
                    sb.AppendLine("\tDrive allows any number of filemarks immediately preceding EOD to be overwritten");
                    break;
                default:
                    sb.AppendFormat("\tUnknown WORM mode filemark restrictions code {0}", page.WormModeLabelRestrictions).AppendLine();
                    break;
            }

            return sb.ToString();
        }

        #endregion Mode Page 0x1D: Medium Configuration Mode Page

        #region IBM Mode Page 0x24: Drive Capabilities Control Mode page
        public struct IBM_ModePage_24
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public byte ModeControl;
            public byte VelocitySetting;
            public bool EncryptionEnabled;
            public bool EncryptionCapable;
        }

        public static IBM_ModePage_24? DecodeIBMModePage_24(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x24)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 8)
                return null;

            IBM_ModePage_24 decoded = new IBM_ModePage_24();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.ModeControl = pageResponse[2];
            decoded.VelocitySetting = pageResponse[3];
            decoded.EncryptionEnabled |= (pageResponse[7] & 0x08) == 0x08;
            decoded.EncryptionCapable |= (pageResponse[7] & 0x01) == 0x01;

            return decoded;
        }

        public static string PrettifyIBMModePage_24(byte[] pageResponse)
        {
            return PrettifyIBMModePage_24(DecodeIBMModePage_24(pageResponse));
        }

        public static string PrettifyIBMModePage_24(IBM_ModePage_24? modePage)
        {
            if(!modePage.HasValue)
                return null;

            IBM_ModePage_24 page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("IBM Vendor-Specific Control Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\tVendor-specific mode control: {0}", page.ModeControl);
            sb.AppendFormat("\tVendor-specific velocity setting: {0}", page.VelocitySetting);

            if(page.EncryptionCapable)
            {
                sb.AppendLine("\tDrive supports encryption");
                if(page.EncryptionEnabled)
                    sb.AppendLine("\tDrive has encryption enabled");
            }

            return sb.ToString();
        }

        #endregion IBM Mode Page 0x24: Drive Capabilities Control Mode page

        #region IBM Mode Page 0x2F: Behaviour Configuration Mode page
        public struct IBM_ModePage_2F
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public byte FenceBehaviour;
            public byte CleanBehaviour;
            public byte WORMEmulation;
            public byte SenseDataBehaviour;
            public bool CCDM;
            public bool DDEOR;
            public bool CLNCHK;
            public byte FirmwareUpdateBehaviour;
            public byte UOE_D;
            public byte UOE_F;
            public byte UOE_C;
        }

        public static IBM_ModePage_2F? DecodeIBMModePage_2F(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x2F)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length < 8)
                return null;

            IBM_ModePage_2F decoded = new IBM_ModePage_2F();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.FenceBehaviour = pageResponse[2];
            decoded.CleanBehaviour = pageResponse[3];
            decoded.WORMEmulation = pageResponse[4];
            decoded.SenseDataBehaviour = pageResponse[5];
            decoded.CCDM |= (pageResponse[6] & 0x04) == 0x04;
            decoded.DDEOR |= (pageResponse[6] & 0x02) == 0x02;
            decoded.CLNCHK |= (pageResponse[6] & 0x01) == 0x01;
            decoded.FirmwareUpdateBehaviour = pageResponse[7];
            decoded.UOE_C = (byte)((pageResponse[8] & 0x30) >> 4);
            decoded.UOE_F = (byte)((pageResponse[8] & 0x0C) >> 2);
            decoded.UOE_F = ((byte)(pageResponse[8] & 0x03));

            return decoded;
        }

        public static string PrettifyIBMModePage_2F(byte[] pageResponse)
        {
            return PrettifyIBMModePage_2F(DecodeIBMModePage_2F(pageResponse));
        }

        public static string PrettifyIBMModePage_2F(IBM_ModePage_2F? modePage)
        {
            if(!modePage.HasValue)
                return null;

            IBM_ModePage_2F page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("IBM Behaviour Configuration Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            switch(page.FenceBehaviour)
            {
                case 0:
                    sb.AppendLine("\tFence behaviour is normal");
                    break;
                case 1:
                    sb.AppendLine("\tPanic fence behaviour is enabled");
                    break;
                default:
                    sb.AppendFormat("\tUnknown fence behaviour code {0}", page.FenceBehaviour).AppendLine();
                    break;
            }

            switch(page.CleanBehaviour)
            {
                case 0:
                    sb.AppendLine("\tCleaning behaviour is normal");
                    break;
                case 1:
                    sb.AppendLine("\tDrive will periodically request cleaning");
                    break;
                default:
                    sb.AppendFormat("\tUnknown cleaning behaviour code {0}", page.CleanBehaviour).AppendLine();
                    break;
            }

            switch(page.WORMEmulation)
            {
                case 0:
                    sb.AppendLine("\tWORM emulation is disabled");
                    break;
                case 1:
                    sb.AppendLine("\tWORM emulation is enabled");
                    break;
                default:
                    sb.AppendFormat("\tUnknown WORM emulation code {0}", page.WORMEmulation).AppendLine();
                    break;
            }

            switch(page.SenseDataBehaviour)
            {
                case 0:
                    sb.AppendLine("\tUses 35-bytes sense data");
                    break;
                case 1:
                    sb.AppendLine("\tUses 96-bytes sense data");
                    break;
                default:
                    sb.AppendFormat("\tUnknown sense data behaviour code {0}", page.WORMEmulation).AppendLine();
                    break;
            }

            if(page.CLNCHK)
                sb.AppendLine("\tDrive will set Check Condition when cleaning is needed");
            if(page.DDEOR)
                sb.AppendLine("\tNo deferred error will be reported to a rewind command");
            if(page.CCDM)
                sb.AppendLine("\tDrive will set Check Condition when the criteria for Dead Media is met");
            if(page.FirmwareUpdateBehaviour > 0)
                sb.AppendLine("\tDrive will not accept downlevel firmware via an FMR tape");

            if(page.UOE_C == 1)
                sb.AppendLine("\tDrive will eject cleaning cartridges on error");
            if(page.UOE_F == 1)
                sb.AppendLine("\tDrive will eject firmware cartridges on error");
            if(page.UOE_D == 1)
                sb.AppendLine("\tDrive will eject data cartridges on error");

            return sb.ToString();
        }

        #endregion IBM Mode Page 0x24: Drive Capabilities Control Mode page

        #region IBM Mode Page 0x3D: Behaviour Configuration Mode page
        public struct IBM_ModePage_3D
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public ushort NumberOfWraps;
        }

        public static IBM_ModePage_3D? DecodeIBMModePage_3D(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x3D)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 5)
                return null;

            IBM_ModePage_3D decoded = new IBM_ModePage_3D();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.NumberOfWraps = (ushort)((pageResponse[3] << 8) + pageResponse[4]);

            return decoded;
        }

        public static string PrettifyIBMModePage_3D(byte[] pageResponse)
        {
            return PrettifyIBMModePage_3D(DecodeIBMModePage_3D(pageResponse));
        }

        public static string PrettifyIBMModePage_3D(IBM_ModePage_3D? modePage)
        {
            if(!modePage.HasValue)
                return null;

            IBM_ModePage_3D page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("IBM LEOT Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            sb.AppendFormat("\t{0} wraps", page.NumberOfWraps).AppendLine();

            return sb.ToString();
        }

        #endregion IBM Mode Page 0x3D: Behaviour Configuration Mode page

        #region HP Mode Page 0x3B: Serial Number Override Mode page
        public struct HP_ModePage_3B
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public byte MSN;
            public byte[] SerialNumber;
        }

        public static HP_ModePage_3B? DecodeHPModePage_3B(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x3B)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 16)
                return null;

            HP_ModePage_3B decoded = new HP_ModePage_3B();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.MSN = (byte)(pageResponse[2] & 0x03);
            decoded.SerialNumber = new byte[10];
            Array.Copy(pageResponse, 6, decoded.SerialNumber, 0, 10);

            return decoded;
        }

        public static string PrettifyHPModePage_3B(byte[] pageResponse)
        {
            return PrettifyHPModePage_3B(DecodeHPModePage_3B(pageResponse));
        }

        public static string PrettifyHPModePage_3B(HP_ModePage_3B? modePage)
        {
            if(!modePage.HasValue)
                return null;

            HP_ModePage_3B page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("HP Serial Number Override Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            switch(page.MSN)
            {
                case 1:
                    sb.AppendLine("\tSerial number is the manufacturer's default value");
                    break;
                case 3:
                    sb.AppendLine("\tSerial number is not the manufacturer's default value");
                    break;
            }

            sb.AppendFormat("\tSerial number: {0}", StringHandlers.CToString(page.SerialNumber)).AppendLine();

            return sb.ToString();
        }

        #endregion HP Mode Page 0x3B: Serial Number Override Mode page

        #region HP Mode Page 0x3C: Device Time Mode page
        public struct HP_ModePage_3C
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public bool LT;
            public bool WT;
            public bool PT;
            public ushort CurrentPowerOn;
            public uint PowerOnTime;
            public bool UTC;
            public bool NTP;
            public uint WorldTime;
            public byte LibraryHours;
            public byte LibraryMinutes;
            public byte LibrarySeconds;
            public uint CumulativePowerOn;
        }

        public static HP_ModePage_3C? DecodeHPModePage_3C(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x3C)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 36)
                return null;

            HP_ModePage_3C decoded = new HP_ModePage_3C();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.LT |= (pageResponse[2] & 0x04) == 0x04;
            decoded.WT |= (pageResponse[2] & 0x02) == 0x02;
            decoded.PT |= (pageResponse[2] & 0x01) == 0x01;
            decoded.CurrentPowerOn = (ushort)((pageResponse[6] << 8) + pageResponse[7]);
            decoded.PowerOnTime = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) + pageResponse[11]);
            decoded.UTC |= (pageResponse[14] & 0x02) == 0x02;
            decoded.NTP |= (pageResponse[14] & 0x01) == 0x01;
            decoded.WorldTime = (uint)((pageResponse[16] << 24) + (pageResponse[17] << 16) + (pageResponse[18] << 8) + pageResponse[19]);
            decoded.LibraryHours = pageResponse[23];
            decoded.LibraryMinutes = pageResponse[24];
            decoded.LibrarySeconds = pageResponse[25];
            decoded.CumulativePowerOn = (uint)((pageResponse[32] << 24) + (pageResponse[33] << 16) + (pageResponse[34] << 8) + pageResponse[35]);

            return decoded;
        }

        public static string PrettifyHPModePage_3C(byte[] pageResponse)
        {
            return PrettifyHPModePage_3C(DecodeHPModePage_3C(pageResponse));
        }

        public static string PrettifyHPModePage_3C(HP_ModePage_3C? modePage)
        {
            if(!modePage.HasValue)
                return null;

            HP_ModePage_3C page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("HP Device Time Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.PT)
            {
                sb.AppendFormat("\tDrive has been powered up {0} times", page.CurrentPowerOn);
                sb.AppendFormat("\tDrive has been powered up since {0} this time", TimeSpan.FromSeconds(page.PowerOnTime)).AppendLine();
                sb.AppendFormat("\tDrive has been powered up a total of {0}", TimeSpan.FromSeconds(page.CumulativePowerOn)).AppendLine();
            }

            if(page.WT)
            {
                sb.AppendFormat("\tDrive's date/time is: {0}", DateHandlers.UNIXUnsignedToDateTime(page.WorldTime)).AppendLine();
                if(page.UTC)
                    sb.AppendLine("\tDrive's time is UTC");
                if(page.NTP)
                    sb.AppendLine("\tDrive's time is synchronized with a NTP source");
            }

            if(page.LT)
                sb.AppendFormat("\tLibrary time is {0}", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, page.LibraryHours, page.LibraryMinutes, page.LibrarySeconds)).AppendLine();

            return sb.ToString();
        }

        #endregion HP Mode Page 0x3C: Device Time Mode page

        #region HP Mode Page 0x3D: Extended Reset Mode page
        public struct HP_ModePage_3D
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public byte ResetBehaviour;
        }

        public static HP_ModePage_3D? DecodeHPModePage_3D(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x3D)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 4)
                return null;

            HP_ModePage_3D decoded = new HP_ModePage_3D();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.ResetBehaviour = (byte)(pageResponse[2] & 0x03);

            return decoded;
        }

        public static string PrettifyHPModePage_3D(byte[] pageResponse)
        {
            return PrettifyHPModePage_3D(DecodeHPModePage_3D(pageResponse));
        }

        public static string PrettifyHPModePage_3D(HP_ModePage_3D? modePage)
        {
            if(!modePage.HasValue)
                return null;

            HP_ModePage_3D page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("HP Extended Reset Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            switch(page.ResetBehaviour)
            {
                case 0:
                    sb.AppendLine("\tNormal reset behaviour");
                    break;
                case 1:
                    sb.AppendLine("\tDrive will flush and position itself on a LUN or target reset");
                    break;
                case 2:
                    sb.AppendLine("\tDrive will maintain position on a LUN or target reset");
                    break;
            }

            return sb.ToString();
        }

        #endregion HP Mode Page 0x3D: Extended Reset Mode page

        #region HP Mode Page 0x3E: CD-ROM Emulation/Disaster Recovery Mode page
        public struct HP_ModePage_3E
        {
            /// <summary>
            /// Parameters can be saved
            /// </summary>
            public bool PS;
            public bool NonAuto;
            public bool CDmode;
        }

        public static HP_ModePage_3E? DecodeHPModePage_3E(byte[] pageResponse)
        {
            if(pageResponse == null)
                return null;

            if((pageResponse[0] & 0x40) == 0x40)
                return null;

            if((pageResponse[0] & 0x3F) != 0x3E)
                return null;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return null;

            if(pageResponse.Length != 4)
                return null;

            HP_ModePage_3E decoded = new HP_ModePage_3E();

            decoded.PS |= (pageResponse[0] & 0x80) == 0x80;
            decoded.NonAuto |= (pageResponse[2] & 0x02) == 0x02;
            decoded.CDmode|= (pageResponse[2] & 0x01) == 0x01;

            return decoded;
        }

        public static string PrettifyHPModePage_3E(byte[] pageResponse)
        {
            return PrettifyHPModePage_3E(DecodeHPModePage_3E(pageResponse));
        }

        public static string PrettifyHPModePage_3E(HP_ModePage_3E? modePage)
        {
            if(!modePage.HasValue)
                return null;

            HP_ModePage_3E page = modePage.Value;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("HP CD-ROM Emulation/Disaster Recovery Mode Page:");

            if(page.PS)
                sb.AppendLine("\tParameters can be saved");

            if(page.CDmode)
                sb.AppendLine("\tDrive is emulating a CD-ROM drive");
            else
                sb.AppendLine("\tDrive is not emulating a CD-ROM drive");
            if(page.NonAuto)
                sb.AppendLine("\tDrive will not exit emulation automatically");

            return sb.ToString();
        }

        #endregion HP Mode Page 0x3E: CD-ROM Emulation/Disaster Recovery Mode page

        #region Apple Mode Page 0x30: Apple OEM String
        static readonly byte[] AppleOEMString = { 0x41, 0x50, 0x50, 0x4C, 0x45, 0x20, 0x43, 0x4F, 0x4D, 0x50, 0x55, 0x54, 0x45, 0x52, 0x2C, 0x20, 0x49, 0x4E, 0x43, 0x2E };

        public static bool IsAppleModePage_30(byte[] pageResponse)
        {
            if(pageResponse == null)
                return false;

            if((pageResponse[0] & 0x40) == 0x40)
                return false;

            if((pageResponse[0] & 0x3F) != 0x30)
                return false;

            if(pageResponse[1] + 2 != pageResponse.Length)
                return false;

            if(pageResponse.Length != 30)
                return false;

            byte[] str = new byte[20];
            Array.Copy(pageResponse, 10, str, 0, 20);

            return AppleOEMString.SequenceEqual(str);
        }
        #endregion Apple Mode Page 0x30: Apple OEM String
    }
}

