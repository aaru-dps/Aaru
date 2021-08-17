// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Destructor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Returns the device to the operating system.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.Devices.Linux;
using Microsoft.Win32.SafeHandles;
using PlatformID = Aaru.CommonTypes.Interop.PlatformID;

namespace Aaru.Devices
{
    public sealed partial class Device
    {
        /// <summary>
        ///     Releases unmanaged resources and performs other cleanup operations before the <see cref="Device" /> is
        ///     reclaimed by garbage collection.
        /// </summary>
        ~Device() => Close();

        /// <summary>Closes a device</summary>
        public void Close()
        {
            if(_remote != null)
            {
                _remote.Close();
                _remote.Disconnect();

                return;
            }

            if(FileHandle == null)
                return;

            switch(PlatformId)
            {
                case PlatformID.Win32NT:
                    (FileHandle as SafeFileHandle)?.Close();

                    break;
                case PlatformID.Linux:
                    Extern.close((int)FileHandle);

                    break;
                case PlatformID.FreeBSD:
                    FreeBSD.Extern.cam_close_device((IntPtr)FileHandle);

                    break;
            }

            FileHandle = null;
        }
    }
}