// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Checksums
{
    /// <summary>Implements a CRC16 algorithm</summary>
    public class Crc16Context : IChecksum
    {
        readonly ushort   _finalSeed;
        readonly bool     _inverse;
        readonly ushort[] _table;
        ushort            _hashInt;

        /// <summary>Initializes the CRC16 table with a custom polynomial and seed</summary>
        public Crc16Context(ushort polynomial, ushort seed, ushort[] table, bool inverse)
        {
            _hashInt   = seed;
            _finalSeed = seed;
            _inverse   = inverse;

            _table = table ?? GenerateTable(polynomial, inverse);
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++)
            {
                if(_inverse)
                    _hashInt = (ushort)(_table[(_hashInt >> 8) ^ data[i]] ^ (_hashInt << 8));
                else
                    _hashInt = (ushort)((_hashInt >> 8) ^ _table[data[i] ^ (_hashInt & 0xFF)]);
            }
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data) => Update(data, (uint)data.Length);

        /// <inheritdoc />
        /// <summary>Returns a byte array of the hash value.</summary>
        public byte[] Final() => BigEndianBitConverter.GetBytes((ushort)(_hashInt ^ _finalSeed));

        /// <inheritdoc />
        /// <summary>Returns a hexadecimal representation of the hash value.</summary>
        public string End()
        {
            var crc16Output = new StringBuilder();

            ushort final = (ushort)(_hashInt ^ _finalSeed);

            if(_inverse)
                final = (ushort)~final;

            byte[] finalBytes = BigEndianBitConverter.GetBytes(final);

            for(int i = 0; i < finalBytes.Length; i++)
                crc16Output.Append(finalBytes[i].ToString("x2"));

            return crc16Output.ToString();
        }

        static ushort[] GenerateTable(ushort polynomial, bool inverseTable)
        {
            ushort[] table = new ushort[256];

            if(!inverseTable)
                for(uint i = 0; i < 256; i++)
                {
                    uint entry = i;

                    for(int j = 0; j < 8; j++)
                        if((entry & 1) == 1)
                            entry = (entry >> 1) ^ polynomial;
                        else
                            entry = entry >> 1;

                    table[i] = (ushort)entry;
                }
            else
            {
                for(uint i = 0; i < 256; i++)
                {
                    uint entry = i << 8;

                    for(uint j = 0; j < 8; j++)
                    {
                        if((entry & 0x8000) > 0)
                            entry = (entry << 1) ^ polynomial;
                        else
                            entry <<= 1;

                        table[i] = (ushort)entry;
                    }
                }
            }

            return table;
        }

        /// <summary>Gets the hash of a file in hexadecimal and as a byte array.</summary>
        /// <param name="filename">File path.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        /// <param name="polynomial">CRC polynomial</param>
        /// <param name="seed">CRC seed</param>
        /// <param name="table">CRC lookup table</param>
        /// <param name="inverse">Is CRC inverted?</param>
        public static string File(string filename, out byte[] hash, ushort polynomial, ushort seed, ushort[] table,
                                  bool inverse)
        {
            var fileStream = new FileStream(filename, FileMode.Open);

            ushort localHashInt = seed;

            ushort[] localTable = table ?? GenerateTable(polynomial, inverse);

            for(int i = 0; i < fileStream.Length; i++)
                if(inverse)
                    localHashInt =
                        (ushort)(localTable[(localHashInt >> 8) ^ fileStream.ReadByte()] ^ (localHashInt << 8));
                else
                    localHashInt =
                        (ushort)((localHashInt >> 8) ^ localTable[fileStream.ReadByte() ^ (localHashInt & 0xff)]);

            localHashInt ^= seed;

            if(inverse)
                localHashInt = (ushort)~localHashInt;

            hash = BigEndianBitConverter.GetBytes(localHashInt);

            var crc16Output = new StringBuilder();

            foreach(byte h in hash)
                crc16Output.Append(h.ToString("x2"));

            fileStream.Close();

            return crc16Output.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        /// <param name="polynomial">CRC polynomial</param>
        /// <param name="seed">CRC seed</param>
        /// <param name="table">CRC lookup table</param>
        /// <param name="inverse">Is CRC inverted?</param>
        public static string Data(byte[] data, uint len, out byte[] hash, ushort polynomial, ushort seed,
                                  ushort[] table, bool inverse)
        {
            ushort localHashInt = seed;

            ushort[] localTable = table ?? GenerateTable(polynomial, inverse);

            for(int i = 0; i < len; i++)
                if(inverse)
                    localHashInt = (ushort)(localTable[(localHashInt >> 8) ^ data[i]] ^ (localHashInt << 8));
                else
                    localHashInt = (ushort)((localHashInt >> 8) ^ localTable[data[i] ^ (localHashInt & 0xff)]);

            localHashInt ^= seed;

            if(inverse)
                localHashInt = (ushort)~localHashInt;

            hash = BigEndianBitConverter.GetBytes(localHashInt);

            var crc16Output = new StringBuilder();

            foreach(byte h in hash)
                crc16Output.Append(h.ToString("x2"));

            return crc16Output.ToString();
        }

        public static ushort Calculate(byte[] buffer, ushort polynomial, ushort seed, ushort[] table, bool inverse)
        {
            ushort[] localTable = table ?? GenerateTable(polynomial, inverse);

            ushort crc16 =
                buffer.Aggregate<byte, ushort>(0,
                                               (current, b) =>
                                                   inverse ? (ushort)(localTable[(current >> 8) ^ b] ^ (current << 8))
                                                       : (ushort)((current >> 8) ^ localTable[b ^ (current & 0xff)]));

            crc16 ^= seed;

            if(inverse)
                crc16 = (ushort)~crc16;

            return crc16;
        }
    }
}