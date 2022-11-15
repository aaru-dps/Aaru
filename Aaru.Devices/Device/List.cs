// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Interop;
using Aaru.Console;

namespace Aaru.Devices;

/// <summary>Contains device information</summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct DeviceInfo
{
    /// <summary>Device path</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
    public string Path;

    /// <summary>Device vendor or manufacturer</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Vendor;

    /// <summary>Device model or product name</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Model;

    /// <summary>Device serial number</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Serial;

    /// <summary>Bus the device is attached to</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Bus;

    /// <summary>
    ///     Set to <c>true</c> if Aaru can send commands to the device in the current machine or remote, <c>false</c>
    ///     otherwise
    /// </summary>
    [MarshalAs(UnmanagedType.U1)]
    public bool Supported;

    /// <summary>Padding</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public readonly byte[] Padding;
}

public partial class Device
{
    /// <summary>Lists devices attached to current machine</summary>
    /// <returns>List of devices</returns>
    public static DeviceInfo[] ListDevices() => ListDevices(out _, out _, out _, out _, out _, out _);

    /// <summary>Lists devices attached to current machine or specified remote</summary>
    /// <param name="isRemote">Is remote</param>
    /// <param name="serverApplication">Remote application</param>
    /// <param name="serverVersion">Remote application version</param>
    /// <param name="serverOperatingSystem">Remote operating system name</param>
    /// <param name="serverOperatingSystemVersion">Remote operating system version</param>
    /// <param name="serverArchitecture">Remote architecture</param>
    /// <param name="aaruRemote">Remote URI</param>
    /// <returns>List of devices</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DeviceInfo[] ListDevices(out bool isRemote, out string serverApplication, out string serverVersion,
                                           out string serverOperatingSystem, out string serverOperatingSystemVersion,
                                           out string serverArchitecture, string aaruRemote = null)
    {
        isRemote                     = false;
        serverApplication            = null;
        serverVersion                = null;
        serverOperatingSystem        = null;
        serverOperatingSystemVersion = null;
        serverArchitecture           = null;

        if(aaruRemote is null)
        {
            if(OperatingSystem.IsWindows())
                return Windows.ListDevices.GetList();

            if(OperatingSystem.IsLinux())
                return Linux.ListDevices.GetList();

            throw new InvalidOperationException($"Platform {DetectOS.GetRealPlatformID()} not yet supported.");
        }

        try
        {
            var aaruUri = new Uri(aaruRemote);

            if(aaruUri.Scheme != "aaru" &&
               aaruUri.Scheme != "dic")
            {
                AaruConsole.ErrorWriteLine("Invalid remote URI.");

                return Array.Empty<DeviceInfo>();
            }

            using var remote = new Remote.Remote(aaruUri);

            isRemote                     = true;
            serverApplication            = remote.ServerApplication;
            serverVersion                = remote.ServerVersion;
            serverOperatingSystem        = remote.ServerOperatingSystem;
            serverOperatingSystemVersion = remote.ServerOperatingSystemVersion;
            serverArchitecture           = remote.ServerArchitecture;

            return remote.ListDevices();
        }
        catch(Exception)
        {
            AaruConsole.ErrorWriteLine("Error connecting to host.");

            return Array.Empty<DeviceInfo>();
        }
    }
}