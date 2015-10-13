// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Device.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        /// <summary>
        /// Opens the device for sending direct commands
        /// </summary>
        /// <param name="devicePath">Device path</param>
        public Device(string devicePath)
        {
            platformID = Interop.DetectOS.GetRealPlatformID();
            Timeout = 15;
            error = false;

            switch (platformID)
            {
                case Interop.PlatformID.Win32NT:
                    {
                        fd = Windows.Extern.CreateFile(devicePath,
                            Windows.FileAccess.GenericRead | Windows.FileAccess.GenericWrite,
                            Windows.FileShare.Read | Windows.FileShare.Write,
                            IntPtr.Zero, Windows.FileMode.OpenExisting,
                            Windows.FileAttributes.Normal, IntPtr.Zero);

                        if (((SafeFileHandle)fd).IsInvalid)
                        {
                            error = true;
                            lastError = Marshal.GetLastWin32Error();
                        }

                        //throw new NotImplementedException();
                        break;
                    }
                case Interop.PlatformID.Linux:
                    {
                        fd = Linux.Extern.open(devicePath, Linux.FileFlags.Readonly | Linux.FileFlags.NonBlocking);

                        if ((int)fd < 0)
                        {
                            error = true;
                            lastError = Marshal.GetLastWin32Error();
                        }

                        //throw new NotImplementedException();
                        break;
                    }
                default:
                    throw new InvalidOperationException(String.Format("Platform {0} not yet supported.", platformID));
            }
        }
    }
}

