// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SMC.cs
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

static class Smc
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_SCSI_Media_Changer_command_to_the_device);
            AaruConsole.WriteLine(Localization._1_Send_READ_ATTRIBUTE_command);
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
                    ReadAttribute(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void ReadAttribute(string devPath, Device dev)
    {
        ushort              element        = 0;
        byte                elementType    = 0;
        byte                volume         = 0;
        byte                partition      = 0;
        ushort              firstAttribute = 0;
        bool                cache          = false;
        ScsiAttributeAction action         = ScsiAttributeAction.Values;
        string              strDev;
        int                 item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_ATTRIBUTE_command);
            AaruConsole.WriteLine(Localization.Action_0, action);
            AaruConsole.WriteLine(Localization.Element_0, element);
            AaruConsole.WriteLine(Localization.Element_type_0, elementType);
            AaruConsole.WriteLine(Localization.Volume_0, volume);
            AaruConsole.WriteLine(Localization.Partition_0, partition);
            AaruConsole.WriteLine(Localization.First_attribute_0, firstAttribute);
            AaruConsole.WriteLine(Localization.Use_cache_0, cache);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Media_Changer_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Media_Changer_commands_menu);

                    return;
                case 1:
                    AaruConsole.WriteLine(Localization.Attribute_action);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3_4, ScsiAttributeAction.Values,
                                          ScsiAttributeAction.List, ScsiAttributeAction.VolumeList,
                                          ScsiAttributeAction.PartitionList, ScsiAttributeAction.ElementList,
                                          ScsiAttributeAction.Supported);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out action))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_valid_attribute_action_Press_any_key_to_continue);
                        action = ScsiAttributeAction.Values;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Element_Q);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out element))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        element = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Element_type_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out elementType))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        elementType = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Volume_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out volume))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        volume = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Partition_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out partition))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        partition = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.First_attribute_Q);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out firstAttribute))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        firstAttribute = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Use_cache_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out cache))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        cache = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ReadAttribute(out byte[] buffer, out byte[] senseBuffer, action, element, elementType, volume,
                                       partition, firstAttribute, cache, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_ATTRIBUTE_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);
        AaruConsole.WriteLine(Localization.Buffer_is_0_bytes, buffer?.Length.ToString() ?? Localization._null);
        AaruConsole.WriteLine(Localization.Buffer_is_null_or_empty_0_Q, ArrayHelpers.ArrayIsNullOrEmpty(buffer));

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization.Print_buffer);
        AaruConsole.WriteLine(Localization._2_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._3_Decode_sense_buffer);
        AaruConsole.WriteLine(Localization._4_Send_command_again);
        AaruConsole.WriteLine(Localization._5_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Media_Changer_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Media_Changer_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_ATTRIBUTE_response);

                if(buffer != null)
                    PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_ATTRIBUTE_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_ATTRIBUTE_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 4: goto start;
            case 5: goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}