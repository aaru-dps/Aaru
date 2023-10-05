// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Constants.cs
// Author(s)      : Rebecca Wallander <sakcheen@gmail.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains constants for A2R flux images.
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
// Copyright © 2011-2023 Rebecca Wallander
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;

namespace Aaru.DiscImages;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public sealed partial class A2R
{
    readonly byte[] _a2Rv2Signature =
    {
        0x41, 0x32, 0x52, 0x32 // A2R2
    };

    readonly byte[] _a2Rv3Signature =
    {
        0x41, 0x32, 0x52, 0x33 // A2R3
    };

    readonly byte[] _infoChunkSignature =
    {
        0x49, 0x4E, 0x46, 0x4F // INFO
    };

    readonly byte[] _metaChunkSignature =
    {
        0x4D, 0x45, 0x54, 0x41 // META
    };

    readonly byte[] _rwcpChunkSignature =
    {
        0x52, 0x57, 0x43, 0x50 // RWCP
    };

    readonly byte[] _slvdChunkSignature =
    {
        0x53, 0x4C, 0x56, 0x44 // SLVD
    };

    readonly byte[] _strmChunkSignature =
    {
        0x53, 0x54, 0x52, 0x4D // STRM
    };
}