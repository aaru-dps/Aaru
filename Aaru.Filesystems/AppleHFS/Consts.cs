// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Consts.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Apple Hierarchical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Apple Hierarchical File System constants.
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

using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming

namespace Aaru.Filesystems;

// Information from Inside Macintosh
// https://developer.apple.com/legacy/library/documentation/mac/pdf/Files/File_Manager.pdf
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class AppleHFS
{
    /// <summary>Parent ID of the root directory.</summary>
    const uint kRootParentCnid = 1;
    /// <summary>Directory ID of the root directory.</summary>
    const uint kRootCnid = 2;
    /// <summary>File number of the extents file.</summary>
    const uint kExtentsFileCnid = 3;
    /// <summary>File number of the catalog file.</summary>
    const uint kCatalogFileCnid = 4;
    /// <summary>File number of the bad allocation block file.</summary>
    const uint kBadBlocksFileCnid = 5;
}