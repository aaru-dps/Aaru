// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Certance.cs
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

using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;

namespace Aaru.Tests.Devices.SCSI
{
    static class Certance
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a Certance vendor command to the device:");
                DicConsole.WriteLine("1.- Send PARK command.");
                DicConsole.WriteLine("2.- Send UNPARK command.");
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
                        Park(devPath, dev);
                        continue;
                    case 2:
                        Unpark(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void Park(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.CertancePark(out byte[] senseBuffer, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending PARK to the device:");
            DicConsole.WriteLine("Command took {0} ms.",               duration);
            DicConsole.WriteLine("Sense is {0}.",                      sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.",         senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("PARK decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("0.- Return to Certance vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Certance vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("PARK sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2: goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void Unpark(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.CertanceUnpark(out byte[] senseBuffer, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending UNPARK to the device:");
            DicConsole.WriteLine("Command took {0} ms.",               duration);
            DicConsole.WriteLine("Sense is {0}.",                      sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.",         senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("UNPARK decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("0.- Return to Certance vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Certance vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("UNPARK sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2: goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }
    }
}