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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using DiscImageChef.CommonTypes.Metadata;

namespace DiscImageChef.Core.Devices.Report
{
    /// <summary>
    ///     Implements creating a report for a USB device
    /// </summary>
    public partial class DeviceReport
    {
        /// <summary>
        ///     Fills a device report with parameters specific to a USB device
        /// </summary>
        /// <param name="dev">Device</param>
        /// <param name="report">Device report</param>
        /// <param name="removable">If device is removable</param>
        /// <param name="debug">If debug is enabled</param>
        public Usb UsbReport()
        {
            Usb usbReport = new Usb
            {
                Manufacturer = dev.UsbManufacturerString,
                Product      = dev.UsbProductString,
                ProductID    = dev.UsbProductId,
                VendorID     = dev.UsbVendorId
            };

            if(debug) usbReport.Descriptors = dev.UsbDescriptors;

            return usbReport;
        }
    }
}