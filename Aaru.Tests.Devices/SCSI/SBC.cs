// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SBC.cs
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
    internal static class Sbc
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Send a SCSI Block Command to the device:");
                AaruConsole.WriteLine("1.- Send READ (6) command.");
                AaruConsole.WriteLine("2.- Send READ (10) command.");
                AaruConsole.WriteLine("3.- Send READ (12) command.");
                AaruConsole.WriteLine("4.- Send READ (16) command.");
                AaruConsole.WriteLine("5.- Send READ LONG (10) command.");
                AaruConsole.WriteLine("6.- Send READ LONG (16) command.");
                AaruConsole.WriteLine("7.- Send SEEK (6) command.");
                AaruConsole.WriteLine("8.- Send SEEK (10) command.");
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
                        AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();

                        continue;
                }
            }
        }

        static void Read6(string devPath, Device dev)
        {
            uint   lba       = 0;
            uint   blockSize = 512;
            byte   count     = 1;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ (6) command:");
                AaruConsole.WriteLine("LBA: {0}", lba);
                AaruConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                AaruConsole.WriteLine("{0} bytes expected per block", blockSize);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("LBA?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        if(lba > 0x1FFFFF)
                        {
                            AaruConsole.WriteLine("Max LBA is {0}, setting to {0}", 0x1FFFFF);
                            lba = 0x1FFFFF;
                        }

                        AaruConsole.Write("Blocks to read (0 for 256 blocks)?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How many bytes to expect per block?: ");
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

            bool sense = dev.Read6(out byte[] buffer, out byte[] senseBuffer, lba, blockSize, count, dev.Timeout,
                                   out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ (6) to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ (6) response:");

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
                    AaruConsole.WriteLine("READ (6) sense:");

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
                    AaruConsole.WriteLine("READ (6) decoded sense:");
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

        static void Read10(string devPath, Device dev)
        {
            uint   lba         = 0;
            uint   blockSize   = 512;
            byte   count       = 1;
            byte   rdprotect   = 0;
            bool   dpo         = false;
            bool   fua         = false;
            bool   fuaNv       = false;
            byte   groupNumber = 0;
            bool   relative    = false;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ (10) command:");
                AaruConsole.WriteLine("Address relative to current position?: {0}", relative);
                AaruConsole.WriteLine("{1}: {0}", lba, relative ? "Address" : "LBA");
                AaruConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                AaruConsole.WriteLine("{0} bytes expected per block", blockSize);
                AaruConsole.WriteLine("How to check protection information: {0}", rdprotect);
                AaruConsole.WriteLine("Give lowest cache priority?: {0}", dpo);
                AaruConsole.WriteLine("Force bypassing cache and reading from medium?: {0}", fua);
                AaruConsole.WriteLine("Force bypassing cache and reading from non-volatile cache?: {0}", fuaNv);
                AaruConsole.WriteLine("Group number: {0}", groupNumber);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Address relative to current position?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out relative))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("{0}?: ", relative ? "Address" : "LBA");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Blocks to read (0 for 256 blocks)?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How to check protection information?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out rdprotect))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Give lowest cache priority?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out dpo))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Force bypassing cache and reading from medium?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out fua))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Force bypassing cache and reading from non-volatile cache?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out fuaNv))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Group number?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.Read10(out byte[] buffer, out byte[] senseBuffer, rdprotect, dpo, fua, fuaNv, relative,
                                    lba, blockSize, groupNumber, count, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ (10) to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ (10) response:");

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
                    AaruConsole.WriteLine("READ (10) sense:");

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
                    AaruConsole.WriteLine("READ (10) decoded sense:");
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

        static void Read12(string devPath, Device dev)
        {
            uint   lba         = 0;
            uint   blockSize   = 512;
            byte   count       = 1;
            byte   rdprotect   = 0;
            bool   dpo         = false;
            bool   fua         = false;
            bool   fuaNv       = false;
            byte   groupNumber = 0;
            bool   relative    = false;
            bool   streaming   = false;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ (12) command:");
                AaruConsole.WriteLine("Address relative to current position?: {0}", relative);
                AaruConsole.WriteLine("{1}: {0}", lba, relative ? "Address" : "LBA");
                AaruConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                AaruConsole.WriteLine("{0} bytes expected per block", blockSize);
                AaruConsole.WriteLine("How to check protection information: {0}", rdprotect);
                AaruConsole.WriteLine("Give lowest cache priority?: {0}", dpo);
                AaruConsole.WriteLine("Force bypassing cache and reading from medium?: {0}", fua);
                AaruConsole.WriteLine("Force bypassing cache and reading from non-volatile cache?: {0}", fuaNv);
                AaruConsole.WriteLine("Group number: {0}", groupNumber);
                AaruConsole.WriteLine("Use streaming?: {0}", streaming);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Address relative to current position?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out relative))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("{0}?: ", relative ? "Address" : "LBA");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Blocks to read (0 for 256 blocks)?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How to check protection information?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out rdprotect))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Give lowest cache priority?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out dpo))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Force bypassing cache and reading from medium?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out fua))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Force bypassing cache and reading from non-volatile cache?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out fuaNv))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Group number?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Use streaming?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out streaming))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.Read12(out byte[] buffer, out byte[] senseBuffer, rdprotect, dpo, fua, fuaNv, relative,
                                    lba, blockSize, groupNumber, count, streaming, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ (12) to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ (12) response:");

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
                    AaruConsole.WriteLine("READ (12) sense:");

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
                    AaruConsole.WriteLine("READ (12) decoded sense:");
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

        static void Read16(string devPath, Device dev)
        {
            ulong  lba         = 0;
            uint   blockSize   = 512;
            byte   count       = 1;
            byte   rdprotect   = 0;
            bool   dpo         = false;
            bool   fua         = false;
            bool   fuaNv       = false;
            byte   groupNumber = 0;
            bool   streaming   = false;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ (16) command:");
                AaruConsole.WriteLine("LBA: {0}", lba);
                AaruConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                AaruConsole.WriteLine("{0} bytes expected per block", blockSize);
                AaruConsole.WriteLine("How to check protection information: {0}", rdprotect);
                AaruConsole.WriteLine("Give lowest cache priority?: {0}", dpo);
                AaruConsole.WriteLine("Force bypassing cache and reading from medium?: {0}", fua);
                AaruConsole.WriteLine("Force bypassing cache and reading from non-volatile cache?: {0}", fuaNv);
                AaruConsole.WriteLine("Group number: {0}", groupNumber);
                AaruConsole.WriteLine("Use streaming?: {0}", streaming);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("LBA?: ");
                        strDev = System.Console.ReadLine();

                        if(!ulong.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Blocks to read (0 for 256 blocks)?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How to check protection information?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out rdprotect))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Give lowest cache priority?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out dpo))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Force bypassing cache and reading from medium?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out fua))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Force bypassing cache and reading from non-volatile cache?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out fuaNv))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Group number?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out count))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 1;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Use streaming?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out streaming))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.Read16(out byte[] buffer, out byte[] senseBuffer, rdprotect, dpo, fua, fuaNv, lba,
                                    blockSize, groupNumber, count, streaming, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ (16) to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ (16) response:");

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
                    AaruConsole.WriteLine("READ (16) sense:");

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
                    AaruConsole.WriteLine("READ (16) decoded sense:");
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

        static void ReadLong10(string devPath, Device dev)
        {
            uint   lba       = 0;
            ushort blockSize = 512;
            bool   correct   = false;
            bool   relative  = false;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ LONG (10) command:");
                AaruConsole.WriteLine("Address relative to current position?: {0}", relative);
                AaruConsole.WriteLine("{1}: {0}", lba, relative ? "Address" : "LBA");
                AaruConsole.WriteLine("{0} bytes expected per block", blockSize);
                AaruConsole.WriteLine("Try to error correct block?: {0}", correct);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Address relative to current position?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out relative))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("{0}?: ", relative ? "Address" : "LBA");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Try to error correct block?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out correct))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadLong10(out byte[] buffer, out byte[] senseBuffer, correct, relative, lba, blockSize,
                                        dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ LONG (10) to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ LONG (10) response:");

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
                    AaruConsole.WriteLine("READ LONG (10) sense:");

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
                    AaruConsole.WriteLine("READ LONG (10) decoded sense:");
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

        static void ReadLong16(string devPath, Device dev)
        {
            ulong  lba       = 0;
            uint   blockSize = 512;
            bool   correct   = false;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ LONG (16) command:");
                AaruConsole.WriteLine("LBA: {0}", lba);
                AaruConsole.WriteLine("{0} bytes expected per block", blockSize);
                AaruConsole.WriteLine("Try to error correct block?: {0}", correct);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("LBA?: ");
                        strDev = System.Console.ReadLine();

                        if(!ulong.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How many bytes to expect per block?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 512;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Try to error correct block?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out correct))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadLong16(out byte[] buffer, out byte[] senseBuffer, correct, lba, blockSize, dev.Timeout,
                                        out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ LONG (16) to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ LONG (16) response:");

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
                    AaruConsole.WriteLine("READ LONG (16) sense:");

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
                    AaruConsole.WriteLine("READ LONG (16) decoded sense:");
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

        static void Seek6(string devPath, Device dev)
        {
            uint   lba = 0;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for SEEK (6) command:");
                AaruConsole.WriteLine("Descriptor: {0}", lba);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("LBA?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        if(lba > 0x1FFFFF)
                        {
                            AaruConsole.WriteLine("Max LBA is {0}, setting to {0}", 0x1FFFFF);
                            lba = 0x1FFFFF;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.Seek6(out byte[] senseBuffer, lba, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending SEEK (6) to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            AaruConsole.WriteLine("SEEK (6) decoded sense:");
            AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Print sense buffer.");
            AaruConsole.WriteLine("2.- Send command again.");
            AaruConsole.WriteLine("3.- Change parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("SEEK (6) sense:");

                    if(senseBuffer != null)
                        PrintHex.PrintHexArray(senseBuffer, 64);

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

        static void Seek10(string devPath, Device dev)
        {
            uint   lba = 0;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for SEEK (10) command:");
                AaruConsole.WriteLine("Descriptor: {0}", lba);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Descriptor?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out lba))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.Seek10(out byte[] senseBuffer, lba, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending SEEK (10) to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            AaruConsole.WriteLine("SEEK (6) decoded sense:");
            AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Print sense buffer.");
            AaruConsole.WriteLine("2.- Send command again.");
            AaruConsole.WriteLine("3.- Change parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI Block Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI Block Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("SEEK (10) sense:");

                    if(senseBuffer != null)
                        PrintHex.PrintHexArray(senseBuffer, 64);

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