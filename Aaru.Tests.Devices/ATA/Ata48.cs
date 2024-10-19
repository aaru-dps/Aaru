// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Ata48.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru device testing.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;
using Aaru.Decoders.ATA;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.ATA;

static class Ata48
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_48_bit_ATA_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_GET_NATIVE_MAX_ADDRESS_EXT_command);
            AaruConsole.WriteLine(Localization.Send_READ_DMA_EXT_command);
            AaruConsole.WriteLine(Localization.Send_READ_LOG_DMA_EXT_command);
            AaruConsole.WriteLine(Localization.Send_READ_LOG_EXT_command);
            AaruConsole.WriteLine(Localization.Send_READ_NATIVE_MAX_ADDRESS_EXT_command);
            AaruConsole.WriteLine(Localization.Send_READ_MULTIPLE_EXT_command);
            AaruConsole.WriteLine(Localization.Send_READ_SECTORS_EXT_command);
            AaruConsole.WriteLine(Localization.Return_to_ATA_commands_menu);
            AaruConsole.Write(Localization.Choose);

            string strDev = System.Console.ReadLine();

            if(!int.TryParse(strDev, out int item))
            {
                AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                System.Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine(Localization.Returning_to_ATA_commands_menu);

                    return;
                case 1:
                    GetNativeMaxAddressExt(devPath, dev);

                    continue;
                case 2:
                    ReadDmaExt(devPath, dev);

                    continue;
                case 3:
                    ReadLogDmaExt(devPath, dev);

                    continue;
                case 4:
                    ReadLogExt(devPath, dev);

                    continue;
                case 5:
                    ReadNativeMaxAddressExt(devPath, dev);

                    continue;
                case 6:
                    ReadMultipleExt(devPath, dev);

                    continue;
                case 7:
                    ReadSectorsExt(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void GetNativeMaxAddressExt(string devPath, Device dev)
    {
    start:
        System.Console.Clear();

        bool sense = dev.GetNativeMaxAddressExt(out ulong lba,
                                                out AtaErrorRegistersLba48 errorRegisters,
                                                dev.Timeout,
                                                out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_GET_NATIVE_MAX_ADDRESS_EXT_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);
        AaruConsole.WriteLine(Localization.Max_LBA_is_0,      lba);
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Decode_error_registers);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);
        AaruConsole.Write(Localization.Choose);

        string strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.GET_NATIVE_MAX_ADDRESS_EXT_status_registers);
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadDmaExt(string devPath, Device dev)
    {
        ulong  lba   = 0;
        ushort count = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_DMA_EXT_command);
            AaruConsole.WriteLine(Localization.LBA_0,   lba);
            AaruConsole.WriteLine(Localization.Count_0, count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);

            strDev = System.Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                System.Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_logical_block_address);
                    strDev = System.Console.ReadLine();

                    if(!ulong.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        lba = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(lba > 0xFFFFFFFFFFFF)
                    {
                        AaruConsole.WriteLine(Localization
                                                 .Logical_block_address_cannot_be_bigger_than_0_Setting_it_to_0,
                                              0xFFFFFFFFFFFF);

                        lba = 0xFFFFFFFFFFFF;
                    }

                    AaruConsole.Write(Localization.How_many_sectors);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        count = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.ReadDma(out byte[] buffer,
                                 out AtaErrorRegistersLba48 errorRegisters,
                                 lba,
                                 count,
                                 dev.Timeout,
                                 out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_DMA_EXT_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);
        AaruConsole.WriteLine(Localization.Buffer_is_0_bytes, buffer?.Length.ToString() ?? Localization._null);
        AaruConsole.WriteLine(Localization.Buffer_is_null_or_empty_0_Q, ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization.Print_buffer);
        AaruConsole.WriteLine(Localization.Decode_error_registers);
        AaruConsole.WriteLine(Localization.Send_command_again);
        AaruConsole.WriteLine(Localization._4_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);
        AaruConsole.Write(Localization.Choose);

        strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_DMA_EXT_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_DMA_EXT_status_registers);
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                goto start;
            case 4:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadLogExt(string devPath, Device dev)
    {
        byte   address = 0;
        ushort page    = 0;
        ushort count   = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_LOG_EXT_command);
            AaruConsole.WriteLine(Localization.Log_address_0, address);
            AaruConsole.WriteLine(Localization.Page_number_0, page);
            AaruConsole.WriteLine(Localization.Count_0,       count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);

            strDev = System.Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                System.Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_log_address);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        address = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.What_page_number);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out page))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        page = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_sectors);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        count = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.ReadLog(out byte[] buffer,
                                 out AtaErrorRegistersLba48 errorRegisters,
                                 address,
                                 page,
                                 count,
                                 dev.Timeout,
                                 out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_LOG_EXT_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);
        AaruConsole.WriteLine(Localization.Buffer_is_0_bytes, buffer?.Length.ToString() ?? Localization._null);
        AaruConsole.WriteLine(Localization.Buffer_is_null_or_empty_0_Q, ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization.Print_buffer);
        AaruConsole.WriteLine(Localization.Decode_error_registers);
        AaruConsole.WriteLine(Localization.Send_command_again);
        AaruConsole.WriteLine(Localization._4_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);
        AaruConsole.Write(Localization.Choose);

        strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_LOG_EXT_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_LOG_EXT_status_registers);
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                goto start;
            case 4:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadLogDmaExt(string devPath, Device dev)
    {
        byte   address = 0;
        ushort page    = 0;
        ushort count   = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_LOG_DMA_EXT_command);
            AaruConsole.WriteLine(Localization.Log_address_0, address);
            AaruConsole.WriteLine(Localization.Page_number_0, page);
            AaruConsole.WriteLine(Localization.Count_0,       count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);

            strDev = System.Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                System.Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_log_address);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        address = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.What_page_number);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out page))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        page = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_sectors);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        count = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.ReadLogDma(out byte[] buffer,
                                    out AtaErrorRegistersLba48 errorRegisters,
                                    address,
                                    page,
                                    count,
                                    dev.Timeout,
                                    out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_LOG_DMA_EXT_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);
        AaruConsole.WriteLine(Localization.Buffer_is_0_bytes, buffer?.Length.ToString() ?? Localization._null);
        AaruConsole.WriteLine(Localization.Buffer_is_null_or_empty_0_Q, ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization.Print_buffer);
        AaruConsole.WriteLine(Localization.Decode_error_registers);
        AaruConsole.WriteLine(Localization.Send_command_again);
        AaruConsole.WriteLine(Localization._4_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);
        AaruConsole.Write(Localization.Choose);

        strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_LOG_DMA_EXT_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_LOG_DMA_EXT_status_registers);
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                goto start;
            case 4:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadMultipleExt(string devPath, Device dev)
    {
        ulong  lba   = 0;
        ushort count = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_MULTIPLE_EXT_command);
            AaruConsole.WriteLine(Localization.LBA_0,   lba);
            AaruConsole.WriteLine(Localization.Count_0, count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);

            strDev = System.Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                System.Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_logical_block_address);
                    strDev = System.Console.ReadLine();

                    if(!ulong.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        lba = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(lba > 0xFFFFFFFFFFFF)
                    {
                        AaruConsole.WriteLine(Localization
                                                 .Logical_block_address_cannot_be_bigger_than_0_Setting_it_to_0,
                                              0xFFFFFFFFFFFF);

                        lba = 0xFFFFFFFFFFFF;
                    }

                    AaruConsole.Write(Localization.How_many_sectors);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        count = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.ReadMultiple(out byte[] buffer,
                                      out AtaErrorRegistersLba48 errorRegisters,
                                      lba,
                                      count,
                                      dev.Timeout,
                                      out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_MULTIPLE_EXT_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);
        AaruConsole.WriteLine(Localization.Buffer_is_0_bytes, buffer?.Length.ToString() ?? Localization._null);
        AaruConsole.WriteLine(Localization.Buffer_is_null_or_empty_0_Q, ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization.Print_buffer);
        AaruConsole.WriteLine(Localization.Decode_error_registers);
        AaruConsole.WriteLine(Localization.Send_command_again);
        AaruConsole.WriteLine(Localization._4_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);
        AaruConsole.Write(Localization.Choose);

        strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_MULTIPLE_EXT_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_MULTIPLE_EXT_status_registers);
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                goto start;
            case 4:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadNativeMaxAddressExt(string devPath, Device dev)
    {
    start:
        System.Console.Clear();

        bool sense = dev.ReadNativeMaxAddress(out ulong lba,
                                              out AtaErrorRegistersLba48 errorRegisters,
                                              dev.Timeout,
                                              out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_NATIVE_MAX_ADDRESS_EXT_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);
        AaruConsole.WriteLine(Localization.Max_LBA_is_0,      lba);
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Decode_error_registers);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);
        AaruConsole.Write(Localization.Choose);

        string strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_NATIVE_MAX_ADDRESS_status_registers);
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadSectorsExt(string devPath, Device dev)
    {
        ulong  lba   = 0;
        ushort count = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_SECTORS_EXT_command);
            AaruConsole.WriteLine(Localization.LBA_0,   lba);
            AaruConsole.WriteLine(Localization.Count_0, count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);

            strDev = System.Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                System.Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_logical_block_address);
                    strDev = System.Console.ReadLine();

                    if(!ulong.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        lba = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(lba > 0xFFFFFFFFFFFF)
                    {
                        AaruConsole.WriteLine(Localization
                                                 .Logical_block_address_cannot_be_bigger_than_0_Setting_it_to_0,
                                              0xFFFFFFFFFFFF);

                        lba = 0xFFFFFFFFFFFF;
                    }

                    AaruConsole.Write(Localization.How_many_sectors);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        count = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.Read(out byte[] buffer,
                              out AtaErrorRegistersLba48 errorRegisters,
                              lba,
                              count,
                              dev.Timeout,
                              out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_SECTORS_EXT_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);
        AaruConsole.WriteLine(Localization.Buffer_is_0_bytes, buffer?.Length.ToString() ?? Localization._null);
        AaruConsole.WriteLine(Localization.Buffer_is_null_or_empty_0_Q, ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization.Print_buffer);
        AaruConsole.WriteLine(Localization.Decode_error_registers);
        AaruConsole.WriteLine(Localization.Send_command_again);
        AaruConsole.WriteLine(Localization._4_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_48_bit_ATA_commands_menu);
        AaruConsole.Write(Localization.Choose);

        strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine(Localization.Returning_to_48_bit_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_SECTORS_EXT_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_SECTORS_EXT_status_registers);
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                goto start;
            case 4:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}