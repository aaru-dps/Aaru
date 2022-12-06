// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Plasmon.cs
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
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SCSI
{
    internal static class Plasmon
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Send a Plasmon vendor command to the device:");
                AaruConsole.WriteLine("1.- Send READ LONG command.");
                AaruConsole.WriteLine("2.- Send READ SECTOR LOCATION command.");
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
                        ReadLong(devPath, dev);

                        continue;
                    case 2:
                        ReadSectorLocation(devPath, dev);

                        continue;
                    default:
                        AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();

                        continue;
                }
            }
        }

        static void ReadLong(string devPath, Device dev)
        {
            bool   relative    = false;
            uint   address     = 0;
            ushort length      = 1;
            ushort bps         = 512;
            bool   physical    = false;
            bool   sectorCount = true;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ LONG command:");
                AaruConsole.WriteLine("{0} Block Address: {1}", physical ? "Physical" : "Logical", address);
                AaruConsole.WriteLine("Relative?: {0}", relative);
                AaruConsole.WriteLine("Will transfer {0} {1}", length, sectorCount ? "sectors" : "bytes");

                if(sectorCount)
                    AaruConsole.WriteLine("Expected sector size: {0} bytes", bps);

                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to Plasmon vendor commands menu.");

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
                        AaruConsole.WriteLine("Returning to Plasmon vendor commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Physical address?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out physical))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            physical = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Relative address?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out relative))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            relative = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("{0} Block Address?: ", physical ? "Physical" : "Logical");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out address))
                        {
                            AaruConsole.WriteLine("Not a numbr. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Transfer sectors?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out sectorCount))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sectorCount = true;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How many {0} to transfer?: ", sectorCount ? "sectors" : "bytes");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out length))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            length = (ushort)(sectorCount ? 1 : 512);
                            System.Console.ReadKey();

                            continue;
                        }

                        if(sectorCount)
                        {
                            AaruConsole.Write("How many bytes to expect per sector?");
                            strDev = System.Console.ReadLine();

                            if(!ushort.TryParse(strDev, out bps))
                            {
                                AaruConsole.WriteLine("Not a numbr. Press any key to continue...");
                                bps = 512;
                                System.Console.ReadKey();
                            }
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.PlasmonReadLong(out byte[] buffer, out byte[] senseBuffer, relative, address, length, bps,
                                             physical, sectorCount, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ LONG to the device:");
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
            AaruConsole.WriteLine("0.- Return to Plasmon vendor commands menu.");
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
                    AaruConsole.WriteLine("Returning to Plasmon vendor commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ LONG response:");

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
                    AaruConsole.WriteLine("READ LONG sense:");

                    if(senseBuffer != null)
                        PrintHex.PrintHexArray(senseBuffer, 64);

                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 3:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ LONG decoded sense:");
                    AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 4: goto start;
                case 5: goto parameters;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();

                    goto menu;
            }
        }

        static void ReadSectorLocation(string devPath, Device dev)
        {
            uint   address  = 0;
            bool   physical = false;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ SECTOR LOCATION command:");
                AaruConsole.WriteLine("{0} Block Address: {1}", physical ? "Physical" : "Logical", address);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to Plasmon vendor commands menu.");

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
                        AaruConsole.WriteLine("Returning to Plasmon vendor commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Physical address?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out physical))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            physical = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("{0} Block Address?: ", physical ? "Physical" : "Logical");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out address))
                        {
                            AaruConsole.WriteLine("Not a numbr. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.PlasmonReadSectorLocation(out byte[] buffer, out byte[] senseBuffer, address, physical,
                                                       dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ SECTOR LOCATION to the device:");
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
            AaruConsole.WriteLine("0.- Return to Plasmon vendor commands menu.");
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
                    AaruConsole.WriteLine("Returning to Plasmon vendor commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ SECTOR LOCATION response:");

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
                    AaruConsole.WriteLine("READ SECTOR LOCATION sense:");

                    if(senseBuffer != null)
                        PrintHex.PrintHexArray(senseBuffer, 64);

                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 3:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ SECTOR LOCATION decoded sense:");
                    AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 4: goto start;
                case 5: goto parameters;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();

                    goto menu;
            }
        }
    }
}