// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ArchiveCorp.cs
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
    internal static class ArchiveCorp
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Send an Archive vendor command to the device:");
                AaruConsole.WriteLine("1.- Send REQUEST BLOCK ADDRESS command.");
                AaruConsole.WriteLine("2.- Send SEEK BLOCK command.");
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
                        RequestBlockAddress(devPath, dev);

                        continue;
                    case 2:
                        SeekBlock(devPath, dev);

                        continue;
                    default:
                        AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();

                        continue;
                }
            }
        }

        static void RequestBlockAddress(string devPath, Device dev)
        {
            uint   lba = 0;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for REQUEST BLOCK ADDRESS command:");
                AaruConsole.WriteLine("LBA: {0}", lba);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to Archive vendor commands menu.");

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
                        AaruConsole.WriteLine("Returning to Archive vendor commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("LBA?: ");
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

            bool sense = dev.ArchiveCorpRequestBlockAddress(out byte[] buffer, out byte[] senseBuffer, lba, dev.Timeout,
                                                            out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending REQUEST BLOCK ADDRESS to the device:");
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
            AaruConsole.WriteLine("0.- Return to Archive vendor commands menu.");
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
                    AaruConsole.WriteLine("Returning to Archive vendor commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("REQUEST BLOCK ADDRESS response:");

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
                    AaruConsole.WriteLine("REQUEST BLOCK ADDRESS sense:");

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
                    AaruConsole.WriteLine("REQUEST BLOCK ADDRESS decoded sense:");
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

        static void SeekBlock(string devPath, Device dev)
        {
            bool   immediate = false;
            uint   lba       = 0;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for SEEK BLOCK command:");
                AaruConsole.WriteLine("Immediate?: {0}", immediate);
                AaruConsole.WriteLine("LBA: {0}", lba);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to Archive vendor commands menu.");

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
                        AaruConsole.WriteLine("Returning to Archive vendor commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Immediate?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out immediate))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            immediate = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("LBA?: ");
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

            bool sense =
                dev.ArchiveCorpSeekBlock(out byte[] senseBuffer, immediate, lba, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending SEEK BLOCK to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            AaruConsole.WriteLine("SEEK BLOCK decoded sense:");
            AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Print sense buffer.");
            AaruConsole.WriteLine("2.- Send command again.");
            AaruConsole.WriteLine("3.- Change parameters.");
            AaruConsole.WriteLine("0.- Return to Archive vendor commands menu.");
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
                    AaruConsole.WriteLine("Returning to Archive vendor commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("SEEK BLOCK sense:");

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