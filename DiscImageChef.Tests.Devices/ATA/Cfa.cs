// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Cfa.cs
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
    public static class Cfa
    {
        public static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a CompactFlash command to the device:");
                DicConsole.WriteLine("1.- Send REQUEST EXTENDED ERROR CODE command.");
                DicConsole.WriteLine("2.- Send CHS TRANSLATE SECTOR command.");
                DicConsole.WriteLine("3.- Send LBA TRANSLATE SECTOR command.");
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
                        RequestExtendedErrorCode(devPath, dev);
                        continue;
                    case 2:
                        TranslateSectorChs(devPath, dev);
                        continue;
                    case 3:
                        TranslateSectorLba(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void RequestExtendedErrorCode(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.RequestExtendedErrorCode(out byte errorCode, out AtaErrorRegistersLBA28 errorRegisters,
                                                      dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending REQUEST EXTENDED ERROR CODE to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Error code is {0}.", errorCode);
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Decode error registers.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("0.- Return to CompactFlash commands menu.");
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
                    DicConsole.WriteLine("Returning to CompactFlash commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("REQUEST EXTENDED ERROR CODE status registers:");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2: goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void TranslateSectorChs(string devPath, Device dev)
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
                DicConsole.WriteLine("Parameters for TRANSLATE SECTOR command:");
                DicConsole.WriteLine("Cylinder: {0}", cylinder);
                DicConsole.WriteLine("Head: {0}", head);
                DicConsole.WriteLine("Sector: {0}", sector);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to CompactFlash commands menu.");

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
                        DicConsole.WriteLine("Returning to CompactFlash commands menu...");
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
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.TranslateSector(out byte[] buffer, out AtaErrorRegistersCHS errorRegisters, cylinder, head,
                                             sector, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending TRANSLATE SECTOR to the device:");
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
            DicConsole.WriteLine("0.- Return to CompactFlash commands menu.");
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
                    DicConsole.WriteLine("Returning to CompactFlash commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("TRANSLATE SECTOR response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("TRANSLATE SECTOR status registers:");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3: goto start;
                case 4: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void TranslateSectorLba(string devPath, Device dev)
        {
            uint lba = 0;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for TRANSLATE SECTOR command:");
                DicConsole.WriteLine("LBA: {0}", lba);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to CompactFlash commands menu.");

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
                        DicConsole.WriteLine("Returning to CompactFlash commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("What logical block address?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out lba))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        if(lba > 0xFFFFFFF)
                        {
                            DicConsole
                                .WriteLine("Logical block address cannot be bigger than {0}. Setting it to {0}...",
                                           0xFFFFFFF);
                            lba = 0xFFFFFFF;
                        }
                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.TranslateSector(out byte[] buffer, out AtaErrorRegistersLBA28 errorRegisters, lba,
                                             dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending TRANSLATE SECTOR to the device:");
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
            DicConsole.WriteLine("0.- Return to CompactFlash commands menu.");
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
                    DicConsole.WriteLine("Returning to CompactFlash commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("TRANSLATE SECTOR response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("TRANSLATE SECTOR status registers:");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3: goto start;
                case 4: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }
    }
}