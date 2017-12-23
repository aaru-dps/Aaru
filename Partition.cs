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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;

namespace DiscImageChef.CommonTypes
{
    /// <summary>
    ///     Partition structure.
    /// </summary>
    public struct Partition : IEquatable<Partition>, IComparable<Partition>
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
        public ulong End => Start + Length - 1;
        /// <summary>Name of partition scheme that contains this partition</summary>
        public string Scheme;

        /// <summary>
        ///     Compares two partitions
        /// </summary>
        /// <param name="other">Partition to compare with</param>
        /// <returns>0 if both partitions start and end at the same sector</returns>
        public bool Equals(Partition other)
        {
            return Start == other.Start && Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is Partition)) return false;

            return Equals((Partition)obj);
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() + End.GetHashCode();
        }

        /// <summary>
        ///     Compares this partition with another and returns an integer that indicates whether the current partition precedes,
        ///     follows, or is in the same place as the other partition.
        /// </summary>
        /// <param name="other">Partition to compare with</param>
        /// <returns>A value that indicates the relative equality of the partitions being compared.</returns>
        public int CompareTo(Partition other)
        {
            if(Start == other.Start && End == other.End) return 0;

            if(Start > other.Start || End > other.End) return 1;

            return -1;
        }

        // Define the equality operator.
        public static bool operator ==(Partition operand1, Partition operand2)
        {
            return operand1.Equals(operand2);
        }

        // Define the inequality operator.
        public static bool operator !=(Partition operand1, Partition operand2)
        {
            return !operand1.Equals(operand2);
        }

        // Define the is greater than operator.
        public static bool operator >(Partition operand1, Partition operand2)
        {
            return operand1.CompareTo(operand2) == 1;
        }

        // Define the is less than operator.
        public static bool operator <(Partition operand1, Partition operand2)
        {
            return operand1.CompareTo(operand2) == -1;
        }

        // Define the is greater than or equal to operator.
        public static bool operator >=(Partition operand1, Partition operand2)
        {
            return operand1.CompareTo(operand2) >= 0;
        }

        // Define the is less than or equal to operator.
        public static bool operator <=(Partition operand1, Partition operand2)
        {
            return operand1.CompareTo(operand2) <= 0;
        }
    }
}