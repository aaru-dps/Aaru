// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Acorn.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Acorn RISC OS system area structures.
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
// Copyright © 2011-2026 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

public sealed partial class ISO9660
{
    /// <summary>Size of the Acorn system area structure.</summary>
    const int ACORN_SYSTEM_AREA_SIZE = 32;

    /// <summary>Acorn RISC OS system area magic signature.</summary>
    static readonly byte[] _acornMagic = "ARCHIMEDES"u8.ToArray();

    /// <summary>Acorn RISC OS system area structure, exactly 32 bytes.</summary>
    /// <remarks>
    ///     This structure is found in the system use area of ISO9660 directory records on discs created with Acorn RISC
    ///     OS tools, and by mkisofs with the ARCHIMEDES patch. It stores the RISC OS load and execution addresses, which
    ///     contain the filetype and date stamp, and the RISC OS attributes.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct AcornSystemArea
    {
        /// <summary>Magic signature "ARCHIMEDES" (10 bytes).</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] Signature;

        /// <summary>Load address at offset 10, &amp;FFFtttdd for files with a filetype (ttt) and date stamp (dd).</summary>
        public readonly uint LoadAddress;

        /// <summary>Execution address at offset 14, the low 32 bits of the date stamp for files with a filetype.</summary>
        public readonly uint ExecAddress;

        /// <summary>
        ///     Attributes at offset 18, the RISC OS access permissions in the low byte, and bit 8 set if the initial
        ///     underscore of the filename stands for an exclamation mark.
        /// </summary>
        public readonly uint Attributes;

        /// <summary>Reserved bytes at offset 22-31 (10 bytes).</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] Reserved;

        /// <summary>Gets a value indicating whether this entry represents an application (starts with '!').</summary>
        public bool IsApplication => (Attributes & 0x100) == 0x100;

        /// <summary>Gets a value indicating whether the load address holds a filetype and date stamp.</summary>
        public bool HasFiletype => (LoadAddress & 0xFFF00000) == 0xFFF00000;

        /// <summary>Gets the RISC OS filetype if present, otherwise null.</summary>
        public ushort? Filetype => HasFiletype ? (ushort)(LoadAddress >> 8 & 0xFFF) : null;
    }
}