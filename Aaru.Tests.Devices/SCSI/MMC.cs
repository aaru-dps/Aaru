// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
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
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SCSI;

static class Mmc
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_MultiMedia_Command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_GET_CONFIGURATION_command);
            AaruConsole.WriteLine(Localization.Send_PREVENT_ALLOW_MEDIUM_REMOVAL_command);
            AaruConsole.WriteLine(Localization.Send_READ_CD_command);
            AaruConsole.WriteLine(Localization.Send_READ_CD_MSF_command);
            AaruConsole.WriteLine(Localization.Send_READ_DISC_INFORMATION_command);
            AaruConsole.WriteLine(Localization.Send_READ_DISC_STRUCTURE_command);
            AaruConsole.WriteLine(Localization.Send_READ_TOC_PMA_ATIP_command);
            AaruConsole.WriteLine(Localization.Send_START_STOP_UNIT_command);
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
                    GetConfiguration(devPath, dev);

                    continue;
                case 2:
                    PreventAllowMediumRemoval(devPath, dev);

                    continue;
                case 3:
                    ReadCd(devPath, dev);

                    continue;
                case 4:
                    ReadCdMsf(devPath, dev);

                    continue;
                case 5:
                    ReadDiscInformation(devPath, dev);

                    continue;
                case 6:
                    ReadDiscStructure(devPath, dev);

                    continue;
                case 7:
                    ReadTocPmaAtip(devPath, dev);

                    continue;
                case 8:
                    StartStopUnit(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void GetConfiguration(string devPath, Device dev)
    {
        MmcGetConfigurationRt rt                    = MmcGetConfigurationRt.All;
        ushort                startingFeatureNumber = 0;
        string                strDev;
        int                   item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_GET_CONFIGURATION_command);
            AaruConsole.WriteLine(Localization.RT_0, rt);
            AaruConsole.WriteLine(Localization.Feature_number_0, startingFeatureNumber);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                    return;
                case 1:
                    AaruConsole.WriteLine(Localization.RT);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3, MmcGetConfigurationRt.All,
                                          MmcGetConfigurationRt.Current, MmcGetConfigurationRt.Reserved,
                                          MmcGetConfigurationRt.Single);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out rt))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_object_type_Press_any_key_to_continue);
                        rt = MmcGetConfigurationRt.All;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Feature_number);
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out startingFeatureNumber))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        startingFeatureNumber = 1;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.GetConfiguration(out byte[] buffer, out byte[] senseBuffer, startingFeatureNumber, rt,
                                          dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_GET_CONFIGURATION_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.GET_CONFIGURATION_buffer);

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
                AaruConsole.WriteLine(Localization.GET_CONFIGURATION_decoded_buffer);

                if(buffer != null)
                {
                    Features.SeparatedFeatures ftr = Features.Separate(buffer);
                    AaruConsole.WriteLine(Localization.GET_CONFIGURATION_length_is_0_bytes, ftr.DataLength);
                    AaruConsole.WriteLine(Localization.GET_CONFIGURATION_current_profile_is_0_X4, ftr.CurrentProfile);

                    if(ftr.Descriptors != null)
                        foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
                        {
                            AaruConsole.WriteLine(Localization.Feature_0_X4, desc.Code);

                            switch(desc.Code)
                            {
                                case 0x0000:
                                    AaruConsole.Write("{0}", Features.Prettify_0000(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0001:
                                    AaruConsole.Write("{0}", Features.Prettify_0001(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0002:
                                    AaruConsole.Write("{0}", Features.Prettify_0002(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0003:
                                    AaruConsole.Write("{0}", Features.Prettify_0003(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0004:
                                    AaruConsole.Write("{0}", Features.Prettify_0004(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0010:
                                    AaruConsole.Write("{0}", Features.Prettify_0010(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x001D:
                                    AaruConsole.Write("{0}", Features.Prettify_001D(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x001E:
                                    AaruConsole.Write("{0}", Features.Prettify_001E(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x001F:
                                    AaruConsole.Write("{0}", Features.Prettify_001F(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0020:
                                    AaruConsole.Write("{0}", Features.Prettify_0020(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0021:
                                    AaruConsole.Write("{0}", Features.Prettify_0021(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0022:
                                    AaruConsole.Write("{0}", Features.Prettify_0022(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0023:
                                    AaruConsole.Write("{0}", Features.Prettify_0023(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0024:
                                    AaruConsole.Write("{0}", Features.Prettify_0024(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0025:
                                    AaruConsole.Write("{0}", Features.Prettify_0025(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0026:
                                    AaruConsole.Write("{0}", Features.Prettify_0026(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0027:
                                    AaruConsole.Write("{0}", Features.Prettify_0027(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0028:
                                    AaruConsole.Write("{0}", Features.Prettify_0028(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0029:
                                    AaruConsole.Write("{0}", Features.Prettify_0029(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x002A:
                                    AaruConsole.Write("{0}", Features.Prettify_002A(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x002B:
                                    AaruConsole.Write("{0}", Features.Prettify_002B(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x002C:
                                    AaruConsole.Write("{0}", Features.Prettify_002C(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x002D:
                                    AaruConsole.Write("{0}", Features.Prettify_002D(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x002E:
                                    AaruConsole.Write("{0}", Features.Prettify_002E(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x002F:
                                    AaruConsole.Write("{0}", Features.Prettify_002F(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0030:
                                    AaruConsole.Write("{0}", Features.Prettify_0030(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0031:
                                    AaruConsole.Write("{0}", Features.Prettify_0031(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0032:
                                    AaruConsole.Write("{0}", Features.Prettify_0032(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0033:
                                    AaruConsole.Write("{0}", Features.Prettify_0033(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0035:
                                    AaruConsole.Write("{0}", Features.Prettify_0035(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0037:
                                    AaruConsole.Write("{0}", Features.Prettify_0037(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0038:
                                    AaruConsole.Write("{0}", Features.Prettify_0038(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x003A:
                                    AaruConsole.Write("{0}", Features.Prettify_003A(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x003B:
                                    AaruConsole.Write("{0}", Features.Prettify_003B(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0040:
                                    AaruConsole.Write("{0}", Features.Prettify_0040(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0041:
                                    AaruConsole.Write("{0}", Features.Prettify_0041(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0042:
                                    AaruConsole.Write("{0}", Features.Prettify_0042(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0050:
                                    AaruConsole.Write("{0}", Features.Prettify_0050(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0051:
                                    AaruConsole.Write("{0}", Features.Prettify_0051(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0080:
                                    AaruConsole.Write("{0}", Features.Prettify_0080(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0100:
                                    AaruConsole.Write("{0}", Features.Prettify_0100(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0101:
                                    AaruConsole.Write("{0}", Features.Prettify_0101(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0102:
                                    AaruConsole.Write("{0}", Features.Prettify_0102(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0103:
                                    AaruConsole.Write("{0}", Features.Prettify_0103(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0104:
                                    AaruConsole.Write("{0}", Features.Prettify_0104(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0105:
                                    AaruConsole.Write("{0}", Features.Prettify_0105(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0106:
                                    AaruConsole.Write("{0}", Features.Prettify_0106(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0107:
                                    AaruConsole.Write("{0}", Features.Prettify_0107(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0108:
                                    AaruConsole.Write("{0}", Features.Prettify_0108(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0109:
                                    AaruConsole.Write("{0}", Features.Prettify_0109(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x010A:
                                    AaruConsole.Write("{0}", Features.Prettify_010A(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x010B:
                                    AaruConsole.Write("{0}", Features.Prettify_010B(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x010C:
                                    AaruConsole.Write("{0}", Features.Prettify_010C(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x010D:
                                    AaruConsole.Write("{0}", Features.Prettify_010D(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x010E:
                                    AaruConsole.Write("{0}", Features.Prettify_010E(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0110:
                                    AaruConsole.Write("{0}", Features.Prettify_0110(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0113:
                                    AaruConsole.Write("{0}", Features.Prettify_0113(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                case 0x0142:
                                    AaruConsole.Write("{0}", Features.Prettify_0142(desc.Data));
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                                default:
                                    AaruConsole.WriteLine(Localization.Dont_know_how_to_decode_feature_0, desc.Code);
                                    PrintHex.PrintHexArray(desc.Data, 64);

                                    break;
                            }
                        }
                }

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.GET_CONFIGURATION_sense);

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
                AaruConsole.WriteLine(Localization.GET_CONFIGURATION_decoded_sense);

                if(senseBuffer != null)
                    AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 5: goto start;
            case 6: goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void PreventAllowMediumRemoval(string devPath, Device dev)
    {
        bool   prevent    = false;
        bool   persistent = false;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_PREVENT_ALLOW_MEDIUM_REMOVAL_command);
            AaruConsole.WriteLine(Localization.Prevent_removal_0, prevent);
            AaruConsole.WriteLine(Localization.Persistent_value_0, persistent);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Prevent_removal_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out prevent))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        prevent = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Persistent_value_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out persistent))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        persistent = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.PreventAllowMediumRemoval(out byte[] senseBuffer, persistent, prevent, dev.Timeout,
                                                   out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_PREVENT_ALLOW_MEDIUM_REMOVAL_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.PREVENT_ALLOW_MEDIUM_REMOVAL_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.PREVENT_ALLOW_MEDIUM_REMOVAL_sense);

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

    static void ReadCd(string devPath, Device dev)
    {
        uint           address    = 0;
        uint           length     = 1;
        MmcSectorTypes sectorType = MmcSectorTypes.AllTypes;
        bool           dap        = false;
        bool           relative   = false;
        bool           sync       = false;
        MmcHeaderCodes header     = MmcHeaderCodes.None;
        bool           user       = true;
        bool           edc        = false;
        MmcErrorField  c2         = MmcErrorField.None;
        MmcSubchannel  subchan    = MmcSubchannel.None;
        uint           blockSize  = 2352;
        string         strDev;
        int            item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_CD_command);
            AaruConsole.WriteLine(Localization.Address_relative_to_current_position_0, relative);
            AaruConsole.WriteLine(relative ? Localization.Address_0 : Localization.LBA_0, address);
            AaruConsole.WriteLine(Localization.Will_transfer_0_sectors, length);
            AaruConsole.WriteLine(Localization.Sector_type_0, sectorType);
            AaruConsole.WriteLine(Localization.Process_audio_0, dap);
            AaruConsole.WriteLine(Localization.Retrieve_sync_bytes_0, sync);
            AaruConsole.WriteLine(Localization.Header_mode_0, header);
            AaruConsole.WriteLine(Localization.Retrieve_user_data_0, user);
            AaruConsole.WriteLine(Localization.Retrieve_EDC_ECC_data_0, edc);
            AaruConsole.WriteLine(Localization.C2_mode_0, c2);
            AaruConsole.WriteLine(Localization.Subchannel_mode_0, subchan);
            AaruConsole.WriteLine(Localization._0_bytes_per_sector, blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Address_is_relative_to_current_position);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out relative))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        relative = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(relative ? Localization.Address_Q : Localization.LBA_Q);
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

                    AaruConsole.WriteLine(Localization.Sector_type);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3_4_5, MmcSectorTypes.AllTypes,
                                          MmcSectorTypes.Cdda, MmcSectorTypes.Mode1, MmcSectorTypes.Mode2,
                                          MmcSectorTypes.Mode2Form1, MmcSectorTypes.Mode2Form2);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out sectorType))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_sector_type_Press_any_key_to_continue);
                        sectorType = MmcSectorTypes.AllTypes;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Process_audio_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out dap))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        dap = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Retrieve_sync_bytes_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out sync))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        sync = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine(Localization.Header_mode);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3, MmcHeaderCodes.None,
                                          MmcHeaderCodes.HeaderOnly, MmcHeaderCodes.SubHeaderOnly,
                                          MmcHeaderCodes.AllHeaders);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out header))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_header_mode_Press_any_key_to_continue);
                        header = MmcHeaderCodes.None;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Retrieve_user_data_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out user))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        user = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Retrieve_EDC_ECC_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out edc))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        edc = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine(Localization.C2_mode);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2, MmcErrorField.None,
                                          MmcErrorField.C2Pointers, MmcErrorField.C2PointersAndBlock);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out c2))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_C2_mode_Press_any_key_to_continue);
                        c2 = MmcErrorField.None;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine(Localization.Subchannel_mode);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3, MmcSubchannel.None, MmcSubchannel.Raw,
                                          MmcSubchannel.Q16, MmcSubchannel.Rw);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out subchan))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_subchannel_mode_Press_any_key_to_continue);
                        subchan = MmcSubchannel.None;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Expected_block_size_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out blockSize))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        blockSize = 2352;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ReadCd(out byte[] buffer, out byte[] senseBuffer, address, blockSize, length, sectorType, dap,
                                relative, sync, header, user, edc, c2, subchan, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_CD_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_response);

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
                AaruConsole.WriteLine(Localization.READ_CD_sense);

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
                AaruConsole.WriteLine(Localization.READ_CD_decoded_sense);
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

    static void ReadCdMsf(string devPath, Device dev)
    {
        byte           startFrame  = 0;
        byte           startSecond = 2;
        byte           startMinute = 0;
        byte           endFrame    = 0;
        const byte     endSecond   = 0;
        byte           endMinute   = 0;
        MmcSectorTypes sectorType  = MmcSectorTypes.AllTypes;
        bool           dap         = false;
        bool           sync        = false;
        MmcHeaderCodes header      = MmcHeaderCodes.None;
        bool           user        = true;
        bool           edc         = false;
        MmcErrorField  c2          = MmcErrorField.None;
        MmcSubchannel  subchan     = MmcSubchannel.None;
        uint           blockSize   = 2352;
        string         strDev;
        int            item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_CD_MSF_command);
            AaruConsole.WriteLine(Localization.Start_0_1_2, startMinute, startSecond, startFrame);
            AaruConsole.WriteLine(Localization.End_0_1_2, endMinute, endSecond, endFrame);
            AaruConsole.WriteLine(Localization.Sector_type_0, sectorType);
            AaruConsole.WriteLine(Localization.Process_audio_0, dap);
            AaruConsole.WriteLine(Localization.Retrieve_sync_bytes_0, sync);
            AaruConsole.WriteLine(Localization.Header_mode_0, header);
            AaruConsole.WriteLine(Localization.Retrieve_user_data_0, user);
            AaruConsole.WriteLine(Localization.Retrieve_EDC_ECC_data_0, edc);
            AaruConsole.WriteLine(Localization.C2_mode_0, c2);
            AaruConsole.WriteLine(Localization.Subchannel_mode_0, subchan);
            AaruConsole.WriteLine(Localization._0_bytes_per_sector, blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

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

                    AaruConsole.WriteLine(Localization.Sector_type);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3_4_5, MmcSectorTypes.AllTypes,
                                          MmcSectorTypes.Cdda, MmcSectorTypes.Mode1, MmcSectorTypes.Mode2,
                                          MmcSectorTypes.Mode2Form1, MmcSectorTypes.Mode2Form2);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out sectorType))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_sector_type_Press_any_key_to_continue);
                        sectorType = MmcSectorTypes.AllTypes;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Process_audio_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out dap))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        dap = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Retrieve_sync_bytes_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out sync))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        sync = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine(Localization.Header_mode);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3, MmcHeaderCodes.None,
                                          MmcHeaderCodes.HeaderOnly, MmcHeaderCodes.SubHeaderOnly,
                                          MmcHeaderCodes.AllHeaders);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out header))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_header_mode_Press_any_key_to_continue);
                        header = MmcHeaderCodes.None;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Retrieve_user_data_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out user))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        user = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Retrieve_EDC_ECC_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out edc))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        edc = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine(Localization.C2_mode);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2, MmcErrorField.None,
                                          MmcErrorField.C2Pointers, MmcErrorField.C2PointersAndBlock);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out c2))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_C2_mode_Press_any_key_to_continue);
                        c2 = MmcErrorField.None;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine(Localization.Subchannel_mode);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2_3, MmcSubchannel.None, MmcSubchannel.Raw,
                                          MmcSubchannel.Q16, MmcSubchannel.Rw);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out subchan))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_subchannel_mode_Press_any_key_to_continue);
                        subchan = MmcSubchannel.None;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Expected_block_size_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out blockSize))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        blockSize = 2352;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        uint startMsf = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
        uint endMsf   = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
        System.Console.Clear();

        bool sense = dev.ReadCdMsf(out byte[] buffer, out byte[] senseBuffer, startMsf, endMsf, blockSize, sectorType,
                                   dap, sync, header, user, edc, c2, subchan, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_CD_MSF_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_CD_MSF_response);

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
                AaruConsole.WriteLine(Localization.READ_CD_MSF_sense);

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
                AaruConsole.WriteLine(Localization.READ_CD_MSF_decoded_sense);
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

    static void ReadDiscInformation(string devPath, Device dev)
    {
        MmcDiscInformationDataTypes info = MmcDiscInformationDataTypes.DiscInformation;
        string                      strDev;
        int                         item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_DISC_INFORMATION_command);
            AaruConsole.WriteLine(Localization.Information_type_0, info);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                    return;
                case 1:
                    AaruConsole.WriteLine(Localization.Information_type);

                    AaruConsole.WriteLine(Localization.Available_values_0_1_2,
                                          MmcDiscInformationDataTypes.DiscInformation,
                                          MmcDiscInformationDataTypes.TrackResources,
                                          MmcDiscInformationDataTypes.PowResources);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out info))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_information_type_Press_any_key_to_continue);
                        info = MmcDiscInformationDataTypes.DiscInformation;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ReadDiscInformation(out byte[] buffer, out byte[] senseBuffer, info, dev.Timeout,
                                             out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_DISC_INFORMATION_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_DISC_INFORMATION_response);

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
                AaruConsole.WriteLine(Localization.READ_DISC_INFORMATION_decoded_response);
                AaruConsole.Write("{0}", DiscInformation.Prettify(buffer));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_DISC_INFORMATION_sense);

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
                AaruConsole.WriteLine(Localization.READ_DISC_INFORMATION_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 5: goto start;
            case 6: goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadDiscStructure(string devPath, Device dev)
    {
        MmcDiscStructureMediaType mediaType = MmcDiscStructureMediaType.Dvd;
        MmcDiscStructureFormat    format    = MmcDiscStructureFormat.CapabilityList;
        uint                      address   = 0;
        byte                      layer     = 0;
        byte                      agid      = 0;
        string                    strDev;
        int                       item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_DISC_STRUCTURE_command);
            AaruConsole.WriteLine(Localization.Media_type_0, mediaType);
            AaruConsole.WriteLine(Localization.Format_0, format);
            AaruConsole.WriteLine(Localization.Address_0, address);
            AaruConsole.WriteLine(Localization.Layer_0, layer);
            AaruConsole.WriteLine(Localization.AGID_0, agid);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                    return;
                case 1:
                    AaruConsole.WriteLine(Localization.Media_type);

                    AaruConsole.WriteLine(Localization.Available_values_0_1, MmcDiscStructureMediaType.Dvd,
                                          MmcDiscStructureMediaType.Bd);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out mediaType))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_media_type_Press_any_key_to_continue);
                        mediaType = MmcDiscStructureMediaType.Dvd;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine(Localization.Format);
                    AaruConsole.WriteLine(Localization.Available_values);

                    switch(mediaType)
                    {
                        case MmcDiscStructureMediaType.Dvd:
                            AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.PhysicalInformation,
                                                  MmcDiscStructureFormat.CopyrightInformation,
                                                  MmcDiscStructureFormat.DiscKey,
                                                  MmcDiscStructureFormat.BurstCuttingArea);

                            AaruConsole.WriteLine("\t{0} {1} {2} {3}",
                                                  MmcDiscStructureFormat.DiscManufacturingInformation,
                                                  MmcDiscStructureFormat.SectorCopyrightInformation,
                                                  MmcDiscStructureFormat.MediaIdentifier,
                                                  MmcDiscStructureFormat.MediaKeyBlock);

                            AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.DvdramDds,
                                                  MmcDiscStructureFormat.DvdramMediumStatus,
                                                  MmcDiscStructureFormat.DvdramSpareAreaInformation,
                                                  MmcDiscStructureFormat.DvdramRecordingType);

                            AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.LastBorderOutRmd,
                                                  MmcDiscStructureFormat.SpecifiedRmd,
                                                  MmcDiscStructureFormat.PreRecordedInfo,
                                                  MmcDiscStructureFormat.DvdrMediaIdentifier);

                            AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.DvdrPhysicalInformation,
                                                  MmcDiscStructureFormat.Adip,
                                                  MmcDiscStructureFormat.HddvdCopyrightInformation,
                                                  MmcDiscStructureFormat.DvdAacs);

                            AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.HddvdrMediumStatus,
                                                  MmcDiscStructureFormat.HddvdrLastRmd,
                                                  MmcDiscStructureFormat.DvdrLayerCapacity,
                                                  MmcDiscStructureFormat.MiddleZoneStart);

                            AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.JumpIntervalSize,
                                                  MmcDiscStructureFormat.ManualLayerJumpStartLba,
                                                  MmcDiscStructureFormat.RemapAnchorPoint, MmcDiscStructureFormat.Dcb);

                            break;
                        case MmcDiscStructureMediaType.Bd:
                            AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.DiscInformation,
                                                  MmcDiscStructureFormat.BdBurstCuttingArea,
                                                  MmcDiscStructureFormat.BdDds, MmcDiscStructureFormat.CartridgeStatus);

                            AaruConsole.WriteLine("\t{0} {1} {2}", MmcDiscStructureFormat.BdSpareAreaInformation,
                                                  MmcDiscStructureFormat.RawDfl, MmcDiscStructureFormat.Pac);

                            break;
                    }

                    AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.AacsVolId,
                                          MmcDiscStructureFormat.AacsMediaSerial, MmcDiscStructureFormat.AacsMediaId,
                                          MmcDiscStructureFormat.Aacsmkb);

                    AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.AacsDataKeys,
                                          MmcDiscStructureFormat.AacslbaExtents, MmcDiscStructureFormat.Aacsmkbcprm,
                                          MmcDiscStructureFormat.RecognizedFormatLayers);

                    AaruConsole.WriteLine("\t{0} {1}", MmcDiscStructureFormat.WriteProtectionStatus,
                                          MmcDiscStructureFormat.CapabilityList);

                    AaruConsole.Write(Localization.Choose_Q);
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out format))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_correct_structure_format_Press_any_key_to_continue);
                        format = MmcDiscStructureFormat.CapabilityList;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Address_Q);
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        address = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Layer_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out layer))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        layer = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.AGID_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out agid))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        agid = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ReadDiscStructure(out byte[] buffer, out byte[] senseBuffer, mediaType, address, layer, format,
                                           agid, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_DISC_STRUCTURE_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_DISC_STRUCTURE_response);

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

                // TODO: Implement
                AaruConsole.WriteLine(Localization.READ_DISC_STRUCTURE_decoding_not_yet_implemented);
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_DISC_STRUCTURE_sense);

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
                AaruConsole.WriteLine(Localization.READ_DISC_STRUCTURE_decoded_sense);
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 5: goto start;
            case 6: goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadTocPmaAtip(string devPath, Device dev)
    {
        bool   msf     = false;
        byte   format  = 0;
        byte   session = 0;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_READ_TOC_PMA_ATIP_command);
            AaruConsole.WriteLine(Localization.Return_MSF_values_0, msf);
            AaruConsole.WriteLine(Localization.Format_byte_0, format);
            AaruConsole.WriteLine(Localization.Session_0, session);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Return_MSF_values_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out msf))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        msf = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Format_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out format))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        format = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Session_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out session))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        session = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ReadTocPmaAtip(out byte[] buffer, out byte[] senseBuffer, msf, format, session, dev.Timeout,
                                        out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_READ_TOC_PMA_ATIP_to_the_device);
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
        AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_TOC_PMA_ATIP_buffer);

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
                AaruConsole.WriteLine(Localization.READ_TOC_PMA_ATIP_decoded_buffer);

                if(buffer != null)
                    switch(format)
                    {
                        case 0:
                            AaruConsole.Write("{0}", TOC.Prettify(buffer));
                            PrintHex.PrintHexArray(buffer, 64);

                            break;
                        case 1:
                            AaruConsole.Write("{0}", Session.Prettify(buffer));
                            PrintHex.PrintHexArray(buffer, 64);

                            break;
                        case 2:
                            AaruConsole.Write("{0}", FullTOC.Prettify(buffer));
                            PrintHex.PrintHexArray(buffer, 64);

                            break;
                        case 3:
                            AaruConsole.Write("{0}", PMA.Prettify(buffer));
                            PrintHex.PrintHexArray(buffer, 64);

                            break;
                        case 4:
                            AaruConsole.Write("{0}", ATIP.Prettify(buffer));
                            PrintHex.PrintHexArray(buffer, 64);

                            break;
                        case 5:
                            AaruConsole.Write("{0}", CDTextOnLeadIn.Prettify(buffer));
                            PrintHex.PrintHexArray(buffer, 64);

                            break;
                    }

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.READ_TOC_PMA_ATIP_sense);

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
                AaruConsole.WriteLine(Localization.READ_TOC_PMA_ATIP_decoded_sense);

                if(senseBuffer != null)
                    AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));

                AaruConsole.WriteLine(Localization.Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);

                goto menu;
            case 5: goto start;
            case 6: goto parameters;
            default:
                AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void StartStopUnit(string devPath, Device dev)
    {
        bool   immediate         = false;
        bool   changeFormatLayer = false;
        bool   loadEject         = false;
        bool   start             = false;
        byte   formatLayer       = 0;
        byte   powerConditions   = 0;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Parameters_for_START_STOP_UNIT_command);
            AaruConsole.WriteLine(Localization.Immediate_0, immediate);
            AaruConsole.WriteLine(Localization.Change_format_layer_0, changeFormatLayer);
            AaruConsole.WriteLine(Localization.Eject_0, loadEject);
            AaruConsole.WriteLine(Localization.Start_0, start);
            AaruConsole.WriteLine(Localization.Format_layer_0, formatLayer);
            AaruConsole.WriteLine(Localization.Power_conditions_0, powerConditions);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine(Localization.Choose_what_to_do);
            AaruConsole.WriteLine(Localization._1_Change_parameters);
            AaruConsole.WriteLine(Localization._2_Send_command_with_these_parameters);
            AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);

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
                    AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                    return;
                case 1:
                    AaruConsole.Write(Localization.Immediate_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out immediate))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        immediate = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Change_format_layer_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out changeFormatLayer))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        changeFormatLayer = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Eject_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out loadEject))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        loadEject = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Start_Q);
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out start))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_boolean_Press_any_key_to_continue);
                        start = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Format_layer_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out formatLayer))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        formatLayer = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write(Localization.Power_conditions_Q);
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out powerConditions))
                    {
                        AaruConsole.WriteLine(Localization.Not_a_number_Press_any_key_to_continue);
                        powerConditions = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.StartStopUnit(out byte[] senseBuffer, immediate, formatLayer, powerConditions,
                                       changeFormatLayer, loadEject, start, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine(Localization.Device_0, devPath);
        AaruConsole.WriteLine(Localization.Sending_START_STOP_UNIT_to_the_device);
        AaruConsole.WriteLine(Localization.Command_took_0_ms, duration);
        AaruConsole.WriteLine(Localization.Sense_is_0, sense);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_0_bytes,
                              senseBuffer?.Length.ToString() ?? Localization._null);

        AaruConsole.WriteLine(Localization.Sense_buffer_is_null_or_empty_0,
                              ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));

        AaruConsole.WriteLine(Localization.START_STOP_UNIT_decoded_sense);
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine(Localization.Choose_what_to_do);
        AaruConsole.WriteLine(Localization._1_Print_sense_buffer);
        AaruConsole.WriteLine(Localization._2_Send_command_again);
        AaruConsole.WriteLine(Localization._3_Change_parameters);
        AaruConsole.WriteLine(Localization.Return_to_SCSI_MultiMedia_Commands_menu);
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
                AaruConsole.WriteLine(Localization.Returning_to_SCSI_MultiMedia_Commands_menu);

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine(Localization.Device_0, devPath);
                AaruConsole.WriteLine(Localization.START_STOP_UNIT_sense);

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