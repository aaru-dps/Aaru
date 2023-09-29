// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft exFAT filesystem plugin.
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

using System;
using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

// Information from https://www.sans.org/reading-room/whitepapers/forensics/reverse-engineering-microsoft-exfat-file-system-33274
/// <inheritdoc />
/// <summary>Implements detection of the exFAT filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]

// ReSharper disable once InconsistentNaming
public sealed partial class exFAT
{
    readonly Guid   _oemFlashParameterGuid = new("0A0C7E46-3399-4021-90C8-FA6D389C4BA2");
    readonly byte[] _signature             = "EXFAT   "u8.ToArray();
    const string FS_TYPE = "exfat";
}