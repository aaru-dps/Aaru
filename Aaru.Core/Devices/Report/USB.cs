// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using Aaru.CommonTypes.Metadata;

namespace Aaru.Core.Devices.Report;

/// <summary>Implements creating a report for a USB device</summary>
public sealed partial class DeviceReport
{
    /// <summary>Fills a device report with parameters specific to a USB device</summary>
    public Usb UsbReport()
    {
        var usbReport = new Usb
        {
            Manufacturer = _dev.UsbManufacturerString,
            Product      = _dev.UsbProductString,
            ProductID    = _dev.UsbProductId,
            VendorID     = _dev.UsbVendorId,
            Descriptors  = _dev.UsbDescriptors
        };

        return usbReport;
    }
}