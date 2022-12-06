// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Ata48.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;
using Aaru.Decoders.ATA;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.ATA
{
    internal static class Ata48
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Send a 48-bit ATA command to the device:");
                AaruConsole.WriteLine("1.- Send GET NATIVE MAX ADDRESS EXT command.");
                AaruConsole.WriteLine("2.- Send READ DMA EXT command.");
                AaruConsole.WriteLine("3.- Send READ LOG DMA EXT command.");
                AaruConsole.WriteLine("4.- Send READ LOG EXT command.");
                AaruConsole.WriteLine("5.- Send READ MATIVE MAX ADDRESS EXT command.");
                AaruConsole.WriteLine("6.- Send READ MULTIPLE EXT command.");
                AaruConsole.WriteLine("7.- Send READ SECTORS EXT command.");
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
                        GetNativeMaxAddressExt(devPath, dev);

                        continue;
                    case 2:
                        ReadDmaExt(devPath, dev);

                        continue;
                    case 3:
                        ReadLogDmaExt(devPath, dev);

                        continue;
                    case 4:
                        ReadLogExt(devPath, dev);

                        continue;
                    case 5:
                        ReadNativeMaxAddressExt(devPath, dev);

                        continue;
                    case 6:
                        ReadMultipleExt(devPath, dev);

                        continue;
                    case 7:
                        ReadSectorsExt(devPath, dev);

                        continue;
                    default:
                        AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();

                        continue;
                }
            }
        }

        static void GetNativeMaxAddressExt(string devPath, Device dev)
        {
            start:
            System.Console.Clear();

            bool sense = dev.GetNativeMaxAddressExt(out ulong lba, out AtaErrorRegistersLba48 errorRegisters,
                                                    dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending GET NATIVE MAX ADDRESS EXT to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("Max LBA is {0}.", lba);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Decode error registers.");
            AaruConsole.WriteLine("2.- Send command again.");
            AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("GET NATIVE MAX ADDRESS EXT status registers:");
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

        static void ReadDmaExt(string devPath, Device dev)
        {
            ulong  lba   = 0;
            ushort count = 1;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ DMA EXT command:");
                AaruConsole.WriteLine("LBA: {0}", lba);
                AaruConsole.WriteLine("Count: {0}", count);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("What logical block address?: ");
                        strDev = System.Console.ReadLine();

                        if(!ulong.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        if(lba > 0xFFFFFFFFFFFF)
                        {
                            AaruConsole.
                                WriteLine("Logical block address cannot be bigger than {0}. Setting it to {0}...",
                                          0xFFFFFFFFFFFF);

                            lba = 0xFFFFFFFFFFFF;
                        }

                        AaruConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadDma(out byte[] buffer, out AtaErrorRegistersLba48 errorRegisters, lba, count,
                                     dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ DMA EXT to the device:");
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
            AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ DMA EXT response:");

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
                    AaruConsole.WriteLine("READ DMA EXT status registers:");
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

        static void ReadLogExt(string devPath, Device dev)
        {
            byte   address = 0;
            ushort page    = 0;
            ushort count   = 1;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ LOG EXT command:");
                AaruConsole.WriteLine("Log address: {0}", address);
                AaruConsole.WriteLine("Page number: {0}", page);
                AaruConsole.WriteLine("Count: {0}", count);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("What log address?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out address))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("What page number?: ");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out page))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            page = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadLog(out byte[] buffer, out AtaErrorRegistersLba48 errorRegisters, address, page, count,
                                     dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ LOG EXT to the device:");
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
            AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ LOG EXT response:");

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
                    AaruConsole.WriteLine("READ LOG EXT status registers:");
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

        static void ReadLogDmaExt(string devPath, Device dev)
        {
            byte   address = 0;
            ushort page    = 0;
            ushort count   = 1;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ LOG DMA EXT command:");
                AaruConsole.WriteLine("Log address: {0}", address);
                AaruConsole.WriteLine("Page number: {0}", page);
                AaruConsole.WriteLine("Count: {0}", count);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("What log address?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out address))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("What page number?: ");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out page))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            page = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadLogDma(out byte[] buffer, out AtaErrorRegistersLba48 errorRegisters, address, page,
                                        count, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ LOG DMA EXT to the device:");
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
            AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ LOG DMA EXT response:");

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
                    AaruConsole.WriteLine("READ LOG DMA EXT status registers:");
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

        static void ReadMultipleExt(string devPath, Device dev)
        {
            ulong  lba   = 0;
            ushort count = 1;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ MULTIPLE EXT command:");
                AaruConsole.WriteLine("LBA: {0}", lba);
                AaruConsole.WriteLine("Count: {0}", count);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("What logical block address?: ");
                        strDev = System.Console.ReadLine();

                        if(!ulong.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        if(lba > 0xFFFFFFFFFFFF)
                        {
                            AaruConsole.
                                WriteLine("Logical block address cannot be bigger than {0}. Setting it to {0}...",
                                          0xFFFFFFFFFFFF);

                            lba = 0xFFFFFFFFFFFF;
                        }

                        AaruConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadMultiple(out byte[] buffer, out AtaErrorRegistersLba48 errorRegisters, lba, count,
                                          dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ MULTIPLE EXT to the device:");
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
            AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ MULTIPLE EXT response:");

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
                    AaruConsole.WriteLine("READ MULTIPLE EXT status registers:");
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

        static void ReadNativeMaxAddressExt(string devPath, Device dev)
        {
            start:
            System.Console.Clear();

            bool sense = dev.ReadNativeMaxAddress(out ulong lba, out AtaErrorRegistersLba48 errorRegisters, dev.Timeout,
                                                  out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ NATIVE MAX ADDRESS EXT to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("Max LBA is {0}.", lba);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Decode error registers.");
            AaruConsole.WriteLine("2.- Send command again.");
            AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ NATIVE MAX ADDRESS status registers:");
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

        static void ReadSectorsExt(string devPath, Device dev)
        {
            ulong  lba   = 0;
            ushort count = 1;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ SECTORS EXT command:");
                AaruConsole.WriteLine("LBA: {0}", lba);
                AaruConsole.WriteLine("Count: {0}", count);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("What logical block address?: ");
                        strDev = System.Console.ReadLine();

                        if(!ulong.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        if(lba > 0xFFFFFFFFFFFF)
                        {
                            AaruConsole.
                                WriteLine("Logical block address cannot be bigger than {0}. Setting it to {0}...",
                                          0xFFFFFFFFFFFF);

                            lba = 0xFFFFFFFFFFFF;
                        }

                        AaruConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.Read(out byte[] buffer, out AtaErrorRegistersLba48 errorRegisters, lba, count, dev.Timeout,
                                  out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ SECTORS EXT to the device:");
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
            AaruConsole.WriteLine("0.- Return to 48-bit ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to 48-bit ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ SECTORS EXT response:");

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
                    AaruConsole.WriteLine("READ SECTORS EXT status registers:");
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
}