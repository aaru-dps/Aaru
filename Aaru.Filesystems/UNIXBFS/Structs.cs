// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UnixWare boot filesystem plugin.
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

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the UNIX boot filesystem</summary>
public sealed partial class BFS
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    struct SuperBlock
    {
        /// <summary>0x00, 0x1BADFACE</summary>
        public uint s_magic;
        /// <summary>0x04, start in bytes of volume</summary>
        public uint s_start;
        /// <summary>0x08, end in bytes of volume</summary>
        public uint s_end;
        /// <summary>0x0C, unknown :p</summary>
        public uint s_from;
        /// <summary>0x10, unknown :p</summary>
        public uint s_to;
        /// <summary>0x14, unknown :p</summary>
        public int s_bfrom;
        /// <summary>0x18, unknown :p</summary>
        public int s_bto;
        /// <summary>0x1C, 6 bytes, filesystem name</summary>
        public string s_fsname;
        /// <summary>0x22, 6 bytes, volume name</summary>
        public string s_volume;
    }
}