// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SolarOS filesystem plugin.
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

using System.Diagnostics.CodeAnalysis;

namespace Aaru.Filesystems;

// Based on FAT's BPB, cannot find a FAT or directory
/// <inheritdoc />
/// <summary>Implements detection of the Solar OS filesystem</summary>
public sealed partial class SolarFS
{
#region Nested type: BiosParameterBlock

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    struct BiosParameterBlock
    {
        /// <summary>0x00, x86 jump (3 bytes), jumps to 0x60</summary>
        public byte[] x86_jump;
        /// <summary>0x03, 8 bytes, "SOLAR_OS"</summary>
        public string OEMName;
        /// <summary>0x0B, Bytes per sector</summary>
        public ushort bps;
        /// <summary>0x0D, unknown, 0x01</summary>
        public byte unk1;
        /// <summary>0x0E, unknown, 0x0201</summary>
        public ushort unk2;
        /// <summary>0x10, Number of entries on root directory ? (no root directory found)</summary>
        public ushort root_ent;
        /// <summary>0x12, Sectors in volume</summary>
        public ushort sectors;
        /// <summary>0x14, Media descriptor</summary>
        public byte media;
        /// <summary>0x15, Sectors per FAT ? (no FAT found)</summary>
        public ushort spfat;
        /// <summary>0x17, Sectors per track</summary>
        public ushort sptrk;
        /// <summary>0x19, Heads</summary>
        public ushort heads;
        /// <summary>0x1B, unknown, 10 bytes, zero-filled</summary>
        public byte[] unk3;
        /// <summary>0x25, 0x29</summary>
        public byte signature;
        /// <summary>0x26, unknown, zero-filled</summary>
        public uint unk4;
        /// <summary>0x2A, 11 bytes, volume name, space-padded</summary>
        public string vol_name;
        /// <summary>0x35, 8 bytes, "SOL_FS  "</summary>
        public string fs_type;
    }

#endregion
}