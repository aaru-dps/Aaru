// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MMC.cs
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
    static class MMC
    {
        internal static void Menu(string devPath, Device dev)
        {
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Send a MultiMedia Command to the device:");
                DicConsole.WriteLine("1.- Send GET CONFIGURATION command.");
                DicConsole.WriteLine("2.- Send PREVENT ALLOW MEDIUM REMOVAL command.");
                DicConsole.WriteLine("3.- Send READ CD command.");
                DicConsole.WriteLine("4.- Send READ CD MSF command.");
                DicConsole.WriteLine("5.- Send READ DISC INFORMATION command.");
                DicConsole.WriteLine("6.- Send READ DISC STRUCTURE command.");
                DicConsole.WriteLine("7.- Send READ TOC/PMA/ATIP command.");
                DicConsole.WriteLine("8.- Send START STOP UNIT command.");
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
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        continue;
                }
            }
        }

        static void GetConfiguration(string devPath, Device dev)
        {
            MmcGetConfigurationRt rt = MmcGetConfigurationRt.All;
            ushort startingFeatureNumber = 0;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for GET CONFIGURATION command:");
                DicConsole.WriteLine("RT: {0}", rt);
                DicConsole.WriteLine("Feature number: {0}", startingFeatureNumber);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                        return;
                    case 1:
                        DicConsole.WriteLine("RT");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", MmcGetConfigurationRt.All,
                                             MmcGetConfigurationRt.Current, MmcGetConfigurationRt.Reserved,
                                             MmcGetConfigurationRt.Single);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out rt))
                        {
                            DicConsole.WriteLine("Not a correct object type. Press any key to continue...");
                            rt = MmcGetConfigurationRt.All;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Feature number");
                        strDev = System.Console.ReadLine();
                        if(!ushort.TryParse(strDev, out startingFeatureNumber))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            startingFeatureNumber = 1;
                            System.Console.ReadKey();
                            continue;
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending GET CONFIGURATION to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("GET CONFIGURATION buffer:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("GET CONFIGURATION decoded buffer:");
                    if(buffer != null)
                    {
                        Decoders.SCSI.MMC.Features.SeparatedFeatures ftr = Decoders.SCSI.MMC.Features.Separate(buffer);
                        DicConsole.WriteLine("GET CONFIGURATION length is {0} bytes", ftr.DataLength);
                        DicConsole.WriteLine("GET CONFIGURATION current profile is {0:X4}h", ftr.CurrentProfile);
                        if(ftr.Descriptors != null)
                            foreach(Decoders.SCSI.MMC.Features.FeatureDescriptor desc in ftr.Descriptors)
                            {
                                DicConsole.WriteLine("Feature {0:X4}h", desc.Code);

                                switch(desc.Code)
                                {
                                    case 0x0000:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0000(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0001:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0001(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0002:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0002(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0003:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0003(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0004:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0004(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0010:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0010(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x001D:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_001D(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x001E:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_001E(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x001F:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_001F(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0020:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0020(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0021:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0021(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0022:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0022(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0023:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0023(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0024:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0024(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0025:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0025(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0026:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0026(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0027:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0027(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0028:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0028(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0029:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0029(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x002A:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_002A(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x002B:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_002B(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x002C:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_002C(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x002D:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_002D(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x002E:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_002E(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x002F:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_002F(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0030:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0030(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0031:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0031(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0032:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0032(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0033:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0033(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0035:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0035(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0037:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0037(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0038:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0038(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x003A:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_003A(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x003B:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_003B(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0040:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0040(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0041:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0041(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0042:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0042(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0050:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0050(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0051:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0051(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0080:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0080(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0100:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0100(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0101:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0101(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0102:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0102(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0103:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0103(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0104:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0104(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0105:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0105(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0106:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0106(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0107:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0107(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0108:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0108(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0109:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0109(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x010A:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_010A(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x010B:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_010B(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x010C:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_010C(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x010D:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_010D(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x010E:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_010E(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0110:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0110(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0113:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0113(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    case 0x0142:
                                        DicConsole.Write("{0}", Decoders.SCSI.MMC.Features.Prettify_0142(desc.Data));
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                    default:
                                        DicConsole.WriteLine("Don't know how to decode feature 0x{0:X4}", desc.Code);
                                        PrintHex.PrintHexArray(desc.Data, 64);
                                        break;
                                }
                            }
                    }

                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("GET CONFIGURATION sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("GET CONFIGURATION decoded sense:");
                    if(senseBuffer != null) DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
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
            bool prevent = false;
            bool persistent = false;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for PREVENT ALLOW MEDIUM REMOVAL command:");
                DicConsole.WriteLine("Prevent removal?: {0}", prevent);
                DicConsole.WriteLine("Persistent value?: {0}", persistent);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Prevent removal?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out prevent))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            prevent = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Persistent value?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out persistent))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            persistent = false;
                            System.Console.ReadKey();
                            continue;
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending PREVENT ALLOW MEDIUM REMOVAL to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("PREVENT ALLOW MEDIUM REMOVAL decoded sense:");
            DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("PREVENT ALLOW MEDIUM REMOVAL sense:");
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

        static void ReadCd(string devPath, Device dev)
        {
            uint address = 0;
            uint length = 1;
            MmcSectorTypes sectorType = MmcSectorTypes.AllTypes;
            bool dap = false;
            bool relative = false;
            bool sync = false;
            MmcHeaderCodes header = MmcHeaderCodes.None;
            bool user = true;
            bool edc = false;
            MmcErrorField c2 = MmcErrorField.None;
            MmcSubchannel subchan = MmcSubchannel.None;
            uint blockSize = 2352;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ CD command:");
                DicConsole.WriteLine("Address relative to current position?: {0}", relative);
                DicConsole.WriteLine("{1}: {0}", address, relative ? "Address" : "LBA");
                DicConsole.WriteLine("Will transfer {0} sectors", length);
                DicConsole.WriteLine("Sector type: {0}", sectorType);
                DicConsole.WriteLine("Process audio?: {0}", dap);
                DicConsole.WriteLine("Retrieve sync bytes?: {0}", sync);
                DicConsole.WriteLine("Header mode: {0}", header);
                DicConsole.WriteLine("Retrieve user data?: {0}", user);
                DicConsole.WriteLine("Retrieve EDC/ECC data?: {0}", edc);
                DicConsole.WriteLine("C2 mode: {0}", c2);
                DicConsole.WriteLine("Subchannel mode: {0}", subchan);
                DicConsole.WriteLine("{0} bytes per sector", blockSize);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Address is relative to current position?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out relative))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            relative = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("{0}?: ", relative ? "Address" : "LBA");
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

                        DicConsole.WriteLine("Sector type");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3} {4} {5}", MmcSectorTypes.AllTypes,
                                             MmcSectorTypes.Cdda, MmcSectorTypes.Mode1, MmcSectorTypes.Mode2,
                                             MmcSectorTypes.Mode2Form1, MmcSectorTypes.Mode2Form2);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out sectorType))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            sectorType = MmcSectorTypes.AllTypes;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Process audio?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out dap))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            dap = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Retrieve sync bytes?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out sync))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sync = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("Header mode");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", MmcHeaderCodes.None,
                                             MmcHeaderCodes.HeaderOnly, MmcHeaderCodes.SubHeaderOnly,
                                             MmcHeaderCodes.AllHeaders);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out header))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            header = MmcHeaderCodes.None;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Retrieve user data?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out user))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            user = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Retrieve EDC/ECC?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out edc))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            edc = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("C2 mode");
                        DicConsole.WriteLine("Available values: {0} {1} {2}", MmcErrorField.None,
                                             MmcErrorField.C2Pointers, MmcErrorField.C2PointersAndBlock);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out c2))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            c2 = MmcErrorField.None;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("Subchannel mode");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", MmcSubchannel.None, MmcSubchannel.Raw,
                                             MmcSubchannel.Q16, MmcSubchannel.Rw);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out subchan))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            subchan = MmcSubchannel.None;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Expected block size?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 2352;
                            System.Console.ReadKey();
                            continue;
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ CD to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD decoded sense:");
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

        static void ReadCdMsf(string devPath, Device dev)
        {
            byte startFrame = 0;
            byte startSecond = 2;
            byte startMinute = 0;
            byte endFrame = 0;
            byte endSecond = 0;
            byte endMinute = 0;
            MmcSectorTypes sectorType = MmcSectorTypes.AllTypes;
            bool dap = false;
            bool sync = false;
            MmcHeaderCodes header = MmcHeaderCodes.None;
            bool user = true;
            bool edc = false;
            MmcErrorField c2 = MmcErrorField.None;
            MmcSubchannel subchan = MmcSubchannel.None;
            uint blockSize = 2352;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ CD MSF command:");
                DicConsole.WriteLine("Start: {0:D2}:{1:D2}:{2:D2}", startMinute, startSecond, startFrame);
                DicConsole.WriteLine("End: {0:D2}:{1:D2}:{2:D2}", endMinute, endSecond, endFrame);
                DicConsole.WriteLine("Sector type: {0}", sectorType);
                DicConsole.WriteLine("Process audio?: {0}", dap);
                DicConsole.WriteLine("Retrieve sync bytes?: {0}", sync);
                DicConsole.WriteLine("Header mode: {0}", header);
                DicConsole.WriteLine("Retrieve user data?: {0}", user);
                DicConsole.WriteLine("Retrieve EDC/ECC data?: {0}", edc);
                DicConsole.WriteLine("C2 mode: {0}", c2);
                DicConsole.WriteLine("Subchannel mode: {0}", subchan);
                DicConsole.WriteLine("{0} bytes per sector", blockSize);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
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

                        DicConsole.WriteLine("Sector type");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3} {4} {5}", MmcSectorTypes.AllTypes,
                                             MmcSectorTypes.Cdda, MmcSectorTypes.Mode1, MmcSectorTypes.Mode2,
                                             MmcSectorTypes.Mode2Form1, MmcSectorTypes.Mode2Form2);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out sectorType))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            sectorType = MmcSectorTypes.AllTypes;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Process audio?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out dap))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            dap = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Retrieve sync bytes?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out sync))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            sync = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("Header mode");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", MmcHeaderCodes.None,
                                             MmcHeaderCodes.HeaderOnly, MmcHeaderCodes.SubHeaderOnly,
                                             MmcHeaderCodes.AllHeaders);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out header))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            header = MmcHeaderCodes.None;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Retrieve user data?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out user))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            user = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Retrieve EDC/ECC?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out edc))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            edc = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("C2 mode");
                        DicConsole.WriteLine("Available values: {0} {1} {2}", MmcErrorField.None,
                                             MmcErrorField.C2Pointers, MmcErrorField.C2PointersAndBlock);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out c2))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            c2 = MmcErrorField.None;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("Subchannel mode");
                        DicConsole.WriteLine("Available values: {0} {1} {2} {3}", MmcSubchannel.None, MmcSubchannel.Raw,
                                             MmcSubchannel.Q16, MmcSubchannel.Rw);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out subchan))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            subchan = MmcSubchannel.None;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Expected block size?: ");
                        strDev = System.Console.ReadLine();
                        if(!uint.TryParse(strDev, out blockSize))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            blockSize = 2352;
                            System.Console.ReadKey();
                            continue;
                        }

                        break;
                    case 2: goto start;
                }
            }

            start:
            uint startMsf = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
            uint endMsf = (uint)((startMinute << 16) + (startSecond << 8) + startFrame);
            System.Console.Clear();
            bool sense = dev.ReadCdMsf(out byte[] buffer, out byte[] senseBuffer, startMsf, endMsf, blockSize,
                                       sectorType, dap, sync, header, user, edc, c2, subchan, dev.Timeout,
                                       out double duration);

            menu:
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ CD MSF to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD MSF response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD MSF sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ CD MSF decoded sense:");
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

        static void ReadDiscInformation(string devPath, Device dev)
        {
            MmcDiscInformationDataTypes info = MmcDiscInformationDataTypes.DiscInformation;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ DISC INFORMATION command:");
                DicConsole.WriteLine("Information type: {0}", info);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                        return;
                    case 1:
                        DicConsole.WriteLine("Information type");
                        DicConsole.WriteLine("Available values: {0} {1} {2}",
                                             MmcDiscInformationDataTypes.DiscInformation,
                                             MmcDiscInformationDataTypes.TrackResources,
                                             MmcDiscInformationDataTypes.PowResources);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out info))
                        {
                            DicConsole.WriteLine("Not a correct page control. Press any key to continue...");
                            info = MmcDiscInformationDataTypes.DiscInformation;
                            System.Console.ReadKey();
                            continue;
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ DISC INFORMATION to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DISC INFORMATION response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DISC INFORMATION decoded response:");
                    DicConsole.Write("{0}", Decoders.SCSI.MMC.DiscInformation.Prettify(buffer));
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DISC INFORMATION sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DISC INFORMATION decoded sense:");
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

        static void ReadDiscStructure(string devPath, Device dev)
        {
            MmcDiscStructureMediaType mediaType = MmcDiscStructureMediaType.Dvd;
            MmcDiscStructureFormat format = MmcDiscStructureFormat.CapabilityList;
            uint address = 0;
            byte layer = 0;
            byte agid = 0;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ DISC STRUCTURE command:");
                DicConsole.WriteLine("Media type: {0}", mediaType);
                DicConsole.WriteLine("Format: {0}", format);
                DicConsole.WriteLine("Address: {0}", address);
                DicConsole.WriteLine("Layer: {0}", layer);
                DicConsole.WriteLine("AGID: {0}", agid);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                        return;
                    case 1:
                        DicConsole.WriteLine("Media type");
                        DicConsole.WriteLine("Available values: {0} {1}", MmcDiscStructureMediaType.Dvd,
                                             MmcDiscStructureMediaType.Bd);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out mediaType))
                        {
                            DicConsole.WriteLine("Not a correct media type. Press any key to continue...");
                            mediaType = MmcDiscStructureMediaType.Dvd;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.WriteLine("Format");
                        DicConsole.WriteLine("Available values:");
                        if(mediaType == MmcDiscStructureMediaType.Dvd)
                        {
                            DicConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.PhysicalInformation,
                                                 MmcDiscStructureFormat.CopyrightInformation,
                                                 MmcDiscStructureFormat.DiscKey,
                                                 MmcDiscStructureFormat.BurstCuttingArea);
                            DicConsole.WriteLine("\t{0} {1} {2} {3}",
                                                 MmcDiscStructureFormat.DiscManufacturingInformation,
                                                 MmcDiscStructureFormat.SectorCopyrightInformation,
                                                 MmcDiscStructureFormat.MediaIdentifier,
                                                 MmcDiscStructureFormat.MediaKeyBlock);
                            DicConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.DvdramDds,
                                                 MmcDiscStructureFormat.DvdramMediumStatus,
                                                 MmcDiscStructureFormat.DvdramSpareAreaInformation,
                                                 MmcDiscStructureFormat.DvdramRecordingType);
                            DicConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.LastBorderOutRmd,
                                                 MmcDiscStructureFormat.SpecifiedRmd,
                                                 MmcDiscStructureFormat.PreRecordedInfo,
                                                 MmcDiscStructureFormat.DvdrMediaIdentifier);
                            DicConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.DvdrPhysicalInformation,
                                                 MmcDiscStructureFormat.Adip,
                                                 MmcDiscStructureFormat.HddvdCopyrightInformation,
                                                 MmcDiscStructureFormat.DvdAacs);
                            DicConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.HddvdrMediumStatus,
                                                 MmcDiscStructureFormat.HddvdrLastRmd,
                                                 MmcDiscStructureFormat.DvdrLayerCapacity,
                                                 MmcDiscStructureFormat.MiddleZoneStart);
                            DicConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.JumpIntervalSize,
                                                 MmcDiscStructureFormat.ManualLayerJumpStartLba,
                                                 MmcDiscStructureFormat.RemapAnchorPoint, MmcDiscStructureFormat.Dcb);
                        }
                        if(mediaType == MmcDiscStructureMediaType.Bd)
                        {
                            DicConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.DiscInformation,
                                                 MmcDiscStructureFormat.BdBurstCuttingArea,
                                                 MmcDiscStructureFormat.BdDds, MmcDiscStructureFormat.CartridgeStatus);
                            DicConsole.WriteLine("\t{0} {1} {2}", MmcDiscStructureFormat.BdSpareAreaInformation,
                                                 MmcDiscStructureFormat.RawDfl, MmcDiscStructureFormat.Pac);
                        }
                        DicConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.AacsVolId,
                                             MmcDiscStructureFormat.AacsMediaSerial, MmcDiscStructureFormat.AacsMediaId,
                                             MmcDiscStructureFormat.Aacsmkb);
                        DicConsole.WriteLine("\t{0} {1} {2} {3}", MmcDiscStructureFormat.AacsDataKeys,
                                             MmcDiscStructureFormat.AacslbaExtents, MmcDiscStructureFormat.Aacsmkbcprm,
                                             MmcDiscStructureFormat.RecognizedFormatLayers);
                        DicConsole.WriteLine("\t{0} {1}", MmcDiscStructureFormat.WriteProtectionStatus,
                                             MmcDiscStructureFormat.CapabilityList);
                        DicConsole.Write("Choose?: ");
                        strDev = System.Console.ReadLine();
                        if(!System.Enum.TryParse(strDev, true, out format))
                        {
                            DicConsole.WriteLine("Not a correct media type. Press any key to continue...");
                            format = MmcDiscStructureFormat.CapabilityList;
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

                        DicConsole.Write("Layer?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out layer))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            layer = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("AGID?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out agid))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            agid = 0;
                            System.Console.ReadKey();
                            continue;
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ DISC STRUCTURE to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DISC STRUCTURE response:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    // TODO: Implement
                    DicConsole.WriteLine("READ DISC STRUCTURE decoding not yet implemented:");
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DISC STRUCTURE sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ DISC STRUCTURE decoded sense:");
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

        static void ReadTocPmaAtip(string devPath, Device dev)
        {
            bool msf = false;
            byte format = 0;
            byte session = 0;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for READ TOC/PMA/ATIP command:");
                DicConsole.WriteLine("Return MSF values?: {0}", msf);
                DicConsole.WriteLine("Format byte: {0}", format);
                DicConsole.WriteLine("Session: {0}", session);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Return MSF values?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out msf))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            msf = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Format?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out format))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            format = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Session?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out session))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            session = 0;
                            System.Console.ReadKey();
                            continue;
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending READ TOC/PMA/ATIP to the device:");
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
            DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ TOC/PMA/ATIP buffer:");
                    if(buffer != null) PrintHex.PrintHexArray(buffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 2:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ TOC/PMA/ATIP decoded buffer:");
                    if(buffer != null)
                        switch(format)
                        {
                            case 0:
                                DicConsole.Write("{0}", Decoders.CD.TOC.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);
                                break;
                            case 1:
                                DicConsole.Write("{0}", Decoders.CD.Session.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);
                                break;
                            case 2:
                                DicConsole.Write("{0}", Decoders.CD.FullTOC.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);
                                break;
                            case 3:
                                DicConsole.Write("{0}", Decoders.CD.PMA.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);
                                break;
                            case 4:
                                DicConsole.Write("{0}", Decoders.CD.ATIP.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);
                                break;
                            case 5:
                                DicConsole.Write("{0}", Decoders.CD.CDTextOnLeadIn.Prettify(buffer));
                                PrintHex.PrintHexArray(buffer, 64);
                                break;
                        }

                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 3:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ TOC/PMA/ATIP sense:");
                    if(senseBuffer != null) PrintHex.PrintHexArray(senseBuffer, 64);
                    DicConsole.WriteLine("Press any key to continue...");
                    System.Console.ReadKey();
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    goto menu;
                case 4:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("READ TOC/PMA/ATIP decoded sense:");
                    if(senseBuffer != null) DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
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

        static void StartStopUnit(string devPath, Device dev)
        {
            bool immediate = false;
            bool changeFormatLayer = false;
            bool loadEject = false;
            bool start = false;
            byte formatLayer = 0;
            byte powerConditions = 0;
            string strDev;
            int item;

            parameters:
            while(true)
            {
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Parameters for START STOP UNIT command:");
                DicConsole.WriteLine("Immediate?: {0}", immediate);
                DicConsole.WriteLine("Change format layer?: {0}", changeFormatLayer);
                DicConsole.WriteLine("Eject?: {0}", loadEject);
                DicConsole.WriteLine("Start?: {0}", start);
                DicConsole.WriteLine("Format layer: {0}", formatLayer);
                DicConsole.WriteLine("Power conditions: {0}", powerConditions);
                DicConsole.WriteLine();
                DicConsole.WriteLine("Choose what to do:");
                DicConsole.WriteLine("1.- Change parameters.");
                DicConsole.WriteLine("2.- Send command with these parameters.");
                DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");

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
                        DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                        return;
                    case 1:
                        DicConsole.Write("Immediate?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out immediate))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            immediate = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Change format layer?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out changeFormatLayer))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            changeFormatLayer = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Eject?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out loadEject))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            loadEject = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Start?: ");
                        strDev = System.Console.ReadLine();
                        if(!bool.TryParse(strDev, out start))
                        {
                            DicConsole.WriteLine("Not a boolean. Press any key to continue...");
                            start = false;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Format layer?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out formatLayer))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            formatLayer = 0;
                            System.Console.ReadKey();
                            continue;
                        }

                        DicConsole.Write("Power conditions?: ");
                        strDev = System.Console.ReadLine();
                        if(!byte.TryParse(strDev, out powerConditions))
                        {
                            DicConsole.WriteLine("Not a number. Press any key to continue...");
                            powerConditions = 0;
                            System.Console.ReadKey();
                            continue;
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
            DicConsole.WriteLine("Device: {0}", devPath);
            DicConsole.WriteLine("Sending START STOP UNIT to the device:");
            DicConsole.WriteLine("Command took {0} ms.", duration);
            DicConsole.WriteLine("Sense is {0}.", sense);
            DicConsole.WriteLine("Sense buffer is {0} bytes.",
                                 senseBuffer == null ? "null" : senseBuffer.Length.ToString());
            DicConsole.WriteLine("Sense buffer is null or empty? {0}", ArrayHelpers.ArrayIsNullOrEmpty(senseBuffer));
            DicConsole.WriteLine("START STOP UNIT decoded sense:");
            DicConsole.Write("{0}", Decoders.SCSI.Sense.PrettifySense(senseBuffer));
            DicConsole.WriteLine();
            DicConsole.WriteLine("Choose what to do:");
            DicConsole.WriteLine("1.- Print sense buffer.");
            DicConsole.WriteLine("2.- Send command again.");
            DicConsole.WriteLine("3.- Change parameters.");
            DicConsole.WriteLine("0.- Return to SCSI MultiMedia Commands menu.");
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
                    DicConsole.WriteLine("Returning to SCSI MultiMedia Commands menu...");
                    return;
                case 1:
                    System.Console.Clear();
                    DicConsole.WriteLine("Device: {0}", devPath);
                    DicConsole.WriteLine("START STOP UNIT sense:");
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