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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Interop;
using Aaru.Console;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;

namespace Aaru.Devices
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DeviceInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string Path;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Vendor;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Model;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Serial;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Bus;

        [MarshalAs(UnmanagedType.U1)]
        public bool Supported;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] Padding;
    }

    public partial class Device
    {
        public static DeviceInfo[] ListDevices() => ListDevices(out _, out _, out _, out _, out _, out _);

        public static DeviceInfo[] ListDevices(out bool isRemote, out string serverApplication,
                                               out string serverVersion, out string serverOperatingSystem,
                                               out string serverOperatingSystemVersion, out string serverArchitecture,
                                               string dicRemote = null)
        {
            isRemote                     = false;
            serverApplication            = null;
            serverVersion                = null;
            serverOperatingSystem        = null;
            serverOperatingSystemVersion = null;
            serverArchitecture           = null;

            if(dicRemote is null)
                switch(DetectOS.GetRealPlatformID())
                {
                    case PlatformID.Win32NT: return Windows.ListDevices.GetList();
                    case PlatformID.Linux:   return Linux.ListDevices.GetList();
                    case PlatformID.FreeBSD: return FreeBSD.ListDevices.GetList();
                    default:
                        throw new
                            InvalidOperationException($"Platform {DetectOS.GetRealPlatformID()} not yet supported.");
                }

            try
            {
                using(var remote = new Remote.Remote(dicRemote))
                {
                    isRemote                     = true;
                    serverApplication            = remote.ServerApplication;
                    serverVersion                = remote.ServerVersion;
                    serverOperatingSystem        = remote.ServerOperatingSystem;
                    serverOperatingSystemVersion = remote.ServerOperatingSystemVersion;
                    serverArchitecture           = remote.ServerArchitecture;

                    return remote.ListDevices();
                }
            }
            catch(Exception)
            {
                DicConsole.ErrorWriteLine("Error connecting to host.");

                return new DeviceInfo[0];
            }
        }
    }
}