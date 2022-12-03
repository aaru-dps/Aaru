// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for Microsoft Hyper-V disk images.
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
// ----------------------------------------------------------------------------c
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class Vhdx
{
    const ulong VHDX_SIGNATURE    = 0x656C696678646876;
    const uint  VHDX_HEADER_SIG   = 0x64616568;
    const uint  VHDX_REGION_SIG   = 0x69676572;
    const ulong VHDX_METADATA_SIG = 0x617461646174656D;

    const string PARENT_LINKAGE_KEY      = "parent_linkage";
    const string PARENT_LINKAGE2_KEY     = "parent_linkage2";
    const string RELATIVE_PATH_KEY       = "relative_path";
    const string VOLUME_PATH_KEY         = "volume_path";
    const string ABSOLUTE_WIN32_PATH_KEY = "absolute_win32_path";

    const uint REGION_FLAGS_REQUIRED = 0x01;

    const uint METADATA_FLAGS_USER     = 0x01;
    const uint METADATA_FLAGS_VIRTUAL  = 0x02;
    const uint METADATA_FLAGS_REQUIRED = 0x04;

    const uint FILE_FLAGS_LEAVE_ALLOCATED = 0x01;
    const uint FILE_FLAGS_HAS_PARENT      = 0x02;

    /// <summary>Block has never been stored on this image, check parent</summary>
    const ulong PAYLOAD_BLOCK_NOT_PRESENT = 0x00;
    /// <summary>Block was stored on this image and is removed, return whatever data you wish</summary>
    const ulong PAYLOAD_BLOCK_UNDEFINED = 0x01;
    /// <summary>Block is filled with zeroes</summary>
    const ulong PAYLOAD_BLOCK_ZERO = 0x02;
    /// <summary>All sectors in this block were UNMAPed/TRIMed, return zeroes</summary>
    const ulong PAYLOAD_BLOCK_UNMAPPER = 0x03;
    /// <summary>Block is present on this image</summary>
    const ulong PAYLOAD_BLOCK_FULLY_PRESENT = 0x06;
    /// <summary>Block is present on image but there may be sectors present on parent image</summary>
    const ulong PAYLOAD_BLOCK_PARTIALLY_PRESENT = 0x07;

    const ulong SECTOR_BITMAP_NOT_PRESENT = 0x00;
    const ulong SECTOR_BITMAP_PRESENT     = 0x06;

    const ulong BAT_FILE_OFFSET_MASK = 0xFFFFFFFFFFFC0000;
    const ulong BAT_FLAGS_MASK       = 0x7;
    const ulong BAT_RESERVED_MASK    = 0x3FFF8;

    const    int  MAX_CACHE_SIZE          = 16777216;
    readonly Guid _batGuid                = new("2DC27766-F623-4200-9D64-115E9BFD4A08");
    readonly Guid _fileParametersGuid     = new("CAA16737-FA36-4D43-B3B6-33F0AA44E76B");
    readonly Guid _logicalSectorSizeGuid  = new("8141BF1D-A96F-4709-BA47-F233A8FAAB5F");
    readonly Guid _metadataGuid           = new("8B7CA206-4790-4B9A-B8FE-575F050F886E");
    readonly Guid _page83DataGuid         = new("BECA12AB-B2E6-4523-93EF-C309E000C746");
    readonly Guid _parentLocatorGuid      = new("A8D35F2D-B30B-454D-ABF7-D3D84834AB0C");
    readonly Guid _parentTypeVhdxGuid     = new("B04AEFB7-D19E-4A81-B789-25B8E9445913");
    readonly Guid _physicalSectorSizeGuid = new("CDA348C7-445D-4471-9CC9-E9885251C556");
    readonly Guid _virtualDiskSizeGuid    = new("2FA54224-CD1B-4876-B211-5DBED83BF4B8");
}