// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Destructor.cs
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

namespace DiscImageChef.Devices
{
    public partial class Device
    {
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="DiscImageChef.Devices.Device"/> is reclaimed by garbage collection.
        /// </summary>
        ~Device()
        {
            if(fd != null)
            {
                switch(platformID)
                {
                    case Interop.PlatformID.Win32NT:
                        Windows.Extern.CloseHandle((SafeFileHandle)fd);
                        break;
                    case Interop.PlatformID.Linux:
                        Linux.Extern.close((int)fd);
                        break;
                }
            }
        }
    }
}

