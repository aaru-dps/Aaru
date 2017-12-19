// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Device.cs
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

namespace DiscImageChef.Tests.Devices
{
    partial class MainClass
    {
        public static void Device(string devPath)
        {
            DicConsole.WriteLine("Going to open {0}. Press any key to continue...", devPath);
            System.Console.ReadKey();

            DiscImageChef.Devices.Device dev = new DiscImageChef.Devices.Device(devPath);

            while(true)
            {
                DicConsole.WriteLine("dev.PlatformID = {0}", dev.PlatformID);
                DicConsole.WriteLine("dev.FileHandle = {0}", dev.FileHandle);
                DicConsole.WriteLine("dev.Timeout = {0}", dev.Timeout);
                DicConsole.WriteLine("dev.Error = {0}", dev.Error);
                DicConsole.WriteLine("dev.LastError = {0}", dev.LastError);
                DicConsole.WriteLine("dev.Type = {0}", dev.Type);
                DicConsole.WriteLine("dev.Manufacturer = \"{0}\"", dev.Manufacturer);
                DicConsole.WriteLine("dev.Model = \"{0}\"", dev.Model);
                DicConsole.WriteLine("dev.Revision = \"{0}\"", dev.Revision);
                DicConsole.WriteLine("dev.Serial = \"{0}\"", dev.Serial);
                DicConsole.WriteLine("dev.SCSIType = {0}", dev.SCSIType);
                DicConsole.WriteLine("dev.IsRemovable = {0}", dev.IsRemovable);
                DicConsole.WriteLine("dev.IsUSB = {0}", dev.IsUSB);
                DicConsole.WriteLine("dev.USBVendorID = 0x{0:X4}", dev.USBVendorID);
                DicConsole.WriteLine("dev.USBProductID = 0x{0:X4}", dev.USBProductID);
                DicConsole.WriteLine("dev.USBDescriptors.Length = {0}",
                                     dev.USBDescriptors == null ? "null" : dev.USBDescriptors.Length.ToString());
                DicConsole.WriteLine("dev.USBManufacturerString = \"{0}\"", dev.USBManufacturerString);
                DicConsole.WriteLine("dev.USBProductString = \"{0}\"", dev.USBProductString);
                DicConsole.WriteLine("dev.USBSerialString = \"{0}\"", dev.USBSerialString);
                DicConsole.WriteLine("dev.IsFireWire = {0}", dev.IsFireWire);
                DicConsole.WriteLine("dev.FireWireGUID = {0:X16}", dev.FireWireGUID);
                DicConsole.WriteLine("dev.FireWireModel = 0x{0:X8}", dev.FireWireModel);
                DicConsole.WriteLine("dev.FireWireModelName = \"{0}\"", dev.FireWireModelName);
                DicConsole.WriteLine("dev.FireWireVendor = 0x{0:X8}", dev.FireWireVendor);
                DicConsole.WriteLine("dev.FireWireVendorName = \"{0}\"", dev.FireWireVendorName);
                DicConsole.WriteLine("dev.IsCompactFlash = {0}", dev.IsCompactFlash);
                DicConsole.WriteLine("dev.IsPCMCIA = {0}", dev.IsPCMCIA);
                DicConsole.WriteLine("dev.CIS.Length = {0}", dev.CIS == null ? "null" : dev.CIS.Length.ToString());

                DicConsole.WriteLine("Press any key to continue...", devPath);
                System.Console.ReadKey();

                menu:
                System.Console.Clear();
                DicConsole.WriteLine("Device: {0}", devPath);
                DicConsole.WriteLine("Options:");
                DicConsole.WriteLine("1.- Print USB descriptors.");
                DicConsole.WriteLine("2.- Print PCMCIA CIS.");
                DicConsole.WriteLine("3.- Send a command to the device.");
                DicConsole.WriteLine("0.- Return to device selection.");
                DicConsole.Write("Choose: ");

                string strDev = System.Console.ReadLine();
                if(!int.TryParse(strDev, out int item))
                {
                    DicConsole.WriteLine("Not a number. Press any key to continue...");
                    System.Console.ReadKey();
                    goto menu;
                }

                switch(item)
                {
                    case 0:
                        DicConsole.WriteLine("Returning to device selection...");
                        return;
                    case 1:
                        System.Console.Clear();
                        DicConsole.WriteLine("Device: {0}", devPath);
                        DicConsole.WriteLine("USB descriptors:");
                        if(dev.USBDescriptors != null) PrintHex.PrintHexArray(dev.USBDescriptors, 64);
                        DicConsole.WriteLine("Press any key to continue...");
                        System.Console.ReadKey();
                        goto menu;
                    case 2:
                        System.Console.Clear();
                        DicConsole.WriteLine("Device: {0}", devPath);
                        DicConsole.WriteLine("PCMCIA CIS:");
                        if(dev.CIS != null) PrintHex.PrintHexArray(dev.CIS, 64);
                        DicConsole.WriteLine("Press any key to continue...");
                        System.Console.ReadKey();
                        goto menu;
                    case 3:
                        Command(devPath, dev);
                        goto menu;
                    default:
                        DicConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();
                        goto menu;
                }
            }
        }
    }
}