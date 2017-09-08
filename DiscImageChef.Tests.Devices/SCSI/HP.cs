// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HP.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using DiscImageChef.Console;
using DiscImageChef.Devices;

namespace DiscImageChef.Tests.Devices.SCSI
{
    public static class HP
    {
        public static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a Hewlett-Packard vendor command to the device:");
                DicConsole.WriteLine("1.- Send READ LONG command.");
                DicConsole.WriteLine("0.- Return to SCSI commands menu.");
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
                        DicConsole.WriteLine("Returning to SCSI commands menu...");
                        return;
                    case 1:
                        ReadLong(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void ReadLong(string devPath, Device dev)
        {
            bool relative = false;
            uint address = 0;
            ushort length = 1;
            ushort bps = 512;
            bool physical = false;
            bool sectorCount = true;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ LONG command:");
                DicConsole.WriteLine("{0} Block Address: {1}", physical ? "Physical" : "Logical", address);
                DicConsole.WriteLine("Relative?: {0}", relative);
                DicConsole.WriteLine("Will transfer {0} {1}", length, sectorCount ? "sectors" : "bytes");
                if(sectorCount)
                    DicConsole.WriteLine("Expected sector size: {0} bytes", bps);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Hewlett-Packard vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to Hewlett-Packard vendor commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Physical address?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out physical))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            physical = false;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Relative address?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out relative))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            relative = false;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("{0} Block Address?: ", physical ? "Physical" : "Logical");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out address))
                        {
                            DicConsole.WriteLine("Not a numbr. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Transfer sectors?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out sectorCount))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sectorCount = true;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many {0} to transfer?: ", sectorCount ? "sectors" : "bytes");
                        strDev = System.Console.ReadLine();
                        if(!ushort.TryParse(strDev, out length))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            length = (ushort)(sectorCount ? 1 : 512);
                            System.Console.ReadKey();
                            continue;
                        }
                        if(sectorCount)
                        {
                            DicConsole.Write("How many bytes to expect per sector?");
                            strDev = System.Console.ReadLine();
                            if(!ushort.TryParse(strDev, out bps))
                            {
                                DicConsole.WriteLine("Not a numbr. Press any key to continue...");
                                bps = 512;
                                System.Console.ReadKey();
                                continue;
                            }
                        }
                        break;
                    case 2:
                        goto start;
                }
            }

        start:
            System.Console.Clear();
            bool sense = dev.HPReadLong(out byte[] buffer, out byte[] senseBuffer, relative, address, length, bps, physical, sectorCount, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ LONG to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("5.- Change parameters.");
            DicConsole.WriteLine("0.- Return to Hewlett-Packard vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Hewlett-Packard vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ LONG response:");
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
                    DicConsole.WriteLine("READ LONG sense:");
                    if(senseBuffer != null)
                        PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ LONG decoded sense:");
                    DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    goto start;
                case 5:
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
