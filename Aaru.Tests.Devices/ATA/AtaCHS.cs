// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AtaCHS.cs
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
    internal static class AtaChs
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Send a CHS ATA command to the device:");
                AaruConsole.WriteLine("1.- Send IDENTIFY DEVICE command.");
                AaruConsole.WriteLine("2.- Send READ DMA command.");
                AaruConsole.WriteLine("3.- Send READ DMA WITH RETRIES command.");
                AaruConsole.WriteLine("4.- Send READ LONG command.");
                AaruConsole.WriteLine("5.- Send READ LONG WITH RETRIES command.");
                AaruConsole.WriteLine("6.- Send READ MULTIPLE command.");
                AaruConsole.WriteLine("7.- Send READ SECTORS command.");
                AaruConsole.WriteLine("8.- Send READ SECTORS WITH RETRIES command.");
                AaruConsole.WriteLine("9.- Send SEEK command.");
                AaruConsole.WriteLine("10.- Send SET FEATURES command.");
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
                    case 10:
                        SetFeatures(devPath, dev);

                        continue;
                    default:
                        AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();

                        continue;
                }
            }
        }

        static void Identify(string devPath, Device dev)
        {
            start:
            System.Console.Clear();

            bool sense = dev.AtaIdentify(out byte[] buffer, out AtaErrorRegistersChs errorRegisters,
                                         out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending IDENTIFY DEVICE to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
            AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Print buffer.");
            AaruConsole.WriteLine("2.- Decode buffer.");
            AaruConsole.WriteLine("3.- Decode error registers.");
            AaruConsole.WriteLine("4.- Send command again.");
            AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("IDENTIFY DEVICE response:");

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
                    AaruConsole.WriteLine("IDENTIFY DEVICE decoded response:");

                    if(buffer != null)
                        AaruConsole.WriteLine("{0}", Decoders.ATA.Identify.Prettify(buffer));

                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 3:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("IDENTIFY DEVICE status registers:");
                    AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 4: goto start;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();

                    goto menu;
            }
        }

        static void ReadDma(string devPath, Device dev, bool retries)
        {
            ushort cylinder = 0;
            byte   head     = 0;
            byte   sector   = 1;
            byte   count    = 1;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ DMA {0}command:", retries ? "WITH RETRIES " : "");
                AaruConsole.WriteLine("Cylinder: {0}", cylinder);
                AaruConsole.WriteLine("Head: {0}", head);
                AaruConsole.WriteLine("Sector: {0}", sector);
                AaruConsole.WriteLine("Count: {0}", count);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

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

                            continue;
                        }

                        AaruConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out count))
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

            bool sense = dev.ReadDma(out byte[] buffer, out AtaErrorRegistersChs errorRegisters, retries, cylinder,
                                     head, sector, count, dev.Timeout, out double duration);

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
            AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ DMA {0}response:", retries ? "WITH RETRIES " : "");

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
                    AaruConsole.WriteLine("READ DMA {0}status registers:", retries ? "WITH RETRIES " : "");
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

        static void ReadLong(string devPath, Device dev, bool retries)
        {
            ushort cylinder  = 0;
            byte   head      = 0;
            byte   sector    = 1;
            uint   blockSize = 1;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ LONG {0}command:", retries ? "WITH RETRIES " : "");
                AaruConsole.WriteLine("Cylinder: {0}", cylinder);
                AaruConsole.WriteLine("Head: {0}", head);
                AaruConsole.WriteLine("Sector: {0}", sector);
                AaruConsole.WriteLine("Block size: {0}", blockSize);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

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

                            continue;
                        }

                        AaruConsole.Write("How many bytes to expect?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadLong(out byte[] buffer, out AtaErrorRegistersChs errorRegisters, retries, cylinder,
                                      head, sector, blockSize, dev.Timeout, out double duration);

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
            AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ LONG {0}response:", retries ? "WITH RETRIES " : "");

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
                    AaruConsole.WriteLine("READ LONG {0}status registers:", retries ? "WITH RETRIES " : "");
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

        static void ReadMultiple(string devPath, Device dev)
        {
            ushort cylinder = 0;
            byte   head     = 0;
            byte   sector   = 1;
            byte   count    = 1;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ MULTIPLE command:");
                AaruConsole.WriteLine("Cylinder: {0}", cylinder);
                AaruConsole.WriteLine("Head: {0}", head);
                AaruConsole.WriteLine("Sector: {0}", sector);
                AaruConsole.WriteLine("Count: {0}", count);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

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

                            continue;
                        }

                        AaruConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out count))
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

            bool sense = dev.ReadMultiple(out byte[] buffer, out AtaErrorRegistersChs errorRegisters, cylinder, head,
                                          sector, count, dev.Timeout, out double duration);

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
            AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ MULTIPLE response:");

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
                    AaruConsole.WriteLine("READ MULTIPLE status registers:");
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

        static void ReadSectors(string devPath, Device dev, bool retries)
        {
            ushort cylinder = 0;
            byte   head     = 0;
            byte   sector   = 1;
            byte   count    = 1;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ SECTORS {0}command:", retries ? "WITH RETRIES " : "");
                AaruConsole.WriteLine("Cylinder: {0}", cylinder);
                AaruConsole.WriteLine("Head: {0}", head);
                AaruConsole.WriteLine("Sector: {0}", sector);
                AaruConsole.WriteLine("Count: {0}", count);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

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

                            continue;
                        }

                        AaruConsole.Write("How many sectors?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out count))
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

            bool sense = dev.Read(out byte[] buffer, out AtaErrorRegistersChs errorRegisters, retries, cylinder, head,
                                  sector, count, dev.Timeout, out double duration);

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
            AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ SECTORS {0}response:", retries ? "WITH RETRIES " : "");

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
                    AaruConsole.WriteLine("READ SECTORS {0}status registers:", retries ? "WITH RETRIES " : "");
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

        static void Seek(string devPath, Device dev)
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
                AaruConsole.WriteLine("Parameters for SEEK command:");
                AaruConsole.WriteLine("Cylinder: {0}", cylinder);
                AaruConsole.WriteLine("Head: {0}", head);
                AaruConsole.WriteLine("Sector: {0}", sector);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

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

            bool sense = dev.Seek(out AtaErrorRegistersChs errorRegisters, cylinder, head, sector, dev.Timeout,
                                  out double duration);

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
            AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("SEEK status registers:");
                    AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
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

        static void SetFeatures(string devPath, Device dev)
        {
            ushort cylinder    = 0;
            byte   head        = 0;
            byte   sector      = 0;
            byte   feature     = 0;
            byte   sectorCount = 0;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for SET FEATURES command:");
                AaruConsole.WriteLine("Cylinder: {0}", cylinder);
                AaruConsole.WriteLine("Head: {0}", head);
                AaruConsole.WriteLine("Sector: {0}", sector);
                AaruConsole.WriteLine("Sector count: {0}", sectorCount);
                AaruConsole.WriteLine("Feature: 0x{0:X2}", feature);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");

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
                        AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

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

                        AaruConsole.Write("What sector count?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out sectorCount))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            sectorCount = 0;
                            System.Console.ReadKey();
                        }

                        AaruConsole.Write("What feature?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out feature))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            feature = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.Seek(out AtaErrorRegistersChs errorRegisters, cylinder, head, sector, dev.Timeout,
                                  out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending SET FEATURES to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Decode error registers.");
            AaruConsole.WriteLine("2.- Send command again.");
            AaruConsole.WriteLine("3.- Change parameters.");
            AaruConsole.WriteLine("0.- Return to CHS ATA commands menu.");
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
                    AaruConsole.WriteLine("Returning to CHS ATA commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("SET FEATURES status registers:");
                    AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
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
}