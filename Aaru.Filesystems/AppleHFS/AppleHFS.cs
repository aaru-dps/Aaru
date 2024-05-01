// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleHFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Constructors and common variables for the Apple Hierarchical File System
//     plugin.
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

using System;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Filesystems;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
/// <inheritdoc />
/// <summary>Implements detection of the Apple Hierarchical File System (HFS)</summary>
public sealed partial class AppleHFS : IFilesystem
{
#region IFilesystem Members

    /// <inheritdoc />
    public string Name => Localization.Name_Apple_Hierarchical_File_System;

    /// <inheritdoc />
    public Guid Id => new("36405F8D-0D26-6ECC-0BBB-1D5225FF404F");

    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

#endregion
}