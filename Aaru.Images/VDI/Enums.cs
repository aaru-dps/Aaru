// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains enumerations for VirtualBox disk images.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed partial class Vdi
    {
        enum VdiImageType : uint
        {
            /// <summary> Normal dynamically growing base image file.</summary>
            Normal = 1,
            /// <summary>Preallocated base image file of a fixed size.</summary>
            Fixed,
            /// <summary>Dynamically growing image file for undo/commit changes support.</summary>
            Undo,
            /// <summary>Dynamically growing image file for differencing support.</summary>
            Differential,

            /// <summary>First valid image type value.</summary>
            First = Normal,
            /// <summary>Last valid image type value.</summary>
            Last = Differential
        }

        enum VdiImageFlags : uint
        {
            /// <summary>
            ///     Fill new blocks with zeroes while expanding image file. Only valid for newly created images, never set for
            ///     opened existing images.
            /// </summary>
            ZeroExpand = 0x100
        }
    }
}