// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Headers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Prettifies SCSI MODE headers.
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

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.Structs.Devices.SCSI;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    public static string GetMediumTypeDescription(MediumTypes type) => type switch
    {
        MediumTypes.ECMA54 =>
            "ECMA-54: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on One Side",
        MediumTypes.ECMA59 =>
            "ECMA-59 & ANSI X3.121-1984: 200 mm Flexible Disk Cartridge using Two-Frequency Recording at 13262 ftprad on Both Sides",
        MediumTypes.ECMA69 =>
            "ECMA-69: 200 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides",
        MediumTypes.ECMA66 =>
            "ECMA-66: 130 mm Flexible Disk Cartridge using Two-Frequency Recording at 7958 ftprad on One Side",
        MediumTypes.ECMA70 =>
            "ECMA-70 & ANSI X3.125-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 1,9 Tracks per mm",
        MediumTypes.ECMA78 =>
            "ECMA-78 & ANSI X3.126-1986: 130 mm Flexible Disk Cartridge using MFM Recording at 7958 ftprad on Both Sides; 3,8 Tracks per mm",
        MediumTypes.ECMA99 =>
            "ECMA-99 & ISO 8630-1985: 130 mm Flexible Disk Cartridge using MFM Recording at 13262 ftprad on Both Sides; 3,8 Tracks per mm",
        MediumTypes.ECMA100 =>
            "ECMA-100 & ANSI X3.137: 90 mm Flexible Disk Cartridge using MFM Recording at 7859 ftprad on Both Sides; 5,3 Tracks per mm",

        // Most probably they will never appear, but magneto-opticals use these codes
        /*
case MediumTypes.Unspecified_SS:
return "Unspecified single sided flexible disk";
case MediumTypes.Unspecified_DS:
return "Unspecified double sided flexible disk";
*/
        MediumTypes.X3_73       => "ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 1 side",
        MediumTypes.X3_73_DS    => "ANSI X3.73-1980: 200 mm, 6631 ftprad, 1,9 Tracks per mm, 2 sides",
        MediumTypes.X3_82       => "ANSI X3.80-1980: 130 mm, 3979 ftprad, 1,9 Tracks per mm, 1 side",
        MediumTypes.Type3Floppy => "3.5-inch, 135 tpi, 12362 bits/radian, double-sided MFM (aka 1.25Mb)",
        MediumTypes.HDFloppy    => "3.5-inch, 135 tpi, 15916 bits/radian, double-sided MFM (aka 1.44Mb)",
        MediumTypes.ReadOnly    => "a Read-only optical",
        MediumTypes.WORM        => "a Write-once Read-many optical",
        MediumTypes.Erasable    => "a Erasable optical",
        MediumTypes.RO_WORM     => "a combination of read-only and write-once optical",

        // These magneto-opticals were never manufactured
        /*
case MediumTypes.RO_RW:
return "a combination of read-only and erasable optical";
break;
case MediumTypes.WORM_RW:
return "a combination of write-once and erasable optical";
*/
        MediumTypes.DOW  => "a direct-overwrite optical",
        MediumTypes.HiMD => "a Sony Hi-MD disc",
        _                => $"Unknown medium type 0x{(byte)type:X2}"
    };

    public static string PrettifyModeHeader(ModeHeader? header, PeripheralDeviceTypes deviceType)
    {
        if(!header.HasValue)
            return null;

        var sb = new StringBuilder();

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
                    foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                    {
                        string density = "";

                        switch(descriptor.Density)
                        {
                            case DensityType.Default: break;
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
                                density = $"with unknown density code 0x{(byte)descriptor.Density:X2}";

                                break;
                        }

                        if(density != "")
                            if(descriptor.Blocks == 0)
                                sb.AppendFormat("\tAll remaining blocks have {0} and are {1} bytes each", density,
                                                descriptor.BlockLength).AppendLine();
                            else
                                sb.AppendFormat("\t{0} blocks have {1} and are {2} bytes each", descriptor.Blocks,
                                                density, descriptor.BlockLength).AppendLine();
                        else if(descriptor.Blocks == 0)
                            sb.AppendFormat("\tAll remaining blocks are {0} bytes each", descriptor.BlockLength).
                               AppendLine();
                        else
                            sb.AppendFormat("\t{0} blocks are {1} bytes each", descriptor.Blocks,
                                            descriptor.BlockLength).AppendLine();
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
                        sb.AppendFormat("\tUnknown buffered mode code 0x{0:X2}", header.Value.BufferedMode).
                           AppendLine();

                        break;
                }

                if(header.Value.Speed == 0)
                    sb.AppendLine("\tDevice uses default speed");
                else
                    sb.AppendFormat("\tDevice uses speed {0}", header.Value.Speed).AppendLine();

                if(header.Value.WriteProtected)
                    sb.AppendLine("\tMedium is write protected");

                string medium = header.Value.MediumType switch
                {
                    MediumTypes.Default       => "undefined",
                    MediumTypes.Tape12        => "6,3 mm tape with 12 tracks at 394 ftpmm or DC-9250",
                    MediumTypes.Tape24        => "6,3 mm tape with 24 tracks at 394 ftpmm or MLR1-26GBSL",
                    MediumTypes.LTOWORM       => "LTO Ultrium WORM or cleaning cartridge",
                    MediumTypes.LTO           => "LTO Ultrium",
                    MediumTypes.LTO2          => "LTO Ultrium-2",
                    MediumTypes.DC2900SL      => "DC-2900SL",
                    MediumTypes.MLR1          => "MLR1-26GB or DDS-3",
                    MediumTypes.DC9200        => "DC-9200 or DDS-4",
                    MediumTypes.DAT72         => "DAT-72",
                    MediumTypes.LTO3          => "LTO Ultrium-3",
                    MediumTypes.LTO3WORM      => "LTO Ultrium-3 WORM",
                    MediumTypes.DDSCleaning   => "DDS cleaning cartridge",
                    MediumTypes.SLR32         => "SLR-32",
                    MediumTypes.SLRtape50     => "SLRtape-50",
                    MediumTypes.LTO4          => "LTO Ultrium-4",
                    MediumTypes.LTO4WORM      => "LTO Ultrium-4 WORM",
                    MediumTypes.SLRtape50SL   => "SLRtape-50 SL",
                    MediumTypes.SLR32SL       => "SLR-32SL",
                    MediumTypes.SLR5          => "SLR-5",
                    MediumTypes.SLR5SL        => "SLR-5SL",
                    MediumTypes.LTO5          => "LTO Ultrium-5",
                    MediumTypes.LTO5WORM      => "LTO Ultrium-5 WORM",
                    MediumTypes.SLRtape7      => "SLRtape-7",
                    MediumTypes.SLRtape7SL    => "SLRtape-7 SL",
                    MediumTypes.SLRtape24     => "SLRtape-24",
                    MediumTypes.SLRtape24SL   => "SLRtape-24 SL",
                    MediumTypes.LTO6          => "LTO Ultrium-6",
                    MediumTypes.LTO6WORM      => "LTO Ultrium-6 WORM",
                    MediumTypes.SLRtape140    => "SLRtape-140",
                    MediumTypes.SLRtape40     => "SLRtape-40",
                    MediumTypes.SLRtape60     => "SLRtape-60 or SLRtape-75",
                    MediumTypes.SLRtape100    => "SLRtape-100",
                    MediumTypes.SLR40_60_100  => "SLR-40, SLR-60 or SLR-100",
                    MediumTypes.LTO7          => "LTO Ultrium-7",
                    MediumTypes.LTO7WORM      => "LTO Ultrium-7 WORM",
                    MediumTypes.LTOCD         => "LTO Ultrium",
                    MediumTypes.Exatape15m    => "Exatape 15m, IBM MagStar or VXA",
                    MediumTypes.CT1           => "CompactTape I, Exatape 28m, CompactTape II, VXA-2 or VXA-3",
                    MediumTypes.Exatape54m    => "Exatape 54m or DLTtape III",
                    MediumTypes.Exatape80m    => "Exatape 80m or DLTtape IIIxt",
                    MediumTypes.Exatape106m   => "Exatape 106m, DLTtape IV or Travan 5",
                    MediumTypes.Exatape106mXL => "Exatape 160m XL or Super DLTtape I",
                    MediumTypes.SDLT2         => "Super DLTtape II",
                    MediumTypes.VStapeI       => "VStape I",
                    MediumTypes.DLTtapeS4     => "DLTtape S4",
                    MediumTypes.Travan7       => "Travan 7",
                    MediumTypes.Exatape22m    => "Exatape 22m",
                    MediumTypes.Exatape40m    => "Exatape 40m",
                    MediumTypes.Exatape76m    => "Exatape 76m",
                    MediumTypes.Exatape112m   => "Exatape 112m",
                    MediumTypes.Exatape22mAME => "Exatape 22m AME",
                    MediumTypes.Exatape170m   => "Exatape 170m",
                    MediumTypes.Exatape125m   => "Exatape 125m",
                    MediumTypes.Exatape45m    => "Exatape 45m",
                    MediumTypes.Exatape225m   => "Exatape 225m",
                    MediumTypes.Exatape150m   => "Exatape 150m",
                    MediumTypes.Exatape75m    => "Exatape 75m",
                    _                         => $"unknown medium type 0x{(byte)header.Value.MediumType:X2}"
                };

                sb.AppendFormat("\tMedium is {0}", medium).AppendLine();

                if(header.Value.BlockDescriptors != null)
                    foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                    {
                        string density = "";

                        switch(header.Value.MediumType)
                        {
                            case MediumTypes.Default:
                            {
                                switch(descriptor.Density)
                                {
                                    case DensityType.Default: break;
                                    case DensityType.ECMA62:
                                        density =
                                            "ECMA-62 & ANSI X3.22-1983: 12,7 mm 9-Track Magnetic Tape, 32 ftpmm, NRZI, 32 cpmm";

                                        break;
                                    case DensityType.ECMA62_Phase:
                                        density =
                                            "ECMA-62 & ANSI X3.39-1986: 12,7 mm 9-Track Magnetic Tape, 126 ftpmm, Phase Encoding, 63 cpmm";

                                        break;
                                    case DensityType.ECMA62_GCR:
                                        density =
                                            "ECMA-62 & ANSI X3.54-1986: 12,7 mm 9-Track Magnetic Tape, 356 ftpmm, NRZI, 245 cpmm GCR";

                                        break;
                                    case DensityType.ECMA79:
                                        density =
                                            "ECMA-79 & ANSI X3.116-1986: 6,30 mm Magnetic Tape Cartridge, 252 ftpmm, MFM";

                                        break;
                                    case DensityType.IBM3480:
                                        density =
                                            "Draft ECMA & ANSI X3B5/87-099: 12,7 mm 18-Track Magnetic Tape Cartridge, 1944 ftpmm, IFM, GCR (IBM 3480, 3490, 3490E)";

                                        break;
                                    case DensityType.ECMA46:
                                        density =
                                            "ECMA-46 & ANSI X3.56-1986: 6,30 mm Magnetic Tape Cartridge, Phase Encoding, 63 bpmm";

                                        break;
                                    case DensityType.ECMA98:
                                        density = "ECMA-98: 6,30 mm Magnetic Tape Cartridge, NRZI, 394 ftpmm";

                                        break;
                                    case DensityType.X3_136:
                                        density =
                                            "ANSI X3.136-1986: 6,3 mm 4 or 9-Track Magnetic Tape Cartridge, 315 bpmm, GCR (QIC-24)";

                                        break;
                                    case DensityType.X3_157:
                                        density =
                                            "ANSI X3.157-1987: 12,7 mm 9-Track Magnetic Tape, 126 bpmm, Phase Encoding";

                                        break;
                                    case DensityType.X3_158:
                                        density =
                                            "ANSI X3.158-1987: 3,81 mm 4-Track Magnetic Tape Cassette, 315 bpmm, GCR";

                                        break;
                                    case DensityType.X3B5_86:
                                        density =
                                            "ANSI X3B5/86-199: 12,7 mm 22-Track Magnetic Tape Cartridge, 262 bpmm, MFM";

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
                                        density = "ANSI X3B5/88-185A: 3,81 mm Magnetic Tape Cassette, 2400 bpmm, DDS";

                                        break;
                                    case DensityType.X3_202:
                                        density = "ANSI X3.202-1991: 8 mm Magnetic Tape Cassette, 1703 bpmm, RLL";

                                        break;
                                    case DensityType.ECMA_TC17:
                                        density = "ECMA TC17: 8 mm Magnetic Tape Cassette, 1789 bpmm, RLL";

                                        break;
                                    case DensityType.X3_193:
                                        density =
                                            "ANSI X3.193-1990: 12,7 mm 48-Track Magnetic Tape Cartridge, 394 bpmm, MFM";

                                        break;
                                    case DensityType.X3B5_91:
                                        density =
                                            "ANSI X3B5/97-174: 12,7 mm 48-Track Magnetic Tape Cartridge, 1673 bpmm, MFM";

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
                                        density = $"unknown density code 0x{(byte)descriptor.Density:X2}";

                                        break;
                                }
                            }

                                break;
                            case MediumTypes.LTOWORM:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "LTO Ultrium cleaning cartridge",
                                    DensityType.LTO3    => "LTO Ultrium-3 WORM",
                                    DensityType.LTO4    => "LTO Ultrium-4 WORM",
                                    DensityType.LTO5    => "LTO Ultrium-5 WORM",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.LTO:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.LTO1 => "LTO Ultrium",
                                    _                => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.LTO2:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.LTO2 => "LTO Ultrium-2",
                                    _                => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.DDS3:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "MLR1-26GB",
                                    DensityType.DDS3    => "DDS-3",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.DDS4:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "DC-9200",
                                    DensityType.DDS4    => "DDS-4",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.DAT72:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.DAT72 => "DAT-72",
                                    _                 => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.LTO3:
                            case MediumTypes.LTO3WORM:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.LTO3 => "LTO Ultrium-3",
                                    _                => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.DDSCleaning:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "DDS cleaning cartridge",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.LTO4:
                            case MediumTypes.LTO4WORM:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.LTO4 => "LTO Ultrium-4",
                                    _                => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.LTO5:
                            case MediumTypes.LTO5WORM:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.LTO5 => "LTO Ultrium-5",
                                    _                => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.LTO6:
                            case MediumTypes.LTO6WORM:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.LTO6 => "LTO Ultrium-6",
                                    _                => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.LTO7:
                            case MediumTypes.LTO7WORM:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.LTO7 => "LTO Ultrium-7",
                                    _                => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.LTOCD:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.LTO2 => "LTO Ultrium-2 in CD emulation mode",
                                    DensityType.LTO3 => "LTO Ultrium-3 in CD emulation mode",
                                    DensityType.LTO4 => "LTO Ultrium-4 in CD emulation mode",
                                    DensityType.LTO5 => "LTO Ultrium-5 in CD emulation mode",
                                    _                => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.Exatape15m:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Ex8200   => "EXB-8200",
                                    DensityType.Ex8200c  => "EXB-8200 compressed",
                                    DensityType.Ex8500   => "EXB-8500",
                                    DensityType.Ex8500c  => "EXB-8500 compressed",
                                    DensityType.Mammoth  => "Mammoth",
                                    DensityType.IBM3590  => "IBM 3590",
                                    DensityType.IBM3590E => "IBM 3590E",
                                    DensityType.VXA1     => "VXA-1",
                                    _                    => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.Exatape28m:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Ex8200   => "EXB-8200",
                                    DensityType.Ex8200c  => "EXB-8200 compressed",
                                    DensityType.Ex8500   => "EXB-8500",
                                    DensityType.Ex8500c  => "EXB-8500 compressed",
                                    DensityType.Mammoth  => "Mammoth",
                                    DensityType.CT1      => "CompactTape I",
                                    DensityType.CT2      => "CompactTape II",
                                    DensityType.IBM3590  => "IBM 3590 extended",
                                    DensityType.IBM3590E => "IBM 3590E extended",
                                    DensityType.VXA2     => "VXA-2",
                                    DensityType.VXA3     => "VXA-3",
                                    _                    => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
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
                                        density = $"unknown density code 0x{(byte)descriptor.Density:X2}";

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
                                        density = $"unknown density code 0x{(byte)descriptor.Density:X2}";

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
                                        density = $"unknown density code 0x{(byte)descriptor.Density:X2}";

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
                                        density = $"unknown density code 0x{(byte)descriptor.Density:X2}";

                                        break;
                                }
                            }

                                break;
                            case MediumTypes.SDLT2:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.SDLT2 => "Super DLTtape II",
                                    _                 => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
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
                                        density = $"unknown density code 0x{(byte)descriptor.Density:X2}";

                                        break;
                                }
                            }

                                break;
                            case MediumTypes.DLTtapeS4:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.DLTS4 => "DLTtape S4",
                                    _                 => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.Exatape22m:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Ex8200  => "EXB-8200",
                                    DensityType.Ex8200c => "EXB-8200 compressed",
                                    DensityType.Ex8500  => "EXB-8500",
                                    DensityType.Ex8500c => "EXB-8500 compressed",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.Exatape40m:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Ex8200  => "EXB-8200",
                                    DensityType.Ex8200c => "EXB-8200 compressed",
                                    DensityType.Ex8500  => "EXB-8500",
                                    DensityType.Ex8500c => "EXB-8500 compressed",
                                    DensityType.Mammoth => "Mammoth",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.Exatape76m:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Ex8200  => "EXB-8200",
                                    DensityType.Ex8200c => "EXB-8200 compressed",
                                    DensityType.Ex8500  => "EXB-8500",
                                    DensityType.Ex8500c => "EXB-8500 compressed",
                                    DensityType.Mammoth => "Mammoth",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.Exatape112m:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Ex8200  => "EXB-8200",
                                    DensityType.Ex8200c => "EXB-8200 compressed",
                                    DensityType.Ex8500  => "EXB-8500",
                                    DensityType.Ex8500c => "EXB-8500 compressed",
                                    DensityType.Mammoth => "Mammoth",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
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
                                density = descriptor.Density switch
                                {
                                    DensityType.Mammoth  => "Mammoth",
                                    DensityType.Mammoth2 => "Mammoth-2",
                                    _                    => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.DC2900SL:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "DC-2900SL",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.DC9250:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "DC-9250",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLR32:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLR-32",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.MLR1SL:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "MRL1-26GBSL",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLRtape50:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLRtape-50",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLRtape50SL:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLRtape-50 SL",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLR32SL:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLR-32 SL",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLR5:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLR-5",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLR5SL:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLR-5 SL",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLRtape7:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLRtape-7",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLRtape7SL:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLRtape-7 SL",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLRtape24:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLRtape-24",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLRtape24SL:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLRtape-24 SL",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLRtape140:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLRtape-140",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLRtape40:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLRtape-40",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLRtape60:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLRtape-60 or SLRtape-75",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLRtape100:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLRtape-100",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            case MediumTypes.SLR40_60_100:
                            {
                                density = descriptor.Density switch
                                {
                                    DensityType.Default => "SLR40, SLR60 or SLR100",
                                    _                   => $"unknown density code 0x{(byte)descriptor.Density:X2}"
                                };
                            }

                                break;
                            default:
                                density = $"unknown density code 0x{(byte)descriptor.Density:X2}";

                                break;
                        }

                        if(density != "")
                            if(descriptor.Blocks == 0)
                                if(descriptor.BlockLength == 0)
                                    sb.AppendFormat("\tAll remaining blocks conform to {0} and have a variable length",
                                                    density).AppendLine();
                                else
                                    sb.AppendFormat("\tAll remaining blocks conform to {0} and are {1} bytes each",
                                                    density, descriptor.BlockLength).AppendLine();
                            else if(descriptor.BlockLength == 0)
                                sb.AppendFormat("\t{0} blocks conform to {1} and have a variable length",
                                                descriptor.Blocks, density).AppendLine();
                            else
                                sb.AppendFormat("\t{0} blocks conform to {1} and are {2} bytes each", descriptor.Blocks,
                                                density, descriptor.BlockLength).AppendLine();
                        else if(descriptor.Blocks == 0)
                            if(descriptor.BlockLength == 0)
                                sb.AppendFormat("\tAll remaining blocks have a variable length").AppendLine();
                            else
                                sb.AppendFormat("\tAll remaining blocks are {0} bytes each", descriptor.BlockLength).
                                   AppendLine();
                        else if(descriptor.BlockLength == 0)
                            sb.AppendFormat("\t{0} blocks have a variable length", descriptor.Blocks).AppendLine();
                        else
                            sb.AppendFormat("\t{0} blocks are {1} bytes each", descriptor.Blocks,
                                            descriptor.BlockLength).AppendLine();
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
                        sb.AppendFormat("\tUnknown buffered mode code 0x{0:X2}", header.Value.BufferedMode).
                           AppendLine();

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
                            sb.AppendFormat("an unknown medium type 0x{0:X2}", (byte)header.Value.MediumType).
                               AppendLine();

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
                    foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                    {
                        string density = "";

                        switch(descriptor.Density)
                        {
                            case DensityType.Default: break;
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
                                density = $"unknown density code 0x{(byte)descriptor.Density:X2}";

                                break;
                        }

                        if(density != "")
                            if(descriptor.Blocks == 0)
                                if(descriptor.BlockLength == 0)
                                    sb.AppendFormat("\tAll remaining blocks are {0} and have a variable length",
                                                    density).AppendLine();
                                else
                                    sb.AppendFormat("\tAll remaining blocks are {0} and are {1} bytes each", density,
                                                    descriptor.BlockLength).AppendLine();
                            else if(descriptor.BlockLength == 0)
                                sb.AppendFormat("\t{0} blocks are {1} and have a variable length", descriptor.Blocks,
                                                density).AppendLine();
                            else
                                sb.AppendFormat("\t{0} blocks are {1} and are {2} bytes each", descriptor.Blocks,
                                                density, descriptor.BlockLength).AppendLine();
                        else if(descriptor.Blocks == 0)
                            if(descriptor.BlockLength == 0)
                                sb.AppendFormat("\tAll remaining blocks have a variable length").AppendLine();
                            else
                                sb.AppendFormat("\tAll remaining blocks are {0} bytes each", descriptor.BlockLength).
                                   AppendLine();
                        else if(descriptor.BlockLength == 0)
                            sb.AppendFormat("\t{0} blocks have a variable length", descriptor.Blocks).AppendLine();
                        else
                            sb.AppendFormat("\t{0} blocks are {1} bytes each", descriptor.Blocks,
                                            descriptor.BlockLength).AppendLine();
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
                        sb.AppendFormat("Unknown medium type 0x{0:X2}", (byte)header.Value.MediumType).AppendLine();

                        break;
                }

                if(header.Value.WriteProtected)
                    sb.AppendLine("\tMedium is write protected");

                if(header.Value.DPOFUA)
                    sb.AppendLine("\tDrive supports DPO and FUA bits");

                if(header.Value.BlockDescriptors != null)
                    foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                    {
                        string density = "";

                        switch(descriptor.Density)
                        {
                            case DensityType.Default: break;
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
                                density = $"with unknown density code 0x{(byte)descriptor.Density:X2}";

                                break;
                        }

                        if(density != "")
                            if(descriptor.Blocks == 0)
                                sb.AppendFormat("\tAll remaining blocks have {0} and are {1} bytes each", density,
                                                descriptor.BlockLength).AppendLine();
                            else
                                sb.AppendFormat("\t{0} blocks have {1} and are {2} bytes each", descriptor.Blocks,
                                                density, descriptor.BlockLength).AppendLine();
                        else if(descriptor.Blocks == 0)
                            sb.AppendFormat("\tAll remaining blocks are {0} bytes each", descriptor.BlockLength).
                               AppendLine();
                        else
                            sb.AppendFormat("\t{0} blocks are {1} bytes each", descriptor.Blocks,
                                            descriptor.BlockLength).AppendLine();
                    }

                break;
            }
            #endregion Multimedia device mode header
        }

        return sb.ToString();
    }
}