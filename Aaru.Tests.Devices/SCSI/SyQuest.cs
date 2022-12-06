// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SyQuest.cs
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
    internal static class SyQuest
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Send a SyQuest vendor command to the device:");
                AaruConsole.WriteLine("1.- Send READ (6) command.");
                AaruConsole.WriteLine("2.- Send READ (10) command.");
                AaruConsole.WriteLine("3.- Send READ LONG (6) command.");
                AaruConsole.WriteLine("4.- Send READ LONG (10) command.");
                AaruConsole.WriteLine("5.- Send READ/RESET USAGE COUNTER command.");
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
                        Read6(devPath, dev, false);

                        continue;
                    case 2:
                        Read10(devPath, dev, false);

                        continue;
                    case 3:
                        Read6(devPath, dev, true);

                        continue;
                    case 4:
                        Read10(devPath, dev, true);

                        continue;
                    case 5:
                        ReadResetUsageCounter(devPath, dev);

                        continue;
                    default:
                        AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();

                        continue;
                }
            }
        }

        static void Read6(string devPath, Device dev, bool readlong)
        {
            uint   lba       = 0;
            uint   blockSize = 512;
            byte   count     = 1;
            bool   noDma     = false;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ {0}(6) command:", readlong ? "LONG " : "");
                AaruConsole.WriteLine("LBA: {0}", lba);
                AaruConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                AaruConsole.WriteLine("{0} bytes expected per block", blockSize);
                AaruConsole.WriteLine("Inhibit DMA?: {0}", noDma);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SyQuest vendor commands menu.");

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
                        AaruConsole.WriteLine("Returning to SyQuest vendor commands menu...");

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

                            continue;
                        }

                        AaruConsole.Write("Inhibit DMA?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out noDma))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            noDma = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.SyQuestRead6(out byte[] buffer, out byte[] senseBuffer, lba, blockSize, count, noDma,
                                          readlong, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ {0}(6) to the device:", readlong ? "LONG " : "");
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
            AaruConsole.WriteLine("0.- Return to SyQuest vendor commands menu.");
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
                    AaruConsole.WriteLine("Returning to SyQuest vendor commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ {0}(6) response:", readlong ? "LONG " : "");

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
                    AaruConsole.WriteLine("READ {0}(6) sense:", readlong ? "LONG " : "");

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
                    AaruConsole.WriteLine("READ {0}(6) decoded sense:", readlong ? "LONG " : "");
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

        static void Read10(string devPath, Device dev, bool readlong)
        {
            uint   lba       = 0;
            uint   blockSize = 512;
            byte   count     = 1;
            bool   noDma     = false;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ {0}(10) command:", readlong ? "LONG " : "");
                AaruConsole.WriteLine("LBA: {0}", lba);
                AaruConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                AaruConsole.WriteLine("{0} bytes expected per block", blockSize);
                AaruConsole.WriteLine("Inhibit DMA?: {0}", noDma);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SyQuest vendor commands menu.");

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
                        AaruConsole.WriteLine("Returning to SyQuest vendor commands menu...");

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

                        AaruConsole.Write("Inhibit DMA?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out noDma))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            noDma = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.SyQuestRead10(out byte[] buffer, out byte[] senseBuffer, lba, blockSize, count, noDma,
                                           readlong, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ {0}(10) to the device:", readlong ? "LONG " : "");
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
            AaruConsole.WriteLine("0.- Return to SyQuest vendor commands menu.");
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
                    AaruConsole.WriteLine("Returning to SyQuest vendor commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ {0}(10) response:", readlong ? "LONG " : "");

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
                    AaruConsole.WriteLine("READ {0}(10) sense:", readlong ? "LONG " : "");

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
                    AaruConsole.WriteLine("READ {0}(10) decoded sense:", readlong ? "LONG " : "");
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

        static void ReadResetUsageCounter(string devPath, Device dev)
        {
            start:
            System.Console.Clear();

            bool sense =
                dev.SyQuestReadUsageCounter(out byte[] buffer, out byte[] senseBuffer, dev.Timeout,
                                            out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ/RESET USAGE COUNTER to the device:");
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
            AaruConsole.WriteLine("0.- Return to SyQuest vendor commands menu.");
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
                    AaruConsole.WriteLine("Returning to SyQuest vendor commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ/RESET USAGE COUNTER response:");

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
                    AaruConsole.WriteLine("READ/RESET USAGE COUNTER sense:");

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
                    AaruConsole.WriteLine("READ/RESET USAGE COUNTER decoded sense:");
                    AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
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
    }
}