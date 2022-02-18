// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Amiga.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Commodore Amiga floppy structures.
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
using System.Runtime.InteropServices;

namespace Aaru.Decoders.Floppy
{
    /// <summary>Methods and structures for Commodore Amiga decoding</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class Amiga
    {
        public struct Sector
        {
            /// <summary>Set to 0x00</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] zero;
            /// <summary>Set to 0xA1</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] sync;
            /// <summary>Set to 0xFF</summary>
            public byte amiga;
            /// <summary>Track number</summary>
            public byte track;
            /// <summary>Sector number</summary>
            public byte sector;
            /// <summary>Remaining sectors til end of writing</summary>
            public byte remaining;
            /// <summary>OS dependent tag</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] label;
            /// <summary>Checksum from <see cref="amiga" /> to <see cref="label" /></summary>
            public uint headerChecksum;
            /// <summary>Checksum from <see cref="data" /></summary>
            public uint dataChecksum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] data;
        }
    }
}