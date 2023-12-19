// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Commodore.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Commodore (pre-Amiga) floppy structures.
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Decoders.Floppy;

/// <summary>Methods and structures for Commodore GCR floppy decoding</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Commodore
{
#region Nested type: SectorData

    /// <summary>Decoded Commodore GCR sector data</summary>
    public struct SectorData
    {
        /// <summary>Always 0x07</summary>
        public byte id;
        /// <summary>User data</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte data;
        /// <summary>XOR of <see cref="data" /></summary>
        public byte checksum;
        /// <summary>Filled with 0x0F</summary>
        public ushort fill;
    }

#endregion

#region Nested type: SectorHeader

    /// <summary>Decoded Commodore GCR sector header</summary>
    public struct SectorHeader
    {
        /// <summary>Always 0x08</summary>
        public byte id;
        /// <summary>XOR of following fields</summary>
        public byte checksum;
        /// <summary>Sector number</summary>
        public byte sector;
        /// <summary>Track number</summary>
        public byte track;
        /// <summary>Format ID, unknown meaning</summary>
        public ushort format;
        /// <summary>Filled with 0x0F</summary>
        public ushort fill;
    }

#endregion
}