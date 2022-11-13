// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Pioneer.cs
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

namespace Aaru.Tests.Devices.SCSI;

using System;
using Aaru.Console;
using Aaru.Decoders.SCSI;
using Aaru.Devices;
using Aaru.Helpers;

static class Pioneer
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Send a Pioneer vendor command to the device:");
            AaruConsole.WriteLine("1.- Send READ CD-DA command.");
            AaruConsole.WriteLine("2.- Send READ CD-DA MSF command.");
            AaruConsole.WriteLine("3.- Send READ CD-XA command.");
            AaruConsole.WriteLine("0.- Return to SCSI commands menu.");
            AaruConsole.Write("Choose: ");

            string strDev = Console.ReadLine();

            if(!int.TryParse(strDev, out int item))
            {
                AaruConsole.WriteLine("Not a number. Press any key to continue...");
                Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine("Returning to SCSI commands menu...");

                    return;
                case 1:
                    ReadCdDa(devPath, dev);

                    continue;
                case 2:
                    ReadCdDaMsf(devPath, dev);

                    continue;
                case 3:
                    ReadCdXa(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    Console.ReadKey();

                    continue;
            }
        }
    }

    static void ReadCdDa(string devPath, Device dev)
    {
        uint              address   = 0;
        uint              length    = 1;
        PioneerSubchannel subchan   = PioneerSubchannel.None;
        uint              blockSize = 2352;
        string            strDev;
        int               item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ CD-DA command:");
            AaruConsole.WriteLine("LBA: {0}", address);
            AaruConsole.WriteLine("Will transfer {0} sectors", length);
            AaruConsole.WriteLine("Subchannel mode: {0}", subchan);
            AaruConsole.WriteLine("{0} bytes per sectors", blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");

            strDev = Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine("Not a number. Press any key to continue...");
                Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine("Returning to Pioneer vendor commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Logical Block Address?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        address = 0;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("How many sectors to transfer?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out length))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        length = 1;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine("Subchannel mode");

                    AaruConsole.WriteLine("Available values: {0} {1} {2} {3}", PioneerSubchannel.None,
                                          PioneerSubchannel.Q16, PioneerSubchannel.All, PioneerSubchannel.Only);

                    AaruConsole.Write("Choose?: ");
                    strDev = Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out subchan))
                    {
                        AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                        subchan = PioneerSubchannel.None;
                        Console.ReadKey();

                        continue;
                    }

                    blockSize = subchan switch
                                {
                                    PioneerSubchannel.Q16  => 2368,
                                    PioneerSubchannel.All  => 2448,
                                    PioneerSubchannel.Only => 96,
                                    _                      => 2352
                                };

                    break;
                case 2: goto start;
            }
        }

    start:
        Console.Clear();

        bool sense = dev.PioneerReadCdDa(out byte[] buffer, out byte[] senseBuffer, address, blockSize, length, subchan,
                                         dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ CD-DA to the device:");
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
        AaruConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");
        AaruConsole.Write("Choose: ");

        strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Pioneer vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CD-DA response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CD-DA sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CD-DA decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            case 5: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void ReadCdDaMsf(string devPath, Device dev)
    {
        byte              startFrame  = 0;
        byte              startSecond = 2;
        byte              startMinute = 0;
        byte              endFrame    = 0;
        const byte        endSecond   = 0;
        byte              endMinute   = 0;
        PioneerSubchannel subchan     = PioneerSubchannel.None;
        uint              blockSize   = 2352;
        string            strDev;
        int               item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ CD-DA MSF command:");
            AaruConsole.WriteLine("Start: {0:D2}:{1:D2}:{2:D2}", startMinute, startSecond, startFrame);
            AaruConsole.WriteLine("End: {0:D2}:{1:D2}:{2:D2}", endMinute, endSecond, endFrame);
            AaruConsole.WriteLine("Subchannel mode: {0}", subchan);
            AaruConsole.WriteLine("{0} bytes per sectors", blockSize);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");

            strDev = Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine("Not a number. Press any key to continue...");
                Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine("Returning to Pioneer vendor commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Start minute?: ");
                    strDev = Console.ReadLine();

                    if(!byte.TryParse(strDev, out startMinute))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        startMinute = 0;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Start second?: ");
                    strDev = Console.ReadLine();

                    if(!byte.TryParse(strDev, out startSecond))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        startSecond = 2;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Start frame?: ");
                    strDev = Console.ReadLine();

                    if(!byte.TryParse(strDev, out startFrame))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        startFrame = 0;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("End minute?: ");
                    strDev = Console.ReadLine();

                    if(!byte.TryParse(strDev, out endMinute))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        endMinute = 0;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("End second?: ");
                    strDev = Console.ReadLine();

                    if(!byte.TryParse(strDev, out endMinute))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        endMinute = 2;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("End frame?: ");
                    strDev = Console.ReadLine();

                    if(!byte.TryParse(strDev, out endFrame))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        endFrame = 0;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine("Subchannel mode");

                    AaruConsole.WriteLine("Available values: {0} {1} {2} {3}", PioneerSubchannel.None,
                                          PioneerSubchannel.Q16, PioneerSubchannel.All, PioneerSubchannel.Only);

                    AaruConsole.Write("Choose?: ");
                    strDev = Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out subchan))
                    {
                        AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                        subchan = PioneerSubchannel.None;
                        Console.ReadKey();

                        continue;
                    }

                    blockSize = subchan switch
                                {
                                    PioneerSubchannel.Q16  => 2368,
                                    PioneerSubchannel.All  => 2448,
                                    PioneerSubchannel.Only => 96,
                                    _                      => 2352
                                };

                    break;
                case 2: goto start;
            }
        }

    start:
        var startMsf = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
        var endMsf   = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
        Console.Clear();

        bool sense = dev.PioneerReadCdDaMsf(out byte[] buffer, out byte[] senseBuffer, startMsf, endMsf, blockSize,
                                            subchan, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ CD-DA MSF to the device:");
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
        AaruConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");
        AaruConsole.Write("Choose: ");

        strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Pioneer vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CD-DA MSF response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CD-DA MSF sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CD-DA MSF decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            case 5: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }

    static void ReadCdXa(string devPath, Device dev)
    {
        uint   address     = 0;
        uint   length      = 1;
        var    errorFlags  = false;
        var    wholeSector = false;
        string strDev;
        int    item;

    parameters:

        while(true)
        {
            Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ CD-XA command:");
            AaruConsole.WriteLine("LBA: {0}", address);
            AaruConsole.WriteLine("Will transfer {0} sectors", length);
            AaruConsole.WriteLine("Include error flags?: {0}", errorFlags);
            AaruConsole.WriteLine("Whole sector?: {0}", wholeSector);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");

            strDev = Console.ReadLine();

            if(!int.TryParse(strDev, out item))
            {
                AaruConsole.WriteLine("Not a number. Press any key to continue...");
                Console.ReadKey();

                continue;
            }

            switch(item)
            {
                case 0:
                    AaruConsole.WriteLine("Returning to Pioneer vendor commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Logical Block Address?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        address = 0;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("How many sectors to transfer?: ");
                    strDev = Console.ReadLine();

                    if(!uint.TryParse(strDev, out length))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        length = 1;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Include error flags?: ");
                    strDev = Console.ReadLine();

                    if(!bool.TryParse(strDev, out errorFlags))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        errorFlags = false;
                        Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Read whole sector?: ");
                    strDev = Console.ReadLine();

                    if(!bool.TryParse(strDev, out wholeSector))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        wholeSector = false;
                        Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

    start:
        Console.Clear();

        bool sense = dev.PioneerReadCdXa(out byte[] buffer, out byte[] senseBuffer, address, length, errorFlags,
                                         wholeSector, dev.Timeout, out double duration);

    menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ CD-XA to the device:");
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
        AaruConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");
        AaruConsole.Write("Choose: ");

        strDev = Console.ReadLine();

        if(!int.TryParse(strDev, out item))
        {
            AaruConsole.WriteLine("Not a number. Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            goto menu;
        }

        switch(item)
        {
            case 0:
                AaruConsole.WriteLine("Returning to Pioneer vendor commands menu...");

                return;
            case 1:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CD-XA response:");

                if(buffer != null)
                    PrintHex.PrintHexArray(buffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CD-XA sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CD-XA decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4: goto start;
            case 5: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();

                goto menu;
        }
    }
}