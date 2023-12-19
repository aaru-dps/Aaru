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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.Structs.Devices.SCSI;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    public static string GetMediumTypeDescription(MediumTypes type) => type switch
                                                                       {
                                                                           MediumTypes.ECMA54 => Localization.
                                                                               GetMediumTypeDescription_ECMA_54,
                                                                           MediumTypes.ECMA59 => Localization.
                                                                               GetMediumTypeDescription_ECMA_59,
                                                                           MediumTypes.ECMA69 => Localization.
                                                                               GetMediumTypeDescription_ECMA_69,
                                                                           MediumTypes.ECMA66 => Localization.
                                                                               GetMediumTypeDescription_ECMA_66,
                                                                           MediumTypes.ECMA70 => Localization.
                                                                               GetMediumTypeDescription_ECMA_70,
                                                                           MediumTypes.ECMA78 => Localization.
                                                                               GetMediumTypeDescription_ECMA_78,
                                                                           MediumTypes.ECMA99 => Localization.
                                                                               GetMediumTypeDescription_ECMA_99,
                                                                           MediumTypes.ECMA100 => Localization.
                                                                               GetMediumTypeDescription_ECMA_100,

                                                                           // Most probably they will never appear, but magneto-opticals use these codes
                                                                           /*
                                                                   case MediumTypes.Unspecified_SS:
                                                                   return "Unspecified single sided flexible disk";
                                                                   case MediumTypes.Unspecified_DS:
                                                                   return "Unspecified double sided flexible disk";
                                                                   */
                                                                           MediumTypes.X3_73 => Localization.
                                                                               GetMediumTypeDescription_X3_73,
                                                                           MediumTypes.X3_73_DS => Localization.
                                                                               GetMediumTypeDescription_X3_73_DS,
                                                                           MediumTypes.X3_82 => Localization.
                                                                               GetMediumTypeDescription_X3_82,
                                                                           MediumTypes.Type3Floppy => Localization.
                                                                               GetMediumTypeDescription_Type3Floppy,
                                                                           MediumTypes.HDFloppy => Localization.
                                                                               GetMediumTypeDescription_HDFloppy,
                                                                           MediumTypes.ReadOnly => Localization.
                                                                               GetMediumTypeDescription_ReadOnly,
                                                                           MediumTypes.WORM => Localization.
                                                                               GetMediumTypeDescription_WORM,
                                                                           MediumTypes.Erasable => Localization.
                                                                               GetMediumTypeDescription_Erasable,
                                                                           MediumTypes.RO_WORM => Localization.
                                                                               GetMediumTypeDescription_RO_WORM,

                                                                           // These magneto-opticals were never manufactured
                                                                           /*
                                                                   case MediumTypes.RO_RW:
                                                                   return "a combination of read-only and erasable optical";
                                                                   break;
                                                                   case MediumTypes.WORM_RW:
                                                                   return "a combination of write-once and erasable optical";
                                                                   */
                                                                           MediumTypes.DOW => Localization.
                                                                               GetMediumTypeDescription_DOW,
                                                                           MediumTypes.HiMD => Localization.
                                                                               GetMediumTypeDescription_HiMD,
                                                                           _ => string.
                                                                               Format(Localization.Unknown_medium_type_0,
                                                                                   (byte)type)
                                                                       };

    public static string PrettifyModeHeader(ModeHeader? header, PeripheralDeviceTypes deviceType)
    {
        if(!header.HasValue)
            return null;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.SCSI_Mode_Sense_Header);

        switch(deviceType)
        {
        #region Direct access device mode header

            case PeripheralDeviceTypes.DirectAccess:
            {
                if(header.Value.MediumType != MediumTypes.Default)
                {
                    sb.AppendFormat("\t" + Localization.Medium_is_0, GetMediumTypeDescription(header.Value.MediumType)).
                       AppendLine();
                }

                if(header.Value.WriteProtected)
                    sb.AppendLine("\t" + Localization.Medium_is_write_protected);

                if(header.Value.DPOFUA)
                    sb.AppendLine("\t" + Localization.Drive_supports_DPO_and_FUA_bits);

                if(header.Value.BlockDescriptors != null)
                {
                    foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                    {
                        var density = "";

                        switch(descriptor.Density)
                        {
                            case DensityType.Default:
                                break;
                            case DensityType.Flux7958:
                                density = Localization._7958_ftprad;

                                break;
                            case DensityType.Flux13262:
                                density = Localization._13262_ftprad;

                                break;
                            case DensityType.Flux15916:
                                density = Localization._15916_ftprad;

                                break;
                            default:
                                density = string.Format(Localization.with_unknown_density_code_0,
                                                        (byte)descriptor.Density);

                                break;
                        }

                        if(density != "")
                        {
                            if(descriptor.Blocks == 0)
                            {
                                sb.AppendFormat("\t" + Localization.All_remaining_blocks_have_0_and_are_1_bytes_each,
                                                density, descriptor.BlockLength).
                                   AppendLine();
                            }
                            else
                            {
                                sb.AppendFormat("\t" + Localization._0_blocks_have_1_and_are_2_bytes_each,
                                                descriptor.Blocks, density, descriptor.BlockLength).
                                   AppendLine();
                            }
                        }
                        else if(descriptor.Blocks == 0)
                        {
                            sb.AppendFormat("\t" + Localization.All_remaining_blocks_are_0_bytes_each,
                                            descriptor.BlockLength).
                               AppendLine();
                        }
                        else
                        {
                            sb.AppendFormat("\t" + Localization._0_blocks_are_1_bytes_each, descriptor.Blocks,
                                            descriptor.BlockLength).
                               AppendLine();
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
                        sb.AppendLine("\t" + Localization.Device_writes_directly_to_media);

                        break;
                    case 1:
                        sb.AppendLine("\t" + Localization.Device_uses_a_write_cache);

                        break;
                    case 2:
                        sb.AppendLine("\t" +
                                      Localization.Device_uses_a_write_cache_but_doesn_t_return_until_cache_is_flushed);

                        break;
                    default:
                        sb.AppendFormat("\t" + Localization.Unknown_buffered_mode_code_0, header.Value.BufferedMode).
                           AppendLine();

                        break;
                }

                if(header.Value.Speed == 0)
                    sb.AppendLine("\t" + Localization.Device_uses_default_speed);
                else
                    sb.AppendFormat("\t" + Localization.Device_uses_speed_0, header.Value.Speed).AppendLine();

                if(header.Value.WriteProtected)
                    sb.AppendLine("\t" + Localization.Medium_is_write_protected);

                string medium = header.Value.MediumType switch
                                {
                                    MediumTypes.Default       => Localization.MediumType_undefined,
                                    MediumTypes.Tape12        => Localization.MediumType_Tape12,
                                    MediumTypes.Tape24        => Localization.MediumType_Tape24,
                                    MediumTypes.LTOWORM       => Localization.MediumType_LTOWORM,
                                    MediumTypes.LTO           => Localization.MediumType_LTO,
                                    MediumTypes.LTO2          => Localization.MediumType_LTO2,
                                    MediumTypes.DC2900SL      => Localization.MediumType_DC2900SL,
                                    MediumTypes.MLR1          => Localization.MediumType_MLR1,
                                    MediumTypes.DC9200        => Localization.MediumType_DC9200,
                                    MediumTypes.DAT72         => Localization.MediumType_DAT72,
                                    MediumTypes.LTO3          => Localization.MediumType_LTO3,
                                    MediumTypes.LTO3WORM      => Localization.MediumType_LTO3WORM,
                                    MediumTypes.DDSCleaning   => Localization.MediumType_DDSCleaning,
                                    MediumTypes.SLR32         => Localization.MediumType_SLR32,
                                    MediumTypes.SLRtape50     => Localization.MediumType_SLRtape50,
                                    MediumTypes.LTO4          => Localization.MediumType_LTO4,
                                    MediumTypes.LTO4WORM      => Localization.MediumType_LTO4WORM,
                                    MediumTypes.SLRtape50SL   => Localization.MediumType_SLRtape50SL,
                                    MediumTypes.SLR32SL       => Localization.MediumType_SLR32SL,
                                    MediumTypes.SLR5          => Localization.MediumType_SLR5,
                                    MediumTypes.SLR5SL        => Localization.MediumType_SLR5SL,
                                    MediumTypes.LTO5          => Localization.MediumType_LTO5,
                                    MediumTypes.LTO5WORM      => Localization.MediumType_LTO5WORM,
                                    MediumTypes.SLRtape7      => Localization.MediumType_SLRtape7,
                                    MediumTypes.SLRtape7SL    => Localization.MediumType_SLRtape7SL,
                                    MediumTypes.SLRtape24     => Localization.MediumType_SLRtape24,
                                    MediumTypes.SLRtape24SL   => Localization.MediumType_SLRtape24SL,
                                    MediumTypes.LTO6          => Localization.MediumType_LTO6,
                                    MediumTypes.LTO6WORM      => Localization.MediumType_LTO6WORM,
                                    MediumTypes.SLRtape140    => Localization.MediumType_SLRtape140,
                                    MediumTypes.SLRtape40     => Localization.MediumType_SLRtape40,
                                    MediumTypes.SLRtape60     => Localization.MediumType_SLRtape60,
                                    MediumTypes.SLRtape100    => Localization.MediumType_SLRtape100,
                                    MediumTypes.SLR40_60_100  => Localization.MediumType_SLR40_60_100,
                                    MediumTypes.LTO7          => Localization.MediumType_LTO7,
                                    MediumTypes.LTO7WORM      => Localization.MediumType_LTO7WORM,
                                    MediumTypes.LTOCD         => Localization.MediumType_LTO,
                                    MediumTypes.Exatape15m    => Localization.MediumType_Exatape15m,
                                    MediumTypes.CT1           => Localization.MediumType_CT1,
                                    MediumTypes.Exatape54m    => Localization.MediumType_Exatape54m,
                                    MediumTypes.Exatape80m    => Localization.MediumType_Exatape80m,
                                    MediumTypes.Exatape106m   => Localization.MediumType_Exatape106m,
                                    MediumTypes.Exatape106mXL => Localization.MediumType_Exatape106mXL,
                                    MediumTypes.SDLT2         => Localization.MediumType_SDLT2,
                                    MediumTypes.VStapeI       => Localization.MediumType_VStapeI,
                                    MediumTypes.DLTtapeS4     => Localization.MediumType_DLTtapeS4,
                                    MediumTypes.Travan7       => Localization.MediumType_Travan7,
                                    MediumTypes.Exatape22m    => Localization.MediumType_Exatape22m,
                                    MediumTypes.Exatape40m    => Localization.MediumType_Exatape40m,
                                    MediumTypes.Exatape76m    => Localization.MediumType_Exatape76m,
                                    MediumTypes.Exatape112m   => Localization.MediumType_Exatape112m,
                                    MediumTypes.Exatape22mAME => Localization.MediumType_Exatape22mAME,
                                    MediumTypes.Exatape170m   => Localization.MediumType_Exatape170m,
                                    MediumTypes.Exatape125m   => Localization.MediumType_Exatape125m,
                                    MediumTypes.Exatape45m    => Localization.MediumType_Exatape45m,
                                    MediumTypes.Exatape225m   => Localization.MediumType_Exatape225m,
                                    MediumTypes.Exatape150m   => Localization.MediumType_Exatape150m,
                                    MediumTypes.Exatape75m    => Localization.MediumType_Exatape75m,
                                    _ => string.Format(Localization.Unknown_medium_type_0,
                                                       (byte)header.Value.MediumType)
                                };

                sb.AppendFormat("\t" + Localization.Medium_is_0, medium).AppendLine();

                if(header.Value.BlockDescriptors != null)
                {
                    foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                    {
                        var density = "";

                        switch(header.Value.MediumType)
                        {
                            case MediumTypes.Default:
                            {
                                switch(descriptor.Density)
                                {
                                    case DensityType.Default:
                                        break;
                                    case DensityType.ECMA62:
                                        density = Localization.ECMA62;

                                        break;
                                    case DensityType.ECMA62_Phase:
                                        density = Localization.ECMA62_Phase;

                                        break;
                                    case DensityType.ECMA62_GCR:
                                        density = Localization.ECMA62_GCR;

                                        break;
                                    case DensityType.ECMA79:
                                        density = Localization.ECMA79;

                                        break;
                                    case DensityType.IBM3480:
                                        density = Localization.IBM3480;

                                        break;
                                    case DensityType.ECMA46:
                                        density = Localization.ECMA46;

                                        break;
                                    case DensityType.ECMA98:
                                        density = Localization.ECMA98;

                                        break;
                                    case DensityType.X3_136:
                                        density = Localization.X3_136;

                                        break;
                                    case DensityType.X3_157:
                                        density = Localization.X3_157;

                                        break;
                                    case DensityType.X3_158:
                                        density = Localization.X3_158;

                                        break;
                                    case DensityType.X3B5_86:
                                        density = Localization.X3B5_86;

                                        break;
                                    case DensityType.HiTC1:
                                        density = Localization.HiTC1;

                                        break;
                                    case DensityType.HiTC2:
                                        density = Localization.HiTC2;

                                        break;
                                    case DensityType.QIC120:
                                        density = Localization.QIC120;

                                        break;
                                    case DensityType.QIC150:
                                        density = Localization.QIC150;

                                        break;
                                    case DensityType.QIC320:
                                        density = Localization.QIC320;

                                        break;
                                    case DensityType.QIC1350:
                                        density = Localization.QIC1350;

                                        break;
                                    case DensityType.X3B5_88:
                                        density = Localization.X3B5_88;

                                        break;
                                    case DensityType.X3_202:
                                        density = Localization.X3_202;

                                        break;
                                    case DensityType.ECMA_TC17:
                                        density = Localization.ECMA_TC17;

                                        break;
                                    case DensityType.X3_193:
                                        density = Localization.X3_193;

                                        break;
                                    case DensityType.X3B5_91:
                                        density = Localization.X3B5_91;

                                        break;
                                    case DensityType.QIC11:
                                        density = Localization.QIC11;

                                        break;
                                    case DensityType.IBM3490E:
                                        density = Localization.IBM3490E;

                                        break;
                                    case DensityType.LTO1:
                                        //case DensityType.SAIT1:
                                        density = Localization.LTO_or_SAIT1;

                                        break;
                                    case DensityType.LTO2Old:
                                        density = Localization.MediumType_LTO2;

                                        break;
                                    case DensityType.LTO2:
                                        //case DensityType.T9840:
                                        density = Localization.LTO2_or_T9840;

                                        break;
                                    case DensityType.T9940:
                                        density = Localization.T9940;

                                        break;
                                    case DensityType.LTO3:
                                        //case DensityType.T9940:
                                        density = Localization.LTO3_or_T9940;

                                        break;
                                    case DensityType.T9840C:
                                        density = Localization.T9840C;

                                        break;
                                    case DensityType.LTO4:
                                        //case DensityType.T9840D:
                                        density = Localization.LTO4_or_T9840D;

                                        break;
                                    case DensityType.T10000A:
                                        density = Localization.T10000A;

                                        break;
                                    case DensityType.T10000B:
                                        density = Localization.T10000B;

                                        break;
                                    case DensityType.T10000C:
                                        density = Localization.T10000C;

                                        break;
                                    case DensityType.T10000D:
                                        density = Localization.T10000D;

                                        break;
                                    case DensityType.AIT1:
                                        density = Localization.AIT1;

                                        break;
                                    case DensityType.AIT2:
                                        density = Localization.AIT2;

                                        break;
                                    case DensityType.AIT3:
                                        density = Localization.AIT3;

                                        break;
                                    case DensityType.DDS2:
                                        density = Localization.DDS2;

                                        break;
                                    case DensityType.DDS3:
                                        density = Localization.DDS3;

                                        break;
                                    case DensityType.DDS4:
                                        density = Localization.DDS4;

                                        break;
                                    default:
                                        density = string.Format(Localization.unknown_density_code_0,
                                                                (byte)descriptor.Density);

                                        break;
                                }
                            }

                                break;
                            case MediumTypes.LTOWORM:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.LTO_Ultrium_cleaning_cartridge,
                                              DensityType.LTO3    => Localization.MediumType_LTO3WORM,
                                              DensityType.LTO4    => Localization.MediumType_LTO4WORM,
                                              DensityType.LTO5    => Localization.MediumType_LTO5WORM,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.LTO:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.LTO1 => Localization.MediumType_LTO,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.LTO2:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.LTO2 => Localization.MediumType_LTO2,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.DDS3:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MLR1_26GB,
                                              DensityType.DDS3    => Localization.DDS3,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.DDS4:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.DC9200,
                                              DensityType.DDS4    => Localization.DDS4,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.DAT72:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.DAT72 => Localization.MediumType_DAT72,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.LTO3:
                            case MediumTypes.LTO3WORM:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.LTO3 => Localization.MediumType_LTO3,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.DDSCleaning:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_DDSCleaning,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.LTO4:
                            case MediumTypes.LTO4WORM:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.LTO4 => Localization.MediumType_LTO4,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.LTO5:
                            case MediumTypes.LTO5WORM:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.LTO5 => Localization.MediumType_LTO5,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.LTO6:
                            case MediumTypes.LTO6WORM:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.LTO6 => Localization.MediumType_LTO6,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.LTO7:
                            case MediumTypes.LTO7WORM:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.LTO7 => Localization.MediumType_LTO7,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.LTOCD:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.LTO2 => Localization.LTO2_CDemu,
                                              DensityType.LTO3 => Localization.LTO3_CDemu,
                                              DensityType.LTO4 => Localization.LTO4_CDemu,
                                              DensityType.LTO5 => Localization.LTO5_CDemu,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.Exatape15m:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Ex8200   => Localization.EXB8200,
                                              DensityType.Ex8200c  => Localization.EXB8200_compressed,
                                              DensityType.Ex8500   => Localization.EXB8500,
                                              DensityType.Ex8500c  => Localization.EXB8500_compressed,
                                              DensityType.Mammoth  => Localization.TapeName_Mammoth,
                                              DensityType.IBM3590  => Localization.IBM3590,
                                              DensityType.IBM3590E => Localization.IBM3590E,
                                              DensityType.VXA1     => Localization.VXA1,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.Exatape28m:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Ex8200   => Localization.EXB8200,
                                              DensityType.Ex8200c  => Localization.EXB8200_compressed,
                                              DensityType.Ex8500   => Localization.EXB8500,
                                              DensityType.Ex8500c  => Localization.EXB8500_compressed,
                                              DensityType.Mammoth  => Localization.TapeName_Mammoth,
                                              DensityType.CT1      => Localization.CT1,
                                              DensityType.CT2      => Localization.CT2,
                                              DensityType.IBM3590  => Localization.IBM3590_extended,
                                              DensityType.IBM3590E => Localization.IBM3590E_extended,
                                              DensityType.VXA2     => Localization.VXA2,
                                              DensityType.VXA3     => Localization.VXA3,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.Exatape54m:
                            {
                                switch(descriptor.Density)
                                {
                                    case DensityType.Ex8200:
                                        density = Localization.EXB8200;

                                        break;
                                    case DensityType.Ex8200c:
                                        density = Localization.EXB8200_compressed;

                                        break;
                                    case DensityType.Ex8500:
                                        density = Localization.EXB8500;

                                        break;
                                    case DensityType.Ex8500c:
                                        density = Localization.EXB8500_compressed;

                                        break;
                                    case DensityType.Mammoth:
                                        density = Localization.TapeName_Mammoth;

                                        break;
                                    case DensityType.DLT3_42k:
                                        density = Localization.DLT3_42k;

                                        break;
                                    case DensityType.DLT3_56t:
                                        density = Localization.DLT3_56t;

                                        break;
                                    case DensityType.DLT3_62k:
                                    case DensityType.DLT3_62kAlt:
                                        density = Localization.DLT3_62k;

                                        break;
                                    case DensityType.DLT3c:
                                        density = Localization.DLT3c;

                                        break;
                                    default:
                                        density = string.Format(Localization.unknown_density_code_0,
                                                                (byte)descriptor.Density);

                                        break;
                                }
                            }

                                break;
                            case MediumTypes.Exatape80m:
                            {
                                switch(descriptor.Density)
                                {
                                    case DensityType.Ex8200:
                                        density = Localization.EXB8200;

                                        break;
                                    case DensityType.Ex8200c:
                                        density = Localization.EXB8200_compressed;

                                        break;
                                    case DensityType.Ex8500:
                                        density = Localization.EXB8500;

                                        break;
                                    case DensityType.Ex8500c:
                                        density = Localization.EXB8500_compressed;

                                        break;
                                    case DensityType.Mammoth:
                                        density = Localization.TapeName_Mammoth;

                                        break;
                                    case DensityType.DLT3_62k:
                                    case DensityType.DLT3_62kAlt:
                                        density = Localization.DLT3_XT;

                                        break;
                                    case DensityType.DLT3c:
                                        density = Localization.DLT3_XT_compressed;

                                        break;
                                    default:
                                        density = string.Format(Localization.unknown_density_code_0,
                                                                (byte)descriptor.Density);

                                        break;
                                }
                            }

                                break;
                            case MediumTypes.Exatape106m:
                            {
                                switch(descriptor.Density)
                                {
                                    case DensityType.Ex8200:
                                        density = Localization.EXB8200;

                                        break;
                                    case DensityType.Ex8200c:
                                        density = Localization.EXB8200_compressed;

                                        break;
                                    case DensityType.Ex8500:
                                        density = Localization.EXB8500;

                                        break;
                                    case DensityType.Ex8500c:
                                        density = Localization.EXB8500_compressed;

                                        break;
                                    case DensityType.Mammoth:
                                        density = Localization.TapeName_Mammoth;

                                        break;
                                    case DensityType.DLT4:
                                    case DensityType.DLT4Alt:
                                        density = Localization.DLT4;

                                        break;
                                    case DensityType.DLT4_123k:
                                    case DensityType.DLT4_123kAlt:
                                        density = Localization.DLT4_123k;

                                        break;
                                    case DensityType.DLT4_98k:
                                        density = Localization.DLT4_98k;

                                        break;
                                    case DensityType.Travan5:
                                        density = Localization.Travan5;

                                        break;
                                    case DensityType.DLT4c:
                                        density = Localization.DLT4c;

                                        break;
                                    case DensityType.DLT4_85k:
                                        density = Localization.DLT4_85k;

                                        break;
                                    case DensityType.DLT4c_85k:
                                        density = Localization.DLT4c_85k;

                                        break;
                                    case DensityType.DLT4c_123k:
                                        density = Localization.DLT4c_123k;

                                        break;
                                    case DensityType.DLT4c_98k:
                                        density = Localization.DLT4c_98k;

                                        break;
                                    default:
                                        density = string.Format(Localization.unknown_density_code_0,
                                                                (byte)descriptor.Density);

                                        break;
                                }
                            }

                                break;
                            case MediumTypes.Exatape106mXL:
                            {
                                switch(descriptor.Density)
                                {
                                    case DensityType.Ex8200:
                                        density = Localization.EXB8200;

                                        break;
                                    case DensityType.Ex8200c:
                                        density = Localization.EXB8200_compressed;

                                        break;
                                    case DensityType.Ex8500:
                                        density = Localization.EXB8500;

                                        break;
                                    case DensityType.Ex8500c:
                                        density = Localization.EXB8500_compressed;

                                        break;
                                    case DensityType.Mammoth:
                                        density = Localization.TapeName_Mammoth;

                                        break;
                                    case DensityType.SDLT1_133k:
                                    case DensityType.SDLT1_133kAlt:
                                        density = Localization.SDLT1_133k;

                                        break;
                                    case DensityType.SDLT1:
                                        //case DensityType.SDLT1Alt:
                                        density = Localization.SDLT1;

                                        break;
                                    case DensityType.SDLT1c:
                                        density = Localization.SDLT1c;

                                        break;
                                    /*case DensityType.SDLT1_133kAlt:
                                        density = "Super DLTtape I at 133000 bpi compressed";
                                        break;*/
                                    default:
                                        density = string.Format(Localization.unknown_density_code_0,
                                                                (byte)descriptor.Density);

                                        break;
                                }
                            }

                                break;
                            case MediumTypes.SDLT2:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.SDLT2 => Localization.MediumType_SDLT2,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.VStapeI:
                            {
                                switch(descriptor.Density)
                                {
                                    case DensityType.VStape1:
                                    case DensityType.VStape1Alt:
                                        density = Localization.MediumType_VStapeI;

                                        break;
                                    case DensityType.VStape1c:
                                        density = Localization.VStape1c;

                                        break;
                                    default:
                                        density = string.Format(Localization.unknown_density_code_0,
                                                                (byte)descriptor.Density);

                                        break;
                                }
                            }

                                break;
                            case MediumTypes.DLTtapeS4:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.DLTS4 => Localization.MediumType_DLTtapeS4,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.Exatape22m:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Ex8200  => Localization.EXB8200,
                                              DensityType.Ex8200c => Localization.EXB8200_compressed,
                                              DensityType.Ex8500  => Localization.EXB8500,
                                              DensityType.Ex8500c => Localization.EXB8500_compressed,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.Exatape40m:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Ex8200  => Localization.EXB8200,
                                              DensityType.Ex8200c => Localization.EXB8200_compressed,
                                              DensityType.Ex8500  => Localization.EXB8500,
                                              DensityType.Ex8500c => Localization.EXB8500_compressed,
                                              DensityType.Mammoth => Localization.TapeName_Mammoth,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.Exatape76m:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Ex8200  => Localization.EXB8200,
                                              DensityType.Ex8200c => Localization.EXB8200_compressed,
                                              DensityType.Ex8500  => Localization.EXB8500,
                                              DensityType.Ex8500c => Localization.EXB8500_compressed,
                                              DensityType.Mammoth => Localization.TapeName_Mammoth,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.Exatape112m:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Ex8200  => Localization.EXB8200,
                                              DensityType.Ex8200c => Localization.EXB8200_compressed,
                                              DensityType.Ex8500  => Localization.EXB8500,
                                              DensityType.Ex8500c => Localization.EXB8500_compressed,
                                              DensityType.Mammoth => Localization.TapeName_Mammoth,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
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
                                              DensityType.Mammoth  => Localization.TapeName_Mammoth,
                                              DensityType.Mammoth2 => Localization.Mammoth2,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.DC2900SL:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_DC2900SL,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.DC9250:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.DC9250,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLR32:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLR32,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.MLR1SL:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MLR1_26GBSL,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLRtape50:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLRtape50,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLRtape50SL:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLRtape50SL,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLR32SL:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.SLR32SL,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLR5:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLR5,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLR5SL:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.SLR5SL,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLRtape7:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLRtape7,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLRtape7SL:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLRtape7SL,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLRtape24:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLRtape24,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLRtape24SL:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLRtape24SL,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLRtape140:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLRtape140,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLRtape40:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLRtape40,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLRtape60:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLRtape60,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLRtape100:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.MediumType_SLRtape100,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            case MediumTypes.SLR40_60_100:
                            {
                                density = descriptor.Density switch
                                          {
                                              DensityType.Default => Localization.SLR40_60_100,
                                              _ => string.Format(Localization.unknown_density_code_0,
                                                                 (byte)descriptor.Density)
                                          };
                            }

                                break;
                            default:
                                density = string.Format(Localization.unknown_density_code_0, (byte)descriptor.Density);

                                break;
                        }

                        if(density != "")
                        {
                            if(descriptor.Blocks == 0)
                            {
                                if(descriptor.BlockLength == 0)
                                {
                                    sb.
                                        AppendFormat("\t" + Localization.All_remaining_blocks_conform_to_0_and_have_a_variable_length,
                                                     density).
                                        AppendLine();
                                }
                                else
                                {
                                    sb.
                                        AppendFormat("\t" + Localization.All_remaining_blocks_conform_to_0_and_are_1_bytes_each,
                                                     density, descriptor.BlockLength).
                                        AppendLine();
                                }
                            }
                            else if(descriptor.BlockLength == 0)
                            {
                                sb.AppendFormat("\t" + Localization._0_blocks_conform_to_1_and_have_a_variable_length,
                                                descriptor.Blocks, density).
                                   AppendLine();
                            }
                            else
                            {
                                sb.AppendFormat("\t" + Localization._0_blocks_conform_to_1_and_are_2_bytes_each,
                                                descriptor.Blocks, density, descriptor.BlockLength).
                                   AppendLine();
                            }
                        }
                        else if(descriptor.Blocks == 0)
                        {
                            if(descriptor.BlockLength == 0)
                            {
                                sb.AppendFormat("\t" + Localization.All_remaining_blocks_have_a_variable_length).
                                   AppendLine();
                            }
                            else
                            {
                                sb.AppendFormat("\t" + Localization.All_remaining_blocks_are_0_bytes_each,
                                                descriptor.BlockLength).
                                   AppendLine();
                            }
                        }
                        else if(descriptor.BlockLength == 0)
                        {
                            sb.AppendFormat("\t" + Localization._0_blocks_have_a_variable_length, descriptor.Blocks).
                               AppendLine();
                        }
                        else
                        {
                            sb.AppendFormat("\t" + Localization._0_blocks_are_1_bytes_each, descriptor.Blocks,
                                            descriptor.BlockLength).
                               AppendLine();
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
                        sb.AppendLine("\t" + Localization.Device_prints_directly);

                        break;
                    case 1:
                        sb.AppendLine("\t" + Localization.Device_uses_a_print_cache);

                        break;
                    default:
                        sb.AppendFormat("\t" + Localization.Unknown_buffered_mode_code_0, header.Value.BufferedMode).
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
                    sb.Append("\t" + Localization.Medium_is_);

                    switch(header.Value.MediumType)
                    {
                        case MediumTypes.ReadOnly:
                            sb.AppendLine(Localization.GetMediumTypeDescription_ReadOnly);

                            break;
                        case MediumTypes.WORM:
                            sb.AppendLine(Localization.GetMediumTypeDescription_WORM);

                            break;
                        case MediumTypes.Erasable:
                            sb.AppendLine(Localization.GetMediumTypeDescription_Erasable);

                            break;
                        case MediumTypes.RO_WORM:
                            sb.AppendLine(Localization.GetMediumTypeDescription_RO_WORM);

                            break;
                        case MediumTypes.RO_RW:
                            sb.AppendLine(Localization.a_combination_of_read_only_and_erasable_optical);

                            break;
                        case MediumTypes.WORM_RW:
                            sb.AppendLine(Localization.a_combination_of_write_once_and_erasable_optical);

                            break;
                        case MediumTypes.DOW:
                            sb.AppendLine(Localization.GetMediumTypeDescription_DOW);

                            break;
                        default:
                            sb.AppendFormat(Localization.an_unknown_medium_type_0, (byte)header.Value.MediumType).
                               AppendLine();

                            break;
                    }
                }

                if(header.Value.WriteProtected)
                    sb.AppendLine("\t" + Localization.Medium_is_write_protected);

                if(header.Value.EBC)
                    sb.AppendLine("\t" + Localization.Blank_checking_during_write_is_enabled);

                if(header.Value.DPOFUA)
                    sb.AppendLine("\t" + Localization.Drive_supports_DPO_and_FUA_bits);

                if(header.Value.BlockDescriptors != null)
                {
                    foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                    {
                        var density = "";

                        switch(descriptor.Density)
                        {
                            case DensityType.Default:
                                break;
                            case DensityType.ISO10090:
                                density = Localization.ISO10090;

                                break;
                            case DensityType.D581:
                                density = Localization.D581;

                                break;
                            case DensityType.X3_212:
                                density = Localization.X3_212;

                                break;
                            case DensityType.X3_191:
                                density = Localization.X3_191;

                                break;
                            case DensityType.X3_214:
                                density = Localization.X3_214;

                                break;
                            case DensityType.X3_211:
                                density = Localization.X3_211;

                                break;
                            case DensityType.D407:
                                density = Localization.D407;

                                break;
                            case DensityType.ISO13614:
                                density = Localization.ISO13614;

                                break;
                            case DensityType.X3_200:
                                density = Localization.X3_200;

                                break;
                            default:
                                density = string.Format(Localization.unknown_density_code_0, (byte)descriptor.Density);

                                break;
                        }

                        if(density != "")
                        {
                            if(descriptor.Blocks == 0)
                            {
                                if(descriptor.BlockLength == 0)
                                {
                                    sb.
                                        AppendFormat("\t" + Localization.All_remaining_blocks_are_0_and_have_a_variable_length,
                                                     density).
                                        AppendLine();
                                }
                                else
                                {
                                    sb.AppendFormat("\t" + Localization.All_remaining_blocks_are_0_and_are_1_bytes_each,
                                                    density, descriptor.BlockLength).
                                       AppendLine();
                                }
                            }
                            else if(descriptor.BlockLength == 0)
                            {
                                sb.AppendFormat("\t" + Localization._0_blocks_are_1_and_have_a_variable_length,
                                                descriptor.Blocks, density).
                                   AppendLine();
                            }
                            else
                            {
                                sb.AppendFormat("\t" + Localization._0_blocks_are_1_and_are_2_bytes_each,
                                                descriptor.Blocks, density, descriptor.BlockLength).
                                   AppendLine();
                            }
                        }
                        else if(descriptor.Blocks == 0)
                        {
                            if(descriptor.BlockLength == 0)
                            {
                                sb.AppendFormat("\t" + Localization.All_remaining_blocks_have_a_variable_length).
                                   AppendLine();
                            }
                            else
                            {
                                sb.AppendFormat("\t" + Localization.All_remaining_blocks_are_0_bytes_each,
                                                descriptor.BlockLength).
                                   AppendLine();
                            }
                        }
                        else if(descriptor.BlockLength == 0)
                        {
                            sb.AppendFormat("\t" + Localization._0_blocks_have_a_variable_length, descriptor.Blocks).
                               AppendLine();
                        }
                        else
                        {
                            sb.AppendFormat("\t" + Localization._0_blocks_are_1_bytes_each, descriptor.Blocks,
                                            descriptor.BlockLength).
                               AppendLine();
                        }
                    }
                }

                break;
            }

        #endregion Optical device mode header

        #region Multimedia device mode header

            case PeripheralDeviceTypes.MultiMediaDevice:
            {
                sb.Append("\t" + Localization.Medium_is_);

                switch(header.Value.MediumType)
                {
                    case MediumTypes.CDROM:
                        sb.AppendLine(Localization.MediumTypes_CDROM);

                        break;
                    case MediumTypes.CDDA:
                        sb.AppendLine(Localization.MediumTypes_CDDA);

                        break;
                    case MediumTypes.MixedCD:
                        sb.AppendLine(Localization.MediumTypes_MixedCD);

                        break;
                    case MediumTypes.CDROM_80:
                        sb.AppendLine(Localization.MediumTypes_CDROM_80);

                        break;
                    case MediumTypes.CDDA_80:
                        sb.AppendLine(Localization.MediumTypes_CDDA_80);

                        break;
                    case MediumTypes.MixedCD_80:
                        sb.AppendLine(Localization.MediumTypes_MixedCD_80);

                        break;
                    case MediumTypes.Unknown_CD:
                        sb.AppendLine(Localization.Unknown_medium_type);

                        break;
                    case MediumTypes.HybridCD:
                        sb.AppendLine(Localization.MediumTypes_HybridCD);

                        break;
                    case MediumTypes.Unknown_CDR:
                        sb.AppendLine(Localization.MediumTypes_Unknown_CDR);

                        break;
                    case MediumTypes.CDR:
                        sb.AppendLine(Localization.MediumTypes_CDR);

                        break;
                    case MediumTypes.CDR_DA:
                        sb.AppendLine(Localization.MediumTypes_CDR_DA);

                        break;
                    case MediumTypes.CDR_Mixed:
                        sb.AppendLine(Localization.MediumTypes_CDR_Mixed);

                        break;
                    case MediumTypes.HybridCDR:
                        sb.AppendLine(Localization.MediumTypes_HybridCDR);

                        break;
                    case MediumTypes.CDR_80:
                        sb.AppendLine(Localization.MediumTypes_CDR_80);

                        break;
                    case MediumTypes.CDR_DA_80:
                        sb.AppendLine(Localization.MediumTypes_CDR_DA_80);

                        break;
                    case MediumTypes.CDR_Mixed_80:
                        sb.AppendLine("80 mm CD-R with data and audio");

                        break;
                    case MediumTypes.HybridCDR_80:
                        sb.AppendLine(Localization.MediumTypes_HybridCDR_80);

                        break;
                    case MediumTypes.Unknown_CDRW:
                        sb.AppendLine(Localization.MediumTypes_Unknown_CDRW);

                        break;
                    case MediumTypes.CDRW:
                        sb.AppendLine(Localization.MediumTypes_CDRW);

                        break;
                    case MediumTypes.CDRW_DA:
                        sb.AppendLine(Localization.MediumTypes_CDRW_DA);

                        break;
                    case MediumTypes.CDRW_Mixed:
                        sb.AppendLine(Localization.MediumTypes_CDRW_Mixed);

                        break;
                    case MediumTypes.HybridCDRW:
                        sb.AppendLine(Localization.MediumTypes_HybridCDRW);

                        break;
                    case MediumTypes.CDRW_80:
                        sb.AppendLine(Localization.MediumTypes_CDRW_80);

                        break;
                    case MediumTypes.CDRW_DA_80:
                        sb.AppendLine(Localization.MediumTypes_CDRW_DA_80);

                        break;
                    case MediumTypes.CDRW_Mixed_80:
                        sb.AppendLine(Localization.MediumTypes_CDRW_Mixed_80);

                        break;
                    case MediumTypes.HybridCDRW_80:
                        sb.AppendLine(Localization.MediumTypes_HybridCDRW_80);

                        break;
                    case MediumTypes.Unknown_HD:
                        sb.AppendLine(Localization.MediumTypes_Unknown_HD);

                        break;
                    case MediumTypes.HD:
                        sb.AppendLine(Localization.MediumTypes_HD);

                        break;
                    case MediumTypes.HD_80:
                        sb.AppendLine(Localization.MediumTypes_HD_80);

                        break;
                    case MediumTypes.NoDisc:
                        sb.AppendLine(Localization.No_disc_inserted_tray_closed_or_caddy_inserted);

                        break;
                    case MediumTypes.TrayOpen:
                        sb.AppendLine(Localization.Tray_open_or_no_caddy_inserted);

                        break;
                    case MediumTypes.MediumError:
                        sb.AppendLine(Localization.Tray_closed_or_caddy_inserted_but_medium_error);

                        break;
                    case MediumTypes.UnknownBlockDevice:
                        sb.AppendLine(Localization.Unknown_block_device);

                        break;
                    case MediumTypes.ReadOnlyBlockDevice:
                        sb.AppendLine(Localization.Read_only_block_device);

                        break;
                    case MediumTypes.ReadWriteBlockDevice:
                        sb.AppendLine(Localization.Read_Write_block_device);

                        break;
                    case MediumTypes.LTOCD:
                        sb.AppendLine(Localization.LTO_in_CD_ROM_emulation_mode);

                        break;
                    default:
                        sb.AppendFormat(Localization.Unknown_medium_type_0, (byte)header.Value.MediumType).AppendLine();

                        break;
                }

                if(header.Value.WriteProtected)
                    sb.AppendLine("\t" + Localization.Medium_is_write_protected);

                if(header.Value.DPOFUA)
                    sb.AppendLine("\t" + Localization.Drive_supports_DPO_and_FUA_bits);

                if(header.Value.BlockDescriptors != null)
                {
                    foreach(BlockDescriptor descriptor in header.Value.BlockDescriptors)
                    {
                        var density = "";

                        switch(descriptor.Density)
                        {
                            case DensityType.Default:
                                break;
                            case DensityType.User:
                                density = Localization.user_data_only;

                                break;
                            case DensityType.UserAuxiliary:
                                density = Localization.user_data_plus_auxiliary_data;

                                break;
                            case DensityType.UserAuxiliaryTag:
                                density = Localization._4byte_tag_user_data_plus_auxiliary_data;

                                break;
                            case DensityType.Audio:
                                density = Localization.audio_information_only;

                                break;
                            case DensityType.LTO2:
                                density = Localization.MediumType_LTO2;

                                break;
                            case DensityType.LTO3:
                                density = Localization.MediumType_LTO3;

                                break;
                            case DensityType.LTO4:
                                density = Localization.MediumType_LTO4;

                                break;
                            case DensityType.LTO5:
                                density = Localization.MediumType_LTO5;

                                break;
                            default:
                                density = string.Format(Localization.with_unknown_density_code_0,
                                                        (byte)descriptor.Density);

                                break;
                        }

                        if(density != "")
                        {
                            if(descriptor.Blocks == 0)
                            {
                                sb.AppendFormat("\t" + Localization.All_remaining_blocks_have_0_and_are_1_bytes_each,
                                                density, descriptor.BlockLength).
                                   AppendLine();
                            }
                            else
                            {
                                sb.AppendFormat("\t" + Localization._0_blocks_have_1_and_are_2_bytes_each,
                                                descriptor.Blocks, density, descriptor.BlockLength).
                                   AppendLine();
                            }
                        }
                        else if(descriptor.Blocks == 0)
                        {
                            sb.AppendFormat("\t" + Localization.All_remaining_blocks_are_0_bytes_each,
                                            descriptor.BlockLength).
                               AppendLine();
                        }
                        else
                        {
                            sb.AppendFormat("\t" + Localization._0_blocks_are_1_bytes_each, descriptor.Blocks,
                                            descriptor.BlockLength).
                               AppendLine();
                        }
                    }
                }

                break;
            }

        #endregion Multimedia device mode header
        }

        return sb.ToString();
    }
}