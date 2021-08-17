// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CRC64Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements a CRC64 algorithm.
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
    /// <summary>Implements a CRC64 algorithm</summary>
    public sealed class Crc64Context : IChecksum
    {
        /// <summary>
        /// ECMA CRC64 polynomial
        /// </summary>
        public const ulong CRC64_ECMA_POLY = 0xC96C5795D7870F42;
        /// <summary>
        /// ECMA CRC64 seed
        /// </summary>
        public const ulong CRC64_ECMA_SEED = 0xFFFFFFFFFFFFFFFF;

        readonly ulong   _finalSeed;
        readonly ulong[] _table;
        ulong            _hashInt;

        /// <summary>Initializes the CRC64 table and seed as CRC64-ECMA</summary>
        public Crc64Context()
        {
            _hashInt = CRC64_ECMA_SEED;

            _table = new ulong[256];

            for(int i = 0; i < 256; i++)
            {
                ulong entry = (ulong)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ CRC64_ECMA_POLY;
                    else
                        entry = entry >> 1;

                _table[i] = entry;
            }

            _finalSeed = CRC64_ECMA_SEED;
        }

        /// <summary>Initializes the CRC16 table with a custom polynomial and seed</summary>
        public Crc64Context(ulong polynomial, ulong seed)
        {
            _hashInt = seed;

            _table = new ulong[256];

            for(int i = 0; i < 256; i++)
            {
                ulong entry = (ulong)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;

                _table[i] = entry;
            }

            _finalSeed = seed;
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++)
                _hashInt = (_hashInt >> 8) ^ _table[data[i] ^ (_hashInt & 0xff)];
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data) => Update(data, (uint)data.Length);

        /// <inheritdoc />
        /// <summary>Returns a byte array of the hash value.</summary>
        public byte[] Final() => BigEndianBitConverter.GetBytes(_hashInt ^= _finalSeed);

        /// <inheritdoc />
        /// <summary>Returns a hexadecimal representation of the hash value.</summary>
        public string End()
        {
            var crc64Output = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(_hashInt ^= _finalSeed).Length; i++)
                crc64Output.Append(BigEndianBitConverter.GetBytes(_hashInt ^= _finalSeed)[i].ToString("x2"));

            return crc64Output.ToString();
        }

        /// <summary>Gets the hash of a file</summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            File(filename, out byte[] localHash);

            return localHash;
        }

        /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash) =>
            File(filename, out hash, CRC64_ECMA_POLY, CRC64_ECMA_SEED);

        /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        /// <param name="polynomial">CRC polynomial</param>
        /// <param name="seed">CRC seed</param>
        public static string File(string filename, out byte[] hash, ulong polynomial, ulong seed)
        {
            var fileStream = new FileStream(filename, FileMode.Open);

            ulong localhashInt = seed;

            ulong[] localTable = new ulong[256];

            for(int i = 0; i < 256; i++)
            {
                ulong entry = (ulong)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;

                localTable[i] = entry;
            }

            for(int i = 0; i < fileStream.Length; i++)
                localhashInt = (localhashInt >> 8) ^ localTable[(ulong)fileStream.ReadByte() ^ (localhashInt & 0xffL)];

            localhashInt ^= seed;
            hash         =  BigEndianBitConverter.GetBytes(localhashInt);

            var crc64Output = new StringBuilder();

            foreach(byte h in hash)
                crc64Output.Append(h.ToString("x2"));

            fileStream.Close();

            return crc64Output.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash) =>
            Data(data, len, out hash, CRC64_ECMA_POLY, CRC64_ECMA_SEED);

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        /// <param name="polynomial">CRC polynomial</param>
        /// <param name="seed">CRC seed</param>
        public static string Data(byte[] data, uint len, out byte[] hash, ulong polynomial, ulong seed)
        {
            ulong localhashInt = seed;

            ulong[] localTable = new ulong[256];

            for(int i = 0; i < 256; i++)
            {
                ulong entry = (ulong)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;

                localTable[i] = entry;
            }

            for(int i = 0; i < len; i++)
                localhashInt = (localhashInt >> 8) ^ localTable[data[i] ^ (localhashInt & 0xff)];

            localhashInt ^= seed;
            hash         =  BigEndianBitConverter.GetBytes(localhashInt);

            var crc64Output = new StringBuilder();

            foreach(byte h in hash)
                crc64Output.Append(h.ToString("x2"));

            return crc64Output.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash) => Data(data, (uint)data.Length, out hash);
    }
}