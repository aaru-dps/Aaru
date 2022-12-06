// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DeviceException.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Devices.
//
// --[ Description ] ----------------------------------------------------------
//
//     Exception to be returned by the device constructor.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;

namespace Aaru.Devices
{
    /// <inheritdoc />
    /// <summary>Exception to be returned by the device constructor</summary>
    public sealed class DeviceException : Exception
    {
        internal DeviceException(string message) : base(message) {}

        internal DeviceException(int lastError) => LastError = lastError;

        /// <summary>Last error sent by the operating systen</summary>
        public int LastError { get; }
    }
}