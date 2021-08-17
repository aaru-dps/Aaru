// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CRC32Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements a CRC32 algorithm.
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
    /// <summary>Implements a CRC32 algorithm</summary>
    public sealed class Crc32Context : IChecksum
    {
        const uint CRC32_ISO_POLY = 0xEDB88320;
        const uint CRC32_ISO_SEED = 0xFFFFFFFF;

        readonly uint   _finalSeed;
        readonly uint[] _table;
        uint            _hashInt;

        /// <summary>Initializes the CRC32 table and seed as CRC32-ISO</summary>
        public Crc32Context()
        {
            _hashInt   = CRC32_ISO_SEED;
            _finalSeed = CRC32_ISO_SEED;

            _table = new uint[256];

            for(int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ CRC32_ISO_POLY;
                    else
                        entry >>= 1;

                _table[i] = entry;
            }
        }

        /// <summary>Initializes the CRC32 table with a custom polynomial and seed</summary>
        public Crc32Context(uint polynomial, uint seed)
        {
            _hashInt   = seed;
            _finalSeed = seed;

            _table = new uint[256];

            for(int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry >>= 1;

                _table[i] = entry;
            }
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
        public byte[] Final() => BigEndianBitConverter.GetBytes(_hashInt ^ _finalSeed);

        /// <inheritdoc />
        /// <summary>Returns a hexadecimal representation of the hash value.</summary>
        public string End()
        {
            var crc32Output = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(_hashInt ^ _finalSeed).Length; i++)
                crc32Output.Append(BigEndianBitConverter.GetBytes(_hashInt ^ _finalSeed)[i].ToString("x2"));

            return crc32Output.ToString();
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
        public static string File(string filename, out byte[] hash) =>
            File(filename, out hash, CRC32_ISO_POLY, CRC32_ISO_SEED);

        /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        /// <param name="polynomial">CRC polynomial</param>
        /// <param name="seed">CRC seed</param>
        public static string File(string filename, out byte[] hash, uint polynomial, uint seed)
        {
            var fileStream = new FileStream(filename, FileMode.Open);

            uint localHashInt = seed;

            uint[] localTable = new uint[256];

            for(int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry >>= 1;

                localTable[i] = entry;
            }

            for(int i = 0; i < fileStream.Length; i++)
                localHashInt = (localHashInt >> 8) ^ localTable[fileStream.ReadByte() ^ (localHashInt & 0xff)];

            localHashInt ^= seed;
            hash         =  BigEndianBitConverter.GetBytes(localHashInt);

            var crc32Output = new StringBuilder();

            foreach(byte h in hash)
                crc32Output.Append(h.ToString("x2"));

            fileStream.Close();

            return crc32Output.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash) =>
            Data(data, len, out hash, CRC32_ISO_POLY, CRC32_ISO_SEED);

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        /// <param name="polynomial">CRC polynomial</param>
        /// <param name="seed">CRC seed</param>
        public static string Data(byte[] data, uint len, out byte[] hash, uint polynomial, uint seed)
        {
            uint localHashInt = seed;

            uint[] localTable = new uint[256];

            for(int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry >>= 1;

                localTable[i] = entry;
            }

            for(int i = 0; i < len; i++)
                localHashInt = (localHashInt >> 8) ^ localTable[data[i] ^ (localHashInt & 0xff)];

            localHashInt ^= seed;
            hash         =  BigEndianBitConverter.GetBytes(localHashInt);

            var crc32Output = new StringBuilder();

            foreach(byte h in hash)
                crc32Output.Append(h.ToString("x2"));

            return crc32Output.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash) => Data(data, (uint)data.Length, out hash);
    }
}