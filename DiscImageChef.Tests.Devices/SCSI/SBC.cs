// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SBC.cs
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
    public static class SBC
    {
        public static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a SCSI Block Command to the device:");
                DicConsole.WriteLine("1.- Send READ (6) command.");
                DicConsole.WriteLine("2.- Send READ (10) command.");
                DicConsole.WriteLine("3.- Send READ (12) command.");
                DicConsole.WriteLine("4.- Send READ (16) command.");
                DicConsole.WriteLine("5.- Send READ LONG (10) command.");
                DicConsole.WriteLine("6.- Send READ LONG (16) command.");
                DicConsole.WriteLine("7.- Send SEEK (6) command.");
                DicConsole.WriteLine("8.- Send SEEK (10) command.");
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
                        Read6(devPath, dev);
                        continue;
                    case 2:
                        Read10(devPath, dev);
                        continue;
                    case 3:
                        Read12(devPath, dev);
                        continue;
                    case 4:
                        Read16(devPath, dev);
                        continue;
                    case 5:
                        ReadLong10(devPath, dev);
                        continue;
                    case 6:
                        ReadLong16(devPath, dev);
                        continue;
                    case 7:
                        Seek6(devPath, dev);
                        continue;
                    case 8:
                        Seek10(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void Read6(string devPath, Device dev)
        {
            uint lba = 0;
            uint blockSize = 512;
            byte count = 1;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ (6) command:");
                DicConsole.WriteLine("LBA: {0}", lba);
                DicConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("LBA?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out lba))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        if(lba > 0x1FFFFF)
                        {
                            DicConsole.WriteLine("Max LBA is {0}, setting to {0}", 0x1FFFFF);
                            lba = 0x1FFFFF;
                        }
                        DicConsole.Write("Blocks to read (0 for 256 blocks)?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
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
            bool sense = dev.Read6(out byte[] buffer, out byte[] senseBuffer, lba, blockSize, count, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ (6) to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ (6) response:");
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
                    DicConsole.WriteLine("READ (6) sense:");
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
                    DicConsole.WriteLine("READ (6) decoded sense:");
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

        static void Read10(string devPath, Device dev)
        {
            uint lba = 0;
            uint blockSize = 512;
            byte count = 1;
            byte rdprotect = 0;
            bool dpo = false;
            bool fua = false;
            bool fuaNv = false;
            byte groupNumber = 0;
            bool relative = false;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ (10) command:");
                DicConsole.WriteLine("Address relative to current position?: {0}", relative);
                DicConsole.WriteLine("{1}: {0}", lba, relative ? "Address" : "LBA");
                DicConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine("How to check protection information: {0}", rdprotect);
                DicConsole.WriteLine("Give lowest cache priority?: {0}", dpo);
                DicConsole.WriteLine("Force bypassing cache and reading from medium?: {0}", fua);
                DicConsole.WriteLine("Force bypassing cache and reading from non-volatile cache?: {0}", fuaNv);
                DicConsole.WriteLine("Group number: {0}", groupNumber);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Address relative to current position?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out relative))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("{0}?: ", relative ? "Address" : "LBA");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out lba))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Blocks to read (0 for 256 blocks)?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How to check protection information?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out rdprotect))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Give lowest cache priority?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out dpo))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Force bypassing cache and reading from medium?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fua))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Force bypassing cache and reading from non-volatile cache?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fuaNv))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Group number?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
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
            bool sense = dev.Read10(out byte[] buffer, out byte[] senseBuffer, rdprotect, dpo, fua, fuaNv, relative, lba, blockSize, groupNumber, count, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ (10) to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ (10) response:");
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
                    DicConsole.WriteLine("READ (10) sense:");
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
                    DicConsole.WriteLine("READ (10) decoded sense:");
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

        static void Read12(string devPath, Device dev)
        {
            uint lba = 0;
            uint blockSize = 512;
            byte count = 1;
            byte rdprotect = 0;
            bool dpo = false;
            bool fua = false;
            bool fuaNv = false;
            byte groupNumber = 0;
            bool relative = false;
            bool streaming = false;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ (12) command:");
                DicConsole.WriteLine("Address relative to current position?: {0}", relative);
                DicConsole.WriteLine("{1}: {0}", lba, relative ? "Address" : "LBA");
                DicConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine("How to check protection information: {0}", rdprotect);
                DicConsole.WriteLine("Give lowest cache priority?: {0}", dpo);
                DicConsole.WriteLine("Force bypassing cache and reading from medium?: {0}", fua);
                DicConsole.WriteLine("Force bypassing cache and reading from non-volatile cache?: {0}", fuaNv);
                DicConsole.WriteLine("Group number: {0}", groupNumber);
                DicConsole.WriteLine("Use streaming?: {0}", streaming);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Address relative to current position?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out relative))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("{0}?: ", relative ? "Address" : "LBA");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out lba))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Blocks to read (0 for 256 blocks)?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How to check protection information?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out rdprotect))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Give lowest cache priority?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out dpo))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Force bypassing cache and reading from medium?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fua))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Force bypassing cache and reading from non-volatile cache?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fuaNv))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Group number?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Use streaming?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out streaming))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
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
            bool sense = dev.Read12(out byte[] buffer, out byte[] senseBuffer, rdprotect, dpo, fua, fuaNv, relative, lba, blockSize, groupNumber, count, streaming, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ (12) to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ (12) response:");
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
                    DicConsole.WriteLine("READ (12) sense:");
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
                    DicConsole.WriteLine("READ (12) decoded sense:");
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

        static void Read16(string devPath, Device dev)
        {
            ulong lba = 0;
            uint blockSize = 512;
            byte count = 1;
            byte rdprotect = 0;
            bool dpo = false;
            bool fua = false;
            bool fuaNv = false;
            byte groupNumber = 0;
            bool relative = false;
            bool streaming = false;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ (16) command:");
                DicConsole.WriteLine("LBA: {0}", lba);
                DicConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine("How to check protection information: {0}", rdprotect);
                DicConsole.WriteLine("Give lowest cache priority?: {0}", dpo);
                DicConsole.WriteLine("Force bypassing cache and reading from medium?: {0}", fua);
                DicConsole.WriteLine("Force bypassing cache and reading from non-volatile cache?: {0}", fuaNv);
                DicConsole.WriteLine("Group number: {0}", groupNumber);
                DicConsole.WriteLine("Use streaming?: {0}", streaming);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("LBA?: ");
                        strDev = System.Console.ReadLine();
                        if(!ulong.TryParse(strDev, out lba))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Blocks to read (0 for 256 blocks)?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How to check protection information?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out rdprotect))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Give lowest cache priority?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out dpo))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Force bypassing cache and reading from medium?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fua))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Force bypassing cache and reading from non-volatile cache?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fuaNv))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Group number?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Use streaming?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out streaming))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
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
            bool sense = dev.Read16(out byte[] buffer, out byte[] senseBuffer, rdprotect, dpo, fua, fuaNv, lba, blockSize, groupNumber, count, streaming, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ (16) to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ (16) response:");
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
                    DicConsole.WriteLine("READ (16) sense:");
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
                    DicConsole.WriteLine("READ (16) decoded sense:");
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

        static void ReadLong10(string devPath, Device dev)
        {
            uint lba = 0;
            ushort blockSize = 512;
            bool correct = false;
            bool relative = false;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ LONG (10) command:");
                DicConsole.WriteLine("Address relative to current position?: {0}", relative);
                DicConsole.WriteLine("{1}: {0}", lba, relative ? "Address" : "LBA");
                DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine("Try to error correct block?: {0}", correct);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Address relative to current position?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out relative))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("{0}?: ", relative ? "Address" : "LBA");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out lba))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();
                        if(!ushort.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Try to error correct block?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out correct))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
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
            bool sense = dev.ReadLong10(out byte[] buffer, out byte[] senseBuffer, correct, relative, lba, blockSize, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ LONG (10) to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ LONG (10) response:");
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
                    DicConsole.WriteLine("READ LONG (10) sense:");
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
                    DicConsole.WriteLine("READ LONG (10) decoded sense:");
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

        static void ReadLong16(string devPath, Device dev)
        {
            ulong lba = 0;
            uint blockSize = 512;
            bool correct = false;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ LONG (16) command:");
                DicConsole.WriteLine("LBA: {0}", lba);
                DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine("Try to error correct block?: {0}", correct);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("LBA?: ");
                        strDev = System.Console.ReadLine();
                        if(!ulong.TryParse(strDev, out lba))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();
                            continue;
                        }
                        DicConsole.Write("Try to error correct block?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out correct))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
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
            bool sense = dev.ReadLong16(out byte[] buffer, out byte[] senseBuffer, correct, lba, blockSize, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ LONG (16) to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ LONG (16) response:");
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
                    DicConsole.WriteLine("READ LONG (16) sense:");
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
                    DicConsole.WriteLine("READ LONG (16) decoded sense:");
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

        static void Seek6(string devPath, Device dev)
        {
            uint lba = 0;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for SEEK (6) command:");
                DicConsole.WriteLine("Descriptor: {0}", lba);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("LBA?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out lba))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                            continue;
                        }
                        if(lba > 0x1FFFFF)
                        {
                            DicConsole.WriteLine("Max LBA is {0}, setting to {0}", 0x1FFFFF);
                            lba = 0x1FFFFF;
                        }
                        break;
                    case 2:
                        goto start;
                }
            }

        start:
            System.Console.Clear();
            bool sense = dev.Seek6(out byte[] senseBuffer, lba, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SEEK (6) to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("SEEK (6) decoded sense:");
            DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEEK (6) sense:");
                    if(senseBuffer != null)
                        PrintHex.PrintHexArray(senseBuffer, 64);
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

        static void Seek10(string devPath, Device dev)
        {
            uint lba = 0;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for SEEK (10) command:");
                DicConsole.WriteLine("Descriptor: {0}", lba);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Descriptor?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out lba))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
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
            bool sense = dev.Seek10(out byte[] senseBuffer, lba, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SEEK (10) to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("SEEK (6) decoded sense:");
            DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Block Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SEEK (10) sense:");
                    if(senseBuffer != null)
                        PrintHex.PrintHexArray(senseBuffer, 64);
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
