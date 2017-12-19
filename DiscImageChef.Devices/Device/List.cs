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

namespace DiscImageChef.Devices
{
    public struct DeviceInfo
    {
        public string path;
        public string vendor;
        public string model;
        public string serial;
        public string bus;
        public bool supported;
    }

    public partial class Device
    {
        public static DeviceInfo[] ListDevices()
        {
            switch(Interop.DetectOS.GetRealPlatformID())
            {
                case Interop.PlatformID.Win32NT:
                    return Windows.ListDevices.GetList();
                case Interop.PlatformID.Linux:
                    return Linux.ListDevices.GetList();
                case Interop.PlatformID.FreeBSD:
                    return FreeBSD.ListDevices.GetList();
                default:
                    throw new InvalidOperationException(string.Format("Platform {0} not yet supported.", Interop.DetectOS.GetRealPlatformID()));
            }

        }
    }
}
