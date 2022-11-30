// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Fujitsu.cs
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SCSI;

static class Fujitsu
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_Fujitsu_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_DISPLAY_command);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_commands_menu);
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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_commands_menu);

                    return;
                case 1:
                    Display(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void Display(string devPath, Device dev)
    {
        bool                flash      = false;
        FujitsuDisplayModes mode       = FujitsuDisplayModes.Ready;
        string              firstHalf  = "AARUTEST";
        string              secondHalf = "TESTAARU";
        string              strDev;
        int                 item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_DISPLAY_command);
            AaruConsole.WriteLine(Localization.Descriptor_0, flash);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_Fujitsu_vendor_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_Fujitsu_vendor_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write("Flash?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out flash))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        flash = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine(Localization.Display_mode);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3_4, FujitsuDisplayModes.Cancel,
                                          FujitsuDisplayModes.Cart, FujitsuDisplayModes.Half, FujitsuDisplayModes.Idle,
                                          FujitsuDisplayModes.Ready);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out mode))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_display_mode_Press_any_key_to_continue);
                        mode = FujitsuDisplayModes.Ready;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.First_display_half_will_be_cut_to_7_bit_ASCII_8_chars);
                    firstHalf = System.Console.ReadLine();
                    AaruConsole.Write(Localization.Second_display_half_will_be_cut_to_7_bit_ASCII_8_chars);
                    secondHalf = System.Console.ReadLine();

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.FujitsuDisplay(out byte[] senseBuffer, flash, mode, firstHalf, secondHalf, dev.Timeout,
                                        out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_DISPLAY_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.DISPLAY_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_Fujitsu_vendor_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_Fujitsu_vendor_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.DISPLAY_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2: goto start;
            case 3: goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}