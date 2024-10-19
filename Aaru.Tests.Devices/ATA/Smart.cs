// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Smart.cs
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

static class Smart
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_SMART_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_DISABLE_ATTRIBUTE_AUTOSAVE_command);
            AaruConsole.WriteLine(Localization.Send_DISABLE_OPERATIONS_command);
            AaruConsole.WriteLine(Localization.Send_ENABLE_ATTRIBUTE_AUTOSAVE_command);
            AaruConsole.WriteLine(Localization.Send_ENABLE_OPERATIONS_command);
            AaruConsole.WriteLine(Localization.Send_EXECUTE_OFF_LINE_IMMEDIATE_command);
            AaruConsole.WriteLine(Localization.Send_READ_DATA_command);
            AaruConsole.WriteLine(Localization.Send_READ_LOG_command);
            AaruConsole.WriteLine(Localization.Send_RETURN_STATUS_command);
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
                    DisableAttributeAutosave(devPath, dev);

                    continue;
                case 2:
                    DisableOperations(devPath, dev);

                    continue;
                case 3:
                    EnableAttributeAutosave(devPath, dev);

                    continue;
                case 4:
                    EnableOperations(devPath, dev);

                    continue;
                case 5:
                    ExecuteOfflineImmediate(devPath, dev);

                    continue;
                case 6:
                    ReadData(devPath, dev);

                    continue;
                case 7:
                    ReadLog(devPath, dev);

                    continue;
                case 8:
                    ReturnStatus(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void DisableAttributeAutosave(string devPath, Device dev)
    {
    start:
        System.Console.Clear();

        bool sense =
            dev.SmartDisableAttributeAutosave(out AtaErrorRegistersLba28 errorRegisters,
                                              dev.Timeout,
                                              out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_DISABLE_ATTRIBUTE_AUTOSAVE_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);
        AaruConsole.WriteLine(Localization.DISABLE_ATTRIBUTE_AUTOSAVE_status_registers);
        AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_SMART_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SMART_commands_menu);

                return;
            case 1:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void DisableOperations(string devPath, Device dev)
    {
    start:
        System.Console.Clear();
        bool sense = dev.SmartDisable(out AtaErrorRegistersLba28 errorRegisters, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_DISABLE_OPERATIONS_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);
        AaruConsole.WriteLine(Localization.DISABLE_OPERATIONS_status_registers);
        AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_SMART_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SMART_commands_menu);

                return;
            case 1:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void EnableAttributeAutosave(string devPath, Device dev)
    {
    start:
        System.Console.Clear();

        bool sense =
            dev.SmartEnableAttributeAutosave(out AtaErrorRegistersLba28 errorRegisters,
                                             dev.Timeout,
                                             out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_ENABLE_ATTRIBUTE_AUTOSAVE_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);
        AaruConsole.WriteLine(Localization.ENABLE_ATTRIBUTE_AUTOSAVE_status_registers);
        AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_SMART_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SMART_commands_menu);

                return;
            case 1:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void EnableOperations(string devPath, Device dev)
    {
    start:
        System.Console.Clear();
        bool sense = dev.SmartEnable(out AtaErrorRegistersLba28 errorRegisters, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_ENABLE_OPERATIONS_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);
        AaruConsole.WriteLine(Localization.ENABLE_OPERATIONS_status_registers);
        AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_SMART_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SMART_commands_menu);

                return;
            case 1:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ExecuteOfflineImmediate(string devPath, Device dev)
    {
        byte   subcommand = 0;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_EXECUTE_OFF_LINE_IMMEDIATE_command);
            AaruConsole.WriteLine(Localization.Subcommand_0, subcommand);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SMART_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SMART_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Subcommand);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out subcommand))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        subcommand = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.SmartExecuteOffLineImmediate(out AtaErrorRegistersLba28 errorRegisters,
                                                      subcommand,
                                                      dev.Timeout,
                                                      out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_EXECUTE_OFF_LINE_IMMEDIATE_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);
        AaruConsole.WriteLine(Localization.EXECUTE_OFF_LINE_IMMEDIATE_status_registers);
        AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Send_command_again);
        AaruConsole.WriteLine(Localization._2_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SMART_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SMART_commands_menu);

                return;
            case 1:
                goto start;
            case 2:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadData(string devPath, Device dev)
    {
    start:
        System.Console.Clear();

        bool sense = dev.SmartReadData(out byte[] buffer,
                                       out AtaErrorRegistersLba28 errorRegisters,
                                       dev.Timeout,
                                       out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_DATA_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);
        AaruConsole.WriteLine(Localization.Buffer_is_0_bytes, buffer?.Length.ToString() ?? Localization._null);
        AaruConsole.WriteLine(Localization.Buffer_is_null_or_empty_0_Q, ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization.Print_buffer);
        AaruConsole.WriteLine(Localization.Decode_error_registers);
        AaruConsole.WriteLine(Localization.Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_SMART_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SMART_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_DATA_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_DATA_status_registers);
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 4:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadLog(string devPath, Device dev)
    {
        byte   address = 0;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_LOG_command);
            AaruConsole.WriteLine(Localization.Log_address_0, address);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SMART_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SMART_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Log_address);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        address = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.SmartReadLog(out byte[] buffer,
                                      out AtaErrorRegistersLba28 errorRegisters,
                                      address,
                                      dev.Timeout,
                                      out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_LOG_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);
        AaruConsole.WriteLine(Localization.Buffer_is_0_bytes, buffer?.Length.ToString() ?? Localization._null);
        AaruConsole.WriteLine(Localization.Buffer_is_null_or_empty_0_Q, ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization.Print_buffer);
        AaruConsole.WriteLine(Localization.Decode_error_registers);
        AaruConsole.WriteLine(Localization.Send_command_again);
        AaruConsole.WriteLine(Localization._1_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SMART_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SMART_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_LOG_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_LOG_status_registers);
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

    static void ReturnStatus(string devPath, Device dev)
    {
    start:
        System.Console.Clear();

        bool sense = dev.SmartReturnStatus(out AtaErrorRegistersLba28 errorRegisters, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_RETURN_STATUS_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);
        AaruConsole.WriteLine(Localization.RETURN_STATUS_status_registers);
        AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_SMART_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SMART_commands_menu);

                return;
            case 1:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}