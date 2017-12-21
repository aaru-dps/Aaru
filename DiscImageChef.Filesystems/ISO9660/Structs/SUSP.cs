// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SUSP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : ISO9660 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     SUSP extensions structures.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ContinuationArea
        {
            public ushort signature;
            public byte length;
            public byte version;
            public uint block;
            public uint block_be;
            public uint offset;
            public uint offset_be;
            public uint ca_length;
            public uint ca_length_be;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PaddingArea
        {
            public ushort signature;
            public byte length;
            public byte version;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct IndicatorArea
        {
            public ushort signature;
            public byte length;
            public byte version;
            public ushort magic;
            public byte skipped;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct TerminatorArea
        {
            public ushort signature;
            public byte length;
            public byte version;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ReferenceArea
        {
            public ushort signature;
            public byte length;
            public byte version;
            public byte id_len;
            public byte des_len;
            public byte src_len;
            public byte ext_ver;
            // Follows extension identifier for id_len bytes
            // Follows extension descriptor for des_len bytes
            // Follows extension source for src_len bytes
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SelectorArea
        {
            public ushort signature;
            public byte length;
            public byte version;
            public byte sequence;
        }
    }
}