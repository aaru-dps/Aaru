// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Pioneer.cs
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

using System;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SCSI;

static class Pioneer
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_Pioneer_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._1_Send_READ_CD_DA_command);
            AaruConsole.WriteLine(Localization._2_Send_READ_CD_DA_MSF_command);
            AaruConsole.WriteLine(Localization._3_Send_READ_CD_XA_command);
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
                    ReadCdDa(devPath, dev);

                    continue;
                case 2:
                    ReadCdDaMsf(devPath, dev);

                    continue;
                case 3:
                    ReadCdXa(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void ReadCdDa(string devPath, Device dev)
    {
        uint              address   = 0;
        uint              length    = 1;
        PioneerSubchannel subchan   = PioneerSubchannel.None;
        uint              blockSize = 2352;
        string            strDev;
        int               item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_CD_DA_command);
            AaruConsole.WriteLine(Localization.LBA_0,                   address);
            AaruConsole.WriteLine(Localization.Will_transfer_0_sectors, length);
            AaruConsole.WriteLine(Localization.Subchannel_mode_0,       subchan);
            AaruConsole.WriteLine(Localization._0_bytes_per_sector,     blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_Pioneer_vendor_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_Pioneer_vendor_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Logical_Block_Address_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        address = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_sectors_to_transfer_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out length))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        length = 1;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine(Localization.Subchannel_mode);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3,
                                          PioneerSubchannel.None,
                                          PioneerSubchannel.Q16,
                                          PioneerSubchannel.All,
                                          PioneerSubchannel.Only);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out subchan))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_subchannel_mode_Press_any_key_to_continue);
                        subchan = PioneerSubchannel.None;
                        System.Console.ReadKey();

                        continue;
                    }

                    blockSize = subchan switch
                                {
                                    PioneerSubchannel.Q16  => 2368,
                                    PioneerSubchannel.All  => 2448,
                                    PioneerSubchannel.Only => 96,
                                    _                      => 2352
                                };

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.PioneerReadCdDa(out byte[] buffer,
                                         out byte[] senseBuffer,
                                         address,
                                         blockSize,
                                         length,
                                         subchan,
                                         dev.Timeout,
                                         out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_CD_DA_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_Pioneer_vendor_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_Pioneer_vendor_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_DA_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_DA_sense);

                if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_DA_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 4:
                goto start;
            case 5:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadCdDaMsf(string devPath, Device dev)
    {
        byte              startFrame  = 0;
        byte              startSecond = 2;
        byte              startMinute = 0;
        byte              endFrame    = 0;
        const byte        endSecond   = 0;
        byte              endMinute   = 0;
        PioneerSubchannel subchan     = PioneerSubchannel.None;
        uint              blockSize   = 2352;
        string            strDev;
        int               item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_CD_DA_MSF_command);
            AaruConsole.WriteLine(Localization.Start_0_1_2,         startMinute, startSecond, startFrame);
            AaruConsole.WriteLine(Localization.End_0_1_2,           endMinute,   endSecond,   endFrame);
            AaruConsole.WriteLine(Localization.Subchannel_mode_0,   subchan);
            AaruConsole.WriteLine(Localization._0_bytes_per_sector, blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_Pioneer_vendor_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_Pioneer_vendor_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Start_minute_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out startMinute))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        startMinute = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Start_second_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out startSecond))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        startSecond = 2;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Start_frame_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out startFrame))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        startFrame = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.End_minute_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out endMinute))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        endMinute = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.End_second_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out endMinute))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        endMinute = 2;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.End_frame_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out endFrame))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        endFrame = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine(Localization.Subchannel_mode);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3,
                                          PioneerSubchannel.None,
                                          PioneerSubchannel.Q16,
                                          PioneerSubchannel.All,
                                          PioneerSubchannel.Only);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out subchan))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_subchannel_mode_Press_any_key_to_continue);
                        subchan = PioneerSubchannel.None;
                        System.Console.ReadKey();

                        continue;
                    }

                    blockSize = subchan switch
                                {
                                    PioneerSubchannel.Q16  => 2368,
                                    PioneerSubchannel.All  => 2448,
                                    PioneerSubchannel.Only => 96,
                                    _                      => 2352
                                };

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        var startMsf = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
        var endMsf   = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
        System.Console.Clear();

        bool sense = dev.PioneerReadCdDaMsf(out byte[] buffer,
                                            out byte[] senseBuffer,
                                            startMsf,
                                            endMsf,
                                            blockSize,
                                            subchan,
                                            dev.Timeout,
                                            out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_CD_DA_MSF_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_Pioneer_vendor_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_Pioneer_vendor_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_DA_MSF_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_DA_MSF_sense);

                if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_DA_MSF_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 4:
                goto start;
            case 5:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadCdXa(string devPath, Device dev)
    {
        uint   address     = 0;
        uint   length      = 1;
        var    errorFlags  = false;
        var    wholeSector = false;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_CD_XA_command);
            AaruConsole.WriteLine(Localization.LBA_0,                   address);
            AaruConsole.WriteLine(Localization.Will_transfer_0_sectors, length);
            AaruConsole.WriteLine(Localization.Include_error_flags_0,   errorFlags);
            AaruConsole.WriteLine(Localization.Whole_sector_0,          wholeSector);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_Pioneer_vendor_commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_Pioneer_vendor_commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Logical_Block_Address_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        address = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_sectors_to_transfer_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out length))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        length = 1;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Include_error_flags_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out errorFlags))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        errorFlags = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Read_whole_sector_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out wholeSector))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        wholeSector = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.PioneerReadCdXa(out byte[] buffer,
                                         out byte[] senseBuffer,
                                         address,
                                         length,
                                         errorFlags,
                                         wholeSector,
                                         dev.Timeout,
                                         out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_CD_XA_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_Pioneer_vendor_commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_Pioneer_vendor_commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_XA_response);

                if(buffer != null) PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_XA_sense);

                if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_XA_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 4:
                goto start;
            case 5:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}