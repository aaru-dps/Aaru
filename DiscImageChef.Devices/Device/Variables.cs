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
        bool error;
        int lastError;
        readonly DeviceType type;
        readonly string manufacturer;
        readonly string model;
        readonly string revision;
        readonly string serial;
        readonly Decoders.SCSI.PeripheralDeviceTypes scsiType;
        readonly bool removable;

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

        /// <summary>
        /// Gets a value indicating whether this <see cref="DiscImageChef.Devices.Device"/> is in error.
        /// </summary>
        /// <value><c>true</c> if error; otherwise, <c>false</c>.</value>
        public bool Error
        {
            get
            {
                return error;
            }
        }

        /// <summary>
        /// Gets the last error number.
        /// </summary>
        /// <value>The last error.</value>
        public int LastError
        {
            get
            {
                return lastError;
            }
        }

        /// <summary>
        /// Gets the device type.
        /// </summary>
        /// <value>The device type.</value>
        public DeviceType Type
        {
            get
            {
                return type;
            }
        }

        /// <summary>
        /// Gets the device's manufacturer
        /// </summary>
        /// <value>The manufacturer.</value>
        public string Manufacturer
        {
            get
            {
                return manufacturer;
            }
        }

        /// <summary>
        /// Gets the device model
        /// </summary>
        /// <value>The model.</value>
        public string Model
        {
            get
            {
                return model;
            }
        }

        /// <summary>
        /// Gets the device's revision.
        /// </summary>
        /// <value>The revision.</value>
        public string Revision
        {
            get
            {
                return revision;
            }
        }

        /// <summary>
        /// Gets the device's serial number.
        /// </summary>
        /// <value>The serial number.</value>
        public string Serial
        {
            get
            {
                return serial;
            }
        }

        public Decoders.SCSI.PeripheralDeviceTypes SCSIType
        {
            get
            {
                return scsiType;
            }
        }

        public bool IsRemovable
        {
            get
            {
                return removable;
            }
        }
    }
}

