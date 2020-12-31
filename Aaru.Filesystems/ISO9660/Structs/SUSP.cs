// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// In the loving memory of Facunda "Tata" Suárez Domínguez, R.I.P. 2019/07/24
// ****************************************************************************/

using System.Runtime.InteropServices;

// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems
{
    public sealed partial class ISO9660
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ContinuationArea
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
            public readonly uint   block;
            public readonly uint   block_be;
            public readonly uint   offset;
            public readonly uint   offset_be;
            public readonly uint   ca_length;
            public readonly uint   ca_length_be;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct PaddingArea
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct IndicatorArea
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
            public readonly ushort magic;
            public readonly byte   skipped;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct TerminatorArea
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct ReferenceArea
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
            public readonly byte   id_len;
            public readonly byte   des_len;
            public readonly byte   src_len;
            public readonly byte   ext_ver;

            // Follows extension identifier for id_len bytes
            // Follows extension descriptor for des_len bytes
            // Follows extension source for src_len bytes
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct SelectorArea
        {
            public readonly ushort signature;
            public readonly byte   length;
            public readonly byte   version;
            public readonly byte   sequence;
        }
    }
}