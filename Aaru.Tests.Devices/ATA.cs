// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ATA.cs
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
using Aaru.Tests.Devices.ATA;

namespace Aaru.Tests.Devices;

static partial class MainClass
{
    public static void Ata(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine(Localization.Device_0, devPath);
            AaruConsole.WriteLine(Localization.Send_an_ATA_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_a_CHS_ATA_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_a_28_bit_ATA_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_a_48_bit_ATA_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_an_ATAPI_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_a_CompactFlash_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_a_Media_Card_Pass_Through_command_to_the_device);
            AaruConsole.WriteLine(Localization.Send_a_SMART_command_to_the_device);
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
                    AtaChs.Menu(devPath, dev);

                    continue;
                case 2:
                    Ata28.Menu(devPath, dev);

                    continue;
                case 3:
                    Ata48.Menu(devPath, dev);

                    continue;
                case 4:
                    Atapi.Menu(devPath, dev);

                    continue;
                case 5:
                    Cfa.Menu(devPath, dev);

                    continue;
                case 6:
                    Mcpt.Menu(devPath, dev);

                    continue;
                case 7:
                    Smart.Menu(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine(Localization.Incorrect_option_Press_any_key_to_continue);
                    System.Console.ReadKey();

                    continue;
            }
        }
    }
}