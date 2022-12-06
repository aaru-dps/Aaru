// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
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

using System;
using Aaru.Console;
using Aaru.Decoders.CD;
using Aaru.Decoders.SCSI;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices.SCSI
{
    internal static class Mmc
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Send a MultiMedia Command to the device:");
                AaruConsole.WriteLine("1.- Send GET CONFIGURATION command.");
                AaruConsole.WriteLine("2.- Send PREVENT ALLOW MEDIUM REMOVAL command.");
                AaruConsole.WriteLine("3.- Send READ CD command.");
                AaruConsole.WriteLine("4.- Send READ CD MSF command.");
                AaruConsole.WriteLine("5.- Send READ DISC INFORMATION command.");
                AaruConsole.WriteLine("6.- Send READ DISC STRUCTURE command.");
                AaruConsole.WriteLine("7.- Send READ TOC/PMA/ATIP command.");
                AaruConsole.WriteLine("8.- Send START STOP UNIT command.");
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
                        GetConfiguration(devPath, dev);

                        continue;
                    case 2:
                        PreventAllowMediumRemoval(devPath, dev);

                        continue;
                    case 3:
                        ReadCd(devPath, dev);

                        continue;
                    case 4:
                        ReadCdMsf(devPath, dev);

                        continue;
                    case 5:
                        ReadDiscInformation(devPath, dev);

                        continue;
                    case 6:
                        ReadDiscStructure(devPath, dev);

                        continue;
                    case 7:
                        ReadTocPmaAtip(devPath, dev);

                        continue;
                    case 8:
                        StartStopUnit(devPath, dev);

                        continue;
                    default:
                        AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();

                        continue;
                }
            }
        }

        static void GetConfiguration(string devPath, Device dev)
        {
            MmcGetConfigurationRt rt                    = MmcGetConfigurationRt.All;
            ushort                startingFeatureNumber = 0;
            string                strDev;
            int                   item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for GET CONFIGURATION command:");
                AaruConsole.WriteLine("RT: {0}", rt);
                AaruConsole.WriteLine("Feature number: {0}", startingFeatureNumber);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                        return;
                    case 1:
                        AaruConsole.WriteLine("RT");

                        AaruConsole.WriteLine("Available values: {0} {1} {2} {3}", MmcGetConfigurationRt.All,
                                              MmcGetConfigurationRt.Current, MmcGetConfigurationRt.Reserved,
                                              MmcGetConfigurationRt.Single);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out rt))
                        {
                            AaruConsole.WriteLine("Not a correct object type. Press any key to continue...");
                            rt = MmcGetConfigurationRt.All;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Feature number");
                        strDev = System.Console.ReadLine();

                        if(!ushort.TryParse(strDev, out startingFeatureNumber))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            startingFeatureNumber = 1;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.GetConfiguration(out byte[] buffer, out byte[] senseBuffer, startingFeatureNumber, rt,
                                              dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending GET CONFIGURATION to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("GET CONFIGURATION buffer:");

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
                    AaruConsole.WriteLine("GET CONFIGURATION decoded buffer:");

                    if(buffer != null)
                    {
                        Features.SeparatedFeatures ftr = Features.Separate(buffer);
                        AaruConsole.WriteLine("GET CONFIGURATION length is {0} bytes", ftr.DataLength);
                        AaruConsole.WriteLine("GET CONFIGURATION current profile is {0:X4}h", ftr.CurrentProfile);

                        if(ftr.Descriptors != null)
                            foreach(Features.FeatureDescriptor desc in ftr.Descriptors)
                            {
                                AaruConsole.WriteLine("Feature {0:X4}h", desc.Code);

                                switch(desc.Code)
                                {
                                    case 0x0000:
                                        AaruConsole.Write("{0}", Features.Prettify_0000(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0001:
                                        AaruConsole.Write("{0}", Features.Prettify_0001(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0002:
                                        AaruConsole.Write("{0}", Features.Prettify_0002(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0003:
                                        AaruConsole.Write("{0}", Features.Prettify_0003(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0004:
                                        AaruConsole.Write("{0}", Features.Prettify_0004(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0010:
                                        AaruConsole.Write("{0}", Features.Prettify_0010(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x001D:
                                        AaruConsole.Write("{0}", Features.Prettify_001D(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x001E:
                                        AaruConsole.Write("{0}", Features.Prettify_001E(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x001F:
                                        AaruConsole.Write("{0}", Features.Prettify_001F(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0020:
                                        AaruConsole.Write("{0}", Features.Prettify_0020(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0021:
                                        AaruConsole.Write("{0}", Features.Prettify_0021(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0022:
                                        AaruConsole.Write("{0}", Features.Prettify_0022(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0023:
                                        AaruConsole.Write("{0}", Features.Prettify_0023(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0024:
                                        AaruConsole.Write("{0}", Features.Prettify_0024(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0025:
                                        AaruConsole.Write("{0}", Features.Prettify_0025(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0026:
                                        AaruConsole.Write("{0}", Features.Prettify_0026(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0027:
                                        AaruConsole.Write("{0}", Features.Prettify_0027(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0028:
                                        AaruConsole.Write("{0}", Features.Prettify_0028(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0029:
                                        AaruConsole.Write("{0}", Features.Prettify_0029(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x002A:
                                        AaruConsole.Write("{0}", Features.Prettify_002A(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x002B:
                                        AaruConsole.Write("{0}", Features.Prettify_002B(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x002C:
                                        AaruConsole.Write("{0}", Features.Prettify_002C(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x002D:
                                        AaruConsole.Write("{0}", Features.Prettify_002D(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x002E:
                                        AaruConsole.Write("{0}", Features.Prettify_002E(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x002F:
                                        AaruConsole.Write("{0}", Features.Prettify_002F(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0030:
                                        AaruConsole.Write("{0}", Features.Prettify_0030(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0031:
                                        AaruConsole.Write("{0}", Features.Prettify_0031(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0032:
                                        AaruConsole.Write("{0}", Features.Prettify_0032(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0033:
                                        AaruConsole.Write("{0}", Features.Prettify_0033(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0035:
                                        AaruConsole.Write("{0}", Features.Prettify_0035(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0037:
                                        AaruConsole.Write("{0}", Features.Prettify_0037(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0038:
                                        AaruConsole.Write("{0}", Features.Prettify_0038(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x003A:
                                        AaruConsole.Write("{0}", Features.Prettify_003A(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x003B:
                                        AaruConsole.Write("{0}", Features.Prettify_003B(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0040:
                                        AaruConsole.Write("{0}", Features.Prettify_0040(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0041:
                                        AaruConsole.Write("{0}", Features.Prettify_0041(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0042:
                                        AaruConsole.Write("{0}", Features.Prettify_0042(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0050:
                                        AaruConsole.Write("{0}", Features.Prettify_0050(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0051:
                                        AaruConsole.Write("{0}", Features.Prettify_0051(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0080:
                                        AaruConsole.Write("{0}", Features.Prettify_0080(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0100:
                                        AaruConsole.Write("{0}", Features.Prettify_0100(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0101:
                                        AaruConsole.Write("{0}", Features.Prettify_0101(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0102:
                                        AaruConsole.Write("{0}", Features.Prettify_0102(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0103:
                                        AaruConsole.Write("{0}", Features.Prettify_0103(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0104:
                                        AaruConsole.Write("{0}", Features.Prettify_0104(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0105:
                                        AaruConsole.Write("{0}", Features.Prettify_0105(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0106:
                                        AaruConsole.Write("{0}", Features.Prettify_0106(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0107:
                                        AaruConsole.Write("{0}", Features.Prettify_0107(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0108:
                                        AaruConsole.Write("{0}", Features.Prettify_0108(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0109:
                                        AaruConsole.Write("{0}", Features.Prettify_0109(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x010A:
                                        AaruConsole.Write("{0}", Features.Prettify_010A(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x010B:
                                        AaruConsole.Write("{0}", Features.Prettify_010B(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x010C:
                                        AaruConsole.Write("{0}", Features.Prettify_010C(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x010D:
                                        AaruConsole.Write("{0}", Features.Prettify_010D(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x010E:
                                        AaruConsole.Write("{0}", Features.Prettify_010E(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0110:
                                        AaruConsole.Write("{0}", Features.Prettify_0110(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0113:
                                        AaruConsole.Write("{0}", Features.Prettify_0113(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    case 0x0142:
                                        AaruConsole.Write("{0}", Features.Prettify_0142(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                    default:
                                        AaruConsole.WriteLine("Don't know how to decode feature 0x{0:X4}", desc.Code);
                                        PrintHex.PrintHexArray(desc.Data, 64);

                                        break;
                                }
                            }
                    }

                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 3:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("GET CONFIGURATION sense:");

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
                    AaruConsole.WriteLine("GET CONFIGURATION decoded sense:");

                    if(senseBuffer != null)
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
            bool   prevent    = false;
            bool   persistent = false;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for PREVENT ALLOW MEDIUM REMOVAL command:");
                AaruConsole.WriteLine("Prevent removal?: {0}", prevent);
                AaruConsole.WriteLine("Persistent value?: {0}", persistent);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Prevent removal?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out prevent))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            prevent = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Persistent value?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out persistent))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            persistent = false;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.PreventAllowMediumRemoval(out byte[] senseBuffer, persistent, prevent, dev.Timeout,
                                                       out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending PREVENT ALLOW MEDIUM REMOVAL to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            AaruConsole.WriteLine("PREVENT ALLOW MEDIUM REMOVAL decoded sense:");
            AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Print sense buffer.");
            AaruConsole.WriteLine("2.- Send command again.");
            AaruConsole.WriteLine("3.- Change parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("PREVENT ALLOW MEDIUM REMOVAL sense:");

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

        static void ReadCd(string devPath, Device dev)
        {
            uint           address    = 0;
            uint           length     = 1;
            MmcSectorTypes sectorType = MmcSectorTypes.AllTypes;
            bool           dap        = false;
            bool           relative   = false;
            bool           sync       = false;
            MmcHeaderCodes header     = MmcHeaderCodes.None;
            bool           user       = true;
            bool           edc        = false;
            MmcErrorField  c2         = MmcErrorField.None;
            MmcSubchannel  subchan    = MmcSubchannel.None;
            uint           blockSize  = 2352;
            string         strDev;
            int            item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ CD command:");
                AaruConsole.WriteLine("Address relative to current position?: {0}", relative);
                AaruConsole.WriteLine("{1}: {0}", address, relative ? "Address" : "LBA");
                AaruConsole.WriteLine("Will transfer {0} sectors", length);
                AaruConsole.WriteLine("Sector type: {0}", sectorType);
                AaruConsole.WriteLine("Process audio?: {0}", dap);
                AaruConsole.WriteLine("Retrieve sync bytes?: {0}", sync);
                AaruConsole.WriteLine("Header mode: {0}", header);
                AaruConsole.WriteLine("Retrieve user data?: {0}", user);
                AaruConsole.WriteLine("Retrieve EDC/ECC data?: {0}", edc);
                AaruConsole.WriteLine("C2 mode: {0}", c2);
                AaruConsole.WriteLine("Subchannel mode: {0}", subchan);
                AaruConsole.WriteLine("{0} bytes per sector", blockSize);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Address is relative to current position?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out relative))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            relative = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("{0}?: ", relative ? "Address" : "LBA");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out address))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            address = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("How many sectors to transfer?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out length))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            length = 1;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.WriteLine("Sector type");

                        AaruConsole.WriteLine("Available values: {0} {1} {2} {3} {4} {5}", MmcSectorTypes.AllTypes,
                                              MmcSectorTypes.Cdda, MmcSectorTypes.Mode1, MmcSectorTypes.Mode2,
                                              MmcSectorTypes.Mode2Form1, MmcSectorTypes.Mode2Form2);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out sectorType))
                        {
                            AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            sectorType = MmcSectorTypes.AllTypes;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Process audio?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out dap))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            dap = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Retrieve sync bytes?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out sync))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sync = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.WriteLine("Header mode");

                        AaruConsole.WriteLine("Available values: {0} {1} {2} {3}", MmcHeaderCodes.None,
                                              MmcHeaderCodes.HeaderOnly, MmcHeaderCodes.SubHeaderOnly,
                                              MmcHeaderCodes.AllHeaders);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out header))
                        {
                            AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            header = MmcHeaderCodes.None;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Retrieve user data?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out user))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            user = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Retrieve EDC/ECC?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out edc))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            edc = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.WriteLine("C2 mode");

                        AaruConsole.WriteLine("Available values: {0} {1} {2}", MmcErrorField.None,
                                              MmcErrorField.C2Pointers, MmcErrorField.C2PointersAndBlock);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out c2))
                        {
                            AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            c2 = MmcErrorField.None;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.WriteLine("Subchannel mode");

                        AaruConsole.WriteLine("Available values: {0} {1} {2} {3}", MmcSubchannel.None,
                                              MmcSubchannel.Raw, MmcSubchannel.Q16, MmcSubchannel.Rw);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out subchan))
                        {
                            AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            subchan = MmcSubchannel.None;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Expected block size?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 2352;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadCd(out byte[] buffer, out byte[] senseBuffer, address, blockSize, length, sectorType,
                                    dap, relative, sync, header, user, edc, c2, subchan, dev.Timeout,
                                    out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ CD to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ CD response:");

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
                    AaruConsole.WriteLine("READ CD sense:");

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
                    AaruConsole.WriteLine("READ CD decoded sense:");
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

        static void ReadCdMsf(string devPath, Device dev)
        {
            byte           startFrame  = 0;
            byte           startSecond = 2;
            byte           startMinute = 0;
            byte           endFrame    = 0;
            byte           endSecond   = 0;
            byte           endMinute   = 0;
            MmcSectorTypes sectorType  = MmcSectorTypes.AllTypes;
            bool           dap         = false;
            bool           sync        = false;
            MmcHeaderCodes header      = MmcHeaderCodes.None;
            bool           user        = true;
            bool           edc         = false;
            MmcErrorField  c2          = MmcErrorField.None;
            MmcSubchannel  subchan     = MmcSubchannel.None;
            uint           blockSize   = 2352;
            string         strDev;
            int            item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ CD MSF command:");
                AaruConsole.WriteLine("Start: {0:D2}:{1:D2}:{2:D2}", startMinute, startSecond, startFrame);
                AaruConsole.WriteLine("End: {0:D2}:{1:D2}:{2:D2}", endMinute, endSecond, endFrame);
                AaruConsole.WriteLine("Sector type: {0}", sectorType);
                AaruConsole.WriteLine("Process audio?: {0}", dap);
                AaruConsole.WriteLine("Retrieve sync bytes?: {0}", sync);
                AaruConsole.WriteLine("Header mode: {0}", header);
                AaruConsole.WriteLine("Retrieve user data?: {0}", user);
                AaruConsole.WriteLine("Retrieve EDC/ECC data?: {0}", edc);
                AaruConsole.WriteLine("C2 mode: {0}", c2);
                AaruConsole.WriteLine("Subchannel mode: {0}", subchan);
                AaruConsole.WriteLine("{0} bytes per sector", blockSize);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Start minute?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out startMinute))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            startMinute = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Start second?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out startSecond))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            startSecond = 2;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Start frame?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out startFrame))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            startFrame = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("End minute?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out endMinute))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            endMinute = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("End second?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out endMinute))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            endMinute = 2;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("End frame?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out endFrame))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            endFrame = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.WriteLine("Sector type");

                        AaruConsole.WriteLine("Available values: {0} {1} {2} {3} {4} {5}", MmcSectorTypes.AllTypes,
                                              MmcSectorTypes.Cdda, MmcSectorTypes.Mode1, MmcSectorTypes.Mode2,
                                              MmcSectorTypes.Mode2Form1, MmcSectorTypes.Mode2Form2);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out sectorType))
                        {
                            AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            sectorType = MmcSectorTypes.AllTypes;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Process audio?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out dap))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            dap = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Retrieve sync bytes?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out sync))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sync = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.WriteLine("Header mode");

                        AaruConsole.WriteLine("Available values: {0} {1} {2} {3}", MmcHeaderCodes.None,
                                              MmcHeaderCodes.HeaderOnly, MmcHeaderCodes.SubHeaderOnly,
                                              MmcHeaderCodes.AllHeaders);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out header))
                        {
                            AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            header = MmcHeaderCodes.None;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Retrieve user data?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out user))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            user = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Retrieve EDC/ECC?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out edc))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            edc = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.WriteLine("C2 mode");

                        AaruConsole.WriteLine("Available values: {0} {1} {2}", MmcErrorField.None,
                                              MmcErrorField.C2Pointers, MmcErrorField.C2PointersAndBlock);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out c2))
                        {
                            AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            c2 = MmcErrorField.None;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.WriteLine("Subchannel mode");

                        AaruConsole.WriteLine("Available values: {0} {1} {2} {3}", MmcSubchannel.None,
                                              MmcSubchannel.Raw, MmcSubchannel.Q16, MmcSubchannel.Rw);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out subchan))
                        {
                            AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            subchan = MmcSubchannel.None;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Expected block size?: ");
                        strDev = System.Console.ReadLine();

                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 2352;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            uint startMsf = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
            uint endMsf   = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
            System.Console.Clear();

            bool sense = dev.ReadCdMsf(out byte[] buffer, out byte[] senseBuffer, startMsf, endMsf, blockSize,
                                       sectorType, dap, sync, header, user, edc, c2, subchan, dev.Timeout,
                                       out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ CD MSF to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ CD MSF response:");

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
                    AaruConsole.WriteLine("READ CD MSF sense:");

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
                    AaruConsole.WriteLine("READ CD MSF decoded sense:");
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

        static void ReadDiscInformation(string devPath, Device dev)
        {
            MmcDiscInformationDataTypes info = MmcDiscInformationDataTypes.DiscInformation;
            string                      strDev;
            int                         item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ DISC INFORMATION command:");
                AaruConsole.WriteLine("Information type: {0}", info);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                        return;
                    case 1:
                        AaruConsole.WriteLine("Information type");

                        AaruConsole.WriteLine("Available values: {0} {1} {2}",
                                              MmcDiscInformationDataTypes.DiscInformation,
                                              MmcDiscInformationDataTypes.TrackResources,
                                              MmcDiscInformationDataTypes.PowResources);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out info))
                        {
                            AaruConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            info = MmcDiscInformationDataTypes.DiscInformation;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadDiscInformation(out byte[] buffer, out byte[] senseBuffer, info, dev.Timeout,
                                                 out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ DISC INFORMATION to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ DISC INFORMATION response:");

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
                    AaruConsole.WriteLine("READ DISC INFORMATION decoded response:");
                    AaruConsole.Write("{0}", DiscInformation.Prettify(buffer));
                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 3:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ DISC INFORMATION sense:");

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
                    AaruConsole.WriteLine("READ DISC INFORMATION decoded sense:");
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

        static void ReadDiscStructure(string devPath, Device dev)
        {
            MmcDiscStructureMediaType mediaType = MmcDiscStructureMediaType.Dvd;
            MmcDiscStructureFormat    format    = MmcDiscStructureFormat.CapabilityList;
            uint                      address   = 0;
            byte                      layer     = 0;
            byte                      agid      = 0;
            string                    strDev;
            int                       item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ DISC STRUCTURE command:");
                AaruConsole.WriteLine("Media type: {0}", mediaType);
                AaruConsole.WriteLine("Format: {0}", format);
                AaruConsole.WriteLine("Address: {0}", address);
                AaruConsole.WriteLine("Layer: {0}", layer);
                AaruConsole.WriteLine("AGID: {0}", agid);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                        return;
                    case 1:
                        AaruConsole.WriteLine("Media type");

                        AaruConsole.WriteLine("Available values: {0} {1}", MmcDiscStructureMediaType.Dvd,
                                              MmcDiscStructureMediaType.Bd);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out mediaType))
                        {
                            AaruConsole.WriteLine("Not a correct media type. Press any key to continue...");
                            mediaType = MmcDiscStructureMediaType.Dvd;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.WriteLine("Format");
                        AaruConsole.WriteLine("Available values:");

                        switch(mediaType)
                        {
                            case MmcDiscStructureMediaType.Dvd:
                                AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.PhysicalInformation,
                                                      MmcDiscStructureFormat.CopyrightInformation,
                                                      MmcDiscStructureFormat.DiscKey,
                                                      MmcDiscStructureFormat.BurstCuttingArea);

                                AaruConsole.WriteLine("\t{0} {1} {2} {3}",
                                                      MmcDiscStructureFormat.DiscManufacturingInformation,
                                                      MmcDiscStructureFormat.SectorCopyrightInformation,
                                                      MmcDiscStructureFormat.MediaIdentifier,
                                                      MmcDiscStructureFormat.MediaKeyBlock);

                                AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.DvdramDds,
                                                      MmcDiscStructureFormat.DvdramMediumStatus,
                                                      MmcDiscStructureFormat.DvdramSpareAreaInformation,
                                                      MmcDiscStructureFormat.DvdramRecordingType);

                                AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.LastBorderOutRmd,
                                                      MmcDiscStructureFormat.SpecifiedRmd,
                                                      MmcDiscStructureFormat.PreRecordedInfo,
                                                      MmcDiscStructureFormat.DvdrMediaIdentifier);

                                AaruConsole.WriteLine("\t{0} {1} {2} {3}",
                                                      MmcDiscStructureFormat.DvdrPhysicalInformation,
                                                      MmcDiscStructureFormat.Adip,
                                                      MmcDiscStructureFormat.HddvdCopyrightInformation,
                                                      MmcDiscStructureFormat.DvdAacs);

                                AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.HddvdrMediumStatus,
                                                      MmcDiscStructureFormat.HddvdrLastRmd,
                                                      MmcDiscStructureFormat.DvdrLayerCapacity,
                                                      MmcDiscStructureFormat.MiddleZoneStart);

                                AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.JumpIntervalSize,
                                                      MmcDiscStructureFormat.ManualLayerJumpStartLba,
                                                      MmcDiscStructureFormat.RemapAnchorPoint,
                                                      MmcDiscStructureFormat.Dcb);

                                break;
                            case MmcDiscStructureMediaType.Bd:
                                AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.DiscInformation,
                                                      MmcDiscStructureFormat.BdBurstCuttingArea,
                                                      MmcDiscStructureFormat.BdDds,
                                                      MmcDiscStructureFormat.CartridgeStatus);

                                AaruConsole.WriteLine("\t{0} {1} {2}", MmcDiscStructureFormat.BdSpareAreaInformation,
                                                      MmcDiscStructureFormat.RawDfl, MmcDiscStructureFormat.Pac);

                                break;
                        }

                        AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.AacsVolId,
                                              MmcDiscStructureFormat.AacsMediaSerial,
                                              MmcDiscStructureFormat.AacsMediaId, MmcDiscStructureFormat.Aacsmkb);

                        AaruConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.AacsDataKeys,
                                              MmcDiscStructureFormat.AacslbaExtents, MmcDiscStructureFormat.Aacsmkbcprm,
                                              MmcDiscStructureFormat.RecognizedFormatLayers);

                        AaruConsole.WriteLine("\t{0} {1}", MmcDiscStructureFormat.WriteProtectionStatus,
                                              MmcDiscStructureFormat.CapabilityList);

                        AaruConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();

                        if(!Enum.TryParse(strDev, true, out format))
                        {
                            AaruConsole.WriteLine("Not a correct media type. Press any key to continue...");
                            format = MmcDiscStructureFormat.CapabilityList;
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

                            continue;
                        }

                        AaruConsole.Write("Layer?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out layer))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            layer = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("AGID?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out agid))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            agid = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadDiscStructure(out byte[] buffer, out byte[] senseBuffer, mediaType, address, layer,
                                               format, agid, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ DISC STRUCTURE to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ DISC STRUCTURE response:");

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

                    // TODO: Implement
                    AaruConsole.WriteLine("READ DISC STRUCTURE decoding not yet implemented:");
                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 3:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ DISC STRUCTURE sense:");

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
                    AaruConsole.WriteLine("READ DISC STRUCTURE decoded sense:");
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

        static void ReadTocPmaAtip(string devPath, Device dev)
        {
            bool   msf     = false;
            byte   format  = 0;
            byte   session = 0;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for READ TOC/PMA/ATIP command:");
                AaruConsole.WriteLine("Return MSF values?: {0}", msf);
                AaruConsole.WriteLine("Format byte: {0}", format);
                AaruConsole.WriteLine("Session: {0}", session);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Return MSF values?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out msf))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            msf = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Format?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out format))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            format = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Session?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out session))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            session = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.ReadTocPmaAtip(out byte[] buffer, out byte[] senseBuffer, msf, format, session,
                                            dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending READ TOC/PMA/ATIP to the device:");
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
            AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ TOC/PMA/ATIP buffer:");

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
                    AaruConsole.WriteLine("READ TOC/PMA/ATIP decoded buffer:");

                    if(buffer != null)
                        switch(format)
                        {
                            case 0:
                                AaruConsole.Write("{0}", TOC.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);

                                break;
                            case 1:
                                AaruConsole.Write("{0}", Session.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);

                                break;
                            case 2:
                                AaruConsole.Write("{0}", FullTOC.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);

                                break;
                            case 3:
                                AaruConsole.Write("{0}", PMA.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);

                                break;
                            case 4:
                                AaruConsole.Write("{0}", ATIP.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);

                                break;
                            case 5:
                                AaruConsole.Write("{0}", CDTextOnLeadIn.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);

                                break;
                        }

                    AaruConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);

                    goto menu;
                case 3:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("READ TOC/PMA/ATIP sense:");

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
                    AaruConsole.WriteLine("READ TOC/PMA/ATIP decoded sense:");

                    if(senseBuffer != null)
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

        static void StartStopUnit(string devPath, Device dev)
        {
            bool   immediate         = false;
            bool   changeFormatLayer = false;
            bool   loadEject         = false;
            bool   start             = false;
            byte   formatLayer       = 0;
            byte   powerConditions   = 0;
            string strDev;
            int    item;

            parameters:

            while(true)
            {
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Parameters for START STOP UNIT command:");
                AaruConsole.WriteLine("Immediate?: {0}", immediate);
                AaruConsole.WriteLine("Change format layer?: {0}", changeFormatLayer);
                AaruConsole.WriteLine("Eject?: {0}", loadEject);
                AaruConsole.WriteLine("Start?: {0}", start);
                AaruConsole.WriteLine("Format layer: {0}", formatLayer);
                AaruConsole.WriteLine("Power conditions: {0}", powerConditions);
                AaruConsole.WriteLine();
                AaruConsole.WriteLine("Choose what to do:");
                AaruConsole.WriteLine("1.- Change parameters.");
                AaruConsole.WriteLine("2.- Send command with these parameters.");
                AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                        return;
                    case 1:
                        AaruConsole.Write("Immediate?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out immediate))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            immediate = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Change format layer?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out changeFormatLayer))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            changeFormatLayer = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Eject?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out loadEject))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            loadEject = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Start?: ");
                        strDev = System.Console.ReadLine();

                        if(!bool.TryParse(strDev, out start))
                        {
                            AaruConsole.WriteLine("Not a boolean. Press any key to continue...");
                            start = false;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Format layer?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out formatLayer))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            formatLayer = 0;
                            System.Console.ReadKey();

                            continue;
                        }

                        AaruConsole.Write("Power conditions?: ");
                        strDev = System.Console.ReadLine();

                        if(!byte.TryParse(strDev, out powerConditions))
                        {
                            AaruConsole.WriteLine("Not a number. Press any key to continue...");
                            powerConditions = 0;
                            System.Console.ReadKey();
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            System.Console.Clear();

            bool sense = dev.StartStopUnit(out byte[] senseBuffer, immediate, formatLayer, powerConditions,
                                           changeFormatLayer, loadEject, start, dev.Timeout, out double duration);

            menu:
            AaruConsole.WriteLine("Device: {0}", devPath);
            AaruConsole.WriteLine("Sending START STOP UNIT to the device:");
            AaruConsole.WriteLine("Command took {0} ms.", duration);
            AaruConsole.WriteLine("Sense is {0}.", sense);
            AaruConsole.WriteLine("Sense buffer is {0} bytes.", senseBuffer?.Length.ToString() ?? "null");
            AaruConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            AaruConsole.WriteLine("START STOP UNIT decoded sense:");
            AaruConsole.Write("{0}", Sense.PrettifySense(senseBuffer));
            AaruConsole.WriteLine();
            AaruConsole.WriteLine("Choose what to do:");
            AaruConsole.WriteLine("1.- Print sense buffer.");
            AaruConsole.WriteLine("2.- Send command again.");
            AaruConsole.WriteLine("3.- Change parameters.");
            AaruConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    AaruConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");

                    return;
                case 1:
                    System.Console.Clear();
                    AaruConsole.WriteLine("Device: {0}", devPath);
                    AaruConsole.WriteLine("START STOP UNIT sense:");

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
    }
}