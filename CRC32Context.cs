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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Text;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Checksums
{
    /// <summary>Implements a CRC32 algorithm</summary>
    public class Crc32Context : IChecksum
    {
        const uint CRC32_ISO_POLY        = 0xEDB88320;
        const uint CRC32_ISO_SEED        = 0xFFFFFFFF;
        const uint CRC32_CASTAGNOLI_POLY = 0x8F6E37A0;
        const uint CRC32_CASTAGNOLI_SEED = 0xFFFFFFFF;

        readonly uint   finalSeed;
        readonly uint[] table;
        uint            hashInt;

        /// <summary>Initializes the CRC32 table and seed as CRC32-ISO</summary>
        public Crc32Context()
        {
            hashInt   = CRC32_ISO_SEED;
            finalSeed = CRC32_ISO_SEED;

            table = new uint[256];

            for(int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ CRC32_ISO_POLY;
                    else
                        entry = entry >> 1;

                table[i] = entry;
            }
        }

        /// <summary>Initializes the CRC32 table with a custom polynomial and seed</summary>
        public Crc32Context(uint polynomial, uint seed)
        {
            hashInt   = seed;
            finalSeed = seed;

            table = new uint[256];

            for(int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;

                table[i] = entry;
            }
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++)
                hashInt = (hashInt >> 8) ^ table[data[i] ^ (hashInt & 0xff)];
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data) => Update(data, (uint)data.Length);

        /// <inheritdoc />
        /// <summary>Returns a byte array of the hash value.</summary>
        public byte[] Final() => BigEndianBitConverter.GetBytes(hashInt ^ finalSeed);

        /// <inheritdoc />
        /// <summary>Returns a hexadecimal representation of the hash value.</summary>
        public string End()
        {
            var crc32Output = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(hashInt ^ finalSeed).Length; i++)
                crc32Output.Append(BigEndianBitConverter.GetBytes(hashInt ^ finalSeed)[i].ToString("x2"));

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
        public static string File(string filename, out byte[] hash, uint polynomial, uint seed)
        {
            var fileStream = new FileStream(filename, FileMode.Open);

            uint localhashInt = seed;

            uint[] localTable = new uint[256];

            for(int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;

                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;

                localTable[i] = entry;
            }

            for(int i = 0; i < fileStream.Length; i++)
                localhashInt = (localhashInt >> 8) ^ localTable[fileStream.ReadByte() ^ (localhashInt & 0xff)];

            localhashInt ^= seed;
            hash         =  BigEndianBitConverter.GetBytes(localhashInt);

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
            uint localhashInt = seed;

            uint[] localTable = new uint[256];

            for(int i = 0; i < 256; i++)
            {
                uint entry = (uint)i;

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