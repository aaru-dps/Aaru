// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SPC.cs
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

static class Spc
{
    internal static void Menu(string devPath, Device dev)
    {
        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Send a SCSI Primary Commands to the device:");
            AaruConsole.WriteLine("1.- Send INQUIRY command.");
            AaruConsole.WriteLine("2.- Send INQUIRY EVPD command.");
            AaruConsole.WriteLine("3.- Send MODE SENSE (6) command.");
            AaruConsole.WriteLine("4.- Send MODE SENSE (10) command.");
            AaruConsole.WriteLine("5.- Send PREVENT ALLOW MEDIUM REMOVAL command.");
            AaruConsole.WriteLine("6.- Send READ CAPACITY (10) command.");
            AaruConsole.WriteLine("7.- Send READ CAPACITY (16) command.");
            AaruConsole.WriteLine("8.- Send READ MEDIA SERIAL NUMBER command.");
            AaruConsole.WriteLine("9.- Send REQUEST SENSE command.");
            AaruConsole.WriteLine("10.- Send TEST UNIT READY command.");
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
                    Inquiry(devPath, dev);

                    continue;
                case 2:
                    InquiryEvpd(devPath, dev);

                    continue;
                case 3:
                    ModeSense6(devPath, dev);

                    continue;
                case 4:
                    ModeSense10(devPath, dev);

                    continue;
                case 5:
                    PreventAllowMediumRemoval(devPath, dev);

                    continue;
                case 6:
                    ReadCapacity10(devPath, dev);

                    continue;
                case 7:
                    ReadCapacity16(devPath, dev);

                    continue;
                case 8:
                    ReadMediaSerialNumber(devPath, dev);

                    continue;
                case 9:
                    RequestSense(devPath, dev);

                    continue;
                case 10:
                    TestUnitReady(devPath, dev);

                    continue;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();

                    continue;
            }
        }
    }

    static void Inquiry(string devPath, Device dev)
    {
        start:
        System.Console.Clear();
        bool sense = dev.ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending INQUIRY to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode buffer.");
        AaruConsole.WriteLine("3.- Print sense buffer.");
        AaruConsole.WriteLine("4.- Decode sense buffer.");
        AaruConsole.WriteLine("5.- Send command again.");
        AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("INQUIRY response:");

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
                AaruConsole.WriteLine("INQUIRY decoded response:");

                if(buffer != null)
                    AaruConsole.WriteLine("{0}", Decoders.SCSI.Inquiry.Prettify(buffer));

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("INQUIRY sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("INQUIRY decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 5: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void InquiryEvpd(string devPath, Device dev)
    {
        byte   page = 1;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for INQUIRY command:");
            AaruConsole.WriteLine("EVPD page: {0}", page);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                    AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Page?: ");
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out page))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        page = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, page, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending INQUIRY to the device:");
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
        AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("INQUIRY response:");

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
                AaruConsole.WriteLine("INQUIRY sense:");

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
                AaruConsole.WriteLine("INQUIRY decoded sense:");
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

    static void ModeSense6(string devPath, Device dev)
    {
        bool                     dbd         = false;
        ScsiModeSensePageControl pageControl = ScsiModeSensePageControl.Current;
        byte                     page        = 0x3F;
        byte                     subpage     = 0xFF;
        string                   strDev;
        int                      item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for MODE SENSE (6) command:");
            AaruConsole.WriteLine("DBD?: {0}", dbd);
            AaruConsole.WriteLine("Page control: {0}", pageControl);
            AaruConsole.WriteLine("Page: {0}", page);
            AaruConsole.WriteLine("Subpage: {0}", subpage);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                    AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("DBD?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out dbd))
                    {
                        AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                        dbd = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine("Page control");

                    AaruConsole.WriteLine("Available values: {0} {1} {2} {3}", ScsiModeSensePageControl.Changeable,
                                          ScsiModeSensePageControl.Current, ScsiModeSensePageControl.Default,
                                          ScsiModeSensePageControl.Saved);

                    AaruConsole.Write("Choose?: ");
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out pageControl))
                    {
                        AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                        pageControl = ScsiModeSensePageControl.Current;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Page?: ");
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out page))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        page = 0x3F;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Subpage?: ");
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out subpage))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        subpage = 0xFF;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ModeSense6(out byte[] buffer, out byte[] senseBuffer, dbd, pageControl, page, subpage,
                                    dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending MODE SENSE (6) to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode buffer.");
        AaruConsole.WriteLine("3.- Print sense buffer.");
        AaruConsole.WriteLine("4.- Decode sense buffer.");
        AaruConsole.WriteLine("5.- Send command again.");
        AaruConsole.WriteLine("6.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("MODE SENSE (6) response:");

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
                AaruConsole.WriteLine("MODE SENSE (6) decoded response:");

                if(buffer != null)
                    AaruConsole.WriteLine("{0}", Modes.PrettifyModeHeader6(buffer, dev.ScsiType));

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("MODE SENSE (6) sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("MODE SENSE (6) decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 5: goto start;
            case 6: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ModeSense10(string devPath, Device dev)
    {
        bool                     llba        = false;
        bool                     dbd         = false;
        ScsiModeSensePageControl pageControl = ScsiModeSensePageControl.Current;
        byte                     page        = 0x3F;
        byte                     subpage     = 0xFF;
        string                   strDev;
        int                      item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for MODE SENSE (10) command:");
            AaruConsole.WriteLine("LLBA?: {0}", llba);
            AaruConsole.WriteLine("DBD?: {0}", dbd);
            AaruConsole.WriteLine("Page control: {0}", pageControl);
            AaruConsole.WriteLine("Page: {0}", page);
            AaruConsole.WriteLine("Subpage: {0}", subpage);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                    AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("LLBA?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out llba))
                    {
                        AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                        llba = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("DBD?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out dbd))
                    {
                        AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                        dbd = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.WriteLine("Page control");

                    AaruConsole.WriteLine("Available values: {0} {1} {2} {3}", ScsiModeSensePageControl.Changeable,
                                          ScsiModeSensePageControl.Current, ScsiModeSensePageControl.Default,
                                          ScsiModeSensePageControl.Saved);

                    AaruConsole.Write("Choose?: ");
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out pageControl))
                    {
                        AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                        pageControl = ScsiModeSensePageControl.Current;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Page?: ");
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out page))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        page = 0x3F;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Subpage?: ");
                    strDev = System.Console.ReadLine();

                    if(!byte.TryParse(strDev, out subpage))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        subpage = 0xFF;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ModeSense10(out byte[] buffer, out byte[] senseBuffer, llba, dbd, pageControl, page, subpage,
                                     dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending MODE SENSE (10) to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
        AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print buffer.");
        AaruConsole.WriteLine("2.- Decode buffer.");
        AaruConsole.WriteLine("3.- Print sense buffer.");
        AaruConsole.WriteLine("4.- Decode sense buffer.");
        AaruConsole.WriteLine("5.- Send command again.");
        AaruConsole.WriteLine("6.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("MODE SENSE (10) response:");

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
                AaruConsole.WriteLine("MODE SENSE (10) decoded response:");

                if(buffer != null)
                    AaruConsole.WriteLine("{0}", Modes.PrettifyModeHeader10(buffer, dev.ScsiType));

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 3:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("MODE SENSE (10) sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 4:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("MODE SENSE (10) decoded sense:");
                AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 5: goto start;
            case 6: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void PreventAllowMediumRemoval(string devPath, Device dev)
    {
        ScsiPreventAllowMode mode = ScsiPreventAllowMode.Allow;
        string               strDev;
        int                  item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for PREVENT ALLOW MEDIUM REMOVAL command:");
            AaruConsole.WriteLine("Mode: {0}", mode);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                    AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                    return;
                case 1:
                    AaruConsole.WriteLine("Mode");

                    AaruConsole.WriteLine("Available values: {0} {1} {2} {3}", ScsiPreventAllowMode.Allow,
                                          ScsiPreventAllowMode.Prevent, ScsiPreventAllowMode.PreventAll,
                                          ScsiPreventAllowMode.PreventChanger);

                    AaruConsole.Write("Choose?: ");
                    strDev = System.Console.ReadLine();

                    if(!Enum.TryParse(strDev, true, out mode))
                    {
                        AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                        mode = ScsiPreventAllowMode.Allow;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.SpcPreventAllowMediumRemoval(out byte[] senseBuffer, mode, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending PREVENT ALLOW MEDIUM REMOVAL to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("PREVENT ALLOW MEDIUM REMOVAL decoded sense:");
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Send command again.");
        AaruConsole.WriteLine("2.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                return;
            case 1: goto start;
            case 2: goto parameters;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }

    static void ReadCapacity10(string devPath, Device dev)
    {
        bool   relative = false;
        bool   partial  = false;
        uint   address  = 0;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ CAPACITY (10) command:");
            AaruConsole.WriteLine("Relative address?: {0}", relative);
            AaruConsole.WriteLine("Partial capacity?: {0}", partial);
            AaruConsole.WriteLine("Address: {0}", address);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                    AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Relative address?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out relative))
                    {
                        AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                        relative = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Partial capacity?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out partial))
                    {
                        AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                        partial = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Address?: ");
                    strDev = System.Console.ReadLine();

                    if(!uint.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        address = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ReadCapacity(out byte[] buffer, out byte[] senseBuffer, relative, address, partial,
                                      dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ CAPACITY (10) to the device:");
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
        AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CAPACITY (10) response:");

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
                AaruConsole.WriteLine("READ CAPACITY (10) sense:");

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
                AaruConsole.WriteLine("READ CAPACITY (10) decoded sense:");
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

    static void ReadCapacity16(string devPath, Device dev)
    {
        bool   partial = false;
        ulong  address = 0;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for READ CAPACITY (16) command:");
            AaruConsole.WriteLine("Partial capacity?: {0}", partial);
            AaruConsole.WriteLine("Address: {0}", address);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                    AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Partial capacity?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out partial))
                    {
                        AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                        partial = false;
                        System.Console.ReadKey();

                        continue;
                    }

                    AaruConsole.Write("Address?: ");
                    strDev = System.Console.ReadLine();

                    if(!ulong.TryParse(strDev, out address))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        address = 0;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();

        bool sense = dev.ReadCapacity16(out byte[] buffer, out byte[] senseBuffer, address, partial, dev.Timeout,
                                        out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ CAPACITY (16) to the device:");
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
        AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ CAPACITY (16) response:");

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
                AaruConsole.WriteLine("READ CAPACITY (16) sense:");

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
                AaruConsole.WriteLine("READ CAPACITY (16) decoded sense:");
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

    static void ReadMediaSerialNumber(string devPath, Device dev)
    {
        start:
        System.Console.Clear();

        bool sense = dev.ReadMediaSerialNumber(out byte[] buffer, out byte[] senseBuffer, dev.Timeout,
                                               out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending READ MEDIA SERIAL NUMBER to the device:");
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
        AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("READ MEDIA SERIAL NUMBER response:");

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
                AaruConsole.WriteLine("READ MEDIA SERIAL NUMBER sense:");

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
                AaruConsole.WriteLine("READ MEDIA SERIAL NUMBER decoded sense:");
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

    static void RequestSense(string devPath, Device dev)
    {
        bool   descriptor = false;
        string strDev;
        int    item;

        parameters:

        while(true)
        {
            System.Console.Clear();
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Parameters for REQUEST SENSE command:");
            AaruConsole.WriteLine("Descriptor: {0}", descriptor);
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Change parameters.");
            AaruConsole.WriteLine("2.- Send command with these parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                    AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                    return;
                case 1:
                    AaruConsole.Write("Descriptor?: ");
                    strDev = System.Console.ReadLine();

                    if(!bool.TryParse(strDev, out descriptor))
                    {
                        AaruConsole.WriteLine("Not a number. Press any key to continue...");
                        descriptor = false;
                        System.Console.ReadKey();
                    }

                    break;
                case 2: goto start;
            }
        }

        start:
        System.Console.Clear();
        bool sense = dev.RequestSense(descriptor, out byte[] senseBuffer, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending REQUEST SENSE to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
        AaruConsole.WriteLine("REQUEST SENSE decoded sense:");
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print sense buffer.");
        AaruConsole.WriteLine("2.- Send command again.");
        AaruConsole.WriteLine("3.- Change parameters.");
        AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("REQUEST SENSE sense:");

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

    static void TestUnitReady(string devPath, Device dev)
    {
        start:
        System.Console.Clear();
        bool sense = dev.ScsiTestUnitReady(out byte[] senseBuffer, dev.Timeout, out double duration);

        menu:
        AaruConsole.WriteLine("Device: {0}", devPath);
        AaruConsole.WriteLine("Sending TEST UNIT READY to the device:");
        AaruConsole.WriteLine("Command took {0} ms.", duration);
        AaruConsole.WriteLine("Sense is {0}.", sense);
        AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
        AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
        AaruConsole.WriteLine("TEST UNIT READY decoded sense:");
        AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
        AaruConsole.WriteLine();
        AaruConsole.WriteLine("Choose what to do:");
        AaruConsole.WriteLine("1.- Print sense buffer.");
        AaruConsole.WriteLine("2.- Send command again.");
        AaruConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                AaruConsole.WriteLine("Returning to SCSI Primary Commands menu...");

                return;
            case 1:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("TEST UNIT READY sense:");

                if(senseBuffer != null)
                    PrintHex.PrintHexArray(senseBuffer, 64);

                AaruConsole.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);

                goto menu;
            case 2: goto start;
            default:
                AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                System.Console.ReadKey();
                System.Console.Clear();

                goto menu;
        }
    }
}