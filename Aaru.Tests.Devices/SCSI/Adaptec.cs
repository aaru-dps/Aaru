// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Adaptec.cs
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.Console;
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Devices;

namespace DiscImageChef.Tests.Devices.SCSI
{
    static class Adaptec
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send an Adaptec vendor command to the device:");
                DicConsole.WriteLine("1.- Send READ/RESET USAGE COUNTER command.");
                DicConsole.WriteLine("2.- Send READ DATA BUFFER command.");
                DicConsole.WriteLine("3.- Send SET ERROR THRESHOLD command.");
                DicConsole.WriteLine("4.- Send TRANSLATE command.");
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
                        ReadResetUsageCounter(devPath, dev);
                        continue;
                    case 2:
                        ReadDataBuffer(devPath, dev);
                        continue;
                    case 3:
                        SetErrorThreshold(devPath, dev);
                        continue;
                    case 4:
                        Translate(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void ReadResetUsageCounter(string devPath, Device dev)
        {
            bool   drive1 = false;
            string strDev;
            int    item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ/RESET USAGE COUNTER command:");
                DicConsole.WriteLine("Drive 1?: {0}", drive1);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Adaptec vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to Adaptec vendor commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Drive 1?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out drive1))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            drive1 = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.AdaptecReadUsageCounter(out byte[] buffer, out byte[] senseBuffer, drive1, dev.Timeout,
                                                     out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ/RESET USAGE COUNTER to the device:");
            DicConsole.WriteLine("Command took {0} ms.",               duration);
            DicConsole.WriteLine("Sense is {0}.",                      sense);
            DicConsole.WriteLine("Buffer is {0} bytes.",               buffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Buffer is null or empty? {0}",       ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.",         senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("5.- Change parameters.");
            DicConsole.WriteLine("0.- Return to Adaptec vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Adaptec vendor commands menu...");
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
                case 5: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void ReadDataBuffer(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.AdaptecReadBuffer(out byte[] buffer, out byte[] senseBuffer, dev.Timeout,
                                               out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ DATA BUFFER to the device:");
            DicConsole.WriteLine("Command took {0} ms.",               duration);
            DicConsole.WriteLine("Sense is {0}.",                      sense);
            DicConsole.WriteLine("Buffer is {0} bytes.",               buffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Buffer is null or empty? {0}",       ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.",         senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("0.- Return to Adaptec vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Adaptec vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DATA BUFFER response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DATA BUFFER sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DATA BUFFER decoded sense:");
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

        static void SetErrorThreshold(string devPath, Device dev)
        {
            bool   drive1    = false;
            byte   threshold = 0;
            string strDev;
            int    item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for SET ERROR THRESHOLD command:");
                DicConsole.WriteLine("Drive 1?: {0}",        drive1);
                DicConsole.WriteLine("Error threshold: {0}", threshold);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Adaptec vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to Adaptec vendor commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Drive 1?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out drive1))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            drive1 = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Error threshold?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out threshold))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            threshold = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense =
                dev.AdaptecSetErrorThreshold(threshold, out byte[] senseBuffer, drive1, dev.Timeout,
                                             out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SET ERROR THRESHOLD to the device:");
            DicConsole.WriteLine("Command took {0} ms.",               duration);
            DicConsole.WriteLine("Sense is {0}.",                      sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.",         senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("SET ERROR THRESHOLD decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to Adaptec vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Adaptec vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SET ERROR THRESHOLD sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2: goto start;
                case 3: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void Translate(string devPath, Device dev)
        {
            bool   drive1 = false;
            uint   lba    = 0;
            string strDev;
            int    item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for TRANSLATE command:");
                DicConsole.WriteLine("Drive 1?: {0}", drive1);
                DicConsole.WriteLine("LBA: {0}",      lba);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Adaptec vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to Adaptec vendor commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Drive 1?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out drive1))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            drive1 = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("LBA?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out lba))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            lba = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.AdaptecTranslate(out byte[] buffer, out byte[] senseBuffer, drive1, lba, dev.Timeout,
                                              out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending TRANSLATE to the device:");
            DicConsole.WriteLine("Command took {0} ms.",               duration);
            DicConsole.WriteLine("Sense is {0}.",                      sense);
            DicConsole.WriteLine("Buffer is {0} bytes.",               buffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Buffer is null or empty? {0}",       ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.",         senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("5.- Change parameters.");
            DicConsole.WriteLine("0.- Return to Adaptec vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Adaptec vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("TRANSLATE response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("TRANSLATE sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("TRANSLATE decoded sense:");
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
    }
}