// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Adler32Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements an Adler-32 algorithm.
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
// ****************************************************************************/

using System.IO;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Checksums
{
    /// <inheritdoc />
    /// <summary>Implements the Adler-32 algorithm</summary>
    public sealed class Adler32Context : IChecksum
    {
        const ushort ADLER_MODULE = 65521;
        ushort       _sum1, _sum2;

        /// <summary>Initializes the Adler-32 sums</summary>
        public Adler32Context()
        {
            _sum1 = 1;
            _sum2 = 0;
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++)
            {
                _sum1 = (ushort)((_sum1 + data[i]) % ADLER_MODULE);
                _sum2 = (ushort)((_sum2 + _sum1)   % ADLER_MODULE);
            }
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data) => Update(data, (uint)data.Length);

        /// <inheritdoc />
        /// <summary>Returns a byte array of the hash value.</summary>
        public byte[] Final()
        {
            uint finalSum = (uint)((_sum2 << 16) | _sum1);

            return BigEndianBitConverter.GetBytes(finalSum);
        }

        /// <inheritdoc />
        /// <summary>Returns a hexadecimal representation of the hash value.</summary>
        public string End()
        {
            uint finalSum    = (uint)((_sum2 << 16) | _sum1);
            var  adlerOutput = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
                adlerOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

            return adlerOutput.ToString();
        }

        /// <summary>Gets the hash of a file</summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            File(filename, out byte[] hash);

            return hash;
        }

        /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash)
        {
            var fileStream = new FileStream(filename, FileMode.Open);

            ushort localSum1 = 1;
            ushort localSum2 = 0;

            for(int i = 0; i < fileStream.Length; i++)
            {
                localSum1 = (ushort)((localSum1 + fileStream.ReadByte()) % ADLER_MODULE);
                localSum2 = (ushort)((localSum2 + localSum1)             % ADLER_MODULE);
            }

            uint finalSum = (uint)((localSum2 << 16) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            var adlerOutput = new StringBuilder();

            foreach(byte h in hash)
                adlerOutput.Append(h.ToString("x2"));

            fileStream.Close();

            return adlerOutput.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            ushort localSum1 = 1;
            ushort localSum2 = 0;

            for(int i = 0; i < len; i++)
            {
                localSum1 = (ushort)((localSum1 + data[i])   % ADLER_MODULE);
                localSum2 = (ushort)((localSum2 + localSum1) % ADLER_MODULE);
            }

            uint finalSum = (uint)((localSum2 << 16) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            var adlerOutput = new StringBuilder();

            foreach(byte h in hash)
                adlerOutput.Append(h.ToString("x2"));

            return adlerOutput.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash) => Data(data, (uint)data.Length, out hash);
    }
}