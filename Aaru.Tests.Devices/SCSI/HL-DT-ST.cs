// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HL-DT-ST.cs
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

namespace Aaru.Tests.Devices.SCSI;

using System;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

static class HlDtSt
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Send an HL-DT-ST vendor command to the device:");
            AaruConsole.WriteLine("1.- Send READ DVD (RAW) command.");
            AaruConsole.WriteLine("0.- Return to SCSI commands menu.");
            AaruConsole.Write("Choose: ");

            string strDev = Console.ReadLine();

            if(!int.TryParse(strDev, out int item))
            {
                AaruConsole.WriteLine("Not a number. Press any key to continue...");
                Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine("Returning to SCSI commands menu...");

                    return;
                case 1:
                    ReadDvdRaw(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    Console.ReadKey();

                    continue;
            }
        }
    }

    static void ReadDvdRaw(string devPath, Device dev)
    {
        uint   count = 1;
        uint   lba   = 0;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ DVD (RAW) command:");
            AaruConsole.WriteLine("LBA: {0}", lba);
            AaruConsole.WriteLine("Count: {0}", count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to HL-DT-ST vendor commands menu.");

            strDev = Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine("Not a number. Press any key to continue...");
                Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine("Returning to HL-DT-ST vendor commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("How many sectors?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine("Not a numbr. Press any key to continue...");
                        count = 1;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("LBA?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        lba = 0;
                        Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

    start:
        Console.Clear();

        bool sense = dev.HlDtStReadRawDvd(out byte[] buffer, out byte[] senseBuffer, lba, count, dev.Timeout,
                                          out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ DVD (RAW) to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Print sense buffer.");
        AaruConsole.WriteLine("3.- Decode sense buffer.");
        AaruConsole.WriteLine("4.- Send command again.");
        AaruConsole.WriteLine("5.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to HL-DT-ST vendor commands menu.");
        AaruConsole.Write("Choose: ");

        strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to HL-DT-ST vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ DVD (RAW) response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ DVD (RAW) sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ DVD (RAW) decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            case 5: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }
}