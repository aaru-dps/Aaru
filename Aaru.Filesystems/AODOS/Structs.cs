// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commodore file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the AO-DOS file system and shows information.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

// Information has been extracted looking at available disk images
// This may be missing fields, or not, I don't know russian so any help is appreciated
/// <inheritdoc />
/// <summary>Implements detection of the AO-DOS filesystem</summary>
public sealed partial class AODOS
{
#region Nested type: BootBlock

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BootBlock
    {
        /// <summary>A NOP opcode</summary>
        public readonly byte nop;
        /// <summary>A branch to real bootloader</summary>
        public readonly ushort branch;
        /// <summary>Unused</summary>
        public readonly byte unused;
        /// <summary>" AO-DOS "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] identifier;
        /// <summary>Volume label</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public readonly byte[] volumeLabel;
        /// <summary>How many files are present in disk</summary>
        public readonly ushort files;
        /// <summary>How many sectors are used</summary>
        public readonly ushort usedSectors;
    }

#endregion
}