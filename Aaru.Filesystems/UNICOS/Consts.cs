// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNICOS filesystem plugin.
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

// UNICOS is ILP64 so let's think everything is 64-bit

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection for the Cray UNICOS filesystem</summary>
public sealed partial class UNICOS
{
    const int NC1_MAXPART = 64;
    const int NC1_MAXIREG = 4;

    const ulong UNICOS_MAGIC  = 0x6e6331667331636e;
    const ulong UNICOS_SECURE = 0xcd076d1771d670cd;

    const string FS_TYPE = "unicos";
}