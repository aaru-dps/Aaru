// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Certance.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SCSI;

static class Certance
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_Certance_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_PARK_command);
            AaruConsole.WriteLine(Localization.Send_UNPARK_command);
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
                    Park(devPath, dev);

                    continue;
                case 2:
                    Unpark(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void Park(string devPath, Device dev)
    {
    start:
        System.Console.Clear();
        bool sense = dev.CertancePark(out byte[] senseBuffer, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_PARK_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.PARK_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_Certance_vendor_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_Certance_vendor_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.PARK_sense);

                if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);

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

    static void Unpark(string devPath, Device dev)
    {
    start:
        System.Console.Clear();
        bool sense = dev.CertanceUnpark(out byte[] senseBuffer, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_UNPARK_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.UNPARK_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_Certance_vendor_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_Certance_vendor_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.UNPARK_sense);

                if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);

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
}