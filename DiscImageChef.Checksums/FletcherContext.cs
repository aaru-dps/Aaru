// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

// Disabled because the speed is abnormally slow

using System.IO;
using System.Text;
using DiscImageChef.CommonTypes.Interfaces;

namespace DiscImageChef.Checksums
{
    /// <summary>
    ///     Implements the Fletcher-32 algorithm
    /// </summary>
    public class Fletcher32Context : IChecksum
    {
        const ushort FLETCHER_MODULE = 0xFFFF;
        ushort       sum1, sum2;

        /// <summary>
        ///     Initializes the Fletcher-32 sums
        /// </summary>
        public Fletcher32Context()
        {
            sum1 = 0xFFFF;
            sum2 = 0xFFFF;
        }

        /// <summary>
        ///     Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++)
            {
                sum1 = (ushort)((sum1 + data[i]) % FLETCHER_MODULE);
                sum2 = (ushort)((sum2 + sum1)    % FLETCHER_MODULE);
            }
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
            uint finalSum = (uint)((sum2 << 16) | sum1);
            return BigEndianBitConverter.GetBytes(finalSum);
        }

        /// <summary>
        ///     Returns a hexadecimal representation of the hash value.
        /// </summary>
        public string End()
        {
            uint          finalSum       = (uint)((sum2 << 16) | sum1);
            StringBuilder fletcherOutput = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
                fletcherOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

            return fletcherOutput.ToString();
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
            FileStream fileStream = new FileStream(filename, FileMode.Open);

            ushort localSum1 = 0xFFFF;
            ushort localSum2 = 0xFFFF;

            for(int i = 0; i < fileStream.Length; i++)
            {
                localSum1 = (ushort)((localSum1 + fileStream.ReadByte()) % FLETCHER_MODULE);
                localSum2 = (ushort)((localSum2 + localSum1)             % FLETCHER_MODULE);
            }

            uint finalSum = (uint)((localSum2 << 16) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            StringBuilder fletcherOutput = new StringBuilder();

            foreach(byte h in hash) fletcherOutput.Append(h.ToString("x2"));

            fileStream.Close();

            return fletcherOutput.ToString();
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
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

            StringBuilder adlerOutput = new StringBuilder();

            foreach(byte h in hash) adlerOutput.Append(h.ToString("x2"));

            return adlerOutput.ToString();
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash) => Data(data, (uint)data.Length, out hash);
    }

    /// <summary>
    ///     Implements the Fletcher-16 algorithm
    /// </summary>
    public class Fletcher16Context : IChecksum
    {
        const byte FLETCHER_MODULE = 0xFF;
        byte       sum1, sum2;

        /// <summary>
        ///     Initializes the Fletcher-16 sums
        /// </summary>
        public Fletcher16Context()
        {
            sum1 = 0xFF;
            sum2 = 0xFF;
        }

        /// <summary>
        ///     Updates the hash with data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="len">Length of buffer to hash.</param>
        public void Update(byte[] data, uint len)
        {
            for(int i = 0; i < len; i++)
            {
                sum1 = (byte)((sum1 + data[i]) % FLETCHER_MODULE);
                sum2 = (byte)((sum2 + sum1)    % FLETCHER_MODULE);
            }
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
            ushort finalSum = (ushort)((sum2 << 8) | sum1);
            return BigEndianBitConverter.GetBytes(finalSum);
        }

        /// <summary>
        ///     Returns a hexadecimal representation of the hash value.
        /// </summary>
        public string End()
        {
            ushort        finalSum       = (ushort)((sum2 << 8) | sum1);
            StringBuilder fletcherOutput = new StringBuilder();

            for(int i = 0; i < BigEndianBitConverter.GetBytes(finalSum).Length; i++)
                fletcherOutput.Append(BigEndianBitConverter.GetBytes(finalSum)[i].ToString("x2"));

            return fletcherOutput.ToString();
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
            FileStream fileStream = new FileStream(filename, FileMode.Open);

            byte localSum1 = 0xFF;
            byte localSum2 = 0xFF;

            for(int i = 0; i < fileStream.Length; i++)
            {
                localSum1 = (byte)((localSum1 + fileStream.ReadByte()) % FLETCHER_MODULE);
                localSum2 = (byte)((localSum2 + localSum1)             % FLETCHER_MODULE);
            }

            ushort finalSum = (ushort)((localSum2 << 8) | localSum1);

            hash = BigEndianBitConverter.GetBytes(finalSum);

            StringBuilder fletcherOutput = new StringBuilder();

            foreach(byte h in hash) fletcherOutput.Append(h.ToString("x2"));

            fileStream.Close();

            return fletcherOutput.ToString();
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
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

            StringBuilder adlerOutput = new StringBuilder();

            foreach(byte h in hash) adlerOutput.Append(h.ToString("x2"));

            return adlerOutput.ToString();
        }

        /// <summary>
        ///     Gets the hash of the specified data buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="hash">Byte array of the hash value.</param>
        public static string Data(byte[] data, out byte[] hash) => Data(data, (uint)data.Length, out hash);
    }
}