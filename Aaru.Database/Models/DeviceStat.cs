// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DeviceStat.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Model for storing device statistics in database.
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

namespace Aaru.Database.Models
{
    /// <inheritdoc />
    /// <summary>Device found in usage</summary>
    public class DeviceStat : BaseModel
    {
        /// <summary>Manufacturer</summary>
        public string Manufacturer { get; set; }
        /// <summary>Model</summary>
        public string Model { get; set; }
        /// <summary>Revision or firmware version</summary>
        public string Revision { get; set; }
        /// <summary>Bus</summary>
        public string Bus { get; set; }
        /// <summary>Has already been synchronized with Aaru's server</summary>
        public bool Synchronized { get; set; }
    }
}