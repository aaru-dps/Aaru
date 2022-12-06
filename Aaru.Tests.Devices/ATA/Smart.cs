// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Smart.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;
using Aaru.Decoders.ATA;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.ATA
{
    internal static class Smart
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Send a S.M.A.R.T. command to the device:");
                AaruConsole.WriteLine("1.- Send DISABLE ATTRIBUTE AUTOSAVE command.");
                AaruConsole.WriteLine("2.- Send DISABLE OPERATIONS command.");
                AaruConsole.WriteLine("3.- Send ENABLE ATTRIBUTE AUTOSAVE command.");
                AaruConsole.WriteLine("4.- Send ENABLE OPERATIONS command.");
                AaruConsole.WriteLine("5.- Send EXECUTE OFF-LINE IMMEDIATE command.");
                AaruConsole.WriteLine("6.- Send READ DATA command.");
                AaruConsole.WriteLine("7.- Send READ LOG command.");
                AaruConsole.WriteLine("8.- Send RETURN STATUS command.");
                AaruConsole.WriteLine("0.- Return to ATA commands menu.");
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
                        AaruConsole.WriteLine("Returning to ATA commands menu...");

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
                        AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();

                        continue;
                }
            }
        }

        static void DisableAttributeAutosave(string devPath, Device dev)
        {
            start:
            System.Console.Clear();

            bool sense =
                dev.SmartDisableAttributeAutosave(out AtaErrorRegistersLba28 errorRegisters, dev.Timeout,
                                                  out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending DISABLE ATTRIBUTE AUTOSAVE to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("DISABLE ATTRIBUTE AUTOSAVE status registers:");
            AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Send command again.");
            AaruConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    AaruConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");

                    return;
                case 1: goto start;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();

                    goto menu;
            }
        }

        static void DisableOperations(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.SmartDisable(out AtaErrorRegistersLba28 errorRegisters, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending DISABLE OPERATIONS to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("DISABLE OPERATIONS status registers:");
            AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Send command again.");
            AaruConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    AaruConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");

                    return;
                case 1: goto start;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();

                    goto menu;
            }
        }

        static void EnableAttributeAutosave(string devPath, Device dev)
        {
            start:
            System.Console.Clear();

            bool sense =
                dev.SmartEnableAttributeAutosave(out AtaErrorRegistersLba28 errorRegisters, dev.Timeout,
                                                 out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending ENABLE ATTRIBUTE AUTOSAVE to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("ENABLE ATTRIBUTE AUTOSAVE status registers:");
            AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Send command again.");
            AaruConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    AaruConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");

                    return;
                case 1: goto start;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();

                    goto menu;
            }
        }

        static void EnableOperations(string devPath, Device dev)
        {
            start:
            System.Console.Clear();
            bool sense = dev.SmartEnable(out AtaErrorRegistersLba28 errorRegisters, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending ENABLE OPERATIONS to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("ENABLE OPERATIONS status registers:");
            AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Send command again.");
            AaruConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    AaruConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");

                    return;
                case 1: goto start;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();

                    goto menu;
            }
        }

        static void ExecuteOfflineImmediate(string devPath, Device dev)
        {
            byte   subcommand = 0;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for EXECUTE OFF-LINE IMMEDIATE command:");
                AaruConsole.WriteLine("Subcommand: {0}", subcommand);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");

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
                        AaruConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Subcommand?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out subcommand))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            subcommand = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.SmartExecuteOffLineImmediate(out AtaErrorRegistersLba28 errorRegisters, subcommand,
                                                          dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending EXECUTE OFF-LINE IMMEDIATE to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("EXECUTE OFF-LINE IMMEDIATE status registers:");
            AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Send command again.");
            AaruConsole.WriteLine("2.- Change parameters.");
            AaruConsole.WriteLine("0.- Return to Media Card Pass Through commands menu.");
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
                    AaruConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");

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

        static void ReadData(string devPath, Device dev)
        {
            start:
            System.Console.Clear();

            bool sense = dev.SmartReadData(out byte[] buffer, out AtaErrorRegistersLba28 errorRegisters, dev.Timeout,
                                           out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ DATA to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
            AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Print buffer.");
            AaruConsole.WriteLine("2.- Decode error registers.");
            AaruConsole.WriteLine("3.- Send command again.");
            AaruConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    AaruConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ DATA response:");

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
                    AaruConsole.WriteLine("READ DATA status registers:");
                    AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
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

        static void ReadLog(string devPath, Device dev)
        {
            byte   address = 0;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ LOG command:");
                AaruConsole.WriteLine("Log address: {0}", address);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");

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
                        AaruConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Log address?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out address))
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

            bool sense = dev.SmartReadLog(out byte[] buffer, out AtaErrorRegistersLba28 errorRegisters, address,
                                          dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ LOG to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("Buffer is {0} bytes.", buffer?.Length.ToString() ?? "null");
            AaruConsole.WriteLine("Buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(buffer));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Print buffer.");
            AaruConsole.WriteLine("2.- Decode error registers.");
            AaruConsole.WriteLine("3.- Send command again.");
            AaruConsole.WriteLine("4.- Change parameters.");
            AaruConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    AaruConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ LOG response:");

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
                    AaruConsole.WriteLine("READ LOG status registers:");
                    AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 3: goto start;
                case 4: goto parameters;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();

                    goto menu;
            }
        }

        static void ReturnStatus(string devPath, Device dev)
        {
            start:
            System.Console.Clear();

            bool sense = dev.SmartReturnStatus(out AtaErrorRegistersLba28 errorRegisters, dev.Timeout,
                                               out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending RETURN STATUS to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("RETURN STATUS status registers:");
            AaruConsole.Write("{0}", MainClass.DecodeAtaRegisters(errorRegisters));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Send command again.");
            AaruConsole.WriteLine("0.- Return to S.M.A.R.T. commands menu.");
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
                    AaruConsole.WriteLine("Returning to S.M.A.R.T. commands menu...");

                    return;
                case 1: goto start;
                default:
                    AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();

                    goto menu;
            }
        }
    }
}