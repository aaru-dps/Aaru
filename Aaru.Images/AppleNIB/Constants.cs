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
//     Contains constants for Apple nibbelized disk images.
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

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class AppleNib
{
    readonly byte[] _apple3Sign =
    {
        0x8D, 0xD0, 0x03, 0x4C, 0xC7, 0xA4
    };
    readonly byte[] _cpmSign =
    {
        0xA2, 0x55, 0xA9, 0x00, 0x9D, 0x00, 0x0D, 0xCA
    };
    readonly byte[] _dosSign =
    {
        0xA2, 0x02, 0x8E, 0x52
    };
    readonly ulong[] _dosSkewing =
    {
        0, 7, 14, 6, 13, 5, 12, 4, 11, 3, 10, 2, 9, 1, 8, 15
    };
    readonly byte[] _driString =
    {
        0x43, 0x4F, 0x50, 0x59, 0x52, 0x49, 0x47, 0x48, 0x54, 0x20, 0x28, 0x43, 0x29, 0x20, 0x31, 0x39, 0x37, 0x39,
        0x2C, 0x20, 0x44, 0x49, 0x47, 0x49, 0x54, 0x41, 0x4C, 0x20, 0x52, 0x45, 0x53, 0x45, 0x41, 0x52, 0x43, 0x48
    };
    readonly byte[] _pascalSign =
    {
        0x08, 0xA5, 0x0F, 0x29
    };
    readonly byte[] _pascalString =
    {
        0x53, 0x59, 0x53, 0x54, 0x45, 0x2E, 0x41, 0x50, 0x50, 0x4C, 0x45
    };
    readonly byte[] _pascal2Sign =
    {
        0xFF, 0xA2, 0x00, 0x8E
    };
    readonly byte[] _prodosString =
    {
        0x50, 0x52, 0x4F, 0x44, 0x4F, 0x53
    };
    readonly ulong[] _proDosSkewing =
    {
        0, 8, 1, 9, 2, 10, 3, 11, 4, 12, 5, 13, 6, 14, 7, 15
    };
    readonly byte[] _sosSign =
    {
        0xC9, 0x20, 0xF0, 0x3E
    };
}