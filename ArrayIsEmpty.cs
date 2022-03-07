// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ArrayIsEmpty.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Helpers.
//
// --[ Description ] ----------------------------------------------------------
//
//     Methods for detecting an empty array.
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

namespace Aaru.Helpers;

using System.Linq;

/// <summary>Helper operations to work with arrays</summary>
public static partial class ArrayHelpers
{
    /// <summary>Checks if an array is null, filled with the NULL byte (0x00) or ASCII whitespace (0x20)</summary>
    /// <param name="array">Array</param>
    /// <returns>True if null or whitespace</returns>
    public static bool ArrayIsNullOrWhiteSpace(byte[] array) => array?.All(b => b == 0x00 || b == 0x20) != false;

    /// <summary>Checks if an array is null or filled with the NULL byte (0x00)</summary>
    /// <param name="array">Array</param>
    /// <returns>True if null</returns>
    public static bool ArrayIsNullOrEmpty(byte[] array) => array?.All(b => b == 0x00) != false;
}