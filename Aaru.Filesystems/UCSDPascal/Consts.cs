// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : U.C.S.D. Pascal filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     U.C.S.D. Pascal filesystem constants.
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

namespace Aaru.Filesystems.UCSDPascal;

using System.Diagnostics.CodeAnalysis;

// Information from Call-A.P.P.L.E. Pascal Disk Directory Structure
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class PascalPlugin
{
    enum PascalFileKind : short
    {
        /// <summary>Disk volume entry</summary>
        Volume = 0,
        /// <summary>File containing bad blocks</summary>
        Bad,
        /// <summary>Code file, machine executable</summary>
        Code,
        /// <summary>Text file, human readable</summary>
        Text,
        /// <summary>Information file for debugger</summary>
        Info,
        /// <summary>Data file</summary>
        Data,
        /// <summary>Graphics vectors</summary>
        Graf,
        /// <summary>Graphics screen image</summary>
        Foto,
        /// <summary>Security, not used</summary>
        Secure
    }
}