// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Partition.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru common types.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains common partition types.
//
// --[ License ] --------------------------------------------------------------
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the
//     "Software"), to deal in the Software without restriction, including
//     without limitation the rights to use, copy, modify, merge, publish,
//     distribute, sublicense, and/or sell copies of the Software, and to
//     permit persons to whom the Software is furnished to do so, subject to
//     the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//     MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//     IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//     CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//     TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//     SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;

namespace Aaru.CommonTypes
{
    /// <summary>Partition structure.</summary>
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

        /// <summary>Compares two partitions</summary>
        /// <param name="other">Partition to compare with</param>
        /// <returns>0 if both partitions start and end at the same sector</returns>
        public bool Equals(Partition other) => Start == other.Start && Length == other.Length;

        public override bool Equals(object obj) => obj is Partition partition && Equals(partition);

        public override int GetHashCode() => Start.GetHashCode() + End.GetHashCode();

        /// <summary>
        ///     Compares this partition with another and returns an integer that indicates whether the current partition
        ///     precedes, follows, or is in the same place as the other partition.
        /// </summary>
        /// <param name="other">Partition to compare with</param>
        /// <returns>A value that indicates the relative equality of the partitions being compared.</returns>
        public int CompareTo(Partition other)
        {
            if(Start == other.Start &&
               End   == other.End)
                return 0;

            if(Start > other.Start ||
               End   > other.End)
                return 1;

            return -1;
        }

        // Define the equality operator.
        public static bool operator ==(Partition operand1, Partition operand2) => operand1.Equals(operand2);

        // Define the inequality operator.
        public static bool operator !=(Partition operand1, Partition operand2) => !operand1.Equals(operand2);

        // Define the is greater than operator.
        public static bool operator >(Partition operand1, Partition operand2) => operand1.CompareTo(operand2) == 1;

        // Define the is less than operator.
        public static bool operator <(Partition operand1, Partition operand2) => operand1.CompareTo(operand2) == -1;

        // Define the is greater than or equal to operator.
        public static bool operator >=(Partition operand1, Partition operand2) => operand1.CompareTo(operand2) >= 0;

        // Define the is less than or equal to operator.
        public static bool operator <=(Partition operand1, Partition operand2) => operand1.CompareTo(operand2) <= 0;
    }
}