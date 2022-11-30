// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SyQuest.cs
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

using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SCSI;

static class SyQuest
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_SyQuest_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._1_Send_READ_6_command);
            AaruConsole.WriteLine(Localization._2_Send_READ_10_command);
            AaruConsole.WriteLine(Localization._3_Send_READ_LONG_6_command);
            AaruConsole.WriteLine(Localization._4_Send_READ_LONG_10_command);
            AaruConsole.WriteLine(Localization._5_Send_READ_RESET_USAGE_COUNTER_command);
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
                    Read6(devPath, dev, false);

                    continue;
                case 2:
                    Read10(devPath, dev, false);

                    continue;
                case 3:
                    Read6(devPath, dev, true);

                    continue;
                case 4:
                    Read10(devPath, dev, true);

                    continue;
                case 5:
                    ReadResetUsageCounter(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void Read6(string devPath, Device dev, bool readlong)
    {
        uint   lba       = 0;
        uint   blockSize = 512;
        byte   count     = 1;
        bool   noDma     = false;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);

            AaruConsole.WriteLine(readlong ? Localization.Parameters_for_READ_LONG_6_command
                                      : Localization.Parameters_for_READ_6_command);

            AaruConsole.WriteLine(Localization.LBA_0, lba);
            AaruConsole.WriteLine(Localization._0_blocks_to_read, count == 0 ? 256 : count);
            AaruConsole.WriteLine(Localization._0_bytes_expected_per_block, blockSize);
            AaruConsole.WriteLine(Localization.Inhibit_DMA_0, noDma);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SyQuest_vendor_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SyQuest_vendor_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.LBA_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        lba = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(lba > 0x1FFFFF)
                    {
                        AaruConsole.WriteLine(Localization.Max_LBA_is_0_setting_to_0, 0x1FFFFF);
                        lba = 0x1FFFFF;
                    }

                    AaruConsole.Write(Localization.Blocks_to_read_zero_for_256_blocks);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        count = 1;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_bytes_to_expect_per_block_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out blockSize))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        blockSize = 512;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Inhibit_DMA_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out noDma))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        noDma = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.SyQuestRead6(out byte[] buffer, out byte[] senseBuffer, lba, blockSize, count, noDma, readlong,
                                      dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);

        AaruConsole.WriteLine(readlong ? Localization.Sending_READ_LONG_6_to_the_device
                                  : Localization.Sending_READ_6_to_the_device);

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
        AaruConsole.WriteLine(Localization.Return_to_SyQuest_vendor_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SyQuest_vendor_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(readlong ? Localization.READ_LONG_6_response : Localization.READ_6_response);

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
                AaruConsole.WriteLine(readlong ? Localization.READ_LONG_6_sense : Localization.READ_6_sense);

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

                AaruConsole.WriteLine(readlong ? Localization.READ_LONG_6_decoded_sense
                                          : Localization.READ_6_decoded_sense);

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

    static void Read10(string devPath, Device dev, bool readlong)
    {
        uint   lba       = 0;
        uint   blockSize = 512;
        byte   count     = 1;
        bool   noDma     = false;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);

            AaruConsole.WriteLine(readlong ? Localization.Parameters_for_READ_LONG_10_command
                                      : Localization.Parameters_for_READ_10_command);

            AaruConsole.WriteLine(Localization.LBA_0, lba);
            AaruConsole.WriteLine(Localization._0_blocks_to_read, count == 0 ? 256 : count);
            AaruConsole.WriteLine(Localization._0_bytes_expected_per_block, blockSize);
            AaruConsole.WriteLine(Localization.Inhibit_DMA_0, noDma);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SyQuest_vendor_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SyQuest_vendor_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.LBA_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        lba = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Blocks_to_read_zero_for_256_blocks);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        count = 1;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_bytes_to_expect_per_block_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out blockSize))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        blockSize = 512;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Inhibit_DMA_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out noDma))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        noDma = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.SyQuestRead10(out byte[] buffer, out byte[] senseBuffer, lba, blockSize, count, noDma,
                                       readlong, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);

        AaruConsole.WriteLine(readlong ? Localization.Sending_READ_LONG_10_to_the_device
                                  : Localization.Sending_READ_10_to_the_device);

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
        AaruConsole.WriteLine(Localization.Return_to_SyQuest_vendor_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SyQuest_vendor_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(readlong ? Localization.READ_LONG_10_response : Localization.READ_10_response);

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
                AaruConsole.WriteLine(readlong ? Localization.READ_LONG_10_sense : Localization.READ_10_sense);

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

                AaruConsole.WriteLine(readlong ? Localization.READ_LONG_10_decoded_sense
                                          : Localization.READ_10_decoded_sense);

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

    static void ReadResetUsageCounter(string devPath, Device dev)
    {
        start:
        System.Console.Clear();

        bool sense =
            dev.SyQuestReadUsageCounter(out byte[] buffer, out byte[] senseBuffer, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_RESET_USAGE_COUNTER_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SyQuest_vendor_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SyQuest_vendor_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_RESET_USAGE_COUNTER_response);

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
                AaruConsole.WriteLine(Localization.READ_RESET_USAGE_COUNTER_sense);

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
                AaruConsole.WriteLine(Localization.READ_RESET_USAGE_COUNTER_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 4: goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}