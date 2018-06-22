// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FireWire.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Creates reports from FireWire devices.
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
    /// <summary>
    ///     Implements creating a report for a FireWire device
    /// </summary>
    static class FireWire
    {
        /// <summary>
        ///     Fills a device report with parameters specific to a FireWire device
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="report">Device report</param>
        /// <param name="removable">If device is removable</param>
        internal static void Report(Device dev, ref DeviceReport report, ref bool removable)
        {
            if(report == null) return;

            ConsoleKeyInfo pressedKey = new ConsoleKeyInfo();
            while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
            {
                DicConsole.Write("Is the device natively FireWire (in case of doubt, press Y)? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                DicConsole.WriteLine();
            }

            if(pressedKey.Key != ConsoleKey.Y) return;

            report.FireWire = new firewireType
            {
                Manufacturer = dev.FireWireVendorName,
                Product      = dev.FireWireModelName,
                ProductID    = dev.FireWireModel,
                VendorID     = dev.FireWireVendor
            };

            pressedKey = new ConsoleKeyInfo();
            while(pressedKey.Key != ConsoleKey.Y && pressedKey.Key != ConsoleKey.N)
            {
                DicConsole.Write("Is the media removable from the reading/writing elements? (Y/N): ");
                pressedKey = System.Console.ReadKey();
                DicConsole.WriteLine();
            }

            report.FireWire.RemovableMedia = pressedKey.Key == ConsoleKey.Y;
            removable                      = report.FireWire.RemovableMedia;
        }
    }
}