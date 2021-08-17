// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FletcherContext.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Checksums.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements Fletcher-32 and Fletcher-16 algorithms.
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

// Disabled because the speed is abnormally slow

using System.IO;
using System.Text;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;

namespace Aaru.Checksums
{
    /// <summary>Implements the Fletcher-32 algorithm</summary>
    public sealed class Fletcher32Context : IChecksum
    {
        const ushort FLETCHER_MODULE = 0xFFFF;
        ushort       _sum1, _sum2;

        /// <summary>Initializes the Fletcher-32 sums</summary>
        public Fletcher32Context()
        {
            _sum1 = 0xFFFF;
            _sum2 = 0xFFFF;
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++)
            {
                _sum1 = (ushort)((_sum1 + data[i]) % FLETCHER_MODULE);
                _sum2 = (ushort)((_sum2 + _sum1)   % FLETCHER_MODULE);
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
            uint finalSum       = (uint)((_sum2 << 16) | _sum1);
            var  fletcherOutput = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
                fletcherOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

            return fletcherOutput.ToString();
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

            ushort localSum1 = 0xFFFF;
            ushort localSum2 = 0xFFFF;

            for(int i = 0; i < fileStream.Length; i++)
            {
                localSum1 = (ushort)((localSum1 + fileStream.ReadByte()) % FLETCHER_MODULE);
                localSum2 = (ushort)((localSum2 + localSum1)             % FLETCHER_MODULE);
            }

            uint finalSum = (uint)((localSum2 << 16) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            var fletcherOutput = new StringBuilder();

            foreach(byte h in hash)
                fletcherOutput.Append(h.ToString("x2"));

            fileStream.Close();

            return fletcherOutput.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            ushort localSum1 = 0xFFFF;
            ushort localSum2 = 0xFFFF;

            for(int i = 0; i < len; i++)
            {
                localSum1 = (ushort)((localSum1 + data[i])   % FLETCHER_MODULE);
                localSum2 = (ushort)((localSum2 + localSum1) % FLETCHER_MODULE);
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

    /// <inheritdoc />
    /// <summary>Implements the Fletcher-16 algorithm</summary>
    public sealed class Fletcher16Context : IChecksum
    {
        const byte FLETCHER_MODULE = 0xFF;
        byte       _sum1, _sum2;

        /// <summary>Initializes the Fletcher-16 sums</summary>
        public Fletcher16Context()
        {
            _sum1 = 0xFF;
            _sum2 = 0xFF;
        }

        /// <inheritdoc />
        /// <summary>Updates the hash with data.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++)
            {
                _sum1 = (byte)((_sum1 + data[i]) % FLETCHER_MODULE);
                _sum2 = (byte)((_sum2 + _sum1)   % FLETCHER_MODULE);
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
            ushort finalSum = (ushort)((_sum2 << 8) | _sum1);

            return BigEndianBitConverter.GetBytes(finalSum);
        }

        /// <inheritdoc />
        /// <summary>Returns a hexadecimal representation of the hash value.</summary>
        public string End()
        {
            ushort finalSum       = (ushort)((_sum2 << 8) | _sum1);
            var    fletcherOutput = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
                fletcherOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

            return fletcherOutput.ToString();
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

            byte localSum1 = 0xFF;
            byte localSum2 = 0xFF;

            for(int i = 0; i < fileStream.Length; i++)
            {
                localSum1 = (byte)((localSum1 + fileStream.ReadByte()) % FLETCHER_MODULE);
                localSum2 = (byte)((localSum2 + localSum1)             % FLETCHER_MODULE);
            }

            ushort finalSum = (ushort)((localSum2 << 8) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            var fletcherOutput = new StringBuilder();

            foreach(byte h in hash)
                fletcherOutput.Append(h.ToString("x2"));

            fileStream.Close();

            return fletcherOutput.ToString();
        }

        /// <summary>Gets the hash of the specified data buffer.</summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of the data buffer to hash.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, uint len, out byte[] hash)
        {
            byte localSum1 = 0xFF;
            byte localSum2 = 0xFF;

            for(int i = 0; i < len; i++)
            {
                localSum1 = (byte)((localSum1 + data[i])   % FLETCHER_MODULE);
                localSum2 = (byte)((localSum2 + localSum1) % FLETCHER_MODULE);
            }

            ushort finalSum = (ushort)((localSum2 << 8) | localSum1);

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