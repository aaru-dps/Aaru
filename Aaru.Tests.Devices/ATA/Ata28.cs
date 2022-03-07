// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Ata28.cs
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

namespace Aaru.Tests.Devices.ATA;

using System;
using Aaru.Console;
using Aaru.Decoders.ATA;
using Aaru.Devices;
using Aaru.Helpers;

static class Ata28
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Send a 28-bit ATA command to the device:");
            AaruConsole.WriteLine("1.- Send READ BUFFER command.");
            AaruConsole.WriteLine("2.- Send READ BUFFER DMA command.");
            AaruConsole.WriteLine("3.- Send READ DMA command.");
            AaruConsole.WriteLine("4.- Send READ DMA WITH RETRIES command.");
            AaruConsole.WriteLine("5.- Send READ LONG command.");
            AaruConsole.WriteLine("6.- Send READ LONG WITH RETRIES command.");
            AaruConsole.WriteLine("7.- Send READ MULTIPLE command.");
            AaruConsole.WriteLine("8.- Send READ NATIVE MAX ADDRESS command.");
            AaruConsole.WriteLine("9.- Send READ SECTORS command.");
            AaruConsole.WriteLine("10.- Send READ SECTORS WITH RETRIES command.");
            AaruConsole.WriteLine("11.- Send SEEK command.");
            AaruConsole.WriteLine("0.- Return to ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to ATA commands menu...");

                    return;
                case 1:
                    ReadBuffer(devPath, dev);

                    continue;
                case 2:
                    ReadBufferDma(devPath, dev);

                    continue;
                case 3:
                    ReadDma(devPath, dev, false);

                    continue;
                case 4:
                    ReadDma(devPath, dev, true);

                    continue;
                case 5:
                    ReadLong(devPath, dev, false);

                    continue;
                case 6:
                    ReadLong(devPath, dev, true);

                    continue;
                case 7:
                    ReadMultiple(devPath, dev);

                    continue;
                case 8:
                    ReadNativeMaxAddress(devPath, dev);

                    continue;
                case 9:
                    ReadSectors(devPath, dev, false);

                    continue;
                case 10:
                    ReadSectors(devPath, dev, true);

                    continue;
                case 11:
                    Seek(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    Console.ReadKey();

                    continue;
            }
        }
    }

    static void ReadBuffer(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.ReadBuffer(out byte[] buffer, out AtaErrorRegistersLba28 errorRegisters, dev.Timeout,
                                    out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ BUFFER to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode error registers.");
        AaruConsole.WriteLine("3.- Send command again.");
        AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");
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
                AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ BUFFER response:");

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
                AaruConsole.WriteLine("READ BUFFER status registers:");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void ReadBufferDma(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.ReadBufferDma(out byte[] buffer, out AtaErrorRegistersLba28 errorRegisters, dev.Timeout,
                                       out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ BUFFER DMA to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode error registers.");
        AaruConsole.WriteLine("3.- Send command again.");
        AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");
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
                AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ BUFFER DMA response:");

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
                AaruConsole.WriteLine("READ BUFFER DMA status registers:");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void ReadDma(string devPath, Device dev, bool retries)
    {
        uint   lba   = 0;
        byte   count = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ DMA {0}command:", retries ? "WITH RETRIES " : "");
            AaruConsole.WriteLine("LBA: {0}", lba);
            AaruConsole.WriteLine("Count: {0}", count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");

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
                    AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("What logical block address?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        lba = 0;
                        Console.ReadKey();

                        continue;
                    }

                    if(lba > 0xFFFFFFF)
                    {
                        AaruConsole.WriteLine("Logical block address cannot be bigger than {0}. Setting it to {0}...",
                                              0xFFFFFFF);

                        lba = 0xFFFFFFF;
                    }

                    AaruConsole.Write("How many sectors?: ");
                    strDev = Console.ReadLine();

                    if(!byte.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        count = 0;
                        Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

    start:
        Console.Clear();

        bool sense = dev.ReadDma(out byte[] buffer, out AtaErrorRegistersLba28 errorRegisters, retries, lba, count,
                                 dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ DMA {0}to the device:", retries ? "WITH RETRIES " : "");
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
        AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");
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
                AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ DMA {0}response:", retries ? "WITH RETRIES " : "");

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
                AaruConsole.WriteLine("READ DMA {0}status registers:", retries ? "WITH RETRIES " : "");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3: goto start;
            case 4: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void ReadLong(string devPath, Device dev, bool retries)
    {
        uint   lba       = 0;
        uint   blockSize = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ LONG {0}command:", retries ? "WITH RETRIES " : "");
            AaruConsole.WriteLine("LBA: {0}", lba);
            AaruConsole.WriteLine("Block size: {0}", blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");

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
                    AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("What logical block address?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        lba = 0;
                        Console.ReadKey();

                        continue;
                    }

                    if(lba > 0xFFFFFFF)
                    {
                        AaruConsole.WriteLine("Logical block address cannot be bigger than {0}. Setting it to {0}...",
                                              0xFFFFFFF);

                        lba = 0xFFFFFFF;
                    }

                    AaruConsole.Write("How many bytes to expect?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out blockSize))
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

        bool sense = dev.ReadLong(out byte[] buffer, out AtaErrorRegistersLba28 errorRegisters, retries, lba, blockSize,
                                  dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ LONG {0}to the device:", retries ? "WITH RETRIES " : "");
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
        AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");
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
                AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ LONG {0}response:", retries ? "WITH RETRIES " : "");

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
                AaruConsole.WriteLine("READ LONG {0}status registers:", retries ? "WITH RETRIES " : "");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3: goto start;
            case 4: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void ReadMultiple(string devPath, Device dev)
    {
        uint   lba   = 0;
        byte   count = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ MULTIPLE command:");
            AaruConsole.WriteLine("LBA: {0}", lba);
            AaruConsole.WriteLine("Count: {0}", count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");

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
                    AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("What logical block address?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        lba = 0;
                        Console.ReadKey();

                        continue;
                    }

                    if(lba > 0xFFFFFFF)
                    {
                        AaruConsole.WriteLine("Logical block address cannot be bigger than {0}. Setting it to {0}...",
                                              0xFFFFFFF);

                        lba = 0xFFFFFFF;
                    }

                    AaruConsole.Write("How many sectors?: ");
                    strDev = Console.ReadLine();

                    if(!byte.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        count = 0;
                        Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

    start:
        Console.Clear();

        bool sense = dev.ReadMultiple(out byte[] buffer, out AtaErrorRegistersLba28 errorRegisters, lba, count,
                                      dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ MULTIPLE to the device:");
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
        AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");
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
                AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ MULTIPLE response:");

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
                AaruConsole.WriteLine("READ MULTIPLE status registers:");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3: goto start;
            case 4: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void ReadNativeMaxAddress(string devPath, Device dev)
    {
    start:
        Console.Clear();

        bool sense = dev.ReadNativeMaxAddress(out uint lba, out AtaErrorRegistersLba28 errorRegisters, dev.Timeout,
                                              out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ NATIVE MAX ADDRESS to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Max LBA is {0}.", lba);
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Decode error registers.");
        AaruConsole.WriteLine("2.- Send command again.");
        AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");
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
                AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ NATIVE MAX ADDRESS status registers:");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
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

    static void ReadSectors(string devPath, Device dev, bool retries)
    {
        uint   lba   = 0;
        byte   count = 1;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ SECTORS {0}command:", retries ? "WITH RETRIES " : "");
            AaruConsole.WriteLine("LBA: {0}", lba);
            AaruConsole.WriteLine("Count: {0}", count);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");

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
                    AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("What logical block address?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        lba = 0;
                        Console.ReadKey();

                        continue;
                    }

                    if(lba > 0xFFFFFFF)
                    {
                        AaruConsole.WriteLine("Logical block address cannot be bigger than {0}. Setting it to {0}...",
                                              0xFFFFFFF);

                        lba = 0xFFFFFFF;
                    }

                    AaruConsole.Write("How many sectors?: ");
                    strDev = Console.ReadLine();

                    if(!byte.TryParse(strDev, out count))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        count = 0;
                        Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

    start:
        Console.Clear();

        bool sense = dev.Read(out byte[] buffer, out AtaErrorRegistersLba28 errorRegisters, retries, lba, count,
                              dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ SECTORS {0}to the device:", retries ? "WITH RETRIES " : "");
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
        AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");
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
                AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ SECTORS {0}response:", retries ? "WITH RETRIES " : "");

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
                AaruConsole.WriteLine("READ SECTORS {0}status registers:", retries ? "WITH RETRIES " : "");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3: goto start;
            case 4: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void Seek(string devPath, Device dev)
    {
        uint   lba = 0;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for SEEK command:");
            AaruConsole.WriteLine("LBA: {0}", lba);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");

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
                    AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("What logical block address?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out lba))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        lba = 0;
                        Console.ReadKey();

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
        Console.Clear();
        bool sense = dev.Seek(out AtaErrorRegistersLba28 errorRegisters, lba, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending SEEK to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Decode error registers.");
        AaruConsole.WriteLine("2.- Send command again.");
        AaruConsole.WriteLine("3.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to 28-bit ATA commands menu.");
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
                AaruConsole.WriteLine("Returning to 28-bit ATA commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("SEEK status registers:");
                AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2: goto start;
            case 3: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }
}