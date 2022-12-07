// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Nintendo optical filesystems plugin.
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

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the filesystem used by Nintendo Gamecube and Wii discs</summary>
public sealed partial class NintendoPlugin
{
    struct NintendoFields
    {
        public string              DiscType;
        public string              GameCode;
        public string              RegionCode;
        public string              PublisherCode;
        public string              DiscId;
        public byte                DiscNumber;
        public byte                DiscVersion;
        public bool                Streaming;
        public byte                StreamBufferSize;
        public string              Title;
        public uint                DebugOff;
        public uint                DebugAddr;
        public uint                DolOff;
        public uint                FstOff;
        public uint                FstSize;
        public uint                FstMax;
        public NintendoPartition[] FirstPartitions;
        public NintendoPartition[] SecondPartitions;
        public NintendoPartition[] ThirdPartitions;
        public NintendoPartition[] FourthPartitions;
        public byte                Region;
        public byte                JapanAge;
        public byte                UsaAge;
        public byte                GermanAge;
        public byte                PegiAge;
        public byte                FinlandAge;
        public byte                PortugalAge;
        public byte                UkAge;
        public byte                AustraliaAge;
        public byte                KoreaAge;
    }

    struct NintendoPartition
    {
        public uint Offset;
        public uint Type;
    }
}