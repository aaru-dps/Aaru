// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MCPT.cs
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
using Aaru.Decoders.ATA;
using Aaru.Devices;

namespace Aaru.Tests.Devices.ATA
{
    static class Mcpt
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a Media Card Pass Through command to the device:");
                DicConsole.WriteLine("1.- Send CHECK MEDIA CARD TYPE command.");
                DicConsole.WriteLine("0.- Return to ATA commands menu.");
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
                        DicConsole.WriteLine("Returning to ATA commands menu...");
                        return;
                    case 1:
                        CheckMediaCardType(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void CheckMediaCardType(string devPath, Device dev)
        {
            byte   feature = 0;
            string strDev;
            int    item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for CHECK MEDIA CARD TYPE command:");
                DicConsole.WriteLine("Feature: {0}", feature);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Media Card Pass Through commands menu.");

                strDev = System.Console.ReadLine();
                if(!int.TryParse(strDev, out item))
                {
                    DicConsole.WriteLine("Not a number. Press any key to continue...");
                    System.Console.ReadKey();
                    continue;
                }

                switch(item)
                {
                    case 0:
                        DicConsole.WriteLine("Returning to Media Card Pass Through commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Feature?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out feature))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            feature = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.CheckMediaCardType(feature, out AtaErrorRegistersChs errorRegisters, dev.Timeout,
                                                out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending CHECK MEDIA CARD TYPE to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.",        sense);
            DicConsole.WriteLine("CHECK MEDIA CARD TYPE status registers:");
            DicConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Send command again.");
            DicConsole.WriteLine("2.- Change parameters.");
            DicConsole.WriteLine("0.- Return to Media Card Pass Through commands menu.");
            DicConsole.Write("Choose: ");

            strDev = System.Console.ReadLine();
            if(!int.TryParse(strDev, out item))
            {
                DicConsole.WriteLine("Not a number. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                goto menu;
            }

            switch(item)
            {
                case 0:
                    DicConsole.WriteLine("Returning to Media Card Pass Through commands menu...");
                    return;
                case 1: goto start;
                case 2: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }
    }
}