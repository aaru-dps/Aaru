// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Fujitsu.cs
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

using System;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SCSI;

static class Fujitsu
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Send a Fujitsu vendor command to the device:");
            AaruConsole.WriteLine("1.- Send DISPLAY command.");
            AaruConsole.WriteLine("0.- Return to SCSI commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI commands menu...");

                    return;
                case 1:
                    Display(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
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
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for DISPLAY command:");
            AaruConsole.WriteLine("Descriptor: {0}", flash);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to Fujitsu vendor commands menu.");

            strDev = System.Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine("Not a number. Press any key to continue...");
                System.Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine("Returning to Fujitsu vendor commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Flash?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out flash))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        flash = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine("Display mode");

                    AaruConsole.WriteLine("Available values: {0} {1} {2} {3} {4}", FujitsuDisplayModes.Cancel,
                                          FujitsuDisplayModes.Cart, FujitsuDisplayModes.Half, FujitsuDisplayModes.Idle,
                                          FujitsuDisplayModes.Ready);

                    AaruConsole.Write("Choose?: ");
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out mode))
                    {
                        AaruConsole.WriteLine("Not a correct display mode. Press any key to continue...");
                        mode = FujitsuDisplayModes.Ready;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("First display half (will be cut to 7-bit ASCII, 8 chars?: ");
                    firstHalf = System.Console.ReadLine();
                    AaruConsole.Write("Second display half (will be cut to 7-bit ASCII, 8 chars?: ");
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
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending DISPLAY to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
        AaruConsole.WriteLine("DISPLAY decoded sense:");
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print sense buffer.");
        AaruConsole.WriteLine("2.- Send command again.");
        AaruConsole.WriteLine("3.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to Fujitsu vendor commands menu.");
        AaruConsole.Write("Choose: ");

        strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Fujitsu vendor commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("DISPLAY sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2: goto start;
            case 3: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}