// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AtaCHS.cs
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
using Aaru.Decoders.ATA;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.ATA;

static class AtaChs
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_CHS_ATA_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_IDENTIFY_DEVICE_command);
            AaruConsole.WriteLine(Localization._2_Send_READ_DMA_command);
            AaruConsole.WriteLine(Localization._3_Send_READ_DMA_WITH_RETRIES_command);
            AaruConsole.WriteLine(Localization._4_Send_READ_LONG_command);
            AaruConsole.WriteLine(Localization._5_Send_READ_LONG_WITH_RETRIES_command);
            AaruConsole.WriteLine(Localization._6_Send_READ_MULTIPLE_command);
            AaruConsole.WriteLine(Localization._7_Send_READ_SECTORS_command);
            AaruConsole.WriteLine(Localization._8_Send_READ_SECTORS_WITH_RETRIES_command);
            AaruConsole.WriteLine(Localization._9_Send_SEEK_command);
            AaruConsole.WriteLine(Localization.Send_SET_FEATURES_command);
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
                    Identify(devPath, dev);

                    continue;
                case 2:
                    ReadDma(devPath, dev, false);

                    continue;
                case 3:
                    ReadDma(devPath, dev, true);

                    continue;
                case 4:
                    ReadLong(devPath, dev, false);

                    continue;
                case 5:
                    ReadLong(devPath, dev, true);

                    continue;
                case 6:
                    ReadMultiple(devPath, dev);

                    continue;
                case 7:
                    ReadSectors(devPath, dev, false);

                    continue;
                case 8:
                    ReadSectors(devPath, dev, true);

                    continue;
                case 9:
                    Seek(devPath, dev);

                    continue;
                case 10:
                    SetFeatures(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void Identify(string devPath, Device dev)
    {
    start:
        System.Console.Clear();

        bool sense = dev.AtaIdentify(out byte[] buffer, out AtaErrorRegistersChs errorRegisters, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_IDENTIFY_DEVICE_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);
        AaruConsole.WriteLine(Localization.Buffer_is_0_bytes, buffer?.Length.ToString() ?? Localization._null);
        AaruConsole.WriteLine(Localization.Buffer_is_null_or_empty_0_Q, ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization.Print_buffer);
        AaruConsole.WriteLine(Localization._2_Decode_buffer);
        AaruConsole.WriteLine(Localization._3_Decode_error_registers);
        AaruConsole.WriteLine(Localization._4_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.IDENTIFY_DEVICE_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.IDENTIFY_DEVICE_decoded_response);

                if(buffer != null) AaruConsole.WriteLine("{0}", Decoders.ATA.Identify.Prettify(buffer));

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.IDENTIFY_DEVICE_status_registers);
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

    static void ReadDma(string devPath, Device dev, bool retries)
    {
        ushort cylinder = 0;
        byte   head     = 0;
        byte   sector   = 1;
        byte   count    = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);

            AaruConsole.WriteLine(retries
                                      ? Localization.Parameters_for_READ_DMA_WITH_RETRIES_command
                                      : Localization.Parameters_for_READ_DMA_command);

            AaruConsole.WriteLine(Localization.Cylinder_0, cylinder);
            AaruConsole.WriteLine(Localization.Head_0,     head);
            AaruConsole.WriteLine(Localization.Sector_0,   sector);
            AaruConsole.WriteLine(Localization.Count_0,    count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_cylinder);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out cylinder))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        cylinder = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.What_head);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out head))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        head = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(head > 15)
                    {
                        AaruConsole.WriteLine(Localization.Head_cannot_be_bigger_than_15_Setting_it_to_15);
                        head = 15;
                    }

                    AaruConsole.Write(Localization.What_sector);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out sector))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        sector = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_sectors);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out count))
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
                                 out AtaErrorRegistersChs errorRegisters,
                                 retries,
                                 cylinder,
                                 head,
                                 sector,
                                 count,
                                 dev.Timeout,
                                 out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);

        AaruConsole.WriteLine(retries
                                  ? Localization.Sending_READ_DMA_WITH_RETRIES_to_the_device
                                  : Localization.Sending_READ_DMA_to_the_device);

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
        AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                AaruConsole.WriteLine(retries
                                          ? Localization.READ_DMA_WITH_RETRIES_response
                                          : Localization.READ_DMA_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                AaruConsole.WriteLine(retries
                                          ? Localization.READ_DMA_WITH_RETRIES_status_registers
                                          : Localization.READ_DMA_status_registers);

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

    static void ReadLong(string devPath, Device dev, bool retries)
    {
        ushort cylinder  = 0;
        byte   head      = 0;
        byte   sector    = 1;
        uint   blockSize = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);

            AaruConsole.WriteLine(retries
                                      ? Localization.Parameters_for_READ_LONG_WITH_RETRIES_command
                                      : Localization.Parameters_for_READ_LONG_command);

            AaruConsole.WriteLine(Localization.Cylinder_0,   cylinder);
            AaruConsole.WriteLine(Localization.Head_0,       head);
            AaruConsole.WriteLine(Localization.Sector_0,     sector);
            AaruConsole.WriteLine(Localization.Block_size_0, blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_cylinder);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out cylinder))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        cylinder = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.What_head);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out head))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        head = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(head > 15)
                    {
                        AaruConsole.WriteLine(Localization.Head_cannot_be_bigger_than_15_Setting_it_to_15);
                        head = 15;
                    }

                    AaruConsole.Write(Localization.What_sector);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out sector))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        sector = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_bytes_to_expect);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out blockSize))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        blockSize = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.ReadLong(out byte[] buffer,
                                  out AtaErrorRegistersChs errorRegisters,
                                  retries,
                                  cylinder,
                                  head,
                                  sector,
                                  blockSize,
                                  dev.Timeout,
                                  out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);

        AaruConsole.WriteLine(retries
                                  ? Localization.Sending_READ_LONG_WITH_RETRIES_to_the_device
                                  : Localization.Sending_READ_LONG_to_the_device);

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
        AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                AaruConsole.WriteLine(retries
                                          ? Localization.READ_LONG_WITH_RETRIES_response
                                          : Localization.READ_LONG_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                AaruConsole.WriteLine(retries
                                          ? Localization.READ_LONG_WITH_RETRIES_status_registers
                                          : Localization.READ_LONG_status_registers);

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

    static void ReadMultiple(string devPath, Device dev)
    {
        ushort cylinder = 0;
        byte   head     = 0;
        byte   sector   = 1;
        byte   count    = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_MULTIPLE_command);
            AaruConsole.WriteLine(Localization.Cylinder_0, cylinder);
            AaruConsole.WriteLine(Localization.Head_0,     head);
            AaruConsole.WriteLine(Localization.Sector_0,   sector);
            AaruConsole.WriteLine(Localization.Count_0,    count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_cylinder);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out cylinder))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        cylinder = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.What_head);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out head))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        head = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(head > 15)
                    {
                        AaruConsole.WriteLine(Localization.Head_cannot_be_bigger_than_15_Setting_it_to_15);
                        head = 15;
                    }

                    AaruConsole.Write(Localization.What_sector);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out sector))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        sector = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_sectors);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out count))
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
                                      out AtaErrorRegistersChs errorRegisters,
                                      cylinder,
                                      head,
                                      sector,
                                      count,
                                      dev.Timeout,
                                      out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_MULTIPLE_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_MULTIPLE_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_MULTIPLE_status_registers);
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

    static void ReadSectors(string devPath, Device dev, bool retries)
    {
        ushort cylinder = 0;
        byte   head     = 0;
        byte   sector   = 1;
        byte   count    = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);

            AaruConsole.WriteLine(retries
                                      ? Localization.Parameters_for_READ_SECTORS_WITH_RETRIES_command
                                      : Localization.Parameters_for_READ_SECTORS_command);

            AaruConsole.WriteLine(Localization.Cylinder_0, cylinder);
            AaruConsole.WriteLine(Localization.Head_0,     head);
            AaruConsole.WriteLine(Localization.Sector_0,   sector);
            AaruConsole.WriteLine(Localization.Count_0,    count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_cylinder);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out cylinder))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        cylinder = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.What_head);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out head))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        head = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(head > 15)
                    {
                        AaruConsole.WriteLine(Localization.Head_cannot_be_bigger_than_15_Setting_it_to_15);
                        head = 15;
                    }

                    AaruConsole.Write(Localization.What_sector);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out sector))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        sector = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_sectors);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out count))
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
                              out AtaErrorRegistersChs errorRegisters,
                              retries,
                              cylinder,
                              head,
                              sector,
                              count,
                              dev.Timeout,
                              out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);

        AaruConsole.WriteLine(retries
                                  ? Localization.Sending_READ_SECTORS_WITH_RETRIES_to_the_device
                                  : Localization.Sending_READ_SECTORS_to_the_device);

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
        AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                AaruConsole.WriteLine(retries
                                          ? Localization.READ_SECTORS_WITH_RETRIES_response
                                          : Localization.READ_SECTORS_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                AaruConsole.WriteLine(retries
                                          ? Localization.READ_SECTORS_WITH_RETRIES_status_registers
                                          : Localization.READ_SECTORS_status_registers);

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

    static void Seek(string devPath, Device dev)
    {
        ushort cylinder = 0;
        byte   head     = 0;
        byte   sector   = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_SEEK_command);
            AaruConsole.WriteLine(Localization.Cylinder_0, cylinder);
            AaruConsole.WriteLine(Localization.Head_0,     head);
            AaruConsole.WriteLine(Localization.Sector_0,   sector);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_cylinder);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out cylinder))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        cylinder = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.What_head);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out head))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        head = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(head > 15)
                    {
                        AaruConsole.WriteLine(Localization.Head_cannot_be_bigger_than_15_Setting_it_to_15);
                        head = 15;
                    }

                    AaruConsole.Write(Localization.What_sector);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out sector))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        sector = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.Seek(out AtaErrorRegistersChs errorRegisters,
                              cylinder,
                              head,
                              sector,
                              dev.Timeout,
                              out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_SEEK_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Decode_error_registers);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.SEEK_status_registers);
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                goto start;
            case 3:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void SetFeatures(string devPath, Device dev)
    {
        ushort cylinder    = 0;
        byte   head        = 0;
        byte   sector      = 0;
        byte   feature     = 0;
        byte   sectorCount = 0;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_SET_FEATURES_command);
            AaruConsole.WriteLine(Localization.Cylinder_0,     cylinder);
            AaruConsole.WriteLine(Localization.Head_0,         head);
            AaruConsole.WriteLine(Localization.Sector_0,       sector);
            AaruConsole.WriteLine(Localization.Sector_count_0, sectorCount);
            AaruConsole.WriteLine(Localization.Feature_0_X2,   feature);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.What_cylinder);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out cylinder))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        cylinder = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.What_head);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out head))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        head = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(head > 15)
                    {
                        AaruConsole.WriteLine(Localization.Head_cannot_be_bigger_than_15_Setting_it_to_15);
                        head = 15;
                    }

                    AaruConsole.Write(Localization.What_sector);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out sector))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        sector = 0;
                        System.Console.ReadKey();
                    }

                    AaruConsole.Write(Localization.What_sector_count);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out sectorCount))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        sectorCount = 0;
                        System.Console.ReadKey();
                    }

                    AaruConsole.Write(Localization.What_feature);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out feature))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        feature = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.Seek(out AtaErrorRegistersChs errorRegisters,
                              cylinder,
                              head,
                              sector,
                              dev.Timeout,
                              out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_SET_FEATURES_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Decode_error_registers);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_CHS_ATA_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_CHS_ATA_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.SET_FEATURES_status_registers);
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                goto start;
            case 3:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}