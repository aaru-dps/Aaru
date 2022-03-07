// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures for XGS emulator disk images.
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

namespace Aaru.DiscImages;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

public sealed partial class Apple2Mg
{
    [SuppressMessage("ReSharper", "NotAccessedField.Local"), StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Header
    {
        /// <summary>Offset 0x00, magic</summary>
        public uint Magic;
        /// <summary>Offset 0x04, disk image creator ID</summary>
        public uint Creator;
        /// <summary>Offset 0x08, header size, constant 0x0040</summary>
        public ushort HeaderSize;
        /// <summary>Offset 0x0A, disk image version</summary>
        public ushort Version;
        /// <summary>Offset 0x0C, disk image format</summary>
        public SectorOrder ImageFormat;
        /// <summary>Offset 0x10, flags and volume number</summary>
        public uint Flags;
        /// <summary>Offset 0x14, blocks for ProDOS, 0 otherwise</summary>
        public uint Blocks;
        /// <summary>Offset 0x18, offset to data</summary>
        public uint DataOffset;
        /// <summary>Offset 0x1C, data size in bytes</summary>
        public uint DataSize;
        /// <summary>Offset 0x20, offset to optional comment</summary>
        public uint CommentOffset;
        /// <summary>Offset 0x24, length of optional comment</summary>
        public uint CommentSize;
        /// <summary>Offset 0x28, offset to creator specific chunk</summary>
        public readonly uint CreatorSpecificOffset;
        /// <summary>Offset 0x2C, creator specific chunk size</summary>
        public readonly uint CreatorSpecificSize;
        /// <summary>Offset 0x30, reserved, should be zero</summary>
        public readonly uint Reserved1;
        /// <summary>Offset 0x34, reserved, should be zero</summary>
        public readonly uint Reserved2;
        /// <summary>Offset 0x38, reserved, should be zero</summary>
        public readonly uint Reserved3;
        /// <summary>Offset 0x3C, reserved, should be zero</summary>
        public readonly uint Reserved4;
    }
}