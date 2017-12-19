// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : USB.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from USB devices.
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
using DiscImageChef.Metadata;

namespace DiscImageChef.Core.Devices.Report
{
    public static class USB
    {
        public static void Report(Device dev, ref DeviceReport report, bool debug, ref bool removable)
        {
            if(report == null)
                return;

            ConsoleKeyInfo pressedKey = new ConsoleKeyInfo();
            while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
            {
                DicConsole.Write("Is the device natively USB (in case of doubt, press Y)? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                DicConsole.WriteLine();
            }

            if(pressedKey.Key == ConsoleKey.Y)
            {
                report.USB = new usbType();
                report.USB.Manufacturer = dev.USBManufacturerString;
                report.USB.Product = dev.USBProductString;
                report.USB.ProductID = dev.USBProductID;
                report.USB.VendorID = dev.USBVendorID;

                pressedKey = new ConsoleKeyInfo();
                while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
                {
                    DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                    pressedKey = System.Console.ReadKey();
                    DicConsole.WriteLine();
                }

                report.USB.RemovableMedia = pressedKey.Key == ConsoleKey.Y;
                removable = report.USB.RemovableMedia;
                if(debug)
                    report.USB.Descriptors = dev.USBDescriptors;
            }
        }
    }
}
