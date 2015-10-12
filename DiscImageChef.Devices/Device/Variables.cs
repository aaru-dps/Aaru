// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Variables.cs
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
        Interop.PlatformID platformID;
        object fd;

        /// <summary>
        /// Gets the Platform ID for this device
        /// </summary>
        /// <value>The Platform ID</value>
        public Interop.PlatformID PlatformID
        {
            get
            {
                return platformID;
            }
        }

        /// <summary>
        /// Gets the file handle representing this device
        /// </summary>
        /// <value>The file handle</value>
        public object FileHandle
        {
            get
            {
                return fd;
            }
        }

        /// <summary>
        /// Gets or sets the standard timeout for commands sent to this device
        /// </summary>
        /// <value>The timeout in seconds</value>
        public uint Timeout
        {
            get;
            set;
        }
    }
}

