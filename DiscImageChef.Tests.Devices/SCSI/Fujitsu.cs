// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Fujitsu.cs
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Console;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;

namespace DiscImageChef.Tests.Devices.SCSI
{
    static class Fujitsu
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a Fujitsu vendor command to the device:");
                DicConsole.WriteLine("1.- Send DISPLAY command.");
                DicConsole.WriteLine("0.- Return to SCSI commands menu.");
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
                        DicConsole.WriteLine("Returning to SCSI commands menu...");
                        return;
                    case 1:
                        Display(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void Display(string devPath, Device dev)
        {
            bool                flash      = false;
            FujitsuDisplayModes mode       = FujitsuDisplayModes.Ready;
            string              firstHalf  = "DIC TEST";
            string              secondHalf = "TEST DIC";
            string              strDev;
            int                 item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for DISPLAY command:");
                DicConsole.WriteLine("Descriptor: {0}", flash);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Fujitsu vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to Fujitsu vendor commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Flash?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out flash))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            flash = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("Display mode");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3} {4}", FujitsuDisplayModes.Cancel,
                                             FujitsuDisplayModes.Cart, FujitsuDisplayModes.Half,
                                             FujitsuDisplayModes.Idle, FujitsuDisplayModes.Ready);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!Enum.TryParse(strDev, true, out mode))
                        {
                            DicConsole.WriteLine("Not a correct display mode. Press any key to continue...");
                            mode = FujitsuDisplayModes.Ready;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("First display half (will be cut to 7-bit ASCII, 8 chars?: ");
                        firstHalf = System.Console.ReadLine();
                        DicConsole.Write("Second display half (will be cut to 7-bit ASCII, 8 chars?: ");
                        secondHalf = System.Console.ReadLine();
                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.FujitsuDisplay(out byte[] senseBuffer, flash, mode, firstHalf, secondHalf, dev.Timeout,
                                            out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending DISPLAY to the device:");
            DicConsole.WriteLine("Command took {0} ms.",               duration);
            DicConsole.WriteLine("Sense is {0}.",                      sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.",         senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("DISPLAY decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to Fujitsu vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Fujitsu vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("DISPLAY sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2: goto start;
                case 3: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }
    }
}