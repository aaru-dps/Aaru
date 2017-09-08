// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Smart.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using DiscImageChef.Console;
using DiscImageChef.Devices;
using DiscImageChef.Decoders.ATA;

namespace DiscImageChef.Tests.Devices.ATA
{
    public static class Smart
    {
        public static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a S.M.A.R.T. command to the device:");
                DicConsole.WriteLine("1.- Send DISABLE ATTRIBUTE AUTOSAVE command.");
                DicConsole.WriteLine("2.- Send DISABLE OPERATIONS command.");
                DicConsole.WriteLine("3.- Send ENABLE ATTRIBUTE AUTOSAVE command.");
                DicConsole.WriteLine("4.- Send ENABLE OPERATIONS command.");
                DicConsole.WriteLine("5.- Send EXECUTE OFF-LINE IMMEDIATE command.");
                DicConsole.WriteLine("6.- Send READ DATA command.");
                DicConsole.WriteLine("7.- Send READ LOG command.");
                DicConsole.WriteLine("8.- Send RETURN STATUS command.");
                DicConsole.WriteLine("0.- Return to ATA commands menu.");
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
                        DicConsole.WriteLine("Returning to ATA commands menu...");
                        return;
                    case 1:
                        DisableAttributeAutosave(devPath, dev);
                        continue;
                    case 2:
                        DisableOperations(devPath, dev);
                        continue;
                    case 3:
                        EnableAttributeAutosave(devPath, dev);
                        continue;
                    case 4:
                        EnableOperations(devPath, dev);
                        continue;
                    case 5:
                        ExecuteOfflineImmediate(devPath, dev);
                        continue;
                    case 6:
                        ReadData(devPath, dev);
                        continue;
                    case 7:
                        ReadLog(devPath, dev);
                        continue;
                    case 8:
                        ReturnStatus(devPath, dev);
                        continue;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        public static void DisableAttributeAutosave(string devPath, Device dev)
        {
        start:
            System.Console.Clear();
            bool sense = dev.SmartDisableAttributeAutosave(out AtaErrorRegistersLBA28 errorRegisters, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending DISABLE ATTRIBUTE AUTOSAVE to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("DISABLE ATTRIBUTE AUTOSAVE status registers:");
            DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Send command again.");
            DicConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    DicConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");
                    return;
                case 1:
                    goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        public static void DisableOperations(string devPath, Device dev)
        {
        start:
            System.Console.Clear();
            bool sense = dev.SmartDisable(out AtaErrorRegistersLBA28 errorRegisters, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending DISABLE OPERATIONS to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("DISABLE OPERATIONS status registers:");
            DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Send command again.");
            DicConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    DicConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");
                    return;
                case 1:
                    goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        public static void EnableAttributeAutosave(string devPath, Device dev)
        {
        start:
            System.Console.Clear();
            bool sense = dev.SmartEnableAttributeAutosave(out AtaErrorRegistersLBA28 errorRegisters, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending ENABLE ATTRIBUTE AUTOSAVE to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("ENABLE ATTRIBUTE AUTOSAVE status registers:");
            DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Send command again.");
            DicConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    DicConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");
                    return;
                case 1:
                    goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        public static void EnableOperations(string devPath, Device dev)
        {
        start:
            System.Console.Clear();
            bool sense = dev.SmartEnable(out AtaErrorRegistersLBA28 errorRegisters, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending ENABLE OPERATIONS to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("ENABLE OPERATIONS status registers:");
            DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Send command again.");
            DicConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    DicConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");
                    return;
                case 1:
                    goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        public static void ExecuteOfflineImmediate(string devPath, Device dev)
        {
            byte subcommand = 0;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for EXECUTE OFF-LINE IMMEDIATE command:");
                DicConsole.WriteLine("Subcommand: {0}", subcommand);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");

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
                        DicConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Subcommand?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out subcommand))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            subcommand = 0;
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
            bool sense = dev.SmartExecuteOffLineImmediate(out AtaErrorRegistersLBA28 errorRegisters, subcommand, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending EXECUTE OFF-LINE IMMEDIATE to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("EXECUTE OFF-LINE IMMEDIATE status registers:");
            DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Send command again.");
            DicConsole.WriteLine("2.- Change parameters.");
            DicConsole.WriteLine("0.- Return to Media Card Pass Through commands menu.");
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
                    DicConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");
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

        public static void ReadData(string devPath, Device dev)
        {
        start:
            System.Console.Clear();
            bool sense = dev.SmartReadData(out byte[] buffer, out AtaErrorRegistersLBA28 errorRegisters, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ DATA to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode error registers.");
            DicConsole.WriteLine("3.- Send command again.");
            DicConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    DicConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DATA response:");
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
                    DicConsole.WriteLine("READ DATA status registers:");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    goto start;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        public static void ReadLog(string devPath, Device dev)
        {
            byte address = 0;
            string strDev;
            int item;

        parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ LOG command:");
                DicConsole.WriteLine("Log address: {0}", address);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");

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
                        DicConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("What logical block address?: ");
                        strDev = System.Console.ReadLine();
                        DicConsole.Write("Log address?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out address))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            address = 0;
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
            bool sense = dev.SmartReadLog(out byte[] buffer, out AtaErrorRegistersLBA28 errorRegisters, address, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ LOG to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Buffer is {0} bytes.", buffer == null ? "null" : buffer.Length.ToString());
            DicConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print buffer.");
            DicConsole.WriteLine("2.- Decode error registers.");
            DicConsole.WriteLine("3.- Send command again.");
            DicConsole.WriteLine("4.- Change parameters.");
            DicConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    DicConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ LOG response:");
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
                    DicConsole.WriteLine("READ LOG status registers:");
                    DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    goto start;
                case 4:
                    goto parameters;
                default:
                    DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    goto menu;
            }
        }

        public static void ReturnStatus(string devPath, Device dev)
        {
        start:
            System.Console.Clear();
            bool sense = dev.SmartReturnStatus(out AtaErrorRegistersLBA28 errorRegisters, dev.Timeout, out double duration);

        menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending RETURN STATUS to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("RETURN STATUS status registers:");
            DicConsole.Write("{0}", MainClass.DecodeATARegisters(errorRegisters));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Send command again.");
            DicConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    DicConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");
                    return;
                case 1:
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

