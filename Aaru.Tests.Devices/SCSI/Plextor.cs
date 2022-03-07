// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Plextor.cs
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

static class Plextor
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Send a Plextor vendor command to the device:");
            AaruConsole.WriteLine("1.- Send GET BOOK BITSETTING command.");
            AaruConsole.WriteLine("2.- Send GET GIGAREC command.");
            AaruConsole.WriteLine("3.- Send GET SECUREC command.");
            AaruConsole.WriteLine("4.- Send GET SILENT MODE command.");
            AaruConsole.WriteLine("5.- Send GET SINGLE-SESSION / HIDE CD-R command.");
            AaruConsole.WriteLine("6.- Send GET SPEEDREAD command.");
            AaruConsole.WriteLine("7.- Send GET TEST WRITE DVD+ command.");
            AaruConsole.WriteLine("8.- Send GET VARIREC command.");
            AaruConsole.WriteLine("9.- Send POWEREC GET SPEEDS command.");
            AaruConsole.WriteLine("10.- Send READ CD-DA command.");
            AaruConsole.WriteLine("11.- Send READ DVD (RAW) command.");
            AaruConsole.WriteLine("12.- Send READ EEPROM (CD recorders) command.");
            AaruConsole.WriteLine("13.- Send READ EEPROM (DVD recorders) command.");
            AaruConsole.WriteLine("14.- Send READ EEPROM (PX-708 and PX-712) command.");
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
                    GetBookBitsetting(devPath, dev);

                    continue;
                case 2:
                    GetGigaRec(devPath, dev);

                    continue;
                case 3:
                    GetSecuRec(devPath, dev);

                    continue;
                case 4:
                    GetSilentMode(devPath, dev);

                    continue;
                case 5:
                    GetSingleSessionHideCdR(devPath, dev);

                    continue;
                case 6:
                    GetSpeedRead(devPath, dev);

                    continue;
                case 7:
                    GetTestWriteDvdPlus(devPath, dev);

                    continue;
                case 8:
                    GetVariRec(devPath, dev);

                    continue;
                case 9:
                    PoweRecGetSpeeds(devPath, dev);

                    continue;
                case 10:
                    ReadCdDa(devPath, dev);

                    continue;
                case 11:
                    ReadDvdRaw(devPath, dev);

                    continue;
                case 12:
                    ReadEepromCdR(devPath, dev);

                    continue;
                case 13:
                    ReadEeprom(devPath, dev);

                    continue;
                case 14:
                    ReadEepromBlock(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    Console.ReadKey();

                    continue;
            }
        }
    }

    static void GetBookBitsetting(string devPath, Device dev)
    {
        var    dl = false;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for GET BOOK BITSETTING command:");
            AaruConsole.WriteLine("Dual layer?: {0}", dl);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");

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
                    AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Dual layer?: ");
                    strDev = Console.ReadLine();

                    if(!bool.TryParse(strDev, out dl))
                    {
                        AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                        dl = false;
                        Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

    start:
        Console.Clear();

        bool sense = dev.PlextorGetBitsetting(out byte[] buffer, out byte[] senseBuffer, dl, dev.Timeout,
                                              out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending GET BOOK BITSETTING to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
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
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("GET BOOK BITSETTING response:");

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
                AaruConsole.WriteLine("GET BOOK BITSETTING sense:");

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
                AaruConsole.WriteLine("GET BOOK BITSETTING decoded sense:");
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

    static void GetGigaRec(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.PlextorGetGigaRec(out byte[] buffer, out byte[] senseBuffer, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending GET GIGAREC to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("GET GIGAREC response:");

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
                AaruConsole.WriteLine("GET GIGAREC sense:");

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
                AaruConsole.WriteLine("GET GIGAREC decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void GetSecuRec(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.PlextorGetSecuRec(out byte[] buffer, out byte[] senseBuffer, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending GET SECUREC to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("GET SECUREC response:");

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
                AaruConsole.WriteLine("GET SECUREC sense:");

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
                AaruConsole.WriteLine("GET SECUREC decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void GetSilentMode(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.PlextorGetSilentMode(out byte[] buffer, out byte[] senseBuffer, dev.Timeout,
                                              out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending GET SILENT MODE to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("GET SILENT MODE response:");

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
                AaruConsole.WriteLine("GET SILENT MODE sense:");

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
                AaruConsole.WriteLine("GET SILENT MODE decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void GetSingleSessionHideCdR(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.PlextorGetHiding(out byte[] buffer, out byte[] senseBuffer, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending GET SINGLE-SESSION / HIDE CD-R to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("GET SINGLE-SESSION / HIDE CD-R response:");

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
                AaruConsole.WriteLine("GET SINGLE-SESSION / HIDE CD-R sense:");

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
                AaruConsole.WriteLine("GET SINGLE-SESSION / HIDE CD-R decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void GetSpeedRead(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.PlextorGetSpeedRead(out byte[] buffer, out byte[] senseBuffer, dev.Timeout,
                                             out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending GET SPEEDREAD to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("GET SPEEDREAD response:");

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
                AaruConsole.WriteLine("GET SPEEDREAD sense:");

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
                AaruConsole.WriteLine("GET SPEEDREAD decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void GetTestWriteDvdPlus(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense =
            dev.PlextorGetTestWriteDvdPlus(out byte[] buffer, out byte[] senseBuffer, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending GET TEST WRITE DVD+ to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("GET TEST WRITE DVD+ response:");

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
                AaruConsole.WriteLine("GET TEST WRITE DVD+ sense:");

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
                AaruConsole.WriteLine("GET TEST WRITE DVD+ decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void GetVariRec(string devPath, Device dev)
    {
        var    dvd = false;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for GET VARIREC command:");
            AaruConsole.WriteLine("DVD?: {0}", dvd);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");

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
                    AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("DVD?: ");
                    strDev = Console.ReadLine();

                    if(!bool.TryParse(strDev, out dvd))
                    {
                        AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                        dvd = false;
                        Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

    start:
        Console.Clear();

        bool sense = dev.PlextorGetVariRec(out byte[] buffer, out byte[] senseBuffer, dvd, dev.Timeout,
                                           out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending GET VARIREC to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
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
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("GET VARIREC response:");

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
                AaruConsole.WriteLine("GET VARIREC sense:");

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
                AaruConsole.WriteLine("GET VARIREC decoded sense:");
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

    static void PoweRecGetSpeeds(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.PlextorGetPoweRec(out byte[] senseBuffer, out bool enabled, out ushort speed, dev.Timeout,
                                           out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending POWEREC GET SPEEDS to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("PoweRec is {0}.", enabled ? "enabled" : "disabled");
        AaruConsole.WriteLine("Speed: {0}", speed);
        AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
        AaruConsole.WriteLine("POWEREC GET SPEEDS decoded sense:");
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print sense buffer.");
        AaruConsole.WriteLine("2.- Send command again.");
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("POWEREC GET SPEEDS sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void ReadCdDa(string devPath, Device dev)
    {
        uint              address   = 0;
        uint              length    = 1;
        PlextorSubchannel subchan   = PlextorSubchannel.None;
        uint              blockSize = 2352;
        string            strDev;
        int               item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ CD-DA command:");
            AaruConsole.WriteLine("LBA: {0}", address);
            AaruConsole.WriteLine("Will transfer {0} sectors", length);
            AaruConsole.WriteLine("Subchannel mode: {0}", subchan);
            AaruConsole.WriteLine("{0} bytes per sectors", blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");

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
                    AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Logical Block Address?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        address = 0;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("How many sectors to transfer?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out length))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        length = 1;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine("Subchannel mode");

                    AaruConsole.WriteLine("Available values: {0} {1} {2} {3} {4}", PlextorSubchannel.None,
                                          PlextorSubchannel.Q16, PlextorSubchannel.All, PlextorSubchannel.Pack,
                                          PlextorSubchannel.RawC2);

                    AaruConsole.Write("Choose?: ");
                    strDev = Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out subchan))
                    {
                        AaruConsole.WriteLine("Not a correct subchannel mode. Press any key to continue...");
                        subchan = PlextorSubchannel.None;
                        Console.ReadKey();

                        continue;
                    }

                    switch(subchan)
                    {
                        case PlextorSubchannel.Q16:
                            blockSize = 2368;

                            break;
                        case PlextorSubchannel.Pack:
                        case PlextorSubchannel.All:
                            blockSize = 2448;

                            break;
                        case PlextorSubchannel.RawC2:
                            blockSize = 2742;

                            break;
                        default:
                            blockSize = 2352;

                            break;
                    }

                    break;
                case 2: goto start;
            }
        }

    start:
        Console.Clear();

        bool sense = dev.PlextorReadCdDa(out byte[] buffer, out byte[] senseBuffer, address, blockSize, length, subchan,
                                         dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ CD-DA to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
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
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CD-DA response:");

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
                AaruConsole.WriteLine("READ CD-DA sense:");

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
                AaruConsole.WriteLine("READ CD-DA decoded sense:");
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
            AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");

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
                    AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

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

        bool sense = dev.PlextorReadRawDvd(out byte[] buffer, out byte[] senseBuffer, lba, count, dev.Timeout,
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
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
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

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

    static void ReadEepromCdR(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.PlextorReadEepromCdr(out byte[] buffer, out byte[] senseBuffer, dev.Timeout,
                                              out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ EEPROM to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ EEPROM response:");

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
                AaruConsole.WriteLine("READ EEPROM sense:");

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
                AaruConsole.WriteLine("READ EEPROM decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void ReadEepromBlock(string devPath, Device dev)
    {
        byte   block     = 0;
        ushort blockSize = 256;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ EEPROM command:");
            AaruConsole.WriteLine("EEPROM block to read: {0}", block);
            AaruConsole.WriteLine("EEPROM block size: {0}", blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");

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
                    AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("EEPROM block to read?: ");
                    strDev = Console.ReadLine();

                    if(!byte.TryParse(strDev, out block))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        block = 0;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("EEPROM block size?: ");
                    strDev = Console.ReadLine();

                    if(!ushort.TryParse(strDev, out blockSize))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        blockSize = 0;
                        Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

    start:
        Console.Clear();

        bool sense = dev.PlextorReadEepromBlock(out byte[] buffer, out byte[] senseBuffer, block, blockSize,
                                                dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ EEPROM to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
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
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ EEPROM response:");

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
                AaruConsole.WriteLine("READ EEPROM sense:");

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
                AaruConsole.WriteLine("READ EEPROM decoded sense:");
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

    static void ReadEeprom(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.PlextorReadEeprom(out byte[] buffer, out byte[] senseBuffer, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ EEPROM to the device:");
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
        AaruConsole.WriteLine("0.- Return to Plextor vendor commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Plextor vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ EEPROM response:");

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
                AaruConsole.WriteLine("READ EEPROM sense:");

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
                AaruConsole.WriteLine("READ EEPROM decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }
}