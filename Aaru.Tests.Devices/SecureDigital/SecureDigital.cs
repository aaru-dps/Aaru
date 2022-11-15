// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SecureDigital.cs
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
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SecureDigital;

static class SecureDigital
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Send a SecureDigital command to the device:");
            AaruConsole.WriteLine("1.- Send READ_MULTIPLE_BLOCK command.");
            AaruConsole.WriteLine("2.- Send READ_SINGLE_BLOCK command.");
            AaruConsole.WriteLine("3.- Send SD_SEND_OP_COND command.");
            AaruConsole.WriteLine("4.- Send SD_STATUS command.");
            AaruConsole.WriteLine("5.- Send SEND_CID command.");
            AaruConsole.WriteLine("6.- Send SEND_CSD command.");
            AaruConsole.WriteLine("7.- Send SEND_SCR command.");
            AaruConsole.WriteLine("8.- Send SET_BLOCKLEN command.");
            AaruConsole.WriteLine("0.- Return to SecureDigital/MultiMediaCard commands menu.");
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
                    AaruConsole.WriteLine("Returning to SecureDigital/MultiMediaCard commands menu...");

                    return;
                case 1:
                    Read(devPath, dev, true);

                    continue;
                case 2:
                    Read(devPath, dev, false);

                    continue;
                case 3:
                    SendOpCond(devPath, dev);

                    continue;
                case 4:
                    Status(devPath, dev);

                    continue;
                case 5:
                    SendCid(devPath, dev);

                    continue;
                case 6:
                    SendCsd(devPath, dev);

                    continue;
                case 7:
                    SendScr(devPath, dev);

                    continue;
                case 8:
                    SetBlockLength(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void Read(string devPath, Device dev, bool multiple)
    {
        uint   address   = 0;
        uint   blockSize = 512;
        ushort count     = 1;
        bool   byteAddr  = false;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ_{0}_BLOCK command:", multiple ? "MULTIPLE" : "SINGLE");
            AaruConsole.WriteLine("Read from {1}: {0}", address, byteAddr ? "byte" : "block");
            AaruConsole.WriteLine("Expected block size: {0} bytes", blockSize);

            if(multiple)
                AaruConsole.WriteLine("Will read {0} blocks", count);

            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to SecureDigital commands menu.");

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
                    AaruConsole.WriteLine("Returning to SecureDigital commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Use byte addressing?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out byteAddr))
                    {
                        AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                        byteAddr = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Read from {0}?: ", byteAddr ? "byte" : "block");
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        address = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    if(multiple)
                    {
                        AaruConsole.Write("How many blocks to read?");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();

                            continue;
                        }
                    }

                    AaruConsole.Write("How many bytes to expect in a block?");
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out blockSize))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        blockSize = 512;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.Read(out byte[] buffer, out uint[] response, address, blockSize, multiple ? count : (ushort)1,
                              byteAddr, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ_{0}_BLOCK to the device:", multiple ? "MULTIPLE" : "SINGLE");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Response has {0} elements.", response?.Length.ToString() ?? "null");
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Print response buffer.");
        AaruConsole.WriteLine("3.- Send command again.");
        AaruConsole.WriteLine("4.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to SecureDigital commands menu.");
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
                AaruConsole.WriteLine("Returning to SecureDigital commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ_{0}_BLOCK buffer:", multiple ? "MULTIPLE" : "SINGLE");

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
                AaruConsole.WriteLine("READ_{0}_BLOCK response:", multiple ? "MULTIPLE" : "SINGLE");

                if(response != null)
                {
                    foreach(uint res in response)
                        AaruConsole.Write("0x{0:x8} ", res);

                    AaruConsole.WriteLine();
                }

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

    static void SendOpCond(string devPath, Device dev)
    {
        start:
        System.Console.Clear();
        bool sense = dev.ReadSdocr(out byte[] buffer, out uint[] response, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending SD_SEND_OP_COND to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Response has {0} elements.", response?.Length.ToString() ?? "null");
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode buffer.");
        AaruConsole.WriteLine("3.- Print response buffer.");
        AaruConsole.WriteLine("4.- Send command again.");
        AaruConsole.WriteLine("0.- Return to SecureDigital commands menu.");
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
                AaruConsole.WriteLine("Returning to SecureDigital commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("SD_SEND_OP_COND buffer:");

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
                AaruConsole.WriteLine("SD_SEND_OP_COND decoded buffer:");

                if(buffer != null)
                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyOCR(buffer));

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("SD_SEND_OP_COND response:");

                if(response != null)
                {
                    foreach(uint res in response)
                        AaruConsole.Write("0x{0:x8} ", res);

                    AaruConsole.WriteLine();
                }

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

    static void Status(string devPath, Device dev)
    {
        start:
        System.Console.Clear();
        bool sense = dev.ReadSdStatus(out byte[] buffer, out uint[] response, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending SD_STATUS to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Response has {0} elements.", response?.Length.ToString() ?? "null");
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Print response buffer.");
        AaruConsole.WriteLine("3.- Send command again.");
        AaruConsole.WriteLine("0.- Return to SecureDigital commands menu.");
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
                AaruConsole.WriteLine("Returning to SecureDigital commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("SD_STATUS buffer:");

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
                AaruConsole.WriteLine("SD_STATUS response:");

                if(response != null)
                {
                    foreach(uint res in response)
                        AaruConsole.Write("0x{0:x8} ", res);

                    AaruConsole.WriteLine();
                }

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void SendCid(string devPath, Device dev)
    {
        start:
        System.Console.Clear();
        bool sense = dev.ReadCid(out byte[] buffer, out uint[] response, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending SEND_CID to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Response has {0} elements.", response?.Length.ToString() ?? "null");
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode buffer.");
        AaruConsole.WriteLine("3.- Print response buffer.");
        AaruConsole.WriteLine("4.- Send command again.");
        AaruConsole.WriteLine("0.- Return to SecureDigital commands menu.");
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
                AaruConsole.WriteLine("Returning to SecureDigital commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("SEND_CID buffer:");

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
                AaruConsole.WriteLine("SEND_CID decoded buffer:");

                if(buffer != null)
                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyCID(buffer));

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("SEND_CID response:");

                if(response != null)
                {
                    foreach(uint res in response)
                        AaruConsole.Write("0x{0:x8} ", res);

                    AaruConsole.WriteLine();
                }

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

    static void SendCsd(string devPath, Device dev)
    {
        start:
        System.Console.Clear();
        bool sense = dev.ReadCsd(out byte[] buffer, out uint[] response, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending SEND_CSD to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Response has {0} elements.", response?.Length.ToString() ?? "null");
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode buffer.");
        AaruConsole.WriteLine("3.- Print response buffer.");
        AaruConsole.WriteLine("4.- Send command again.");
        AaruConsole.WriteLine("0.- Return to SecureDigital commands menu.");
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
                AaruConsole.WriteLine("Returning to SecureDigital commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("SEND_CSD buffer:");

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
                AaruConsole.WriteLine("SEND_CSD decoded buffer:");

                if(buffer != null)
                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifyCSD(buffer));

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("SEND_CSD response:");

                if(response != null)
                {
                    foreach(uint res in response)
                        AaruConsole.Write("0x{0:x8} ", res);

                    AaruConsole.WriteLine();
                }

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

    static void SendScr(string devPath, Device dev)
    {
        start:
        System.Console.Clear();
        bool sense = dev.ReadScr(out byte[] buffer, out uint[] response, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending SEND_SCR to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Response has {0} elements.", response?.Length.ToString() ?? "null");
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode buffer.");
        AaruConsole.WriteLine("3.- Print response buffer.");
        AaruConsole.WriteLine("4.- Send command again.");
        AaruConsole.WriteLine("0.- Return to SecureDigital commands menu.");
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
                AaruConsole.WriteLine("Returning to SecureDigital commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("SEND_SCR buffer:");

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
                AaruConsole.WriteLine("SEND_SCR decoded buffer:");

                if(buffer != null)
                    AaruConsole.WriteLine("{0}", Decoders.SecureDigital.Decoders.PrettifySCR(buffer));

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("SEND_SCR response:");

                if(response != null)
                {
                    foreach(uint res in response)
                        AaruConsole.Write("0x{0:x8} ", res);

                    AaruConsole.WriteLine();
                }

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

    static void SetBlockLength(string devPath, Device dev)
    {
        uint   blockSize = 512;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for SET_BLOCKLEN command:");
            AaruConsole.WriteLine("Set block length to: {0} bytes", blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to SecureDigital commands menu.");

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
                    AaruConsole.WriteLine("Returning to SecureDigital commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Set block length to?");
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out blockSize))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        blockSize = 512;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();
        bool sense = dev.SetBlockLength(blockSize, out uint[] response, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending SET_BLOCKLEN to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Response has {0} elements.", response?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("SET_BLOCKLEN response:");

        if(response != null)
        {
            foreach(uint res in response)
                AaruConsole.Write("0x{0:x8} ", res);

            AaruConsole.WriteLine();
        }

        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Send command again.");
        AaruConsole.WriteLine("2.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to SecureDigital commands menu.");
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
                AaruConsole.WriteLine("Returning to SecureDigital commands menu...");

                return;
            case 1: goto start;
            case 2: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}