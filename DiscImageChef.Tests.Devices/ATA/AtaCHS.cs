// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AtaCHS.cs
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

using DiscImageChef.Console;
using DiscImageChef.Decoders.ATA;
using DiscImageChef.Devices;

namespace DiscImageChef.Tests.Devices.ATA
{
    public static class AtaCHS
    {
        public static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a CHS ATA command to the device:");
                DicConsole.WriteLine("1.- Send IDENTIFY DEVICE command.");
                DicConsole.WriteLine("2.- Send READ DMA command.");
                DicConsole.WriteLine("3.- Send READ DMA WITH RETRIES command.");
                DicConsole.WriteLine("4.- Send READ LONG command.");
                DicConsole.WriteLine("5.- Send READ LONG WITH RETRIES command.");
                DicConsole.WriteLine("6.- Send READ MULTIPLE command.");
                DicConsole.WriteLine("7.- Send READ SECTORS command.");
                DicConsole.WriteLine("8.- Send READ SECTORS WITH RETRIES command.");
                DicConsole.WriteLine("9.- Send SEEK command.");
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
                        Identify(devPath, dev);
                        continue;
                    case 2:
                        ReadDma(devPath, dev, false);
                        continue;
                    case 3:
                        ReadDma(devPath, dev, true);
                        continue;
                    case 4:
                        ReadLong(devPath, dev, false);
                        continue;
                    case 5:
                        ReadLong(devPath, dev, true);
                        continue;
                    case 6:
                        ReadMultiple(devPath, dev);
                        continue;
                    case 7:
                        ReadSectors(devPath, dev, false);
                        continue;
                    case 8:
                        ReadSectors(devPath, dev, true);
                        continue;
                    case 9:
                        Seek(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void Identify(string devPath, Device dev)
        {
        start:
            System.Console.Clear();
            bool sense = dev.AtaIdentify(out byte[] buffer, out AtaErrorRegistersCHS errorRegisters, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending IDENTIFY DEVICE to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode buffer.");
            DicConsole.WriteLine("3.- Decode error registers.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");
            DicConsole.Write("Choose: ");

            string strDev = System.Console.ReadLine();
            if(!int.TryParse(strDev, out int item))
            {
                DicConsole.WriteLine("Not a number. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                goto menu;
            }

            switch(item)
            {
                case 0:
                    DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("IDENTIFY DEVICE response:");
                    if(buffer != null)
                        PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("IDENTIFY DEVICE decoded response:");
                    if(buffer != null)
                        DicConsole.WriteLine("{0}", Decoders.ATA.Identify.Prettify(buffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("IDENTIFY DEVICE status registers:");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void ReadDma(string devPath, Device dev, bool retries)
        {
            ushort cylinder = 0;
            byte head = 0;
            byte sector = 1;
            byte count = 1;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ DMA {0}command:", retries ? "WITH RETRIES " : "");
                DicConsole.WriteLine("Cylinder: {0}", cylinder);
                DicConsole.WriteLine("Head: {0}", head);
                DicConsole.WriteLine("Sector: {0}", sector);
                DicConsole.WriteLine("Count: {0}", count);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("What cylinder?: ");
                        strDev = System.Console.ReadLine();
                        if(!ushort.TryParse(strDev, out cylinder))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            cylinder = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("What head?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out head))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            head = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        if(head > 15)
                        {
                            DicConsole.WriteLine("Head cannot be bigger than 15. Setting it to 15...");
                            head = 15;
                        }
                        DicConsole.Write("What sector?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out sector))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            sector = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        break;
                    case 2:
                        goto start;
                }
            }

        start:
            System.Console.Clear();
            bool sense = dev.ReadDma(out byte[] buffer, out AtaErrorRegistersCHS errorRegisters, retries, cylinder, head, sector, count, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ DMA {0}to the device:", retries ? "WITH RETRIES " : "");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode error registers.");
            DicConsole.WriteLine("3.- Send command again.");
            DicConsole.WriteLine("4.- Change parameters.");
            DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DMA {0}response:", retries ? "WITH RETRIES " : "");
                    if(buffer != null)
                        PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DMA {0}status registers:", retries ? "WITH RETRIES " : "");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    goto start;
                case 4:
                    goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void ReadLong(string devPath, Device dev, bool retries)
        {
            ushort cylinder = 0;
            byte head = 0;
            byte sector = 1;
            uint blockSize = 1;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ LONG {0}command:", retries ? "WITH RETRIES " : "");
                DicConsole.WriteLine("Cylinder: {0}", cylinder);
                DicConsole.WriteLine("Head: {0}", head);
                DicConsole.WriteLine("Sector: {0}", sector);
                DicConsole.WriteLine("Block size: {0}", blockSize);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("What cylinder?: ");
                        strDev = System.Console.ReadLine();
                        if(!ushort.TryParse(strDev, out cylinder))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            cylinder = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("What head?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out head))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            head = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        if(head > 15)
                        {
                            DicConsole.WriteLine("Head cannot be bigger than 15. Setting it to 15...");
                            head = 15;
                        }
                        DicConsole.Write("What sector?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out sector))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            sector = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many bytes to expect?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        break;
                    case 2:
                        goto start;
                }
            }

        start:
            System.Console.Clear();
            bool sense = dev.ReadLong(out byte[] buffer, out AtaErrorRegistersCHS errorRegisters, retries, cylinder, head, sector, blockSize, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ LONG {0}to the device:", retries ? "WITH RETRIES " : "");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode error registers.");
            DicConsole.WriteLine("3.- Send command again.");
            DicConsole.WriteLine("4.- Change parameters.");
            DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ LONG {0}response:", retries ? "WITH RETRIES " : "");
                    if(buffer != null)
                        PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ LONG {0}status registers:", retries ? "WITH RETRIES " : "");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    goto start;
                case 4:
                    goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void ReadMultiple(string devPath, Device dev)
        {
            ushort cylinder = 0;
            byte head = 0;
            byte sector = 1;
            byte count = 1;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ MULTIPLE command:");
                DicConsole.WriteLine("Cylinder: {0}", cylinder);
                DicConsole.WriteLine("Head: {0}", head);
                DicConsole.WriteLine("Sector: {0}", sector);
                DicConsole.WriteLine("Count: {0}", count);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("What cylinder?: ");
                        strDev = System.Console.ReadLine();
                        if(!ushort.TryParse(strDev, out cylinder))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            cylinder = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("What head?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out head))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            head = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        if(head > 15)
                        {
                            DicConsole.WriteLine("Head cannot be bigger than 15. Setting it to 15...");
                            head = 15;
                        }
                        DicConsole.Write("What sector?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out sector))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            sector = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        break;
                    case 2:
                        goto start;
                }
            }

        start:
            System.Console.Clear();
            bool sense = dev.ReadMultiple(out byte[] buffer, out AtaErrorRegistersCHS errorRegisters, cylinder, head, sector, count, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ MULTIPLE to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode error registers.");
            DicConsole.WriteLine("3.- Send command again.");
            DicConsole.WriteLine("4.- Change parameters.");
            DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ MULTIPLE response:");
                    if(buffer != null)
                        PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ MULTIPLE status registers:");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    goto start;
                case 4:
                    goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void ReadSectors(string devPath, Device dev, bool retries)
        {
            ushort cylinder = 0;
            byte head = 0;
            byte sector = 1;
            byte count = 1;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ SECTORS {0}command:", retries ? "WITH RETRIES " : "");
                DicConsole.WriteLine("Cylinder: {0}", cylinder);
                DicConsole.WriteLine("Head: {0}", head);
                DicConsole.WriteLine("Sector: {0}", sector);
                DicConsole.WriteLine("Count: {0}", count);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("What cylinder?: ");
                        strDev = System.Console.ReadLine();
                        if(!ushort.TryParse(strDev, out cylinder))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            cylinder = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("What head?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out head))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            head = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        if(head > 15)
                        {
                            DicConsole.WriteLine("Head cannot be bigger than 15. Setting it to 15...");
                            head = 15;
                        }
                        DicConsole.Write("What sector?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out sector))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            sector = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        break;
                    case 2:
                        goto start;
                }
            }

        start:
            System.Console.Clear();
            bool sense = dev.Read(out byte[] buffer, out AtaErrorRegistersCHS errorRegisters, retries, cylinder, head, sector, count, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ SECTORS {0}to the device:", retries ? "WITH RETRIES " : "");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode error registers.");
            DicConsole.WriteLine("3.- Send command again.");
            DicConsole.WriteLine("4.- Change parameters.");
            DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ SECTORS {0}response:", retries ? "WITH RETRIES " : "");
                    if(buffer != null)
                        PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ SECTORS {0}status registers:", retries ? "WITH RETRIES " : "");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    goto start;
                case 4:
                    goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void Seek(string devPath, Device dev)
        {
            ushort cylinder = 0;
            byte head = 0;
            byte sector = 1;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for SEEK command:");
                DicConsole.WriteLine("Cylinder: {0}", cylinder);
                DicConsole.WriteLine("Head: {0}", head);
                DicConsole.WriteLine("Sector: {0}", sector);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("What cylinder?: ");
                        strDev = System.Console.ReadLine();
                        if(!ushort.TryParse(strDev, out cylinder))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            cylinder = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("What head?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out head))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            head = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        if(head > 15)
                        {
                            DicConsole.WriteLine("Head cannot be bigger than 15. Setting it to 15...");
                            head = 15;
                        }
                        DicConsole.Write("What sector?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out sector))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            sector = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        break;
                    case 2:
                        goto start;
                }
            }

        start:
            System.Console.Clear();
            bool sense = dev.Seek(out AtaErrorRegistersCHS errorRegisters, cylinder, head, sector, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SEEK to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Decode error registers.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    DicConsole.WriteLine("Returning to CHS ATA commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEEK status registers:");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    goto start;
                case 3:
                    goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }
    }
}

