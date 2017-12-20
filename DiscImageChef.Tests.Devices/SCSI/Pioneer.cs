// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Pioneer.cs
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
    static class Pioneer
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a Pioneer vendor command to the device:");
                DicConsole.WriteLine("1.- Send READ CD-DA command.");
                DicConsole.WriteLine("2.- Send READ CD-DA MSF command.");
                DicConsole.WriteLine("3.- Send READ CD-XA command.");
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
                        ReadCdDa(devPath, dev);
                        continue;
                    case 2:
                        ReadCdDaMsf(devPath, dev);
                        continue;
                    case 3:
                        ReadCdXa(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void ReadCdDa(string devPath, Device dev)
        {
            uint address = 0;
            uint length = 1;
            PioneerSubchannel subchan = PioneerSubchannel.None;
            uint blockSize = 2352;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ CD-DA command:");
                DicConsole.WriteLine("LBA: {0}", address);
                DicConsole.WriteLine("Will transfer {0} sectors", length);
                DicConsole.WriteLine("Subchannel mode: {0}", subchan);
                DicConsole.WriteLine("{0} bytes per sectors", blockSize);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to Pioneer vendor commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Logical Block Address?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out address))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("How many sectors to transfer?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out length))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            length = 1;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("Subchannel mode");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", PioneerSubchannel.None,
                                             PioneerSubchannel.Q16, PioneerSubchannel.All, PioneerSubchannel.Only);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out subchan))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            subchan = PioneerSubchannel.None;
                            System.Console.ReadKey();
                            continue;
                        }

                        switch(subchan)
                        {
                            case PioneerSubchannel.Q16:
                                blockSize = 2368;
                                break;
                            case PioneerSubchannel.All:
                                blockSize = 2448;
                                break;
                            case PioneerSubchannel.Only:
                                blockSize = 96;
                                break;
                            default:
                                blockSize = 2352;
                                break;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.PioneerReadCdDa(out byte[] buffer, out byte[] senseBuffer, address, blockSize, length,
                                             subchan, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ CD-DA to the device:");
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
            DicConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Pioneer vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD-DA response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD-DA sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD-DA decoded sense:");
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

        static void ReadCdDaMsf(string devPath, Device dev)
        {
            byte startFrame = 0;
            byte startSecond = 2;
            byte startMinute = 0;
            byte endFrame = 0;
            byte endSecond = 0;
            byte endMinute = 0;
            PioneerSubchannel subchan = PioneerSubchannel.None;
            uint blockSize = 2352;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ CD-DA MSF command:");
                DicConsole.WriteLine("Start: {0:D2}:{1:D2}:{2:D2}", startMinute, startSecond, startFrame);
                DicConsole.WriteLine("End: {0:D2}:{1:D2}:{2:D2}", endMinute, endSecond, endFrame);
                DicConsole.WriteLine("Subchannel mode: {0}", subchan);
                DicConsole.WriteLine("{0} bytes per sectors", blockSize);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to Pioneer vendor commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Start minute?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out startMinute))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            startMinute = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Start second?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out startSecond))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            startSecond = 2;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Start frame?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out startFrame))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            startFrame = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("End minute?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out endMinute))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            endMinute = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("End second?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out endMinute))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            endMinute = 2;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("End frame?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out endFrame))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            endFrame = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("Subchannel mode");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", PioneerSubchannel.None,
                                             PioneerSubchannel.Q16, PioneerSubchannel.All, PioneerSubchannel.Only);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out subchan))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            subchan = PioneerSubchannel.None;
                            System.Console.ReadKey();
                            continue;
                        }

                        switch(subchan)
                        {
                            case PioneerSubchannel.Q16:
                                blockSize = 2368;
                                break;
                            case PioneerSubchannel.All:
                                blockSize = 2448;
                                break;
                            case PioneerSubchannel.Only:
                                blockSize = 96;
                                break;
                            default:
                                blockSize = 2352;
                                break;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            uint startMsf = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
            uint endMsf = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
            System.Console.Clear();
            bool sense = dev.PioneerReadCdDaMsf(out byte[] buffer, out byte[] senseBuffer, startMsf, endMsf, blockSize,
                                                subchan, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ CD-DA MSF to the device:");
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
            DicConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Pioneer vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD-DA MSF response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD-DA MSF sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD-DA MSF decoded sense:");
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

        static void ReadCdXa(string devPath, Device dev)
        {
            uint address = 0;
            uint length = 1;
            bool errorFlags = false;
            bool wholeSector = false;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ CD-XA command:");
                DicConsole.WriteLine("LBA: {0}", address);
                DicConsole.WriteLine("Will transfer {0} sectors", length);
                DicConsole.WriteLine("Include error flags?: {0}", errorFlags);
                DicConsole.WriteLine("Whole sector?: {0}", wholeSector);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");

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
                        DicConsole.WriteLine("Returning to Pioneer vendor commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Logical Block Address?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out address))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("How many sectors to transfer?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out length))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            length = 1;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Include error flags?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out errorFlags))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            errorFlags = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Read whole sector?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out wholeSector))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            wholeSector = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.PioneerReadCdXa(out byte[] buffer, out byte[] senseBuffer, address, length, errorFlags,
                                             wholeSector, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ CD-XA to the device:");
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
            DicConsole.WriteLine("0.- Return to Pioneer vendor commands menu.");
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
                    DicConsole.WriteLine("Returning to Pioneer vendor commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD-XA response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD-XA sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD-XA decoded sense:");
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
    }
}