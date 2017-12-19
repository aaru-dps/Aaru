// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Kreon.cs
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

namespace DiscImageChef.Tests.Devices.SCSI
{
    public static class Kreon
    {
        public static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a Kreon vendor command to the device:");
                DicConsole.WriteLine("1.- Send EXTRACT SS command.");
                DicConsole.WriteLine("2.- Send GET FEATURE LIST command.");
                DicConsole.WriteLine("3.- Send SET LOCK STATE command.");
                DicConsole.WriteLine("4.- Send UNLOCK command.");
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
                        ExtractSecuritySectors(devPath, dev);
                        continue;
                    case 2:
                        GetFeatureList(devPath, dev);
                        continue;
                    case 3:
                        SetLockState(devPath, dev);
                        continue;
                    case 4:
                        Unlock(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void ExtractSecuritySectors(string devPath, Device dev)
        {
            byte requestNumber = 0;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for EXTRACT SS command:");
                DicConsole.WriteLine("Request number: {0}", requestNumber);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Kreon vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to Kreon vendor commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Request number?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out requestNumber))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            requestNumber = 0;
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
            bool sense = dev.KreonExtractSS(out byte[] buffer, out byte[] senseBuffer, dev.Timeout, out double duration, requestNumber);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending EXTRACT SS to the device:");
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
            DicConsole.WriteLine("0.- Return to Kreon vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Kreon vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("EXTRACT SS response:");
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
                    DicConsole.WriteLine("EXTRACT SS sense:");
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
                    DicConsole.WriteLine("EXTRACT SS decoded sense:");
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

        static void GetFeatureList(string devPath, Device dev)
        {
        start:
            System.Console.Clear();
            bool sense = dev.KreonGetFeatureList(out byte[] senseBuffer, out KreonFeatures features, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending GET FEATURE LIST to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("Features: {0}.", features);
            DicConsole.WriteLine("GET FEATURE LIST decoded sense:");
            DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("0.- Return to Kreon vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Kreon vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("GET FEATURE LIST sense:");
                    if(senseBuffer != null)
                        PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void SetLockState(string devPath, Device dev)
        {
            KreonLockStates state = KreonLockStates.Locked;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for SET LOCK STATE command:");
                DicConsole.WriteLine("Lock state: {0}", state);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Kreon vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to Kreon vendor commands menu...");
                        return;
                    case 1:
                        DicConsole.WriteLine("Lock state");
                        DicConsole.WriteLine("Available values: {0} {1} {2}", KreonLockStates.Locked, KreonLockStates.Wxripper, KreonLockStates.Xtreme);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out state))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            state = KreonLockStates.Locked;
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
            bool sense = dev.KreonSetLockState(out byte[] senseBuffer, state, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SET LOCK STATE to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("SET LOCK STATE decoded sense:");
            DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Send command again.");
            DicConsole.WriteLine("2.- Change parameters.");
            DicConsole.WriteLine("0.- Return to Kreon vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Kreon vendor commands menu...");
                    return;
                case 1:
                    goto start;
                case 2:
                    goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void Unlock(string devPath, Device dev)
        {
        start:
            System.Console.Clear();
            bool sense = dev.KreonDeprecatedUnlock(out byte[] senseBuffer, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending UNLOCK to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("UNLOCK decoded sense:");
            DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("0.- Return to Kreon vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Kreon vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("UNLOCK sense:");
                    if(senseBuffer != null)
                        PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }
    }
}
