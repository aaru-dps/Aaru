// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Command.cs
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

using Aaru.CommonTypes.Structs.Devices.SCSI;
using Aaru.Console;
using Aaru.Devices;

namespace Aaru.Tests.Devices;

static partial class MainClass
{
    public static void Command(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Send a command to the device:");
            AaruConsole.WriteLine("1.- Send a SCSI command.");
            AaruConsole.WriteLine("2.- Send an ATA command.");
            AaruConsole.WriteLine("3.- Send a SecureDigital/MultiMediaCard command.");
            AaruConsole.WriteLine("4.- Send a NVMe command.");

            if(dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice)
                AaruConsole.WriteLine("5.- Send a special sequence of commands for SCSI Multimedia devices.");

            AaruConsole.WriteLine("0.- Return to device menu.");
            AaruConsole.Write("Choose: ");

            string strDev = System.Console.ReadLine();

            if(!int.TryParse(strDev, out int item))
            {
                AaruConsole.WriteLine("Not a number. Press any key to continue...");
                System.Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine("Returning to device menu...");

                    return;
                case 1:
                    Scsi(devPath, dev);

                    continue;
                case 2:
                    Ata(devPath, dev);

                    continue;
                case 3:
                    SecureDigital(devPath, dev);

                    continue;
                case 4:
                    NVMe(devPath, dev);

                    continue;
                case 5 when dev.ScsiType == PeripheralDeviceTypes.MultiMediaDevice:
                    ScsiMmc.Menu(devPath, dev);

                    continue;

                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();

                    continue;
            }
        }
    }
}