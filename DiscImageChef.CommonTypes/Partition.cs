// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Partition.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef common types.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains common partition types.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

namespace DiscImageChef.CommonTypes
{
    /// <summary>
    /// Partition structure.
    /// </summary>
    public struct Partition
    {
        /// <summary>Partition number, 0-started</summary>
        public ulong Sequence;
        /// <summary>Partition type</summary>
        public string Type;
        /// <summary>Partition name (if the scheme supports it)</summary>
        public string Name;
        /// <summary>Start of the partition, in bytes</summary>
        public ulong Offset;
        /// <summary>LBA of partition start</summary>
        public ulong Start;
        /// <summary>Length in bytes of the partition</summary>
        public ulong Size;
        /// <summary>Length in sectors of the partition</summary>
        public ulong Length;
        /// <summary>Information that does not find space in this struct</summary>
        public string Description;
        /// <summary>LBA of last partition sector</summary>
        public ulong End { get { return Start + Length - 1; }}
        /// <summary>Name of partition scheme that contains this partition</summary>
        public string Scheme;
    }
}

