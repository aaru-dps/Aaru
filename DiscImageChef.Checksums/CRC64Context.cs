// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Text;

namespace DiscImageChef.Checksums
{
    /// <summary>
    /// Provides a UNIX similar API to calculate CRC64 (ECMA).
    /// </summary>
    public class Crc64Context
    {
        const ulong CRC64_POLY = 0xC96C5795D7870F42;
        const ulong CRC64_SEED = 0xFFFFFFFFFFFFFFFF;

        ulong[] table;
        ulong hashInt;

        /// <summary>
        /// Initializes the CRC64 table and seed
        /// </summary>
        public void Init()
        {
            hashInt = CRC64_SEED;

            table = new ulong[256];
            for(int i = 0; i < 256; i++)
            {
                ulong entry = (ulong)i;
                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1) entry = (entry >> 1) ^ CRC64_POLY;
                    else entry = entry >> 1;

                table[i] = entry;
            }
        }

        /// <summary>
        /// Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++) hashInt = (hashInt >> 8) ^ table[data[i] ^ (hashInt & 0xff)];
        }

        /// <summary>
        /// Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data)
        {
            Update(data, (uint)data.Length);
        }

        /// <summary>
        /// Returns a byte array of the hash value.
        /// </summary>
        public byte[] Final()
        {
            hashInt ^= CRC64_SEED;
            BigEndianBitConverter.IsLittleEndian = BigEndianBitConverter.IsLittleEndian;
            return BigEndianBitConverter.GetBytes(hashInt);
        }

        /// <summary>
        /// Returns a hexadecimal representation of the hash value.
        /// </summary>
        public string End()
        {
            hashInt ^= CRC64_SEED;
            StringBuilder crc64Output = new StringBuilder();

            BigEndianBitConverter.IsLittleEndian = BigEndianBitConverter.IsLittleEndian;
            for(int i = 0; i < BigEndianBitConverter.GetBytes(hashInt).Length; i++)
            {
                crc64Output.Append(BigEndianBitConverter.GetBytes(hashInt)[i].ToString("x2"));
            }

            return crc64Output.ToString();
        }

        /// <summary>
        /// Gets the hash of a file
        /// </summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            byte[] localHash;
            File(filename, out localHash);
            return localHash;
        }

        /// <summary>
        /// Gets the hash of a file in hexadecimal and as a byte array.
        /// </summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            ulong[] localTable;
            ulong localhashInt;

            localhashInt = CRC64_SEED;

            localTable = new ulong[256];
            for(int i = 0; i < 256; i++)
            {
                ulong entry = (ulong)i;
                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1) entry = (entry >> 1) ^ CRC64_POLY;
                    else entry = entry >> 1;

                localTable[i] = entry;
            }

            for(int i = 0; i < fileStream.Length; i++)
                localhashInt = (localhashInt >> 8) ^ localTable[(ulong)fileStream.ReadByte() ^ (localhashInt & 0xffL)];

            localhashInt ^= CRC64_SEED;
            BigEndianBitConverter.IsLittleEndian = BigEndianBitConverter.IsLittleEndian;
            hash = BigEndianBitConverter.GetBytes(localhashInt);

            StringBuilder crc64Output = new StringBuilder();

            for(int i = 0; i < hash.Length; i++) { crc64Output.Append(hash[i].ToString("x2")); }

            fileStream.Close();

            return crc64Output.ToString();
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            return Data(data, len, out hash, CRC64_POLY, CRC64_SEED);
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        /// <param name="polynomial">CRC polynomial</param>
        /// <param name="seed">CRC seed</param>
        public static string Data(byte[] data, uint len, out byte[] hash, ulong polynomial, ulong seed)
        {
            ulong[] localTable;
            ulong localhashInt;

            localhashInt = seed;

            localTable = new ulong[256];
            for(int i = 0; i < 256; i++)
            {
                ulong entry = (ulong)i;
                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1) entry = (entry >> 1) ^ polynomial;
                    else entry = entry >> 1;

                localTable[i] = entry;
            }

            for(int i = 0; i < len; i++) localhashInt = (localhashInt >> 8) ^ localTable[data[i] ^ (localhashInt & 0xff)];

            localhashInt ^= CRC64_SEED;
            BigEndianBitConverter.IsLittleEndian = BigEndianBitConverter.IsLittleEndian;
            hash = BigEndianBitConverter.GetBytes(localhashInt);

            StringBuilder crc64Output = new StringBuilder();

            for(int i = 0; i < hash.Length; i++) { crc64Output.Append(hash[i].ToString("x2")); }

            return crc64Output.ToString();
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash)
        {
            return Data(data, (uint)data.Length, out hash);
        }
    }
}