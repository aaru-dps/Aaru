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

using System;
using System.IO;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Checksums
{
    /// <inheritdoc />
    /// <summary>Implements a CRC16 algorithm</summary>
    public class Crc16Context : IChecksum
    {
        readonly ushort     _finalSeed;
        readonly bool       _inverse;
        readonly ushort[][] _table;
        ushort              _hashInt;

        /// <summary>Initializes the CRC16 table with a custom polynomial and seed</summary>
        public Crc16Context(ushort polynomial, ushort seed, ushort[][] table, bool inverse)
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
            if(_inverse)
                StepInverse(ref _hashInt, _table, data, len);
            else
                Step(ref _hashInt, _table, data, len);
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        public void Update(byte[] data) => Update(data, (uint)data.Length);

        /// <inheritdoc />
        /// <summary>Returns a byte array of the hash value.</summary>
        public byte[] Final() => !_inverse ? BigEndianBitConverter.GetBytes((ushort)(_hashInt ^ _finalSeed))
                                     : BigEndianBitConverter.GetBytes((ushort)~(_hashInt ^ _finalSeed));

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

        static void Step(ref ushort previousCrc, ushort[][] table, byte[] data, uint len)
        {
            // Unroll according to Intel slicing by uint8_t
            // http://www.intel.com/technology/comms/perfnet/download/CRC_generators.pdf
            // http://sourceforge.net/projects/slicing-by-8/

            ushort    crc;
            int       current_pos   = 0;
            const int unroll        = 4;
            const int bytes_at_once = 8 * unroll;

            crc = previousCrc;

            while(len >= bytes_at_once)
            {
                int unrolling;

                for(unrolling = 0; unrolling < unroll; unrolling++)
                {
                    // TODO: What trick is Microsoft doing here that's faster than arithmetic conversion
                    uint one = BitConverter.ToUInt32(data, current_pos) ^ crc;
                    current_pos += 4;
                    uint two = BitConverter.ToUInt32(data, current_pos);
                    current_pos += 4;

                    crc = (ushort)(table[0][(two >> 24) & 0xFF] ^ table[1][(two >> 16) & 0xFF] ^
                                   table[2][(two >> 8)  & 0xFF] ^ table[3][two & 0xFF] ^ table[4][(one >> 24) & 0xFF] ^
                                   table[5][(one >> 16) & 0xFF] ^ table[6][(one >> 8) & 0xFF] ^ table[7][one & 0xFF]);
                }

                len -= bytes_at_once;
            }

            while(len-- != 0)
                crc = (ushort)((crc >> 8) ^ table[0][(crc & 0xFF) ^ data[current_pos++]]);

            previousCrc = crc;
        }

        static void StepInverse(ref ushort previousCrc, ushort[][] table, byte[] data, uint len)
        {
            // Unroll according to Intel slicing by uint8_t
            // http://www.intel.com/technology/comms/perfnet/download/CRC_generators.pdf
            // http://sourceforge.net/projects/slicing-by-8/

            ushort    crc;
            int       current_pos   = 0;
            const int unroll        = 4;
            const int bytes_at_once = 8 * unroll;

            crc = previousCrc;

            while(len >= bytes_at_once)
            {
                int unrolling;

                for(unrolling = 0; unrolling < unroll; unrolling++)
                {
                    crc = (ushort)(table[7][data[current_pos + 0] ^ (crc >> 8)]   ^
                                   table[6][data[current_pos + 1] ^ (crc & 0xFF)] ^ table[5][data[current_pos + 2]] ^
                                   table[4][data[current_pos + 3]] ^ table[3][data[current_pos + 4]] ^
                                   table[2][data[current_pos + 5]] ^ table[1][data[current_pos + 6]] ^
                                   table[0][data[current_pos + 7]]);

                    current_pos += 8;
                }

                len -= bytes_at_once;
            }

            while(len-- != 0)
                crc = (ushort)((crc << 8) ^ table[0][(crc >> 8) ^ data[current_pos++]]);

            previousCrc = crc;
        }

        static ushort[][] GenerateTable(ushort polynomial, bool inverseTable)
        {
            ushort[][] table = new ushort[8][];

            for(int i = 0; i < 8; i++)
                table[i] = new ushort[256];

            if(!inverseTable)
                for(uint i = 0; i < 256; i++)
                {
                    uint entry = i;

                    for(int j = 0; j < 8; j++)
                        if((entry & 1) == 1)
                            entry = (entry >> 1) ^ polynomial;
                        else
                            entry >>= 1;

                    table[0][i] = (ushort)entry;
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

                        table[0][i] = (ushort)entry;
                    }
                }
            }

            for(int slice = 1; slice < 8; slice++)
                for(int i = 0; i < 256; i++)
                {
                    if(inverseTable)
                        table[slice][i] = (ushort)((table[slice - 1][i] << 8) ^ table[0][table[slice - 1][i] >> 8]);
                    else
                        table[slice][i] = (ushort)((table[slice - 1][i] >> 8) ^ table[0][table[slice - 1][i] & 0xFF]);
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
        public static string File(string filename, out byte[] hash, ushort polynomial, ushort seed, ushort[][] table,
                                  bool inverse)
        {
            var fileStream = new FileStream(filename, FileMode.Open);

            ushort localHashInt = seed;

            ushort[][] localTable = table ?? GenerateTable(polynomial, inverse);

            byte[] buffer = new byte[65536];
            int    read   = fileStream.Read(buffer, 0, 65536);

            while(read > 0)
            {
                if(inverse)
                    StepInverse(ref localHashInt, localTable, buffer, (uint)read);
                else
                    Step(ref localHashInt, localTable, buffer, (uint)read);

                read = fileStream.Read(buffer, 0, 65536);
            }

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
                                  ushort[][] table, bool inverse)
        {
            ushort localHashInt = seed;

            ushort[][] localTable = table ?? GenerateTable(polynomial, inverse);

            if(inverse)
                StepInverse(ref localHashInt, localTable, data, len);
            else
                Step(ref localHashInt, localTable, data, len);

            localHashInt ^= seed;

            if(inverse)
                localHashInt = (ushort)~localHashInt;

            hash = BigEndianBitConverter.GetBytes(localHashInt);

            var crc16Output = new StringBuilder();

            foreach(byte h in hash)
                crc16Output.Append(h.ToString("x2"));

            return crc16Output.ToString();
        }

        /// <summary>Calculates the CRC16 of the specified buffer with the specified parameters</summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="polynomial">Polynomial</param>
        /// <param name="seed">Seed</param>
        /// <param name="table">Pre-generated lookup table</param>
        /// <param name="inverse">Inverse CRC</param>
        /// <returns>CRC16</returns>
        public static ushort Calculate(byte[] buffer, ushort polynomial, ushort seed, ushort[][] table, bool inverse)
        {
            ushort localHashInt = seed;

            ushort[][] localTable = table ?? GenerateTable(polynomial, inverse);

            if(inverse)
                StepInverse(ref localHashInt, localTable, buffer, (uint)buffer.Length);
            else
                Step(ref localHashInt, localTable, buffer, (uint)buffer.Length);

            localHashInt ^= seed;

            if(inverse)
                localHashInt = (ushort)~localHashInt;

            return localHashInt;
        }
    }
}