// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BaseModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Database.
//
// --[ Description ] ----------------------------------------------------------
//
//     Base model generic class.
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

using System.ComponentModel.DataAnnotations;

namespace Aaru.Database.Models
{
    /// <summary>
    /// Base database model
    /// </summary>
    public abstract class BaseModel
    {
        /// <summary>
        /// Database ID
        /// </summary>
        [Key]
        public int Id { get; set; }
    }
}