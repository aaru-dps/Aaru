// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SCSI_MMC.cs
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
using Aaru.Devices;

namespace Aaru.Tests.Devices;

static partial class ScsiMmc
{
    public static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_a_special_SCSI_MultiMedia_command_to_the_device);

            AaruConsole.WriteLine(Localization.
                                      Try_to_read_the_cache_data_from_a_device_with_a_MediaTek_chipset_F1h_command_06h_subcommand);

            AaruConsole.WriteLine(Localization.Try_to_read_a_GD_ROM_using_a_trap_disc);
            AaruConsole.WriteLine(Localization.Try_to_read_Lead_Out_using_a_trap_disc);

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
                    MediaTekReadCache(devPath, dev);

                    continue;
                case 2:
                    CheckGdromReadability(devPath, dev);

                    continue;
                case 3:
                    ReadLeadOutUsingTrapDisc(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }
}