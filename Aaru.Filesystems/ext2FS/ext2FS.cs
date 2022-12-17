// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ext2FS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux extended filesystem 2, 3 and 4 plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Linux extended filesystem 2, 3 and 4 and shows information.
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
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the Linux extended filesystem v2, v3 and v4</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]

// ReSharper disable once InconsistentNaming
public sealed partial class ext2FS : IFilesystem
{
    /// <inheritdoc />
    public string Name => Localization.ext2FS_Name_Linux_extended_Filesystem_2_3_and_4;
    /// <inheritdoc />
    public Guid Id => new("6AA91B88-150B-4A7B-AD56-F84FB2DF4184");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;
}