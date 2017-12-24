// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SSC.cs
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
using DiscImageChef.Decoders.SCSI;
using DiscImageChef.Decoders.SCSI.SSC;
using DiscImageChef.Devices;

namespace DiscImageChef.Tests.Devices.SCSI
{
    static class Ssc
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a SCSI Streaming Command to the device:");
                DicConsole.WriteLine("1.- Send LOAD UNLOAD command.");
                DicConsole.WriteLine("2.- Send LOCATE (10) command.");
                DicConsole.WriteLine("3.- Send LOCATE (16) command.");
                DicConsole.WriteLine("4.- Send READ (6) command.");
                DicConsole.WriteLine("5.- Send READ (16) command.");
                DicConsole.WriteLine("6.- Send READ BLOCK LIMITS command.");
                DicConsole.WriteLine("7.- Send READ POSITION command.");
                DicConsole.WriteLine("8.- Send READ REVERSE (6) command.");
                DicConsole.WriteLine("9.- Send READ REVERSE (16) command.");
                DicConsole.WriteLine("10.- Send RECOVER BUFFERED DATA command.");
                DicConsole.WriteLine("11.- Send REPORT DENSITY SUPPORT command.");
                DicConsole.WriteLine("12.- Send REWIND command.");
                DicConsole.WriteLine("13.- Send SPACE command.");
                DicConsole.WriteLine("14.- Send TRACK SELECT command.");
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
                        LoadUnload(devPath, dev);
                        continue;
                    case 2:
                        Locate10(devPath, dev);
                        continue;
                    case 3:
                        Locate16(devPath, dev);
                        continue;
                    case 4:
                        Read6(devPath, dev);
                        continue;
                    case 5:
                        Read16(devPath, dev);
                        continue;
                    case 6:
                        ReadBlockLimits(devPath, dev);
                        continue;
                    case 7:
                        ReadPosition(devPath, dev);
                        continue;
                    case 8:
                        ReadReverse6(devPath, dev);
                        continue;
                    case 9:
                        ReadReverse16(devPath, dev);
                        continue;
                    case 10:
                        RecoverBufferedData(devPath, dev);
                        continue;
                    case 11:
                        ReportDensitySupport(devPath, dev);
                        continue;
                    case 12:
                        Rewind(devPath, dev);
                        continue;
                    case 13:
                        Space(devPath, dev);
                        continue;
                    case 14:
                        TrackSelect(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void LoadUnload(string devPath, Device dev)
        {
            bool load = true;
            bool immediate = false;
            bool retense = false;
            bool eot = false;
            bool hold = false;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for LOAD UNLOAD command:");
                DicConsole.WriteLine("Load?: {0}", load);
                DicConsole.WriteLine("Immediate?: {0}", immediate);
                DicConsole.WriteLine("Retense?: {0}", retense);
                DicConsole.WriteLine("End of tape?: {0}", eot);
                DicConsole.WriteLine("Hold?: {0}", hold);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Load?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out load))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            load = true;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Immediate?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out immediate))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            immediate = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Retense?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out retense))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            retense = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("End of tape?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out eot))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            eot = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Hold?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out hold))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            hold = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.LoadUnload(out byte[] senseBuffer, immediate, load, retense, eot, hold, dev.Timeout,
                                        out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending LOAD UNLOAD to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("LOAD UNLOAD decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("LOAD UNLOAD sense:");
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

        static void Locate10(string devPath, Device dev)
        {
            bool blockType = true;
            bool immediate = false;
            bool changePartition = false;
            byte partition = 0;
            uint objectId = 0;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for LOCATE (10) command:");
                DicConsole.WriteLine("Locate block?: {0}", blockType);
                DicConsole.WriteLine("Immediate?: {0}", immediate);
                DicConsole.WriteLine("Change partition?: {0}", changePartition);
                DicConsole.WriteLine("Partition: {0}", partition);
                DicConsole.WriteLine("{1}: {0}", objectId, blockType ? "Block" : "Object");
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Load?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out blockType))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            blockType = true;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Immediate?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out immediate))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            immediate = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Change partition?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out changePartition))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            changePartition = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Partition?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out partition))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            partition = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("{0}?: ", blockType ? "Block" : "Object");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out objectId))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            objectId = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.Locate(out byte[] senseBuffer, immediate, blockType, changePartition, partition, objectId,
                                    dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending LOCATE (10) to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("LOCATE (10) decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("LOCATE (10) sense:");
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

        static void Locate16(string devPath, Device dev)
        {
            SscLogicalIdTypes destType = SscLogicalIdTypes.FileId;
            bool immediate = false;
            bool changePartition = false;
            bool bam = false;
            byte partition = 0;
            ulong objectId = 1;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for LOCATE (16) command:");
                DicConsole.WriteLine("Object type: {0}", destType);
                DicConsole.WriteLine("Immediate?: {0}", immediate);
                DicConsole.WriteLine("Explicit identifier?: {0}", bam);
                DicConsole.WriteLine("Change partition?: {0}", changePartition);
                DicConsole.WriteLine("Partition: {0}", partition);
                DicConsole.WriteLine("Object identifier: {0}", objectId);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.WriteLine("Object type");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", SscLogicalIdTypes.FileId,
                                             SscLogicalIdTypes.ObjectId, SscLogicalIdTypes.Reserved,
                                             SscLogicalIdTypes.SetId);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!Enum.TryParse(strDev, true, out destType))
                        {
                            DicConsole.WriteLine("Not a correct object type. Press any key to continue...");
                            destType = SscLogicalIdTypes.FileId;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Immediate?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out immediate))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            immediate = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Explicit identifier?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out bam))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            bam = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Change partition?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out changePartition))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            changePartition = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Partition?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out partition))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            partition = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Identifier");
                        strDev = System.Console.ReadLine();
                        if(!ulong.TryParse(strDev, out objectId))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            objectId = 1;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.Locate16(out byte[] senseBuffer, immediate, changePartition, destType, bam, partition,
                                      objectId, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending LOCATE (16) to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("LOCATE (16) decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("LOCATE (16) sense:");
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

        static void Read6(string devPath, Device dev)
        {
            bool sili = false;
            bool fixedLen = true;
            uint blockSize = 512;
            uint length = 1;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ (6) command:");
                DicConsole.WriteLine("Fixed block size?: {0}", fixedLen);
                DicConsole.WriteLine("Will read {0} {1}", length, fixedLen ? "blocks" : "bytes");
                if(fixedLen) DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine("Suppress length indicator?: {0}", sili);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Fixed block size?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fixedLen))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            fixedLen = true;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("How many {0} to read?: ", fixedLen ? "blocks" : "bytes");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out length))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            length = (uint)(fixedLen ? 1 : 512);
                            System.Console.ReadKey();
                            continue;
                        }

                        if(length > 0xFFFFFF)
                        {
                            DicConsole.WriteLine("Max number of {1} is {0}, setting to {0}", 0xFFFFFF,
                                                 fixedLen ? "blocks" : "bytes");
                            length = 0xFFFFFF;
                        }
                        if(fixedLen)
                        {
                            DicConsole.Write("How many bytes to expect per block?: ");
                            strDev = System.Console.ReadLine();
                            if(!uint.TryParse(strDev, out blockSize))
                            {
                                DicConsole.WriteLine("Not a number. Press any key to continue...");
                                blockSize = 512;
                                System.Console.ReadKey();
                                continue;
                            }
                        }

                        DicConsole.Write("Suppress length indicator?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out sili))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sili = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.Read6(out byte[] buffer, out byte[] senseBuffer, sili, fixedLen, length, blockSize,
                                   dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ (6) to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ (6) response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ (6) sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ (6) decoded sense:");
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

        static void Read16(string devPath, Device dev)
        {
            bool sili = false;
            bool fixedLen = true;
            uint objectSize = 512;
            uint length = 1;
            byte partition = 0;
            ulong objectId = 0;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ (16) command:");
                DicConsole.WriteLine("Fixed block size?: {0}", fixedLen);
                DicConsole.WriteLine("Will read {0} {1}", length, fixedLen ? "objects" : "bytes");
                if(fixedLen) DicConsole.WriteLine("{0} bytes expected per object", objectSize);
                DicConsole.WriteLine("Suppress length indicator?: {0}", sili);
                DicConsole.WriteLine("Read object {0} from partition {1}", objectId, partition);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Fixed block size?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fixedLen))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            fixedLen = true;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("How many {0} to read?: ", fixedLen ? "objects" : "bytes");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out length))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            length = (uint)(fixedLen ? 1 : 512);
                            System.Console.ReadKey();
                            continue;
                        }

                        if(length > 0xFFFFFF)
                        {
                            DicConsole.WriteLine("Max number of {1} is {0}, setting to {0}", 0xFFFFFF,
                                                 fixedLen ? "blocks" : "bytes");
                            length = 0xFFFFFF;
                        }
                        if(fixedLen)
                        {
                            DicConsole.Write("How many bytes to expect per object?: ");
                            strDev = System.Console.ReadLine();
                            if(!uint.TryParse(strDev, out objectSize))
                            {
                                DicConsole.WriteLine("Not a number. Press any key to continue...");
                                objectSize = 512;
                                System.Console.ReadKey();
                                continue;
                            }
                        }

                        DicConsole.Write("Suppress length indicator?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out sili))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sili = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Object identifier?: ");
                        strDev = System.Console.ReadLine();
                        if(!ulong.TryParse(strDev, out objectId))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            objectId = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Partition?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out partition))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            partition = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.Read16(out byte[] buffer, out byte[] senseBuffer, sili, fixedLen, partition, objectId,
                                    length, objectSize, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ (16) to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ (16) response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ (16) sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ (16) decoded sense:");
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

        static void ReadBlockLimits(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.ReadBlockLimits(out byte[] buffer, out byte[] senseBuffer, dev.Timeout,
                                             out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ BLOCK LIMITS to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode buffer.");
            DicConsole.WriteLine("3.- Print sense buffer.");
            DicConsole.WriteLine("4.- Decode sense buffer.");
            DicConsole.WriteLine("5.- Send command again.");
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ BLOCK LIMITS response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ BLOCK LIMITS decoded response:");
                    if(buffer != null) DicConsole.WriteLine("{0}", BlockLimits.Prettify(buffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ BLOCK LIMITS sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ BLOCK LIMITS decoded sense:");
                    DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
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

        static void ReadPosition(string devPath, Device dev)
        {
            SscPositionForms responseForm = SscPositionForms.Short;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for LOCATE (16) command:");
                DicConsole.WriteLine("Response form: {0}", responseForm);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.WriteLine("Response form");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3} {4} {5} {6} {7} {8}",
                                             SscPositionForms.Short, SscPositionForms.VendorShort,
                                             SscPositionForms.OldLong, SscPositionForms.OldLongVendor,
                                             SscPositionForms.OldTclp, SscPositionForms.OldTclpVendor,
                                             SscPositionForms.Long, SscPositionForms.OldLongTclpVendor,
                                             SscPositionForms.Extended);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!Enum.TryParse(strDev, true, out responseForm))
                        {
                            DicConsole.WriteLine("Not a correct response form. Press any key to continue...");
                            responseForm = SscPositionForms.Short;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.ReadPosition(out _, out byte[] senseBuffer, responseForm, dev.Timeout,
                                          out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ POSITION to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("READ POSITION decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ POSITION sense:");
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

        static void ReadReverse6(string devPath, Device dev)
        {
            bool byteOrder = false;
            bool sili = false;
            bool fixedLen = true;
            uint blockSize = 512;
            uint length = 1;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ REVERSE (6) command:");
                DicConsole.WriteLine("Fixed block size?: {0}", fixedLen);
                DicConsole.WriteLine("Will read {0} {1}", length, fixedLen ? "blocks" : "bytes");
                if(fixedLen) DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine("Suppress length indicator?: {0}", sili);
                DicConsole.WriteLine("Drive should unreverse bytes?: {0}", byteOrder);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Fixed block size?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fixedLen))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            fixedLen = true;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("How many {0} to read?: ", fixedLen ? "blocks" : "bytes");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out length))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            length = (uint)(fixedLen ? 1 : 512);
                            System.Console.ReadKey();
                            continue;
                        }

                        if(length > 0xFFFFFF)
                        {
                            DicConsole.WriteLine("Max number of {1} is {0}, setting to {0}", 0xFFFFFF,
                                                 fixedLen ? "blocks" : "bytes");
                            length = 0xFFFFFF;
                        }
                        if(fixedLen)
                        {
                            DicConsole.Write("How many bytes to expect per block?: ");
                            strDev = System.Console.ReadLine();
                            if(!uint.TryParse(strDev, out blockSize))
                            {
                                DicConsole.WriteLine("Not a number. Press any key to continue...");
                                blockSize = 512;
                                System.Console.ReadKey();
                                continue;
                            }
                        }

                        DicConsole.Write("Suppress length indicator?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out sili))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sili = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Drive should unreverse bytes?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out byteOrder))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            byteOrder = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.ReadReverse6(out byte[] buffer, out byte[] senseBuffer, byteOrder, sili, fixedLen, length,
                                          blockSize, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ REVERSE (6) to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ REVERSE (6) response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ REVERSE (6) sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ REVERSE (6) decoded sense:");
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

        static void ReadReverse16(string devPath, Device dev)
        {
            bool byteOrder = false;
            bool sili = false;
            bool fixedLen = true;
            uint objectSize = 512;
            uint length = 1;
            byte partition = 0;
            ulong objectId = 0;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ REVERSE (16) command:");
                DicConsole.WriteLine("Fixed block size?: {0}", fixedLen);
                DicConsole.WriteLine("Will read {0} {1}", length, fixedLen ? "objects" : "bytes");
                if(fixedLen) DicConsole.WriteLine("{0} bytes expected per object", objectSize);
                DicConsole.WriteLine("Suppress length indicator?: {0}", sili);
                DicConsole.WriteLine("Read object {0} from partition {1}", objectId, partition);
                DicConsole.WriteLine("Drive should unreverse bytes?: {0}", byteOrder);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Fixed block size?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fixedLen))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            fixedLen = true;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("How many {0} to read?: ", fixedLen ? "objects" : "bytes");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out length))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            length = (uint)(fixedLen ? 1 : 512);
                            System.Console.ReadKey();
                            continue;
                        }

                        if(length > 0xFFFFFF)
                        {
                            DicConsole.WriteLine("Max number of {1} is {0}, setting to {0}", 0xFFFFFF,
                                                 fixedLen ? "blocks" : "bytes");
                            length = 0xFFFFFF;
                        }
                        if(fixedLen)
                        {
                            DicConsole.Write("How many bytes to expect per object?: ");
                            strDev = System.Console.ReadLine();
                            if(!uint.TryParse(strDev, out objectSize))
                            {
                                DicConsole.WriteLine("Not a number. Press any key to continue...");
                                objectSize = 512;
                                System.Console.ReadKey();
                                continue;
                            }
                        }

                        DicConsole.Write("Suppress length indicator?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out sili))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sili = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Object identifier?: ");
                        strDev = System.Console.ReadLine();
                        if(!ulong.TryParse(strDev, out objectId))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            objectId = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Partition?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out partition))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            partition = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Drive should unreverse bytes?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out byteOrder))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            byteOrder = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.ReadReverse16(out byte[] buffer, out byte[] senseBuffer, byteOrder, sili, fixedLen,
                                           partition, objectId, length, objectSize, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ REVERSE (16) to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ REVERSE (16) response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ REVERSE (16) sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ REVERSE (16) decoded sense:");
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

        static void RecoverBufferedData(string devPath, Device dev)
        {
            bool sili = false;
            bool fixedLen = true;
            uint blockSize = 512;
            uint length = 1;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for RECOVER BUFFERED DATA command:");
                DicConsole.WriteLine("Fixed block size?: {0}", fixedLen);
                DicConsole.WriteLine("Will read {0} {1}", length, fixedLen ? "blocks" : "bytes");
                if(fixedLen) DicConsole.WriteLine("{0} bytes expected per block", blockSize);
                DicConsole.WriteLine("Suppress length indicator?: {0}", sili);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Fixed block size?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out fixedLen))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            fixedLen = true;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("How many {0} to read?: ", fixedLen ? "blocks" : "bytes");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out length))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            length = (uint)(fixedLen ? 1 : 512);
                            System.Console.ReadKey();
                            continue;
                        }

                        if(length > 0xFFFFFF)
                        {
                            DicConsole.WriteLine("Max number of {1} is {0}, setting to {0}", 0xFFFFFF,
                                                 fixedLen ? "blocks" : "bytes");
                            length = 0xFFFFFF;
                        }
                        if(fixedLen)
                        {
                            DicConsole.Write("How many bytes to expect per block?: ");
                            strDev = System.Console.ReadLine();
                            if(!uint.TryParse(strDev, out blockSize))
                            {
                                DicConsole.WriteLine("Not a number. Press any key to continue...");
                                blockSize = 512;
                                System.Console.ReadKey();
                                continue;
                            }
                        }

                        DicConsole.Write("Suppress length indicator?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out sili))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sili = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.RecoverBufferedData(out byte[] buffer, out byte[] senseBuffer, sili, fixedLen, length,
                                                 blockSize, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending RECOVER BUFFERED DATA to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("RECOVER BUFFERED DATA response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("RECOVER BUFFERED DATA sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("RECOVER BUFFERED DATA decoded sense:");
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

        static void ReportDensitySupport(string devPath, Device dev)
        {
            bool medium = false;
            bool current = false;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for REPORT DENSITY SUPPORT command:");
                DicConsole.WriteLine("Report about medium types?: {0}", medium);
                DicConsole.WriteLine("Report about current medium?: {0}", current);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Report about medium types?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out medium))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            medium = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Report about current medium?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out current))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            current = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.ReportDensitySupport(out byte[] buffer, out byte[] senseBuffer, medium, current,
                                                  dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending REPORT DENSITY SUPPORT to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode buffer.");
            DicConsole.WriteLine("3.- Print sense buffer.");
            DicConsole.WriteLine("4.- Decode sense buffer.");
            DicConsole.WriteLine("5.- Send command again.");
            DicConsole.WriteLine("6.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("REPORT DENSITY SUPPORT response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("REPORT DENSITY SUPPORT decoded buffer:");
                    DicConsole.Write("{0}",
                                     medium
                                         ? DensitySupport.PrettifyMediumType(buffer)
                                         : DensitySupport.PrettifyDensity(buffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("REPORT DENSITY SUPPORT sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("REPORT DENSITY SUPPORT decoded sense:");
                    DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
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

        static void Rewind(string devPath, Device dev)
        {
            bool immediate = false;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for REWIND command:");
                DicConsole.WriteLine("Immediate?: {0}", immediate);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Immediate?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out immediate))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            immediate = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.Rewind(out byte[] senseBuffer, immediate, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending REWIND to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("REWIND decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("REWIND sense:");
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

        static void Space(string devPath, Device dev)
        {
            SscSpaceCodes what = SscSpaceCodes.LogicalBlock;
            int count = -1;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for SPACE command:");
                DicConsole.WriteLine("What to space: {0}", what);
                DicConsole.WriteLine("How many: {0}", count);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.WriteLine("What to space");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3} {4} {5}", SscSpaceCodes.LogicalBlock,
                                             SscSpaceCodes.Filemark, SscSpaceCodes.SequentialFilemark,
                                             SscSpaceCodes.EndOfData, SscSpaceCodes.Obsolete1, SscSpaceCodes.Obsolete2);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!Enum.TryParse(strDev, true, out what))
                        {
                            DicConsole.WriteLine("Not a correct object type. Press any key to continue...");
                            what = SscSpaceCodes.LogicalBlock;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("How many (negative for reverse)?: ");
                        strDev = System.Console.ReadLine();
                        if(!int.TryParse(strDev, out count))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            count = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.Space(out byte[] senseBuffer, what, count, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending SPACE to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("SPACE decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("SPACE sense:");
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

        static void TrackSelect(string devPath, Device dev)
        {
            byte track = 1;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for TRACK SELECT command:");
                DicConsole.WriteLine("Track: {0}", track);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Track?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out track))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            track = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();
            bool sense = dev.TrackSelect(out byte[] senseBuffer, track, dev.Timeout, out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending TRACK SELECT to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("TRACK SELECT decoded sense:");
            DicConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI Streaming Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI Streaming Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("TRACK SELECT sense:");
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
    }
}