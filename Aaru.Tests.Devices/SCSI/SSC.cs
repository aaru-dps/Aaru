// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SSC.cs
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

using System;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.SSC;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SCSI;

static class Ssc
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_SCSI_Streaming_Command_to_the_device);
            AaruConsole.WriteLine(Localization._1_Send_LOAD_UNLOAD_command);
            AaruConsole.WriteLine(Localization._2_Send_LOCATE_10_command);
            AaruConsole.WriteLine(Localization._3_Send_LOCATE_16_command);
            AaruConsole.WriteLine(Localization._4_Send_READ_6_command);
            AaruConsole.WriteLine(Localization._5_Send_READ_16_command);
            AaruConsole.WriteLine(Localization._6_Send_READ_BLOCK_LIMITS_command);
            AaruConsole.WriteLine(Localization._7_Send_READ_POSITION_command);
            AaruConsole.WriteLine(Localization._8_Send_READ_REVERSE_6_command);
            AaruConsole.WriteLine(Localization._9_Send_READ_REVERSE_16_command);
            AaruConsole.WriteLine(Localization._10_Send_RECOVER_BUFFERED_DATA_command);
            AaruConsole.WriteLine(Localization._11_Send_REPORT_DENSITY_SUPPORT_command);
            AaruConsole.WriteLine(Localization._12_Send_REWIND_command);
            AaruConsole.WriteLine(Localization._13_Send_SPACE_command);
            AaruConsole.WriteLine(Localization._14_Send_TRACK_SELECT_command);
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
                    LoadUnload(devPath, dev);

                    continue;
                case 2:
                    Locate10(devPath, dev);

                    continue;
                case 3:
                    Locate16(devPath, dev);

                    continue;
                case 4:
                    Read6(devPath, dev);

                    continue;
                case 5:
                    Read16(devPath, dev);

                    continue;
                case 6:
                    ReadBlockLimits(devPath, dev);

                    continue;
                case 7:
                    ReadPosition(devPath, dev);

                    continue;
                case 8:
                    ReadReverse6(devPath, dev);

                    continue;
                case 9:
                    ReadReverse16(devPath, dev);

                    continue;
                case 10:
                    RecoverBufferedData(devPath, dev);

                    continue;
                case 11:
                    ReportDensitySupport(devPath, dev);

                    continue;
                case 12:
                    Rewind(devPath, dev);

                    continue;
                case 13:
                    Space(devPath, dev);

                    continue;
                case 14:
                    TrackSelect(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void LoadUnload(string devPath, Device dev)
    {
        var    load      = true;
        var    immediate = false;
        var    retense   = false;
        var    eot       = false;
        var    hold      = false;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_LOAD_UNLOAD_command);
            AaruConsole.WriteLine(Localization.Load_0,        load);
            AaruConsole.WriteLine(Localization.Immediate_0,   immediate);
            AaruConsole.WriteLine(Localization.Retense_0,     retense);
            AaruConsole.WriteLine(Localization.End_of_tape_0, eot);
            AaruConsole.WriteLine(Localization.Hold_0,        hold);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Load_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out load))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        load = true;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Immediate_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out immediate))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        immediate = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Retense_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out retense))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        retense = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.End_of_tape_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out eot))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        eot = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Hold_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out hold))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        hold = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.LoadUnload(out byte[] senseBuffer, immediate, load, retense, eot, hold, dev.Timeout,
                                    out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_LOAD_UNLOAD_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.LOAD_UNLOAD_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LOAD_UNLOAD_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

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

    static void Locate10(string devPath, Device dev)
    {
        var    blockType       = true;
        var    immediate       = false;
        var    changePartition = false;
        byte   partition       = 0;
        uint   objectId        = 0;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_LOCATE_10_command);
            AaruConsole.WriteLine(Localization.Locate_block_0,                              blockType);
            AaruConsole.WriteLine(Localization.Immediate_0,                                 immediate);
            AaruConsole.WriteLine(Localization.Change_partition_0,                          changePartition);
            AaruConsole.WriteLine(Localization.Partition_0,                                 partition);
            AaruConsole.WriteLine(blockType ? Localization.Block_0 : Localization.Object_0, objectId);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Load_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out blockType))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        blockType = true;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Immediate_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out immediate))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        immediate = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Change_partition_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out changePartition))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        changePartition = false;
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

                    AaruConsole.Write(blockType ? Localization.Block_Q : Localization.Object_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out objectId))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        objectId = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.Locate(out byte[] senseBuffer, immediate, blockType, changePartition, partition, objectId,
                                dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_LOCATE_10_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.LOCATE_10_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LOCATE_10_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

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

    static void Locate16(string devPath, Device dev)
    {
        SscLogicalIdTypes destType        = SscLogicalIdTypes.FileId;
        var               immediate       = false;
        var               changePartition = false;
        var               bam             = false;
        byte              partition       = 0;
        ulong             objectId        = 1;
        string            strDev;
        int               item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_LOCATE_16_command);
            AaruConsole.WriteLine(Localization.Object_type_0,         destType);
            AaruConsole.WriteLine(Localization.Immediate_0,           immediate);
            AaruConsole.WriteLine(Localization.Explicit_identifier_0, bam);
            AaruConsole.WriteLine(Localization.Change_partition_0,    changePartition);
            AaruConsole.WriteLine(Localization.Partition_0,           partition);
            AaruConsole.WriteLine(Localization.Object_identifier_0,   objectId);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.WriteLine(Localization.Object_type);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3, SscLogicalIdTypes.FileId,
                                          SscLogicalIdTypes.ObjectId, SscLogicalIdTypes.Reserved,
                                          SscLogicalIdTypes.SetId);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out destType))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_object_type_Press_any_key_to_continue);
                        destType = SscLogicalIdTypes.FileId;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Immediate_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out immediate))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        immediate = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Explicit_identifier_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out bam))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        bam = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Change_partition_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out changePartition))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        changePartition = false;
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

                    AaruConsole.Write(Localization.Identifier);
                    strDev = System.Console.ReadLine();

                    if(!ulong.TryParse(strDev, out objectId))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        objectId = 1;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.Locate16(out byte[] senseBuffer, immediate, changePartition, destType, bam, partition,
                                  objectId, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_LOCATE_16_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.LOCATE_16_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.LOCATE_16_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

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

    static void Read6(string devPath, Device dev)
    {
        var    sili      = false;
        var    fixedLen  = true;
        uint   blockSize = 512;
        uint   length    = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_6_command);
            AaruConsole.WriteLine(Localization.Fixed_block_size_0, fixedLen);
            AaruConsole.WriteLine(fixedLen ? Localization.Will_read_0_blocks : Localization.Will_read_0_bytes, length);

            if(fixedLen)
                AaruConsole.WriteLine(Localization._0_bytes_expected_per_block, blockSize);

            AaruConsole.WriteLine(Localization.Suppress_length_indicator_0, sili);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Fixed_block_size_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out fixedLen))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        fixedLen = true;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(fixedLen
                                          ? Localization.How_many_blocks_to_read_Q
                                          : Localization.How_many_bytes_to_read_Q);

                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out length))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        length = (uint)(fixedLen ? 1 : 512);
                        System.Console.ReadKey();

                        continue;
                    }

                    if(length > 0xFFFFFF)
                    {
                        AaruConsole.
                            WriteLine(
                                fixedLen
                                    ? Localization.Max_number_of_blocks_is_0_setting_to_0
                                    : Localization.Max_number_of_bytes_is_0_setting_to_0,
                                0xFFFFFF);

                        length = 0xFFFFFF;
                    }

                    if(fixedLen)
                    {
                        AaruConsole.Write(Localization.How_many_bytes_to_expect_per_block_Q);
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                            blockSize = 512;
                            System.Console.ReadKey();

                            continue;
                        }
                    }

                    AaruConsole.Write(Localization.Suppress_length_indicator_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out sili))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        sili = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.Read6(out byte[] buffer, out byte[] senseBuffer, sili, fixedLen, length, blockSize,
                               dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_6_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_6_response);

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
                AaruConsole.WriteLine(Localization.READ_6_sense);

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
                AaruConsole.WriteLine(Localization.READ_6_decoded_sense);
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

    static void Read16(string devPath, Device dev)
    {
        var    sili       = false;
        var    fixedLen   = true;
        uint   objectSize = 512;
        uint   length     = 1;
        byte   partition  = 0;
        ulong  objectId   = 0;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_16_command);
            AaruConsole.WriteLine(Localization.Fixed_block_size_0, fixedLen);
            AaruConsole.WriteLine(fixedLen ? Localization.Will_read_0_objects : Localization.Will_read_0_bytes, length);

            if(fixedLen)
                AaruConsole.WriteLine(Localization._0_bytes_expected_per_object, objectSize);

            AaruConsole.WriteLine(Localization.Suppress_length_indicator_0,    sili);
            AaruConsole.WriteLine(Localization.Read_object_0_from_partition_1, objectId, partition);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Fixed_block_size_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out fixedLen))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        fixedLen = true;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(fixedLen
                                          ? Localization.How_many_objects_to_read_Q
                                          : Localization.How_many_bytes_to_read_Q);

                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out length))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        length = (uint)(fixedLen ? 1 : 512);
                        System.Console.ReadKey();

                        continue;
                    }

                    if(length > 0xFFFFFF)
                    {
                        AaruConsole.
                            WriteLine(
                                fixedLen
                                    ? Localization.Max_number_of_blocks_is_0_setting_to_0
                                    : Localization.Max_number_of_bytes_is_0_setting_to_0,
                                0xFFFFFF);

                        length = 0xFFFFFF;
                    }

                    if(fixedLen)
                    {
                        AaruConsole.Write(Localization.How_many_bytes_to_expect_per_object_Q);
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out objectSize))
                        {
                            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                            objectSize = 512;
                            System.Console.ReadKey();

                            continue;
                        }
                    }

                    AaruConsole.Write(Localization.Suppress_length_indicator_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out sili))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        sili = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Object_identifier_Q);
                    strDev = System.Console.ReadLine();

                    if(!ulong.TryParse(strDev, out objectId))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        objectId = 0;
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
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.Read16(out byte[] buffer, out byte[] senseBuffer, sili, fixedLen, partition, objectId, length,
                                objectSize, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_16_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_16_response);

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
                AaruConsole.WriteLine(Localization.READ_16_sense);

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
                AaruConsole.WriteLine(Localization.READ_16_decoded_sense);
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

    static void ReadBlockLimits(string devPath, Device dev)
    {
    start:
        System.Console.Clear();

        bool sense = dev.ReadBlockLimits(out byte[] buffer, out byte[] senseBuffer, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_BLOCK_LIMITS_to_the_device);
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
        AaruConsole.WriteLine(Localization._2_Decode_buffer);
        AaruConsole.WriteLine(Localization._3_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._4_Decode_sense_buffer);
        AaruConsole.WriteLine(Localization._5_Send_command_again);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_BLOCK_LIMITS_response);

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
                AaruConsole.WriteLine(Localization.READ_BLOCK_LIMITS_decoded_response);

                if(buffer != null)
                    AaruConsole.WriteLine("{0}", BlockLimits.Prettify(buffer));

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_BLOCK_LIMITS_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 4:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_BLOCK_LIMITS_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 5:
                goto start;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadPosition(string devPath, Device dev)
    {
        SscPositionForms responseForm = SscPositionForms.Short;
        string           strDev;
        int              item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_LOCATE_16_command);
            AaruConsole.WriteLine(Localization.Response_form_0, responseForm);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.WriteLine(Localization.Response_form);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3_4_5_6_7_8, SscPositionForms.Short,
                                          SscPositionForms.VendorShort, SscPositionForms.OldLong,
                                          SscPositionForms.OldLongVendor, SscPositionForms.OldTclp,
                                          SscPositionForms.OldTclpVendor, SscPositionForms.Long,
                                          SscPositionForms.OldLongTclpVendor, SscPositionForms.Extended);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out responseForm))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_response_form_Press_any_key_to_continue);
                        responseForm = SscPositionForms.Short;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.ReadPosition(out _, out byte[] senseBuffer, responseForm, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_POSITION_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.READ_POSITION_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_POSITION_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

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

    static void ReadReverse6(string devPath, Device dev)
    {
        var    byteOrder = false;
        var    sili      = false;
        var    fixedLen  = true;
        uint   blockSize = 512;
        uint   length    = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_REVERSE_6_command);
            AaruConsole.WriteLine(Localization.Fixed_block_size_0, fixedLen);
            AaruConsole.WriteLine(fixedLen ? Localization.Will_read_0_blocks : Localization.Will_read_0_bytes, length);

            if(fixedLen)
                AaruConsole.WriteLine(Localization._0_bytes_expected_per_block, blockSize);

            AaruConsole.WriteLine(Localization.Suppress_length_indicator_0,    sili);
            AaruConsole.WriteLine(Localization.Drive_should_unreverse_bytes_0, byteOrder);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Fixed_block_size_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out fixedLen))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        fixedLen = true;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(fixedLen
                                          ? Localization.How_many_blocks_to_read_Q
                                          : Localization.How_many_bytes_to_read_Q);

                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out length))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        length = (uint)(fixedLen ? 1 : 512);
                        System.Console.ReadKey();

                        continue;
                    }

                    if(length > 0xFFFFFF)
                    {
                        AaruConsole.
                            WriteLine(
                                fixedLen
                                    ? Localization.Max_number_of_blocks_is_0_setting_to_0
                                    : Localization.Max_number_of_bytes_is_0_setting_to_0,
                                0xFFFFFF);

                        length = 0xFFFFFF;
                    }

                    if(fixedLen)
                    {
                        AaruConsole.Write(Localization.How_many_bytes_to_expect_per_block_Q);
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                            blockSize = 512;
                            System.Console.ReadKey();

                            continue;
                        }
                    }

                    AaruConsole.Write(Localization.Suppress_length_indicator_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out sili))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        sili = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Drive_should_unreverse_bytes_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out byteOrder))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        byteOrder = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.ReadReverse6(out byte[] buffer, out byte[] senseBuffer, byteOrder, sili, fixedLen, length,
                                      blockSize, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_REVERSE_6_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_REVERSE_6_response);

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
                AaruConsole.WriteLine(Localization.READ_REVERSE_6_sense);

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
                AaruConsole.WriteLine(Localization.READ_REVERSE_6_decoded_sense);
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

    static void ReadReverse16(string devPath, Device dev)
    {
        var    byteOrder  = false;
        var    sili       = false;
        var    fixedLen   = true;
        uint   objectSize = 512;
        uint   length     = 1;
        byte   partition  = 0;
        ulong  objectId   = 0;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_REVERSE_16_command);
            AaruConsole.WriteLine(Localization.Fixed_block_size_0, fixedLen);
            AaruConsole.WriteLine(fixedLen ? Localization.Will_read_0_objects : Localization.Will_read_0_bytes, length);

            if(fixedLen)
                AaruConsole.WriteLine(Localization._0_bytes_expected_per_object, objectSize);

            AaruConsole.WriteLine(Localization.Suppress_length_indicator_0,    sili);
            AaruConsole.WriteLine(Localization.Read_object_0_from_partition_1, objectId, partition);
            AaruConsole.WriteLine(Localization.Drive_should_unreverse_bytes_0, byteOrder);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Fixed_block_size_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out fixedLen))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        fixedLen = true;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(fixedLen
                                          ? Localization.How_many_objects_to_read_Q
                                          : Localization.How_many_bytes_to_read_Q);

                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out length))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        length = (uint)(fixedLen ? 1 : 512);
                        System.Console.ReadKey();

                        continue;
                    }

                    if(length > 0xFFFFFF)
                    {
                        AaruConsole.
                            WriteLine(
                                fixedLen
                                    ? Localization.Max_number_of_blocks_is_0_setting_to_0
                                    : Localization.Max_number_of_bytes_is_0_setting_to_0,
                                0xFFFFFF);

                        length = 0xFFFFFF;
                    }

                    if(fixedLen)
                    {
                        AaruConsole.Write(Localization.How_many_bytes_to_expect_per_object_Q);
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out objectSize))
                        {
                            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                            objectSize = 512;
                            System.Console.ReadKey();

                            continue;
                        }
                    }

                    AaruConsole.Write(Localization.Suppress_length_indicator_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out sili))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        sili = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Object_identifier_Q);
                    strDev = System.Console.ReadLine();

                    if(!ulong.TryParse(strDev, out objectId))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        objectId = 0;
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

                    AaruConsole.Write(Localization.Drive_should_unreverse_bytes_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out byteOrder))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        byteOrder = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.ReadReverse16(out byte[] buffer, out byte[] senseBuffer, byteOrder, sili, fixedLen, partition,
                                       objectId, length, objectSize, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_REVERSE_16_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_REVERSE_16_response);

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
                AaruConsole.WriteLine(Localization.READ_REVERSE_16_sense);

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
                AaruConsole.WriteLine(Localization.READ_REVERSE_16_decoded_sense);
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

    static void RecoverBufferedData(string devPath, Device dev)
    {
        var    sili      = false;
        var    fixedLen  = true;
        uint   blockSize = 512;
        uint   length    = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_RECOVER_BUFFERED_DATA_command);
            AaruConsole.WriteLine(Localization.Fixed_block_size_0, fixedLen);
            AaruConsole.WriteLine(fixedLen ? Localization.Will_read_0_blocks : Localization.Will_read_0_bytes, length);

            if(fixedLen)
                AaruConsole.WriteLine(Localization._0_bytes_expected_per_block, blockSize);

            AaruConsole.WriteLine(Localization.Suppress_length_indicator_0, sili);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Fixed_block_size_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out fixedLen))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        fixedLen = true;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(fixedLen
                                          ? Localization.How_many_blocks_to_read_Q
                                          : Localization.How_many_bytes_to_read_Q);

                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out length))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        length = (uint)(fixedLen ? 1 : 512);
                        System.Console.ReadKey();

                        continue;
                    }

                    if(length > 0xFFFFFF)
                    {
                        AaruConsole.
                            WriteLine(
                                fixedLen
                                    ? Localization.Max_number_of_blocks_is_0_setting_to_0
                                    : Localization.Max_number_of_bytes_is_0_setting_to_0,
                                0xFFFFFF);

                        length = 0xFFFFFF;
                    }

                    if(fixedLen)
                    {
                        AaruConsole.Write(Localization.How_many_bytes_to_expect_per_block_Q);
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                            blockSize = 512;
                            System.Console.ReadKey();

                            continue;
                        }
                    }

                    AaruConsole.Write(Localization.Suppress_length_indicator_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out sili))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        sili = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.RecoverBufferedData(out byte[] buffer, out byte[] senseBuffer, sili, fixedLen, length,
                                             blockSize, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_RECOVER_BUFFERED_DATA_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.RECOVER_BUFFERED_DATA_response);

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
                AaruConsole.WriteLine(Localization.RECOVER_BUFFERED_DATA_sense);

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
                AaruConsole.WriteLine(Localization.RECOVER_BUFFERED_DATA_decoded_sense);
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

    static void ReportDensitySupport(string devPath, Device dev)
    {
        var    medium  = false;
        var    current = false;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_REPORT_DENSITY_SUPPORT_command);
            AaruConsole.WriteLine(Localization.Report_about_medium_types_0,   medium);
            AaruConsole.WriteLine(Localization.Report_about_current_medium_0, current);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Report_about_medium_types_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out medium))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        medium = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Report_about_current_medium_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out current))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        current = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();

        bool sense = dev.ReportDensitySupport(out byte[] buffer, out byte[] senseBuffer, medium, current, dev.Timeout,
                                              out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_REPORT_DENSITY_SUPPORT_to_the_device);
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
        AaruConsole.WriteLine(Localization._2_Decode_buffer);
        AaruConsole.WriteLine(Localization._3_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._4_Decode_sense_buffer);
        AaruConsole.WriteLine(Localization._5_Send_command_again);
        AaruConsole.WriteLine(Localization._6_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.REPORT_DENSITY_SUPPORT_response);

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
                AaruConsole.WriteLine(Localization.REPORT_DENSITY_SUPPORT_decoded_buffer);

                AaruConsole.Write("{0}",
                                  medium
                                      ? DensitySupport.PrettifyMediumType(buffer)
                                      : DensitySupport.PrettifyDensity(buffer));

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.REPORT_DENSITY_SUPPORT_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 4:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.REPORT_DENSITY_SUPPORT_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 5:
                goto start;
            case 6:
                goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void Rewind(string devPath, Device dev)
    {
        var    immediate = false;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_REWIND_command);
            AaruConsole.WriteLine(Localization.Immediate_0, immediate);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Immediate_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out immediate))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        immediate = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();
        bool sense = dev.Rewind(out byte[] senseBuffer, immediate, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_REWIND_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.REWIND_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.REWIND_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

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

    static void Space(string devPath, Device dev)
    {
        SscSpaceCodes what  = SscSpaceCodes.LogicalBlock;
        int           count = -1;
        string        strDev;
        int           item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_SPACE_command);
            AaruConsole.WriteLine(Localization.What_to_space_0, what);
            AaruConsole.WriteLine(Localization.How_many_0,      count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.WriteLine(Localization.What_to_space);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3_4_5, SscSpaceCodes.LogicalBlock,
                                          SscSpaceCodes.Filemark, SscSpaceCodes.SequentialFilemark,
                                          SscSpaceCodes.EndOfData, SscSpaceCodes.Obsolete1, SscSpaceCodes.Obsolete2);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out what))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_space_type_Press_any_key_to_continue);
                        what = SscSpaceCodes.LogicalBlock;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.How_many_negative_for_reverse_Q);
                    strDev = System.Console.ReadLine();

                    if(!int.TryParse(strDev, out count))
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
        bool sense = dev.Space(out byte[] senseBuffer, what, count, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_SPACE_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.SPACE_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.SPACE_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

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

    static void TrackSelect(string devPath, Device dev)
    {
        byte   track = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_TRACK_SELECT_command);
            AaruConsole.WriteLine(Localization.Track_0, track);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Track_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out track))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        track = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2:
                    goto start;
            }
        }

    start:
        System.Console.Clear();
        bool sense = dev.TrackSelect(out byte[] senseBuffer, track, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_TRACK_SELECT_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0,        sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.TRACK_SELECT_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_Streaming_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_Streaming_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.TRACK_SELECT_sense);

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

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