// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Command.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef device testing.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;
using Aaru.Devices;

namespace Aaru.Tests.Devices
{
    static partial class MainClass
    {
        public static void Command(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a command to the device:");
                DicConsole.WriteLine("1.- Send a SCSI command.");
                DicConsole.WriteLine("2.- Send an ATA command.");
                DicConsole.WriteLine("3.- Send a SecureDigital/MultiMediaCard command.");
                DicConsole.WriteLine("4.- Send a NVMe command.");
                DicConsole.WriteLine("0.- Return to device menu.");
                DicConsole.Write("Choose: ");

                string strDev = System.Console.ReadLine();
                if(!int.TryParse(strDev, out int item))
                {
                    DicConsole.WriteLine("Not a number. Press any key to continue...");
                    System.Console.ReadKey();
                    continue;
                }

                switch(item)
                {
                    case 0:
                        DicConsole.WriteLine("Returning to device menu...");
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
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }
    }
}