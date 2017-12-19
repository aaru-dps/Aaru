// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SPC.cs
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

using System;
using DiscImageChef.Console;
using DiscImageChef.Devices;

namespace DiscImageChef.Tests.Devices.SCSI
{
    public static class SPC
    {
        public static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a SCSI Primary Commands to the device:");
                DicConsole.WriteLine("1.- Send INQUIRY command.");
                DicConsole.WriteLine("2.- Send INQUIRY EVPD command.");
                DicConsole.WriteLine("3.- Send MODE SENSE (6) command.");
                DicConsole.WriteLine("4.- Send MODE SENSE (10) command.");
                DicConsole.WriteLine("5.- Send PREVENT ALLOW MEDIUM REMOVAL command.");
                DicConsole.WriteLine("6.- Send READ CAPACITY (10) command.");
                DicConsole.WriteLine("7.- Send READ CAPACITY (16) command.");
                DicConsole.WriteLine("8.- Send READ MEDIA SERIAL NUMBER command.");
                DicConsole.WriteLine("9.- Send REQUEST SENSE command.");
                DicConsole.WriteLine("10.- Send TEST UNIT READY command.");
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
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending INQUIRY to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode buffer.");
            DicConsole.WriteLine("3.- Print sense buffer.");
            DicConsole.WriteLine("4.- Decode sense buffer.");
            DicConsole.WriteLine("5.- Send command again.");
            DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("INQUIRY response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("INQUIRY decoded response:");
                    if(buffer != null) DicConsole.WriteLine("{0}", Decoders.SCSI.Inquiry.Prettify(buffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("INQUIRY sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("INQUIRY decoded sense:");
                    DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 5: goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void InquiryEvpd(string devPath, Device dev)
        {
            byte page = 1;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for INQUIRY command:");
                DicConsole.WriteLine("EVPD page: {0}", page);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Page?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out page))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            page = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.ScsiInquiry(out byte[] buffer, out byte[] senseBuffer, page, dev.Timeout,
                                         out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending INQUIRY to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("5.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("INQUIRY response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("INQUIRY sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("INQUIRY decoded sense:");
                    DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
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

        static void ModeSense6(string devPath, Device dev)
        {
            bool dbd = false;
            ScsiModeSensePageControl pageControl = ScsiModeSensePageControl.Current;
            byte page = 0x3F;
            byte subpage = 0xFF;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for MODE SENSE (6) command:");
                DicConsole.WriteLine("DBD?: {0}", dbd);
                DicConsole.WriteLine("Page control: {0}", pageControl);
                DicConsole.WriteLine("Page: {0}", page);
                DicConsole.WriteLine("Subpage: {0}", subpage);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("DBD?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out dbd))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            dbd = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("Page control");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", ScsiModeSensePageControl.Changeable,
                                             ScsiModeSensePageControl.Current, ScsiModeSensePageControl.Default,
                                             ScsiModeSensePageControl.Saved);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!Enum.TryParse(strDev, true, out pageControl))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            pageControl = ScsiModeSensePageControl.Current;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Page?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out page))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            page = 0x3F;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Subpage?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out subpage))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            subpage = 0xFF;
                            System.Console.ReadKey();
                            continue;
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending MODE SENSE (6) to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode buffer.");
            DicConsole.WriteLine("3.- Print sense buffer.");
            DicConsole.WriteLine("4.- Decode sense buffer.");
            DicConsole.WriteLine("5.- Send command again.");
            DicConsole.WriteLine("6.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("MODE SENSE (6) response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("MODE SENSE (6) decoded response:");
                    if(buffer != null)
                        DicConsole.WriteLine("{0}", Decoders.SCSI.Modes.PrettifyModeHeader6(buffer, dev.SCSIType));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("MODE SENSE (6) sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("MODE SENSE (6) decoded sense:");
                    DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 5: goto start;
                case 6: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void ModeSense10(string devPath, Device dev)
        {
            bool llba = false;
            bool dbd = false;
            ScsiModeSensePageControl pageControl = ScsiModeSensePageControl.Current;
            byte page = 0x3F;
            byte subpage = 0xFF;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for MODE SENSE (10) command:");
                DicConsole.WriteLine("LLBA?: {0}", llba);
                DicConsole.WriteLine("DBD?: {0}", dbd);
                DicConsole.WriteLine("Page control: {0}", pageControl);
                DicConsole.WriteLine("Page: {0}", page);
                DicConsole.WriteLine("Subpage: {0}", subpage);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("LLBA?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out llba))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            llba = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("DBD?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out dbd))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            dbd = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("Page control");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", ScsiModeSensePageControl.Changeable,
                                             ScsiModeSensePageControl.Current, ScsiModeSensePageControl.Default,
                                             ScsiModeSensePageControl.Saved);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!Enum.TryParse(strDev, true, out pageControl))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            pageControl = ScsiModeSensePageControl.Current;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Page?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out page))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            page = 0x3F;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Subpage?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out subpage))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            subpage = 0xFF;
                            System.Console.ReadKey();
                            continue;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.ModeSense10(out byte[] buffer, out byte[] senseBuffer, llba, dbd, pageControl, page,
                                         subpage, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending MODE SENSE (10) to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode buffer.");
            DicConsole.WriteLine("3.- Print sense buffer.");
            DicConsole.WriteLine("4.- Decode sense buffer.");
            DicConsole.WriteLine("5.- Send command again.");
            DicConsole.WriteLine("6.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("MODE SENSE (10) response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("MODE SENSE (10) decoded response:");
                    if(buffer != null)
                        DicConsole.WriteLine("{0}", Decoders.SCSI.Modes.PrettifyModeHeader10(buffer, dev.SCSIType));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("MODE SENSE (10) sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("MODE SENSE (10) decoded sense:");
                    DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 5: goto start;
                case 6: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void PreventAllowMediumRemoval(string devPath, Device dev)
        {
            ScsiPreventAllowMode mode = ScsiPreventAllowMode.Allow;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for PREVENT ALLOW MEDIUM REMOVAL command:");
                DicConsole.WriteLine("Mode: {0}", mode);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                        return;
                    case 1:
                        DicConsole.WriteLine("Mode");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", ScsiPreventAllowMode.Allow,
                                             ScsiPreventAllowMode.Prevent, ScsiPreventAllowMode.PreventAll,
                                             ScsiPreventAllowMode.PreventChanger);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!Enum.TryParse(strDev, true, out mode))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            mode = ScsiPreventAllowMode.Allow;
                            System.Console.ReadKey();
                            continue;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense =
                dev.SpcPreventAllowMediumRemoval(out byte[] senseBuffer, mode, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending PREVENT ALLOW MEDIUM REMOVAL to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("PREVENT ALLOW MEDIUM REMOVAL decoded sense:");
            DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Send command again.");
            DicConsole.WriteLine("2.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                    return;
                case 1: goto start;
                case 2: goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        static void ReadCapacity10(string devPath, Device dev)
        {
            bool relative = false;
            bool partial = false;
            uint address = 0;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ CAPACITY (10) command:");
                DicConsole.WriteLine("Relative address?: {0}", relative);
                DicConsole.WriteLine("Partial capacity?: {0}", partial);
                DicConsole.WriteLine("Address: {0}", address);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Relative address?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out relative))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            relative = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Partial capacity?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out partial))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            partial = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Address?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out address))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();
                            continue;
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ CAPACITY (10) to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("5.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CAPACITY (10) response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CAPACITY (10) sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CAPACITY (10) decoded sense:");
                    DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
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

        static void ReadCapacity16(string devPath, Device dev)
        {
            bool partial = false;
            ulong address = 0;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ CAPACITY (16) command:");
                DicConsole.WriteLine("Partial capacity?: {0}", partial);
                DicConsole.WriteLine("Address: {0}", address);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Partial capacity?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out partial))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            partial = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Address?: ");
                        strDev = System.Console.ReadLine();
                        if(!ulong.TryParse(strDev, out address))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();
                            continue;
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ CAPACITY (16) to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("5.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CAPACITY (16) response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CAPACITY (16) sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CAPACITY (16) decoded sense:");
                    DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
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

        static void ReadMediaSerialNumber(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.ReadMediaSerialNumber(out byte[] buffer, out byte[] senseBuffer, dev.Timeout,
                                                   out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ MEDIA SERIAL NUMBER to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Print sense buffer.");
            DicConsole.WriteLine("3.- Decode sense buffer.");
            DicConsole.WriteLine("4.- Send command again.");
            DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ MEDIA SERIAL NUMBER response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ MEDIA SERIAL NUMBER sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ MEDIA SERIAL NUMBER decoded sense:");
                    DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
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

        static void RequestSense(string devPath, Device dev)
        {
            bool descriptor = false;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for REQUEST SENSE command:");
                DicConsole.WriteLine("Descriptor: {0}", descriptor);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Descriptor?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out descriptor))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            descriptor = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.RequestSense(descriptor, out byte[] senseBuffer, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending REQUEST SENSE to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("REQUEST SENSE decoded sense:");
            DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("REQUEST SENSE sense:");
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

        static void TestUnitReady(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.ScsiTestUnitReady(out byte[] senseBuffer, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending TEST UNIT READY to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("TEST UNIT READY decoded sense:");
            DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("0.- Return to SCSI Primary Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Primary Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("TEST UNIT READY sense:");
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