// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Common structures for MODE pages decoding and encoding.
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

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    public struct BlockDescriptor
    {
        public DensityType Density;
        public ulong       Blocks;
        public uint        BlockLength;
    }

    public struct ModeHeader
    {
        public MediumTypes       MediumType;
        public bool              WriteProtected;
        public BlockDescriptor[] BlockDescriptors;
        public byte              Speed;
        public byte              BufferedMode;
        public bool              EBC;
        public bool              DPOFUA;
    }

    public struct ModePage
    {
        public byte   Page;
        public byte   Subpage;
        public byte[] PageResponse;
    }

    public struct DecodedMode
    {
        public ModeHeader Header;
        public ModePage[] Pages;
    }
}