// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : List.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Gets a list of known physical devices.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Interop;
using PlatformID = DiscImageChef.Interop.PlatformID;

namespace DiscImageChef.Devices
{
    public struct DeviceInfo
    {
        public string Path;
        public string Vendor;
        public string Model;
        public string Serial;
        public string Bus;
        public bool   Supported;
    }

    public partial class Device
    {
        public static DeviceInfo[] ListDevices()
        {
            switch(DetectOS.GetRealPlatformID())
            {
                case PlatformID.Win32NT: return Windows.ListDevices.GetList();
                case PlatformID.Linux:   return Linux.ListDevices.GetList();
                case PlatformID.FreeBSD: return FreeBSD.ListDevices.GetList();
                default:
                    throw new InvalidOperationException($"Platform {DetectOS.GetRealPlatformID()} not yet supported.");
            }
        }
    }
}