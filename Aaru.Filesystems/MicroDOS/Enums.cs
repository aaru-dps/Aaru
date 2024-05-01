// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MicroDOS filesystem plugin
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

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>
///     Implements detection for the MicroDOS filesystem. Information from http://www.owg.ru/mkt/BK/MKDOS.TXT Thanks
///     to tarlabnor for translating it
/// </summary>
public sealed partial class MicroDOS
{
#region Nested type: FileStatus

    enum FileStatus : byte
    {
        CommonFile  = 0,
        Protected   = 1,
        LogicalDisk = 2,
        BadFile     = 0x80,
        Deleted     = 0xFF
    }

#endregion
}