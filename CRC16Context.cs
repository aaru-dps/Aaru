// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CRC16Context.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements a CRC16 algorithm.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.Checksums
{
    /// <summary>
    ///     Implements a CRC16 algorithm
    /// </summary>
    public class Crc16Context : IChecksum
    {
        public const ushort CRC16_IBM_POLY   = 0xA001;
        public const ushort CRC16_IBM_SEED   = 0x0000;
        public const ushort CRC16_CCITT_POLY = 0x8408;
        public const ushort CRC16_CCITT_SEED = 0x0000;

        readonly ushort   finalSeed;
        readonly ushort[] table;
        ushort            hashInt;

        /// <summary>
        ///     Initializes the CRC16 table and seed as CRC16-IBM
        /// </summary>
        public Crc16Context()
        {
            hashInt   = CRC16_IBM_SEED;
            finalSeed = CRC16_IBM_SEED;

            table = new ushort[256];
            for(int i = 0; i < 256; i++)
            {
                ushort entry = (ushort)i;
                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (ushort)((entry >> 1) ^ CRC16_IBM_POLY);
                    else
                        entry = (ushort)(entry >> 1);

                table[i] = entry;
            }
        }

        /// <summary>
        ///     Initializes the CRC16 table with a custom polynomial and seed
        /// </summary>
        public Crc16Context(ushort polynomial, ushort seed)
        {
            hashInt   = seed;
            finalSeed = seed;

            table = new ushort[256];
            for(int i = 0; i < 256; i++)
            {
                ushort entry = (ushort)i;
                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (ushort)((entry >> 1) ^ polynomial);
                    else
                        entry = (ushort)(entry >> 1);

                table[i] = entry;
            }
        }

        /// <summary>
        ///     Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++) hashInt = (ushort)((hashInt >> 8) ^ table[data[i] ^ (hashInt & 0xFF)]);
        }

        /// <summary>
        ///     Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data)
        {
            Update(data, (uint)data.Length);
        }

        /// <summary>
        ///     Returns a byte array of the hash value.
        /// </summary>
        public byte[] Final()
        {
            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            return BigEndianBitConverter.GetBytes((ushort)(hashInt ^ finalSeed));
        }

        /// <summary>
        ///     Returns a hexadecimal representation of the hash value.
        /// </summary>
        public string End()
        {
            StringBuilder crc16Output = new StringBuilder();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;
            for(int i = 0; i < BigEndianBitConverter.GetBytes((ushort)(hashInt ^ finalSeed)).Length; i++)
                crc16Output.Append(BigEndianBitConverter.GetBytes((ushort)(hashInt ^ finalSeed))[i].ToString("x2"));

            return crc16Output.ToString();
        }

        /// <summary>
        ///     Gets the hash of a file
        /// </summary>
        /// <param name="filename">File path.</param>
        public static byte[] File(string filename)
        {
            File(filename, out byte[] hash);
            return hash;
        }

        /// <summary>
        ///     Gets the hash of a file in hexadecimal and as a byte array.
        /// </summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash)
        {
            return File(filename, out hash, CRC16_IBM_POLY, CRC16_IBM_SEED);
        }

        /// <summary>
        ///     Gets the hash of a file in hexadecimal and as a byte array.
        /// </summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string File(string filename, out byte[] hash, ushort polynomial, ushort seed)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open);

            ushort localhashInt = seed;

            ushort[] localTable = new ushort[256];
            for(int i = 0; i < 256; i++)
            {
                ushort entry = (ushort)i;
                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (ushort)((entry >> 1) ^ polynomial);
                    else
                        entry = (ushort)(entry >> 1);

                localTable[i] = entry;
            }

            for(int i = 0; i < fileStream.Length; i++)
                localhashInt =
                    (ushort)((localhashInt >> 8) ^ localTable[fileStream.ReadByte() ^ (localhashInt & 0xff)]);

            localhashInt                         ^= seed;
            BigEndianBitConverter.IsLittleEndian =  BitConverter.IsLittleEndian;
            hash                                 =  BigEndianBitConverter.GetBytes(localhashInt);

            StringBuilder crc16Output = new StringBuilder();

            foreach(byte h in hash) crc16Output.Append(h.ToString("x2"));

            fileStream.Close();

            return crc16Output.ToString();
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            return Data(data, len, out hash, CRC16_IBM_POLY, CRC16_IBM_SEED);
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        /// <param name="polynomial">CRC polynomial</param>
        /// <param name="seed">CRC seed</param>
        public static string Data(byte[] data, uint len, out byte[] hash, ushort polynomial, ushort seed)
        {
            ushort localhashInt = seed;

            ushort[] localTable = new ushort[256];
            for(int i = 0; i < 256; i++)
            {
                ushort entry = (ushort)i;
                for(int j = 0; j < 8; j++)
                    if((entry & 1) == 1)
                        entry = (ushort)((entry >> 1) ^ polynomial);
                    else
                        entry = (ushort)(entry >> 1);

                localTable[i] = entry;
            }

            for(int i = 0; i < len; i++)
                localhashInt = (ushort)((localhashInt >> 8) ^ localTable[data[i] ^ (localhashInt & 0xff)]);

            localhashInt                         ^= seed;
            BigEndianBitConverter.IsLittleEndian =  BitConverter.IsLittleEndian;
            hash                                 =  BigEndianBitConverter.GetBytes(localhashInt);

            StringBuilder crc16Output = new StringBuilder();

            foreach(byte h in hash) crc16Output.Append(h.ToString("x2"));

            return crc16Output.ToString();
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash)
        {
            return Data(data, (uint)data.Length, out hash);
        }
    }
}