/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : CRC64Context.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Checksums
Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Implements a CRC64 algorithm.
 
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
    /// Provides a UNIX similar API to calculate CRC64 (ECMA).
    /// </summary>
    public class CRC64Context
    {
        const UInt64 crc64Poly = 0xC96C5795D7870F42;
        const UInt64 crc64Seed = 0xFFFFFFFFFFFFFFFF;

        UInt64[] table;
        UInt64 hashInt;

        /// <summary>
        /// Initializes the CRC64 table and seed
        /// </summary>
        public void Init()
        {
            hashInt = crc64Seed;

            table = new UInt64[256];
            for (int i = 0; i < 256; i++)
            {
                UInt64 entry = (UInt64)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ crc64Poly;
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
            for (int i = 0; i < len; i++)
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
            hashInt ^= crc64Seed;
            return BigEndianBitConverter.GetBytes(hashInt);
        }

        /// <summary>
        /// Returns a hexadecimal representation of the hash value.
        /// </summary>
        public string End()
        {
            hashInt ^= crc64Seed;
            StringBuilder crc64Output = new StringBuilder();

            for (int i = 0; i < BigEndianBitConverter.GetBytes(hashInt).Length; i++)
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
            UInt64[] localTable;
            UInt64 localhashInt;

            localhashInt = crc64Seed;

            localTable = new UInt64[256];
            for (int i = 0; i < 256; i++)
            {
                UInt64 entry = (UInt64)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ crc64Poly;
                    else
                        entry = entry >> 1;
                localTable[i] = entry;
            }

            for (int i = 0; i < fileStream.Length; i++)
                localhashInt = (localhashInt >> 8) ^ localTable[(ulong)fileStream.ReadByte() ^ localhashInt & (ulong)0xff];

            hash = BitConverter.GetBytes(localhashInt);

            StringBuilder crc64Output = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                crc64Output.Append(hash[i].ToString("x2"));
            }

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
            UInt64[] localTable;
            UInt64 localhashInt;

            localhashInt = crc64Seed;

            localTable = new UInt64[256];
            for (int i = 0; i < 256; i++)
            {
                UInt64 entry = (UInt64)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ crc64Poly;
                    else
                        entry = entry >> 1;
                localTable[i] = entry;
            }

            for (int i = 0; i < len; i++)
                localhashInt = (localhashInt >> 8) ^ localTable[data[i] ^ localhashInt & 0xff];

            hash = BitConverter.GetBytes(localhashInt);

            StringBuilder crc64Output = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                crc64Output.Append(hash[i].ToString("x2"));
            }

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

