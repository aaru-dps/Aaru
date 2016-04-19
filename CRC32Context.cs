/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : CRC32Context.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Checksums.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements a CRC32 algorithm.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System.Text;
using System.IO;
using System;

namespace DiscImageChef.Checksums
{
    /// <summary>
    /// Provides a UNIX similar API to calculate CRC32.
    /// </summary>
    public class CRC32Context
    {
        const UInt32 crc32Poly = 0xEDB88320;
        const UInt32 crc32Seed = 0xFFFFFFFF;

        UInt32[] table;
        UInt32 hashInt;

        /// <summary>
        /// Initializes the CRC32 table and seed
        /// </summary>
        public void Init()
        {
            hashInt = crc32Seed;

            table = new UInt32[256];
            for(int i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ crc32Poly;
                    else
                        entry = entry >> 1;
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
            for(int i = 0; i < len; i++)
                hashInt = (hashInt >> 8) ^ table[data[i] ^ hashInt & 0xff];
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
            hashInt ^= crc32Seed;
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            return BigEndianBitConverter.GetBytes(hashInt);
        }

        /// <summary>
        /// Returns a hexadecimal representation of the hash value.
        /// </summary>
        public string End()
        {
            hashInt ^= crc32Seed;
            StringBuilder crc32Output = new StringBuilder();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            for(int i = 0; i < BigEndianBitConverter.GetBytes(hashInt).Length; i++)
            {
                crc32Output.Append(BigEndianBitConverter.GetBytes(hashInt)[i].ToString("x2"));
            }

            return crc32Output.ToString();
        }

        /// <summary>
        /// Gets the hash of a file
        /// </summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            byte[] hash;
            File(filename, out hash);
            return hash;
        }

        /// <summary>
        /// Gets the hash of a file in hexadecimal and as a byte array.
        /// </summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);
            UInt32[] localTable;
            UInt32 localhashInt;

            localhashInt = crc32Seed;

            localTable = new UInt32[256];
            for(int i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ crc32Poly;
                    else
                        entry = entry >> 1;
                localTable[i] = entry;
            }

            for(int i = 0; i < fileStream.Length; i++)
                localhashInt = (localhashInt >> 8) ^ localTable[fileStream.ReadByte() ^ localhashInt & 0xff];

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            hash = BitConverter.GetBytes(localhashInt);

            StringBuilder crc32Output = new StringBuilder();

            for(int i = 0; i < hash.Length; i++)
            {
                crc32Output.Append(hash[i].ToString("x2"));
            }

            return crc32Output.ToString();
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            return Data(data, len, out hash, crc32Poly, crc32Seed);
        }

        /// <summary>
        /// Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        /// <param name="polynomial">CRC polynomial</param>
        /// <param name="seed">CRC seed</param>
        public static string Data(byte[] data, uint len, out byte[] hash, UInt32 polynomial, UInt32 seed)
        {
            UInt32[] localTable;
            UInt32 localhashInt;

            localhashInt = seed;

            localTable = new UInt32[256];
            for(int i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                localTable[i] = entry;
            }

            for(int i = 0; i < len; i++)
                localhashInt = (localhashInt >> 8) ^ localTable[data[i] ^ localhashInt & 0xff];

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            hash = BitConverter.GetBytes(localhashInt);

            StringBuilder crc32Output = new StringBuilder();

            for(int i = 0; i < hash.Length; i++)
            {
                crc32Output.Append(hash[i].ToString("x2"));
            }

            return crc32Output.ToString();
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


