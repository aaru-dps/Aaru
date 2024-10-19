// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SCSI.cs
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
using Aaru.Devices;
using Aaru.Tests.Devices.SCSI;

namespace Aaru.Tests.Devices;

static partial class MainClass
{
    public static void Scsi(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_SCSI_command_to_the_device);
            AaruConsole.WriteLine(Localization._1_Send_an_Adaptec_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._2_Send_an_Archive_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._3_Send_a_Certance_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._4_Send_a_Fujitsu_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._5_Send_an_HLDTST_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._6_Send_a_Hewlett_Packard_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._7_Send_a_Kreon_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._8_Send_a_SCSI_MultiMedia_Command_to_the_device);
            AaruConsole.WriteLine(Localization._9_Send_a_NEC_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._10_Send_a_Pioneer_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._11_Send_a_Plasmon_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._12_Send_a_Plextor_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization._13_Send_a_SCSI_Block_Command_to_the_device);
            AaruConsole.WriteLine(Localization._14_Send_a_SCSI_Media_Changer_command_to_the_device);
            AaruConsole.WriteLine(Localization._15_Send_a_SCSI_Primary_Command_to_the_device);
            AaruConsole.WriteLine(Localization._16_Send_a_SCSI_Streaming_Command_to_the_device);
            AaruConsole.WriteLine(Localization._17_Send_a_SyQuest_vendor_command_to_the_device);
            AaruConsole.WriteLine(Localization.Return_to_command_class_menu);
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
                    AaruConsole.WriteLine(Localization.Returning_to_command_class_menu);

                    return;
                case 1:
                    Adaptec.Menu(devPath, dev);

                    continue;
                case 2:
                    ArchiveCorp.Menu(devPath, dev);

                    continue;
                case 3:
                    Certance.Menu(devPath, dev);

                    continue;
                case 4:
                    Fujitsu.Menu(devPath, dev);

                    continue;
                case 5:
                    HlDtSt.Menu(devPath, dev);

                    continue;
                case 6:
                    Hp.Menu(devPath, dev);

                    continue;
                case 7:
                    Kreon.Menu(devPath, dev);

                    continue;
                case 8:
                    Mmc.Menu(devPath, dev);

                    continue;
                case 9:
                    Nec.Menu(devPath, dev);

                    continue;
                case 10:
                    Pioneer.Menu(devPath, dev);

                    continue;
                case 11:
                    Plasmon.Menu(devPath, dev);

                    continue;
                case 12:
                    Plextor.Menu(devPath, dev);

                    continue;
                case 13:
                    Sbc.Menu(devPath, dev);

                    continue;
                case 14:
                    Smc.Menu(devPath, dev);

                    continue;
                case 15:
                    Spc.Menu(devPath, dev);

                    continue;
                case 16:
                    Ssc.Menu(devPath, dev);

                    continue;
                case 17:
                    SyQuest.Menu(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }
}