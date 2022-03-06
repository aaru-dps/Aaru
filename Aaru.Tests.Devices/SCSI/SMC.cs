// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SMC.cs
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

using System;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SCSI;

internal static class Smc
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Send a SCSI Media Changer command to the device:");
            AaruConsole.WriteLine("1.- Send READ ATTRIBUTE command.");
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
                    ReadAttribute(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void ReadAttribute(string devPath, Device dev)
    {
        ushort              element        = 0;
        byte                elementType    = 0;
        byte                volume         = 0;
        byte                partition      = 0;
        ushort              firstAttribute = 0;
        bool                cache          = false;
        ScsiAttributeAction action         = ScsiAttributeAction.Values;
        string              strDev;
        int                 item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ ATTRIBUTE command:");
            AaruConsole.WriteLine("Action: {0}", action);
            AaruConsole.WriteLine("Element: {0}", element);
            AaruConsole.WriteLine("Element type: {0}", elementType);
            AaruConsole.WriteLine("Volume: {0}", volume);
            AaruConsole.WriteLine("Partition: {0}", partition);
            AaruConsole.WriteLine("First attribute: {0}", firstAttribute);
            AaruConsole.WriteLine("Use cache?: {0}", cache);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI Media Changer commands menu.");

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
                    AaruConsole.WriteLine("Returning to SCSI Media Changer commands menu...");

                    return;
                case 1:
                    AaruConsole.WriteLine("Attribute action");

                    AaruConsole.WriteLine("Available values: {0} {1} {2} {3} {4}", ScsiAttributeAction.Values,
                                          ScsiAttributeAction.List, ScsiAttributeAction.VolumeList,
                                          ScsiAttributeAction.PartitionList, ScsiAttributeAction.ElementList,
                                          ScsiAttributeAction.Supported);

                    AaruConsole.Write("Choose?: ");
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out action))
                    {
                        AaruConsole.WriteLine("Not a valid attribute action. Press any key to continue...");
                        action = ScsiAttributeAction.Values;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Element?: ");
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out element))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        element = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Element type?: ");
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out elementType))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        elementType = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Volume?: ");
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out volume))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        volume = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Partition?: ");
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out partition))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        partition = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("First attribute?: ");
                    strDev = System.Console.ReadLine();

                    if(!ushort.TryParse(strDev, out firstAttribute))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        firstAttribute = 0;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Use cache?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out cache))
                    {
                        AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                        cache = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ReadAttribute(out byte[] buffer, out byte[] senseBuffer, action, element, elementType,
                                       volume, partition, firstAttribute, cache, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ ATTRIBUTE to the device:");
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
        AaruConsole.WriteLine("0.- Return to SCSI Media Changer commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Media Changer commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ ATTRIBUTE response:");

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
                AaruConsole.WriteLine("READ ATTRIBUTE sense:");

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
                AaruConsole.WriteLine("READ ATTRIBUTE decoded sense:");
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
}