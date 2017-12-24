// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SyQuest.cs
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
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;

namespace DiscImageChef.Tests.Devices.SCSI
{
    static class SyQuest
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a SyQuest vendor command to the device:");
                DicConsole.WriteLine("1.- Send READ (6) command.");
                DicConsole.WriteLine("2.- Send READ (10) command.");
                DicConsole.WriteLine("3.- Send READ LONG (6) command.");
                DicConsole.WriteLine("4.- Send READ LONG (10) command.");
                DicConsole.WriteLine("5.- Send READ/RESET USAGE COUNTER command.");
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
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void Read6(string devPath, Device dev, bool readlong)
        {
            uint lba = 0;
            uint blockSize = 512;
            byte count = 1;
            bool noDma = false;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ {0}(6) command:", readlong ? "LONG " : "");
                DicConsole.WriteLine("LBA: {0}", lba);
                DicConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine("Inhibit DMA?: {0}", noDma);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SyQuest vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to SyQuest vendor commands menu...");
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

                        DicConsole.Write("Inhibit DMA?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out noDma))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ {0}(6) to the device:", readlong ? "LONG " : "");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("5.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SyQuest vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to SyQuest vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ {0}(6) response:", readlong ? "LONG " : "");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ {0}(6) sense:", readlong ? "LONG " : "");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ {0}(6) decoded sense:", readlong ? "LONG " : "");
                    DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4: goto start;
                case 5: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void Read10(string devPath, Device dev, bool readlong)
        {
            uint lba = 0;
            uint blockSize = 512;
            byte count = 1;
            bool noDma = false;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ {0}(10) command:", readlong ? "LONG " : "");
                DicConsole.WriteLine("LBA: {0}", lba);
                DicConsole.WriteLine("{0} blocks to read", count == 0 ? 256 : count);
                DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine("Inhibit DMA?: {0}", noDma);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SyQuest vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to SyQuest vendor commands menu...");
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

                        DicConsole.Write("Inhibit DMA?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out noDma))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ {0}(10) to the device:", readlong ? "LONG " : "");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("5.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SyQuest vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to SyQuest vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ {0}(10) response:", readlong ? "LONG " : "");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ {0}(10) sense:", readlong ? "LONG " : "");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ {0}(10) decoded sense:", readlong ? "LONG " : "");
                    DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4: goto start;
                case 5: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ/RESET USAGE COUNTER to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("0.- Return to SyQuest vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to SyQuest vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ/RESET USAGE COUNTER response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ/RESET USAGE COUNTER sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ/RESET USAGE COUNTER decoded sense:");
                    DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
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
    }
}