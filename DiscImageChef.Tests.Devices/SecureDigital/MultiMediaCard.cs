// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MultiMediaCard.cs
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
using DiscImageChef.Devices;

namespace DiscImageChef.Tests.Devices.SecureDigital
{
    static class MultiMediaCard
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a MultiMediaCard command to the device:");
                DicConsole.WriteLine("1.- Send READ_MULTIPLE_BLOCK command.");
                DicConsole.WriteLine("2.- Send READ_SINGLE_BLOCK command.");
                DicConsole.WriteLine("3.- Send SEND_CID command.");
                DicConsole.WriteLine("4.- Send SEND_CSD command.");
                DicConsole.WriteLine("5.- Send SEND_EXT_CSD command.");
                DicConsole.WriteLine("6.- Send SEND_OP_COND command.");
                DicConsole.WriteLine("7.- Send SEND_STATUS command.");
                DicConsole.WriteLine("8.- Send SET_BLOCKLEN command.");
                DicConsole.WriteLine("0.- Return to SecureDigital/MultiMediaCard commands menu.");
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
                        DicConsole.WriteLine("Returning to SecureDigital/MultiMediaCard commands menu...");
                        return;
                    case 1:
                        Read(devPath, dev, true);
                        continue;
                    case 2:
                        Read(devPath, dev, false);
                        continue;
                    case 3:
                        SendCID(devPath, dev);
                        continue;
                    case 4:
                        SendCSD(devPath, dev);
                        continue;
                    case 5:
                        SendExtendedCSD(devPath, dev);
                        continue;
                    case 6:
                        SendOpCond(devPath, dev);
                        continue;
                    case 7:
                        Status(devPath, dev);
                        continue;
                    case 8:
                        SetBlockLength(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void Read(string devPath, Device dev, bool multiple)
        {
            uint address = 0;
            uint blockSize = 512;
            uint count = 1;
            bool byteAddr = false;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ_{0}_BLOCK command:", multiple ? "MULTIPLE" : "SINGLE");
                DicConsole.WriteLine("Read from {1}: {0}", address, byteAddr ? "byte" : "block");
                DicConsole.WriteLine("Expected block size: {0} bytes", blockSize);
                if(multiple) DicConsole.WriteLine("Will read {0} blocks", count);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to MultiMediaCard commands menu.");

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
                        DicConsole.WriteLine("Returning to MultiMediaCard commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Use byte addressing?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out byteAddr))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            byteAddr = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Read from {0}?: ", byteAddr ? "byte" : "block");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out address))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        if(multiple)
                        {
                            DicConsole.Write("How many blocks to read?");
                            strDev = System.Console.ReadLine();
                            if(!uint.TryParse(strDev, out count))
                            {
                                DicConsole.WriteLine("Not a number. Press any key to continue...");
                                count = 1;
                                System.Console.ReadKey();
                                continue;
                            }
                        }

                        DicConsole.Write("How many bytes to expect in a block?");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();
                            continue;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.Read(out byte[] buffer, out uint[] response, address, blockSize, multiple ? count : 1,
                                  byteAddr, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ_{0}_BLOCK to the device:", multiple ? "MULTIPLE" : "SINGLE");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Response has {0} elements.", response == null ? "null" : response.Length.ToString());
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print response buffer.");
            DicConsole.WriteLine("3.- Send command again.");
            DicConsole.WriteLine("4.- Change parameters.");
            DicConsole.WriteLine("0.- Return to MultiMediaCard commands menu.");
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
                    DicConsole.WriteLine("Returning to MultiMediaCard commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ_{0}_BLOCK buffer:", multiple ? "MULTIPLE" : "SINGLE");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ_{0}_BLOCK response:", multiple ? "MULTIPLE" : "SINGLE");
                    if(response != null)
                    {
                        foreach(uint res in response) DicConsole.Write("0x{0:x8} ", res);

                        DicConsole.WriteLine();
                    }

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

