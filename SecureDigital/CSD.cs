// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CSD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SecureDigital CSD.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Decoders.SecureDigital;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class CSD
{
    public ushort Classes;
    public bool   Copy;
    public byte   CRC;
    public bool   DSRImplemented;
    public bool   EraseBlockEnable;
    public byte   EraseSectorSize;
    public byte   FileFormat;
    public bool   FileFormatGroup;
    public byte   NSAC;
    public bool   PermanentWriteProtect;
    public byte   ReadBlockLength;
    public byte   ReadCurrentAtVddMax;
    public byte   ReadCurrentAtVddMin;
    public bool   ReadMisalignment;
    public bool   ReadsPartialBlocks;
    public uint   Size;
    public byte   SizeMultiplier;
    public byte   Speed;
    public byte   Structure;
    public byte   TAAC;
    public bool   TemporaryWriteProtect;
    public byte   WriteBlockLength;
    public byte   WriteCurrentAtVddMax;
    public byte   WriteCurrentAtVddMin;
    public bool   WriteMisalignment;
    public bool   WriteProtectGroupEnable;
    public byte   WriteProtectGroupSize;
    public bool   WritesPartialBlocks;
    public byte   WriteSpeedFactor;
}

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Decoders
{
    public static CSD DecodeCSD(uint[] response)
    {
        if(response?.Length != 4)
            return null;

        byte[] data = new byte[16];

        byte[] tmp = BitConverter.GetBytes(response[0]);
        Array.Copy(tmp, 0, data, 0, 4);
        tmp = BitConverter.GetBytes(response[1]);
        Array.Copy(tmp, 0, data, 4, 4);
        tmp = BitConverter.GetBytes(response[2]);
        Array.Copy(tmp, 0, data, 8, 4);
        tmp = BitConverter.GetBytes(response[3]);
        Array.Copy(tmp, 0, data, 12, 4);

        return DecodeCSD(data);
    }

    public static CSD DecodeCSD(byte[] response)
    {
        if(response?.Length != 16)
            return null;

        var csd = new CSD
        {
            Structure               = (byte)((response[0] & 0xC0) >> 6),
            TAAC                    = response[1],
            NSAC                    = response[2],
            Speed                   = response[3],
            Classes                 = (ushort)((response[4] << 4) + ((response[5] & 0xF0) >> 4)),
            ReadBlockLength         = (byte)(response[5] & 0x0F),
            ReadsPartialBlocks      = (response[6]       & 0x80) == 0x80,
            WriteMisalignment       = (response[6]       & 0x40) == 0x40,
            ReadMisalignment        = (response[6]       & 0x20) == 0x20,
            DSRImplemented          = (response[6]       & 0x10) == 0x10,
            EraseBlockEnable        = (response[10]      & 0x40) == 0x40,
            EraseSectorSize         = (byte)(((response[10] & 0x3F) << 1) + ((response[11] & 0x80) >> 7)),
            WriteProtectGroupSize   = (byte)(response[11] & 0x7F),
            WriteProtectGroupEnable = (response[12]       & 0x80) == 0x80,
            WriteSpeedFactor        = (byte)((response[12] & 0x1C) >> 2),
            WriteBlockLength        = (byte)(((response[12] & 0x03) << 2) + ((response[13] & 0xC0) >> 6)),
            WritesPartialBlocks     = (response[13] & 0x20) == 0x20,
            FileFormatGroup         = (response[14] & 0x80) == 0x80,
            Copy                    = (response[14] & 0x40) == 0x40,
            PermanentWriteProtect   = (response[14] & 0x20) == 0x20,
            TemporaryWriteProtect   = (response[14] & 0x10) == 0x10,
            FileFormat              = (byte)((response[14] & 0x0C) >> 2),
            CRC                     = (byte)((response[15] & 0xFE) >> 1)
        };

        if(csd.Structure == 0)
        {
            csd.Size = (ushort)(((response[6] & 0x03) << 10) + (response[7] << 2) + ((response[8] & 0xC0) >> 6));

            csd.ReadCurrentAtVddMin  = (byte)((response[8] & 0x38) >> 3);
            csd.ReadCurrentAtVddMax  = (byte)(response[8] & 0x07);
            csd.WriteCurrentAtVddMin = (byte)((response[9] & 0xE0) >> 5);
            csd.WriteCurrentAtVddMax = (byte)((response[9] & 0x1C) >> 2);
            csd.SizeMultiplier       = (byte)(((response[9] & 0x03) << 1) + ((response[10] & 0x80) >> 7));
        }
        else
            csd.Size = (uint)(((response[7] & 0x3F) << 16) + (response[8] << 8) + response[9]);

        return csd;
    }

    public static string PrettifyCSD(CSD csd)
    {
        if(csd == null)
            return null;

        double unitFactor = 0;
        double multiplier = 0;
        string unit       = "";

        var sb = new StringBuilder();
        sb.AppendLine(Localization.SecureDigital_Device_Specific_Data_Register);

        switch(csd.Structure)
        {
            case 0:
                sb.AppendLine("\t" + Localization.Register_version_1_0);

                break;
            case 1:
                sb.AppendLine("\t" + Localization.Register_version_2_0);

                break;
        }

        switch(csd.TAAC & 0x07)
        {
            case 0:
                unit       = Localization.unit_ns;
                unitFactor = 1;

                break;
            case 1:
                unit       = Localization.unit_ns;
                unitFactor = 10;

                break;
            case 2:
                unit       = Localization.unit_ns;
                unitFactor = 100;

                break;
            case 3:
                unit       = Localization.unit_μs;
                unitFactor = 1;

                break;
            case 4:
                unit       = Localization.unit_μs;
                unitFactor = 10;

                break;
            case 5:
                unit       = Localization.unit_μs;
                unitFactor = 100;

                break;
            case 6:
                unit       = Localization.unit_ms;
                unitFactor = 1;

                break;
            case 7:
                unit       = Localization.unit_ms;
                unitFactor = 10;

                break;
        }

        multiplier = ((csd.TAAC & 0x78) >> 3) switch
        {
            0  => 0,
            1  => 1,
            2  => 1.2,
            3  => 1.3,
            4  => 1.5,
            5  => 2,
            6  => 2.5,
            7  => 3,
            8  => 3.5,
            9  => 4,
            10 => 4.5,
            11 => 5,
            12 => 5.5,
            13 => 6,
            14 => 7,
            15 => 8,
            _  => multiplier
        };

        double result = unitFactor * multiplier;
        sb.AppendFormat("\t" + Localization.Asynchronous_data_access_time_is_0_1, result, unit).AppendLine();

        sb.AppendFormat("\t" + Localization.Clock_dependent_part_of_data_access_is_0_clock_cycles, csd.NSAC * 100).
           AppendLine();

        unit = Localization.MBit_s;

        switch(csd.Speed & 0x07)
        {
            case 0:
                unitFactor = 0.1;

                break;
            case 1:
                unitFactor = 1;

                break;
            case 2:
                unitFactor = 10;

                break;
            case 3:
                unitFactor = 100;

                break;
            default:
                unit       = Localization.unit_unknown;
                unitFactor = 0;

                break;
        }

        multiplier = ((csd.Speed & 0x78) >> 3) switch
        {
            0  => 0,
            1  => 1,
            2  => 1.2,
            3  => 1.3,
            4  => 1.5,
            5  => 2,
            6  => 2.6,
            7  => 3,
            8  => 3.5,
            9  => 4,
            10 => 4.5,
            11 => 5.2,
            12 => 5.5,
            13 => 6,
            14 => 7,
            15 => 8,
            _  => multiplier
        };

        result = unitFactor * multiplier;
        sb.AppendFormat("\t" + Localization.Device_transfer_speed_0_1, result, unit).AppendLine();

        unit = "";

        for(int cl = 0, mask = 1; cl <= 11; cl++, mask <<= 1)
            if((csd.Classes & mask) == mask)
                unit += $" {cl}";

        sb.AppendFormat("\t" + Localization.Device_support_command_classes_0, unit).AppendLine();

        sb.AppendFormat("\t" + Localization.Read_block_length_is_0_bytes, Math.Pow(2, csd.ReadBlockLength)).
           AppendLine();

        if(csd.ReadsPartialBlocks)
            sb.AppendLine("\t" + Localization.Device_allows_reading_partial_blocks);

        if(csd.WriteMisalignment)
            sb.AppendLine("\t" + Localization.Write_commands_can_cross_physical_block_boundaries);

        if(csd.ReadMisalignment)
            sb.AppendLine("\t" + Localization.Read_commands_can_cross_physical_block_boundaries);

        if(csd.DSRImplemented)
            sb.AppendLine("\t" + Localization.Device_implements_configurable_driver_stage);

        if(csd.Structure == 0)
        {
            result = (csd.Size   + 1) * Math.Pow(2, csd.SizeMultiplier + 2);
            sb.AppendFormat("\t" + Localization.Device_has_0_blocks, (int)result).AppendLine();

            result = (csd.Size + 1) * Math.Pow(2, csd.SizeMultiplier + 2) * Math.Pow(2, csd.ReadBlockLength);

            switch(result)
            {
                case > 1073741824:
                    sb.AppendFormat("\t" + Localization.Device_has_0_GiB, result / 1073741824.0).AppendLine();

                    break;
                case > 1048576:
                    sb.AppendFormat("\t" + Localization.Device_has_0_MiB, result / 1048576.0).AppendLine();

                    break;
                case > 1024:
                    sb.AppendFormat("\t" + Localization.Device_has_0_KiB, result / 1024.0).AppendLine();

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Device_has_0_bytes, result).AppendLine();

                    break;
            }
        }
        else
        {
            sb.AppendFormat("\t"      + Localization.Device_has_0_blocks, (csd.Size + 1) * 1024).AppendLine();
            result = ((ulong)csd.Size + 1) * 1024 * 512;

            switch(result)
            {
                case > 1099511627776:
                    sb.AppendFormat("\t" + Localization.Device_has_0_TiB, result / 1099511627776.0).AppendLine();

                    break;
                case > 1073741824:
                    sb.AppendFormat("\t" + Localization.Device_has_0_GiB, result / 1073741824.0).AppendLine();

                    break;
                case > 1048576:
                    sb.AppendFormat("\t" + Localization.Device_has_0_MiB, result / 1048576.0).AppendLine();

                    break;
                case > 1024:
                    sb.AppendFormat("\t" + Localization.Device_has_0_KiB, result / 1024.0).AppendLine();

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Device_has_0_bytes, result).AppendLine();

                    break;
            }
        }

        if(csd.Structure == 0)
        {
            switch(csd.ReadCurrentAtVddMin & 0x07)
            {
                case 0:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_0_5mA_for_reading_at_minimum_voltage);

                    break;
                case 1:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_1mA_for_reading_at_minimum_voltage);

                    break;
                case 2:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_5mA_for_reading_at_minimum_voltage);

                    break;
                case 3:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_10mA_for_reading_at_minimum_voltage);

                    break;
                case 4:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_25mA_for_reading_at_minimum_voltage);

                    break;
                case 5:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_35mA_for_reading_at_minimum_voltage);

                    break;
                case 6:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_60mA_for_reading_at_minimum_voltage);

                    break;
                case 7:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_100mA_for_reading_at_minimum_voltage);

                    break;
            }

            switch(csd.ReadCurrentAtVddMax & 0x07)
            {
                case 0:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_1mA_for_reading_at_maximum_voltage);

                    break;
                case 1:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_5mA_for_reading_at_maximum_voltage);

                    break;
                case 2:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_10mA_for_reading_at_maximum_voltage);

                    break;
                case 3:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_25mA_for_reading_at_maximum_voltage);

                    break;
                case 4:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_35mA_for_reading_at_maximum_voltage);

                    break;
                case 5:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_45mA_for_reading_at_maximum_voltage);

                    break;
                case 6:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_80mA_for_reading_at_maximum_voltage);

                    break;
                case 7:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_200mA_for_reading_at_maximum_voltage);

                    break;
            }

            switch(csd.WriteCurrentAtVddMin & 0x07)
            {
                case 0:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_0_5mA_for_writing_at_minimum_voltage);

                    break;
                case 1:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_1mA_for_writing_at_minimum_voltage);

                    break;
                case 2:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_5mA_for_writing_at_minimum_voltage);

                    break;
                case 3:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_10mA_for_writing_at_minimum_voltage);

                    break;
                case 4:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_25mA_for_writing_at_minimum_voltage);

                    break;
                case 5:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_35mA_for_writing_at_minimum_voltage);

                    break;
                case 6:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_60mA_for_writing_at_minimum_voltage);

                    break;
                case 7:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_100mA_for_writing_at_minimum_voltage);

                    break;
            }

            switch(csd.WriteCurrentAtVddMax & 0x07)
            {
                case 0:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_1mA_for_writing_at_maximum_voltage);

                    break;
                case 1:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_5mA_for_writing_at_maximum_voltage);

                    break;
                case 2:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_10mA_for_writing_at_maximum_voltage);

                    break;
                case 3:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_25mA_for_writing_at_maximum_voltage);

                    break;
                case 4:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_35mA_for_writing_at_maximum_voltage);

                    break;
                case 5:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_45mA_for_writing_at_maximum_voltage);

                    break;
                case 6:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_80mA_for_writing_at_maximum_voltage);

                    break;
                case 7:
                    sb.AppendLine("\t" + Localization.Device_uses_a_maximum_of_200mA_for_writing_at_maximum_voltage);

                    break;
            }

            if(csd.EraseBlockEnable)
                sb.AppendLine("\t" + Localization.Device_can_erase_multiple_blocks);

            // TODO: Check specification
            sb.AppendFormat("\t" + Localization.Device_must_erase_a_minimum_of_0_blocks_at_a_time,
                            Convert.ToUInt32(csd.EraseSectorSize) + 1).AppendLine();

            if(csd.WriteProtectGroupEnable)
            {
                sb.AppendLine("\t" + Localization.Device_can_write_protect_regions);

                // TODO: Check specification
                // unitFactor = Convert.ToDouble(csd.WriteProtectGroupSize);

                sb.AppendFormat("\t" + Localization.Device_can_write_protect_a_minimum_of_0_blocks_at_a_time,
                                (int)(result + 1)).AppendLine();
            }
            else
                sb.AppendLine("\t" + Localization.Device_cant_write_protect_regions);
        }

        sb.AppendFormat("\t" + Localization.Writing_is_0_times_slower_than_reading, Math.Pow(2, csd.WriteSpeedFactor)).
           AppendLine();

        sb.AppendFormat("\t" + Localization.Write_block_length_is_0_bytes, Math.Pow(2, csd.WriteBlockLength)).
           AppendLine();

        if(csd.WritesPartialBlocks)
            sb.AppendLine("\t" + Localization.Device_allows_writing_partial_blocks);

        if(!csd.Copy)
            sb.AppendLine("\t" + Localization.Device_contents_are_original);

        if(csd.PermanentWriteProtect)
            sb.AppendLine("\t" + Localization.Device_is_permanently_write_protected);

        if(csd.TemporaryWriteProtect)
            sb.AppendLine("\t" + Localization.Device_is_temporarily_write_protected);

        if(!csd.FileFormatGroup)
            switch(csd.FileFormat)
            {
                case 0:
                    sb.AppendLine("\t" + Localization.Device_is_formatted_like_a_hard_disk);

                    break;
                case 1:
                    sb.AppendLine("\t" + Localization.Device_is_formatted_like_a_floppy_disk_using_Microsoft_FAT);

                    break;
                case 2:
                    sb.AppendLine("\t" + Localization.Device_uses_Universal_File_Format);

                    break;
                default:
                    sb.AppendFormat("\t" + Localization.Device_uses_unknown_file_format_code_0, csd.FileFormat).
                       AppendLine();

                    break;
            }
        else
            sb.AppendFormat("\t" + Localization.Device_uses_unknown_file_format_code_0_and_file_format_group_1,
                            csd.FileFormat).AppendLine();

        sb.AppendFormat("\t" + Localization.CSD_CRC_0, csd.CRC).AppendLine();

        return sb.ToString();
    }

    public static string PrettifyCSD(uint[] response) => PrettifyCSD(DecodeCSD(response));

    public static string PrettifyCSD(byte[] response) => PrettifyCSD(DecodeCSD(response));
}