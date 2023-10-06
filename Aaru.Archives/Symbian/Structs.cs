// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Symbian.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Symbian plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Symbian installer (.sis) packages and shows information.
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

namespace Aaru.Archives;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedType.Local")]
public partial class Symbian
{
#region Nested type: SymbianHeader

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SymbianHeader
    {
        /// <summary>
        ///     Application UID before SymbianOS 9, magic after
        /// </summary>
        public uint uid1;
        /// <summary>
        ///     EPOC release magic before SOS 9, NULLs after
        /// </summary>
        public uint uid2;
        /// <summary>
        ///     Application UID after SOS 9, magic before
        /// </summary>
        public uint uid3;
        /// <summary>
        ///     Checksum of UIDs 1 to 3
        /// </summary>
        public uint uid4;
        /// <summary>
        ///     CRC16 of all header
        /// </summary>
        public ushort crc16;
        /// <summary>
        ///     Number of languages
        /// </summary>
        public ushort languages;
        /// <summary>
        ///     Number of files
        /// </summary>
        public ushort files;
        /// <summary>
        ///     Number of requisites
        /// </summary>
        public ushort requisites;
        /// <summary>
        ///     Installed language (only residual SIS)
        /// </summary>
        public ushort inst_lang;
        /// <summary>
        ///     Installed files (only residual SIS)
        /// </summary>
        public ushort inst_files;
        /// <summary>
        ///     Installed drive (only residual SIS), NULL or 0x0021
        /// </summary>
        public ushort inst_drive;
        /// <summary>
        ///     Number of capabilities
        /// </summary>
        public ushort capabilities;
        /// <summary>
        ///     Version of Symbian Installer required
        /// </summary>
        public uint inst_version;
        /// <summary>
        ///     Option flags
        /// </summary>
        public SymbianOptions options;
        /// <summary>
        ///     Type
        /// </summary>
        public SymbianType type;
        /// <summary>
        ///     Major version of application
        /// </summary>
        public ushort major;
        /// <summary>
        ///     Minor version of application
        /// </summary>
        public ushort minor;
        /// <summary>
        ///     Variant when SIS is a prerequisite for other SISs
        /// </summary>
        public uint variant;
        /// <summary>
        ///     Pointer to language records
        /// </summary>
        public uint lang_ptr;
        /// <summary>
        ///     Pointer to file records
        /// </summary>
        public uint files_ptr;
        /// <summary>
        ///     Pointer to requisite records
        /// </summary>
        public uint reqs_ptr;
        /// <summary>
        ///     Pointer to certificate records
        /// </summary>
        public uint certs_ptr;
        /// <summary>
        ///     Pointer to component name record
        /// </summary>
        public uint comp_ptr;
        // From EPOC Release 6
        /// <summary>
        ///     Pointer to signature record
        /// </summary>
        public uint sig_ptr;
        /// <summary>
        ///     Pointer to capability records
        /// </summary>
        public uint caps_ptr;
        /// <summary>
        ///     Installed space (only residual SIS)
        /// </summary>
        public uint instspace;
        /// <summary>
        ///     Space required
        /// </summary>
        public uint maxinsspc;
        /// <summary>
        ///     Reserved
        /// </summary>
        public ulong reserved1;
        /// <summary>
        ///     Reserved
        /// </summary>
        public ulong reserved2;
    }

#endregion
}