        static void SendOpCond(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.ReadOCR(out byte[] buffer, out uint[] response, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SEND_OP_COND to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Response has {0} elements.", response == null ? "null" : response.Length.ToString());
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode buffer.");
            DicConsole.WriteLine("3.- Print response buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("0.- Return to MultiMediaCard commands menu.");
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
                    DicConsole.WriteLine("Returning to MultiMediaCard commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_OP_COND buffer:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_OP_COND decoded buffer:");
                    if(buffer != null) DicConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyOCR(buffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_OP_COND response:");
                    if(response != null)
                    {
                        foreach(uint res in response) DicConsole.Write("0x{0:x8} ", res);

                        DicConsole.WriteLine();
                    }

                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4: goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void Status(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.ReadSDStatus(out byte[] buffer, out uint[] response, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SEND_STATUS to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Response has {0} elements.", response == null ? "null" : response.Length.ToString());
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print response buffer.");
            DicConsole.WriteLine("3.- Send command again.");
            DicConsole.WriteLine("0.- Return to MultiMediaCard commands menu.");
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
                    DicConsole.WriteLine("Returning to MultiMediaCard commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_STATUS buffer:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_STATUS response:");
                    if(response != null)
                    {
                        foreach(uint res in response) DicConsole.Write("0x{0:x8} ", res);

                        DicConsole.WriteLine();
                    }

                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3: goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void SendCID(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.ReadCID(out byte[] buffer, out uint[] response, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SEND_CID to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Response has {0} elements.", response == null ? "null" : response.Length.ToString());
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode buffer.");
            DicConsole.WriteLine("3.- Print response buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("0.- Return to MultiMediaCard commands menu.");
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
                    DicConsole.WriteLine("Returning to MultiMediaCard commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_CID buffer:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_CID decoded buffer:");
                    if(buffer != null) DicConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyCID(buffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_CID response:");
                    if(response != null)
                    {
                        foreach(uint res in response) DicConsole.Write("0x{0:x8} ", res);

                        DicConsole.WriteLine();
                    }

                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4: goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void SendCSD(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.ReadCSD(out byte[] buffer, out uint[] response, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SEND_CSD to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Response has {0} elements.", response == null ? "null" : response.Length.ToString());
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode buffer.");
            DicConsole.WriteLine("3.- Print response buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("0.- Return to MultiMediaCard commands menu.");
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
                    DicConsole.WriteLine("Returning to MultiMediaCard commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_CSD buffer:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_CSD decoded buffer:");
                    if(buffer != null) DicConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyCSD(buffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_CSD response:");
                    if(response != null)
                    {
                        foreach(uint res in response) DicConsole.Write("0x{0:x8} ", res);

                        DicConsole.WriteLine();
                    }

                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4: goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void SendExtendedCSD(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.ReadExtendedCSD(out byte[] buffer, out uint[] response, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SEND_EXT_CSD to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Response has {0} elements.", response == null ? "null" : response.Length.ToString());
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode buffer.");
            DicConsole.WriteLine("3.- Print response buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("0.- Return to MultiMediaCard commands menu.");
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
                    DicConsole.WriteLine("Returning to MultiMediaCard commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_EXT_CSD buffer:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_EXT_CSD decoded buffer:");
                    if(buffer != null) DicConsole.WriteLine("{0}", Decoders.MMC.Decoders.PrettifyExtendedCSD(buffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEND_EXT_CSD response:");
                    if(response != null)
                    {
                        foreach(uint res in response) DicConsole.Write("0x{0:x8} ", res);

                        DicConsole.WriteLine();
                    }

                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4: goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void SetBlockLength(string devPath, Device dev)
        {
            uint blockSize = 512;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for SET_BLOCKLEN command:");
                DicConsole.WriteLine("Set block length to: {0} bytes", blockSize);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to MultiMediaCard commands menu.");

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
                        DicConsole.WriteLine("Returning to MultiMediaCard commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Set block length to?");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();
                            continue;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.SetBlockLength(blockSize, out uint[] response, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SET_BLOCKLEN to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Response has {0} elements.", response == null ? "null" : response.Length.ToString());
            DicConsole.WriteLine("SET_BLOCKLEN response:");
            if(response != null)
            {
                foreach(uint res in response) DicConsole.Write("0x{0:x8} ", res);

                DicConsole.WriteLine();
            }

            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Send command again.");
            DicConsole.WriteLine("2.- Change parameters.");
            DicConsole.WriteLine("0.- Return to MultiMediaCard commands menu.");
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
                    DicConsole.WriteLine("Returning to MultiMediaCard commands menu...");
                    return;
                case 1: goto start;
                case 2: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }
    }
}