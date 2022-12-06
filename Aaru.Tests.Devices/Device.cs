// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Device.cs
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
using Aaru.Devices;
using Aaru.Helpers;

namespace Aaru.Tests.Devices
{
    internal static partial class MainClass
    {
        public static void Device(string devPath)
        {
            AaruConsole.WriteLine("Going to open {0}. Press any key to continue...", devPath);
            System.Console.ReadKey();

            var dev = new Device(devPath);

            while(true)
            {
                AaruConsole.WriteLine("dev.PlatformID = {0}", dev.PlatformId);
                AaruConsole.WriteLine("dev.FileHandle = {0}", dev.FileHandle);
                AaruConsole.WriteLine("dev.Timeout = {0}", dev.Timeout);
                AaruConsole.WriteLine("dev.Error = {0}", dev.Error);
                AaruConsole.WriteLine("dev.LastError = {0}", dev.LastError);
                AaruConsole.WriteLine("dev.Type = {0}", dev.Type);
                AaruConsole.WriteLine("dev.Manufacturer = \"{0}\"", dev.Manufacturer);
                AaruConsole.WriteLine("dev.Model = \"{0}\"", dev.Model);
                AaruConsole.WriteLine("dev.Revision = \"{0}\"", dev.FirmwareRevision);
                AaruConsole.WriteLine("dev.Serial = \"{0}\"", dev.Serial);
                AaruConsole.WriteLine("dev.SCSIType = {0}", dev.ScsiType);
                AaruConsole.WriteLine("dev.IsRemovable = {0}", dev.IsRemovable);
                AaruConsole.WriteLine("dev.IsUSB = {0}", dev.IsUsb);
                AaruConsole.WriteLine("dev.USBVendorID = 0x{0:X4}", dev.UsbVendorId);
                AaruConsole.WriteLine("dev.USBProductID = 0x{0:X4}", dev.UsbProductId);

                AaruConsole.WriteLine("dev.USBDescriptors.Length = {0}",
                                      dev.UsbDescriptors?.Length.ToString() ?? "null");

                AaruConsole.WriteLine("dev.USBManufacturerString = \"{0}\"", dev.UsbManufacturerString);
                AaruConsole.WriteLine("dev.USBProductString = \"{0}\"", dev.UsbProductString);
                AaruConsole.WriteLine("dev.USBSerialString = \"{0}\"", dev.UsbSerialString);
                AaruConsole.WriteLine("dev.IsFireWire = {0}", dev.IsFireWire);
                AaruConsole.WriteLine("dev.FireWireGUID = {0:X16}", dev.FireWireGuid);
                AaruConsole.WriteLine("dev.FireWireModel = 0x{0:X8}", dev.FireWireModel);
                AaruConsole.WriteLine("dev.FireWireModelName = \"{0}\"", dev.FireWireModelName);
                AaruConsole.WriteLine("dev.FireWireVendor = 0x{0:X8}", dev.FireWireVendor);
                AaruConsole.WriteLine("dev.FireWireVendorName = \"{0}\"", dev.FireWireVendorName);
                AaruConsole.WriteLine("dev.IsCompactFlash = {0}", dev.IsCompactFlash);
                AaruConsole.WriteLine("dev.IsPCMCIA = {0}", dev.IsPcmcia);
                AaruConsole.WriteLine("dev.CIS.Length = {0}", dev.Cis?.Length.ToString() ?? "null");

                AaruConsole.WriteLine("Press any key to continue...", devPath);
                System.Console.ReadKey();

                menu:
                System.Console.Clear();
                AaruConsole.WriteLine("Device: {0}", devPath);
                AaruConsole.WriteLine("Options:");
                AaruConsole.WriteLine("1.- Print USB descriptors.");
                AaruConsole.WriteLine("2.- Print PCMCIA CIS.");
                AaruConsole.WriteLine("3.- Send a command to the device.");
                AaruConsole.WriteLine("0.- Return to device selection.");
                AaruConsole.Write("Choose: ");

                string strDev = System.Console.ReadLine();

                if(!int.TryParse(strDev, out int item))
                {
                    AaruConsole.WriteLine("Not a number. Press any key to continue...");
                    System.Console.ReadKey();

                    goto menu;
                }

                switch(item)
                {
                    case 0:
                        AaruConsole.WriteLine("Returning to device selection...");

                        return;
                    case 1:
                        System.Console.Clear();
                        AaruConsole.WriteLine("Device: {0}", devPath);
                        AaruConsole.WriteLine("USB descriptors:");

                        if(dev.UsbDescriptors != null)
                            PrintHex.PrintHexArray(dev.UsbDescriptors, 64);

                        AaruConsole.WriteLine("Press any key to continue...");
                        System.Console.ReadKey();

                        goto menu;
                    case 2:
                        System.Console.Clear();
                        AaruConsole.WriteLine("Device: {0}", devPath);
                        AaruConsole.WriteLine("PCMCIA CIS:");

                        if(dev.Cis != null)
                            PrintHex.PrintHexArray(dev.Cis, 64);

                        AaruConsole.WriteLine("Press any key to continue...");
                        System.Console.ReadKey();

                        goto menu;
                    case 3:
                        Command(devPath, dev);

                        goto menu;
                    default:
                        AaruConsole.WriteLine("Incorrect option. Press any key to continue...");
                        System.Console.ReadKey();

                        goto menu;
                }
            }
        }
    }
}