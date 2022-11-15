// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Cfa.cs
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
using Aaru.Decoders.ATA;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.ATA;

static class Cfa
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Send a CompactFlash command to the device:");
            AaruConsole.WriteLine("1.- Send REQUEST EXTENDED ERROR CODE command.");
            AaruConsole.WriteLine("2.- Send CHS TRANSLATE SECTOR command.");
            AaruConsole.WriteLine("3.- Send LBA TRANSLATE SECTOR command.");
            AaruConsole.WriteLine("0.- Return to ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to ATA commands menu...");

                    return;
                case 1:
                    RequestExtendedErrorCode(devPath, dev);

                    continue;
                case 2:
                    TranslateSectorChs(devPath, dev);

                    continue;
                case 3:
                    TranslateSectorLba(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void RequestExtendedErrorCode(string devPath, Device dev)
    {
        start:
        System.Console.Clear();

        bool sense = dev.RequestExtendedErrorCode(out byte errorCode, out AtaErrorRegistersLba28 errorRegisters,
                                                  dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending REQUEST EXTENDED ERROR CODE to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Error code is {0}.", errorCode);
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Decode error registers.");
        AaruConsole.WriteLine("2.- Send command again.");
        AaruConsole.WriteLine("0.- Return to CompactFlash commands menu.");
        AaruConsole.Write("Choose: ");

        string strDev = System.Console.ReadLine();

        if(!int.TryParse(strDev, out int item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            System.Console.ReadKey();
            System.Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to CompactFlash commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("REQUEST EXTENDED ERROR CODE status registers:");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void TranslateSectorChs(string devPath, Device dev)
    {
        ushort cylinder = 0;
        byte   head     = 0;
        byte   sector   = 1;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for TRANSLATE SECTOR command:");
            AaruConsole.WriteLine("Cylinder: {0}", cylinder);
            AaruConsole.WriteLine("Head: {0}", head);
            AaruConsole.WriteLine("Sector: {0}", sector);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to CompactFlash commands menu.");

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
                    AaruConsole.WriteLine("Returning to CompactFlash commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("What cylinder?: ");
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out cylinder))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        cylinder = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("What head?: ");
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out head))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        head = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(head > 15)
                    {
                        AaruConsole.WriteLine("Head cannot be bigger than 15. Setting it to 15...");
                        head = 15;
                    }

                    AaruConsole.Write("What sector?: ");
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out sector))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        sector = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.TranslateSector(out byte[] buffer, out AtaErrorRegistersChs errorRegisters, cylinder, head,
                                         sector, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending TRANSLATE SECTOR to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode error registers.");
        AaruConsole.WriteLine("3.- Send command again.");
        AaruConsole.WriteLine("4.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to CompactFlash commands menu.");
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
                AaruConsole.WriteLine("Returning to CompactFlash commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("TRANSLATE SECTOR response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("TRANSLATE SECTOR status registers:");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3: goto start;
            case 4: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void TranslateSectorLba(string devPath, Device dev)
    {
        uint   lba = 0;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for TRANSLATE SECTOR command:");
            AaruConsole.WriteLine("LBA: {0}", lba);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to CompactFlash commands menu.");

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
                    AaruConsole.WriteLine("Returning to CompactFlash commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("What logical block address?: ");
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        lba = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(lba > 0xFFFFFFF)
                    {
                        AaruConsole.WriteLine("Logical block address cannot be bigger than {0}. Setting it to {0}...",
                                              0xFFFFFFF);

                        lba = 0xFFFFFFF;
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.TranslateSector(out byte[] buffer, out AtaErrorRegistersLba28 errorRegisters, lba, dev.Timeout,
                                         out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending TRANSLATE SECTOR to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode error registers.");
        AaruConsole.WriteLine("3.- Send command again.");
        AaruConsole.WriteLine("4.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to CompactFlash commands menu.");
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
                AaruConsole.WriteLine("Returning to CompactFlash commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("TRANSLATE SECTOR response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("TRANSLATE SECTOR status registers:");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3: goto start;
            case 4: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}